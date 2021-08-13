using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IAGrim.Database.Interfaces;
using IAGrim.UI.Controller;
using IAGrim.Utilities;
using IAGrim.Utilities.HelperClasses;
using log4net;
using log4net.Repository.Hierarchy;

namespace IAGrim.UI.Tabs.Util {
    class ModSelectionHandler : IDisposable {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ModSelectionHandler));
        private long _numAvailableModFiltersLastUpdate = -1;
        private ComboBox _cbModFilter;
        private readonly Action _updateView;
        private readonly Action<string> _setStatus;
        private string _lastMod;
        private bool _lastHardcoreSetting;

        public GDTransferFile SelectedMod => _cbModFilter.SelectedItem as GDTransferFile;

        public ModSelectionHandler(ComboBox cbModFilter, Action updateView, Action<string> setStatus) {
            this._cbModFilter = cbModFilter;
            _cbModFilter.DropDown += modFilter_DropDown;
            this._cbModFilter.SelectedIndexChanged += new System.EventHandler(this.cbModFilter_SelectedIndexChanged);
            _updateView = updateView;
            _setStatus = setStatus;
        }

        ~ModSelectionHandler() {
            Dispose();
        }

        private void cbModFilter_SelectedIndexChanged(object sender, EventArgs e) {
            if (Properties.Settings.Default.AutoSearch) {
                _updateView();
            }
        }


        private void UpdateModSelection(string mod, bool isHardcore) {
            if (mod != _lastMod || isHardcore != _lastHardcoreSetting) {
                var options = GetAvailableModSelection();
                _cbModFilter.Items.Clear();
                _cbModFilter.Items.AddRange(options);
                var query = options.Where(m => m.Mod.Equals(mod) && m.IsHardcore == isHardcore);
                if (query.Any()) {
                    _cbModFilter.SelectedItem = query.FirstOrDefault();
                }
            }

            _lastMod = mod;
            _lastHardcoreSetting = isHardcore;
        }



        public GDTransferFile[] GetAvailableModSelection() {
            var mods = GlobalPaths.TransferFiles;

            if (mods.Count != _numAvailableModFiltersLastUpdate) {
                List<GDTransferFile> result = new List<GDTransferFile>();
                for (int i = 0; i < mods.Count; i++) {
                    var item = mods[i];
                    item.Enabled = true;
                    result.Add(item);
                }

                mods = result;
            }

            _numAvailableModFiltersLastUpdate = mods.Count;
            var modsArray = mods.ToArray();
            return modsArray;
        }

        public void Dispose() {
            if (_cbModFilter != null) {
                _cbModFilter.DropDown -= modFilter_DropDown;
                _cbModFilter = null;
            }
        }

        public void UpdateModSelection(string mod) {
            GDTransferFile selected = _cbModFilter.SelectedItem as GDTransferFile;
            if (selected != null) {
                // Survival mode is the crucible, which shares items with campaign.
                UpdateModSelection(mod.Replace("survivalmode", ""), selected.IsHardcore);
            }
        }

        public void UpdateModSelection(bool isHardcore) {
            GDTransferFile selected = _cbModFilter.SelectedItem as GDTransferFile;
            if (selected != null) {
                UpdateModSelection(selected.Mod, isHardcore);
            }
        }


        private void modFilter_DropDown(object sender, EventArgs e) {
            var entries = GetAvailableModSelection();
            if (entries.Length != _cbModFilter.Items.Count) {
                _cbModFilter.Items.Clear();
                _cbModFilter.Items.AddRange(entries);
            }
        }
    }
}
