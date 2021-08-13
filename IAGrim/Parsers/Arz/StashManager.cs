using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Threading;
using System.Windows.Forms;
using EvilsoftCommons.Exceptions;
using IAGrim.Database;
using IAGrim.Database.Interfaces;
using IAGrim.Properties;
using IAGrim.Services;
using IAGrim.StashFile;
using IAGrim.UI;
using IAGrim.Utilities;
using IAGrim.Utilities.HelperClasses;
using IAGrim.Utilities.RectanglePacker;
using log4net;
using Timer = System.Timers.Timer;

namespace IAGrim.Parsers.Arz {
    internal class StashManager {
        public List<Item> UnlootedItems => _unlootedItems.ToList();
        private ConcurrentBag<Item> _unlootedItems = new ConcurrentBag<Item>();
        //private bool _hasLootedItemsOnceThisSession = false;

        public event EventHandler StashUpdated;

        public StashManager(
            IDatabaseItemStatDao dbItemStatDao,
            Action<string> setFeedback,
            Action performedLootCallback
            ) {

            string path = GlobalPaths.SavePath;

            if (!string.IsNullOrEmpty(path) && File.Exists(path)) {
                UpdateUnlooted(path);
            }
        }


        public void UpdateUnlooted(string filename) {
            GDCryptoDataBuffer pCrypto = new GDCryptoDataBuffer(DataBuffer.ReadBytesFromDisk(filename));

            Stash stash = new Stash();
            if (stash.Read(pCrypto)) {
                // Update the internal listing of unlooted items (in stash tabs)
                List<Item> unlootedLocal = new List<Item>();
                foreach (StashTab tab in stash.Tabs) {
                    unlootedLocal.AddRange(tab.Items);
                }
                Interlocked.Exchange(ref _unlootedItems, new ConcurrentBag<Item>(unlootedLocal));
            }
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