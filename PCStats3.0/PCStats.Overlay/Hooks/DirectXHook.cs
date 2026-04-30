using System;
using System.Collections.Generic;
using GameOverlay.Drawing;
using GameOverlay.Windows;
using PCStats.Shared.Models;
using PCStats.Overlay.Rendering;

namespace PCStats.Overlay.Hooks
{
    public class DirectXHook : IDisposable
    {
        private readonly GraphicsWindow _window;
        private readonly Graphics _graphics;
        private List<SensorData> _currentData;
        private TextRenderer _renderer;

        public DirectXHook()
        {
            _graphics = new Graphics()
            {
                MeasureFPS = true,
                PerPrimitiveAntiAliasing = true,
                TextAntiAliasing = true
            };

            _window = new GraphicsWindow(0, 0, 400, 300, _graphics)
            {
                FPS = 60,
                IsTopmost = true,
                IsVisible = true,
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
        }

        public void Stop()
        {
            _window.Hide();
        }

        public void UpdateData(List<SensorData> data)
        {
            _currentData = data;
        }

        private void SetupGraphics(object sender, SetupGraphicsEventArgs e)
        {
            _renderer.Setup(e.Graphics);
        }

        private void DestroyGraphics(object sender, DestroyGraphicsEventArgs e)
        {
            _renderer?.Dispose();
        }

        private void DrawGraphics(object sender, DrawGraphicsEventArgs e)
        {
            _renderer.Draw(e.Graphics, _currentData);
        }

        public void Dispose()
        {
            _window?.Dispose();
            _graphics?.Dispose();
            _renderer?.Dispose();
        }
    }
}