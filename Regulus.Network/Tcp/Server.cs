using System;
using System.Net;
using System.Net.Sockets;
using Regulus.Utility;

namespace Regulus.Network.Tcp
{
    public class Listener : IListenable
    {
        private readonly System.Net.Sockets.Socket _Socket;
        private event Action<IStreamable> Acctpe;

        event Action<IStreamable> IListenable.AcceptEvent
        {
            add { Acctpe += value; }
            remove { Acctpe -= value; } 
        }
        public Listener()
        {
            _Socket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _Socket.NoDelay = true;
        }
        void IListenable.Bind(int Port)
        {
            

            
            _Socket.Bind(new IPEndPoint(IPAddress.Any, Port));
            _Socket.Listen(backlog: 5);
            _Socket.BeginAccept(Accept, state: null);
        }

        private void Accept(IAsyncResult Ar)
        {
            try
            {
                var socket = _Socket.EndAccept(Ar);
                lock(Acctpe)
                {
                    Acctpe(new Peer(socket));
                }

                _Socket.BeginAccept(Accept, state: null);
            }

                
            catch(SocketException se)
            {
                Singleton<Log>.Instance.WriteInfo(se.ToString());
            }
            catch(ObjectDisposedException ode)
            {
                Singleton<Log>.Instance.WriteInfo(ode.ToString());
            }
            catch(InvalidOperationException ioe)
            {
                Singleton<Log>.Instance.WriteInfo(ioe.ToString());
            }
            catch(Exception e)
            {
                Singleton<Log>.Instance.WriteInfo(e.ToString());
            }
        }

        void IListenable.Close()
        {
            if (_Socket.Connected)
                _Socket.Shutdown(SocketShutdown.Both);

            _Socket.Close();
        }
    }
}