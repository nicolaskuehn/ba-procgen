using System;

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
                    this.name = $"Setting #{this.GetHashCode().ToString()}";
                else
                    name = value;
            }
        }

        public object Value { get; set; }

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
