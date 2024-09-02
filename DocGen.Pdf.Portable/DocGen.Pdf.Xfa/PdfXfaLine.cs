using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Xfa;

public class PdfXfaLine : PdfXfaField
{
	internal PointF m_startLocation;

	internal PointF m_endLocation;

	private float m_thickness = 1f;

	private PdfColor m_color;

	internal PdfXfaForm parent;

	public PdfColor Color
	{
		get
		{
			return m_color;
		}
		set
		{
			m_color = value;
		}
	}

	public float Thickness
	{
		get
		{
			return m_thickness;
		}
		set
		{
			m_thickness = value;
		}
	}

	public PdfXfaLine(PointF startLocation, PointF endLocation, float thickness)
	{
		m_startLocation = startLocation;
		m_endLocation = endLocation;
		m_thickness = thickness;
	}

	internal void Save(XfaWriter xfaWriter)
	{
		float num = m_endLocation.X - m_startLocation.X;
		float num2 = m_endLocation.Y - m_startLocation.Y;
		string slope = "";
		if (num < 0f || num2 < 0f)
		{
			slope = "/";
		}
		if (num < 0f && num2 < 0f)
		{
			slope = "";
		}
		xfaWriter.Write.WriteStartElement("draw");
		xfaWriter.Write.WriteAttributeString("name", "line" + xfaWriter.m_fieldCount++);
		xfaWriter.SetRPR(PdfXfaRotateAngle.RotateAngle0, base.Visibility, isReadOnly: false);
		xfaWriter.SetSize(Math.Abs(num2), Math.Abs(num), 0f, 0f);
		xfaWriter.DrawLine(m_thickness, slope, Color.R + "," + Color.G + "," + Color.B);
		xfaWriter.Write.WriteEndElement();
	}

	internal void SaveAcroForm(PdfPage page, RectangleF bounds)
	{
		PdfPen pdfPen = null;
		pdfPen = ((!(Color == PdfColor.Empty)) ? new PdfPen(Color) : PdfPens.Black);
		pdfPen.Width = Thickness;
		page.Graphics.DrawLine(pdfPen, bounds.Location, new PointF(bounds.Width + bounds.X, bounds.Height + bounds.Y));
	}

	internal SizeF GetSize()
	{
		return new SizeF(m_endLocation.X - m_startLocation.X, m_endLocation.Y - m_startLocation.Y);
	}

	public object Clone()
	{
		return MemberwiseClone();
	}
}
