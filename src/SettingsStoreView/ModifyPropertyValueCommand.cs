// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System;
using System.Windows.Input;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace SettingsStoreView
{
    internal class ModifyPropertyValueCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter) => parameter as SettingsStoreProperty != null;

        public void Execute(object parameter)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!(parameter is SettingsStoreProperty property))
            {
                return;
            }

            if (!(ServiceProvider.GlobalProvider.GetService(typeof(SVsSettingsManager)) is IVsSettingsManager settingsManager))
            {
                return;
            }

            if (ErrorHandler.Failed(settingsManager.GetWritableSettingsStore((uint)property.Root.EnclosingScope, out var writableStore)))
            {
                // Cannot get a writable setting store. The usual case is trying to modify a value under Config
                // TODO: Show a message? Run as admin?
                return;
            }


            ErrorHandler.ThrowOnFailure(writableStore.PropertyExists(property.CollectionPath, property.Name, out var exists));
            if (exists == 0)
            {
                // Property has been deleted
                // TODO: Show a message
                return;
            }

            switch (property.Type)
            {
                case  __VsSettingsType.SettingsType_String:
                    {
                        var dialog = new EditStringDialog(property);
                        if (dialog.ShowModal() == true)
                        {
                            ErrorHandler.ThrowOnFailure(writableStore.SetString(property.CollectionPath, property.Name, (string)property.Value));
                        }
                    }
                    break;

                case __VsSettingsType.SettingsType_Int:
                    {
                        var dialog = new EditIntegerDialog("Edit DWORD (32-bit) Value", DwordToStringConverter.Instance, property);
                        if (dialog.ShowModal() == true)
                        {
                            ErrorHandler.ThrowOnFailure(writableStore.SetUnsignedInt(property.CollectionPath, property.Name, (uint)property.Value));
                        }
                    }
                    break;

                case __VsSettingsType.SettingsType_Int64:
                    {
                        var dialog = new EditIntegerDialog("Edit QWORD (64-bit) Value", QwordToStringConverter.Instance, property);
                        if (dialog.ShowModal() == true)
                        {
                            ErrorHandler.ThrowOnFailure(writableStore.SetUnsignedInt64(property.CollectionPath, property.Name, (ulong)property.Value));
                        }
                    }
                    break;

                case __VsSettingsType.SettingsType_Binary:
                    {
                        var dialog = new EditBinaryDialog(property);
                        if (dialog.ShowModal() == true)
                        {
                            var binary = (byte[])property.Value;
                            ErrorHandler.ThrowOnFailure(writableStore.SetBinary(property.CollectionPath, property.Name, (uint)binary.Length, binary));
                        }
                    }
                    break;

                default:
                    break;
            }
        }
    }
}
