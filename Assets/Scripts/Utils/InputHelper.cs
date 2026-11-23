using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense.Utils
{
    public static class InputHelper
    {
        public static Vector3 MousePosition
        {
            get
            {
                if (Mouse.current != null)
                {
                    Vector2 pos = Mouse.current.position.ReadValue();
                    return new Vector3(pos.x, pos.y, 0f);
                }
                return Vector3.zero;
            }
        }

        public static bool MouseLeftDown => Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        public static bool MouseLeftUp => Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;
        public static bool MouseLeftIsPressed => Mouse.current != null && Mouse.current.leftButton.isPressed;
        public static bool MouseRightDown => Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;

        public static bool Key1Down => Keyboard.current != null && (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame);
        public static bool Key2Down => Keyboard.current != null && (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame);
        public static bool Key3Down => Keyboard.current != null && (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame);

        public static bool EscapeDown => Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        public static bool SpaceDown => Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
    }
}
