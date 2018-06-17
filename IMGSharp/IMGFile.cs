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
    /// IMG file class
    /// </summary>
    public static class IMGFile
    {
        /// <summary>
        /// Create IMG archive from directory
        /// </summary>
        /// <param name="sourceDirectoryName">Source directory name</param>
        /// <param name="destinationArchiveFileName">Destination directory name</param>
        public static void CreateFromDirectory(string sourceDirectoryName, string destinationArchiveFileName)
        {
            CreateFromDirectory(sourceDirectoryName, destinationArchiveFileName, false, Encoding.UTF8);
        }

        /// <summary>
        /// Create IMG archive from directory
        /// </summary>
        /// <param name="sourceDirectoryName">Source directory name</param>
        /// <param name="destinationArchiveFileName">Destination directory name</param>
        /// <param name="includeBaseDirectory">Include base directory</param>
        public static void CreateFromDirectory(string sourceDirectoryName, string destinationArchiveFileName, bool includeBaseDirectory)
        {
            CreateFromDirectory(sourceDirectoryName, destinationArchiveFileName, includeBaseDirectory, Encoding.UTF8);
        }

        /// <summary>
        /// Create IMG archive from directory
        /// </summary>
        /// <param name="sourceDirectoryName">Source directory name</param>
        /// <param name="destinationArchiveFileName">Destination aechive file name</param>
        /// <param name="includeBaseDirectory">Include base directory into archive</param>
        /// <param name="entryNameEncoding">Entry name encoding</param>
        public static void CreateFromDirectory(string sourceDirectoryName, string destinationArchiveFileName, bool includeBaseDirectory, Encoding entryNameEncoding)
        {
            try
            {
                if ((sourceDirectoryName != null) && (destinationArchiveFileName != null) && (entryNameEncoding != null))
                {
                    if (Directory.Exists(sourceDirectoryName))
                    {
                        string[] files = (includeBaseDirectory ? Directory.GetFiles(sourceDirectoryName + Path.PathSeparator + "..", sourceDirectoryName, SearchOption.AllDirectories) : Directory.GetFiles(sourceDirectoryName, "*", SearchOption.AllDirectories));
                        string[] relative_files = new string[files.Length];
                        bool file_name_lenghts_ok = true;
                        for (int i = 0; i < files.Length; i++)
                        {
                            relative_files[i] = IMGUtils.GetRelativePath(files[i], includeBaseDirectory ? (sourceDirectoryName + Path.PathSeparator + "..") : sourceDirectoryName).Replace('\\', '/');
                            if (relative_files[i].Length > 24)
                            {
                                file_name_lenghts_ok = false;
                                break;
                            }
                        }
                        if (file_name_lenghts_ok)
                        {
                            using (FileStream archive_stream = File.Open(destinationArchiveFileName, FileMode.Create))
                            {
                                using (BinaryWriter archive_writer = new BinaryWriter(archive_stream))
                                {
                                    int first_entry_offset = ((2048 % (files.Length * 32) == 0) ? (2048 / (files.Length * 32)) : ((2048 / (files.Length * 32)) + 1));
                                    int current_entry_offset = first_entry_offset;
                                    List<KeyValuePair<string, int>> entries = new List<KeyValuePair<string, int>>();
                                    archive_writer.Write(new byte[] { 0x56, 0x45, 0x52, 0x32, (byte)(files.Length & 0xFF), (byte)((files.Length >> 8) & 0xFF), 0x0, 0x0 });
                                    for (int i = 0; i < files.Length; i++)
                                    {
                                        long file_length = (new FileInfo(files[i])).Length;
                                        int entry_length = (int)(((file_length % 2048L) == 0L) ? (file_length / 2048L) : ((file_length / 2048L) + 1L));
                                        byte[] name_bytes_raw = entryNameEncoding.GetBytes(relative_files[i]);
                                        byte[] name_bytes = new byte[24];
                                        Array.Copy(name_bytes_raw, name_bytes, Math.Min(name_bytes_raw.Length, name_bytes.Length));
                                        archive_writer.Write(current_entry_offset);
                                        archive_writer.Write(new byte[] { (byte)(entry_length & 0xFF), (byte)((entry_length >> 8) & 0xFF), 0x0, 0x0 });
                                        archive_writer.Write(name_bytes);
                                        entries.Add(new KeyValuePair<string, int>(files[i], current_entry_offset));
                                        current_entry_offset += entry_length;
                                    }
                                    foreach (KeyValuePair<string, int> entry in entries)
                                    {
                                        using (FileStream stream = File.Open(entry.Key, FileMode.Open))
                                        {
                                            while ((archive_stream.Length / 2048) < entry.Value)
                                            {
                                                archive_stream.WriteByte(0);
                                            }
                                            byte[] data = new byte[stream.Length];
                                            stream.Read(data, 0, (int)(stream.Length));
                                            archive_writer.Write(data);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Extract IMG archive to directory
        /// </summary>
        /// <param name="sourceArchiveFileName">Source archive directory</param>
        /// <param name="destinationDirectoryName">Destination directory name</param>
        public static void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName)
        {
            ExtractToDirectory(sourceArchiveFileName, destinationDirectoryName, Encoding.UTF8);
        }

        /// <summary>
        /// Extract IMG archive to directory
        /// </summary>
        /// <param name="sourceArchiveFileName">Source archive file name</param>
        /// <param name="destinationDirectoryName">Destination directory name</param>
        /// <param name="entryNameEncoding">Entry name encoding</param>
        public static void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName, Encoding entryNameEncoding)
        {
            if ((sourceArchiveFileName != null) && (destinationDirectoryName != null) && (entryNameEncoding != null))
            {
                IMGArchive archive = Open(sourceArchiveFileName, EIMGArchiveMode.Read, entryNameEncoding);
                if (archive != null)
                {
                    if (!(Directory.Exists(destinationDirectoryName)))
                    {
                        Directory.CreateDirectory(destinationDirectoryName);
                    }
                    foreach (IMGArchiveEntry entry in archive.Entries)
                    {
                        string file_path = Path.Combine(destinationDirectoryName, entry.FullName);
                        string directory_path = Path.GetDirectoryName(file_path);
                        if (!(Directory.Exists(directory_path)))
                        {
                            Directory.CreateDirectory(directory_path);
                        }
                        using (FileStream file_stream = File.Open(file_path, FileMode.Create))
                        {
                            using (Stream entry_stream = entry.Open())
                            {
                                byte[] buffer = new byte[2048];
                                int read_bytes = 0;
                                while ((read_bytes = entry_stream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    file_stream.Write(buffer, 0, read_bytes);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Open IMG archive
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
                            byte[] header_bytes = new byte[] { 0x56, 0x45, 0x52, 0x32, 0x0, 0x0, 0x0, 0x0 };
                            ret.Stream.Write(header_bytes, 0, header_bytes.Length);
                        }
                        else
                        {
                            byte[] version_bytes = new byte[4];
                            if (ret.Stream.Read(version_bytes, 0, version_bytes.Length) == version_bytes.Length)
                            {
                                string version = Encoding.UTF8.GetString(version_bytes);
                                byte[] int_bytes = new byte[4];
                                if (ret.Stream.Read(int_bytes, 0, int_bytes.Length) == int_bytes.Length)
                                {
                                    uint num_entries = int_bytes[0] | (((uint)(int_bytes[1])) << 8) | (((uint)(int_bytes[1])) << 16) | (((uint)(int_bytes[1])) << 24);
                                    if ((version == "VER2") && (ret.Stream.Length >= (num_entries * 8)))
                                    {
                                        for (uint num_entry = 0U; num_entry != num_entries; num_entry++)
                                        {
                                            byte[] long_bytes = new byte[8];
                                            if (ret.Stream.Read(long_bytes, 0, long_bytes.Length) == long_bytes.Length)
                                            {
                                                long offset = (long_bytes[0] | (((long)(long_bytes[1])) << 8) | (((long)(long_bytes[1])) << 16) | (((long)(long_bytes[1])) << 24) | (((long)(long_bytes[1])) << 32) | (((long)(long_bytes[1])) << 40) | (((long)(long_bytes[1])) << 48) | (((long)(long_bytes[1])) << 56)) * 2048L;
                                                byte[] short_bytes = new byte[2];
                                                if (ret.Stream.Read(short_bytes, 0, short_bytes.Length) == short_bytes.Length)
                                                {
                                                    int length = (short_bytes[0] | (short_bytes[1] << 8)) * 2048;
                                                    if (ret.Stream.Read(short_bytes, 0, short_bytes.Length) == short_bytes.Length)
                                                    {
                                                        byte[] full_name_bytes_raw = new byte[24];
                                                        if (ret.Stream.Read(full_name_bytes_raw, 0, full_name_bytes_raw.Length) == full_name_bytes_raw.Length)
                                                        {
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
                                                        else
                                                        {
                                                            throw new InvalidDataException("IMG entry name can't be empty.");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        throw new InvalidDataException("IMG entry name can't be empty.");
                                                    }
                                                }
                                                else
                                                {
                                                    throw new InvalidDataException("IMG entry name can't be empty.");
                                                }
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
                                else
                                {
                                    throw new InvalidDataException("\"" + archiveFileName + "\" is not an IMG file");
                                }
                            }
                            else
                            {
                                throw new InvalidDataException("\"" + archiveFileName + "\" is not an IMG file");
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
