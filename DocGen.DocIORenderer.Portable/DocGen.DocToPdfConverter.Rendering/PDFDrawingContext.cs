#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using DocGen.ChartToImageConverter;
using DocGen.DocIO;
using DocGen.DocIO.DLS;
using DocGen.DocIO.DLS.Entities;
using DocGen.DocIO.DLS.Rendering;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;
using DocGen.DocIORenderer.Rendering;
using DocGen.Drawing;
using DocGen.Drawing.DocIOHelper;
using DocGen.Drawing.SkiaSharpHelper;
using DocGen.Layouting;
using DocGen.Office;
using DocGen.OfficeChart;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.Pdf;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.Primitives;

namespace DocGen.DocToPdfConverter.Rendering;

internal class PDFDrawingContext : DocumentLayouter
{
	internal class DefaultBorders : RowFormat
	{
		public Border Vertical => base.Borders.Vertical;

		public Border Horizontal => base.Borders.Horizontal;

		public DefaultBorders(RowFormat format)
		{
			InitBorder(base.Borders.Horizontal, format.Borders.Horizontal);
			InitBorder(base.Borders.Vertical, format.Borders.Vertical);
		}

		private void InitBorder(Border destination, Border sourse)
		{
			destination.BorderType = sourse.BorderType;
			destination.Color = sourse.Color;
			destination.LineWidth = sourse.LineWidth;
			destination.Shadow = sourse.Shadow;
			destination.Space = sourse.Space;
			destination.HasNoneStyle = sourse.HasNoneStyle;
		}
	}

	private const float DEF_SCRIPT_FACTOR = 1.5f;

	private const float DEF_EMBOSS_ENGRAVE_FACTOR = 0.2f;

	private const float DEF_DINOFFC_TEXT_FACTOR = 0.2f;

	private const float DEF_DINOFFC_LISTTEXT_FACTOR = 0.1f;

	private const float DEF_FONT_SIZE = 12f;

	private const float DEF_PICBULLET_MIN_FONT_SIZE = 4f;

	private const float DEF_PICBULLET_SCALE_FACTOR = 10f;

	private const char NONBREAK_HYPHEN = '\u001e';

	private const char Zero_Width_Joiner = '\u200d';

	private const char SOFT_HYPHEN = '\u00ad';

	private const char SPACE = ' ';

	private bool m_enableComplexScript;

	[ThreadStatic]
	private static Dictionary<WPicture, DocGen.Drawing.SkiaSharpHelper.Image> headerImageCollection;

	private Stack<PdfStructureElement> pdfStructureTable;

	private PdfGraphics m_pdfgraphics;

	private int m_imageQuality;

	private bool m_preserveFormFields;

	[ThreadStatic]
	private static Dictionary<string, PdfFont> pdfFontCollection;

	[ThreadStatic]
	private static Dictionary<string, PdfFont> m_substitutedFonts;

	private ChartRenderingOptions m_chartRenderingOptions;

	private ChartToImageconverter m_chartToImageconverter;

	private Dictionary<string, Stream> m_privateFontStream;

	private List<FallbackFont> m_fallbackFonts;

	private PdfDocument m_pdfdocument;

	private Graphics m_graphics;

	private Graphics m_graphicsBmp;

	private List<Dictionary<string, RectangleF>> m_hyperLinks = new List<Dictionary<string, RectangleF>>();

	internal WParagraph currParagraph;

	internal WTextRange currTextRange;

	internal Hyperlink currHyperlink;

	internal WFieldMark formFieldEnd;

	private WField m_currentField;

	private string m_currentBkName;

	private new ExportBookmarkType m_exportBookmarkType;

	internal RectangleF CurrParagraphBounds;

	private byte m_bFlag;

	private byte m_bFlag1;

	internal int m_orderIndex = 1;

	internal float m_pageMarginLeft;

	private Dictionary<int, LayoutedWidget> m_overLappedShapeWidgets;

	private FontMetric m_fontmetric;

	private StringFormat m_stringformat;

	internal Stack<RectangleF> ClipBoundsContainer;

	private List<LayoutedWidget> m_editableFormFieldinEMF = new List<LayoutedWidget>();

	private RectangleF m_editableTextFormBounds;

	private RectangleF m_rotateTransform;

	private string m_editableTextFormText = "";

	private WTextRange m_editableTextFormTextRange;

	private int m_lastTextRangeIndex;

	private Dictionary<int, int> autoTagIndex;

	private string m_commentId;

	private List<PointF[]> m_commentMarks;

	internal List<KeyValuePair<string, bool>> m_previousLineCommentStartMarks;

	internal Hyperlink m_prevPageHyperlink;

	private int autoTagCount;

	[ThreadStatic]
	private static List<BookmarkPosition> m_bookmarks;

	private int currParagraphIndex = -1;

	private Dictionary<WCharacterFormat, List<RectangleF>> underLineValues;

	private Dictionary<WCharacterFormat, RectangleF> strikeThroughValues;

	private Dictionary<WPicture, DocGen.Drawing.SkiaSharpHelper.Image> HeaderImageCollection
	{
		get
		{
			if (headerImageCollection == null)
			{
				headerImageCollection = new Dictionary<WPicture, DocGen.Drawing.SkiaSharpHelper.Image>();
			}
			return headerImageCollection;
		}
	}

	private Dictionary<string, PdfFont> PdfFontCollection
	{
		get
		{
			if (pdfFontCollection == null)
			{
				pdfFontCollection = new Dictionary<string, PdfFont>();
			}
			return pdfFontCollection;
		}
	}

	private Dictionary<string, PdfFont> SubstitutedFonts
	{
		get
		{
			if (m_substitutedFonts == null)
			{
				m_substitutedFonts = new Dictionary<string, PdfFont>();
			}
			return m_substitutedFonts;
		}
	}

	private bool IsListCharacter
	{
		get
		{
			return (m_bFlag & 1) != 0;
		}
		set
		{
			m_bFlag = (byte)((m_bFlag & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal bool RecreateNestedMetafile
	{
		get
		{
			return (m_bFlag & 4) >> 2 != 0;
		}
		set
		{
			m_bFlag = (byte)((m_bFlag & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal Dictionary<int, int> AutoTagIndex
	{
		get
		{
			if (autoTagIndex == null)
			{
				autoTagIndex = new Dictionary<int, int>();
			}
			return autoTagIndex;
		}
	}

	internal new List<LayoutedWidget> EditableFormFieldinEMF
	{
		get
		{
			return m_editableFormFieldinEMF;
		}
		set
		{
			m_editableFormFieldinEMF = value;
		}
	}

	internal WField CurrentRefField
	{
		get
		{
			return m_currentField;
		}
		set
		{
			m_currentField = value;
		}
	}

	internal PdfDocument PdfDocument
	{
		get
		{
			return m_pdfdocument;
		}
		set
		{
			m_pdfdocument = value;
		}
	}

	internal string CurrentBookmarkName
	{
		get
		{
			return m_currentBkName;
		}
		set
		{
			m_currentBkName = value;
		}
	}

	internal new ExportBookmarkType ExportBookmarks
	{
		get
		{
			return m_exportBookmarkType;
		}
		set
		{
			m_exportBookmarkType = value;
		}
	}

	internal bool EnableComplexScript
	{
		get
		{
			return m_enableComplexScript;
		}
		set
		{
			m_enableComplexScript = value;
		}
	}

	internal PdfGraphics PDFGraphics
	{
		get
		{
			return m_pdfgraphics;
		}
		set
		{
			if (m_pdfgraphics != value)
			{
				m_pdfgraphics = value;
			}
		}
	}

	internal int ImageQuality
	{
		get
		{
			return m_imageQuality;
		}
		set
		{
			m_imageQuality = value;
		}
	}

	internal bool PreserveFormFields
	{
		get
		{
			return m_preserveFormFields;
		}
		set
		{
			m_preserveFormFields = value;
		}
	}

	internal List<FallbackFont> FallbackFonts
	{
		get
		{
			return m_fallbackFonts;
		}
		set
		{
			m_fallbackFonts = value;
		}
	}

	public Dictionary<string, Stream> FontStreams
	{
		get
		{
			return m_privateFontStream;
		}
		set
		{
			if (value != null)
			{
				m_privateFontStream = value;
			}
		}
	}

	public Graphics Graphics
	{
		get
		{
			return m_graphics;
		}
		set
		{
			if (m_graphics != value)
			{
				m_graphics = value;
			}
		}
	}

	internal Graphics GraphicsBmp
	{
		get
		{
			if (m_graphicsBmp == null)
			{
				DocGen.Drawing.SkiaSharpHelper.Bitmap bitmap = new DocGen.Drawing.SkiaSharpHelper.Bitmap(120, 120);
				m_graphicsBmp = new Graphics(bitmap);
				bitmap.SetResolution(120f, 120f);
				m_graphicsBmp.PageUnit = GraphicsUnit.Point;
			}
			return m_graphicsBmp;
		}
	}

	internal List<Dictionary<string, RectangleF>> Hyperlinks => m_hyperLinks;

	internal static List<Dictionary<string, BookmarkHyperlink>> BookmarkHyperlinksList => DocumentLayouter.BookmarkHyperlinks;

	internal new static List<BookmarkPosition> Bookmarks
	{
		get
		{
			if (m_bookmarks == null)
			{
				m_bookmarks = new List<BookmarkPosition>();
			}
			return m_bookmarks;
		}
	}

	internal Dictionary<int, LayoutedWidget> OverLappedShapeWidgets
	{
		get
		{
			if (m_overLappedShapeWidgets == null)
			{
				m_overLappedShapeWidgets = new Dictionary<int, LayoutedWidget>();
			}
			return m_overLappedShapeWidgets;
		}
	}

	public FontMetric FontMetric
	{
		get
		{
			if (m_fontmetric == null)
			{
				m_fontmetric = new FontMetric();
			}
			return m_fontmetric;
		}
	}

	public StringFormat StringFormt
	{
		get
		{
			if (m_stringformat == null)
			{
				m_stringformat = new StringFormat();
				m_stringformat.FormatFlags &= ~StringFormatFlags.LineLimit;
				m_stringformat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
				m_stringformat.FormatFlags |= StringFormatFlags.NoClip;
				m_stringformat.Trimming = StringTrimming.Word;
			}
			return m_stringformat;
		}
	}

	internal bool EmbedFonts
	{
		get
		{
			return (m_bFlag & 8) >> 3 != 0;
		}
		set
		{
			m_bFlag = (byte)((m_bFlag & 0xF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	internal bool EmbedCompleteFonts
	{
		get
		{
			return (m_bFlag & 0x10) >> 4 != 0;
		}
		set
		{
			m_bFlag = (byte)((m_bFlag & 0xEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	internal bool AutoTag
	{
		get
		{
			return (m_bFlag1 & 1) != 0;
		}
		set
		{
			m_bFlag1 = (byte)((m_bFlag1 & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal ChartRenderingOptions ChartRenderingOptions
	{
		get
		{
			if (m_chartRenderingOptions == null)
			{
				m_chartRenderingOptions = new ChartRenderingOptions();
			}
			return m_chartRenderingOptions;
		}
		set
		{
			m_chartRenderingOptions = value;
		}
	}

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

	internal static void ClearFontCache()
	{
		if (pdfFontCollection != null)
		{
			pdfFontCollection.Clear();
			pdfFontCollection = null;
		}
		if (m_substitutedFonts != null)
		{
			m_substitutedFonts.Clear();
			m_substitutedFonts = null;
		}
		if (headerImageCollection != null)
		{
			foreach (KeyValuePair<WPicture, DocGen.Drawing.SkiaSharpHelper.Image> item in headerImageCollection)
			{
				DocGen.Drawing.SkiaSharpHelper.Image value = item.Value;
				if (value != null)
				{
					value.Dispose();
					value = null;
				}
			}
			headerImageCollection.Clear();
			headerImageCollection = null;
		}
		if (m_bookmarks != null)
		{
			for (int i = 0; i < m_bookmarks.Count; i++)
			{
				_ = m_bookmarks[i];
			}
			m_bookmarks.Clear();
			m_bookmarks = null;
		}
	}

	internal PDFDrawingContext()
	{
		DocGen.Drawing.SkiaSharpHelper.Bitmap bitmap = new DocGen.Drawing.SkiaSharpHelper.Bitmap(1, 1);
		bitmap.SetResolution(120f, 120f);
		Graphics = new Graphics(bitmap);
		Graphics.PageUnit = GraphicsUnit.Point;
	}

	internal PDFDrawingContext(bool isEmpty)
	{
	}

	internal PDFDrawingContext(PdfGraphics pdfgraphics, Graphics graphics, GraphicsUnit pageUnit, PdfDocument pdfDocument)
	{
		if (pdfgraphics == null)
		{
			throw new ArgumentException("Graphics");
		}
		m_pdfdocument = pdfDocument;
		m_pdfgraphics = pdfgraphics;
		m_pdfgraphics.PageUnit = pageUnit;
		m_graphics = graphics;
		m_graphics.PageUnit = pageUnit;
	}

	internal void DrawOverLappedShapeWidgets(bool isHaveToInitLayoutInfo)
	{
		List<int> list = new List<int>(OverLappedShapeWidgets.Keys);
		list.Sort();
		int num = 0;
		if (AutoTag)
		{
			num = autoTagCount;
		}
		foreach (int item in list)
		{
			if (AutoTag)
			{
				autoTagCount = AutoTagIndex[item];
			}
			currParagraph = GetOwnerParagraph(OverLappedShapeWidgets[item]);
			Draw(OverLappedShapeWidgets[item], isHaveToInitLayoutInfo);
			if (isHaveToInitLayoutInfo && !(OverLappedShapeWidgets[item].Widget is WTable))
			{
				OverLappedShapeWidgets[item].InitLayoutInfoAll();
			}
		}
		if (AutoTag)
		{
			autoTagCount = num;
			AutoTagIndex.Clear();
		}
		OverLappedShapeWidgets.Clear();
		m_orderIndex = 1;
	}

	internal bool IsColorMismatched(IEntity nextsibling, WParagraph paragraph)
	{
		if (nextsibling != null && nextsibling is WParagraph && !(nextsibling as WParagraph).ParagraphFormat.BackColor.IsEmpty && (!(nextsibling as WParagraph).ParagraphFormat.Borders.Top.Color.Equals(paragraph.ParagraphFormat.Borders.Top.Color) || !(nextsibling as WParagraph).ParagraphFormat.Borders.Bottom.Color.Equals(paragraph.ParagraphFormat.Borders.Bottom.Color) || !(nextsibling as WParagraph).ParagraphFormat.Borders.Left.Color.Equals(paragraph.ParagraphFormat.Borders.Left.Color) || !(nextsibling as WParagraph).ParagraphFormat.Borders.Right.Color.Equals(paragraph.ParagraphFormat.Borders.Right.Color)))
		{
			return true;
		}
		return false;
	}

	internal PdfStructureElement CreateAutoTag(PdfTagType tagType, string alternateText, string title, bool isOverlapedShape)
	{
		PdfStructureElement pdfStructureElement = null;
		if (!isOverlapedShape)
		{
			autoTagCount++;
		}
		switch (tagType)
		{
		case PdfTagType.Figure:
			pdfStructureElement = new PdfStructureElement(PdfTagType.Figure);
			pdfStructureElement.AlternateText = alternateText;
			pdfStructureElement.Title = title;
			pdfStructureElement.Order = autoTagCount;
			break;
		case PdfTagType.Form:
			pdfStructureElement = new PdfStructureElement(PdfTagType.Form);
			pdfStructureElement.Order = autoTagCount;
			break;
		case PdfTagType.Paragraph:
			pdfStructureElement = new PdfStructureElement(PdfTagType.Paragraph);
			pdfStructureElement.Order = autoTagCount;
			break;
		case PdfTagType.HeadingLevel1:
			pdfStructureElement = new PdfStructureElement(PdfTagType.HeadingLevel1);
			pdfStructureElement.Order = autoTagCount;
			break;
		case PdfTagType.HeadingLevel2:
			pdfStructureElement = new PdfStructureElement(PdfTagType.HeadingLevel2);
			pdfStructureElement.Order = autoTagCount;
			break;
		case PdfTagType.HeadingLevel3:
			pdfStructureElement = new PdfStructureElement(PdfTagType.HeadingLevel3);
			pdfStructureElement.Order = autoTagCount;
			break;
		case PdfTagType.HeadingLevel4:
			pdfStructureElement = new PdfStructureElement(PdfTagType.HeadingLevel4);
			pdfStructureElement.Order = autoTagCount;
			break;
		case PdfTagType.HeadingLevel5:
			pdfStructureElement = new PdfStructureElement(PdfTagType.HeadingLevel5);
			pdfStructureElement.Order = autoTagCount;
			break;
		case PdfTagType.HeadingLevel6:
			pdfStructureElement = new PdfStructureElement(PdfTagType.HeadingLevel6);
			pdfStructureElement.Order = autoTagCount;
			break;
		case PdfTagType.Table:
			pdfStructureElement = new PdfStructureElement(PdfTagType.Table);
			pdfStructureElement.Order = autoTagCount;
			break;
		case PdfTagType.TableRow:
			pdfStructureElement = new PdfStructureElement(PdfTagType.TableRow);
			pdfStructureElement.Order = autoTagCount;
			break;
		case PdfTagType.TableHeader:
			pdfStructureElement = new PdfStructureElement(PdfTagType.TableHeader);
			pdfStructureElement.Order = autoTagCount;
			break;
		case PdfTagType.TableDataCell:
			pdfStructureElement = new PdfStructureElement(PdfTagType.TableDataCell);
			pdfStructureElement.Order = autoTagCount;
			break;
		}
		return pdfStructureElement;
	}

	internal PdfStructureElement CreatePdfStructureElement(WParagraph paragraph, LayoutedWidget layoutedWidget)
	{
		if (paragraph.ParaStyle is WParagraphStyle wParagraphStyle && wParagraphStyle.ParagraphFormat.IsBuiltInHeadingStyle(wParagraphStyle.Name) && !paragraph.IsInCell)
		{
			int headingLevel = paragraph.GetHeadingLevel(wParagraphStyle, paragraph);
			if (headingLevel >= 0 && headingLevel < 6)
			{
				PdfTagType headingLevelPdfTagType = GetHeadingLevelPdfTagType(headingLevel);
				return CreateAutoTag(headingLevelPdfTagType, "", "", IsOverLappedShapeWidget(layoutedWidget));
			}
			return CreateAutoTag(PdfTagType.Paragraph, "", "", IsOverLappedShapeWidget(layoutedWidget));
		}
		if (paragraph.IsInCell)
		{
			PdfStructureElement pdfStructureElement = CreateAutoTag(PdfTagType.Paragraph, "", "", isOverlapedShape: true);
			pdfStructureElement.Parent = pdfStructureTable.Peek();
			return pdfStructureElement;
		}
		return CreateAutoTag(PdfTagType.Paragraph, "", "", IsOverLappedShapeWidget(layoutedWidget));
	}

	internal PdfTagType GetHeadingLevelPdfTagType(int outlineLevel)
	{
		return outlineLevel switch
		{
			0 => PdfTagType.HeadingLevel1, 
			1 => PdfTagType.HeadingLevel2, 
			2 => PdfTagType.HeadingLevel3, 
			3 => PdfTagType.HeadingLevel4, 
			4 => PdfTagType.HeadingLevel5, 
			5 => PdfTagType.HeadingLevel6, 
			_ => PdfTagType.Heading, 
		};
	}

	internal void CreatePdfStructureElement(LayoutedWidget ltWidget, Entity ownerWidget, WTableCell cellWidget)
	{
		if (ltWidget.Widget is WTable)
		{
			if (pdfStructureTable == null)
			{
				pdfStructureTable = new Stack<PdfStructureElement>();
			}
			PdfStructureElement pdfStructureElement = null;
			if (pdfStructureTable.Count > 0)
			{
				pdfStructureElement = CreateAutoTag(PdfTagType.Table, "", "", isOverlapedShape: true);
				pdfStructureElement.Parent = pdfStructureTable.Peek();
			}
			else
			{
				pdfStructureElement = CreateAutoTag(PdfTagType.Table, "", "", isOverlapedShape: false);
			}
			pdfStructureTable.Push(pdfStructureElement);
		}
		else if (ltWidget.Widget is WTableRow)
		{
			PdfStructureElement pdfStructureElement2 = CreateAutoTag(PdfTagType.TableRow, "", "", isOverlapedShape: true);
			pdfStructureElement2.Parent = pdfStructureTable.Peek();
			pdfStructureTable.Push(pdfStructureElement2);
		}
		else if (ownerWidget is WTableRow)
		{
			PdfStructureElement pdfStructureElement3 = null;
			pdfStructureElement3 = ((ownerWidget.Index != 0 && cellWidget.Index != 0) ? CreateAutoTag(PdfTagType.TableDataCell, "", "", isOverlapedShape: true) : CreateAutoTag(PdfTagType.TableHeader, "", "", isOverlapedShape: true));
			int value = (((ownerWidget as WTableRow).OwnerTable == null) ? 1 : (ownerWidget as WTableRow).OwnerTable.GetRowSpan(cellWidget));
			PdfDictionary pdfDictionary = new PdfDictionary();
			pdfDictionary["Scope"] = new PdfName("Column");
			pdfDictionary["O"] = new PdfName("Table");
			pdfDictionary["RowSpan"] = new PdfNumber(value);
			pdfDictionary["ColSpan"] = new PdfNumber(cellWidget.GridSpan);
			pdfStructureElement3.SetAttributeDictionary(pdfDictionary);
			pdfStructureElement3.Parent = pdfStructureTable.Peek();
			pdfStructureTable.Push(pdfStructureElement3);
		}
	}

	internal void DrawParagraph(WParagraph paragraph, LayoutedWidget ltWidget)
	{
		WListFormat listFormatValue = paragraph.GetListFormatValue();
		currTextRange = null;
		CurrParagraphBounds = ltWidget.Bounds;
		LayoutedWidget layoutedWidget = null;
		if (paragraph.m_layoutInfo is ParagraphLayoutInfo && (paragraph.m_layoutInfo as ParagraphLayoutInfo).IsSectionEndMark)
		{
			return;
		}
		bool isParagraphMarkIsHidden = false;
		if (paragraph.BreakCharacterFormat.Hidden)
		{
			isParagraphMarkIsHidden = true;
		}
		WParagraphFormat paragraphFormat = paragraph.ParagraphFormat;
		bool isEmpty = paragraphFormat.BackColor.IsEmpty;
		if ((paragraphFormat.TextureStyle != 0 || !isEmpty) && !IsLinesInteresectWithFloatingItems(ltWidget, isLineContainer: true))
		{
			bool resetTransform = false;
			RectangleF boundsToDrawParagraphBackGroundColor = GetBoundsToDrawParagraphBackGroundColor(paragraph, ltWidget, isParagraphMarkIsHidden, isLineDrawing: false, ref resetTransform);
			IEntity nextSibling = paragraph.NextSibling;
			Entity ownerEntity = paragraph.GetOwnerEntity();
			if (ownerEntity is WTableCell && (ownerEntity as WTableCell).OwnerRow != null && ((IWidget)(ownerEntity as WTableCell).OwnerRow).LayoutInfo is RowLayoutInfo)
			{
				RowLayoutInfo rowLayoutInfo = ((IWidget)(ownerEntity as WTableCell).OwnerRow).LayoutInfo as RowLayoutInfo;
				if (paragraph.IsInCell && !rowLayoutInfo.IsExactlyRowHeight && (nextSibling == null || !(nextSibling is WParagraph) || !(nextSibling as WParagraph).ParagraphFormat.BackColor.IsEmpty || !ltWidget.IsLastItemInPage))
				{
					boundsToDrawParagraphBackGroundColor.Height -= paragraphFormat.AfterSpacing;
				}
			}
			if (IsColorMismatched(nextSibling, paragraph))
			{
				boundsToDrawParagraphBackGroundColor.Height -= paragraphFormat.AfterSpacing;
			}
			if (paragraphFormat.TextureStyle != 0)
			{
				DrawTextureStyle(paragraphFormat.TextureStyle, paragraphFormat.ForeColor, paragraphFormat.BackColor, boundsToDrawParagraphBackGroundColor);
			}
			else if (!isEmpty)
			{
				PDFGraphics.DrawRectangle(new PdfSolidBrush(paragraphFormat.BackColor), boundsToDrawParagraphBackGroundColor);
			}
			if (AutoTag && (paragraphFormat.TextureStyle != 0 || !isEmpty))
			{
				PdfStructureElement tag = CreatePdfStructureElement(paragraph, ltWidget);
				PDFGraphics.SetTag(tag);
			}
			if (resetTransform)
			{
				ResetTransform();
			}
		}
		if (ltWidget.TextTag != "Splitted" && ltWidget.ChildWidgets.Count > 0)
		{
			layoutedWidget = ltWidget.ChildWidgets[ltWidget.ChildWidgets.Count - 1];
			if (layoutedWidget.ChildWidgets.Count > 0 && layoutedWidget.ChildWidgets[0].HorizontalAlign == HAlignment.Justify && !layoutedWidget.IsLastLine)
			{
				for (int i = 0; i < layoutedWidget.ChildWidgets.Count; i++)
				{
					layoutedWidget.ChildWidgets[i].IsLastLine = true;
				}
				AlignChildWidgets(layoutedWidget, paragraph);
			}
			DrawBarTabStop(paragraph, ltWidget);
		}
		if (!paragraphFormat.Borders.NoBorder && listFormatValue != null && listFormatValue.ListType == ListType.NoList)
		{
			DrawParagraphBorders(paragraph, paragraphFormat, ltWidget, isParagraphMarkIsHidden);
		}
		else
		{
			DrawParagraphBorders(paragraph, paragraphFormat, ltWidget, isParagraphMarkIsHidden);
		}
		if (!IsParagraphContainingListHasBreak(ltWidget))
		{
			DrawList(paragraph, ltWidget, listFormatValue);
		}
	}

	internal void DrawTextBox(WTextBox textBox, LayoutedWidget ltWidget)
	{
		Color color = default(Color);
		WTextBoxFormat textBoxFormat = textBox.TextBoxFormat;
		color = ((textBox.IsShape && textBox.Shape.FillFormat.FillType == FillType.FillSolid) ? ((textBox.Shape.FillFormat.Fill && (!textBox.Shape.FillFormat.IsDefaultFill || textBox.Shape.IsFillStyleInline) && textBox.Shape.FillFormat.Transparency != 100f) ? textBox.Shape.FillFormat.Color : Color.Empty) : textBoxFormat.FillColor);
		Borders borders = new Borders();
		float num = ((textBox.Shape != null) ? textBox.Shape.Rotation : textBox.TextBoxFormat.Rotation);
		if (textBox.Owner is GroupShape || textBox.Owner is ChildGroupShape)
		{
			RectangleF rectangleF = ltWidget.Bounds;
			if (textBox.TextBoxFormat.TextDirection != 0 && textBox.TextBoxFormat.TextDirection != DocGen.DocIO.DLS.TextDirection.HorizontalFarEast)
			{
				RectangleF rectangleF2 = rectangleF;
				rectangleF2.Width += rectangleF2.Height;
				rectangleF2.Height = rectangleF2.Width - rectangleF2.Height;
				rectangleF2.Width -= rectangleF2.Height;
				rectangleF = rectangleF2;
			}
			rectangleF = ((textBoxFormat.Rotation == 0f) ? UpdateClipBounds(rectangleF, reverseClipping: false) : UpdateClipBounds(rectangleF));
			ClipBoundsContainer.Push(rectangleF);
		}
		else if (textBoxFormat.Rotation != 0f)
		{
			RectangleF empty = RectangleF.Empty;
			if (num != 0f && (textBoxFormat.AutoFit || textBoxFormat.WrappingMode == DocGen.DocIO.WrapMode.None || (textBox.IsShape && textBox.Shape.TextFrame.NoWrap)))
			{
				float width = textBoxFormat.Width;
				float height = textBoxFormat.Height;
				if (textBoxFormat.WrappingMode == DocGen.DocIO.WrapMode.None || (textBox.IsShape && textBox.Shape.TextFrame.NoWrap))
				{
					width = textBox.TextLayoutingBounds.Width + textBoxFormat.InternalMargin.Left + textBoxFormat.InternalMargin.Right;
				}
				if (textBoxFormat.AutoFit)
				{
					height = textBox.TextLayoutingBounds.Height + textBoxFormat.InternalMargin.Top + textBoxFormat.InternalMargin.Bottom;
				}
				empty = GetBoundingBoxCoordinates(new RectangleF(0f, 0f, width, height), num);
				ltWidget.Bounds = new RectangleF(ltWidget.Bounds.X - empty.X, ltWidget.Bounds.Y - empty.Y, width, height);
				textBox.TextLayoutingBounds = ltWidget.Bounds;
			}
			else
			{
				empty = GetBoundingBoxCoordinates(new RectangleF(0f, 0f, textBoxFormat.Width, textBoxFormat.Height), num);
				ltWidget.Bounds = new RectangleF(ltWidget.Bounds.X - empty.X, ltWidget.Bounds.Y - empty.Y, textBoxFormat.Width, textBoxFormat.Height);
				textBox.TextLayoutingBounds = ltWidget.Bounds;
			}
			if (!textBox.IsShape || !textBox.Shape.TextFrame.Upright || textBoxFormat.AutoFit)
			{
				ClipBoundsContainer.Pop();
				ClipBoundsContainer.Push(ltWidget.Bounds);
			}
		}
		bool flipHorizontal = textBoxFormat.FlipHorizontal;
		bool flipVertical = textBoxFormat.FlipVertical;
		if ((num != 0f || (num == 0f && (flipHorizontal || flipVertical))) && textBoxFormat.TextWrappingStyle != TextWrappingStyle.Tight && textBoxFormat.TextWrappingStyle != TextWrappingStyle.Through)
		{
			if (num > 360f)
			{
				num %= 360f;
			}
			if (num != 0f || flipVertical || flipHorizontal)
			{
				PDFGraphics.Transform = GetTransformMatrix(ltWidget.Bounds, num, flipHorizontal, flipVertical);
			}
		}
		if (textBoxFormat.FillEfects.Type == BackgroundType.NoBackground)
		{
			color = ((textBoxFormat.TextWrappingStyle != TextWrappingStyle.InFrontOfText) ? Color.Transparent : textBoxFormat.FillColor);
		}
		PdfPath pdfPath = null;
		if (textBoxFormat.VMLPathPoints == null)
		{
			if (textBoxFormat.FillEfects.Type == BackgroundType.Gradient)
			{
				color = textBoxFormat.FillEfects.Gradient.Color2;
				DrawTextureStyle(TextureStyle.Texture30Percent, textBoxFormat.TextThemeColor, color, ltWidget.Bounds);
			}
			else if (textBoxFormat.FillEfects.Type == BackgroundType.Picture && IsTexBoxHaveBackgroundPicture(textBox))
			{
				DrawPictureFill(textBoxFormat.FillEfects.ImageRecord, pdfPath, ltWidget.Bounds, textBox.IsShape ? textBox.Shape.FillFormat.BlipFormat.Transparency : 0f);
			}
			else
			{
				PDFGraphics.DrawRectangle(new PdfSolidBrush(color), ltWidget.Bounds);
			}
		}
		else
		{
			pdfPath = new ShapePath(ltWidget.Bounds, null).GetVMLCustomShapePath(textBoxFormat.VMLPathPoints);
			if (pdfPath.PointCount > 0)
			{
				if (textBoxFormat.FillEfects.Type == BackgroundType.Picture && IsTexBoxHaveBackgroundPicture(textBox))
				{
					DrawPictureFill(textBoxFormat.FillEfects.ImageRecord, pdfPath, ltWidget.Bounds, textBox.IsShape ? textBox.Shape.FillFormat.BlipFormat.Transparency : 0f);
				}
				else if (!color.IsEmpty)
				{
					PdfBrush brush = new PdfSolidBrush(color);
					PDFGraphics.DrawPath(brush, pdfPath);
				}
			}
		}
		borders.Color = textBoxFormat.LineColor;
		borders.BorderType = textBox.GetBordersStyle(textBoxFormat.LineStyle);
		LineDashing lineDashing = (textBox.IsShape ? textBox.Shape.LineFormat.DashStyle : textBoxFormat.LineDashing);
		if (!textBoxFormat.NoLine)
		{
			float num2 = textBoxFormat.LineWidth;
			if (textBoxFormat.LineStyle == TextBoxLineStyle.Double)
			{
				num2 /= 3f;
			}
			else if (textBoxFormat.LineStyle == TextBoxLineStyle.Triple)
			{
				num2 /= 5f;
			}
			borders.LineWidth = num2;
			borders.Color = textBoxFormat.LineColor;
			PdfPen pdfPen = GetPen(borders.Left, isParagraphBorder: false);
			if (textBox.Shape != null)
			{
				pdfPen.LineJoin = GetLineJoin(GetPdfLineJoin(textBox.Shape.LineFormat.LineJoin));
			}
			if (textBoxFormat.LineStyle == TextBoxLineStyle.Simple && lineDashing != 0)
			{
				pdfPen = GetDashStyle(lineDashing, pdfPen);
				pdfPen.StartCap = PdfLineCap.Flat;
				pdfPen.EndCap = PdfLineCap.Flat;
			}
			if (pdfPath != null && pdfPath.PointCount > 0)
			{
				PDFGraphics.DrawPath(pdfPen, pdfPath);
			}
			else if (AutoTag)
			{
				string alternateText = ((textBox.Shape != null) ? textBox.Shape.AlternativeText : null);
				string title = ((textBox.Shape != null) ? textBox.Shape.Title : null);
				PdfRectangle pdfRectangle = new PdfRectangle(ltWidget.Bounds);
				pdfRectangle.Pen = pdfPen;
				pdfRectangle.PdfTag = CreateAutoTag(PdfTagType.Figure, alternateText, title, IsOverLappedShapeWidget(ltWidget));
				pdfRectangle.Draw(PDFGraphics);
			}
			else
			{
				PDFGraphics.DrawRectangle(pdfPen, ltWidget.Bounds.X, ltWidget.Bounds.Y, ltWidget.Bounds.Width, ltWidget.Bounds.Height);
			}
		}
		PDFGraphics.ResetTransform();
		if (num != 0f && textBox.Shape != null && textBox.Shape.TextFrame.Upright)
		{
			textBox.TextLayoutingBounds = GetBoundingBoxCoordinates(ltWidget.Bounds, num);
		}
	}

	private RectangleF UpdateClipBounds(RectangleF clipBounds)
	{
		if (ClipBoundsContainer == null)
		{
			ClipBoundsContainer = new Stack<RectangleF>();
		}
		if (ClipBoundsContainer.Count > 0)
		{
			RectangleF rectangleF = ClipBoundsContainer.Peek();
			if (rectangleF.Height < clipBounds.Width)
			{
				clipBounds.Width = rectangleF.Height;
			}
			if (rectangleF.Width < clipBounds.Height)
			{
				clipBounds.Height = rectangleF.Width;
			}
		}
		return clipBounds;
	}

	private void DrawBarTabStop(WParagraph paragraph, LayoutedWidget ltWidget)
	{
		TabCollection tabs = paragraph.ParagraphFormat.Tabs;
		for (int i = 0; i < tabs.Count; i++)
		{
			if (tabs[i].Justification == DocGen.DocIO.DLS.TabJustification.Bar)
			{
				PointF point = new PointF(tabs[i].Position + ltWidget.Bounds.X, ltWidget.Bounds.Y);
				PointF point2 = new PointF(tabs[i].Position + ltWidget.Bounds.X, ltWidget.Bounds.Bottom + paragraph.ParagraphFormat.AfterSpacing);
				PdfPen pen = new PdfPen(new PdfColor(Color.Black));
				PDFGraphics.DrawLine(pen, point, point2);
			}
		}
	}

	private RectangleF GetBoundsToDrawParagraphBackGroundColor(WParagraph paragraph, LayoutedWidget ltWidget, bool isParagraphMarkIsHidden, bool isLineDrawing, ref bool resetTransform)
	{
		RectangleF bounds = ltWidget.Bounds;
		bool flag = false;
		bool flag2 = false;
		if (isLineDrawing && ltWidget.Owner.ChildWidgets.Count > 0)
		{
			LayoutedWidget owner = ltWidget.Owner;
			if (owner.ChildWidgets.Count == 1)
			{
				flag = true;
				flag2 = true;
			}
			else if (owner.ChildWidgets[0] == ltWidget)
			{
				flag = true;
			}
			else if (owner.ChildWidgets[owner.ChildWidgets.Count - 1] == ltWidget)
			{
				flag2 = true;
			}
			ltWidget = owner;
		}
		if (ltWidget.Owner != null && isParagraphMarkIsHidden && (!isLineDrawing || flag2))
		{
			AddNextParagraphBounds(ltWidget, ref bounds);
		}
		ParagraphLayoutInfo paragraphLayoutInfo = paragraph.m_layoutInfo as ParagraphLayoutInfo;
		if (ltWidget.ChildWidgets.Count > 0 && Math.Round(ltWidget.Bounds.Y, 2) != Math.Round(ltWidget.ChildWidgets[0].Bounds.Y, 2) && (!isLineDrawing || flag) && paragraphLayoutInfo != null)
		{
			bounds.Y = ltWidget.ChildWidgets[0].Bounds.Y - ((!paragraphLayoutInfo.SkipTopBorder) ? paragraph.ParagraphFormat.Borders.Top.Space : (paragraphLayoutInfo.SkipHorizonatalBorder ? 0f : paragraph.ParagraphFormat.Borders.Horizontal.Space));
			bounds.Height = ltWidget.Bounds.Bottom - bounds.Y;
		}
		bool flag3 = false;
		WParagraph wParagraph = paragraph.NextSibling as WParagraph;
		if (paragraphLayoutInfo != null && wParagraph != null && !paragraph.ParagraphFormat.Borders.NoBorder && paragraph.IsAdjacentParagraphHaveSameBorders(wParagraph, paragraphLayoutInfo.Margins.Left + ((paragraphLayoutInfo.FirstLineIndent > 0f) ? 0f : paragraphLayoutInfo.FirstLineIndent)) && !ltWidget.IsLastItemInPage && (!isLineDrawing || flag2))
		{
			bounds.Height += paragraphLayoutInfo.Margins.Bottom;
			flag3 = true;
		}
		Entity owner2 = paragraph.Owner;
		WSection wSection = null;
		if (!(owner2 is WSection))
		{
			while (owner2 != null && !(owner2 is WSection) && !(owner2 is WTableCell) && !(owner2 is WTextBox) && !(owner2 is Shape) && !(owner2 is WFootnote))
			{
				owner2 = owner2.Owner;
			}
		}
		float num = paragraphLayoutInfo.FirstLineIndent;
		if (num > 0f)
		{
			num = 0f;
		}
		IEntity previousSibling = paragraph.PreviousSibling;
		WParagraph wParagraph2 = ((previousSibling is WParagraph) ? (previousSibling as WParagraph) : null);
		if (wParagraph2 != null && !wParagraph2.ParagraphFormat.Borders.NoBorder && !wParagraph2.SectionEndMark && (!isLineDrawing || flag))
		{
			float num2 = 0f;
			if (paragraph.ParagraphFormat.Borders.Top.BorderType == BorderStyle.None && wParagraph2.ParagraphFormat.Borders.Bottom.BorderType != 0)
			{
				num2 += wParagraph2.ParagraphFormat.Borders.Bottom.LineWidth / 2f;
			}
			bounds.Y += num2;
			bounds.Height -= num2;
		}
		ILayoutSpacingsInfo layoutSpacingsInfo = ((ltWidget.Widget.LayoutInfo is ILayoutSpacingsInfo) ? (ltWidget.Widget.LayoutInfo as ILayoutSpacingsInfo) : null);
		float num3 = bounds.Height;
		IEntity nextSibling = paragraph.NextSibling;
		if (nextSibling != null && nextSibling is WParagraph && !(nextSibling as WParagraph).ParagraphFormat.BackColor.IsEmpty && !ltWidget.IsLastItemInPage && !flag3 && (!isLineDrawing || flag2))
		{
			num3 += layoutSpacingsInfo?.Margins.Bottom ?? 0f;
		}
		if (owner2 is WSection)
		{
			wSection = owner2 as WSection;
			float num4 = wSection.PageSetup.ClientWidth;
			float num5 = Layouter.GetLeftMargin(wSection);
			if (wSection.Columns.Count > 1)
			{
				int num6 = GetColumnIndex(wSection, ltWidget.Owner.Bounds);
				if (num6 > -1 && num6 < wSection.Columns.Count)
				{
					num4 = wSection.Columns[num6].Width;
					while (num6 > 0)
					{
						num5 += wSection.Columns[num6 - 1].Width + wSection.Columns[num6 - 1].Space;
						num6--;
					}
				}
			}
			if (paragraph.ParagraphFormat.IsInFrame())
			{
				RectangleF innerItemsRenderingBounds = GetInnerItemsRenderingBounds(ltWidget.ChildWidgets[0]);
				float width = ((paragraph.ParagraphFormat.FrameWidth != 0f) ? paragraph.ParagraphFormat.FrameWidth : ltWidget.Bounds.Width);
				float num7 = (paragraph.ParagraphFormat.IsNextParagraphInSameFrame() ? 0f : paragraph.ParagraphFormat.Borders.Bottom.GetLineWidthValue());
				float y = innerItemsRenderingBounds.Y;
				return new RectangleF(ltWidget.Bounds.X, y, width, ltWidget.Bounds.Height - num7);
			}
			Borders borders = paragraph.ParagraphFormat.Borders;
			float num8 = (paragraph.ParagraphFormat.Bidi ? paragraphLayoutInfo.Margins.Right : (paragraphLayoutInfo.Margins.Left + num)) - ((!paragraphLayoutInfo.SkipLeftBorder && !paragraphLayoutInfo.SkipRightBorder) ? borders.Right.Space : 0f);
			float num9 = (paragraph.ParagraphFormat.Bidi ? (paragraphLayoutInfo.Margins.Left + num) : paragraphLayoutInfo.Margins.Right) - ((!paragraphLayoutInfo.SkipRightBorder) ? borders.Right.Space : 0f);
			RectangleF result = new RectangleF(num5 + num8, bounds.Y, num4 - num8 - num9, num3);
			result.X -= 1.5f;
			result.Width += 3f;
			return result;
		}
		if (owner2 is WTableCell && paragraph.IsInCell)
		{
			CellLayoutInfo cellLayoutInfo = (owner2 as WTableCell).m_layoutInfo as CellLayoutInfo;
			LayoutedWidget ownerCellLayoutedWidget = GetOwnerCellLayoutedWidget(ltWidget);
			if (ownerCellLayoutedWidget != null)
			{
				if (paragraph.IsInCell && !paragraph.IsExactlyRowHeight() && paragraphLayoutInfo.BottomMargin > 0f && !(paragraph.ParagraphFormat.AfterSpacing > 0f))
				{
					bounds.Height -= paragraphLayoutInfo.BottomMargin;
				}
				float num10 = bounds.X + num + paragraphLayoutInfo.Paddings.Left;
				if (num10 <= ownerCellLayoutedWidget.Owner.Bounds.X)
				{
					num10 = ownerCellLayoutedWidget.Owner.Bounds.X + cellLayoutInfo.Paddings.Left;
				}
				float num11 = 0f;
				num11 = ((bounds.Right > ownerCellLayoutedWidget.Owner.Bounds.Right) ? (ownerCellLayoutedWidget.Owner.Bounds.Right - num10 - paragraphLayoutInfo.Margins.Right - paragraphLayoutInfo.Paddings.Left) : ((!(bounds.Right > ownerCellLayoutedWidget.Bounds.Right)) ? (ownerCellLayoutedWidget.Bounds.Right - num10 - paragraphLayoutInfo.Margins.Right - paragraphLayoutInfo.Paddings.Left) : (bounds.Right - num10 - paragraphLayoutInfo.Margins.Right - paragraphLayoutInfo.Paddings.Left)));
				float num12 = bounds.Y;
				if (num12 < ownerCellLayoutedWidget.Bounds.Top)
				{
					num12 = ownerCellLayoutedWidget.Bounds.Top;
				}
				float height = bounds.Bottom - num12;
				if (bounds.Bottom > ownerCellLayoutedWidget.Bounds.Bottom)
				{
					height = ownerCellLayoutedWidget.Bounds.Bottom - num12;
				}
				return new RectangleF(num10, num12, num11, height);
			}
			return bounds;
		}
		if (owner2 is WTextBox || owner2 is Shape)
		{
			if (ltWidget.Widget.LayoutInfo.IsVerticalText)
			{
				ResetTransform();
				resetTransform = true;
				PointF translatePoints = PointF.Empty;
				float rotationAngle = 0f;
				bool isRotateTransformApplied = false;
				TransformGraphicsPosition(ltWidget, isNeedToScale: false, ref isRotateTransformApplied, ref translatePoints, ref rotationAngle, paragraph);
			}
			else
			{
				for (LayoutedWidget owner3 = ltWidget.Owner; owner3 != null; owner3 = owner3.Owner)
				{
					if (owner3.Widget is WTextBox || owner3.Widget is Shape)
					{
						RectangleF rectangleF = owner3.Bounds;
						Shape shape = owner2 as Shape;
						WTextBox wTextBox = owner2 as WTextBox;
						rectangleF = new RectangleF(rectangleF.X, rectangleF.Y, rectangleF.Width - ((owner2 is Shape) ? shape.TextFrame.InternalMargin.Right : wTextBox.TextBoxFormat.InternalMargin.Right), rectangleF.Height - ((owner2 is Shape) ? shape.TextFrame.InternalMargin.Bottom : wTextBox.TextBoxFormat.InternalMargin.Bottom));
						if (rectangleF.Right > bounds.Right)
						{
							bounds = new RectangleF(bounds.X, bounds.Y, bounds.Width + (rectangleF.Right - bounds.Right), bounds.Height);
						}
						if ((flag2 || (!isLineDrawing && paragraph.NextSibling == null)) && rectangleF.Bottom > bounds.Bottom)
						{
							bounds = new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height + (rectangleF.Bottom - bounds.Bottom));
						}
						break;
					}
				}
			}
			return bounds;
		}
		return bounds;
	}

	internal bool IsParagraphContainingListHasBreak(LayoutedWidget ltWidget)
	{
		if (ltWidget != null && ltWidget.ChildWidgets.Count > 0)
		{
			for (int i = 0; i < ltWidget.ChildWidgets[0].ChildWidgets.Count; i++)
			{
				if (!(ltWidget.ChildWidgets[0].ChildWidgets[i].Widget is BookmarkStart) && !(ltWidget.ChildWidgets[0].ChildWidgets[i].Widget is BookmarkEnd))
				{
					if (!(ltWidget.ChildWidgets[0].ChildWidgets[i].Widget is Break) || (ltWidget.ChildWidgets[0].ChildWidgets[i].Widget as Break).BreakType == BreakType.LineBreak || (ltWidget.ChildWidgets[0].ChildWidgets[i].Widget as Break).BreakType == BreakType.TextWrappingBreak || GetOwnerParagraph(ltWidget).IsInCell)
					{
						break;
					}
					return true;
				}
			}
		}
		return false;
	}

	private Entity GetBaseEntity(Entity entity)
	{
		Entity entity2 = entity;
		do
		{
			if (entity2.Owner == null)
			{
				return entity2;
			}
			entity2 = entity2.Owner;
		}
		while (!(entity2 is WSection) && !(entity2 is HeaderFooter) && !(entity2 is WTextBox) && !(entity2 is Shape) && !(entity2 is GroupShape));
		return entity2;
	}

	private bool IsNeedtoUpdateTabPosition(LayoutedWidget widget, int index)
	{
		if (widget.ChildWidgets[index].Widget is Entity)
		{
			return !(widget.ChildWidgets[index].Widget as Entity).IsFloatingItem(isTextWrapAround: false);
		}
		return false;
	}

	public void UpdateTabPosition(LayoutedWidget widget, RectangleF clientArea)
	{
		if (widget == null)
		{
			return;
		}
		bool flag = false;
		if ((widget.Widget is WParagraph && widget.TextTag != "Splitted" && widget.ChildWidgets.Count > 0) || (widget.Widget is SplitWidgetContainer && (widget.Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph && widget.TextTag != "Splitted" && widget.ChildWidgets.Count > 0))
		{
			flag = true;
		}
		float num = 0f;
		int i = 0;
		bool flag2 = true;
		for (int j = 0; j < widget.ChildWidgets.Count; j++)
		{
			TabsLayoutInfo tabsLayoutInfo = widget.ChildWidgets[j].Widget.LayoutInfo as TabsLayoutInfo;
			if ((widget.ChildWidgets[j].PrevTabJustification == DocGen.Layouting.TabJustification.Right && tabsLayoutInfo == null && j != 0) || (tabsLayoutInfo != null && j < widget.ChildWidgets.Count - 1 && widget.ChildWidgets[j].PrevTabJustification == DocGen.Layouting.TabJustification.Right && widget.ChildWidgets[j + 1].Widget.LayoutInfo is TabsLayoutInfo))
			{
				if (tabsLayoutInfo != null)
				{
					for (; i <= j; i++)
					{
						if (widget.ChildWidgets[i].PrevTabJustification == DocGen.Layouting.TabJustification.Right && !(widget.ChildWidgets[i].Widget.LayoutInfo is TabsLayoutInfo) && IsNeedtoUpdateTabPosition(widget, i))
						{
							widget.ChildWidgets[i].Bounds = new RectangleF(widget.ChildWidgets[i].Bounds.X + num, widget.ChildWidgets[i].Bounds.Y, widget.ChildWidgets[i].Bounds.Width, widget.ChildWidgets[i].Bounds.Height);
						}
					}
					flag2 = true;
				}
				if (flag2)
				{
					i = j;
					flag2 = false;
					if (widget.ChildWidgets.Count > j - 1)
					{
						TabsLayoutInfo tabsLayoutInfo2 = ((tabsLayoutInfo != null) ? (widget.ChildWidgets[j].Widget.LayoutInfo as TabsLayoutInfo) : (widget.ChildWidgets[j - 1].Widget.LayoutInfo as TabsLayoutInfo));
						int index = ((tabsLayoutInfo != null) ? i : GetTabEndIndex(widget, i));
						if (tabsLayoutInfo2 != null)
						{
							float num2 = (float)tabsLayoutInfo2.PageMarginLeft;
							float num3 = widget.ChildWidgets[index].Bounds.Right - widget.ChildWidgets[i].Bounds.X;
							WParagraph ownerParagraph = GetOwnerParagraph(widget);
							double num4 = ((ownerParagraph.ParagraphFormat.RightIndent >= 0f && (double)(tabsLayoutInfo2.m_currTab.Position + num2) <= tabsLayoutInfo2.PageMarginRight) ? tabsLayoutInfo2.PageMarginRight : (tabsLayoutInfo2.PageMarginRight - (double)ownerParagraph.ParagraphFormat.RightIndent));
							float num5 = ((ownerParagraph != null && ownerParagraph.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013 && (double)(tabsLayoutInfo2.m_currTab.Position + num2) > num4) ? ((float)num4) : (tabsLayoutInfo2.m_currTab.Position + num2)) - widget.ChildWidgets[i].Bounds.X;
							if (num3 < num5)
							{
								num = num5 - num3;
								if (tabsLayoutInfo != null)
								{
									widget.ChildWidgets[i].Bounds = new RectangleF(widget.ChildWidgets[i].Bounds.X, widget.ChildWidgets[i].Bounds.Y, num, widget.ChildWidgets[i].Bounds.Height);
								}
								else
								{
									widget.ChildWidgets[i - 1].Bounds = new RectangleF(widget.ChildWidgets[i - 1].Bounds.X, widget.ChildWidgets[i - 1].Bounds.Y, num, widget.ChildWidgets[i - 1].Bounds.Height);
								}
								widget.Bounds = new RectangleF(widget.Bounds.X, widget.Bounds.Y, widget.Bounds.Width + num, widget.Bounds.Height);
							}
							else
							{
								num = 0f;
								tabsLayoutInfo2.TabWidth = 0f;
							}
						}
					}
				}
			}
			else
			{
				for (; i <= j; i++)
				{
					if (widget.ChildWidgets[i].PrevTabJustification == DocGen.Layouting.TabJustification.Right && !(widget.ChildWidgets[i].Widget.LayoutInfo is TabsLayoutInfo) && IsNeedtoUpdateTabPosition(widget, i))
					{
						widget.ChildWidgets[i].Bounds = new RectangleF(widget.ChildWidgets[i].Bounds.X + num, widget.ChildWidgets[i].Bounds.Y, widget.ChildWidgets[i].Bounds.Width, widget.ChildWidgets[i].Bounds.Height);
					}
					flag2 = true;
				}
			}
			if (j != widget.ChildWidgets.Count - 1)
			{
				continue;
			}
			for (; i <= j; i++)
			{
				if (widget.ChildWidgets[i].PrevTabJustification == DocGen.Layouting.TabJustification.Right && !(widget.ChildWidgets[i].Widget.LayoutInfo is TabsLayoutInfo) && IsNeedtoUpdateTabPosition(widget, i))
				{
					widget.ChildWidgets[i].Bounds = new RectangleF(widget.ChildWidgets[i].Bounds.X + num, widget.ChildWidgets[i].Bounds.Y, widget.ChildWidgets[i].Bounds.Width, widget.ChildWidgets[i].Bounds.Height);
				}
				flag2 = true;
			}
			if (widget.ChildWidgets[j].Widget.LayoutInfo is TabsLayoutInfo tabsLayoutInfo3 && tabsLayoutInfo3.m_currTab.Justification == DocGen.Layouting.TabJustification.Right)
			{
				widget.ChildWidgets[j].Bounds = new RectangleF(widget.ChildWidgets[j].Bounds.X, widget.ChildWidgets[j].Bounds.Y, tabsLayoutInfo3.m_currTab.Position + (float)tabsLayoutInfo3.PageMarginLeft - widget.ChildWidgets[j].Bounds.X, widget.ChildWidgets[j].Bounds.Height);
			}
		}
		bool flag3 = true;
		int k = 0;
		for (int l = 0; l < widget.ChildWidgets.Count; l++)
		{
			TabsLayoutInfo tabsLayoutInfo4 = widget.ChildWidgets[l].Widget.LayoutInfo as TabsLayoutInfo;
			if ((widget.ChildWidgets[l].PrevTabJustification == DocGen.Layouting.TabJustification.Centered && tabsLayoutInfo4 == null && l != 0) || (tabsLayoutInfo4 != null && l < widget.ChildWidgets.Count - 1 && widget.ChildWidgets[l].PrevTabJustification == DocGen.Layouting.TabJustification.Centered && widget.ChildWidgets[l + 1].Widget.LayoutInfo is TabsLayoutInfo))
			{
				if (tabsLayoutInfo4 != null)
				{
					for (; k <= l; k++)
					{
						if (widget.ChildWidgets[k].PrevTabJustification == DocGen.Layouting.TabJustification.Centered && !(widget.ChildWidgets[k].Widget.LayoutInfo is TabsLayoutInfo) && IsNeedtoUpdateTabPosition(widget, k))
						{
							widget.ChildWidgets[k].Bounds = new RectangleF(widget.ChildWidgets[k].Bounds.X + num, widget.ChildWidgets[k].Bounds.Y, widget.ChildWidgets[k].Bounds.Width, widget.ChildWidgets[k].Bounds.Height);
						}
					}
					flag3 = true;
				}
				if (flag3)
				{
					k = l;
					flag3 = false;
					if (widget.ChildWidgets.Count > l - 1)
					{
						TabsLayoutInfo tabsLayoutInfo5 = ((tabsLayoutInfo4 != null) ? (widget.ChildWidgets[l].Widget.LayoutInfo as TabsLayoutInfo) : (widget.ChildWidgets[l - 1].Widget.LayoutInfo as TabsLayoutInfo));
						int index2 = ((tabsLayoutInfo4 != null) ? k : GetTabEndIndex(widget, k));
						float num6 = (widget.ChildWidgets[index2].Bounds.Right - widget.ChildWidgets[k].Bounds.X) / 2f;
						if (tabsLayoutInfo5 != null)
						{
							float num7 = tabsLayoutInfo5.m_currTab.Position + (float)tabsLayoutInfo5.PageMarginLeft - widget.ChildWidgets[k].Bounds.X;
							WParagraph ownerParagraph2 = GetOwnerParagraph(widget);
							if (ownerParagraph2 != null && ((ownerParagraph2.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013 && (double)tabsLayoutInfo5.m_currTab.Position + tabsLayoutInfo5.PageMarginLeft > tabsLayoutInfo5.PageMarginRight) || (ownerParagraph2.ParagraphFormat.RightIndent == 0f && (double)tabsLayoutInfo5.m_currTab.Position + tabsLayoutInfo5.PageMarginLeft + (double)num6 > tabsLayoutInfo5.PageMarginRight)))
							{
								num7 = (float)(tabsLayoutInfo5.PageMarginRight - (double)ownerParagraph2.ParagraphFormat.RightIndent) - widget.ChildWidgets[k].Bounds.X;
								num6 *= 2f;
							}
							WParagraph ownerParagraph3 = GetOwnerParagraph(widget);
							if (num6 < num7 && (ownerParagraph3 == null || ownerParagraph3.ParagraphFormat.GetAlignmentToRender() != HorizontalAlignment.Justify || flag || widget.ChildWidgets[index2].Bounds.Right + num7 - num6 < (float)tabsLayoutInfo5.PageMarginRight))
							{
								num = num7 - num6;
								if (tabsLayoutInfo4 != null)
								{
									widget.ChildWidgets[k].Bounds = new RectangleF(widget.ChildWidgets[k].Bounds.X, widget.ChildWidgets[k].Bounds.Y, num, widget.ChildWidgets[k].Bounds.Height);
								}
								else
								{
									widget.ChildWidgets[k - 1].Bounds = new RectangleF(widget.ChildWidgets[k - 1].Bounds.X, widget.ChildWidgets[k - 1].Bounds.Y, num, widget.ChildWidgets[k - 1].Bounds.Height);
								}
							}
							else
							{
								tabsLayoutInfo5.TabWidth = 0f;
							}
						}
					}
				}
			}
			else
			{
				for (; k <= l; k++)
				{
					if (widget.ChildWidgets[k].PrevTabJustification == DocGen.Layouting.TabJustification.Centered && !(widget.ChildWidgets[k].Widget.LayoutInfo is TabsLayoutInfo) && IsNeedtoUpdateTabPosition(widget, k))
					{
						widget.ChildWidgets[k].Bounds = new RectangleF(widget.ChildWidgets[k].Bounds.X + num, widget.ChildWidgets[k].Bounds.Y, widget.ChildWidgets[k].Bounds.Width, widget.ChildWidgets[k].Bounds.Height);
					}
				}
				flag3 = true;
			}
			if (l != widget.ChildWidgets.Count - 1)
			{
				continue;
			}
			for (; k <= l; k++)
			{
				if (widget.ChildWidgets[k].PrevTabJustification == DocGen.Layouting.TabJustification.Centered && !(widget.ChildWidgets[k].Widget.LayoutInfo is TabsLayoutInfo) && IsNeedtoUpdateTabPosition(widget, k))
				{
					widget.ChildWidgets[k].Bounds = new RectangleF(widget.ChildWidgets[k].Bounds.X + num, widget.ChildWidgets[k].Bounds.Y, widget.ChildWidgets[k].Bounds.Width, widget.ChildWidgets[k].Bounds.Height);
				}
			}
			flag3 = true;
			if (widget.ChildWidgets[l].Widget.LayoutInfo is TabsLayoutInfo tabsLayoutInfo6 && tabsLayoutInfo6.m_currTab.Justification == DocGen.Layouting.TabJustification.Centered)
			{
				widget.ChildWidgets[l].Bounds = new RectangleF(widget.ChildWidgets[l].Bounds.X, widget.ChildWidgets[l].Bounds.Y, tabsLayoutInfo6.m_currTab.Position + (float)tabsLayoutInfo6.PageMarginLeft - widget.ChildWidgets[l].Bounds.X, widget.ChildWidgets[l].Bounds.Height);
			}
		}
		UpdateDecimalTabPosition(widget, clientArea);
		UpdateDecimalTabPositionInCell(widget, clientArea);
	}

	private void UpdateDecimalTabPosition(LayoutedWidget ltWidget, RectangleF clientArea)
	{
		bool flag = false;
		bool isDecimalTab = false;
		int num = 0;
		float num2 = 0f;
		for (int i = 0; i < ltWidget.ChildWidgets.Count; i++)
		{
			TabsLayoutInfo tabsLayoutInfo = ltWidget.ChildWidgets[i].Widget.LayoutInfo as TabsLayoutInfo;
			if (ltWidget.ChildWidgets[i].PrevTabJustification == DocGen.Layouting.TabJustification.Decimal && tabsLayoutInfo == null)
			{
				if (flag)
				{
					num = i;
					flag = false;
					isDecimalTab = true;
					num2 = GetWidthToShift(ltWidget, num, isInCell: false, clientArea);
					ltWidget.ChildWidgets[num - 1].Bounds = new RectangleF(ltWidget.ChildWidgets[num - 1].Bounds.X, ltWidget.ChildWidgets[num - 1].Bounds.Y, num2, ltWidget.ChildWidgets[num - 1].Bounds.Height);
				}
			}
			else
			{
				flag = IsDecimalTabStart(ltWidget, num, isDecimalTab, i, num2, isInCell: false);
				isDecimalTab = false;
			}
			if (i == ltWidget.ChildWidgets.Count - 1)
			{
				flag = IsDecimalTabStart(ltWidget, num, isDecimalTab, i, num2, isInCell: false);
				if (ltWidget.ChildWidgets[i].Widget.LayoutInfo is TabsLayoutInfo tabsLayoutInfo2 && tabsLayoutInfo2.m_currTab.Justification == DocGen.Layouting.TabJustification.Decimal)
				{
					ltWidget.ChildWidgets[i].Bounds = new RectangleF(ltWidget.ChildWidgets[i].Bounds.X, ltWidget.ChildWidgets[i].Bounds.Y, tabsLayoutInfo2.m_currTab.Position + (float)tabsLayoutInfo2.PageMarginLeft - ltWidget.ChildWidgets[i].Bounds.X, ltWidget.ChildWidgets[i].Bounds.Height);
				}
			}
		}
	}

	private void UpdateDecimalTabPositionInCell(LayoutedWidget ltWidget, RectangleF clientArea)
	{
		int decimalTabStart = 0;
		float num = 0f;
		WParagraph ownerParagraph = GetOwnerParagraph(ltWidget);
		if (ownerParagraph == null)
		{
			return;
		}
		for (int i = 0; i < ltWidget.ChildWidgets.Count; i++)
		{
			if (ownerParagraph.IsInCell && ownerParagraph.ParagraphFormat.Tabs.Count == 1 && ownerParagraph.ParagraphFormat.Tabs[0].Justification == DocGen.DocIO.DLS.TabJustification.Decimal)
			{
				num = GetWidthToShift(ltWidget, decimalTabStart, isInCell: true, clientArea);
				IsDecimalTabStart(ltWidget, i, isDecimalTab: false, i, num, isInCell: true);
			}
		}
	}

	private bool IsDecimalTabStart(LayoutedWidget ltWidget, int decimalTabStart, bool isDecimalTab, int i, float widthToShift, bool isInCell)
	{
		while (decimalTabStart <= i)
		{
			if (isInCell)
			{
				ltWidget.ChildWidgets[decimalTabStart].Bounds = new RectangleF(ltWidget.ChildWidgets[decimalTabStart].Bounds.X + widthToShift, ltWidget.ChildWidgets[decimalTabStart].Bounds.Y, ltWidget.ChildWidgets[decimalTabStart].Bounds.Width, ltWidget.ChildWidgets[decimalTabStart].Bounds.Height);
			}
			else if (ltWidget.ChildWidgets[decimalTabStart].PrevTabJustification == DocGen.Layouting.TabJustification.Decimal && !(ltWidget.ChildWidgets[decimalTabStart].Widget.LayoutInfo is TabsLayoutInfo) && isDecimalTab)
			{
				ltWidget.ChildWidgets[decimalTabStart].Bounds = new RectangleF(ltWidget.ChildWidgets[decimalTabStart].Bounds.X + widthToShift, ltWidget.ChildWidgets[decimalTabStart].Bounds.Y, ltWidget.ChildWidgets[decimalTabStart].Bounds.Width, ltWidget.ChildWidgets[decimalTabStart].Bounds.Height);
			}
			decimalTabStart++;
		}
		return true;
	}

	private float GetWidthToShift(LayoutedWidget ltWidget, int decimalTabStart, bool isInCell, RectangleF clientArea)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		int tabEndIndex = GetTabEndIndex(ltWidget, decimalTabStart);
		WParagraph ownerParagraph = GetOwnerParagraph(ltWidget);
		float leftWidth = GetLeftWidth(ltWidget, decimalTabStart, tabEndIndex);
		WParagraphFormat currentTabFormat = GetCurrentTabFormat(ownerParagraph);
		float num4 = 0f;
		if (currentTabFormat != null && currentTabFormat.Tabs.Count > 0)
		{
			num4 = currentTabFormat.Tabs[0].Position;
		}
		float num5 = 0f;
		for (int i = decimalTabStart; i <= tabEndIndex; i++)
		{
			num2 += ltWidget.ChildWidgets[i].Bounds.Width;
		}
		if (isInCell)
		{
			num3 = clientArea.Width;
			num5 = clientArea.X;
		}
		else
		{
			num3 = ((GetColumnWidth(ownerParagraph) > num4) ? GetColumnWidth(ownerParagraph) : 1584f);
			if (ltWidget.ChildWidgets[decimalTabStart - 1].Widget.LayoutInfo is TabsLayoutInfo tabsLayoutInfo)
			{
				num5 = (float)tabsLayoutInfo.PageMarginLeft;
				num4 = ((ownerParagraph != null && ownerParagraph.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013 && (double)tabsLayoutInfo.m_currTab.Position + tabsLayoutInfo.PageMarginLeft > tabsLayoutInfo.PageMarginRight) ? ((float)(tabsLayoutInfo.PageMarginRight - (double)ownerParagraph.ParagraphFormat.RightIndent)) : (tabsLayoutInfo.m_currTab.Position + num5)) - ltWidget.ChildWidgets[decimalTabStart].Bounds.X;
			}
		}
		if (leftWidth < num4)
		{
			if (num2 - leftWidth < num3 - num4)
			{
				num = num4 - leftWidth;
				float num6 = ltWidget.ChildWidgets[decimalTabStart].Bounds.X - num5;
				if (!isInCell && num3 < num6 + num4 + num2 - leftWidth)
				{
					float num7 = num6 + num4 + num2 - leftWidth - num3;
					num -= num7;
				}
			}
			else
			{
				num = num3 - num2;
			}
		}
		return num;
	}

	public WParagraphFormat GetCurrentTabFormat(WParagraph paragraph)
	{
		WParagraphFormat wParagraphFormat = paragraph.ParagraphFormat;
		while (wParagraphFormat != null && wParagraphFormat.Tabs.Count <= 0)
		{
			wParagraphFormat = wParagraphFormat.BaseFormat as WParagraphFormat;
		}
		return wParagraphFormat;
	}

	private float GetColumnWidth(WParagraph paragraph)
	{
		Entity entity = paragraph;
		float result = 0f;
		while (!(entity is WSection) && entity != null)
		{
			entity = entity.Owner;
		}
		if (entity is WSection && paragraph.m_layoutInfo is ParagraphLayoutInfo)
		{
			result = (entity as WSection).PageSetup.ClientWidth - (paragraph.m_layoutInfo as ParagraphLayoutInfo).Margins.Right;
		}
		return result;
	}

	public float GetLeftWidth(WParagraph paragraph, int decimalTabStart, int decimalTabEnd)
	{
		float leftWidth = 0f;
		if (paragraph.ChildEntities.Count != 0)
		{
			int num = 0;
			int decimalSeparator = 0;
			bool isSeparator = false;
			num = GetIndexOfDecimalseparator(paragraph, decimalTabStart, decimalTabEnd, ref leftWidth, ref decimalSeparator, ref isSeparator);
			if (paragraph.ChildEntities[num] is WTextRange && isSeparator)
			{
				string[] array = (paragraph.ChildEntities[num] as WTextRange).Text.Split((char)decimalSeparator);
				SizeF sizeF = MeasureTextRange(paragraph.ChildEntities[num] as WTextRange, array[0]);
				leftWidth += sizeF.Width;
			}
		}
		return leftWidth;
	}

	internal float GetLeftWidth(LayoutedWidget ltWidget, int decimalTabStart, int decimalTabEnd)
	{
		float leftWidth = 0f;
		if (ltWidget.ChildWidgets.Count != 0)
		{
			int num = 0;
			int decimalSeparator = 0;
			bool isSeparator = false;
			num = GetIndexOfDecimalseparator(ltWidget, decimalTabStart, decimalTabEnd, ref leftWidth, ref decimalSeparator, ref isSeparator);
			if (ltWidget.ChildWidgets[num].Widget is WTextRange && isSeparator)
			{
				string[] array = (ltWidget.ChildWidgets[num].Widget as WTextRange).Text.Split((char)decimalSeparator);
				SizeF sizeF = MeasureTextRange(ltWidget.ChildWidgets[num].Widget as WTextRange, array[0]);
				leftWidth += sizeF.Width;
			}
			else if (ltWidget.ChildWidgets[num].Widget is SplitStringWidget && isSeparator)
			{
				string[] array2 = (ltWidget.ChildWidgets[num].Widget as SplitStringWidget).SplittedText.Split((char)decimalSeparator);
				SizeF sizeF2 = MeasureTextRange((ltWidget.ChildWidgets[num].Widget as SplitStringWidget).RealStringWidget as WTextRange, array2[0]);
				leftWidth += sizeF2.Width;
			}
		}
		return leftWidth;
	}

	private int GetIndexOfDecimalseparator(WParagraph paragraph, int decimalTabStart, int decimalTabEnd, ref float leftWidth, ref int decimalSeparator, ref bool isSeparator)
	{
		char c = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
		bool flag = false;
		bool flag2 = false;
		int result = 0;
		bool isPrevTextHasNumber = false;
		for (int i = decimalTabStart; i <= decimalTabEnd; i++)
		{
			if (!(paragraph.ChildEntities[i] is WTextRange) || (paragraph.ChildEntities[i] as IWidget).LayoutInfo.IsSkip)
			{
				continue;
			}
			decimalSeparator = 0;
			char[] array = (paragraph.ChildEntities[i] as WTextRange).Text.ToCharArray();
			for (int j = 0; j < array.Length; j++)
			{
				if (char.IsNumber(array[j]))
				{
					flag2 = true;
					break;
				}
			}
			if (flag2)
			{
				flag = IsDecimalSeparator(array, ref decimalSeparator, isPrevTextHasNumber);
			}
			if (!flag2 && !flag)
			{
				if ((paragraph.ChildEntities[i] as WTextRange).Text.Contains(c.ToString()))
				{
					flag = true;
				}
				decimalSeparator = c;
			}
			if (!flag)
			{
				((IWidget)(paragraph.ChildEntities[i] as WTextRange)).LayoutInfo.Size = (paragraph.ChildEntities[i] as WTextRange).GetTextRangeSize(null);
				leftWidth += ((IWidget)(paragraph.ChildEntities[i] as WTextRange)).LayoutInfo.Size.Width;
				isPrevTextHasNumber = flag2;
				continue;
			}
			result = i;
			isSeparator = true;
			break;
		}
		return result;
	}

	private int GetIndexOfDecimalseparator(LayoutedWidget ltWidget, int decimalTabStart, int decimalTabEnd, ref float leftWidth, ref int decimalSeparator, ref bool isSeparator)
	{
		char c = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
		bool flag = false;
		bool flag2 = false;
		int result = decimalTabStart;
		bool isPrevTextHasNumber = false;
		for (int i = decimalTabStart; i <= decimalTabEnd; i++)
		{
			if (!(ltWidget.ChildWidgets[i].Widget is WTextRange) && !(ltWidget.ChildWidgets[i].Widget is SplitStringWidget))
			{
				continue;
			}
			bool flag3 = ltWidget.ChildWidgets[i].Widget is WTextRange;
			decimalSeparator = 0;
			char[] array = (flag3 ? (ltWidget.ChildWidgets[i].Widget as WTextRange).Text.ToCharArray() : (ltWidget.ChildWidgets[i].Widget as SplitStringWidget).SplittedText.ToCharArray());
			for (int j = 0; j < array.Length; j++)
			{
				if (char.IsNumber(array[j]))
				{
					flag2 = true;
					break;
				}
			}
			if (flag2)
			{
				flag = IsDecimalSeparator(array, ref decimalSeparator, isPrevTextHasNumber);
			}
			if (!flag2 && !flag)
			{
				flag = ((!flag3) ? (ltWidget.ChildWidgets[i].Widget as SplitStringWidget).SplittedText.Contains(c.ToString()) : (ltWidget.ChildWidgets[i].Widget as WTextRange).Text.Contains(c.ToString()));
				decimalSeparator = c;
			}
			if (!flag)
			{
				leftWidth += (flag3 ? ((IWidget)(ltWidget.ChildWidgets[i].Widget as WTextRange)).LayoutInfo.Size : MeasureTextRange((ltWidget.ChildWidgets[i].Widget as SplitStringWidget).RealStringWidget as WTextRange, (ltWidget.ChildWidgets[i].Widget as SplitStringWidget).SplittedText)).Width;
				isPrevTextHasNumber = flag2;
				continue;
			}
			result = i;
			isSeparator = true;
			break;
		}
		return result;
	}

	private bool IsDecimalSeparator(char[] ch, ref int decimalSeparator, bool isPrevTextHasNumber)
	{
		int num = 8217;
		int num2 = 8221;
		for (int i = 0; i < ch.Length; i++)
		{
			int num3 = ch[i];
			if (!char.IsNumber(ch[i]) && ((num3 > 31 && num3 < 127) || num3 == num || num3 == num2) && ((!isPrevTextHasNumber && (num3 == CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator[0] || (i > 0 && char.IsNumber(ch[i - 1])))) || isPrevTextHasNumber) && num3 != CultureInfo.CurrentCulture.NumberFormat.CurrencyGroupSeparator[0])
			{
				decimalSeparator = num3;
				return true;
			}
		}
		return false;
	}

	private WParagraph GetOwnerParagraph(LayoutedWidget ltWidget)
	{
		WParagraph result = null;
		if (ltWidget.Widget != null)
		{
			if (ltWidget.Widget is WParagraph)
			{
				result = ltWidget.Widget as WParagraph;
			}
			else if (ltWidget.Widget is SplitWidgetContainer && (ltWidget.Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph)
			{
				result = (ltWidget.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph;
			}
		}
		return result;
	}

	private int GetTabEndIndex(LayoutedWidget ltWidget, int startIndex)
	{
		for (int i = startIndex; i < ltWidget.ChildWidgets.Count; i++)
		{
			if (ltWidget.ChildWidgets[i].Widget.LayoutInfo is TabsLayoutInfo)
			{
				return i - 1;
			}
		}
		return ltWidget.ChildWidgets.Count - 1;
	}

	public float GetListValue(WParagraph paragraph, ParagraphLayoutInfo paragraphInfo, WListFormat listFormat)
	{
		float num = 0f;
		num = 0f - Math.Abs(paragraphInfo.ListTab);
		SizeF sizeF = default(SizeF);
		DocGen.Drawing.Font font = paragraphInfo.ListFont.GetFont(paragraph.Document, FontScriptType.English);
		WListLevel listLevel = paragraph.GetListLevel(listFormat);
		if (paragraphInfo.CurrentListType == ListType.Bulleted && listLevel.PicBullet != null)
		{
			return MeasurePictureBulletSize(listLevel.PicBullet, font).Width;
		}
		sizeF = MeasureString(paragraphInfo.ListValue, font, null, paragraphInfo.CharacterFormat, isMeasureFromTabList: true, FontScriptType.English);
		if (paragraphInfo.ListAlignment == ListNumberAlignment.Center)
		{
			num -= sizeF.Width / 2f;
		}
		else if (paragraphInfo.ListAlignment == ListNumberAlignment.Right)
		{
			num -= sizeF.Width;
		}
		if (paragraph.ParagraphFormat.Bidi)
		{
			num = 0f - num;
		}
		return num;
	}

	internal void DrawList(WParagraph paragraph, LayoutedWidget ltWidget, WListFormat listFormat)
	{
		if (!(ltWidget.Widget.LayoutInfo is ParagraphLayoutInfo paragraphLayoutInfo) || ltWidget.ChildWidgets.Count == 0 || paragraphLayoutInfo.ListValue == string.Empty)
		{
			return;
		}
		float num = 0f - Math.Abs(paragraphLayoutInfo.ListTab);
		bool flag = false;
		DocGen.Drawing.Font font = paragraphLayoutInfo.ListFont.GetFont(paragraph.Document, FontScriptType.English);
		WListLevel listLevel = paragraph.GetListLevel(listFormat);
		SizeF sizeF;
		if (paragraphLayoutInfo.CurrentListType == ListType.Bulleted && listLevel.PicBullet != null)
		{
			flag = true;
			sizeF = MeasurePictureBulletSize(listLevel.PicBullet, font);
		}
		else
		{
			sizeF = MeasureString(paragraphLayoutInfo.ListValue, font, null, paragraphLayoutInfo.CharacterFormat, isMeasureFromTabList: true, FontScriptType.English);
		}
		if (paragraphLayoutInfo.ListAlignment == ListNumberAlignment.Center)
		{
			num -= sizeF.Width / 2f;
		}
		else if (paragraphLayoutInfo.ListAlignment == ListNumberAlignment.Right)
		{
			num -= sizeF.Width;
		}
		if (paragraph.ParagraphFormat.Bidi)
		{
			num = 0f - num;
		}
		float num2 = 0f;
		if (ltWidget.ChildWidgets[0].ChildWidgets.Count > 0)
		{
			foreach (LayoutedWidget childWidget in ltWidget.ChildWidgets[0].ChildWidgets)
			{
				if (num2 < childWidget.Bounds.Right)
				{
					num2 = childWidget.Bounds.Right;
				}
			}
		}
		else
		{
			num2 = ltWidget.ChildWidgets[0].Bounds.Right;
		}
		float x = (paragraph.ParagraphFormat.Bidi ? (num2 + (num - sizeF.Width)) : (ltWidget.ChildWidgets[0].Bounds.X + num));
		string listValue = paragraphLayoutInfo.ListValue;
		if (ltWidget.ChildWidgets[0].ChildWidgets.Count > 0)
		{
			int index = 0;
			if (ltWidget.ChildWidgets[0].ChildWidgets.Count > 1 && paragraph.IsLineNumbersEnabled())
			{
				index = 1;
			}
			IWidget widget = ltWidget.ChildWidgets[0].ChildWidgets[index].Widget;
			if (!(widget is WTextRange) && !(widget is SplitStringWidget) && ((!(widget is WPicture) && !(widget is Shape) && !(widget is WTextBox) && !(widget is WChart) && !(widget is GroupShape)) || (widget as ParagraphItem).GetTextWrappingStyle() != 0))
			{
				for (int i = 1; i < ltWidget.ChildWidgets[0].ChildWidgets.Count; i++)
				{
					IWidget widget2 = ltWidget.ChildWidgets[0].ChildWidgets[i].Widget;
					if (widget2 is WTextRange || widget2 is SplitStringWidget || ((widget2 is WPicture || widget2 is Shape || widget2 is WTextBox || widget2 is WChart || widget2 is GroupShape) && (widget2 as ParagraphItem).GetTextWrappingStyle() == TextWrappingStyle.Inline))
					{
						x = (paragraph.ParagraphFormat.Bidi ? (ltWidget.ChildWidgets[0].ChildWidgets[i].Bounds.Right + (num - sizeF.Width)) : (ltWidget.ChildWidgets[0].ChildWidgets[i].Bounds.X + num));
						break;
					}
				}
			}
			else
			{
				x = (paragraph.ParagraphFormat.Bidi ? (num2 + (num - sizeF.Width)) : (ltWidget.ChildWidgets[0].ChildWidgets[index].Bounds.X + num));
			}
		}
		float num3 = 0f;
		if (paragraphLayoutInfo.ListYPositions.Count >= 1)
		{
			num3 = paragraphLayoutInfo.ListYPositions[0];
			paragraphLayoutInfo.ListYPositions.RemoveAt(0);
			if (paragraph.IsContainDinOffcFont())
			{
				num3 += sizeF.Height * 0.1f;
			}
		}
		IsListCharacter = true;
		if (flag && listLevel.PicBullet.GetImage(listLevel.PicBullet.ImageBytes, isImageFromScratch: false) != null)
		{
			bool num4 = listLevel.PicBullet.Image != null && listLevel.PicBullet.Image.RawFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.Tiff);
			MemoryStream memoryStream = new MemoryStream(listLevel.PicBullet.ImageBytes);
			PdfImage pdfImage = null;
			pdfImage = ((!num4) ? GetPdfImage(memoryStream) : PDFGraphics.GetImage(memoryStream));
			memoryStream.Dispose();
			memoryStream = null;
			if (AutoTag)
			{
				pdfImage.PdfTag = CreateAutoTag(PdfTagType.Paragraph, null, null, isOverlapedShape: false);
			}
			PDFGraphics.DrawImage(pdfImage, new RectangleF(x, num3, sizeF.Width, sizeF.Height));
			IsListCharacter = false;
		}
		else
		{
			DrawString(FontScriptType.English, listValue, paragraphLayoutInfo.CharacterFormat, paragraph.ParagraphFormat, new RectangleF(x, num3, Math.Abs(num), sizeF.Height), Math.Abs(num), ltWidget);
		}
		if (paragraphLayoutInfo.ListTabStop != null && paragraphLayoutInfo.ListTabStop.TabLeader != 0)
		{
			DrawListTabLeader(paragraph, paragraphLayoutInfo, paragraphLayoutInfo.ListTab + sizeF.Width + num, ltWidget.ChildWidgets[0].Bounds.X, num3);
		}
	}

	private void DrawListTabLeader(WParagraph paragraph, ParagraphLayoutInfo paragraphInfo, float listWidth, float xPosition, float yPosition)
	{
		string tabLeader = GetTabLeader(paragraphInfo);
		if (tabLeader != string.Empty)
		{
			float width = MeasureString(tabLeader, paragraphInfo.ListFont.GetFont(paragraph.Document, FontScriptType.English), null, paragraphInfo.CharacterFormat, isMeasureFromTabList: true, isMeasureFromSmallCapString: false, FontScriptType.English).Width;
			float num = paragraphInfo.ListTab - (float)Math.Ceiling(listWidth / width) * width;
			string text = string.Empty;
			int num2 = (int)Math.Floor(num / width);
			for (int i = 0; i < num2; i++)
			{
				text += tabLeader;
			}
			SizeF sizeF = MeasureString(text, paragraphInfo.ListFont.GetFont(paragraph.Document, FontScriptType.English), null, paragraphInfo.CharacterFormat, isMeasureFromTabList: true, isMeasureFromSmallCapString: false, FontScriptType.English);
			DrawString(FontScriptType.English, text, paragraphInfo.CharacterFormat, paragraph.ParagraphFormat, new RectangleF(xPosition - num, yPosition, sizeF.Width, sizeF.Height), sizeF.Width, null);
		}
	}

	private string GetTabLeader(ParagraphLayoutInfo paragraphInfo)
	{
		string result = string.Empty;
		switch (paragraphInfo.ListTabStop.TabLeader)
		{
		case DocGen.Layouting.TabLeader.Dotted:
			result = ".";
			break;
		case DocGen.Layouting.TabLeader.Single:
			result = "_";
			break;
		case DocGen.Layouting.TabLeader.Hyphenated:
			result = "-";
			break;
		}
		return result;
	}

	public float GetAscentValueForEQField(WField field)
	{
		WCharacterFormat characterFormatValue = field.GetCharacterFormatValue();
		float result = GetAscent(characterFormatValue.GetFontToRender(field.ScriptType), field.ScriptType);
		for (int i = 0; i < DocumentLayouter.EquationFields.Count; i++)
		{
			if (DocumentLayouter.EquationFields[i].EQFieldEntity == field)
			{
				result = 0f - DocumentLayouter.EquationFields[i].LayouttedEQField.Bounds.Y;
				break;
			}
		}
		return result;
	}

	internal float IsLineContainsEQfield(LayoutedWidget ltWidget)
	{
		for (int i = 0; i < ltWidget.ChildWidgets.Count; i++)
		{
			LayoutedWidget layoutedWidget = ltWidget.ChildWidgets[i];
			if (layoutedWidget.Widget is WField && (layoutedWidget.Widget as WField).FieldType == FieldType.FieldExpression)
			{
				WCharacterFormat charFormat = (layoutedWidget.Widget as WField).GetCharFormat();
				FontScriptType scriptType = (layoutedWidget.Widget as WField).ScriptType;
				float num = MeasureString(" ", charFormat.GetFontToRender(scriptType), null, charFormat, isMeasureFromTabList: false, scriptType).Height - GetAscent(charFormat.GetFontToRender(scriptType), scriptType);
				return ltWidget.ChildWidgets[i].Bounds.Height - (GetAscentValueForEQField(layoutedWidget.Widget as WField) + num);
			}
		}
		return float.MinValue;
	}

	internal bool IsEmptyParagraph(WParagraph para)
	{
		bool result = false;
		if (para != null && para.Text == "" && para.Items.Count == 0)
		{
			result = true;
		}
		return result;
	}

	private void AddCommentMark(WCommentMark commentMark, LayoutedWidget ltWidget)
	{
		RevisionOptions revisionOptions = commentMark.Document.RevisionOptions;
		GetRevisionColor(revisionOptions.CommentColor);
		if (m_commentMarks == null)
		{
			m_commentMarks = new List<PointF[]>();
		}
		float height = ltWidget.Bounds.Height;
		if (height == 0f && commentMark.OwnerParagraph != null)
		{
			WCharacterFormat breakCharacterFormat = commentMark.OwnerParagraph.BreakCharacterFormat;
			height = MeasureString(" ", breakCharacterFormat.GetFontToRender(FontScriptType.English), null, FontScriptType.English).Height;
		}
		PointF pointF = new PointF(ltWidget.Bounds.X, ltWidget.Bounds.Y);
		PointF pointF2 = new PointF(ltWidget.Bounds.X, ltWidget.Bounds.Y + height);
		float num = 0.3f;
		PointF[] array = new PointF[4];
		if (commentMark.Type == CommentMarkType.CommentStart)
		{
			array[0] = new PointF(pointF.X + num, pointF.Y - num);
			array[1] = pointF;
			array[2] = pointF2;
			array[3] = new PointF(pointF2.X + num, pointF2.Y + num);
			if (m_commentId == null)
			{
				m_commentId = commentMark.CommentId;
			}
		}
		else
		{
			array[0] = new PointF(pointF.X - num, pointF.Y - num);
			array[1] = pointF;
			array[2] = pointF2;
			array[3] = new PointF(pointF2.X - num, pointF2.Y + num);
			if (m_commentId == commentMark.CommentId)
			{
				m_commentId = null;
			}
		}
		m_commentMarks.Add(array);
	}

	private void DrawCommentMarks(RevisionOptions revisionOptions)
	{
		Color revisionColor = GetRevisionColor(revisionOptions.CommentColor);
		foreach (PointF[] commentMark in m_commentMarks)
		{
			PdfPen pen = new PdfPen(new PdfColor(revisionColor), 0.2f);
			PdfPath pdfPath = new PdfPath();
			pdfPath.AddLines(commentMark);
			PDFGraphics.DrawPath(pen, pdfPath);
		}
	}

	internal void DrawAbsoluteTab(WAbsoluteTab absoluteTab, LayoutedWidget ltWidget)
	{
		float left = ltWidget.Bounds.Left;
		float top = ltWidget.Bounds.Top;
		float height = ltWidget.Bounds.Height;
		StringFormat stringFormat = new StringFormat(StringFormt);
		if (absoluteTab.CharacterFormat.Bidi)
		{
			stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
		}
		else
		{
			stringFormat.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;
		}
		string text = string.Empty;
		if (absoluteTab.m_layoutInfo is TabsLayoutInfo && absoluteTab.Alignment != 0)
		{
			UpdateAbsoluteTabLeader(absoluteTab, ltWidget, ref text);
		}
		DrawString(bounds: new RectangleF(left, top, MeasureString(text, absoluteTab.CharacterFormat.GetFontToRender(FontScriptType.English), stringFormat, FontScriptType.English).Width + ltWidget.SubWidth, height), scriptType: FontScriptType.English, text: text, charFormat: absoluteTab.CharacterFormat, paraFormat: absoluteTab.GetOwnerParagraphValue().ParagraphFormat, clipWidth: ltWidget.Bounds.Width, ltWidget: ltWidget);
	}

	private void UpdateAbsoluteTabLeader(WAbsoluteTab absoluteTab, LayoutedWidget ltWidget, ref string text)
	{
		TabsLayoutInfo tabsLayoutInfo = absoluteTab.m_layoutInfo as TabsLayoutInfo;
		text = string.Empty;
		StringFormat stringFormat = new StringFormat(StringFormt);
		WCharacterFormat characterFormat = absoluteTab.CharacterFormat;
		if (characterFormat.Bidi)
		{
			stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
		}
		else
		{
			stringFormat.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;
		}
		new WTextRange(absoluteTab.Document).ApplyCharacterFormat(characterFormat);
		if (characterFormat.GetFontToRender(FontScriptType.English).Underline || characterFormat.GetFontToRender(FontScriptType.English).Strikeout)
		{
			FillSpace(absoluteTab.m_layoutInfo.Font.GetFont(absoluteTab.Document, FontScriptType.English), ltWidget, stringFormat, ref text);
		}
		if (tabsLayoutInfo != null)
		{
			switch (tabsLayoutInfo.CurrTabLeader)
			{
			case DocGen.Layouting.TabLeader.Dotted:
				FillDots(absoluteTab.m_layoutInfo.Font.GetFont(absoluteTab.Document, FontScriptType.English), ltWidget, characterFormat, stringFormat, ref text);
				break;
			case DocGen.Layouting.TabLeader.Single:
				FillSingle(absoluteTab.m_layoutInfo.Font.GetFont(absoluteTab.Document, FontScriptType.English), ltWidget, characterFormat, stringFormat, ref text);
				break;
			case DocGen.Layouting.TabLeader.Hyphenated:
				FillHyphens(absoluteTab.m_layoutInfo.Font.GetFont(absoluteTab.Document, FontScriptType.English), ltWidget, characterFormat, stringFormat, ref text);
				break;
			}
		}
	}

	internal void DrawSeparator(WTextRange txtRange, LayoutedWidget ltWidget)
	{
		Color color = ((txtRange.CharacterFormat.TextColor != Color.Empty) ? txtRange.CharacterFormat.TextColor : Color.Black);
		if (AutoTag)
		{
			PdfArtifact tag = new PdfArtifact(PdfArtifactType.Pagination, ltWidget.Bounds, new PdfAttached(PdfEdge.Bottom), PdfArtifactSubType.Footer);
			PDFGraphics.SetTag(tag);
		}
		PdfPen pen = new PdfPen(new PdfColor(color), 0.5f);
		PDFGraphics.DrawLine(pen, new PointF(ltWidget.Bounds.X, ltWidget.Bounds.Y + ltWidget.Bounds.Height / 2f), new PointF(ltWidget.Bounds.Right, ltWidget.Bounds.Y + ltWidget.Bounds.Height / 2f));
		if (AutoTag)
		{
			PDFGraphics.ReSetTag();
		}
	}

	internal void DrawEditableTextRange(WTextRange txtRange, LayoutedWidget ltWidget)
	{
		WTextFormField wTextFormField = (m_editableTextFormTextRange.PreviousSibling as WFieldMark).ParentField as WTextFormField;
		PdfTextBoxField pdfTextBoxField = new PdfTextBoxField(PDFGraphics.Page, wTextFormField.Name);
		pdfTextBoxField.Text = ((m_editableTextFormTextRange.PreviousSibling as WFieldMark).ParentField as WTextFormField).Text;
		pdfTextBoxField.ForeColor = (m_editableTextFormTextRange.CharacterFormat.TextColor.IsEmpty ? wTextFormField.CharacterFormat.TextColor : m_editableTextFormTextRange.CharacterFormat.TextColor);
		pdfTextBoxField.Font = CreatePdfFont(WordDocument.RenderHelper.GetFontStream(m_editableTextFormTextRange.CharacterFormat.Font, m_editableTextFormTextRange.ScriptType), m_editableTextFormTextRange.CharacterFormat.FontSize, GetFontStyle(m_editableTextFormTextRange.CharacterFormat.Font.Style));
		pdfTextBoxField.Font.Ascent = GetAscent(m_editableTextFormTextRange.CharacterFormat.Font, txtRange.ScriptType);
		pdfTextBoxField.Bounds = new RectangleF(m_editableTextFormBounds.X, m_editableTextFormBounds.Y, (float)Math.Round(m_editableTextFormBounds.Width), (float)Math.Round(m_editableTextFormBounds.Height));
		pdfTextBoxField.BackColor = (m_editableTextFormTextRange.CharacterFormat.HighlightColor.IsEmpty ? wTextFormField.CharacterFormat.HighlightColor : m_editableTextFormTextRange.CharacterFormat.HighlightColor);
		pdfTextBoxField.BorderColor = Color.Empty;
		pdfTextBoxField.DefaultValue = ((m_editableTextFormTextRange.PreviousSibling as WFieldMark).ParentField as WTextFormField).DefaultText;
		pdfTextBoxField.MaxLength = ((m_editableTextFormTextRange.PreviousSibling as WFieldMark).ParentField as WTextFormField).MaximumLength;
		pdfTextBoxField.Visible = ((m_editableTextFormTextRange.PreviousSibling as WFieldMark).ParentField as WTextFormField).Enabled;
		if (AutoTag)
		{
			pdfTextBoxField.PdfTag = CreateAutoTag(PdfTagType.Form, "", "", IsOverLappedShapeWidget(ltWidget));
		}
		(PDFGraphics.Page as PdfPage).Document.Form.Fields.Add(pdfTextBoxField);
		(PDFGraphics.Page as PdfPage).Document.Form.SetDefaultAppearance(applyDefault: false);
	}

	private char InverseCharacter(char ch)
	{
		return ch switch
		{
			'(' => ')', 
			')' => '(', 
			'<' => '>', 
			'>' => '<', 
			'{' => '}', 
			'}' => '{', 
			'[' => ']', 
			']' => '[', 
			_ => ch, 
		};
	}

	private bool IsInversedCharacter(string text)
	{
		char[] array = text.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			char c = array[i];
			array[i] = InverseCharacter(c);
			if (c != array[i])
			{
				return true;
			}
		}
		return false;
	}

	internal void DrawTextRange(WTextRange txtRange, LayoutedWidget ltWidget, string text)
	{
		if (text == '\u0003'.ToString() || text == '\u0004'.ToString())
		{
			DrawSeparator(txtRange, ltWidget);
			return;
		}
		if (txtRange.CharacterRange == CharacterRangeType.WordSplit && txtRange.CharacterFormat.Bidi && !EmbedFonts && !EmbedCompleteFonts)
		{
			char[] array = text.ToCharArray();
			Array.Reverse(array);
			for (int i = 0; i < array.Length; i++)
			{
				char ch = array[i];
				array[i] = InverseCharacter(ch);
			}
			text = new string(array);
		}
		currTextRange = txtRange;
		float num = ltWidget.Bounds.Left;
		float top = ltWidget.Bounds.Top;
		float height = ltWidget.Bounds.Height;
		StringFormat stringFormat = new StringFormat(StringFormt);
		if (txtRange.CharacterFormat.Bidi)
		{
			stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
		}
		else
		{
			stringFormat.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;
		}
		if (txtRange.m_layoutInfo is TabsLayoutInfo tabsLayoutInfo)
		{
			if (!tabsLayoutInfo.IsTabWidthUpdatedBasedOnIndent)
			{
				UpdateTabLeader(txtRange, ltWidget, ref text);
			}
			txtRange.Text = ControlChar.Tab;
			if (tabsLayoutInfo.CurrTabLeader != 0)
			{
				num += MeasureTextRange(txtRange, " ").Width;
			}
		}
		SizeF sizeF = new SizeF(ltWidget.Widget.LayoutInfo.Size);
		if (text == '\u0002'.ToString() && txtRange.GetOwnerParagraphValue().OwnerTextBody.Owner is WFootnote)
		{
			WFootnote wFootnote = txtRange.GetOwnerParagraphValue().OwnerTextBody.Owner as WFootnote;
			if (wFootnote.m_layoutInfo is FootnoteLayoutInfo)
			{
				text = (wFootnote.m_layoutInfo as FootnoteLayoutInfo).FootnoteID;
			}
		}
		RectangleF bounds = default(RectangleF);
		if (txtRange.Owner is WParagraph && txtRange.Text == text && txtRange.GetIndexInOwnerCollection() == txtRange.OwnerParagraph.Items.Count - 1 && !(txtRange.Owner as WParagraph).ParagraphFormat.Bidi)
		{
			text = GetTrimmedText(text);
		}
		else
		{
			sizeF = MeasureTextRange(txtRange, text);
		}
		bounds = ((!(txtRange.Text == "\t")) ? new RectangleF(num, top, sizeF.Width + ltWidget.SubWidth, height) : new RectangleF(num, top, ltWidget.Bounds.Width, ltWidget.Bounds.Height));
		WParagraphFormat paraFormat = null;
		if (txtRange.Owner is WParagraph)
		{
			paraFormat = txtRange.OwnerParagraph.ParagraphFormat;
		}
		WCharacterFormat characterFormat = txtRange.CharacterFormat;
		bool flag = ltWidget.HorizontalAlign == HAlignment.Distributed || ltWidget.HorizontalAlign == HAlignment.Justify;
		if ((!flag || ltWidget.IsLastLine) && ltWidget.HorizontalAlign != HAlignment.Distributed && !IsOwnerParagraphEmpty(text))
		{
			DrawString(txtRange.ScriptType, text, characterFormat, paraFormat, bounds, ltWidget.Bounds.Width, ltWidget);
		}
		else if (flag)
		{
			if (IsTextRangeFollowWithTab(ltWidget))
			{
				DrawString(txtRange.ScriptType, text, characterFormat, paraFormat, bounds, ltWidget.Bounds.Width, ltWidget);
			}
			else
			{
				DrawJustifiedLine(txtRange, text, characterFormat, paraFormat, bounds, ltWidget);
			}
		}
		if (currHyperlink != null)
		{
			if (IsValidFieldResult(currHyperlink.Field, ltWidget.Widget))
			{
				AddHyperLink(currHyperlink, ltWidget.Bounds);
			}
			else
			{
				currHyperlink = null;
			}
		}
	}

	internal string GetTrimmedText(string originalText)
	{
		string result = originalText;
		if (originalText.Contains(ControlChar.NonBreakingSpace))
		{
			result = originalText.Replace(ControlChar.NonBreakingSpaceChar, ControlChar.HyphenChar).TrimEnd().Replace(ControlChar.HyphenChar, ControlChar.NonBreakingSpaceChar);
		}
		else if (originalText.Trim() != string.Empty)
		{
			result = originalText.TrimEnd();
		}
		return result;
	}

	private bool IsValidFieldResult(WField hyperLinkField, IWidget widget)
	{
		WTextRange wTextRange = null;
		WPicture wPicture = null;
		if (widget is WTextRange)
		{
			wTextRange = widget as WTextRange;
		}
		else if (widget is SplitStringWidget)
		{
			wTextRange = (widget as SplitStringWidget).RealStringWidget as WTextRange;
		}
		else if (widget is WPicture)
		{
			wPicture = widget as WPicture;
		}
		for (int i = 0; i < hyperLinkField.Range.Count; i++)
		{
			if (wTextRange != null && hyperLinkField.Range.InnerList[i] is WTextRange && hyperLinkField.Range.InnerList[i] == wTextRange)
			{
				return true;
			}
			if (wTextRange != null && hyperLinkField.Range.InnerList[i] is WParagraph && hyperLinkField.Range.InnerList[i] == wTextRange.OwnerParagraph)
			{
				return true;
			}
			if (wPicture != null && hyperLinkField.Range.InnerList[i] is WPicture && hyperLinkField.Range.InnerList[i] == wPicture)
			{
				return true;
			}
		}
		return false;
	}

	internal void UpdateBookmarkTargetPosition(Entity ent, LayoutedWidget ltWidget)
	{
		bool flag = false;
		if (!(ent.PreviousSibling is BookmarkStart))
		{
			return;
		}
		BookmarkStart bookmarkStart = ent.PreviousSibling as BookmarkStart;
		if (BookmarkHyperlinksList.Count != 0)
		{
			for (int i = 0; i < BookmarkHyperlinksList.Count; i++)
			{
				foreach (KeyValuePair<string, BookmarkHyperlink> item in BookmarkHyperlinksList[i])
				{
					if (item.Key == bookmarkStart.Name)
					{
						flag = true;
						item.Value.TargetBounds = ltWidget.Bounds;
						item.Value.TargetPageNumber = DocumentLayouter.PageNumber;
						if (ent is ParagraphItem)
						{
							UpdateTOCLevel((ent as ParagraphItem).OwnerParagraph, item.Value);
						}
					}
				}
			}
		}
		if (!flag)
		{
			Dictionary<string, BookmarkHyperlink> dictionary = new Dictionary<string, BookmarkHyperlink>();
			BookmarkHyperlink bookmarkHyperlink = new BookmarkHyperlink();
			bookmarkHyperlink.HyperlinkValue = bookmarkStart.Name;
			bookmarkHyperlink.TargetBounds = ltWidget.Bounds;
			bookmarkHyperlink.TargetPageNumber = DocumentLayouter.PageNumber;
			bookmarkHyperlink.Hyperlink = currHyperlink;
			dictionary.Add(bookmarkHyperlink.HyperlinkValue, bookmarkHyperlink);
			BookmarkHyperlinksList.Add(dictionary);
		}
	}

	internal void CreateBookmarkRerefeceLink(Entity ent, LayoutedWidget ltWidget)
	{
		if (CurrentBookmarkName != null && CurrentBookmarkName != null)
		{
			if (!(ent is WField) || ltWidget.Bounds.Width > 0f)
			{
				CreateAndAddLinkToBookmark(ltWidget.Bounds, CurrentBookmarkName, isTargetNull: false);
			}
			if (ent.NextSibling is WFieldMark && (ent.NextSibling as WFieldMark).Type == FieldMarkType.FieldEnd && ent.NextSibling as WFieldMark == CurrentRefField.FieldEnd)
			{
				CurrentBookmarkName = null;
				CurrentRefField = null;
			}
		}
	}

	private bool IsTextRangeFollowWithTab(LayoutedWidget ltWidget)
	{
		int num = ltWidget.Owner.ChildWidgets.IndexOf(ltWidget);
		if (num >= 0)
		{
			for (int i = num; i < ltWidget.Owner.ChildWidgets.Count; i++)
			{
				if (ltWidget.Owner.ChildWidgets[i].Widget.LayoutInfo is TabsLayoutInfo)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void UpdateTabLeader(WTextRange txtRange, LayoutedWidget ltWidget, ref string text)
	{
		TabsLayoutInfo tabsLayoutInfo = txtRange.m_layoutInfo as TabsLayoutInfo;
		text = string.Empty;
		StringFormat stringFormat = new StringFormat(StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoClip);
		if (txtRange.CharacterFormat.Bidi)
		{
			stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
		}
		else
		{
			stringFormat.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;
		}
		if (IsTOC(txtRange) && txtRange.GetOwnerParagraphValue().ParaStyle != null)
		{
			stringFormat = StringFormt;
			if (txtRange.GetOwnerParagraphValue().ParaStyle.CharacterFormat.Bidi)
			{
				stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
			}
			else
			{
				stringFormat.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;
			}
		}
		if (txtRange.CharacterFormat != null && (txtRange.CharacterFormat.GetFontToRender(txtRange.ScriptType).UnderlineStyle != 0 || txtRange.CharacterFormat.GetFontToRender(txtRange.ScriptType).Strikeout))
		{
			FillSpace(txtRange.m_layoutInfo.Font.GetFont(txtRange.Document, txtRange.ScriptType), ltWidget, stringFormat, ref text);
		}
		if (tabsLayoutInfo != null)
		{
			switch (tabsLayoutInfo.CurrTabLeader)
			{
			case DocGen.Layouting.TabLeader.Dotted:
				FillDots(txtRange.m_layoutInfo.Font.GetFont(txtRange.Document, txtRange.ScriptType), ltWidget, txtRange.CharacterFormat, stringFormat, ref text);
				break;
			case DocGen.Layouting.TabLeader.Single:
				FillSingle(txtRange.m_layoutInfo.Font.GetFont(txtRange.Document, txtRange.ScriptType), ltWidget, txtRange.CharacterFormat, stringFormat, ref text);
				break;
			case DocGen.Layouting.TabLeader.Hyphenated:
				FillHyphens(txtRange.m_layoutInfo.Font.GetFont(txtRange.Document, txtRange.ScriptType), ltWidget, txtRange.CharacterFormat, stringFormat, ref text);
				break;
			}
		}
	}

	private void FillDots(DocGen.Drawing.Font font, LayoutedWidget ltWidget, WCharacterFormat charFormat, StringFormat format, ref string text)
	{
		text = string.Empty;
		float width = MeasureString(".", font, format, charFormat, isMeasureFromTabList: true, isMeasureFromSmallCapString: false, FontScriptType.English).Width;
		float num = width;
		float width2 = MeasureString(" ", font, format, charFormat, isMeasureFromTabList: true, isMeasureFromSmallCapString: false, FontScriptType.English).Width;
		for (float num2 = ltWidget.Bounds.Width - width2; num <= num2; num += width)
		{
			text += ".";
		}
	}

	private void FillSingle(DocGen.Drawing.Font font, LayoutedWidget ltWidget, WCharacterFormat charFormat, StringFormat format, ref string text)
	{
		float num = 0.1f;
		text = string.Empty;
		float width = MeasureString("_", font, format, charFormat, isMeasureFromTabList: true, isMeasureFromSmallCapString: false, FontScriptType.English).Width;
		width = ((width < 0f) ? num : width);
		float num2 = width;
		float width2 = MeasureString(" ", font, format, charFormat, isMeasureFromTabList: true, isMeasureFromSmallCapString: false, FontScriptType.English).Width;
		for (float num3 = ltWidget.Bounds.Width - width2; num2 <= num3; num2 += width)
		{
			text += "_";
		}
	}

	private void FillHyphens(DocGen.Drawing.Font font, LayoutedWidget ltWidget, WCharacterFormat charFormat, StringFormat format, ref string text)
	{
		text = string.Empty;
		float width = MeasureString("-", font, format, charFormat, isMeasureFromTabList: true, isMeasureFromSmallCapString: false, FontScriptType.English).Width;
		float num = width;
		float width2 = MeasureString(" ", font, format, charFormat, isMeasureFromTabList: true, isMeasureFromSmallCapString: false, FontScriptType.English).Width;
		for (float num2 = ltWidget.Bounds.Width - width2; num <= num2; num += width)
		{
			text += "-";
		}
	}

	private void FillSpace(DocGen.Drawing.Font font, LayoutedWidget ltWidget, StringFormat format, ref string text)
	{
		text = string.Empty;
		while (MeasureString(text, font, format, FontScriptType.English).Width <= ltWidget.Bounds.Width)
		{
			text += " ";
		}
	}

	internal void DrawSymbol(WSymbol symbol, LayoutedWidget ltWidget)
	{
		string text = char.ConvertFromUtf32(symbol.CharacterCode);
		WCharacterFormat charFormat = symbol.CharacterFormat;
		RectangleF bounds = ltWidget.Bounds;
		float clipWidth = ltWidget.Bounds.Width;
		RectangleF clipBounds = new RectangleF(ltWidget.Bounds.X, ltWidget.Bounds.Y, clipWidth, ltWidget.Bounds.Height);
		float scaling = charFormat.Scaling;
		bool flag = scaling != 100f && (scaling >= 1f || scaling <= 600f);
		PointF translatePoints = PointF.Empty;
		float rotationAngle = 0f;
		bool isRotateTransformApplied = false;
		if (symbol != null && symbol.m_layoutInfo != null && symbol.m_layoutInfo.IsVerticalText)
		{
			TransformGraphicsPosition(ltWidget, flag, ref isRotateTransformApplied, ref translatePoints, ref rotationAngle, symbol.OwnerParagraph);
		}
		if (clipWidth == 0f || IsWidgetNeedToClipBasedOnXPosition(ltWidget, ref clipWidth, bounds))
		{
			ResetTransform();
			return;
		}
		bool flag2 = IsNeedToClip(clipBounds);
		if (flag2)
		{
			clipBounds = ClipBoundsContainer.Peek();
			if (clipBounds.Width == 0f)
			{
				ResetTransform();
				return;
			}
		}
		if (ltWidget.Bounds.Width > 0f && flag)
		{
			RotateAndScaleTransform(ref bounds, ref clipBounds, scaling, PointF.Empty, 0f, isListCharacter: false, flipV: false, flipH: false);
		}
		if (flag2)
		{
			PDFGraphics.SetClip(clipBounds, CombineMode.Replace);
		}
		StringFormat stringFormat = new StringFormat(StringFormt);
		if (charFormat.Bidi)
		{
			stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
		}
		else
		{
			stringFormat.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;
		}
		if (symbol.m_layoutInfo == null)
		{
			return;
		}
		DocGen.Drawing.Font font = symbol.m_layoutInfo.Font.GetFont(symbol.Document, FontScriptType.English);
		DocGen.Drawing.Font font2 = ((charFormat != null && charFormat.SubSuperScript != 0) ? charFormat.Document.FontSettings.GetFont(font.Name, GetSubSuperScriptFontSize(font), font.Style, FontScriptType.English) : font);
		if (m_privateFontStream != null && m_privateFontStream.ContainsKey(symbol.FontName))
		{
			font2 = GetPrivateFont(font2.Name, font2.Size, font2.Style);
		}
		float num = 0f;
		string fontNameToRender = charFormat.GetFontNameToRender(FontScriptType.English);
		if (!IsListCharacter && (fontNameToRender == "DIN Offc" || fontNameToRender == "DIN OT") && currParagraph != null && currParagraph.IsContainDinOffcFont())
		{
			num = bounds.Height * 0.2f;
		}
		RectangleF textBounds = new RectangleF(bounds.X, bounds.Y + num, bounds.Width, bounds.Height);
		if (charFormat.CharacterSpacing != 0f)
		{
			DrawStringBasedOnCharSpacing(FontScriptType.English, ltWidget?.CharacterRange ?? CharacterRangeType.LTR, font2, GetBrush(GetTextColor(symbol.CharacterFormat)), bounds, text, stringFormat, charFormat);
		}
		else
		{
			PdfFont pdfFont = null;
			string text2 = GetEmbedFontStyle(charFormat);
			if (!string.IsNullOrEmpty(text2))
			{
				text2 = symbol.FontName + "_" + text2;
			}
			if (text2 != null && symbol.FontName != null && m_privateFontStream != null && (m_privateFontStream.ContainsKey(text2) || m_privateFontStream.ContainsKey(symbol.FontName)))
			{
				pdfFont = CreatePdfFont(text2, symbol.FontName, font2.Size, GetFontStyle(font2.Style));
			}
			else if (PdfFontCollection.ContainsKey(GetPdfFontCollectionKey(font2, IsUnicode(text))))
			{
				pdfFont = PdfFontCollection[GetPdfFontCollectionKey(font2, IsUnicode(text))];
			}
			else
			{
				pdfFont = CreatePdfFont(text, WordDocument.RenderHelper.GetFontStream(font2, FontScriptType.English), font2.Size, GetFontStyle(font2.Style));
				pdfFont.Ascent = GetAscent(font2, FontScriptType.English);
				PdfFontCollection.Add(GetPdfFontCollectionKey(font2, IsUnicode(text)), pdfFont);
			}
			PdfStringFormat pdfStringFormat = PDFGraphics.ConvertFormat(stringFormat, symbol.CharacterFormat.ComplexScript || EnableComplexScript);
			PdfBrush brush = new PdfSolidBrush(GetTextColor(symbol.CharacterFormat));
			if (FallbackFonts != null && FallbackFonts.Count > 0)
			{
				pdfFont = GetFallbackPdfFont(pdfFont, font2, text, FontScriptType.English, pdfStringFormat);
			}
			PDFGraphics.DrawString(text, pdfFont, brush, textBounds, pdfStringFormat, directConversion: true);
		}
		if (charFormat.UnderlineStyle != 0 || charFormat.Strikeout || charFormat.DoubleStrike)
		{
			bool isSameLine = false;
			CheckPreOrNextSiblingIsTab(ref charFormat, ref textBounds, ltWidget, ref isSameLine);
			AddLineToCollection(text, charFormat.SubSuperScript != DocGen.DocIO.DLS.SubSuperScript.None, font, charFormat, textBounds, isSameLine, FontScriptType.English);
		}
		ResetTransform();
		if (flag2)
		{
			PDFGraphics.ResetClip();
		}
	}

	internal void DrawPicture(WPicture picture, LayoutedWidget ltwidget)
	{
		if (picture != null)
		{
			bool flag = picture.Image != null && picture.Image.RawFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.Tiff);
			bool flag2 = picture.GetBaseEntity(picture) is HeaderFooter;
			DocGen.Drawing.SkiaSharpHelper.Image image;
			if (picture.ImageBytes != null)
			{
				if (flag2 && HeaderImageCollection.ContainsKey(picture))
				{
					image = HeaderImageCollection[picture];
				}
				else
				{
					byte[] array = picture.ImageBytes;
					if (flag)
					{
						array = WordDocument.RenderHelper.ConvertTiffToPng(array);
					}
					image = CreateBitmap().Decode(array) as DocGen.Drawing.SkiaSharpHelper.Image;
					if (((double)picture.FillRectangle.LeftOffset != 0.0 || (double)picture.FillRectangle.RightOffset != 0.0 || (double)picture.FillRectangle.TopOffset != 0.0 || (double)picture.FillRectangle.BottomOffset != 0.0) && image is DocGen.Drawing.SkiaSharpHelper.Bitmap && WordDocument.RenderHelper.HasBitmap(image))
					{
						RectangleF cropRectangle = CropPosition(picture);
						if (cropRectangle.X == 0f && cropRectangle.Y == 0f && cropRectangle.Height == 0f && cropRectangle.Width == 0f)
						{
							image = null;
						}
						else
						{
							using DocGen.Drawing.SkiaSharpHelper.Image image2 = (image as DocGen.Drawing.SkiaSharpHelper.Bitmap).Clone(cropRectangle, image.PixelFormat) as DocGen.Drawing.SkiaSharpHelper.Image;
							using MemoryStream memoryStream = new MemoryStream();
							image2.Save(memoryStream, image.RawFormat);
							memoryStream.Position = 0L;
							image = DocGen.Drawing.SkiaSharpHelper.Image.FromStream(new MemoryStream(memoryStream.ToArray()));
						}
					}
				}
			}
			else
			{
				image = null;
			}
			if (image == null)
			{
				return;
			}
			SizeF size = MeasureImage(picture);
			RectangleF bounds = ltwidget.Bounds;
			if (float.IsNaN(bounds.X))
			{
				bounds.X = 0f;
			}
			ResetTransform();
			if (!picture.IsShape && (picture.TextWrappingStyle == TextWrappingStyle.Tight || picture.TextWrappingStyle == TextWrappingStyle.Through))
			{
				float lineWidth = GetLineWidth(picture);
				if (lineWidth > 0f)
				{
					bounds = new RectangleF(bounds.X - lineWidth, bounds.Y - lineWidth, bounds.Width + lineWidth * 2f, bounds.Height + lineWidth * 2f);
				}
			}
			if (ltwidget.Widget.LayoutInfo.IsVerticalText && ltwidget.Widget is WPicture)
			{
				WParagraph ownerParagraphValue = (ltwidget.Widget as WPicture).GetOwnerParagraphValue();
				Entity ownerEntity = ownerParagraphValue.GetOwnerEntity();
				if (ownerParagraphValue.IsInCell)
				{
					WTableCell wTableCell = ownerParagraphValue.GetOwnerEntity() as WTableCell;
					LayoutedWidget ownerCellLayoutedWidget = GetOwnerCellLayoutedWidget(ltwidget);
					if (ownerCellLayoutedWidget != null)
					{
						RectangleF bounds2 = ownerCellLayoutedWidget.Owner.Bounds;
						if (wTableCell.CellFormat.TextDirection == DocGen.DocIO.DLS.TextDirection.VerticalTopToBottom)
						{
							PDFGraphics.TranslateTransform(bounds2.X + bounds2.Y + bounds2.Width, bounds2.Y - bounds2.X);
							PDFGraphics.RotateTransform(90f);
						}
						else
						{
							PDFGraphics.TranslateTransform(bounds2.X - bounds2.Y, bounds2.X + bounds2.Y + bounds2.Height);
							PDFGraphics.RotateTransform(270f);
						}
					}
				}
				else if (ownerEntity is WTextBox)
				{
					WTextBox wTextBox = ownerEntity as WTextBox;
					float left = wTextBox.TextLayoutingBounds.Left;
					float top = wTextBox.TextLayoutingBounds.Top;
					float num = wTextBox.TextBoxFormat.InternalMargin.Left;
					float offsetY = top - bounds.Y + (bounds.X - left) + num;
					if (wTextBox.TextBoxFormat.TextDirection == DocGen.DocIO.DLS.TextDirection.VerticalTopToBottom)
					{
						float layoutedTextBoxContentHeight = GetLayoutedTextBoxContentHeight(ltwidget);
						float widthToShiftVerticalText = GetWidthToShiftVerticalText(wTextBox.TextBoxFormat.TextVerticalAlignment, layoutedTextBoxContentHeight, wTextBox.TextLayoutingBounds.Height);
						PDFGraphics.TranslateTransform(wTextBox.TextLayoutingBounds.X + wTextBox.TextLayoutingBounds.Y + wTextBox.TextLayoutingBounds.Height - widthToShiftVerticalText, wTextBox.TextLayoutingBounds.Y - wTextBox.TextLayoutingBounds.X);
						PDFGraphics.RotateTransform(90f);
					}
					else
					{
						PDFGraphics.TranslateTransform(bounds.Y - top, offsetY);
					}
				}
				else if (ownerEntity is Shape)
				{
					Shape shape = ownerEntity as Shape;
					float left2 = shape.TextLayoutingBounds.Left;
					float top2 = shape.TextLayoutingBounds.Top;
					float num2 = shape.TextFrame.InternalMargin.Left;
					float offsetY2 = top2 - bounds.Y + (bounds.X - left2) + num2;
					if (shape.TextFrame.TextDirection == DocGen.DocIO.DLS.TextDirection.VerticalTopToBottom || shape.TextFrame.TextDirection == DocGen.DocIO.DLS.TextDirection.VerticalFarEast)
					{
						float offsetX = GetCellHeightForVerticalText(picture) + left2 - bounds.Y - bounds.Width - num2;
						PDFGraphics.TranslateTransform(offsetX, offsetY2);
					}
					else
					{
						PDFGraphics.TranslateTransform(bounds.Y - top2, offsetY2);
					}
				}
				else if (ownerParagraphValue.Owner.Owner is ChildShape)
				{
					ChildShape childShape = ownerParagraphValue.Owner.Owner as ChildShape;
					float left3 = childShape.TextLayoutingBounds.Left;
					float top3 = childShape.TextLayoutingBounds.Top;
					float num3 = childShape.TextFrame.InternalMargin.Left;
					float offsetY3 = top3 - bounds.Y + (bounds.X - left3) + num3;
					if (childShape.TextFrame.TextDirection == DocGen.DocIO.DLS.TextDirection.VerticalTopToBottom || childShape.TextFrame.TextDirection == DocGen.DocIO.DLS.TextDirection.VerticalFarEast)
					{
						float offsetX2 = GetCellHeightForVerticalText(picture) + left3 - bounds.Y - bounds.Width - num3;
						PDFGraphics.TranslateTransform(offsetX2, offsetY3);
					}
					else
					{
						PDFGraphics.TranslateTransform(bounds.Y - top3, offsetY3);
					}
				}
			}
			float num4 = 0f;
			WParagraph ownerParagraphValue2 = picture.GetOwnerParagraphValue();
			Entity ownerEntity2 = ownerParagraphValue2.GetOwnerEntity();
			if (picture.TextWrappingStyle == TextWrappingStyle.Inline && ownerParagraphValue2 != null && !ownerParagraphValue2.IsInCell && !(ownerEntity2 is Shape) && !(ownerEntity2 is WTextBox))
			{
				num4 = GetClipTopPosition(bounds, isInlinePicture: true) * 2f;
				if (num4 > 0f)
				{
					num4 += (float)FontMetric.Descent(picture.GetOwnerParagraphValue().BreakCharacterFormat.GetFontToRender(FontScriptType.English), FontScriptType.English);
				}
			}
			float clipWidth = bounds.Width;
			if (clipWidth == 0f || IsWidgetNeedToClipBasedOnXPosition(ltwidget, ref clipWidth, bounds))
			{
				ResetTransform();
				return;
			}
			if (ownerParagraphValue2 != null && ((picture.LayoutInCell && picture.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013) || picture.TextWrappingStyle == TextWrappingStyle.Inline) && (!ownerParagraphValue2.IsInCell || !IsTableInTextBoxOrShape(ownerParagraphValue2.GetOwnerEntity(), checkTextBoxOnly: true)) && (!ownerParagraphValue2.IsInCell || !(ownerParagraphValue2.GetOwnerEntity() is WTableCell) || !(ownerParagraphValue2.GetOwnerEntity() as WTableCell).OwnerRow.OwnerTable.IsInCell || !(bounds.Width < ltwidget.Owner.Owner.Owner.Bounds.Width)))
			{
				RectangleF clipBounds = GetClipBounds(bounds, clipWidth, num4);
				bool flag3 = false;
				if (IsNeedToClip(clipBounds))
				{
					clipBounds = ((!(num4 > 0f)) ? ClipBoundsContainer.Peek() : UpdateClipBounds(clipBounds, reverseClipping: false));
					SetClip(clipBounds);
					flag3 = true;
				}
				else if (num4 > 0f)
				{
					SetClip(clipBounds);
					flag3 = true;
				}
				if (flag3 && PDFGraphics.ClipBounds.Width == 0f)
				{
					PDFGraphics.ResetTransform();
					return;
				}
			}
			if (picture.Owner is GroupShape || picture.Owner is ChildGroupShape)
			{
				size = new SizeF(bounds.Width, bounds.Height);
			}
			if ((double)picture.FillRectangle.LeftOffset != 0.0 || (double)picture.FillRectangle.RightOffset != 0.0 || (double)picture.FillRectangle.TopOffset != 0.0 || (double)picture.FillRectangle.BottomOffset != 0.0)
			{
				if (image is DocGen.Drawing.SkiaSharpHelper.Bitmap && WordDocument.RenderHelper.HasBitmap(image))
				{
					if (picture.FillRectangle.LeftOffset < 0f || picture.FillRectangle.RightOffset < 0f || picture.FillRectangle.TopOffset < 0f || picture.FillRectangle.BottomOffset < 0f)
					{
						CropImageBounds(picture, ref bounds, ref size);
					}
				}
				else
				{
					PDFGraphics.SetClip(bounds, CombineMode.Replace);
					TileRectangle fillRectangle = picture.FillRectangle;
					float num5 = size.Height - size.Height * (fillRectangle.TopOffset + fillRectangle.BottomOffset) / 100f;
					float num6 = size.Width - size.Width * (fillRectangle.LeftOffset + fillRectangle.RightOffset) / 100f;
					float num7 = size.Height * 100f / num5;
					float num8 = size.Width * 100f / num6 * size.Width / 100f;
					float num9 = num7 * size.Height / 100f;
					float num10 = bounds.X;
					float num11 = bounds.Y;
					if (fillRectangle.LeftOffset != 0f && fillRectangle.RightOffset != 0f)
					{
						float num12 = fillRectangle.LeftOffset / (fillRectangle.LeftOffset + fillRectangle.RightOffset);
						num10 += ((float.IsNaN(num12) || float.IsInfinity(num12)) ? 0f : num12) * (size.Width - num8);
					}
					else if (fillRectangle.LeftOffset != 0f)
					{
						num10 -= fillRectangle.LeftOffset * num8 / 100f;
					}
					if (fillRectangle.TopOffset != 0f && fillRectangle.BottomOffset != 0f)
					{
						float num13 = fillRectangle.TopOffset / (fillRectangle.TopOffset + fillRectangle.BottomOffset);
						num11 += ((float.IsNaN(num13) || float.IsInfinity(num13)) ? 0f : num13) * (size.Height - num9);
					}
					else if (fillRectangle.TopOffset != 0f)
					{
						num11 -= fillRectangle.TopOffset * num9 / 100f;
					}
					if (!float.IsNaN(num10) && !float.IsInfinity(num10))
					{
						bounds.X = num10;
					}
					if (!float.IsNaN(num11) && !float.IsInfinity(num11))
					{
						bounds.Y = num11;
					}
					if (!float.IsNaN(num8) && !float.IsInfinity(num8))
					{
						size.Width = num8;
					}
					if (!float.IsNaN(num9) && !float.IsInfinity(num9))
					{
						size.Height = num9;
					}
				}
			}
			float rotation = picture.Rotation;
			if ((rotation != 0f || (rotation == 0f && (picture.FlipHorizontal || picture.FlipVertical))) && !ltwidget.Widget.LayoutInfo.IsVerticalText && picture.TextWrappingStyle != TextWrappingStyle.Tight && picture.TextWrappingStyle != TextWrappingStyle.Through)
			{
				if (!(picture.Owner is GroupShape) && !(picture.Owner is ChildGroupShape) && rotation != 0f)
				{
					RectangleF boundingBoxCoordinates = GetBoundingBoxCoordinates(new RectangleF(0f, 0f, picture.Width, picture.Height), rotation);
					bounds = new RectangleF(bounds.X - boundingBoxCoordinates.X, bounds.Y - boundingBoxCoordinates.Y, picture.Width, picture.Height);
				}
				PDFGraphics.Transform = GetTransformMatrix(bounds, rotation, picture.FlipHorizontal, picture.FlipVertical);
			}
			if ((rotation != 0f || picture.FlipHorizontal || picture.FlipVertical) && !ltwidget.Widget.LayoutInfo.IsVerticalText && (picture.TextWrappingStyle == TextWrappingStyle.Tight || picture.TextWrappingStyle == TextWrappingStyle.Through))
			{
				PDFGraphics.Transform = GetTransformMatrix(bounds, rotation, picture.FlipHorizontal, picture.FlipVertical);
			}
			Entity ownerEntity3 = picture.GetOwnerParagraphValue().GetOwnerEntity();
			if (ownerEntity3 is ChildShape)
			{
				Rotate(ownerEntity3 as ChildShape, (ownerEntity3 as ChildShape).Rotation, (ownerEntity3 as ChildShape).FlipVertical, (ownerEntity3 as ChildShape).FlipHorizantal, (ownerEntity3 as ChildShape).TextLayoutingBounds);
			}
			else if (ownerEntity3 is WTextBox)
			{
				WTextBox wTextBox2 = ownerEntity3 as WTextBox;
				if (wTextBox2.TextBoxFormat.Rotation != 0f && (!wTextBox2.IsShape || !wTextBox2.Shape.TextFrame.Upright))
				{
					PDFGraphics.Transform = GetTransformMatrix(m_rotateTransform, wTextBox2.TextBoxFormat.Rotation, wTextBox2.TextBoxFormat.FlipHorizontal, wTextBox2.TextBoxFormat.FlipVertical);
				}
				if (wTextBox2.TextBoxFormat.Rotation != 0f && wTextBox2.IsShape && wTextBox2.Shape.TextFrame.Upright && !wTextBox2.TextBoxFormat.AutoFit)
				{
					RectangleF clipBounds2 = Graphics.ClipBounds;
					if (clipBounds2.Y > bounds.Y && clipBounds2.Bottom < bounds.Bottom)
					{
						bounds.Y = clipBounds2.Y;
					}
					else if (clipBounds2.X > bounds.X && clipBounds2.Right < bounds.Right)
					{
						bounds.X = clipBounds2.X;
					}
				}
			}
			else if (ownerEntity3 is Shape)
			{
				Shape shape2 = ownerEntity3 as Shape;
				if (shape2.Rotation != 0f)
				{
					PDFGraphics.Transform = GetTransformMatrix(m_rotateTransform, shape2.Rotation, shape2.FlipHorizontal, shape2.FlipVertical);
				}
			}
			if (image != null && !HeaderImageCollection.ContainsKey(picture) && picture.FillFormat.BlipFormat.BlipTransparency == BlipTransparency.GrayScale && image.SKBitmap != null)
			{
				image.ApplyGrayScale();
			}
			if (picture.PictureShape.FillFormat.FillType == FillType.FillPicture && picture.PictureShape.FillFormat.ImageRecord != null)
			{
				DrawPictureFill(picture.PictureShape.FillFormat.ImageRecord, null, bounds, picture.PictureShape.FillFormat.BlipFormat.Transparency);
			}
			PdfImage pdfImage = null;
			if (image != null && flag && (double)picture.FillRectangle.LeftOffset == 0.0 && (double)picture.FillRectangle.RightOffset == 0.0 && (double)picture.FillRectangle.TopOffset == 0.0 && (double)picture.FillRectangle.BottomOffset == 0.0 && picture.FillFormat.BlipFormat.BlipTransparency != BlipTransparency.GrayScale)
			{
				image.ImageData = picture.ImageBytes;
				image.RawFormat = DocGen.Drawing.ImageFormat.Tiff;
			}
			if (picture.IsShape)
			{
				DrawInlinePictureShape(picture, bounds, size, image);
			}
			else
			{
				float lineWidth2 = GetLineWidth(picture);
				pdfImage = GetPdfImage(image);
				if (AutoTag)
				{
					pdfImage.PdfTag = CreateAutoTag(PdfTagType.Figure, picture.AlternativeText, picture.Title, IsOverLappedShapeWidget(ltwidget));
				}
				PDFGraphics.DrawImage(pdfImage, new RectangleF(bounds.X + lineWidth2, bounds.Y + lineWidth2, size.Width, size.Height));
				if (lineWidth2 > 0f)
				{
					PdfPen pdfPen = CreatePen(picture.PictureShape);
					pdfPen.Width = lineWidth2;
					PDFGraphics.DrawRectangle(pdfPen, bounds.X + lineWidth2 / 2f, bounds.Y + lineWidth2 / 2f, bounds.Width - lineWidth2, bounds.Height - lineWidth2);
				}
			}
			if (image != null && !flag2)
			{
				image.Dispose();
				image = null;
			}
			else if (!HeaderImageCollection.ContainsKey(picture))
			{
				HeaderImageCollection.Add(picture, image);
			}
			if (currHyperlink != null)
			{
				AddHyperLink(picture, ltwidget);
			}
			ResetTransform();
		}
		ResetClip();
	}

	private bool IsTableInTextBoxOrShape(Entity entity, bool checkTextBoxOnly)
	{
		if (!(entity is WTableCell))
		{
			return false;
		}
		while (entity is WTableCell)
		{
			entity = (entity as WTableCell).OwnerRow.OwnerTable.Owner;
		}
		if (checkTextBoxOnly)
		{
			return entity.Owner is WTextBox;
		}
		if (!(entity.Owner is WTextBox))
		{
			return entity.Owner is Shape;
		}
		return true;
	}

	public RectangleF GetBoundingBoxCoordinates(RectangleF bounds, float angle)
	{
		if (bounds.Width > 0f && bounds.Height > 0f)
		{
			DocGen.Drawing.SkiaSharpHelper.GraphicsPath graphicsPath = CreateGraphicsPath();
			graphicsPath.AddRectangle(bounds);
			graphicsPath.Transform(GetTransformMatrix(bounds, angle));
			PointF[] pathPoints = graphicsPath.PathPoints;
			RectangleF result = CalculateBoundingBox(pathPoints);
			graphicsPath.Dispose();
			return result;
		}
		return bounds;
	}

	private RectangleF CalculateBoundingBox(PointF[] imageCoordinates)
	{
		float x = imageCoordinates[0].X;
		float x2 = imageCoordinates[3].X;
		float y = imageCoordinates[0].Y;
		float y2 = imageCoordinates[3].Y;
		for (int i = 0; i < 4; i++)
		{
			if (imageCoordinates[i].X < x)
			{
				x = imageCoordinates[i].X;
			}
			if (imageCoordinates[i].X > x2)
			{
				x2 = imageCoordinates[i].X;
			}
			if (imageCoordinates[i].Y < y)
			{
				y = imageCoordinates[i].Y;
			}
			if (imageCoordinates[i].Y > y2)
			{
				y2 = imageCoordinates[i].Y;
			}
		}
		return new RectangleF(x, y, x2 - x, y2 - y);
	}

	internal Matrix GetTransformMatrix(RectangleF bounds, float angle)
	{
		Matrix matrix = new Matrix();
		PointF pointF = new PointF(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f);
		matrix = WordDocument.RenderHelper.MakeRotationDegrees(angle, pointF.X, pointF.Y);
		return GetMatrixValuesFromSkia(matrix);
	}

	private Matrix GetMatrixValuesFromSkia(Matrix matrix)
	{
		Matrix matrix2 = new Matrix();
		matrix2.Elements[0] = matrix.Elements[0];
		matrix2.Elements[1] = matrix.Elements[3];
		matrix2.Elements[2] = matrix.Elements[1];
		matrix2.Elements[3] = matrix.Elements[4];
		matrix2.Elements[4] = matrix.Elements[2];
		matrix2.Elements[5] = matrix.Elements[5];
		return matrix2;
	}

	private PdfImage GetPdfImage(DocGen.Drawing.SkiaSharpHelper.Image img)
	{
		MemoryStream memoryStream = new MemoryStream(img.ImageData);
		PdfImage pdfImage = null;
		pdfImage = ((img.RawFormat != DocGen.Drawing.ImageFormat.Tiff) ? GetPdfImage(memoryStream) : PDFGraphics.GetImage(memoryStream));
		memoryStream.Dispose();
		memoryStream = null;
		return pdfImage;
	}

	private PdfImage GetPdfImage(MemoryStream stream)
	{
		PdfImage result = null;
		try
		{
			result = PDFGraphics.GetImage(stream, PdfDocument);
		}
		catch (PdfException ex)
		{
			if (ex.Message == "Metafile format is not supported" || ex.Message == "Only JPEG and PNG images are supported")
			{
				stream.Position = 0L;
				DocGen.Drawing.SkiaSharpHelper.Bitmap bitmap = new DocGen.Drawing.SkiaSharpHelper.Bitmap(stream);
				if (bitmap != null && bitmap.SKBitmap != null)
				{
					MemoryStream stream2 = new MemoryStream();
					bitmap.Save(stream2, DocGen.Drawing.ImageFormat.Png);
					result = PDFGraphics.GetImage(stream2, PdfDocument);
				}
				else
				{
					result = PDFGraphics.GetImage(GetManifestResourceStream("ImageNotFound.jpg"), PdfDocument);
				}
			}
		}
		return result;
	}

	private PdfImage GetPdfImage(DocGen.Drawing.SkiaSharpHelper.Bitmap bmp)
	{
		PdfImage result = null;
		Stream stream = ((bmp.SKBitmap == null) ? new MemoryStream(bmp.ImageData) : DocGen.Drawing.SkiaSharpHelper.Extension.GetBitmapStream(bmp.SKBitmap));
		try
		{
			result = PDFGraphics.GetImage(stream, PdfDocument);
		}
		catch (PdfException ex)
		{
			if (ex.Message == "Only JPEG and PNG images are supported")
			{
				stream.Position = 0L;
				DocGen.Drawing.SkiaSharpHelper.Bitmap bitmap = new DocGen.Drawing.SkiaSharpHelper.Bitmap(stream);
				if (bitmap != null && bitmap.SKBitmap != null)
				{
					MemoryStream stream2 = new MemoryStream();
					bitmap.Save(stream2, DocGen.Drawing.ImageFormat.Png);
					result = PDFGraphics.GetImage(stream2, PdfDocument);
				}
				else
				{
					result = PDFGraphics.GetImage(GetManifestResourceStream("ImageNotFound.jpg"), PdfDocument);
				}
			}
		}
		return result;
	}

	private Stream GetManifestResourceStream(string fileName)
	{
		Assembly assembly = typeof(WordDocument).GetTypeInfo().Assembly;
		string[] manifestResourceNames = assembly.GetManifestResourceNames();
		foreach (string text in manifestResourceNames)
		{
			if (text.EndsWith("." + fileName))
			{
				fileName = text;
				break;
			}
		}
		return assembly.GetManifestResourceStream(fileName);
	}

	private void DrawInlinePictureShape(WPicture picture, RectangleF bounds, SizeF size, DocGen.Drawing.SkiaSharpHelper.Image image)
	{
		float lineWidth = GetLineWidth(picture.PictureShape.PictureDescriptor.BorderLeft);
		float lineWidth2 = GetLineWidth(picture.PictureShape.PictureDescriptor.BorderTop);
		float lineWidth3 = GetLineWidth(picture.PictureShape.PictureDescriptor.BorderRight);
		float lineWidth4 = GetLineWidth(picture.PictureShape.PictureDescriptor.BorderBottom);
		DocGen.Drawing.SkiaSharpHelper.ImageAttributes imageAttributes = null;
		if (picture.FillFormat.BlipFormat.ImageEffect.PropertiesHash.ContainsKey(3) || picture.FillFormat.BlipFormat.ImageEffect.PropertiesHash.ContainsKey(4))
		{
			AdjustPictureBrightnessAndContrast(image as DocGen.Drawing.SkiaSharpHelper.Bitmap, picture, ref imageAttributes, isWatermark: false);
		}
		PdfImage pdfImage = GetPdfImage(image);
		if (AutoTag)
		{
			pdfImage.PdfTag = CreateAutoTag(PdfTagType.Figure, picture.AlternativeText, picture.Title, isOverlapedShape: false);
		}
		PDFGraphics.DrawImage(pdfImage, new RectangleF(bounds.X + lineWidth, bounds.Y + lineWidth2, size.Width, size.Height));
		Color color = picture.PictureShape.PictureDescriptor.BorderBottom.LineColorExt;
		Color color2 = picture.PictureShape.PictureDescriptor.BorderLeft.LineColorExt;
		Color color3 = picture.PictureShape.PictureDescriptor.BorderRight.LineColorExt;
		Color color4 = picture.PictureShape.PictureDescriptor.BorderTop.LineColorExt;
		if (picture.PictureShape.ShapeContainer != null && picture.PictureShape.ShapeContainer.ShapePosition != null)
		{
			if (picture.PictureShape.ShapeContainer.ShapePosition.Properties.ContainsKey(924))
			{
				color2 = WordColor.ConvertRGBToColor(picture.PictureShape.ShapeContainer.ShapePosition.GetPropertyValue(924));
			}
			if (picture.PictureShape.ShapeContainer.ShapePosition.Properties.ContainsKey(926))
			{
				color3 = WordColor.ConvertRGBToColor(picture.PictureShape.ShapeContainer.ShapePosition.GetPropertyValue(926));
			}
			if (picture.PictureShape.ShapeContainer.ShapePosition.Properties.ContainsKey(923))
			{
				color4 = WordColor.ConvertRGBToColor(picture.PictureShape.ShapeContainer.ShapePosition.GetPropertyValue(923));
			}
			if (picture.PictureShape.ShapeContainer.ShapePosition.Properties.ContainsKey(925))
			{
				color = WordColor.ConvertRGBToColor(picture.PictureShape.ShapeContainer.ShapePosition.GetPropertyValue(925));
			}
		}
		if (color2.IsEmpty || color2.ToArgb() == 0)
		{
			color2 = Color.Black;
		}
		if (color3.IsEmpty || color3.ToArgb() == 0)
		{
			color3 = Color.Black;
		}
		if (color.IsEmpty || color.ToArgb() == 0)
		{
			color = Color.Black;
		}
		if (color4.IsEmpty || color4.ToArgb() == 0)
		{
			color4 = Color.Black;
		}
		PdfPen pdfPen = new PdfPen(new PdfColor(Color.Black));
		if (lineWidth > 0f)
		{
			pdfPen.Color = color2;
			pdfPen.Width = lineWidth;
			pdfPen = GetPictureBorderPen(picture.PictureShape, picture.PictureShape.PictureDescriptor.BorderLeft, pdfPen);
			PDFGraphics.DrawLine(pdfPen, new PointF(bounds.Left + lineWidth / 2f, bounds.Top), new PointF(bounds.Left + lineWidth / 2f, bounds.Bottom));
		}
		if (lineWidth2 > 0f)
		{
			pdfPen.Color = color4;
			pdfPen.Width = lineWidth2;
			pdfPen = GetPictureBorderPen(picture.PictureShape, picture.PictureShape.PictureDescriptor.BorderTop, pdfPen);
			PDFGraphics.DrawLine(pdfPen, new PointF(bounds.Left, bounds.Top + lineWidth2 / 2f), new PointF(bounds.Right, bounds.Top + lineWidth2 / 2f));
		}
		if (lineWidth3 > 0f)
		{
			pdfPen.Color = color3;
			pdfPen.Width = lineWidth3;
			pdfPen = GetPictureBorderPen(picture.PictureShape, picture.PictureShape.PictureDescriptor.BorderRight, pdfPen);
			PDFGraphics.DrawLine(pdfPen, new PointF(bounds.Right - lineWidth3 / 2f, bounds.Top), new PointF(bounds.Right - lineWidth3 / 2f, bounds.Bottom));
		}
		if (lineWidth4 > 0f)
		{
			pdfPen.Color = color;
			pdfPen.Width = lineWidth4;
			pdfPen = GetPictureBorderPen(picture.PictureShape, picture.PictureShape.PictureDescriptor.BorderBottom, pdfPen);
			PDFGraphics.DrawLine(pdfPen, new PointF(bounds.Left, bounds.Bottom - lineWidth4 / 2f), new PointF(bounds.Right, bounds.Bottom - lineWidth4 / 2f));
		}
	}

	private PdfPen CreatePen(InlineShapeObject inlineShapeObject)
	{
		PdfPen pen = new PdfPen(new PdfColor(GetColorBorder(inlineShapeObject)));
		pen = GetDashAndLineStyle(inlineShapeObject, pen);
		pen.LineJoin = GetLineJoin((PdfLineJoin)inlineShapeObject.ShapeContainer.GetPropertyValue(470));
		pen.LineCap = GetLineCap((PdfLineCap)inlineShapeObject.ShapeContainer.GetPropertyValue(471));
		return pen;
	}

	private Color GetColorBorder(InlineShapeObject inlineShapeObject)
	{
		if (inlineShapeObject.ShapeContainer.ShapeOptions.Properties.ContainsKey(448))
		{
			return WordColor.ConvertRGBToColor(inlineShapeObject.ShapeContainer.ShapeOptions.GetPropertyValue(448));
		}
		return Color.Black;
	}

	private PdfLineCap GetLineCap(PdfLineCap lineCap)
	{
		return lineCap switch
		{
			PdfLineCap.Square => PdfLineCap.Square, 
			PdfLineCap.Round => PdfLineCap.Round, 
			_ => PdfLineCap.Flat, 
		};
	}

	private PdfLineJoin GetLineJoin(PdfLineJoin lineJoin)
	{
		return lineJoin switch
		{
			PdfLineJoin.Bevel => PdfLineJoin.Bevel, 
			PdfLineJoin.Miter => PdfLineJoin.Miter, 
			_ => PdfLineJoin.Round, 
		};
	}

	private PdfPen GetPictureBorderPen(InlineShapeObject inlineShape, BorderCode borderCode, PdfPen pen)
	{
		LineDashing lineDashing = LineDashing.Solid;
		TextBoxLineStyle lineStyle = TextBoxLineStyle.Simple;
		lineDashing = inlineShape.GetDashStyle((BorderStyle)borderCode.BorderType, ref lineStyle);
		return GetDashStyle(lineDashing, pen);
	}

	private PdfPen GetDashAndLineStyle(InlineShapeObject inlineShape, PdfPen pen)
	{
		LineDashing lineDashing = LineDashing.Solid;
		TextBoxLineStyle textBoxLineStyle = TextBoxLineStyle.Simple;
		lineDashing = (LineDashing)inlineShape.ShapeContainer.GetPropertyValue(462);
		textBoxLineStyle = (TextBoxLineStyle)inlineShape.ShapeContainer.GetPropertyValue(461);
		pen = GetDashStyle(lineDashing, pen);
		switch (textBoxLineStyle)
		{
		case TextBoxLineStyle.Double:
			pen.CompoundArray = new float[4]
			{
				0f,
				0.3333333f,
				2f / 3f,
				1f
			};
			break;
		case TextBoxLineStyle.ThinThick:
			pen.CompoundArray = new float[4] { 0f, 0.16666f, 0.3f, 1f };
			break;
		case TextBoxLineStyle.ThickThin:
			pen.CompoundArray = new float[4] { 0f, 0.6f, 0.73333f, 1f };
			break;
		case TextBoxLineStyle.Triple:
			pen.CompoundArray = new float[6]
			{
				0f,
				0.1666667f,
				0.3333333f,
				2f / 3f,
				5f / 6f,
				1f
			};
			break;
		}
		return pen;
	}

	private PdfPen GetDashStyle(LineDashing lineDashing, PdfPen pen)
	{
		pen.DashStyle = PdfDashStyle.Custom;
		switch (lineDashing)
		{
		case LineDashing.Solid:
			pen.DashStyle = PdfDashStyle.Solid;
			break;
		case LineDashing.Dot:
			pen.DashStyle = PdfDashStyle.Dot;
			break;
		case LineDashing.DashGEL:
			pen.DashStyle = PdfDashStyle.Dash;
			break;
		case LineDashing.DashDotGEL:
			pen.DashStyle = PdfDashStyle.DashDot;
			break;
		case LineDashing.DashDotDot:
			pen.DashStyle = PdfDashStyle.DashDotDot;
			break;
		case LineDashing.Dash:
			pen.DashPattern = new float[2] { 3f, 1f };
			break;
		case LineDashing.DotGEL:
			pen.DashPattern = new float[2] { 1f, 3f };
			break;
		case LineDashing.DashDot:
			pen.DashPattern = new float[4] { 3f, 1f, 1f, 1f };
			break;
		case LineDashing.LongDashDotDotGEL:
			pen.DashPattern = new float[6] { 8f, 2f, 1f, 2f, 1f, 2f };
			break;
		case LineDashing.LongDashGEL:
			pen.DashPattern = new float[2] { 8f, 2f };
			break;
		case LineDashing.LongDashDotGEL:
			pen.DashPattern = new float[4] { 8f, 2f, 1f, 2f };
			break;
		}
		return pen;
	}

	private void CropImageBounds(WPicture picture, ref RectangleF bounds, ref SizeF size)
	{
		TileRectangle fillRectangle = picture.FillRectangle;
		float num = size.Height - size.Height * (fillRectangle.TopOffset + fillRectangle.BottomOffset) / 100f;
		float num2 = size.Width - size.Width * (fillRectangle.LeftOffset + fillRectangle.RightOffset) / 100f;
		float num3 = size.Height * 100f / num;
		float num4 = size.Width * 100f / num2 * size.Width / 100f;
		float num5 = num3 * size.Height / 100f;
		if (fillRectangle.LeftOffset < 0f && fillRectangle.RightOffset < 0f)
		{
			bounds.X += fillRectangle.LeftOffset / (fillRectangle.LeftOffset + fillRectangle.RightOffset) * (size.Width - num4);
			size.Width = num4;
		}
		else if (fillRectangle.LeftOffset < 0f)
		{
			bounds.X -= fillRectangle.LeftOffset * num4 / 100f;
			size.Width += fillRectangle.LeftOffset * num4 / 100f;
		}
		else if (fillRectangle.RightOffset < 0f)
		{
			size.Width += fillRectangle.RightOffset * num4 / 100f;
		}
		if (fillRectangle.TopOffset < 0f && fillRectangle.BottomOffset < 0f)
		{
			bounds.Y += fillRectangle.TopOffset / (fillRectangle.TopOffset + fillRectangle.BottomOffset) * (size.Height - num5);
			size.Height = num5;
		}
		else if (fillRectangle.TopOffset < 0f)
		{
			bounds.Y -= fillRectangle.TopOffset * num5 / 100f;
			size.Height += fillRectangle.TopOffset * num5 / 100f;
		}
		else if (fillRectangle.BottomOffset < 0f)
		{
			size.Height += fillRectangle.BottomOffset * num5 / 100f;
		}
	}

	private RectangleF CropPosition(WPicture picture)
	{
		RectangleF result = default(RectangleF);
		float num = 0f;
		float num2 = 0f;
		DocGen.DocIO.DLS.Entities.Image image = picture.GetImage(picture.ImageBytes, isImageFromScratch: false);
		TileRectangle fillRectangle = picture.FillRectangle;
		if (fillRectangle.LeftOffset > 0f)
		{
			result.X = fillRectangle.LeftOffset * (float)image.Width / 100f;
		}
		if (fillRectangle.TopOffset > 0f)
		{
			result.Y = fillRectangle.TopOffset * (float)image.Height / 100f;
		}
		if (fillRectangle.RightOffset > 0f)
		{
			num = fillRectangle.RightOffset * (float)image.Width / 100f;
		}
		if (fillRectangle.BottomOffset > 0f)
		{
			num2 = fillRectangle.BottomOffset * (float)image.Height / 100f;
		}
		result.Width = (float)image.Width - (result.X + num);
		result.Height = (float)image.Height - (result.Y + num2);
		if (result.Y > (float)image.Height || result.X > (float)image.Width || (fillRectangle.TopOffset < 0f && num2 > (float)image.Height) || (fillRectangle.LeftOffset < 0f && num > (float)image.Width))
		{
			result = new RectangleF(0f, 0f, 0f, 0f);
		}
		else
		{
			result.Width = ((result.Width < 1f) ? 1f : ((result.Width > (float)image.Width) ? ((float)image.Width) : result.Width));
			result.Height = ((result.Height < 1f) ? 1f : ((result.Height > (float)image.Height) ? ((float)image.Height) : result.Height));
		}
		image.Dispose();
		return result;
	}

	internal void DrawEquationField(FontScriptType scriptType, LayoutedEQFields ltEQField, WCharacterFormat charFormat)
	{
		Color textColor = GetTextColor(charFormat);
		GetBrush(textColor);
		StringFormat stringFormat = new StringFormat(StringFormt);
		stringFormat.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;
		for (int i = 0; i < ltEQField.ChildEQFileds.Count; i++)
		{
			if (ltEQField.ChildEQFileds[i] is TextEQField)
			{
				TextEQField textEQField = ltEQField.ChildEQFileds[i] as TextEQField;
				DocGen.Drawing.Font font = ((textEQField.Font != null) ? textEQField.Font : charFormat.GetFontToRender(scriptType));
				float num = 0f;
				string fontNameToRender = charFormat.GetFontNameToRender(scriptType);
				if (!IsListCharacter && (fontNameToRender == "DIN Offc" || fontNameToRender == "DIN OT") && currParagraph != null && currParagraph.IsContainDinOffcFont())
				{
					num = textEQField.Bounds.Height * 0.2f;
				}
				RectangleF rect = new RectangleF(textEQField.Bounds.X, textEQField.Bounds.Y + num, textEQField.Bounds.Width, textEQField.Bounds.Height);
				PdfFont pdfFont = null;
				string text = GetEmbedFontStyle(charFormat);
				if (!string.IsNullOrEmpty(text))
				{
					text = charFormat.GetFontNameToRender(scriptType) + "_" + text;
				}
				if (m_privateFontStream != null && (m_privateFontStream.ContainsKey(text) || m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType))))
				{
					pdfFont = CreatePdfFont(text, charFormat.GetFontNameToRender(scriptType), font.Size, GetFontStyle(font.Style));
				}
				else if (PdfFontCollection.ContainsKey(GetPdfFontCollectionKey(font, IsUnicode(textEQField.Text))))
				{
					pdfFont = PdfFontCollection[GetPdfFontCollectionKey(font, IsUnicode(textEQField.Text))];
				}
				else
				{
					pdfFont = CreatePdfFont(textEQField.Text, WordDocument.RenderHelper.GetFontStream(font, scriptType), font.Size, GetFontStyle(font.Style));
					pdfFont.Ascent = GetAscent(font, scriptType);
					PdfFontCollection.Add(GetPdfFontCollectionKey(font, IsUnicode(textEQField.Text)), pdfFont);
				}
				PdfStringFormat pdfStringFormat = PDFGraphics.ConvertFormat(stringFormat, charFormat.ComplexScript || EnableComplexScript);
				PdfBrush brush = new PdfSolidBrush(textColor);
				if (FallbackFonts != null && FallbackFonts.Count > 0)
				{
					pdfFont = GetFallbackPdfFont(pdfFont, font, textEQField.Text, scriptType, pdfStringFormat);
				}
				PDFGraphics.DrawString(textEQField.Text, pdfFont, brush, rect, pdfStringFormat, directConversion: true);
			}
			else if (ltEQField.ChildEQFileds[i] is LineEQField)
			{
				LineEQField lineEQField = ltEQField.ChildEQFileds[i] as LineEQField;
				PdfPen pen = new PdfPen(new PdfColor(Color.Black), 0.5f);
				PDFGraphics.DrawLine(pen, lineEQField.Point1, lineEQField.Point2);
			}
			else if (ltEQField.ChildEQFileds[i] != null)
			{
				if (ltEQField.ChildEQFileds[i].SwitchType == LayoutedEQFields.EQSwitchType.Radical)
				{
					DrawRadicalSwitch(scriptType, ltEQField.ChildEQFileds[i], charFormat);
				}
				else if (ltEQField.ChildEQFileds[i].SwitchType == LayoutedEQFields.EQSwitchType.Array)
				{
					DrawArraySwitch(scriptType, ltEQField.ChildEQFileds[i], charFormat, ltEQField.ChildEQFileds[i].Alignment);
				}
				else
				{
					DrawEquationField(scriptType, ltEQField.ChildEQFileds[i], charFormat);
				}
			}
		}
	}

	private void DrawArraySwitch(FontScriptType scriptType, LayoutedEQFields ltEQField, WCharacterFormat charFormat, StringAlignment arraySwitchAlignment)
	{
		Color textColor = GetTextColor(charFormat);
		GetBrush(textColor);
		StringFormat stringFormat = new StringFormat(StringFormt);
		stringFormat.Alignment = arraySwitchAlignment;
		stringFormat.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;
		for (int i = 0; i < ltEQField.ChildEQFileds.Count; i++)
		{
			if (ltEQField.ChildEQFileds[i] is TextEQField)
			{
				TextEQField textEQField = ltEQField.ChildEQFileds[i] as TextEQField;
				DocGen.Drawing.Font font = ((textEQField.Font != null) ? textEQField.Font : charFormat.GetFontToRender(scriptType));
				float num = 0f;
				string fontNameToRender = charFormat.GetFontNameToRender(scriptType);
				if (!IsListCharacter && (fontNameToRender == "DIN Offc" || fontNameToRender == "DIN OT") && currParagraph != null && currParagraph.IsContainDinOffcFont())
				{
					num = textEQField.Bounds.Height * 0.2f;
				}
				RectangleF rect = new RectangleF(textEQField.Bounds.X, textEQField.Bounds.Y + num, textEQField.Bounds.Width, textEQField.Bounds.Height);
				PdfFont pdfFont = null;
				string text = GetEmbedFontStyle(charFormat);
				if (!string.IsNullOrEmpty(text))
				{
					text = charFormat.GetFontNameToRender(scriptType) + "_" + text;
				}
				if (m_privateFontStream != null && (m_privateFontStream.ContainsKey(text) || m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType))))
				{
					pdfFont = CreatePdfFont(text, charFormat.GetFontNameToRender(scriptType), font.Size, GetFontStyle(font.Style));
				}
				else if (PdfFontCollection.ContainsKey(GetPdfFontCollectionKey(font, IsUnicode(textEQField.Text))))
				{
					pdfFont = PdfFontCollection[GetPdfFontCollectionKey(font, IsUnicode(textEQField.Text))];
				}
				else
				{
					pdfFont = CreatePdfFont(textEQField.Text, WordDocument.RenderHelper.GetFontStream(font, scriptType), font.Size, GetFontStyle(font.Style));
					pdfFont.Ascent = GetAscent(font, scriptType);
					PdfFontCollection.Add(GetPdfFontCollectionKey(font, IsUnicode(textEQField.Text)), pdfFont);
				}
				PdfStringFormat pdfStringFormat = PDFGraphics.ConvertFormat(stringFormat, charFormat.ComplexScript || EnableComplexScript);
				PdfBrush brush = new PdfSolidBrush(textColor);
				if (FallbackFonts != null && FallbackFonts.Count > 0)
				{
					pdfFont = GetFallbackPdfFont(pdfFont, font, textEQField.Text, scriptType, pdfStringFormat);
				}
				PDFGraphics.DrawString(textEQField.Text, pdfFont, brush, rect, pdfStringFormat, directConversion: true);
			}
			else if (ltEQField.ChildEQFileds[i] is LineEQField)
			{
				LineEQField lineEQField = ltEQField.ChildEQFileds[i] as LineEQField;
				PdfPen pen = new PdfPen(new PdfColor(Color.Black), 0.5f);
				PDFGraphics.DrawLine(pen, lineEQField.Point1, lineEQField.Point2);
			}
			else if (ltEQField.ChildEQFileds[i] != null)
			{
				DrawArraySwitch(scriptType, ltEQField.ChildEQFileds[i], charFormat, arraySwitchAlignment);
			}
		}
	}

	internal PdfFontStyle GetFontStyle(FontStyle style)
	{
		PdfFontStyle pdfFontStyle = PdfFontStyle.Regular;
		if ((style & FontStyle.Bold) == FontStyle.Bold)
		{
			pdfFontStyle = PdfFontStyle.Bold;
		}
		if ((style & FontStyle.Italic) == FontStyle.Italic)
		{
			pdfFontStyle |= PdfFontStyle.Italic;
		}
		if ((style & FontStyle.Underline) == FontStyle.Underline)
		{
			pdfFontStyle |= PdfFontStyle.Underline;
		}
		if ((style & FontStyle.Strikeout) == FontStyle.Strikeout)
		{
			pdfFontStyle |= PdfFontStyle.Strikeout;
		}
		return pdfFontStyle;
	}

	private void DrawRadicalSwitch(FontScriptType scriptType, LayoutedEQFields ltEQField, WCharacterFormat charFormat)
	{
		Color textColor = GetTextColor(charFormat);
		GetBrush(textColor);
		StringFormat stringFormat = new StringFormat(StringFormt);
		stringFormat.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;
		Pen pen = CreatePen(Color.Black, 0.7f);
		pen.LineJoin = DocGen.Drawing.LineJoin.Round;
		DocGen.Drawing.SkiaSharpHelper.GraphicsPath graphicsPath = CreateGraphicsPath();
		for (int i = 0; i < ltEQField.ChildEQFileds.Count; i++)
		{
			for (; i < ltEQField.ChildEQFileds.Count && ltEQField.ChildEQFileds[i] is LineEQField; i++)
			{
				LineEQField lineEQField = ltEQField.ChildEQFileds[i] as LineEQField;
				graphicsPath.AddLine(lineEQField.Point1, lineEQField.Point2);
			}
			if (graphicsPath.PointCount > 0)
			{
				Graphics.DrawPath(pen, graphicsPath);
				graphicsPath.Reset();
			}
			if (i < ltEQField.ChildEQFileds.Count && ltEQField.ChildEQFileds[i] is TextEQField)
			{
				TextEQField textEQField = ltEQField.ChildEQFileds[i] as TextEQField;
				DocGen.Drawing.Font font = ((textEQField.Font != null) ? textEQField.Font : charFormat.GetFontToRender(scriptType));
				float num = 0f;
				string fontNameToRender = charFormat.GetFontNameToRender(scriptType);
				if (!IsListCharacter && (fontNameToRender == "DIN Offc" || fontNameToRender == "DIN OT") && currParagraph != null && currParagraph.IsContainDinOffcFont())
				{
					num = textEQField.Bounds.Height * 0.2f;
				}
				RectangleF rect = new RectangleF(textEQField.Bounds.X, textEQField.Bounds.Y + num, textEQField.Bounds.Width, textEQField.Bounds.Height);
				PdfFont pdfFont = null;
				string text = GetEmbedFontStyle(charFormat);
				if (!string.IsNullOrEmpty(text))
				{
					text = charFormat.GetFontNameToRender(scriptType) + "_" + text;
				}
				if (m_privateFontStream != null && (m_privateFontStream.ContainsKey(text) || m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType))))
				{
					pdfFont = CreatePdfFont(text, charFormat.GetFontNameToRender(scriptType), font.Size, GetFontStyle(font.Style));
				}
				else if (PdfFontCollection.ContainsKey(GetPdfFontCollectionKey(font, IsUnicode(textEQField.Text))))
				{
					pdfFont = PdfFontCollection[GetPdfFontCollectionKey(font, IsUnicode(textEQField.Text))];
				}
				else
				{
					pdfFont = CreatePdfFont(textEQField.Text, WordDocument.RenderHelper.GetFontStream(font, scriptType), font.Size, GetFontStyle(font.Style));
					pdfFont.Ascent = GetAscent(font, scriptType);
					PdfFontCollection.Add(GetPdfFontCollectionKey(font, IsUnicode(textEQField.Text)), pdfFont);
				}
				PdfStringFormat pdfStringFormat = PDFGraphics.ConvertFormat(stringFormat, charFormat.ComplexScript || EnableComplexScript);
				PdfBrush brush = new PdfSolidBrush(textColor);
				if (FallbackFonts != null && FallbackFonts.Count > 0)
				{
					pdfFont = GetFallbackPdfFont(pdfFont, font, textEQField.Text, scriptType, pdfStringFormat);
				}
				PDFGraphics.DrawString(textEQField.Text, pdfFont, brush, rect, pdfStringFormat, directConversion: true);
			}
			else if (i < ltEQField.ChildEQFileds.Count)
			{
				DrawEquationField(scriptType, ltEQField.ChildEQFileds[i], charFormat);
			}
		}
	}

	internal void DrawEditableDropDown(WDropDownFormField dropDownFormField, LayoutedWidget ltWidget)
	{
		if (string.IsNullOrEmpty(dropDownFormField.Name))
		{
			dropDownFormField.Name = "dropdownformfield";
		}
		float num = 15f;
		num += ltWidget.Bounds.Width;
		ltWidget.Bounds = new RectangleF(ltWidget.Bounds.X, ltWidget.Bounds.Y, num, ltWidget.Bounds.Height);
		PdfComboBoxField pdfComboBoxField = new PdfComboBoxField(PDFGraphics.Page, dropDownFormField.Name);
		pdfComboBoxField.Editable = true;
		pdfComboBoxField.Bounds = ltWidget.Bounds;
		pdfComboBoxField.ToolTip = dropDownFormField.StatusBarHelp;
		pdfComboBoxField.BorderColor = Color.Empty;
		pdfComboBoxField.Visible = dropDownFormField.Enabled;
		pdfComboBoxField.Font = CreatePdfFont(WordDocument.RenderHelper.GetFontStream(dropDownFormField.CharacterFormat.Font, dropDownFormField.ScriptType), dropDownFormField.CharacterFormat.FontSize, GetFontStyle(dropDownFormField.CharacterFormat.Font.Style));
		pdfComboBoxField.Font.Ascent = GetAscent(dropDownFormField.CharacterFormat.Font, dropDownFormField.ScriptType);
		int i = 0;
		for (int num2 = dropDownFormField.DropDownItems.Count - 1; i <= num2; i++)
		{
			pdfComboBoxField.Items.Add(new PdfListFieldItem(dropDownFormField.DropDownItems[i].Text, dropDownFormField.DropDownItems[i].Text));
		}
		if (pdfComboBoxField.Items.Count > 0)
		{
			pdfComboBoxField.SelectedIndex = dropDownFormField.DropDownSelectedIndex;
			pdfComboBoxField.SelectedValue = dropDownFormField.DropDownValue;
		}
		if (AutoTag)
		{
			pdfComboBoxField.PdfTag = CreateAutoTag(PdfTagType.Form, "", "", IsOverLappedShapeWidget(ltWidget));
		}
		(PDFGraphics.Page as PdfPage).Document.Form.Fields.Add(pdfComboBoxField);
	}

	private void AlignEqFieldSwitches(LayoutedEQFields ltEQField, float xPosition, float yPosition)
	{
		float num = yPosition - GetTopMostY(ltEQField, yPosition);
		if (num != 0f)
		{
			ShiftEqFieldXYPosition(ltEQField, xPosition, num);
		}
	}

	internal void ShiftEqFieldXYPosition(LayoutedEQFields ltEQField, float xPosition, float yPosition)
	{
		if (ltEQField is TextEQField)
		{
			RectangleF bounds = (ltEQField as TextEQField).Bounds;
			(ltEQField as TextEQField).Bounds = new RectangleF(bounds.X + xPosition, bounds.Y + yPosition, bounds.Width, bounds.Height);
		}
		else if (ltEQField is LineEQField)
		{
			PointF point = (ltEQField as LineEQField).Point1;
			PointF point2 = (ltEQField as LineEQField).Point2;
			(ltEQField as LineEQField).Point1 = new PointF(point.X + xPosition, point.Y + yPosition);
			(ltEQField as LineEQField).Point2 = new PointF(point2.X + xPosition, point2.Y + yPosition);
			RectangleF bounds2 = (ltEQField as LineEQField).Bounds;
			(ltEQField as LineEQField).Bounds = new RectangleF(bounds2.X + xPosition, bounds2.Y + yPosition, bounds2.Width, bounds2.Height);
		}
		else if (ltEQField != null)
		{
			ltEQField.Bounds = new RectangleF(ltEQField.Bounds.X + xPosition, ltEQField.Bounds.Y + yPosition, ltEQField.Bounds.Width, ltEQField.Bounds.Height);
			for (int i = 0; i < ltEQField.ChildEQFileds.Count; i++)
			{
				ShiftEqFieldXYPosition(ltEQField.ChildEQFileds[i], xPosition, yPosition);
			}
		}
	}

	public void GenerateErrorFieldCode(LayoutedEQFields ltEQFiled, float xPosition, float yPosition, WCharacterFormat charFormat)
	{
		TextEQField textEQField = new TextEQField();
		textEQField.Text = "Error!";
		textEQField.Font = charFormat.Document.FontSettings.GetFont("Calibri", 11f, FontStyle.Bold, FontScriptType.English);
		textEQField.Bounds = new RectangleF(new PointF(xPosition, yPosition), MeasureString(textEQField.Text, textEQField.Font, null, charFormat, isMeasureFromTabList: false, FontScriptType.English));
		float ascent = GetAscent(textEQField.Font, FontScriptType.English);
		ShiftEqFieldYPosition(textEQField, 0f - ascent);
		ltEQFiled.ChildEQFileds.Add(textEQField);
		ltEQFiled.Bounds = textEQField.Bounds;
	}

	public void ShiftEqFieldYPosition(LayoutedEQFields LayoutedEQFields, float yPosition)
	{
		if (LayoutedEQFields is TextEQField)
		{
			TextEQField textEQField = LayoutedEQFields as TextEQField;
			textEQField.Bounds = new RectangleF(textEQField.Bounds.X, textEQField.Bounds.Y + yPosition, textEQField.Bounds.Width, textEQField.Bounds.Height);
		}
		else if (LayoutedEQFields is LineEQField)
		{
			LineEQField lineEQField = LayoutedEQFields as LineEQField;
			lineEQField.Point1 = new PointF(lineEQField.Point1.X, lineEQField.Point1.Y + yPosition);
			lineEQField.Point2 = new PointF(lineEQField.Point2.X, lineEQField.Point2.Y + yPosition);
			lineEQField.Bounds = new RectangleF(lineEQField.Bounds.X, lineEQField.Bounds.Y + yPosition, lineEQField.Bounds.Width, lineEQField.Bounds.Height);
		}
		else if (LayoutedEQFields != null)
		{
			LayoutedEQFields.Bounds = new RectangleF(LayoutedEQFields.Bounds.X, LayoutedEQFields.Bounds.Y + yPosition, LayoutedEQFields.Bounds.Width, LayoutedEQFields.Bounds.Height);
			for (int i = 0; i < LayoutedEQFields.ChildEQFileds.Count; i++)
			{
				ShiftEqFieldYPosition(LayoutedEQFields.ChildEQFileds[i], yPosition);
			}
		}
	}

	internal float GetTopMostY(LayoutedEQFields ltEQField, float minY)
	{
		if (ltEQField is TextEQField)
		{
			if (ltEQField.Bounds.Y < minY)
			{
				minY = ltEQField.Bounds.Y;
			}
		}
		else if (ltEQField is LineEQField)
		{
			if ((ltEQField as LineEQField).Point1.Y < minY)
			{
				minY = (ltEQField as LineEQField).Point1.Y;
			}
		}
		else if (ltEQField != null)
		{
			if (ltEQField.Bounds.Y < minY)
			{
				minY = ltEQField.Bounds.Y;
			}
			foreach (LayoutedEQFields childEQFiled in ltEQField.ChildEQFileds)
			{
				float topMostY = GetTopMostY(childEQFiled, minY);
				if (topMostY < minY)
				{
					minY = topMostY;
				}
			}
		}
		return minY;
	}

	internal void DrawString(FontScriptType scriptType, string text, WCharacterFormat charFormat, WParagraphFormat paraFormat, RectangleF bounds, float clipWidth, LayoutedWidget ltWidget)
	{
		if (text == null || bounds.Height == 0f || clipWidth == 0f)
		{
			return;
		}
		text = text.Replace('\u001e'.ToString(), "-");
		text = text.Replace('\u00ad'.ToString(), "-");
		text = text.Replace('\u200d'.ToString(), "");
		bool flag = true;
		if (ltWidget != null && ltWidget.Widget.LayoutInfo is TabsLayoutInfo)
		{
			flag = (charFormat.HighlightColor.IsEmpty ? true : false);
		}
		if (text.Length == 0 && flag)
		{
			return;
		}
		if (charFormat.Bidi || charFormat.ComplexScript)
		{
			if (!charFormat.GetBoldToRender())
			{
				charFormat.Bold = false;
			}
			else if (!charFormat.Bold)
			{
				charFormat.Bold = true;
			}
			if (!charFormat.GetItalicToRender())
			{
				charFormat.Italic = false;
			}
			else if (!charFormat.Italic)
			{
				charFormat.Italic = true;
			}
		}
		DocGen.Drawing.Font font = null;
		bool flag2 = HasUnderlineOrStricthrough(currTextRange, charFormat, scriptType);
		bool hasCommentsHighlighter = m_commentId != null;
		font = ((IsListCharacter || currTextRange == null) ? GetFont(scriptType, charFormat, text) : GetFont(currTextRange, charFormat, text));
		Color black = Color.Black;
		StringFormat stringFormat = new StringFormat(StringFormt);
		if (charFormat.Bidi)
		{
			if (currTextRange.CharacterRange != CharacterRangeType.WordSplit || EmbedFonts || EmbedCompleteFonts || !IsInversedCharacter(text))
			{
				stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
			}
		}
		else
		{
			stringFormat.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;
		}
		if (font.Name == "Arial Narrow" && font.Style == FontStyle.Bold)
		{
			text = text.Replace(ControlChar.NonBreakingSpaceChar, ControlChar.SpaceChar);
		}
		if (IsSoftHyphen(ltWidget))
		{
			text = "-";
			if (bounds.Width == 0f)
			{
				bounds.Width = ltWidget.Bounds.Width;
			}
		}
		if (IsTOC(currTextRange) && currParagraph != null && currParagraph.ParaStyle != null)
		{
			stringFormat = new StringFormat(StringFormt);
			if (currParagraph.ParaStyle.CharacterFormat.Bidi)
			{
				stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
			}
			else
			{
				stringFormat.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;
			}
		}
		if (ltWidget != null && ltWidget.Widget.LayoutInfo is ParagraphLayoutInfo && currParagraph != null)
		{
			stringFormat = new StringFormat(StringFormt);
			if (currParagraph.ParagraphFormat.Bidi)
			{
				stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
			}
			else
			{
				stringFormat.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;
			}
		}
		black = GetTextColor(charFormat);
		DocGen.Drawing.SkiaSharpHelper.Brush brush = GetBrush(black);
		float num = bounds.Height;
		float num2 = bounds.Y;
		if (paraFormat != null)
		{
			num = bounds.Height - paraFormat.Borders.Bottom.LineWidth - paraFormat.Borders.Top.LineWidth;
			num2 = bounds.Y + paraFormat.Borders.Top.LineWidth;
			if (currParagraph != null && currParagraph.OwnerTextBody.Owner is Shape && (currParagraph.OwnerTextBody.Owner as Shape).TextLayoutingBounds.Y == num2)
			{
				num2 += (currParagraph.OwnerTextBody.Owner as Shape).LineFormat.Weight;
				num -= (currParagraph.OwnerTextBody.Owner as Shape).LineFormat.Weight;
			}
			else if (currParagraph != null && currParagraph.OwnerTextBody.Owner is ChildShape && (currParagraph.OwnerTextBody.Owner as ChildShape).TextLayoutingBounds.Y == num2)
			{
				num2 += (currParagraph.OwnerTextBody.Owner as ChildShape).LineFormat.Weight;
				num -= (currParagraph.OwnerTextBody.Owner as ChildShape).LineFormat.Weight;
			}
		}
		ResetTransform();
		float scaling = charFormat.Scaling;
		bool flag3 = scaling != 100f && (scaling >= 1f || scaling <= 600f);
		PointF translatePoints = PointF.Empty;
		float rotationAngle = 0f;
		float ang = 0f;
		bool flipH = false;
		bool flipV = false;
		bool doNotRotateText = false;
		bool isRotateTransformApplied = false;
		TextWrappingStyle textWrappingStyle = TextWrappingStyle.Square;
		GetTextboxOrShapesRotationValue(ref ang, ref flipH, ref flipV, ref doNotRotateText, ref textWrappingStyle, ref hasCommentsHighlighter, currTextRange);
		Rotate(ang, ref flipV, ref flipH, textWrappingStyle, doNotRotateText, flag3, ref isRotateTransformApplied, ref rotationAngle);
		RectangleF textBounds = bounds;
		if (currTextRange != null && currTextRange.m_layoutInfo != null && currTextRange.m_layoutInfo.IsVerticalText)
		{
			TransformGraphicsPosition(ltWidget, flag3, ref isRotateTransformApplied, ref translatePoints, ref rotationAngle, currParagraph);
		}
		float clipTopPosition = GetClipTopPosition(bounds, isInlinePicture: false);
		if (clipWidth == 0f || IsTextNeedToClip(ltWidget) || IsWidgetNeedToClipBasedOnXPosition(ltWidget, ref clipWidth, bounds))
		{
			ResetTransform();
			return;
		}
		RectangleF clipBounds = GetClipBounds(bounds, clipWidth, clipTopPosition);
		bool flag4 = IsNeedToClip(clipBounds);
		if (ClipBoundsContainer != null && ClipBoundsContainer.Count > 0)
		{
			RectangleF rectangleF = ClipBoundsContainer.Peek();
			if (currParagraph != null && currParagraph.GetOwnerEntity() is WTextBox)
			{
				WParagraph ownerParagraph = (currParagraph.GetOwnerEntity() as WTextBox).OwnerParagraph;
				if (ownerParagraph != null && ownerParagraph.ParagraphFormat.IsInFrame())
				{
					rectangleF = GetTextBoxWidget(ltWidget)?.Bounds ?? rectangleF;
				}
				flag4 = !currParagraph.IsInCell && (Math.Round(clipBounds.Bottom, 2) >= Math.Round(rectangleF.Bottom, 2) || clipBounds.X < rectangleF.X);
			}
		}
		if (flag4)
		{
			clipBounds = ((!(clipTopPosition > 0f)) ? ClipBoundsContainer.Peek() : UpdateClipBounds(clipBounds, reverseClipping: false));
			if (clipBounds.Width == 0f)
			{
				ResetTransform();
				return;
			}
		}
		if (bounds.Width > 0f && flag3)
		{
			RotateAndScaleTransform(ref bounds, ref clipBounds, scaling, translatePoints, rotationAngle, isListCharacter: false, flipV, flipH);
			clipWidth = clipBounds.Width;
			textBounds = new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height);
		}
		if (flag4 || clipTopPosition > 0f)
		{
			SetClip(clipBounds);
		}
		float x = bounds.X;
		TabsLayoutInfo tabsLayoutInfo = ((currTextRange != null && currTextRange.m_layoutInfo is TabsLayoutInfo) ? (currTextRange.m_layoutInfo as TabsLayoutInfo) : null);
		if (tabsLayoutInfo != null && tabsLayoutInfo.CurrTabLeader != 0)
		{
			x = ltWidget.Bounds.X;
		}
		if (!charFormat.TextBackgroundColor.IsEmpty && !hasCommentsHighlighter)
		{
			PDFGraphics.DrawRectangle(new PdfSolidBrush(charFormat.TextBackgroundColor), x, num2, bounds.Width, num);
		}
		if (!charFormat.HighlightColor.IsEmpty && !hasCommentsHighlighter)
		{
			Color hightLightColor = GetHightLightColor(charFormat.HighlightColor);
			PDFGraphics.DrawRectangle(GetPDFBrush(hightLightColor), x, num2, bounds.Width, num);
		}
		if (IsListCharacter)
		{
			bounds.Width = MeasureString(text, font, null, charFormat, isMeasureFromTabList: true, scriptType).Width;
		}
		if (bounds.Width > 0f && flag3 && IsListCharacter)
		{
			RotateAndScaleTransform(ref bounds, ref clipBounds, scaling, translatePoints, rotationAngle, IsListCharacter, flipV, flipH);
		}
		bool flag5 = charFormat.SubSuperScript == DocGen.DocIO.DLS.SubSuperScript.None;
		float num3 = 0f;
		string fontNameToRender = charFormat.GetFontNameToRender(scriptType);
		if (!IsListCharacter && (fontNameToRender == "DIN Offc" || fontNameToRender == "DIN OT") && currParagraph != null && currParagraph.IsContainDinOffcFont())
		{
			num3 = bounds.Height * 0.2f;
		}
		if (charFormat.Emboss)
		{
			DocGen.Drawing.Font font2 = ((!flag5) ? charFormat.Document.FontSettings.GetFont(charFormat.GetFontNameToRender(scriptType), GetSubSuperScriptFontSize(charFormat.GetFontToRender(scriptType)), charFormat.GetFontToRender(scriptType).Style, scriptType) : charFormat.GetFontToRender(scriptType));
			if (!charFormat.GetFontNameToRender(scriptType).Equals(font2.Name, StringComparison.OrdinalIgnoreCase) && m_privateFontStream != null && m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType)))
			{
				font2 = GetPrivateFont(charFormat.GetFontNameToRender(scriptType), charFormat.GetFontSizeToRender(), font2.Style);
			}
			PdfFont pdfFont = null;
			string text2 = GetEmbedFontStyle(charFormat);
			if (!string.IsNullOrEmpty(text2))
			{
				text2 = charFormat.GetFontNameToRender(scriptType) + "_" + text2;
			}
			if (m_privateFontStream != null && (m_privateFontStream.ContainsKey(text2) || m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType))))
			{
				pdfFont = CreatePdfFont(text2, charFormat.GetFontNameToRender(scriptType), font2.Size, GetFontStyle(font2.Style));
			}
			else if (PdfFontCollection.ContainsKey(GetPdfFontCollectionKey(font2, IsUnicode(text))))
			{
				pdfFont = PdfFontCollection[GetPdfFontCollectionKey(font2, IsUnicode(text))];
			}
			else
			{
				pdfFont = CreatePdfFont(text, WordDocument.RenderHelper.GetFontStream(font2, scriptType), font2.Size, GetFontStyle(font2.Style));
				pdfFont.Ascent = GetAscent(font2, scriptType);
				PdfFontCollection.Add(GetPdfFontCollectionKey(font2, IsUnicode(text)), pdfFont);
			}
			PdfStringFormat pdfStringFormat = PDFGraphics.ConvertFormat(stringFormat, charFormat.ComplexScript || EnableComplexScript);
			if (FallbackFonts != null && FallbackFonts.Count > 0)
			{
				pdfFont = GetFallbackPdfFont(pdfFont, font2, text, scriptType, pdfStringFormat);
			}
			PdfBrush brush2 = new PdfSolidBrush(new PdfColor(Color.Gray));
			if (IsUnicode(text))
			{
				PDFDrawString(text, pdfFont, brush2, bounds.X - 0.2f, bounds.Y - 0.2f + num3, pdfStringFormat, charFormat, scriptType);
			}
			else
			{
				PDFGraphics.DrawString(text, pdfFont, brush2, new RectangleF(bounds.X - 0.2f, bounds.Y - 0.2f + num3, bounds.Width, bounds.Height), pdfStringFormat, directConversion: true);
			}
			textBounds = new RectangleF(bounds.X - 0.2f, bounds.Y - 0.2f + num3, bounds.Width, bounds.Height);
		}
		if (charFormat.Engrave)
		{
			DocGen.Drawing.Font font3 = ((!flag5) ? charFormat.Document.FontSettings.GetFont(charFormat.GetFontNameToRender(scriptType), charFormat.GetFontSizeToRender() / 1.5f, charFormat.GetFontToRender(scriptType).Style, scriptType) : charFormat.GetFontToRender(scriptType));
			if (!charFormat.GetFontNameToRender(scriptType).Equals(font3.Name, StringComparison.OrdinalIgnoreCase) && m_privateFontStream != null && m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType)))
			{
				font3 = GetPrivateFont(charFormat.GetFontNameToRender(scriptType), font3.Size, font3.Style);
			}
			PdfStringFormat pdfStringFormat2 = PDFGraphics.ConvertFormat(stringFormat, charFormat.ComplexScript || EnableComplexScript);
			PdfBrush brush3 = new PdfSolidBrush(new PdfColor(Color.Gray));
			if (IsUnicode(text))
			{
				PdfFont pdfFont2 = null;
				string text3 = GetEmbedFontStyle(charFormat);
				if (!string.IsNullOrEmpty(text3))
				{
					text3 = charFormat.GetFontNameToRender(scriptType) + "_" + text3;
				}
				if (m_privateFontStream != null && (m_privateFontStream.ContainsKey(text3) || m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType))))
				{
					pdfFont2 = CreatePdfFont(text3, charFormat.GetFontNameToRender(scriptType), font3.Size, GetFontStyle(font3.Style));
				}
				else
				{
					if (PdfFontCollection.ContainsKey(GetPdfFontCollectionKey(font3, IsUnicode(text))))
					{
						pdfFont2 = PdfFontCollection[GetPdfFontCollectionKey(font3, IsUnicode(text))];
					}
					else
					{
						pdfFont2 = CreatePdfFont(text, WordDocument.RenderHelper.GetFontStream(font3, scriptType), font3.Size, GetFontStyle(font3.Style));
						pdfFont2.Ascent = GetAscent(font3, scriptType);
						PdfFontCollection.Add(GetPdfFontCollectionKey(font3, IsUnicode(text)), pdfFont2);
					}
					if (FallbackFonts != null && FallbackFonts.Count > 0)
					{
						pdfFont2 = GetFallbackPdfFont(pdfFont2, font3, text, scriptType, pdfStringFormat2);
					}
					PDFDrawString(text, pdfFont2, brush3, bounds.X - 0.2f, bounds.Y - 0.2f, pdfStringFormat2, charFormat, scriptType);
				}
			}
			else
			{
				PdfFont pdfFont3 = null;
				string text4 = GetEmbedFontStyle(charFormat);
				if (!string.IsNullOrEmpty(text4))
				{
					text4 = charFormat.GetFontNameToRender(scriptType) + "_" + text4;
				}
				if (m_privateFontStream != null && (m_privateFontStream.ContainsKey(text4) || m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType))))
				{
					pdfFont3 = CreatePdfFont(text4, charFormat.GetFontNameToRender(scriptType), font3.Size, GetFontStyle(font3.Style));
				}
				else if (PdfFontCollection.ContainsKey(GetPdfFontCollectionKey(font3, IsUnicode(text))))
				{
					pdfFont3 = PdfFontCollection[GetPdfFontCollectionKey(font3, IsUnicode(text))];
				}
				else
				{
					pdfFont3 = CreatePdfFont(text, WordDocument.RenderHelper.GetFontStream(font3, scriptType), font3.Size, GetFontStyle(font.Style));
					pdfFont3.Ascent = GetAscent(font3, scriptType);
					PdfFontCollection.Add(GetPdfFontCollectionKey(font3, IsUnicode(text)), pdfFont3);
				}
				if (FallbackFonts != null && FallbackFonts.Count > 0)
				{
					pdfFont3 = GetFallbackPdfFont(pdfFont3, font3, text, scriptType, pdfStringFormat2);
				}
				PDFGraphics.DrawString(text, pdfFont3, brush3, new RectangleF(bounds.X - 0.2f, bounds.Y - 0.2f, bounds.Width, bounds.Height), pdfStringFormat2, directConversion: true);
			}
			textBounds = new RectangleF(bounds.X - 0.2f, bounds.Y - 0.2f + num3, bounds.Width, bounds.Height);
		}
		if (charFormat.AllCaps)
		{
			text = text.ToUpper();
		}
		if (!IsOwnerParagraphEmpty(text))
		{
			CharacterRangeType characterRangeType = ltWidget?.CharacterRange ?? CharacterRangeType.LTR;
			if (characterRangeType == CharacterRangeType.RTL)
			{
				stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
				DrawRTLText(scriptType, characterRangeType, text, charFormat, font, brush, bounds, stringFormat);
			}
			else if (IsUnicode(text))
			{
				DrawChineseText(scriptType, characterRangeType, text, charFormat, font, brush, bounds, stringFormat);
			}
			else
			{
				if (charFormat.BiDirectionalOverride == BiDirectionalOverride.RTL && text.Length > 0)
				{
					ReverseString(ref text);
				}
				ParagraphLayoutInfo paragraphLayoutInfo = null;
				if (ltWidget != null)
				{
					paragraphLayoutInfo = ltWidget.Widget.LayoutInfo as ParagraphLayoutInfo;
				}
				if (charFormat.SmallCaps && (paragraphLayoutInfo == null || paragraphLayoutInfo.CurrentListType != ListType.Bulleted || !IsListCharacter))
				{
					DrawSmallCapString(scriptType, characterRangeType, text, charFormat, bounds, font, stringFormat, brush, charFormat.CharacterSpacing != 0f);
				}
				else
				{
					DocGen.Drawing.Font font4 = ((!flag5) ? charFormat.Document.FontSettings.GetFont(fontNameToRender, GetSubSuperScriptFontSize(font), font.Style, scriptType) : font);
					if (!charFormat.GetFontNameToRender(scriptType).Equals(font4.Name, StringComparison.OrdinalIgnoreCase) && m_privateFontStream != null && m_privateFontStream != null && m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType)))
					{
						font4 = GetPrivateFont(charFormat.GetFontNameToRender(scriptType), font4.Size, font4.Style);
					}
					if (charFormat.CharacterSpacing != 0f && (ltWidget == null || (!IsTabWidget(ltWidget) && (currParagraph == null || !currParagraph.ParagraphFormat.Bidi || !(ltWidget.Widget.LayoutInfo is ParagraphLayoutInfo)))))
					{
						DrawStringBasedOnCharSpacing(scriptType, characterRangeType, font4, brush, bounds, text, stringFormat, charFormat);
					}
					else
					{
						PdfFont pdfFont4 = null;
						string text5 = GetEmbedFontStyle(charFormat);
						if (!string.IsNullOrEmpty(text5))
						{
							text5 = charFormat.GetFontNameToRender(scriptType) + "_" + text5;
						}
						if (m_privateFontStream != null && (m_privateFontStream.ContainsKey(text5) || m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType))))
						{
							pdfFont4 = CreatePdfFont(text5, charFormat.GetFontNameToRender(scriptType), font4.Size, GetFontStyle(font4.Style));
						}
						else if (PdfFontCollection.ContainsKey(GetPdfFontCollectionKey(font4, IsUnicode(text))))
						{
							pdfFont4 = PdfFontCollection[GetPdfFontCollectionKey(font4, IsUnicode(text))];
						}
						else
						{
							pdfFont4 = CreatePdfFont(text, WordDocument.RenderHelper.GetFontStream(font4, scriptType), font4.Size, GetFontStyle(font4.Style));
							pdfFont4.Ascent = GetAscent(font4, scriptType);
							PdfFontCollection.Add(GetPdfFontCollectionKey(font4, IsUnicode(text)), pdfFont4);
						}
						PdfStringFormat pdfStringFormat3 = PDFGraphics.ConvertFormat(stringFormat, charFormat.ComplexScript || EnableComplexScript);
						PdfBrush brush4 = new PdfSolidBrush(black);
						if (FallbackFonts != null && FallbackFonts.Count > 0)
						{
							pdfFont4 = GetFallbackPdfFont(pdfFont4, font4, text, scriptType, pdfStringFormat3);
						}
						PDFGraphics.DrawString(text, pdfFont4, brush4, new RectangleF(bounds.X, bounds.Y + num3, bounds.Width, bounds.Height), pdfStringFormat3, directConversion: true);
					}
					textBounds = new RectangleF(bounds.X, bounds.Y + num3, bounds.Width, bounds.Height);
				}
			}
		}
		if ((ExportBookmarks & ExportBookmarkType.Headings) != 0 && (currParagraph != null || paraFormat != null) && !currParagraph.IsInCell)
		{
			WParagraphStyle style = currParagraph.ParaStyle as WParagraphStyle;
			int headingLevel = currParagraph.GetHeadingLevel(style, currParagraph);
			if (headingLevel >= 0 && headingLevel < 9 && !currParagraph.OmitHeadingStyles() && currParagraphIndex != currParagraph.Index)
			{
				string bookmarkName = (IsListCharacter ? (text + "\t" + currParagraph.GetDisplayText(currParagraph.Items)) : currParagraph.GetDisplayText(currParagraph.Items));
				Bookmarks.Add(new BookmarkPosition(bookmarkName, DocumentLayouter.PageNumber, bounds, headingLevel));
				currParagraphIndex = currParagraph.Index;
			}
		}
		if (flag2)
		{
			bool isSameLine = false;
			CheckPreOrNextSiblingIsTab(ref charFormat, ref textBounds, ltWidget, ref isSameLine);
			AddLineToCollection(text, flag5, font, charFormat, textBounds, isSameLine, scriptType);
		}
		ResetTransform();
		if (flag4 || clipTopPosition > 0f)
		{
			ResetClip();
		}
		IsListCharacter = false;
	}

	private void Rotate(float ang, ref bool flipV, ref bool flipH, TextWrappingStyle textWrappingStyle, bool doNotRotateText, bool isNeedToScale, ref bool isRotateTransformApplied, ref float rotationAngle)
	{
		if ((ang == 0f && (ang != 0f || !(flipH | flipV))) || textWrappingStyle == TextWrappingStyle.Tight || textWrappingStyle == TextWrappingStyle.Through)
		{
			return;
		}
		if (ang > 360f)
		{
			ang %= 360f;
		}
		if ((ang != 0f) | flipV | flipH)
		{
			if (flipV)
			{
				flipH = true;
			}
			else if (flipH)
			{
				flipH = false;
			}
			if (!doNotRotateText && !isNeedToScale)
			{
				isRotateTransformApplied = true;
				SetRotateTransform(m_rotateTransform, ang, flipV, flipH);
			}
			rotationAngle = ang;
		}
	}

	private void CheckPreOrNextSiblingIsTab(ref WCharacterFormat charFormat, ref RectangleF textBounds, LayoutedWidget ltWidget, ref bool isSameLine)
	{
		TabsLayoutInfo tabsLayoutInfo = ((charFormat.OwnerBase is WTextRange) ? ((charFormat.OwnerBase as WTextRange).m_layoutInfo as TabsLayoutInfo) : null);
		if (tabsLayoutInfo != null && tabsLayoutInfo.CurrTabLeader != 0)
		{
			SizeF sizeF = MeasureTextRange(charFormat.OwnerBase as WTextRange, " ");
			textBounds.X -= sizeF.Width;
		}
		if (ltWidget == null)
		{
			return;
		}
		LayoutedWidget owner = ltWidget.Owner;
		for (int i = 0; i < owner.ChildWidgets.Count; i++)
		{
			LayoutedWidget layoutedWidget = owner.ChildWidgets[i];
			if (!(layoutedWidget.Widget.LayoutInfo is WTextRange.LayoutTabInfo) || charFormat.UnderlineStyle == DocGen.Drawing.UnderlineStyle.None || (!IsSame(layoutedWidget.Bounds.X, textBounds.Right, 1) && !IsSame(layoutedWidget.Bounds.Right, textBounds.X, 1)))
			{
				continue;
			}
			WCharacterFormat characterFormat = (layoutedWidget.Widget as WTextRange).CharacterFormat;
			bool flag = (!charFormat.Strikeout && !charFormat.DoubleStrike) || charFormat.GetFontSizeToRender() == characterFormat.GetFontSizeToRender();
			if (characterFormat.UnderlineColor == charFormat.UnderlineColor && charFormat.Position == characterFormat.Position && characterFormat.UnderlineStyle == charFormat.UnderlineStyle && charFormat.SubSuperScript == DocGen.DocIO.DLS.SubSuperScript.None && GetTextColor(characterFormat) == GetTextColor(charFormat) && flag)
			{
				charFormat = characterFormat;
				textBounds.Y = layoutedWidget.Bounds.Y;
				if (IsSame(layoutedWidget.Bounds.Right, textBounds.X, 1))
				{
					isSameLine = true;
				}
			}
			break;
		}
	}

	private List<RectangleF> CalculateTextBounds(string text, RectangleF textBounds, DocGen.Drawing.Font font, StringFormat format, WCharacterFormat charFormat, FontScriptType scriptType)
	{
		_ = charFormat.OwnerBase;
		List<RectangleF> list = new List<RectangleF>();
		RectangleF rectangleF = default(RectangleF);
		rectangleF = textBounds;
		string text2 = null;
		int num = 0;
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] != ' ')
			{
				text2 += text[i];
				if (num != 0)
				{
					rectangleF.Width = 0f;
					for (int j = 0; j < num; j++)
					{
						SizeF sizeF = MeasureString(" ", font, format, FontScriptType.English);
						rectangleF.Width += sizeF.Width;
					}
					rectangleF.X += rectangleF.Width;
					num = 0;
				}
			}
			else if (text[i] == ' ')
			{
				num++;
				if (text2 != null)
				{
					rectangleF.Width = MeasureString(text2, font, format, null, isMeasureFromTabList: false, scriptType).Width;
					list.Add(rectangleF);
					rectangleF.X += rectangleF.Width;
					rectangleF.Width = 0f;
					text2 = null;
				}
			}
		}
		if (text2 != null)
		{
			rectangleF.Width = MeasureString(text2, font, format, null, isMeasureFromTabList: false, scriptType).Width;
			list.Add(rectangleF);
		}
		if (list.Count > 0)
		{
			return list;
		}
		return null;
	}

	private void AddLineToCollection(string text, bool isSubSuperScriptNone, DocGen.Drawing.Font font, WCharacterFormat charFormat, RectangleF textBounds, bool isSameLine, FontScriptType scriptType)
	{
		DocGen.Drawing.Font font2 = ((!isSubSuperScriptNone) ? charFormat.Document.FontSettings.GetFont(font.Name, font.Size / 1.5f, font.Style, scriptType) : font);
		if (FallbackFonts != null && FallbackFonts.Count > 0)
		{
			font2 = WordDocument.RenderHelper.GetFallbackFont(font2, text, (charFormat.OwnerBase as WTextRange).ScriptType, FallbackFonts, FontStreams);
		}
		WCharacterFormat wCharacterFormat = null;
		StringFormat stringFormat = new StringFormat();
		stringFormat.FormatFlags = StringFormatFlags.MeasureTrailingSpaces;
		stringFormat.Trimming = StringTrimming.None;
		RectangleF rectangleF = default(RectangleF);
		List<RectangleF> list = new List<RectangleF>();
		if (charFormat.UnderlineStyle != 0)
		{
			if (underLineValues != null)
			{
				foreach (WCharacterFormat key in underLineValues.Keys)
				{
					wCharacterFormat = key;
				}
			}
			else
			{
				underLineValues = new Dictionary<WCharacterFormat, List<RectangleF>>();
			}
			if (wCharacterFormat != null)
			{
				rectangleF = underLineValues[wCharacterFormat][underLineValues[wCharacterFormat].Count - 1];
				if ((charFormat.SubSuperScript == DocGen.DocIO.DLS.SubSuperScript.SuperScript && wCharacterFormat.SubSuperScript == DocGen.DocIO.DLS.SubSuperScript.None) || (charFormat.Position > 0f && wCharacterFormat.Position == 0f))
				{
					if (charFormat.UnderlineStyle == DocGen.Drawing.UnderlineStyle.Words)
					{
						List<RectangleF> list2 = CalculateTextBounds(text, textBounds, font2, stringFormat, charFormat, scriptType);
						if (list2 != null)
						{
							foreach (RectangleF item in list2)
							{
								if (rectangleF.Right >= item.X)
								{
									rectangleF.Width += item.Width;
									underLineValues[wCharacterFormat][underLineValues[wCharacterFormat].Count - 1] = rectangleF;
									rectangleF = underLineValues[wCharacterFormat][underLineValues[wCharacterFormat].Count - 1];
								}
								else
								{
									list.Add(item);
								}
							}
							if (list.Count > 0 && !underLineValues.ContainsKey(charFormat))
							{
								underLineValues.Add(charFormat, list);
							}
						}
					}
					else if (IsSameLine(rectangleF.Right, textBounds.X, charFormat, wCharacterFormat, isPositionRaisedOrSuperScript: true) || IsSameLine(rectangleF.X, textBounds.Right, charFormat, wCharacterFormat, isPositionRaisedOrSuperScript: true))
					{
						rectangleF.Width = textBounds.X + textBounds.Width - rectangleF.X;
						underLineValues[wCharacterFormat][underLineValues[wCharacterFormat].Count - 1] = rectangleF;
					}
					else if (!underLineValues.ContainsKey(charFormat))
					{
						list.Add(textBounds);
						underLineValues.Add(charFormat, list);
					}
				}
				else if ((charFormat.SubSuperScript == DocGen.DocIO.DLS.SubSuperScript.None && wCharacterFormat.SubSuperScript == DocGen.DocIO.DLS.SubSuperScript.SuperScript) || (charFormat.Position == 0f && wCharacterFormat.Position > 0f))
				{
					if (charFormat.UnderlineStyle == DocGen.Drawing.UnderlineStyle.Words)
					{
						List<RectangleF> list3 = CalculateTextBounds(text, textBounds, font2, stringFormat, charFormat, scriptType);
						if (list3 != null && !underLineValues.ContainsKey(charFormat))
						{
							underLineValues.Add(charFormat, list3);
						}
					}
					else if (IsSameLine(rectangleF.Right, textBounds.X, charFormat, wCharacterFormat, isPositionRaisedOrSuperScript: true) || IsSameLine(rectangleF.X, textBounds.Right, charFormat, wCharacterFormat, isPositionRaisedOrSuperScript: true))
					{
						rectangleF.Width += textBounds.Width;
						underLineValues.Remove(wCharacterFormat);
						list.Add(rectangleF);
						if (!underLineValues.ContainsKey(charFormat))
						{
							underLineValues.Add(charFormat, list);
						}
					}
					else if (!underLineValues.ContainsKey(charFormat))
					{
						list.Add(textBounds);
						underLineValues.Add(charFormat, list);
					}
				}
				else if (isSameLine || IsSameLine(rectangleF.Right, textBounds.X, charFormat, wCharacterFormat, isPositionRaisedOrSuperScript: false) || IsSameLine(rectangleF.X, textBounds.Right, charFormat, wCharacterFormat, isPositionRaisedOrSuperScript: false))
				{
					if (IsSameLine(rectangleF.X, textBounds.Right, charFormat, wCharacterFormat, isPositionRaisedOrSuperScript: false))
					{
						rectangleF.X = textBounds.X;
					}
					rectangleF.Width += textBounds.Width;
					if (charFormat.UnderlineStyle == DocGen.Drawing.UnderlineStyle.Words)
					{
						List<RectangleF> list4 = CalculateTextBounds(text, textBounds, font2, stringFormat, charFormat, scriptType);
						if (list4 != null && !underLineValues.ContainsKey(charFormat))
						{
							underLineValues.Add(charFormat, list4);
						}
					}
					else if (font2.Size > wCharacterFormat.GetFontSizeToRender())
					{
						rectangleF.Y = textBounds.Y;
						underLineValues.Remove(wCharacterFormat);
						list.Add(rectangleF);
						if (!underLineValues.ContainsKey(charFormat))
						{
							underLineValues.Add(charFormat, list);
						}
					}
					else
					{
						list.Add(rectangleF);
						underLineValues[wCharacterFormat] = list;
					}
				}
				else if (charFormat.UnderlineStyle == DocGen.Drawing.UnderlineStyle.Words)
				{
					List<RectangleF> list5 = CalculateTextBounds(text, textBounds, font2, stringFormat, charFormat, scriptType);
					if (list5 != null && !underLineValues.ContainsKey(charFormat))
					{
						underLineValues.Add(charFormat, list5);
					}
				}
				else if (!underLineValues.ContainsKey(charFormat))
				{
					list.Add(textBounds);
					underLineValues.Add(charFormat, list);
				}
			}
			else if (charFormat.UnderlineStyle == DocGen.Drawing.UnderlineStyle.Words)
			{
				List<RectangleF> list6 = CalculateTextBounds(text, textBounds, font2, stringFormat, charFormat, scriptType);
				if (list6 != null && !underLineValues.ContainsKey(charFormat))
				{
					underLineValues.Add(charFormat, list6);
				}
			}
			else if (!underLineValues.ContainsKey(charFormat))
			{
				list.Add(textBounds);
				underLineValues.Add(charFormat, list);
			}
		}
		wCharacterFormat = null;
		if (!charFormat.Strikeout && !charFormat.DoubleStrike)
		{
			return;
		}
		if (strikeThroughValues != null)
		{
			foreach (WCharacterFormat key2 in strikeThroughValues.Keys)
			{
				wCharacterFormat = key2;
			}
		}
		else
		{
			strikeThroughValues = new Dictionary<WCharacterFormat, RectangleF>();
		}
		if (wCharacterFormat != null && font2.Size == wCharacterFormat.GetFontSizeToRender() && charFormat.Position == wCharacterFormat.Position && charFormat.SubSuperScript == wCharacterFormat.SubSuperScript && IsSame(strikeThroughValues[wCharacterFormat].Right, textBounds.X, 2))
		{
			rectangleF = strikeThroughValues[wCharacterFormat];
			rectangleF.Width += textBounds.Width;
			strikeThroughValues[wCharacterFormat] = rectangleF;
		}
		else if (!strikeThroughValues.ContainsKey(charFormat))
		{
			strikeThroughValues.Add(charFormat, textBounds);
		}
	}

	private bool IsSameLine(float boundsRight, float boundsX, WCharacterFormat charFormat, WCharacterFormat preCharFormat, bool isPositionRaisedOrSuperScript)
	{
		if (isPositionRaisedOrSuperScript)
		{
			if (IsSame(boundsRight, boundsX, 1) && preCharFormat.UnderlineColor == charFormat.UnderlineColor && preCharFormat.UnderlineStyle == charFormat.UnderlineStyle && GetTextColor(preCharFormat) == GetTextColor(charFormat))
			{
				return true;
			}
		}
		else if (IsSame(boundsRight, boundsX, 1) && preCharFormat.UnderlineColor == charFormat.UnderlineColor && charFormat.Position == preCharFormat.Position && preCharFormat.UnderlineStyle == charFormat.UnderlineStyle && charFormat.SubSuperScript == DocGen.DocIO.DLS.SubSuperScript.None && GetTextColor(preCharFormat) == GetTextColor(charFormat))
		{
			return true;
		}
		return false;
	}

	private bool IsSame(float value1, float value2, int digit)
	{
		if (Math.Round(value1, digit) == Math.Round(value2, digit))
		{
			return true;
		}
		return false;
	}

	private bool HasUnderlineOrStricthrough(WTextRange txtRange, WCharacterFormat charFormat, FontScriptType scriptType)
	{
		bool result = false;
		if (!IsListCharacter && currTextRange != null)
		{
			if (txtRange.CharacterFormat != null && (!(txtRange.CharacterFormat.CharStyleName == "Hyperlink") || !IsTOC(txtRange)))
			{
				result = true;
			}
			if (txtRange != null && txtRange.Text.Trim(' ') == string.Empty && !(((IWidget)txtRange).LayoutInfo is TabsLayoutInfo))
			{
				if (txtRange.NextSibling == null)
				{
					result = false;
				}
				else if (txtRange.NextSibling != null)
				{
					Entity entity = txtRange.NextSibling as Entity;
					while (entity is WTextRange && (entity as WTextRange).Text.Trim(' ') == string.Empty && !((entity as WTextRange).m_layoutInfo is TabsLayoutInfo))
					{
						entity = entity.NextSibling as Entity;
						if (entity == null)
						{
							break;
						}
					}
					if ((entity == null || !(entity is WTextRange) || (!((entity as WTextRange).Text.Trim(' ') != string.Empty) && !((entity as WTextRange).m_layoutInfo is TabsLayoutInfo))) && (!(entity is InlineContentControl) || !IsContentControlHavingTextRange(entity as InlineContentControl)))
					{
						result = false;
					}
				}
			}
		}
		else
		{
			if (charFormat.UnderlineStyle != 0 || charFormat.Strikeout || charFormat.DoubleStrike)
			{
				result = true;
			}
			if (charFormat.CharStyleName != null && charFormat.CharStyleName.ToLower().Equals("hyperlink") && (currTextRange == null || !IsTOC(currTextRange)) && !charFormat.HasKey(7))
			{
				result = true;
			}
			if (IsListCharacter && currParagraph != null && currParagraph.ListFormat.ListType != ListType.NoList && currParagraph.ListFormat.CurrentListLevel != null)
			{
				if (currParagraph.ListFormat.CurrentListLevel.CharacterFormat.GetFontToRender(scriptType).Underline)
				{
					result = true;
				}
				else if (charFormat.UnderlineStyle != 0)
				{
					result = false;
				}
			}
		}
		return result;
	}

	private bool IsContentControlHavingTextRange(InlineContentControl inlineContentControl)
	{
		Entity entity = inlineContentControl;
		for (int i = 0; i < inlineContentControl.ParagraphItems.Count; i++)
		{
			entity = inlineContentControl.ParagraphItems[i];
			if (entity is InlineContentControl)
			{
				return IsContentControlHavingTextRange(entity as InlineContentControl);
			}
			if (!(entity is BookmarkStart) && !(entity is BookmarkEnd))
			{
				break;
			}
		}
		if (entity is WTextRange wTextRange && (wTextRange.Text.Trim(' ') != string.Empty || wTextRange.m_layoutInfo is TabsLayoutInfo))
		{
			return true;
		}
		return false;
	}

	internal PdfFont CreatePdfFont(string styleName, string fontName, float size, PdfFontStyle style)
	{
		Stream stream = null;
		string text = string.Empty;
		if (m_privateFontStream.ContainsKey(styleName))
		{
			text = styleName;
			stream = m_privateFontStream[styleName];
		}
		else if (m_privateFontStream.ContainsKey(fontName))
		{
			text = fontName;
			stream = m_privateFontStream[fontName];
		}
		text = text + ";" + size.ToString(CultureInfo.InvariantCulture) + ";" + style;
		if (stream != null && stream.CanRead)
		{
			PdfFont pdfFont = null;
			if (SubstitutedFonts.ContainsKey(text))
			{
				pdfFont = SubstitutedFonts[text];
			}
			else
			{
				stream.Position = 0L;
				byte[] array = new byte[stream.Length];
				stream.Read(array, 0, array.Length);
				pdfFont = new PdfTrueTypeFont(new MemoryStream(array), size, isEnableEmbedding: true, style);
				SubstitutedFonts.Add(text, pdfFont);
			}
			return pdfFont;
		}
		return null;
	}

	internal string GetEmbedFontStyle(WCharacterFormat format)
	{
		if (format.GetBoldToRender() && format.GetItalicToRender())
		{
			return "BoldItalic";
		}
		if (format.GetBoldToRender())
		{
			return "Bold";
		}
		if (format.GetItalicToRender())
		{
			return "Italic";
		}
		return "Regular";
	}

	private DocGen.Drawing.Font GetPrivateFont(string fontName, float fontsize, FontStyle style)
	{
		if (string.IsNullOrEmpty(fontName))
		{
			return null;
		}
		return new DocGen.Drawing.Font(fontName, fontsize, style);
	}

	private bool IsTabWidget(LayoutedWidget ltWidget)
	{
		if (ltWidget.Widget is WTextRange)
		{
			if (!((ltWidget.Widget as WTextRange).m_layoutInfo is TabsLayoutInfo))
			{
				return false;
			}
			return ((ltWidget.Widget as WTextRange).m_layoutInfo as TabsLayoutInfo).CurrTabLeader == DocGen.Layouting.TabLeader.NoLeader;
		}
		return false;
	}

	private LayoutedWidget GetTextBoxWidget(LayoutedWidget ltWidget)
	{
		LayoutedWidget result = null;
		while (ltWidget != null)
		{
			ltWidget = ltWidget.Owner;
			if (ltWidget != null && ltWidget.Widget is WTextBox)
			{
				result = ltWidget;
				break;
			}
		}
		return result;
	}

	private void ReverseString(ref string text)
	{
		char[] array = text.ToCharArray();
		string text2 = string.Empty;
		for (int num = array.Length - 1; num > -1; num--)
		{
			text2 += array[num];
		}
		text = text2;
	}

	private void DrawSmallCapString(FontScriptType scriptType, CharacterRangeType characterRangeType, string text, WCharacterFormat charFormat, RectangleF bounds, DocGen.Drawing.Font font, StringFormat format, DocGen.Drawing.SkiaSharpHelper.Brush textBrush, bool isCharacterSpacing)
	{
		DocGen.Drawing.Font font2 = charFormat.Document.FontSettings.GetFont(font.Name, (float)(((double)font.Size * 0.8 > 3.0) ? ((double)font.Size * 0.8) : 2.0), font.Style, scriptType);
		float num = 0f;
		SizeF empty = SizeF.Empty;
		float ascent = GetAscent(font, scriptType);
		ascent = ((ascent > bounds.Height) ? bounds.Height : ascent);
		float ascent2 = GetAscent(font2, scriptType);
		ascent2 = ((ascent2 > bounds.Height) ? bounds.Height : ascent2);
		string text2 = string.Empty;
		string text3 = string.Empty;
		List<char> list = new List<char>();
		List<string> list2 = new List<string>();
		float num2 = 0f;
		string fontNameToRender = charFormat.GetFontNameToRender(scriptType);
		if (!IsListCharacter && (fontNameToRender == "DIN Offc" || fontNameToRender == "DIN OT") && currParagraph != null && currParagraph.IsContainDinOffcFont())
		{
			num2 = bounds.Height * 0.2f;
		}
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			if (char.IsUpper(c) || (!char.IsLetter(c) && !c.Equals(ControlChar.SpaceChar)))
			{
				if (text3.Length != 0)
				{
					list.Add('s');
					list2.Add(text3.ToUpper());
					text3 = string.Empty;
				}
				text2 += c;
			}
			else
			{
				if (text2.Length != 0)
				{
					list.Add('c');
					list2.Add(text2);
					text2 = string.Empty;
				}
				text3 += c;
			}
		}
		if (text3.Length != 0)
		{
			list.Add('s');
			list2.Add(text3.ToUpper());
			text3 = string.Empty;
		}
		else if (text2.Length != 0)
		{
			list.Add('c');
			list2.Add(text2);
			text2 = string.Empty;
		}
		for (int j = 0; j < list.Count; j++)
		{
			if (list[j] == 'c')
			{
				empty = MeasureString(list2[j], font, format, charFormat, isMeasureFromTabList: false, isMeasureFromSmallCapString: true, scriptType);
				DocGen.Drawing.Font font3 = ((charFormat != null && charFormat.SubSuperScript != 0) ? charFormat.Document.FontSettings.GetFont(font.Name, GetSubSuperScriptFontSize(font), font.Style, scriptType) : font);
				if (!charFormat.GetFontNameToRender(scriptType).Equals(font3.Name, StringComparison.OrdinalIgnoreCase) && m_privateFontStream != null && m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType)))
				{
					font3 = GetPrivateFont(charFormat.GetFontNameToRender(scriptType), font3.Size, font3.Style);
				}
				if (isCharacterSpacing)
				{
					DrawStringBasedOnCharSpacing(scriptType, characterRangeType, font3, textBrush, new RectangleF(bounds.X + num, bounds.Y, empty.Width, bounds.Height), list2[j], format, charFormat);
				}
				else
				{
					PdfFont pdfFont = null;
					string text4 = GetEmbedFontStyle(charFormat);
					if (!string.IsNullOrEmpty(text4))
					{
						text4 = charFormat.GetFontNameToRender(scriptType) + "_" + text4;
					}
					if (m_privateFontStream != null && ((!string.IsNullOrEmpty(text4) && m_privateFontStream.ContainsKey(text4)) || (!string.IsNullOrEmpty(charFormat.GetFontNameToRender(scriptType)) && m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType)))))
					{
						pdfFont = CreatePdfFont(text4, charFormat.GetFontNameToRender(scriptType), font3.Size, GetFontStyle(font3.Style));
					}
					else if (PdfFontCollection.ContainsKey(GetPdfFontCollectionKey(font3, IsUnicode(list2[j]))))
					{
						pdfFont = PdfFontCollection[GetPdfFontCollectionKey(font3, IsUnicode(list2[j]))];
					}
					else
					{
						pdfFont = CreatePdfFont(list2[j], WordDocument.RenderHelper.GetFontStream(font3, scriptType), font3.Size, GetFontStyle(font3.Style));
						pdfFont.Ascent = GetAscent(font3, scriptType);
						PdfFontCollection.Add(GetPdfFontCollectionKey(font3, IsUnicode(list2[j])), pdfFont);
					}
					PdfStringFormat pdfStringFormat = PDFGraphics.ConvertFormat(format, charFormat.ComplexScript || EnableComplexScript);
					PdfBrush pDFBrush = GetPDFBrush(GetTextColor(charFormat));
					if (FallbackFonts != null && FallbackFonts.Count > 0)
					{
						pdfFont = GetFallbackPdfFont(pdfFont, font3, list2[j], scriptType, pdfStringFormat);
					}
					PDFGraphics.DrawString(list2[j], pdfFont, pDFBrush, new RectangleF(bounds.X + num, bounds.Y + num2, bounds.Width, bounds.Height), pdfStringFormat, directConversion: true);
				}
				num += empty.Width;
				continue;
			}
			empty = MeasureString(list2[j].ToLower(), font, format, charFormat, isMeasureFromTabList: false, isMeasureFromSmallCapString: true, scriptType);
			DocGen.Drawing.Font font4 = ((charFormat != null && charFormat.SubSuperScript != 0) ? charFormat.Document.FontSettings.GetFont(font2.Name, GetSubSuperScriptFontSize(font2), font2.Style, scriptType) : font2);
			if (isCharacterSpacing)
			{
				DrawStringBasedOnCharSpacing(scriptType, characterRangeType, font4, textBrush, new RectangleF(bounds.X + num, bounds.Y + (ascent - ascent2), empty.Width, bounds.Height), list2[j], format, charFormat);
			}
			else
			{
				PdfFont pdfFont2 = null;
				string text5 = GetEmbedFontStyle(charFormat);
				if (!string.IsNullOrEmpty(text5))
				{
					text5 = charFormat.GetFontNameToRender(scriptType) + "_" + text5;
				}
				if (m_privateFontStream != null && ((!string.IsNullOrEmpty(text5) && m_privateFontStream.ContainsKey(text5)) || (!string.IsNullOrEmpty(charFormat.GetFontNameToRender(scriptType)) && m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType)))))
				{
					pdfFont2 = CreatePdfFont(text5, charFormat.GetFontNameToRender(scriptType), font4.Size, GetFontStyle(font4.Style));
				}
				else if (PdfFontCollection.ContainsKey(GetPdfFontCollectionKey(font4, IsUnicode(list2[j]))))
				{
					pdfFont2 = PdfFontCollection[GetPdfFontCollectionKey(font4, IsUnicode(list2[j]))];
				}
				else
				{
					pdfFont2 = CreatePdfFont(list2[j], WordDocument.RenderHelper.GetFontStream(font4, scriptType), font4.Size, GetFontStyle(font4.Style));
					pdfFont2.Ascent = GetAscent(font4, scriptType);
					PdfFontCollection.Add(GetPdfFontCollectionKey(font4, IsUnicode(list2[j])), pdfFont2);
				}
				PdfStringFormat pdfStringFormat2 = PDFGraphics.ConvertFormat(format, charFormat.ComplexScript || EnableComplexScript);
				PdfBrush brush = new PdfSolidBrush(GetTextColor(charFormat));
				if (FallbackFonts != null && FallbackFonts.Count > 0)
				{
					pdfFont2 = GetFallbackPdfFont(pdfFont2, font4, list2[j], scriptType, pdfStringFormat2);
				}
				PDFGraphics.DrawString(list2[j], pdfFont2, brush, new RectangleF(bounds.X + num, bounds.Y + (ascent - ascent2) + num2, bounds.Width, bounds.Height), pdfStringFormat2, directConversion: true);
			}
			num += empty.Width;
		}
	}

	private bool IsTextNeedToClip(LayoutedWidget ltWidget)
	{
		if (ltWidget != null && !ltWidget.Widget.LayoutInfo.IsVerticalText && currParagraph != null && currParagraph.IsInCell)
		{
			LayoutedWidget owner = ltWidget.Owner;
			LayoutedWidget layoutedWidget = ltWidget.Owner;
			while (owner != null && (!(owner.Widget is WTableRow) || !(owner.Widget is Shape)))
			{
				if (owner.Widget is WTableCell)
				{
					layoutedWidget = owner;
				}
				owner = owner.Owner;
			}
			if (layoutedWidget != null && layoutedWidget.Widget is WTableCell && (!(layoutedWidget.Widget.LayoutInfo is CellLayoutInfo) || !(layoutedWidget.Widget.LayoutInfo as CellLayoutInfo).IsRowMergeStart) && owner != null && owner.Widget is WTableRow)
			{
				return Math.Round(ltWidget.Bounds.Y, 2) >= Math.Round(owner.Bounds.Bottom, 2);
			}
			if (owner != null && owner.Widget is Shape)
			{
				return Math.Round(ltWidget.Bounds.Y, 2) >= Math.Round((owner.Widget as Shape).TextLayoutingBounds.Bottom, 2);
			}
			if (owner != null && owner.Widget is ChildShape)
			{
				return Math.Round(ltWidget.Bounds.Y, 2) >= Math.Round((owner.Widget as ChildShape).TextLayoutingBounds.Bottom, 2);
			}
			return false;
		}
		return false;
	}

	private bool IsWidgetNeedToClipBasedOnXPosition(LayoutedWidget ltWidget, ref float clipWidth, RectangleF bounds)
	{
		float num = -1f;
		if (ltWidget != null && !ltWidget.Widget.LayoutInfo.IsVerticalText && currParagraph != null)
		{
			LayoutedWidget owner = ltWidget.Owner;
			WTextBox wTextBox = ((owner.Widget is Entity) ? (GetBaseEntity(owner.Widget as Entity) as WTextBox) : null);
			if (currParagraph.IsInCell)
			{
				LayoutedWidget layoutedWidget = ltWidget.Owner;
				while (owner != null && !(owner.Widget is WTableRow))
				{
					if (owner.Widget is WTableCell)
					{
						layoutedWidget = owner;
					}
					owner = owner.Owner;
				}
				if (layoutedWidget != null && layoutedWidget.Widget is WTableCell)
				{
					num = layoutedWidget.Bounds.Right;
				}
			}
			else if (currParagraph.OwnerTextBody.Owner is Shape)
			{
				num = (currParagraph.OwnerTextBody.Owner as Shape).TextLayoutingBounds.Right;
			}
			else if (wTextBox != null)
			{
				num = wTextBox.TextLayoutingBounds.Right;
				if (wTextBox.TextBoxFormat.Width > 0f)
				{
					num += wTextBox.TextBoxFormat.InternalMargin.Right;
				}
			}
			else if (currParagraph.ParagraphFormat.IsInFrame())
			{
				num = bounds.Right;
			}
			else
			{
				Entity baseEntity = GetBaseEntity(currParagraph);
				while (owner != null && !(owner.Widget is WSection) && (!(owner.Widget is SplitWidgetContainer) || !((owner.Widget as SplitWidgetContainer).RealWidgetContainer is WSection)))
				{
					owner = owner.Owner;
				}
				if (owner != null && baseEntity is WSection && (baseEntity as WSection).Columns.Count > 1)
				{
					int columnIndex = GetColumnIndex(baseEntity as WSection, owner.Bounds);
					if (columnIndex != (baseEntity as WSection).Columns.Count - 1)
					{
						num = owner.Bounds.X + (baseEntity as WSection).Columns[columnIndex].Width + (baseEntity as WSection).Columns[columnIndex].Space / 2f;
					}
					ParagraphLayoutInfo paragraphLayoutInfo = ((IWidget)currParagraph).LayoutInfo as ParagraphLayoutInfo;
					float num2 = paragraphLayoutInfo.Margins.Left + paragraphLayoutInfo.Margins.Right + (paragraphLayoutInfo.IsFirstLine ? paragraphLayoutInfo.FirstLineIndent : 0f);
					num = ((num > 0f && owner.Bounds.X + num2 > num) ? (-1f) : num);
				}
			}
		}
		if (num > 0f && bounds.X > num)
		{
			return true;
		}
		if (num > 0f && bounds.Right > num)
		{
			clipWidth = num - bounds.X;
		}
		return false;
	}

	public int GetColumnIndex(WSection section, RectangleF sectionBounds)
	{
		int result = 0;
		float num = m_pageMarginLeft;
		for (int i = 0; i < section.Columns.Count; i++)
		{
			num += section.Columns[i].Width;
			if (sectionBounds.X < num)
			{
				result = i;
				break;
			}
			num += section.Columns[i].Space;
		}
		return result;
	}

	private float GetClipTopPosition(RectangleF bounds, bool isInlinePicture)
	{
		float result = 0f;
		float num = 0f;
		LineSpacingRule lineSpacingRule = LineSpacingRule.Multiple;
		if (currParagraph != null)
		{
			num = Math.Abs(currParagraph.ParagraphFormat.LineSpacing);
			lineSpacingRule = currParagraph.ParagraphFormat.LineSpacingRule;
		}
		if (currParagraph != null && (lineSpacingRule == LineSpacingRule.Exactly || (bounds.Height * (num / 12f) < 12f && lineSpacingRule == LineSpacingRule.Multiple && !isInlinePicture)) && num < bounds.Height)
		{
			float num2 = 0f;
			if (((IWidget)currParagraph).LayoutInfo is ParagraphLayoutInfo paragraphLayoutInfo)
			{
				num2 = paragraphLayoutInfo.Margins.Top;
			}
			if (bounds.Height * (num / 12f) < 12f && lineSpacingRule == LineSpacingRule.Multiple)
			{
				num = bounds.Height * (num / 12f);
			}
			if (num2 + num < bounds.Height)
			{
				result = (bounds.Height - num - num2) / 2f;
			}
		}
		return result;
	}

	public DocGen.Drawing.Font GetDefaultFont(FontScriptType scriptType, DocGen.Drawing.Font font, WCharacterFormat charFormat)
	{
		if (charFormat.HasValue(72) && !TextSplitter.IsEastAsiaScript(scriptType))
		{
			string fontNameFromHint = charFormat.GetFontNameFromHint(scriptType);
			if (charFormat.FontNameNonFarEast != fontNameFromHint && !charFormat.IsThemeFont(charFormat.FontNameNonFarEast))
			{
				return charFormat.Document.FontSettings.GetFont(charFormat.FontNameNonFarEast, font.Size, font.Style, scriptType);
			}
		}
		return font;
	}

	internal void DrawStringBasedOnCharSpacing(FontScriptType scriptType, CharacterRangeType characterRangeType, DocGen.Drawing.Font font, DocGen.Drawing.SkiaSharpHelper.Brush textBrush, RectangleF bounds, string text, StringFormat format, WCharacterFormat charFormat)
	{
		if (scriptType != 0 && FontScriptType.Arabic.HasFlag(scriptType))
		{
			ArabicShapeRenderer arabicShapeRenderer = new ArabicShapeRenderer();
			text = arabicShapeRenderer.Shape(text.ToCharArray(), 0);
			arabicShapeRenderer.Dispose();
		}
		if (charFormat.CharacterSpacing > 0f)
		{
			DrawStringBasedOnExpandedCharSpacing(scriptType, font, bounds, text, format, charFormat);
			return;
		}
		if ((format.FormatFlags & StringFormatFlags.DirectionRightToLeft) == StringFormatFlags.DirectionRightToLeft && characterRangeType == CharacterRangeType.RTL)
		{
			char[] array = text.ToCharArray();
			Array.Reverse(array);
			text = new string(array);
		}
		float num = 0f;
		string text2 = text;
		for (int i = 0; i < text2.Length; i++)
		{
			num += MeasureString(text2[i].ToString(), font, format, scriptType).Width;
		}
		float num2 = (bounds.Width - num) / (float)text.Length;
		float num3 = 0f;
		string fontNameToRender = charFormat.GetFontNameToRender(scriptType);
		if (!IsListCharacter && (fontNameToRender == "DIN Offc" || fontNameToRender == "DIN OT") && currParagraph != null && currParagraph.IsContainDinOffcFont())
		{
			num3 = bounds.Height * 0.2f;
		}
		float num4 = 0f;
		text2 = text;
		for (int i = 0; i < text2.Length; i++)
		{
			char c = text2[i];
			float width = MeasureString(c.ToString(), font, format, scriptType).Width;
			if (c == '\uf06f' && font.Name == "Arial")
			{
				DocGen.Drawing.Font font2 = charFormat.Document.FontSettings.GetFont("Wingdings", font.Size, font.Style, scriptType);
				if (font2.Name == "Wingdings")
				{
					font = font2;
				}
			}
			PdfFont pdfFont = null;
			string text3 = GetEmbedFontStyle(charFormat);
			if (!string.IsNullOrEmpty(text3))
			{
				text3 = charFormat.GetFontNameToRender(scriptType) + "_" + text3;
			}
			if (m_privateFontStream != null && (m_privateFontStream.ContainsKey(text3) || m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType))))
			{
				pdfFont = CreatePdfFont(text3, charFormat.GetFontNameToRender(scriptType), font.Size, GetFontStyle(font.Style));
			}
			else if (PdfFontCollection.ContainsKey(GetPdfFontCollectionKey(font, IsUnicode(c.ToString()))))
			{
				pdfFont = PdfFontCollection[GetPdfFontCollectionKey(font, IsUnicode(c.ToString()))];
			}
			else
			{
				pdfFont = CreatePdfFont(c.ToString(), WordDocument.RenderHelper.GetFontStream(font, scriptType), font.Size, string.Empty, GetFontStyle(font.Style));
				pdfFont.Ascent = GetAscent(font, scriptType);
				PdfFontCollection.Add(GetPdfFontCollectionKey(font, IsUnicode(c.ToString())), pdfFont);
			}
			PdfBrush brush = new PdfSolidBrush(GetTextColor(charFormat));
			PdfStringFormat pdfStringFormat = PDFGraphics.ConvertFormat(format, charFormat.ComplexScript || EnableComplexScript);
			if (FallbackFonts != null && FallbackFonts.Count > 0)
			{
				pdfFont = GetFallbackPdfFont(pdfFont, font, c.ToString(), scriptType, pdfStringFormat);
			}
			if (bounds.X <= bounds.X + num4)
			{
				PDFGraphics.DrawString(c.ToString(), pdfFont, brush, new RectangleF(bounds.X + num4, bounds.Y + num3, width, bounds.Height), pdfStringFormat, directConversion: true);
				if (num2 > 0f)
				{
					PDFGraphics.DrawString(" ", pdfFont, brush, new RectangleF(bounds.X + num4 + width, bounds.Y + num3, num2, bounds.Height), pdfStringFormat, directConversion: true);
				}
			}
			else
			{
				PDFGraphics.DrawString(c.ToString(), pdfFont, brush, new RectangleF(bounds.X, bounds.Y + num3, width, bounds.Height), pdfStringFormat, directConversion: true);
			}
			num4 += width + num2;
		}
	}

	internal void DrawStringBasedOnExpandedCharSpacing(FontScriptType scriptType, DocGen.Drawing.Font font, RectangleF bounds, string text, StringFormat format, WCharacterFormat charFormat)
	{
		float num = 0f;
		for (int i = 0; i < text.Length; i++)
		{
			num += MeasureString(text[i].ToString(), font, format, scriptType).Width;
		}
		float characterSpacing = (bounds.Width - num) / (float)text.Length;
		float num2 = 0f;
		string fontNameToRender = charFormat.GetFontNameToRender(scriptType);
		if (!IsListCharacter && (fontNameToRender == "DIN Offc" || fontNameToRender == "DIN OT") && currParagraph != null && currParagraph.IsContainDinOffcFont())
		{
			num2 = bounds.Height * 0.2f;
		}
		PdfFont pdfFont = null;
		string text2 = GetEmbedFontStyle(charFormat);
		if (!string.IsNullOrEmpty(text2))
		{
			text2 = charFormat.GetFontNameToRender(scriptType) + "_" + text2;
		}
		if (m_privateFontStream != null && ((!string.IsNullOrEmpty(text2) && m_privateFontStream.ContainsKey(text2)) || (!string.IsNullOrEmpty(charFormat.GetFontNameToRender(scriptType)) && m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType)))))
		{
			pdfFont = CreatePdfFont(text2, charFormat.GetFontNameToRender(scriptType), font.Size, GetFontStyle(font.Style));
		}
		else if (PdfFontCollection.ContainsKey(GetPdfFontCollectionKey(font, IsUnicode(text))))
		{
			pdfFont = PdfFontCollection[GetPdfFontCollectionKey(font, IsUnicode(text))];
		}
		else
		{
			pdfFont = CreatePdfFont(text, WordDocument.RenderHelper.GetFontStream(font, scriptType), font.Size, string.Empty, GetFontStyle(font.Style));
			pdfFont.Ascent = GetAscent(font, scriptType);
			PdfFontCollection.Add(GetPdfFontCollectionKey(font, IsUnicode(text)), pdfFont);
		}
		PdfBrush brush = new PdfSolidBrush(GetTextColor(charFormat));
		PdfStringFormat pdfStringFormat = PDFGraphics.ConvertFormat(format, charFormat.ComplexScript || EnableComplexScript);
		pdfStringFormat.CharacterSpacing = characterSpacing;
		if (FallbackFonts != null && FallbackFonts.Count > 0)
		{
			pdfFont = GetFallbackPdfFont(pdfFont, font, text, scriptType, pdfStringFormat);
		}
		PDFGraphics.DrawString(text, pdfFont, brush, new RectangleF(bounds.X, bounds.Y + num2, bounds.Width, bounds.Height), pdfStringFormat, directConversion: true);
	}

	private void TransformGraphicsPosition(LayoutedWidget ltWidget, bool isNeedToScale, ref bool isRotateTransformApplied, ref PointF translatePoints, ref float rotationAngle, WParagraph ownerParagraph)
	{
		Entity ownerEntity = ownerParagraph.GetOwnerEntity();
		if (ownerParagraph.IsInCell)
		{
			WTableCell wTableCell = ownerParagraph.GetOwnerEntity() as WTableCell;
			LayoutedWidget ownerCellLayoutedWidget = GetOwnerCellLayoutedWidget(ltWidget);
			if (ownerCellLayoutedWidget == null)
			{
				return;
			}
			RectangleF bounds = ownerCellLayoutedWidget.Owner.Bounds;
			if (wTableCell.CellFormat.TextDirection == DocGen.DocIO.DLS.TextDirection.VerticalTopToBottom)
			{
				translatePoints = new PointF(bounds.X + bounds.Y + bounds.Width, bounds.Y - bounds.X);
				rotationAngle = 90f;
				if (!isNeedToScale)
				{
					isRotateTransformApplied = true;
					PDFGraphics.TranslateTransform(translatePoints.X, translatePoints.Y);
					PDFGraphics.RotateTransform(rotationAngle);
				}
			}
			else
			{
				translatePoints = new PointF(bounds.X - bounds.Y, bounds.X + bounds.Y + bounds.Height);
				rotationAngle = 270f;
				if (!isNeedToScale)
				{
					isRotateTransformApplied = true;
					PDFGraphics.TranslateTransform(translatePoints.X, translatePoints.Y);
					PDFGraphics.RotateTransform(rotationAngle);
				}
			}
		}
		else if (ownerEntity is WTextBox)
		{
			WTextBox wTextBox = ownerEntity as WTextBox;
			float layoutedTextBoxContentHeight = GetLayoutedTextBoxContentHeight(ltWidget);
			UpdateXYPosition(out var xPosition, out var yPosition, ownerEntity, ltWidget);
			float widthToShiftVerticalText = GetWidthToShiftVerticalText(wTextBox.TextBoxFormat.TextVerticalAlignment, layoutedTextBoxContentHeight, wTextBox.TextLayoutingBounds.Height);
			if (wTextBox.TextBoxFormat.TextDirection == DocGen.DocIO.DLS.TextDirection.VerticalTopToBottom || wTextBox.TextBoxFormat.TextDirection == DocGen.DocIO.DLS.TextDirection.VerticalFarEast)
			{
				translatePoints = new PointF(xPosition + yPosition + wTextBox.TextLayoutingBounds.Height - widthToShiftVerticalText, yPosition - xPosition);
				rotationAngle = 90f;
				if (!isNeedToScale)
				{
					isRotateTransformApplied = true;
					PDFGraphics.TranslateTransform(xPosition + yPosition + wTextBox.TextLayoutingBounds.Height - widthToShiftVerticalText, yPosition - xPosition);
					PDFGraphics.RotateTransform(90f);
				}
			}
			else
			{
				translatePoints = new PointF(xPosition - yPosition + widthToShiftVerticalText, xPosition + yPosition + wTextBox.TextLayoutingBounds.Width);
				rotationAngle = 270f;
				if (!isNeedToScale)
				{
					isRotateTransformApplied = true;
					PDFGraphics.TranslateTransform(xPosition - yPosition + widthToShiftVerticalText, xPosition + yPosition + wTextBox.TextLayoutingBounds.Width);
					PDFGraphics.RotateTransform(270f);
				}
			}
		}
		else if (ownerEntity is Shape)
		{
			Shape shape = ownerEntity as Shape;
			UpdateXYPosition(out var xPosition2, out var yPosition2, ownerEntity, ltWidget);
			if (shape.TextFrame.TextDirection == DocGen.DocIO.DLS.TextDirection.VerticalTopToBottom || shape.TextFrame.TextDirection == DocGen.DocIO.DLS.TextDirection.VerticalFarEast)
			{
				translatePoints = new PointF(xPosition2 + yPosition2 + shape.TextLayoutingBounds.Height, yPosition2 - xPosition2);
				rotationAngle = 90f;
				if (!isNeedToScale)
				{
					isRotateTransformApplied = true;
					PDFGraphics.TranslateTransform(xPosition2 + yPosition2 + shape.TextLayoutingBounds.Height, yPosition2 - xPosition2);
					PDFGraphics.RotateTransform(90f);
				}
			}
			else
			{
				translatePoints = new PointF(xPosition2 - yPosition2, shape.TextLayoutingBounds.Width + xPosition2 + yPosition2);
				rotationAngle = 270f;
				if (!isNeedToScale)
				{
					isRotateTransformApplied = true;
					PDFGraphics.TranslateTransform(xPosition2 - yPosition2, shape.TextLayoutingBounds.Width + xPosition2 + yPosition2);
					PDFGraphics.RotateTransform(270f);
				}
			}
		}
		else
		{
			if (!(ownerParagraph.Owner.Owner is ChildShape))
			{
				return;
			}
			ChildShape childShape = ownerParagraph.Owner.Owner as ChildShape;
			UpdateXYPosition(out var xPosition3, out var yPosition3, ownerEntity, ltWidget);
			if (childShape.TextFrame.TextDirection == DocGen.DocIO.DLS.TextDirection.VerticalTopToBottom || childShape.TextFrame.TextDirection == DocGen.DocIO.DLS.TextDirection.VerticalFarEast)
			{
				translatePoints = new PointF(xPosition3 + yPosition3 + childShape.TextLayoutingBounds.Height, yPosition3 - xPosition3);
				rotationAngle = 90f;
				if (!isNeedToScale)
				{
					isRotateTransformApplied = true;
					PDFGraphics.TranslateTransform(xPosition3 + yPosition3 + childShape.TextLayoutingBounds.Height, yPosition3 - xPosition3);
					PDFGraphics.RotateTransform(90f);
				}
			}
			else
			{
				translatePoints = new PointF(xPosition3 - yPosition3, childShape.TextLayoutingBounds.Width + xPosition3 + yPosition3);
				rotationAngle = 270f;
				if (!isNeedToScale)
				{
					isRotateTransformApplied = true;
					PDFGraphics.TranslateTransform(xPosition3 - yPosition3, childShape.TextLayoutingBounds.Width + xPosition3 + yPosition3);
					PDFGraphics.RotateTransform(270f);
				}
			}
		}
	}

	private void UpdateXYPosition(out float xPosition, out float yPosition, Entity entity, LayoutedWidget ltWidget)
	{
		xPosition = 0f;
		yPosition = 0f;
		if (entity is WTextBox)
		{
			xPosition = (entity as WTextBox).TextLayoutingBounds.X;
			yPosition = (entity as WTextBox).TextLayoutingBounds.Y;
		}
		else if (entity is Shape)
		{
			xPosition = (entity as Shape).TextLayoutingBounds.X;
			yPosition = (entity as Shape).TextLayoutingBounds.Y;
		}
		else if (entity is ChildShape)
		{
			xPosition = (entity as ChildShape).TextLayoutingBounds.X;
			yPosition = (entity as ChildShape).TextLayoutingBounds.Y;
		}
		if (entity.Document.Sections.Count > 1 && ((entity is WTextBox && (entity as WTextBox).GetVerticalOrigin() == VerticalOrigin.Paragraph) || (entity is Shape && (entity as Shape).GetVerticalOrigin() == VerticalOrigin.Paragraph) || (entity is ChildShape && entity.Owner is GroupShape && (entity.Owner as GroupShape).GetVerticalOrigin() == VerticalOrigin.Paragraph)) && entity.GetOwnerTextBody(entity).EntityType == EntityType.HeaderFooter && entity.Document.HasDifferentPageSetup())
		{
			RectangleF ownerTextBodyBounds = ltWidget.GetOwnerTextBodyBounds();
			xPosition = ownerTextBodyBounds.X;
			yPosition = ownerTextBodyBounds.Y;
		}
	}

	private LayoutedWidget GetOwnerCellLayoutedWidget(LayoutedWidget ltWidget)
	{
		LayoutedWidget result = null;
		while (ltWidget != null)
		{
			ltWidget = ltWidget.Owner;
			if (ltWidget != null && ((ltWidget.Widget is SplitWidgetContainer && (ltWidget.Widget as SplitWidgetContainer).RealWidgetContainer is WTableCell) || ltWidget.Widget is WTableCell))
			{
				break;
			}
		}
		if (ltWidget != null && ((ltWidget.Widget is SplitWidgetContainer && (ltWidget.Widget as SplitWidgetContainer).RealWidgetContainer is WTableCell) || ltWidget.Widget is WTableCell))
		{
			result = ltWidget;
		}
		return result;
	}

	private float GetLayoutedTextBoxContentHeight(LayoutedWidget ltWidget)
	{
		while (ltWidget != null && !(ltWidget.Widget is WTextBody))
		{
			ltWidget = ltWidget.Owner;
		}
		if (ltWidget != null && ltWidget.Owner != null && ltWidget.Owner.Widget is WTextBox)
		{
			WTextBox wTextBox = ltWidget.Owner.Widget as WTextBox;
			return ltWidget.Bounds.Height + wTextBox.TextBoxFormat.InternalMargin.Top + wTextBox.TextBoxFormat.InternalMargin.Bottom - wTextBox.TextBoxFormat.LineWidth / 2f;
		}
		if (ltWidget != null && ltWidget.Owner != null && ltWidget.Owner.Widget is ChildShape)
		{
			return (ltWidget.Owner.Widget as ChildShape).Height;
		}
		return 0f;
	}

	private float GetWidthToShiftVerticalText(DocGen.DocIO.DLS.VerticalAlignment verticalAlignment, float cellLayoutedHeight, float cellHeight)
	{
		float num = 0f;
		switch ((byte)verticalAlignment)
		{
		case 1:
			num = (cellHeight - cellLayoutedHeight) / 2f;
			break;
		case 2:
			num = cellHeight - cellLayoutedHeight;
			break;
		}
		if (num < 0f)
		{
			num = 0f;
		}
		return num;
	}

	internal RectangleF GetClipBounds(RectangleF bounds, float clipWidth, float clipTop)
	{
		float num = bounds.X;
		float num2 = bounds.Y + clipTop;
		if ((double)num % 0.75 != 0.0 && Math.Round((double)num % 0.75, 2) > 0.02)
		{
			num = ((!((double)(float)((double)num - Math.Round((double)num % 0.75, 2)) % 0.75 < 0.03)) ? ((float)Math.Round((double)num - Math.Round((double)num % 0.75, 2) - 0.75, 2)) : ((float)Math.Round((double)num - Math.Round((double)num % 0.75, 2), 2)));
		}
		if ((double)num2 % 0.75 != 0.0 && Math.Round((double)num2 % 0.75, 2) > 0.02)
		{
			num2 = ((!((double)(float)((double)num2 - Math.Round((double)num2 % 0.75, 2)) % 0.75 < 0.03)) ? ((float)Math.Round((double)num2 - Math.Round((double)num2 % 0.75, 2) - 0.75, 2)) : ((float)Math.Round((double)num2 - Math.Round((double)num2 % 0.75, 2), 2)));
		}
		clipWidth += bounds.X - num;
		float height = bounds.Height + bounds.Y - num2;
		return new RectangleF(num, num2, clipWidth, height);
	}

	private RectangleF UpdateClipBounds(RectangleF clipBounds, RectangleF ownerClipBounds)
	{
		if (ownerClipBounds.X > clipBounds.X)
		{
			clipBounds.X = ownerClipBounds.X;
		}
		if (ownerClipBounds.Y > clipBounds.Y)
		{
			clipBounds.Y = ownerClipBounds.Y;
		}
		if (ownerClipBounds.Y + ownerClipBounds.Width < clipBounds.Y + clipBounds.Width)
		{
			clipBounds.Width = ownerClipBounds.Y + ownerClipBounds.Width - clipBounds.Y;
		}
		if (ownerClipBounds.X + ownerClipBounds.Height < clipBounds.X + clipBounds.Height)
		{
			clipBounds.Height = ownerClipBounds.X + ownerClipBounds.Height - clipBounds.X;
		}
		clipBounds.Width = ((clipBounds.Width < 0f) ? 0f : clipBounds.Width);
		clipBounds.Height = ((clipBounds.Height < 0f) ? 0f : clipBounds.Height);
		return clipBounds;
	}

	internal RectangleF UpdateClipBoundsBasedOnOwner(RectangleF clipBounds, RectangleF ownerClipBounds)
	{
		if (ownerClipBounds.X > clipBounds.X)
		{
			clipBounds.X = ownerClipBounds.X;
		}
		if (ownerClipBounds.Right < clipBounds.Right)
		{
			clipBounds.Width = ownerClipBounds.Right - clipBounds.X;
		}
		if (ownerClipBounds.Y > clipBounds.Y)
		{
			clipBounds.Height -= ownerClipBounds.Y - clipBounds.Y;
			clipBounds.Y = ownerClipBounds.Y;
		}
		if (ownerClipBounds.Bottom < clipBounds.Bottom)
		{
			clipBounds.Height = ownerClipBounds.Bottom - clipBounds.Y;
		}
		clipBounds.Width = ((clipBounds.Width < 0f) ? 0f : clipBounds.Width);
		clipBounds.Height = ((clipBounds.Height < 0f) ? 0f : clipBounds.Height);
		return clipBounds;
	}

	private float GetCellHeightForVerticalText(Entity ent)
	{
		float result = 0f;
		if (ent is ParagraphItem)
		{
			WTableCell wTableCell = (ent as ParagraphItem).GetOwnerParagraphValue().OwnerTextBody as WTableCell;
			float num = 0f;
			if (wTableCell != null)
			{
				if (wTableCell.OwnerRow != null && wTableCell.OwnerRow.OwnerTable != null && wTableCell.OwnerRow.OwnerTable.TableFormat != null && wTableCell.OwnerRow.OwnerTable.TableFormat.CellSpacing > 0f)
				{
					num = wTableCell.OwnerRow.OwnerTable.TableFormat.CellSpacing * 2f;
				}
				result = wTableCell.Width;
				if (((IWidget)wTableCell).LayoutInfo is CellLayoutInfo cellLayoutInfo)
				{
					float num2 = cellLayoutInfo.Paddings.Left + cellLayoutInfo.Margins.Left - num;
					float num3 = cellLayoutInfo.Paddings.Right + cellLayoutInfo.Margins.Right - num;
					result = result - num2 - num3;
				}
				return result;
			}
		}
		return result;
	}

	private void DrawRTLText(FontScriptType scriptType, CharacterRangeType characterRangeType, string text, WCharacterFormat charFormat, DocGen.Drawing.Font font, DocGen.Drawing.SkiaSharpHelper.Brush textBrush, RectangleF bounds, StringFormat format)
	{
		if (charFormat.CharacterSpacing != 0f)
		{
			if (charFormat.SmallCaps)
			{
				DrawSmallCapString(scriptType, characterRangeType, text, charFormat, bounds, font, format, textBrush, isCharacterSpacing: true);
				return;
			}
			DocGen.Drawing.Font font2 = ((charFormat.SubSuperScript != 0) ? charFormat.Document.FontSettings.GetFont(font.Name, GetSubSuperScriptFontSize(font), font.Style, scriptType) : font);
			DrawStringBasedOnCharSpacing(scriptType, characterRangeType, font2, textBrush, bounds, text, format, charFormat);
			return;
		}
		if (!charFormat.Bidi)
		{
			font = GetDefaultFont(scriptType, font, charFormat);
			string fontNameToRender = charFormat.GetFontNameToRender(scriptType);
			if (!IsListCharacter && (fontNameToRender == "DIN Offc" || fontNameToRender == "DIN OT") && currParagraph != null && currParagraph.IsContainDinOffcFont())
			{
				_ = bounds.Height;
			}
			DocGen.Drawing.Font font3 = font;
			if (font3.Name == "Arial" && IsInvalidCharacter(text))
			{
				font3 = charFormat.Document.FontSettings.GetFont("Arial Unicode MS", font3.Size, font3.Style, scriptType);
			}
			if (charFormat.SmallCaps)
			{
				DrawSmallCapString(scriptType, characterRangeType, text, charFormat, bounds, font3, format, textBrush, isCharacterSpacing: false);
				return;
			}
			DocGen.Drawing.Font font4 = ((charFormat.SubSuperScript != 0) ? charFormat.Document.FontSettings.GetFont(font3.Name, GetSubSuperScriptFontSize(font3), font3.Style, scriptType) : font3);
			if (font4.Name == "Arial Narrow" && font4.Style == FontStyle.Bold)
			{
				text = text.Replace(ControlChar.NonBreakingSpaceChar, ControlChar.SpaceChar);
			}
			if (!charFormat.GetFontNameToRender(scriptType).Equals(font4.Name, StringComparison.OrdinalIgnoreCase) && m_privateFontStream != null && m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType)))
			{
				font4 = GetPrivateFont(charFormat.GetFontNameToRender(scriptType), font4.Size, font4.Style);
			}
			PdfFont pdfFont = null;
			string text2 = GetEmbedFontStyle(charFormat);
			if (!string.IsNullOrEmpty(text2))
			{
				text2 = charFormat.GetFontNameToRender(scriptType) + "_" + text2;
			}
			if (m_privateFontStream != null && ((!string.IsNullOrEmpty(text2) && m_privateFontStream.ContainsKey(text2)) || (!string.IsNullOrEmpty(charFormat.GetFontNameToRender(scriptType)) && m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType)))))
			{
				pdfFont = CreatePdfFont(text2, charFormat.GetFontNameToRender(scriptType), font4.Size, GetFontStyle(font4.Style));
			}
			else if (PdfFontCollection.ContainsKey(font4.Name.ToLower() + font4.Style.ToString() + font4.Size + "unicode"))
			{
				pdfFont = PdfFontCollection[font4.Name.ToLower() + font4.Style.ToString() + font4.Size + "unicode"];
			}
			else
			{
				pdfFont = CreatePdfFont(text, WordDocument.RenderHelper.GetFontStream(font4, scriptType), font4.Size, GetFontStyle(font4.Style));
				PdfFontCollection.Add(font4.Name.ToLower() + font4.Style.ToString() + font4.Size + "unicode", pdfFont);
			}
			PdfBrush brush = new PdfSolidBrush(GetTextColor(charFormat));
			PdfStringFormat pdfStringFormat = PDFGraphics.ConvertFormat(format, charFormat.ComplexScript || EnableComplexScript);
			if (FallbackFonts != null && FallbackFonts.Count > 0)
			{
				pdfFont = GetFallbackPdfFont(pdfFont, font4, text, scriptType, pdfStringFormat);
			}
			string[] array = text.Split(' ');
			if (array.Length != 0)
			{
				float width = MeasureString(" ", font, format, charFormat, isMeasureFromTabList: false, isMeasureFromSmallCapString: true, scriptType).Width;
				float num = 0f;
				for (int num2 = array.Length - 1; num2 >= 0; num2--)
				{
					string text3 = array[num2];
					num = MeasureString(array[num2], font, format, charFormat, isMeasureFromTabList: false, isMeasureFromSmallCapString: true, scriptType).Width;
					if (num2 != 0)
					{
						text3 = " " + text3;
						num += width;
					}
					PDFDrawString(text3, pdfFont, brush, bounds, pdfStringFormat, charFormat, scriptType);
					bounds.X += num;
				}
			}
			else
			{
				PDFDrawString(text, pdfFont, brush, bounds, pdfStringFormat, charFormat, scriptType);
			}
			return;
		}
		string[] array2 = text.Split(' ');
		if (array2.Length != 0)
		{
			float width2 = MeasureString(" ", font, format, charFormat, isMeasureFromTabList: false, isMeasureFromSmallCapString: true, scriptType).Width;
			float num3 = 0f;
			for (int num4 = array2.Length - 1; num4 >= 0; num4--)
			{
				string text4 = array2[num4];
				num3 = MeasureString(text4, font, format, charFormat, isMeasureFromTabList: false, isMeasureFromSmallCapString: true, scriptType).Width;
				if (num4 != 0)
				{
					text4 = " " + text4;
					num3 += width2;
				}
				DrawUnicodeString(scriptType, characterRangeType, text4, charFormat, font, textBrush, new RectangleF(bounds.X, bounds.Y, num3, bounds.Height), format);
				bounds.X += num3;
			}
		}
		else
		{
			DrawUnicodeString(scriptType, characterRangeType, text, charFormat, font, textBrush, bounds, format);
		}
	}

	private void DrawChineseText(FontScriptType scriptType, CharacterRangeType characterRangeType, string text, WCharacterFormat charFormat, DocGen.Drawing.Font font, DocGen.Drawing.SkiaSharpHelper.Brush textBrush, RectangleF bounds, StringFormat format)
	{
		text = text.Replace('\u200d'.ToString(), "");
		if (text.Length == 0)
		{
			return;
		}
		if (charFormat.CharacterSpacing != 0f)
		{
			if (charFormat.SmallCaps)
			{
				DrawSmallCapString(scriptType, characterRangeType, text, charFormat, bounds, font, format, textBrush, isCharacterSpacing: true);
				return;
			}
			DocGen.Drawing.Font font2 = ((charFormat.SubSuperScript != 0) ? charFormat.Document.FontSettings.GetFont(font.Name, GetSubSuperScriptFontSize(font), font.Style, scriptType) : font);
			DrawStringBasedOnCharSpacing(scriptType, characterRangeType, font2, textBrush, bounds, text, format, charFormat);
			return;
		}
		char[] array = text.ToCharArray();
		string text2 = null;
		string text3 = null;
		DocGen.Drawing.Font font3 = font;
		if (font3.Name == "Arial" || font3.Name == "Times New Roman" || font3.Name == "Trebuchet MS")
		{
			font3 = charFormat.Document.FontSettings.GetFont("Arial Unicode MS", font3.Size, font3.Style, scriptType);
		}
		float num = 0f;
		string fontNameToRender = charFormat.GetFontNameToRender(scriptType);
		if (!IsListCharacter && (fontNameToRender == "DIN Offc" || fontNameToRender == "DIN OT") && currParagraph != null && currParagraph.IsContainDinOffcFont())
		{
			num = bounds.Height * 0.2f;
		}
		if (!charFormat.ComplexScript)
		{
			font = GetDefaultFont(scriptType, font, charFormat);
		}
		float num2 = 0f;
		float ascent = GetAscent(font, scriptType);
		float ascent2 = GetAscent(font3, scriptType);
		float num3 = 0f;
		float num4 = 0f;
		if (font.Name != font3.Name && IsUnicodeText(text))
		{
			if (ascent2 > ascent)
			{
				num3 = ascent2 - ascent;
			}
			if (ascent > ascent2)
			{
				num4 = ascent - ascent2;
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (!IsUnicodeText(array[i].ToString()))
				{
					text2 += array[i];
					if (text3 == null)
					{
						continue;
					}
					num2 = MeasureString(text3, font3, format, charFormat, isMeasureFromTabList: false, isMeasureFromSmallCapString: true, scriptType).Width;
					if (charFormat != null && charFormat.SmallCaps)
					{
						DrawSmallCapString(scriptType, characterRangeType, text, charFormat, new RectangleF(new PointF(bounds.X, bounds.Y - num4), bounds.Size), font, format, textBrush, isCharacterSpacing: false);
					}
					else
					{
						DocGen.Drawing.Font font4 = ((charFormat != null && charFormat.SubSuperScript != 0) ? charFormat.Document.FontSettings.GetFont(font3.Name, GetSubSuperScriptFontSize(font3), font3.Style, scriptType) : font3);
						if (!charFormat.GetFontNameToRender(scriptType).Equals(font4.Name, StringComparison.OrdinalIgnoreCase) && m_privateFontStream != null && m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType)))
						{
							font4 = GetPrivateFont(charFormat.GetFontNameToRender(scriptType), font4.Size, font4.Style);
						}
						PdfFont pdfFont = null;
						string text4 = GetEmbedFontStyle(charFormat);
						if (!string.IsNullOrEmpty(text4))
						{
							text4 = charFormat.GetFontNameToRender(scriptType) + "_" + text4;
						}
						if (m_privateFontStream != null && (m_privateFontStream.ContainsKey(text4) || m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType))))
						{
							pdfFont = CreatePdfFont(text4, charFormat.GetFontNameToRender(scriptType), font4.Size, GetFontStyle(font4.Style));
						}
						else if (PdfFontCollection.ContainsKey(GetPdfFontCollectionKey(font4, IsUnicode(text3))))
						{
							pdfFont = PdfFontCollection[GetPdfFontCollectionKey(font4, IsUnicode(text3))];
						}
						else
						{
							pdfFont = CreatePdfFont(text3, WordDocument.RenderHelper.GetFontStream(font4, scriptType), font4.Size, GetFontStyle(font4.Style));
							pdfFont.Ascent = GetAscent(font4, scriptType);
							PdfFontCollection.Add(GetPdfFontCollectionKey(font4, IsUnicode(text3)), pdfFont);
						}
						PdfBrush brush = new PdfSolidBrush(GetTextColor(charFormat));
						PdfStringFormat pdfStringFormat = PDFGraphics.ConvertFormat(format, charFormat.ComplexScript || EnableComplexScript);
						if (font4.Style != 0 && scriptType != 0 && !IsContainFont(pdfFont, text, pdfStringFormat))
						{
							pdfFont = GetRegularStyleFontToRender(font4, scriptType);
						}
						if (FallbackFonts != null && FallbackFonts.Count > 0)
						{
							pdfFont = GetFallbackPdfFont(pdfFont, font4, text3, scriptType, pdfStringFormat);
						}
						PDFDrawString(text3, pdfFont, brush, bounds.X, bounds.Y - num4 + num, pdfStringFormat, charFormat, scriptType);
					}
					bounds.X += num2;
					text3 = null;
				}
				else
				{
					text3 += array[i];
					if (text2 != null)
					{
						num2 = MeasureString(text2, font, format, charFormat, isMeasureFromTabList: false, isMeasureFromSmallCapString: true, scriptType).Width;
						DrawUnicodeText(scriptType, characterRangeType, text2, charFormat, font, textBrush, new RectangleF(bounds.X, bounds.Y + num3, bounds.Width, bounds.Height), format);
						bounds.X += num2;
						text2 = null;
					}
				}
			}
			if (text3 != null)
			{
				if (charFormat != null && charFormat.SmallCaps)
				{
					DrawSmallCapString(scriptType, characterRangeType, text, charFormat, new RectangleF(new PointF(bounds.X, bounds.Y - num4), bounds.Size), font, format, textBrush, isCharacterSpacing: false);
					return;
				}
				DocGen.Drawing.Font font5 = ((charFormat != null && charFormat.SubSuperScript != 0) ? charFormat.Document.FontSettings.GetFont(font3.Name, GetSubSuperScriptFontSize(font3), font3.Style, scriptType) : font3);
				PdfFont pdfFont2 = null;
				string text5 = GetEmbedFontStyle(charFormat);
				if (!string.IsNullOrEmpty(text5))
				{
					text5 = charFormat.GetFontNameToRender(scriptType) + "_" + text5;
				}
				if (m_privateFontStream != null && (m_privateFontStream.ContainsKey(text5) || m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType))))
				{
					pdfFont2 = CreatePdfFont(text5, charFormat.GetFontNameToRender(scriptType), font5.Size, GetFontStyle(font5.Style));
				}
				else if (PdfFontCollection.ContainsKey(GetPdfFontCollectionKey(font5, IsUnicode(text3))))
				{
					pdfFont2 = PdfFontCollection[GetPdfFontCollectionKey(font5, IsUnicode(text3))];
				}
				else
				{
					pdfFont2 = CreatePdfFont(text3, WordDocument.RenderHelper.GetFontStream(font5, scriptType), font5.Size, GetFontStyle(font5.Style));
					pdfFont2.Ascent = GetAscent(font5, scriptType);
					PdfFontCollection.Add(GetPdfFontCollectionKey(font5, IsUnicode(text3)), pdfFont2);
				}
				PdfBrush brush2 = new PdfSolidBrush(GetTextColor(charFormat));
				PdfStringFormat pdfStringFormat2 = PDFGraphics.ConvertFormat(format, charFormat.ComplexScript || EnableComplexScript);
				if (font5.Style != 0 && scriptType != 0 && !IsContainFont(pdfFont2, text, pdfStringFormat2))
				{
					pdfFont2 = GetRegularStyleFontToRender(font5, scriptType);
				}
				if (FallbackFonts != null && FallbackFonts.Count > 0)
				{
					pdfFont2 = GetFallbackPdfFont(pdfFont2, font5, text3, scriptType, pdfStringFormat2);
				}
				PDFDrawString(text3, pdfFont2, brush2, bounds.X, bounds.Y - num4, pdfStringFormat2, charFormat, scriptType);
			}
			else if (text2 != null)
			{
				DrawUnicodeText(scriptType, characterRangeType, text2, charFormat, font, textBrush, new RectangleF(bounds.X, bounds.Y + num3, bounds.Width, bounds.Height), format);
			}
		}
		else
		{
			DrawUnicodeText(scriptType, characterRangeType, text, charFormat, font, textBrush, bounds, format);
		}
	}

	private void PDFDrawString(string text, PdfFont font, PdfBrush brush, float x, float y, PdfStringFormat stringFormat, WCharacterFormat charFormat, FontScriptType scriptType)
	{
		PDFDrawString(text, font, brush, new RectangleF(x, y, 0f, 0f), stringFormat, charFormat, scriptType);
	}

	private void PDFDrawString(string text, PdfFont font, PdfBrush brush, RectangleF bounds, PdfStringFormat stringFormat, WCharacterFormat charFormat, FontScriptType scriptType)
	{
		string text2 = ((charFormat.SymExFontName != null) ? charFormat.SymExFontName : charFormat.FontName);
		string text3 = GetEmbedFontStyle(charFormat);
		if (!string.IsNullOrEmpty(text3))
		{
			text3 = charFormat.GetFontNameToRender(scriptType) + "_" + text3;
		}
		if (FallbackFonts == null)
		{
			if (m_privateFontStream != null && !text2.Equals(font.Name, StringComparison.OrdinalIgnoreCase) && ((!string.IsNullOrEmpty(text3) && m_privateFontStream.ContainsKey(text3)) || (!string.IsNullOrEmpty(charFormat.GetFontNameToRender(scriptType)) && m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType)))))
			{
				PdfTrueTypeFont obj = font as PdfTrueTypeFont;
				obj.MeasureString(text, stringFormat);
				if (!obj.IsContainsFont)
				{
					font = UnicodeStringFontSubstitution(text, font, stringFormat, scriptType);
				}
			}
			else if (text2 != "Cambria Math" && !text2.Equals(font.Name, StringComparison.OrdinalIgnoreCase))
			{
				font = UnicodeStringFontSubstitution(text, font, stringFormat, scriptType);
			}
			else if (IsListCharacter && PdfString.IsUnicode(text))
			{
				PdfTrueTypeFont obj2 = font as PdfTrueTypeFont;
				obj2.MeasureString(text, stringFormat);
				if (!obj2.IsContainsFont)
				{
					font = UnicodeStringFontSubstitution(text, font, stringFormat, scriptType);
				}
			}
		}
		PDFGraphics.DrawString(text, font, brush, bounds, stringFormat, directConversion: true);
	}

	private PdfFont UnicodeStringFontSubstitution(string text, PdfFont font, PdfStringFormat stringFormat, FontScriptType scriptType)
	{
		if (!string.IsNullOrEmpty(text))
		{
			string unicodeFamilyName = DocGen.Drawing.SkiaSharpHelper.Extension.GetUnicodeFamilyName(text, font.Name);
			if (unicodeFamilyName != null && font.Name == unicodeFamilyName)
			{
				return font;
			}
			Stream unicodeFontStream = DocGen.Drawing.SkiaSharpHelper.Extension.GetUnicodeFontStream(text, font.Name);
			if (unicodeFontStream != null)
			{
				return CreatePdfFont(text, unicodeFontStream, font.Size, font.Style);
			}
			if (PdfString.IsUnicode(text))
			{
				PdfTrueTypeFont pdfTrueTypeFont = font as PdfTrueTypeFont;
				try
				{
					if (stringFormat != null && stringFormat.RightToLeft)
					{
						DocGen.Drawing.Font font2 = new DocGen.Drawing.Font("Times New Roman", font.Size);
						return font = CreatePdfFont(text, WordDocument.RenderHelper.GetFontStream(font2, scriptType), font2.Size, font.Style);
					}
					if (!pdfTrueTypeFont.IsContainsFont)
					{
						DocGen.Drawing.Font font3 = new DocGen.Drawing.Font("Arial Unicode MS", font.Size);
						FontExtension fontExtension = new FontExtension(font3.Name, font3.Size, font3.Style, GraphicsUnit.Point);
						if (fontExtension.Typeface.FamilyName != "Arial Unicode MS")
						{
							font3.Name = "sans-serif";
							fontExtension = new FontExtension(font3.Name, font3.Size, font3.Style, GraphicsUnit.Point);
						}
						return font = CreatePdfFont(text, fontExtension.FontStream, font3.Size, font.Style);
					}
				}
				catch
				{
					if (font.Name.ToLower().Equals("microsoft sans serif"))
					{
						DocGen.Drawing.Font font4 = new DocGen.Drawing.Font("Arial Unicode MS", font.Size);
						return font = CreatePdfFont(text, WordDocument.RenderHelper.GetFontStream(font4, scriptType), font4.Size, font.Style);
					}
					if (font.Name.ToLower().Equals("arial"))
					{
						DocGen.Drawing.Font font5 = new DocGen.Drawing.Font("Arial", font.Size);
						return font = CreatePdfFont(text, WordDocument.RenderHelper.GetFontStream(font5, scriptType), font5.Size, font.Style);
					}
				}
			}
		}
		return font;
	}

	private DocGen.Drawing.Font GetAlternateFontToMeasure(string text, DocGen.Drawing.Font font)
	{
		string unicodeFamilyName = WordDocument.RenderHelper.GetUnicodeFamilyName(text, font.Name);
		if (unicodeFamilyName != null && font.Name == unicodeFamilyName)
		{
			return font;
		}
		if (unicodeFamilyName != null && font.Name != unicodeFamilyName && font.Name != "Cambria Math")
		{
			font = new DocGen.Drawing.Font(unicodeFamilyName, font.Size, font.Style);
			return font;
		}
		if (unicodeFamilyName == null)
		{
			unicodeFamilyName = WordDocument.RenderHelper.GetUnicodeFamilyName(text, "Arial Unicode MS");
			if (unicodeFamilyName != null)
			{
				font = new DocGen.Drawing.Font(unicodeFamilyName, font.Size, font.Style);
				return font;
			}
			unicodeFamilyName = WordDocument.RenderHelper.GetUnicodeFamilyName(text, "sans-serif");
			if (unicodeFamilyName != null)
			{
				font = new DocGen.Drawing.Font(unicodeFamilyName, font.Size, font.Style);
				return font;
			}
		}
		return font;
	}

	internal DocGen.Drawing.Font GetAlternateFontToRender(string text, DocGen.Drawing.Font tempfont, WCharacterFormat charFormat)
	{
		string text2 = "";
		if (charFormat != null)
		{
			text2 = ((charFormat.SymExFontName != null) ? charFormat.SymExFontName : charFormat.FontName);
		}
		if (charFormat != null && !charFormat.Bidi && charFormat.Document != null && charFormat.Document.FontSettings.FontStreams.Count == 0 && !charFormat.SmallCaps && IsContainsUnicodeCharOnly(text))
		{
			if ((tempfont.Name == "Arial" || tempfont.Name == "Times New Roman" || tempfont.Name == "Trebuchet MS") && IsUnicodeText(text))
			{
				tempfont = new DocGen.Drawing.Font("Arial Unicode MS", tempfont.Size, tempfont.Style);
			}
			if (!text2.Equals(tempfont.Name, StringComparison.OrdinalIgnoreCase))
			{
				tempfont = GetAlternateFontToMeasure(text, tempfont);
			}
		}
		return tempfont;
	}

	private bool IsContainsUnicodeCharOnly(string text)
	{
		if (text == null)
		{
			return false;
		}
		if (Encoding.UTF8.GetByteCount(text) != text.Length)
		{
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] <= 'ÿ' && text[i] != ControlChar.SpaceChar)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	private bool IsInvalidCharacter(string text)
	{
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] != '★')
			{
				return false;
			}
		}
		return true;
	}

	private void DrawUnicodeText(FontScriptType scriptType, CharacterRangeType characterRangeType, string text, WCharacterFormat charFormat, DocGen.Drawing.Font font, DocGen.Drawing.SkiaSharpHelper.Brush textBrush, RectangleF bounds, StringFormat format)
	{
		char[] array = text.ToCharArray();
		string text2 = null;
		string text3 = null;
		DocGen.Drawing.Font font2 = font;
		float num = 0f;
		string fontNameToRender = charFormat.GetFontNameToRender(scriptType);
		if (!IsListCharacter && (fontNameToRender == "DIN Offc" || fontNameToRender == "DIN OT") && currParagraph != null && currParagraph.IsContainDinOffcFont())
		{
			_ = bounds.Height;
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] < 'ÿ')
			{
				text2 += array[i];
				if (text3 == null)
				{
					continue;
				}
				if (font2.Name == "Arial" && IsInvalidCharacter(text3))
				{
					font2 = charFormat.Document.FontSettings.GetFont("Arial Unicode MS", font2.Size, font2.Style, scriptType);
				}
				num = MeasureString(text3, font2, format, charFormat, isMeasureFromTabList: false, isMeasureFromSmallCapString: true, scriptType).Width;
				if (charFormat.SmallCaps)
				{
					DrawSmallCapString(scriptType, characterRangeType, text3, charFormat, bounds, font2, format, textBrush, isCharacterSpacing: false);
				}
				else
				{
					DocGen.Drawing.Font font3 = ((charFormat.SubSuperScript != 0) ? charFormat.Document.FontSettings.GetFont(font2.Name, GetSubSuperScriptFontSize(font2), font2.Style, scriptType) : font2);
					if (font3.Name == "Arial Narrow" && font3.Style == FontStyle.Bold)
					{
						text3 = text3.Replace(ControlChar.NonBreakingSpaceChar, ControlChar.SpaceChar);
					}
					if (!charFormat.GetFontNameToRender(scriptType).Equals(font3.Name, StringComparison.OrdinalIgnoreCase) && m_privateFontStream != null && m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType)))
					{
						font3 = GetPrivateFont(charFormat.GetFontNameToRender(scriptType), font3.Size, font3.Style);
					}
					PdfFont pdfFont = null;
					string text4 = GetEmbedFontStyle(charFormat);
					if (!string.IsNullOrEmpty(text4))
					{
						text4 = charFormat.GetFontNameToRender(scriptType) + "_" + text4;
					}
					if (m_privateFontStream != null && (m_privateFontStream.ContainsKey(text4) || m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType))))
					{
						pdfFont = CreatePdfFont(text4, charFormat.GetFontNameToRender(scriptType), font3.Size, GetFontStyle(font3.Style));
					}
					else if (PdfFontCollection.ContainsKey(GetPdfFontCollectionKey(font3, isUnicode: true)))
					{
						pdfFont = PdfFontCollection[GetPdfFontCollectionKey(font3, isUnicode: true)];
					}
					else
					{
						pdfFont = new PdfTrueTypeFont(WordDocument.RenderHelper.GetFontStream(font3, scriptType), font3.Size, isUnicode: true, "", GetFontStyle(font3.Style));
						pdfFont.Ascent = GetAscent(font3, scriptType);
						PdfFontCollection.Add(GetPdfFontCollectionKey(font3, isUnicode: true), pdfFont);
					}
					PdfBrush brush = new PdfSolidBrush(GetTextColor(charFormat));
					PdfStringFormat pdfStringFormat = PDFGraphics.ConvertFormat(format, charFormat.ComplexScript || EnableComplexScript);
					if (FallbackFonts != null && FallbackFonts.Count > 0)
					{
						pdfFont = GetFallbackPdfFont(pdfFont, font3, text3, scriptType, pdfStringFormat);
					}
					PDFDrawString(text3, pdfFont, brush, bounds.X, bounds.Y, pdfStringFormat, charFormat, scriptType);
				}
				bounds.X += num;
				text3 = null;
			}
			else
			{
				text3 += array[i];
				if (text2 != null)
				{
					num = MeasureString(text2, font, format, charFormat, isMeasureFromTabList: false, isMeasureFromSmallCapString: true, scriptType).Width;
					DrawUnicodeString(scriptType, characterRangeType, text2, charFormat, font, textBrush, new RectangleF(bounds.X, bounds.Y, num, bounds.Height), format);
					bounds.X += num;
					text2 = null;
				}
			}
		}
		if (text3 != null)
		{
			if (charFormat.SmallCaps)
			{
				DrawSmallCapString(scriptType, characterRangeType, text3, charFormat, bounds, font, format, textBrush, isCharacterSpacing: false);
				return;
			}
			DocGen.Drawing.Font font4 = ((charFormat.SubSuperScript != 0) ? charFormat.Document.FontSettings.GetFont(font.Name, GetSubSuperScriptFontSize(font), font.Style, scriptType) : font);
			if (font4.Name == "Arial Narrow" && font4.Style == FontStyle.Bold)
			{
				text3 = text3.Replace(ControlChar.NonBreakingSpaceChar, ControlChar.SpaceChar);
			}
			PdfFont pdfFont2 = null;
			string text5 = GetEmbedFontStyle(charFormat);
			if (!string.IsNullOrEmpty(text5))
			{
				text5 = charFormat.GetFontNameToRender(scriptType) + "_" + text5;
			}
			if (m_privateFontStream != null && (m_privateFontStream.ContainsKey(text5) || m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType))))
			{
				pdfFont2 = CreatePdfFont(text5, charFormat.GetFontNameToRender(scriptType), font4.Size, GetFontStyle(font4.Style));
			}
			else if (PdfFontCollection.ContainsKey(GetPdfFontCollectionKey(font4, isUnicode: true)))
			{
				pdfFont2 = PdfFontCollection[GetPdfFontCollectionKey(font4, isUnicode: true)];
			}
			else
			{
				pdfFont2 = new PdfTrueTypeFont(WordDocument.RenderHelper.GetFontStream(font4, scriptType), font4.Size, isUnicode: true, "", GetFontStyle(font4.Style));
				pdfFont2.Ascent = GetAscent(font4, scriptType);
				PdfFontCollection.Add(GetPdfFontCollectionKey(font4, isUnicode: true), pdfFont2);
			}
			PdfBrush brush2 = new PdfSolidBrush(GetTextColor(charFormat));
			PdfStringFormat pdfStringFormat2 = PDFGraphics.ConvertFormat(format, charFormat.ComplexScript || EnableComplexScript);
			if (FallbackFonts != null && FallbackFonts.Count > 0)
			{
				pdfFont2 = GetFallbackPdfFont(pdfFont2, font4, text3, scriptType, pdfStringFormat2);
			}
			PDFDrawString(text3, pdfFont2, brush2, bounds.X, bounds.Y, pdfStringFormat2, charFormat, scriptType);
		}
		else if (text2 != null)
		{
			num = MeasureString(text2, font, format, charFormat, isMeasureFromTabList: false, isMeasureFromSmallCapString: true, scriptType).Width;
			DrawUnicodeString(scriptType, characterRangeType, text2, charFormat, font, textBrush, new RectangleF(bounds.X, bounds.Y, num, bounds.Height), format);
		}
	}

	internal void DrawUnicodeString(FontScriptType scriptType, CharacterRangeType characterRangeType, string text, WCharacterFormat charFormat, DocGen.Drawing.Font font, DocGen.Drawing.SkiaSharpHelper.Brush textBrush, RectangleF bounds, StringFormat format)
	{
		float num = 0f;
		string fontNameToRender = charFormat.GetFontNameToRender(scriptType);
		if (!IsListCharacter && (fontNameToRender == "DIN Offc" || fontNameToRender == "DIN OT") && currParagraph != null && currParagraph.IsContainDinOffcFont())
		{
			num = bounds.Height * 0.2f;
		}
		if (charFormat.SmallCaps)
		{
			DrawSmallCapString(scriptType, characterRangeType, text, charFormat, bounds, font, format, textBrush, isCharacterSpacing: false);
			return;
		}
		DocGen.Drawing.Font font2 = ((charFormat.SubSuperScript != 0) ? charFormat.Document.FontSettings.GetFont(font.Name, GetSubSuperScriptFontSize(font), font.Style, scriptType) : font);
		if (font2.Name == "Arial Narrow" && font2.Style == FontStyle.Bold)
		{
			text = text.Replace(ControlChar.NonBreakingSpaceChar, ControlChar.SpaceChar);
		}
		RectangleF rect = new RectangleF(bounds.X, bounds.Y + num, bounds.Width, bounds.Height);
		PdfFont pdfFont = null;
		string text2 = GetEmbedFontStyle(charFormat);
		if (!string.IsNullOrEmpty(text2))
		{
			text2 = charFormat.GetFontNameToRender(scriptType) + "_" + text2;
		}
		if (m_privateFontStream != null && ((!string.IsNullOrEmpty(text2) && m_privateFontStream.ContainsKey(text2)) || (!string.IsNullOrEmpty(charFormat.GetFontNameToRender(scriptType)) && m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType)))))
		{
			pdfFont = CreatePdfFont(text2, charFormat.GetFontNameToRender(scriptType), font2.Size, GetFontStyle(font2.Style));
		}
		else if (PdfFontCollection.ContainsKey(GetPdfFontCollectionKey(font2, isUnicode: true)))
		{
			pdfFont = PdfFontCollection[GetPdfFontCollectionKey(font2, isUnicode: true)];
		}
		else
		{
			pdfFont = new PdfTrueTypeFont(WordDocument.RenderHelper.GetFontStream(font2, scriptType), font2.Size, isUnicode: true, "", GetFontStyle(font2.Style));
			pdfFont.Ascent = GetAscent(font2, scriptType);
			PdfFontCollection.Add(GetPdfFontCollectionKey(font2, isUnicode: true), pdfFont);
		}
		PdfBrush brush = new PdfSolidBrush(GetTextColor(charFormat));
		PdfStringFormat pdfStringFormat = PDFGraphics.ConvertFormat(format, charFormat.ComplexScript || EnableComplexScript);
		if (font2.Style != 0 && scriptType != 0 && !IsContainFont(pdfFont, text, pdfStringFormat))
		{
			pdfFont = GetRegularStyleFontToRender(font2, scriptType);
		}
		if (FallbackFonts != null && FallbackFonts.Count > 0)
		{
			pdfFont = GetFallbackPdfFont(pdfFont, font2, text, scriptType, pdfStringFormat);
		}
		PDFGraphics.DrawString(text, pdfFont, brush, rect, pdfStringFormat, directConversion: true);
	}

	internal bool IsOwnerParagraphEmpty(string text)
	{
		bool result = false;
		if (text == " " && currParagraph != null && currParagraph.Text == "" && currParagraph.Items.Count == 0)
		{
			result = true;
		}
		return result;
	}

	internal void DrawJustifiedLine(WTextRange txtRange, string text, WCharacterFormat charFormat, WParagraphFormat paraFormat, RectangleF bounds, LayoutedWidget ltWidget)
	{
		if (text == null)
		{
			return;
		}
		FontScriptType scriptType = txtRange.ScriptType;
		if (ltWidget.IsContainsSpaceCharAtEnd)
		{
			text = text.TrimEnd(ControlChar.SpaceChar);
		}
		_ = txtRange.CharacterRange;
		char c = '\u001e';
		if (text.Contains(c.ToString()))
		{
			text = text.Replace(c.ToString(), "-");
		}
		float num = 0f;
		string[] array = text.Split(' ');
		bool flag = HasUnderlineOrStricthrough(txtRange, charFormat, scriptType);
		DocGen.Drawing.Font font = ((currTextRange != null && currTextRange.m_layoutInfo != null) ? currTextRange.m_layoutInfo.Font.GetFont(currTextRange.Document, currTextRange.ScriptType) : GetFont(scriptType, charFormat, text));
		StringFormat stringFormat = new StringFormat(StringFormt);
		if (charFormat.Bidi)
		{
			stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
		}
		else
		{
			stringFormat.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;
		}
		Color textColor = GetTextColor(charFormat);
		DocGen.Drawing.SkiaSharpHelper.Brush brush = GetBrush(textColor);
		DocGen.Drawing.Font font2 = GetDefaultFont(scriptType, font, charFormat);
		if (font.Name != font2.Name && !IsUnicodeText(text))
		{
			font2 = font;
		}
		if (IsSoftHyphen(ltWidget))
		{
			text = "-";
			array[0] = text;
			if (bounds.Width == 0f)
			{
				bounds.Width = ltWidget.Bounds.Width;
			}
		}
		bool flag2 = font.Name != font2.Name && IsUnicodeText(text);
		if (!charFormat.TextBackgroundColor.IsEmpty)
		{
			PDFGraphics.DrawRectangle(new PdfSolidBrush(charFormat.TextBackgroundColor), bounds);
		}
		if (ltWidget.Owner.ChildWidgets[0] == ltWidget)
		{
			array = text.TrimStart().Split(' ');
			SizeF sizeF = MeasureString(text, font, font2, stringFormat, charFormat, scriptType);
			SizeF sizeF2 = MeasureString(text.TrimStart(), font, font2, stringFormat, charFormat, scriptType);
			if (sizeF != sizeF2)
			{
				num = sizeF.Width - sizeF2.Width;
				bounds.X += num;
				text = text.TrimStart();
			}
		}
		if (!charFormat.HighlightColor.IsEmpty)
		{
			Color hightLightColor = GetHightLightColor(charFormat.HighlightColor);
			new DocGen.Drawing.SkiaSharpHelper.Brush(hightLightColor);
			SizeF sizeF3 = default(SizeF);
			sizeF3 = ((!flag2) ? MeasureString(text, font2, stringFormat, charFormat, isMeasureFromTabList: false, isMeasureFromSmallCapString: true, scriptType) : MeasureString(text, font, font2, stringFormat, charFormat, scriptType));
			PDFGraphics.DrawRectangle(new PdfSolidBrush(hightLightColor), bounds.X, bounds.Y, bounds.Width, sizeF3.Height);
		}
		ResetTransform();
		float clipWidth = ltWidget.Bounds.Width;
		float clipTopPosition = GetClipTopPosition(bounds, isInlinePicture: false);
		float scaling = charFormat.Scaling;
		bool flag3 = scaling != 100f && (scaling >= 1f || scaling <= 600f);
		PointF translatePoints = PointF.Empty;
		float rotationAngle = 0f;
		float ang = 0f;
		bool flipH = false;
		bool flipV = false;
		bool doNotRotateText = false;
		bool hasCommentsHighlighter = false;
		bool isRotateTransformApplied = false;
		TextWrappingStyle textWrappingStyle = TextWrappingStyle.Square;
		GetTextboxOrShapesRotationValue(ref ang, ref flipH, ref flipV, ref doNotRotateText, ref textWrappingStyle, ref hasCommentsHighlighter, currTextRange);
		RectangleF bounds2 = ((num != 0f) ? new RectangleF(bounds.X - num, bounds.Y, bounds.Width, bounds.Height) : bounds);
		if (clipWidth == 0f || IsWidgetNeedToClipBasedOnXPosition(ltWidget, ref clipWidth, bounds2))
		{
			ResetTransform();
			return;
		}
		RectangleF clipBounds = GetClipBounds(bounds, clipWidth, clipTopPosition);
		if (bounds.Width > 0f && flag3)
		{
			RotateAndScaleTransform(ref bounds, ref clipBounds, scaling, translatePoints, rotationAngle, isListCharacter: false, flipV, flipH);
			clipWidth = clipBounds.Width;
		}
		bool flag4 = charFormat.SubSuperScript == DocGen.DocIO.DLS.SubSuperScript.None;
		RectangleF textBounds = bounds;
		Rotate(ang, ref flipV, ref flipH, textWrappingStyle, doNotRotateText, flag3, ref isRotateTransformApplied, ref rotationAngle);
		float num2 = 0f;
		string fontNameToRender = charFormat.GetFontNameToRender(scriptType);
		if (!IsListCharacter && (fontNameToRender == "DIN Offc" || fontNameToRender == "DIN OT") && currParagraph != null && currParagraph.IsContainDinOffcFont())
		{
			num2 = bounds.Height * 0.2f;
		}
		if (charFormat.Emboss)
		{
			DocGen.Drawing.Font font3 = ((!flag4) ? charFormat.Document.FontSettings.GetFont(charFormat.GetFontNameToRender(scriptType), GetSubSuperScriptFontSize(charFormat.GetFontToRender(scriptType)), charFormat.GetFontToRender(scriptType).Style, scriptType) : charFormat.GetFontToRender(scriptType));
			if (!charFormat.GetFontNameToRender(scriptType).Equals(font3.Name, StringComparison.OrdinalIgnoreCase) && m_privateFontStream != null && m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType)))
			{
				font3 = GetPrivateFont(charFormat.GetFontNameToRender(scriptType), font3.Size, font3.Style);
			}
			PdfFont pdfFont = null;
			string text2 = GetEmbedFontStyle(charFormat);
			if (!string.IsNullOrEmpty(text2))
			{
				text2 = charFormat.GetFontNameToRender(scriptType) + "_" + text2;
			}
			if (m_privateFontStream != null && (m_privateFontStream.ContainsKey(text2) || m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType))))
			{
				pdfFont = CreatePdfFont(text2, charFormat.GetFontNameToRender(scriptType), font3.Size, GetFontStyle(font3.Style));
			}
			else if (PdfFontCollection.ContainsKey(GetPdfFontCollectionKey(font3, IsUnicode(text))))
			{
				pdfFont = PdfFontCollection[GetPdfFontCollectionKey(font3, IsUnicode(text))];
			}
			else
			{
				pdfFont = CreatePdfFont(text, WordDocument.RenderHelper.GetFontStream(font3, scriptType), font3.Size, GetFontStyle(font3.Style));
				pdfFont.Ascent = GetAscent(font3, txtRange.ScriptType);
				PdfFontCollection.Add(GetPdfFontCollectionKey(font3, IsUnicode(text)), pdfFont);
			}
			PdfBrush brush2 = new PdfSolidBrush(Color.Gray);
			PdfStringFormat pdfStringFormat = PDFGraphics.ConvertFormat(stringFormat, charFormat.ComplexScript || EnableComplexScript);
			if (FallbackFonts != null && FallbackFonts.Count > 0)
			{
				pdfFont = GetFallbackPdfFont(pdfFont, font3, text, scriptType, pdfStringFormat);
			}
			if (IsUnicode(text))
			{
				PDFDrawString(text, pdfFont, brush2, bounds.X + 0.2f, bounds.Y + 0.2f, pdfStringFormat, charFormat, scriptType);
			}
			else
			{
				PDFGraphics.DrawString(text, pdfFont, brush2, new RectangleF(bounds.X + 0.2f, bounds.Y + 0.2f + num2, bounds.Width, bounds.Height), pdfStringFormat, directConversion: true);
			}
			textBounds = new RectangleF(bounds.X + 0.2f, bounds.Y + 0.2f + num2, bounds.Width, bounds.Height);
		}
		if (charFormat.Engrave)
		{
			DocGen.Drawing.Font font4 = ((!flag4) ? charFormat.Document.FontSettings.GetFont(charFormat.GetFontNameToRender(scriptType), charFormat.GetFontSizeToRender() / 1.5f, charFormat.GetFontToRender(scriptType).Style, scriptType) : charFormat.GetFontToRender(scriptType));
			if (!charFormat.GetFontNameToRender(scriptType).Equals(font4.Name, StringComparison.OrdinalIgnoreCase) && m_privateFontStream != null && m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType)))
			{
				font4 = GetPrivateFont(charFormat.GetFontNameToRender(scriptType), font4.Size, font4.Style);
			}
			PdfFont pdfFont2 = null;
			string text3 = GetEmbedFontStyle(charFormat);
			if (!string.IsNullOrEmpty(text3))
			{
				text3 = charFormat.GetFontNameToRender(scriptType) + "_" + text3;
			}
			if (m_privateFontStream != null && (m_privateFontStream.ContainsKey(text3) || m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType))))
			{
				pdfFont2 = CreatePdfFont(text3, charFormat.GetFontNameToRender(scriptType), font4.Size, GetFontStyle(font4.Style));
			}
			else if (PdfFontCollection.ContainsKey(GetPdfFontCollectionKey(font4, IsUnicode(text))))
			{
				pdfFont2 = PdfFontCollection[GetPdfFontCollectionKey(font4, IsUnicode(text))];
			}
			else
			{
				pdfFont2 = CreatePdfFont(text, WordDocument.RenderHelper.GetFontStream(font4, scriptType), font4.Size, GetFontStyle(font4.Style));
				pdfFont2.Ascent = GetAscent(font4, txtRange.ScriptType);
				PdfFontCollection.Add(GetPdfFontCollectionKey(font4, IsUnicode(text)), pdfFont2);
			}
			PdfBrush brush3 = new PdfSolidBrush(Color.Gray);
			PdfStringFormat pdfStringFormat2 = PDFGraphics.ConvertFormat(stringFormat, charFormat.ComplexScript || EnableComplexScript);
			if (FallbackFonts != null && FallbackFonts.Count > 0)
			{
				pdfFont2 = GetFallbackPdfFont(pdfFont2, font4, text, scriptType, pdfStringFormat2);
			}
			if (IsUnicode(text))
			{
				PDFDrawString(text, pdfFont2, brush3, bounds.X - 0.2f, bounds.Y - 0.2f, pdfStringFormat2, charFormat, scriptType);
			}
			else
			{
				PDFGraphics.DrawString(text, pdfFont2, brush3, new RectangleF(bounds.X - 0.2f, bounds.Y - 0.2f + num2, bounds.Width, bounds.Height), pdfStringFormat2, directConversion: true);
			}
			textBounds = new RectangleF(bounds.X - 0.2f, bounds.Y - 0.2f + num2, bounds.Width, bounds.Height);
		}
		if (charFormat.AllCaps)
		{
			text = text.ToUpper();
		}
		SizeF sizeF4 = default(SizeF);
		sizeF4 = ((!flag2) ? MeasureString(text, font2, stringFormat, charFormat, isMeasureFromTabList: false, isMeasureFromSmallCapString: false, scriptType) : MeasureString(text, font, font2, stringFormat, charFormat, scriptType));
		if (ltWidget.Bounds.Width != Convert.ToSingle(sizeF4.Width + ltWidget.SubWidth) && text != string.Empty)
		{
			float num3 = ltWidget.Bounds.Width - num - Convert.ToSingle(sizeF4.Width + ltWidget.SubWidth);
			ltWidget.SubWidth += num3;
			ltWidget.WordSpace = ((ltWidget.Spaces != 0) ? Convert.ToSingle(ltWidget.SubWidth / (float)ltWidget.Spaces) : 0f);
		}
		StringFormat stringFormat2 = new StringFormat();
		stringFormat.FormatFlags &= ~StringFormatFlags.LineLimit;
		stringFormat2.FormatFlags &= ~StringFormatFlags.LineLimit;
		stringFormat2.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
		stringFormat2.FormatFlags |= StringFormatFlags.NoClip;
		if (charFormat.Bidi && (txtRange.CharacterRange != CharacterRangeType.WordSplit || EmbedFonts || EmbedCompleteFonts || !IsInversedCharacter(text)))
		{
			stringFormat2.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
		}
		text.Replace(" ", string.Empty);
		float num4 = 0f;
		num4 = ((!flag2) ? MeasureString(text, font2, stringFormat, charFormat, isMeasureFromTabList: false, isMeasureFromSmallCapString: true, scriptType).Width : MeasureString(text, font, font2, stringFormat, charFormat, scriptType).Width);
		float num5 = 0f;
		for (int i = 0; i < array.Length; i++)
		{
			num5 = ((!flag2) ? (num5 + MeasureString(array[i], font2, stringFormat, charFormat, isMeasureFromTabList: false, isMeasureFromSmallCapString: true, scriptType).Width) : (num5 + MeasureString(array[i], font, font2, stringFormat, charFormat, scriptType).Width));
		}
		float num6 = 0f;
		if (ltWidget.Spaces > 0)
		{
			num6 = (num4 - num5) / (float)ltWidget.Spaces;
		}
		float num7 = 0f;
		float num8 = scaling / 100f;
		if (num8 != 1f)
		{
			clipBounds = new RectangleF(clipBounds.X * num8, clipBounds.Y, clipBounds.Width * num8, clipBounds.Height);
			bounds = new RectangleF(bounds.X * num8, bounds.Y, bounds.Width * num8, bounds.Height);
		}
		RectangleF rectangleF = clipBounds;
		RectangleF rectangleF2 = bounds;
		if (currTextRange != null && currTextRange.m_layoutInfo != null && currTextRange.m_layoutInfo.IsVerticalText)
		{
			TransformGraphicsPosition(ltWidget, flag3, ref isRotateTransformApplied, ref translatePoints, ref rotationAngle, currParagraph);
		}
		if (bounds.Width > 0f && flag3)
		{
			RotateAndScaleTransform(ref bounds, ref clipBounds, scaling, translatePoints, rotationAngle, isListCharacter: false, flipV, flipH);
			clipWidth = clipBounds.Width;
		}
		float num9 = bounds.X + ltWidget.Bounds.Width;
		for (int j = 0; j < array.Length; j++)
		{
			bool flag5 = false;
			num7 = ((!(font.Name != font2.Name) || !IsUnicodeText(array[j])) ? MeasureString(array[j], font2, stringFormat, charFormat, isMeasureFromTabList: false, isMeasureFromSmallCapString: true, scriptType).Width : MeasureString(array[j], font, font2, stringFormat, charFormat, scriptType).Width);
			if (charFormat.AllCaps)
			{
				array[j] = array[j].ToUpper();
			}
			if (ltWidget.CharacterRange == CharacterRangeType.RTL)
			{
				stringFormat2.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
				num9 -= num7;
				DrawRTLText(scriptType, ltWidget.CharacterRange, array[j], charFormat, font2, brush, new RectangleF(num9, bounds.Y, num7, bounds.Height), stringFormat2);
			}
			else if (IsUnicode(array[j]))
			{
				DrawChineseText(scriptType, ltWidget.CharacterRange, array[j], charFormat, font, brush, new RectangleF(bounds.X, bounds.Y, num7, bounds.Height), stringFormat2);
				flag5 = true;
			}
			else
			{
				float num10 = ltWidget.SubWidth / num8;
				if (num10 < 0f)
				{
					num10 = 0f;
				}
				float num11 = clipWidth - (bounds.X - ltWidget.Bounds.X / num8) + num10;
				float width = clipWidth - (bounds.X - ltWidget.Bounds.X) + num10;
				if (num11 < 0f)
				{
					num11 = 0f;
				}
				clipBounds = new RectangleF(bounds.X, bounds.Y, num11, bounds.Height);
				rectangleF = new RectangleF(rectangleF2.X, rectangleF2.Y, width, rectangleF2.Height);
				if (IsNeedToClip(rectangleF))
				{
					clipBounds = ClipBoundsContainer.Peek();
				}
				SetClip(clipBounds);
				if (PDFGraphics.ClipBounds.Width == 0f)
				{
					PDFGraphics.ResetTransform();
					ResetClip();
					continue;
				}
				if (charFormat.SmallCaps)
				{
					DrawSmallCapString(scriptType, ltWidget.CharacterRange, array[j], charFormat, new RectangleF(bounds.X, bounds.Y, num7, bounds.Height), font2, stringFormat2, brush, isCharacterSpacing: false);
				}
				else
				{
					DocGen.Drawing.Font font5 = ((!flag4) ? charFormat.Document.FontSettings.GetFont(font2.Name, font2.Size / 1.5f, font2.Style, scriptType) : font2);
					if (!charFormat.GetFontNameToRender(scriptType).Equals(font5.Name, StringComparison.OrdinalIgnoreCase) && m_privateFontStream != null && m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType)))
					{
						font5 = GetPrivateFont(charFormat.GetFontNameToRender(scriptType), font5.Size, font5.Style);
					}
					if (charFormat.CharacterSpacing != 0f)
					{
						DrawStringBasedOnCharSpacing(scriptType, ltWidget.CharacterRange, font5, brush, new RectangleF(bounds.X, bounds.Y, num7, bounds.Height), array[j], stringFormat, charFormat);
					}
					else
					{
						PdfFont pdfFont3 = null;
						string text4 = GetEmbedFontStyle(charFormat);
						if (!string.IsNullOrEmpty(text4))
						{
							text4 = charFormat.GetFontNameToRender(scriptType) + "_" + text4;
						}
						if (m_privateFontStream != null && (m_privateFontStream.ContainsKey(text4) || m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType))))
						{
							pdfFont3 = CreatePdfFont(text4, charFormat.GetFontNameToRender(scriptType), font5.Size, GetFontStyle(font5.Style));
						}
						else if (PdfFontCollection.ContainsKey(GetPdfFontCollectionKey(font5, IsUnicode(array[j]))))
						{
							pdfFont3 = PdfFontCollection[GetPdfFontCollectionKey(font5, IsUnicode(array[j]))];
						}
						else
						{
							pdfFont3 = CreatePdfFont(array[j], WordDocument.RenderHelper.GetFontStream(font5, scriptType), font5.Size, GetFontStyle(font5.Style));
							pdfFont3.Ascent = GetAscent(font5, txtRange.ScriptType);
							PdfFontCollection.Add(GetPdfFontCollectionKey(font5, IsUnicode(array[j])), pdfFont3);
						}
						PdfBrush brush4 = new PdfSolidBrush(textColor);
						PdfStringFormat pdfStringFormat3 = PDFGraphics.ConvertFormat(stringFormat2, charFormat.ComplexScript || EnableComplexScript);
						if (FallbackFonts != null && FallbackFonts.Count > 0)
						{
							pdfFont3 = GetFallbackPdfFont(pdfFont3, font5, array[j], scriptType, pdfStringFormat3);
						}
						PDFGraphics.DrawString(array[j], pdfFont3, brush4, new RectangleF(bounds.X, bounds.Y + num2, num7, bounds.Height), pdfStringFormat3, directConversion: true);
					}
				}
			}
			DocGen.Drawing.Font font6 = (flag5 ? font : font2);
			float num12 = num6 + ltWidget.WordSpace / num8;
			if (font6.Underline && j != array.Length - 1)
			{
				string text5 = "";
				while (num12 > 0f)
				{
					text5 += " ";
					num12 -= MeasureString(" ", font2, stringFormat, FontScriptType.English).Width;
				}
				float width2 = MeasureString(text5, font6, stringFormat, FontScriptType.English).Width;
				DocGen.Drawing.Font font7 = ((!flag4) ? charFormat.Document.FontSettings.GetFont(font6.Name, font6.Size / 1.5f, font6.Style, scriptType) : font6);
				if (!charFormat.GetFontNameToRender(scriptType).Equals(font7.Name, StringComparison.OrdinalIgnoreCase) && m_privateFontStream != null && m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType)))
				{
					font7 = GetPrivateFont(charFormat.GetFontNameToRender(scriptType), font7.Size, font7.Style);
				}
				PdfFont pdfFont4 = null;
				string text6 = GetEmbedFontStyle(charFormat);
				if (!string.IsNullOrEmpty(text6))
				{
					text6 = charFormat.GetFontNameToRender(scriptType) + "_" + text6;
				}
				if (m_privateFontStream != null && (m_privateFontStream.ContainsKey(text6) || m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType))))
				{
					pdfFont4 = CreatePdfFont(text6, charFormat.GetFontNameToRender(scriptType), font7.Size, GetFontStyle(font7.Style));
				}
				else if (PdfFontCollection.ContainsKey(GetPdfFontCollectionKey(font, IsUnicode(text5))))
				{
					pdfFont4 = PdfFontCollection[GetPdfFontCollectionKey(font, IsUnicode(text5))];
				}
				else
				{
					pdfFont4 = CreatePdfFont(text5, WordDocument.RenderHelper.GetFontStream(font, scriptType), font.Size, GetFontStyle(font7.Style));
					pdfFont4.Ascent = GetAscent(font7, txtRange.ScriptType);
					PdfFontCollection.Add(GetPdfFontCollectionKey(font, IsUnicode(text5)), pdfFont4);
				}
				PdfBrush brush5 = new PdfSolidBrush(GetTextColor(charFormat));
				PdfStringFormat pdfStringFormat4 = PDFGraphics.ConvertFormat(stringFormat, charFormat.ComplexScript || EnableComplexScript);
				if (FallbackFonts != null && FallbackFonts.Count > 0)
				{
					pdfFont4 = GetFallbackPdfFont(pdfFont4, font7, text5, scriptType, pdfStringFormat4);
				}
				PDFGraphics.DrawString(text5, pdfFont4, brush5, new RectangleF(bounds.X + num7, bounds.Y + num2, width2, bounds.Height), pdfStringFormat4, directConversion: true);
			}
			if (!ltWidget.IsLastLine && j != array.Length - 1)
			{
				string text7 = " ";
				RectangleF rect = new RectangleF(bounds.X + num7, bounds.Y + num2, num6 + ltWidget.WordSpace, bounds.Height);
				DocGen.Drawing.Font font8 = ((!flag4) ? charFormat.Document.FontSettings.GetFont(font6.Name, font6.Size / 1.5f, font6.Style, scriptType) : font6);
				if (!charFormat.GetFontNameToRender(scriptType).Equals(font8.Name, StringComparison.OrdinalIgnoreCase) && m_privateFontStream != null && m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType)))
				{
					font8 = GetPrivateFont(charFormat.GetFontNameToRender(scriptType), font8.Size, font8.Style);
				}
				if ((ltWidget.CharacterRange & CharacterRangeType.RTL) == CharacterRangeType.RTL)
				{
					rect = new RectangleF(num9 - (num6 + ltWidget.WordSpace), bounds.Y + num2, num6 + ltWidget.WordSpace, bounds.Height);
					num9 -= ltWidget.WordSpace / num8 + num6;
				}
				PdfFont pdfFont5 = null;
				string text8 = GetEmbedFontStyle(charFormat);
				if (!string.IsNullOrEmpty(text8))
				{
					text8 = charFormat.GetFontNameToRender(scriptType) + "_" + text8;
				}
				if (m_privateFontStream != null && (m_privateFontStream.ContainsKey(text8) || m_privateFontStream.ContainsKey(charFormat.GetFontNameToRender(scriptType))))
				{
					pdfFont5 = CreatePdfFont(text8, charFormat.GetFontNameToRender(scriptType), font8.Size, GetFontStyle(font8.Style));
				}
				else if (PdfFontCollection.ContainsKey(GetPdfFontCollectionKey(font, IsUnicode(text7))))
				{
					pdfFont5 = PdfFontCollection[GetPdfFontCollectionKey(font, IsUnicode(text7))];
				}
				else
				{
					pdfFont5 = CreatePdfFont(WordDocument.RenderHelper.GetFontStream(font, scriptType), font.Size, GetFontStyle(font8.Style));
					pdfFont5.Ascent = GetAscent(font, txtRange.ScriptType);
					PdfFontCollection.Add(GetPdfFontCollectionKey(font, IsUnicode(text7)), pdfFont5);
				}
				PdfBrush brush6 = new PdfSolidBrush(GetTextColor(charFormat));
				PdfStringFormat format = PDFGraphics.ConvertFormat(stringFormat, charFormat.ComplexScript || EnableComplexScript);
				if (FallbackFonts != null && FallbackFonts.Count > 0)
				{
					pdfFont5 = GetFallbackPdfFont(pdfFont5, font8, text7, scriptType, new PdfStringFormat());
				}
				PDFGraphics.DrawString(text7, pdfFont5, brush6, rect, format, directConversion: true);
				float num13 = num7 + ltWidget.WordSpace / num8 + num6;
				bounds.X += num13;
				float num14 = num7 + ltWidget.WordSpace + num6;
				rectangleF2.X += num14;
			}
			else
			{
				bounds.X = bounds.X + num7 + num6;
				rectangleF2.X = rectangleF2.X + num7 + num6;
				if ((ltWidget.CharacterRange & CharacterRangeType.RTL) == CharacterRangeType.RTL)
				{
					num9 -= num6;
				}
			}
			ResetClip();
		}
		if (flag)
		{
			bool isSameLine = false;
			CheckPreOrNextSiblingIsTab(ref charFormat, ref textBounds, ltWidget, ref isSameLine);
			AddLineToCollection(text, flag4, font, charFormat, textBounds, isSameLine, scriptType);
		}
		ResetTransform();
	}

	private void RotateAndScaleTransform(ref RectangleF bounds, ref RectangleF clipBounds, float scaleFactor, PointF translatePoints, float rotationAngle, bool isListCharacter, bool flipV, bool flipH)
	{
		scaleFactor /= 100f;
		if (scaleFactor == 0f)
		{
			scaleFactor = 1f;
		}
		if (!isListCharacter)
		{
			ScaleTransformMatrix(scaleFactor, translatePoints, rotationAngle, flipV, flipH);
			bounds = new RectangleF(bounds.X / scaleFactor, bounds.Y, bounds.Width / scaleFactor, bounds.Height);
			clipBounds = new RectangleF(clipBounds.X / scaleFactor, clipBounds.Y, clipBounds.Width / scaleFactor, clipBounds.Height);
		}
		else
		{
			bounds = new RectangleF(bounds.X, bounds.Y, bounds.Width / scaleFactor, bounds.Height);
		}
	}

	private void ScaleTransformMatrix(float scaleFactor, PointF translatePoints, float rotationAngle, bool flipV, bool flipH)
	{
		Matrix matrix = new Matrix();
		if ((rotationAngle != 0f || flipV || flipH) && currTextRange != null && currTextRange.m_layoutInfo != null && !currTextRange.m_layoutInfo.IsVerticalText)
		{
			matrix = GetTransformMatrix(m_rotateTransform, rotationAngle, flipH, flipV);
			WordDocument.RenderHelper.ApplyScale(matrix, scaleFactor, 1f);
		}
		else
		{
			WordDocument.RenderHelper.RotateMatrix(matrix, rotationAngle);
			WordDocument.RenderHelper.ApplyScale(matrix, scaleFactor, 1f);
			WordDocument.RenderHelper.TranslateMatrix(matrix, translatePoints.X, translatePoints.Y, MatrixOrder.Append);
		}
		PDFGraphics.Transform = matrix;
	}

	private float[] GetPDFMatrix(float[] drawingMatrix)
	{
		return new float[6]
		{
			drawingMatrix[0],
			drawingMatrix[3],
			drawingMatrix[1],
			drawingMatrix[4],
			drawingMatrix[2],
			drawingMatrix[5]
		};
	}

	private PdfTrueTypeFont CreatePdfFont(Stream fontStream, float size, PdfFontStyle fontStyle)
	{
		return CreatePdfFont(null, fontStream, size, string.Empty, fontStyle);
	}

	internal PdfTrueTypeFont CreatePdfFont(string text, Stream fontStream, float size, PdfFontStyle fontStyle)
	{
		return CreatePdfFont(text, fontStream, size, string.Empty, fontStyle);
	}

	private PdfTrueTypeFont CreatePdfFont(string text, Stream fontStream, float size, string metricsName, PdfFontStyle fontStyle)
	{
		if (EmbedCompleteFonts)
		{
			return new PdfTrueTypeFont(fontStream, size, metricsName, isEnableEmbedding: true, fontStyle);
		}
		if (EmbedFonts || (text != null && IsUnicode(text)))
		{
			return new PdfTrueTypeFont(fontStream, size, isUnicode: true, metricsName, fontStyle);
		}
		return new PdfTrueTypeFont(fontStream, size, isUnicode: false, metricsName, fontStyle);
	}

	private PdfTransformationMatrix PrepareMatrix(Matrix matrix, float pageScale)
	{
		PdfTransformationMatrix pdfTransformationMatrix = new PdfTransformationMatrix();
		PdfTransformationMatrix matrix2 = new PdfTransformationMatrix
		{
			Matrix = matrix
		};
		pdfTransformationMatrix.Scale(pageScale, 0f - pageScale);
		pdfTransformationMatrix.Multiply(matrix2);
		pdfTransformationMatrix.Scale(1f, -1f);
		return pdfTransformationMatrix;
	}

	private void DrawParagraphBorders(WParagraph paragraph, WParagraphFormat paraFormat, LayoutedWidget ltWidget, bool isParagraphMarkIsHidden)
	{
		RectangleF bounds = ltWidget.Bounds;
		ParagraphLayoutInfo paragraphLayoutInfo = paragraph.m_layoutInfo as ParagraphLayoutInfo;
		if (paragraphLayoutInfo != null && paragraphLayoutInfo.SkipBottomBorder && paragraphLayoutInfo.SkipLeftBorder && paragraphLayoutInfo.SkipRightBorder && paragraphLayoutInfo.SkipTopBorder && paragraphLayoutInfo.SkipHorizonatalBorder)
		{
			return;
		}
		if (ltWidget.ChildWidgets.Count == 1 && ltWidget.ChildWidgets[0].Bounds.Width == 0f)
		{
			LayoutedWidget layoutedWidget = ltWidget.ChildWidgets[0];
			bool flag = false;
			for (int i = 0; i < layoutedWidget.ChildWidgets.Count; i++)
			{
				if (!(layoutedWidget.ChildWidgets[i].Widget is BookmarkStart) && !(layoutedWidget.ChildWidgets[i].Widget is BookmarkEnd))
				{
					if (!(layoutedWidget.ChildWidgets[i].Widget is Break) || (layoutedWidget.ChildWidgets[i].Widget as Break).BreakType != 0 || i != layoutedWidget.ChildWidgets.Count - 1)
					{
						break;
					}
					flag = true;
				}
			}
			if (flag)
			{
				return;
			}
		}
		if (paragraph.GetOwnerEntity() is WTableCell { Owner: WTableRow { m_layoutInfo: RowLayoutInfo layoutInfo } } && paragraph.IsInCell && !layoutInfo.IsExactlyRowHeight)
		{
			bounds.Height -= paragraph.ParagraphFormat.AfterSpacing;
		}
		if (ltWidget.Owner != null && isParagraphMarkIsHidden)
		{
			AddNextParagraphBounds(ltWidget, ref bounds);
		}
		bool flag2 = false;
		bool flag3 = false;
		ILayoutInfo layoutInfo2 = ltWidget.Widget.LayoutInfo;
		if (!(layoutInfo2 is ParagraphLayoutInfo { FirstLineIndent: var num } paragraphLayoutInfo2))
		{
			return;
		}
		if (num > 0f)
		{
			num = 0f;
		}
		float num2 = paragraphLayoutInfo2.Paddings.Left + num;
		float num3 = paragraphLayoutInfo2.Paddings.Right;
		IEntity entity = paragraph.NextSibling;
		if (entity == null && paragraph.OwnerTextBody.Owner is BlockContentControl)
		{
			entity = (paragraph.OwnerTextBody.Owner as BlockContentControl).NextSibling;
		}
		if (entity is BlockContentControl)
		{
			entity = (entity as BlockContentControl).GetFirstParagraphOfSDTContent();
		}
		if (ltWidget.IsLastItemInPage && ltWidget.TextTag == "Splitted")
		{
			flag2 = true;
			bounds.Height -= paragraph.ParagraphFormat.Borders.Bottom.Space;
		}
		if (ltWidget.IsLastItemInPage && ltWidget.TextTag != "Splitted")
		{
			paragraphLayoutInfo.SkipBottomBorder = paragraph.ParagraphFormat.Borders.Bottom.BorderType == BorderStyle.None;
		}
		if (entity is WParagraph && !ltWidget.IsLastItemInPage && !(entity as WParagraph).ParagraphFormat.Borders.NoBorder)
		{
			flag2 = paragraph.IsAdjacentParagraphHaveSameBorders(entity as WParagraph, paragraphLayoutInfo2.Margins.Left + num);
		}
		IEntity entity2 = paragraph.PreviousSibling;
		if (entity2 == null && paragraph.OwnerTextBody.Owner is BlockContentControl)
		{
			entity2 = (paragraph.OwnerTextBody.Owner as BlockContentControl).PreviousSibling;
		}
		if (entity2 is BlockContentControl)
		{
			entity2 = (entity2 as BlockContentControl).GetLastParagraphOfSDTContent();
		}
		Borders borders = null;
		if (entity2 is WParagraph && !layoutInfo2.IsFirstItemInPage)
		{
			borders = (entity2 as WParagraph).ParagraphFormat.Borders;
			if (!borders.NoBorder)
			{
				flag3 = paragraph.IsAdjacentParagraphHaveSameBorders(entity2 as WParagraph, paragraphLayoutInfo2.Margins.Left + num);
			}
		}
		else if (paragraphLayoutInfo != null)
		{
			paragraphLayoutInfo.SkipTopBorder = paragraph.ParagraphFormat.Borders.Top.BorderType == BorderStyle.None;
			paragraphLayoutInfo.SkipHorizonatalBorder = true;
		}
		bool flag4 = false;
		if (ltWidget.ChildWidgets.Count > 0 && Math.Round(ltWidget.Bounds.Y, 2) != Math.Round(ltWidget.ChildWidgets[0].Bounds.Y, 2))
		{
			float num4 = 0f;
			if (borders != null)
			{
				num4 = borders.Horizontal.GetLineWidthValue() + borders.Horizontal.Space;
			}
			bounds.Y = ltWidget.ChildWidgets[0].Bounds.Y - ((!paragraphLayoutInfo.SkipTopBorder) ? (paraFormat.Borders.Top.GetLineWidthValue() + paraFormat.Borders.Top.Space) : (paragraphLayoutInfo.SkipHorizonatalBorder ? 0f : num4));
			bounds.Height = ltWidget.Bounds.Bottom - bounds.Y;
			flag4 = true;
		}
		if (paragraph.IsInCell && !paragraph.IsExactlyRowHeight() && paragraphLayoutInfo.BottomMargin > 0f && !(paragraph.ParagraphFormat.AfterSpacing > 0f))
		{
			bounds.Height -= paragraphLayoutInfo.BottomMargin;
		}
		Entity entity3 = paragraph.GetOwnerEntity();
		if (paragraphLayoutInfo.SkipBottomBorder)
		{
			bounds.Height += paragraphLayoutInfo.BottomMargin;
			if (!paraFormat.BackColor.IsEmpty && !flag2)
			{
				bounds.Height -= paragraphLayoutInfo.BottomMargin;
			}
		}
		if (paragraph.Document.DOP.Dop2000.Copts.DontUseHTMLParagraphAutoSpacing && paragraphLayoutInfo.SkipTopBorder)
		{
			bounds.Y -= paragraphLayoutInfo.TopMargin;
			bounds.Height += paragraphLayoutInfo.TopMargin;
		}
		bounds.Width = bounds.Width - (layoutInfo2 as ILayoutSpacingsInfo).Margins.Left - paraFormat.RightIndent;
		if (ltWidget.ChildWidgets.Count == 1 && ltWidget.TextTag != "Splitted" && !paragraph.IsInCell && paragraph.ParagraphFormat.IsFrame && paragraph.ParagraphFormat.FrameWidth == 0f && !paragraph.ParagraphFormat.IsNextParagraphInSameFrame() && !paragraph.ParagraphFormat.IsPreviousParagraphInSameFrame())
		{
			bounds.Width = ltWidget.ChildWidgets[0].Bounds.Width;
		}
		bounds.X += num2;
		bounds.Width += Math.Abs(num2 + num3);
		if (!flag4)
		{
			bounds.Height = bounds.Bottom - bounds.Y;
		}
		Borders borders2 = paraFormat.Borders;
		List<Border> list = new List<Border>();
		if (!paragraphLayoutInfo.SkipHorizonatalBorder && flag3)
		{
			if (!paragraphLayoutInfo.SkipLeftBorder && !paragraphLayoutInfo.SkipRightBorder)
			{
				list.Add(borders.Horizontal);
				list.Add(borders2.Left);
				list.Add(borders2.Right);
				if (!(borders.Horizontal.Color == borders2.Left.Color) || !(borders.Horizontal.Color == borders2.Right.Color))
				{
					if (borders.Horizontal.LineWidth == borders2.Left.LineWidth && borders.Horizontal.LineWidth == borders2.Right.LineWidth)
					{
						list.Sort(new SortByColorBrightness());
					}
					else if (borders.Horizontal.LineWidth == borders2.Left.LineWidth)
					{
						SortTwoBorders(list, borders.Horizontal, borders2.Left, borders2, isLeftBorder: true);
					}
					else if (borders.Horizontal.LineWidth == borders2.Right.LineWidth)
					{
						SortTwoBorders(list, borders.Horizontal, borders2.Right, borders2, isLeftBorder: false);
					}
				}
			}
			else if (!paragraphLayoutInfo.SkipLeftBorder)
			{
				list.Add(borders.Horizontal);
				list.Add(borders2.Left);
				SortTwoBorders(list, borders.Horizontal, borders2.Left, null, isLeftBorder: false);
			}
			else if (!paragraphLayoutInfo.SkipRightBorder)
			{
				list.Add(borders.Horizontal);
				list.Add(borders2.Right);
				SortTwoBorders(list, borders.Horizontal, borders2.Right, null, isLeftBorder: false);
			}
			else
			{
				list.Add(borders.Horizontal);
			}
		}
		if (list.Count > 0)
		{
			if (!paragraphLayoutInfo.SkipBottomBorder && !flag2)
			{
				list.Add(borders2.Bottom);
			}
		}
		else
		{
			if (!paragraphLayoutInfo.SkipLeftBorder)
			{
				list.Add(borders2.Left);
			}
			if (!paragraphLayoutInfo.SkipRightBorder)
			{
				list.Add(borders2.Right);
			}
			if (!paragraphLayoutInfo.SkipTopBorder && !flag3)
			{
				list.Add(borders2.Top);
			}
			if (!paragraphLayoutInfo.SkipBottomBorder && !flag2)
			{
				list.Add(borders2.Bottom);
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		RectangleF rectangleF = RectangleF.Empty;
		if (ClipBoundsContainer != null && ClipBoundsContainer.Count > 0 && !(entity3 is WTextBox))
		{
			rectangleF = ClipBoundsContainer.Peek();
		}
		SetClip(rectangleF);
		bool flag5 = false;
		while (entity3 is WTableCell)
		{
			entity3 = (entity3 as WTableCell).OwnerRow.OwnerTable.Owner;
		}
		if (entity3 is WTextBody)
		{
			entity3 = entity3.Owner;
		}
		if (entity3 is WTextBox && (entity3 as WTextBox).TextBoxFormat.Rotation != 0f)
		{
			flag5 = true;
			SetRotateTransform(m_rotateTransform, (entity3 as WTextBox).TextBoxFormat.Rotation, (entity3 as WTextBox).TextBoxFormat.FlipVertical, (entity3 as WTextBox).TextBoxFormat.FlipHorizontal);
		}
		else if (entity3 is Shape && (entity3 as Shape).Rotation != 0f)
		{
			flag5 = true;
			SetRotateTransform(m_rotateTransform, (entity3 as Shape).Rotation, (entity3 as Shape).FlipVertical, (entity3 as Shape).FlipHorizontal);
		}
		else if (entity3 is ChildShape && (entity3 as ChildShape).Rotation != 0f)
		{
			flag5 = true;
			ChildShape childShape = entity3 as ChildShape;
			RectangleF textLayoutingBounds = childShape.TextLayoutingBounds;
			if (childShape.AutoShapeType == DocGen.DocIO.DLS.AutoShapeType.Rectangle)
			{
				textLayoutingBounds.Height = GetLayoutedTextBoxContentHeight(ltWidget);
			}
			Rotate(childShape, childShape.Rotation, childShape.FlipVertical, childShape.FlipHorizantal, textLayoutingBounds);
		}
		DrawParagraphBorders(list, bounds, borders2, borders, paragraph, ltWidget);
		if (flag5)
		{
			ResetTransform();
		}
		if (rectangleF != RectangleF.Empty)
		{
			ResetClip();
		}
	}

	private void DrawParagraphBorders(List<Border> borderRenderingOrder, RectangleF bounds, Borders borders, Borders previousBorder, WParagraph paragraph, LayoutedWidget ltWidget)
	{
		bounds.X -= 1.5f;
		bounds.Width += 3f;
		float rightBorderLineWidth;
		float topBorderLineWidth;
		float bottomBorderLineWidth;
		float betweenBorderLineWidth;
		float leftBorderLineWidth = (rightBorderLineWidth = (topBorderLineWidth = (bottomBorderLineWidth = (betweenBorderLineWidth = 0f))));
		bool isMultiLineHorizontalBorder;
		bool isMultiLineLeftBorder;
		bool isMultiLineRightBorder;
		bool isMultiLineTopBorder;
		bool isMultiLineBottomBorder = (isMultiLineHorizontalBorder = (isMultiLineLeftBorder = (isMultiLineRightBorder = (isMultiLineTopBorder = false))));
		foreach (Border item in borderRenderingOrder)
		{
			switch (item.BorderPosition)
			{
			case Border.BorderPositions.Left:
				isMultiLineLeftBorder = IsMultiLineParagraphBorder(item.BorderType);
				leftBorderLineWidth = item.GetLineWidthValue() / 2f;
				break;
			case Border.BorderPositions.Right:
				isMultiLineRightBorder = IsMultiLineParagraphBorder(item.BorderType);
				rightBorderLineWidth = item.GetLineWidthValue() / 2f;
				break;
			case Border.BorderPositions.Bottom:
				isMultiLineBottomBorder = IsMultiLineParagraphBorder(item.BorderType);
				bottomBorderLineWidth = item.GetLineWidthValue() / 2f;
				break;
			case Border.BorderPositions.Top:
				isMultiLineTopBorder = IsMultiLineParagraphBorder(item.BorderType);
				topBorderLineWidth = item.GetLineWidthValue() / 2f;
				break;
			case Border.BorderPositions.Horizontal:
				isMultiLineHorizontalBorder = IsMultiLineParagraphBorder(item.BorderType);
				betweenBorderLineWidth = item.GetLineWidthValue() / 2f;
				break;
			}
		}
		foreach (Border item2 in borderRenderingOrder)
		{
			switch (item2.BorderPosition)
			{
			case Border.BorderPositions.Horizontal:
				DrawHorizontalBorder(borderRenderingOrder, bounds, borders, item2, isMultiLineLeftBorder, isMultiLineRightBorder, isMultiLineHorizontalBorder, betweenBorderLineWidth, leftBorderLineWidth, rightBorderLineWidth, paragraph, ltWidget);
				break;
			case Border.BorderPositions.Left:
				DrawLeftBorder(borderRenderingOrder, bounds, borders, item2, previousBorder, isMultiLineTopBorder, isMultiLineBottomBorder, isMultiLineLeftBorder, isMultiLineHorizontalBorder, leftBorderLineWidth, topBorderLineWidth, betweenBorderLineWidth, bottomBorderLineWidth, paragraph, ltWidget);
				break;
			case Border.BorderPositions.Right:
				DrawRightBorder(borderRenderingOrder, bounds, borders, item2, previousBorder, isMultiLineTopBorder, isMultiLineHorizontalBorder, isMultiLineRightBorder, isMultiLineBottomBorder, rightBorderLineWidth, topBorderLineWidth, betweenBorderLineWidth, bottomBorderLineWidth, paragraph, ltWidget);
				break;
			case Border.BorderPositions.Top:
				DrawTopBorder(borderRenderingOrder, bounds, borders, item2, isMultiLineTopBorder, topBorderLineWidth, leftBorderLineWidth, rightBorderLineWidth);
				break;
			case Border.BorderPositions.Bottom:
				DrawBottomBorder(borderRenderingOrder, bounds, borders, item2, isMultiLineBottomBorder, leftBorderLineWidth, bottomBorderLineWidth, rightBorderLineWidth, paragraph, ltWidget);
				break;
			}
		}
	}

	private void DrawHorizontalBorder(List<Border> borderRenderingOrder, RectangleF bounds, Borders borders, Border border, bool isMultiLineLeftBorder, bool isMultiLineRightBorder, bool isMultiLineHorizontalBorder, float betweenBorderLineWidth, float leftBorderLineWidth, float rightBorderLineWidth, WParagraph paragraph, LayoutedWidget ltWidget)
	{
		bool flag = false;
		bool flag2 = false;
		bool isOverlapLeft = false;
		bool isOverlapRight = false;
		Border leftBorder = null;
		Border rightBorder = null;
		if (borderRenderingOrder.Contains(borders.Left))
		{
			if (isMultiLineLeftBorder)
			{
				flag = border.BorderType == borders.Left.BorderType && betweenBorderLineWidth == leftBorderLineWidth;
				isOverlapLeft = borderRenderingOrder.IndexOf(border) > borderRenderingOrder.IndexOf(borders.Left);
			}
			leftBorder = borders.Left;
		}
		if (borderRenderingOrder.Contains(borders.Right))
		{
			if (isMultiLineRightBorder)
			{
				flag2 = border.BorderType == borders.Right.BorderType && betweenBorderLineWidth == rightBorderLineWidth;
				isOverlapRight = borderRenderingOrder.IndexOf(border) > borderRenderingOrder.IndexOf(borders.Right);
			}
			rightBorder = borders.Right;
		}
		float left = bounds.Left;
		float right = bounds.Right;
		left = (flag ? (left - leftBorderLineWidth) : (left + leftBorderLineWidth));
		right = (flag2 ? (right + rightBorderLineWidth) : (right - rightBorderLineWidth));
		if (isMultiLineHorizontalBorder)
		{
			DrawMultiLineBetweenBorder(border, new PointF(left, bounds.Top), new PointF(right, bounds.Top), flag, flag2, leftBorder, rightBorder, isOverlapLeft, isOverlapRight);
		}
		else
		{
			DrawParagraphBorder(border, new PointF(left, bounds.Top + betweenBorderLineWidth), new PointF(right, bounds.Top + betweenBorderLineWidth));
		}
	}

	private void DrawLeftBorder(List<Border> borderRenderingOrder, RectangleF bounds, Borders borders, Border border, Borders previousBorder, bool isMultiLineTopBorder, bool isMultiLineBottomBorder, bool isMultiLineLeftBorder, bool isMultiLineHorizontalBorder, float leftBorderLineWidth, float topBorderLineWidth, float betweenBorderLineWidth, float bottomBorderLineWidth, WParagraph paragraph, LayoutedWidget ltWidget)
	{
		float num = 0f;
		float num2 = 0f;
		bool flag = false;
		bool flag2 = false;
		bool isBetweenBorderSame = false;
		Entity ownerEntity = paragraph.GetOwnerEntity();
		if ((ownerEntity is WTextBox && paragraph.Index == (ownerEntity as WTextBox).TextBoxBody.ChildEntities.Count - 1) || (ownerEntity is Shape && paragraph.Index == (ownerEntity as Shape).TextBody.ChildEntities.Count - 1))
		{
			bounds.Height = ltWidget.Owner.Owner.Bounds.Bottom - bounds.Y - GetSpaceForBottomBorder(ownerEntity);
		}
		if (borderRenderingOrder.Contains(borders.Top) && isMultiLineTopBorder)
		{
			flag = border.BorderType == borders.Top.BorderType && leftBorderLineWidth == topBorderLineWidth;
			num = (flag ? 0f : (topBorderLineWidth * 2f));
		}
		else if (previousBorder != null && borderRenderingOrder.Contains(previousBorder.Horizontal) && isMultiLineHorizontalBorder)
		{
			isBetweenBorderSame = border.BorderType == previousBorder.Horizontal.BorderType && leftBorderLineWidth == betweenBorderLineWidth && (previousBorder.Horizontal.Color == border.Color || borderRenderingOrder.IndexOf(previousBorder.Horizontal) > borderRenderingOrder.IndexOf(border));
		}
		if (borderRenderingOrder.Contains(borders.Bottom) && isMultiLineBottomBorder)
		{
			flag2 = border.BorderType == borders.Bottom.BorderType && leftBorderLineWidth == bottomBorderLineWidth;
			num2 = (flag2 ? 0f : (bottomBorderLineWidth * 2f));
		}
		if (isMultiLineLeftBorder)
		{
			DrawMultiLineLeftBorder(border, new PointF(bounds.Left - leftBorderLineWidth, bounds.Top + num), new PointF(bounds.Left - leftBorderLineWidth, bounds.Bottom - bottomBorderLineWidth * 2f), flag, isBetweenBorderSame, flag2);
		}
		else
		{
			DrawParagraphBorder(border, new PointF(bounds.Left, bounds.Top + num), new PointF(bounds.Left, bounds.Bottom - num2));
		}
	}

	private void DrawRightBorder(List<Border> borderRenderingOrder, RectangleF bounds, Borders borders, Border border, Borders previousBorder, bool isMultiLineTopBorder, bool isMultiLineHorizontalBorder, bool isMultiLineRightBorder, bool isMultiLineBottomBorder, float rightBorderLineWidth, float topBorderLineWidth, float betweenBorderLineWidth, float bottomBorderLineWidth, WParagraph paragraph, LayoutedWidget ltWidget)
	{
		float num = 0f;
		float num2 = 0f;
		bool flag = false;
		bool flag2 = false;
		bool isBetweenBorderSame = false;
		Entity ownerEntity = paragraph.GetOwnerEntity();
		if ((ownerEntity is WTextBox && paragraph.Index == (ownerEntity as WTextBox).TextBoxBody.ChildEntities.Count - 1) || (ownerEntity is Shape && paragraph.Index == (ownerEntity as Shape).TextBody.ChildEntities.Count - 1))
		{
			bounds.Height = ltWidget.Owner.Owner.Bounds.Bottom - bounds.Y - GetSpaceForBottomBorder(ownerEntity);
		}
		if (borderRenderingOrder.Contains(borders.Top) && isMultiLineTopBorder)
		{
			flag = border.BorderType == borders.Top.BorderType && rightBorderLineWidth == topBorderLineWidth;
			num = (flag ? 0f : (topBorderLineWidth * 2f));
		}
		else if (previousBorder != null && borderRenderingOrder.Contains(previousBorder.Horizontal) && isMultiLineHorizontalBorder)
		{
			isBetweenBorderSame = border.BorderType == previousBorder.Horizontal.BorderType && rightBorderLineWidth == betweenBorderLineWidth && (previousBorder.Horizontal.Color == border.Color || borderRenderingOrder.IndexOf(previousBorder.Horizontal) > borderRenderingOrder.IndexOf(border));
		}
		if (borderRenderingOrder.Contains(borders.Bottom) && isMultiLineBottomBorder)
		{
			flag2 = border.BorderType == borders.Bottom.BorderType && rightBorderLineWidth == bottomBorderLineWidth;
			num2 = (flag2 ? 0f : (bottomBorderLineWidth * 2f));
		}
		if (isMultiLineRightBorder)
		{
			DrawMultiLineRightBorder(border, new PointF(bounds.Right - rightBorderLineWidth, bounds.Top + num), new PointF(bounds.Right - rightBorderLineWidth, bounds.Bottom - bottomBorderLineWidth * 2f), flag, isBetweenBorderSame, flag2);
		}
		else
		{
			DrawParagraphBorder(border, new PointF(bounds.Right, bounds.Top + num), new PointF(bounds.Right, bounds.Bottom - num2));
		}
	}

	private void DrawTopBorder(List<Border> borderRenderingOrder, RectangleF bounds, Borders borders, Border border, bool isMultiLineTopBorder, float topBorderLineWidth, float leftBorderLineWidth, float rightBorderLineWidth)
	{
		bool isLeftBorderSame = false;
		bool isRightBorderSame = false;
		if (borderRenderingOrder.Contains(borders.Left))
		{
			isLeftBorderSame = border.BorderType == borders.Left.BorderType && topBorderLineWidth == leftBorderLineWidth;
		}
		if (borderRenderingOrder.Contains(borders.Right))
		{
			isRightBorderSame = border.BorderType == borders.Right.BorderType && topBorderLineWidth == rightBorderLineWidth;
		}
		if (isMultiLineTopBorder)
		{
			DrawMultiLineTopBorder(border, new PointF(bounds.Left - leftBorderLineWidth, bounds.Top), new PointF(bounds.Right + rightBorderLineWidth, bounds.Top), isLeftBorderSame, isRightBorderSame);
		}
		else
		{
			DrawParagraphBorder(border, new PointF(bounds.Left - leftBorderLineWidth, bounds.Top + topBorderLineWidth), new PointF(bounds.Right + rightBorderLineWidth, bounds.Top + topBorderLineWidth));
		}
	}

	private void DrawBottomBorder(List<Border> borderRenderingOrder, RectangleF bounds, Borders borders, Border border, bool isMultiLineBottomBorder, float leftBorderLineWidth, float bottomBorderLineWidth, float rightBorderLineWidth, WParagraph paragraph, LayoutedWidget ltWidget)
	{
		bool isLeftBorderSame = false;
		bool isRightBorderSame = false;
		Entity ownerEntity = paragraph.GetOwnerEntity();
		if ((ownerEntity is WTextBox && paragraph.Index == (ownerEntity as WTextBox).TextBoxBody.ChildEntities.Count - 1) || (ownerEntity is Shape && paragraph.Index == (ownerEntity as Shape).TextBody.ChildEntities.Count - 1))
		{
			bounds.Y = ltWidget.Owner.Owner.Bounds.Bottom - bounds.Height - GetSpaceForBottomBorder(ownerEntity);
		}
		if (borderRenderingOrder.Contains(borders.Left))
		{
			isLeftBorderSame = border.BorderType == borders.Left.BorderType && leftBorderLineWidth == bottomBorderLineWidth;
		}
		if (borderRenderingOrder.Contains(borders.Right))
		{
			isRightBorderSame = border.BorderType == borders.Right.BorderType && rightBorderLineWidth == bottomBorderLineWidth;
		}
		if (isMultiLineBottomBorder)
		{
			DrawMultiLineBottomBorder(border, new PointF(bounds.Left - leftBorderLineWidth, bounds.Bottom - bottomBorderLineWidth * 2f), new PointF(bounds.Right + rightBorderLineWidth, bounds.Bottom - bottomBorderLineWidth * 2f), isLeftBorderSame, isRightBorderSame);
		}
		else
		{
			DrawParagraphBorder(border, new PointF(bounds.Left - leftBorderLineWidth, bounds.Bottom - bottomBorderLineWidth), new PointF(bounds.Right + rightBorderLineWidth, bounds.Bottom - bottomBorderLineWidth));
		}
	}

	private float GetSpaceForBottomBorder(Entity entity)
	{
		if (!(entity is WTextBox))
		{
			if (!(entity is Shape))
			{
				return 0f;
			}
			return (entity as Shape).LineFormat.Weight / 2f + (entity as Shape).TextFrame.InternalMargin.Bottom;
		}
		return (entity as WTextBox).TextBoxFormat.LineWidth / 2f + (entity as WTextBox).TextBoxFormat.InternalMargin.Bottom;
	}

	private void SortTwoBorders(List<Border> renderingOrderList, Border firstBorder, Border secondBorder, Borders borders, bool isLeftBorder)
	{
		if (!(firstBorder.Color == secondBorder.Color) && firstBorder.LineWidth == secondBorder.LineWidth)
		{
			if (borders != null)
			{
				renderingOrderList.Remove(isLeftBorder ? borders.Right : borders.Left);
			}
			renderingOrderList.Sort(new SortByColorBrightness());
			if (borders != null)
			{
				renderingOrderList.Add(isLeftBorder ? borders.Right : borders.Left);
			}
		}
	}

	private void AddNextParagraphBounds(LayoutedWidget layoutedWidget, ref RectangleF bounds)
	{
		while (!(layoutedWidget.Owner.Widget is WSection) && (!(layoutedWidget.Widget is SplitWidgetContainer) || !((layoutedWidget.Widget as SplitWidgetContainer).RealWidgetContainer is WSection)))
		{
			layoutedWidget = layoutedWidget.Owner;
		}
		for (int i = layoutedWidget.Owner.ChildWidgets.IndexOf(layoutedWidget) + 1; i < layoutedWidget.Owner.ChildWidgets.Count; i++)
		{
			bounds.Height += layoutedWidget.Owner.ChildWidgets[i].Bounds.Bottom - bounds.Bottom;
			if (!(layoutedWidget.Owner.ChildWidgets[i].Widget is WParagraph) || !(layoutedWidget.Owner.ChildWidgets[i].Widget as WParagraph).BreakCharacterFormat.Hidden)
			{
				break;
			}
		}
	}

	internal void DrawRevisionMark(PointF start, PointF end, Color lineColor, float lineWidth)
	{
		RectangleF clipBounds = PDFGraphics.ClipBounds;
		PDFGraphics.ResetClip();
		PdfPen pen = new PdfPen(new PdfColor(lineColor), lineWidth);
		PDFGraphics.DrawLine(pen, start, end);
		PDFGraphics.SetClip(clipBounds, CombineMode.Replace);
	}

	private void DrawBorder(Border border, PointF start, PointF end)
	{
		if (border.BorderType != BorderStyle.Cleared && ((border.BorderType == BorderStyle.None && !border.HasNoneStyle) || border.BorderType != 0))
		{
			PdfPen pen = GetPen(border, isParagraphBorder: false);
			PDFGraphics.DrawLine(pen, start, end);
		}
	}

	private void DrawParagraphBorder(Border border, PointF start, PointF end)
	{
		if (border.BorderType != BorderStyle.Cleared && ((border.BorderType == BorderStyle.None && !border.HasNoneStyle) || border.BorderType != 0))
		{
			PdfPen pen = GetPen(border, isParagraphBorder: true);
			PDFGraphics.DrawLine(pen, start, end);
		}
	}

	private void DrawBorder(CellLayoutInfo.CellBorder border, PointF start, PointF end)
	{
		PdfPen pen = GetPen(border.BorderType, border.RenderingLineWidth, border.BorderColor);
		PDFGraphics.DrawLine(pen, start, end);
	}

	internal virtual void DrawTable(WTable table, LayoutedWidget ltWidget)
	{
		if (table.TableFormat.CellSpacing > 0f)
		{
			RectangleF bounds = ltWidget.Bounds;
			Borders borders = table.TableFormat.Borders;
			CellLayoutInfo.CellBorder cellBorder = new CellLayoutInfo.CellBorder(borders.Left.BorderType, borders.Left.Color, borders.Left.GetLineWidthValue(), borders.Left.LineWidth);
			CellLayoutInfo.CellBorder cellBorder2 = new CellLayoutInfo.CellBorder(borders.Right.BorderType, borders.Right.Color, borders.Right.GetLineWidthValue(), borders.Right.LineWidth);
			CellLayoutInfo.CellBorder cellBorder3 = new CellLayoutInfo.CellBorder(borders.Top.BorderType, borders.Top.Color, borders.Top.GetLineWidthValue(), borders.Top.LineWidth);
			CellLayoutInfo.CellBorder cellBorder4 = new CellLayoutInfo.CellBorder(borders.Bottom.BorderType, borders.Bottom.Color, borders.Bottom.GetLineWidthValue(), borders.Bottom.LineWidth);
			if (borders.Left.IsBorderDefined && cellBorder.RenderingLineWidth != 0f)
			{
				bounds.X += cellBorder.RenderingLineWidth / 2f;
				bounds.Width -= cellBorder.RenderingLineWidth / 2f;
			}
			if (borders.Right.IsBorderDefined && cellBorder2.RenderingLineWidth != 0f)
			{
				bounds.Width -= cellBorder2.RenderingLineWidth / 2f;
			}
			if (borders.Top.IsBorderDefined && cellBorder3.RenderingLineWidth != 0f)
			{
				bounds.Y += cellBorder3.RenderingLineWidth / 2f;
				bounds.Height -= cellBorder3.RenderingLineWidth / 2f;
			}
			if (table.TableFormat.TextureStyle != 0)
			{
				DrawTextureStyle(table.TableFormat.TextureStyle, table.TableFormat.ForeColor, table.TableFormat.BackColor, ltWidget.Bounds);
			}
			else if (!table.TableFormat.BackColor.IsEmpty)
			{
				PDFGraphics.DrawRectangle(new PdfSolidBrush(table.TableFormat.BackColor), ltWidget.Bounds);
			}
			bounds = ltWidget.Bounds;
			if (borders.Left.IsBorderDefined && cellBorder.BorderType != BorderStyle.Cleared && cellBorder.BorderType != 0 && table.Rows[0] != null && ((IWidget)table.Rows[0]).LayoutInfo is RowLayoutInfo && ((IWidget)table.Rows[0]).LayoutInfo is RowLayoutInfo rowLayoutInfo)
			{
				float x = bounds.Left + rowLayoutInfo.Paddings.Left;
				DrawBorder(cellBorder, new PointF(x, bounds.Top), new PointF(x, bounds.Bottom));
			}
			if (borders.Right.IsBorderDefined && cellBorder2.BorderType != BorderStyle.Cleared && cellBorder2.BorderType != 0)
			{
				DrawBorder(cellBorder2, new PointF(bounds.Right, bounds.Top), new PointF(bounds.Right, bounds.Bottom));
			}
			if (borders.Top.IsBorderDefined && cellBorder3.BorderType != BorderStyle.Cleared && cellBorder3.BorderType != 0 && table.Rows[0] != null && ((IWidget)table.Rows[0]).LayoutInfo is RowLayoutInfo && ((IWidget)table.Rows[0]).LayoutInfo is RowLayoutInfo rowLayoutInfo2)
			{
				float x2 = bounds.Left + rowLayoutInfo2.Paddings.Left - cellBorder.RenderingLineWidth / 2f;
				float x3 = bounds.Right + cellBorder2.RenderingLineWidth / 2f;
				float y = bounds.Top + cellBorder3.RenderingLineWidth / 2f;
				DrawBorder(cellBorder3, new PointF(x2, y), new PointF(x3, y));
			}
			if (borders.Bottom.IsBorderDefined && cellBorder4.BorderType != BorderStyle.Cleared && cellBorder4.BorderType != 0 && table.Rows[0] != null && ((IWidget)table.Rows[0]).LayoutInfo is RowLayoutInfo && ((IWidget)table.Rows[0]).LayoutInfo is RowLayoutInfo rowLayoutInfo3)
			{
				float x4 = bounds.Left + rowLayoutInfo3.Paddings.Left - cellBorder.RenderingLineWidth / 2f;
				float x5 = bounds.Right + cellBorder2.RenderingLineWidth / 2f;
				float y2 = bounds.Bottom - cellBorder4.RenderingLineWidth / 2f;
				DrawBorder(cellBorder3, new PointF(x4, y2), new PointF(x5, y2));
			}
		}
		if (ltWidget.ChildWidgets.Count > 0)
		{
			ltWidget.ChildWidgets[0].Widget.LayoutInfo.IsFirstItemInPage = true;
			ltWidget.ChildWidgets[ltWidget.ChildWidgets.Count - 1].IsLastItemInPage = true;
			for (int i = 0; i < ltWidget.ChildWidgets[ltWidget.ChildWidgets.Count - 1].ChildWidgets.Count; i++)
			{
				ltWidget.ChildWidgets[ltWidget.ChildWidgets.Count - 1].ChildWidgets[i].IsLastItemInPage = true;
			}
		}
	}

	internal virtual void DrawTableRow(WTableRow row, LayoutedWidget ltWidget)
	{
	}

	internal virtual void DrawTableCell(WTableCell cell, LayoutedWidget ltWidget)
	{
		if (!(ltWidget.Widget.LayoutInfo is CellLayoutInfo { IsRowMergeContinue: false } cellLayoutInfo) || !(ltWidget.Owner.Widget is WTableRow))
		{
			return;
		}
		RectangleF bounds = ltWidget.Bounds;
		Entity entity = cell;
		while (entity is WTableCell)
		{
			entity = (entity as WTableCell).OwnerRow.OwnerTable.Owner;
		}
		bool flag = false;
		if (entity.Owner is WTextBox)
		{
			WTextBox wTextBox = entity.Owner as WTextBox;
			if (wTextBox.TextBoxFormat.Rotation != 0f)
			{
				flag = true;
				SetRotateTransform(m_rotateTransform, wTextBox.TextBoxFormat.Rotation, wTextBox.TextBoxFormat.FlipVertical, wTextBox.TextBoxFormat.FlipHorizontal);
			}
		}
		else if (entity.Owner is Shape)
		{
			Shape shape = entity.Owner as Shape;
			if (shape.Rotation != 0f)
			{
				flag = true;
				SetRotateTransform(m_rotateTransform, shape.Rotation, shape.FlipVertical, shape.FlipHorizontal);
			}
		}
		else if (entity.Owner is ChildShape)
		{
			ChildShape childShape = entity.Owner as ChildShape;
			RectangleF textLayoutingBounds = childShape.TextLayoutingBounds;
			if (childShape.AutoShapeType == DocGen.DocIO.DLS.AutoShapeType.Rectangle)
			{
				textLayoutingBounds.Height = GetLayoutedTextBoxContentHeight(ltWidget);
			}
			if (childShape.Rotation != 0f)
			{
				flag = true;
				Rotate(childShape, childShape.Rotation, childShape.FlipVertical, childShape.FlipHorizantal, textLayoutingBounds);
			}
		}
		if (cellLayoutInfo.Paddings.Left > 0f)
		{
			bounds.X += cellLayoutInfo.Paddings.Left;
			bounds.Width -= cellLayoutInfo.Paddings.Left;
		}
		if (cellLayoutInfo.Paddings.Right > 0f)
		{
			bounds.Width -= cellLayoutInfo.Paddings.Right;
		}
		if (cellLayoutInfo.Paddings.Top > 0f)
		{
			bool flag2 = ltWidget.Owner != null && ltWidget.Owner.Owner != null && ltWidget.Owner.Owner.ChildWidgets.IndexOf(ltWidget.Owner) == 0;
			bounds.Y += (flag2 ? cellLayoutInfo.TopPadding : cellLayoutInfo.UpdatedTopPadding);
			bounds.Height -= (flag2 ? cellLayoutInfo.TopPadding : cellLayoutInfo.UpdatedTopPadding);
		}
		bool flag3 = false;
		if ((cell.TextureStyle != 0 || !cell.CellFormat.BackColor.IsEmpty) && IsNeedToClip(bounds))
		{
			flag3 = true;
			RectangleF rectangleF = ClipBoundsContainer.Peek();
			if (cell.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013 && IsTableInTextBoxOrShape(cell, checkTextBoxOnly: false))
			{
				WTable ownerTable = cell.OwnerRow.OwnerTable;
				rectangleF = TextboxClipBounds(ownerTable, rectangleF);
			}
			SetClip(rectangleF);
		}
		if (cell.TextureStyle != 0)
		{
			DrawTextureStyle(cell.CellFormat.TextureStyle, cell.CellFormat.ForeColor, cell.CellFormat.BackColor, bounds);
		}
		else if (!cell.CellFormat.BackColor.IsEmpty && cell.Owner is WTableRow)
		{
			if (AutoTag)
			{
				PdfArtifact tag = new PdfArtifact();
				PDFGraphics.SetTag(tag);
			}
			if (ltWidget.Bounds.Height < cellLayoutInfo.UpdatedTopPadding && cell.GridSpan > 1 && (cell.Owner as WTableRow).HeightType == TableRowHeightType.Exactly)
			{
				FillCellColor(ltWidget);
			}
			else
			{
				PDFGraphics.DrawRectangle(GetPDFBrush(cell.CellFormat.BackColor), bounds);
			}
			if (AutoTag)
			{
				PDFGraphics.ReSetTag();
			}
		}
		if (flag)
		{
			ResetTransform();
		}
		if (flag3)
		{
			ResetClip();
		}
	}

	private void FillCellColor(LayoutedWidget ltWidget)
	{
		if (!(ltWidget.Owner.Widget is WTableRow))
		{
			return;
		}
		int rowIndex = (ltWidget.Owner.Widget as WTableRow).GetRowIndex();
		LayoutedWidgetList childWidgets = ltWidget.Owner.Owner.ChildWidgets[rowIndex - 1].ChildWidgets;
		float num = 0f;
		foreach (LayoutedWidget item in childWidgets)
		{
			RectangleF bounds = ltWidget.Bounds;
			if (!(item.Widget is WTableCell))
			{
				continue;
			}
			Border bottom = (item.Widget as WTableCell).CellFormat.Borders.Bottom;
			bounds.X += num;
			bounds.Width = item.Bounds.Width;
			float lineWidthValue = bottom.GetLineWidthValue();
			if (item.Bounds.X < ltWidget.Bounds.Right && item.Bounds.Right > ltWidget.Bounds.X)
			{
				if (lineWidthValue > 0f && lineWidthValue <= bounds.Height)
				{
					bounds.Y += lineWidthValue;
					bounds.Height -= lineWidthValue;
				}
				num += item.Bounds.Width;
				if (ltWidget.Widget is WTableCell)
				{
					PDFGraphics.DrawRectangle(GetPDFBrush((ltWidget.Widget as WTableCell).CellFormat.BackColor), bounds);
				}
			}
		}
	}

	internal bool IsTexBoxHaveBackgroundPicture(WTextBox textbox)
	{
		if (textbox.TextBoxFormat.FillEfects.Picture != null)
		{
			return true;
		}
		return false;
	}

	internal void DrawTextureStyle(TextureStyle textureStyle, Color foreColor, Color backColor, RectangleF bounds)
	{
		if (backColor.IsEmpty)
		{
			backColor = Color.White;
		}
		if (textureStyle.ToString().Contains("Percent"))
		{
			float percent = float.Parse(textureStyle.ToString().Replace("Texture", "").Replace("Percent", "")
				.Replace("Pt", "."), CultureInfo.InvariantCulture);
			Color foreColor2 = GetForeColor(foreColor, backColor, percent);
			PDFGraphics.DrawRectangle(GetPDFBrush(foreColor2), bounds);
		}
		FillTexture(textureStyle, foreColor, backColor, bounds);
	}

	private Color GetForeColor(Color foreColor, Color backColor, float percent)
	{
		int num = 0;
		int num2 = 0;
		int colorValue = GetColorValue(foreColor.R, backColor.R, percent, foreColor.IsEmpty, backColor.IsEmpty);
		num = GetColorValue(foreColor.G, backColor.G, percent, foreColor.IsEmpty, backColor.IsEmpty);
		num2 = GetColorValue(foreColor.B, backColor.B, percent, foreColor.IsEmpty, backColor.IsEmpty);
		foreColor = Color.FromArgb(colorValue, num, num2);
		return foreColor;
	}

	private int GetColorValue(int foreColorValue, int backColorValue, float percent, bool isForeColorEmpty, bool isBackColorEmpty)
	{
		int num = 0;
		if (percent == 100f)
		{
			return foreColorValue;
		}
		if (isForeColorEmpty)
		{
			if (isBackColorEmpty)
			{
				return (int)Math.Round(255f * (1f - percent / 100f));
			}
			return (int)Math.Round((float)backColorValue * (1f - percent / 100f));
		}
		if (isBackColorEmpty)
		{
			return (int)Math.Round((float)foreColorValue * (percent / 100f));
		}
		return backColorValue + (int)Math.Round((float)foreColorValue * (percent / 100f)) - (int)Math.Round((float)backColorValue * (percent / 100f));
	}

	private void FillTexture(TextureStyle textureStyle, Color foreColor, Color backColor, RectangleF bounds)
	{
		if (foreColor.IsEmpty)
		{
			foreColor = ((textureStyle != TextureStyle.TextureSolid) ? (WordColor.IsNotVeryDarkColor(backColor) ? Color.Black : Color.White) : (WordColor.IsNotVeryDarkColor(foreColor) ? Color.White : Color.Black));
		}
		switch (textureStyle)
		{
		case TextureStyle.TextureSolid:
			PDFGraphics.DrawRectangle(GetPDFBrush(foreColor), bounds);
			break;
		case TextureStyle.TextureCross:
			PDFGraphics.DrawRectangle(new PdfHatchBrush(PdfHatchStyle.Cross, foreColor, backColor), bounds);
			break;
		case TextureStyle.TextureDarkDiagonalCross:
			PDFGraphics.DrawRectangle(new PdfHatchBrush(PdfHatchStyle.DiagonalCross, foreColor, backColor), bounds);
			break;
		case TextureStyle.TextureDarkDiagonalDown:
			PDFGraphics.DrawRectangle(new PdfHatchBrush(PdfHatchStyle.DarkDownwardDiagonal, foreColor, backColor), bounds);
			break;
		case TextureStyle.TextureDarkDiagonalUp:
			PDFGraphics.DrawRectangle(new PdfHatchBrush(PdfHatchStyle.DarkUpwardDiagonal, foreColor, backColor), bounds);
			break;
		case TextureStyle.TextureDarkHorizontal:
			PDFGraphics.DrawRectangle(new PdfHatchBrush(PdfHatchStyle.Horizontal, foreColor, backColor), bounds);
			break;
		case TextureStyle.TextureDarkVertical:
			PDFGraphics.DrawRectangle(new PdfHatchBrush(PdfHatchStyle.DarkVertical, foreColor, backColor), bounds);
			break;
		case TextureStyle.TextureDiagonalCross:
			PDFGraphics.DrawRectangle(new PdfHatchBrush(PdfHatchStyle.DiagonalCross, foreColor, backColor), bounds);
			break;
		case TextureStyle.TextureDiagonalDown:
			PDFGraphics.DrawRectangle(new PdfHatchBrush(PdfHatchStyle.LightDownwardDiagonal, foreColor, backColor), bounds);
			break;
		case TextureStyle.TextureDiagonalUp:
			PDFGraphics.DrawRectangle(new PdfHatchBrush(PdfHatchStyle.LightUpwardDiagonal, foreColor, backColor), bounds);
			break;
		case TextureStyle.TextureHorizontal:
			PDFGraphics.DrawRectangle(new PdfHatchBrush(PdfHatchStyle.LightHorizontal, foreColor, backColor), bounds);
			break;
		case TextureStyle.TextureVertical:
			PDFGraphics.DrawRectangle(new PdfHatchBrush(PdfHatchStyle.LightVertical, foreColor, backColor), bounds);
			break;
		}
	}

	internal virtual void DrawCellBorders(WTableCell cell, LayoutedWidget ltWidget, float previousCellsTopBorderWidth)
	{
		CellLayoutInfo cellLayoutInfo = ltWidget.Widget.LayoutInfo as CellLayoutInfo;
		RectangleF rectangleF = ltWidget.Bounds;
		int num = ltWidget.Owner.Owner.ChildWidgets.IndexOf(ltWidget.Owner);
		LayoutedWidget layoutedWidget = ((num != 0) ? ltWidget.Owner.Owner.ChildWidgets[num - 1] : null);
		if (cellLayoutInfo != null && cellLayoutInfo.IsRowMergeStart && cellLayoutInfo.SkipBottomBorder)
		{
			rectangleF.Height = ltWidget.Owner.Bounds.Height;
		}
		bool flag = false;
		bool flag2 = false;
		if (cell.OwnerRow.RowFormat.CellSpacing > 0f || (cell.OwnerRow.OwnerTable.TableFormat.CellSpacing > 0f && cellLayoutInfo != null))
		{
			flag = true;
			if (!cell.OwnerRow.OwnerTable.TableFormat.Bidi && cellLayoutInfo != null)
			{
				rectangleF = new RectangleF(rectangleF.Left + ((cellLayoutInfo.LeftBorder != null) ? (cellLayoutInfo.LeftBorder.RenderingLineWidth / 2f) : 0f), rectangleF.Top, rectangleF.Width - (((cellLayoutInfo.LeftBorder != null) ? (cellLayoutInfo.LeftBorder.RenderingLineWidth / 2f) : 0f) + ((cellLayoutInfo.RightBorder != null) ? (cellLayoutInfo.RightBorder.RenderingLineWidth / 2f) : 0f)), rectangleF.Height);
			}
			else if (cell.OwnerRow.OwnerTable.TableFormat.Bidi && cellLayoutInfo != null)
			{
				rectangleF = new RectangleF(rectangleF.Left + ((cellLayoutInfo.RightBorder != null) ? (cellLayoutInfo.RightBorder.RenderingLineWidth / 2f) : 0f), rectangleF.Top, rectangleF.Width - (((cellLayoutInfo.RightBorder != null) ? (cellLayoutInfo.RightBorder.RenderingLineWidth / 2f) : 0f) + ((cellLayoutInfo.LeftBorder != null) ? (cellLayoutInfo.LeftBorder.RenderingLineWidth / 2f) : 0f)), rectangleF.Height);
			}
		}
		LayoutedWidget owner = ltWidget.Owner.Owner;
		bool isBiDiTable = false;
		if (owner.Widget is WTable && (owner.Widget as WTable).TableFormat.Bidi)
		{
			isBiDiTable = true;
		}
		int num2 = owner.ChildWidgets.IndexOf(ltWidget.Owner);
		bool flag3 = num2 == 0;
		bool flag4 = num2 == owner.ChildWidgets.Count - 1;
		bool flag5 = ltWidget.Owner.ChildWidgets.IndexOf(ltWidget) == ltWidget.Owner.ChildWidgets.Count - 1;
		bool flag6 = ltWidget.Owner.ChildWidgets.IndexOf(ltWidget) == 0;
		RowLayoutInfo rowLayoutInfo = ((IWidget)cell.OwnerRow).LayoutInfo as RowLayoutInfo;
		if (!flag3 && owner.ChildWidgets[num2 - 1].Widget is WTableRow && (owner.ChildWidgets[num2 - 1].Widget as WTableRow).IsHeader && cellLayoutInfo != null && cellLayoutInfo.UpdatedSplittedTopBorders != null)
		{
			cellLayoutInfo.UpdatedTopBorders.Clear();
			foreach (CellLayoutInfo.CellBorder key in cellLayoutInfo.UpdatedSplittedTopBorders.Keys)
			{
				cellLayoutInfo.UpdatedTopBorders.Add(key, cellLayoutInfo.UpdatedSplittedTopBorders[key]);
			}
		}
		if (AutoTag)
		{
			PdfArtifact tag = new PdfArtifact();
			PDFGraphics.SetTag(tag);
		}
		if (!(owner.Widget as WTable).TableFormat.Bidi && !cellLayoutInfo.SkipLeftBorder && cellLayoutInfo.LeftBorder != null)
		{
			if (IsMultiLineBorder(cellLayoutInfo.LeftBorder.BorderType))
			{
				DrawMultiLineLeftBorder(cellLayoutInfo, cellLayoutInfo.LeftBorder, new PointF(rectangleF.Left - cellLayoutInfo.LeftBorder.RenderingLineWidth / 2f, rectangleF.Top), new PointF(rectangleF.Left - cellLayoutInfo.LeftBorder.RenderingLineWidth / 2f, rectangleF.Bottom), flag3, flag4 || (flag && !cellLayoutInfo.IsRowMergeContinue), flag6, flag5);
			}
			else
			{
				CellLayoutInfo.CellBorder cellBorder = (flag3 ? cellLayoutInfo.TopBorder : ((cellLayoutInfo.UpdatedTopBorders.Count > 0) ? new List<CellLayoutInfo.CellBorder>(cellLayoutInfo.UpdatedTopBorders.Keys)[0] : null));
				float num3 = ((cellBorder != null && IsMultiLineBorder(cellBorder.BorderType) && !flag6) ? cellBorder.RenderingLineWidth : 0f);
				DrawBorder(cellLayoutInfo.LeftBorder, new PointF(rectangleF.Left, rectangleF.Top + num3), new PointF(rectangleF.Left, rectangleF.Bottom));
			}
		}
		else if (!cellLayoutInfo.SkipLeftBorder && cellLayoutInfo.LeftBorder != null && (owner.Widget as WTable).TableFormat.Bidi)
		{
			if (IsMultiLineBorder(cellLayoutInfo.LeftBorder.BorderType))
			{
				DrawMultiLineRightBorder(cellLayoutInfo, cellLayoutInfo.LeftBorder, new PointF(rectangleF.Right - cellLayoutInfo.LeftBorder.RenderingLineWidth / 2f, rectangleF.Top), new PointF(rectangleF.Right - cellLayoutInfo.LeftBorder.RenderingLineWidth / 2f, rectangleF.Bottom), flag3, flag4 || (flag && !cellLayoutInfo.IsRowMergeContinue), flag6, flag5);
			}
			else
			{
				CellLayoutInfo.CellBorder cellBorder2 = (flag3 ? cellLayoutInfo.TopBorder : ((cellLayoutInfo.UpdatedTopBorders.Count > 0) ? new List<CellLayoutInfo.CellBorder>(cellLayoutInfo.UpdatedTopBorders.Keys)[0] : null));
				float num4 = ((cellBorder2 != null && IsMultiLineBorder(cellBorder2.BorderType) && !flag6) ? cellBorder2.RenderingLineWidth : 0f);
				DrawBorder(cellLayoutInfo.LeftBorder, new PointF(rectangleF.Right, rectangleF.Top + num4), new PointF(rectangleF.Right, rectangleF.Bottom));
			}
		}
		if (!(owner.Widget as WTable).TableFormat.Bidi && !cellLayoutInfo.SkipRightBorder && cellLayoutInfo.RightBorder != null)
		{
			if (IsMultiLineBorder(cellLayoutInfo.RightBorder.BorderType))
			{
				DrawMultiLineRightBorder(cellLayoutInfo, cellLayoutInfo.RightBorder, new PointF(rectangleF.Right - cellLayoutInfo.RightBorder.RenderingLineWidth / 2f, rectangleF.Top), new PointF(rectangleF.Right - cellLayoutInfo.RightBorder.RenderingLineWidth / 2f, rectangleF.Bottom), flag3, flag4 || (flag && !cellLayoutInfo.IsRowMergeContinue), flag6, flag5);
			}
			else
			{
				CellLayoutInfo.CellBorder cellBorder3 = (flag3 ? cellLayoutInfo.TopBorder : ((cellLayoutInfo.UpdatedTopBorders.Count > 0) ? new List<CellLayoutInfo.CellBorder>(cellLayoutInfo.UpdatedTopBorders.Keys)[cellLayoutInfo.UpdatedTopBorders.Count - 1] : null));
				float num5 = ((cellBorder3 != null && IsMultiLineBorder(cellBorder3.BorderType) && !flag5) ? cellBorder3.RenderingLineWidth : 0f);
				DrawBorder(cellLayoutInfo.RightBorder, new PointF(rectangleF.Right, rectangleF.Top + num5), new PointF(rectangleF.Right, rectangleF.Bottom));
			}
		}
		else if (!cellLayoutInfo.SkipRightBorder && cellLayoutInfo.RightBorder != null && (owner.Widget as WTable).TableFormat.Bidi)
		{
			if (IsMultiLineBorder(cellLayoutInfo.RightBorder.BorderType))
			{
				DrawMultiLineLeftBorder(cellLayoutInfo, cellLayoutInfo.RightBorder, new PointF(rectangleF.Left - cellLayoutInfo.RightBorder.RenderingLineWidth / 2f, rectangleF.Top), new PointF(rectangleF.Left - cellLayoutInfo.RightBorder.RenderingLineWidth / 2f, rectangleF.Bottom), flag3, flag4 || (flag && !cellLayoutInfo.IsRowMergeContinue), flag6, flag5);
			}
			else
			{
				CellLayoutInfo.CellBorder cellBorder4 = (flag3 ? cellLayoutInfo.TopBorder : ((cellLayoutInfo.UpdatedTopBorders.Count > 0) ? new List<CellLayoutInfo.CellBorder>(cellLayoutInfo.UpdatedTopBorders.Keys)[cellLayoutInfo.UpdatedTopBorders.Count - 1] : null));
				float num6 = ((cellBorder4 != null && IsMultiLineBorder(cellBorder4.BorderType) && !flag5) ? cellBorder4.RenderingLineWidth : 0f);
				DrawBorder(cellLayoutInfo.RightBorder, new PointF(rectangleF.Left, rectangleF.Top + num6), new PointF(rectangleF.Left, rectangleF.Bottom));
			}
		}
		if (!cellLayoutInfo.SkipTopBorder && (!cellLayoutInfo.IsRowMergeContinue || (flag5 && cellLayoutInfo.UpdatedTopBorders.Count > 1)) && !cellLayoutInfo.IsColumnMergeContinue)
		{
			if (cellLayoutInfo.UpdatedTopBorders.Count > 0 && !flag3)
			{
				float num7 = 0f;
				List<CellLayoutInfo.CellBorder> list = new List<CellLayoutInfo.CellBorder>(cellLayoutInfo.UpdatedTopBorders.Keys);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] != null && list[i].BorderType != BorderStyle.Cleared && list[i].BorderType != 0)
					{
						if (IsMultiLineBorder(list[i].BorderType))
						{
							float num8 = 0f;
							float num9 = 0f;
							if (!(owner.Widget as WTable).TableFormat.Bidi)
							{
								num8 = ((list[i].AdjCellLeftBorder != null) ? (list[i].AdjCellLeftBorder.RenderingLineWidth / 2f) : 0f);
								num8 = ((i == 0 && cellLayoutInfo.LeftBorder != null && cellLayoutInfo.LeftBorder.RenderingLineWidth / 2f > num8) ? (cellLayoutInfo.LeftBorder.RenderingLineWidth / 2f) : num8);
								num9 = ((list[i].AdjCellRightBorder != null) ? (list[i].AdjCellRightBorder.RenderingLineWidth / 2f) : 0f);
								num9 = ((i == list.Count - 1 && cellLayoutInfo.RightBorder != null && cellLayoutInfo.RightBorder.RenderingLineWidth / 2f > num9) ? (cellLayoutInfo.RightBorder.RenderingLineWidth / 2f) : num9);
							}
							else if ((owner.Widget as WTable).TableFormat.Bidi)
							{
								num8 = ((list[i].AdjCellRightBorder != null) ? (list[i].AdjCellRightBorder.RenderingLineWidth / 2f) : 0f);
								num8 = ((i == 0 && cellLayoutInfo.RightBorder != null && cellLayoutInfo.RightBorder.RenderingLineWidth / 2f > num8) ? (cellLayoutInfo.RightBorder.RenderingLineWidth / 2f) : num8);
								num9 = ((list[i].AdjCellLeftBorder != null) ? (list[i].AdjCellLeftBorder.RenderingLineWidth / 2f) : 0f);
								num9 = ((i == list.Count - 1 && cellLayoutInfo.LeftBorder != null && cellLayoutInfo.LeftBorder.RenderingLineWidth / 2f > num9) ? (cellLayoutInfo.LeftBorder.RenderingLineWidth / 2f) : num9);
							}
							DrawMultiLineTopBorder(cellLayoutInfo, list[i], new PointF(rectangleF.Left - num8 + num7, rectangleF.Top), new PointF(rectangleF.Left + num7 + cellLayoutInfo.UpdatedTopBorders[list[i]] + num9, rectangleF.Top), i == 0, i == list.Count - 1);
						}
						else
						{
							RectangleF bounds = layoutedWidget.Bounds;
							float num10 = 0f;
							float num11 = 0f;
							int num12 = -1;
							for (int j = 0; j < layoutedWidget.ChildWidgets.Count; j++)
							{
								if (!(layoutedWidget.ChildWidgets[j].Widget.LayoutInfo as CellLayoutInfo).SkipBottomBorder || layoutedWidget.ChildWidgets[j].Bounds.X > ltWidget.Owner.Bounds.X)
								{
									num12 = j;
									break;
								}
							}
							if (!(owner.Widget as WTable).TableFormat.Bidi)
							{
								num10 = ((list[i].AdjCellLeftBorder == null) ? 0f : (IsMultiLineBorder(list[i].AdjCellLeftBorder.BorderType) ? (0f - list[i].AdjCellLeftBorder.RenderingLineWidth / 2f) : (list[i].AdjCellLeftBorder.RenderingLineWidth / 2f)));
								num10 = ((i != 0 || cellLayoutInfo.LeftBorder == null || cellLayoutInfo.SkipLeftBorder || !(cellLayoutInfo.LeftBorder.RenderingLineWidth / 2f > num10)) ? num10 : (IsMultiLineBorder(cellLayoutInfo.LeftBorder.BorderType) ? (0f - cellLayoutInfo.LeftBorder.RenderingLineWidth / 2f) : (cellLayoutInfo.LeftBorder.RenderingLineWidth / 2f)));
								num11 = ((list[i].AdjCellRightBorder == null) ? 0f : (IsMultiLineBorder(list[i].AdjCellRightBorder.BorderType) ? (0f - list[i].AdjCellRightBorder.RenderingLineWidth / 2f) : (list[i].AdjCellRightBorder.RenderingLineWidth / 2f)));
								num11 = ((i != list.Count - 1 || cellLayoutInfo.RightBorder == null || cellLayoutInfo.SkipRightBorder || !(cellLayoutInfo.RightBorder.RenderingLineWidth / 2f > num11)) ? num11 : (IsMultiLineBorder(cellLayoutInfo.RightBorder.BorderType) ? (0f - cellLayoutInfo.RightBorder.RenderingLineWidth / 2f) : (cellLayoutInfo.RightBorder.RenderingLineWidth / 2f)));
							}
							else if ((owner.Widget as WTable).TableFormat.Bidi)
							{
								num10 = ((list[i].AdjCellRightBorder == null) ? 0f : (IsMultiLineBorder(list[i].AdjCellRightBorder.BorderType) ? (0f - list[i].AdjCellRightBorder.RenderingLineWidth / 2f) : (list[i].AdjCellRightBorder.RenderingLineWidth / 2f)));
								num10 = ((i != 0 || cellLayoutInfo.RightBorder == null || cellLayoutInfo.SkipRightBorder || !(cellLayoutInfo.RightBorder.RenderingLineWidth / 2f > num10)) ? num10 : (IsMultiLineBorder(cellLayoutInfo.RightBorder.BorderType) ? (0f - cellLayoutInfo.RightBorder.RenderingLineWidth / 2f) : (cellLayoutInfo.RightBorder.RenderingLineWidth / 2f)));
								num11 = ((list[i].AdjCellLeftBorder == null) ? 0f : (IsMultiLineBorder(list[i].AdjCellLeftBorder.BorderType) ? (0f - list[i].AdjCellLeftBorder.RenderingLineWidth / 2f) : (list[i].AdjCellLeftBorder.RenderingLineWidth / 2f)));
								num11 = ((i != list.Count - 1 || cellLayoutInfo.LeftBorder == null || cellLayoutInfo.SkipLeftBorder || !(cellLayoutInfo.LeftBorder.RenderingLineWidth / 2f > num11)) ? num11 : (IsMultiLineBorder(cellLayoutInfo.LeftBorder.BorderType) ? (0f - cellLayoutInfo.LeftBorder.RenderingLineWidth / 2f) : (cellLayoutInfo.LeftBorder.RenderingLineWidth / 2f)));
							}
							PointF start;
							PointF end;
							if (num12 != -1 && layoutedWidget.ChildWidgets[num12].Bounds.X < ltWidget.Owner.Bounds.X)
							{
								start = new PointF(bounds.Left + num7 + previousCellsTopBorderWidth - num10, rectangleF.Top + list[i].RenderingLineWidth / 2f);
								end = new PointF(bounds.Left + num7 + cellLayoutInfo.UpdatedTopBorders[list[i]] + previousCellsTopBorderWidth + num11, rectangleF.Top + list[i].RenderingLineWidth / 2f);
							}
							else
							{
								start = new PointF(rectangleF.Left + num7 - num10, rectangleF.Top + list[i].RenderingLineWidth / 2f);
								end = new PointF(rectangleF.Left + num7 + cellLayoutInfo.UpdatedTopBorders[list[i]] + num11, rectangleF.Top + list[i].RenderingLineWidth / 2f);
							}
							DrawBorder(list[i], start, end);
						}
					}
					num7 += cellLayoutInfo.UpdatedTopBorders[list[i]];
				}
			}
			else if (cellLayoutInfo.TopBorder != null && cellLayoutInfo.TopBorder.BorderType != BorderStyle.Cleared && cellLayoutInfo.TopBorder.BorderType != 0)
			{
				if (IsMultiLineBorder(cellLayoutInfo.TopBorder.BorderType))
				{
					if (!(owner.Widget as WTable).TableFormat.Bidi)
					{
						DrawMultiLineTopBorder(cellLayoutInfo, cellLayoutInfo.TopBorder, new PointF(rectangleF.Left - ((cellLayoutInfo.LeftBorder != null) ? (cellLayoutInfo.LeftBorder.RenderingLineWidth / 2f) : 0f), rectangleF.Top), new PointF(rectangleF.Right + ((cellLayoutInfo.RightBorder != null) ? (cellLayoutInfo.RightBorder.RenderingLineWidth / 2f) : 0f), rectangleF.Top), isStart: true, isEnd: true);
					}
					else if ((owner.Widget as WTable).TableFormat.Bidi)
					{
						DrawMultiLineTopBorder(cellLayoutInfo, cellLayoutInfo.TopBorder, new PointF(rectangleF.Left - ((cellLayoutInfo.RightBorder != null) ? (cellLayoutInfo.RightBorder.RenderingLineWidth / 2f) : 0f), rectangleF.Top), new PointF(rectangleF.Right + ((cellLayoutInfo.LeftBorder != null) ? (cellLayoutInfo.LeftBorder.RenderingLineWidth / 2f) : 0f), rectangleF.Top), isStart: true, isEnd: true);
					}
				}
				else
				{
					float num13 = 0f;
					float num14 = 0f;
					if (!(owner.Widget as WTable).TableFormat.Bidi)
					{
						num13 = ((cellLayoutInfo.LeftBorder != null && !cellLayoutInfo.SkipLeftBorder) ? (cellLayoutInfo.LeftBorder.RenderingLineWidth / 2f) : 0f);
						num14 = ((cellLayoutInfo.RightBorder != null && !cellLayoutInfo.SkipRightBorder) ? (cellLayoutInfo.RightBorder.RenderingLineWidth / 2f) : 0f);
					}
					else if ((owner.Widget as WTable).TableFormat.Bidi)
					{
						num13 = ((cellLayoutInfo.RightBorder != null && !cellLayoutInfo.SkipRightBorder) ? (cellLayoutInfo.RightBorder.RenderingLineWidth / 2f) : 0f);
						num14 = ((cellLayoutInfo.LeftBorder != null && !cellLayoutInfo.SkipLeftBorder) ? (cellLayoutInfo.LeftBorder.RenderingLineWidth / 2f) : 0f);
					}
					DrawBorder(start: new PointF(rectangleF.Left - num13, rectangleF.Top + cellLayoutInfo.TopBorder.RenderingLineWidth / 2f), end: new PointF(rectangleF.Right + num14, rectangleF.Top + cellLayoutInfo.TopBorder.RenderingLineWidth / 2f), border: cellLayoutInfo.TopBorder);
				}
			}
		}
		if (!cellLayoutInfo.SkipBottomBorder && (flag4 || (flag && !cellLayoutInfo.IsRowMergeContinue) || rowLayoutInfo.IsRowSplittedByFloatingItem) && cellLayoutInfo.BottomBorder != null)
		{
			if (IsMultiLineBorder(cellLayoutInfo.BottomBorder.BorderType))
			{
				if (!(owner.Widget as WTable).TableFormat.Bidi)
				{
					DrawMultiLineBottomBorder(cellLayoutInfo, new PointF(rectangleF.Left - ((cellLayoutInfo.LeftBorder != null) ? (cellLayoutInfo.LeftBorder.RenderingLineWidth / 2f) : 0f), rectangleF.Bottom), new PointF(rectangleF.Right - ((cellLayoutInfo.RightBorder != null) ? (cellLayoutInfo.RightBorder.RenderingLineWidth / 2f) : 0f), rectangleF.Bottom), isBiDiTable);
				}
				else if ((owner.Widget as WTable).TableFormat.Bidi)
				{
					DrawMultiLineBottomBorder(cellLayoutInfo, new PointF(rectangleF.Left - ((cellLayoutInfo.RightBorder != null) ? (cellLayoutInfo.RightBorder.RenderingLineWidth / 2f) : 0f), rectangleF.Bottom), new PointF(rectangleF.Right - ((cellLayoutInfo.LeftBorder != null) ? (cellLayoutInfo.LeftBorder.RenderingLineWidth / 2f) : 0f), rectangleF.Bottom), isBiDiTable);
				}
			}
			else
			{
				if (cell.NextSibling != null && ((cell.NextSibling as WTableCell).m_layoutInfo as CellLayoutInfo).SkipBottomBorder)
				{
					flag2 = true;
				}
				if (!(owner.Widget as WTable).TableFormat.Bidi)
				{
					DrawBorder(cellLayoutInfo.BottomBorder, new PointF(rectangleF.Left - ((cellLayoutInfo.LeftBorder != null) ? (cellLayoutInfo.LeftBorder.RenderingLineWidth / 2f) : 0f), rectangleF.Bottom + cellLayoutInfo.BottomPadding / 2f), new PointF(rectangleF.Right + (((flag5 || flag || flag2) && cellLayoutInfo.RightBorder != null) ? (cellLayoutInfo.RightBorder.RenderingLineWidth / 2f) : 0f), rectangleF.Bottom + cellLayoutInfo.BottomPadding / 2f));
				}
				else if ((owner.Widget as WTable).TableFormat.Bidi)
				{
					DrawBorder(cellLayoutInfo.BottomBorder, new PointF(rectangleF.Left - ((cellLayoutInfo.RightBorder != null) ? (cellLayoutInfo.RightBorder.RenderingLineWidth / 2f) : 0f), rectangleF.Bottom + cellLayoutInfo.BottomPadding / 2f), new PointF(rectangleF.Right + (((flag5 || flag || flag2) && cellLayoutInfo.LeftBorder != null) ? (cellLayoutInfo.LeftBorder.RenderingLineWidth / 2f) : 0f), rectangleF.Bottom + cellLayoutInfo.BottomPadding / 2f));
				}
			}
		}
		Border diagonalDown = cell.CellFormat.Borders.DiagonalDown;
		if ((diagonalDown.IsBorderDefined && diagonalDown.BorderType != BorderStyle.Cleared) || diagonalDown.BorderType != 0)
		{
			DrawBorder(diagonalDown, new PointF(rectangleF.Left, rectangleF.Top), new PointF(rectangleF.Right, rectangleF.Bottom));
		}
		Border diagonalUp = cell.CellFormat.Borders.DiagonalUp;
		if ((diagonalUp.IsBorderDefined && diagonalUp.BorderType != BorderStyle.Cleared) || diagonalUp.BorderType != 0)
		{
			DrawBorder(diagonalUp, new PointF(rectangleF.Left, rectangleF.Bottom), new PointF(rectangleF.Right, rectangleF.Top));
		}
		if (AutoTag)
		{
			PDFGraphics.ReSetTag();
		}
	}

	private void DrawMultiLineLeftBorder(CellLayoutInfo cellLayoutInfo, CellLayoutInfo.CellBorder leftBorder, PointF start, PointF end, bool isFirstRow, bool isLastRow, bool isFirstCell, bool isLastCell)
	{
		switch (leftBorder.BorderType)
		{
		case BorderStyle.Double:
		case BorderStyle.ThinThickSmallGap:
		case BorderStyle.ThinThinSmallGap:
		case BorderStyle.ThinThickMediumGap:
		case BorderStyle.ThickThinMediumGap:
		case BorderStyle.ThinThickLargeGap:
		case BorderStyle.ThickThinLargeGap:
			DrawDoubleLineLeftBorder(cellLayoutInfo, leftBorder, start, end, isFirstRow, isLastRow, isFirstCell, isLastCell);
			break;
		}
	}

	private void DrawDoubleLineLeftBorder(CellLayoutInfo cellLayoutInfo, CellLayoutInfo.CellBorder leftBorder, PointF start, PointF end, bool isFirstRow, bool isLastRow, bool isFirstCell, bool isLastCell)
	{
		float[] borderLineWidthArray = GetBorderLineWidthArray(leftBorder.BorderType, leftBorder.BorderLineWidth);
		PdfPen pen = GetPen(leftBorder.BorderType, borderLineWidthArray[0], leftBorder.BorderColor);
		PointF point = new PointF(start.X + borderLineWidthArray[0] / 2f, start.Y);
		PointF point2 = new PointF(end.X + borderLineWidthArray[0] / 2f, end.Y);
		if (cellLayoutInfo.PrevCellTopBorder != null && cellLayoutInfo.PrevCellTopBorder.BorderType == leftBorder.BorderType && cellLayoutInfo.PrevCellTopBorder.RenderingLineWidth == leftBorder.RenderingLineWidth)
		{
			point = new PointF(point.X, point.Y + borderLineWidthArray[0] + borderLineWidthArray[1]);
		}
		if ((isLastRow && cellLayoutInfo.PrevCellBottomBorder != null && cellLayoutInfo.PrevCellBottomBorder.BorderType != leftBorder.BorderType && cellLayoutInfo.PrevCellBottomBorder.RenderingLineWidth != leftBorder.RenderingLineWidth && cellLayoutInfo.BottomBorder != null && cellLayoutInfo.BottomBorder.BorderType == leftBorder.BorderType && cellLayoutInfo.BottomBorder.RenderingLineWidth == leftBorder.RenderingLineWidth) || (isFirstCell && cellLayoutInfo.BottomBorder != null && cellLayoutInfo.BottomBorder.BorderType == leftBorder.BorderType && cellLayoutInfo.BottomBorder.RenderingLineWidth == leftBorder.RenderingLineWidth))
		{
			point2 = new PointF(point2.X, point2.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2]);
		}
		PDFGraphics.DrawLine(pen, point, point2);
		PointF point3 = new PointF(start.X + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f, start.Y);
		PointF point4 = new PointF(end.X + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f, end.Y);
		PdfPen pen2 = GetPen(leftBorder.BorderType, borderLineWidthArray[2], leftBorder.BorderColor);
		CellLayoutInfo.CellBorder cellBorder = (isFirstRow ? cellLayoutInfo.TopBorder : ((cellLayoutInfo.UpdatedTopBorders.Count > 0) ? new List<CellLayoutInfo.CellBorder>(cellLayoutInfo.UpdatedTopBorders.Keys)[0] : null));
		if (cellBorder != null && cellBorder.BorderType == leftBorder.BorderType && cellBorder.RenderingLineWidth == leftBorder.RenderingLineWidth)
		{
			point3 = new PointF(point3.X, point3.Y + borderLineWidthArray[0] + borderLineWidthArray[1]);
		}
		PDFGraphics.DrawLine(pen2, point3, point4);
	}

	private void DrawMultiLineRightBorder(CellLayoutInfo cellLayoutInfo, CellLayoutInfo.CellBorder rightBorder, PointF start, PointF end, bool isFirstRow, bool isLastRow, bool isFirstCell, bool isLastCell)
	{
		switch (rightBorder.BorderType)
		{
		case BorderStyle.Double:
		case BorderStyle.ThinThickSmallGap:
		case BorderStyle.ThinThinSmallGap:
		case BorderStyle.ThinThickMediumGap:
		case BorderStyle.ThickThinMediumGap:
		case BorderStyle.ThinThickLargeGap:
		case BorderStyle.ThickThinLargeGap:
			DrawDoubleLineRightBorder(cellLayoutInfo, rightBorder, start, end, isFirstRow, isLastRow, isFirstCell, isLastCell);
			break;
		}
	}

	private void DrawDoubleLineRightBorder(CellLayoutInfo cellLayoutInfo, CellLayoutInfo.CellBorder rightBorder, PointF start, PointF end, bool isFirstRow, bool isLastRow, bool isFirstCell, bool isLastCell)
	{
		float[] borderLineWidthArray = GetBorderLineWidthArray(rightBorder.BorderType, rightBorder.BorderLineWidth);
		PdfPen pen = GetPen(rightBorder.BorderType, borderLineWidthArray[0], rightBorder.BorderColor);
		CellLayoutInfo.CellBorder cellBorder = (isFirstRow ? cellLayoutInfo.TopBorder : ((cellLayoutInfo.UpdatedTopBorders.Count > 0) ? new List<CellLayoutInfo.CellBorder>(cellLayoutInfo.UpdatedTopBorders.Keys)[cellLayoutInfo.UpdatedTopBorders.Count - 1] : null));
		PointF point = new PointF(start.X + borderLineWidthArray[0] / 2f, start.Y);
		PointF point2 = new PointF(end.X + borderLineWidthArray[0] / 2f, end.Y);
		if (cellBorder != null && cellBorder.BorderType == rightBorder.BorderType && cellBorder.RenderingLineWidth == rightBorder.RenderingLineWidth)
		{
			point = new PointF(point.X, point.Y + borderLineWidthArray[0] + borderLineWidthArray[1]);
		}
		PDFGraphics.DrawLine(pen, point, point2);
		PointF point3 = new PointF(start.X + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f, start.Y);
		PointF point4 = new PointF(end.X + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f, end.Y);
		PdfPen pen2 = GetPen(rightBorder.BorderType, borderLineWidthArray[2], rightBorder.BorderColor);
		if (cellLayoutInfo.NextCellTopBorder != null && cellLayoutInfo.NextCellTopBorder.BorderType == rightBorder.BorderType && cellLayoutInfo.NextCellTopBorder.RenderingLineWidth == rightBorder.RenderingLineWidth)
		{
			point3 = new PointF(point3.X, point3.Y + borderLineWidthArray[0] + borderLineWidthArray[1]);
		}
		if ((isLastRow && cellLayoutInfo.NextCellBottomBorder != null && cellLayoutInfo.NextCellBottomBorder.BorderType != rightBorder.BorderType && cellLayoutInfo.NextCellBottomBorder.RenderingLineWidth != rightBorder.RenderingLineWidth && cellLayoutInfo.BottomBorder != null && cellLayoutInfo.BottomBorder.BorderType == rightBorder.BorderType && cellLayoutInfo.BottomBorder.RenderingLineWidth == rightBorder.RenderingLineWidth) || (isLastCell && cellLayoutInfo.BottomBorder != null && cellLayoutInfo.BottomBorder.BorderType == rightBorder.BorderType && cellLayoutInfo.BottomBorder.RenderingLineWidth == rightBorder.RenderingLineWidth))
		{
			point4 = new PointF(point4.X, point4.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2]);
		}
		PDFGraphics.DrawLine(pen2, point3, point4);
	}

	private void DrawMultiLineBottomBorder(CellLayoutInfo cellLayoutInfo, PointF start, PointF end, bool isBiDiTable)
	{
		switch (cellLayoutInfo.BottomBorder.BorderType)
		{
		case BorderStyle.Double:
		case BorderStyle.ThinThickSmallGap:
		case BorderStyle.ThinThinSmallGap:
		case BorderStyle.ThinThickMediumGap:
		case BorderStyle.ThickThinMediumGap:
		case BorderStyle.ThinThickLargeGap:
		case BorderStyle.ThickThinLargeGap:
			DrawDoubleLineBottomBorder(cellLayoutInfo, start, end, isBiDiTable);
			break;
		}
	}

	private void DrawDoubleLineBottomBorder(CellLayoutInfo cellLayoutInfo, PointF start, PointF end, bool isBiDiTable)
	{
		float[] borderLineWidthArray = GetBorderLineWidthArray(cellLayoutInfo.BottomBorder.BorderType, cellLayoutInfo.BottomBorder.BorderLineWidth);
		PdfPen pen = GetPen(cellLayoutInfo.BottomBorder.BorderType, borderLineWidthArray[0], cellLayoutInfo.BottomBorder.BorderColor);
		PointF point = new PointF(start.X, start.Y + borderLineWidthArray[0] / 2f);
		PointF point2 = new PointF(end.X, end.Y + borderLineWidthArray[0] / 2f);
		if (!isBiDiTable && cellLayoutInfo.LeftBorder != null && cellLayoutInfo.LeftBorder.BorderType == cellLayoutInfo.BottomBorder.BorderType && cellLayoutInfo.LeftBorder.RenderingLineWidth == cellLayoutInfo.BottomBorder.RenderingLineWidth)
		{
			point = new PointF(point.X + borderLineWidthArray[0] + borderLineWidthArray[1], point.Y);
		}
		else if (isBiDiTable && cellLayoutInfo.RightBorder != null && cellLayoutInfo.RightBorder.BorderType == cellLayoutInfo.BottomBorder.BorderType && cellLayoutInfo.RightBorder.RenderingLineWidth == cellLayoutInfo.BottomBorder.RenderingLineWidth)
		{
			point = new PointF(point.X + borderLineWidthArray[0] + borderLineWidthArray[1], point.Y);
		}
		if (isBiDiTable && cellLayoutInfo.LeftBorder != null && cellLayoutInfo.LeftBorder.BorderType == cellLayoutInfo.BottomBorder.BorderType && cellLayoutInfo.LeftBorder.RenderingLineWidth == cellLayoutInfo.BottomBorder.RenderingLineWidth)
		{
			point2 = new PointF(point2.X + borderLineWidthArray[0], point2.Y);
		}
		else if (!isBiDiTable && cellLayoutInfo.RightBorder != null && cellLayoutInfo.RightBorder.BorderType == cellLayoutInfo.BottomBorder.BorderType && cellLayoutInfo.RightBorder.RenderingLineWidth == cellLayoutInfo.BottomBorder.RenderingLineWidth)
		{
			point2 = new PointF(point2.X + borderLineWidthArray[0], point2.Y);
		}
		PDFGraphics.DrawLine(pen, point, point2);
		PdfPen pen2 = GetPen(cellLayoutInfo.BottomBorder.BorderType, borderLineWidthArray[2], cellLayoutInfo.BottomBorder.BorderColor);
		PointF point3 = new PointF(start.X, start.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f);
		PointF point4 = new PointF(end.X, start.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f);
		if (!isBiDiTable && cellLayoutInfo.RightBorder != null && cellLayoutInfo.RightBorder.BorderType == cellLayoutInfo.BottomBorder.BorderType && cellLayoutInfo.RightBorder.RenderingLineWidth == cellLayoutInfo.BottomBorder.RenderingLineWidth)
		{
			point4 = new PointF(point4.X + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2], point4.Y);
		}
		else if (isBiDiTable && cellLayoutInfo.RightBorder != null && cellLayoutInfo.LeftBorder != null && cellLayoutInfo.LeftBorder.BorderType == cellLayoutInfo.BottomBorder.BorderType && cellLayoutInfo.LeftBorder.RenderingLineWidth == cellLayoutInfo.BottomBorder.RenderingLineWidth)
		{
			point4 = new PointF(point4.X + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2], point4.Y);
		}
		PDFGraphics.DrawLine(pen2, point3, point4);
	}

	private void DrawMultiLineTopBorder(CellLayoutInfo cellLayoutInfo, CellLayoutInfo.CellBorder topBorder, PointF start, PointF end, bool isStart, bool isEnd)
	{
		switch (topBorder.BorderType)
		{
		case BorderStyle.Double:
		case BorderStyle.ThinThickSmallGap:
		case BorderStyle.ThinThinSmallGap:
		case BorderStyle.ThinThickMediumGap:
		case BorderStyle.ThickThinMediumGap:
		case BorderStyle.ThinThickLargeGap:
		case BorderStyle.ThickThinLargeGap:
			DrawDoubleLineTopBorder(cellLayoutInfo, topBorder, start, end, isStart, isEnd);
			break;
		}
	}

	private void DrawDoubleLineTopBorder(CellLayoutInfo cellLayoutInfo, CellLayoutInfo.CellBorder topBorder, PointF start, PointF end, bool isStart, bool isEnd)
	{
		float[] borderLineWidthArray = GetBorderLineWidthArray(topBorder.BorderType, topBorder.BorderLineWidth);
		PdfPen pen = GetPen(topBorder.BorderType, borderLineWidthArray[0], topBorder.BorderColor);
		PointF point = new PointF(start.X, start.Y + borderLineWidthArray[0] / 2f);
		PointF point2 = new PointF(end.X, end.Y + borderLineWidthArray[0] / 2f);
		if (topBorder.AdjCellLeftBorder != null && topBorder.AdjCellLeftBorder.BorderType == topBorder.BorderType && topBorder.AdjCellLeftBorder.RenderingLineWidth == topBorder.RenderingLineWidth)
		{
			point = new PointF(point.X + borderLineWidthArray[0] + borderLineWidthArray[1], point.Y);
		}
		if (topBorder.AdjCellRightBorder != null && topBorder.AdjCellRightBorder.BorderType == topBorder.BorderType && topBorder.AdjCellRightBorder.RenderingLineWidth == topBorder.RenderingLineWidth)
		{
			point2 = new PointF(point2.X - (borderLineWidthArray[1] + borderLineWidthArray[2]), point2.Y);
		}
		PDFGraphics.DrawLine(pen, point, point2);
		PdfPen pen2 = GetPen(topBorder.BorderType, borderLineWidthArray[2], topBorder.BorderColor);
		PointF point3 = new PointF(start.X, start.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f);
		PointF point4 = new PointF(end.X, end.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f);
		if (isStart && cellLayoutInfo.LeftBorder != null && cellLayoutInfo.LeftBorder.BorderType == topBorder.BorderType && cellLayoutInfo.LeftBorder.RenderingLineWidth == topBorder.RenderingLineWidth)
		{
			point3 = new PointF(point3.X + borderLineWidthArray[0] + borderLineWidthArray[1], point3.Y);
		}
		if (isEnd && cellLayoutInfo.RightBorder != null && cellLayoutInfo.RightBorder.BorderType == topBorder.BorderType && cellLayoutInfo.RightBorder.RenderingLineWidth == topBorder.RenderingLineWidth)
		{
			point4 = new PointF(point4.X - (borderLineWidthArray[1] + borderLineWidthArray[2]), point4.Y);
		}
		PDFGraphics.DrawLine(pen2, point3, point4);
	}

	private bool IsMultiLineBorder(BorderStyle borderType)
	{
		switch (borderType)
		{
		case BorderStyle.Double:
		case BorderStyle.ThinThickSmallGap:
		case BorderStyle.ThinThinSmallGap:
		case BorderStyle.ThinThickMediumGap:
		case BorderStyle.ThickThinMediumGap:
		case BorderStyle.ThinThickLargeGap:
		case BorderStyle.ThickThinLargeGap:
			return true;
		default:
			return false;
		}
	}

	private bool IsMultiLineParagraphBorder(BorderStyle borderType)
	{
		if (borderType == BorderStyle.Double || (uint)(borderType - 10) <= 9u)
		{
			return true;
		}
		return false;
	}

	private float[] GetBorderLineWidthArray(BorderStyle borderType, float lineWidth)
	{
		float[] array = new float[1] { lineWidth };
		switch (borderType)
		{
		case BorderStyle.Double:
			array = new float[3] { 1f, 1f, 1f };
			break;
		case BorderStyle.ThinThickSmallGap:
			array = new float[3] { 1f, -0.75f, -0.75f };
			break;
		case BorderStyle.ThinThinSmallGap:
			array = new float[3] { -0.75f, -0.75f, 1f };
			break;
		case BorderStyle.ThinThickMediumGap:
			array = new float[3] { 1f, 0.5f, 0.5f };
			break;
		case BorderStyle.ThickThinMediumGap:
			array = new float[3] { 0.5f, 0.5f, 1f };
			break;
		case BorderStyle.ThinThickLargeGap:
			array = new float[3] { -1.5f, 1f, -0.75f };
			break;
		case BorderStyle.ThickThinLargeGap:
			array = new float[3] { -0.75f, 1f, -1.5f };
			break;
		case BorderStyle.Triple:
			array = new float[5] { 1f, 1f, 1f, 1f, 1f };
			break;
		case BorderStyle.ThinThickThinSmallGap:
			array = new float[5] { -0.75f, -0.75f, 1f, -0.75f, -0.75f };
			break;
		case BorderStyle.ThickThickThinMediumGap:
			array = new float[5] { 0.5f, 0.5f, 1f, 0.5f, 0.5f };
			break;
		case BorderStyle.ThinThickThinLargeGap:
			array = new float[5] { -0.75f, 1f, -1.5f, 1f, -0.75f };
			break;
		}
		if (array.Length == 1)
		{
			return new float[1] { lineWidth };
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] >= 0f)
			{
				array[i] *= lineWidth;
			}
			else
			{
				array[i] = Math.Abs(array[i]);
			}
		}
		return array;
	}

	private bool IsDoubleBorder(Border border)
	{
		switch (border.BorderType)
		{
		case BorderStyle.Double:
		case BorderStyle.ThinThickSmallGap:
		case BorderStyle.ThinThinSmallGap:
		case BorderStyle.ThinThickMediumGap:
		case BorderStyle.ThickThinMediumGap:
		case BorderStyle.ThinThickLargeGap:
		case BorderStyle.ThickThinLargeGap:
			return true;
		default:
			return false;
		}
	}

	private void DrawMultiLineLeftBorder(Border leftBorder, PointF start, PointF end, bool isTopBorderSame, bool isBetweenBorderSame, bool isBottomBorderSame)
	{
		if (IsDoubleBorder(leftBorder))
		{
			DrawDoubleLineLeftBorder(leftBorder, start, end, isTopBorderSame, isBetweenBorderSame, isBottomBorderSame);
		}
		else
		{
			DrawTripleLineLeftBorder(leftBorder, start, end, isTopBorderSame, isBetweenBorderSame, isBottomBorderSame);
		}
	}

	private void DrawDoubleLineLeftBorder(Border leftBorder, PointF start, PointF end, bool isTopBorderSame, bool isBetweenBorderSame, bool isBottomBorderSame)
	{
		float[] borderLineWidthArray = GetBorderLineWidthArray(leftBorder.BorderType, leftBorder.LineWidth);
		PdfPen pen = GetPen(leftBorder.BorderType, borderLineWidthArray[0], leftBorder.Color);
		PointF point = new PointF(start.X + borderLineWidthArray[0] / 2f, start.Y);
		PointF point2 = new PointF(end.X + borderLineWidthArray[0] / 2f, end.Y);
		if (isBottomBorderSame)
		{
			point2 = new PointF(point2.X, point2.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2]);
		}
		PDFGraphics.DrawLine(pen, point, point2);
		PdfPen pen2 = GetPen(leftBorder.BorderType, borderLineWidthArray[2], leftBorder.Color);
		PointF point3 = new PointF(start.X + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f, start.Y);
		PointF point4 = new PointF(end.X + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f, end.Y);
		if (isTopBorderSame || isBetweenBorderSame)
		{
			point3 = new PointF(point3.X, point3.Y + borderLineWidthArray[0] + borderLineWidthArray[1]);
		}
		PDFGraphics.DrawLine(pen2, point3, point4);
	}

	private void DrawTripleLineLeftBorder(Border leftBorder, PointF start, PointF end, bool isTopBorderSame, bool isBetweenBorderSame, bool isBottomBorderSame)
	{
		float[] borderLineWidthArray = GetBorderLineWidthArray(leftBorder.BorderType, leftBorder.LineWidth);
		PdfPen pen = GetPen(leftBorder.BorderType, borderLineWidthArray[0], leftBorder.Color);
		PointF point = new PointF(start.X + borderLineWidthArray[0] / 2f, start.Y);
		PointF point2 = new PointF(end.X + borderLineWidthArray[0] / 2f, end.Y);
		if (isBottomBorderSame)
		{
			point2 = new PointF(point2.X, point2.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] + borderLineWidthArray[3] + borderLineWidthArray[4]);
		}
		PDFGraphics.DrawLine(pen, point, point2);
		PdfPen pen2 = GetPen(leftBorder.BorderType, borderLineWidthArray[2], leftBorder.Color);
		PointF point3 = new PointF(start.X + (borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f), start.Y);
		PointF point4 = new PointF(end.X + (borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f), end.Y);
		if (isTopBorderSame)
		{
			point3 = new PointF(point3.X, point3.Y + borderLineWidthArray[0] + borderLineWidthArray[1]);
		}
		if (isBottomBorderSame)
		{
			point4 = new PointF(point4.X, point4.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2]);
		}
		PDFGraphics.DrawLine(pen2, point3, point4);
		PdfPen pen3 = GetPen(leftBorder.BorderType, borderLineWidthArray[4], leftBorder.Color);
		PointF point5 = new PointF(start.X + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] + borderLineWidthArray[3] + borderLineWidthArray[4] / 2f, start.Y);
		PointF point6 = new PointF(end.X + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] + borderLineWidthArray[3] + borderLineWidthArray[4] / 2f, end.Y);
		if (isTopBorderSame || isBetweenBorderSame)
		{
			point5 = new PointF(point5.X, point5.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] + borderLineWidthArray[3]);
		}
		PDFGraphics.DrawLine(pen3, point5, point6);
	}

	private void DrawMultiLineRightBorder(Border rightBorder, PointF start, PointF end, bool isTopBorderSame, bool isBetweenBorderSame, bool isBottomBorderSame)
	{
		if (IsDoubleBorder(rightBorder))
		{
			DrawDoubleLineRightBorder(rightBorder, start, end, isTopBorderSame, isBetweenBorderSame, isBottomBorderSame);
		}
		else
		{
			DrawTripleLineRightBorder(rightBorder, start, end, isTopBorderSame, isBetweenBorderSame, isBottomBorderSame);
		}
	}

	private void DrawDoubleLineRightBorder(Border rightBorder, PointF start, PointF end, bool isTopBorderSame, bool isBetweenBorderSame, bool isBottomBorderSame)
	{
		float[] borderLineWidthArray = GetBorderLineWidthArray(rightBorder.BorderType, rightBorder.LineWidth);
		PdfPen pen = GetPen(rightBorder.BorderType, borderLineWidthArray[0], rightBorder.Color);
		PointF point = new PointF(start.X + borderLineWidthArray[0] / 2f, start.Y);
		PointF point2 = new PointF(end.X + borderLineWidthArray[0] / 2f, end.Y);
		if (isTopBorderSame || isBetweenBorderSame)
		{
			point = new PointF(point.X, point.Y + borderLineWidthArray[0] + borderLineWidthArray[1]);
		}
		PDFGraphics.DrawLine(pen, point, point2);
		PdfPen pen2 = GetPen(rightBorder.BorderType, borderLineWidthArray[2], rightBorder.Color);
		PointF point3 = new PointF(start.X + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f, start.Y);
		PointF point4 = new PointF(end.X + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f, end.Y);
		if (isBottomBorderSame)
		{
			point4 = new PointF(point4.X, point4.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2]);
		}
		PDFGraphics.DrawLine(pen2, point3, point4);
	}

	private void DrawTripleLineRightBorder(Border rightBorder, PointF start, PointF end, bool isTopBorderSame, bool isBetweenBorderSame, bool isBottomBorderSame)
	{
		float[] borderLineWidthArray = GetBorderLineWidthArray(rightBorder.BorderType, rightBorder.LineWidth);
		PdfPen pen = GetPen(rightBorder.BorderType, borderLineWidthArray[0], rightBorder.Color);
		PointF point = new PointF(start.X + borderLineWidthArray[0] / 2f, start.Y);
		PointF point2 = new PointF(end.X + borderLineWidthArray[0] / 2f, end.Y);
		if (isTopBorderSame || isBetweenBorderSame)
		{
			point = new PointF(point.X, point.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] + borderLineWidthArray[3]);
		}
		PDFGraphics.DrawLine(pen, point, point2);
		PdfPen pen2 = GetPen(rightBorder.BorderType, borderLineWidthArray[2], rightBorder.Color);
		PointF point3 = new PointF(start.X + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f, start.Y);
		PointF point4 = new PointF(end.X + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f, end.Y);
		if (isTopBorderSame)
		{
			point3 = new PointF(point3.X, point3.Y + borderLineWidthArray[0] + borderLineWidthArray[1]);
		}
		if (isBottomBorderSame)
		{
			point4 = new PointF(point4.X, point4.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2]);
		}
		PDFGraphics.DrawLine(pen2, point3, point4);
		PdfPen pen3 = GetPen(rightBorder.BorderType, borderLineWidthArray[4], rightBorder.Color);
		PointF point5 = new PointF(start.X + borderLineWidthArray[0] + borderLineWidthArray[1] + (borderLineWidthArray[2] + borderLineWidthArray[3] + borderLineWidthArray[4] / 2f), start.Y);
		PointF point6 = new PointF(end.X + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] + borderLineWidthArray[3] + borderLineWidthArray[4] / 2f, end.Y);
		if (isBottomBorderSame)
		{
			point6 = new PointF(point6.X, point6.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] + borderLineWidthArray[3] + borderLineWidthArray[4]);
		}
		PDFGraphics.DrawLine(pen3, point5, point6);
	}

	private void DrawMultiLineTopBorder(Border topBorder, PointF start, PointF end, bool isLeftBorderSame, bool isRightBorderSame)
	{
		if (IsDoubleBorder(topBorder))
		{
			DrawDoubleLineTopBorder(topBorder, start, end, isLeftBorderSame, isRightBorderSame);
		}
		else
		{
			DrawTripleLineTopBorder(topBorder, start, end, isLeftBorderSame, isRightBorderSame);
		}
	}

	private void DrawDoubleLineTopBorder(Border topBorder, PointF start, PointF end, bool isLeftBorderSame, bool isRightBorderSame)
	{
		float[] borderLineWidthArray = GetBorderLineWidthArray(topBorder.BorderType, topBorder.LineWidth);
		PdfPen pen = GetPen(topBorder.BorderType, borderLineWidthArray[0], topBorder.Color);
		PointF point = new PointF(start.X, start.Y + borderLineWidthArray[0] / 2f);
		PointF point2 = new PointF(end.X, end.Y + borderLineWidthArray[0] / 2f);
		PDFGraphics.DrawLine(pen, point, point2);
		PdfPen pen2 = GetPen(topBorder.BorderType, borderLineWidthArray[2], topBorder.Color);
		PointF point3 = new PointF(start.X, start.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f);
		PointF point4 = new PointF(end.X, end.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f);
		if (isLeftBorderSame)
		{
			point3 = new PointF(point3.X + borderLineWidthArray[0] + borderLineWidthArray[1], point3.Y);
		}
		if (isRightBorderSame)
		{
			point4 = new PointF(point4.X - (borderLineWidthArray[1] + borderLineWidthArray[2]), point4.Y);
		}
		PDFGraphics.DrawLine(pen2, point3, point4);
	}

	private void DrawTripleLineTopBorder(Border topBorder, PointF start, PointF end, bool isLeftBorderSame, bool isRightBorderSame)
	{
		float[] borderLineWidthArray = GetBorderLineWidthArray(topBorder.BorderType, topBorder.LineWidth);
		PdfPen pen = GetPen(topBorder.BorderType, borderLineWidthArray[0], topBorder.Color);
		PointF point = new PointF(start.X, start.Y + borderLineWidthArray[0] / 2f);
		PointF point2 = new PointF(end.X, end.Y + borderLineWidthArray[0] / 2f);
		PDFGraphics.DrawLine(pen, point, point2);
		PdfPen pen2 = GetPen(topBorder.BorderType, borderLineWidthArray[2], topBorder.Color);
		PointF point3 = new PointF(start.X, start.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f);
		PointF point4 = new PointF(end.X, end.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f);
		if (isLeftBorderSame)
		{
			point3 = new PointF(point3.X + borderLineWidthArray[0] + borderLineWidthArray[1], point3.Y);
		}
		if (isRightBorderSame)
		{
			point4 = new PointF(point4.X - (borderLineWidthArray[3] + borderLineWidthArray[4]), point4.Y);
		}
		PDFGraphics.DrawLine(pen2, point3, point4);
		PdfPen pen3 = GetPen(topBorder.BorderType, borderLineWidthArray[4], topBorder.Color);
		PointF point5 = new PointF(start.X, start.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] + borderLineWidthArray[3] + borderLineWidthArray[4] / 2f);
		PointF point6 = new PointF(end.X, end.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] + borderLineWidthArray[3] + borderLineWidthArray[4] / 2f);
		if (isLeftBorderSame)
		{
			point5 = new PointF(point5.X + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] + borderLineWidthArray[3], point5.Y);
		}
		if (isRightBorderSame)
		{
			point6 = new PointF(point6.X - (borderLineWidthArray[1] + borderLineWidthArray[2] + borderLineWidthArray[3] + borderLineWidthArray[4]), point6.Y);
		}
		PDFGraphics.DrawLine(pen3, point5, point6);
	}

	private void DrawMultiLineBottomBorder(Border bottomBorder, PointF start, PointF end, bool isLeftBorderSame, bool isRightBorderSame)
	{
		if (IsDoubleBorder(bottomBorder))
		{
			DrawDoubleLineBottomBorder(bottomBorder, start, end, isLeftBorderSame, isRightBorderSame);
		}
		else
		{
			DrawTripleLineBottomBorder(bottomBorder, start, end, isLeftBorderSame, isRightBorderSame);
		}
	}

	private void DrawDoubleLineBottomBorder(Border bottomBorder, PointF start, PointF end, bool isLeftBorderSame, bool isRightBorderSame)
	{
		float[] borderLineWidthArray = GetBorderLineWidthArray(bottomBorder.BorderType, bottomBorder.LineWidth);
		PdfPen pen = GetPen(bottomBorder.BorderType, borderLineWidthArray[0], bottomBorder.Color);
		PointF point = new PointF(start.X, start.Y + borderLineWidthArray[0] / 2f);
		PointF point2 = new PointF(end.X, end.Y + borderLineWidthArray[0] / 2f);
		if (isLeftBorderSame)
		{
			point = new PointF(point.X + borderLineWidthArray[0] + borderLineWidthArray[1], point.Y);
		}
		if (isRightBorderSame)
		{
			point2 = new PointF(point2.X - (borderLineWidthArray[1] + borderLineWidthArray[2]), point2.Y);
		}
		PDFGraphics.DrawLine(pen, point, point2);
		PdfPen pen2 = GetPen(bottomBorder.BorderType, borderLineWidthArray[2], bottomBorder.Color);
		PointF point3 = new PointF(start.X, start.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f);
		PointF point4 = new PointF(end.X, end.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f);
		PDFGraphics.DrawLine(pen2, point3, point4);
	}

	private void DrawDoubleLine(WCharacterFormat charFormat, BorderStyle borderType, float lineWidth, PointF start, PointF end)
	{
		float[] borderLineWidthArray = GetBorderLineWidthArray(borderType, lineWidth);
		Color textColor = GetTextColor(charFormat);
		Color borderColor = Color.Black;
		if (textColor != Color.Black)
		{
			borderColor = textColor;
		}
		PdfPen pen = GetPen(borderType, borderLineWidthArray[0], borderColor);
		PointF point = new PointF(start.X, start.Y + borderLineWidthArray[0] / 2f);
		PointF point2 = new PointF(end.X, end.Y + borderLineWidthArray[0] / 2f);
		PDFGraphics.DrawLine(pen, point, point2);
		PdfPen pen2 = GetPen(borderType, borderLineWidthArray[2], borderColor);
		PointF point3 = new PointF(start.X, start.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f);
		PointF point4 = new PointF(end.X, end.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f);
		PDFGraphics.DrawLine(pen2, point3, point4);
	}

	private void DrawTripleLineBottomBorder(Border bottomBorder, PointF start, PointF end, bool isLeftBorderSame, bool isRightBorderSame)
	{
		float[] borderLineWidthArray = GetBorderLineWidthArray(bottomBorder.BorderType, bottomBorder.LineWidth);
		PdfPen pen = GetPen(bottomBorder.BorderType, borderLineWidthArray[0], bottomBorder.Color);
		PointF point = new PointF(start.X, start.Y + borderLineWidthArray[0] / 2f);
		PointF point2 = new PointF(end.X, end.Y + borderLineWidthArray[0] / 2f);
		if (isLeftBorderSame)
		{
			point = new PointF(point.X + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] + borderLineWidthArray[3], point.Y);
		}
		if (isRightBorderSame)
		{
			point2 = new PointF(point2.X - (borderLineWidthArray[1] + borderLineWidthArray[2] + borderLineWidthArray[3] + borderLineWidthArray[4]), point2.Y);
		}
		PDFGraphics.DrawLine(pen, point, point2);
		PdfPen pen2 = GetPen(bottomBorder.BorderType, borderLineWidthArray[2], bottomBorder.Color);
		PointF point3 = new PointF(start.X, start.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f);
		PointF point4 = new PointF(end.X, end.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f);
		if (isLeftBorderSame)
		{
			point3 = new PointF(point3.X + borderLineWidthArray[0] + borderLineWidthArray[1], point3.Y);
		}
		if (isRightBorderSame)
		{
			point4 = new PointF(point4.X - (borderLineWidthArray[1] + borderLineWidthArray[2]), point4.Y);
		}
		PDFGraphics.DrawLine(pen2, point3, point4);
		PdfPen pen3 = GetPen(bottomBorder.BorderType, borderLineWidthArray[4], bottomBorder.Color);
		PointF point5 = new PointF(start.X, start.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] + borderLineWidthArray[3] + borderLineWidthArray[4] / 2f);
		PointF point6 = new PointF(end.X, end.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] + borderLineWidthArray[3] + borderLineWidthArray[4] / 2f);
		PDFGraphics.DrawLine(pen3, point5, point6);
	}

	private void DrawMultiLineBetweenBorder(Border betweenBorder, PointF start, PointF end, bool isLeftBorderSame, bool isRightBorderSame, Border leftBorder, Border rightBorder, bool isOverlapLeft, bool isOverlapRight)
	{
		if (IsDoubleBorder(betweenBorder))
		{
			DrawDoubleLineBetweenBorder(betweenBorder, start, end, isLeftBorderSame, isRightBorderSame, leftBorder, rightBorder, isOverlapLeft, isOverlapRight);
		}
		else
		{
			DrawTripleLineBetweenBorder(betweenBorder, start, end, isLeftBorderSame, isRightBorderSame, leftBorder, rightBorder, isOverlapLeft, isOverlapRight);
		}
	}

	private void DrawDoubleLineBetweenBorder(Border betweenBorder, PointF start, PointF end, bool isLeftBorderSame, bool isRightBorderSame, Border leftBorder, Border rightBorder, bool isOverlapLeft, bool isOverlapRight)
	{
		float[] borderLineWidthArray = GetBorderLineWidthArray(betweenBorder.BorderType, betweenBorder.LineWidth);
		float[] array = ((leftBorder != null && !isLeftBorderSame) ? GetBorderLineWidthArray(leftBorder.BorderType, leftBorder.LineWidth) : null);
		float[] array2 = ((rightBorder != null && !isRightBorderSame) ? GetBorderLineWidthArray(rightBorder.BorderType, rightBorder.LineWidth) : null);
		PdfPen pen = GetPen(betweenBorder.BorderType, borderLineWidthArray[0], betweenBorder.Color);
		PointF point = new PointF(start.X, start.Y + borderLineWidthArray[0] / 2f);
		PointF point2 = new PointF(end.X, end.Y + borderLineWidthArray[0] / 2f);
		if (isLeftBorderSame)
		{
			point = new PointF(point.X + borderLineWidthArray[0] + borderLineWidthArray[1], point.Y);
		}
		else if (array != null)
		{
			point = new PointF(point.X + GetLeftRightLineWidht(array, isLeft: true), point.Y);
		}
		if (isRightBorderSame)
		{
			point2 = new PointF(point2.X - (borderLineWidthArray[1] + borderLineWidthArray[2]), point2.Y);
		}
		else if (array2 != null)
		{
			point2 = new PointF(point2.X - GetLeftRightLineWidht(array2, isLeft: false), point2.Y);
		}
		PDFGraphics.DrawLine(pen, point, point2);
		PdfPen pen2 = GetPen(betweenBorder.BorderType, borderLineWidthArray[2], betweenBorder.Color);
		PointF point3 = new PointF(start.X, start.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f);
		PointF point4 = new PointF(end.X, end.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f);
		if (isLeftBorderSame)
		{
			point3 = new PointF(point3.X + borderLineWidthArray[0] + borderLineWidthArray[1], point3.Y);
		}
		else if (array != null)
		{
			point3 = new PointF(point3.X + GetLeftRightLineWidht(array, isLeft: true), point3.Y);
		}
		if (isRightBorderSame)
		{
			point4 = new PointF(point4.X - (borderLineWidthArray[1] + borderLineWidthArray[2]), point4.Y);
		}
		else if (array2 != null)
		{
			point4 = new PointF(point4.X - GetLeftRightLineWidht(array2, isLeft: false), point4.Y);
		}
		PDFGraphics.DrawLine(pen2, point3, point4);
		if (isOverlapRight || isOverlapLeft)
		{
			if (isOverlapLeft)
			{
				PointF point5 = new PointF(start.X + borderLineWidthArray[0] / 2f, start.Y);
				PointF point6 = new PointF(start.X + borderLineWidthArray[0] / 2f, start.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2]);
				PDFGraphics.DrawLine(pen, point5, point6);
			}
			if (isOverlapRight)
			{
				PointF point7 = new PointF(end.X - borderLineWidthArray[0] / 2f, end.Y);
				PointF point8 = new PointF(end.X - borderLineWidthArray[0] / 2f, end.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2]);
				PDFGraphics.DrawLine(pen, point7, point8);
			}
		}
	}

	private void DrawTripleLineBetweenBorder(Border betweenBorder, PointF start, PointF end, bool isLeftBorderSame, bool isRightBorderSame, Border leftBorder, Border rightBorder, bool isOverlapLeft, bool isOverlapRight)
	{
		float[] borderLineWidthArray = GetBorderLineWidthArray(betweenBorder.BorderType, betweenBorder.LineWidth);
		float[] array = ((leftBorder != null && !isLeftBorderSame) ? GetBorderLineWidthArray(leftBorder.BorderType, leftBorder.LineWidth) : null);
		float[] array2 = ((rightBorder != null && !isRightBorderSame) ? GetBorderLineWidthArray(rightBorder.BorderType, rightBorder.LineWidth) : null);
		PdfPen pen = GetPen(betweenBorder.BorderType, borderLineWidthArray[0], betweenBorder.Color);
		PointF point = new PointF(start.X, start.Y + borderLineWidthArray[0] / 2f);
		PointF point2 = new PointF(end.X, end.Y + borderLineWidthArray[0] / 2f);
		if (isLeftBorderSame)
		{
			point = new PointF(point.X + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] + borderLineWidthArray[3], point.Y);
		}
		else if (array != null)
		{
			point = new PointF(point.X + GetLeftRightLineWidht(array, isLeft: true), point.Y);
		}
		if (isRightBorderSame)
		{
			point2 = new PointF(point2.X - (borderLineWidthArray[1] + borderLineWidthArray[2] + borderLineWidthArray[3] + borderLineWidthArray[4]), point2.Y);
		}
		else if (array2 != null)
		{
			point2 = new PointF(point2.X - GetLeftRightLineWidht(array2, isLeft: false), point2.Y);
		}
		PDFGraphics.DrawLine(pen, point, point2);
		PdfPen pen2 = GetPen(betweenBorder.BorderType, borderLineWidthArray[2], betweenBorder.Color);
		PointF point3 = new PointF(start.X, start.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f);
		PointF point4 = new PointF(end.X, end.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f);
		if (isLeftBorderSame)
		{
			point3 = new PointF(point3.X + borderLineWidthArray[0] + borderLineWidthArray[1] + (isOverlapLeft ? 0f : (borderLineWidthArray[2] + borderLineWidthArray[3])), point3.Y);
		}
		else if (array != null)
		{
			point3 = new PointF(point3.X + GetLeftRightLineWidht(array, isLeft: true), point3.Y);
		}
		if (isRightBorderSame)
		{
			point4 = new PointF(point4.X - ((isOverlapRight ? 0f : (borderLineWidthArray[1] + borderLineWidthArray[2])) + borderLineWidthArray[3] + borderLineWidthArray[4]), point4.Y);
		}
		else if (array2 != null)
		{
			point4 = new PointF(point4.X - GetLeftRightLineWidht(array2, isLeft: false), point4.Y);
		}
		PDFGraphics.DrawLine(pen2, point3, point4);
		PdfPen pen3 = GetPen(betweenBorder.BorderType, borderLineWidthArray[4], betweenBorder.Color);
		PointF point5 = new PointF(start.X, start.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] + borderLineWidthArray[3] + borderLineWidthArray[4] / 2f);
		PointF point6 = new PointF(end.X, end.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] + borderLineWidthArray[3] + borderLineWidthArray[4] / 2f);
		if (isLeftBorderSame)
		{
			point5 = new PointF(point5.X + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] + borderLineWidthArray[3], point5.Y);
		}
		else if (array != null)
		{
			point5 = new PointF(point5.X + GetLeftRightLineWidht(array, isLeft: true), point5.Y);
		}
		if (isRightBorderSame)
		{
			point6 = new PointF(point6.X - (borderLineWidthArray[1] + borderLineWidthArray[2] + borderLineWidthArray[3] + borderLineWidthArray[4]), point6.Y);
		}
		else if (array2 != null)
		{
			point6 = new PointF(point6.X - GetLeftRightLineWidht(array2, isLeft: false), point6.Y);
		}
		PDFGraphics.DrawLine(pen3, point5, point6);
		if (isOverlapRight || isOverlapLeft)
		{
			if (isOverlapLeft)
			{
				PointF point7 = new PointF(start.X + borderLineWidthArray[0] / 2f, start.Y);
				PointF point8 = new PointF(point7.X, start.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] + borderLineWidthArray[3] + borderLineWidthArray[4]);
				PDFGraphics.DrawLine(pen, point7, point8);
				point7 = new PointF(start.X + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f, start.Y);
				point8 = new PointF(point7.X, point8.Y);
				PDFGraphics.DrawLine(pen, point7, point8);
			}
			if (isOverlapRight)
			{
				PointF point9 = new PointF(end.X - borderLineWidthArray[0] / 2f, end.Y);
				PointF point10 = new PointF(point9.X, end.Y + borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] + borderLineWidthArray[3] + borderLineWidthArray[4]);
				PDFGraphics.DrawLine(pen, point9, point10);
				point9 = new PointF(end.X - (borderLineWidthArray[0] + borderLineWidthArray[1] + borderLineWidthArray[2] / 2f), end.Y);
				point10 = new PointF(point9.X, point10.Y);
				PDFGraphics.DrawLine(pen, point9, point10);
			}
		}
	}

	private float GetLeftRightLineWidht(float[] lineArray, bool isLeft)
	{
		float num = 0f;
		if (lineArray.Length > 4)
		{
			num += lineArray[1] + lineArray[2] + lineArray[3];
			return num + (isLeft ? lineArray[0] : lineArray[4]);
		}
		if (lineArray.Length > 2)
		{
			num += lineArray[1];
			return num + (isLeft ? lineArray[0] : lineArray[2]);
		}
		return num;
	}

	internal void DrawBackgroundColor(Color bgColor, int width, int height)
	{
		PDFGraphics.DrawRectangle(GetPDFBrush(bgColor), new RectangleF(0f, 0f, width, height));
	}

	internal void DrawBackgroundImage(DocGen.Drawing.SkiaSharpHelper.Image image, WPageSetup pageSetup)
	{
		PdfImage pdfImage = GetPdfImage(image);
		PDFGraphics.DrawImage(pdfImage, 0f, 0f, pageSetup.PageSize.Width, pageSetup.PageSize.Height);
		if (image != null)
		{
			image.Dispose();
			image = null;
		}
	}

	internal void DrawWatermark(Watermark watermark, WPageSetup pageSetup, RectangleF bounds)
	{
		switch (watermark.Type)
		{
		case WatermarkType.PictureWatermark:
			DrawImageWatermark(watermark as PictureWatermark, bounds, pageSetup);
			break;
		case WatermarkType.TextWatermark:
			DrawTextWatermark(watermark as TextWatermark, bounds, pageSetup);
			break;
		case WatermarkType.NoWatermark:
			break;
		}
	}

	internal void Draw(Page page, ref int autoTagsCount)
	{
		m_pageMarginLeft = page.Setup.Margins.Left;
		if (page.DocSection.Document.DOP.MirrorMargins && page.Number % 2 == 0)
		{
			m_pageMarginLeft = page.Setup.Margins.Right;
		}
		PDFGraphics.DocIOPageBounds = new RectangleF(0f, 0f, (int)page.Setup.PageSize.Width, (int)page.Setup.PageSize.Height);
		autoTagCount = autoTagsCount;
		bool autoTag = AutoTag;
		bool flag = false;
		float num = page.DocSection.PageSetup.PageSize.Width - page.DocSection.PageSetup.Margins.Right + 10f;
		if (page.PageWidgets.Count != 0)
		{
			if (page.DocSection.Document.Background.Type == BackgroundType.Picture && page.BackgroundImage != null)
			{
				DrawBackgroundImage(CreateBitmap().Decode(page.BackgroundImage) as DocGen.Drawing.SkiaSharpHelper.Image, page.Setup);
			}
			if (page.DocSection.Document.Background.Type == BackgroundType.Color)
			{
				DrawBackgroundColor(page.BackgroundColor, (int)page.Setup.PageSize.Width, (int)page.Setup.PageSize.Height);
			}
			for (int i = 0; i < page.PageWidgets.Count; i++)
			{
				if (!(page.PageWidgets[i].Widget is HeaderFooter))
				{
					continue;
				}
				Watermark watermark = (page.PageWidgets[i].Widget as HeaderFooter).Watermark;
				if (!(page.PageWidgets[i].Widget as HeaderFooter).WriteWatermark || IsEmptyWaterMark(watermark))
				{
					continue;
				}
				if (watermark != null && watermark.OrderIndex != int.MaxValue)
				{
					flag = IsWaterMarkNeedToBeDraw(page, page.PageWidgets[i].Widget as HeaderFooter);
				}
				if (!flag)
				{
					if (AutoTag)
					{
						PdfArtifact tag = new PdfArtifact(PdfArtifactType.Pagination, new RectangleF(page.PageWidgets[2].Bounds.X, page.PageWidgets[2].Bounds.Y, page.Setup.ClientWidth, page.PageWidgets[1].Bounds.Y - page.PageWidgets[2].Bounds.Y), new PdfAttached(PdfEdge.Top), PdfArtifactSubType.Watermark);
						PDFGraphics.SetTag(tag);
					}
					DrawWatermark(watermark, page.Setup, new RectangleF(page.PageWidgets[2].Bounds.X, page.PageWidgets[2].Bounds.Y, page.Setup.ClientWidth, page.PageWidgets[1].Bounds.Y - page.PageWidgets[2].Bounds.Y));
					if (AutoTag)
					{
						PDFGraphics.ReSetTag();
					}
				}
			}
			if (page.DocSection.Document.TrackChangesBalloonCount > 0)
			{
				PDFGraphics.DrawRectangle(new PdfSolidBrush(Color.FromArgb(240, 240, 240)), new RectangleF(num, 0f, 250f, page.DocSection.PageSetup.PageSize.Height));
			}
		}
		bool flag2 = true;
		bool flag3 = false;
		for (int j = 0; j < page.PageWidgets.Count; j++)
		{
			if (!(page.PageWidgets[j].Widget is HeaderFooter) && m_prevPageHyperlink != null)
			{
				currHyperlink = m_prevPageHyperlink;
				m_prevPageHyperlink = null;
			}
			LayoutedWidget layoutedWidget = page.PageWidgets[j];
			if (layoutedWidget.Widget is HeaderFooter && autoTag && (layoutedWidget.Widget as HeaderFooter).ChildEntities.Count > 0)
			{
				AutoTag = false;
				PdfEdge pageEdge;
				PdfArtifactSubType subType;
				if (j == 0)
				{
					pageEdge = PdfEdge.Top;
					subType = PdfArtifactSubType.Header;
				}
				else
				{
					pageEdge = PdfEdge.Bottom;
					subType = PdfArtifactSubType.Footer;
				}
				PdfArtifact tag2 = new PdfArtifact(PdfArtifactType.Pagination, new RectangleF(layoutedWidget.Bounds.X, layoutedWidget.Bounds.Y, layoutedWidget.Bounds.Width, layoutedWidget.Bounds.Height), new PdfAttached(pageEdge), subType);
				PDFGraphics.SetTag(tag2);
			}
			bool isHaveToInitLayoutInfo = !(layoutedWidget.Widget is HeaderFooter);
			if ((page.NumberOfBehindWidgetsInHeader > 0 || page.NumberOfBehindWidgetsInFooter > 0) && flag2)
			{
				flag2 = false;
				int length = page.NumberOfBehindWidgetsInHeader + page.NumberOfBehindWidgetsInFooter;
				DrawBehindWidgets(page.BehindWidgets, layoutedWidget.Widget, length, isHaveToInitLayoutInfo);
			}
			else if (!(layoutedWidget.Widget is HeaderFooter) && page.BehindWidgets.Count > 0)
			{
				DrawBehindWidgets(page.BehindWidgets, layoutedWidget.Widget, page.BehindWidgets.Count, isHaveToInitLayoutInfo: true);
			}
			if (!(layoutedWidget.Widget is HeaderFooter) && !flag3)
			{
				for (int k = 0; k < page.FootnoteWidgets.Count; k++)
				{
					Draw(page.FootnoteWidgets[k], isHaveToInitLayoutInfo: true);
				}
				for (int l = 0; l < page.EndnoteWidgets.Count; l++)
				{
					Draw(page.EndnoteWidgets[l], isHaveToInitLayoutInfo: true);
				}
				flag3 = true;
			}
			Draw(layoutedWidget, isHaveToInitLayoutInfo);
			DrawOverLappedShapeWidgets(isHaveToInitLayoutInfo);
			if (flag && page.PageWidgets[j].Widget is HeaderFooter)
			{
				if (AutoTag)
				{
					PdfArtifact tag3 = new PdfArtifact(PdfArtifactType.Pagination, new RectangleF(page.PageWidgets[2].Bounds.X, page.PageWidgets[2].Bounds.Y, page.Setup.ClientWidth, page.PageWidgets[1].Bounds.Y - page.PageWidgets[2].Bounds.Y), new PdfAttached(PdfEdge.Top), PdfArtifactSubType.Watermark);
					PDFGraphics.SetTag(tag3);
				}
				DrawWatermark((page.PageWidgets[j].Widget as HeaderFooter).Watermark, page.Setup, new RectangleF(page.PageWidgets[2].Bounds.X, page.PageWidgets[2].Bounds.Y, page.Setup.ClientWidth, page.PageWidgets[1].Bounds.Y - page.PageWidgets[2].Bounds.Y));
				if (AutoTag)
				{
					PDFGraphics.ReSetTag();
				}
			}
			if (currParagraph != null)
			{
				currParagraph = null;
			}
			if (j == page.PageWidgets.Count - 1 && currHyperlink != null)
			{
				m_prevPageHyperlink = currHyperlink;
			}
			if (layoutedWidget.Widget is HeaderFooter && autoTag && (layoutedWidget.Widget as HeaderFooter).ChildEntities.Count > 0)
			{
				AutoTag = true;
				PDFGraphics.ReSetTag();
			}
			if (layoutedWidget.Widget is HeaderFooter)
			{
				continue;
			}
			if (m_commentMarks != null)
			{
				DrawCommentMarks(page.DocSection.Document.RevisionOptions);
			}
			for (int m = 0; m < page.TrackChangesMarkups.Count; m++)
			{
				RevisionOptions revisionOptions = page.DocSection.Document.RevisionOptions;
				TrackChangesMarkups trackChangesMarkups = page.TrackChangesMarkups[m];
				RectangleF bounds = new RectangleF(num + 20f, trackChangesMarkups.LtWidget.Bounds.Y, 210f, trackChangesMarkups.LtWidget.Bounds.Height);
				float num2 = ((trackChangesMarkups is CommentsMarkups) ? (trackChangesMarkups as CommentsMarkups).ExtraSpacing : 0f);
				PointF point = new PointF(trackChangesMarkups.Position.X, trackChangesMarkups.Position.Y - num2);
				PointF pointF = new PointF(num - 5f, trackChangesMarkups.Position.Y - num2);
				PointF point2 = new PointF(num + 20f, trackChangesMarkups.LtWidget.Bounds.Y + 10f);
				Color color = Color.Red;
				bool flag4 = false;
				if (trackChangesMarkups is CommentsMarkups)
				{
					flag4 = (trackChangesMarkups as CommentsMarkups).Comment.Done;
					color = ((!flag4) ? GetRevisionColor(revisionOptions.CommentColor) : GetRevisionColor(revisionOptions.CommentColor, isInsertText: false, flag4));
				}
				else if (trackChangesMarkups.TypeOfMarkup == RevisionType.Deletions)
				{
					color = GetRevisionColor(revisionOptions.DeletedTextColor);
				}
				else if (trackChangesMarkups.TypeOfMarkup == RevisionType.Formatting)
				{
					color = GetRevisionColor(revisionOptions.RevisedPropertiesColor);
				}
				PdfPen pen = new PdfPen(color, 1f);
				PdfPen pdfPen = new PdfPen(color, 0.5f);
				pdfPen.DashStyle = PdfDashStyle.Dot;
				PDFGraphics.DrawLine(pdfPen, point, pointF);
				PDFGraphics.DrawLine(pdfPen, pointF, point2);
				PDFGraphics.DrawRoundedRectangle(bounds, 5, pen, (trackChangesMarkups is CommentsMarkups) ? new PdfSolidBrush(GetRevisionFillColor(revisionOptions.CommentColor, flag4)) : new PdfSolidBrush(Color.White));
				if (!(trackChangesMarkups is CommentsMarkups))
				{
					DrawMarkupTriangles(trackChangesMarkups.Position, color);
				}
				List<KeyValuePair<string, bool>> list = null;
				if (m_previousLineCommentStartMarks != null && m_previousLineCommentStartMarks.Count > 0)
				{
					list = m_previousLineCommentStartMarks;
					m_previousLineCommentStartMarks = null;
				}
				Draw(trackChangesMarkups.LtWidget, isHaveToInitLayoutInfo: true);
				if (list != null)
				{
					m_previousLineCommentStartMarks = list;
				}
			}
		}
		autoTagsCount = autoTagCount;
	}

	private void DrawMarkupTriangles(PointF position, Color revisionColor)
	{
		PdfBrush brush = new PdfSolidBrush(revisionColor);
		PdfPath pdfPath = new PdfPath();
		PointF pointF = new PointF(position.X - 2f, position.Y);
		PointF pointF2 = new PointF(position.X + 2f, position.Y);
		position.Y -= 3f;
		pdfPath.StartFigure();
		pdfPath.AddLine(pointF, pointF2);
		pdfPath.AddLine(pointF2, position);
		pdfPath.AddLine(position, pointF);
		pdfPath.CloseFigure();
		PDFGraphics.DrawPath(brush, pdfPath);
	}

	private bool IsEmptyWaterMark(Watermark waterMark)
	{
		if (waterMark is TextWatermark && (waterMark as TextWatermark).Text.Trim(ControlChar.SpaceChar) == string.Empty)
		{
			return true;
		}
		if (waterMark is PictureWatermark && (waterMark as PictureWatermark).Picture == null)
		{
			return true;
		}
		return false;
	}

	private bool IsWaterMarkNeedToBeDraw(Page page, HeaderFooter headerFooter)
	{
		bool flag = false;
		for (int i = 0; i < headerFooter.ChildEntities.Count; i++)
		{
			if (headerFooter.ChildEntities[i] is WParagraph)
			{
				WParagraph paragraph = headerFooter.ChildEntities[i] as WParagraph;
				flag = IsWaterMarkInParagraph(paragraph, page);
			}
			else if (headerFooter.ChildEntities[i] is WTable)
			{
				WTable table = headerFooter.ChildEntities[i] as WTable;
				flag = IsWaterMarkInTable(table, page);
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsWaterMarkInParagraph(WParagraph paragraph, Page page)
	{
		bool flag = false;
		for (int i = 0; i < paragraph.ChildEntities.Count; i++)
		{
			if (paragraph.ChildEntities[i] is WPicture)
			{
				if ((paragraph.ChildEntities[i] as WPicture).TextWrappingStyle != 0)
				{
					WPicture wPicture = paragraph.ChildEntities[i] as WPicture;
					flag = IsWaterMarkOrderHasChanged(wPicture.OrderIndex, wPicture.IsBelowText, page);
				}
			}
			else if (paragraph.ChildEntities[i] is Shape && (paragraph.ChildEntities[i] as Shape).WrapFormat.TextWrappingStyle != 0)
			{
				Shape shape = paragraph.ChildEntities[i] as Shape;
				flag = IsWaterMarkOrderHasChanged(shape.ZOrderPosition, shape.IsBelowText, page);
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsWaterMarkInTable(WTable table, Page page)
	{
		bool flag = false;
		for (int i = 0; i < table.Rows.Count; i++)
		{
			for (int j = 0; j < table.Rows[i].Cells.Count; j++)
			{
				WTableCell wTableCell = table.Rows[i].Cells[j];
				for (int k = 0; k < wTableCell.ChildEntities.Count; k++)
				{
					if (wTableCell.ChildEntities[k] is WParagraph)
					{
						flag = IsWaterMarkInParagraph(wTableCell.ChildEntities[k] as WParagraph, page);
					}
					else if (wTableCell.ChildEntities[k] is WTable)
					{
						flag = IsWaterMarkInTable(wTableCell.ChildEntities[k] as WTable, page);
					}
					if (flag)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool IsWaterMarkOrderHasChanged(int OrderIndex, bool IsBelowText, Page page)
	{
		if (page.DocSection.Document.Settings.CompatibilityMode.ToString() == "Word2013")
		{
			if (OrderIndex < (GetCurrentHeader(page.DocSection) as HeaderFooter).Watermark.OrderIndex)
			{
				return true;
			}
		}
		else if (OrderIndex < (GetCurrentHeader(page.DocSection) as HeaderFooter).Watermark.OrderIndex || IsBelowText)
		{
			return true;
		}
		return false;
	}

	internal void Draw(SplitWidgetContainer widget, LayoutedWidget layoutedWidget)
	{
		if (layoutedWidget.ChildWidgets.Count > 0)
		{
			LayoutedWidget layoutedWidget2 = layoutedWidget.ChildWidgets[0];
			if (layoutedWidget2.Widget is SplitWidgetContainer && layoutedWidget2.ChildWidgets.Count > 0)
			{
				WParagraph paragraph = (layoutedWidget2.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph;
				LayoutedWidget layoutedWidget3 = layoutedWidget2.ChildWidgets[layoutedWidget2.ChildWidgets.Count - 1];
				int count = layoutedWidget3.ChildWidgets.Count;
				if (count > 0 && layoutedWidget3.ChildWidgets[0].HorizontalAlign != HAlignment.Distributed && (layoutedWidget3.ChildWidgets[count - 1].Widget is SplitStringWidget || layoutedWidget3.ChildWidgets[count - 1].Widget is WTextRange) && !layoutedWidget3.IsLastLine)
				{
					for (int i = 0; i < layoutedWidget3.ChildWidgets.Count; i++)
					{
						layoutedWidget3.ChildWidgets[i].IsLastLine = true;
					}
					AlignChildWidgets(layoutedWidget3, paragraph);
				}
			}
		}
		Draw((IWidget)widget.RealWidgetContainer, layoutedWidget);
	}

	private LayoutedWidgetList SortLayoutWidgetsWithXPosition(LayoutedWidgetList childWidgets)
	{
		LayoutedWidgetList layoutedWidgetList = new LayoutedWidgetList();
		for (int i = 0; i <= childWidgets.Count - 1; i++)
		{
			LayoutedWidget layoutedWidget = childWidgets[i];
			int num = 0;
			while (true)
			{
				if (layoutedWidgetList.Count == 0)
				{
					layoutedWidgetList.Insert(0, layoutedWidget);
					break;
				}
				if (num == layoutedWidgetList.Count)
				{
					layoutedWidgetList.Add(layoutedWidget);
					break;
				}
				if (layoutedWidget.Bounds.X < layoutedWidgetList[num].Bounds.X)
				{
					layoutedWidgetList.Insert(num, layoutedWidget);
					break;
				}
				_ = layoutedWidget.Bounds.X;
				_ = layoutedWidgetList[num].Bounds.X;
				num++;
			}
		}
		return layoutedWidgetList;
	}

	private void AlignChildWidgets(LayoutedWidget LastLine, WParagraph paragraph)
	{
		LayoutedWidgetList layoutedWidgetList = SortLayoutWidgetsWithXPosition(LastLine.ChildWidgets);
		bool flag = false;
		if (paragraph != null)
		{
			flag = paragraph.ParagraphFormat.Bidi;
		}
		int num = 0;
		if (flag)
		{
			num = layoutedWidgetList.Count - 1;
		}
		if (paragraph != null && LastLine.ChildWidgets[0].Widget is WTextRange && (LastLine.ChildWidgets[0].Widget as WTextRange).m_layoutInfo != null && ((LastLine.ChildWidgets[0].Widget as WTextRange).m_layoutInfo as LayoutInfo).IsLineNumberItem && !flag)
		{
			num = 1;
		}
		if (flag)
		{
			layoutedWidgetList[num].Bounds = new RectangleF(layoutedWidgetList[num].Bounds.X + Convert.ToSingle((float)layoutedWidgetList[num].Spaces * layoutedWidgetList[num].WordSpace), layoutedWidgetList[num].Bounds.Y, layoutedWidgetList[num].Bounds.Width - Convert.ToSingle((float)layoutedWidgetList[num].Spaces * layoutedWidgetList[num].WordSpace), layoutedWidgetList[num].Bounds.Height);
			float num2 = layoutedWidgetList[num].Bounds.X;
			for (int num3 = num - 1; num3 >= 0; num3--)
			{
				LayoutedWidget layoutedWidget = layoutedWidgetList[num3 + 1];
				LayoutedWidget layoutedWidget2 = layoutedWidgetList[num3];
				if (!(layoutedWidget.Widget is WTextBox) && !(layoutedWidget.Widget is WPicture) && !(layoutedWidget.Widget is WChart) && !(layoutedWidget.Widget is Shape) && !(layoutedWidget.Widget is GroupShape) && !(layoutedWidget2.Widget is WTextBox) && !(layoutedWidget2.Widget is WPicture) && !(layoutedWidget2.Widget is WChart) && !(layoutedWidget2.Widget is Shape) && !(layoutedWidget2.Widget is GroupShape))
				{
					float num4 = layoutedWidget2.Bounds.Width - ((layoutedWidget2.SubWidth != 0f) ? Convert.ToSingle((float)layoutedWidget2.Spaces * layoutedWidget2.WordSpace) : 0f);
					num2 -= num4;
					layoutedWidget2.Bounds = new RectangleF(num2, layoutedWidget2.Bounds.Y, num4, layoutedWidget2.Bounds.Height);
				}
			}
		}
		else
		{
			layoutedWidgetList[num].Bounds = new RectangleF(layoutedWidgetList[num].Bounds.X, layoutedWidgetList[num].Bounds.Y, layoutedWidgetList[num].Bounds.Width - Convert.ToSingle((float)layoutedWidgetList[num].Spaces * layoutedWidgetList[num].WordSpace), layoutedWidgetList[num].Bounds.Height);
			for (int i = num + 1; i < layoutedWidgetList.Count; i++)
			{
				LayoutedWidget layoutedWidget3 = layoutedWidgetList[i - 1];
				LayoutedWidget layoutedWidget4 = layoutedWidgetList[i];
				if (!(layoutedWidget3.Widget is WTextBox) && !(layoutedWidget3.Widget is WPicture) && !(layoutedWidget3.Widget is WChart) && !(layoutedWidget3.Widget is Shape) && !(layoutedWidget3.Widget is GroupShape) && !(layoutedWidget4.Widget is WTextBox) && !(layoutedWidget4.Widget is WPicture) && !(layoutedWidget4.Widget is WChart) && !(layoutedWidget4.Widget is Shape) && !(layoutedWidget4.Widget is GroupShape))
				{
					float width = layoutedWidget4.Bounds.Width - ((layoutedWidget4.SubWidth != 0f) ? Convert.ToSingle((float)layoutedWidget4.Spaces * layoutedWidget4.WordSpace) : 0f);
					layoutedWidget4.Bounds = new RectangleF(layoutedWidget3.Bounds.X + layoutedWidget3.Bounds.Width, layoutedWidget4.Bounds.Y, width, layoutedWidget4.Bounds.Height);
				}
			}
		}
		layoutedWidgetList.Clear();
	}

	internal void Draw(IWidgetContainer widget, LayoutedWidget ltWidget)
	{
		DrawImpl(widget, ltWidget);
	}

	internal virtual void DrawImpl(IWidgetContainer widget, LayoutedWidget ltWidget)
	{
	}

	internal void Draw(IWidget widget, LayoutedWidget layoutedWidget)
	{
		if (widget is Entity)
		{
			switch ((widget as Entity).EntityType)
			{
			case EntityType.BookmarkStart:
				Draw(widget as BookmarkStart, layoutedWidget);
				break;
			case EntityType.Picture:
				Draw(widget as WPicture, layoutedWidget);
				break;
			case EntityType.AutoShape:
				Draw(widget as Shape, layoutedWidget);
				break;
			case EntityType.TextBox:
				Draw(widget as WTextBox, layoutedWidget);
				break;
			case EntityType.TextRange:
				Draw(widget as WTextRange, layoutedWidget);
				break;
			case EntityType.Chart:
				Draw(widget as WChart, layoutedWidget);
				break;
			case EntityType.TableRow:
				Draw(widget as WTableRow, layoutedWidget);
				break;
			case EntityType.TableCell:
				Draw(widget as WTableCell, layoutedWidget);
				break;
			case EntityType.Table:
				Draw(widget as WTable, layoutedWidget);
				break;
			case EntityType.Symbol:
				Draw(widget as WSymbol, layoutedWidget);
				break;
			case EntityType.Paragraph:
				Draw(widget as WParagraph, layoutedWidget);
				break;
			case EntityType.OleObject:
				Draw(widget as WOleObject, layoutedWidget);
				break;
			case EntityType.Field:
				Draw(widget as WField, layoutedWidget);
				break;
			case EntityType.DropDownFormField:
				Draw(widget as WDropDownFormField, layoutedWidget);
				break;
			case EntityType.CheckBox:
				Draw(widget as WCheckBox, layoutedWidget);
				break;
			case EntityType.AbsoluteTab:
				Draw(widget as WAbsoluteTab, layoutedWidget);
				break;
			case EntityType.BlockContentControl:
				Draw(widget as BlockContentControl, layoutedWidget);
				break;
			case EntityType.InlineContentControl:
				Draw(widget as InlineContentControl, layoutedWidget);
				break;
			case EntityType.Footnote:
				DrawImpl(widget as WFootnote, layoutedWidget);
				break;
			case EntityType.ChildShape:
				Draw(widget as ChildShape, layoutedWidget);
				break;
			case EntityType.Math:
			{
				MathRenderer mathRenderer = new MathRenderer(this);
				mathRenderer.Draw(widget as WMath, layoutedWidget);
				mathRenderer.Dispose();
				break;
			}
			case EntityType.CommentMark:
				Draw(widget as WCommentMark, layoutedWidget);
				break;
			default:
				DrawImpl(layoutedWidget);
				break;
			}
		}
		else if (widget is SplitWidgetContainer)
		{
			Draw(widget as SplitWidgetContainer, layoutedWidget);
		}
		else if (widget is SplitStringWidget)
		{
			Draw(widget as SplitStringWidget, layoutedWidget);
		}
		else if (widget is SplitTableWidget)
		{
			Draw(widget as SplitTableWidget, layoutedWidget);
		}
		else if (widget is LeafEmtyWidget)
		{
			Draw(widget as LeafEmtyWidget, layoutedWidget);
		}
		else
		{
			DrawImpl(layoutedWidget);
		}
	}

	private bool IsNeedToSkip(IWidget widget)
	{
		bool result = false;
		if (widget is Entity)
		{
			switch ((widget as Entity).EntityType)
			{
			case EntityType.AutoShape:
				if (!(widget as Shape).Visible && (widget as Shape).Rotation != 0f)
				{
					result = true;
				}
				break;
			case EntityType.TextBox:
				if (!(widget as WTextBox).Visible && (widget as WTextBox).TextBoxFormat.Rotation != 0f)
				{
					result = true;
				}
				break;
			case EntityType.GroupShape:
				if (!(widget as GroupShape).Visible)
				{
					result = true;
				}
				break;
			case EntityType.ChildGroupShape:
				if (!(widget as ChildGroupShape).Visible)
				{
					result = true;
				}
				break;
			case EntityType.ChildShape:
				if (!(widget as ChildShape).Visible)
				{
					result = true;
				}
				break;
			}
		}
		return result;
	}

	internal void DrawPageBorder(int pageNumber, PageCollection pageCollection)
	{
		bool flag = false;
		Page page = pageCollection[pageNumber];
		switch (page.Setup.PageBordersApplyType)
		{
		case PageBordersApplyType.AllExceptFirstPage:
			if (pageNumber > 0 && page.Setup.OwnerBase == pageCollection[pageNumber - 1].Setup.OwnerBase)
			{
				flag = true;
			}
			break;
		case PageBordersApplyType.AllPages:
			flag = true;
			break;
		case PageBordersApplyType.FirstPage:
			if (pageNumber == 0 || (pageNumber > 0 && page.Setup.OwnerBase != pageCollection[pageNumber - 1].Setup.OwnerBase))
			{
				flag = true;
			}
			break;
		}
		if (flag && page.PageWidgets.Count != 0)
		{
			DrawPageBorder(page.Setup, page.PageWidgets[0].Bounds, page.PageWidgets[1].Bounds, page.PageWidgets[2].Bounds);
		}
	}

	internal void Draw(BookmarkStart bookmarkStart, LayoutedWidget ltWidget)
	{
		if (!bookmarkStart.Name.StartsWith("_") && (ExportBookmarks & ExportBookmarkType.Bookmarks) != 0)
		{
			Bookmarks.Add(new BookmarkPosition(bookmarkStart.Name, DocumentLayouter.PageNumber, ltWidget.Bounds));
		}
	}

	private bool IsNeedToClip(RectangleF itemBounds)
	{
		if (ClipBoundsContainer == null || ClipBoundsContainer.Count == 0)
		{
			return false;
		}
		RectangleF rectangleF = ClipBoundsContainer.Peek();
		if (Math.Round(itemBounds.X, 2) < Math.Round(rectangleF.X, 2))
		{
			return true;
		}
		if (itemBounds.Y < rectangleF.Y)
		{
			return true;
		}
		if (itemBounds.Right > rectangleF.Right)
		{
			return true;
		}
		if (Math.Round(itemBounds.Bottom) >= Math.Round(rectangleF.Bottom))
		{
			return true;
		}
		return false;
	}

	private Color GetHightLightColor(Color hightLightColor)
	{
		if (hightLightColor == Color.Green)
		{
			return Color.FromArgb(255, 0, 255, 0);
		}
		if (hightLightColor == Color.Gold)
		{
			return Color.FromArgb(255, 128, 128, 0);
		}
		if (hightLightColor.Name == "808080")
		{
			return Color.FromArgb(255, 128, 128, 128);
		}
		return hightLightColor;
	}

	internal bool IsTextRange(WParagraph paragraph)
	{
		for (int i = 0; i < paragraph.ChildEntities.Count; i++)
		{
			Entity entity = paragraph.ChildEntities[i];
			if (formFieldEnd != null)
			{
				if (!paragraph.ChildEntities.Contains(formFieldEnd))
				{
					break;
				}
				i = formFieldEnd.Index;
				formFieldEnd = null;
			}
			else if (entity.EntityType == EntityType.TextFormField || entity.EntityType == EntityType.CheckBox || entity.EntityType == EntityType.DropDownFormField)
			{
				WField wField = entity as WField;
				if (wField.FieldEnd != null && formFieldEnd == null)
				{
					if (wField.FieldEnd.GetOwnerParagraphValue() != paragraph)
					{
						formFieldEnd = wField.FieldEnd;
						break;
					}
					i = wField.FieldEnd.Index;
				}
			}
			else if (entity is WTextRange wTextRange && !(wTextRange.PreviousSibling is WOleObject) && !(wTextRange.m_layoutInfo is TabsLayoutInfo) && !(wTextRange.Text == "") && !StringParser.IsWhitespace(wTextRange.Text))
			{
				return true;
			}
		}
		return false;
	}

	internal void DrawEmptyString(LayoutedWidget layoutedWidget, WParagraph paragraph)
	{
		WCharacterFormat breakCharacterFormat = paragraph.BreakCharacterFormat;
		string text = " ";
		string embedFontStyle = GetEmbedFontStyle(breakCharacterFormat);
		if (!string.IsNullOrEmpty(embedFontStyle))
		{
			embedFontStyle = breakCharacterFormat.GetFontNameToRender(FontScriptType.English) + "_" + embedFontStyle;
		}
		DocGen.Drawing.Font fontToRender = breakCharacterFormat.GetFontToRender(FontScriptType.English);
		PdfFont pdfFont = null;
		if (PdfFontCollection.ContainsKey(GetPdfFontCollectionKey(fontToRender, IsUnicode(text))))
		{
			pdfFont = PdfFontCollection[GetPdfFontCollectionKey(fontToRender, IsUnicode(text))];
		}
		else
		{
			pdfFont = CreatePdfFont(text, WordDocument.RenderHelper.GetFontStream(fontToRender, FontScriptType.English), fontToRender.Size, GetFontStyle(fontToRender.Style));
			pdfFont.Ascent = GetAscent(fontToRender, FontScriptType.English);
			PdfFontCollection.Add(GetPdfFontCollectionKey(fontToRender, IsUnicode(text)), pdfFont);
		}
		if (FallbackFonts != null && FallbackFonts.Count > 0)
		{
			pdfFont = GetFallbackPdfFont(pdfFont, fontToRender, text, FontScriptType.English, new PdfStringFormat());
		}
		PDFGraphics.DrawString(text, pdfFont, PdfBrushes.Black, new PointF(layoutedWidget.Bounds.X, layoutedWidget.Bounds.Y));
		PDFGraphics.ReSetTag();
	}

	internal void Draw(LayoutedWidget layoutedWidget, bool isHaveToInitLayoutInfo)
	{
		if (IsNeedToSkip(layoutedWidget.Widget))
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		if (AutoTag)
		{
			WParagraph ownerParagraph = GetOwnerParagraph(layoutedWidget);
			if (ownerParagraph != null && layoutedWidget.ChildWidgets.Count > 0 && GetOwnerParagraph(layoutedWidget.ChildWidgets[0]) == ownerParagraph && layoutedWidget.ChildWidgets[0].ChildWidgets.Count > 0 && (!(layoutedWidget.ChildWidgets[0].ChildWidgets[0].Widget is Break) || (layoutedWidget.ChildWidgets[0].ChildWidgets[0].Widget as Break).BreakType != BreakType.ColumnBreak))
			{
				PdfStructureElement tag = CreatePdfStructureElement(ownerParagraph, layoutedWidget);
				PDFGraphics.SetTag(tag);
				if (IsTextRange(ownerParagraph))
				{
					flag = true;
				}
				else
				{
					flag2 = true;
				}
			}
		}
		Draw(layoutedWidget.Widget, layoutedWidget);
		Entity entity = ((layoutedWidget.Widget is ParagraphItem) ? (layoutedWidget.Widget as Entity) : ((layoutedWidget.Widget is SplitStringWidget) ? ((layoutedWidget.Widget as SplitStringWidget).RealStringWidget as Entity) : null));
		if (entity != null)
		{
			UpdateBookmarkTargetPosition(entity, layoutedWidget);
			CreateBookmarkRerefeceLink(entity, layoutedWidget);
		}
		if (AutoTag && flag2)
		{
			WParagraph ownerParagraph2 = GetOwnerParagraph(layoutedWidget);
			DrawEmptyString(layoutedWidget, ownerParagraph2);
		}
		int i = 0;
		for (int num = layoutedWidget.ChildWidgets.Count; i < num; i++)
		{
			LayoutedWidget layoutedWidget2 = layoutedWidget.ChildWidgets[i];
			Entity ownerLayoutedWidget = GetOwnerLayoutedWidget(layoutedWidget2);
			WParagraph paragraphWidget = GetParagraphWidget(layoutedWidget2);
			WTableCell cellWidget = GetCellWidget(layoutedWidget2);
			if (AutoTag && layoutedWidget2 != null)
			{
				CreatePdfStructureElement(layoutedWidget2, ownerLayoutedWidget, cellWidget);
			}
			if (IsOverLappedShapeWidget(layoutedWidget2))
			{
				int orderIndex = GetOrderIndex(layoutedWidget2.Widget);
				if (!OverLappedShapeWidgets.ContainsKey(orderIndex))
				{
					OverLappedShapeWidgets.Add(orderIndex, layoutedWidget2);
					if (layoutedWidget2.Widget is WTable)
					{
						AddFloatingItemsOfTable(layoutedWidget2);
					}
					if (AutoTag)
					{
						AutoTagIndex.Add(orderIndex, ++autoTagCount);
					}
				}
				if (layoutedWidget2.Widget is WPicture)
				{
					WPicture picture = layoutedWidget2.Widget as WPicture;
					if (currHyperlink != null)
					{
						AddHyperLink(picture, layoutedWidget2);
					}
				}
				if (!(layoutedWidget2.Widget is WTable))
				{
					layoutedWidget.ChildWidgets.RemoveAt(i);
					i--;
					num--;
				}
			}
			else if (layoutedWidget2.IsBehindWidget())
			{
				if (layoutedWidget2.Widget is WPicture)
				{
					WPicture picture2 = layoutedWidget2.Widget as WPicture;
					if (currHyperlink != null)
					{
						AddHyperLink(picture2, layoutedWidget2);
					}
				}
				layoutedWidget.ChildWidgets.RemoveAt(i);
				if (isHaveToInitLayoutInfo)
				{
					layoutedWidget2.InitLayoutInfoAll();
				}
				i--;
				num--;
			}
			else if (ownerLayoutedWidget is WTableRow)
			{
				Draw(layoutedWidget2, isHaveToInitLayoutInfo);
				if (AutoTag)
				{
					pdfStructureTable.Pop();
				}
			}
			else if (ownerLayoutedWidget is WTable && (ownerLayoutedWidget as WTable).TableFormat.Bidi && layoutedWidget2.Widget is WTableRow)
			{
				layoutedWidget2.ChildWidgets.Reverse();
				Draw(layoutedWidget2, isHaveToInitLayoutInfo);
				if (AutoTag)
				{
					pdfStructureTable.Pop();
				}
			}
			else if (cellWidget != null && ownerLayoutedWidget is WTableCell)
			{
				DocGen.Layouting.Spacings margins = (layoutedWidget2.Widget.LayoutInfo as CellLayoutInfo).Margins;
				RectangleF rectangleF = ((!cellWidget.m_layoutInfo.IsVerticalText) ? new RectangleF(layoutedWidget2.Bounds.X - margins.Left, layoutedWidget2.Bounds.Y, layoutedWidget2.Bounds.Width + margins.Left + margins.Right, layoutedWidget2.Bounds.Height) : new RectangleF(layoutedWidget2.Bounds.X - margins.Bottom, layoutedWidget2.Bounds.Y, layoutedWidget2.Bounds.Width + margins.Top + margins.Bottom, layoutedWidget2.Bounds.Height));
				bool isVerticalText = (ownerLayoutedWidget as IWidget).LayoutInfo.IsVerticalText;
				rectangleF = UpdateClipBounds(rectangleF, isVerticalText);
				if (rectangleF.Width != 0f && rectangleF.Height != 0f)
				{
					ClipBoundsContainer.Push(rectangleF);
					Draw(layoutedWidget2, isHaveToInitLayoutInfo);
					ClipBoundsContainer.Pop();
				}
			}
			else if (layoutedWidget2 != null && paragraphWidget != null && !paragraphWidget.IsInCell && paragraphWidget.ParagraphFormat.IsFrame && !(ownerLayoutedWidget is WParagraph))
			{
				RectangleF bounds = layoutedWidget2.Bounds;
				WParagraphFormat paragraphFormat = paragraphWidget.ParagraphFormat;
				ParagraphLayoutInfo paragraphLayoutInfo = layoutedWidget2.Widget.LayoutInfo as ParagraphLayoutInfo;
				if (!bounds.IsEmpty && (paragraphFormat.FrameHeight != 0f || paragraphFormat.FrameWidth != 0f) && paragraphLayoutInfo != null)
				{
					LayoutedWidget maximumHeightWidget = GetMaximumHeightWidget(layoutedWidget2.ChildWidgets[0]);
					RectangleF frameClipBounds = layoutedWidget.GetFrameClipBounds(bounds, paragraphFormat, paragraphLayoutInfo, maximumHeightWidget?.Bounds.Height ?? 0f);
					frameClipBounds = UpdateClipBounds(frameClipBounds, reverseClipping: false);
					frameClipBounds = GetClipBounds(frameClipBounds, frameClipBounds.Width, 0f);
					if (frameClipBounds.Width != 0f)
					{
						ClipBoundsContainer.Push(frameClipBounds);
						Draw(layoutedWidget2, isHaveToInitLayoutInfo);
						ClipBoundsContainer.Pop();
					}
				}
				else
				{
					Draw(layoutedWidget2, isHaveToInitLayoutInfo);
				}
			}
			else if (layoutedWidget2 != null)
			{
				if (ownerLayoutedWidget is WTextBox || ownerLayoutedWidget is Shape || ownerLayoutedWidget is ChildShape)
				{
					float num2 = 0f;
					bool flag3 = false;
					bool flag4 = false;
					if (ownerLayoutedWidget is WTextBox)
					{
						WTextBox wTextBox = ownerLayoutedWidget as WTextBox;
						num2 = ((wTextBox.Shape != null) ? wTextBox.Shape.Rotation : wTextBox.TextBoxFormat.Rotation);
						flag3 = wTextBox.TextBoxFormat.FlipHorizontal;
						flag4 = wTextBox.TextBoxFormat.FlipVertical;
					}
					else if (ownerLayoutedWidget is Shape)
					{
						Shape obj = ownerLayoutedWidget as Shape;
						num2 = obj.Rotation;
						flag3 = obj.FlipHorizontal;
						flag4 = obj.FlipVertical;
					}
					else if (ownerLayoutedWidget is ChildShape)
					{
						ChildShape obj2 = ownerLayoutedWidget as ChildShape;
						num2 = obj2.RotationToRender;
						flag3 = obj2.FlipHorizantalToRender;
						flag4 = obj2.FlipVerticalToRender;
					}
					if (num2 != 0f || (num2 == 0f && (flag3 || flag4)))
					{
						m_rotateTransform = new RectangleF(layoutedWidget.Bounds.X, layoutedWidget.Bounds.Y, layoutedWidget.Bounds.Width, layoutedWidget.Bounds.Height);
						Draw(layoutedWidget2, isHaveToInitLayoutInfo);
						m_rotateTransform = default(RectangleF);
					}
					else
					{
						Draw(layoutedWidget2, isHaveToInitLayoutInfo);
					}
				}
				else
				{
					Draw(layoutedWidget2, isHaveToInitLayoutInfo);
				}
				if (layoutedWidget2.Widget is Shape || layoutedWidget2.Widget is WTextBox || layoutedWidget2.Widget is GroupShape || layoutedWidget2.Widget is ChildShape)
				{
					currParagraph = (layoutedWidget2.Widget as ParagraphItem).OwnerParagraph;
				}
				if (IsLineItemDrawn(layoutedWidget2) && (underLineValues != null || strikeThroughValues != null))
				{
					LayoutedWidget layoutedWidget3 = layoutedWidget2.ChildWidgets[layoutedWidget2.ChildWidgets.Count - 1];
					if ((layoutedWidget3.Widget is WTextRange || layoutedWidget3.Widget is SplitStringWidget) && !IsRTLParaLine(layoutedWidget2) && !HasTextRangeBidi(layoutedWidget2.ChildWidgets) && layoutedWidget3.Bounds.Width > 0f)
					{
						WTextRange wTextRange = ((layoutedWidget3.Widget is WTextRange) ? ((WTextRange)layoutedWidget3.Widget) : ((layoutedWidget3.Widget as SplitStringWidget).RealStringWidget as WTextRange));
						if (wTextRange != null && i != num - 1)
						{
							WCharacterFormat key = null;
							float num3 = 0f;
							if (underLineValues != null && underLineValues.Count != 0 && wTextRange.CharacterFormat.UnderlineStyle != 0 && wTextRange.CharacterFormat.UnderlineStyle != DocGen.Drawing.UnderlineStyle.Words)
							{
								foreach (WCharacterFormat key2 in underLineValues.Keys)
								{
									key = key2;
								}
								RectangleF value = underLineValues[key][underLineValues[key].Count - 1];
								string text = ((layoutedWidget3.Widget is WTextRange) ? wTextRange.Text : (layoutedWidget3.Widget as SplitStringWidget).SplittedText);
								num3 = GetSpaceWidthAtEndFromText(wTextRange, text);
								if (num3 > 0f)
								{
									value.Width -= num3;
									if (value.Width == 0f)
									{
										List<RectangleF> list = underLineValues[key];
										if (list.Count > 1)
										{
											list.RemoveAt(list.Count - 1);
										}
										else
										{
											underLineValues.Remove(key);
										}
									}
									else
									{
										underLineValues[key][underLineValues[key].Count - 1] = value;
									}
								}
							}
							if (strikeThroughValues != null && strikeThroughValues.Count != 0 && (wTextRange.CharacterFormat.Strikeout || wTextRange.CharacterFormat.DoubleStrike))
							{
								foreach (WCharacterFormat key3 in strikeThroughValues.Keys)
								{
									key = key3;
								}
								RectangleF value2 = strikeThroughValues[key];
								string text2 = ((layoutedWidget3.Widget is WTextRange) ? wTextRange.Text : (layoutedWidget3.Widget as SplitStringWidget).SplittedText);
								num3 = GetSpaceWidthAtEndFromText(wTextRange, text2);
								if (num3 > 0f)
								{
									value2.Width -= num3;
									if (value2.Width == 0f)
									{
										strikeThroughValues.Remove(key);
									}
									else
									{
										strikeThroughValues[key] = value2;
									}
								}
							}
						}
					}
					DrawLine(underLineValues, strikeThroughValues, layoutedWidget2);
				}
				if (!AutoTag)
				{
					continue;
				}
				if (layoutedWidget2.Widget is WTableRow)
				{
					pdfStructureTable.Pop();
				}
				else if (layoutedWidget2.Widget is WTable)
				{
					pdfStructureTable.Pop();
					if (pdfStructureTable.Count == 0)
					{
						pdfStructureTable.Clear();
						pdfStructureTable = null;
					}
				}
			}
			else
			{
				Trace.WriteLine("object is null", "LayoutedWidget.Draw()");
			}
		}
		if (AutoTag && flag)
		{
			PDFGraphics.ReSetTag();
		}
		if (layoutedWidget.Widget is WTable && layoutedWidget.ChildWidgets.Count > 0)
		{
			for (int j = 0; j < layoutedWidget.ChildWidgets.Count; j++)
			{
				LayoutedWidget layoutedWidget4 = layoutedWidget.ChildWidgets[j];
				if ((layoutedWidget.Widget as WTable).TableFormat.Bidi)
				{
					layoutedWidget4.ChildWidgets.Reverse();
				}
				float num4 = 0f;
				for (int k = 0; k < layoutedWidget4.ChildWidgets.Count; k++)
				{
					LayoutedWidget layoutedWidget5 = layoutedWidget4.ChildWidgets[k];
					WTableCell cellWidget2 = GetCellWidget(layoutedWidget5);
					WTableRow wTableRow = GetOwnerLayoutedWidget(layoutedWidget5) as WTableRow;
					bool flag5 = false;
					bool flag6 = false;
					if (cellWidget2 != null)
					{
						Entity owner = cellWidget2.OwnerRow.OwnerTable.Owner;
						if (owner is WTextBody)
						{
							owner = owner.Owner;
						}
						if (owner is WTextBox && (owner as WTextBox).TextBoxFormat.Rotation != 0f)
						{
							flag5 = true;
							WTextBoxFormat textBoxFormat = (owner as WTextBox).TextBoxFormat;
							SetRotateTransform(m_rotateTransform, textBoxFormat.Rotation, textBoxFormat.FlipVertical, textBoxFormat.FlipHorizontal);
						}
						else if (owner is Shape && (owner as Shape).Rotation != 0f)
						{
							Shape shape = owner as Shape;
							flag5 = true;
							SetRotateTransform(m_rotateTransform, shape.Rotation, shape.FlipVertical, shape.FlipHorizontal);
						}
						else if (owner is ChildShape && (owner as ChildShape).Rotation != 0f)
						{
							ChildShape childShape = owner as ChildShape;
							RectangleF textLayoutingBounds = childShape.TextLayoutingBounds;
							if (childShape.AutoShapeType == DocGen.DocIO.DLS.AutoShapeType.Rectangle)
							{
								textLayoutingBounds.Height = GetLayoutedTextBoxContentHeight(layoutedWidget5);
							}
							flag5 = true;
							Rotate(childShape, childShape.Rotation, childShape.FlipVertical, childShape.FlipHorizantal, textLayoutingBounds);
						}
						if (ClipBoundsContainer != null && ClipBoundsContainer.Count > 0)
						{
							flag6 = true;
							RectangleF rectangleF2 = ClipBoundsContainer.Peek();
							if (wTableRow.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013 && IsTableInTextBoxOrShape(layoutedWidget5.Widget as Entity, checkTextBoxOnly: false))
							{
								WTable table = wTableRow.Owner as WTable;
								rectangleF2 = TextboxClipBounds(table, rectangleF2);
							}
							SetClip(rectangleF2);
						}
						DrawCellBorders(cellWidget2, layoutedWidget5, num4);
						foreach (float item in new List<float>((layoutedWidget5.Widget.LayoutInfo as CellLayoutInfo).UpdatedTopBorders.Values))
						{
							num4 += item;
						}
					}
					if (flag5)
					{
						ResetTransform();
					}
					if (flag6)
					{
						ResetClip();
					}
				}
			}
		}
		if (!IsNeedToRemoveClipBounds(layoutedWidget.Widget))
		{
			return;
		}
		if (layoutedWidget.Widget is WTable)
		{
			WParagraphFormat paragraphFormat2 = (layoutedWidget.Widget as WTable).Rows[0].Cells[0].Paragraphs[0].ParagraphFormat;
			if (paragraphFormat2.FrameWidth != 0f || paragraphFormat2.FrameHeight != 0f)
			{
				ClipBoundsContainer.Pop();
			}
		}
		else
		{
			ClipBoundsContainer.Pop();
		}
	}

	private void AddFloatingItemsOfTable(LayoutedWidget layoutedWidget)
	{
		if (!(layoutedWidget.Widget is WTable) || layoutedWidget.ChildWidgets.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < layoutedWidget.ChildWidgets.Count; i++)
		{
			LayoutedWidget layoutedWidget2 = layoutedWidget.ChildWidgets[i];
			for (int j = 0; j < layoutedWidget2.ChildWidgets.Count; j++)
			{
				LayoutedWidget layoutedWidget3 = layoutedWidget2.ChildWidgets[j];
				for (int k = 0; k < layoutedWidget3.ChildWidgets.Count; k++)
				{
					LayoutedWidget layoutedWidget4 = layoutedWidget3.ChildWidgets[k];
					for (int l = 0; l < layoutedWidget4.ChildWidgets.Count; l++)
					{
						LayoutedWidget layoutedWidget5 = layoutedWidget4.ChildWidgets[l];
						if (layoutedWidget5.Widget is WTable)
						{
							AddFloatingItemsOfTable(layoutedWidget5);
							AddFloatingItemsInCollection(layoutedWidget5);
						}
						else if (layoutedWidget5.Widget is BlockContentControl)
						{
							for (int m = 0; m < layoutedWidget5.ChildWidgets.Count; m++)
							{
								LayoutedWidget para_LtWidget = layoutedWidget5.ChildWidgets[m];
								IterateChildElements(para_LtWidget);
							}
						}
						else
						{
							IterateChildElements(layoutedWidget5);
						}
					}
				}
			}
		}
	}

	private void IterateChildElements(LayoutedWidget para_LtWidget)
	{
		for (int i = 0; i < para_LtWidget.ChildWidgets.Count; i++)
		{
			LayoutedWidget layoutedWidget = para_LtWidget.ChildWidgets[i];
			for (int j = 0; j < layoutedWidget.ChildWidgets.Count; j++)
			{
				LayoutedWidget ltWidget = layoutedWidget.ChildWidgets[j];
				AddFloatingItemsInCollection(ltWidget);
			}
		}
	}

	private void AddFloatingItemsInCollection(LayoutedWidget ltWidget)
	{
		if (!IsOverLappedShapeWidget(ltWidget))
		{
			return;
		}
		int orderIndex = GetOrderIndex(ltWidget.Widget);
		if (!OverLappedShapeWidgets.ContainsKey(orderIndex))
		{
			OverLappedShapeWidgets.Add(orderIndex, ltWidget);
			if (AutoTag)
			{
				AutoTagIndex.Add(orderIndex, ++autoTagCount);
			}
		}
	}

	private float GetSpaceWidthAtEndFromText(WTextRange textRange, string text)
	{
		if (text.EndsWith(" "))
		{
			float width = MeasureTextRange(textRange, text).Width;
			string text2 = text.TrimEnd();
			float width2 = MeasureTextRange(textRange, text2).Width;
			if (width > width2)
			{
				return width - width2;
			}
		}
		return 0f;
	}

	private bool IsRTLParaLine(LayoutedWidget m_currChildLW)
	{
		bool result = false;
		if (m_currChildLW.Widget is WParagraph || (m_currChildLW.Widget is SplitWidgetContainer && (m_currChildLW.Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph))
		{
			result = ((!(m_currChildLW.Widget is WParagraph)) ? ((m_currChildLW.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph).ParagraphFormat.Bidi : (m_currChildLW.Widget as WParagraph).ParagraphFormat.Bidi);
		}
		return result;
	}

	private bool HasTextRangeBidi(LayoutedWidgetList layoutedWidgets)
	{
		foreach (LayoutedWidget layoutedWidget in layoutedWidgets)
		{
			if ((layoutedWidget.Widget is WTextRange || layoutedWidget.Widget is SplitStringWidget) && ((layoutedWidget.Widget is SplitStringWidget) ? ((layoutedWidget.Widget as SplitStringWidget).RealStringWidget as WTextRange) : (layoutedWidget.Widget as WTextRange)).CharacterFormat.Bidi)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsLineItemDrawn(LayoutedWidget ltWidget)
	{
		if (ltWidget.ChildWidgets.Count > 0 && ((ltWidget.Widget is WParagraph && (ltWidget.ChildWidgets[0].Widget is ParagraphItem || ltWidget.ChildWidgets[0].Widget is SplitStringWidget)) || (ltWidget.Widget is SplitWidgetContainer && (ltWidget.Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph && (ltWidget.ChildWidgets[0].Widget is ParagraphItem || ltWidget.ChildWidgets[0].Widget is SplitStringWidget))))
		{
			return true;
		}
		return false;
	}

	private void DrawLine(Dictionary<WCharacterFormat, List<RectangleF>> underLineValues, Dictionary<WCharacterFormat, RectangleF> strikeThroughValues, LayoutedWidget ltWidget)
	{
		if (underLineValues != null)
		{
			foreach (KeyValuePair<WCharacterFormat, List<RectangleF>> underLineValue in underLineValues)
			{
				List<RectangleF> value = underLineValue.Value;
				WCharacterFormat key = underLineValue.Key;
				bool isNeedToScale = false;
				bool isRotateTransformApplied = false;
				bool isNeedToClip = false;
				TransformGraphics(key, ref isNeedToScale, ref isRotateTransformApplied, ref isNeedToClip, ltWidget);
				foreach (RectangleF item in value)
				{
					DrawUnderLine(key, item);
				}
				ResetTransform();
				if (isNeedToClip)
				{
					ResetClip();
				}
			}
			underLineValues.Clear();
			underLineValues = null;
		}
		if (strikeThroughValues == null)
		{
			return;
		}
		foreach (KeyValuePair<WCharacterFormat, RectangleF> strikeThroughValue in strikeThroughValues)
		{
			RectangleF value2 = strikeThroughValue.Value;
			WCharacterFormat key2 = strikeThroughValue.Key;
			bool isNeedToScale2 = false;
			bool isRotateTransformApplied2 = false;
			bool isNeedToClip2 = false;
			TransformGraphics(key2, ref isNeedToScale2, ref isRotateTransformApplied2, ref isNeedToClip2, ltWidget);
			DrawStrikeThrough(key2, value2);
			ResetTransform();
			if (isNeedToClip2)
			{
				ResetClip();
			}
		}
		strikeThroughValues.Clear();
		strikeThroughValues = null;
	}

	private void TransformGraphics(WCharacterFormat characterFormat, ref bool isNeedToScale, ref bool isRotateTransformApplied, ref bool isNeedToClip, LayoutedWidget ltWidget)
	{
		float scaling = characterFormat.Scaling;
		isNeedToScale = scaling != 100f && (scaling >= 1f || scaling <= 600f);
		scaling /= 100f;
		if (scaling == 0f)
		{
			scaling = 1f;
		}
		PointF translatePoints = PointF.Empty;
		float rotationAngle = 0f;
		float ang = 0f;
		bool flipH = false;
		bool flipV = false;
		bool doNotRotateText = false;
		bool hasCommentsHighlighter = false;
		TextWrappingStyle textWrappingStyle = TextWrappingStyle.Square;
		WTextRange wTextRange = ((characterFormat.OwnerBase is WTextRange) ? (characterFormat.OwnerBase as WTextRange) : null);
		WSymbol wSymbol = ((characterFormat.OwnerBase is WSymbol) ? (characterFormat.OwnerBase as WSymbol) : null);
		GetTextboxOrShapesRotationValue(ref ang, ref flipH, ref flipV, ref doNotRotateText, ref textWrappingStyle, ref hasCommentsHighlighter, wTextRange);
		Rotate(ang, ref flipV, ref flipH, textWrappingStyle, doNotRotateText, isNeedToScale, ref isRotateTransformApplied, ref rotationAngle);
		if ((wTextRange != null && wTextRange.m_layoutInfo != null && wTextRange.m_layoutInfo.IsVerticalText) || (wSymbol != null && wSymbol.m_layoutInfo != null && wSymbol.m_layoutInfo.IsVerticalText))
		{
			TransformGraphicsPosition(ltWidget, isNeedToScale, ref isRotateTransformApplied, ref translatePoints, ref rotationAngle, currParagraph);
		}
		if (isNeedToScale)
		{
			ScaleTransformMatrix(scaling, translatePoints, rotationAngle, flipV, flipH);
		}
		RectangleF bounds = ltWidget.Bounds;
		float clipTopPosition = GetClipTopPosition(bounds, isInlinePicture: false);
		RectangleF rectangleF = GetClipBounds(bounds, bounds.Width, clipTopPosition);
		isNeedToClip = IsNeedToClip(rectangleF);
		if (ClipBoundsContainer != null && ClipBoundsContainer.Count > 0)
		{
			RectangleF rectangleF2 = ClipBoundsContainer.Peek();
			if (currParagraph != null && currParagraph.GetOwnerEntity() is WTextBox)
			{
				WParagraph ownerParagraph = (currParagraph.GetOwnerEntity() as WTextBox).OwnerParagraph;
				if (ownerParagraph != null && ownerParagraph.ParagraphFormat.IsInFrame())
				{
					rectangleF2 = GetTextBoxWidget(ltWidget)?.Bounds ?? rectangleF2;
				}
				isNeedToClip = !currParagraph.IsInCell && (Math.Round(rectangleF.Bottom, 2) >= Math.Round(rectangleF2.Bottom, 2) || rectangleF.X < rectangleF2.X);
			}
		}
		if (isNeedToClip)
		{
			rectangleF = ((!(clipTopPosition > 0f)) ? ClipBoundsContainer.Peek() : UpdateClipBounds(rectangleF, reverseClipping: false));
			if (isNeedToScale)
			{
				rectangleF = new RectangleF(rectangleF.X / scaling, rectangleF.Y, rectangleF.Width / scaling, rectangleF.Height);
			}
			SetClip(rectangleF);
		}
		else if (clipTopPosition > 0f)
		{
			if (isNeedToScale)
			{
				rectangleF = new RectangleF(rectangleF.X / scaling, rectangleF.Y, rectangleF.Width / scaling, rectangleF.Height);
			}
			SetClip(rectangleF);
			isNeedToClip = true;
		}
	}

	private void GetTextboxOrShapesRotationValue(ref float ang, ref bool flipH, ref bool flipV, ref bool doNotRotateText, ref TextWrappingStyle textWrappingStyle, ref bool hasCommentsHighlighter, WTextRange textRange)
	{
		if (textRange == null)
		{
			return;
		}
		WParagraph wParagraph = textRange.Owner as WParagraph;
		if (wParagraph == null)
		{
			wParagraph = textRange.GetOwnerParagraphValue();
		}
		if (wParagraph == null)
		{
			return;
		}
		Entity ownerEntity = wParagraph.GetOwnerEntity();
		if (ownerEntity is ChildShape)
		{
			ChildShape childShape = ownerEntity as ChildShape;
			ang = childShape.RotationToRender;
			flipH = childShape.FlipHorizantalToRender;
			flipV = childShape.FlipVerticalToRender;
			doNotRotateText = childShape.TextFrame.Upright;
			hasCommentsHighlighter = false;
		}
		else if (ownerEntity is Shape)
		{
			Shape shape = ownerEntity as Shape;
			ang = shape.Rotation;
			flipH = shape.FlipHorizontal;
			flipV = shape.FlipVertical;
			textWrappingStyle = shape.WrapFormat.TextWrappingStyle;
			doNotRotateText = shape.TextFrame.Upright;
			hasCommentsHighlighter = false;
		}
		else if (ownerEntity is WTextBox)
		{
			WTextBox wTextBox = ownerEntity as WTextBox;
			ang = ((wTextBox.Shape != null) ? wTextBox.Shape.Rotation : wTextBox.TextBoxFormat.Rotation);
			flipH = wTextBox.TextBoxFormat.FlipHorizontal;
			flipV = wTextBox.TextBoxFormat.FlipVertical;
			textWrappingStyle = wTextBox.TextBoxFormat.TextWrappingStyle;
			doNotRotateText = wTextBox.Shape != null && wTextBox.Shape.TextFrame.Upright;
			hasCommentsHighlighter = false;
		}
		else if (ownerEntity is WTableCell)
		{
			Entity entity = ownerEntity as WTableCell;
			while (entity is WTableCell)
			{
				entity = (entity as WTableCell).OwnerRow.OwnerTable.Owner;
			}
			if (entity.Owner is WTextBox)
			{
				WTextBox wTextBox2 = entity.Owner as WTextBox;
				ang = ((wTextBox2.Shape != null) ? wTextBox2.Shape.Rotation : wTextBox2.TextBoxFormat.Rotation);
				flipH = wTextBox2.TextBoxFormat.FlipHorizontal;
				flipV = wTextBox2.TextBoxFormat.FlipVertical;
				textWrappingStyle = wTextBox2.TextBoxFormat.TextWrappingStyle;
				doNotRotateText = wTextBox2.Shape != null && wTextBox2.Shape.TextFrame.Upright;
				hasCommentsHighlighter = false;
			}
			else if (entity.Owner is Shape)
			{
				Shape shape2 = entity.Owner as Shape;
				ang = shape2.Rotation;
				flipH = shape2.FlipHorizontal;
				flipV = shape2.FlipVertical;
				textWrappingStyle = shape2.WrapFormat.TextWrappingStyle;
				doNotRotateText = shape2.TextFrame.Upright;
				hasCommentsHighlighter = false;
			}
			else if (entity.Owner is ChildShape)
			{
				ChildShape childShape2 = entity.Owner as ChildShape;
				ang = childShape2.RotationToRender;
				flipH = childShape2.FlipHorizantalToRender;
				flipV = childShape2.FlipVerticalToRender;
				doNotRotateText = childShape2.TextFrame.Upright;
				hasCommentsHighlighter = false;
			}
		}
	}

	private bool IsNeedToChangeUnderLineWidth(string fontName)
	{
		switch (fontName)
		{
		case "Arial":
		case "Times New Roman":
		case "Century Gothic":
		case "Cambria":
		case "Verdana":
			return true;
		default:
			return false;
		}
	}

	private void DrawUnderLine(WCharacterFormat characterFormat, RectangleF textBounds)
	{
		float x = textBounds.X;
		float y = textBounds.Y;
		DocGen.Drawing.Font font = characterFormat.Font;
		float num = ((characterFormat.SubSuperScript != 0) ? (characterFormat.GetFontSizeToRender() / 1.5f) : characterFormat.GetFontSizeToRender());
		if (num == 0f)
		{
			num = 0.5f;
		}
		DocGen.Drawing.Font font2 = characterFormat.Document.FontSettings.GetFont(font.Name, num, font.Style, FontScriptType.English);
		if (FallbackFonts != null && FallbackFonts.Count > 0)
		{
			font2 = WordDocument.RenderHelper.GetFallbackFont(font2, (characterFormat.OwnerBase as WTextRange).Text, (characterFormat.OwnerBase as WTextRange).ScriptType, FallbackFonts, FontStreams);
		}
		float num2 = font2.Size / 15f;
		float ascent = GetAscent(font2, FontScriptType.English);
		float descent = GetDescent(font2, FontScriptType.English);
		float num3 = ascent + descent - 2f * num2;
		if (num3 - ascent < num2)
		{
			num3 = ascent + descent - num2;
		}
		y += num3;
		if (characterFormat.Bold && IsNeedToChangeUnderLineWidth(font2.Name))
		{
			num2 = font2.Size / 9.5f;
			y = textBounds.Y + GetAscent(font2, FontScriptType.English) + GetDescent(font2, FontScriptType.English) - 0.65f * num2;
		}
		if (characterFormat.UnderlineStyle == DocGen.Drawing.UnderlineStyle.DashLongHeavy)
		{
			y = textBounds.Y + GetAscent(font2, FontScriptType.English) + 1.5f * num2;
		}
		else if (characterFormat.UnderlineStyle == DocGen.Drawing.UnderlineStyle.DashHeavy || characterFormat.UnderlineStyle == DocGen.Drawing.UnderlineStyle.DotDashHeavy)
		{
			y = textBounds.Y + GetAscent(font2, FontScriptType.English) + 1.75f * num2;
		}
		else if (characterFormat.UnderlineStyle == DocGen.Drawing.UnderlineStyle.Double)
		{
			y = textBounds.Y + GetAscent(font2, FontScriptType.English) + 1.25f * num2;
		}
		PointF[] points = new PointF[2]
		{
			new PointF(x, y),
			new PointF(x + textBounds.Width, y)
		};
		float[] compoundArray = new float[4] { 0f, 0.21f, 0.79f, 1f };
		PdfPen pen = CreatePen(characterFormat, num2);
		if (characterFormat.UnderlineStyle == DocGen.Drawing.UnderlineStyle.Double)
		{
			DrawCompoundLine(compoundArray, points, pen, PDFGraphics);
		}
		else if (characterFormat.UnderlineStyle == DocGen.Drawing.UnderlineStyle.Wavy || characterFormat.UnderlineStyle == DocGen.Drawing.UnderlineStyle.WavyHeavy)
		{
			PDFGraphics.DrawPath(pen, CreateWavyPath(characterFormat, textBounds, font));
		}
		else if (characterFormat.UnderlineStyle == DocGen.Drawing.UnderlineStyle.WavyDouble)
		{
			PDFGraphics.DrawPath(pen, CreateWavyPath(characterFormat, textBounds, font));
			PDFGraphics.DrawPath(pen, CreateWavyPath(characterFormat, new RectangleF(textBounds.X, textBounds.Y + num2 / 1.8f, textBounds.Width, textBounds.Height), font));
		}
		else
		{
			PDFGraphics.DrawLine(pen, new PointF(x, y), new PointF(x + textBounds.Width, y));
		}
	}

	private void DrawCompoundLine(float[] compoundArray, PointF[] points, PdfPen pen, PdfGraphics PDFGraphics)
	{
		float num = 0f;
		bool flag = false;
		float width = pen.Width;
		if (points.Length > 1 && compoundArray.Length != 0 && points[0].X == points[1].X)
		{
			flag = true;
		}
		for (int i = 0; i < compoundArray.Length; i += 2)
		{
			float num2 = width;
			pen.Width = (compoundArray[i + 1] - compoundArray[i]) * num2;
			if (!flag)
			{
				PDFGraphics.DrawLine(pen, points[0].X, points[0].Y + num, points[1].X, points[1].Y + num);
			}
			else
			{
				PDFGraphics.DrawLine(pen, points[0].X + num, points[0].Y, points[1].X + num, points[1].Y);
			}
			if (i + 1 < compoundArray.Length - 1)
			{
				num += (compoundArray[i + 2] - compoundArray[i + 1]) * num2 + pen.Width;
			}
		}
	}

	private PdfPath CreateWavyPath(WCharacterFormat characterFormat, RectangleF bounds, DocGen.Drawing.Font font)
	{
		PdfPath pdfPath = new PdfPath();
		int num = (int)bounds.Width / 3;
		DocGen.Drawing.Font font2 = ((characterFormat.SubSuperScript != 0) ? characterFormat.Document.FontSettings.GetFont(font.Name, font.Size / 1.5f, font.Style, FontScriptType.English) : font);
		float num2 = bounds.X;
		float num3 = bounds.Y + (float)(int)font2.Size + 0.5f;
		float num4 = bounds.Width / (float)num;
		float num5 = ((characterFormat.UnderlineStyle == DocGen.Drawing.UnderlineStyle.WavyDouble) ? ((float)Math.Sqrt(font2.Size) / 9f) : ((float)Math.Sqrt(font2.Size) / 4f));
		for (int i = 0; i < num; i++)
		{
			PointF startPoint = new PointF(num2, num3);
			PointF pointF = new PointF(num2 + num4 * 0.25f, num3 + num5);
			PointF secondControlPoint = pointF;
			PointF pointF2 = new PointF(num2 + num4 * 0.5f, num3);
			pdfPath.AddBezier(startPoint, pointF, secondControlPoint, pointF2);
			startPoint = pointF2;
			pointF = new PointF(num2 + num4 * 0.75f, num3 - num5);
			secondControlPoint = pointF;
			pointF2 = new PointF(num2 + num4, num3);
			pdfPath.AddBezier(startPoint, pointF, secondControlPoint, pointF2);
			num2 += num4;
		}
		return pdfPath;
	}

	private void DrawStrikeThrough(WCharacterFormat characterFormat, RectangleF textBounds)
	{
		float x = textBounds.X;
		float y = textBounds.Y;
		DocGen.Drawing.Font font = characterFormat.Font;
		DocGen.Drawing.Font font2 = ((characterFormat.SubSuperScript != 0) ? characterFormat.Document.FontSettings.GetFont(font.Name, font.Size / 1.5f, font.Style, FontScriptType.English) : font);
		if (FallbackFonts != null && FallbackFonts.Count > 0)
		{
			font2 = WordDocument.RenderHelper.GetFallbackFont(font2, (characterFormat.OwnerBase as WTextRange).Text, (characterFormat.OwnerBase as WTextRange).ScriptType, FallbackFonts, FontStreams);
		}
		float num = font2.Size / 15f;
		if (characterFormat.Strikeout)
		{
			y += GetAscent(font2, FontScriptType.English) - GetDescent(font2, FontScriptType.English);
			PdfPen pen = CreatePen(characterFormat, num);
			PDFGraphics.DrawLine(pen, new PointF(x, y), new PointF(x + textBounds.Width, y));
		}
		else if (characterFormat.DoubleStrike)
		{
			x = textBounds.X;
			y = textBounds.Y;
			y += GetAscent(font2, FontScriptType.English) - (GetDescent(font2, FontScriptType.English) + num);
			PointF start = new PointF(x, y);
			PointF end = new PointF(x + textBounds.Width, y);
			DrawDoubleLine(characterFormat, BorderStyle.Double, num, start, end);
		}
	}

	private PdfPen CreatePen(WCharacterFormat charFormat, float lineWidth)
	{
		Color textColor = GetTextColor(charFormat);
		Color lineColor = Color.Black;
		if (textColor != Color.Black)
		{
			lineColor = textColor;
		}
		if (!charFormat.UnderlineColor.IsEmpty && charFormat.UnderlineColor.ToArgb() != 0 && charFormat.UnderlineColor != Color.Black)
		{
			lineColor = charFormat.UnderlineColor;
		}
		return GetPen(charFormat.UnderlineStyle, lineWidth, lineColor);
	}

	private bool IsNeedToRemoveClipBounds(IWidget widget)
	{
		if (ClipBoundsContainer == null || ClipBoundsContainer.Count == 0)
		{
			return false;
		}
		if (widget is ChildShape)
		{
			EntityType elementType = (widget as ChildShape).ElementType;
			if (elementType == EntityType.TextBox || elementType == EntityType.AutoShape)
			{
				return true;
			}
		}
		if (!(widget is WTextBox) && !(widget is Shape))
		{
			if (widget is WTable)
			{
				return (widget as WTable).IsFrame;
			}
			return false;
		}
		return true;
	}

	private RectangleF TextboxClipBounds(WTable table, RectangleF clipBounds)
	{
		float num = 0f;
		float num2 = 0f;
		num = table.Rows[0].Cells[0].GetLeftPadding();
		for (int i = 0; i < table.Rows.Count; i++)
		{
			if (table.Rows[i].Cells.Count > 0 && table.Rows[i].Cells.Count - 1 < table.Rows[i].Cells.Count)
			{
				float rightPadding = table.Rows[i].Cells[table.Rows[i].Cells.Count - 1].GetRightPadding();
				if (rightPadding > num2)
				{
					num2 = rightPadding;
				}
			}
		}
		RectangleF result = new RectangleF(clipBounds.X, clipBounds.Y, clipBounds.Width, clipBounds.Height);
		result.X -= num;
		result.Width += num + num2;
		return result;
	}

	private RectangleF UpdateClipBounds(RectangleF clipBounds, bool reverseClipping)
	{
		if (ClipBoundsContainer == null)
		{
			ClipBoundsContainer = new Stack<RectangleF>();
		}
		if (ClipBoundsContainer.Count > 0)
		{
			RectangleF ownerClipBounds = ClipBoundsContainer.Peek();
			if (reverseClipping)
			{
				ownerClipBounds = new RectangleF(ownerClipBounds.X, ownerClipBounds.Y, ownerClipBounds.Height, ownerClipBounds.Width);
				clipBounds = UpdateClipBounds(clipBounds, ownerClipBounds);
			}
			else
			{
				clipBounds = UpdateClipBoundsBasedOnOwner(clipBounds, ownerClipBounds);
			}
		}
		return clipBounds;
	}

	private void SetClip(RectangleF clippingBounds)
	{
		if (clippingBounds != RectangleF.Empty)
		{
			PDFGraphics.SetClip(GetClipBounds(clippingBounds, clippingBounds.Width, 0f), CombineMode.Replace);
		}
	}

	private void ResetClip()
	{
		PDFGraphics.ResetClip();
	}

	internal void ResetTransform()
	{
		PDFGraphics.ResetTransform();
	}

	private void SetRotateTransform(RectangleF rotateBounds, float rotation, bool flipV, bool flipH)
	{
		PDFGraphics.Transform = GetTransformMatrix(rotateBounds, rotation, flipH, flipV);
	}

	public void SetScaleTransform(float sx, float sy)
	{
		Matrix matrix = new Matrix();
		matrix.Scale(sx, sy);
		PDFGraphics.Transform = matrix;
	}

	private RectangleF GetClippingBounds(LayoutedWidget cellltWidget)
	{
		IWidget widget = cellltWidget.Widget;
		if (widget is WTableCell && (widget as WTableCell).Index == 0)
		{
			WParagraph wParagraph = (((widget as WTableCell).ChildEntities.Count > 0) ? ((widget as WTableCell).ChildEntities[0] as WParagraph) : null);
			if (wParagraph != null && wParagraph.ParagraphFormat.IsFrame && (wParagraph.ParagraphFormat.FrameWidth > 0f || wParagraph.ParagraphFormat.FrameHeight > 0f))
			{
				ushort num = (ushort)(wParagraph.ParagraphFormat.FrameHeight * 20f);
				bool num2 = (num & 0x8000) != 0;
				float num3 = wParagraph.ParagraphFormat.FrameWidth;
				float num4 = (num2 ? 0f : ((float)((num & 0x7FFF) / 20)));
				LayoutedWidget firstItemInFrame = GetFirstItemInFrame(cellltWidget.Owner.Owner.Owner.ChildWidgets, cellltWidget.Owner.Owner.Owner.ChildWidgets.IndexOf(cellltWidget.Owner.Owner), wParagraph.ParagraphFormat);
				float x;
				if (num3 <= 0f)
				{
					num3 = cellltWidget.Owner.Owner.Bounds.Width;
					x = cellltWidget.Owner.Owner.Bounds.X;
				}
				else
				{
					x = firstItemInFrame.Bounds.X;
				}
				float y;
				if (num4 <= 0f)
				{
					num4 = cellltWidget.Owner.Owner.Bounds.Height;
					y = cellltWidget.Owner.Owner.Bounds.Y;
				}
				else
				{
					y = firstItemInFrame.Bounds.Y;
				}
				return new RectangleF(x, y, num3, num4);
			}
		}
		return RectangleF.Empty;
	}

	private void Draw(WParagraph paragraph, LayoutedWidget ltWidget)
	{
		DrawImpl(ltWidget);
		currParagraph = paragraph;
		bool flag = ltWidget.ChildWidgets.Count > 0 && ltWidget.ChildWidgets[0].Widget == paragraph;
		if (ltWidget.Widget is WParagraph || (ltWidget.Widget is SplitWidgetContainer && (ltWidget.Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph))
		{
			WParagraph wParagraph = ((ltWidget.Widget is WParagraph) ? (ltWidget.Widget as WParagraph) : ((ltWidget.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph));
			if ((ltWidget.IsTrackChanges || wParagraph.IsInsertRevision || wParagraph.IsDeleteRevision || (flag && (wParagraph.IsChangedPFormat || wParagraph.BreakCharacterFormat.IsChangedFormat))) && paragraph.Document.RevisionOptions.ShowRevisionBars)
			{
				PointF start = new PointF(0f, 0f);
				PointF end = new PointF(0f, 0f);
				float num = 0f;
				if (wParagraph.GetOwnerSection(wParagraph) is WSection)
				{
					num = (wParagraph.GetOwnerSection(wParagraph) as WSection).PageSetup.Margins.Left;
				}
				if ((wParagraph.OwnerTextBody.Owner is Shape || wParagraph.OwnerTextBody.Owner is WTextBox) && (wParagraph.OwnerTextBody.Owner as ParagraphItem).IsInsertRevision && !(wParagraph.OwnerTextBody.Owner as ParagraphItem).IsFloatingItem(isTextWrapAround: false))
				{
					LayoutedWidget layoutedWidget = ltWidget;
					while (layoutedWidget != null && !(layoutedWidget.Widget is WTextBox) && !(layoutedWidget.Widget is Shape))
					{
						layoutedWidget = layoutedWidget.Owner;
					}
					if (layoutedWidget != null)
					{
						start = new PointF(num / 2f, layoutedWidget.Bounds.Y);
						end = new PointF(num / 2f, layoutedWidget.Bounds.Y + layoutedWidget.Bounds.Height);
					}
				}
				else
				{
					LayoutedWidget owner = ltWidget.Owner;
					bool flag2 = false;
					bool flag3 = false;
					if (owner.ChildWidgets.Count == 1)
					{
						flag2 = true;
						flag3 = true;
					}
					else if (owner.ChildWidgets[0] == ltWidget)
					{
						flag2 = true;
					}
					else if (owner.ChildWidgets[owner.ChildWidgets.Count - 1] == ltWidget)
					{
						flag3 = true;
					}
					float num2 = ((wParagraph.PreviousSibling == null) ? 0f : ((wParagraph.PreviousSibling is WParagraph) ? (wParagraph.PreviousSibling as WParagraph).ParagraphFormat.AfterSpacing : 0f));
					float num3 = ((wParagraph.ParagraphFormat.BeforeSpacing > num2) ? (wParagraph.ParagraphFormat.BeforeSpacing - num2) : 0f);
					float num4 = (flag2 ? (num3 + wParagraph.ParagraphFormat.Borders.Top.GetLineWidthValue() + wParagraph.ParagraphFormat.Borders.Top.Space) : 0f);
					float num5 = (flag3 ? ((wParagraph.IsInCell ? 0f : wParagraph.ParagraphFormat.AfterSpacing) + wParagraph.ParagraphFormat.Borders.Bottom.GetLineWidthValue() + wParagraph.ParagraphFormat.Borders.Bottom.Space) : 0f);
					start = new PointF(num / 2f, ltWidget.Bounds.Y - num4);
					float num6 = ((wParagraph.IsChangedPFormat && flag) ? ltWidget.ChildWidgets[0].Bounds.Height : ltWidget.Bounds.Height);
					end = new PointF(num / 2f, ltWidget.Bounds.Y + num6 + num5);
				}
				RevisionOptions revisionOptions = wParagraph.Document.RevisionOptions;
				DrawRevisionMark(start, end, GetRevisionColor(revisionOptions.RevisionBarsColor), revisionOptions.RevisionMarkWidth);
			}
		}
		if (ltWidget.Widget is SplitWidgetContainer && (ltWidget.Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph)
		{
			WParagraph wParagraph2 = (ltWidget.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph;
			int count = wParagraph2.ChildEntities.Count;
			int num7 = count - 1 - (ltWidget.Widget as SplitWidgetContainer).Count;
			if (!paragraph.IsInCell && count > (ltWidget.Widget as SplitWidgetContainer).Count && num7 >= 0 && num7 < count && wParagraph2.ChildEntities[num7] is Break && wParagraph2.ChildEntities[num7] is Break @break && (@break.BreakType == BreakType.PageBreak || @break.BreakType == BreakType.ColumnBreak) && !paragraph.SplitWidgetContainerDrawn)
			{
				flag = true;
				paragraph.SplitWidgetContainerDrawn = true;
			}
		}
		if (flag)
		{
			DrawParagraph(paragraph, ltWidget);
		}
		else if (IsLinesInteresectWithFloatingItems(ltWidget, isLineContainer: false))
		{
			WParagraphFormat paragraphFormat = paragraph.ParagraphFormat;
			bool isEmpty = paragraphFormat.BackColor.IsEmpty;
			if (paragraphFormat.TextureStyle != 0 || !isEmpty)
			{
				DrawLineBackGroundColors(paragraph, ltWidget);
			}
		}
		flag = ltWidget.ChildWidgets.Count > 0 && ((ltWidget.ChildWidgets[0].Widget is SplitWidgetContainer) ? ((ltWidget.ChildWidgets[0].Widget as SplitWidgetContainer).RealWidgetContainer == paragraph) : (ltWidget.ChildWidgets[0].Widget == paragraph));
		RevisionOptions revisionOptions2 = paragraph.Document.RevisionOptions;
		if (flag || revisionOptions2.CommentDisplayMode != CommentDisplayMode.ShowInBalloons || ltWidget.ChildWidgets.Count <= 0)
		{
			return;
		}
		Entity baseEntity = GetBaseEntity(paragraph);
		if (!(baseEntity is Shape) && !(baseEntity is WTextBox) && !(baseEntity is GroupShape) && !(baseEntity is HeaderFooter))
		{
			if (m_previousLineCommentStartMarks == null)
			{
				m_previousLineCommentStartMarks = new List<KeyValuePair<string, bool>>();
			}
			DrawCommentHighlighter(ltWidget, paragraph.Document);
		}
	}

	private void DrawCommentHighlighter(LayoutedWidget ltWidget, WordDocument document)
	{
		RevisionOptions revisionOptions = document.RevisionOptions;
		LayoutedWidget maximumHeightWidget = GetMaximumHeightWidget(ltWidget);
		float height = maximumHeightWidget?.Bounds.Height ?? 0f;
		float y = maximumHeightWidget?.Bounds.Y ?? 0f;
		bool isResolvedComment = false;
		if (m_previousLineCommentStartMarks.Count > 0)
		{
			isResolvedComment = m_previousLineCommentStartMarks[0].Value;
		}
		for (int i = 0; i < ltWidget.ChildWidgets.Count; i++)
		{
			LayoutedWidget layoutedWidget = ltWidget.ChildWidgets[i];
			if (layoutedWidget.Widget is WCommentMark && (layoutedWidget.Widget as WCommentMark).Comment != null)
			{
				WCommentMark wCommentMark = layoutedWidget.Widget as WCommentMark;
				if (wCommentMark.Type == CommentMarkType.CommentStart && wCommentMark.Comment.CommentRangeEnd != null)
				{
					if (m_previousLineCommentStartMarks.Count == 0 && wCommentMark.Comment.Done)
					{
						isResolvedComment = true;
					}
					m_previousLineCommentStartMarks.Add(new KeyValuePair<string, bool>(wCommentMark.CommentId, wCommentMark.Comment.Done));
				}
				else if (wCommentMark.Type == CommentMarkType.CommentEnd)
				{
					if (ContainsKey(wCommentMark.CommentId, m_previousLineCommentStartMarks))
					{
						m_previousLineCommentStartMarks.Remove(GetKeyValuePair(wCommentMark.CommentId, m_previousLineCommentStartMarks));
					}
					if (m_previousLineCommentStartMarks.Count > 0)
					{
						isResolvedComment = m_previousLineCommentStartMarks[0].Value;
					}
				}
				layoutedWidget.Bounds = new RectangleF(layoutedWidget.Bounds.X, layoutedWidget.Bounds.Y, layoutedWidget.Bounds.Width, height);
			}
			if (m_previousLineCommentStartMarks.Count > 0)
			{
				RectangleF bounds = new RectangleF(layoutedWidget.Bounds.X, y, layoutedWidget.Bounds.Width, height);
				DrawCommentHighlighter(revisionOptions, bounds, isResolvedComment);
			}
		}
	}

	private KeyValuePair<string, bool> GetKeyValuePair(string inputKey, List<KeyValuePair<string, bool>> keyValuePairCollection)
	{
		foreach (KeyValuePair<string, bool> item in keyValuePairCollection)
		{
			if (item.Key == inputKey)
			{
				return item;
			}
		}
		return new KeyValuePair<string, bool>(string.Empty, value: false);
	}

	private bool ContainsKey(string inputKey, List<KeyValuePair<string, bool>> keyValuePairCollection)
	{
		return GetKeyValuePair(inputKey, keyValuePairCollection).Key == inputKey;
	}

	private void DrawCommentHighlighter(RevisionOptions revisionOptions, RectangleF bounds, bool isResolvedComment)
	{
		if (bounds.Width > 0f)
		{
			Color revisionFillColor = GetRevisionFillColor(revisionOptions.CommentColor, isResolvedComment);
			PDFGraphics.DrawRectangle(GetPDFBrush(revisionFillColor), bounds);
		}
	}

	private LayoutedWidget GetMaximumHeightWidget(LayoutedWidget ltWidget)
	{
		LayoutedWidget result = null;
		float num = 0f;
		for (int i = 0; i < ltWidget.ChildWidgets.Count; i++)
		{
			LayoutedWidget layoutedWidget = ltWidget.ChildWidgets[i];
			if (((!(layoutedWidget.Widget is WPicture) && !(layoutedWidget.Widget is Shape) && !(layoutedWidget.Widget is WTextBox) && !(layoutedWidget.Widget is WChart) && !(layoutedWidget.Widget is GroupShape)) || (layoutedWidget.Widget as ParagraphItem).GetTextWrappingStyle() == TextWrappingStyle.Inline) && num < layoutedWidget.Bounds.Height)
			{
				num = layoutedWidget.Bounds.Height;
				result = layoutedWidget;
			}
		}
		return result;
	}

	private bool IsLinesInteresectWithFloatingItems(LayoutedWidget ltWidget, bool isLineContainer)
	{
		if (!isLineContainer)
		{
			ltWidget = ltWidget.Owner;
		}
		for (int i = 0; i < ltWidget.ChildWidgets.Count; i++)
		{
			if (ltWidget.ChildWidgets[i].IntersectingBounds.Count > 0)
			{
				return true;
			}
		}
		return false;
	}

	private void DrawLineBackGroundColors(WParagraph paragraph, LayoutedWidget ltWidget)
	{
		bool resetTransform = false;
		RectangleF boundsToDrawParagraphBackGroundColor = GetBoundsToDrawParagraphBackGroundColor(paragraph, ltWidget, paragraph.BreakCharacterFormat.Hidden, isLineDrawing: true, ref resetTransform);
		if (boundsToDrawParagraphBackGroundColor.Width <= 0f || boundsToDrawParagraphBackGroundColor.Height <= 0f)
		{
			return;
		}
		IEntity nextSibling = paragraph.NextSibling;
		WParagraphFormat paragraphFormat = paragraph.ParagraphFormat;
		if (ltWidget.Owner.ChildWidgets[ltWidget.Owner.ChildWidgets.Count - 1] == ltWidget && paragraph.GetOwnerEntity() is WTableCell && paragraph.GetOwnerEntity() is WTableCell { OwnerRow: not null } wTableCell && ((IWidget)wTableCell.OwnerRow).LayoutInfo is RowLayoutInfo)
		{
			RowLayoutInfo rowLayoutInfo = ((IWidget)wTableCell.OwnerRow).LayoutInfo as RowLayoutInfo;
			if (paragraph.IsInCell && !rowLayoutInfo.IsExactlyRowHeight && (nextSibling == null || !(nextSibling is WParagraph) || !(nextSibling as WParagraph).ParagraphFormat.BackColor.IsEmpty || !ltWidget.IsLastItemInPage))
			{
				boundsToDrawParagraphBackGroundColor.Height -= paragraphFormat.AfterSpacing;
			}
		}
		List<RectangleF> backGroundColorRenderingBounds = GetBackGroundColorRenderingBounds(ltWidget, boundsToDrawParagraphBackGroundColor);
		if (paragraphFormat.TextureStyle != 0)
		{
			for (int i = 0; i < backGroundColorRenderingBounds.Count; i++)
			{
				DrawTextureStyle(paragraphFormat.TextureStyle, paragraphFormat.ForeColor, paragraphFormat.BackColor, backGroundColorRenderingBounds[i]);
			}
		}
		else if (!paragraphFormat.BackColor.IsEmpty)
		{
			for (int j = 0; j < backGroundColorRenderingBounds.Count; j++)
			{
				PDFGraphics.DrawRectangle(new PdfSolidBrush(paragraphFormat.BackColor), backGroundColorRenderingBounds[j]);
			}
		}
		backGroundColorRenderingBounds.Clear();
		if (resetTransform)
		{
			ResetTransform();
		}
	}

	private List<RectangleF> GetBackGroundColorRenderingBounds(LayoutedWidget ltWidget, RectangleF remaingingBounds)
	{
		List<RectangleF> intersectingBounds = ltWidget.IntersectingBounds;
		List<RectangleF> list = new List<RectangleF>();
		if (intersectingBounds.Count > 0)
		{
			RectangleF innerItemsRenderingBounds = GetInnerItemsRenderingBounds(ltWidget);
			bool isNeedToFindFillColorRenderingBounds = true;
			remaingingBounds = FindFillColorBounds(ref isNeedToFindFillColorRenderingBounds, innerItemsRenderingBounds, intersectingBounds, remaingingBounds);
			if (isNeedToFindFillColorRenderingBounds)
			{
				for (int i = 0; i < ltWidget.IntersectingBounds.Count; i++)
				{
					RectangleF rectangleF = ltWidget.IntersectingBounds[i];
					if (remaingingBounds.X < rectangleF.X)
					{
						RectangleF item = new RectangleF(remaingingBounds.X, remaingingBounds.Y, ltWidget.IntersectingBounds[i].X - remaingingBounds.X, remaingingBounds.Height);
						list.Add(item);
						remaingingBounds = new RectangleF(ltWidget.IntersectingBounds[i].Right, remaingingBounds.Y, remaingingBounds.Right - ltWidget.IntersectingBounds[i].Right, remaingingBounds.Height);
					}
					else if (remaingingBounds.X > rectangleF.Right)
					{
						remaingingBounds = new RectangleF(rectangleF.Right, remaingingBounds.Y, remaingingBounds.Right - rectangleF.Right, remaingingBounds.Height);
					}
				}
			}
			list.Add(remaingingBounds);
			intersectingBounds.Clear();
		}
		return list;
	}

	private RectangleF FindFillColorBounds(ref bool isNeedToFindFillColorRenderingBounds, RectangleF childItemBounds, List<RectangleF> intersectingBoundsCollection, RectangleF remaingingBounds)
	{
		if (childItemBounds.X < intersectingBoundsCollection[0].X && childItemBounds.Right < intersectingBoundsCollection[0].X)
		{
			remaingingBounds = new RectangleF(remaingingBounds.X, remaingingBounds.Y, intersectingBoundsCollection[0].X - remaingingBounds.X, remaingingBounds.Height);
			isNeedToFindFillColorRenderingBounds = false;
		}
		else if (childItemBounds.X >= intersectingBoundsCollection[intersectingBoundsCollection.Count - 1].Right)
		{
			remaingingBounds = new RectangleF(intersectingBoundsCollection[intersectingBoundsCollection.Count - 1].Right, remaingingBounds.Y, remaingingBounds.Right - intersectingBoundsCollection[intersectingBoundsCollection.Count - 1].Right, remaingingBounds.Height);
			isNeedToFindFillColorRenderingBounds = false;
		}
		else
		{
			for (int i = 0; i + 1 < intersectingBoundsCollection.Count; i++)
			{
				RectangleF rectangleF = intersectingBoundsCollection[i];
				RectangleF rectangleF2 = intersectingBoundsCollection[i + 1];
				if (childItemBounds.X >= rectangleF.Right && childItemBounds.Right < rectangleF2.X)
				{
					remaingingBounds = new RectangleF(rectangleF.Right, remaingingBounds.Y, rectangleF2.X - rectangleF.Right, remaingingBounds.Height);
					isNeedToFindFillColorRenderingBounds = false;
					break;
				}
			}
		}
		return remaingingBounds;
	}

	public RectangleF GetInnerItemsRenderingBounds(LayoutedWidget ltWidget)
	{
		RectangleF bounds = ltWidget.Bounds;
		float num = float.MaxValue;
		float num2 = 0f;
		for (int i = 0; i < ltWidget.ChildWidgets.Count; i++)
		{
			if (ltWidget.ChildWidgets[i].Widget is ParagraphItem paragraphItem && !paragraphItem.IsFloatingItem(isTextWrapAround: false))
			{
				if (num > ltWidget.ChildWidgets[i].Bounds.X)
				{
					num = ltWidget.ChildWidgets[i].Bounds.X;
				}
				if (num2 < ltWidget.ChildWidgets[i].Bounds.Right)
				{
					num2 = ltWidget.ChildWidgets[i].Bounds.Right;
				}
			}
		}
		return new RectangleF((num != float.MaxValue) ? num : bounds.X, bounds.Y, num2 - num, bounds.Height);
	}

	internal void Draw(SplitTableWidget splitTableWidget, LayoutedWidget layoutedWidget)
	{
		throw new NotImplementedException();
	}

	private void Draw(BlockContentControl SDT, LayoutedWidget ltWidget)
	{
	}

	private void Draw(InlineContentControl SDT, LayoutedWidget ltWidget)
	{
	}

	private void Draw(WCommentMark commentMark, LayoutedWidget ltWidget)
	{
		AddCommentMark(commentMark, ltWidget);
	}

	private void Draw(WAbsoluteTab absoluteTab, LayoutedWidget ltWidget)
	{
		DrawAbsoluteTab(absoluteTab, ltWidget);
	}

	internal void Draw(WChart chart, LayoutedWidget ltWidget)
	{
		DrawChart(chart, ltWidget);
	}

	internal void DrawChart(WChart chart, LayoutedWidget widget)
	{
		RectangleF bounds = widget.Bounds;
		MemoryStream memoryStream = new MemoryStream();
		chart.OfficeChart.Width = chart.Width;
		chart.OfficeChart.Height = chart.Height;
		IOfficeChart officeChart = chart.OfficeChart;
		ChartToImageconverter.SaveAsImage(chart.OfficeChart, memoryStream, ChartRenderingOptions);
		int num = 0;
		int num2 = 0;
		ChartImpl chartImpl = ((officeChart is ChartImpl) ? (officeChart as ChartImpl) : (officeChart as ChartShapeImpl).ChartObject);
		if (!(officeChart is ChartShapeImpl))
		{
			num = (int)officeChart.Width;
			num2 = (int)officeChart.Height;
		}
		else
		{
			num = (officeChart as ChartShapeImpl).Width;
			num2 = (officeChart as ChartShapeImpl).Height;
		}
		if (chartImpl != null && chartImpl.Shapes.Count > 0)
		{
			new ConvertChartShapes(chartImpl.ParentWorkbook, chartImpl).DrawChartShapes(memoryStream, num, num2);
		}
		Entity entity = ((chart.OwnerParagraph != null) ? chart.OwnerParagraph.GetOwnerEntity() : null);
		float width = bounds.Width;
		if ((entity is Shape || entity is WTextBox || entity is ChildShape) && widget.Widget.LayoutInfo.IsClipped)
		{
			RectangleF clipBounds = GetClipBounds(bounds, width, 0f);
			if (IsNeedToClip(clipBounds))
			{
				clipBounds = ClipBoundsContainer.Peek();
				SetClip(clipBounds);
			}
		}
		if (memoryStream.Length > 0)
		{
			PdfImage pdfImage = GetPdfImage(memoryStream);
			if (AutoTag)
			{
				pdfImage.PdfTag = CreateAutoTag(PdfTagType.Figure, chart.AlternativeText, chart.Title, IsOverLappedShapeWidget(widget));
				PDFGraphics.DrawImage(pdfImage, bounds);
				PDFGraphics.ResetTransform();
			}
			else
			{
				PDFGraphics.DrawImage(pdfImage, bounds);
				PDFGraphics.ResetTransform();
			}
		}
		memoryStream.Dispose();
	}

	internal void Draw(WCheckBox checkBox, LayoutedWidget ltWidget)
	{
		if (PreserveFormFields)
		{
			DrawEditableCheckbox(checkBox, ltWidget);
		}
		else
		{
			DrawCheckBox(checkBox, ltWidget);
		}
	}

	internal void Draw(WDropDownFormField dropDownFormField, LayoutedWidget ltWidget)
	{
		if (PreserveFormFields)
		{
			DrawEditableDropDown(dropDownFormField, ltWidget);
		}
		else
		{
			DrawString(dropDownFormField.ScriptType, dropDownFormField.DropDownValue, dropDownFormField.CharacterFormat, null, ltWidget.Bounds, ltWidget.Bounds.Width, ltWidget);
		}
	}

	private void Draw(WField field, LayoutedWidget ltWidget)
	{
		switch (field.FieldType)
		{
		case FieldType.FieldHyperlink:
			currHyperlink = new Hyperlink(field);
			break;
		case FieldType.FieldAutoNumLegal:
		case FieldType.FieldAutoNum:
			currTextRange = field.GetCurrentTextRange();
			DrawString(currTextRange.ScriptType, ltWidget.TextTag, currTextRange.CharacterFormat, field.GetOwnerParagraphValue().ParagraphFormat, ltWidget.Bounds, ltWidget.Bounds.Width, ltWidget);
			break;
		case FieldType.FieldPage:
			currTextRange = field.GetCurrentTextRange();
			DrawString(currTextRange.ScriptType, ltWidget.TextTag, currTextRange.CharacterFormat, field.GetOwnerParagraphValue().ParagraphFormat, ltWidget.Bounds, ltWidget.Bounds.Width, ltWidget);
			AddLinkToBookmark(ltWidget.Bounds, "PAGE-" + ltWidget.TextTag, isTargetNull: true);
			break;
		case FieldType.FieldSectionPages:
			currTextRange = field.GetCurrentTextRange();
			DrawString(currTextRange.ScriptType, ltWidget.TextTag, currTextRange.CharacterFormat, field.GetOwnerParagraphValue().ParagraphFormat, ltWidget.Bounds, ltWidget.Bounds.Width, ltWidget);
			AddLinkToBookmark(ltWidget.Bounds, "SECTIONPAGES-" + ltWidget.TextTag, isTargetNull: true);
			break;
		case FieldType.FieldExpression:
		{
			for (int i = 0; i < DocumentLayouter.EquationFields.Count; i++)
			{
				if (DocumentLayouter.EquationFields[i].EQFieldEntity == field)
				{
					AlignEqFieldSwitches(DocumentLayouter.EquationFields[i].LayouttedEQField, ltWidget.Bounds.X, ltWidget.Bounds.Y);
					currTextRange = field.GetCurrentTextRange();
					DrawEquationField(currTextRange.ScriptType, DocumentLayouter.EquationFields[i].LayouttedEQField, currTextRange.CharacterFormat);
					break;
				}
			}
			break;
		}
		case FieldType.FieldDocVariable:
			ltWidget.TextTag = field.Document.Variables[field.FieldValue];
			DrawString(field.ScriptType, ltWidget.TextTag, field.CharacterFormat, field.OwnerParagraph.ParagraphFormat, ltWidget.Bounds, ltWidget.Bounds.Width, ltWidget);
			break;
		case FieldType.FieldNumPages:
			currTextRange = field.GetCurrentTextRange();
			DrawString(currTextRange.ScriptType, field.FieldResult, currTextRange.CharacterFormat, field.GetOwnerParagraphValue().ParagraphFormat, ltWidget.Bounds, ltWidget.Bounds.Width, ltWidget);
			AddLinkToBookmark(ltWidget.Bounds, "NUMPAGES-" + field.FieldResult, isTargetNull: true);
			break;
		case FieldType.FieldRef:
		case FieldType.FieldPageRef:
		{
			string bkName = null;
			if (field.IsBookmarkCrossRefField(ref bkName))
			{
				CurrentRefField = field;
				CurrentBookmarkName = bkName;
			}
			break;
		}
		}
	}

	internal void Draw(WOleObject oleObject, LayoutedWidget ltWidget)
	{
		if (oleObject != null && oleObject.OlePicture != null)
		{
			DrawPicture(oleObject.OlePicture, ltWidget);
		}
	}

	internal void Draw(WPicture picture, LayoutedWidget ltWidget)
	{
		DrawPicture(picture, ltWidget);
	}

	internal void Draw(WSymbol symbol, LayoutedWidget ltWidget)
	{
		DrawSymbol(symbol, ltWidget);
	}

	private void Draw(WTable table, LayoutedWidget ltWidget)
	{
		if (table.IsFrame)
		{
			WParagraph wParagraph = table.Rows[0].Cells[0].Paragraphs[0];
			if (!ltWidget.Bounds.IsEmpty && (wParagraph.ParagraphFormat.FrameHeight != 0f || wParagraph.ParagraphFormat.FrameWidth != 0f) && wParagraph.m_layoutInfo != null)
			{
				RectangleF clipBounds = GetClippingBounds(ltWidget.ChildWidgets[0].ChildWidgets[0]);
				if (wParagraph.ParagraphFormat.FrameWidth < ltWidget.Bounds.Width)
				{
					clipBounds = new RectangleF(clipBounds.X, clipBounds.Y, clipBounds.Width + wParagraph.ParagraphFormat.FrameHorizontalDistanceFromText, clipBounds.Height);
				}
				clipBounds = UpdateClipBounds(clipBounds, reverseClipping: false);
				clipBounds = GetClipBounds(clipBounds, clipBounds.Width, 0f);
				ClipBoundsContainer.Push(clipBounds);
			}
		}
		DrawTable(table, ltWidget);
	}

	private LayoutedWidget GetFirstItemInFrame(LayoutedWidgetList layoutedWidgets, int index, WParagraphFormat originalFormat)
	{
		while (index - 1 > -1)
		{
			WParagraphFormat wParagraphFormat = new WParagraphFormat();
			LayoutedWidget layoutedWidget = layoutedWidgets[index - 1];
			if (layoutedWidget.Widget is WTable)
			{
				wParagraphFormat = (layoutedWidget.Widget as WTable).Rows[0].Cells[0].Paragraphs[0].ParagraphFormat;
			}
			else if (layoutedWidget.Widget is WParagraph)
			{
				wParagraphFormat = (layoutedWidget.Widget as WParagraph).ParagraphFormat;
			}
			if (!wParagraphFormat.IsInSameFrame(originalFormat))
			{
				break;
			}
			index--;
		}
		return layoutedWidgets[index];
	}

	private void Draw(WTableCell cell, LayoutedWidget ltWidget)
	{
		if (cell.CellFormat.IsFormattingChange && cell.Document.RevisionOptions.ShowRevisionBars && cell.GetOwnerSection(cell) is WSection)
		{
			float left = (cell.GetOwnerSection(cell) as WSection).PageSetup.Margins.Left;
			PointF start = new PointF(left / 2f, ltWidget.Bounds.Y);
			PointF end = new PointF(left / 2f, ltWidget.Bounds.Y + ltWidget.Bounds.Height);
			RevisionOptions revisionOptions = cell.Document.RevisionOptions;
			DrawRevisionMark(start, end, GetRevisionColor(revisionOptions.RevisionBarsColor), revisionOptions.RevisionMarkWidth);
		}
		DrawTableCell(cell, ltWidget);
	}

	private void Draw(WTableRow row, LayoutedWidget ltWidget)
	{
		if ((row.IsInsertRevision || row.RowFormat.IsChangedFormat || (row.IsDeleteRevision && row.Document.RevisionOptions.ShowDeletedText)) && row.Document.RevisionOptions.ShowRevisionBars && row.GetOwnerSection(row) is WSection)
		{
			float left = (row.GetOwnerSection(row) as WSection).PageSetup.Margins.Left;
			PointF start = new PointF(left / 2f, ltWidget.Bounds.Y);
			PointF end = new PointF(left / 2f, ltWidget.Bounds.Y + ltWidget.Bounds.Height);
			RevisionOptions revisionOptions = row.Document.RevisionOptions;
			DrawRevisionMark(start, end, GetRevisionColor(revisionOptions.RevisionBarsColor), revisionOptions.RevisionMarkWidth);
		}
		DrawTableRow(row, ltWidget);
	}

	internal void Draw(WTextBox textBox, LayoutedWidget ltWidget)
	{
		RectangleF bounds = ltWidget.Bounds;
		bounds.Y += textBox.TextBoxFormat.InternalMargin.Top;
		bounds.Height -= textBox.TextBoxFormat.InternalMargin.Top + textBox.TextBoxFormat.InternalMargin.Bottom;
		if (textBox.TextBoxFormat.TextDirection != 0 && textBox.TextBoxFormat.TextDirection != DocGen.DocIO.DLS.TextDirection.HorizontalFarEast)
		{
			if (textBox.TextBoxFormat.Rotation == 0f && !textBox.IsNoNeedToConsiderLineWidth())
			{
				bounds = ltWidget.Bounds;
				textBox.CalculateBoundsBasedOnLineWidth(ref bounds, textBox.TextBoxFormat);
			}
			RectangleF rectangleF = bounds;
			rectangleF.Width += rectangleF.Height;
			rectangleF.Height = rectangleF.Width - rectangleF.Height;
			rectangleF.Width -= rectangleF.Height;
			bounds = rectangleF;
		}
		bounds = UpdateClipBounds(bounds, reverseClipping: false);
		ClipBoundsContainer.Push(bounds);
		if (textBox.Visible)
		{
			DrawTextBox(textBox, ltWidget);
		}
	}

	internal void Draw(WTextFormField textFormField, LayoutedWidget ltWidget)
	{
	}

	internal void Draw(LeafEmtyWidget leafEmtyWidget, LayoutedWidget layoutedWidget)
	{
	}

	internal void Draw(Shape shape, LayoutedWidget ltWidget)
	{
		RectangleF layoutRect = shape.GetBoundsToLayoutShapeTextBody(shape.AutoShapeType, shape.ShapeGuide, ltWidget.Bounds);
		shape.UpdateShapeBoundsToLayoutTextBody(ref layoutRect, shape.TextFrame.InternalMargin, ltWidget);
		layoutRect = UpdateClipBounds(layoutRect, reverseClipping: false);
		if (layoutRect.Height <= 0f && ltWidget.Bounds.Height > 0f)
		{
			layoutRect = UpdateClipBounds(ltWidget.Bounds, reverseClipping: false);
		}
		if (((IWidget)shape).LayoutInfo.IsVerticalText)
		{
			layoutRect = UpdateClipBounds(ltWidget.Bounds, reverseClipping: false);
			RectangleF rectangleF = layoutRect;
			rectangleF.Width += rectangleF.Height;
			rectangleF.Height = rectangleF.Width - rectangleF.Height;
			rectangleF.Width -= rectangleF.Height;
			layoutRect = rectangleF;
		}
		ClipBoundsContainer.Push(layoutRect);
		if (shape.Visible)
		{
			DrawShape(shape, ltWidget);
		}
	}

	internal void Draw(ChildShape shape, LayoutedWidget ltWidget)
	{
		DrawChildShape(shape, ltWidget);
	}

	internal void Draw(SplitStringWidget splitStringWidget, LayoutedWidget layoutedWidget)
	{
		Draw(splitStringWidget.RealStringWidget, layoutedWidget, splitStringWidget.SplittedText);
		DrawImpl(layoutedWidget);
	}

	internal void Draw(WTextRange textRange, LayoutedWidget ltWidget)
	{
		string text = ((ltWidget.TextTag != null) ? ltWidget.TextTag : textRange.Text);
		Draw(textRange, ltWidget, text);
	}

	internal void Draw(IStringWidget stringWidget, LayoutedWidget ltWidget, string text)
	{
		if (PreserveFormFields)
		{
			WTextRange wTextRange = ((ltWidget.Widget is WTextRange) ? (ltWidget.Widget as WTextRange) : (stringWidget as WTextRange));
			if (wTextRange != null && wTextRange.PreviousSibling is WFieldMark)
			{
				WFieldMark wFieldMark = wTextRange.PreviousSibling as WFieldMark;
				if (wFieldMark.ParentField is WTextFormField)
				{
					WTextFormField wTextFormField = wFieldMark.ParentField as WTextFormField;
					if (m_editableTextFormTextRange == null && wFieldMark.Type != FieldMarkType.FieldEnd)
					{
						m_editableTextFormTextRange = wTextRange;
						m_editableTextFormBounds = ltWidget.Bounds;
						if (string.IsNullOrEmpty(wTextFormField.Name))
						{
							wTextFormField.Name = "Textformfield";
						}
						for (int num = ltWidget.Owner.ChildWidgets.Count - 1; num >= 0; num--)
						{
							if (ltWidget.Owner.ChildWidgets[num].Widget is WTextRange || ltWidget.Owner.ChildWidgets[num].Widget is SplitStringWidget)
							{
								m_lastTextRangeIndex = num;
								break;
							}
						}
					}
				}
			}
			if (m_editableTextFormTextRange != null)
			{
				if (m_lastTextRangeIndex != 0 && m_editableTextFormTextRange != wTextRange)
				{
					m_editableTextFormBounds.Width += ltWidget.Bounds.Width;
				}
				if (ltWidget.Widget == ltWidget.Owner.ChildWidgets[m_lastTextRangeIndex].Widget)
				{
					m_lastTextRangeIndex = 0;
				}
				if (ltWidget.Widget is SplitStringWidget)
				{
					m_editableTextFormText += (ltWidget.Widget as SplitStringWidget).SplittedText;
				}
				if (ltWidget.Widget is WTextRange)
				{
					m_editableTextFormText += (ltWidget.Widget as WTextRange).Text;
				}
				string text2 = "";
				text2 = ((m_editableTextFormTextRange.PreviousSibling as WFieldMark).ParentField as WTextFormField).Text;
				text2 = text2.Replace(" ", string.Empty);
				m_editableTextFormText = m_editableTextFormText.Replace(" ", string.Empty);
				if (m_editableTextFormText == text2)
				{
					DrawEditableTextRange(stringWidget as WTextRange, ltWidget);
					m_editableTextFormBounds.Width = 0f;
					m_editableTextFormTextRange = null;
					m_editableTextFormText = "";
				}
			}
			else
			{
				DrawTextRange(stringWidget as WTextRange, ltWidget, text);
				DrawImpl(ltWidget);
			}
		}
		else
		{
			DrawTextRange(stringWidget as WTextRange, ltWidget, text);
			DrawImpl(ltWidget);
		}
	}

	internal void DrawImpl(LayoutedWidget ltWidget)
	{
	}

	internal void DrawImpl(WFootnote footNote, LayoutedWidget ltWidget)
	{
		if (((IWidget)footNote).LayoutInfo is FootnoteLayoutInfo footnoteLayoutInfo)
		{
			DrawTextRange(footnoteLayoutInfo.TextRange, ltWidget, footnoteLayoutInfo.FootnoteID);
		}
	}

	private Entity GetOwnerLayoutedWidget(LayoutedWidget ltWidget)
	{
		if (ltWidget.Owner == null)
		{
			return null;
		}
		if (!(ltWidget.Owner.Widget is Entity))
		{
			if (!(ltWidget.Owner.Widget is SplitWidgetContainer))
			{
				return null;
			}
			return (ltWidget.Owner.Widget as SplitWidgetContainer).RealWidgetContainer as Entity;
		}
		return ltWidget.Owner.Widget as Entity;
	}

	private WParagraph GetParagraphWidget(LayoutedWidget ltWidget)
	{
		if (!(ltWidget.Widget is WParagraph))
		{
			if (!(ltWidget.Widget is SplitWidgetContainer))
			{
				return null;
			}
			return (ltWidget.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph;
		}
		return ltWidget.Widget as WParagraph;
	}

	private WTableCell GetCellWidget(LayoutedWidget ltWidget)
	{
		if (!(ltWidget.Widget is WTableCell))
		{
			if (!(ltWidget.Widget is SplitWidgetContainer))
			{
				return null;
			}
			return (ltWidget.Widget as SplitWidgetContainer).RealWidgetContainer as WTableCell;
		}
		return ltWidget.Widget as WTableCell;
	}

	private bool IsOverLappedShapeWidget(LayoutedWidget ltWidget)
	{
		if ((!(ltWidget.Widget is WPicture) || (ltWidget.Widget as WPicture).TextWrappingStyle == TextWrappingStyle.Inline || (ltWidget.Widget as WPicture).TextWrappingStyle == TextWrappingStyle.Behind || ((ltWidget.Widget as WPicture).Document.Settings.CompatibilityMode != CompatibilityMode.Word2013 && (ltWidget.Widget as WPicture).IsBelowText && (ltWidget.Widget as WPicture).TextWrappingStyle != TextWrappingStyle.InFrontOfText)) && (!(ltWidget.Widget is WChart) || (ltWidget.Widget as WChart).WrapFormat.TextWrappingStyle == TextWrappingStyle.Inline || (ltWidget.Widget as WChart).WrapFormat.TextWrappingStyle == TextWrappingStyle.Behind || ((ltWidget.Widget as WChart).Document.Settings.CompatibilityMode != CompatibilityMode.Word2013 && (ltWidget.Widget as WChart).IsBelowText && (ltWidget.Widget as WChart).WrapFormat.TextWrappingStyle != TextWrappingStyle.InFrontOfText)) && (!(ltWidget.Widget is Shape) || (ltWidget.Widget as Shape).WrapFormat.TextWrappingStyle == TextWrappingStyle.Inline || (ltWidget.Widget as Shape).WrapFormat.TextWrappingStyle == TextWrappingStyle.Behind || ((ltWidget.Widget as Shape).Document.Settings.CompatibilityMode != CompatibilityMode.Word2013 && (ltWidget.Widget as Shape).IsBelowText && (ltWidget.Widget as Shape).WrapFormat.TextWrappingStyle != TextWrappingStyle.InFrontOfText)) && (!(ltWidget.Widget is GroupShape) || (ltWidget.Widget as GroupShape).WrapFormat.TextWrappingStyle == TextWrappingStyle.Inline || (ltWidget.Widget as GroupShape).WrapFormat.TextWrappingStyle == TextWrappingStyle.Behind || ((ltWidget.Widget as GroupShape).Document.Settings.CompatibilityMode != CompatibilityMode.Word2013 && (ltWidget.Widget as GroupShape).IsBelowText && (ltWidget.Widget as GroupShape).WrapFormat.TextWrappingStyle != TextWrappingStyle.InFrontOfText)) && (!(ltWidget.Widget is WTextBox) || (ltWidget.Widget as WTextBox).TextBoxFormat.TextWrappingStyle == TextWrappingStyle.Inline || (ltWidget.Widget as WTextBox).TextBoxFormat.TextWrappingStyle == TextWrappingStyle.Behind || ((ltWidget.Widget as WTextBox).Document.Settings.CompatibilityMode != CompatibilityMode.Word2013 && (ltWidget.Widget as WTextBox).TextBoxFormat.IsBelowText && (ltWidget.Widget as WTextBox).TextBoxFormat.TextWrappingStyle != TextWrappingStyle.InFrontOfText)))
		{
			if (ltWidget.Widget is WTable && (ltWidget.Widget as WTable).TableFormat.WrapTextAround)
			{
				return (ltWidget.Widget as WTable).Document.Settings.CompatibilityMode == CompatibilityMode.Word2013;
			}
			return false;
		}
		return true;
	}

	private RectangleF UpdateWaterMarkPosition(ParagraphItem pItem, WPageSetup pageSetup, RectangleF bounds)
	{
		float num = 0f;
		float num2 = 0f;
		TextWrappingStyle textWrappingStyle = TextWrappingStyle.Behind;
		if (pItem is WPicture)
		{
			WPicture obj = pItem as WPicture;
			num = obj.Width;
			num2 = obj.Height;
			textWrappingStyle = obj.TextWrappingStyle;
		}
		else if (pItem is TextWatermark textWatermark)
		{
			num = textWatermark.Width;
			num2 = textWatermark.Height;
			textWrappingStyle = textWatermark.TextWrappingStyle;
		}
		if (textWrappingStyle != 0)
		{
			float num3 = 0f;
			float num4 = 0f;
			VerticalOrigin verticalOrigin = VerticalOrigin.Margin;
			HorizontalOrigin horizontalOrigin = HorizontalOrigin.Margin;
			ShapeVerticalAlignment shapeVerticalAlignment = ShapeVerticalAlignment.None;
			ShapeHorizontalAlignment shapeHorizontalAlignment = ShapeHorizontalAlignment.None;
			if (pItem is WPicture)
			{
				WPicture obj2 = pItem as WPicture;
				num3 = obj2.VerticalPosition;
				num4 = obj2.HorizontalPosition;
				verticalOrigin = obj2.VerticalOrigin;
				horizontalOrigin = obj2.HorizontalOrigin;
				shapeVerticalAlignment = obj2.VerticalAlignment;
				shapeHorizontalAlignment = obj2.HorizontalAlignment;
			}
			else if (pItem is TextWatermark textWatermark2)
			{
				num3 = textWatermark2.VerticalPosition;
				num4 = textWatermark2.HorizontalPosition;
				verticalOrigin = textWatermark2.VerticalOrigin;
				horizontalOrigin = textWatermark2.HorizontalOrigin;
				shapeVerticalAlignment = textWatermark2.VerticalAlignment;
				shapeHorizontalAlignment = textWatermark2.HorizontalAlignment;
			}
			float num5 = bounds.Top + num3;
			float num6 = bounds.Left + num4;
			switch (verticalOrigin)
			{
			case VerticalOrigin.Page:
			case VerticalOrigin.TopMargin:
				num5 = num3;
				switch (shapeVerticalAlignment)
				{
				case ShapeVerticalAlignment.Top:
					num5 = 0f;
					break;
				case ShapeVerticalAlignment.Center:
					num5 = (pageSetup.PageSize.Height - num2) / 2f;
					break;
				case ShapeVerticalAlignment.Bottom:
					num5 = pageSetup.PageSize.Height - num2;
					break;
				}
				break;
			case VerticalOrigin.Margin:
				switch (shapeVerticalAlignment)
				{
				case ShapeVerticalAlignment.Top:
					num5 = bounds.Y;
					break;
				case ShapeVerticalAlignment.Center:
					if (num2 <= pageSetup.PageSize.Height)
					{
						num5 = bounds.Y + (bounds.Height - num2) / 2f;
						break;
					}
					num5 = (pageSetup.PageSize.Height - num2) / 2f;
					num5 += (pageSetup.Margins.Top - pageSetup.Margins.Bottom) / 2f;
					break;
				case ShapeVerticalAlignment.Bottom:
					num5 = bounds.Y + bounds.Height - num2;
					break;
				}
				break;
			}
			switch (horizontalOrigin)
			{
			case HorizontalOrigin.Page:
				num6 = num4;
				switch (shapeHorizontalAlignment)
				{
				case ShapeHorizontalAlignment.Center:
					num6 = (pageSetup.PageSize.Width - num) / 2f;
					break;
				case ShapeHorizontalAlignment.Left:
					num6 = 0f;
					break;
				case ShapeHorizontalAlignment.Right:
					num6 = pageSetup.PageSize.Width - num;
					break;
				}
				break;
			case HorizontalOrigin.Margin:
			case HorizontalOrigin.Column:
				switch (shapeHorizontalAlignment)
				{
				case ShapeHorizontalAlignment.Center:
					num6 = bounds.X + (bounds.Width - num) / 2f;
					break;
				case ShapeHorizontalAlignment.Left:
					num6 = bounds.Left;
					break;
				case ShapeHorizontalAlignment.Right:
					num6 = bounds.Left + bounds.Width - num;
					break;
				}
				break;
			}
			if ((textWrappingStyle == TextWrappingStyle.Square || textWrappingStyle == TextWrappingStyle.Through || textWrappingStyle == TextWrappingStyle.Tight) && verticalOrigin == VerticalOrigin.Paragraph && pItem.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013)
			{
				if (num6 < 0f)
				{
					num6 = 0f;
				}
				if (num5 < 0f)
				{
					num5 = 0f;
				}
			}
			bounds.X = num6;
			bounds.Y = num5;
		}
		bounds.Height = num2;
		bounds.Width = num;
		return bounds;
	}

	private void DrawTextWatermark(TextWatermark textWatermark, RectangleF bounds, WPageSetup pageSetup)
	{
		DocGen.Drawing.Font font = textWatermark.Document.FontSettings.GetFont(textWatermark.FontName, (textWatermark.Size == 1f || !textWatermark.HasKey(1)) ? GetFontSize(textWatermark) : textWatermark.Size, FontStyle.Regular, FontScriptType.English);
		if (font.Name == "Arial Narrow" && font.Style == FontStyle.Bold)
		{
			textWatermark.Text = textWatermark.Text.Replace(ControlChar.NonBreakingSpaceChar, ControlChar.SpaceChar);
		}
		if (textWatermark.Width == -1f || textWatermark.Height == -1f)
		{
			textWatermark.SetDefaultSize();
		}
		bounds = UpdateWaterMarkPosition(textWatermark, pageSetup, bounds);
		bounds = new RectangleF(bounds.X, bounds.Y, textWatermark.Width, textWatermark.Height);
		Rectangle rectangle = new Rectangle((int)bounds.X, (int)bounds.Y, (int)textWatermark.Width, (int)textWatermark.Height);
		DocGen.Drawing.SkiaSharpHelper.Bitmap bitmap = ConvertAsImage(textWatermark, font);
		if (textWatermark.Rotation != 0)
		{
			bounds = new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height);
			PDFGraphics.Transform = GetTransformMatrix(bounds, textWatermark.Rotation);
			rectangle = new Rectangle((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height);
		}
		MemoryStream memoryStream = new MemoryStream();
		bitmap.Save(memoryStream, DocGen.Drawing.ImageFormat.Png);
		memoryStream.Position = 0L;
		PdfImage pdfImage = GetPdfImage(memoryStream);
		memoryStream.Dispose();
		PDFGraphics.DrawImage(pdfImage, rectangle);
		if (textWatermark.Rotation != 0)
		{
			ResetTransform();
		}
		bitmap.Dispose();
	}

	private DocGen.Drawing.SkiaSharpHelper.Bitmap ConvertAsImage(TextWatermark textWatermark, DocGen.Drawing.Font font)
	{
		DocGen.Drawing.SkiaSharpHelper.Bitmap bmp = CreateBitmap(1, 1);
		Graphics graphicsFromImage = GetGraphicsFromImage(bmp);
		int width = (int)graphicsFromImage.MeasureString(textWatermark.Text, font, font.Style, FontScriptType.English).Width;
		int height = (int)graphicsFromImage.MeasureString(textWatermark.Text, font, font.Style, FontScriptType.English).Height;
		bmp = CreateBitmap(width, height);
		Graphics graphicsFromImage2 = GetGraphicsFromImage(bmp);
		graphicsFromImage2.Clear(Color.White);
		graphicsFromImage2.TextRenderingHint = TextRenderingHint.AntiAlias;
		graphicsFromImage2.SmoothingMode = SmoothingMode.HighQuality;
		graphicsFromImage2.InterpolationMode = InterpolationMode.HighQualityBicubic;
		Color color = textWatermark.Color;
		if (textWatermark.Semitransparent)
		{
			color = ChangeColorBrightness(color, 0.5f);
		}
		graphicsFromImage2.DrawString(textWatermark.Text, font, CreateBrush(color), 0, 0, font.Style, FontScriptType.English);
		graphicsFromImage2.Dispose();
		return bmp;
	}

	private Color ChangeColorBrightness(Color color, float correctionFactor)
	{
		float num = (int)color.R;
		float num2 = (int)color.G;
		float num3 = (int)color.B;
		if (correctionFactor < 0f)
		{
			correctionFactor = 1f + correctionFactor;
			num *= correctionFactor;
			num2 *= correctionFactor;
			num3 *= correctionFactor;
		}
		else
		{
			num = (255f - num) * correctionFactor + num;
			num2 = (255f - num2) * correctionFactor + num2;
			num3 = (255f - num3) * correctionFactor + num3;
		}
		return Color.FromArgb(color.A, (int)num, (int)num2, (int)num3);
	}

	internal void DrawPageBorder(WPageSetup pageSetup, RectangleF headerBounds, RectangleF footerBounds, RectangleF pageBounds)
	{
		switch (pageSetup.PageBorderOffsetFrom)
		{
		case PageBorderOffsetFrom.PageEdge:
		{
			float space = pageSetup.Borders.Left.Space;
			float num = pageSetup.Borders.Right.Space + pageSetup.Borders.Right.LineWidth / 2f;
			float space2 = pageSetup.Borders.Top.Space;
			float space3 = pageSetup.Borders.Bottom.Space;
			if (pageSetup.Borders.Left.BorderType != 0)
			{
				DrawBorder(pageSetup.Borders.Left, new PointF(space, space2), new PointF(space, pageSetup.PageSize.Height - space3));
			}
			if (pageSetup.Borders.Right.BorderType != 0)
			{
				DrawBorder(pageSetup.Borders.Right, new PointF(pageSetup.PageSize.Width - num, space2), new PointF(pageSetup.PageSize.Width - num, pageSetup.PageSize.Height - space3));
			}
			if (pageSetup.Borders.Top.BorderType != 0)
			{
				DrawBorder(pageSetup.Borders.Top, new PointF(space, space2), new PointF(pageSetup.PageSize.Width - num, space2));
			}
			if (pageSetup.Borders.Bottom.BorderType != 0)
			{
				DrawBorder(pageSetup.Borders.Bottom, new PointF(space, pageSetup.PageSize.Height - space3), new PointF(pageSetup.PageSize.Width - num, pageSetup.PageSize.Height - space3));
			}
			break;
		}
		case PageBorderOffsetFrom.Text:
		{
			PointF[] array = new PointF[2];
			if (pageSetup.Borders.Left.BorderType != 0)
			{
				array = GetLeftBorderPoints(pageSetup, headerBounds, footerBounds, pageBounds);
				DrawBorder(pageSetup.Borders.Left, array[0], array[1]);
			}
			if (pageSetup.Borders.Right.BorderType != 0)
			{
				array = GetRightBorderPoints(pageSetup, headerBounds, footerBounds, pageBounds);
				DrawBorder(pageSetup.Borders.Right, array[0], array[1]);
			}
			if (pageSetup.Borders.Top.BorderType != 0)
			{
				array = GetTopBorderPoints(pageSetup, headerBounds, footerBounds, pageBounds);
				DrawBorder(pageSetup.Borders.Top, array[0], array[1]);
			}
			if (pageSetup.Borders.Bottom.BorderType != 0)
			{
				array = GetBottomBorderPoints(pageSetup, headerBounds, footerBounds, pageBounds);
				DrawBorder(pageSetup.Borders.Bottom, array[0], array[1]);
			}
			break;
		}
		}
	}

	private PointF[] GetLeftBorderPoints(WPageSetup pageSetup, RectangleF headerBounds, RectangleF footerBounds, RectangleF pageBounds)
	{
		float space = pageSetup.Borders.Left.Space;
		float space2 = pageSetup.Borders.Top.Space;
		float space3 = pageSetup.Borders.Bottom.Space;
		float num = pageSetup.PageSize.Height - ((pageSetup.Margins.Bottom > footerBounds.Height + pageSetup.FooterDistance) ? pageSetup.Margins.Bottom : (footerBounds.Height + pageSetup.FooterDistance));
		PointF[] array = new PointF[2];
		if (pageSetup.Document.BordersSurroundHeader && pageSetup.Document.BordersSurroundFooter)
		{
			array[0] = new PointF(headerBounds.X - space, (headerBounds.Height == 0f) ? (pageBounds.Y - space2) : (headerBounds.Y - space2));
			array[1] = new PointF(headerBounds.X - space, footerBounds.Bottom + space3);
		}
		else if (pageSetup.Document.BordersSurroundHeader)
		{
			array[0] = new PointF(headerBounds.X - space, (headerBounds.Height == 0f) ? (pageBounds.Y - space2) : (headerBounds.Y - space2));
			array[1] = new PointF(headerBounds.X - space, num + space3);
		}
		else if (pageSetup.Document.BordersSurroundFooter)
		{
			array[0] = new PointF(headerBounds.X - space, pageBounds.Y - space2);
			array[1] = new PointF(headerBounds.X - space, footerBounds.Bottom + space3);
		}
		else
		{
			array[0] = new PointF(headerBounds.X - space, pageBounds.Y - space2);
			array[1] = new PointF(headerBounds.X - space, num + space3);
		}
		return array;
	}

	private PointF[] GetRightBorderPoints(WPageSetup pageSetup, RectangleF headerBounds, RectangleF footerBounds, RectangleF pageBounds)
	{
		float space = pageSetup.Borders.Right.Space;
		float space2 = pageSetup.Borders.Top.Space;
		float space3 = pageSetup.Borders.Bottom.Space;
		float num = pageSetup.PageSize.Height - ((pageSetup.Margins.Bottom > footerBounds.Height + pageSetup.FooterDistance) ? pageSetup.Margins.Bottom : (footerBounds.Height + pageSetup.FooterDistance));
		PointF[] array = new PointF[2];
		if (pageSetup.Document.BordersSurroundHeader && pageSetup.Document.BordersSurroundFooter)
		{
			array[0] = new PointF(pageSetup.ClientWidth + headerBounds.X + space, (headerBounds.Height == 0f) ? (pageBounds.Y - space2) : (headerBounds.Y - space2));
			array[1] = new PointF(pageSetup.ClientWidth + headerBounds.X + space, footerBounds.Bottom + space3);
		}
		else if (pageSetup.Document.BordersSurroundHeader)
		{
			array[0] = new PointF(pageSetup.ClientWidth + headerBounds.X + space, (headerBounds.Height == 0f) ? (pageBounds.Y - space2) : (headerBounds.Y - space2));
			array[1] = new PointF(pageSetup.ClientWidth + headerBounds.X + space, num + space3);
		}
		else if (pageSetup.Document.BordersSurroundFooter)
		{
			array[0] = new PointF(pageSetup.ClientWidth + headerBounds.X + space, pageBounds.Y - space2);
			array[1] = new PointF(pageSetup.ClientWidth + headerBounds.X + space, footerBounds.Bottom + space3);
		}
		else
		{
			array[0] = new PointF(pageSetup.ClientWidth + headerBounds.X + space, pageBounds.Y - space2);
			array[1] = new PointF(pageSetup.ClientWidth + headerBounds.X + space, num + space3);
		}
		return array;
	}

	private PointF[] GetBottomBorderPoints(WPageSetup pageSetup, RectangleF headerBounds, RectangleF footerBounds, RectangleF pageBounds)
	{
		float space = pageSetup.Borders.Left.Space;
		float space2 = pageSetup.Borders.Right.Space;
		float space3 = pageSetup.Borders.Bottom.Space;
		float num = pageSetup.PageSize.Height - ((pageSetup.Margins.Bottom > footerBounds.Height + pageSetup.FooterDistance) ? pageSetup.Margins.Bottom : (footerBounds.Height + pageSetup.FooterDistance));
		PointF[] array = new PointF[2];
		if (pageSetup.Document.BordersSurroundHeader && pageSetup.Document.BordersSurroundFooter)
		{
			array[0] = new PointF(headerBounds.X - space, footerBounds.Bottom + space3);
			array[1] = new PointF(pageSetup.ClientWidth + headerBounds.X + space2, footerBounds.Bottom + space3);
		}
		else if (pageSetup.Document.BordersSurroundHeader)
		{
			array[0] = new PointF(headerBounds.X - space, num + space3);
			array[1] = new PointF(pageSetup.ClientWidth + headerBounds.X + space2, num + space3);
		}
		else if (pageSetup.Document.BordersSurroundFooter)
		{
			array[0] = new PointF(headerBounds.X - space, footerBounds.Bottom + space3);
			array[1] = new PointF(pageSetup.ClientWidth + headerBounds.X + space2, footerBounds.Bottom + space3);
		}
		else
		{
			array[0] = new PointF(headerBounds.X - space, num + space3);
			array[1] = new PointF(pageSetup.ClientWidth + headerBounds.X + space2, num + space3);
		}
		return array;
	}

	private PointF[] GetTopBorderPoints(WPageSetup pageSetup, RectangleF headerBounds, RectangleF footerBounds, RectangleF pageBounds)
	{
		float space = pageSetup.Borders.Left.Space;
		float space2 = pageSetup.Borders.Right.Space;
		float space3 = pageSetup.Borders.Top.Space;
		PointF[] array = new PointF[2]
		{
			new PointF(headerBounds.X - space, pageBounds.Y - space3),
			default(PointF)
		};
		if (pageSetup.Document.BordersSurroundHeader)
		{
			array[0] = new PointF(headerBounds.X - space, (headerBounds.Height == 0f) ? (pageBounds.Y - space3) : (headerBounds.Y - space3));
			array[1] = new PointF(pageSetup.ClientWidth + headerBounds.X + space2, (headerBounds.Height == 0f) ? (pageBounds.Y - space3) : (headerBounds.Y - space3));
		}
		else
		{
			array[0] = new PointF(headerBounds.X - space, pageBounds.Y - space3);
			array[1] = new PointF(pageSetup.ClientWidth + headerBounds.X + space2, pageBounds.Y - space3);
		}
		return array;
	}

	private float GetFontSize(TextWatermark textWatermark)
	{
		float result = 8f;
		float num = 0f;
		int num2 = 0;
		float num3 = 8f;
		num = textWatermark.Width;
		DocGen.Drawing.SkiaSharpHelper.Bitmap bmp = CreateBitmap(1, 1);
		Graphics graphicsFromImage = GetGraphicsFromImage(bmp);
		graphicsFromImage.PageUnit = GraphicsUnit.Point;
		while ((float)num2 <= num)
		{
			DocGen.Drawing.Font font = textWatermark.Document.FontSettings.GetFont(textWatermark.FontName, num3, FontStyle.Regular, FontScriptType.English);
			if (font.Name == "Arial Narrow" && font.Style == FontStyle.Bold)
			{
				textWatermark.Text = textWatermark.Text.Replace(ControlChar.NonBreakingSpaceChar, ControlChar.SpaceChar);
			}
			num2 = (int)graphicsFromImage.MeasureString(textWatermark.Text, font, font.Style, FontScriptType.English).Width;
			if ((float)num2 <= num)
			{
				result = num3;
				num3 += 1f;
			}
		}
		return result;
	}

	private void AdjustPictureBrightnessAndContrast(DocGen.Drawing.SkiaSharpHelper.Bitmap bmp, WPicture picture, ref DocGen.Drawing.SkiaSharpHelper.ImageAttributes imageAttributes, bool isWatermark)
	{
		if (!isWatermark)
		{
			return;
		}
		for (int i = 0; i < bmp.Width; i++)
		{
			for (int j = 0; j < bmp.Height; j++)
			{
				Color pixel = bmp.GetPixel(i, j);
				pixel = ChangeColorBrightness(pixel, 0.8f);
				bmp.SetPixel(i, j, pixel);
			}
		}
	}

	private void DrawImageWatermark(PictureWatermark pictureWatermark, RectangleF bounds, WPageSetup pageSetup)
	{
		DocGen.Drawing.SkiaSharpHelper.ImageAttributes imageAttributes = null;
		bool flag = pictureWatermark.Picture.RawFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.Tiff);
		DocGen.Drawing.SkiaSharpHelper.Bitmap bitmap = null;
		byte[] array = pictureWatermark.Picture.ImageData;
		if (flag)
		{
			array = WordDocument.RenderHelper.ConvertTiffToPng(array);
		}
		bitmap = CreateBitmap(1, 1).Decode(array) as DocGen.Drawing.SkiaSharpHelper.Bitmap;
		bounds = UpdateWaterMarkPosition(pictureWatermark.WordPicture, pageSetup, bounds);
		if (pictureWatermark.Washout)
		{
			AdjustPictureBrightnessAndContrast(bitmap, pictureWatermark.WordPicture, ref imageAttributes, isWatermark: true);
			PdfImage pdfImage = GetPdfImage(bitmap);
			PDFGraphics.DrawImage(pdfImage, new RectangleF((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height));
			return;
		}
		PdfImage pdfImage2 = null;
		if (flag)
		{
			MemoryStream memoryStream = new MemoryStream(pictureWatermark.Picture.ImageData);
			pdfImage2 = PDFGraphics.GetImage(memoryStream);
			memoryStream.Dispose();
		}
		else
		{
			pdfImage2 = GetPdfImage(bitmap);
		}
		PDFGraphics.DrawImage(pdfImage2, new RectangleF((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height));
	}

	internal void DrawEditableCheckbox(WCheckBox checkbox, LayoutedWidget ltWidget)
	{
		if (string.IsNullOrEmpty(checkbox.Name))
		{
			checkbox.Name = "checkboxformfield";
		}
		PdfCheckBoxField pdfCheckBoxField = new PdfCheckBoxField(PDFGraphics.Page, checkbox.Name);
		pdfCheckBoxField.Bounds = ltWidget.Bounds;
		pdfCheckBoxField.Checked = checkbox.Checked;
		pdfCheckBoxField.ToolTip = checkbox.StatusBarHelp;
		pdfCheckBoxField.Visible = checkbox.Enabled;
		pdfCheckBoxField.ForeColor = checkbox.CharacterFormat.HighlightColor;
		pdfCheckBoxField.BackColor = checkbox.CharacterFormat.TextColor;
		pdfCheckBoxField.BorderColor = Color.White;
		if (AutoTag)
		{
			pdfCheckBoxField.PdfTag = CreateAutoTag(PdfTagType.Form, "", "", IsOverLappedShapeWidget(ltWidget));
		}
		(PDFGraphics.Page as PdfPage).Document.Form.Fields.Add(pdfCheckBoxField);
	}

	internal void DrawCheckBox(WCheckBox checkbox, LayoutedWidget ltWidget)
	{
		PdfPen pdfPen = new PdfPen(new PdfColor(GetTextColor(checkbox.CharacterFormat)));
		pdfPen.Width = 0.7f;
		RectangleF rectangleF = new RectangleF(ltWidget.Bounds.X + pdfPen.Width, ltWidget.Bounds.Y + pdfPen.Width, ltWidget.Bounds.Width - 2f * pdfPen.Width, ltWidget.Bounds.Height - 2f * pdfPen.Width);
		PDFGraphics.DrawRectangle(pdfPen, rectangleF.X, rectangleF.Y, rectangleF.Width, rectangleF.Height);
		if (checkbox.Checked)
		{
			PointF point = new PointF(rectangleF.X, rectangleF.Y);
			PointF point2 = new PointF(rectangleF.Right, rectangleF.Bottom);
			PDFGraphics.DrawLine(pdfPen, point, point2);
			point = new PointF(rectangleF.Right, rectangleF.Top);
			point2 = new PointF(rectangleF.Left, rectangleF.Bottom);
			PDFGraphics.DrawLine(pdfPen, point, point2);
		}
	}

	internal void DrawShape(Shape shape, LayoutedWidget ltWidget)
	{
		RectangleF rectangleF = ltWidget.Bounds;
		ResetTransform();
		PdfPen pen = new PdfPen(new PdfColor((!shape.LineFormat.Color.IsEmpty) ? shape.LineFormat.Color : ((!shape.LineFormat.ForeColor.IsEmpty) ? shape.LineFormat.ForeColor : Color.Black)));
		PdfStructureElement pdfStructureElement = null;
		if (AutoTag)
		{
			pdfStructureElement = CreateAutoTag(PdfTagType.Figure, shape.AlternativeText, shape.Title, IsOverLappedShapeWidget(ltWidget));
		}
		GetDashStyle(shape.LineFormat.DashStyle, pen);
		pen.Width = shape.LineFormat.Weight;
		if (shape.Owner is GroupShape || shape.Owner is ChildGroupShape)
		{
			RectangleF bounds = ltWidget.Bounds;
			if (shape.Rotation != 0f)
			{
				bounds = UpdateClipBounds(bounds);
			}
			else
			{
				bounds = UpdateClipBounds(bounds, reverseClipping: false);
			}
			ClipBoundsContainer.Push(ltWidget.Bounds);
		}
		else if (shape.Rotation != 0f)
		{
			RectangleF boundingBoxCoordinates = GetBoundingBoxCoordinates(new RectangleF(0f, 0f, shape.Width, shape.Height), shape.Rotation);
			rectangleF = (ltWidget.Bounds = new RectangleF(rectangleF.X - boundingBoxCoordinates.X, rectangleF.Y - boundingBoxCoordinates.Y, shape.Width, shape.Height));
			if (!shape.TextFrame.Upright)
			{
				ClipBoundsContainer.Pop();
				ClipBoundsContainer.Push(ltWidget.Bounds);
			}
		}
		float num = shape.Rotation;
		bool flag = shape.FlipHorizontal;
		bool flag2 = shape.FlipVertical;
		if ((num != 0f || (num == 0f && (flag || flag2))) && shape.WrapFormat.TextWrappingStyle != TextWrappingStyle.Tight && shape.WrapFormat.TextWrappingStyle != TextWrappingStyle.Through)
		{
			if (num > 360f)
			{
				num %= 360f;
			}
			if (shape.AutoShapeType == DocGen.DocIO.DLS.AutoShapeType.Line || shape.AutoShapeType == DocGen.DocIO.DLS.AutoShapeType.StraightConnector)
			{
				flag = false;
				flag2 = false;
			}
			if (num != 0f || flag2 || flag)
			{
				PDFGraphics.Transform = GetTransformMatrix(rectangleF, num, flag, flag2);
			}
		}
		PdfPath graphicsPath = GetGraphicsPath(shape, rectangleF, ref pen);
		if (graphicsPath != null && pdfStructureElement != null)
		{
			graphicsPath.PdfTag = pdfStructureElement;
		}
		if (!shape.IsHorizontalRule && graphicsPath.PointCount == 0 && ((rectangleF.Width <= 0f) ^ (rectangleF.Height <= 0f)))
		{
			if (rectangleF.Width <= 0f)
			{
				rectangleF.Width = 0.1f;
			}
			else if (rectangleF.Height <= 0f)
			{
				rectangleF.Height = 0.1f;
			}
			graphicsPath.AddRectangle(rectangleF);
		}
		if (graphicsPath.PointCount > 0)
		{
			if (shape.FillFormat.Fill && (IsShapeNeedToBeFill(shape.AutoShapeType) || shape.IsHorizontalRule))
			{
				Color color = ((!shape.FillFormat.Color.IsEmpty && shape.FillFormat.Transparency != 100f) ? shape.FillFormat.Color : ((!shape.FillFormat.ForeColor.IsEmpty) ? shape.FillFormat.ForeColor : Color.Empty));
				if (shape.FillFormat.FillType == FillType.FillGradient && shape.FillFormat.GradientFill != null && shape.FillFormat.GradientFill.GradientStops.Count > 0)
				{
					color = shape.FillFormat.GradientFill.GradientStops[shape.FillFormat.GradientFill.GradientStops.Count - 1].Color;
				}
				if (shape.FillFormat.FillType == FillType.FillPicture && shape.FillFormat.ImageRecord != null)
				{
					DrawPictureFill(shape.FillFormat.ImageRecord, graphicsPath, rectangleF, shape.FillFormat.BlipFormat.Transparency);
				}
				else if (color != Color.Empty)
				{
					PdfBrush brush = new PdfSolidBrush(color);
					if (AutoTag)
					{
						graphicsPath.Brush = brush;
						PDFGraphics.DrawPath(brush, graphicsPath);
					}
					else
					{
						PDFGraphics.DrawPath(brush, graphicsPath);
					}
				}
			}
			if (shape.LineFormat.Line)
			{
				pen.LineJoin = GetLineJoin(GetPdfLineJoin(shape.LineFormat.LineJoin));
				if (shape.LineFormat.Style == LineStyle.ThinThin && (shape.AutoShapeType == DocGen.DocIO.DLS.AutoShapeType.Line || shape.AutoShapeType == DocGen.DocIO.DLS.AutoShapeType.StraightConnector) && !shape.FlipHorizontal && !shape.FlipVertical && !IsArrowPreserved(shape))
				{
					DrawLineShapeBasedOnLineType(shape, rectangleF, pen);
				}
				else
				{
					PDFGraphics.DrawPath(pen, graphicsPath);
				}
			}
		}
		else if (AutoTag)
		{
			graphicsPath.Pen = pen;
			PDFGraphics.DrawPath(pen, graphicsPath);
		}
		ResetTransform();
		if (shape.Rotation != 0f && shape.TextFrame.Upright)
		{
			shape.TextLayoutingBounds = GetBoundingBoxCoordinates(ltWidget.Bounds, shape.Rotation);
		}
	}

	private void DrawPictureFill(ImageRecord imageRecord, PdfPath path, RectangleF bounds, float transparency)
	{
		byte[] array = imageRecord.ImageBytes;
		bool flag = imageRecord.ImageFormat.Equals(DocGen.DocIO.DLS.Entities.ImageFormat.Tiff);
		if (flag)
		{
			array = WordDocument.RenderHelper.ConvertTiffToPng(array);
		}
		MemoryStream memoryStream = new MemoryStream(array);
		DocGen.Drawing.SkiaSharpHelper.Image image = DocGen.Drawing.SkiaSharpHelper.Image.FromStream(memoryStream);
		PdfImage pdfImage = null;
		if ((float)image.Width < bounds.Width || (float)image.Height < bounds.Height)
		{
			DocGen.Drawing.SkiaSharpHelper.Bitmap bmp = new DocGen.Drawing.SkiaSharpHelper.Bitmap(image, (int)bounds.Width, (int)bounds.Height);
			pdfImage = GetPdfImage(bmp);
		}
		else
		{
			if (flag && image != null)
			{
				image.RawFormat = DocGen.Drawing.ImageFormat.Tiff;
				image.ImageData = imageRecord.ImageBytes;
			}
			pdfImage = GetPdfImage(image);
		}
		PDFGraphics.Save();
		PDFGraphics.SetTransparency((100f - transparency) / 100f);
		if (path != null)
		{
			PDFGraphics.SetClip(new PdfPath(path.PathPoints, path.PathTypes));
		}
		PDFGraphics.TranslateTransform(bounds.X, bounds.Y);
		if (pdfImage != null)
		{
			PDFGraphics.DrawImage(pdfImage, new RectangleF(PointF.Empty, new SizeF(bounds.Width, bounds.Height)));
		}
		image.Dispose();
		PDFGraphics.Restore();
		memoryStream.Dispose();
	}

	private void ApplyImageTransparency(DocGen.Drawing.SkiaSharpHelper.ImageAttributes imageAttributes, float transparency)
	{
		DocGen.Drawing.SkiaSharpHelper.ColorMatrix colorMatrix = new DocGen.Drawing.SkiaSharpHelper.ColorMatrix();
		colorMatrix.Matrix33 = (100f - transparency) / 100f;
		imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
	}

	private PdfLineJoin GetPdfLineJoin(DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.LineJoin lineJoin)
	{
		return lineJoin switch
		{
			DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.LineJoin.Bevel => PdfLineJoin.Bevel, 
			DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.LineJoin.Miter => PdfLineJoin.Miter, 
			_ => PdfLineJoin.Round, 
		};
	}

	private void DrawLineShapeBasedOnLineType(Shape shape, RectangleF bounds, PdfPen pen)
	{
		pen.Width /= 3f;
		PdfPath pdfPath = new PdfPath();
		PointF[] linePointsBasedOnFlip = GetLinePointsBasedOnFlip(shape.FlipHorizontal, shape.FlipVertical, bounds);
		pdfPath.AddLines(linePointsBasedOnFlip);
		PDFGraphics.DrawPath(pen, pdfPath);
		PointF pointF = linePointsBasedOnFlip[0];
		PointF pointF2 = linePointsBasedOnFlip[1];
		pointF.X += pen.Width * 2f;
		pointF2.X += pen.Width * 2f;
		linePointsBasedOnFlip[0] = pointF;
		linePointsBasedOnFlip[1] = pointF2;
		PdfPath pdfPath2 = new PdfPath();
		pdfPath2.AddLines(linePointsBasedOnFlip);
		PDFGraphics.DrawPath(pen, pdfPath2);
	}

	private void DrawLineShapeBasedOnLineType(ChildShape shape, RectangleF bounds, PdfPen pen)
	{
		pen.Width /= 3f;
		PdfPath pdfPath = new PdfPath();
		PointF[] linePointsBasedOnFlip = GetLinePointsBasedOnFlip(shape.FlipHorizantal, shape.FlipVertical, bounds);
		pdfPath.AddLines(linePointsBasedOnFlip);
		PDFGraphics.DrawPath(pen, pdfPath);
		PointF pointF = linePointsBasedOnFlip[0];
		PointF pointF2 = linePointsBasedOnFlip[1];
		pointF.X += pen.Width * 2f;
		pointF2.X += pen.Width * 2f;
		linePointsBasedOnFlip[0] = pointF;
		linePointsBasedOnFlip[1] = pointF2;
		PdfPath pdfPath2 = new PdfPath();
		pdfPath2.AddLines(linePointsBasedOnFlip);
		PDFGraphics.DrawPath(pen, pdfPath2);
	}

	private bool IsArrowPreserved(Shape shape)
	{
		ArrowheadStyle endArrowheadStyle = shape.LineFormat.EndArrowheadStyle;
		if (endArrowheadStyle == ArrowheadStyle.ArrowheadTriangle || endArrowheadStyle == ArrowheadStyle.ArrowheadOpen)
		{
			return true;
		}
		endArrowheadStyle = shape.LineFormat.BeginArrowheadStyle;
		if (endArrowheadStyle == ArrowheadStyle.ArrowheadTriangle || endArrowheadStyle == ArrowheadStyle.ArrowheadOpen)
		{
			return true;
		}
		return false;
	}

	private bool IsArrowPreserved(ChildShape shape)
	{
		ArrowheadStyle endArrowheadStyle = shape.LineFormat.EndArrowheadStyle;
		if (endArrowheadStyle == ArrowheadStyle.ArrowheadTriangle || endArrowheadStyle == ArrowheadStyle.ArrowheadOpen)
		{
			return true;
		}
		endArrowheadStyle = shape.LineFormat.BeginArrowheadStyle;
		if (endArrowheadStyle == ArrowheadStyle.ArrowheadTriangle || endArrowheadStyle == ArrowheadStyle.ArrowheadOpen)
		{
			return true;
		}
		return false;
	}

	internal void DrawChildShape(ChildShape childShape, LayoutedWidget ltWidget)
	{
		switch (childShape.ElementType)
		{
		case EntityType.Picture:
		{
			WPicture wPicture = childShape.GetOwnerGroupShape().ConvertChildShapeToPicture(childShape);
			wPicture.SetOwner(childShape.Owner);
			wPicture.FlipHorizontal = childShape.FlipHorizantalToRender;
			wPicture.FlipVertical = childShape.FlipVerticalToRender;
			wPicture.Rotation = childShape.RotationToRender;
			DrawPicture(wPicture, ltWidget);
			break;
		}
		case EntityType.TextBox:
		{
			WTextBox wTextBox = childShape.GetOwnerGroupShape().ConvertChildShapeToTextbox(childShape);
			wTextBox.SetOwner(childShape.Owner);
			wTextBox.TextBoxFormat.FlipHorizontal = childShape.FlipHorizantalToRender;
			wTextBox.TextBoxFormat.FlipVertical = childShape.FlipVerticalToRender;
			wTextBox.TextBoxFormat.Rotation = childShape.RotationToRender;
			DrawTextBox(wTextBox, ltWidget);
			break;
		}
		case EntityType.AutoShape:
		{
			Shape shape = childShape.GetOwnerGroupShape().ConvertChildShapeToShape(childShape);
			shape.SetOwner(childShape.Owner);
			shape.FlipHorizontal = childShape.FlipHorizantalToRender;
			shape.FlipVertical = childShape.FlipVerticalToRender;
			shape.Rotation = childShape.RotationToRender;
			DrawShape(shape, ltWidget);
			break;
		}
		case EntityType.Chart:
			DrawChart(childShape.Chart, ltWidget);
			break;
		}
	}

	private bool IsShapeNeedToBeFill(DocGen.DocIO.DLS.AutoShapeType shapeType)
	{
		if (shapeType == DocGen.DocIO.DLS.AutoShapeType.Line || (uint)(shapeType - 227) <= 8u)
		{
			return false;
		}
		return true;
	}

	private void Rotate(ParagraphItem shapeFrame, float rotation, bool flipV, bool flipH, RectangleF rect)
	{
		float num = rotation;
		ParagraphItem paragraphItem = shapeFrame;
		while (paragraphItem.Owner is GroupShape || paragraphItem.Owner is ChildGroupShape)
		{
			float num2 = ((paragraphItem.Owner is GroupShape) ? (paragraphItem.Owner as GroupShape).Rotation : (paragraphItem.Owner as ChildGroupShape).Rotation);
			num += num2;
			paragraphItem = paragraphItem.Owner as ParagraphItem;
		}
		if (num > 360f)
		{
			num %= 360f;
		}
		if (num > 0f || flipV || flipH)
		{
			PDFGraphics.Transform = GetTransformMatrix(rect, num, flipV, flipH);
		}
	}

	private Matrix GetTransformMatrix(RectangleF bounds, float ang, bool flipH, bool flipV)
	{
		Matrix matrix = new Matrix();
		Matrix target = new Matrix(1f, 0f, 0f, -1f, 0f, 0f);
		Matrix target2 = new Matrix(-1f, 0f, 0f, 1f, 0f, 0f);
		PointF point = new PointF(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f);
		if (flipV)
		{
			MatrixMultiply(matrix, target, MatrixOrder.Append);
			MatrixTranslate(matrix, 0f, point.Y * 2f, MatrixOrder.Append);
		}
		if (flipH)
		{
			MatrixMultiply(matrix, target2, MatrixOrder.Append);
			MatrixTranslate(matrix, point.X * 2f, 0f, MatrixOrder.Append);
		}
		MatrixRotate(matrix, ang, point, MatrixOrder.Append);
		return matrix;
	}

	private PdfPath GetGraphicsPath(Shape shape, RectangleF bounds, ref PdfPen pen)
	{
		ShapePath shapePath = new ShapePath().Get(bounds, shape.ShapeGuide);
		PdfPath path = new PdfPath();
		switch (shape.AutoShapeType)
		{
		case DocGen.DocIO.DLS.AutoShapeType.Rectangle:
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartProcess:
			path.AddRectangle(bounds);
			return path;
		case DocGen.DocIO.DLS.AutoShapeType.RoundedRectangle:
			return shapePath.GetRoundedRectanglePath();
		case DocGen.DocIO.DLS.AutoShapeType.SnipSingleCornerRectangle:
			return shapePath.GetSnipSingleCornerRectanglePath();
		case DocGen.DocIO.DLS.AutoShapeType.SnipSameSideCornerRectangle:
			return shapePath.GetSnipSameSideCornerRectanglePath();
		case DocGen.DocIO.DLS.AutoShapeType.SnipDiagonalCornerRectangle:
			return shapePath.GetSnipDiagonalCornerRectanglePath();
		case DocGen.DocIO.DLS.AutoShapeType.SnipAndRoundSingleCornerRectangle:
			return shapePath.GetSnipAndRoundSingleCornerRectanglePath();
		case DocGen.DocIO.DLS.AutoShapeType.RoundSingleCornerRectangle:
			return shapePath.GetRoundSingleCornerRectanglePath();
		case DocGen.DocIO.DLS.AutoShapeType.RoundSameSideCornerRectangle:
			return shapePath.GetRoundSameSideCornerRectanglePath();
		case DocGen.DocIO.DLS.AutoShapeType.RoundDiagonalCornerRectangle:
			return shapePath.GetRoundDiagonalCornerRectanglePath();
		case DocGen.DocIO.DLS.AutoShapeType.Line:
		case DocGen.DocIO.DLS.AutoShapeType.StraightConnector:
		{
			if (shape.IsHorizontalRule)
			{
				path.AddRectangle(bounds);
				return path;
			}
			float width = pen.Width;
			if (pen.Width < 1f)
			{
				pen.Width = 1f;
			}
			bool isArrowHeadExist = false;
			PointF[] linePointsBasedOnFlip = GetLinePointsBasedOnFlip(shape.FlipHorizontal, shape.FlipVertical, bounds);
			DrawArrowHead(shape, pen, bounds, ref isArrowHeadExist, ref path, linePointsBasedOnFlip);
			if (!isArrowHeadExist)
			{
				path.AddLines(linePointsBasedOnFlip);
			}
			pen.Width = width;
			return path;
		}
		case DocGen.DocIO.DLS.AutoShapeType.ElbowConnector:
			return shapePath.GetBentConnectorPath();
		case DocGen.DocIO.DLS.AutoShapeType.BentConnector2:
			return shapePath.GetBendConnector2Path();
		case DocGen.DocIO.DLS.AutoShapeType.BentConnector4:
			return shapePath.GetBentConnector4Path();
		case DocGen.DocIO.DLS.AutoShapeType.BentConnector5:
			return shapePath.GetBentConnector5Path();
		case DocGen.DocIO.DLS.AutoShapeType.CurvedConnector:
			return shapePath.GetCurvedConnectorPath();
		case DocGen.DocIO.DLS.AutoShapeType.CurvedConnector2:
			return shapePath.GetCurvedConnector2Path();
		case DocGen.DocIO.DLS.AutoShapeType.CurvedConnector4:
			return shapePath.GetCurvedConnector4Path();
		case DocGen.DocIO.DLS.AutoShapeType.CurvedConnector5:
			return shapePath.GetCurvedConnector5Path();
		case DocGen.DocIO.DLS.AutoShapeType.Oval:
			path.AddEllipse(bounds);
			return path;
		case DocGen.DocIO.DLS.AutoShapeType.RightTriangle:
		{
			PointF[] linePoints = new PointF[3]
			{
				new PointF(bounds.X, bounds.Bottom),
				new PointF(bounds.X, bounds.Y),
				new PointF(bounds.Right, bounds.Bottom)
			};
			path.AddLines(linePoints);
			path.CloseFigure();
			return path;
		}
		case DocGen.DocIO.DLS.AutoShapeType.IsoscelesTriangle:
			return shapePath.GetTrianglePath();
		case DocGen.DocIO.DLS.AutoShapeType.Parallelogram:
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartData:
			return shapePath.GetParallelogramPath();
		case DocGen.DocIO.DLS.AutoShapeType.Trapezoid:
			if (shape.Is2007Shape)
			{
				return shapePath.GetFlowChartManualOperationPath();
			}
			return shapePath.GetTrapezoidPath();
		case DocGen.DocIO.DLS.AutoShapeType.Diamond:
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartDecision:
		{
			PointF[] linePoints = new PointF[4]
			{
				new PointF(bounds.X, bounds.Y + bounds.Height / 2f),
				new PointF(bounds.X + bounds.Width / 2f, bounds.Y),
				new PointF(bounds.Right, bounds.Y + bounds.Height / 2f),
				new PointF(bounds.X + bounds.Width / 2f, bounds.Bottom)
			};
			path.AddLines(linePoints);
			path.CloseFigure();
			break;
		}
		case DocGen.DocIO.DLS.AutoShapeType.RegularPentagon:
			return shapePath.GetRegularPentagonPath();
		case DocGen.DocIO.DLS.AutoShapeType.Hexagon:
			return shapePath.GetHexagonPath();
		case DocGen.DocIO.DLS.AutoShapeType.Heptagon:
			return shapePath.GetHeptagonPath();
		case DocGen.DocIO.DLS.AutoShapeType.Octagon:
			return shapePath.GetOctagonPath();
		case DocGen.DocIO.DLS.AutoShapeType.Decagon:
			return shapePath.GetDecagonPath();
		case DocGen.DocIO.DLS.AutoShapeType.Dodecagon:
			return shapePath.GetDodecagonPath();
		case DocGen.DocIO.DLS.AutoShapeType.Pie:
			return shapePath.GetPiePath();
		case DocGen.DocIO.DLS.AutoShapeType.Chord:
			return shapePath.GetChordPath();
		case DocGen.DocIO.DLS.AutoShapeType.Teardrop:
			return shapePath.GetTearDropPath();
		case DocGen.DocIO.DLS.AutoShapeType.Frame:
			return shapePath.GetFramePath();
		case DocGen.DocIO.DLS.AutoShapeType.HalfFrame:
			return shapePath.GetHalfFramePath();
		case DocGen.DocIO.DLS.AutoShapeType.L_Shape:
			return shapePath.GetL_ShapePath();
		case DocGen.DocIO.DLS.AutoShapeType.DiagonalStripe:
			return shapePath.GetDiagonalStripePath();
		case DocGen.DocIO.DLS.AutoShapeType.Cross:
			return shapePath.GetCrossPath();
		case DocGen.DocIO.DLS.AutoShapeType.Plaque:
			return shapePath.GetPlaquePath();
		case DocGen.DocIO.DLS.AutoShapeType.Can:
			return shapePath.GetCanPath();
		case DocGen.DocIO.DLS.AutoShapeType.Cube:
			return shapePath.GetCubePath();
		case DocGen.DocIO.DLS.AutoShapeType.Bevel:
			return shapePath.GetBevelPath();
		case DocGen.DocIO.DLS.AutoShapeType.Donut:
			return shapePath.GetDonutPath();
		case DocGen.DocIO.DLS.AutoShapeType.NoSymbol:
			return shapePath.GetNoSymbolPath();
		case DocGen.DocIO.DLS.AutoShapeType.BlockArc:
			return shapePath.GetBlockArcPath();
		case DocGen.DocIO.DLS.AutoShapeType.FoldedCorner:
			return shapePath.GetFoldedCornerPath();
		case DocGen.DocIO.DLS.AutoShapeType.SmileyFace:
		{
			PdfPath[] horizontalScroll = shapePath.GetSmileyFacePath();
			int num = 0;
			Color color = ((!shape.FillFormat.Color.IsEmpty) ? shape.FillFormat.Color : ((!shape.FillFormat.ForeColor.IsEmpty) ? shape.FillFormat.ForeColor : Color.Empty));
			if (shape.FillFormat.FillType == FillType.FillGradient && shape.FillFormat.GradientFill != null && shape.FillFormat.GradientFill.GradientStops.Count > 0)
			{
				color = shape.FillFormat.GradientFill.GradientStops[shape.FillFormat.GradientFill.GradientStops.Count - 1].Color;
			}
			PdfPath[] array = horizontalScroll;
			foreach (PdfPath path3 in array)
			{
				if (num == 2)
				{
					color = ChangeColorBrightness(color, -0.2f);
				}
				num++;
				if (shape.FillFormat.FillType == FillType.FillPicture && shape.FillFormat.ImageRecord != null)
				{
					DrawPictureFill(shape.FillFormat.ImageRecord, path3, bounds, shape.FillFormat.BlipFormat.Transparency);
				}
				else if (color != Color.Empty)
				{
					PdfBrush brush3 = new PdfSolidBrush(color);
					PDFGraphics.DrawPath(brush3, path3);
				}
				PDFGraphics.DrawPath(pen, path3);
			}
			break;
		}
		case DocGen.DocIO.DLS.AutoShapeType.Heart:
			return shapePath.GetHeartPath();
		case DocGen.DocIO.DLS.AutoShapeType.LightningBolt:
			return shapePath.GetLightningBoltPath();
		case DocGen.DocIO.DLS.AutoShapeType.Sun:
			return shapePath.GetSunPath();
		case DocGen.DocIO.DLS.AutoShapeType.Moon:
			return shapePath.GetMoonPath();
		case DocGen.DocIO.DLS.AutoShapeType.Cloud:
			return shapePath.GetCloudPath();
		case DocGen.DocIO.DLS.AutoShapeType.Arc:
		{
			PdfPath[] horizontalScroll = shapePath.GetArcPath();
			Color color = ((!shape.FillFormat.Color.IsEmpty) ? shape.FillFormat.Color : ((!shape.FillFormat.ForeColor.IsEmpty) ? shape.FillFormat.ForeColor : Color.Empty));
			if (shape.FillFormat.FillType == FillType.FillGradient && shape.FillFormat.GradientFill != null && shape.FillFormat.GradientFill.GradientStops.Count > 0)
			{
				color = shape.FillFormat.GradientFill.GradientStops[shape.FillFormat.GradientFill.GradientStops.Count - 1].Color;
			}
			if (shape.FillFormat.FillType == FillType.FillPicture && shape.FillFormat.ImageRecord != null)
			{
				DrawPictureFill(shape.FillFormat.ImageRecord, horizontalScroll[1], bounds, shape.FillFormat.BlipFormat.Transparency);
			}
			else if (color != Color.Empty && shape.FillFormat.Fill && !shape.FillFormat.IsDefaultFill)
			{
				PdfBrush brush2 = new PdfSolidBrush(color);
				PDFGraphics.DrawPath(brush2, horizontalScroll[1]);
			}
			if (shape.LineFormat.Line)
			{
				PDFGraphics.DrawPath(pen, horizontalScroll[0]);
			}
			break;
		}
		case DocGen.DocIO.DLS.AutoShapeType.DoubleBracket:
			return shapePath.GetDoubleBracketPath();
		case DocGen.DocIO.DLS.AutoShapeType.DoubleBrace:
			return shapePath.GetDoubleBracePath();
		case DocGen.DocIO.DLS.AutoShapeType.LeftBracket:
			return shapePath.GetLeftBracketPath();
		case DocGen.DocIO.DLS.AutoShapeType.RightBracket:
			return shapePath.GetRightBracketPath();
		case DocGen.DocIO.DLS.AutoShapeType.LeftBrace:
			return shapePath.GetLeftBracePath();
		case DocGen.DocIO.DLS.AutoShapeType.RightBrace:
			return shapePath.GetRightBracePath();
		case DocGen.DocIO.DLS.AutoShapeType.RightArrow:
			return shapePath.GetRightArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.LeftArrow:
			return shapePath.GetLeftArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.UpArrow:
			return shapePath.GetUpArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.DownArrow:
			return shapePath.GetDownArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.LeftRightArrow:
			return shapePath.GetLeftRightArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.UpDownArrow:
			return shapePath.GetUpDownArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.QuadArrow:
			return shapePath.GetQuadArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.BentArrow:
			return shapePath.GetBentArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.LeftRightUpArrow:
			return shapePath.GetLeftRightUpArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.UTurnArrow:
			return shapePath.GetUTrunArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.LeftUpArrow:
			return shapePath.GetLeftUpArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.BentUpArrow:
			return shapePath.GetBentUpArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.CurvedRightArrow:
			return shapePath.GetCurvedRightArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.CurvedLeftArrow:
			return shapePath.GetCurvedLeftArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.CurvedDownArrow:
			return shapePath.GetCurvedDownArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.CurvedUpArrow:
			return shapePath.GetCurvedUpArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.StripedRightArrow:
			return shapePath.GetStripedRightArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.NotchedRightArrow:
			return shapePath.GetNotchedRightArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.Pentagon:
			return shapePath.GetPentagonPath();
		case DocGen.DocIO.DLS.AutoShapeType.Chevron:
			return shapePath.GetChevronPath();
		case DocGen.DocIO.DLS.AutoShapeType.RightArrowCallout:
			return shapePath.GetRightArrowCalloutPath();
		case DocGen.DocIO.DLS.AutoShapeType.DownArrowCallout:
			return shapePath.GetDownArrowCalloutPath();
		case DocGen.DocIO.DLS.AutoShapeType.LeftArrowCallout:
			return shapePath.GetLeftArrowCalloutPath();
		case DocGen.DocIO.DLS.AutoShapeType.UpArrowCallout:
			return shapePath.GetUpArrowCalloutPath();
		case DocGen.DocIO.DLS.AutoShapeType.LeftRightArrowCallout:
			return shapePath.GetLeftRightArrowCalloutPath();
		case DocGen.DocIO.DLS.AutoShapeType.QuadArrowCallout:
			return shapePath.GetQuadArrowCalloutPath();
		case DocGen.DocIO.DLS.AutoShapeType.CircularArrow:
			return shapePath.GetCircularArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.MathPlus:
			return shapePath.GetMathPlusPath();
		case DocGen.DocIO.DLS.AutoShapeType.MathMinus:
			return shapePath.GetMathMinusPath();
		case DocGen.DocIO.DLS.AutoShapeType.MathMultiply:
			return shapePath.GetMathMultiplyPath();
		case DocGen.DocIO.DLS.AutoShapeType.MathDivision:
			return shapePath.GetMathDivisionPath();
		case DocGen.DocIO.DLS.AutoShapeType.MathEqual:
			return shapePath.GetMathEqualPath();
		case DocGen.DocIO.DLS.AutoShapeType.MathNotEqual:
			return shapePath.GetMathNotEqualPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartAlternateProcess:
			return shapePath.GetFlowChartAlternateProcessPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartPredefinedProcess:
			return shapePath.GetFlowChartPredefinedProcessPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartInternalStorage:
			return shapePath.GetFlowChartInternalStoragePath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartDocument:
			return shapePath.GetFlowChartDocumentPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartMultiDocument:
			return shapePath.GetFlowChartMultiDocumentPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartPreparation:
			return shapePath.GetFlowChartPreparationPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartManualInput:
			return shapePath.GetFlowChartManualInputPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartManualOperation:
			return shapePath.GetFlowChartManualOperationPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartConnector:
			return shapePath.GetFlowChartConnectorPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartOffPageConnector:
			return shapePath.GetFlowChartOffPageConnectorPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartCard:
			return shapePath.GetFlowChartCardPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartTerminator:
			return shapePath.GetFlowChartTerminatorPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartPunchedTape:
			return shapePath.GetFlowChartPunchedTapePath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartSummingJunction:
			return shapePath.GetFlowChartSummingJunctionPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartOr:
			return shapePath.GetFlowChartOrPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartCollate:
			return shapePath.GetFlowChartCollatePath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartSort:
			return shapePath.GetFlowChartSortPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartExtract:
			return shapePath.GetFlowChartExtractPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartMerge:
			return shapePath.GetFlowChartMergePath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartStoredData:
			return shapePath.GetFlowChartOnlineStoragePath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartDelay:
			return shapePath.GetFlowChartDelayPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartSequentialAccessStorage:
			return shapePath.GetFlowChartSequentialAccessStoragePath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartMagneticDisk:
			return shapePath.GetFlowChartMagneticDiskPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartDirectAccessStorage:
			return shapePath.GetFlowChartDirectAccessStoragePath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartDisplay:
			return shapePath.GetFlowChartDisplayPath();
		case DocGen.DocIO.DLS.AutoShapeType.RectangularCallout:
			return shapePath.GetRectangularCalloutPath();
		case DocGen.DocIO.DLS.AutoShapeType.RoundedRectangularCallout:
			return shapePath.GetRoundedRectangularCalloutPath();
		case DocGen.DocIO.DLS.AutoShapeType.OvalCallout:
			return shapePath.GetOvalCalloutPath();
		case DocGen.DocIO.DLS.AutoShapeType.CloudCallout:
			return shapePath.GetCloudCalloutPath();
		case DocGen.DocIO.DLS.AutoShapeType.LineCallout1:
		case DocGen.DocIO.DLS.AutoShapeType.LineCallout1NoBorder:
			return shapePath.GetLineCallout1Path();
		case DocGen.DocIO.DLS.AutoShapeType.LineCallout2:
		case DocGen.DocIO.DLS.AutoShapeType.LineCallout2NoBorder:
			return shapePath.GetLineCallout2Path();
		case DocGen.DocIO.DLS.AutoShapeType.LineCallout3:
		case DocGen.DocIO.DLS.AutoShapeType.LineCallout3NoBorder:
			return shapePath.GetLineCallout3Path();
		case DocGen.DocIO.DLS.AutoShapeType.LineCallout1AccentBar:
		case DocGen.DocIO.DLS.AutoShapeType.LineCallout1BorderAndAccentBar:
			return shapePath.GetLineCallout1AccentBarPath();
		case DocGen.DocIO.DLS.AutoShapeType.LineCallout2AccentBar:
		case DocGen.DocIO.DLS.AutoShapeType.LineCallout2BorderAndAccentBar:
			return shapePath.GetLineCallout2AccentBarPath();
		case DocGen.DocIO.DLS.AutoShapeType.LineCallout3AccentBar:
		case DocGen.DocIO.DLS.AutoShapeType.LineCallout3BorderAndAccentBar:
			return shapePath.GetLineCallout3AccentBarPath();
		case DocGen.DocIO.DLS.AutoShapeType.Explosion1:
			return shapePath.GetExplosion1();
		case DocGen.DocIO.DLS.AutoShapeType.Explosion2:
			return shapePath.GetExplosion2();
		case DocGen.DocIO.DLS.AutoShapeType.Star4Point:
			return shapePath.GetStar4Point();
		case DocGen.DocIO.DLS.AutoShapeType.Star5Point:
			return shapePath.GetStar5Point();
		case DocGen.DocIO.DLS.AutoShapeType.Star6Point:
			return shapePath.GetStar6Point();
		case DocGen.DocIO.DLS.AutoShapeType.Star7Point:
			return shapePath.GetStar7Point();
		case DocGen.DocIO.DLS.AutoShapeType.Star8Point:
			return shapePath.GetStar8Point();
		case DocGen.DocIO.DLS.AutoShapeType.Star10Point:
			return shapePath.GetStar10Point();
		case DocGen.DocIO.DLS.AutoShapeType.Star12Point:
			return shapePath.GetStar12Point();
		case DocGen.DocIO.DLS.AutoShapeType.Star16Point:
			return shapePath.GetStar16Point();
		case DocGen.DocIO.DLS.AutoShapeType.Star24Point:
			return shapePath.GetStar24Point();
		case DocGen.DocIO.DLS.AutoShapeType.Star32Point:
			return shapePath.GetStar32Point();
		case DocGen.DocIO.DLS.AutoShapeType.UpRibbon:
			return shapePath.GetUpRibbon();
		case DocGen.DocIO.DLS.AutoShapeType.DownRibbon:
			return shapePath.GetDownRibbon();
		case DocGen.DocIO.DLS.AutoShapeType.CurvedUpRibbon:
			return shapePath.GetCurvedUpRibbon();
		case DocGen.DocIO.DLS.AutoShapeType.CurvedDownRibbon:
			return shapePath.GetCurvedDownRibbon();
		case DocGen.DocIO.DLS.AutoShapeType.VerticalScroll:
			return shapePath.GetVerticalScroll();
		case DocGen.DocIO.DLS.AutoShapeType.HorizontalScroll:
		{
			PdfPath[] horizontalScroll = shapePath.GetHorizontalScroll();
			Color color = ((!shape.FillFormat.Color.IsEmpty) ? shape.FillFormat.Color : ((!shape.FillFormat.ForeColor.IsEmpty) ? shape.FillFormat.ForeColor : Color.Empty));
			if (shape.FillFormat.FillType == FillType.FillGradient && shape.FillFormat.GradientFill != null && shape.FillFormat.GradientFill.GradientStops.Count > 0)
			{
				color = shape.FillFormat.GradientFill.GradientStops[shape.FillFormat.GradientFill.GradientStops.Count - 1].Color;
			}
			PdfPath[] array = horizontalScroll;
			foreach (PdfPath path2 in array)
			{
				if (shape.FillFormat.FillType == FillType.FillPicture && shape.FillFormat.ImageRecord != null)
				{
					DrawPictureFill(shape.FillFormat.ImageRecord, path2, bounds, shape.FillFormat.BlipFormat.Transparency);
				}
				else if (color != Color.Empty)
				{
					PdfBrush brush = new PdfSolidBrush(color);
					PDFGraphics.DrawPath(brush, path2);
				}
				PDFGraphics.DrawPath(pen, path2);
			}
			break;
		}
		case DocGen.DocIO.DLS.AutoShapeType.Wave:
			return shapePath.GetWave();
		case DocGen.DocIO.DLS.AutoShapeType.DoubleWave:
			return shapePath.GetDoubleWave();
		}
		if (shape.Is2007Shape && shape.VMLPathPoints != null && shape.VMLPathPoints.Count > 0)
		{
			return shapePath.GetVMLCustomShapePath(shape.VMLPathPoints);
		}
		if (shape.Path2DList != null && shape.Path2DList.Count > 0)
		{
			return shapePath.GetCustomGeomentryPath(bounds, path, shape);
		}
		return path;
	}

	private PdfPath GetGraphicsPath(ChildShape shape, RectangleF bounds, ref PdfPen pen)
	{
		ShapePath shapePath = new ShapePath().Get(bounds, shape.ShapeGuide);
		PdfPath path = new PdfPath();
		DocGen.DocIO.DLS.AutoShapeType autoShapeType = shape.AutoShapeType;
		if (autoShapeType == DocGen.DocIO.DLS.AutoShapeType.Unknown && shape.IsTextBoxShape)
		{
			autoShapeType = DocGen.DocIO.DLS.AutoShapeType.Rectangle;
		}
		switch (autoShapeType)
		{
		case DocGen.DocIO.DLS.AutoShapeType.Rectangle:
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartProcess:
			path.AddRectangle(bounds);
			return path;
		case DocGen.DocIO.DLS.AutoShapeType.RoundedRectangle:
			return shapePath.GetRoundedRectanglePath();
		case DocGen.DocIO.DLS.AutoShapeType.SnipSingleCornerRectangle:
			return shapePath.GetSnipSingleCornerRectanglePath();
		case DocGen.DocIO.DLS.AutoShapeType.SnipSameSideCornerRectangle:
			return shapePath.GetSnipSameSideCornerRectanglePath();
		case DocGen.DocIO.DLS.AutoShapeType.SnipDiagonalCornerRectangle:
			return shapePath.GetSnipDiagonalCornerRectanglePath();
		case DocGen.DocIO.DLS.AutoShapeType.SnipAndRoundSingleCornerRectangle:
			return shapePath.GetSnipAndRoundSingleCornerRectanglePath();
		case DocGen.DocIO.DLS.AutoShapeType.RoundSingleCornerRectangle:
			return shapePath.GetRoundSingleCornerRectanglePath();
		case DocGen.DocIO.DLS.AutoShapeType.RoundSameSideCornerRectangle:
			return shapePath.GetRoundSameSideCornerRectanglePath();
		case DocGen.DocIO.DLS.AutoShapeType.RoundDiagonalCornerRectangle:
			return shapePath.GetRoundDiagonalCornerRectanglePath();
		case DocGen.DocIO.DLS.AutoShapeType.Line:
		case DocGen.DocIO.DLS.AutoShapeType.StraightConnector:
		{
			if (pen.Width < 1f)
			{
				pen.Width = 1f;
			}
			bool isArrowHeadExist = false;
			PointF[] linePointsBasedOnFlip = GetLinePointsBasedOnFlip(shape.FlipHorizantal, shape.FlipVertical, bounds);
			DrawArrowHead(shape, pen, bounds, ref isArrowHeadExist, ref path, linePointsBasedOnFlip);
			if (!isArrowHeadExist)
			{
				path.AddLines(linePointsBasedOnFlip);
			}
			return path;
		}
		case DocGen.DocIO.DLS.AutoShapeType.ElbowConnector:
			path = shapePath.GetBentConnectorPath();
			if (shape.FlipVertical || shape.FlipHorizantal)
			{
				PDFGraphics.Transform = GetTransformMatrix(bounds, 0f, shape.FlipHorizantal, shape.FlipVertical);
			}
			return path;
		case DocGen.DocIO.DLS.AutoShapeType.CurvedConnector:
			return shapePath.GetCurvedConnectorPath();
		case DocGen.DocIO.DLS.AutoShapeType.Oval:
			path.AddEllipse(bounds);
			return path;
		case DocGen.DocIO.DLS.AutoShapeType.RightTriangle:
		{
			PointF[] linePoints = new PointF[3]
			{
				new PointF(bounds.X, bounds.Bottom),
				new PointF(bounds.X, bounds.Y),
				new PointF(bounds.Right, bounds.Bottom)
			};
			path.AddLines(linePoints);
			path.CloseFigure();
			return path;
		}
		case DocGen.DocIO.DLS.AutoShapeType.IsoscelesTriangle:
			return shapePath.GetTrianglePath();
		case DocGen.DocIO.DLS.AutoShapeType.Parallelogram:
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartData:
			return shapePath.GetParallelogramPath();
		case DocGen.DocIO.DLS.AutoShapeType.Trapezoid:
			if (shape.Is2007Shape)
			{
				return shapePath.GetFlowChartManualOperationPath();
			}
			return shapePath.GetTrapezoidPath();
		case DocGen.DocIO.DLS.AutoShapeType.Diamond:
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartDecision:
		{
			PointF[] linePoints = new PointF[4]
			{
				new PointF(bounds.X, bounds.Y + bounds.Height / 2f),
				new PointF(bounds.X + bounds.Width / 2f, bounds.Y),
				new PointF(bounds.Right, bounds.Y + bounds.Height / 2f),
				new PointF(bounds.X + bounds.Width / 2f, bounds.Bottom)
			};
			path.AddLines(linePoints);
			path.CloseFigure();
			break;
		}
		case DocGen.DocIO.DLS.AutoShapeType.RegularPentagon:
			return shapePath.GetRegularPentagonPath();
		case DocGen.DocIO.DLS.AutoShapeType.Hexagon:
			return shapePath.GetHexagonPath();
		case DocGen.DocIO.DLS.AutoShapeType.Heptagon:
			return shapePath.GetHeptagonPath();
		case DocGen.DocIO.DLS.AutoShapeType.Octagon:
			return shapePath.GetOctagonPath();
		case DocGen.DocIO.DLS.AutoShapeType.Decagon:
			return shapePath.GetDecagonPath();
		case DocGen.DocIO.DLS.AutoShapeType.Dodecagon:
			return shapePath.GetDodecagonPath();
		case DocGen.DocIO.DLS.AutoShapeType.Pie:
			return shapePath.GetPiePath();
		case DocGen.DocIO.DLS.AutoShapeType.Chord:
			return shapePath.GetChordPath();
		case DocGen.DocIO.DLS.AutoShapeType.Teardrop:
			return shapePath.GetTearDropPath();
		case DocGen.DocIO.DLS.AutoShapeType.Frame:
			return shapePath.GetFramePath();
		case DocGen.DocIO.DLS.AutoShapeType.HalfFrame:
			return shapePath.GetHalfFramePath();
		case DocGen.DocIO.DLS.AutoShapeType.L_Shape:
			return shapePath.GetL_ShapePath();
		case DocGen.DocIO.DLS.AutoShapeType.DiagonalStripe:
			return shapePath.GetDiagonalStripePath();
		case DocGen.DocIO.DLS.AutoShapeType.Cross:
			return shapePath.GetCrossPath();
		case DocGen.DocIO.DLS.AutoShapeType.Plaque:
			return shapePath.GetPlaquePath();
		case DocGen.DocIO.DLS.AutoShapeType.Can:
			return shapePath.GetCanPath();
		case DocGen.DocIO.DLS.AutoShapeType.Cube:
			return shapePath.GetCubePath();
		case DocGen.DocIO.DLS.AutoShapeType.Bevel:
			return shapePath.GetBevelPath();
		case DocGen.DocIO.DLS.AutoShapeType.Donut:
			return shapePath.GetDonutPath();
		case DocGen.DocIO.DLS.AutoShapeType.NoSymbol:
			return shapePath.GetNoSymbolPath();
		case DocGen.DocIO.DLS.AutoShapeType.BlockArc:
			return shapePath.GetBlockArcPath();
		case DocGen.DocIO.DLS.AutoShapeType.FoldedCorner:
			return shapePath.GetFoldedCornerPath();
		case DocGen.DocIO.DLS.AutoShapeType.SmileyFace:
		{
			PdfPath[] horizontalScroll = shapePath.GetSmileyFacePath();
			Color color = ((!shape.FillFormat.Color.IsEmpty) ? shape.FillFormat.Color : ((!shape.FillFormat.ForeColor.IsEmpty) ? shape.FillFormat.ForeColor : Color.Empty));
			if (shape.FillFormat.FillType == FillType.FillGradient && shape.FillFormat.GradientFill != null && shape.FillFormat.GradientFill.GradientStops.Count > 0)
			{
				color = shape.FillFormat.GradientFill.GradientStops[shape.FillFormat.GradientFill.GradientStops.Count - 1].Color;
			}
			PdfPath[] array = horizontalScroll;
			foreach (PdfPath path3 in array)
			{
				if (color != Color.Empty)
				{
					PdfBrush brush3 = new PdfSolidBrush(color);
					PDFGraphics.DrawPath(brush3, path3);
				}
				PDFGraphics.DrawPath(pen, path3);
			}
			break;
		}
		case DocGen.DocIO.DLS.AutoShapeType.Heart:
			return shapePath.GetHeartPath();
		case DocGen.DocIO.DLS.AutoShapeType.LightningBolt:
			return shapePath.GetLightningBoltPath();
		case DocGen.DocIO.DLS.AutoShapeType.Sun:
			return shapePath.GetSunPath();
		case DocGen.DocIO.DLS.AutoShapeType.Moon:
			return shapePath.GetMoonPath();
		case DocGen.DocIO.DLS.AutoShapeType.Cloud:
			return shapePath.GetCloudPath();
		case DocGen.DocIO.DLS.AutoShapeType.Arc:
		{
			PdfPath[] horizontalScroll = shapePath.GetArcPath();
			Color color = ((!shape.FillFormat.Color.IsEmpty) ? shape.FillFormat.Color : ((!shape.FillFormat.ForeColor.IsEmpty) ? shape.FillFormat.ForeColor : Color.Empty));
			if (shape.FillFormat.FillType == FillType.FillGradient && shape.FillFormat.GradientFill != null && shape.FillFormat.GradientFill.GradientStops.Count > 0)
			{
				color = shape.FillFormat.GradientFill.GradientStops[shape.FillFormat.GradientFill.GradientStops.Count - 1].Color;
			}
			if (color != Color.Empty)
			{
				PdfBrush brush2 = new PdfSolidBrush(color);
				PDFGraphics.DrawPath(brush2, horizontalScroll[1]);
			}
			PDFGraphics.DrawPath(pen, horizontalScroll[0]);
			break;
		}
		case DocGen.DocIO.DLS.AutoShapeType.DoubleBracket:
			return shapePath.GetDoubleBracketPath();
		case DocGen.DocIO.DLS.AutoShapeType.DoubleBrace:
			return shapePath.GetDoubleBracePath();
		case DocGen.DocIO.DLS.AutoShapeType.LeftBracket:
			return shapePath.GetLeftBracketPath();
		case DocGen.DocIO.DLS.AutoShapeType.RightBracket:
			return shapePath.GetRightBracketPath();
		case DocGen.DocIO.DLS.AutoShapeType.LeftBrace:
			return shapePath.GetLeftBracePath();
		case DocGen.DocIO.DLS.AutoShapeType.RightBrace:
			return shapePath.GetRightBracePath();
		case DocGen.DocIO.DLS.AutoShapeType.RightArrow:
			return shapePath.GetRightArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.LeftArrow:
			return shapePath.GetLeftArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.UpArrow:
			return shapePath.GetUpArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.DownArrow:
			return shapePath.GetDownArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.LeftRightArrow:
			return shapePath.GetLeftRightArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.UpDownArrow:
			return shapePath.GetUpDownArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.QuadArrow:
			return shapePath.GetQuadArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.BentArrow:
			return shapePath.GetBentArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.LeftRightUpArrow:
			return shapePath.GetLeftRightUpArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.UTurnArrow:
			return shapePath.GetUTrunArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.LeftUpArrow:
			return shapePath.GetLeftUpArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.BentUpArrow:
			return shapePath.GetBentUpArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.CurvedRightArrow:
			return shapePath.GetCurvedRightArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.CurvedLeftArrow:
			return shapePath.GetCurvedLeftArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.CurvedDownArrow:
			return shapePath.GetCurvedDownArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.CurvedUpArrow:
			return shapePath.GetCurvedUpArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.StripedRightArrow:
			return shapePath.GetStripedRightArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.NotchedRightArrow:
			return shapePath.GetNotchedRightArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.Pentagon:
			return shapePath.GetPentagonPath();
		case DocGen.DocIO.DLS.AutoShapeType.Chevron:
			return shapePath.GetChevronPath();
		case DocGen.DocIO.DLS.AutoShapeType.RightArrowCallout:
			return shapePath.GetRightArrowCalloutPath();
		case DocGen.DocIO.DLS.AutoShapeType.DownArrowCallout:
			return shapePath.GetDownArrowCalloutPath();
		case DocGen.DocIO.DLS.AutoShapeType.LeftArrowCallout:
			return shapePath.GetLeftArrowCalloutPath();
		case DocGen.DocIO.DLS.AutoShapeType.UpArrowCallout:
			return shapePath.GetUpArrowCalloutPath();
		case DocGen.DocIO.DLS.AutoShapeType.LeftRightArrowCallout:
			return shapePath.GetLeftRightArrowCalloutPath();
		case DocGen.DocIO.DLS.AutoShapeType.QuadArrowCallout:
			return shapePath.GetQuadArrowCalloutPath();
		case DocGen.DocIO.DLS.AutoShapeType.CircularArrow:
			return shapePath.GetCircularArrowPath();
		case DocGen.DocIO.DLS.AutoShapeType.MathPlus:
			return shapePath.GetMathPlusPath();
		case DocGen.DocIO.DLS.AutoShapeType.MathMinus:
			return shapePath.GetMathMinusPath();
		case DocGen.DocIO.DLS.AutoShapeType.MathMultiply:
			return shapePath.GetMathMultiplyPath();
		case DocGen.DocIO.DLS.AutoShapeType.MathDivision:
			return shapePath.GetMathDivisionPath();
		case DocGen.DocIO.DLS.AutoShapeType.MathEqual:
			return shapePath.GetMathEqualPath();
		case DocGen.DocIO.DLS.AutoShapeType.MathNotEqual:
			return shapePath.GetMathNotEqualPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartAlternateProcess:
			return shapePath.GetFlowChartAlternateProcessPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartPredefinedProcess:
			return shapePath.GetFlowChartPredefinedProcessPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartInternalStorage:
			return shapePath.GetFlowChartInternalStoragePath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartDocument:
			return shapePath.GetFlowChartDocumentPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartMultiDocument:
			return shapePath.GetFlowChartMultiDocumentPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartPreparation:
			return shapePath.GetFlowChartPreparationPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartManualInput:
			return shapePath.GetFlowChartManualInputPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartManualOperation:
			return shapePath.GetFlowChartManualOperationPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartConnector:
			return shapePath.GetFlowChartConnectorPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartOffPageConnector:
			return shapePath.GetFlowChartOffPageConnectorPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartCard:
			return shapePath.GetFlowChartCardPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartTerminator:
			return shapePath.GetFlowChartTerminatorPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartPunchedTape:
			return shapePath.GetFlowChartPunchedTapePath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartSummingJunction:
			return shapePath.GetFlowChartSummingJunctionPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartOr:
			return shapePath.GetFlowChartOrPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartCollate:
			return shapePath.GetFlowChartCollatePath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartSort:
			return shapePath.GetFlowChartSortPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartExtract:
			return shapePath.GetFlowChartExtractPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartMerge:
			return shapePath.GetFlowChartMergePath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartStoredData:
			return shapePath.GetFlowChartOnlineStoragePath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartDelay:
			return shapePath.GetFlowChartDelayPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartSequentialAccessStorage:
			return shapePath.GetFlowChartSequentialAccessStoragePath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartMagneticDisk:
			return shapePath.GetFlowChartMagneticDiskPath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartDirectAccessStorage:
			return shapePath.GetFlowChartDirectAccessStoragePath();
		case DocGen.DocIO.DLS.AutoShapeType.FlowChartDisplay:
			return shapePath.GetFlowChartDisplayPath();
		case DocGen.DocIO.DLS.AutoShapeType.RectangularCallout:
			return shapePath.GetRectangularCalloutPath();
		case DocGen.DocIO.DLS.AutoShapeType.RoundedRectangularCallout:
			return shapePath.GetRoundedRectangularCalloutPath();
		case DocGen.DocIO.DLS.AutoShapeType.OvalCallout:
			return shapePath.GetOvalCalloutPath();
		case DocGen.DocIO.DLS.AutoShapeType.CloudCallout:
			return shapePath.GetCloudCalloutPath();
		case DocGen.DocIO.DLS.AutoShapeType.LineCallout1:
		case DocGen.DocIO.DLS.AutoShapeType.LineCallout1NoBorder:
			return shapePath.GetLineCallout1Path();
		case DocGen.DocIO.DLS.AutoShapeType.LineCallout2:
		case DocGen.DocIO.DLS.AutoShapeType.LineCallout2NoBorder:
			return shapePath.GetLineCallout2Path();
		case DocGen.DocIO.DLS.AutoShapeType.LineCallout3:
		case DocGen.DocIO.DLS.AutoShapeType.LineCallout3NoBorder:
			return shapePath.GetLineCallout3Path();
		case DocGen.DocIO.DLS.AutoShapeType.LineCallout1AccentBar:
		case DocGen.DocIO.DLS.AutoShapeType.LineCallout1BorderAndAccentBar:
			return shapePath.GetLineCallout1AccentBarPath();
		case DocGen.DocIO.DLS.AutoShapeType.LineCallout2AccentBar:
		case DocGen.DocIO.DLS.AutoShapeType.LineCallout2BorderAndAccentBar:
			return shapePath.GetLineCallout2AccentBarPath();
		case DocGen.DocIO.DLS.AutoShapeType.LineCallout3AccentBar:
		case DocGen.DocIO.DLS.AutoShapeType.LineCallout3BorderAndAccentBar:
			return shapePath.GetLineCallout3AccentBarPath();
		case DocGen.DocIO.DLS.AutoShapeType.Explosion1:
			return shapePath.GetExplosion1();
		case DocGen.DocIO.DLS.AutoShapeType.Explosion2:
			return shapePath.GetExplosion2();
		case DocGen.DocIO.DLS.AutoShapeType.Star4Point:
			return shapePath.GetStar4Point();
		case DocGen.DocIO.DLS.AutoShapeType.Star5Point:
			return shapePath.GetStar5Point();
		case DocGen.DocIO.DLS.AutoShapeType.Star6Point:
			return shapePath.GetStar6Point();
		case DocGen.DocIO.DLS.AutoShapeType.Star7Point:
			return shapePath.GetStar7Point();
		case DocGen.DocIO.DLS.AutoShapeType.Star8Point:
			return shapePath.GetStar8Point();
		case DocGen.DocIO.DLS.AutoShapeType.Star10Point:
			return shapePath.GetStar10Point();
		case DocGen.DocIO.DLS.AutoShapeType.Star12Point:
			return shapePath.GetStar12Point();
		case DocGen.DocIO.DLS.AutoShapeType.Star16Point:
			return shapePath.GetStar16Point();
		case DocGen.DocIO.DLS.AutoShapeType.Star24Point:
			return shapePath.GetStar24Point();
		case DocGen.DocIO.DLS.AutoShapeType.Star32Point:
			return shapePath.GetStar32Point();
		case DocGen.DocIO.DLS.AutoShapeType.UpRibbon:
			return shapePath.GetUpRibbon();
		case DocGen.DocIO.DLS.AutoShapeType.DownRibbon:
			return shapePath.GetDownRibbon();
		case DocGen.DocIO.DLS.AutoShapeType.CurvedUpRibbon:
			return shapePath.GetCurvedUpRibbon();
		case DocGen.DocIO.DLS.AutoShapeType.CurvedDownRibbon:
			return shapePath.GetCurvedDownRibbon();
		case DocGen.DocIO.DLS.AutoShapeType.VerticalScroll:
			return shapePath.GetVerticalScroll();
		case DocGen.DocIO.DLS.AutoShapeType.HorizontalScroll:
		{
			PdfPath[] horizontalScroll = shapePath.GetHorizontalScroll();
			Color color = ((!shape.FillFormat.Color.IsEmpty) ? shape.FillFormat.Color : ((!shape.FillFormat.ForeColor.IsEmpty) ? shape.FillFormat.ForeColor : Color.Empty));
			if (shape.FillFormat.FillType == FillType.FillGradient && shape.FillFormat.GradientFill != null && shape.FillFormat.GradientFill.GradientStops.Count > 0)
			{
				color = shape.FillFormat.GradientFill.GradientStops[shape.FillFormat.GradientFill.GradientStops.Count - 1].Color;
			}
			PdfPath[] array = horizontalScroll;
			foreach (PdfPath path2 in array)
			{
				if (color != Color.Empty)
				{
					PdfBrush brush = new PdfSolidBrush(color);
					PDFGraphics.DrawPath(brush, path2);
				}
				PDFGraphics.DrawPath(pen, path2);
			}
			break;
		}
		case DocGen.DocIO.DLS.AutoShapeType.Wave:
			return shapePath.GetWave();
		case DocGen.DocIO.DLS.AutoShapeType.DoubleWave:
			return shapePath.GetDoubleWave();
		}
		return path;
	}

	public SizeF MeasureImage(WPicture image)
	{
		float width = image.Size.Width * image.WidthScale / 100f;
		float height = image.Size.Height * image.HeightScale / 100f;
		return new SizeF(width, height);
	}

	public SizeF MeasurePictureBulletSize(WPicture picture, DocGen.Drawing.Font font)
	{
		SizeF result = MeasureImage(picture);
		float num = ((font.Size <= 4f) ? 4f : font.Size);
		float num2 = (float)Math.Round(result.Width / result.Height, MidpointRounding.AwayFromZero);
		float num3 = (float)Math.Round(result.Height / result.Width, MidpointRounding.AwayFromZero);
		num2 = ((num2 <= 0f) ? 1f : num2);
		num3 = ((num3 <= 0f) ? 1f : num3);
		result.Width = ((num == 12f) ? (num2 * 10f) : (num2 * (num - 12f + 10f)));
		result.Height = ((num == 12f) ? (num3 * 10f) : (num3 * (num - 12f + 10f)));
		return result;
	}

	public SizeF MeasureString(string text, DocGen.Drawing.Font font, StringFormat format, FontScriptType scriptType)
	{
		return MeasureString(text, font, format, null, isMeasureFromTabList: false, scriptType);
	}

	public SizeF MeasureString(string text, DocGen.Drawing.Font font, StringFormat format, WCharacterFormat charFormat, bool isMeasureFromTabList, FontScriptType scriptType)
	{
		return MeasureString(text, font, format, charFormat, isMeasureFromTabList, isMeasureFromSmallCapString: false, scriptType);
	}

	public RectangleF GetExactStringBounds(string text, DocGen.Drawing.Font font)
	{
		RectangleF rectangleF = default(RectangleF);
		DocGen.Drawing.SkiaSharpHelper.GraphicsPath graphicsPath = CreateGraphicsPath();
		IFontFamily fontFamily = WordDocument.RenderHelper.GetFontFamily(font.Name, font.Size);
		graphicsPath.AddString(text, font.Name, (int)font.Style, font.Size, new PointF(0f, 0f), StringFormat.GenericDefault);
		rectangleF = graphicsPath.GetBounds();
		rectangleF.Y = fontFamily.GetCellAscent(font.Style) - Math.Abs(rectangleF.Y);
		return rectangleF;
	}

	public SizeF MeasureString(string text, DocGen.Drawing.Font font, StringFormat format, WCharacterFormat charFormat, bool isMeasureFromTabList, bool isMeasureFromSmallCapString, FontScriptType scriptType)
	{
		float num = 0f;
		float num2 = 0f;
		if (charFormat != null)
		{
			num = charFormat.CharacterSpacing;
			num2 = charFormat.Scaling;
		}
		if (text == null || text.Length == 0)
		{
			return SizeF.Empty;
		}
		text = text.Replace('\u001e'.ToString(), "-");
		text = text.Replace('\u00ad'.ToString(), "-");
		text = text.Replace('\u200d'.ToString(), "");
		if (text.Length == 0)
		{
			return SizeF.Empty;
		}
		if (format == null)
		{
			format = new StringFormat(StringFormt);
		}
		string name = font.Name;
		if (charFormat != null && name == "Arial Unicode MS")
		{
			FontStyle style = font.Style;
			style &= (FontStyle)(-2);
			font = charFormat.Document.FontSettings.GetFont(name, font.Size, style, scriptType);
		}
		SizeF size = SizeF.Empty;
		if (text.Length > 4000)
		{
			text = text[..4000];
		}
		if (charFormat != null && charFormat.AllCaps)
		{
			text = text.ToUpper();
		}
		StringBuilder stringBuilder = new StringBuilder(text);
		stringBuilder = stringBuilder.Replace('\u001e'.ToString(), "-");
		if ((name == "Arial Narrow" && font.Style == FontStyle.Bold) || (name == "Bookman Old Style" && (font.Style == FontStyle.Regular || font.Style == FontStyle.Italic)))
		{
			stringBuilder = stringBuilder.Replace(ControlChar.NonBreakingSpaceChar, ControlChar.SpaceChar);
		}
		stringBuilder = stringBuilder.Replace('\u00ad'.ToString(), "-");
		text = stringBuilder.ToString();
		DocGen.Drawing.Font font2 = ((charFormat != null && charFormat.SubSuperScript != 0) ? charFormat.Document.FontSettings.GetFont(name, GetSubSuperScriptFontSize(font), font.Style, scriptType) : font);
		if (font2.Style != 0 && scriptType != 0)
		{
			font2 = WordDocument.RenderHelper.GetRegularStyleFontToMeasure(font2, text, scriptType);
		}
		font2 = ((charFormat == null || charFormat.Document.FontSettings.FallbackFonts.Count <= 0) ? GetAlternateFontToRender(text, font2, charFormat) : WordDocument.RenderHelper.GetFallbackFont(font2, text, scriptType, charFormat.Document.FontSettings.FallbackFonts, charFormat.Document.FontSettings.FontStreams));
		if (charFormat != null && charFormat.IsKernFont)
		{
			size.Width = Graphics.MeasureString(text.ToString(), font2, new PointF(0f, 0f), format, font.Style, scriptType).Width;
		}
		else
		{
			size.Width = GraphicsBmp.MeasureString(text, font2, new PointF(0f, 0f), format, font.Style, scriptType).Width;
		}
		DocGen.Drawing.Font font3 = ((!font.Name.Equals(font2.Name, StringComparison.OrdinalIgnoreCase)) ? new DocGen.Drawing.Font(font2.Name, font.Size, font.Style) : font);
		size.Height = Graphics.MeasureString(text[0].ToString(), font3, new PointF(0f, 0f), format, font.Style, scriptType).Height;
		if (charFormat != null && charFormat.SmallCaps)
		{
			MeasureSmallCapString(text, ref size, font2, format, charFormat, scriptType);
		}
		if (charFormat != null && num2 != 100f && !isMeasureFromSmallCapString)
		{
			size = new SizeF(size.Width * (num2 / 100f), size.Height);
		}
		if (num != 0f)
		{
			size.Width += (float)text.Length * num;
		}
		if (!isMeasureFromTabList && (charFormat == null || (!charFormat.ComplexScript && text != "")) && name == "Arial Unicode MS")
		{
			size.Height = GetExceededLineHeightForArialUnicodeMSFont(font, isAscent: false, scriptType);
		}
		size.Height += GetFactor(name) * font.Size;
		if (text == '\u001f'.ToString())
		{
			size.Width = 0f;
		}
		return size;
	}

	internal PdfFont GetFallbackPdfFont(PdfFont pdfFont, DocGen.Drawing.Font systemFont, string inputText, FontScriptType scriptType, PdfStringFormat pdfStringFormat)
	{
		return GetFallbackPdfFont(pdfFont, systemFont, inputText, scriptType, pdfStringFormat, FallbackFonts, FontStreams, EmbedFonts, EmbedCompleteFonts);
	}

	internal PdfFont GetFallbackPdfFont(PdfFont pdfFont, DocGen.Drawing.Font systemFont, string inputText, FontScriptType scriptType, PdfStringFormat pdfStringFormat, List<FallbackFont> fallbackFonts, Dictionary<string, Stream> fontStreams, bool isEmbedFonts, bool isEmbedCompleteFonts)
	{
		if (!string.IsNullOrEmpty(inputText) && !IsContainFont(pdfFont, inputText, pdfStringFormat))
		{
			for (int i = 0; i < fallbackFonts.Count; i++)
			{
				FallbackFont fallbackFont = fallbackFonts[i];
				if (string.IsNullOrEmpty(fallbackFont.FontNames) || !fallbackFont.IsWithInRange(inputText))
				{
					continue;
				}
				string[] array = fallbackFont.FontNames.Split(',');
				for (int j = 0; j < array.Length; j++)
				{
					string text = array[j].Trim();
					DocGen.Drawing.Font font = new DocGen.Drawing.Font(text, systemFont.Size, systemFont.Style);
					string key = text.ToLower() + ";" + font.Style.ToString() + ";" + font.Size + ";" + IsUnicode(inputText);
					PdfFont pdfFont2 = null;
					if (PdfFontCollection.ContainsKey(key))
					{
						pdfFont2 = PdfFontCollection[key];
					}
					else
					{
						pdfFont2 = ((!isEmbedCompleteFonts) ? new PdfTrueTypeFont(WordDocument.RenderHelper.GetFontStream(font, scriptType), font.Size, isEmbedFonts || IsUnicode(inputText), string.Empty, GetFontStyle(font.Style)) : new PdfTrueTypeFont(WordDocument.RenderHelper.GetFontStream(font, scriptType), font.Size, string.Empty, isEnableEmbedding: true, GetFontStyle(font.Style)));
						if (text != pdfFont2.Name)
						{
							PdfFont fallBackFontFromSubstitutionOrEmbedded = GetFallBackFontFromSubstitutionOrEmbedded(inputText, text, font, fontStreams);
							if (fallBackFontFromSubstitutionOrEmbedded != null)
							{
								pdfFont2 = fallBackFontFromSubstitutionOrEmbedded;
							}
						}
						pdfFont2.Ascent = GetAscent(font, scriptType);
						PdfFontCollection.Add(key, pdfFont2);
					}
					if (IsContainFont(pdfFont2, inputText, pdfStringFormat))
					{
						return pdfFont2;
					}
				}
			}
		}
		return pdfFont;
	}

	private PdfFont GetFallBackFontFromSubstitutionOrEmbedded(string inputText, string fallbackFontName, DocGen.Drawing.Font tempFont, Dictionary<string, Stream> fontStreams)
	{
		if (fontStreams != null && fontStreams.Count > 0)
		{
			Stream stream = null;
			string text = fallbackFontName + "_" + tempFont.Style.ToString().Replace(", ", "");
			if (fontStreams.ContainsKey(text))
			{
				stream = fontStreams[text];
			}
			else if (fontStreams.ContainsKey(text.ToLower()))
			{
				stream = fontStreams[text.ToLower()];
			}
			else if (fontStreams.ContainsKey(fallbackFontName))
			{
				stream = fontStreams[fallbackFontName];
			}
			if (stream != null && stream.CanRead)
			{
				stream.Position = 0L;
				return new PdfTrueTypeFont(stream, tempFont.Size, isEnableEmbedding: true, GetFontStyle(tempFont.Style));
			}
		}
		return null;
	}

	internal bool IsContainFont(PdfFont pdfFont, string inputText, PdfStringFormat pdfStringFormat)
	{
		pdfFont.MeasureString(inputText, pdfStringFormat);
		if (pdfFont is PdfTrueTypeFont)
		{
			return (pdfFont as PdfTrueTypeFont).IsContainsFont;
		}
		return false;
	}

	private PdfFont GetRegularStyleFontToRender(DocGen.Drawing.Font font, FontScriptType scriptType)
	{
		string pdfFontCollectionKey = GetPdfFontCollectionKey(font, isUnicode: true);
		if (PdfFontCollection.ContainsKey(pdfFontCollectionKey))
		{
			PdfFontCollection.Remove(pdfFontCollectionKey);
		}
		DocGen.Drawing.Font font2 = new DocGen.Drawing.Font(font.Name, font.Size);
		PdfFont pdfFont = new PdfTrueTypeFont(WordDocument.RenderHelper.GetFontStream(font2, scriptType), font2.Size, isUnicode: true, "", GetFontStyle(font.Style));
		pdfFont.Ascent = GetAscent(font2, scriptType);
		PdfFontCollection.Add(pdfFontCollectionKey, pdfFont);
		return pdfFont;
	}

	private float GetFactor(string fontName)
	{
		float num = 0f;
		return fontName switch
		{
			"MS Gothic" => 0.3154f, 
			"Malgun Gothic" => 0.3994f, 
			"SimSun" => 0.16f, 
			_ => 0f, 
		};
	}

	public float GetSubSuperScriptFontSize(DocGen.Drawing.Font font)
	{
		return (float)Math.Round(font.Size / 1.54f * 2f, MidpointRounding.AwayFromZero) / 2f;
	}

	public float GetExceededLineHeightForArialUnicodeMSFont(DocGen.Drawing.Font fontExt, bool isAscent, FontScriptType scriptType)
	{
		IFontExtension fontExtension = WordDocument.RenderHelper.GetFontExtension(fontExt.Name, fontExt.Size, fontExt.Style, GraphicsUnit.Point, scriptType);
		float size = fontExtension.Size;
		float emHeight = fontExtension.GetEmHeight(fontExt.Style);
		float num = fontExtension.GetCellAscent(fontExt.Style) * emHeight / size;
		float num2 = fontExtension.GetCellDescent(fontExt.Style) * emHeight / size;
		float num3 = fontExtension.GetLineSpacing(fontExt.Style) * emHeight / size;
		float num4 = size / emHeight;
		int num5 = (int)(num + num2 - emHeight);
		int num6 = num5 / 2;
		int num7 = num5 - num6;
		int num8 = (int)(0.3 * (double)(num + num2));
		num8 -= num5;
		num5 = ((num8 > 0) ? num8 : 0);
		num = (int)(num + (float)num6);
		num2 = (int)(num2 + (float)num7);
		num3 = (int)(num + num2 + (float)num5);
		float num9 = num3 - (num + num2);
		if (isAscent)
		{
			num3 -= num2 + num9;
		}
		return num4 * (float)(int)num3;
	}

	public SizeF MeasureString(string text, DocGen.Drawing.Font font, DocGen.Drawing.Font defaultFont, StringFormat format, WCharacterFormat charFormat, FontScriptType scriptType)
	{
		float num = 0f;
		if (charFormat != null)
		{
			num = charFormat.CharacterSpacing;
		}
		if (text == null || text.Length == 0)
		{
			return SizeF.Empty;
		}
		text = text.Replace('\u001e'.ToString(), "-");
		text = text.Replace('\u00ad'.ToString(), "-");
		text = text.Replace('\u200d'.ToString(), "");
		if (text.Length == 0)
		{
			return SizeF.Empty;
		}
		if (format == null)
		{
			format = StringFormt;
		}
		string name = font.Name;
		if (charFormat != null && name == "Arial Unicode MS")
		{
			FontStyle style = font.Style;
			style &= (FontStyle)(-2);
			font = charFormat.Document.FontSettings.GetFont(name, font.Size, style, scriptType);
		}
		if (charFormat != null && charFormat.Document.FontSettings.FallbackFonts.Count > 0)
		{
			font = WordDocument.RenderHelper.GetFallbackFont(font, text, scriptType, charFormat.Document.FontSettings.FallbackFonts, charFormat.Document.FontSettings.FontStreams);
		}
		SizeF size = MeasureUnicodeString(text, font, defaultFont, format, charFormat, scriptType);
		DocGen.Drawing.Font font2 = ((charFormat != null && charFormat.SubSuperScript != 0) ? charFormat.Document.FontSettings.GetFont(name, GetSubSuperScriptFontSize(font), font.Style, scriptType) : font);
		if (charFormat != null && charFormat.SmallCaps)
		{
			MeasureSmallCapString(text, ref size, font2, format, charFormat, scriptType);
		}
		if (num != 0f)
		{
			size.Width += (float)text.Length * num;
		}
		if ((charFormat == null || (!charFormat.ComplexScript && text.Trim(' ') != "")) && name == "Arial Unicode MS")
		{
			size.Height = GetExceededLineHeightForArialUnicodeMSFont(font, isAscent: false, scriptType);
		}
		return size;
	}

	private void MeasureSmallCapString(string text, ref SizeF size, DocGen.Drawing.Font font, StringFormat format, WCharacterFormat charFormat, FontScriptType scriptType)
	{
		float num = 0f;
		DocGen.Drawing.Font font2 = charFormat.Document.FontSettings.GetFont(font.Name, (float)(((double)font.Size * 0.8 > 3.0) ? ((double)font.Size * 0.8) : 2.0), font.Style, scriptType);
		string text2 = string.Empty;
		string text3 = string.Empty;
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			if (char.IsUpper(c) || (!char.IsLetter(c) && !c.Equals(ControlChar.SpaceChar)))
			{
				text2 += c;
			}
			else
			{
				text3 += c;
			}
		}
		if (charFormat.IsKernFont)
		{
			num = Graphics.MeasureString(text2, font, new PointF(0f, 0f), format, font.Style, scriptType).Width;
			num += Graphics.MeasureString(text3.ToUpper(), font2, new PointF(0f, 0f), format, font.Style, scriptType).Width;
		}
		else
		{
			num = GraphicsBmp.MeasureString(text2, font, new PointF(0f, 0f), format, font.Style, scriptType).Width;
			num += GraphicsBmp.MeasureString(text3.ToUpper(), font2, new PointF(0f, 0f), format, font.Style, scriptType).Width;
		}
		size.Width = num;
	}

	private SizeF MeasureUnicodeString(string text, DocGen.Drawing.Font font, DocGen.Drawing.Font defaultFont, StringFormat format, WCharacterFormat charFormat, FontScriptType scriptType)
	{
		char[] array = text.ToCharArray();
		string text2 = null;
		string text3 = null;
		DocGen.Drawing.Font font2 = font;
		if (font.Name == "Arial" || font.Name == "Times New Roman")
		{
			font = charFormat.Document.FontSettings.GetFont("Arial Unicode MS", font.Size, font.Style, scriptType);
		}
		if (font.Name == "Arial Narrow" && font.Style == FontStyle.Bold)
		{
			text = text.Replace(ControlChar.NonBreakingSpaceChar, ControlChar.SpaceChar);
		}
		float num = 0f;
		for (int i = 0; i < array.Length; i++)
		{
			if (!IsUnicodeText(array[i].ToString()))
			{
				text2 += array[i];
				if (text3 != null)
				{
					DocGen.Drawing.Font font3 = ((charFormat != null && charFormat.SubSuperScript != 0) ? charFormat.Document.FontSettings.GetFont(font.Name, GetSubSuperScriptFontSize(font), font.Style, scriptType) : font);
					num = ((charFormat == null || !charFormat.IsKernFont) ? (num + GraphicsBmp.MeasureString(text3, font3, new PointF(0f, 0f), format, font.Style, scriptType).Width) : (num + Graphics.MeasureString(text3, font3, new PointF(0f, 0f), format, font.Style, scriptType).Width));
					text3 = null;
				}
			}
			else
			{
				text3 += array[i];
				if (text2 != null)
				{
					DocGen.Drawing.Font font4 = ((charFormat != null && charFormat.SubSuperScript != 0) ? charFormat.Document.FontSettings.GetFont(defaultFont.Name, GetSubSuperScriptFontSize(defaultFont), defaultFont.Style, scriptType) : defaultFont);
					num = ((charFormat == null || !charFormat.IsKernFont) ? (num + GraphicsBmp.MeasureString(text2, font4, new PointF(0f, 0f), format, font.Style, scriptType).Width) : (num + Graphics.MeasureString(text2, font4, new PointF(0f, 0f), format, font.Style, scriptType).Width));
					text2 = null;
				}
			}
		}
		if (text3 != null)
		{
			DocGen.Drawing.Font font5 = ((charFormat != null && charFormat.SubSuperScript != 0) ? charFormat.Document.FontSettings.GetFont(font.Name, GetSubSuperScriptFontSize(font), font.Style, scriptType) : font);
			num = ((charFormat == null || !charFormat.IsKernFont) ? (num + GraphicsBmp.MeasureString(text3, font5, new PointF(0f, 0f), format, font.Style, scriptType).Width) : (num + Graphics.MeasureString(text3, font5, new PointF(0f, 0f), format, font.Style, scriptType).Width));
		}
		else if (text2 != null)
		{
			DocGen.Drawing.Font font6 = ((charFormat != null && charFormat.SubSuperScript != 0) ? charFormat.Document.FontSettings.GetFont(defaultFont.Name, GetSubSuperScriptFontSize(defaultFont), defaultFont.Style, scriptType) : defaultFont);
			num = ((charFormat == null || !charFormat.IsKernFont) ? (num + GraphicsBmp.MeasureString(text2, font6, new PointF(0f, 0f), format, font.Style, scriptType).Width) : (num + Graphics.MeasureString(text2, font6, new PointF(0f, 0f), format, font.Style, scriptType).Width));
		}
		if (font.Name == string.Empty)
		{
			font = font2;
		}
		SizeF result = Graphics.MeasureString(text[0].ToString(), font, new PointF(0f, 0f), format, font.Style, scriptType);
		result.Width = num;
		return result;
	}

	public SizeF MeasureTextRange(WTextRange txtRange, string text)
	{
		WParagraph ownerParagraph = txtRange.OwnerParagraph;
		WCharacterFormat wCharacterFormat = txtRange.CharacterFormat;
		if (txtRange.Text.Trim(' ') == string.Empty && ownerParagraph != null && ownerParagraph.Text == txtRange.Text)
		{
			wCharacterFormat = ownerParagraph.BreakCharacterFormat;
		}
		DocGen.Drawing.Font font = ((txtRange.m_layoutInfo != null) ? txtRange.m_layoutInfo.Font.GetFont(txtRange.Document, txtRange.ScriptType) : GetFont(txtRange, wCharacterFormat, text));
		StringFormat stringFormat = new StringFormat(StringFormt);
		if (wCharacterFormat.Bidi)
		{
			stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
		}
		else
		{
			stringFormat.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;
		}
		if (IsTOC(txtRange) && txtRange.GetOwnerParagraphValue().ParaStyle != null)
		{
			stringFormat = new StringFormat(StringFormt);
			if (txtRange.OwnerParagraph.ParaStyle.CharacterFormat.Bidi)
			{
				stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
			}
			else
			{
				stringFormat.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;
			}
		}
		if (text != null && wCharacterFormat.AllCaps)
		{
			text = text.ToUpper();
		}
		DocGen.Drawing.Font defaultFont = GetDefaultFont(txtRange.ScriptType, font, wCharacterFormat);
		if (font.Name != defaultFont.Name && IsUnicodeText(text))
		{
			return MeasureString(text, font, defaultFont, stringFormat, wCharacterFormat, txtRange.ScriptType);
		}
		return MeasureString(text, font, stringFormat, wCharacterFormat, isMeasureFromTabList: false, txtRange.ScriptType);
	}

	public float GetAscent(DocGen.Drawing.Font font, FontScriptType scriptType)
	{
		return (float)FontMetric.Ascent(font, scriptType);
	}

	public float GetDescent(DocGen.Drawing.Font font, FontScriptType scriptType)
	{
		return (float)FontMetric.Descent(font, scriptType);
	}

	public void IntializeGraphics(WPageSetup pageSetup)
	{
		int width = (int)UnitsConvertor.Instance.ConvertToPixels(pageSetup.PageSize.Width, PrintUnits.Point);
		int height = (int)UnitsConvertor.Instance.ConvertToPixels(pageSetup.PageSize.Height, PrintUnits.Point);
		DocGen.Drawing.SkiaSharpHelper.Image image = new DocGen.Drawing.SkiaSharpHelper.Image(width, height);
		Graphics = new Graphics(image);
		Graphics.PageUnit = GraphicsUnit.Point;
	}

	public void DisposeGraphics()
	{
		Graphics.Dispose();
		Graphics = null;
	}

	public void FillRectangle(Color color, Rectangle rectangle)
	{
		Graphics.FillRectangle(color, rectangle);
	}

	public void MatrixTranslate(Matrix matrix, float x, float y, MatrixOrder matrixOrder)
	{
		WordDocument.RenderHelper.TranslateMatrix(matrix, x, y, matrixOrder);
	}

	public void MatrixMultiply(Matrix matrix, Matrix target, MatrixOrder matrixOrder)
	{
		WordDocument.RenderHelper.MultiplyMatrix(matrix, target, matrixOrder);
	}

	private void MatrixRotate(Matrix matrix, float angle, PointF point, MatrixOrder matrixOrder)
	{
		WordDocument.RenderHelper.RotateMatrix(matrix, angle, point, matrixOrder);
	}

	private DocGen.Drawing.SkiaSharpHelper.GraphicsPath CreateGraphicsPath()
	{
		return new DocGen.Drawing.SkiaSharpHelper.GraphicsPath();
	}

	private DocGen.Drawing.SkiaSharpHelper.Bitmap CreateBitmap(int width, int height)
	{
		return new DocGen.Drawing.SkiaSharpHelper.Bitmap(width, height);
	}

	private Graphics GetGraphicsFromImage(DocGen.Drawing.SkiaSharpHelper.Bitmap bmp)
	{
		return Graphics.FromImage(bmp);
	}

	private DocGen.Drawing.SkiaSharpHelper.Bitmap CreateBitmap()
	{
		return new DocGen.Drawing.SkiaSharpHelper.Bitmap();
	}

	private DocGen.Drawing.SkiaSharpHelper.Brush CreateBrush(Color color)
	{
		return WordDocument.RenderHelper.GetSolidBrush(color) as DocGen.Drawing.SkiaSharpHelper.Brush;
	}

	private Pen CreatePen(Color color, float width)
	{
		return new Pen(color, width);
	}

	private void DrawArrowHead(ChildShape shape, PdfPen pen, RectangleF bounds, ref bool isArrowHeadExist, ref PdfPath path, PointF[] linePoints)
	{
		isArrowHeadExist = false;
		PointF endPoint = new PointF(0f, 0f);
		switch (shape.LineFormat.EndArrowheadStyle)
		{
		case ArrowheadStyle.ArrowheadOpen:
			DrawOpenEndArrowHead(shape, pen, bounds, linePoints, ref endPoint, ref isArrowHeadExist, ref path);
			break;
		case ArrowheadStyle.ArrowheadTriangle:
			DrawCloseEndArrowHead(shape, pen, bounds, linePoints, ref endPoint, ref isArrowHeadExist, ref path);
			break;
		case ArrowheadStyle.ArrowheadStealth:
			DrawStealthEndArrowHead(shape, pen, bounds, linePoints, ref endPoint, ref isArrowHeadExist, ref path);
			break;
		}
		switch (shape.LineFormat.BeginArrowheadStyle)
		{
		case ArrowheadStyle.ArrowheadOpen:
			DrawOpenBeginArrowHead(shape, pen, bounds, linePoints, ref endPoint, ref isArrowHeadExist, ref path);
			break;
		case ArrowheadStyle.ArrowheadTriangle:
			DrawCloseBeginArrowHead(shape, pen, bounds, linePoints, ref endPoint, ref isArrowHeadExist, ref path);
			break;
		case ArrowheadStyle.ArrowheadStealth:
			DrawStealthBeginArrowHead(shape, pen, bounds, linePoints, ref endPoint, ref isArrowHeadExist, ref path);
			break;
		case ArrowheadStyle.ArrowheadDiamond:
		case ArrowheadStyle.ArrowheadOval:
			break;
		}
	}

	private void DrawArrowHead(Shape shape, PdfPen pen, RectangleF bounds, ref bool isArrowHeadExist, ref PdfPath path, PointF[] linePoints)
	{
		isArrowHeadExist = false;
		PointF endPoint = new PointF(0f, 0f);
		switch (shape.LineFormat.EndArrowheadStyle)
		{
		case ArrowheadStyle.ArrowheadOpen:
			DrawOpenEndArrowHead(shape, pen, bounds, linePoints, ref endPoint, ref isArrowHeadExist, ref path);
			break;
		case ArrowheadStyle.ArrowheadTriangle:
			DrawCloseEndArrowHead(shape, pen, bounds, linePoints, ref endPoint, ref isArrowHeadExist, ref path);
			break;
		case ArrowheadStyle.ArrowheadStealth:
			DrawStealthEndArrowHead(shape, pen, bounds, linePoints, ref endPoint, ref isArrowHeadExist, ref path);
			break;
		}
		switch (shape.LineFormat.BeginArrowheadStyle)
		{
		case ArrowheadStyle.ArrowheadOpen:
			DrawOpenBeginArrowHead(shape, pen, bounds, linePoints, ref endPoint, ref isArrowHeadExist, ref path);
			break;
		case ArrowheadStyle.ArrowheadTriangle:
			DrawCloseBeginArrowHead(shape, pen, bounds, linePoints, ref endPoint, ref isArrowHeadExist, ref path);
			break;
		case ArrowheadStyle.ArrowheadStealth:
			DrawStealthBeginArrowHead(shape, pen, bounds, linePoints, ref endPoint, ref isArrowHeadExist, ref path);
			break;
		case ArrowheadStyle.ArrowheadDiamond:
		case ArrowheadStyle.ArrowheadOval:
			break;
		}
	}

	private void DrawOpenEndArrowHead(Shape shape, PdfPen pen, RectangleF bounds, PointF[] linePoints, ref PointF endPoint, ref bool isArrowHeadExist, ref PdfPath path)
	{
		PointF[] array = FindArrowHeadPoints(shape, pen, bounds, linePoints, isFromOpenArrow: true, isFromBeginArrow: false);
		if (shape.LineFormat.BeginArrowheadStyle != ArrowheadStyle.ArrowheadTriangle && shape.LineFormat.BeginArrowheadStyle != ArrowheadStyle.ArrowheadOpen)
		{
			path.AddLine(linePoints[0].X, linePoints[0].Y, array[0].X, array[0].Y);
			path.CloseFigure();
		}
		else
		{
			endPoint = array[0];
		}
		AddOpenArrowHeadPoints(array, ref path);
		isArrowHeadExist = true;
	}

	private void DrawOpenEndArrowHead(ChildShape shape, PdfPen pen, RectangleF bounds, PointF[] linePoints, ref PointF endPoint, ref bool isArrowHeadExist, ref PdfPath path)
	{
		PointF[] array = FindArrowHeadPoints(shape, pen, bounds, linePoints, isFromOpenArrow: true, isFromBeginArrow: false);
		if (shape.LineFormat.BeginArrowheadStyle != ArrowheadStyle.ArrowheadTriangle && shape.LineFormat.BeginArrowheadStyle != ArrowheadStyle.ArrowheadOpen)
		{
			path.AddLine(linePoints[0].X, linePoints[0].Y, array[0].X, array[0].Y);
			path.CloseFigure();
		}
		else
		{
			endPoint = array[0];
		}
		AddOpenArrowHeadPoints(array, ref path);
		isArrowHeadExist = true;
	}

	private void DrawCloseEndArrowHead(Shape shape, PdfPen pen, RectangleF bounds, PointF[] linePoints, ref PointF endPoint, ref bool isArrowHeadExist, ref PdfPath path)
	{
		PointF[] array = FindArrowHeadPoints(shape, pen, bounds, linePoints, isFromOpenArrow: false, isFromBeginArrow: false);
		if (shape.LineFormat.BeginArrowheadStyle != ArrowheadStyle.ArrowheadTriangle && shape.LineFormat.BeginArrowheadStyle != ArrowheadStyle.ArrowheadOpen)
		{
			path.AddLine(linePoints[0].X, linePoints[0].Y, array[0].X, array[0].Y);
			path.CloseFigure();
		}
		else
		{
			endPoint = array[0];
		}
		AddCloseArrowHeadPoints(array, pen);
		isArrowHeadExist = true;
	}

	private void DrawStealthEndArrowHead(Shape shape, PdfPen pen, RectangleF bounds, PointF[] linePoints, ref PointF endPoint, ref bool isArrowHeadExist, ref PdfPath path)
	{
		PointF[] array = FindArrowHeadPoints(shape, pen, bounds, linePoints, isFromOpenArrow: false, isFromBeginArrow: false);
		if (shape.LineFormat.BeginArrowheadStyle != ArrowheadStyle.ArrowheadTriangle && shape.LineFormat.BeginArrowheadStyle != ArrowheadStyle.ArrowheadOpen)
		{
			path.AddLine(linePoints[0].X, linePoints[0].Y, array[0].X, array[0].Y);
			path.CloseFigure();
		}
		else
		{
			endPoint = array[0];
		}
		AddStealthArrowHeadPoints(array, pen);
		isArrowHeadExist = true;
	}

	private void DrawCloseEndArrowHead(ChildShape shape, PdfPen pen, RectangleF bounds, PointF[] linePoints, ref PointF endPoint, ref bool isArrowHeadExist, ref PdfPath path)
	{
		PointF[] array = FindArrowHeadPoints(shape, pen, bounds, linePoints, isFromOpenArrow: false, isFromBeginArrow: false);
		if (shape.LineFormat.BeginArrowheadStyle != ArrowheadStyle.ArrowheadTriangle && shape.LineFormat.BeginArrowheadStyle != ArrowheadStyle.ArrowheadOpen)
		{
			path.AddLine(linePoints[0].X, linePoints[0].Y, array[0].X, array[0].Y);
			path.CloseFigure();
		}
		else
		{
			endPoint = array[0];
		}
		AddCloseArrowHeadPoints(array, pen);
		isArrowHeadExist = true;
	}

	private void DrawStealthEndArrowHead(ChildShape shape, PdfPen pen, RectangleF bounds, PointF[] linePoints, ref PointF endPoint, ref bool isArrowHeadExist, ref PdfPath path)
	{
		PointF[] array = FindArrowHeadPoints(shape, pen, bounds, linePoints, isFromOpenArrow: false, isFromBeginArrow: false);
		if (shape.LineFormat.BeginArrowheadStyle != ArrowheadStyle.ArrowheadTriangle && shape.LineFormat.BeginArrowheadStyle != ArrowheadStyle.ArrowheadOpen)
		{
			path.AddLine(linePoints[0].X, linePoints[0].Y, array[0].X, array[0].Y);
			path.CloseFigure();
		}
		else
		{
			endPoint = array[0];
		}
		AddStealthArrowHeadPoints(array, pen);
		isArrowHeadExist = true;
	}

	private void DrawOpenBeginArrowHead(Shape shape, PdfPen pen, RectangleF bounds, PointF[] linePoints, ref PointF endPoint, ref bool isArrowHeadExist, ref PdfPath path)
	{
		PointF[] array = FindArrowHeadPoints(shape, pen, bounds, linePoints, isFromOpenArrow: true, isFromBeginArrow: true);
		path.StartFigure();
		if (endPoint.X == 0f && endPoint.Y == 0f)
		{
			path.AddLine(linePoints[1].X, linePoints[1].Y, array[0].X, array[0].Y);
		}
		else
		{
			path.AddLine(endPoint.X, endPoint.Y, array[0].X, array[0].Y);
		}
		path.CloseFigure();
		AddOpenArrowHeadPoints(array, ref path);
		isArrowHeadExist = true;
	}

	private void DrawOpenBeginArrowHead(ChildShape shape, PdfPen pen, RectangleF bounds, PointF[] linePoints, ref PointF endPoint, ref bool isArrowHeadExist, ref PdfPath path)
	{
		PointF[] array = FindArrowHeadPoints(shape, pen, bounds, linePoints, isFromOpenArrow: true, isFromBeginArrow: true);
		path.StartFigure();
		if (endPoint.X == 0f && endPoint.Y == 0f)
		{
			path.AddLine(linePoints[1].X, linePoints[1].Y, array[0].X, array[0].Y);
		}
		else
		{
			path.AddLine(endPoint.X, endPoint.Y, array[0].X, array[0].Y);
		}
		path.CloseFigure();
		AddOpenArrowHeadPoints(array, ref path);
		isArrowHeadExist = true;
	}

	private void DrawCloseBeginArrowHead(Shape shape, PdfPen pen, RectangleF bounds, PointF[] linePoints, ref PointF endPoint, ref bool isArrowHeadExist, ref PdfPath path)
	{
		PointF[] array = FindArrowHeadPoints(shape, pen, bounds, linePoints, isFromOpenArrow: false, isFromBeginArrow: true);
		path.StartFigure();
		if (endPoint.X == 0f && endPoint.Y == 0f)
		{
			path.AddLine(linePoints[1].X, linePoints[1].Y, array[0].X, array[0].Y);
		}
		else
		{
			path.AddLine(endPoint.X, endPoint.Y, array[0].X, array[0].Y);
		}
		path.CloseFigure();
		AddCloseArrowHeadPoints(array, pen);
		isArrowHeadExist = true;
	}

	private void DrawCloseBeginArrowHead(ChildShape shape, PdfPen pen, RectangleF bounds, PointF[] linePoints, ref PointF endPoint, ref bool isArrowHeadExist, ref PdfPath path)
	{
		PointF[] array = FindArrowHeadPoints(shape, pen, bounds, linePoints, isFromOpenArrow: false, isFromBeginArrow: true);
		path.StartFigure();
		if (endPoint.X == 0f && endPoint.Y == 0f)
		{
			path.AddLine(linePoints[1].X, linePoints[1].Y, array[0].X, array[0].Y);
		}
		else
		{
			path.AddLine(endPoint.X, endPoint.Y, array[0].X, array[0].Y);
		}
		path.CloseFigure();
		AddCloseArrowHeadPoints(array, pen);
		isArrowHeadExist = true;
	}

	private void DrawStealthBeginArrowHead(Shape shape, PdfPen pen, RectangleF bounds, PointF[] linePoints, ref PointF endPoint, ref bool isArrowHeadExist, ref PdfPath path)
	{
		PointF[] array = FindArrowHeadPoints(shape, pen, bounds, linePoints, isFromOpenArrow: false, isFromBeginArrow: true);
		path.StartFigure();
		if (endPoint.X == 0f && endPoint.Y == 0f)
		{
			path.AddLine(linePoints[1].X, linePoints[1].Y, array[0].X, array[0].Y);
		}
		else
		{
			path.AddLine(endPoint.X, endPoint.Y, array[0].X, array[0].Y);
		}
		path.CloseFigure();
		AddStealthArrowHeadPoints(array, pen);
		isArrowHeadExist = true;
	}

	private void DrawStealthBeginArrowHead(ChildShape shape, PdfPen pen, RectangleF bounds, PointF[] linePoints, ref PointF endPoint, ref bool isArrowHeadExist, ref PdfPath path)
	{
		PointF[] array = FindArrowHeadPoints(shape, pen, bounds, linePoints, isFromOpenArrow: false, isFromBeginArrow: true);
		path.StartFigure();
		if (endPoint.X == 0f && endPoint.Y == 0f)
		{
			path.AddLine(linePoints[1].X, linePoints[1].Y, array[0].X, array[0].Y);
		}
		else
		{
			path.AddLine(endPoint.X, endPoint.Y, array[0].X, array[0].Y);
		}
		path.CloseFigure();
		AddStealthArrowHeadPoints(array, pen);
		isArrowHeadExist = true;
	}

	private void AddCloseArrowHeadPoints(PointF[] points, PdfPen pen)
	{
		PdfPath pdfPath = new PdfPath();
		PointF[] points2 = new PointF[3]
		{
			points[1],
			points[2],
			points[3]
		};
		pdfPath.AddPolygon(points2);
		pdfPath.CloseFigure();
		Graphics.FillPolygon(CreateBrush(pen.Color), points2);
		PDFGraphics.DrawPath(pen, pdfPath);
	}

	private void AddStealthArrowHeadPoints(PointF[] points, PdfPen pen)
	{
		PdfPath pdfPath = new PdfPath();
		PointF[] array = new PointF[4]
		{
			points[1],
			points[2],
			points[3],
			default(PointF)
		};
		float x = (array[0].X + array[1].X + array[2].X) / 3f;
		float y = (array[0].Y + array[1].Y + array[2].Y) / 3f;
		array[3] = new PointF(x, y);
		pdfPath.AddPolygon(array);
		pdfPath.CloseFigure();
		Graphics.FillPolygon(CreateBrush(pen.Color), array);
		PDFGraphics.DrawPath(pen, pdfPath);
	}

	private void AddOpenArrowHeadPoints(PointF[] points, ref PdfPath path)
	{
		path.AddLine(points[1], points[2]);
		path.AddLine(points[2], points[3]);
	}

	private void GetOpenArrowDefaultValues(LineFormat lineFormat, float lineWidth, ref float arrowLength, ref float arrowAngle, ref float adjustValue, bool isFromBeginArrow)
	{
		LineEndWidth lineEndWidth = lineFormat.EndArrowheadWidth;
		if (isFromBeginArrow)
		{
			lineEndWidth = lineFormat.BeginArrowheadWidth;
		}
		switch (lineEndWidth)
		{
		case LineEndWidth.NarrowArrow:
		{
			LineEndLength arrowHeadLength = lineFormat.EndArrowheadLength;
			arrowHeadLength = GetArrowHeadLength(lineFormat, isFromBeginArrow);
			GetOpenNarrowArrowDefaultValues(arrowHeadLength, lineWidth, ref arrowLength, ref arrowAngle, ref adjustValue);
			break;
		}
		case LineEndWidth.MediumWidthArrow:
		{
			LineEndLength arrowHeadLength = GetArrowHeadLength(lineFormat, isFromBeginArrow);
			GetOpenMediumArrowDefaultValues(arrowHeadLength, lineWidth, ref arrowLength, ref arrowAngle, ref adjustValue);
			break;
		}
		case LineEndWidth.WideArrow:
		{
			LineEndLength arrowHeadLength = GetArrowHeadLength(lineFormat, isFromBeginArrow);
			GetOpenWideArrowDefaultValues(arrowHeadLength, lineWidth, ref arrowLength, ref arrowAngle, ref adjustValue);
			break;
		}
		}
	}

	private void GetCloseArrowDefaultValues(LineFormat lineFormat, float lineWidth, ref float arrowLength, ref float arrowAngle, ref float adjustValue, bool isFromBeginArrow)
	{
		LineEndWidth lineEndWidth = lineFormat.EndArrowheadWidth;
		if (isFromBeginArrow)
		{
			lineEndWidth = lineFormat.BeginArrowheadWidth;
		}
		switch (lineEndWidth)
		{
		case LineEndWidth.NarrowArrow:
		{
			LineEndLength arrowHeadLength = lineFormat.EndArrowheadLength;
			arrowHeadLength = GetArrowHeadLength(lineFormat, isFromBeginArrow);
			GetCloseNarrowArrowDefaultValues(arrowHeadLength, lineWidth, ref arrowLength, ref arrowAngle, ref adjustValue);
			break;
		}
		case LineEndWidth.MediumWidthArrow:
		{
			LineEndLength arrowHeadLength = GetArrowHeadLength(lineFormat, isFromBeginArrow);
			GetCloseMediumArrowDefaultValues(arrowHeadLength, lineWidth, ref arrowLength, ref arrowAngle, ref adjustValue);
			break;
		}
		case LineEndWidth.WideArrow:
		{
			LineEndLength arrowHeadLength = GetArrowHeadLength(lineFormat, isFromBeginArrow);
			GetCloseWideArrowDefaultValues(arrowHeadLength, lineWidth, ref arrowLength, ref arrowAngle, ref adjustValue);
			break;
		}
		}
	}

	private LineEndLength GetArrowHeadLength(LineFormat lineFormat, bool isFromBeginArrow)
	{
		LineEndLength result = lineFormat.EndArrowheadLength;
		if (isFromBeginArrow)
		{
			result = lineFormat.BeginArrowheadLength;
		}
		return result;
	}

	private void GetCloseNarrowArrowDefaultValues(LineEndLength arrowHeadLength, float lineWidth, ref float arrowLength, ref float arrowAngle, ref float adjustValue)
	{
		switch (arrowHeadLength)
		{
		case LineEndLength.ShortArrow:
			if (lineWidth <= 1f)
			{
				arrowLength = 2.7f;
			}
			else
			{
				arrowLength = lineWidth * 0.37f;
			}
			arrowAngle = 26f;
			adjustValue = lineWidth * 1.15f;
			break;
		case LineEndLength.MediumLenArrow:
			if (lineWidth <= 1f)
			{
				arrowLength = 4.2f;
			}
			else
			{
				arrowLength = (float)Math.Round(lineWidth * 0.97f);
			}
			arrowAngle = 18.5f;
			adjustValue = lineWidth * 1.59f;
			break;
		case LineEndLength.LongArrow:
			if (lineWidth <= 1f)
			{
				arrowLength = 9f;
			}
			else
			{
				arrowLength = (float)Math.Round(lineWidth * 2.05f);
			}
			arrowAngle = 11.3f;
			adjustValue = lineWidth * 2.52f;
			break;
		}
	}

	private void GetCloseMediumArrowDefaultValues(LineEndLength arrowHeadLength, float lineWidth, ref float arrowLength, ref float arrowAngle, ref float adjustValue)
	{
		switch (arrowHeadLength)
		{
		case LineEndLength.ShortArrow:
			if (lineWidth <= 1f)
			{
				arrowLength = 3.5f;
			}
			else
			{
				arrowLength = lineWidth * 0.845f;
			}
			arrowAngle = 37f;
			adjustValue = lineWidth * 0.83f;
			break;
		case LineEndLength.MediumLenArrow:
			if (lineWidth <= 1f)
			{
				arrowLength = 5f;
			}
			else
			{
				arrowLength = lineWidth * 1.5f;
			}
			arrowAngle = 26.5f;
			adjustValue = lineWidth * 1.15f;
			break;
		case LineEndLength.LongArrow:
			if (lineWidth <= 1f)
			{
				arrowLength = 8f;
			}
			else
			{
				arrowLength = (float)Math.Round(lineWidth * 2.87f);
			}
			arrowAngle = 16.65f;
			adjustValue = lineWidth * 1.75f;
			break;
		}
	}

	private void GetCloseWideArrowDefaultValues(LineEndLength arrowHeadLength, float lineWidth, ref float arrowLength, ref float arrowAngle, ref float adjustValue)
	{
		switch (arrowHeadLength)
		{
		case LineEndLength.ShortArrow:
			if (lineWidth <= 1f)
			{
				arrowLength = 4.5f;
			}
			else
			{
				arrowLength = lineWidth * 1.36f;
			}
			arrowAngle = 51.5f;
			adjustValue = lineWidth * 0.65f;
			break;
		case LineEndLength.MediumLenArrow:
			if (lineWidth <= 1f)
			{
				arrowLength = 6.2f;
			}
			else
			{
				arrowLength = lineWidth * 2.24f;
			}
			arrowAngle = 39.7f;
			adjustValue = lineWidth * 0.78f;
			break;
		case LineEndLength.LongArrow:
			if (lineWidth <= 1f)
			{
				arrowLength = 9.45f;
			}
			else
			{
				arrowLength = (float)Math.Round(lineWidth * 3.78f);
			}
			arrowAngle = 26.55f;
			adjustValue = lineWidth * 1.13f;
			break;
		}
	}

	private void GetOpenNarrowArrowDefaultValues(LineEndLength arrowHeadLength, float lineWidth, ref float arrowLength, ref float arrowAngle, ref float adjustValue)
	{
		switch (arrowHeadLength)
		{
		case LineEndLength.ShortArrow:
			if (lineWidth <= 1f)
			{
				arrowLength = 5f;
			}
			else
			{
				arrowLength = lineWidth * 2.8f;
			}
			arrowAngle = 32f;
			adjustValue = lineWidth * 0.9f;
			break;
		case LineEndLength.MediumLenArrow:
			if (lineWidth <= 1f)
			{
				arrowLength = 7f;
			}
			else
			{
				arrowLength = (float)Math.Round((double)lineWidth * 3.5);
			}
			arrowAngle = 22f;
			adjustValue = lineWidth * 1.3f;
			break;
		case LineEndLength.LongArrow:
			if (lineWidth <= 1f)
			{
				arrowLength = 9.5f;
			}
			else
			{
				arrowLength = (float)Math.Round(lineWidth * 5f);
			}
			arrowAngle = 15.5f;
			adjustValue = lineWidth * 1.83f;
			break;
		}
	}

	private void GetOpenMediumArrowDefaultValues(LineEndLength arrowHeadLength, float lineWidth, ref float arrowLength, ref float arrowAngle, ref float adjustValue)
	{
		switch (arrowHeadLength)
		{
		case LineEndLength.ShortArrow:
			if (lineWidth <= 1f)
			{
				arrowLength = 5.5f;
			}
			else
			{
				arrowLength = lineWidth * 3f;
			}
			arrowAngle = 41f;
			adjustValue = lineWidth * 0.75f;
			break;
		case LineEndLength.MediumLenArrow:
			if (lineWidth <= 1f)
			{
				arrowLength = 7f;
			}
			else
			{
				arrowLength = (float)Math.Round((double)lineWidth * 3.8);
			}
			arrowAngle = 30f;
			adjustValue = lineWidth;
			break;
		case LineEndLength.LongArrow:
			if (lineWidth <= 1f)
			{
				arrowLength = 10f;
			}
			else
			{
				arrowLength = (float)Math.Round(lineWidth * 5f);
			}
			arrowAngle = 21f;
			adjustValue = lineWidth * 1.35f;
			break;
		}
	}

	private void GetOpenWideArrowDefaultValues(LineEndLength arrowHeadLength, float lineWidth, ref float arrowLength, ref float arrowAngle, ref float adjustValue)
	{
		switch (arrowHeadLength)
		{
		case LineEndLength.ShortArrow:
			if (lineWidth <= 1f)
			{
				arrowLength = 6.5f;
			}
			else
			{
				arrowLength = lineWidth * 3.7f;
			}
			arrowAngle = 52f;
			adjustValue = lineWidth * 0.65f;
			break;
		case LineEndLength.MediumLenArrow:
			if (lineWidth <= 1f)
			{
				arrowLength = 8f;
			}
			else
			{
				arrowLength = (float)Math.Round((double)lineWidth * 4.2);
			}
			arrowAngle = 40f;
			adjustValue = lineWidth;
			break;
		case LineEndLength.LongArrow:
			if (lineWidth <= 1f)
			{
				arrowLength = 10.5f;
			}
			else
			{
				arrowLength = (float)Math.Round(lineWidth * 5.7f);
			}
			arrowAngle = 29f;
			adjustValue = lineWidth;
			break;
		}
	}

	private double FindAngleToLeftAndRightHeadPoint(bool isFlipHorizontal, bool isFlipVertical, float width, PointF point1, PointF point2, bool isFromBeginArrow)
	{
		double num = 0.0;
		if (isFlipHorizontal && isFlipVertical)
		{
			num = ((width != 0f) ? (180.0 - RadianToDegree(FindArrowHeadAngleRadians(point1, point2, isFromSeparateOrientation: true))) : (360.0 - RadianToDegree(FindArrowHeadAngleRadians(point1, point2, isFromSeparateOrientation: true))));
			if (isFromBeginArrow && width != 0f)
			{
				num -= 180.0;
			}
		}
		else if (isFlipVertical || isFlipHorizontal)
		{
			num = 360.0 - RadianToDegree(FindArrowHeadAngleRadians(point1, point2, isFromSeparateOrientation: false));
		}
		else
		{
			num = 360.0 - RadianToDegree(FindArrowHeadAngleRadians(point1, point2, isFromSeparateOrientation: true));
			if (isFromBeginArrow && width != 0f)
			{
				num -= 180.0;
			}
		}
		return num;
	}

	private double FindAngleToLeftAndRightHeadPoint(Shape shape, PointF point1, PointF point2, bool isFromBeginArrow)
	{
		double num = 0.0;
		if (shape.FlipHorizontal && shape.FlipVertical)
		{
			num = ((shape.Width != 0f) ? (180.0 - RadianToDegree(FindArrowHeadAngleRadians(point1, point2, isFromSeparateOrientation: true))) : (360.0 - RadianToDegree(FindArrowHeadAngleRadians(point1, point2, isFromSeparateOrientation: true))));
			if (isFromBeginArrow && shape.Width != 0f)
			{
				num -= 180.0;
			}
		}
		else if (shape.FlipVertical || shape.FlipHorizontal)
		{
			num = 360.0 - RadianToDegree(FindArrowHeadAngleRadians(point1, point2, isFromSeparateOrientation: false));
		}
		else
		{
			num = 360.0 - RadianToDegree(FindArrowHeadAngleRadians(point1, point2, isFromSeparateOrientation: true));
			if (isFromBeginArrow && shape.Width != 0f)
			{
				num -= 180.0;
			}
		}
		return num;
	}

	private double FindArrowHeadAngleRadians(PointF point1, PointF point2, bool isFromSeparateOrientation)
	{
		PointF pointF = new PointF(isFromSeparateOrientation ? point1.X : 0f, point2.Y);
		PointF pointF2 = point2;
		PointF pointF3 = point2;
		PointF pointF4 = point1;
		return Math.Atan2(pointF2.Y - pointF.Y, pointF2.X - pointF.X) - Math.Atan2(pointF4.Y - pointF3.Y, pointF4.X - pointF3.X);
	}

	private PointF FindBaseLineEndPoint(bool isFlipHorizontal, bool isFlipVertical, float width, float height, PointF[] linePoints, float adjustValue, bool isFromBeginArrow)
	{
		float x = 0f;
		float y = 0f;
		if ((isFlipHorizontal && isFlipVertical) || isFlipHorizontal)
		{
			double num = 0.0;
			num = ((width != 0f) ? (180.0 - RadianToDegree(FindAngleRadians(linePoints, isFromBottomToTop: false))) : (360.0 - RadianToDegree(FindAngleRadians(linePoints, isFromBottomToTop: false))));
			GetEndPointForBaseLine(isFromBeginArrow, num, Math.Sqrt(width * width + height * height), adjustValue, linePoints, ref x, ref y);
		}
		else if (isFlipVertical)
		{
			double degree = 360.0 - RadianToDegree(FindAngleRadians(linePoints, isFromBottomToTop: false));
			GetEndPointForBaseLine(isFromBeginArrow, degree, Math.Sqrt(width * width + height * height), adjustValue, linePoints, ref x, ref y);
		}
		else
		{
			double degree2 = RadianToDegree(FindAngleRadians(linePoints, isFromBottomToTop: true));
			GetEndPointForBaseLine(isFromBeginArrow, degree2, Math.Sqrt(width * width + height * height), adjustValue, linePoints, ref x, ref y);
		}
		return new PointF(x, y);
	}

	private void GetEndPointForBaseLine(bool isFromBeginArrow, double degree, double length, float adjustValue, PointF[] linePoints, ref float x, ref float y)
	{
		if (isFromBeginArrow)
		{
			degree -= 180.0;
			GetEndPoint(Degree2Radian(degree), (float)length - adjustValue, linePoints[1].X, linePoints[1].Y, ref x, ref y);
		}
		else
		{
			GetEndPoint(Degree2Radian(degree), (float)length - adjustValue, linePoints[0].X, linePoints[0].Y, ref x, ref y);
		}
	}

	private double FindAngleRadians(PointF[] linePoints, bool isFromBottomToTop)
	{
		PointF pointF = linePoints[0];
		PointF pointF2 = new PointF(linePoints[1].X, isFromBottomToTop ? linePoints[1].Y : linePoints[0].Y);
		PointF pointF3 = linePoints[0];
		PointF pointF4 = new PointF(linePoints[1].X, isFromBottomToTop ? linePoints[0].Y : linePoints[1].Y);
		return Math.Atan2(pointF2.Y - pointF.Y, pointF2.X - pointF.X) - Math.Atan2(pointF4.Y - pointF3.Y, pointF4.X - pointF3.X);
	}

	private PointF[] FindArrowHeadPoints(Shape shape, PdfPen pen, RectangleF bounds, PointF[] linePoints, bool isFromOpenArrow, bool isFromBeginArrow)
	{
		PointF[] points = new PointF[4];
		float arrowLength = 0f;
		float arrowAngle = 0f;
		float adjustValue = 0f;
		GetArrowDefaultValues(shape.LineFormat, pen, ref arrowLength, ref arrowAngle, ref adjustValue, isFromOpenArrow, isFromBeginArrow);
		points[0] = FindBaseLineEndPoint(shape.FlipHorizontal, shape.FlipVertical, bounds.Width, bounds.Height, linePoints, adjustValue, isFromBeginArrow);
		FindLeftRightHeadPoints(shape.FlipHorizontal, shape.FlipVertical, bounds.Width, linePoints, ref points, arrowAngle, arrowLength, isFromBeginArrow);
		return points;
	}

	private PointF[] FindArrowHeadPoints(ChildShape shape, PdfPen pen, RectangleF bounds, PointF[] linePoints, bool isFromOpenArrow, bool isFromBeginArrow)
	{
		PointF[] points = new PointF[4];
		float arrowLength = 0f;
		float arrowAngle = 0f;
		float adjustValue = 0f;
		GetArrowDefaultValues(shape.LineFormat, pen, ref arrowLength, ref arrowAngle, ref adjustValue, isFromOpenArrow, isFromBeginArrow);
		points[0] = FindBaseLineEndPoint(shape.FlipHorizantal, shape.FlipVertical, shape.Width, shape.Height, linePoints, adjustValue, isFromBeginArrow);
		FindLeftRightHeadPoints(shape.FlipHorizantal, shape.FlipHorizantal, shape.Width, linePoints, ref points, arrowAngle, arrowLength, isFromBeginArrow);
		return points;
	}

	private void FindLeftRightHeadPoints(bool isFlipHorizontal, bool isFlipVertical, float width, PointF[] linePoints, ref PointF[] points, float arrowAngle, float arrowLength, bool isFromBeginArrow)
	{
		PointF point = default(PointF);
		PointF point2 = default(PointF);
		ConstrucBasetLine(isFromBeginArrow, points[0], linePoints, ref point, ref point2);
		double num = FindAngleToLeftAndRightHeadPoint(isFlipHorizontal, isFlipVertical, width, point, point2, isFromBeginArrow);
		float end_x = 0f;
		float end_y = 0f;
		GetEndPoint(Degree2Radian(num - (double)arrowAngle), arrowLength, point2.X, point2.Y, ref end_x, ref end_y);
		points[1] = new PointF(end_x, end_y);
		points[2] = new PointF(point2.X, point2.Y);
		GetEndPoint(Degree2Radian(num + (double)arrowAngle), arrowLength, point2.X, point2.Y, ref end_x, ref end_y);
		points[3] = new PointF(end_x, end_y);
	}

	private void ConstrucBasetLine(bool isFromBeginArrow, PointF points, PointF[] linePoints, ref PointF point1, ref PointF point2)
	{
		if (isFromBeginArrow)
		{
			point1 = new PointF(linePoints[1].X, linePoints[1].Y);
			point2 = points;
		}
		else
		{
			point1 = new PointF(linePoints[0].X, linePoints[0].Y);
			point2 = points;
		}
	}

	private void GetArrowDefaultValues(LineFormat lineFormat, PdfPen pen, ref float arrowLength, ref float arrowAngle, ref float adjustValue, bool isFromOpenArrow, bool isFromBeginArrow)
	{
		if (isFromOpenArrow)
		{
			GetOpenArrowDefaultValues(lineFormat, pen.Width, ref arrowLength, ref arrowAngle, ref adjustValue, isFromBeginArrow);
		}
		else
		{
			GetCloseArrowDefaultValues(lineFormat, pen.Width, ref arrowLength, ref arrowAngle, ref adjustValue, isFromBeginArrow);
		}
	}

	private double RadianToDegree(double angle)
	{
		return angle * (180.0 / Math.PI);
	}

	private double Degree2Radian(double a)
	{
		return a * 0.017453292519;
	}

	private void GetEndPoint(double angle, float len, float start_x, float start_y, ref float end_x, ref float end_y)
	{
		end_x = (float)((double)start_x + (double)len * Math.Cos(angle));
		end_y = (float)((double)start_y + (double)len * Math.Sin(angle));
	}

	private PointF[] GetLinePointsBasedOnFlip(bool isFlipHorizontal, bool isFlipVertical, RectangleF bounds)
	{
		PointF[] array = new PointF[2];
		if (isFlipHorizontal && isFlipVertical)
		{
			array[0] = new PointF(bounds.Right, bounds.Bottom);
			array[1] = new PointF(bounds.X, bounds.Y);
		}
		else if (isFlipVertical)
		{
			array[0] = new PointF(bounds.X, bounds.Bottom);
			array[1] = new PointF(bounds.Right, bounds.Y);
		}
		else if (isFlipHorizontal)
		{
			array[0] = new PointF(bounds.Right, bounds.Y);
			array[1] = new PointF(bounds.X, bounds.Bottom);
		}
		else
		{
			array[0] = new PointF(bounds.X, bounds.Y);
			array[1] = new PointF(bounds.Right, bounds.Bottom);
		}
		return array;
	}

	private bool IsSoftHyphen(LayoutedWidget ltWidget)
	{
		bool result = false;
		if (ltWidget != null)
		{
			WTextRange wTextRange = ((ltWidget.Widget is WTextRange) ? (ltWidget.Widget as WTextRange) : ((ltWidget.Widget is SplitStringWidget) ? ((ltWidget.Widget as SplitStringWidget).RealStringWidget as WTextRange) : null));
			if (wTextRange != null && wTextRange.Text == '\u001f'.ToString() && ltWidget.Bounds.Width > 0f)
			{
				result = true;
			}
		}
		return result;
	}

	private StringFormat GetStringFormat(WCharacterFormat charFormat)
	{
		StringFormat stringFormat = new StringFormat();
		stringFormat.FormatFlags &= ~StringFormatFlags.LineLimit;
		stringFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
		stringFormat.FormatFlags |= StringFormatFlags.NoClip;
		stringFormat.Trimming = StringTrimming.Word;
		if (charFormat.Bidi)
		{
			stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
		}
		return stringFormat;
	}

	private DocGen.Drawing.SkiaSharpHelper.Brush GetBrush(Color color)
	{
		return new DocGen.Drawing.SkiaSharpHelper.Brush(color);
	}

	private PdfSolidBrush GetPDFBrush(Color color)
	{
		return new PdfSolidBrush(color);
	}

	public Color GetTextColor(WCharacterFormat charFormat)
	{
		Color color = Color.Black;
		bool flag = false;
		WParagraph wParagraph = currParagraph;
		if (currTextRange != null && !(currTextRange.Owner is InlineContentControl))
		{
			wParagraph = currTextRange.Owner as WParagraph;
		}
		Entity entity = null;
		if (wParagraph != null)
		{
			entity = wParagraph.GetOwnerEntity();
		}
		if (currTextRange != null && currTextRange.Document.RevisionOptions.ShowRevisionMarks)
		{
			RevisionOptions revisionOptions = currTextRange.Document.RevisionOptions;
			if (charFormat.IsInsertRevision && charFormat.IsNeedToShowInsertionMarkups())
			{
				return GetRevisionColor(revisionOptions.InsertedTextColor, isInsertText: true);
			}
			if (charFormat.IsDeleteRevision && charFormat.IsNeedToShowDeletionMarkups())
			{
				return GetRevisionColor(revisionOptions.DeletedTextColor);
			}
		}
		if (isTOCParagraphInHyperLink(currTextRange))
		{
			if (charFormat.PropertiesHash.ContainsKey(1))
			{
				WCharacterStyle charStyle = charFormat.CharStyle;
				color = ((charStyle == null || !(charStyle.Name == "Hyperlink") || !(charStyle.CharacterFormat.TextColor == charFormat.TextColor)) ? charFormat.TextColor : ((wParagraph == null) ? Color.Black : ((wParagraph.ParaStyle != null) ? wParagraph.ParaStyle.CharacterFormat.TextColor : Color.Black)));
			}
			else
			{
				color = ((wParagraph == null) ? Color.Black : ((wParagraph.ParaStyle != null) ? wParagraph.ParaStyle.CharacterFormat.TextColor : Color.Black));
			}
			flag = color == Color.Empty;
		}
		else if (!charFormat.TextColor.IsEmpty)
		{
			color = charFormat.TextColor;
		}
		else if (entity is WTextBox && !(entity as WTextBox).TextBoxFormat.TextThemeColor.IsEmpty)
		{
			color = (entity as WTextBox).TextBoxFormat.TextThemeColor;
		}
		else if (entity is Shape)
		{
			Color fontRefColor = (entity as Shape).FontRefColor;
			if (fontRefColor != Color.Empty)
			{
				color = fontRefColor;
				if (!charFormat.TextBackgroundColor.IsEmpty)
				{
					color = (WordColor.IsNotVeryDarkColor(charFormat.TextBackgroundColor) ? Color.Black : Color.White);
				}
				else if (!wParagraph.ParagraphFormat.BackGroundColor.IsEmpty)
				{
					color = (WordColor.IsNotVeryDarkColor(wParagraph.ParagraphFormat.BackGroundColor) ? Color.Black : Color.White);
				}
				else if (!wParagraph.ParagraphFormat.BackColor.IsEmpty)
				{
					color = (WordColor.IsNotVeryDarkColor(wParagraph.ParagraphFormat.BackColor) ? Color.Black : Color.White);
				}
			}
			else
			{
				flag = true;
			}
		}
		else if (entity is ChildShape)
		{
			Color fontRefColor2 = (entity as ChildShape).FontRefColor;
			if (fontRefColor2 != Color.Empty)
			{
				color = fontRefColor2;
			}
			else
			{
				flag = true;
			}
		}
		else
		{
			flag = true;
		}
		if (flag && wParagraph != null)
		{
			color = Color.Black;
			if (wParagraph.IsInCell && wParagraph.GetOwnerEntity() is WTableCell)
			{
				CellFormat cellFormat = (wParagraph.GetOwnerEntity() as WTableCell).CellFormat;
				if (!cellFormat.BackColor.IsEmpty && !WordColor.IsNotVeryDarkColor(cellFormat.BackColor))
				{
					color = Color.White;
				}
				TextureStyle textureStyle = cellFormat.TextureStyle;
				if (textureStyle != 0)
				{
					Color foreColor = cellFormat.ForeColor;
					Color backColor = cellFormat.BackColor;
					if (textureStyle.ToString().Contains("Percent"))
					{
						if (backColor.IsEmpty)
						{
							backColor = Color.White;
						}
						float percent = float.Parse(textureStyle.ToString().Replace("Texture", "").Replace("Percent", "")
							.Replace("Pt", "."), CultureInfo.InvariantCulture);
						foreColor = GetForeColor(foreColor, backColor, percent);
					}
					color = ((((foreColor.IsEmpty || !textureStyle.ToString().Contains("Percent")) && textureStyle != TextureStyle.TextureSolid) || WordColor.IsNotVeryDarkColor(foreColor)) ? Color.Black : Color.White);
				}
			}
			if ((entity is WTextBox && (entity as WTextBox).TextBoxFormat != null && !(entity as WTextBox).TextBoxFormat.FillColor.IsEmpty && !WordColor.IsNotVeryDarkColor((entity as WTextBox).TextBoxFormat.FillColor)) || (entity is Shape && !(entity as Shape).FillFormat.Color.IsEmpty && !WordColor.IsNotVeryDarkColor((entity as Shape).FillFormat.Color)))
			{
				color = Color.White;
			}
			if (!wParagraph.ParagraphFormat.BackColor.IsEmpty && !WordColor.IsNotVeryDarkColor(wParagraph.ParagraphFormat.BackColor))
			{
				color = Color.White;
			}
			if (!charFormat.TextBackgroundColor.IsEmpty)
			{
				color = ((!WordColor.IsNotVeryDarkColor(charFormat.TextBackgroundColor)) ? Color.White : Color.Black);
			}
		}
		if (color == Color.Transparent)
		{
			color = Color.White;
		}
		return color;
	}

	public DocGen.Drawing.Font GetFont(WTextRange txtRange, WCharacterFormat charFormat, string text)
	{
		DocGen.Drawing.Font font = null;
		string text2 = null;
		float num = 0f;
		FontStyle fontStyle = FontStyle.Regular;
		if (txtRange != null && txtRange.OwnerParagraph != null && txtRange.OwnerParagraph == null)
		{
			return GetFont(txtRange.ScriptType, charFormat, text);
		}
		if (txtRange != null)
		{
			text2 = txtRange.CharacterFormat.GetFontNameToRender(txtRange.ScriptType);
		}
		num = charFormat.GetFontSizeToRender();
		if (txtRange != null && txtRange.CharacterFormat != null)
		{
			string charStyleName = txtRange.CharacterFormat.CharStyleName;
			FontStyle fontStyle2 = charFormat.GetFontStyle();
			if (!(charStyleName == "Hyperlink") || !IsTOC(txtRange))
			{
				fontStyle = fontStyle2;
			}
			if (IsTOC(txtRange))
			{
				fontStyle = ((txtRange.CharacterFormat.CharStyle != null && ((charStyleName != null && charStyleName.ToLower() == "hyperlink") || txtRange.CharacterFormat.CharStyle.StyleId == 85)) ? (fontStyle2 & ~txtRange.CharacterFormat.CharStyle.CharacterFormat.GetFontStyle()) : fontStyle2);
			}
		}
		if (txtRange != null && txtRange.Text.Trim(' ') == string.Empty)
		{
			_ = ((IWidget)txtRange).LayoutInfo;
		}
		if (charFormat.HasValue(72) && IsUnicodeText(text) && txtRange != null)
		{
			text2 = charFormat.GetFontNameFromHint(txtRange.ScriptType);
		}
		if (text2 == "Times New Roman Bold")
		{
			text2 = "Times New Roman";
		}
		if (num == 0f)
		{
			num = 0.5f;
		}
		if (text2 == "ArialUnicodeMS")
		{
			text2 = ((!charFormat.Document.FontSubstitutionTable.ContainsKey(text2)) ? "Arial" : charFormat.Document.FontSubstitutionTable[text2]);
			font = charFormat.Document.FontSettings.GetFont(text2, num, fontStyle, txtRange.ScriptType);
		}
		else
		{
			font = CreateFont(charFormat, text2, num, fontStyle);
			UpdateAlternateFont(charFormat, text2, ref font);
		}
		if (txtRange != null && (txtRange.CharacterFormat.Bidi || txtRange.CharacterFormat.ComplexScript))
		{
			font = UpdateBidiFont(txtRange.ScriptType, txtRange.CharacterFormat, font, num, txtRange.CharacterFormat.GetFontStyle());
		}
		return font;
	}

	private Color GetRevisionColor(RevisionColor revisionColor)
	{
		return GetRevisionColor(revisionColor, isInsertText: false);
	}

	private Color GetRevisionColor(RevisionColor revisionColor, bool isInsertText)
	{
		return GetRevisionColor(revisionColor, isInsertText, isResolvedComment: false);
	}

	private Color GetRevisionColor(RevisionColor revisionColor, bool isInsertText, bool isResolvedComment)
	{
		switch (revisionColor)
		{
		case RevisionColor.ByAuthor:
		case RevisionColor.Auto:
			if (isInsertText)
			{
				return Color.FromArgb(46, 151, 211);
			}
			if (isResolvedComment)
			{
				return Color.FromArgb(252, 177, 194);
			}
			return Color.FromArgb(181, 8, 46);
		case RevisionColor.Black:
			if (isResolvedComment)
			{
				return Color.FromArgb(191, 191, 191);
			}
			return Color.FromArgb(0, 0, 0);
		case RevisionColor.Blue:
			if (isResolvedComment)
			{
				return Color.FromArgb(202, 228, 244);
			}
			return Color.FromArgb(46, 151, 211);
		case RevisionColor.BrightGreen:
			if (isResolvedComment)
			{
				return Color.FromArgb(224, 232, 215);
			}
			return Color.FromArgb(132, 163, 91);
		case RevisionColor.DarkBlue:
			if (isResolvedComment)
			{
				return Color.FromArgb(199, 220, 235);
			}
			return Color.FromArgb(55, 110, 150);
		case RevisionColor.DarkRed:
			if (isResolvedComment)
			{
				return Color.FromArgb(244, 198, 202);
			}
			return Color.FromArgb(136, 24, 36);
		case RevisionColor.DarkYellow:
			if (isResolvedComment)
			{
				return Color.FromArgb(248, 231, 201);
			}
			return Color.FromArgb(224, 154, 43);
		case RevisionColor.Gray25:
			if (isResolvedComment)
			{
				return Color.FromArgb(231, 232, 233);
			}
			return Color.FromArgb(160, 163, 169);
		case RevisionColor.Gray50:
			if (isResolvedComment)
			{
				return Color.FromArgb(209, 213, 216);
			}
			return Color.FromArgb(80, 86, 94);
		case RevisionColor.Green:
			if (isResolvedComment)
			{
				return Color.FromArgb(190, 226, 196);
			}
			return Color.FromArgb(44, 98, 52);
		case RevisionColor.Pink:
			if (isResolvedComment)
			{
				return Color.FromArgb(242, 204, 227);
			}
			return Color.FromArgb(206, 51, 143);
		case RevisionColor.Red:
			if (isResolvedComment)
			{
				return Color.FromArgb(252, 177, 194);
			}
			return Color.FromArgb(181, 8, 46);
		case RevisionColor.Teal:
			if (isResolvedComment)
			{
				return Color.FromArgb(187, 239, 244);
			}
			return Color.FromArgb(27, 156, 171);
		case RevisionColor.Turquoise:
			if (isResolvedComment)
			{
				return Color.FromArgb(218, 239, 242);
			}
			return Color.FromArgb(62, 175, 194);
		case RevisionColor.Violet:
			if (isResolvedComment)
			{
				return Color.FromArgb(219, 196, 230);
			}
			return Color.FromArgb(99, 50, 119);
		case RevisionColor.White:
			return Color.FromArgb(255, 255, 255);
		case RevisionColor.Yellow:
			if (isResolvedComment)
			{
				return Color.FromArgb(254, 243, 218);
			}
			return Color.FromArgb(250, 210, 114);
		case RevisionColor.ClassicRed:
			if (isResolvedComment)
			{
				return Color.FromArgb(255, 191, 191);
			}
			return Color.FromArgb(255, 0, 0);
		case RevisionColor.ClassicBlue:
			if (isResolvedComment)
			{
				return Color.FromArgb(191, 191, 255);
			}
			return Color.FromArgb(0, 0, 255);
		default:
			return Color.Empty;
		}
	}

	private Color GetRevisionFillColor(RevisionColor revisionColor, bool isResolvedComment)
	{
		switch (revisionColor)
		{
		case RevisionColor.ByAuthor:
		case RevisionColor.Auto:
			if (isResolvedComment)
			{
				return Color.FromArgb(255, 244, 247);
			}
			return Color.FromArgb(253, 215, 223);
		case RevisionColor.Black:
			if (isResolvedComment)
			{
				return Color.FromArgb(248, 248, 248);
			}
			return Color.FromArgb(233, 233, 233);
		case RevisionColor.Blue:
			if (isResolvedComment)
			{
				return Color.FromArgb(247, 251, 253);
			}
			return Color.FromArgb(220, 237, 248);
		case RevisionColor.BrightGreen:
			if (isResolvedComment)
			{
				return Color.FromArgb(251, 252, 248);
			}
			return Color.FromArgb(235, 240, 227);
		case RevisionColor.DarkBlue:
			if (isResolvedComment)
			{
				return Color.FromArgb(248, 250, 252);
			}
			return Color.FromArgb(224, 235, 243);
		case RevisionColor.DarkRed:
			if (isResolvedComment)
			{
				return Color.FromArgb(254, 245, 247);
			}
			return Color.FromArgb(249, 219, 222);
		case RevisionColor.DarkYellow:
			if (isResolvedComment)
			{
				return Color.FromArgb(254, 251, 245);
			}
			return Color.FromArgb(250, 238, 218);
		case RevisionColor.Gray25:
			if (isResolvedComment)
			{
				return Color.FromArgb(250, 250, 250);
			}
			return Color.FromArgb(233, 235, 235);
		case RevisionColor.Gray50:
			if (isResolvedComment)
			{
				return Color.FromArgb(250, 250, 250);
			}
			return Color.FromArgb(232, 234, 236);
		case RevisionColor.Green:
			if (isResolvedComment)
			{
				return Color.FromArgb(248, 252, 249);
			}
			return Color.FromArgb(225, 242, 227);
		case RevisionColor.Pink:
			if (isResolvedComment)
			{
				return Color.FromArgb(253, 247, 250);
			}
			return Color.FromArgb(247, 221, 236);
		case RevisionColor.Red:
			if (isResolvedComment)
			{
				return Color.FromArgb(255, 244, 247);
			}
			return Color.FromArgb(253, 215, 223);
		case RevisionColor.Teal:
			if (isResolvedComment)
			{
				return Color.FromArgb(245, 253, 254);
			}
			return Color.FromArgb(218, 247, 250);
		case RevisionColor.Turquoise:
			if (isResolvedComment)
			{
				return Color.FromArgb(248, 251, 252);
			}
			return Color.FromArgb(223, 241, 244);
		case RevisionColor.Violet:
			if (isResolvedComment)
			{
				return Color.FromArgb(251, 248, 252);
			}
			return Color.FromArgb(237, 225, 242);
		case RevisionColor.White:
			if (isResolvedComment)
			{
				return Color.FromArgb(248, 248, 248);
			}
			return Color.FromArgb(233, 233, 233);
		case RevisionColor.Yellow:
			if (isResolvedComment)
			{
				return Color.FromArgb(255, 252, 244);
			}
			return Color.FromArgb(254, 242, 214);
		case RevisionColor.ClassicRed:
			if (isResolvedComment)
			{
				return Color.FromArgb(255, 244, 244);
			}
			return Color.FromArgb(255, 213, 213);
		case RevisionColor.ClassicBlue:
			if (isResolvedComment)
			{
				return Color.FromArgb(244, 244, 255);
			}
			return Color.FromArgb(213, 213, 255);
		default:
			return Color.Empty;
		}
	}

	private void UpdateAlternateFont(WCharacterFormat charFormat, string fontName, ref DocGen.Drawing.Font font)
	{
		if (charFormat.Document != null && charFormat.Document.FontSubstitutionTable.ContainsKey(fontName) && fontName != font.Name)
		{
			fontName = charFormat.Document.FontSubstitutionTable[fontName];
		}
		if (charFormat.Document != null)
		{
			FontScriptType scriptType = ((charFormat.OwnerBase is WTextRange) ? (charFormat.OwnerBase as WTextRange).ScriptType : FontScriptType.English);
			font = charFormat.Document.FontSettings.GetFont(fontName, font.Size, font.Style, scriptType);
		}
	}

	public bool IsTOC(WTextRange txtRange)
	{
		if (txtRange != null && txtRange.Owner is WParagraph && txtRange.OwnerParagraph.ChildEntities.FirstItem != null && (ParagraphContainsTOC(txtRange) || ParagraphContainsHyperlink(txtRange)))
		{
			return true;
		}
		return false;
	}

	private bool ParagraphContainsTOC(WTextRange txtRange)
	{
		WParagraph ownerParagraph = txtRange.OwnerParagraph;
		for (int i = 0; i < ownerParagraph.ChildEntities.Count; i++)
		{
			if (ownerParagraph.Items[i] is TableOfContent)
			{
				return true;
			}
		}
		return false;
	}

	private bool ParagraphContainsHyperlink(Entity entity)
	{
		if (entity.Owner is WParagraph wParagraph)
		{
			for (int i = 0; i < wParagraph.ChildEntities.Count; i++)
			{
				if (!(wParagraph.ChildEntities[i] is WField))
				{
					continue;
				}
				if ((wParagraph.ChildEntities[i] as WField).FieldType == FieldType.FieldHyperlink)
				{
					if (new Hyperlink(wParagraph.ChildEntities[i] as WField).BookmarkName != null)
					{
						return new Hyperlink(wParagraph.ChildEntities[i] as WField).BookmarkName.StartsWith("_Toc");
					}
					return false;
				}
				return false;
			}
		}
		return false;
	}

	internal bool isTOCParagraphInHyperLink(WTextRange txtRange)
	{
		IWParagraph iWParagraph = null;
		if (txtRange != null && txtRange.Owner is WParagraph && txtRange.OwnerParagraph.ChildEntities.FirstItem != null)
		{
			iWParagraph = txtRange.Owner as WParagraph;
			if (ParagraphContainsTOC(txtRange))
			{
				for (int i = 1; i < iWParagraph.ChildEntities.Count; i++)
				{
					if (iWParagraph.ChildEntities[i] is WField && ParagraphContainsHyperlink(iWParagraph.ChildEntities[i]))
					{
						if (!IsTextRangeFound(iWParagraph.ChildEntities[i] as WField, txtRange))
						{
							break;
						}
						return true;
					}
				}
			}
			else if (ParagraphContainsHyperlink(iWParagraph.ChildEntities.FirstItem))
			{
				for (int j = 0; j < iWParagraph.ChildEntities.Count; j++)
				{
					if (iWParagraph.ChildEntities[j] is WField && ParagraphContainsHyperlink(iWParagraph.ChildEntities[j]) && IsTextRangeFound(iWParagraph.ChildEntities[j] as WField, txtRange))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool IsTextRangeFound(WField hyperLinkField, WTextRange textRange)
	{
		if (hyperLinkField != null)
		{
			for (int i = 0; i < hyperLinkField.Range.Count; i++)
			{
				if (hyperLinkField.Range.InnerList[i] is WTextRange && hyperLinkField.Range.InnerList[i] == textRange)
				{
					return true;
				}
			}
		}
		return false;
	}

	public DocGen.Drawing.Font GetFont(FontScriptType scriptType, WCharacterFormat charFormat, string text)
	{
		DocGen.Drawing.Font font = null;
		string text2 = charFormat.GetFontNameToRender(scriptType);
		float num = charFormat.GetFontSizeToRender();
		FontStyle fontStyle = charFormat.GetFontStyle();
		if (charFormat.HasValue(72) && IsUnicodeText(text))
		{
			text2 = charFormat.GetFontNameFromHint(scriptType);
		}
		if (text2 == "Times New Roman Bold")
		{
			text2 = "Times New Roman";
		}
		if (num == 0f)
		{
			num = 0.5f;
		}
		if (text2 == "ArialUnicodeMS")
		{
			text2 = ((!charFormat.Document.FontSubstitutionTable.ContainsKey(text2)) ? "Arial" : charFormat.Document.FontSubstitutionTable[text2]);
			font = charFormat.Document.FontSettings.GetFont(text2, num, fontStyle, scriptType);
		}
		else
		{
			font = CreateFont(charFormat, text2, num, fontStyle);
			UpdateAlternateFont(charFormat, text2, ref font);
		}
		if (charFormat.Bidi || charFormat.ComplexScript)
		{
			font = UpdateBidiFont(scriptType, charFormat, font, num, fontStyle);
		}
		return font;
	}

	private DocGen.Drawing.Font UpdateBidiFont(FontScriptType scriptType, WCharacterFormat charFormat, DocGen.Drawing.Font font, float fontSize, FontStyle fontStyle)
	{
		if (charFormat.FontSize != charFormat.GetFontSizeToRender())
		{
			font = charFormat.Document.FontSettings.GetFont(font.Name, charFormat.GetFontSizeToRender(), font.Style, scriptType);
		}
		return font;
	}

	private DocGen.Drawing.Font CreateFont(WCharacterFormat characterFormat, string fontName, float fontSize, FontStyle fontStyle)
	{
		try
		{
			DocGen.Drawing.Font font = null;
			if (characterFormat.Document != null)
			{
				FontScriptType scriptType = ((characterFormat.OwnerBase is WTextRange) ? (characterFormat.OwnerBase as WTextRange).ScriptType : FontScriptType.English);
				font = characterFormat.Document.FontSettings.GetFont(fontName, fontSize, fontStyle, scriptType);
			}
			else
			{
				font = new DocGen.Drawing.Font(fontName, fontSize, fontStyle);
			}
			return font;
		}
		catch
		{
			IFontFamily fontFamily = WordDocument.RenderHelper.GetFontFamily(fontName);
			if (fontFamily.IsStyleAvailable(FontStyle.Bold))
			{
				fontStyle |= FontStyle.Bold;
			}
			if (fontFamily.IsStyleAvailable(FontStyle.Italic))
			{
				fontStyle |= FontStyle.Italic;
			}
			if (fontFamily.IsStyleAvailable(FontStyle.Underline))
			{
				fontStyle |= FontStyle.Underline;
			}
			if (fontFamily.IsStyleAvailable(FontStyle.Strikeout))
			{
				fontStyle |= FontStyle.Strikeout;
			}
			return new DocGen.Drawing.Font(fontName, fontSize, fontStyle);
		}
	}

	private bool HasPrivateFont(string fontName)
	{
		return false;
	}

	private StringAlignment GetStringAlignment(WParagraphFormat paraFormat)
	{
		StringAlignment result = StringAlignment.Near;
		switch (paraFormat.GetAlignmentToRender())
		{
		case HorizontalAlignment.Right:
			result = StringAlignment.Far;
			break;
		case HorizontalAlignment.Center:
			result = StringAlignment.Center;
			break;
		}
		return result;
	}

	private PdfPen GetPen(Border border, bool isParagraphBorder)
	{
		float lineWidth = border.LineWidth;
		PdfPen pdfPen = null;
		Color color = Color.Black;
		if (!border.Color.IsEmpty && border.Color.ToArgb() != 0)
		{
			color = border.Color;
		}
		pdfPen = new PdfPen(new PdfColor(color), border.LineWidth);
		pdfPen.DashStyle = PdfDashStyle.Custom;
		switch (border.BorderType)
		{
		case BorderStyle.DashLargeGap:
			pdfPen.DashPattern = new float[2] { 3f, 5f };
			break;
		case BorderStyle.DashSmallGap:
			pdfPen.DashPattern = new float[2] { 3f, 3f };
			break;
		case BorderStyle.Dot:
			pdfPen.DashPattern = new float[2] { 1f, 2f };
			break;
		case BorderStyle.DotDash:
			pdfPen.DashPattern = new float[4] { 4f, 3f, 1f, 3f };
			break;
		case BorderStyle.DotDotDash:
			pdfPen.DashPattern = new float[6] { 8f, 3f, 1f, 3f, 1f, 3f };
			break;
		case BorderStyle.Double:
			if (!isParagraphBorder)
			{
				pdfPen.Width = lineWidth * 3f;
				pdfPen.CompoundArray = new float[4]
				{
					0f,
					0.3333333f,
					2f / 3f,
					1f
				};
			}
			break;
		case BorderStyle.Triple:
			pdfPen.Width = lineWidth * 5f;
			pdfPen.CompoundArray = new float[6]
			{
				0f,
				0.1666667f,
				0.3333333f,
				2f / 3f,
				5f / 6f,
				1f
			};
			break;
		}
		if (!isParagraphBorder)
		{
			pdfPen.StartCap = PdfLineCap.Square;
			pdfPen.EndCap = PdfLineCap.Square;
		}
		return pdfPen;
	}

	private PdfPen GetPen(BorderStyle borderType, float borderLineWidth, Color borderColor)
	{
		PdfPen pdfPen = null;
		Color color = Color.Black;
		if (!borderColor.IsEmpty && borderColor.ToArgb() != 0)
		{
			color = borderColor;
		}
		pdfPen = new PdfPen(new PdfColor(color), borderLineWidth);
		pdfPen.DashStyle = PdfDashStyle.Custom;
		switch (borderType)
		{
		case BorderStyle.DashLargeGap:
			pdfPen.DashPattern = new float[2] { 3f, 5f };
			break;
		case BorderStyle.DashSmallGap:
			pdfPen.DashPattern = new float[2] { 3f, 3f };
			break;
		case BorderStyle.Dot:
			pdfPen.DashPattern = new float[2] { 1f, 2f };
			break;
		case BorderStyle.DotDash:
			pdfPen.DashPattern = new float[4] { 4f, 3f, 1f, 3f };
			break;
		case BorderStyle.DotDotDash:
			pdfPen.DashPattern = new float[6] { 8f, 3f, 1f, 3f, 1f, 3f };
			break;
		}
		return pdfPen;
	}

	private PdfPen GetPen(DocGen.Drawing.UnderlineStyle underlineStyle, float lineWidth, Color lineColor)
	{
		PdfPen pdfPen = null;
		Color color = Color.Black;
		if (!lineColor.IsEmpty && lineColor.ToArgb() != 0)
		{
			color = lineColor;
		}
		pdfPen = new PdfPen(new PdfColor(color), lineWidth);
		pdfPen.DashStyle = PdfDashStyle.Custom;
		switch (underlineStyle)
		{
		case DocGen.Drawing.UnderlineStyle.DashLong:
			pdfPen.DashPattern = new float[2] { 10.8f, 5.41f };
			break;
		case DocGen.Drawing.UnderlineStyle.DashLongHeavy:
			pdfPen.Width = lineWidth * 1.75f;
			pdfPen.DashPattern = new float[2] { 6.17f, 3.09f };
			break;
		case DocGen.Drawing.UnderlineStyle.Dash:
			pdfPen.DashPattern = new float[2] { 3f, 5f };
			break;
		case DocGen.Drawing.UnderlineStyle.DashHeavy:
			pdfPen.Width = lineWidth * 1.75f;
			pdfPen.DashPattern = new float[2] { 3.05f, 1.628f };
			break;
		case DocGen.Drawing.UnderlineStyle.Dotted:
			pdfPen.DashPattern = new float[2] { 1f, 2f };
			break;
		case DocGen.Drawing.UnderlineStyle.DottedHeavy:
			pdfPen.Width = lineWidth * 1.75f;
			pdfPen.DashPattern = new float[2] { 1.2f, 1f };
			break;
		case DocGen.Drawing.UnderlineStyle.DotDash:
			pdfPen.DashPattern = new float[4] { 4f, 3f, 1f, 3f };
			break;
		case DocGen.Drawing.UnderlineStyle.DotDashHeavy:
			pdfPen.Width = lineWidth * 1.75f;
			pdfPen.DashPattern = new float[4] { 2.25f, 1.03f, 0.5f, 1.03f };
			break;
		case DocGen.Drawing.UnderlineStyle.DotDotDash:
			pdfPen.DashPattern = new float[6] { 8f, 3f, 1f, 3f, 1f, 3f };
			break;
		case DocGen.Drawing.UnderlineStyle.DotDotDashHeavy:
			pdfPen.Width = lineWidth * 1.45f;
			pdfPen.DashPattern = new float[6] { 3.25f, 1.45f, 1f, 1.45f, 1f, 1.45f };
			break;
		case DocGen.Drawing.UnderlineStyle.Thick:
			pdfPen.Width = lineWidth * 1.819375f;
			break;
		case DocGen.Drawing.UnderlineStyle.Double:
			pdfPen.Width = lineWidth * 2.55f;
			break;
		case DocGen.Drawing.UnderlineStyle.Wavy:
			pdfPen.Width = 0.1f;
			break;
		case DocGen.Drawing.UnderlineStyle.WavyHeavy:
			pdfPen.Width = 1.05f;
			break;
		case DocGen.Drawing.UnderlineStyle.WavyDouble:
			pdfPen.Width = 0.05f;
			break;
		}
		return pdfPen;
	}

	private DocGen.Drawing.SkiaSharpHelper.Image ScaleImage(DocGen.Drawing.SkiaSharpHelper.Image srcImage, float width, float height)
	{
		if ((float)srcImage.Width <= width && (float)srcImage.Height <= height)
		{
			return srcImage;
		}
		_ = srcImage.Width;
		_ = srcImage.Height;
		int num = 0;
		int num2 = 0;
		int num3 = (int)width;
		int num4 = (int)height;
		if (num3 <= 0)
		{
			num3 = 1;
		}
		if (num4 <= 0)
		{
			num4 = 1;
		}
		DocGen.Drawing.SkiaSharpHelper.Bitmap bitmap = CreateBitmap(num3, num4);
		bitmap.SetResolution(srcImage.HorizontalResolution, srcImage.VerticalResolution);
		PdfImage pdfImage = GetPdfImage(bitmap);
		PDFGraphics.DrawImage(pdfImage, new RectangleF(num, num2, num3, num4));
		return null;
	}

	private void AddLinkToBookmark(RectangleF bounds, string bookmarkName, bool isTargetNull)
	{
		BookmarkHyperlink bookmarkHyperlink = null;
		BookmarkHyperlink bookmarkHyperlink2 = null;
		for (int i = 0; i < BookmarkHyperlinksList.Count; i++)
		{
			foreach (KeyValuePair<string, BookmarkHyperlink> item in BookmarkHyperlinksList[i])
			{
				if (item.Key == bookmarkName)
				{
					bookmarkHyperlink = item.Value;
					break;
				}
			}
			if (bookmarkHyperlink != null && bookmarkHyperlink.Hyperlink != null && bookmarkHyperlink.Hyperlink != currHyperlink)
			{
				bookmarkHyperlink2 = ((bookmarkHyperlink.TargetBounds != RectangleF.Empty) ? bookmarkHyperlink : null);
				bookmarkHyperlink = null;
			}
			if (bookmarkHyperlink == null)
			{
				continue;
			}
			if (bookmarkHyperlink.SourceBounds != RectangleF.Empty)
			{
				float x = bookmarkHyperlink.SourceBounds.X;
				float y = bookmarkHyperlink.SourceBounds.Y;
				float width = bookmarkHyperlink.SourceBounds.Width;
				float height = bookmarkHyperlink.SourceBounds.Height;
				if (x > bounds.X)
				{
					x = bounds.X;
				}
				if (y > bounds.Y)
				{
					y = bounds.Y;
				}
				if (bookmarkHyperlink.SourceBounds.Right < bounds.Right)
				{
					width = bounds.Right - x;
				}
				if (bookmarkHyperlink.SourceBounds.Bottom < bounds.Bottom)
				{
					height = bounds.Bottom - y;
				}
				bookmarkHyperlink.SourceBounds = new RectangleF(x, y, width, height);
				UpdateBookmarkTargetBoundsAndPageNumber(bookmarkHyperlink, bookmarkHyperlink.HyperlinkValue);
			}
			else if (bookmarkHyperlink.TargetBounds != RectangleF.Empty)
			{
				bookmarkHyperlink.SourceBounds = bounds;
				bookmarkHyperlink.SourcePageNumber = DocumentLayouter.PageNumber;
				bookmarkHyperlink.Hyperlink = currHyperlink;
			}
		}
		if (bookmarkHyperlink == null)
		{
			Dictionary<string, BookmarkHyperlink> dictionary = new Dictionary<string, BookmarkHyperlink>();
			BookmarkHyperlink bookmarkHyperlink3 = new BookmarkHyperlink();
			bookmarkHyperlink3.HyperlinkValue = bookmarkName;
			bookmarkHyperlink3.SourceBounds = bounds;
			bookmarkHyperlink3.SourcePageNumber = DocumentLayouter.PageNumber;
			bookmarkHyperlink3.Hyperlink = currHyperlink;
			if (isTargetNull)
			{
				bookmarkHyperlink3.IsTargetNull = true;
			}
			else if (bookmarkHyperlink2 != null && bookmarkHyperlink2.HyperlinkValue == bookmarkHyperlink3.HyperlinkValue)
			{
				bookmarkHyperlink3.TargetBounds = bookmarkHyperlink2.TargetBounds;
				bookmarkHyperlink3.TargetPageNumber = bookmarkHyperlink2.TargetPageNumber;
			}
			else
			{
				UpdateBookmarkTargetBoundsAndPageNumber(bookmarkHyperlink3, bookmarkName);
			}
			dictionary.Add(bookmarkHyperlink3.HyperlinkValue, bookmarkHyperlink3);
			BookmarkHyperlinksList.Add(dictionary);
		}
	}

	private void CreateAndAddLinkToBookmark(RectangleF bounds, string bookmarkName, bool isTargetNull)
	{
		Dictionary<string, BookmarkHyperlink> dictionary = new Dictionary<string, BookmarkHyperlink>();
		BookmarkHyperlink bookmarkHyperlink = new BookmarkHyperlink();
		bookmarkHyperlink.HyperlinkValue = bookmarkName;
		bookmarkHyperlink.SourceBounds = bounds;
		bookmarkHyperlink.SourcePageNumber = DocumentLayouter.PageNumber;
		bookmarkHyperlink.Hyperlink = currHyperlink;
		if (isTargetNull)
		{
			bookmarkHyperlink.IsTargetNull = true;
		}
		else
		{
			UpdateBookmarkTargetBoundsAndPageNumber(bookmarkHyperlink, bookmarkName);
		}
		if (BookmarkHyperlinksList.Count != 0)
		{
			for (int i = 0; i < BookmarkHyperlinksList.Count; i++)
			{
				foreach (KeyValuePair<string, BookmarkHyperlink> item in BookmarkHyperlinksList[i])
				{
					if (item.Key == bookmarkName)
					{
						bookmarkHyperlink.TargetBounds = item.Value.TargetBounds;
						bookmarkHyperlink.TargetPageNumber = item.Value.TargetPageNumber;
					}
				}
			}
		}
		dictionary.Add(bookmarkHyperlink.HyperlinkValue, bookmarkHyperlink);
		BookmarkHyperlinksList.Add(dictionary);
	}

	private void AddHyperLink(Hyperlink hyperlink, RectangleF bounds)
	{
		string key = "";
		string text = string.Empty;
		if (hyperlink.Field.IsLocal && hyperlink.Field.LocalReference != null && hyperlink.Field.LocalReference != string.Empty)
		{
			text = "#" + hyperlink.Field.LocalReference;
		}
		switch (hyperlink.Type)
		{
		case HyperlinkType.FileLink:
			key = hyperlink.FilePath + text;
			break;
		case HyperlinkType.EMailLink:
			key = hyperlink.Uri;
			break;
		case HyperlinkType.WebLink:
			key = hyperlink.Uri + text;
			break;
		case HyperlinkType.Bookmark:
		{
			string bookmarkName = hyperlink.Field.FieldValue.Replace("\"", string.Empty);
			if (hyperlink.BookmarkName.ToLower().StartsWith("_toc"))
			{
				bounds = ((!(bounds.X < CurrParagraphBounds.X)) ? CurrParagraphBounds : new RectangleF(bounds.X, CurrParagraphBounds.Y, bounds.Width, CurrParagraphBounds.Height));
			}
			AddLinkToBookmark(bounds, bookmarkName, isTargetNull: false);
			return;
		}
		}
		Dictionary<string, RectangleF> dictionary = new Dictionary<string, RectangleF>();
		dictionary.Add(key, bounds);
		m_hyperLinks.Add(dictionary);
	}

	private void AddHyperLink(WPicture picture, LayoutedWidget ltwidget)
	{
		if (IsValidFieldResult(currHyperlink.Field, ltwidget.Widget))
		{
			AddHyperLink(currHyperlink, ltwidget.Bounds);
		}
		if (picture.NextSibling is WFieldMark && (picture.NextSibling as WFieldMark).Type == FieldMarkType.FieldEnd)
		{
			currHyperlink = null;
		}
	}

	private void UpdateBookmarkTargetBoundsAndPageNumber(BookmarkHyperlink bmhyperlink, string bmHyperlinkValue)
	{
		foreach (BookmarkPosition bookmark in Bookmarks)
		{
			if (bookmark.BookmarkName == bmHyperlinkValue)
			{
				bmhyperlink.TargetBounds = bookmark.Bounds;
				bmhyperlink.TargetPageNumber = bookmark.PageNumber;
				break;
			}
		}
	}

	private void UpdateTOCLevel(WParagraph paragraph, BookmarkHyperlink bookmark)
	{
		if (bookmark.HyperlinkValue == null || !bookmark.HyperlinkValue.StartsWith("_Toc"))
		{
			return;
		}
		string styleName = paragraph.StyleName;
		styleName = ((styleName == null) ? "normal" : styleName.ToLower().Replace(" ", ""));
		foreach (TableOfContent value in paragraph.Document.TOC.Values)
		{
			value.UpdateTOCStyleLevels();
			int num = 1;
			foreach (KeyValuePair<int, List<string>> tOCLevel in value.TOCLevels)
			{
				foreach (string item in tOCLevel.Value)
				{
					if (styleName == item.ToLower().Replace(" ", ""))
					{
						bookmark.TOCLevel = num;
						bookmark.TOCText = paragraph.Text;
						break;
					}
				}
				num++;
			}
		}
	}

	public int GetSplitIndexByOffset(string text, ITextMeasurable measurer, double offset, bool bSplitByChar, bool bIsInCell, float clientWidth, float clientActiveAreaWidth, bool isSplitByCharacter)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (measurer == null)
		{
			throw new ArgumentNullException("strWidget");
		}
		if (offset < 0.0)
		{
			throw new ArgumentOutOfRangeException("offset", offset, "Value can not be less 0");
		}
		int result = -1;
		if (clientWidth == 0f)
		{
			clientWidth = (float)offset;
		}
		if (text.Length != 0 && (offset > 0.0 || clientWidth <= 0f))
		{
			int num = 0;
			int num2 = 0;
			double num3 = 0.0;
			while ((num2 = GetWordLength(text, num)) > -1)
			{
				SizeF sizeF = measurer.Measure(text.Substring(num, num2));
				if ((double)sizeF.Width + num3 > offset)
				{
					num = 0;
					break;
				}
				num3 += (double)sizeF.Width;
				num += num2;
			}
			result = num - 1;
			result = UpdateResIndex(text, measurer, result, bSplitByChar, bIsInCell, offset, clientWidth, clientActiveAreaWidth, isSplitByCharacter);
		}
		return result;
	}

	internal int UpdateResIndex(string text, ITextMeasurable measurer, int resIndex, bool bSplitByChar, bool bIsInCell, double offset, float clientWidth, float clientActiveAreaWidth, bool isSplitByCharacter)
	{
		if (resIndex < 0 || bSplitByChar || bIsInCell)
		{
			for (int i = 0; i < text.Length; i++)
			{
				SizeF sizeF = measurer.Measure(text.Substring(0, i + 1));
				if (!((double)sizeF.Width > offset))
				{
					continue;
				}
				resIndex = i - 1;
				if (resIndex == -1 && bIsInCell)
				{
					float cellWidth = GetCellWidth(measurer as WTextRange);
					if (sizeF.Width > cellWidth)
					{
						resIndex = 0;
					}
				}
				if (clientWidth < 0f)
				{
					resIndex = 0;
				}
				break;
			}
		}
		string[] array = text.Split(' ');
		if (IsUnicodeText(text))
		{
			if (resIndex > -1)
			{
				if ((text.Length > resIndex + 1 && IsBeginCharacter(text[resIndex + 1])) || IsLeadingCharacter(text[resIndex]))
				{
					resIndex--;
				}
				if (text.Length > resIndex + 1 && IsOverFlowCharacter(text[resIndex + 1]))
				{
					resIndex++;
				}
			}
		}
		else if (array.Length >= 1 && measurer.Measure(array[0]).Width > clientActiveAreaWidth && !isSplitByCharacter)
		{
			resIndex = -1;
		}
		if (resIndex < 0)
		{
			resIndex = text.Length - 1;
		}
		return resIndex;
	}

	internal bool IsLeadingCharacter(char c)
	{
		switch (c)
		{
		case '"':
		case '$':
		case '\'':
		case '(':
		case '[':
		case '{':
		case '\u009d':
		case '〈':
		case '《':
		case '「':
		case '『':
		case '【':
		case '〔':
		case '＄':
		case '）':
		case 'ｋ':
		case 'ｚ':
		case '￡':
		case '￥':
			return true;
		default:
			return false;
		}
	}

	public bool IsBeginCharacter(char c)
	{
		switch (c)
		{
		case '!':
		case '%':
		case ')':
		case ',':
		case '.':
		case '?':
		case ']':
		case '}':
		case '、':
		case '〉':
		case '》':
		case '」':
		case '〕':
		case '〟':
		case '\u302a':
		case 'う':
		case 'え':
		case 'つ':
		case 'ね':
		case 'は':
		case 'や':
		case 'ゆ':
		case 'ア':
		case 'イ':
		case 'エ':
		case 'ク':
		case 'ー':
		case '）':
		case '．':
		case '］':
		case '\uff3f':
		case '｝':
			return true;
		default:
			return false;
		}
	}

	internal bool IsOverFlowCharacter(char c)
	{
		switch (c)
		{
		case ',':
		case '.':
		case '`':
		case '．':
		case '｡':
			return true;
		default:
			return false;
		}
	}

	public bool IsUnicodeText(string text)
	{
		bool result = false;
		if (text != null)
		{
			foreach (char c in text)
			{
				if ((c >= '\u3000' && c <= 'ヿ') || (c >= '\uff00' && c <= '\uffef') || (c >= '一' && c <= '龯') || (c >= '㐀' && c <= '䶿') || (c >= '가' && c <= '\uffef') || (c >= '\u0d80' && c <= '\u0dff'))
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	public Entity GetPreviousSibling(WTextRange textRange)
	{
		Entity entity = textRange.PreviousSibling as Entity;
		while (entity != null && (!(entity is WTextRange) || (entity as IWidget).LayoutInfo.IsSkip))
		{
			if (entity is BookmarkStart || entity is BookmarkEnd || entity is WFieldMark || (entity is IWidget && (entity as IWidget).LayoutInfo != null && (entity as IWidget).LayoutInfo.IsSkip))
			{
				entity = entity.PreviousSibling as Entity;
				continue;
			}
			return entity;
		}
		return entity;
	}

	public float GetCellWidth(ParagraphItem paraItem)
	{
		WParagraph wParagraph = paraItem.Owner as WParagraph;
		if (wParagraph == null)
		{
			if (paraItem.Owner is InlineContentControl || paraItem.Owner is XmlParagraphItem)
			{
				wParagraph = paraItem.GetOwnerParagraphValue();
			}
			else if (paraItem is WTextRange && (paraItem as WTextRange).ParaItemCharFormat.BaseFormat.OwnerBase is WParagraph)
			{
				wParagraph = (paraItem as WTextRange).ParaItemCharFormat.BaseFormat.OwnerBase as WParagraph;
			}
		}
		float num = 0f;
		if (wParagraph != null)
		{
			WTextBody wTextBody = wParagraph.OwnerTextBody;
			while (!(wTextBody is WTableCell) && wTextBody != null)
			{
				if (wTextBody.Owner is BlockContentControl)
				{
					wTextBody = (wTextBody.Owner as BlockContentControl).Owner as WTextBody;
				}
			}
			if (wTextBody != null && ((IWidget)wTextBody).LayoutInfo is CellLayoutInfo && ((IWidget)wTextBody).LayoutInfo is CellLayoutInfo { CellContentLayoutingBounds: var cellContentLayoutingBounds })
			{
				num = cellContentLayoutingBounds.Width;
			}
		}
		if (wParagraph != null && ((IWidget)wParagraph).LayoutInfo is ParagraphLayoutInfo)
		{
			ParagraphLayoutInfo paragraphLayoutInfo = ((IWidget)wParagraph).LayoutInfo as ParagraphLayoutInfo;
			num -= paragraphLayoutInfo.Margins.Left + paragraphLayoutInfo.Margins.Right + (paragraphLayoutInfo.IsFirstLine ? (paragraphLayoutInfo.FirstLineIndent + paragraphLayoutInfo.ListTab) : 0f);
		}
		if (num < 0f)
		{
			num = 0f;
		}
		return num;
	}

	internal string GetPdfFontCollectionKey(DocGen.Drawing.Font font, bool isUnicode)
	{
		return font.Name.ToLower() + ";" + font.Style.ToString() + ";" + font.Size + ";" + isUnicode;
	}

	internal bool IsUnicode(string text)
	{
		if (text == null)
		{
			return false;
		}
		return Encoding.UTF8.GetByteCount(text) != text.Length;
	}

	private int GetWordLength(string text, int startIndex)
	{
		int result = -1;
		if (text.Length - startIndex > 0)
		{
			int i = startIndex;
			for (int length = text.Length; i < length; i++)
			{
				if (i == length - 1)
				{
					result = 1;
					break;
				}
				if (text[i] == ' ' && text[i + 1] != ' ')
				{
					result = i - startIndex + 1;
					break;
				}
			}
		}
		return result;
	}

	public new void Close()
	{
		if (m_graphics != null)
		{
			if (m_pdfgraphics != null)
			{
				m_pdfgraphics.Clear();
			}
			m_graphics = null;
		}
		if (m_graphicsBmp != null)
		{
			m_graphicsBmp.Dispose();
			m_graphicsBmp = null;
		}
		if (m_hyperLinks != null)
		{
			m_hyperLinks.Clear();
			m_hyperLinks = null;
		}
		if (m_overLappedShapeWidgets != null)
		{
			m_overLappedShapeWidgets.Clear();
			m_overLappedShapeWidgets = null;
		}
		if (ClipBoundsContainer != null)
		{
			ClipBoundsContainer.Clear();
			ClipBoundsContainer = null;
		}
		if (m_fontmetric != null)
		{
			m_fontmetric = null;
		}
		if (m_commentMarks != null)
		{
			m_commentMarks.Clear();
			m_commentMarks = null;
		}
		if (m_previousLineCommentStartMarks != null)
		{
			m_previousLineCommentStartMarks.Clear();
			m_previousLineCommentStartMarks = null;
		}
		if (pdfStructureTable != null)
		{
			pdfStructureTable.Clear();
			pdfStructureTable = null;
		}
	}

	internal void DrawBehindWidgets(LayoutedWidgetList behindWidgets, IWidget ownerWidget, int length, bool isHaveToInitLayoutInfo)
	{
		Dictionary<int, LayoutedWidget> dictionary = new Dictionary<int, LayoutedWidget>();
		for (int i = 0; i < length; i++)
		{
			int orderIndex = GetOrderIndex(behindWidgets[i].Widget);
			if (!dictionary.ContainsKey(orderIndex))
			{
				dictionary.Add(orderIndex, behindWidgets[i]);
			}
		}
		List<int> list = new List<int>(dictionary.Keys);
		list.Sort();
		foreach (int item in list)
		{
			currParagraph = GetOwnerParagraph(dictionary[item]);
			Draw(dictionary[item], isHaveToInitLayoutInfo);
			behindWidgets.Remove(dictionary[item]);
		}
		dictionary.Clear();
		list.Clear();
		m_orderIndex = 1;
	}

	internal int GetOrderIndex(IWidget widget)
	{
		int num = 0;
		if (widget is WPicture)
		{
			num = (widget as WPicture).OrderIndex;
		}
		else if (widget is Shape)
		{
			num = (widget as Shape).ZOrderPosition;
		}
		else if (widget is WTextBox)
		{
			num = (widget as WTextBox).TextBoxFormat.OrderIndex;
		}
		else if (widget is GroupShape)
		{
			num = (widget as GroupShape).ZOrderPosition;
		}
		else if (widget is WChart)
		{
			num = (widget as WChart).ZOrderPosition;
		}
		else if (widget is WOleObject && (widget as WOleObject).OlePicture != null)
		{
			num = (widget as WOleObject).OlePicture.OrderIndex;
		}
		if (num == int.MaxValue || num == 0)
		{
			num = m_orderIndex;
			m_orderIndex++;
		}
		return num;
	}

	public float GetLineWidth(BorderCode border)
	{
		float num = 0f;
		if (border.BorderType != 0)
		{
			num = border.LineWidth / 8;
			if (num == 0f)
			{
				num = 0.75f;
			}
		}
		return num;
	}

	public float GetLineWidth(WPicture picture)
	{
		float result = 0f;
		if (picture.PictureShape.ShapeContainer != null && picture.PictureShape.ShapeContainer.ShapeOptions != null && picture.PictureShape.ShapeContainer.ShapeOptions.LineProperties.Line)
		{
			if (picture.Document.ActualFormatType != 0 && !picture.IsShape && !picture.PictureShape.ShapeContainer.ShapeOptions.Properties.ContainsKey(448))
			{
				return result;
			}
			result = ((!picture.PictureShape.ShapeContainer.ShapeOptions.Properties.ContainsKey(459)) ? 0.75f : ((float)(float.TryParse(picture.PictureShape.ShapeContainer.GetPropertyValue(459).ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out result) ? ((double)result / 12700.0) : 0.75)));
		}
		return result;
	}
}
