// Inspired by: https://github.com/eDonnes124/Town-Of-Us-R/blob/master/source/Patches/Keybinds.cs#L29
using Rewired;
using Rewired.Data;

namespace MiraAPI.Keybinds
{
    /// <summary>
    /// Adds helper methods for registering mod keybinds with Rewired.
    /// </summary>
    public static class UserDataExtensions
    {
        /// <summary>
        /// Registers a new mod keybind as a user-assignable button action in Rewired.
        /// </summary>
        /// <param name="userData">The Rewired user data to add the action to.</param>
        /// <param name="actionName">The internal name of the action.</param>
        /// <param name="description">Text shown in the rebinding UI.</param>
        /// <param name="key">The default key to assign to this action.</param>
        /// <param name="category">Category ID to group actions in Rewired (default is 0).</param>
        /// <param name="elementIdentifierId">The element identifier ID (default is -1, meaning none specified).</param>
        /// <param name="type">The <see cref="InputActionType"/> for this action (default is Button).</param>
        /// <returns>The action ID of the newly registered action.</returns>
        public static int RegisterModBind(this UserData userData, string actionName, string description, KeyboardKeyCode key, int category = 0, int elementIdentifierId = -1, InputActionType type = InputActionType.Button)
        {
            userData.AddAction(category);

            var action = userData.GetAction(userData.actions.Count - 1)!;

            action.name = actionName;
            action.descriptiveName = description;
            action.categoryId = category;
            action.type = type;
            action.userAssignable = true;

            var map = new ActionElementMap
            {
                _elementIdentifierId = elementIdentifierId,
                _actionId = action.id,
                _elementType = ControllerElementType.Button,
                _axisContribution = Pole.Positive,
                _keyboardKeyCode = key,
                _modifierKey1 = ModifierKey.None,
                _modifierKey2 = ModifierKey.None,
                _modifierKey3 = ModifierKey.None,
            };
            userData.keyboardMaps[0].actionElementMaps.Add(map);
            userData.joystickMaps[0].actionElementMaps.Add(map);

            return action.id;
        }
    }
}
