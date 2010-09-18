/********************************************************************************************
 * InterIMAP
 * Copyright (C) 2008-2009 Jason Miesionczek
 * Original Author: Rohit Joshi
 * Based on this article on codeproject.com:
 * IMAP Client library using C#
 * http://www.codeproject.com/KB/IP/imaplibrary.aspx?msg=2498332
 * Posted: August 16th 2004
 * 
 * ZipStorer code written by Jaime Olivares
 * http://www.codeproject.com/KB/recipes/ZipStorer.aspx
 * 
 * InterIMAP is free software; you can redistribute it and/or modify it under the terms
 * of the GNU Lesser General Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 * 
 * InterIMAP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License along with
 * InterIMAP. If not, see http://www.gnu.org/licenses/.
 * 
 * *****************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace InterIMAP.Common.Data
{
    /// <summary>
    /// Static class for managing the physical mailbox file
    /// </summary>
    public class ZipStorer
    {
        #region Data Structures
        /// <summary>
        /// Represents a single file entry
        /// </summary>
        public struct ZipFileEntry
        {
            public string FilenameInZip;
            public uint FileSize;
            public uint FileOffset;
            public uint HeaderSize;
            public uint HeaderOffset;
            public uint Crc32;
            public DateTime ModifyTime;
            public string Comment;
        }
        #endregion

        #region CRC32 table
        static readonly UInt32[] CrcTable = new UInt32[] {
                                                    0x00000000, 0x77073096, 0xee0e612c, 0x990951ba, 0x076dc419, 0x706af48f, 0xe963a535, 0x9e6495a3, 0x0edb8832, 0x79dcb8a4,
                                                    0xe0d5e91e, 0x97d2d988, 0x09b64c2b, 0x7eb17cbd, 0xe7b82d07, 0x90bf1d91, 0x1db71064, 0x6ab020f2, 0xf3b97148, 0x84be41de,
                                                    0x1adad47d, 0x6ddde4eb, 0xf4d4b551, 0x83d385c7, 0x136c9856, 0x646ba8c0, 0xfd62f97a, 0x8a65c9ec, 0x14015c4f, 0x63066cd9,
                                                    0xfa0f3d63, 0x8d080df5, 0x3b6e20c8, 0x4c69105e, 0xd56041e4, 0xa2677172, 0x3c03e4d1, 0x4b04d447, 0xd20d85fd, 0xa50ab56b,
                                                    0x35b5a8fa, 0x42b2986c, 0xdbbbc9d6, 0xacbcf940, 0x32d86ce3, 0x45df5c75, 0xdcd60dcf, 0xabd13d59, 0x26d930ac, 0x51de003a,
                                                    0xc8d75180, 0xbfd06116, 0x21b4f4b5, 0x56b3c423, 0xcfba9599, 0xb8bda50f, 0x2802b89e, 0x5f058808, 0xc60cd9b2, 0xb10be924,
                                                    0x2f6f7c87, 0x58684c11, 0xc1611dab, 0xb6662d3d, 0x76dc4190, 0x01db7106, 0x98d220bc, 0xefd5102a, 0x71b18589, 0x06b6b51f,
                                                    0x9fbfe4a5, 0xe8b8d433, 0x7807c9a2, 0x0f00f934, 0x9609a88e, 0xe10e9818, 0x7f6a0dbb, 0x086d3d2d, 0x91646c97, 0xe6635c01,
                                                    0x6b6b51f4, 0x1c6c6162, 0x856530d8, 0xf262004e, 0x6c0695ed, 0x1b01a57b, 0x8208f4c1, 0xf50fc457, 0x65b0d9c6, 0x12b7e950,
                                                    0x8bbeb8ea, 0xfcb9887c, 0x62dd1ddf, 0x15da2d49, 0x8cd37cf3, 0xfbd44c65, 0x4db26158, 0x3ab551ce, 0xa3bc0074, 0xd4bb30e2,
                                                    0x4adfa541, 0x3dd895d7, 0xa4d1c46d, 0xd3d6f4fb, 0x4369e96a, 0x346ed9fc, 0xad678846, 0xda60b8d0, 0x44042d73, 0x33031de5,
                                                    0xaa0a4c5f, 0xdd0d7cc9, 0x5005713c, 0x270241aa, 0xbe0b1010, 0xc90c2086, 0x5768b525, 0x206f85b3, 0xb966d409, 0xce61e49f,
                                                    0x5edef90e, 0x29d9c998, 0xb0d09822, 0xc7d7a8b4, 0x59b33d17, 0x2eb40d81, 0xb7bd5c3b, 0xc0ba6cad, 0xedb88320, 0x9abfb3b6,
                                                    0x03b6e20c, 0x74b1d29a, 0xead54739, 0x9dd277af, 0x04db2615, 0x73dc1683, 0xe3630b12, 0x94643b84, 0x0d6d6a3e, 0x7a6a5aa8,
                                                    0xe40ecf0b, 0x9309ff9d, 0x0a00ae27, 0x7d079eb1, 0xf00f9344, 0x8708a3d2, 0x1e01f268, 0x6906c2fe, 0xf762575d, 0x806567cb,
                                                    0x196c3671, 0x6e6b06e7, 0xfed41b76, 0x89d32be0, 0x10da7a5a, 0x67dd4acc, 0xf9b9df6f, 0x8ebeeff9, 0x17b7be43, 0x60b08ed5,
                                                    0xd6d6a3e8, 0xa1d1937e, 0x38d8c2c4, 0x4fdff252, 0xd1bb67f1, 0xa6bc5767, 0x3fb506dd, 0x48b2364b, 0xd80d2bda, 0xaf0a1b4c,
                                                    0x36034af6, 0x41047a60, 0xdf60efc3, 0xa867df55, 0x316e8eef, 0x4669be79, 0xcb61b38c, 0xbc66831a, 0x256fd2a0, 0x5268e236,
                                                    0xcc0c7795, 0xbb0b4703, 0x220216b9, 0x5505262f, 0xc5ba3bbe, 0xb2bd0b28, 0x2bb45a92, 0x5cb36a04, 0xc2d7ffa7, 0xb5d0cf31,
                                                    0x2cd99e8b, 0x5bdeae1d, 0x9b64c2b0, 0xec63f226, 0x756aa39c, 0x026d930a, 0x9c0906a9, 0xeb0e363f, 0x72076785, 0x05005713,
                                                    0x95bf4a82, 0xe2b87a14, 0x7bb12bae, 0x0cb61b38, 0x92d28e9b, 0xe5d5be0d, 0x7cdcefb7, 0x0bdbdf21, 0x86d3d2d4, 0xf1d4e242,
                                                    0x68ddb3f8, 0x1fda836e, 0x81be16cd, 0xf6b9265b, 0x6fb077e1, 0x18b74777, 0x88085ae6, 0xff0f6a70, 0x66063bca, 0x11010b5c,
                                                    0x8f659eff, 0xf862ae69, 0x616bffd3, 0x166ccf45, 0xa00ae278, 0xd70dd2ee, 0x4e048354, 0x3903b3c2, 0xa7672661, 0xd06016f7,
                                                    0x4969474d, 0x3e6e77db, 0xaed16a4a, 0xd9d65adc, 0x40df0b66, 0x37d83bf0, 0xa9bcae53, 0xdebb9ec5, 0x47b2cf7f, 0x30b5ffe9,
                                                    0xbdbdf21c, 0xcabac28a, 0x53b39330, 0x24b4a3a6, 0xbad03605, 0xcdd70693, 0x54de5729, 0x23d967bf, 0xb3667a2e, 0xc4614ab8,
                                                    0x5d681b02, 0x2a6f2b94, 0xb40bbe37, 0xc30c8ea1, 0x5a05df1b, 0x2d02ef8d
                                                };
        #endregion

        #region Private fields
        // List of files to store
        private readonly List<ZipFileEntry> Files = new List<ZipFileEntry>();
        // Filename of storage file
        private string FileName;
        // Stream object of storage file
        private FileStream ZipFileStream;
        // General comment
        private string Comment = "";
        // Central dir image
        private byte[] CentralDirImage = null;
        // Existing files in zip
        private ushort ExistingFiles = 0;
        // File access for Open method
        private FileAccess Access;
        #endregion

        #region Public methods
        // Private constructor. Avoid direct construction
        private ZipStorer()
        {
        }

        /// <summary>
        /// Creates a new storage file
        /// </summary>
        /// <param name="_filename"></param>
        /// <param name="_comment"></param>
        /// <returns></returns>
        public static ZipStorer Create(string _filename, string _comment)
        {
            ZipStorer zip = new ZipStorer();
            zip.FileName = _filename;
            zip.Comment = _comment;
            zip.ZipFileStream = new FileStream(_filename, FileMode.Create, FileAccess.ReadWrite);
            zip.Access = FileAccess.Write;

            return zip;
        }

        /// <summary>
        /// Open an existing storage file
        /// </summary>
        /// <param name="_filename"></param>
        /// <param name="_access"></param>
        /// <returns></returns>
        public static ZipStorer Open(string _filename, FileAccess _access)
        {
            ZipStorer zip = new ZipStorer();
            zip.FileName = _filename;
            zip.ZipFileStream = new FileStream(_filename, FileMode.Open, _access == FileAccess.Read ? FileAccess.Read : FileAccess.ReadWrite);
            zip.Access = _access;

            if (zip.ReadFileInfo())
                return zip;

            throw new System.IO.InvalidDataException();
        }

        /// <summary>
        /// Add a file to the currently open storage file
        /// </summary>
        /// <param name="_pathname"></param>
        /// <param name="_filenameInZip"></param>
        /// <param name="_comment"></param>
        public void AddFile(string _pathname, string _filenameInZip, string _comment)
        {
            if (Access == FileAccess.Read)
                throw new InvalidOperationException("Writing is not alowed");

            FileStream stream = new FileStream(_pathname, FileMode.Open, FileAccess.Read);
            AddStream(_filenameInZip, stream, File.GetLastWriteTime(_pathname), _comment);
            stream.Close();
        }

        /// <summary>
        /// Add a file, based on a stream, to the currently open storage file
        /// </summary>
        /// <param name="_filenameInZip"></param>
        /// <param name="_source"></param>
        /// <param name="_modTime"></param>
        /// <param name="_comment"></param>
        public void AddStream(string _filenameInZip, Stream _source, DateTime _modTime, string _comment)
        {
            if (Access == FileAccess.Read)
                throw new InvalidOperationException("Writing is not alowed");

            long offset;
            if (this.Files.Count==0)
                offset = 0;
            else
            {
                ZipFileEntry last = this.Files[this.Files.Count-1];
                offset = last.HeaderOffset + last.HeaderSize;
            }

            // Prepare the fileinfo
            ZipFileEntry zfe = new ZipFileEntry();
            zfe.FilenameInZip = NormalizedFilename(_filenameInZip);
            zfe.Comment = (_comment ?? "");

            // Even though we write the header now, it will have to be rewritten, since we don't know compressed size or crc.
            zfe.Crc32 = 0;  // to be updated later
            zfe.HeaderOffset = (uint)this.ZipFileStream.Position;  // offset within file of the start of this local record
            zfe.ModifyTime = _modTime;

            // Write local header
            WriteLocalHeader(ref zfe);
            zfe.FileOffset = (uint)this.ZipFileStream.Position;

            // Write file to zip (store)
            zfe.FileSize = Store(ref zfe, _source);
            _source.Close();

            this.UpdateCrcAndSizes(ref zfe);

            Files.Add(zfe);
        }

        /// <summary>
        /// Close the storage file
        /// </summary>
        public void Close()
        {
            if (this.Access != FileAccess.Read)
            {
                uint centralOffset = (uint)this.ZipFileStream.Position;
                uint centralSize = 0;

                if (this.CentralDirImage != null)
                    this.ZipFileStream.Write(CentralDirImage, 0, CentralDirImage.Length);

                for (int i = 0; i < Files.Count; i++)
                {
                    long pos = this.ZipFileStream.Position;
                    this.WriteCentralDirRecord(Files[i]);
                    centralSize += (uint)(this.ZipFileStream.Position - pos);
                }

                if (this.CentralDirImage != null)
                    this.WriteEndRecord(centralSize + (uint)CentralDirImage.Length, centralOffset);
                else
                    this.WriteEndRecord(centralSize, centralOffset);
            }

            this.ZipFileStream.Close();
        }

        /// <summary>
        /// Read all of the file records
        /// </summary>
        /// <returns></returns>
        public List<ZipFileEntry> ReadCentralDir()
        {
            if (this.CentralDirImage == null)
                throw new InvalidOperationException("Central directory currently does not exist");

            List<ZipFileEntry> result = new List<ZipFileEntry>();

            for (int pointer = 0; pointer < this.CentralDirImage.Length; )
            {
                uint comprSize = BitConverter.ToUInt32(CentralDirImage, pointer + 20);
                ushort filenameSize = BitConverter.ToUInt16(CentralDirImage, pointer + 28);
                ushort extraSize = BitConverter.ToUInt16(CentralDirImage, pointer + 30);
                ushort commentSize = BitConverter.ToUInt16(CentralDirImage, pointer + 32);
                uint headerOffset = BitConverter.ToUInt32(CentralDirImage, pointer + 42);
                uint headerSize = (uint)( 46 + filenameSize + extraSize + commentSize);

                ZipFileEntry zfe = new ZipFileEntry();
                zfe.FilenameInZip = Encoding.UTF8.GetString(CentralDirImage, pointer + 46, filenameSize);
                zfe.FileOffset = GetFileOffset(headerOffset);
                zfe.FileSize = comprSize;
                zfe.HeaderOffset = headerOffset;
                zfe.HeaderSize = headerSize;
                //zfe.ModifyTime = ;  // Date format can vary
                if (commentSize > 0)
                    zfe.Comment = Encoding.UTF8.GetString(CentralDirImage, pointer + 46 + filenameSize + extraSize, commentSize);

                result.Add(zfe);
                pointer += (46 + filenameSize + extraSize + commentSize);
            }

            return result;
        }

        /// <summary>
        /// Calculate the file offset
        /// </summary>
        /// <param name="_headerOffset"></param>
        /// <returns></returns>
        public uint GetFileOffset(uint _headerOffset)
        {
            byte[] buffer = new byte[2];

            this.ZipFileStream.Seek(_headerOffset+26, SeekOrigin.Begin);
            this.ZipFileStream.Read(buffer, 0, 2);
            ushort filenameSize = BitConverter.ToUInt16(buffer, 0);
            this.ZipFileStream.Read(buffer, 0, 2);
            ushort extraSize = BitConverter.ToUInt16(buffer, 0);

            return (uint)(30 + filenameSize + extraSize + _headerOffset);
        }

        /// <summary>
        /// Extract a stored file
        /// </summary>
        /// <param name="_zfe"></param>
        /// <param name="_filename"></param>
        public void ExtractStoredFile(ZipFileEntry _zfe, string _filename)
        {
            byte[] buffer = new byte[32768];
            uint bytesPending = _zfe.FileSize;

            FileStream output = new FileStream(_filename, FileMode.Create, FileAccess.Write);
            this.ZipFileStream.Seek(_zfe.FileOffset, SeekOrigin.Begin);

            // Buffered copy
            while (bytesPending > 0)
            {
                int bytesRead = this.ZipFileStream.Read(buffer, 0, (int)Math.Min(bytesPending, buffer.Length));
                output.Write(buffer, 0, bytesRead);
                bytesPending -= (uint)bytesRead;
            }
            output.Close();
        }
        #endregion

        #region Private methods
        /* Local file header:
            local file header signature     4 bytes  (0x04034b50)
            version needed to extract       2 bytes
            general purpose bit flag        2 bytes
            compression method              2 bytes
            last mod file time              2 bytes
            last mod file date              2 bytes
            crc-32                          4 bytes
            compressed size                 4 bytes
            uncompressed size               4 bytes
            filename length                 2 bytes
            extra field length              2 bytes

            filename (variable size)
            extra field (variable size)
        */
        private void WriteLocalHeader(ref ZipFileEntry zfe)
        {
            long pos = this.ZipFileStream.Position;

            this.ZipFileStream.Write(new byte[] { 80, 75, 3, 4, 20, 0, 0, 0 }, 0, 8); // No extra header
            this.ZipFileStream.Write(BitConverter.GetBytes((ushort)0), 0, 2);  // stored (0)
            this.ZipFileStream.Write(BitConverter.GetBytes(DosTime(zfe.ModifyTime)), 0, 4); // zipping date and time
            this.ZipFileStream.Write(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 0, 12); // unused CRC, un/compressed size, updated later
            this.ZipFileStream.Write(BitConverter.GetBytes((ushort)zfe.FilenameInZip.Length), 0, 2); // filename length
            this.ZipFileStream.Write(BitConverter.GetBytes((ushort)0), 0, 2); // extra length

            this.ZipFileStream.Write(Encoding.UTF8.GetBytes(zfe.FilenameInZip), 0, zfe.FilenameInZip.Length);
            zfe.HeaderSize = (uint)(this.ZipFileStream.Position - pos);
        }
        /* Central directory's File header:
            central file header signature   4 bytes  (0x02014b50)
            version made by                 2 bytes
            version needed to extract       2 bytes
            general purpose bit flag        2 bytes
            compression method              2 bytes
            last mod file time              2 bytes
            last mod file date              2 bytes
            crc-32                          4 bytes
            compressed size                 4 bytes
            uncompressed size               4 bytes
            filename length                 2 bytes
            extra field length              2 bytes
            file comment length             2 bytes
            disk number start               2 bytes
            internal file attributes        2 bytes
            external file attributes        4 bytes
            relative offset of local header 4 bytes

            filename (variable size)
            extra field (variable size)
            file comment (variable size)
        */
        private void WriteCentralDirRecord(ZipFileEntry zfe)
        {
            this.ZipFileStream.Write(new byte[] { 80, 75, 1, 2, 23, 0xB, 20, 0, 0, 0 }, 0, 10);
            this.ZipFileStream.Write(BitConverter.GetBytes((ushort)0), 0, 2);  // zipping method: stored (0)
            this.ZipFileStream.Write(BitConverter.GetBytes(DosTime(zfe.ModifyTime)), 0, 4);  // zipping date and time
            this.ZipFileStream.Write(BitConverter.GetBytes(zfe.Crc32), 0, 4); // file CRC
            this.ZipFileStream.Write(BitConverter.GetBytes(zfe.FileSize), 0, 4); // compressed file size
            this.ZipFileStream.Write(BitConverter.GetBytes(zfe.FileSize), 0, 4); // uncompressed file size
            this.ZipFileStream.Write(BitConverter.GetBytes((ushort)zfe.FilenameInZip.Length), 0, 2); // Filename in zip
            this.ZipFileStream.Write(BitConverter.GetBytes((ushort)0), 0, 2); // extra length
            this.ZipFileStream.Write(BitConverter.GetBytes((ushort)zfe.Comment.Length), 0, 2);

            this.ZipFileStream.Write(BitConverter.GetBytes((ushort)0), 0, 2); // disk=0
            this.ZipFileStream.Write(BitConverter.GetBytes((ushort)0), 0, 2); // file type: binary
            this.ZipFileStream.Write(BitConverter.GetBytes((ushort)0), 0, 2); // Internal file attributes
            this.ZipFileStream.Write(BitConverter.GetBytes((ushort)0x8100), 0, 2); // External file attributes (normal/readable)
            this.ZipFileStream.Write(BitConverter.GetBytes(zfe.HeaderOffset), 0, 4);  // Offset of header

            this.ZipFileStream.Write(Encoding.UTF8.GetBytes(zfe.FilenameInZip), 0, zfe.FilenameInZip.Length);
            this.ZipFileStream.Write(Encoding.UTF8.GetBytes(zfe.Comment), 0, zfe.Comment.Length);
        }
        /* End of central dir record:
            end of central dir signature    4 bytes  (0x06054b50)
            number of this disk             2 bytes
            number of the disk with the
            start of the central directory  2 bytes
            total number of entries in
            the central dir on this disk    2 bytes
            total number of entries in
            the central dir                 2 bytes
            size of the central directory   4 bytes
            offset of start of central
            directory with respect to
            the starting disk number        4 bytes
            zipfile comment length          2 bytes
            zipfile comment (variable size)
        */
        private void WriteEndRecord(uint size, uint offset)
        {
            this.ZipFileStream.Write(new byte[] { 80, 75, 5, 6, 0, 0, 0, 0 }, 0, 8);
            this.ZipFileStream.Write(BitConverter.GetBytes((ushort)Files.Count+ExistingFiles), 0, 2);
            this.ZipFileStream.Write(BitConverter.GetBytes((ushort)Files.Count+ExistingFiles), 0, 2);
            this.ZipFileStream.Write(BitConverter.GetBytes(size), 0, 4);
            this.ZipFileStream.Write(BitConverter.GetBytes(offset), 0, 4);
            this.ZipFileStream.Write(BitConverter.GetBytes((ushort)this.Comment.Length), 0, 2);
            this.ZipFileStream.Write(Encoding.UTF8.GetBytes(this.Comment), 0, this.Comment.Length);
        }
        // Copies all source file into storage file
        private uint Store(ref ZipFileEntry zfe, Stream source)
        {
            byte[] buffer = new byte[16384];
            int bytesRead;
            uint totalRead = 0;

            do
            {
                bytesRead = source.Read(buffer, 0, buffer.Length);
                totalRead += (uint)bytesRead;
                if (bytesRead > 0)
                    this.ZipFileStream.Write(buffer, 0, bytesRead);
            } while (bytesRead == buffer.Length);

            return totalRead;
        }
        /* DOS Date and time:
            MS-DOS date. The date is a packed value with the following format. Bits Description 
                0-4 Day of the month (1–31) 
                5-8 Month (1 = January, 2 = February, and so on) 
                9-15 Year offset from 1980 (add 1980 to get actual year) 
            MS-DOS time. The time is a packed value with the following format. Bits Description 
                0-4 Second divided by 2 
                5-10 Minute (0–59) 
                11-15 Hour (0–23 on a 24-hour clock) 
        */
        private uint DosTime(DateTime dt)
        {
            return (uint)((dt.Second / 2) | (dt.Minute << 5) | (dt.Hour << 11) | (dt.Day<<16) | (dt.Month << 21) | ((dt.Year - 1980) << 25));
        }
        /* CRC32 algorithm
          The 'magic number' for the CRC is 0xdebb20e3.  
          The proper CRC pre and post conditioning
          is used, meaning that the CRC register is
          pre-conditioned with all ones (a starting value
          of 0xffffffff) and the value is post-conditioned by
          taking the one's complement of the CRC residual.
          If bit 3 of the general purpose flag is set, this
          field is set to zero in the local header and the correct
          value is put in the data descriptor and in the central
          directory.
        */
        private void UpdateCrcAndSizes(ref ZipFileEntry zfe)
        {
            long lastPos = this.ZipFileStream.Position;  // remember position

            zfe.Crc32 = 0 ^ 0xffffffff;
            this.ZipFileStream.Position = zfe.FileOffset;
            for (uint i = 0; i < zfe.FileSize; i++)
            {
                byte b = (byte)this.ZipFileStream.ReadByte();
                zfe.Crc32 = ZipStorer.CrcTable[(zfe.Crc32 ^ b) & 0xff] ^ (zfe.Crc32 >> 8);
            }
            zfe.Crc32 ^= 0xffffffff;

            this.ZipFileStream.Position = zfe.HeaderOffset + 14;
            this.ZipFileStream.Write(BitConverter.GetBytes(zfe.Crc32), 0, 4);  // Update CRC
            this.ZipFileStream.Write(BitConverter.GetBytes(zfe.FileSize), 0, 4);  // Compressed size
            this.ZipFileStream.Write(BitConverter.GetBytes(zfe.FileSize), 0, 4);  // Uncompressed size

            this.ZipFileStream.Position = lastPos;  // restore position
        }
        // Replaces backslashes with slashes to store in zip header
        private static string NormalizedFilename(string _filename)
        {
            string filename = _filename.Replace('\\', '/');

            int pos = filename.IndexOf(':');
            if (pos >= 0)
                filename = filename.Remove(0, pos + 1);

            return filename.Trim('/');
        }
        // Read entire directory, by reading local headers
        private bool ReadFileInfo()
        {
            byte[] sig = new byte[4];
            byte[] header = new byte[42];
            uint signature;
            ushort filenameSize, extraSize, commentSize;
            uint comprSize;

            try
            {
                this.ZipFileStream.Seek(0, SeekOrigin.Begin);
                do
                {
                    if (this.ZipFileStream.Read(sig, 0, 4) < 4) // end of file
                        break;
                    signature = BitConverter.ToUInt32(sig, 0);

                    if (signature == 0x04034b50)  // Local header
                    {
                        if (this.ZipFileStream.Read(header, 0, 26) < 26) // error
                            return false;

                        comprSize    = BitConverter.ToUInt32(header, 14);
                        filenameSize = BitConverter.ToUInt16(header, 22);
                        extraSize    = BitConverter.ToUInt16(header, 24);

                        // Just skip the record and file contents to reach the end-of-central-dir entry
                        this.ZipFileStream.Seek(comprSize + filenameSize + extraSize, SeekOrigin.Current);

                        ExistingFiles++;
                    }
                    else if (signature == 0x02014b50)  // Central dir header
                    {
                        if (this.ZipFileStream.Read(header, 0, 42) < 42) // error
                            return false;

                        filenameSize = BitConverter.ToUInt16(header, 24);
                        extraSize    = BitConverter.ToUInt16(header, 26);
                        commentSize  = BitConverter.ToUInt16(header, 28);

                        // Just skip the record to reach the end-of-central-dir entry
                        this.ZipFileStream.Seek(filenameSize + extraSize + commentSize, SeekOrigin.Current);
                    }
                    else if (signature == 0x06054b50) // end of central dir
                    {
                        if (this.ZipFileStream.Read(header, 0, 18) < 18) // error
                            return false;

                        int centralSize = BitConverter.ToInt32(header, 8);
                        uint centralDirOffset = BitConverter.ToUInt32(header, 12);

                        // Copy entire central directory to a memory buffer
                        this.CentralDirImage = new byte[centralSize];
                        this.ZipFileStream.Seek(centralDirOffset, SeekOrigin.Begin);
                        this.ZipFileStream.Read(this.CentralDirImage, 0, centralSize);

                        // Leave the pointer at the begining of central dir, to append new files
                        this.ZipFileStream.Seek(centralDirOffset, SeekOrigin.Begin);

                        break;
                    }
                } while (true);

                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}