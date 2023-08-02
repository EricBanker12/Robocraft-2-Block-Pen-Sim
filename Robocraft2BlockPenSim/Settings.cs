using IniParser;
using IniParser.Model;
using Newtonsoft.Json;
using static Robocraft2BlockPenSim.Types;

namespace Robocraft2BlockPenSim
{
    internal class Settings
    {
        // ------------------------------------------------------------------------------------------------------------------------
        // Default values, used to generate config files if missing.
        // ------------------------------------------------------------------------------------------------------------------------
        private static readonly Material[] defaultMaterials =
        {
            new Material { name = "Airium", density = 0.7, connectionStrength = 1.59, energyAbsorption = 2500 },
            new Material { name = "Moderonium", density = 1.8, connectionStrength = 2.65, energyAbsorption = 5000 },
            new Material { name = "Ladium", density = 7.3, connectionStrength = 3.35, energyAbsorption = 6500 },
        };

        private static readonly Weapon[] defaultWeapons =
        {
            new Weapon { name = "Laser Blaster", cpu = 20, damage = 13.0, pellets = 1.0, energy = 2000.0, radius = 0.3, cooldown = 0.6 },
            new Weapon { name = "Plasma Cannon", cpu = 60, damage = 500.0, pellets = 10.0, energy = 1000.0, radius = 12.5, cooldown = 2.0 },
            new Weapon { name = "Arc Discharger", cpu = 40, damage = 15000.0, pellets = 20.0, energy = 120000.0, radius = 30.0, cooldown = 6.0 },
            new Weapon { name = "Rail Gun", cpu = 120, damage = 950.0, pellets = 1.0, energy = 10000.0, radius = 3.0, cooldown = 4.0 },
        };

        private static readonly BlockFillConstraints defaultBlockFillConstraints = new()
        {
            cpuMin = 0,
            cpuMax = 50,
            weightMin = 0.0,
            weightMax = 4000.0,
            lengthMin = 4.0,
            lengthMax = 18.0,
            widthMin = 9.0,
            widthMax = 9.0,
            heightMin = 9.0,
            heightMax = 9.0,
        };

        private static readonly DamageDirectionRatio defaultDamageDirectionRatio = new()
        {
            front = 2,
            side = 1,
            top = 1,
        };
        // ------------------------------------------------------------------------------------------------------------------------
        // public/internal properties/fields
        // ------------------------------------------------------------------------------------------------------------------------
        internal Weapon[] weapons;
        internal Material[] materials;
        internal BlockFillConstraints blockFillConstraints;
        internal DamageDirectionRatio damageDirectionRatio;
        internal DamageWeaponCount damageWeaponCount;
        internal DamageWeaponRatio damageWeaponRatio;
        internal string directory;
        // ------------------------------------------------------------------------------------------------------------------------
        // Constructor
        // ------------------------------------------------------------------------------------------------------------------------
        internal Settings()
        {
            directory = AppContext.BaseDirectory;

            // Read JSON
            var jsonPath = Path.Combine(directory, "Robocraft2BlockData.json");
            try
            {
                var blockDataJson = File.ReadAllText(jsonPath);
                if (string.IsNullOrWhiteSpace(blockDataJson)) throw new ApplicationException("File was empty!");
                var blockData = JsonConvert.DeserializeObject<BlockData>(blockDataJson);

                weapons = blockData.weapons;
                materials = blockData.materials;
            }
            // Write default values to JSON
            catch (Exception ex)
            {
                weapons = defaultWeapons;
                materials = defaultMaterials;

                var blockData = new BlockData() { weapons = weapons, materials = materials };
                var blockDataJson = JsonConvert.SerializeObject(blockData, Formatting.Indented);
                File.WriteAllText(jsonPath, blockDataJson);
            }

            // Read INI
            var ini = new FileIniDataParser();
            ini.Parser.Configuration.AllowDuplicateKeys = true;
            ini.Parser.Configuration.AllowDuplicateSections = true;
            ini.Parser.Configuration.CaseInsensitive = true;
            ini.Parser.Configuration.CommentString = "#";

            string sectionName;
            var iniPath = Path.Combine(directory, "Robocraft2BlockPenSimConfig.ini");
            try
            {
                var data = ini.ReadFile(iniPath);
                var updated = false;
                
                sectionName = "BlockFillConstraints";
                blockFillConstraints = new BlockFillConstraints()
                {
                    cpuMin = int.Parse(data[sectionName]["cpuMin"]),
                    cpuMax = int.Parse(data[sectionName]["cpuMax"]),
                    weightMin = double.Parse(data[sectionName]["weightMin"]),
                    weightMax = double.Parse(data[sectionName]["weightMax"]),
                    lengthMin = double.Parse(data[sectionName]["lengthMin"]),
                    lengthMax = double.Parse(data[sectionName]["lengthMax"]),
                    widthMin = double.Parse(data[sectionName]["widthMin"]),
                    widthMax = double.Parse(data[sectionName]["widthMax"]),
                    heightMin = double.Parse(data[sectionName]["heightMin"]),
                    heightMax = double.Parse(data[sectionName]["heightMax"])
                };

                sectionName = "DamageDirectionRatio";
                damageDirectionRatio = new DamageDirectionRatio()
                {
                    front = double.Parse(data[sectionName]["front"]),
                    side = double.Parse(data[sectionName]["side"]),
                    top = double.Parse(data[sectionName]["top"])
                };

                sectionName = "DamageWeaponCount";
                damageWeaponCount = new DamageWeaponCount();
                foreach (var weapon in weapons)
                {
                    var weaponName = weapon.name.Replace(" ", "");
                    var value = data[sectionName][weaponName];
                    if (!string.IsNullOrWhiteSpace(value))
                        damageWeaponCount.Add(weaponName, double.Parse(value));
                    else
                    {
                        var defaultValue = 120.0 / weapon.cpu;
                        damageWeaponCount.Add(weaponName, defaultValue);
                        data[sectionName][weaponName] = defaultValue.ToString();
                        updated = true;
                    }
                }

                sectionName = "DamageWeaponRatio";
                damageWeaponRatio = new DamageWeaponRatio();
                foreach (var weapon in weapons)
                {
                    var weaponName = weapon.name.Replace(" ", "");
                    var value = data[sectionName][weaponName];
                    if (!string.IsNullOrWhiteSpace(value))
                        damageWeaponRatio.Add(weaponName, double.Parse(value));
                    else
                    {
                        var defaultValue = 1.0;
                        damageWeaponRatio.Add(weaponName, defaultValue);
                        data[sectionName][weaponName] = defaultValue.ToString();
                        updated = true;
                    }
                }

                if (updated) ini.WriteFile(iniPath, data);
            }
            // Write default values to INI
            catch (Exception ex)
            {
                var data = new IniData();
                data.Configuration.CommentString = "#";
                data.Configuration.AllowDuplicateKeys = true;

                blockFillConstraints = defaultBlockFillConstraints;
                damageDirectionRatio = defaultDamageDirectionRatio;
                damageWeaponCount = new DamageWeaponCount();
                damageWeaponRatio = new DamageWeaponRatio();
                
                sectionName = "BlockFillConstraints";
                data.Sections.AddSection(sectionName);
                data.Sections.GetSectionData(sectionName).Comments.Add("Set minimum and maximum constraints for size, weight, and CPU cost to fill with blocks and simulate taking weapon damage.");
                data[sectionName]["cpuMin"] = blockFillConstraints.cpuMin.ToString();
                data[sectionName]["cpuMax"] = blockFillConstraints.cpuMax.ToString();
                data[sectionName]["weightMin"] = blockFillConstraints.weightMin.ToString();
                data[sectionName]["weightMax"] = blockFillConstraints.weightMax.ToString();
                data[sectionName].GetKeyData("weightMin").Comments.Add("Weight should be given in kilograms (kg).");
                data[sectionName]["lengthMin"] = blockFillConstraints.lengthMin.ToString();
                data[sectionName]["lengthMax"] = blockFillConstraints.lengthMax.ToString();
                data[sectionName].GetKeyData("lengthMin").Comments.Add("Length is the size (in cubes) between front and back sides.");
                data[sectionName]["widthMin"] = blockFillConstraints.widthMin.ToString();
                data[sectionName]["widthMax"] = blockFillConstraints.widthMax.ToString();
                data[sectionName].GetKeyData("widthMin").Comments.Add("Width is the size between left and right sides.");
                data[sectionName]["heightMin"] = blockFillConstraints.heightMin.ToString();
                data[sectionName]["heightMax"] = blockFillConstraints.heightMax.ToString();
                data[sectionName].GetKeyData("heightMin").Comments.Add("Height is the size between bottom and top sides.");

                sectionName = "DamageDirectionRatio";
                data.Sections.AddSection(sectionName);
                data.Sections.GetSectionData(sectionName).Comments.Add("Set what proportion of incomming damage comes from which direction.");
                data.Sections.GetSectionData(sectionName).Comments.Add("Values are treated as a ratio, so 1:1:1 is equal to 0.333:0.333:0.333");
                data[sectionName]["front"] = damageDirectionRatio.front.ToString();
                data[sectionName]["side"] = damageDirectionRatio.side.ToString();
                data[sectionName]["top"] = damageDirectionRatio.top.ToString();

                sectionName = "DamageWeaponCount";
                data.Sections.AddSection(sectionName);
                data.Sections.GetSectionData(sectionName).Comments.Add("Set weapon count for calculating Time-To-Penetrate (TTP) from simulation Shots-To-Penetrate (STP).");
                foreach (Weapon weapon in weapons)
                {
                    var weaponName = weapon.name.Replace(" ", "");
                    var defaultValue = 120.0 / weapon.cpu;
                    damageWeaponCount.Add(weaponName, defaultValue);
                    data[sectionName][weaponName] = defaultValue.ToString();
                }

                sectionName = "DamageWeaponRatio";
                data.Sections.AddSection(sectionName);
                data.Sections.GetSectionData(sectionName).Comments.Add("Set what proportion of incomming damage comes from which weapon count.");
                data.Sections.GetSectionData(sectionName).Comments.Add("Values are treated as a ratio, so 1:1:1 is equal to 0.333:0.333:0.333");
                for (int w = 0; w < weapons.Length; w++)
                {
                    var weapon = weapons[w];
                    var weaponName = weapon.name.Replace(" ", "");
                    var defaultValue = w < 2 ? 1.0 : 0.0;
                    damageWeaponRatio.Add(weaponName, defaultValue);
                    data[sectionName][weaponName] = defaultValue.ToString();
                }

                ini.WriteFile(iniPath, data);
            }
        }
    }
}
