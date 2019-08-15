// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System.Text;
using System.Windows.Input;
using static System.Globalization.CultureInfo;

namespace SettingsStoreExplorer
{
    internal sealed class HexDumpHexBytesTextBox : HexDumpControlCustomTextBox
    {
        /// <summary>
        /// The format string used to format bytes as hexadecimal.
        /// </summary>
        private const string ByteFormatString = " {0:X2}   ";

        /// <summary>
        /// Set to true if you've already typed the first (high) nybble of a hex value.
        /// </summary>
        private bool _highNybble;

        protected override StringBuilder AppendFormattedByte(StringBuilder sb, byte b)
            => sb.AppendFormat(InvariantCulture, ByteFormatString, b);

        protected override void MoveSelection(int selectionStart, int selectionLength)
        {
            if (_highNybble && selectionLength == 0)
            {
                selectionStart += 3;
            }

            base.MoveSelection(selectionStart, selectionLength);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            var oldHighByte = _highNybble;
            _highNybble = false;

            base.OnPreviewKeyDown(e);

            if (oldHighByte && !e.Handled)
            {
                _highNybble = true;
            }
        }

        protected override void OnChar(char ch)
        {
            if (TryParseHexDigit(ch, out var digit))
            {
                var (insertPoint, replacementLength) = InsertPointAndSelectionLength;
                if (insertPoint < 0)
                {
                    return;
                }

                if (_highNybble)
                {
                    _highNybble = false;
                    InsertOrReplaceByte(insertPoint, replacementLength: 1, replacementValue: (byte)(GetByteAt(insertPoint) | digit));
                    Select(CaretIndexFromByteOffset(insertPoint + 1), 0);
                }
                else
                {
                    _highNybble = true;
                    InsertOrReplaceByte(insertPoint, replacementLength, (byte)(digit << 4));
                    Select(CaretIndexFromByteOffset(insertPoint), 0);
                }
            }
        }

        /// <summary>
        /// Try to parse the given character as a hexadecimal digit.
        /// </summary>
        /// <param name="ch">The character to parse.</param>
        /// <param name="digit">The parsed hexadecimal digit.</param>
        /// <returns>True if <paramref name="ch"/> is a valid hexadecimal digit.</returns>
        private static bool TryParseHexDigit(char ch, out byte digit)
        {
            if (ch >= '0' && ch <= '9')
            {
                digit = (byte)(ch - '0');
                return true;
            }

            if (ch >= 'A' && ch <= 'F')
            {
                digit = (byte)(10 + ch - 'A');
                return true;
            }

            if (ch >= 'a' && ch <= 'f')
            {
                digit = (byte)(10 + ch - 'a');
                return true;
            }

            digit = 0;
            return false;
        }
    }
}
