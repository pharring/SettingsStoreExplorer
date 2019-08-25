// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using static SettingsStoreExplorer.SettingsStoreCommandSet;

namespace SettingsStoreExplorer
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid(c_toolWindowGuidString)]
    public class SettingsStoreExplorerToolWindow : ToolWindowPane
    {
        internal const string c_toolWindowGuidString = "f24ec500-28a5-4f29-82da-4e7d307f9d63";

        private static readonly char[] s_invalidCollectionNameChars = new[] { '\\' };
        private readonly MenuCommand _addNewSubCollectionCommand;
        private readonly MenuCommand _addNewStringValueCommand;
        private readonly MenuCommand _addNewDWORDValueCommand;
        private readonly MenuCommand _addNewQWORDValueCommand;
        private readonly MenuCommand _addNewBinaryValueCommand;
        private readonly MenuCommand _renameCommand;
        private readonly MenuCommand _deleteCommand;
        private readonly MenuCommand _modifyCommand;
        private readonly MenuCommand _refreshCommand;
        private SettingsStoreExplorerToolWindowControl _control;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsStoreExplorerToolWindow"/> class.
        /// </summary>
        public SettingsStoreExplorerToolWindow() : base(null)
        {
            Caption = VSPackage.ToolWindowCaption;
            BitmapImageMoniker = KnownMonikers.SettingsStoreExplorer;

            _addNewSubCollectionCommand = new MenuCommand(AddNewSubCollectionExecuted, AddNewSubCollectionCommandId);
            _addNewStringValueCommand = new MenuCommand(AddNewStringValueExecuted, AddNewStringValueCommandId);
            _addNewDWORDValueCommand = new MenuCommand(AddNewDWORDValueExecuted, AddNewDWORDValueCommandId);
            _addNewQWORDValueCommand = new MenuCommand(AddNewQWORDValueExecuted, AddNewQWORDValueCommandId);
            _addNewBinaryValueCommand = new MenuCommand(AddNewBinaryValueExecuted, AddNewBinaryValueCommandId);
            _renameCommand = new MenuCommand(RenameExecuted, RenameCommandId);
            _deleteCommand = new MenuCommand(DeleteExecuted, DeleteCommandId);
            _modifyCommand = new MenuCommand(ModifyExecuted, ModifyCommandId);
            _refreshCommand = new MenuCommand(RefreshExecuted, RefreshCommandId);
        }

        protected override void Initialize()
        {
            base.Initialize();

            var commandService = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            commandService.AddCommand(_addNewSubCollectionCommand);
            commandService.AddCommand(_addNewStringValueCommand);
            commandService.AddCommand(_addNewDWORDValueCommand);
            commandService.AddCommand(_addNewQWORDValueCommand);
            commandService.AddCommand(_addNewBinaryValueCommand);
            commandService.AddCommand(_renameCommand);
            commandService.AddCommand(_deleteCommand);
            commandService.AddCommand(_modifyCommand);
            commandService.AddCommand(_refreshCommand);

            Content = _control = new SettingsStoreExplorerToolWindowControl(this);

            _control.treeView.SelectedItemChanged += TreeView_SelectedItemChanged;
            _control.listView.SelectionChanged += ListView_SelectionChanged;

            KnownUIContexts.ShellInitializedContext.WhenActivated(InitializeViewModel);
        }

        /// <summary>
        /// The SVsSettingsPersistenceManager class
        /// </summary>
        [Guid("9B164E40-C3A2-4363-9BC5-EB4039DEF653")]
        private static class SVsSettingsPersistenceManager { }

        private void InitializeViewModel()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var vsSettingsManager = GetService(typeof(SVsSettingsManager)) as IVsSettingsManager;
            var roamingSettingsManager = GetService(typeof(SVsSettingsPersistenceManager)) as ISettingsManager;
            _control.InitializeViewModel(vsSettingsManager, roamingSettingsManager);

            Telemetry.Client.TrackPageView(nameof(SettingsStoreExplorerToolWindow));
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var isWritable = e.NewValue is SettingsStoreSubCollection subCollection && subCollection.TryGetWritableSettingsStore(out _);
            var isRoot = e.NewValue is RootSettingsStore;
            _addNewBinaryValueCommand.Enabled = isWritable;
            _addNewQWORDValueCommand.Enabled = isWritable;
            _addNewDWORDValueCommand.Enabled = isWritable;
            _addNewStringValueCommand.Enabled = isWritable;
            _addNewSubCollectionCommand.Enabled = isWritable;
            _renameCommand.Enabled = isWritable && !isRoot;
            _deleteCommand.Enabled = isWritable && !isRoot;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                return;
            }

            var isWritable = e.AddedItems[0] is SettingsStoreProperty property && property.TryGetWritableSettingsStore(out _);
            _modifyCommand.Enabled = isWritable;
            _renameCommand.Enabled = isWritable;
            _deleteCommand.Enabled = isWritable;
        }

        private SettingsStoreSubCollection GetSelectedSubCollection()
            => _control.treeView.SelectedItem as SettingsStoreSubCollection;

        private SettingsStoreProperty GetSelectedProperty()
            => _control.listView.SelectedItem as SettingsStoreProperty;

        private void AddNewSubCollectionExecuted(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var subCollection = GetSelectedSubCollection();
            if (subCollection != null && subCollection.TryGetWritableSettingsStore(out var settingsStore))
            {
                var newCollection = subCollection.GenerateNewSubCollection();

                if (ErrorHandler.Succeeded(settingsStore.CreateCollection(newCollection.Path)))
                {
                    // Update the view model.
                    subCollection.SubCollections.Add(newCollection);
                    _control.SetTreeViewSelection(newCollection);
                    Telemetry.Client.TrackEvent("AddNewSubCollection");

                    InPlaceEditSubCollectionName(newCollection, settingsStore);
                }
            }
        }

        private void AddNewStringValueExecuted(object sender, EventArgs e)
        {
            AddNewValueHelper((settingsStore, name, path) =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return settingsStore.SetString(path, name, "");
            });
        }

        private void AddNewDWORDValueExecuted(object sender, EventArgs e)
        {
            AddNewValueHelper((settingsStore, name, path) =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return settingsStore.SetInt(path, name, 0);
            });
        }

        private void AddNewQWORDValueExecuted(object sender, EventArgs e)
        {
            AddNewValueHelper((settingsStore, name, path) =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return settingsStore.SetInt64(path, name, 0);
            });
        }

        private void AddNewBinaryValueExecuted(object sender, EventArgs e)
        {
            AddNewValueHelper((settingsStore, name, path) =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return settingsStore.SetBinary(path, name, 0u, Array.Empty<byte>());
            });
        }

        private void AddNewValueHelper(Func<IVsWritableSettingsStore, string, string, int> setter)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var subCollection = GetSelectedSubCollection();
            if (subCollection != null && subCollection.TryGetWritableSettingsStore(out var settingsStore))
            {
                var newPropertyName = subCollection.GenerateNewPropertyName();
                if (ErrorHandler.Succeeded(setter(settingsStore, newPropertyName, subCollection.Path)))
                {
                    var type = settingsStore.GetPropertyType(subCollection.Path, newPropertyName);

                    // Update the view model.
                    var newProperty = SettingsStoreProperty.CreateInstance(subCollection, newPropertyName, type);
                    subCollection.Properties.Add(newProperty);

                    // Move focus to the newly-created value and start in-place editing of the name.
                    _control.SetListViewSelection(newProperty);

                    Telemetry.Client.TrackEvent("AddNewProperty", new Dictionary<string, string> { ["Type"] = type.ToString() });

                    InPlaceEditPropertyName(newProperty, settingsStore);
                }
            }
        }

        private void RenameExecuted(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_control.listView.IsKeyboardFocusWithin)
            {
                RenamePropertyExecuted();
            }
            else if (_control.treeView.IsKeyboardFocusWithin)
            {
                RenameCollectionExecuted();
            }
        }

        private void RenameCollectionExecuted()
        { 
            ThreadHelper.ThrowIfNotOnUIThread();
            var subCollection = GetSelectedSubCollection();
            if (subCollection == null || subCollection is RootSettingsStore)
            {
                return;
            }

            if (!subCollection.TryGetWritableSettingsStore(out var settingsStore))
            {
                return;
            }

            InPlaceEditSubCollectionName(subCollection, settingsStore);
        }

        private void InPlaceEditSubCollectionName(SettingsStoreSubCollection subCollection, IVsWritableSettingsStore settingsStore)
        {
            _control.InPlaceEditTreeViewItem(subCollection, subCollection.Name, newName =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                if (newName.Length == 0)
                {
                    var uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
                    ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(0, Guid.Empty, "Error renaming collection", $"A collection name cannot be blank. Try again with a different name.", null, 0, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, OLEMSGICON.OLEMSGICON_CRITICAL, 0, out int result));
                    return;
                }

                if (newName.IndexOfAny(s_invalidCollectionNameChars) >= 0)
                {
                    var uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
                    ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(0, Guid.Empty, "Error renaming collection", $"A collection name cannot contain a backslash character (\\). Try again with a different name.", null, 0, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, OLEMSGICON.OLEMSGICON_CRITICAL, 0, out int result));
                    return;
                }

                // Create a sibling and check for duplicate names.
                var parent = subCollection.Parent;
                var renamedSubCollection = new SettingsStoreSubCollection(parent, newName);
                if (settingsStore.CollectionExists(renamedSubCollection.Path))
                {
                    var uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
                    ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(0, Guid.Empty, "Error renaming collection", $"There is already a collection called '{newName}'. Try again with a different name.", null, 0, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, OLEMSGICON.OLEMSGICON_CRITICAL, 0, out int result));
                    return;
                }

                // Clone and recreate the entire tree beneath this collection and then delete the original.
                ErrorHandler.ThrowOnFailure(settingsStore.CreateCollection(renamedSubCollection.Path));
                settingsStore.CopyTree(subCollection, renamedSubCollection);
                ErrorHandler.ThrowOnFailure(settingsStore.DeleteCollection(subCollection.Path));

                // Update the view model.
                subCollection.Rename(newName);

                // Select the newly-renamed sub-collection.
                _control.SetTreeViewSelection(subCollection);

                Telemetry.Client.TrackEvent("RenameSubCollection");
            });
        }

        private void DeleteExecuted(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_control.listView.IsKeyboardFocusWithin)
            {
                DeletePropertyExecuted();
            }
            else if (_control.treeView.IsKeyboardFocusWithin)
            {
                DeleteCollectionExecuted();
            }
        }

        private void DeleteCollectionExecuted()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var subCollection = GetSelectedSubCollection();
            if (subCollection == null || subCollection is RootSettingsStore)
            {
                return;
            }

            if (!subCollection.TryGetWritableSettingsStore(out var settingsStore))
            {
                return;
            }

            var uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(0, Guid.Empty, "Confirm Collection Delete", $"Are you sure you want to permanently delete '{subCollection.Name}' and all its subcollections?", null, 0, OLEMSGBUTTON.OLEMSGBUTTON_YESNO, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, OLEMSGICON.OLEMSGICON_WARNING, 0, out int result));
            if (result != (int)MessageBoxResult.Yes)
            {
                return;
            }

            var parent = subCollection.Parent;
            ErrorHandler.ThrowOnFailure(settingsStore.DeleteCollection(subCollection.Path));

            // Update the view model.
            parent.SubCollections.Remove(subCollection);

            Telemetry.Client.TrackEvent("DeleteSubCollection");
        }

        private void DeletePropertyExecuted()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var property = GetSelectedProperty();
            if (property == null)
            {
                return;
            }

            if (!property.TryGetWritableSettingsStore(out var settingsStore))
            {
                return;
            }

            var uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(0, Guid.Empty, "Confirm Property Delete", $"Are you sure you want to permanently delete '{property.Name}'?", null, 0, OLEMSGBUTTON.OLEMSGBUTTON_YESNO, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, OLEMSGICON.OLEMSGICON_WARNING, 0, out int result));
            if (result != (int)MessageBoxResult.Yes)
            {
                return;
            }

            ErrorHandler.ThrowOnFailure(settingsStore.DeleteProperty(property.CollectionPath, property.Name));

            // Update the view model.
            property.Parent.Properties.Remove(property);

            Telemetry.Client.TrackEvent("DeleteProperty");
        }

        private void RenamePropertyExecuted()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var property = GetSelectedProperty();
            if (property == null)
            {
                return;
            }

            if (!property.TryGetWritableSettingsStore(out var settingsStore))
            {
                return;
            }

            InPlaceEditPropertyName(property, settingsStore);
        }

        private void InPlaceEditPropertyName(SettingsStoreProperty property, IVsWritableSettingsStore settingsStore)
        {
            _control.InPlaceEditListViewItem(property, property.Name, newName =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                // Allow renaming to blank to support the (Default) value, but only for strings.
                if (newName.Length == 0 && property.Type != __VsSettingsType.SettingsType_String)
                {
                    var uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
                    ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(0, Guid.Empty, "Error renaming property", "Only a string property may have a blank name. Try again with a different name.", null, 0, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, OLEMSGICON.OLEMSGICON_CRITICAL, 0, out int result));
                    return;
                }

                // Check for duplicate names.
                if (settingsStore.PropertyExists(property.CollectionPath, newName))
                {
                    var uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
                    var message = newName.Length == 0 ?
                        "There is already a (Default) property. Delete it first and try again or use a different name." :
                        $"There is already a property called '{newName}'. Try again with a different name.";

                    ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(0, Guid.Empty, "Error renaming property", message, null, 0, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, OLEMSGICON.OLEMSGICON_CRITICAL, 0, out int result));
                    return;
                }

                // Clone the property then delete the original.
                settingsStore.CopyProperty(property, newName);
                ErrorHandler.ThrowOnFailure(settingsStore.DeleteProperty(property.CollectionPath, property.Name));

                // Update the view model (keeping the selection the same)
                property.Rename(newName);

                Telemetry.Client.TrackEvent("RenameProperty");
            });
        }

        private void ModifyExecuted(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var property = GetSelectedProperty();
            if (property == null)
            {
                return;
            }

            if (!property.TryGetWritableSettingsStore(out var settingsStore))
            {
                return;
            }

            ModifyPropertyValueCommand.ShowModifyPropertyDialog(property, settingsStore);
        }

        private void RefreshExecuted(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var fullPath = GetSelectedSubCollection().FullPath;

            InitializeViewModel();

            var model = _control.DataContext as SettingsStoreViewModel;
            model.RequestExpansion(fullPath, subCollection =>
            {
                _control.SetTreeViewSelection(subCollection);
            });

            Telemetry.Client.TrackEvent("Refresh");
        }
    }
}
