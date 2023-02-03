namespace CrudBot.Main.Configuration;

public class OpenWeatherApi
{
    public static readonly string Configuration = "OpenWeatherApi";

    public string BaseUrl { get; set; } = "";
    public string BaseGeoUrl { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public int TimeOut { get; set; }
    public int GeoCityLimit { get; set; }
}