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
    /// IMG archive class
    /// </summary>
    public class IMGArchive : IDisposable
    {
        /// <summary>
        /// Stream
        /// </summary>
        private Stream stream;

        /// <summary>
        /// Archive mode
        /// </summary>
        private EIMGArchiveMode mode;

        /// <summary>
        /// Entry name encoding
        /// </summary>
        private Encoding entryNameEncoding;

        /// <summary>
        /// Entries
        /// </summary>
        internal Dictionary<string, IMGArchiveEntry> entries = new Dictionary<string, IMGArchiveEntry>();

        /// <summary>
        /// Stream
        /// </summary>
        internal Stream Stream
        {
            get
            {
                return stream;
            }
        }

        /// <summary>
        /// Entries
        /// </summary>
        public IMGArchiveEntry[] Entries
        {
            get
            {
                return (new List<IMGArchiveEntry>(entries.Values)).ToArray();
            }
        }

        /// <summary>
        /// Archive mode
        /// </summary>
        public EIMGArchiveMode Mode
        {
            get
            {
                return mode;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stream">IMG stream</param>
        /// <param name="entryNameEncoding">Entry name encoding</param>
        internal IMGArchive(Stream stream, Encoding entryNameEncoding)
        {
            this.stream = stream;
            this.entryNameEncoding = entryNameEncoding;
        }

        /// <summary>
        /// Create IMG archive entry
        /// </summary>
        /// <param name="entryName">Entry name</param>
        /// <returns>IMG archive entry if successful, otherwise "null"</returns>
        public IMGArchiveEntry CreateEntry(string entryName)
        {
            IMGArchiveEntry ret = null;
            string entry_name = entryName.Trim();
            bool proceed = true;
            foreach (char invalid_path_char in Path.GetInvalidPathChars())
            {
                if (entry_name.Contains(new string(new char[] { invalid_path_char })))
                {
                    proceed = false;
                    break;
                }
            }
            if (proceed)
            {
                string key = entry_name.ToLower();
                if (!(entries.ContainsKey(key)))
                {
                    entries.Add(key, new IMGArchiveEntry(this, stream.Length, 0, entry_name, true));
                }
            }
            return ret;
        }

        /// <summary>
        /// Get IMG archive entry
        /// </summary>
        /// <param name="entryName">Entry name</param>
        /// <returns>IMG archive entry if successful, otherwise "null"</returns>
        public IMGArchiveEntry GetEntry(string entryName)
        {
            IMGArchiveEntry ret = null;
            if (entryName != null)
            {
                string key = entryName.ToLower();
                if (entries.ContainsKey(key))
                {
                    ret = entries[key];
                }
            }
            return ret;
        }

        /// <summary>
        /// Write IMG archive entry
        /// </summary>
        /// <param name="tempArchive">Temporary IMG archive</param>
        /// <param name="newEntry">New IMG archive entry</param>
        /// <param name="writer">Binary writer</param>
        private void WriteEntry(IMGArchive tempArchive, IMGArchiveEntry newEntry, BinaryWriter writer)
        {
            using (Stream temp_stream = tempArchive.entries[newEntry.FullName.ToLower()].Open())
            {
                byte[] temp_data = new byte[temp_stream.Length];
                temp_stream.Read(temp_data, 0, temp_data.Length);
                writer.Write(temp_data);
            }
        }

        /// <summary>
        /// Commit IMG archive entry
        /// </summary>
        /// <param name="entry">IMG archive entry</param>
        /// <param name="stream">IMG archive entry stream</param>
        internal void CommitEntry(IMGArchiveEntry entry, IMGArchiveEntryStream stream)
        {
            try
            {
                if (mode != EIMGArchiveMode.Read)
                {
                    string temp_path = Path.GetTempPath() + Guid.NewGuid().ToString() + ".img";
                    if (File.Exists(temp_path))
                    {
                        File.Delete(temp_path);
                    }
                    using (FileStream temp_stream = File.Open(temp_path, FileMode.Create))
                    {
                        this.stream.Seek(0L, SeekOrigin.Begin);
                        this.stream.CopyTo(temp_stream);
                    }
                    using (IMGArchive temp_archive = IMGFile.OpenRead(temp_path))
                    {
                        entries.Clear();
                        this.stream.SetLength(0L);
                        using (BinaryWriter writer = new BinaryWriter(this.stream, Encoding.UTF8, true))
                        {
                            int entry_count = entries.Values.Count + ((entry == null) ? 0 : (entry.IsNewEntry ? 1 : 0));
                            int first_entry_offset = ((2048 % (entry_count * 32) == 0) ? (2048 / (entry_count * 32)) : ((2048 / (entry_count * 32)) + 1));
                            int current_entry_offset = first_entry_offset;
                            List<IMGArchiveEntry> new_entries = new List<IMGArchiveEntry>();
                            writer.Write(new byte[] { 0x56, 0x45, 0x52, 0x32, (byte)(entry_count & 0xFF), (byte)((entry_count >> 8) & 0xFF), 0x0, 0x0 });
                            Dictionary<string, IMGArchiveEntry> temp_entries = new Dictionary<string, IMGArchiveEntry>(temp_archive.entries);
                            if (entry != null)
                            {
                                if (entry.IsNewEntry)
                                {
                                    temp_entries.Add(entry.FullName.ToLower(), new IMGArchiveEntry(temp_archive, 0L, (int)(stream.Length), entry.FullName));
                                }
                            }
                            foreach (KeyValuePair<string, IMGArchiveEntry> temp_entry in temp_entries)
                            {
                                int entry_length = (int)((entry == null) ? (((temp_entry.Value.Length % 2048L) == 0L) ? (temp_entry.Value.Length / 2048L) : ((temp_entry.Value.Length / 2048L) + 1L)) : ((entry.FullName.ToLower() == temp_entry.Key) ? (((stream.Length % 2048L) == 0L) ? (stream.Length / 2048L) : ((stream.Length / 2048L) + 1)) : (((temp_entry.Value.Length % 2048L) == 0L) ? (temp_entry.Value.Length / 2048L) : ((temp_entry.Value.Length / 2048L) + 1L))));
                                byte[] name_bytes_raw = entryNameEncoding.GetBytes(temp_entry.Value.FullName);
                                byte[] name_bytes = new byte[24];
                                Array.Copy(name_bytes_raw, name_bytes, Math.Min(name_bytes_raw.Length, name_bytes.Length));
                                writer.Write(current_entry_offset);
                                writer.Write(new byte[] { (byte)(entry_length & 0xFF), (byte)((entry_length >> 8) & 0xFF), 0x0, 0x0 });
                                writer.Write(name_bytes);
                                IMGArchiveEntry new_entry = new IMGArchiveEntry(this, current_entry_offset * 2048, entry_length * 2048, temp_entry.Value.FullName);
                                new_entries.Add(new_entry);
                                entries.Add(new_entry.FullName.ToLower(), new_entry);
                                current_entry_offset += entry_length;
                            }
                            foreach (IMGArchiveEntry new_entry in new_entries)
                            {
                                while (this.stream.Length < new_entry.Offset)
                                {
                                    this.stream.WriteByte(0);
                                }
                                if (entry != null)
                                {
                                    if (entry.FullName == new_entry.FullName)
                                    {
                                        byte[] data = new byte[stream.Length];
                                        stream.Seek(0L, SeekOrigin.Begin);
                                        stream.Read(data, 0, data.Length);
                                        writer.Write(data);
                                    }
                                    else
                                    {
                                        WriteEntry(temp_archive, new_entry, writer);
                                    }
                                }
                                else
                                {
                                    WriteEntry(temp_archive, new_entry, writer);
                                }
                            }
                        }
                    }
                    if (File.Exists(temp_path))
                    {
                        File.Delete(temp_path);
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            stream.Dispose();
        }
    }
}
