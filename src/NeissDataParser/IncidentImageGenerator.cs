using System.Globalization;
using SkiaSharp;

namespace NeissDataParser;

public class IncidentImageGenerator
{
    private const int WIDTH = 800;
    private const int HEIGHT = 600;
    private const int PADDING = 20;
    private const int TITLE_TOP_PADDING = 20;
    private const float TITLE_FONT_SIZE = 24;
    private const float BODY_FONT_SIZE = 16;
    private const float DATE_FONT_SIZE = 14;
    private const int DATE_BOTTOM_MARGIN = 40; // Space reserved for date at bottom

    public static void GenerateImage(IncidentRecord record, string outputPath)
    {
        using (var surface = SKSurface.Create(new SKImageInfo(WIDTH, HEIGHT)))
        {
            var canvas = surface.Canvas;

            // Clear background
            canvas.Clear(SKColors.White);

            using (var titlePaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = TITLE_FONT_SIZE,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                TextAlign = SKTextAlign.Left
            })
            using (var datePaint = new SKPaint
            {
                Color = SKColors.Gray,
                TextSize = DATE_FONT_SIZE,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                TextAlign = SKTextAlign.Right
            })
            {
                // Draw title (diagnoses)
                var primaryDiagnosis = DiagnosisCodeLookup.GetDiagnosis(record.Diagnosis);
                string titleText = $"Diagnosis: {primaryDiagnosis}";
                if (record.Diagnosis2.HasValue)
                {
                    var secondaryDiagnosis = DiagnosisCodeLookup.GetDiagnosis(record.Diagnosis2.Value);
                    titleText += $"\nSecondary: {secondaryDiagnosis}";
                }

                // Draw title with word wrap
                float currentY = TITLE_TOP_PADDING;
                var titleLines = WrapText(titleText, WIDTH - 2 * PADDING, titlePaint);
                foreach (var line in titleLines)
                {
                    canvas.DrawText(line, PADDING, currentY + TITLE_FONT_SIZE, titlePaint);
                    currentY += TITLE_FONT_SIZE + 5;
                }

                // Add some space after title
                currentY += 20;

                // Calculate available space for narrative
                float availableHeight = HEIGHT - currentY - DATE_BOTTOM_MARGIN;
                float availableWidth = WIDTH - 2 * PADDING;

                // Find optimal font size for narrative
                float optimalFontSize = FindOptimalFontSize(
                    record.Narrative,
                    availableWidth,
                    availableHeight,
                    12, // min font size
                    48  // max font size
                );

                // Create paint with optimal font size
                using (var bodyPaint = new SKPaint
                {
                    Color = SKColors.Black,
                    TextSize = optimalFontSize,
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill,
                    TextAlign = SKTextAlign.Left
                })
                {
                    // Draw narrative with optimal font size
                    var narrativeLines = WrapText(record.Narrative, availableWidth, bodyPaint);
                    float lineHeight = bodyPaint.FontSpacing;
                    float totalTextHeight = narrativeLines.Count() * lineHeight;

                    // Center text vertically in available space
                    float startY = currentY + (availableHeight - totalTextHeight) / 2;

                    foreach (var line in narrativeLines)
                    {
                        canvas.DrawText(line, PADDING, startY + bodyPaint.FontSpacing, bodyPaint);
                        startY += lineHeight;
                    }
                }

                // Draw date
                canvas.DrawText(
                    record.TreatmentDate.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture),
                    WIDTH - PADDING,
                    HEIGHT - PADDING,
                    datePaint
                );

                // Draw border
                using (var borderPaint = new SKPaint
                {
                    Color = SKColors.LightGray,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 2
                })
                {
                    canvas.DrawRect(0, 0, WIDTH, HEIGHT, borderPaint);
                }
            }

            // Save the image
            using (var image = surface.Snapshot())
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            using (var stream = File.OpenWrite(outputPath))
            {
                data.SaveTo(stream);
            }
        }
    }

    private static float FindOptimalFontSize(string text, float maxWidth, float maxHeight, float minSize, float maxSize)
    {
        float low = minSize;
        float high = maxSize;
        float optimal = minSize;

        while (high - low > 0.5f)
        {
            float mid = (low + high) / 2;

            using (var paint = new SKPaint { TextSize = mid })
            {
                var lines = WrapText(text, maxWidth, paint).ToList();
                float totalHeight = lines.Count * paint.FontSpacing;

                if (totalHeight <= maxHeight)
                {
                    optimal = mid;
                    low = mid;
                }
                else
                {
                    high = mid;
                }
            }
        }

        return optimal;
    }

    private static IEnumerable<string> WrapText(string text, float maxWidth, SKPaint paint)
    {
        var words = text.Split(' ');
        var lines = new List<string>();
        var currentLine = new List<string>();
        float currentWidth = 0;

        foreach (var word in words)
        {
            float wordWidth = paint.MeasureText(word + " ");
            if (currentWidth + wordWidth > maxWidth)
            {
                if (currentLine.Count > 0)
                {
                    lines.Add(string.Join(" ", currentLine));
                    currentLine.Clear();
                    currentWidth = 0;
                }
            }
            currentLine.Add(word);
            currentWidth += wordWidth;
        }

        if (currentLine.Count > 0)
        {
            lines.Add(string.Join(" ", currentLine));
        }

        return lines;
    }
}