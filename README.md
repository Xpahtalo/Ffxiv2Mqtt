# FFXIV2MQTT
Outputs data to an MQTT broker for home automation.
Requires a broker that supports MQTT V5.

## Setup
Use /mqtt to open the menu.
Defaults are fine for most cases. If you have multiple computers running the plugin, use a unique ClientID on both, and check "Include Client ID in Topic". This will change the topic structure from `ffxiv/#` to `ffxiv/ClientID/#`, allowing you to easily differentiate them.

## Using with Home Assistant
If you are using Mosquitto Broker through HomeAssistant, you can just add new a Home Assistant user and input the password and user into the plugin config.

Automations can be set up easily with the GUI or YAML. Here's an example:
```yaml
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

## Custom Messages
You can also send a custom message with  `/mqttcustom <topic> <payload>`. An example of using this would be to put `/mqttcustom lights on` in a macro, then create an automation in HomeAssistant like this:
```yaml
- id: '1654026881455'
  alias: FFXIV Turn Lights Up
  description: ''
  trigger:
  - platform: mqtt
    topic: ffxiv/lights
    payload: on
  condition: []
  action:
  - service: light.turn_on
    data: {}
    target:
      area_id:
      - bedroom
```
You now have a macro in game that can turn on your bedroom lights!