using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using WinUtilities.CoreAudio.Enumerations;

namespace WinUtilities {

    /*
    Most of this code is ripped directly out of the EarTrumpet project,
    so credit goes to the creators:

    David Golden
    Rafael Rivera
    Dave Amenta

    The MIT License (MIT)

    Copyright (c) 2015

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
    */

    internal static class AudioUtils {

        private const string DEVINTERFACE_AUDIO_RENDER = "#{e6327cad-dcec-4949-ae8a-991e976a79d2}";
        private const string DEVINTERFACE_AUDIO_CAPTURE = "#{2eef81be-33fa-4800-9670-1cd474972c3f}";
        private const string MMDEVAPI_TOKEN = @"\\?\SWD#MMDEVAPI#";

        private static IAudioPolicyConfigFactory _sharedPolicyConfig;

        private static void EnsurePolicyConfig() {
            if (_sharedPolicyConfig == null)
                _sharedPolicyConfig = AudioPolicyConfigFactory.Create();
        }

        internal static void SetDefaultEndpoint(string deviceId, uint processId, EDataFlow flow) {
            try {
                EnsurePolicyConfig();
                IntPtr hstring = IntPtr.Zero;

                if (!string.IsNullOrWhiteSpace(deviceId)) {
                    var str = GenerateDeviceId(deviceId, flow);
                    Combase.WindowsCreateString(str, (uint)str.Length, out hstring);
                }

                _sharedPolicyConfig.SetPersistedDefaultAudioEndpoint(processId, flow, ERole.eMultimedia, hstring);
                _sharedPolicyConfig.SetPersistedDefaultAudioEndpoint(processId, flow, ERole.eConsole, hstring);
            } catch (Exception ex) {
                Console.WriteLine($"{ex}");
            }
        }

        internal static string GetDefaultEndPoint(uint processId, EDataFlow flow) {
            try {
                EnsurePolicyConfig();

                _sharedPolicyConfig.GetPersistedDefaultAudioEndpoint(processId, flow, ERole.eMultimedia | ERole.eConsole, out string deviceId);
                return UnpackDeviceId(deviceId);
            } catch (Exception ex) {
                Console.WriteLine($"{ex}");
            }

            Console.WriteLine("Error: AudioUtils.GetDefaultEndPoint Returned null device id");
            return null;
        }

        internal static string GenerateDeviceId(string deviceId, EDataFlow flow) {
            return $"{MMDEVAPI_TOKEN}{deviceId}{(flow == EDataFlow.eRender ? DEVINTERFACE_AUDIO_RENDER : DEVINTERFACE_AUDIO_CAPTURE)}";
        }

        internal static string UnpackDeviceId(string deviceId) {
            if (deviceId.StartsWith(MMDEVAPI_TOKEN)) deviceId = deviceId.Remove(0, MMDEVAPI_TOKEN.Length);
            if (deviceId.EndsWith(DEVINTERFACE_AUDIO_RENDER)) deviceId = deviceId.Remove(deviceId.Length - DEVINTERFACE_AUDIO_RENDER.Length);
            if (deviceId.EndsWith(DEVINTERFACE_AUDIO_CAPTURE)) deviceId = deviceId.Remove(deviceId.Length - DEVINTERFACE_AUDIO_CAPTURE.Length);
            return deviceId;
        }
    }

    #region helpers
    internal static class Combase {
        [DllImport("combase.dll", PreserveSig = false)]
        public static extern void RoGetActivationFactory(
            [MarshalAs(UnmanagedType.HString)] string activatableClassId,
            [In] ref Guid iid,
            [Out, MarshalAs(UnmanagedType.IInspectable)] out Object factory);

        [DllImport("combase.dll", PreserveSig = false)]
        public static extern void WindowsCreateString(
            [MarshalAs(UnmanagedType.LPWStr)] string src,
            [In] uint length,
            [Out] out IntPtr hstring);
    }

    internal class AudioPolicyConfigFactory {
        internal static IAudioPolicyConfigFactory Create() {
            if (Environment.OSVersion.IsAtLeast(OSVersions.Version21H2)) {
                return new AudioPolicyConfigFactoryImplFor21H2();
            } else {
                return new AudioPolicyConfigFactoryImplForDownlevel();
            }
        }
    }

    internal class AudioPolicyConfigFactoryImplFor21H2 : IAudioPolicyConfigFactory {
        private readonly IAudioPolicyConfigFactoryVariantFor21H2 _factory;

        internal AudioPolicyConfigFactoryImplFor21H2() {
            var iid = typeof(IAudioPolicyConfigFactoryVariantFor21H2).GUID;
            Combase.RoGetActivationFactory("Windows.Media.Internal.AudioPolicyConfig", ref iid, out object factory);
            _factory = (IAudioPolicyConfigFactoryVariantFor21H2)factory;
        }

        public HRESULT ClearAllPersistedApplicationDefaultEndpoints() {
            return _factory.ClearAllPersistedApplicationDefaultEndpoints();
        }

        public HRESULT GetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, out string deviceId) {
            return _factory.GetPersistedDefaultAudioEndpoint(processId, flow, role, out deviceId);
        }

        public HRESULT SetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, IntPtr deviceId) {
            return _factory.SetPersistedDefaultAudioEndpoint(processId, flow, role, deviceId);
        }
    }

    internal class AudioPolicyConfigFactoryImplForDownlevel : IAudioPolicyConfigFactory {
        private readonly IAudioPolicyConfigFactoryVariantForDownlevel _factory;

        internal AudioPolicyConfigFactoryImplForDownlevel() {
            var iid = typeof(IAudioPolicyConfigFactoryVariantForDownlevel).GUID;
            Combase.RoGetActivationFactory("Windows.Media.Internal.AudioPolicyConfig", ref iid, out object factory);
            _factory = (IAudioPolicyConfigFactoryVariantForDownlevel)factory;
        }

        public HRESULT ClearAllPersistedApplicationDefaultEndpoints() {
            return _factory.ClearAllPersistedApplicationDefaultEndpoints();
        }

        public HRESULT GetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, out string deviceId) {
            return _factory.GetPersistedDefaultAudioEndpoint(processId, flow, role, out deviceId);
        }

        public HRESULT SetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, IntPtr deviceId) {
            return _factory.SetPersistedDefaultAudioEndpoint(processId, flow, role, deviceId);
        }
    }

    internal interface IAudioPolicyConfigFactory {
        HRESULT SetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, IntPtr deviceId);
        HRESULT GetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, out string deviceId);
        HRESULT ClearAllPersistedApplicationDefaultEndpoints();
    }

    [Guid("ab3d4648-e242-459f-b02f-541c70306324")]
    [InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
    internal interface IAudioPolicyConfigFactoryVariantFor21H2 {
        int __incomplete__add_CtxVolumeChange();
        int __incomplete__remove_CtxVolumeChanged();
        int __incomplete__add_RingerVibrateStateChanged();
        int __incomplete__remove_RingerVibrateStateChange();
        int __incomplete__SetVolumeGroupGainForId();
        int __incomplete__GetVolumeGroupGainForId();
        int __incomplete__GetActiveVolumeGroupForEndpointId();
        int __incomplete__GetVolumeGroupsForEndpoint();
        int __incomplete__GetCurrentVolumeContext();
        int __incomplete__SetVolumeGroupMuteForId();
        int __incomplete__GetVolumeGroupMuteForId();
        int __incomplete__SetRingerVibrateState();
        int __incomplete__GetRingerVibrateState();
        int __incomplete__SetPreferredChatApplication();
        int __incomplete__ResetPreferredChatApplication();
        int __incomplete__GetPreferredChatApplication();
        int __incomplete__GetCurrentChatApplications();
        int __incomplete__add_ChatContextChanged();
        int __incomplete__remove_ChatContextChanged();
        [PreserveSig]
        HRESULT SetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, IntPtr deviceId);
        [PreserveSig]
        HRESULT GetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, [Out, MarshalAs(UnmanagedType.HString)] out string deviceId);
        [PreserveSig]
        HRESULT ClearAllPersistedApplicationDefaultEndpoints();
    }

    [Guid("2a59116d-6c4f-45e0-a74f-707e3fef9258")]
    [InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
    internal interface IAudioPolicyConfigFactoryVariantForDownlevel {
        int __incomplete__add_CtxVolumeChange();
        int __incomplete__remove_CtxVolumeChanged();
        int __incomplete__add_RingerVibrateStateChanged();
        int __incomplete__remove_RingerVibrateStateChange();
        int __incomplete__SetVolumeGroupGainForId();
        int __incomplete__GetVolumeGroupGainForId();
        int __incomplete__GetActiveVolumeGroupForEndpointId();
        int __incomplete__GetVolumeGroupsForEndpoint();
        int __incomplete__GetCurrentVolumeContext();
        int __incomplete__SetVolumeGroupMuteForId();
        int __incomplete__GetVolumeGroupMuteForId();
        int __incomplete__SetRingerVibrateState();
        int __incomplete__GetRingerVibrateState();
        int __incomplete__SetPreferredChatApplication();
        int __incomplete__ResetPreferredChatApplication();
        int __incomplete__GetPreferredChatApplication();
        int __incomplete__GetCurrentChatApplications();
        int __incomplete__add_ChatContextChanged();
        int __incomplete__remove_ChatContextChanged();
        [PreserveSig]
        HRESULT SetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, IntPtr deviceId);
        [PreserveSig]
        HRESULT GetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, [Out, MarshalAs(UnmanagedType.HString)] out string deviceId);
        [PreserveSig]
        HRESULT ClearAllPersistedApplicationDefaultEndpoints();
    }

    internal enum OSVersions : int {
        RS3 = 16299,
        RS4 = 17134,
        RS5_1809 = 17763,
        Version21H2 = 21390,
        Windows11 = 22000,
    }

    internal static class OperatingSystemExtensions {
        internal static bool IsAtLeast(this OperatingSystem os, OSVersions version) {
            return os.Version.Build >= (int)version;
        }

        internal static bool IsGreaterThan(this OperatingSystem os, OSVersions version) {
            return os.Version.Build > (int)version;
        }

        internal static bool IsLessThan(this OperatingSystem os, OSVersions version) {
            return os.Version.Build < (int)version;
        }
    }
    #endregion
}
