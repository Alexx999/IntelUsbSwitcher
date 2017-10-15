using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace UsbSwitcher.Core
{
	public static class DllLoader
	{
		
		[DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
		static extern IntPtr LoadLibrary(string lpFileName);
        
		public static void Load(string dllName, Version version, string x32Resource, string x64Resource)
		{
			var thisAss = Assembly.GetExecutingAssembly();


		    var arch = IntPtr.Size == 4 ? "x32" : "x64";
            
		    var assemblyName = Path.GetFileNameWithoutExtension(thisAss.CodeBase);

            // Get a temporary directory in which we can store the unmanaged DLL, with
            // this assembly's version number in the path in order to avoid version
            // conflicts in case two applications are running at once with different versions
            var dirName = Path.Combine(Path.GetTempPath(), $"{assemblyName}{Path.DirectorySeparatorChar}{version}{Path.DirectorySeparatorChar}{arch}");

			try
			{
				if (!Directory.Exists(dirName))
					Directory.CreateDirectory(dirName);
			}
			catch
			{
				// raced?
				if (!Directory.Exists(dirName))
					throw;
			}

			var dllFullName = Path.Combine(dirName, dllName);

		    var resourceName = IntPtr.Size == 4 ? x32Resource : x64Resource;

            // Get the embedded resource stream that holds the Internal DLL in this assembly.
            // The name looks funny because it must be the default namespace of this project
            // (MyAssembly.) plus the name of the Properties subdirectory where the
            // embedded resource resides (Properties.) plus the name of the file.
            if (!File.Exists(dllFullName))
			{
				// Copy the assembly to the temporary file
				var tempFile = Path.GetTempFileName();
				using (var stm = thisAss.GetManifestResourceStream($"{assemblyName}.{resourceName}"))
				{
                    if(stm == null) return;

					using (Stream outFile = File.Create(tempFile))
					{
						stm.CopyTo(outFile);
					}
				}

				try
				{
					File.Move(tempFile, dllFullName);
				}
				catch
				{
					// clean up tempfile
					try
					{
						File.Delete(tempFile);
					}
					catch
					{
						// eat
					}

					// raced?
					if (!File.Exists(dllFullName))
						throw;
				}

			}

			// We must explicitly load the DLL here because the temporary directory is not in the PATH.
			// Once it is loaded, the DllImport directives that use the DLL will use the one that is already loaded into the process.
			var hFile = LoadLibrary(dllFullName);
			if (hFile == IntPtr.Zero)
				throw new Exception("Can't load " + dllFullName);
		}
	}
}
