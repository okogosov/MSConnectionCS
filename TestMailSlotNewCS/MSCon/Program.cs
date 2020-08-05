using  System;
using System.Text;
using  MSConnectionCS;
using System.Linq;
using System.Collections.Generic;
using System.Net;
//using System.Transactions;

namespace MSCon
{

 
  class Program
  {
    static MSConnectionServerTest server = null;
    static MSConnectionClient client = null;
    static string WorkMode = "debug";
    static string Name = "server";
    static string server_address = ".";
    static string client_name = "client"; //[MAX_COMPUTERNAME_LENGTH];
    static ConnectionMode mode = ConnectionMode.OTHER_COMPUTER;
    static uint numberOfMessages = 10;

    static bool IsAllDigits(string s) => s.All(char.IsDigit);
    static string ToString(string src, int MaxSize)
    {
      if (src.Length > MaxSize - 1)
        return null;
      return src;
    }
    static void Help()
    {
      Console.WriteLine("usage: mscon /wm:server|client|debug /cm:0|1 /print:0|1 {0}\n {1}\n {2}\n {3}\n {4}",
                         "      /nmsg:<number>",
                         "      /msname:<mailslot name> ",
                         "      /cname:<client name>",
                         "      /scomp:<server comp.name>",
                         "      /scompip:<server comp.ip>"
                         );

      Console.WriteLine("        /wm      - workmode");
      Console.WriteLine("        /cm      - connection mode 1 - different computers or 0 -same computer (by default 1)");
      Console.WriteLine("        /print   - write(1) or not write(0) on console debug information (by default 1)");
      Console.WriteLine("        /nmsg    - numberOfMessages in MailSlot queue - only for /cm = 0 (by default = 10)");
      Console.WriteLine("        /msname  - server_name by default = server");
      Console.WriteLine("        /cname   - client_name < 19 symbols");
      Console.WriteLine("        /scomp   - server computer name  (if server computer name  != \".\"  /cm = 1)");
      Console.WriteLine("        /scompip - server computer id (if server computer ip is set - /cm = 1)");

      Environment.Exit(0);
    }

    static Dictionary<string, string> GetParameters(string[] args)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      foreach (string str in args)
      {
        string[] KeyValue = str.Split(":");
        if (KeyValue.Length != 2)
        {
          Console.WriteLine("Error {0}", KeyValue[0]);
          Environment.Exit(0);
        }
        parameters.Add(KeyValue[0], KeyValue[1]);
      }
      return parameters;
    }
    //===================================================================================
    static void ReadParameters(string[] args)
    {   // from command line
      string[] KeyList = { "/wm", "/cm", "/nmsg", "/print", "/msname", "/cname", "/scomp", "/scompip" };
      Dictionary<string, string> parameters = GetParameters(args);
      foreach (string key in parameters.Keys)
      {
        bool key_exist = false;
        foreach (string key_pattern in KeyList)
        {
          if (key == key_pattern)
          {
            key_exist = true;
            break;
          }
          continue;
        }
        if (key_exist == false)
        {
            Console.WriteLine("Error: key {0} in parameters is wrong", parameters[key]);
            Environment.Exit(0);
        }
      }
      if (parameters.ContainsKey("/wm"))
      {
        WorkMode = parameters["/wm"];
      }
      if (parameters.ContainsKey("/cm"))
      {
        if (parameters["/cm"] == "0" || parameters["/cm"] == "1")
        {
          mode  = (ConnectionMode)Convert.ToInt32(parameters["/cm"]);
        }
        else
        {
          Console.WriteLine("Error: in /cm {0}", parameters["/cm"]);
          Environment.Exit(0);
        }
      }
      if (parameters.ContainsKey("/nmsg"))
      {
        if (IsAllDigits(parameters["/nmsg"]) == true)
        {
          numberOfMessages = (uint)Convert.ToInt32(parameters["/nmsg"]);
        }
        else
        {
          Console.WriteLine("Error: in /nmsg {0}", parameters["/nmsg"]);
          Environment.Exit(0);
        }
      }
      if (parameters.ContainsKey("/print"))
      {
        if (IsAllDigits(parameters["/print"]) == true)
        {
          if (Convert.ToInt32(parameters["/print"]) == 0)
            Global.PrintFlag = false;
        }
        else
        {
          Console.WriteLine("Error: in /nmsg {0}", parameters["/print"]);
          Environment.Exit(0);
        }
      }
      if (parameters.ContainsKey("/msname"))
      {
        Name = parameters["/msname"];
      }
      if (parameters.ContainsKey("/cname"))
      {
        client_name = ToString(parameters["/cname"], Global.MAX_CLIENTNAME_LENGTH - 1);
        if (client_name == null)
        {
          Console.WriteLine("Error: in /cname {0}  long", parameters["/cname"]);
          Environment.Exit(0);
        }
      }
      if ((parameters.ContainsKey("/scomp") == true) && (parameters.ContainsKey("/scompip") == true))
      {
        Console.WriteLine("Error:  /scomp  &  /scompip together, set one of them");
        Environment.Exit(0);
      }

      if ((parameters.ContainsKey("/scomp") == true) && (parameters.ContainsKey("/scompip") == false))
      {
        server_address = ToString(parameters["/scomp"], Global.MAX_COMPUTERNAME_LENGTH - 1);
        if (server_address != ".")
        {
          mode = ConnectionMode.OTHER_COMPUTER;
          Console.WriteLine("if server_address != \".\"  connection mode = {0}",mode);
        }
        if (Name == null)
        {
          Console.WriteLine("Error in /scomp {0}  long", parameters["/scomp"]);
          Environment.Exit(0);
        }
      }

      if ((parameters.ContainsKey("/scomp") == false) && (parameters.ContainsKey("/scompip") == true))
      {
        server_address = ToString(parameters["/scompip"], Global.MAX_COMPUTERNAME_LENGTH - 1);
        if (server_address != ".")
        {
          mode = ConnectionMode.OTHER_COMPUTER;
          Console.WriteLine("when server_address != \".\"  connection mode = {0}", mode);
        }

        if (Name == null)
        {
          Console.WriteLine("Error in /scomp {0}  long", parameters["/scomp"]);
          Environment.Exit(0);
        }
        IPAddress hostIPAddress = IPAddress.Parse(server_address);
        IPHostEntry hostInfo = Dns.GetHostEntry(hostIPAddress);  
        string[] ServerCompName = hostInfo.HostName.Split("."); // NAME.mshome.net
        server_address = ServerCompName[0];
      }
    }
    //===============================================================
    static void ServerWorkMode()
    {
      server = new MSConnectionServerTest();
      bool ret = server.BuildServer(Name, (int)numberOfMessages, mode);
      if (ret == false)
      {
        Console.WriteLine("Server Creation error!");
        Environment.Exit(0);
      }
      while (true)
      {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("exit - terminate process");
        Console.ForegroundColor = ConsoleColor.White;
        string word = Console.ReadLine();
        if (word == "exit")
        {
          server.Dispose();
          Environment.Exit(0);
        }
      }
    }
    static void DebugWorkMode()
    {
      server = new MSConnectionServerTest();
      bool ret = server.BuildServer(Name, (int)numberOfMessages, mode);
      if (ret == false)
      {
        Console.WriteLine("Server Creation error!");
        Environment.Exit(0);
      }
    }
   
  //===============================================================
  static void Main(string[] args)
    {
 
      if (
           (args.Length == 0) || (args.Length > 5) ||
           (args[0] != "/wm:debug") && (args[0] != "/wm:server") && (args[0] != "/wm:client")
         )
        Help();

      ReadParameters(args);
      Console.WriteLine("{0} {1}\n{2} {3}\n {4} {5}\n {6} {7}\n {8} {9}\n {10} {11}\n {12} {13}\n",
             "Work mode:        ", WorkMode,
             "  Connection mode:  ", mode,
             "  Print:            ", Global.PrintFlag,
             "  numberOfMessages: ", numberOfMessages,
             "  Mailslot name:    ", Name,
             "  client_name:      ", client_name,
             "  server_addr:      ", server_address);
      if (WorkMode == "server")
        ServerWorkMode();
      if (WorkMode == "debug")
        DebugWorkMode();
      
      client = new MSConnectionCS.MSConnectionClient();
      client.BuildClient(server_address, Name, client_name, mode);
 
      while (true)
      {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("0 - to send msg with id =  STRING_MESSAGE");
        Console.WriteLine("1 - to send msg with id =  MSG_COMPUTER_ORDER");
        Console.WriteLine("2 - to send msg with id =  MSG_MOBILE_ORDER");
        Console.WriteLine("exit - terminate process");
        Console.ForegroundColor = ConsoleColor.White;
        string cmd = Console.ReadLine();
        switch (cmd)
        {
          case "0":
            {
              Console.Write("string to send: ");
              string word = Console.ReadLine();
              STRING_MESSAGE sm = new STRING_MESSAGE();
              sm.message = client.StringToByte(word, 200, Encoding.ASCII);
              client.SendMsg<STRING_MESSAGE>((uint)MESSAGE_ID.MSG_COMPUTER_STRING_MSG, sm);
              break;
            }
          case "1":
            {
              ALL_IN_ONE_COMPUTER aoc = new ALL_IN_ONE_COMPUTER();
              aoc.Display = 22;
              aoc.memory = 8;
              aoc.SSD = 256;
              aoc.os = OPERATING_SYSTEM_COMPUTER.Linux;

              aoc.Customer = client.StringToByte("Tom", 20, Encoding.ASCII);
              aoc.Manufacturer = client.StringToByte("Hewlett Paccard", 20, Encoding.ASCII);
              aoc.CPU = client.StringToByte("Intel", 20, Encoding.ASCII);
              client.SendMsg<ALL_IN_ONE_COMPUTER>((uint)MESSAGE_ID.MSG_COMPUTER_ORDER, aoc);
              break;
            }
          case "2":
            {
              MOBILE_PHONE mp = new MOBILE_PHONE();
              mp.Customer = client.StringToByte("Bob", 20, Encoding.ASCII);
              mp.Display = 6;
              mp.Manufacturer = MANUFACTURER.Samsung;
              mp.memory = 512;
              mp.osm = OPERATING_SYSTEM_MOBILE.Android;
              client.SendMsg<MOBILE_PHONE>((uint)MESSAGE_ID.MSG_MOBILE_ORDER, mp);
              break;
            }
          case "exit":
            if (client != null)
              client.Dispose();
            if (server != null)
              server.Dispose();
            Environment.Exit(0);
            break;
          default:
          break;

        }
      }
    }  // static void Main...
  }
}