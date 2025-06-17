using System.Net.Http.Json;
using System.Text.Json;

namespace DemoApp;

/// <summary>
/// HTTP request message extension methods.
/// </summary>
public static class HttpRequestMessageExtensions
{
    /// <summary>
    /// Adds JSON content to the HTTP request message.
    /// </summary>
    /// <typeparam name="T">The type of the JSON content.</typeparam>
    /// <param name="httpRequestMessage">The HTTP request message.</param>
    /// <param name="jsonContent">The JSON content to add.</param>
    /// <returns>The updated HTTP request message.</returns>
    public static async Task<HttpRequestMessage> AddJsonContent<T>(this HttpRequestMessage httpRequestMessage,
        T jsonContent)
    {
        httpRequestMessage.Content = JsonContent.Create(jsonContent);
        await httpRequestMessage.Content.LoadIntoBufferAsync();
        return httpRequestMessage;
    }

    /// <summary>
    /// Sends the HTTP request message and processes the response.
    /// </summary>
    /// <typeparam name="T">The type of the response content.</typeparam>
    /// <param name="httpRequestMessage">The HTTP request message.</param>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="errorMessage">The error message to throw if the request fails.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response content deserialized as an instance of type T.</returns>
    public static async Task<T> ProcessRequest<T>(this HttpRequestMessage httpRequestMessage, HttpClient httpClient, string errorMessage = "Request failed", CancellationToken cancellationToken = default)
    {
        var response = await httpClient.SendAsync(httpRequestMessage, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"{errorMessage}: {response.StatusCode} {response.ReasonPhrase} {content}");
        }

        return JsonSerializer.Deserialize<T>(content);
    }
}