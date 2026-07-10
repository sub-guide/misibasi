using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace MiniParty.Input
{
    /// <summary>
    /// 부스용 USB 게임패드 HID (Unity Input Debugger: Joystick / HID::USB gamepad).
    /// 기획·대화는 “패드에서 보이는 이름” 기준. 코드에서는 아래 Unity Control 경로(및Joystick.trigger)로만 읽는다.
    ///
    /// [사용자가 부르는 이름 = 유니티 인풋디버거]
    /// left = Stick Left, right = Stick Right, up = Stick Up, down = Stick Down,
    /// select = Button 9, start = Button 10,
    /// Y = Button 4, X = Trigger, A = Button 2, B = Button 3, L = Button 5, R = Button 6
    /// </summary>
    public static class BoothUsbGamepadLayout
    {
        public const string StickLeft = "stick/left";
        public const string StickRight = "stick/right";
        public const string StickUp = "stick/up";
        public const string StickDown = "stick/down";

        public const string Select = "button9";
        public const string Start = "button10";

        public const string FaceY = "button4";

        /// <summary>경로 문자열 이름. 실패 시 <see cref="Joystick.trigger"/> 폴백은 <see cref="PrimaryTrigger"/>.</summary>
        public const string FaceXTriggerAlias = "trigger";

        public const string FaceA = "button2";
        public const string FaceB = "button3";

        public const string ShoulderL = "button5";
        public const string ShoulderR = "button6";

        public static ButtonControl Button(InputDevice device, string pathRelativeToDevice)
        {
            if (device == null || string.IsNullOrEmpty(pathRelativeToDevice))
                return null;

            return device.TryGetChildControl<ButtonControl>(pathRelativeToDevice);
        }

        /// <summary>사용자 표기 ‘X’. Joystick 레이아웃의 표준 트리거(.trigger) 또는 <c>trigger</c> 이름.</summary>
        public static ButtonControl PrimaryTrigger(Joystick j)
        {
            if (j == null)
                return null;

            if (j.trigger != null)
                return j.trigger;

            return j.TryGetChildControl<ButtonControl>(FaceXTriggerAlias);
        }
    }
}
