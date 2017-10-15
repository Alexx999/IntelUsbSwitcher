using System;

namespace UsbSwitcher.Core
{
    public class IntelConf1 : IPciAccess
    {
        private static uint GetConfigAddress(byte bus, byte dev, byte func, ushort pos)
        {
            return 0x80000000 | (uint)(bus << 16) | (uint)(PCI_DEVFN(dev, func) << 8) | (uint)(pos & ~3);
        }

        private static byte PCI_DEVFN(int dev, int func)
        {
            return (byte) (((dev & 0x1f) << 3) | (func & 0x07));
        }

        public bool Detect()
        {
            if (!DirectIo.Startup())
            {
                return false;
            }

            DirectIo.Write(1, 0xCFB);
            var tmp = DirectIo.ReadUInt32(0xCF8);
            DirectIo.Write(0x80000000, 0xCF8);
            var result = DirectIo.ReadUInt32(0xCF8) == 0x80000000;
            DirectIo.Write(tmp, 0xCF8);
            
            return result && Check();
        }

        private bool Check()
        {
            for (byte i = 0; i < 32; i++)
            {
                var device = new PciDevice(this, 0, i, 0);

                var cls = device.Class;
                if (cls != PciClass.BridgeHost && cls != PciClass.DisplayVga) continue;

                var vendor = device.Vendor;

                if (vendor != PciVendor.Intel && vendor != PciVendor.Compaq) continue;

                return true;
            }

            return false;
        }

        public uint ReadUint32(PciDevice device, ushort offset)
        {
            var ptr = PrepareAccess(device, offset);
            return DirectIo.ReadUInt32(ptr);
        }

        public ushort ReadUint16(PciDevice device, ushort offset)
        {
            var ptr = PrepareAccess(device, offset);
            return DirectIo.ReadUInt16(ptr);
        }

        public byte ReadByte(PciDevice device, ushort offset)
        {
            var ptr = PrepareAccess(device, offset);
            return DirectIo.ReadByte(ptr);
        }

        public void Write(PciDevice device, ushort offset, byte value)
        {
            var ptr = PrepareAccess(device, offset);
            DirectIo.Write(value, ptr);
        }

        public void Write(PciDevice device, ushort offset, ushort value)
        {
            var ptr = PrepareAccess(device, offset);
            DirectIo.Write(value, ptr);
        }

        public void Write(PciDevice device, ushort offset, uint value)
        {
            var ptr = PrepareAccess(device, offset);
            DirectIo.Write(value, ptr);
        }

        private ushort PrepareAccess(PciDevice d, ushort pos)
        {
            if (pos >= 256)
            {
                throw new ArgumentOutOfRangeException(nameof(pos));
            }

            var code = GetConfigAddress(d.Bus, d.Dev, d.Func, pos);

            DirectIo.Write(code, 0xcf8);

            var addr = (ushort) (0xcfc + (ushort)(pos & 3));
            return addr;
        }
    }
}
