using System.Diagnostics;

namespace ProcGen.Benchmarking
{
    public static class StatisticsManager
    {
        public static Stopwatch TerrainGenerationTimer { get; private set; } = new Stopwatch();
    }

    public static class Statistics
    {
        // The time the generation of the terrain mesh took in milliseconds
        public static long TerrainGenerationTimeMilliseconds
        {
            get => StatisticsManager.TerrainGenerationTimer.ElapsedMilliseconds;
            private set { }
        } 
    }
}
