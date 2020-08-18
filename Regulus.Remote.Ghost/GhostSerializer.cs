﻿using Regulus.Network;
using Regulus.Serialization;
using Regulus.Utility;
using System;
using System.Collections.Generic;

namespace Regulus.Remote.Ghost
{
    class GhostSerializer : IGhostRequest
    {
        private readonly PackageReader<ResponsePackage> _Reader;

        private readonly System.Collections.Concurrent.ConcurrentQueue<ResponsePackage> _Receives;

        private readonly System.Collections.Concurrent.ConcurrentQueue<RequestPackage> _Sends;

        private readonly PackageWriter<RequestPackage> _Writer;


        
        public GhostSerializer(ISerializer serializer)
        {
            _Reader = new PackageReader<ResponsePackage>(serializer);
            _Writer = new PackageWriter<RequestPackage>(serializer);
            _Sends = new System.Collections.Concurrent.ConcurrentQueue<RequestPackage>();
            _Receives = new System.Collections.Concurrent.ConcurrentQueue<ResponsePackage>();

            _ResponseEvent += _Empty;
        }

        private void _Empty(ServerToClientOpCode arg1, byte[] arg2)
        {
        }

        event Action<ServerToClientOpCode, byte[]> _ResponseEvent;

        event Action<ServerToClientOpCode, byte[]> IGhostRequest.ResponseEvent
        {
            add
            {
                _ResponseEvent += value;

            }

            remove
            {
                _ResponseEvent -= value;
            }
        }

        void IGhostRequest.Request(ClientToServerOpCode code, byte[] args)
        {
            _Sends.Enqueue(
                    new RequestPackage()
                    {
                        Data = args,
                        Code = code
                    });            
        }

        public void Start(IStreamable peer)
        {
            Singleton<Log>.Instance.WriteInfo("Agent online enter.");
            _ReaderStart(peer);
            _WriterStart(peer);
        }

        public void Stop()
        {
            _WriterStop();
            _ReaderStop();
            Singleton<Log>.Instance.WriteInfo("Agent online leave.");
        }

        void _Update()
        {
            _Process();
        }

        private void _ReceivePackage(ResponsePackage package)
        {
            _Receives.Enqueue(package);
            
        }

        private void _Process()
        {
            ResponsePackage receivePkg;
            while(_Receives.TryDequeue(out receivePkg))
            {
            
                _ResponseEvent(receivePkg.Code, receivePkg.Data);
            }

            

            RequestPackage[] sends = _SendsPop();
            if (sends.Length > 0)
                _Writer.Push(sends);


        }

        private void _WriterStart(IStreamable peer)
        {
            _Writer.ErrorEvent += _Disable;

            _Writer.Start(peer);
        }

        private void _WriterStop()
        {
            _Writer.ErrorEvent -= _Disable;

            _Writer.Stop();
        }

        private RequestPackage[] _SendsPop()
        {           

            List<RequestPackage> pkgs = new List<RequestPackage>();
            RequestPackage pkg;
            while(_Sends.TryDequeue(out pkg))
            {
                pkgs.Add(pkg);
                
            }
            return pkgs.ToArray();
        }

        private void _ReaderStart(IStreamable peer)
        {
            _Reader.DoneEvent += _ReceivePackage;

            _Reader.ErrorEvent += _Disable;
            _Reader.Start(peer);
        }

        private void _Disable()
        {

        }

        private void _ReaderStop()
        {
            _Reader.DoneEvent -= _ReceivePackage;
            _Reader.ErrorEvent -= _Disable;
            _Reader.Stop();
        }

        public void Update()
        {
            _Update();
        }
    }
}
