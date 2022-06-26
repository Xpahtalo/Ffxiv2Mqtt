# FFXIV2MQTT
Outputs data to an MQTT broker so you can open your garage door from within the game.

## Setup
Use /mqtt to open the menu.
Defaults are fine for most cases. If you have multiple computers running the plugin, use a unique ClientID on both, and check "Include Client ID in Topic".

## Using with Home Assistant
If you are using Mosquitto Broker through HomeAssistant, you can just add new a new Home Assistant user and input the password and user into the plugin config.

Automations can be set up easily with the GUI or YAML. Here's an example:
```
- id: '1233546621323'
  alias: FFXIV Queue Pop Notification
  description: ''
  trigger:
  - platform: mqtt
    topic: ffxiv/Event/ContentFinder
    payload: Pop
  action:
  - service: notify.homeassistant_app
    data:
      message: Pop
```