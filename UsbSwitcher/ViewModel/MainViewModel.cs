using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using UsbSwitcher.Core;
using System.ServiceProcess;
using System.Management;
using System.Configuration.Install;

namespace UsbSwitcher.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private static string _serviceName = "Intel USB Switcher Service";

        public SupportState BasicSupportState { get; set; }

        public SupportState EhciControllersState { get; set; }

        public int SwitchablePorts { get; set; }

        public bool? IsRunningOn30 { get; set; }
        public bool CanTest { get; set; }
        public bool TestRunning { get; set; }
        public bool TestSuccess { get; set; }
        public int TestCountdown { get; set; }

        public bool ServiceEnabled { get; set; }

        public bool? ServiceRunning { get; set; }

        public ICommand TestCommand { get; }
        public ICommand FinishTestCommand { get; }
        public ICommand EnableServiceCommand { get; }
        public ICommand DisableServiceCommand { get; }
        public ICommand StartServiceCommand { get; }
        public ICommand StopServiceCommand { get; }

        public MainViewModel()
        {
            if (IsInDesignMode) return;

            Initialize();

            TestCommand = new RelayCommand(async () => await StartTest());
            FinishTestCommand = new RelayCommand(EndTestSuccess);
            EnableServiceCommand = new RelayCommand(async () => await EnableService());
            DisableServiceCommand = new RelayCommand(async () => await DisableService());
            StartServiceCommand = new RelayCommand(StartService);
            StopServiceCommand = new RelayCommand(StopService);
        }

        private async Task StartTest()
        {
            if (BasicSupportState != SupportState.Supported)
            {
                return;
            }

            if (EhciControllersState != SupportState.Supported)
            {
                if (MessageBox.Show("EHCI controllers not detected - USB devices might stop working (including mouse and keyboard)\nIf this happens - just wait for 30s until timeout ends", "Warning", MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                {
                    return;
                }
            }

            IntelXhci.SwitchTo20();

            CanTest = false;
            TestRunning = true;
            TestCountdown = 30;
            
            do
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                if (TestSuccess) break;
                TestCountdown -= 1;
            } while (TestCountdown > 0);

            if(TestSuccess) return;

            EndTest();

            IntelXhci.SwitchTo30();

            MessageBox.Show("Button not clicked in time - assuming that it didn't work well");
        }

        private async Task EnableService()
        {
            if (ServiceRunning == null)
            {
                MessageBox.Show("Service not installed");
                return;
            }

            await Task.Run(() => SetServiceEnabled(_serviceName, true));
            ServiceEnabled = await Task.Run(() => GetServiceEnabled(_serviceName));
        }

        private async Task DisableService()
        {
            if (!ServiceEnabled)
            {
                return;
            }

            await Task.Run(() => SetServiceEnabled(_serviceName, false));
            ServiceEnabled = await Task.Run(() => GetServiceEnabled(_serviceName));
        }

        private void StartService()
        {
            if (ServiceRunning == null)
            {
                MessageBox.Show("Service not installed");
                return;
            }
            
            var service = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == _serviceName);
            service?.Start();

            if (service != null)
            {
                ServiceRunning = service.Status == ServiceControllerStatus.Running;
            }
        }

        private void StopService()
        {
            var service = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == _serviceName);
            service?.Stop();

            if (service != null)
            {
                ServiceRunning = service.Status == ServiceControllerStatus.Running;
            }
        }

        private void EndTest()
        {
            TestRunning = false;
            CanTest = true;
        }

        private void EndTestSuccess()
        {
            TestSuccess = true;
            EndTest();
        }

        private async void Initialize()
        {
            var state = await Task.Run(() =>
            {
                var res = new Status();
                res.Initialize();
                return res;
            });

            BasicSupportState = state.BasicSupportState;
            EhciControllersState = state.EhciControllersState;
            SwitchablePorts = state.SwitchablePorts;
            CanTest = state.CanTest;
            IsRunningOn30 = state.IsRunningOn30;
            ServiceRunning = state.ServiceRunning;
            ServiceEnabled = state.ServiceEnabled;
        }
        
        private static bool GetServiceEnabled(string serviceName)
        {
            using (var querySearch = new ManagementObjectSearcher($"SELECT StartMode FROM Win32_Service WHERE Name = '{serviceName}'"))
            {
                var service = querySearch.Get().Cast<ManagementBaseObject>().FirstOrDefault();

                if (service == null) return false;

                return service.GetPropertyValue("StartMode").ToString() == "Automatic";
            }
        }

        private static void SetServiceEnabled(string serviceName, bool enabled)
        {
            using (var m = new ManagementObject($"Win32_Service.Name='{serviceName}'"))
            {
                m.InvokeMethod("ChangeStartMode", new object[] { enabled ? "Automatic" : "Disabled" });
            }
        }

        private class Status
        {
            public SupportState BasicSupportState { get; private set; }

            public SupportState EhciControllersState { get; private set; }

            public int SwitchablePorts { get; private set; }

            public bool CanTest { get; private set; }

            public bool? IsRunningOn30 { get; private set; }
            public bool? ServiceRunning { get; private set; }
            public bool ServiceEnabled { get; private set; }

            
            internal void Initialize()
            {
                BasicSupportState = IntelXhci.GetSupportState();

                if (BasicSupportState == SupportState.FailDriverLoad) return;

                EhciControllersState = IntelXhci.GetEhciState();

                if (BasicSupportState != SupportState.Supported) return;

                SwitchablePorts = IntelXhci.GetNumberOfSwitchablePorts();

                IsRunningOn30 = IntelXhci.IsRunningOn30();

                CanTest = SwitchablePorts > 0;

                var service = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == _serviceName);

                if (service == null)
                {
                    InstallService();
                }

                service = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == _serviceName);

                if (service != null)
                {
                    ServiceRunning = service.Status == ServiceControllerStatus.Running;
                }

                ServiceEnabled = GetServiceEnabled(_serviceName);
            }

            private void InstallService()
            {
                var executablePath = "UsbSwitcher.Service.exe";
                ManagedInstallerClass.InstallHelper(new string[] { executablePath });
            }

            private void UninstallService()
            {
                var executablePath = "UsbSwitcher.Service.exe";
                ManagedInstallerClass.InstallHelper(new string[] { "/u", executablePath });
            }
        }
    }
}
