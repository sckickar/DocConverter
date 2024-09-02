using System;
using System.Collections.Generic;
using System.IO;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf;

public class ImageToPdfConverter
{
	private PdfImagePosition m_imagePosition;

	private SizeF m_size;

	private PdfMargins m_margins;

	public PdfImagePosition ImagePosition
	{
		get
		{
			return m_imagePosition;
		}
		set
		{
			if (m_imagePosition != value)
			{
				m_imagePosition = value;
			}
		}
	}

	public SizeF PageSize
	{
		get
		{
			return m_size;
		}
		set
		{
			m_size = value;
		}
	}

	public PdfMargins Margin
	{
		get
		{
			return m_margins;
		}
		set
		{
			if (value != null)
			{
				m_margins = value;
			}
		}
	}

	public ImageToPdfConverter()
	{
		m_margins = new PdfMargins();
		m_size = PdfPageSize.A4;
		m_imagePosition = PdfImagePosition.FitToPageAndMaintainAspectRatio;
	}

	private PdfPage GetPage(PdfDocument document, PdfTiffImage image)
	{
		if (document.Sections.Count > 0 && ImagePosition != PdfImagePosition.CropPage)
		{
			return document.Sections[0].Pages.Add();
		}
		PdfSection pdfSection = document.Sections.Add();
		pdfSection.PageSettings.Size = PageSize;
		if (ImagePosition == PdfImagePosition.CropPage)
		{
			pdfSection.PageSettings.Size = image.PhysicalDimension;
			pdfSection.PageSettings.Margins.All = 0f;
		}
		else
		{
			pdfSection.PageSettings.Margins = Margin;
		}
		return pdfSection.Pages.Add();
	}

	private void DrawImage(PdfTiffImage image, PdfPage page)
	{
		switch (ImagePosition)
		{
		case PdfImagePosition.BottomLeftCornerOfPage:
		{
			SizeF clientSize3 = page.Graphics.ClientSize;
			page.Graphics.DrawImage(image, new PointF(0f, clientSize3.Height - image.PhysicalDimension.Height));
			break;
		}
		case PdfImagePosition.BottomRightCornerOfPage:
		{
			SizeF clientSize2 = page.Graphics.ClientSize;
			page.Graphics.DrawImage(image, new PointF(clientSize2.Width - image.PhysicalDimension.Width, clientSize2.Height - image.PhysicalDimension.Height));
			break;
		}
		case PdfImagePosition.CenteredOnPage:
		{
			SizeF clientSize4 = page.Graphics.ClientSize;
			float x = (clientSize4.Width - image.PhysicalDimension.Width) / 2f;
			float y = (clientSize4.Height - image.PhysicalDimension.Height) / 2f;
			page.Graphics.DrawImage(image, x, y);
			break;
		}
		case PdfImagePosition.CropPage:
			page.Graphics.DrawImage(image, PointF.Empty);
			break;
		case PdfImagePosition.FitToPage:
			page.Graphics.DrawImage(image, new RectangleF(0f, 0f, page.Graphics.ClientSize.Width, page.Graphics.ClientSize.Height));
			break;
		case PdfImagePosition.FitToPageAndMaintainAspectRatio:
		{
			SizeF clientSize = page.Graphics.ClientSize;
			float val = clientSize.Width / (float)image.Width;
			float val2 = clientSize.Height / (float)image.Height;
			float num = Math.Min(val, val2);
			float num2 = (float)image.Width * num;
			float num3 = (float)image.Height * num;
			RectangleF rectangleF = new RectangleF(0f, 0f, num2, num3);
			rectangleF = ((!(num2 > num3) && num2 != num3) ? new RectangleF((clientSize.Width - num2) / 2f, 0f, num2, num3) : new RectangleF(0f, (clientSize.Height - num3) / 2f, num2, num3));
			page.Graphics.DrawImage(image, rectangleF);
			break;
		}
		case PdfImagePosition.TopLeftCornerOfPage:
			page.Graphics.DrawImage(image, PointF.Empty);
			break;
		case PdfImagePosition.TopRightCornerOfPage:
			page.Graphics.DrawImage(image, new PointF(page.Graphics.ClientSize.Width - image.PhysicalDimension.Width, 0f));
			break;
		}
	}

	public PdfDocument Convert(Stream image)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		PdfTiffImage image2 = new PdfTiffImage(image);
		PdfDocument pdfDocument = new PdfDocument();
		PdfPage page = GetPage(pdfDocument, image2);
		DrawImage(image2, page);
		return pdfDocument;
	}

	public PdfDocument Convert(IEnumerable<Stream> images)
	{
		if (images == null)
		{
			throw new ArgumentNullException("images");
		}
		PdfDocument pdfDocument = new PdfDocument();
		foreach (Stream image in images)
		{
			if (image == null)
			{
				continue;
			}
			try
			{
				PdfTiffImage pdfTiffImage = new PdfTiffImage(image);
				if (pdfTiffImage != null)
				{
					PdfPage page = GetPage(pdfDocument, pdfTiffImage);
					DrawImage(pdfTiffImage, page);
				}
			}
			catch (Exception)
			{
			}
		}
		return pdfDocument;
	}
}
