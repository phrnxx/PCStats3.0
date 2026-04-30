using System;
using System.Collections.Generic;
using System.Linq;
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
            _titleFont = gfx.CreateFont("Consolas", 24, true);
            _textBrush = gfx.CreateSolidBrush(200, 200, 200);
            _valueBrush = gfx.CreateSolidBrush(218, 165, 32); 
            _bgBrush = gfx.CreateSolidBrush(13, 13, 13, 220); 
        }

        public void Draw(Graphics gfx, List<SensorData> data)
        {
            gfx.ClearScene();

            if (data == null || data.Count == 0)
            {
                gfx.FillRoundedRectangle(_bgBrush, 0, 0, 430, 120, 15);
                gfx.DrawRoundedRectangle(_valueBrush, 0, 0, 430, 120, 15, 1);
                gfx.DrawText(_titleFont, _valueBrush, 25, 25, "PCStats 3.0");
                gfx.DrawText(_font, _textBrush, 25, 70, "Ожидание данных от ядра...");
                return;
            }

            var filteredData = data.Where(d =>
            {
                string name = d.HardwareName.ToLower();
                return name.Contains("cpu") || name.Contains("intel") || name.Contains("amd") ||
                       name.Contains("gpu") || name.Contains("nvidia") || name.Contains("radeon") ||
                       name == "ram";
            }).ToList();

            float startX = 25f;
            float startY = 25f;
            float dataOffsetY = 26f;
            float windowWidth = 430f;

            float windowHeight = (filteredData.Count * dataOffsetY) + 85f;

            gfx.FillRoundedRectangle(_bgBrush, 0, 0, windowWidth, windowHeight, 15);
            gfx.DrawRoundedRectangle(_valueBrush, 0, 0, windowWidth, windowHeight, 15, 1);
            gfx.DrawText(_titleFont, _valueBrush, startX, startY, "PCStats HUD");

            float currentY = startY + 50f;
            foreach (var item in filteredData)
            {
                gfx.DrawText(_font, _textBrush, startX, currentY, item.SensorName);
                gfx.DrawText(_font, _valueBrush, startX + 290, currentY, item.Value);
                currentY += dataOffsetY;
            }
        }

        public void Dispose()
        {
            _font?.Dispose(); _titleFont?.Dispose();
            _textBrush?.Dispose(); _valueBrush?.Dispose(); _bgBrush?.Dispose();
        }
    }
}