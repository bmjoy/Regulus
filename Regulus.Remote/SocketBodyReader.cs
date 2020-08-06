﻿using System;
using System.Net.Sockets;
using Regulus.Network;

namespace Regulus.Remote
{

    internal interface ISocketReader
    {
        event OnByteDataCallback DoneEvent;
        event OnErrorCallback ErrorEvent;
    }
    internal class SocketBodyReader : ISocketReader
    {
        public event OnByteDataCallback DoneEvent;

        public event OnErrorCallback ErrorEvent;

        private readonly IStreamable _Peer;

        private byte[] _Buffer;

        private int _Offset;

        public SocketBodyReader(IStreamable peer)
        {
            this._Peer = peer;
        }

        internal void Read(int size)
        {
            _Offset = 0;
            _Buffer = new byte[size];
            try
            {
                var task = _Peer.Receive(_Buffer, _Offset, _Buffer.Length - _Offset);
                task.ContinueWith(t=>_Readed(t.Result));


            }
            catch(SystemException e)
            {
                if(ErrorEvent != null)
                {
                    ErrorEvent();
                }
            }
        }

        private void _Readed(int read_count  )
        {
            var readSize = read_count;

            _Offset += readSize;
            NetworkMonitor.Instance.Read.Set(readSize);
            if (_Offset == _Buffer.Length)
            {
                DoneEvent(_Buffer);
            }
            else
            {
                var task = _Peer.Receive(
                    _Buffer,
                    _Offset,
                    _Buffer.Length - _Offset);
                task.ContinueWith(t => _Readed(t.Result));
            }
        }
    }
}