using Discord;
using Discord.Audio;
using Discord.WebSocket;
using DiscordAudioStream;
using DiscordAudioStream.Views;
using NAudio.Utils;
using NAudio.Wave;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DiscordAudioStreamService
{


    public class AudioStreamingService
    {


        // discord 
        private DiscordSocketClient discordSocketClient = null;
        private string discordBotKey;
        private string currentServerName = "";
        private string currentVoiceChannelName = "";
        private bool connected = false;
        private ConcurrentDictionary<string, ulong> serversUIDMap = new ConcurrentDictionary<string, ulong>();
        private ConcurrentDictionary<string, ulong> currentServerVoiceChannelsUIDMap = new ConcurrentDictionary<string, ulong>();

        private SocketGuild currentServer = null;
        private SocketVoiceChannel currentVoiceChannel = null;
        private AudioOutStream audioOutStream = null;
        private int audioBitrate;
        private AudioApplication audioContent;
        private int packetLoss;

        // audio 
        private WaveInEvent audioIn = null;
        private int captureBufferDuration;
        private string currentAudioDeviceName = "";
        private ConcurrentDictionary<string, int> audioDeviceNameToIDMap = new ConcurrentDictionary<string, int>();
        private CircularBuffer circularBuffer;
        private int circularBufferSize;

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
            // N seconds of 48khz 16 bits stereo 
            circularBufferSize = 4 * 48000 * 3;
            // 96kbps (high quality) 
            // Opus supports bitrate from 6kbps to 510kbps
            // Discord won't go aboev 128kbps
            audioBitrate = 96000;
            packetLoss = 30;
            audioContent = AudioApplication.Music;
            DiscordConnectionView = discordConnectionView;
            AudioCaptureView = audioCaptureView;
            discordSocketClient = new DiscordSocketClient();
            discordSocketClient.Log += LogAsync;
            discordSocketClient.Connected += DiscordSocketClient_Connected;
            discordSocketClient.Disconnected += DiscordSocketClient_Disconnected;
            discordSocketClient.Ready += DiscordSocketClient_Ready;
        }

        public void Initialize()
        {
            UpdateAudioDevicesList();
            ConnectAsync().ConfigureAwait(false);
        }
        public async void Terminate()
        {
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
                if (_audioCaptureView != null) _audioCaptureView.SelectedAudioDeviceChanged += AudioCaptureView_SelectedDeviceChanged;
            }
        }

        private void AudioCaptureView_SelectedDeviceChanged(object sender, string newAudioDeviceName)
        {
            Log("Changing audio device to " + newAudioDeviceName);
            if (currentAudioDeviceName != newAudioDeviceName)
            {
                StopCapture();
                currentAudioDeviceName = newAudioDeviceName;
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
        public string DiscordBotKey { get => discordBotKey; set => discordBotKey = value; }
        public int CircularBufferSize { get => circularBufferSize; set => circularBufferSize = value; }
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

        public int PacketLoss { get => packetLoss; set => packetLoss = value; }

        private void DiscordConnectionView_CurrentVoiceChannelChanged(object sender, string newVoiceChannel)
        {
            ChangeVoiceChannel(newVoiceChannel);
        }

        public async void ChangeVoiceChannel(string newVoiceChannel)
        {
            if (newVoiceChannel == null)
                newVoiceChannel = "";

            if (newVoiceChannel == currentVoiceChannelName)
                return;

            if (DiscordConnectionView != null)
                DiscordConnectionView.Status = "Idle";


            if (currentVoiceChannel != null)
            {
                if (audioOutStream != null)
                {
                    lock (audioOutStream)
                    {
                        //audioOutStream.ClearAsync(new System.Threading.CancellationToken()).GetAwaiter().GetResult();
                        audioOutStream.Dispose();
                        audioOutStream = null;
                    }
                }
                try
                { 
                    await currentVoiceChannel.DisconnectAsync();
                }
                catch (Exception e)
                {
                    Log("Excaption raised while leaving channel " + e.Message);
                }
            }

            if (currentServerVoiceChannelsUIDMap.ContainsKey(newVoiceChannel))
            {
                currentVoiceChannel = currentServer.GetVoiceChannel(currentServerVoiceChannelsUIDMap[newVoiceChannel]);
                await currentVoiceChannel.ConnectAsync();
            }

            IAudioClient client;

            if (currentServer.AudioClient != null)
            {
                client = currentServer.AudioClient;
                var stream = client.CreatePCMStream(audioContent, audioBitrate, 1000, packetLoss );
                //var stream = client.CreatePCMStream(AudioApplication.Voice ,128000 );
                if (stream == null)
                {
                    Log("Unable to create output stream");
                    return;
                }
                audioOutStream = (AudioOutStream)stream;
            }
            else
            {
                Log("Audio client not connected");
            }
            currentVoiceChannelName = newVoiceChannel;
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
                    currentServer = discordSocketClient.GetGuild(serversUIDMap[newServerName]);
                    currentServerName = newServerName;
                }
                else
                {
                    currentServer = null;
                    currentServerName = "";
                }
                UpdateVoiceChannels(currentServerName);
            }
        }

        private Task DiscordSocketClient_Disconnected(Exception arg)
        {
            if (DiscordConnectionView != null)
            {
                DiscordConnectionView.Status = "Disconnected from discord";
                DiscordConnectionView.StatusColour = StatusColourCode.Red;
            }
            this.Connected = false;
            UpdateServers();
            return Task.CompletedTask;
        }

        private Task DiscordSocketClient_Connected()
        {
            if (DiscordConnectionView != null) DiscordConnectionView.Status = "Connected to discord";
            return Task.CompletedTask;
        }

        private Task DiscordSocketClient_Ready()
        {
            this.Connected = true;
            UpdateServers();
            if (DiscordConnectionView != null)
            {
                DiscordConnectionView.Status = "Idle";
                DiscordConnectionView.StatusColour = StatusColourCode.Green;
            }
            return Task.CompletedTask;
        }

        public async Task ConnectAsync()
        {

            if ((discordBotKey == "") || (discordBotKey == null))
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
            await discordSocketClient.LoginAsync(TokenType.Bot, discordBotKey);
            await discordSocketClient.StartAsync();
        }

        public async Task DisconnectAsync(bool force = false)
        {
            if (DiscordConnectionView != null) DiscordConnectionView.Status = "Disconnecting from discord";
            if (Connected || force)
            {
                // review why callinng lougout is blocking on exit
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

            List<string> list = new List<string>(WaveIn.DeviceCount + 2);
            audioDeviceNameToIDMap.Clear();
            list.Insert(0, "");
            for (int i = -1; i < WaveIn.DeviceCount; i++)
            {
                string compoundName = "" + (i + 1) + " - " + WaveIn.GetCapabilities(i).ProductName;
                audioDeviceNameToIDMap[compoundName] = i;
                list.Insert(i + 2, compoundName);
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

        private void StartCapture()
        {
            Log("Starting Capture");

            if (!audioDeviceNameToIDMap.ContainsKey(currentAudioDeviceName))
            {
                Log("Device does not exist");
                return;
            }


            try
            {
                circularBuffer = new CircularBuffer(circularBufferSize);               
                audioIn = new WaveInEvent();
                audioIn.WaveFormat = new WaveFormat(48000, 16, 2);
                audioIn.BufferMilliseconds = captureBufferDuration;
                audioIn.DeviceNumber = audioDeviceNameToIDMap[currentAudioDeviceName];
                audioIn.DataAvailable += Audioin_DataAvailable;
                audioIn.RecordingStopped += Audioin_RecordingStopped;
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
                    UpdateAudioCapureViewMeter(new byte[] { 0 }, 1);
                    circularBuffer.Reset();
                }
                finally
                {
                    audioIn.Dispose();
                    audioIn = null;
                }
            }
        }

        private void Audioin_RecordingStopped(object sender, StoppedEventArgs e)
        {
            Log("Capture stopped.");
            //throw new System.NotImplementedException();
        }

        private void Audioin_DataAvailable(object sender, WaveInEventArgs e)
        {
            //Log("Bytes recorded " + e.BytesRecorded);

            if (e.BytesRecorded >0)
            {
                // store the recorded data in a the circular buffer
                // then tell a worker thread to send the content
                //lock (circularBuffer)
                //{
                //    circularBuffer.Write(e.Buffer, 0, e.BytesRecorded);
                //}
                SendioAudioBufferAsync(e.Buffer, e.BytesRecorded);
            }
            UpdateAudioCapureViewMeter(e.Buffer, e.BytesRecorded);

        }

        public async void SendioAudioBufferAsync(byte [] buffer, int byteCount)
        {
            if (audioOutStream != null)
            {
                if (DiscordConnectionView != null)
                    DiscordConnectionView.Status = "Streaming";
                //DestinationBlockSize = 3840
                //byte[] buffer = new byte [3840 * 4];
                //int byteCount = 0;
                //while (circularBuffer.Count > 3840 * 8)
                //{
                //    lock (circularBuffer)
                //    {
                //        byteCount = circularBuffer.Read(buffer, 0, 3840 * 4);
                //        if (byteCount == 0)
                //            break;
                //    }
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
                                audioOutStream.WriteAsync(buffer, 0, byteCount).GetAwaiter().GetResult();
                            }
                            catch (System.Exception ex)
                            {
                                Log("Exception raised while sending audio " + ex.Message);
                            }
                            finally
                            {
                                //audioOutStream.Flush();
                                //audioOutStream.FlushAsync().GetAwaiter().GetResult();
                                audioOutStream.FlushAsync();
                            }
                        }

                    }
                //}
            }
        }


        public void UpdateAudioCapureViewMeter(byte[] buffer, int bytesRecorded)
        {
            if (_audioCaptureView != null)
            {
                // prevent overflow on trailing incomplete samples
                if (bytesRecorded % 4 != 0) bytesRecorded -= bytesRecorded % 4;
                short maxL = 0, maxR = 0;
                for (int sampleIndex = 0; sampleIndex < bytesRecorded; sampleIndex += 4)
                {
                    short sampleL = (short)((buffer[sampleIndex + 1] << 8) | buffer[sampleIndex + 0]);
                    //if (sampleL < 0) sampleL = (short)(0-sampleL);
                    // is Math.Abs implementation faster ?
                    sampleL = Math.Abs(sampleL);
                    if (sampleL > maxL) maxL = sampleL;

                    short sampleR = (short)((buffer[sampleIndex + 3] << 8) | buffer[sampleIndex + 2]);
                    sampleR = Math.Abs(sampleR);
                    if (sampleR > maxR) maxR = sampleR;

                }
                // max samples as float 
                float maxLF = maxL / 32768f;
                float maxRF = maxR / 32768f;

                // values in dbfs -infinity to 0 
                float dBFSvalueL = 20.0f * (float)Math.Log10(maxLF);
                float dBFSvalueR = 20.0f * (float)Math.Log10(maxRF);
                AudioCaptureView.SetLevel(dBFSvalueL, dBFSvalueR);

            }
        }
    }

}