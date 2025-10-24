using System;
using System.Net.Http;
using System.Threading.Tasks;
using Refit;

// Wspólny interfejs ułatwiający testowanie różnych klientów
public interface IApiClient
{
    Task<string> GetItemAsync(int id);
}

// Przykładowy klient Refit (zakładamy, że ten interfejs jest wygenerowany lub zdefiniowany)
public interface IRefitApi
{
    [Get("/items/{id}")]
    Task<string> GetItemAsync(int id);
}

// Adapter/Wrapper dla klienta Refit implementujący IApiClient
public class RefitApiClient : IApiClient
{
    private readonly IRefitApi _refitApi;

    public RefitApiClient(HttpClient httpClient)
    {
        _refitApi = RestService.For<IRefitApi>(httpClient);
    }

    public Task<string> GetItemAsync(int id)
    {
        return _refitApi.GetItemAsync(id);
    }
}

// Przykładowy klient OpenAPI Generator (załóżmy, że to wygenerowana klasa)
public class OpenApiGeneratedClient
{
    private readonly HttpClient _httpClient;

    public OpenApiGeneratedClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetItemAsync(int id)
    {
        var response = await _httpClient.GetAsync($"/items/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}

// Adapter dla klienta OpenAPI Generator (implementacja IApiClient)
public class OpenApiClientAdapter : IApiClient
{
    private readonly OpenApiGeneratedClient _client;

    public OpenApiClientAdapter(HttpClient httpClient)
    {
        _client = new OpenApiGeneratedClient(httpClient);
    }

    public Task<string> GetItemAsync(int id)
    {
        return _client.GetItemAsync(id);
    }
}

// Przykładowa konsolowa aplikacja testująca
class Program
{
    static async Task Main(string[] args)
    {
        var baseAddress = "https://localhost:5001"; // Adres Twojego API

        // Konfiguracja HttpClientów z tym samym base URL (można rozszerzyć o delegaty itp)
        var refitHttpClient = new HttpClient { BaseAddress = new Uri(baseAddress) };
        var openApiHttpClient = new HttpClient { BaseAddress = new Uri(baseAddress) };

        // Inicjalizacja klientów
        IApiClient refitClient = new RefitApiClient(refitHttpClient);
        IApiClient openApiClient = new OpenApiClientAdapter(openApiHttpClient);

        // Testowe wywołania
        Console.WriteLine("Test Refit client:");
        string refitResult = await refitClient.GetItemAsync(1);
        Console.WriteLine(refitResult);

        Console.WriteLine("Test OpenAPI Generator client:");
        string openApiResult = await openApiClient.GetItemAsync(1);
        Console.WriteLine(openApiResult);
    }
}
