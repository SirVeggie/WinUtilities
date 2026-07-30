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
        /// <summary>The GDI device name of the monitor (e.g. \\.\DISPLAY1). Not stable across disable/enable.</summary>
        [DataMember]
        public string Name { get; private set; }
        /// <summary>
        /// Persistent CCD target identity for this monitor. Prefer saving this over <see cref="Name"/>.
        /// Survives disable/enable and typically survives application restarts and PC reboots for the same display.
        /// </summary>
        [DataMember]
        public string TargetId { get; private set; }
        /// <summary>Friendly monitor name from Windows (may be empty)</summary>
        [DataMember]
        public string FriendlyName { get; private set; }
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
        public Monitor(string name, bool isPrimary, IntPtr handle, Area area, Area workarea, string targetId = null, string friendlyName = null) {
            Name = name;
            IsPrimary = isPrimary;
            Handle = handle;
            Area = area;
            WorkArea = workarea;
            TargetId = targetId;
            FriendlyName = friendlyName;
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
        /// <summary>
        /// Find an attached monitor by persistent <see cref="TargetId"/>.
        /// Returns null if the target is unknown or not currently attached to the desktop.
        /// </summary>
        public static Monitor FromTargetId(string targetId) {
            if (string.IsNullOrEmpty(targetId)) {
                return null;
            }

            var monitors = GetMonitors();
            if (monitors == null) {
                return null;
            }

            for (int i = 0; i < monitors.Count; i++) {
                if (string.Equals(monitors[i].TargetId, targetId, StringComparison.OrdinalIgnoreCase)) {
                    return monitors[i];
                }
            }

            return null;
        }

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
        public bool Disable() => DisableByTargetId(ResolveTargetId());

        /// <summary>Enable this monitor (attach to the desktop)</summary>
        /// <returns>True if successful</returns>
        public bool Enable() => EnableByTargetId(ResolveTargetId());

        /// <summary>Enable a monitor by persistent <see cref="TargetId"/></summary>
        /// <returns>True if successful</returns>
        public static bool EnableByTargetId(string targetId) => SetDisplayEnabledByTargetId(targetId, true);

        /// <summary>Disable a monitor by persistent <see cref="TargetId"/></summary>
        /// <returns>True if successful</returns>
        public static bool DisableByTargetId(string targetId) => SetDisplayEnabledByTargetId(targetId, false);

        /// <summary>Enable a monitor by GDI device name (e.g. \\.\DISPLAY2). Prefer <see cref="EnableByTargetId"/> for persistence.</summary>
        /// <returns>True if successful</returns>
        public static bool Enable(string deviceName) {
            string targetId = TryResolveTargetIdFromGdiName(deviceName);
            if (string.IsNullOrEmpty(targetId)) {
                Console.WriteLine($"No CCD target found for GDI device '{deviceName}'");
                return false;
            }
            return EnableByTargetId(targetId);
        }

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

        /// <summary>
        /// List CCD display targets, including ones not currently attached to the desktop.
        /// <see cref="DisplayTarget.TargetId"/> is suitable for saving and later enable/disable.
        /// </summary>
        public static List<DisplayTarget> GetDisplayTargets() {
            var results = new List<DisplayTarget>();
            if (!TryQueryDisplayConfig(WinAPI.QueryDisplayConfigFlags.QDC_ALL_PATHS, out var allPaths, out _)) {
                return results;
            }

            TryQueryDisplayConfig(WinAPI.QueryDisplayConfigFlags.QDC_ONLY_ACTIVE_PATHS, out var activePaths, out _);
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < allPaths.Length; i++) {
                if (!TryGetTargetDeviceName(allPaths[i].targetInfo, out var targetInfo)) {
                    continue;
                }

                string targetId = BuildTargetId(ref targetInfo);
                if (string.IsNullOrEmpty(targetId) || !seen.Add(targetId)) {
                    continue;
                }

                bool attached = false;
                string gdiName = null;
                if (activePaths != null) {
                    for (int a = 0; a < activePaths.Length; a++) {
                        if (!PathMatchesTargetInfo(activePaths[a], allPaths[i].targetInfo)) {
                            continue;
                        }
                        attached = true;
                        gdiName = GetSourceGdiDeviceName(activePaths[a].sourceInfo);
                        break;
                    }
                }

                results.Add(new DisplayTarget(targetId, targetInfo.monitorFriendlyDeviceName, attached, gdiName));
            }

            return results;
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
                TryResolveTargetDetailsFromGdiName(res.DeviceName, out string targetId, out string friendlyName);
                return new Monitor(res.DeviceName, res.Flags == 1, hMonitor, res.Monitor, res.WorkArea, targetId, friendlyName);
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

        private string ResolveTargetId() {
            if (!string.IsNullOrEmpty(TargetId)) {
                return TargetId;
            }
            return TryResolveTargetIdFromGdiName(Name);
        }

        private static bool SetDisplayEnabledByTargetId(string targetId, bool enable) {
            if (string.IsNullOrEmpty(targetId)) {
                Console.WriteLine("Target id is empty");
                return false;
            }

            if (!TryQueryDisplayConfig(WinAPI.QueryDisplayConfigFlags.QDC_ONLY_ACTIVE_PATHS, out var activePaths, out var activeModes)) {
                return false;
            }

            bool isCurrentlyActive = false;
            for (int i = 0; i < activePaths.Length; i++) {
                if (PathMatchesTargetId(activePaths[i], targetId)) {
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

                if (!TryFindPathToEnable(allPaths, activePaths, targetId, out var pathToEnable)) {
                    Console.WriteLine($"No available display path found to enable target '{targetId}'");
                    return false;
                }

                var newPaths = BuildPathsWithEnabledTarget(activePaths, pathToEnable);
                if (!TryApplyTopologyFromDatabase(newPaths)) {
                    Console.WriteLine($"No CCD database entry found to enable target '{targetId}'");
                    return false;
                }

                return true;
            }

            if (!isCurrentlyActive) {
                return true;
            }

            if (activePaths.Length <= 1) {
                Console.WriteLine("Cannot disable the only active display");
                return false;
            }

            // Ensure the full layout (with positions) is in the CCD DB before we change topology,
            // so a later enable can restore it with SDC_TOPOLOGY_SUPPLIED.
            TryApplySuppliedDisplayConfig(activePaths, activeModes, saveToDatabase: true);

            var remaining = new List<WinAPI.DISPLAYCONFIG_PATH_INFO>(activePaths.Length - 1);
            for (int i = 0; i < activePaths.Length; i++) {
                if (!PathMatchesTargetId(activePaths[i], targetId)) {
                    remaining.Add(activePaths[i]);
                }
            }

            if (remaining.Count == activePaths.Length) {
                Console.WriteLine($"No active display path found for target '{targetId}'");
                return false;
            }

            if (remaining.Count == 0) {
                Console.WriteLine("Cannot disable the only active display");
                return false;
            }

            // Save the reduced topology to the DB, matching Display Settings disable behavior.
            return TryApplySuppliedDisplayConfig(remaining.ToArray(), activeModes, saveToDatabase: true);
        }

        private static WinAPI.DISPLAYCONFIG_PATH_INFO[] BuildPathsWithEnabledTarget(
            WinAPI.DISPLAYCONFIG_PATH_INFO[] activePaths,
            WinAPI.DISPLAYCONFIG_PATH_INFO pathToEnable) {

            pathToEnable.flags |= WinAPI.DisplayConfigPathFlags.DISPLAYCONFIG_PATH_ACTIVE;
            pathToEnable.sourceInfo.modeInfoIdx = WinAPI.DISPLAYCONFIG_PATH_MODE_IDX_INVALID;
            pathToEnable.targetInfo.modeInfoIdx = WinAPI.DISPLAYCONFIG_PATH_MODE_IDX_INVALID;

            var newPaths = new WinAPI.DISPLAYCONFIG_PATH_INFO[activePaths.Length + 1];
            Array.Copy(activePaths, newPaths, activePaths.Length);
            newPaths[activePaths.Length] = pathToEnable;
            return newPaths;
        }

        /// <summary>
        /// Prefer a path whose topology exists in the CCD DB, otherwise an unused source (extend).
        /// </summary>
        private static bool TryFindPathToEnable(
            WinAPI.DISPLAYCONFIG_PATH_INFO[] allPaths,
            WinAPI.DISPLAYCONFIG_PATH_INFO[] activePaths,
            string targetId,
            out WinAPI.DISPLAYCONFIG_PATH_INFO pathToEnable) {

            pathToEnable = default;
            int databaseIndex = -1;
            int unusedSourceIndex = -1;

            for (int i = 0; i < allPaths.Length; i++) {
                if (!PathMatchesTargetId(allPaths[i], targetId)) {
                    continue;
                }
                if (!allPaths[i].targetInfo.targetAvailable) {
                    continue;
                }
                if ((allPaths[i].flags & WinAPI.DisplayConfigPathFlags.DISPLAYCONFIG_PATH_ACTIVE) != 0) {
                    continue;
                }

                if (IsSourceInUse(allPaths[i].sourceInfo, activePaths)) {
                    continue;
                }

                if (unusedSourceIndex < 0) {
                    unusedSourceIndex = i;
                }

                if (databaseIndex < 0) {
                    var probePaths = BuildPathsWithEnabledTarget(activePaths, allPaths[i]);
                    if (TryValidateTopologyInDatabase(probePaths)) {
                        databaseIndex = i;
                    }
                }
            }

            int chosen = databaseIndex >= 0 ? databaseIndex : unusedSourceIndex;
            if (chosen < 0) {
                return false;
            }

            pathToEnable = allPaths[chosen];
            return true;
        }

        private static bool IsSourceInUse(WinAPI.DISPLAYCONFIG_PATH_SOURCE_INFO source, WinAPI.DISPLAYCONFIG_PATH_INFO[] activePaths) {
            for (int i = 0; i < activePaths.Length; i++) {
                if (source.id == activePaths[i].sourceInfo.id
                    && source.adapterId.LowPart == activePaths[i].sourceInfo.adapterId.LowPart
                    && source.adapterId.HighPart == activePaths[i].sourceInfo.adapterId.HighPart) {
                    return true;
                }
            }
            return false;
        }

        private static bool PathMatchesTargetId(WinAPI.DISPLAYCONFIG_PATH_INFO path, string targetId) {
            if (!TryGetTargetDeviceName(path.targetInfo, out var targetInfo)) {
                return false;
            }
            string id = BuildTargetId(ref targetInfo);
            return !string.IsNullOrEmpty(id)
                && string.Equals(id, targetId, StringComparison.OrdinalIgnoreCase);
        }

        private static bool PathMatchesTargetInfo(WinAPI.DISPLAYCONFIG_PATH_INFO path, WinAPI.DISPLAYCONFIG_PATH_TARGET_INFO target) {
            return path.targetInfo.id == target.id
                && path.targetInfo.adapterId.LowPart == target.adapterId.LowPart
                && path.targetInfo.adapterId.HighPart == target.adapterId.HighPart;
        }

        private static string TryResolveTargetIdFromGdiName(string gdiName) {
            TryResolveTargetDetailsFromGdiName(gdiName, out string targetId, out _);
            return targetId;
        }

        private static bool TryResolveTargetDetailsFromGdiName(string gdiName, out string targetId, out string friendlyName) {
            targetId = null;
            friendlyName = null;
            if (string.IsNullOrEmpty(gdiName)) {
                return false;
            }

            if (!TryQueryDisplayConfig(WinAPI.QueryDisplayConfigFlags.QDC_ONLY_ACTIVE_PATHS, out var activePaths, out _)) {
                return false;
            }

            for (int i = 0; i < activePaths.Length; i++) {
                string sourceName = GetSourceGdiDeviceName(activePaths[i].sourceInfo);
                if (!string.Equals(sourceName, gdiName, StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }
                if (!TryGetTargetDeviceName(activePaths[i].targetInfo, out var targetInfo)) {
                    continue;
                }

                targetId = BuildTargetId(ref targetInfo);
                friendlyName = targetInfo.monitorFriendlyDeviceName;
                return !string.IsNullOrEmpty(targetId);
            }

            return false;
        }

        private static string BuildTargetId(ref WinAPI.DISPLAYCONFIG_TARGET_DEVICE_NAME targetInfo) {
            if (!string.IsNullOrEmpty(targetInfo.monitorDevicePath)) {
                return targetInfo.monitorDevicePath;
            }

            // Fallback when Windows does not provide a device path (less ideal, but still serializable).
            return $"edid:{targetInfo.edidManufactureId:X4}:{targetInfo.edidProductCodeId:X4}:{targetInfo.connectorInstance}:{(int)targetInfo.outputTechnology}";
        }

        private static bool TryGetTargetDeviceName(WinAPI.DISPLAYCONFIG_PATH_TARGET_INFO target, out WinAPI.DISPLAYCONFIG_TARGET_DEVICE_NAME info) {
            info = new WinAPI.DISPLAYCONFIG_TARGET_DEVICE_NAME();
            info.header.type = WinAPI.DisplayConfigDeviceInfoType.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME;
            info.header.size = Marshal.SizeOf(typeof(WinAPI.DISPLAYCONFIG_TARGET_DEVICE_NAME));
            info.header.adapterId = target.adapterId;
            info.header.id = target.id;

            return WinAPI.DisplayConfigGetDeviceInfo(ref info) == WinAPI.ERROR_SUCCESS;
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

        private static WinAPI.DISPLAYCONFIG_PATH_INFO[] ClonePathsWithInvalidModes(WinAPI.DISPLAYCONFIG_PATH_INFO[] paths) {
            var clone = (WinAPI.DISPLAYCONFIG_PATH_INFO[])paths.Clone();
            for (int i = 0; i < clone.Length; i++) {
                clone[i].sourceInfo.modeInfoIdx = WinAPI.DISPLAYCONFIG_PATH_MODE_IDX_INVALID;
                clone[i].targetInfo.modeInfoIdx = WinAPI.DISPLAYCONFIG_PATH_MODE_IDX_INVALID;
            }
            return clone;
        }

        /// <summary>
        /// Ask Windows whether this topology exists in the CCD persistence database (Display Settings store).
        /// </summary>
        private static bool TryValidateTopologyInDatabase(WinAPI.DISPLAYCONFIG_PATH_INFO[] paths) {
            var topologyPaths = ClonePathsWithInvalidModes(paths);
            var flags = WinAPI.SetDisplayConfigFlags.SDC_VALIDATE
                | WinAPI.SetDisplayConfigFlags.SDC_TOPOLOGY_SUPPLIED
                | WinAPI.SetDisplayConfigFlags.SDC_ALLOW_PATH_ORDER_CHANGES;

            return WinAPI.SetDisplayConfig((uint)topologyPaths.Length, topologyPaths, 0, null, flags) == WinAPI.ERROR_SUCCESS;
        }

        /// <summary>
        /// Restore topology + modes/positions from the CCD persistence database (Display Settings flow).
        /// </summary>
        private static bool TryApplyTopologyFromDatabase(WinAPI.DISPLAYCONFIG_PATH_INFO[] paths) {
            var topologyPaths = ClonePathsWithInvalidModes(paths);
            var flags = WinAPI.SetDisplayConfigFlags.SDC_APPLY
                | WinAPI.SetDisplayConfigFlags.SDC_TOPOLOGY_SUPPLIED
                | WinAPI.SetDisplayConfigFlags.SDC_ALLOW_PATH_ORDER_CHANGES;

            int result = WinAPI.SetDisplayConfig((uint)topologyPaths.Length, topologyPaths, 0, null, flags);
            if (result == WinAPI.ERROR_SUCCESS) {
                return true;
            }

            // Some hosts need ALLOW_CHANGES when path order differs slightly from the DB entry.
            flags |= WinAPI.SetDisplayConfigFlags.SDC_ALLOW_CHANGES;
            result = WinAPI.SetDisplayConfig((uint)topologyPaths.Length, topologyPaths, 0, null, flags);
            return result == WinAPI.ERROR_SUCCESS;
        }

        /// <summary>
        /// Apply an explicit path/mode config, optionally writing it to the CCD persistence database.
        /// </summary>
        private static bool TryApplySuppliedDisplayConfig(
            WinAPI.DISPLAYCONFIG_PATH_INFO[] paths,
            WinAPI.DISPLAYCONFIG_MODE_INFO[] modes,
            bool saveToDatabase) {

            var flags = WinAPI.SetDisplayConfigFlags.SDC_APPLY
                | WinAPI.SetDisplayConfigFlags.SDC_USE_SUPPLIED_DISPLAY_CONFIG
                | WinAPI.SetDisplayConfigFlags.SDC_ALLOW_CHANGES;

            if (saveToDatabase) {
                flags |= WinAPI.SetDisplayConfigFlags.SDC_SAVE_TO_DATABASE;
            }

            int result = WinAPI.SetDisplayConfig((uint)paths.Length, paths, (uint)modes.Length, modes, flags);
            if (result == WinAPI.ERROR_SUCCESS) {
                return true;
            }

            if (!saveToDatabase && result == WinAPI.ERROR_GEN_FAILURE) {
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

        /// <summary>Serializable CCD display target information</summary>
        [DataContract]
        public class DisplayTarget {
            /// <summary>Persistent target identity suitable for saving and later enable/disable</summary>
            [DataMember]
            public string TargetId { get; private set; }
            /// <summary>Friendly monitor name from Windows (may be empty)</summary>
            [DataMember]
            public string FriendlyName { get; private set; }
            /// <summary>True if currently attached to the desktop</summary>
            [DataMember]
            public bool IsAttached { get; private set; }
            /// <summary>Current GDI device name when attached; otherwise null</summary>
            [DataMember]
            public string GdiName { get; private set; }

            /// <summary>Create display target info</summary>
            public DisplayTarget(string targetId, string friendlyName, bool isAttached, string gdiName) {
                TargetId = targetId;
                FriendlyName = friendlyName;
                IsAttached = isAttached;
                GdiName = gdiName;
            }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            public override string ToString() =>
                $"[DisplayTarget: {FriendlyName} | Attached: {IsAttached} | Gdi: {GdiName} | Id: {TargetId}]";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }

        #region operators
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool operator ==(Monitor a, Monitor b) => (a is null && b is null) || !(a is null) && !(b is null) && a.Handle == b.Handle;
        public static bool operator !=(Monitor a, Monitor b) => !(a == b);
        public override bool Equals(object obj) => obj is Monitor && this == (Monitor)obj;
        public override int GetHashCode() => 1786700523 + Handle.GetHashCode();
        public override string ToString() => "[Monitor: " + Name + " | TargetId: " + TargetId + " | Primary: " + IsPrimary + " | Handle: " + Handle + " | Full area: " + Area + " | Work area: " + WorkArea + "]";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #endregion
    }
}
