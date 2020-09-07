using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// IMG sharp namespace
/// </summary>
namespace IMGSharp
{
    /// <summary>
    /// IMG archive class
    /// </summary>
    internal class IMGArchive : IIMGArchive
    {
        /// <summary>
        /// Entries
        /// </summary>
        internal Dictionary<string, IIMGArchiveEntry> InternalEntries { get; }

        /// <summary>
        /// IMG archive stream
        /// </summary>
        public Stream Stream { get; }

        /// <summary>
        /// IMG archive entries
        /// </summary>
        public IReadOnlyDictionary<string, IIMGArchiveEntry> Entries => InternalEntries;

        /// <summary>
        /// IMG archive access mode
        /// </summary>
        public EIMGArchiveAccessMode AccessMode { get; }

        /// <summary>
        /// IMG archive entry name encoding
        /// </summary>
        public Encoding EntryNameEncoding { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stream">IMG stream</param>
        /// <param name="mode">IMG archive mode</param>
        /// <param name="entryNameEncoding">Entry name encoding</param>
        internal IMGArchive(Stream stream, EIMGArchiveAccessMode mode, Encoding entryNameEncoding, Dictionary<string, IIMGArchiveEntry> entries)
        {
            Stream = stream;
            AccessMode = mode;
            EntryNameEncoding = entryNameEncoding;
            InternalEntries = entries;
        }

        /// <summary>
        /// Write IMG archive entry
        /// </summary>
        /// <param name="tempArchive">Temporary IMG archive</param>
        /// <param name="newEntry">New IMG archive entry</param>
        /// <param name="stream">Stream</param>
        private void WriteEntry(IMGArchive tempArchive, IIMGArchiveEntry newEntry, Stream stream)
        {
            using (IIMGArchiveEntryStream temporary_img_archive_entry_stream = tempArchive.InternalEntries[newEntry.FullName.ToLower()].Open())
            {
                byte[] temp_data = new byte[temporary_img_archive_entry_stream.Stream.Length];
                temporary_img_archive_entry_stream.Stream.Read(temp_data, 0, temp_data.Length);
                stream.Write(temp_data, 0, temp_data.Length);
            }
        }

        /// <summary>
        /// Commit IMG archive entry
        /// </summary>
        /// <param name="entry">IMG archive entry</param>
        /// <param name="entryStream">IMG archive entry stream</param>
        internal void CommitEntry(IIMGArchiveEntry entry, IIMGArchiveEntryStream entryStream)
        {
            try
            {
                if (AccessMode != EIMGArchiveAccessMode.Read)
                {
                    string temporary_img_archive_path = Path.Combine(Path.GetTempPath(), $"{ Guid.NewGuid() }.img");
                    if (File.Exists(temporary_img_archive_path))
                    {
                        File.Delete(temporary_img_archive_path);
                    }
                    using (FileStream temporary_img_archive_file_stream = File.Open(temporary_img_archive_path, FileMode.Create))
                    {
                        Stream.Seek(0L, SeekOrigin.Begin);
                        Stream.CopyTo(temporary_img_archive_file_stream);
                    }
                    using (IMGArchive temporary_img_archive = IMGFile.OpenRead(temporary_img_archive_path) as IMGArchive)
                    {
                        if (temporary_img_archive != null)
                        {
                            InternalEntries.Clear();
                            Stream.SetLength(0L);
                            using (BinaryWriter stream_binary_writer = new BinaryWriter(Stream, EntryNameEncoding, true))
                            {
                                int entry_count = temporary_img_archive.InternalEntries.Values.Count + ((entry == null) ? 0 : (entry.IsNewEntry ? 1 : 0));
                                int first_entry_offset = ((((entry_count * 32) % 2048) == 0) ? ((entry_count * 32) / 2048) : (((entry_count * 32) / 2048) + 1));
                                int current_entry_offset = first_entry_offset;
                                List<IMGArchiveEntry> new_entries = new List<IMGArchiveEntry>();
                                Dictionary<string, IIMGArchiveEntry> temporary_entries = new Dictionary<string, IIMGArchiveEntry>(temporary_img_archive.InternalEntries);
                                long missing_byte_count;
                                stream_binary_writer.Write(IMGFile.SupportedIMGArchiveVersion);
                                stream_binary_writer.Write((short)entry_count);
                                if ((entry != null) && entry.IsNewEntry)
                                {
                                    temporary_entries.Add(entry.FullName.ToLower(), new IMGArchiveEntry(temporary_img_archive, 0L, (int)(entryStream.Stream.Length), entry.FullName));
                                }
                                foreach (KeyValuePair<string, IIMGArchiveEntry> temporary_entry in temporary_entries)
                                {
                                    int entry_length = (int)((entry == null) ? (((temporary_entry.Value.Length % 2048L) == 0L) ? (temporary_entry.Value.Length / 2048L) : ((temporary_entry.Value.Length / 2048L) + 1L)) : ((entry.FullName.ToLower() == temporary_entry.Key) ? (((entryStream.Stream.Length % 2048L) == 0L) ? (entryStream.Stream.Length / 2048L) : ((entryStream.Stream.Length / 2048L) + 1)) : (((temporary_entry.Value.Length % 2048L) == 0L) ? (temporary_entry.Value.Length / 2048L) : ((temporary_entry.Value.Length / 2048L) + 1L))));
                                    byte[] name_bytes_raw = EntryNameEncoding.GetBytes(temporary_entry.Value.FullName);
                                    byte[] name_bytes = new byte[24];
                                    Array.Copy(name_bytes_raw, name_bytes, Math.Min(name_bytes_raw.Length, name_bytes.Length));
                                    stream_binary_writer.Write(current_entry_offset);
                                    stream_binary_writer.Write((short)entry_length);
                                    stream_binary_writer.Write(name_bytes);
                                    IMGArchiveEntry new_entry = new IMGArchiveEntry(this, current_entry_offset * 2048, entry_length * 2048, temporary_entry.Value.FullName);
                                    new_entries.Add(new_entry);
                                    InternalEntries.Add(new_entry.FullName.ToLower(), new_entry);
                                    current_entry_offset += entry_length;
                                }
                                foreach (IMGArchiveEntry new_entry in new_entries)
                                {
                                    stream_binary_writer.Flush();
                                    missing_byte_count = new_entry.Offset - Stream.Length;
                                    if (missing_byte_count > 0L)
                                    {
                                        stream_binary_writer.Write(new byte[missing_byte_count]);
                                    }
                                    if (entry != null)
                                    {
                                        if (entry.FullName == new_entry.FullName)
                                        {
                                            byte[] data = new byte[entryStream.Stream.Length];
                                            entryStream.Stream.Seek(0L, SeekOrigin.Begin);
                                            entryStream.Stream.Read(data, 0, data.Length);
                                            Stream.Write(data, 0, data.Length);
                                        }
                                        else
                                        {
                                            WriteEntry(temporary_img_archive, new_entry, Stream);
                                        }
                                    }
                                    else
                                    {
                                        WriteEntry(temporary_img_archive, new_entry, Stream);
                                    }
                                }
                                stream_binary_writer.Flush();
                                missing_byte_count = current_entry_offset - (Stream.Length / 2048);
                                if (missing_byte_count > 0)
                                {
                                    stream_binary_writer.Write(new byte[missing_byte_count]);
                                }
                                stream_binary_writer.Flush();
                            }
                        }
                    }
                    if (File.Exists(temporary_img_archive_path))
                    {
                        File.Delete(temporary_img_archive_path);
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }

        /// <summary>
        /// Create IMG archive entry
        /// </summary>
        /// <param name="entryName">Entry name</param>
        /// <returns>IMG archive entry if successful, otherwise "null"</returns>
        public IIMGArchiveEntry CreateEntry(string entryName)
        {
            if (entryName == null)
            {
                throw new ArgumentNullException(nameof(entryName));
            }
            IIMGArchiveEntry ret = null;
            string entry_name = entryName.Trim();
            bool proceed = true;
            Parallel.ForEach(Path.GetInvalidPathChars(), (invalid_path_character, parallelLoopState) =>
            {
                if (entry_name.Contains(invalid_path_character.ToString()))
                {
                    proceed = false;
                    parallelLoopState.Break();
                }
            });
            if (proceed)
            {
                string key = entry_name.ToLower();
                if (!InternalEntries.ContainsKey(key))
                {
                    InternalEntries.Add(key, new IMGArchiveEntry(this, Stream.Length, 0, entry_name, true));
                }
            }
            return ret;
        }

        /// <summary>
        /// Get IMG archive entry
        /// </summary>
        /// <param name="entryName">Entry name</param>
        /// <returns>IMG archive entry if successful, otherwise "null"</returns>
        public IIMGArchiveEntry GetEntry(string entryName)
        {
            if (entryName == null)
            {
                throw new ArgumentNullException(nameof(entryName));
            }
            string key = entryName.ToLower();
            return InternalEntries.ContainsKey(key) ? InternalEntries[key] : null;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose() => Stream.Dispose();
    }
}
