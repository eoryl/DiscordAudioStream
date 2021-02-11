# DiscordAudioStream
A simple tool for Windows to stream audio on Discord: play higher quality audio to a voice channel via a secondary Discord bot type connection. The application based on Discord.net and NAudio, and is designed for Windows. It works best in conjunction with a dedicated virtual audio soundcard.

# Setup
* Get latest DiscordAudioStream version here https://github.com/eoryl/DiscordAudioStream/releases
* DiscordAudioStream and its dependicies require .NET 4.6.1 runtime. If you are running on Windows 10 up to date you should have the correct version of .Net runtime. If not or running Windows 7 install .NET 4.6.1 runtime or a later .NET 4 framework. .NET framework 4.6.1 can be found here https://www.microsoft.com/en-us/download/details.aspx?id=49981
* Install Visual C++ 2019 x86 redist from here https://support.microsoft.com/en-us/help/2977003/the-latest-supported-visual-c-downloads
* To avoid using analogue loop (or WASAPI loopback), install a virtual audio cable virtual audio card driver. I tested with this one https://www.vb-audio.com/Cable/
* If using WASAPI capture set the virtual audio card is set to 48khz 16bit stereo in Settings > Sounds > Sound Control Pannel > Recording > Cable output > Properties > Advanced. This is optional but recommended if using MME. You should also use 48khz for playback in Settings > Sounds > Sound Control Pannel > Playback > Cable input > Properties > Advanced, and in your playout application (audio clips and output).This is to avoid unecessary resampling and dithering along the chain because Discord expects 48kHz audio.
* The audio is played by the selected audio applications through the virtual audio cable. Route either in the app itself or using Windows 10 routing interface in Settings > Sounds >  Advanced Sound Options > App volume and device preferences 
* Get a discord bot token at https://discordapp.com/developers/applications (you can find a better in depth tutorial here on how to create a bot https://docs.stillu.cc/guides/getting_started/first-bot.html)
  * Create an application
  * Then in your application go to bot and "Add bot" (tick public if you intended to add it on someone else's server)
  * Add the bot to your server https://discord.com/api/oauth2/authorize?client_id=YOUR_BOT_ID_HERE&permissions=36701184&scope=bot I have tried to limit so necessary permissions only to do audio, just replace YOUR_BOT_ID_HERE with your actual bot client ID
* Run DiscordAudioStream in File > Settings enter your bot token and exit
* Run DiscordAudioStream again the application should connect the status should go to green / idle and the bot connect to the voice channel
* Select the virtual audio cable as a source, the server and the voice channel.
* The meter should move and status go to streaming.
* Run Discord using your regular audio interface.

# Basic configuration
* Audio device: The audio card input you record from (or which output you clone if using WASAPI loopback mode). 
* Server: The Discord server you connect to.
* Voice channel: The voice channel you stream to.

# Advanced configuration
* Bot token: This is your private key for DiscordAudioStream to connect to Discord. Follow the instructions in the Setup section to create one.
* Capture API: This is how the app connects to the driver. MME is the oldest and is more compatible but less performant. WASAPI is more recent and has better performance but often not as well supported by audio device manufacturers. It also does not support sample rate conversion and requires manually setting the device to 48kHz capture sample rate (Discord only support 48KHz and current version of DiscordAudioStream does not embark a sample rate converter).  WASAPI shared mode usually works with most configuration to record from inputs (with Virtual Audio Cable for instance). WASAPI exclusive mode is the most performant and stable (should support lowest latencies) but requires exclusive access to the audio input and may simply not work with some audio drivers. WASAPI looback mode does not record from a card input but duplicates the output of a card (so in theory you could do without the virtual audio cable depending on your audio cards and use case).
* Audio API capture buffer: This is how frequently the app will receive audio from the driver. With a lower value you will get a responsive peak meter but you pontetially increase chances of audio dropouts if your CPU gets a bit busy. 
* Audio codec content type: That's how Discord.net presents Opus encoding parameters for content. It should be self explanatory, but you can get more details from the Opus codec site.   
* Encoder bitrate: This is the audio quality of your stream. 96kbps is the default. 128kbps the highest quality supported.
* Packet loss: This is Opus builtin packet loss management (unsure if this is actually used by Discord) 
* Streaming buffer duration: How much audio is cached in the record buffer before sending. Higher is supposedly more tolerant to network degradation/jitter but less responsive. 

# FAQ 

* *The app won't connect and what is that bot token*

Usually that means you have not set your bot token. See the Setup section for instructions on how to create your bot token. This could also mean your token is already in use by another instance of DiscordAudioStream. Don't run multiple DiscordAudioStream with the same token and don't share your key it's private and should remain secret only known to you.

* *The server I want to connect to is not available in the list*

Once you have created the bot and its token you must add it to your server and grant it right to list, connect and talk on the voice channel. This means somebody with the "manage server" role must add it to the server. See above in setup  

* *Which audio capture API should I use?*

The most compatible mode is probably MME. The most performant is WASAPI but it requires settings the device sample rate to 48kHz (see above).

* *What are compatibility parameters?*

MME capture mode, 100ms capture buffer, Mixed content, 96kbps, 15% packet loss, 1000 ms network buffer. 

* *What are performance parameters?*

WASAPI capture mode, 40ms capture buffer, Music content, 128kbps, 20% packet loss, 200 ms or less network buffer. 
  
* *Why can't I go over 128kbps quality?*

Discord API won't allow it on regular servers. Non boosted servers are in fact defaulted at 64kbps and capped at 96kbps, but stream via the bot connection can go up to 128kbps. Higher bitrate could be possible. But not test have been done sor far. According to xiph.org tests (https://opus-codec.org/comparison/) after 96kbps you are already past the point of dimnishing returns for the Opus codec anyway.

* *Where are the logs?*

There are no file based logs at the moment but you can get some logs with debugview (https://docs.microsoft.com/en-us/sysinternals/downloads/debugview).

* *The app crashes at startup*

Check the setup instructions prerequisites.

* *But I installed them and it was working before!*

Maybe something isn't right in the config. Just delete %APPDATA%\Local\DiscordAudioStream (C:\Users\<username>\AppData\Local\DiscordAudioStream) and reset your bot token and other params.

* *What about Operating Systems support?*

Only tested on up to date Windows 10 64bit. Althought it should work on Windows 10 32bit as the app runs in 32 bit mode. Windows 7 should also be supported as well but is untested. 

# Compiling
Compilation should work without tinkering using visual studio 2019 with .Net desktop development workload. 
Note some nugets and their repsective dependencies are required :
* Discord.net https://github.com/discord-net/Discord.Net
* NAudio https://github.com/naudio/NAudio

Visual studio should resolve all for you.

After compilation add the dependencies dlls (32bit version) opus.dll and libsodium.dll in the bin/[profile] folder

# Alternatives 
* The initial idea from Rollyjumper as bot in Rust https://github.com/Rollyjumper/fp_music_bot
* A similar tool written in Python https://github.com/QiCuiHub/discord-audio-pipe/

