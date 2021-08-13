using IAGrim.Database;
using IAGrim.Database.Interfaces;
using IAGrim.UI.Controller;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IAGrim.Database.Model;
using log4net;

namespace IAGrim.Utilities {

    internal static class ItemHtmlWriter {


        /// <summary>
        /// Copy any css/js files from the app\resource folder to the items working directory
        /// </summary>
        private static void CopyMissingFiles() {
            string appResFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources");

            foreach (string dirPath in Directory.GetDirectories(appResFolder, "*", SearchOption.AllDirectories)) {
                Directory.CreateDirectory(dirPath.Replace(appResFolder, GlobalPaths.StorageFolder));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(appResFolder, "*.*", SearchOption.AllDirectories)) {
                File.Copy(newPath, newPath.Replace(appResFolder, GlobalPaths.StorageFolder), true);
            }

        }

        public static void ToJsonSerializeable() {
            CopyMissingFiles();

            string src = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", "item-kjs.html");
            string dst = GlobalPaths.ItemsHtmlFile;

            File.Copy(src, dst, true); // Redundant really, static file now

        }

    }
}