using System;
using System.Collections.Generic;
using System.Drawing;
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

        /// <summary>[Not implemented] Set as the current primary monitor</summary>
        public bool SetPrimary() {
            throw new NotImplementedException();
        }

        /// <summary>[Not implemented] Set the orientation of the monitor</summary>
        public void SetOrientation(Orientation orientation) {
            throw new NotImplementedException();
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

        private static Area GetScreenArea() {
            var x = WinAPI.GetSystemMetrics(WinAPI.SM.XVIRTUALSCREEN);
            var y = WinAPI.GetSystemMetrics(WinAPI.SM.YVIRTUALSCREEN);
            var w = WinAPI.GetSystemMetrics(WinAPI.SM.CXVIRTUALSCREEN);
            var h = WinAPI.GetSystemMetrics(WinAPI.SM.CYVIRTUALSCREEN);
            return new Area(x, y, w, h);
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
        public override bool Equals(object obj) => obj is Monitor && this == (Monitor) obj;
        public override int GetHashCode() => 1786700523 + Handle.GetHashCode();
        public override string ToString() => "[Monitor: " + Name + " | Primary: " + IsPrimary + " | Handle: " + Handle + " | Full area: " + Area + " | Work area: " + WorkArea + "]";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #endregion
    }
}
