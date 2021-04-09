using Discord;
using Discord.Audio;
using Discord.WebSocket;
using DiscordAudioStream;
using DiscordAudioStream.Views;
using NAudio.Utils;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AudioProcessing;
using System.Linq;

namespace DiscordAudioStreamService
{

    public enum AudioAPI
    {
        MME,
        MME_Event,
        WASAPI_Exclusive,
        WASAPI_Shared,
        WASAPI_Loopback
    }


    public class AudioStreamingService
    {



        // discord 
        private DiscordSocketClient discordSocketClient = null;
        private string discordBotToken;
        private ulong currentServerId = 0;
        private ulong currentVoiceChannelId = 0;
        private bool connected = false;
        private bool disconnecting = false;
        //private ConcurrentDictionary<string, ulong> serversUIDMap = new ConcurrentDictionary<string, ulong>();
        //private ConcurrentDictionary<string, ulong> currentServerVoiceChannelsUIDMap = new ConcurrentDictionary<string, ulong>();

        private bool autoReconnectChannel;

        private AudioOutStream audioOutStream = null;
        private int audioBitrate;
        private AudioApplication audioContent;
        private int packetLoss;
        private int streamingBufferDuration;
        private byte[] audioTxBuffer;

        // audio 
        AudioAPI audioCaptureAPI = AudioAPI.MME;
        private IWaveIn audioIn = null;
        private int captureBufferDuration;
        private string currentAudioDevice = "";
        private byte[] audioVisBuffer;
        private volatile bool initialised = false;

        // audio buffers
        byte[] txBuffer = null;
        int txBufferUsage = 0;
        float[] monoConversionBuffer = null;
        int monoConversionBufferUsage = 0;
        float[] audioProcessingBuffer = null;
        int audioProcessingBufferUsage = 0;

        // gain management
        private object gainUpdateLock;
        private bool muted;
        private float gain;


        // views
        private IDiscordConnectionView _discordConnectionView;
        private IAudioCaptureView _audioCaptureView;


        //
        public AudioStreamingService() : this(null, null)
        {
        }

        public AudioStreamingService(IDiscordConnectionView discordConnectionView, IAudioCaptureView audioCaptureView)
        {
             // ask NAudio to send buffers by 40ms block
             captureBufferDuration = 40;
            streamingBufferDuration = 1000;
            // 96kbps (high quality) 
            // Opus supports bitrate from 6kbps to 510kbps
            // Default channel bitrate at the time this is being written is 64k 
            // you need to change the channel setting to go to 96k
            // Discord won't go above 128kbps for boosted servers and only 96kbps for standard
            // According to Opus the quality increase from 96k to 128k is marginal anyway
            // https://opus-codec.org/comparison/
            // quality issue is most likely due to packet loss / jittering and discord 
            // putting forward low latency vs quality
            audioBitrate = 128 * 1024;
            packetLoss = 20;
            audioContent = AudioApplication.Music;
            // allocate buffer large enough for 100ms at 48khz 16bits stereo 
            audioTxBuffer = new byte[19200];
            audioVisBuffer = new byte[19200];

            autoReconnectChannel = false;

            gainUpdateLock = new object();
            muted = false;
            gain = 1.0f;

            DiscordConnectionView = discordConnectionView;
            AudioCaptureView = audioCaptureView;
            discordSocketClient = new DiscordSocketClient(
                new DiscordSocketConfig { LogLevel = LogSeverity.Verbose }                
                );
            discordSocketClient.Log += LogAsync;
            discordSocketClient.Connected += DiscordSocketClient_Connected;
            discordSocketClient.Disconnected += DiscordSocketClient_Disconnected;
            discordSocketClient.Ready += DiscordSocketClient_Ready;
            discordSocketClient.LoggedIn += DiscordSocketClient_LoggedIn;
            discordSocketClient.LoggedOut += DiscordSocketClient_LoggedOut;
            discordSocketClient.VoiceServerUpdated += DiscordSocketClient_VoiceServerUpdated;
            discordSocketClient.UserVoiceStateUpdated += DiscordSocketClient_UserVoiceStateUpdated;

        }

        public static AudioAPI ParseAudioAPI(string api)
        {
            if (api == "MME") return AudioAPI.MME;
            else if (api == "MME event") return AudioAPI.MME;
            else if (api == "WASAPI shared") return AudioAPI.WASAPI_Shared;
            else if (api == "WASAPI exclusive") return AudioAPI.WASAPI_Exclusive;
            else if (api == "WASAPI loopback") return AudioAPI.WASAPI_Loopback;
            else return AudioAPI.MME;
        }

        private Task DiscordSocketClient_UserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
        {
            if ((arg2.VoiceChannel == null ) && (arg3.VoiceChannel != null))
                Log("User " + arg1 + " connected  to voice channel " + arg3.VoiceChannel.Name);
            else if ((arg2.VoiceChannel != null) && (arg3.VoiceChannel == null))
                Log("User " + arg1 + " left voice channel " + arg2.VoiceChannel.Name);
            else
                Log("User " + arg1 + " voice state updated");

            return Task.CompletedTask;
        }

        private Task DiscordSocketClient_VoiceServerUpdated(SocketVoiceServer arg)
        {
            Log("Voice server updated " + arg?.Guild.Value?.Name ) ;
            return Task.CompletedTask;
        }

        private Task DiscordSocketClient_LoggedOut()
        {
            Log("Logged out");

            if (DiscordConnectionView != null)
            {
                DiscordConnectionView.Status = "Logged out";
                DiscordConnectionView.StatusColour = StatusColourCode.Red;
            }

            return Task.CompletedTask;
        }

        private Task DiscordSocketClient_LoggedIn()
        {
            Log("Logged in");
            if (DiscordConnectionView != null)
            {
                DiscordConnectionView.Status = "Logged in";
                DiscordConnectionView.StatusColour = StatusColourCode.Green;
            }
            return Task.CompletedTask;
        }

        public void Initialize()
        {
            UpdateAudioDevicesList();
            ConnectAsync().ConfigureAwait(false);
            initialised = true;
        }
        public async void Terminate()
        {
            initialised = false;
            DiscordConnectionView = null;
            AudioCaptureView = null;
            StopCapture();


            Task task = DisconnectAsync(true);

            if (await Task.WhenAny(task, Task.Delay(3000)) == task)
            {
                Log("Disconnected in timely fashion");
            }
            else
            {
                Log("Disconnection timed out exiting.");
            }

        }

        public IAudioCaptureView AudioCaptureView
        {
            get => _audioCaptureView;
            set
            {
                _audioCaptureView = value;
                if (_audioCaptureView != null) _audioCaptureView.SelectedAudioDeviceIDChanged += AudioCaptureView_SelectedDeviceChanged;
                if (_audioCaptureView != null) _audioCaptureView.Muted += _audioCaptureView_Muted;
                if (_audioCaptureView != null) _audioCaptureView.GainChanged += _audioCaptureView_GainChanged;
            }
        }

        private void _audioCaptureView_GainChanged(object sender, float e)
        {
            lock (gainUpdateLock)
            {
                gain = (float) AudioTools.dBFSToLinear(e);
            }
        }

        private void _audioCaptureView_Muted(object sender, bool e)
        {
            lock (gainUpdateLock)
            {
                muted = e;
            }
        }

        private void AudioCaptureView_SelectedDeviceChanged(object sender, string newAudioDeviceName)
        {
            Log("Changing audio device to " + newAudioDeviceName);
            if (currentAudioDevice != newAudioDeviceName)
            {
                StopCapture();
                currentAudioDevice = newAudioDeviceName;
                StartCapture();
            }
        }

        public IDiscordConnectionView DiscordConnectionView
        {
            get => _discordConnectionView;
            set
            {
                _discordConnectionView = value;
                if (_discordConnectionView != null) _discordConnectionView.CurrentServerChanged += DiscordConnectionView_CurrentServerChanged;
                if (_discordConnectionView != null) _discordConnectionView.CurrentVoiceChannelChanged += DiscordConnectionView_CurrentVoiceChannelChanged;
            }
        }

        public bool Connected { get => connected; set => connected = value; }
        public int CaptureBufferDuration { get => captureBufferDuration; set => captureBufferDuration = value; }
        public string DiscordBotToken { get => discordBotToken; set => discordBotToken = value; }
        public int AudioBitrate { get => audioBitrate; set => audioBitrate = value; }
        public string AudioContent 
        { get 
            {
                if (audioContent == AudioApplication.Voice) return "Voice";
                else if (audioContent == AudioApplication.Music) return "Music";
                else return "Mixed";
            } 
            set 
            {
                if (value.ToLower().Equals("voice")) audioContent = AudioApplication.Voice;
                else if (value.ToLower().Equals("music")) audioContent = AudioApplication.Music;
                else audioContent = AudioApplication.Mixed; 
            } 
        }

        public AudioAPI AudioCaptureAPI
        {
            get
            {
                return audioCaptureAPI;
            }
            set
            {
                audioCaptureAPI = value;
            }
        }

        public int PacketLoss { get => packetLoss; set => packetLoss = value; }
        public int StreamingBufferDuration { get => streamingBufferDuration; set => streamingBufferDuration = value; }

        private void DiscordConnectionView_CurrentVoiceChannelChanged(object sender, ulong newVoiceChannel)
        {
            ChangeVoiceChannel(newVoiceChannel);
        }

        public async void ConnectVoiceChannel(ulong newVoiceChannel)
        {
            if (newVoiceChannel != 0)
            {
                try
                {
                    currentVoiceChannelId = newVoiceChannel;
                    if (currentVoiceChannel != null)
                        await currentVoiceChannel.ConnectAsync();
                    Log("Stream created for " + newVoiceChannel);
                }
                catch (Exception e)
                {
                    Log("Exception raised while joining channel " + e.Message);
                }
            }

            //IAudioClient audioClient;

            if (currentServer != null)
            {
                if (currentServer.AudioClient != null)
                {
                    //audioClient = currentServer.AudioClient;
                    AudioOutStream stream = null;
                    try
                    {
                        stream = currentServer.AudioClient.CreatePCMStream(audioContent, audioBitrate, streamingBufferDuration, packetLoss);
                    }
                    catch(Exception e)
                    {
                        Log("Failed to create AudioClient: " +e.Message );

                    }
                        //var stream = client.CreatePCMStream(AudioApplication.Voice ,128000 );
                    if (stream == null)
                    {
                        Log("Unable to create output stream");
                        return;
                    }

                    currentServer.AudioClient.Connected += AudioClient_Connected;
                    currentServer.AudioClient.Disconnected += AudioClient_Disconnected;
                    currentServer.AudioClient.StreamCreated += AudioClient_StreamCreated;
                    currentServer.AudioClient.StreamDestroyed += AudioClient_StreamDestroyed;
                    audioOutStream = (AudioOutStream)stream;


                }
                else
                {
                    Log("Audioclient not connected");
                }
                currentVoiceChannelId = newVoiceChannel;
            }
        }

        private Task AudioClient_StreamDestroyed(ulong arg)
        {
            Log("Stream destroyed " + arg);
            return Task.CompletedTask;
        }

        private Task AudioClient_StreamCreated(ulong arg1, AudioInStream arg2)
        {
            Log("Stream created " + arg1  );
            return Task.CompletedTask;
        }

        public void DisconnectVoiceChannel()
        {
            disconnecting = true;
            if (currentVoiceChannel != null)
            {
                if (audioOutStream != null)
                {
                    lock (audioOutStream)
                    {
                        try
                        {
                            //audioOutStream.ClearAsync(new System.Threading.CancellationToken()).GetAwaiter().GetResult();
                            audioOutStream.Dispose();
                            audioOutStream = null;
                        }
                        catch (Exception disEx)
                        {
                            Log("Exception raised disposing stream " + disEx.Message);
                        }
                    }
                }
                try
                {
                     currentVoiceChannel.DisconnectAsync();
                }
                catch (Exception e)
                {
                    Log("Exception raised while leaving channel " + e.Message);
                }
            }

            if (DiscordConnectionView != null)
            {
                DiscordConnectionView.Status = "Idle";
                DiscordConnectionView.StatusColour = StatusColourCode.Green;
            }
            disconnecting = false;

        }

        private SocketGuild currentServer
        {
            get
            {
                if (currentServerId == 0) return null;
                return this.discordSocketClient.GetGuild(currentServerId) ;
            }
        }

        private SocketVoiceChannel currentVoiceChannel
        {
            get
            {
                if (currentServer == null) return null;
                if ((currentServerId == 0) || (currentVoiceChannelId == 0)) return null;
                return currentServer.GetVoiceChannel(currentVoiceChannelId); ;
            }
        }

        public bool AutoReconnectChannel { get => autoReconnectChannel; set => autoReconnectChannel = value; }

        public async void ChangeVoiceChannel(ulong newVoiceChannel)
        {

            if (newVoiceChannel == currentVoiceChannelId)
                return;

            DisconnectVoiceChannel();
            ConnectVoiceChannel(newVoiceChannel);
        }

        public async void ReconnectVoiceChannel()
        {
            if ((currentVoiceChannelId != 0) && (currentServerId != 0) )
            {
                Log("Attempting reconnection to " + currentServerId + " / " + currentVoiceChannelId);
                DisconnectVoiceChannel();
                ConnectVoiceChannel(currentVoiceChannelId);
                //Log("Reconnected to " + currentServerName + " / " + currentVoiceChannelName);
            }
        }

        private Task AudioClient_Connected()
        {
            Log("Audioclient connected");
            return Task.CompletedTask;
        }

        private Task AudioClient_Disconnected(Exception arg)
        {
            if (arg != null)
            {
                Log("Audioclient disconnected with exception: " + arg.Message);

                try
                {
                    if (!disconnecting && autoReconnectChannel)
                    {
                        Log("Testing for voice channel reconnection in 3 seconds");
                        Task.Run(action: async () =>
                        {
                            await Task.Delay(3000);
                            if (Connected)
                            {
                                if (currentServer != null)
                                {
                                    if (currentServer.AudioClient != null)
                                    {
                                        if (
                                        (currentServer.AudioClient.ConnectionState != ConnectionState.Connected) ||
                                        (currentServer.AudioClient.ConnectionState != ConnectionState.Connecting)
                                        )
                                        { 
                                            ReconnectVoiceChannel();
                                        }
                                    }
                                }
                            }
                        });
                    }
                    //else
                    //{
                    //    DisconnectVoiceChannel();
                    //}

                    //DisconnectVoiceChannel();
                    //Task.Run(action: async () =>
                    //{
                    //    Log("Attempting full reconnection in 3 seconds");
                    //    await Task.Delay(3000);
                    //    await DisconnectAsync();
                    //    await ConnectAsync();
                    //});


                }
                catch (Exception e)
                {
                    Log("Exception disconnecting from channel: " + e.Message);
                }

            }
            else
            {
                Log("Audioclient disconnected");
            }
            return Task.CompletedTask;
        }

        private void DiscordConnectionView_CurrentServerChanged(object sender, ulong newServer)
        {
            ChangeServer(newServer);
        }

        public void ChangeServer(ulong newServer)
        {
            Log("CurrentServerChanged to : " + newServer);
            if (currentServerId != newServer)
            {
                if (newServer != 0)
                {
                    ChangeVoiceChannel(0);
                    currentServerId = newServer;
                }
                else
                {
                    //currentServer = null;
                    currentServerId = 0;
                }
                UpdateVoiceChannels(currentServerId);
            }
        }

        private Task DiscordSocketClient_Disconnected(Exception arg)
        {
            Log("Disconnected from discord");
            if (DiscordConnectionView != null)
            {
                DiscordConnectionView.Status = "Disconnected from discord";
                DiscordConnectionView.StatusColour = StatusColourCode.Red;
            }
            this.Connected = false;
            //UpdateServers();
            // disable controls instead
            return Task.CompletedTask;
        }

        private Task DiscordSocketClient_Connected()
        {
            Log("Connected to discord");
            this.Connected = true;
            if (DiscordConnectionView != null) DiscordConnectionView.Status = "Connected to discord";
            return Task.CompletedTask;
        }

        private Task DiscordSocketClient_Ready()
        {
            Log("Discord client ready");
            UpdateServers();

            // re enable controls

            if ((currentServerId != 0 ) && (currentVoiceChannelId != 0))
            {
                ReconnectVoiceChannel();
            }

            if (DiscordConnectionView != null)
            {
                DiscordConnectionView.Status = "Idle";
                DiscordConnectionView.StatusColour = StatusColourCode.Green;
            }
            return Task.CompletedTask;
        }

        public async Task ConnectAsync()
        {

            if ((discordBotToken == "") || (discordBotToken == null))
            {
                if (_discordConnectionView != null)
                {
                    //_discordConnectionView.DisplayErrorMessage("Empty discord bot key. Get a bot key at https://discordapp.com/developers/applications/ and assign in the serrings menu.");
                    if (DiscordConnectionView != null)
                    {
                        Log("Empty discord bot token. Get a bot token at https://discordapp.com/developers/applications/");
                        DiscordConnectionView.Status = "Empty discord bot token. Update token in settings and restart.";
                        DiscordConnectionView.StatusColour = StatusColourCode.Red;
                    }
                    return;
                }
            }
            DiscordConnectionView.StatusColour = StatusColourCode.Orange;

            if (DiscordConnectionView != null) DiscordConnectionView.Status = "Connecting to discord";
            await discordSocketClient.LoginAsync(TokenType.Bot, discordBotToken);
            await discordSocketClient.StartAsync();
        }

        public async Task DisconnectAsync(bool force = false)
        {
            if (DiscordConnectionView != null) DiscordConnectionView.Status = "Disconnecting from discord";
            if (Connected || force)
            {
                // review why calling lougout is blocking on exit
                if (!force)
                {
                    try
                    {
                        if (currentVoiceChannel != null)
                        {
                            await currentVoiceChannel.DisconnectAsync();
                            await discordSocketClient.LogoutAsync();
                        }
                    }
                    catch (Exception e)
                    {
                        Log("Exception raised while dicsonnecting " + currentVoiceChannelId + " " + e.Message);
                    }
                }
                await discordSocketClient.StopAsync();
            }

        }

        void UpdateAudioDevicesList()
        {

            List<AudioCaptureDeviceInfo> list = new List<AudioCaptureDeviceInfo>(WaveIn.DeviceCount + 2);
            list.Insert(0, new AudioCaptureDeviceInfo("",""));

            if ((audioCaptureAPI == AudioAPI.MME) || (audioCaptureAPI == AudioAPI.MME_Event))
            {
                for (int i = -1; i < WaveIn.DeviceCount; i++)
                {
                    list.Insert(i + 2, new AudioCaptureDeviceInfo(WaveIn.GetCapabilities(i).ProductName, ""+i) );
                }
            }
            else if ((audioCaptureAPI == AudioAPI.WASAPI_Exclusive ) || (audioCaptureAPI == AudioAPI.WASAPI_Shared))
            {
                MMDeviceEnumerator deviceEnum = new MMDeviceEnumerator();
                MMDeviceCollection devices = deviceEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
                foreach (MMDevice device in devices)
                {
                    list.Add(new AudioCaptureDeviceInfo(device.FriendlyName, device.ID.ToString()) );
                }

            }
            else if (audioCaptureAPI == AudioAPI.WASAPI_Loopback)
            {
                MMDeviceEnumerator deviceEnum = new MMDeviceEnumerator();
                MMDeviceCollection devices = deviceEnum.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                foreach (MMDevice device in devices)
                {
                    list.Add(new AudioCaptureDeviceInfo(device.FriendlyName, device.ID.ToString()));
                }
            }

            if (AudioCaptureView != null) AudioCaptureView.AudioDevices = list;

        }

        void UpdateServers()
        {

            List< DiscordServerInfo> serverList = new List<DiscordServerInfo>();
            
            serverList.Add(new DiscordServerInfo("",0));

            if (Connected)
            {
                foreach (SocketGuild server in discordSocketClient.Guilds)
                {
                    if (server.Name != null)
                    {
                        serverList.Add(new DiscordServerInfo(server.Name, server.Id));
                    }
                }
            }

            if (DiscordConnectionView != null) this.DiscordConnectionView.Servers = serverList;
        }

        void UpdateVoiceChannels(ulong serverID)
        {
            //if (discordSocketClient.ConnectionState != ConnectionState.Connected) return;
            List<DiscordVoiceChannelInfo> list = new List<DiscordVoiceChannelInfo>();
            list.Add(new DiscordVoiceChannelInfo("",0,-1,null));
            if (serverID != 0)
            {
                SocketGuild server = discordSocketClient.GetGuild(serverID);
                if (server != null)
                {
                    foreach (SocketVoiceChannel voiceChannel in server.VoiceChannels)
                    {
                        //
                        Log("Channel");
                        Log("Name: " + voiceChannel.Name);
                        Log("ID: " + voiceChannel.Id);
                        Log("Position: " + voiceChannel.Position);
                        Log("Category: " + voiceChannel.Category?.Name);
                        Log("Category Position: " + voiceChannel.Category?.Position);
                        Log("---");

                        list.Add(new DiscordVoiceChannelInfo(voiceChannel.Name,voiceChannel.Id,voiceChannel.Position,voiceChannel.Category?.Name));
                    }
                }
            }
            if (DiscordConnectionView != null) DiscordConnectionView.VoiceChannels = list.OrderBy(n => n.Position).ToList<DiscordVoiceChannelInfo>();

        }


        private void Log(string message)
        {
            Debug.WriteLine(message);
        }

        private Task LogAsync(LogMessage log)
        {
            Log(log.ToString());
            return Task.CompletedTask;
        }

        private IWaveIn CreateCapture()
        {
            IWaveIn iWaveIn = null;

            if (audioCaptureAPI == AudioAPI.MME_Event)
            {
                try
                {
                    int deviceID = -1;
                    WaveInEvent waveInEvent = new WaveInEvent();
                    waveInEvent.WaveFormat = new WaveFormat(48000, 16, 2);
                    waveInEvent.BufferMilliseconds = captureBufferDuration;
                    if (!int.TryParse(currentAudioDevice, out deviceID))
                        deviceID = -1;
                    waveInEvent.DeviceNumber = deviceID;
                    iWaveIn = waveInEvent;
                }
                catch (Exception e)
                {
                    Log("Exception creating MME event capture device" + e.Message);
                }
            }
            else if (audioCaptureAPI == AudioAPI.MME)
            {
                try
                {
                    int deviceID = -1;
                    WaveIn waveIn = new WaveIn();
                    waveIn.WaveFormat = new WaveFormat(48000, 16, 2);
                    waveIn.BufferMilliseconds = captureBufferDuration;
                    if (!int.TryParse(currentAudioDevice, out deviceID))
                        deviceID = -1;
                    waveIn.DeviceNumber = deviceID;
                    iWaveIn = waveIn;
                }
                catch (Exception e)
                {
                    Log("Exception creating MME capture device" + e.Message);
                }
            }
            else if (audioCaptureAPI == AudioAPI.WASAPI_Exclusive)
            {
                MMDeviceEnumerator mmDevEnum = new MMDeviceEnumerator();               
                WasapiCapture wasapiCapture = new WasapiCapture(mmDevEnum.GetDevice(currentAudioDevice));
                wasapiCapture.ShareMode = AudioClientShareMode.Exclusive;
                iWaveIn = wasapiCapture;
            }
            else if (audioCaptureAPI == AudioAPI.WASAPI_Shared)
            {
                MMDeviceEnumerator mmDevEnum = new MMDeviceEnumerator();
                WasapiCapture wasapiCapture = new WasapiCapture(mmDevEnum.GetDevice(currentAudioDevice));
                wasapiCapture.ShareMode = AudioClientShareMode.Shared;
                iWaveIn = wasapiCapture;
            }
            else if(audioCaptureAPI == AudioAPI.WASAPI_Loopback)
            {
                MMDeviceEnumerator mmDevEnum = new MMDeviceEnumerator();
                WasapiLoopbackCapture wasapiCapture = new WasapiLoopbackCapture(mmDevEnum.GetDevice(currentAudioDevice));
                wasapiCapture.ShareMode = AudioClientShareMode.Shared;
                iWaveIn = wasapiCapture;
            }

            return iWaveIn;
        }

        private void StartCapture()
        {
            Log("Starting Capture");

            if (currentAudioDevice == "")
            {
                Log("No device selected");
                return;
            }


            try
            {
                audioIn = CreateCapture();
                if (audioIn == null)
                {
                    Log("No device available for capture");
                    return;
                }

                if ((audioCaptureAPI == AudioAPI.WASAPI_Exclusive) || (audioCaptureAPI == AudioAPI.WASAPI_Shared) || (audioCaptureAPI == AudioAPI.WASAPI_Loopback))
                {
                    if (audioIn.WaveFormat.SampleRate != 48000)
                    {
                        Log("WASAPI capture invalid sampling rate.");
                        AudioCaptureView.DisplayErrorMessage("Invalid sampling rate for WASAPI capture. Set device to 48000Hz or switch MME.");
                        audioIn = null;
                        return;
                    }
                }

                audioIn.DataAvailable += Audioin_DataAvailable;
                audioIn.RecordingStopped += Audioin_RecordingStopped;


                audioTxBuffer = new byte[48 * 4 * captureBufferDuration];
                audioVisBuffer = new byte[48 * 4 * captureBufferDuration];

                audioIn.StartRecording();
                Log("Capture started");
            }
            catch (Exception e)
            {
                if (AudioCaptureView != null)
                    AudioCaptureView.DisplayErrorMessage("Unable to start capture from selected device. Please check settings.");
                Log("Exception raised while starting capture " + e.Message);
            }
        }

        private void StopCapture()
        {
            if (audioIn != null)
            {
                try
                {
                    audioIn.DataAvailable  -= Audioin_DataAvailable;
                    audioIn.StopRecording();
                    ResetVisualisation();
                }
                catch(Exception e)
                {
                    Log("Exception raised while stopping capture " + e.Message);
                }
                finally
                {
                    //audioIn.Dispose();
                    audioIn = null;
                }
            }
        }

        private void ResetVisualisation()
        {
            if ((_audioCaptureView != null) && initialised)
            {
                AudioCaptureView.SetPeak(-144f, -144f);
            }
        }

        private void Audioin_RecordingStopped(object sender, StoppedEventArgs e)
        {
            Log("Capture stopped.");
        }

        private void Audioin_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (!initialised) return;

            int bitdepth = this.audioIn.WaveFormat.BitsPerSample;
            int channels = this.audioIn.WaveFormat.Channels;

            if (channels == 1)
            {
                AudioBufferConverter.ConvertByteArrayToFloatArray(e.Buffer, e.BytesRecorded, bitdepth, ref monoConversionBuffer, ref monoConversionBufferUsage);
                AudioBufferConverter.MonoToStereo<float>(monoConversionBuffer, monoConversionBufferUsage, ref audioProcessingBuffer, ref audioProcessingBufferUsage);
            }
            else if (channels == 2)
            {
                AudioBufferConverter.ConvertByteArrayToFloatArray(e.Buffer, e.BytesRecorded, bitdepth, ref audioProcessingBuffer, ref audioProcessingBufferUsage);
            }

            if ((audioProcessingBuffer == null) || (audioProcessingBufferUsage <= 0)) return;
            // TODO: replace by audio processing filter chain
            for (int i = 0; i < audioProcessingBufferUsage; i++)
            {
                audioProcessingBuffer[i] *= gain;
            }

            if (!muted) TransmitAudioBufferAsync(audioProcessingBuffer, audioProcessingBufferUsage);
            UpdateAudioVisualisationAsync(audioProcessingBuffer, audioProcessingBufferUsage);

        }

        public async void TransmitAudioBufferAsync(float[] buffer, int sampleCount)
        {
            if ((audioOutStream != null) && (sampleCount > 0) && initialised)
            {
                lock (buffer)
                {
                    AudioBufferConverter.ConvertFloatArrayToByteArray(buffer, sampleCount, 16, ref txBuffer, ref txBufferUsage);
                }

                if (DiscordConnectionView != null)
                {
                    try
                    {
                        DiscordConnectionView.Status = "Streaming";
                        DiscordConnectionView.StatusColour = StatusColourCode.Blue;
                    }
                    catch (Exception e)
                    {
                        Log("Failed to update status: " + e.Message);
                    }
                }

                if (audioOutStream != null)
                {
                    lock (audioOutStream)
                    {
                        try
                        {
                            audioOutStream.WriteAsync(txBuffer, 0, txBufferUsage).GetAwaiter().GetResult();
                            audioOutStream.FlushAsync();
                        }
                        catch (System.Exception ex)
                        {
                            Log("Exception raised while sending audio " + ex.Message);
                        }
                        finally
                        {
                        }
                    }
                }
                
            }
        }


        public async void UpdateAudioVisualisationAsync(float[] buffer, int sampleCount)
        {
            if ((_audioCaptureView != null) && initialised)
            {
                float maxLF = 0f;
                float maxRF = 0f;
                lock (buffer)
                {
                    for (int i = 0; i < sampleCount; i+=2)
                    {
                        maxLF = Math.Max(maxLF, Math.Abs(buffer[i]));
                        maxRF = Math.Max(maxRF, Math.Abs(buffer[i+1]));
                    }
                }

                // values in dbfs -infinity to 0 
                float dBFSvalueL = 20.0f * (float)Math.Log10(maxLF);
                float dBFSvalueR = 20.0f * (float)Math.Log10(maxRF);

                if ((maxLF > 1.0) || (maxRF > 1.0)) 
                    Log("Clipping");
                AudioCaptureView.SetPeak(dBFSvalueL, dBFSvalueR);

            }
        }
 
    }

}