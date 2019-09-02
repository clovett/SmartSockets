// ------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------------------------------------------

using System;
using LovettSoftware.SmartSockets.Interface;
using System.Threading.Tasks;

namespace LovettSoftware.SmartSockets.TestServer
{    
    class Program
    {
        const string Name = "TestServer";

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Server:");
            Program p = new Program();
            p.Start();
        }

        void Start()
        {
            SmartSocketServer server = SmartSocketServer.StartServer(Name, new SmartSocketTypeResolver(typeof(ServerMessage), typeof(ClientMessage)));
            server.ClientConnected += OnClientConnected;
            server.ClientDisconnected += OnClientDisconnected;
            server.BackChannelOpened += OnBackChannelOpened;

            Console.WriteLine("Press any key to terminate...");
            Console.ReadLine();
        }

        private void OnClientDisconnected(object sender, SmartSocketClient e)
        {
            Console.WriteLine("Client '{0}' has gone bye bye...", e.Name);
        }

        private void OnClientConnected(object sender, SmartSocketClient e)
        {
            e.Error += OnClientError;
            Console.WriteLine("Client '{0}' is connected", e.Name);
            Task.Run(() => HandleClientAsync(e));
        }

        private async void OnBackChannelOpened(object sender, SmartSocketClient e)
        {
            for (int i = 0; i < 10; i++)
            {
                var response = await e.SendReceiveAsync(new SocketMessage("BackChannelRequest", Name) { Message = "backchannel request" });                
                Console.WriteLine("Response from client is: " + response.Id);
            }
        }

        private void OnClientError(object sender, Exception e)
        {
            var saved = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(e.Message);
            Console.ForegroundColor = saved;
        }

        private async void HandleClientAsync(SmartSocketClient client)
        {
            while (client.IsConnected)
            {
                ClientMessage e = await client.ReceiveAsync() as ClientMessage;
                if (e != null)
                {
                    if (e.Id == "test")
                    {
                        await client.SendAsync(new ServerMessage("test", Name, DateTime.Now));
                    }
                    else
                    {
                        Console.WriteLine("Received message '{0}' from '{1}' at '{2}'", e.Id, e.Sender, e.Timestamp);
                        await client.SendAsync(new ServerMessage("Server says hi!", Name, DateTime.Now));
                    }
                }
            }
        }
    }
}
