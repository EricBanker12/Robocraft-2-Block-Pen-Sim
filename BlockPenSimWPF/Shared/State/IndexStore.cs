using BlockPenSimWPF.Data;
using BlockPenSimWPF.Shared.Models;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.DirectoryServices;
using SortDirection = BlockPenSimWPF.Shared.Models.SortDirection;

namespace BlockPenSimWPF.Shared.State
{
    public class IndexStore
    {
        // ------------------------------------------------------------------------------------------------------------------------
        // Constructors
        // ------------------------------------------------------------------------------------------------------------------------
        public IndexStore() { }

        public IndexStore(Action StateHasChanged)
        {
            this.HasChanged = () => { this.SavePreferences(); StateHasChanged(); };
        }

        // ------------------------------------------------------------------------------------------------------------------------
        // State not persistent between sessions
        // ------------------------------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public readonly Action HasChanged = () => { throw new NotImplementedException("IndexStore missing method HasChanged."); };

        private DataTable? simData;

        [JsonIgnore]
        public DataTable SimData
        {
            get
            {
                simData ??= BlockPenSimulator.CreateSchema(this);
                return simData;
            }
            set { simData = value; }
        }

        [JsonIgnore]
        public bool ShowSideBar;
        
        [JsonIgnore]
        public bool ShowSettings;

        [JsonIgnore]
        public bool ShowLoading = true;

        [JsonIgnore]
        public bool IsDarkMode;

        [JsonIgnore]
        public List<Action> OnResetPreferences = new();

        // ------------------------------------------------------------------------------------------------------------------------
        // settings
        // ------------------------------------------------------------------------------------------------------------------------
        public bool useDecimalComma = false;
        public bool hideZeroRatioWeaponColumns = false;
        public bool hideZeroRatioDirectionColumns = false;
        public bool simulateWithScaledPostStraights = false;
        public bool updateDefaultBlockdataOverInternet = false;
        public bool applyKilledBlockCollisionDamage = false;

        // ------------------------------------------------------------------------------------------------------------------------
        // Results view
        // ------------------------------------------------------------------------------------------------------------------------
        public Dictionary<string, bool> HighlightValues = new();
        public Dictionary<string, string> RowFilters = new();
        public OrderedDictionary ColumnsSort = new();

        // ------------------------------------------------------------------------------------------------------------------------
        // blockfill constraints
        // ------------------------------------------------------------------------------------------------------------------------
        public MinMax Cpu = new();
        public MinMax Weight = new();
        public MinMax Length = new();
        public MinMax Width = new();
        public MinMax Height = new();

        // ------------------------------------------------------------------------------------------------------------------------
        // scoring ratios
        // ------------------------------------------------------------------------------------------------------------------------
        public Dictionary<string, WeaponSettings> WeaponSettings = new();

        // ------------------------------------------------------------------------------------------------------------------------
        // block data
        // ------------------------------------------------------------------------------------------------------------------------
        public Dictionary<string, Weapon> Weapons = new();
        public Dictionary<string, Material> Materials = new();

        // ------------------------------------------------------------------------------------------------------------------------
        // key for local storage
        // ------------------------------------------------------------------------------------------------------------------------
        private string storageKey = "IndexStore";

        // ------------------------------------------------------------------------------------------------------------------------
        // Public Methods
        // ------------------------------------------------------------------------------------------------------------------------
        public void SavePreferences()
        {
            LocalSettings.SetValue(storageKey, this);
        }

        public void ResetPreferences()
        {
            LocalSettings.Reset(storageKey);
            LoadPreferences();
            foreach(var action in OnResetPreferences)
            {
                action();
            }
        }

        public void LoadPreferences()
        {
            try
            {
                this.IsDarkMode = ThemeData.GetCurrentTheme() == Theme.Dark;
                
                var settings = LocalSettings.GetValue<IndexStore>(storageKey);
                if (settings != null)
                {

                    this.useDecimalComma = settings.useDecimalComma;
                    this.hideZeroRatioWeaponColumns = settings.hideZeroRatioWeaponColumns;
                    this.hideZeroRatioDirectionColumns = settings.hideZeroRatioDirectionColumns;
                    this.simulateWithScaledPostStraights = settings.simulateWithScaledPostStraights;
                    this.updateDefaultBlockdataOverInternet = settings.updateDefaultBlockdataOverInternet;
                    this.applyKilledBlockCollisionDamage = settings.applyKilledBlockCollisionDamage;

                    this.HighlightValues = settings.HighlightValues;
                    this.RowFilters = settings.RowFilters;
                    
                    this.ColumnsSort.Clear();
                    foreach (DictionaryEntry entry in settings.ColumnsSort)
                    {
                        SortDirection? value = null;
                        if (entry.Value is SortDirection) value = (SortDirection)entry.Value;
                        else if (entry.Value is string) value = (SortDirection)Enum.GetNames(typeof(SortDirection)).ToList().IndexOf((string)entry.Value);
                        else if (entry.Value is long) value = (SortDirection)(long)entry.Value;
                        else if (entry.Value is int) value = (SortDirection)(int)entry.Value;
                        this.ColumnsSort.Add(entry.Key, value);
                    }

                    this.Cpu = settings.Cpu;
                    this.Weight = settings.Weight;
                    this.Length = settings.Length;
                    this.Width = settings.Width;
                    this.Height = settings.Height;

                    this.Weapons.Clear();
                    this.WeaponSettings.Clear();
                    foreach (var Weapon in settings.Weapons)
                    {
                        this.Weapons[Weapon.Key] = Weapon.Value;
                        if (settings.WeaponSettings.ContainsKey(Weapon.Key))
                        {
                            this.WeaponSettings[Weapon.Key] = settings.WeaponSettings[Weapon.Key];
                        }
                        else
                        {
                            this.WeaponSettings[Weapon.Key] = new WeaponSettings()
                            {
                                WeaponCount = 120.0 / Weapon.Value.cpu,
                                WeaponRatio = 1.0,
                                WeaponFrontRatio = 1.0,
                                WeaponSideRatio = 1.0,
                                WeaponTopRatio = 1.0,
                            };
                        }
                    }

                    this.Materials.Clear();
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
