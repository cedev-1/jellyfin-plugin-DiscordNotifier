using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Jellyfin.Plugin.DiscordNotifier.Configuration;
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
        /// Sends the payload to every webhook subscribed to the given event.
        /// Each webhook gets its own branding (falling back to the global one); a failure on one
        /// webhook does not prevent delivery to the others.
        /// </summary>
        /// <param name="notifierEvent">The notification event used to select the target webhooks.</param>
        /// <param name="payload">The webhook payload to send.</param>
        /// <returns>A <see cref="Task"/> returning true if every targeted webhook accepted the message (or none are configured).</returns>
        public async Task<bool> SendAsync(NotifierEvent notifierEvent, Models.DiscordWebhookPayload payload)
        {
            ArgumentNullException.ThrowIfNull(payload);

            var config = Plugin.Config;
            var targets = config.GetWebhooksFor(notifierEvent);
            if (targets.Count == 0)
            {
                return true;
            }

            // Serialize one body per target up front: the payload is shared and branding is applied in place.
            var originalUsername = payload.Username;
            var originalAvatarUrl = payload.AvatarUrl;
            var bodies = new List<(string Url, string Json)>(targets.Count);
            try
            {
                foreach (var target in targets)
                {
                    payload.Username = originalUsername;
                    payload.AvatarUrl = originalAvatarUrl;
                    ApplyBranding(payload, config, target);
                    bodies.Add((target.Url, JsonSerializer.Serialize(payload)));
                }
            }
            finally
            {
                payload.Username = originalUsername;
                payload.AvatarUrl = originalAvatarUrl;
            }

            var results = await Task.WhenAll(bodies.Select(b => PostJsonAsync(b.Url, b.Json))).ConfigureAwait(false);
            return results.All(r => r);
        }

        /// <summary>
        /// Sends a POST request to the specified Discord webhook URL with the given payload.
        /// Applies the global webhook branding (username / avatar) before serializing.
        /// </summary>
        /// <param name="webhookUrl">The Discord webhook URL to send the message to.</param>
        /// <param name="payload">The webhook payload to send.</param>
        /// <param name="username">Optional bot name override, taking precedence over the global one.</param>
        /// <param name="avatarUrl">Optional avatar URL override, taking precedence over the global one.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation that returns true if successful, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown when webhookUrl is empty or payload is null.</exception>
        public async Task<bool> SendPostToWebhook(string webhookUrl, Models.DiscordWebhookPayload payload, string? username = null, string? avatarUrl = null)
        {
            if (string.IsNullOrWhiteSpace(webhookUrl))
            {
                _logger.LogError("Webhook URL cannot be null or empty");
                throw new ArgumentNullException(nameof(webhookUrl));
            }

            ArgumentNullException.ThrowIfNull(payload);

            ApplyBranding(payload, Plugin.Config, new WebhookConfig { Username = username ?? string.Empty, AvatarUrl = avatarUrl ?? string.Empty });
            return await PostJsonAsync(webhookUrl, JsonSerializer.Serialize(payload)).ConfigureAwait(false);
        }

        private static void ApplyBranding(Models.DiscordWebhookPayload payload, PluginConfiguration config, WebhookConfig webhook)
        {
            var username = !string.IsNullOrWhiteSpace(webhook.Username) ? webhook.Username : config.WebhookUsername;
            if (!string.IsNullOrWhiteSpace(username)
                && !username.Contains("discord", StringComparison.OrdinalIgnoreCase))
            {
                payload.Username = username;
            }

            var avatarUrl = !string.IsNullOrWhiteSpace(webhook.AvatarUrl) ? webhook.AvatarUrl : config.WebhookAvatarUrl;
            if (!string.IsNullOrWhiteSpace(avatarUrl))
            {
                payload.AvatarUrl = avatarUrl;
            }
        }

        private async Task<bool> PostJsonAsync(string webhookUrl, string jsonMessage)
        {
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
