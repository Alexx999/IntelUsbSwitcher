using System;
using System.Runtime.InteropServices;

namespace UsbSwitcher.Core
{

    public static class DirectIo
    {

        [DllImport("directio.dll", EntryPoint = "DirectIO_Init")]
        private static extern bool Init();

        [DllImport("directio.dll", EntryPoint = "DirectIO_DeInit")]
        private static extern bool Deinit();

        [DllImport("directio.dll", EntryPoint = "DirectIO_WritePort")]
        private static extern bool WritePort(uint value, ushort port, PortSize portSize);

        [DllImport("directio.dll", EntryPoint = "DirectIO_ReadPort")]
        private static extern bool ReadPort(out uint value, ushort port, PortSize portSize);

        public enum PortSize
        {
            Byte = 1,
            Word = 2,
            DWord = 3
        }

        private static readonly object Locker = new object();
        private static bool _dllLoaded;
        private static bool _initialized;
        
        public static bool Startup()
        {
            if (_initialized) return true;

            lock (Locker)
            {
                if (_initialized) return true;

                if (!_dllLoaded)
                {
                    DllLoader.Load("directio.dll", new Version(0, 2, 1, 0), "DirectIOLib32.dll", "DirectIOLibx64.dll");
                    _dllLoaded = true;
                }
                _initialized = Init();
                return _initialized;
            }
        }

        public static void Shutdown()
        {
            if(!_initialized) return;

            lock (Locker)
            {
                if (!_initialized) return;

                Deinit();
                _initialized = false;
            }
        }

        public static void Write(byte value, ushort port)
        {
            WritePort(value, port, PortSize.Byte);
        }

        public static void Write(ushort value, ushort port)
        {
            WritePort(value, port, PortSize.Word);
        }

        public static void Write(uint value, ushort port)
        {
            WritePort(value, port, PortSize.DWord);
        }

        public static uint ReadUInt32(ushort port)
        {
            ReadPort(out var value, port, PortSize.DWord);
            return value;
        }

        public static ushort ReadUInt16(ushort port)
        {
            ReadPort(out var value, port, PortSize.Word);
            return (ushort) value;
        }

        public static byte ReadByte(ushort port)
        {
            ReadPort(out var value, port, PortSize.Byte);
            return (byte) value;
        }
    }
}
