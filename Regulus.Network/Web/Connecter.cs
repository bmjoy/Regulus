﻿using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Regulus.Network.Web
{
    public class Connecter : Peer, IConnectable
    {
        readonly ClientWebSocket _Socket;

        public Connecter(ClientWebSocket socket) : base(socket)
        {
            _Socket = socket;
        }

        System.Threading.Tasks.Task<bool> IConnectable.Connect(EndPoint endpoint)
        {
            IPEndPoint ip = endpoint as IPEndPoint;
            Task connectTask = _Socket.ConnectAsync(new Uri($"ws://{ip.Address.ToString()}:{ip.Port}"), new System.Threading.CancellationToken());
            return connectTask.ContinueWith<bool>(_ConnectResult);


        }

        Task IConnectable.Disconnect()
        {
            throw new NotImplementedException();
        }

        private bool _ConnectResult(Task arg)
        {
            if (_Socket.State == WebSocketState.Open)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
