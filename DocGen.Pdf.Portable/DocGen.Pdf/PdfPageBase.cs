using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Graphics.Fonts;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;
using DocGen.PdfViewer.Base;

namespace DocGen.Pdf;

public abstract class PdfPageBase : IPdfWrapper
{
	private class SplitWord
	{
		internal string Text;

		internal bool IsManuallySplit;

		internal SplitWord(string word, bool isManuallySplit)
		{
			Text = word;
			IsManuallySplit = isManuallySplit;
		}
	}

	private Color TextColor;

	internal bool isFlatten;

	private PdfDictionary m_pageDictionary;

	internal List<string> pageWords = new List<string>();

	private bool isExtractWithFormat;

	private List<IPdfPrimitive> pdfPrimitivesCollection;

	private PdfResources m_resources;

	private PdfPageLayerCollection m_layers;

	private PdfLoadedAnnotationCollection m_annotations;

	private int m_defLayerIndex = -1;

	private bool m_initializeGraphics;

	internal bool m_parseLayerGraphics;

	internal bool m_isRedactionPage;

	private List<PdfName> m_fontNames = new List<PdfName>();

	internal List<IPdfPrimitive> m_fontReference = new List<IPdfPrimitive>();

	private Dictionary<PdfName, IPdfPrimitive> m_fontcollect;

	private PdfTemplate m_contentTemplate;

	private int m_annotCount;

	private int m_layersCount;

	private long m_pageContentLength;

	private bool m_imported;

	private static object s_syncLockTemplate = new object();

	private int m_fieldCount;

	internal List<PdfStream> m_xObjectContentStream;

	private PdfFormFieldsTabOrder m_formFieldsTabOrder;

	private PdfPageRotateAngle m_LoadedrotateAngle;

	internal bool m_isProgressOn;

	internal bool m_removedPage;

	internal bool isExtractImages;

	internal bool isFlateCompress;

	private bool m_isTagged;

	private Dictionary<long, PdfDictionary> annotMapReference;

	private List<IPdfPrimitive> m_mcrContentType;

	private List<IPdfPrimitive> m_mcrObjType;

	private bool m_modified;

	private Dictionary<int, string> m_imageLengthDict = new Dictionary<int, string>();

	private bool m_isContainsImage;

	private int m_nonBreakingSpaceCharValue = 160;

	private long resourceNumber;

	internal List<RectangleF> m_RedactionBounds = new List<RectangleF>();

	internal bool is_Contains_Redaction;

	private float pt = 1.3333f;

	internal PdfArray m_childSTR = new PdfArray();

	private PdfDocumentBase m_documentBase;

	internal bool templateResource;

	internal List<PdfReference> m_xobjectReferenceCollection;

	internal int m_id;

	private Dictionary<int, string> m_abbreviationCollection;

	private Dictionary<int, List<TextElement>> m_mcidAndTextElements;

	private Dictionary<int, Dictionary<RectangleF, bool>> m_mcidAndFigureBounds;

	private Bidi m_bidiInstance;

	internal bool m_visualOrder = true;

	private bool m_hasRTL;

	internal List<PdfReference> repeatedReferenceCollection;

	internal int importPageEndIndex = -1;

	internal int importPageStartIndex = -1;

	internal bool m_isBooklet;

	internal bool isSkipColorSpace;

	private string resultantText;

	private float m_wordSpacing;

	private float TextHorizontalScaling = 100f;

	private float m_fontSize;

	private float m_previousFontSize;

	private DocGen.PdfViewer.Base.Matrix m_previousTextMatrix;

	private DocGen.PdfViewer.Base.Matrix m_textLineMatrix;

	private DocGen.PdfViewer.Base.Matrix m_currentTextMatrix;

	internal DocGen.PdfViewer.Base.Matrix Ctm = DocGen.PdfViewer.Base.Matrix.Identity;

	internal int m_rise;

	private int m_horizontalScaling = 100;

	private int m_charID;

	private FontStructure m_structure;

	internal Dictionary<int, int> FontGlyphWidths;

	private double m_advancedWidth;

	private float m_charSizeMultiplier = 0.001f;

	private double m_characterWidth;

	private DocGen.PdfViewer.Base.Matrix m_textMatrix;

	private PdfUnitConvertor m_unitConvertor;

	private TransformationStack m_transformations;

	private bool m_isTextMatrix;

	private PointF m_currentLocation = PointF.Empty;

	private bool m_isRotated;

	private RectangleF m_boundingRect;

	private RectangleF m_tempBoundingRectangle;

	private float m_textLeading;

	private string m_strBackup = string.Empty;

	private bool m_hasNoSpacing;

	private bool m_hasLeading;

	private bool m_hasTj;

	private bool m_hasTm;

	private bool m_hasET;

	private bool m_hasBDC;

	private bool hasSpace;

	private PageResourceLoader m_resourceLoader = new PageResourceLoader();

	private PdfRecordCollection m_recordCollection;

	private Stack<PdfPageResources> m_parentResources = new Stack<PdfPageResources>();

	private PdfPageResources m_pageResources;

	private char[] m_symbolChars = new char[6] { '(', ')', '[', ']', '<', '>' };

	private string m_currentFont;

	private float m_characterSpacing;

	private bool m_isLayoutTextExtraction;

	private bool spaceBetweenWord;

	private bool contentOptimize;

	private bool annotArrayModified;

	private bool annotOptimize;

	private Dictionary<PdfDictionary, PdfArray> colorSpaceList = new Dictionary<PdfDictionary, PdfArray>();

	public PdfGraphics Graphics => DefaultLayer.Graphics;

	internal bool Imported
	{
		get
		{
			return m_imported;
		}
		set
		{
			m_imported = value;
		}
	}

	public PdfPageLayerCollection Layers
	{
		get
		{
			if (m_layers == null || (!m_parseLayerGraphics && m_isRedactionPage))
			{
				if (m_initializeGraphics)
				{
					m_layers = new PdfPageLayerCollection(this);
				}
				else
				{
					m_layers = new PdfPageLayerCollection(this, initializeLayer: true);
				}
			}
			return m_layers;
		}
	}

	public PdfLoadedAnnotationCollection Annotations
	{
		get
		{
			PdfLoadedAnnotationCollection annotations = null;
			if (this is PdfLoadedPage pdfLoadedPage)
			{
				annotations = pdfLoadedPage.Annotations;
			}
			m_annotations = annotations;
			if ((m_annotations == null || (m_annotations.Annotations.Count == 0 && m_annotations.Count != 0)) && this is PdfLoadedPage)
			{
				m_annotations = new PdfLoadedAnnotationCollection(this as PdfLoadedPage);
			}
			return m_annotations;
		}
	}

	public PdfFormFieldsTabOrder FormFieldsTabOrder
	{
		get
		{
			return m_formFieldsTabOrder = ObtainTabOrder();
		}
		set
		{
			m_formFieldsTabOrder = value;
			if (m_formFieldsTabOrder != 0)
			{
				string value2 = "";
				if (m_formFieldsTabOrder == PdfFormFieldsTabOrder.Row)
				{
					value2 = "R";
				}
				if (m_formFieldsTabOrder == PdfFormFieldsTabOrder.Column)
				{
					value2 = "C";
				}
				if (m_formFieldsTabOrder == PdfFormFieldsTabOrder.Structure)
				{
					value2 = "S";
				}
				Dictionary["Tabs"] = new PdfName(value2);
			}
		}
	}

	public int DefaultLayerIndex
	{
		get
		{
			if (Layers.Count == 0 || m_defLayerIndex == -1)
			{
				PdfPageLayer layer = Layers.Add();
				m_defLayerIndex = Layers.IndexOf(layer);
			}
			return m_defLayerIndex;
		}
		set
		{
			if (value < 0 || value > Layers.Count - 1)
			{
				throw new ArgumentOutOfRangeException("value", "Index can not be less 0 and greater Layers.Count - 1");
			}
			m_defLayerIndex = value;
			m_modified = true;
		}
	}

	public PdfPageLayer DefaultLayer
	{
		get
		{
			m_initializeGraphics = true;
			PdfPageLayer result = Layers[DefaultLayerIndex];
			m_initializeGraphics = false;
			return result;
		}
	}

	public abstract SizeF Size { get; }

	internal abstract PointF Origin { get; }

	internal PdfArray Contents
	{
		get
		{
			IPdfPrimitive pdfPrimitive = m_pageDictionary["Contents"];
			PdfArray pdfArray = pdfPrimitive as PdfArray;
			PdfReferenceHolder pdfReferenceHolder = pdfPrimitive as PdfReferenceHolder;
			if (pdfReferenceHolder != null)
			{
				pdfArray = pdfReferenceHolder.Object as PdfArray;
				if (pdfArray == null && pdfReferenceHolder.Object is PdfStream obj)
				{
					pdfArray = new PdfArray();
					pdfArray.Add(new PdfReferenceHolder(obj));
					m_pageDictionary["Contents"] = pdfArray;
				}
			}
			if (pdfArray == null)
			{
				pdfArray = new PdfArray();
				m_pageDictionary["Contents"] = pdfArray;
			}
			return pdfArray;
		}
	}

	internal PdfDictionary Dictionary => m_pageDictionary;

	public PdfPageRotateAngle Rotation
	{
		get
		{
			m_LoadedrotateAngle = ObtainRotation();
			return m_LoadedrotateAngle;
		}
		set
		{
			PdfPage pdfPage = this as PdfPage;
			if (this is PdfLoadedPage || (pdfPage != null && pdfPage.Imported))
			{
				m_LoadedrotateAngle = value;
				int num = 90;
				int num2 = 360;
				int num3 = (int)m_LoadedrotateAngle * num;
				if (num3 >= 360)
				{
					num3 %= num2;
				}
				PdfNumber value2 = new PdfNumber(num3);
				Dictionary["Rotate"] = value2;
			}
		}
	}

	internal PdfPageOrientation Orientation => ObtainOrientation();

	internal PdfTemplate ContentTemplate
	{
		get
		{
			bool flag = false;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				Layers.CombineContent(memoryStream);
				if (m_pageContentLength != memoryStream.Length)
				{
					flag = true;
				}
			}
			if (m_contentTemplate == null || m_contentTemplate.m_content.Data.Length == 0 || m_layersCount != ((m_layers != null) ? m_layers.Count : 0) || m_annotCount != GetAnnotationCount())
			{
				flag = true;
			}
			if (m_modified || flag)
			{
				m_contentTemplate = GetContent();
			}
			return m_contentTemplate;
		}
	}

	internal int FieldsCount => m_fieldCount;

	internal PdfDocumentBase DestinationDocument
	{
		get
		{
			return m_documentBase;
		}
		set
		{
			m_documentBase = value;
		}
	}

	internal bool IsTagged
	{
		get
		{
			return m_isTagged;
		}
		set
		{
			m_isTagged = value;
		}
	}

	internal Dictionary<long, PdfDictionary> ImportedAnnotationReference
	{
		get
		{
			if (annotMapReference == null)
			{
				annotMapReference = new Dictionary<long, PdfDictionary>();
			}
			return annotMapReference;
		}
	}

	internal List<IPdfPrimitive> McrContentCollection
	{
		get
		{
			return m_mcrContentType;
		}
		set
		{
			m_mcrContentType = value;
		}
	}

	internal List<IPdfPrimitive> McrObjectCollection
	{
		get
		{
			return m_mcrObjType;
		}
		set
		{
			m_mcrObjType = value;
		}
	}

	private PointF CurrentLocation
	{
		get
		{
			return m_currentLocation;
		}
		set
		{
			m_currentLocation = value;
		}
	}

	internal PdfUnitConvertor UnitConvertor
	{
		get
		{
			if (m_unitConvertor == null)
			{
				m_unitConvertor = new PdfUnitConvertor();
			}
			return m_unitConvertor;
		}
	}

	public bool IsBlank
	{
		get
		{
			if (this is PdfLoadedPage)
			{
				int count = Annotations.Count;
				int contentLength = GetContentLength();
				int num = 0;
				PdfResources pdfResources = null;
				if (Dictionary.ContainsKey("Resources") && PdfCrossTable.Dereference(Dictionary["Resources"]) is PdfDictionary baseDictionary)
				{
					pdfResources = new PdfResources(baseDictionary);
				}
				if (pdfResources != null && pdfResources.ContainsKey("XObject") && (pdfResources["XObject"] is PdfDictionary || pdfResources["XObject"] is PdfReferenceHolder))
				{
					PdfDictionary pdfDictionary = PdfCrossTable.Dereference(pdfResources["XObject"]) as PdfDictionary;
					if (pdfDictionary == null)
					{
						PdfCrossTable.Dereference(PdfCrossTable.Dereference(pdfResources["XObject"]) as PdfReferenceHolder);
					}
					if (pdfDictionary != null)
					{
						num = pdfDictionary.Count;
					}
				}
				if (count > 0 || contentLength > 0 || num > 0)
				{
					return false;
				}
				return true;
			}
			if (this is PdfPage)
			{
				int count2 = (this as PdfPage).Annotations.Count;
				int contentLength2 = GetContentLength();
				int num2 = 0;
				PdfResources resources = GetResources();
				if (resources.ContainsKey("XObject") && (resources["XObject"] is PdfDictionary || resources["XObject"] is PdfReferenceHolder))
				{
					PdfDictionary pdfDictionary2 = PdfCrossTable.Dereference(resources["XObject"]) as PdfDictionary;
					if (pdfDictionary2 == null)
					{
						PdfCrossTable.Dereference(PdfCrossTable.Dereference(resources["XObject"]) as PdfReferenceHolder);
					}
					if (pdfDictionary2 != null)
					{
						num2 = pdfDictionary2.Count;
					}
				}
				if (count2 > 0 || contentLength2 > 0 || num2 > 0)
				{
					return false;
				}
				return true;
			}
			return false;
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_pageDictionary;

	internal PdfPageBase(PdfDictionary dic)
	{
		if (dic == null)
		{
			throw new ArgumentNullException("dic");
		}
		m_pageDictionary = dic;
	}

	public PdfTemplate CreateTemplate()
	{
		PdfLoadedPage pdfLoadedPage = this as PdfLoadedPage;
		PdfLoadedDocument pdfLoadedDocument = null;
		if (pdfLoadedPage != null && pdfLoadedPage.Document != null)
		{
			pdfLoadedDocument = pdfLoadedPage.Document as PdfLoadedDocument;
		}
		PdfTemplate content = GetContent();
		if (pdfLoadedDocument != null && pdfLoadedDocument.CrossTable != null && !pdfLoadedDocument.CrossTable.IsPDFAppend)
		{
			pdfLoadedPage.Document.CrossTable.IsPDFAppend = true;
			if (pdfLoadedDocument.Form != null || (pdfLoadedPage.Annotations != null && pdfLoadedPage.Annotations.Count > 0))
			{
				content.isLoadedPageTemplate = false;
			}
		}
		if (pdfLoadedPage != null)
		{
			pdfLoadedDocument = pdfLoadedPage.Document as PdfLoadedDocument;
			if (pdfLoadedDocument != null && !pdfLoadedDocument.CrossTable.IsPDFAppend)
			{
				pdfLoadedPage.Document.CrossTable.IsPDFAppend = true;
				if (pdfLoadedDocument.Form != null || (pdfLoadedPage.Annotations != null && pdfLoadedPage.Annotations.Count > 0))
				{
					content.isLoadedPageTemplate = false;
				}
			}
			else if ((pdfLoadedDocument.CrossTable.IsPDFAppend && pdfLoadedPage.CropBox == RectangleF.Empty && pdfLoadedPage.MediaBox.Y != 0f) || pdfLoadedPage.MediaBox == RectangleF.Empty)
			{
				content.isLoadedPageTemplate = false;
			}
		}
		SetTemplateMatrix(content.m_content);
		if (Rotation != 0 || Rotation != 0)
		{
			SetMatrix(content);
		}
		return content;
	}

	private int GetContentLength()
	{
		int result = 0;
		if (Dictionary.ContainsKey("Contents"))
		{
			if (Dictionary["Contents"] is PdfArray && Dictionary["Contents"] is PdfArray pdfArray)
			{
				result = pdfArray.Count;
			}
			if (Dictionary["Contents"] is PdfReferenceHolder)
			{
				PdfReferenceHolder pdfReferenceHolder = PdfCrossTable.Dereference(Dictionary["Contents"]) as PdfReferenceHolder;
				if (pdfReferenceHolder != null)
				{
					if (PdfCrossTable.Dereference((PdfCrossTable.Dereference(pdfReferenceHolder) as PdfDictionary)["Length"]) is PdfNumber pdfNumber)
					{
						result = pdfNumber.IntValue;
					}
				}
				else if (PdfCrossTable.Dereference(Dictionary["Contents"]) is PdfStream obj && PdfCrossTable.Dereference((PdfCrossTable.Dereference(obj) as PdfDictionary)["Length"]) is PdfNumber pdfNumber2)
				{
					result = pdfNumber2.IntValue;
				}
			}
		}
		return result;
	}

	public string ExtractText(out TextLineCollection textLineCollection)
	{
		CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
		TextLineCollection textLineCollection2 = new TextLineCollection();
		DeviceCMYK cmyk = new DeviceCMYK();
		TextLine textLine = null;
		Page page = new Page(this);
		page.Initialize(this, needParsing: false);
		if (page.RecordCollection == null)
		{
			page.Initialize(this, needParsing: true);
		}
		ImageRenderer imageRenderer = new ImageRenderer(page.RecordCollection, page.Resources, page.Height, page.CurrentLeftLocation, cmyk);
		imageRenderer.isExtractLineCollection = true;
		imageRenderer.pageRotation = (float)page.Rotation;
		imageRenderer.RenderAsImage();
		imageRenderer.isExtractLineCollection = false;
		_ = string.Empty;
		int num = 0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		int num6 = 0;
		string text = string.Empty;
		int lineStartIndex = 0;
		int glyphIndex = 0;
		double num7 = 0.0;
		double num8 = 0.0;
		double previousBottomPosition = 0.0;
		double previousYPosition = 0.0;
		double num9 = 0.0;
		double num10 = 0.0;
		double num11 = 0.0;
		double num12 = 0.0;
		double num13 = 0.0;
		double num14 = 0.0;
		float num15 = 0f;
		m_hasRTL = false;
		string text2 = "";
		TextWord textWord = new TextWord();
		List<TextGlyph> list = new List<TextGlyph>();
		List<TextGlyph> list2 = new List<TextGlyph>();
		bool flag = false;
		if (imageRenderer.extractTextElement.Count == imageRenderer.imageRenderGlyphList.Count)
		{
			flag = true;
		}
		if (text == "")
		{
			foreach (DocGen.PdfViewer.Base.Glyph imageRenderGlyph in imageRenderer.imageRenderGlyphList)
			{
				text += imageRenderGlyph.ToUnicode;
			}
		}
		if (imageRenderer.imageRenderGlyphList.Count > 0)
		{
			textLine = new TextLine();
		}
		bool flag2 = false;
		if (!flag2 && Bidi.HasAnyRTL(text))
		{
			flag2 = true;
		}
		PdfArray cropOrMediaBox = null;
		cropOrMediaBox = GetCropOrMediaBox(this, cropOrMediaBox);
		if (cropOrMediaBox != null && cropOrMediaBox.Count > 3)
		{
			PdfNumber pdfNumber = cropOrMediaBox[0] as PdfNumber;
			PdfNumber pdfNumber2 = cropOrMediaBox[1] as PdfNumber;
			if (pdfNumber != null && pdfNumber2 != null && (pdfNumber.FloatValue != 0f || pdfNumber2.FloatValue != 0f))
			{
				for (int i = 0; i < imageRenderer.imageRenderGlyphList.Count; i++)
				{
					RectangleF bounds = imageRenderer.imageRenderGlyphList[i].BoundingRect;
					bounds = GetCropOrMediaBoxBounds(bounds, pdfNumber.FloatValue, pdfNumber2.FloatValue);
					imageRenderer.imageRenderGlyphList[i].BoundingRect = new Rect(bounds.X, bounds.Y, bounds.Width, bounds.Height);
				}
			}
		}
		for (int j = 0; j < imageRenderer.extractTextElement.Count; j++)
		{
			if (num >= imageRenderer.imageRenderGlyphList.Count)
			{
				continue;
			}
			if (page.Rotation == 270.0)
			{
				num7 = imageRenderer.imageRenderGlyphList[num].BoundingRect.X;
				num8 = imageRenderer.imageRenderGlyphList[num].BoundingRect.Right;
				num10 = imageRenderer.imageRenderGlyphList[num].BoundingRect.Y;
				if (j == 0)
				{
					previousBottomPosition = imageRenderer.imageRenderGlyphList[num].BoundingRect.Right;
					previousYPosition = (num6 = (int)imageRenderer.imageRenderGlyphList[num].BoundingRect.X);
					num9 = imageRenderer.imageRenderGlyphList[num].BoundingRect.Y;
				}
			}
			else if (page.Rotation == 0.0)
			{
				num7 = imageRenderer.imageRenderGlyphList[num].BoundingRect.Y;
				num8 = imageRenderer.imageRenderGlyphList[num].BoundingRect.Bottom;
				num10 = imageRenderer.imageRenderGlyphList[num].BoundingRect.X;
				if (j == 0)
				{
					previousBottomPosition = imageRenderer.imageRenderGlyphList[num].BoundingRect.Bottom;
					previousYPosition = (num6 = (int)imageRenderer.imageRenderGlyphList[num].BoundingRect.Y);
					num9 = imageRenderer.imageRenderGlyphList[num].BoundingRect.X;
				}
			}
			else
			{
				num7 = imageRenderer.imageRenderGlyphList[num].BoundingRect.Y;
			}
			if (((num != 0 && (int)num7 != num6 && imageRenderer.extractTextElement[j].renderedText != string.Empty && imageRenderer.extractTextElement[j].renderedText != "" && imageRenderer.extractTextElement[j].renderedText != " ") || num == imageRenderer.imageRenderGlyphList.Count - 1 || (num != 0 && (int)num7 != num6 && num9 > num10 && imageRenderer.extractTextElement[j].renderedText != string.Empty && imageRenderer.extractTextElement[j].renderedText != "" && imageRenderer.extractTextElement[j].renderedText == " " && !m_hasRTL)) && IsNotInSameLine(page.Rotation, num7, num8, num10, previousYPosition, previousBottomPosition, num9, j))
			{
				num6 = (int)num7;
				if (textLine.WordCollection.Count > 0)
				{
					AddLineCollection(textLine, textLineCollection2, imageRenderer, lineStartIndex, num);
				}
				lineStartIndex = num;
				textLine = new TextLine();
			}
			TextElement textElement = imageRenderer.extractTextElement[j];
			string renderedText = textElement.renderedText;
			SplitWord[] array = ((!flag || !(renderedText == " ")) ? SplitRenderedText(renderedText, imageRenderer.imageRenderGlyphList, num, flag2) : new SplitWord[1]
			{
				new SplitWord(renderedText, isManuallySplit: false)
			});
			textElement.Text = " ";
			TextWord textWord2 = null;
			bool flag3 = false;
			for (int k = 0; k < array.Length; k++)
			{
				if (text.Contains(array[k].Text) && array[k].Text.Length != 0)
				{
					textWord2 = new TextWord();
					bool flag4 = false;
					for (int l = num; l < num + array[k].Text.Length; l++)
					{
						TextGlyph textGlyph = new TextGlyph();
						textGlyph.FontName = textElement.FontName;
						textGlyph.FontSize = textElement.FontSize;
						textGlyph.FontStyle = textElement.FontStyle;
						textGlyph.TextColor = textElement.TextColor;
						if (!string.IsNullOrEmpty(imageRenderer.imageRenderGlyphList[l].ToUnicode))
						{
							textGlyph.Text = Convert.ToChar(imageRenderer.imageRenderGlyphList[l].ToUnicode[0]);
						}
						if (!m_hasRTL && Bidi.IsRTLChar(textGlyph.Text))
						{
							m_hasRTL = true;
						}
						textGlyph.Bounds = new RectangleF((float)imageRenderer.imageRenderGlyphList[l].BoundingRect.X, (float)imageRenderer.imageRenderGlyphList[l].BoundingRect.Y, (float)imageRenderer.imageRenderGlyphList[l].BoundingRect.Width, (float)imageRenderer.imageRenderGlyphList[l].BoundingRect.Height);
						if (imageRenderer.imageRenderGlyphList[l].IsRotated)
						{
							flag3 = true;
						}
						if (flag3 && (page.Rotation == 270.0 || page.Rotation == 90.0))
						{
							num3 += imageRenderer.imageRenderGlyphList[l].BoundingRect.Height;
						}
						else
						{
							num2 += imageRenderer.imageRenderGlyphList[l].BoundingRect.Width;
						}
						if (textWord2.Glyphs.Count > 1 && array.Length == 1 && (float)imageRenderer.imageRenderGlyphList[l].BoundingRect.Y == (float)imageRenderer.imageRenderGlyphList[l - 1].BoundingRect.Y && imageRenderer.imageRenderGlyphList[l - 1].AdvancedWidth > 0.0 && imageRenderer.imageRenderGlyphList[l - 1].Name != null && Math.Round((float)imageRenderer.imageRenderGlyphList[l].BoundingRect.X - 1.5f) > Math.Round((float)imageRenderer.imageRenderGlyphList[l - 1].BoundingRect.X + (float)imageRenderer.imageRenderGlyphList[l - 1].BoundingRect.Width))
						{
							TextGlyph textGlyph2 = new TextGlyph();
							textGlyph2.FontName = textElement.FontName;
							textGlyph2.FontSize = textElement.FontSize;
							textGlyph2.FontStyle = textElement.FontStyle;
							textGlyph2.TextColor = textElement.TextColor;
							textGlyph2.Text = ' ';
							float num16 = (float)imageRenderer.imageRenderGlyphList[l - 1].BoundingRect.X + (float)imageRenderer.imageRenderGlyphList[l - 1].BoundingRect.Width;
							float y = (float)imageRenderer.imageRenderGlyphList[l].BoundingRect.Y;
							float width = (float)imageRenderer.imageRenderGlyphList[l].BoundingRect.X - num16;
							float height = (float)imageRenderer.imageRenderGlyphList[l].BoundingRect.Height;
							textGlyph2.Bounds = new RectangleF(num16, y, width, height);
							textWord2.Glyphs.Add(textGlyph2);
							textWord2.Text = textGlyph2.Text.ToString();
							textLine.WordCollection.Add(textWord2);
							textWord2 = new TextWord();
							flag4 = true;
						}
						if (array.Length == 1)
						{
							list.Add(textGlyph);
						}
						else
						{
							list2.Add(textGlyph);
						}
						textWord2.Glyphs.Add(textGlyph);
					}
					num4 = imageRenderer.imageRenderGlyphList[num].BoundingRect.X;
					num5 = imageRenderer.imageRenderGlyphList[num].BoundingRect.Y;
					if (flag3 && (page.Rotation == 270.0 || page.Rotation == 90.0))
					{
						num2 = imageRenderer.imageRenderGlyphList[num].BoundingRect.Width;
					}
					else
					{
						num3 = imageRenderer.imageRenderGlyphList[num].BoundingRect.Height;
					}
					num2 = ((!(num4 > imageRenderer.imageRenderGlyphList[num + array[k].Text.Length - 1].BoundingRect.X)) ? (imageRenderer.imageRenderGlyphList[num + array[k].Text.Length - 1].BoundingRect.X + imageRenderer.imageRenderGlyphList[num + array[k].Text.Length - 1].BoundingRect.Width) : (num4 - imageRenderer.imageRenderGlyphList[num + array[k].Text.Length - 1].BoundingRect.X + imageRenderer.imageRenderGlyphList[num + array[k].Text.Length - 1].BoundingRect.Width));
					previousBottomPosition = num8;
					previousYPosition = num7;
					num9 = num10;
					num11 = imageRenderer.imageRenderGlyphList[num].BoundingRect.Width;
					textWord2.Bounds = new RectangleF((float)num4, (float)num5, (float)(num2 - num4), (float)num3);
					if (textWord2.Bounds.Width < textWord2.Bounds.Height && array[k].Text.Length > 2 && !Bidi.HasAnyRTL(array[k].Text) && num5 > imageRenderer.imageRenderGlyphList[num + array[k].Text.Length - 1].BoundingRect.Y)
					{
						num3 = num5 - imageRenderer.imageRenderGlyphList[num + array[k].Text.Length - 1].BoundingRect.Y + imageRenderer.imageRenderGlyphList[num + array[k].Text.Length - 1].BoundingRect.Height;
						textWord2.Bounds = new RectangleF((float)(num4 - (num2 - num4)), (float)(num5 - (num3 - (num2 - num4))), (float)(num2 - num4), (float)num3);
					}
					if (flag && array.Length == 1 && array[0].Text.Length == 1)
					{
						if (num + 1 < imageRenderer.imageRenderGlyphList.Count)
						{
							if (page.Rotation == 270.0)
							{
								num7 = imageRenderer.imageRenderGlyphList[num + 1].BoundingRect.X;
								num8 = imageRenderer.imageRenderGlyphList[num + 1].BoundingRect.Right;
								num10 = imageRenderer.imageRenderGlyphList[num + 1].BoundingRect.Y;
							}
							else if (page.Rotation == 0.0)
							{
								num7 = imageRenderer.imageRenderGlyphList[num + 1].BoundingRect.Y;
								num8 = imageRenderer.imageRenderGlyphList[num + 1].BoundingRect.Bottom;
								num10 = imageRenderer.imageRenderGlyphList[num + 1].BoundingRect.X;
							}
						}
						if (array[k].Text == " " || (((num != 0 && (int)num7 != num6 && imageRenderer.extractTextElement[j].renderedText != string.Empty && imageRenderer.extractTextElement[j].renderedText != "" && imageRenderer.extractTextElement[j].renderedText != " ") || num == imageRenderer.imageRenderGlyphList.Count - 1) && IsNotInSameLine(page.Rotation, num7, num8, num10, previousYPosition, previousBottomPosition, num9, j)))
						{
							if (array[k].Text[0] == '\u00a0')
							{
								num++;
								break;
							}
							if (textWord.Glyphs.Count <= 0)
							{
								textWord.Glyphs.Add(textWord2.Glyphs[0]);
							}
							if (array[k].Text != " ")
							{
								text2 += array[k].Text;
							}
							num4 = textWord.Glyphs[0].Bounds.X;
							num5 = textWord.Glyphs[0].Bounds.Y;
							if (flag3 && (page.Rotation == 270.0 || page.Rotation == 90.0))
							{
								num2 = textWord.Glyphs[0].Bounds.Width;
							}
							else
							{
								num3 = textWord.Glyphs[0].Bounds.Height;
							}
							if (num4 > imageRenderer.imageRenderGlyphList[num].BoundingRect.X)
							{
								num2 = num4 - imageRenderer.imageRenderGlyphList[num].BoundingRect.X + imageRenderer.imageRenderGlyphList[num].BoundingRect.Width;
							}
							else if (!flag3 || (page.Rotation != 270.0 && page.Rotation != 90.0))
							{
								num2 = imageRenderer.imageRenderGlyphList[num].BoundingRect.X + imageRenderer.imageRenderGlyphList[num].BoundingRect.Width;
							}
							textWord.Bounds = GetBounds(page.Rotation, flag3, (float)num4, (float)num5, (float)num2, (float)num3);
							if (text2.Length > 1)
							{
								textWord.Glyphs.Add(textWord2.Glyphs[0]);
								if (textWord2.Glyphs[0].Text == ' ' && !text2.EndsWith(" "))
								{
									text2 += textWord2.Glyphs[0].Text;
								}
							}
							textWord.Text = text2;
							textWord.FontName = textElement.FontName;
							textWord.FontSize = textElement.FontSize;
							textWord.FontStyle = textElement.FontStyle;
							textWord.TextColor = textElement.TextColor;
							textLine.WordCollection.Add(textWord);
							text2 = "";
							textWord = new TextWord();
							num2 = 0.0;
							num3 = 0.0;
							glyphIndex = num + 1;
						}
						else
						{
							text2 += array[k].Text;
							textWord.Glyphs.Add(textWord2.Glyphs[0]);
							if (Math.Round(num10 - 1.0) > Math.Round(num9 + num11))
							{
								textWord.Bounds = GetBounds(page.Rotation, flag3, textWord.Glyphs[0].Bounds.X, (float)num5, (float)num2, (float)num3);
								textWord.Text = text2;
								textWord.FontName = textElement.FontName;
								textWord.FontSize = textElement.FontSize;
								textWord.FontStyle = textElement.FontStyle;
								textWord.TextColor = textElement.TextColor;
								textLine.WordCollection.Add(textWord);
								text2 = "";
								textWord = new TextWord();
								num2 = 0.0;
								num3 = 0.0;
							}
						}
						num += array[k].Text.Length;
						if (num == imageRenderer.imageRenderGlyphList.Count && textWord.Glyphs.Count > 0)
						{
							textWord.Bounds = GetBounds(page.Rotation, flag3, textWord.Glyphs[0].Bounds.X, (float)num5, (float)num2, (float)num3);
							textWord.Text = text2;
							textWord.FontName = textElement.FontName;
							textWord.FontSize = textElement.FontSize;
							textWord.FontStyle = textElement.FontStyle;
							textWord.TextColor = textElement.TextColor;
							textLine.WordCollection.Add(textWord);
							text2 = "";
							textWord = new TextWord();
							num2 = 0.0;
							num3 = 0.0;
						}
					}
					else
					{
						num += array[k].Text.Length;
						string text3 = array[k].Text;
						if (m_visualOrder && !string.IsNullOrEmpty(text3) && Bidi.HasAnyRTL(text3))
						{
							if (m_bidiInstance == null)
							{
								m_bidiInstance = new Bidi();
							}
							text3 = m_bidiInstance.GetLogicalToVisualString(text3, isRTL: true);
						}
						if (flag4 && array.Length == 1 && textLine.WordCollection.Count == 1)
						{
							TextWord textWord3 = textLine.WordCollection[0];
							string text4 = string.Empty;
							foreach (TextGlyph glyph in textWord3.Glyphs)
							{
								text4 += glyph.Text;
							}
							text3 = text3.Remove(0, text4.Length - 1);
							textWord3.Text = text4;
							textWord3.FontSize = textElement.FontSize;
							textWord3.FontName = textElement.FontName;
							textWord3.FontStyle = textElement.FontStyle;
							textWord3.TextColor = textElement.TextColor;
							float width2 = textWord2.Bounds.Width;
							textWord3.Bounds = new RectangleF(textWord3.Glyphs[0].Bounds.X, textWord3.Glyphs[0].Bounds.Y, textWord3.Glyphs[textWord3.Glyphs.Count - 1].Bounds.X - textWord3.Glyphs[0].Bounds.X + textWord3.Glyphs[textWord3.Glyphs.Count - 1].Bounds.Width, textWord3.Glyphs[textWord3.Glyphs.Count - 1].Bounds.Height);
							textWord2.Bounds = new RectangleF(textWord3.Bounds.X + textWord3.Bounds.Width, textWord2.Bounds.Y, width2 - textWord3.Bounds.Width, textWord2.Bounds.Height);
						}
						bool flag5 = false;
						if (array.Length == 1 && !m_hasRTL)
						{
							text2 += text3;
							if (num15 == 0f)
							{
								num12 = textWord2.Bounds.X;
								num13 = textWord2.Bounds.Y;
							}
							num14 += (double)textWord2.Bounds.Width;
							if ((j + 1 < imageRenderer.extractTextElement.Count && (imageRenderer.extractTextElement[j + 1].renderedText.StartsWith(" ") || imageRenderer.extractTextElement[j + 1].renderedText == "")) || j + 1 == imageRenderer.extractTextElement.Count)
							{
								textWord2.Bounds = new RectangleF((float)num12, (float)num13, (float)num14, (float)num3);
								textWord2.Text = text2;
								textWord2.FontName = textElement.FontName;
								textWord2.FontSize = textElement.FontSize;
								textWord2.FontStyle = textElement.FontStyle;
								textWord2.TextColor = textElement.TextColor;
								textLine.WordCollection.Add(textWord2);
								if (list != null && list.Count > 0)
								{
									textWord2.Glyphs.Clear();
								}
								foreach (TextGlyph item in list)
								{
									textWord2.Glyphs.Add(item);
								}
								num12 = 0.0;
								num13 = 0.0;
								num15 = 0f;
								num14 = 0.0;
								num2 = 0.0;
								num3 = 0.0;
								list.Clear();
								text2 = "";
								flag5 = true;
							}
							else if (num < imageRenderer.imageRenderGlyphList.Count && Math.Round(imageRenderer.imageRenderGlyphList[num].BoundingRect.X) != Math.Round(textWord2.Bounds.X + textWord2.Bounds.Width))
							{
								if (text2 != string.Empty)
								{
									textWord2.Bounds = new RectangleF((float)num12, (float)num13, (float)num14, (float)num3);
									textWord2.Text = text2;
									textWord2.FontName = textElement.FontName;
									textWord2.FontSize = textElement.FontSize;
									textWord2.FontStyle = textElement.FontStyle;
									textWord2.TextColor = textElement.TextColor;
									if (list != null && list.Count > 0)
									{
										textWord2.Glyphs.Clear();
									}
									foreach (TextGlyph item2 in list)
									{
										textWord2.Glyphs.Add(item2);
									}
									num15 = 0f;
									num12 = 0.0;
									num13 = 0.0;
									num14 = 0.0;
									num2 = 0.0;
									num3 = 0.0;
									list.Clear();
									text2 = "";
								}
								textWord2.FontName = textElement.FontName;
								textWord2.FontSize = textElement.FontSize;
								textWord2.FontStyle = textElement.FontStyle;
								textWord2.TextColor = textElement.TextColor;
								textLine.WordCollection.Add(textWord2);
								foreach (TextGlyph item3 in list2)
								{
									textWord2.Glyphs.Add(item3);
								}
								list2.Clear();
								flag5 = true;
							}
							if (!flag5)
							{
								num15 += 1f;
							}
						}
						else
						{
							if (text2 != string.Empty)
							{
								num14 += (double)textWord2.Bounds.Width;
								textWord2.Bounds = new RectangleF((float)num12, (float)num13, (float)num14, (float)num3);
								textWord2.Text = text2;
								textWord2.FontName = textElement.FontName;
								textWord2.FontSize = textElement.FontSize;
								textWord2.FontStyle = textElement.FontStyle;
								textWord2.TextColor = textElement.TextColor;
								if (list != null && list.Count > 0)
								{
									textWord2.Glyphs.Clear();
								}
								foreach (TextGlyph item4 in list)
								{
									textWord2.Glyphs.Add(item4);
								}
								num15 = 0f;
								num12 = 0.0;
								num13 = 0.0;
								num2 = 0.0;
								num3 = 0.0;
								list.Clear();
								num14 = 0.0;
								text2 = "";
							}
							if (textWord2.Text != null && textWord2.Text != text3)
							{
								textWord2.Text += text3;
							}
							else
							{
								textWord2.Text = text3;
							}
							textWord2.FontName = textElement.FontName;
							textWord2.FontSize = textElement.FontSize;
							textWord2.FontStyle = textElement.FontStyle;
							textWord2.TextColor = textElement.TextColor;
							textLine.WordCollection.Add(textWord2);
							if (list2 != null && list2.Count > 0 && textWord2.Text == text3)
							{
								textWord2.Glyphs.Clear();
							}
							foreach (TextGlyph item5 in list2)
							{
								textWord2.Glyphs.Add(item5);
							}
							list2.Clear();
							num2 = 0.0;
							num3 = 0.0;
						}
						glyphIndex = num;
					}
				}
				textElement.Text = array[k].Text;
				if (textElement.Text.Length > 0)
				{
					if ((k < array.Length - 1 || (k <= array.Length - 1 && array[k].Text.Length > 1 && !m_hasRTL && j + 1 < imageRenderer.extractTextElement.Count && imageRenderer.extractTextElement[j + 1].renderedText == " ")) && !array[k].IsManuallySplit)
					{
						if (num != 0)
						{
							AddLine(textWord2, imageRenderer, textElement, textLine, num, num4, num5, num2, num3);
						}
						if (num != imageRenderer.imageRenderGlyphList.Count - 1)
						{
							num++;
						}
					}
					if (k < array.Length - 1 && !array[k].IsManuallySplit && num <= imageRenderer.imageRenderGlyphList.Count - 1 && imageRenderer.imageRenderGlyphList[num].ToUnicode == " " && num != imageRenderer.imageRenderGlyphList.Count - 1)
					{
						num++;
					}
				}
				else if (num <= imageRenderer.imageRenderGlyphList.Count - 1 && k != array.Length - 1 && imageRenderer.imageRenderGlyphList[num].ToUnicode == " ")
				{
					if (num != 0)
					{
						AddLine(textWord2, imageRenderer, textElement, textLine, num, num4, num5, num2, num3);
					}
					if (num != imageRenderer.imageRenderGlyphList.Count - 1)
					{
						num++;
					}
				}
			}
			if (((num != 0 && (int)num7 != num6 && imageRenderer.extractTextElement[j].renderedText != string.Empty && imageRenderer.extractTextElement[j].renderedText != "" && imageRenderer.extractTextElement[j].renderedText != " ") || num == imageRenderer.imageRenderGlyphList.Count - 1) && IsNotInSameLine(page.Rotation, num7, num8, num10, previousYPosition, previousBottomPosition, num9, j) && imageRenderer.extractTextElement.Count > 0 && j == 0)
			{
				num6 = (int)num7;
				if (textLine.WordCollection.Count > 0)
				{
					AddLineCollection(textLine, textLineCollection2, imageRenderer, lineStartIndex, num);
				}
				lineStartIndex = num;
				textLine = new TextLine();
			}
		}
		if (textLine != null && textLine.WordCollection != null && textLine.WordCollection.Count > 0 && !textLineCollection2.TextLine.Contains(textLine))
		{
			AddLineCollection(textLine, textLineCollection2, imageRenderer, lineStartIndex, glyphIndex);
			textLine = new TextLine();
		}
		textLineCollection = textLineCollection2;
		DocGen.PdfViewer.Base.Matrix matrix = new DocGen.PdfViewer.Base.Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);
		int num17 = 96;
		DocGen.PdfViewer.Base.Matrix initialTransform = new DocGen.PdfViewer.Base.Matrix(1.33333333333333 * (double)(num17 / 96) * matrix.M11, 0.0, 0.0, -1.33333333333333 * (double)(num17 / 96) * matrix.M22, 0.0, (double)page.Height * matrix.M22);
		m_transformations = new TransformationStack(initialTransform);
		Thread.CurrentThread.CurrentCulture = currentCulture;
		return ExtractTextWithLayout();
	}

	private SplitWord[] SplitRenderedText(string text, List<DocGen.PdfViewer.Base.Glyph> glyphs, int index, bool isRTL)
	{
		List<SplitWord> list = new List<SplitWord>();
		string text2 = string.Empty;
		Rect? rect = null;
		bool flag = false;
		if (!isRTL && GetRotationAngle(glyphs[index].TransformMatrix.M11, glyphs[index].TransformMatrix.M12, glyphs[index].TransformMatrix.M21, glyphs[index].TransformMatrix.M22) == 0.0 && !string.IsNullOrEmpty(text) && !string.IsNullOrWhiteSpace(text) && text[0].ToString() == glyphs[index].ToUnicode && Rotation == PdfPageRotateAngle.RotateAngle0)
		{
			int num = index;
			for (int i = 0; i < text.Length; i++)
			{
				try
				{
					DocGen.PdfViewer.Base.Glyph glyph = glyphs[num];
					if (glyph.ToUnicode == null && glyph.Width == 0.0)
					{
						text2 += text[i];
					}
					else if (text[i] == ' ')
					{
						if (text2 != "")
						{
							list.Add(new SplitWord(text2, isManuallySplit: false));
							if (i == text.Length - 1)
							{
								list.Add(new SplitWord(string.Empty, isManuallySplit: false));
							}
						}
						else
						{
							list.Add(new SplitWord(string.Empty, isManuallySplit: false));
						}
						rect = null;
						text2 = string.Empty;
					}
					else
					{
						Rect boundingRect = glyph.BoundingRect;
						if (rect.HasValue)
						{
							double num2 = boundingRect.Height * 0.07;
							if (num2 < 2.0)
							{
								num2 = 2.0;
							}
							double num3 = rect.Value.Left + rect.Value.Width - boundingRect.Left;
							if (num3 > 0.0 && num2 == 2.0)
							{
								num2 = 2.5;
							}
							if (Math.Abs(num3) > num2)
							{
								flag = true;
							}
							else
							{
								text2 += text[i];
							}
						}
						else
						{
							text2 += text[i];
						}
						if (flag)
						{
							list.Add(new SplitWord(text2, isManuallySplit: true));
							flag = false;
							rect = null;
							text2 = string.Empty;
							num--;
							i--;
						}
						else
						{
							rect = boundingRect;
						}
					}
				}
				catch (Exception)
				{
					string[] array = text.Split(' ');
					foreach (string word in array)
					{
						list.Add(new SplitWord(word, isManuallySplit: false));
					}
				}
				num++;
			}
			if (text2 != string.Empty)
			{
				list.Add(new SplitWord(text2, isManuallySplit: false));
			}
		}
		else
		{
			string[] array = text.Split(' ');
			foreach (string word2 in array)
			{
				list.Add(new SplitWord(word2, isManuallySplit: false));
			}
		}
		return list.ToArray();
	}

	private double GetRotationAngle(double m11, double m12, double m21, double m22)
	{
		return Math.Atan2(m21, m11) * (180.0 / Math.PI);
	}

	private bool IsNotInSameLine(double pageRotation, double currentYPosition, double currentBottomPosition, double currentXPosition, double previousYPosition, double previousBottomPosition, double previousXPosition, int extractTextElementIndex)
	{
		if (pageRotation == 0.0 || pageRotation == 270.0)
		{
			if ((currentYPosition >= previousYPosition && currentYPosition <= previousBottomPosition) || (currentBottomPosition >= previousYPosition && currentBottomPosition <= previousBottomPosition) || (previousYPosition >= currentYPosition && previousYPosition <= currentBottomPosition) || (previousBottomPosition >= currentYPosition && previousBottomPosition <= currentBottomPosition))
			{
				if (previousXPosition > currentXPosition)
				{
					return true;
				}
				return false;
			}
			return true;
		}
		return true;
	}

	private void AddLine(TextWord textwords, ImageRenderer renderer, TextElement textElements, TextLine textLine, int glyphIndex, double x, double y, double width, double height)
	{
		textwords = new TextWord();
		TextGlyph textGlyph = new TextGlyph();
		if (!string.IsNullOrEmpty(renderer.imageRenderGlyphList[glyphIndex].ToUnicode) && renderer.imageRenderGlyphList[glyphIndex].ToUnicode.Length == 1)
		{
			textGlyph.Text = Convert.ToChar(renderer.imageRenderGlyphList[glyphIndex].ToUnicode);
		}
		textGlyph.FontName = textElements.FontName;
		textGlyph.FontSize = textElements.FontSize;
		textGlyph.FontStyle = textElements.FontStyle;
		textGlyph.TextColor = textElements.TextColor;
		textGlyph.Bounds = new RectangleF((float)renderer.imageRenderGlyphList[glyphIndex].BoundingRect.X, (float)renderer.imageRenderGlyphList[glyphIndex].BoundingRect.Y, (float)renderer.imageRenderGlyphList[glyphIndex].BoundingRect.Width, (float)renderer.imageRenderGlyphList[glyphIndex].BoundingRect.Height);
		textwords.Glyphs.Add(textGlyph);
		x = renderer.imageRenderGlyphList[glyphIndex].BoundingRect.X;
		y = renderer.imageRenderGlyphList[glyphIndex].BoundingRect.Y;
		height = renderer.imageRenderGlyphList[glyphIndex].BoundingRect.Height;
		width = ((!(x > renderer.imageRenderGlyphList[glyphIndex].BoundingRect.X)) ? (renderer.imageRenderGlyphList[glyphIndex].BoundingRect.X - x + renderer.imageRenderGlyphList[glyphIndex].BoundingRect.Width) : (x - renderer.imageRenderGlyphList[glyphIndex].BoundingRect.X + renderer.imageRenderGlyphList[glyphIndex].BoundingRect.Width));
		textwords.Bounds = new RectangleF((float)x, (float)y, (float)width, (float)height);
		textwords.Text = " ";
		textwords.FontName = textElements.FontName;
		textwords.FontSize = textElements.FontSize;
		textwords.FontStyle = textElements.FontStyle;
		textwords.TextColor = textElements.TextColor;
		textLine.WordCollection.Add(textwords);
	}

	private void AddLineCollection(TextLine textLine, TextLineCollection lineCollection, ImageRenderer renderer, int lineStartIndex, int glyphIndex)
	{
		bool flag = true;
		bool flag2 = true;
		bool flag3 = true;
		string text = string.Empty;
		float num = 0f;
		FontStyle fontStyle = FontStyle.Regular;
		if (!m_hasRTL)
		{
			bool flag4 = false;
			if (textLine.WordCollection.Count > 1 && textLine.WordCollection[0].Bounds.Y != textLine.WordCollection[1].Bounds.Y)
			{
				flag4 = true;
			}
			if (!flag4 && Rotation == PdfPageRotateAngle.RotateAngle0)
			{
				PdfPath pdfPath = new PdfPath();
				for (int i = 0; i < textLine.WordCollection.Count; i++)
				{
					if (textLine.WordCollection[i].Bounds.Width > 0f && textLine.WordCollection[i].Bounds.Height > 0f)
					{
						pdfPath.AddRectangle(textLine.WordCollection[i].Bounds);
					}
				}
				textLine.Bounds = pdfPath.GetBounds();
			}
			else
			{
				Rect boundingRect = renderer.imageRenderGlyphList[lineStartIndex].BoundingRect;
				Rect boundingRect2 = renderer.imageRenderGlyphList[glyphIndex - 1].BoundingRect;
				double num2 = ((boundingRect.Y < boundingRect2.Y) ? boundingRect.Y : boundingRect2.Y);
				double num3 = ((boundingRect.Bottom > boundingRect2.Bottom) ? boundingRect.Bottom : boundingRect2.Bottom);
				if (Rotation == PdfPageRotateAngle.RotateAngle270)
				{
					num2 = ((boundingRect.X < boundingRect2.X) ? boundingRect.X : boundingRect2.X);
					num3 = ((boundingRect.Right > boundingRect2.Right) ? boundingRect.Right : boundingRect2.Right);
				}
				if ((Rotation == PdfPageRotateAngle.RotateAngle270 || Rotation == PdfPageRotateAngle.RotateAngle90) && textLine.WordCollection.Count == 1 && (double)textLine.WordCollection[0].Bounds.Y != boundingRect2.Y)
				{
					textLine.Bounds = textLine.WordCollection[0].Bounds;
				}
				else if (textLine.WordCollection[0].Bounds.Width > textLine.WordCollection[0].Bounds.Height || textLine.WordCollection[textLine.WordCollection.Count - 1].Bounds.Width > textLine.WordCollection[textLine.WordCollection.Count - 1].Bounds.Height)
				{
					textLine.Bounds = new RectangleF((float)boundingRect.X, (float)boundingRect2.Y, (float)(boundingRect2.X + boundingRect2.Width - boundingRect.X), (float)(num3 - num2));
				}
				else if (textLine.WordCollection[0].Bounds.Width < textLine.WordCollection[0].Bounds.Height)
				{
					textLine.Bounds = new RectangleF((float)boundingRect.X - (float)boundingRect.Height, (float)boundingRect2.Y, (float)(boundingRect2.X + boundingRect2.Width - boundingRect.X) + (float)boundingRect.Height, (float)(num3 - num2));
				}
			}
			RectangleF? rectangleF = null;
			if (textLine.WordCollection.Count > 0)
			{
				string empty = string.Empty;
				for (int j = 0; j < textLine.WordCollection.Count; j++)
				{
					if (j == 0)
					{
						text = textLine.WordCollection[j].FontName;
						num = textLine.WordCollection[j].FontSize;
						fontStyle = textLine.WordCollection[j].FontStyle;
						TextColor = textLine.WordCollection[j].TextColor;
					}
					empty = textLine.WordCollection[j].Text;
					if (empty.Contains("\u0001"))
					{
						empty = empty.Replace("\u0001", "");
					}
					else if (empty.Contains("\u0003"))
					{
						empty = empty.Replace("\u0003", "");
					}
					else if (empty.Contains("\u0002"))
					{
						empty = empty.Replace("\u0002", "");
					}
					double num4 = (double)textLine.WordCollection[j].Bounds.Height * 0.07;
					if (num4 < 1.5)
					{
						num4 = 1.5;
					}
					if (rectangleF.HasValue && !textLine.Text.EndsWith(" ") && !textLine.Text.EndsWith("\0") && !empty.StartsWith(" ") && !empty.StartsWith("\0") && (double)Math.Abs(rectangleF.Value.X + rectangleF.Value.Width - textLine.WordCollection[j].Bounds.X) > num4 && Rotation == PdfPageRotateAngle.RotateAngle0 && !Bidi.HasAnyRTL(textLine.Text))
					{
						textLine.Text += " ";
					}
					textLine.Text += empty;
					rectangleF = textLine.WordCollection[j].Bounds;
					bool flag5 = string.IsNullOrEmpty(empty) || string.IsNullOrWhiteSpace(empty);
					if (text == textLine.WordCollection[j].FontName && flag)
					{
						textLine.FontName = text;
					}
					else if (!flag5)
					{
						flag = false;
						textLine.FontName = ((!string.IsNullOrEmpty(textLine.WordCollection[j].FontName)) ? textLine.WordCollection[j].FontName : string.Empty);
					}
					if (num == textLine.WordCollection[j].FontSize && flag2)
					{
						textLine.FontSize = num;
					}
					else if (!flag5)
					{
						flag2 = false;
						textLine.FontSize = 0f;
					}
					if (fontStyle == textLine.WordCollection[j].FontStyle && flag3)
					{
						textLine.FontStyle = fontStyle;
					}
					else if (!flag5)
					{
						flag3 = false;
						textLine.FontStyle = FontStyle.Regular;
					}
					if (TextColor == textLine.WordCollection[j].TextColor)
					{
						textLine.TextColor = TextColor;
					}
				}
				if (!flag)
				{
					flag = true;
				}
				if (!flag2)
				{
					flag2 = true;
				}
				if (!flag3)
				{
					flag3 = true;
				}
			}
		}
		else
		{
			float num5 = 0f;
			float num6 = 0f;
			float num7 = 0f;
			float num8 = 0f;
			float num9 = 0f;
			float num10 = 0f;
			for (int k = lineStartIndex; k < glyphIndex; k++)
			{
				DocGen.PdfViewer.Base.Glyph glyph = renderer.imageRenderGlyphList[k];
				if (k == 0)
				{
					text = glyph.FontFamily;
					num = (float)glyph.FontSize;
					fontStyle = glyph.FontStyle;
					TextColor = glyph.TextColor;
				}
				textLine.Text += glyph.ToUnicode;
				if (text == glyph.FontFamily && flag)
				{
					textLine.FontName = text;
				}
				else
				{
					flag = false;
					textLine.FontName = string.Empty;
				}
				if ((double)num == glyph.FontSize && flag2)
				{
					textLine.FontSize = num;
				}
				else
				{
					flag2 = false;
					textLine.FontSize = 0f;
				}
				if (fontStyle == glyph.FontStyle && flag3)
				{
					textLine.FontStyle = fontStyle;
				}
				else
				{
					flag3 = false;
					textLine.FontStyle = FontStyle.Regular;
				}
				if (!flag)
				{
					flag = true;
				}
				if (!flag2)
				{
					flag2 = true;
				}
				if (!flag3)
				{
					flag3 = true;
				}
				if (!string.IsNullOrEmpty(glyph.ToUnicode) && !string.IsNullOrWhiteSpace(glyph.ToUnicode))
				{
					RectangleF rectangleF2 = new RectangleF((float)glyph.BoundingRect.X, (float)glyph.BoundingRect.Y, (float)glyph.BoundingRect.Width, (float)glyph.BoundingRect.Height);
					if (rectangleF2.X + rectangleF2.Width > num9)
					{
						num9 = rectangleF2.X + rectangleF2.Width;
					}
					if (rectangleF2.Y + rectangleF2.Height > num10)
					{
						num10 = rectangleF2.Y + rectangleF2.Height;
					}
					if (num7 == 0f || rectangleF2.X < num7)
					{
						num7 = rectangleF2.X;
					}
					if (num8 == 0f || rectangleF2.Y < num8)
					{
						num8 = rectangleF2.Y;
					}
					if (num5 == 0f || num9 - num7 > num5)
					{
						num5 = num9 - num7;
					}
					if (num6 == 0f || num10 - num8 > num6)
					{
						num6 = num10 - num8;
					}
				}
			}
			textLine.Bounds = new RectangleF(num7, num8, num5, num6);
		}
		if (Rotation == PdfPageRotateAngle.RotateAngle90)
		{
			textLine.Bounds = new RectangleF(Size.Height - (textLine.Bounds.Y + textLine.Bounds.Width), textLine.Bounds.X - textLine.Bounds.Width, textLine.Bounds.Height, textLine.Bounds.Width);
		}
		if (m_visualOrder && !string.IsNullOrEmpty(textLine.Text) && Bidi.HasAnyRTL(textLine.Text))
		{
			if (m_bidiInstance == null)
			{
				m_bidiInstance = new Bidi();
			}
			textLine.Text = m_bidiInstance.GetLogicalToVisualString(textLine.Text, isRTL: true);
		}
		if (!m_hasRTL || (m_hasRTL && textLine.Text != " "))
		{
			lineCollection.TextLine.Add(textLine);
		}
	}

	private string ExtractTextWithLayout()
	{
		m_isLayoutTextExtraction = true;
		resultantText = null;
		SizeF size = Size;
		if (Rotation == PdfPageRotateAngle.RotateAngle90 || Rotation == PdfPageRotateAngle.RotateAngle270)
		{
			if (this is PdfLoadedPage)
			{
				_ = (this as PdfLoadedPage).CropBox;
				if ((this as PdfLoadedPage).CropBox.Width > 0f && (this as PdfLoadedPage).CropBox.Height > 0f && (this as PdfLoadedPage).CropBox != (this as PdfLoadedPage).MediaBox)
				{
					UnitConvertor.ConvertToPixels((this as PdfLoadedPage).CropBox.Height - (this as PdfLoadedPage).CropBox.Y, PdfGraphicsUnit.Point);
					UnitConvertor.ConvertToPixels((this as PdfLoadedPage).CropBox.Width - (this as PdfLoadedPage).CropBox.X, PdfGraphicsUnit.Point);
					goto IL_0232;
				}
			}
			UnitConvertor.ConvertToPixels(size.Height, PdfGraphicsUnit.Point);
			UnitConvertor.ConvertToPixels(size.Width, PdfGraphicsUnit.Point);
		}
		else
		{
			if (this is PdfLoadedPage)
			{
				_ = (this as PdfLoadedPage).CropBox;
				if ((this as PdfLoadedPage).CropBox.Width > 0f && (this as PdfLoadedPage).CropBox.Height > 0f && (this as PdfLoadedPage).CropBox != (this as PdfLoadedPage).MediaBox)
				{
					UnitConvertor.ConvertToPixels((this as PdfLoadedPage).CropBox.Width - (this as PdfLoadedPage).CropBox.X, PdfGraphicsUnit.Point);
					UnitConvertor.ConvertToPixels((this as PdfLoadedPage).CropBox.Height - (this as PdfLoadedPage).CropBox.Y, PdfGraphicsUnit.Point);
					goto IL_0232;
				}
			}
			UnitConvertor.ConvertToPixels(size.Width, PdfGraphicsUnit.Point);
			UnitConvertor.ConvertToPixels(size.Height, PdfGraphicsUnit.Point);
		}
		goto IL_0232;
		IL_0232:
		using (MemoryStream memoryStream = new MemoryStream())
		{
			Layers.CombineContent(memoryStream);
			memoryStream.Position = 0L;
			ContentParser contentParser = new ContentParser(memoryStream.ToArray());
			contentParser.IsTextExtractionProcess = true;
			m_recordCollection = contentParser.ReadContent();
			contentParser.IsTextExtractionProcess = false;
		}
		m_pageResources = PageResourceLoader.Instance.GetPageResources(this);
		RenderTextAsLayout(m_recordCollection, m_pageResources);
		m_recordCollection.RecordCollection.Clear();
		m_pageResources.Resources.Clear();
		m_pageResources = null;
		resultantText += "\r\n\r\n";
		if (resultantText != null)
		{
			resultantText = SkipEscapeSequence(resultantText);
		}
		return resultantText;
	}

	public string ExtractText()
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			Layers.CombineContent(memoryStream);
			memoryStream.Position = 0L;
			ContentParser contentParser = new ContentParser(memoryStream.ToArray());
			contentParser.IsTextExtractionProcess = true;
			m_recordCollection = contentParser.ReadContent();
			contentParser.IsTextExtractionProcess = false;
		}
		m_pageResources = PageResourceLoader.Instance.GetPageResources(this);
		resultantText = null;
		RenderText(m_recordCollection, m_pageResources);
		m_recordCollection.RecordCollection.Clear();
		m_recordCollection = null;
		m_pageResources.Resources.Clear();
		if (m_pageResources.fontCollection != null)
		{
			m_pageResources.fontCollection.Clear();
		}
		m_pageResources = null;
		if (resultantText != null)
		{
			resultantText = SkipEscapeSequence(resultantText);
		}
		return resultantText;
	}

	public string ExtractText(bool IsLayout)
	{
		m_isLayoutTextExtraction = IsLayout;
		resultantText = null;
		if (!IsLayout)
		{
			resultantText = ExtractText();
			goto IL_03ed;
		}
		SizeF size = Size;
		float num;
		if (Rotation == PdfPageRotateAngle.RotateAngle90 || Rotation == PdfPageRotateAngle.RotateAngle270)
		{
			if (this is PdfLoadedPage)
			{
				_ = (this as PdfLoadedPage).CropBox;
				if ((this as PdfLoadedPage).CropBox.Width > 0f && (this as PdfLoadedPage).CropBox.Height > 0f && (this as PdfLoadedPage).CropBox != (this as PdfLoadedPage).MediaBox)
				{
					UnitConvertor.ConvertToPixels((this as PdfLoadedPage).CropBox.Height - (this as PdfLoadedPage).CropBox.Y, PdfGraphicsUnit.Point);
					num = UnitConvertor.ConvertToPixels((this as PdfLoadedPage).CropBox.Width - (this as PdfLoadedPage).CropBox.X, PdfGraphicsUnit.Point);
					goto IL_0252;
				}
			}
			UnitConvertor.ConvertToPixels(size.Height, PdfGraphicsUnit.Point);
			num = UnitConvertor.ConvertToPixels(size.Width, PdfGraphicsUnit.Point);
		}
		else
		{
			if (this is PdfLoadedPage)
			{
				_ = (this as PdfLoadedPage).CropBox;
				if ((this as PdfLoadedPage).CropBox.Width > 0f && (this as PdfLoadedPage).CropBox.Height > 0f && (this as PdfLoadedPage).CropBox != (this as PdfLoadedPage).MediaBox)
				{
					UnitConvertor.ConvertToPixels((this as PdfLoadedPage).CropBox.Width - (this as PdfLoadedPage).CropBox.X, PdfGraphicsUnit.Point);
					num = UnitConvertor.ConvertToPixels((this as PdfLoadedPage).CropBox.Height - (this as PdfLoadedPage).CropBox.Y, PdfGraphicsUnit.Point);
					goto IL_0252;
				}
			}
			UnitConvertor.ConvertToPixels(size.Width, PdfGraphicsUnit.Point);
			num = UnitConvertor.ConvertToPixels(size.Height, PdfGraphicsUnit.Point);
		}
		goto IL_0252;
		IL_03ed:
		return resultantText;
		IL_0252:
		using (MemoryStream memoryStream = new MemoryStream())
		{
			Layers.CombineContent(memoryStream);
			memoryStream.Position = 0L;
			ContentParser contentParser = new ContentParser(memoryStream.ToArray());
			contentParser.IsTextExtractionProcess = true;
			m_recordCollection = contentParser.ReadContent();
			contentParser.IsTextExtractionProcess = false;
		}
		DocGen.PdfViewer.Base.Matrix matrix = new DocGen.PdfViewer.Base.Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);
		int num2 = 96;
		DocGen.PdfViewer.Base.Matrix initialTransform = new DocGen.PdfViewer.Base.Matrix(1.33333333333333 * (double)(num2 / 96) * matrix.M11, 0.0, 0.0, -1.33333333333333 * (double)(num2 / 96) * matrix.M22, 0.0, (double)num * matrix.M22);
		m_transformations = new TransformationStack(initialTransform);
		m_pageResources = PageResourceLoader.Instance.GetPageResources(this);
		RenderTextAsLayout(m_recordCollection, m_pageResources);
		m_recordCollection.RecordCollection.Clear();
		m_recordCollection = null;
		m_pageResources.Resources.Clear();
		if (m_pageResources.fontCollection != null)
		{
			m_pageResources.fontCollection.Clear();
		}
		m_pageResources = null;
		resultantText += "\r\n\r\n";
		if (resultantText != null)
		{
			resultantText = SkipEscapeSequence(resultantText);
		}
		goto IL_03ed;
	}

	private void RenderText(PdfRecordCollection recordCollection, PdfPageResources m_pageResources)
	{
		if (recordCollection == null)
		{
			return;
		}
		foreach (PdfRecord item in recordCollection)
		{
			string text = item.OperatorName;
			string[] operands = item.Operands;
			char[] symbolChars = m_symbolChars;
			int i;
			for (i = 0; i < symbolChars.Length; i++)
			{
				char c = symbolChars[i];
				if (text.Contains(c.ToString()))
				{
					text = text.Replace(c.ToString(), "");
				}
			}
			string text2 = text.Trim();
			if (text2 == null)
			{
				continue;
			}
			i = text2.Length;
			if (i != 1)
			{
				if (i != 2)
				{
					continue;
				}
				char c2 = text2[1];
				if ((uint)c2 <= 99u)
				{
					if ((uint)c2 > 74u)
					{
						switch (c2)
						{
						case 'c':
							if (text2 == "Tc")
							{
								GetCharacterSpacing(operands);
							}
							break;
						case 'T':
							if (text2 == "ET")
							{
								resultantText += "\r\n";
							}
							break;
						}
						continue;
					}
					if (c2 == '*')
					{
						if (text2 == "T*")
						{
							resultantText += "\r\n";
						}
						continue;
					}
					if (c2 != 'J' || !(text2 == "TJ"))
					{
						continue;
					}
				}
				else
				{
					if ((uint)c2 > 106u)
					{
						switch (c2)
						{
						case 'w':
							if (text2 == "Tw")
							{
								GetWordSpacing(operands);
							}
							break;
						case 'o':
							if (text2 == "Do")
							{
								GetXObject(operands, m_pageResources);
							}
							break;
						}
						continue;
					}
					if (c2 == 'f')
					{
						if (text2 == "Tf")
						{
							RenderFont(operands);
						}
						continue;
					}
					if (c2 != 'j' || !(text2 == "Tj"))
					{
						continue;
					}
				}
			}
			else if (!(text2 == "'"))
			{
				continue;
			}
			resultantText += RenderTextElement(operands, text, m_pageResources);
			if (text == "'")
			{
				resultantText += "\r\n";
			}
		}
	}

	private void GetCharacterSpacing(string[] element)
	{
		m_characterSpacing = float.Parse(element[0], CultureInfo.InvariantCulture);
	}

	private void GetWordSpacing(string[] element)
	{
		m_wordSpacing = float.Parse(element[0], CultureInfo.InvariantCulture);
		if (m_wordSpacing > 1f)
		{
			m_wordSpacing = 1f;
		}
	}

	private string RenderTextElementTJ(string[] textElements, string token, PdfPageResources m_pageResources, DocGen.PdfViewer.Base.Matrix m_textLineMatrix)
	{
		try
		{
			List<string> list = new List<string>();
			string text = string.Join("", textElements);
			string text2 = text;
			if (m_pageResources.ContainsKey(m_currentFont))
			{
				m_structure = m_pageResources[m_currentFont] as FontStructure;
				m_structure.IsTextExtraction = true;
				if (m_structure != null)
				{
					m_structure.FontSize = m_fontSize;
				}
				list = m_structure.DecodeTextExtractionTJ(text, isSameFont: true);
				m_structure.IsTextExtraction = false;
				text2 = RenderTextFromTJ(list, m_textLineMatrix);
				if (m_structure.IsAdobeJapanFont && m_structure.AdobeJapanCidMapTable != null && m_structure.AdobeJapanCidMapTable.Count > 0)
				{
					string text3 = string.Empty;
					string text4 = text2;
					for (int i = 0; i < text4.Length; i++)
					{
						string mapChar = text4[i].ToString();
						text3 += m_structure.AdobeJapanCidMapTableGlyphParser(mapChar);
					}
					text2 = text3;
				}
				if (m_visualOrder && !string.IsNullOrEmpty(text2) && Bidi.HasAnyRTL(text2))
				{
					if (m_bidiInstance == null)
					{
						m_bidiInstance = new Bidi();
					}
					text2 = m_bidiInstance.GetLogicalToVisualString(text2, isRTL: true);
				}
			}
			return text2;
		}
		catch
		{
			return null;
		}
	}

	private string RenderTextFromLeading(string decodedText, DocGen.PdfViewer.Base.Matrix m_textLineMatrix)
	{
		string text = string.Empty;
		for (int i = 0; i < decodedText.Length; i++)
		{
			char c = decodedText[i];
			string text2 = c.ToString();
			if (m_strBackup == " " && m_strBackup.Length == 1 && m_hasNoSpacing)
			{
				resultantText = resultantText.Remove(resultantText.Length - 1);
				m_hasNoSpacing = false;
			}
			DocGen.PdfViewer.Base.Matrix matrix = new DocGen.PdfViewer.Base.Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);
			m_charID = c;
			m_characterWidth = GetCharacterWidth(c);
			m_textMatrix = GetTextRenderingMatrix();
			DocGen.PdfViewer.Base.Matrix identity = DocGen.PdfViewer.Base.Matrix.Identity;
			identity.Scale(0.01, 0.01, 0.0, 0.0);
			identity.Translate(0.0, 1.0);
			m_transformations.PushTransform(identity * m_textMatrix);
			DocGen.PdfViewer.Base.Matrix matrix2 = matrix;
			DocGen.PdfViewer.Base.Matrix matrix3 = matrix2.Clone();
			matrix3 *= GetTransformationMatrix(m_transformations.CurrentTransform);
			float num = 0f;
			num = ((m_textMatrix.M11 > 0.0) ? ((float)m_textMatrix.M11) : ((m_textMatrix.M12 == 0.0 || m_textMatrix.M21 == 0.0) ? m_structure.FontSize : ((!(m_textMatrix.M12 < 0.0)) ? ((float)m_textMatrix.M12) : ((float)(0.0 - m_textMatrix.M12)))));
			if (Rotation == PdfPageRotateAngle.RotateAngle90 || Rotation == PdfPageRotateAngle.RotateAngle270)
			{
				if (matrix3.M12 == 0.0 && matrix3.M21 == 0.0)
				{
					m_isRotated = false;
					m_boundingRect = new RectangleF(new DocGen.PdfViewer.Base.Point(UnitConvertor.ConvertFromPixels((float)matrix3.OffsetX, PdfGraphicsUnit.Point) / 1f, (UnitConvertor.ConvertFromPixels((float)matrix3.OffsetY, PdfGraphicsUnit.Point) - UnitConvertor.ConvertFromPixels(num * 1f, PdfGraphicsUnit.Point)) / 1f), new DocGen.Drawing.Size((int)(m_characterWidth * (double)num), (int)num));
				}
				else
				{
					m_isRotated = true;
					m_boundingRect = new RectangleF(new DocGen.PdfViewer.Base.Point(UnitConvertor.ConvertFromPixels((float)matrix3.OffsetX, PdfGraphicsUnit.Point) / 1f, (UnitConvertor.ConvertFromPixels((float)matrix3.OffsetY, PdfGraphicsUnit.Point) - UnitConvertor.ConvertFromPixels(num * 1f, PdfGraphicsUnit.Point)) / 1f), new DocGen.Drawing.Size((int)num, (int)(m_characterWidth * (double)num)));
				}
			}
			else
			{
				m_boundingRect = new RectangleF(new DocGen.PdfViewer.Base.Point(UnitConvertor.ConvertFromPixels((float)matrix3.OffsetX, PdfGraphicsUnit.Point) / 1f, (UnitConvertor.ConvertFromPixels((float)matrix3.OffsetY, PdfGraphicsUnit.Point) - UnitConvertor.ConvertFromPixels(num * 1f, PdfGraphicsUnit.Point)) / 1f), new DocGen.Drawing.Size((int)(m_characterWidth * (double)num), (int)num));
			}
			_ = m_tempBoundingRectangle;
			double num2 = Math.Round((m_boundingRect.Left - m_tempBoundingRectangle.Right) / 10f, 1);
			if (m_tempBoundingRectangle.Right != 0f && m_boundingRect.Left != 0f && num2 >= 1.0 && m_hasLeading)
			{
				text += Convert.ToChar(32);
			}
			m_transformations.PopTransform();
			text += text2;
			UpdateTextMatrix();
			m_tempBoundingRectangle = m_boundingRect;
		}
		m_strBackup = text;
		return text;
	}

	private string RenderTextFromTJ(List<string> decodedList, DocGen.PdfViewer.Base.Matrix m_textLineMatrix)
	{
		string text = string.Empty;
		foreach (string decoded in decodedList)
		{
			if (double.TryParse(decoded, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
			{
				UpdateTextMatrix(result);
				if (((int)(this.m_textLineMatrix.OffsetX - m_textMatrix.OffsetX) > 1 && !m_hasBDC) || hasSpace)
				{
					text += Convert.ToChar(32);
				}
				continue;
			}
			string text2 = decoded.Remove(decoded.Length - 1, 1);
			for (int i = 0; i < text2.Length; i++)
			{
				char c = text2[i];
				DocGen.PdfViewer.Base.Matrix matrix = new DocGen.PdfViewer.Base.Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);
				m_charID = c;
				m_characterWidth = GetCharacterWidth(c);
				m_textMatrix = GetTextRenderingMatrix();
				DocGen.PdfViewer.Base.Matrix identity = DocGen.PdfViewer.Base.Matrix.Identity;
				identity.Scale(0.01, 0.01, 0.0, 0.0);
				identity.Translate(0.0, 1.0);
				m_transformations.PushTransform(identity * m_textMatrix);
				DocGen.PdfViewer.Base.Matrix matrix2 = matrix;
				DocGen.PdfViewer.Base.Matrix matrix3 = matrix2.Clone();
				matrix3 *= GetTransformationMatrix(m_transformations.CurrentTransform);
				float num = 0f;
				num = ((m_textMatrix.M11 > 0.0) ? ((float)m_textMatrix.M11) : ((m_textMatrix.M12 == 0.0 || m_textMatrix.M21 == 0.0) ? m_structure.FontSize : ((!(m_textMatrix.M12 < 0.0)) ? ((float)m_textMatrix.M12) : ((float)(0.0 - m_textMatrix.M12)))));
				if (Rotation == PdfPageRotateAngle.RotateAngle90 || Rotation == PdfPageRotateAngle.RotateAngle270)
				{
					if (matrix3.M12 == 0.0 && matrix3.M21 == 0.0)
					{
						m_isRotated = false;
						m_boundingRect = new RectangleF(new DocGen.PdfViewer.Base.Point(UnitConvertor.ConvertFromPixels((float)matrix3.OffsetX, PdfGraphicsUnit.Point) / 1f, (UnitConvertor.ConvertFromPixels((float)matrix3.OffsetY, PdfGraphicsUnit.Point) - UnitConvertor.ConvertFromPixels(num * 1f, PdfGraphicsUnit.Point)) / 1f), new DocGen.Drawing.Size((int)(m_characterWidth * (double)num), (int)num));
					}
					else
					{
						m_isRotated = true;
						m_boundingRect = new RectangleF(new DocGen.PdfViewer.Base.Point(UnitConvertor.ConvertFromPixels((float)matrix3.OffsetX, PdfGraphicsUnit.Point) / 1f, (UnitConvertor.ConvertFromPixels((float)matrix3.OffsetY, PdfGraphicsUnit.Point) - UnitConvertor.ConvertFromPixels(num * 1f, PdfGraphicsUnit.Point)) / 1f), new DocGen.Drawing.Size((int)num, (int)(m_characterWidth * (double)num)));
					}
				}
				else
				{
					m_boundingRect = new RectangleF(new DocGen.PdfViewer.Base.Point(UnitConvertor.ConvertFromPixels((float)matrix3.OffsetX, PdfGraphicsUnit.Point) / 1f, (UnitConvertor.ConvertFromPixels((float)matrix3.OffsetY, PdfGraphicsUnit.Point) - UnitConvertor.ConvertFromPixels(num * 1f, PdfGraphicsUnit.Point)) / 1f), new DocGen.Drawing.Size((int)(m_characterWidth * (double)num), (int)num));
				}
				_ = m_tempBoundingRectangle;
				double num2 = Math.Round((m_boundingRect.Left - m_tempBoundingRectangle.Right) / 10f);
				if (m_tempBoundingRectangle.Right != 0f && m_boundingRect.Left != 0f && num2 > 1.0)
				{
					text += Convert.ToChar(32);
				}
				m_transformations.PopTransform();
				text += c;
				UpdateTextMatrix();
				m_tempBoundingRectangle = m_boundingRect;
				m_textMatrix = this.m_textLineMatrix;
			}
		}
		if (m_structure.FontEncoding == "MacRomanEncoding")
		{
			string text3 = string.Empty;
			string text2 = text;
			for (int i = 0; i < text2.Length; i++)
			{
				char c2 = text2[i];
				if ((byte)c2 > 126)
				{
					string text4 = m_structure.MacEncodeTable[(byte)c2];
					text3 += text4;
				}
				else
				{
					text3 += c2;
				}
			}
			if (text3 != string.Empty)
			{
				text = text3;
			}
		}
		if (text.Contains("\u0092"))
		{
			text = text.Replace("\u0092", "");
		}
		return text;
	}

	private DocGen.PdfViewer.Base.Matrix GetTransformationMatrix(DocGen.PdfViewer.Base.Matrix transform)
	{
		return new DocGen.PdfViewer.Base.Matrix((float)transform.M11, (float)transform.M12, (float)transform.M21, (float)transform.M22, (float)transform.OffsetX, (float)transform.OffsetY);
	}

	private void UpdateTextMatrix()
	{
		m_textLineMatrix = CalculateTextMatrix(m_textLineMatrix);
	}

	private DocGen.PdfViewer.Base.Matrix CalculateTextMatrix(DocGen.PdfViewer.Base.Matrix m)
	{
		if (m_charID == 32)
		{
			m_wordSpacing = m_wordSpacing;
		}
		double offsetX = (m_characterWidth * (double)m_fontSize + (double)m_characterSpacing + (double)m_wordSpacing) * (double)(m_horizontalScaling / 100);
		return new DocGen.PdfViewer.Base.Matrix(1.0, 0.0, 0.0, 1.0, offsetX, 0.0) * m;
	}

	private double GetCharacterWidth(char character)
	{
		if (m_structure.FontGlyphWidths != null && m_structure.fontType != null && m_structure.fontType.Value == "TrueType" && m_structure.FontGlyphWidths.ContainsKey(character))
		{
			double advancedWidth = (float)m_structure.FontGlyphWidths[character] * m_charSizeMultiplier;
			m_advancedWidth = advancedWidth;
		}
		else
		{
			m_advancedWidth = 1.0;
		}
		return m_advancedWidth;
	}

	private void UpdateTextMatrix(double tj)
	{
		double num = 0.0 - tj * 0.001 * (double)m_fontSize * (double)TextHorizontalScaling / 100.0;
		DocGen.PdfViewer.Base.Point point = m_textLineMatrix.Transform(new DocGen.PdfViewer.Base.Point(0.0, 0.0));
		DocGen.PdfViewer.Base.Point point2 = m_textLineMatrix.Transform(new DocGen.PdfViewer.Base.Point(num, 0.0));
		if (point.X != point2.X)
		{
			m_textLineMatrix.OffsetX = point2.X;
			return;
		}
		if (num > 0.0 && !m_hasRTL)
		{
			hasSpace = true;
		}
		else
		{
			hasSpace = false;
		}
		m_textLineMatrix.OffsetY = point2.Y;
	}

	private DocGen.PdfViewer.Base.Matrix GetTextRenderingMatrix()
	{
		DocGen.PdfViewer.Base.Matrix matrix = default(DocGen.PdfViewer.Base.Matrix);
		matrix.M11 = m_fontSize * (TextHorizontalScaling / 100f);
		matrix.M22 = 0f - m_fontSize;
		matrix.OffsetY = m_fontSize + (float)m_rise;
		return matrix * m_textLineMatrix * Ctm;
	}

	private void RenderTextAsLayout(PdfRecordCollection recordCollection, PdfPageResources m_pageResources)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float[] array = new float[0];
		int num4 = 0;
		float num5 = 0f;
		string empty = string.Empty;
		string text = string.Empty;
		string[] array2 = null;
		if (recordCollection == null)
		{
			return;
		}
		foreach (PdfRecord item in recordCollection)
		{
			string text2 = item.OperatorName;
			string[] operands = item.Operands;
			char[] symbolChars = m_symbolChars;
			for (int i = 0; i < symbolChars.Length; i++)
			{
				char c = symbolChars[i];
				if (text2.Contains(c.ToString()))
				{
					text2 = text2.Replace(c.ToString(), "");
				}
			}
			switch (text2.Trim())
			{
			case "q":
				m_hasET = false;
				break;
			case "Tc":
				GetCharacterSpacing(operands);
				break;
			case "Tw":
				GetWordSpacing(operands);
				break;
			case "Tm":
			{
				m_hasTm = true;
				float num10 = float.Parse(operands[0], CultureInfo.InvariantCulture);
				float num11 = float.Parse(operands[1], CultureInfo.InvariantCulture);
				float num12 = float.Parse(operands[2], CultureInfo.InvariantCulture);
				float num13 = float.Parse(operands[3], CultureInfo.InvariantCulture);
				float num14 = float.Parse(operands[4], CultureInfo.InvariantCulture);
				float num15 = float.Parse(operands[5], CultureInfo.InvariantCulture);
				m_textMatrix = (m_textLineMatrix = new DocGen.PdfViewer.Base.Matrix(num10, num11, num12, num13, num14, num15));
				CurrentLocation = new DocGen.PdfViewer.Base.Point(0.0, 0.0);
				m_isTextMatrix = true;
				if (m_textMatrix.OffsetY == m_textLineMatrix.OffsetY && m_textMatrix.OffsetX != m_textLineMatrix.OffsetX)
				{
					m_textLineMatrix = m_textMatrix;
				}
				if (m_textLineMatrix.OffsetY != m_currentTextMatrix.OffsetY || (m_textLineMatrix.OffsetX != m_currentTextMatrix.OffsetX && m_hasBDC && !m_hasTj))
				{
					m_tempBoundingRectangle = default(RectangleF);
					m_hasBDC = false;
				}
				break;
			}
			case "Tf":
				RenderFont(operands);
				break;
			case "TL":
				SetTextLeading(float.Parse(operands[0], CultureInfo.InvariantCulture));
				break;
			case "T*":
				MoveToNextLine(0f, m_textLeading);
				break;
			case "BT":
				m_textLineMatrix = (m_textMatrix = DocGen.PdfViewer.Base.Matrix.Identity);
				break;
			case "ET":
			{
				m_hasET = true;
				float num20 = (float)(m_textLineMatrix.OffsetX - (double)m_tempBoundingRectangle.Right) / 10f;
				if (m_hasLeading && num20 == 0f && m_hasNoSpacing)
				{
					resultantText += " ";
					m_tempBoundingRectangle = default(RectangleF);
					m_hasLeading = false;
				}
				CurrentLocation = PointF.Empty;
				if (m_isTextMatrix)
				{
					m_isTextMatrix = false;
				}
				break;
			}
			case "cm":
			{
				m_hasET = false;
				float num16 = float.Parse(operands[5], CultureInfo.InvariantCulture);
				int num17 = (int)num16;
				int num18 = (int)num;
				int num19 = (num17 - num18) / 10;
				if (num17 != num18 && m_hasTm && (num19 < 0 || num19 >= 1))
				{
					resultantText += "\r\n";
					num4++;
					m_hasTm = false;
				}
				num = num16;
				break;
			}
			case "BDC":
				m_hasBDC = true;
				m_hasET = true;
				array2 = operands;
				break;
			case "TD":
				SetTextLeading(0f - float.Parse(operands[1], CultureInfo.InvariantCulture));
				MoveToNextLine(float.Parse(operands[0], CultureInfo.InvariantCulture), float.Parse(operands[1], CultureInfo.InvariantCulture));
				if (m_textLineMatrix.OffsetY != m_currentTextMatrix.OffsetY || (m_hasBDC && m_textLineMatrix.OffsetX != m_currentTextMatrix.OffsetX && !m_hasTj))
				{
					m_tempBoundingRectangle = default(RectangleF);
					m_hasBDC = false;
				}
				break;
			case "Td":
				MoveToNextLine(float.Parse(operands[0], CultureInfo.InvariantCulture), float.Parse(operands[1], CultureInfo.InvariantCulture));
				if (m_textLineMatrix.OffsetY != m_currentTextMatrix.OffsetY || (m_hasBDC && m_textLineMatrix.OffsetX != m_currentTextMatrix.OffsetX))
				{
					m_tempBoundingRectangle = default(RectangleF);
					m_hasBDC = false;
				}
				if (Math.Abs(m_textLineMatrix.OffsetX - m_currentTextMatrix.OffsetX) > 0.0 && !spaceBetweenWord && m_hasTj)
				{
					num5 = (float)Math.Abs(m_textLineMatrix.OffsetX - m_currentTextMatrix.OffsetX);
					spaceBetweenWord = true;
				}
				array = new float[2]
				{
					float.Parse(operands[0], CultureInfo.InvariantCulture),
					float.Parse(operands[1], CultureInfo.InvariantCulture)
				};
				break;
			case "Tz":
				GetScalingFactor(operands);
				break;
			case "'":
			{
				MoveToNextLine(0f, m_textLeading);
				num2 = (float)m_textMatrix.OffsetY;
				m_hasNoSpacing = false;
				float num7 = 0f;
				num7 = ((!(m_fontSize >= 10f)) ? ((float)Math.Round((num2 - num3) / m_fontSize)) : ((float)Math.Round((num2 - num3) / 10f)));
				if (num7 < 0f)
				{
					num7 = 0f - num7;
				}
				m_hasLeading = true;
				if (num3 != 0f && num7 >= 1f)
				{
					resultantText += "\r\n";
				}
				long num8 = Convert.ToInt64(m_textLineMatrix.OffsetX);
				if (Convert.ToInt64(m_currentTextMatrix.OffsetX) - num8 > 0)
				{
					m_hasNoSpacing = true;
				}
				empty = RenderTextElement(operands, text2, m_pageResources);
				m_currentTextMatrix = m_textLineMatrix;
				num3 = num2;
				resultantText += empty;
				m_textMatrix = m_textLineMatrix;
				break;
			}
			case "TJ":
			{
				num2 = (float)m_textMatrix.OffsetY;
				float num9 = 0f;
				num9 = ((!(m_fontSize >= 10f)) ? ((float)Math.Round((num2 - num3) / m_fontSize)) : ((float)Math.Round((num2 - num3) / 10f)));
				if (num9 < 0f)
				{
					num9 = 0f - num9;
				}
				if (spaceBetweenWord)
				{
					if (num5 > m_fontSize)
					{
						_ = num5 / m_fontSize;
						num5 = 0f;
					}
					spaceBetweenWord = false;
				}
				m_hasTj = true;
				if (num3 != 0f && num9 >= 1f)
				{
					resultantText += "\r\n";
				}
				empty = RenderTextElementTJ(operands, text2, m_pageResources, m_textLineMatrix);
				m_currentTextMatrix = m_textLineMatrix;
				num3 = num2;
				resultantText += empty;
				if (m_textLineMatrix.M11 != 1.0 && m_textLineMatrix.M22 != 1.0)
				{
					resultantText += " ";
				}
				m_textMatrix = m_textLineMatrix;
				m_hasET = false;
				m_hasBDC = true;
				break;
			}
			case "Tj":
			{
				num2 = (float)m_textMatrix.OffsetY;
				float num6 = 0f;
				string empty2 = string.Empty;
				string empty3 = string.Empty;
				num6 = ((!(m_fontSize >= 10f)) ? ((float)Math.Round((num2 - num3) / m_fontSize)) : ((float)Math.Round((num2 - num3) / 10f)));
				if (num6 < 0f)
				{
					num6 = 0f - num6;
				}
				if (spaceBetweenWord)
				{
					if (num5 > m_fontSize)
					{
						_ = num5 / m_fontSize;
						num5 = 0f;
					}
					if (array2 != null && array2.Length > 1)
					{
						array2[1] = array2[1].TrimStart('<');
						empty2 = array2[1].TrimEnd('>');
						empty3 = operands[0].Trim('(', ')');
						if (empty2 != string.Empty && empty2.Contains("<") && empty3.Length == 1 && char.IsLetter(char.Parse(empty3)))
						{
							m_hasET = false;
						}
					}
					if (m_hasET)
					{
						resultantText += " ";
					}
					m_hasET = false;
					spaceBetweenWord = false;
				}
				m_hasTj = true;
				if (num3 != 0f && num6 >= 1f)
				{
					resultantText += "\r\n";
				}
				empty = RenderTextElement(operands, text2, m_pageResources);
				if (!(empty == text) || array == null || array.Length == 0 || !((double)array[0] < 0.5))
				{
					m_currentTextMatrix = m_textLineMatrix;
					num3 = num2;
					text = empty;
					if (m_previousTextMatrix.OffsetY != 0.0 && m_currentTextMatrix.OffsetY != 0.0 && m_previousTextMatrix.OffsetY + (double)m_previousFontSize > m_currentTextMatrix.OffsetY + (double)m_fontSize && m_previousTextMatrix.OffsetY < m_currentTextMatrix.OffsetY && resultantText.Length >= 2 && resultantText.Substring(resultantText.Length - 2) == "\r\n")
					{
						resultantText = resultantText.Remove(resultantText.Length - 2, 2);
					}
					m_previousFontSize = m_fontSize;
					resultantText += empty;
					m_textMatrix = m_textLineMatrix;
					m_previousTextMatrix = m_textLineMatrix;
				}
				break;
			}
			case "Do":
				m_isLayoutTextExtraction = true;
				GetXObject(operands, m_pageResources);
				m_isLayoutTextExtraction = false;
				break;
			}
		}
	}

	private void MoveToNextLine(float tx, float ty)
	{
		DocGen.PdfViewer.Base.Matrix matrix = default(DocGen.PdfViewer.Base.Matrix);
		matrix.M11 = 1.0;
		matrix.M12 = 0.0;
		matrix.OffsetX = tx;
		matrix.M21 = 0.0;
		matrix.M22 = 1.0;
		matrix.OffsetY = ty;
		DocGen.PdfViewer.Base.Matrix matrix2 = matrix;
		m_textLineMatrix = (m_textMatrix = matrix2 * m_textLineMatrix);
	}

	private void SetTextLeading(float txtLeading)
	{
		m_textLeading = 0f - txtLeading;
	}

	private void GetScalingFactor(string[] scaling)
	{
		TextHorizontalScaling = float.Parse(scaling[0], CultureInfo.InvariantCulture);
	}

	private void GetXObject(string[] xobjectElement, PdfPageResources m_pageResources)
	{
		string key = StripSlashes(xobjectElement[0]);
		if (m_pageResources.ContainsKey(key) && m_pageResources[key] is XObjectElement && (m_pageResources[key] as XObjectElement).ObjectType == "Form")
		{
			PdfRecordCollection pdfRecordCollection = (m_pageResources[key] as XObjectElement).Render(m_pageResources);
			PdfDictionary xObjectDictionary = (m_pageResources[key] as XObjectElement).XObjectDictionary;
			PdfPageResources pageResources = new PdfPageResources();
			Dictionary<string, PdfMatrix> commonMatrix = new Dictionary<string, PdfMatrix>();
			if (xObjectDictionary.ContainsKey("Resources"))
			{
				PdfDictionary pdfDictionary = new PdfDictionary();
				pdfDictionary = ((!(xObjectDictionary["Resources"] is PdfReferenceHolder)) ? (xObjectDictionary["Resources"] as PdfDictionary) : ((xObjectDictionary["Resources"] as PdfReferenceHolder).Object as PdfDictionary));
				pageResources = m_resourceLoader.UpdatePageResources(pageResources, m_resourceLoader.GetImageResources(pdfDictionary, this, ref commonMatrix));
				pageResources = m_resourceLoader.UpdatePageResources(pageResources, m_resourceLoader.GetFontResources(pdfDictionary));
			}
			else
			{
				pageResources = UpdateFontResources(m_pageResources);
			}
			if (!m_isLayoutTextExtraction)
			{
				RenderText(pdfRecordCollection, pageResources);
			}
			else
			{
				RenderTextAsLayout(pdfRecordCollection, pageResources);
				resultantText += "\r\n";
			}
			pdfRecordCollection.RecordCollection.Clear();
			commonMatrix.Clear();
		}
	}

	private PdfPageResources UpdateFontResources(PdfPageResources pageResources)
	{
		PdfPageResources pdfPageResources = new PdfPageResources();
		foreach (KeyValuePair<string, object> resource in pageResources.Resources)
		{
			if (resource.Value is FontStructure)
			{
				pdfPageResources.Resources.Add(resource.Key, resource.Value);
				pdfPageResources.fontCollection.Add(resource.Key, resource.Value as FontStructure);
			}
		}
		return pdfPageResources;
	}

	private string RenderTextElement(string[] textElements, string tokenType, PdfPageResources m_pageResources)
	{
		try
		{
			string text = string.Join("", textElements);
			if (!m_pageResources.ContainsKey(m_currentFont) && m_currentFont != null && m_currentFont.Contains("-"))
			{
				m_currentFont = m_currentFont.Replace("-", "#2D");
			}
			if (m_pageResources.ContainsKey(m_currentFont))
			{
				FontStructure fontStructure = m_pageResources[m_currentFont] as FontStructure;
				fontStructure.IsTextExtraction = true;
				if (fontStructure != null)
				{
					fontStructure.FontSize = m_fontSize;
				}
				text = fontStructure.DecodeTextExtraction(text, isSameFont: true);
				if (pageWords == null)
				{
					pageWords = new List<string>();
				}
				pageWords.Add(text);
				fontStructure.IsTextExtraction = false;
				if (fontStructure.IsAdobeJapanFont && fontStructure.AdobeJapanCidMapTable != null && fontStructure.AdobeJapanCidMapTable.Count > 0)
				{
					string text2 = string.Empty;
					string text3 = text;
					for (int i = 0; i < text3.Length; i++)
					{
						string mapChar = text3[i].ToString();
						text2 += fontStructure.AdobeJapanCidMapTableGlyphParser(mapChar);
					}
					text = text2;
				}
				if (m_isLayoutTextExtraction)
				{
					m_structure = fontStructure;
					if (tokenType == "Tj")
					{
						m_tempBoundingRectangle = default(RectangleF);
					}
					text = RenderTextFromLeading(text, m_textLineMatrix);
				}
			}
			if (m_visualOrder && !string.IsNullOrEmpty(text) && Bidi.HasAnyRTL(text))
			{
				if (m_bidiInstance == null)
				{
					m_bidiInstance = new Bidi();
				}
				text = m_bidiInstance.GetLogicalToVisualString(text, isRTL: true);
			}
			return text;
		}
		catch
		{
			return null;
		}
	}

	private void RenderFont(string[] fontElements)
	{
		int i;
		for (i = 0; i < fontElements.Length; i++)
		{
			if (fontElements[i].Contains("/"))
			{
				m_currentFont = fontElements[i].Replace("/", "");
				break;
			}
		}
		m_fontSize = float.Parse(fontElements[i + 1], CultureInfo.InvariantCulture);
	}

	private string SkipEscapeSequence(string text)
	{
		int num = -1;
		do
		{
			num = text.IndexOf("\\", num + 1);
			if (text.Length > num + 1)
			{
				string text2 = text[num + 1].ToString();
				if (num >= 0)
				{
					switch (text2)
					{
					case "\\":
					case "(":
					case ")":
						text = text.Remove(num, 1);
						break;
					}
				}
			}
			else
			{
				text = text.Remove(num, 1);
				num = -1;
			}
		}
		while (num >= 0);
		return text;
	}

	private PdfDictionary GetPDFFontDictionary(PdfDictionary resources)
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		PdfDictionary pdfDictionary2 = null;
		IPdfPrimitive pDFXObject = GetPDFXObject(resources);
		if (pDFXObject != null && pDFXObject is PdfDictionary)
		{
			foreach (KeyValuePair<PdfName, IPdfPrimitive> item in ((PdfDictionary)pDFXObject).Items)
			{
				if (!(item.Value is PdfReferenceHolder))
				{
					continue;
				}
				PdfStream pdfStream = ((PdfReferenceHolder)item.Value).Object as PdfStream;
				PdfDictionary pdfDictionary3 = pdfStream;
				if (!pdfDictionary3.ContainsKey("Resources"))
				{
					continue;
				}
				pdfDictionary2 = pdfDictionary3["Resources"] as PdfDictionary;
				if (pdfDictionary2 == null && pdfDictionary3["Resources"] is PdfReferenceHolder)
				{
					PdfReferenceHolder pdfReferenceHolder = pdfDictionary3["Resources"] as PdfReferenceHolder;
					pdfDictionary2 = (pdfDictionary3["Resources"] as PdfReferenceHolder).Object as PdfDictionary;
					if (resourceNumber == pdfReferenceHolder.Reference.ObjNum)
					{
						continue;
					}
					resourceNumber = pdfReferenceHolder.Reference.ObjNum;
				}
				PdfDictionary pdfDictionary4 = null;
				if (pdfDictionary2.ContainsKey("Font"))
				{
					pdfDictionary4 = pdfDictionary2["Font"] as PdfDictionary;
				}
				if (pdfDictionary2.ContainsKey("XObject") && pdfDictionary4 == null)
				{
					PdfDictionary pDFFontDictionary = GetPDFFontDictionary(pdfDictionary2);
					if (pDFFontDictionary != null)
					{
						foreach (KeyValuePair<PdfName, IPdfPrimitive> item2 in pDFFontDictionary.Items)
						{
							if (!pdfDictionary.ContainsKey(item2.Key))
							{
								pdfDictionary.Items.Add(item2.Key, item2.Value);
							}
						}
					}
				}
				if (!pdfDictionary2.ContainsKey("Font"))
				{
					continue;
				}
				if (pdfDictionary2.Items[new PdfName("Font")] is PdfDictionary pdfDictionary5)
				{
					foreach (KeyValuePair<PdfName, IPdfPrimitive> item3 in pdfDictionary5.Items)
					{
						if (!pdfDictionary.ContainsKey(item3.Key))
						{
							pdfDictionary.Items.Add(item3.Key, item3.Value);
						}
					}
				}
				if (pdfDictionary3.ContainsKey("Subtype") && (pdfDictionary3.Items[new PdfName("Subtype")] as PdfName).Value != "Image")
				{
					if (m_xObjectContentStream == null)
					{
						m_xObjectContentStream = new List<PdfStream>();
					}
					m_xObjectContentStream.Add(pdfStream);
				}
			}
		}
		return pdfDictionary;
	}

	internal void GetFontStream()
	{
		_ = Dictionary;
		if (!Dictionary.ContainsKey("Resources"))
		{
			return;
		}
		PdfDictionary pdfDictionary = Dictionary["Resources"] as PdfDictionary;
		if (pdfDictionary != null)
		{
			PdfDictionary pdfDictionary2 = null;
			PdfDictionary pdfDictionary3 = null;
			bool flag = false;
			if (pdfDictionary.ContainsKey("Font"))
			{
				pdfDictionary2 = pdfDictionary["Font"] as PdfDictionary;
				PdfDictionary pDFFontDictionary = GetPDFFontDictionary(GetResources());
				if (pDFFontDictionary != null)
				{
					foreach (KeyValuePair<PdfName, IPdfPrimitive> item in pDFFontDictionary.Items)
					{
						if (!pdfDictionary2.ContainsKey(item.Key))
						{
							pdfDictionary2.Items.Add(item.Key, item.Value);
						}
					}
				}
				flag = true;
			}
			if (pdfDictionary.ContainsKey("XObject"))
			{
				PdfDictionary pdfDictionary4 = null;
				PdfResources resources = GetResources();
				IPdfPrimitive xObject = GetXObject(resources);
				while (true)
				{
					if (xObject != null && xObject is PdfDictionary)
					{
						foreach (KeyValuePair<PdfName, IPdfPrimitive> item2 in ((PdfDictionary)xObject).Items)
						{
							if (item2.Value is PdfReferenceHolder)
							{
								PdfDictionary pdfDictionary5 = ((PdfReferenceHolder)item2.Value).Object as PdfStream;
								if (pdfDictionary5.ContainsKey("Resources"))
								{
									pdfDictionary4 = pdfDictionary5["Resources"] as PdfDictionary;
								}
							}
						}
					}
					if (pdfDictionary4 == null)
					{
						break;
					}
					if (pdfDictionary4.ContainsKey("Font"))
					{
						pdfDictionary3 = pdfDictionary4["Font"] as PdfDictionary;
						break;
					}
					if (xObject == null || !pdfDictionary4.ContainsKey("XObject") || !(PdfCrossTable.Dereference(pdfDictionary4["XObject"]) is PdfDictionary baseDictionary))
					{
						break;
					}
					resources = new PdfResources(baseDictionary);
					xObject = GetXObject(resources);
				}
			}
			if (pdfDictionary2 != null)
			{
				m_fontcollect = pdfDictionary2.Items;
				foreach (KeyValuePair<PdfName, IPdfPrimitive> item3 in m_fontcollect)
				{
					m_fontNames.Add(item3.Key);
					m_fontReference.Add(item3.Value);
				}
			}
			if (pdfDictionary3 != null)
			{
				m_fontcollect = pdfDictionary3.Items;
				foreach (KeyValuePair<PdfName, IPdfPrimitive> item4 in m_fontcollect)
				{
					m_fontNames.Add(item4.Key);
					m_fontReference.Add(item4.Value);
				}
			}
			else if (pdfDictionary["Font"] is PdfReferenceHolder)
			{
				pdfDictionary2 = (pdfDictionary["Font"] as PdfReferenceHolder).Object as PdfDictionary;
				if (pdfDictionary2 != null)
				{
					m_fontNames = new List<PdfName>();
					m_fontReference = new List<IPdfPrimitive>();
					foreach (KeyValuePair<PdfName, IPdfPrimitive> item5 in pdfDictionary2.Items)
					{
						m_fontNames.Add(item5.Key);
						m_fontReference.Add(item5.Value);
					}
				}
			}
			if (pdfDictionary2 == null && pdfDictionary.ContainsKey("XObject") && pdfDictionary["XObject"] is PdfDictionary pdfDictionary6)
			{
				foreach (KeyValuePair<PdfName, IPdfPrimitive> item6 in pdfDictionary6.Items)
				{
					if (!(item6.Value is PdfReferenceHolder))
					{
						continue;
					}
					PdfDictionary pdfDictionary7 = (item6.Value as PdfReferenceHolder).Object as PdfDictionary;
					if (!pdfDictionary7.ContainsKey("Resources"))
					{
						continue;
					}
					pdfDictionary7 = pdfDictionary7["Resources"] as PdfDictionary;
					if (!pdfDictionary7.ContainsKey("Font"))
					{
						continue;
					}
					m_fontNames = new List<PdfName>();
					m_fontReference = new List<IPdfPrimitive>();
					PdfDictionary pdfDictionary8 = pdfDictionary7["Font"] as PdfDictionary;
					if (pdfDictionary8 == null)
					{
						PdfReferenceHolder pdfReferenceHolder = pdfDictionary7["Font"] as PdfReferenceHolder;
						if (pdfReferenceHolder != null)
						{
							pdfDictionary8 = pdfReferenceHolder.Object as PdfDictionary;
						}
					}
					foreach (KeyValuePair<PdfName, IPdfPrimitive> item7 in pdfDictionary8.Items)
					{
						m_fontNames.Add(item7.Key);
						m_fontReference.Add(item7.Value);
					}
				}
			}
			if (flag || pdfDictionary3 != null)
			{
				return;
			}
			pdfDictionary2 = GetPDFFontDictionary(GetResources());
			if (pdfDictionary2 == null)
			{
				return;
			}
			m_fontNames = new List<PdfName>();
			m_fontReference = new List<IPdfPrimitive>();
			{
				foreach (KeyValuePair<PdfName, IPdfPrimitive> item8 in pdfDictionary2.Items)
				{
					m_fontNames.Add(item8.Key);
					m_fontReference.Add(item8.Value);
				}
				return;
			}
		}
		if (pdfDictionary != null)
		{
			return;
		}
		PdfReferenceHolder pdfReferenceHolder2 = Dictionary["Resources"] as PdfReferenceHolder;
		if (!(pdfReferenceHolder2 != null) || !(pdfReferenceHolder2.Object is PdfDictionary))
		{
			return;
		}
		PdfDictionary pdfDictionary9 = pdfReferenceHolder2.Object as PdfDictionary;
		if (pdfDictionary9["Font"] is PdfDictionary)
		{
			if (!(pdfDictionary9["Font"] is PdfDictionary pdfDictionary10))
			{
				return;
			}
			m_fontNames = new List<PdfName>();
			m_fontReference = new List<IPdfPrimitive>();
			{
				foreach (KeyValuePair<PdfName, IPdfPrimitive> item9 in pdfDictionary10.Items)
				{
					m_fontNames.Add(item9.Key);
					m_fontReference.Add(item9.Value);
				}
				return;
			}
		}
		if (!(pdfDictionary9["Font"] is PdfReferenceHolder) || !((pdfDictionary9["Font"] as PdfReferenceHolder).Object is PdfDictionary pdfDictionary11))
		{
			return;
		}
		m_fontNames = new List<PdfName>();
		m_fontReference = new List<IPdfPrimitive>();
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item10 in pdfDictionary11.Items)
		{
			m_fontNames.Add(item10.Key);
			m_fontReference.Add(item10.Value);
		}
	}

	internal IPdfPrimitive GetXObject(PdfResources resources)
	{
		IPdfPrimitive result = null;
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item in resources.Items)
		{
			if (item.Key.ToString() == "/XObject")
			{
				result = PdfCrossTable.Dereference(item.Value);
			}
		}
		return result;
	}

	private IPdfPrimitive GetPDFXObject(PdfDictionary resources)
	{
		IPdfPrimitive result = null;
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item in resources.Items)
		{
			if (item.Key.ToString() == "/XObject")
			{
				result = PdfCrossTable.Dereference(item.Value);
			}
		}
		return result;
	}

	private string StripSlashes(string text)
	{
		return text.Replace("/", "");
	}

	internal string ExtractTaggedText(List<int> mcids, out Dictionary<string, object> properties)
	{
		if (m_mcidAndTextElements == null)
		{
			DeviceCMYK cmyk = new DeviceCMYK();
			Page page = new Page(this);
			page.Initialize(this, needParsing: false);
			if (page.RecordCollection == null)
			{
				page.Initialize(this, needParsing: true);
			}
			ImageRenderer imageRenderer = new ImageRenderer(page.RecordCollection, page.Resources, page.Height, page.CurrentLeftLocation, cmyk);
			imageRenderer.m_extractTags = true;
			imageRenderer.isExtractLineCollection = true;
			imageRenderer.RenderAsImage();
			imageRenderer.isExtractLineCollection = false;
			imageRenderer.m_extractTags = false;
			m_abbreviationCollection = imageRenderer.m_abbreviationCollection;
			foreach (TextElement item in imageRenderer.extractTextElement)
			{
				if (m_mcidAndTextElements == null)
				{
					m_mcidAndTextElements = new Dictionary<int, List<TextElement>>();
				}
				if (m_mcidAndTextElements.ContainsKey(item.m_mcid))
				{
					m_mcidAndTextElements[item.m_mcid].Add(item);
					continue;
				}
				m_mcidAndTextElements[item.m_mcid] = new List<TextElement> { item };
			}
		}
		List<TextElement> list = new List<TextElement>();
		string text = "";
		string value = null;
		bool flag = true;
		foreach (int mcid in mcids)
		{
			if (m_mcidAndTextElements != null && m_mcidAndTextElements.ContainsKey(mcid))
			{
				list.AddRange(m_mcidAndTextElements[mcid]);
			}
			if (flag && m_abbreviationCollection != null && m_abbreviationCollection.ContainsKey(mcid))
			{
				value = m_abbreviationCollection[mcid];
				flag = false;
			}
		}
		properties = new Dictionary<string, object>();
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		RectangleF? rectangleF = null;
		bool flag2 = true;
		for (int num5 = 0; num5 < list.Count; num5++)
		{
			TextElement textElement = list[num5];
			if (flag2)
			{
				if (string.IsNullOrEmpty(textElement.FontName))
				{
					_ = textElement.FontSize;
					if (!(textElement.FontSize > 0f))
					{
						goto IL_026d;
					}
				}
				flag2 = false;
				properties.Add("FontName", textElement.FontName);
				properties.Add("FontSize", textElement.FontSize);
				_ = textElement.FontStyle;
				properties.Add("FontStyle", textElement.FontStyle);
			}
			goto IL_026d;
			IL_026d:
			int count = textElement.textElementGlyphList.Count;
			for (int i = 0; i < count; i++)
			{
				DocGen.PdfViewer.Base.Glyph glyph = textElement.textElementGlyphList[i];
				string text2 = string.Empty;
				if (glyph.ToUnicode != null && glyph.ToUnicode != string.Empty && glyph.ToUnicode.Length == 1)
				{
					text2 = Convert.ToChar(glyph.ToUnicode).ToString();
				}
				RectangleF value2 = new RectangleF((float)glyph.BoundingRect.X, (float)glyph.BoundingRect.Y, (float)glyph.BoundingRect.Width, (float)glyph.BoundingRect.Height);
				text = ((!rectangleF.HasValue || !(rectangleF != RectangleF.Empty)) ? (text + text2) : ((!(Math.Abs(rectangleF.Value.X + rectangleF.Value.Width - value2.X) < 1f)) ? (text + " " + text2) : (text + text2)));
				if (i == 0)
				{
					if (num3 == 0f || value2.X < num3)
					{
						num3 = value2.X;
					}
					if (num4 == 0f || value2.Y < num4)
					{
						num4 = value2.Y;
					}
				}
				if (i == count - 1)
				{
					if (num == 0f || value2.X - num3 + value2.Width > num)
					{
						num = value2.X - num3 + value2.Width;
					}
					if (num2 == 0f || value2.Y - num4 + value2.Height > num2)
					{
						num2 = value2.Y - num4 + value2.Height;
					}
				}
				rectangleF = value2;
			}
		}
		rectangleF = null;
		list.Clear();
		if (!string.IsNullOrEmpty(value))
		{
			properties.Add("Abbreviation", value);
		}
		properties.Add("Bounds", new RectangleF(num3, num4, num, num2));
		return text;
	}

	internal RectangleF ExtractTaggedContent(out string abbreviation, int mcid, out bool objectType)
	{
		objectType = false;
		if (m_mcidAndFigureBounds == null)
		{
			RectangleF rectangleF = default(RectangleF);
			int key = 0;
			Stack<DocGen.Drawing.Matrix> stack = new Stack<DocGen.Drawing.Matrix>();
			DocGen.Drawing.Matrix matrix = null;
			stack.Push(new DocGen.Drawing.Matrix(1f, 0f, 0f, 1f, 0f, 0f));
			PdfDictionary pdfDictionary = PdfCrossTable.Dereference(Dictionary["Resources"]) as PdfDictionary;
			if (pdfDictionary == null)
			{
				pdfDictionary = new PdfDictionary();
			}
			PdfPageResources pageResources = PageResourceLoader.Instance.GetPageResources(this);
			if (pdfDictionary.ContainsKey("XObject"))
			{
				using (new MemoryStream())
				{
					Page page = new Page(this);
					page.Initialize(this, needParsing: false);
					if (page.RecordCollection == null)
					{
						page.Initialize(this, needParsing: true);
					}
					m_recordCollection = page.RecordCollection;
				}
				for (int i = 0; i < m_recordCollection.RecordCollection.Count; i++)
				{
					string text = m_recordCollection.RecordCollection[i].OperatorName;
					string[] operands = m_recordCollection.RecordCollection[i].Operands;
					char[] symbolChars = m_symbolChars;
					for (int j = 0; j < symbolChars.Length; j++)
					{
						char c = symbolChars[j];
						if (text.Contains(c.ToString()))
						{
							text = text.Replace(c.ToString(), "");
						}
					}
					switch (text.Trim())
					{
					case "q":
					{
						DocGen.Drawing.Matrix item2 = new DocGen.Drawing.Matrix(1f, 0f, 0f, 1f, 0f, 0f);
						if (stack.Count > 0)
						{
							item2 = stack.Peek();
						}
						stack.Push(item2);
						break;
					}
					case "BDC":
					{
						key = -1;
						char[] separator = new char[5] { ' ', '(', ')', '>', '<' };
						string[] array = operands[1].Replace("/", " ").Split(separator);
						string value = null;
						for (int k = 0; k < array.Length; k++)
						{
							string text2 = array[k];
							if (text2 == "E")
							{
								if (k + 1 < array.Length && array[k + 1].Length > 2)
								{
									value = array[k + 1];
									k++;
								}
							}
							else
							{
								if (!(text2 == "MCID"))
								{
									continue;
								}
								try
								{
									key = int.Parse(array[k + 1]);
									if (!string.IsNullOrEmpty(value))
									{
										if (m_abbreviationCollection == null)
										{
											m_abbreviationCollection = new Dictionary<int, string>();
										}
										m_abbreviationCollection[key] = value;
									}
								}
								catch (Exception)
								{
									key = -1;
								}
								break;
							}
						}
						break;
					}
					case "cm":
					{
						float m = float.Parse(operands[0]);
						float m2 = float.Parse(operands[1]);
						float m3 = float.Parse(operands[2]);
						float m4 = float.Parse(operands[3]);
						float dx = float.Parse(operands[4]);
						float dy = float.Parse(operands[5]);
						matrix = new DocGen.Drawing.Matrix(m, m2, m3, m4, dx, dy);
						DocGen.Drawing.Matrix item = Multiply(matrix, stack.Peek());
						stack.Pop();
						stack.Push(item);
						break;
					}
					case "Do":
					{
						DocGen.Drawing.Matrix matrix2 = new DocGen.Drawing.Matrix(1f, 0f, 0f, -1.01f, 0f, 1f);
						DocGen.Drawing.Matrix matrix3 = Multiply(matrix2, stack.Peek());
						DocGen.Drawing.Matrix matrix4 = new DocGen.Drawing.Matrix(1.3333334f, 0f, 0f, -1.3333334f, 0f, Size.Height * 1.3333334f);
						matrix3 = Multiply(matrix3, matrix4);
						rectangleF = ((Rotation == PdfPageRotateAngle.RotateAngle270) ? ((matrix3.Elements[0] == 0f || matrix3.Elements[3] == 0f) ? new RectangleF((float)Math.Floor(matrix3.OffsetY / 1.3333334f), (float)Math.Floor(Size.Width) - ((float)Math.Round(matrix3.OffsetX / 1.3333334f, 5) + (float)Math.Floor(matrix3.Elements[0] / 1.3333334f)), Math.Abs(stack.Peek().Elements[1]), Math.Abs(stack.Peek().Elements[2])) : new RectangleF((float)Math.Floor(matrix3.OffsetY / 1.3333334f), Size.Width - ((float)Math.Round(matrix3.OffsetX / 1.3333334f, 5) + matrix3.Elements[0] / 1.3333334f), stack.Peek().Elements[3], stack.Peek().Elements[0])) : ((Rotation == PdfPageRotateAngle.RotateAngle90) ? ((matrix3.Elements[0] != 0f || matrix3.Elements[3] != 0f) ? new RectangleF(new PointF(Size.Height - matrix3.OffsetY / 1.3333334f - stack.Peek().Elements[3], matrix3.Elements[4] / 1.3333334f), new SizeF(stack.Peek().Elements[3], stack.Peek().Elements[0])) : new RectangleF(new PointF(Size.Height - matrix3.Elements[5] / 1.3333334f, matrix3.Elements[4] / 1.3333334f), new SizeF(stack.Peek().Elements[1], 0f - stack.Peek().Elements[2]))) : ((Rotation == PdfPageRotateAngle.RotateAngle180) ? new RectangleF(Size.Width - matrix3.OffsetX / 1.3333334f - stack.Peek().Elements[0], Size.Height - stack.Peek().Elements[3] - (float)Math.Round(matrix3.OffsetY / 1.3333334f, 5), stack.Peek().Elements[0], stack.Peek().Elements[3]) : ((matrix3.Elements[0] != 0f || matrix3.Elements[3] != 0f) ? new RectangleF(matrix3.OffsetX / 1.3333334f, (float)Math.Round(matrix3.OffsetY / 1.3333334f, 5), stack.Peek().Elements[0], stack.Peek().Elements[3]) : ((matrix3.Elements[1] < 0f && matrix3.Elements[2] > 0f) ? new RectangleF(Size.Height - matrix3.Elements[5] / 1.3333334f, (float)Math.Round(matrix3.Elements[4] / 1.3333334f, 5), 0f - stack.Peek().Elements[2], stack.Peek().Elements[1]) : ((matrix3.Elements[1] > 0f && matrix3.Elements[2] < 0f) ? new RectangleF(matrix3.Elements[5] / 1.3333334f, (float)Math.Round(Size.Width - matrix3.Elements[4] / 1.3333334f, 5), 0f - stack.Peek().Elements[2], 0f - stack.Peek().Elements[1]) : ((!(matrix3.Elements[1] < 0f) || !(matrix3.Elements[2] < 0f)) ? new RectangleF(Size.Width - matrix3.OffsetX / 1.3333334f - stack.Peek().Elements[0], Size.Height - stack.Peek().Elements[3] - (float)Math.Round(matrix3.OffsetY / 1.3333334f, 5), stack.Peek().Elements[0], stack.Peek().Elements[3]) : new RectangleF(Size.Width - matrix3.OffsetX / 1.3333334f - stack.Peek().Elements[0], Size.Height - stack.Peek().Elements[3] - (float)Math.Round(matrix3.OffsetY / 1.3333334f, 5), stack.Peek().Elements[0], stack.Peek().Elements[3]))))))));
						XObjectElement xObjectElement = null;
						if (pageResources != null && pageResources.ContainsKey(operands[0].Replace("/", "")))
						{
							xObjectElement = pageResources[operands[0].Replace("/", "")] as XObjectElement;
						}
						if (xObjectElement != null && xObjectElement.ObjectType == "Form")
						{
							objectType = true;
						}
						if (m_mcidAndFigureBounds == null)
						{
							m_mcidAndFigureBounds = new Dictionary<int, Dictionary<RectangleF, bool>>();
						}
						if (m_mcidAndFigureBounds.ContainsKey(key))
						{
							PdfPath pdfPath = new PdfPath();
							Dictionary<RectangleF, bool> dictionary = m_mcidAndFigureBounds[key];
							RectangleF rectangle = RectangleF.Empty;
							foreach (KeyValuePair<RectangleF, bool> item3 in dictionary)
							{
								rectangle = item3.Key;
								objectType = (item3.Value ? item3.Value : objectType);
							}
							pdfPath.AddRectangle(rectangle);
							pdfPath.AddRectangle(rectangleF);
							rectangleF = pdfPath.GetBounds();
							Dictionary<RectangleF, bool> dictionary2 = new Dictionary<RectangleF, bool>();
							dictionary2[rectangleF] = objectType;
							m_mcidAndFigureBounds[key] = dictionary2;
						}
						else
						{
							Dictionary<RectangleF, bool> dictionary3 = new Dictionary<RectangleF, bool>();
							dictionary3[rectangleF] = objectType;
							m_mcidAndFigureBounds[key] = dictionary3;
						}
						objectType = false;
						break;
					}
					case "Q":
						stack.Pop();
						break;
					}
				}
			}
		}
		abbreviation = ((m_abbreviationCollection != null && m_abbreviationCollection.ContainsKey(mcid)) ? m_abbreviationCollection[mcid] : null);
		if (m_mcidAndFigureBounds != null && m_mcidAndFigureBounds.ContainsKey(mcid))
		{
			Dictionary<RectangleF, bool> dictionary4 = m_mcidAndFigureBounds[mcid];
			RectangleF result = RectangleF.Empty;
			{
				foreach (KeyValuePair<RectangleF, bool> item4 in dictionary4)
				{
					result = item4.Key;
					objectType = item4.Value;
				}
				return result;
			}
		}
		return RectangleF.Empty;
	}

	private DocGen.Drawing.Matrix Multiply(DocGen.Drawing.Matrix matrix1, DocGen.Drawing.Matrix matrix2)
	{
		return new DocGen.Drawing.Matrix(matrix1.Elements[0] * matrix2.Elements[0] + matrix1.Elements[1] * matrix2.Elements[2], matrix1.Elements[0] * matrix2.Elements[1] + matrix1.Elements[1] * matrix2.Elements[3], matrix1.Elements[2] * matrix2.Elements[0] + matrix1.Elements[3] * matrix2.Elements[2], matrix1.Elements[2] * matrix2.Elements[1] + matrix1.Elements[3] * matrix2.Elements[3], matrix1.OffsetX * matrix2.Elements[0] + matrix1.OffsetY * matrix2.Elements[2] + matrix2.OffsetX, matrix1.OffsetX * matrix2.Elements[1] + matrix1.OffsetY * matrix2.Elements[3] + matrix2.OffsetY);
	}

	internal virtual PdfResources GetResources()
	{
		if (m_resources == null)
		{
			m_resources = new PdfResources();
			Dictionary["Resources"] = m_resources;
		}
		return m_resources;
	}

	internal void SetResources(PdfResources res)
	{
		m_resources = res;
		Dictionary["Resources"] = m_resources;
		m_modified = true;
	}

	internal void SetProgress()
	{
		m_isProgressOn = true;
	}

	internal void ResetProgress()
	{
		m_isProgressOn = false;
	}

	internal PdfTemplate GetContent()
	{
		lock (s_syncLockTemplate)
		{
			m_modified = false;
			m_layersCount = ((m_layers != null) ? m_layers.Count : 0);
			m_annotCount = GetAnnotationCount();
			MemoryStream memoryStream = new MemoryStream();
			Layers.CombineContent(memoryStream);
			m_pageContentLength = memoryStream.Length;
			bool isLoadedPage = false;
			if (this is PdfLoadedPage)
			{
				isLoadedPage = true;
			}
			PdfDictionary resources = PdfCrossTable.Dereference(Dictionary["Resources"]) as PdfDictionary;
			return new PdfTemplate(Origin, Size, memoryStream, resources, isLoadedPage, this);
		}
	}

	private void SetMatrix(PdfTemplate template)
	{
		PdfArray pdfArray = null;
		_ = new float[0];
		bool flag = false;
		if (this is PdfLoadedPage { CropBox: { X: 0f }, CropBox: { Y: 0f } })
		{
			flag = true;
		}
		if (PdfCrossTable.Dereference(template.m_content["BBox"]) is PdfArray && (Rotation != 0 || Rotation != 0))
		{
			PdfTransformationMatrix pdfTransformationMatrix = new PdfTransformationMatrix();
			if (Rotation == PdfPageRotateAngle.RotateAngle180)
			{
				pdfTransformationMatrix.Rotate(180f);
			}
			else if (Rotation == PdfPageRotateAngle.RotateAngle270 && Size.Width > Size.Height && m_isBooklet)
			{
				pdfTransformationMatrix.Rotate(-270f);
			}
			else if (Rotation == PdfPageRotateAngle.RotateAngle90 && Size.Width > Size.Height && flag && m_isBooklet)
			{
				pdfTransformationMatrix.Rotate(-90f);
			}
			pdfArray = new PdfArray(pdfTransformationMatrix.Matrix.Elements);
		}
		if (pdfArray != null)
		{
			template.m_content["Matrix"] = pdfArray;
			template.isContainPageRotation = true;
		}
	}

	private void SetTemplateMatrix(PdfDictionary template)
	{
		if (template["BBox"] is PdfArray { Count: >3 } pdfArray)
		{
			PdfNumber pdfNumber = pdfArray[0] as PdfNumber;
			PdfNumber pdfNumber2 = pdfArray[1] as PdfNumber;
			if (pdfNumber != null && pdfNumber2 != null && (pdfNumber.FloatValue < 0f || pdfNumber2.FloatValue < 0f))
			{
				float[] array = new float[6]
				{
					1f,
					0f,
					0f,
					1f,
					0f - pdfNumber.FloatValue,
					0f - pdfNumber2.FloatValue
				};
				template["Matrix"] = new PdfArray(array);
			}
		}
	}

	internal PdfArray ReInitializeContentReference()
	{
		IPdfPrimitive pdfPrimitive = m_pageDictionary["Contents"];
		PdfArray pdfArray = pdfPrimitive as PdfArray;
		PdfArray pdfArray2 = new PdfArray();
		if (pdfArray != null)
		{
			for (int i = 0; i < pdfArray.Elements.Count; i++)
			{
				if (pdfArray.Elements[i] as PdfReferenceHolder != null && (pdfArray.Elements[i] as PdfReferenceHolder).Object is PdfStream obj)
				{
					pdfArray2.Elements.Add(new PdfReferenceHolder(obj));
				}
			}
		}
		else if (pdfPrimitive as PdfReferenceHolder != null)
		{
			PdfStream pdfStream = (pdfPrimitive as PdfReferenceHolder).Object as PdfStream;
			if (pdfStream != null)
			{
				m_pageDictionary["Contents"] = new PdfReferenceHolder(pdfStream);
			}
			else if (pdfStream == null && (pdfPrimitive as PdfReferenceHolder).Object is PdfArray)
			{
				pdfArray = (pdfPrimitive as PdfReferenceHolder).Object as PdfArray;
				for (int j = 0; j < pdfArray.Elements.Count; j++)
				{
					if (pdfArray.Elements[j] as PdfReferenceHolder != null && (pdfArray.Elements[j] as PdfReferenceHolder).Object is PdfStream obj2)
					{
						pdfArray2.Elements.Add(new PdfReferenceHolder(obj2));
					}
				}
			}
		}
		return pdfArray2;
	}

	internal PdfDictionary CheckTypeOfXObject(PdfDictionary xObjectDictionary)
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		foreach (PdfName key in xObjectDictionary.Keys)
		{
			PdfStream pdfStream = PdfCrossTable.Dereference(xObjectDictionary[key]) as PdfStream;
			PdfName pdfName = pdfStream["Subtype"] as PdfName;
			PdfReferenceHolder referenceHolder = xObjectDictionary[key] as PdfReferenceHolder;
			if (IsRepeatedResource(referenceHolder))
			{
				continue;
			}
			string value = pdfName.Value;
			if (!(value == "Image"))
			{
				if (value == "Form")
				{
					ReInitializeFormXObject(pdfStream);
				}
			}
			else
			{
				ReInitializeImageData(pdfStream);
			}
			if (pdfStream != null)
			{
				pdfDictionary.Items.Add(key, new PdfReferenceHolder(pdfStream));
			}
		}
		return pdfDictionary;
	}

	internal void ReInitializeFormXObject(PdfStream formXObjectData)
	{
		if (formXObjectData.ContainsKey("Resources"))
		{
			PdfDictionary pdfDictionary = null;
			pdfDictionary = ((!(formXObjectData["Resources"] as PdfReferenceHolder != null)) ? (formXObjectData["Resources"] as PdfDictionary) : (PdfCrossTable.Dereference(formXObjectData["Resources"]) as PdfDictionary));
			ReInitializeXobjectResources(pdfDictionary);
			if (formXObjectData["Resources"] as PdfReferenceHolder != null)
			{
				formXObjectData["Resources"] = new PdfReferenceHolder(pdfDictionary);
			}
			else
			{
				formXObjectData["Resources"] = pdfDictionary;
			}
		}
		if (formXObjectData.ContainsKey((PdfName)"OC"))
		{
			PdfDictionary obj = CheckOptionalContent(formXObjectData);
			formXObjectData.Items[(PdfName)"OC"] = new PdfReferenceHolder(obj);
		}
		if (formXObjectData.ContainsKey((PdfName)"PieceInfo"))
		{
			PdfDictionary pdfDictionary2 = formXObjectData.Items[(PdfName)"PieceInfo"] as PdfDictionary;
			foreach (PdfName key in pdfDictionary2.Keys)
			{
				PdfDictionary pdfDictionary3 = pdfDictionary2[key] as PdfDictionary;
				foreach (PdfName key2 in pdfDictionary3.Keys)
				{
					if (pdfDictionary3[key2] as PdfReferenceHolder != null)
					{
						if ((pdfDictionary3[key2] as PdfReferenceHolder).Object is PdfStream obj2)
						{
							pdfDictionary3[key2] = new PdfReferenceHolder(obj2);
						}
						break;
					}
				}
			}
		}
		if (formXObjectData.ContainsKey("Group"))
		{
			PdfDictionary pdfDictionary4 = null;
			pdfDictionary4 = ((!(formXObjectData["Group"] as PdfReferenceHolder != null)) ? (formXObjectData["Group"] as PdfDictionary) : (PdfCrossTable.Dereference(formXObjectData["Group"]) as PdfDictionary));
			if (pdfDictionary4.ContainsKey("CS") && PdfCrossTable.Dereference(pdfDictionary4["CS"]) is PdfArray pdfArray)
			{
				ReinitializeColorSpace(pdfArray);
				pdfDictionary4["CS"] = new PdfReferenceHolder(pdfArray);
			}
			if (formXObjectData["Group"] as PdfReferenceHolder != null)
			{
				formXObjectData["Group"] = new PdfReferenceHolder(pdfDictionary4);
			}
			else
			{
				formXObjectData["Group"] = pdfDictionary4;
			}
		}
	}

	internal PdfDictionary CheckOptionalContent(PdfStream xObjectData)
	{
		PdfDictionary pdfDictionary = (xObjectData.Items[(PdfName)"OC"] as PdfReferenceHolder).Object as PdfDictionary;
		PdfDictionary pdfDictionary2 = new PdfDictionary();
		foreach (PdfName key in pdfDictionary.Keys)
		{
			if (pdfDictionary[key] as PdfReferenceHolder != null)
			{
				PdfReferenceHolder pdfReferenceHolder = pdfDictionary[key] as PdfReferenceHolder;
				pdfDictionary2.Items.Add(key, pdfReferenceHolder.Object as PdfDictionary);
			}
		}
		foreach (PdfName key2 in pdfDictionary2.Keys)
		{
			if (pdfDictionary[key2] as PdfReferenceHolder != null)
			{
				pdfDictionary[key2] = new PdfReferenceHolder(pdfDictionary2[key2]);
			}
		}
		return pdfDictionary;
	}

	internal void ReInitializeImageData(PdfStream imageData)
	{
		if (imageData.ContainsKey("OC"))
		{
			PdfDictionary obj = CheckOptionalContent(imageData);
			imageData.Items[(PdfName)"OC"] = new PdfReferenceHolder(obj);
		}
		if (imageData.ContainsKey("SMask"))
		{
			PdfDictionary obj2 = PdfCrossTable.Dereference(imageData["SMask"]) as PdfDictionary;
			imageData.Items[(PdfName)"SMask"] = new PdfReferenceHolder(obj2);
		}
		if (imageData.ContainsKey("ColorSpace"))
		{
			IPdfPrimitive pdfPrimitive = imageData.Items[(PdfName)"ColorSpace"];
			if (pdfPrimitive as PdfReferenceHolder != null)
			{
				pdfPrimitive = PdfCrossTable.Dereference(pdfPrimitive) as PdfArray;
			}
			if (pdfPrimitive is PdfArray)
			{
				PdfArray pdfArray = pdfPrimitive as PdfArray;
				ReinitializeColorSpace(pdfArray);
				if (imageData["ColorSpace"] as PdfReferenceHolder != null)
				{
					imageData["ColorSpace"] = new PdfReferenceHolder(pdfArray);
				}
				else
				{
					imageData["ColorSpace"] = pdfArray;
				}
			}
		}
		if (imageData.Items.ContainsKey((PdfName)"Metadata"))
		{
			PdfStream obj3 = PdfCrossTable.Dereference(imageData["Metadata"]) as PdfStream;
			imageData["Metadata"] = new PdfReferenceHolder(obj3);
		}
		if (imageData.ContainsKey("Filter") && imageData["Filter"] as PdfReferenceHolder != null)
		{
			imageData["Filter"] = new PdfReferenceHolder(PdfCrossTable.Dereference(imageData["Filter"]));
		}
		if (!imageData.ContainsKey("DecodeParms"))
		{
			return;
		}
		PdfDictionary pdfDictionary = imageData["DecodeParms"] as PdfDictionary;
		PdfDictionary pdfDictionary2 = new PdfDictionary();
		if (pdfDictionary != null)
		{
			foreach (PdfName key in pdfDictionary.Keys)
			{
				if (pdfDictionary[key] as PdfReferenceHolder != null && PdfCrossTable.Dereference(pdfDictionary[key]) is PdfStream obj4)
				{
					pdfDictionary2.Items.Add(key, new PdfReferenceHolder(obj4));
				}
			}
			foreach (PdfName key2 in pdfDictionary2.Keys)
			{
				if (pdfDictionary.ContainsKey(key2))
				{
					pdfDictionary[key2] = pdfDictionary2[key2];
				}
			}
		}
		if (pdfDictionary != null)
		{
			imageData["DecodeParms"] = pdfDictionary;
		}
	}

	internal PdfDictionary ReinitializePageResources()
	{
		IPdfPrimitive pdfPrimitive = m_pageDictionary["Resources"];
		PdfDictionary pdfDictionary = null;
		pdfDictionary = ((!(pdfPrimitive as PdfReferenceHolder != null)) ? (pdfPrimitive as PdfDictionary) : (PdfCrossTable.Dereference(pdfPrimitive) as PdfDictionary));
		new PdfDictionary();
		if (pdfDictionary.ContainsKey("Font"))
		{
			PdfDictionary pdfDictionary2 = null;
			pdfDictionary2 = ((!(pdfDictionary["Font"] as PdfReferenceHolder != null)) ? (pdfDictionary["Font"] as PdfDictionary) : (PdfCrossTable.Dereference(pdfDictionary["Font"]) as PdfDictionary));
			PdfDictionary pdfDictionary3 = new PdfDictionary();
			foreach (PdfName key in pdfDictionary2.Keys)
			{
				if (pdfDictionary2[key] as PdfReferenceHolder != null && PdfCrossTable.Dereference(pdfDictionary2[key]) is PdfDictionary pdfDictionary4)
				{
					CheckFontInternalReference(pdfDictionary4);
					pdfDictionary3.Items.Add(key, new PdfReferenceHolder(pdfDictionary4));
				}
			}
			if (pdfDictionary["Font"] as PdfReferenceHolder != null)
			{
				pdfDictionary["Font"] = new PdfReferenceHolder(pdfDictionary3);
			}
			else
			{
				pdfDictionary["Font"] = pdfDictionary3;
			}
		}
		if (pdfDictionary.ContainsKey("XObject"))
		{
			PdfDictionary pdfDictionary5 = null;
			m_xobjectReferenceCollection = new List<PdfReference>();
			pdfDictionary5 = ((!(pdfDictionary["XObject"] as PdfReferenceHolder != null)) ? (pdfDictionary["XObject"] as PdfDictionary) : (PdfCrossTable.Dereference(pdfDictionary["XObject"]) as PdfDictionary));
			PdfDictionary pdfDictionary6 = CheckTypeOfXObject(pdfDictionary5);
			m_xobjectReferenceCollection.Clear();
			if (pdfDictionary["XObject"] as PdfReferenceHolder != null)
			{
				pdfDictionary["XObject"] = new PdfReferenceHolder(pdfDictionary6);
			}
			else
			{
				pdfDictionary["XObject"] = pdfDictionary6;
			}
		}
		if (pdfDictionary.ContainsKey("Pattern"))
		{
			_ = pdfDictionary["Pattern"];
			if (PdfCrossTable.Dereference(pdfDictionary["Pattern"]) is PdfDictionary pagePattern)
			{
				ReInitializePagePatterns(pagePattern);
			}
		}
		if (pdfDictionary.ContainsKey("ColorSpace"))
		{
			PdfDictionary pdfDictionary7 = null;
			pdfDictionary7 = ((!(pdfDictionary["ColorSpace"] as PdfReferenceHolder != null)) ? (pdfDictionary["ColorSpace"] as PdfDictionary) : (PdfCrossTable.Dereference(pdfDictionary["ColorSpace"]) as PdfDictionary));
			ReinitializeColorSpaceItem(pdfDictionary7);
			pdfDictionary["ColorSpace"] = pdfDictionary7;
		}
		if (pdfDictionary.ContainsKey("ExtGState"))
		{
			PdfDictionary pdfDictionary8 = null;
			pdfDictionary8 = ((!(pdfDictionary["ExtGState"] as PdfReferenceHolder != null)) ? (pdfDictionary["ExtGState"] as PdfDictionary) : (PdfCrossTable.Dereference(pdfDictionary["ExtGState"]) as PdfDictionary));
			PdfDictionary value = ReInitializeExtGState(pdfDictionary8);
			pdfDictionary["ExtGState"] = value;
		}
		if (pdfDictionary.ContainsKey("Shading"))
		{
			PdfDictionary pdfDictionary9 = null;
			pdfDictionary9 = ((!(pdfDictionary["Shading"] as PdfReferenceHolder != null)) ? (pdfDictionary["Shading"] as PdfDictionary) : (PdfCrossTable.Dereference(pdfDictionary["Shading"]) as PdfDictionary));
			CheckPageShadingReference(pdfDictionary9);
			if (pdfDictionary["Shading"] as PdfReferenceHolder != null)
			{
				pdfDictionary["Shading"] = new PdfReferenceHolder(pdfDictionary9);
			}
			else
			{
				pdfDictionary["Shading"] = pdfDictionary9;
			}
		}
		return pdfDictionary;
	}

	internal void CheckPageShadingReference(PdfDictionary pageShadingItems)
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		foreach (PdfName key in pageShadingItems.Keys)
		{
			PdfDictionary pdfDictionary2 = new PdfDictionary();
			if (pageShadingItems[key] as PdfReferenceHolder != null)
			{
				PdfDictionary pdfDictionary3 = PdfCrossTable.Dereference(pageShadingItems[key]) as PdfDictionary;
				if (pdfDictionary3.ContainsKey("Function") && (pdfDictionary3["Function"] as PdfReferenceHolder).Object is PdfDictionary obj)
				{
					pdfDictionary2.Items.Add((PdfName)"Function", new PdfReferenceHolder(obj));
				}
				if (pdfDictionary3.ContainsKey("ColorSpace") && pdfDictionary3["ColorSpace"] as PdfReferenceHolder != null)
				{
					if (PdfCrossTable.Dereference(pdfDictionary3["ColorSpace"]) as PdfName != null)
					{
						pdfDictionary2.Items.Add((PdfName)"ColorSpace", new PdfReferenceHolder(PdfCrossTable.Dereference(pdfDictionary3["ColorSpace"]) as PdfName));
					}
					else if (PdfCrossTable.Dereference(pdfDictionary3["ColorSpace"]) is PdfStream)
					{
						pdfDictionary2.Items.Add((PdfName)"ColorSpace", new PdfReferenceHolder(PdfCrossTable.Dereference(pdfDictionary3["ColorSpace"]) as PdfStream));
					}
					else if (PdfCrossTable.Dereference(pdfDictionary3["ColorSpace"]) is PdfArray)
					{
						ReinitializeColorSpace(PdfCrossTable.Dereference(pdfDictionary3["ColorSpace"]) as PdfArray);
						pdfDictionary2.Items.Add((PdfName)"ColorSpace", new PdfReferenceHolder(PdfCrossTable.Dereference(pdfDictionary3["ColorSpace"]) as PdfArray));
					}
				}
			}
			pdfDictionary.Items.Add(key, pdfDictionary2);
		}
		foreach (PdfName key2 in pdfDictionary.Keys)
		{
			PdfDictionary pdfDictionary4 = PdfCrossTable.Dereference(pdfDictionary[key2]) as PdfDictionary;
			PdfDictionary pdfDictionary5 = PdfCrossTable.Dereference(pageShadingItems[key2]) as PdfDictionary;
			foreach (PdfName key3 in pdfDictionary4.Keys)
			{
				pdfDictionary5[key3] = pdfDictionary4[key3];
			}
			if (pageShadingItems[key2] as PdfReferenceHolder != null)
			{
				pageShadingItems[key2] = new PdfReferenceHolder(pdfDictionary5);
			}
			else
			{
				pageShadingItems[key2] = pdfDictionary5;
			}
		}
	}

	internal void ReInitializePageAnnotation(PdfDictionary acroFormData)
	{
		if (acroFormData != null)
		{
			InitializeAcroformReference(acroFormData);
		}
		IPdfPrimitive pdfPrimitive = m_pageDictionary["Annots"];
		PdfArray pdfArray = null;
		pdfArray = ((!(pdfPrimitive as PdfReferenceHolder != null)) ? (pdfPrimitive as PdfArray) : (PdfCrossTable.Dereference(pdfPrimitive) as PdfArray));
		PdfArray pdfArray2 = new PdfArray();
		for (int i = 0; i < pdfArray.Count; i++)
		{
			PdfDictionary pdfDictionary = null;
			pdfDictionary = ((!(pdfArray[i] as PdfReferenceHolder != null)) ? (pdfArray[i] as PdfDictionary) : (PdfCrossTable.Dereference(pdfArray[i]) as PdfDictionary));
			if (pdfDictionary.ContainsKey("AP") && PdfCrossTable.Dereference(pdfDictionary["AP"]) is PdfDictionary annotAppearanceData)
			{
				CheckAnnotAppearanceData(annotAppearanceData);
			}
			if (pdfDictionary.ContainsKey("P"))
			{
				PdfDictionary obj = PdfCrossTable.Dereference(pdfDictionary["P"]) as PdfDictionary;
				pdfDictionary["P"] = new PdfReferenceHolder(obj);
			}
			if (pdfDictionary.ContainsKey("Popup") && pdfDictionary["Popup"] as PdfReferenceHolder != null)
			{
				PdfDictionary pdfDictionary2 = PdfCrossTable.Dereference(pdfDictionary["Popup"]) as PdfDictionary;
				if (pdfDictionary2.ContainsKey("Parent") && pdfDictionary2["Parent"] as PdfReferenceHolder != null)
				{
					pdfDictionary2["Parent"] = new PdfReferenceHolder(PdfCrossTable.Dereference(pdfDictionary2["Parent"]) as PdfDictionary);
				}
			}
			if (pdfDictionary.ContainsKey("Parent"))
			{
				PdfDictionary pdfDictionary3 = (pdfDictionary["Parent"] as PdfReferenceHolder).Object as PdfDictionary;
				if (pdfDictionary3.ContainsKey("Kids"))
				{
					PdfArray pdfArray3 = pdfDictionary3["Kids"] as PdfArray;
					PdfArray pdfArray4 = new PdfArray();
					for (int j = 0; j < pdfArray3.Count; j++)
					{
						if (pdfArray3[j] as PdfReferenceHolder != null)
						{
							pdfArray4.Add(new PdfReferenceHolder(PdfCrossTable.Dereference(pdfArray3[j]) as PdfDictionary));
						}
					}
					pdfDictionary3["Kids"] = pdfArray4;
				}
				pdfDictionary["Parent"] = new PdfReferenceHolder(pdfDictionary3);
			}
			if (pdfDictionary.ContainsKey("BS"))
			{
				PdfReferenceHolder pdfReferenceHolder = pdfDictionary["BS"] as PdfReferenceHolder;
				if (pdfReferenceHolder != null)
				{
					PdfDictionary obj2 = pdfReferenceHolder.Object as PdfDictionary;
					pdfDictionary["BS"] = new PdfReferenceHolder(obj2);
				}
			}
			if (pdfDictionary.ContainsKey("A") && pdfDictionary["A"] as PdfReferenceHolder != null)
			{
				pdfDictionary["A"] = new PdfReferenceHolder(PdfCrossTable.Dereference(pdfDictionary["A"]) as PdfDictionary);
			}
			if (pdfArray.Elements[i] as PdfReferenceHolder != null)
			{
				pdfArray2.Elements.Add(new PdfReferenceHolder(pdfDictionary));
			}
			else
			{
				pdfArray2.Elements.Add(pdfDictionary);
			}
		}
		if (m_pageDictionary["Annots"] as PdfReferenceHolder != null)
		{
			m_pageDictionary["Annots"] = new PdfReferenceHolder(pdfArray2);
		}
		else
		{
			m_pageDictionary["Annots"] = pdfArray2;
		}
	}

	internal void CheckAnnotAppearanceData(PdfDictionary annotAppearanceData)
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		foreach (PdfName key in annotAppearanceData.Keys)
		{
			PdfStream pdfStream = null;
			PdfDictionary pdfDictionary2 = null;
			if (annotAppearanceData[key] as PdfReferenceHolder != null)
			{
				pdfStream = PdfCrossTable.Dereference(annotAppearanceData[key]) as PdfStream;
				if (pdfStream == null)
				{
					pdfDictionary2 = PdfCrossTable.Dereference(annotAppearanceData[key]) as PdfDictionary;
				}
			}
			else if (annotAppearanceData[key] is PdfDictionary)
			{
				pdfDictionary2 = annotAppearanceData[key] as PdfDictionary;
			}
			if (pdfStream != null && pdfStream.ContainsKey("Resources"))
			{
				PdfDictionary xObjectData = pdfStream["Resources"] as PdfDictionary;
				ReInitializeXobjectResources(xObjectData);
				pdfDictionary.Items.Add(key, new PdfReferenceHolder(pdfStream));
			}
			if (pdfDictionary2 == null)
			{
				continue;
			}
			PdfDictionary pdfDictionary3 = new PdfDictionary();
			foreach (PdfName key2 in pdfDictionary2.Keys)
			{
				PdfStream pdfStream2 = PdfCrossTable.Dereference(pdfDictionary2[key2]) as PdfStream;
				if (pdfStream2 != null && pdfStream2.ContainsKey("Resources"))
				{
					PdfDictionary xObjectData2 = pdfStream2["Resources"] as PdfDictionary;
					ReInitializeXobjectResources(xObjectData2);
				}
				pdfDictionary3.Items.Add(key2, new PdfReferenceHolder(pdfStream2));
			}
			pdfDictionary.Items.Add(key, pdfDictionary3);
		}
		foreach (PdfName key3 in pdfDictionary.Keys)
		{
			if (annotAppearanceData[key3] as PdfReferenceHolder != null)
			{
				annotAppearanceData[key3] = new PdfReferenceHolder(pdfDictionary[key3]);
			}
			else
			{
				annotAppearanceData[key3] = pdfDictionary[key3];
			}
		}
	}

	internal void InitializeAcroformReference(PdfDictionary acroFormData)
	{
		if (acroFormData.ContainsKey("DR"))
		{
			PdfDictionary pdfDictionary = null;
			pdfDictionary = ((!(acroFormData["DR"] as PdfReferenceHolder != null)) ? (acroFormData["DR"] as PdfDictionary) : (PdfCrossTable.Dereference(acroFormData["DR"]) as PdfDictionary));
			if (pdfDictionary.ContainsKey("Font"))
			{
				PdfDictionary pdfDictionary2 = pdfDictionary["Font"] as PdfDictionary;
				PdfDictionary pdfDictionary3 = new PdfDictionary();
				foreach (PdfName key in pdfDictionary2.Keys)
				{
					PdfDictionary pdfDictionary4 = PdfCrossTable.Dereference(pdfDictionary2[key]) as PdfDictionary;
					if (pdfDictionary4 != null)
					{
						CheckFontInternalReference(pdfDictionary4);
					}
					pdfDictionary3.Items.Add(key, new PdfReferenceHolder(pdfDictionary4));
				}
				foreach (PdfName key2 in pdfDictionary3.Keys)
				{
					pdfDictionary2[key2] = pdfDictionary3[key2];
				}
			}
			if (pdfDictionary.ContainsKey("Encoding"))
			{
				PdfDictionary pdfDictionary5 = null;
				pdfDictionary5 = ((!(pdfDictionary["Encoding"] as PdfReferenceHolder != null)) ? (pdfDictionary["Encoding"] as PdfDictionary) : (PdfCrossTable.Dereference(pdfDictionary["Encoding"]) as PdfDictionary));
				PdfDictionary pdfDictionary6 = new PdfDictionary();
				foreach (PdfName key3 in pdfDictionary5.Keys)
				{
					if (pdfDictionary5[key3] as PdfReferenceHolder != null)
					{
						pdfDictionary6.Items.Add(key3, new PdfReferenceHolder(PdfCrossTable.Dereference(pdfDictionary5[key3]) as PdfDictionary));
					}
				}
				foreach (PdfName key4 in pdfDictionary6.Keys)
				{
					pdfDictionary5[key4] = pdfDictionary6[key4];
				}
			}
		}
		if (acroFormData.ContainsKey("Fields"))
		{
			PdfArray pdfArray = null;
			pdfArray = ((!(acroFormData["Fields"] as PdfReferenceHolder != null)) ? (acroFormData["Fields"] as PdfArray) : (PdfCrossTable.Dereference(acroFormData["Fields"]) as PdfArray));
			PdfArray pdfArray2 = new PdfArray();
			for (int i = 0; i < pdfArray.Count; i++)
			{
				if (pdfArray[i] as PdfReferenceHolder != null)
				{
					pdfArray2.Add(new PdfReferenceHolder(PdfCrossTable.Dereference(pdfArray[i]) as PdfDictionary));
				}
			}
			if (acroFormData["Fields"] as PdfReferenceHolder != null)
			{
				acroFormData["Fields"] = new PdfReferenceHolder(pdfArray2);
			}
			else
			{
				acroFormData["Fields"] = pdfArray2;
			}
		}
		if (!acroFormData.ContainsKey("XFA"))
		{
			return;
		}
		PdfArray pdfArray3 = acroFormData["XFA"] as PdfArray;
		for (int j = 0; j < pdfArray3.Count; j++)
		{
			if (pdfArray3[j] as PdfReferenceHolder != null && (pdfArray3[j] as PdfReferenceHolder).Object is PdfStream obj)
			{
				pdfArray3.RemoveAt(j);
				pdfArray3.Elements.Insert(j, new PdfReferenceHolder(obj));
			}
		}
	}

	internal void ReInitializeThumbnail()
	{
		PdfStream pdfStream = null;
		pdfStream = ((!(m_pageDictionary["Thumb"] as PdfReferenceHolder != null)) ? (m_pageDictionary["Thumb"] as PdfStream) : (PdfCrossTable.Dereference(m_pageDictionary["Thumb"]) as PdfStream));
		if (pdfStream.ContainsKey("ColorSpace"))
		{
			PdfArray pdfArray = null;
			pdfArray = ((!(pdfStream["ColorSpace"] as PdfReferenceHolder != null)) ? (pdfStream["ColorSpace"] as PdfArray) : (PdfCrossTable.Dereference(pdfStream["ColorSpace"]) as PdfArray));
			ReinitializeColorSpace(pdfArray);
			if (pdfStream["ColorSpace"] as PdfReferenceHolder != null)
			{
				pdfStream["ColorSpace"] = new PdfReferenceHolder(pdfArray);
			}
			else
			{
				pdfStream["ColorSpace"] = pdfArray;
			}
		}
		if (m_pageDictionary["Thumb"] as PdfReferenceHolder != null)
		{
			m_pageDictionary["Thumb"] = new PdfReferenceHolder(pdfStream);
		}
		else
		{
			m_pageDictionary["Thumb"] = pdfStream;
		}
	}

	internal void ReinitializeColorSpace(PdfArray colorSpaceCollection)
	{
		for (int i = 0; i < colorSpaceCollection.Count; i++)
		{
			if (!(colorSpaceCollection[i] as PdfReferenceHolder != null))
			{
				continue;
			}
			PdfReferenceHolder pdfReferenceHolder = colorSpaceCollection[i] as PdfReferenceHolder;
			if (pdfReferenceHolder.Object is PdfArray)
			{
				ReinitializeColorSpace(pdfReferenceHolder.Object as PdfArray);
				colorSpaceCollection.RemoveAt(i);
				colorSpaceCollection.Elements.Insert(i, new PdfReferenceHolder(pdfReferenceHolder.Object));
				continue;
			}
			if (pdfReferenceHolder.Object is PdfStream obj)
			{
				pdfReferenceHolder = new PdfReferenceHolder(obj);
			}
			colorSpaceCollection.RemoveAt(i);
			colorSpaceCollection.Elements.Insert(i, pdfReferenceHolder);
		}
	}

	internal void ReinitializeColorSpaceItem(PdfDictionary colorSpaceItems)
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		foreach (PdfName key in colorSpaceItems.Keys)
		{
			if (PdfCrossTable.Dereference(colorSpaceItems[key]) is PdfArray pdfArray)
			{
				ReinitializeColorSpace(pdfArray);
				if (colorSpaceItems[key] as PdfReferenceHolder != null)
				{
					pdfDictionary[key] = new PdfReferenceHolder(pdfArray);
				}
				else
				{
					pdfDictionary[key] = pdfArray;
				}
			}
		}
		foreach (PdfName key2 in pdfDictionary.Keys)
		{
			colorSpaceItems[key2] = pdfDictionary[key2];
		}
	}

	internal void ReInitializePagePatterns(PdfDictionary pagePattern)
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		foreach (PdfName key in pagePattern.Keys)
		{
			PdfDictionary pdfDictionary2 = PdfCrossTable.Dereference(pagePattern[key]) as PdfDictionary;
			if (pdfDictionary2 != null && pdfDictionary2.ContainsKey("Shading") && PdfCrossTable.Dereference(pdfDictionary2["Shading"]) is PdfDictionary pdfDictionary3 && pdfDictionary3.ContainsKey("Function"))
			{
				if (PdfCrossTable.Dereference(pdfDictionary3["Function"]) is PdfStream obj)
				{
					pdfDictionary3["Function"] = new PdfReferenceHolder(obj);
				}
				pdfDictionary2["Shading"] = new PdfReferenceHolder(pdfDictionary3);
			}
			if (pdfDictionary2 != null && pdfDictionary2.ContainsKey("Resources"))
			{
				PdfDictionary pdfDictionary4 = PdfCrossTable.Dereference(pdfDictionary2["Resources"]) as PdfDictionary;
				if (pdfDictionary4 != null)
				{
					ReInitializeXobjectResources(pdfDictionary4);
				}
				if (pdfDictionary2["Resources"] as PdfReferenceHolder != null)
				{
					pdfDictionary2["Resources"] = new PdfReferenceHolder(pdfDictionary4);
				}
				else
				{
					pdfDictionary2["Resources"] = pdfDictionary4;
				}
			}
			pdfDictionary[key] = new PdfReferenceHolder(pdfDictionary2);
		}
		foreach (PdfName key2 in pdfDictionary.Keys)
		{
			pagePattern[key2] = pdfDictionary[key2];
		}
	}

	internal void CheckFontInternalReference(PdfDictionary fontDictionary)
	{
		if (fontDictionary.ContainsKey("DescendantFonts"))
		{
			PdfArray pdfArray = PdfCrossTable.Dereference(fontDictionary["DescendantFonts"]) as PdfArray;
			PdfArray pdfArray2 = new PdfArray();
			if (pdfArray != null)
			{
				for (int i = 0; i < pdfArray.Count; i++)
				{
					if (!(PdfCrossTable.Dereference(pdfArray[i]) is PdfDictionary pdfDictionary))
					{
						continue;
					}
					PdfDictionary pdfDictionary2 = ReInitializeFontDescriptor(pdfDictionary);
					foreach (PdfName key in pdfDictionary2.Keys)
					{
						if (pdfDictionary.ContainsKey(key))
						{
							pdfDictionary[key] = pdfDictionary2[key];
						}
					}
					pdfArray2.Elements.Add(new PdfReferenceHolder(pdfDictionary));
				}
			}
			fontDictionary["DescendantFonts"] = pdfArray2;
		}
		if (fontDictionary.ContainsKey("FontDescriptor") && PdfCrossTable.Dereference(fontDictionary["FontDescriptor"]) is PdfDictionary pdfDictionary3)
		{
			PdfDictionary pdfDictionary4 = ReInitializeFontDescriptor(pdfDictionary3);
			foreach (PdfName key2 in pdfDictionary4.Keys)
			{
				if (pdfDictionary3.ContainsKey(key2))
				{
					pdfDictionary3[key2] = pdfDictionary4[key2];
				}
			}
			fontDictionary["FontDescriptor"] = new PdfReferenceHolder(pdfDictionary3);
		}
		if (fontDictionary.ContainsKey("Widths") && fontDictionary["Widths"] as PdfReferenceHolder != null && (fontDictionary["Widths"] as PdfReferenceHolder).Object is PdfArray obj)
		{
			fontDictionary["Widths"] = new PdfReferenceHolder(obj);
		}
		if (fontDictionary.ContainsKey("ToUnicode") && fontDictionary["ToUnicode"] as PdfReferenceHolder != null && PdfCrossTable.Dereference(fontDictionary["ToUnicode"]) is PdfStream obj2)
		{
			fontDictionary["ToUnicode"] = new PdfReferenceHolder(obj2);
		}
		if (!fontDictionary.ContainsKey("Encoding") || !(fontDictionary["Encoding"] as PdfReferenceHolder != null))
		{
			return;
		}
		if (PdfCrossTable.Dereference(fontDictionary["Encoding"]) is PdfDictionary pdfDictionary5)
		{
			if (pdfDictionary5.ContainsKey("Differences") && pdfDictionary5["Differences"] as PdfReferenceHolder != null)
			{
				if (PdfCrossTable.Dereference(pdfDictionary5["Differences"]) is PdfDictionary obj3)
				{
					pdfDictionary5["Differences"] = new PdfReferenceHolder(obj3);
				}
				else if (PdfCrossTable.Dereference(pdfDictionary5["Differences"]) is PdfArray)
				{
					pdfDictionary5["Differences"] = new PdfReferenceHolder(PdfCrossTable.Dereference(pdfDictionary5["Differences"]));
				}
			}
			fontDictionary["Encoding"] = new PdfReferenceHolder(pdfDictionary5);
		}
		else
		{
			fontDictionary["Encoding"] = new PdfReferenceHolder(PdfCrossTable.Dereference(fontDictionary["Encoding"]));
		}
	}

	internal PdfDictionary ReInitializeFontDescriptor(PdfDictionary fontDictionary)
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		foreach (PdfName key in fontDictionary.Keys)
		{
			if (!(fontDictionary[key] as PdfReferenceHolder != null))
			{
				continue;
			}
			PdfDictionary pdfDictionary2 = PdfCrossTable.Dereference(fontDictionary[key]) as PdfDictionary;
			PdfDictionary pdfDictionary3 = new PdfDictionary();
			if (pdfDictionary2 != null)
			{
				foreach (PdfName key2 in pdfDictionary2.Keys)
				{
					if (pdfDictionary2[key2] as PdfReferenceHolder != null)
					{
						PdfReferenceHolder pdfReferenceHolder = pdfDictionary2[key2] as PdfReferenceHolder;
						if (pdfReferenceHolder.Object is PdfNumber)
						{
							pdfDictionary3[key2] = new PdfReferenceHolder(pdfReferenceHolder.Object as PdfNumber);
						}
						else if (pdfReferenceHolder.Object is PdfStream obj)
						{
							pdfDictionary3[key2] = new PdfReferenceHolder(obj);
						}
					}
				}
				foreach (PdfName key3 in pdfDictionary3.Keys)
				{
					if (pdfDictionary2.ContainsKey(key3))
					{
						pdfDictionary2[key3] = pdfDictionary3[key3];
					}
				}
				pdfDictionary[key] = new PdfReferenceHolder(pdfDictionary2);
			}
			else if (pdfDictionary2 == null && key.Value == "W" && PdfCrossTable.Dereference(fontDictionary[key]) is PdfArray obj2)
			{
				pdfDictionary[key] = new PdfReferenceHolder(obj2);
			}
		}
		return pdfDictionary;
	}

	internal PdfDictionary ReInitializeExtGState(PdfDictionary extStateData)
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		foreach (PdfName key in extStateData.Keys)
		{
			PdfDictionary pdfDictionary2 = null;
			pdfDictionary2 = ((!(extStateData[key] as PdfReferenceHolder != null)) ? (extStateData[key] as PdfDictionary) : (PdfCrossTable.Dereference(extStateData[key]) as PdfDictionary));
			if (pdfDictionary2.ContainsKey("SMask") && pdfDictionary2["SMask"] as PdfReferenceHolder != null)
			{
				pdfDictionary2["SMask"] = new PdfReferenceHolder(PdfCrossTable.Dereference(pdfDictionary2["SMask"]) as PdfDictionary);
			}
			PdfDictionary pdfDictionary3 = new PdfDictionary();
			foreach (PdfName key2 in pdfDictionary2.Keys)
			{
				if (pdfDictionary2[key2] as PdfReferenceHolder != null && PdfCrossTable.Dereference(pdfDictionary2[key2]) is PdfStream obj)
				{
					pdfDictionary3.Items.Add(key2, new PdfReferenceHolder(obj));
				}
			}
			foreach (PdfName key3 in pdfDictionary3.Keys)
			{
				pdfDictionary2[key3] = pdfDictionary3[key3];
			}
			pdfDictionary.Items.Add(key, new PdfReferenceHolder(pdfDictionary2));
		}
		return pdfDictionary;
	}

	internal void ReInitializeXobjectResources(PdfDictionary xObjectData)
	{
		if (xObjectData.ContainsKey("Font") && PdfCrossTable.Dereference(xObjectData["Font"]) is PdfDictionary pdfDictionary)
		{
			PdfDictionary pdfDictionary2 = new PdfDictionary();
			foreach (PdfName key in pdfDictionary.Keys)
			{
				if (PdfCrossTable.Dereference(pdfDictionary[key]) is PdfDictionary pdfDictionary3)
				{
					CheckFontInternalReference(pdfDictionary3);
					pdfDictionary2.Items.Add(key, pdfDictionary3);
				}
			}
			foreach (PdfName key2 in pdfDictionary2.Keys)
			{
				if (pdfDictionary.ContainsKey(key2))
				{
					pdfDictionary[key2] = new PdfReferenceHolder(pdfDictionary2[key2]);
				}
			}
			if (xObjectData["Font"] as PdfReferenceHolder != null)
			{
				xObjectData["Font"] = new PdfReferenceHolder(pdfDictionary);
			}
			else
			{
				xObjectData["Font"] = pdfDictionary;
			}
		}
		if (xObjectData.ContainsKey("ExtGState") && PdfCrossTable.Dereference(xObjectData["ExtGState"]) is PdfDictionary extStateData)
		{
			PdfDictionary value = ReInitializeExtGState(extStateData);
			xObjectData["ExtGState"] = value;
		}
		if (xObjectData.ContainsKey("Properties") && PdfCrossTable.Dereference(xObjectData["Properties"]) is PdfDictionary pdfDictionary4)
		{
			PdfDictionary pdfDictionary5 = new PdfDictionary();
			foreach (PdfName key3 in pdfDictionary4.Keys)
			{
				PdfDictionary obj = PdfCrossTable.Dereference(pdfDictionary4[key3]) as PdfDictionary;
				pdfDictionary5.Items.Add(key3, new PdfReferenceHolder(obj));
			}
			xObjectData["Properties"] = pdfDictionary5;
		}
		if (xObjectData.ContainsKey("XObject") && PdfCrossTable.Dereference(xObjectData["XObject"]) is PdfDictionary xObjectDictionary)
		{
			PdfDictionary value2 = CheckTypeOfXObject(xObjectDictionary);
			xObjectData["XObject"] = value2;
		}
		if (xObjectData.ContainsKey("Pattern"))
		{
			IPdfPrimitive pdfPrimitive = xObjectData["Pattern"];
			if (pdfPrimitive is PdfDictionary)
			{
				ReInitializePagePatterns(pdfPrimitive as PdfDictionary);
			}
		}
		if (!xObjectData.ContainsKey("ColorSpace") || !(PdfCrossTable.Dereference(xObjectData["ColorSpace"]) is PdfDictionary pdfDictionary6))
		{
			return;
		}
		PdfDictionary pdfDictionary7 = new PdfDictionary();
		foreach (PdfName key4 in pdfDictionary6.Keys)
		{
			if (pdfDictionary6[key4] as PdfReferenceHolder != null && PdfCrossTable.Dereference(pdfDictionary6[key4]) is PdfArray pdfArray)
			{
				ReinitializeColorSpace(pdfArray);
				pdfDictionary7[key4] = new PdfReferenceHolder(pdfArray);
			}
		}
		xObjectData["ColorSpace"] = pdfDictionary7;
	}

	private PdfPageOrientation ObtainOrientation()
	{
		if (!(Size.Width > Size.Height))
		{
			return PdfPageOrientation.Portrait;
		}
		return PdfPageOrientation.Landscape;
	}

	internal virtual void Clear()
	{
		if (m_layers != null)
		{
			m_layers.Clear();
		}
		m_layers = null;
		m_resources = null;
		m_pageDictionary = null;
		m_annotations = null;
		m_fontNames = null;
		m_fontReference = null;
		if (m_contentTemplate != null)
		{
			m_contentTemplate = null;
		}
		if (m_mcidAndTextElements != null)
		{
			m_mcidAndTextElements.Clear();
		}
		if (m_abbreviationCollection != null)
		{
			m_abbreviationCollection.Clear();
		}
		if (m_mcidAndFigureBounds != null)
		{
			m_mcidAndFigureBounds.Clear();
		}
	}

	internal void ImportAnnotations(PdfLoadedDocument ldDoc, PdfPageBase page, List<PdfArray> destinations)
	{
		if (ldDoc.Security.EncryptOnlyAttachment)
		{
			_ = page.Annotations;
		}
		PdfArray pdfArray = page.ObtainAnnotations();
		if (pdfArray == null)
		{
			return;
		}
		Dictionary<long, PdfDictionary> dictionary = new Dictionary<long, PdfDictionary>();
		PdfArray pdfArray2 = new PdfArray();
		if (Dictionary.ContainsKey("Annots"))
		{
			pdfArray2 = Dictionary["Annots"] as PdfArray;
		}
		else
		{
			Dictionary["Annots"] = pdfArray2;
		}
		foreach (IPdfPrimitive item in pdfArray)
		{
			if (item is PdfNull)
			{
				continue;
			}
			PdfDictionary pdfDictionary = PdfCrossTable.Dereference(item) as PdfDictionary;
			bool flag = false;
			flag = ((!(this is PdfPage)) ? (this as PdfLoadedPage).Document.EnableMemoryOptimization : (this as PdfPage).Section.ParentDocument.EnableMemoryOptimization);
			if (flag && pdfDictionary != null)
			{
				if (ldDoc.Form != null && pdfDictionary.ContainsKey("Subtype") && (pdfDictionary["Subtype"] as PdfName).Value == "Widget" && ldDoc.Form.Fields.Count > 0)
				{
					m_fieldCount++;
					continue;
				}
				m_modified = true;
				PdfDictionary pdfDictionary2 = new PdfDictionary(pdfDictionary);
				PdfArray pdfArray3 = null;
				if (pdfDictionary2.ContainsKey("Dest"))
				{
					pdfArray3 = GetDestination(ldDoc, pdfDictionary2);
				}
				pdfDictionary2.Remove("Dest");
				if (pdfDictionary2.ContainsKey("A") && pdfDictionary2["A"] is PdfReferenceHolder)
				{
					((pdfDictionary2["A"] as PdfReferenceHolder).Object as PdfDictionary).Remove("AN");
				}
				pdfDictionary2.Remove("Popup");
				pdfDictionary2.Remove("P");
				PdfName pdfName = PdfCrossTable.Dereference(pdfDictionary2["Subtype"]) as PdfName;
				if ((pdfDictionary2.ContainsKey("Parent") && !FindAcroFromFieldsCount(ldDoc)) || (pdfName != null && pdfName.Value != "Widget" && pdfDictionary2.ContainsKey("Parent")))
				{
					pdfDictionary2.Remove("Parent");
				}
				PdfCrossTable pdfCrossTable = null;
				pdfCrossTable = ((!(this is PdfPage)) ? (this as PdfLoadedPage).Document.CrossTable : (this as PdfPage).Section.ParentDocument.CrossTable);
				if (pdfDictionary2.ContainsKey("AP") && PdfCrossTable.Dereference(pdfDictionary2["AP"]) is PdfDictionary pdfDictionary3 && pdfDictionary3.ContainsKey("N") && PdfCrossTable.Dereference(pdfDictionary3["N"]) is PdfDictionary pdfDictionary4 && !pdfDictionary4.ContainsKey("Subtype") && pdfDictionary4.ContainsKey("Root"))
				{
					pdfDictionary3.Remove("N");
					pdfDictionary4.Clear();
					PdfDictionary pdfDictionary5 = null;
					pdfDictionary2["AP"] = new PdfDictionary();
				}
				PdfDictionary pdfDictionary6 = pdfDictionary2.Clone(pdfCrossTable) as PdfDictionary;
				PdfReferenceHolder primitive = new PdfReferenceHolder(this);
				pdfDictionary6.SetProperty("P", primitive);
				pdfArray2.Add(new PdfReferenceHolder(pdfDictionary6));
				if (page.IsTagged)
				{
					PdfReferenceHolder pdfReferenceHolder = item as PdfReferenceHolder;
					if (pdfReferenceHolder != null && pdfReferenceHolder.Reference != null)
					{
						ImportedAnnotationReference[pdfReferenceHolder.Reference.ObjNum] = pdfDictionary6;
					}
				}
				AddFieldParent(pdfDictionary6, ldDoc);
				if (pdfArray3 != null)
				{
					pdfDictionary6["Dest"] = pdfArray3.Clone(pdfCrossTable);
				}
			}
			else
			{
				if (pdfDictionary == null)
				{
					continue;
				}
				PdfDictionary pdfDictionary7 = new PdfDictionary(pdfDictionary);
				if (pdfDictionary7.ContainsKey("AP") && PdfCrossTable.Dereference(pdfDictionary7["AP"]) is PdfDictionary pdfDictionary8 && pdfDictionary8.ContainsKey("N") && PdfCrossTable.Dereference(pdfDictionary8["N"]) is PdfDictionary pdfDictionary9 && !pdfDictionary9.ContainsKey("Subtype") && pdfDictionary9.ContainsKey("Root"))
				{
					pdfDictionary8.Remove("N");
					pdfDictionary9.Clear();
					PdfDictionary pdfDictionary10 = null;
					pdfDictionary7["AP"] = new PdfDictionary();
				}
				pdfDictionary7.SetProperty("P", new PdfReferenceHolder(this));
				pdfArray2.Add(new PdfReferenceHolder(pdfDictionary7));
				if (page.IsTagged)
				{
					PdfReferenceHolder pdfReferenceHolder2 = item as PdfReferenceHolder;
					if (pdfReferenceHolder2 != null && pdfReferenceHolder2.Reference != null)
					{
						ImportedAnnotationReference[pdfReferenceHolder2.Reference.ObjNum] = pdfDictionary7;
					}
				}
				if (pdfDictionary7.ContainsKey("IRT"))
				{
					PdfReferenceHolder pdfReferenceHolder3 = pdfDictionary7["IRT"] as PdfReferenceHolder;
					if (pdfReferenceHolder3 != null && pdfReferenceHolder3.Reference != null && dictionary.ContainsKey(pdfReferenceHolder3.Reference.ObjNum))
					{
						PdfDictionary obj = dictionary[pdfReferenceHolder3.Reference.ObjNum];
						pdfDictionary7["IRT"] = new PdfReferenceHolder(obj);
					}
				}
				PdfReferenceHolder pdfReferenceHolder4 = item as PdfReferenceHolder;
				if (pdfReferenceHolder4 != null && pdfReferenceHolder4.Reference != null)
				{
					dictionary[pdfReferenceHolder4.Reference.ObjNum] = pdfDictionary7;
				}
				AddFieldParent(pdfDictionary7, ldDoc);
				if (pdfDictionary7.ContainsKey("A") && (pdfDictionary7["A"] is PdfReferenceHolder || pdfDictionary7["A"] is PdfDictionary))
				{
					PdfDictionary pdfDictionary11 = null;
					if (pdfDictionary7["A"] is PdfReferenceHolder)
					{
						pdfDictionary11 = (pdfDictionary7["A"] as PdfReferenceHolder).Object as PdfDictionary;
					}
					else if (pdfDictionary7["A"] is PdfDictionary)
					{
						pdfDictionary11 = pdfDictionary7["A"] as PdfDictionary;
					}
					if (ldDoc.IsEncrypted && pdfDictionary11.ContainsKey("URI"))
					{
						PdfString pdfString = null;
						if (pdfDictionary11["URI"] is PdfString)
						{
							pdfString = PdfCrossTable.Dereference(pdfDictionary11["URI"]) as PdfString;
						}
						if (pdfString != null && item is PdfReferenceHolder)
						{
							PdfReferenceHolder pdfReferenceHolder5 = item as PdfReferenceHolder;
							if (pdfReferenceHolder5 != null && pdfReferenceHolder5.Reference != null)
							{
								pdfString.Decrypt(ldDoc.CrossTable.Encryptor, pdfReferenceHolder5.Reference.ObjNum);
							}
						}
					}
					pdfDictionary11.Remove("AN");
					if (pdfDictionary11["D"] is PdfArray pdfArray4)
					{
						for (int i = 0; i < pdfArray4.Count; i++)
						{
							IPdfPrimitive pdfPrimitive2 = pdfArray4[i];
							if (pdfPrimitive2 is PdfReferenceHolder && (pdfPrimitive2 as PdfReferenceHolder).Object is PdfDictionary pdfDictionary12)
							{
								PdfLoadedPage page2 = ldDoc.Pages.GetPage(pdfDictionary12) as PdfLoadedPage;
								int num = ldDoc.Pages.IndexOf(page2);
								if ((page.importPageStartIndex == page.importPageEndIndex || num > page.importPageEndIndex) && pdfDictionary12 != null && pdfDictionary12.ContainsKey("Type") && pdfDictionary12["Type"].ToString().ToLower().Contains("page"))
								{
									pdfArray4.Remove(pdfPrimitive2);
									pdfArray4.Insert(i, new PdfNull());
									break;
								}
							}
						}
					}
				}
				if (pdfDictionary7.ContainsKey("Dest"))
				{
					PdfArray destination = GetDestination(ldDoc, pdfDictionary7);
					if (destination != null)
					{
						destinations.Add(destination);
						pdfDictionary7["Dest"] = destination;
					}
				}
				else if (pdfDictionary7.ContainsKey("Popup"))
				{
					PdfDictionary pdfDictionary13 = (pdfDictionary7.Items[new PdfName("Popup")] as PdfReferenceHolder).Object as PdfDictionary;
					if (pdfDictionary13.ContainsKey("Parent"))
					{
						pdfDictionary13.Items.Remove(new PdfName("Parent"));
					}
				}
			}
		}
		dictionary.Clear();
	}

	private bool FindAcroFromFieldsCount(PdfLoadedDocument loadedDocument)
	{
		bool result = true;
		if (loadedDocument.Catalog != null && loadedDocument.Catalog.ContainsKey("AcroForm") && PdfCrossTable.Dereference(loadedDocument.Catalog["AcroForm"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("Fields") && PdfCrossTable.Dereference(pdfDictionary["Fields"]) is PdfArray { Count: not 0 })
		{
			result = false;
		}
		return result;
	}

	private void AddFieldParent(PdfDictionary annot, PdfLoadedDocument loadedDocument)
	{
		if (annot.ContainsKey("Parent") && FindAcroFromFieldsCount(loadedDocument) && PdfCrossTable.Dereference(annot["Parent"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("Kids") && PdfCrossTable.Dereference(pdfDictionary["Kids"]) is PdfArray { Count: 1 } pdfArray)
		{
			pdfArray.Clear();
			pdfArray.Add(new PdfReferenceHolder(annot));
		}
	}

	internal void ImportAnnotations(PdfLoadedDocument ldDoc, PdfPageBase page)
	{
		PdfArray pdfArray = page.ObtainAnnotations();
		if (pdfArray == null)
		{
			return;
		}
		PdfArray pdfArray2 = new PdfArray();
		PdfReferenceHolder primitive = new PdfReferenceHolder(this);
		Dictionary["Annots"] = pdfArray2;
		m_modified = true;
		foreach (IPdfPrimitive item in pdfArray)
		{
			PdfDictionary pdfDictionary = new PdfDictionary(PdfCrossTable.Dereference(item) as PdfDictionary);
			pdfDictionary.SetProperty("P", primitive);
			pdfArray2.Add(new PdfReferenceHolder(pdfDictionary));
		}
	}

	private PdfArray GetDestination(PdfLoadedDocument ldDoc, PdfDictionary annotation)
	{
		IPdfPrimitive pdfPrimitive = PdfCrossTable.Dereference(annotation["Dest"]);
		PdfName pdfName = pdfPrimitive as PdfName;
		PdfString pdfString = pdfPrimitive as PdfString;
		PdfArray pdfArray = pdfPrimitive as PdfArray;
		if (ldDoc.Catalog.Destinations != null)
		{
			if (pdfName != null)
			{
				pdfArray = ldDoc.GetNamedDestination(pdfName);
			}
			else if (pdfString != null)
			{
				pdfArray = ldDoc.GetNamedDestination(pdfString);
			}
		}
		if (pdfArray != null)
		{
			pdfArray = new PdfArray(pdfArray);
		}
		return pdfArray;
	}

	internal PdfArray ObtainAnnotations()
	{
		IPdfPrimitive value = Dictionary.GetValue("Annots", "Parent");
		PdfReferenceHolder pdfReferenceHolder = value as PdfReferenceHolder;
		if (pdfReferenceHolder != null)
		{
			return pdfReferenceHolder.Object as PdfArray;
		}
		return value as PdfArray;
	}

	private int GetAnnotationCount()
	{
		if (m_annotations != null)
		{
			return m_annotations.Count;
		}
		return 0;
	}

	internal PdfFormFieldsTabOrder ObtainTabOrder()
	{
		if (Dictionary.ContainsKey("Tabs"))
		{
			PdfName pdfName = PdfCrossTable.Dereference(Dictionary["Tabs"]) as PdfName;
			if (pdfName != null)
			{
				if (pdfName.Value == "R")
				{
					m_formFieldsTabOrder = PdfFormFieldsTabOrder.Row;
				}
				else if (pdfName.Value == "C")
				{
					m_formFieldsTabOrder = PdfFormFieldsTabOrder.Column;
				}
				else if (pdfName.Value == "S")
				{
					m_formFieldsTabOrder = PdfFormFieldsTabOrder.Structure;
				}
				else if (pdfName.Value == "W")
				{
					m_formFieldsTabOrder = PdfFormFieldsTabOrder.Widget;
				}
			}
		}
		return m_formFieldsTabOrder;
	}

	private PdfPageRotateAngle ObtainRotation()
	{
		int num = 90;
		PdfDictionary pdfDictionary = Dictionary;
		PdfNumber pdfNumber = null;
		while (pdfDictionary != null && pdfNumber == null)
		{
			if (pdfDictionary.ContainsKey("Rotate"))
			{
				pdfNumber = PdfCrossTable.Dereference(pdfDictionary["Rotate"]) as PdfNumber;
			}
			pdfDictionary = PdfCrossTable.Dereference(pdfDictionary["Parent"]) as PdfDictionary;
		}
		if (pdfNumber == null)
		{
			pdfNumber = new PdfNumber(0);
		}
		if (pdfNumber.IntValue < 0)
		{
			pdfNumber.IntValue = 360 + pdfNumber.IntValue;
		}
		return (PdfPageRotateAngle)(pdfNumber.IntValue / num);
	}

	private void DrawAnnotationTemplates(PdfGraphics g)
	{
		PdfArray pdfArray = ObtainAnnotations();
		if (pdfArray == null)
		{
			return;
		}
		foreach (IPdfPrimitive item in pdfArray)
		{
			PdfReferenceHolder pdfReferenceHolder = item as PdfReferenceHolder;
			PdfDictionary annotation = ((!(pdfReferenceHolder != null)) ? (item as PdfDictionary) : (pdfReferenceHolder.Object as PdfDictionary));
			PdfTemplate annotTemplate = GetAnnotTemplate(annotation);
			if (annotTemplate != null)
			{
				PointF annotationLocation = GetAnnotationLocation(annotation);
				annotationLocation = NormalizeAnnotationLocation(annotationLocation, g, annotTemplate);
				g.DrawPdfTemplate(annotTemplate, annotationLocation);
			}
		}
	}

	private PointF NormalizeAnnotationLocation(PointF location, PdfGraphics graphics, PdfTemplate template)
	{
		location.Y = graphics.Size.Height - location.Y - template.Height;
		return location;
	}

	private PointF GetAnnotationLocation(PdfDictionary annotation)
	{
		PdfArray obj = (PdfCrossTable.Dereference(annotation["Rect"]) as PdfArray) ?? throw new PdfDocumentException("Invalid format: annotation dictionary doesn't contain rectangle array.");
		if (obj.Count < 4)
		{
			throw new PdfDocumentException("Invalid format: annotation rectangle has less then four elements.");
		}
		PdfNumber pdfNumber = obj[0] as PdfNumber;
		PdfNumber pdfNumber2 = obj[1] as PdfNumber;
		PdfNumber pdfNumber3 = obj[2] as PdfNumber;
		PdfNumber pdfNumber4 = obj[3] as PdfNumber;
		float x = Math.Min(pdfNumber.FloatValue, pdfNumber3.FloatValue);
		float y = Math.Min(pdfNumber2.FloatValue, pdfNumber4.FloatValue);
		return new PointF(x, y);
	}

	private SizeF GetAnnotationSize(PdfDictionary annotation)
	{
		return GetElementSize(annotation, "Rect");
	}

	private SizeF GetElementSize(PdfDictionary dictionary, string propertyName)
	{
		PdfArray obj = (PdfCrossTable.Dereference(dictionary[propertyName]) as PdfArray) ?? throw new PdfDocumentException("Invalid format: dictionary doesn't contain rectangle array.");
		if (obj.Count < 4)
		{
			throw new PdfDocumentException("Invalid format: rectangle array has less then four elements.");
		}
		PdfNumber pdfNumber = obj[0] as PdfNumber;
		PdfNumber pdfNumber2 = obj[1] as PdfNumber;
		PdfNumber pdfNumber3 = obj[2] as PdfNumber;
		PdfNumber pdfNumber4 = obj[3] as PdfNumber;
		float width = Math.Abs(pdfNumber.FloatValue - pdfNumber3.FloatValue);
		float height = Math.Abs(pdfNumber2.FloatValue - pdfNumber4.FloatValue);
		return new SizeF(width, height);
	}

	private PdfTemplate GetAnnotTemplate(PdfDictionary annotation)
	{
		PdfDictionary pdfDictionary = PdfCrossTable.Dereference(annotation["AP"]) as PdfDictionary;
		PdfTemplate result = null;
		if (pdfDictionary != null && PdfCrossTable.Dereference(pdfDictionary["N"]) is PdfDictionary pdfDictionary2)
		{
			PdfStream pdfStream = pdfDictionary2 as PdfStream;
			if (pdfStream == null)
			{
				PdfName pdfName = null;
				if (annotation.ContainsKey("AS"))
				{
					pdfName = PdfCrossTable.Dereference(annotation["AS"]) as PdfName;
				}
				else
				{
					IEnumerator enumerator = pdfDictionary2.Keys.GetEnumerator();
					if (enumerator.MoveNext())
					{
						pdfName = enumerator.Current as PdfName;
					}
				}
				if (pdfName != null)
				{
					pdfStream = PdfCrossTable.Dereference(pdfDictionary2[pdfName]) as PdfStream;
				}
			}
			if (pdfStream != null)
			{
				PdfDictionary resources = PdfCrossTable.Dereference(pdfStream["Resources"]) as PdfDictionary;
				result = new PdfTemplate(GetElementSize(pdfStream, "BBox"), pdfStream.InternalStream, resources);
			}
		}
		return result;
	}

	internal PdfArray GetCropOrMediaBox(PdfPageBase page, PdfArray cropOrMediaBox)
	{
		if (page != null && page.Dictionary.ContainsKey("CropBox"))
		{
			cropOrMediaBox = PdfCrossTable.Dereference(page.Dictionary["CropBox"]) as PdfArray;
		}
		else if (page != null && page.Dictionary.ContainsKey("MediaBox"))
		{
			cropOrMediaBox = PdfCrossTable.Dereference(page.Dictionary["MediaBox"]) as PdfArray;
		}
		return cropOrMediaBox;
	}

	internal RectangleF GetCropOrMediaBoxBounds(RectangleF bounds, float x, float y)
	{
		RectangleF result = bounds;
		result.X = 0f - x - (0f - bounds.X);
		result.Y = y + bounds.Y;
		return result;
	}

	private RectangleF GetBounds(double pageRotation, bool isGlyphRotated, float xValue, float yValue, float Width, float Height)
	{
		if (!isGlyphRotated || (pageRotation != 270.0 && pageRotation != 90.0))
		{
			return new RectangleF(xValue, yValue, Width - xValue, Height);
		}
		return new RectangleF(xValue, yValue, Width, Height);
	}

	private void ExtGStateOptimization(PdfDictionary extGDictionary, PdfPageBase newPage)
	{
		Dictionary<PdfName, IPdfPrimitive> dictionary = new Dictionary<PdfName, IPdfPrimitive>();
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item in extGDictionary.Items)
		{
			dictionary.Add(item.Key, item.Value);
		}
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item2 in dictionary)
		{
			if (newPage.DestinationDocument == null)
			{
				continue;
			}
			if (newPage.DestinationDocument.ExtGstateCollection.ContainsKey(item2.Key.Value))
			{
				if (newPage.DestinationDocument.ExtGstateCollection[item2.Key.Value] is PdfReferenceHolder)
				{
					PdfReferenceHolder pdfReferenceHolder = newPage.DestinationDocument.ExtGstateCollection[item2.Key.Value] as PdfReferenceHolder;
					if (item2.Value is PdfReferenceHolder && pdfReferenceHolder.Reference == (item2.Value as PdfReferenceHolder).Reference)
					{
						extGDictionary.Items.Remove(item2.Key);
						extGDictionary.Items.Add(item2.Key, pdfReferenceHolder);
					}
				}
			}
			else
			{
				newPage.DestinationDocument.ExtGstateCollection.Add(item2.Key.Value, item2.Value);
			}
		}
	}

	private void PropertiseOptimization(PdfDictionary propDictionary, PdfPageBase newPage)
	{
		Dictionary<PdfName, IPdfPrimitive> dictionary = new Dictionary<PdfName, IPdfPrimitive>();
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item in propDictionary.Items)
		{
			dictionary.Add(item.Key, item.Value);
		}
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item2 in dictionary)
		{
			if (!(item2.Value is PdfReferenceHolder) || !(PdfCrossTable.Dereference(item2.Value) is PdfDictionary pdfDictionary) || pdfDictionary.Items.Count <= 0)
			{
				continue;
			}
			foreach (KeyValuePair<PdfName, IPdfPrimitive> item3 in pdfDictionary.Items)
			{
				if (!(item3.Value is PdfReferenceHolder) || !(PdfCrossTable.Dereference(item3.Value) is PdfDictionary pdfDictionary2) || !pdfDictionary2.ContainsKey("Subtype") || !pdfDictionary2.ContainsKey("Type"))
				{
					continue;
				}
				PdfName pdfName = PdfCrossTable.Dereference(pdfDictionary2["Subtype"]) as PdfName;
				PdfName pdfName2 = PdfCrossTable.Dereference(pdfDictionary2["Type"]) as PdfName;
				if (pdfName != null && pdfName.Value == "XML" && pdfName2.Value == "Metadata" && pdfDictionary2 is PdfStream pdfStream)
				{
					string hashValue = string.Empty;
					if (CompareStream(pdfStream.InternalStream, newPage, out hashValue))
					{
						PdfDictionary obj = newPage.DestinationDocument.ResourceCollection[hashValue] as PdfDictionary;
						propDictionary.Items.Remove(item2.Key);
						propDictionary.Items.Add(item2.Key, new PdfReferenceHolder(obj));
					}
					else if (hashValue != string.Empty)
					{
						newPage.DestinationDocument.ResourceCollection.Add(hashValue, pdfDictionary);
					}
				}
			}
		}
	}

	internal void RemoveIdeticalContentStreams(PdfArray pageContents, PdfPage newPage)
	{
		contentOptimize = true;
		foreach (PdfReferenceHolder pageContent in pageContents)
		{
			if (pageContent.Object is PdfStream stream)
			{
				AddContentElement(stream, pageContent, newPage);
			}
		}
		contentOptimize = false;
	}

	internal void AddContentElement(PdfStream stream, PdfReferenceHolder contentReferenceHolder, PdfPage newPage)
	{
		string hashValue = string.Empty;
		if (CompareStream(stream.InternalStream, newPage, out hashValue))
		{
			PdfReferenceHolder pdfReferenceHolder = newPage.DestinationDocument.ContentCollection[hashValue] as PdfReferenceHolder;
			PdfArray pdfArray = newPage.Dictionary["Contents"] as PdfArray;
			if (contentReferenceHolder != null && pdfReferenceHolder != null && contentReferenceHolder.Reference != null && pdfReferenceHolder.Reference != null)
			{
				if (contentReferenceHolder.Reference.ObjNum == pdfReferenceHolder.Reference.ObjNum)
				{
					pdfArray.Remove(contentReferenceHolder);
					pdfArray.Add(pdfReferenceHolder);
				}
			}
			else if (contentReferenceHolder.Reference == null && pdfReferenceHolder.Reference == null)
			{
				pdfArray.Remove(contentReferenceHolder);
				pdfArray.Add(pdfReferenceHolder);
			}
		}
		else if (hashValue != string.Empty)
		{
			newPage.DestinationDocument.ContentCollection.Add(hashValue, contentReferenceHolder);
		}
	}

	internal void RemoveIdenticalAnnotations(PdfArray annotArray, PdfPage newPage)
	{
		annotOptimize = true;
		PdfArray pdfArray = null;
		if (annotArray == null)
		{
			return;
		}
		PdfArray pdfArray2 = new PdfArray();
		foreach (PdfReferenceHolder item in annotArray)
		{
			pdfArray2.Add(item);
		}
		foreach (PdfReferenceHolder item2 in pdfArray2)
		{
			PdfDictionary pdfDictionary = item2.Object as PdfDictionary;
			if (!pdfDictionary.ContainsKey("FT"))
			{
				if (pdfDictionary == null || !pdfDictionary.ContainsKey("AP") || !(PdfCrossTable.Dereference(pdfDictionary["AP"]) is PdfDictionary pdfDictionary2) || !pdfDictionary2.ContainsKey("N") || !(PdfCrossTable.Dereference(pdfDictionary2["N"]) is PdfDictionary pdfDictionary3) || !pdfDictionary3.ContainsKey("Subtype"))
				{
					continue;
				}
				PdfName pdfName = PdfCrossTable.Dereference(pdfDictionary3["Subtype"]) as PdfName;
				if (!(pdfName != null) || !(pdfName.Value == "Form"))
				{
					continue;
				}
				PdfDictionary pdfDictionary4 = PdfCrossTable.Dereference(pdfDictionary3["Resources"]) as PdfDictionary;
				_ = pdfDictionary3["Resources"];
				if (pdfDictionary4 != null && pdfDictionary4.ContainsKey("XObject"))
				{
					PdfResources resources = new PdfResources(pdfDictionary4);
					RemoveidenticalAnnotationResource(resources, newPage, item2, pdfArray);
					continue;
				}
				PdfStream pdfStream = pdfDictionary3 as PdfStream;
				string hashValue = string.Empty;
				if (CompareStream(pdfStream.InternalStream, newPage, out hashValue))
				{
					PdfReferenceHolder element2 = item2;
					PdfReferenceHolder element3 = newPage.DestinationDocument.AnnotationCollection[hashValue] as PdfReferenceHolder;
					pdfArray = newPage.Dictionary["Annots"] as PdfArray;
					pdfArray.Remove(element2);
					pdfArray.Add(element3);
					annotArrayModified = true;
				}
				else if (hashValue != string.Empty)
				{
					newPage.DestinationDocument.AnnotationCollection.Add(hashValue, item2);
				}
			}
			else if (pdfDictionary.ContainsKey("T"))
			{
				PdfString pdfString = PdfCrossTable.Dereference(pdfDictionary["T"]) as PdfString;
				if (newPage.DestinationDocument.WidgetAnnotationCollection.ContainsKey(pdfString.Value))
				{
					PdfReferenceHolder element4 = item2;
					PdfReferenceHolder element5 = newPage.DestinationDocument.WidgetAnnotationCollection[pdfString.Value] as PdfReferenceHolder;
					pdfArray = newPage.Dictionary["Annots"] as PdfArray;
					pdfArray.Remove(element4);
					pdfArray.Add(element5);
					annotArrayModified = true;
				}
				else
				{
					newPage.DestinationDocument.WidgetAnnotationCollection.Add(pdfString.Value, item2);
				}
			}
		}
		if (pdfArray != null && annotArrayModified)
		{
			newPage.Dictionary["Annots"] = pdfArray;
			annotArrayModified = false;
		}
	}

	private void RemoveidenticalAnnotationResource(PdfResources resources, PdfPage newPage, PdfReferenceHolder annotReference, PdfArray annots)
	{
		if (!(PdfCrossTable.Dereference(resources["XObject"]) is PdfDictionary pdfDictionary))
		{
			return;
		}
		Dictionary<PdfName, IPdfPrimitive> dictionary = new Dictionary<PdfName, IPdfPrimitive>();
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item in pdfDictionary.Items)
		{
			dictionary.Add(item.Key, item.Value);
		}
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item2 in dictionary)
		{
			if (!(item2.Value is PdfReferenceHolder) || CheckRepeatedReference(item2.Value as PdfReferenceHolder) || !(PdfCrossTable.Dereference(item2.Value) is PdfDictionary pdfDictionary2) || !pdfDictionary2.ContainsKey("Subtype"))
			{
				continue;
			}
			PdfName pdfName = PdfCrossTable.Dereference(pdfDictionary2["Subtype"]) as PdfName;
			if (pdfName != null && pdfName.Value == "Image")
			{
				if (pdfDictionary2 is PdfStream stream)
				{
					AddAnnotationResourceCollection(stream, newPage, pdfDictionary2, pdfDictionary, item2.Key, annotReference, annots);
					resources.Remove("XObject");
					resources["XObject"] = pdfDictionary;
				}
			}
			else
			{
				if (!(pdfName != null) || !(pdfName.Value == "Form") || !pdfDictionary2.ContainsKey("Resources"))
				{
					continue;
				}
				PdfDictionary pdfDictionary3 = PdfCrossTable.Dereference(pdfDictionary2["Resources"]) as PdfDictionary;
				PdfReferenceHolder referenceHolder = pdfDictionary2["Resources"] as PdfReferenceHolder;
				bool flag = IsRepeatedResource(referenceHolder);
				if (!flag && pdfDictionary3 != null && (pdfDictionary3.ContainsKey("XObject") || pdfDictionary3.ContainsKey("Font")))
				{
					RemoveIdenticalResources(new PdfResources(pdfDictionary3), newPage);
					if (pdfDictionary2 is PdfStream stream2 && newPage.DestinationDocument != null && !newPage.DestinationDocument.m_isImported && newPage.templateResource)
					{
						AddAnnotationResourceCollection(stream2, newPage, pdfDictionary2, pdfDictionary, item2.Key, annotReference, annots);
					}
				}
				else if (!flag && pdfDictionary2 is PdfStream stream3)
				{
					AddAnnotationResourceCollection(stream3, newPage, pdfDictionary2, pdfDictionary, item2.Key, annotReference, annots);
				}
			}
		}
	}

	private void AddAnnotationResourceCollection(PdfStream stream, PdfPageBase newPage, PdfDictionary xObject, PdfDictionary xObjectDictionary, PdfName xObjectkey, PdfReferenceHolder annotReference, PdfArray annots)
	{
		string hashValue = string.Empty;
		if (CompareStream(stream.InternalStream, newPage, out hashValue))
		{
			PdfDictionary pdfDictionary = newPage.DestinationDocument.AnnotationCollection[hashValue] as PdfDictionary;
			PdfReferenceHolder element = newPage.DestinationDocument.AnnotationReferenceCollection[hashValue] as PdfReferenceHolder;
			annots = newPage.Dictionary["Annots"] as PdfArray;
			if (newPage.templateResource && !ValidateXObjectDictionary(pdfDictionary, xObject))
			{
				return;
			}
			if (xObject.ObjectCollectionIndex != pdfDictionary.ObjectCollectionIndex && !xObject.ContainsKey("SMask") && !pdfDictionary.ContainsKey("SMask"))
			{
				pdfPrimitivesCollection = new List<IPdfPrimitive>();
				RemoveFromDocument(xObject);
				pdfPrimitivesCollection.Clear();
				pdfPrimitivesCollection = null;
			}
			if (xObject == null)
			{
				return;
			}
			if (xObject != null && xObject.ContainsKey("SMask") && pdfDictionary.ContainsKey("SMask"))
			{
				PdfStream pdfStream = PdfCrossTable.Dereference(xObject["SMask"]) as PdfStream;
				PdfStream pdfStream2 = PdfCrossTable.Dereference(pdfDictionary["SMask"]) as PdfStream;
				if (pdfStream == null || pdfStream2 == null)
				{
					return;
				}
				pdfStream.InternalStream.Position = 0L;
				byte[] array = new byte[(int)pdfStream.InternalStream.Length];
				pdfStream.InternalStream.Read(array, 0, array.Length);
				pdfStream.InternalStream.Position = 0L;
				string text = newPage.DestinationDocument.CreateHashFromStream(array);
				pdfStream2.InternalStream.Position = 0L;
				array = new byte[(int)pdfStream2.InternalStream.Length];
				pdfStream2.InternalStream.Read(array, 0, array.Length);
				pdfStream2.InternalStream.Position = 0L;
				string text2 = newPage.DestinationDocument.CreateHashFromStream(array);
				if (!string.IsNullOrEmpty(text2) && !string.IsNullOrEmpty(text) && text == text2)
				{
					if (xObject.ObjectCollectionIndex != pdfDictionary.ObjectCollectionIndex)
					{
						pdfPrimitivesCollection = new List<IPdfPrimitive>();
						RemoveFromDocument(xObject);
						pdfPrimitivesCollection.Clear();
						pdfPrimitivesCollection = null;
					}
					xObjectDictionary.Items.Remove(xObjectkey);
					xObjectDictionary.Items.Add(xObjectkey, new PdfReferenceHolder(pdfDictionary));
					annots.Remove(annotReference);
					annots.Add(element);
					annotArrayModified = true;
				}
			}
			else if (xObject != null && !stream.ContainsKey("SMask") && !xObject.ContainsKey("SMask"))
			{
				xObjectDictionary.Items.Remove(xObjectkey);
				pdfPrimitivesCollection = new List<IPdfPrimitive>();
				AddToDocument(pdfDictionary);
				pdfPrimitivesCollection.Clear();
				pdfPrimitivesCollection = null;
				xObjectDictionary.Items.Add(xObjectkey, new PdfReferenceHolder(pdfDictionary));
				annots.Remove(annotReference);
				annots.Add(element);
				annotArrayModified = true;
			}
		}
		else if (hashValue != string.Empty)
		{
			newPage.DestinationDocument.AnnotationCollection.Add(hashValue, xObject);
			newPage.DestinationDocument.AnnotationReferenceCollection.Add(hashValue, annotReference);
		}
	}

	private void RemoveFromDocument(PdfDictionary dictionary)
	{
		dictionary.isSkip = true;
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item in dictionary.Items)
		{
			if (PdfCrossTable.Dereference(item.Value) is PdfDictionary pdfDictionary && !IsRepeatedEntry(pdfDictionary))
			{
				RemoveFromDocument(pdfDictionary);
			}
		}
	}

	private void AddToDocument(PdfDictionary dictionary)
	{
		dictionary.isSkip = false;
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item in dictionary.Items)
		{
			if (PdfCrossTable.Dereference(item.Value) is PdfDictionary pdfDictionary && !IsRepeatedEntry(pdfDictionary))
			{
				AddToDocument(pdfDictionary);
			}
		}
	}

	private bool IsRepeatedEntry(IPdfPrimitive primitive)
	{
		if (pdfPrimitivesCollection.Contains(primitive))
		{
			return true;
		}
		pdfPrimitivesCollection.Add(primitive);
		return false;
	}

	private bool CheckRepeatedReference(PdfReferenceHolder referenceHolder)
	{
		if (referenceHolder != null && referenceHolder.Reference != null)
		{
			if (repeatedReferenceCollection.Contains(referenceHolder.Reference))
			{
				return true;
			}
			repeatedReferenceCollection.Add(referenceHolder.Reference);
		}
		return false;
	}

	internal void RemoveIdenticalResources(PdfResources resources, PdfPageBase newPage)
	{
		PdfDictionary pdfDictionary = PdfCrossTable.Dereference(resources["XObject"]) as PdfDictionary;
		PdfDictionary fontDictionary = PdfCrossTable.Dereference(resources["Font"]) as PdfDictionary;
		PdfDictionary pdfDictionary2 = PdfCrossTable.Dereference(resources["Properties"]) as PdfDictionary;
		PdfDictionary pdfDictionary3 = PdfCrossTable.Dereference(resources["ExtGState"]) as PdfDictionary;
		if (pdfDictionary != null)
		{
			Dictionary<PdfName, IPdfPrimitive> dictionary = new Dictionary<PdfName, IPdfPrimitive>();
			foreach (KeyValuePair<PdfName, IPdfPrimitive> item in pdfDictionary.Items)
			{
				dictionary.Add(item.Key, item.Value);
			}
			foreach (KeyValuePair<PdfName, IPdfPrimitive> item2 in dictionary)
			{
				if (!(item2.Value is PdfReferenceHolder) || CheckRepeatedReference(item2.Value as PdfReferenceHolder) || !(PdfCrossTable.Dereference(item2.Value) is PdfDictionary pdfDictionary4) || !pdfDictionary4.ContainsKey("Subtype"))
				{
					continue;
				}
				PdfName pdfName = PdfCrossTable.Dereference(pdfDictionary4["Subtype"]) as PdfName;
				if (pdfName != null && pdfName.Value == "Image")
				{
					if (pdfDictionary4 is PdfStream stream)
					{
						AddResourceCollection(stream, newPage, pdfDictionary4, pdfDictionary, item2.Key);
					}
				}
				else
				{
					if (!(pdfName != null) || !(pdfName.Value == "Form") || !pdfDictionary4.ContainsKey("Resources"))
					{
						continue;
					}
					PdfDictionary pdfDictionary5 = PdfCrossTable.Dereference(pdfDictionary4["Resources"]) as PdfDictionary;
					PdfReferenceHolder referenceHolder = pdfDictionary4["Resources"] as PdfReferenceHolder;
					bool flag = IsRepeatedResource(referenceHolder);
					if (!flag && pdfDictionary5 != null && (pdfDictionary5.ContainsKey("XObject") || pdfDictionary5.ContainsKey("Font")))
					{
						RemoveIdenticalResources(new PdfResources(pdfDictionary5), newPage);
						if (pdfDictionary4 is PdfStream stream2 && newPage.DestinationDocument != null && !newPage.DestinationDocument.m_isImported && newPage.templateResource)
						{
							AddResourceCollection(stream2, newPage, pdfDictionary4, pdfDictionary, item2.Key);
						}
					}
					else if (!flag && pdfDictionary4 is PdfStream stream3)
					{
						AddResourceCollection(stream3, newPage, pdfDictionary4, pdfDictionary, item2.Key);
					}
				}
			}
		}
		FontOptimization(fontDictionary, newPage);
		if (pdfDictionary2 != null)
		{
			PropertiseOptimization(pdfDictionary2, newPage);
		}
		if (pdfDictionary3 != null)
		{
			ExtGStateOptimization(pdfDictionary3, newPage);
		}
	}

	private bool IsRepeatedResource(PdfReferenceHolder referenceHolder)
	{
		bool result = false;
		if (referenceHolder != null && referenceHolder.Reference != null && m_xobjectReferenceCollection != null)
		{
			if (m_xobjectReferenceCollection.Contains(referenceHolder.Reference))
			{
				result = true;
			}
			else
			{
				m_xobjectReferenceCollection.Add(referenceHolder.Reference);
			}
		}
		return result;
	}

	private void FontOptimization(PdfDictionary fontDictionary, PdfPageBase newPage)
	{
		if (fontDictionary == null)
		{
			return;
		}
		Dictionary<PdfName, IPdfPrimitive> dictionary = new Dictionary<PdfName, IPdfPrimitive>();
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item in fontDictionary.Items)
		{
			dictionary.Add(item.Key, item.Value);
		}
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item2 in dictionary)
		{
			PdfReferenceHolder pdfReferenceHolder = item2.Value as PdfReferenceHolder;
			if ((pdfReferenceHolder != null && pdfReferenceHolder.Reference == null) || !(PdfCrossTable.Dereference(item2.Value) is PdfDictionary pdfDictionary) || !pdfDictionary.ContainsKey("Subtype"))
			{
				continue;
			}
			PdfName pdfName = PdfCrossTable.Dereference(pdfDictionary["Subtype"]) as PdfName;
			if (pdfName != null && pdfDictionary.ContainsKey("FontDescriptor"))
			{
				if (!(PdfCrossTable.Dereference(pdfDictionary["FontDescriptor"]) is PdfDictionary pdfDictionary2) || (!pdfDictionary2.ContainsKey("FontFile2") && !pdfDictionary2.ContainsKey("FontFile3")))
				{
					continue;
				}
				PdfStream pdfStream = null;
				PdfDictionary pdfDictionary3 = PdfCrossTable.Dereference(pdfDictionary2["FontFile2"]) as PdfDictionary;
				if (pdfDictionary3 == null)
				{
					pdfDictionary3 = PdfCrossTable.Dereference(pdfDictionary2["FontFile3"]) as PdfDictionary;
				}
				if (pdfDictionary3 != null)
				{
					pdfStream = pdfDictionary3 as PdfStream;
				}
				if (pdfStream == null)
				{
					continue;
				}
				string hashValue = string.Empty;
				if (CompareStream(pdfStream.InternalStream, newPage, out hashValue))
				{
					if (!(newPage.DestinationDocument.ResourceCollection[hashValue] is PdfDictionary))
					{
						continue;
					}
					PdfReferenceHolder pdfReferenceHolder2 = pdfReferenceHolder;
					PdfReferenceHolder pdfReferenceHolder3 = newPage.DestinationDocument.FontCollection[hashValue] as PdfReferenceHolder;
					PdfDictionary pdfDictionary4 = newPage.DestinationDocument.ResourceCollection[hashValue] as PdfDictionary;
					PdfDictionary pdfDictionary5 = PdfCrossTable.Dereference(pdfDictionary4["FontDescriptor"]) as PdfDictionary;
					if (pdfDictionary2.ObjectCollectionIndex != pdfDictionary5.ObjectCollectionIndex)
					{
						pdfPrimitivesCollection = new List<IPdfPrimitive>();
						RemoveFromDocument(pdfDictionary2);
						pdfPrimitivesCollection.Clear();
						pdfPrimitivesCollection = null;
					}
					if (!newPage.templateResource)
					{
						if (!(pdfReferenceHolder2 != null) || !(pdfReferenceHolder3 != null) || !(pdfReferenceHolder2.Reference != null) || !(pdfReferenceHolder3.Reference != null))
						{
							continue;
						}
						if (pdfReferenceHolder2.Reference.ObjNum == pdfReferenceHolder3.Reference.ObjNum)
						{
							if (pdfDictionary2 != null)
							{
								if (pdfDictionary4.ContainsKey("FontDescriptor"))
								{
									PdfDictionary pdfDictionary6 = PdfCrossTable.Dereference(pdfDictionary["FontDescriptor"]) as PdfDictionary;
								}
								fontDictionary.Items.Remove(item2.Key);
								fontDictionary.Items.Add(item2.Key, new PdfReferenceHolder(pdfDictionary4));
							}
						}
						else if (pdfDictionary2 != null)
						{
							pdfDictionary.Items.Remove(new PdfName("FontDescriptor"));
							pdfDictionary.Items.Add(new PdfName("FontDescriptor"), new PdfReferenceHolder(pdfDictionary5));
						}
					}
					else if (pdfDictionary2 != null)
					{
						pdfDictionary.Items.Remove(new PdfName("FontDescriptor"));
						pdfDictionary.Items.Add(new PdfName("FontDescriptor"), new PdfReferenceHolder(pdfDictionary5));
					}
				}
				else if (hashValue != string.Empty)
				{
					newPage.DestinationDocument.ResourceCollection.Add(hashValue, pdfDictionary);
					if (!newPage.DestinationDocument.FontCollection.ContainsKey(hashValue))
					{
						newPage.DestinationDocument.FontCollection.Add(hashValue, pdfReferenceHolder);
					}
				}
			}
			else if (pdfName != null && pdfDictionary.ContainsKey("DescendantFonts"))
			{
				OptimizeDescendantFonts(pdfDictionary, newPage, fontDictionary, item2.Key);
			}
			else
			{
				if (newPage.DestinationDocument == null)
				{
					continue;
				}
				if (newPage.DestinationDocument.ResourceCollection.ContainsKey(item2.Key.Value))
				{
					if (newPage.DestinationDocument.ResourceCollection[item2.Key.Value] is PdfReferenceHolder)
					{
						PdfReferenceHolder pdfReferenceHolder4 = newPage.DestinationDocument.ResourceCollection[item2.Key.Value] as PdfReferenceHolder;
						if (pdfReferenceHolder4 != null && pdfReferenceHolder4.Reference != null && item2.Value is PdfReferenceHolder && (item2.Value as PdfReferenceHolder).Reference != null && pdfReferenceHolder4.Reference == (item2.Value as PdfReferenceHolder).Reference)
						{
							fontDictionary.Items.Remove(item2.Key);
							fontDictionary.Items.Add(item2.Key, pdfReferenceHolder4);
						}
					}
				}
				else
				{
					newPage.DestinationDocument.ResourceCollection.Add(item2.Key.Value, item2.Value);
				}
			}
		}
	}

	private bool CompareStream(MemoryStream stream, PdfPageBase page, out string hashValue)
	{
		bool result = false;
		hashValue = string.Empty;
		if (page != null && page.DestinationDocument != null)
		{
			stream.Position = 0L;
			byte[] array = new byte[(int)stream.Length];
			stream.Read(array, 0, array.Length);
			stream.Position = 0L;
			hashValue = page.DestinationDocument.CreateHashFromStream(array);
			if (annotOptimize)
			{
				return page.DestinationDocument.AnnotationCollection.ContainsKey(hashValue);
			}
			if (contentOptimize)
			{
				return page.DestinationDocument.ContentCollection.ContainsKey(hashValue);
			}
			return page.DestinationDocument.ResourceCollection.ContainsKey(hashValue);
		}
		return result;
	}

	internal void CompareColorSpace(PdfDictionary dictionary)
	{
		if (dictionary.ContainsKey("ColorSpace"))
		{
			PdfArray obj = PdfCrossTable.Dereference(dictionary["ColorSpace"]) as PdfArray;
			obj.Add(obj);
		}
	}

	private void AddResourceCollection(PdfStream stream, PdfPageBase newPage, PdfDictionary xObject, PdfDictionary xObjectDictionary, PdfName xObjectkey)
	{
		string hashValue = string.Empty;
		PdfArray pdfArray = null;
		if (xObject.ContainsKey("ColorSpace"))
		{
			pdfArray = PdfCrossTable.Dereference(xObject["ColorSpace"]) as PdfArray;
			if (!colorSpaceList.ContainsKey(xObject))
			{
				colorSpaceList.Add(xObject, pdfArray);
			}
		}
		if (CompareStream(stream.InternalStream, newPage, out hashValue))
		{
			PdfDictionary pdfDictionary = newPage.DestinationDocument.ResourceCollection[hashValue] as PdfDictionary;
			if (colorSpaceList.ContainsKey(pdfDictionary) && colorSpaceList[pdfDictionary] != pdfArray)
			{
				isSkipColorSpace = true;
			}
			else
			{
				if (newPage.templateResource && !ValidateXObjectDictionary(pdfDictionary, xObject))
				{
					return;
				}
				if (xObject.ObjectCollectionIndex != pdfDictionary.ObjectCollectionIndex && !xObject.ContainsKey("SMask") && !pdfDictionary.ContainsKey("SMask"))
				{
					pdfPrimitivesCollection = new List<IPdfPrimitive>();
					RemoveFromDocument(xObject);
					pdfPrimitivesCollection.Clear();
					pdfPrimitivesCollection = null;
				}
				if (xObject == null)
				{
					return;
				}
				if (xObject != null && xObject.ContainsKey("SMask") && pdfDictionary.ContainsKey("SMask"))
				{
					PdfStream pdfStream = PdfCrossTable.Dereference(xObject["SMask"]) as PdfStream;
					PdfStream pdfStream2 = PdfCrossTable.Dereference(pdfDictionary["SMask"]) as PdfStream;
					if (pdfStream == null || pdfStream2 == null)
					{
						return;
					}
					pdfStream.InternalStream.Position = 0L;
					byte[] array = new byte[(int)pdfStream.InternalStream.Length];
					pdfStream.InternalStream.Read(array, 0, array.Length);
					pdfStream.InternalStream.Position = 0L;
					string text = newPage.DestinationDocument.CreateHashFromStream(array);
					pdfStream2.InternalStream.Position = 0L;
					array = new byte[(int)pdfStream2.InternalStream.Length];
					pdfStream2.InternalStream.Read(array, 0, array.Length);
					pdfStream2.InternalStream.Position = 0L;
					string text2 = newPage.DestinationDocument.CreateHashFromStream(array);
					if (!string.IsNullOrEmpty(text2) && !string.IsNullOrEmpty(text) && text == text2)
					{
						if (xObject.ObjectCollectionIndex != pdfDictionary.ObjectCollectionIndex)
						{
							pdfPrimitivesCollection = new List<IPdfPrimitive>();
							RemoveFromDocument(xObject);
							pdfPrimitivesCollection.Clear();
							pdfPrimitivesCollection = null;
						}
						xObjectDictionary.Items.Remove(xObjectkey);
						xObjectDictionary.Items.Add(xObjectkey, new PdfReferenceHolder(pdfDictionary));
					}
				}
				else if (xObject != null && !stream.ContainsKey("SMask") && !xObject.ContainsKey("SMask"))
				{
					xObjectDictionary.Items.Remove(xObjectkey);
					pdfPrimitivesCollection = new List<IPdfPrimitive>();
					AddToDocument(pdfDictionary);
					pdfPrimitivesCollection.Clear();
					pdfPrimitivesCollection = null;
					xObjectDictionary.Items.Add(xObjectkey, new PdfReferenceHolder(pdfDictionary));
				}
			}
		}
		else if (hashValue != string.Empty)
		{
			newPage.DestinationDocument.ResourceCollection.Add(hashValue, xObject);
		}
	}

	private string GetHashValuse(MemoryStream stream)
	{
		string result = string.Empty;
		if (DestinationDocument != null)
		{
			stream.Position = 0L;
			byte[] array = new byte[(int)stream.Length];
			stream.Read(array, 0, array.Length);
			stream.Position = 0L;
			result = DestinationDocument.CreateHashFromStream(array);
		}
		return result;
	}

	private bool ValidateXObjectDictionary(PdfDictionary existing, PdfDictionary current)
	{
		bool flag = true;
		PdfStream pdfStream = existing as PdfStream;
		PdfStream pdfStream2 = current as PdfStream;
		if (pdfStream != null && pdfStream2 != null && pdfStream2.Data.Length != pdfStream.Data.Length)
		{
			return false;
		}
		if (existing != null && existing.ContainsKey("Resources"))
		{
			PdfDictionary pdfDictionary = PdfCrossTable.Dereference(current["Resources"]) as PdfDictionary;
			PdfDictionary pdfDictionary2 = PdfCrossTable.Dereference(existing["Resources"]) as PdfDictionary;
			PdfReferenceHolder referenceHolder = current["Resources"] as PdfReferenceHolder;
			if (IsRepeatedResource(referenceHolder))
			{
				return false;
			}
			if (pdfStream.InternalStream != null && pdfStream2.InternalStream != null && GetHashValuse(pdfStream.InternalStream) != GetHashValuse(pdfStream2.InternalStream))
			{
				return false;
			}
			PdfReferenceHolder pdfReferenceHolder = null;
			if (pdfDictionary != null && pdfDictionary2 != null && pdfDictionary.ContainsKey("XObject"))
			{
				PdfDictionary pdfDictionary3 = PdfCrossTable.Dereference(pdfDictionary["XObject"]) as PdfDictionary;
				PdfDictionary pdfDictionary4 = PdfCrossTable.Dereference(pdfDictionary2["XObject"]) as PdfDictionary;
				if (pdfDictionary3 != null && pdfDictionary4 != null && pdfDictionary3.Count == pdfDictionary4.Count)
				{
					foreach (KeyValuePair<PdfName, IPdfPrimitive> item in pdfDictionary4.Items)
					{
						pdfReferenceHolder = item.Value as PdfReferenceHolder;
						foreach (KeyValuePair<PdfName, IPdfPrimitive> item2 in pdfDictionary3.Items)
						{
							PdfReferenceHolder pdfReferenceHolder2 = item2.Value as PdfReferenceHolder;
							if ((!(pdfReferenceHolder2 != null) || !(pdfReferenceHolder2.Reference != null) || IsRepeatedResource(pdfReferenceHolder2)) && (!(pdfReferenceHolder2 != null) || !(pdfReferenceHolder2.Reference != null) || !(pdfReferenceHolder != null) || !(pdfReferenceHolder.Reference != null) || !(pdfReferenceHolder2.Reference != pdfReferenceHolder.Reference)))
							{
								continue;
							}
							PdfStream pdfStream3 = PdfCrossTable.Dereference(item2.Value) as PdfStream;
							if (!pdfDictionary4.Items.ContainsKey(item2.Key))
							{
								continue;
							}
							PdfStream pdfStream4 = PdfCrossTable.Dereference(pdfDictionary4.Items[item2.Key]) as PdfStream;
							if (pdfStream3 != null && pdfStream4 != null)
							{
								flag = ValidateXObjectDictionary(pdfStream4, pdfStream3);
								if (!flag)
								{
									return flag;
								}
							}
						}
					}
				}
			}
		}
		return flag;
	}

	private void OptimizeDescendantFonts(PdfDictionary xObject, PdfPageBase newPage, PdfDictionary fontDictionary, PdfName fontKey)
	{
		if (!(PdfCrossTable.Dereference(xObject["DescendantFonts"]) is PdfArray { Count: >0 } pdfArray) || !(PdfCrossTable.Dereference(pdfArray[0]) is PdfDictionary pdfDictionary) || !pdfDictionary.ContainsKey("FontDescriptor") || !(PdfCrossTable.Dereference(pdfDictionary["FontDescriptor"]) is PdfDictionary pdfDictionary2) || (!pdfDictionary2.ContainsKey("FontFile2") && !pdfDictionary2.ContainsKey("FontFile3")))
		{
			return;
		}
		PdfStream pdfStream = null;
		PdfDictionary pdfDictionary3 = PdfCrossTable.Dereference(pdfDictionary2["FontFile2"]) as PdfDictionary;
		if (pdfDictionary3 == null)
		{
			pdfDictionary3 = PdfCrossTable.Dereference(pdfDictionary2["FontFile3"]) as PdfDictionary;
		}
		if (pdfDictionary3 != null)
		{
			pdfStream = pdfDictionary3 as PdfStream;
		}
		if (pdfStream == null)
		{
			return;
		}
		string hashValue = string.Empty;
		if (CompareStream(pdfStream.InternalStream, newPage, out hashValue))
		{
			if (newPage.DestinationDocument.ResourceCollection[hashValue] is PdfArray)
			{
				PdfArray pdfArray2 = newPage.DestinationDocument.ResourceCollection[hashValue] as PdfArray;
				PdfDictionary pdfDictionary4 = PdfCrossTable.Dereference(pdfArray2[0]) as PdfDictionary;
				if (pdfDictionary.ObjectCollectionIndex != pdfDictionary4.ObjectCollectionIndex)
				{
					pdfPrimitivesCollection = new List<IPdfPrimitive>();
					RemoveFromDocument(pdfDictionary);
					pdfPrimitivesCollection.Clear();
					pdfPrimitivesCollection = null;
				}
				if (pdfArray2 != null)
				{
					xObject.Items.Remove(new PdfName("DescendantFonts"));
					xObject.Items.Add(new PdfName("DescendantFonts"), new PdfReferenceHolder(pdfArray2));
					fontDictionary.Items.Remove(fontKey);
					fontDictionary.Items.Add(fontKey, new PdfReferenceHolder(xObject));
				}
			}
		}
		else if (hashValue != string.Empty)
		{
			newPage.DestinationDocument.ResourceCollection.Add(hashValue, pdfArray);
		}
	}
}
