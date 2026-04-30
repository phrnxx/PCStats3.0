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
            _titleFont = gfx.CreateFont("Consolas", 24, true);
            _textBrush = gfx.CreateSolidBrush(200, 200, 200);
            _valueBrush = gfx.CreateSolidBrush(218, 165, 32); // Золото
            _bgBrush = gfx.CreateSolidBrush(13, 13, 13, 220); // Слегка темнее для читаемости
        }

        public void Draw(Graphics gfx, List<SensorData> data)
        {
            gfx.ClearScene();

            float startX = 25f;
            float startY = 25f;
            float dataOffsetY = 26f;
            float windowWidth = 430f;

            // Если данных нет, рисуем маленькое окно. Если есть - растягиваем.
            float windowHeight = (data == null || data.Count == 0) ? 120f : (data.Count * dataOffsetY) + 80f;

            // РИСУЕМ ФОН ВСЕГДА
            gfx.FillRoundedRectangle(_bgBrush, 0, 0, windowWidth, windowHeight, 15);
            gfx.DrawRoundedRectangle(_valueBrush, 0, 0, windowWidth, windowHeight, 15, 1);
            gfx.DrawText(_titleFont, _valueBrush, startX, startY, "PCStats 3.0");

            // Если список пуст, выводим статус
            if (data == null || data.Count == 0)
            {
                gfx.DrawText(_font, _textBrush, startX, startY + 45f, "Ожидание данных от ядра...");
                return;
            }

            float currentY = startY + 45f;
            foreach (var item in data)
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