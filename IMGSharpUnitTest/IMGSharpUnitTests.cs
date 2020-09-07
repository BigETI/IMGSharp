using IMGSharp;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

/// <summary>
/// IMG sharp unit test namespace
/// </summary>
namespace IMGSharpUnitTest
{
    /// <summary>
    /// IMG sharp unit tests class
    /// </summary>
    public class IMGSharpUnitTests
    {
        /// <summary>
        /// "test1.img" file path
        /// </summary>
        private static readonly string testDotOneDotIMGFilePath = Path.GetFullPath("./test1.img");

        /// <summary>
        /// "test2.img" file path
        /// </summary>
        private static readonly string testDotTwoDotIMGFilePath = Path.GetFullPath("./test2.img");

        /// <summary>
        /// "test3.img" file path
        /// </summary>
        private static readonly string testDotThreeDotIMGFilePath = Path.GetFullPath("./test3.img");

        /// <summary>
        /// "test" directory path
        /// </summary>
        private static readonly string testDirectoryPath = Path.GetFullPath("../../../test");

        /// <summary>
        /// Initialize IMG archives
        /// </summary>
        private void InitializeIMGArchives()
        {
            if (!File.Exists(testDotOneDotIMGFilePath))
            {
                IMGFile.CreateFromDirectory(testDirectoryPath, testDotOneDotIMGFilePath);
            }
            if (!File.Exists(testDotTwoDotIMGFilePath))
            {
                IMGFile.CreateFromDirectory(testDirectoryPath, testDotTwoDotIMGFilePath, true);
            }
        }

        /// <summary>
        /// Setup
        /// </summary>
        [SetUp]
        public void Setup() => InitializeIMGArchives();

        /// <summary>
        /// Create and read IMG files
        /// </summary>
        [Test]
        public void CreateReadIMGFiles()
        {
            InitializeIMGArchives();
            using (IIMGArchive archive = IMGFile.Open(testDotOneDotIMGFilePath, EIMGArchiveAccessMode.Read))
            {
                Assert.IsNotNull(archive);
                Assert.Less(0, archive.Entries.Count);
            }
            using (IIMGArchive archive = IMGFile.Open(testDotOneDotIMGFilePath, EIMGArchiveAccessMode.Read))
            {
                Assert.IsNotNull(archive);
                Assert.Less(0, archive.Entries.Count);
            }
        }

        /// <summary>
        /// Commit to IMG file
        /// </summary>
        [Test]
        public void CommitToIMGFile()
        {
            InitializeIMGArchives();
            if (File.Exists(testDotThreeDotIMGFilePath))
            {
                File.Delete(testDotThreeDotIMGFilePath);
            }
            File.Copy(testDotOneDotIMGFilePath, testDotThreeDotIMGFilePath);
            using (IIMGArchive archive = IMGFile.Open(testDotThreeDotIMGFilePath, EIMGArchiveAccessMode.Update))
            {
                string entry_name = string.Empty;
                long entry_size = 0;
                Assert.IsNotNull(archive);
                IReadOnlyDictionary<string, IIMGArchiveEntry> entries = archive.Entries;
                int entry_count = entries.Count;
                Assert.Less(0, entry_count);
                foreach (IIMGArchiveEntry entry in entries.Values)
                {
                    entry_name = entry.FullName;
                    Debug.WriteLine($"Unpacking file \"{ entry.FullName }\"");
                    if (!(Directory.Exists("test")))
                    {
                        Directory.CreateDirectory("test");
                    }
                    using (IIMGArchiveEntryStream img_archive_entry_stream = entry.Open())
                    {
                        Assert.IsNotNull(img_archive_entry_stream);
                        entry_size = img_archive_entry_stream.Stream.Length;
                        Assert.AreEqual(entry_size, (long)(entry.Length));
                        img_archive_entry_stream.Stream.Seek(0L, SeekOrigin.End);
                        using (BinaryWriter img_archive_entry_stream_binary_writer = new BinaryWriter(img_archive_entry_stream.Stream, archive.EntryNameEncoding, true))
                        {
                            byte[] zero_bytes = new byte[2048];
                            img_archive_entry_stream_binary_writer.Write(zero_bytes);
                            img_archive_entry_stream_binary_writer.Flush();
                            entry_size += zero_bytes.Length;
                        }
                    }
                    break;
                }
                if (entry_count > 0)
                {
                    entries = archive.Entries;
                    Assert.AreEqual(entry_count, entries.Count);
                    IIMGArchiveEntry another_entry = archive.GetEntry(entry_name);
                    Assert.IsNotNull(another_entry);
                    using (IIMGArchiveEntryStream img_archive_entry_stream = another_entry.Open())
                    {
                        Assert.AreEqual(entry_size, img_archive_entry_stream.Stream.Length);
                        Assert.AreEqual(entry_size, another_entry.Length);
                        if (entry_size >= 2048)
                        {
                            entry_size -= 2048;
                            img_archive_entry_stream.Stream.SetLength(entry_size);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Extract to test directory
        /// </summary>
        [Test]
        public void ExtractToTestDirectory()
        {
            int entry_count = 0;
            InitializeIMGArchives();
            using (IIMGArchive archive = IMGFile.Open(testDotOneDotIMGFilePath, EIMGArchiveAccessMode.Read))
            {
                entry_count = archive.Entries.Count;
            }
            IMGFile.ExtractToDirectory(testDotOneDotIMGFilePath, "test1");
            Assert.LessOrEqual(entry_count, Directory.GetFiles("test1", "*", SearchOption.AllDirectories).Length);
            using (IIMGArchive archive = IMGFile.Open(testDotTwoDotIMGFilePath, EIMGArchiveAccessMode.Read))
            {
                entry_count = archive.Entries.Count;
            }
            IMGFile.ExtractToDirectory(testDotTwoDotIMGFilePath, "test2");
            Assert.LessOrEqual(entry_count, Directory.GetFiles("test2", "*", SearchOption.AllDirectories).Length);
        }
    }
}
