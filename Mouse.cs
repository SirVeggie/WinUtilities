using System;
using System.Collections.Generic;

namespace WinUtilities {
    /// <summary>Class for controlling the mouse</summary>
    public static class Mouse {

        #region properties
        /// <summary>Position of the mouse</summary>
        public static Coord Position {
            get { WinAPI.GetCursorPos(out WinAPI.POINT p); return new Coord(p); }
            set => Move(value);
        }
        /// <summary>Check if the mouse is visible</summary>
        public static bool IsVisible {
            get {
                var info = WinAPI.CURSORINFO.Initialized();
                WinAPI.GetCursorInfo(ref info);
                return info.state == WinAPI.CursorState.Showing;
            }
        }

        /// <summary>Check if the mouse if confined to an area</summary>
        public static bool IsConfined {
            get {
                WinAPI.GetClipCursor(out WinAPI.RECT res);
                return (Area) res != Monitor.Screen;
            }
        }

        /// <summary>Retrieve the area the mouse is contained in</summary>
        public static Area ConfinedArea {
            get {
                WinAPI.GetClipCursor(out WinAPI.RECT area);
                return area;
            }
        }

        /// <summary>Retrieve the current type of the mouse</summary>
        public static CursorType CursorType {
            get {
                var info = WinAPI.CURSORINFO.Initialized();
                WinAPI.GetCursorInfo(ref info);
                return CursorTypes.FromHandle(info.hCursor);
            }
        }

        /// <summary>Check how many buttons the mouse has</summary>
        public static int ButtonAmount { get => WinAPI.GetSystemMetrics(WinAPI.SM.CMOUSEBUTTONS); }

        /// <summary>Check if the mouse is hidden by this process</summary>
        public static bool IsHidden { get; private set; }
        #endregion

        #region methods
        /// <summary>Send a left click</summary>
        public static void Click() => Input.Send(Key.LButton);
        /// <summary>Send a right click</summary>
        public static void RightClick() => Input.Send(Key.RButton);
        /// <summary>Send a middle click</summary>
        public static void MiddleClick() => Input.Send(Key.MButton);
        /// <summary>Send a double click</summary>
        public static void DoubleClick() => Input.Send(Key.LButton, Key.LButton);
        /// <summary>Send a scroll wheel event</summary>
        public static void Scroll(Key key, double amount) => Scroll(key, (int) Math.Round(amount));
        /// <summary>Send a scroll wheel event</summary>
        public static void Scroll(Key key, int amount = 120) => Input.Scroll(key, amount);
        /// <summary>Move the mouse to a point</summary>
        public static bool Move(int x, int y, CoordRelation rel = CoordRelation.Screen) => Move(new Coord(x, y), rel);
        /// <summary>Move the mouse to a point</summary>
        public static bool Move(Coord point, CoordRelation rel = CoordRelation.Screen) {
            if (rel == CoordRelation.ActiveWindow)
                point += Window.Active.Area.Point;
            else if (rel == CoordRelation.Mouse)
                return Input.MouseMoveRelative(point.IntX, point.IntY);
            return WinAPI.SetCursorPos(point.IntX, point.IntY);
        }

        /// <summary>Restrict cursor movement to within the specified area.</summary>
        /// <param name="area">Set null to free the cursor.</param>
        public static bool ConfineToArea(Area? area) {
            if (area != null) {
                WinAPI.RECT rect = (Area) area;
                return WinAPI.ClipCursor(ref rect);
            } else {
                return WinAPI.ClipCursor(IntPtr.Zero);
            }
        }

        /// <summary>This will fuck up cursor if the program exits while the cursor is hidden.</summary>
        public static void Hide(bool state) {
            if (IsHidden == state)
                return;
            if (state) {
                WinAPI.SetSystemCursor(WinAPI.CopyIcon(CursorTypes.Invisible), CursorType.Arrow);
            } else {
                WinAPI.SetSystemCursor(WinAPI.CopyIcon(CursorTypes.Arrow), CursorType.Arrow);
            }

            IsHidden = state;
        }
        #endregion
    }

    /// <summary>Helper class for controlling the look of the cursor</summary>
    public static class CursorTypes {

        /// <summary>Dictionary of handle -> Type of the different cursors</summary>
        public static Dictionary<IntPtr, CursorType> Types { get; private set; }
        /// <summary>Dictionary of Type -> Handle of the different cursors</summary>
        public static Dictionary<CursorType, IntPtr> Reverse { get; private set; }
        /// <summary>Handle to an invisible cursor</summary>
        public static IntPtr Invisible { get; private set; }
        /// <summary>Handle to the default cursor</summary>
        public static IntPtr Arrow { get; private set; }

        static CursorTypes() {
            Types = new Dictionary<IntPtr, CursorType>();
            Reverse = new Dictionary<CursorType, IntPtr>();

            foreach (CursorType cursor in Enum.GetValues(typeof(CursorType))) {
                if (cursor != CursorType.Undefined) {
                    var res = WinAPI.LoadCursor(IntPtr.Zero, cursor);

                    if ((int) res == 0)
                        throw new Exception("Couldn't find a handle for cursor " + cursor);
                    Types.Add(res, cursor);
                    Reverse.Add(cursor, res);
                }
            }

            Invisible = CreateInvisibleCursor();
            Arrow = WinAPI.CopyIcon(Reverse[CursorType.Arrow]);
        }

        /// <summary>Get the type of a cursor from its handle</summary>
        public static CursorType FromHandle(IntPtr handle) {
            if (Types.ContainsKey(handle)) {
                return Types[handle];
            } else {
                return CursorType.Undefined;
            }
        }

        /// <summary>Get the handle of a cursor from its type</summary>
        public static IntPtr FromType(CursorType type) {
            if (Reverse.ContainsKey(type)) {
                return Reverse[type];
            } else {
                throw new Exception("No cursor handle found");
            }
        }

        /// <summary>Create an instance of an invisible cursor</summary>
        public static IntPtr CreateInvisibleCursor() {
            var invisible = WinAPI.LoadCursorFromFile(AppDomain.CurrentDomain.BaseDirectory + "invisible.cur");

            if (invisible == IntPtr.Zero) {
                throw new Exception("Failed to create invisible cursor");
            }

            return invisible;
        }
    }
}
