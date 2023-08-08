using BlockPenSimWPF.Data;
using BlockPenSimWPF.Shared.Models;
using Newtonsoft.Json;
using System.Data;

namespace BlockPenSimWPF.Shared.State
{
    public class IndexStore
    {
        public IndexStore() { }
        public IndexStore(Action StateHasChanged)
        {
            this.HasChanged = () => { SavePreferences(); StateHasChanged(); };
        }

        // State not persistent between sessions
        [JsonIgnore]
        public readonly Action HasChanged = () => { throw new NotImplementedException("IndexStore missing method HasChanged."); };

        private DataTable? simData;

        [JsonIgnore]
        public DataTable SimData
        {
            get
            {
                if (simData == null)
                    simData = BlockPenSimulator.Run(this);

                return simData;
            }
            set { simData = value; }
        }

        [JsonIgnore]
        public bool ShowSideBar;
        
        [JsonIgnore]
        public bool ShowSettings;

        // settings
        public bool useDecimalComma = false;
        public bool hideZeroRatioWeaponColumns = true;
        public bool hideZeroRatioDirectionColumns = true;
        public bool updateDefaultBlockdataOverInternet = false;

        // highlighting
        public Dictionary<string, bool> ColumnsHighlightMaxValue = new() { {"Score", true}, {"Score / CPU", true}, {"Score / Weight", true} };

        // blockfill constraints
        public MinMax Cpu = new MinMax { Min = 0, Max = 100, };
        public MinMax Weight = new MinMax { Min = 0, Max = 4000, };
        public MinMax Length = new MinMax { Min = 4, Max = 9, };
        public MinMax Width = new MinMax { Min = 9, Max = 9, };
        public MinMax Height = new MinMax { Min = 9, Max = 9, };

        // scoring ratios
        public Dictionary<string, double> WeaponCount = new() { {"LaserBlaster", 6}, {"PlasmaCannon", 2}, {"ArcDischarger", 3}, {"RailGun", 1} };
        public Dictionary<string, double> WeaponRatio = new() { {"LaserBlaster", 1}, {"PlasmaCannon", 1}, {"ArcDischarger", 0}, {"RailGun", 0} };
        public double[] DirectionRatio = { 8, 1, 1 };

        // block data
        public Dictionary<string, Weapon> Weapons = BlockData.DefaultWeapons;
        public Dictionary<string, Material> Materials = BlockData.DefaultMaterials;

        // key for local storage
        private string storageKey = "IndexStore";

        public void SavePreferences()
        {
            LocalSettings.SetValue(storageKey, this);
        }

        public void LoadPreferences()
        {
            try
            {
                var settings = LocalSettings.GetValue<IndexStore>(storageKey);
                if (settings != null)
                {
                    this.useDecimalComma = settings.useDecimalComma;
                    this.hideZeroRatioWeaponColumns = settings.hideZeroRatioWeaponColumns;
                    this.hideZeroRatioDirectionColumns = settings.hideZeroRatioDirectionColumns;

                    this.Cpu = settings.Cpu;
                    this.Weight = settings.Weight;
                    this.Length = settings.Length;
                    this.Width = settings.Width;
                    this.Height = settings.Height;

                    this.DirectionRatio = settings.DirectionRatio;

                    foreach (var Weapon in settings.Weapons)
                    {
                        this.Weapons[Weapon.Key] = Weapon.Value;
                        this.WeaponCount[Weapon.Key] = settings.WeaponCount[Weapon.Key];
                        this.WeaponRatio[Weapon.Key] = settings.WeaponRatio[Weapon.Key];
                    }
                    foreach (var Material in settings.Materials)
                    {
                        this.Materials[Material.Key] = Material.Value;
                    }
                }
            }
            catch (Exception)
            {
                SavePreferences();
            }
        }
    }
}
