// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System;
using System.Windows.Input;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace SettingsStoreExplorer
{
    internal class ModifyPropertyValueCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter) => parameter is SettingsStoreProperty;

        public void Execute(object parameter)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Telemetry.Client.TrackEvent(nameof(ModifyPropertyValueCommand) + "." + nameof(Execute));

            if (!(parameter is SettingsStoreProperty property))
            {
                return;
            }

            if (!property.TryGetWritableSettingsStore(out var writableStore))
            {
                // Cannot get a writable setting store. The usual case is trying to modify a value under Config
                // TODO: Show a message? Run as admin?
                Telemetry.Client.TrackEvent("No writable store");
                return;
            }

            ErrorHandler.ThrowOnFailure(writableStore.PropertyExists(property.CollectionPath, property.Name, out var exists));
            if (exists == 0)
            {
                // Property has been deleted
                // TODO: Show a message
                Telemetry.Client.TrackEvent("Property deleted");
                return;
            }

            ShowModifyPropertyDialog(property, writableStore);
        }

        public static void ShowModifyPropertyDialog(SettingsStoreProperty property, IVsWritableSettingsStore writableStore)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            switch (property.Type)
            {
                case __VsSettingsType.SettingsType_String:
                    {
                        Telemetry.Client.TrackPageView(nameof(EditStringDialog));
                        var dialog = new EditStringDialog(property);
                        if (dialog.ShowModal() == true)
                        {
                            ErrorHandler.ThrowOnFailure(writableStore.SetString(property.CollectionPath, property.Name, (string)property.Value));
                        }
                    }
                    break;

                case __VsSettingsType.SettingsType_Int:
                    {
                        Telemetry.Client.TrackPageView(nameof(EditIntegerDialog) + "(32)");
                        var dialog = new EditIntegerDialog("Edit DWORD (32-bit) Value", DwordToStringConverter.Instance, property);
                        if (dialog.ShowModal() == true)
                        {
                            ErrorHandler.ThrowOnFailure(writableStore.SetUnsignedInt(property.CollectionPath, property.Name, (uint)property.Value));
                        }
                    }
                    break;

                case __VsSettingsType.SettingsType_Int64:
                    {
                        Telemetry.Client.TrackPageView(nameof(EditIntegerDialog) + "(64)");
                        var dialog = new EditIntegerDialog("Edit QWORD (64-bit) Value", QwordToStringConverter.Instance, property);
                        if (dialog.ShowModal() == true)
                        {
                            ErrorHandler.ThrowOnFailure(writableStore.SetUnsignedInt64(property.CollectionPath, property.Name, (ulong)property.Value));
                        }
                    }
                    break;

                case __VsSettingsType.SettingsType_Binary:
                    {
                        Telemetry.Client.TrackPageView(nameof(EditBinaryDialog));
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
