// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using Microsoft.VisualStudio.PlatformUI;
using System.Windows;

namespace SettingsStoreView
{
    /// <summary>
    /// Interaction logic for EditBinaryDialog.xaml
    /// </summary>
    public partial class EditBinaryDialog : DialogWindow
    {
        private readonly SettingsStoreProperty _property;

        public EditBinaryDialog(SettingsStoreProperty property)
        {
            _property = property;
            InitializeComponent();
            DataContext = new { property.Name, property.Value };
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            _property.Value = ValueTextBox.EditedValue;
            Close();
        }
    }
}
