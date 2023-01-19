namespace CrudBot.Main.Configuration;

internal class DataBaseConfiguration
{
    public static readonly string Configuration = "DatabaseConfiguration";

    public string ConnectionString { get; set; } = "";
}