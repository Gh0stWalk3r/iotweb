using System;
using System.IO;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.System.Threading;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Networking.Connectivity;
using IotWeb.Common;
using IotWeb.Common.Util;

namespace IotWeb.Server
{
    public class SocketServer : ISocketServer
    {
        // Instance variables
        private ConnectionHandler m_handler;
        private List<StreamSocketListener> m_listeners;
        public string HostName { get; set; }
        public NetworkInterface NetworkInterface { get; set; }
        public bool Running { get; private set; }
        public int Port { get; private set; }

        public ConnectionHandler ConnectionRequested
        {
            get
            {
                return m_handler;
            }

            set
            {
                lock (this)
                {
                    if (Running)
                        throw new InvalidOperationException("Cannot change handler while server is running.");
                    m_handler = value;
                }
            }
        }

        public event ServerStoppedHandler ServerStopped;

        public SocketServer(int port)
        {
            Running = false;
            Port = port;
        }
        public SocketServer(int port, string hostName)
        {
            Running = false;
            Port = port;
            NetworkInterface = NetworkInterface.Any;
        }
        public SocketServer(int port, string hostName, NetworkInterface interfaceType)
        {
            Running = false;
            Port = port;
            NetworkInterface = interfaceType;
        }
        public async System.Threading.Tasks.Task<bool> StartAsync()
        {
            lock (this)
            {
                if (Running)
                    throw new InvalidOperationException("Server is already running.");
                Running = true;
            }
            m_listeners = new List<StreamSocketListener>();
            StreamSocketListener listener;


            foreach (HostName candidate in NetworkInformation.GetHostNames())
            {

                if ((candidate.Type == HostNameType.Ipv4) || (candidate.Type == HostNameType.Ipv6))
                {
                    if (string.IsNullOrEmpty(HostName))
                    {
                        listener = new StreamSocketListener();
                        listener.ConnectionReceived += OnConnectionReceived;
                        await listener.BindEndpointAsync(candidate, Port.ToString());
                        m_listeners.Add(listener);
                    }
                    else
                    {
                        if (NetworkInterface == NetworkInterface.Any)
                        {
                            if (HostName == candidate.DisplayName)
                            {
                                listener = new StreamSocketListener();
                                listener.ConnectionReceived += OnConnectionReceived;
                                await listener.BindEndpointAsync(candidate, Port.ToString());
                                m_listeners.Add(listener);
                            }
                        }
                        else if (NetworkInterface == NetworkInterface.Ethernet)
                        {
                            if (HostName == candidate.DisplayName)
                            {
                                listener = new StreamSocketListener();
                                listener.ConnectionReceived += OnConnectionReceived;
                                await listener.BindEndpointAsync(candidate, Port.ToString());
                                m_listeners.Add(listener);
                            }
                        }
                        else if (NetworkInterface == NetworkInterface.Wireless)
                        {
                            if (HostName == candidate.DisplayName)
                            {
                                listener = new StreamSocketListener();
                                listener.ConnectionReceived += OnConnectionReceived;
                                await listener.BindEndpointAsync(candidate, Port.ToString());
                                m_listeners.Add(listener);
                            }
                        }
                    }
                }
            }

            return m_listeners.Count > 0;
        }

        public void Stop()
        {
            lock (this)
            {
                if (!Running)
                    return;
                Running = false;
                // Clean up all listeners
                foreach (StreamSocketListener listener in m_listeners)
                    listener.Dispose();
                m_listeners.Clear();
                m_listeners = null;
                // Fire the stopped events
                ServerStoppedHandler handler = ServerStopped;
                if (handler != null)
                    handler(this);
            }
        }

        private void OnConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            if ((m_handler != null) && Running)
            {
                IAsyncAction asyncAction = ThreadPool.RunAsync((workItem) =>
                    {
                        StreamSocket s = args.Socket;
                        try
                        {
                            m_handler(
                                this,
                                s.Information.RemoteHostName.CanonicalName.ToString(),
                                s.InputStream.AsStreamForRead(),
                                s.OutputStream.AsStreamForWrite()
                                );
                        }
                        catch (Exception)
                        {
                            // Quietly consume the exception
                        }
                        // Close the client socket
                        s.Dispose();
                    });
            }
        }

    }
}
