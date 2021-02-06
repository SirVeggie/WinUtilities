# WinUtilities

WinUtilities is a small wrapper project that wraps Windows API functions to make Windows automation easy.
The project is made with C# .NET Standard 2.0.

The project is only tested to work on Windows 10, but it should work on earlier versions as well.

## Core idea

The wrapper contains some user32, gdi32 etc... API calls as well as structs and enums that were necessary while implementing the features of the project.
This means it does not contain all of the functions of those APIs.

The wrapper is mainly centered around the Window class, as in Windows automation there is very often a window that is the target of some sort of action.

## Feature list

The list is not exhaustive.

- Window.cs
  - Find and match windows with title, class, exe, hwnd, pid or other information like window under the mouse pointer
  - Retrieve area information like its normal area, client area and visible area
  - Check for properties: visibility, active, enabled, exists, always on top, click through, child window, maximize state, fullscreen, borderless, style, opacity, etc.
  - Do actions: Move, activate, enable, close, kill (process), minimize, maximize, set visibility, post messages, set style, set Z level, set always on top, click through, etc.
  - Set window into a borderless state (disable windows borders and crop the visible area)
  - Capture images of the window
  - Send input to the window

- Coorinates.cs
  - Coordinate objects that have a bunch of useful methods

- DeviceHook.cs
  - Hook into low level mouse and keyboard hooks to receive mouse and keyboard events

- EnhancedKey.cs
  - An enhanced Virtual Key enum that contains a variety of helper methods that improve efficiency and ease of use

- Input.cs
  - Send input to current window using SendInput, keybd_event or mouse_event APIs
  - Send input directly to background windows
  - Get the current logical state of keys of the OS
  - Parsed string input as a flexible input method

- Monitor.cs
  - Get information of current monitors
  - Get the entire screen, a single monitor or specified area as an Image
  - Set primary monitor and monitor orientation (WIP)

- Mouse.cs
  - Get mouse related information like position
  - Move mouse and send clicks and scrolls

- SystemUtils.cs
  - Go to lock screen
  - Toggle internet on/off (Might or might not work depending on your network device)
