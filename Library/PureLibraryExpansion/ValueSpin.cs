﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Regulus.Remoting.Native.Soul
{
    class ValueSpin<T>
    {
        private Value<T> value;
        volatile bool _HasValue;
        T _Value;

        public T Value { get { return _Value; } }
        public ValueSpin(Value<T> value)
        {
            // TODO: Complete member initialization
            this.value = value;
            _HasValue = false;

        }

        internal T Wait()
        {
            value.OnValue += _Getted;
            
            var sw = new Regulus.Utility.SpinWait();
            while (_HasValue == false)
                sw.SpinOnce();


            return _Value;
        }

        internal void Run(object obj)
        {
            value.OnValue += _Getted;


            var sw = new Regulus.Utility.SpinWait();
            while (_HasValue == false)
                sw.SpinOnce();


            
        }

        private void _Getted(T obj)
        {
            _Value = obj;
            _HasValue = true;            
        }
    }
}
