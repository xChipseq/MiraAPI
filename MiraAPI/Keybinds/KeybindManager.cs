using System;
using System.Collections.Generic;
using Rewired;

namespace MiraAPI.Keybinds
{
    /// <summary>
    /// Lets you register and manage mod keybinds with conflict checks and rebinding support.
    /// </summary>
    public static class KeybindManager
    {
        /// <summary>
        /// Stores all info for a registered keybind.
        /// </summary>
        public class KeybindEntry
        {
            /// <summary>Unique identifier for this keybind.</summary>
            public string Id;

            /// <summary>Text shown in the Among Us rebinding menu to describe this keybind.</summary>
            public string Description;

            /// <summary>
            /// The method that runs when this keybind is pressed.
            /// </summary>
            public Action Handler;

            /// <summary>
            /// The key currently assigned to this keybind.
            /// </summary>
            public KeyboardKeyCode Key;

            /// <summary>
            /// If true, this keybind will be checked for conflicts with other exclusive keybinds using the same key.
            /// </summary>
            public bool Exclusive;

            /// <summary>
            /// The first optional modifier key (e.g., Control, Shift, Alt) that must be held with the main key.
            /// </summary>
            public ModifierKey Modifier1 = ModifierKey.None;

            /// <summary>
            /// The second optional modifier key, if needed. Set to <c>ModifierKey.None</c> if unused.
            /// </summary>
            public ModifierKey Modifier2 = ModifierKey.None;

            /// <summary>
            /// The third optional modifier key, if needed. Set to <c>ModifierKey.None</c> if unused.
            /// </summary>
            public ModifierKey Modifier3 = ModifierKey.None;
        }

        private static readonly List<KeybindEntry> Registered = new();

        /// <summary>
        /// Registers a new mod keybind.
        /// </summary>
        /// <param name="id">Unique identifier for the keybind.</param>
        /// <param name="description">Description to display in UI.</param>
        /// <param name="key">Default key to use if no rebind exists.</param>
        /// <param name="handler">Method to invoke when the keybind is pressed.</param>
        /// <param name="exclusive">If true, conflicts with other exclusive keybinds on the same key.</param>
        /// <param name="modifier1">Optional first modifier key (e.g., <c>Control</c>, <c>Shift</c>, <c>Alt</c>) that must be held with the main key.</param>
        /// <param name="modifier2">Optional second modifier key. Use <c>ModifierKey.None</c> if unused.</param>
        /// <param name="modifier3">Optional third modifier key. Use <c>ModifierKey.None</c> if unused.</param>
        /// <returns> Returns the key that was registered for this keybind. </returns>
        public static KeyboardKeyCode Register(string id, string description, KeyboardKeyCode key, Action handler, bool exclusive = true, ModifierKey modifier1 = ModifierKey.None, ModifierKey modifier2 = ModifierKey.None, ModifierKey modifier3 = ModifierKey.None)
        {
            Registered.Add(new KeybindEntry
            {
                Id = id,
                Description = description,
                Key = key,
                Handler = handler,
                Exclusive = exclusive,
                Modifier1 = modifier1,
                Modifier2 = modifier2,
                Modifier3 = modifier3,
            });

            return key;
        }

        /// <summary>
        /// Returns all conflicts where exclusive keybinds use the same key.
        /// </summary>
        /// <returns>A list of pairs of keybinds that conflict.</returns>
        public static List<(KeybindEntry, KeybindEntry)> GetConflicts()
        {
            var conflicts = new List<(KeybindEntry, KeybindEntry)>();
            for (int i = 0; i < Registered.Count; i++)
            {
                for (int j = i + 1; j < Registered.Count; j++)
                {
                    var a = Registered[i];
                    var b = Registered[j];
                    if (a.Key == b.Key && a.Exclusive && b.Exclusive)
                    {
                        conflicts.Add((a, b));
                    }
                }
            }
            return conflicts;
        }

        /// <summary>
        /// Returns all currently registered keybind entries.
        /// </summary>
        /// <returns>A list of all registered keybind entries.</returns>
        public static List<KeybindEntry> GetEntries()
        {
            return Registered;
        }
    }
}
