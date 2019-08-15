// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using Microsoft.VisualStudio.PlatformUI;
using System.Globalization;
using System.Text;

namespace SettingsStoreExplorer
{
    internal class BinaryToHexDumpConverter : ValueConverter<byte[], string>
    {
        protected override string Convert(byte[] value, object parameter, CultureInfo culture)
            => value == null ? "<null>" : BinaryToString(value);

        /// <summary>
        /// Render the byte array as a string in hex-dump format.
        /// This matches the format used by regedit for REG_BINARY.
        /// </summary>
        /// <param name="binary">The bytes.</param>
        /// <returns>A string representing a hex-dump.</returns>
        /// <example>
        /// 0000      01    23    45    67    FF    12    30     . # E g ÿ þ . 0
        /// 0008      98    17    23    10                       . . # .
        /// </example>
        private static string BinaryToString(byte[] binary)
        {
            var sbMain = new StringBuilder();
            var sbAscii = new StringBuilder();

            int row = 0;
            int offset = 0;
            foreach (var x in binary)
            {
                if (offset == 0)
                {
                    sbMain.Append((row * 8).ToString("X4"));
                    sbMain.Append("   ");
                }

                sbMain.AppendFormat(" {0:X2}   ", x);

                if (char.IsControl((char)x))
                {
                    sbAscii.Append('.');
                }
                else
                {
                    sbAscii.Append((char)x);
                }
               
                sbAscii.Append(' ');

                offset++;
                if (offset == 8)
                {
                    sbMain.AppendLine(sbAscii.ToString());
                    sbAscii.Clear();
                    offset = 0;
                    row++;
                }
            }

            if (offset != 0)
            {
                sbMain.Append(' ', 6 * (8 - offset));
                sbMain.Append(sbAscii.ToString());
            }

            return sbMain.ToString();
        }
    }
}
