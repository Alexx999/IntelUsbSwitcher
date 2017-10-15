using System.ServiceProcess;
using UsbSwitcher.Core;

namespace UsbSwitcher.Service
{
    partial class SwitcherService : ServiceBase
    {
        private uint _initialValue;

        public SwitcherService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _initialValue = IntelXhci.GetRouting();
            IntelXhci.SwitchTo20();
        }

        protected override void OnStop()
        {
            IntelXhci.SetRouting(_initialValue);
        }
    }
}
