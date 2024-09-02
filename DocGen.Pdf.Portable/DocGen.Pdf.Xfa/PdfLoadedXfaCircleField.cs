using System.Xml;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Xfa;

internal class PdfLoadedXfaCircleField : PdfLoadedXfaStyledField
{
	private int m_sweepAngle;

	private int m_startAngle;

	private PdfXfaCircleAppearance m_Appearance;

	private string m_toolTip = string.Empty;

	private PdfXfaRotateAngle m_rotate;

	private float m_width;

	private float m_height;

	private PdfXfaBorder m_border = new PdfXfaBorder();

	internal new PdfXfaBorder Border
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

	internal new string ToolTip
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

	internal int StartAngle
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

	internal int SweepAngle
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

	internal PdfXfaCircleAppearance Appearance
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

	internal new PdfXfaRotateAngle Rotate
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

	internal new float Width
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

	internal new float Height
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

	internal void ReadField(XmlNode node)
	{
		ReadCommonProperties(node);
		Appearance = PdfXfaCircleAppearance.Ellipse;
		if (node["value"] == null || node["value"]["arc"] == null)
		{
			return;
		}
		XmlNode xmlNode = node["value"]["arc"];
		if (xmlNode.Attributes["sweepAngle"] != null)
		{
			SweepAngle = int.Parse(xmlNode.Attributes["sweepAngle"].Value);
			if (SweepAngle != 0)
			{
				Appearance = PdfXfaCircleAppearance.Arc;
			}
		}
		if (xmlNode.Attributes["startAngle"] != null)
		{
			StartAngle = int.Parse(xmlNode.Attributes["startAngle"].Value);
			if (StartAngle != 0)
			{
				Appearance = PdfXfaCircleAppearance.Arc;
			}
		}
		if (xmlNode.Attributes["circular"] != null)
		{
			Appearance = PdfXfaCircleAppearance.Circle;
		}
		if (xmlNode["fill"] != null)
		{
			if (Border == null)
			{
				Border = new PdfXfaBorder();
			}
			Border.ReadFillBrush(xmlNode);
		}
	}

	internal void DrawCircle(PdfGraphics graphics, RectangleF bounds)
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
			pen = new PdfPen(Border.Color.IsEmpty ? new PdfColor(Color.Black) : Border.Color, Border.Width);
		}
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
}
