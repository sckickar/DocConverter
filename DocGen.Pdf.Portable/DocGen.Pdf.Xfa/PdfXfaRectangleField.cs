using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Xfa;

public class PdfXfaRectangleField : PdfXfaField
{
	private PdfXfaCorner m_corner = new PdfXfaCorner();

	private float m_radius;

	private PdfXfaRotateAngle m_rotate;

	private PdfXfaBorder m_border = new PdfXfaBorder();

	private string m_toolTip;

	private float m_width;

	private float m_height;

	internal PdfXfaForm parent;

	public PdfXfaBorder Border
	{
		get
		{
			return m_border;
		}
		set
		{
			if (value != null)
			{
				m_border = value;
			}
		}
	}

	public string ToolTip
	{
		get
		{
			return m_toolTip;
		}
		set
		{
			if (value != null)
			{
				m_toolTip = value;
			}
		}
	}

	public PdfXfaCorner Corner
	{
		get
		{
			return m_corner;
		}
		set
		{
			m_corner = value;
		}
	}

	public PdfXfaRotateAngle Rotate
	{
		get
		{
			return m_rotate;
		}
		set
		{
			m_rotate = value;
		}
	}

	public float Width
	{
		get
		{
			return m_width;
		}
		set
		{
			m_width = value;
		}
	}

	public float Height
	{
		get
		{
			return m_height;
		}
		set
		{
			m_height = value;
		}
	}

	public PdfXfaRectangleField(string name, SizeF size)
	{
		base.Name = name;
		Width = size.Width;
		Height = size.Height;
	}

	public PdfXfaRectangleField(string name, float width, float height)
	{
		base.Name = name;
		Width = width;
		Height = height;
	}

	internal void Save(XfaWriter xfaWriter)
	{
		if (base.Name == "" || base.Name == string.Empty)
		{
			base.Name = "Rectangle" + xfaWriter.m_fieldCount++;
		}
		xfaWriter.Write.WriteStartElement("draw");
		xfaWriter.Write.WriteAttributeString("name", base.Name);
		xfaWriter.SetSize(Height, Width, 0f, 0f);
		xfaWriter.SetRPR(Rotate, base.Visibility, isReadOnly: false);
		xfaWriter.Write.WriteStartElement("value");
		xfaWriter.Write.WriteStartElement("rectangle");
		if (Border.FillColor != null)
		{
			xfaWriter.DrawFillColor(Border.FillColor);
		}
		_ = string.Empty;
		xfaWriter.DrawBorder(Border, isSkip: true);
		if (Corner != null)
		{
			xfaWriter.DrawCorner(Corner);
		}
		xfaWriter.Write.WriteEndElement();
		xfaWriter.Write.WriteEndElement();
		xfaWriter.WriteMargins(base.Margins);
		if (ToolTip != null)
		{
			xfaWriter.WriteToolTip(ToolTip);
		}
		xfaWriter.Write.WriteEndElement();
	}

	internal void SaveAcroForm(PdfPage page, RectangleF bounds)
	{
		RectangleF rectangleF = default(RectangleF);
		SizeF size = GetSize();
		rectangleF = new RectangleF(new PointF(bounds.Location.X + base.Margins.Left, bounds.Location.Y + base.Margins.Top), new SizeF(size.Width - (base.Margins.Right + base.Margins.Left), size.Height - (base.Margins.Top + base.Margins.Bottom)));
		PdfPen pen = PdfPens.Black;
		if (Border != null)
		{
			pen = new PdfPen(Border.Color, Border.Width);
		}
		PdfGraphics graphics = page.Graphics;
		graphics.Save();
		graphics.TranslateTransform(rectangleF.X, rectangleF.Y);
		graphics.RotateTransform(-GetRotationAngle());
		RectangleF rectangleF2 = RectangleF.Empty;
		switch (GetRotationAngle())
		{
		case 180:
			rectangleF2 = new RectangleF(0f - rectangleF.Width, 0f - rectangleF.Height, rectangleF.Width, rectangleF.Height);
			break;
		case 90:
			rectangleF2 = new RectangleF(0f - rectangleF.Height, 0f, rectangleF.Height, rectangleF.Width);
			break;
		case 270:
			rectangleF2 = new RectangleF(0f, 0f - rectangleF.Width, rectangleF.Height, rectangleF.Width);
			break;
		case 0:
			rectangleF2 = new RectangleF(0f, 0f, rectangleF.Width, rectangleF.Height);
			break;
		}
		PdfBrush brush = Border.GetBrush(rectangleF2);
		if (base.Visibility != 0)
		{
			pen = null;
			brush = null;
		}
		graphics.DrawRectangle(pen, brush, rectangleF2);
		graphics.Restore();
	}

	private int GetRotationAngle()
	{
		int result = 0;
		if (Rotate != 0)
		{
			switch (Rotate)
			{
			case PdfXfaRotateAngle.RotateAngle180:
				result = 180;
				break;
			case PdfXfaRotateAngle.RotateAngle270:
				result = 270;
				break;
			case PdfXfaRotateAngle.RotateAngle90:
				result = 90;
				break;
			}
		}
		return result;
	}

	internal SizeF GetSize()
	{
		if (Rotate == PdfXfaRotateAngle.RotateAngle270 || Rotate == PdfXfaRotateAngle.RotateAngle90)
		{
			return new SizeF(Height, Width);
		}
		return new SizeF(Width, Height);
	}

	public object Clone()
	{
		return MemberwiseClone();
	}
}
