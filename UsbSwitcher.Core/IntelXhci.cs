using System.Collections.Generic;

namespace UsbSwitcher.Core
{
    public static class IntelXhci
    {
        private static Dictionary<ushort, EhciIds> Supported = new Dictionary<ushort, EhciIds>
        {
            {0x1E31, new EhciIds(0x1E26, 0x1E2D)}, // 7 series chipsets
            {0x8C31, new EhciIds(0x8C26, 0x8C2D)}, // 8 series chipsets
            {0x8CB1, new EhciIds(0x8CA6, 0x8CAD)}, // 9 series chipsets
            {0x8D31, new EhciIds(0x8D26, 0x8D2D)}  // x99 chipset
        };

        private static readonly IntelConf1 _access = new IntelConf1();

        public static SupportState GetSupportState()
        {
            if (!_access.Detect())
            {
                return SupportState.FailDriverLoad;
            }

            var device = new PciDevice(_access, 0, 20, 0);

            if (device.Vendor == 0 || device.Vendor == (PciVendor) ushort.MaxValue || device.DeviceId == 0 || device.DeviceId == ushort.MaxValue)
            {
                return SupportState.FailNoUsb30;
            }

            if (device.Vendor != PciVendor.Intel || device.Class != PciClass.UsbController)
            {
                return SupportState.FailNotUsb30;
            }

            if (!Supported.ContainsKey(device.DeviceId))
            {
                return SupportState.FailChipsetNotSupported;
            }

            return SupportState.Supported;
        }

        public static SupportState GetEhciState()
        {
            var device = new PciDevice(_access, 0, 20, 0);

            Supported.TryGetValue(device.DeviceId, out var ehci);

            var firstEhci = new PciDevice(_access, 0, 29, 0);

            if (firstEhci.DeviceId != ehci.Primary)
            {
                return SupportState.WarnPrimaryEhciInactive;
            }

            var secondEhci = new PciDevice(_access, 0, 26, 0);

            if (secondEhci.DeviceId != ehci.Secondary)
            {
                return SupportState.WarnSecondaryEhciInactive;
            }

            return SupportState.Supported;
        }

        public static int GetNumberOfSwitchablePorts()
        {
            var device = new PciDevice(_access, 0, 20, 0);
            var mask = device.ReadUInt32(0xD4);

            return NumberOfSetBits((int)mask);
        }

        public static bool IsRunningOn30()
        {
            var device = new PciDevice(_access, 0, 20, 0);
            var mask = device.ReadUInt32(0xD4);
            var state = device.ReadUInt32(0xD0);

            return mask == state;
        }

        public static void SwitchTo20()
        {
            var device = new PciDevice(_access, 0, 20, 0);
            device.Write(0xD0, (uint)0);
        }

        public static void SwitchTo30()
        {
            var device = new PciDevice(_access, 0, 20, 0);
            var mask = device.ReadUInt32(0xD4);
            device.Write(0xD0, mask);
        }

        public static uint GetRouting()
        {
            var device = new PciDevice(_access, 0, 20, 0);
            return device.ReadUInt32(0xD0);
        }

        public static void SetRouting(uint value)
        {
            var device = new PciDevice(_access, 0, 20, 0);
            device.Write(0xD0, value);
        }

        private static int NumberOfSetBits(int i)
        {
            i = i - ((i >> 1) & 0x55555555);
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
        }

        private struct EhciIds
        {
            public EhciIds(ushort primary, ushort secondary)
            {
                Primary = primary;
                Secondary = secondary;
            }

            public ushort Primary { get; }
            public ushort Secondary { get; }
        }
    }

    public enum SupportState
    {
        Unknown, Supported, FailDriverLoad, FailNoUsb30, FailNotUsb30, FailChipsetNotSupported, WarnPrimaryEhciInactive, WarnSecondaryEhciInactive
    }
}
