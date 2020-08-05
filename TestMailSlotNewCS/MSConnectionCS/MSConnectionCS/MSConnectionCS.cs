using System;
using System.Text;
using System.Runtime.InteropServices;

namespace MSConnectionCS
{
  public enum ConnectionMode : byte
  {
    SAME_COMPUTER,
    OTHER_COMPUTER
  }

  static public class Global
  {
    public const int MAX_COMPUTERNAME_LENGTH = 50;
    public const int MAX_CLIENTNAME_LENGTH = 20;     
    static public bool PrintFlag = true;
    static public readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct CONNECT_HEADER
  {
    public uint msg_id;
    public uint tickCount;
    public uint ProcessId;
    public int RandomId;    // offset 12 bytes   message with same RandomId will throw away as repeated
                                               // if server_address != "."  
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Global.MAX_CLIENTNAME_LENGTH)]
    public byte[] ClientName;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Global.MAX_COMPUTERNAME_LENGTH)]
    public byte[] ClientCompName;
  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct CONNECT_SHORT_HEADER
  {
    public uint msg_id;
    public uint tickCount;
    public uint ProcessId;
    public int RandomId;      // offset 12 bytes
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = Global.MAX_CLIENTNAME_LENGTH)]
    public byte[] ClientName;
  }
  //===================================================================================
  public class MSConnectionCS : IDisposable
  {
    //===================================================================================
    public byte[] CreateInitializedArray(byte value, int counter)
    {
      byte[] arr = new byte[counter];
      if (value != 0)
        for (int i = 0; i < counter; i++) arr[i] = value;
      return arr;
    }
    //===================================================================================
    public byte[] StringToByte(string str, int size_byte_arr)
    {
      if (str.Length > (size_byte_arr - 1))
        return null;
      byte[] arr = new byte[size_byte_arr];
      byte[] tmp = Encoding.ASCII.GetBytes(str);
      Buffer.BlockCopy(tmp, 0, arr, 0, str.Length);
      return arr;
    }
    //===================================================================================
    public byte[] StringToByte(string str, int size_byte_arr, Encoding enc)
    {
      if (str.Length > (size_byte_arr - 1))
        return null;
      byte[] arr = new byte[size_byte_arr];
      byte[] tmp = enc.GetBytes(str);
      Buffer.BlockCopy(tmp, 0, arr, 0, tmp.Length);
      return arr;
    }
    //===================================================================================
    public string ByteToString(byte[] arr, Encoding enc)
    {
      int size = 0;
      for (int i = 0; i < arr.Length; i += Marshal.SizeOf(arr[0]))
      {
        if (arr[i] == '\0')
        {
          size = i;
          break;
        }
      }
      if (size == 0)
        return null;
      else
      {
        return enc.GetString(arr, 0, size);
      }
    }
    //===================================================================================
    public void ConsoleWriteLineDeb(params object[] list)
    {
      bool WriteFlag = true;
      if (list.Length > 0)
        WriteFlag = (bool)list[0];
      else
      {
        Console.WriteLine();
        return;
      }
      if (WriteFlag == false)   // only false 
        return;
      if (list.Length == 1)     // only true
      {
        Console.WriteLine();
        return;
      }
      string format = "";
      if (list.Length == 2)          // true|false +  format
      {
        Console.WriteLine(list[1]);
        return;
      }
      object[] list2 = new object[list.Length - 2];
      format = (string)list[1];
      for (int i = 2; i < list.Length; i++)
        list2[i - 2] = list[i];

      Console.WriteLine(format, list2);
    }
    //===================================================================================
    public void Dispose() { }
  }
}
