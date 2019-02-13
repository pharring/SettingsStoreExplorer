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

        public bool CanExecute(object parameter)
        {
            return parameter as SettingsStoreProperty != null;
        }

        public void Execute(object parameter)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var property = parameter as SettingsStoreProperty;
            if (property == null)
            {
                return;
            }

            if (!(ServiceProvider.GlobalProvider.GetService(typeof(SVsSettingsManager)) is IVsSettingsManager settingsManager))
            {
                return;
            }

            IVsWritableSettingsStore writableStore;
            if (ErrorHandler.Failed(settingsManager.GetWritableSettingsStore((uint)property.Root.EnclosingScope, out writableStore)))
            {
                // Cannot get a writable setting store. The usual case is trying to modify a value under Config
                // TODO: Show a message? Run as admin?
                return;
            }


            int exists;
            ErrorHandler.ThrowOnFailure(writableStore.PropertyExists(property.CollectionPath, property.Name, out exists));
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
