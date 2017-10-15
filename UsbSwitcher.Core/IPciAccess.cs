using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsbSwitcher.Core
{
    public interface IPciAccess
    {
        uint ReadUint32(PciDevice device, ushort offset);
        ushort ReadUint16(PciDevice device, ushort offset);
        byte ReadByte(PciDevice device, ushort offset);
        void Write(PciDevice device, ushort offset, byte value);
        void Write(PciDevice device, ushort offset, ushort value);
        void Write(PciDevice device, ushort offset, uint value);
    }
}
