using SkiaSharp;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Nevrona
{
    public class Chart
    {
        public static void Show(int width, int height, IEnumerable<float> values)
        {
            var bmp = new SKBitmap(width, height, true);

            var cnv = new SKCanvas(bmp);

            cnv.Clear(SKColor.Parse("000000"));

            var vArray = values.ToArray();

            var yMax = vArray.Max();
            var yMin = vArray.Min();
            var yRange = yMax - yMin;
            var xRange = vArray.Length;

            cnv.DrawPoints(SKPointMode.Polygon,
                vArray
                .Select((v, i) =>
                    new SKPoint(
                        i * width / xRange,
                        height - (v - yMin) * height / yRange))
                .ToArray(),
                new SKPaint()
                {
                    StrokeWidth = 3f,
                    IsAntialias = true,
                    Color = SKColor.Parse("FF8000")
                });

            cnv.DrawRect(0f, 0f, width - 1, height - 1, new SKPaint()
            {
                IsStroke = true,
                Color = SKColor.Parse("FFFFFF")
            });

            var tmpFileName = Path.GetTempPath() + "NevronaImage.png";
            var tmpFile = File.Create(tmpFileName);

            SKImage.FromBitmap(bmp).Encode(SKEncodedImageFormat.Png, 80)
                .SaveTo(tmpFile);

            tmpFile.Close();

            Process.Start("explorer.exe", tmpFileName);
        }
    }
}