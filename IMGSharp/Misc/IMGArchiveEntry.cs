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
    internal class IMGArchiveEntry : IIMGArchiveEntry
    {
        /// <summary>
        /// IMG archive
        /// </summary>
        private IMGArchive archive;

        /// <summary>
        /// Data is available
        /// </summary>
        private bool available = true;

        /// <summary>
        /// IMG archive
        /// </summary>
        public IIMGArchive Archive => archive;

        /// <summary>
        /// Data offset in bytes
        /// </summary>
        public long Offset { get; }

        /// <summary>
        /// Length in bytes
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Full name
        /// </summary>
        public string FullName { get; }

        /// <summary>
        /// Is new entry
        /// </summary>
        public bool IsNewEntry { get; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name => Path.GetFileName(FullName);

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
            Offset = offset;
            Length = length;
            FullName = fullName;
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
            Offset = offset;
            Length = length;
            FullName = fullName;
            IsNewEntry = isNewEntry;
        }

        /// <summary>
        /// Commit changes
        /// </summary>
        private void Commit(IIMGArchiveEntryStream stream)
        {
            if (archive.AccessMode != EIMGArchiveAccessMode.Read)
            {
                archive.CommitEntry(this, stream);
            }
        }

        /// <summary>
        /// Delete IMG archive entry
        /// </summary>
        public void Delete()
        {
            if (available)
            {
                available = false;
                if (archive.InternalEntries.Remove(FullName.ToLower()))
                {
                    archive.CommitEntry(null, null);
                }
            }
        }

        /// <summary>
        /// Open IMG archive entry
        /// </summary>
        /// <returns>Stream to archive entry if successful, otherwise "null"</returns>
        public IIMGArchiveEntryStream Open()
        {
            IIMGArchiveEntryStream ret = null;
            try
            {
                if (available && archive.Stream.CanRead)
                {
                    byte[] data = new byte[Length];
                    archive.Stream.Seek(Offset, SeekOrigin.Begin);
                    archive.Stream.Read(data, 0, Length);
                    ret = new IMGArchiveEntryStream(this);
                    ret.Stream.Write(data, 0, data.Length);
                    ret.Stream.Seek(0L, SeekOrigin.Begin);
                    ret.OnIMGArchiveEntryClosed += (entry, stream) => (entry as IMGArchiveEntry)?.Commit(stream);
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
    }
}
