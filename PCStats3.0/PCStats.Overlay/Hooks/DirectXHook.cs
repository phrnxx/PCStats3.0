using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using GameOverlay.Drawing;
using GameOverlay.Windows;
using PCStats.Shared.Models;
using PCStats.Overlay.Rendering;

namespace PCStats.Overlay.Hooks
{
    public class DirectXHook : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        private const int VK_MENU = 0x12;
        private const int VK_P = 0x50;

        private readonly GraphicsWindow _window;
        private readonly Graphics _graphics;
        private List<SensorData> _currentData;
        private TextRenderer _renderer;

        // ФИКС: Оверлей снова скрыт при старте
        private bool _isOverlayVisible = false;
        private CancellationTokenSource _hotkeyCts;

        public DirectXHook()
        {
            _graphics = new Graphics() { PerPrimitiveAntiAliasing = true, TextAntiAliasing = true };

            _window = new GraphicsWindow(0, 0, 450, 900, _graphics)
            {
                FPS = 60,
                IsTopmost = true,
                IsVisible = false, // ФИКС: Не показываем сразу
                X = 20,
                Y = 20
            };

            _window.SetupGraphics += SetupGraphics;
            _window.DrawGraphics += DrawGraphics;
            _window.DestroyGraphics += DestroyGraphics;

            _currentData = new List<SensorData>();
            _renderer = new TextRenderer();
        }

        public void Start()
        {
            _window.Create();
            _hotkeyCts = new CancellationTokenSource();
            Task.Run(() => HotkeyLoop(_hotkeyCts.Token));
        }

        public void Stop()
        {
            _hotkeyCts?.Cancel();
            _window.Hide();
        }

        private async Task HotkeyLoop(CancellationToken token)
        {
            bool wasPressed = false;
            while (!token.IsCancellationRequested)
            {
                bool isAltPressed = (GetAsyncKeyState(VK_MENU) & 0x8000) != 0;
                bool isPPressed = (GetAsyncKeyState(VK_P) & 0x8000) != 0;

                if (isAltPressed && isPPressed)
                {
                    if (!wasPressed)
                    {
                        wasPressed = true;
                        _isOverlayVisible = !_isOverlayVisible;
                        if (_isOverlayVisible) _window.Show(); else _window.Hide();
                    }
                }
                else wasPressed = false;

                await Task.Delay(50, token);
            }
        }

        public void UpdateData(List<SensorData> data) => _currentData = data;
        private void SetupGraphics(object sender, SetupGraphicsEventArgs e) => _renderer.Setup(e.Graphics);
        private void DestroyGraphics(object sender, DestroyGraphicsEventArgs e) => _renderer?.Dispose();
        private void DrawGraphics(object sender, DrawGraphicsEventArgs e) => _renderer.Draw(e.Graphics, _currentData);

        public void Dispose()
        {
            _hotkeyCts?.Cancel();
            _window?.Dispose();
            _graphics?.Dispose();
            _renderer?.Dispose();
        }
    }
}