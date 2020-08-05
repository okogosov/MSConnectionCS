using System;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;

namespace MSConnectionCS
{
  public class MSConnectionClient : MSConnectionCS
  {
    string ClientCompName;
    string ServerAddress;
    string MailslotName;
    string ClientName;
    ConnectionMode mode;
    string Full_MailslotName;
    string SemaphoreName;
    uint ProcessId;
    Random random;

    public IntPtr MailSlotWOut;
    public Semaphore SemClient = null;
    //=======================================================================================
    public bool BuildClient(string ServerAddress, string MailSlot_Name, string client_name, ConnectionMode mode)
    {
      random = new Random();
        
      ClientCompName = Environment.MachineName.ToString();
      this.ServerAddress = ServerAddress;
      this.MailslotName =  MailSlot_Name;
      ClientName = client_name;
      this.mode = mode;
      Full_MailslotName = "\\\\" + ServerAddress + "\\mailslot\\" + MailSlot_Name;
      SemaphoreName = MailSlot_Name + "_Sem";
      Process currentProcess = Process.GetCurrentProcess();
      ProcessId = (uint)currentProcess.Id;
      RunClient();
      return true;
    }
    //=======================================================================================
    public bool Send(string word)
    {
      int cbWritten = 0;
      byte[] bword = new byte[word.Length];
      bword = Encoding.ASCII.GetBytes(word);  
      bool ret = Kernel32.WriteFile(MailSlotWOut, bword,  bword.Length, out cbWritten, IntPtr.Zero);
      if (ret == false)
      {
        ConsoleWriteLineDeb(Global.PrintFlag, "Client: error MailSlotWOut");
        return false;
      }
      if (mode == ConnectionMode.SAME_COMPUTER)
      {
        try
        {
          int count = SemClient.Release();
        }
        catch (Exception ex)
        {
          ConsoleWriteLineDeb(Global.PrintFlag, "Client: {0}", ex.Message);
          return false;
        }
      }
      return true;
    }
    //=======================================================================================
    public bool Send(byte[] b)  
    {
      int cbWritten = 0;
      
      bool ret = Kernel32.WriteFile(MailSlotWOut, b, b.Length,  out cbWritten, IntPtr.Zero);
      if (ret == false)
      {
        ConsoleWriteLineDeb(Global.PrintFlag, "Client: error MailSlotWOut");
        return false;
      }
      if (mode == ConnectionMode.SAME_COMPUTER)
      {
        try
        {
          int count = SemClient.Release();
        }
        catch (Exception ex)
        {
          ConsoleWriteLineDeb(Global.PrintFlag, "Client: {0}", ex.Message);
          return false;
        }
      }
      return true;
    }
    //===================================================================================
    public CONNECT_HEADER PrepareHeader(uint msg_id)
    {
      CONNECT_HEADER Header = new CONNECT_HEADER();
      Header.msg_id = msg_id;
      Header.ProcessId = ProcessId;
      Header.tickCount = Kernel32.GetTickCount();
      Header.RandomId =  random.Next(0, 100000);
      Header.ClientName = StringToByte(ClientName, Global.MAX_CLIENTNAME_LENGTH, Encoding.ASCII);
      Header.ClientName = StringToByte(ClientCompName, Global.MAX_COMPUTERNAME_LENGTH, Encoding.ASCII);
      return Header;
    }
    //===================================================================================
    public CONNECT_SHORT_HEADER PrepareShortHeader(uint msg_id)
    {
      CONNECT_SHORT_HEADER Header = new CONNECT_SHORT_HEADER();
      Header.msg_id = msg_id;
      Header.ProcessId = ProcessId;
      Header.tickCount = Kernel32.GetTickCount();
      Header.RandomId =  random.Next(0, 100000);
      Header.ClientName = StringToByte(ClientName, Global.MAX_CLIENTNAME_LENGTH, Encoding.ASCII);
      return Header;
    }
    //===================================================================================  
    public bool SendMsg<T>(uint msg_id, T Data) 
    {
      bool ret = true;
      int TransmitMsg_size = 0;
      byte[] TransmitMsg  = null;
      int sizeHeader = 0;
      int sizeT = Marshal.SizeOf(typeof(T));
      IntPtr intptr = IntPtr.Zero;

      if (mode == ConnectionMode.OTHER_COMPUTER)
      {
        sizeHeader = Marshal.SizeOf(typeof(CONNECT_HEADER));

        TransmitMsg_size = sizeHeader + sizeT;
        TransmitMsg = new byte[TransmitMsg_size];
        CONNECT_HEADER Header = PrepareHeader(msg_id);
        intptr = Marshal.AllocHGlobal(TransmitMsg_size); // place for (CONNECT_HEADER) header + T data
        Marshal.StructureToPtr(Header, intptr, true);
      }
 
      else
      {
          sizeHeader = Marshal.SizeOf(typeof(CONNECT_SHORT_HEADER));

          TransmitMsg_size = sizeHeader + sizeT;
          TransmitMsg = new byte[TransmitMsg_size];
          CONNECT_SHORT_HEADER Header = PrepareShortHeader(msg_id);
          intptr = Marshal.AllocHGlobal(TransmitMsg_size);   // place for (CONNECT_SHORT_HEADER) header + T data
          Marshal.StructureToPtr(Header, intptr, true);        
      }
      Marshal.StructureToPtr(Data, intptr + sizeHeader, true);
      Marshal.Copy(intptr, TransmitMsg, 0, TransmitMsg_size);
      Marshal.FreeHGlobal(intptr);
      ret = Send(TransmitMsg);
      return ret;
    }
 

//=======================================================================================
    public bool RunClient()
    {
      //it is waiting connection with mailslot server if server does not running
      while (true)
      {
        MailSlotWOut = Kernel32.CreateFile(Full_MailslotName,
           Kernel32.GENERIC_WRITE,
           Kernel32.FILE_SHARE_READ | Kernel32.FILE_SHARE_WRITE,
           0,
           Kernel32.OPENEXISTING,
           Kernel32.FILE_ATTRIBUTE_NORMAL,
           0);
        ConsoleWriteLineDeb(Global.PrintFlag, "Client: Create File  Client  MailSlotWOut  {0}\n", Full_MailslotName);
        if (MailSlotWOut == Global.INVALID_HANDLE_VALUE)
        {
          Thread.Sleep(500);   // wait for server
          continue;
        }
        break;
      }
      if (mode == ConnectionMode.SAME_COMPUTER)
      {
        try
        {
          SemClient = Semaphore.OpenExisting(SemaphoreName);  
          ConsoleWriteLineDeb(Global.PrintFlag, "Client:  Semaphore.OpenExisting 0x{0:X}\n", SemClient.Handle);
         }
        catch(Exception ex)
        {
          ConsoleWriteLineDeb(Global.PrintFlag,"Client: {0}",ex.Message);
          return false;
        }
      }
     return true;
    }
   
    public void DisposeUnmanagedResources()
    {
      if (SemClient != null)
        Kernel32.CloseHandle(SemClient.Handle);
      Kernel32.CloseHandle(MailSlotWOut);
    }
        
    public void Dispose()
    {
      //Dispose(true);
      DisposeUnmanagedResources();
      GC.SuppressFinalize(this);
    }
  }
}
