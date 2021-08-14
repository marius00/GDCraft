using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DataAccess;
using EvilsoftCommons;
using EvilsoftCommons.Exceptions;
using IAGrim.Database;
using IAGrim.Database.Interfaces;
using IAGrim.Parser.Arc;
using IAGrim.Parsers.GameDataParsing.Service;
using IAGrim.Utilities;
using log4net;

namespace IAGrim.Parsers.Arz {
    public class ArzParser : IDisposable {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ArzParser));
        private readonly List<DatabaseItem> _skills = new List<DatabaseItem>();

        public ArzParser(
        ) {
        }



        public static void LoadIconsOnly(string grimdawnLocation) {
            Logger.Debug("Icon loading requested");
            {
                var arcItemsFile = GrimFolderUtility.FindArcFile(grimdawnLocation, "items.arc");
                if (!string.IsNullOrEmpty(arcItemsFile)) {
                    Logger.Debug($"Loading vanilla icons from {arcItemsFile}");
                    LoadIcons(arcItemsFile);
                }
                else {
                    Logger.Warn("Could not find the vanilla icons, skipping.");
                }
            }

            foreach (string path in GrimFolderUtility.GetGrimExpansionFolders(grimdawnLocation)) {
                var arcItemsFile = GrimFolderUtility.FindArcFile(path, "items.arc");
                if (!string.IsNullOrEmpty(arcItemsFile)) {
                    Logger.Debug($"Loading expansion icons from {arcItemsFile}");
                    LoadIcons(arcItemsFile);
                }
                else {
                    Logger.Warn("Could not find the expansion, skipping.");
                }

            }
        }

        private static void LoadIcons(string arcItemsFile) {
            Logger.Info($"Loading item icons from {arcItemsFile}..");

            bool arcfileLocked = IOHelper.IsFileLocked(new FileInfo(arcItemsFile));
            TemporaryCopy arcTempFile = new TemporaryCopy(arcItemsFile, arcfileLocked);
            string arcItemfile = arcTempFile.Filename;

            if (arcfileLocked) {
                Logger.Info($"The file {arcItemsFile} is currently locked for reading. Perhaps Grim Dawn is running?");
                Logger.Info($"A copy of {arcItemsFile} has been created at {arcItemfile}");
            }
            if (!File.Exists(arcItemfile)) {
                Logger.Warn($"Item icon file \"{arcItemfile}\" could not be located..");
            }

            try {
                DDSImageReader.ExtractItemIcons(arcItemfile, GlobalPaths.StorageFolder);
            }
            catch (ArgumentException ex) {
                // Ideally we'd like to catch the specific exception, but the available log files don't contain the exception name..
                Logger.Error(ex.Message, ex);
                MessageBox.Show("Unable to parse icons, ARZ file is corrupted.\nIf you are using steam, please verify the install integrity.", "Corrupted GD installation", MessageBoxButtons.OK);
            }
        }


        public void Dispose() {
            _skills.Clear();
        }
    }
}