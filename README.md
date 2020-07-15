# HollowTwitch

Go to https://twitchapps.com/tmi/ and get your oauth token in the form oauth:token.  
Place your token, username, and channel (Note: the channel and username are all lowercase) within TwitchMod.GlobalSettings.json in your Hollow Knight saves folder.  
If the file does not exist, run the game with TwitchMod installed and quit the game using the menus to generate it. 

Once the game has been ran, a file called TwitchCommandList.txt will be in your Mods folder containing the commands, their cooldowns, and descriptions.

You can modify cooldowns within the config under "Cooldowns".

Admin users are able to bypass blacklisted commands and cooldowns.

The config should look something like this:

```json
{
  "Token": "your_token_goes_here",
  "Username": "twitchchannel",
  "Channel": "twitchchannel",
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
