using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace WinUtilities {

    /* Functionality missing for full virtual desktop control:
     * 
     * Create desktop
     * Close desktop
     * Switch to desktop
     * 
     * Pin/unpin/check window
     * Pin/unpin/check process
     * 
     * Enumerate desktops
     * Get current desktop
     */

    internal static class SimpleDesktop {
        private static IVirtualDesktopManager manager = (IVirtualDesktopManager) new VirtualDesktopManager();

        internal static bool IsOnCurrent(Window window) {
            manager.IsWindowOnCurrentVirtualDesktop(window.Hwnd.Raw, out bool result);
            return result;
        }

        internal static Guid GetDesktopID(Window window) {
            manager.GetWindowDesktopId(window.Hwnd.Raw, out Guid id);
            return id;
        }

        internal static void MoveWindow(Window window, Guid desktop) {
            manager.MoveWindowToDesktop(window.Hwnd.Raw, desktop);
        }
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("a5cd92ff-29be-454c-8d04-d82879fb3f1b")]
    [System.Security.SuppressUnmanagedCodeSecurity]
    internal interface IVirtualDesktopManager {
        [PreserveSig]
        int IsWindowOnCurrentVirtualDesktop([In] IntPtr TopLevelWindow, [Out] out bool OnCurrentDesktop);

        [PreserveSig]
        int GetWindowDesktopId([In] IntPtr TopLevelWindow, [Out] out Guid CurrentDesktop);

        [PreserveSig]
        int MoveWindowToDesktop([In] IntPtr TopLevelWindow, [MarshalAs(UnmanagedType.LPStruct)][In] Guid CurrentDesktop);
    }

    [ComImport, Guid("aa509086-5ca9-4c25-8f95-589d3c07b48a")]
    internal class VirtualDesktopManager { }
}
