﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BlockPenSimWPF.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.6.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("\"1280,696\"")]
        public string WindowSize {
            get {
                return ((string)(this["WindowSize"]));
            }
            set {
                this["WindowSize"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("{\r\n  \"useDecimalComma\": false,\r\n  \"hideZeroRatioWeaponColumns\": true,\r\n  \"hideZer" +
            "oRatioDirectionColumns\": true,\r\n  \"updateDefaultBlockdataOverInternet\": false,\r\n" +
            "  \"HighlightValues\": {\r\n    \"Score\": true,\r\n    \"Score / CPU\": true,\r\n    \"Score" +
            " / Weight\": true,\r\n    \"Block Length\": false,\r\n    \"Block Width\": false,\r\n    \"B" +
            "lock Height\": false,\r\n    \"Length\": false,\r\n    \"Width\": false,\r\n    \"Height\": f" +
            "alse,\r\n    \"Length Block Count\": false,\r\n    \"Width Block Count\": false,\r\n    \"H" +
            "eight Block Count\": false,\r\n    \"CPU\": false,\r\n    \"Weight (kg)\": false,\r\n    \"S" +
            "TP Laser Blaster (Front)\": false,\r\n    \"STP Laser Blaster (Side)\": false,\r\n    \"" +
            "STP Laser Blaster (Top)\": false,\r\n    \"STP Plasma Cannon (Front)\": false,\r\n    \"" +
            "STP Plasma Cannon (Side)\": false,\r\n    \"STP Plasma Cannon (Top)\": false,\r\n    \"S" +
            "TP Arc Discharger (Front)\": false,\r\n    \"STP Arc Discharger (Side)\": false,\r\n   " +
            " \"STP Arc Discharger (Top)\": false,\r\n    \"STP Rail Gun (Front)\": false,\r\n    \"ST" +
            "P Rail Gun (Side)\": false,\r\n    \"STP Rail Gun (Top)\": false,\r\n    \"TTP Laser Bla" +
            "ster (Front)\": false,\r\n    \"TTP Laser Blaster (Side)\": false,\r\n    \"TTP Laser Bl" +
            "aster (Top)\": false,\r\n    \"TTP Plasma Cannon (Front)\": false,\r\n    \"TTP Plasma C" +
            "annon (Side)\": false,\r\n    \"TTP Plasma Cannon (Top)\": false,\r\n    \"TTP Arc Disch" +
            "arger (Front)\": false,\r\n    \"TTP Arc Discharger (Side)\": false,\r\n    \"TTP Arc Di" +
            "scharger (Top)\": false,\r\n    \"TTP Rail Gun (Front)\": false,\r\n    \"TTP Rail Gun (" +
            "Side)\": false,\r\n    \"TTP Rail Gun (Top)\": false\r\n  },\r\n  \"RowFilters\": {},\r\n  \"C" +
            "olumnsSort\": {\r\n    \"Score\": 1\r\n  },\r\n  \"Cpu\": {\r\n    \"Min\": 0,\r\n    \"Max\": 200\r" +
            "\n  },\r\n  \"Weight\": {\r\n    \"Min\": 0,\r\n    \"Max\": 4000\r\n  },\r\n  \"Length\": {\r\n    \"" +
            "Min\": 4,\r\n    \"Max\": 9\r\n  },\r\n  \"Width\": {\r\n    \"Min\": 9,\r\n    \"Max\": 9\r\n  },\r\n " +
            " \"Height\": {\r\n    \"Min\": 9,\r\n    \"Max\": 9\r\n  },\r\n  \"WeaponCount\": {\r\n    \"LaserB" +
            "laster\": 6,\r\n    \"PlasmaCannon\": 2,\r\n    \"ArcDischarger\": 3,\r\n    \"RailGun\": 1\r\n" +
            "  },\r\n  \"WeaponRatio\": {\r\n    \"LaserBlaster\": 1,\r\n    \"PlasmaCannon\": 1,\r\n    \"A" +
            "rcDischarger\": 0,\r\n    \"RailGun\": 0\r\n  },\r\n  \"DirectionRatio\": [\r\n    8,\r\n    1," +
            "\r\n    1\r\n  ],\r\n  \"Weapons\": {\r\n    \"LaserBlaster\": {\r\n      \"name\": \"Laser Blast" +
            "er\",\r\n      \"cpu\": 20,\r\n      \"damage\": 13,\r\n      \"pellets\": 1,\r\n      \"radius\"" +
            ": 0.3,\r\n      \"energy\": 2000,\r\n      \"cooldown\": 0.6\r\n    },\r\n    \"PlasmaCannon\"" +
            ": {\r\n      \"name\": \"Plasma Cannon\",\r\n      \"cpu\": 60,\r\n      \"damage\": 500,\r\n   " +
            "   \"pellets\": 10,\r\n      \"radius\": 12.5,\r\n      \"energy\": 1000,\r\n      \"cooldown" +
            "\": 2\r\n    },\r\n    \"ArcDischarger\": {\r\n      \"name\": \"Arc Discharger\",\r\n      \"cp" +
            "u\": 40,\r\n      \"damage\": 15000,\r\n      \"pellets\": 20,\r\n      \"radius\": 30,\r\n    " +
            "  \"energy\": 120000,\r\n      \"cooldown\": 6\r\n    },\r\n    \"RailGun\": {\r\n      \"name\"" +
            ": \"Rail Gun\",\r\n      \"cpu\": 120,\r\n      \"damage\": 950,\r\n      \"pellets\": 1,\r\n   " +
            "   \"radius\": 0.3,\r\n      \"energy\": 10000,\r\n      \"cooldown\": 4\r\n    }\r\n  },\r\n  \"" +
            "Materials\": {\r\n    \"Airium\": {\r\n      \"name\": \"Airium\",\r\n      \"density\": 0.7,\r\n" +
            "      \"connectionStrength\": 1.59,\r\n      \"energyAbsorption\": 2500\r\n    },\r\n    \"" +
            "Moderonium\": {\r\n      \"name\": \"Moderonium\",\r\n      \"density\": 1.8,\r\n      \"conne" +
            "ctionStrength\": 2.65,\r\n      \"energyAbsorption\": 5000\r\n    },\r\n    \"Ladium\": {\r\n" +
            "      \"name\": \"\",\r\n      \"density\": 7.3,\r\n      \"connectionStrength\": 3.35,\r\n   " +
            "   \"energyAbsorption\": 7500\r\n    }\r\n  }\r\n}")]
        public string IndexStore {
            get {
                return ((string)(this["IndexStore"]));
            }
            set {
                this["IndexStore"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public string ThemeOverride {
            get {
                return ((string)(this["ThemeOverride"]));
            }
            set {
                this["ThemeOverride"] = value;
            }
        }
    }
}
