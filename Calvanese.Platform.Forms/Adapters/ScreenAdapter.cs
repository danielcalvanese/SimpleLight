using Calvanese.Windows.API.Contexts;

namespace Calvanese.Platform.Forms.Adapters;

public static class ScreenAdapter {
    #region Methods (Devices)
    
    public static IReadOnlyList<DeviceContext> GetDeviceContexts() {
        // Prepare the list of device contexts.
        List<DeviceContext> deviceContexts = new List<DeviceContext>();

        // Fill the list of device contexts.
        foreach (Screen screen in Screen.AllScreens) {
            // Get the device context of the screen.
            DeviceContext? deviceContext = DeviceContext.GetDeviceContextFromDeviceName(screen.DeviceName);

            // Add the device context.
            if (deviceContext is not null) deviceContexts.Add(deviceContext);
        }

        // Return the list of device contexts.
        return deviceContexts;
    }

    #endregion
}
