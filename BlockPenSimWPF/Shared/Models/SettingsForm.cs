using BlockPenSimWPF.Data;
using BlockPenSimWPF.Shared.State;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BlockPenSimWPF.Shared.Models
{
    internal class SettingsForm
    {
        public void Reset(IndexStore State)
        {
            // Set initial values
            this.ThemeOverride = ThemeData.ThemeOverride;
            this.UseDecimalComma = State.useDecimalComma;
            this.HideZeroRatioWeaponColumns = State.hideZeroRatioWeaponColumns;
            this.HideZeroRatioDirectionColumns = State.hideZeroRatioDirectionColumns;
            this.UpdateDefaultBlockdataOverInternet = State.updateDefaultBlockdataOverInternet;

            // duplicate weapons/materials
            this.Weapons = State.Weapons.ToDictionary(e => e.Key, e => e.Value);
            this.Materials = State.Materials.ToDictionary(e => e.Key, e => e.Value);

            // set select to None
            this.weaponEdit = State.Weapons.Count + 1;
            this.weaponKey = string.Empty;
            this.weaponName = string.Empty;
            this.weaponCpu = 20;
            this.weaponDamage = 13;
            this.weaponPellets = 1;
            this.weaponRadius = 0.3;
            this.weaponEnergy = 2000;
            this.weaponCooldown = 0.6;

            this.materialEdit = State.Materials.Count + 1;
            this.materialKey = string.Empty;
            this.materialName = string.Empty;
            this.materialDensity = 1.8;
            this.materialConnectionStrength = 2.65;
            this.materialEnergyAbsorption = 5000;

            this.ErrorMessages.Clear();
        }

        public bool WasValid(string fieldName)
        {
            if (fieldName.StartsWith("Weapon") && WeaponEdit == this.Weapons?.Count + 1)
                return true;

            if (fieldName.StartsWith("Material") && MaterialEdit == this.Weapons?.Count + 1)
                return true;

            return !ErrorMessages.ContainsKey(fieldName) || ErrorMessages[fieldName].Count == 0;
        }

        private void OverrideWasValid(string fieldName)
        {
            if (ErrorMessages.ContainsKey(fieldName)) ErrorMessages[fieldName].Clear();
        }

        public bool IsValid(string fieldName)
        {
            if (ErrorMessages.ContainsKey(fieldName)) ErrorMessages[fieldName].Clear();
            else ErrorMessages[fieldName] = new List<string>();

            if (fieldName.StartsWith("Weapon") && WeaponEdit == this.Weapons?.Count + 1)
                return true;

            if (fieldName.StartsWith("Material") && MaterialEdit == this.Materials?.Count + 1)
                return true;

            switch (fieldName)
            {
                case nameof(WeaponKey):
                    if (string.IsNullOrWhiteSpace(WeaponKey) || WeaponKey?.Length > 255 || WeaponKey?.Length < 3)
                        ErrorMessages[fieldName].Add("Key must be between 3 and 255 characters in length.");
                    else if (WeaponEdit <= this.Weapons?.Count)
                    {
                        var matches = Weapons.Where(kv => kv.Key == WeaponKey);
                        if (matches.Count() > 1) ErrorMessages[fieldName].Add("Key must be unique.");
                    }
                    break;
                case nameof(WeaponName):
                    if (string.IsNullOrWhiteSpace(WeaponName) || WeaponName?.Length > 255 || WeaponName?.Length < 3)
                        ErrorMessages[fieldName].Add("Name must be between 3 and 255 characters in length.");
                    else if (weaponEdit <= this.Weapons?.Count)
                    {
                        var matches = Weapons.Where(kv => kv.Value.name.Equals(WeaponName, StringComparison.OrdinalIgnoreCase));
                        if (matches.Count() > 1) ErrorMessages[fieldName].Add("Name must be unique.");
                    }
                    break;
                case nameof(WeaponCpu):
                    if (WeaponCpu < 1) ErrorMessages[fieldName].Add("CPU Cost must be at least 1.");
                    break;
                case nameof(WeaponDamage):
                    if (WeaponDamage < 1) ErrorMessages[fieldName].Add("Damage must be at least 1.");
                    break;
                case nameof(WeaponPellets):
                    if (WeaponPellets < 1) ErrorMessages[fieldName].Add("Pellet Count must be at least 1.");
                    break;
                case nameof(WeaponCooldown):
                    if (WeaponCooldown < 0.01) ErrorMessages[fieldName].Add("Cooldown cannot be less than one-hundredth (1/100) of a second.");
                    break;
                case nameof(WeaponEnergy):
                    if (WeaponEnergy < 1) ErrorMessages[fieldName].Add("Penetration Energy must be at least 1 kJ/m.");
                    break;
                case nameof(WeaponRadius):
                    if (WeaponRadius < 0) ErrorMessages[fieldName].Add("Radius cannot be less than 0 blocks.");
                    break;
                case nameof(MaterialKey):
                    if (string.IsNullOrWhiteSpace(MaterialKey) || MaterialKey?.Length > 255 || MaterialKey?.Length < 3)
                        ErrorMessages[fieldName].Add("Key must be between 3 and 255 characters in length.");
                    else if (MaterialEdit <= this.Materials?.Count)
                    {
                        var matches = Materials.Where(kv => kv.Key == MaterialKey);
                        if (matches.Count() > 1) ErrorMessages[fieldName].Add("Key must be unique.");
                    }
                    break;
                case nameof(MaterialName):
                    if (string.IsNullOrWhiteSpace(MaterialName) || MaterialName?.Length > 255 || MaterialName?.Length < 3)
                        ErrorMessages[fieldName].Add("Name must be between 3 and 255 characters in length.");
                    else if (materialEdit <= this.Materials?.Count)
                    {
                        var matches = Materials.Where(kv => kv.Value.name.Equals(MaterialName, StringComparison.OrdinalIgnoreCase));
                        if (matches.Count() > 1) ErrorMessages[fieldName].Add("Name must be unique.");
                    }
                    break;
                case nameof(MaterialDensity):
                    if (MaterialDensity < 0.01) ErrorMessages[fieldName].Add("Mass cannot be less than one-hundredth (1/100) of a kilogram.");
                    break;
                case nameof(MaterialConnectionStrength):
                    if (MaterialConnectionStrength < 0) ErrorMessages[fieldName].Add("Connection Strength cannot be less than 0.");
                    break;
                case nameof(MaterialEnergyAbsorption):
                    if (MaterialEnergyAbsorption < 0) ErrorMessages[fieldName].Add("Penetration Energy Absorption cannot be less than 0 kJ/m.");
                    break;
                default:
                    break;
            }

            return !ErrorMessages.ContainsKey(fieldName) || ErrorMessages[fieldName].Count == 0;
        }

        public bool IsValid()
        {
            return  IsValidWeapon() && IsValidMaterial();
        }

        public bool IsValidWeapon()
        {
            if (!IsValid(nameof(WeaponKey))) return false;
            if (!IsValid(nameof(WeaponName))) return false;
            if (!IsValid(nameof(WeaponCpu))) return false;
            if (!IsValid(nameof(WeaponDamage))) return false;
            if (!IsValid(nameof(WeaponPellets))) return false;
            if (!IsValid(nameof(WeaponRadius))) return false;
            if (!IsValid(nameof(WeaponEnergy))) return false;
            if (!IsValid(nameof(WeaponCooldown))) return false;

            return true;
        }

        public bool IsValidMaterial()
        {
            if (!IsValid(nameof(MaterialKey))) return false;
            if (!IsValid(nameof(MaterialName))) return false;
            if (!IsValid(nameof(MaterialDensity))) return false;
            if (!IsValid(nameof(MaterialEnergyAbsorption))) return false;
            if (!IsValid(nameof(MaterialConnectionStrength))) return false;
            return true;
        }

        public void OnChange_WeaponEdit()
        {
            if (this.WeaponEdit < this.Weapons?.Count)
            {
                var weapon = this.Weapons.ToArray()[this.WeaponEdit];
                this.WeaponKey = weapon.Key;
                this.WeaponName = weapon.Value.name;
                this.WeaponDamage = weapon.Value.damage;
                this.WeaponCpu = weapon.Value.cpu;
                this.WeaponPellets = weapon.Value.pellets;
                this.WeaponRadius = weapon.Value.radius;
                this.WeaponEnergy = weapon.Value.energy;
                this.weaponCooldown = weapon.Value.cooldown;
            }
            else
            {
                this.weaponKey = string.Empty;
                this.weaponName = string.Empty;
                OverrideWasValid(nameof(WeaponKey));
                OverrideWasValid(nameof(WeaponName));
            }
        }

        public void OnChange_MaterialEdit()
        {
            if (this.MaterialEdit < this.Materials?.Count)
            {
                var material = this.Materials.ToArray()[this.MaterialEdit];
                this.MaterialKey = material.Key;
                this.MaterialName = material.Value.name;
                this.MaterialDensity = material.Value.density;
                this.MaterialConnectionStrength = material.Value.connectionStrength;
                this.MaterialEnergyAbsorption = material.Value.energyAbsorption;
            }
            else
            {
                this.materialKey = string.Empty;
                this.materialName = string.Empty;
                OverrideWasValid(nameof(MaterialKey));
                OverrideWasValid(nameof(MaterialName));
            }
        }

        public void AddWeapon()
        {
            if (this.Weapons == null) return;

            if (IsValidWeapon())
            {
                this.Weapons.Add(WeaponKey, new Weapon()
                {
                    name = WeaponName,
                    cpu = WeaponCpu,
                    damage = WeaponDamage,
                    pellets = WeaponPellets,
                    cooldown = WeaponCooldown,
                    energy = WeaponEnergy,
                    radius = WeaponRadius,
                });
            }
        }

        public void AddMaterial()
        {
            if (this.Materials == null) return;

            if (IsValidMaterial())
            {
                this.Materials.Add(MaterialKey, new Material()
                {
                    name = MaterialName,
                    density = MaterialDensity,
                    connectionStrength = MaterialConnectionStrength,
                    energyAbsorption = MaterialEnergyAbsorption,
                });
            }
        }

        public void DeleteWeapon()
        {
            if (this.Weapons == null) return;
            var weapon = this.Weapons.ToArray()[this.WeaponEdit];
            this.Weapons.Remove(weapon.Key);
            this.WeaponEdit = this.Weapons.Count + 1;
        }

        public void DeleteMaterial()
        {
            if (this.Materials == null) return;
            var material = this.Materials.ToArray()[this.MaterialEdit];
            this.Materials.Remove(material.Key);
            this.MaterialEdit = this.Materials.Count + 1;
        }

        public List<string> GetErrorMessages()
        {
            var errorMessages = new List<string>();
            foreach (var kv in this.ErrorMessages)
            {
                errorMessages.AddRange(kv.Value);
            }
            return errorMessages;
        }

        public List<string> GetErrorMessages(string fieldName)
        {
            List<string>? errorMessages;
            if (this.ErrorMessages.TryGetValue(fieldName, out errorMessages))
            {
                return errorMessages;
            }
            else
            {
                return new List<string>();
            }
        }
        
        private Dictionary<string, List<string>> ErrorMessages = new();

        // Basic Settings
        public Theme ThemeOverride { get; set; }
        public bool UseDecimalComma { get; set; }
        public bool HideZeroRatioWeaponColumns { get; set;}
        public bool HideZeroRatioDirectionColumns { get; set; }
        public bool UpdateDefaultBlockdataOverInternet { get; set; }

        // Weapon Edit
        public Dictionary<string, Weapon> Weapons { get; set; } = new();

        private int weaponEdit;
        public int WeaponEdit {
            get { return weaponEdit; }
            set {
                weaponEdit = value;
                OnChange_WeaponEdit();
            }
        }

        private string weaponKey = string.Empty;
        public string WeaponKey
        {
            get { return weaponKey; }
            set {
                weaponKey = value;
                IsValid(nameof(WeaponKey));
            }
        }
        
        private string weaponName = string.Empty;
        public string WeaponName
        {
            get { return weaponName; }
            set
            {
                weaponName = value;
                IsValid(nameof(WeaponName));
            }
        }
        
        private int weaponCpu = 1;
        public int WeaponCpu
        {
            get { return weaponCpu; }
            set
            {
                weaponCpu = value;
                IsValid(nameof(WeaponCpu));
            }
        }

        private double weaponDamage = 1;
        public double WeaponDamage
        {
            get { return weaponDamage; }
            set
            {
                weaponDamage = value;
                IsValid(nameof(WeaponDamage));
            }
        }
        
        private double weaponPellets = 1;
        public double WeaponPellets
        {
            get { return weaponPellets; }
            set
            {
                weaponPellets = value;
                IsValid(nameof(WeaponPellets));
            }
        }

        private double weaponRadius = 0;
        public double WeaponRadius
        {
            get { return weaponRadius; }
            set
            {
                weaponRadius = value;
                IsValid(nameof(WeaponRadius));
            }
        }

        private double weaponEnergy = 1;
        public double WeaponEnergy
        {
            get { return weaponEnergy; }
            set
            {
                weaponEnergy = value;
                IsValid(nameof(WeaponEnergy));
            }
        }

        private double weaponCooldown = 0.01;
        public double WeaponCooldown
        {
            get { return weaponCooldown; }
            set
            {
                weaponCooldown = value;
                IsValid(nameof(WeaponCooldown));
            }
        }

        // Material Edit
        public Dictionary<string, Material> Materials { get; set; } = new();
        
        private int materialEdit;
        public int MaterialEdit {
            get { return materialEdit; }
            set
            {
                materialEdit = value;
                OnChange_MaterialEdit();
            }
        }
        
        private string materialKey = string.Empty;
        public string MaterialKey
        {
            get { return materialKey; }
            set
            {
                materialKey = value;
                IsValid(nameof(MaterialKey));
            }
        }
        
        private string materialName = string.Empty;
        public string MaterialName
        {
            get { return materialName; }
            set
            {
                materialName = value;
                IsValid(nameof(MaterialName));
            }
        }

        private double materialDensity = 0.01;
        public double MaterialDensity
        {
            get { return materialDensity; }
            set
            {
                materialDensity = value;
                IsValid(nameof(MaterialDensity));
            }
        }

        private double materialConnectionStrength = 0;
        public double MaterialConnectionStrength
        {
            get => materialConnectionStrength;
            set
            {
                materialConnectionStrength = value;
                IsValid(nameof(MaterialConnectionStrength));
            }
        }

        private double materialEnergyAbsorption = 0;
        public double MaterialEnergyAbsorption
        {
            get => materialEnergyAbsorption;
            set
            {
                materialEnergyAbsorption = value;
                IsValid(nameof(MaterialEnergyAbsorption));
            }
        }
    }
}
