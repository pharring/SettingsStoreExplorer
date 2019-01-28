// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

namespace SettingsStoreView
{
    using Microsoft.VisualStudio.Shell.Interop;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for SettingsStoreViewToolWindowControl.
    /// </summary>
    public partial class SettingsStoreViewToolWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsStoreViewToolWindowControl"/> class.
        /// </summary>
        public SettingsStoreViewToolWindowControl() => InitializeComponent();

        public void InitializeViewModel(IVsSettingsManager settingsManager) => DataContext = new SettingsStoreViewModel(settingsManager);
    }
}