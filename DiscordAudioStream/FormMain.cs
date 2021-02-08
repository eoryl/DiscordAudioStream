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
                }
                catch (Exception e)
                {
                    System.Console.WriteLine("Failed to update status strip: " + e.Message);
                }
            }
        }

        ICollection<string> IDiscordConnectionView.Servers
        {
            get
            {
                return new List<string>(this.comboBoxDiscordServer.Items.Cast<string>());
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

                            string oldServer = comboBoxDiscordServer.Text;

                            this.comboBoxDiscordServer.Items.Clear();
                            this.comboBoxDiscordServer.Items.AddRange(value.ToArray<string>());
                            if (comboBoxDiscordServer.Items.Contains(oldServer))
                                comboBoxDiscordServer.Text = oldServer;
                            updatingServerList = false;

                            if (comboBoxDiscordServer.Text != oldServer)
                                CurrentServerChanged?.Invoke(this, comboBoxDiscordServer.Text);

                        }
                        )
                    );
                }
                else
                {
                    this.comboBoxDiscordServer.Items.Clear();
                    this.comboBoxDiscordServer.Items.AddRange(value.ToArray<string>());
                }
            }

        }

        ICollection<string> IDiscordConnectionView.VoiceChannels
        {
            get
            {
                List<string> res = new List<string>();
                foreach (string s in comboBoxDiscordVoiceChannel.Items)
                    res.Add(s);
                return res;
            }
            set
            {
                comboBoxDiscordVoiceChannel.Items.Clear();
                if (value != null) comboBoxDiscordVoiceChannel.Items.AddRange(value.ToArray<Object>());
            }
        }

        string IDiscordConnectionView.CurrentServer
        {
            get
            {
                return comboBoxDiscordServer?.SelectedItem.ToString();
            }
            set
            {
                if (comboBoxDiscordServer.Items.Contains(value))
                    comboBoxDiscordServer.SelectedItem = value;
            }
        }
        string IDiscordConnectionView.CurrentVoiceChannel
        {
            get
            {
                return comboBoxDiscordVoiceChannel?.SelectedItem.ToString();
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
                                if ((value == StatusColourCode.Red) && (toolStripStatusIcon.Image != Properties.Resources.icon_cross))  toolStripStatusIcon.Image = Properties.Resources.icon_cross;
                                else if ((value == StatusColourCode.Orange) && (toolStripStatusIcon.Image != Properties.Resources.icon_question_mark)) toolStripStatusIcon.Image = Properties.Resources.icon_question_mark;
                                else if ((value == StatusColourCode.Green) && (toolStripStatusIcon.Image != Properties.Resources.icon_check_green)) toolStripStatusIcon.Image = Properties.Resources.icon_check_green;
                                else if ((value == StatusColourCode.Blue) && (toolStripStatusIcon.Image != Properties.Resources.icon_streaming)) toolStripStatusIcon.Image = Properties.Resources.icon_streaming;
                            }
                            )
                    );
            }
        }

        public string AudioContent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int AudioBitrate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event EventHandler<string> CurrentServerChanged;
        public event EventHandler<string> CurrentVoiceChannelChanged;
        public event EventHandler<string> SelectedAudioDeviceIDChanged;
        public event EventHandler<string> DiscordBotTokenChanged;
        public event EventHandler<string> AudioContentChanged;
        public event EventHandler<int> AudioBitrateChanged;
        public event EventHandler ViewRefreshRequested;

        private void FormMain_Load(object sender, EventArgs e)
        {
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
            }
            catch (Exception e)
            {
                System.Console.WriteLine("failed to update peak: " + e.Message);
            }

            /*
            if (this.progressBarLeft.InvokeRequired)
            {
                this.progressBarLeft.Invoke(new Action(
                    () => progressBarLeft.Value = (int)(leftChannel * 100)
                    )
                );
            }
            else
            {
                progressBarLeft.Value = (int)(leftChannel * 100);
            }

            if (this.progressBarRight.InvokeRequired)
            {
                this.progressBarRight.Invoke(new Action(
                    () => progressBarRight.Value = (int)(rightChannel * 100)
                    )
                );
            }
            else
            {
                progressBarRight.Value = (int)(rightChannel * 100);
            }
            */
        }

        private void comboBoxDiscordServer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!updatingServerList) CurrentServerChanged?.Invoke(this, comboBoxDiscordServer.Text);

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
            if (!updatingChannelList) CurrentVoiceChannelChanged?.Invoke(this, comboBoxDiscordVoiceChannel?.Text);
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
    }
}
