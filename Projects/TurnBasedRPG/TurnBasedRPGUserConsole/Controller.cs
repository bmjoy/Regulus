﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Regulus.Project.TurnBasedRPGUserConsole
{
    abstract class Controller : Samebest.Game.IFramework
    {
        Samebest.Game.IFramework _User;
        Regulus.Project.TurnBasedRPGUserConsole.CommandHandler _CommandHandler;
        Regulus.Project.TurnBasedRPGUserConsole.CommandBinder _CommandBinder;
        
        void Samebest.Game.IFramework.Launch()
        {
            //Console.Write("請輸入連線位置&Port (127.0.0.1:5055):");
            //var addr = Console.ReadLine();
            var addr = "114.34.90.217:5055";           
            //var addr = "127.0.0.1:5055";
            var user = _GenerateUser(addr);
            user.LinkSuccess += () => { Console.WriteLine("連線成功."); };
            user.LinkFail += () => { Console.WriteLine("連線失敗."); };

            _CommandHandler = new Regulus.Project.TurnBasedRPGUserConsole.CommandHandler();
            _CommandHandler.Initialize();
            _CommandBinder = new Regulus.Project.TurnBasedRPGUserConsole.CommandBinder(_CommandHandler, user);
            _CommandBinder.Setup();

            
            _User = user;
            _User.Launch();
        }

        protected abstract string[] _HandlerInput();
        bool Samebest.Game.IFramework.Update()
        {            
            string[] command = _HandlerInput();
            if (command != null)
                _HandleCommand(_CommandHandler, command);

            return _User.Update();
        }

        void Samebest.Game.IFramework.Shutdown()
        {
            _User.Shutdown();
            _CommandBinder.TearDown();
            _CommandHandler.Finialize();
        }

        private TurnBasedRPG.User _GenerateUser(string addr)
        {
            
            var user = new TurnBasedRPG.User(new Samebest.Remoting.Ghost.Config() { Address = addr, Name = "TurnBasedRPGComplex" });
            return user;
        }
        private void _HandleCommand(CommandHandler command_handler, string[] command)
        {
            if (command.Length > 0)
            {
                var queue = new Queue<string>(command);
                var cmd = queue.Dequeue();
                command_handler.Run(cmd, queue.ToArray());
            }

        }
        
    }

    class BotController : Controller
    {
        Command[] _Commands;
        
        public BotController(string script_path )
        {
            _Commands = _ReadCommand(script_path);
            _Random = new Random(System.DateTime.Now.Millisecond);
        }
        [Samebest.Game.Data.Table("Command")]
        class Command
        {
            [Samebest.Game.Data.Field("Command")]
            public string Content { get; set; }
            [Samebest.Game.Data.Field("Cooldown")]
            public float Cooldown { get; set; }
        }

        private Command[] _ReadCommand(string p)
        {
            Samebest.Game.Data.PrototypeFactory factory = new Samebest.Game.Data.PrototypeFactory();
            factory.LoadCSV("Command", p);
            var cmds = factory.GeneratePrototype<Command>();
            return cmds;
        }

        protected override string[] _HandlerInput()
        {
            Command cmd = _FindCommand();
            if (cmd != null)
            {
                return cmd.Content.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            }
            return null;
        }

        System.DateTime _Expiry;
        Random _Random ;
        private Command _FindCommand()
        {
            var seconds = (System.DateTime.Now - _Expiry);
            if ( seconds.TotalSeconds >= 0)
            {
                var idx = _Random.Next(0,_Commands.Length);
                var cmd = _Commands[idx];
                
                _Expiry = System.DateTime.Now;
                _Expiry = _Expiry.AddSeconds(cmd.Cooldown);
                return cmd;                
            }
            return null;
        }
    }

    class UserController : Controller
    {

        public UserController()
        { 
        }

        Stack<char> _InputData = new Stack<char>();
        protected override string[] _HandlerInput()
        {
            if (Console.KeyAvailable)
            {
                return _HandlerInput(_InputData);
                
            }
            return null;
        }

        private string[] _HandlerInput(Stack<char> chars)
        {
            var keyInfo = Console.ReadKey(true);
            // Ignore if Alt or Ctrl is pressed.
            if ((keyInfo.Modifiers & ConsoleModifiers.Alt) == ConsoleModifiers.Alt)
                return null;
            if ((keyInfo.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control)
                return null;
            // Ignore if KeyChar value is \u0000.
            if (keyInfo.KeyChar == '\u0000')
                return null;
            // Ignore tab key.
            if (keyInfo.Key == ConsoleKey.Tab)
                return null;
            if (keyInfo.Key == ConsoleKey.Escape)
                return null;

            if (keyInfo.Key == ConsoleKey.Backspace && chars.Count() > 0)
            {
                chars.Pop();
                Console.Write("\b \b");
            }
            else if (keyInfo.Key == ConsoleKey.Enter)
            {

                string commands = new string(chars.Reverse().ToArray());
                Samebest.Utility.Singleton<Regulus.Utility.ConsoleLogger>.Instance.Log("Enter Command : " + commands);
                chars.Clear();

                Console.Write("\n");
                return commands.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                chars.Push(keyInfo.KeyChar);
                Console.Write(keyInfo.KeyChar);
            }
            return null;
        }
    }

    
}