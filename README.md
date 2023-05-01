# VRCFTtoVMCP

VRCFaceTracking からフェイストラッキングデータを受け取り、そのデータを PerfectSync の Blendshape に変換して VMCProtocol で送信する Windows アプリです。

<img src="image/window_sample.png" alt="attach:window_sample" title="attach:window_sample">

VRCFaceTracking (https://github.com/benaclejames/VRCFaceTracking) は Vive Pro Eye や Quest Pro などといったフェイストラッキングに対応したデバイスのフェイストラッキングデータを VRChat に OSC で送信するアプリです。

本アプリは VRCFaceTracking が送信するフェイストラッキングデータを VRChat の代わりに受信します。
そして受信したフェイストラッキングデータを PerfectSync の Blendshape に変換して VMCProtocol で送信します。

本アプリが送信した VMCProtocol のメッセージは VirtualMotionCapture などといった VMCProtocol による PerfectSync 対応アプリで受け取ることができます。


# 注意

本アプリは自分用に作っているため安定した動作を保証しません。

本アプリと VRChat を同時に使用できません。

VRCFaceTracking の仕様変更などにより本アプリが動作しなくなる可能性があります。


# 環境構築

## VRCFaceTracking

1. https://docs.vrcft.io/ に従い VRCFaceTracking をセットアップします。

## VRCFTtoVMCP

1. .NET6.0のSDKまたはランタイムをインストールします。

2. https://github.com/tkns3/VRCFTtoVMCP/releases より VRCFTtoVMCP.exe をダウンロードします。

3. ダウンロードした VRCFTtoVMCP.exe を任意のフォルダに置きます。

## VRChat

1. Steam で VRChat をインストールします。

2. VRChat を起動してアバターを表示し操作できる状態まで進めます。

3. VRChat を終了します。

※ VRChat で一度は OSC を有効にしないとダメかもしれない？未確認。


# 使い方

## 起動方法

Quest Pro と VirtualMotionCapture を使う例。

1. Quest Pro を (Air)Link で接続します。

2. PC で VRCFaceTracking を起動します。 ※1

3. PC で SteamVR を起動します。 ※1

4. VRCFTtoVMCP を起動します。

6. VirtualMotionCapture で VRM を読み込みキャリブレーションします。

7. VirtualMotionCapture の モーション受信(VMCProtocol) を有効にします。

9. VRCFTtoVMCP の Start をクリックします。

※1 VR 内で Oculus や SteamVR のダッシュボードから操作しない。マウスやキーボードなどを使いPCで起動する。

## VRCFTtoVMCP の設定

VRCFaceTracking と通信するポートは基本的に変更不要です。

VMCProtocol の送信先アドレスとポートは受信するアプリにあわせて変更してください。デフォルトは VirtualMotionCaptrue にあわせています。

Send rate per second は VMCProtocol の秒間あたりのメッセージ送信数です。数値を大きくすると表情変化が滑らかになります。ただし VRCFaceTracking の送信頻度のほうが頭打ちとなり 30 より大きくしても効果は少ないかもしれません。

VRCFTtoVMCP.exe と同じフォルダに VRCFTtoVMCP.json を作成して起動時の設定値を変えることが可能です。

```
{
  "autoStart": false,
  "addr1": "127.0.0.1",
  "port1": 39540,
  "port2": 9000,
  "port3": 9001,
  "rate": 30
}
```


# 使用OSS

## uOSC
- https://github.com/hecomi/uOSC
- 開発者: hecomi
- ライセンス: MIT license
- 備考: 参考に一部実装をコピー

