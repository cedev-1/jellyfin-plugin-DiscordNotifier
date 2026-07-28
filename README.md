# jellyfin-plugin-DiscordNotifier

 Jellyfin media server plugin to easily send notifications on Discord server ! 

![Discord Notifier](./media/DiscordNotifier.png)

## About

This plugin allows you to send notifications to your Discord server when events occur on your Jellyfin media server. You can configure the notifications to be sent for various events such as new media added, media played, and more.

Current notifications (v1.7.0.0) :
 - UserCreated
 - UserDeleted
 - UserConnection
 - UserFailedConnection
 - ItemAdded
 - PlaybackStart — (with options)
 - PlaybackStop — (with options)

dotnet version : 9.0

## Installation

1. You have to open the dashboard of your Jellyfin server. Go to Catalog, click on ⚙️ button.
2. Click to + to add the URL.
```bash
https://raw.githubusercontent.com/cedev-1/Jellyfin-Plugin-DiscordNotifier/master/manifest.json
```
3. On the Catalog page click on Install.

## Plugin Configuration

<img width="909" height="692" alt="image" src="https://github.com/user-attachments/assets/8836bb18-51c2-4dc2-a7ef-d17df4935baa" />

<img width="909" height="692" alt="image" src="https://github.com/user-attachments/assets/3e21ed71-39a7-4fdd-9a84-0ca83033270e" />

<img width="909" height="475" alt="image" src="https://github.com/user-attachments/assets/d9c1fecc-3da8-4617-b944-fd732b211482" />

## Development

I recommend using Nix with flakes for a reproducible development environment. You can use the provided `flake.nix` file to set up your development shell with all the necessary dependencies.

You can also use the `Taskfile.yml` to automate common tasks during the development process like building and run in a docker local jellyfin server.

For Task you can run  

```bash
task build
task clean
```

## TODO

The following features are planned.

### Notifications

- [x] `UserLockedOut` — notify when an account is locked after too many failed attempts
- [x] `UserUpdated` — notify when a user account is modified
- [x] `UserPasswordChanged` — notify when a user changes their password
- [x] `SessionStart` — notify when a new client session is opened (before playback)
- [x] `PlaybackProgress` — notify at a defined interval during playback (configurable, movies and series only)
- [x] `ItemDeleted` — notify when media is removed from the library
- [x] `TaskCompleted` — notify when a scheduled task finishes (library scan, backup, etc.)
- [x] `PluginInstalled` / `PluginUpdated` / `PluginUninstalled` / `PluginInstallationFailed` / `PluginInstallationCancelled` — notify on plugin lifecycle events
- [x] `PendingRestart` — notify when the server requires a restart after an update
- [x] `SubtitleDownloadFailure` — notify when a subtitle download fails
- [x] `UserDataSaved` — notify when a user marks content as watched/unwatched or rates it

---

> Want to contribute? Open an [issue](https://github.com/cedev-1/Jellyfin-Plugin-DiscordNotifier/issues) to discuss or submit a [PR](https://github.com/cedev-1/Jellyfin-Plugin-DiscordNotifier/pulls) directly.

## License

LICENSE [MIT](./LICENSE)
