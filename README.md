# Một Chút Thôi / One Little Step — prototype

Prototype 2D pixel art, màn hình dọc 9:16, Unity **6000.3.8f1** (URP 2D renderer có sẵn trong project).
Người chơi chọn một việc, một bước nhỏ, tập trung vài phút cạnh chú mèo, tự ghi nhận kết quả, nhận Gỗ
và sửa sang căn phòng.

> Đây là công cụ hỗ trợ bắt đầu làm việc và nghỉ ngơi, không phải công cụ chẩn đoán hay điều trị ADHD, trầm cảm.

Toàn bộ prototype nằm trong `Assets/OneLittleStep`; `Assets/Scenes/SampleScene` và các asset template khác giữ nguyên.

## Chạy prototype

1. Mở project bằng Unity 6000.3.8f1.
2. Mở scene `Assets/OneLittleStep/Scenes/OneLittleStep.unity`.
3. Trong Game view, chọn độ phân giải dọc (ví dụ 1080×1920 hoặc 1080×2400).
4. Bấm Play và dùng chuột: **Bắt đầu một chút** → nhập việc + bước nhỏ → chọn thời lượng → **Bắt đầu**.

Trong Editor và development build có thêm lựa chọn **10 giây (test)** để thử nhanh luồng hết giờ.
Lựa chọn này dựa trên `Debug.isDebugBuild` nên không xuất hiện trong bản release.

Dữ liệu được lưu tại `Application.persistentDataPath/onelittlestep_save.json`
(trên Windows Editor: `%USERPROFILE%\AppData\LocalLow\DefaultCompany\Just A Little\`).
Xoá file đó để chơi lại từ màn giới thiệu.

## Cấu trúc

| Đường dẫn | Nội dung |
| --- | --- |
| `Scripts/Runtime/Core` | Logic thuần, không phụ thuộc hiển thị: `FocusTimer`, `GameModel`, `DecorService`, `SaveStore`, `SaveData`, `RoomLayout` (slot, toạ độ, lớp cảnh) |
| `Scripts/Runtime/View` | `Bootstrap` (composition root), `RoomView`/`CatView`, `AppController` (+ `AppScreens`, `AppDecor`), `UIKit`, `Texts` (toàn bộ chữ tiếng Việt) |
| `Scripts/Editor` | `SceneBuilder`, `BatchTasks`, `Art/*` (bộ tạo pixel art) |
| `Resources/OneLittleStepArt` | 46 PNG do generator xuất ra (đã commit) |
| `Tests/EditMode` | Test timer, thưởng một lần, mở khóa/đặt đồ, save hỏng |
| `Tests/PlayMode` | Test bấm qua UI thật với đồng hồ giả, chụp screenshot QA |

Scene chỉ chứa camera (Pixel Perfect Camera), một Global Light 2D và `Bootstrap`. Phòng, mèo và UI được dựng
bằng code lúc chạy, nên không cần kéo thả reference nào.

## Menu `One Little Step`

- **Regenerate Pixel Art** — vẽ lại toàn bộ sprite bằng C# (`Texture2D`), áp import settings (Point, không nén,
  không mipmap, 16 PPU, pivot thống nhất). Kết quả tất định: chạy lại không đổi file nào nếu code vẽ không đổi.
  Preview phòng trống/đầy đủ (1× và 3×), contact sheet có nhãn và palette được ghi vào `QA/art/` (không commit).
  Art không bao giờ tự sinh lại khi mở Unity hay khi Play.
- **Rebuild Scene** — tạo lại scene (ghi đè đúng file cũ, không nhân đôi object) và đưa scene lên đầu Build Settings,
  giữ các scene đã có.
- **Apply Android Portrait Settings** — khoá màn hình dọc, đặt application id nếu đang là mặc định.
- **Build Android APK (release / development)** — xuất `Builds/Android/OneLittleStep.apk` chỉ với scene prototype.

Dòng lệnh (khi Unity Editor đang đóng):

```
Unity.exe -batchmode -projectPath "<project>" -executeMethod OneLittleStep.EditorTools.BatchTasks.GenerateArt -quit
Unity.exe -batchmode -projectPath "<project>" -buildTarget Android -executeMethod OneLittleStep.EditorTools.BatchTasks.BuildAndroidRelease -quit
Unity.exe -batchmode -projectPath "<project>" -runTests -testPlatform EditMode
Unity.exe -batchmode -projectPath "<project>" -runTests -testPlatform PlayMode -testCategory QA
```

Test PlayMode ghi screenshot vào `QA/screens/` và dùng thư mục save tạm, không đụng dữ liệu thật.

## Build Android

1. Cài module Android Build Support (kèm SDK/NDK/OpenJDK) cho Unity 6000.3.8f1 qua Unity Hub.
2. Menu **One Little Step → Build Android APK (release)**, hoặc File → Build Profiles → Android → Switch Platform → Build.
   Scene prototype nằm ở vị trí 0 trong Build Settings.
3. Cài `Builds/Android/OneLittleStep.apk` lên máy (`adb install -r ...`).

## Quyết định triển khai

- **Art bằng Unity Editor C#**: máy không có Python/Pillow. Mỗi vật thể có hàm vẽ riêng
  (`SetPixel`, `Rectangle`, `Line`, `FillPolygon`, `FillOval`) trên canvas pixel nguyên, toạ độ gốc dưới-trái.
- **Bố cục**: canvas thiết kế 216×384 (9:16), 16 PPU. Các lớp nền 288×576 rộng hơn để điện thoại dài/rộng không lộ mép.
  Pixel Perfect Camera (URP) phóng theo bội số nguyên (5× ở 1080 px rộng).
- **Phong cách theo ảnh tham chiếu** trong `Assets/Art_Reference` (không commit, không dùng trực tiếp): trời cyan,
  mây dạng bậc 2–3 sắc độ, hậu cảnh xanh nhạt ít tương phản, cụm lá sáng/tối, hoa trắng hình chữ thập; cộng với nhà Nhật
  mái ngói xanh xám, mặt cắt mở để thấy nội thất.
- **UI** dùng uGUI + font `LegacyRuntime.ttf` có sẵn (động, đủ dấu tiếng Việt qua font hệ thống), độ phân giải màn hình.
  Thẻ chữ đặt trên vùng trời, nút đặt trên vùng đường phố để không che căn phòng.
- **Timer** tính bằng mốc UTC, chỉ cộng thời gian hoạt động, không phụ thuộc `Time.timeScale`; vẫn chạy khi app ở nền.
  Hết giờ chỉ chuyển sang màn ghi nhận, không tự trao thưởng. Thưởng 1 Gỗ tối đa một lần/phiên, sau ≥ 30 giây hoạt động.
- **Save** ghi tại các mốc thay đổi trạng thái và khi app pause/mất focus/thoát; ghi qua file tạm, giữ `.bak`,
  file hỏng được sao sang `onelittlestep_save.corrupt-*.json`.
- **Giảm chuyển động** (Cài đặt): mèo đứng một frame, tắt fade chuyển màn và chuyển động của thông báo thưởng.
