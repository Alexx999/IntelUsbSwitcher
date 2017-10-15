using System.ServiceProcess;

namespace UsbSwitcher.Service
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var servicesToRun = new ServiceBase[]
            {
                new SwitcherService()
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}
