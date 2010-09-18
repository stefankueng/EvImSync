// EvImSync - A tool to sync Evernote notes to IMAP mails and vice versa
// Copyright (C) 2010 - Stefan Kueng

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

using System;
using System.IO;
using System.Security.Cryptography;

namespace EveImSync
{
    public class SimpleAES
    {
        // Change these keys for other applications
        private byte[] key = { 127, 15, 214, 77, 101, 74, 133, 232, 95, 125, 58, 4, 124, 12, 46, 163, 113, 101, 53, 68, 160, 42, 224, 204, 15, 126, 229, 51, 183, 118, 7, 84 };
        private byte[] vector = { 145, 226, 144, 251, 106, 226, 239, 188, 163, 36, 110, 124, 106, 240, 141, 113 };

        private ICryptoTransform encryptorTransform, decryptorTransform;
        private System.Text.UTF8Encoding utfEncoder;

        public SimpleAES()
        {
            // This is our encryption method
            RijndaelManaged rm = new RijndaelManaged();

            // Create an encryptor and a decryptor using our encryption method, key, and vector.
            encryptorTransform = rm.CreateEncryptor(this.key, this.vector);
            decryptorTransform = rm.CreateDecryptor(this.key, this.vector);

            // Used to translate bytes to text and vice versa
            utfEncoder = new System.Text.UTF8Encoding();
        }

        // -------------- Two Utility Methods (not used but may be useful) -----------

        /// <summary>
        /// Generates an encryption key.
        /// </summary>
        public static byte[] GenerateEncryptionKey()
        {
            // Generate a Key.
            RijndaelManaged rm = new RijndaelManaged();
            rm.GenerateKey();
            return rm.Key;
        }

        /// <summary>
        /// Generates a unique encryption vector
        /// </summary>
        public static byte[] GenerateEncryptionVector()
        {
            // Generate a Vector
            RijndaelManaged rm = new RijndaelManaged();
            rm.GenerateIV();
            return rm.IV;
        }

        // ----------- The commonly used methods ------------------------------

        /// <summary>
        /// Encrypt some text and return a string suitable for passing in a URL.
        /// </summary>
        public string EncryptToString(string textValue)
        {
            return ByteArrToString(Encrypt(textValue));
        }

        /// Encrypt some text and return an encrypted byte array.
        /// <summary>
        /// Encrypt some text and return a string suitable for passing in a URL.
        /// </summary>
        public byte[] Encrypt(string textValue)
        {
            // Translates our text value into a byte array.
            byte[] bytes = utfEncoder.GetBytes(textValue);

            // Used to stream the data in and out of the CryptoStream.
            MemoryStream memoryStream = new MemoryStream();

            // We will have to write the unencrypted bytes to the stream,
            // then read the encrypted result back from the stream.
            CryptoStream cs = new CryptoStream(memoryStream, encryptorTransform, CryptoStreamMode.Write);
            cs.Write(bytes, 0, bytes.Length);
            cs.FlushFinalBlock();

            memoryStream.Position = 0;
            byte[] encrypted = new byte[memoryStream.Length];
            memoryStream.Read(encrypted, 0, encrypted.Length);

            // Clean up.
            cs.Close();
            memoryStream.Close();

            return encrypted;
        }

        /// The other side: Decryption methods
        /// <summary>
        /// Encrypt some text and return a string suitable for passing in a URL.
        /// </summary>
        public string DecryptString(string encryptedString)
        {
            return Decrypt(StrToByteArray(encryptedString));
        }

        /// <summary>
        /// Decryption when working with byte arrays. 
        /// </summary>
        public string Decrypt(byte[] encryptedValue)
        {
            MemoryStream encryptedStream = new MemoryStream();
            CryptoStream decryptStream = new CryptoStream(encryptedStream, decryptorTransform, CryptoStreamMode.Write);
            decryptStream.Write(encryptedValue, 0, encryptedValue.Length);
            decryptStream.FlushFinalBlock();

            encryptedStream.Position = 0;
            byte[] decryptedBytes = new byte[encryptedStream.Length];
            encryptedStream.Read(decryptedBytes, 0, decryptedBytes.Length);
            encryptedStream.Close();

            return utfEncoder.GetString(decryptedBytes);
        }

        /// <summary>
        /// Convert a string to a byte array.  NOTE: Normally we'd create a Byte Array 
        /// from a string using an ASCII encoding (like so):
        /// <para/>
        /// System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
        /// return encoding.GetBytes(str);
        /// <para/>
        /// However, this results in character values that cannot be passed in a URL.  So, instead, I just
        /// lay out all of the byte values in a long string of numbers (three per - must pad numbers less than 100).
        /// </summary>
        public byte[] StrToByteArray(string str)
        {
            if (str.Length == 0)
            {
                throw new Exception("Invalid string value in StrToByteArray");
            }

            byte val;
            byte[] byteArr = new byte[str.Length / 3];
            int i = 0;
            int j = 0;
            do
            {
                val = byte.Parse(str.Substring(i, 3));
                byteArr[j++] = val;
                i += 3;
            }
            while (i < str.Length);

            return byteArr;
        }

        /// <summary>
        /// Same comment as above.  Normally the conversion would use an ASCII encoding in the other direction:
        /// <para/>
        /// System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
        /// return enc.GetString(byteArr); 
        /// </summary>
        public string ByteArrToString(byte[] byteArr)
        {
            byte val;
            string tempStr = string.Empty;
            for (int i = 0; i <= byteArr.GetUpperBound(0); i++)
            {
                val = byteArr[i];
                if (val < (byte)10)
                {
                    tempStr += "00" + val.ToString();
                }
                else if (val < (byte)100)
                {
                    tempStr += "0" + val.ToString();
                }
                else
                {
                    tempStr += val.ToString();
                }
            }

            return tempStr;
        }
    }
}
