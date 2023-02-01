using CrudBot.Weather.Contract;
using CrudBot.Weather.Model.GeoApiModel;
using RestSharp;

namespace CrudBot.Weather.Service;

public class OpenWeatherRestGeoService : BaseService, IOpenWeatherGeoRestService
{
    private readonly string _apiKey;
    private readonly int _cityLimit;

    public OpenWeatherRestGeoService(string apiKey, string baseUrl, int cityLimit, int timeout) : base(baseUrl, timeout)
    {
        _apiKey = apiKey;
        _cityLimit = cityLimit;
    }

    //http://api.openweathermap.org/geo/1.0/direct?q=London&limit=5&appid={API key}
    //May be country two letter code 
    public async Task<List<GeoData>> GetCityCoordinates(string city, CancellationToken token)
    {
        var url = new Uri($"{BaseUrl}{city}&limit={_cityLimit}&appid={_apiKey}");
        var client = new RestClient(SetOptions(url));
        var request = new RestRequest();
        var response = await client.ExecuteAsync(request, token);

        return GetContent<List<GeoData>>(response, url.AbsoluteUri);
    }
}