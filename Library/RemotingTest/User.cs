﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemotingTest
{
    class User : IUser
    {
        Regulus.Utility.Updater _Updater;
        Regulus.Remoting.User _User;

        private Regulus.Remoting.IAgent _Agent;

        public User(Regulus.Standalong.Agent agent)
        {
            this._Agent = agent;
            _Updater = new Regulus.Utility.Updater();
            _User = new Regulus.Remoting.User(_Agent);
        }
        bool Regulus.Utility.IUpdatable.Update()
        {
            _Updater.Update();
            return true;
        }

        void Regulus.Framework.ILaunched.Launch()
        {
            _Updater.Add(_User);
        }

        void Regulus.Framework.ILaunched.Shutdown()
        {
            _Updater.Shutdown();
        }

        Regulus.Remoting.User IUser.Remoting
        {
            get { return _User; }
        }


        


        Regulus.Remoting.Ghost.IProviderNotice<ITestReturn> IUser.TestReturnProvider
        {
            get { return _Agent.QueryProvider<ITestReturn>(); }
        }
    }
}