using System;
using System.IO;

/// <summary>
/// IMG sharp namespace
/// </summary>
namespace IMGSharp
{
    /// <summary>
    /// IMG utilities class
    /// </summary>
    internal static class IMGUtils
    {
        /// <summary>
        /// Get null terminated byte string length
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Length of null terminated byte string</returns>
        public static int GetNullTerminatedByteStringLength(byte[] bytes)
        {
            int ret = 0;
            if (bytes != null)
            {
                ret = bytes.Length;
                for (int i = 0; i < bytes.Length; i++)
                {
                    if (bytes[i] == 0)
                    {
                        ret = i;
                        break;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Get relative path
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="relativeToPath">Relative to path</param>
        /// <returns>Relative path of "path"</returns>
        public static string GetRelativePath(string path, string relativeToPath)
        {
            return (new Uri(relativeToPath.EndsWith("\\") ? relativeToPath : (relativeToPath.EndsWith("/") ? relativeToPath : (relativeToPath + Path.DirectorySeparatorChar)))).MakeRelativeUri(new Uri(path)).ToString();
        }
    }
}
