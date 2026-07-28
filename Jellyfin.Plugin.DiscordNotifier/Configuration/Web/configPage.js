export default function (view) {
    console.log('Initializing DiscordNotifierConfig...');
    const pluginUniqueId = '8f3c85d3-19cf-4e50-a57e-7d2ce3c0c413';

    async function loadConfig() {
        console.log('Loading configuration...');
        Dashboard.showLoadingMsg();

        try {
            const config = await ApiClient.getPluginConfiguration(pluginUniqueId);
            console.log('Received config:', config);

            document.querySelector('#EnablePlugin').checked = config.EnablePlugin !== false;
            document.querySelector('#webhookUrl').value = config.WebhookUrl || '';
            document.querySelector('#serverUrl').value = config.ServerUrl || '';
            document.querySelector('#serverName').value = config.ServerName || '';
            document.querySelector('#webhookUsername').value = config.WebhookUsername || '';
            document.querySelector('#webhookAvatarUrl').value = config.WebhookAvatarUrl || '';

            document.querySelector('#ItemAddedNotifier').checked = config.ItemAddedNotifier !== false;
            document.querySelector('#ItemAddedGroupingDelay').value = config.ItemAddedGroupingDelayMinutes || 5;
            document.querySelector('#ItemAddedMaxWait').value = config.ItemAddedMaxWaitMinutes || 60;
            document.querySelector('#ItemAddedNotifyMovies').checked = config.ItemAddedNotifyMovies !== false;
            document.querySelector('#ItemAddedNotifySeries').checked = config.ItemAddedNotifySeries !== false;
            document.querySelector('#ItemAddedNotifyMusic').checked = config.ItemAddedNotifyMusic !== false;
            document.querySelector('#ItemAddedNotifyBooks').checked = config.ItemAddedNotifyBooks !== false;
            document.querySelector('#ItemDeletedNotifier').checked = config.ItemDeletedNotifier || false;
            document.querySelector('#ItemUpdatedNotifier').checked = config.ItemUpdatedNotifier || false;

            document.querySelector('#UserDataSavedNotifier').checked = config.UserDataSavedNotifier || false;
            document.querySelector('#SubtitleDownloadFailureNotifier').checked = config.SubtitleDownloadFailureNotifier || false;
            document.querySelector('#LyricDownloadFailureNotifier').checked = config.LyricDownloadFailureNotifier || false;

            document.querySelector('#PlaybackStartNotifier').checked = config.PlaybackStartNotifier || false;
            document.querySelector('#PlaybackStopNotifier').checked = config.PlaybackStopNotifier || false;
            document.querySelector('#PlaybackNotifyMovies').checked = config.PlaybackNotifyMovies !== false;
            document.querySelector('#PlaybackNotifySeries').checked = config.PlaybackNotifySeries || false;
            document.querySelector('#PlaybackDetailedMode').checked = config.PlaybackDetailedMode || false;
            document.querySelector('#PlaybackTranscodeOnlyNotifier').checked = config.PlaybackTranscodeOnlyNotifier || false;
            document.querySelector('#PlaybackProgressNotifier').checked = config.PlaybackProgressNotifier || false;
            document.querySelector('#PlaybackProgressInterval').value = config.PlaybackProgressIntervalMinutes || 10;

            document.querySelector('#UserCreatedNotifier').checked = config.UserCreatedNotifier !== false;
            document.querySelector('#UserDeletedNotifier').checked = config.UserDeletedNotifier !== false;
            document.querySelector('#AuthenticationSuccessNotifier').checked = config.AuthenticationSuccessNotifier !== false;
            document.querySelector('#AuthenticationFailureNotifier').checked = config.AuthenticationFailureNotifier !== false;
            document.querySelector('#UserLockedOutNotifier').checked = config.UserLockedOutNotifier !== false;
            document.querySelector('#UserPasswordChangedNotifier').checked = config.UserPasswordChangedNotifier !== false;
            document.querySelector('#UserUpdatedNotifier').checked = config.UserUpdatedNotifier || false;
            document.querySelector('#IncludeIpAddresses').checked = config.IncludeIpAddresses || false;

            document.querySelector('#SessionStartNotifier').checked = config.SessionStartNotifier || false;
            document.querySelector('#SessionEndedNotifier').checked = config.SessionEndedNotifier || false;
            document.querySelector('#TaskCompletedNotifier').checked = config.TaskCompletedNotifier || false;
            document.querySelector('#TaskCompletedOnlyOnFailure').checked = config.TaskCompletedOnlyOnFailure !== false;
            document.querySelector('#PendingRestartNotifier').checked = config.PendingRestartNotifier !== false;
            document.querySelector('#ServerStartedNotifier').checked = config.ServerStartedNotifier || false;

            document.querySelector('#PluginInstalledNotifier').checked = config.PluginInstalledNotifier !== false;
            document.querySelector('#PluginUpdatedNotifier').checked = config.PluginUpdatedNotifier !== false;
            document.querySelector('#PluginUninstalledNotifier').checked = config.PluginUninstalledNotifier !== false;
            document.querySelector('#PluginInstallationFailedNotifier').checked = config.PluginInstallationFailedNotifier !== false;
            document.querySelector('#PluginInstallationCancelledNotifier').checked = config.PluginInstallationCancelledNotifier || false;
            document.querySelector('#PluginInstallingNotifier').checked = config.PluginInstallingNotifier || false;

            updateAvatarPreview();
            renderPreview();

        } catch (error) {
            console.error('Error loading config:', error);
            Dashboard.alert('Error loading configuration: ' + error.message);
        } finally {
            Dashboard.hideLoadingMsg();
        }
    }

    async function saveConfig(e) {
        console.log('Saving configuration...');
        if (e) e.preventDefault();

        Dashboard.showLoadingMsg();

        try {
            const config = await ApiClient.getPluginConfiguration(pluginUniqueId);

            config.EnablePlugin = document.querySelector('#EnablePlugin').checked;
            config.WebhookUrl = document.querySelector('#webhookUrl').value.trim();
            config.ServerUrl = document.querySelector('#serverUrl').value.trim();
            config.ServerName = document.querySelector('#serverName').value.trim();
            config.WebhookUsername = document.querySelector('#webhookUsername').value.trim();
            config.WebhookAvatarUrl = document.querySelector('#webhookAvatarUrl').value.trim();

            config.ItemAddedNotifier = document.querySelector('#ItemAddedNotifier').checked;
            config.ItemAddedGroupingDelayMinutes = parseInt(document.querySelector('#ItemAddedGroupingDelay').value, 10) || 5;
            config.ItemAddedMaxWaitMinutes = parseInt(document.querySelector('#ItemAddedMaxWait').value, 10) || 60;
            config.ItemAddedNotifyMovies = document.querySelector('#ItemAddedNotifyMovies').checked;
            config.ItemAddedNotifySeries = document.querySelector('#ItemAddedNotifySeries').checked;
            config.ItemAddedNotifyMusic = document.querySelector('#ItemAddedNotifyMusic').checked;
            config.ItemAddedNotifyBooks = document.querySelector('#ItemAddedNotifyBooks').checked;
            config.ItemDeletedNotifier = document.querySelector('#ItemDeletedNotifier').checked;
            config.ItemUpdatedNotifier = document.querySelector('#ItemUpdatedNotifier').checked;

            config.UserDataSavedNotifier = document.querySelector('#UserDataSavedNotifier').checked;
            config.SubtitleDownloadFailureNotifier = document.querySelector('#SubtitleDownloadFailureNotifier').checked;
            config.LyricDownloadFailureNotifier = document.querySelector('#LyricDownloadFailureNotifier').checked;

            config.PlaybackStartNotifier = document.querySelector('#PlaybackStartNotifier').checked;
            config.PlaybackStopNotifier = document.querySelector('#PlaybackStopNotifier').checked;
            config.PlaybackNotifyMovies = document.querySelector('#PlaybackNotifyMovies').checked;
            config.PlaybackNotifySeries = document.querySelector('#PlaybackNotifySeries').checked;
            config.PlaybackDetailedMode = document.querySelector('#PlaybackDetailedMode').checked;
            config.PlaybackTranscodeOnlyNotifier = document.querySelector('#PlaybackTranscodeOnlyNotifier').checked;
            config.PlaybackProgressNotifier = document.querySelector('#PlaybackProgressNotifier').checked;
            config.PlaybackProgressIntervalMinutes = parseInt(document.querySelector('#PlaybackProgressInterval').value, 10) || 10;

            config.UserCreatedNotifier = document.querySelector('#UserCreatedNotifier').checked;
            config.UserDeletedNotifier = document.querySelector('#UserDeletedNotifier').checked;
            config.AuthenticationSuccessNotifier = document.querySelector('#AuthenticationSuccessNotifier').checked;
            config.AuthenticationFailureNotifier = document.querySelector('#AuthenticationFailureNotifier').checked;
            config.UserLockedOutNotifier = document.querySelector('#UserLockedOutNotifier').checked;
            config.UserPasswordChangedNotifier = document.querySelector('#UserPasswordChangedNotifier').checked;
            config.UserUpdatedNotifier = document.querySelector('#UserUpdatedNotifier').checked;
            config.IncludeIpAddresses = document.querySelector('#IncludeIpAddresses').checked;

            config.SessionStartNotifier = document.querySelector('#SessionStartNotifier').checked;
            config.SessionEndedNotifier = document.querySelector('#SessionEndedNotifier').checked;
            config.TaskCompletedNotifier = document.querySelector('#TaskCompletedNotifier').checked;
            config.TaskCompletedOnlyOnFailure = document.querySelector('#TaskCompletedOnlyOnFailure').checked;
            config.PendingRestartNotifier = document.querySelector('#PendingRestartNotifier').checked;
            config.ServerStartedNotifier = document.querySelector('#ServerStartedNotifier').checked;

            config.PluginInstalledNotifier = document.querySelector('#PluginInstalledNotifier').checked;
            config.PluginUpdatedNotifier = document.querySelector('#PluginUpdatedNotifier').checked;
            config.PluginUninstalledNotifier = document.querySelector('#PluginUninstalledNotifier').checked;
            config.PluginInstallationFailedNotifier = document.querySelector('#PluginInstallationFailedNotifier').checked;
            config.PluginInstallationCancelledNotifier = document.querySelector('#PluginInstallationCancelledNotifier').checked;
            config.PluginInstallingNotifier = document.querySelector('#PluginInstallingNotifier').checked;

            console.log('Saving config:', config);

            await ApiClient.updatePluginConfiguration(pluginUniqueId, config);

            console.log('Configuration saved successfully');
            Dashboard.alert('Configuration saved successfully!');

        } catch (error) {
            console.error('Error saving config:', error);
            Dashboard.alert('Error saving configuration: ' + error.message);
        } finally {
            Dashboard.hideLoadingMsg();
        }
    }

    function testWebhook() {
        console.log('Testing webhook...');
        const button = this;
        const originalText = button.querySelector('span').textContent;
        button.disabled = true;

        const webhookUrl = document.querySelector('#webhookUrl').value.trim();

        if (!webhookUrl) {
            Dashboard.alert('Please enter a webhook URL first');
            button.disabled = false;
            return;
        }

        if (!webhookUrl.startsWith('https://discord.com/api/webhooks/')) {
            Dashboard.alert('Please enter a valid Discord webhook URL');
            button.disabled = false;
            return;
        }

        button.querySelector('span').textContent = 'Testing...';

        const encodedUrl = encodeURIComponent(webhookUrl);

        ApiClient.ajax({
            url: ApiClient.getUrl(`DiscordNotifierApi/TestNotifier?webhookUrl=${encodedUrl}`),
            type: 'GET'
        })
        .then(() => {
            console.log('Webhook test successful');
            button.style.backgroundColor = 'green';
            button.querySelector('span').textContent = 'Test successful!';
        })
        .catch(error => {
            console.error('Webhook test failed:', error);
            button.style.backgroundColor = 'red';
            button.querySelector('span').textContent = 'Test failed!';
        })
        .finally(() => {
            setTimeout(() => {
                button.disabled = false;
                button.style.backgroundColor = '';
                button.querySelector('span').textContent = originalText;
            }, 3000);
        });
    }

    function updateAvatarPreview() {
        const url = document.querySelector('#webhookAvatarUrl').value.trim();
        const targets = [
            { img: document.getElementById('avatarPreviewImg'), fallback: document.getElementById('avatarPreviewFallback') },
            { img: document.getElementById('previewAvatarImg'), fallback: document.getElementById('previewAvatarFallback') }
        ];
        targets.forEach(({ img, fallback }) => {
            if (url) {
                img.src = url;
                img.style.display = 'block';
                img.onerror = () => { img.style.display = 'none'; fallback.style.display = ''; };
                fallback.style.display = 'none';
            } else {
                img.style.display = 'none';
                fallback.style.display = '';
            }
        });
    }

    const previewData = {
        newMovie: {
            title: 'New Movie: Interstellar (2014)',
            description: "When Earth becomes uninhabitable in the future, a farmer and ex-NASA pilot is tasked with piloting a spacecraft...",
            color: 0x9B59B6,
            thumbnail: true,
            fields: null
        },
        newSeriesGrouped: {
            title: 'New Series: Breaking Bad (2008)',
            description: '**Season 1**\nS01E01 \u2014 Pilot\nS01E02 \u2014 Cat\'s in the Bag...\nS01E03 \u2014 ...And the Bag\'s in the River',
            color: 0x9B59B6,
            thumbnail: true,
            fields: null
        },
        newAlbumGrouped: {
            title: 'New Album: Abbey Road (1969)',
            description: '**The Beatles**\n\n01. Come Together\n02. Something\n03. Maxwell\'s Silver Hammer\n04. Oh! Darling\n05. Octopus\'s Garden',
            color: 0x9B59B6,
            thumbnail: true,
            fields: null
        },
        contentDeleted: {
            title: 'Content Removed: The Office (2005)',
            description: 'A series has been removed from the library.',
            color: 0xE74C3C,
            thumbnail: false,
            fields: [
                { name: 'Type', value: 'Series', inline: true },
                { name: 'Library', value: 'TV Shows', inline: true }
            ]
        },
        contentUpdated: {
            title: 'Metadata Updated: Inception (2010)',
            description: 'Metadata has been refreshed for this item.',
            color: 0x3498DB,
            thumbnail: true,
            fields: [
                { name: 'Type', value: 'Movie', inline: true }
            ]
        },
        subtitleFailure: {
            title: 'Subtitle Download Failed',
            description: 'Failed to download subtitles for **Breaking Bad \u2014 S01E01**.',
            color: 0xE74C3C,
            thumbnail: false,
            fields: [
                { name: 'Provider', value: 'OpenSubtitles', inline: true },
                { name: 'Error', value: 'Request timed out', inline: true }
            ]
        },
        lyricFailure: {
            title: 'Lyric Download Failed',
            description: 'Failed to download lyrics for **Come Together**.',
            color: 0xE74C3C,
            thumbnail: false,
            fields: [
                { name: 'Provider', value: 'LrcLib', inline: true },
                { name: 'Error', value: 'No match found', inline: true }
            ]
        },
        watchStatus: {
            title: 'Watch Status Update',
            description: '**john_doe** marked **Interstellar** as watched.',
            color: 0x2ECC71,
            thumbnail: false,
            fields: [
                { name: 'User', value: 'john_doe', inline: true },
                { name: 'Action', value: 'Watched', inline: true }
            ]
        },
        watchStatusBatch: {
            title: 'Batch Update: 8 items',
            description: '**john_doe** marked as watched **8** items.',
            color: 0x2ECC71,
            thumbnail: false,
            fields: null
        },
        playbackStart: {
            title: 'Playback Started',
            description: '**john_doe** started playing **Interstellar (2014)**',
            color: 0x9B59B6,
            thumbnail: true,
            fields: [
                { name: 'User', value: 'john_doe', inline: true },
                { name: 'Device', value: 'iPhone 15', inline: true },
                { name: 'Client', value: 'Jellyfin Mobile', inline: true }
            ]
        },
        playbackStartDetailed: {
            title: 'Playback Started',
            description: '**john_doe** started playing **Interstellar (2014)**',
            color: 0x9B59B6,
            thumbnail: true,
            fields: [
                { name: 'User', value: 'john_doe', inline: true },
                { name: 'Device', value: 'Apple TV', inline: true },
                { name: 'Client', value: 'Swiftfin', inline: true },
                { name: 'Video', value: '4K HEVC HDR', inline: true },
                { name: 'Audio', value: 'TrueHD 7.1 Atmos', inline: true },
                { name: 'Play Method', value: 'Direct Play', inline: true },
                { name: 'Transcode Reason', value: '\u2014', inline: false }
            ]
        },
        playbackStop: {
            title: 'Playback Stopped',
            description: '**john_doe** stopped playing **Interstellar (2014)**',
            color: 0x9B59B6,
            thumbnail: true,
            fields: [
                { name: 'User', value: 'john_doe', inline: true },
                { name: 'Device', value: 'iPhone 15', inline: true },
                { name: 'Progress', value: '75% (1h 52m / 2h 49m)', inline: true }
            ]
        },
        playbackProgress: {
            title: 'Now Playing',
            description: '**john_doe** is watching **Interstellar (2014)**',
            color: 0x9B59B6,
            thumbnail: true,
            fields: [
                { name: 'Progress', value: '45% (1h 16m / 2h 49m)', inline: true },
                { name: 'Device', value: 'iPhone 15', inline: true }
            ]
        },
        userCreated: {
            title: 'New User Created',
            description: 'A new account has been created on the server.',
            color: 0x4C8BF5,
            thumbnail: false,
            fields: [
                { name: 'Username', value: 'john_doe', inline: true },
                { name: 'User ID', value: 'a1b2c3d4-e5f6-7890-abcd-ef1234567890', inline: true }
            ]
        },
        userDeleted: {
            title: 'User Deleted',
            description: 'An account has been removed from the server.',
            color: 0xE74C3C,
            thumbnail: false,
            fields: [
                { name: 'Username', value: 'old_user', inline: true },
                { name: 'User ID', value: 'a1b2c3d4-e5f6-7890-abcd-ef1234567890', inline: true }
            ]
        },
        loginSuccess: {
            title: 'Login Successful',
            description: '**john_doe** logged in successfully.',
            color: 0x2ECC71,
            thumbnail: false,
            fields: [
                { name: 'Device', value: 'Chrome (Windows)', inline: true },
                { name: 'Client', value: 'Jellyfin Web', inline: true }
            ]
        },
        loginFailure: {
            title: 'Login Failed',
            description: 'A failed login attempt was detected.',
            color: 0xE74C3C,
            thumbnail: false,
            fields: [
                { name: 'Username', value: 'admin', inline: true },
                { name: 'Device', value: 'Unknown Browser', inline: true },
                { name: 'Client', value: 'Jellyfin Web', inline: true }
            ]
        },
        userLockedOut: {
            title: 'User Locked Out',
            description: '**john_doe** has been locked out after too many failed login attempts.',
            color: 0xE74C3C,
            thumbnail: false,
            fields: [
                { name: 'Username', value: 'john_doe', inline: true }
            ]
        },
        passwordChanged: {
            title: 'Password Changed',
            description: '**john_doe** has changed their password.',
            color: 0xF1C40F,
            thumbnail: false,
            fields: [
                { name: 'Username', value: 'john_doe', inline: true }
            ]
        },
        userUpdated: {
            title: 'User Account Updated',
            description: 'The account **john_doe** has been updated.',
            color: 0x3498DB,
            thumbnail: false,
            fields: [
                { name: 'Username', value: 'john_doe', inline: true },
                { name: 'User ID', value: 'a1b2c3d4-e5f6-7890-abcd-ef1234567890', inline: true }
            ]
        },
        sessionStart: {
            title: 'Session Started',
            description: '**john_doe** started a new session.',
            color: 0x3498DB,
            thumbnail: false,
            fields: [
                { name: 'Device', value: 'iPhone 15', inline: true },
                { name: 'Client', value: 'Jellyfin Mobile', inline: true }
            ]
        },
        sessionEnded: {
            title: 'Session Ended',
            description: '**john_doe** session has ended.',
            color: 0x3498DB,
            thumbnail: false,
            fields: [
                { name: 'Device', value: 'iPhone 15', inline: true },
                { name: 'Client', value: 'Jellyfin Mobile', inline: true }
            ]
        },
        taskCompleted: {
            title: 'Task Completed',
            description: 'A scheduled task finished successfully.',
            color: 0x2ECC71,
            thumbnail: false,
            fields: [
                { name: 'Task', value: 'Scan Media Library', inline: true },
                { name: 'Category', value: 'Library', inline: true },
                { name: 'Duration', value: '2m 34s', inline: true }
            ]
        },
        taskFailed: {
            title: 'Task Failed',
            description: 'A scheduled task finished with an error.',
            color: 0xE74C3C,
            thumbnail: false,
            fields: [
                { name: 'Task', value: 'Download Subtitles', inline: true },
                { name: 'Category', value: 'Library', inline: true },
                { name: 'Error', value: 'Connection timed out', inline: false }
            ]
        },
        pendingRestart: {
            title: 'Restart Required',
            description: 'The server requires a restart to apply pending changes.',
            color: 0xE67E22,
            thumbnail: false,
            fields: null
        },
        serverStarted: {
            title: 'Server Started',
            description: 'The Jellyfin server has started successfully.',
            color: 0x2ECC71,
            thumbnail: false,
            fields: null
        },
        pluginInstalled: {
            title: 'Plugin Installed',
            description: '**Intro Skipper** has been installed.',
            color: 0xAA5CC3,
            thumbnail: false,
            fields: [
                { name: 'Plugin', value: 'Intro Skipper', inline: true },
                { name: 'Version', value: '1.2.3.0', inline: true }
            ]
        },
        pluginUpdated: {
            title: 'Plugin Updated',
            description: '**Intro Skipper** has been updated.',
            color: 0xAA5CC3,
            thumbnail: false,
            fields: [
                { name: 'Plugin', value: 'Intro Skipper', inline: true },
                { name: 'Version', value: '1.3.0.0', inline: true }
            ]
        },
        pluginUninstalled: {
            title: 'Plugin Uninstalled',
            description: '**Intro Skipper** has been removed.',
            color: 0xE74C3C,
            thumbnail: false,
            fields: [
                { name: 'Plugin', value: 'Intro Skipper', inline: true }
            ]
        },
        pluginInstallFailed: {
            title: 'Plugin Installation Failed',
            description: '**Intro Skipper** could not be installed.',
            color: 0xE74C3C,
            thumbnail: false,
            fields: [
                { name: 'Plugin', value: 'Intro Skipper', inline: true },
                { name: 'Error', value: 'Checksum mismatch', inline: true }
            ]
        },
        pluginInstallCancelled: {
            title: 'Plugin Installation Cancelled',
            description: 'The installation of **Intro Skipper** was cancelled.',
            color: 0xE67E22,
            thumbnail: false,
            fields: [
                { name: 'Plugin', value: 'Intro Skipper', inline: true }
            ]
        },
        pluginInstalling: {
            title: 'Plugin Installing',
            description: '**Intro Skipper** is being installed...',
            color: 0xE67E22,
            thumbnail: false,
            fields: [
                { name: 'Plugin', value: 'Intro Skipper', inline: true },
                { name: 'Version', value: '1.2.3.0', inline: true }
            ]
        }
    };

    const footerIconUrl = 'https://external-content.duckduckgo.com/iu/?u=https%3A%2F%2Fstatic0.xdaimages.com%2Fwordpress%2Fwp-content%2Fuploads%2F2024%2F02%2Fjellyfin-logo.png%3Fq%3D70%26fit%3Dcontain%26w%3D420%26dpr%3D1&f=1&nofb=1&ipt=403502d7a0586532bfb11842ad227f3212229625ccbf001cd1f60dc0abf41465';

    function renderPreview() {
        const eventKey = document.getElementById('previewEventSelect').value;
        const data = previewData[eventKey];
        if (!data) return;

        // Bot name
        const botName = document.querySelector('#webhookUsername').value.trim() || 'Jellyfin';
        document.getElementById('previewBotName').textContent = botName;

        // Timestamp
        const now = new Date();
        const hours = now.getHours().toString().padStart(2, '0');
        const minutes = now.getMinutes().toString().padStart(2, '0');
        document.getElementById('previewTimestamp').textContent = 'Today at ' + hours + ':' + minutes;

        // Avatar
        updateAvatarPreview();

        // Embed border color
        const previewEmbed = document.getElementById('previewEmbed');
        previewEmbed.style.borderLeftColor = '#' + data.color.toString(16).padStart(6, '0');

        // Title & description — convert markdown bold to HTML
        const titleEl = document.getElementById('previewEmbedTitle');
        titleEl.textContent = data.title;

        const descEl = document.getElementById('previewEmbedDescription');
        descEl.innerHTML = data.description.replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>');

        // Fields
        const fieldsEl = document.getElementById('previewEmbedFields');
        fieldsEl.innerHTML = '';
        if (data.fields && data.fields.length > 0) {
            fieldsEl.style.display = 'grid';
            data.fields.forEach(function(field) {
                const fieldDiv = document.createElement('div');
                fieldDiv.style.minWidth = '0';
                fieldDiv.style.gridColumn = field.inline ? '' : '1 / -1';
                fieldDiv.innerHTML =
                    '<div style="color: #949ba4; font-size: 0.75em; font-weight: 600; margin-bottom: 2px;">' + field.name + '</div>' +
                    '<div style="font-size: 0.875em; color: #dbdee1;">' + field.value + '</div>';
                fieldsEl.appendChild(fieldDiv);
            });
        } else {
            fieldsEl.style.display = 'none';
        }

        // Footer
        document.getElementById('previewFooterIcon').src = footerIconUrl;
        var footerName = document.querySelector('#serverName').value.trim() || 'Jellyfin Discord Notifier';
        document.getElementById('previewFooterText').textContent = footerName + ' \u2022 Today at ' + hours + ':' + minutes;

        // Thumbnail
        document.getElementById('previewThumbnail').style.display = data.thumbnail ? 'block' : 'none';
    }

    view.addEventListener('viewshow', loadConfig);
    document.getElementById('saveButton').addEventListener('click', saveConfig);
    document.getElementById('testButton').addEventListener('click', testWebhook);
    document.getElementById('previewEventSelect').addEventListener('change', renderPreview);
    document.getElementById('webhookUsername').addEventListener('input', function() {
        var warn = document.getElementById('webhookUsernameWarning');
        warn.style.display = this.value.toLowerCase().includes('discord') ? '' : 'none';
        renderPreview();
    });
    document.getElementById('webhookAvatarUrl').addEventListener('input', function() { updateAvatarPreview(); renderPreview(); });
    document.getElementById('serverName').addEventListener('input', renderPreview);
}
