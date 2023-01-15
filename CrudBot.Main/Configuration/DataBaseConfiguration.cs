namespace CrudBot.Main.Configuration;

internal class DataBaseConfiguration
{
    public static readonly string Configuration = "ConnectionString";

    public string ConnectionString { get; set; } = "";
}