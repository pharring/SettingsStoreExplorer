// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
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
    [DebuggerDisplay("Name = {Name} Path = {Path}")]
    public abstract class SettingsStoreItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected SettingsStoreItem(SettingsStoreSubCollection parent, string name)
        {
            Parent = parent;
            Name = name;
        }

        protected SettingsStoreSubCollection Parent { get; }

        private static string CombinePaths(string path1, string path2)
        {
            return string.IsNullOrEmpty(path1) ? path2 : (path1 + @"\" + path2);
        }

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
        public string FullPath
        {
            get
            {
                if (Parent == null)
                {
                    return Name;
                }

                return CombinePaths(Parent.FullPath, Name);
            }
        }

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
        /// The node's actual name in the settings store.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Update the backing field of a property and fire notification if necessary.
        /// The default equality comparer is used to determine if the value has changed.
        /// </summary>
        /// <typeparam name="T">Type of the value being updated.</typeparam>
        /// <param name="field">The field to update.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="propertyName">The name of the property being updated.</param>
        protected void UpdateProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, newValue))
            {
                field = newValue;
                NotifyPropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// Fire the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        private void NotifyPropertyChanged(string propertyName)
        {
            if (ThreadHelper.CheckAccess())
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            else
            {
#pragma warning disable VSTHRD001 // Avoid legacy thread switching APIs
                ThreadHelper.Generic.BeginInvoke(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
#pragma warning restore VSTHRD001 // Avoid legacy thread switching APIs
            }
        }
    }

    public sealed class SettingsStoreProperty : SettingsStoreItem
    {
        public SettingsStoreProperty(SettingsStoreSubCollection parent, string name) : base(parent, name)
        {
        }

        public __VsSettingsType Type { get; set; }
        public object Value { get; set; }
        public string CollectionPath => Parent.Path;
    }

    public class SettingsStoreSubCollection : SettingsStoreItem
    {
        private SettingsStoreSubCollection[] _subCollections;
        private SettingsStoreProperty[] _properties;

        public SettingsStoreSubCollection(SettingsStoreSubCollection parent, string name) : base(parent, name)
        {
        }

        public SettingsStoreSubCollection[] SubCollections => _subCollections ?? (_subCollections = CreateSubCollections());

        public SettingsStoreProperty[] Properties => _properties ?? (_properties = PopulateProperties());

        protected virtual SettingsStoreSubCollection[] CreateSubCollections()
        {
            // We don't need to know the full collection yet, just whether the collection
            // is empty or not so that the tree view knows whether to display the expander.
            var store = Root.SettingsStore;

            // Don't use GetSubCollectionCount because it's essentially a full enumeration
            // which defeats this optimization.
            if (ErrorHandler.Failed(store.GetSubCollectionName(Path, 0u, out string subCollectionName)))
            {
                return Array.Empty<SettingsStoreSubCollection>();
            }

            // Create an array with a single placeholder that knows how to expand its
            // parent.
            return new[] { new SettingsStoreSubCollectionPlaceholder(this) };
        }

        private void ExpandSubCollection()
        {
            var store = Root.SettingsStore;

            // Don't get the count up-front. It's essentially an enumeration which is as expensive
            // as just looping until we get an error.

            var subCollections = new List<SettingsStoreSubCollection>();

            var path = Path;
            for (uint i = 0; ; i++)
            {
                if (ErrorHandler.Failed(store.GetSubCollectionName(path, i, out string subCollectionName)))
                {
                    break;
                }

                subCollections.Add(new SettingsStoreSubCollection(this, subCollectionName));
            }

            UpdateProperty(ref _subCollections, subCollections.ToArray(), nameof(SubCollections));
        }

        private SettingsStoreProperty[] PopulateProperties()
        {
            Task.Run(() =>
            {
                var store = Root.SettingsStore;
                uint propertyCount;
                ErrorHandler.ThrowOnFailure(store.GetPropertyCount(Path, out propertyCount));

                var properties = new SettingsStoreProperty[propertyCount];
                for (uint i = 0; i < propertyCount; i++)
                {
                    properties[i] = CreateProperty(store, i);
                }

                UpdateProperty(ref _properties, properties, nameof(Properties));
            }).Forget();

            return Array.Empty<SettingsStoreProperty>();
        }

        private SettingsStoreProperty CreateProperty(IVsSettingsStore store, uint index)
        {
            var path = Path;

            string name;
            ErrorHandler.ThrowOnFailure(store.GetPropertyName(path, index, out name));
            var property = new SettingsStoreProperty(this, name);

            uint type;
            ErrorHandler.ThrowOnFailure(store.GetPropertyType(path, name, out type));
            property.Type = (__VsSettingsType)type;

            switch ((__VsSettingsType)type)
            {
                case __VsSettingsType.SettingsType_String:
                    string stringValue;
                    ErrorHandler.ThrowOnFailure(store.GetString(path, name, out stringValue));
                    property.Value = stringValue;
                    break;

                case __VsSettingsType.SettingsType_Int:
                    int intValue;
                    ErrorHandler.ThrowOnFailure(store.GetInt(path, name, out intValue));
                    property.Value = intValue;
                    break;

                case __VsSettingsType.SettingsType_Int64:
                    long longValue;
                    ErrorHandler.ThrowOnFailure(store.GetInt64(path, name, out longValue));
                    property.Value = longValue;
                    break;

                case __VsSettingsType.SettingsType_Binary:
                    uint[] actualByteLength = { 0 };
                    ErrorHandler.ThrowOnFailure(store.GetBinary(path, name, 0, null, actualByteLength));
                    byte[] binaryValue = new byte[actualByteLength[0]];
                    ErrorHandler.ThrowOnFailure(store.GetBinary(path, name, actualByteLength[0], binaryValue, actualByteLength));
                    property.Value = binaryValue;
                    break;

                default:
                    property.Value = null;
                    break;
            }

            return property;
        }

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
            /// <returns>An empty array, always. The parent's subcolleciton is enumerated
            /// in the background.</returns>
            protected override SettingsStoreSubCollection[] CreateSubCollections()
            {
                Task.Run(() => Parent.ExpandSubCollection()).Forget();
                return Array.Empty<SettingsStoreSubCollection>();
            }
        }
    }

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
    }

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
