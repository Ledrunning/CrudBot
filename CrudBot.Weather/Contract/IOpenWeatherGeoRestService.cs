using CrudBot.Weather.Model.GeoApiModel;

namespace CrudBot.Weather.Contract;

public interface IOpenWeatherGeoRestService
{
    public Task<List<GeoData>> GetCityCoordinates(string city, CancellationToken token);
}