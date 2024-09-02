using System.Xml;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Xfa;

internal class PdfLoadedXfaLine : PdfLoadedXfaStyledField
{
	private PdfXfaEdge edge;

	internal void ReadField(XmlNode node)
	{
		currentNode = node;
		ReadCommonProperties(node);
		if (node["value"] != null && node["value"]["line"] != null && node["value"]["line"]["edge"] != null)
		{
			edge = new PdfXfaEdge();
			edge.Read(node["value"]["line"]["edge"], edge);
			if (base.Height == 0f)
			{
				base.Height = edge.Thickness;
			}
		}
	}

	internal void DrawLine(PdfGraphics graphics, RectangleF bounds)
	{
		PdfPen pen = null;
		if (edge != null)
		{
			pen = new PdfPen(edge.Color, edge.Thickness);
		}
		graphics.DrawLine(pen, bounds.Location, new PointF(bounds.Width + bounds.X, bounds.Height + bounds.Y));
	}
}
