using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Xfa;

public class PdfXfaCircleField : PdfXfaField
{
	private int m_sweepAngle;

	private int m_startAngle;

	private PdfXfaCircleAppearance m_Appearance;

	private string m_toolTip = string.Empty;

	private PdfXfaRotateAngle m_rotate;

	private float m_width;

	private float m_height;

	private PdfXfaBorder m_border = new PdfXfaBorder();

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

	public int StartAngle
	{
		get
		{
			return m_startAngle;
		}
		set
		{
			m_startAngle = value;
		}
	}

	public int SweepAngle
	{
		get
		{
			return m_sweepAngle;
		}
		set
		{
			m_sweepAngle = value;
		}
	}

	public PdfXfaCircleAppearance Appearance
	{
		get
		{
			return m_Appearance;
		}
		set
		{
			m_Appearance = value;
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

	public PdfXfaCircleField(string name, SizeF size)
	{
		base.Name = name;
		Width = size.Width;
		Height = size.Height;
	}

	public PdfXfaCircleField(string name, float width, float height)
	{
		base.Name = name;
		Width = width;
		Height = height;
	}

	internal void Save(XfaWriter xfaWriter)
	{
		if (base.Name == "" || base.Name == string.Empty)
		{
			base.Name = "Circle" + xfaWriter.m_fieldCount++;
		}
		xfaWriter.Write.WriteStartElement("draw");
		xfaWriter.Write.WriteAttributeString("name", base.Name);
		xfaWriter.SetSize(Height, Width, 0f, 0f);
		xfaWriter.SetRPR(Rotate, base.Visibility, isReadOnly: false);
		xfaWriter.Write.WriteStartElement("value");
		xfaWriter.Write.WriteStartElement("arc");
		switch (Appearance)
		{
		case PdfXfaCircleAppearance.Arc:
			xfaWriter.Write.WriteAttributeString("sweepAngle", SweepAngle.ToString());
			xfaWriter.Write.WriteAttributeString("startAngle", StartAngle.ToString());
			break;
		case PdfXfaCircleAppearance.Circle:
			xfaWriter.Write.WriteAttributeString("circular", "1");
			break;
		}
		if (Border.FillColor != null)
		{
			xfaWriter.DrawFillColor(Border.FillColor);
		}
		if (Border != null)
		{
			xfaWriter.DrawBorder(Border, isSkip: true);
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
		if (Appearance == PdfXfaCircleAppearance.Circle)
		{
			if (rectangleF.Width > rectangleF.Height)
			{
				rectangleF.X += (rectangleF.Height + rectangleF.Width) / 2f - rectangleF.Height;
				rectangleF.Width = rectangleF.Height;
			}
			else
			{
				rectangleF.Y += (rectangleF.Height + rectangleF.Width) / 2f - rectangleF.Width;
				rectangleF.Height = rectangleF.Width;
			}
		}
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
		if (Appearance == PdfXfaCircleAppearance.Arc)
		{
			graphics.DrawArc(pen, rectangleF2, m_startAngle, m_sweepAngle);
		}
		else
		{
			graphics.DrawEllipse(pen, brush, rectangleF2);
		}
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
