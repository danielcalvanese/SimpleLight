using Calvanese.Windows.API.Structs;
using System;
using System.Runtime.InteropServices;

namespace Calvanese.Windows.API.NativeAdapters;

public static class KernelNativeAdapter {
    #region Data (Library)

    private const string KernelFileName = "kernel32.dll";

    #endregion

    #region Methods (Modules)

    [DllImport(KernelFileName, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr GetModuleHandle(string moduleName);

    #endregion
}
