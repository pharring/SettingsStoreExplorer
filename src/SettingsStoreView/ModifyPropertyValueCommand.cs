// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System;
using System.Windows.Input;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace SettingsStoreView
{
    internal sealed class ModifyPropertyValueCommand : ICommand
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

            IVsWritableSettingsStore store;
            if (ErrorHandler.Failed(settingsManager.GetWritableSettingsStore((uint)property.Root.EnclosingScope, out store)))
            {
                // Cannot get a writable setting store. The usual case is trying to modify a value under Config
                // TODO: Show a message?
                return;
            }


            int exists;
            ErrorHandler.ThrowOnFailure(store.PropertyExists(property.CollectionPath, property.Name, out exists));
            if (exists == 0)
            {
                // Property has been deleted
                // TODO: Show a message
                return;
            }

            // TODO: Modal dlg
        }
    }
}
