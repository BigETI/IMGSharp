using System.IO;

/// <summary>
/// IMG sharp namespace
/// </summary>
namespace IMGSharp
{
    /// <summary>
    /// IMG archive entry stream class
    /// </summary>
    internal class IMGArchiveEntryStream : IIMGArchiveEntryStream
    {
        /// <summary>
        /// Stream
        /// </summary>
        public Stream Stream { get; }

        /// <summary>
        /// IMG archive entry
        /// </summary>
        public IIMGArchiveEntry IMGArchiveEntry { get; }

        /// <summary>
        /// On IMG archive entry closed
        /// </summary>
        public event IMGArchiveEntryClosedDelegate OnIMGArchiveEntryClosed;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="imgArchiveEntry">IMG archive entry</param>
        public IMGArchiveEntryStream(IIMGArchiveEntry imgArchiveEntry)
        {
            IMGArchiveEntry = imgArchiveEntry;
            Stream = new MemoryStream();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            OnIMGArchiveEntryClosed?.Invoke(IMGArchiveEntry, this);
            Stream?.Dispose();
        }
    }
}
