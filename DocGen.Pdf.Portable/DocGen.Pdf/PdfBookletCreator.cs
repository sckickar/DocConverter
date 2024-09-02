using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Parsing;

namespace DocGen.Pdf;

public sealed class PdfBookletCreator
{
	private PdfBookletCreator()
	{
		throw new NotSupportedException("Instantination of BookletCreator class is not supported");
	}

	public static PdfDocument CreateBooklet(PdfLoadedDocument loadedDocument, SizeF pageSize)
	{
		return CreateBooklet(loadedDocument, pageSize, twoSide: false);
	}

	public static PdfDocument CreateBooklet(PdfLoadedDocument loadedDocument, SizeF pageSize, bool twoSide)
	{
		if (loadedDocument == null)
		{
			throw new ArgumentNullException("loadedDocument");
		}
		if (pageSize == SizeF.Empty)
		{
			throw new ArgumentOutOfRangeException("pageSize", "Parameter can not be empty");
		}
		SizeF size = new SizeF(pageSize.Width / 2f, pageSize.Height);
		PointF empty = PointF.Empty;
		PointF location = new PointF(size.Width, 0f);
		PdfDocument pdfDocument = new PdfDocument();
		pdfDocument.PageSettings.Margins.All = 0f;
		int count = loadedDocument.Pages.Count;
		PdfLoadedPageCollection pages = loadedDocument.Pages;
		int num = count / 2 + count % 2;
		bool flag = false;
		if (twoSide)
		{
			flag = num % 2 == 0;
		}
		for (int i = 0; i < num; i++)
		{
			pdfDocument.PageSettings.Size = pageSize;
			if (pageSize.Width > pageSize.Height)
			{
				pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
			}
			PdfPage pdfPage = pdfDocument.Pages.Add();
			int[] nextPair = GetNextPair(i, count, twoSide);
			int num2 = ((twoSide && flag) ? 1 : 0);
			num2 = nextPair[num2];
			if (num2 >= 0)
			{
				PdfPageBase pdfPageBase = pages[num2];
				pdfPageBase.m_isBooklet = true;
				PdfTemplate template = pdfPageBase.CreateTemplate();
				if (pdfPageBase.Orientation == PdfPageOrientation.Landscape)
				{
					pdfPage.Graphics.DrawPdfTemplate(template, empty, new SizeF(size.Height, size.Width));
				}
				else
				{
					pdfPage.Graphics.DrawPdfTemplate(template, empty, size);
				}
			}
			num2 = ((!flag) ? 1 : 0);
			num2 = nextPair[num2];
			if (num2 >= 0)
			{
				PdfPageBase pdfPageBase2 = pages[num2];
				pdfPageBase2.m_isBooklet = true;
				PdfTemplate template = pdfPageBase2.CreateTemplate();
				if (pdfPageBase2.Orientation == PdfPageOrientation.Landscape)
				{
					pdfPage.Graphics.DrawPdfTemplate(template, location, new SizeF(size.Height, size.Width));
				}
				else
				{
					pdfPage.Graphics.DrawPdfTemplate(template, location, size);
				}
			}
		}
		return pdfDocument;
	}

	public static PdfDocument CreateBooklet(PdfLoadedDocument loadedDocument, SizeF pageSize, bool twoSide, PdfMargins margin)
	{
		if (loadedDocument == null)
		{
			throw new ArgumentNullException("loadedDocument");
		}
		if (pageSize == SizeF.Empty)
		{
			throw new ArgumentOutOfRangeException("pageSize", "Parameter can not be empty");
		}
		SizeF size = new SizeF(pageSize.Width / 2f, pageSize.Height);
		PointF empty = PointF.Empty;
		PointF location = new PointF(size.Width, 0f);
		PdfDocument pdfDocument = new PdfDocument();
		pdfDocument.PageSettings.Margins = margin;
		int count = loadedDocument.Pages.Count;
		PdfLoadedPageCollection pages = loadedDocument.Pages;
		int num = count / 2 + count % 2;
		bool flag = false;
		if (twoSide)
		{
			flag = num % 2 == 0;
		}
		for (int i = 0; i < num; i++)
		{
			pdfDocument.PageSettings.Size = pageSize;
			PdfPage pdfPage = pdfDocument.Pages.Add();
			int[] nextPair = GetNextPair(i, count, twoSide);
			int num2 = ((twoSide && flag) ? 1 : 0);
			num2 = nextPair[num2];
			if (num2 >= 0)
			{
				PdfTemplate template = pages[num2].CreateTemplate();
				pdfPage.Graphics.DrawPdfTemplate(template, empty, size);
			}
			num2 = ((!flag) ? 1 : 0);
			num2 = nextPair[num2];
			if (num2 >= 0)
			{
				PdfTemplate template = pages[num2].CreateTemplate();
				pdfPage.Graphics.DrawPdfTemplate(template, location, size);
			}
		}
		return pdfDocument;
	}

	private static int[] GetNextPair(int index, int count, bool twoSide)
	{
		int[] array = new int[2];
		int num = count - index - (count + 1) % 2;
		if (num == count)
		{
			num = -1;
		}
		if (twoSide && index % 2 > 0)
		{
			array[1] = index;
			array[0] = num;
		}
		else
		{
			array[0] = index;
			array[1] = num;
		}
		return array;
	}
}
