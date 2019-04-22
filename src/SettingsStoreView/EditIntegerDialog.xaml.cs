// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace SettingsStoreView
{
    /// <summary>
    /// Interaction logic for EditIntegerDialog.xaml
    /// </summary>
    public partial class EditIntegerDialog : DialogWindow
    {
        private readonly SettingsStoreProperty _property;

        public EditIntegerDialog(string title, IntegerToStringConverter converter, SettingsStoreProperty property)
        {
            Title = title;
            _converter = converter;
            _property = property;
            InitializeComponent();
            DataContext = new { property.Name, Value = _converter.ToString(property.Value, ValueFormat, CultureInfo.CurrentUICulture) };
        }

        /// <summary>
        /// The format of the number. Hexadecimal or Decimal.
        /// </summary>
        public NumberStyles ValueFormat
        {
            get => (NumberStyles)GetValue(ValueFormatProperty);
            set => SetValue(ValueFormatProperty, value);
        }

        // Using a DependencyProperty as the backing store for ValueFormat.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueFormatProperty =
            DependencyProperty.Register(nameof(ValueFormat), typeof(NumberStyles), typeof(EditIntegerDialog), new PropertyMetadata(NumberStyles.HexNumber, OnValueFormatChanged));

        private static void OnValueFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((EditIntegerDialog)d).OnValueFormatChanged((NumberStyles)e.OldValue, (NumberStyles)e.NewValue);

        private void OnValueFormatChanged(NumberStyles oldFormat, NumberStyles newFormat)
        {
            var text = ValueTextBox.Text;
            var oldValue = _converter.Parse(text, oldFormat, CultureInfo.CurrentUICulture);
            var newText = _converter.ToString(oldValue, newFormat, CultureInfo.CurrentUICulture);
            ValueTextBox.Text = newText;
        }

        protected override void OnActivated(EventArgs e)
        {
            ValueTextBox.SelectAll();
            base.OnActivated(e);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            _property.Value = _converter.Parse(ValueTextBox.Text, ValueFormat, CultureInfo.CurrentUICulture);
            Close();
        }

        private void OnPreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            var text = textBox.Text;
            if (textBox.SelectionLength > 0)
            {
                text.Remove(textBox.SelectionStart, textBox.SelectionLength);
            }

            text = text.Insert(textBox.SelectionStart, e.Text);

            // If the value doesn't parse, then swallow the edit (say we handled it).
            e.Handled = !_converter.TryParse(text, ValueFormat, CultureInfo.CurrentUICulture, out _);
        }

        private readonly IntegerToStringConverter _converter;
    }
}
