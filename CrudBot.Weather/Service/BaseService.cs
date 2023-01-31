using CrudBot.Exceptions.Exceptions;
using Newtonsoft.Json;
using RestSharp;

namespace CrudBot.Weather.Service;

public class BaseService
{
    protected readonly string BaseUrl;
    private readonly int _timeout;

    public BaseService(string baseUrl, int timeout)
    {
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
                if (model != null)
                {
                    return model;
                }
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