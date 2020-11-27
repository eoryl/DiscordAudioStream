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

    public interface IDiscordConnectionView
    {


        string Status { get; set; }
        ICollection<string> Servers { get; set; }
        ICollection<string> VoiceChannels { get; set; }
        string CurrentServer { get; set; }
        string CurrentVoiceChannel { get; set; }
        string DiscordBotToken { get; set; }
        string AudioContent { get; set; }
        int AudioBitrate { get; set; }

        void DisplayErrorMessage(string errorMessage);
        StatusColourCode StatusColour { set; }

        event EventHandler<string> CurrentServerChanged;
        event EventHandler<string> CurrentVoiceChannelChanged;
        event EventHandler<string> DiscordBotTokenChanged;
        event EventHandler<string> AudioContentChanged;
        event EventHandler<int> AudioBitrateChanged;

    }
}
