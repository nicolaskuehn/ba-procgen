using System.Collections.Generic;

namespace ProcGen.Generation
{
    public class HeightfieldCompositor
    {
        public List<Octave> Octaves { get; private set; }

        public void AddOctave(Octave octave)
        {
            Octaves.Add(octave);
        }

        public void RemoveOctave(Octave octave)
        {
            Octaves.Remove(octave);
        }

        public float GetComposedHeight(float x, float z)
        {
            float composedHeight = 0.0f;

            // Combine heights of the height generators of all octaves
            foreach (Octave octave in Octaves)
            {
                composedHeight += octave.HeightfieldGenerator.GetHeight(x, z);
            }

            return composedHeight;
        }

        public HeightfieldCompositor()
        {
            // Initialize list of octaves (empty)
            Octaves = new List<Octave>();
        }
    }
}
