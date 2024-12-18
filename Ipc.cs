using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO.Pipes;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Runtime.CompilerServices;
using static QwertyLauncher.Views.InputHook;
using static QwertyLauncher.Ipc;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace QwertyLauncher
{
    public class Ipc
    {
        private static readonly string _pipeName = $"\\\\.\\{App.Name}-{Environment.UserName}";
        private  Task _task;
        internal event EventHandler<CommandLineEventArgs> OnCommandLineEvent = delegate { };

        public void StartServer()
        {
            _task = Task.Run(() =>
            {
                while (true)
                {
                    NamedPipeServerStream pipeServer = null;
                    try
                    {
                        PipeSecurity pipeSecurity = new PipeSecurity();
                        pipeSecurity.AddAccessRule(new PipeAccessRule(
                            WindowsIdentity.GetCurrent().User,
                            PipeAccessRights.FullControl,
                            AccessControlType.Allow));
                        pipeSecurity.AddAccessRule(new PipeAccessRule(
                            @"NT AUTHORITY\NETWORK",
                            PipeAccessRights.FullControl,
                            AccessControlType.Deny));

                        pipeServer = new NamedPipeServerStream(
                            _pipeName,
                            PipeDirection.In,
                            1,
                            PipeTransmissionMode.Byte,
                            PipeOptions.None,
                            1024,
                            1024,
                            pipeSecurity,
                            HandleInheritability.None
                            );
                        PipeSecurity acl = pipeServer.GetAccessControl();

                        Console.WriteLine("Waiting for client connection...");
                        pipeServer.WaitForConnection();
                        Console.WriteLine("Client Connected.");

                        using (StreamReader reader = new StreamReader(pipeServer)) 
                        {
                            string response = reader.ReadLine();
                            Console.WriteLine("Received from server: " + response);
                            OnCommandLineEvent(this, new CommandLineEventArgs(response));
                        }
                    }
                    catch (Exception ex) {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        pipeServer?.Dispose();
                    }
                }
            });
        }
        public static void SendToMainProcess(string[] args) {

            using (var pipeClient = new NamedPipeClientStream(
                ".", 
                _pipeName, 
                PipeDirection.Out
            )){
                pipeClient.Connect();
                using (StreamWriter writer = new StreamWriter(pipeClient))
                {
                    writer.AutoFlush = true;
                    writer.WriteLine(JsonSerializer.Serialize(args));
                }
            }
        }
        public class CommandLineEventArgs : EventArgs
        {
            public string[] args;
            public CommandLineEventArgs(string argsJson){
                args = JsonSerializer.Deserialize<string[]>(argsJson);
            }
        }
    }
}