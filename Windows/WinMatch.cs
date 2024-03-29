﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WinUtilities {

    /// <summary>A condition that matches some windows</summary>
    [DataContract]
    public struct WinMatch : IWinMatch {

        private string _title;
        private string _class;
        private string _exe;
        private string _exePath;

        private Regex rTitle;
        private Regex rClass;
        private Regex rExe;
        private Regex rExePath;

        private static readonly RegexOptions rOptions = RegexOptions.IgnoreCase;

        #region properties
        /// <summary>Matched window handle</summary>
        [DataMember]
        public IntPtr? Hwnd { get; set; }

        /// <summary>Matched window title</summary>
        [DataMember]
        public string Title {
            get => _title;
            set {
                _title = value;
                rTitle = CreateRegex(value, Type);
            }
        }

        /// <summary>Matched window class</summary>
        [DataMember]
        public string Class {
            get => _class;
            set {
                _class = value;
                rClass = CreateRegex(value, Type);
            }
        }

        /// <summary>Matched window executable name</summary>
        [DataMember]
        public string Exe {
            get => _exe;
            set {
                _exe = value;
                rExe = CreateRegex(value, Type);
            }
        }

        /// <summary>Matched window full executable name and path</summary>
        [DataMember]
        public string ExePath {
            get => _exePath;
            set {
                _exePath = value;
                rExePath = CreateRegex(value, Type);
            }
        }

        /// <summary>Matched window process id</summary>
        [DataMember]
        public uint PID { get; set; }

        /// <summary>Matched desktop guid</summary>
        [DataMember]
        public Guid Desktop { get; set; }

        /// <summary>Reverse the result of the match</summary>
        [DataMember]
        public bool IsReverse { get; set; }
        /// <summary>Get a reversed match</summary>
        public IWinMatch AsReverse {
            get {
                var match = this;
                match.IsReverse ^= true;
                return match;
            }
        }

        /// <summary>Get whitelisted matches as a list</summary>
        public WinMatch[] AsList => new WinMatch[] { this };

        /// <summary>Specifies how the strings are matched</summary>
        [DataMember]
        public WinMatchType Type { get; }
        #endregion

        /// <summary>Create a new match condition</summary>
        public WinMatch(IntPtr? hwnd = null, string title = null, string className = null, string exe = null, string exePath = null, uint pid = 0, Guid desktop = default, WinMatchType type = WinMatchType.RegEx) {
            Hwnd = hwnd;
            _title = title;
            _class = className;
            _exe = exe;
            _exePath = exePath;
            PID = pid;
            Desktop = desktop;

            IsReverse = false;
            Type = type;

            rTitle = CreateRegex(title, type);
            rClass = CreateRegex(className, type);
            rExe = CreateRegex(exe, type);
            rExePath = CreateRegex(exePath, type);
        }

        private static Regex CreateRegex(string pattern, WinMatchType type) {
            return type == WinMatchType.RegEx ? new Regex(pattern ?? "", rOptions) : null;
        }

        #region methods
        /// <summary>Check if the given window matches</summary>
        public bool Match(Window win) => Match(new WindowInfo(win));

        /// <summary>Check if the given info matches</summary>
        public bool Match(WindowInfo info) {
            bool result = (Hwnd == null || Hwnd == info.Hwnd)
                       && (Class == null || MatchSingle(info.Class, Class, rClass))
                       && (PID == 0 || PID == info.PID)
                       && (Exe == null || MatchSingle(info.Exe, Exe, rExe))
                       && (ExePath == null || MatchSingle(info.ExePath, ExePath, rExePath))
                       && (Title == null || MatchSingle(info.Title, Title, rTitle))
                       && (Desktop == Guid.Empty || Desktop == info.Desktop);
            return IsReverse ^ result;
        }

        /// <summary>A window from this group is the active window</summary>
        public bool IsActive() => Window.Active.Match(this);

        /// <summary>Perform an action on all matching windows</summary>
        public void ForAll(Action<Window> action, WinFindMode mode = WinFindMode.TopLevel) => MatchActions.ForAll(this, action, mode);

        /// <summary>Perform an action on all matching windows. Return false to stop enumerating windows.</summary>
        /// <returns>True if all found windows were enumerated</returns>
        public bool ForAll(Func<Window, bool> action, WinFindMode mode = WinFindMode.TopLevel) => MatchActions.ForAll(this, action, mode);

        /// <summary>Perform an async action on all matching windows one at a time. Return false to stop enumerating windows.</summary>
        /// <returns>True if all found windows were enumerated</returns>
        public async Task<bool> ForAll(Func<Window, Task<bool>> action, WinFindMode mode = WinFindMode.TopLevel) => await MatchActions.ForAll(this, action, mode);
        #endregion

        #region single matching
        /// <summary>Check if the window handle matches</summary>
        public bool MatchHwnd(IntPtr? hwnd) => IsReverse ^ (Hwnd == null || Hwnd == hwnd);
        /// <summary>Check if the window title matches</summary>
        public bool MatchTitle(string title) => IsReverse ^ MatchSingle(title, Title, rTitle);
        /// <summary>Check if the window class matches</summary>
        public bool MatchClass(string className) => IsReverse ^ MatchSingle(className, Class, rClass);
        /// <summary>Check if the window executable name matches</summary>
        public bool MatchExe(string exe) => IsReverse ^ MatchSingle(exe, Exe, rExe);
        /// <summary>Check if the window executable path matches</summary>
        public bool MatchExePath(string exePath) => IsReverse ^ MatchSingle(exePath, ExePath, rExePath);
        /// <summary>Check if the window process id matches</summary>
        public bool MatchPID(uint pid) => IsReverse ^ (PID == 0 || PID == pid);

        private bool MatchSingle(string value, string match, Regex regex) {
            if (match == null) {
                return true;
            } else if (Type == WinMatchType.RegEx) {
                return regex.IsMatch(value);
            } else if (Type == WinMatchType.Full) {
                return match == value;
            } else {
                return value.Contains(match);
            }
        }
        #endregion

        /// <summary>Get a match object that matches the given window</summary>
        public static explicit operator WinMatch(Window window) => new WinMatch(window.Hwnd);

        /// <summary>Create a combined match that matches either match</summary>
        public static WinGroup operator |(WinMatch m1, IWinMatch m2) => new WinGroup(m1, m2);
        /// <summary>Create a combined match that must match both matches</summary>
        public static WinAndGroup operator &(WinMatch m1, IWinMatch m2) => new WinAndGroup(m1, m2);
    }
}
