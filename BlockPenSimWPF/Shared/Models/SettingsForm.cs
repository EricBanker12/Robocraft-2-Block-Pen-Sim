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
        public EditContext InitAndGetContext(IndexStore State)
        {
            this.Reset(State);
            
            // create context and add custom validation
            this.EditContext = new EditContext(this);
            this.ValidationMessageStore = new ValidationMessageStore(EditContext);
            this.EditContext.OnFieldChanged += OnFieldChanged;
            this.EditContext.OnValidationRequested += OnValidationRequested;
            this.EditContext.SetFieldCssClassProvider(new SettingsFormCssProvider());

            return EditContext;
        }

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
            this.WeaponEdit = State.Weapons.Count + 1;
            this.WeaponKey = string.Empty;
            this.WeaponName = string.Empty;
            this.WeaponCpu = 20;
            this.WeaponDamage = 13;
            this.WeaponPellets = 1;
            this.WeaponRadius = 0.3;
            this.WeaponEnergy = 2000;
            this.WeaponCooldown = 0.6;

            this.MaterialEdit = State.Materials.Count + 1;
            this.MaterialKey = string.Empty;
            this.MaterialName = string.Empty;
            this.MaterialDensity = 1.8;
            this.MaterialConnecitonStrength = 2.65;
            this.MaterialEnergyAbsorption = 5000;
        }

        public bool Validate()
        {
            bool isValid = this.EditContext?.Validate() ?? false;
            if (!isValid)
            {
                isValid = true;
                
                var weaponFields = new string[] { "WeaponKey", "WeaponName", "WeaponCpu", "WeaponDamage", "WeaponPellets", "WeaponRadius", "WeaponEnergy", "WeaponCooldown" };
                for (int i = 0; i < weaponFields.Length && isValid; i++)
                {
                    var fieldIdentifier = new FieldIdentifier(this, weaponFields[i]);
                    if (this.WeaponEdit != this.Weapons?.Count + 1)
                    {
                        isValid = this.EditContext?.GetValidationMessages(fieldIdentifier).Count() == 0;
                    }
                    else
                    {
                        this.ClearValidation(fieldIdentifier);
                    }
                }

                if (!isValid) { return false; }

                var materialFields = new string[] { "MaterialKey", "MaterialName", "MaterialDensity", "MaterialConnecitonStrength", "MaterialEnergyAbsorption" };
                for (int i = 0; i < materialFields.Length && isValid; i++)
                {
                    var fieldIdentifier = new FieldIdentifier(this, materialFields[i]);
                    if (this.MaterialEdit != this.Materials?.Count + 1)
                    {
                        isValid = this.EditContext?.GetValidationMessages(fieldIdentifier).Count() == 0;
                    }
                    else
                    {
                        this.ClearValidation(fieldIdentifier);
                    }
                }

                return isValid;
            }
            return isValid;
        }

        // https://stackoverflow.com/a/63356079
        public void ClearValidation()
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            object GetInstanceField(Type type, object instance, string fieldName)
            {
                var fieldInfo = type.GetField(fieldName, bindingFlags);
                return fieldInfo.GetValue(instance);
            }

            var fieldStates = GetInstanceField(typeof(EditContext), this.EditContext, "_fieldStates");
            var clearMethodInfo = typeof(HashSet<ValidationMessageStore>).GetMethod("Clear", bindingFlags);

            foreach (DictionaryEntry kv in (IDictionary)fieldStates)
            {
                var messageStores = GetInstanceField(kv.Value.GetType(), kv.Value, "_validationMessageStores");
                if (messageStores != null) clearMethodInfo?.Invoke(messageStores, null);
            }
        }

        public void ClearValidation(FieldIdentifier fieldIdentifier)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            object GetInstanceField(Type type, object instance, string fieldName)
            {
                var fieldInfo = type.GetField(fieldName, bindingFlags);
                return fieldInfo.GetValue(instance);
            }

            var fieldStates = GetInstanceField(typeof(EditContext), this.EditContext, "_fieldStates");
            var allMethodInfo = typeof(HashSet<ValidationMessageStore>).GetMethods(bindingFlags);
            var clearMethodInfo = allMethodInfo.Where(info => info.Name == "Clear").Skip(1).FirstOrDefault();

            foreach (DictionaryEntry kv in (IDictionary)fieldStates)
            {
                var messageStores = GetInstanceField(kv.Value.GetType(), kv.Value, "_validationMessageStores");
                if (messageStores != null) clearMethodInfo?.Invoke(messageStores, new object[] { fieldIdentifier });
            }
        }

        private void OnFieldChanged(object? sender, FieldChangedEventArgs args)
        {
            CustomValidate(args.FieldIdentifier);
            if (args.FieldIdentifier.FieldName == "WeaponEdit")
            {
                OnSelect_WeaponEdit();
            }
            else if (args.FieldIdentifier.FieldName == "MaterialEdit")
            {
                OnSelect_MaterialEdit();
            }
        }

        public void OnSelect_WeaponEdit()
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
            }
            else
            {
                this.WeaponKey = string.Empty;
                this.WeaponName = string.Empty;
                this.Validate();
            }
        }

        public void OnSelect_MaterialEdit()
        {
            if (this.MaterialEdit < this.Materials?.Count)
            {
                var material = this.Materials.ToArray()[this.MaterialEdit];
                this.MaterialKey = material.Key;
                this.MaterialName = material.Value.name;
                this.MaterialDensity = material.Value.density;
                this.MaterialConnecitonStrength = material.Value.connectionStrength;
                this.MaterialEnergyAbsorption = material.Value.energyAbsorption;
            }
            else
            {
                this.MaterialKey = string.Empty;
                this.MaterialName = string.Empty;
                this.Validate();
            }
        }

        public void DeleteWeapon()
        {
            var weapon = this.Weapons.ToArray()[this.WeaponEdit];
            this.Weapons?.Remove(weapon.Key);
            this.WeaponEdit = this.Weapons.Count + 1;
            this.OnSelect_WeaponEdit();
        }

        public void DeleteMaterial()
        {
            var material = this.Materials.ToArray()[this.MaterialEdit];
            this.Materials?.Remove(material.Key);
            this.MaterialEdit = this.Materials.Count + 1;
            this.OnSelect_MaterialEdit();
        }

        private void OnValidationRequested(object? sender, ValidationRequestedEventArgs args)
        {
            CustomValidate(new FieldIdentifier(this, "WeaponKey"));
            CustomValidate(new FieldIdentifier(this, "WeaponName"));
            
            CustomValidate(new FieldIdentifier(this, "MaterialKey"));
            CustomValidate(new FieldIdentifier(this, "MaterialName"));
        }

        private void CustomValidate(FieldIdentifier fieldIdentifier)
        {
            this.ValidationMessageStore?.Clear(fieldIdentifier);

            var field = fieldIdentifier.FieldName;
            if (field == "WeaponKey")
            {
                if (string.IsNullOrWhiteSpace(WeaponKey))
                    ValidationMessageStore?.Add(fieldIdentifier, "Weapon Key is required.");
                else
                {
                    if (this.WeaponEdit == this.Weapons?.Count)
                    {
                        if (this.Weapons.ContainsKey(WeaponKey))
                        {
                            ValidationMessageStore?.Add(fieldIdentifier, "Weapon Key must be unique.");
                        }
                    }
                    if (this.WeaponEdit < this.Weapons?.Count)
                    {
                        var keyIndex = this.Weapons?.Keys.ToList().IndexOf(WeaponKey);
                        if (keyIndex > -1 && keyIndex != this.WeaponEdit)
                        {
                            ValidationMessageStore?.Add(fieldIdentifier, "Weapon Key must be unique.");
                        }
                    }
                }
            }
            if (field == "MaterialKey")
            {
                if (string.IsNullOrWhiteSpace(MaterialKey))
                    ValidationMessageStore?.Add(fieldIdentifier, "Material Key is required.");
                else
                {
                    if (this.MaterialEdit == this.Materials?.Count)
                    {
                        if (this.Materials.ContainsKey(MaterialKey))
                        {
                            ValidationMessageStore?.Add(fieldIdentifier, "Material Key must be unique.");
                        }
                    }
                    if (this.MaterialEdit < this.Materials?.Count)
                    {
                        var keyIndex = this.Materials?.Keys.ToList().IndexOf(MaterialKey);
                        if (keyIndex > -1 && keyIndex != this.MaterialEdit)
                        {
                            ValidationMessageStore?.Add(fieldIdentifier, "Material Key must be unique.");
                        }
                    }
                }
            }
            if (field == "WeaponName")
            {
                if (string.IsNullOrWhiteSpace(WeaponName))
                    ValidationMessageStore?.Add(fieldIdentifier, "Weapon Name is required.");
                else
                {
                    if (this.WeaponEdit == this.Weapons?.Count)
                    {
                        if (this.Weapons.Values.Select(w => w.name).Contains(WeaponName))
                        {
                            ValidationMessageStore?.Add(fieldIdentifier, "Weapon Name must be unique.");
                        }
                    }
                    if (this.WeaponEdit < this.Weapons?.Count)
                    {
                        var keyIndex = this.Weapons.Values.Select(w => w.name).ToList().IndexOf(WeaponName);
                        if (keyIndex > -1 && keyIndex != this.WeaponEdit)
                        {
                            ValidationMessageStore?.Add(fieldIdentifier, "Weapon Name must be unique.");
                        }
                    }
                }
            }
            if (field == "MaterialName")
            {
                if (string.IsNullOrWhiteSpace(MaterialName))
                    ValidationMessageStore?.Add(fieldIdentifier, "Material Name is required.");
                else
                {
                    if (this.MaterialEdit == this.Materials?.Count)
                    {
                        if (this.Materials.Values.Select(w => w.name).Contains(MaterialName))
                        {
                            ValidationMessageStore?.Add(fieldIdentifier, "Material Name must be unique.");
                        }
                    }
                    if (this.MaterialEdit < this.Materials?.Count)
                    {
                        var keyIndex = this.Materials?.Values.Select(w => w.name).ToList().IndexOf(MaterialName);
                        if (keyIndex > -1 && keyIndex != this.MaterialEdit)
                        {
                            ValidationMessageStore?.Add(fieldIdentifier, "Material Name must be unique.");
                        }
                    }
                }
            }
        }

        public EditContext? EditContext;
        public ValidationMessageStore? ValidationMessageStore { get; set; }

        // Basic Settings
        public Theme ThemeOverride { get; set; }
        public bool UseDecimalComma { get; set; }
        public bool HideZeroRatioWeaponColumns { get; set;}
        public bool HideZeroRatioDirectionColumns { get; set; }
        public bool UpdateDefaultBlockdataOverInternet { get; set; }
        
        // Weapon Edit
        public Dictionary<string, Weapon>? Weapons { get; set; }
        public int WeaponEdit { get; set; }

        [StringLength(255, MinimumLength = 3, ErrorMessage = "Key must be between 3 and 255 characters in length.")]
        public string? WeaponKey { get; set; }
        
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 255 characters in length.")]
        public string? WeaponName { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "CPU Cost should be at least 1.")]
        public int WeaponCpu { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "Damage should be at least 1.")]
        public double WeaponDamage { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "Pellet Count should be at least 1.")]
        public double WeaponPellets { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "Radius cannot be less than 0.")]
        public double WeaponRadius { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "Penetration Energy should be at least 1.")]
        public double WeaponEnergy { get; set; }
        
        [Range(0.01, int.MaxValue, ErrorMessage = "Cooldown cannot be less than one-hundredth (1/100) of a second.")]
        public double WeaponCooldown { get; set; }

        // Material Edit
        public Dictionary<string, Material>? Materials { get; set; }
        public int MaterialEdit { get; set; }
        
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Key must be between 3 and 255 characters in length.")]
        public string? MaterialKey { get; set; }
        
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 255 characters in length.")]
        public string? MaterialName { get; set; }

        [Range(0.01, int.MaxValue, ErrorMessage = "Mass cannot be less than one-hundredth (1/100) of a kilogram.")]
        public double MaterialDensity { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "Connection Strength cannot be less than 0.")]
        public double MaterialConnecitonStrength { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "Penetration Energy Absorption cannot be less than 0.")]
        public double MaterialEnergyAbsorption { get; set; }
    }

    //internal class WeaponAttribute : ValidationAttribute
    //{
    //    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    //    {
    //        var settingsForm = validationContext.ObjectInstance as SettingsForm;
    //        if (settingsForm?.WeaponEdit == settingsForm?.Weapons?.Count + 1)
    //            return null;

    //        return null;
    //    }
    //}

    internal class SettingsFormCssProvider : FieldCssClassProvider
    {
        public override string GetFieldCssClass(EditContext editContext, in FieldIdentifier fieldIdentifier)
        {
            var errors = editContext.GetValidationMessages(fieldIdentifier);
            if (errors != null && !string.IsNullOrEmpty(errors.FirstOrDefault()))
                return "is-invalid";
            
            return string.Empty;
            //return base.GetFieldCssClass(editContext, fieldIdentifier);
        }
    }
}
