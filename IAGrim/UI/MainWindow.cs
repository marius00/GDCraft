using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using EvilsoftCommons;
using EvilsoftCommons.Exceptions;
using IAGrim.Database.Interfaces;
using IAGrim.Parsers;
using IAGrim.Parsers.Arz;
using IAGrim.Parsers.Arz.dto;
using IAGrim.Parsers.GameDataParsing.Service;
using IAGrim.Services;
using IAGrim.Services.Crafting;
using IAGrim.UI.Controller;
using IAGrim.UI.Misc;
using IAGrim.Utilities;
using log4net;
using MoreLinq;
using Timer = System.Timers.Timer;

namespace IAGrim.UI {

    public partial class MainWindow : Form {
        public readonly JSWrapper JsBind = new JSWrapper();
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MainWindow));
        readonly CefBrowserHandler _cefBrowserHandler;

        private readonly TooltipHelper _tooltipHelper = new TooltipHelper();
        private DateTime _lastTimeNotMinimized = DateTime.Now;

        private StashManager _stashManager;
        private StashFileMonitor _stashFileMonitor = new StashFileMonitor();
        private JsonBindingService _jsonBindingService;


        private Timer _timerReportUsage;
        
        private readonly IDatabaseItemDao _databaseItemDao;
        private readonly IDatabaseSettingDao _databaseSettingDao;
        private readonly ParsingService _parsingService;

        private readonly Stopwatch _reportUsageStatistics;


        public MainWindow(
            CefBrowserHandler browser,
             IDatabaseItemDao databaseItemDao,
             IDatabaseSettingDao databaseSettingDao,
            ParsingService parsingService) {
            _cefBrowserHandler = browser;
            InitializeComponent();
            FormClosing += MainWindow_FormClosing;

            _reportUsageStatistics = new Stopwatch();
            _reportUsageStatistics.Start();

            _databaseItemDao = databaseItemDao;
            _databaseSettingDao = databaseSettingDao;
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
                }
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

            _stashFileMonitor?.Dispose();
            _stashFileMonitor = null;
            _stashManager = null;


            _timerReportUsage?.Stop();
            _timerReportUsage?.Dispose();
            _timerReportUsage = null;

            _tooltipHelper?.Dispose();


            IterAndCloseForms(Controls);
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
            buttonDevTools.Visible = Debugger.IsAttached;

            _stashManager = new StashManager();
            _stashFileMonitor.OnStashModified += (_, __) => {
                StashEventArg args = __ as StashEventArg;
                _stashManager.UpdateUnlooted(args?.Filename);
            };

            _cefBrowserHandler.InitializeChromium(JsBind);
            searchPanel.Controls.Add(_cefBrowserHandler.BrowserControl);


            // Load the grim database
            string gdPath = GrimDawnDetector.GetGrimLocation();
            if (!string.IsNullOrEmpty(gdPath)) {
            } else {
                Logger.Warn("Could not find the Grim Dawn install location");

                var timer = new System.Windows.Forms.Timer();
                timer.Tick += TimerTickLookForGrimDawn;
                timer.Interval = 10000;
                timer.Start();
            }


            var addAndShow = UIHelper.AddAndShow;

            var costCalculationService = new CostCalculationService(_stashManager);
            _jsonBindingService = new JsonBindingService(_stashManager, JsBind, _cefBrowserHandler, new RecipeService(_databaseItemDao), costCalculationService);
            
            addAndShow(new ModsDatabaseConfig(() => {
                _jsonBindingService.JsBind_OnRequestRecipeList(null, null);
            }, _parsingService), modsPanel);

            addAndShow(
                new SettingsWindow(
                    _tooltipHelper,
                    _databaseSettingDao,
                    _databaseItemDao
                ),
                settingsPanel);

            
#if !DEBUG
            ThreadPool.QueueUserWorkItem(m => ExceptionReporter.ReportUsage());
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



            Application.AddMessageFilter(new MousewheelMessageFilter());

        }



        private void button1_Click(object sender, EventArgs e) {
            _cefBrowserHandler.ShowDevTools();
        }


    } // CLASS



}