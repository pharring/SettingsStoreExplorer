// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Windows.Data;

namespace SettingsStoreExplorer
{
    /// <summary>
    /// A boolean converter that returns true when the value equals the parameter, false otherwise.
    /// </summary>
    public sealed class GenericBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) 
            => value?.Equals(parameter);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
            => value?.Equals(true) == true ? parameter : Binding.DoNothing;
    }
}
