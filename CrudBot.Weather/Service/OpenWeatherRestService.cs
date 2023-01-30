using CrudBot.Weather.Model;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace CrudBot.Weather.Service;

public class OpenWeatherRestService : BaseService
{
    private readonly string _apiKey;

    public OpenWeatherRestService(string apiKey, ILogger logger, string baseUrl, int timeout) : base(logger, baseUrl,
        timeout)
    {
        _apiKey = apiKey;
    }

    //URL - https://api.openweathermap.org/data/2.5/weather?q={city name}&appid={API key}
    public async Task<MainWeather> GetWeatherFromOpenWeatherApi(string city, CancellationToken token)
    {
        var url = new Uri($"{BaseUrl}/weather?q={city}&appid={_apiKey}");
        var client = new RestClient(SetOptions(url));
        var request = new RestRequest();
        var response = await client.ExecuteAsync(request, token);

        return GetContent<MainWeather>(response, url.AbsoluteUri);
    }
}