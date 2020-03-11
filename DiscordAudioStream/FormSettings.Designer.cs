namespace DiscordAudioStream
{
    partial class FormSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelDiscordBotKey = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelAudioCaptureBufferSize = new System.Windows.Forms.Label();
            this.labelAudioCaptureSizeValue = new System.Windows.Forms.Label();
            this.linkLabelBotTokenURL = new System.Windows.Forms.LinkLabel();
            this.labelEncoderBirate = new System.Windows.Forms.Label();
            this.labelEncoderBirateValue = new System.Windows.Forms.Label();
            this.labelContentType = new System.Windows.Forms.Label();
            this.labelPacketLoss = new System.Windows.Forms.Label();
            this.labelPacketLossValue = new System.Windows.Forms.Label();
            this.trackBarPacketLoss = new System.Windows.Forms.TrackBar();
            this.comboBoxContentType = new System.Windows.Forms.ComboBox();
            this.trackBarEncoderBirate = new System.Windows.Forms.TrackBar();
            this.trackBarAudioCaptureBuffer = new System.Windows.Forms.TrackBar();
            this.textBoxDiscordBotKey = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarPacketLoss)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarEncoderBirate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarAudioCaptureBuffer)).BeginInit();
            this.SuspendLayout();
            // 
            // labelDiscordBotKey
            // 
            this.labelDiscordBotKey.AutoSize = true;
            this.labelDiscordBotKey.Location = new System.Drawing.Point(9, 16);
            this.labelDiscordBotKey.Name = "labelDiscordBotKey";
            this.labelDiscordBotKey.Size = new System.Drawing.Size(68, 17);
            this.labelDiscordBotKey.TabIndex = 0;
            this.labelDiscordBotKey.Text = "Bot token";
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(428, 271);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(509, 271);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // labelAudioCaptureBufferSize
            // 
            this.labelAudioCaptureBufferSize.AutoSize = true;
            this.labelAudioCaptureBufferSize.Location = new System.Drawing.Point(12, 82);
            this.labelAudioCaptureBufferSize.Name = "labelAudioCaptureBufferSize";
            this.labelAudioCaptureBufferSize.Size = new System.Drawing.Size(166, 17);
            this.labelAudioCaptureBufferSize.TabIndex = 4;
            this.labelAudioCaptureBufferSize.Text = "Audio capture buffer size";
            // 
            // labelAudioCaptureSizeValue
            // 
            this.labelAudioCaptureSizeValue.AutoSize = true;
            this.labelAudioCaptureSizeValue.Location = new System.Drawing.Point(203, 82);
            this.labelAudioCaptureSizeValue.Name = "labelAudioCaptureSizeValue";
            this.labelAudioCaptureSizeValue.Size = new System.Drawing.Size(54, 17);
            this.labelAudioCaptureSizeValue.TabIndex = 7;
            this.labelAudioCaptureSizeValue.Text = "100 ms";
            // 
            // linkLabelBotTokenURL
            // 
            this.linkLabelBotTokenURL.AutoSize = true;
            this.linkLabelBotTokenURL.Location = new System.Drawing.Point(12, 44);
            this.linkLabelBotTokenURL.Name = "linkLabelBotTokenURL";
            this.linkLabelBotTokenURL.Size = new System.Drawing.Size(426, 17);
            this.linkLabelBotTokenURL.TabIndex = 8;
            this.linkLabelBotTokenURL.TabStop = true;
            this.linkLabelBotTokenURL.Text = "Get a bot token on https://discordapp.com/developers/applications";
            this.linkLabelBotTokenURL.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelBotTokenURL_LinkClicked);
            // 
            // labelEncoderBirate
            // 
            this.labelEncoderBirate.AutoSize = true;
            this.labelEncoderBirate.Location = new System.Drawing.Point(12, 163);
            this.labelEncoderBirate.Name = "labelEncoderBirate";
            this.labelEncoderBirate.Size = new System.Drawing.Size(181, 17);
            this.labelEncoderBirate.TabIndex = 9;
            this.labelEncoderBirate.Text = "Opus audio encoder bitrate";
            // 
            // labelEncoderBirateValue
            // 
            this.labelEncoderBirateValue.AutoSize = true;
            this.labelEncoderBirateValue.Location = new System.Drawing.Point(199, 163);
            this.labelEncoderBirateValue.Name = "labelEncoderBirateValue";
            this.labelEncoderBirateValue.Size = new System.Drawing.Size(58, 17);
            this.labelEncoderBirateValue.TabIndex = 10;
            this.labelEncoderBirateValue.Text = "96 kbps";
            // 
            // labelContentType
            // 
            this.labelContentType.AutoSize = true;
            this.labelContentType.Location = new System.Drawing.Point(12, 122);
            this.labelContentType.Name = "labelContentType";
            this.labelContentType.Size = new System.Drawing.Size(168, 17);
            this.labelContentType.TabIndex = 12;
            this.labelContentType.Text = "Audio codec content type";
            // 
            // labelPacketLoss
            // 
            this.labelPacketLoss.AutoSize = true;
            this.labelPacketLoss.Location = new System.Drawing.Point(12, 208);
            this.labelPacketLoss.Name = "labelPacketLoss";
            this.labelPacketLoss.Size = new System.Drawing.Size(80, 17);
            this.labelPacketLoss.TabIndex = 14;
            this.labelPacketLoss.Text = "Packet loss";
            // 
            // labelPacketLossValue
            // 
            this.labelPacketLossValue.AutoSize = true;
            this.labelPacketLossValue.Location = new System.Drawing.Point(202, 207);
            this.labelPacketLossValue.Name = "labelPacketLossValue";
            this.labelPacketLossValue.Size = new System.Drawing.Size(36, 17);
            this.labelPacketLossValue.TabIndex = 16;
            this.labelPacketLossValue.Text = "30%";
            // 
            // trackBarPacketLoss
            // 
            this.trackBarPacketLoss.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::DiscordAudioStream.Properties.Settings.Default, "PacketLoss", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.trackBarPacketLoss.Location = new System.Drawing.Point(263, 197);
            this.trackBarPacketLoss.Maximum = 100;
            this.trackBarPacketLoss.Name = "trackBarPacketLoss";
            this.trackBarPacketLoss.Size = new System.Drawing.Size(321, 56);
            this.trackBarPacketLoss.TabIndex = 15;
            this.trackBarPacketLoss.Value = global::DiscordAudioStream.Properties.Settings.Default.PacketLoss;
            this.trackBarPacketLoss.ValueChanged += new System.EventHandler(this.trackBarPacketLoss_ValueChanged);
            // 
            // comboBoxContentType
            // 
            this.comboBoxContentType.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::DiscordAudioStream.Properties.Settings.Default, "AudioContent", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.comboBoxContentType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxContentType.FormattingEnabled = true;
            this.comboBoxContentType.Items.AddRange(new object[] {
            "Mixed",
            "Voice",
            "Music"});
            this.comboBoxContentType.Location = new System.Drawing.Point(263, 122);
            this.comboBoxContentType.Name = "comboBoxContentType";
            this.comboBoxContentType.Size = new System.Drawing.Size(121, 24);
            this.comboBoxContentType.TabIndex = 13;
            this.comboBoxContentType.Text = global::DiscordAudioStream.Properties.Settings.Default.AudioContent;
            this.comboBoxContentType.TextChanged += new System.EventHandler(this.comboBoxContentType_TextChanged);
            // 
            // trackBarEncoderBirate
            // 
            this.trackBarEncoderBirate.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::DiscordAudioStream.Properties.Settings.Default, "AudioBitrate", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.trackBarEncoderBirate.LargeChange = 16384;
            this.trackBarEncoderBirate.Location = new System.Drawing.Point(263, 152);
            this.trackBarEncoderBirate.Maximum = 131072;
            this.trackBarEncoderBirate.Minimum = 16384;
            this.trackBarEncoderBirate.Name = "trackBarEncoderBirate";
            this.trackBarEncoderBirate.Size = new System.Drawing.Size(321, 56);
            this.trackBarEncoderBirate.SmallChange = 8192;
            this.trackBarEncoderBirate.TabIndex = 11;
            this.trackBarEncoderBirate.TickFrequency = 8192;
            this.trackBarEncoderBirate.Value = global::DiscordAudioStream.Properties.Settings.Default.AudioBitrate;
            this.trackBarEncoderBirate.ValueChanged += new System.EventHandler(this.trackBarEncoderBirate_ValueChanged);
            // 
            // trackBarAudioCaptureBuffer
            // 
            this.trackBarAudioCaptureBuffer.DataBindings.Add(new System.Windows.Forms.Binding("Value", global::DiscordAudioStream.Properties.Settings.Default, "AudioCaptureBufferMS", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.trackBarAudioCaptureBuffer.LargeChange = 20;
            this.trackBarAudioCaptureBuffer.Location = new System.Drawing.Point(263, 70);
            this.trackBarAudioCaptureBuffer.Maximum = 1000;
            this.trackBarAudioCaptureBuffer.Minimum = 20;
            this.trackBarAudioCaptureBuffer.Name = "trackBarAudioCaptureBuffer";
            this.trackBarAudioCaptureBuffer.Size = new System.Drawing.Size(321, 56);
            this.trackBarAudioCaptureBuffer.SmallChange = 10;
            this.trackBarAudioCaptureBuffer.TabIndex = 6;
            this.trackBarAudioCaptureBuffer.TickFrequency = 20;
            this.trackBarAudioCaptureBuffer.Value = global::DiscordAudioStream.Properties.Settings.Default.AudioCaptureBufferMS;
            this.trackBarAudioCaptureBuffer.Scroll += new System.EventHandler(this.trackBarAudioCaptureBuffer_Scroll);
            this.trackBarAudioCaptureBuffer.ValueChanged += new System.EventHandler(this.trackBarAudioCaptureBuffer_ValueChanged);
            // 
            // textBoxDiscordBotKey
            // 
            this.textBoxDiscordBotKey.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::DiscordAudioStream.Properties.Settings.Default, "DiscordBotKey", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.textBoxDiscordBotKey.Location = new System.Drawing.Point(83, 13);
            this.textBoxDiscordBotKey.Name = "textBoxDiscordBotKey";
            this.textBoxDiscordBotKey.Size = new System.Drawing.Size(501, 22);
            this.textBoxDiscordBotKey.TabIndex = 1;
            this.textBoxDiscordBotKey.Text = global::DiscordAudioStream.Properties.Settings.Default.DiscordBotKey;
            // 
            // FormSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(596, 306);
            this.Controls.Add(this.labelPacketLossValue);
            this.Controls.Add(this.trackBarPacketLoss);
            this.Controls.Add(this.labelPacketLoss);
            this.Controls.Add(this.comboBoxContentType);
            this.Controls.Add(this.labelContentType);
            this.Controls.Add(this.trackBarEncoderBirate);
            this.Controls.Add(this.labelEncoderBirateValue);
            this.Controls.Add(this.labelEncoderBirate);
            this.Controls.Add(this.linkLabelBotTokenURL);
            this.Controls.Add(this.labelAudioCaptureSizeValue);
            this.Controls.Add(this.trackBarAudioCaptureBuffer);
            this.Controls.Add(this.labelAudioCaptureBufferSize);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.textBoxDiscordBotKey);
            this.Controls.Add(this.labelDiscordBotKey);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormSettings";
            this.Text = "Preferences";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormSettings_FormClosing);
            this.Load += new System.EventHandler(this.FormSettings_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarPacketLoss)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarEncoderBirate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarAudioCaptureBuffer)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelDiscordBotKey;
        private System.Windows.Forms.TextBox textBoxDiscordBotKey;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelAudioCaptureBufferSize;
        private System.Windows.Forms.TrackBar trackBarAudioCaptureBuffer;
        private System.Windows.Forms.Label labelAudioCaptureSizeValue;
        private System.Windows.Forms.LinkLabel linkLabelBotTokenURL;
        private System.Windows.Forms.Label labelEncoderBirate;
        private System.Windows.Forms.Label labelEncoderBirateValue;
        private System.Windows.Forms.TrackBar trackBarEncoderBirate;
        private System.Windows.Forms.Label labelContentType;
        private System.Windows.Forms.ComboBox comboBoxContentType;
        private System.Windows.Forms.Label labelPacketLoss;
        private System.Windows.Forms.TrackBar trackBarPacketLoss;
        private System.Windows.Forms.Label labelPacketLossValue;
    }
}