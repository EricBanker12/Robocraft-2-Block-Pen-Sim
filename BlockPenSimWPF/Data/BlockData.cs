using BlockPenSimWPF.Shared.Models;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;

namespace BlockPenSimWPF.Data
{
    internal class BlockData
    {

        public Dictionary<string, Weapon> Weapons;
        public Dictionary<string, Material> Materials;

        public static Dictionary<string, Weapon> DefaultWeapons = new Dictionary<string, Weapon>()
        {
            {"LaserBlaster", new Weapon { name = "Laser Blaster", cpu = 20, damage = 13.0, pellets = 1.0, radius = 0.3, energy = 2000.0, cooldown = 0.6  } },
            {"PlasmaCannon", new Weapon { name = "Plasma Cannon", cpu = 60, damage = 500.0, pellets = 10.0, radius = 12.5, energy = 1000.0, cooldown = 2.0  } },
            {"ArcDischarger", new Weapon{ name = "Arc Discharger", cpu = 40, damage = 15000.0, pellets = 20.0, radius = 30.0, energy = 120000.0, cooldown = 6.0  } },
            {"RailGun", new Weapon      { name = "Rail Gun",       cpu = 120, damage = 950.0, pellets = 1.0, radius = 0.3, energy = 10000.0, cooldown = 4.0  } },
        };

        public static Dictionary<string, Material> DefaultMaterials = new Dictionary<string, Material>()
        {
            {"Airium", new Material     { name = "Airium", density = 0.7, connectionStrength = 1.59, energyAbsorption = 2500.0 } },
            {"Moderonium", new Material { name = "Moderonium", density = 1.8, connectionStrength = 2.65, energyAbsorption = 5000.0 } },
            {"Ladium", new Material     { name = "Ladium", density = 7.3, connectionStrength = 3.35, energyAbsorption = 7500.0 } },
        };

        public BlockData()
        {
            Weapons = DefaultWeapons;
            Materials = DefaultMaterials;
        }

        private static readonly string dataUrl = "https://raw.githubusercontent.com/EricBanker12/Robocraft-2-Block-Pen-Sim/master/Data/BlockData.json";

        public static async Task WriteData()
        {
            var json = JsonConvert.SerializeObject(new BlockData(), Formatting.Indented);
            await File.WriteAllTextAsync(Path.Join(AppContext.BaseDirectory, "BlockData.json"), json);
        }

        public static async Task<BlockData> GetData()
        {
            var retval = new BlockData();
            try
            {
                var client = new HttpClient();
                var resp = await client.GetAsync(dataUrl);
                if (resp != null && resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    var cloudData = JsonConvert.DeserializeObject<BlockData>(json);
                    if (cloudData != null)
                    {
                        foreach (var weapon in cloudData.Weapons)
                        {
                            retval.Weapons[weapon.Key] = weapon.Value;
                        }
                        foreach (var material in cloudData.Materials)
                        {
                            retval.Materials[material.Key] = material.Value;
                        }
                    }
                }
                return retval;
            }
            catch (Exception)
            {
                return retval;
            }
        }
    }
}
