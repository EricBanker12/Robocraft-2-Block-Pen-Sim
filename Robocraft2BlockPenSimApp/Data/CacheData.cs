using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Robocraft2BlockPenSimApp.Data.Structs;

namespace Robocraft2BlockPenSimApp.Data
{
    internal class CacheData
    {
        public MinMax Cpu;
        public MinMax Weight;
        public MinMax Length;
        public MinMax Width;
        public MinMax Height;

        public double[] WeaponCount;
        public double[] WeaponRatio;
        public double[] DirectionRatio;

        public async Task SaveData()
        {
            var cachePath = Path.Combine(FileSystem.Current.CacheDirectory, "Robocraft2BlockPenSimAppCache.json");
            var cacheJson = JsonConvert.SerializeObject(this);
            await File.WriteAllTextAsync(cachePath, cacheJson);
        }

        public static async Task<CacheData> GetData()
        {
            var cachePath = Path.Combine(FileSystem.Current.CacheDirectory, "Robocraft2BlockPenSimAppCache.json");
            if (File.Exists(cachePath))
            {
                var cacheJson = await File.ReadAllTextAsync(cachePath);
                return JsonConvert.DeserializeObject<CacheData>(cacheJson);
            }
            else
            {
                return new CacheData()
                {
                    Cpu = new MinMax { Min = 0, Max = 100, },
                    Weight = new MinMax { Min = 0, Max = 4000, },
                    Length = new MinMax { Min = 4, Max = 9 },
                    Width = new MinMax { Min = 9, Max = 9 },
                    Height = new MinMax { Min = 9, Max = 9 },
                    WeaponCount = new double[] { 6, 2, 3, 1 },
                    WeaponRatio = new double[] { 1, 1, 0, 0 },
                    DirectionRatio = new double[] { 8, 1, 1 },
                };
            }
        }
    }
}
