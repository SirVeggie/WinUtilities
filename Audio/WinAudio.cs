using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using WinUtilities.CoreAudio.Constants;
using WinUtilities.CoreAudio.Enumerations;
using WinUtilities.CoreAudio.Externals;
using WinUtilities.CoreAudio.Interfaces;

namespace WinUtilities {
    /// <summary>Specifies the state of a Windows audio device</summary>
    [Flags]
    public enum AudioDeviceState {
        /// <summary>The device is active</summary>
        Active = 0x1,
        /// <summary>The device is disabled</summary>
        Disabled = 0x2,
        /// <summary>The audio endpoint device is not present because the audio adapter that connects to the endpoint device has been removed or disabled</summary>
        NotPresent = 0x4,
        /// <summary>The device has been unplugged</summary>
        Unplugged = 0x8
    }

    /// <summary>Class for controlling a specific application's volume</summary>
    /// <remarks>Note: if created by exe name, new audio sources with the same exe name will not be controlled</remarks>
    public class AppVolume : IDisposable {

        private List<AudioSource> audios = new List<AudioSource>();

        /// <summary>Get or set the volume of the included process/processes</summary>
        public float Volume { get => GetVolume(); set => SetVolume(value); }
        /// <summary>Get or set the mute of the included process/processes</summary>
        public bool Mute { get => GetMute(); set => SetMute(value); }
        /// <summary>Used by <see cref="AdjustVolume(float)"/>. Threshold under which lessening volume goes straight to 0 and increasing volume goes to at least the threshold.</summary>
        public static float LowThreshold { get; set; } = 0.005f;

        #region creation
        internal AppVolume() { }
        internal AppVolume(ISimpleAudioVolume volume, uint pid) {
            audios = new List<AudioSource> { new AudioSource(volume, pid) };
        }
        internal AppVolume(AudioSource source) {
            audios = new List<AudioSource> { source };
        }

        /// <summary>Get app volume controller by process id</summary>
        public static AppVolume New(uint pid) {
            var vol = WinAudio.GetVolumeObject(pid);
            if (vol == null)
                return null;
            return new AppVolume(vol);
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

            foreach (var source in audios) {
                source.Source.GetMasterVolume(out float vol);
                if (vol < min) {
                    min = vol;
                }
            }

            return min;
        }

        /// <summary>Set the volume of the included process/processes</summary>
        public void SetVolume(float volume) {
            volume = volume > 1 ? 1 : volume < 0 ? 0 : volume;
            foreach (var source in audios) {
                source.Source.SetMasterVolume(volume, Guid.Empty);
            }
        }

        /// <summary>Adjust relative volume using a percentage. 0.1 increases volume by 10% while -0.1 decreases by 10%.</summary>
        /// <remarks>Actual formula is [ current volume * (1 + value) ] for positive and [ current volume / (1 - value) ] for negative.
        /// If volume is LowThreshold or lower while decreasing, volume goes straight to 0. When increasing, new volume is at least the LowThreshold.</remarks>
        public void AdjustVolume(float percentage) {
            var volume = Volume;
            if (percentage > 0) {
                SetVolume(Math.Max(volume * (1 + percentage), LowThreshold));
            } else {
                SetVolume(volume <= LowThreshold ? 0 : volume / (1 - percentage));
            }
        }

        /// <summary>Get the mute of the included process/processes</summary>
        public bool GetMute() {
            foreach (var source in audios) {
                source.Source.GetMute(out bool mute);
                if (!mute) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Set the mute of the included process/processes</summary>
        public void SetMute(bool state) {
            foreach (var source in audios) {
                source.Source.SetMute(state, Guid.Empty);
            }
        }

        /// <summary>Move the audio source to the default device</summary>
        public void SetDeviceDefault() => SetDevice((AudioDevice)null);
        /// <summary>Move the audio source to a different device</summary>
        public void SetDevice(string deviceName) => SetDevice(AudioDevice.FindByName(deviceName));
        /// <summary>Move the audio source to a different device</summary>
        public void SetDevice(AudioDevice device) {
            foreach (var source in audios) {
                WinAudio.SetDefaultEndpoint(device?.ID ?? null, source.Pid, device?.Flow ?? EDataFlow.eRender);
            }
        }

        /// <summary>Find the device that the audio source is using</summary>
        public AudioDevice GetDevice() {
            var ids = audios.Select(x => WinAudio.GetDefaultEnpoint(x.Pid)).ToList();
            if (ids.Count == 0)
                return null;
            if (ids.Any(x => x != ids[0]))
                return null;
            if (string.IsNullOrWhiteSpace(ids[0]))
                return AudioDevice.DefaultOutput;
            Console.WriteLine($"Device: '{ids[0]}'");
            return new AudioDevice(ids[0], null, EDataFlow.eRender);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        private bool _disposed;
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~AppVolume() => Dispose(false);
        protected virtual void Dispose(bool disposing) {
            if (_disposed)
                return;
            _disposed = true;

            foreach (var source in audios) {
                Marshal.ReleaseComObject(source.Source);
            }

            audios = null;
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    /// <summary>Audio device used to set its volume or default device</summary>
    public class AudioDevice {

        /// <summary>ID of the audio device</summary>
        public string ID { get; }
        /// <summary>Display name of the audio device</summary>
        public string Name { get; }
        /// <summary>Control the device's master volume</summary>
        public float Volume { get => GetVolume(); set => SetVolume(value); }
        /// <summary>Audio data flow direction (Playback/Capture)</summary>
        public EDataFlow? Flow { get; }
        /// <summary>Role of the device when fetched</summary>
        public ERole? Role { get; }

        /// <summary>Returns the default speakers</summary>
        public static AudioDevice DefaultOutput => WinAudio.GetDefaultPlayback();
        /// <summary>Returns the default microphone</summary>
        public static AudioDevice DefaultInput => WinAudio.GetDefaultCapture();

        /// <summary></summary>
        internal AudioDevice(string id, string name = null, EDataFlow? flow = null, ERole? prevRole = null) {
            ID = id;
            Flow = flow;
            Role = prevRole;

            if (name == "")
                throw new ArgumentException($"Argument '{nameof(name)}' was empty");
            if (name == null) {
                Name = FindByID(id).Name;
            } else {
                Name = name;
            }
        }

        internal AudioDevice(IMMDevice device, EDataFlow? flow = null, ERole? role = null) {
            device.GetId(out string id);

            ID = id;
            Name = GetName(device);
            Flow = flow;
            Role = role;
        }

        /// <summary>Checks if this audio device still exists</summary>
        public bool Exists() {
            return FindByID(ID) != null;
        }

        /// <summary>Set device as the default device with a specific role</summary>
        public bool SetDefault(ERole role) {
            if (!Exists())
                return false;
            WinAudio.SetDefaultEndpoint(ID, role);
            return true;
        }
        /// <summary>Set device as the default device (all roles)</summary>
        public bool SetDefault() {
            if (!Exists())
                return false;
            WinAudio.SetDefaultEndpoint(ID, ERole.eConsole, ERole.eCommunications);
            return true;
        }

        /// <summary>Get device's master volume</summary>
        public float GetVolume() {
            IMMDevice device = null;
            IAudioEndpointVolume vol = null;

            try {
                device = GetDevice();
                device.Activate(Guid.Parse(ComIIDs.IAudioEndpointVolumeIID), 0, IntPtr.Zero, out object o);
                vol = (IAudioEndpointVolume)o;
                if (vol == null) throw new Exception("Failed to get IAudioEndpointVolume object");
                vol.GetMasterVolumeLevelScalar(out float level);
                return level;

            } finally {
                if (device != null)
                    Marshal.ReleaseComObject(device);
            }
        }

        /// <summary>Set device's master volume</summary>
        /// <returns>True if successful</returns>
        public bool SetVolume(float level) {
            IMMDevice device = null;
            IAudioEndpointVolume vol = null;

            try {
                device = GetDevice();
                device.Activate(Guid.Parse(ComIIDs.IAudioEndpointVolumeIID), 0, IntPtr.Zero, out object o);
                vol = (IAudioEndpointVolume)o;
                if (vol == null) return false;
                return vol.SetMasterVolumeLevelScalar(level, Guid.Empty) == 0;

            } catch {
                return false;

            } finally {
                if (device != null)
                    Marshal.ReleaseComObject(device);
                if (vol != null)
                    Marshal.ReleaseComObject(vol);
            }
        }

        #region helpers
        internal IMMDevice GetDevice() {
            IMMDeviceEnumerator enumerator = null;
            IMMDevice device = null;

            try {
                enumerator = (IMMDeviceEnumerator)new WinAudio.MMDeviceEnumerator();
                enumerator.GetDevice(ID, out device);
                return device;
            } finally {
                if (enumerator != null)
                    Marshal.ReleaseComObject(enumerator);
            }
        }

        internal static IMMDevice GetDevice(string id) {
            IMMDeviceEnumerator enumerator = null;

            try {
                enumerator = (IMMDeviceEnumerator)new WinAudio.MMDeviceEnumerator();
                enumerator.GetDevice(id, out var device);
                return device;
            } finally {
                if (enumerator != null)
                    Marshal.ReleaseComObject(enumerator);
            }
        }

        internal static string GetName(IMMDevice device) {
            IPropertyStore store = null;

            try {
                Marshal.ThrowExceptionForHR(device.OpenPropertyStore(0, out store));
                PROPERTYKEY key = new PROPERTYKEY { fmtid = Guid.Parse("A45C254E-DF1C-4EFD-8020-67D146A850E0"), pid = 14 };
                Marshal.ThrowExceptionForHR(store.GetValue(ref key, out var value));
                return Marshal.PtrToStringAuto(value.Data.AsStringPtr);
            } finally {
                if (store != null)
                    Marshal.ReleaseComObject(store);
            }
        }
        #endregion

        #region find
        /// <summary>Find a device by its ID</summary>
        public static AudioDevice FindByID(string id) {
            IMMDeviceEnumerator enumerator = null;
            IMMDevice device = null;

            try {
                enumerator = (IMMDeviceEnumerator)new WinAudio.MMDeviceEnumerator();
                enumerator.GetDevice(id, out device);
                if (device == null)
                    return null;
                return new AudioDevice(device);
            } finally {
                if (enumerator != null)
                    Marshal.ReleaseComObject(enumerator);
                if (device != null)
                    Marshal.ReleaseComObject(device);
            }
        }

        /// <summary>Find a device by its name</summary>
        public static AudioDevice FindByName(string name) {
            IMMDeviceEnumerator enumerator = null;
            IMMDevice device = null;
            IMMDeviceCollection collection = null;

            try {
                enumerator = (IMMDeviceEnumerator)new WinAudio.MMDeviceEnumerator();
                enumerator.EnumAudioEndpoints(EDataFlow.eAll, 1, out collection);
                collection.GetCount(out uint count);

                for (uint i = 0; i < count; i++) {
                    if (collection.Item(i, out device) == 0) {
                        string temp = GetName(device);
                        if (temp.Contains(name)) {
                            device.GetId(out string id);
                            return new AudioDevice(id, temp);
                        } else {
                            Marshal.ReleaseComObject(device);
                            device = null;
                        }
                    }
                }

                return null;
            } finally {
                if (enumerator != null)
                    Marshal.ReleaseComObject(enumerator);
                if (device != null)
                    Marshal.ReleaseComObject(device);
                if (collection != null)
                    Marshal.ReleaseComObject(collection);
            }
        }
        #endregion

        #region operators
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool operator ==(AudioDevice a, AudioDevice b) => (a is null && b is null) || !(a is null) && !(b is null) && a.ID == b.ID;
        public static bool operator !=(AudioDevice a, AudioDevice b) => !(a == b);
        public override bool Equals(object obj) => obj is AudioDevice device && this == device;
        public override int GetHashCode() => 1213502048 + ID.GetHashCode();
        public override string ToString() => $"{{AudioDevice: {Name}}}";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #endregion
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

        /// <summary>Find an audio device by its full or partial name</summary>
        public static AudioDevice GetDevice(string name) => AudioDevice.FindByName(name);

        /// <summary>Get the default playback audio device</summary>
        public static AudioDevice GetDefaultPlayback() => GetDefaultDevice(EDataFlow.eRender);
        /// <summary>Get the default microphone</summary>
        public static AudioDevice GetDefaultCapture() => GetDefaultDevice(EDataFlow.eCapture);
        private static AudioDevice GetDefaultDevice(EDataFlow flow) {
            ERole role = ERole.eConsole;
            IMMDeviceEnumerator enumerator = null;
            IMMDevice device = null;

            try {
                enumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
                enumerator.GetDefaultAudioEndpoint(flow, role, out device);
                return new AudioDevice(device, flow, role);
            } finally {
                if (enumerator != null)
                    Marshal.ReleaseComObject(enumerator);
                if (device != null)
                    Marshal.ReleaseComObject(device);
            }
        }

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
                enumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
                enumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out speakers);

                speakers.Activate(typeof(IAudioEndpointVolume).GUID, 0, IntPtr.Zero, out object o);
                IAudioEndpointVolume master = (IAudioEndpointVolume)o;

                return master;
            } finally {
                if (speakers != null)
                    Marshal.ReleaseComObject(speakers);
                if (enumerator != null)
                    Marshal.ReleaseComObject(enumerator);
            }
        }

        internal static AudioSource GetVolumeObject(uint pid) {
            IMMDeviceEnumerator enumerator = null;
            IAudioSessionEnumerator session = null;
            IAudioSessionManager2 manager = null;
            IMMDevice speakers = null;

            try {
                enumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
                string id = AudioUtils.GetDefaultEndPoint(pid, EDataFlow.eRender);
                if (string.IsNullOrEmpty(id))
                    enumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out speakers);
                else
                    enumerator.GetDevice(id, out speakers);

                if (speakers == null)
                    return null;
                speakers.Activate(typeof(IAudioSessionManager2).GUID, 0, IntPtr.Zero, out object o);
                manager = (IAudioSessionManager2)o;

                manager.GetSessionEnumerator(out session);
                session.GetCount(out int count);

                ISimpleAudioVolume volume = null;

                for (int i = 0; i < count; i++) {
                    IAudioSessionControl2 ctl = null;

                    try {
                        session.GetSession(i, out IAudioSessionControl temp);
                        ctl = (IAudioSessionControl2)temp;
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

                return volume == null ? null : new AudioSource(volume, pid);

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

        internal static List<AudioSource> GetVolumeObject(string exe) {
            var result = new List<AudioSource>();

            IMMDeviceEnumerator enumerator = null;
            IMMDeviceCollection collection = null;
            IAudioSessionEnumerator session = null;
            IAudioSessionManager2 manager = null;
            IMMDevice speakers = null;

            try {
                enumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
                enumerator.EnumAudioEndpoints(EDataFlow.eRender, DEVICE_STATE_XXX.DEVICE_STATE_ACTIVE, out collection);
                collection.GetCount(out uint count);

                for (uint i = 0; i < count; i++) {
                    if (collection.Item(i, out speakers) != 0)
                        continue;
                    Guid IID_IAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;
                    speakers.Activate(IID_IAudioSessionManager2, 0, IntPtr.Zero, out object o);
                    manager = (IAudioSessionManager2)o;

                    manager.GetSessionEnumerator(out session);
                    session.GetCount(out int count2);

                    for (int ii = 0; ii < count2; ii++) {
                        IAudioSessionControl2 ctl = null;

                        try {
                            session.GetSession(ii, out IAudioSessionControl temp);
                            ctl = (IAudioSessionControl2)temp;
                            ctl.GetProcessId(out uint cpid);

                            if (WinAPI.GetExeNameFromPid(cpid) == exe) {
                                result.Add(new AudioSource(temp as ISimpleAudioVolume, cpid));
                            } else if (ctl != null) {
                                Marshal.ReleaseComObject(ctl);
                            }

                        } catch {
                            if (ctl != null) {
                                Marshal.ReleaseComObject(ctl);
                            }
                        }
                    }

                    Marshal.ReleaseComObject(speakers);
                    speakers = null;
                    Marshal.ReleaseComObject(manager);
                    manager = null;
                    Marshal.ReleaseComObject(session);
                    session = null;
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
                if (collection != null)
                    Marshal.ReleaseComObject(collection);
            }
        }

        /// <summary>Retrieve a list of all current audio sources and their related data</summary>
        public static List<AudioInfo> ListAudioSources() {
            List<AudioInfo> result = new List<AudioInfo>();

            IMMDeviceEnumerator enumerator = null;
            IMMDeviceCollection collection = null;
            IAudioSessionEnumerator session = null;
            IAudioSessionManager2 manager = null;
            IMMDevice speakers = null;

            try {
                enumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
                enumerator.EnumAudioEndpoints(EDataFlow.eRender, DEVICE_STATE_XXX.DEVICE_STATE_ACTIVE, out collection);
                collection.GetCount(out uint count);

                for (uint i = 0; i < count; i++) {
                    if (collection.Item(i, out speakers) != 0)
                        continue;
                    Guid IID_IAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;
                    speakers.Activate(IID_IAudioSessionManager2, 0, IntPtr.Zero, out object o);
                    manager = (IAudioSessionManager2)o;

                    manager.GetSessionEnumerator(out session);
                    session.GetCount(out int count2);

                    for (int ii = 0; ii < count2; ii++) {
                        IAudioSessionControl2 ctl = null;

                        try {
                            session.GetSession(ii, out IAudioSessionControl temp);
                            ctl = (IAudioSessionControl2)temp;
                            ctl.GetProcessId(out uint pid);

                            AudioDevice device = new AudioDevice(speakers, EDataFlow.eRender);
                            AppVolume controller = new AppVolume(temp as ISimpleAudioVolume, pid);
                            result.Add(new AudioInfo(device, controller, pid));

                        } catch {
                            if (ctl != null) {
                                Marshal.ReleaseComObject(ctl);
                            }
                        }
                    }

                    Marshal.ReleaseComObject(speakers);
                    speakers = null;
                    Marshal.ReleaseComObject(manager);
                    manager = null;
                    Marshal.ReleaseComObject(session);
                    session = null;
                }

                return result;

            } finally {
                if (enumerator != null)
                    Marshal.ReleaseComObject(enumerator);
                if (collection != null)
                    Marshal.ReleaseComObject(collection);
                if (session != null)
                    Marshal.ReleaseComObject(session);
                if (manager != null)
                    Marshal.ReleaseComObject(manager);
                if (speakers != null)
                    Marshal.ReleaseComObject(speakers);
            }
        }

        /// <summary>Collection of info about an audio source</summary>
        public class AudioInfo {
            /// <summary>The device this audio source is playing on</summary>
            public AudioDevice Device { get; }
            /// <summary>Audio source controller</summary>
            public AppVolume Controller { get; }
            /// <summary>Executable name of this audio source's process</summary>
            public string Exe { get; }
            /// <summary>Process ID of this audio source's process</summary>
            public uint Pid { get; }

            /// <summary></summary>
            public AudioInfo(AudioDevice device, AppVolume controller, uint pid) {
                Exe = WinAPI.GetExeNameFromPid(pid);
                Controller = controller;
                Pid = pid;
            }
        }
        #endregion

        #region device helpers
        internal static void SetDefaultEndpoint(string deviceId, params ERole[] roles) {
            IPolicyConfig config = null;

            try {
                config = (IPolicyConfig)new CPolicyConfigVistaClient();
                foreach (var role in roles) {
                    HRESULT hr = config.SetDefaultEndpoint(deviceId, role);
                    if (hr != HRESULT.S_OK)
                        throw new ExternalException($"SetDefaultEndpoint failed with code {hr}");
                }
            } finally {
                if (config != null)
                    Marshal.ReleaseComObject(config);
            }
        }

        internal static void SetDefaultEndpoint(string deviceId, uint processId, EDataFlow flow = EDataFlow.eRender) {
            AudioUtils.SetDefaultEndpoint(deviceId, processId, flow);
        }

        internal static string GetDefaultEnpoint(uint processId, EDataFlow flow = EDataFlow.eRender) {
            return AudioUtils.GetDefaultEndPoint(processId, flow);
        }
        #endregion

        #region device events
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
            deviceEnum = (IMMDeviceEnumerator)new MMDeviceEnumerator();
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
        #endregion

        [ComImport, Guid(ComCLSIDs.MMDeviceEnumeratorCLSID)]
        internal class MMDeviceEnumerator { }

        #region policy config
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        [ComImport, Guid("870AF99C-171D-4F9E-AF0D-E63DF40C2BC9")]
        internal class CPolicyConfigVistaClient { }

        [ComImport]
        [Guid("f8679f50-850a-41cf-9c72-430f290290c8")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IPolicyConfig {
            HRESULT GetMixFormat([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, out WAVEFORMATEXTENSIBLE ppFormat);
            HRESULT GetDeviceFormat([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, [In][MarshalAs(UnmanagedType.Bool)] bool bDefault, out WAVEFORMATEXTENSIBLE ppFormat);
            HRESULT ResetDeviceFormat([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName);
            HRESULT SetDeviceFormat([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, [In][MarshalAs(UnmanagedType.LPStruct)] WAVEFORMATEXTENSIBLE pEndpointFormat, [In][MarshalAs(UnmanagedType.LPStruct)] WAVEFORMATEXTENSIBLE pMixFormat);
            HRESULT GetProcessingPeriod([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, [In][MarshalAs(UnmanagedType.Bool)] bool bDefault, out Int64 pmftDefaultPeriod, out Int64 pmftMinimumPeriod);
            HRESULT SetProcessingPeriod([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, Int64 pmftPeriod);
            HRESULT GetShareMode([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, out DeviceShareMode pMode);
            HRESULT SetShareMode([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, [In] DeviceShareMode mode);
            HRESULT GetPropertyValue([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, [In][MarshalAs(UnmanagedType.Bool)] bool bFxStore, ref PROPERTYKEY pKey, out PROPVARIANT pv);
            HRESULT SetPropertyValue([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, [In][MarshalAs(UnmanagedType.Bool)] bool bFxStore, [In] ref PROPERTYKEY pKey, ref PROPVARIANT pv);
            HRESULT SetDefaultEndpoint([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, [In][MarshalAs(UnmanagedType.U4)] ERole role);
            HRESULT SetEndpointVisibility([In][MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, [In][MarshalAs(UnmanagedType.Bool)] bool bVisible);
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #endregion
    }

    internal class AudioSource {
        internal ISimpleAudioVolume Source { get; }
        internal uint Pid { get; }

        internal AudioSource(ISimpleAudioVolume source, uint pid) {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            Source = source;
            Pid = pid;
        }
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    internal class WAVEFORMATEXTENSIBLE : WAVEFORMATEX {
        [FieldOffset(0)]
        public short wValidBitsPerSample;
        [FieldOffset(0)]
        public short wSamplesPerBlock;
        [FieldOffset(0)]
        public short wReserved;
        [FieldOffset(2)]
        public WaveMask dwChannelMask;
        [FieldOffset(6)]
        public Guid SubFormat;
    }

    [Flags]
    internal enum WaveMask {
        None = 0x0,
        FrontLeft = 0x1,
        FrontRight = 0x2,
        FrontCenter = 0x4,
        LowFrequency = 0x8,
        BackLeft = 0x10,
        BackRight = 0x20,
        FrontLeftOfCenter = 0x40,
        FrontRightOfCenter = 0x80,
        BackCenter = 0x100,
        SideLeft = 0x200,
        SideRight = 0x400,
        TopCenter = 0x800,
        TopFrontLeft = 0x1000,
        TopFrontCenter = 0x2000,
        TopFrontRight = 0x4000,
        TopBackLeft = 0x8000,
        TopBackCenter = 0x10000,
        TopBackRight = 0x20000
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal class WAVEFORMATEX {
        public short wFormatTag;
        public short nChannels;
        public int nSamplesPerSec;
        public int nAvgBytesPerSec;
        public short nBlockAlign;
        public short wBitsPerSample;
        public short cbSize;
    }

    internal enum DeviceShareMode {
        Shared,
        Exclusive
    }

    internal enum HRESULT : int {
        S_OK = 0,
        S_FALSE = 1,
        E_NOINTERFACE = unchecked((int)0x80004002),
        E_NOTIMPL = unchecked((int)0x80004001),
        E_FAIL = unchecked((int)0x80004005),
        E_UNEXPECTED = unchecked((int)0x8000FFFF)
    }
}
