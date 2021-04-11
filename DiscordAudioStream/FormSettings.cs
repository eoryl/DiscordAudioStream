using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace DiscordAudioStream
{
    public partial class FormSettings : Form
    {
        public FormSettings()
        {
            InitializeComponent();
            labelAudioCaptureSizeValue.Text = $"{trackBarAudioCaptureBuffer.Value} ms";
            labelPacketLossValue.Text = $"{trackBarPacketLoss.Value}%";
            labelEncoderBirateValue.Text = "" + trackBarEncoderBirate.Value+ " kbps";
            labelStreamingBufferDurationValue.Text = $"{trackBarStreamingBufferDuration.Value} ms";
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();

        }

        private void trackBarAudioCaptureBuffer_Scroll(object sender, EventArgs e)
        {

        }

        private void trackBarAudioCaptureBuffer_ValueChanged(object sender, EventArgs e)
        {
            if (trackBarAudioCaptureBuffer.Value % 20 != 0) trackBarAudioCaptureBuffer.Value -= (trackBarAudioCaptureBuffer.Value % 20);
            labelAudioCaptureSizeValue.Text = "" + trackBarAudioCaptureBuffer.Value + " ms";
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {

        }

        private void linkLabelBotTokenURL_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://discordapp.com/developers/applications");
        }

        private void trackBarEncoderBirate_ValueChanged(object sender, EventArgs e)
        {
            if (trackBarEncoderBirate.Value % 8 != 0) trackBarEncoderBirate.Value -= (trackBarEncoderBirate.Value % 8);
            labelEncoderBirateValue.Text = "" + trackBarEncoderBirate.Value + " kbps";
        }

        private void comboBoxContentType_TextChanged(object sender, EventArgs e)
        {

        }

        private void FormSettings_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void trackBarPacketLoss_ValueChanged(object sender, EventArgs e)
        {
            labelPacketLossValue.Text = $"{trackBarPacketLoss.Value}%";
        }

        private void trackBarStreamingBufferDuration_ValueChanged(object sender, EventArgs e)
        {
            if (trackBarStreamingBufferDuration.Value % 20 > 0) trackBarStreamingBufferDuration.Value -= trackBarStreamingBufferDuration.Value % 20;
            labelStreamingBufferDurationValue.Text = $"{trackBarStreamingBufferDuration.Value} ms"; 
        }

        private void trackBarEncoderBirate_Scroll(object sender, EventArgs e)
        {

        }

        private void labelDiscordBotToken_Click(object sender, EventArgs e)
        {
            textBoxDiscordBotToken.UseSystemPasswordChar = !textBoxDiscordBotToken.UseSystemPasswordChar;
        }

    }
}
