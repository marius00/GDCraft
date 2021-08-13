using IAGrim.Parsers.Arz;
using IAGrim.UI;
using IAGrim.Utilities.HelperClasses;
using StatTranslator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IAGrim.Utilities {
    static class GlobalSettings {
        static GlobalSettings() {
            if (string.IsNullOrEmpty(Properties.Settings.Default.LocalizationFile) || !File.Exists(Properties.Settings.Default.LocalizationFile))
                Language = new EnglishLanguage();
            else {
                Language = new LocalizationLoader().LoadLanguage(Properties.Settings.Default.LocalizationFile);
            }
        }

        public static string Uuid { get; set; }




        private static ILocalizedLanguage _language;
        public static ILocalizedLanguage Language {
            get {
                return _language;
            }
            set {
                _language = value;
                StatManager = new StatManager(value);
            }
        }
        public static StatManager StatManager { get; set; }

    }
}
