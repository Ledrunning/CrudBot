using CrudBot.Weather.Contract;
using CrudBot.Weather.Model;
using RestSharp;

namespace CrudBot.Weather.Service;

public class OpenWeatherRestService : BaseService, IOpenWeatherRestService
{
    private readonly IOpenWeatherGeoRestService _geoRestService;
    private readonly string _apiKey;

    public OpenWeatherRestService(IOpenWeatherGeoRestService geoRestService, string apiKey, string baseUrl, int timeout) : base(baseUrl, timeout)
    {
        _geoRestService = geoRestService;
        _apiKey = apiKey;
    }

    //URL - https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={API key}
    public async Task<MainWeather> GetWeatherFromOpenWeatherApi(string city, CancellationToken token)
    {
        var cityInfoList = await _geoRestService.GetCityCoordinates(city, token);
        var cityInfo = cityInfoList.Select(data => data).FirstOrDefault();

        var url = new Uri($"{BaseUrl}lat={cityInfo?.Lat}&lon={cityInfo?.Lon}&appid={_apiKey}");
        var client = new RestClient(SetOptions(url));
        var request = new RestRequest();
        var response = await client.ExecuteAsync(request, token);

        return GetContent<MainWeather>(response, url.AbsoluteUri);
    }
}