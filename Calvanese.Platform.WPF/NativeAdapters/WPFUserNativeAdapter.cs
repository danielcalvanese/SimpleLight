using System;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace Calvanese.Platform.WPF.NativeAdapters;

internal static class WPFUserNativeAdapter {
    #region Data (Library)

    private const string UserInterfaceFileName = "user32.dll";

    #endregion

    #region Data (Keys)

    public const int HotKeyWindowsMessageIdentifier = 0x0312;

    #endregion

    #region Methods (Hotkeys)

    [DllImport(UserInterfaceFileName, SetLastError = true)]
    public static extern bool RegisterHotKey(IntPtr hWnd, int id, ModifierKeys fsModifiers, int vk);

    [DllImport(UserInterfaceFileName, SetLastError = true)]
    public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    #endregion
}
