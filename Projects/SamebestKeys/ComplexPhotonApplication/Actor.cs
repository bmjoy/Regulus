﻿using System;

namespace Regulus.Project.SamebestKeys
{
    class Actor : Entity
	{
		// Entity屬性
        Serializable.EntityPropertyInfomation _Property;
		// Entity外表資訊
        Serializable.EntityLookInfomation _Look;
		// 身體寬度
        float _BodyWidth;
		// 身體高度
        float _BodyHeight;
        
        public ActionStatue CurrentAction { get; protected set; }        

		public Actor(Serializable.EntityPropertyInfomation property , Serializable.EntityLookInfomation look)
            : base(property.Id)
		{
            _Property = property;
            _Look = look;
            _BodyWidth = 1;
            _BodyHeight = 1;
		}

		/// <summary>
		/// 移動功能
		/// </summary>
        private ActorMoverAbility2 _MoverAbility;

        PhysicalAbility _QuadTreeObjectAbility;
        public Action<Serializable.MoveInfomation> ShowActionEvent;

		/// <summary>
		/// 設定功能
		/// </summary>
		/// <param name="abilitys">現有功能Dict</param>
        protected override void _SetAbility(Entity.AbilitySet abilitys)
        {
            _MoverAbility = new ActorMoverAbility2(_Property.Direction, _Property.Position.X, _Property.Position.Y);
            _MoverAbility.ActionEvent += _OnAction;
            _MoverAbility.PositionEvent += _OnPosition;
            
            abilitys.AttechAbility<IMoverAbility>(_MoverAbility);

            _QuadTreeObjectAbility = new PhysicalAbility(new Regulus.Types.Rect(_Property.Position.X - _BodyWidth / 2, _Property.Position.Y - _BodyHeight / 2, _BodyWidth, _BodyHeight), this);
            abilitys.AttechAbility<PhysicalAbility>(_QuadTreeObjectAbility);
        }

		/// <summary>
		/// On位置改變
		/// </summary>
		/// <param name="time">目前時間Ticks</param>
		/// <param name="unit_vector">單位時間移動向量</param>
        private void _OnPosition(long time, Types.Vector2 unit_vector)
        {
            _Property.Position = Types.Vector2.FromPoint(unit_vector.X + _Property.Position.X, unit_vector.Y + _Property.Position.Y);            
            _MoverAbility.SetPosition(_Property.Position.X, _Property.Position.Y);
            _QuadTreeObjectAbility.UpdateBounds(_Property.Position.X - _BodyWidth / 2, _Property.Position.Y - _BodyHeight / 2);
        }

        protected override void _RiseAbility(Entity.AbilitySet abilitys)
        {
            abilitys.DetechAbility<IMoverAbility>();
            abilitys.DetechAbility<PhysicalAbility>();
        }

		// 方向
        public float Direction { get { return _Property.Direction; } }

        void _OnAction(long begin_time, float move_speed,float direction, Regulus.Types.Vector2 unit_vector, ActionStatue action_state)
        {
            _Property.Direction = direction;
            
            Serializable.MoveInfomation mi = new Serializable.MoveInfomation();
            mi.ActionStatue = action_state;
            mi.MoveDirectionAngle = direction;
            CurrentAction = action_state;

            mi.BeginPosition = _Property.Position;
            mi.BeginTime = begin_time;
            mi.MoveDirection = unit_vector;
            mi.Speed = move_speed;

            if (ShowActionEvent != null)
                ShowActionEvent(mi);
        }

		/// <summary>
		/// 設定位置
		/// </summary>
        public void SetPosition(float x, float y)
        {
            _Property.Position = Types.Vector2.FromPoint(x,y);            
            _MoverAbility.SetPosition(_Property.Position.X, _Property.Position.Y);
            _QuadTreeObjectAbility.UpdateBounds(_Property.Position.X - _BodyWidth / 2, _Property.Position.Y - _BodyHeight / 2);
        }
    }
}