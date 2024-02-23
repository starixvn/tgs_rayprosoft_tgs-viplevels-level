namespace Tgs.VipLevels.Level;

public record MongoDBOptions
{
	public string ConnectionString { get; set; } = "mongodb://localhost";
}
