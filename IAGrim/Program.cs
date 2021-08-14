﻿using EvilsoftCommons.Exceptions;
using EvilsoftCommons.SingleInstance;
using IAGrim.Database;
using IAGrim.Database.Interfaces;
using IAGrim.Database.Synchronizer;
using IAGrim.Parsers.Arz;
using IAGrim.UI;
using IAGrim.UI.Misc;
using IAGrim.Utilities;
using log4net;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using EvilsoftCommons.Exceptions.UUIDGenerator;
using IAGrim.Parsers.GameDataParsing.Service;
using IAGrim.Utilities.HelperClasses;


namespace IAGrim {

    internal class 
        Program {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));
        private static MainWindow _mw;


        private static void LoadUuid(IDatabaseSettingDao dao) {
            string uuid = dao.GetUuid();
            if (string.IsNullOrEmpty(uuid)) {
                UuidGenerator g = Guid.NewGuid();
                uuid = g.ToString().Replace("-", "");
                dao.SetUuid(uuid);
            }

            GlobalSettings.Uuid = uuid;
            ExceptionReporter.Uuid = uuid;
            Logger.InfoFormat("Your user id is {0}, use this for any bug reports", GlobalSettings.Uuid);
        }


        [STAThread]
        private static void Main(string[] args) {
            if (Thread.CurrentThread.Name == null)
                Thread.CurrentThread.Name = "Main";

            Logger.Info("Starting IA:GD..");
            ExceptionReporter.UrlCrashreport = "http://ribbs.dreamcrash.org/iagd/crashreport.php"; 
            ExceptionReporter.UrlStats = "http://ribbs.dreamcrash.org/iagd/stats.php";
#if !DEBUG
            ExceptionReporter.LogExceptions = true;
#endif

            Logger.Info("Starting exception monitor for bug reports.."); // Phrased this way since people took it as a 'bad' thing.
            Logger.Debug("Crash reports can be seen at http://ribbs.dreamcrash.org/iagd/logs.html");
            ExceptionReporter.EnableLogUnhandledOnThread();


            var version = Assembly.GetExecutingAssembly().GetName().Version;
            DateTime buildDate = new DateTime(2000, 1, 1)
                .AddDays(version.Build)
                .AddSeconds(version.Revision * 2);

            Logger.InfoFormat("Running version {0}.{1}.{2}.{3} from {4}", version.Major, version.Minor, version.Build, version.Revision, buildDate.ToString("dd/MM/yyyy"));


            if (!DependencyChecker.CheckNet452Installed()) {
                MessageBox.Show("It appears .Net Framework 4.5.2 is not installed.\nIA May not function correctly", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            if (!DependencyChecker.CheckVS2013Installed()) {
                MessageBox.Show("It appears VS 2013 (x86) redistributable is not installed.\nPlease install it to continue using IA", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            if (!DependencyChecker.CheckVS2010Installed()) {
                MessageBox.Show("It appears VS 2010 (x86) redistributable is not installed.\nPlease install it to continue using IA", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }


            // Prevent running in RELEASE mode by accident
            // And thus risking the live database
#if !DEBUG
            if (Debugger.IsAttached) {
                Logger.Fatal("Debugger attached, please run in DEBUG mode");
                return;
            }
#endif
            //ParsingUIBackgroundWorker tmp = new ParsingUIBackgroundWorker();

            Guid guid = new Guid("{F3693953-AAAA-AAAA-86A2-B98AB96A9368}");
            using (SingleInstance singleInstance = new SingleInstance(guid)) {
                if (singleInstance.IsFirstInstance) {
                    Logger.Info("Calling run..");
                    using (ThreadExecuter threadExecuter = new ThreadExecuter()) {

                        ItemHtmlWriter.ToJsonSerializeable();
                        Run(threadExecuter);
                    }
                } else {
                    singleInstance_ArgumentsReceived(null, null);
                }
            }
        }

        /// <summary>
        /// Upgrade any settings if required
        /// This happens for just about every compile
        /// </summary>
        private static void UpgradeSettings() {
            try {
                if (Properties.Settings.Default.CallUpgrade) {
                    Properties.Settings.Default.Upgrade();
                    Properties.Settings.Default.CallUpgrade = false;
                    Logger.Info("Settings upgraded..");

                }
            } catch (Exception ex) {
                Logger.Warn(ex.Message);
                Logger.Warn(ex.StackTrace);
                ExceptionReporter.ReportException(ex);
            }

        }

        /// <summary>
        /// Attempting to run a second copy of the program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void singleInstance_ArgumentsReceived(object _, ArgumentsReceivedEventArgs e) {
            try {
                if (_mw != null) {
                    Action<string[]> restoreWindow = arguments => {
                        _mw.WindowState = FormWindowState.Normal;
                        _mw.Activate();
                    };

                    _mw.Invoke(restoreWindow);
                }
            } catch (Exception ex) {
                ExceptionReporter.ReportException(ex, "singleInstance_ArgumentsReceived");
            }
        }

        // TODO: This creates another session instance, should be executed inside the ThreadExecuter
        private static void PrintStartupInfo(SessionFactory factory) {

            if (Properties.Settings.Default.UserNeverWantsBackups)
                Logger.Warn("You have opted out of backups");


            var mods = GlobalPaths.TransferFiles;
            if (mods.Count == 0) {
                Logger.Warn("No transfer files has been found");
            }
            else {
                Logger.Info("The following transfer files has been found:");
                foreach (var mod in mods) {
                    Logger.Info($"\"{mod.Filename}\": Mod: \"{mod.Mod}\", HC: {mod.IsHardcore}");
                }
            }

            var gdPath = new DatabaseSettingDaoImpl(factory).GetCurrentDatabasePath();
            if (string.IsNullOrEmpty(gdPath)) {
                Logger.Info("The path to Grim Dawn is unknown (not great)");
            }
            else {
                Logger.Info($"The path to Grim Dawn is \"{gdPath}\"");
            }

            Logger.Info("Startup data dump complete");
        }

        private static void Run(ThreadExecuter threadExecuter) {
            var factory = new SessionFactory();

            // Prohibited for now
            Properties.Settings.Default.InstaTransfer = false;
            Properties.Settings.Default.Save();
            
            IDatabaseSettingDao databaseSettingDao = new DatabaseSettingRepo(threadExecuter, factory);
            LoadUuid(databaseSettingDao);
            IDatabaseItemDao databaseItemDao = new DatabaseItemRepo(threadExecuter, factory);
            IDatabaseItemStatDao databaseItemStatDao = new DatabaseItemStatRepo(threadExecuter, factory);
            IItemTagDao itemTagDao = new ItemTagRepo(threadExecuter, factory);
            

            // TODO: GD Path has to be an input param, as does potentially mods.
            ParsingService parsingService = new ParsingService(itemTagDao, null, databaseItemDao, databaseItemStatDao, Properties.Settings.Default.LocalizationFile);

            PrintStartupInfo(factory);



            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Logger.Info("Visual styles enabled..");
            UpgradeSettings();

            using (CefBrowserHandler browser = new CefBrowserHandler()) {
                _mw = new MainWindow(browser, 
                    databaseItemDao, 
                    databaseSettingDao, 
                    parsingService
                );

                Logger.Info("Checking for database updates..");
            

                // Load the GD database (or mod, if any)
                string GDPath = databaseSettingDao.GetCurrentDatabasePath();
                bool isVanilla;
                if (string.IsNullOrEmpty(GDPath) || !Directory.Exists(GDPath)) {
                    GDPath = GrimDawnDetector.GetGrimLocation();
                    isVanilla = true;
                } else {
                    isVanilla = GDPath.Equals(GrimDawnDetector.GetGrimLocation());
                }

                if (!string.IsNullOrEmpty(GDPath) && Directory.Exists(GDPath)) {

                    var numFiles = Directory.GetFiles(GlobalPaths.StorageFolder).Length;
                    if (numFiles < 2000) {
                        Logger.Debug($"Only found {numFiles} in storage, expected ~3200+, parsing item icons.");
                        ThreadPool.QueueUserWorkItem((m) => ArzParser.LoadIconsOnly(GDPath));
                    }

                } else {
                    Logger.Warn("Could not find the Grim Dawn install location");
                }

                _mw.Visible = false;
                if (DonateNagScreen.CanNag)
                    Application.Run(new DonateNagScreen());

                Logger.Info("Running the main application..");


                Application.Run(_mw);
            }

            Logger.Info("Application ended.");
        }
    }
} 