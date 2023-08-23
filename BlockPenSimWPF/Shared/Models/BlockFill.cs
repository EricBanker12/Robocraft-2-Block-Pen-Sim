using BlockPenSimWPF.Shared.State;

namespace BlockPenSimWPF.Shared.Models
{
    internal class BlockFill
    {
        public BlockFill(Block block, IndexStore constraints, BlockFillMethod blockFillMethod)
        {
            this.block = block;

            // get minimum size, cpu, weight
            this.lengthCount = this.widthCount = this.heightCount = 1;
            if (this.Length < constraints.Length.Min)
                this.lengthCount = Math.Ceiling(constraints.Length.Min / block.length);
            if (this.Width < constraints.Width.Min)
                this.widthCount = Math.Ceiling(constraints.Width.Min / block.width);
            if (this.heightCount < constraints.Height.Min)
                this.heightCount = Math.Ceiling(constraints.Height.Min / block.height);

            if (this.Cpu > constraints.Cpu.Max || this.Weight > constraints.Weight.Max)
                return;

            double allowedSize, allowedCpu, allowedWeight;

            switch (blockFillMethod)
            {
                default:
                    break;
                case BlockFillMethod.LWH:
                case BlockFillMethod.LHW:
                    allowedSize = Math.Floor((constraints.Length.Max - this.Length) / block.length);
                    allowedCpu = Math.Floor((constraints.Cpu.Max - this.Cpu) / this.widthCount / this.heightCount);
                    allowedWeight = Math.Floor((constraints.Weight.Max - this.Weight) / this.widthCount / this.heightCount / this.block.Weight);
                    this.lengthCount += Math.Max(0, Math.Min(allowedSize, Math.Min(allowedCpu, allowedWeight)));
                    break;
                case BlockFillMethod.WHL:
                case BlockFillMethod.WLH:
                    allowedSize = Math.Floor((constraints.Width.Max - this.Width) / block.width);
                    allowedCpu = Math.Floor((constraints.Cpu.Max - this.Cpu) / this.lengthCount / this.heightCount);
                    allowedWeight = Math.Floor((constraints.Weight.Max - this.Weight) / this.lengthCount / this.heightCount / this.block.Weight);
                    this.widthCount += Math.Max(0, Math.Min(allowedSize, Math.Min(allowedCpu, allowedWeight)));
                    break;
                case BlockFillMethod.HWL:
                case BlockFillMethod.HLW:
                    allowedSize = Math.Floor((constraints.Height.Max - this.Height) / block.height);
                    allowedCpu = Math.Floor((constraints.Cpu.Max - this.Cpu) / this.lengthCount / this.widthCount);
                    allowedWeight = Math.Floor((constraints.Weight.Max - this.Weight) / this.lengthCount / this.widthCount / this.block.Weight);
                    this.heightCount += Math.Max(0, Math.Min(allowedSize, Math.Min(allowedCpu, allowedWeight)));
                    break;
            }

            switch (blockFillMethod)
            {
                default:
                    break;
                case BlockFillMethod.WLH:
                case BlockFillMethod.HLW:
                    allowedSize = Math.Floor((constraints.Length.Max - this.Length) / block.length);
                    allowedCpu = Math.Floor((constraints.Cpu.Max - this.Cpu) / this.widthCount / this.heightCount);
                    allowedWeight = Math.Floor((constraints.Weight.Max - this.Weight) / this.widthCount / this.heightCount / this.block.Weight);
                    this.lengthCount += Math.Max(0, Math.Min(allowedSize, Math.Min(allowedCpu, allowedWeight)));
                    break;
                case BlockFillMethod.LWH:
                case BlockFillMethod.HWL:
                    allowedSize = Math.Floor((constraints.Width.Max - this.Width) / block.width);
                    allowedCpu = Math.Floor((constraints.Cpu.Max - this.Cpu) / this.lengthCount / this.heightCount);
                    allowedWeight = Math.Floor((constraints.Weight.Max - this.Weight) / this.lengthCount / this.heightCount / this.block.Weight);
                    this.widthCount += Math.Max(0, Math.Min(allowedSize, Math.Min(allowedCpu, allowedWeight)));
                    break;
                case BlockFillMethod.WHL:
                case BlockFillMethod.LHW:
                    allowedSize = Math.Floor((constraints.Height.Max - this.Height) / block.height);
                    allowedCpu = Math.Floor((constraints.Cpu.Max - this.Cpu) / this.lengthCount / this.widthCount);
                    allowedWeight = Math.Floor((constraints.Weight.Max - this.Weight) / this.lengthCount / this.widthCount / this.block.Weight);
                    this.heightCount += Math.Max(0, Math.Min(allowedSize, Math.Min(allowedCpu, allowedWeight)));
                    break;
            }

            switch (blockFillMethod)
            {
                default:
                    break;
                case BlockFillMethod.HWL:
                case BlockFillMethod.WHL:
                    allowedSize = Math.Floor((constraints.Length.Max - this.Length) / block.length);
                    allowedCpu = Math.Floor((constraints.Cpu.Max - this.Cpu) / this.widthCount / this.heightCount);
                    allowedWeight = Math.Floor((constraints.Weight.Max - this.Weight) / this.widthCount / this.heightCount / this.block.Weight);
                    this.lengthCount += Math.Max(0, Math.Min(allowedSize, Math.Min(allowedCpu, allowedWeight)));
                    break;
                case BlockFillMethod.HLW:
                case BlockFillMethod.LHW:
                    allowedSize = Math.Floor((constraints.Width.Max - this.Width) / block.width);
                    allowedCpu = Math.Floor((constraints.Cpu.Max - this.Cpu) / this.lengthCount / this.heightCount);
                    allowedWeight = Math.Floor((constraints.Weight.Max - this.Weight) / this.lengthCount / this.heightCount / this.block.Weight);
                    this.widthCount += Math.Max(0, Math.Min(allowedSize, Math.Min(allowedCpu, allowedWeight)));
                    break;
                case BlockFillMethod.LWH:
                case BlockFillMethod.WLH:
                    allowedSize = Math.Floor((constraints.Height.Max - this.Height) / block.height);
                    allowedCpu = Math.Floor((constraints.Cpu.Max - this.Cpu) / this.lengthCount / this.widthCount);
                    allowedWeight = Math.Floor((constraints.Weight.Max - this.Weight) / this.lengthCount / this.widthCount / this.block.Weight);
                    this.heightCount += Math.Max(0, Math.Min(allowedSize, Math.Min(allowedCpu, allowedWeight)));
                    break;
            }
            // breadth first instead of depth first
            if (blockFillMethod == BlockFillMethod.ALL)
            {
                double allowedLength = 1.0;
                double allowedWidth = 1.0;
                double allowedHeight = 1.0;

                while (allowedLength > 0.0 || allowedWidth > 0.0 || allowedHeight > 0.0)
                {
                    allowedSize = Math.Floor((constraints.Length.Max - this.Length) / block.length);
                    allowedCpu = Math.Floor((constraints.Cpu.Max - this.Cpu) / this.widthCount / this.heightCount);
                    allowedWeight = Math.Floor((constraints.Weight.Max - this.Weight) / this.widthCount / this.heightCount / this.block.Weight);
                    allowedLength = Math.Max(0, Math.Min(allowedSize, Math.Min(allowedCpu, allowedWeight)));
                    if (allowedLength > 0.0) this.lengthCount++;

                    allowedSize = Math.Floor((constraints.Width.Max - this.Width) / block.width);
                    allowedCpu = Math.Floor((constraints.Cpu.Max - this.Cpu) / this.lengthCount / this.heightCount);
                    allowedWeight = Math.Floor((constraints.Weight.Max - this.Weight) / this.lengthCount / this.heightCount / this.block.Weight);
                    allowedWidth = Math.Max(0, Math.Min(allowedSize, Math.Min(allowedCpu, allowedWeight)));
                    if (allowedWidth > 0.0) this.widthCount++;

                    allowedSize = Math.Floor((constraints.Height.Max - this.Height) / block.height);
                    allowedCpu = Math.Floor((constraints.Cpu.Max - this.Cpu) / this.lengthCount / this.widthCount);
                    allowedWeight = Math.Floor((constraints.Weight.Max - this.Weight) / this.lengthCount / this.widthCount / this.block.Weight);
                    allowedHeight = Math.Max(0, Math.Min(allowedSize, Math.Min(allowedCpu, allowedWeight)));
                    if (allowedHeight > 0.0) this.heightCount++;
                }
            }

            this.isValid = this.Cpu >= constraints.Cpu.Min
                        && this.Cpu <= constraints.Cpu.Max
                        && this.Weight >= constraints.Weight.Min
                        && this.Weight <= constraints.Weight.Max
                        && this.Length >= constraints.Length.Min
                        && this.Length <= constraints.Length.Max
                        && this.Width >= constraints.Width.Min
                        && this.Width <= constraints.Width.Max
                        && this.Height >= constraints.Height.Min
                        && this.Height <= constraints.Height.Max;
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
