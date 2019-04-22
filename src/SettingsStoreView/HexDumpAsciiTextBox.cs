// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System.Text;
using System.Windows.Input;

namespace SettingsStoreView
{
    internal sealed class HexDumpAsciiTextBox : HexDumpControlCustomTextBox
    {
        /// <summary>
        /// The width of one rendered ASCII char in the ASCII text box.
        /// </summary>
        private const int AsciiWidth = 2;

        protected override StringBuilder AppendFormattedByte(StringBuilder sbAscii, byte x)
        {
            if (char.IsControl((char)x))
            {
                sbAscii.Append('.');
            }
            else
            {
                sbAscii.Append((char)x);
            }

            sbAscii.Append(' ', AsciiWidth - 1);

            return sbAscii;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            switch(e.Key)
            {
                case Key.Space:
                    InsertOrReplaceSelection((byte)' ');
                    e.Handled = true;
                    break;
            }
        }

        protected override void OnChar(char ch)
        {
            if (ch >= 32 && ch <= 255)
            {
                InsertOrReplaceSelection((byte)ch);
            }
        }
    }
}
