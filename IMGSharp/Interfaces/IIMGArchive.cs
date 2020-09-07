using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// IMG sharp namespace
/// </summary>
namespace IMGSharp
{
    /// <summary>
    /// IMG archive interface
    /// </summary>
    public interface IIMGArchive : IDisposable
    {
        /// <summary>
        /// IMG archive stream
        /// </summary>
        Stream Stream { get; }

        /// <summary>
        /// IMG archive entries
        /// </summary>
        IReadOnlyDictionary<string, IIMGArchiveEntry> Entries { get; }

        /// <summary>
        /// IMG archive access mode
        /// </summary>
        EIMGArchiveAccessMode AccessMode { get; }

        /// <summary>
        /// IMG archive entry name encoding
        /// </summary>
        Encoding EntryNameEncoding { get; }

        /// <summary>
        /// Create IMG archive entry
        /// </summary>
        /// <param name="entryName">Entry name</param>
        /// <returns>IMG archive entry if successful, otherwise "null"</returns>
        IIMGArchiveEntry CreateEntry(string entryName);

        /// <summary>
        /// Get IMG archive entry
        /// </summary>
        /// <param name="entryName">Entry name</param>
        /// <returns>IMG archive entry if successful, otherwise "null"</returns>
        IIMGArchiveEntry GetEntry(string entryName);
    }
}
