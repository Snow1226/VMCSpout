## VMCSpout
[Virtual Motion Capture](https://github.com/sh-akira/VirtualMotionCapture) 用Mod
VMC内にカメラを追加し、SpoutSenderを使用して映像を送信します。

SpoutライブラリはKlakSpoutを使用
[KlakSpout](https://github.com/keijiro/KlakSpout)

### Install
[Release](https://github.com/Snow1226/VMCSpout/releases)から最新版をダウンロードし、  
Zipファイルの中身をフォルダごとVirtualMotionCaptureのフォルダに入れてください。  
  
VMC内の設定から設定画面が呼び出せない場合は、  
Windowsに起動を止められているため、  
VirtualMotionCapture/Mods/VMCSpoutにあるVMCSpoutSettingWPF.exeを１回手動で起動してください。

### Setting
  
<img width="584" height="441" alt="image" src="https://github.com/user-attachments/assets/6c315c27-d363-4394-a176-8795ba2fe45f" />
   
| 設定名 |  |
| ---- | ---- |
| SpoutName | Spoutカメラの名前を付けます。他と被らないようにしてください。 |
| Width | SpoutCameraの横解像度です。OBSやBeatSaberなどのウインドウサイズに合わせるくらいが推奨 |
| Height | SpoutCameraの縦解像度です。OBSやBeatSaberなどのウインドウサイズに合わせるくらいが推奨 |
| VMCP Port | CameraPlus等からSpoutCameraの位置を受信するポートです。重複しないようにしてください。 |
|  |  |
| Use Mirror | 床面ミラーのオンオフ |
| MirrorResolution | ミラーの解像度です。あえて解像度を下げるとぼかせます。 |
| Mirror Width | 床面ミラーの幅です、BeatSaberのステージは横 3 mです。 |
| Mirror Height | 床面ミラーの奥行です、BeatSaberのステージは奥行 2 mです。 |
| Follow Mirror Position | ミラーの中心がアバターに追従します。 |
| Center Position | ミラーの中心位置を手動で入力します。BeatSaberのRoomSettingを弄っている場合はその数字を入れます。 |
| Center Rotation | ミラーの向きをを手動で入力します。BeatSaberのRoomSettingを弄っている場合はその数字を入れます。 |
