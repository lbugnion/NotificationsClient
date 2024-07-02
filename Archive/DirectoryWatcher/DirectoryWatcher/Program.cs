using System;
using System.Collections.Generic;
using System.IO;

namespace DirectoryWatcher
{
    class Program
    {
        private static List<string> _extensions;

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Syntax: dotnet DirectoryWatcher.dll [PATH] [EXTENSIONS]");
                Console.ReadLine();
                return;
            }

            var path = args[0];

            if (!Directory.Exists(path))
            {
                Console.WriteLine($"Directory not found: {path}");
                Console.ReadLine();
                return;
            }

            _extensions = new List<string>(args);
            _extensions.RemoveAt(0);

            var watcher = new FileSystemWatcher();
            watcher.Path = path;
            watcher.Created += WatcherCreated;
            watcher.EnableRaisingEvents = true;

            Console.WriteLine("Press Enter to end");
            Console.ReadLine();
        }

        private static void WatcherCreated(object sender, FileSystemEventArgs e)
        {
            var isExtensionValid = false;


        }
    }
}
