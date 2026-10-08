# 目间 EyeRest

[简体中文](README.md) | [English](README.en.md)

A lightweight Windows desktop break reminder built with C#, native WinForms, and .NET Framework 4.8. It has no third-party runtime dependencies and makes no network requests.

Current version: **v1.5.0**. Author: **Lar** (turelar367@gmail.com).

The application's interface is currently in Chinese. This guide includes Chinese labels for the main controls to help you find them.

## Download and installation

Download the latest version from [GitHub Releases](https://github.com/Jax367/EyeRest/releases/latest):

- `EyeRest-1.5.0-portable.zip`: extract the archive and double-click `EyeRest.exe` inside the extracted folder.
- `EyeRest-1.5.0-Setup.exe`: installs for the current user, with a Start menu entry, an optional desktop shortcut, and an uninstaller. Administrator privileges are not required.
- `SHA256SUMS.txt`: SHA-256 checksums for both downloads.

The installed and portable versions share the same local settings. Uninstalling keeps your personal settings and removes the EyeRest startup entry only if it points to that installation directory. Exit the running application from its tray menu before upgrading.

## Running the application

After building from source, double-click `build\EyeRest.exe`. Keep `EyeRest.exe.config` in the same folder as the executable. You can also extract `build\EyeRest-portable.zip` and run the executable inside it.

Requirements: Windows 10 / 11 and .NET Framework 4.8. Python, Node.js, and the .NET SDK are not required to run EyeRest. The application does not require administrator privileges.

## Usage

- By default, EyeRest reminds you every 20 minutes to take a 20-second break. The timer starts when the application launches.
- The reminder interval can be set to 1–240 minutes, the break duration to 5–600 seconds, and the snooze duration to 1–60 minutes.
- Reminder text supports multiple lines and up to 500 characters. You can optionally keep reminders on top and enable a system notification sound.
- **Reminder window size (提醒窗口大小)** ranges from 70% to 150%, with 100% as the default. The window, portrait, text, and buttons scale together while preserving the window's aspect ratio. If necessary, the window is scaled down to fit the screen's working area. Preview your changes, then click **Save settings (保存设置)** to apply them to future reminders.
- Changes take effect when you save settings. Changing the reminder interval restarts the timer; changing only the text or appearance does not. Saving settings while paused keeps the timer paused.
- **Preview reminder (预览提醒)** uses the settings currently shown in the interface without changing the next scheduled reminder.
- When a break ends, the reminder closes automatically and a new cycle begins. **Finish break (完成休息)** ends the break early. **Remind me later (稍后提醒)** or the reminder's × button postpones it by the configured snooze duration.
- Pausing preserves the remaining time; resuming continues from there. The tray menu also offers **Take a break now (现在休息)**.
- Closing or minimizing the settings window leaves EyeRest running in the system tray. Double-click the tray icon to reopen settings. A single click or right-click opens a menu styled to match the current theme. Choose **Exit EyeRest (退出目间)** to quit completely. The icon may be in Windows' hidden tray icons area.
- Running the executable again opens the existing instance's settings window instead of starting another reminder process.
- Countdown progress is not saved when you exit or restart your computer. The next launch starts a full cycle.

## Launch at sign-in

Select **Start with Windows (开机自启（登录后进入托盘）)** and click **Save settings (保存设置)** to enable it. This option is off by default. Clear the checkbox and save to disable it. When enabled, EyeRest starts counting and quietly enters the system tray at sign-in; launching the executable manually still opens settings.

This applies only to the current Windows user and requires no administrator privileges. The startup entry stores the executable's full path, so extract the portable version to a permanent location before enabling it. If you move the program, launch it and save the startup settings again to update the path. If you disabled its startup entry in Windows Task Manager, re-enable it there.

EyeRest uses the `EyeRest` value under `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`. The command contains the quoted executable path followed by `--startup`. If saving the configuration fails, the previous startup entry is restored. If a registry write fails, EyeRest displays a message and keeps the old settings. Self-tests use an isolated registry adapter and do not enable startup on your computer.

## Custom quote collections

Click **Edit quote collection (编辑语录集)** to enter your own messages. Separate messages with `---` on a line by itself. Each message may contain multiple lines. A collection can contain up to 200 messages, each with up to 500 characters.

```text
Look into the distance for a moment.
Even during a busy day, give yourself time to rest.
---
Take it slowly. Leave a little time for yourself.
---
Relax your shoulders and have some water.
```

Click **Use this collection (使用此语录集)** to return to the main window. Select **Quote collection · Sequential (语录集 · 按顺序)** or **Quote collection · Random (语录集 · 随机)**, then save settings. Sequential mode advances through the collection and wraps back to the beginning. Random mode avoids repeating the same message consecutively when multiple messages are available. Each reminder shows one message, which stays unchanged throughout that break.

Previewing does not advance the scheduled reminders' sequence. Changing colors, portraits, fonts, or the reminder interval does not reset it either. Editing the collection or changing playback mode restarts the sequence. Playback position is not saved on exit. Both ordinary themes and the Floor of Art theme can use the same custom collection.

Select **Single reminder (单条提醒)** to return to the original single-message mode. If the collection is empty, EyeRest asks you to add messages or switch back to single-message mode; it will not save an empty collection as the active playback source.

## Appearance

EyeRest includes four themes: **Pine Forest (松林)**, **Night Voyage (夜航)**, **Warm Paper (暖纸)**, and **Floor of Art · Netzach (艺术层 · Netzach)**. You can customize the interface background, input/button background, interface text, accent color, reminder background, and reminder text color separately. You can also choose an installed font and a size from 9 to 14 pt. Reminder body text uses a larger size.

Color and font changes appear immediately in settings. Preview a reminder to check the result, then save. Both settings and reminder windows have themed title bars and can be dragged or closed. The settings window also supports minimizing, maximizing, and resizing from its edges. Font dropdowns, numeric controls, the tray icon, and tray menus use the theme's colors. Long text and small windows have scrollable areas; native scrollbars are still drawn by Windows.

Click the font or font-size control before using the mouse wheel to change its selection. Hovering or focusing it with Tab alone does not enable wheel adjustment. Leaving the control locks it again. While inactive, scrolling can continue to scroll the settings page.

### Floor of Art · Netzach

Select **Floor of Art · Netzach (艺术层 · Netzach)** under **Interface and reminder appearance (界面与提醒样式)**, preview it, and save settings. Inspired by the Floor of Art in *Library of Ruina*, this theme uses deep olive greens, yellow-green glows, and a library atmosphere. Reminders feature a character illustration and attributed dialogue.

Under **Character portrait (人物形象)**, choose **Chibi · Image 1, background removed (Q 版 · 图 1 去背景)**, the default, or **Full-body portrait · Original image 2 (立绘 · 图 2 原图)**. Only the checkerboard background was removed from the chibi image; its dimensions, character linework, and RGB color values remain unchanged, with no recoloring of the face. The original is retained separately. The full-body image uses the original file byte for byte. The small character in settings and future reminders use the selected portrait.

A third option, **Chibi · Weary, hands in pockets (Q 版 · 倦怠插兜)**, was generated using a user-provided expression reference. It depicts a relaxed standing pose with half-closed eyes, a slightly weary expression, and both hands in the trouser pockets, on a transparent background. Select it and save to use it in future reminders.

The theme's default reminder reads as follows in English; the application displays the Chinese text:

> …You've been busy for quite a while, haven't you?
> Take your eyes off the screen and look into the distance.
> No hurry. I'll keep you company while you rest.

**Choose another Netzach line (换一句 Netzach 台词)** cycles through four built-in messages. You can also edit the text directly. These are original, character-inspired messages written for break reminders, not quotations or voice lines from the game. This is an unofficial fan theme. Its dialogue is saved separately from the ordinary reminder text, so switching themes does not overwrite either. Older configuration files remain readable.

All three portraits are embedded in the executable and decoded on demand. No separate image files need to be copied, and the theme adds no background animation. See [assets/ARTWORK.md](assets/ARTWORK.md) for artwork notes and SHA-256 checksums (in Chinese).

## Settings file

Settings are stored in `%LOCALAPPDATA%\EyeRest\settings.json`. Saving keeps the previous version as a `.bak` backup. If the configuration is corrupted, EyeRest first tries to restore the backup; otherwise, it uses defaults and displays a notice. Settings remain on your computer.

## Building and verification

Run this command in PowerShell from the project directory:

```powershell
.\build.ps1 -Test
```

The script uses the C# compiler included with Windows .NET Framework and produces `build\EyeRest.exe`, its configuration file, and a portable ZIP. It does not download dependencies.

To create the complete release packages, install [Inno Setup](https://jrsoftware.org/isinfo.php) and run:

```powershell
.\package.ps1 -IsccPath 'C:\Program Files\Inno Setup 7\ISCC.exe'
```

By default, this script recompiles the application and runs its self-tests. It produces versioned portable and installer packages and a checksum file in `build\release`. The version is read from `src\Program.cs`.

Self-tests cover reminder boundaries, duplicate reminder prevention, pause/resume, snoozing, large clock jumps, interval changes while paused, settings persistence and corruption recovery, window closing behavior, theme/font changes, numeric control layout, and a real five-second automatic reminder close. The standard build writes results to `build\test-results.txt`.

To regenerate interface previews:

```powershell
Start-Process .\build\EyeRest.exe -ArgumentList '--render-preview', "$PWD\artifacts" -Wait
```

The `artifacts` folder contains settings and reminder previews for all four themes, along with large-font and 500-character text samples. The Floor of Art previews are `settings-3.png` and `reminder-3.png`. Self-test and preview modes do not overwrite personal settings.

## Code structure

- `src/Core.cs`: settings, atomic saves, and the timer state machine.
- `src/Program.cs`: entry point, single-instance handling, system tray, and reminder scheduling.
- `src/SettingsForm.cs`: settings interface.
- `src/ReminderForm.cs`: break reminder and countdown.
- `src/Styles.cs`: themes, color selection, and drawing.
- `src/ArtTheme.cs`: lazy loading of artwork and library decorations.
- `src/WindowTheme.cs`: themed title bars, font dropdowns, numeric controls, and tray menu drawing.
- `src/Startup.cs`: current-user startup registration and rollback when saving settings fails.
- `src/Quotes.cs`: quote collection editing and sequential/random playback.
- `src/SelfTests.cs`: self-tests and interface rendering.

EyeRest uses a WinForms timer that ticks once per second and a monotonic clock, with no busy loop. While a break reminder is open, its countdown updates every 250 milliseconds. Actual memory usage includes the CLR, fonts, and window resources; executable size is not a measure of runtime memory usage.
