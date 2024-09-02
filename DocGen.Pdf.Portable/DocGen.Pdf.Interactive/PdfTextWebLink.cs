using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Interactive;

public class PdfTextWebLink : PdfTextElement
{
	private string m_url;

	private PdfUriAnnotation m_uriAnnotation;

	public string Url
	{
		get
		{
			return m_url;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("url");
			}
			if (value.Length == 0)
			{
				throw new ArgumentException("Url - string can not be empty");
			}
			m_url = value;
		}
	}

	public PdfLayoutResult DrawTextWebLink(PdfPage page, PointF location)
	{
		SizeF size = base.Font.MeasureString(base.Value, base.StringFormat);
		RectangleF rectangle = CalculateBounds(location, size);
		m_uriAnnotation = new PdfUriAnnotation(rectangle, Url);
		m_uriAnnotation.Border = new PdfAnnotationBorder(0f, 0f, 0f);
		if (page.Document != null && page.Document.AutoTag)
		{
			if (base.PdfTag == null)
			{
				base.PdfTag = new PdfStructureElement(PdfTagType.Link);
			}
			m_uriAnnotation.PdfTag = base.PdfTag;
		}
		page.Annotations.Add(m_uriAnnotation);
		base.PdfTag = null;
		return Draw(page, location);
	}

	public void DrawTextWebLink(PdfGraphics graphics, PointF location)
	{
		if (graphics.Page is PdfLoadedPage)
		{
			SizeF size = base.Font.MeasureString(base.Value, base.StringFormat);
			RectangleF rectangle = CalculateBounds(location, size);
			m_uriAnnotation = new PdfUriAnnotation(rectangle, Url);
			m_uriAnnotation.Border = new PdfAnnotationBorder(0f, 0f, 0f);
			graphics.Page.Annotations.Add(m_uriAnnotation);
			PdfGraphicsState state = graphics.Save();
			AnnotationRotateAndTransform(graphics);
			Draw(graphics, location);
			graphics.Restore(state);
		}
		else
		{
			new PdfPage();
			SizeF size2 = base.Font.MeasureString(base.Value, base.StringFormat);
			RectangleF rectangle2 = CalculateBounds(location, size2);
			m_uriAnnotation = new PdfUriAnnotation(rectangle2, Url);
			m_uriAnnotation.Border = new PdfAnnotationBorder(0f, 0f, 0f);
			(graphics.Page as PdfPage).Annotations.Add(m_uriAnnotation);
			Draw(graphics, location);
		}
	}

	private RectangleF CalculateBounds(PointF location, SizeF size)
	{
		RectangleF result = new RectangleF(location, size);
		if (base.StringFormat != null)
		{
			if (base.StringFormat.Alignment == PdfTextAlignment.Right)
			{
				result = new RectangleF(location.X - size.Width, location.Y, size.Width, size.Height);
			}
			else if (base.StringFormat.Alignment == PdfTextAlignment.Center)
			{
				result = new RectangleF(location.X - size.Width / 2f, location.Y, size.Width, size.Height);
			}
		}
		return result;
	}

	internal void AnnotationRotateAndTransform(PdfGraphics graphics)
	{
		switch (graphics.Page.Rotation)
		{
		case PdfPageRotateAngle.RotateAngle90:
			graphics.TranslateTransform(graphics.Page.Size.Height, 0f);
			graphics.RotateTransform(90f);
			break;
		case PdfPageRotateAngle.RotateAngle180:
			graphics.TranslateTransform(graphics.Page.Size.Width, graphics.Page.Size.Height);
			graphics.RotateTransform(180f);
			break;
		case PdfPageRotateAngle.RotateAngle270:
			graphics.TranslateTransform(0f, graphics.Page.Size.Width);
			graphics.RotateTransform(270f);
			break;
		}
	}
}
