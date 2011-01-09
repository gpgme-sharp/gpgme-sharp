using System;
using System.Text;
using System.IO;

using Libgpgme;

namespace DataBufferTest
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			// Read sample data
			byte[] bytedata = File.ReadAllBytes(@"daniel.pub");
			
			// Create memory based buffer for GPGME
			GpgmeMemoryData memdata = new GpgmeMemoryData();
			// write sample data into the GPGME memory based buffer
			Console.WriteLine("Bytes written: " + memdata.Write(bytedata, bytedata.Length));
			
			// Can seek?
			Console.WriteLine("Can seek: " + memdata.CanSeek);
			
			// Set the cursor to the beginning
			Console.WriteLine("Seek to begin: " + memdata.Seek(0, SeekOrigin.Begin));
			
			// Re-read the data into a tempory buffer
			byte[] tmp = new byte[bytedata.Length];
			Console.WriteLine("Bytes read: " + memdata.Read(tmp));
			
			return;
		}
	}
}

