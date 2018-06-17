using System;
using System.Diagnostics;
using System.IO;
using IMGSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        /// Create and read IMG files
        /// </summary>
        [TestMethod]
        public void CreateReadIMGFiles()
        {
            IMGFile.CreateFromDirectory("../../../IMGSharp", "./test1.img");
            using (IMGArchive archive = IMGFile.Open("./test1.img", EIMGArchiveMode.Read))
            {
                Assert.IsNotNull(archive);
                Assert.IsTrue(archive.Entries.Length > 0);
            }
            IMGFile.CreateFromDirectory("../../../IMGSharp", "./test2.img", true);
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
            using (IMGArchive archive = IMGFile.Open("./test1.img", EIMGArchiveMode.Read))
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
                    Assert.Equals(entry_size, (long)(entry.Length));
                    entry_stream.WriteByte(0);
                    ++entry_size;
                }
                entries = archive.Entries;
                Assert.Equals(entry_count, entries.Length);
                entry = archive.GetEntry(entry_name);
                Assert.IsNotNull(entry);
                using (Stream entry_stream = entry.Open())
                {
                    Assert.AreEqual(entry_size, entry_stream.Length);
                    Assert.AreEqual(entry_size, entry.Length);
                    if (entry_size > 0)
                    {
                        entry_stream.SetLength(--entry_size);
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
            using (IMGArchive archive = IMGFile.Open("./test2.img", EIMGArchiveMode.Read))
            {
                entry_count = archive.Entries.Length;
            }
            IMGFile.ExtractToDirectory("test2.img", "test");
            Assert.IsTrue(entry_count <= Directory.GetFiles("test", "*", SearchOption.AllDirectories).Length);
        }
    }
}
