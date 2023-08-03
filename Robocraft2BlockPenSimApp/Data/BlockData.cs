using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Robocraft2BlockPenSimApp.Data.Structs;

namespace Robocraft2BlockPenSimApp.Data
{
    internal class BlockData
    {
        public Weapon[] weapons = { };
        public Material[] materials = { };

        static async Task<string> ReadTextFile(string filePath)
        {
            using Stream fileStream = await FileSystem.Current.OpenAppPackageFileAsync(filePath);
            using StreamReader reader = new StreamReader(fileStream);

            return await reader.ReadToEndAsync();
        }

        public static async Task<BlockData> GetData()
        {
            var json = await ReadTextFile("BlockData.json");
            return JsonConvert.DeserializeObject<BlockData>(json) ?? new BlockData();
        }
    }
}
