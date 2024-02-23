using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Tgs.VipLevels.Level;

internal class MongoDBConnectionManager
{
	public IMongoClient MongoClient { get; }

	public IMongoDatabase MongoDatabase { get; }

	public MongoDBConnectionManager(
		IOptions<MongoDBOptions> optionsAccessor)
	{
		var options = optionsAccessor.Value;
		var mongoUrl = new MongoUrl(options.ConnectionString);
		MongoClient = new MongoClient(mongoUrl);
		MongoDatabase = MongoClient.GetDatabase("vip_level_master");
	}
}
