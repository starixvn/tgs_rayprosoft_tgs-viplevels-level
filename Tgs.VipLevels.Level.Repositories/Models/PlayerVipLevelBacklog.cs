using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Tgs.VipLevels.Level.Repositories.Models;
[Table("player_vip_level_backlogs")]
public record PlayerVipLevelBacklog
{
	/// <summary>
	/// The identifier
	/// </summary>
	[BsonId]
	public ObjectId Id { get; set; } = default!;

	/// <summary>
	/// The player code
	/// </summary>
	[BsonElement("player_code")]
	[BsonRepresentation(BsonType.String)]
	public string PlayerCode { get; set; } = default!;
	/// <summary>
	/// The count
	/// </summary>
	[BsonElement("count")]
	[BsonRepresentation(BsonType.Int32)]
	[BsonDefaultValue(0)]
	public int Count { get; set; }
	/// <summary>
	/// The updated on
	/// </summary>
	[BsonElement("trigger_at")]
	public DateTime TriggerAt { get; set; }
	/// <summary>
	/// The expire on
	/// </summary>
	[BsonElement("expire_at")]
	public DateTime ExpireAt { get; set; }
}
