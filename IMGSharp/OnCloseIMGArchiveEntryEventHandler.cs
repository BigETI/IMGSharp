/// <summary>
/// IMG sharp namespace
/// </summary>
namespace IMGSharp
{
    /// <summary>
    /// On close IMG archive event handler delegate
    /// </summary>
    /// <param name="entry">IMG archive entry</param>
    /// <param name="stream">IMG archive entry stream</param>
    internal delegate void OnCloseIMGArchiveEntryEventHandler(IMGArchiveEntry entry, IMGArchiveEntryStream stream);
}
