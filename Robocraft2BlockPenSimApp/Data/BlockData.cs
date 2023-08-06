using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Robocraft2BlockPenSimApp.Shared.Models;
using Windows.Storage.Pickers.Provider;

namespace Robocraft2BlockPenSimApp.Data
{
    internal class BlockData
    {
        public Weapon[] weapons = { };
        public Material[] materials = { };

        private static readonly string filename = "BlockData.json";

        static async Task<string> ReadTextFile(string filePath)
        {
            using Stream fileStream = await FileSystem.Current.OpenAppPackageFileAsync(filePath);
            using StreamReader reader = new StreamReader(fileStream);
            return await reader.ReadToEndAsync();
        }

        static async Task WriteTextFile(string filePath, string fileContent)
        {
            using Stream fileStream = await FileSystem.Current.OpenAppPackageFileAsync(filePath);
            using StreamWriter writer = new StreamWriter(fileStream);
            await writer.WriteAsync(fileContent);
        }

        public static async Task WriteData(Weapon[] weapons, Material[] materials)
        {
            var json = JsonConvert.SerializeObject(new BlockData { materials = materials, weapons = weapons });
            await WriteTextFile(filename, json);
        }

        public static async Task WriteData(BlockData blockData)
        {
            var json = JsonConvert.SerializeObject(blockData);
            await WriteTextFile(filename, json);
        }

        public static async Task<BlockData> GetData()
        {
            var json = await ReadTextFile(filename);
            return JsonConvert.DeserializeObject<BlockData>(json) ?? new BlockData();
        }
    }
}
