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
        /// On close IMG archive entry event
        /// </summary>
        public event CloseIMGArchiveEntryDelegate OnCloseIMGArchiveEntry;

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
            OnCloseIMGArchiveEntry?.Invoke(IMGArchiveEntry, this);
            Stream?.Dispose();
        }
    }
}
