using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier
{
    /// <summary>
    /// Handles sending messages to Discord webhooks.
    /// </summary>
    public class DiscordSender
    {
        /// <summary>
        /// The named HttpClient identifier used for Discord webhook requests.
        /// </summary>
        internal const string HttpClientName = "DiscordNotifier";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DiscordSender> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordSender"/> class.
        /// </summary>
        /// <param name="httpClientFactory">The HTTP client factory.</param>
        /// <param name="logger">The logger instance for logging messages.</param>
        public DiscordSender(IHttpClientFactory httpClientFactory, ILogger<DiscordSender> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// Validates that the given URL is a Discord webhook URL.
        /// </summary>
        /// <param name="url">The URL to validate.</param>
        /// <returns>True if the URL is a valid Discord webhook URL.</returns>
        public static bool IsValidDiscordWebhookUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return false;
            }

            return uri.Scheme == Uri.UriSchemeHttps
                && (uri.Host.Equals("discord.com", StringComparison.OrdinalIgnoreCase)
                    || uri.Host.Equals("discordapp.com", StringComparison.OrdinalIgnoreCase))
                && uri.AbsolutePath.StartsWith("/api/webhooks/", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Sends a POST request to the specified Discord webhook URL with the given payload.
        /// Applies webhook branding (username / avatar) from plugin configuration before
        /// serializing. Handles Discord 429 rate-limit responses with a single retry.
        /// </summary>
        /// <param name="webhookUrl">The Discord webhook URL to send the message to.</param>
        /// <param name="payload">The webhook payload to send.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation that returns true if successful, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown when webhookUrl is empty or payload is null.</exception>
        public async Task<bool> SendPostToWebhook(string webhookUrl, Models.DiscordWebhookPayload payload)
        {
            if (string.IsNullOrWhiteSpace(webhookUrl))
            {
                _logger.LogError("Webhook URL cannot be null or empty");
                throw new ArgumentNullException(nameof(webhookUrl));
            }

            ArgumentNullException.ThrowIfNull(payload);

            var config = Plugin.Config;
            if (!string.IsNullOrWhiteSpace(config.WebhookUsername)
                && !config.WebhookUsername.Contains("discord", StringComparison.OrdinalIgnoreCase))
            {
                payload.Username = config.WebhookUsername;
            }

            if (!string.IsNullOrWhiteSpace(config.WebhookAvatarUrl))
            {
                payload.AvatarUrl = config.WebhookAvatarUrl;
            }

            var jsonMessage = JsonSerializer.Serialize(payload);

            try
            {
                var client = _httpClientFactory.CreateClient(HttpClientName);
                var uri = new Uri(webhookUrl);

                for (int attempt = 0; attempt < 2; attempt++)
                {
                    using var content = new StringContent(jsonMessage, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(uri, content).ConfigureAwait(false);

                    if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        var retryAfter = await GetRetryAfterAsync(response).ConfigureAwait(false);
                        _logger.LogWarning("Discord rate limited, retrying after {Delay}s", retryAfter.TotalSeconds);
                        await Task.Delay(retryAfter).ConfigureAwait(false);
                        continue;
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        _logger.LogError(
                            "Failed to send Discord notification. Status: {StatusCode}, Response: {ResponseText}",
                            response.StatusCode,
                            responseText);
                        return false;
                    }

                    _logger.LogDebug("Successfully sent Discord notification");
                    return true;
                }

                _logger.LogError("Discord rate limited after retry, dropping notification");
                return false;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to send Discord notification (network error)");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while sending Discord notification");
                return false;
            }
        }

        private static async Task<TimeSpan> GetRetryAfterAsync(HttpResponseMessage response)
        {
            // Try reading retry_after from Discord's JSON body first.
            try
            {
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("retry_after", out var ra))
                {
                    return TimeSpan.FromSeconds(ra.GetDouble());
                }
            }
            catch
            {
                // Fall through to header / default.
            }

            // Fall back to the HTTP Retry-After header.
            if (response.Headers.RetryAfter?.Delta is TimeSpan headerDelta)
            {
                return headerDelta;
            }

            return TimeSpan.FromSeconds(2);
        }
    }
}
