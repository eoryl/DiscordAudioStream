using DiscordAudioStream.Views;
using DiscordAudioStreamService;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace DiscordAudioStream
{
    public partial class FormMain : Form, IAudioCaptureView, IDiscordConnectionView
    {

        private AudioStreamingService audioStreamingService;
        private bool updatingServerList = false;
        private bool updatingChannelList = false;

        private volatile float peakL = -144.0f;
        private volatile float peakR = -144.0f;
        private volatile StatusColourCode statusColourCode = StatusColourCode.Red;
        private string statusMessage = "";
        private Object updateLock = new Object();
        private bool gainUpdating = false;

        public FormMain()
        {
            InitializeComponent();
        }

        string IDiscordConnectionView.Status
        {
            get
            {
                return statusStripMain?.Items["toolStripStatusLabelMessage"].Text;
            }
            set
            {
                try
                {
                    statusStripMain?.Invoke(
                        new Action(
                                () =>
                                {
                                    if (statusStripMain != null)
                                    {
                                        if (statusStripMain.Items["toolStripStatusLabelMessage"].Text != value)
                                            statusStripMain.Items["toolStripStatusLabelMessage"].Text = value;
                }
                                }
                                )
                        );
                    lock (updateLock)
                    {
                        statusMessage = value;
                    }
                }
                catch (Exception e)
                {
                    System.Console.WriteLine("Failed to update status strip: " + e.Message);
                }
            }
        }

        ICollection<DiscordServerInfo> IDiscordConnectionView.Servers
        {
            get
            {
                return (List<DiscordServerInfo>) comboBoxDiscordServer.DataSource;
            }
            set
            {
                // use invoke to update control if is done from another thread 
                if (this.comboBoxDiscordServer.InvokeRequired)
                {
                    this.comboBoxDiscordServer.Invoke(new Action(
                        () =>
                        {
                            updatingServerList = true;

                            ulong oldServer = 0;
                            if (comboBoxDiscordServer.SelectedValue != null) oldServer =  (ulong) comboBoxDiscordServer.SelectedValue;
                            comboBoxDiscordServer.DataSource = value;
                            comboBoxDiscordServer.ValueMember = "ID";
                            comboBoxDiscordServer.DisplayMember = "Name";

                            if (comboBoxDiscordServer.Items.Contains(oldServer))
                                comboBoxDiscordServer.SelectedValue = oldServer;
                            updatingServerList = false;

                            if (comboBoxDiscordServer.SelectedValue != null)
                            {
                                if (oldServer != (ulong) comboBoxDiscordServer.SelectedValue)
                                    CurrentServerChanged?.Invoke(this, (ulong)comboBoxDiscordServer?.SelectedValue);
                            }

                        }
                        )
                    );
                }
                else
                {
                    this.comboBoxDiscordServer.DataSource = value;
                }
            }

        }

        ICollection<DiscordVoiceChannelInfo> IDiscordConnectionView.VoiceChannels
        {
            get
            {
                return (ICollection<DiscordVoiceChannelInfo>) comboBoxDiscordVoiceChannel.DataSource;
            }
            set
            {
                updatingChannelList = true;
                comboBoxDiscordVoiceChannel.DataSource = value;
                comboBoxDiscordVoiceChannel.ValueMember = "ID";
                comboBoxDiscordVoiceChannel.DisplayMember = "CompositeName";
                updatingChannelList = false;
            }
        }

        ulong IDiscordConnectionView.CurrentServer
        {
            get
            {
                if (comboBoxDiscordServer.SelectedValue != null)
                    return (ulong)comboBoxDiscordServer.SelectedValue;
                else return 0;
            }
            set
            {
                if (comboBoxDiscordServer.Items.Contains(value))
                    comboBoxDiscordServer.SelectedItem = value;
            }
        }
        ulong IDiscordConnectionView.CurrentVoiceChannel
        {
            get
            {
                if (comboBoxDiscordVoiceChannel.SelectedValue != null)
                    return (ulong)comboBoxDiscordVoiceChannel.SelectedValue;
                else return 0;
            }
            set
            {
                if (comboBoxDiscordVoiceChannel.Items.Contains(value))
                    comboBoxDiscordVoiceChannel.SelectedItem = value;
            }
        }
        ICollection<AudioCaptureDeviceInfo> IAudioCaptureView.AudioDevices
        {
            get
            {
                //List<string> res = new List<string>();
                //foreach (string s in comboBoxAudioDevice.Items)
                //    res.Add(s);
                //return res;
                return null;
            }
            set
            {
                // use BindingSource
                comboBoxAudioDevice.ValueMember = "DeviceID";
                comboBoxAudioDevice.DisplayMember = "DisplayName";
                comboBoxAudioDevice.DataSource = value;
                //comboBoxAudioDevice.Items.Clear();
                //if (value != null) comboBoxAudioDevice.Items.AddRange(value.ToArray<Object>());
            }
        }

        string IAudioCaptureView.SelectedAudioDeviceID { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string DiscordBotToken
        {
            get
            {
                return Properties.Settings.Default.DiscordBotToken;
            }
            set
            {
                Properties.Settings.Default.DiscordBotToken = value;
                Properties.Settings.Default.Save();
            }
        }

        public StatusColourCode StatusColour
        {
            set
            {
                Invoke(
                    new Action(
                            () =>
                            {
                                if ((value == StatusColourCode.Red) && (toolStripStatusIcon.Image != Properties.Resources.icon_cross)) toolStripStatusIcon.Image = Properties.Resources.icon_cross;
                                else if ((value == StatusColourCode.Orange) && (toolStripStatusIcon.Image != Properties.Resources.icon_question_mark)) toolStripStatusIcon.Image = Properties.Resources.icon_question_mark;
                                else if ((value == StatusColourCode.Green) && (toolStripStatusIcon.Image != Properties.Resources.icon_check_green)) toolStripStatusIcon.Image = Properties.Resources.icon_check_green;
                                else if ((value == StatusColourCode.Blue) && (toolStripStatusIcon.Image != Properties.Resources.icon_streaming)) toolStripStatusIcon.Image = Properties.Resources.icon_streaming;
                            }
                            )
                    );
                statusColourCode = value;
            }
        }

        public string AudioContent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int AudioBitrate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float Gain 
        { 
            get 
            {
                return (float)trackBarMasterFader.Value;
            }
            set 
            { 
                try
                {
                    gainUpdating = true;
                    trackBarMasterFader.Value = (int)value;
                    gainUpdating = false;
                }
                catch(Exception e)
                {
                }
            }
        }
        public bool Mute 
        {
            get
            {
                return trackBarMasterFader.Value == trackBarMasterFader.Minimum;
            }
            set
            {
                gainUpdating = true;
                trackBarMasterFader.Value = trackBarMasterFader.Minimum;
                gainUpdating = false;
            }
        }

        public event EventHandler<ulong> CurrentServerChanged;
        public event EventHandler<ulong> CurrentVoiceChannelChanged;
        public event EventHandler<string> SelectedAudioDeviceIDChanged;
        public event EventHandler<string> DiscordBotTokenChanged;
        public event EventHandler<string> AudioContentChanged;
        public event EventHandler<int> AudioBitrateChanged;
        public event EventHandler<bool> Muted;
        public event EventHandler<float> GainChanged;

        private void FormMain_Load(object sender, EventArgs e)
        {
            peakMeterL.PeakHoldTimeMS = Convert.ToInt64( Properties.Settings.Default.PeakHoldTime);
            peakMeterR.PeakHoldTimeMS = Convert.ToInt64(Properties.Settings.Default.PeakHoldTime);

            audioStreamingService = new AudioStreamingService(this, this);
            // TODO : 
            // redo that properly with with an interface to load settings
            audioStreamingService.DiscordBotToken = Properties.Settings.Default.DiscordBotToken;
            audioStreamingService.CaptureBufferDuration = Properties.Settings.Default.AudioCaptureBufferMS;
            audioStreamingService.AudioBitrate = Properties.Settings.Default.AudioBitrate * 1024;
            audioStreamingService.AudioContent = Properties.Settings.Default.AudioContent;
            audioStreamingService.StreamingBufferDuration = Properties.Settings.Default.StreamingBufferDuration;
            audioStreamingService.AudioCaptureAPI = AudioStreamingService.ParseAudioAPI( Properties.Settings.Default.CaptureAPI);
            audioStreamingService.Initialize();


        }

        void IAudioCaptureView.SetPeak(float leftChannel, float rightChannel)
        {
            try
            {
                if (this.peakMeterL.InvokeRequired)
                {
                    this.peakMeterL.Invoke(new Action(
                        () => peakMeterL.Level = leftChannel
                        )
                    );
                }
                else
                {
                    peakMeterL.Level = leftChannel;
                }

                if (this.peakMeterR.InvokeRequired)
                {
                    this.peakMeterR.Invoke(new Action(
                        () => peakMeterR.Level = rightChannel
                        )
                    );
                }
                else
                {
                    peakMeterR.Level = rightChannel;
                }

                peakL = leftChannel;
                peakR = rightChannel;
            }
            catch (Exception e)
            {
                System.Console.WriteLine("failed to update peak: " + e.Message);
            }


        }

        private void comboBoxDiscordServer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!updatingServerList) CurrentServerChanged?.Invoke(this, (ulong) comboBoxDiscordServer?.SelectedValue);

        }

        public void DisplayErrorMessage(string errorMessage)
        {
            Invoke(
                new Action(
                        () => MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        )
                );
        }

        private void toolStripStatusIcon_Click(object sender, EventArgs e)
        {
            //if (audioStreamingService.Connected) audioStreamingService.DisconnectAsync().ConfigureAwait(false);
            //else audioStreamingService.ConnectAsync().ConfigureAwait(false);
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void comboBoxDiscordVoiceChannel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!updatingChannelList)
            {
                ulong newChannel = 0;
                if (comboBoxDiscordVoiceChannel.SelectedValue != null)
                {
                    newChannel = (ulong)comboBoxDiscordVoiceChannel.SelectedValue;
                }
                CurrentVoiceChannelChanged?.Invoke(this, newChannel);

            }
        }



        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            audioStreamingService.Terminate();

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (AboutBox aboutBox = new AboutBox())
            {
                aboutBox.ShowDialog(this);
            }
        }

        private void comboBoxAudioDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedAudioDeviceIDChanged != null) SelectedAudioDeviceIDChanged(this, comboBoxAudioDevice.SelectedValue.ToString());
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {

            using (FormSettings formSettings = new FormSettings())
            {
                if (formSettings.ShowDialog() == DialogResult.OK)
                {
                    Properties.Settings.Default.Save();
                    MessageBox.Show("Application restart needed to apply new parameters.");
                }
                else
                {
                    Properties.Settings.Default.Reload();
                }
                
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void menuStripMain_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void floatOnTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.TopMost = !this.TopMost;
            floatOnTopToolStripMenuItem.Checked = this.TopMost;
        }

        private void timerRefresh_Tick(object sender, EventArgs e)
        {
            //float peakLNow = peakL;
            //float peakRNow = peakR;
            //StatusColourCode statusColourCodeNow = statusColourCode;
            //string statusMessageNow = "";

            //lock (updateLock)
            //{
            //    statusMessageNow = statusMessage;
            //}

            //if (statusStripMain != null)
            //{
            //    if (statusStripMain.Items["toolStripStatusLabelMessage"].Text != statusMessage)
            //        statusStripMain.Items["toolStripStatusLabelMessage"].Text = statusMessage;
            //}

            //if ((statusColourCode == StatusColourCode.Red) && (toolStripStatusIcon.Image != Properties.Resources.icon_cross)) toolStripStatusIcon.Image = Properties.Resources.icon_cross;
            //else if ((statusColourCode == StatusColourCode.Orange) && (toolStripStatusIcon.Image != Properties.Resources.icon_question_mark)) toolStripStatusIcon.Image = Properties.Resources.icon_question_mark;
            //else if ((statusColourCode == StatusColourCode.Green) && (toolStripStatusIcon.Image != Properties.Resources.icon_check_green)) toolStripStatusIcon.Image = Properties.Resources.icon_check_green;
            //else if ((statusColourCode == StatusColourCode.Blue) && (toolStripStatusIcon.Image != Properties.Resources.icon_streaming)) toolStripStatusIcon.Image = Properties.Resources.icon_streaming;
               
            //peakMeterL.Level = peakLNow;
            //peakMeterR.Level = peakRNow;

            peakMeterL.UpdatePeak();
            peakMeterR.UpdatePeak();


        }

        private void DisplayGainTooltip()
        {
            string strval = "";
            if (trackBarMasterFader.Value == trackBarMasterFader.Minimum)
            {
                strval = "muted";
            }
            else
            {
                strval = "" + trackBarMasterFader.Value + " dB";
            }
            toolTipMasterFaderGain.Show(strval, trackBarMasterFader, trackBarMasterFader.PointToClient(MousePosition), 2000);
        }

        private void trackBarMasterFader_Scroll(object sender, EventArgs e)
        {
            if (gainUpdating == true)
                return;

            if (trackBarMasterFader.Value == trackBarMasterFader.Minimum)
            {
                if (Muted != null) Muted(this, true);
            }
            else
            {
                if (Muted != null) Muted(this, false);
            }
            if (GainChanged != null) GainChanged(this, (float)trackBarMasterFader.Value);

            DisplayGainTooltip();

        }

        private void trackBarMasterFader_MouseDown(object sender, MouseEventArgs e)
        {
            DisplayGainTooltip();
        }

        private void trackBarMasterFader_MouseHover(object sender, EventArgs e)
        {
            DisplayGainTooltip();

        }
    }
}
