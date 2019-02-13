// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SettingsStoreView
{
    /// <summary>
    /// Interaction logic for EditBinaryDialog.xaml
    /// </summary>
    public partial class EditBinaryDialog : DialogWindow
    {
        private readonly SettingsStoreProperty _property;

        private class NameValue
        {
            public NameValue(string name, byte[] value)
            {
                Name = name;
                Value = value;
            }

            public string Name { get; }
            public byte[] Value { get; }
        }

        public EditBinaryDialog(SettingsStoreProperty property)
        {
            _property = property;
            InitializeComponent();
            DataContext = new NameValue(property.Name, (byte[])property.Value);
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow entering of hex digits only
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            _property.Value = ((NameValue)DataContext).Value;
            Close();
        }
    }
}
