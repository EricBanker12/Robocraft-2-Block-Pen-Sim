﻿using BlockPenSimWPF.Shared.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockPenSimWPF.Shared.Models
{
    internal class SimulationForm
    {
        private Dictionary<string, List<string>> errorMessages = new();

        private MinMax cpu;
        public double CpuMin
        {
            get { return cpu.Min; }
            set { cpu.Min = value; IsValid(nameof(CpuMin)); }
        }
        public double CpuMax
        {
            get { return cpu.Max; }
            set { cpu.Max = value; IsValid(nameof(CpuMax)); }
        }
        private MinMax weight;
        public double WeightMin
        {
            get { return weight.Min; }
            set { weight.Min = value; IsValid(nameof(WeightMin)); }
        }
        public double WeightMax
        {
            get { return weight.Max; }
            set { weight.Max = value; IsValid(nameof(WeightMax)); }
        }

        private MinMax length;
        public double LengthMin
        {
            get { return length.Min; }
            set { length.Min = value; IsValid(nameof(LengthMin)); }
        }
        public double LengthMax
        {
            get { return length.Max; }
            set { length.Max = value; IsValid(nameof(LengthMax)); }
        }
        private MinMax width;
        public double WidthMin
        {
            get { return width.Min; }
            set { width.Min = value; IsValid(nameof(WidthMin)); }
        }
        public double WidthMax
        {
            get { return width.Max; }
            set { width.Max = value; IsValid(nameof(WidthMax)); }
        }
        private MinMax height;
        public double HeightMin
        {
            get { return height.Min; }
            set { height.Min = value; IsValid(nameof(HeightMin)); }
        }
        public double HeightMax
        {
            get { return height.Max; }
            set { height.Max = value; IsValid(nameof(HeightMax)); }
        }

        public Dictionary<string, double> WeaponCount = new();
        public Dictionary<string, double> WeaponRatio = new();
        public double[] DirectionRatio = { };

        public bool IsValid(string fieldName)
        {
            if (errorMessages.ContainsKey(fieldName)) errorMessages[fieldName].Clear();
            else errorMessages[fieldName] = new List<string>();
            
            switch (fieldName)
            {
                case nameof(CpuMin):
                    if (CpuMin < 1) errorMessages[fieldName].Add("Min cannot be less than 1.");
                    break;
                case nameof(CpuMax):
                    if (CpuMax > 940) errorMessages[fieldName].Add("Max cannot be more than 940.");
                    break;
                case nameof(WeightMin):
                    if (WeightMin < 0) errorMessages[fieldName].Add("Min cannot be less than 0.");
                    break;
                case nameof(LengthMin):
                    if (LengthMin < 0) errorMessages[fieldName].Add("Min cannot be less than 0.");
                    break;
                case nameof(LengthMax):
                    if (LengthMax > 63) errorMessages[fieldName].Add("Max cannot be more than 63 blocks.");
                    break;
                case nameof(WidthMin):
                    if (WidthMin < 0) errorMessages[fieldName].Add("Min cannot be less than 0.");
                    break;
                case nameof(WidthMax):
                    if (WidthMax > 63) errorMessages[fieldName].Add("Max cannot be more than 63 blocks.");
                    break;
                case nameof(HeightMin):
                    if (HeightMin < 0) errorMessages[fieldName].Add("Min cannot be less than 0.");
                    break;
                case nameof(HeightMax):
                    if (HeightMax > 63) errorMessages[fieldName].Add("Max cannot be more than 63 blocks.");
                    break;
            }

            return !errorMessages.ContainsKey(fieldName) || errorMessages[fieldName].Count == 0;
        }

        public bool IsValid(string fieldName, object key)
        {
            if (fieldName.StartsWith("Weapon"))
            {
                var weaponKey = (string)key;
                var errorKey = fieldName + weaponKey;

                if (errorMessages.ContainsKey(errorKey)) errorMessages[errorKey].Clear();
                else errorMessages[errorKey] = new List<string>();

                if (fieldName == nameof(WeaponCount))
                {
                    if (WeaponCount[weaponKey] < 1) errorMessages[errorKey].Add("Count should not be less than 1.");
                }
                else if (fieldName == nameof(WeaponRatio))
                {
                    if (WeaponRatio[weaponKey] < 0) errorMessages[errorKey].Add("Ratio should not be less than 0.");
                }

                return !errorMessages.ContainsKey(errorKey) || errorMessages[errorKey].Count == 0;
            }
            else if (fieldName.StartsWith("Direction"))
            {
                var directionKey = (Direction)key;
                var errorKey = fieldName + directionKey;

                if (errorMessages.ContainsKey(errorKey)) errorMessages[errorKey].Clear();
                else errorMessages[errorKey] = new List<string>();

                if (fieldName == nameof(DirectionRatio))
                {
                    if (DirectionRatio[(int)directionKey] < 0) errorMessages[errorKey].Add("Ratio should not be less than 0.");
                }

                return !errorMessages.ContainsKey(errorKey) || errorMessages[errorKey].Count == 0;
            }
            return IsValid(fieldName);
        }

        public bool IsValid()
        {
            if (!IsValid(nameof(CpuMin))) return false;
            if (!IsValid(nameof(CpuMax))) return false;
            if (!IsValid(nameof(WeightMin))) return false;
            if (!IsValid(nameof(WeightMax))) return false;
            if (!IsValid(nameof(LengthMin))) return false;
            if (!IsValid(nameof(LengthMax))) return false;
            if (!IsValid(nameof(WidthMin))) return false;
            if (!IsValid(nameof(WidthMax))) return false;
            if (!IsValid(nameof(HeightMin))) return false;
            if (!IsValid(nameof(HeightMax))) return false;
            foreach (var weapon in WeaponCount)
            {
                if (!IsValid(nameof(WeaponCount), weapon.Key)) return false;
                if (!IsValid(nameof(WeaponRatio), weapon.Key)) return false;
            }
            for (int i = 0; i < DirectionRatio.Length; i++)
            {
                if (!IsValid(nameof(DirectionRatio), i)) return false;
            }

            return true;
        }

        public bool WasValid(string fieldName)
        {
            return !errorMessages.ContainsKey(fieldName) || errorMessages[fieldName].Count == 0;
        }

        public void OverrideValid(string fieldName)
        {
            if (errorMessages.ContainsKey(fieldName))
            {
                errorMessages[fieldName].Clear();
            }
        }

        public void OverrideValid(string fieldName, object key)
        {
            if (fieldName.StartsWith("Weapon"))
            {
                var weaponKey = (string)key;
                var errorKey = fieldName + weaponKey;

                if (errorMessages.ContainsKey(errorKey))
                {
                    errorMessages[errorKey].Clear();
                }
            }
            else if (fieldName.StartsWith("Direction"))
            {
                var directionKey = (Direction)key;
                var errorKey = fieldName + directionKey;

                if (errorMessages.ContainsKey(errorKey))
                {
                    errorMessages[errorKey].Clear();
                }
            }
            else
            {
                OverrideValid(fieldName);
            }
        }

        public void OverrideValid()
        {
            errorMessages.Clear();
        }

        public List<string> GetErrorMessages()
        {
            var retval = new List<string>();
            foreach (var error in errorMessages)
            {
                retval.AddRange(error.Value);
            }
            return retval;
        }

        public List<string> GetErrorMessages(string fieldName)
        {
            if (errorMessages.ContainsKey(fieldName))
            {
                return errorMessages[fieldName];
            }
            return new List<string>();
        }

        public List<string> GetErrorMessages(string fieldName, object key)
        {
            if (fieldName.StartsWith("Weapon"))
            {
                var weaponKey = (string)key;
                var errorKey = fieldName + weaponKey;

                if (errorMessages.ContainsKey(errorKey))
                {
                    return errorMessages[errorKey];
                }
                return GetErrorMessages(fieldName);
            }
            else if (fieldName.StartsWith("Direction"))
            {
                var directionKey = (Direction)key;
                var errorKey = fieldName + directionKey;

                if (errorMessages.ContainsKey(errorKey))
                {
                    return errorMessages[errorKey];
                }
                return GetErrorMessages(fieldName);
            }
            return GetErrorMessages(fieldName);
        }

        public void Reset(IndexStore state)
        {
            this.CpuMin = state.Cpu.Min;
            this.CpuMax = state.Cpu.Max;
            this.WeightMin = state.Weight.Min;
            this.WeightMax = state.Weight.Max;
            this.LengthMin = state.Length.Min;
            this.LengthMax = state.Length.Max;
            this.WidthMin = state.Width.Min;
            this.WidthMax = state.Width.Max;
            this.HeightMin = state.Height.Min;
            this.HeightMax = state.Height.Max;
            this.WeaponCount = state.WeaponCount.ToDictionary(e => e.Key, e => e.Value);
            this.WeaponRatio = state.WeaponRatio.ToDictionary(e => e.Key, e => e.Value);
            this.DirectionRatio = state.DirectionRatio.ToArray();
            OverrideValid();
        }

        public void SetState(IndexStore state)
        {
            state.Cpu = this.cpu;
            state.Weight = this.weight;
            state.Length = this.length;
            state.Width = this.width;
            state.Height = this.height;
            
            state.WeaponCount.Clear();
            foreach (var item in this.WeaponCount)
            {
                state.WeaponCount[item.Key] = item.Value;
            }
            
            state.WeaponRatio.Clear();
            foreach (var item in this.WeaponRatio)
            {
                state.WeaponRatio[item.Key] = item.Value;
            }
            
            for (int i = 0; i < this.DirectionRatio.Length; i++)
            {
                state.DirectionRatio[i] = this.DirectionRatio[i];
            }
        }
    }
}
