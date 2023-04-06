using Calvanese.Windows.API.NativeAdapters;
using Calvanese.Windows.API.Structs;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Calvanese.Windows.API.Contexts;

public class DeviceContext : IDisposable {
    #region Constructors

    public DeviceContext(IntPtr handle) {
        // Fields.
        _handle = handle;
    }

    ~DeviceContext() {
        // Destruct.
        Dispose();
    }

    public void Dispose() {
        // Destruct.
        GDINativeAdapter.DeleteDC(_handle);

        // Suppress.
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Data (Handle)

    private readonly IntPtr _handle = IntPtr.Zero;

    #endregion

    #region Data (Seeds)

    private int _randomSeed = 0;
    private int _randomInterval = 4;

    #endregion

    #region Methods (Devices)
    #nullable enable

    public static DeviceContext? GetDeviceContextFromDeviceName(string deviceName) {
        // Get the device context handle.
        IntPtr deviceContextHandle = GDINativeAdapter.CreateDC(deviceName, null, null, IntPtr.Zero);

        // Return null if it doesn't exist.
        if (deviceContextHandle == IntPtr.Zero) return null;

        // Return the device context of the device context handle.
        return new DeviceContext(deviceContextHandle);
    }

    #nullable disable
    #endregion

    #region Methods (Gamma)

    public void ResetDeviceGammaRamp() {
        // Route.
        SetDeviceGammaRamp(1.0, 1.0, 1.0);
    }

    public void SetDeviceGammaRamp(double redGammaRamp, double greenGammaRamp, double blueGammaRamp) {
        // Create the next gamma ramp struct.
        GammaRampStruct nextGammaRampStruct = new GammaRampStruct {
            Red = new ushort[256],
            Green = new ushort[256],
            Blue = new ushort[256]
        };

        // Fill the next gamma ramp struct.
        for (int i = 0; i < 256; i++) {
            nextGammaRampStruct.Red[i] = (ushort)(i * 255 * redGammaRamp);
            nextGammaRampStruct.Green[i] = (ushort)(i * 255 * greenGammaRamp);
            nextGammaRampStruct.Blue[i] = (ushort)(i * 255 * blueGammaRamp);
        }

        // Adjust the random seed.
        _randomSeed = (_randomSeed + 1) % _randomInterval;

        // Add the random seed to the next gamma ramp struct to make it always different from the one before (causes problems otherwise).
        nextGammaRampStruct.Red[255] = (ushort)(nextGammaRampStruct.Red[255] + _randomSeed);
        nextGammaRampStruct.Green[255] = (ushort)(nextGammaRampStruct.Green[255] + _randomSeed);
        nextGammaRampStruct.Blue[255] = (ushort)(nextGammaRampStruct.Blue[255] + _randomSeed);

        // Route.
        SetDeviceGammaRamp(nextGammaRampStruct);
    }

    #endregion

    #region Support (Gamma)

    private void SetDeviceGammaRamp(GammaRampStruct nextGammaRampStruct) {
        // Route.
        GDINativeAdapter.SetDeviceGammaRamp(_handle, ref nextGammaRampStruct);
    }

    #endregion
}
