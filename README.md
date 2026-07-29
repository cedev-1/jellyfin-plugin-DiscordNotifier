# jellyfin-plugin-DiscordNotifier

Jellyfin media server plugin to easily send notifications on Discord server!

![Discord Notifier](./media/DiscordNotifier.png)

## About

This plugin allows you to send notifications to your Discord server when events occur on your Jellyfin media server. With **27 configurable notification types**, you can stay informed about everything happening on your server.

**Requirements:** Jellyfin 10.11+ (`.NET 9.0`)

## Features

- **27 notification types** across media, playback, users, server and plugins
- **Webhook branding** — custom bot display name and avatar
- **Smart grouping** — episodes are batched into a single notification with configurable delay
- **Anti-spam** — debouncing, burst caps, scan-aware suppression
- **Rate-limit retry** — automatic retry on Discord 429 responses
- **IP privacy** — GDPR toggle to hide IP addresses
- **Notification preview** — Discord-styled embed preview in the config page
- **Admin navbar** — accessible directly from the Jellyfin dashboard sidebar

## Supported Notifications

| Category | Events |
|---|---|
| **Media** | Item Added, Item Deleted, Item Updated |
| **Playback** | Playback Start, Playback Stop, Playback Progress |
| **Users & Security** | User Created, User Deleted, Login Success, Login Failure, User Locked Out, User Password Changed, User Updated, User Data Saved |
| **Sessions** | Session Start, Session Ended |
| **Server** | Server Started, Pending Restart, Task Completed, Subtitle Download Failure, Lyric Download Failure |
| **Plugins** | Plugin Installed, Plugin Updated, Plugin Uninstalled, Plugin Installing, Plugin Installation Failed, Plugin Installation Cancelled |

## Installation

1. Open the dashboard of your Jellyfin server. Go to **Catalog**, click on the settings button.
2. Click **+** to add the repository URL:
```
https://raw.githubusercontent.com/cedev-1/Jellyfin-Plugin-DiscordNotifier/main/manifest.json
```
3. Go back to the **Catalog** page and install **Discord Notifier**.
4. Restart Jellyfin, then configure the plugin under **Server > Notifications** in the admin dashboard.

## Configuration

1. **Discord Connection** — paste your webhook URL and set your server name/URL
2. **Webhook Appearance** — customize the bot display name and avatar
3. **Notifications** — enable/disable each event type individually
4. **Advanced options** — grouping delays, playback filters, detailed mode

## Development

I recommend using Nix with flakes for a reproducible development environment. You can use the provided `flake.nix` file to set up your development shell with all the necessary dependencies.

```bash
# Build the plugin
dotnet build --configuration Release

# Run a local Jellyfin server with Docker
docker compose up
```

---

> Want to contribute? Open an [issue](https://github.com/cedev-1/Jellyfin-Plugin-DiscordNotifier/issues) to discuss or submit a [PR](https://github.com/cedev-1/Jellyfin-Plugin-DiscordNotifier/pulls) directly.

## License

LICENSE [MIT](./LICENSE)
