using CrudBot.Exceptions.Exceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;

namespace CrudBot.Weather.Service;

public class BaseService
{
    protected readonly string BaseUrl;
    private readonly ILogger _logger;
    private readonly int _timeout;

    public BaseService(ILogger logger, string baseUrl, int timeout)
    {
        _logger = logger;
        BaseUrl = baseUrl;
        _timeout = timeout;
    }

    protected T GetContent<T>(RestResponseBase response, string url)
    {
        if (response.IsSuccessful)
        {
            if (response.Content != null)
            {
                var model = JsonConvert.DeserializeObject<T>(response.Content);
                _logger.LogInformation("Request to OpenWeather successfully finished {Url}", url);
                if (model != null)
                {
                    return model;
                }

                _logger.LogInformation("Requested data from OpenWeather is null {Url}", url);
            }
        }

        throw new CrudBotException(
            $"Response from OpenWeather failed. Status code: {response.StatusCode}, {response.ErrorMessage}");
    }

    protected RestClientOptions SetOptions(Uri url)
    {
        return new RestClientOptions(url)
        {
            ThrowOnAnyError = true,
            MaxTimeout = _timeout
        };
    }
}