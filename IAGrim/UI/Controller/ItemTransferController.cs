using CefSharp.WinForms;
using CefSharp;
using IAGrim.Database;
using IAGrim.Parsers.Arz;
using IAGrim.Services.MessageProcessor;
using IAGrim.UI.Misc;
using IAGrim.Utilities;
using IAGrim.Utilities.HelperClasses;
using IAGrim.Utilities.RectanglePacker;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IAGrim.Database.Interfaces;
using IAGrim.Services;

namespace IAGrim.UI.Controller {
    class ItemTransferController {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ItemTransferController));
        private readonly IPlayerItemDao _dao;
        private readonly Action<string> _setFeedback;
        private readonly Action<string> _setTooltip;
        private readonly ISettingsReadController _settingsController;
        private readonly DynamicPacker _dynamicPacker;
        private readonly CefBrowserHandler _browser;
        private readonly StashManager _stashManager;
        private readonly ItemStatService _itemStatService;

        public ItemTransferController(
                CefBrowserHandler browser,
                Action<string> feedback,
                Action<string> setTooltip,
                ISettingsReadController settingsController,
                DynamicPacker dynamicPacker,
                IPlayerItemDao playerItemDao,
                StashManager stashManager,
                ItemStatService itemStatService
            ) {
            this._browser = browser;
            this._setFeedback = feedback;
            this._setTooltip = setTooltip;
            this._settingsController = settingsController;
            this._dynamicPacker = dynamicPacker;
            this._dao = playerItemDao;
            this._stashManager = stashManager;
            this._itemStatService = itemStatService;
        }



        List<PlayerItem> GetItemsForTransfer(StashTransferEventArgs args) {
            List<PlayerItem> items = new List<PlayerItem>();

            // Detect the record type (long or string) and add the item(s)
            if (args.Id != null && args.Id > 0) {
                items.Add(_dao.GetById(args.Id.Value));
            }
            else if (args.HasValidId) {
                IList<PlayerItem> tmp = _dao.GetByRecord(args.Prefix, args.BaseRecord, args.Suffix, args.Materia);
                if (tmp.Count > 0) {
                    if (args.Count == 1)
                        items.Add(tmp[0]);
                    else {
                        items.AddRange(tmp);
                    }
                }
            }


            if (items.Contains(null)) {
                Logger.Warn("Attempted to transfer NULL item.");

                var message = GlobalSettings.Language.GetTag("iatag_feedback_item_does_not_exist");
                _setFeedback(GlobalSettings.Language.GetTag("iatag_feedback_item_does_not_exist"));
                // 
                // 
                _browser.ShowMessage(message, "Error");

                return null;
            }

            return items;
        }





        struct TransferStatus {
            public int NumItemsRequested;
            public int NumItemsTransferred;
        }

        private TransferStatus TransferItems(string transferFile, List<PlayerItem> items, int maxItemsToTransfer) {
        
            // Remove all items deposited (may or may not be less than the requested amount, if no inventory space is available)
            string error;
            int numItemsReceived = (int)items.Sum(item => Math.Max(1, item.StackCount));
            int numItemsRequested = Math.Min(maxItemsToTransfer, numItemsReceived);
            

            _itemStatService.ApplyStatsToPlayerItems(items); // For item class? 'IsStackable' maybe?
            _stashManager.Deposit(transferFile, items, maxItemsToTransfer, out error);
            _dao.Update(items, true);


            int NumItemsTransferred = numItemsRequested - (numItemsRequested - (int)items.Sum(item => Math.Max(1, item.StackCount)));

            if (!string.IsNullOrEmpty(error)) {
                Logger.Warn(error);
                _browser.ShowMessage(error, "Error");
            }

            return new TransferStatus {
                NumItemsTransferred = NumItemsTransferred,
                NumItemsRequested = numItemsRequested
            };
        }
    }
}
