using System;
using System.Collections.Generic;
using System.Linq;

namespace WinUtilities {

    /// <summary>A combination of a key and its state</summary>
    public struct KeyState {
        /// <summary>The key whose state is recorded</summary>
        public Key Key { get; }
        /// <summary>The state of the key</summary>
        public bool State { get; }

        /// <summary>Create a new <see cref="KeyState"/> object</summary>
        public KeyState(Key key, bool state) {
            Key = key;
            State = state;
        }

        #region operators
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool operator ==(KeyState a, KeyState b) => a.Key == b.Key && a.State == b.State;
        public static bool operator !=(KeyState a, KeyState b) => !(a == b);
        public override bool Equals(object obj) => obj is KeyState && this == (KeyState) obj;
        public override string ToString() => $"{{KeyState: {Key} | {State}}}";
        public override int GetHashCode() {
            int hashCode = 1492374402;
            hashCode = hashCode * -1521134295 + Key.GetHashCode();
            hashCode = hashCode * -1521134295 + State.GetHashCode();
            return hashCode;
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #endregion
    }

    /// <summary>Extension methods for the enhanced Key enum</summary>
    public static class EnhancedKey {

        static EnhancedKey() {
            KeyMap = MapNormalKeys();
            KeyExtendedMap = MapExtendedKeys();
        }

        #region mapping
        /// <summary>Map virtual key codes to the Key enum</summary>
        public static Dictionary<VKey, Key> KeyMap { get; }
        /// <summary>Map extended versions of virtual key codes to the Key enum</summary>
        public static Dictionary<VKey, Key> KeyExtendedMap { get; }
        /// <summary>Map strings to the Key enum</summary>

        private static Dictionary<VKey, Key> MapNormalKeys() {
            var dict = new Dictionary<VKey, Key>();

            foreach (Key key in Enum.GetValues(typeof(Key))) {
                if (!key.IsCustom() && key.IsKey() && !key.IsExtended()) {
                    var vkey = key.AsVirtualKey();
                    if (!dict.ContainsKey(vkey)) {
                        dict.Add(vkey, key);
                    }
                }
            }

            return dict;
        }

        private static Dictionary<VKey, Key> MapExtendedKeys() {
            var dict = new Dictionary<VKey, Key>();

            foreach (Key key in Enum.GetValues(typeof(Key))) {
                if (key.IsExtended()) {
                    var vkey = key.AsVirtualKey();
                    if (!dict.ContainsKey(vkey)) {
                        dict.Add(vkey, key);
                    }
                }
            }

            return dict;
        }
        #endregion

        #region flags
        /// <summary>Check if key has any of the flags given</summary>
        public static bool HasAny(this Key key, Key flags) => (key & flags) != 0;
        /// <summary>Check if key has all of the flags given</summary>
        public static bool HasAll(this Key key, Key flags) => (key & flags) == flags;

        /// <summary>Check if the Key value is a key instead of a flag etc.</summary>
        public static bool IsKey(this Key key) => key.HasAny(Key.M_KeyMask) && key != Key.M_KeyMask && !key.IsNone() && !key.IsMouseMove();
        /// <summary>Check if the Key value is a flag</summary>
        public static bool IsFlag(this Key key) => (key & ~Key.M_FlagMask) == 0 && key != Key.M_FlagMask;
        /// <summary>Check if the Key value is a mask</summary>
        public static bool IsMask(this Key key) => key == Key.M_KeyMask || key == Key.M_FlagMask;
        /// <summary>Check if the Key value is a custom entry</summary>
        public static bool IsCustom(this Key key) => key.HasFlag(Key.F_Custom);
        /// <summary>Check if the key is a modifier</summary>
        public static bool IsModifier(this Key key) => key.HasFlag(Key.F_Modifier);
        /// <summary>Check if the key is a mouse key</summary>
        public static bool IsMouse(this Key key) => key.HasFlag(Key.F_Mouse);
        /// <summary>Check if the key is a numpad key</summary>
        public static bool IsNumpad(this Key key) => key.HasFlag(Key.F_Numpad);
        /// <summary>Check if the key represents a scroll event</summary>
        public static bool IsScroll(this Key key) => key.HasFlag(Key.F_Scroll);
        /// <summary>Check if the Key value represents mouse movement</summary>
        public static bool IsMouseMove(this Key key) => key == Key.MouseMove;
        /// <summary>Check if the key is a number</summary>
        public static bool IsNumber(this Key key) => key.HasFlag(Key.F_Number);
        /// <summary>Check if the key is has the extended property</summary>
        public static bool IsExtended(this Key key) => key.HasFlag(Key.F_Extended) && key.HasAny(Key.M_KeyMask);
        /// <summary>Check if the key is a media key</summary>
        public static bool IsMedia(this Key key) => key.HasFlag(Key.F_Media);
        /// <summary>Check if the key produces a character when typed</summary>
        public static bool IsChar(this Key key) => key.HasFlag(Key.F_Char);
        /// <summary>Check if the key is stateless. Stateless keys have no up event.</summary>
        public static bool IsStateless(this Key key) => key.HasFlag(Key.F_Stateless);
        /// <summary>Check if the key is a toggleable key</summary>
        public static bool IsToggle(this Key key) => key.HasFlag(Key.F_Toggle);
        /// <summary>Check if the key is the None key. Represents a fail or null state.</summary>
        public static bool IsNone(this Key key) => key == Key.None;

        /// <summary>Check if the key is a keyboard key instead of a mouse or a custom key</summary>
        public static bool IsKeyboard(this Key key) {
            return key.IsKey() && !key.HasAny(Key.F_Mouse | Key.F_Custom);
        }

        /// <summary>Check if the Key value is a modifier flag</summary>
        public static bool IsModifierFlag(this Key key) => key.HasFlag(Key.F_Modifier) && !key.HasAny(Key.M_KeyMask);
        /// <summary>Check if the key is a left or right shift key</summary>
        public static bool IsShift(this Key key) => key.HasFlag(Key.Shift);
        /// <summary>Check if the key is a left or right control key</summary>
        public static bool IsCtrl(this Key key) => key.HasFlag(Key.Ctrl);
        /// <summary>Check if the key is a left or right win key</summary>
        public static bool IsWin(this Key key) => key.HasFlag(Key.Win);
        /// <summary>Check if the key is a left or right alt key</summary>
        public static bool IsAlt(this Key key) => key.HasFlag(Key.Alt);
        #endregion

        #region key casting
        /// <summary>Get the <see cref="VKey"/> equivalent of this <see cref="Key"/></summary>
        public static VKey AsVirtualKey(this Key key) {
            if (key.IsCustom())
                throw new Exception("Custom keys do not have virtual codes");
            if (!key.IsKey())
                throw new Exception("This enum member is not a key");
            return (VKey) (key & Key.M_KeyMask);
        }

        /// <summary>Get the <see cref="ScanCode"/> equivalent of this <see cref="Key"/></summary>
        public static ScanCode AsScanCode(this Key key) => WinAPI.MapVirtualKey(key.AsVirtualKey()) | (ScanCode) (key.IsExtended() ? 0xE000 : 0);

        /// <summary>Get the <see cref="VKey"/> equivalent of this <see cref="ScanCode"/></summary>
        public static VKey AsVirtualKey(this ScanCode sc) => WinAPI.MapVirtualKey(sc);

        /// <summary>Get the <see cref="ScanCode"/> equivalent of this <see cref="VKey"/></summary>
        public static ScanCode AsScanCode(this VKey vk) => WinAPI.MapVirtualKey(vk);

        /// <summary>Get the <see cref="Key"/> equivalent of this <see cref="ScanCode"/></summary>
        public static Key AsCustom(this ScanCode sc) => WinAPI.MapVirtualKey(sc).AsCustom(sc.IsExtended());

        /// <summary>Get the <see cref="Key"/> equivalent of this <see cref="VKey"/>. If an extended key is not found, a non-extended version is returned if possible.</summary>
        /// <param name="key">The <see cref="VKey"/> to cast into a <see cref="ScanCode"/></param>
        /// <param name="extended">Set true to prioritize the extended version of the key (Example: Enter vs NumpadEnter). Returns non-extended version as fallback if not found and vice versa.</param>
        public static Key AsCustom(this VKey key, bool extended) {
            if (extended) {
                if (KeyExtendedMap.ContainsKey(key))
                    return KeyExtendedMap[key];
                else if (KeyMap.ContainsKey(key))
                    return KeyMap[key];
            } else {
                if (KeyMap.ContainsKey(key))
                    return KeyMap[key];
                else if (KeyExtendedMap.ContainsKey(key))
                    return KeyExtendedMap[key];
            }

            throw new Exception("This virtual key is not defined in the Key enum.");
        }
        #endregion

        /// <summary>Get all keys as a list. Does not include duplicates</summary>
        public static List<Key> GetKeys() {
            HashSet<Key> set = new HashSet<Key>();
            foreach (Key key in Enum.GetValues(typeof(Key))) {
                if (key.IsKey() && key != Key.Unknown && key != Key.NoMapping) {
                    set.Add(key);
                }
            }

            return set.ToList();
        }

        private static ushort scMask = 0xFF00;
        private static ushort scValue = 0xE000;

        /// <summary>Check if the <see cref="ScanCode"/> is an extended key</summary>
        public static bool IsExtended(this ScanCode sc) => ((ushort) sc & scMask) == scValue;
    }

    #region enum
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    /// <summary>An enhanced list of Virtual Keys.</summary>
    public enum Key : uint {
        /// <summary>Flag for custom defined keys</summary>
        F_Custom = 1 << 8,
        /// <summary>Flag for modifier keys</summary>
        F_Modifier = F_Custom << 1,
        /// <summary>Flag for mouse keys</summary>
        F_Mouse = F_Custom << 2,
        /// <summary>Flag for numpad keys</summary>
        F_Numpad = F_Custom << 3,
        /// <summary>Flag for scroll keys</summary>
        F_Scroll = F_Custom << 4,
        /// <summary>Flag for number keys</summary>
        F_Number = F_Custom << 5,
        /// <summary>Flag for extended keys</summary>
        F_Extended = F_Custom << 6,
        /// <summary>Flag for media keys</summary>
        F_Media = F_Custom << 7,
        /// <summary>Flag for keys that produce visible characters</summary>
        F_Char = F_Custom << 8,
        /// <summary>Flag for stateless keys</summary>
        F_Stateless = F_Custom << 9,
        /// <summary>Flag for toggle keys</summary>
        F_Toggle = F_Custom << 10,
        /// <summary>Flag for keys that are out of the norm somehow</summary>
        F_Special = F_Custom << 11,

        /// <summary>Flag for shift keys</summary>
        Shift = F_Custom << 20 | F_Modifier,
        /// <summary>Flag for ctrl keys</summary>
        Ctrl = F_Custom << 21 | F_Modifier,
        /// <summary>Flag for win keys</summary>
        Win = F_Custom << 22 | F_Modifier,
        /// <summary>Flag for alt keys</summary>
        Alt = F_Custom << 23 | F_Modifier,

        /// <summary>Mask for the flag bits</summary>
        M_FlagMask = 0xFFFFFF00,
        /// <summary>Mask for the Virtual Key</summary>
        M_KeyMask = 0xFF,

        // -------------------------- //

        /// <summary>This key doesn't (shouldn't) do anything</summary>
        NoMapping = 0xFF | F_Special,

        /// <summary>Left mouse button</summary>
        LButton = VKey.LBUTTON | F_Mouse,
        /// <summary>Right mouse button</summary>
        RButton = VKey.RBUTTON | F_Mouse,
        /// <summary>Middle mouse button</summary>
        MButton = VKey.MBUTTON | F_Mouse,
        /// <summary>Extra mouse button 1</summary>
        XButton1 = VKey.XBUTTON1 | F_Mouse,
        /// <summary>Extra mouse button 2</summary>
        XButton2 = VKey.XBUTTON2 | F_Mouse,

        /// <summary>Digit 0</summary>
        D0 = VKey.D0 | F_Number | F_Char,
        /// <summary>Digit 1</summary>
        D1 = VKey.D1 | F_Number | F_Char,
        /// <summary>Digit 2</summary>
        D2 = VKey.D2 | F_Number | F_Char,
        /// <summary>Digit 3</summary>
        D3 = VKey.D3 | F_Number | F_Char,
        /// <summary>Digit 4</summary>
        D4 = VKey.D4 | F_Number | F_Char,
        /// <summary>Digit 5</summary>
        D5 = VKey.D5 | F_Number | F_Char,
        /// <summary>Digit 6</summary>
        D6 = VKey.D6 | F_Number | F_Char,
        /// <summary>Digit 7</summary>
        D7 = VKey.D7 | F_Number | F_Char,
        /// <summary>Digit 8</summary>
        D8 = VKey.D8 | F_Number | F_Char,
        /// <summary>Digit 9</summary>
        D9 = VKey.D9 | F_Number | F_Char,

        A = VKey.A | F_Char,
        B = VKey.B | F_Char,
        C = VKey.C | F_Char,
        D = VKey.D | F_Char,
        E = VKey.E | F_Char,
        F = VKey.F | F_Char,
        G = VKey.G | F_Char,
        H = VKey.H | F_Char,
        I = VKey.I | F_Char,
        J = VKey.J | F_Char,
        K = VKey.K | F_Char,
        L = VKey.L | F_Char,
        M = VKey.M | F_Char,
        N = VKey.N | F_Char,
        O = VKey.O | F_Char,
        P = VKey.P | F_Char,
        Q = VKey.Q | F_Char,
        R = VKey.R | F_Char,
        S = VKey.S | F_Char,
        T = VKey.T | F_Char,
        U = VKey.U | F_Char,
        V = VKey.V | F_Char,
        W = VKey.W | F_Char,
        X = VKey.X | F_Char,
        Y = VKey.Y | F_Char,
        Z = VKey.Z | F_Char,

        Numpad0 = VKey.NUMPAD0 | F_Numpad | F_Number | F_Char,
        Numpad1 = VKey.NUMPAD1 | F_Numpad | F_Number | F_Char,
        Numpad2 = VKey.NUMPAD2 | F_Numpad | F_Number | F_Char,
        Numpad3 = VKey.NUMPAD3 | F_Numpad | F_Number | F_Char,
        Numpad4 = VKey.NUMPAD4 | F_Numpad | F_Number | F_Char,
        Numpad5 = VKey.NUMPAD5 | F_Numpad | F_Number | F_Char,
        Numpad6 = VKey.NUMPAD6 | F_Numpad | F_Number | F_Char,
        Numpad7 = VKey.NUMPAD7 | F_Numpad | F_Number | F_Char,
        Numpad8 = VKey.NUMPAD8 | F_Numpad | F_Number | F_Char,
        Numpad9 = VKey.NUMPAD9 | F_Numpad | F_Number | F_Char,
        NumpadDot = VKey.DECIMAL | F_Numpad | F_Char,

        NumpadIns = VKey.INSERT | F_Numpad,
        NumpadEnd = VKey.END | F_Numpad,
        NumpadDown = VKey.DOWN | F_Numpad,
        NumpadPgDn = VKey.NEXT | F_Numpad,
        NumpadLeft = VKey.LEFT | F_Numpad,
        NumpadClear = VKey.CLEAR | F_Numpad,
        NumpadRight = VKey.RIGHT | F_Numpad,
        NumpadHome = VKey.HOME | F_Numpad,
        NumpadUp = VKey.UP | F_Numpad,
        NumpadPgUp = VKey.PRIOR | F_Numpad,
        NumpadDel = VKey.DELETE | F_Numpad,

        NumpadDiv = VKey.DIVIDE | F_Numpad | F_Char | F_Extended,
        NumpadMult = VKey.MULTIPLY | F_Numpad | F_Char,
        NumpadSub = VKey.SUBTRACT | F_Numpad | F_Char,
        NumpadAdd = VKey.ADD | F_Numpad | F_Char,
        NumpadEnter = VKey.RETURN | F_Numpad | F_Extended,

        Numlock = VKey.NUMLOCK | F_Numpad | F_Extended | F_Toggle,
        ScrollLock = VKey.SCROLL | F_Toggle,
        CapsLock = VKey.CAPITAL | F_Toggle,

        F1 = VKey.F1,
        F2 = VKey.F2,
        F3 = VKey.F3,
        F4 = VKey.F4,
        F5 = VKey.F5,
        F6 = VKey.F6,
        F7 = VKey.F7,
        F8 = VKey.F8,
        F9 = VKey.F9,
        F10 = VKey.F10,
        F11 = VKey.F11,
        F12 = VKey.F12,

        F13 = VKey.F13,
        F14 = VKey.F14,
        F15 = VKey.F15,
        F16 = VKey.F16,
        F17 = VKey.F17,
        F18 = VKey.F18,
        F19 = VKey.F19,
        F20 = VKey.F20,
        F21 = VKey.F21,
        F22 = VKey.F22,
        F23 = VKey.F23,
        F24 = VKey.F24,

        /// <summary>Left arrow key</summary>
        Left = VKey.LEFT | F_Extended,
        /// <summary>Right arrow key</summary>
        Right = VKey.RIGHT | F_Extended,
        /// <summary>Up arrow key</summary>
        Up = VKey.UP | F_Extended,
        /// <summary>Down arrow key</summary>
        Down = VKey.DOWN | F_Extended,

        /// <summary>Left shift key</summary>
        LShift = VKey.LSHIFT | Shift,
        /// <summary>Right shift key</summary>
        RShift = VKey.RSHIFT | Shift,
        /// <summary>Left control key</summary>
        LCtrl = VKey.LCONTROL | Ctrl,
        /// <summary>Right control key</summary>
        RCtrl = VKey.RCONTROL | Ctrl | F_Extended,
        /// <summary>Left alt key</summary>
        LAlt = VKey.LMENU | Alt,
        /// <summary>Right alt key</summary>
        RAlt = VKey.RMENU | Alt | F_Extended,
        /// <summary>Left win key</summary>
        LWin = VKey.LWIN | Win,
        /// <summary>Right win key</summary>
        RWin = VKey.RWIN | Win,

        Enter = VKey.RETURN | F_Char,
        Return = Enter,
        Space = VKey.SPACE | F_Char,
        Tab = VKey.TAB | F_Char,
        Backspace = VKey.BACK,
        Escape = VKey.ESCAPE,
        /// <summary>Context menu key</summary>
        App = VKey.APPS,
        /// <summary>Context menu key</summary>
        Context = App,

        /// <summary>Print screen key</summary>
        PrintScrn = VKey.SNAPSHOT | F_Extended,
        /// <summary>Pause break key</summary>
        Pause = VKey.PAUSE,
        Insert = VKey.INSERT | F_Extended,
        Home = VKey.HOME | F_Extended,
        End = VKey.END | F_Extended,
        Delete = VKey.DELETE | F_Extended,
        PageUp = VKey.PRIOR | F_Extended,
        PageDown = VKey.NEXT | F_Extended,
        PgUp = PageUp,
        PgDn = PageDown,

        /// <summary>The general back key</summary>
        BrowserBack = VKey.BROWSER_BACK,
        /// <summary>The general forward key</summary>
        BrowserForward = VKey.BROWSER_FORWARD,
        BrowserFavorites = VKey.BROWSER_FAVORITES,
        BrowserHome = VKey.BROWSER_HOME,
        BrowserRefresh = VKey.BROWSER_REFRESH,
        BrowserSearch = VKey.BROWSER_SEARCH,
        BrowserStop = VKey.BROWSER_STOP,

        MediaNext = VKey.MEDIA_NEXT_TRACK | F_Media,
        MediaPrev = VKey.MEDIA_PREV_TRACK | F_Media,
        MediaStop = VKey.MEDIA_STOP | F_Media,
        /// <summary>Media play pause key</summary>
        MediaPlay = VKey.MEDIA_PLAY_PAUSE | F_Media,

        VolumeUp = VKey.VOLUME_UP | F_Media,
        VolumeDown = VKey.VOLUME_DOWN | F_Media,
        VolumeMute = VKey.VOLUME_MUTE | F_Media,

        LaunchApp1 = VKey.LAUNCH_APP1,
        LaunchApp2 = VKey.LAUNCH_APP2,
        LaunchMail = VKey.LAUNCH_MAIL,
        LaunchMediaSelect = VKey.LAUNCH_MEDIA_SELECT,

        Plus = VKey.OEM_PLUS | F_Char,
        Minus = VKey.OEM_MINUS | F_Char,
        Comma = VKey.OEM_COMMA | F_Char,
        Period = VKey.OEM_PERIOD | F_Char,

        /// <summary>The ¨ key</summary>
        Umlaut = VKey.OEM_1 | F_Char,
        /// <summary>The ' key</summary>
        Apostrophe = VKey.OEM_2 | F_Char,
        /// <summary>The Ö key</summary>
        Ö = VKey.OEM_3 | F_Char,
        /// <summary>The ´ key</summary>
        Tilde = VKey.OEM_4 | F_Char,
        /// <summary>The § key</summary>
        Section = VKey.OEM_5 | F_Char,
        /// <summary>The Å key</summary>
        Å = VKey.OEM_6 | F_Char,
        /// <summary>The Ä key</summary>
        Ä = VKey.OEM_7 | F_Char,
        /// <summary>The &lt; key</summary>
        Less = VKey.OEM_102 | F_Char,

        OEM_1 = Umlaut,
        OEM_2 = Apostrophe,
        OEM_3 = Ö,
        OEM_4 = Tilde,
        OEM_5 = Section,
        OEM_6 = Å,
        OEM_7 = Ä,
        OEM_8 = VKey.OEM_8,
        OEM_102 = Less,
        OEM_Clear = VKey.OEM_CLEAR,
        OEM_Plus = Plus,
        OEM_Minus = Minus,
        OEM_Comma = Comma,
        OEM_Period = Period,

        // Other //

        Packet = VKey.PACKET,
        Kana = VKey.KANA,
        Junja = VKey.JUNJA,
        Kanji = VKey.KANJI,
        Convert = VKey.CONVERT,
        NonConvert = VKey.NONCONVERT,
        ModeChange = VKey.MODECHANGE,
        Break = VKey.CANCEL | F_Extended,
        Clear = VKey.CLEAR | F_Extended,
        Print = VKey.PRINT,
        Final = VKey.FINAL,
        Accept = VKey.ACCEPT,
        Select = VKey.SELECT,
        Execute = VKey.EXECUTE,
        Help = VKey.HELP,
        Sleep = VKey.SLEEP,
        Separator = VKey.SEPARATOR,
        ProcessKey = VKey.PROCESSKEY,
        Attn = VKey.ATTN,
        CrSel = VKey.CRSEL,
        ExSel = VKey.EXSEL,
        EraseEOF = VKey.EREOF,
        Play = VKey.PLAY,
        Zoom = VKey.ZOOM,
        NoName = VKey.NONAME,
        Pa1 = VKey.PA1,

        // Custom //

        /// <summary>This represents a key that does not exist</summary>
        None = 1 | F_Custom,

        /// <summary>This represents a key that was not recognized</summary>
        Unknown = 2 | F_Custom,

        /// <summary>Mouse wheel left</summary>
        WheelLeft = 3 | F_Scroll | F_Mouse | F_Custom | F_Stateless,
        /// <summary>Mouse wheel right</summary>
        WheelRight = 4 | F_Scroll | F_Mouse | F_Custom | F_Stateless,
        /// <summary>Mouse wheel up</summary>
        WheelUp = 5 | F_Scroll | F_Mouse | F_Custom | F_Stateless,
        /// <summary>Mouse wheel down</summary>
        WheelDown = 6 | F_Scroll | F_Mouse | F_Custom | F_Stateless,

        /// <summary>Mouse movement</summary>
        MouseMove = 7 | F_Mouse | F_Custom | F_Stateless
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    #endregion
}
