# DiscordAudioStream
A simple audio streaming tool for Discord based on Discord.net and NAudio to play some higher quality audio via a secondary Discord bot type connection on an audio channel. The application works best in conjunction with a dedicated virtual audio soundcard.

# Use cases
It's initial purpose is to be used to play ambiant sound and music for virtual table top sessions on Discord. 

# Compiling
Compilation should work without tinkering using visual studio 2019 with .Net desktop development workload. 
Note some nugets and their repsective dependencies are required :
* Discord.net https://github.com/discord-net/Discord.Net
* NAudio https://github.com/naudio/NAudio

Visual studio should resolve all for you.

After compilation add the dependencies dlls (32bit version) opus.dll and libsodium.dll in the bin/[profile] folder

# Setup
* Get latest DiscordAudioStream version here https://github.com/eoryl/DiscordAudioStream/releases
* Install a virtual audio cable virtual audio card driver. I tested with this one https://www.vb-audio.com/Cable/
* Optionally use 48khz audio in your playiut app and set the virtual audio card is set to 48khz 16bit stereo as well in Settings > Sounds > Sound Control Pannel > Recording > Cable output > Properties > Advanced. This is to avoid unecessary resampling and dithering along the chain.
* The audio is played by the selected audio applications through the virtual audio cable. Route either in the app itself or using Windows 10 routing interface in Settings > Sounds >  Advanced Sound Options > App volume and device preferences 
* Get a discord bot token at https://discordapp.com/developers/applications 
* Run DiscordAudioStream in File > Settings enter your bot token and exit
* Run DiscordAudioStream again the application shoudl connect the status should go to green / idle and the bot connect to the voice channel
* Select the virtual audio cable as a source, the server and the voice channel.
* The meter should move and status go to streaming.
* Run Discord using your regular  audio interface.

# Alternatives 
* The initial idea from Rollyjumper as bot in Rust https://github.com/Rollyjumper/fp_music_bot
* A similar tool written in Python https://github.com/QiCuiHub/discord-audio-pipe/

