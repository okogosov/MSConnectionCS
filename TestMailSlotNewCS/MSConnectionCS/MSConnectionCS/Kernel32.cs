using System;
using System.Threading;
using System.Runtime.InteropServices;

//https://www.pinvoke.net/default.aspx/kernel32.GetMailslotInfo

namespace MSConnectionCS
{
    static public class Kernel32
    {
 
    public const uint GENERIC_READ = 0x80000000;
    public const int GENERIC_WRITE = 0x40000000;
    public const int GENERIC_EXECUTE = 0x20000000;

    public const int FILE_SHARE_READ = 1;
    public const int FILE_SHARE_WRITE = 2;
    public const int OPENEXISTING = 3;
    public const int FILE_SHARE_DELETE = 4;

    public const int FILE_ATTRIBUTE_DIRECTORY = 0x10;
    public const int FILE_ATTRIBUTE_NORMAL = 0x80;


    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateMailslot(string lpName, uint nMaxMessageSize,
                 uint lReadTimeout, IntPtr SecurityAttributes);


        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateFile(string fileName,
                 /*FileDesiredAccess*/ uint desiredAccess,
                 /*FileShareMode*/ uint shareMode,
                 /*IntPtr*/uint securityAttributes,
                 /*FileCreationDisposition*/uint creationDisposition,
                 uint flagsAndAttributes, 
                 /*IntPtr*/
                 uint hTemplateFile);


    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
     public static extern bool GetMailslotInfo(IntPtr hMailslot,
          IntPtr lpMaxMessageSize,
          out int lpNextSize, 
          out int lpMessageCount,
          IntPtr lpReadTimeout);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        // [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ReadFile(IntPtr handle,
            byte[] bytes, int numBytesToRead, out int numBytesRead,
            IntPtr overlapped);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        // [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WriteFile(IntPtr handle,
            byte[] bytes, int numBytesToWrite, out int numBytesWritten,
            IntPtr overlapped);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hHandle);

    [DllImport("kernel32.dll")]
       public static extern uint GetTickCount();

  }
}
