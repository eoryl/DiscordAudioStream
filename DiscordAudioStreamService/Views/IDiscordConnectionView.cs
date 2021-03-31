using System;
using System.Collections.Generic;

namespace DiscordAudioStream.Views
{
    public enum StatusColourCode
    {
        Red,
        Orange,
        Green,
        Blue
    };


    public class DiscordServerInfo
    {
        public DiscordServerInfo(string name, ulong iD)
        {
            Name = name;
            ID = iD;
        }

        public string Name
        {
            get;
            set;
        }
        public ulong ID
        {
            get;
            set;
        }
        

    }

    public class DiscordVoiceChannelInfo
    {
        public DiscordVoiceChannelInfo(string name, ulong iD, int position, string category)
        {
            Name = name;
            ID = iD;
            Position = position;
            Category = category;
        }

        public string Name
        {
            get;
            set;
        }
        public ulong ID
        {
            get;
            set;
        }
        public int Position
        {
            get;
            set;
        }
        public string Category
        {
            get;
            set;
        }
        public string CompositeName
        {
            get
            {
                return Name + ((Category == null) ? "" : (" (" + Category + ")"));
            }
        }
    }
        public interface IDiscordConnectionView
    {


        string Status { get; set; }
        ICollection<DiscordServerInfo> Servers { get; set; }
        ICollection<DiscordVoiceChannelInfo> VoiceChannels { get; set; }
        ulong CurrentServer { get; set; }
        ulong CurrentVoiceChannel { get; set; }
        string DiscordBotToken { get; set; }
        string AudioContent { get; set; }
        int AudioBitrate { get; set; }

        void DisplayErrorMessage(string errorMessage);
        StatusColourCode StatusColour { set; }

        event EventHandler<ulong> CurrentServerChanged;
        event EventHandler<ulong> CurrentVoiceChannelChanged;
        event EventHandler<string> DiscordBotTokenChanged;
        event EventHandler<string> AudioContentChanged;
        event EventHandler<int> AudioBitrateChanged;

    }
}
