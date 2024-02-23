using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Polly;
using Tgs.VipLevels.Level.Core;
using Tgs.VipLevels.Level.Core.Extensions;
using Tgs.VipLevels.Level.Core.Models;
using Tgs.VipLevels.Level.Repositories.Models;

namespace Tgs.VipLevels.Level.Repositories;

internal class PlayerVipLevelBacklogRepository(
	ILogger<PlayerVipLevelBacklogRepository> logger,
	IMongoDatabase mongoDatabase) : IPlayerVipLevelBacklogRepository
{
	public async ValueTask AddAsync(string playerCode, DateTimeOffset triggerAt, CancellationToken cancellationToken = default)
	{
		var collection = await GetCollectionAsync(cancellationToken).ConfigureAwait(false);

		var filter = Builders<PlayerVipLevelBacklog>.Filter.Eq(x => x.PlayerCode, playerCode)
			& Builders<PlayerVipLevelBacklog>.Filter.Eq(x => x.TriggerAt, triggerAt.UtcDateTime);

		var update = Builders<PlayerVipLevelBacklog>.Update
			.Set(s => s.PlayerCode, playerCode)
			.Set(s => s.TriggerAt, triggerAt.UtcDateTime)
			.Set(s => s.ExpireAt, DateTime.UtcNow.AddDays(1))
			.Inc(s => s.Count, 1);

		var options = new UpdateOptions
		{
			IsUpsert = true
		};

		var retryPolicy = Policy
		   .Handle<MongoBulkWriteException>()
		   .WaitAndRetryAsync(
			retryCount: 3,
			retryAttempt => TimeSpan.FromSeconds(5),
			(result, retryTime, context) => logger.LogWarning("LogOn:{logOn}  PlayerCode:{playerCode} TriggerAt:{triggerAt} Result:{result} Delay:{retryTime}",
				DateTime.Now.ToString("o"),
				playerCode,
				triggerAt,
				result,
				retryTime));

		_ = await retryPolicy.ExecuteAsync(
			() => collection.UpdateOneAsync(
				filter: filter,
				update: update,
				options: options,
				cancellationToken: cancellationToken))
			.ConfigureAwait(false);
	}

	public async IAsyncEnumerable<PlayerVipLevelBacklogInfo> QueryAsync(DateTimeOffset endOn, int offset = 0, int limit = 20, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var collection = await GetCollectionAsync(cancellationToken).ConfigureAwait(false);

		var filter = Builders<PlayerVipLevelBacklog>.Filter.Lt(x => x.TriggerAt, endOn.UtcDateTime);

		var sort = Builders<PlayerVipLevelBacklog>
					.Sort
					.Ascending(x => x.Id);

		var options = new FindOptions<PlayerVipLevelBacklog>
		{
			Sort = sort,
			Skip = offset,
			Limit = limit,
		};

		var asyncCursor = await collection.FindAsync(
			filter: filter,
			options: options,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		while (await asyncCursor
					.MoveNextAsync(
					cancellationToken: cancellationToken)
					.ConfigureAwait(false))
		{
			foreach (var current in asyncCursor.Current)
			{
				yield return new PlayerVipLevelBacklogInfo(
				PlayerCode: current.PlayerCode,
				TriggerAt: current.TriggerAt);
			}
		}
	}

	public async ValueTask UpdateExpireOnAsync(string playerCode, DateTimeOffset triggerAt, CancellationToken cancellationToken = default)
	{
		var collection = await GetCollectionAsync(cancellationToken).ConfigureAwait(false);

		var filter = Builders<PlayerVipLevelBacklog>.Filter.Eq(x => x.PlayerCode, playerCode)
			& Builders<PlayerVipLevelBacklog>.Filter.Eq(x => x.TriggerAt, triggerAt.UtcDateTime);

		var update = Builders<PlayerVipLevelBacklog>.Update
			.Set(s => s.PlayerCode, playerCode)
			.Set(s => s.TriggerAt, triggerAt.UtcDateTime)
			.Set(s => s.ExpireAt, DateTime.UtcNow.AddSeconds(1))
			.Inc(s => s.Count, 1);

		var options = new UpdateOptions
		{
			IsUpsert = true
		};

		var retryPolicy = Policy
		   .Handle<MongoBulkWriteException>()
		   .WaitAndRetryAsync(
			retryCount: 3,
			retryAttempt => TimeSpan.FromSeconds(5),
			(result, retryTime, context) => logger.LogWarning("LogOn:{logOn}  PlayerCode:{playerCode} TriggerAt:{triggerAt} Result:{result} Delay:{retryTime}",
				DateTime.Now.ToString("o"),
				playerCode,
				triggerAt,
				result,
				retryTime));

		_ = await retryPolicy.ExecuteAsync(
			() => collection.UpdateOneAsync(
				filter: filter,
				update: update,
				options: options,
				cancellationToken: cancellationToken))
			.ConfigureAwait(false);
	}

	private static Task PlayerVipLevelBacklog_IndexAsync(
		IMongoCollection<PlayerVipLevelBacklog> collection,
		CancellationToken cancellationToken = default)
	{
		var index = Builders<PlayerVipLevelBacklog>.IndexKeys
			.Ascending(m => m.PlayerCode);

		var createIndexOptions = new CreateIndexOptions
		{
			Name = "px_player_vip_level_backlogs"
		};
		var createIndexModel = new CreateIndexModel<PlayerVipLevelBacklog>(index, createIndexOptions);

		return _ = collection.Indexes.CreateOneAsync(
			model: createIndexModel,
			cancellationToken: cancellationToken);
	}

	private static Task ExpireOn_IndexAsync(
		IMongoCollection<PlayerVipLevelBacklog> collection,
		CancellationToken cancellationToken = default)
	{
		var index = Builders<PlayerVipLevelBacklog>.IndexKeys
			.Ascending(m => m.ExpireAt);

		var createIndexOptions = new CreateIndexOptions
		{
			ExpireAfter = TimeSpan.FromSeconds(1),
			Name = "idx_expire_at"
		};
		var createIndexModel = new CreateIndexModel<PlayerVipLevelBacklog>(index, createIndexOptions);

		return _ = collection.Indexes.CreateOneAsync(
			model: createIndexModel,
			cancellationToken: cancellationToken);
	}

	private async ValueTask<IMongoCollection<PlayerVipLevelBacklog>> GetCollectionAsync(CancellationToken cancellationToken = default)
	{
		var collection = mongoDatabase.GetCollection<PlayerVipLevelBacklog>(typeof(PlayerVipLevelBacklog).GetTableName());

		if (mongoDatabase.ListCollectionNames(
			options: new ListCollectionNamesOptions
			{
				Filter = new BsonDocument("name", typeof(PlayerVipLevelBacklog).GetTableName())
			},
			cancellationToken: cancellationToken).Any(cancellationToken: cancellationToken))
		{
			return collection;
		}

		await PlayerVipLevelBacklog_IndexAsync(
			collection: collection,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		await ExpireOn_IndexAsync(
			collection: collection,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		return collection;
	}
}
