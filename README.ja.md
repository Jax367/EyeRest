<div align="center">

![EyeRest · A little pause, a softer day](https://raw.githubusercontent.com/Jax367/EyeRest/main/docs/images/banner.svg)

# 目间 EyeRest

**忙しい毎日に、少しの休息を。**

軽量 Windows 休憩リマインダー · 4 種類のテーマ · カスタムメッセージ · ローカル設定

[![Release](https://img.shields.io/github/v/release/Jax367/EyeRest?style=flat-square&color=687d42)](https://github.com/Jax367/EyeRest/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/Jax367/EyeRest/total?style=flat-square&color=28765b)](https://github.com/Jax367/EyeRest/releases)
[![Windows](https://img.shields.io/badge/Windows-10%20%2F%2011-486b82?style=flat-square)](https://github.com/Jax367/EyeRest/releases/latest)
[![C Sharp](https://img.shields.io/badge/C%23-.NET%20Framework%204.8-77608c?style=flat-square)](#技術スタック)
[![Stars](https://img.shields.io/github/stars/Jax367/EyeRest?style=flat-square&color=ba9550)](https://github.com/Jax367/EyeRest/stargazers)

[简体中文](README.md) · [English](README.en.md) · **日本語**

[ダウンロード](https://github.com/Jax367/EyeRest/releases/latest) · [スクリーンショット](#スクリーンショット) · [技術スタック](#技術スタック) · [問題を報告](https://github.com/Jax367/EyeRest/issues)

</div>

---

## EyeRest について

| ⏱️ 自分のペースで | 🎨 好みの見た目に | 🌿 静かに寄り添う |
| :--- | :--- | :--- |
| 初期設定は 20 分ごとに 20 秒の休憩。一時停止・再開・スヌーズに対応 | 4 種類のテーマ、人物画像、フォント・配色、70%–150% のウィンドウ拡大縮小 | タスクトレイ常駐、単一起動、任意の自動起動。管理者権限は不要 |
| 自分のメッセージを順番またはランダムに表示 | 非公式ファンテーマ「芸術の階 · Netzach」 | 設定はローカル保存。アプリはネットワーク通信を行いません |

現在のバージョン：**v1.5.0** · 作者：**Lar**（turelar367@gmail.com）。ドキュメントは中国語・英語・日本語に対応しています。アプリの UI は現在中国語です。以下では操作に必要な中国語のラベルを併記しています。

## スクリーンショット

| 松林 · 設定 | 松林 · 休憩リマインダー |
| :---: | :---: |
| ![松林テーマの設定画面](https://raw.githubusercontent.com/Jax367/EyeRest/main/docs/images/settings-0.png) | ![松林テーマのリマインダー](https://raw.githubusercontent.com/Jax367/EyeRest/main/docs/images/reminder-0.png) |
| **芸術の階 · 設定** | **芸術の階 · Netzach** |
| ![芸術の階の設定画面](https://raw.githubusercontent.com/Jax367/EyeRest/main/docs/images/settings-3.png) | ![Netzach のリマインダー](https://raw.githubusercontent.com/Jax367/EyeRest/main/docs/images/reminder-3.png) |

## 技術スタック

| 技術 | 用途 |
| :--- | :--- |
| **C# / .NET Framework 4.8** | アプリの処理と実行環境。システム付属の C# コンパイラでビルド |
| **WinForms** | 設定画面、休憩ウィンドウ、トレイメニュー、イベント処理 |
| **System.Drawing / GDI+** | テーマ、タイトルバー、アイコン、イラスト、比率を保った描画 |
| **JSON / DataContractJsonSerializer** | ローカル設定、アトミック保存、バックアップ復元 |
| **Stopwatch / WinForms Timer** | 単調増加時計による計時、一時停止・再開、カウントダウン |
| **Mutex / EventWaitHandle** | 多重起動防止と既存ウィンドウの表示 |
| **PowerShell / Inno Setup** | 自己テスト、ポータブル ZIP、ユーザー単位のインストーラー、SHA-256 検証 |

サードパーティのランタイム依存関係はありません。実行時に Python、Node.js、.NET SDK は不要です。

## ダウンロードとインストール

[GitHub Releases](https://github.com/Jax367/EyeRest/releases/latest) から最新版を入手してください。

- **`EyeRest-1.5.0-portable.zip`**：展開し、フォルダー内の `EyeRest.exe` を実行します。`EyeRest.exe.config` は EXE と同じフォルダーに置いてください。
- **`EyeRest-1.5.0-Setup.exe`**：現在のユーザーのディレクトリにインストールします。スタートメニュー、任意のデスクトップショートカット、アンインストーラーが用意されます。管理者権限は不要です。
- **`SHA256SUMS.txt`**：両ファイルの SHA-256 チェックサムです。

動作環境は Windows 10 / 11 と .NET Framework 4.8 です。インストール版とポータブル版は同じ設定を使用します。更新前にトレイメニューの「退出目间」で終了してください。アンインストールしても個人設定は残り、そのインストール先を指す自動起動項目だけが削除されます。

## 基本操作

1. 起動すると、初期設定では 20 分のカウントダウンが始まります。
2. リマインダー間隔は 1–240 分、休憩時間は 5–600 秒、スヌーズは 1–60 分に設定できます。
3. テキストや見た目を変更し、「预览提醒」で確認してから「保存设置」で保存します。
4. 休憩が終わるとウィンドウは自動で閉じ、次の周期が始まります。「完成休息」で早めに終了、「稍后提醒」または × でスヌーズできます。

設定を保存すると変更が適用されます。間隔を変更した場合は計時がリセットされますが、文言や見た目だけの変更ではリセットされません。一時停止中の保存でも停止状態は維持されます。プレビューは次回の通知時刻を変更しません。

設定画面を閉じる、または最小化してもトレイで動作を続けます。トレイアイコンをダブルクリックすると設定画面が開き、クリックまたは右クリックでメニューが開きます。「现在休息」で今すぐ休憩、「退出目间」で完全に終了できます。アイコンは隠れている場合があります。

再度 EXE を実行しても多重起動せず、既存の設定画面が開きます。終了や PC 再起動後に残り時間は引き継がれず、次回は新しい周期から始まります。

## サインイン時の自動起動

「开机自启（登录后进入托盘）」をオンにして「保存设置」を押すと有効になります。初期設定は無効です。オフにして保存すると解除されます。サインイン時は設定画面を開かずトレイで計時を開始し、手動起動時は設定画面を表示します。

現在のユーザーにだけ適用されます。ポータブル版は移動しないフォルダーに置いてから有効にしてください。移動後は再起動して設定を保存し、パスを更新します。タスクマネージャーで無効にした場合は、そちらで再び有効にする必要があります。

自動起動には `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` の `EyeRest` 値を使用します。保存に失敗した場合は元の自動起動設定に戻します。自己テストは隔離されたアダプターを使用し、PC の自動起動設定を変更しません。

## カスタムメッセージ

「编辑语录集」を開き、自分のメッセージを入力します。メッセージの間には、`---` だけの行を入れてください。1 件に複数行を含められます。最大 200 件、各 500 文字までです。

```text
少し遠くを眺めてみましょう。
忙しいときこそ、ひと息ついて。
---
肩の力を抜いて、水を飲みましょう。
```

「使用此语录集」で戻り、「语录集 · 按顺序」（順番）または「语录集 · 随机」（ランダム）を選び、設定を保存します。順番モードは最後まで進むと最初に戻ります。複数のメッセージがある場合、ランダムモードは同じ文の連続表示を避けます。休憩中のメッセージは変わりません。

プレビューは再生順を進めません。配色、人物、フォント、通知間隔の変更でも再生順はリセットされません。メッセージ集や再生モードを変更すると最初から始まり、終了後に位置は保存されません。「单条提醒」で単一メッセージに戻せます。空のメッセージ集を有効な再生設定として保存することはできません。

## テーマと人物画像

「松林」「夜航」「暖纸」「艺术层 · Netzach」の 4 テーマを用意しています。背景、入力欄・ボタン、文字、アクセント、通知の配色を個別に変更できます。インストール済みフォント、9–14 pt の文字サイズ、70%–150% の通知サイズにも対応し、画面に収まるよう自動調整します。

フォントと文字サイズをホイールで変更するには、先にその入力欄をクリックしてください。マウスを重ねるだけ、または Tab でフォーカスするだけでは変更されません。

「艺术层 · Netzach」は *Library of Ruina* の芸術の階を参考にした非公式ファンテーマです。背景を除去したちびキャラ、元の立ち絵、半目でポケットに手を入れたちびキャラを選べます。人物画像は EXE に埋め込まれており、必要なときだけ読み込まれます。

「换一句 Netzach 台词」で 4 つの内蔵メッセージを切り替え、直接編集することもできます。文言は休憩向けに書かれたオリジナルで、ゲーム本編の台詞や音声ではありません。通常の通知文とは別に保存され、テーマを変更しても互いに上書きされません。素材の説明とチェックサムは [assets/ARTWORK.md](assets/ARTWORK.md)（中国語）を参照してください。

## 設定ファイル

設定は `%LOCALAPPDATA%\EyeRest\settings.json` に保存します。保存前の内容は `.bak` として残します。ファイルが壊れた場合はバックアップから復元を試み、復元できなければ初期設定に戻して通知します。

## ビルドと検証

プロジェクトのディレクトリで PowerShell を実行します。

```powershell
.\build.ps1 -Test
```

Windows .NET Framework 付属の C# コンパイラを使用し、`build\EyeRest.exe`、設定ファイル、ポータブル ZIP を生成します。依存関係のダウンロードは行いません。

インストーラーも生成する場合は [Inno Setup](https://jrsoftware.org/isinfo.php) を用意してください。

```powershell
.\package.ps1 -IsccPath 'C:\Program Files\Inno Setup 7\ISCC.exe'
```

再コンパイルと自己テストの後、`build\release` にバージョン付き ZIP、インストーラー、チェックサムを生成します。バージョンは `src\Program.cs` から読み取ります。

テストでは計時の境界、多重通知防止、一時停止・再開、スヌーズ、設定復元、テーマ、ウィンドウ操作、実際の 5 秒後の自動終了などを確認します。通常のビルドのテスト結果は `build\test-results.txt` に保存されます。

```powershell
Start-Process .\build\EyeRest.exe -ArgumentList '--render-preview', "$PWD\artifacts" -Wait
```

このコマンドで 4 テーマと長文・大きい文字のプレビューを生成できます。自己テストとプレビューは個人設定を上書きしません。

## ソースコード

| ファイル | 役割 |
| :--- | :--- |
| `src/Core.cs` | 設定、アトミック保存、計時の状態管理 |
| `src/Program.cs` | エントリーポイント、単一起動、トレイ、通知スケジュール |
| `src/SettingsForm.cs` / `src/ReminderForm.cs` | 設定画面 / 休憩とカウントダウン |
| `src/Styles.cs` / `src/WindowTheme.cs` | テーマ、描画、タイトルバー、入力欄、トレイメニュー |
| `src/ArtTheme.cs` | イラストの遅延読み込みと図書館風の装飾 |
| `src/Startup.cs` / `src/Quotes.cs` | 自動起動 / メッセージ集の編集・再生 |
| `src/SelfTests.cs` | 自己テストと UI プレビュー |

通常時のタイマーは 1 秒ごと、通知中のカウントダウンは 250 ミリ秒ごとに更新します。ビジーループは使用しません。メモリ使用量には CLR、フォント、ウィンドウのリソースも含まれ、EXE のサイズとは異なります。

---

<div align="center">

### 🌱 ひと休みした人たち

![EyeRest ページ表示カウンター](https://count.kjchmc.cn/get/@Jax367-EyeRest?theme=asoul)

<sub>Powered by <a href="https://github.com/journey-ad/Moe-Counter">Moe Counter</a> · <a href="https://count.kjchmc.cn/?lang=en">カウンターサービス</a> · 画像の読み込み回数です。ユニーク訪問者数ではありません。</sub>

**Made by Lar · Take a break, then keep going.**

</div>
