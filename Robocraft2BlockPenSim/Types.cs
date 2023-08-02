namespace Robocraft2BlockPenSim
{
    public static class Types
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

        public enum Orientation
        {
            ForwardsTall,
            ForwardsWide,
            SidewaysTall,
            SidewaysLong,
            FlatLong,
            FlatWide
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

        public struct BlockFillConstraints
        {
            public int cpuMin;
            public int cpuMax;
            public double weightMin;
            public double weightMax;
            public double lengthMin;
            public double lengthMax;
            public double widthMin;
            public double widthMax;
            public double heightMin;
            public double heightMax;
        }

        public struct DamageDirectionRatio
        {
            public double front;
            public double side;
            public double top;
            public readonly double Sum { get => front + side + top; }
            public readonly double PercentFront { get => front / Sum; }
            public readonly double PercentSide { get => side / Sum; }
            public readonly double PercentTop { get => top / Sum; }
        }

        public class DamageWeaponCount : Dictionary<string, double> { }

        public class DamageWeaponRatio : Dictionary<string, double>
        {
            public new double this[string key]
            {
                get
                {
                    var sum = base.Values.Sum();
                    return base[key] / sum;
                }
                set
                {
                    base[key] = value;
                }
            }
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

        public struct BlockData
        {
            public Material[] materials;
            public Weapon[] weapons;
        }

        public enum Direction
        {
            Front,
            Side,
            Top
        }


        public enum BlockFillMethod
        {
            LWH,
            LHW,
            WLH,
            WHL,
            HLW,
            HWL,
            ALL
        }

        public class BlockFill
        {
            public BlockFill(Block block, BlockFillConstraints constraints, BlockFillMethod blockFillMethod)
            {
                this.block = block;

                // get minimum size, cpu, weight
                this.lengthCount = this.widthCount = this.heightCount = 1;
                if (this.Length < constraints.lengthMin)
                    this.lengthCount = Math.Ceiling(constraints.lengthMin / block.length);
                if (this.Width < constraints.widthMin)
                    this.widthCount = Math.Ceiling(constraints.widthMin / block.width);
                if (this.heightCount < constraints.heightMin)
                    this.heightCount = Math.Ceiling(constraints.heightMin / block.height);

                if (this.Cpu > constraints.cpuMax || this.Weight > constraints.weightMax)
                    return;

                double allowedSize, allowedCpu, allowedWeight;

                switch (blockFillMethod)
                {
                    default:
                        break;
                    case BlockFillMethod.LWH:
                    case BlockFillMethod.LHW:
                        allowedSize = Math.Floor((constraints.lengthMax - this.Length) / block.length);
                        allowedCpu = Math.Floor((constraints.cpuMax - this.Cpu) / this.widthCount / this.heightCount);
                        allowedWeight = Math.Floor((constraints.weightMax - this.Weight) / this.widthCount / this.heightCount / this.block.Weight);
                        this.lengthCount += Math.Max(0, Math.Min(allowedSize, Math.Min(allowedCpu, allowedWeight)));
                        break;
                    case BlockFillMethod.WHL:
                    case BlockFillMethod.WLH:
                        allowedSize = Math.Floor((constraints.widthMax - this.Width) / block.width);
                        allowedCpu = Math.Floor((constraints.cpuMax - this.Cpu) / this.lengthCount / this.heightCount);
                        allowedWeight = Math.Floor((constraints.weightMax - this.Weight) / this.lengthCount / this.heightCount / this.block.Weight);
                        this.widthCount += Math.Max(0, Math.Min(allowedSize, Math.Min(allowedCpu, allowedWeight)));
                        break;
                    case BlockFillMethod.HWL:
                    case BlockFillMethod.HLW:
                        allowedSize = Math.Floor((constraints.heightMax - this.Height) / block.height);
                        allowedCpu = Math.Floor((constraints.cpuMax - this.Cpu) / this.lengthCount / this.widthCount);
                        allowedWeight = Math.Floor((constraints.weightMax - this.Weight) / this.lengthCount / this.widthCount / this.block.Weight);
                        this.heightCount += Math.Max(0, Math.Min(allowedSize, Math.Min(allowedCpu, allowedWeight)));
                        break;
                }

                switch (blockFillMethod)
                {
                    default:
                        break;
                    case BlockFillMethod.WLH:
                    case BlockFillMethod.HLW:
                        allowedSize = Math.Floor((constraints.lengthMax - this.Length) / block.length);
                        allowedCpu = Math.Floor((constraints.cpuMax - this.Cpu) / this.widthCount / this.heightCount);
                        allowedWeight = Math.Floor((constraints.weightMax - this.Weight) / this.widthCount / this.heightCount / this.block.Weight);
                        this.lengthCount += Math.Max(0, Math.Min(allowedSize, Math.Min(allowedCpu, allowedWeight)));
                        break;
                    case BlockFillMethod.LWH:
                    case BlockFillMethod.HWL:
                        allowedSize = Math.Floor((constraints.widthMax - this.Width) / block.width);
                        allowedCpu = Math.Floor((constraints.cpuMax - this.Cpu) / this.lengthCount / this.heightCount);
                        allowedWeight = Math.Floor((constraints.weightMax - this.Weight) / this.lengthCount / this.heightCount / this.block.Weight);
                        this.widthCount += Math.Max(0, Math.Min(allowedSize, Math.Min(allowedCpu, allowedWeight)));
                        break;
                    case BlockFillMethod.WHL:
                    case BlockFillMethod.LHW:
                        allowedSize = Math.Floor((constraints.heightMax - this.Height) / block.height);
                        allowedCpu = Math.Floor((constraints.cpuMax - this.Cpu) / this.lengthCount / this.widthCount);
                        allowedWeight = Math.Floor((constraints.weightMax - this.Weight) / this.lengthCount / this.widthCount / this.block.Weight);
                        this.heightCount += Math.Max(0, Math.Min(allowedSize, Math.Min(allowedCpu, allowedWeight)));
                        break;
                }

                switch (blockFillMethod)
                {
                    default:
                        break;
                    case BlockFillMethod.HWL:
                    case BlockFillMethod.WHL:
                        allowedSize = Math.Floor((constraints.lengthMax - this.Length) / block.length);
                        allowedCpu = Math.Floor((constraints.cpuMax - this.Cpu) / this.widthCount / this.heightCount);
                        allowedWeight = Math.Floor((constraints.weightMax - this.Weight) / this.widthCount / this.heightCount / this.block.Weight);
                        this.lengthCount += Math.Max(0, Math.Min(allowedSize, Math.Min(allowedCpu, allowedWeight)));
                        break;
                    case BlockFillMethod.HLW:
                    case BlockFillMethod.LHW:
                        allowedSize = Math.Floor((constraints.widthMax - this.Width) / block.width);
                        allowedCpu = Math.Floor((constraints.cpuMax - this.Cpu) / this.lengthCount / this.heightCount);
                        allowedWeight = Math.Floor((constraints.weightMax - this.Weight) / this.lengthCount / this.heightCount / this.block.Weight);
                        this.widthCount += Math.Max(0, Math.Min(allowedSize, Math.Min(allowedCpu, allowedWeight)));
                        break;
                    case BlockFillMethod.LWH:
                    case BlockFillMethod.WLH:
                        allowedSize = Math.Floor((constraints.heightMax - this.Height) / block.height);
                        allowedCpu = Math.Floor((constraints.cpuMax - this.Cpu) / this.lengthCount / this.widthCount);
                        allowedWeight = Math.Floor((constraints.weightMax - this.Weight) / this.lengthCount / this.widthCount / this.block.Weight);
                        this.heightCount += Math.Max(0, Math.Min(allowedSize, Math.Min(allowedCpu, allowedWeight)));
                        break;
                }

                if (blockFillMethod == BlockFillMethod.ALL)
                {
                    double allowedLength = 1.0;
                    double allowedWidth = 1.0;
                    double allowedHeight = 1.0;

                    while (allowedLength > 0.0 || allowedWidth > 0.0 || allowedHeight > 0.0)
                    {
                        allowedSize = Math.Floor((constraints.lengthMax - this.Length) / block.length);
                        allowedCpu = Math.Floor((constraints.cpuMax - this.Cpu) / this.widthCount / this.heightCount);
                        allowedWeight = Math.Floor((constraints.weightMax - this.Weight) / this.widthCount / this.heightCount / this.block.Weight);
                        allowedLength = Math.Max(0, Math.Min(allowedSize, Math.Min(allowedCpu, allowedWeight)));
                        if (allowedLength > 0.0) this.lengthCount++;

                        allowedSize = Math.Floor((constraints.widthMax - this.Width) / block.width);
                        allowedCpu = Math.Floor((constraints.cpuMax - this.Cpu) / this.lengthCount / this.heightCount);
                        allowedWeight = Math.Floor((constraints.weightMax - this.Weight) / this.lengthCount / this.heightCount / this.block.Weight);
                        allowedWidth = Math.Max(0, Math.Min(allowedSize, Math.Min(allowedCpu, allowedWeight)));
                        if (allowedWidth > 0.0) this.widthCount++;

                        allowedSize = Math.Floor((constraints.heightMax - this.Height) / block.height);
                        allowedCpu = Math.Floor((constraints.cpuMax - this.Cpu) / this.lengthCount / this.widthCount);
                        allowedWeight = Math.Floor((constraints.weightMax - this.Weight) / this.lengthCount / this.widthCount / this.block.Weight);
                        allowedHeight = Math.Max(0, Math.Min(allowedSize, Math.Min(allowedCpu, allowedWeight)));
                        if (allowedHeight > 0.0) this.heightCount++;
                    }
                }

                this.isValid = this.Cpu >= constraints.cpuMin
                            && this.Cpu <= constraints.cpuMax
                            && this.Weight >= constraints.weightMin
                            && this.Weight <= constraints.weightMax
                            && this.Length >= constraints.lengthMin
                            && this.Length <= constraints.lengthMax
                            && this.Width >= constraints.widthMin
                            && this.Width <= constraints.widthMax
                            && this.Height >= constraints.heightMin
                            && this.Height <= constraints.heightMax;
            }

            public Block block;
            public double lengthCount;
            public double widthCount;
            public double heightCount;
            public bool isValid = false;
            public int Cpu { get => (int)(this.lengthCount * this.heightCount * this.widthCount); }
            public double Weight { get => this.Cpu * this.block.Weight; }
            public double Length { get => lengthCount * block.length; }
            public double Width { get => widthCount * block.width; }
            public double Height { get => heightCount * block.height; }
        }
    }
}
