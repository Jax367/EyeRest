<div align="center">

![EyeRest · 给忙碌留一点空隙](https://raw.githubusercontent.com/Jax367/EyeRest/main/docs/images/banner.svg)

# 目间 EyeRest

**给忙碌留一点空隙，让休息有一点温度。**

轻量 Windows 休息提醒 · 四套主题 · 自定义语录 · 本地运行

[![Release](https://img.shields.io/github/v/release/Jax367/EyeRest?style=flat-square&color=687d42)](https://github.com/Jax367/EyeRest/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/Jax367/EyeRest/total?style=flat-square&color=28765b)](https://github.com/Jax367/EyeRest/releases)
[![Windows](https://img.shields.io/badge/Windows-10%20%2F%2011-486b82?style=flat-square)](https://github.com/Jax367/EyeRest/releases/latest)
[![C Sharp](https://img.shields.io/badge/C%23-.NET%20Framework%204.8-77608c?style=flat-square)](#技术栈)
[![Stars](https://img.shields.io/github/stars/Jax367/EyeRest?style=flat-square&color=ba9550)](https://github.com/Jax367/EyeRest/stargazers)

**简体中文** · [English](README.en.md) · [日本語](README.ja.md)

[下载最新版](https://github.com/Jax367/EyeRest/releases/latest) · [界面预览](#界面预览) · [技术栈](#技术栈) · [反馈问题](https://github.com/Jax367/EyeRest/issues)

</div>

---

## 为什么选择目间

| ⏱️ 按自己的节奏 | 🎨 让提醒更合心意 | 🌿 安静地陪伴 |
| :--- | :--- | :--- |
| 默认 20 分钟 / 20 秒；支持暂停、继续和稍后提醒 | 四套主题、人物形象、字体配色与 70%–150% 窗口缩放 | 托盘运行、单实例、可选开机自启，无需管理员权限 |
| 自定义语录按顺序或随机播放 | 艺术层 · Netzach 非官方同人主题 | 设置保存在本机，应用无网络请求 |

当前版本：**v1.5.0** · 作者：**Lar**（turelar367@gmail.com）。文档提供中文、英文和日文；应用界面目前为中文。

## 界面预览

| 松林 · 设置 | 松林 · 休息提醒 |
| :---: | :---: |
| ![松林设置界面](https://raw.githubusercontent.com/Jax367/EyeRest/main/docs/images/settings-0.png) | ![松林休息提醒](https://raw.githubusercontent.com/Jax367/EyeRest/main/docs/images/reminder-0.png) |
| **艺术层 · 设置** | **艺术层 · Netzach 提醒** |
| ![艺术层设置界面](https://raw.githubusercontent.com/Jax367/EyeRest/main/docs/images/settings-3.png) | ![Netzach 休息提醒](https://raw.githubusercontent.com/Jax367/EyeRest/main/docs/images/reminder-3.png) |

## 技术栈

| 技术 | 在项目中的用途 |
| :--- | :--- |
| **C# / .NET Framework 4.8** | 应用逻辑与运行时；使用系统自带 C# 编译器构建 |
| **WinForms** | 设置窗口、休息弹窗、托盘菜单与事件调度 |
| **System.Drawing / GDI+** | 主题、标题栏、图标、人物插画与等比例绘制 |
| **JSON / DataContractJsonSerializer** | 本机配置持久化、原子保存与备份恢复 |
| **Stopwatch / WinForms Timer** | 单调时钟计时、暂停恢复与提醒倒计时 |
| **Mutex / EventWaitHandle** | 单实例防护与已有窗口激活 |
| **PowerShell / Inno Setup** | 自检、便携 ZIP、当前用户安装包和 SHA-256 校验文件 |

应用无第三方运行时依赖，运行时无需 Python、Node.js 或 .NET SDK。

## 下载与安装

从 [GitHub Releases](https://github.com/Jax367/EyeRest/releases/latest) 下载最新版：

- `EyeRest-1.5.0-portable.zip`：便携版，解压后双击目录内的 `EyeRest.exe`。
- `EyeRest-1.5.0-Setup.exe`：安装版，安装到当前用户目录，提供开始菜单入口、可选桌面快捷方式和卸载入口，无需管理员权限。
- `SHA256SUMS.txt`：两个下载文件的 SHA-256 校验值。

安装版和便携版使用同一份本机设置；卸载保留个人设置，并移除指向该安装目录的开机自启项。升级前请从托盘菜单退出正在运行的程序。

## 运行

双击 `build\EyeRest.exe`。请将 `EyeRest.exe.config` 保留在 EXE 同一目录。也可以解压 `build\EyeRest-portable.zip` 后运行。

运行环境：Windows 10 / 11，.NET Framework 4.8。无需 Python、Node.js 或 .NET SDK。程序不需要管理员权限。

## 使用

- 默认每 20 分钟提醒一次，休息 20 秒。启动后即开始计时。
- 提醒间隔可设为 1–240 分钟，休息时长为 5–600 秒，稍后提醒为 1–60 分钟。
- 支持最多 500 字的多行提醒文本、可选置顶和系统提示音。
- 「提醒窗口大小」可设为 70%–150%，默认 100%。窗口、人物、文字和按钮同步等比例缩放，宽高比固定；如超出屏幕可用范围，会自动整体缩小。修改后可预览，点击「保存设置」应用于后续提醒。
- 点击「保存设置」后生效。修改提醒间隔会重新计时；仅修改文本或样式不会重置当前计时。暂停状态下保存仍保持暂停。
- 「预览提醒」使用当前界面上的设置，不改变下一次提醒时间。
- 休息结束自动关闭弹窗并开始下一轮；「完成休息」可提前结束。点击「稍后提醒」或弹窗右上角 ×，会按设定时间推迟提醒。
- 暂停会保留剩余时间，继续后接着计时。托盘菜单还提供「现在休息」。
- 关闭或最小化设置窗口后继续在托盘运行。双击托盘图标重新打开；单击或右键可展开与当前主题配色一致的菜单，选择「退出目间」才能完全退出。托盘图标可能在 Windows 的隐藏图标区域内。
- 重复运行 EXE 会打开已有实例的设置窗口，不会生成多个提醒进程。
- 退出或重启电脑后不保留倒计时进度；下次启动开始一个完整周期。

## 开机自启

勾选「开机自启（登录后进入托盘）」并点击「保存设置」即可开启。默认关闭，取消勾选并保存即可移除。启用后，登录 Windows 时开始计时并静默进入托盘；手动运行 EXE 仍会打开设置窗口。

自启只作用于当前 Windows 用户，无需管理员权限。自启项记录当前 EXE 的完整路径，因此应先将便携包解压到长期使用的目录；移动程序后重新打开并保存自启设置以更新路径。如在 Windows 任务管理器中禁用了该启动项，需要在那里重新启用。

实现使用当前用户注册表 `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` 的 `EyeRest` 值，命令为带引号的 EXE 路径加 `--startup`。保存配置失败时会恢复之前的自启项，注册表写入失败时会提示并保留旧设置。自检使用隔离的注册表适配器，不会替你开启系统自启。

## 自定义语录集

点击「编辑语录集」，输入自己的语录。不同语录用单独一行 `---` 分隔，同一条语录可以包含多行。最多 200 条，每条最多 500 字。

```text
先看看远处吧。
忙完这段，也别忘了休息。
---
慢慢来，给自己留一点时间。
---
放松肩膀，喝口水吧。
```

点击「使用此语录集」返回主窗口，选择「语录集 · 按顺序」或「语录集 · 随机」，最后点击「保存设置」。按顺序模式每次弹窗取下一条，末尾回到第一条；随机模式有多个条目时避免连续选择同一条目。每次弹窗只显示一条，休息过程中保持不变。

预览不会推进正式提醒的顺序；改变颜色、人物、字体或提醒间隔也不会重置语录顺序。修改语录集或播放模式后重新开始，退出程序后不保存播放位置。普通主题和艺术层都可使用同一份自定义语录集。

选择「单条提醒」恢复原来的单条文本。语录集为空时，界面会要求先添加语录或切回单条模式；不会保存无内容的播放设置。

## 样式自定义

内置「松林」「夜航」「暖纸」「艺术层 · Netzach」四套主题。可以分别选择界面背景、输入框/按钮底色、界面文字、强调色、提醒背景、提醒文字，也可以选择本机字体和 9–14 pt 字号。提醒正文会使用较大字号。

颜色和字体修改会即时反映在设置页；点击「预览提醒」查看弹窗效果，满意后保存。设置页和提醒窗均使用主题标题栏，可拖动或关闭；设置页还支持最小化、最大化与边缘调整大小。字体下拉框、数值按钮、托盘图标和托盘菜单使用主题配色。长文本与小窗口提供滚动区域，原生滚动条仍由 Windows 绘制。

字体与字号框须先用鼠标点击，才能用滚轮改变选项；鼠标悬停或仅用 Tab 聚焦不会启用滚轮调整。焦点离开后重新锁定，未激活时滚轮可继续滚动设置页。

### 艺术层 · Netzach

在「界面与提醒样式」选择「艺术层 · Netzach」，预览后点击「保存设置」。主题参考《废墟图书馆》艺术层的深橄榄绿、黄绿色微光与书库氛围，提醒窗口采用人物插画和署名对白。

「人物形象」可选择「Q 版 · 图 1 去背景」（默认）或「立绘 · 图 2 原图」。Q 版仅去除格子背景，尺寸、人物线条与全部 RGB 颜色像素保持不变，面部不调色。原始文件另行保留；立绘仍逐字节使用原文件。设置页的小人和后续提醒窗同步使用所选形象。

另有「Q 版 · 倦怠插兜」：以用户提供的表情为参考生成，半闭眼、略显倦怠，双手插裤兜自然站立，使用真实透明背景。选择后点击「保存设置」即可用于后续提醒。

艺术层默认提醒：

> ……已经忙了好一会儿了吧。
> 先把视线从屏幕上挪开，看看远处。
> 别急，我陪你歇一会儿。

「换一句 Netzach 台词」可依次选择四段内置文案，也可直接编辑文本。文案为休息场景原创的角色风格对白，并非游戏原文或配音；本主题为非官方同人主题。艺术层对白与普通提醒文本分别保存，切换主题不会覆盖另一套文本。旧版配置可直接读取。

三个形象均内嵌在程序中，按需解码，无需另外复制图片，不新增后台动画。素材说明及 SHA-256 校验值见 `assets/ARTWORK.md`。

## 设置文件

保存在 `%LOCALAPPDATA%\EyeRest\settings.json`，保存时保留上一份 `.bak` 备份。配置文件损坏时优先恢复备份，否则使用默认值并提示。设置只存储在本机。

## 构建与验证

在项目目录运行 PowerShell：

```powershell
.\build.ps1 -Test
```

脚本调用 Windows .NET Framework 自带的 C# 编译器，生成 `build\EyeRest.exe`、配置文件和便携 ZIP。没有依赖下载步骤。

生成完整发布包需要安装 [Inno Setup](https://jrsoftware.org/isinfo.php)，然后运行：

```powershell
.\package.ps1 -IsccPath 'C:\Program Files\Inno Setup 7\ISCC.exe'
```

脚本默认重新编译并运行自检，在 `build\release` 中生成带版本号的便携包、安装包及校验文件。版本号读取自 `src\Program.cs`。

自检覆盖提醒边界、重复弹窗防护、暂停恢复、稍后提醒、时间大幅跳跃、暂停时修改间隔、设置持久化与损坏恢复、窗口关闭行为、主题和字体切换、数值控件布局，以及真实的 5 秒自动关闭。测试结果写入 `build\test-results.txt`。

需要重新生成界面预览时：

```powershell
Start-Process .\build\EyeRest.exe -ArgumentList '--render-preview', "$PWD\artifacts" -Wait
```

`artifacts` 内包含四套主题的设置页和提醒窗，以及大字号、500 字长文本的渲染结果。艺术层预览为 `settings-3.png` 和 `reminder-3.png`。自检和预览模式不会改写个人设置。

## 代码结构

- `src/Core.cs`：设置、原子保存、计时状态机。
- `src/Program.cs`：入口、单实例、托盘、提醒调度。
- `src/SettingsForm.cs`：设置 GUI。
- `src/ReminderForm.cs`：休息提醒和倒计时。
- `src/Styles.cs`：主题、颜色选择和绘制。
- `src/ArtTheme.cs`：艺术层插画的延迟加载与书库装饰。
- `src/WindowTheme.cs`：主题标题栏、字体选择框、数值按钮和托盘菜单绘制。
- `src/Startup.cs`：当前用户自启项及设置保存失败时的恢复。
- `src/Quotes.cs`：语录集编辑、顺序与随机选择。
- `src/SelfTests.cs`：自检和界面渲染。

后台使用每秒一次的 WinForms 定时器和单调时钟，不进行忙循环。休息弹窗存在期间每 250 毫秒更新倒计时。实际内存占用包含系统 CLR、字体和窗口资源，不能用 EXE 大小代替内存占用。

---

<div align="center">

### 🌱 来这里歇一会儿的人

![EyeRest 页面浏览计数](https://count.kjchmc.cn/get/@Jax367-EyeRest?theme=asoul)

<sub>Powered by <a href="https://github.com/journey-ad/Moe-Counter">Moe Counter</a> · <a href="https://count.kjchmc.cn/">计数服务</a> · 统计图片加载次数，非独立访客人数。</sub>

**Made by Lar · Take a break, then keep going.**

</div>
