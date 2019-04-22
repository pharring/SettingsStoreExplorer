// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SettingsStoreView
{
    public sealed class HandleEnterBehavior
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
            "Command", typeof(ICommand), typeof(HandleEnterBehavior), new PropertyMetadata(default(ICommand), OnComandChanged));

        public static void SetCommand(DependencyObject element, ICommand value) => element.SetValue(CommandProperty, value);

        public static ICommand GetCommand(DependencyObject element) => (ICommand)element.GetValue(CommandProperty);

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.RegisterAttached(
            "CommandParameter", typeof(object), typeof(HandleEnterBehavior), new PropertyMetadata(default(object)));

        public static void SetCommandParameter(DependencyObject element, object value) => element.SetValue(CommandParameterProperty, value);

        public static object GetCommandParameter(DependencyObject element) => element.GetValue(CommandParameterProperty);

        private static void OnComandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Control c))
            {
                throw new InvalidOperationException($"can only be attached to {nameof(Control)}");
            }

            c.KeyDown -= OnKeyDown;
            if (GetCommand(c) != null)
            {
                c.KeyDown += OnKeyDown;
            }
        }

        private static void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            if (!(sender is DependencyObject d))
            {
                return;
            }

            var command = GetCommand(d);
            if (command == null)
            {
                return;
            }

            var parameter = GetCommandParameter(d);
            if (command.CanExecute(parameter))
            {
                command.Execute(parameter);
            }
        }
    }
}