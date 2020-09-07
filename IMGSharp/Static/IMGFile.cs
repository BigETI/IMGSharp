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
        /// Supported IMG archive version "VER2"
        /// </summary>
        public static int SupportedIMGArchiveVersion { get; } = 0x32524556;

        /// <summary>
        /// Create IMG archive from directory
        /// </summary>
        /// <param name="sourceDirectoryPath">Source directory path</param>
        /// <param name="destinationIMGArchiveFilePath">Destination IMG archive file path</param>
        public static void CreateFromDirectory(string sourceDirectoryPath, string destinationIMGArchiveFilePath) => CreateFromDirectory(sourceDirectoryPath, destinationIMGArchiveFilePath, false, Encoding.UTF8);

        /// <summary>
        /// Create IMG archive from directory
        /// </summary>
        /// <param name="sourceDirectoryPath">Source directory path</param>
        /// <param name="destinationIMGArchiveFilePath">Destination IMG archive file path</param>
        /// <param name="includeBaseDirectory">Include base directory in IMG archive</param>
        public static void CreateFromDirectory(string sourceDirectoryPath, string destinationIMGArchiveFilePath, bool includeBaseDirectory) => CreateFromDirectory(sourceDirectoryPath, destinationIMGArchiveFilePath, includeBaseDirectory, Encoding.UTF8);

        /// <summary>
        /// Create IMG archive from directory
        /// </summary>
        /// <param name="sourceDirectoryPath">Source directory path</param>
        /// <param name="destinationIMGArchiveFilePath">Destination IMG archive file path</param>
        /// <param name="includeBaseDirectory">Include base directory in IMG archive</param>
        /// <param name="entryNameEncoding">Entry name encoding</param>
        public static void CreateFromDirectory(string sourceDirectoryPath, string destinationIMGArchiveFilePath, bool includeBaseDirectory, Encoding entryNameEncoding)
        {
            if (sourceDirectoryPath == null)
            {
                throw new ArgumentNullException(nameof(sourceDirectoryPath));
            }
            if (destinationIMGArchiveFilePath == null)
            {
                throw new ArgumentNullException(nameof(destinationIMGArchiveFilePath));
            }
            if (entryNameEncoding == null)
            {
                throw new ArgumentNullException(nameof(entryNameEncoding));
            }
            try
            {
                string source_directory_name = Path.GetFullPath(sourceDirectoryPath.TrimEnd('\\', '/'));
                string destination_archive_file_name = Path.GetFullPath(destinationIMGArchiveFilePath);
                if (Directory.Exists(source_directory_name))
                {
                    string[] files = Directory.GetFiles(source_directory_name, "*", SearchOption.AllDirectories);
                    string[] relative_files = new string[files.Length];
                    bool file_name_lenghts_ok = true;
                    for (int i = 0; i < files.Length; i++)
                    {
                        relative_files[i] = IMGUtilities.GetRelativePath(files[i], includeBaseDirectory ? Directory.GetParent(source_directory_name).FullName : source_directory_name).Replace('\\', '/');
                        if (relative_files[i].Length > 24)
                        {
                            file_name_lenghts_ok = false;
                            break;
                        }
                    }
                    if (file_name_lenghts_ok)
                    {
                        using (FileStream img_archive_file_stream = File.Open(destination_archive_file_name, FileMode.Create))
                        {
                            using (BinaryWriter img_archive_file_stream_binary_writer = new BinaryWriter(img_archive_file_stream, entryNameEncoding, true))
                            {
                                int first_entry_offset = ((((files.Length * 32) % 2048) == 0) ? ((files.Length * 32) / 2048) : (((files.Length * 32) / 2048) + 1));
                                int current_entry_offset = first_entry_offset;
                                List<KeyValuePair<string, int>> entries = new List<KeyValuePair<string, int>>();
                                img_archive_file_stream_binary_writer.Write(new byte[] { 0x56, 0x45, 0x52, 0x32, (byte)(files.Length & 0xFF), (byte)((files.Length >> 8) & 0xFF), 0x0, 0x0 });
                                for (int i = 0; i < files.Length; i++)
                                {
                                    long file_length = (new FileInfo(files[i])).Length;
                                    int entry_length = (int)(((file_length % 2048L) == 0L) ? (file_length / 2048L) : ((file_length / 2048L) + 1L));
                                    byte[] name_bytes_raw = entryNameEncoding.GetBytes(relative_files[i]);
                                    byte[] name_bytes = new byte[24];
                                    Array.Copy(name_bytes_raw, name_bytes, Math.Min(name_bytes_raw.Length, name_bytes.Length));
                                    img_archive_file_stream_binary_writer.Write(current_entry_offset);
                                    img_archive_file_stream_binary_writer.Write(new byte[] { (byte)(entry_length & 0xFF), (byte)((entry_length >> 8) & 0xFF), 0x0, 0x0 });
                                    img_archive_file_stream_binary_writer.Write(name_bytes);
                                    entries.Add(new KeyValuePair<string, int>(files[i], current_entry_offset));
                                    current_entry_offset += entry_length;
                                }
                                foreach (KeyValuePair<string, int> entry in entries)
                                {
                                    using (FileStream stream = File.Open(entry.Key, FileMode.Open, FileAccess.Read))
                                    {
                                        while ((img_archive_file_stream.Length / 2048) < entry.Value)
                                        {
                                            img_archive_file_stream.WriteByte(0);
                                        }
                                        byte[] data = new byte[stream.Length];
                                        stream.Read(data, 0, (int)(stream.Length));
                                        img_archive_file_stream_binary_writer.Write(data);
                                    }
                                }
                                while ((img_archive_file_stream.Length / 2048) < current_entry_offset)
                                {
                                    img_archive_file_stream.WriteByte(0);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }

        /// <summary>
        /// Extract IMG archive to directory
        /// </summary>
        /// <param name="sourceIMGArchiveFilePath">Source IMG archive file path</param>
        /// <param name="destinationDirectoryPath">Destination directory path</param>
        public static void ExtractToDirectory(string sourceIMGArchiveFilePath, string destinationDirectoryPath) => ExtractToDirectory(sourceIMGArchiveFilePath, destinationDirectoryPath, Encoding.UTF8);

        /// <summary>
        /// Extract IMG archive to directory
        /// </summary>
        /// <param name="sourceIMGArchiveFilePath">Source IMG archive file path</param>
        /// <param name="destinationDirectoryPath">Destination directory path</param>
        /// <param name="entryNameEncoding">Entry name encoding</param>
        public static void ExtractToDirectory(string sourceIMGArchiveFilePath, string destinationDirectoryPath, Encoding entryNameEncoding)
        {
            if (sourceIMGArchiveFilePath == null)
            {
                throw new ArgumentNullException(nameof(sourceIMGArchiveFilePath));
            }
            if (destinationDirectoryPath == null)
            {
                throw new ArgumentNullException(nameof(destinationDirectoryPath));
            }
            if (entryNameEncoding == null)
            {
                throw new ArgumentNullException(nameof(entryNameEncoding));
            }
            IMGArchive img_archive = Open(sourceIMGArchiveFilePath, EIMGArchiveAccessMode.Read, entryNameEncoding) as IMGArchive;
            if (img_archive != null)
            {
                if (!(Directory.Exists(destinationDirectoryPath)))
                {
                    Directory.CreateDirectory(destinationDirectoryPath);
                }
                foreach (IMGArchiveEntry img_archive_entry in img_archive.InternalEntries.Values)
                {
                    string file_path = Path.Combine(destinationDirectoryPath, img_archive_entry.FullName);
                    string directory_path = Path.GetDirectoryName(file_path);
                    if (!(Directory.Exists(directory_path)))
                    {
                        Directory.CreateDirectory(directory_path);
                    }
                    using (FileStream img_archive_entry_file_stream = File.Open(file_path, FileMode.Create))
                    {
                        using (IIMGArchiveEntryStream img_archive_entry_stream = img_archive_entry.Open())
                        {
                            img_archive_entry_file_stream.Position = 0L;
                            img_archive_entry_stream.Stream.Position = 0L;
                            img_archive_entry_file_stream.CopyTo(img_archive_entry_stream.Stream);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Open IMG archive
        /// </summary>
        /// <param name="imgArchiveFilePath">IMG archive file path</param>
        /// <param name="accessMode">IMG archive access mode</param>
        /// <returns>IMG archive</returns>
        /// <exception cref="InvalidOperationException">Return value is null</exception>
        /// <exception cref="ArgumentNullException">Archive file name is null</exception>
        /// <exception cref="FileNotFoundException">Archive file not found</exception>
        /// <exception cref="InvalidDataException">Archive error</exception>
        public static IIMGArchive Open(string imgArchiveFilePath, EIMGArchiveAccessMode accessMode) => Open(imgArchiveFilePath, accessMode, Encoding.UTF8);

        /// <summary>
        /// Open IMG archive
        /// </summary>
        /// <param name="imgArchiveFilePath">IMG archive file path</param>
        /// <param name="accessMode">IMG archive access mode</param>
        /// <param name="entryNameEncoding">IMG archive entry name encoding</param>
        /// <returns>IMG archive</returns>
        /// <exception cref="InvalidOperationException">Return value is null</exception>
        /// <exception cref="ArgumentNullException">Archive file name is null</exception>
        /// <exception cref="FileNotFoundException">Archive file not found</exception>
        /// <exception cref="InvalidDataException">Archive error</exception>
        public static IIMGArchive Open(string imgArchiveFilePath, EIMGArchiveAccessMode accessMode, Encoding entryNameEncoding)
        {
            if (imgArchiveFilePath == null)
            {
                throw new ArgumentNullException(nameof(imgArchiveFilePath));
            }
            if (entryNameEncoding == null)
            {
                throw new ArgumentNullException(nameof(entryNameEncoding));
            }
            IMGArchive ret = null;
            try
            {
                if ((accessMode != EIMGArchiveAccessMode.Create) && (!File.Exists(imgArchiveFilePath)))
                {
                    throw new FileNotFoundException("Archive not found", imgArchiveFilePath);
                }
                FileStream archive_file_stream = File.Open(imgArchiveFilePath, (accessMode == EIMGArchiveAccessMode.Create) ? FileMode.Create : FileMode.Open, (accessMode != EIMGArchiveAccessMode.Read) ? FileAccess.ReadWrite : FileAccess.Read);
                if (archive_file_stream == null)
                {
                    throw new FileNotFoundException("Archive not found", imgArchiveFilePath);
                }
                Dictionary<string, IIMGArchiveEntry> img_archive_entries = new Dictionary<string, IIMGArchiveEntry>();
                ret = new IMGArchive(archive_file_stream, accessMode, entryNameEncoding, img_archive_entries);
                if (accessMode == EIMGArchiveAccessMode.Create)
                {
                    byte[] new_data = new byte[2048];
                    new_data[0] = 0x56;
                    new_data[1] = 0x45;
                    new_data[2] = 0x52;
                    new_data[3] = 0x32;
                    archive_file_stream.Write(new_data, 0, new_data.Length);
                }
                else
                {
                    using (BinaryReader archive_file_stream_binary_reader = new BinaryReader(archive_file_stream, entryNameEncoding, true))
                    {
                        int archive_version = archive_file_stream_binary_reader.ReadInt32();
                        if (archive_version != SupportedIMGArchiveVersion)
                        {
                            throw new InvalidDataException($"Invalid IMG archive version { archive_version }.");
                        }
                        uint num_entries = archive_file_stream_binary_reader.ReadUInt32();
                        if (archive_file_stream.Length < (num_entries * 8))
                        {
                            throw new InvalidDataException($"IMG archive file size is smaller than expected.");
                        }
                        for (uint num_entry = 0U; num_entry != num_entries; num_entry++)
                        {
                            long offset = archive_file_stream_binary_reader.ReadInt32() * 2048L;
                            int length = archive_file_stream_binary_reader.ReadInt16() * 2048;
                            short dummy = archive_file_stream_binary_reader.ReadInt16();
                            byte[] full_name_bytes_raw = new byte[24];
                            if (archive_file_stream_binary_reader.Read(full_name_bytes_raw, 0, full_name_bytes_raw.Length) != full_name_bytes_raw.Length)
                            {
                                throw new InvalidDataException("IMG entry name is missing.");
                            }
                            int full_name_bytes_count = IMGUtilities.GetNullTerminatedByteStringLength(full_name_bytes_raw);
                            if (full_name_bytes_count <= 0)
                            {
                                throw new InvalidDataException("IMG entry name can't be empty.");
                            }
                            string full_name = entryNameEncoding.GetString(full_name_bytes_raw, 0, full_name_bytes_count);
                            img_archive_entries.Add(full_name.ToLower(), new IMGArchiveEntry(ret, offset, length, full_name));
                        }
                    }
                }
                if (ret == null)
                {
                    throw new InvalidOperationException("Return value is null");
                }
            }
            catch (Exception e)
            {
                ret?.Dispose();
                throw e;
            }
            return ret;
        }

        /// <summary>
        /// Open IMG archive in read only mode
        /// </summary>
        /// <param name="imgArchiveFilePath">IMG archive file path</param>
        /// <returns>IMG archive</returns>
        /// <exception cref="InvalidOperationException">Return value is null</exception>
        /// <exception cref="ArgumentNullException">Archive file name is null</exception>
        /// <exception cref="FileNotFoundException">Archive file not found</exception>
        /// <exception cref="InvalidDataException">Archive error</exception>
        public static IIMGArchive OpenRead(string imgArchiveFilePath) => Open(imgArchiveFilePath, EIMGArchiveAccessMode.Read);

        /// <summary>
        /// Open IMG archive in read only mode
        /// </summary>
        /// <param name="imgArchiveFilePath">IMG archive file path</param>
        /// <param name="entryNameEncoding">Entry name encoding</param>
        /// <returns>IMG archive</returns>
        /// <exception cref="InvalidOperationException">Return value is null</exception>
        /// <exception cref="ArgumentNullException">Archive file name is null</exception>
        /// <exception cref="FileNotFoundException">Archive file not found</exception>
        /// <exception cref="InvalidDataException">Archive error</exception>
        public static IIMGArchive OpenRead(string imgArchiveFilePath, Encoding entryNameEncoding) => Open(imgArchiveFilePath, EIMGArchiveAccessMode.Read, entryNameEncoding);
    }
}
