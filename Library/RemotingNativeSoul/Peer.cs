﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Regulus.Remoting.Soul.Native
{
	
	class Peer : Regulus.Remoting.IRequestQueue, Regulus.Remoting.IResponseQueue , Regulus.Utility.IUpdatable
	{
        class Request
        {
            public Guid EntityId { get; set; }
            public string MethodName { get; set; }
            public Guid ReturnId { get; set; }
            public object[] MethodParams { get; set; }
        }

		System.Net.Sockets.Socket _Socket;
		Regulus.Remoting.Soul.SoulProvider _SoulProvider;
		System.Collections.Generic.Queue<Regulus.Remoting.Package> _Responses;
        System.Collections.Generic.Queue<Request> _Requests;
		Regulus.Game.StageMachine _ReadMachine;
		Regulus.Game.StageMachine _WriteMachine;		
        public Peer(System.Net.Sockets.Socket client)
		{
			
			_Socket = client;
			_SoulProvider = new Remoting.Soul.SoulProvider(this, this);
			_Responses = new Queue<Remoting.Package>();
            _Requests = new Queue<Request>();
			_ReadMachine = new Game.StageMachine();
			_WriteMachine = new Game.StageMachine();

			
		}
		private void _HandleWrite()
		{
			var stage = new NetworkStreamWriteStage(_Socket, _Responses);
			stage.WriteCompletionEvent += _HandleWrite;
			_WriteMachine.Push(stage);
		}
		private void _HandleRead()
		{
			var stage = new NetworkStreamReadStage(_Socket);
			stage.ReadCompletionEvent += (package) =>
			{
				_HandlePackage(package);

				_HandleRead();
			};
			_ReadMachine.Push(stage);
		}

		private void _HandlePackage(Package package)
		{
			if (package.Code == (byte)ClientToServerPhotonOpCode.Ping)
			{
				
				(this as Regulus.Remoting.IResponseQueue).Push((int)ServerToClientPhotonOpCode.Ping, new Dictionary<byte, byte[]>());
			}
            else if (package.Code == (byte)ClientToServerPhotonOpCode.CallMethod)
            {

                var entityId = new Guid(package.Args[0]);
                var methodName = System.Text.Encoding.Default.GetString(package.Args[1]);
                    
                    
                byte[] par = null;
                Guid returnId = Guid.Empty;
                if (package.Args.TryGetValue(2, out par))
                {
                    returnId = new Guid(par as byte[]);
                }

                var methodParams = (from p in package.Args
                                    where p.Key >= 3
                                    orderby p.Key
                                    select Regulus.PhotonExtension.TypeHelper.Deserialize(p.Value)).ToArray();

                _PushRequest(entityId, methodName, returnId, methodParams);
            }
		}

        private void _PushRequest(Guid entity_id, string method_name, Guid return_id, object[] method_params)
        {
            _Requests.Enqueue(new Request() { EntityId = entity_id, MethodName = method_name, MethodParams = method_params, ReturnId = return_id });
        }

		

		private bool _Connected()
		{
            return _Socket.Connected;
		}

		void Remoting.IResponseQueue.Push(byte cmd, Dictionary<byte, byte[]> args)
		{
			_Responses.Enqueue(new Regulus.Remoting.Package() { Code = cmd, Args = Regulus.Utility.Map<byte, byte[]>.ToMap(args) });
		}

        event Action<Guid, string, Guid, object[]> _InvokeMethodEvent;
		event Action<Guid, string, Guid, object[]> Remoting.IRequestQueue.InvokeMethodEvent
		{
			add
			{
                _InvokeMethodEvent += value;
			}
			remove
			{
                _InvokeMethodEvent -= value;
			}
		}

        event Action _BreakEvent;
		event Action Remoting.IRequestQueue.BreakEvent
		{
            add { _BreakEvent += value; }
            remove { _BreakEvent -= value; }
		}

		void IRequestQueue.Update()
		{

		}

        internal void Disconnect()
        {
            _BreakEvent();
        }

        public ISoulBinder Binder { get { return _SoulProvider; } }

        bool Utility.IUpdatable.Update()
        {
            if (_Connected())
            {
                _SoulProvider.Update();

                _ReadMachine.Update();
                _WriteMachine.Update();

                if (_Requests.Count > 0)
                {
                    var request = _Requests.Dequeue();
                    _InvokeMethodEvent(request.EntityId, request.MethodName, request.ReturnId, request.MethodParams);
                }

                return true;
            }
            return false;
        }

        void Framework.ILaunched.Launch()
        {
            _HandleWrite();
            _HandleRead();
        }

        void Framework.ILaunched.Shutdown()
        {
            _Socket.Close();
        }
    }
	
}
