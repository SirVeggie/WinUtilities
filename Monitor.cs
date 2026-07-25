using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace WinUtilities {
    /// <summary>Class for the retrieving of info and the control of monitors</summary>
    [DataContract]
    public class Monitor {

        #region properties
        /// <summary>The device name of the monitor</summary>
        [DataMember]
        public string Name { get; private set; }
        /// <summary>Check if the monitor is the primary monitor</summary>
        [DataMember]
        public bool IsPrimary { get; private set; }
        /// <summary>The area of the monitor</summary>
        [DataMember]
        public Area Area { get; private set; }
        /// <summary>The work area of the monitor, excludes the taskbar</summary>
        [DataMember]
        public Area WorkArea { get; private set; }
        /// <summary>Handle to the monitor</summary>
        [DataMember]
        public IntPtr Handle { get; private set; }

        /// <summary>Check if the monitor is in portrait mode instead of landscape</summary>
        public bool IsPortrait => Area.W < Area.H;
        /// <summary>Get the monitor's area as an image</summary>
        public Image Image => GetImage(Area);
        /// <summary>Get the scaling factor set by the user in the display settings</summary>
        public int Scale => GetMonitorScale(Handle);

        /// <summary>Retrieve the current primary monitor</summary>
        public static Monitor Primary => FromPoint(0, 0, MonitorDefault.Primary);
        /// <summary>Get the total screen area</summary>
        public static Area Screen => GetScreenArea();
        /// <summary>The amount of current monitors</summary>
        public static int Count => WinAPI.GetSystemMetrics(WinAPI.SM.CMONITORS);
        /// <summary>Get the entire screen as an image</summary>
        public static Image ScreenImage => GetImage(Screen);
        #endregion

        #region constructors
        /// <summary>Create a new monitor object</summary>
        public Monitor(string name, bool isPrimary, IntPtr handle, Area area, Area workarea) {
            Name = name;
            IsPrimary = isPrimary;
            Handle = handle;
            Area = area;
            WorkArea = workarea;
        }
        #endregion

        #region static
        /// <summary>Find the monitor the that contains the specified point</summary>
        public static Monitor FromPoint(int x, int y, MonitorDefault def = MonitorDefault.Nearest) => GetMonitor(HandleFromPoint(x, y, def));
        /// <summary>Find the monitor the that contains the specified point</summary>
        public static Monitor FromPoint(Coord point, MonitorDefault def = MonitorDefault.Nearest) => GetMonitor(HandleFromPoint(point, def));
        /// <summary>Find the monitor the mouse is on currently</summary>
        public static Monitor FromMouse() => FromPoint(Mouse.Position);
        /// <summary>Find the monitor the specified window is on currently</summary>
        public static Monitor FromWindow(Window win = null, MonitorDefault def = MonitorDefault.Nearest) => GetMonitor(HandleFromWindow(win, def));
        /// <summary>Find the best fitting monitor for the specified area</summary>
        public static Monitor FromArea(Area area, MonitorDefault def = MonitorDefault.Nearest) => GetMonitor(HandleFromArea(area, def));
        /// <summary>Find a monitor with a specific index. Don't rely on the index staying the same between restarts or monitor disconnects.</summary>
        public static Monitor FromIndex(int index) => GetMonitor(HandleFromIndex(index));

        /// <summary>Sets the monitors into a 'sleep' state, any user activity wakes them up</summary>
        public static void SetIdle(bool state) => Window.Find("Program Manager").PostMessage(WM.SYSCOMMAND, 0x170, state ? 2 : -1);

        /// <summary>Retrieve an image from the current screen</summary>
        public static Image GetImage(Area area) {
            Image img = new Bitmap(area.IntW, area.IntH);
            Graphics g = Graphics.FromImage(img);
            g.CopyFromScreen(area, Point.Empty, area);
            g.Dispose();
            return img;
        }
        #endregion

        #region special features
        /// <summary>Set Windows wallpaper for each monitor separately</summary>
        /// <param name="fill">if true, images are cropped to fill the entire monitor, otherwise black bars are shown if the aspect ratio doesn't match</param>
        /// <param name="filePaths">list of image paths</param>
        /// <returns>True if successful</returns>
        public static bool SetWallpapers(bool fill, params string[] filePaths) {
            Area screen = Screen;
            Bitmap wallpaper = new Bitmap(screen.IntW, screen.IntH);
            List<Area> monitors = GetMonitors().Select(x => x.Area).ToList();

            if (filePaths.Length != monitors.Count) {
                throw new ArgumentException("Number of images did not match the number of monitors");
            }

            int wOffset = 0;
            int hOffset = 0;
            foreach (Area monitor in monitors) {
                wOffset = Math.Min(wOffset, monitor.IntX);
                hOffset = Math.Min(hOffset, monitor.IntY);
            }

            if (wOffset + hOffset != 0) {
                monitors = monitors.Select(x => x.AddX(-wOffset).AddY(-hOffset)).ToList();
            }

            using (Graphics g = Graphics.FromImage(wallpaper)) {
                for (int i = 0; i < filePaths.Length; i++) {
                    Area m = monitors[i];
                    Image img = Image.FromFile(filePaths[i]);
                    Area target = new Area(0, 0, img.Width, img.Height);
                    Area source = target;
                    target = fill ? target.Fill(m) : target.Fit(m);
                    if (fill) {
                        source = m.Fit(source);
                        target = m;
                    }
                    g.DrawImage(img, target, source, GraphicsUnit.Pixel);
                    //g.DrawImage(img, target.IntX, target.IntY, target.IntW, target.IntH);
                    img.Dispose();
                }
            }

            return WinAPI.SetWallpaper(wallpaper, WinAPI.WallpaperStyle.Tile);
        }
        #endregion

        /// <summary>Find out the current index of this monitor</summary>
        public int GetIndex() {
            var list = GetMonitors();
            for (int i = 0; i < list.Count; i++) {
                if (list[i] == this) {
                    return i;
                }
            }

            throw new Exception("Monitor not found, was it disconnected from the computer?");
        }

        /// <summary>Get the next monitor that is <paramref name="steps"/> forward from current index while looping around</summary>
        public Monitor Next(int steps = 1) {
            if (steps < 1)
                throw new ArgumentException("Invalid step value, must be 1 or higher");
            int index = (GetIndex() + steps) % Count;
            return FromIndex(index);
        }

        /// <summary>Get the previous monitor that is <paramref name="steps"/> behind from current index while looping around</summary>
        public Monitor Previous(int steps = 1) {
            if (steps < 1)
                throw new ArgumentException("Invalid step value, must be 1 or higher");
            int index = (Count + (GetIndex() - steps) % Count) % Count;
            return FromIndex(index);
        }

        /// <summary>Set the work area of a monitor</summary>
        /// <remarks>This represents the area to which windows are maximized to. Work area usually excludes the taskbar, but this can change that.</remarks>
        public Monitor SetWorkArea(Area area) {
            WinAPI.SetWorkArea(area);
            return this;
        }

        /// <summary>Checks if a monitor by this handle still exists</summary>
        /// <returns>True if monitor exists</returns>
        public bool Exists() {
            return GetMonitor(Handle) != null;
        }

        /// <summary>Set as the current primary monitor</summary>
        /// <returns>True if successful</returns>
        public bool SetPrimary() {
            if (IsPrimary) {
                Console.WriteLine("Monitor already primary");
                return false;
            }
            return SetPrimaryMonitor(Name);
        }

        /// <summary>Set the orientation of the monitor</summary>
        /// <returns>True if successful</returns>
        public bool SetOrientation(Orientation orientation) {
            var mode = new WinAPI.DEVMODE();
            mode.dmSize = (short)Marshal.SizeOf(typeof(WinAPI.DEVMODE));

            if (!WinAPI.EnumDisplaySettings(Name, -1, ref mode)) {
                Console.WriteLine($"Failed to get display settings for '{Name}'");
                return false;
            }

            int newOrient = (int)orientation;
            int oldOrient = mode.dmDisplayOrientation;
            bool oldPortrait = oldOrient == WinAPI.DMDO_90 || oldOrient == WinAPI.DMDO_270;
            bool newPortrait = newOrient == WinAPI.DMDO_90 || newOrient == WinAPI.DMDO_270;

            if (oldPortrait != newPortrait) {
                int tmp = mode.dmPelsWidth;
                mode.dmPelsWidth = mode.dmPelsHeight;
                mode.dmPelsHeight = tmp;
            }

            mode.dmDisplayOrientation = newOrient;
            mode.dmFields = WinAPI.DM_DISPLAYORIENTATION | WinAPI.DM_PELSWIDTH | WinAPI.DM_PELSHEIGHT;

            WinAPI.DisplayReturn ret = WinAPI.ChangeDisplaySettingsEx(
                Name,
                ref mode,
                IntPtr.Zero,
                WinAPI.DisplaySettingsFlags.CDS_UPDATEREGISTRY,
                IntPtr.Zero);

            if (ret != WinAPI.DisplayReturn.Successful && ret != WinAPI.DisplayReturn.Restart) {
                Console.WriteLine($"Failed to set orientation with reason '{ret}'");
                return false;
            }

            return true;
        }

        /// <summary>Get the current orientation of the monitor</summary>
        public Orientation GetOrientation() {
            var mode = new WinAPI.DEVMODE();
            mode.dmSize = (short)Marshal.SizeOf(typeof(WinAPI.DEVMODE));

            if (!WinAPI.EnumDisplaySettings(Name, -1, ref mode)) {
                return Orientation.Landscape;
            }

            return (Orientation)mode.dmDisplayOrientation;
        }

        /// <summary>Disable this monitor (detach from the desktop)</summary>
        /// <returns>True if successful</returns>
        public bool Disable() => SetDisplayEnabled(Name, false);

        /// <summary>Enable this monitor (attach to the desktop)</summary>
        /// <returns>True if successful</returns>
        public bool Enable() => SetDisplayEnabled(Name, true);

        /// <summary>Enable a monitor by GDI device name (e.g. \\.\DISPLAY2)</summary>
        /// <returns>True if successful</returns>
        public static bool Enable(string deviceName) => SetDisplayEnabled(deviceName, true);

        /// <summary>Get GDI device names of displays that are present but not attached to the desktop</summary>
        public static List<string> GetDetachedDeviceNames() {
            var names = new List<string>();
            var device = new WinAPI.DISPLAY_DEVICE();
            device.cb = Marshal.SizeOf(device);

            for (uint i = 0; WinAPI.EnumDisplayDevices(null, i, ref device, 0); i++) {
                bool attached = device.StateFlags.HasFlag(WinAPI.DisplayDeviceStateFlags.AttachedToDesktop);
                bool mirror = device.StateFlags.HasFlag(WinAPI.DisplayDeviceStateFlags.MirroringDriver);

                if (!attached && !mirror && !string.IsNullOrEmpty(device.DeviceName)) {
                    names.Add(device.DeviceName);
                }

                device.cb = Marshal.SizeOf(device);
            }

            return names;
        }

        #region helpers
        /// <summary>Retrieve a handle to a monitor that contains the given point</summary>
        public static IntPtr HandleFromPoint(int x, int y, MonitorDefault def = MonitorDefault.Nearest) => HandleFromPoint(new Coord(x, y), def);
        /// <summary>Retrieve a handle to a monitor that contains the given point</summary>
        public static IntPtr HandleFromPoint(Coord point, MonitorDefault def = MonitorDefault.Nearest) => WinAPI.MonitorFromPoint(point, def);
        /// <summary>Retrieve a handle to a monitor that contains the given window</summary>
        public static IntPtr HandleFromWindow(Window win = null, MonitorDefault def = MonitorDefault.Nearest) => WinAPI.MonitorFromWindow((win ?? Window.Active).Hwnd, def);
        /// <summary>Retrieve a handle to a monitor that best fits the given area</summary>
        public static IntPtr HandleFromArea(Area area, MonitorDefault def = MonitorDefault.Nearest) {
            WinAPI.RECT rect = area;
            return WinAPI.MonitorFromRect(ref rect, def);
        }
        /// <summary>Retrieve a handle to a monitor with an index</summary>
        public static IntPtr HandleFromIndex(int index) {
            var list = GetMonitors();
            if (index < 0 || index >= list.Count)
                return IntPtr.Zero;
            return list[index].Handle;
        }

        /// <summary>Retrieve a monitor with a handle</summary>
        public static Monitor GetMonitor(IntPtr hMonitor) {
            if (hMonitor == IntPtr.Zero) {
                return null;
            }

            WinAPI.MONITORINFOEX res = new WinAPI.MONITORINFOEX();
            res.Size = Marshal.SizeOf(res);

            if (WinAPI.GetMonitorInfo(hMonitor, ref res)) {
                return new Monitor(res.DeviceName, res.Flags == 1, hMonitor, res.Monitor, res.WorkArea);
            } else {
                return null;
            }
        }

        /// <summary>Retrieve all current monitors as a list</summary>
        public static List<Monitor> GetMonitors() {
            List<Monitor> list = new List<Monitor>();

            if (WinAPI.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, Collector, IntPtr.Zero))
                return list;
            return null;

            bool Collector(IntPtr hMonitor, IntPtr hdcMonitor, ref WinAPI.RECT lprcMonitor, IntPtr dwData) {
                Monitor monitor = GetMonitor(hMonitor);

                if (monitor != null) {
                    list.Add(monitor);
                }

                return true;
            }
        }

        private static bool SetPrimaryMonitor(string deviceName) {
            // Source - https://stackoverflow.com/a/23044185
            // Posted by ADBailey
            // Retrieved 2026-03-09, License - CC BY-SA 3.0
            // Adapted to target by GDI device name (Monitor.Name). Do not use GetIndex() here:
            // EnumDisplayMonitors order and EnumDisplayDevices indices are not the same.
            if (string.IsNullOrEmpty(deviceName)) {
                Console.WriteLine("Device name is empty");
                return false;
            }

            var deviceMode = new WinAPI.DEVMODE();
            deviceMode.dmSize = (short)Marshal.SizeOf(typeof(WinAPI.DEVMODE));

            if (!WinAPI.EnumDisplaySettings(deviceName, -1, ref deviceMode)) {
                Console.WriteLine($"Failed to get display settings for '{deviceName}'");
                return false;
            }

            var offsetx = deviceMode.dmPosition.x;
            var offsety = deviceMode.dmPosition.y;
            deviceMode.dmPosition.x = 0;
            deviceMode.dmPosition.y = 0;
            deviceMode.dmFields |= WinAPI.DM_POSITION;

            WinAPI.DisplayReturn ret = WinAPI.ChangeDisplaySettingsEx(
                deviceName,
                ref deviceMode,
                (IntPtr)null,
                WinAPI.DisplaySettingsFlags.CDS_SET_PRIMARY | WinAPI.DisplaySettingsFlags.CDS_UPDATEREGISTRY | WinAPI.DisplaySettingsFlags.CDS_NORESET,
                IntPtr.Zero);

            if (ret != WinAPI.DisplayReturn.Successful) {
                Console.WriteLine($"Failed to set initial monitor settings with reason '{ret}'");
                return false;
            }

            var device = new WinAPI.DISPLAY_DEVICE();
            device.cb = Marshal.SizeOf(device);

            // Update remaining devices
            for (uint otherid = 0; WinAPI.EnumDisplayDevices(null, otherid, ref device, 0); otherid++) {
                if (device.StateFlags.HasFlag(WinAPI.DisplayDeviceStateFlags.AttachedToDesktop)
                    && !string.Equals(device.DeviceName, deviceName, StringComparison.OrdinalIgnoreCase)) {
                    device.cb = Marshal.SizeOf(device);
                    var otherDeviceMode = new WinAPI.DEVMODE();
                    otherDeviceMode.dmSize = (short)Marshal.SizeOf(typeof(WinAPI.DEVMODE));

                    if (!WinAPI.EnumDisplaySettings(device.DeviceName, -1, ref otherDeviceMode)) {
                        device.cb = Marshal.SizeOf(device);
                        continue;
                    }

                    otherDeviceMode.dmPosition.x -= offsetx;
                    otherDeviceMode.dmPosition.y -= offsety;
                    otherDeviceMode.dmFields |= WinAPI.DM_POSITION;

                    WinAPI.ChangeDisplaySettingsEx(
                        device.DeviceName,
                        ref otherDeviceMode,
                        (IntPtr)null,
                        WinAPI.DisplaySettingsFlags.CDS_UPDATEREGISTRY | WinAPI.DisplaySettingsFlags.CDS_NORESET,
                        IntPtr.Zero);
                }

                device.cb = Marshal.SizeOf(device);
            }

            // Apply settings
            ret = WinAPI.ChangeDisplaySettingsEx(null, IntPtr.Zero, (IntPtr)null, WinAPI.DisplaySettingsFlags.CDS_NONE, (IntPtr)null);
            if (ret != WinAPI.DisplayReturn.Successful) {
                Console.WriteLine($"Failed to set monitor settings with reason '{ret}'");
                return false;
            }

            Console.WriteLine("Succesfully set monitor settings");
            return true;
        }

        /// <summary>Fetch a human readable list of connected displays</summary>
        public static List<string> EnumDisplayNames() {
            List<string> names = new List<string>();

            WinAPI.DISPLAY_DEVICE device = new WinAPI.DISPLAY_DEVICE();
            device.cb = Marshal.SizeOf(device);

            for (uint otherid = 0; WinAPI.EnumDisplayDevices(null, otherid, ref device, 0); otherid++) {
                if (device.StateFlags.HasFlag(WinAPI.DisplayDeviceStateFlags.AttachedToDesktop)) {
                    device.cb = Marshal.SizeOf(device);
                    var otherDeviceMode = new WinAPI.DEVMODE();

                    WinAPI.EnumDisplaySettings(device.DeviceName, -1, ref otherDeviceMode);

                    names.Add($"[{otherid}] {device.DeviceName} | {device.DeviceString}");
                }
            }

            return names;
        }

        private static bool SetDisplayEnabled(string deviceName, bool enable) {
            if (string.IsNullOrEmpty(deviceName)) {
                Console.WriteLine("Device name is empty");
                return false;
            }

            if (!TryQueryDisplayConfig(WinAPI.QueryDisplayConfigFlags.QDC_ONLY_ACTIVE_PATHS, out var activePaths, out var activeModes)) {
                return false;
            }

            bool isCurrentlyActive = false;
            for (int i = 0; i < activePaths.Length; i++) {
                if (PathMatchesDevice(activePaths[i], deviceName)) {
                    isCurrentlyActive = true;
                    break;
                }
            }

            if (enable) {
                if (isCurrentlyActive) {
                    return true;
                }

                if (!TryQueryDisplayConfig(WinAPI.QueryDisplayConfigFlags.QDC_ALL_PATHS, out var allPaths, out _)) {
                    return false;
                }

                int candidateIndex = -1;
                for (int i = 0; i < allPaths.Length; i++) {
                    if (!PathMatchesDevice(allPaths[i], deviceName)) {
                        continue;
                    }
                    if (!allPaths[i].targetInfo.targetAvailable) {
                        continue;
                    }
                    if ((allPaths[i].flags & WinAPI.DisplayConfigPathFlags.DISPLAYCONFIG_PATH_ACTIVE) != 0) {
                        continue;
                    }

                    candidateIndex = i;
                    break;
                }

                if (candidateIndex < 0) {
                    Console.WriteLine($"No available display path found to enable '{deviceName}'");
                    return false;
                }

                var pathToEnable = allPaths[candidateIndex];
                pathToEnable.flags |= WinAPI.DisplayConfigPathFlags.DISPLAYCONFIG_PATH_ACTIVE;
                pathToEnable.sourceInfo.modeInfoIdx = WinAPI.DISPLAYCONFIG_PATH_MODE_IDX_INVALID;
                pathToEnable.targetInfo.modeInfoIdx = WinAPI.DISPLAYCONFIG_PATH_MODE_IDX_INVALID;

                var newPaths = new WinAPI.DISPLAYCONFIG_PATH_INFO[activePaths.Length + 1];
                Array.Copy(activePaths, newPaths, activePaths.Length);
                newPaths[activePaths.Length] = pathToEnable;

                return ApplyDisplayConfig(newPaths, activeModes);
            }

            if (!isCurrentlyActive) {
                return true;
            }

            if (activePaths.Length <= 1) {
                Console.WriteLine("Cannot disable the only active display");
                return false;
            }

            var remaining = new List<WinAPI.DISPLAYCONFIG_PATH_INFO>(activePaths.Length - 1);
            for (int i = 0; i < activePaths.Length; i++) {
                if (!PathMatchesDevice(activePaths[i], deviceName)) {
                    remaining.Add(activePaths[i]);
                }
            }

            if (remaining.Count == activePaths.Length) {
                Console.WriteLine($"No active display path found for '{deviceName}'");
                return false;
            }

            if (remaining.Count == 0) {
                Console.WriteLine("Cannot disable the only active display");
                return false;
            }

            return ApplyDisplayConfig(remaining.ToArray(), activeModes);
        }

        private static bool PathMatchesDevice(WinAPI.DISPLAYCONFIG_PATH_INFO path, string deviceName) {
            string gdiName = GetSourceGdiDeviceName(path.sourceInfo);
            return !string.IsNullOrEmpty(gdiName)
                && string.Equals(gdiName, deviceName, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetSourceGdiDeviceName(WinAPI.DISPLAYCONFIG_PATH_SOURCE_INFO sourceInfo) {
            var request = new WinAPI.DISPLAYCONFIG_SOURCE_DEVICE_NAME();
            request.header.type = WinAPI.DisplayConfigDeviceInfoType.DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME;
            request.header.size = Marshal.SizeOf(typeof(WinAPI.DISPLAYCONFIG_SOURCE_DEVICE_NAME));
            request.header.adapterId = sourceInfo.adapterId;
            request.header.id = sourceInfo.id;

            int result = WinAPI.DisplayConfigGetDeviceInfo(ref request);
            if (result != WinAPI.ERROR_SUCCESS) {
                return null;
            }

            return request.viewGdiDeviceName;
        }

        private static bool TryQueryDisplayConfig(
            WinAPI.QueryDisplayConfigFlags flags,
            out WinAPI.DISPLAYCONFIG_PATH_INFO[] paths,
            out WinAPI.DISPLAYCONFIG_MODE_INFO[] modes) {

            paths = null;
            modes = null;

            int err = WinAPI.GetDisplayConfigBufferSizes(flags, out int pathCount, out int modeCount);
            if (err != WinAPI.ERROR_SUCCESS) {
                Console.WriteLine($"GetDisplayConfigBufferSizes failed with error {err}");
                return false;
            }

            paths = new WinAPI.DISPLAYCONFIG_PATH_INFO[pathCount];
            modes = new WinAPI.DISPLAYCONFIG_MODE_INFO[modeCount];

            err = WinAPI.QueryDisplayConfig(flags, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
            if (err != WinAPI.ERROR_SUCCESS) {
                Console.WriteLine($"QueryDisplayConfig failed with error {err}");
                paths = null;
                modes = null;
                return false;
            }

            if (pathCount != paths.Length) {
                Array.Resize(ref paths, pathCount);
            }
            if (modeCount != modes.Length) {
                Array.Resize(ref modes, modeCount);
            }

            return true;
        }

        private static bool ApplyDisplayConfig(WinAPI.DISPLAYCONFIG_PATH_INFO[] paths, WinAPI.DISPLAYCONFIG_MODE_INFO[] modes) {
            var flags = WinAPI.SetDisplayConfigFlags.SDC_APPLY
                | WinAPI.SetDisplayConfigFlags.SDC_USE_SUPPLIED_DISPLAY_CONFIG
                | WinAPI.SetDisplayConfigFlags.SDC_ALLOW_CHANGES;

            int result = WinAPI.SetDisplayConfig((uint)paths.Length, paths, (uint)modes.Length, modes, flags);
            if (result == WinAPI.ERROR_SUCCESS) {
                return true;
            }

            if (result == WinAPI.ERROR_GEN_FAILURE) {
                flags |= WinAPI.SetDisplayConfigFlags.SDC_SAVE_TO_DATABASE;
                result = WinAPI.SetDisplayConfig((uint)paths.Length, paths, (uint)modes.Length, modes, flags);
                if (result == WinAPI.ERROR_SUCCESS) {
                    return true;
                }
            }

            Console.WriteLine($"SetDisplayConfig failed with error {result}");
            return false;
        }

        private static Area GetScreenArea() {
            var x = WinAPI.GetSystemMetrics(WinAPI.SM.XVIRTUALSCREEN);
            var y = WinAPI.GetSystemMetrics(WinAPI.SM.YVIRTUALSCREEN);
            var w = WinAPI.GetSystemMetrics(WinAPI.SM.CXVIRTUALSCREEN);
            var h = WinAPI.GetSystemMetrics(WinAPI.SM.CYVIRTUALSCREEN);
            return new Area(x, y, w, h);
        }

        private static int GetMonitorScale(IntPtr Handle) {
            WinAPI.GetDpiForMonitor(Handle, WinAPI.MonitorDpiType.Effective_DPI, out uint x, out uint y);
            return (int)(x * 100 / 96);
        }
        #endregion

        /// <summary>Representation of a monitor orientation</summary>
        public enum Orientation {
            /// <summary>Image pointed up</summary>
            Landscape,
            /// <summary>Image pointed left</summary>
            Portrait,
            /// <summary>Image pointed down</summary>
            LandscapeFlipped,
            /// <summary>Image pointed right</summary>
            PortraitFlipped
        }

        #region operators
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool operator ==(Monitor a, Monitor b) => (a is null && b is null) || !(a is null) && !(b is null) && a.Handle == b.Handle;
        public static bool operator !=(Monitor a, Monitor b) => !(a == b);
        public override bool Equals(object obj) => obj is Monitor && this == (Monitor)obj;
        public override int GetHashCode() => 1786700523 + Handle.GetHashCode();
        public override string ToString() => "[Monitor: " + Name + " | Primary: " + IsPrimary + " | Handle: " + Handle + " | Full area: " + Area + " | Work area: " + WorkArea + "]";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #endregion
    }
}
