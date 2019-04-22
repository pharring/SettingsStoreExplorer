// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SettingsStoreView
{
    internal sealed class HexDumpAddressTextBox : TextBox
    {
        public const int BytesPerRow = 8;

        public HexDumpAddressTextBox()
        {
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is HexDumpControlDataContext oldDataContext)
            {
                oldDataContext.PropertyChanged -= BytesChanged;
            }

            if (e.NewValue is HexDumpControlDataContext newDataContext)
            {
                newDataContext.PropertyChanged += BytesChanged;
                UpdateContent(newDataContext.Bytes);
            }
        }

        private void BytesChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(HexDumpControlDataContext.Bytes))
            {
                var dataContext = (HexDumpControlDataContext)sender;
                UpdateContent(dataContext.Bytes);
            }
        }

        /// <summary>
        /// Refresh the content of the text box when the data context changes.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        private void UpdateContent(ICollection<byte> bytes)
        {
            var text = BuildContent(bytes);
            Text = text;
        }

        private string BuildContent(ICollection<byte> bytes)
        {
            var sb = new StringBuilder();

            // Intentionally using <= here so we get a placeholder
            // for otherwise blank lines.
            for (int x = 0; x <= bytes.Count; x += BytesPerRow)
            { 
                sb.AppendLine(x.ToString("X4"));
            }

            return sb.ToString();
        }
    }
}
