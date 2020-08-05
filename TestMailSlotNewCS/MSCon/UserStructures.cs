using System.Runtime.InteropServices;

namespace MSCon
{
  public enum MESSAGE_ID
  {
    MSG_COMPUTER_STRING_MSG,
    MSG_COMPUTER_ORDER, 
    MSG_MOBILE_ORDER
  }

  public enum OPERATING_SYSTEM_COMPUTER { Free_Dos, Windows10, Mac_OS_X, Linux };

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct STRING_MESSAGE
  {
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
    public byte[] message;
  }
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct ALL_IN_ONE_COMPUTER
  {
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public byte[] Customer;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public byte[] Manufacturer;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public byte[] CPU;
    public uint Display;
    public OPERATING_SYSTEM_COMPUTER os;
    public uint SSD;
    public uint memory;
  }

  public enum MANUFACTURER { Apple, Samsung, Huawei, LG, Xiaomi };
  public enum OPERATING_SYSTEM_MOBILE { iOS, Android, Other };
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct MOBILE_PHONE
  {
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public byte[] Customer;
    public MANUFACTURER Manufacturer;
    public uint Display;
    public OPERATING_SYSTEM_MOBILE osm;
    public uint memory;
}


}
