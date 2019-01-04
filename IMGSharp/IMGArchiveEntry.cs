using System;
using System.IO;

/// <summary>
/// IMG sharp namespace
/// </summary>
namespace IMGSharp
{
    /// <summary>
    /// IMG entry class
    /// </summary>
    public class IMGArchiveEntry
    {
        /// <summary>
        /// Archive
        /// </summary>
        private IMGArchive archive;

        /// <summary>
        /// Data offset
        /// </summary>
        private long offset;

        /// <summary>
        /// Length in bytes
        /// </summary>
        private int length;

        /// <summary>
        /// Full name
        /// </summary>
        private string fullName;

        /// <summary>
        /// Is new entry
        /// </summary>
        private bool isNewEntry;

        /// <summary>
        /// Data is available
        /// </summary>
        private bool available = true;

        /// <summary>
        /// Data offset
        /// </summary>
        internal long Offset
        {
            get
            {
                return offset;
            }
        }

        /// <summary>
        /// Length in bytes
        /// </summary>
        public int Length
        {
            get
            {
                return length;
            }
        }

        /// <summary>
        /// Full name
        /// </summary>
        public string FullName
        {
            get
            {
                return fullName;
            }
        }

        /// <summary>
        /// Is new entry
        /// </summary>
        internal bool IsNewEntry
        {
            get
            {
                return isNewEntry;
            }
        }

        /// <summary>
        /// Name
        /// </summary>
        public string Name
        {
            get
            {
                return Path.GetFileName(fullName);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="archive">Archive</param>
        /// <param name="offset">Data offset</param>
        /// <param name="length">Length in bytes</param>
        /// <param name="fullName">Full name</param>
        internal IMGArchiveEntry(IMGArchive archive, long offset, int length, string fullName)
        {
            this.archive = archive;
            this.offset = offset;
            this.length = length;
            this.fullName = fullName;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="archive">Archive</param>
        /// <param name="offset">Data offset</param>
        /// <param name="length">Length in bytes</param>
        /// <param name="fullName">Full name</param>
        /// <param name="isNewEntry">Is new entry</param>
        internal IMGArchiveEntry(IMGArchive archive, long offset, int length, string fullName, bool isNewEntry)
        {
            this.archive = archive;
            this.offset = offset;
            this.length = length;
            this.fullName = fullName;
            this.isNewEntry = isNewEntry;
        }

        /// <summary>
        /// Delete IMG archive entry
        /// </summary>
        public void Delete()
        {
            if (available)
            {
                available = false;
                archive.entries.Remove(fullName.ToLower());
                archive.CommitEntry(null, null);
            }
        }

        /// <summary>
        /// Open IMG archive entry
        /// </summary>
        /// <returns>Stream to archive entry if successful, otherwise "null"</returns>
        public Stream Open()
        {
            IMGArchiveEntryStream ret = null;
            try
            {
                if (available && archive.Stream.CanRead)
                {
                    byte[] data = new byte[length];
                    archive.Stream.Seek(offset, SeekOrigin.Begin);
                    archive.Stream.Read(data, 0, length);
                    ret = new IMGArchiveEntryStream(this);
                    ret.Write(data, 0, data.Length);
                    ret.Seek(0L, SeekOrigin.Begin);
                    ret.OnClose += (entry, stream) =>
                    {
                        if (entry != null)
                        {
                            entry.Commit(stream);
                        }
                    };
                }
            }
            catch (Exception e)
            {
                if (ret != null)
                {
                    ret.Dispose();
                    ret = null;
                }
                Console.Error.WriteLine(e);
            }
            return ret;
        }

        /// <summary>
        /// Commit changes
        /// </summary>
        private void Commit(IMGArchiveEntryStream stream)
        {
            if (archive.Mode != EIMGArchiveMode.Read)
            {
                archive.CommitEntry(this, stream);
            }
        }
    }
}
