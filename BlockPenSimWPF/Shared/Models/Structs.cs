namespace BlockPenSimWPF.Shared.Models
{
    public struct Shape
    {
        public double smallest;
        public double middle;
        public double largest;
    }

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
        public double impulse;
    }
    public struct MinMax
    {
        private double min;
        private double max;
        public double Min { get => min; set { min = value; max = Math.Max(value, max); } }
        public double Max { get => max; set { max = value; min = Math.Min(value, min); } }
    }

    public struct Block
    {
        public Block(Shape shape, Orientation orientation, Material material)
        {
            this.material = material;
            switch (orientation)
            {
                default:
                case Orientation.ForwardsTall:
                    this.height = shape.largest;
                    this.width = shape.middle;
                    this.length = shape.smallest;
                    break;
                case Orientation.ForwardsWide:
                    this.width = shape.largest;
                    this.height = shape.middle;
                    this.length = shape.smallest;
                    break;
                case Orientation.SidewaysTall:
                    this.height = shape.largest;
                    this.length = shape.middle;
                    this.width = shape.smallest;
                    break;
                case Orientation.SidewaysLong:
                    this.length = shape.largest;
                    this.height = shape.middle;
                    this.width = shape.smallest;
                    break;
                case Orientation.FlatLong:
                    this.length = shape.largest;
                    this.width = shape.middle;
                    this.height = shape.smallest;
                    break;
                case Orientation.FlatWide:
                    this.width = shape.largest;
                    this.length = shape.middle;
                    this.height = shape.smallest;
                    break;
            }
        }

        public readonly double length;
        public readonly double width;
        public readonly double height;
        public readonly Material material;
        public readonly double PerimeterFront { get => (width + height) * 2.0; }
        public readonly double PerimeterSide { get => (length + height) * 2.0; }
        public readonly double PerimeterTop { get => (length + width) * 2.0; }
        public readonly double AreaFront { get => width * height; }
        public readonly double AreaSide { get => length * height; }
        public readonly double AreaTop { get => length * width; }
        public readonly double HpFront { get => PerimeterFront * material.connectionStrength; }
        public readonly double HpSide { get => PerimeterSide * material.connectionStrength; }
        public readonly double HpTop { get => PerimeterTop * material.connectionStrength; }
        public readonly double EnergyAbsFront { get => material.energyAbsorption / 5.0 * length; }
        public readonly double EnergyAbsSide { get => material.energyAbsorption / 5.0 * width; }
        public readonly double EnergyAbsTop { get => material.energyAbsorption / 5.0 * height; }
        public readonly double Volume { get => length * width * height; }
        public readonly double Weight { get => Volume * material.density; }
    }

    public struct SimBlock
    {
        public SimBlock(Block block)
        {
            this.hpFront = block.HpFront;
            this.hpSide = block.HpSide;
            this.hpTop = block.HpTop;
        }
        public double hpFront;
        public double hpSide;
        public double hpTop;
        public readonly bool IsDead { get => hpFront <= 0.0 && hpSide <= 0.0 && hpTop <= 0.0; }
    }

    public struct WeaponSettings
    {
        public double WeaponCount;
        public double WeaponRatio;
        public double WeaponFrontRatio;
        public double WeaponSideRatio;
        public double WeaponTopRatio;
    }
}
