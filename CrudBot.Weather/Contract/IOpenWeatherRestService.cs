using CrudBot.Weather.Model;

namespace CrudBot.Weather.Contract;

public interface IOpenWeatherRestService
{
    Task<MainWeather> GetWeatherFromOpenWeatherApi(string city, CancellationToken token);
}