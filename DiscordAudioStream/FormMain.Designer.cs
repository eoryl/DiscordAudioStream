namespace DiscordAudioStream
{
    partial class FormMain
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.groupBoxDiscordSettings = new System.Windows.Forms.GroupBox();
            this.comboBoxDiscordVoiceChannel = new System.Windows.Forms.ComboBox();
            this.labelDiscordVoiceChannel = new System.Windows.Forms.Label();
            this.comboBoxDiscordServer = new System.Windows.Forms.ComboBox();
            this.labelDiscordServer = new System.Windows.Forms.Label();
            this.groupBoxLivesource = new System.Windows.Forms.GroupBox();
            this.scale1 = new DiscordAudioStream.Scale();
            this.peakMeterR = new DiscordAudioStream.PeakMeter();
            this.peakMeterL = new DiscordAudioStream.PeakMeter();
            this.comboBoxAudioDevice = new System.Windows.Forms.ComboBox();
            this.labelAudioDevice = new System.Windows.Forms.Label();
            this.menuStripMain = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.windowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.floatOnTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStripMain = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusIcon = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusMessage = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelMessage = new System.Windows.Forms.ToolStripStatusLabel();
            this.timerRefresh = new System.Windows.Forms.Timer(this.components);
            this.groupBoxDiscordSettings.SuspendLayout();
            this.groupBoxLivesource.SuspendLayout();
            this.menuStripMain.SuspendLayout();
            this.statusStripMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxDiscordSettings
            // 
            this.groupBoxDiscordSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxDiscordSettings.Controls.Add(this.comboBoxDiscordVoiceChannel);
            this.groupBoxDiscordSettings.Controls.Add(this.labelDiscordVoiceChannel);
            this.groupBoxDiscordSettings.Controls.Add(this.comboBoxDiscordServer);
            this.groupBoxDiscordSettings.Controls.Add(this.labelDiscordServer);
            this.groupBoxDiscordSettings.Location = new System.Drawing.Point(10, 188);
            this.groupBoxDiscordSettings.Name = "groupBoxDiscordSettings";
            this.groupBoxDiscordSettings.Size = new System.Drawing.Size(461, 140);
            this.groupBoxDiscordSettings.TabIndex = 1;
            this.groupBoxDiscordSettings.TabStop = false;
            this.groupBoxDiscordSettings.Text = "Discord";
            // 
            // comboBoxDiscordVoiceChannel
            // 
            this.comboBoxDiscordVoiceChannel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxDiscordVoiceChannel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDiscordVoiceChannel.DropDownWidth = 235;
            this.comboBoxDiscordVoiceChannel.FormattingEnabled = true;
            this.comboBoxDiscordVoiceChannel.Location = new System.Drawing.Point(10, 101);
            this.comboBoxDiscordVoiceChannel.Name = "comboBoxDiscordVoiceChannel";
            this.comboBoxDiscordVoiceChannel.Size = new System.Drawing.Size(445, 24);
            this.comboBoxDiscordVoiceChannel.TabIndex = 3;
            this.comboBoxDiscordVoiceChannel.SelectedIndexChanged += new System.EventHandler(this.comboBoxDiscordVoiceChannel_SelectedIndexChanged);
            // 
            // labelDiscordVoiceChannel
            // 
            this.labelDiscordVoiceChannel.AutoSize = true;
            this.labelDiscordVoiceChannel.Location = new System.Drawing.Point(7, 77);
            this.labelDiscordVoiceChannel.Name = "labelDiscordVoiceChannel";
            this.labelDiscordVoiceChannel.Size = new System.Drawing.Size(97, 17);
            this.labelDiscordVoiceChannel.TabIndex = 2;
            this.labelDiscordVoiceChannel.Text = "Voice channel";
            // 
            // comboBoxDiscordServer
            // 
            this.comboBoxDiscordServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxDiscordServer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDiscordServer.DropDownWidth = 235;
            this.comboBoxDiscordServer.FormattingEnabled = true;
            this.comboBoxDiscordServer.Location = new System.Drawing.Point(10, 42);
            this.comboBoxDiscordServer.Name = "comboBoxDiscordServer";
            this.comboBoxDiscordServer.Size = new System.Drawing.Size(445, 24);
            this.comboBoxDiscordServer.TabIndex = 1;
            this.comboBoxDiscordServer.SelectedIndexChanged += new System.EventHandler(this.comboBoxDiscordServer_SelectedIndexChanged);
            // 
            // labelDiscordServer
            // 
            this.labelDiscordServer.AutoSize = true;
            this.labelDiscordServer.Location = new System.Drawing.Point(7, 22);
            this.labelDiscordServer.Name = "labelDiscordServer";
            this.labelDiscordServer.Size = new System.Drawing.Size(50, 17);
            this.labelDiscordServer.TabIndex = 0;
            this.labelDiscordServer.Text = "Server";
            // 
            // groupBoxLivesource
            // 
            this.groupBoxLivesource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxLivesource.Controls.Add(this.scale1);
            this.groupBoxLivesource.Controls.Add(this.peakMeterR);
            this.groupBoxLivesource.Controls.Add(this.peakMeterL);
            this.groupBoxLivesource.Controls.Add(this.comboBoxAudioDevice);
            this.groupBoxLivesource.Controls.Add(this.labelAudioDevice);
            this.groupBoxLivesource.Location = new System.Drawing.Point(11, 45);
            this.groupBoxLivesource.Name = "groupBoxLivesource";
            this.groupBoxLivesource.Size = new System.Drawing.Size(460, 137);
            this.groupBoxLivesource.TabIndex = 2;
            this.groupBoxLivesource.TabStop = false;
            this.groupBoxLivesource.Text = "Live source";
            // 
            // scale1
            // 
            this.scale1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scale1.LargeGradation = 10;
            this.scale1.LargeGrationColor = System.Drawing.Color.DarkGray;
            this.scale1.Location = new System.Drawing.Point(9, 51);
            this.scale1.Max = 0;
            this.scale1.Min = -60;
            this.scale1.Name = "scale1";
            this.scale1.Orientation = DiscordAudioStream.Scale.ScaleOrientation.Horizontal;
            this.scale1.Size = new System.Drawing.Size(445, 23);
            this.scale1.SmallGradation = 2;
            this.scale1.SmallGrationColor = System.Drawing.Color.LightGray;
            this.scale1.TabIndex = 8;
            // 
            // peakMeterR
            // 
            this.peakMeterR.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.peakMeterR.BackColor = System.Drawing.Color.DimGray;
            this.peakMeterR.ClipLevel = -6F;
            this.peakMeterR.ClipLevelColor = System.Drawing.Color.Firebrick;
            this.peakMeterR.DrawPeak = true;
            this.peakMeterR.HeadroomLevel = -18F;
            this.peakMeterR.HeadroomLevelColor = System.Drawing.Color.Chocolate;
            this.peakMeterR.Level = -60F;
            this.peakMeterR.LevelColor = System.Drawing.Color.DarkGreen;
            this.peakMeterR.Location = new System.Drawing.Point(9, 36);
            this.peakMeterR.Name = "peakMeterR";
            this.peakMeterR.Orientation = DiscordAudioStream.PeakMeter.MeterOrientation.Horizontal;
            this.peakMeterR.PeakColor = System.Drawing.Color.LightSteelBlue;
            this.peakMeterR.PeakDecayTimeMS = ((long)(2000));
            this.peakMeterR.Size = new System.Drawing.Size(445, 10);
            this.peakMeterR.TabIndex = 7;
            // 
            // peakMeterL
            // 
            this.peakMeterL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.peakMeterL.BackColor = System.Drawing.Color.DimGray;
            this.peakMeterL.ClipLevel = -6F;
            this.peakMeterL.ClipLevelColor = System.Drawing.Color.Firebrick;
            this.peakMeterL.DrawPeak = true;
            this.peakMeterL.HeadroomLevel = -18F;
            this.peakMeterL.HeadroomLevelColor = System.Drawing.Color.Chocolate;
            this.peakMeterL.Level = -60F;
            this.peakMeterL.LevelColor = System.Drawing.Color.DarkGreen;
            this.peakMeterL.Location = new System.Drawing.Point(9, 24);
            this.peakMeterL.Name = "peakMeterL";
            this.peakMeterL.Orientation = DiscordAudioStream.PeakMeter.MeterOrientation.Horizontal;
            this.peakMeterL.PeakColor = System.Drawing.Color.LightSteelBlue;
            this.peakMeterL.PeakDecayTimeMS = ((long)(1000));
            this.peakMeterL.Size = new System.Drawing.Size(445, 10);
            this.peakMeterL.TabIndex = 6;
            // 
            // comboBoxAudioDevice
            // 
            this.comboBoxAudioDevice.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxAudioDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAudioDevice.FormattingEnabled = true;
            this.comboBoxAudioDevice.Location = new System.Drawing.Point(9, 97);
            this.comboBoxAudioDevice.Name = "comboBoxAudioDevice";
            this.comboBoxAudioDevice.Size = new System.Drawing.Size(445, 24);
            this.comboBoxAudioDevice.TabIndex = 3;
            this.comboBoxAudioDevice.SelectedIndexChanged += new System.EventHandler(this.comboBoxAudioDevice_SelectedIndexChanged);
            // 
            // labelAudioDevice
            // 
            this.labelAudioDevice.AutoSize = true;
            this.labelAudioDevice.Location = new System.Drawing.Point(6, 77);
            this.labelAudioDevice.Name = "labelAudioDevice";
            this.labelAudioDevice.Size = new System.Drawing.Size(156, 17);
            this.labelAudioDevice.TabIndex = 2;
            this.labelAudioDevice.Text = "Audio device / endpoint";
            // 
            // menuStripMain
            // 
            this.menuStripMain.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.windowToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStripMain.Location = new System.Drawing.Point(0, 0);
            this.menuStripMain.Name = "menuStripMain";
            this.menuStripMain.Size = new System.Drawing.Size(484, 28);
            this.menuStripMain.TabIndex = 3;
            this.menuStripMain.Text = "menuStripMain";
            this.menuStripMain.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.menuStripMain_ItemClicked);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(145, 26);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(145, 26);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // windowToolStripMenuItem
            // 
            this.windowToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.floatOnTopToolStripMenuItem});
            this.windowToolStripMenuItem.Name = "windowToolStripMenuItem";
            this.windowToolStripMenuItem.Size = new System.Drawing.Size(78, 24);
            this.windowToolStripMenuItem.Text = "Window";
            // 
            // floatOnTopToolStripMenuItem
            // 
            this.floatOnTopToolStripMenuItem.Name = "floatOnTopToolStripMenuItem";
            this.floatOnTopToolStripMenuItem.Size = new System.Drawing.Size(173, 26);
            this.floatOnTopToolStripMenuItem.Text = "Float on top";
            this.floatOnTopToolStripMenuItem.Click += new System.EventHandler(this.floatOnTopToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(55, 24);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(133, 26);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // statusStripMain
            // 
            this.statusStripMain.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusIcon,
            this.toolStripStatusMessage,
            this.toolStripStatusLabelMessage});
            this.statusStripMain.Location = new System.Drawing.Point(0, 331);
            this.statusStripMain.Name = "statusStripMain";
            this.statusStripMain.Size = new System.Drawing.Size(484, 26);
            this.statusStripMain.TabIndex = 4;
            this.statusStripMain.Text = "test";
            // 
            // toolStripStatusIcon
            // 
            this.toolStripStatusIcon.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripStatusIcon.Image = ((System.Drawing.Image)(resources.GetObject("toolStripStatusIcon.Image")));
            this.toolStripStatusIcon.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripStatusIcon.Name = "toolStripStatusIcon";
            this.toolStripStatusIcon.Size = new System.Drawing.Size(20, 20);
            this.toolStripStatusIcon.Click += new System.EventHandler(this.toolStripStatusIcon_Click);
            // 
            // toolStripStatusMessage
            // 
            this.toolStripStatusMessage.Name = "toolStripStatusMessage";
            this.toolStripStatusMessage.Size = new System.Drawing.Size(0, 20);
            // 
            // toolStripStatusLabelMessage
            // 
            this.toolStripStatusLabelMessage.Name = "toolStripStatusLabelMessage";
            this.toolStripStatusLabelMessage.Size = new System.Drawing.Size(0, 20);
            // 
            // timerRefresh
            // 
            this.timerRefresh.Enabled = true;
            this.timerRefresh.Interval = 500;
            this.timerRefresh.Tick += new System.EventHandler(this.timerRefresh_Tick);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 357);
            this.Controls.Add(this.statusStripMain);
            this.Controls.Add(this.groupBoxLivesource);
            this.Controls.Add(this.groupBoxDiscordSettings);
            this.Controls.Add(this.menuStripMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStripMain;
            this.MinimumSize = new System.Drawing.Size(310, 190);
            this.Name = "FormMain";
            this.Text = "Discord audio stream";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormMain_FormClosed);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.groupBoxDiscordSettings.ResumeLayout(false);
            this.groupBoxDiscordSettings.PerformLayout();
            this.groupBoxLivesource.ResumeLayout(false);
            this.groupBoxLivesource.PerformLayout();
            this.menuStripMain.ResumeLayout(false);
            this.menuStripMain.PerformLayout();
            this.statusStripMain.ResumeLayout(false);
            this.statusStripMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxDiscordSettings;
        private System.Windows.Forms.GroupBox groupBoxLivesource;
        private System.Windows.Forms.MenuStrip menuStripMain;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStripMain;
        private System.Windows.Forms.ComboBox comboBoxDiscordVoiceChannel;
        private System.Windows.Forms.Label labelDiscordVoiceChannel;
        private System.Windows.Forms.ComboBox comboBoxDiscordServer;
        private System.Windows.Forms.Label labelDiscordServer;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusMessage;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusIcon;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelMessage;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ComboBox comboBoxAudioDevice;
        private System.Windows.Forms.Label labelAudioDevice;
        private PeakMeter peakMeterL;
        private PeakMeter peakMeterR;
        private Scale scale1;
        private System.Windows.Forms.ToolStripMenuItem windowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem floatOnTopToolStripMenuItem;
        private System.Windows.Forms.Timer timerRefresh;
    }
}

