﻿namespace Aircloak.JsonApi
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text.Json;
    using System.Threading.Tasks;

    /// <summary>
    /// Convenience class derived from <c>HttpClient</c> provides GET and POST methods adapted to the
    /// Aircloak API:
    /// <list type="bullet">
    /// <item>
    /// <description>Sets the provided Api Key on all outgoing requests.</description>
    /// </item>
    /// <item>
    /// <description>Augments unsuccessful requests with custom error messages. </description>
    /// </item>
    /// <item>
    /// <description>Deserializes Json responses.</description>
    /// </item>
    /// </list>
    /// </summary>
    public class JsonApiClient
    {
        private static readonly JsonSerializerOptions DefaultJsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
        };

        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonApiClient" /> class.
        /// </summary>
        /// <param name="httpClient">A HttpClient object injected into this instance.</param>
        public JsonApiClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        /// <summary>
        /// Send a GET request to the Aircloak API. Handles authentication.
        /// </summary>
        /// <param name="apiEndpoint">The API endpoint to target.</param>
        /// <param name="apiKey">The API key for the service.</param>
        /// <param name="options">Overrides the default <c>JsonSerializerOptions</c>.</param>
        /// <typeparam name="T">The type to deserialize the JSON response to.</typeparam>
        /// <returns>A <c>Task&lt;T&gt;</c> which, upon completion, contains the API response deserialized
        /// to the provided return type.</returns>
        /// <exception cref="HttpRequestException">The request failed due to an underlying issue
        /// such as network connectivity, DNS failure, server certificate validation or timeout.
        /// </exception>
        /// <exception cref="JsonException">The JSON is invalid.
        /// -or- <c>T</c> is not compatible with the JSON.
        /// -or- There is remaining data in the stream.
        /// </exception>
        public async Task<T> ApiGetRequest<T>(
            string apiEndpoint,
            string apiKey,
            JsonSerializerOptions? options = null)
        {
            return await ApiRequest<T>(HttpMethod.Get, apiEndpoint, apiKey, options: options);
        }

        /// <summary>
        /// Send a POST request to the Aircloak API. Handles authentication.
        /// </summary>
        /// <param name="apiEndpoint">The API endpoint to target.</param>
        /// <param name="apiKey">The API key for the service.</param>
        /// <param name="requestContent">JSON-encoded request message (optional).</param>
        /// <param name="options">Overrides the default <c>JsonSerializerOptions</c>.</param>
        /// <typeparam name="T">The type to deserialize the JSON response to.</typeparam>
        /// <returns>A <c>Task&lt;T&gt;</c> which, upon completion, contains the API response deserialized
        /// to the provided return type.</returns>
        /// <exception cref="HttpRequestException">The request failed due to an underlying issue
        /// such as network connectivity, DNS failure, server certificate validation or timeout.
        /// </exception>
        /// <exception cref="JsonException">The JSON is invalid.
        /// -or- <c>T</c> is not compatible with the JSON.
        /// -or- There is remaining data in the stream.
        /// </exception>
        public async Task<T> ApiPostRequest<T>(
            string apiEndpoint,
            string apiKey,
            string? requestContent = default,
            JsonSerializerOptions? options = null)
        {
            return await ApiRequest<T>(HttpMethod.Post, apiEndpoint, apiKey, requestContent, options);
        }

        /// <summary>
        /// Turns the HTTP response into a custom error string.
        /// </summary>
        /// <param name="response">The HTTP response.</param>
        /// <returns>A string containing a custom error message.</returns>
        private static string ServiceError(HttpResponseMessage response)
        {
            return response.StatusCode switch
            {
                HttpStatusCode.Unauthorized =>
                    "Unauthorized -- Your API token is wrong",
                HttpStatusCode.NotFound =>
                    "Not Found -- Invalid URL",
                HttpStatusCode.InternalServerError =>
                    "Internal Server Error -- We had a problem with our server. Try again later.",
                HttpStatusCode.ServiceUnavailable =>
                    "Service Unavailable -- We're temporarily offline for maintenance. Please try again later.",
                HttpStatusCode.GatewayTimeout =>
                    "Gateway Timeout -- A timeout occured while contacting the data source. " +
                    "The system might be overloaded. Try again later.",
                _ => response.StatusCode.ToString(),
            };
        }

        /// <summary>
        /// Send a request to the Aircloak API. Handles authentication.
        /// </summary>
        /// <param name="requestMethod">The HTTP method to use in the request.</param>
        /// <param name="apiEndpoint">The API endpoint to target.</param>
        /// <param name="apiKey">The API key for the service.</param>
        /// <param name="requestContent">JSON-encoded request message (optional).</param>
        /// <param name="options">Overrides the default <c>JsonSerializerOptions</c>.</param>
        /// <typeparam name="T">The type to deserialize the JSON response to.</typeparam>
        /// <returns>A <c>Task&lt;T&gt;</c> which, upon completion, contains the API response deserialized
        /// to the provided return type.</returns>
        /// <exception cref="HttpRequestException">The request failed due to an underlying issue
        /// such as network connectivity, DNS failure, server certificate validation or timeout.
        /// </exception>
        /// <exception cref="JsonException">The JSON is invalid.
        /// -or- <c>T</c> is not compatible with the JSON.
        /// -or- There is remaining data in the stream.
        /// </exception>
        private async Task<T> ApiRequest<T>(
            HttpMethod requestMethod,
            string apiEndpoint,
            string apiKey,
            string? requestContent = default,
            JsonSerializerOptions? options = null)
        {
            using var requestMessage =
                new HttpRequestMessage(requestMethod, apiEndpoint);

            if (!requestMessage.Headers.TryAddWithoutValidation("auth-token", apiKey))
            {
                throw new Exception($"Failed to add Http header 'auth-token: {apiKey}'");
            }

            if (!(requestContent is null))
            {
                requestMessage.Content = new StringContent(requestContent);
                requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(System.Net.Mime.MediaTypeNames.Application.Json);
            }

            using var response = await httpClient.SendAsync(
                requestMessage,
                HttpCompletionOption.ResponseHeadersRead);

            if (response.IsSuccessStatusCode)
            {
                using var contentStream = await response.Content.ReadAsStreamAsync();
                return await JsonSerializer.DeserializeAsync<T>(
                    contentStream,
                    options ?? DefaultJsonOptions);
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request Error: {ServiceError(response)}.\n{requestMessage}\n{requestContent}\n{responseContent}");
            }
        }
    }
}
