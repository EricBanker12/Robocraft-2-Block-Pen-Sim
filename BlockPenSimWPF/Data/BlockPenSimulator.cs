using BlockPenSimWPF.Shared.Models;
using BlockPenSimWPF.Shared.State;
using Microsoft.VisualBasic;
using System.Collections;
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
        private static void SimulateShots(BlockFill blockFill, IEnumerable<Weapon> weapons, DataRow dataRow, bool applyCollision)
        {
            uint blockCount;
            double blockDistance;
            double blockFaceX;
            double blockFaceY;
            double blockArea;
            double blockEnergyAbs;
            foreach (var direction in Enum.GetValues<Direction>())
            {
                switch (direction)
                {
                    default:
                    case Direction.Front:
                        blockCount = (uint)blockFill.lengthCount;
                        blockDistance = blockFill.block.length;
                        blockFaceX = blockFill.block.width;
                        blockFaceY = blockFill.block.height;
                        blockArea = blockFill.block.AreaFront;
                        blockEnergyAbs = blockFill.block.EnergyAbsFront;
                        break;
                    case Direction.Side:
                        blockCount = (uint)blockFill.widthCount;
                        blockDistance = blockFill.block.width;
                        blockFaceX = blockFill.block.length;
                        blockFaceY = blockFill.block.height;
                        blockArea = blockFill.block.AreaSide;
                        blockEnergyAbs = blockFill.block.EnergyAbsSide;
                        break;
                    case Direction.Top:
                        blockCount = (uint)blockFill.heightCount;
                        blockDistance = blockFill.block.height;
                        blockFaceX = blockFill.block.width;
                        blockFaceY = blockFill.block.length;
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


                    // simulate and count shots to penetrate
                    int shots = 0;
                    int deadCount = 0;
                    while (deadCount <= blockCount) // blockCount
                    {
                        var energy = weapon.energy;
                        double distance = 0.0;
                        for (int i = deadCount; i < blockCount && energy > 0.0; i++) // blockCount
                        {
                            if (simBlocks[i].IsDead) continue;

                            var damage = weapon.damage / weapon.pellets;

                            // handle shrapnel for splash damage weapons
                            if (weapon.radius > 1.0 && weapon.pellets > 1.0)
                            {
                                double d = 0;
                                if (direction == Direction.Front) d = blockFill.block.length * i;
                                if (direction == Direction.Side) d = blockFill.block.width * i;
                                if (direction == Direction.Top) d = blockFill.block.height * i;

                                double x = 0;
                                if (direction == Direction.Front) x = blockFill.block.width;
                                if (direction == Direction.Side) x = blockFill.block.length;
                                if (direction == Direction.Top) x = blockFill.block.width;

                                double y = 0;
                                if (direction == Direction.Front) y = blockFill.block.height;
                                if (direction == Direction.Side) y = blockFill.block.height;
                                if (direction == Direction.Top) y = blockFill.block.length;

                                var conePellets = (weapon.pellets - 1.0) / 2.0;

                                // too much work to do actual calculation, so to this is a decent estimate instead
                                // for each shrapnel angle, compare length of inner to outer trajectory to length of block
                                double innerHits = 0.0;
                                double inner = d * Math.Tan(15.0 * (1.0 - 0.3) * Math.PI / 180.0);
                                double outer = d * Math.Tan(15.0 * (1.0 + 0.3) * Math.PI / 180.0);
                                
                                double innerOffset = Math.PI / conePellets;
                                for (int j = 0; j < conePellets; j++)
                                {
                                    var angle = innerOffset + j * 2.0 * Math.PI / conePellets;
                                    var xInner = Math.Cos(angle) * inner;
                                    var yInner = Math.Sin(angle) * inner;
                                    var xOuter = Math.Cos(angle) * outer;
                                    var yOuter = Math.Sin(angle) * outer;

                                    if (x > xOuter && y > yOuter) {
                                        innerHits += 1.0;
                                        continue;
                                    }

                                    var dio = Math.Sqrt(Math.Pow(xOuter - xInner, 2) + Math.Pow(yOuter - yInner, 2));
                                    var dxy = Math.Sqrt(Math.Pow(Math.Max(xOuter, x) - xInner, 2) + Math.Pow(Math.Max(yOuter, y) - yInner, 2));

                                    if (dxy > dio)
                                    {
                                        innerHits += dxy / dio;
                                    }
                                }

                                double outerHits = 0.0;
                                inner = d * Math.Tan(30.0 * (1.0 - 0.3) * Math.PI / 180.0);
                                outer = d * Math.Tan(30.0 * (1.0 + 0.3) * Math.PI / 180.0);

                                for (int j = 0; j < conePellets; j++)
                                {
                                    var angle = j * 2.0 * Math.PI / conePellets;
                                    var xInner = Math.Cos(angle) * inner;
                                    var yInner = Math.Sin(angle) * inner;
                                    var xOuter = Math.Cos(angle) * outer;
                                    var yOuter = Math.Sin(angle) * outer;

                                    if (x > xOuter && y > yOuter)
                                    {
                                        outerHits += 1.0;
                                        continue;
                                    }

                                    var dio = Math.Sqrt(Math.Pow(xOuter - xInner, 2) + Math.Pow(yOuter - yInner, 2));
                                    var dxy = Math.Sqrt(Math.Pow(Math.Max(xOuter, x) - xInner, 2) + Math.Pow(Math.Max(yOuter, y) - yInner, 2));

                                    if (dxy > dio)
                                    {
                                        outerHits += dxy / dio;
                                    }
                                }

                                damage *= 1 + innerHits + outerHits;
                            }
                            
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

                                // for rail gun, damage adjacent block connections for previous block, if they exist.
                                // this is not accurate, energy should be higher, but the high damage and high energy profile for rail minimizes the impact of this shortcut
                                if (weapon.pellets == 1.0 && weapon.radius >= 1.0/3.0)
                                {
                                    if (direction != Direction.Front && blockFill.block.length <= weapon.radius * 2.0)
                                        simBlocks[i - 1].hpFront -= damage * energy / weapon.energy;
                                    if (direction != Direction.Side && blockFill.block.width <= weapon.radius * 2.0)
                                        simBlocks[i - 1].hpSide -= damage * energy / weapon.energy;
                                    if (direction != Direction.Top && blockFill.block.height <= weapon.radius * 2.0)
                                        simBlocks[i - 1].hpTop -= damage * energy / weapon.energy;
                                }
                            }

                            energy -= blockEnergyAbs;

                            // for rail gun, reduce energy by adjacent blocks
                            if (weapon.pellets == 1.0 && weapon.radius >= 1.0/3.0)
                            {
                                double penBlockCount = 0.0;
                                double penBlockDistance = 0.0;

                                // get blocks touching hemisphere
                                for (int j = 1; j < weapon.radius / blockDistance; j++)
                                {
                                    // get circle radius at each block distance such that block plane insersects sphere
                                    double d = j * blockDistance;
                                    double r = weapon.radius * Math.Sin(Math.Acos((weapon.radius - d) / weapon.radius));

                                    // count touching blocks
                                    var r2 = r * r;
                                    for (double x = -blockFill.Width * 0.5; x <= blockFill.Width * 0.5 - blockFill.block.width; x += blockFill.block.width) // up to 189, could be reduced by restricting to min of width and radius
                                    {
                                        var xn = Math.Max(x, Math.Min(0, x + blockFill.block.width));
                                        var x2 = xn * xn;
                                        for (double y = -blockFill.Length * 0.5; y <= blockFill.Length * 0.5 - blockFill.block.length; y += blockFill.block.length) // up to 189, could be reduced by restricting to min of width and radius
                                        {
                                            var yn = Math.Max(y, Math.Min(0, y + blockFill.block.length));
                                            var y2 = yn * yn;
                                            if (x2 + y2 <= r2)
                                            {
                                                penBlockCount++;
                                                // get pen distance from radius
                                                var offset = Math.Sqrt(x2 + y2);
                                                var dOffset = weapon.radius * 2.0 * (1.0 - Math.Cos(Math.Asin(offset / weapon.radius)));
                                                penBlockDistance += Math.Max(0.0, blockDistance - dOffset);
                                            }
                                        }
                                    }
                                }

                                // get blocks touching cylinder
                                var cylinderCount = (distance - weapon.radius) / blockDistance;
                                if (cylinderCount > 0)
                                {
                                    // get circle radius at each block distance such that block plane insersects sphere
                                    double r = weapon.radius;

                                    // count touching blocks
                                    var r2 = r * r;
                                    for (double x = -blockFill.Width * 0.5; x <= blockFill.Width * 0.5 - blockFill.block.width; x += blockFill.block.width) // up to 63
                                    {
                                        var xn = Math.Max(x, Math.Min(0, x + blockFill.block.width));
                                        var x2 = xn * xn;
                                        for (double y = -blockFill.Length * 0.5; y <= blockFill.Length * 0.5 - blockFill.block.length; y += blockFill.block.length) // up to 63
                                        {
                                            var yn = Math.Max(y, Math.Min(0, y + blockFill.block.length));
                                            var y2 = yn * yn;
                                            if (x2 + y2 <= r2)
                                            {
                                                penBlockCount += cylinderCount;
                                                // get pen distance from radius
                                                var offset = Math.Sqrt(x2 + y2);
                                                var dOffset = weapon.radius * 2.0 * (1.0 - Math.Cos(Math.Asin(offset / weapon.radius)));
                                                penBlockDistance += Math.Max(0.0, blockDistance - dOffset) * cylinderCount;
                                            }
                                        }
                                    }
                                }

                                energy = weapon.energy - blockEnergyAbs - penBlockDistance * blockFill.block.material.energyAbsorption / 5.0;
                            }
                            
                            // limit arc discharger distance
                            distance += blockDistance;
                            if (weapon.radius > 1.0 && weapon.pellets > 1.0 && distance > weapon.radius)
                                energy = 0.0;
                        }
                        shots++;
                        if (energy > 0.0) break;
                        if (applyCollision)
                        {
                            var prevDeadCount = deadCount;
                            deadCount = simBlocks.Where(b => b.IsDead).Count();
                            if (deadCount > prevDeadCount && deadCount < simBlocks.Length)
                            {
                                var collisionImpulse = weapon.impulse / 1000.0; // very rough estimate, needs more testing
                                var unlerp20 = (collisionImpulse - 1.0) / (20.0 - 1.0);
                                var collisionDamage = 144.56 * unlerp20 + (1 - unlerp20) * 13.056;
                                var unlerp60 = (collisionImpulse - 1.0) / (60.0 - 1.0);
                                var collisionRadius = 25 * unlerp60 + (1 - unlerp60) * 12.5;
                                for (int i = deadCount; i < simBlocks.Length; i++)
                                {
                                    double d; 
                                    d = blockFill.block.width;
                                    if (direction != Direction.Side) d /= 2.0;
                                    if (direction == Direction.Front) d += blockFill.block.length * (i - deadCount);
                                    if (direction == Direction.Side) d += blockFill.block.width * (i - deadCount);
                                    if (direction == Direction.Top) d += blockFill.block.height * (i - deadCount);
                                    if (d < collisionRadius)
                                    {
                                        var connectionDamage = collisionDamage * Math.Pow((collisionRadius - d) / collisionRadius, 2.5);
                                        simBlocks[i].hpSide -= connectionDamage;
                                    }

                                    d = blockFill.block.height;
                                    if (direction != Direction.Top) d /= 2.0;
                                    if (direction == Direction.Front) d += blockFill.block.length * (i - deadCount);
                                    if (direction == Direction.Side) d += blockFill.block.width * (i - deadCount);
                                    if (direction == Direction.Top) d += blockFill.block.height * (i - deadCount);
                                    if (d < collisionRadius)
                                    {
                                        var connectionDamage = collisionDamage * Math.Pow((collisionRadius - d) / collisionRadius, 2.5);
                                        simBlocks[i].hpTop -= connectionDamage;
                                    }

                                    d = blockFill.block.length;
                                    if (direction != Direction.Front) d /= 2.0;
                                    if (direction == Direction.Front) d += blockFill.block.length * (i - deadCount);
                                    if (direction == Direction.Side) d += blockFill.block.width * (i - deadCount);
                                    if (direction == Direction.Top) d += blockFill.block.height * (i - deadCount);
                                    if (d < collisionRadius)
                                    {
                                        var connectionDamage = collisionDamage * Math.Pow((collisionRadius - d) / collisionRadius, 2.5);
                                        simBlocks[i].hpFront -= connectionDamage;
                                    }
                                }
                            }
                        }
                    }
                    dataRow[$"STP {weapon.name} ({direction})"] = shots;
                }
            }
        }

        static public List<string> GetWeaponColumNames(IndexStore settings, string WeaponName)
        {
            var retval = new List<string>();

            foreach (Weapon weapon in settings.Weapons.Values)
                foreach (var direction in Enum.GetValues<Direction>())
                    if (weapon.name == WeaponName) retval.Add($"STP {weapon.name} ({direction})");

            foreach (Weapon weapon in settings.Weapons.Values)
                foreach (var direction in Enum.GetValues<Direction>())
                    if (weapon.name == WeaponName) retval.Add($"TTP {weapon.name} ({direction})");

            return retval;
        }
        static public List<string> GetWeaponColumNames(IndexStore settings, string WeaponName, Direction direction)
        {
            var retval = new List<string>();

            foreach (Weapon weapon in settings.Weapons.Values)
                if (weapon.name == WeaponName) retval.Add($"STP {weapon.name} ({direction})");

            foreach (Weapon weapon in settings.Weapons.Values)
                if (weapon.name == WeaponName) retval.Add($"TTP {weapon.name} ({direction})");

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
                foreach (var direction in Enum.GetValues<Direction>())
                    schema.Columns.Add($"STP {weapon.name} ({direction})", typeof(int));

            foreach (Weapon weapon in settings.Weapons.Values)
                foreach (var direction in Enum.GetValues<Direction>())
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
                                SimulateShots(blockFill, settings.Weapons.Values, dataRow, settings.applyKilledBlockCollisionDamage);

                                // Add score
                                double score = 0.0;
                                var weaponSum = settings.WeaponSettings.Values.Sum(w => w.WeaponRatio);
                                foreach (var weapon in settings.Weapons)
                                {
                                    var weaponSettings = settings.WeaponSettings[weapon.Key];
                                    var directionSum = weaponSettings.WeaponFrontRatio + weaponSettings.WeaponSideRatio + weaponSettings.WeaponTopRatio;

                                    foreach (var direction in Enum.GetValues<Direction>())
                                    {
                                        var stpColumnName = $"STP {weapon.Value.name} ({direction})";
                                        var ttpColumnName = $"TTP {weapon.Value.name} ({direction})";

                                        double timeToPen = (Math.Ceiling((int)dataRow[stpColumnName] / weaponSettings.WeaponCount) - 1.0) * weapon.Value.cooldown;
                                        dataRow[ttpColumnName] = timeToPen;

                                        if (direction == Direction.Front)
                                            score += timeToPen * weaponSettings.WeaponRatio / weaponSum * weaponSettings.WeaponFrontRatio / directionSum;
                                        else if (direction == Direction.Side)
                                            score += timeToPen * weaponSettings.WeaponRatio / weaponSum * weaponSettings.WeaponSideRatio / directionSum;
                                        else if (direction == Direction.Top)
                                            score += timeToPen * weaponSettings.WeaponRatio / weaponSum * weaponSettings.WeaponTopRatio / directionSum;
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
