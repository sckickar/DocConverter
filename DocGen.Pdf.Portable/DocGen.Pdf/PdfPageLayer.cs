using System;
using System.Text;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

public class PdfPageLayer : IPdfWrapper
{
	private PdfPageBase m_page;

	internal PdfGraphics m_graphics;

	internal PdfStream m_content;

	internal PdfGraphicsState m_graphicsState;

	private bool m_clipPageTemplates;

	private bool m_bSaved;

	private PdfColorSpace m_colorspace;

	private string m_layerid;

	private string m_name;

	internal bool m_visible = true;

	private PdfPageLayerCollection m_layer;

	internal bool m_sublayer;

	internal long m_contentLength;

	internal PdfDictionary m_printOption;

	internal PdfDictionary m_usage;

	private PdfPrintState printState;

	private bool m_isEndState;

	private PdfDictionary m_dictionary = new PdfDictionary();

	private PdfReferenceHolder m_refholder;

	internal bool isResourceLayer;

	internal PdfReferenceHolder ReferenceHolder
	{
		get
		{
			return m_refholder;
		}
		set
		{
			m_refholder = value;
		}
	}

	internal PdfDictionary Dictionary
	{
		get
		{
			return m_dictionary;
		}
		set
		{
			m_dictionary = value;
		}
	}

	internal PdfColorSpace Colorspace
	{
		get
		{
			return m_colorspace;
		}
		set
		{
			m_colorspace = value;
		}
	}

	public PdfPageBase Page => m_page;

	internal string LayerId
	{
		get
		{
			return m_layerid;
		}
		set
		{
			m_layerid = value;
		}
	}

	public string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
			if (Dictionary != null && value != null)
			{
				Dictionary["Name"] = new PdfString(value);
			}
		}
	}

	public bool Visible
	{
		get
		{
			if (Dictionary != null && Dictionary.ContainsKey("Visible"))
			{
				m_visible = (Dictionary["Visible"] as PdfBoolean).Value;
			}
			return m_visible;
		}
		set
		{
			m_visible = value;
			if (Dictionary != null)
			{
				Dictionary.SetProperty("Visible", new PdfBoolean(value));
			}
			SetVisibility(value);
		}
	}

	public PdfGraphics Graphics
	{
		get
		{
			if (m_graphics == null || m_bSaved)
			{
				InitializeGraphics(Page);
			}
			return m_graphics;
		}
	}

	public PdfPrintState PrintState
	{
		get
		{
			return printState;
		}
		set
		{
			printState = value;
			if (m_printOption != null)
			{
				if (printState.Equals(PdfPrintState.AlwaysPrint))
				{
					m_printOption.SetProperty("PrintState", new PdfName("ON"));
				}
				else if (PrintState.Equals(PdfPrintState.NeverPrint))
				{
					m_printOption.SetProperty("PrintState", new PdfName("OFF"));
				}
			}
		}
	}

	public PdfPageLayerCollection Layers
	{
		get
		{
			if (m_layer == null)
			{
				m_layer = new PdfPageLayerCollection(Page);
			}
			m_layer.m_sublayer = true;
			return m_layer;
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_content;

	public PdfPageLayer Add()
	{
		return new PdfPageLayer(m_page)
		{
			Name = string.Empty
		};
	}

	public PdfPageLayer(PdfPageBase page)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		m_page = page;
		m_clipPageTemplates = true;
		m_content = new PdfStream();
	}

	internal PdfPageLayer(PdfPageBase page, PdfStream stream)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_page = page;
		m_content = stream;
	}

	internal PdfPageLayer(PdfPageBase page, bool clipPageTemplates)
		: this(page)
	{
		m_clipPageTemplates = clipPageTemplates;
	}

	private void InitializeGraphics(PdfPageBase page)
	{
		PdfPage pdfPage = page as PdfPage;
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 0f;
		float num7 = 0f;
		if (m_graphics == null)
		{
			PdfGraphics.GetResources resources = Page.GetResources;
			bool flag = false;
			bool flag2 = false;
			if (page.Dictionary.ContainsKey("MediaBox"))
			{
				flag = true;
			}
			PdfArray pdfArray = page.Dictionary.GetValue("MediaBox", "Parent") as PdfArray;
			if (pdfArray != null && (pdfArray[3] as PdfNumber).FloatValue < 0f)
			{
				float floatValue = (pdfArray[1] as PdfNumber).FloatValue;
				float floatValue2 = (pdfArray[3] as PdfNumber).FloatValue;
				(pdfArray[1] as PdfNumber).FloatValue = floatValue2;
				(pdfArray[3] as PdfNumber).FloatValue = floatValue;
			}
			PdfReferenceHolder element = new PdfReferenceHolder(this);
			if (pdfArray != null)
			{
				num = (pdfArray[0] as PdfNumber).FloatValue;
				num2 = (pdfArray[1] as PdfNumber).FloatValue;
				num3 = (pdfArray[2] as PdfNumber).FloatValue;
				num4 = (pdfArray[3] as PdfNumber).FloatValue;
			}
			PdfArray pdfArray2 = page.Dictionary.GetValue("CropBox", "Parent") as PdfArray;
			if (pdfArray2 != null && (pdfArray2[3] as PdfNumber).FloatValue < 0f)
			{
				float floatValue3 = (pdfArray2[1] as PdfNumber).FloatValue;
				float floatValue4 = (pdfArray2[3] as PdfNumber).FloatValue;
				(pdfArray2[1] as PdfNumber).FloatValue = floatValue4;
				(pdfArray2[3] as PdfNumber).FloatValue = floatValue3;
			}
			if (page.Dictionary.ContainsKey("CropBox"))
			{
				num5 = (pdfArray2[0] as PdfNumber).FloatValue;
				num6 = (pdfArray2[1] as PdfNumber).FloatValue;
				_ = (pdfArray2[2] as PdfNumber).FloatValue;
				num7 = (pdfArray2[3] as PdfNumber).FloatValue;
			}
			if (m_page != null && !m_page.m_parseLayerGraphics)
			{
				PdfArray contents = m_page.Contents;
				PdfStream pdfStream = new PdfStream();
				PdfStream pdfStream2 = new PdfStream();
				byte b = 113;
				byte b2 = 10;
				byte b3 = 81;
				byte b4 = 32;
				byte[] array = new byte[4] { b4, b, b4, b2 };
				pdfStream.Data = array;
				contents.Insert(0, new PdfReferenceHolder(pdfStream));
				array[0] = b4;
				array[1] = b3;
				array[2] = b4;
				array[3] = b2;
				pdfStream2.Data = array;
				contents.Insert(contents.Count, new PdfReferenceHolder(pdfStream2));
				m_page.m_parseLayerGraphics = true;
			}
			PdfArray pdfArray3 = null;
			if (page.Dictionary.ContainsKey("CropBox"))
			{
				if (page.Dictionary.GetValue("CropBox", "Parent") is PdfArray pdfArray4)
				{
					float floatValue5 = (pdfArray4[0] as PdfNumber).FloatValue;
					float floatValue6 = (pdfArray4[1] as PdfNumber).FloatValue;
					float floatValue7 = (pdfArray4[2] as PdfNumber).FloatValue;
					float floatValue8 = (pdfArray4[3] as PdfNumber).FloatValue;
					if ((floatValue5 < 0f || floatValue6 < 0f || floatValue7 < 0f || floatValue8 < 0f) && Math.Floor(Math.Abs(floatValue6)) == Math.Floor(Math.Abs(page.Size.Height)) && Math.Floor(Math.Abs(floatValue5)) == Math.Floor(Math.Abs(page.Size.Width)))
					{
						RectangleF rectangleF = new RectangleF(Math.Min(floatValue5, floatValue7), Math.Min(floatValue6, floatValue8), Math.Max(floatValue5, floatValue7), Math.Max(floatValue6, floatValue8));
						m_graphics = new PdfGraphics(new SizeF(rectangleF.Width, rectangleF.Height), resources, m_content);
						if (!page.Contents.Contains(element))
						{
							page.Contents.Add(element);
						}
					}
					else
					{
						m_graphics = new PdfGraphics(page.Size, resources, m_content);
						m_graphics.m_cropBox = pdfArray4;
						if (pdfArray != null)
						{
							m_graphics.mBox = pdfArray;
						}
						if (!page.Contents.Contains(element))
						{
							page.Contents.Add(element);
						}
					}
				}
				else
				{
					m_graphics = new PdfGraphics(page.Size, resources, m_content);
					if (!page.Contents.Contains(element))
					{
						page.Contents.Add(element);
					}
				}
			}
			else if ((num < 0f || num2 < 0f || num3 < 0f || num4 < 0f) && Math.Floor(Math.Abs(num2)) == Math.Floor(Math.Abs(page.Size.Height)) && Math.Floor(Math.Abs(num3)) == Math.Floor(page.Size.Width))
			{
				RectangleF rectangleF2 = new RectangleF(Math.Min(num, num3), Math.Min(num2, num4), Math.Max(num, num3), Math.Max(num2, num4));
				if (rectangleF2.Width <= 0f || rectangleF2.Height <= 0f)
				{
					flag2 = true;
					if (num < 0f)
					{
						num = 0f - num;
					}
					else if (num3 < 0f)
					{
						num3 = 0f - num3;
					}
					if (num2 < 0f)
					{
						num2 = 0f - num2;
					}
					else if (num4 < 0f)
					{
						num4 = 0f - num4;
					}
					rectangleF2.Size = new SizeF(Math.Max(num, num3), Math.Max(num2, num4));
				}
				m_graphics = new PdfGraphics(new SizeF(rectangleF2.Width, rectangleF2.Height), resources, m_content);
				if (!page.Contents.Contains(element))
				{
					page.Contents.Add(element);
				}
			}
			else
			{
				m_graphics = new PdfGraphics(page.Size, resources, m_content);
				if (!page.Contents.Contains(element))
				{
					page.Contents.Add(element);
				}
			}
			if (flag)
			{
				if (!flag2)
				{
					m_graphics.MediaBoxUpperRightBound = num4;
				}
				else
				{
					m_graphics.MediaBoxUpperRightBound = 0f - num2;
				}
			}
			if (pdfPage != null && pdfPage.Section != null && pdfPage.Section.Parent != null)
			{
				PdfSectionCollection parent = pdfPage.Section.Parent;
				if (parent != null && parent.Document != null)
				{
					_ = parent.Document.ColorSpace;
					m_graphics.ColorSpace = parent.Document.ColorSpace;
					Colorspace = parent.Document.ColorSpace;
				}
			}
			m_content.BeginSave += BeginSaveContent;
		}
		m_graphicsState = m_graphics.Save();
		if (!string.IsNullOrEmpty(m_name))
		{
			if (isResourceLayer && LayerId != null)
			{
				page.GetResources()?.AddProperties(LayerId, ReferenceHolder);
				if (isResourceLayer && !Dictionary.ContainsKey("LayerID"))
				{
					Dictionary["LayerID"] = new PdfName(LayerId);
				}
			}
			if (LayerId != null)
			{
				byte[] bytes = Encoding.UTF8.GetBytes("/OC /" + LayerId + " BDC\n");
				if (Name != null && Name != string.Empty)
				{
					m_graphics.StreamWriter.Write(bytes);
					m_isEndState = true;
				}
				else
				{
					m_content.Write(bytes);
				}
			}
		}
		if ((page.Origin.X >= 0f && page.Origin.Y >= 0f) || Math.Sign(page.Origin.X) != Math.Sign(page.Origin.Y))
		{
			m_page = page;
			PdfArray cropBox = m_graphics.m_cropBox;
			PdfArray mBox = m_graphics.mBox;
			if (cropBox != null && ((cropBox[0] as PdfNumber).FloatValue > 0f || (cropBox[1] as PdfNumber).FloatValue > 0f || m_graphics.Size.Width == (cropBox[2] as PdfNumber).FloatValue || m_graphics.Size.Height == (cropBox[3] as PdfNumber).FloatValue))
			{
				m_graphics.TranslateTransform((cropBox[0] as PdfNumber).FloatValue, 0f - (cropBox[3] as PdfNumber).FloatValue);
				if (mBox != null && (cropBox[3] as PdfNumber).FloatValue == 0f && (cropBox[1] as PdfNumber).FloatValue == (mBox[3] as PdfNumber).FloatValue && (cropBox[2] as PdfNumber).FloatValue == (mBox[2] as PdfNumber).FloatValue)
				{
					m_graphics.TranslateTransform((cropBox[0] as PdfNumber).FloatValue, 0f - (cropBox[1] as PdfNumber).FloatValue);
				}
			}
			else if (num != 0f || num2 != 0f || num5 != 0f || num6 != 0f)
			{
				if (num5 != 0f || num6 != 0f)
				{
					m_graphics.TranslateTransform(num5, 0f - num7);
				}
				else if (m_graphics.MediaBoxUpperRightBound == m_graphics.Size.Height || m_graphics.MediaBoxUpperRightBound == 0f)
				{
					m_graphics.TranslateTransform(num, 0f - m_graphics.Size.Height);
				}
				else
				{
					m_graphics.TranslateTransform(num, 0f - num4);
				}
			}
			else
			{
				m_graphics.InitializeCoordinates();
			}
		}
		else
		{
			m_graphics.InitializeCoordinates(page);
		}
		if (PdfGraphics.TransparencyObject)
		{
			m_graphics.SetTransparencyGroup(page);
		}
		if ((Page is PdfLoadedPage && page.Rotation != 0) || page.Dictionary.ContainsKey("Rotate"))
		{
			PdfNumber pdfNumber = null;
			if (page.Dictionary.ContainsKey("Rotate"))
			{
				pdfNumber = page.Dictionary["Rotate"] as PdfNumber;
				if (pdfNumber == null)
				{
					pdfNumber = PdfCrossTable.Dereference(page.Dictionary["Rotate"]) as PdfNumber;
				}
			}
			else if (page.Rotation != 0)
			{
				pdfNumber = new PdfNumber(0);
				if (page.Rotation == PdfPageRotateAngle.RotateAngle90)
				{
					pdfNumber.FloatValue = 90f;
				}
				if (page.Rotation == PdfPageRotateAngle.RotateAngle180)
				{
					pdfNumber.FloatValue = 180f;
				}
				if (page.Rotation == PdfPageRotateAngle.RotateAngle270)
				{
					pdfNumber.FloatValue = 270f;
				}
			}
			if (pdfNumber.FloatValue == 90f)
			{
				m_graphics.TranslateTransform(0f, page.Size.Height);
				m_graphics.RotateTransform(-90f);
				m_graphics.m_clipBounds.Size = new SizeF(page.Size.Height, page.Size.Width);
			}
			else if (pdfNumber.FloatValue == 180f)
			{
				m_graphics.TranslateTransform(page.Size.Width, page.Size.Height);
				m_graphics.RotateTransform(-180f);
			}
			else if (pdfNumber.FloatValue == 270f)
			{
				m_graphics.TranslateTransform(page.Size.Width, 0f);
				m_graphics.RotateTransform(-270f);
				m_graphics.m_clipBounds.Size = new SizeF(page.Size.Height, page.Size.Width);
			}
		}
		bool flag3 = false;
		if (pdfPage != null && pdfPage.Section != null && pdfPage.Section.Parent != null)
		{
			PdfSectionCollection parent2 = pdfPage.Section.Parent;
			if (parent2 != null && parent2.Document != null && (pdfPage.Section.Document.Template.Top != null || pdfPage.Section.Document.Template.Bottom != null))
			{
				flag3 = true;
			}
			else if (parent2 != null && parent2.Document != null)
			{
				flag3 = pdfPage.Section.ContainsTemplates(parent2.Document, pdfPage, foreground: false);
			}
		}
		if (pdfPage != null && pdfPage.Section != null && pdfPage.Section.PageSettings != null && (!pdfPage.Imported || flag3))
		{
			RectangleF actualBounds = pdfPage.Section.GetActualBounds(pdfPage, includeMargins: true);
			PdfMargins margins = pdfPage.Section.PageSettings.Margins;
			if (margins != null)
			{
				if (m_clipPageTemplates)
				{
					if (page.Origin.X >= 0f && page.Origin.Y >= 0f)
					{
						m_graphics.ClipTranslateMargins(actualBounds);
					}
				}
				else
				{
					m_graphics.ClipTranslateMargins(actualBounds.X, actualBounds.Y, margins.Left, margins.Top, margins.Right, margins.Bottom);
				}
			}
		}
		m_graphics.SetLayer(this);
		m_bSaved = false;
	}

	internal void Clear()
	{
		if (m_graphics != null)
		{
			m_graphics.StreamWriter.Clear();
		}
		if (m_content != null)
		{
			m_content = null;
		}
		if (m_graphics != null)
		{
			m_graphics = null;
		}
	}

	private void SetVisibility(bool value)
	{
		PdfCatalog obj = ((m_page is PdfPage) ? (m_page as PdfPage).Document.Catalog : (m_page as PdfLoadedPage).Document.Catalog);
		PdfDictionary pdfDictionary = null;
		if (obj.ContainsKey("OCProperties"))
		{
			if (m_page is PdfPage)
			{
				pdfDictionary = PdfCrossTable.Dereference((m_page as PdfPage).Document.Catalog["OCProperties"]) as PdfDictionary;
			}
			else if (m_page is PdfLoadedPage)
			{
				pdfDictionary = PdfCrossTable.Dereference((m_page as PdfLoadedPage).Document.Catalog["OCProperties"]) as PdfDictionary;
			}
		}
		if (pdfDictionary == null || !(pdfDictionary["D"] is PdfDictionary pdfDictionary2))
		{
			return;
		}
		PdfArray pdfArray = pdfDictionary2["ON"] as PdfArray;
		PdfArray pdfArray2 = pdfDictionary2["OFF"] as PdfArray;
		_ = m_refholder;
		if (!(m_refholder != null))
		{
			return;
		}
		if (!value)
		{
			pdfArray?.Remove(m_refholder);
			if (pdfArray2 == null)
			{
				pdfArray2 = new PdfArray();
				pdfDictionary2.Items.Add(new PdfName("OFF"), pdfArray2);
			}
			pdfArray2.Add(m_refholder);
		}
		else if (value)
		{
			pdfArray2?.Remove(m_refholder);
			if (pdfArray == null)
			{
				pdfArray = new PdfArray();
				pdfDictionary2.Items.Add(new PdfName("ON"), pdfArray);
			}
			pdfArray.Add(m_refholder);
		}
	}

	private void BeginSaveContent(object sender, SavePdfPrimitiveEventArgs e)
	{
		if (m_graphicsState != null)
		{
			if (m_isEndState)
			{
				Graphics.StreamWriter.Write("EMC\n");
				m_isEndState = false;
			}
			Graphics.Restore(m_graphicsState);
			m_graphicsState = null;
		}
		m_bSaved = true;
	}
}
