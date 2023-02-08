using ProcGen.Settings;
using System;
using System.Collections.Generic;

namespace ProcGen.Generation
{
    public abstract class HeightfieldGenerator
    {
        protected Random randomGenerator;
        private int seed;
        public int Seed 
        { 
            get => seed; 
            set 
            {
                if (seed != value)
                    randomGenerator = new Random(value);
            } 
        }

        public List<Setting> Settings { get; set; }

        public HeightfieldGenerator(int seed = 42)
        {
            Seed = seed;
            
            Settings = new List<Setting>();
        }

        public abstract float GetHeight(float x, float z);
    }
}
