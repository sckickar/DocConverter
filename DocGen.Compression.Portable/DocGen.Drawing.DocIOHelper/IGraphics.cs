using System;
using System.IO;
using DocGen.Office;

namespace DocGen.Drawing.DocIOHelper;

internal interface IGraphics : IDisposable
{
	TextRenderingHint TextRenderingHint { get; set; }

	SmoothingMode SmoothingMode { get; set; }

	InterpolationMode InterpolationMode { get; set; }

	GraphicsUnit PageUnit { get; set; }

	Matrix Transform { get; set; }

	RectangleF ClipBounds { get; }

	SizeF MeasureString(string text, Font font, FontStyle fontStyle, FontScriptType scriptType);

	SizeF MeasureString(string text, Font font, PointF pointF, StringFormat format, FontStyle fontStyle, FontScriptType scriptType);

	void FillRectangle(Color color, Rectangle rectangle);

	void FillRectangle(IBrush brush, Rectangle rectangle);

	void FillRectangle(IBrush brush, RectangleF rectangle);

	void FillRectangle(IBrush brush, float x, float y, float width, float height);

	void FillPath(IBrush brush, IGraphicsPath gPath);

	void FillPolygon(IBrush pen, PointF[] points);

	void DrawImage(byte[] imageBytes, Rectangle rectangle);

	void DrawImage(IImage image, RectangleF rectangle);

	void DrawImage(IImage image, float x, float y, float width, float height);

	void DrawImage(IImage image, RectangleF destRect, RectangleF srcRect, GraphicsUnit unit);

	void DrawImage(IImage image, Rectangle destRect, float x, float y, float width, float height, GraphicsUnit unit, IImageAttributes attr);

	void DrawLine(IPen pen, PointF pt1, PointF pt2);

	void DrawString(string text, Font font, IBrush brush, RectangleF rectangle);

	void DrawString(string text, Font font, IBrush brush, RectangleF rectangle, StringFormat stringFormat, FontScriptType scriptType);

	void DrawString(string text, Font font, IBrush brush, float x, float y, StringFormat stringFormat);

	void DrawString(string text, Font font, IBrush solidBrush, int x, int y, FontStyle fontStyle, FontScriptType scriptType);

	void DrawPath(IPen pen, IGraphicsPath path);

	void DrawRectangle(IPen p, float x, float y, float width, float height);

	void SetClip(RectangleF rect, CombineMode mode);

	void TranslateTransform(float x, float y);

	void RotateTransform(float degree);

	void ResetTransform();

	void ResetClip();

	void Clear(Color color);

	void ExportAsImage(ImageFormat imageFormat, MemoryStream memoryStream);
}
