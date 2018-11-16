using System;
using System.IO;
using System.Linq;
using IMGSharp;

namespace ConsoleIMG
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("IMG file location: ");
            string fileLocation = Console.ReadLine();
            if (File.Exists(@fileLocation))
            {
                Console.WriteLine("Reading the IMG file!");
                IMGArchive imgArchive = IMGFile.Open(@fileLocation, EIMGArchiveMode.Read);
                IMGArchiveEntry[] iMGArchiveEntries = imgArchive.Entries.OrderBy(x => x.Name).ToArray();
                imgArchive.Dispose();
                imgArchive.Entries = iMGArchiveEntries;

                for (int i = 0; i < imgArchive.Entries.Length; i++)
                {
                    IMGArchiveEntry entry = imgArchive.Entries[i];
                    Console.WriteLine("Entry file " + i + " name is: " + entry.Name);
                }

                Console.WriteLine("Total entries is: " + imgArchive.Entries.Length);
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("File doesn't exist");
                Console.ReadLine();
            }
        }
    }
}
