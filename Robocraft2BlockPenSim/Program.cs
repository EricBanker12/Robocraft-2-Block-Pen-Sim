using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;
using System.Text;
using static Robocraft2BlockPenSim.Types;

namespace Robocraft2BlockPenSim
{
    internal class Program
    {
        // ------------------------------------------------------------------------------------------------------------------------
        // Static variables
        // ------------------------------------------------------------------------------------------------------------------------
        static readonly double[] shapeSizes = { 1.0 / 3.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0 };

        // ------------------------------------------------------------------------------------------------------------------------
        // Helper functions
        // ------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns all (except scaled Post-Straight) permutations of 3 dimensional rectangular prism shape.
        /// </summary>
        /// <returns></returns>
        static List<Shape> GetAllShapes()
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
        static void SimulateShots(BlockFill blockFill, Weapon[] weapons, DataRow dataRow)
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
                for (int w = 0; w < weapons.Length; w++) // 3
                {
                    for (int i = 0; i < blockCount; i++)
                        simBlocks[i] = new SimBlock(blockFill.block);

                    var weapon = weapons[w];
                    var damage = weapon.damage / weapon.pellets;
                    if (weapon.radius > 1.0 && weapon.pellets > 1.0)
                        damage *= (1 + (weapon.pellets - 1) * blockArea / weapon.radius / weapon.radius / Math.PI * 4.0); // pellet spread is roughly 1/2 the radius listed in tooltip

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

                            // for rail gun, damage adjacent block connections, if they exist.
                            if (weapon.pellets == 1.0 && weapon.radius > 1.0)
                            {
                                if (direction == Direction.Front)
                                {
                                    if (blockFill.block.width < weapon.radius * 2.0 && blockFill.widthCount > 1)
                                        simBlocks[i].hpSide -= damage * energy / weapon.energy;
                                    
                                    if (blockFill.block.height < weapon.radius * 2.0 && blockFill.heightCount > 1)
                                        simBlocks[i].hpTop -= damage * energy / weapon.energy;

                                    // Not sure if energy drain is same or different
                                    // energy -= blockEnergyAbs * Math.Min(Math.Ceiling(weapon.radius * 2.0 / blockFill.block.width), blockFill.widthCount) * Math.Min(Math.Ceiling(weapon.radius * 2.0 / blockFill.block.height), blockFill.heightCount);
                                }
                                if (direction == Direction.Side)
                                {
                                    if (blockFill.block.length < weapon.radius * 2.0 && blockFill.lengthCount > 1)
                                        simBlocks[i].hpFront -= damage * energy / weapon.energy;
                                    
                                    if (blockFill.block.height < weapon.radius * 2.0 && blockFill.heightCount > 1)
                                        simBlocks[i].hpTop -= damage * energy / weapon.energy;
                                }
                                if (direction == Direction.Top)
                                {
                                    if (blockFill.block.width < weapon.radius * 2.0 && blockFill.widthCount > 1)
                                        simBlocks[i].hpSide -= damage * energy / weapon.energy;
                                    
                                    if (blockFill.block.length < weapon.radius * 2.0 && blockFill.lengthCount > 1)
                                        simBlocks[i].hpFront -= damage * energy / weapon.energy;
                                }
                            }

                            energy -= blockEnergyAbs;
                            
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

        /// <summary>
        /// Writes datatable content to a .tsv file
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="path"></param>
        static void DataTableToFile(DataTable dataTable, string path)
        {
            DataColumn[] columns = new DataColumn[dataTable.Columns.Count];
            dataTable.Columns.CopyTo(columns, 0);

            try
            {
                using (var fs = new FileStream(path, FileMode.Create))
                {
                    using (var sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(string.Join("\t", columns.Select(col => col.ColumnName)));

                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            sw.WriteLine(string.Join("\t", dataTable.Rows[i].ItemArray));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong!");
                Console.WriteLine(ex.Message);
                Console.WriteLine("Press the 'R' key to retry...");
                Console.WriteLine("Press any other key to exit...");
                var keyinfo = Console.ReadKey();
                if (keyinfo.Key == ConsoleKey.R)
                    DataTableToFile(dataTable, path);
                else
                    Environment.Exit(0);
            }
        }

        /// <summary>
        /// Depicts datatable content in an easily readable format
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        static string DataTableToString(DataTable dataTable)
        {
            DataColumn[] columns = new DataColumn[dataTable.Columns.Count];
            dataTable.Columns.CopyTo(columns, 0);

            var retval = new StringBuilder();
            retval.AppendLine(string.Join(" ", columns.Select(col => col.ColumnName.PadRight(18))));
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                retval.AppendLine(string.Join(" ", dataTable.Rows[i].ItemArray.Select(o => o.ToString().PadRight(18))));
            }
            return retval.ToString();
        }

        // ------------------------------------------------------------------------------------------------------------------------
        // Main routine
        // ------------------------------------------------------------------------------------------------------------------------
        static void Main(string[] args)
        {
            // initialize settings
            var settings = new Settings();

            // initialize simulation results datatable
            var datatable = new DataTable();
            datatable.Columns.Add("Material", typeof(string));
            datatable.Columns.Add("Block Length", typeof(double));
            datatable.Columns.Add("Block Width", typeof(double));
            datatable.Columns.Add("Block Height", typeof(double));
            datatable.Columns.Add("Length", typeof(double));
            datatable.Columns.Add("Width", typeof(double));
            datatable.Columns.Add("Height", typeof(double));
            datatable.Columns.Add("Length Block Count", typeof(double));
            datatable.Columns.Add("Width Block Count", typeof(double));
            datatable.Columns.Add("Height Block Count", typeof(double));
            datatable.Columns.Add("CPU", typeof(int));
            datatable.Columns.Add("Weight", typeof(double));

            foreach (Weapon weapon in settings.weapons)
                foreach (Direction direction in Enum.GetValues(typeof(Direction)))
                    datatable.Columns.Add($"STP {weapon.name} ({direction})", typeof(int));
            
            foreach (Weapon weapon in settings.weapons)
                foreach (Direction direction in Enum.GetValues(typeof(Direction)))
                    datatable.Columns.Add($"TTP {weapon.name} ({direction})", typeof(double));

            datatable.Columns.Add("Score", typeof(double));
            datatable.Columns.Add("Score/CPU", typeof(double));
            datatable.Columns.Add("Score/Weight", typeof(double));

            // output current settings
            Console.WriteLine("BlockFillConstraints");
            Console.WriteLine(JsonConvert.SerializeObject(settings.blockFillConstraints, Formatting.Indented));
            Console.WriteLine("");
            Console.WriteLine("DamageDirectionRatio");
            Console.WriteLine(JsonConvert.SerializeObject(settings.damageDirectionRatio, Formatting.Indented));
            Console.WriteLine("");
            Console.WriteLine("DamageWeaponCount");
            Console.WriteLine(JsonConvert.SerializeObject(settings.damageWeaponCount, Formatting.Indented));
            Console.WriteLine("");
            Console.WriteLine("DamageWeaponRatio");
            Console.WriteLine(JsonConvert.SerializeObject(settings.damageWeaponRatio, Formatting.Indented));
            Console.WriteLine("");

            // Start damage simulation
            Console.WriteLine("Running Simulation...");
            var shapes = GetAllShapes();
            var tasks = new List<Task<DataTable>>();
            
            var sw = Stopwatch.StartNew();
            foreach (Material material in settings.materials) // 3
            {
                foreach (Orientation orientation in Enum.GetValues(typeof(Orientation))) // 6
                {
                    // 18 threads
                    tasks.Add(Task.Run(DataTable () =>
                    {
                        var output = datatable.Clone();
                        foreach (Shape shape in shapes) // 219
                        {
                            Block block = new Block(shape, orientation, material);
                        
                            foreach (BlockFillMethod blockFillMethod in Enum.GetValues(typeof(BlockFillMethod))) // 6
                            {
                                BlockFill blockFill = new BlockFill(block, settings.blockFillConstraints, blockFillMethod);
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
                                dataRow[7] = blockFill.lengthCount;
                                dataRow[8] = blockFill.widthCount;
                                dataRow[9] = blockFill.heightCount;
                                dataRow[10] = blockFill.Cpu;
                                dataRow[11] = blockFill.Weight;

                                // Add STP
                                SimulateShots(blockFill, settings.weapons, dataRow);

                                // Add score
                                double score = 0.0;
                                foreach (Direction direction in Enum.GetValues(typeof(Direction)))
                                {
                                    double directionScore;
                                    if (direction == Direction.Front)
                                        directionScore = settings.damageDirectionRatio.PercentFront;
                                    else if (direction == Direction.Side)
                                        directionScore = settings.damageDirectionRatio.PercentSide;
                                    else //if (direction == Direction.Top)
                                        directionScore = settings.damageDirectionRatio.PercentTop;

                                    foreach (Weapon weapon in settings.weapons)
                                    {
                                        string weaponKey = weapon.name.Replace(" ", "");
                                        var stpColumnName = $"STP {weapon.name} ({direction})";
                                        var ttpColumnName = $"TTP {weapon.name} ({direction})";

                                        double timeToPen = (Math.Ceiling(((int)dataRow[stpColumnName]) / settings.damageWeaponCount[weaponKey]) - 1.0) * weapon.cooldown;
                                        dataRow[ttpColumnName] = timeToPen;

                                        score += timeToPen * settings.damageWeaponRatio[weaponKey] * directionScore;
                                    }
                                }
                                dataRow["Score"] = score;
                                dataRow["Score/CPU"] = score / blockFill.Cpu;
                                dataRow["Score/Weight"] = score / blockFill.Weight;

                                output.Rows.Add(dataRow);
                            }
                        }
                        return output;
                    }));
                }
            }
            
            // Merge tables from all the threads together
            Task.WaitAll(tasks.ToArray());
            foreach (var task in tasks)
            {
                datatable.Merge(task.Result);
                task.Result.Dispose();
            }
            var retval = datatable.AsEnumerable().Distinct(DataRowComparer.Default).CopyToDataTable();
            datatable.Dispose();
            sw.Stop();
            Console.WriteLine($"Simulation completed in {sw.Elapsed}. Produced {retval.Rows.Count} unique results.");

            // Save complete results to file
            var resultsPath = Path.Combine(settings.directory, "results.tsv");
            DataTableToFile(retval, resultsPath);
            Console.WriteLine($"Results saved to \"{resultsPath}\"");
            Console.WriteLine("");

            // Show Top 10 Scores
            var columns = new DataColumn[retval.Columns.Count];
            retval.Columns.CopyTo(columns, 0);
            var columNames = columns.Select(col => col.ColumnName).ToArray();

            var topScoreView = new DataView(retval);
            topScoreView.Sort = "Score desc";
            DataRow[] topScoreRows = new DataRow[10];
            for (int i = 0; i < topScoreRows.Length; i++)
            {
                topScoreRows[i] = topScoreView[i].Row;
            }
            
            var topScoreTable = topScoreRows.CopyToDataTable();
            var topScoreTableSmall = topScoreTable.DefaultView.ToTable(false, new string[] { columNames[0], columNames[1], columNames[2], columNames[3], "Score" });

            var topScoreString = DataTableToString(topScoreTableSmall);
            Console.WriteLine("Top 10 results by Score:");
            Console.WriteLine(topScoreString);

            var topScorePath = Path.Combine(settings.directory, "resultsTopScore.tsv");
            DataTableToFile(topScoreTable, topScorePath);
            Console.WriteLine($"Results saved to \"{topScorePath}\"");
            Console.WriteLine("");

            // Show Top 10 Score/CPU
            topScoreView.Sort = "Score/CPU desc";
            for (int i = 0; i < topScoreRows.Length; i++)
            {
                topScoreRows[i] = topScoreView[i].Row;
            }

            topScoreTable = topScoreRows.CopyToDataTable();
            topScoreTableSmall = topScoreTable.DefaultView.ToTable(false, new string[] { columNames[0], columNames[1], columNames[2], columNames[3], "Score/CPU" });

            topScoreString = DataTableToString(topScoreTableSmall);
            Console.WriteLine("Top 10 results by Score/CPU:");
            Console.WriteLine(topScoreString);

            topScorePath = Path.Combine(settings.directory, "resultsTopCPU.tsv");
            DataTableToFile(topScoreTable, topScorePath);
            Console.WriteLine($"Results saved to \"{topScorePath}\"");
            Console.WriteLine("");

            // Show Top 10 Score/Weight
            topScoreView.Sort = "Score/Weight desc";
            for (int i = 0; i < topScoreRows.Length; i++)
            {
                topScoreRows[i] = topScoreView[i].Row;
            }

            topScoreTable = topScoreRows.CopyToDataTable();
            topScoreTableSmall = topScoreTable.DefaultView.ToTable(false, new string[] { columNames[0], columNames[1], columNames[2], columNames[3], "Score/Weight" });

            topScoreString = DataTableToString(topScoreTableSmall);
            Console.WriteLine("Top 10 results by Score/Weight:");
            Console.WriteLine(topScoreString);

            topScorePath = Path.Combine(settings.directory, "resultsTopWeight.tsv");
            DataTableToFile(topScoreTable, topScorePath);
            Console.WriteLine($"Results saved to \"{topScorePath}\"");
            Console.WriteLine("");

            // Prompt to exit
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}