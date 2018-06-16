using System.IO;

/// <summary>
/// IMG sharp namespace
/// </summary>
namespace IMGSharp
{
    /// <summary>
    /// IMG archive entry stream class
    /// </summary>
    internal class IMGArchiveEntryStream : MemoryStream
    {
        /// <summary>
        /// IMG archive entry
        /// </summary>
        private IMGArchiveEntry imgArchiveEntry;

        /// <summary>
        /// On close event
        /// </summary>
        public event OnCloseIMGArchiveEntryEventHandler OnClose;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="imgArchiveEntry">IMG archive entry</param>
        public IMGArchiveEntryStream(IMGArchiveEntry imgArchiveEntry) : base()
        {
            this.imgArchiveEntry = imgArchiveEntry;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="imgArchiveEntry"></param>
        /// <param name="buffer">Buffer</param>
        public IMGArchiveEntryStream(IMGArchiveEntry imgArchiveEntry, byte[] buffer) : base(buffer)
        {
            this.imgArchiveEntry = imgArchiveEntry;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="imgArchiveEntry">IMG archive entry</param>
        /// <param name="capacity">Capacity</param>
        public IMGArchiveEntryStream(IMGArchiveEntry imgArchiveEntry, int capacity) : base(capacity)
        {
            this.imgArchiveEntry = imgArchiveEntry;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="imgArchiveEntry">IMG archive entry</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="writable">Writable</param>
        public IMGArchiveEntryStream(IMGArchiveEntry imgArchiveEntry, byte[] buffer, bool writable) : base(buffer, writable)
        {
            this.imgArchiveEntry = imgArchiveEntry;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="imgArchiveEntry">IMG archive entry</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="index">Index</param>
        /// <param name="count">Count</param>
        public IMGArchiveEntryStream(IMGArchiveEntry imgArchiveEntry, byte[] buffer, int index, int count) : base(buffer, index, count)
        {
            this.imgArchiveEntry = imgArchiveEntry;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="imgArchiveEntry">IMG archive entry</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="index">Index</param>
        /// <param name="count">Count</param>
        /// <param name="writable">Writable</param>
        public IMGArchiveEntryStream(IMGArchiveEntry imgArchiveEntry, byte[] buffer, int index, int count, bool writable) : base(buffer, index, count, writable)
        {
            this.imgArchiveEntry = imgArchiveEntry;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="imgArchiveEntry">IMG archive entry</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="index">Index</param>
        /// <param name="count">Count</param>
        /// <param name="writable">Writable</param>
        /// <param name="publiclyVisible">Publicly visible</param>
        public IMGArchiveEntryStream(IMGArchiveEntry imgArchiveEntry, byte[] buffer, int index, int count, bool writable, bool publiclyVisible) : base(buffer, index, count, writable, publiclyVisible)
        {
            this.imgArchiveEntry = imgArchiveEntry;
        }

        /// <summary>
        /// Close
        /// </summary>
        public override void Close()
        {
            if (OnClose != null)
            {
                OnClose(imgArchiveEntry, this);
                OnClose = null;
            }
            base.Close();
        }
    }
}
