﻿namespace Regulus.Remoting
{
    using System.Linq;

    public enum SocketIOResult
    {
        None, Done, Break
    }
    public partial class NetworkStreamWriteStage : Regulus.Game.IStage
    {
        
        class WrittingStage : Regulus.Game.IStage
        {
            System.Net.Sockets.Socket _Socket;
            System.IAsyncResult _AsyncResult;
            byte[] _Buffer;
            public event System.Action<SocketIOResult> DoneEvent;            
            SocketIOResult _Result;

            public WrittingStage(System.Net.Sockets.Socket socket, byte[] buffer)
            {                
                
                this._Socket = socket;                
                _Buffer = buffer;
                
            }
            void Game.IStage.Enter()            
            {
                try
                {
                    _Result = SocketIOResult.None;
                    _AsyncResult = _Socket.BeginSend(_Buffer, 0, _Buffer.Length, 0, _WriteCompletion, null);
                }
                catch (System.Net.Sockets.SocketException ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString() + ex.ErrorCode);
                    _Result = SocketIOResult.Break;
                }
                catch
                {
                    _Result = SocketIOResult.Break;
                }
                    
            }

            void Game.IStage.Leave()
            {
                
            }

            void Game.IStage.Update()
            {
                if (_Result != SocketIOResult.None)
                {
                    DoneEvent(_Result);
                }
            }

            private void _WriteCompletion(System.IAsyncResult ar)
            {
                try                
                {
                    _Socket.EndSend(ar);
                    _Result = SocketIOResult.Done;
                }
                catch (System.Net.Sockets.SocketException ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString() + ex.ErrorCode);
                    _Result = SocketIOResult.Break;
                }                
                catch
                {
                    _Result = SocketIOResult.Break;
                }
            }
        }
    }

	public partial class NetworkStreamWriteStage : Regulus.Game.IStage
	{
		System.Net.Sockets.Socket _Socket;
        Package[] _Packages;
        
        public event System.Action WriteCompletionEvent;
        public event System.Action ErrorEvent;
        Regulus.Game.StageMachine _Machine;

        
        public static int TotalBytes { get; private set; }
        static Regulus.Utility.TimeCounter _AfterTime = new Utility.TimeCounter();
        public static double TotalBytesPerSecond { get { return TotalBytes / _AfterTime.Second; } }
        const int _HeadSize = 4;
        public NetworkStreamWriteStage(System.Net.Sockets.Socket socket, Package[] packages)
		{
            _Socket = socket;
            _Packages = packages;
            _Machine = new Game.StageMachine();
            
		}
		void Game.IStage.Enter()
		{            
            var packages = _Packages;
            var buffer = _CreateBuffer(packages);
            _ToWrite(buffer);
		}

        private void _ToWrite(byte[] buffer)
        {
            var stage = new WrittingStage(_Socket, buffer);
            stage.DoneEvent += (result) =>
            {
                if (result == SocketIOResult.Done)
                {
                    _Done(buffer.Length);
                }
                else
                    ErrorEvent();
            };

            _Machine.Push(stage);
        }

        byte[] _CreateBuffer(Package[] packages)
        {
            
            var buffers = from p in packages select Regulus.PhotonExtension.TypeHelper.Serializer<Package>(p);            

            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            { 
                foreach(var buffer in buffers)
                {
                    stream.Write(System.BitConverter.GetBytes((int)buffer.Length) ,0 , _HeadSize );
                    stream.Write(buffer, 0, buffer.Length);
                }
                return stream.ToArray();
            }
        }

        private void _ToHead(byte[] buffer)
        {
            
            var header = System.BitConverter.GetBytes((int)buffer.Length);            
            var stage = new WrittingStage(_Socket, header);
            stage.DoneEvent += (result) => 
            {
                if (result == SocketIOResult.Done)
                    _ToBody(buffer);
                else
                    ErrorEvent();
            };
            
            _Machine.Push(stage);
        }

        private void _ToBody(byte[] buffer)
        {            
            var stage = new WrittingStage(_Socket, buffer);
            stage.DoneEvent += (result) => 
            {
                if (result == SocketIOResult.Done)
                {
                    _Done(buffer.Length + _HeadSize);
                }
                else
                    ErrorEvent();
            };
            
            _Machine.Push(stage);
        }

        private void _Done(int size)
        {
            WriteCompletionEvent();
            TotalBytes += size;            
        }

		void Game.IStage.Leave()
		{
            
		}

		void Game.IStage.Update()
		{
            
            _Machine.Update();
            
            
		}
	}


    public partial class NetworkStreamReadStage : Regulus.Game.IStage
    {
        
        class ReadingStage : Regulus.Game.IStage
        {
            public event System.Action<byte[]> DoneEvent;            
            private System.Net.Sockets.Socket _Socket;
            private int _Size;
            int _Offset;
            byte[] _Buffer;
            SocketIOResult _Result;
            public ReadingStage(System.Net.Sockets.Socket socket, int size)
            {                
                this._Socket = socket;
                
                this._Size = size;
            }


            void Game.IStage.Enter()
            {

                try
                {
                    _Buffer = new byte[_Size];                
                    _Result = SocketIOResult.None;
                    _Socket.BeginReceive(_Buffer, _Offset, _Buffer.Length - _Offset, 0, _Readed, null);
                }
                catch 
                {
                    _Result = SocketIOResult.Break;
                }
                
            }

            void Game.IStage.Leave()
            {
                
            }

            void Game.IStage.Update()
            {
                if (_Result == SocketIOResult.Done)
                { 
                    DoneEvent(_Buffer);                    
                }
                else if (_Result == SocketIOResult.Break)
                    DoneEvent(null);                    
            }

            private void _Readed(System.IAsyncResult ar)
            {
                try
                {
                    _Offset += _Socket.EndReceive(ar);
                    if (_Offset == _Size)
                    {
                        _Result = SocketIOResult.Done;
                    }
                    else
                    {
                        _Result = SocketIOResult.None;
                        _Socket.BeginReceive(_Buffer, _Offset, _Buffer.Length - _Offset, 0, _Readed, null);
                    }                
                }
                catch 
                {
                    _Result = SocketIOResult.Break; 
                }
                
            }

        }
    }
    
	public partial class NetworkStreamReadStage : Regulus.Game.IStage
	{
        public delegate void OnReadCompletion(Package package);
        public event OnReadCompletion ReadCompletionEvent;
        public event System.Action ErrorEvent;       
		System.Net.Sockets.Socket _Socket;
        Regulus.Game.StageMachine _Machine;
        Regulus.Utility.TimeCounter _LifeCycle;
        public static int TotalBytes { get; private set; }        
        static Regulus.Utility.TimeCounter _AfterTime = new Utility.TimeCounter();
        public static double TotalBytesPerSecond { get { return TotalBytes / _AfterTime.Second; } }
        const int _HeadSize = 4;
        public NetworkStreamReadStage(System.Net.Sockets.Socket socket )
		{            
			_Socket = socket;
            _Machine = new Game.StageMachine();
            _LifeCycle = new Utility.TimeCounter();
		}
		void Game.IStage.Enter()
		{
            _LifeCycle.Reset();
            _ToHead();
		}

        private void _ToHead()
        {
            var stage = new ReadingStage(_Socket, _HeadSize);
            stage.DoneEvent += (buffer)=>
            {
                if (buffer != null)
                    _ToBody(buffer);
                else
                    ErrorEvent();
            };
            
            _Machine.Push(stage);
        }
        private void _ToBody(byte[] head)
        {                       
            var bodySize = System.BitConverter.ToInt32(head, 0);
            var stage = new ReadingStage(_Socket, bodySize);
            stage.DoneEvent += (buffer)=>
            {
                if (buffer != null)
                    _Done(buffer);
                else
                    ErrorEvent();
            };
            
            _Machine.Push(stage);
        }
        private void _Done(byte[] body)
        {
            ReadCompletionEvent(Regulus.PhotonExtension.TypeHelper.Deserialize<Package>(body) );
            TotalBytes += (body.Length + _HeadSize);
            
        }

		void Game.IStage.Leave()
		{
            
		}
        
		void Game.IStage.Update()
		{            
            _Machine.Update();
		}
	}

    public partial class WaitQueueStage : Regulus.Game.IStage
    {


        public event System.Action DoneEvent;
        private System.Collections.Generic.Queue<Package> _Packages;

        public WaitQueueStage(System.Collections.Generic.Queue<Package> packages)
        {            
            this._Packages = packages;
        }
        void Game.IStage.Enter()
        {
            
        }

        void Game.IStage.Leave()
        {
            
        }

        void Game.IStage.Update()
        {
            if (_Packages.Count > 0)
            {
                DoneEvent();
            }
        }
    }
}