﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Regulus.Project.TurnBasedRPG
{
    class QuadTreeObjectAbility : Regulus.Physics.IQuadObject
    {
        System.Windows.Rect _Bounds;
        Entity _Owner;
        public QuadTreeObjectAbility(System.Windows.Rect bounds ,Entity owner)
        {            
            _Bounds = bounds;
            _Owner = owner;
        }

        public IMoverAbility MoverAbility { get { return _Owner.FindAbility<IMoverAbility>(); } }
        public IObservedAbility ObservedAbility { get { return _Owner.FindAbility<IObservedAbility>(); } }
        public System.Windows.Rect Bounds
        {
            get { return _Bounds; }
        }

        public event EventHandler BoundsChanged;
        public void UpdateBounds(Types.Vector2 vector2)
        {
            _UpdateBounds(vector2, ref _Bounds);
        }
        private void _UpdateBounds(Types.Vector2 vector2, ref System.Windows.Rect bounds)
		{
			bounds.Offset(vector2.X, vector2.Y);
			if (BoundsChanged != null)
				BoundsChanged(this, new EventArgs());
		}        

    }
}