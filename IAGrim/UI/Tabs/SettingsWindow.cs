using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IAGrim.Database;
using IAGrim.Parsers.Arz;
using log4net;
using EvilsoftCommons;
using IAGrim.UI.Controller;
using IAGrim.Utilities;
using IAGrim.Database.Interfaces;
using IAGrim.Parsers.GameDataParsing.Service;
using IAGrim.Utilities.HelperClasses;
// 
namespace IAGrim.UI {
    partial class SettingsWindow : Form {
        private ISettingsController _controller = new SettingsController();
        private TooltipHelper _tooltipHelper;
        
        private readonly IDatabaseSettingDao _settingsDao;
        private readonly IDatabaseItemDao _itemDao;


        public SettingsWindow(
            TooltipHelper tooltipHelper, 
            IDatabaseSettingDao settingsDao, 
            IDatabaseItemDao itemDao) {            
            InitializeComponent();
            this._tooltipHelper = tooltipHelper;
            this._settingsDao = settingsDao;
            this._itemDao = itemDao;


            _controller.LoadDefaults();

        }

        private void SettingsWindow_Load(object sender, EventArgs e) {
            this.Dock = DockStyle.Fill;

            

            this.labelNumItems.Text = string.Format("Number of items parsed from Grim Dawn database: {0}", _itemDao.GetRowCount());
            
            // TODO:
            string filename = GrimDawnDetector.GetGrimLocation();
            string databaseFile = Path.Combine(filename, "database", "database.arz");
            DateTime lastPatch = default(DateTime);
            if (File.Exists(databaseFile)) {
                lastPatch = System.IO.File.GetLastWriteTime(databaseFile);
                this.labelLastPatch.Text = string.Format("Last Grim Dawn patch: {0}", lastPatch.ToString("dd/MM/yyyy"));
            }
            else {
                this.labelLastPatch.Text = "Could not find Grim Dawn install folder";
            }

            DateTime lastUpdate = new DateTime(_settingsDao.GetLastDatabaseUpdate());
            this.labelLastUpdated.Text = string.Format("Grim Dawn database last updated: {0}", lastUpdate.ToString("dd/MM/yyyy"));
            if (lastUpdate < lastPatch)
                this.labelLastUpdated.ForeColor = Color.Red;

        }
        

        private void buttonViewLogs_Click(object sender, EventArgs e) {
            _controller.OpenLogFolder();
        }


        // create bindings and stick these into its own settings class
        // unit testable


        private void buttonDonate_Click(object sender, EventArgs e) {
            _controller.DonateNow();
        }


        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                System.Diagnostics.Process.Start("https://discord.gg/5wuCPbB");
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e) {
            Clipboard.SetText("https://discord.gg/5wuCPbB");
            _tooltipHelper.ShowTooltipForControl("Copied to clipboard", linkLabel1, TooltipHelper.TooltipLocation.TOP);
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e) {
            e.Cancel = false;
        }


        private void buttonLanguageSelect_Click(object sender, EventArgs e) {
        }


    }
}
