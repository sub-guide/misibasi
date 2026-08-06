# 부스 USB 게임패드 — 사용자 입력 ↔ Unity Input Debugger

프로젝트 전제: 이 패드는 표준 `Gamepad` 레이아웃이 아니라 **Joystick / HID::USB gamepad** 로 잡힌다. 코드는 플레이어 번호 `i`에 **`Joystick.all[i]`** 를 읽지만, **부스 운영**에서는 연결 상태를 보고 **패드를 플레이어 자리(1P~4P, 왼쪽부터)에 맞게 옮기면** 된다.

## 대응표 (무조건 이 법칙)

| 사용자·기획에서 부르는 이름 | Unity Input Debugger (Control) |
|----------------------------|--------------------------------|
| left | Stick Left (`stick/left`) |
| right | Stick Right (`stick/right`) |
| up | Stick Up (`stick/up`) |
| down | Stick Down (`stick/down`) |
| select | Button 9 (`button9`) |
| start | Button 10 (`button10`) |
| Y | Button 4 (`button4`) |
| X | Trigger (`trigger` / `Joystick.trigger`) |
| A | Button 2 (`button2`) |
| B | Button 3 (`button3`) |
| L | Button 5 (`button5`) |
| R | Button 6 (`button6`) |

## 코드 위치

- 상수·헬퍼: `Assets/_Project/Scripts/Input/BoothUsbGamepadLayout.cs`
- 슬롯별 디바이스: `Assets/_Project/Scripts/Input/SlotGamepad.cs` (`Joystick.all[index]`)
- 슬롯 입력 합성(실패 패드 + 1P 키보드 디버그): `Assets/_Project/Scripts/Input/BoothUsbSlotInput.cs`
- 개발자 1P 키보드 에뮬: `Assets/_Project/Scripts/Input/DeveloperKeyboardGamepadDebug.cs`
- Oiia 패턴: 문자 **O / I / A** 는 각각 **사용자 기준 X(Trigger) / A(Button2) / Y(Button4)** 에 대응한다.

## 개발자 키보드 디버그 (1P)

`Ctrl`(좌·우)로 **토글**. 활성 시 **슬롯 0(1P)** 에만 아래 키가 패드 입력과 **OR** 합성된다. 실제 패드가 없어도 `SlotGamepad.HasInput(0)` 이 true.

| 키 | 부스 패드 (기획명) |
|----|-------------------|
| W / A / S / D | up / left / down / right |
| Q / E | L / R |
| 키패드 7 / 8 / 4 / 5 | Y / X(Trigger) / B / A |
| V / B | Select / Start |

## 변경 시

매핑을 바꿀 때는 **이 문서**, **`BoothUsbGamepadLayout`** 주석, **`SlotGamepad`** / 미니게임 읽기 코드를 함께 맞출 것.
