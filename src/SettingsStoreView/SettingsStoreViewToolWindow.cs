// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace SettingsStoreView
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
    [Guid("f24ec500-28a5-4f29-82da-4e7d307f9d63")]
    public class SettingsStoreViewToolWindow : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsStoreViewToolWindow"/> class.
        /// </summary>
        public SettingsStoreViewToolWindow() : base(null)
        {
            Caption = "Settings Store";
        }

        protected override void Initialize()
        {
            base.Initialize();

            var control = new SettingsStoreViewToolWindowControl();
            Content = control;

            KnownUIContexts.ShellInitializedContext.WhenActivated(() =>
            {
                // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
                // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
                // the object returned by the Content property.
                ThreadHelper.ThrowIfNotOnUIThread();
                var settingsManager = GetService(typeof(SVsSettingsManager)) as IVsSettingsManager;
                control.InitializeViewModel(settingsManager);
            });
        }
    }
}
