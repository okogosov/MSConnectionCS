using System;
using System.Collections.Generic;
using System.Text;
using MSConnectionCS;
using System.Runtime.InteropServices;
namespace MSCon
{ 
  class MSConnectionServerTest : MSConnectionServer
  {
//=====================================================================
    public override void GetMsg(byte[] MailMessage, int size)
    {
      string ClientCompName;
      string ClientName;
      int size_header;
      uint id = 0;
      IntPtr intptr = IntPtr.Zero;

      if (mode == ConnectionMode.OTHER_COMPUTER)
      {
        size_header = Marshal.SizeOf(typeof(CONNECT_HEADER));
        if (MailMessage.Length < size_header)
        {
          ConsoleWriteLineDeb("Server: Error - Message without header");
          return;
        }
        intptr = Marshal.AllocHGlobal(size);
        Marshal.Copy(MailMessage, 0, intptr, size);
        CONNECT_HEADER Header;
        Header = (CONNECT_HEADER)Marshal.PtrToStructure(intptr, typeof(CONNECT_HEADER));

        id = Header.msg_id;
        foreach (var value in Enum.GetValues(typeof(MESSAGE_ID)))
        {
          if ((MESSAGE_ID)id == (MESSAGE_ID)value)
          {
            ClientName = ByteToString(Header.ClientName, Encoding.ASCII);
            ConsoleWriteLineDeb(Global.PrintFlag, "Header.ClientName: " + ClientName);
            ConsoleWriteLineDeb(Global.PrintFlag, "Header.ProcessId: " + Header.ProcessId);
            ConsoleWriteLineDeb(Global.PrintFlag, "Header.tickCount: " + Header.tickCount);
            ConsoleWriteLineDeb(Global.PrintFlag, "Header.RandomId: " + Header.RandomId);
            ConsoleWriteLineDeb(Global.PrintFlag);
            break;
          }
        }
      }
      else
      {
        size_header = Marshal.SizeOf(typeof(CONNECT_SHORT_HEADER));
        if (MailMessage.Length < size_header)
        {
          ConsoleWriteLineDeb("Server: Error - Message without header");
          return;
        }

        intptr = Marshal.AllocHGlobal(size);
        Marshal.Copy(MailMessage, 0, intptr, size);
        CONNECT_SHORT_HEADER Header;
        Header = (CONNECT_SHORT_HEADER)Marshal.PtrToStructure(intptr, typeof(CONNECT_SHORT_HEADER));

        id = Header.msg_id;
        foreach (var value in Enum.GetValues(typeof(MESSAGE_ID)))
        {
          if ((MESSAGE_ID)id == (MESSAGE_ID)value)
          {
            ClientName = ByteToString(Header.ClientName, Encoding.ASCII);
            ConsoleWriteLineDeb(Global.PrintFlag, "Header.ClientName: " + ClientName);
            ConsoleWriteLineDeb(Global.PrintFlag, "Header.ProcessId: " + Header.ProcessId);
            ConsoleWriteLineDeb(Global.PrintFlag, "Header.tickCount: " + Header.tickCount);
            ConsoleWriteLineDeb(Global.PrintFlag, "Header.RandomId: " + Header.RandomId);
            ConsoleWriteLineDeb(Global.PrintFlag);
            break;
          }
        }    

      }
      MESSAGE_ID msg_id = (MESSAGE_ID)id;
      switch (msg_id)
      {
        case MESSAGE_ID.MSG_COMPUTER_STRING_MSG:
        {
            Console.WriteLine("---MSG_COMPUTER_STRING_MSG---");
            STRING_MESSAGE sm;
            sm = (STRING_MESSAGE)Marshal.PtrToStructure(intptr + size_header, typeof(STRING_MESSAGE));
            string message = ByteToString(sm.message, Encoding.ASCII);
            Console.WriteLine("   {0}", message);
            break;
          }
        case MESSAGE_ID.MSG_COMPUTER_ORDER:
          {
            Console.WriteLine("---MSG_COMPUTER_ORDER---");
            ALL_IN_ONE_COMPUTER aoc;
            aoc = (ALL_IN_ONE_COMPUTER)Marshal.PtrToStructure(intptr + size_header, typeof(ALL_IN_ONE_COMPUTER));
            string Customer = ByteToString(aoc.Customer, Encoding.ASCII);
            string Manufacturer = ByteToString(aoc.Manufacturer, Encoding.ASCII);
            string CPU = ByteToString(aoc.CPU, Encoding.ASCII);
            Marshal.FreeHGlobal(intptr);
            Console.WriteLine("   Display: " + aoc.Display);
            Console.WriteLine("   memory: " + aoc.memory);
            Console.WriteLine("   os:" + aoc.os);
            Console.WriteLine("   SSD: " + aoc.SSD);
            Console.WriteLine("   Manufacturer: " + Manufacturer);
            Console.WriteLine("   Customer: " + Customer);
            Console.WriteLine("   CPU: " + CPU);
            Console.WriteLine();
            break;
          }
        case MESSAGE_ID.MSG_MOBILE_ORDER:
        {
            Console.WriteLine("---MSG_MOBILE_ORDER---");
            MOBILE_PHONE mp;
            mp = (MOBILE_PHONE)Marshal.PtrToStructure(intptr + size_header, typeof(MOBILE_PHONE));
            string Customer = ByteToString(mp.Customer, Encoding.ASCII);
            Console.WriteLine("   Display: " + mp.Display);
            Console.WriteLine("   memory: " + mp.memory);
            Console.WriteLine("   osm:" + mp.osm);
            Console.WriteLine("   Manufacturer: " + mp.Manufacturer);
            Console.WriteLine("   Customer: " + Customer);
            Console.WriteLine();
            break;
        }

        default:
        {
            Console.WriteLine(Encoding.ASCII.GetString(MailMessage));
            break;
        }

      }
      return;
    }
  }
}
