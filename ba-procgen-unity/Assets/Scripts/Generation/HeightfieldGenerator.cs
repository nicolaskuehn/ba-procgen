using ProcGen.Settings;
using System.Collections.Generic;

namespace ProcGen.Generation
{
    public abstract class HeightfieldGenerator
    {
        public List<Setting> Settings { get; }

        public HeightfieldGenerator()
        {
            Settings = new List<Setting>();
        }

        public abstract float GetHeight(int x, int z);
    }
}
