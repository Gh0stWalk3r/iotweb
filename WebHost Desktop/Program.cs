﻿using System;
using System.Threading;
using IotWeb.Server;
using IotWeb.Common.Util;
using IotWeb.Common.Http;
using System.Security.Cryptography.X509Certificates;
using IotWeb.Common;

namespace WebHost.Desktop
{
	/// <summary>
	/// Simple 'echo' web socket server
	/// </summary>
	class WebSocketHandler : IWebSocketRequestHandler
	{

		public bool WillAcceptRequest(string uri, string protocol)
		{
			return (uri.Length == 0) && (protocol == "echo");
		}

		public void Connected(WebSocket socket)
		{
			socket.DataReceived += OnDataReceived;
		}

		void OnDataReceived(WebSocket socket, string frame)
		{
			socket.Send(frame);
		}
	}

	class Program
	{
        static bool USE_SSL = true;
        static void Main(string[] args)
		{
            IServer server;
            if (USE_SSL)
            {
                // Set up and run the server
                string Certificate = "my.cert.pfx";
                // Load the certificate into an X509Certificate object.
                X509Certificate2 cert = new X509Certificate2(Certificate);
                server = new HttpsServer(8000, new SessionConfiguration(), cert);
            }
            else
            {
                server = new HttpServer(8000, new SessionConfiguration());
            }

            server.AddHttpRequestHandler(
                "/",
                new HttpResourceHandler(
                    Utilities.GetContainingAssembly(typeof(Program)),
                    "Resources.Site",
                    "index.html"
                    )
                );

            server.AddWebSocketRequestHandler(
				"/sockets/",
				new WebSocketHandler()
				);
			server.Start();
            Console.WriteLine("Server running - press any key to stop.");
            while (!Console.KeyAvailable)
                Thread.Sleep(100);
            Console.ReadKey();
		}
	}
}
