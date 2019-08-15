// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Windows;

namespace SettingsStoreExplorer
{
    /// <summary>
    /// Interaction logic for EditStringDialog.xaml
    /// </summary>
    public partial class EditStringDialog : DialogWindow
    {
        private readonly SettingsStoreProperty _property;

        public EditStringDialog(SettingsStoreProperty property)
        {
            _property = property;
            InitializeComponent();
            DataContext = new { property.Name, property.Value };
        }

        protected override void OnActivated(EventArgs e)
        {
            ValueTextBox.SelectAll();
            base.OnActivated(e);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            _property.Value = ValueTextBox.Text;
            Close();
        }
    }
}
