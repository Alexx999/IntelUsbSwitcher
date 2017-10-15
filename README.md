# Intel USB Controller Switcher

Tool to enable use of EHCI controller for USB 2.0 devices instead of xHCI controller on Intel platforms that have it.

This allows to save limited xHCI resources.

Supported chipsets:
* Intel 7 series
* Intel 8 series
* Intel 9 series
* Intel x99

Supported OS:
* Windows 7
* Windows 8
* Windows 10

On Windows 7 requires [.NET 4.5](http://go.microsoft.com/fwlink/p/?LinkId=245484) to run.

To use this tool - [install](https://github.com/Alexx999/IntelUsbSwitcher/releases), launch "Intel USB Switcher" app, run test to ensure that your system is compatible and click confirmation button if your input works. Then click "Enable service" to have this applied on system startup.