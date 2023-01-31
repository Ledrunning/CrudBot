using CrudBot.Weather.Contract;
using CrudBot.Weather.Model;
using RestSharp;

namespace CrudBot.Weather.Service;

public class OpenWeatherGeoService : BaseService
{
    private readonly string _apiKey;

    public OpenWeatherGeoService(string apiKey, string baseUrl, int timeout) : base(baseUrl, timeout)
    {
        _apiKey = apiKey;
    }
    
    //http://api.openweathermap.org/geo/1.0/direct?q=London&limit=5&appid={API key}
    public async Task GetCityCoordinates(string city, int limit, CancellationToken token)
    {
        var url = new Uri($"{BaseUrl}/direct?q={city}&limit={limit}&appid={_apiKey}");
        var client = new RestClient(SetOptions(url));
        var request = new RestRequest();
        var response = await client.ExecuteAsync(request, token);

        return GetContent<MainWeather>(response, url.AbsoluteUri);
    }
}