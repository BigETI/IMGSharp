using System;
using System.IO;

/// <summary>
/// IMG sharp namespace
/// </summary>
namespace IMGSharp
{
    /// <summary>
    /// IMG archive entry stream interface
    /// </summary>
    public interface IIMGArchiveEntryStream : IDisposable
    {
        /// <summary>
        /// Stream
        /// </summary>
        Stream Stream { get; }

        /// <summary>
        /// IMG archive entry
        /// </summary>
        IIMGArchiveEntry IMGArchiveEntry { get; }

        /// <summary>
        /// On IMG archive entry closed
        /// </summary>
        event IMGArchiveEntryClosedDelegate OnIMGArchiveEntryClosed;
    }
}
