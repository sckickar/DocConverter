using System;
using System.Collections.Generic;
using System.IO;
using DocGen.ChartToImageConverter;
using DocGen.DocIO;
using DocGen.DocIO.DLS;
using DocGen.DocIO.DLS.Rendering;
using DocGen.DocIO.Rendering;
using DocGen.DocToPdfConverter.Rendering;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;
using DocGen.Layouting;
using DocGen.OfficeChart;
using DocGen.Pdf;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;

namespace DocGen.DocIORenderer;

public class DocIORenderer : IDisposable
{
	private List<WPageSetup> m_pageSettings;

	private WordDocument m_wordDocument;

	private PdfDocument m_pdfDocument;

	private PdfPage m_currentPage;

	private ChartToImageconverter m_chartToImageconverter;

	private DocIORendererSettings m_settings = new DocIORendererSettings();

	private byte m_flag;

	private ChartToImageconverter ChartToImageconverter
	{
		get
		{
			if (m_chartToImageconverter == null)
			{
				m_chartToImageconverter = new ChartToImageconverter();
			}
			return m_chartToImageconverter;
		}
	}

	private List<WPageSetup> PageSettings
	{
		get
		{
			if (m_pageSettings == null)
			{
				m_pageSettings = new List<WPageSetup>();
			}
			return m_pageSettings;
		}
	}

	public DocIORendererSettings Settings
	{
		get
		{
			return m_settings;
		}
		set
		{
			m_settings = value;
		}
	}

	public bool IsCanceled
	{
		get
		{
			return (m_flag & 1) != 0;
		}
		set
		{
			m_flag |= (value ? ((byte)1) : ((byte)0));
		}
	}

	internal bool IsTrial
	{
		get
		{
			return (m_flag & 2) >> 1 != 0;
		}
		set
		{
			m_flag = (byte)((m_flag & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	public DocIORenderer()
	{
		m_pageSettings = new List<WPageSetup>();
		WordDocument.RenderHelper = new RenderHelper();
	}

	internal void Close()
	{
		if (m_pageSettings != null)
		{
			m_pageSettings.Clear();
			m_pageSettings = null;
		}
		if (m_settings != null)
		{
			m_settings = null;
		}
		if (m_wordDocument != null)
		{
			m_wordDocument = null;
		}
		if (m_pdfDocument != null)
		{
			m_pdfDocument = null;
		}
		if (m_currentPage != null)
		{
			m_currentPage = null;
		}
	}

	public void Dispose()
	{
		Close();
	}

	public void SaveChartAsImage(IOfficeChart officeChart, Stream outputImageStream)
	{
		ChartToImageconverter.SaveAsImage(officeChart, outputImageStream, Settings.ChartRenderingOptions);
	}

	public PdfDocument ConvertToPDF(WordDocument wordDocument)
	{
		m_wordDocument = wordDocument;
		ShowWarnings();
		if (Settings.EnableAlternateChunks)
		{
			m_wordDocument.UpdateAlternateChunks();
		}
		m_wordDocument.SortByZIndex(isFromHTMLExport: false);
		if (!IsCanceled)
		{
			m_wordDocument.FontSettings.EmbedDocumentFonts(m_wordDocument);
			WordDocument.RenderHelper = new RenderHelper();
			(WordDocument.RenderHelper as RenderHelper).IsPdfConversion = true;
			DocumentLayouter documentLayouter = null;
			DocumentLayouter.m_dc = new DrawingContext();
			DocumentLayouter.m_dc.FontStreams = m_wordDocument.FontSettings.FontStreams;
			if (m_wordDocument.FontSettings.FallbackFonts.Count > 0)
			{
				(WordDocument.RenderHelper as RenderHelper).EmbedCompleteFonts = Settings.EmbedCompleteFonts;
				(WordDocument.RenderHelper as RenderHelper).EmbedFonts = Settings.EmbedFonts;
			}
			if (Settings.UpdateDocumentFields)
			{
				documentLayouter = m_wordDocument.UpdateDocumentFieldsInOptimalWay();
			}
			if (documentLayouter == null)
			{
				documentLayouter = new DocumentLayouter();
				documentLayouter.Layout(wordDocument);
			}
			UpdateTrackChangesBalloonsCount(documentLayouter);
			if (Settings.PdfConformanceLevel != 0)
			{
				documentLayouter.EnablePdfConformanceLevel = true;
			}
			PdfDocument result = DrawDirectWordToPDF(documentLayouter);
			(WordDocument.RenderHelper as RenderHelper).IsPdfConversion = false;
			return result;
		}
		return null;
	}

	private void UpdateTrackChangesBalloonsCount(DocumentLayouter layouter)
	{
		int num = 0;
		foreach (Page page in layouter.Pages)
		{
			if (page.TrackChangesMarkups != null)
			{
				num += page.TrackChangesMarkups.Count;
			}
		}
		m_wordDocument.TrackChangesBalloonCount = num;
	}

	public PdfDocument ConvertToPDF(Stream stream)
	{
		WordDocument wordDocument = new WordDocument(stream, FormatType.Automatic);
		stream.Position = 0L;
		return ConvertToPDF(wordDocument);
	}

	private PdfDocument CreateDocument()
	{
		PdfDocument obj = ((Settings.PdfConformanceLevel == PdfConformanceLevel.None) ? new PdfDocument() : new PdfDocument(Settings.PdfConformanceLevel));
		PdfDocument.EnableCache = false;
		obj.EnableMemoryOptimization = true;
		obj.PageSettings.Margins.All = 0f;
		obj.FileStructure.CrossReferenceType = PdfCrossReferenceType.CrossReferenceTable;
		obj.FileStructure.Version = PdfVersion.Version1_4;
		return obj;
	}

	private PdfSection AddSection(WPageSetup pageSetup)
	{
		PdfSection pdfSection = m_pdfDocument.Sections.Add();
		pdfSection.PageSettings.Margins.All = 0f;
		if (m_wordDocument.TrackChangesBalloonCount > 0)
		{
			pdfSection.PageSettings.Margins.Top = 80f;
			pdfSection.PageSettings.Margins.Bottom = 80f;
		}
		pdfSection.PageSettings.Orientation = ((pageSetup.Orientation != 0 && !(pageSetup.Orientation.ToString() == "1")) ? PdfPageOrientation.Landscape : PdfPageOrientation.Portrait);
		pdfSection.PageSettings.Height = pageSetup.PageSize.Height;
		pdfSection.PageSettings.Width = pageSetup.PageSize.Width;
		return pdfSection;
	}

	private void InitPagesSettings(DocumentLayouter layouter)
	{
		for (int i = 0; i < layouter.Pages.Count; i++)
		{
			Page page = layouter.Pages[i];
			PageSettings.Add(page.Setup);
		}
	}

	private void AddDocumentProperties(BuiltinDocumentProperties docProperties)
	{
		if (!string.IsNullOrEmpty(docProperties.Author))
		{
			m_pdfDocument.DocumentInformation.Author = docProperties.Author;
		}
		if (!string.IsNullOrEmpty(docProperties.CreateDate.ToString()))
		{
			m_pdfDocument.DocumentInformation.CreationDate = docProperties.CreateDate;
		}
		if (!string.IsNullOrEmpty(docProperties.Company))
		{
			m_pdfDocument.DocumentInformation.Creator = docProperties.Company;
			m_pdfDocument.DocumentInformation.Producer = docProperties.Company;
		}
		if (!string.IsNullOrEmpty(docProperties.Keywords))
		{
			m_pdfDocument.DocumentInformation.Keywords = docProperties.Keywords;
		}
		if (!string.IsNullOrEmpty(docProperties.Subject))
		{
			m_pdfDocument.DocumentInformation.Subject = docProperties.Subject;
		}
		if (!string.IsNullOrEmpty(docProperties.Title))
		{
			m_pdfDocument.DocumentInformation.Title = docProperties.Title;
		}
	}

	private void AddHyperLinks(List<Dictionary<string, RectangleF>> hyperlinks)
	{
		for (int i = 0; i < hyperlinks.Count; i++)
		{
			foreach (KeyValuePair<string, RectangleF> item in hyperlinks[i])
			{
				RectangleF value = item.Value;
				string key = item.Key;
				if (!key.Equals(string.Empty))
				{
					PdfUriAnnotation pdfUriAnnotation = new PdfUriAnnotation(value);
					pdfUriAnnotation.Uri = key;
					pdfUriAnnotation.Border.Width = 0f;
					m_currentPage.Annotations.Add(pdfUriAnnotation);
				}
			}
		}
	}

	private void AddBookmarkHyperlinks(List<Dictionary<string, DocumentLayouter.BookmarkHyperlink>> bookmarkHyperlinks, List<Dictionary<string, DocumentLayouter.BookmarkHyperlink>> tempBookmarkHyperlinks, int pageIndex)
	{
		for (int i = 0; i < tempBookmarkHyperlinks.Count; i++)
		{
			foreach (KeyValuePair<string, DocumentLayouter.BookmarkHyperlink> item in tempBookmarkHyperlinks[i])
			{
				DocumentLayouter.BookmarkHyperlink value = item.Value;
				if (value.SourcePageNumber == pageIndex + 1 && !item.Key.Equals(string.Empty))
				{
					PdfDocumentLinkAnnotation pdfDocumentLinkAnnotation = new PdfDocumentLinkAnnotation(value.SourceBounds);
					pdfDocumentLinkAnnotation.Border = new PdfAnnotationBorder(0f);
					pdfDocumentLinkAnnotation.Name = value.HyperlinkValue;
					if (m_pdfDocument.Pages.Count >= value.TargetPageNumber && value.TargetPageNumber != 0)
					{
						pdfDocumentLinkAnnotation.Destination = new PdfDestination(m_pdfDocument.Pages[value.TargetPageNumber - 1]);
						pdfDocumentLinkAnnotation.Destination.Location = value.TargetBounds.Location;
					}
					m_currentPage.Annotations.Add(pdfDocumentLinkAnnotation);
					if (i == tempBookmarkHyperlinks.Count - 1)
					{
						AddBookmarks(bookmarkHyperlinks);
					}
					if (tempBookmarkHyperlinks[i].Count == 1)
					{
						tempBookmarkHyperlinks.Remove(tempBookmarkHyperlinks[i]);
						i--;
					}
				}
			}
		}
	}

	private void AddBookmarks(List<Dictionary<string, DocumentLayouter.BookmarkHyperlink>> bookmarkHyperlinks)
	{
		int num = -1;
		PdfBookmark[] array = new PdfBookmark[bookmarkHyperlinks.Count + 1];
		for (int i = 0; i < bookmarkHyperlinks.Count; i++)
		{
			foreach (KeyValuePair<string, DocumentLayouter.BookmarkHyperlink> item in bookmarkHyperlinks[i])
			{
				DocumentLayouter.BookmarkHyperlink value = item.Value;
				if (i == 0 && value.TOCText != "")
				{
					num = value.TOCLevel;
				}
				if (value.SourcePageNumber == m_pdfDocument.Pages.IndexOf(m_currentPage) + 1 && !item.Key.Equals(string.Empty) && m_pdfDocument.Pages.Count >= value.TargetPageNumber && value.TargetPageNumber != 0)
				{
					if (value.TOCLevel <= num)
					{
						array[value.TOCLevel] = m_pdfDocument.Bookmarks.Add(value.TOCText);
						PdfPage page = m_pdfDocument.Pages[value.TargetPageNumber - 1];
						array[value.TOCLevel].Destination = new PdfDestination(m_pdfDocument.Pages[value.TargetPageNumber - 1]);
						array[value.TOCLevel].Destination.Page = page;
						array[value.TOCLevel].Destination.Location = new PointF(value.TargetBounds.X, value.TargetBounds.Y);
					}
					else if (num != -1 && array[value.TOCLevel - 1] != null)
					{
						array[value.TOCLevel] = array[value.TOCLevel - 1].Add(value.TOCText);
						PdfPage page2 = m_pdfDocument.Pages[value.TargetPageNumber - 1];
						array[value.TOCLevel].Destination = new PdfDestination(m_pdfDocument.Pages[value.TargetPageNumber - 1]);
						array[value.TOCLevel].Destination.Page = page2;
						array[value.TOCLevel].Destination.Location = new PointF(value.TargetBounds.X, value.TargetBounds.Y);
					}
				}
			}
		}
	}

	private void AddDocumentBookmarks(List<DocGen.DocToPdfConverter.Rendering.BookmarkPosition> bookmarks)
	{
		PdfBookmark[] array = new PdfBookmark[9];
		int[] array2 = new int[bookmarks.Count];
		int previousLevel = 0;
		for (int i = 0; i < bookmarks.Count; i++)
		{
			DocGen.DocToPdfConverter.Rendering.BookmarkPosition bookmarkPosition = bookmarks[i];
			if (string.IsNullOrEmpty(bookmarkPosition.BookmarkName) || bookmarkPosition.PageNumber == 0 || bookmarkPosition.PageNumber > m_pdfDocument.Pages.Count)
			{
				continue;
			}
			PdfBookmark pdfBookmark = null;
			if ((Settings.ExportBookmarks & ExportBookmarkType.Headings) == ExportBookmarkType.Headings && bookmarkPosition.BookmarkStyle > 0)
			{
				int num = (array2[i] = bookmarkPosition.BookmarkStyle);
				if (num == 1 || i == 0)
				{
					pdfBookmark = (array[num - 1] = m_pdfDocument.Bookmarks.Add(bookmarkPosition.BookmarkName));
					pdfBookmark.Destination = new PdfDestination(m_pdfDocument.Pages[bookmarkPosition.PageNumber - 1]);
					pdfBookmark.Destination.Location = bookmarkPosition.Bounds.Location;
					previousLevel = num;
				}
				else if (HasParentNode(ref previousLevel, array2, i))
				{
					pdfBookmark = array[previousLevel - 1].Insert(array[previousLevel - 1].Count, bookmarkPosition.BookmarkName);
					if (num == 10)
					{
						array[num - 2] = pdfBookmark;
					}
					else
					{
						array[num - 1] = pdfBookmark;
					}
					pdfBookmark.Destination = new PdfDestination(m_pdfDocument.Pages[bookmarkPosition.PageNumber - 1]);
					pdfBookmark.Destination.Location = bookmarkPosition.Bounds.Location;
					previousLevel = num;
				}
				else
				{
					pdfBookmark = m_pdfDocument.Bookmarks.Add(bookmarkPosition.BookmarkName);
					if (num == 10)
					{
						array[num - 2] = pdfBookmark;
					}
					else
					{
						array[num - 1] = pdfBookmark;
					}
					pdfBookmark.Destination = new PdfDestination(m_pdfDocument.Pages[bookmarkPosition.PageNumber - 1]);
					pdfBookmark.Destination.Location = bookmarkPosition.Bounds.Location;
					previousLevel = num;
				}
			}
			else
			{
				pdfBookmark = m_pdfDocument.Bookmarks.Add(bookmarkPosition.BookmarkName);
				pdfBookmark.Destination = new PdfDestination(m_pdfDocument.Pages[bookmarkPosition.PageNumber - 1]);
				pdfBookmark.Destination.Location = bookmarkPosition.Bounds.Location;
			}
		}
	}

	private bool HasParentNode(ref int previousLevel, int[] previousLevelArray, int i)
	{
		for (int num = i; num > 0; num--)
		{
			if (previousLevelArray[i] > previousLevelArray[num - 1] && previousLevelArray[num - 1] != 0)
			{
				previousLevel = previousLevelArray[num - 1];
				return true;
			}
		}
		return false;
	}

	private PdfDocument DrawDirectWordToPDF(DocumentLayouter layouter)
	{
		InitPagesSettings(layouter);
		int count = layouter.Pages.Count;
		m_pdfDocument = CreateDocument();
		int autoTagsCount = 0;
		List<KeyValuePair<string, bool>> list = null;
		Hyperlink hyperlink = null;
		for (int i = 0; i < count; i++)
		{
			PdfGraphics graphics = (m_currentPage = AddSection(PageSettings[i]).Pages.Add()).Graphics;
			graphics.IsDirectPDF = true;
			graphics.OptimizeIdenticalImages = Settings.OptimizeIdenticalImages;
			WPageSetup setup = layouter.Pages[i].Setup;
			int width = (int)UnitsConvertor.Instance.ConvertToPixels(setup.PageSize.Width, PrintUnits.Point);
			int height = (int)UnitsConvertor.Instance.ConvertToPixels(setup.PageSize.Height, PrintUnits.Point);
			DocGen.Drawing.SkiaSharpHelper.Image image = new DocGen.Drawing.SkiaSharpHelper.Image(width, height);
			DocumentLayouter.PageNumber = i + 1;
			Graphics graphics2 = new Graphics(image);
			graphics2.PageUnit = GraphicsUnit.Point;
			PDFDrawingContext pDFDrawingContext = new PDFDrawingContext(graphics, graphics2, GraphicsUnit.Point, m_pdfDocument);
			float right = PageSettings[i].Margins.Right;
			float num = PageSettings[i].PageSize.Width / (PageSettings[i].PageSize.Width + (260f - right));
			if (m_wordDocument.TrackChangesBalloonCount > 0)
			{
				graphics.ScaleTransform(num, num);
			}
			if (list != null)
			{
				pDFDrawingContext.m_previousLineCommentStartMarks = list;
			}
			if (hyperlink != null)
			{
				pDFDrawingContext.m_prevPageHyperlink = hyperlink;
			}
			pDFDrawingContext.EmbedFonts = Settings.EmbedFonts;
			pDFDrawingContext.ChartRenderingOptions = Settings.ChartRenderingOptions;
			pDFDrawingContext.EmbedCompleteFonts = Settings.EmbedCompleteFonts;
			pDFDrawingContext.AutoTag = Settings.AutoTag;
			pDFDrawingContext.ExportBookmarks = Settings.ExportBookmarks;
			pDFDrawingContext.FontStreams = m_wordDocument.FontSettings.FontStreams;
			if (m_wordDocument.FontSettings.FallbackFonts.Count > 0)
			{
				pDFDrawingContext.FallbackFonts = m_wordDocument.FontSettings.FallbackFonts;
			}
			pDFDrawingContext.PreserveFormFields = Settings.PreserveFormFields;
			pDFDrawingContext.EnableComplexScript = Settings.AutoDetectComplexScript;
			pDFDrawingContext.ImageQuality = Settings.ImageQuality;
			pDFDrawingContext.Draw(layouter.Pages[i], ref autoTagsCount);
			pDFDrawingContext.DrawPageBorder(i, layouter.Pages);
			AddHyperLinks(pDFDrawingContext.Hyperlinks);
			graphics.IsDirectPDF = false;
			if (pDFDrawingContext.m_previousLineCommentStartMarks != null)
			{
				list = pDFDrawingContext.m_previousLineCommentStartMarks;
			}
			if (pDFDrawingContext.m_prevPageHyperlink != null)
			{
				hyperlink = pDFDrawingContext.m_prevPageHyperlink;
			}
		}
		layouter.InitLayoutInfo();
		List<Dictionary<string, DocumentLayouter.BookmarkHyperlink>> tempBookmarkHyperlinks = new List<Dictionary<string, DocumentLayouter.BookmarkHyperlink>>(PDFDrawingContext.BookmarkHyperlinksList);
		for (int j = 0; j < count; j++)
		{
			m_currentPage = m_pdfDocument.Pages[j];
			AddBookmarkHyperlinks(PDFDrawingContext.BookmarkHyperlinksList, tempBookmarkHyperlinks, j);
			if (IsTrial)
			{
				FontExtension fontExtension = new FontExtension("Times New Roman", 12f, FontStyle.Regular, GraphicsUnit.Point);
				PdfFont font = new PdfTrueTypeFont(fontExtension.FontStream, fontExtension.TextSize, isUnicode: false, string.Empty, PdfFontStyle.Regular);
				PdfBrush brush = new PdfSolidBrush(Color.White);
				m_currentPage.Graphics.DrawRectangle(brush, 0f, 0f, 205f, 20.65f);
				brush = new PdfSolidBrush(Color.Red);
				m_currentPage.Graphics.DrawString("Created by DocGen – DocIO library", font, brush, 6f, 4f);
				fontExtension.Close();
			}
		}
		if (Settings.ExportBookmarks != 0)
		{
			AddDocumentBookmarks(PDFDrawingContext.Bookmarks);
		}
		AddDocumentProperties(m_wordDocument.BuiltinDocumentProperties);
		layouter.Pages.Clear();
		layouter.Close();
		PDFDrawingContext.ClearFontCache();
		RenderHelper.ClearTypeFaceCache(m_wordDocument);
		if (m_pageSettings != null)
		{
			m_pageSettings.Clear();
			m_pageSettings = null;
		}
		if (m_chartToImageconverter != null)
		{
			m_chartToImageconverter = null;
		}
		if (list != null)
		{
			list.Clear();
			list = null;
		}
		if (hyperlink != null)
		{
			hyperlink = null;
		}
		return m_pdfDocument;
	}

	private void ShowWarnings()
	{
		if (Settings.Warning == null)
		{
			return;
		}
		List<WarningInfo> list = new List<WarningInfo>();
		List<string> list2 = CreateWarningElmentNames();
		bool flag = m_wordDocument.ActualFormatType == FormatType.Rtf;
		string arg = (flag ? " element is not supported in Rtf to Pdf conversion" : " element is not supported in Word to Pdf conversion");
		for (int i = 0; i <= 34; i++)
		{
			switch (i)
			{
			case 4:
			case 6:
			case 7:
			case 10:
			case 15:
			case 18:
			case 19:
			case 24:
			case 25:
			case 26:
			case 28:
			case 30:
			case 31:
			case 33:
			case 34:
				if (m_wordDocument.HasElement(m_wordDocument.m_notSupportedElementFlag, i))
				{
					list.Add(new WarningInfo($"{list2[i]}{arg}", (WarningType)i));
				}
				break;
			case 5:
				if (m_wordDocument.HasElement(m_wordDocument.m_supportedElementFlag_1, 1))
				{
					list.Add(new WarningInfo($"{list2[i]}{arg}", WarningType.DateTime));
				}
				break;
			case 32:
				if (m_wordDocument.HasElement(m_wordDocument.m_supportedElementFlag_1, 0))
				{
					list.Add(new WarningInfo($"{list2[i]}{arg}", WarningType.DateTime));
				}
				break;
			default:
				if (m_wordDocument.HasElement(m_wordDocument.m_notSupportedElementFlag, i))
				{
					list.Add(new WarningInfo($"{list2[i]}{arg}", WarningType.DateTime));
				}
				break;
			}
		}
		if (flag)
		{
			if (m_wordDocument.HasElement(m_wordDocument.m_supportedElementFlag_1, 29))
			{
				list.Add(new WarningInfo(string.Format("{0}{1}", "OLE Object", arg), WarningType.OLEObject));
			}
			if (m_wordDocument.HasElement(m_wordDocument.m_supportedElementFlag_2, 13))
			{
				list.Add(new WarningInfo(string.Format("{0}{1}", "Textbox ", arg), WarningType.Textbox));
			}
			if (m_wordDocument.HasElement(m_wordDocument.m_supportedElementFlag_1, 29))
			{
				list.Add(new WarningInfo(string.Format("{0}{1}", "Text watermark", arg), WarningType.Watermark));
			}
		}
		list2.Clear();
		IsCanceled = !Settings.Warning.ShowWarnings(list);
		list.Clear();
	}

	private List<string> CreateWarningElmentNames()
	{
		return new List<string>
		{
			"Abbreviated date", "Abbreviated day of week", "Abbreviated month", "AM/PM for current time", "Annotation", "Year short", "Comments", "Current section number", "Current time in hours:minutes", "Current Time in hours:minutes:seconds",
			"Custom shape", "Date and month", "Day of week ", "Day long ", "Day short ", "Group shape ", "Hour of current time ", "Hour of current time with no leading zero ", "Line Number", "Math",
			"Minute of current time with no leading zero", "Minute of current time", "Month long", "Month short", "Page number", "Shape", "Print merge helper field", "Seconds of current time", "Smart art", "Time in hours:minutes:seconds",
			"Track changes", "Word art", "Year long", "Chart", "Metafile"
		};
	}
}
