﻿using Regulus.Network;
using System;
using System.Collections.Generic;
using System.Net;

namespace Regulus.Remote.Tests
{
    internal class SocketHeadReaderTestPeer : Network.IPeer
    {
        private readonly Queue<byte> _Buffer;

        public SocketHeadReaderTestPeer()
        {
            _Buffer = new System.Collections.Generic.Queue<byte>(new byte[] { 0x85 , 0x05 });
        }

        EndPoint IPeer.RemoteEndPoint => throw new NotImplementedException();

        EndPoint IPeer.LocalEndPoint => throw new NotImplementedException();

        bool IPeer.Connected => throw new NotImplementedException();

        void IPeer.Close()
        {
            throw new NotImplementedException();
        }

        void IPeer.Receive(byte[] buffer, int offset, int count, Action<int> done)
        {
            buffer[offset] = _Buffer.Dequeue();
            done(1);
        }

        Task IPeer.Send(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}