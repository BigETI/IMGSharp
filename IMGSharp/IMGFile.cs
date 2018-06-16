using System;
using System.IO;
using System.Text;

/// <summary>
/// IMG sharp namespace
/// </summary>
namespace IMGSharp
{
    /// <summary>
    /// IMG file class
    /// </summary>
    public static class IMGFile
    {
        /// <summary>
        /// Create archive from directory
        /// </summary>
        /// <param name="sourceDirectoryName">Source directory name</param>
        /// <param name="destinationArchiveFileName">Destination directory name</param>
        public static void CreateFromDirectory(string sourceDirectoryName, string destinationArchiveFileName)
        {
            CreateFromDirectory(sourceDirectoryName, destinationArchiveFileName, false);
        }

        public static void CreateFromDirectory(string sourceDirectoryName, string destinationArchiveFileName, bool includeBaseDirectory)
        {
            // TODO
            throw new NotImplementedException();
        }

        public static void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName)
        {
            // TODO
            throw new NotImplementedException();
        }

        /// <summary>
        /// Open archive
        /// </summary>
        /// <param name="archiveFileName">Archive file name</param>
        /// <param name="archiveMode">Archive mode</param>
        /// <returns>IMG archive if successful, otherwise "null"</returns>
        public static IMGArchive Open(string archiveFileName, EIMGArchiveMode archiveMode)
        {
            return Open(archiveFileName, archiveMode, Encoding.UTF8);
        }

        /// <summary>
        /// Open IMG archive
        /// </summary>
        /// <param name="archiveFileName">Archive file name</param>
        /// <returns>IMG archive if successful, otherwise "null"</returns>
        public static IMGArchive Open(string archiveFileName, EIMGArchiveMode archiveMode, Encoding entryNameEncoding)
        {
            IMGArchive ret = null;
            if ((archiveFileName != null) && (entryNameEncoding != null))
            {
                try
                {
                    if ((archiveMode == EIMGArchiveMode.Create) || File.Exists(archiveFileName))
                    {
                        ret = new IMGArchive(File.Open(archiveFileName, (archiveMode == EIMGArchiveMode.Create) ? FileMode.Create : FileMode.Open), entryNameEncoding);
                        if (archiveMode == EIMGArchiveMode.Create)
                        {
                            using (BinaryWriter writer = new BinaryWriter(ret.Stream, Encoding.UTF8, true))
                            {
                                writer.Write(Encoding.UTF8.GetBytes("VER2"));
                                writer.Write(0);
                                writer.Write((short)0);
                            }
                        }
                        else
                        {
                            using (BinaryReader reader = new BinaryReader(ret.Stream, Encoding.UTF8, true))
                            {
                                string version = Encoding.UTF8.GetString(reader.ReadBytes(4));
                                uint num_entries = reader.ReadUInt32();
                                if ((version == "VER2") && (ret.Stream.Length >= (num_entries * 8)))
                                {
                                    for (uint num_entry = 0U; num_entry != num_entries; num_entry++)
                                    {
                                        long offset = reader.ReadInt32() * 2048U;
                                        int length = reader.ReadInt16() * 2048;
                                        uint size_in_archive = reader.ReadUInt16();
                                        byte[] full_name_bytes_raw = reader.ReadBytes(24);
                                        long full_name_bytes_count = IMGUtils.GetNullTerminatedBytesLenghtFromBytes(full_name_bytes_raw);
                                        if (full_name_bytes_count > 0L)
                                        {
                                            byte[] full_name_bytes = new byte[full_name_bytes_count];
                                            Array.Copy(full_name_bytes_raw, full_name_bytes, full_name_bytes_count);
                                            string full_name = entryNameEncoding.GetString(full_name_bytes);
                                            ret.entries.Add(full_name.ToLower(), new IMGArchiveEntry(ret, offset, length, full_name));
                                        }
                                        else
                                        {
                                            throw new InvalidDataException("IMG entry name can't be empty.");
                                        }
                                    }
                                }
                                else
                                {
                                    throw new InvalidDataException("\"" + archiveFileName + "\" is not an IMG file");
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (ret != null)
                    {
                        ret.Dispose();
                        ret = null;
                    }
                    Console.Error.WriteLine(e.Message);
                }
            }
            return ret;
        }

        /// <summary>
        /// Open IMG archive in read only mode
        /// </summary>
        /// <param name="archiveFileName">Archive file name</param>
        /// <returns></returns>
        public static IMGArchive OpenRead(string archiveFileName)
        {
            return Open(archiveFileName, EIMGArchiveMode.Read);
        }
    }
}
