using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IAGrim.StashFile;
using IAGrim.Utilities;

namespace IAGrim.Parsers.Arz {
    internal class StashManager {
        private string _selectedStashFile = GlobalPaths.TransferFiles.FirstOrDefault()?.Filename ?? ""; // TODO: Some kind of setting to change this, both cloud and non-cloud
        public List<Item> UnlootedItems => UpdateUnlooted(_selectedStashFile);

        public event EventHandler StashUpdated;

        public StashManager(
            ) {

            string path = GlobalPaths.SavePath;

            if (!string.IsNullOrEmpty(path) && File.Exists(path)) {
                UpdateUnlooted(path);
            }
        }


        public List<Item> UpdateUnlooted(string filename) {
            GDCryptoDataBuffer pCrypto = new GDCryptoDataBuffer(DataBuffer.ReadBytesFromDisk(filename));
            List<Item> unlootedLocal = new List<Item>();

            Stash stash = new Stash();
            if (stash.Read(pCrypto)) {
                // Update the internal listing of unlooted items (in stash tabs)
                foreach (StashTab tab in stash.Tabs) {
                    unlootedLocal.AddRange(tab.Items);
                }
            }

            return unlootedLocal;
        }


        /// <summary>
        ///     Attempt to get the name of the current mod
        ///     Vanilla leaves this tag empty
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryGetModLabel(string filename, out string result) {
            if (File.Exists(filename)) {
                GDCryptoDataBuffer pCrypto = new GDCryptoDataBuffer(DataBuffer.ReadBytesFromDisk(filename));
                Stash stash = new Stash();

                if (stash.Read(pCrypto)) {
                    result = stash.ModLabel;
                    return true;
                }
            }

            result = string.Empty;
            return false;
        }

    }
}