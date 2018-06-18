using IMGSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

/// <summary>
/// IMG sharp unit test namespace
/// </summary>
namespace IMGSharpUnitTest
{
    /// <summary>
    /// IMG unit test
    /// </summary>
    [TestClass]
    public class IMGUnitTest
    {
        /// <summary>
        /// Initialize archives
        /// </summary>
        private static void InitArchives()
        {
            if (!(File.Exists("./test1.img")))
            {
                IMGFile.CreateFromDirectory("..\\..\\test", "./test1.img");
            }
            if (!(File.Exists("./test2.img")))
            {
                IMGFile.CreateFromDirectory("..\\..\\test", "./test2.img", true);
            }
        }

        /// <summary>
        /// Create and read IMG files
        /// </summary>
        [TestMethod]
        public void CreateReadIMGFiles()
        {
            InitArchives();
            using (IMGArchive archive = IMGFile.Open("./test1.img", EIMGArchiveMode.Read))
            {
                Assert.IsNotNull(archive);
                Assert.IsTrue(archive.Entries.Length > 0);
            }
            using (IMGArchive archive = IMGFile.Open("./test2.img", EIMGArchiveMode.Read))
            {
                Assert.IsNotNull(archive);
                Assert.IsTrue(archive.Entries.Length > 0);
            }
        }

        /// <summary>
        /// Commit to IMG file
        /// </summary>
        [TestMethod]
        public void CommitToIMGFile()
        {
            InitArchives();
            if (File.Exists("test3.img"))
            {
                File.Delete("test3.img");
            }
            File.Copy("test1.img", "test3.img");
            using (IMGArchive archive = IMGFile.Open("./test3.img", EIMGArchiveMode.Update))
            {
                Assert.IsNotNull(archive);
                IMGArchiveEntry[] entries = archive.Entries;
                int entry_count = entries.Length;
                Assert.IsTrue(entries.Length > 0);
                IMGArchiveEntry entry = entries[0];
                string entry_name = entry.FullName;
                Console.WriteLine("Unpacking file \"" + entries[0].FullName + "\"");
                if (!(Directory.Exists("test")))
                {
                    Directory.CreateDirectory("test");
                }
                long entry_size = 0;
                using (Stream entry_stream = entry.Open())
                {
                    Assert.IsNotNull(entry_stream);
                    entry_size = entry_stream.Length;
                    Assert.AreEqual(entry_size, (long)(entry.Length));
                    entry_stream.Seek(0L, SeekOrigin.End);
                    for (int i = 0; i < 2048; i++)
                    {
                        entry_stream.WriteByte(0);
                    }
                    entry_size += 2048;
                }
                entries = archive.Entries;
                Assert.AreEqual(entry_count, entries.Length);
                entry = archive.GetEntry(entry_name);
                Assert.IsNotNull(entry);
                using (Stream entry_stream = entry.Open())
                {
                    Assert.AreEqual(entry_size, entry_stream.Length);
                    Assert.AreEqual(entry_size, entry.Length);
                    if (entry_size >= 2048)
                    {
                        entry_size -= 2048;
                        entry_stream.SetLength(entry_size);
                    }
                }
            }
        }

        /// <summary>
        /// Extract to test directory
        /// </summary>
        [TestMethod]
        public void ExtractToTestDirectory()
        {
            int entry_count = 0;
            InitArchives();
            using (IMGArchive archive = IMGFile.Open("./test1.img", EIMGArchiveMode.Read))
            {
                entry_count = archive.Entries.Length;
            }
            IMGFile.ExtractToDirectory("test1.img", "test1");
            Assert.IsTrue(entry_count <= Directory.GetFiles("test1", "*", SearchOption.AllDirectories).Length);
            using (IMGArchive archive = IMGFile.Open("./test2.img", EIMGArchiveMode.Read))
            {
                entry_count = archive.Entries.Length;
            }
            IMGFile.ExtractToDirectory("test2.img", "test2");
            Assert.IsTrue(entry_count <= Directory.GetFiles("test2", "*", SearchOption.AllDirectories).Length);
        }
    }
}
