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
                simData ??= BlockPenSimulator.Run(this);
                return simData;
            }
            set { simData = value; }
        }

        [JsonIgnore]
        public bool ShowSideBar;
        
        [JsonIgnore]
        public bool ShowSettings;

        [JsonIgnore]
        public bool IsDarkMode;

        // ------------------------------------------------------------------------------------------------------------------------
        // settings
        // ------------------------------------------------------------------------------------------------------------------------
        public bool useDecimalComma = false;
        public bool hideZeroRatioWeaponColumns = false;
        public bool hideZeroRatioDirectionColumns = false;
        public bool updateDefaultBlockdataOverInternet = false;

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
        public Dictionary<string, double> WeaponCount = new();
        public Dictionary<string, double> WeaponRatio = new();
        public double[] DirectionRatio = { 0, 0, 0 };

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
                    this.updateDefaultBlockdataOverInternet = settings.updateDefaultBlockdataOverInternet;

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

                    this.DirectionRatio = settings.DirectionRatio;

                    this.Weapons.Clear();
                    this.WeaponCount.Clear();
                    this.WeaponRatio.Clear();
                    foreach (var Weapon in settings.Weapons)
                    {
                        this.Weapons[Weapon.Key] = Weapon.Value;
                        this.WeaponCount[Weapon.Key] = settings.WeaponCount[Weapon.Key];
                        this.WeaponRatio[Weapon.Key] = settings.WeaponRatio[Weapon.Key];
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
