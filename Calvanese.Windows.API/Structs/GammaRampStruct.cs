using System.Runtime.InteropServices;

namespace Calvanese.Windows.API.Structs;

[StructLayout(LayoutKind.Sequential)]
public struct GammaRampStruct {
    #region Data (Colors)

    // Marshall the arrays in place.  

    [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    public ushort[] Red { get; set; }

    [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    public ushort[] Green { get; set; }

    [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    public ushort[] Blue { get; set; }

    #endregion
}
