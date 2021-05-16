using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;

namespace WinUtilities {
    /// <summary>Class for accessing a file explorer window's content</summary>
    public static class Explorer {

        private static void RunStaJoin(this Thread thread) {
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        #region paths
        /// <summary>Get the path of the currently opened folder</summary>
        public static string GetPath(Window win) {
            string res = null;
            new Thread(() => res = _GetPath(win)).RunStaJoin();
            return res;
        }

        /// <summary>Get all file and folder paths in the currently opened folder</summary>
        public static List<string> GetAll(Window win) {
            List<string> res = null;
            new Thread(() => res = Get(win, false)).RunStaJoin();
            return res ?? new List<string>();
        }

        /// <summary>Get all selected file and folder paths in the currently opened folder</summary>
        public static List<string> GetSelected(Window win) {
            List<string> res = null;
            new Thread(() => res = Get(win, true)).RunStaJoin();
            return res ?? new List<string>();
        }

        private static dynamic GetWindow(Window win) {
            if (win.Match(WinGroup.Desktop))
                return null;
            if (win.Match(WinGroup.Folder)) {
                dynamic shell = new Shell32();
                foreach (dynamic window in shell.Application.Windows()) {
                    if (window.hwnd == (long) win.Hwnd) {
                        return window;
                    }
                }
            }

            return null;
        }

        private static List<string> Get(Window win, bool selection) {
            var window = GetWindow(win);
            dynamic collection;
            var res = new List<string>();

            if (window == null)
                return null;
            if (selection)
                collection = window.document.SelectedItems;
            else
                collection = window.document.Folder.Items;
            foreach (var item in collection)
                res.Add(item.path);
            return res;
        }

        private static string _GetPath(Window win) {
            var window = GetWindow(win);
            if (window == null)
                return null;
            string path = window.LocationURL;
            path = Regex.Replace(path, "ftp://.*@", "ftp://", RegexOptions.None);
            path = path.Replace("file:///", "");
            path = path.Replace('/', '\\');

            while (Regex.Match(path, @"(?<=%)[\da-f]{1,2}", RegexOptions.IgnoreCase) is var match && match.Success) {
                path = Regex.Replace(path, $"%{match.Value}", ((char) int.Parse(match.Value, System.Globalization.NumberStyles.HexNumber)).ToString(), RegexOptions.None);
            }

            return path;
        }
        #endregion

        #region shortcuts
        /// <summary>Get the filepath the shortcut points towards</summary>
        public static string GetShortcutTarget(string shortcutPath) {
            try {
                string pathOnly = Path.GetDirectoryName(shortcutPath);
                string fileOnly = Path.GetFileName(shortcutPath);

                dynamic shell = new Shell32();
                dynamic item = shell.NameSpace(pathOnly).ParseName(fileOnly);

                if (item != null) {
                    return item.GetLink.Path;
                }

                return null;
            } catch {
                return null;
            }
        }
        #endregion

        [ComImport, Guid("13709620-C279-11CE-A49E-444553540000")]
        private class Shell32 { }
    }
}
