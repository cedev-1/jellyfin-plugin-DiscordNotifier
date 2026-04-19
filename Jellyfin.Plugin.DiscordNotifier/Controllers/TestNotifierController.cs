using System;
using System.Text.Json;
using System.Threading.Tasks;
using Jellyfin.Plugin.DiscordNotifier.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DiscordNotifier.Controllers
{
    /// <summary>
    /// API controller for testing Discord webhook notifications.
    /// </summary>
    [Route("DiscordNotifierApi/TestNotifier")]
    [ApiController]
    public class TestNotifierController : ControllerBase
    {
        private readonly DiscordSender _sender;
        private readonly ILogger<TestNotifierController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestNotifierController"/> class.
        /// </summary>
        /// <param name="logger">The logger for the controller.</param>
        /// <param name="sender">The Discord sender service.</param>
        public TestNotifierController(ILogger<TestNotifierController> logger, DiscordSender sender)
        {
            _logger = logger;
            _sender = sender;
        }

        /// <summary>
        /// Tests the Discord webhook by sending a test message.
        /// </summary>
        /// <param name="webhookUrl">The Discord webhook URL to test.</param>
        /// <returns>A response indicating whether the test was successful.</returns>
        [HttpGet]
        public async Task<ActionResult<string>> Get([FromQuery] string webhookUrl)
        {
            var config = Plugin.Config;

            var testMessage = new DiscordWebhookPayload
            {
                Embeds =
                [
                    new DiscordEmbed
                    {
                        Title = "🪼 Test Notification",
                        Description = "This is a test message from **Jellyfin Discord Notifier**.",
                        Color = 0xAA5CC3,
                        Footer = DiscordEmbedFooter.FromConfig(config),
                        Timestamp = DateTime.UtcNow.ToString("o")
                    }
                ]
            };
            string jsonMessage = JsonSerializer.Serialize(testMessage);

            bool result = await _sender.SendPostToWebhook(webhookUrl, jsonMessage).ConfigureAwait(false);

            if (result)
            {
                return Ok("Message sent successfully");
            }
            else
            {
                return BadRequest($"Message could not be sent, please check your configuration (webhookUrl: {webhookUrl}).");
            }
        }
    }
}