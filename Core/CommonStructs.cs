using System;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace WinUtilities {
    #region enums
    /// <summary>Enum of all Window Messages.</summary>
    public enum WM : uint {
        NULL = 0x00,
        CREATE = 0x01,
        DESTROY = 0x02,
        MOVE = 0x03,
        SIZE = 0x05,
        ACTIVATE = 0x06,
        SETFOCUS = 0x07,
        KILLFOCUS = 0x08,
        ENABLE = 0x0A,
        SETREDRAW = 0x0B,
        SETTEXT = 0x0C,
        GETTEXT = 0x0D,
        GETTEXTLENGTH = 0x0E,
        PAINT = 0x0F,
        CLOSE = 0x10,
        QUERYENDSESSION = 0x11,
        QUIT = 0x12,
        QUERYOPEN = 0x13,
        ERASEBKGND = 0x14,
        SYSCOLORCHANGE = 0x15,
        ENDSESSION = 0x16,
        SYSTEMERROR = 0x17,
        SHOWWINDOW = 0x18,
        CTLCOLOR = 0x19,
        WININICHANGE = 0x1A,
        SETTINGCHANGE = 0x1A,
        DEVMODECHANGE = 0x1B,
        ACTIVATEAPP = 0x1C,
        FONTCHANGE = 0x1D,
        TIMECHANGE = 0x1E,
        CANCELMODE = 0x1F,
        SETCURSOR = 0x20,
        MOUSEACTIVATE = 0x21,
        CHILDACTIVATE = 0x22,
        QUEUESYNC = 0x23,
        GETMINMAXINFO = 0x24,
        PAINTICON = 0x26,
        ICONERASEBKGND = 0x27,
        NEXTDLGCTL = 0x28,
        SPOOLERSTATUS = 0x2A,
        DRAWITEM = 0x2B,
        MEASUREITEM = 0x2C,
        DELETEITEM = 0x2D,
        VKEYTOITEM = 0x2E,
        CHARTOITEM = 0x2F,

        SETFONT = 0x30,
        GETFONT = 0x31,
        SETHOTKEY = 0x32,
        GETHOTKEY = 0x33,
        QUERYDRAGICON = 0x37,
        COMPAREITEM = 0x39,
        COMPACTING = 0x41,
        WINDOWPOSCHANGING = 0x46,
        WINDOWPOSCHANGED = 0x47,
        POWER = 0x48,
        COPYDATA = 0x4A,
        CANCELJOURNAL = 0x4B,
        NOTIFY = 0x4E,
        INPUTLANGCHANGEREQUEST = 0x50,
        INPUTLANGCHANGE = 0x51,
        TCARD = 0x52,
        HELP = 0x53,
        USERCHANGED = 0x54,
        NOTIFYFORMAT = 0x55,
        CONTEXTMENU = 0x7B,
        STYLECHANGING = 0x7C,
        STYLECHANGED = 0x7D,
        DISPLAYCHANGE = 0x7E,
        GETICON = 0x7F,
        SETICON = 0x80,

        NCCREATE = 0x81,
        NCDESTROY = 0x82,
        NCCALCSIZE = 0x83,
        NCHITTEST = 0x84,
        NCPAINT = 0x85,
        NCACTIVATE = 0x86,
        GETDLGCODE = 0x87,
        NCMOUSEMOVE = 0xA0,
        NCLBUTTONDOWN = 0xA1,
        NCLBUTTONUP = 0xA2,
        NCLBUTTONDBLCLK = 0xA3,
        NCRBUTTONDOWN = 0xA4,
        NCRBUTTONUP = 0xA5,
        NCRBUTTONDBLCLK = 0xA6,
        NCMBUTTONDOWN = 0xA7,
        NCMBUTTONUP = 0xA8,
        NCMBUTTONDBLCLK = 0xA9,
        NCXBUTTONDOWN = 0xAB,
        NCXBUTTONUP = 0xAC,
        NCXBUTTONDBLCLK = 0xAD,

        KEYDOWN = 0x100,
        KEYFIRST = 0x100,
        KEYUP = 0x101,
        CHAR = 0x102,
        DEADCHAR = 0x103,
        SYSKEYDOWN = 0x104,
        SYSKEYUP = 0x105,
        SYSCHAR = 0x106,
        SYSDEADCHAR = 0x107,
        KEYLAST = 0x108,
        UNICHAR = 0x109,

        IME_STARTCOMPOSITION = 0x10D,
        IME_ENDCOMPOSITION = 0x10E,
        IME_COMPOSITION = 0x10F,
        IME_KEYLAST = 0x10F,

        INITDIALOG = 0x110,
        COMMAND = 0x111,
        SYSCOMMAND = 0x112,
        TIMER = 0x113,
        HSCROLL = 0x114,
        VSCROLL = 0x115,
        INITMENU = 0x116,
        INITMENUPOPUP = 0x117,
        MENUSELECT = 0x11F,
        MENUCHAR = 0x120,
        ENTERIDLE = 0x121,

        CTLCOLORMSGBOX = 0x132,
        CTLCOLOREDIT = 0x133,
        CTLCOLORLISTBOX = 0x134,
        CTLCOLORBTN = 0x135,
        CTLCOLORDLG = 0x136,
        CTLCOLORSCROLLBAR = 0x137,
        CTLCOLORSTATIC = 0x138,

        MOUSEMOVE = 0x200,
        MOUSEFIRST = 0x200,

        LBUTTONDOWN = 0x201,
        LBUTTONUP = 0x202,
        LBUTTONDBLCLK = 0x203,
        RBUTTONDOWN = 0x204,
        RBUTTONUP = 0x205,
        RBUTTONDBLCLK = 0x206,
        MBUTTONDOWN = 0x207,
        MBUTTONUP = 0x208,
        MBUTTONDBLCLK = 0x209,
        XBUTTONDOWN = 0x20B,
        XBUTTONUP = 0x20C,
        XBUTTONDBLCLK = 0x20D,

        MOUSEWHEEL = 0x20A,
        MOUSEHWHEEL = 0x20E,

        PARENTNOTIFY = 0x210,
        ENTERMENULOOP = 0x211,
        EXITMENULOOP = 0x212,
        NEXTMENU = 0x213,
        SIZING = 0x214,
        CAPTURECHANGED = 0x215,
        MOVING = 0x216,
        POWERBROADCAST = 0x218,
        DEVICECHANGE = 0x219,

        MDICREATE = 0x220,
        MDIDESTROY = 0x221,
        MDIACTIVATE = 0x222,
        MDIRESTORE = 0x223,
        MDINEXT = 0x224,
        MDIMAXIMIZE = 0x225,
        MDITILE = 0x226,
        MDICASCADE = 0x227,
        MDIICONARRANGE = 0x228,
        MDIGETACTIVE = 0x229,
        MDISETMENU = 0x230,
        ENTERSIZEMOVE = 0x231,
        EXITSIZEMOVE = 0x232,
        DROPFILES = 0x233,
        MDIREFRESHMENU = 0x234,

        IME_SETCONTEXT = 0x281,
        IME_NOTIFY = 0x282,
        IME_CONTROL = 0x283,
        IME_COMPOSITIONFULL = 0x284,
        IME_SELECT = 0x285,
        IME_CHAR = 0x286,
        IME_KEYDOWN = 0x290,
        IME_KEYUP = 0x291,

        MOUSEHOVER = 0x2A1,
        NCMOUSELEAVE = 0x2A2,
        MOUSELEAVE = 0x2A3,

        CUT = 0x300,
        COPY = 0x301,
        PASTE = 0x302,
        CLEAR = 0x303,
        UNDO = 0x304,

        RENDERFORMAT = 0x305,
        RENDERALLFORMATS = 0x306,
        DESTROYCLIPBOARD = 0x307,
        DRAWCLIPBOARD = 0x308,
        PAINTCLIPBOARD = 0x309,
        VSCROLLCLIPBOARD = 0x30A,
        SIZECLIPBOARD = 0x30B,
        ASKCBFORMATNAME = 0x30C,
        CHANGECBCHAIN = 0x30D,
        HSCROLLCLIPBOARD = 0x30E,
        QUERYNEWPALETTE = 0x30F,
        PALETTEISCHANGING = 0x310,
        PALETTECHANGED = 0x311,

        HOTKEY = 0x312,
        PRINT = 0x317,
        PRINTCLIENT = 0x318,

        CLIPBOARDCHANGED = 0x31D,

        HANDHELDFIRST = 0x358,
        HANDHELDLAST = 0x35F,
        PENWINFIRST = 0x380,
        PENWINLAST = 0x38F,
        COALESCE_FIRST = 0x390,
        COALESCE_LAST = 0x39F,
        DDE_FIRST = 0x3E0,
        DDE_INITIATE = 0x3E0,
        DDE_TERMINATE = 0x3E1,
        DDE_ADVISE = 0x3E2,
        DDE_UNADVISE = 0x3E3,
        DDE_ACK = 0x3E4,
        DDE_DATA = 0x3E5,
        DDE_REQUEST = 0x3E6,
        DDE_POKE = 0x3E7,
        DDE_EXECUTE = 0x3E8,
        DDE_LAST = 0x3E8,

        USER = 0x400,
        APP = 0x8000
    }

    /// <summary>Enum of all Window Styles.</summary>
    [Flags]
    public enum WS : uint {
        /// <summary>The window has a thin-line border.</summary>
        BORDER = 0x800000,

        /// <summary>The window has a title bar (includes the WS_BORDER style).</summary>
        CAPTION = 0xc00000,

        /// <summary>The window is a child window. A window with this style cannot have a menu bar. This style cannot be used with the WS_POPUP style.</summary>
        CHILD = 0x40000000,

        /// <summary>Excludes the area occupied by child windows when drawing occurs within the parent window. This style is used when creating the parent window.</summary>
        CLIPCHILDREN = 0x2000000,

        /// <summary>
        /// Clips child windows relative to each other; that is, when a particular child window receives a WM_PAINT message, the WS_CLIPSIBLINGS style clips all other overlapping child windows out of the region of the child window to be updated.
        /// If WS_CLIPSIBLINGS is not specified and child windows overlap, it is possible, when drawing within the client area of a child window, to draw within the client area of a neighboring child window.
        /// </summary>
        CLIPSIBLINGS = 0x4000000,

        /// <summary>The window is initially disabled. A disabled window cannot receive input from the user. To change this after a window has been created, use the EnableWindow function.</summary>
        DISABLED = 0x8000000,

        /// <summary>The window has a border of a style typically used with dialog boxes. A window with this style cannot have a title bar.</summary>
        DLGFRAME = 0x400000,

        /// <summary>
        /// The window is the first control of a group of controls. The group consists of this first control and all controls defined after it, up to the next control with the WS_GROUP style.
        /// The first control in each group usually has the WS_TABSTOP style so that the user can move from group to group. The user can subsequently change the keyboard focus from one control in the group to the next control in the group by using the direction keys.
        /// You can turn this style on and off to change dialog box navigation. To change this style after a window has been created, use the SetWindowLong function.
        /// </summary>
        GROUP = 0x20000,

        /// <summary>The window has a horizontal scroll bar.</summary>
        HSCROLL = 0x100000,

        /// <summary>The window is initially maximized.</summary>
        MAXIMIZE = 0x1000000,

        /// <summary>The window has a maximize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified.</summary>
        MAXIMIZEBOX = 0x10000,

        /// <summary>The window is initially minimized.</summary>
        MINIMIZE = 0x20000000,

        /// <summary>The window has a minimize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified.</summary>
        MINIMIZEBOX = 0x20000,

        /// <summary>The window is an overlapped window. An overlapped window has a title bar and a border.</summary>
        OVERLAPPED = 0x0,

        // /// <summary>The window is an overlapped window.</summary>
        //OVERLAPPEDWINDOW = OVERLAPPED | CAPTION | SYSMENU | SIZEFRAME | MINIMIZEBOX | MAXIMIZEBOX,

        /// <summary>The window is a pop-up window. This style cannot be used with the WS_CHILD style.</summary>
        POPUP = 0x80000000u,

        // /// <summary>The window is a pop-up window. The WS_CAPTION and WS_POPUPWINDOW styles must be combined to make the window menu visible.</summary>
        //POPUPWINDOW = POPUP | BORDER | SYSMENU,

        /// <summary>The window has a sizing border.</summary>
        SIZEFRAME = 0x40000,

        /// <summary>The window has a window menu on its title bar. The WS_CAPTION style must also be specified.</summary>
        SYSMENU = 0x80000,

        /// <summary>
        /// The window is a control that can receive the keyboard focus when the user presses the TAB key.
        /// Pressing the TAB key changes the keyboard focus to the next control with the WS_TABSTOP style.  
        /// You can turn this style on and off to change dialog box navigation. To change this style after a window has been created, use the SetWindowLong function.
        /// For user-created windows and modeless dialogs to work with tab stops, alter the message loop to call the IsDialogMessage function.
        /// </summary>
        TABSTOP = 0x10000,

        /// <summary>The window is initially visible. This style can be turned on and off by using the ShowWindow or SetWindowPos function.</summary>
        VISIBLE = 0x10000000,

        /// <summary>The window has a vertical scroll bar.</summary>
        VSCROLL = 0x200000
    }

    /// <summary>Enum of all Window Ex Styles.</summary>
    [Flags]
    public enum WS_EX : uint {
        /// <summary>Specifies a window that accepts drag-drop files.</summary>
        ACCEPTFILES = 0x00000010,

        /// <summary>Forces a top-level window onto the taskbar when the window is visible.</summary>
        APPWINDOW = 0x00040000,

        /// <summary>Specifies a window that has a border with a sunken edge.</summary>
        CLIENTEDGE = 0x00000200,

        /// <summary>
        /// Specifies a window that paints all descendants in bottom-to-top painting order using double-buffering.
        /// This cannot be used if the window has a class style of either CS_OWNDC or CS_CLASSDC. This style is not supported in Windows 2000.
        /// </summary>
        /// <remarks>
        /// With WS_EX_COMPOSITED set, all descendants of a window get bottom-to-top painting order using double-buffering.
        /// Bottom-to-top painting order allows a descendent window to have translucency (alpha) and transparency (color-key) effects,
        /// but only if the descendent window also has the WS_EX_TRANSPARENT bit set.
        /// Double-buffering allows the window and its descendents to be painted without flicker.
        /// </remarks>
        COMPOSITED = 0x02000000,

        /// <summary>
        /// Specifies a window that includes a question mark in the title bar. When the user clicks the question mark,
        /// the cursor changes to a question mark with a pointer. If the user then clicks a child window, the child receives a WM_HELP message.
        /// The child window should pass the message to the parent window procedure, which should call the WinHelp function using the HELP_WM_HELP command.
        /// The Help application displays a pop-up window that typically contains help for the child window.
        /// WS_EX_CONTEXTHELP cannot be used with the WS_MAXIMIZEBOX or WS_MINIMIZEBOX styles.
        /// </summary>
        CONTEXTHELP = 0x00000400,

        /// <summary>
        /// Specifies a window which contains child windows that should take part in dialog box navigation.
        /// If this style is specified, the dialog manager recurses into children of this window when performing navigation operations
        /// such as handling the TAB key, an arrow key, or a keyboard mnemonic.
        /// </summary>
        CONTROLPARENT = 0x00010000,

        /// <summary>Specifies a window that has a double border.</summary>
        DLGMODALFRAME = 0x00000001,

        /// <summary>
        /// Specifies a window that is a layered window.
        /// This cannot be used for child windows or if the window has a class style of either CS_OWNDC or CS_CLASSDC.
        /// </summary>
        LAYERED = 0x00080000,

        /// <summary>
        /// Specifies a window with the horizontal origin on the right edge. Increasing horizontal values advance to the left.
        /// The shell language must support reading-order alignment for this to take effect.
        /// </summary>
        LAYOUTRTL = 0x00400000,

        /// <summary>Specifies a window that has generic left-aligned properties. This is the default.</summary>
        LEFT = 0x00000000,

        /// <summary>
        /// Specifies a window with the vertical scroll bar (if present) to the left of the client area.
        /// The shell language must support reading-order alignment for this to take effect.
        /// </summary>
        LEFTSCROLLBAR = 0x00004000,

        /// <summary>
        /// Specifies a window that displays text using left-to-right reading-order properties. This is the default.
        /// </summary>
        LTRREADING = 0x00000000,

        /// <summary>
        /// Specifies a multiple-document interface (MDI) child window.
        /// </summary>
        MDICHILD = 0x00000040,

        /// <summary>
        /// Specifies a top-level window created with this style does not become the foreground window when the user clicks it.
        /// The system does not bring this window to the foreground when the user minimizes or closes the foreground window.
        /// The window does not appear on the taskbar by default. To force the window to appear on the taskbar, use the WS_EX_APPWINDOW style.
        /// To activate the window, use the SetActiveWindow or SetForegroundWindow function.
        /// </summary>
        NOACTIVATE = 0x08000000,

        /// <summary>
        /// Specifies a window which does not pass its window layout to its child windows.
        /// </summary>
        NOINHERITLAYOUT = 0x00100000,

        /// <summary>
        /// Specifies that a child window created with this style does not send the WM_PARENTNOTIFY message to its parent window when it is created or destroyed.
        /// </summary>
        NOPARENTNOTIFY = 0x00000004,

        /// <summary>
        /// The window does not render to a redirection surface.
        /// This is for windows that do not have visible content or that use mechanisms other than surfaces to provide their visual.
        /// </summary>
        NOREDIRECTIONBITMAP = 0x00200000,

        // /// <summary>Specifies an overlapped window.</summary>
        //OVERLAPPEDWINDOW = WINDOWEDGE | CLIENTEDGE,

        // /// <summary>Specifies a palette window, which is a modeless dialog box that presents an array of commands.</summary>
        //PALETTEWINDOW = WINDOWEDGE | TOOLWINDOW | TOPMOST,

        /// <summary>
        /// Specifies a window that has generic "right-aligned" properties. This depends on the window class.
        /// The shell language must support reading-order alignment for this to take effect.
        /// Using the WS_EX_RIGHT style has the same effect as using the SS_RIGHT (static), ES_RIGHT (edit), and BS_RIGHT/BS_RIGHTBUTTON (button) control styles.
        /// </summary>
        RIGHT = 0x00001000,

        /// <summary>Specifies a window with the vertical scroll bar (if present) to the right of the client area. This is the default.</summary>
        RIGHTSCROLLBAR = 0x00000000,

        /// <summary>
        /// Specifies a window that displays text using right-to-left reading-order properties.
        /// The shell language must support reading-order alignment for this to take effect.
        /// </summary>
        RTLREADING = 0x00002000,

        /// <summary>Specifies a window with a three-dimensional border style intended to be used for items that do not accept user input.</summary>
        STATICEDGE = 0x00020000,

        /// <summary>
        /// Specifies a window that is intended to be used as a floating toolbar.
        /// A tool window has a title bar that is shorter than a normal title bar, and the window title is drawn using a smaller font.
        /// A tool window does not appear in the taskbar or in the dialog that appears when the user presses ALT+TAB.
        /// If a tool window has a system menu, its icon is not displayed on the title bar.
        /// However, you can display the system menu by right-clicking or by typing ALT+SPACE.
        /// </summary>
        TOOLWINDOW = 0x00000080,

        /// <summary>
        /// Specifies a window that should be placed above all non-topmost windows and should stay above them, even when the window is deactivated.
        /// To add or remove this style, use the SetWindowPos function.
        /// </summary>
        TOPMOST = 0x00000008,

        /// <summary>
        /// Specifies a window that should not be painted until siblings beneath the window (that were created by the same thread) have been painted.
        /// The window appears transparent because the bits of underlying sibling windows have already been painted.
        /// To achieve transparency without these restrictions, use the SetWindowRgn function.
        /// </summary>
        TRANSPARENT = 0x00000020,

        /// <summary>Specifies a window that has a border with a raised edge.</summary>
        WINDOWEDGE = 0x00000100
    }

    /// <summary>Enum of all Hit Test values.</summary>
    public enum HT {
        /// <summary>In the border of a window that does not have a sizing border.</summary>
        BORDER = 18,

        /// <summary>In the lower-horizontal border of a resizable window (the user can click the mouse to resize the window vertically).</summary>
        BOTTOM = 15,

        /// <summary>In the lower-left corner of a border of a resizable window (the user can click the mouse to resize the window diagonally).</summary>
        BOTTOMLEFT = 16,

        /// <summary>In the lower-right corner of a border of a resizable window (the user can click the mouse to resize the window diagonally).</summary>
        BOTTOMRIGHT = 17,

        /// <summary>In a title bar.</summary>
        CAPTION = 2,

        /// <summary>In a client area.</summary>
        CLIENT = 1,

        /// <summary>In a Close button.</summary>
        CLOSE = 20,

        /// <summary>On the screen background or on a dividing line between windows (same as HTNOWHERE, except that the DefWindowProc function produces a system beep to indicate an error).</summary>
        ERROR = -2,

        /// <summary>In a size box (same as HTSIZE).</summary>
        GROWBOX = 4,

        /// <summary>In a Help button.</summary>
        HELP = 21,

        /// <summary>In a horizontal scroll bar.</summary>
        HSCROLL = 6,

        /// <summary>In the left border of a resizable window (the user can click the mouse to resize the window horizontally).</summary>
        LEFT = 10,

        /// <summary>In a menu.</summary>
        MENU = 5,

        /// <summary>In a Maximize button.</summary>
        MAXBUTTON = 9,

        /// <summary>In a Minimize button.</summary>
        MINBUTTON = 8,

        /// <summary>On the screen background or on a dividing line between windows.</summary>
        NOWHERE = 0,

        // /// <summary>Not implemented.</summary>
        /* HTOBJECT = 19, */

        /// <summary>In a Minimize button.</summary>
        REDUCE = MINBUTTON,

        /// <summary>In the right border of a resizable window (the user can click the mouse to resize the window horizontally).</summary>
        RIGHT = 11,

        /// <summary>In a size box (same as HTGROWBOX).</summary>
        SIZE = GROWBOX,

        /// <summary>In a window menu or in a Close button in a child window.</summary>
        SYSMENU = 3,

        /// <summary>In the upper-horizontal border of a window.</summary>
        TOP = 12,

        /// <summary>In the upper-left corner of a window border.</summary>
        TOPLEFT = 13,

        /// <summary>In the upper-right corner of a window border.</summary>
        TOPRIGHT = 14,

        /// <summary>In a window currently covered by another window in the same thread (the message will be sent to underlying windows in the same thread until one of them returns a code that is not HTTRANSPARENT).</summary>
        TRANSPARENT = -1,

        /// <summary>In the vertical scroll bar.</summary>
        VSCROLL = 7,

        /// <summary>In a Maximize button.</summary>
        ZOOM = MAXBUTTON,
    }

    /// <summary>Determines what to return if an exact match was not found.</summary>
    public enum MonitorDefault : uint {
        /// <summary>Returns null if point is not on any monitor.</summary>
        Null,
        /// <summary>Returns primary monitor if point is not on any monitor.</summary>
        Primary,
        /// <summary>Returns nearest monitor if point is not on any monitor.</summary>
        Nearest
    }

    /// <summary>Enum of virtual key codes.</summary>
    public enum VKey : ushort {
        ///<summary>
        ///Left mouse button
        ///</summary>
        LBUTTON = 0x01,
        ///<summary>
        ///Right mouse button
        ///</summary>
        RBUTTON = 0x02,
        ///<summary>
        ///Control-break processing
        ///</summary>
        CANCEL = 0x03,
        ///<summary>
        ///Middle mouse button (three-button mouse)
        ///</summary>
        MBUTTON = 0x04,
        ///<summary>
        ///Windows 2000/XP: X1 mouse button
        ///</summary>
        XBUTTON1 = 0x05,
        ///<summary>
        ///Windows 2000/XP: X2 mouse button
        ///</summary>
        XBUTTON2 = 0x06,
        /// <summary>This key is undefined</summary>
        UNDEFINED = 0x07,
        ///<summary>
        ///BACKSPACE key
        ///</summary>
        BACK = 0x08,
        ///<summary>
        ///TAB key
        ///</summary>
        TAB = 0x09,
        ///<summary>
        ///CLEAR key
        ///</summary>
        CLEAR = 0x0C,
        ///<summary>
        ///ENTER key
        ///</summary>
        RETURN = 0x0D,
        ///<summary>
        ///SHIFT key
        ///</summary>
        SHIFT = 0x10,
        ///<summary>
        ///CTRL key
        ///</summary>
        CONTROL = 0x11,
        ///<summary>
        ///ALT key
        ///</summary>
        MENU = 0x12,
        ///<summary>
        ///PAUSE key
        ///</summary>
        PAUSE = 0x13,
        ///<summary>
        ///CAPS LOCK key
        ///</summary>
        CAPITAL = 0x14,
        ///<summary>
        ///Input Method Editor (IME) Kana mode
        ///</summary>
        KANA = 0x15,
        ///<summary>
        ///IME Junja mode
        ///</summary>
        JUNJA = 0x17,
        ///<summary>
        ///IME final mode
        ///</summary>
        FINAL = 0x18,
        ///<summary>
        ///IME Kanji mode
        ///</summary>
        KANJI = 0x19,
        ///<summary>
        ///ESC key
        ///</summary>
        ESCAPE = 0x1B,
        ///<summary>
        ///IME convert
        ///</summary>
        CONVERT = 0x1C,
        ///<summary>
        ///IME nonconvert
        ///</summary>
        NONCONVERT = 0x1D,
        ///<summary>
        ///IME accept
        ///</summary>
        ACCEPT = 0x1E,
        ///<summary>
        ///IME mode change request
        ///</summary>
        MODECHANGE = 0x1F,
        ///<summary>
        ///SPACEBAR
        ///</summary>
        SPACE = 0x20,
        ///<summary>
        ///PAGE UP key
        ///</summary>
        PRIOR = 0x21,
        ///<summary>
        ///PAGE DOWN key
        ///</summary>
        NEXT = 0x22,
        ///<summary>
        ///END key
        ///</summary>
        END = 0x23,
        ///<summary>
        ///HOME key
        ///</summary>
        HOME = 0x24,
        ///<summary>
        ///LEFT ARROW key
        ///</summary>
        LEFT = 0x25,
        ///<summary>
        ///UP ARROW key
        ///</summary>
        UP = 0x26,
        ///<summary>
        ///RIGHT ARROW key
        ///</summary>
        RIGHT = 0x27,
        ///<summary>
        ///DOWN ARROW key
        ///</summary>
        DOWN = 0x28,
        ///<summary>
        ///SELECT key
        ///</summary>
        SELECT = 0x29,
        ///<summary>
        ///PRINT key
        ///</summary>
        PRINT = 0x2A,
        ///<summary>
        ///EXECUTE key
        ///</summary>
        EXECUTE = 0x2B,
        ///<summary>
        ///PRINT SCREEN key
        ///</summary>
        SNAPSHOT = 0x2C,
        ///<summary>
        ///INS key
        ///</summary>
        INSERT = 0x2D,
        ///<summary>
        ///DEL key
        ///</summary>
        DELETE = 0x2E,
        ///<summary>
        ///HELP key
        ///</summary>
        HELP = 0x2F,
        ///<summary>
        ///0 key
        ///</summary>
        D0 = 0x30,
        ///<summary>
        ///1 key
        ///</summary>
        D1 = 0x31,
        ///<summary>
        ///2 key
        ///</summary>
        D2 = 0x32,
        ///<summary>
        ///3 key
        ///</summary>
        D3 = 0x33,
        ///<summary>
        ///4 key
        ///</summary>
        D4 = 0x34,
        ///<summary>
        ///5 key
        ///</summary>
        D5 = 0x35,
        ///<summary>
        ///6 key
        ///</summary>
        D6 = 0x36,
        ///<summary>
        ///7 key
        ///</summary>
        D7 = 0x37,
        ///<summary>
        ///8 key
        ///</summary>
        D8 = 0x38,
        ///<summary>
        ///9 key
        ///</summary>
        D9 = 0x39,
        ///<summary>
        ///A key
        ///</summary>
        A = 0x41,
        ///<summary>
        ///B key
        ///</summary>
        B = 0x42,
        ///<summary>
        ///C key
        ///</summary>
        C = 0x43,
        ///<summary>
        ///D key
        ///</summary>
        D = 0x44,
        ///<summary>
        ///E key
        ///</summary>
        E = 0x45,
        ///<summary>
        ///F key
        ///</summary>
        F = 0x46,
        ///<summary>
        ///G key
        ///</summary>
        G = 0x47,
        ///<summary>
        ///H key
        ///</summary>
        H = 0x48,
        ///<summary>
        ///I key
        ///</summary>
        I = 0x49,
        ///<summary>
        ///J key
        ///</summary>
        J = 0x4A,
        ///<summary>
        ///K key
        ///</summary>
        K = 0x4B,
        ///<summary>
        ///L key
        ///</summary>
        L = 0x4C,
        ///<summary>
        ///M key
        ///</summary>
        M = 0x4D,
        ///<summary>
        ///N key
        ///</summary>
        N = 0x4E,
        ///<summary>
        ///O key
        ///</summary>
        O = 0x4F,
        ///<summary>
        ///P key
        ///</summary>
        P = 0x50,
        ///<summary>
        ///Q key
        ///</summary>
        Q = 0x51,
        ///<summary>
        ///R key
        ///</summary>
        R = 0x52,
        ///<summary>
        ///S key
        ///</summary>
        S = 0x53,
        ///<summary>
        ///T key
        ///</summary>
        T = 0x54,
        ///<summary>
        ///U key
        ///</summary>
        U = 0x55,
        ///<summary>
        ///V key
        ///</summary>
        V = 0x56,
        ///<summary>
        ///W key
        ///</summary>
        W = 0x57,
        ///<summary>
        ///X key
        ///</summary>
        X = 0x58,
        ///<summary>
        ///Y key
        ///</summary>
        Y = 0x59,
        ///<summary>
        ///Z key
        ///</summary>
        Z = 0x5A,
        ///<summary>
        ///Left Windows key (Microsoft Natural keyboard)
        ///</summary>
        LWIN = 0x5B,
        ///<summary>
        ///Right Windows key (Natural keyboard)
        ///</summary>
        RWIN = 0x5C,
        ///<summary>
        ///Applications key (Natural keyboard)
        ///</summary>
        APPS = 0x5D,
        ///<summary>
        ///Computer Sleep key
        ///</summary>
        SLEEP = 0x5F,
        ///<summary>
        ///Numeric keypad 0 key
        ///</summary>
        NUMPAD0 = 0x60,
        ///<summary>
        ///Numeric keypad 1 key
        ///</summary>
        NUMPAD1 = 0x61,
        ///<summary>
        ///Numeric keypad 2 key
        ///</summary>
        NUMPAD2 = 0x62,
        ///<summary>
        ///Numeric keypad 3 key
        ///</summary>
        NUMPAD3 = 0x63,
        ///<summary>
        ///Numeric keypad 4 key
        ///</summary>
        NUMPAD4 = 0x64,
        ///<summary>
        ///Numeric keypad 5 key
        ///</summary>
        NUMPAD5 = 0x65,
        ///<summary>
        ///Numeric keypad 6 key
        ///</summary>
        NUMPAD6 = 0x66,
        ///<summary>
        ///Numeric keypad 7 key
        ///</summary>
        NUMPAD7 = 0x67,
        ///<summary>
        ///Numeric keypad 8 key
        ///</summary>
        NUMPAD8 = 0x68,
        ///<summary>
        ///Numeric keypad 9 key
        ///</summary>
        NUMPAD9 = 0x69,
        ///<summary>
        ///Multiply key
        ///</summary>
        MULTIPLY = 0x6A,
        ///<summary>
        ///Add key
        ///</summary>
        ADD = 0x6B,
        ///<summary>
        ///Separator key
        ///</summary>
        SEPARATOR = 0x6C,
        ///<summary>
        ///Subtract key
        ///</summary>
        SUBTRACT = 0x6D,
        ///<summary>
        ///Decimal key
        ///</summary>
        DECIMAL = 0x6E,
        ///<summary>
        ///Divide key
        ///</summary>
        DIVIDE = 0x6F,
        ///<summary>
        ///F1 key
        ///</summary>
        F1 = 0x70,
        ///<summary>
        ///F2 key
        ///</summary>
        F2 = 0x71,
        ///<summary>
        ///F3 key
        ///</summary>
        F3 = 0x72,
        ///<summary>
        ///F4 key
        ///</summary>
        F4 = 0x73,
        ///<summary>
        ///F5 key
        ///</summary>
        F5 = 0x74,
        ///<summary>
        ///F6 key
        ///</summary>
        F6 = 0x75,
        ///<summary>
        ///F7 key
        ///</summary>
        F7 = 0x76,
        ///<summary>
        ///F8 key
        ///</summary>
        F8 = 0x77,
        ///<summary>
        ///F9 key
        ///</summary>
        F9 = 0x78,
        ///<summary>
        ///F10 key
        ///</summary>
        F10 = 0x79,
        ///<summary>
        ///F11 key
        ///</summary>
        F11 = 0x7A,
        ///<summary>
        ///F12 key
        ///</summary>
        F12 = 0x7B,
        ///<summary>
        ///F13 key
        ///</summary>
        F13 = 0x7C,
        ///<summary>
        ///F14 key
        ///</summary>
        F14 = 0x7D,
        ///<summary>
        ///F15 key
        ///</summary>
        F15 = 0x7E,
        ///<summary>
        ///F16 key
        ///</summary>
        F16 = 0x7F,
        ///<summary>
        ///F17 key  
        ///</summary>
        F17 = 0x80,
        ///<summary>
        ///F18 key  
        ///</summary>
        F18 = 0x81,
        ///<summary>
        ///F19 key  
        ///</summary>
        F19 = 0x82,
        ///<summary>
        ///F20 key  
        ///</summary>
        F20 = 0x83,
        ///<summary>
        ///F21 key  
        ///</summary>
        F21 = 0x84,
        ///<summary>
        ///F22 key, (PPC only) Key used to lock device.
        ///</summary>
        F22 = 0x85,
        ///<summary>
        ///F23 key  
        ///</summary>
        F23 = 0x86,
        ///<summary>
        ///F24 key  
        ///</summary>
        F24 = 0x87,
        ///<summary>
        ///NUM LOCK key
        ///</summary>
        NUMLOCK = 0x90,
        ///<summary>
        ///SCROLL LOCK key
        ///</summary>
        SCROLL = 0x91,
        ///<summary>
        ///Left SHIFT key
        ///</summary>
        LSHIFT = 0xA0,
        ///<summary>
        ///Right SHIFT key
        ///</summary>
        RSHIFT = 0xA1,
        ///<summary>
        ///Left CONTROL key
        ///</summary>
        LCONTROL = 0xA2,
        ///<summary>
        ///Right CONTROL key
        ///</summary>
        RCONTROL = 0xA3,
        ///<summary>
        ///Left MENU key
        ///</summary>
        LMENU = 0xA4,
        ///<summary>
        ///Right MENU key
        ///</summary>
        RMENU = 0xA5,
        ///<summary>
        ///Windows 2000/XP: Browser Back key
        ///</summary>
        BROWSER_BACK = 0xA6,
        ///<summary>
        ///Windows 2000/XP: Browser Forward key
        ///</summary>
        BROWSER_FORWARD = 0xA7,
        ///<summary>
        ///Windows 2000/XP: Browser Refresh key
        ///</summary>
        BROWSER_REFRESH = 0xA8,
        ///<summary>
        ///Windows 2000/XP: Browser Stop key
        ///</summary>
        BROWSER_STOP = 0xA9,
        ///<summary>
        ///Windows 2000/XP: Browser Search key
        ///</summary>
        BROWSER_SEARCH = 0xAA,
        ///<summary>
        ///Windows 2000/XP: Browser Favorites key
        ///</summary>
        BROWSER_FAVORITES = 0xAB,
        ///<summary>
        ///Windows 2000/XP: Browser Start and Home key
        ///</summary>
        BROWSER_HOME = 0xAC,
        ///<summary>
        ///Windows 2000/XP: Volume Mute key
        ///</summary>
        VOLUME_MUTE = 0xAD,
        ///<summary>
        ///Windows 2000/XP: Volume Down key
        ///</summary>
        VOLUME_DOWN = 0xAE,
        ///<summary>
        ///Windows 2000/XP: Volume Up key
        ///</summary>
        VOLUME_UP = 0xAF,
        ///<summary>
        ///Windows 2000/XP: Next Track key
        ///</summary>
        MEDIA_NEXT_TRACK = 0xB0,
        ///<summary>
        ///Windows 2000/XP: Previous Track key
        ///</summary>
        MEDIA_PREV_TRACK = 0xB1,
        ///<summary>
        ///Windows 2000/XP: Stop Media key
        ///</summary>
        MEDIA_STOP = 0xB2,
        ///<summary>
        ///Windows 2000/XP: Play/Pause Media key
        ///</summary>
        MEDIA_PLAY_PAUSE = 0xB3,
        ///<summary>
        ///Windows 2000/XP: Start Mail key
        ///</summary>
        LAUNCH_MAIL = 0xB4,
        ///<summary>
        ///Windows 2000/XP: Select Media key
        ///</summary>
        LAUNCH_MEDIA_SELECT = 0xB5,
        ///<summary>
        ///Windows 2000/XP: Start Application 1 key
        ///</summary>
        LAUNCH_APP1 = 0xB6,
        ///<summary>
        ///Windows 2000/XP: Start Application 2 key
        ///</summary>
        LAUNCH_APP2 = 0xB7,
        ///<summary>
        ///Used for miscellaneous characters; it can vary by keyboard.
        ///</summary>
        OEM_1 = 0xBA,
        ///<summary>
        ///Windows 2000/XP: For any country/region, the '+' key
        ///</summary>
        OEM_PLUS = 0xBB,
        ///<summary>
        ///Windows 2000/XP: For any country/region, the ',' key
        ///</summary>
        OEM_COMMA = 0xBC,
        ///<summary>
        ///Windows 2000/XP: For any country/region, the '-' key
        ///</summary>
        OEM_MINUS = 0xBD,
        ///<summary>
        ///Windows 2000/XP: For any country/region, the '.' key
        ///</summary>
        OEM_PERIOD = 0xBE,
        ///<summary>
        ///Used for miscellaneous characters; it can vary by keyboard.
        ///</summary>
        OEM_2 = 0xBF,
        ///<summary>
        ///Used for miscellaneous characters; it can vary by keyboard.
        ///</summary>
        OEM_3 = 0xC0,
        ///<summary>
        ///Used for miscellaneous characters; it can vary by keyboard.
        ///</summary>
        OEM_4 = 0xDB,
        ///<summary>
        ///Used for miscellaneous characters; it can vary by keyboard.
        ///</summary>
        OEM_5 = 0xDC,
        ///<summary>
        ///Used for miscellaneous characters; it can vary by keyboard.
        ///</summary>
        OEM_6 = 0xDD,
        ///<summary>
        ///Used for miscellaneous characters; it can vary by keyboard.
        ///</summary>
        OEM_7 = 0xDE,
        ///<summary>
        ///Used for miscellaneous characters; it can vary by keyboard.
        ///</summary>
        OEM_8 = 0xDF,
        ///<summary>
        ///Windows 2000/XP: Either the angle bracket key or the backslash key on the RT 102-key keyboard
        ///</summary>
        OEM_102 = 0xE2,
        ///<summary>
        ///Windows 95/98/Me, Windows NT 4.0, Windows 2000/XP: IME PROCESS key
        ///</summary>
        PROCESSKEY = 0xE5,
        /// <summary>
        /// Windows 2000/XP: Used to pass Unicode characters as if they were keystrokes.
        /// The VK_PACKET key is the low word of a 32-bit Virtual Key value used for non-keyboard input methods. For more information,
        /// see Remark in KEYBDINPUT, SendInput, WM_KEYDOWN, and WM_KEYUP
        /// </summary>
        PACKET = 0xE7,
        /// <summary>Attn key</summary>
        ATTN = 0xF6,
        /// <summary>CrSel key</summary>
        CRSEL = 0xF7,
        /// <summary>ExSel key</summary>
        EXSEL = 0xF8,
        /// <summary>Erase EOF key</summary>
        EREOF = 0xF9,
        /// <summary>Play key</summary>
        PLAY = 0xFA,
        /// <summary>Zoom key</summary>
        ZOOM = 0xFB,
        /// <summary>Reserved</summary>
        NONAME = 0xFC,
        /// <summary>PA1 key</summary>
        PA1 = 0xFD,
        /// <summary>Clear key</summary>
        OEM_CLEAR = 0xFE
    }

    /// <summary>Enum of key scan codes.</summary>
    public enum ScanCode : ushort {
        LBUTTON = 0,
        RBUTTON = 0,
        CANCEL = 70,
        MBUTTON = 0,
        XBUTTON1 = 0,
        XBUTTON2 = 0,
        BACK = 14,
        TAB = 15,
        CLEAR = 76,
        RETURN = 28,
        SHIFT = 42,
        CONTROL = 29,
        MENU = 56,
        PAUSE = 0,
        CAPITAL = 58,
        KANA = 0,
        HANGUL = 0,
        JUNJA = 0,
        FINAL = 0,
        HANJA = 0,
        KANJI = 0,
        ESCAPE = 1,
        CONVERT = 0,
        NONCONVERT = 0,
        ACCEPT = 0,
        MODECHANGE = 0,
        SPACE = 57,
        PRIOR = 73,
        NEXT = 81,
        END = 79,
        HOME = 71,
        LEFT = 75,
        UP = 72,
        RIGHT = 77,
        DOWN = 80,
        SELECT = 0,
        PRINT = 0,
        EXECUTE = 0,
        SNAPSHOT = 84,
        INSERT = 82,
        DELETE = 83,
        HELP = 99,
        D0 = 11,
        D1 = 2,
        D2 = 3,
        D3 = 4,
        D4 = 5,
        D5 = 6,
        D6 = 7,
        D7 = 8,
        D8 = 9,
        D9 = 10,
        A = 30,
        B = 48,
        C = 46,
        D = 32,
        E = 18,
        F = 33,
        G = 34,
        H = 35,
        I = 23,
        J = 36,
        K = 37,
        L = 38,
        M = 50,
        N = 49,
        O = 24,
        P = 25,
        Q = 16,
        R = 19,
        S = 31,
        T = 20,
        U = 22,
        V = 47,
        W = 17,
        X = 45,
        Y = 21,
        Z = 44,
        LWIN = 91,
        RWIN = 92,
        APPS = 93,
        SLEEP = 95,
        NUMPAD0 = 82,
        NUMPAD1 = 79,
        NUMPAD2 = 80,
        NUMPAD3 = 81,
        NUMPAD4 = 75,
        NUMPAD5 = 76,
        NUMPAD6 = 77,
        NUMPAD7 = 71,
        NUMPAD8 = 72,
        NUMPAD9 = 73,
        MULTIPLY = 55,
        ADD = 78,
        SEPARATOR = 0,
        SUBTRACT = 74,
        DECIMAL = 83,
        DIVIDE = 53,
        F1 = 59,
        F2 = 60,
        F3 = 61,
        F4 = 62,
        F5 = 63,
        F6 = 64,
        F7 = 65,
        F8 = 66,
        F9 = 67,
        F10 = 68,
        F11 = 87,
        F12 = 88,
        F13 = 100,
        F14 = 101,
        F15 = 102,
        F16 = 103,
        F17 = 104,
        F18 = 105,
        F19 = 106,
        F20 = 107,
        F21 = 108,
        F22 = 109,
        F23 = 110,
        F24 = 118,
        NUMLOCK = 69,
        SCROLL = 70,
        LSHIFT = 42,
        RSHIFT = 54,
        LCONTROL = 29,
        RCONTROL = 29,
        LMENU = 56,
        RMENU = 56,
        BROWSER_BACK = 106,
        BROWSER_FORWARD = 105,
        BROWSER_REFRESH = 103,
        BROWSER_STOP = 104,
        BROWSER_SEARCH = 101,
        BROWSER_FAVORITES = 102,
        BROWSER_HOME = 50,
        VOLUME_MUTE = 32,
        VOLUME_DOWN = 46,
        VOLUME_UP = 48,
        MEDIA_NEXT_TRACK = 25,
        MEDIA_PREV_TRACK = 16,
        MEDIA_STOP = 36,
        MEDIA_PLAY_PAUSE = 34,
        LAUNCH_MAIL = 108,
        LAUNCH_MEDIA_SELECT = 109,
        LAUNCH_APP1 = 107,
        LAUNCH_APP2 = 33,
        OEM_1 = 39,
        OEM_PLUS = 13,
        OEM_COMMA = 51,
        OEM_MINUS = 12,
        OEM_PERIOD = 52,
        OEM_2 = 53,
        OEM_3 = 41,
        OEM_4 = 26,
        OEM_5 = 43,
        OEM_6 = 27,
        OEM_7 = 40,
        OEM_8 = 0,
        OEM_102 = 86,
        PROCESSKEY = 0,
        PACKET = 0,
        ATTN = 0,
        CRSEL = 0,
        EXSEL = 0,
        EREOF = 93,
        PLAY = 0,
        ZOOM = 98,
        NONAME = 0,
        PA1 = 0,
        OEM_CLEAR = 0,
    }

    public enum CursorType {
        Undefined = 0,
        Arrow = 32512,
        IBeam = 32513,
        Wait = 32514,
        Cross = 32515,
        UpArrow = 32516,
        SizeLeftSlant = 32642,
        SizeRightSlant = 32643,
        SizeHorizontal = 32644,
        SizeVertical = 32645,
        SizeAll = 32646,
        No = 32648,
        Hand = 32649,
        ArrowWaiting = 32650,
        Help = 32651
    }
    #endregion

    #region structs

    #endregion
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member