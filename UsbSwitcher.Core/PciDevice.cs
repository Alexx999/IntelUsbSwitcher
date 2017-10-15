using System;
using System.Threading;

namespace UsbSwitcher.Core
{
    public class PciDevice
    {
        private const ushort PCI_VENDOR_ID = 0x00; /* 16 bits */
        private const ushort PCI_DEVICE_ID = 0x02; /* 16 bits */

        private const ushort PCI_CLASS_DEVICE = 0x0a;

        private readonly Lazy<PciClass> classLazy;
        private readonly Lazy<PciVendor> vendorLazy;
        private readonly Lazy<ushort> deviceIdLazy;

        public PciDevice(IPciAccess access, byte bus, byte dev, byte func)
        {
            PciAccess = access;
            Bus = bus;
            Dev = dev;
            Func = func;

            classLazy = new Lazy<PciClass>(() => (PciClass) ReadUInt16(PCI_CLASS_DEVICE), LazyThreadSafetyMode.None);
            vendorLazy = new Lazy<PciVendor>(() => (PciVendor) ReadUInt16(PCI_VENDOR_ID), LazyThreadSafetyMode.None);
            deviceIdLazy = new Lazy<ushort>(() => ReadUInt16(PCI_DEVICE_ID), LazyThreadSafetyMode.None);
        }

        private IPciAccess PciAccess { get; }
        public byte Bus { get; }
        public byte Dev { get; }
        public byte Func { get; }

        public uint ReadUInt32(ushort pos)
        {
            return PciAccess.ReadUint32(this, pos);
        }

        public ushort ReadUInt16(ushort pos)
        {
            return PciAccess.ReadUint16(this, pos);
        }

        public ushort ReadByte(ushort pos)
        {
            return PciAccess.ReadByte(this, pos);
        }

        public void Write(ushort pos, byte value)
        {
            PciAccess.Write(this, pos, value);
        }

        public void Write(ushort pos, ushort value)
        {
            PciAccess.Write(this, pos, value);
        }

        public void Write(ushort pos, uint value)
        {
            PciAccess.Write(this, pos, value);
        }

        public PciClass Class => classLazy.Value;
        public PciVendor Vendor => vendorLazy.Value;
        public ushort DeviceId => deviceIdLazy.Value;
    }

    public enum PciClass : ushort
    {
        DisplayVga = 0x0300,
        BridgeHost = 0x0600,
        UsbController = 0x0c03,
    }

    public enum PciVendor : ushort
    {
        Compaq = 0x0e11,
        Intel = 0x8086
    }
}
