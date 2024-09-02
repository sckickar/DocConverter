using System.Xml;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Xfa;

internal class PdfLoadedXfaRectangleField : PdfLoadedXfaStyledField
{
	private PdfXfaCorner m_corner = new PdfXfaCorner();

	private float m_radius;

	private PdfXfaRotateAngle m_rotate;

	private PdfXfaBorder m_border = new PdfXfaBorder();

	private string m_toolTip;

	public new PdfXfaBorder Border
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

	public new string ToolTip
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

	public new PdfXfaRotateAngle Rotate
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

	internal void ReadField(XmlNode node)
	{
		currentNode = node;
		ReadCommonProperties(node);
		if (node["value"] == null || node["value"]["rectangle"] == null)
		{
			return;
		}
		XmlNode xmlNode = node["value"]["rectangle"];
		if (xmlNode["fill"] != null)
		{
			if (Border == null)
			{
				Border = new PdfXfaBorder();
			}
			Border.ReadFillBrush(xmlNode);
		}
	}

	internal void DrawRectangle(PdfGraphics graphics, RectangleF bounds)
	{
		RectangleF rectangleF = default(RectangleF);
		SizeF size = GetSize();
		rectangleF = new RectangleF(new PointF(bounds.Location.X + base.Margins.Left, bounds.Location.Y + base.Margins.Top), new SizeF(size.Width - (base.Margins.Right + base.Margins.Left), size.Height - (base.Margins.Top + base.Margins.Bottom)));
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
		graphics.DrawRectangle(pen, brush, rectangleF2);
		graphics.Restore();
	}
}
