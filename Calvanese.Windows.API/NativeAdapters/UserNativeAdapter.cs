using Calvanese.Windows.API.Structs;
using System;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace Calvanese.Windows.API.NativeAdapters;

public static class UserNativeAdapter {
    #region Data (Library)

    private const string UserInterfaceFileName = "user32.dll";

    #endregion

    #region Delegates (Callbacks)

    public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    #endregion

    #region Methods (Hooks)

    [DllImport(UserInterfaceFileName, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport(UserInterfaceFileName, CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport(UserInterfaceFileName, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport(UserInterfaceFileName)]
    public static extern IntPtr GetForegroundWindow();

    #endregion
}
