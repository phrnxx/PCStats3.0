using System;
using System.Collections.Generic;
using GameOverlay.Drawing;
using PCStats.Shared.Models;

namespace PCStats.Overlay.Rendering
{
    public class TextRenderer : IDisposable
    {
        private Font _font;
        private Font _titleFont;
        private SolidBrush _textBrush;
        private SolidBrush _valueBrush;
        private SolidBrush _bgBrush;

        public void Setup(Graphics gfx)
        {
            _font = gfx.CreateFont("Consolas", 14);
            _titleFont = gfx.CreateFont("Consolas", 16, true);
            _textBrush = gfx.CreateSolidBrush(200, 200, 200);
            _valueBrush = gfx.CreateSolidBrush(0, 255, 204);
            _bgBrush = gfx.CreateSolidBrush(13, 13, 13, 220);
        }

        public void Draw(Graphics gfx, List<SensorData> data)
        {
            gfx.ClearScene();

            if (data == null || data.Count == 0) return;

            float startX = 15f;
            float startY = 15f;
            float offsetY = 22f;

            gfx.FillRectangle(_bgBrush, 0, 0, 350, (data.Count * offsetY) + 50);

            gfx.DrawText(_titleFont, _valueBrush, startX, startY, "PCStats 3.0 [HUD]");

            float currentY = startY + 30f;

            foreach (var item in data)
            {
                gfx.DrawText(_font, _textBrush, startX, currentY, item.SensorName);
                gfx.DrawText(_font, _valueBrush, startX + 240, currentY, item.Value);
                currentY += offsetY;
            }
        }

        public void Dispose()
        {
            _font?.Dispose();
            _titleFont?.Dispose();
            _textBrush?.Dispose();
            _valueBrush?.Dispose();
            _bgBrush?.Dispose();
        }
    }
}