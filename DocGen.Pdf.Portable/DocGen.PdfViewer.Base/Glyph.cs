using System;
using DocGen.Drawing;
using DocGen.Pdf.Parsing;

namespace DocGen.PdfViewer.Base;

internal class Glyph : DocGen.Pdf.Parsing.Glyph
{
	private double descent;

	private string m_embededFontFamily;

	private double m_matrixFontSize;

	internal string EmbededFontFamily
	{
		get
		{
			return m_embededFontFamily;
		}
		set
		{
			m_embededFontFamily = value;
		}
	}

	internal double MatrixFontSize
	{
		get
		{
			return m_matrixFontSize;
		}
		set
		{
			m_matrixFontSize = value;
		}
	}

	internal bool IsReplace { get; set; }

	public int FontId { get; set; }

	public CharCode CharId { get; set; }

	public string ToUnicode { get; set; }

	public string FontFamily { get; set; }

	public FontStyle FontStyle { get; set; }

	public bool IsBold { get; set; }

	public bool IsItalic { get; set; }

	public Color TextColor { get; set; }

	public double FontSize { get; set; }

	public double Rise { get; set; }

	public double CharSpacing { get; set; }

	public double WordSpacing { get; set; }

	public double HorizontalScaling { get; set; }

	public double Width { get; set; }

	public double Ascent { get; set; }

	public double Descent
	{
		get
		{
			return descent;
		}
		set
		{
			if (value > 0.0)
			{
				descent = 0.0 - value;
			}
			else
			{
				descent = value;
			}
		}
	}

	public bool IsStroked { get; set; }

	public Matrix TransformMatrix { get; set; }

	public Size Size { get; set; }

	public Rect BoundingRect { get; set; }

	public bool IsSpace
	{
		get
		{
			if (CharId.BytesCount == 1)
			{
				return CharId.Bytes[0] == 32;
			}
			return false;
		}
	}

	public int ZIndex { get; set; }

	public PathGeometry Clip { get; set; }

	public bool HasChildren => false;

	public double StrokeThickness { get; set; }

	public bool IsRotated { get; set; }

	public int RotationAngle { get; set; }

	public Glyph()
	{
		Ascent = 1000.0;
		Descent = 0.0;
		TransformMatrix = Matrix.Identity;
	}

	public Rect Arrange(Matrix transformMatrix)
	{
		BoundingRect = PdfHelper.GetBoundingRect(new Rect(new Point(0.0, 0.0), new Size(Width, Math.Max(1.0, (Ascent - 2.0 * Descent) / 1000.0))), TransformMatrix * transformMatrix);
		return BoundingRect;
	}

	public override string ToString()
	{
		return ToUnicode;
	}
}
