// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SettingsStoreView
{
    /// <summary>
    /// The data context object for the hex dump control. The contents are stored
    /// in a byte array.
    /// </summary>
    internal sealed class HexDumpControlDataContext : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The actual bytes.
        /// </summary>
        private byte[] _bytes;

        public HexDumpControlDataContext(ICollection<byte> bytes)
            => _bytes = bytes is byte[] array ? array : bytes.ToArray();

        public byte[] Bytes
        {
            get => _bytes;
            set
            {
                if (value != _bytes)
                {
                    _bytes = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
