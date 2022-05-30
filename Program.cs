using MediaDevices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PotatoSync
{
    class Program
    {
        static int retryCount = 5;

        static MediaDevice device; 
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("  Usage: PotatoSync <srcFolder> <dstFolder> <phoneFriendlyName>");
                Console.WriteLine("Example: PotatoSync c:\\Music\\Flac  \\Phone\\Music\\Flac\\ \"Galaxy Note9\"");
                Environment.Exit(-1);
            }

            var devices = MediaDevice.GetDevices();
            if (devices.Count() > 0)
            {
                device = devices.First(d => d.FriendlyName == args[2]);
                device.Connect();

                var sourceFolder = args[0];
                var destinationFolder = args[1];
                var srcDirectory = Directory.EnumerateDirectories(sourceFolder);
                RecurseFolders(
                    srcDirectory,
                    sourceFolder,
                    destinationFolder
                    );
            }
            else
            {
                Console.WriteLine("No media device found");
            }

        }

        static void RecurseFolders(
            IEnumerable<string> srcDirectory,
            string sourceFolder,
            string destinationFolder
            )
        {
            try
            {
                if (!srcDirectory.Any())
                {
                    Console.Write(".");
                    return;
                }
                foreach (var folder in srcDirectory)
                {
                    RecurseFolders(
                        Directory.EnumerateDirectories(folder),
                        sourceFolder,
                        destinationFolder
                        ); 
                    var srcFiles = Directory.EnumerateFiles(folder);
                    foreach(var file in srcFiles)
                    {
                        var fileName = string.Concat(destinationFolder, file.Replace(sourceFolder + "\\", ""));
                        if (!device.FileExists(fileName))
                        {
                            Console.WriteLine(file + " ==> " + fileName);
                            device.CreateDirectory(Path.GetDirectoryName(fileName));
                            device.UploadFile(file, fileName);
                        }
                    }
                }
            }
            catch (System.Runtime.InteropServices.COMException cex)
            {
                if( retryCount < 0)
                {
                    Console.WriteLine("Phone connection failed: " + cex);
                    Environment.Exit(-1);

                }
                Task.Delay(1000).Wait();
                try
                {
                    device.Disconnect();
                }
                catch { }

                device.Connect();
            }
            catch (Exception ex)
            {
                 Console.WriteLine("Yikes: " + ex);
            }
        }
    }
}
