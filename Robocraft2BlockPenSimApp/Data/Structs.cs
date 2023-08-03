using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robocraft2BlockPenSimApp.Data
{
    internal static class Structs
    {
        public struct Material
        {
            public string name;
            public double density;
            public double connectionStrength;
            public double energyAbsorption;
        }

        public struct Weapon
        {
            public string name;
            public int cpu;
            public double damage;
            public double pellets;
            public double radius;
            public double energy;
            public double cooldown;
        }
        public struct MinMax
        {
            private double min;
            private double max;
            public double Min { get => min; set { min = value; max = Math.Max(value, max); } }
            public double Max { get => max; set { max = value; min = Math.Min(value, min); } }
        }
    }
}
