using BlockPenSimWPF.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockPenSimWPF.Shared.Models
{
    internal class SettingsForm
    {
        public Theme ThemeOverride { get; set; }
        public bool UseDecimalComma { get; set; }
        public bool HideZeroRatioWeaponColumns { get; set;}
        public bool HideZeroRatioDirectionColumns { get; set; }
        public bool UpdateDefaultBlockdataOverInternet { get; set; }
        public int WeaponEdit { get; set; }
        public string? WeaponKey { get; set; }
        public string? WeaponName { get; set; }
        public int WeaponCpu { get; set; }
        public double WeaponDamage { get; set; }
        public double WeaponPellets { get; set; }
        public double WeaponRadius { get; set; }
        public double WeaponEnergy { get; set; }
        public double WeaponCooldown { get; set; }
        public int MaterialEdit { get; set; }
        public string? MaterialKey { get; set; }
        public string? MaterialName { get; set; }
        public double MaterialDensity { get; set; }
        public double MaterialConnecitonStrength { get; set; }
        public double MaterialEnergyAbsorption { get; set; }
    }
}
