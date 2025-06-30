using System.Text;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier
{
    /// <summary>
    /// Handles sending messages to Discord webhooks.
    /// </summary>
    public class DiscordSender : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DiscordSender> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordSender"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for logging messages.</param>
        public DiscordSender(ILogger<DiscordSender> logger)
        {
            _httpClient = new HttpClient();
            _logger = logger;
        }

        /// <summary>
        /// Releases all resources used by the <see cref="DiscordSender"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DiscordSender"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _httpClient?.Dispose();
            }
        }

        /// <summary>
        /// Sends a POST request to the specified Discord webhook URL with the given JSON message.
        /// </summary>
        /// <param name="webhookUrl">The Discord webhook URL to send the message to.</param>
        /// <param name="jsonMessage">The JSON formatted message to send.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation that returns true if successful, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown when webhookUrl or jsonMessage is null or empty.</exception>
        public async Task<bool> SendPostToWebhook(
            string webhookUrl,
            string jsonMessage)
        {
            if (string.IsNullOrWhiteSpace(webhookUrl))
            {
                _logger.LogError("Webhook URL cannot be null or empty");
                throw new ArgumentNullException(nameof(webhookUrl));
            }

            if (string.IsNullOrWhiteSpace(jsonMessage))
            {
                _logger.LogError("Message cannot be null or empty");
                throw new ArgumentNullException(nameof(jsonMessage));
            }

            try
            {
                using var content = new StringContent(jsonMessage, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(webhookUrl, content).ConfigureAwait(false);

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
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to send Discord notification to {WebhookUrl}", webhookUrl);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while sending Discord notification");
                return false;
            }
        }
    }
}
