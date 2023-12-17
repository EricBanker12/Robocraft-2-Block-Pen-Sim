using BlockPenSimWPF.Shared.Models;
using BlockPenSimWPF.Shared.State;
using Microsoft.VisualBasic;
using System.Collections;
using System.Data;
using System.Threading.Tasks;

namespace BlockPenSimWPF.Data
{
    internal static class BlockPenSimulator
    {
        private static readonly double[] shapeSizes = { 1.0 / 3.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0 };

        private static readonly double[] postSizes = { 1.0 / 3.0, 2.0 / 3.0, 1.0, 4.0 / 3.0, 5.0 / 3.0, 2.0, 7.0 / 3.0, 8.0 / 3.0, 3.0 };

        /// <summary>
        /// Generates all permutations of shapes
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
        /// Generates all permutations of post straights
        /// </summary>
        /// <returns></returns>
        private static List<Shape> GetAllPosts()
        {
            var retval = new List<Shape>();
            for (int a = 0; a < postSizes.Length; a++)
                for (int b = 0; b < postSizes.Length; b++)
                    for (int c = 1; c < shapeSizes.Length; c++)
                    {
                        if (postSizes[a] > postSizes[b]) continue;
                        retval.Add(new Shape { smallest = postSizes[a], middle = postSizes[b], largest = shapeSizes[c] });
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
            double blockFillX;
            double blockFillY;
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
                        blockFillX = blockFill.Width;
                        blockFillY = blockFill.Height;
                        blockArea = blockFill.block.AreaFront;
                        blockEnergyAbs = blockFill.block.EnergyAbsFront;
                        break;
                    case Direction.Side:
                        blockCount = (uint)blockFill.widthCount;
                        blockDistance = blockFill.block.width;
                        blockFaceX = blockFill.block.length;
                        blockFaceY = blockFill.block.height;
                        blockFillX = blockFill.Length;
                        blockFillY = blockFill.Height;
                        blockArea = blockFill.block.AreaSide;
                        blockEnergyAbs = blockFill.block.EnergyAbsSide;
                        break;
                    case Direction.Top:
                        blockCount = (uint)blockFill.heightCount;
                        blockDistance = blockFill.block.height;
                        blockFaceX = blockFill.block.width;
                        blockFaceY = blockFill.block.length;
                        blockFillX = blockFill.Width;
                        blockFillY = blockFill.Length;
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
                    while (deadCount <= blockCount) // blockCount, up to 189
                    {
                        var energy = weapon.energy;
                        double distance = 0.0;
                        for (int i = deadCount; i < blockCount && energy > 0.0; i++) // blockCount, up to 189
                        {
                            if (simBlocks[i].IsDead) continue;

                            var damage = weapon.damage / weapon.pellets;

                            // handle shrapnel for splash damage weapons
                            if (weapon.radius > 0.3 && weapon.pellets > 1.0 && weapon.splashShape != SplashShape.None)
                            {
                                double d = blockDistance * (i - deadCount) + weapon.radius / 2.0;
                                double x = blockFaceX / 2.0;
                                double y = blockFaceY / 2.0;

                                var conePellets = Math.Floor((weapon.pellets - 1.0) / 2.0);

                                // too much work to do actual calculation, so to this is a decent estimate instead
                                // for each shrapnel angle, compare length of inner to outer trajectory to length of block
                                double innerHits = 0.0;
                                double inner = d * Math.Tan(15.0 * (1.0 - 0.3) * Math.PI / 180.0);
                                double outer = d * Math.Tan(15.0 * (1.0 + 0.3) * Math.PI / 180.0);

                                if (weapon.splashShape == SplashShape.Cylinder)
                                {
                                    inner = weapon.radius / 2.0;
                                    outer = inner;
                                }

                                double innerOffset = Math.PI / conePellets;
                                for (int j = 0; j < conePellets; j++)
                                {
                                    var angle = innerOffset + j * 2.0 * Math.PI / conePellets;
                                    var xInner = Math.Abs(Math.Cos(angle) * inner);
                                    var yInner = Math.Abs(Math.Sin(angle) * inner);
                                    var xOuter = Math.Abs(Math.Cos(angle) * outer);
                                    var yOuter = Math.Abs(Math.Sin(angle) * outer);

                                    if (x > xOuter && y > yOuter)
                                    {
                                        innerHits += 1.0;
                                        continue;
                                    }

                                    if (x > xInner && y > yInner)
                                    {
                                        var dio = Math.Sqrt(Math.Pow(xOuter - xInner, 2) + Math.Pow(yOuter - yInner, 2));
                                        var dxy = Math.Sqrt(Math.Pow(Math.Min(xOuter, x) - xInner, 2) + Math.Pow(Math.Min(yOuter, y) - yInner, 2));
                                        innerHits += dxy / dio;
                                        continue;
                                    }
                                }

                                double outerHits = 0.0;
                                inner = d * Math.Tan(30.0 * (1.0 - 0.3) * Math.PI / 180.0);
                                outer = d * Math.Tan(30.0 * (1.0 + 0.3) * Math.PI / 180.0);

                                if (weapon.splashShape == SplashShape.Cylinder)
                                {
                                    inner = weapon.radius / 2.0;
                                    outer = inner;
                                }
                                
                                conePellets = Math.Ceiling((weapon.pellets - 1.0) / 2.0);

                                for (int j = 0; j < conePellets; j++)
                                {
                                    var angle = j * 2.0 * Math.PI / conePellets;
                                    var xInner = Math.Abs(Math.Cos(angle) * inner);
                                    var yInner = Math.Abs(Math.Sin(angle) * inner);
                                    var xOuter = Math.Abs(Math.Cos(angle) * outer);
                                    var yOuter = Math.Abs(Math.Sin(angle) * outer);

                                    if (x > xOuter && y > yOuter)
                                    {
                                        outerHits += 1.0;
                                        continue;
                                    }


                                    if (x > xInner && y > yInner)
                                    {
                                        var dio = Math.Sqrt(Math.Pow(xOuter - xInner, 2) + Math.Pow(yOuter - yInner, 2));
                                        var dxy = Math.Sqrt(Math.Pow(Math.Min(xOuter, x) - xInner, 2) + Math.Pow(Math.Min(yOuter, y) - yInner, 2));
                                        outerHits += dxy / dio;
                                        continue;
                                    }
                                }

                                damage *= 1.0 + innerHits + outerHits;
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
                                if (weapon.pellets == 1.0 && weapon.radius >= 1.0 / 3.0)
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

                            // TODO limit distance to max penetration distance
                            distance += blockDistance;
                        }
                        shots++;
                        if (energy > 0.0) break;
                        if (!applyCollision)
                        {
                            deadCount = simBlocks.Where(b => b.IsDead).Count();
                        }
                        else
                        {
                            var prevDeadCount = deadCount;
                            deadCount = simBlocks.Where(b => b.IsDead).Count();
                            if (deadCount > prevDeadCount && deadCount < simBlocks.Length)
                            {
                                var sizes = new List<double>() { blockFill.block.length, blockFill.block.width, blockFill.block.height };
                                sizes.Sort();
                                var collisionImpulse = weapon.impulse * (1 + (sizes[2] - sizes[0])) / 20000.0; // very rough estimate, needs more testing
                                if (collisionImpulse >= 1.0)
                                {
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
                                    deadCount = simBlocks.Where(b => b.IsDead).Count();
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
            var schema = CreateSchema(settings);
            var shapes = GetAllShapes();

            if (settings.simulateWithScaledPostStraights)
            {
                var posts = GetAllPosts();
                foreach (var post in posts)
                {
                    bool isShape = false;
                    foreach (var shape in shapes)
                    {
                        if (shape.Equals(post))
                        {
                            isShape = true;
                            break;
                        }
                    }
                    
                    if (isShape) continue;
                    else shapes.Add(post);
                }
            }

            var tasks = new List<Task<DataTable>>();

            foreach (Shape shape in shapes) // 219
            {
                tasks.Add(Task.Run(DataTable () =>
                {
                    var output = schema.Clone();
                    foreach (Material material in settings.Materials.Values) // 3
                    {
                        foreach (Orientation orientation in Enum.GetValues(typeof(Orientation))) // 6
                        {
                            // skip duplicate shapes
                            if (shape.smallest == shape.middle && (orientation == Orientation.SidewaysTall || orientation == Orientation.FlatLong || orientation == Orientation.FlatWide)) continue;
                            if (shape.largest == shape.middle && (orientation == Orientation.ForwardsWide || orientation == Orientation.SidewaysLong || orientation == Orientation.FlatWide)) continue;
                            if (shape.smallest == shape.middle && shape.largest == shape.middle && ((int)orientation) > 0) continue;

                            Block block = new Block(shape, orientation, material);

                            if (block.length > settings.Length.Max) continue;
                            if (block.width > settings.Width.Max) continue;
                            if (block.height > settings.Height.Max) continue;

                            var minLengthCount = Math.Max((int)Math.Ceiling(settings.Length.Min / block.length), 1);
                            var minWidthCount = Math.Max((int)Math.Ceiling(settings.Width.Min / block.width), 1);
                            var minHeightCount = Math.Max((int)Math.Ceiling(settings.Height.Min / block.height), 1);

                            var maxLengthCount = Math.Max((int)Math.Floor(settings.Length.Max / block.length), 1);
                            var maxWidthCount = Math.Max((int)Math.Floor(settings.Width.Max / block.width), 1);
                            var maxHeightCount = Math.Max((int)Math.Floor(settings.Height.Max / block.height), 1);

                            for (int lengthCount = minLengthCount; lengthCount <= maxLengthCount; lengthCount++) // 1 - 63
                            {
                                for (int widthCount = minWidthCount; widthCount <= maxWidthCount; widthCount++) // 1 - 63
                                {
                                    for (int heightCount = minHeightCount; heightCount <= maxHeightCount; heightCount++) // 1 - 63
                                    {
                                        var blockFill = new BlockFill(block, lengthCount, widthCount, heightCount);

                                        if (blockFill.Cpu < settings.Cpu.Min || blockFill.Cpu > settings.Cpu.Max) continue;
                                        if (blockFill.Weight < settings.Weight.Min || blockFill.Weight > settings.Weight.Max) continue;

                                        var dataRow = output.NewRow();

                                        // Add block data
                                        dataRow[0] = blockFill.block.material.name;
                                        dataRow[1] = blockFill.block.length;
                                        dataRow[2] = blockFill.block.width;
                                        dataRow[3] = blockFill.block.height;
                                        dataRow[4] = blockFill.Length;
                                        dataRow[5] = blockFill.Width;
                                        dataRow[6] = blockFill.Height;
                                        dataRow[7] = blockFill.lengthCount;
                                        dataRow[8] = blockFill.widthCount;
                                        dataRow[9] = blockFill.heightCount;
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
                            }
                        }
                    }
                    
                    return output;
                }));
            }

            await Task.WhenAll(tasks);

            using (schema)
            {
                foreach (var task in tasks)
                {
                    schema.Merge(task.Result);
                    task.Result.Dispose();
                }

                return schema;
            }
        }

        /// <summary>
        /// Create empty datatable with simulation results schema
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static DataTable CreateSchema(IndexStore settings)
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

            return schema;
        }
    }
}
