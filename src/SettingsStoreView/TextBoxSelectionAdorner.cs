// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace SettingsStoreView
{
    /// <summary>
    /// An adorner for text boxes that paints the selection even when it doesn't have focus.
    /// </summary>
    public class TextBoxSelectionAdorner : Adorner
    {
        public TextBoxSelectionAdorner(TextBox textBox) : base(textBox)
        {
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var textBox = (TextBox)AdornedElement;

            if (textBox.SelectionLength == 0)
            {
                // No selection.
                return;
            }

            if (textBox.IsKeyboardFocused)
            {
                // Let the text box draw its own selection adorner.
                return;
            }

            // We use ViewportWidth/Height to account for scroll bars.
            // TODO: The +4 is necessary because ViewportWidth ignores the margin between the right edge of text and the vertical scroll bar.
            // TODO: Figure out where this comes from and if it can be calculated.
            drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, textBox.ViewportWidth + 4, textBox.ViewportHeight)));

            int firstCharIndex = textBox.SelectionStart;
            int lastCharIndex = firstCharIndex + textBox.SelectionLength;
            var firstCharRect = textBox.GetRectFromCharacterIndex(firstCharIndex);
            var lastCharRect = textBox.GetRectFromCharacterIndex(lastCharIndex);

            // CONSIDER: Cache the geometry and invalidate it only when the selection changes.

            var highlightGeometry = new GeometryGroup();
            if (firstCharRect.Top == lastCharRect.Top)
            {
                // single line selection
                highlightGeometry.Children.Add(new RectangleGeometry(new Rect(firstCharRect.TopLeft, lastCharRect.BottomRight)));
            }
            else
            {
                int firstVisibleLine = textBox.GetFirstVisibleLineIndex();
                int lastVisibleLine = textBox.GetLastVisibleLineIndex();
                if (textBox.GetLineIndexFromCharacterIndex(firstCharIndex) < firstVisibleLine)
                {
                    firstCharIndex = textBox.GetCharacterIndexFromLineIndex(firstVisibleLine - 1);
                    firstCharRect = textBox.GetRectFromCharacterIndex(firstCharIndex);
                }
                if (textBox.GetLineIndexFromCharacterIndex(lastCharIndex) > lastVisibleLine)
                {
                    lastCharIndex = textBox.GetCharacterIndexFromLineIndex(lastVisibleLine + 1);
                    lastCharRect = textBox.GetRectFromCharacterIndex(lastCharIndex);
                }

                var lineHeight = firstCharRect.Height;
                var lineCount = (int)Math.Round((lastCharRect.Top - firstCharRect.Top) / lineHeight);
                var lineLeft = firstCharRect.Left;
                var lineTop = firstCharRect.Top;
                var currentCharIndex = firstCharIndex;
                for (int i = 0; i <= lineCount; i++)
                {
                    var lineIndex = textBox.GetLineIndexFromCharacterIndex(currentCharIndex);
                    var startOfLineCharIndex = textBox.GetCharacterIndexFromLineIndex(lineIndex);
                    var lineLength = textBox.GetLineLength(lineIndex);

                    var endOfLineCharIndex = startOfLineCharIndex + lineLength - 1;
                    if (endOfLineCharIndex > lastCharIndex)
                    {
                        endOfLineCharIndex = lastCharIndex;
                    }

                    var endOfLineCharRect = textBox.GetRectFromCharacterIndex(endOfLineCharIndex);
                    var lineWidth = endOfLineCharRect.Right - lineLeft;

                    if (i < lineCount)
                    {
                        // There's an adjustment (for padding?) for selection that extends over multiple lines
                        // TODO: I came up with this by emprical observation, but it would be nice to figure
                        // out where this comes from in the sources.
                        lineWidth += textBox.FontSize / 2;
                    }

                    if (Math.Round(lineWidth) <= 0)
                    {
                        lineWidth = 5;
                    }

                    highlightGeometry.Children.Add(new RectangleGeometry(new Rect(lineLeft, lineTop, lineWidth, lineHeight)));
                    currentCharIndex = startOfLineCharIndex + lineLength;
                    var nextLineFirstCharRect = textBox.GetRectFromCharacterIndex(currentCharIndex);
                    lineLeft = nextLineFirstCharRect.Left;
                    lineTop = nextLineFirstCharRect.Top;
                }
            }

            // The underlying text box uses an opacity of 0.4 to draw the selection highlight.
            // We use something slightly lower as a visual indication that we don't have keyboard
            // focus.
            drawingContext.PushOpacity(0.25);

            // TODO: Use a themed brush
            drawingContext.DrawGeometry(SystemColors.HighlightBrush, null, highlightGeometry);
        }
    }
}
