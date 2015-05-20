﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Regulus.Extension;
using System.Threading.Tasks;
namespace VGame.Project.FishHunter.Storage
{



    class Service
    {
        public IAccountManager AccountManager { get; private set; }
        public IAccountFinder AccountFinder { get; private set; }
        System.Threading.Tasks.Task _ProxyUpdate;
        VGame.Project.FishHunter.Storage.Proxy _Proxy;
        volatile bool _Enable;
        Regulus.CustomType.Flag<VGame.Project.FishHunter.Data.Account.COMPETENCE> _Competnces;
        public bool Enable {  get {return _Enable;}}
        
        Regulus.Utility.SpinWait _Soin;
        private VGameWebApplication.Models.VerifyData data;
        private IUser _User;

        public Service(VGameWebApplication.Models.VerifyData data)
        {
            this.data = data;
            _Soin = new Regulus.Utility.SpinWait();
            _Enable = true;
            _Proxy = new VGame.Project.FishHunter.Storage.Proxy();
            _ProxyUpdate = new System.Threading.Tasks.Task(_UpdateProxy);
            _ProxyUpdate.Start();
            _User = _Proxy.SpawnUser("1");
            
        }
        ~Service()
        {
            Release();
        }

        private void _GetStorageCompetnces()
        {
            var provider = _User.QueryProvider<IStorageCompetnces>();
            while (provider.Ghosts.Length <= 0)
                _Wait();

            _Competnces = new Regulus.CustomType.Flag<Data.Account.COMPETENCE>(provider.Ghosts[0].Query().WaitResult());
        }
        bool _Initial()
        {
            if (_Connect())
            {
                if (_Verify())
                {
                    _GetStorageCompetnces();

                    if (_Competnces[Data.Account.COMPETENCE.ACCOUNT_MANAGER])
                        _GetAccountManager();

                    if (_Competnces[Data.Account.COMPETENCE.ACCOUNT_FINDER])
                        _GetAccountFinder();

                    return true;
                }
                else
                    return false;
            }
            else
            {
                throw new SystemException("storage verify fail.");
            }
        }
        public void Release()
        {
            _Enable = false;
        }
        

        private void _GetAccountFinder()
        {
            var provider = _User.QueryProvider<IAccountFinder>();
            while (provider.Ghosts.Length <= 0)
                _Wait();

            AccountFinder = provider.Ghosts[0];
        }

        private void _GetAccountManager()
        {
            var provider = _User.QueryProvider<IAccountManager>();
            while (provider.Ghosts.Length <= 0)
                _Wait();

            AccountManager = provider.Ghosts[0];
        }


        private bool _Verify()
        {
            while (_User.VerifyProvider.Ghosts.Length <= 0)
                _Wait();

            return _User.VerifyProvider.Ghosts[0].Login(data.Account, data.Password).WaitResult();
        }

        private bool _Connect()
        {
            while (_User.Remoting.ConnectProvider.Ghosts.Length <= 0)
                _Wait();

            return _User.Remoting.ConnectProvider.Ghosts[0].Connect("127.0.0.1", 38973).WaitResult();
        }

        private void _Wait()
        {
            _Soin.SpinOnce();
        }

        

        

        private void _UpdateProxy()
        {
            Regulus.Utility.SpinWait spin = new Regulus.Utility.SpinWait();
            Regulus.Utility.CenterOfUpdateable updater = new Regulus.Utility.CenterOfUpdateable();
            updater.Add(_Proxy);
            while (_Enable)
            {
                if (Regulus.Utility.Random.NextFloat(0, 1) <= 0.1f) 
                    spin.SpinOnce();
                else
                    spin.Reset();
                updater.Working();
            }
            updater.Shutdown();
        }

        internal static Guid Verify(string user, string password)
        {
            
            var service = new Service(new VGameWebApplication.Models.VerifyData { Account = user, Password = password });
            if(service._Initial())
            {
                service.Release();
                return VGame.Project.FishHunter.Storage.KeyPool.Instance.Query(user, password);
            }
                
            return Guid.Empty;
        }

        internal static Service Create(Guid id)
        {
            var data = VGame.Project.FishHunter.Storage.KeyPool.Instance.Find(id);
            if(data != null)
            {
                var service = new Service(data);
                if(service._Initial())
                    return service;
            }
            return null;
        }

        internal static void Destroy(Guid guid)
        {
            VGame.Project.FishHunter.Storage.KeyPool.Instance.Destroy(guid);
        }

        internal static Service Create(object p)
        {
            return Create((Guid)p);
        }
    }
}
