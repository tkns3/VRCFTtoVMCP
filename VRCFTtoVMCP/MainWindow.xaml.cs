using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Shapes;
using System.Windows.Threading;
using VRCFTtoVMCP.Osc;

namespace VRCFTtoVMCP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly MainWindowViewModel _model;
        readonly VrcOscReceiver _receiver = new();
        readonly VMCPSender _sender = new();
        readonly System.Timers.Timer _timer = new(1000);

        public MainWindow()
        {
            InitializeComponent();
            Title = $"VRCFTtoVMCP v{Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}";
            _model = (MainWindowViewModel)DataContext;
            _timer.Elapsed += Timer_Elapsed;

            AppConfig? appConfig = ReadConfig();
            _model.AutoStart = appConfig?.autoStart ?? _model.AutoStart;
            _model.VmcpSendRatePerSec = appConfig?.rate?.ToString() ?? _model.VmcpSendRatePerSec;
            _model.VmcpSendDstAddr = appConfig?.addr1 ?? _model.VmcpSendDstAddr;
            _model.VmcpSendDstPort = appConfig?.port1?.ToString() ?? _model.VmcpSendDstPort;
            _model.VrcOscRecvSrcPort = appConfig?.port2?.ToString() ?? _model.VrcOscRecvSrcPort;
            _model.VrcOscSendDstPort = appConfig?.port3?.ToString() ?? _model.VrcOscSendDstPort;
            _model.EyeTargetPositionUse = appConfig?.EyeTarget?.Use ?? _model.EyeTargetPositionUse;
            _model.EyeTargetPositionMultiplierUp = appConfig?.EyeTarget?.MultiplierUp ?? _model.EyeTargetPositionMultiplierUp;
            _model.EyeTargetPositionMultiplierDown = appConfig?.EyeTarget?.MultiplierDown ?? _model.EyeTargetPositionMultiplierDown;
            _model.EyeTargetPositionMultiplierLeft = appConfig?.EyeTarget?.MultiplierLeft ?? _model.EyeTargetPositionMultiplierLeft;
            _model.EyeTargetPositionMultiplierRight = appConfig?.EyeTarget?.MultiplierRight ?? _model.EyeTargetPositionMultiplierRight;

            if (_model.AutoStart)
            {
                Start();
            }
        }

        void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"{DateTime.Now:HH:mm:ss.fff}   Tick");
            var count1 = MessageCount.CountClearThisApp2VMC();
            var count2 = MessageCount.CountClearThisApp2VRCFT();
            var count3 = MessageCount.CountClearVRCFT2ThisApp();
            Dispatcher.Invoke(() => UpdateUI(count1, count2, count3));
        }

        void UpdateUI(int count1, int count2, int count3)
        {
            _model.TrasmissionRateThisApp2VMC = count1;
            _model.TrasmissionRateThisApp2VRCFT = count2;
            _model.TrasmissionRateVRCFT2ThisApp = count3;
            _model.ArrowColorThisApp2VMC = count1 > 0 ? "#0f0" : "#000";
            _model.ArrowColorThisApp2VRCFT = count2 > 0 ? "#0f0" : "#000";
            _model.ArrowColorVRCFT2ThisApp = count3 > 0 ? "#0f0" : "#000";
        }

        void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_model.IsStop)
            {
                Start();
            }
            else
            {
                Stop();
            }
        }

        void Start()
        {
            try
            {
                VRChat.CreateVRCAvatarFile("avtr_00000000-0000-0000-0000-000000000000.json");
                VRChat.CreateVRCAvatarFile("avtr_00000000-0000-0000-0000-000000000001.json");

                _receiver.Start(port: int.Parse(_model.VrcOscRecvSrcPort));
                _sender.Start(
                    address: _model.VmcpSendDstAddr,
                    port: int.Parse(_model.VmcpSendDstPort),
                    fps: int.Parse(_model.VmcpSendRatePerSec));

                using OscClient oscClient = new(_model.VrcOscSendDstAddr, int.Parse(_model.VrcOscSendDstPort));
                oscClient.Send(new Message("/avatar/change", "avtr_00000000-0000-0000-0000-000000000001"));
                MessageCount.CountUpThisApp2VRCFT();

                _timer.AutoReset = true;
                _timer.Start();
                _model.IsStop = false;
                _model.ButtonText = "STOP";
                _model.StatusText = "Status: Running.";
            }
            catch (Exception ex)
            {
                _model.StatusText = $"{DateTime.Now}: Start Failed. [{ex.Message}]";
                _sender.Stop();
                _receiver.Stop();
                _timer.Stop();
            }
        }

        void Stop()
        {
            try
            {
                using OscClient oscClient = new(_model.VrcOscSendDstAddr, int.Parse(_model.VrcOscSendDstPort));
                oscClient.Send(new Message("/avatar/change", "avtr_00000000-0000-0000-0000-000000000000"));
                MessageCount.CountUpThisApp2VRCFT();
            }
            catch (Exception)
            {
                //
            }
            _sender.Stop();
            _receiver.Stop();
            _timer.Stop();
            MessageCount.CountClearAll();
            UpdateUI(0, 0, 0);
            _model.IsStop = true;
            _model.ButtonText = "START";
            _model.StatusText = "Status: Stopped.";
        }

        AppConfig? ReadConfig()
        {
            string path = "VRCFTtoVMCP.json";
            if (!File.Exists(path))
            {
                return new();
            }

            try
            {
                using var sr = File.OpenText(path);
                using var reader = new JsonTextReader(sr);
                var serializer = new JsonSerializer();
                var deserialized = serializer.Deserialize<AppConfig>(reader);
                return deserialized;
            }
            catch (Exception)
            {
                return new();
            }
        }

        void CreateConfig()
        {
            if (_model.IsSaveComplete)
            {
                _model.SaveButtonColor = "#FF8EACBB";
                _model.IsSaveComplete = false;
                return;
            }
            try
            {
                _model.IsSaving = true;
                string path = "VRCFTtoVMCP.json";
                AppConfig appConfig = new()
                {
                    autoStart = _model.AutoStart,
                    addr1 = _model.VmcpSendDstAddr,
                    port1 = int.Parse(_model.VmcpSendDstPort),
                    port2 = int.Parse(_model.VrcOscRecvSrcPort),
                    port3 = int.Parse(_model.VrcOscSendDstPort),
                    rate = int.Parse(_model.VmcpSendRatePerSec),
                    EyeTarget = new()
                    {
                        Use = _model.EyeTargetPositionUse,
                        MultiplierUp = _model.EyeTargetPositionMultiplierUp,
                        MultiplierDown = _model.EyeTargetPositionMultiplierDown,
                        MultiplierLeft = _model.EyeTargetPositionMultiplierLeft,
                        MultiplierRight = _model.EyeTargetPositionMultiplierRight,
                    }
                };
                var jsonString = JsonConvert.SerializeObject(appConfig, Formatting.Indented);
                File.WriteAllText(path, jsonString, System.Text.Encoding.UTF8);
                _model.IsSaving = false;
                _model.SaveButtonCompleteIcon = "Check";
                _model.IsSaveComplete = true;
                _model.StatusText = $"{DateTime.Now}: Save Success.";
            }
            catch (Exception ex)
            {
                _model.IsSaving = false;
                _model.SaveButtonColor = "#FFBC5052";
                _model.SaveButtonCompleteIcon = "Close";
                _model.IsSaveComplete = true;
                _model.StatusText = $"{DateTime.Now}: Save Failed. [{ex.Message}]";
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!_model.IsStop)
            {
                Stop();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CreateConfig();
        }
    }

    internal class AppConfigEyeTarget
    {
        public bool? Use = null;
        public int? MultiplierUp = null;
        public int? MultiplierDown = null;
        public int? MultiplierLeft = null;
        public int? MultiplierRight = null;
    }

    internal class AppConfig
    {
        public bool? autoStart = null;
        public string? addr1 = null;
        public int? port1 = null;
        public int? port2 = null;
        public int? port3 = null;
        public int? rate = null;
        public AppConfigEyeTarget? EyeTarget = null;
    }

    internal class ObservableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string? name = null)
        {
            if (!System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, newValue))
            {
                field = newValue;
                OnPropertyChanged(name);
                return true;
            }
            return false;
        }
    }

    internal class MainWindowViewModel : ObservableBase
    {
        private bool _IsStop = true;
        private bool _AutoStart = false;
        private string _VmcpSendRatePerSec = "30";
        private string _VmcpSendDstAddr = "127.0.0.1";
        private string _VmcpSendDstPort = "39540";
        private string _VrcOscRecvSrcAddr = "Any";
        private string _VrcOscRecvSrcPort = "9000";
        private string _VrcOscSendDstAddr = "127.0.0.1";
        private string _VrcOscSendDstPort = "9001";
        private int _TrasmissionRateThisApp2VMC = 0;
        private int _TrasmissionRateThisApp2VRCFT = 0;
        private int _TrasmissionRateVRCFT2ThisApp = 0;
        private string _ArrowColorThisApp2VMC = "#000";
        private string _ArrowColorThisApp2VRCFT = "#000";
        private string _ArrowColorVRCFT2ThisApp = "#000";
        private string _ButtonText = "START";
        private string _StatusText = "Status: Stopped.";

        public bool IsStop { get => _IsStop; set => SetProperty(ref _IsStop, value); }
        public bool AutoStart { get => _AutoStart; set => SetProperty(ref _AutoStart, value); }
        public string VmcpSendRatePerSec { get => _VmcpSendRatePerSec; set => SetProperty(ref _VmcpSendRatePerSec, value); }
        public string VmcpSendDstAddr { get => _VmcpSendDstAddr; set => SetProperty(ref _VmcpSendDstAddr, value); }
        public string VmcpSendDstPort { get => _VmcpSendDstPort; set => SetProperty(ref _VmcpSendDstPort, value); }
        public string VrcOscRecvSrcAddr { get => _VrcOscRecvSrcAddr; set => SetProperty(ref _VrcOscRecvSrcAddr, value); }
        public string VrcOscRecvSrcPort { get => _VrcOscRecvSrcPort; set => SetProperty(ref _VrcOscRecvSrcPort, value); }
        public string VrcOscSendDstAddr { get => _VrcOscSendDstAddr; set => SetProperty(ref _VrcOscSendDstAddr, value); }
        public string VrcOscSendDstPort { get => _VrcOscSendDstPort; set => SetProperty(ref _VrcOscSendDstPort, value); }
        public int TrasmissionRateThisApp2VMC { get => _TrasmissionRateThisApp2VMC; set => SetProperty(ref _TrasmissionRateThisApp2VMC, value); }
        public int TrasmissionRateThisApp2VRCFT { get => _TrasmissionRateThisApp2VRCFT; set => SetProperty(ref _TrasmissionRateThisApp2VRCFT, value); }
        public int TrasmissionRateVRCFT2ThisApp { get => _TrasmissionRateVRCFT2ThisApp; set => SetProperty(ref _TrasmissionRateVRCFT2ThisApp, value); }
        public string ArrowColorThisApp2VMC { get => _ArrowColorThisApp2VMC; set => SetProperty(ref _ArrowColorThisApp2VMC, value); }
        public string ArrowColorThisApp2VRCFT { get => _ArrowColorThisApp2VRCFT; set => SetProperty(ref _ArrowColorThisApp2VRCFT, value); }
        public string ArrowColorVRCFT2ThisApp { get => _ArrowColorVRCFT2ThisApp; set => SetProperty(ref _ArrowColorVRCFT2ThisApp, value); }
        public string ButtonText { get => _ButtonText; set => SetProperty(ref _ButtonText, value); }
        public string StatusText { get => _StatusText; set => SetProperty(ref _StatusText, value); }
        public bool EyeTargetPositionUse { get => DynamicSharedParameter.EyeTargetPositionUse; set { DynamicSharedParameter.EyeTargetPositionUse = value; OnPropertyChanged(); } }
        public int EyeTargetPositionMultiplierUp { get => DynamicSharedParameter.EyeTargetPositionMultiplierUp; set { DynamicSharedParameter.EyeTargetPositionMultiplierUp = value; OnPropertyChanged(); } }
        public int EyeTargetPositionMultiplierDown { get => DynamicSharedParameter.EyeTargetPositionMultiplierDown; set { DynamicSharedParameter.EyeTargetPositionMultiplierDown = value; OnPropertyChanged(); } }
        public int EyeTargetPositionMultiplierLeft { get => DynamicSharedParameter.EyeTargetPositionMultiplierLeft; set { DynamicSharedParameter.EyeTargetPositionMultiplierLeft = value; OnPropertyChanged(); } }
        public int EyeTargetPositionMultiplierRight { get => DynamicSharedParameter.EyeTargetPositionMultiplierRight; set { DynamicSharedParameter.EyeTargetPositionMultiplierRight = value; OnPropertyChanged(); } }

        private bool _IsSaving = false;
        private int _SaveProgress = 0;
        private bool _IsSaveComplete = false;
        private string _SaveButtonCompleteIcon = "Check";
        private string _SaveButtonColor = "#FF8EACBB";

        public bool IsSaving { get => _IsSaving; set => SetProperty(ref _IsSaving, value); }
        public int SaveProgress { get => _SaveProgress; set => SetProperty(ref _SaveProgress, value); }
        public bool IsSaveComplete { get => _IsSaveComplete; set => SetProperty(ref _IsSaveComplete, value); }
        public string SaveButtonCompleteIcon { get => _SaveButtonCompleteIcon; set => SetProperty(ref _SaveButtonCompleteIcon, value); }
        public string SaveButtonColor { get => _SaveButtonColor; set => SetProperty(ref _SaveButtonColor, value); }
    }

    internal class DynamicSharedParameter
    {
        private static readonly object _LockObject = new();
        private static bool _EyeTargetPositionUse = true;
        private static int _EyeTargetPositionMultiplierUp = 100;
        private static int _EyeTargetPositionMultiplierDown = 100;
        private static int _EyeTargetPositionMultiplierLeft = 100;
        private static int _EyeTargetPositionMultiplierRight = 100;

        public static bool EyeTargetPositionUse
        {
            get
            {
                bool ret;
                lock (_LockObject)
                {
                    ret = _EyeTargetPositionUse;
                }
                return ret;
            }
            set
            {
                lock (_LockObject)
                {
                    _EyeTargetPositionUse = value;
                }
            }
        }

        public static int EyeTargetPositionMultiplierUp
        {
            get
            {
                int ret;
                lock (_LockObject)
                {
                    ret = _EyeTargetPositionMultiplierUp;
                }
                return ret;
            }
            set
            {
                lock (_LockObject)
                {
                    _EyeTargetPositionMultiplierUp = value;
                }
            }
        }

        public static int EyeTargetPositionMultiplierDown
        {
            get
            {
                int ret;
                lock (_LockObject)
                {
                    ret = _EyeTargetPositionMultiplierDown;
                }
                return ret;
            }
            set
            {
                lock (_LockObject)
                {
                    _EyeTargetPositionMultiplierDown = value;
                }
            }
        }

        public static int EyeTargetPositionMultiplierLeft
        {
            get
            {
                int ret;
                lock (_LockObject)
                {
                    ret = _EyeTargetPositionMultiplierLeft;
                }
                return ret;
            }
            set
            {
                lock (_LockObject)
                {
                    _EyeTargetPositionMultiplierLeft = value;
                }
            }
        }

        public static int EyeTargetPositionMultiplierRight
        {
            get
            {
                int ret;
                lock (_LockObject)
                {
                    ret = _EyeTargetPositionMultiplierRight;
                }
                return ret;
            }
            set
            {
                lock (_LockObject)
                {
                    _EyeTargetPositionMultiplierRight = value;
                }
            }
        }
    }

    internal static class MessageCount
    {
        private static readonly object _LockObjectVRCFT2ThisApp = new();
        private static readonly object _LockObjectThisApp2VRCFT = new();
        private static readonly object _LockObjectThisApp2VMC = new();
        private static int _CountVRCFT2ThisApp = 0;
        private static int _CountThisApp2VRCFT = 0;
        private static int _CountThisApp2VMC = 0;

        public static int CountUpVRCFT2ThisApp()
        {
            int count;
            lock (_LockObjectVRCFT2ThisApp)
            {
                _CountVRCFT2ThisApp++;
                count = _CountVRCFT2ThisApp;
            }
            return count;
        }

        public static int CountUpThisApp2VRCFT()
        {
            int count;
            lock (_LockObjectThisApp2VRCFT)
            {
                _CountThisApp2VRCFT++;
                count = _CountThisApp2VRCFT;
            }
            return count;
        }

        public static int CountUpThisApp2VMC()
        {
            int count;
            lock (_LockObjectThisApp2VMC)
            {
                _CountThisApp2VMC++;
                count = _CountThisApp2VMC;
            }
            return count;
        }

        public static int CountClearVRCFT2ThisApp()
        {
            int count = 0;
            lock (_LockObjectVRCFT2ThisApp)
            {
                count = _CountVRCFT2ThisApp;
                _CountVRCFT2ThisApp = 0;
            }
            return count;
        }

        public static int CountClearThisApp2VRCFT()
        {
            int count = 0;
            lock (_LockObjectThisApp2VRCFT)
            {
                count = _CountThisApp2VRCFT;
                _CountThisApp2VRCFT = 0;
            }
            return count;
        }

        public static int CountClearThisApp2VMC()
        {
            int count = 0;
            lock (_LockObjectThisApp2VMC)
            {
                count = _CountThisApp2VMC;
                _CountThisApp2VMC = 0;
            }
            return count;
        }

        public static void CountClearAll()
        {
            _CountVRCFT2ThisApp = 0;
            _CountThisApp2VRCFT = 0;
            _CountThisApp2VMC = 0;
        }
    }
}
