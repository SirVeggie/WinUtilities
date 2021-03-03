using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using WinUtilities.CoreAudio.Constants;
using WinUtilities.CoreAudio.Enumerations;
using WinUtilities.CoreAudio.Externals;
using WinUtilities.CoreAudio.Interfaces;

namespace WinUtilities {
    /// <summary>Class for controlling a specific application's volume</summary>
    /// <remarks>Note: if created by exe name, new audio sources with the same exe name will not be controlled</remarks>
    public class AppVolume : IDisposable {

        private List<ISimpleAudioVolume> audios = new List<ISimpleAudioVolume>();

        /// <summary>Get or set the volume of the included process/processes</summary>
        public float Volume { get => GetVolume(); set => SetVolume(value); }
        /// <summary>Get or set the mute of the included process/processes</summary>
        public bool Mute { get => GetMute(); set => SetMute(value); }

        #region creation
        private AppVolume() { }

        /// <summary>Get app volume controller by process id</summary>
        public static AppVolume New(uint pid) {
            var app = new AppVolume();
            var vol = WinAudio.GetVolumeObject(pid);
            if (vol == null)
                return null;
            app.audios = new List<ISimpleAudioVolume> { vol };
            return app;
        }

        /// <summary>Get app volume controller by exe name</summary>
        public static AppVolume New(string exe) {
            var app = new AppVolume();
            app.audios = WinAudio.GetVolumeObject(exe);
            if (app.audios.Count == 0)
                return null;
            return app;
        }
        #endregion

        /// <summary>Get the volume of the included process/processes</summary>
        public float GetVolume() {
            float min = 1;

            foreach (var ISAV in audios) {
                ISAV.GetMasterVolume(out float vol);
                if (vol < min) {
                    min = vol;
                }
            }

            return min;
        }

        /// <summary>Set the volume of the included process/processes</summary>
        public void SetVolume(float volume) {
            volume = volume > 1 ? 1 : volume < 0 ? 0 : volume;
            foreach (var ISAV in audios) {
                ISAV.SetMasterVolume(volume, Guid.Empty);
            }
        }

        /// <summary>Get the mute of the included process/processes</summary>
        public bool GetMute() {
            foreach (var ISAV in audios) {
                ISAV.GetMute(out bool mute);
                if (!mute) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Set the mute of the included process/processes</summary>
        public void SetMute(bool state) {
            foreach (var ISAV in audios) {
                ISAV.SetMute(state, Guid.Empty);
            }
        }

        /// <summary></summary>
        public void Dispose() {
            foreach (var ISAV in audios) {
                Marshal.ReleaseComObject(ISAV);
            }
        }
    }

    /// <summary>Audio device used to set its volume or default device</summary>
    public class AudioDevice : IDisposable {

        /// <summary>ID of the audio device</summary>
        public string ID { get; }
        /// <summary>Display name of the audio device</summary>
        public string Name => GetName();
        /// <summary>Control the device's master volume</summary>
        public float Volume { get => GetVolume(); set => SetVolume(value); }
        /// <summary>Role of the device when fetched</summary>
        public ERole? PreviousRole;

        /// <summary></summary>
        public AudioDevice(string id, ERole? prevRole = null) {
            ID = id;
            PreviousRole = prevRole;
        }

        /// <summary>Set device as the default device</summary>
        public void SetDefault() {
            throw new NotImplementedException();
        }

        /// <summary>Get device's master volume</summary>
        public float GetVolume() {
            throw new NotImplementedException();
        }

        /// <summary>Set device's master volume</summary>
        public void SetVolume(float level) {
            throw new NotImplementedException();
        }

        private string GetName() {
            throw new NotImplementedException();
        }

        /// <summary></summary>
        public void Dispose() {

        }
    }

    /// <summary>Tools to control audio volume and devices</summary>
    public static class WinAudio {

        /// <summary>Get or set current audio device volume</summary>
        public static float MasterVolume { get => GetMasterVolume(); set => SetMasterVolume(value); }
        /// <summary>Get or set current audio device mute</summary>
        public static bool MasterMute { get => GetMasterMute(); set => SetMasterMute(value); }

        #region events
        private static int eventCount = 0;
        private static IMMNotificationClient notifier;
        private static IMMDeviceEnumerator deviceEnum;

        private static event Action<EDataFlow, ERole, AudioDevice> deviceChanged;
        /// <summary>Subscribe to default audio device change</summary>
        public static event Action<EDataFlow, ERole, AudioDevice> DeviceChanged {
            remove {
                RemoveEvent();
                deviceChanged -= value;
            }

            add {
                AddEvent();
                deviceChanged += value;
            }
        }

        private static event Action<AudioDevice> deviceAdded;
        /// <summary>Subscribe to audio device adding</summary>
        public static event Action<AudioDevice> DeviceAdded {
            remove {
                RemoveEvent();
                deviceAdded -= value;
            }

            add {
                AddEvent();
                deviceAdded += value;
            }
        }

        private static event Action<AudioDevice> deviceRemoved;
        /// <summary>Subscribe to audio device removing</summary>
        public static event Action<AudioDevice> DeviceRemoved {
            remove {
                RemoveEvent();
                deviceRemoved -= value;
            }

            add {
                AddEvent();
                deviceRemoved += value;
            }
        }

        private static event Action<AudioDevice, uint> deviceStateChanged;
        /// <summary>Subscribe to audio device state change</summary>
        public static event Action<AudioDevice, uint> DeviceStateChanged {
            remove {
                RemoveEvent();
                deviceStateChanged -= value;
            }

            add {
                AddEvent();
                deviceStateChanged += value;
            }
        }
        #endregion

        /// <summary>Get app volume controller by process id</summary>
        public static AppVolume GetApp(uint pid) => AppVolume.New(pid);
        /// <summary>Get app volume controller by exe name</summary>
        public static AppVolume GetApp(string exe) => AppVolume.New(exe);

        /// <summary>Check if the specified process is playing audio</summary>
        public static bool HasAudioSource(uint pid) => GetVolumeObject(pid) != null ? true : false;
        /// <summary>Check if any process with a specified exe name is playing audio</summary>
        public static bool HasAudioSource(string exe) => GetVolumeObject(exe).Count > 0 ? true : false;

        #region volume helpers
        private static float GetMasterVolume() {
            IAudioEndpointVolume master = null;

            try {
                master = GetMasterVolumeObject();
                if (master == null) {
                    return -1;
                }

                master.GetMasterVolumeLevelScalar(out float level);
                return level;

            } finally {
                if (master != null) {
                    Marshal.ReleaseComObject(master);
                }
            }
        }

        private static void SetMasterVolume(float volume) {
            volume = volume > 1 ? 1 : volume < 0 ? 0 : volume;
            IAudioEndpointVolume master = null;

            try {
                master = GetMasterVolumeObject();
                if (master == null)
                    return;
                master.SetMasterVolumeLevelScalar(volume, Guid.Empty);

            } finally {
                if (master != null) {
                    Marshal.ReleaseComObject(master);
                }
            }
        }

        private static bool GetMasterMute() {
            IAudioEndpointVolume master = null;

            try {
                master = GetMasterVolumeObject();
                if (master == null)
                    return false;
                master.GetMute(out bool mute);
                return mute;

            } finally {
                if (master != null) {
                    Marshal.ReleaseComObject(master);
                }
            }
        }

        private static void SetMasterMute(bool state) {
            IAudioEndpointVolume master = null;

            try {
                master = GetMasterVolumeObject();
                if (master == null)
                    return;
                master.SetMute(state, Guid.Empty);

            } finally {
                if (master != null) {
                    Marshal.ReleaseComObject(master);
                }
            }
        }

        private static IAudioEndpointVolume GetMasterVolumeObject() {
            IMMDeviceEnumerator enumerator = null;
            IMMDevice speakers = null;

            try {
                enumerator = (IMMDeviceEnumerator) new MMDeviceEnumerator();
                enumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out speakers);

                speakers.Activate(typeof(IAudioEndpointVolume).GUID, 0, IntPtr.Zero, out object o);
                IAudioEndpointVolume master = (IAudioEndpointVolume) o;

                return master;
            } finally {
                if (speakers != null)
                    Marshal.ReleaseComObject(speakers);
                if (enumerator != null)
                    Marshal.ReleaseComObject(enumerator);
            }
        }

        internal static ISimpleAudioVolume GetVolumeObject(uint pid) {
            IMMDeviceEnumerator enumerator = null;
            IAudioSessionEnumerator session = null;
            IAudioSessionManager2 manager = null;
            IMMDevice speakers = null;

            try {
                enumerator = (IMMDeviceEnumerator) new MMDeviceEnumerator();
                enumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out speakers);

                speakers.Activate(typeof(IAudioSessionManager2).GUID, 0, IntPtr.Zero, out object o);
                manager = (IAudioSessionManager2) o;

                manager.GetSessionEnumerator(out session);
                session.GetCount(out int count);

                ISimpleAudioVolume volume = null;

                for (int i = 0; i < count; i++) {
                    IAudioSessionControl2 ctl = null;

                    try {
                        session.GetSession(i, out IAudioSessionControl temp);
                        ctl = (IAudioSessionControl2) temp;
                        ctl.GetProcessId(out uint cpid);

                        if (cpid == pid) {
                            volume = temp as ISimpleAudioVolume;
                            break;
                        }

                    } catch {
                        if (ctl != null)
                            Marshal.ReleaseComObject(ctl);
                    }
                }

                return volume;

            } finally {
                if (session != null)
                    Marshal.ReleaseComObject(session);
                if (manager != null)
                    Marshal.ReleaseComObject(manager);
                if (speakers != null)
                    Marshal.ReleaseComObject(speakers);
                if (enumerator != null)
                    Marshal.ReleaseComObject(enumerator);
            }
        }

        internal static List<ISimpleAudioVolume> GetVolumeObject(string exe) {
            var result = new List<ISimpleAudioVolume>();

            IMMDeviceEnumerator enumerator = null;
            IAudioSessionEnumerator session = null;
            IAudioSessionManager2 manager = null;
            IMMDevice speakers = null;

            try {
                enumerator = (IMMDeviceEnumerator) new MMDeviceEnumerator();
                enumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out speakers);

                Guid IID_IAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;
                speakers.Activate(IID_IAudioSessionManager2, 0, IntPtr.Zero, out object o);
                manager = (IAudioSessionManager2) o;

                manager.GetSessionEnumerator(out session);
                session.GetCount(out int count);

                for (int i = 0; i < count; i++) {
                    IAudioSessionControl2 ctl = null;

                    try {
                        session.GetSession(i, out IAudioSessionControl temp);
                        ctl = (IAudioSessionControl2) temp;
                        ctl.GetProcessId(out uint cpid);

                        if (WinAPI.GetExeNameFromPid(cpid) == exe) {
                            result.Add(temp as ISimpleAudioVolume);
                        } else if (ctl != null) {
                            Marshal.ReleaseComObject(ctl);
                        }

                    } catch {
                        if (ctl != null) {
                            Marshal.ReleaseComObject(ctl);
                        }
                    }
                }

                return result;

            } finally {
                if (session != null)
                    Marshal.ReleaseComObject(session);
                if (manager != null)
                    Marshal.ReleaseComObject(manager);
                if (speakers != null)
                    Marshal.ReleaseComObject(speakers);
                if (enumerator != null)
                    Marshal.ReleaseComObject(enumerator);
            }
        }
        #endregion

        #region device helpers
        private static void AddEvent() {
            if (eventCount == 0)
                RegisterDeviceEvents();
            eventCount++;
        }

        private static void RemoveEvent() {
            eventCount--;
            if (eventCount < 0)
                throw new Exception("Unsubscribed more events than subscribed");
            if (eventCount == 0)
                UnregisterDeviceEvents();
        }

        private static void RegisterDeviceEvents() {
            if (notifier != null)
                throw new Exception("Notified object was not null when registering device events");
            if (deviceEnum != null)
                throw new Exception("Device enumerator was not null when registering device events");
            notifier = new AudioDeviceNotifier();
            deviceEnum = (IMMDeviceEnumerator) new MMDeviceEnumerator();
            deviceEnum.RegisterEndpointNotificationCallback(notifier);
        }

        private static void UnregisterDeviceEvents() {
            if (notifier == null)
                throw new Exception("Notified object was null when unregistering device events");
            if (deviceEnum == null)
                throw new Exception("Device enumerator was null when unregistering device events");
            deviceEnum.UnregisterEndpointNotificationCallback(notifier);
            Marshal.ReleaseComObject(deviceEnum);
            deviceEnum = null;
            notifier = null;
        }

        internal class AudioDeviceNotifier : IMMNotificationClient {
            public void OnDefaultDeviceChanged(EDataFlow dataFlow, ERole deviceRole, string defaultDeviceId) {
                deviceChanged?.Invoke(dataFlow, deviceRole, new AudioDevice(defaultDeviceId));
            }

            public void OnDeviceAdded(string deviceId) {
                deviceAdded?.Invoke(new AudioDevice(deviceId));
            }

            public void OnDeviceRemoved(string deviceId) {
                deviceRemoved?.Invoke(new AudioDevice(deviceId));
            }

            public void OnDeviceStateChanged(string deviceId, uint newState) {
                deviceStateChanged?.Invoke(new AudioDevice(deviceId), newState);
            }

            public void OnPropertyValueChanged(string deviceId, PROPERTYKEY propertyKey) {

            }
        }

        private static void SetDefaultEndpoint(string deviceId) {

        }
        #endregion

        [ComImport]
        [Guid(ComCLSIDs.MMDeviceEnumeratorCLSID)]
        internal class MMDeviceEnumerator { }
    }
}
