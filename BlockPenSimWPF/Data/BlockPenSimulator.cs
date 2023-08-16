using BlockPenSimWPF.Shared.Models;
using BlockPenSimWPF.Shared.State;
using System.Data;

namespace BlockPenSimWPF.Data
{
    internal static class BlockPenSimulator
    {
        private static readonly double[] shapeSizes = { 1.0 / 3.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0 };

        /// <summary>
        /// Generates all 219 permutations of shapes
        /// </summary>
        /// <returns></returns>
        private static List<Shape> GetAllShapes()
        {
            var retval = new List<Shape>();
            for (int a = 0; a < shapeSizes.Length; a++)
                for (int b = 0; b < shapeSizes.Length; b++)
                    for (int c = 1; c < shapeSizes.Length; c++)
                    {
                        if (shapeSizes[a] > shapeSizes[b]) continue;
                        if (shapeSizes[b] > shapeSizes[c]) continue;
                        retval.Add(new Shape { smallest = shapeSizes[a], middle = shapeSizes[b], largest = shapeSizes[c] });
                    }

            return retval;
        }

        /// <summary>
        /// Simulates damage from all weapons from all directions, updating shots-to-penetrate columns
        /// </summary>
        /// <param name="blockFill"></param>
        /// <param name="weapons"></param>
        /// <param name="dataRow"></param>
        private static void SimulateShots(BlockFill blockFill, IEnumerable<Weapon> weapons, DataRow dataRow)
        {
            uint blockCount;
            double blockDistance;
            double blockArea;
            double blockEnergyAbs;
            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                switch (direction)
                {
                    default:
                    case Direction.Front:
                        blockCount = (uint)blockFill.lengthCount;
                        blockDistance = blockFill.block.length;
                        blockArea = blockFill.block.AreaFront;
                        blockEnergyAbs = blockFill.block.EnergyAbsFront;
                        break;
                    case Direction.Side:
                        blockCount = (uint)blockFill.widthCount;
                        blockDistance = blockFill.block.width;
                        blockArea = blockFill.block.AreaSide;
                        blockEnergyAbs = blockFill.block.EnergyAbsSide;
                        break;
                    case Direction.Top:
                        blockCount = (uint)blockFill.heightCount;
                        blockDistance = blockFill.block.height;
                        blockArea = blockFill.block.AreaTop;
                        blockEnergyAbs = blockFill.block.EnergyAbsTop;
                        break;
                }

                // sim for each weapon
                var simBlocks = new SimBlock[blockCount];
                foreach (var weapon in weapons) // 3
                {
                    for (int i = 0; i < blockCount; i++)
                        simBlocks[i] = new SimBlock(blockFill.block);

                    var damage = weapon.damage / weapon.pellets;
                    if (weapon.radius > 1.0 && weapon.pellets > 1.0)
                        damage *= 1 + (weapon.pellets - 1) * blockArea / weapon.radius / weapon.radius / Math.PI * 4.0; // pellet spread is roughly 1/2 the radius listed in tooltip

                    // simulate and count shots to penetrate
                    int shots = 0;
                    while (true) // blockCount
                    {
                        var energy = weapon.energy;
                        double distance = 0.0;
                        for (int i = 0; i < blockCount && energy > 0.0; i++) // blockCount
                        {
                            if (simBlocks[i].IsDead) continue;
                            // damage all the face connections for the current block
                            simBlocks[i].hpFront -= damage * energy / weapon.energy;
                            simBlocks[i].hpSide -= damage * energy / weapon.energy;
                            simBlocks[i].hpTop -= damage * energy / weapon.energy;

                            // damage connecting face on previous block (penetration double dipping)
                            if (i > 0)
                            {
                                if (direction == Direction.Front)
                                    simBlocks[i - 1].hpFront -= damage * energy / weapon.energy;
                                if (direction == Direction.Side)
                                    simBlocks[i - 1].hpSide -= damage * energy / weapon.energy;
                                if (direction == Direction.Top)
                                    simBlocks[i - 1].hpTop -= damage * energy / weapon.energy;
                            }

                            var energyLoss = blockEnergyAbs;

                            // for rail gun, damage adjacent block connections, if they exist.
                            if (weapon.pellets == 1.0 && weapon.radius > 1.0)
                            {
                                if (direction == Direction.Front)
                                {
                                    if (blockFill.block.width < blockFill.block.height)
                                    {
                                        if (blockFill.block.width < weapon.radius * 2.0 && blockFill.widthCount > 1)
                                        {
                                            energyLoss += blockEnergyAbs;
                                            if (energy - energyLoss > 0) simBlocks[i].hpSide -= damage * (energy - energyLoss) / weapon.energy;
                                            energyLoss += blockEnergyAbs;
                                        }

                                        if (blockFill.block.height < weapon.radius * 2.0 && blockFill.heightCount > 1)
                                        {
                                            energyLoss += blockEnergyAbs;
                                            if (energy - energyLoss > 0) simBlocks[i].hpTop -= damage * (energy - energyLoss) / weapon.energy;
                                            energyLoss += blockEnergyAbs;
                                        }
                                    }
                                    else
                                    {
                                        if (blockFill.block.height < weapon.radius * 2.0 && blockFill.heightCount > 1)
                                        {
                                            energyLoss += blockEnergyAbs;
                                            if (energy - energyLoss > 0) simBlocks[i].hpTop -= damage * (energy - energyLoss) / weapon.energy;
                                            energyLoss += blockEnergyAbs;
                                        }

                                        if (blockFill.block.width < weapon.radius * 2.0 && blockFill.widthCount > 1)
                                        {
                                            energyLoss += blockEnergyAbs;
                                            if (energy - energyLoss > 0) simBlocks[i].hpSide -= damage * (energy - energyLoss) / weapon.energy;
                                            energyLoss += blockEnergyAbs;
                                        }
                                    }

                                    energyLoss = blockEnergyAbs * Math.Min(Math.Ceiling(weapon.radius * 2.0 / blockFill.block.width), blockFill.widthCount) * Math.Min(Math.Ceiling(weapon.radius * 2.0 / blockFill.block.height), blockFill.heightCount);
                                }
                                if (direction == Direction.Side)
                                {
                                    if (blockFill.block.length < blockFill.block.height)
                                    {
                                        if (blockFill.block.length < weapon.radius * 2.0 && blockFill.lengthCount > 1)
                                        {
                                            energyLoss += blockEnergyAbs;
                                            if (energy - energyLoss > 0) simBlocks[i].hpFront -= damage * (energy - energyLoss) / weapon.energy;
                                            energyLoss += blockEnergyAbs;
                                        }

                                        if (blockFill.block.height < weapon.radius * 2.0 && blockFill.heightCount > 1)
                                        {
                                            energyLoss += blockEnergyAbs;
                                            if (energy - energyLoss > 0) simBlocks[i].hpTop -= damage * (energy - energyLoss) / weapon.energy;
                                            energyLoss += blockEnergyAbs;
                                        }
                                    }
                                    else
                                    {
                                        if (blockFill.block.height < weapon.radius * 2.0 && blockFill.heightCount > 1)
                                        {
                                            energyLoss += blockEnergyAbs;
                                            if (energy - energyLoss > 0) simBlocks[i].hpTop -= damage * (energy - energyLoss) / weapon.energy;
                                            energyLoss += blockEnergyAbs;
                                        }

                                        if (blockFill.block.length < weapon.radius * 2.0 && blockFill.lengthCount > 1)
                                        {
                                            energyLoss += blockEnergyAbs;
                                            if (energy - energyLoss > 0) simBlocks[i].hpFront -= damage * (energy - energyLoss) / weapon.energy;
                                            energyLoss += blockEnergyAbs;
                                        }
                                    }

                                    energyLoss = blockEnergyAbs * Math.Min(Math.Ceiling(weapon.radius * 2.0 / blockFill.block.length), blockFill.lengthCount) * Math.Min(Math.Ceiling(weapon.radius * 2.0 / blockFill.block.height), blockFill.heightCount);
                                }
                                if (direction == Direction.Top)
                                {
                                    if (blockFill.block.width < blockFill.block.length)
                                    {
                                        if (blockFill.block.width < weapon.radius * 2.0 && blockFill.widthCount > 1)
                                        {
                                            energyLoss += blockEnergyAbs;
                                            if (energy - energyLoss > 0) simBlocks[i].hpSide -= damage * (energy - energyLoss) / weapon.energy;
                                            energyLoss += blockEnergyAbs;
                                        }

                                        if (blockFill.block.length < weapon.radius * 2.0 && blockFill.lengthCount > 1)
                                        {
                                            energyLoss += blockEnergyAbs;
                                            if (energy - energyLoss > 0) simBlocks[i].hpFront -= damage * (energy - energyLoss) / weapon.energy;
                                            energyLoss += blockEnergyAbs;
                                        }
                                    }
                                    else
                                    {
                                        if (blockFill.block.length < weapon.radius * 2.0 && blockFill.lengthCount > 1)
                                        {
                                            energyLoss += blockEnergyAbs;
                                            if (energy - energyLoss > 0) simBlocks[i].hpFront -= damage * (energy - energyLoss) / weapon.energy;
                                            energyLoss += blockEnergyAbs;
                                        }

                                        if (blockFill.block.width < weapon.radius * 2.0 && blockFill.widthCount > 1)
                                        {
                                            energyLoss += blockEnergyAbs;
                                            if (energy - energyLoss > 0) simBlocks[i].hpSide -= damage * (energy - energyLoss) / weapon.energy;
                                            energyLoss += blockEnergyAbs;
                                        }
                                    }

                                    energyLoss = blockEnergyAbs * Math.Min(Math.Ceiling(weapon.radius * 2.0 / blockFill.block.width), blockFill.widthCount) * Math.Min(Math.Ceiling(weapon.radius * 2.0 / blockFill.block.length), blockFill.lengthCount);
                                }
                            }

                            energy -= energyLoss;

                            // limit arc discharger distance
                            distance += blockDistance;
                            if (weapon.radius > 1.0 && weapon.pellets > 1.0 && distance > weapon.radius)
                                energy = 0.0;
                        }
                        shots++;
                        if (energy > 0.0) break;
                    }
                    dataRow[$"STP {weapon.name} ({direction})"] = shots;
                }
            }
        }

        static public List<string> GetWeaponColumNames(IndexStore settings, string WeaponName)
        {
            var retval = new List<string>();

            foreach (Weapon weapon in settings.Weapons.Values)
                foreach (Direction direction in Enum.GetValues(typeof(Direction)))
                    if (weapon.name == WeaponName) retval.Add($"STP {weapon.name} ({direction})");

            foreach (Weapon weapon in settings.Weapons.Values)
                foreach (Direction direction in Enum.GetValues(typeof(Direction)))
                    if (weapon.name == WeaponName) retval.Add($"TTP {weapon.name} ({direction})");

            return retval;
        }
        static public List<string> GetDirectionColumNames(IndexStore settings, Direction dir)
        {
            var retval = new List<string>();

            foreach (Weapon weapon in settings.Weapons.Values)
                foreach (Direction direction in Enum.GetValues(typeof(Direction)))
                    if (direction == dir) retval.Add($"STP {weapon.name} ({direction})");

            foreach (Weapon weapon in settings.Weapons.Values)
                foreach (Direction direction in Enum.GetValues(typeof(Direction)))
                    if (direction == dir) retval.Add($"TTP {weapon.name} ({direction})");

            return retval;
        }

        public static DataTable Run(IndexStore settings)
        {
            var t = RunAsync(settings);
            Task.WaitAll(new Task[] { t });
            return t.Result;
        }

        /// <summary>
        /// Fills datatable with simulation results and returns it
        /// </summary>
        /// <returns></returns>
        public static async Task<DataTable> RunAsync(IndexStore settings)
        {
            var schema = new DataTable($"Simulation{DateTime.Now}");
            schema.Columns.Add("Material", typeof(string));
            schema.Columns.Add("Block Length", typeof(double));
            schema.Columns.Add("Block Width", typeof(double));
            schema.Columns.Add("Block Height", typeof(double));
            schema.Columns.Add("Length", typeof(double));
            schema.Columns.Add("Width", typeof(double));
            schema.Columns.Add("Height", typeof(double));
            schema.Columns.Add("Length Block Count", typeof(int));
            schema.Columns.Add("Width Block Count", typeof(int));
            schema.Columns.Add("Height Block Count", typeof(int));
            schema.Columns.Add("CPU", typeof(int));
            schema.Columns.Add("Weight (kg)", typeof(double));

            foreach (Weapon weapon in settings.Weapons.Values)
                foreach (Direction direction in Enum.GetValues(typeof(Direction)))
                    schema.Columns.Add($"STP {weapon.name} ({direction})", typeof(int));

            foreach (Weapon weapon in settings.Weapons.Values)
                foreach (Direction direction in Enum.GetValues(typeof(Direction)))
                    schema.Columns.Add($"TTP {weapon.name} ({direction})", typeof(double));
            
            schema.Columns.Add("Score", typeof(double));
            schema.Columns.Add("Score / CPU", typeof(double));
            schema.Columns.Add("Score / Weight", typeof(double));

            var shapes = GetAllShapes();
            var tasks = new List<Task<DataTable>>();

            foreach (Material material in settings.Materials.Values) // 3
            {
                foreach (Orientation orientation in Enum.GetValues(typeof(Orientation))) // 6
                {
                    // 18 threads
                    tasks.Add(Task.Run(DataTable () =>
                    {
                        var output = schema.Clone();
                        foreach (Shape shape in shapes) // 219
                        {
                            Block block = new Block(shape, orientation, material);

                            foreach (BlockFillMethod blockFillMethod in Enum.GetValues(typeof(BlockFillMethod))) // 6
                            {
                                BlockFill blockFill = new BlockFill(block, settings, blockFillMethod);
                                if (!blockFill.isValid) continue;

                                var dataRow = output.NewRow();
                                // Add block data
                                dataRow[0] = blockFill.block.material.name;
                                dataRow[1] = blockFill.block.length;
                                dataRow[2] = blockFill.block.width;
                                dataRow[3] = blockFill.block.height;
                                dataRow[4] = blockFill.Length;
                                dataRow[5] = blockFill.Width;
                                dataRow[6] = blockFill.Height;
                                dataRow[7] = (int)blockFill.lengthCount;
                                dataRow[8] = (int)blockFill.widthCount;
                                dataRow[9] = (int)blockFill.heightCount;
                                dataRow[10] = blockFill.Cpu;
                                dataRow[11] = blockFill.Weight;

                                // Add STP
                                SimulateShots(blockFill, settings.Weapons.Values, dataRow);

                                // Add score
                                double sumDirectionRatio = settings.DirectionRatio.Sum();
                                double sumWeaponRatio = settings.WeaponRatio.Values.Sum();
                                double score = 0.0;
                                for (int d = 0; d < settings.DirectionRatio.Length; d++)
                                {
                                    double directionScore = settings.DirectionRatio[d] / sumDirectionRatio;
                                    Direction direction = (Direction)d;

                                    foreach (var weapon in settings.Weapons)
                                    {
                                        var stpColumnName = $"STP {weapon.Value.name} ({direction})";
                                        var ttpColumnName = $"TTP {weapon.Value.name} ({direction})";

                                        double timeToPen = (Math.Ceiling((int)dataRow[stpColumnName] / settings.WeaponCount[weapon.Key]) - 1.0) * weapon.Value.cooldown;
                                        dataRow[ttpColumnName] = timeToPen;

                                        score += timeToPen * settings.WeaponRatio[weapon.Key] / sumWeaponRatio * directionScore;
                                    }
                                }
                                dataRow["Score"] = score;
                                dataRow["Score / CPU"] = score / blockFill.Cpu;
                                dataRow["Score / Weight"] = score / blockFill.Weight;

                                output.Rows.Add(dataRow);
                            }
                        }
                        return output;
                    }));
                }
            }

            await Task.WhenAll(tasks);

            using (schema)
            {
                foreach (var task in tasks)
                {
                    schema.Merge(task.Result);
                    task.Result.Dispose();
                }

                if (schema.Rows.Count == 0)
                    return schema;
                else
                    return schema.AsEnumerable().Distinct(DataRowComparer.Default).CopyToDataTable();
            }
        }
    }
}
