﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using AutoUpdaterDotNET;
using CefSharp;
using EvilsoftCommons;
using EvilsoftCommons.Cloud;
using EvilsoftCommons.DllInjector;
using EvilsoftCommons.Exceptions;
using IAGrim.Backup;
using IAGrim.BuddyShare;
using IAGrim.Database.Interfaces;
using IAGrim.Parsers;
using IAGrim.Parsers.Arz;
using IAGrim.Parsers.Arz.dto;
using IAGrim.Parsers.GameDataParsing.Service;
using IAGrim.Properties;
using IAGrim.Services;
using IAGrim.Services.MessageProcessor;
using IAGrim.UI.Controller;
using IAGrim.UI.Misc;
using IAGrim.UI.Popups;
using IAGrim.UI.Tabs;
using IAGrim.Utilities;
using IAGrim.Utilities.Cloud;
using IAGrim.Utilities.HelperClasses;
using IAGrim.Utilities.RectanglePacker;
using log4net;
using MoreLinq;
using Timer = System.Timers.Timer;

namespace IAGrim.UI {

    public partial class MainWindow : Form {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MainWindow));
        readonly CefBrowserHandler _cefBrowserHandler;

        private string UPDATE_XML {
            get {
                var v = Assembly.GetExecutingAssembly().GetName().Version;
                string version = $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";

                if ((bool)Settings.Default.SubscribeExperimentalUpdates) {
                    return $"http://grimdawn.dreamcrash.org/ia/version.php?beta&version={version}";
                }
                return $"http://grimdawn.dreamcrash.org/ia/version.php?version={version}";
            }
        }

        private readonly ISettingsReadController _settingsController = new SettingsController();

        private FormWindowState _previousWindowState = FormWindowState.Normal;
        private readonly TooltipHelper _tooltipHelper = new TooltipHelper();
        private DateTime _lastAutomaticUpdateCheck = default(DateTime);
        private DateTime _lastTimeNotMinimized = DateTime.Now;
        private readonly DynamicPacker _dynamicPacker;
        private readonly List<IMessageProcessor> _messageProcessors = new List<IMessageProcessor>();

        private StashManager _stashManager;
        private BuddySettings _buddySettingsWindow;
        private StashFileMonitor _stashFileMonitor = new StashFileMonitor();

        private Action<RegisterWindow.DataAndType> _registerWindowDelegate;
        private RegisterWindow _window;
        private InjectionHelper _injector;
        private ProgressChangedEventHandler _injectorCallbackDelegate;

        private Timer _timerReportUsage;
        private BuddyBackgroundThread _buddyBackgroundThread;
        private BackgroundTask _backupBackgroundTask;
        private ItemTransferController _transferController;
        private ItemSynchronizer _itemSynchronizer; // Online backups

        private readonly IItemTagDao _itemTagDao;
        private readonly IDatabaseItemDao _databaseItemDao;
        private readonly IDatabaseItemStatDao _databaseItemStatDao;
        private readonly IPlayerItemDao _playerItemDao;
        private readonly IDatabaseSettingDao _databaseSettingDao;
        private readonly IBuddyItemDao _buddyItemDao;
        private readonly IBuddySubscriptionDao _buddySubscriptionDao;
        private readonly ArzParser _arzParser;
        private readonly RecipeParser _recipeParser;
        private readonly IItemSkillDao _itemSkillDao;
        private readonly ParsingService _parsingService;

        private readonly Stopwatch _reportUsageStatistics;

#region Stash Status
        

        /// <summary>
        /// Toolstrip callback for GDInjector
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InjectorCallback(object sender, ProgressChangedEventArgs e) {
            if (InvokeRequired) {
                Invoke((MethodInvoker)delegate { InjectorCallback(sender, e); });
            } else {
                if (e.ProgressPercentage == InjectionHelper.INJECTION_ERROR) {
                    GlobalSettings.StashStatus = StashAvailability.ERROR;
                    statusLabel.Text = e.UserState as string;
                }
                // No grim dawn client, so stash is closed!
                else if (e.ProgressPercentage == InjectionHelper.NO_PROCESS_FOUND_ON_STARTUP) {
                    if (GlobalSettings.StashStatus == StashAvailability.UNKNOWN) {
                        GlobalSettings.StashStatus = StashAvailability.CLOSED;
                        GlobalSettings.GrimDawnRunning = false; // V1.0.4.0 hotfix
                    }
                }
                // No grim dawn client, so stash is closed!
                else if (e.ProgressPercentage == InjectionHelper.NO_PROCESS_FOUND) {
                    GlobalSettings.StashStatus = StashAvailability.CLOSED;
                    GlobalSettings.GrimDawnRunning = false;// V1.0.4.0 hotfix
                }
            }
        }


#endregion Stash Status

        [DllImport("kernel32")]
        private static extern UInt64 GetTickCount64();



        public MainWindow(
            CefBrowserHandler browser,
             IDatabaseItemDao databaseItemDao,
             IDatabaseItemStatDao databaseItemStatDao,
             IPlayerItemDao playerItemDao,
             IDatabaseSettingDao databaseSettingDao,
             IBuddyItemDao buddyItemDao,
             IBuddySubscriptionDao buddySubscriptionDao,
             ArzParser arzParser,
             IRecipeItemDao recipeItemDao,
             IItemSkillDao itemSkillDao,
            IItemTagDao itemTagDao, ParsingService parsingService) {
            _cefBrowserHandler = browser;
            InitializeComponent();
            FormClosing += MainWindow_FormClosing;

            _reportUsageStatistics = new Stopwatch();
            _reportUsageStatistics.Start();

            _dynamicPacker = new DynamicPacker(databaseItemStatDao);
            _databaseItemDao = databaseItemDao;
            _databaseItemStatDao = databaseItemStatDao;
            _playerItemDao = playerItemDao;
            _databaseSettingDao = databaseSettingDao;
            _buddyItemDao = buddyItemDao;
            _buddySubscriptionDao = buddySubscriptionDao;
            _arzParser = arzParser;
            _recipeParser = new RecipeParser(recipeItemDao);
            _itemSkillDao = itemSkillDao;
            _itemTagDao = itemTagDao;
            _parsingService = parsingService;
        }

        /// <summary>
        /// Report usage once every 12 hours, in case the user runs it 'for ever'
        /// Will halt if not opened for 38 hours
        /// </summary>
        private void ReportUsage() {
            if ((DateTime.Now - _lastTimeNotMinimized).TotalHours < 38) {
                if (_reportUsageStatistics.Elapsed.Hours > 12) {
                    _reportUsageStatistics.Restart();
                    ThreadPool.QueueUserWorkItem(m => ExceptionReporter.ReportUsage());
                    AutoUpdater.Start(UPDATE_XML);
                }
            }
        }


        private void OnlineBackupAuthFailureHandler() {
            Logger.Warn("Online backup failed due to an authentication failure.");
            _itemSynchronizer?.Dispose();
            _itemSynchronizer = null;
        }
        private void EnableOnlineBackups(bool enable) {
            if (enable) {
                if (_itemSynchronizer == null) {
                    _itemSynchronizer = new ItemSynchronizer(
                        _playerItemDao, 
                        Settings.Default.OnlineBackupToken, 
                        GlobalSettings.RemoteBackupServer, 
                        OnlineBackupAuthFailureHandler);
                    _itemSynchronizer.Start();
                }
            } else {
                _itemSynchronizer?.Dispose();
                _itemSynchronizer = null;
            }
        }


        private void IterAndCloseForms(Control.ControlCollection controls) {
            foreach (Control c in controls) {
                Form f = c as Form;
                if (f != null)
                    f.Close();

                IterAndCloseForms(c.Controls);
            }
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e) {
            // No idea which of these are triggering on rare occasions, perhaps Deactivate, sizechanged or filterWindow.
            FormClosing -= MainWindow_FormClosing;
            SizeChanged -= OnMinimizeWindow;

            _stashFileMonitor?.Dispose();
            _stashFileMonitor = null;
            _stashManager = null;

            _backupBackgroundTask?.Dispose();

            _timerReportUsage?.Stop();
            _timerReportUsage?.Dispose();
            _timerReportUsage = null;

            _tooltipHelper?.Dispose();

            _buddyBackgroundThread?.Dispose();
            _buddyBackgroundThread = null;

            _itemSynchronizer?.Dispose();
            _itemSynchronizer = null;

            panelHelp.Controls.Clear();

            _injector?.Dispose();
            _injector = null;

            _window?.Dispose();
            _window = null;

            IterAndCloseForms(Controls);
        }


        private void SetFeedback(string feedback) {
            try {
                if (InvokeRequired) {
                    Invoke((MethodInvoker) delegate { SetFeedback(feedback); });
                }
                else {
                    statusLabel.Text = feedback.Replace("\\n", " - ");
                    _cefBrowserHandler.ShowMessage(feedback, "Info");
                }
            }
            catch (ObjectDisposedException) {
                Logger.Debug("Attempted to set feedback, but UI already disposed. (Probably shutting down)");
            }
        }

        private void SetTooltipAtmouse(string message) {
            _tooltipHelper.ShowTooltipAtMouse(message, _cefBrowserHandler.BrowserControl);
        }

        private void CheckForUpdates() {
            if (GetTickCount64() > 5 * 60 * 1000 && (DateTime.Now - _lastAutomaticUpdateCheck).TotalHours > 36) {
                AutoUpdater.LetUserSelectRemindLater = true;
                AutoUpdater.RemindLaterTimeSpan = RemindLaterFormat.Days;
                AutoUpdater.RemindLaterAt = 7;
                AutoUpdater.Start(UPDATE_XML);

                _lastAutomaticUpdateCheck = DateTime.Now;
                Logger.Info("Checking for updates..");
            }
        }

        private void TimerTickLookForGrimDawn(object sender, EventArgs e) {
            System.Windows.Forms.Timer timer = sender as System.Windows.Forms.Timer;
            if (Thread.CurrentThread.Name == null)
                Thread.CurrentThread.Name = "DetectGrimDawnTimer";

            string gdPath = GrimDawnDetector.GetGrimLocation();
            if (!string.IsNullOrEmpty(gdPath) && Directory.Exists(gdPath)) {
                timer?.Stop();

                // Attempt to force a database update
                foreach (Control c in modsPanel.Controls) {
                    ModsDatabaseConfig config = c as ModsDatabaseConfig;
                    if (config != null) {
                        config.ForceDatabaseUpdate(gdPath, string.Empty);
                        break;
                    }
                }

                Logger.InfoFormat("Found Grim Dawn at {0}", gdPath);
            }
        }



        private void MainWindow_Load(object sender, EventArgs e) {
            if (Thread.CurrentThread.Name == null)
                Thread.CurrentThread.Name = "UI";


            ExceptionReporter.EnableLogUnhandledOnThread();
            SizeChanged += OnMinimizeWindow;

            buttonDevTools.Visible = Debugger.IsAttached;

            _stashManager = new StashManager(_playerItemDao, _databaseItemStatDao, SetFeedback, () => { });
            _stashFileMonitor.OnStashModified += (_, __) => {
                StashEventArg args = __ as StashEventArg;
                if (_stashManager.TryLootStashFile(args?.Filename)) {
                    // STOP TIMER
                    _stashFileMonitor.CancelQueuedNotify();
                }
            };
            if (!_stashFileMonitor.StartMonitorStashfile(GlobalPaths.SavePath)) {
                MessageBox.Show("Ooops!\nIt seems you are synchronizing your saves to steam cloud..\nThis tool is unfortunately not compatible.\n");
                Process.Start("http://www.grimdawn.com/forums/showthread.php?t=20752");

                if (!Debugger.IsAttached)
                    Close();

            }

            //ItemHtmlWriter.Write(new List<PlayerHeldItem>());

            // Chicken and the egg..
            SearchController searchController = new SearchController(
                _databaseItemDao,
                _playerItemDao, 
                _databaseItemStatDao, 
                _itemSkillDao, 
                _buddyItemDao,
                _stashManager
                );
            _cefBrowserHandler.InitializeChromium(searchController.JsBind);
            searchController.Browser = _cefBrowserHandler;
            searchPanel.Controls.Add(_cefBrowserHandler.BrowserControl);


            // Load the grim database
            string gdPath = GrimDawnDetector.GetGrimLocation();
            if (!string.IsNullOrEmpty(gdPath)) {
            } else {
                Logger.Warn("Could not find the Grim Dawn install location");
                statusLabel.Text = "Could not find the Grim Dawn install location";

                var timer = new System.Windows.Forms.Timer();
                timer.Tick += TimerTickLookForGrimDawn;
                timer.Interval = 10000;
                timer.Start();
            }

            // Load recipes
            foreach (string file in GlobalPaths.FormulasFiles) {
                if (!string.IsNullOrEmpty(file)) {
                    bool isHardcore = file.EndsWith("gsh");
                    Logger.InfoFormat("Reading recipes at \"{0}\", IsHardcore={1}", file, isHardcore);
                    _recipeParser.UpdateFormulas(file, isHardcore);
                }
            }

            var addAndShow = UIHelper.AddAndShow;


            // Create the tab contents
            _buddySettingsWindow = new BuddySettings(delegate (bool b) { BuddySyncEnabled = b; }, 
                _buddyItemDao, 
                _buddySubscriptionDao
                );


            var backupSettings = new BackupSettings(EnableOnlineBackups, _playerItemDao);
            tabControl1.Selected += ((s, ev) => {

            });
            addAndShow(new ModsDatabaseConfig(() => { }, _databaseSettingDao, _arzParser, _playerItemDao, _parsingService), modsPanel);
            addAndShow(new HelpTab(), panelHelp);            



            addAndShow(
                new SettingsWindow(
                    _itemTagDao,
                    _tooltipHelper,
                    () => { },
                    _databaseSettingDao,
                    _databaseItemDao,
                    _playerItemDao,
                    _arzParser,
                    new GDTransferFile[]{},
                    _stashManager,
                    _parsingService
                ),
                settingsPanel);


            new StashTabPicker(_stashManager.NumStashTabs).SaveStashSettingsToRegistry();

#if !DEBUG
            ThreadPool.QueueUserWorkItem(m => ExceptionReporter.ReportUsage());
            CheckForUpdates();
#endif

            int min = 1000 * 60;
            int hour = 60 * min;
            _timerReportUsage = new Timer();
            _timerReportUsage.Start();
            _timerReportUsage.Elapsed += (a1, a2) => {
                if (Thread.CurrentThread.Name == null)
                    Thread.CurrentThread.Name = "ReportUsageThread";
                ReportUsage();
            };
            _timerReportUsage.Interval = 12 * hour;
            _timerReportUsage.AutoReset = true;
            _timerReportUsage.Start();



            //settingsController.Data.budd
            BuddySyncEnabled = (bool)Settings.Default.BuddySyncEnabled;

            // Start the backup task
            _backupBackgroundTask = new BackgroundTask(new CloudBackup(_playerItemDao));



            LocalizationLoader.ApplyLanguage(Controls, GlobalSettings.Language);
            EasterEgg.Activate(this);


            // Initialize the "stash packer" used to find item positions for transferring items ingame while the stash is open
            {
                _dynamicPacker.Initialize(8, 16);

                var transferFiles = GlobalPaths.TransferFiles;
                if (transferFiles.Count > 0) {
                    var file = transferFiles.MaxBy(m => m.LastAccess);
                    var stash = StashManager.GetStash(file.Filename);
                    if (stash != null) {
                        _dynamicPacker.Initialize(stash.Width, stash.Height);
                        if (stash.Tabs.Count >= 3) {
                            foreach (var item in stash.Tabs[2].Items) {

                                byte[] bx = BitConverter.GetBytes(item.XOffset);
                                uint x = (uint)BitConverter.ToSingle(bx, 0);

                                byte[] by = BitConverter.GetBytes(item.YOffset);
                                uint y = (uint)BitConverter.ToSingle(by, 0);

                                _dynamicPacker.Insert(item.BaseRecord, item.Seed, x, y);
                            }
                        }
                    }
                }
            }

            _messageProcessors.Add(new ItemPositionFinder(_dynamicPacker));
            _messageProcessors.Add(new PlayerPositionTracker());
            _messageProcessors.Add(new StashStatusHandler());
            

            GlobalSettings.StashStatusChanged += GlobalSettings_StashStatusChanged;


            Application.AddMessageFilter(new MousewheelMessageFilter());


            {
                var b = !string.IsNullOrEmpty(Settings.Default.OnlineBackupToken) && Settings.Default.OnlineBackupVerified;
                EnableOnlineBackups(b);
            }


            var titleTag = GlobalSettings.Language.GetTag("iatag_ui_itemassistant");
            if (!string.IsNullOrEmpty(titleTag)) {
                this.Text += $" - {titleTag}";
            }
        }


        private void GlobalSettings_StashStatusChanged(object sender, EventArgs e) {

            if (InvokeRequired) {
                Invoke((MethodInvoker)delegate { GlobalSettings_StashStatusChanged(sender, e); });
                return;
            }

            switch (GlobalSettings.StashStatus) {
                case StashAvailability.OPEN:
                    tsStashStatus.ForeColor = Color.FromArgb(255, 192, 0, 0);
                    tsStashStatus.Text = GlobalSettings.Language.GetTag("iatag_stash_open");
                    break;
                case StashAvailability.CRAFTING:
                    tsStashStatus.ForeColor = Color.FromArgb(255, 192, 0, 0);
                    tsStashStatus.Text = GlobalSettings.Language.GetTag("iatag_stash_crafting");
                    break;
                case StashAvailability.CLOSED:
                    tsStashStatus.ForeColor = Color.FromArgb(255, 0, 142, 0);
                    tsStashStatus.Text = GlobalSettings.Language.GetTag("iatag_stash_closed");
                    break;
                case StashAvailability.ERROR:
                    tsStashStatus.ForeColor = Color.FromArgb(255, 192, 0, 0);
                    tsStashStatus.Text = GlobalSettings.Language.GetTag("iatag_stash_error");
                    break;
                case StashAvailability.UNKNOWN:
                    tsStashStatus.ForeColor = Color.FromArgb(255, 192, 0, 0);
                    tsStashStatus.Text = GlobalSettings.Language.GetTag("iatag_stash_unknown");
                    break;
                case StashAvailability.SORTED:
                    tsStashStatus.ForeColor = Color.FromArgb(255, 192, 0, 0);
                    tsStashStatus.Text = GlobalSettings.Language.GetTag("iatag_stash_sorted");
                    break;
                default:
                    tsStashStatus.ForeColor = Color.FromArgb(255, 192, 0, 0);
                    tsStashStatus.Text = GlobalSettings.Language.GetTag("iatag_stash_") + GlobalSettings.StashStatus;
                    break;

            }
        }

#region Tray and Menu

        /// <summary>
        /// Minimize to tray
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMinimizeWindow(object sender, EventArgs e) {
            try {
                if (_settingsController.MinimizeToTray) {
                    if (WindowState == FormWindowState.Minimized) {
                        Hide();
                        notifyIcon1.Visible = true;
                    } else /*if (this.WindowState == FormWindowState.Normal)*/ {
                        notifyIcon1.Visible = false;
                        _previousWindowState = WindowState;
                    }
                }
            } catch (Exception ex) {
                Logger.Warn(ex.Message);
                Logger.Warn(ex.StackTrace);
            }

            _lastTimeNotMinimized = DateTime.Now;
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e) {
            Visible = true;
            notifyIcon1.Visible = false;
            WindowState = _previousWindowState;
        }

        private void trayContextMenuStrip_Opening(object sender, CancelEventArgs e) {
            e.Cancel = false;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            Close();
        }

#endregion Tray and Menu

#region BuddySync
        
        /// <summary>
        /// Enable / Disable the buddy sync feature
        /// </summary>
        private bool BuddySyncEnabled {
            get {
                return _buddyBackgroundThread != null;
            }
            set {
                if (value) {
                    // Reset timers etc first
                    BuddySyncEnabled = false;

                    List<long> buddies = new List<long>(_buddySubscriptionDao.ListAll().Select(m => m.Id));
                    _buddyBackgroundThread = new BuddyBackgroundThread(BuddyItemsCallback, _playerItemDao, _buddyItemDao, buddies, 3 * 60 * 1000);
                } else {
                    if (_buddyBackgroundThread != null) {
                        _buddyBackgroundThread.Dispose();
                        _buddyBackgroundThread = null;
                    }
                }
            }
        }

        /// <summary>
        /// BuddyShare callback to store data on UI thread (SQL is here)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BuddyItemsCallback(object sender, ProgressChangedEventArgs e) {
            if (InvokeRequired) {
                Invoke((MethodInvoker)delegate {
                    BuddyItemsCallback(sender, e);
                });
            } else {
                if (e.ProgressPercentage == BuddyBackgroundThread.ProgressStoreBuddydata) {

                    _buddySettingsWindow.UpdateBuddyList();
                } else if (e.ProgressPercentage == BuddyBackgroundThread.ProgressSetUid) {
                    _buddySettingsWindow.UID = (long)e.UserState;
                }
            }
        }

#endregion BuddySync

        
        
        private void button1_Click(object sender, EventArgs e) {
            _cefBrowserHandler.ShowDevTools();
        }

        private void SetItemsClipboard(object ignored, EventArgs _args) {
            if (InvokeRequired) {
                Invoke((MethodInvoker)delegate { SetItemsClipboard(ignored, _args); });
            } else {
                ClipboardEventArg args = _args as ClipboardEventArg;
                if (args != null) Clipboard.SetText(args.Text);
                _tooltipHelper.ShowTooltipAtMouse(GlobalSettings.Language.GetTag("iatag_copied_clipboard"), _cefBrowserHandler.BrowserControl);
            }
        }

    } // CLASS



}