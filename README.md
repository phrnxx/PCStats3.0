PCStats 3.0
English
Overview
PCStats 3.0 is a lightweight, high-performance hardware monitoring tool built for gamers, enthusiasts, and system developers. Designed to operate with zero performance overhead, it provides real-time telemetry of your computer's health through a customizable overlay and dashboard.

Features
Low-Overhead Engine: Collects telemetry via high-speed named pipes without the CPU spikes associated with traditional JSON-based monitoring.

DirectX HUD Overlay: A non-intrusive, hardware-accelerated overlay that displays critical stats over your games.

Broad Hardware Support: Tracks temperatures, loads, and frequencies across processors, graphics cards, and storage devices.

Installation & Setup
Download the latest setup executable (PCStats_Modern_Setup.exe) from the Releases page.

Run the installer as an administrator to ensure appropriate access to the hardware monitoring endpoints.

Launch PCStats3.0.exe. The core and dashboard will start automatically.

Usage
Launch the application from the desktop or the system tray.

Toggle HUD Overlay: Press Alt + P in-game to show or hide the statistics.

Customization: Use the Dashboard interface to cycle through various atmospheric backgrounds.

Architecture
The project consists of three main modules:
```text
PCStats3.0/
├── .gitattributes
├── .gitignore
├── App.manifest
├── FodyWeavers.xml
├── PCStats3.0.csproj
├── PCStats3.0.slnx
├── Program.cs
├── packages.config
├── PCStats.Core/
│   ├── Benchmarking/
│   │   ├── CpuStresser.cs
│   │   └── SensorReader.cs
│   ├── CoreService.cs
│   ├── Hardware/
│   │   └── CpuStresser.cs
│   └── IPC/
│       ├── ClientIPC.cs
│       └── DataServer.cs
├── PCStats.Overlay/
│   ├── Hooks/
│   │   └── DirectXHook.cs
│   ├── OverlayClient.cs
│   └── Rendering/
│       └── TextRenderer.cs
├── PCStats.Shared/
│   ├── Interfaces/
│   │   └── IHardwareMonitor.cs
│   └── Models/
│       └── SensorData.cs
└── PCStats.UI/
    ├── App.xaml
    ├── Background/
    │   └── Background01.jpg
    ├── ViewModels/
    │   └── DashboardViewModel.cs
    └── Views/
        ├── DashboardWindow.xaml
        └── DashboardWindow.xaml.cs
```
PCStats.Core: Handles LibreHardwareMonitor telemetry and inter-process communication via named pipes.

PCStats.Overlay: DirectX-based rendering for in-game telemetry.

PCStats.UI: A WPF-based user interface displaying real-time data using custom styling.

Українська
Огляд
PCStats 3.0 — це високопродуктивна утиліта для моніторингу апаратного забезпечення в реальному часі, створена для геймерів та розробників. Працює з мінімальним навантаженням на систему та надає точну інформацію про стан вашого ПК.

Можливості
Висока швидкість передачі даних: Збір телеметрії через іменовані канали (Named Pipes) без затримок та стрибків завантаження процесора.

Оверлей для ігор (DirectX HUD): Ненав'язливий оверлей, який відображає основні показники поверх ігор.

Підтримка багатьох пристроїв: Зчитування даних із процесорів (AMD, Intel), відеокарт, оперативної пам'яті та SSD-накопичувачів.

Встановлення та запуск
Завантажте останню версію інсталятора (PCStats_Modern_Setup.exe) з репозиторію.

Запустіть файл від імені адміністратора для надання прав на читання датчиків.

Запустіть PCStats3.0.exe.

Використання
Керування оверлеєм: Використовуйте комбінацію клавіш Alt + P для ввімкнення та вимкнення HUD.

Зміна фону: Використовуйте інтерфейс головної панелі для перемикання між атмосферними зображеннями.

Структура
```text
PCStats3.0/
├── .gitattributes
├── .gitignore
├── App.manifest
├── FodyWeavers.xml
├── PCStats3.0.csproj
├── PCStats3.0.slnx
├── Program.cs
├── iso22.ico
├── packages.config
├── PCStats.Core/
│   ├── Benchmarking/
│   │   ├── CpuStresser.cs
│   │   └── SensorReader.cs
│   ├── CoreService.cs
│   ├── Hardware/
│   │   └── CpuStresser.cs
│   └── IPC/
│       ├── ClientIPC.cs
│       └── DataServer.cs
├── PCStats.Overlay/
│   ├── Hooks/
│   │   └── DirectXHook.cs
│   ├── OverlayClient.cs
│   └── Rendering/
│       └── TextRenderer.cs
├── PCStats.Shared/
│   ├── Interfaces/
│   │   └── IHardwareMonitor.cs
│   └── Models/
│       └── SensorData.cs
└── PCStats.UI/
    ├── App.xaml
    ├── Background/
    │   └── Background01.jpg
    ├── ViewModels/
    │   └── DashboardViewModel.cs
    └── Views/
        ├── DashboardWindow.xaml
        └── DashboardWindow.xaml.cs
```
PCStats.Core: Робота з бібліотекою LibreHardwareMonitor, керування потоками та IPC.

PCStats.Overlay: Модуль рендерингу оверлея поверх DirectX.

PCStats.UI: Головний користувацький інтерфейс на базі WPF.
