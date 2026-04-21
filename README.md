## VMCSpout
[Virtual Motion Capture](https://github.com/sh-akira/VirtualMotionCapture) 用Mod  
VMC内にカメラを追加し、SpoutSenderを使用して映像を送信します。
ModはFANBOX支援版限定の機能です。  

SpoutライブラリはKlakSpoutを使用
[KlakSpout](https://github.com/keijiro/KlakSpout)

### Install
[Release](https://github.com/Snow1226/VMCSpout/releases)から最新版をダウンロードし、  
ZipファイルをVirtualMotionCaptureのフォルダに展開してください。
  
VMC内の設定から設定画面が呼び出せない場合は、  
Windowsに起動を止められているため、  
**VirtualMotionCapture/Mods/VMCSpoutにあるVMCSpoutSettingWPF.exeを１回手動で起動してください。**

### Setting
  
<img width="583" height="442" alt="image" src="https://github.com/user-attachments/assets/650b2098-83fb-475e-9c73-472467d08737" />

Spout SenderはVMCの画面と同じ画各を「VMC Spout Main」  
追加カメラを「VMC Spout *」（*は数字  
制御が若干異なるため、CameraPlusに映像を連携させるときはMainを使用せず、追加カメラのほうを使用してください。  

- VMC Spout Main : OBSに送信して正面カメラや、デスクトップ配信用  
- VMC Spout 1 ~ : CameraPlus宛送信用  
   
| 設定名 |  |
| ---- | ---- |
| SpoutName | Spoutカメラの名前を付けます。他と被らないようにしてください。 |
| Width | SpoutCameraの横解像度です。OBSやBeatSaberなどのウインドウサイズに合わせるくらいが推奨 |
| Height | SpoutCameraの縦解像度です。OBSやBeatSaberなどのウインドウサイズに合わせるくらいが推奨 |
| VMCP Port | CameraPlus等からSpoutCameraの位置を受信するポートです。重複しないようにしてください。 |
|  |  |
| Use Mirror | 床面ミラーのオンオフ |
| Mirror Intensity | ミラーの透明度です。0にいくほど薄くなります。 |
| Mirror Resolution | ミラーの解像度です。あえて解像度を下げるとぼかせます。 |
| Mirror Width | 床面ミラーの幅です、BeatSaberのステージは横 3 mです。 |
| Mirror Height | 床面ミラーの奥行です、BeatSaberのステージは奥行 2 mです。 |
| Follow Mirror Position | ミラーの中心がアバターに追従します。 |
| Center Position | ミラーの中心位置を手動で入力します。BeatSaberのRoomSettingを弄っている場合はその数字を入れます。 |
| Center Rotation | ミラーの向きをを手動で入力します。BeatSaberのRoomSettingを弄っている場合はその数字を入れます。 |

## CameraPlus Setup
<img width="388" height="446" alt="image" src="https://github.com/user-attachments/assets/9d71e5db-e89a-4273-b0ee-ffb06eb4a816" />

Select BeatSaber FolderでBeatSaberのフォルダを選択し、  
設定するプロファイルにチェックをいれ、CameraPlus Setupを押すとVMC Spout側、CmaeraPlus側共に一括で設定を行います。  
Spoutカメラの解像度はSpoutMainに合わせますので、初期値から変えたい場合はMain CameraのWidthとHeightを変更しておいてください。（あとから変更も可能です。）  
すべてのCameraPlusカメラに一括で設定するので、アバターを映したくないカメラは手動でCameraPlus側のSpoutをオフにしてください
