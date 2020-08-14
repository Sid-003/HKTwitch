# HollowTwitch

Go to https://twitchapps.com/tmi/ and get your oauth token in the form oauth:token.  
Place your token, username, and channel (Note: the channel and username are all lowercase) within TwitchMod.GlobalSettings.json in your Hollow Knight saves folder.  
If the file does not exist, run the game with TwitchMod installed and quit the game using the menus to generate it. 

If you are using Bilibili, change the Client to "Bilibili" instead of "Twitch". 
You'll then need to set your room ID based on the URL of your stream. For example, if your stream url is 'https://live.bilibili.com/22102251?visit_id=7t294f4lbb40', then your room ID is 22102251.

Once the game has been ran, a file called TwitchCommandList.txt will be in your Mods folder containing the commands, their cooldowns, and descriptions.

You can modify cooldowns within the config under "Cooldowns".

Admin users are able to bypass blacklisted commands and cooldowns.

The config should look something like this:

```json
{
  "Client": "Twitch",
  "BilibiliRoomID": "your_room_id_goes_here",
  "TwitchToken": "your_token_goes_here",
  "TwitchUsername": "twitchchannel",
  "TwitchChannel": "twitchchannel",
  "Prefix": "!",
  "BlacklistedCommands": [],
  "AdminUsers": [],
  "BannedUsers": [],
  "Cooldowns": {
	...
  },
  "BoolValues": {},
  "FloatValues": {},
  "IntValues": {},
  "StringValues": {}
}
```
