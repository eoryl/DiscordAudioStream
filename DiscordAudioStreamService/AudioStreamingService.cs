using Discord;
using Discord.Audio;
using Discord.WebSocket;
using DiscordAudioStream;
using DiscordAudioStream.Views;
using NAudio.Utils;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

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
        private string currentServerName = "";
        private string currentVoiceChannelName = "";
        private bool connected = false;
        private bool disconnecting = false;
        private ConcurrentDictionary<string, ulong> serversUIDMap = new ConcurrentDictionary<string, ulong>();
        private ConcurrentDictionary<string, ulong> currentServerVoiceChannelsUIDMap = new ConcurrentDictionary<string, ulong>();

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

        // 
        private volatile uint lastAudioBufferSentID = 0;
        private volatile uint currentAudioBufferID = 0;

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
        }
        public async void Terminate()
        {
            StopCapture();
            DiscordConnectionView = null;
            AudioCaptureView = null;

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

        private void DiscordConnectionView_CurrentVoiceChannelChanged(object sender, string newVoiceChannel)
        {
            ChangeVoiceChannel(newVoiceChannel);
        }

        public async void ConnectVoiceChannel(string newVoiceChannel)
        {
            if (currentServerVoiceChannelsUIDMap.ContainsKey(newVoiceChannel))
            {
                try
                {
                    currentVoiceChannelName = newVoiceChannel;
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
                    audioOutStream = (AudioOutStream)stream;
                }
                else
                {
                    Log("Audioclient not connected");
                }
                currentVoiceChannelName = newVoiceChannel;
            }
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
                if (!serversUIDMap.ContainsKey( currentServerName)) return null;
                return this.discordSocketClient.GetGuild(serversUIDMap[currentServerName]) ;
            }
        }

        private SocketVoiceChannel currentVoiceChannel
        {
            get
            {
                if (currentServer == null) return null;
                if (!currentServerVoiceChannelsUIDMap.ContainsKey(currentVoiceChannelName)) return null;
                return currentServer.GetVoiceChannel(currentServerVoiceChannelsUIDMap[currentVoiceChannelName]); ;
            }
        }

        public async void ChangeVoiceChannel(string newVoiceChannel)
        {
            if (newVoiceChannel == null)
                newVoiceChannel = "";

            if (newVoiceChannel == currentVoiceChannelName)
                return;

            DisconnectVoiceChannel();
            ConnectVoiceChannel(newVoiceChannel);
        }

        public async void ReconnectVoiceChannel()
        {
            if ((currentVoiceChannelName != "") && (currentServerName != "") )
            {
                Log("Attempting reconnection to " + currentServerName + " / " + currentVoiceChannelName);
                DisconnectVoiceChannel();
                ConnectVoiceChannel(currentVoiceChannelName);
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
                    if (!disconnecting)
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
                    Log("Exception disconnecting from channel");
                }

            }
            else
            {
                Log("Audioclient disconnected");
            }
            return Task.CompletedTask;
        }

        private void DiscordConnectionView_CurrentServerChanged(object sender, string newServerName)
        {
            ChangeServer(newServerName);
        }

        public void ChangeServer(string newServerName)
        {
            Log("CurrentServerChanged to : " + newServerName);
            if (currentServerName != newServerName)
            {
                if (serversUIDMap.ContainsKey(newServerName))
                {
                    //currentServer = discordSocketClient.GetGuild(serversUIDMap[newServerName]);
                    ChangeVoiceChannel("");
                    currentServerName = newServerName;
                }
                else
                {
                    //currentServer = null;
                    currentServerName = "";
                }
                UpdateVoiceChannels(currentServerName);
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

            if ((currentServerName != "" ) && (currentVoiceChannelName != ""))
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
                        Log("Exception raised while dicsonnecting " + currentVoiceChannelName + " " + e.Message);
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

            List<string> list = new List<string>();
            serversUIDMap.Clear();

            if (Connected)
            {
                foreach (SocketGuild server in discordSocketClient.Guilds)
                {
                    if (server.Name != null) serversUIDMap[server.Name] = server.Id;
                }
            }

            list.Add("");
            list.AddRange(serversUIDMap.Keys);
            if (DiscordConnectionView != null) this.DiscordConnectionView.Servers = list;
        }

        void UpdateVoiceChannels(string serverName)
        {
            //if (discordSocketClient.ConnectionState != ConnectionState.Connected) return;

            List<string> list = new List<string>();
            list.Add("");
            if (serverName != "")
            {
                if (serversUIDMap.ContainsKey(serverName))
                {
                    SocketGuild server = discordSocketClient.GetGuild(serversUIDMap[serverName]);
                    if (server != null)
                    {
                        foreach (SocketVoiceChannel voiceChannel in server.VoiceChannels)
                        {
                            currentServerVoiceChannelsUIDMap[voiceChannel.Name] = voiceChannel.Id;
                            list.Add(voiceChannel.Name);
                        }
                    }
                }
            }
            if (DiscordConnectionView != null) DiscordConnectionView.VoiceChannels = list.ToArray();

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
                //wasapiCapture.WaveFormat = new WaveFormat(48000, 16, 2);
                iWaveIn = wasapiCapture;
            }
            else if (audioCaptureAPI == AudioAPI.WASAPI_Shared)
            {
                MMDeviceEnumerator mmDevEnum = new MMDeviceEnumerator();
                WasapiCapture wasapiCapture = new WasapiCapture(mmDevEnum.GetDevice(currentAudioDevice));
                wasapiCapture.ShareMode = AudioClientShareMode.Shared;
                //wasapiCapture.WaveFormat = new WaveFormat(48000, 16, 2);
                iWaveIn = wasapiCapture;
            }
            else if(audioCaptureAPI == AudioAPI.WASAPI_Loopback)
            {
                MMDeviceEnumerator mmDevEnum = new MMDeviceEnumerator();
                WasapiLoopbackCapture wasapiCapture = new WasapiLoopbackCapture(mmDevEnum.GetDevice(currentAudioDevice));
                wasapiCapture.ShareMode = AudioClientShareMode.Shared;
                //wasapiCapture.WaveFormat = new WaveFormat(48000, 16, 2);
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
                    audioIn.DataAvailable -= Audioin_DataAvailable;
                    audioIn.StopRecording();

                    UpdateAudioVisualisationAsync(new byte[] { 0 }, 1);
                    //circularBuffer.Reset();
                }
                finally
                {
                    //audioIn.Dispose();
                    audioIn = null;
                }
            }
        }

        private void Audioin_RecordingStopped(object sender, StoppedEventArgs e)
        {
            Log("Capture stopped.");
            //throw new System.NotImplementedException();
        }

        private void Audioin_DataAvailable(object sender, WaveInEventArgs e )
        {
            // parallelise in two threads encoding+tx and visualisation
            currentAudioBufferID += 1;


            int bitdepth = this.audioIn.WaveFormat.BitsPerSample;
            int channels = this.audioIn.WaveFormat.Channels;
            byte[] buffer = null;
            int bufferlen = 0;

            if (!((channels == 1) || (channels == 2)))
                return;

            // convert to correct bitdepth if needed
            if (bitdepth == 16)
            {
                buffer = e.Buffer;
                bufferlen = e.BytesRecorded;
            }
            else if ((bitdepth == 32) || (bitdepth == 24) || (bitdepth == 8))
            {
                short[] buffer16 = null;
                int buffer16len = 0;
                AudioBufferConverter.ConvertBitDepthTo16bit(e.Buffer, e.BytesRecorded, bitdepth, ref buffer16, ref buffer16len);
                AudioBufferConverter.ConvertShortArrayToByte(buffer16, buffer16len, ref buffer, ref bufferlen);
            }

            if ((buffer == null) || (bufferlen <= 0)) return;

            // mono to stereo if needed
            if (channels == 1)
            {
                byte[] bufferstereo = null;
                int bufferstereolen = 0;
                AudioBufferConverter.MonoToStereo(buffer, bufferlen, ref bufferstereo, ref bufferstereolen);
                buffer = bufferstereo;
                bufferlen = bufferstereolen;
            }

               
            TransmitAudioBufferAsync(buffer, bufferlen, currentAudioBufferID);
            UpdateAudioVisualisationAsync(buffer, bufferlen);
        }

        public async void TransmitAudioBufferAsync(byte [] buffer, int byteCount, uint bufferID)
        {
            if ((audioOutStream != null) && byteCount > 0)
            {
                lock (audioTxBuffer)
                {
                    lock (buffer)
                    {
                        if (audioTxBuffer.Length < byteCount)
                            audioTxBuffer = new byte[byteCount];
                        //System.Buffer.BlockCopy(buffer, 0, audioVisBuffer, 0, byteCount);
                        Array.Copy(buffer, audioTxBuffer, byteCount);
                    }

                    if (DiscordConnectionView != null)
                    {
                        try
                        {
                            DiscordConnectionView.Status = "Streaming";
                            DiscordConnectionView.StatusColour = StatusColourCode.Blue;
                        }
                        catch(Exception e)
                        {
                            Log("Failed to update status: " + e.Message);
                        }
                    }
                    if (byteCount % 3840 != 0)
                        Log($"Audio buffer is {byteCount} bytes. This will result in partial frame.");
                    // lock the audioOutStreeam to prevent different threads to call 
                    // the opus encoder encode mehtod that does not seem to be thread safe
                    // consider replacing by a "SemaphoreQueue" type lock to make sure 
                    // worker threads send audio packet in order of arrival
                    if (audioOutStream != null)
                    {
                        lock (audioOutStream)
                        {
                            try
                            {
                                if (lastAudioBufferSentID != bufferID - 1)
                                    Debug.WriteLine($"Buffer ID out of squence by {lastAudioBufferSentID + 1 - bufferID }");
                                audioOutStream.WriteAsync(audioTxBuffer, 0, byteCount).GetAwaiter().GetResult();
                                lastAudioBufferSentID = bufferID;
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
        }


        public async void UpdateAudioVisualisationAsync(byte[] buffer, int byteCount)
        {
            if (_audioCaptureView != null)
            {
                short maxL = 0, maxR = 0;
                lock (audioVisBuffer)
                {
                    lock (buffer)
                    {
                        if (audioVisBuffer.Length < byteCount)
                            audioVisBuffer = new byte[byteCount];
                        //System.Buffer.BlockCopy(buffer, 0, audioVisBuffer, 0, byteCount);
                        Array.Copy(buffer, audioVisBuffer, byteCount);
                    }

                    // prevent overflow on trailing incomplete samples
                    if (byteCount % 4 != 0) byteCount -= byteCount % 4;
                    for (int sampleIndex = 0; sampleIndex < byteCount; sampleIndex += 4)
                    {

                        short sampleL = (short)((audioVisBuffer[sampleIndex + 1] << 8) | audioVisBuffer[sampleIndex + 0]);
                        // is Math.Abs implementation faster ?
                        // workaround add 1 to negative to avoid overflow if value is -32768
                        if (sampleL < 0) sampleL = (short)(1 - sampleL);
                        if (sampleL > maxL) maxL = sampleL;

                        short sampleR = (short)((audioVisBuffer[sampleIndex + 3] << 8) | audioVisBuffer[sampleIndex + 2]);
                        //sampleR = Math.Abs(sampleR);
                        if (sampleR < 0) sampleR = (short)(1 - sampleR);
                        if (sampleR > maxR) maxR = sampleR;

                    }
                }
                // max samples as float 
                float maxLF = maxL / 32767f;
                float maxRF = maxR / 32767f;

                // values in dbfs -infinity to 0 
                float dBFSvalueL = 20.0f * (float)Math.Log10(maxLF);
                float dBFSvalueR = 20.0f * (float)Math.Log10(maxRF);
                AudioCaptureView.SetPeak(dBFSvalueL, dBFSvalueR);

            }
        }
    }

}