/// <summary>
/// IMG sharp namespace
/// </summary>
namespace IMGSharp
{
    /// <summary>
    /// IMG archive entry
    /// </summary>
    public interface IIMGArchiveEntry
    {
        /// <summary>
        /// IMG archive
        /// </summary>
        IIMGArchive Archive { get; }

        /// <summary>
        /// Data offset in bytes
        /// </summary>
        long Offset { get; }

        /// <summary>
        /// Length in bytes
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Full name
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Is new entry
        /// </summary>
        bool IsNewEntry { get; }

        /// <summary>
        /// Name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Delete IMG archive entry
        /// </summary>
        void Delete();

        /// <summary>
        /// Open IMG archive entry
        /// </summary>
        /// <returns>Stream to archive entry if successful, otherwise "null"</returns>
        IIMGArchiveEntryStream Open();
    }
}
