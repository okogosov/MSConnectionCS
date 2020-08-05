using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace MSConnectionCS
{
  //===================================================================================
  public abstract class MSConnectionServer : MSConnectionCS
  { 
        IntPtr MailSlotRIn = IntPtr.Zero;
        private static Semaphore Sem;
        public string Name;
        public int numberOfMessages;  
        public bool PrintFlag;
        public int Timeout = 1;
        public ConnectionMode mode;
        int LastRandomId = -1;
    //===================================================================================
    void RunServer()
        {
            ConsoleWriteLineDeb(Global.PrintFlag,"Server: RunServer thread is running");
            if (mode == ConnectionMode.SAME_COMPUTER)
            {
               Sem = new Semaphore(0, numberOfMessages, Name + "_Sem");
               ConsoleWriteLineDeb(Global.PrintFlag, "Server: CreateSemaphore  0x{0:X}\n",Sem.Handle);
            }

            Processing();
        }
    //===================================================================================
    public bool BuildServer(string name, int numberOfMessages, ConnectionMode mode)
        {      
            string MailslotName = "\\\\.\\mailslot\\" + name;
            Name = name;   
             this.numberOfMessages = numberOfMessages;
             this.mode = mode;

            MailSlotRIn = Kernel32.CreateMailslot(MailslotName, 0, 0, IntPtr.Zero);
            if (MailSlotRIn == Global.INVALID_HANDLE_VALUE)
            {
                ConsoleWriteLineDeb(Global.PrintFlag,"Server: MailSlotRIn is INVALID_HANDLE_VALUE");
                return false;
            }
            ConsoleWriteLineDeb(Global.PrintFlag, "Server: CreateMailslot 0x{0:X}", MailSlotRIn);
            ThreadStart start = new ThreadStart(RunServer);
            Thread thread = new Thread(RunServer);
            thread.Start();
            return true;
    }
    //===================================================================================
    bool Processing()
        {
           ConsoleWriteLineDeb(Global.PrintFlag,"Server: Processing\n");
           while (true)
           {
             int cbMessage = 0;
             int MessageCount = 0;
             int cbRead = 0;
             bool fResult;             
             int rand = 0;
             switch (mode)
             {
                 case ConnectionMode.SAME_COMPUTER:
                 {
                    bool ret_waitone = Sem.WaitOne();
                    fResult = Kernel32.GetMailslotInfo(MailSlotRIn, 
                        IntPtr.Zero, 
                        out cbMessage, 
                        out MessageCount, IntPtr.Zero);

                    byte[] lpszBuffer = new byte[cbMessage];
                    fResult = Kernel32.ReadFile(MailSlotRIn,lpszBuffer, cbMessage, 
                        out  cbRead, IntPtr.Zero);
                    if (!fResult)
                    { 
                      int error = Marshal.GetLastWin32Error();
                      ConsoleWriteLineDeb(Global.PrintFlag,"Server: The last Win32 Error was: " + error);
                      return false;
                    }
                    rand = BitConverter.ToInt32(lpszBuffer, 12);   // mailslot message sends several times via 
                                                                   // each distinct protocol available   
                    if (rand == LastRandomId)
                    {
                        ConsoleWriteLineDeb(Global.PrintFlag, "Repeated message RandomId={0} was throw away", rand);
                        break;
                    } 
                    LastRandomId = rand;
                    GetMsg(lpszBuffer, cbRead);
                    break;
                 }
   
                 case ConnectionMode.OTHER_COMPUTER:
                 {
                     fResult = Kernel32.GetMailslotInfo(MailSlotRIn,
                        IntPtr.Zero,
                        out cbMessage,
                        out MessageCount, IntPtr.Zero);

                    if (MessageCount == 0)
                    {
                      Thread.Sleep(Timeout);
                      break;
                    }
                    byte[] lpszBuffer = new byte[cbMessage];
                    fResult = Kernel32.ReadFile(MailSlotRIn, lpszBuffer, cbMessage,
                        out cbRead, IntPtr.Zero);
                    if (!fResult)
                    {
                      int error = Marshal.GetLastWin32Error();
                      ConsoleWriteLineDeb(Global.PrintFlag, "Server: The last Win32 Error was: " + error);
                      return false;
                    }
                    rand = BitConverter.ToInt32(lpszBuffer, 12);   // mailslot message sends several times via 
                                                             // each distinct protocol available   
                    if (rand == LastRandomId)
                    {
                       ConsoleWriteLineDeb(Global.PrintFlag, "Repeated message RandomId={0} was throw away", rand);
                       break;
                    }
                    LastRandomId = rand;
                    GetMsg(lpszBuffer, cbRead);
                    Thread.Sleep(Timeout);
                    break;
                 }

             }  // switch (mode)
             continue;

           }   // while...  
          // return true;
        }
    //===================================================================================
    public void DisposeUnmanagedResources()
        {
            if (Sem != null)
              Kernel32.CloseHandle(Sem.Handle);
            Kernel32.CloseHandle(MailSlotRIn);
        }
    //===================================================================================
    public void Dispose()
         {
            //Dispose(true);
            DisposeUnmanagedResources();
            GC.SuppressFinalize(this);
         }
         public abstract void GetMsg(byte[] MailMessage, int size);
    }
}
