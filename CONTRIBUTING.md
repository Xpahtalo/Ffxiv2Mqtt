# New Features

Please open an issue first so that we can discuss the feature and decide if it is appropriate in FFXIV2MQTT.

### New Topics

Create a new class inheriting [Topic](https://github.com/Xpahtalo/Ffxiv2Mqtt/blob/main/ffxiv2Mqtt/Topics/Topic.cs). Reflection will handle
actually creating the class at runtime. Services are injected after the constructor, so override[Initialize](https://github.com/Xpahtalo/Ffxiv2Mqtt/blob/main/ffxiv2Mqtt/Topics/Topic.cs#L18)
if any setup needs to be done with the services. If you send any retained messages that aren't relevant while logged out, clean them
up in [Cleanup()](https://github.com/Xpahtalo/Ffxiv2Mqtt/blob/main/ffxiv2Mqtt/Topics/Interfaces/ICleanable.cs) by sending an empty string to the same topic. 