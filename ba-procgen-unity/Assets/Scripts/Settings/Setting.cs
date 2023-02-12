using System;
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

        public object MinValue { get; private set; }
        public object MaxValue { get; private set; }


        // ... Constructors ... //
        public Setting(string name, object value, object minValue = null, object maxValue = null)
        {
            Name = name;
            Value = value;

            MinValue = minValue;
            MaxValue = maxValue;
        }
    }
}
