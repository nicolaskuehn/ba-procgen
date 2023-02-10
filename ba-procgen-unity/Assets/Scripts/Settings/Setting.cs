using ProcGen.Generation;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace ProcGen.Settings
{
    public class Setting
    {
        // ... Attributes ... //
        private string name;
        public string Name
        {
            get => name;
            set
            {
                if (value == null)
                    name = $"Setting #{GetHashCode()}";
                else
                    name = value;
            }
        }

        public UnityEvent valueChanged = new UnityEvent();
        private object _value;
        public object Value 
        {
            get => _value;
            set
            {
                if (_value != null && !_value.Equals(value))
                    valueChanged.Invoke();
                
                _value = value;
            } 
        }

        public Type Type
        {
            get => Value.GetType();
        }

        // ... Constructors ... //
        public Setting(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }
}
