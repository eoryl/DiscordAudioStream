using System;
using System.Collections.Generic;

namespace DiscordAudioStream
{
    public interface IAudioCaptureView
    {
        void SetLevel(float leftChannel, float rightChannel);
        ICollection<string> AudioDevices { get; set; }
        string SelectedAudioDevice { get; set; }
        event EventHandler<string> SelectedAudioDeviceChanged;
        void DisplayErrorMessage(string errorMessage);

    }
}
