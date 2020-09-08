/// <summary>
/// IMG sharp namespace
/// </summary>
namespace IMGSharp
{
    /// <summary>
    /// IMG archive entry closed delegate
    /// </summary>
    /// <param name="entry">IMG archive entry</param>
    /// <param name="stream">IMG archive entry stream</param>
    public delegate void IMGArchiveEntryClosedDelegate(IIMGArchiveEntry entry, IIMGArchiveEntryStream stream);
}
