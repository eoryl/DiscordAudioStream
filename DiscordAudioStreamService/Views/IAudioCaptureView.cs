using System;
using System.Collections.Generic;

namespace DiscordAudioStream
{

    public class AudioCaptureDeviceInfo
    {
        public string DisplayName
        { 
            get;
            set;
        }
        public string DeviceID
        {
            get;
            set;
        }

        public AudioCaptureDeviceInfo(string name, string id)
        {
            DisplayName = name;
            DeviceID = id;
        }
    }

    public interface IAudioCaptureView
    {
        void SetPeak(float leftChannel, float rightChannel);
        ICollection<AudioCaptureDeviceInfo> AudioDevices { get; set; }
        string SelectedAudioDeviceID { get; set; }
        event EventHandler<string> SelectedAudioDeviceIDChanged;
        void DisplayErrorMessage(string errorMessage);

    }
}
