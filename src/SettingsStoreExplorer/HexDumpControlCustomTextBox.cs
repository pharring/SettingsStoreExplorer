// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace SettingsStoreExplorer
{
    /// <summary>
    /// A hex dump control that allows editing. Modeled on the REG_BINARY editor in Regedit.exe
    /// </summary>
    internal abstract class HexDumpControlCustomTextBox : TextBoxWithSelectionAdorner
    {
        /// <summary>
        /// The number of bytes per row of hex dump.
        /// </summary>
        private const int c_bytesPerRow = 8;

        /// <summary>
        /// The current length of the byte array.
        /// </summary>
        private int _currentLength;

        /// <summary>
        /// The width of a single, formatted byte.
        /// </summary>
        private int _quantumWidth;

        /// <summary>
        /// When selecting text you can select from "start to the end" or "end to start".
        /// The underlying <see cref="TextBox"/> control allows for SelectionStart and
        /// SelectionLength, but that doesn't indicate the direction (SelectionLength must
        /// always be positive). So this flag indicates the direction. If it's true, then
        /// the selection started "backwards". i.e. the anchor point is at SelectionEnd
        /// (SelectionStart + SelectionLength).
        /// </summary>
        private bool _isAnchorPointAtTheEndOfSelectedRange;

        protected HexDumpControlCustomTextBox() => DataContextChanged += OnDataContextChanged;

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is HexDumpControlDataContext oldDataContext)
            {
                oldDataContext.PropertyChanged -= BytesChanged;
            }

            if (e.NewValue is HexDumpControlDataContext newDataContext)
            {
                newDataContext.PropertyChanged += BytesChanged;
                RefreshText(newDataContext.Bytes);
            }
        }

        private void BytesChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(HexDumpControlDataContext.Bytes))
            {
                var dataContext = (HexDumpControlDataContext)sender;
                RefreshText(dataContext.Bytes);
            }
        }

        /// <summary>
        /// Refresh the content of the text box when the data context changes.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        private void RefreshText(ICollection<byte> bytes)
        {
            _currentLength = bytes.Count;
            var text = BuildContent(bytes);
            Text = text;
        }

        private byte[] TheBytes
        {
            get => ((HexDumpControlDataContext)DataContext).Bytes;
            set => ((HexDumpControlDataContext)DataContext).Bytes = value;
        }

        /// <summary>
        /// Build the textual content.
        /// </summary>
        /// <param name="bytes">The new data context as a sequence of bytes.</param>
        /// <returns>The new content.</returns>
        private string BuildContent(ICollection<byte> bytes)
        {
            var sb = new StringBuilder();

            if (_quantumWidth == 0)
            {
                AppendFormattedByte(sb, 0);
                _quantumWidth = sb.Length;
                sb.Clear();
            }

            int row = 0;
            int offset = 0;
            foreach (var x in bytes)
            {
                AppendFormattedByte(sb, x);

                offset++;

                if (offset == c_bytesPerRow)
                {
                    sb.AppendLine();
                    offset = 0;
                    row++;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Format the given byte appropriately and append it to the string builder.
        /// </summary>
        /// <param name="sb">The string builder.</param>
        /// <param name="b">The byte to format.</param>
        /// <returns>The string builder.</returns>
        protected abstract StringBuilder AppendFormattedByte(StringBuilder sb, byte b);

        /// <summary>
        /// Set the current selection/caret.
        /// </summary>
        /// <param name="selectionStart">The selection point or caret index.</param>
        /// <param name="selectionLength">The selection length. May be zero.</param>
        protected virtual void MoveSelection(int selectionStart, int selectionLength)
        {
            if ((selectionStart != CaretIndex || SelectionLength != 0) && selectionStart >= 0 && selectionStart < Text.Length)
            {
                Select(selectionStart, selectionLength);
            }
        }

        /// <summary>
        /// Called when a character is typed.
        /// </summary>
        /// <param name="ch">The character typed.</param>
        protected virtual void OnChar(char ch)
        {
        }

        /// <summary>
        /// Called when the selection has changed or the caret moved.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSelectionChanged(RoutedEventArgs e)
        {
            SnapSelection();
            base.OnSelectionChanged(e);
        }

        /// <summary>
        /// Snap the selection to quantized values.
        /// </summary>
        /// <param name="textBox">The text box whose selection has changed.</param>
        /// <param name="width">The quantum width. i.e. the number of chars per byte.</param>
        private void SnapSelection()
        {
            var caretIndex = SelectionStart;
            var selectionLength = SelectionLength;

            if (!GetRowColFromCaretIndex(caretIndex, out int rowStart, out int colStart))
            {
                return;
            }

            if (selectionLength == 0)
            {
                // Just moving the caret
                UpdateInsertPoint(rowStart, colStart);
                return;
            }

            // Handle extended selection
            int selectionEnd = caretIndex + selectionLength;
            if (!GetRowColFromCaretIndex(selectionEnd, out int rowEnd, out int colEnd))
            {
                return;
            }

            if (rowStart == rowEnd && colStart == colEnd)
            {
                UpdateInsertPoint(rowStart, colStart);
                return;
            }

            UpdateSelection(rowStart, colStart, rowEnd, colEnd);
        }

        private void UpdateSelection((int row, int col) start, (int row, int col) end)
        {
            if (start.Equals(end))
            {
                UpdateInsertPoint(start.row, start.col);
            }
            else
            {
                UpdateSelection(start.row, start.col, end.row, end.col);
            }
        }

        /// <summary>
        /// Set the insert point (caret).
        /// </summary>
        /// <param name="row">The row (line number) of the insert point.</param>
        /// <param name="col">The column (offset from start of line) of the insert point.</param>
        private void UpdateInsertPoint(int row, int col)
        {
            // Note that col may be from 0 to BytesPerRow **inclusive**.
            // i.e. we can place the insert point off the end of a row.
            var newInsertPoint = (row * c_bytesPerRow) + col;
            if (newInsertPoint > _currentLength)
            {
                col = _currentLength % c_bytesPerRow;
            }

            var quantizedCaretIndex = GetCharacterIndexFromLineIndex(row) + (col * _quantumWidth);
            MoveSelection(quantizedCaretIndex, 0);
        }

        /// <summary>
        /// Update an extended selection in a text box.
        /// </summary>
        /// <param name="textBox">The text box.</param>
        /// <param name="width">The quantum width. i.e. the number of chars per byte.</param>
        /// <param name="rowStart">The row of the start of the selection.</param>
        /// <param name="colStart">The column of the start of the selection.</param>
        /// <param name="rowEnd">The row of the end of the selection.</param>
        /// <param name="colEnd">The column of the end of the selection.</param>
        private void UpdateSelection(int rowStart, int colStart, int rowEnd, int colEnd)
        {
            var selectionStart = GetCharacterIndexFromLineIndex(rowStart) + (colStart * _quantumWidth);
            var selectionEnd = GetCharacterIndexFromLineIndex(rowEnd) + (colEnd * _quantumWidth);
            var selectionLength = selectionEnd - selectionStart;

            if (SelectionStart != selectionStart || SelectionLength != selectionLength)
            {
                Select(selectionStart, selectionLength);
            }
        }

        /// <summary>
        /// Given a character position (caret index), compute the row and column (line and index).
        /// </summary>
        /// <param name="textBox">The text box.</param>
        /// <param name="width">The quantum width. i.e. the number of chars per byte.</param>
        /// <param name="caretIndex">The caret index.</param>
        /// <param name="row">The computed row in byte array space.</param>
        /// <param name="col">The computed column in byte array space.</param>
        /// <returns></returns>
        private bool GetRowColFromCaretIndex(int caretIndex, out int row, out int col)
        {
            row = GetLineIndexFromCharacterIndex(caretIndex);
            if (row == -1)
            {
                // May return -1 before the first render.
                col = 0;
                return false;
            }

            col = caretIndex - GetCharacterIndexFromLineIndex(row);

            // Convert to byte array space
            col /= _quantumWidth;

            // Note: The last column on one row, is the same as column 0 on the next row.
            // So we allow 0 to BytesPerRow **inclusive**.
            col = Math.Min(col, c_bytesPerRow);

            return true;
        }

        /// <summary>
        /// Convert the given byte offset into a coordinate (row, col)
        /// <seealso cref="ByteOffsetFromRowCol(int, int)"/>.
        /// </summary>
        /// <param name="offset">Offset into the bytes.</param>
        /// <returns>The row,col.</returns>
        private static (int row, int col) GetRowCol(int offset) => (offset / c_bytesPerRow, offset % c_bytesPerRow);

        /// <summary>
        /// Get the byte offset from the given (row, col).
        /// <seealso cref="GetRowCol(int)"/>.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="col">The column.</param>
        /// <returns></returns>
        private static int ByteOffsetFromRowCol(int row, int col) => (row * c_bytesPerRow) + col;

        /// <summary>
        /// Get the byte offset from the given caret index.
        /// <seealso cref="CaretIndexFromByteOffset(int)"/>.
        /// </summary>
        /// <param name="caretIndex">The caret index.</param>
        /// <returns>
        /// The byte offset in the bytes. May be -1 early on before
        /// text has been formatted.
        /// </returns>
        private int ByteOffsetFromCaretIndex(int caretIndex)
            => !GetRowColFromCaretIndex(caretIndex, out var row, out var col) ? -1 : ByteOffsetFromRowCol(row, col);

        /// <summary>
        /// Get the caret index for the given byte array offset.
        /// <seealso cref="ByteOffsetFromCaretIndex(int)"/>.
        /// </summary>
        /// <param name="offset">The byte array offset.</param>
        /// <returns>The caret index.</returns>
        protected int CaretIndexFromByteOffset(int offset)
        {
            var (row, col) = GetRowCol(offset);
            return CaretIndexFromRowCol(row, col);
        }

        /// <summary>
        /// Get the caret index for the given row, col pair.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="col">The column.</param>
        /// <returns>The caret index.</returns>
        private int CaretIndexFromRowCol(int row, int col)
            => GetCharacterIndexFromLineIndex(row) + (col * _quantumWidth);

        /// <summary>
        /// PreviewKeyDown handler.
        /// </summary>
        /// <param name="e">The event.</param>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            HandlePreviewKeyDown(e);
            base.OnPreviewKeyDown(e);
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            if (e.Text.Length == 1)
            {
                OnChar(e.Text[0]);
            }

            e.Handled = true;
            base.OnPreviewTextInput(e);
        }

        private void HandlePreviewKeyDown(KeyEventArgs e)
        {
            // If Shift is held down and an arrow-key is pressed, then extend selection
            // instead of moving caret. If Shift is not down, but SelectionLength is non-zero,
            // then remove the selection. In that case, the new caret position depends on the
            // key pressed.

            // To handle keyboard selection extents (Shift+Arrow Keys) properly,
            // we need to  remember an anchor point -- where the caret was when the selection
            // was first started. It doesn't look like TextBox exposes that. 
            // Note that TextBox.CaretIndex is the same thing as TextBox.SelectionStart
            // So the anchor point may be either at the end of the selection (if extended
            // to the 'left') or at the start of the selection (if extended to the right).
            // We don't need the actual anchor point; just a value indicating whether the
            // extent is "to the left" or "to the right". i.e. SelectionStart is always at
            // the left, but is the "caret" supposed to be at the beginning or the end of
            // the selection.
            // CONSIDER: Another way to think about it is having a negative selection length
            // if the anchor point is at the 'end'.

            var isShiftDown = (e.KeyboardDevice.Modifiers & ModifierKeys.Shift) != 0;
            var isCtrlDown = (e.KeyboardDevice.Modifiers & ModifierKeys.Control) != 0;

            var selectionLength = SelectionLength;
            if (isShiftDown && selectionLength == 0)
            {
                // If you start with Shift+Left or Shift+Up, then the anchor point is at the
                // end of the range.
                _isAnchorPointAtTheEndOfSelectedRange = e.Key == Key.Left || e.Key == Key.Up;
            }

            var caretIndex = SelectionStart + (_isAnchorPointAtTheEndOfSelectedRange ? 0 : selectionLength);

            GetRowColFromCaretIndex(caretIndex, out int row, out int col);
            var insertPoint = ByteOffsetFromRowCol(row, col);

            int replacementLength;
            if (selectionLength == 0)
            {
                replacementLength = 0;
            }
            else
            {
                var anchorPointIndex = SelectionStart + (_isAnchorPointAtTheEndOfSelectedRange ? selectionLength : 0);
                // Note: replacementLength may be negative if the anchor point is at the start of the selection.
                replacementLength = ByteOffsetFromCaretIndex(anchorPointIndex) - insertPoint;
            }

            void UpdateSelection(int newCaretIndex)
            {
                var newSelectionLength = 0;
                if (isShiftDown)
                {
                    if (_isAnchorPointAtTheEndOfSelectedRange)
                    {
                        // Selection is extending up
                        newSelectionLength = SelectionStart + SelectionLength - newCaretIndex;
                    }
                    else
                    {
                        // Selection is shrinking
                        newSelectionLength = newCaretIndex - SelectionStart;
                        newCaretIndex = SelectionStart;
                    }

                    if (newSelectionLength < 0)
                    {
                        newCaretIndex += newSelectionLength;
                        newSelectionLength = -newSelectionLength;
                        _isAnchorPointAtTheEndOfSelectedRange = !_isAnchorPointAtTheEndOfSelectedRange;
                    }
                }

                Select(newCaretIndex, newSelectionLength);

                if (_isAnchorPointAtTheEndOfSelectedRange)
                {
                    ScrollToLine(GetLineIndexFromCharacterIndex(newCaretIndex));
                }
            }

            // Handle arrow-key navigation
            switch (e.Key)
            {
                case Key.Left:
                    if (col != 0)
                    {
                        col--;
                    }
                    else
                    {
                        col = c_bytesPerRow - 1;
                        row--;
                    }

                    if (row >= 0)
                    {
                        UpdateSelection(CaretIndexFromRowCol(row, col));
                    }
                    e.Handled = true;
                    break;

                case Key.Right:
                    if (insertPoint < _currentLength)
                    {
                        UpdateSelection(CaretIndexFromByteOffset(insertPoint + 1));
                    }
                    e.Handled = true;
                    break;

                case Key.Up:
                    if (row != 0)
                    {
                        UpdateSelection(CaretIndexFromRowCol(row - 1, col));
                    }
                    e.Handled = true;
                    break;

                case Key.Down:
                    int lastRow = _currentLength / c_bytesPerRow;
                    if (row < lastRow)
                    {
                        UpdateSelection(CaretIndexFromRowCol(row + 1, col));
                    }
                    e.Handled = true;
                    break;

                case Key.Home:
                    if (isCtrlDown)
                    {
                        if (insertPoint != 0)
                        {
                            UpdateSelection(CaretIndexFromByteOffset(0));
                        }
                    }
                    else if (col != 0)
                    {
                        UpdateSelection(CaretIndexFromRowCol(row, 0));
                    }
                    e.Handled = true;
                    break;

                case Key.End:
                    // Caret is moved to a point after the last value in the row
                    if (isCtrlDown)
                    {
                        if (insertPoint < _currentLength)
                        {
                            UpdateSelection(CaretIndexFromByteOffset(_currentLength));
                        }
                    }
                    else if (col != c_bytesPerRow)
                    {
                        UpdateSelection(CaretIndexFromRowCol(row, c_bytesPerRow));
                    }
                    e.Handled = true;
                    break;

                case Key.PageUp:
                case Key.PageDown:
                    // Do nothing.
                    e.Handled = true;
                    break;

                case Key.Back:
                    if (replacementLength == 0)
                    {
                        // Delete to the left of the caret (if not at the start)
                        if (insertPoint != 0)
                        {
                            var currBytes = TheBytes;
                            var newBytes = new byte[_currentLength - 1];
                            Array.Copy(currBytes, 0, newBytes, 0, insertPoint - 1);
                            Array.Copy(currBytes, insertPoint, newBytes, insertPoint - 1, _currentLength - insertPoint);
                            TheBytes = newBytes;
                            goto case Key.Left;
                        }
                    }
                    else
                    {
                        var currBytes = TheBytes;
                        byte[] newBytes;
                        if (replacementLength > 0)
                        {
                            newBytes = new byte[_currentLength - replacementLength];
                            Array.Copy(currBytes, 0, newBytes, 0, insertPoint);
                            Array.Copy(currBytes, insertPoint + replacementLength, newBytes, insertPoint, _currentLength - replacementLength - insertPoint);
                        }
                        else
                        {
                            newBytes = new byte[_currentLength + replacementLength];
                            Array.Copy(currBytes, 0, newBytes, 0, insertPoint + replacementLength);
                            Array.Copy(currBytes, insertPoint, newBytes, insertPoint + replacementLength, _currentLength - insertPoint);
                        }

                        int finalSelectionStart = SelectionStart;
                        TheBytes = newBytes;
                        Select(finalSelectionStart, 0);
                    }
                    e.Handled = true;
                    break;

                case Key.Delete:
                    if (replacementLength == 0)
                    {
                        // Delete to the right (if not at end)
                        if (insertPoint != _currentLength)
                        {
                            var currBytes = TheBytes;
                            var newBytes = new byte[_currentLength - 1];
                            Array.Copy(currBytes, 0, newBytes, 0, insertPoint);
                            Array.Copy(currBytes, insertPoint + 1, newBytes, insertPoint, _currentLength - (insertPoint + 1));
                            TheBytes = newBytes;
                            Select(caretIndex, 0);
                        }
                    }
                    else
                    {
                        var currBytes = TheBytes;
                        byte[] newBytes;
                        if (replacementLength > 0)
                        {
                            newBytes = new byte[_currentLength - replacementLength];
                            Array.Copy(currBytes, 0, newBytes, 0, insertPoint);
                            Array.Copy(currBytes, insertPoint + replacementLength, newBytes, insertPoint, _currentLength - replacementLength - insertPoint);
                        }
                        else
                        {
                            newBytes = new byte[_currentLength + replacementLength];
                            Array.Copy(currBytes, 0, newBytes, 0, insertPoint + replacementLength);
                            Array.Copy(currBytes, insertPoint, newBytes, insertPoint + replacementLength, _currentLength - insertPoint);
                        }

                        int finalSelectionStart = SelectionStart;
                        TheBytes = newBytes;
                        Select(finalSelectionStart, 0);
                    }

                    e.Handled = true;
                    break;

                case Key.Space:
                    e.Handled = true;
                    break;
            }
        }

        protected void InsertOrReplaceSelection(byte replacementValue)
        {
            var (insertPoint, selectionLength) = InsertPointAndSelectionLength;
            if (insertPoint < 0)
            {
                return;
            }

            InsertOrReplaceByte(insertPoint, selectionLength, replacementValue);
            Select(CaretIndexFromByteOffset(insertPoint + 1), 0);
        }

        protected byte GetByteAt(int offset) => TheBytes[offset];

        protected void InsertOrReplaceByte(int insertPoint, int replacementLength, byte replacementValue)
        {
            // TODO: Edits are quite inefficient, allocating a new byte array each time.
            // This is partly because we need to trigger a DataContextChanged. Perhaps
            // make the DataContext an object with a List<byte> property and supporting
            // INotifyPropertyChanged.
            var currBytes = TheBytes;
            var newBytes = new byte[_currentLength - replacementLength + 1];
            Array.Copy(currBytes, 0, newBytes, 0, insertPoint);
            newBytes[insertPoint] = replacementValue;
            Array.Copy(currBytes, insertPoint + replacementLength, newBytes, insertPoint + 1, _currentLength - insertPoint - replacementLength);
            TheBytes = newBytes;
        }

        /// <summary>
        /// Get the current insert point and selection length in byte array space.
        /// </summary>
        /// <returns>The insert point (offset from start of array) and selection length (in bytes).</returns>
        public (int insertPoint, int selectionLength) InsertPointAndSelectionLength
        {
            get
            {
                var caretIndex = CaretIndex;
                var insertPoint = ByteOffsetFromCaretIndex(caretIndex);
                if (insertPoint < 0)
                {
                    // This can happen before the first render.
                    return (insertPoint, 0);
                }

                var selectionLength = SelectionLength;
                if (selectionLength == 0)
                {
                    return (insertPoint, 0);
                }

                selectionLength = ByteOffsetFromCaretIndex(caretIndex + selectionLength) - insertPoint;
                return (insertPoint, selectionLength);
            }

            set
            {
                var start = GetRowCol(value.insertPoint);
                var end = GetRowCol(value.insertPoint + value.selectionLength);
                UpdateSelection(start, end);
            }
        }
    }
}
