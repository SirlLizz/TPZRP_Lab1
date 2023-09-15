using System.Net.Sockets;
using System.Net;

namespace Server
{
    public class Server
    {
        public EndPoint Ip;
        public int Listen;
        public bool Active;
        private readonly Socket _listener;
        private volatile CancellationTokenSource _cts;

        public Server(int port)
        {
            Listen = port;
            Ip = new IPEndPoint(Dns.GetHostAddresses(Dns.GetHostName())[0], Listen);
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this._cts = new CancellationTokenSource();
        }

        public Server(string ip, int port)
        {
            Listen = port;
            Ip = new IPEndPoint(IPAddress.Parse(ip), Listen);
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this._cts = new CancellationTokenSource();
        }
        public void Start()
        {
            if (!Active)
            {
                _listener.Bind(Ip);
                _listener.Listen();
                Active = true;
                while (Active)
                {
                    try
                    {
                        Socket listenerAccept = _listener.Accept();
                        if (listenerAccept != null)
                        {
                            Task.Run(
                                () => new ClientThread(listenerAccept),
                                _cts.Token
                            );
                        }
                    }
                    catch { }
                }
            }
            else
            {
                Console.WriteLine("Server was started");
            }
        }

        public void Stop()
        {
            if (Active)
            {
                _cts.Cancel();
                _listener.Close();
                Active = false;
            }
            else
            {
                Console.WriteLine("Server was stopped");
            }
        }
    }
}