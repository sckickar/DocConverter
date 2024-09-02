using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Graphics.Images.Metafiles;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.Parsing;

namespace DocGen.Pdf.HtmlToPdf;

internal class HtmlToPdfResult : IDisposable
{
	private HtmlToPdfToc m_toc = new HtmlToPdfToc();

	private bool m_enableBookmark;

	private bool m_enableToc;

	private SinglePageLayout m_singlePageLayout;

	private bool m_isTempDir;

	private bool m_disableIEWarning;

	private bool m_disableWebKitWarning;

	private List<HtmlHyperLink> m_WebKitHyperlinkCollection = new List<HtmlHyperLink>();

	private List<HtmlInternalLink> m_WebKitInternalLinkCollection = new List<HtmlInternalLink>();

	private List<HtmlInternalLink> m_internalLinkDestination = new List<HtmlInternalLink>();

	private List<HtmlToPdfAutoCreateForms> m_webkitAutoCreateForms = new List<HtmlToPdfAutoCreateForms>();

	private Image[] m_images;

	private PointF m_location;

	private float m_metafileTransparency;

	private long m_quality = 100L;

	private Stream m_docStream;

	private bool m_Completed = true;

	private bool m_enableForm;

	private float m_height;

	private float m_remHeight;

	private PdfLayoutResult m_layoutResult;

	private bool m_isImagePath;

	private const int m_splitOffset = 4;

	internal string WebKitFilePath = string.Empty;

	internal string RenderEngine = string.Empty;

	internal string baseURL = string.Empty;

	internal bool m_enableDirectLayout;

	private PdfLayoutResult[] layoutDetails;

	private const string TEXTBOX = "text";

	private const string INPUT = "input";

	private const string TEXTAREA = "textarea";

	private const string SUBMIT = "submit";

	private const string BUTTON = "button";

	private const string CHECKBOX = "checkbox";

	private const string RADIOBUTTON = "radio";

	private const string SELECTBOX = "select";

	private const string READONLY = "readonly";

	private const string CHECKED = "checked";

	private const string SELECTED = "selected";

	private const string PASSWORD = "password";

	private const string NUMBER = "number";

	private const string TEL = "tel";

	private const string EMAIL = "email";

	private const string FORMBEGIN = "formbegin";

	private const string FORMEND = "formend";

	private PdfDocument singlePdfDoc;

	internal bool EnableBookmark
	{
		get
		{
			return m_enableBookmark;
		}
		set
		{
			m_enableBookmark = value;
		}
	}

	internal bool EnableToc
	{
		get
		{
			return m_enableToc;
		}
		set
		{
			m_enableToc = value;
		}
	}

	internal HtmlToPdfToc Toc
	{
		get
		{
			return m_toc;
		}
		set
		{
			m_toc = value;
		}
	}

	internal bool IsImagePath
	{
		get
		{
			return m_isImagePath;
		}
		set
		{
			m_isImagePath = value;
		}
	}

	internal bool DisableIEWarning
	{
		get
		{
			return m_disableIEWarning;
		}
		set
		{
			m_disableIEWarning = value;
		}
	}

	internal bool DisableWebKitWarning
	{
		get
		{
			return m_disableWebKitWarning;
		}
		set
		{
			m_disableWebKitWarning = value;
		}
	}

	internal bool Completed => m_Completed;

	internal bool EnableForms
	{
		get
		{
			return m_enableForm;
		}
		set
		{
			m_enableForm = value;
		}
	}

	internal float Height => m_height;

	public Image RenderedImage => m_images[0];

	public Image[] Images => m_images;

	public long Quality
	{
		set
		{
			m_quality = value;
		}
	}

	public PointF Location
	{
		get
		{
			return m_location;
		}
		set
		{
			m_location = value;
		}
	}

	public float MetafileTransparency
	{
		get
		{
			return m_metafileTransparency;
		}
		set
		{
			if (value > 0f && value <= 1f)
			{
				m_metafileTransparency = value;
				return;
			}
			throw new PdfException("Value can only be greater than 0 and less than or equal to 1");
		}
	}

	internal PdfLayoutResult LayoutResult => m_layoutResult;

	internal SinglePageLayout SinglePageLayout
	{
		get
		{
			return m_singlePageLayout;
		}
		set
		{
			m_singlePageLayout = value;
		}
	}

	internal bool IsTempDirectory
	{
		get
		{
			return m_isTempDir;
		}
		set
		{
			m_isTempDir = value;
		}
	}

	private void DeleteFile(string filePath)
	{
		if (File.Exists(filePath))
		{
			File.Delete(filePath);
		}
	}

	public void Render(PdfDocument document)
	{
		if (RenderEngine == "WebKit")
		{
			PdfMetafileLayoutFormat format = new PdfMetafileLayoutFormat();
			PdfPage page = document.Pages[0];
			RenderWebKit(page, format);
		}
		else
		{
			PdfMetafileLayoutFormat format2 = new PdfMetafileLayoutFormat();
			PdfPage page2 = document.Pages.Add();
			Render(page2, format2);
		}
	}

	internal PdfDocument Render(PdfDocument document, PdfMetafileLayoutFormat metafileFormat)
	{
		RenderWebKit(document.Pages[0], metafileFormat);
		if (singlePdfDoc == null)
		{
			return document;
		}
		return singlePdfDoc;
	}

	public void Render(PdfPageBase page, PdfLayoutFormat format)
	{
		if (page == null)
		{
			throw new PdfException("Page cannot be null.");
		}
		format = ((format == null) ? new PdfLayoutFormat() : format);
		if (RenderEngine == "WebKit")
		{
			RenderWebKit(page, format);
			if (!DisableWebKitWarning && page != null)
			{
				AddWebKitWarningWatermark((PdfPage)page);
			}
		}
	}

	public void Render(PdfPageBase page, PdfLayoutFormat format, out PdfLayoutResult result)
	{
		if (page == null)
		{
			throw new PdfException("Page cannot be null.");
		}
		result = null;
		format = ((format == null) ? new PdfLayoutFormat() : format);
		if (RenderEngine == "WebKit")
		{
			result = RenderWebKit(page, format);
			if (!DisableWebKitWarning && page != null)
			{
				AddWebKitWarningWatermark((PdfPage)page);
			}
		}
	}

	private void AddWebKitWarningWatermark(PdfPage page)
	{
		if (page != null)
		{
			PdfGraphics graphics = page.Graphics;
			graphics.Save();
			graphics.SetTransparency(0.8f);
			graphics.DrawRectangle(PdfBrushes.Red, new RectangleF(0f, 0f, page.Size.Width, 27f));
			RectangleF layoutRectangle = new RectangleF(2f, 2f, page.Size.Width, 27f);
			PdfStandardFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 10f, PdfFontStyle.Bold);
			PdfStringFormat stringFormat = new PdfStringFormat
			{
				NoClip = true,
				MeasureTrailingSpaces = true
			};
			PdfTextLayoutResult pdfTextLayoutResult = new PdfTextElement("Generated using the old legacy WebKit rendering engine. For the best result and quality, use our latest Blink engine with a simple change. To remove this warning message, ", font)
			{
				StringFormat = stringFormat
			}.Draw(format: new PdfLayoutFormat(), page: page, layoutRectangle: layoutRectangle);
			PdfTextWebLink pdfTextWebLink = new PdfTextWebLink();
			pdfTextWebLink.Text = " check here.";
			pdfTextWebLink.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 10f, PdfFontStyle.Bold);
			pdfTextWebLink.Brush = PdfBrushes.Blue;
			pdfTextWebLink.Url = "https://help.syncfusion.com/file-formats/pdf/converting-html-to-pdf#steps-to-disable-webkit-warning-while-performing-html-to-pdf";
			pdfTextWebLink.DrawTextWebLink(page, new PointF(pdfTextLayoutResult.LastLineBounds.Width, pdfTextLayoutResult.LastLineBounds.Y));
			graphics.Restore();
		}
	}

	private void AddIEWarningWatermark(PdfPage page)
	{
		if (page != null)
		{
			PdfGraphics graphics = page.Graphics;
			graphics.Save();
			graphics.SetTransparency(0.8f);
			graphics.DrawRectangle(PdfBrushes.Red, new RectangleF(0f, 0f, page.Size.Width, 40f));
			float num = 0f;
			float y = 0f;
			RectangleF layoutRectangle = new RectangleF(num, y, page.Size.Width, 40f);
			PdfStandardFont pdfStandardFont = new PdfStandardFont(PdfFontFamily.Helvetica, 10f, PdfFontStyle.Bold);
			PdfStringFormat stringFormat = new PdfStringFormat
			{
				NoClip = true,
				MeasureTrailingSpaces = true
			};
			PdfTextLayoutResult pdfTextLayoutResult = new PdfTextElement("Generated using the default Internet Explorer rendering engine. Content is preserved as raster (bitmap) images. For vector rendering and better overall image quality, you may use our ", pdfStandardFont)
			{
				StringFormat = stringFormat
			}.Draw(format: new PdfLayoutFormat(), page: page, layoutRectangle: layoutRectangle);
			string text = "Blink";
			PdfTextWebLink pdfTextWebLink = new PdfTextWebLink();
			pdfTextWebLink.Text = text;
			pdfTextWebLink.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 10f, PdfFontStyle.Bold);
			pdfTextWebLink.Brush = PdfBrushes.Blue;
			pdfTextWebLink.Url = "https://en.wikipedia.org/wiki/Blink_(browser_engine)";
			pdfTextWebLink.DrawTextWebLink(page, new PointF(pdfTextLayoutResult.LastLineBounds.Width, pdfTextLayoutResult.LastLineBounds.Y));
			SizeF sizeF = pdfStandardFont.MeasureString(text);
			num += pdfTextLayoutResult.LastLineBounds.Width + sizeF.Width;
			layoutRectangle = new RectangleF(num, pdfTextLayoutResult.LastLineBounds.Y, page.Size.Width, 40f);
			pdfTextLayoutResult = new PdfTextElement(" or ", pdfStandardFont)
			{
				StringFormat = stringFormat
			}.Draw(format: new PdfLayoutFormat(), page: page, layoutRectangle: layoutRectangle);
			string text2 = "WebKit";
			num += pdfTextLayoutResult.LastLineBounds.Width;
			PdfTextWebLink pdfTextWebLink2 = new PdfTextWebLink();
			pdfTextWebLink2.Text = text2;
			pdfTextWebLink2.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 10f, PdfFontStyle.Bold);
			pdfTextWebLink2.Brush = PdfBrushes.Blue;
			pdfTextWebLink2.Url = "https://en.wikipedia.org/wiki/WebKit";
			pdfTextWebLink2.DrawTextWebLink(page, new PointF(num, pdfTextLayoutResult.LastLineBounds.Y));
			SizeF sizeF2 = pdfStandardFont.MeasureString(text2);
			num += sizeF2.Width;
			layoutRectangle = new RectangleF(num, pdfTextLayoutResult.LastLineBounds.Y, page.Size.Width, 40f);
			pdfTextLayoutResult = new PdfTextElement(" powered engines with a ", pdfStandardFont)
			{
				StringFormat = stringFormat
			}.Draw(format: new PdfLayoutFormat(), page: page, layoutRectangle: layoutRectangle);
			num += pdfTextLayoutResult.LastLineBounds.Width;
			string text3 = "simple change.";
			PdfTextWebLink pdfTextWebLink3 = new PdfTextWebLink();
			pdfTextWebLink3.Text = text3;
			pdfTextWebLink3.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 10f, PdfFontStyle.Bold);
			pdfTextWebLink3.Brush = PdfBrushes.Blue;
			pdfTextWebLink3.Url = "https://www.syncfusion.com/kb/13047/switching-ie-engine-to-blink-and-webkit-engines-using-c-and-vb-net";
			pdfTextWebLink3.DrawTextWebLink(page, new PointF(num, pdfTextLayoutResult.LastLineBounds.Y));
			SizeF sizeF3 = pdfStandardFont.MeasureString(text3);
			num += sizeF3.Width;
			layoutRectangle = new RectangleF(0f, pdfTextLayoutResult.LastLineBounds.Y + sizeF3.Height, page.Size.Width, 40f);
			pdfTextLayoutResult = new PdfTextElement("The use of these engines is subject to additional terms. To remove this warning message, ", pdfStandardFont)
			{
				StringFormat = stringFormat
			}.Draw(format: new PdfLayoutFormat(), page: page, layoutRectangle: layoutRectangle);
			PdfTextWebLink pdfTextWebLink4 = new PdfTextWebLink();
			pdfTextWebLink4.Text = "check here.";
			pdfTextWebLink4.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 10f, PdfFontStyle.Bold);
			pdfTextWebLink4.Brush = PdfBrushes.Blue;
			pdfTextWebLink4.Url = "https://help.syncfusion.com/file-formats/pdf/converting-html-to-pdf#steps-to-disable-ie-warning-while-performing-html-to-pdf-using-the-ie-rendering-engine";
			pdfTextWebLink4.DrawTextWebLink(page, new PointF(pdfTextLayoutResult.LastLineBounds.Width, pdfTextLayoutResult.LastLineBounds.Y));
			graphics.Restore();
		}
	}

	private PdfLayoutResult RenderWebKit(PdfPageBase page, PdfLayoutFormat format)
	{
		PdfLayoutResult pdfLayoutResult = new PdfLayoutResult(page as PdfPage, RectangleF.Empty);
		PdfMetafileLayoutFormat pdfMetafileLayoutFormat = format as PdfMetafileLayoutFormat;
		try
		{
			if (!File.Exists(WebKitFilePath + ".pdf") || !File.Exists(WebKitFilePath + ".txt"))
			{
				throw new PdfException("Failed to convert the webpage");
			}
			PdfLoadedDocument pdfLoadedDocument = new PdfLoadedDocument(File.ReadAllBytes(WebKitFilePath + ".pdf"));
			string text = string.Empty;
			using (StreamReader streamReader = new StreamReader(File.OpenRead(WebKitFilePath + ".txt")))
			{
				text = streamReader.ReadToEnd();
			}
			DeleteFile(WebKitFilePath + ".txt");
			string[] array = new string[0];
			if (EnableForms)
			{
				int num = text.IndexOf("formbegin", StringComparison.CurrentCultureIgnoreCase);
				int num2 = text.IndexOf("formend", StringComparison.CurrentCultureIgnoreCase);
				array = text.Substring(num, num2 - num).Split(new string[1] { "forms" }, StringSplitOptions.None);
				text = text.Remove(num, num2 + "formend".Length - num);
			}
			string[] array2 = text.Split(new string[1] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
			float num3 = float.Parse(array2[1].Trim(), CultureInfo.InvariantCulture);
			int num4 = 0;
			List<TextRegionManager> list = new List<TextRegionManager>();
			TextRegionManager textRegionManager = new TextRegionManager();
			ImageRegionManager imageRegionManager = new ImageRegionManager();
			List<ImageRegionManager> list2 = new List<ImageRegionManager>();
			ImageRegionManager imageRegionManager2 = new ImageRegionManager();
			List<ImageRegionManager> list3 = new List<ImageRegionManager>();
			string[] array3 = array2[0].Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);
			float num5 = float.Parse(array3[1].Trim(), CultureInfo.InvariantCulture);
			float num6 = 0f;
			float num7 = 0f;
			float num8 = 0f;
			new Dictionary<string, RectangleF>();
			for (int i = 2; i < array2.Length; i++)
			{
				array3 = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))").Split(array2[i]);
				if (array3.Length > 4)
				{
					switch (array3[0])
					{
					case "text":
						try
						{
							num8 = float.Parse(array3[1], CultureInfo.InvariantCulture);
							float dx3 = float.Parse(array3[2], CultureInfo.InvariantCulture);
							float dy3 = float.Parse(array3[3], CultureInfo.InvariantCulture);
							float num24 = float.Parse(array3[4], CultureInfo.InvariantCulture);
							float num25 = float.Parse(array3[5], CultureInfo.InvariantCulture);
							float dx4 = float.Parse(array3[6], CultureInfo.InvariantCulture);
							float dy4 = float.Parse(array3[7], CultureInfo.InvariantCulture);
							Matrix matrix2 = new Matrix(1f, 0f, 0f, 1f, 0f, 0f);
							matrix2.Multiply(new Matrix(num3, 0f, 0f, num3, 0f, 0f));
							matrix2.Multiply(new Matrix(num8, 0f, 0f, num8, dx3, dy3));
							matrix2.Multiply(new Matrix(1f, 0f, 0f, 1f, dx4, dy4));
							num6 = num3 * num8;
							if (num6 < 0f)
							{
								num6 = Math.Abs(num6);
							}
							PdfUnitConvertor pdfUnitConvertor2 = new PdfUnitConvertor(96f);
							float y2 = pdfUnitConvertor2.ConvertToPixels(matrix2.OffsetY - (num24 * num6 - num25 * num6), PdfGraphicsUnit.Point);
							float height2 = pdfUnitConvertor2.ConvertToPixels(num24 * num6, PdfGraphicsUnit.Point);
							TextRegion region2 = new TextRegion(y2, height2);
							textRegionManager.Add(region2);
						}
						catch (Exception)
						{
						}
						break;
					case "Hyperlink":
						try
						{
							string empty = string.Empty;
							empty = ((!RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) ? array3[1].Substring(1, array3[1].Length - 2) : array3[1]);
							int num19 = int.Parse(array3[2], CultureInfo.InvariantCulture);
							float num20 = float.Parse(array3[3], CultureInfo.InvariantCulture);
							float num21 = float.Parse(array3[4], CultureInfo.InvariantCulture);
							float num22 = float.Parse(array3[5], CultureInfo.InvariantCulture);
							float num23 = float.Parse(array3[6], CultureInfo.InvariantCulture);
							if (num19 != -1)
							{
								Uri.TryCreate(baseURL, UriKind.RelativeOrAbsolute, out Uri result3);
								Uri.TryCreate(result3, empty, out Uri result4);
								HtmlHyperLink htmlHyperLink2 = new HtmlHyperLink();
								htmlHyperLink2.Bounds = new RectangleF(num20 * num6, num21 * num6, num22 * num6, num23 * num6);
								htmlHyperLink2.Href = result4.OriginalString;
								htmlHyperLink2.Name = num19.ToString();
								m_WebKitHyperlinkCollection.Add(htmlHyperLink2);
							}
						}
						catch (Exception)
						{
						}
						break;
					case "Internallink":
						try
						{
							int num12 = int.Parse(array3[2], CultureInfo.InvariantCulture);
							float num13 = float.Parse(array3[3], CultureInfo.InvariantCulture);
							float num14 = float.Parse(array3[4], CultureInfo.InvariantCulture);
							float num15 = float.Parse(array3[5], CultureInfo.InvariantCulture);
							float num16 = float.Parse(array3[6], CultureInfo.InvariantCulture);
							HtmlHyperLink htmlHyperLink = new HtmlHyperLink();
							HtmlInternalLink htmlInternalLink2 = new HtmlInternalLink();
							if (num12 == -1)
							{
								break;
							}
							Uri.TryCreate(baseURL, UriKind.RelativeOrAbsolute, out Uri result);
							Uri.TryCreate(result, array3[1], out Uri result2);
							htmlHyperLink.Bounds = new RectangleF(num13 * num6, num14 * num6, num15 * num6, num16 * num6);
							htmlHyperLink.Href = result2.OriginalString;
							htmlHyperLink.Name = num12.ToString();
							htmlInternalLink2.SourcePageNumber = num12.ToString();
							htmlInternalLink2.Bounds = new RectangleF(num13 * num6, num14 * num6, num15 * num6, num16 * num6);
							if (array3[1].Contains("#"))
							{
								string[] array4 = array3[1].Split('#');
								if (array3[1] == "#")
								{
									m_WebKitHyperlinkCollection.Add(htmlHyperLink);
								}
								else
								{
									if (array4[0] == baseURL || array4[0] == string.Empty || array4[0].Substring(0, 1) == "/")
									{
										if (!(array4[1] != string.Empty))
										{
											break;
										}
										bool flag2 = true;
										for (int j = 0; j < array4[1].Length; j++)
										{
											if (char.IsLetter(array4[1][j]))
											{
												flag2 = false;
											}
										}
										if (flag2)
										{
											m_WebKitHyperlinkCollection.Add(htmlHyperLink);
											break;
										}
										htmlInternalLink2.Href = array4[1];
										m_WebKitInternalLinkCollection.Add(htmlInternalLink2);
										break;
									}
									m_WebKitHyperlinkCollection.Add(htmlHyperLink);
								}
							}
							else
							{
								string iD = array3[1];
								int destinationPageNumber = int.Parse(array3[2], CultureInfo.InvariantCulture);
								float num17 = float.Parse(array3[3], CultureInfo.InvariantCulture);
								float num18 = float.Parse(array3[4], CultureInfo.InvariantCulture);
								HtmlInternalLink htmlInternalLink3 = new HtmlInternalLink();
								htmlInternalLink3.ID = iD;
								htmlInternalLink3.DestinationPageNumber = destinationPageNumber;
								htmlInternalLink3.Destination = new PointF(num17 * num6, num18 * num6);
								m_internalLinkDestination.Add(htmlInternalLink3);
							}
						}
						catch
						{
						}
						break;
					case "Header":
						try
						{
							HtmlInternalLink htmlInternalLink = new HtmlInternalLink();
							htmlInternalLink.HeaderTagLevel = array3[1];
							htmlInternalLink.ID = array3[2];
							if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
							{
								htmlInternalLink.HeaderContent = array3[3];
							}
							else
							{
								string headerContent = array3[3].Substring(1, array3[3].Length - 2);
								htmlInternalLink.HeaderContent = headerContent;
							}
							if (SinglePageLayout == SinglePageLayout.None)
							{
								htmlInternalLink.DestinationPageNumber = int.Parse(array3[4], CultureInfo.InvariantCulture);
							}
							else
							{
								htmlInternalLink.DestinationPageNumber = 0;
							}
							float num10 = float.Parse(array3[5], CultureInfo.InvariantCulture);
							float num11 = float.Parse(array3[6], CultureInfo.InvariantCulture);
							if (htmlInternalLink.DestinationPageNumber < 1 || !(num10 >= 0f) || !(num11 >= 0f))
							{
								break;
							}
							bool flag = false;
							foreach (HtmlInternalLink item2 in m_WebKitInternalLinkCollection)
							{
								if (item2.Href == htmlInternalLink.ID && item2.Href != "")
								{
									item2.HeaderTagLevel = htmlInternalLink.HeaderTagLevel;
									item2.ID = htmlInternalLink.ID;
									item2.HeaderContent = htmlInternalLink.HeaderContent;
									item2.DestinationPageNumber = htmlInternalLink.DestinationPageNumber;
									item2.Destination = new PointF(num10 * num6, num11 * num6);
									flag = true;
									break;
								}
							}
							if (!flag)
							{
								htmlInternalLink.Destination = new PointF(num10 * num6, num11 * num6);
								m_WebKitInternalLinkCollection.Add(htmlInternalLink);
							}
						}
						catch
						{
						}
						break;
					default:
						try
						{
							num8 = float.Parse(array3[1], CultureInfo.InvariantCulture);
							float dx = float.Parse(array3[2], CultureInfo.InvariantCulture);
							float dy = float.Parse(array3[3], CultureInfo.InvariantCulture);
							float dx2 = float.Parse(array3[4], CultureInfo.InvariantCulture);
							float dy2 = float.Parse(array3[5], CultureInfo.InvariantCulture);
							float.Parse(array3[6], CultureInfo.InvariantCulture);
							float num9 = float.Parse(array3[7], CultureInfo.InvariantCulture);
							Matrix matrix = new Matrix(1f, 0f, 0f, 1f, 0f, 0f);
							matrix.Multiply(new Matrix(num3, 0f, 0f, num3, 0f, 0f));
							matrix.Multiply(new Matrix(num8, 0f, 0f, num8, dx, dy));
							matrix.Multiply(new Matrix(1f, 0f, 0f, 1f, dx2, dy2));
							num6 = num3 * num8;
							PdfUnitConvertor pdfUnitConvertor = new PdfUnitConvertor(96f);
							float y = pdfUnitConvertor.ConvertToPixels(matrix.OffsetY, PdfGraphicsUnit.Point);
							float height = pdfUnitConvertor.ConvertToPixels(num9 * num6, PdfGraphicsUnit.Point);
							ImageRegion region = new ImageRegion(y, height);
							imageRegionManager.Add(region);
						}
						catch (Exception)
						{
						}
						break;
					}
					continue;
				}
				if (array3.Length == 2)
				{
					try
					{
						if (array3[0] == "shape")
						{
							num7 = num3 * float.Parse(array3[1].Trim(), CultureInfo.InvariantCulture);
						}
						else
						{
							num5 = float.Parse(array3[1].Trim(), CultureInfo.InvariantCulture);
						}
					}
					catch (Exception)
					{
					}
					continue;
				}
				try
				{
					num4++;
					if (array3.Length == 1)
					{
						num3 = float.Parse(array3[0].Trim(), CultureInfo.InvariantCulture);
						if (!pdfMetafileLayoutFormat.SplitTextLines)
						{
							list.Add(textRegionManager);
						}
						textRegionManager = new TextRegionManager();
						if (!pdfMetafileLayoutFormat.SplitImages)
						{
							list2.Add(imageRegionManager);
						}
						imageRegionManager = new ImageRegionManager();
					}
				}
				catch (Exception)
				{
				}
			}
			updateInternalLink();
			if (!pdfMetafileLayoutFormat.SplitTextLines)
			{
				list.Add(textRegionManager);
			}
			textRegionManager = new TextRegionManager();
			if (!pdfMetafileLayoutFormat.SplitImages)
			{
				list2.Add(imageRegionManager);
			}
			imageRegionManager = new ImageRegionManager();
			for (int k = 1; k < array.Length; k++)
			{
				num6 = ((num7 > 0f) ? num7 : num6);
				string[] array5 = Regex.Split(array[k], ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
				string id = array5[1];
				string text2 = array5[2];
				if (text2.StartsWith("\"") && text2.EndsWith("\""))
				{
					text2 = text2.Substring(1, text2.Length - 2);
					if (text2.StartsWith("\"\"") && text2.EndsWith("\"\""))
					{
						text2 = text2.Replace("\"\"", "\"");
					}
				}
				string value = text2;
				bool isReadonly = false;
				if (array5[3] == "readonly")
				{
					isReadonly = true;
				}
				bool selected = false;
				string text3 = array5[4];
				if (text3 == "checked" || text3 == "selected")
				{
					selected = true;
				}
				string optionValue = array5[5];
				string type = array5[6];
				int num26 = int.Parse(array5[7], CultureInfo.InvariantCulture);
				num26--;
				int num27 = int.Parse(array5[8], CultureInfo.InvariantCulture);
				int num28 = int.Parse(array5[9], CultureInfo.InvariantCulture);
				int num29 = int.Parse(array5[10], CultureInfo.InvariantCulture);
				int num30 = int.Parse(array5[11], CultureInfo.InvariantCulture);
				RectangleF bounds = new RectangleF((float)num27 * num6, (float)num28 * num6, (float)num29 * num6, (float)num30 * num6);
				HtmlToPdfAutoCreateForms item = new HtmlToPdfAutoCreateForms(id, value, isReadonly, selected, type, num26, bounds, optionValue);
				m_webkitAutoCreateForms.Add(item);
			}
			for (int l = 0; l < pdfLoadedDocument.Pages.Count; l++)
			{
				foreach (HtmlToPdfAutoCreateForms webkitAutoCreateForm in m_webkitAutoCreateForms)
				{
					if (webkitAutoCreateForm.ElementPageNo == l)
					{
						float dx5 = webkitAutoCreateForm.ElementBounds.X / num6;
						float dy5 = webkitAutoCreateForm.ElementBounds.Y / num6;
						_ = webkitAutoCreateForm.ElementBounds.Width / num6;
						float height3 = webkitAutoCreateForm.ElementBounds.Height / num6;
						Matrix matrix3 = new Matrix(1f, 0f, 0f, 1f, 0f, 0f);
						matrix3.Multiply(new Matrix(num3, 0f, 0f, num3, 0f, 0f));
						matrix3.Multiply(new Matrix(num8, 0f, 0f, num8, -1f, -1f));
						matrix3.Multiply(new Matrix(1f, 0f, 0f, 1f, dx5, dy5));
						ImageRegion region3 = new ImageRegion(new PdfUnitConvertor(96f).ConvertToPixels(matrix3.OffsetY, PdfGraphicsUnit.Point), height3);
						imageRegionManager2.Add(region3);
					}
				}
				list3.Add(imageRegionManager2);
				imageRegionManager2 = new ImageRegionManager();
			}
			PdfLayoutResult pdfLayoutResult2 = null;
			PdfPage pdfPage = null;
			double totalPageSize = 0.0;
			if (EnableForms)
			{
				layoutDetails = new PdfLayoutResult[pdfLoadedDocument.PageCount];
			}
			if (EnableBookmark || EnableToc)
			{
				m_WebKitInternalLinkCollection.Sort((HtmlInternalLink Header1, HtmlInternalLink Header2) => Header1.Destination.Y.CompareTo(Header2.Destination.Y));
				List<HtmlInternalLink> list4 = new List<HtmlInternalLink>();
				for (int m = 1; m <= pdfLoadedDocument.Pages.Count; m++)
				{
					foreach (HtmlInternalLink item3 in m_WebKitInternalLinkCollection)
					{
						if (item3.DestinationPageNumber == m)
						{
							list4.Add(item3);
						}
					}
				}
				m_WebKitInternalLinkCollection = list4;
			}
			PdfPage pdfPage2 = page as PdfPage;
			if (EnableToc && m_WebKitInternalLinkCollection.Count != 0)
			{
				Toc.TocPageCount = Toc.GetRectangleHeightAndTocPageCount(page, m_WebKitInternalLinkCollection);
				PdfMargins pdfMargins = new PdfMargins();
				pdfMargins.All = 0f;
				for (int n = 0; n < Toc.TocPageCount; n++)
				{
					pdfLoadedDocument.Pages.Insert(n, new SizeF(pdfPage2.GetClientSize().Width, pdfPage2.GetClientSize().Height), pdfMargins);
				}
			}
			PdfPageBase pdfPageBase = page;
			float num31 = 0f;
			if (SinglePageLayout != 0 && pdfLoadedDocument.PageCount > 1)
			{
				singlePdfDoc = new PdfDocument();
				float num32 = (int)Math.Ceiling(num5 * num6);
				PdfDocument document = (page as PdfPage).Document;
				num32 = num32 + document.PageSettings.Margins.Top + document.PageSettings.Margins.Bottom;
				if (SinglePageLayout == SinglePageLayout.FitWidth)
				{
					if (document.Template.Top != null)
					{
						num32 += document.Template.Top.Height;
					}
					if (document.Template.Bottom != null)
					{
						num32 += document.Template.Bottom.Height;
					}
					singlePdfDoc.Template = document.Template;
				}
				if (document.PageSettings.Size.Width < num32)
				{
					singlePdfDoc.PageSettings.Orientation = PdfPageOrientation.Portrait;
				}
				else
				{
					singlePdfDoc.PageSettings.Orientation = PdfPageOrientation.Landscape;
				}
				singlePdfDoc.PageSettings.Margins = document.PageSettings.Margins;
				singlePdfDoc.PageSettings.Rotate = document.PageSettings.Rotate;
				singlePdfDoc.PageSettings.Size = new SizeF(document.PageSettings.Size.Width, num32);
				page = singlePdfDoc.Pages.Add();
			}
			for (int num33 = 0; num33 < pdfLoadedDocument.Pages.Count; num33++)
			{
				PdfTemplate pdfTemplate = pdfLoadedDocument.Pages[num33].CreateTemplate();
				HtmlToPdfFormat htmlToPdfFormat = new HtmlToPdfFormat();
				htmlToPdfFormat.SplitTextLines = pdfMetafileLayoutFormat.SplitTextLines;
				htmlToPdfFormat.SplitImages = pdfMetafileLayoutFormat.SplitImages;
				htmlToPdfFormat.Layout = PdfLayoutType.Paginate;
				List<HtmlHyperLink> list5 = new List<HtmlHyperLink>();
				if (num33 >= Toc.TocPageCount)
				{
					foreach (HtmlHyperLink item4 in m_WebKitHyperlinkCollection)
					{
						if (item4.Name == (num33 - Toc.TocPageCount + 1).ToString())
						{
							if (pdfLayoutResult2 != null && num33 > Toc.TocPageCount && pdfLayoutResult2.Bounds.Height < (float)(int)(page as PdfPage).GetClientSize().Height)
							{
								item4.Bounds = new RectangleF(item4.Bounds.X, item4.Bounds.Y + pdfLayoutResult2.Bounds.Height, item4.Bounds.Width, item4.Bounds.Height);
							}
							list5.Add(item4);
						}
					}
					foreach (HtmlInternalLink item5 in m_WebKitInternalLinkCollection)
					{
						if (item5.SourcePageNumber == (num33 - Toc.TocPageCount + 1).ToString() && item5.DestinationPageNumber > 0)
						{
							if (pdfLayoutResult2 != null && num33 > Toc.TocPageCount)
							{
								item5.Bounds = new RectangleF(item5.Bounds.X, item5.Bounds.Y + pdfLayoutResult2.Bounds.Height, item5.Bounds.Width, item5.Bounds.Height);
							}
							item5.Destination = new PointF(item5.Destination.X, item5.Destination.Y);
							if (SinglePageLayout == SinglePageLayout.None)
							{
								item5.DestinationPage = pdfLoadedDocument.Pages[item5.DestinationPageNumber + Toc.TocPageCount - 1];
							}
							else
							{
								item5.DestinationPage = pdfLoadedDocument.Pages[0];
							}
							htmlToPdfFormat.HtmlInternalLinksCollection.Add(item5);
						}
					}
				}
				htmlToPdfFormat.HtmlHyperlinksCollection = list5;
				if (num33 >= Toc.TocPageCount)
				{
					if (!htmlToPdfFormat.SplitTextLines && num33 < list.Count + Toc.TocPageCount)
					{
						htmlToPdfFormat.TextRegionManager = list[num33 - Toc.TocPageCount];
					}
					if (!htmlToPdfFormat.SplitImages && num33 < list.Count + Toc.TocPageCount)
					{
						htmlToPdfFormat.ImageRegionManager = list2[num33 - Toc.TocPageCount];
					}
					if (EnableForms)
					{
						layoutDetails[num33 - Toc.TocPageCount] = pdfLayoutResult;
						htmlToPdfFormat.FormRegionManager = list3[num33 - Toc.TocPageCount];
					}
				}
				htmlToPdfFormat.TotalPageLayoutSize = num5 * num6;
				htmlToPdfFormat.PageNumber = num33 + 1;
				htmlToPdfFormat.PageCount = pdfLoadedDocument.Pages.Count;
				htmlToPdfFormat.TotalPageSize = totalPageSize;
				if (SinglePageLayout == SinglePageLayout.FitWidth && pdfLoadedDocument.PageCount > 1)
				{
					htmlToPdfFormat.Layout = PdfLayoutType.OnePage;
					pdfPage = page as PdfPage;
					if (pdfLayoutResult2 == null)
					{
						pdfLayoutResult2 = pdfTemplate.Draw(pdfPage, htmlToPdfFormat, new RectangleF(Location.X, Location.Y, pdfPage.GetClientSize().Width, pdfPage.GetClientSize().Height));
					}
					else
					{
						num31 += pdfLayoutResult2.Bounds.Bottom;
						pdfLayoutResult2 = pdfTemplate.Draw(pdfPage, htmlToPdfFormat, new RectangleF(Location.X, pdfLayoutResult2.Bounds.Bottom, pdfPage.GetClientSize().Width, num31));
					}
				}
				else if (pdfLayoutResult2 == null || pdfLayoutResult2.Bounds.Height > page.Size.Height)
				{
					pdfPage = page as PdfPage;
					pdfLayoutResult2 = pdfTemplate.Draw(pdfPage, htmlToPdfFormat, new RectangleF(Location.X, Location.Y, pdfPage.GetClientSize().Width, pdfPage.GetClientSize().Height));
				}
				else
				{
					pdfLayoutResult2 = pdfTemplate.Draw(pdfLayoutResult2.Page, htmlToPdfFormat, new RectangleF(Location.X, pdfLayoutResult2.Bounds.Bottom, pdfPage.GetClientSize().Width, pdfPage.GetClientSize().Height));
				}
				if (num33 >= Toc.TocPageCount)
				{
					totalPageSize = pdfLayoutResult2.TotalPageSize;
				}
				pdfLayoutResult = pdfLayoutResult2;
			}
			PdfDocument document2 = (page as PdfPage).Document;
			if (EnableBookmark && m_WebKitInternalLinkCollection.Count != 0)
			{
				HtmlInternalLink htmlInternalLink4 = new HtmlInternalLink();
				htmlInternalLink4.TocPageCount = Toc.TocPageCount;
				htmlInternalLink4.AddBookmark(pdfPage, document2, m_WebKitInternalLinkCollection);
			}
			if (EnableToc && m_WebKitInternalLinkCollection.Count != 0)
			{
				Toc.DrawTable(document2, pdfPage2, m_WebKitInternalLinkCollection);
			}
			if (document2 != null && m_WebKitInternalLinkCollection != null && m_WebKitInternalLinkCollection.Count > 0)
			{
				foreach (PdfPage page3 in document2.Pages)
				{
					for (int num34 = 0; num34 < page3.Annotations.Count; num34++)
					{
						if (!(page3.Annotations[num34] is PdfDocumentLinkAnnotation))
						{
							continue;
						}
						PdfDocumentLinkAnnotation pdfDocumentLinkAnnotation = page3.Annotations[num34] as PdfDocumentLinkAnnotation;
						if (pdfDocumentLinkAnnotation.Destination != null)
						{
							continue;
						}
						foreach (HtmlInternalLink item6 in m_WebKitInternalLinkCollection)
						{
							if (pdfDocumentLinkAnnotation.Text == item6.Href)
							{
								PdfPage page2 = document2.Pages[item6.DestinationPageNumber + Toc.TocPageCount - 1];
								pdfDocumentLinkAnnotation.Destination = new PdfDestination(page2);
								pdfDocumentLinkAnnotation.Destination.Location = item6.Destination;
								pdfDocumentLinkAnnotation.Destination.isModified = false;
								break;
							}
						}
					}
				}
			}
			if (EnableForms)
			{
				PdfDocument document3 = (page as PdfPage).Document;
				createPdfForms(document3);
			}
			double num35 = (double)(num5 * num6) - pdfLayoutResult2.TotalPageSize + (double)pdfLayoutResult2.Bounds.Height;
			pdfLayoutResult = new PdfLayoutResult(pdfLayoutResult.Page, new RectangleF(pdfLayoutResult.Bounds.X, pdfLayoutResult.Bounds.Y, pdfLayoutResult.Bounds.Width, (float)num35));
			if (pdfLoadedDocument.PageCount == 1 && pdfLayoutResult2.TotalPageSize > (double)pdfLayoutResult.Page.Size.Height)
			{
				PdfPage pdfPage4 = page as PdfPage;
				pdfPage4.Document.Pages.Remove(pdfLayoutResult.Page);
				pdfLayoutResult = new PdfLayoutResult(pdfPage4, new RectangleF(0f, 0f, pdfPage4.GetClientSize().Width, pdfPage4.GetClientSize().Height));
			}
			if (SinglePageLayout == SinglePageLayout.FitHeight && pdfLoadedDocument.PageCount > 1)
			{
				PdfDocument document4 = (pdfPageBase as PdfPage).Document;
				PdfTemplate pdfTemplate2 = page.CreateTemplate();
				float num36 = pdfTemplate2.Height / pdfPageBase.Size.Height;
				singlePdfDoc = new PdfDocument();
				singlePdfDoc.PageSettings.Margins = document4.PageSettings.Margins;
				singlePdfDoc.PageSettings.Rotate = document4.PageSettings.Rotate;
				float num37 = document4.PageSettings.Width / num36;
				if (num37 < document4.PageSettings.Height)
				{
					singlePdfDoc.PageSettings.Orientation = PdfPageOrientation.Portrait;
				}
				else
				{
					singlePdfDoc.PageSettings.Orientation = PdfPageOrientation.Landscape;
				}
				singlePdfDoc.PageSettings.Size = new SizeF(num37, document4.PageSettings.Height);
				pdfPageBase = singlePdfDoc.Pages.Add();
				pdfPageBase.Graphics.DrawPdfTemplate(pdfTemplate2, new PointF(0f, 0f), new SizeF(pdfPageBase.Size.Width, pdfPageBase.Size.Height));
			}
		}
		finally
		{
			if (IsTempDirectory)
			{
				Directory.Delete(new DirectoryInfo(WebKitFilePath).Parent.FullName, recursive: true);
			}
			DeleteFile(WebKitFilePath + ".txt");
			DeleteFile(WebKitFilePath + ".pdf");
		}
		return pdfLayoutResult;
	}

	internal void UpdateFormBounds(SizeF size)
	{
		for (int i = 0; i < layoutDetails.Length; i++)
		{
			foreach (HtmlToPdfAutoCreateForms webkitAutoCreateForm in m_webkitAutoCreateForms)
			{
				if (webkitAutoCreateForm.ElementPageNo == i && layoutDetails[i].Bounds.Height < size.Height)
				{
					webkitAutoCreateForm.ElementBounds = new RectangleF(webkitAutoCreateForm.ElementBounds.X, webkitAutoCreateForm.ElementBounds.Y + layoutDetails[i].Bounds.Height, webkitAutoCreateForm.ElementBounds.Width, webkitAutoCreateForm.ElementBounds.Height);
					if (webkitAutoCreateForm.ElementBounds.Y + webkitAutoCreateForm.ElementBounds.Height > size.Height)
					{
						webkitAutoCreateForm.ElementPageNo++;
						webkitAutoCreateForm.ElementBounds = new RectangleF(webkitAutoCreateForm.ElementBounds.X, webkitAutoCreateForm.ElementBounds.Y - size.Height - layoutDetails[i].Bounds.Height, webkitAutoCreateForm.ElementBounds.Width, webkitAutoCreateForm.ElementBounds.Height);
					}
				}
			}
		}
	}

	internal void updateInternalLink()
	{
		foreach (HtmlInternalLink item in m_internalLinkDestination)
		{
			foreach (HtmlInternalLink item2 in m_WebKitInternalLinkCollection)
			{
				if (item2.Href == item.ID)
				{
					item2.DestinationPageNumber = item.DestinationPageNumber;
					item2.Destination = item.Destination;
				}
			}
		}
	}

	internal void createPdfForms(PdfDocument lDoc)
	{
		SizeF size = Size.Empty;
		if (lDoc != null && lDoc.Pages.Count >= 0)
		{
			size = lDoc.Pages[0].GetClientSize();
		}
		UpdateFormBounds(size);
		for (int i = 0; i < m_webkitAutoCreateForms.Count; i++)
		{
			HtmlToPdfAutoCreateForms htmlToPdfAutoCreateForms = m_webkitAutoCreateForms[i];
			htmlToPdfAutoCreateForms.ElementPageNo += Toc.TocPageCount;
			if (htmlToPdfAutoCreateForms.ElementPageNo < 0)
			{
				continue;
			}
			if (htmlToPdfAutoCreateForms.ElementType == "text" || htmlToPdfAutoCreateForms.ElementType == "textarea" || htmlToPdfAutoCreateForms.ElementType == "input" || htmlToPdfAutoCreateForms.ElementType == "password" || htmlToPdfAutoCreateForms.ElementType == "number" || htmlToPdfAutoCreateForms.ElementType == "email")
			{
				if (htmlToPdfAutoCreateForms.ElementId == string.Empty)
				{
					htmlToPdfAutoCreateForms.ElementId = "text_" + Guid.NewGuid();
				}
				PdfTextBoxField pdfTextBoxField = new PdfTextBoxField(lDoc.Pages[htmlToPdfAutoCreateForms.ElementPageNo], htmlToPdfAutoCreateForms.ElementId);
				pdfTextBoxField.Bounds = htmlToPdfAutoCreateForms.ElementBounds;
				pdfTextBoxField.Text = htmlToPdfAutoCreateForms.ElementValue;
				pdfTextBoxField.ReadOnly = htmlToPdfAutoCreateForms.IsReadOnly;
				if (htmlToPdfAutoCreateForms.ElementType == "textarea")
				{
					pdfTextBoxField.Multiline = true;
					pdfTextBoxField.Scrollable = true;
				}
				lDoc.Form.Fields.Add(pdfTextBoxField);
			}
			else if (htmlToPdfAutoCreateForms.ElementType == "checkbox")
			{
				if (htmlToPdfAutoCreateForms.ElementId == string.Empty)
				{
					htmlToPdfAutoCreateForms.ElementId = "checkbox_" + Guid.NewGuid();
				}
				PdfCheckBoxField pdfCheckBoxField = new PdfCheckBoxField(lDoc.Pages[htmlToPdfAutoCreateForms.ElementPageNo], htmlToPdfAutoCreateForms.ElementId);
				pdfCheckBoxField.Bounds = htmlToPdfAutoCreateForms.ElementBounds;
				pdfCheckBoxField.Checked = htmlToPdfAutoCreateForms.IsSelected;
				pdfCheckBoxField.ReadOnly = htmlToPdfAutoCreateForms.IsReadOnly;
				lDoc.Form.Fields.Add(pdfCheckBoxField);
			}
			else if (htmlToPdfAutoCreateForms.ElementType.Equals("submit", StringComparison.CurrentCultureIgnoreCase) || (htmlToPdfAutoCreateForms.ElementType.Equals("button", StringComparison.CurrentCultureIgnoreCase) && htmlToPdfAutoCreateForms.ElementValue != ""))
			{
				if (htmlToPdfAutoCreateForms.ElementId == string.Empty)
				{
					htmlToPdfAutoCreateForms.ElementId = "submit_" + Guid.NewGuid();
				}
				PdfButtonField pdfButtonField = new PdfButtonField(lDoc.Pages[htmlToPdfAutoCreateForms.ElementPageNo], htmlToPdfAutoCreateForms.ElementId);
				pdfButtonField.Bounds = htmlToPdfAutoCreateForms.ElementBounds;
				pdfButtonField.Text = htmlToPdfAutoCreateForms.ElementValue;
				pdfButtonField.ReadOnly = htmlToPdfAutoCreateForms.IsReadOnly;
				lDoc.Form.Fields.Add(pdfButtonField);
			}
			else if (htmlToPdfAutoCreateForms.ElementType == "radio")
			{
				int num = 0;
				bool flag = true;
				PdfRadioButtonListField pdfRadioButtonListField = new PdfRadioButtonListField(lDoc.Pages[htmlToPdfAutoCreateForms.ElementPageNo], htmlToPdfAutoCreateForms.ElementId);
				foreach (HtmlToPdfAutoCreateForms webkitAutoCreateForm in m_webkitAutoCreateForms)
				{
					if (webkitAutoCreateForm.ElementPageNo >= 0 && htmlToPdfAutoCreateForms.ElementId == webkitAutoCreateForm.ElementId && htmlToPdfAutoCreateForms.ElementPageNo == webkitAutoCreateForm.ElementPageNo)
					{
						PdfRadioButtonListItem pdfRadioButtonListItem = new PdfRadioButtonListItem(webkitAutoCreateForm.ElementValue);
						pdfRadioButtonListItem.Bounds = webkitAutoCreateForm.ElementBounds;
						pdfRadioButtonListItem.ReadOnly = webkitAutoCreateForm.IsReadOnly;
						if (!webkitAutoCreateForm.IsSelected && flag)
						{
							num++;
						}
						else
						{
							flag = false;
						}
						pdfRadioButtonListField.Items.Add(pdfRadioButtonListItem);
					}
				}
				if (!flag && pdfRadioButtonListField.Items.Count != 0)
				{
					pdfRadioButtonListField.SelectedIndex = num;
				}
				lDoc.Form.Fields.Add(pdfRadioButtonListField);
			}
			else
			{
				if (!(htmlToPdfAutoCreateForms.ElementType == "select"))
				{
					continue;
				}
				int num2 = 0;
				bool flag2 = true;
				PdfComboBoxField pdfComboBoxField = (pdfComboBoxField = new PdfComboBoxField(lDoc.Pages[htmlToPdfAutoCreateForms.ElementPageNo], htmlToPdfAutoCreateForms.ElementId));
				foreach (HtmlToPdfAutoCreateForms webkitAutoCreateForm2 in m_webkitAutoCreateForms)
				{
					if (htmlToPdfAutoCreateForms.ElementBounds == webkitAutoCreateForm2.ElementBounds && webkitAutoCreateForm2.ElementBounds != m_webkitAutoCreateForms[i - 1].ElementBounds)
					{
						PdfListFieldItem item = new PdfListFieldItem(webkitAutoCreateForm2.ElementValue, webkitAutoCreateForm2.OptionValue);
						pdfComboBoxField.Bounds = webkitAutoCreateForm2.ElementBounds;
						if (webkitAutoCreateForm2.IsSelected)
						{
							pdfComboBoxField.ReadOnly = webkitAutoCreateForm2.IsReadOnly;
						}
						if (!webkitAutoCreateForm2.IsSelected && flag2)
						{
							num2++;
						}
						else
						{
							flag2 = false;
						}
						pdfComboBoxField.Items.Add(item);
					}
				}
				if (!flag2)
				{
					if (pdfComboBoxField.Items.Count != 0)
					{
						pdfComboBoxField.SelectedIndex = num2;
					}
				}
				else if (pdfComboBoxField.Items.Count != 0)
				{
					pdfComboBoxField.SelectedIndex = 0;
				}
				lDoc.Form.Fields.Add(pdfComboBoxField);
			}
		}
	}

	void IDisposable.Dispose()
	{
	}
}
