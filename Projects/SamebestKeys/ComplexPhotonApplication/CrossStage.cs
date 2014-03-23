﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Regulus.Project.SamebestKeys
{
    class CrossStage : Regulus.Game.IStage<User>, ITraversable
    {
        private Regulus.Project.SamebestKeys.IWorld _World;
        private string _TargetMap;
        private Regulus.Types.Vector2 _TargetPosition;
        private string _CurrentMap;
        private Regulus.Types.Vector2 _CurrentPosition;

        public delegate void OnResult(string target_map,Regulus.Types.Vector2 target_position );
        public event OnResult ResultEvent;
        private Remoting.ISoulBinder _Provider;
        

        public CrossStage(Remoting.ISoulBinder provider ,Regulus.Project.SamebestKeys.IWorld _World, string target_map, Regulus.Types.Vector2 target_position, string current_map, Regulus.Types.Vector2 current_position)
        {
            // TODO: Complete member initialization
            this._World = _World;
            this._TargetMap = target_map;
            this._TargetPosition = target_position;
            this._CurrentMap = current_map;
            this._CurrentPosition = current_position;
            _Provider = provider;
        }

        

        Game.StageLock Game.IStage<User>.Enter(User obj)
        {
            _Provider.Bind<ITraversable>(this);
            return null;
        }

        

        void Game.IStage<User>.Leave(User obj)
        {
            _Provider.Unbind<ITraversable>(this);
        }

        void Game.IStage<User>.Update(User obj)
        {
            
        }

        Remoting.Value<Serializable.CrossStatus> ITraversable.GetStatus()
        {
            return new Serializable.CrossStatus() { SourceMap = _CurrentMap, SourcePosition = _CurrentPosition, TargetMap = _TargetMap, TargetPosition = _TargetPosition };
        }

        void ITraversable.Ready()
        {
            var mapValue = _World.Find(_TargetMap);
            mapValue.OnValue += (map) =>
            {
                if (map == null)
                {
                    ResultEvent(_CurrentMap, _CurrentPosition);                    
                }
                else
                {
                    ResultEvent(_TargetMap, _TargetPosition);
                    
                }

            };
        }
    }
}
