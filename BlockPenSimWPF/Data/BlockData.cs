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
            {"LaserBlaster", new Weapon { name = "Laser Blaster", cpu = 20, damage = 24.0, pellets = 1.0, radius = 0.3, energy = 2000.0, cooldown = 0.6, impulse = 2750, splashShape = SplashShape.None  } },
            {"PlasmaCannonMax", new Weapon { name = "Plasma Cannon (37.5m)", cpu = 60, damage = 600.0, pellets = 9.0, radius = 25.0, energy = 1000.0, cooldown = 2.5, impulse = 2750, splashShape = SplashShape.Cone } },
            {"PlasmaCannon", new Weapon { name = "Plasma Cannon (78.75m)", cpu = 60, damage = 340.0, pellets = 9.0, radius = 25.0, energy = 1000.0, cooldown = 2.5, impulse = 2750, splashShape = SplashShape.Cone } },
            {"PlasmaCannonMin", new Weapon { name = "Plasma Cannon (120m)", cpu = 60, damage = 80.0, pellets = 9.0, radius = 25.0, energy = 1000.0, cooldown = 2.5, impulse = 2750, splashShape = SplashShape.Cone } },
            {"ArcDischarger", new Weapon{ name = "Arc Discharger", cpu = 40, damage = 15000.0, pellets = 11.0, radius = 30.0, energy = 120000.0, cooldown = 6.0, impulse = 5000, splashShape = SplashShape.Cone  } },
            {"RailGunMin", new Weapon      { name = "Rail Gun (25m)", cpu = 120, damage = 150.0, pellets = 1.0, radius = 0.3, energy = 200.0, cooldown = 3.5, impulse = 2750, splashShape = SplashShape.Cylinder  } },
            {"RailGun", new Weapon      { name = "Rail Gun (47.5m)", cpu = 120, damage = 480.0, pellets = 12.0, radius = 1.5, energy = 2350.0, cooldown = 3.5, impulse = 2750, splashShape = SplashShape.Cylinder  } },
            {"RailGunMax", new Weapon      { name = "Rail Gun (70m)", cpu = 120, damage = 810.0, pellets = 12.0, radius = 3.0, energy = 4500.0, cooldown = 3.5, impulse = 2750, splashShape = SplashShape.Cylinder  } },
        };

        public static Dictionary<string, Material> DefaultMaterials = new Dictionary<string, Material>()
        {
            {"Airium", new Material     { name = "Airium", density = 0.728, connectionStrength = 1.59, energyAbsorption = 2500.0 } },
            {"Moderonium", new Material { name = "Moderonium", density = 1.8, connectionStrength = 2.65, energyAbsorption = 5000.0 } },
            {"Ladium", new Material     { name = "Ladium", density = 7.32, connectionStrength = 3.35225, energyAbsorption = 7500.0 } },
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
