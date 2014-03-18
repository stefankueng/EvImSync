// Evernote2Onenote - imports Evernote notes to Onenote
// Copyright (C) 2014 - Stefan Kueng

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace Evernote2Onenote
{
    /// <summary>
    /// Represents an attachment
    /// </summary>
    public class Attachment
    {
        /// <summary>
        /// The data of the attachment, encoded in BASE64
        /// </summary>
        public string Base64Data { get; set; }

        /// <summary>
        /// the content ID
        /// </summary>
        public string ContentID { get; set; }

        /// <summary>
        /// the content type, e.g. "image/jpeg"
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// the file name of the attachment
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// the hash of the attachment 
        /// </summary>
        public string Hash { get; set; }
    }
}
