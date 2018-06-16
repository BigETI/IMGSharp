using System;
using System.Collections.Generic;
using System.Text;

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
        /// Get null terminated bytes length from bytes
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Length of null terminated bytes without the null terminator</returns>
        public static long GetNullTerminatedBytesLenghtFromBytes(byte[] bytes)
        {
            long ret = -1L;
            if (bytes != null)
            {
                ret = bytes.Length;
                for (long i = 0; i < bytes.LongLength; i++)
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
    }
}
