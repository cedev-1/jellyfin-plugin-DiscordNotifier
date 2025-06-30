using System.Text.Json;
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
        /// <param name="senderLogger">The logger for the Discord sender.</param>
        public TestNotifierController(ILogger<TestNotifierController> logger, ILogger<DiscordSender> senderLogger)
        {
            _logger = logger;
            _sender = new DiscordSender(senderLogger);
        }

        /// <summary>
        /// Tests the Discord webhook by sending a test message.
        /// </summary>
        /// <param name="webhookUrl">The Discord webhook URL to test.</param>
        /// <returns>A response indicating whether the test was successful.</returns>
        [HttpGet]
        public async Task<ActionResult<string>> Get([FromQuery] string webhookUrl)
        {
            var testMessage = new
            {
                content = string.Empty,
                embeds = new[]
                {
                    new
                    {
                        title = "🪼 Test Notification",
                        description = $"This is a test message from Jellyfin Discord Notifier.",
                        color = 0xaa5cc3,
                        footer = new
                        {
                            text = "Jellyfin Discord Notifier",
                            icon_url = "https://static-00.iconduck.com/assets.00/jellyfin-icon-256x255-u0iypdp6.png"
                        }
                    }
                }
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