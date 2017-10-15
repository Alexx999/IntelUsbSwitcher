using System.ServiceProcess;
using UsbSwitcher.Core;

namespace UsbSwitcher.Service
{
    partial class SwitcherService : ServiceBase
    {
        private uint _initialValue;
        private bool _applied;

        public SwitcherService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if(IntelXhci.GetSupportState() != SupportState.Supported) return;

            _initialValue = IntelXhci.GetRouting();
            IntelXhci.SwitchTo20();
            _applied = true;
        }

        protected override void OnStop()
        {
            if (_applied)
            {
                IntelXhci.SetRouting(_initialValue);
            }
            DirectIo.Shutdown();
        }
    }
}
