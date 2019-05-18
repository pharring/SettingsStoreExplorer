// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;

using Task = System.Threading.Tasks.Task;

// I believe it's safe to suppress the analyzer warning about accessing the settings
// store only from the main thread. We know that the settings store manager is
// implemented in .NET and it's a free-threaded object. That much means there's no
// deadlocking. In addition, it's implementation uses registry APIs which are
// thread-safe. And we're not modifying any data -- just querying. Switching to
// the UI thread would diminish the benefit of asynchronous expansion by tying
// up the UI thread.
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread

namespace SettingsStoreView
{
    /// <summary>
    /// Base class for all items in the settings store data-model.
    /// </summary>
    [DebuggerDisplay("Name = {Name} Path = {Path}")]
    public abstract class SettingsStoreItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected SettingsStoreItem(SettingsStoreSubCollection parent, string name)
        {
            Parent = parent;
            _name = name;
        }

        public SettingsStoreSubCollection Parent { get; }

        private static string CombinePaths(string path1, string path2) 
            => string.IsNullOrEmpty(path1) ? path2 : (path1 + @"\" + path2);

        /// <summary>
        /// Path to this settings collection, not including the root node.
        /// </summary>
        public string Path
        {
            get
            {
                if (Parent == null)
                {
                    // The root item doesn't participate in the path
                    return "";
                }

                return CombinePaths(Parent.Path, Name);
            }
        }

        /// <summary>
        /// Full path to this settings collection, including the root node.
        /// </summary>
        public string FullPath => Parent == null ? Name : CombinePaths(Parent.FullPath, Name);

        /// <summary>
        /// The root item. All items have a root (either "Config" or "User").
        /// </summary>
        public RootSettingsStore Root
        {
            get
            {
                SettingsStoreItem node;
                for (node = this; node.Parent != null; node = node.Parent)
                {
                }

                return node as RootSettingsStore;
            }
        }

        /// <summary>
        /// Backing field for <see cref="Name" />
        /// </summary>
        private string _name;

        /// <summary>
        /// The node's actual name in the settings store.
        /// </summary>
        public string Name
        {
            get => _name;
            private set
            {
                // Support for rename
                if (UpdateProperty(ref _name, value))
                {
                    NotifyPropertyChanged(nameof(Path));
                    NotifyPropertyChanged(nameof(FullPath));
                }
            }
        }

        /// <summary>
        /// Rename the item.
        /// </summary>
        /// <param name="newName"></param>
        public void Rename(string newName)
        {
            if (!CanRename)
            {
                throw new InvalidOperationException("Cannot rename this item.");
            }

            Name = newName;
        }

        /// <summary>
        /// Indicates whether the item can be renamed.
        /// </summary>
        protected virtual bool CanRename => true;

        /// <summary>
        /// Update the backing field of a property and fire notification if necessary.
        /// The default equality comparer is used to determine if the value has changed.
        /// </summary>
        /// <typeparam name="T">Type of the value being updated.</typeparam>
        /// <param name="field">The field to update.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="propertyName">The name of the property being updated.</param>
        /// <returns>true if the property was updated.</returns>
        protected bool UpdateProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            field = newValue;
            NotifyPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Notify listeners of a property changed. The notification may be
        /// posted asynchronously if the caller is not on the main thread.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (ThreadHelper.CheckAccess())
            {
                InvokePropertyChanged(propertyName);
            }
            else
            {
#pragma warning disable VSTHRD001 // Avoid legacy thread switching APIs
                ThreadHelper.Generic.BeginInvoke(() => InvokePropertyChanged(propertyName));
#pragma warning restore VSTHRD001 // Avoid legacy thread switching APIs
            }
        }

        /// <summary>
        /// Fire the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        private void InvokePropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// A property in the settings store.
    /// </summary>
    public sealed class SettingsStoreProperty : SettingsStoreItem
    {
        private SettingsStoreProperty(SettingsStoreSubCollection parent, string name, __VsSettingsType type, object value) : base(parent, name)
        {
            Type = type;
            _value = value;
        }

        public static SettingsStoreProperty CreateInstance(SettingsStoreSubCollection parent, string name, __VsSettingsType type)
        {
            var initialValue = GetInitialValue(parent, name, type);
            return new SettingsStoreProperty(parent, name, type, initialValue);
        }

        private static object GetInitialValue(SettingsStoreSubCollection parent, string name, __VsSettingsType type)
        {
            var store = parent.Root.SettingsStore;
            var path = parent.Path;

            switch (type)
            {
                case __VsSettingsType.SettingsType_String:
                    ErrorHandler.ThrowOnFailure(store.GetString(path, name, out var stringValue));
                    return stringValue;

                case __VsSettingsType.SettingsType_Int:
                    ErrorHandler.ThrowOnFailure(store.GetUnsignedInt(path, name, out var uintValue));
                    return uintValue;

                case __VsSettingsType.SettingsType_Int64:
                    ErrorHandler.ThrowOnFailure(store.GetUnsignedInt64(path, name, out var ulongValue));
                    return ulongValue;

                case __VsSettingsType.SettingsType_Binary:
                    uint[] actualByteLength = { 0 };
                    ErrorHandler.ThrowOnFailure(store.GetBinary(path, name, 0, null, actualByteLength));
                    byte[] binaryValue = new byte[actualByteLength[0]];
                    ErrorHandler.ThrowOnFailure(store.GetBinary(path, name, actualByteLength[0], binaryValue, actualByteLength));
                    return binaryValue;

                default:
                    return null;
            }
        }

        public __VsSettingsType Type { get; }

        private object _value;
        public object Value { get => _value; set => UpdateProperty(ref _value, value); }
        public string CollectionPath => Parent.Path;
    }

    /// <summary>
    /// A sub-collection in the settings store.
    /// </summary>
    public class SettingsStoreSubCollection : SettingsStoreItem
    {
        private ObservableCollection<SettingsStoreSubCollection> _subCollections;
        private ObservableCollection<SettingsStoreProperty> _properties;

        public SettingsStoreSubCollection(SettingsStoreSubCollection parent, string name) : base(parent, name)
        {
        }

        public ObservableCollection<SettingsStoreSubCollection> SubCollections => _subCollections ?? (_subCollections = new ObservableCollection<SettingsStoreSubCollection>(CreateSubCollections()));

        public ObservableCollection<SettingsStoreProperty> Properties => _properties ?? (_properties = new ObservableCollection<SettingsStoreProperty>(InitializeProperties()));

        private IEnumerable<SettingsStoreProperty> InitializeProperties()
        {
            Task.Run(() => PopulatePropertiesAsync()).Forget();
            return Array.Empty<SettingsStoreProperty>();
        }

        protected virtual IEnumerable<SettingsStoreSubCollection> CreateSubCollections()
        {
            // We don't need to know the full collection yet, just whether the collection
            // is empty or not so that the tree view knows whether to display the expander.
            var store = Root.SettingsStore;

            // Don't use GetSubCollectionCount because it's essentially a full enumeration
            // which defeats this optimization.
            if (ErrorHandler.Succeeded(store.GetSubCollectionName(Path, 0u, out _)))
            {
                // Create a single placeholder that knows how to expand its parent.
                yield return new SettingsStoreSubCollectionPlaceholder(this);
            }
        }

        private async Task ExpandSubCollectionAsync()
        {
            var store = Root.SettingsStore;
            var path = Path;

            // Don't get the count up-front. It's essentially an enumeration which is as expensive
            // as just looping until we get an error.

            var subCollections = new List<SettingsStoreSubCollection>();
            for (uint index = 0; ; index++)
            {
                if (ErrorHandler.Failed(store.GetSubCollectionName(path, index, out string subCollectionName)))
                {
                    break;
                }

                subCollections.Add(new SettingsStoreSubCollection(this, subCollectionName));
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            UpdateSubCollections(subCollections);
        }

        private void UpdateSubCollections(IEnumerable<SettingsStoreSubCollection> subCollections)
        {
            // Subtle: If you've just added a new collection on a node that wasn't yet expanded, then the _subCollections list
            // will contain a seleceted item n item that is selected and we want to keep that way. We should be careful not
            // to destroy it, otherwise selection will be lost.

            // First remove the placeholder. If it's there, it'll always be the first one.
            if (_subCollections.Count > 0 && _subCollections[0] is SettingsStoreSubCollectionPlaceholder)
            {
                _subCollections.RemoveAt(0);
            }

            // If there's still one left, then we have to preserve it.
            if (_subCollections.Count == 1)
            {
                var preservedItemPath = _subCollections[0].Path;
                bool foundSelected = false;

                foreach (var subCollection in subCollections)
                {
                    if (foundSelected)
                    {
                        // Add to end of the list.
                        _subCollections.Add(subCollection);
                    }
                    else if (preservedItemPath.Equals(subCollection.Path, StringComparison.Ordinal))
                    {
                        foundSelected = true;
                    }
                    else
                    {
                        // Insert before the preserved item.
                        _subCollections.Insert(_subCollections.Count - 1, subCollection);
                    }
                }
            }
            else
            {
                _subCollections.AddRange(subCollections);
            }
        }

        private async Task PopulatePropertiesAsync()
        {
            var store = Root.SettingsStore;
            var path = Path;

            var properties = new List<SettingsStoreProperty>();
            for (uint index = 0; ; index++)
            {
                if (ErrorHandler.Failed(store.GetPropertyName(path, index, out var name)))
                {
                    break;

                }

                if (ErrorHandler.Failed(store.GetPropertyType(path, name, out var type)))
                {
                    break;
                }

                properties.Add(SettingsStoreProperty.CreateInstance(this, name, (__VsSettingsType)type));
            }

            properties.Sort((p, q) => StringComparer.OrdinalIgnoreCase.Compare(p.Name, q.Name));

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            _properties.ReplaceAll(properties);
        }

        /// <summary>
        /// A place-holder for an unexpanded sub-collection. Initially it has just one
        /// item but, when the <see cref="CreateSubCollections"/> method is called,
        /// the sub-collection is replaced, asynchronously, with the real value.
        /// </summary>
        private sealed class SettingsStoreSubCollectionPlaceholder : SettingsStoreSubCollection
        {
            public SettingsStoreSubCollectionPlaceholder(SettingsStoreSubCollection parent) : base(parent, "Please wait...")
            {
            }

            /// <summary>
            /// When WPF really needs to know the SubCollections (because we expanded the node
            /// and it's bound to the name), this is our time to replace the placeholder with
            /// real values.
            /// </summary>
            /// <returns>An empty enumeration, always. The parent's subcollection is expanded
            /// in the background.</returns>
            protected override IEnumerable<SettingsStoreSubCollection> CreateSubCollections()
            {
                Task.Run(() => Parent.ExpandSubCollectionAsync()).Forget();
                return Array.Empty<SettingsStoreSubCollection>();
            }

            protected override bool CanRename => false;
        }
    }

    /// <summary>
    /// A root node in the settings store.
    /// </summary>
    [DebuggerDisplay("Name = {Name} Root")]
    public sealed class RootSettingsStore : SettingsStoreSubCollection
    {
        public RootSettingsStore(IVsSettingsManager settingsManager, __VsEnclosingScopes enclosingScope, string name) : base(null, name)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            EnclosingScope = enclosingScope;
            ErrorHandler.ThrowOnFailure(settingsManager.GetReadOnlySettingsStore((uint)enclosingScope, out var settingsStore));
            SettingsStore = settingsStore;
        }

        public __VsEnclosingScopes EnclosingScope { get; }

        public IVsSettingsStore SettingsStore { get; }

        protected override bool CanRename => false; 
    }

    /// <summary>
    /// The view-model for the settings store. Represents the "Config" and "User" trees.
    /// </summary>
    public sealed class SettingsStoreViewModel
    {
        public SettingsStoreSubCollection[] Root { get; set; }

        public SettingsStoreViewModel(IVsSettingsManager settingsManager)
        {
            Root = new SettingsStoreSubCollection[] {
                new RootSettingsStore(settingsManager, __VsEnclosingScopes.EnclosingScopes_Configuration, "Config"),
                new RootSettingsStore(settingsManager, __VsEnclosingScopes.EnclosingScopes_UserSettings, "User")
            };
        }
    }
}
