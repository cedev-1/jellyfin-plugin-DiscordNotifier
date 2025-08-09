export default function (view) {
    console.log('Initializing DiscordNotifierConfig...');
    const pluginUniqueId = '8f3c85d3-19cf-4e50-a57e-7d2ce3c0c413';

    async function loadConfig() {
        console.log('Loading configuration...');
        Dashboard.showLoadingMsg();
        
        try {
            const config = await ApiClient.getPluginConfiguration(pluginUniqueId);
            console.log('Received config:', config);
            
            document.querySelector('#EnablePlugin').checked = config.EnablePlugin || false;
            document.querySelector('#webhookUrl').value = config.WebhookUrl || '';
            document.querySelector('#serverUrl').value = config.ServerUrl || '';
            document.querySelector('#UserCreatedNotifier').checked = config.UserCreatedNotifier || false;
            document.querySelector('#UserDeletedNotifier').checked = config.UserDeletedNotifier || false;
            document.querySelector('#AuthenticationSuccessNotifier').checked = config.AuthenticationSuccessNotifier || false;
            document.querySelector('#AuthenticationFailureNotifier').checked = config.AuthenticationFailureNotifier || false;
            
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
            config.UserCreatedNotifier = document.querySelector('#UserCreatedNotifier').checked;
            config.UserDeletedNotifier = document.querySelector('#UserDeletedNotifier').checked;
            config.AuthenticationSuccessNotifier = document.querySelector('#AuthenticationSuccessNotifier').checked;
            config.AuthenticationFailureNotifier = document.querySelector('#AuthenticationFailureNotifier').checked;

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

    view.addEventListener('viewshow', loadConfig);
    document.getElementById('saveButton').addEventListener('click', saveConfig);
    document.getElementById('testButton').addEventListener('click', testWebhook);
}
