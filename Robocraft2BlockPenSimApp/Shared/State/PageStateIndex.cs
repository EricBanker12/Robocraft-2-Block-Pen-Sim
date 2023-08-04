using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Robocraft2BlockPenSimApp.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robocraft2BlockPenSimApp.Shared.State
{
    public class PageStateIndex
    {
        public bool ShowSideBar;
        
        public string SortColumn;
        public SortDirection SortDirection;
        
        public double MaxScore;
        public double MaxScorePerCpu;
        public double MaxScorePerWeight;

        public MinMax Cpu;
        public MinMax Weight;
        public MinMax Length;
        public MinMax Width;
        public MinMax Height;

        public double[] WeaponCount = { };
        public double[] WeaponRatio = { };
        public double[] DirectionRatio = { };

        public Weapon[] Weapons = { };
        public Material[] Materials = { };
        public string[] Directions = { };

        private string preferencesKey = "PageStateIndex";

        public void SavePreferences()
        {
            Preferences.Default.Set(preferencesKey, JsonConvert.SerializeObject(this));
        }

        public void LoadPreferences()
        {
            var defaultState = new PageStateIndex()
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

            try
            {
                var pref = JsonConvert.DeserializeObject<PageStateIndex>(Preferences.Default.Get(preferencesKey, JsonConvert.SerializeObject(defaultState)));
                this.Cpu = pref.Cpu;
                this.Weight = pref.Weight;
                this.Length = pref.Length;
                this.Width = pref.Width;
                this.Height = pref.Height;
                this.WeaponCount = pref.WeaponCount;
                this.WeaponRatio = pref.WeaponRatio;
                this.DirectionRatio = pref.DirectionRatio;
            }
            catch (Exception)
            {
                this.Cpu = defaultState.Cpu;
                this.Weight = defaultState.Weight;
                this.Length = defaultState.Length;
                this.Width = defaultState.Width;
                this.Height = defaultState.Height;
                this.WeaponCount = defaultState.WeaponCount;
                this.WeaponRatio = defaultState.WeaponRatio;
                this.DirectionRatio = defaultState.DirectionRatio;

                SavePreferences();
            }
        }
    }
}
