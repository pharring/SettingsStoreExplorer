// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Design;

namespace SettingsStoreView
{
    internal static class SettingsStoreCommandSet
    {
        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("9fc9f69d-174d-4876-b28b-dc1e4fac89dc");

        private static CommandID MakeCommandID(int dword) => new CommandID(CommandSet, dword);

        // Commands (buttons)
        public static readonly CommandID SettingsStoreViewToolWindowCommandId = MakeCommandID(0x0100);
        public static readonly CommandID AddNewSubCollectionCommandId = MakeCommandID(0x0101);
        public static readonly CommandID AddNewStringValueCommandId = MakeCommandID(0x0102);
        public static readonly CommandID AddNewDWORDValueCommandId = MakeCommandID(0x0103);
        public static readonly CommandID AddNewQWORDValueCommandId = MakeCommandID(0x0104);
        public static readonly CommandID AddNewBinaryValueCommandId = MakeCommandID(0x0105);
        public static readonly CommandID RenameCommandId = MakeCommandID(0x0106);
        public static readonly CommandID DeleteCommandId = MakeCommandID(0x0107);
        public static readonly CommandID ModifyCommandId = MakeCommandID(0x0108);
        public static readonly CommandID RefreshCommandId = MakeCommandID(0x0109);

        // Menus
        public static readonly CommandID SubCollectionNewContextMenu = MakeCommandID(0x300);
        public static readonly CommandID TreeViewItemContextMenu = MakeCommandID(0x301);
        public static readonly CommandID ListViewItemContextMenu = MakeCommandID(0x302);
        public static readonly CommandID ListViewContextMenu = MakeCommandID(0x303);
    }
}
