using MiniParty.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MiniParty.Minigames.Pigeon
{
    public sealed partial class PigeonMinigameModule
    {
        Transform GetCursor(int i)
        {
            if (cursors == null || i < 0 || i >= cursors.Length)
                return null;

            return cursors[i];
        }

        void TickCursorMove(int slotIndex, float speed, float dt)
        {
            Transform cursor = GetCursor(slotIndex);
            if (cursor == null || !cursor.gameObject.activeInHierarchy)
                return;

            Vector2 dir = ReadMoveDir(slotIndex);
            if (dir.sqrMagnitude < 0.0001f)
                return;

            Vector3 local = cursor.localPosition;
            local.x += dir.x * speed * dt;
            local.y += dir.y * speed * dt;
            cursor.localPosition = ClampToCamera(cursor, local);
        }

        static Vector2 ReadMoveDir(int slotIndex)
        {
            Joystick pad = SlotGamepad.Get(slotIndex);
            float x = 0f;
            float y = 0f;

            if (BoothUsbSlotInput.IsPathHeld(slotIndex, pad, BoothUsbGamepadLayout.StickRight))
                x += 1f;
            if (BoothUsbSlotInput.IsPathHeld(slotIndex, pad, BoothUsbGamepadLayout.StickLeft))
                x -= 1f;
            if (BoothUsbSlotInput.IsPathHeld(slotIndex, pad, BoothUsbGamepadLayout.StickUp))
                y += 1f;
            if (BoothUsbSlotInput.IsPathHeld(slotIndex, pad, BoothUsbGamepadLayout.StickDown))
                y -= 1f;

            if (x == 0f && y == 0f)
                return Vector2.zero;

            return new Vector2(x, y).normalized;
        }

        Vector3 ClampToCamera(Transform cursor, Vector3 local)
        {
            if (playCamera == null || !playCamera.orthographic)
                return local;

            Transform parent = cursor.parent;
            if (parent == null)
                return local;

            float h = playCamera.orthographicSize;
            float w = h * playCamera.aspect;
            Vector3 cam = playCamera.transform.position;

            Vector3 min = parent.InverseTransformPoint(new Vector3(cam.x - w, cam.y - h, parent.position.z));
            Vector3 max = parent.InverseTransformPoint(new Vector3(cam.x + w, cam.y + h, parent.position.z));

            local.x = Mathf.Clamp(local.x, Mathf.Min(min.x, max.x), Mathf.Max(min.x, max.x));
            local.y = Mathf.Clamp(local.y, Mathf.Min(min.y, max.y), Mathf.Max(min.y, max.y));
            return local;
        }
    }
}
