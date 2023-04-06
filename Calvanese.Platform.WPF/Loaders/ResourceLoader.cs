using System;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace Calvanese.Platform.WPF.Loaders;

public static class ResourceLoader {
    #region Methods (Loaders)

    // Note that resource paths looks like "Calvanese.Platform.WPF.Resources.Icons.MyIcon.ico".
    // They must be marked as embedded resources.
    public static Icon LoadIcon(Assembly assembly, string resourcePath, Size size) {
        // Load the icon via stream.
        Stream? iconStream = assembly.GetManifestResourceStream(resourcePath);

        // Signify failure when the icon stream is null.
        if(iconStream is null) throw new Exception($"Error:  The resource loader could not load the icon: {resourcePath}.");

        // Return the icon.
        return new Icon(iconStream, size); 
    }

    #endregion
}
