﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Regulus.Utility
{
	public class Command : ICommand
	{
		public delegate void OnRegister(string command, CommandParameter ret, CommandParameter[] args);

		public delegate void OnUnregister(string command);

		public event OnRegister RegisterEvent;

		public event OnUnregister UnregisterEvent;

		private class Infomation
		{
			public Action<string[]> Handler;

			public string Name;
		}

		public class CommandParameter
		{
			public Type Param { get; private set; }

			public string Description { get; private set; }

			public CommandParameter(Type p, string description)
			{
				Param = p;
				Description = description;
			}

			public static implicit operator CommandParameter(Type type)
			{
				return new CommandParameter(type, string.Empty);
			}
		}

		public class Analysis
		{
			public string Command { get; private set; }

			public string[] Parameters { get; private set; }

			public Analysis(string message)
			{
				_Analyze(message);
			}

			private void _Analyze(string message)
			{
				var expansion = @"^\s*(?<command>\w+)\s*\[\s*(?<args>.+?)\]|^\s*(?<command>\w+)\s*";
				var regex = new Regex(expansion);
				var match = regex.Match(message);
				if(match.Success)
				{
					var command = match.Groups["command"];
					Command = command.Value;
					var args = match.Groups["args"];
					_SetParameters(_AnalyzeArgs(args.Value));
				}
			}

			private void _SetParameters(string[] parameters)
			{
				Parameters = parameters;
			}

			private string[] _AnalyzeArgs(string message)
			{
				var args = new List<string>();

				// \s*(\w+)\s*,?
				// ^\s*(?<command>\w+)\s*\[\s*(?<args>.+)\]|^\s*(?<command>\w+)\s*
				const string expansion = @"\s*(?<Arg>\w+)\s*,?";
				var regex = new Regex(expansion);
				var matchs = regex.Matches(message);
				foreach(Match match in  matchs)
				{
					args.Add(match.Groups["Arg"].Value);
				}

				return args.ToArray();
			}
		}

		private readonly List<Infomation> _Commands;

		public Command()
		{
			_Commands = new List<Infomation>();
			RegisterEvent += _EmptyRegisterEvent;
			UnregisterEvent += s => { };
		}

		public void Register(string command, Action executer)
		{
			Action<string[]> func = args =>
			{
				if(args.Length != 0)
				{
					throw new ArgumentException("命令參數數量為0");
				}

				executer.Invoke();
			};

			var analysis = new Analysis(command);
			_AddCommand(analysis.Command, func);
			_RegisterEvent(analysis, null, new Type[0]);
		}

		public void Register<T1>(string command, Action<T1> executer)
		{
			Action<string[]> func = args =>
			{
				if(args.Length != 1)
				{
					throw new ArgumentException("命令參數數量為1");
				}

				object arg0;
				Command._Cnv(args[0], out arg0, typeof(T1));
				executer.Invoke((T1)arg0);
			};

			var analysis = new Analysis(command);
			_AddCommand(analysis.Command, func);
			_RegisterEvent(
				analysis, 
				null, 
				new[]
				{
					typeof(T1)
				});
		}

		public void Register<T1, T2>(string command, Action<T1, T2> executer)
		{
			Action<string[]> func = args =>
			{
				if(args.Length != 2)
				{
					throw new ArgumentException("命令參數數量為2");
				}

				object arg0;
				Command._Cnv(args[0], out arg0, typeof(T1));
				object arg1;
				Command._Cnv(args[1], out arg1, typeof(T2));
				executer.Invoke((T1)arg0, (T2)arg1);
			};

			var analysis = new Analysis(command);
			_AddCommand(analysis.Command, func);
			_RegisterEvent(
				analysis, 
				null, 
				new[]
				{
					typeof(T1), 
					typeof(T2)
				});
		}

		public void Register<T1, T2, T3>(string command, Action<T1, T2, T3> executer)
		{
			Action<string[]> func = args =>
			{
				if(args.Length != 3)
				{
					throw new ArgumentException("命令參數數量為3");
				}

				object arg0;
				Command._Cnv(args[0], out arg0, typeof(T1));
				object arg1;
				Command._Cnv(args[1], out arg1, typeof(T2));
				object arg2;
				Command._Cnv(args[2], out arg2, typeof(T3));
				executer.Invoke((T1)arg0, (T2)arg1, (T3)arg2);
			};

			var analysis = new Analysis(command);
			_AddCommand(analysis.Command, func);
			_RegisterEvent(
				analysis, 
				null, 
				new[]
				{
					typeof(T1), 
					typeof(T2), 
					typeof(T3)
				});
		}

		public void Register<T1, T2, T3, T4>(string command, Action<T1, T2, T3, T4> executer)
		{
			Action<string[]> func = args =>
			{
				if(args.Length != 4)
				{
					throw new ArgumentException("命令參數數量為4");
				}

				object arg0;
				Command._Cnv(args[0], out arg0, typeof(T1));
				object arg1;
				Command._Cnv(args[1], out arg1, typeof(T2));
				object arg2;
				Command._Cnv(args[2], out arg2, typeof(T3));
				object arg3;
				Command._Cnv(args[3], out arg3, typeof(T4));
				executer.Invoke((T1)arg0, (T2)arg1, (T3)arg2, (T4)arg3);
			};

			var analysis = new Analysis(command);
			_AddCommand(analysis.Command, func);
			_RegisterEvent(
				analysis, 
				null, 
				new[]
				{
					typeof(T1), 
					typeof(T2), 
					typeof(T3), 
					typeof(T4)
				});
		}

		public void Register<TR>(string command, Func<TR> executer, Action<TR> value)
		{
			Action<string[]> func = args =>
			{
				if(args.Length != 0)
				{
					throw new ArgumentException("命令參數數量為0");
				}

				var ret = executer.Invoke();
				if(ret != null && value != null)
				{
					value(ret);
				}
			};

			var analysis = new Analysis(command);
			_AddCommand(analysis.Command, func);
			_RegisterEvent(analysis, typeof(TR), new Type[0]);
		}

		public void Register<T1, TR>(string command, Func<T1, TR> executer, Action<TR> value)
		{
			Action<string[]> func = args =>
			{
				if(args.Length != 1)
				{
					throw new ArgumentException("命令參數數量為1");
				}

				object arg0;
				Command._Cnv(args[0], out arg0, typeof(T1));
				var ret = executer.Invoke((T1)arg0);
				if(ret != null && value != null)
				{
					value(ret);
				}
			};

			var analysis = new Analysis(command);
			_AddCommand(analysis.Command, func);
			_RegisterEvent(
				analysis, 
				typeof(TR), 
				new[]
				{
					typeof(T1)
				});
		}

		public void Register<T1, T2, TR>(string command, Func<T1, T2, TR> executer, Action<TR> value)
		{
			Action<string[]> func = args =>
			{
				if(args.Length != 2)
				{
					throw new ArgumentException("命令參數數量為2");
				}

				object arg0;
				Command._Cnv(args[0], out arg0, typeof(T1));
				object arg1;
				Command._Cnv(args[1], out arg1, typeof(T2));

				var ret = executer.Invoke((T1)arg0, (T2)arg1);
				if(ret != null && value != null)
				{
					value(ret);
				}
			};

			var analysis = new Analysis(command);
			_AddCommand(analysis.Command, func);
			_RegisterEvent(
				analysis, 
				typeof(TR), 
				new[]
				{
					typeof(T1), 
					typeof(T2)
				});
		}

		public void Register<T1, T2, T3, TR>(string command, Func<T1, T2, T3, TR> executer, Action<TR> value)
		{
			Action<string[]> func = args =>
			{
				if(args.Length != 3)
				{
					throw new ArgumentException("命令參數數量為3");
				}

				object arg0;
				Command._Cnv(args[0], out arg0, typeof(T1));
				object arg1;
				Command._Cnv(args[1], out arg1, typeof(T2));
				object arg2;
				Command._Cnv(args[2], out arg2, typeof(T3));

				var ret = executer.Invoke((T1)arg0, (T2)arg1, (T3)arg2);
				if(ret != null && value != null)
				{
					value(ret);
				}
			};

			var analysis = new Analysis(command);
			_AddCommand(analysis.Command, func);
			_RegisterEvent(
				analysis, 
				typeof(TR), 
				new[]
				{
					typeof(T1), 
					typeof(T2), 
					typeof(T3)
				});
		}

		public void Register<T1, T2, T3, T4, TR>(string command, Func<T1, T2, T3, T4, TR> executer, Action<TR> value)
		{
			Action<string[]> func = args =>
			{
				if(args.Length != 4)
				{
					throw new ArgumentException("命令參數數量為4");
				}

				object arg0;
				Command._Cnv(args[0], out arg0, typeof(T1));
				object arg1;
				Command._Cnv(args[1], out arg1, typeof(T2));
				object arg2;
				Command._Cnv(args[2], out arg2, typeof(T3));
				object arg3;
				Command._Cnv(args[3], out arg3, typeof(T4));

				var ret = executer.Invoke((T1)arg0, (T2)arg1, (T3)arg2, (T4)arg3);
				if(ret != null && value != null)
				{
					value(ret);
				}
			};

			var analysis = new Analysis(command);
			_AddCommand(analysis.Command, func);
			_RegisterEvent(
				analysis, 
				typeof(TR), 
				new[]
				{
					typeof(T1), 
					typeof(T2), 
					typeof(T3), 
					typeof(T4)
				});
		}

		public void Unregister(string command)
		{
			if(_Commands.RemoveAll(cmd => cmd.Name == command) > 0)
			{
				UnregisterEvent(command);
			}
		}

		private static void _Cnv(string p, out object val, Type source)
		{
			val = p;

			if(source == typeof(int))
			{
				var reault = int.MinValue;
				if(int.TryParse(p, out reault))
				{
				}

				val = reault;
			}
			else if(source == typeof(float))
			{
				var reault = float.MinValue;
				if(float.TryParse(p, out reault))
				{
				}

				val = reault;
			}
			else if(source == typeof(byte))
			{
				var reault = byte.MinValue;
				if(byte.TryParse(p, out reault))
				{
				}

				val = reault;
			}
			else if(source == typeof(short))
			{
				var reault = short.MinValue;
				if(short.TryParse(p, out reault))
				{
				}

				val = reault;
			}
			else if(source == typeof(long))
			{
				var reault = long.MinValue;
				if(long.TryParse(p, out reault))
				{
				}

				val = reault;
			}
		}

		private void _EmptyRegisterEvent(string command, CommandParameter ret, CommandParameter[] args)
		{
			// throw new NotImplementedException();
		}

		private void _AddCommand(string command, Action<string[]> func)
		{
			_Commands.Add(
				new Infomation
				{
					Name = command, 
					Handler = func
				});
		}

		public int Run(string command, string[] args)
		{
			var commandInfomations = from ci in _Commands where ci.Name.ToLower() == command.ToLower() select ci;
			var infos = new List<Infomation>();

			foreach(var commandInfomation in commandInfomations)
			{
				infos.Add(commandInfomation);
			}

			foreach(var info in infos)
			{
				info.Handler(args);
			}

			return infos.Count();
		}

		private void _RegisterEvent(Analysis analysis, Type ret, Type[] args)
		{
			if(ret != null)
			{
				var parameterTypes = args.ToArray();
				var parameterDescs = analysis.Parameters.Skip(1).ToArray();
				RegisterEvent(
					analysis.Command, 
					new CommandParameter(
						ret, 
						analysis.Parameters.Length > 0
							? analysis.Parameters[0]
							: string.Empty), 
					_BuildCommandParameters(parameterTypes, parameterDescs));
			}
			else
			{
				var parameterTypes = args.ToArray();
				var parameterDescs = analysis.Parameters.ToArray();
				RegisterEvent(analysis.Command, null, _BuildCommandParameters(parameterTypes, parameterDescs));
			}
		}

		private CommandParameter[] _BuildCommandParameters(Type[] parameterTypes, string[] parameterDescs)
		{
			var count = parameterTypes.Length;
			var cps = new CommandParameter[count];
			for(var i = 0; i < count; ++i)
			{
				cps[i] = new CommandParameter(
					parameterTypes[i], 
					(i < parameterDescs.Length)
						? parameterDescs[i]
						: string.Empty);
			}

			return cps;
		}
	}
}