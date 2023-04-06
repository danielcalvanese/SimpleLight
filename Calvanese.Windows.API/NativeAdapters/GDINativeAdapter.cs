using Calvanese.Windows.API.Structs;
using System;
using System.Runtime.InteropServices;

namespace Calvanese.Windows.API.NativeAdapters;

public static class GDINativeAdapter {
    #region Data (Library)

    private const string GraphicsDeviceInterfaceFileName = "gdi32.dll";

    #endregion

    #region Methods (Device Context)
    #nullable enable

    // Create device context.
    [DllImport(GraphicsDeviceInterfaceFileName, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr CreateDC(string? deviceName, string? nullOnly1, string? nullOnly2, IntPtr initialData);

    // Delete device context.
    [DllImport(GraphicsDeviceInterfaceFileName, SetLastError = true)]
    public static extern bool DeleteDC(IntPtr handleToDeviceContext);

    #nullable disable
    #endregion

    #region Methods (Device Gamma Ramp)

    [DllImport(GraphicsDeviceInterfaceFileName, SetLastError = true)]
    public static extern bool GetDeviceGammaRamp(IntPtr handleToDeviceContext, out GammaRampStruct nextDeviceGammaRamp);

    [DllImport(GraphicsDeviceInterfaceFileName, SetLastError = true)]
    public static extern bool SetDeviceGammaRamp(IntPtr handleToDeviceContext, ref GammaRampStruct nextDeviceGammaRamp);

    #endregion
}
