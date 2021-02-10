using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WinUtilities {

    /// <summary>Specifies what method is used to send the input</summary>
    public enum SendMode {
        /// <summary>Uses the windows SendInput API. Fast and reliable, but rejected by certain applications.</summary>
        Input,
        /// <summary>Uses the event API. Slower and less reliable, but more compatible with some applications.</summary>
        Event,
        /// <summary>Sends key messages directly. Least reliable, but might be able to send input to background windows directly.</summary>
        Control
    }

    /// <summary>Class for sending native windows input</summary>
    public static class Input {

        /// <summary>The text character for parsing special input when sending text. Default is '['.</summary>
        public static char ParseOpen { get; } = '[';
        /// <summary>The text character for parsing special input when sending text. Default is ']'.</summary>
        public static char ParseClose { get; } = ']';

        private const int keydownflag = 0x00000001;
        private const int keyupflag = unchecked((int) 0xC0000001);

        #region mappings
        private static Dictionary<KeyState, WinAPI.MOUSEEVENTF> MouseMap { get; } = new Dictionary<KeyState, WinAPI.MOUSEEVENTF> {
            { new KeyState(Key.LButton, true), WinAPI.MOUSEEVENTF.LEFTDOWN },
            { new KeyState(Key.LButton, false), WinAPI.MOUSEEVENTF.LEFTUP },
            { new KeyState(Key.RButton, true), WinAPI.MOUSEEVENTF.RIGHTDOWN },
            { new KeyState(Key.RButton, false), WinAPI.MOUSEEVENTF.RIGHTUP },
            { new KeyState(Key.MButton, true), WinAPI.MOUSEEVENTF.MIDDLEDOWN },
            { new KeyState(Key.MButton, false), WinAPI.MOUSEEVENTF.MIDDLEUP }
        };

        /// <summary>List of short modifier symbols used in parsed string input</summary>
        public static Dictionary<char, Key> ShortModifiers { get; } = new Dictionary<char, Key> {
            { '+', Key.RShift },
            { '!', Key.LAlt },
            { '#', Key.RWin },
            { '^', Key.RCtrl }
        };

        /// <summary>Retrieve the Key equivalent from a <see cref="WM"/> message</summary>
        public static KeyState FromMouseEvent(WM message, int data = 0) {
            switch (message) {
                case WM.LBUTTONDOWN: {
                    return new KeyState(Key.LButton, true);
                }
                case WM.LBUTTONUP: {
                    return new KeyState(Key.LButton, false);
                }
                case WM.RBUTTONDOWN: {
                    return new KeyState(Key.RButton, true);
                }
                case WM.RBUTTONUP: {
                    return new KeyState(Key.RButton, false);
                }
                case WM.MBUTTONDOWN: {
                    return new KeyState(Key.MButton, true);
                }
                case WM.MBUTTONUP: {
                    return new KeyState(Key.MButton, false);
                }
                case WM.MOUSEWHEEL: {
                    var key = data >> 16 > 0 ? Key.WheelUp : Key.WheelDown;
                    return new KeyState(key, true);
                }
                case WM.MOUSEHWHEEL: {
                    var key = data >> 16 > 0 ? Key.WheelRight : Key.WheelLeft;
                    return new KeyState(key, true);
                }
                case WM.XBUTTONDOWN: {
                    data = data >> 16;
                    if (data == 1) {
                        return new KeyState(Key.XButton1, true);
                    } else {
                        return new KeyState(Key.XButton2, true);
                    }
                }
                case WM.XBUTTONUP: {
                    data = data >> 16;
                    if (data == 1) {
                        return new KeyState(Key.XButton1, false);
                    } else {
                        return new KeyState(Key.XButton2, false);
                    }
                }
                case WM.MOUSEMOVE: {
                    return new KeyState(Key.MouseMove, true);
                }
            }

            throw new Exception("Unknown mouse input" + message);
        }
        #endregion

        #region user methods
        /// <summary>Get the logical state of a key. Does not differentiate between extended and non-extended keys</summary>
        public static bool GetKeyState(Key key) => WinAPI.GetAsyncKeyState(key.AsVirtualKey()) < 0;

        /// <summary>Send text input.</summary>
        /// <remarks>
        /// The <paramref name="text"/> is parsed for special input when between [ and ]. This behaviour can be escaped with [[ and ]].
        /// <para/>Any entity, a single character or a special input [], can be preceded with a modifier set like: [+!][Enter] -> Shift + Alt + Enter -OR- [#]p -> Win + P.
        /// <para/>The modifiers supported are RShift(+), RCtrl(^), LAlt(!) or RWin(#). 
        /// <para/>The general layout of normal special input is [key up/down (amount) text] where anything besides the key can be omitted or included as needed.
        /// <para/>Specifying up or down will send only that event, instead of sending both down and up events.
        /// <para/>Specifying 'text' in the special input sends the 'key' as text instead. For example [Enter 5] sends the Enter key five times but [Enter 5 text] sends the text 'Enter' 5 times.
        /// <para/>While using 'text' the 'key' part must not contain any spaces. This makes sending long text difficult, but is useful in sending a specific symbol many times, like [§ 10 text].
        /// </remarks>
        public static void Send(string text, SendMode mode) {
            if (mode == SendMode.Input) {
                Send(text);
            } else if (mode == SendMode.Event) {
                SendEvent(text);
            } else {
                SendControl(Window.Active, text);
            }
        }

        /// <summary>Send raw text input</summary>
        public static void SendRaw(string text, SendMode mode) {
            if (mode == SendMode.Input) {
                SendRaw(text);
            } else if (mode == SendMode.Event) {
                SendEventRaw(text);
            } else {
                SendControlRaw(Window.Active, text);
            }
        }

        /// <summary>Send char input</summary>
        public static void Send(char c, SendMode mode) {
            if (mode == SendMode.Input) {
                Send(c);
            } else if (mode == SendMode.Event) {
                SendEvent(c);
            } else {
                SendControl(Window.Active, c);
            }
        }

        #region send input
        /// <summary>Send text in Input mode.</summary>
        /// <remarks>
        /// The <paramref name="text"/> is parsed for special input when between [ and ]. This behaviour can be escaped with [[ and ]].
        /// <para/>Any entity, a single character or a special input [], can be preceded with a modifier set like: [+!][Enter] -> Shift + Alt + Enter -OR- [#]p -> Win + P.
        /// <para/>The modifiers supported are RShift(+), RCtrl(^), LAlt(!) or RWin(#). 
        /// <para/>The general layout of normal special input is [key up/down (amount) text] where anything besides the key can be omitted or included as needed.
        /// <para/>Specifying up or down will send only that event, instead of sending both down and up events.
        /// <para/>Specifying 'text' in the special input sends the 'key' as text instead. For example [Enter 5] sends the Enter key five times but [Enter 5 text] sends the text 'Enter' 5 times.
        /// <para/>While using 'text' the 'key' part must not contain any spaces. This makes sending long text difficult, but is useful in sending a specific symbol many times, like [§ 10 text].
        /// </remarks>
        public static bool Send(string text) => SendInput(ToInputList(text));
        /// <summary>Send chars in Input mode</summary>
        public static bool Send(params char[] chars) => SendInput(chars.SelectMany(c => GetCharInput(c)).ToArray());
        /// <summary>Send keys in Input mode</summary>
        public static bool Send(params Key[] keys) => SendInput(ToInputList(keys));
        /// <summary>Send down keys in Input mode</summary>
        public static bool SendDown(params Key[] keys) => SendInput(ToInputList(keys, true));
        /// <summary>Send up keys in Input mode</summary>
        public static bool SendUp(params Key[] keys) => SendInput(ToInputList(keys, false));
        /// <summary>Send raw text in Input mode. This string is not parsed in any way before sending.</summary>
        public static bool SendRaw(string text) => SendInput(text.SelectMany(c => GetCharInput(c)).ToArray());

        /// <summary>Send a scroll event in Input mode</summary>
        public static bool Scroll(Key key, int amount) => SendInput(GetMouseInputScroll(key, amount));
        /// <summary>Send relative mouse movement in Input mode</summary>
        public static bool MouseMoveRelative(int dx, int dy) => SendInput(GetMouseInputMove(dx, dy));

        /// <summary>Send lenghtened key presses that are easily recognized by game like applications in Input mode. Up event is delayed by 20 ms.</summary>
        public static async Task SendGame(params Key[] keys) => await SendGame(20, keys);
        /// <summary>Send lenghtened key presses that are easily recognized by game like applications in Input mode</summary>
        /// <param name="delay">The amount of time before the up event is sent</param>
        /// <param name="keys">The keys to send as input</param>
        public static async Task SendGame(int delay, params Key[] keys) {
            SendInput(keys.Select(k => GetInput(k, true)).ToArray());
            await Task.Delay(delay);
            SendInput(keys.Select(k => GetInput(k, false)).ToArray());
        }
        #endregion

        #region send event
        /// <summary>Send text in Event mode.</summary>
        /// <remarks>
        /// The <paramref name="text"/> is parsed for special input when between [ and ]. This behaviour can be escaped with [[ and ]].
        /// <para/>Any entity, a single character or a special input [], can be preceded with a modifier set like: [+!][Enter] -> Shift + Alt + Enter -OR- [#]p -> Win + P.
        /// <para/>The modifiers supported are RShift(+), RCtrl(^), LAlt(!) or RWin(#). 
        /// <para/>The general layout of normal special input is [key up/down (amount) text] where anything besides the key can be omitted or included as needed.
        /// <para/>Specifying up or down will send only that event, instead of sending both down and up events.
        /// <para/>Specifying 'text' in the special input sends the 'key' as text instead. For example [Enter 5] sends the Enter key five times but [Enter 5 text] sends the text 'Enter' 5 times.
        /// <para/>While using 'text' the 'key' part must not contain any spaces. This makes sending long text difficult, but is useful in sending a specific symbol many times, like [§ 10 text].
        /// </remarks>
        public static void SendEvent(string text) => SendEvent(ToInputList(text));
        /// <summary>Send chars in Event mode</summary>
        public static void SendEvent(params char[] chars) => SendEvent(chars.SelectMany(c => GetCharInput(c)).ToArray());
        /// <summary>Send keys in Event mode</summary>
        public static void SendEvent(params Key[] keys) => SendEvent(ToInputList(keys));
        /// <summary>Send down keys in Event mode</summary>
        public static void SendEventDown(params Key[] keys) => SendEvent(ToInputList(keys, true));
        /// <summary>Send up keys in Event mode</summary>
        public static void SendEventUp(params Key[] keys) => SendEvent(ToInputList(keys, false));
        /// <summary>Send raw text in Event mode. This string is not parsed in any way before sending.</summary>
        public static void SendEventRaw(string text) => SendEvent(text.SelectMany(c => GetCharInput(c)).ToArray());


        /// <summary>Send lenghtened key presses that are easily recognized by game like applications in Event mode. Up event is delayed by 20 ms.</summary>
        public static async Task SendEventGame(params Key[] keys) => await SendEventGame(20, keys);
        /// <summary>Send lenghtened key presses that are easily recognized by game like applications in Event mode</summary>
        /// <param name="delay">The amount of time before the up event is sent</param>
        /// <param name="keys">The keys to send as input</param>
        public static async Task SendEventGame(int delay, params Key[] keys) {
            SendInput(keys.Select(k => GetInput(k, true)).ToArray());
            await Task.Delay(delay);
            SendInput(keys.Select(k => GetInput(k, false)).ToArray());
        }
        #endregion

        #region send control
        /// <summary>Send text in Control mode.</summary>
        /// <remarks>
        /// The <paramref name="text"/> is parsed for special input when between [ and ]. This behaviour can be escaped with [[ and ]].
        /// <para/>Any entity, a single character or a special input [], can be preceded with a modifier set like: [+!][Enter] -> Shift + Alt + Enter -OR- [#]p -> Win + P.
        /// <para/>The modifiers supported are RShift(+), RCtrl(^), LAlt(!) or RWin(#). 
        /// <para/>The general layout of normal special input is [key up/down (amount) text] where anything besides the key can be omitted or included as needed.
        /// <para/>Specifying up or down will send only that event, instead of sending both down and up events.
        /// <para/>Specifying 'text' in the special input sends the 'key' as text instead. For example [Enter 5] sends the Enter key five times but [Enter 5 text] sends the text 'Enter' 5 times.
        /// <para/>While using 'text' the 'key' part must not contain any spaces. This makes sending long text difficult, but is useful in sending a specific symbol many times, like [§ 10 text].
        /// </remarks>
        public static void SendControl(Window window, string text) => SendControl(window, ToInputList(text));
        /// <summary>Send chars in Control mode</summary>
        public static void SendControl(Window window, params char[] chars) => SendControlText(window, chars);
        /// <summary>Send keys in Control mode</summary>
        public static void SendControl(Window window, params Key[] keys) => SendControl(window, ToInputList(keys));
        /// <summary>Send down keys in Control mode</summary>
        public static void SendControlDown(Window window, params Key[] keys) => SendControl(window, ToInputList(keys, true));
        /// <summary>Send up keys in Control mode</summary>
        public static void SendControlUp(Window window, params Key[] keys) => SendControl(window, ToInputList(keys, false));
        /// <summary>Send raw text in Control mode. This string is not parsed in any way before sending.</summary>
        public static void SendControlRaw(Window window, string text) => SendControlText(window, text.ToArray());
        #endregion

        #endregion

        #region helper
        /// <summary>Send input with the SendInput API</summary>
        private static bool SendInput(params WinAPI.INPUT[] inputs) {
            if (inputs.Length < 1)
                return true;
            return WinAPI.SendInput((uint) inputs.Length, inputs, WinAPI.INPUT.Size) != 0;
        }

        /// <summary>Send input with the event API</summary>
        private static void SendEvent(params WinAPI.INPUT[] inputs) {
            if (inputs.Length < 1)
                return;

            foreach (var input in inputs) {
                if (input.type == WinAPI.InputType.Keyboard) {
                    var kInput = input.union.keyboard;
                    WinAPI.keybd_event(kInput.vk, kInput.sc, kInput.flags, kInput.extraInfo);
                } else if (input.type == WinAPI.InputType.Mouse) {
                    var mInput = input.union.mouse;
                    WinAPI.mouse_event(mInput.flags, mInput.dx, mInput.dy, mInput.mouseData, mInput.extraInfo);
                }
            }
        }

        #region control send
        /// <summary>Send input directly to windows</summary>
        private static void SendControl(Window window, params WinAPI.INPUT[] inputs) {
            foreach (var input in inputs) {
                if (input.type == WinAPI.InputType.Mouse) {
                    SendControlMouse(window, input.union.mouse);
                } else {
                    SendControlKeyboard(window, input.union.keyboard);
                }
            }
        }

        private static void SendControlKeyboard(Window window, WinAPI.KEYBDINPUT input) {
            int scan = (int) input.sc | (input.flags.HasFlag(WinAPI.KEYEVENTF.EXTENDEDKEY) ? 1 << 24 : 0);

            if (input.flags.HasFlag(WinAPI.KEYEVENTF.UNICODE)) {
                if (!input.flags.HasFlag(WinAPI.KEYEVENTF.KEYUP))
                    window.PostMessage(WM.CHAR, (int) input.sc, 0);
            } else if (!input.flags.HasFlag(WinAPI.KEYEVENTF.KEYUP))
                window.PostMessage(WM.KEYDOWN, (int) input.vk, scan | keydownflag);
            else
                window.PostMessage(WM.KEYUP, (int) input.vk, scan | keyupflag);
        }

        private static void SendControlMouse(Window window, WinAPI.MOUSEINPUT input) {
            WM message;
            int wParam = 0;
            int lParam = 0;

            if (input.flags.HasFlag(WinAPI.MOUSEEVENTF.LEFTDOWN)) {
                message = WM.LBUTTONDOWN;
                wParam = 1;
                lParam = Mouse.Position.SetRelative(window.ClientArea).AsValue;

            } else if (input.flags.HasFlag(WinAPI.MOUSEEVENTF.LEFTUP)) {
                message = WM.LBUTTONUP;
                lParam = Mouse.Position.SetRelative(window.ClientArea).AsValue;

            } else if (input.flags.HasFlag(WinAPI.MOUSEEVENTF.RIGHTDOWN)) {
                message = WM.RBUTTONDOWN;
                wParam = 2;
                lParam = Mouse.Position.SetRelative(window.ClientArea).AsValue;

            } else if (input.flags.HasFlag(WinAPI.MOUSEEVENTF.RIGHTUP)) {
                message = WM.RBUTTONUP;
                lParam = Mouse.Position.SetRelative(window.ClientArea).AsValue;

            } else if (input.flags.HasFlag(WinAPI.MOUSEEVENTF.MIDDLEDOWN)) {
                message = WM.MBUTTONDOWN;
                wParam = 16;
                lParam = Mouse.Position.SetRelative(window.ClientArea).AsValue;

            } else if (input.flags.HasFlag(WinAPI.MOUSEEVENTF.MIDDLEUP)) {
                message = WM.MBUTTONUP;
                lParam = Mouse.Position.SetRelative(window.ClientArea).AsValue;

            } else if (input.flags.HasFlag(WinAPI.MOUSEEVENTF.MOVE) || input.flags.HasFlag(WinAPI.MOUSEEVENTF.MOVE_NOCOALESCE)) {
                message = WM.MOUSEMOVE;
                lParam = Coord.Zero.AsValue;

            } else if (input.flags.HasFlag(WinAPI.MOUSEEVENTF.WHEEL)) {
                message = WM.MOUSEWHEEL;
                wParam = input.mouseData << 16;

            } else if (input.flags.HasFlag(WinAPI.MOUSEEVENTF.HWHEEL)) {
                message = WM.MOUSEHWHEEL;
                wParam = input.mouseData << 16;

            } else if (input.flags.HasFlag(WinAPI.MOUSEEVENTF.XDOWN)) {
                message = WM.XBUTTONDOWN;
                wParam = input.mouseData << 16;
                lParam = input.mouseData == 1 ? 32 : 64;

            } else if (input.flags.HasFlag(WinAPI.MOUSEEVENTF.XUP)) {
                message = WM.XBUTTONUP;
                wParam = input.mouseData << 16;

            } else {
                throw new Exception("Illegal mouse input event");
            }

            window.PostMessage(message, wParam, lParam);
        }

        private static void SendControlText(Window window, params char[] chars) {
            foreach (char c in chars) {
                if (c == '\r') continue;
                if (c == '\n') SendControl(window, Key.Enter);
                else window.PostMessage(WM.CHAR, c, 0);
            }
        }
        #endregion

        /// <summary>Build a keyboard input object with given data</summary>
        private static WinAPI.INPUT RawKeyboardInput(WinAPI.KEYEVENTF flags, Key key = 0, ScanCode sc = 0) {
            return new WinAPI.INPUT {
                type = WinAPI.InputType.Keyboard,
                union = {
                    keyboard = new WinAPI.KEYBDINPUT {
                        vk = key == 0 ? 0 : key.AsVirtualKey(),
                        sc = sc != 0 ? sc : key.AsScanCode(),
                        flags = flags,
                        time = 0,
                        extraInfo = UIntPtr.Zero
                    }
                }
            };
        }

        /// <summary>Build a mouse input object with given data</summary>
        private static WinAPI.INPUT RawMouseInput(WinAPI.MOUSEEVENTF flags, int data = 0, int dx = 0, int dy = 0) {
            return new WinAPI.INPUT {
                type = WinAPI.InputType.Mouse,
                union = {
                    mouse = new WinAPI.MOUSEINPUT {
                        dx = dx,
                        dy = dy,
                        mouseData = data,
                        flags = flags,
                        time = 0,
                        extraInfo = UIntPtr.Zero
                    }
                }
            };
        }

        /// <summary>Get a key as input</summary>
        private static WinAPI.INPUT GetInput(Key key, bool state) {
            if (key.IsMouse())
                return GetMouseInput(key, state);
            return GetKeyboardInput(key, state);
        }

        /// <summary>Get a keyboard event as input</summary>
        private static WinAPI.INPUT GetKeyboardInput(Key key, bool state) {
            var flags = key.IsExtended() ? WinAPI.KEYEVENTF.EXTENDEDKEY : 0;
            if (!state) flags |= WinAPI.KEYEVENTF.KEYUP;

            return RawKeyboardInput(flags, key);
        }

        /// <summary>Get a mouse event as input</summary>
        private static WinAPI.INPUT GetMouseInput(Key key, bool state) {
            if (key.IsScroll()) {
                if (!state)
                    throw new Exception("Scroll keys cannot be released.");
                return GetMouseInputScroll(key);
            } else if (key == Key.XButton1 || key == Key.XButton2) {
                return GetMouseInputX(key, state);
            }

            var flags = MouseMap[new KeyState(key, state)];

            return RawMouseInput(flags);
        }

        /// <summary>Get a mouse extra button event as input</summary>
        private static WinAPI.INPUT GetMouseInputX(Key key, bool state) {
            var flags = state ? WinAPI.MOUSEEVENTF.XDOWN : WinAPI.MOUSEEVENTF.XUP;
            var data = key == Key.XButton1 ? 1 : 2;

            return RawMouseInput(flags, data);
        }

        /// <summary>Get a scroll event as input</summary>
        private static WinAPI.INPUT GetMouseInputScroll(Key key, int amount = 120) {
            if (amount < 0) {
                throw new ArgumentException("Scroll amount cannot be negative.");
            }

            WinAPI.MOUSEEVENTF flags;

            if (key == Key.WheelLeft) {
                flags = WinAPI.MOUSEEVENTF.HWHEEL;
                amount *= -1;
            } else if (key == Key.WheelRight) {
                flags = WinAPI.MOUSEEVENTF.HWHEEL;
            } else if (key == Key.WheelUp) {
                flags = WinAPI.MOUSEEVENTF.WHEEL;
            } else if (key == Key.WheelDown) {
                flags = WinAPI.MOUSEEVENTF.WHEEL;
                amount *= -1;
            } else {
                throw new ArgumentException("The provided key must be a scroll key.");
            }

            return RawMouseInput(flags, amount);
        }

        /// <summary>Get a mouse move as input</summary>
        private static WinAPI.INPUT GetMouseInputMove(int dx, int dy, bool coalesce = true) {
            var flags = coalesce ? WinAPI.MOUSEEVENTF.MOVE : WinAPI.MOUSEEVENTF.MOVE_NOCOALESCE;

            return RawMouseInput(flags, 0, dx, dy);
        }

        /// <summary>Get an input object from a character</summary>
        private static WinAPI.INPUT[] GetCharInput(char c) {
            ScanCode scan = (ScanCode) c;
            var flags = WinAPI.KEYEVENTF.UNICODE;
            if (scan.IsExtended()) flags |= WinAPI.KEYEVENTF.EXTENDEDKEY;

            var down = RawKeyboardInput(flags, sc: scan);
            var up = RawKeyboardInput(flags | WinAPI.KEYEVENTF.KEYUP, sc: scan);

            return new WinAPI.INPUT[] { down, up };
        }

        /// <summary>Turn an array of keys into inputs</summary>
        private static WinAPI.INPUT[] ToInputList(Key[] keys, bool? state = null) {
            List<WinAPI.INPUT> input = new List<WinAPI.INPUT>();

            foreach (Key key in keys) {
                if (state is bool bstate) {
                    if (bstate)
                        input.Add(GetInput(key, true));
                    else if (!key.IsStateless())
                        input.Add(GetInput(key, false));
                } else {
                    input.Add(GetInput(key, true));
                    if (!key.IsStateless()) {
                        input.Add(GetInput(key, false));
                    }
                }
            }

            return input.ToArray();
        }

        #region string input parsing
        /// <summary>Parse a string to a list of inputs</summary>
        private static WinAPI.INPUT[] ToInputList(string s) {
            s = Regex.Replace(s, @"\r?\n|\r\n?", "[Enter]");
            char[] chars = s.ToCharArray();
            List<WinAPI.INPUT> res = new List<WinAPI.INPUT>();
            var pendingModifiers = new List<Key>();

            for (int i = 0; i < chars.Length; i++) {

                // Parse starting
                if (chars[i] == ParseOpen) {

                    // Fault: end of string can't be the start of a parse
                    if (i + 1 >= chars.Length) {
                        throw new Exception($"No pair found for {ParseOpen}");

                        // Parse character was escaped
                    } else if (chars[i + 1] == ParseOpen) {
                        res.AddRange(GetCharInput(chars[i]));
                        i++;

                        // Parsing following text of parse area
                    } else {
                        var next = chars.IndexOfNext(ParseClose, i + 1);

                        // Fault: end of parse not found
                        if (next < 0)
                            throw new Exception($"No pair found for {ParseOpen}");
                        var length = next - (i + 1);

                        // Fault: parse string was empty
                        if (length < 1)
                            throw new Exception("A parse pair cannot be empty");
                        var temp = s.Substring(i + 1, length).ToLower();

                        // Parse string was a set of modifiers
                        if (Regex.IsMatch(temp, @"^(\+|!|#|\^){1,4}$")) {
                            foreach (var c in temp) pendingModifiers.Add(ShortModifiers[c]);

                            // Parse input has leading modifiers
                        } else if (pendingModifiers.Count > 0) {
                            res.AddRange(pendingModifiers.Select(k => GetInput(k, true)));
                            res.AddRange(new InputParseObject(temp).Parse());
                            res.AddRange(pendingModifiers.Select(k => GetInput(k, false)));
                            pendingModifiers.Clear();

                            // Parse string normally
                        } else {
                            res.AddRange(new InputParseObject(temp).Parse());
                        }

                        i = next;
                    }

                    // Parse close character outside of parse
                } else if (chars[i] == ParseClose) {
                    if (i + 1 >= chars.Length || chars[i + 1] != ParseClose) {
                        throw new Exception($"Unexpected closing {ParseClose}");
                    }

                    // Normal characters
                } else {

                    // Input character has leading modifiers
                    if (pendingModifiers.Count > 0) {
                        res.AddRange(pendingModifiers.Select(k => GetInput(k, true)));
                        res.AddRange(GetCharInput(chars[i]));
                        res.AddRange(pendingModifiers.Select(k => GetInput(k, false)));
                        pendingModifiers.Clear();

                        // Input character is handled normally
                    } else {
                        res.AddRange(GetCharInput(chars[i]));
                    }
                }
            }

            return res.ToArray();
        }

        /// <summary>Return the index of the next matching item of an array</summary>
        private static int IndexOfNext<T>(this T[] ar, T item, int start) {
            for (int i = start; i < ar.Length; i++) {
                if (ar[i].Equals(item)) {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>Object used to parse special text input</summary>
        private class InputParseObject {

            public bool Unicode { get; }
            public bool? State { get; }
            public int Count { get; }
            public string KeyString { get; }
            public Key? Key { get; }
            public List<WinAPI.INPUT> Inputs { get; private set; }

            public InputParseObject(string s) {
                if (!Regex.IsMatch(s, @"^.+?( (text|down|up|\d+)){0,3}$", RegexOptions.IgnoreCase)) {
                    throw new Exception($"Unknown signature with '{s}'");
                }

                var data = s.Split(' ');

                Unicode = false;
                Count = 1;

                for (int i = 1; i < data.Length; i++) {
                    var temp = data[i].ToLower();
                    if (temp == "text") {
                        Unicode = true;
                    } else if (temp == "down") {
                        if (State != null)
                            throw new Exception("Cannot have both 'down' and 'up' in the same command");
                        State = true;
                    } else if (temp == "up") {
                        if (State != null)
                            throw new Exception("Cannot have both 'down' and 'up' in the same command");
                        State = false;
                    } else {
                        Count = int.Parse(temp);
                    }
                }

                KeyString = data[0];
                Key = Unicode ? null : (Key?) GetKey(KeyString);
            }

            private static Key GetKey(string keyString) {
                keyString = Regex.Replace(keyString, "Control", "Ctrl", RegexOptions.IgnoreCase);
                keyString = Regex.Replace(keyString, @"^\d$", "D" + keyString);

                if (!EnhancedKey.StringToKey.ContainsKey(keyString))
                    throw new Exception($"Key named {keyString} not found");
                return EnhancedKey.StringToKey[keyString];
            }

            public List<WinAPI.INPUT> Parse() {
                if (Inputs != null)
                    return Inputs;
                Inputs = new List<WinAPI.INPUT>();

                if (State != null) {
                    if (Unicode) {
                        Inputs.AddRange(KeyString.Select(c => GetCharInput(c)[(bool) State ? 0 : 1]));
                    } else {
                        Inputs.Add(GetKeyboardInput((Key) Key, (bool) State));
                    }
                } else {
                    for (int i = 0; i < Count; i++) {
                        if (Unicode) {
                            Inputs.AddRange(KeyString.SelectMany(c => GetCharInput(c)).ToArray());
                        }
                        if (((Key) Key).IsStateless()) {
                            Inputs.Add(GetInput((Key) Key, true));
                        } else {
                            Inputs.Add(GetInput((Key) Key, true));
                            Inputs.Add(GetInput((Key) Key, false));
                        }
                    }
                }

                return Inputs;
            }
        }
        #endregion

        #endregion
    }
}
