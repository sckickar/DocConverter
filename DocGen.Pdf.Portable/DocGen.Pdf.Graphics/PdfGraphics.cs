using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using DocGen.Drawing;
using DocGen.Pdf.ColorSpace;
using DocGen.Pdf.Graphics.Fonts;
using DocGen.Pdf.Graphics.Images.Decoder;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Graphics;

public sealed class PdfGraphics
{
	internal delegate PdfResources GetResources();

	internal delegate void StructElementEventHandler(PdfTag tag);

	private struct TransparencyData
	{
		internal float AlphaPen;

		internal float AlphaBrush;

		internal PdfBlendMode BlendMode;

		internal TransparencyData(float alphaPen, float alphaBrush, PdfBlendMode blendMode)
		{
			AlphaPen = alphaPen;
			AlphaBrush = alphaBrush;
			BlendMode = blendMode;
		}

		public override bool Equals(object obj)
		{
			bool result = false;
			if (obj != null && obj is TransparencyData transparencyData)
			{
				result = true;
				result &= AlphaBrush == transparencyData.AlphaBrush;
				result &= AlphaPen == transparencyData.AlphaPen;
				result &= BlendMode == transparencyData.BlendMode;
			}
			return result;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}

	private const int PathTypesValuesMask = 15;

	internal bool m_isEMF;

	internal bool m_isEMFPlus;

	internal bool m_isUseFontSize;

	internal bool m_isBaselineFormat = true;

	internal float m_DpiY;

	private PdfStreamWriter m_streamWriter;

	private GetResources m_getResources;

	private SizeF m_canvasSize;

	internal RectangleF m_clipBounds;

	internal bool m_bStateSaved;

	private PdfPen m_currentPen;

	private PdfBrush m_currentBrush;

	private PdfFont m_currentFont;

	private PdfColorSpace m_currentColorSpace;

	private bool m_istransparencySet;

	private bool m_bCSInitialized;

	private bool m_CIEColors;

	private bool m_isItalic;

	private bool m_isSkewed;

	private bool m_isRestoreGraphics;

	internal float m_cellBorderMaxHeight;

	private PdfGraphicsState gState;

	private Stack<PdfGraphicsState> m_graphicsState;

	private PdfTransformationMatrix m_matrix;

	private TextRenderingMode m_previousTextRenderingMode;

	private float m_previousCharacterSpacing;

	private float m_previousWordSpacing;

	private float m_previousTextScaling = 100f;

	private Dictionary<TransparencyData, PdfTransparency> m_trasparencies;

	private PdfStringFormat m_currentStringFormat;

	private PdfPageLayer m_layer;

	private PdfLayer m_documentLayer;

	private PdfAutomaticFieldInfoCollection m_automaticFields;

	private PdfStringLayoutResult m_stringLayoutResult;

	private float m_split;

	private bool m_isTransparentBrush;

	private static bool m_transparencyObject = false;

	private static object s_transparencyLock = new object();

	private static object s_syncLockTemplate = new object();

	private static object s_rtlRenderLock = new object();

	internal SizeF m_emfScalingFactor = SizeF.Empty;

	internal bool m_isEmfTextScaled;

	internal bool isImageOptimized;

	internal bool isStandardUnicode;

	internal PdfDocument m_pdfdocument;

	private float m_mediaBoxUpperRightBound;

	internal PdfArray m_cropBox;

	internal bool m_isWatermarkMediabox;

	internal bool m_isNormalRender = true;

	private PdfColorSpace lastDocumentCS;

	private PdfColorSpace lastGraphicsCS;

	private bool colorSpaceChanged;

	private TextRenderingMode m_textRenderingMode;

	private bool m_isTextRenderingSet;

	private PdfTag m_tag;

	internal bool isBoldFontWeight;

	private bool m_isTaggedPdf;

	private string m_currentTagType;

	private bool isTemplateGraphics;

	internal bool isEmptyLayer;

	private PdfStringLayoutResult m_layoutResult;

	private PdfDictionary m_tableSpan;

	internal bool customTag;

	private Matrix m_transformMatrix;

	private float m_pageScale = 1f;

	private GraphicsUnit m_graphicsUnit;

	private Dictionary<int, PdfGraphicsState> m_graphicsStates;

	private int m_GraphicsStateCount;

	private SizeF m_pageUnitScaleSize;

	private RectangleF RealClip;

	private bool m_stateChanged;

	private bool m_bFirstTransform = true;

	private bool m_stateRestored;

	private bool m_bFirstCall = true;

	private Matrix m_multiplyTransform;

	private RectangleF m_textClip;

	private bool m_clipPath;

	private bool m_isTranslate;

	private SizeF m_translateTransform;

	private RectangleF m_clipBoundsDirectPDF;

	private RectangleF m_DocIOPageBounds;

	private bool m_isDirectPDF;

	private bool m_optimizeIdenticalImages;

	private bool m_isXPStoken;

	private Dictionary<string, string> m_replaceCharacter;

	private bool m_artifactBMCAdded;

	internal PdfArray mBox;

	internal bool bScaleTranform = true;

	public SizeF Size => m_canvasSize;

	internal float MediaBoxUpperRightBound
	{
		get
		{
			return m_mediaBoxUpperRightBound;
		}
		set
		{
			m_mediaBoxUpperRightBound = value;
		}
	}

	public SizeF ClientSize => m_clipBounds.Size;

	public PdfColorSpace ColorSpace
	{
		get
		{
			return m_currentColorSpace;
		}
		set
		{
			if (PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_X1A2001)
			{
				m_currentColorSpace = value;
			}
		}
	}

	internal PdfStreamWriter StreamWriter
	{
		get
		{
			return m_streamWriter;
		}
		set
		{
			m_streamWriter = value;
		}
	}

	internal PdfTransformationMatrix Matrix
	{
		get
		{
			if (m_matrix == null)
			{
				m_matrix = new PdfTransformationMatrix();
			}
			return m_matrix;
		}
	}

	internal PdfPageLayer Layer => m_layer;

	internal PdfPageBase Page
	{
		get
		{
			if (m_layer == null)
			{
				return null;
			}
			if (m_documentLayer != null)
			{
				return m_documentLayer.Page;
			}
			if (m_layer == null)
			{
				return null;
			}
			return m_layer.Page;
		}
	}

	internal PdfAutomaticFieldInfoCollection AutomaticFields
	{
		get
		{
			if (m_automaticFields == null)
			{
				m_automaticFields = new PdfAutomaticFieldInfoCollection();
			}
			return m_automaticFields;
		}
	}

	internal PdfStringLayoutResult StringLayoutResult => m_stringLayoutResult;

	internal float Split
	{
		get
		{
			return m_split;
		}
		set
		{
			m_split = value;
		}
	}

	internal static bool TransparencyObject => m_transparencyObject;

	internal PdfTag Tag
	{
		get
		{
			return m_tag;
		}
		set
		{
			m_tag = value;
		}
	}

	internal bool IsTaggedPdf
	{
		get
		{
			if (Layer != null && Page is PdfPage && (Page as PdfPage).Section.ParentDocument is PdfDocument && ((Page as PdfPage).Section.ParentDocument as PdfDocument).AutoTag)
			{
				m_isTaggedPdf = true;
			}
			return m_isTaggedPdf;
		}
	}

	internal string CurrentTagType
	{
		get
		{
			return m_currentTagType;
		}
		set
		{
			m_currentTagType = value;
			OnStructElementChanged(Tag);
			this.StructElementChanged = null;
		}
	}

	internal bool IsTemplateGraphics
	{
		get
		{
			return isTemplateGraphics;
		}
		set
		{
			isTemplateGraphics = value;
		}
	}

	internal PdfStringLayoutResult LayoutResult
	{
		get
		{
			return m_layoutResult;
		}
		set
		{
			m_layoutResult = value;
		}
	}

	internal Matrix Transform
	{
		get
		{
			return m_transformMatrix;
		}
		set
		{
			m_transformMatrix = value;
			PdfTransformationMatrix matrix = PrepareMatrix(value, PageScale);
			PutComment("Transform property");
			SetTransform();
			MultiplyTransform(matrix);
			Matrix.Multiply(matrix);
		}
	}

	private float PageScale
	{
		get
		{
			return m_pageScale;
		}
		set
		{
			m_pageScale = value;
			PutComment("PageScale property");
			SetTransform();
			ScaleTransform(value, value);
		}
	}

	internal GraphicsUnit PageUnit
	{
		get
		{
			return m_graphicsUnit;
		}
		set
		{
			m_graphicsUnit = value;
			float num = 1f;
			float num2 = 1f;
			if (value != GraphicsUnit.Display)
			{
				PdfUnitConvertor pdfUnitConvertor = new PdfUnitConvertor();
				PdfUnitConvertor pdfUnitConvertor2 = new PdfUnitConvertor();
				num = pdfUnitConvertor.ConvertUnits(num, GraphicsToPrintUnits(value), PdfGraphicsUnit.Pixel);
				num2 = pdfUnitConvertor2.ConvertUnits(num2, GraphicsToPrintUnits(value), PdfGraphicsUnit.Pixel);
			}
			PutComment("PageUnit property");
			m_pageUnitScaleSize = new SizeF(num, num2);
		}
	}

	internal RectangleF ClipBounds
	{
		get
		{
			return m_textClip;
		}
		set
		{
			m_textClip = value;
		}
	}

	internal RectangleF DocIOPageBounds
	{
		get
		{
			return m_DocIOPageBounds;
		}
		set
		{
			m_DocIOPageBounds = value;
			m_clipBoundsDirectPDF = value;
		}
	}

	internal bool IsDirectPDF
	{
		get
		{
			return m_isDirectPDF;
		}
		set
		{
			m_isDirectPDF = value;
			if (!value)
			{
				return;
			}
			if (PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_X1A2001)
			{
				if (Page != null && Page is PdfPage pdfPage)
				{
					pdfPage.Document.ColorSpace = PdfColorSpace.CMYK;
				}
				ColorSpace = PdfColorSpace.CMYK;
			}
			if (Page != null && Page is PdfPage)
			{
				PdfPage pdfPage2 = Page as PdfPage;
				if (pdfPage2.Document != null)
				{
					pdfPage2.Document.m_WordtoPDFTagged = true;
				}
			}
		}
	}

	internal bool OptimizeIdenticalImages
	{
		get
		{
			return m_optimizeIdenticalImages;
		}
		set
		{
			m_optimizeIdenticalImages = value;
		}
	}

	internal bool XPSToken
	{
		get
		{
			return m_isXPStoken;
		}
		set
		{
			m_isXPStoken = value;
		}
	}

	internal Dictionary<string, string> XPSReplaceCharacter
	{
		get
		{
			if (m_replaceCharacter == null)
			{
				m_replaceCharacter = new Dictionary<string, string>();
			}
			return m_replaceCharacter;
		}
	}

	internal PdfDictionary TableSpan
	{
		get
		{
			return m_tableSpan;
		}
		set
		{
			m_tableSpan = value;
		}
	}

	internal event StructElementEventHandler StructElementChanged;

	internal PdfGraphics(SizeF size, GetResources resources, PdfStreamWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (resources == null)
		{
			throw new ArgumentNullException("resources");
		}
		m_streamWriter = writer;
		m_getResources = resources;
		m_canvasSize = size;
		Initialize();
	}

	internal PdfGraphics(SizeF size, GetResources resources, PdfStream stream)
		: this(size, resources, new PdfStreamWriter(stream))
	{
	}

	public void DrawLine(PdfPen pen, PointF point1, PointF point2)
	{
		if (IsDirectPDF)
		{
			OnDrawPrimitive();
			if (pen.Color.A == 0)
			{
				pen = new PdfPen(Color.White);
			}
		}
		DrawLine(pen, point1.X, point1.Y, point2.X, point2.Y);
	}

	public void DrawLine(PdfPen pen, float x1, float y1, float x2, float y2)
	{
		BeginMarkContent();
		bool flag = false;
		if (((!IsTemplateGraphics && (Tag != null || IsTaggedPdf)) || (IsTaggedPdf && !m_isEMF)) && !m_artifactBMCAdded)
		{
			if (Tag == null)
			{
				Tag = new PdfStructureElement(PdfTagType.Figure);
			}
			PdfPath pdfPath = new PdfPath();
			pdfPath.AddLine(x1, y1, x2, y2);
			Tag.Bounds = pdfPath.GetBounds();
			StructElementChanged += ApplyTag;
			CurrentTagType = "Figure";
			Tag = null;
			flag = true;
		}
		if (pen != null && pen.Color.IsEmpty && (pen.Color == new PdfColor(Color.Transparent) || pen == PdfPens.Transparent))
		{
			pen = null;
		}
		StateControl(pen, null, null);
		CapControl(pen, x1, y1, x2, y2);
		CapControl(pen, x2, y2, x1, y1);
		PdfStreamWriter streamWriter = StreamWriter;
		streamWriter.BeginPath(x1, y1);
		streamWriter.AppendLineSegment(x2, y2);
		streamWriter.StrokePath();
		if (flag)
		{
			StreamWriter.Write("EMC" + Environment.NewLine);
		}
		EndMarkContent();
		m_getResources().RequireProcSet("PDF");
	}

	public void DrawRectangle(PdfPen pen, RectangleF rectangle)
	{
		DrawRectangle(pen, null, rectangle);
	}

	public void DrawRectangle(PdfPen pen, float x, float y, float width, float height)
	{
		DrawRectangle(pen, null, x, y, width, height);
	}

	public void DrawRectangle(PdfBrush brush, RectangleF rectangle)
	{
		if (IsDirectPDF)
		{
			OnDrawPrimitive();
			if (brush is PdfHatchBrush)
			{
				PdfHatchBrush pdfHatchBrush = brush as PdfHatchBrush;
				if (!pdfHatchBrush.BackColor.IsEmpty && pdfHatchBrush.BackColor.A != 0)
				{
					DrawRectangle(new PdfSolidBrush(pdfHatchBrush.BackColor), rectangle);
				}
			}
		}
		DrawRectangle(null, brush, rectangle);
	}

	public void DrawRectangle(PdfBrush brush, float x, float y, float width, float height)
	{
		if (IsDirectPDF)
		{
			OnDrawPrimitive();
		}
		DrawRectangle(null, brush, x, y, width, height);
	}

	public void DrawRectangle(PdfPen pen, PdfBrush brush, RectangleF rectangle)
	{
		DrawRectangle(pen, brush, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
	}

	public void DrawRectangle(PdfPen pen, PdfBrush brush, float x, float y, float width, float height)
	{
		BeginMarkContent();
		bool flag = false;
		if (((!IsTemplateGraphics && (Tag != null || IsTaggedPdf) && !m_isEMF) || (IsTaggedPdf && !m_isEMF) || customTag) && !m_artifactBMCAdded)
		{
			if (Tag == null)
			{
				Tag = new PdfStructureElement(PdfTagType.Figure);
			}
			Tag.Bounds = new RectangleF(x, y, width, height);
			StructElementChanged += ApplyTag;
			CurrentTagType = "Figure";
			Tag = null;
			flag = true;
		}
		if (brush is PdfSolidBrush && (brush as PdfSolidBrush).Color.A == 0)
		{
			m_isTransparentBrush = true;
			lock (s_transparencyLock)
			{
				m_transparencyObject = true;
			}
		}
		if (brush is PdfTilingBrush)
		{
			m_bCSInitialized = false;
			float x2 = m_matrix.OffsetX + x;
			float y2 = ((Layer == null || Layer.Page == null) ? (ClientSize.Height - m_matrix.OffsetY + y) : (Layer.Page.Size.Height - m_matrix.OffsetY + y));
			(brush as PdfTilingBrush).Location = new PointF(x2, y2);
			(brush as PdfTilingBrush).Graphics.ColorSpace = ColorSpace;
		}
		else if (brush is PdfGradientBrush)
		{
			m_bCSInitialized = false;
			(brush as PdfGradientBrush).ColorSpace = ColorSpace;
		}
		if (brush is PdfSolidBrush { Color: { IsEmpty: not false } })
		{
			brush = null;
		}
		if (pen != null && pen.Color.IsEmpty && (pen.Color == new PdfColor(Color.Transparent) || pen == PdfPens.Transparent))
		{
			pen = null;
		}
		StateControl(pen, brush, null);
		StreamWriter.AppendRectangle(x, y, width, height);
		DrawPath(pen, brush, needClosing: false);
		if (flag)
		{
			StreamWriter.Write("EMC" + Environment.NewLine);
		}
		EndMarkContent();
	}

	internal void DrawRoundedRectangle(RectangleF bounds, int radius, PdfPen pen, PdfBrush brush)
	{
		DrawRoundedRectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height, radius, pen, brush);
	}

	internal void DrawRoundedRectangle(float x, float y, float width, float height, int radius, PdfPen pen, PdfBrush brush)
	{
		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}
		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}
		RectangleF rectangle = new RectangleF(x, y, width, height);
		int num = radius * 2;
		RectangleF rectangle2 = new RectangleF(size: new SizeF(num, num), location: rectangle.Location);
		PdfPath pdfPath = new PdfPath();
		if (radius == 0)
		{
			pdfPath.AddRectangle(rectangle);
			DrawPath(pen, brush, pdfPath);
			return;
		}
		pdfPath.AddArc(rectangle2, 180f, 90f);
		rectangle2.X = rectangle.Right - (float)num;
		pdfPath.AddArc(rectangle2, 270f, 90f);
		rectangle2.Y = rectangle.Bottom - (float)num;
		pdfPath.AddArc(rectangle2, 0f, 90f);
		rectangle2.X = rectangle.Left;
		pdfPath.AddArc(rectangle2, 90f, 90f);
		pdfPath.CloseFigure();
		DrawPath(pen, brush, pdfPath);
	}

	public void DrawEllipse(PdfPen pen, RectangleF rectangle)
	{
		DrawEllipse(pen, null, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
	}

	public void DrawEllipse(PdfPen pen, float x, float y, float width, float height)
	{
		DrawEllipse(pen, null, x, y, width, height);
	}

	public void DrawEllipse(PdfBrush brush, RectangleF rectangle)
	{
		DrawEllipse(null, brush, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
	}

	public void DrawEllipse(PdfBrush brush, float x, float y, float width, float height)
	{
		DrawEllipse(null, brush, x, y, width, height);
	}

	public void DrawEllipse(PdfPen pen, PdfBrush brush, RectangleF rectangle)
	{
		DrawEllipse(pen, brush, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
	}

	public void DrawEllipse(PdfPen pen, PdfBrush brush, float x, float y, float width, float height)
	{
		BeginMarkContent();
		bool flag = false;
		if (!IsTemplateGraphics && (Tag != null || IsTaggedPdf) && !m_artifactBMCAdded)
		{
			if (Tag == null)
			{
				Tag = new PdfStructureElement(PdfTagType.Figure);
			}
			Tag.Bounds = new RectangleF(x, y, width, height);
			StructElementChanged += ApplyTag;
			CurrentTagType = "Figure";
			Tag = null;
			flag = true;
		}
		if (brush is PdfTilingBrush)
		{
			m_bCSInitialized = true;
			float x2 = m_matrix.OffsetX + x;
			float y2 = ((Layer == null || Layer.Page == null) ? (ClientSize.Height - m_matrix.OffsetY + y) : (Layer.Page.Size.Height - m_matrix.OffsetY + y));
			(brush as PdfTilingBrush).Location = new PointF(x2, y2);
			(brush as PdfTilingBrush).Graphics.ColorSpace = ColorSpace;
		}
		if (brush is PdfSolidBrush && (brush as PdfSolidBrush).Color.IsEmpty)
		{
			brush = null;
		}
		if (pen != null && ((pen.Color.A == 0 && !pen.Color.IsEmpty) || pen == PdfPens.Transparent))
		{
			pen = null;
		}
		StateControl(pen, brush, null);
		ConstructArcPath(x, y, x + width, y + height, 0f, 360f);
		DrawPath(pen, brush, needClosing: true);
		if (flag)
		{
			StreamWriter.Write("EMC" + Environment.NewLine);
		}
		EndMarkContent();
	}

	public void DrawArc(PdfPen pen, RectangleF rectangle, float startAngle, float sweepAngle)
	{
		if (!IsTemplateGraphics && (Tag != null || IsTaggedPdf) && !m_artifactBMCAdded)
		{
			if (Tag == null)
			{
				Tag = new PdfStructureElement(PdfTagType.Figure);
			}
			Tag.Bounds = rectangle;
			StructElementChanged += ApplyTag;
			CurrentTagType = "Figure";
		}
		if (pen != null && pen.Color.IsEmpty && (pen.Color == new PdfColor(Color.Transparent) || pen == PdfPens.Transparent))
		{
			pen = null;
		}
		DrawArc(pen, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, startAngle, sweepAngle);
	}

	public void DrawArc(PdfPen pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
	{
		BeginMarkContent();
		bool flag = false;
		if (!IsTemplateGraphics && (Tag != null || IsTaggedPdf) && !m_artifactBMCAdded)
		{
			if (Tag == null)
			{
				Tag = new PdfStructureElement(PdfTagType.Figure);
			}
			Tag.Bounds = new RectangleF(x, y, width, height);
			StructElementChanged += ApplyTag;
			CurrentTagType = "Figure";
			flag = true;
		}
		if (pen != null && pen.Color.IsEmpty && (pen.Color == new PdfColor(Color.Transparent) || pen == PdfPens.Transparent))
		{
			pen = null;
		}
		if (sweepAngle != 0f)
		{
			StateControl(pen, null, null);
			ConstructArcPath(x, y, x + width, y + height, startAngle, sweepAngle);
			DrawPath(pen, null, needClosing: false);
		}
		if (flag)
		{
			StreamWriter.Write("EMC" + Environment.NewLine);
		}
		EndMarkContent();
	}

	public void DrawPie(PdfPen pen, RectangleF rectangle, float startAngle, float sweepAngle)
	{
		DrawPie(pen, null, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, startAngle, sweepAngle);
	}

	public void DrawPie(PdfPen pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
	{
		DrawPie(pen, null, x, y, width, height, startAngle, sweepAngle);
	}

	public void DrawPie(PdfBrush brush, RectangleF rectangle, float startAngle, float sweepAngle)
	{
		if (!IsTemplateGraphics && (Tag != null || IsTaggedPdf) && !m_artifactBMCAdded)
		{
			if (Tag == null)
			{
				Tag = new PdfStructureElement(PdfTagType.Figure);
			}
			Tag.Bounds = rectangle;
			StructElementChanged += ApplyTag;
			CurrentTagType = "Figure";
		}
		DrawPie(null, brush, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, startAngle, sweepAngle);
	}

	public void DrawPie(PdfBrush brush, float x, float y, float width, float height, float startAngle, float sweepAngle)
	{
		DrawPie(null, brush, x, y, width, height, startAngle, sweepAngle);
	}

	public void DrawPie(PdfPen pen, PdfBrush brush, RectangleF rectangle, float startAngle, float sweepAngle)
	{
		DrawPie(pen, brush, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, startAngle, sweepAngle);
	}

	public void DrawPie(PdfPen pen, PdfBrush brush, float x, float y, float width, float height, float startAngle, float sweepAngle)
	{
		BeginMarkContent();
		bool flag = false;
		if (!IsTemplateGraphics && (Tag != null || IsTaggedPdf) && !m_artifactBMCAdded)
		{
			if (Tag == null)
			{
				Tag = new PdfStructureElement(PdfTagType.Figure);
			}
			Tag.Bounds = new RectangleF(x, y, width, height);
			StructElementChanged += ApplyTag;
			CurrentTagType = "Figure";
			flag = true;
		}
		if (sweepAngle != 0f)
		{
			if (brush is PdfTilingBrush)
			{
				m_bCSInitialized = false;
				float x2 = m_matrix.OffsetX + x;
				float y2 = ((Layer == null || Layer.Page == null) ? (ClientSize.Height - m_matrix.OffsetY + y) : (Layer.Page.Size.Height - m_matrix.OffsetY + y));
				(brush as PdfTilingBrush).Location = new PointF(x2, y2);
				(brush as PdfTilingBrush).Graphics.ColorSpace = ColorSpace;
			}
			else if (brush is PdfGradientBrush)
			{
				m_bCSInitialized = false;
				(brush as PdfGradientBrush).ColorSpace = ColorSpace;
			}
			if (brush is PdfSolidBrush && (brush as PdfSolidBrush).Color.IsEmpty)
			{
				brush = null;
			}
			if (pen != null && pen.Color.IsEmpty && (pen.Color == new PdfColor(Color.Transparent) || pen == PdfPens.Transparent))
			{
				pen = null;
			}
			StateControl(pen, brush, null);
			ConstructArcPath(x, y, x + width, y + height, startAngle, sweepAngle);
			m_streamWriter.AppendLineSegment(x + width / 2f, y + height / 2f);
			DrawPath(pen, brush, needClosing: true);
			if (flag)
			{
				StreamWriter.Write("EMC" + Environment.NewLine);
			}
			EndMarkContent();
		}
	}

	public void DrawPolygon(PdfPen pen, PointF[] points)
	{
		DrawPolygon(pen, null, points);
	}

	public void DrawPolygon(PdfBrush brush, PointF[] points)
	{
		DrawPolygon(null, brush, points);
	}

	public void DrawPolygon(PdfPen pen, PdfBrush brush, PointF[] points)
	{
		BeginMarkContent();
		bool flag = false;
		if (!IsTemplateGraphics && (Tag != null || IsTaggedPdf) && !m_artifactBMCAdded)
		{
			if (Tag == null)
			{
				Tag = new PdfStructureElement(PdfTagType.Figure);
			}
			PdfPath pdfPath = new PdfPath();
			pdfPath.AddLines(points);
			Tag.Bounds = pdfPath.GetBounds();
			StructElementChanged += ApplyTag;
			CurrentTagType = "Figure";
			Tag = null;
			flag = true;
		}
		if (brush is PdfTilingBrush)
		{
			m_bCSInitialized = false;
			(brush as PdfTilingBrush).Graphics.ColorSpace = ColorSpace;
		}
		else if (brush is PdfGradientBrush)
		{
			m_bCSInitialized = false;
			(brush as PdfGradientBrush).ColorSpace = ColorSpace;
		}
		int num = points.Length;
		if (num > 0)
		{
			if (brush is PdfSolidBrush && (brush as PdfSolidBrush).Color.IsEmpty)
			{
				brush = null;
			}
			if (pen != null && pen.Color.IsEmpty && (pen.Color == new PdfColor(Color.Transparent) || pen == PdfPens.Transparent))
			{
				pen = null;
			}
			StateControl(pen, brush, null);
			m_streamWriter.BeginPath(points[0]);
			for (int i = 1; i < num; i++)
			{
				m_streamWriter.AppendLineSegment(points[i]);
			}
			DrawPath(pen, brush, needClosing: true);
			if (flag)
			{
				StreamWriter.Write("EMC" + Environment.NewLine);
			}
			EndMarkContent();
		}
	}

	public void DrawBezier(PdfPen pen, PointF startPoint, PointF firstControlPoint, PointF secondControlPoint, PointF endPoint)
	{
		DrawBezier(pen, startPoint.X, startPoint.Y, firstControlPoint.X, firstControlPoint.Y, secondControlPoint.X, secondControlPoint.Y, endPoint.X, endPoint.Y);
	}

	public void DrawBezier(PdfPen pen, float startPointX, float startPointY, float firstControlPointX, float firstControlPointY, float secondControlPointX, float secondControlPointY, float endPointX, float endPointY)
	{
		BeginMarkContent();
		bool flag = false;
		if (!IsTemplateGraphics && (Tag != null || IsTaggedPdf) && !m_artifactBMCAdded)
		{
			if (Tag == null)
			{
				Tag = new PdfStructureElement(PdfTagType.Figure);
			}
			PdfPath pdfPath = new PdfPath();
			pdfPath.AddLine(startPointX, startPointY, endPointX, endPointY);
			Tag.Bounds = pdfPath.GetBounds();
			StructElementChanged += ApplyTag;
			CurrentTagType = "Figure";
			Tag = null;
			flag = true;
		}
		if (pen != null && pen.Color.IsEmpty && (pen.Color == new PdfColor(Color.Transparent) || pen == PdfPens.Transparent))
		{
			pen = null;
		}
		StateControl(pen, null, null);
		CapControl(pen, secondControlPointX, secondControlPointY, endPointX, endPointY);
		CapControl(pen, firstControlPointX, firstControlPointY, secondControlPointX, startPointY);
		PdfStreamWriter streamWriter = StreamWriter;
		streamWriter.BeginPath(startPointX, startPointY);
		streamWriter.AppendBezierSegment(firstControlPointX, firstControlPointY, secondControlPointX, secondControlPointY, endPointX, endPointY);
		streamWriter.StrokePath();
		if (flag)
		{
			StreamWriter.Write("EMC" + Environment.NewLine);
		}
		EndMarkContent();
	}

	public void DrawPath(PdfPen pen, PdfPath path)
	{
		DrawPath(pen, null, path);
	}

	public void DrawPath(PdfBrush brush, PdfPath path)
	{
		DrawPath(null, brush, path);
	}

	public void DrawPath(PdfPen pen, PdfBrush brush, PdfPath path)
	{
		bool flag = false;
		BeginMarkContent();
		if (Tag == null || path.PdfTag != null)
		{
			Tag = path.PdfTag;
		}
		if (((!IsTemplateGraphics && (Tag != null || IsTaggedPdf)) || (IsTaggedPdf && !m_isEMF)) && (!m_artifactBMCAdded || path.PdfTag != null))
		{
			if (Tag == null)
			{
				Tag = new PdfStructureElement(PdfTagType.Figure);
			}
			Tag.Bounds = path.GetBounds();
			StructElementChanged += ApplyTag;
			CurrentTagType = "Figure";
			flag = true;
		}
		if (brush is PdfSolidBrush && (brush as PdfSolidBrush).Color.A == 0)
		{
			m_isTransparentBrush = true;
			lock (s_transparencyLock)
			{
				m_transparencyObject = true;
			}
		}
		if (brush is PdfTilingBrush)
		{
			m_bCSInitialized = false;
			float x = m_matrix.OffsetX + path.GetBounds().X;
			float y = ((Layer != null && Layer.Page != null) ? (Layer.Page.Size.Height - m_matrix.OffsetY + path.GetBounds().Y) : (ClientSize.Height - m_matrix.OffsetY + path.GetBounds().Y));
			(brush as PdfTilingBrush).Location = new PointF(x, y);
			(brush as PdfTilingBrush).Graphics.ColorSpace = ColorSpace;
		}
		else if (brush is PdfGradientBrush)
		{
			m_bCSInitialized = false;
			(brush as PdfGradientBrush).ColorSpace = ColorSpace;
		}
		if (brush is PdfSolidBrush { Color: { IsEmpty: not false } })
		{
			brush = null;
		}
		StateControl(pen, brush, null);
		BuildUpPath(path);
		DrawPath(pen, brush, path.FillMode, needClosing: false);
		if (flag)
		{
			StreamWriter.Write("EMC" + Environment.NewLine);
		}
		EndMarkContent();
	}

	public void DrawImage(PdfImage image, PointF point)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		DrawImage(image, point.X, point.Y);
	}

	public void DrawImage(PdfImage image, float x, float y)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		SizeF sizeF = image.PhysicalDimension;
		if (image is PdfBitmap)
		{
			sizeF = new PdfUnitConverter().ConvertFromPixels(image.PhysicalDimension, PdfGraphicsUnit.Point);
		}
		DrawImage(image, x, y, sizeF.Width, sizeF.Height);
	}

	public void DrawImage(PdfImage image, RectangleF rectangle)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		if (IsDirectPDF)
		{
			OnDrawPrimitive();
		}
		DrawImage(image, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
	}

	public void DrawImage(PdfImage image, PointF point, SizeF size)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		DrawImage(image, point.X, point.Y, size.Width, size.Height);
	}

	public void DrawImage(PdfImage image, float x, float y, float width, float height)
	{
		BeginMarkContent();
		bool flag = false;
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		if (ClientSize.Height < 0f)
		{
			y += ClientSize.Height;
		}
		if (!isImageOptimized)
		{
			image.Save();
			if (Page != null)
			{
				PdfDocumentBase pdfDocumentBase = ((Page is PdfPage) ? (Page as PdfPage).Document : (Page as PdfLoadedPage).Document);
				if (pdfDocumentBase != null && !pdfDocumentBase.ImageCollection.Contains(image))
				{
					pdfDocumentBase.ImageCollection.Add(image);
				}
			}
		}
		isImageOptimized = false;
		if (!m_artifactBMCAdded || image.PdfTag != null)
		{
			if (Layer != null && Page != null && Page is PdfPage)
			{
				if ((Page as PdfPage).Section.ParentDocument is PdfDocument)
				{
					PdfStructTreeRoot structTreeRoot = PdfCatalog.StructTreeRoot;
					if (structTreeRoot != null)
					{
						structTreeRoot.m_isImage = true;
						int num = 0;
						RectangleF bounds = new RectangleF(x, y, width, height);
						if (image.PdfTag != null && image.PdfTag is PdfStructureElement)
						{
							num = structTreeRoot.Add(image.PdfTag as PdfStructureElement, Page, bounds);
							string abbrevation = (image.PdfTag as PdfStructureElement).Abbrevation;
							StreamWriter.WriteTag(string.Format("/{0} <</E (" + abbrevation + ") /MCID {1} >>BDC", structTreeRoot.ConvertToEquivalentTag((image.PdfTag as PdfStructureElement).TagType), num));
							flag = true;
						}
						else if (image.PdfTag != null && image.PdfTag is PdfArtifact)
						{
							string arg = SetArtifact(Tag as PdfArtifact);
							StreamWriter.Write(string.Format("/Artifact << {0} >>BDC" + Environment.NewLine, arg));
							flag = true;
						}
						else if (IsTaggedPdf)
						{
							num = structTreeRoot.Add(new PdfStructureElement(PdfTagType.Figure), Page, bounds);
							m_streamWriter.WriteTag(string.Format("/{0} <</MCID {1} >>BDC", "Figure", num));
							flag = true;
						}
					}
				}
			}
			else
			{
				PdfStructTreeRoot structTreeRoot2 = PdfCatalog.StructTreeRoot;
				if (structTreeRoot2 != null)
				{
					structTreeRoot2.m_isImage = true;
					int num2 = 0;
					if (Tag != null)
					{
						image.PdfTag = Tag;
					}
					if (image.PdfTag != null && image.PdfTag is PdfArtifact)
					{
						string arg2 = SetArtifact(Tag as PdfArtifact);
						StreamWriter.Write(string.Format("/Artifact << {0} >>BDC" + Environment.NewLine, arg2));
						flag = true;
					}
					else if (!isTemplateGraphics)
					{
						num2 = structTreeRoot2.Add("Figure", "Image", RectangleF.Empty);
						StreamWriter.WriteTag(string.Format("/{0} <</MCID {1} >>BDC", "Figure", num2));
						flag = true;
					}
				}
			}
		}
		if (m_isTransparentBrush && !m_istransparencySet)
		{
			float num3 = 1f;
			PdfBlendMode blendMode = PdfBlendMode.Normal;
			SetTransparency(num3, num3, blendMode);
		}
		PdfGraphicsState state = Save();
		PdfTransformationMatrix pdfTransformationMatrix = new PdfTransformationMatrix();
		GetTranslateTransform(x, y + height, pdfTransformationMatrix);
		GetScaleTransform(width, height, pdfTransformationMatrix);
		m_streamWriter.ModifyCTM(pdfTransformationMatrix);
		PdfResources pdfResources = m_getResources();
		PdfName name = pdfResources.GetName(image);
		if (m_layer != null)
		{
			Page.SetResources(pdfResources);
		}
		m_streamWriter.ExecuteObject(name);
		Restore(state);
		if (flag)
		{
			m_streamWriter.WriteTag("EMC");
		}
		EndMarkContent();
		m_getResources().RequireProcSet("ImageB");
		m_getResources().RequireProcSet("ImageC");
		m_getResources().RequireProcSet("ImageI");
		m_getResources().RequireProcSet("Text");
	}

	internal void SetTransparencyGroup(PdfPageBase page)
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		pdfDictionary.SetName("CS", "DeviceRGB");
		pdfDictionary.SetBoolean("K", value: false);
		pdfDictionary.SetName("S", "Transparency");
		pdfDictionary.SetBoolean("I", value: false);
		page.Dictionary["Group"] = pdfDictionary;
	}

	public void DrawString(string s, PdfFont font, PdfBrush brush, PointF point)
	{
		DrawString(s, font, brush, point, null);
	}

	public void DrawString(string s, PdfFont font, PdfBrush brush, PointF point, PdfStringFormat format)
	{
		DrawString(s, font, brush, point.X, point.Y, format);
	}

	public void DrawString(string s, PdfFont font, PdfBrush brush, float x, float y)
	{
		DrawString(s, font, brush, x, y, null);
	}

	public void DrawString(string s, PdfFont font, PdfBrush brush, float x, float y, PdfStringFormat format)
	{
		DrawString(s, font, null, brush, x, y, format);
	}

	public void DrawString(string s, PdfFont font, PdfPen pen, PointF point)
	{
		DrawString(s, font, pen, point, null);
	}

	public void DrawString(string s, PdfFont font, PdfPen pen, PointF point, PdfStringFormat format)
	{
		DrawString(s, font, pen, point.X, point.Y, format);
	}

	public void DrawString(string s, PdfFont font, PdfPen pen, float x, float y)
	{
		DrawString(s, font, pen, x, y, null);
	}

	public void DrawString(string s, PdfFont font, PdfPen pen, float x, float y, PdfStringFormat format)
	{
		DrawString(s, font, pen, null, x, y, format);
	}

	public void DrawString(string s, PdfFont font, PdfPen pen, PdfBrush brush, PointF point)
	{
		DrawString(s, font, pen, brush, point, null);
	}

	public void DrawString(string s, PdfFont font, PdfPen pen, PdfBrush brush, PointF point, PdfStringFormat format)
	{
		DrawString(s, font, pen, brush, point.X, point.Y, format);
	}

	public void DrawString(string s, PdfFont font, PdfPen pen, PdfBrush brush, float x, float y, PdfStringFormat format)
	{
		RectangleF layoutRectangle = new RectangleF(x, y, 0f, 0f);
		DrawString(s, font, pen, brush, layoutRectangle, format);
	}

	public void DrawString(string s, PdfFont font, PdfPen pen, PdfBrush brush, float x, float y)
	{
		DrawString(s, font, pen, brush, x, y, null);
	}

	public void DrawString(string s, PdfFont font, PdfBrush brush, RectangleF layoutRectangle)
	{
		DrawString(s, font, brush, layoutRectangle, null);
	}

	public void DrawString(string s, PdfFont font, PdfBrush brush, RectangleF layoutRectangle, PdfStringFormat format)
	{
		DrawString(s, font, null, brush, layoutRectangle, format);
	}

	internal void DrawString(string s, PdfFont font, PdfBrush brush, RectangleF layoutRectangle, PdfStringFormat format, double maxRowFontSize, PdfFont maxPdfFont, PdfStringFormat maxPdfFormat)
	{
		DrawString(s, font, null, brush, layoutRectangle, format, maxRowFontSize, maxPdfFont, maxPdfFormat);
	}

	public void DrawString(string s, PdfFont font, PdfPen pen, RectangleF layoutRectangle)
	{
		DrawString(s, font, pen, layoutRectangle, null);
	}

	public void DrawString(string s, PdfFont font, PdfPen pen, RectangleF layoutRectangle, PdfStringFormat format)
	{
		DrawString(s, font, pen, null, layoutRectangle, format);
	}

	public void DrawString(string s, PdfFont font, PdfPen pen, PdfBrush brush, RectangleF layoutRectangle, PdfStringFormat format)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		if (!isStandardUnicode)
		{
			s = NormalizeText(font, s);
		}
		if (IsDirectPDF && format != null && format.TextDirection != PdfTextDirection.RightToLeft && PdfString.IsUnicode(s))
		{
			char[] array = s.ToCharArray();
			for (int i = 0; i < array.Length; i++)
			{
				if (IsRTLChar(array[i]))
				{
					format.TextDirection = PdfTextDirection.RightToLeft;
					format.isCustomRendering = true;
					break;
				}
			}
		}
		PdfStringLayouter pdfStringLayouter = new PdfStringLayouter();
		PdfStringLayoutResult pdfStringLayoutResult = null;
		pdfStringLayoutResult = ((LayoutResult != null) ? LayoutResult : pdfStringLayouter.Layout(s, font, format, layoutRectangle.Size));
		if (!pdfStringLayoutResult.Empty || IsTaggedPdf)
		{
			RectangleF rectangleF = CheckCorrectLayoutRectangle(pdfStringLayoutResult.ActualSize, layoutRectangle.X, layoutRectangle.Y, format);
			if (layoutRectangle.Width <= 0f)
			{
				layoutRectangle.X = rectangleF.X;
				layoutRectangle.Width = rectangleF.Width;
			}
			if (layoutRectangle.Height <= 0f)
			{
				layoutRectangle.Y = rectangleF.Y;
				layoutRectangle.Height = rectangleF.Height;
			}
			if (font.Name.ToLower().Contains("calibri") && m_isNormalRender && font.Style == PdfFontStyle.Regular && (format == null || format.LineAlignment != PdfVerticalAlignment.Bottom))
			{
				m_isUseFontSize = true;
			}
			if (ClientSize.Height < 0f)
			{
				layoutRectangle.Y += ClientSize.Height;
			}
			DrawStringLayoutResult(pdfStringLayoutResult, font, pen, brush, layoutRectangle, format);
			m_isEmfTextScaled = false;
			m_emfScalingFactor = SizeF.Empty;
		}
		m_getResources().RequireProcSet("Text");
		m_isNormalRender = true;
		m_stringLayoutResult = pdfStringLayoutResult;
		if (IsDirectPDF)
		{
			m_isUseFontSize = false;
		}
	}

	internal void DrawString(string s, PdfFont font, PdfPen pen, PdfBrush brush, RectangleF layoutRectangle, PdfStringFormat format, double maxRowFontSize, PdfFont maxPdfFont, PdfStringFormat maxPdfFormat)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		s = NormalizeText(font, s);
		PdfStringLayouter pdfStringLayouter = new PdfStringLayouter();
		PdfStringLayoutResult pdfStringLayoutResult = null;
		pdfStringLayoutResult = ((LayoutResult != null) ? LayoutResult : pdfStringLayouter.Layout(s, font, format, layoutRectangle.Size));
		if (!pdfStringLayoutResult.Empty)
		{
			RectangleF rectangleF = CheckCorrectLayoutRectangle(pdfStringLayoutResult.ActualSize, layoutRectangle.X, layoutRectangle.Y, format);
			if (layoutRectangle.Width <= 0f)
			{
				layoutRectangle.X = rectangleF.X;
				layoutRectangle.Width = rectangleF.Width;
			}
			if (layoutRectangle.Height <= 0f)
			{
				layoutRectangle.Y = rectangleF.Y;
				layoutRectangle.Height = rectangleF.Height;
			}
			if (font.Name.ToLower().Contains("calibri") && m_isNormalRender && font.Style == PdfFontStyle.Regular && (format == null || format.LineAlignment != PdfVerticalAlignment.Bottom))
			{
				m_isUseFontSize = true;
			}
			if (ClientSize.Height < 0f)
			{
				layoutRectangle.Y += ClientSize.Height;
			}
			DrawStringLayoutResult(pdfStringLayoutResult, font, pen, brush, layoutRectangle, format, maxRowFontSize, maxPdfFont, maxPdfFormat);
		}
		m_getResources().RequireProcSet("Text");
		m_isNormalRender = true;
		m_stringLayoutResult = pdfStringLayoutResult;
	}

	internal bool IsRTLChar(char input)
	{
		bool result = false;
		if (input >= '\u0590' && input <= '\u05ff')
		{
			result = true;
		}
		else if ((input >= '\u0600' && input <= 'ۿ') || (input >= 'ݐ' && input <= 'ݿ') || (input >= 'ࢠ' && input <= '\u08ff') || (input >= 'ﭐ' && input <= '\ufeff') || (input >= 126464 && input <= 126719))
		{
			result = true;
		}
		else if (input >= 67648 && input <= 67679)
		{
			result = true;
		}
		else if (input >= 66464 && input <= 66527)
		{
			result = true;
		}
		return result;
	}

	public void TranslateTransform(float offsetX, float offsetY)
	{
		PdfTransformationMatrix pdfTransformationMatrix = new PdfTransformationMatrix();
		GetTranslateTransform(offsetX, offsetY, pdfTransformationMatrix);
		m_streamWriter.ModifyCTM(pdfTransformationMatrix);
		Matrix.Multiply(pdfTransformationMatrix);
	}

	public void ScaleTransform(float scaleX, float scaleY)
	{
		PdfTransformationMatrix pdfTransformationMatrix = new PdfTransformationMatrix();
		GetScaleTransform(scaleX, scaleY, pdfTransformationMatrix);
		m_streamWriter.ModifyCTM(pdfTransformationMatrix);
		Matrix.Multiply(pdfTransformationMatrix);
	}

	public void RotateTransform(float angle)
	{
		PdfTransformationMatrix pdfTransformationMatrix = new PdfTransformationMatrix();
		GetRotateTransform(angle, pdfTransformationMatrix);
		m_streamWriter.ModifyCTM(pdfTransformationMatrix);
		Matrix.Multiply(pdfTransformationMatrix);
	}

	public void SkewTransform(float angleX, float angleY)
	{
		PdfTransformationMatrix pdfTransformationMatrix = new PdfTransformationMatrix();
		GetSkewTransform(angleX, angleY, pdfTransformationMatrix);
		m_streamWriter.ModifyCTM(pdfTransformationMatrix);
		Matrix.Multiply(pdfTransformationMatrix);
	}

	internal void MultiplyTransform(PdfTransformationMatrix matrix)
	{
		m_streamWriter.ModifyCTM(matrix);
	}

	public void DrawPdfTemplate(PdfTemplate template, PointF location)
	{
		if (template == null)
		{
			throw new ArgumentNullException("template");
		}
		DrawPdfTemplate(template, location, template.Size);
	}

	public void DrawPdfTemplate(PdfTemplate template, PointF location, SizeF size)
	{
		if (template.isContainPageRotation && template.m_content.ContainsKey("Matrix"))
		{
			PdfArray matrix = PdfCrossTable.Dereference(template.m_content["Matrix"]) as PdfArray;
			location = ModifyLocation(matrix, size, location);
		}
		lock (s_syncLockTemplate)
		{
			PdfCrossTable pdfCrossTable = null;
			BeginMarkContent();
			if (m_layer != null || m_documentLayer != null)
			{
				bool flag = false;
				if (Page is PdfLoadedPage)
				{
					pdfCrossTable = (Page as PdfLoadedPage).Document.CrossTable;
					flag = (Page as PdfLoadedPage).Document.EnableMemoryOptimization;
				}
				else if (Page is PdfPage)
				{
					pdfCrossTable = (Page as PdfPage).Section.ParentDocument.CrossTable;
					flag = (Page as PdfPage).Section.ParentDocument.EnableMemoryOptimization;
				}
				if (Page is PdfPage && (Page as PdfPage).isMergingPage)
				{
					pdfCrossTable.isTemplateMerging = true;
				}
				if ((template.ReadOnly && flag) || template.isLoadedPageTemplate)
				{
					template.CloneResources(pdfCrossTable);
				}
			}
			if (template == null)
			{
				throw new ArgumentNullException("template");
			}
			float num = ((template.Width > 0f) ? (size.Width / template.Width) : 1f);
			float num2 = ((template.Height > 0f) ? (size.Height / template.Height) : 1f);
			bool flag2 = num != 1f || num2 != 1f;
			if (!bScaleTranform)
			{
				flag2 = false;
			}
			if ((m_layer != null || m_documentLayer != null) && Page != null && Page.Dictionary.ContainsKey("CropBox") && Page.Dictionary.ContainsKey("MediaBox"))
			{
				PdfArray pdfArray = null;
				PdfArray pdfArray2 = null;
				pdfArray = ((!(Page.Dictionary["CropBox"] is PdfReferenceHolder)) ? (Page.Dictionary["CropBox"] as PdfArray) : ((Page.Dictionary["CropBox"] as PdfReferenceHolder).Object as PdfArray));
				pdfArray2 = ((!(Page.Dictionary["MediaBox"] is PdfReferenceHolder)) ? (Page.Dictionary["MediaBox"] as PdfArray) : ((Page.Dictionary["MediaBox"] as PdfReferenceHolder).Object as PdfArray));
				float floatValue = (pdfArray2[0] as PdfNumber).FloatValue;
				float floatValue2 = (pdfArray2[1] as PdfNumber).FloatValue;
				float floatValue3 = (pdfArray[0] as PdfNumber).FloatValue;
				float floatValue4 = (pdfArray[3] as PdfNumber).FloatValue;
				if ((floatValue3 > 0f && floatValue4 > 0f && floatValue < 0f && floatValue2 < 0f) || m_isWatermarkMediabox)
				{
					TranslateTransform(floatValue3, 0f - floatValue4);
					location.X = 0f - floatValue3;
					location.Y = floatValue4;
				}
			}
			PdfGraphicsState state = Save();
			PdfTransformationMatrix pdfTransformationMatrix = new PdfTransformationMatrix();
			if (m_layer != null || (m_documentLayer != null && Page != null))
			{
				bool flag3 = false;
				if (Page.Dictionary.ContainsKey("CropBox") && Page.Dictionary.ContainsKey("MediaBox"))
				{
					PdfArray pdfArray3 = null;
					PdfArray pdfArray4 = null;
					pdfArray3 = ((!(Page.Dictionary["CropBox"] is PdfReferenceHolder)) ? (Page.Dictionary["CropBox"] as PdfArray) : ((Page.Dictionary["CropBox"] as PdfReferenceHolder).Object as PdfArray));
					pdfArray4 = ((!(Page.Dictionary["MediaBox"] is PdfReferenceHolder)) ? (Page.Dictionary["MediaBox"] as PdfArray) : ((Page.Dictionary["MediaBox"] as PdfReferenceHolder).Object as PdfArray));
					if (pdfArray3 != null && pdfArray4 != null && pdfArray3.ToRectangle() == pdfArray4.ToRectangle())
					{
						flag3 = true;
					}
				}
				PdfArray pdfArray5 = null;
				if (Page.Dictionary.ContainsKey("MediaBox"))
				{
					pdfArray5 = ((!(Page.Dictionary["MediaBox"] is PdfReferenceHolder)) ? (Page.Dictionary["MediaBox"] as PdfArray) : ((Page.Dictionary["MediaBox"] as PdfReferenceHolder).Object as PdfArray));
					if (pdfArray5 != null && (pdfArray5[3] as PdfNumber).FloatValue == 0f)
					{
						flag3 = true;
					}
				}
				new RectangleF(location, size);
				if (template.m_origin.X > 0f && template.m_origin.Y > 0f && template.isCropBox && template.Width > size.Width && template.Height > size.Height)
				{
					flag2 = false;
					if (location.X < 0f && location.Y < 0f)
					{
						GetTranslateTransform(location.X, location.Y + template.m_origin.Y + template.Height, pdfTransformationMatrix);
					}
					else
					{
						GetTranslateTransform(location.X - template.m_origin.X, location.Y + template.Height, pdfTransformationMatrix);
					}
				}
				else if ((Page.Origin.X >= 0f && Page.Origin.Y >= 0f) || flag3)
				{
					if (template.m_origin.X == 0f && template.m_origin.Y > 0f)
					{
						GetTranslateTransform(location.X, location.Y + size.Height + template.m_origin.Y, pdfTransformationMatrix);
					}
					else
					{
						GetTranslateTransform(location.X, location.Y + size.Height, pdfTransformationMatrix);
					}
				}
				else if (Page.Origin.X != 0f || Page.Origin.Y != 0f)
				{
					GetTranslateTransform(location.X, location.Y + size.Height, pdfTransformationMatrix);
				}
				else
				{
					GetTranslateTransform(location.X, location.Y + 0f, pdfTransformationMatrix);
				}
			}
			else
			{
				GetTranslateTransform(location.X, location.Y + size.Height, pdfTransformationMatrix);
			}
			PdfArray pdfArray6 = PdfCrossTable.Dereference(template.m_content["BBox"]) as PdfArray;
			if (!template.m_content.ContainsKey("Matrix") && template.m_isSignatureAppearance && !template.NeedScaling && pdfArray6 != null && (pdfArray6[0] as PdfNumber).FloatValue > 0f && (pdfArray6[1] as PdfNumber).FloatValue > 0f)
			{
				float[] array = new float[6]
				{
					1f,
					0f,
					0f,
					1f,
					0f - (pdfArray6[0] as PdfNumber).FloatValue,
					0f - (pdfArray6[1] as PdfNumber).FloatValue
				};
				template.m_content["Matrix"] = new PdfArray(array);
			}
			if (flag2)
			{
				if (template.IsAnnotationTemplate && template.NeedScaling)
				{
					bool flag4 = false;
					if (template.m_content != null && template.m_content.ContainsKey("Matrix") && template.m_content.ContainsKey("BBox"))
					{
						PdfArray pdfArray7 = PdfCrossTable.Dereference(template.m_content["Matrix"]) as PdfArray;
						PdfArray pdfArray8 = PdfCrossTable.Dereference(template.m_content["BBox"]) as PdfArray;
						if (pdfArray7 != null && pdfArray8 != null && pdfArray7.Count > 5 && pdfArray8.Count > 3)
						{
							float num3 = 0f - (pdfArray7[1] as PdfNumber).FloatValue;
							float floatValue5 = (pdfArray7[2] as PdfNumber).FloatValue;
							float floatValue6 = (pdfArray7[4] as PdfNumber).FloatValue;
							float floatValue7 = (pdfArray7[5] as PdfNumber).FloatValue;
							num3 = (float)Math.Round(num3, 2);
							floatValue5 = (float)Math.Round(floatValue5, 2);
							float num4 = (float)Math.Round(num, 2);
							float num5 = (float)Math.Round(num2, 2);
							if (num4 == num3 && num5 == floatValue5 && (pdfArray8[2] as PdfNumber).FloatValue == template.Size.Width && (pdfArray8[3] as PdfNumber).FloatValue == template.Size.Height)
							{
								pdfTransformationMatrix = new PdfTransformationMatrix();
								GetTranslateTransform(location.X - floatValue6, location.Y + floatValue7, pdfTransformationMatrix);
								GetScaleTransform(1f, 1f, pdfTransformationMatrix);
								flag4 = true;
							}
						}
					}
					if (!flag4)
					{
						GetScaleTransform(num2, num, pdfTransformationMatrix);
					}
				}
				else
				{
					GetScaleTransform(num, num2, pdfTransformationMatrix);
				}
			}
			bool flag5 = false;
			if ((!m_artifactBMCAdded || template.PdfTag != null) && ((template.Graphics != null && template.Graphics.Tag != null) || IsTaggedPdf))
			{
				PdfStructTreeRoot structTreeRoot = PdfCatalog.StructTreeRoot;
				RectangleF bounds = new RectangleF(location.X, location.Y, size.Width, size.Height);
				if (structTreeRoot != null && template.Graphics.Tag is PdfStructureElement)
				{
					int num6 = structTreeRoot.Add(template.Graphics.Tag as PdfStructureElement, Page, bounds);
					string abbrevation = (template.Graphics.Tag as PdfStructureElement).Abbrevation;
					StreamWriter.WriteTag(string.Format("/{0} <</E (" + abbrevation + ") /MCID {1} >>BDC", structTreeRoot.ConvertToEquivalentTag((template.Graphics.Tag as PdfStructureElement).TagType), num6));
				}
				else
				{
					if (!Page.Dictionary.ContainsKey("Tabs"))
					{
						Page.Dictionary["Tabs"] = new PdfName("S");
					}
					if (structTreeRoot != null && template.Graphics.Tag is PdfArtifact)
					{
						string arg = SetArtifact(template.Graphics.Tag as PdfArtifact);
						StreamWriter.Write(string.Format("/Artifact << {0} >>BDC" + Environment.NewLine, arg));
					}
					else
					{
						int num7 = structTreeRoot.Add(new PdfStructureElement(PdfTagType.Figure), Page, bounds);
						StreamWriter.WriteTag(string.Format("/{0} <</MCID {1} >>BDC", "Figure", num7));
					}
				}
				flag5 = true;
			}
			m_streamWriter.ModifyCTM(pdfTransformationMatrix);
			PdfResources pdfResources = m_getResources();
			pdfResources.OriginalFontName = null;
			PdfName name = pdfResources.GetName(template);
			m_streamWriter.ExecuteObject(name);
			if (flag5)
			{
				StreamWriter.Write("EMC" + Environment.NewLine);
			}
			EndMarkContent();
			Restore(state);
			PdfGraphics graphics = template.Graphics;
			if (graphics != null)
			{
				foreach (PdfAutomaticFieldInfo automaticField in graphics.AutomaticFields)
				{
					PointF location2 = new PointF(automaticField.Location.X + location.X, automaticField.Location.Y + location.Y);
					float scalingX = ((template.Size.Width == 0f) ? 0f : (size.Width / template.Size.Width));
					float scalingY = ((template.Size.Height == 0f) ? 0f : (size.Height / template.Size.Height));
					AutomaticFields.Add(new PdfAutomaticFieldInfo(automaticField.Field, location2, scalingX, scalingY));
					Page.Dictionary.Modify();
				}
			}
			m_getResources().RequireProcSet("ImageB");
			m_getResources().RequireProcSet("ImageC");
			m_getResources().RequireProcSet("ImageI");
			m_getResources().RequireProcSet("Text");
			if (Page != null && template.isLoadedPageTemplate)
			{
				PdfResources pdfResources2 = m_getResources();
				if (Page is PdfPage)
				{
					Page.DestinationDocument = (Page as PdfPage).Document;
				}
				else if (Page is PdfLoadedPage)
				{
					Page.DestinationDocument = (Page as PdfLoadedPage).Document;
				}
				Page.templateResource = true;
				Page.m_xobjectReferenceCollection = new List<PdfReference>();
				Page.repeatedReferenceCollection = new List<PdfReference>();
				Page.RemoveIdenticalResources(pdfResources2, Page);
				Page.m_xobjectReferenceCollection.Clear();
				Page.m_xobjectReferenceCollection = null;
				Page.repeatedReferenceCollection.Clear();
				Page.repeatedReferenceCollection = null;
				if (Page.isSkipColorSpace && Page.DestinationDocument != null && Page.DestinationDocument.m_resourceCollection != null)
				{
					Page.DestinationDocument.m_resourceCollection.Clear();
					Page.DestinationDocument.m_resourceCollection = null;
				}
				Page.Dictionary["Resources"] = pdfResources2;
				Page.SetResources(pdfResources2);
				Page.templateResource = false;
			}
			else if (Page != null && template.isCustomStamp)
			{
				PdfResources pdfResources3 = m_getResources();
				Page.Dictionary["Resources"] = pdfResources3;
				Page.SetResources(pdfResources3);
			}
		}
	}

	public void Flush()
	{
		if (m_bStateSaved)
		{
			m_streamWriter.RestoreGraphicsState();
			m_bStateSaved = false;
		}
	}

	public PdfGraphicsState Save()
	{
		PdfGraphicsState pdfGraphicsState = new PdfGraphicsState(this, Matrix.Clone());
		pdfGraphicsState.Brush = m_currentBrush;
		pdfGraphicsState.Pen = m_currentPen;
		pdfGraphicsState.Font = m_currentFont;
		pdfGraphicsState.ColorSpace = m_currentColorSpace;
		pdfGraphicsState.CharacterSpacing = m_previousCharacterSpacing;
		pdfGraphicsState.WordSpacing = m_previousWordSpacing;
		pdfGraphicsState.TextScaling = m_previousTextScaling;
		pdfGraphicsState.TextRenderingMode = m_previousTextRenderingMode;
		m_graphicsState.Push(pdfGraphicsState);
		if (m_bStateSaved)
		{
			m_streamWriter.RestoreGraphicsState();
			m_bStateSaved = false;
		}
		m_streamWriter.SaveGraphicsState();
		return pdfGraphicsState;
	}

	public void Restore()
	{
		if (m_graphicsState.Count > 0)
		{
			DoRestoreState();
		}
	}

	public void Restore(PdfGraphicsState state)
	{
		if (state == null)
		{
			throw new ArgumentNullException("state");
		}
		if (state.Graphics != this)
		{
			throw new ArgumentException("The GraphicsState belongs to another Graphics object.", "state");
		}
		if (m_graphicsState.Contains(state))
		{
			while (m_graphicsState.Count != 0 && DoRestoreState() != state)
			{
			}
		}
	}

	public void SetClip(RectangleF rectangle)
	{
		SetClip(rectangle, PdfFillMode.Winding);
	}

	public void SetClip(RectangleF rectangle, PdfFillMode mode)
	{
		m_streamWriter.AppendRectangle(rectangle);
		m_streamWriter.ClipPath(mode == PdfFillMode.Alternate);
	}

	public void SetClip(PdfPath path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		SetClip(path, path.FillMode);
	}

	public void SetClip(PdfPath path, PdfFillMode mode)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		BuildUpPath(path);
		m_streamWriter.ClipPath(mode == PdfFillMode.Alternate);
	}

	public void SetTransparency(float alpha)
	{
		m_istransparencySet = true;
		SetTransparency(alpha, alpha, PdfBlendMode.Normal);
	}

	public void SetTransparency(float alphaPen, float alphaBrush)
	{
		SetTransparency(alphaPen, alphaBrush, PdfBlendMode.Normal);
	}

	public void SetTransparency(float alphaPen, float alphaBrush, PdfBlendMode blendMode)
	{
		if (m_trasparencies == null)
		{
			m_trasparencies = new Dictionary<TransparencyData, PdfTransparency>();
		}
		PdfTransparency pdfTransparency = null;
		TransparencyData key = new TransparencyData(alphaPen, alphaBrush, blendMode);
		if (m_trasparencies.ContainsKey(key))
		{
			pdfTransparency = m_trasparencies[key];
		}
		if (pdfTransparency == null)
		{
			pdfTransparency = new PdfTransparency(alphaPen, alphaBrush, blendMode);
			m_trasparencies[key] = pdfTransparency;
		}
		PdfName name = m_getResources().GetName(pdfTransparency);
		StreamWriter.SetGraphicsState(name);
	}

	internal void SetTextRenderingMode(TextRenderingMode mode)
	{
		m_textRenderingMode = mode;
		m_isTextRenderingSet = true;
	}

	internal static string NormalizeText(PdfFont font, string text)
	{
		PdfTrueTypeFont pdfTrueTypeFont = font as PdfTrueTypeFont;
		if (font is PdfStandardFont || (pdfTrueTypeFont != null && !pdfTrueTypeFont.Unicode))
		{
			text = ((!(font is PdfStandardFont { fontEncoding: not null } pdfStandardFont)) ? PdfStandardFont.Convert(text) : PdfStandardFont.Convert(text, pdfStandardFont.fontEncoding));
		}
		return text;
	}

	internal void TranslateTransform(float offsetX, float offsetY, bool value)
	{
		PdfTransformationMatrix pdfTransformationMatrix = new PdfTransformationMatrix(value);
		GetTranslateTransform(offsetX, offsetY, pdfTransformationMatrix);
		m_streamWriter.ModifyCTM(pdfTransformationMatrix);
		Matrix.Multiply(pdfTransformationMatrix);
	}

	private void Initialize()
	{
		m_bStateSaved = false;
		m_currentPen = null;
		m_currentBrush = null;
		m_currentFont = null;
		if (PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_X1A2001)
		{
			m_currentColorSpace = PdfColorSpace.CMYK;
		}
		else
		{
			m_currentColorSpace = PdfColorSpace.RGB;
		}
		m_bCSInitialized = false;
		m_matrix = null;
		m_previousTextRenderingMode = (TextRenderingMode)(-1);
		m_previousCharacterSpacing = -1f;
		m_previousWordSpacing = -1f;
		m_previousTextScaling = -100f;
		m_trasparencies = null;
		m_currentStringFormat = null;
		m_clipBounds = new RectangleF(PointF.Empty, Size);
		m_graphicsState = new Stack<PdfGraphicsState>();
		m_getResources().RequireProcSet("PDF");
	}

	internal void SetLayer(PdfPageLayer layer)
	{
		m_layer = layer;
		if (layer.Page is PdfPage pdfPage)
		{
			pdfPage.BeginSave += PageSave;
		}
		else
		{
			(layer.Page as PdfLoadedPage).BeginSave += PageSave;
		}
	}

	internal void SetLayer(PdfLayer layer)
	{
		m_documentLayer = layer;
		if (layer.Page is PdfPage pdfPage)
		{
			pdfPage.BeginSave += PageSave;
		}
		else
		{
			(layer.Page as PdfLoadedPage).BeginSave += PageSave;
		}
	}

	internal void EndMarkContent()
	{
		if (m_documentLayer == null)
		{
			return;
		}
		if (m_documentLayer.m_isEndState && m_documentLayer.m_parentLayer.Count != 0)
		{
			for (int i = 0; i < m_documentLayer.m_parentLayer.Count; i++)
			{
				StreamWriter.Write("EMC" + Environment.NewLine);
			}
		}
		if (m_documentLayer.m_isEndState)
		{
			StreamWriter.Write("EMC" + Environment.NewLine);
		}
	}

	private void BeginMarkContent()
	{
		if (m_documentLayer != null)
		{
			m_documentLayer.BeginLayer(this);
		}
	}

	private void PageSave(object sender, EventArgs e)
	{
		if (m_automaticFields == null)
		{
			return;
		}
		foreach (PdfAutomaticFieldInfo automaticField in m_automaticFields)
		{
			automaticField.Field.PerformDraw(this, automaticField.Location, automaticField.ScalingX, automaticField.ScalingY);
		}
	}

	internal static float UpdateY(float y)
	{
		return 0f - y;
	}

	internal void PutComment(string comment)
	{
		m_streamWriter.WriteComment(comment);
	}

	internal void Reset(SizeF size)
	{
		m_canvasSize = size;
		m_streamWriter.Clear();
		Initialize();
		InitializeCoordinates();
	}

	private PdfGraphicsState DoRestoreState()
	{
		PdfGraphicsState pdfGraphicsState = m_graphicsState.Pop();
		m_matrix = pdfGraphicsState.Matrix;
		m_currentBrush = pdfGraphicsState.Brush;
		m_currentPen = pdfGraphicsState.Pen;
		m_currentFont = pdfGraphicsState.Font;
		m_currentColorSpace = pdfGraphicsState.ColorSpace;
		m_previousCharacterSpacing = pdfGraphicsState.CharacterSpacing;
		m_previousWordSpacing = pdfGraphicsState.WordSpacing;
		m_previousTextScaling = pdfGraphicsState.TextScaling;
		m_previousTextRenderingMode = pdfGraphicsState.TextRenderingMode;
		m_streamWriter.RestoreGraphicsState();
		return pdfGraphicsState;
	}

	private void StateControl(PdfPen pen, PdfBrush brush, PdfFont font)
	{
		StateControl(pen, brush, font, null);
	}

	private void StateControl(PdfPen pen, PdfBrush brush, PdfFont font, PdfStringFormat format)
	{
		if (((pen != null && pen.Color.A == 0) || (brush != null && brush is PdfSolidBrush && (brush as PdfSolidBrush).Color.A == 0)) && Layer != null && !Layer.Page.Dictionary.ContainsKey("Group"))
		{
			SetTransparencyGroup(Layer.Page);
		}
		if (brush is PdfGradientBrush)
		{
			m_bCSInitialized = false;
			(brush as PdfGradientBrush).ColorSpace = ColorSpace;
		}
		if (brush is PdfTilingBrush)
		{
			m_bCSInitialized = false;
			(brush as PdfTilingBrush).Graphics.ColorSpace = ColorSpace;
		}
		bool flag = false;
		if (brush != null)
		{
			if (brush is PdfSolidBrush pdfSolidBrush)
			{
				if (pdfSolidBrush.Colorspaces != null)
				{
					ColorSpaceControl(pdfSolidBrush.Colorspaces.ColorSpace);
				}
				else
				{
					if (m_layer != null)
					{
						if (m_layer.Page is PdfPage && (m_layer.Page as PdfPage).Section.ParentDocument.GetType().Name != "PdfLoadedDocument")
						{
							if (!colorSpaceChanged)
							{
								lastDocumentCS = (m_layer.Page as PdfPage).Document.ColorSpace;
								lastGraphicsCS = (m_layer.Page as PdfPage).Graphics.ColorSpace;
								if ((m_layer.Page as PdfPage).Document.ColorSpace == (m_layer.Page as PdfPage).Graphics.ColorSpace)
								{
									ColorSpace = (m_layer.Page as PdfPage).Document.ColorSpace;
									m_currentColorSpace = (m_layer.Page as PdfPage).Document.ColorSpace;
								}
								else if ((m_layer.Page as PdfPage).Document.ColorSpace != (m_layer.Page as PdfPage).Graphics.ColorSpace)
								{
									ColorSpace = (m_layer.Page as PdfPage).Graphics.ColorSpace;
									m_currentColorSpace = (m_layer.Page as PdfPage).Graphics.ColorSpace;
								}
								colorSpaceChanged = true;
							}
							else if ((m_layer.Page as PdfPage).Document.ColorSpace != lastDocumentCS)
							{
								ColorSpace = (m_layer.Page as PdfPage).Document.ColorSpace;
								m_currentColorSpace = (m_layer.Page as PdfPage).Document.ColorSpace;
								lastDocumentCS = (m_layer.Page as PdfPage).Document.ColorSpace;
							}
							else if ((m_layer.Page as PdfPage).Graphics.ColorSpace != lastGraphicsCS)
							{
								ColorSpace = (m_layer.Page as PdfPage).Graphics.ColorSpace;
								m_currentColorSpace = (m_layer.Page as PdfPage).Graphics.ColorSpace;
								lastGraphicsCS = (m_layer.Page as PdfPage).Graphics.ColorSpace;
							}
						}
						else if (m_layer.Page is PdfLoadedPage)
						{
							ColorSpace = ((m_layer.Page as PdfLoadedPage).Document as PdfLoadedDocument).ColorSpace;
							m_currentColorSpace = ((m_layer.Page as PdfLoadedPage).Document as PdfLoadedDocument).ColorSpace;
						}
					}
					InitCurrentColorSpace(m_currentColorSpace);
				}
			}
			else
			{
				if (m_layer != null)
				{
					if (m_layer.Page is PdfPage && (m_layer.Page as PdfPage).Section.ParentDocument.GetType().Name != "PdfLoadedDocument")
					{
						ColorSpace = (m_layer.Page as PdfPage).Document.ColorSpace;
						m_currentColorSpace = (m_layer.Page as PdfPage).Document.ColorSpace;
					}
					else if (m_layer.Page is PdfLoadedPage)
					{
						ColorSpace = ((m_layer.Page as PdfLoadedPage).Document as PdfLoadedDocument).ColorSpace;
						m_currentColorSpace = ((m_layer.Page as PdfLoadedPage).Document as PdfLoadedDocument).ColorSpace;
					}
				}
				InitCurrentColorSpace(m_currentColorSpace);
			}
		}
		else if (pen != null)
		{
			if (pen != null)
			{
				if (pen.Colorspaces != null)
				{
					ColorSpaceControl(pen.Colorspaces.ColorSpace);
				}
				else
				{
					if (m_layer != null)
					{
						if (m_layer.Page is PdfPage && (m_layer.Page as PdfPage).Section.ParentDocument.GetType().Name != "PdfLoadedDocument")
						{
							ColorSpace = (m_layer.Page as PdfPage).Document.ColorSpace;
							m_currentColorSpace = (m_layer.Page as PdfPage).Document.ColorSpace;
						}
						else if (m_layer.Page is PdfLoadedPage)
						{
							ColorSpace = ((m_layer.Page as PdfLoadedPage).Document as PdfLoadedDocument).ColorSpace;
							m_currentColorSpace = ((m_layer.Page as PdfLoadedPage).Document as PdfLoadedDocument).ColorSpace;
						}
					}
					InitCurrentColorSpace(m_currentColorSpace);
				}
			}
			else
			{
				if (m_layer != null)
				{
					if (m_layer.Page is PdfPage && (m_layer.Page as PdfPage).Section.ParentDocument.GetType().Name != "PdfLoadedDocument")
					{
						ColorSpace = (m_layer.Page as PdfPage).Document.ColorSpace;
						m_currentColorSpace = (m_layer.Page as PdfPage).Document.ColorSpace;
					}
					else if (m_layer.Page is PdfLoadedPage)
					{
						ColorSpace = ((m_layer.Page as PdfLoadedPage).Document as PdfLoadedDocument).ColorSpace;
						m_currentColorSpace = ((m_layer.Page as PdfLoadedPage).Document as PdfLoadedDocument).ColorSpace;
					}
				}
				InitCurrentColorSpace(m_currentColorSpace);
			}
		}
		if (flag)
		{
			if (m_bStateSaved)
			{
				m_streamWriter.RestoreGraphicsState();
			}
			m_streamWriter.SaveGraphicsState();
			m_bStateSaved = true;
		}
		PenControl(pen, flag);
		BrushControl(brush, flag);
		FontControl(font, format, flag);
	}

	private void FontControl(PdfFont font, PdfStringFormat format, bool saveState)
	{
		if (font == null)
		{
			return;
		}
		PdfSubSuperScript pdfSubSuperScript = format?.SubSuperScript ?? PdfSubSuperScript.None;
		PdfSubSuperScript pdfSubSuperScript2 = ((m_currentStringFormat != null) ? m_currentStringFormat.SubSuperScript : PdfSubSuperScript.None);
		bool flag = false;
		PdfResources pdfResources = m_getResources();
		float num = font.Metrics.GetSize(format);
		if (saveState || font != m_currentFont || pdfSubSuperScript != pdfSubSuperScript2)
		{
			m_currentFont = font;
			m_currentStringFormat = format;
			if (m_isEmfTextScaled && font.Metrics.LineGap == 0)
			{
				float num2 = ((m_emfScalingFactor.Width > m_emfScalingFactor.Height) ? m_emfScalingFactor.Width : m_emfScalingFactor.Height);
				if (num > num2)
				{
					num /= num2;
				}
				else
				{
					m_isEmfTextScaled = false;
				}
			}
			else
			{
				m_isEmfTextScaled = false;
			}
			if (m_layer != null && Page is PdfLoadedPage)
			{
				if (!pdfResources.Items.ContainsKey(new PdfName("Font")))
				{
					PdfDictionary pdfDictionary = Page.Dictionary["Parent"] as PdfDictionary;
					if (pdfDictionary == null)
					{
						pdfDictionary = (Page.Dictionary["Parent"] as PdfReferenceHolder).Object as PdfDictionary;
					}
					while (pdfDictionary != null)
					{
						PdfDictionary pdfDictionary2 = new PdfDictionary();
						if (pdfDictionary.Items.ContainsKey(new PdfName("Resources")))
						{
							PdfDictionary pdfDictionary3 = pdfDictionary.Items[new PdfName("Resources")] as PdfDictionary;
							if (pdfDictionary3 == null)
							{
								pdfDictionary3 = (pdfDictionary.Items[new PdfName("Resources")] as PdfReferenceHolder).Object as PdfDictionary;
							}
							if (pdfDictionary3.Items.ContainsKey(new PdfName("Font")))
							{
								PdfDictionary pdfDictionary4 = pdfDictionary3.Items[new PdfName("Font")] as PdfDictionary;
								PdfName pdfName = new PdfName(Guid.NewGuid().ToString());
								PdfDictionary value = ((IPdfWrapper)font).Element as PdfDictionary;
								if (pdfDictionary4 == null)
								{
									pdfDictionary4 = new PdfDictionary();
									pdfDictionary4.Items.Add(pdfName, value);
									m_streamWriter.SetFont(font, pdfName, num);
									flag = true;
									PdfResources pdfResources2 = Page.Dictionary.Items[new PdfName("Resources")] as PdfResources;
									if (pdfDictionary3 != null)
									{
										pdfResources2.Items.Add(new PdfName("Font"), pdfDictionary4);
									}
								}
								else
								{
									pdfDictionary4.Items.Add(pdfName, value);
									m_streamWriter.SetFont(font, pdfName, num);
									Page.Dictionary.Remove(new PdfName("Resources"));
									flag = true;
								}
								break;
							}
							if (!pdfDictionary.Items.ContainsKey(new PdfName("Parent")))
							{
								PdfDictionary pdfDictionary5 = new PdfDictionary();
								PdfName name = pdfResources.GetName(font);
								PdfDictionary obj = ((IPdfWrapper)font).Element as PdfDictionary;
								pdfDictionary5.Items.Add(name, new PdfReferenceHolder(obj));
								m_streamWriter.SetFont(font, name, num);
								flag = true;
								PdfDictionary pdfDictionary6 = PdfCrossTable.Dereference(Page.Dictionary["Resources"]) as PdfDictionary;
								pdfResources?.SetProperty(new PdfName("Font"), pdfDictionary5);
								pdfDictionary6?.SetProperty(new PdfName("Font"), pdfDictionary5);
								break;
							}
							pdfDictionary2 = pdfDictionary.Items[new PdfName("Parent")] as PdfDictionary;
							if (pdfDictionary == null)
							{
								pdfDictionary2 = (pdfDictionary.Items[new PdfName("Parent")] as PdfReferenceHolder).Object as PdfDictionary;
							}
							pdfDictionary = pdfDictionary2;
						}
						else
						{
							if (pdfDictionary.Items.ContainsKey(new PdfName("Parent")))
							{
								pdfDictionary2 = pdfDictionary.Items[new PdfName("Parent")] as PdfDictionary;
							}
							if (pdfDictionary2 == null)
							{
								pdfDictionary2 = (pdfDictionary.Items[new PdfName("Parent")] as PdfReferenceHolder).Object as PdfDictionary;
							}
							pdfDictionary = pdfDictionary2;
							if (pdfDictionary != null && pdfDictionary.Items.Count == 0)
							{
								PdfName name2 = pdfResources.GetName(font);
								m_streamWriter.SetFont(font, name2, num);
								flag = true;
								break;
							}
						}
					}
				}
				else
				{
					PdfName name3 = pdfResources.GetName(font);
					m_streamWriter.SetFont(font, name3, num);
					flag = true;
				}
			}
			else
			{
				PdfName name4 = pdfResources.GetName(font);
				m_streamWriter.SetFont(font, name4, num);
				flag = true;
			}
		}
		if (!flag)
		{
			PdfName name5 = pdfResources.GetName(font);
			m_streamWriter.SetFont(font, name5, num);
		}
	}

	private void ColorSpaceControl(PdfColorSpaces colorspace)
	{
		if (colorspace != null)
		{
			PdfName name = m_getResources().GetName(colorspace);
			m_streamWriter.SetColorSpace(colorspace, name);
		}
	}

	private void BrushControl(PdfBrush brush, bool saveState)
	{
		if (brush == null)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		PdfBrush pdfBrush = brush.Clone();
		if (pdfBrush is PdfGradientBrush pdfGradientBrush)
		{
			PdfTransformationMatrix matrix = pdfGradientBrush.Matrix;
			PdfTransformationMatrix matrix2 = Matrix.Clone();
			if (matrix != null)
			{
				matrix.Multiply(matrix2);
				matrix2 = matrix;
			}
			pdfGradientBrush.Matrix = matrix2;
		}
		if (brush is PdfSolidBrush pdfSolidBrush)
		{
			if (pdfSolidBrush.Colorspaces != null)
			{
				if (pdfSolidBrush.Colorspaces is PdfCalRGBColor)
				{
					ColorSpace = PdfColorSpace.RGB;
				}
				else if (pdfSolidBrush.Colorspaces is PdfCalGrayColor)
				{
					ColorSpace = PdfColorSpace.GrayScale;
				}
				else if (pdfSolidBrush.Colorspaces is PdfICCColor)
				{
					flag = true;
					PdfICCColor pdfICCColor = pdfSolidBrush.Colorspaces as PdfICCColor;
					if (pdfICCColor.ColorSpaces.AlternateColorSpace != null)
					{
						if (pdfICCColor.ColorSpaces.AlternateColorSpace is PdfCalGrayColorSpace)
						{
							ColorSpace = PdfColorSpace.GrayScale;
						}
						else if (pdfICCColor.ColorSpaces.AlternateColorSpace is PdfCalRGBColorSpace)
						{
							ColorSpace = PdfColorSpace.RGB;
						}
						else if (pdfICCColor.ColorSpaces.AlternateColorSpace is PdfLabColorSpace)
						{
							ColorSpace = PdfColorSpace.RGB;
						}
						else if (pdfICCColor.ColorSpaces.AlternateColorSpace is PdfDeviceColorSpace)
						{
							switch ((pdfICCColor.ColorSpaces.AlternateColorSpace as PdfDeviceColorSpace).DeviceColorSpaceType.ToString())
							{
							case "RGB":
								ColorSpace = PdfColorSpace.RGB;
								break;
							case "GrayScale":
								ColorSpace = PdfColorSpace.GrayScale;
								break;
							case "CMYK":
								ColorSpace = PdfColorSpace.CMYK;
								break;
							}
						}
					}
					else
					{
						ColorSpace = PdfColorSpace.RGB;
					}
				}
				else if (pdfSolidBrush.Colorspaces is PdfSeparationColor)
				{
					flag = true;
					ColorSpace = PdfColorSpace.GrayScale;
				}
				else if (pdfSolidBrush.Colorspaces is PdfIndexedColor)
				{
					flag2 = true;
					ColorSpace = PdfColorSpace.GrayScale;
				}
				else if (pdfSolidBrush.Colorspaces is PdfLabColor)
				{
					ColorSpace = PdfColorSpace.RGB;
				}
				if (flag ? pdfBrush.MonitorChanges(m_currentBrush, m_streamWriter, m_getResources, saveState, ColorSpace, check: true, iccbased: true) : ((!flag2) ? pdfBrush.MonitorChanges(m_currentBrush, m_streamWriter, m_getResources, saveState, ColorSpace, check: true) : pdfBrush.MonitorChanges(m_currentBrush, m_streamWriter, m_getResources, saveState, ColorSpace, check: true, iccbased: true, indexed: true)))
				{
					m_currentBrush = pdfBrush;
				}
			}
			else if (pdfBrush.MonitorChanges(m_currentBrush, m_streamWriter, m_getResources, saveState, ColorSpace))
			{
				m_currentBrush = pdfBrush;
			}
		}
		else if (pdfBrush.MonitorChanges(m_currentBrush, m_streamWriter, m_getResources, saveState, ColorSpace))
		{
			m_currentBrush = pdfBrush;
		}
		brush = null;
	}

	private void InitCurrentColorSpace()
	{
		if (!m_bCSInitialized)
		{
			m_streamWriter.SetColorSpace("DeviceRGB", forStroking: true);
			m_streamWriter.SetColorSpace("DeviceRGB", forStroking: false);
			m_bCSInitialized = true;
		}
	}

	private void InitCurrentColorSpace(PdfColorSpace colorspace)
	{
		m_getResources();
		if (!m_bCSInitialized)
		{
			if (m_currentColorSpace != PdfColorSpace.GrayScale)
			{
				m_streamWriter.SetColorSpace("Device" + m_currentColorSpace, forStroking: true);
				m_streamWriter.SetColorSpace("Device" + m_currentColorSpace, forStroking: false);
				m_bCSInitialized = true;
			}
			else
			{
				m_streamWriter.SetColorSpace("DeviceGray", forStroking: true);
				m_streamWriter.SetColorSpace("DeviceGray", forStroking: false);
				m_bCSInitialized = true;
			}
		}
	}

	private void PenControl(PdfPen pen, bool saveState)
	{
		if (pen == null)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		if (pen != null && pen.Colorspaces != null)
		{
			if (pen.Colorspaces is PdfCalRGBColor)
			{
				ColorSpace = PdfColorSpace.RGB;
			}
			else if (pen.Colorspaces is PdfCalGrayColor)
			{
				ColorSpace = PdfColorSpace.GrayScale;
			}
			else if (pen.Colorspaces is PdfICCColor)
			{
				flag = true;
				PdfICCColor pdfICCColor = pen.Colorspaces as PdfICCColor;
				if (pdfICCColor.ColorSpaces.AlternateColorSpace != null)
				{
					if (pdfICCColor.ColorSpaces.AlternateColorSpace is PdfCalGrayColorSpace)
					{
						ColorSpace = PdfColorSpace.GrayScale;
					}
					else if (pdfICCColor.ColorSpaces.AlternateColorSpace is PdfCalRGBColorSpace)
					{
						ColorSpace = PdfColorSpace.RGB;
					}
					else if (pdfICCColor.ColorSpaces.AlternateColorSpace is PdfLabColorSpace)
					{
						ColorSpace = PdfColorSpace.RGB;
					}
					else if (pdfICCColor.ColorSpaces.AlternateColorSpace is PdfDeviceColorSpace)
					{
						switch ((pdfICCColor.ColorSpaces.AlternateColorSpace as PdfDeviceColorSpace).DeviceColorSpaceType.ToString())
						{
						case "RGB":
							ColorSpace = PdfColorSpace.RGB;
							break;
						case "GrayScale":
							ColorSpace = PdfColorSpace.GrayScale;
							break;
						case "CMYK":
							ColorSpace = PdfColorSpace.CMYK;
							break;
						}
					}
				}
				else
				{
					ColorSpace = PdfColorSpace.RGB;
				}
			}
			else if (pen.Colorspaces is PdfSeparationColor)
			{
				flag = true;
				ColorSpace = PdfColorSpace.GrayScale;
			}
			else if (pen.Colorspaces is PdfIndexedColor)
			{
				flag2 = true;
				ColorSpace = PdfColorSpace.GrayScale;
			}
		}
		if ((!flag && !flag2) ? pen.MonitorChanges(m_currentPen, m_streamWriter, m_getResources, saveState, ColorSpace, Matrix.Clone()) : ((!flag2) ? pen.MonitorChanges(m_currentPen, m_streamWriter, m_getResources, saveState, ColorSpace, Matrix.Clone(), iccBased: true) : pen.MonitorChanges(m_currentPen, m_streamWriter, m_getResources, saveState, ColorSpace, Matrix.Clone(), iccBased: true)))
		{
			m_currentPen = pen;
		}
	}

	private void CapControl(PdfPen pen, float x2, float y2, float x1, float y1)
	{
	}

	private void DrawPath(PdfPen pen, PdfBrush brush, bool needClosing)
	{
		DrawPath(pen, brush, PdfFillMode.Winding, needClosing);
	}

	private void DrawPath(PdfPen pen, PdfBrush brush, PdfFillMode fillMode, bool needClosing)
	{
		bool flag = pen != null;
		bool flag2 = brush != null;
		bool useEvenOddRule = fillMode == PdfFillMode.Alternate;
		if (flag && flag2)
		{
			if (needClosing)
			{
				StreamWriter.CloseFillStrokePath(useEvenOddRule);
			}
			else
			{
				StreamWriter.FillStrokePath(useEvenOddRule);
			}
			return;
		}
		if (!flag && !flag2)
		{
			StreamWriter.EndPath();
			return;
		}
		if (flag)
		{
			if (needClosing)
			{
				StreamWriter.CloseStrokePath();
			}
			else
			{
				StreamWriter.StrokePath();
			}
			return;
		}
		if (flag2)
		{
			if (needClosing)
			{
				StreamWriter.CloseFillPath(useEvenOddRule);
			}
			else
			{
				StreamWriter.FillPath(useEvenOddRule);
			}
			return;
		}
		throw new PdfException("Internal CLR error.");
	}

	internal static List<float[]> GetBezierArcPoints(float x1, float y1, float x2, float y2, float startAng, float extent)
	{
		if (x1 > x2)
		{
			float num = x1;
			x1 = x2;
			x2 = num;
		}
		if (y2 > y1)
		{
			float num2 = y1;
			y1 = y2;
			y2 = num2;
		}
		float num3;
		int num4;
		if (Math.Abs(extent) <= 90f)
		{
			num3 = extent;
			num4 = 1;
		}
		else
		{
			num4 = (int)Math.Ceiling(Math.Abs(extent) / 90f);
			num3 = extent / (float)num4;
		}
		float num5 = (x1 + x2) / 2f;
		float num6 = (y1 + y2) / 2f;
		float num7 = (x2 - x1) / 2f;
		float num8 = (y2 - y1) / 2f;
		float num9 = (float)((double)num3 * Math.PI / 360.0);
		float num10 = (float)Math.Abs(1.3333333333333333 * (1.0 - Math.Cos(num9)) / Math.Sin(num9));
		List<float[]> list = new List<float[]>();
		for (int i = 0; i < num4; i++)
		{
			float num11 = (float)((double)(startAng + (float)i * num3) * Math.PI / 180.0);
			float num12 = (float)((double)(startAng + (float)(i + 1) * num3) * Math.PI / 180.0);
			float num13 = (float)Math.Cos(num11);
			float num14 = (float)Math.Cos(num12);
			float num15 = (float)Math.Sin(num11);
			float num16 = (float)Math.Sin(num12);
			if (num3 > 0f)
			{
				list.Add(new float[8]
				{
					num5 + num7 * num13,
					num6 - num8 * num15,
					num5 + num7 * (num13 - num10 * num15),
					num6 - num8 * (num15 + num10 * num13),
					num5 + num7 * (num14 + num10 * num16),
					num6 - num8 * (num16 - num10 * num14),
					num5 + num7 * num14,
					num6 - num8 * num16
				});
			}
			else
			{
				list.Add(new float[8]
				{
					num5 + num7 * num13,
					num6 - num8 * num15,
					num5 + num7 * (num13 + num10 * num15),
					num6 - num8 * (num15 - num10 * num13),
					num5 + num7 * (num14 - num10 * num16),
					num6 - num8 * (num16 + num10 * num14),
					num5 + num7 * num14,
					num6 - num8 * num16
				});
			}
		}
		return list;
	}

	private void ConstructArcPath(float x1, float y1, float x2, float y2, float startAng, float sweepAngle)
	{
		List<float[]> bezierArcPoints = GetBezierArcPoints(x1, y1, x2, y2, startAng, sweepAngle);
		if (bezierArcPoints.Count != 0)
		{
			float[] array = bezierArcPoints[0];
			m_streamWriter.BeginPath(array[0], array[1]);
			for (int i = 0; i < bezierArcPoints.Count; i++)
			{
				array = bezierArcPoints[i];
				m_streamWriter.AppendBezierSegment(array[2], array[3], array[4], array[5], array[6], array[7]);
			}
		}
	}

	private void BuildUpPath(PdfPath path)
	{
		PointF[] pathPoints = path.PathPoints;
		byte[] pathTypes = path.PathTypes;
		BuildUpPath(pathPoints, pathTypes);
	}

	private void GetBezierPoints(PointF[] points, byte[] types, ref int i, out PointF p2, out PointF p3)
	{
		i++;
		if ((types[i] & 0xF) == 3)
		{
			p2 = points[i];
			i++;
			if ((types[i] & 0xF) == 3)
			{
				p3 = points[i];
				return;
			}
			throw new ArgumentException("Malforming path.");
		}
		throw new ArgumentException("Malforming path.");
	}

	private void BuildUpPath(PointF[] points, byte[] types)
	{
		int i = 0;
		for (int num = points.Length; i < num; i++)
		{
			byte b = types[i];
			PointF pointF = points[i];
			switch ((PathPointType)(b & 0xF))
			{
			case PathPointType.Start:
				m_streamWriter.BeginPath(pointF);
				break;
			case PathPointType.Bezier3:
			{
				GetBezierPoints(points, types, ref i, out var p, out var p2);
				m_streamWriter.AppendBezierSegment(pointF, p, p2);
				break;
			}
			case PathPointType.Line:
				m_streamWriter.AppendLineSegment(pointF);
				break;
			default:
				throw new ArithmeticException("Incorrect path formation.");
			}
			b = types[i];
			CheckFlags(b);
		}
	}

	private void CheckFlags(byte type)
	{
		if ((type & 0x80) == 128)
		{
			m_streamWriter.ClosePath();
		}
	}

	private TextRenderingMode GetTextRenderingMode(PdfPen pen, PdfBrush brush, PdfStringFormat format)
	{
		TextRenderingMode textRenderingMode = TextRenderingMode.None;
		if (m_isTextRenderingSet)
		{
			textRenderingMode = m_textRenderingMode;
			m_isTextRenderingSet = false;
		}
		else
		{
			if (pen != null && brush != null)
			{
				textRenderingMode = TextRenderingMode.FillStroke;
			}
			else if (pen != null)
			{
				textRenderingMode = TextRenderingMode.Stroke;
			}
			else if (brush != null)
			{
				textRenderingMode = TextRenderingMode.Fill;
			}
			if (format != null && format.ClipPath)
			{
				textRenderingMode |= TextRenderingMode.ClipFlag;
			}
		}
		return textRenderingMode;
	}

	internal void ClipTranslateMargins(float x, float y, float left, float top, float right, float bottom)
	{
		RectangleF rect = (m_clipBounds = new RectangleF(left, top, Size.Width - left - right, Size.Height - top - bottom));
		m_streamWriter.WriteComment("Clip margins.");
		m_streamWriter.AppendRectangle(rect);
		m_streamWriter.ClosePath();
		m_streamWriter.ClipPath(useEvenOddRule: false);
		m_streamWriter.WriteComment("Translate co-ordinate system.");
		TranslateTransform(x, y);
	}

	internal void ClipTranslateMargins(RectangleF clipBounds)
	{
		m_clipBounds = clipBounds;
		m_streamWriter.WriteComment("Clip margins.");
		m_streamWriter.AppendRectangle(clipBounds);
		m_streamWriter.ClosePath();
		m_streamWriter.ClipPath(useEvenOddRule: false);
		m_streamWriter.WriteComment("Translate co-ordinate system.");
		TranslateTransform(clipBounds.X, clipBounds.Y);
	}

	internal void InitializeCoordinates()
	{
		m_streamWriter.WriteComment("Change co-ordinate system to left/top.");
		if (MediaBoxUpperRightBound == 0f - Size.Height)
		{
			return;
		}
		if (m_cropBox == null)
		{
			if (MediaBoxUpperRightBound == Size.Height || MediaBoxUpperRightBound == 0f)
			{
				TranslateTransform(0f, UpdateY(Size.Height));
			}
			else
			{
				TranslateTransform(0f, UpdateY(MediaBoxUpperRightBound));
			}
		}
		else
		{
			if (m_cropBox == null)
			{
				return;
			}
			if ((m_cropBox[0] as PdfNumber).FloatValue > 0f || (m_cropBox[1] as PdfNumber).FloatValue > 0f || Size.Width == (m_cropBox[2] as PdfNumber).FloatValue || Size.Height == (m_cropBox[3] as PdfNumber).FloatValue)
			{
				TranslateTransform((m_cropBox[0] as PdfNumber).FloatValue, UpdateY((m_cropBox[3] as PdfNumber).FloatValue));
				if (mBox != null && (m_cropBox[3] as PdfNumber).FloatValue == 0f && (m_cropBox[1] as PdfNumber).FloatValue == (mBox[3] as PdfNumber).FloatValue && (m_cropBox[2] as PdfNumber).FloatValue == (mBox[2] as PdfNumber).FloatValue)
				{
					TranslateTransform((m_cropBox[0] as PdfNumber).FloatValue, UpdateY((m_cropBox[1] as PdfNumber).FloatValue));
				}
			}
			else if (MediaBoxUpperRightBound == Size.Height || MediaBoxUpperRightBound == 0f)
			{
				TranslateTransform(0f, UpdateY(Size.Height));
			}
			else
			{
				TranslateTransform(0f, UpdateY(MediaBoxUpperRightBound));
			}
		}
	}

	internal void InitializeCoordinates(PdfPageBase page)
	{
		PointF empty = PointF.Empty;
		_ = page.Dictionary;
		bool flag = false;
		PdfArray pdfArray = null;
		bool flag2 = false;
		if (page.Dictionary.ContainsKey("CropBox") && page.Dictionary.ContainsKey("MediaBox"))
		{
			pdfArray = page.Dictionary["CropBox"] as PdfArray;
			PdfArray pdfArray2 = page.Dictionary["MediaBox"] as PdfArray;
			if (pdfArray.ToRectangle() == pdfArray2.ToRectangle())
			{
				flag = true;
			}
			if ((pdfArray[0] as PdfNumber).FloatValue > 0f && (pdfArray[3] as PdfNumber).FloatValue > 0f && (pdfArray2[0] as PdfNumber).FloatValue < 0f && (pdfArray2[1] as PdfNumber).FloatValue < 0f)
			{
				TranslateTransform((pdfArray[0] as PdfNumber).FloatValue, 0f - (pdfArray[3] as PdfNumber).FloatValue);
				empty.X = 0f - (pdfArray[0] as PdfNumber).FloatValue;
				empty.Y = (pdfArray[3] as PdfNumber).FloatValue;
				flag2 = true;
			}
		}
		else if (!page.Dictionary.ContainsKey("CropBox"))
		{
			flag = true;
		}
		float num = 0f;
		float num2 = 0f;
		float y = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float y2 = 0f;
		if (page.Dictionary.ContainsKey("MediaBox") && page.Dictionary.GetValue("MediaBox", "Parent") is PdfArray pdfArray3)
		{
			num = (pdfArray3[0] as PdfNumber).FloatValue;
			num2 = (pdfArray3[1] as PdfNumber).FloatValue;
			_ = (pdfArray3[2] as PdfNumber).FloatValue;
			y = (pdfArray3[3] as PdfNumber).FloatValue;
		}
		if (page.Dictionary.ContainsKey("CropBox") && page.Dictionary.GetValue("CropBox", "Parent") is PdfArray pdfArray4)
		{
			num3 = (pdfArray4[0] as PdfNumber).FloatValue;
			num4 = (pdfArray4[1] as PdfNumber).FloatValue;
			_ = (pdfArray4[2] as PdfNumber).FloatValue;
			y2 = (pdfArray4[3] as PdfNumber).FloatValue;
		}
		if (flag || ((num != 0f || num2 != 0f || num3 != 0f || num4 != 0f) && !flag2))
		{
			m_streamWriter.WriteComment("Change co-ordinate system to left/top.");
			if (num != 0f || num2 != 0f || num3 != 0f || num4 != 0f)
			{
				if (num3 != 0f || num4 != 0f)
				{
					TranslateTransform(num3, UpdateY(y2));
				}
				else if (Size.Height == MediaBoxUpperRightBound || MediaBoxUpperRightBound == 0f)
				{
					TranslateTransform(num, UpdateY(Size.Height));
				}
				else
				{
					TranslateTransform(num, UpdateY(y));
				}
			}
			else if (m_cropBox == null)
			{
				if (page.Origin.Y < MediaBoxUpperRightBound || MediaBoxUpperRightBound == 0f)
				{
					TranslateTransform(0f, UpdateY(Size.Height));
				}
				else
				{
					TranslateTransform(0f, UpdateY(MediaBoxUpperRightBound));
				}
			}
		}
		else
		{
			PdfTransformationMatrix input = new PdfTransformationMatrix();
			GetTranslateTransform(empty.X, empty.Y + 0f, input);
		}
	}

	private void FlipHorizontal()
	{
		PdfTransformationMatrix pdfTransformationMatrix = new PdfTransformationMatrix();
		pdfTransformationMatrix.Translate(0f, Size.Height);
		pdfTransformationMatrix.Scale(1f, -1f);
		m_streamWriter.ModifyCTM(pdfTransformationMatrix);
	}

	private void FlipVertical()
	{
		PdfTransformationMatrix pdfTransformationMatrix = new PdfTransformationMatrix();
		pdfTransformationMatrix.Translate(Size.Width, 0f);
		pdfTransformationMatrix.Scale(-1f, 1f);
		m_streamWriter.ModifyCTM(pdfTransformationMatrix);
	}

	private PdfTransformationMatrix GetTranslateTransform(float x, float y, PdfTransformationMatrix input)
	{
		if (input == null)
		{
			input = new PdfTransformationMatrix();
		}
		input.Translate(x, UpdateY(y));
		return input;
	}

	private PdfTransformationMatrix GetScaleTransform(float x, float y, PdfTransformationMatrix input)
	{
		if (input == null)
		{
			input = new PdfTransformationMatrix();
		}
		input.Scale(x, y);
		return input;
	}

	private PdfTransformationMatrix GetRotateTransform(float angle, PdfTransformationMatrix input)
	{
		if (input == null)
		{
			input = new PdfTransformationMatrix();
		}
		input.Rotate(UpdateY(angle));
		return input;
	}

	private PdfTransformationMatrix GetSkewTransform(float angleX, float angleY, PdfTransformationMatrix input)
	{
		if (input == null)
		{
			input = new PdfTransformationMatrix();
		}
		input.Skew(UpdateY(angleX), UpdateY(angleY));
		return input;
	}

	private void DrawCjkString(LineInfo lineInfo, RectangleF layoutRectangle, PdfFont font, PdfStringFormat format)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		JustifyLine(lineInfo, layoutRectangle.Width, format);
		string text = lineInfo.Text;
		byte[] cjkString = GetCjkString(text);
		m_streamWriter.ShowNextLineText(cjkString, hex: false);
	}

	private byte[] GetCjkString(string line)
	{
		if (line == null)
		{
			throw new ArgumentNullException("line");
		}
		return PdfString.EscapeSymbols(PdfString.ToUnicodeArray(line, bAddPrefix: false));
	}

	private void DrawAsciiLine(LineInfo lineInfo, RectangleF layoutRectangle, PdfFont font, PdfStringFormat format)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		JustifyLine(lineInfo, layoutRectangle.Width, format);
		string text = lineInfo.Text;
		PdfString asciiString = GetAsciiString(text);
		m_streamWriter.ShowNextLineText(asciiString);
	}

	private PdfString GetAsciiString(string token)
	{
		if (token == null)
		{
			throw new ArgumentNullException("token");
		}
		PdfString pdfString = new PdfString(token);
		if (m_currentFont is PdfStandardFont)
		{
			PdfStandardFont pdfStandardFont = m_currentFont as PdfStandardFont;
			if (pdfStandardFont.Name != pdfStandardFont.FontFamily.ToString() && CheckFontEncoding(pdfStandardFont.FontInternal as PdfDictionary) == "MacRomanEncoding")
			{
				pdfString.EncodedBytes = GetMacRomanEncodedByte(token);
			}
		}
		pdfString.Encode = PdfString.ForceEncoding.ASCII;
		return pdfString;
	}

	private byte[] GetMacRomanEncodedByte(string token)
	{
		Encoding encoding = null;
		try
		{
			encoding = Encoding.GetEncoding("macintosh");
		}
		catch (Exception)
		{
			encoding = Encoding.UTF8;
		}
		return encoding.GetBytes(token);
	}

	private string CheckFontEncoding(PdfDictionary fontDictionary)
	{
		PdfName pdfName = new PdfName();
		string result = string.Empty;
		if (fontDictionary.ContainsKey("Encoding"))
		{
			pdfName = fontDictionary["Encoding"] as PdfName;
			if (pdfName == null)
			{
				Type type = fontDictionary["Encoding"].GetType();
				PdfDictionary pdfDictionary = new PdfDictionary();
				if (type.Name == "PdfDictionary")
				{
					pdfDictionary = fontDictionary["Encoding"] as PdfDictionary;
					if (pdfDictionary == null)
					{
						pdfName = (fontDictionary["Encoding"] as PdfReferenceHolder).Object as PdfName;
						result = pdfName.Value;
					}
				}
				else if (type.Name == "PdfReferenceHolder")
				{
					pdfDictionary = (fontDictionary["Encoding"] as PdfReferenceHolder).Object as PdfDictionary;
				}
				if (pdfDictionary != null && pdfDictionary.ContainsKey("Type"))
				{
					result = (pdfDictionary["Type"] as PdfName).Value;
				}
				if (pdfDictionary != null && pdfDictionary.ContainsKey("BaseEncoding"))
				{
					result = (pdfDictionary["BaseEncoding"] as PdfName).Value;
				}
			}
			else
			{
				result = pdfName.Value;
			}
		}
		return result;
	}

	private void DrawUnicodeLine(LineInfo lineInfo, RectangleF layoutRectangle, PdfFont font, PdfStringFormat format)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		string text = lineInfo.Text;
		_ = lineInfo.Width;
		bool flag = format?.RightToLeft ?? false;
		bool flag2 = format != null && (format.WordSpacing != 0f || format.Alignment == PdfTextAlignment.Justify);
		PdfTrueTypeFont pdfTrueTypeFont = font as PdfTrueTypeFont;
		float wordSpacing = JustifyLine(lineInfo, layoutRectangle.Width, format);
		if (flag)
		{
			flag = false;
		}
		if (flag || (format != null && format.TextDirection != 0))
		{
			lock (s_rtlRenderLock)
			{
				string[] array = null;
				bool rtl = format != null && format.Alignment == PdfTextAlignment.Right;
				if (lineInfo.OpenTypeGlyphList == null || (lineInfo.OpenTypeGlyphList != null && lineInfo.OpenTypeGlyphList.Glyphs != null && lineInfo.OpenTypeGlyphList.Glyphs.Count == 0))
				{
					array = ((format == null || format.TextDirection == PdfTextDirection.None) ? RtlRenderer.Layout(lineInfo, pdfTrueTypeFont, rtl, flag2, format) : RtlRenderer.Layout(lineInfo, pdfTrueTypeFont, format.TextDirection == PdfTextDirection.RightToLeft, flag2, format));
					string[] array2 = null;
					array2 = ((array.Length <= 1) ? new string[1] { text } : ((format == null || format.TextDirection == PdfTextDirection.None) ? RtlRenderer.SplitLayout(text, pdfTrueTypeFont, rtl, flag2, format) : RtlRenderer.SplitLayout(text, pdfTrueTypeFont, format.TextDirection == PdfTextDirection.RightToLeft, flag2, format)));
					DrawUnicodeBlocks(array, array2, font, format, wordSpacing);
				}
				else
				{
					Bidi bidi = new Bidi();
					if (format.TextDirection == PdfTextDirection.RightToLeft)
					{
						flag = true;
					}
					OtfGlyphInfo[] logicalToVisualGlyphs = bidi.GetLogicalToVisualGlyphs(lineInfo.OpenTypeGlyphList.Glyphs, flag, pdfTrueTypeFont.TtfReader, lineInfo.BidiLevels);
					OtfGlyphInfoList otfGlyphInfoList = new OtfGlyphInfoList(logicalToVisualGlyphs, 0, logicalToVisualGlyphs.Length);
					OtfGlyphTokenizer otfGlyphTokenizer = new OtfGlyphTokenizer();
					if (flag2)
					{
						OtfGlyphInfo[][] blocks = otfGlyphTokenizer.SplitGlyphs(otfGlyphInfoList.Glyphs);
						DrawOpenTypeStringBlocks(blocks, pdfTrueTypeFont, format, wordSpacing, otfGlyphTokenizer);
					}
					else
					{
						List<OtfGlyphInfo[]> list = new List<OtfGlyphInfo[]>();
						list.Add(otfGlyphInfoList.Glyphs.ToArray());
						DrawOpenTypeStringBlocks(list.ToArray(), pdfTrueTypeFont, format, wordSpacing, otfGlyphTokenizer);
					}
				}
				return;
			}
		}
		if (flag2)
		{
			if (lineInfo.OpenTypeGlyphList != null && pdfTrueTypeFont.InternalFont is UnicodeTrueTypeFont)
			{
				OtfGlyphTokenizer otfGlyphTokenizer2 = new OtfGlyphTokenizer();
				OtfGlyphInfo[][] blocks2 = otfGlyphTokenizer2.SplitGlyphs(lineInfo.OpenTypeGlyphList.Glyphs);
				DrawOpenTypeStringBlocks(blocks2, pdfTrueTypeFont, format, wordSpacing, otfGlyphTokenizer2);
			}
			else
			{
				string[] words = null;
				string[] blocks3 = BreakUnicodeLine(text, pdfTrueTypeFont, out words, format);
				DrawUnicodeBlocks(blocks3, words, font, format, wordSpacing);
			}
			return;
		}
		if (lineInfo.OpenTypeGlyphList != null && pdfTrueTypeFont.InternalFont is UnicodeTrueTypeFont)
		{
			if (lineInfo.OpenTypeGlyphList.HasYPlacement())
			{
				DrawOpenTypeStringBlocks(lineInfo.OpenTypeGlyphList, pdfTrueTypeFont, format, 0f);
			}
			else
			{
				DrawOpenTypeString(lineInfo.OpenTypeGlyphList, pdfTrueTypeFont);
			}
			return;
		}
		int length = text.Length;
		text = RtlRenderer.TrimLRM(text);
		if (text.Length != length && pdfTrueTypeFont.InternalFont is UnicodeTrueTypeFont)
		{
			UnicodeTrueTypeFont unicodeTrueTypeFont = pdfTrueTypeFont.InternalFont as UnicodeTrueTypeFont;
			if (unicodeTrueTypeFont.m_usedChars == null)
			{
				unicodeTrueTypeFont.m_usedChars = new Dictionary<char, char>();
			}
			unicodeTrueTypeFont.m_usedChars[' '] = ' ';
		}
		string token = ConvertToUnicode(text, pdfTrueTypeFont, format);
		PdfString unicodeString = GetUnicodeString(token);
		m_streamWriter.ShowNextLineText(unicodeString);
	}

	private void DrawOpenTypeString(OtfGlyphInfoList glyphInfoList, PdfTrueTypeFont ttfFont)
	{
		_ = ttfFont.InternalFont;
		_ = new ushort[glyphInfoList.Glyphs.Count];
		(ttfFont.InternalFont as UnicodeTrueTypeFont).SetSymbols(glyphInfoList);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("[");
		stringBuilder.Append("<");
		int num = 0;
		for (int i = 0; i < glyphInfoList.Glyphs.Count; i++)
		{
			bool flag = false;
			OtfGlyphInfo otfGlyphInfo = glyphInfoList.Glyphs[i];
			_ = otfGlyphInfo.CharCode;
			if (otfGlyphInfo.CharCode > -1)
			{
				flag = LanguageUtil.IsDiscardGlyph(otfGlyphInfo.CharCode);
			}
			if (flag)
			{
				continue;
			}
			if (otfGlyphInfo.leadingX != 0 || otfGlyphInfo.leadingY != 0)
			{
				if (otfGlyphInfo.Width == 0f && i - 1 >= 0 && glyphInfoList.Glyphs[i - 1].Width != 0f)
				{
					stringBuilder.Append(">");
					int num2 = (int)(glyphInfoList.Glyphs[i - 1].Width - (float)otfGlyphInfo.leadingX);
					stringBuilder.Append(num2.ToString());
					stringBuilder.Append("<");
					num = num2;
				}
				else if (otfGlyphInfo.Width == 0f && otfGlyphInfo.CharCode == -1 && glyphInfoList.Glyphs[i - 1].Width == 0f && otfGlyphInfo.leadingX > 0)
				{
					stringBuilder.Append(">");
					stringBuilder.Append((-otfGlyphInfo.leadingX).ToString());
					stringBuilder.Append("<");
					num = -otfGlyphInfo.leadingX;
				}
				else if (otfGlyphInfo.Width == 0f && otfGlyphInfo.CharCode != -1 && i - 2 >= 0 && glyphInfoList.Glyphs[i - 1].Width == 0f && otfGlyphInfo.leadingX > 0 && glyphInfoList.Glyphs[i - 2].Width != 0f)
				{
					stringBuilder.Append(">");
					int num3 = (int)(glyphInfoList.Glyphs[i - 2].Width - (float)otfGlyphInfo.leadingX);
					stringBuilder.Append(num3.ToString());
					stringBuilder.Append("<");
					num = num3;
				}
			}
			else if (otfGlyphInfo.Width != 0f && num != 0)
			{
				stringBuilder.Append(">");
				stringBuilder.Append((-num).ToString());
				stringBuilder.Append("<");
				num = 0;
			}
			byte[] bytes = PdfString.ToEncode(glyphInfoList.Glyphs[i].Index);
			stringBuilder.Append(PdfString.BytesToHex(bytes));
		}
		stringBuilder.Append(">");
		stringBuilder.Append("]");
		m_streamWriter.ShowFormatedText(stringBuilder.ToString());
	}

	private void DrawOpenTypeString(OtfGlyphInfo[] glyphs, PdfTrueTypeFont ttfFont, PdfStringFormat format, float spaceWidth, out bool skipNextLine)
	{
		skipNextLine = false;
		OtfGlyphInfoList otfGlyphInfoList = new OtfGlyphInfoList(glyphs, 0, glyphs.Length);
		if (otfGlyphInfoList.HasOffSet() && otfGlyphInfoList.HasArabicScript())
		{
			DrawOpenTypeStringUnicodeBlocks(otfGlyphInfoList, ttfFont, format, spaceWidth);
			skipNextLine = true;
		}
		else if (otfGlyphInfoList.HasYPlacement())
		{
			DrawOpenTypeStringBlocks(otfGlyphInfoList, ttfFont, format, spaceWidth);
			skipNextLine = true;
		}
		else
		{
			DrawOpenTypeString(otfGlyphInfoList, ttfFont);
		}
	}

	private void DrawOpenTypeStringUnicodeBlocks(OtfGlyphInfoList glyphInfoList, PdfTrueTypeFont ttfFont, PdfStringFormat format, float spaceWidth)
	{
		_ = ttfFont.InternalFont;
		_ = new ushort[glyphInfoList.Glyphs.Count];
		(ttfFont.InternalFont as UnicodeTrueTypeFont).SetSymbols(glyphInfoList);
		float num = ttfFont.Metrics.GetSize(format) / 1000f;
		float num2 = format.HorizontalScalingFactor / 100f;
		float characterSpacing = format.CharacterSpacing;
		m_streamWriter.StartNextLine();
		int num3 = glyphInfoList.Start;
		for (int i = 0; i < glyphInfoList.Glyphs.Count; i++)
		{
			bool flag = false;
			OtfGlyphInfo otfGlyphInfo = glyphInfoList.Glyphs[i];
			_ = otfGlyphInfo.CharCode;
			if (otfGlyphInfo.CharCode > -1)
			{
				flag = LanguageUtil.IsDiscardGlyph(otfGlyphInfo.CharCode);
			}
			if (flag || !otfGlyphInfo.HasOffset())
			{
				continue;
			}
			if (i - 1 - num3 >= 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("[");
				stringBuilder.Append("<");
				for (int j = num3; j < i; j++)
				{
					byte[] bytes = PdfString.ToEncode(glyphInfoList.Glyphs[j].Index);
					stringBuilder.Append(PdfString.BytesToHex(bytes));
				}
				stringBuilder.Append(">");
				stringBuilder.Append("]");
				m_streamWriter.ShowFormatedText(stringBuilder.ToString());
				m_streamWriter.StartNextLine(GetSubrangeWidth(num3, i - 1, format, ttfFont, glyphInfoList.Glyphs), 0f);
			}
			float num4 = float.NaN;
			float num5 = float.NaN;
			if (otfGlyphInfo.HasPlacement())
			{
				float num6 = 0f;
				int num7 = i;
				OtfGlyphInfo otfGlyphInfo2 = glyphInfoList.Glyphs[i];
				while (otfGlyphInfo2 != null && otfGlyphInfo2.m_placment != 0)
				{
					num6 += (float)otfGlyphInfo2.leadingX;
					if (otfGlyphInfo2.m_placment == 0)
					{
						break;
					}
					num7 += otfGlyphInfo2.m_placment;
					otfGlyphInfo2 = glyphInfoList.Glyphs[num7];
				}
				num4 = 0f - GetSubrangeWidth(num7, i, format, ttfFont, glyphInfoList.Glyphs) + num6 * num * num2;
				float num8 = 0f;
				int num9 = i;
				OtfGlyphInfo otfGlyphInfo3 = glyphInfoList.Glyphs[i];
				while (otfGlyphInfo3 != null && otfGlyphInfo3.leadingY != 0)
				{
					num8 += (float)otfGlyphInfo3.leadingY;
					if (otfGlyphInfo3.m_placment == 0)
					{
						break;
					}
					num9 += otfGlyphInfo3.m_placment;
					otfGlyphInfo3 = glyphInfoList.Glyphs[num9];
				}
				num5 = 0f - GetSubrangeYDelta(glyphInfoList.Glyphs, num9, i, ttfFont) + num8 * num;
				m_streamWriter.StartNextLine(num4, 0f - num5);
			}
			byte[] bytes2 = PdfString.ToEncode(otfGlyphInfo.Index);
			m_streamWriter.ShowText(PdfString.BytesToHex(bytes2), hex: true);
			if (!float.IsNaN(num4))
			{
				m_streamWriter.StartNextLine(0f - num4, num5);
			}
			if (otfGlyphInfo.HasAdvance())
			{
				m_streamWriter.StartNextLine(otfGlyphInfo.HasPlacement() ? 0f : (otfGlyphInfo.Width + (float)otfGlyphInfo.xAdvance * num + characterSpacing + GetWordSpacingAddition(otfGlyphInfo, ttfFont, format) * num2), (float)otfGlyphInfo.yAdvance * num);
			}
			num3 = i + 1;
		}
		if (glyphInfoList.End - num3 > 0)
		{
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder2.Append("[");
			stringBuilder2.Append("<");
			for (int k = num3; k < glyphInfoList.End; k++)
			{
				byte[] bytes3 = PdfString.ToEncode(glyphInfoList.Glyphs[k].Index);
				stringBuilder2.Append(PdfString.BytesToHex(bytes3));
			}
			stringBuilder2.Append(">");
			stringBuilder2.Append("]");
			m_streamWriter.ShowFormatedText(stringBuilder2.ToString());
		}
	}

	private float GetSubrangeYDelta(List<OtfGlyphInfo> glyphInfos, int from, int to, PdfTrueTypeFont font)
	{
		float num = font.Size / 1000f;
		float num2 = 0f;
		for (int i = from; i < to; i++)
		{
			num2 += (float)glyphInfos[i].yAdvance * num;
		}
		return num2;
	}

	private float GetSubrangeWidth(int from, int to, PdfStringFormat stringFormat, PdfTrueTypeFont font, List<OtfGlyphInfo> glyphInfos)
	{
		float num = font.Size / 1000f;
		float characterSpacing = stringFormat.CharacterSpacing;
		float num2 = stringFormat.HorizontalScalingFactor / 100f;
		float num3 = 0f;
		for (int i = from; i <= to; i++)
		{
			OtfGlyphInfo otfGlyphInfo = glyphInfos[i];
			if (!otfGlyphInfo.HasPlacement())
			{
				num3 += (otfGlyphInfo.Width * num + characterSpacing + GetWordSpacingAddition(otfGlyphInfo, font, stringFormat)) * num2;
			}
			if (i > from)
			{
				num3 += (float)glyphInfos[i - 1].xAdvance * num * num2;
			}
		}
		return num3;
	}

	private float GetWordSpacingAddition(OtfGlyphInfo glyph, PdfTrueTypeFont typeFont, PdfStringFormat stringFormat)
	{
		if (typeFont != null || glyph.Characters[0] != ' ')
		{
			return 0f;
		}
		return stringFormat.WordSpacing;
	}

	private void DrawOpenTypeStringBlocks(OtfGlyphInfoList glyphInfoList, PdfTrueTypeFont ttfFont, PdfStringFormat format, float spaceWidth)
	{
		_ = ttfFont.InternalFont;
		_ = new ushort[glyphInfoList.Glyphs.Count];
		(ttfFont.InternalFont as UnicodeTrueTypeFont).SetSymbols(glyphInfoList);
		float size = ttfFont.Metrics.GetSize(format);
		m_streamWriter.StartNextLine();
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 0f;
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < glyphInfoList.Glyphs.Count; i++)
		{
			bool flag3 = false;
			OtfGlyphInfo otfGlyphInfo = glyphInfoList.Glyphs[i];
			_ = otfGlyphInfo.CharCode;
			if (otfGlyphInfo.CharCode > -1)
			{
				flag3 = LanguageUtil.IsDiscardGlyph(otfGlyphInfo.CharCode);
			}
			if (flag3)
			{
				continue;
			}
			float num7 = otfGlyphInfo.Width * (0.001f * size);
			if (otfGlyphInfo.leadingX != 0 || otfGlyphInfo.leadingY != 0)
			{
				if (otfGlyphInfo.leadingX != 0 && otfGlyphInfo.Width == 0f)
				{
					num6 = (float)otfGlyphInfo.leadingX * (0.001f * size);
					flag = true;
				}
				if (otfGlyphInfo.leadingY != 0 && otfGlyphInfo.Width == 0f)
				{
					num5 = (float)otfGlyphInfo.leadingY * (0.001f * size);
					flag = true;
				}
			}
			if (i != 0)
			{
				if (flag && otfGlyphInfo.Width == 0f)
				{
					if (i - 1 >= 0 && glyphInfoList.Glyphs[i - 1].Width == 0f)
					{
						if (otfGlyphInfo.CharCode != -1)
						{
							num6 = 0f;
						}
						else
						{
							flag2 = true;
						}
					}
					if (num7 == 0f && num6 != 0f && num6 < 0f && otfGlyphInfo.CharCode != -1)
					{
						m_streamWriter.StartNextLine(num2 - num3, 0f - (num5 - num4));
					}
					else
					{
						m_streamWriter.StartNextLine(num6, 0f - num5);
					}
					if (num3 != 0f || num4 != 0f)
					{
						num3 += num6;
						num4 += num5;
					}
					else
					{
						num3 = num6;
						num4 = num5;
					}
					num6 = 0f;
					num5 = 0f;
				}
				else if (otfGlyphInfo.Width == 0f)
				{
					if (num2 != 0f && num2 == num)
					{
						m_streamWriter.StartNextLine(num, 0f);
					}
					else
					{
						m_streamWriter.StartNextLine(0f, 0f);
					}
				}
				else if (num3 != 0f || num4 != 0f)
				{
					if (flag2 && (otfGlyphInfo.CharCode != 32 || (otfGlyphInfo.CharCode == 32 && num6 == 0f)))
					{
						m_streamWriter.StartNextLine(0f - num6, 0f - (num5 - num4));
					}
					else if (!flag && num3 < 0f && i - 1 >= 0 && glyphInfoList.Glyphs[i - 1].Width == 0f && glyphInfoList.Glyphs[i - 1].CharCode != -1)
					{
						m_streamWriter.StartNextLine(0f, 0f - (num5 - num4));
					}
					else
					{
						m_streamWriter.StartNextLine(num2 - num3, 0f - (num5 - num4));
					}
					num3 = num6;
					num4 = num5;
					num6 = 0f;
					flag2 = false;
				}
				else
				{
					m_streamWriter.StartNextLine(num, 0f);
				}
				flag = false;
			}
			byte[] bytes = PdfString.ToEncode(otfGlyphInfo.Index);
			m_streamWriter.ShowText(PdfString.BytesToHex(bytes), hex: true);
			num = num7;
			if (num7 != 0f)
			{
				num2 = num7;
			}
		}
		m_streamWriter.StartNextLine(num2 - num3 + spaceWidth, 0f - (num5 - num4));
	}

	private void DrawOpenTypeStringBlocks(OtfGlyphInfo[][] blocks, PdfTrueTypeFont ttfFont, PdfStringFormat format, float wordSpacing, OtfGlyphTokenizer tokenizer)
	{
		m_streamWriter.StartNextLine();
		float num = 0f;
		float num2 = 0f;
		float firstLineIndent = 0f;
		float paragraphIndent = 0f;
		try
		{
			if (format != null)
			{
				firstLineIndent = format.FirstLineIndent;
				paragraphIndent = format.m_paragraphIndent;
				format.FirstLineIndent = 0f;
				format.m_paragraphIndent = 0f;
			}
			float num3 = ttfFont.GetCharWidth(' ', format) + wordSpacing;
			float num4 = format?.CharacterSpacing ?? 0f;
			float num5 = ((format != null && wordSpacing == 0f) ? format.WordSpacing : 0f);
			num3 += num4 + num5;
			bool skipNextLine = false;
			int i = 0;
			for (int num6 = blocks.Length; i < num6; i++)
			{
				OtfGlyphInfo[] array = blocks[i];
				float num7 = 0f;
				if (num != 0f && !skipNextLine)
				{
					m_streamWriter.StartNextLine(num, 0f);
				}
				skipNextLine = false;
				if (array.Length != 0)
				{
					num7 += tokenizer.GetLineWidth(array, ttfFont, format);
					num7 += num4;
					DrawOpenTypeString(array, ttfFont, format, num3, out skipNextLine);
				}
				if (i != num6 - 1)
				{
					num = num7 + num3;
					num2 += num;
				}
			}
			if (num2 > 0f)
			{
				m_streamWriter.StartNextLine(0f - num2, 0f);
			}
		}
		finally
		{
			if (format != null)
			{
				format.FirstLineIndent = firstLineIndent;
				format.m_paragraphIndent = paragraphIndent;
			}
		}
	}

	private PdfString GetUnicodeString(string token)
	{
		if (token == null)
		{
			throw new ArgumentNullException("token");
		}
		return new PdfString(token)
		{
			Converted = true,
			Encode = PdfString.ForceEncoding.ASCII
		};
	}

	private string[] BreakUnicodeLine(string line, PdfTrueTypeFont ttfFont, out string[] words, PdfStringFormat format)
	{
		if (line == null)
		{
			throw new ArgumentNullException("line");
		}
		if (ttfFont == null)
		{
			throw new ArgumentNullException("ttfFont");
		}
		words = line.Split((char[]?)null);
		string[] array = new string[words.Length];
		int i = 0;
		for (int num = words.Length; i < num; i++)
		{
			string text = words[i];
			string text2 = ConvertToUnicode(text, ttfFont, format);
			array[i] = text2;
		}
		return array;
	}

	private string ConvertToUnicode(string text, PdfTrueTypeFont ttfFont, PdfStringFormat format)
	{
		string result = null;
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (ttfFont == null)
		{
			throw new ArgumentNullException("ttfFont");
		}
		if (ttfFont.InternalFont is UnicodeTrueTypeFont)
		{
			TtfReader ttfReader = (ttfFont.InternalFont as UnicodeTrueTypeFont).TtfReader;
			UnicodeTrueTypeFont unicodeTrueTypeFont = ttfFont.InternalFont as UnicodeTrueTypeFont;
			if ((format != null && format.ComplexScript) || (IsDirectPDF && ttfReader.isOTFFont()))
			{
				unicodeTrueTypeFont.SetSymbols(text, opentype: true);
			}
			else
			{
				ttfFont.SetSymbols(text);
			}
			result = ttfReader.ConvertString(text);
			if (XPSToken && XPSReplaceCharacter.ContainsKey(text))
			{
				result = XPSReplaceCharacter[text];
			}
			result = PdfString.ByteToString(PdfString.ToEncode(result.ToCharArray()));
		}
		return result;
	}

	private void DrawUnicodeBlocks(string[] blocks, string[] words, PdfFont font, PdfStringFormat format, float wordSpacing)
	{
		if (blocks == null)
		{
			throw new ArgumentNullException("blocks");
		}
		if (words == null)
		{
			throw new ArgumentNullException("words");
		}
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		m_streamWriter.StartNextLine();
		float num = 0f;
		float num2 = 0f;
		float firstLineIndent = 0f;
		float paragraphIndent = 0f;
		try
		{
			if (format != null)
			{
				firstLineIndent = format.FirstLineIndent;
				paragraphIndent = format.m_paragraphIndent;
				format.FirstLineIndent = 0f;
				format.m_paragraphIndent = 0f;
			}
			float num3 = font.GetCharWidth(' ', format) + wordSpacing;
			float num4 = format?.CharacterSpacing ?? 0f;
			float num5 = ((format != null && wordSpacing == 0f) ? format.WordSpacing : 0f);
			num3 += num4 + num5;
			int i = 0;
			for (int num6 = blocks.Length; i < num6; i++)
			{
				string token = blocks[i];
				string text = words[i];
				float num7 = 0f;
				if (num != 0f)
				{
					m_streamWriter.StartNextLine(num, 0f);
				}
				if (text.Length > 0)
				{
					num7 += font.MeasureString(text, format).Width;
					num7 += num4;
					PdfString unicodeString = GetUnicodeString(token);
					m_streamWriter.ShowText(unicodeString);
					if (i != num6 - 1)
					{
						num = num7 + num3;
						num2 += num;
					}
				}
				else
				{
					num7 += font.GetCharWidth(' ', format);
					if (i != num6 - 1)
					{
						num = num7;
						num2 += num;
					}
				}
			}
			if (num2 > 0f)
			{
				m_streamWriter.StartNextLine(0f - num2, 0f);
			}
		}
		finally
		{
			if (format != null)
			{
				format.FirstLineIndent = firstLineIndent;
				format.m_paragraphIndent = paragraphIndent;
			}
		}
	}

	private string[] GetTextLines(string text)
	{
		MatchCollection matchCollection = new Regex("[^\r\n]*").Matches(text);
		int count = matchCollection.Count;
		List<string> list = new List<string>();
		bool flag = true;
		for (int i = 0; i < count; i++)
		{
			string value = matchCollection[i].Value;
			if (value == string.Empty && !flag)
			{
				flag = true;
				continue;
			}
			if (value != string.Empty)
			{
				flag = false;
			}
			list.Add(value);
		}
		return list.ToArray();
	}

	private void ApplyStringSettings(PdfFont font, PdfPen pen, PdfBrush brush, PdfStringFormat format, RectangleF bounds)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		if (brush is PdfTilingBrush)
		{
			m_bCSInitialized = false;
			(brush as PdfTilingBrush).Graphics.ColorSpace = ColorSpace;
		}
		else if (brush is PdfGradientBrush)
		{
			m_bCSInitialized = false;
			(brush as PdfGradientBrush).ColorSpace = ColorSpace;
		}
		bool flag = false;
		TextRenderingMode textRenderingMode = GetTextRenderingMode(pen, brush, format);
		PdfTrueTypeFont pdfTrueTypeFont = font as PdfTrueTypeFont;
		if ((font is PdfTrueTypeFont && pdfTrueTypeFont != null && pdfTrueTypeFont.Unicode) || (pdfTrueTypeFont != null && pdfTrueTypeFont.m_isStyleBold))
		{
			PdfTrueTypeFont obj = font as PdfTrueTypeFont;
			string postScriptName = obj.m_fontInternal.Metrics.PostScriptName;
			bool flag2 = false;
			if (postScriptName != null && postScriptName.ToLower().Contains("bold"))
			{
				flag2 = true;
			}
			if (obj.m_fontInternal.Metrics.IsBold != font.Bold && font.Bold && !flag2)
			{
				if (pen == null && brush != null)
				{
					pen = new PdfPen(brush);
				}
				textRenderingMode = TextRenderingMode.FillStroke;
				flag = true;
			}
		}
		m_streamWriter.BeginText();
		StateControl(pen, brush, font, format);
		if (!m_artifactBMCAdded)
		{
			if (Layer != null && Page != null && Page is PdfPage)
			{
				if ((Page as PdfPage).Section.ParentDocument is PdfDocument)
				{
					PdfStructTreeRoot structTreeRoot = PdfCatalog.StructTreeRoot;
					if (structTreeRoot != null)
					{
						if (Tag != null && Tag is PdfStructureElement)
						{
							if (IsDirectPDF)
							{
								if (!(Tag as PdfStructureElement).IsAdded)
								{
									int num = structTreeRoot.Add(Tag as PdfStructureElement, Page, bounds);
									string abbrevation = (Tag as PdfStructureElement).Abbrevation;
									StreamWriter.WriteTag(string.Format("/{0} <</E (" + abbrevation + ") /MCID {1} >>BDC", structTreeRoot.ConvertToEquivalentTag((Tag as PdfStructureElement).TagType), num));
									(Tag as PdfStructureElement).IsAdded = true;
								}
							}
							else
							{
								int num2 = structTreeRoot.Add(Tag as PdfStructureElement, Page, bounds);
								string abbrevation2 = (Tag as PdfStructureElement).Abbrevation;
								StreamWriter.WriteTag(string.Format("/{0} <</E (" + abbrevation2 + ") /MCID {1} >>BDC", structTreeRoot.ConvertToEquivalentTag((Tag as PdfStructureElement).TagType), num2));
							}
						}
						else if (Tag != null && Tag is PdfArtifact)
						{
							string arg = SetArtifact(Tag as PdfArtifact);
							StreamWriter.Write(string.Format("/Artifact << {0} >>BDC" + Environment.NewLine, arg));
						}
						else if (IsTaggedPdf)
						{
							int num3 = structTreeRoot.Add(new PdfStructureElement(PdfTagType.Paragraph), Page, bounds);
							StreamWriter.WriteTag(string.Format("/{0} <</MCID {1} >>BDC", "P", num3));
						}
					}
				}
			}
			else
			{
				PdfStructTreeRoot structTreeRoot2 = PdfCatalog.StructTreeRoot;
				if (structTreeRoot2 != null)
				{
					if (Tag != null && Tag is PdfArtifact)
					{
						string arg2 = SetArtifact(Tag as PdfArtifact);
						StreamWriter.Write(string.Format("/Artifact << {0} >>BDC" + Environment.NewLine, arg2));
					}
					else if (Layer != null && Page != null && Page is PdfLoadedPage && (Page as PdfLoadedPage).Document.FileStructure.TaggedPdf)
					{
						int num4 = structTreeRoot2.Add("P", "", Page, bounds);
						StreamWriter.WriteTag(string.Format("/{0} <</MCID {1} >>BDC", "P", num4));
					}
					else if (!IsTemplateGraphics)
					{
						int num5 = structTreeRoot2.Add("P", "", bounds);
						StreamWriter.WriteTag(string.Format("/{0} <</MCID {1} >>BDC", "P", num5));
					}
				}
			}
		}
		if (flag)
		{
			m_streamWriter.SetLineWidth(font.Size / 30f);
		}
		if (textRenderingMode != m_previousTextRenderingMode)
		{
			m_streamWriter.SetTextRenderingMode(textRenderingMode);
			m_previousTextRenderingMode = textRenderingMode;
		}
		float num6 = format?.CharacterSpacing ?? 0f;
		if (num6 != m_previousCharacterSpacing && !m_isEmfTextScaled)
		{
			m_streamWriter.SetCharacterSpacing(num6);
			m_previousCharacterSpacing = num6;
		}
		float num7 = format?.WordSpacing ?? 0f;
		if (num7 != m_previousWordSpacing)
		{
			m_streamWriter.SetWordSpacing(num7);
			m_previousWordSpacing = num7;
		}
	}

	private float GetHorizontalAlignShift(float lineWidth, float boundsWidth, PdfStringFormat format)
	{
		float result = 0f;
		if (boundsWidth >= 0f && format != null && format.Alignment != 0)
		{
			switch (format.Alignment)
			{
			case PdfTextAlignment.Center:
				result = (boundsWidth - lineWidth) / 2f;
				break;
			case PdfTextAlignment.Right:
				result = boundsWidth - lineWidth;
				break;
			}
		}
		return result;
	}

	internal float GetTextVerticalAlignShift(float textHeight, float boundsHeight, PdfStringFormat format)
	{
		float result = 0f;
		if (boundsHeight >= 0f && format != null && format.LineAlignment != 0)
		{
			switch (format.LineAlignment)
			{
			case PdfVerticalAlignment.Middle:
				result = (boundsHeight - textHeight) / 2f;
				break;
			case PdfVerticalAlignment.Bottom:
				result = boundsHeight - textHeight;
				break;
			}
		}
		return result;
	}

	private float JustifyLine(LineInfo lineInfo, float boundsWidth, PdfStringFormat format)
	{
		string text = lineInfo.Text;
		float num = lineInfo.Width;
		bool num2 = ShouldJustify(lineInfo, boundsWidth, format);
		bool flag = format != null && format.WordSpacing != 0f;
		char[] spaces = StringTokenizer.Spaces;
		int charsCount = StringTokenizer.GetCharsCount(text, spaces);
		float num3 = 0f;
		if (num2)
		{
			if (flag)
			{
				num -= (float)charsCount * format.WordSpacing;
			}
			num3 = (boundsWidth - num) / (float)charsCount;
			m_streamWriter.SetWordSpacing(num3);
		}
		else if (format != null && format.Alignment == PdfTextAlignment.Justify)
		{
			m_streamWriter.SetWordSpacing(0f);
		}
		return num3;
	}

	private bool ShouldJustify(LineInfo lineInfo, float boundsWidth, PdfStringFormat format)
	{
		string text = lineInfo.Text;
		float width = lineInfo.Width;
		bool num = format != null && format.Alignment == PdfTextAlignment.Justify;
		bool flag = boundsWidth >= 0f && width < boundsWidth;
		char[] spaces = StringTokenizer.Spaces;
		bool flag2 = StringTokenizer.GetCharsCount(text, spaces) > 0 && text[0] != ' ';
		bool flag3 = (lineInfo.LineType & LineType.LayoutBreak) > LineType.None || (format != null && format.WordWrap == PdfWordWrapType.None);
		return num && flag && flag2 && flag3;
	}

	private bool CheckCorrectLayoutRectangle(ref RectangleF layoutRectangle)
	{
		return true;
	}

	internal RectangleF CheckCorrectLayoutRectangle(SizeF textSize, float x, float y, PdfStringFormat format)
	{
		RectangleF result = new RectangleF(x, y, textSize.Width, textSize.Width);
		if (format != null)
		{
			switch (format.Alignment)
			{
			case PdfTextAlignment.Center:
				result.X -= result.Width / 2f;
				break;
			case PdfTextAlignment.Right:
				result.X -= result.Width;
				break;
			}
			switch (format.LineAlignment)
			{
			case PdfVerticalAlignment.Middle:
				result.Y -= result.Height / 2f;
				break;
			case PdfVerticalAlignment.Bottom:
				result.Y -= result.Height;
				break;
			}
		}
		return result;
	}

	private void UnderlineStrikeoutText(PdfPen pen, PdfBrush brush, PdfStringLayoutResult result, PdfFont font, RectangleF layoutRectangle, PdfStringFormat format)
	{
		if (result == null)
		{
			throw new ArgumentNullException("result");
		}
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		if (!font.Underline && !font.Strikeout)
		{
			return;
		}
		PdfPen pdfPen = CreateUnderlineStikeoutPen(pen, brush, font, format);
		if (pdfPen == null)
		{
			return;
		}
		float num = 0f;
		float num2 = GetTextVerticalAlignShift(result.ActualSize.Height, layoutRectangle.Height, format);
		if (format != null && format.SubSuperScript == PdfSubSuperScript.SubScript)
		{
			num2 += font.Height - font.Metrics.GetHeight(format);
		}
		num = ((!IsDirectPDF) ? (0f - (layoutRectangle.Y + font.Height) - ((font.Metrics.GetDescent(format) > 0f) ? (0f - font.Metrics.GetDescent(format)) : font.Metrics.GetDescent(format)) - 1.5f * pdfPen.Width - num2) : (layoutRectangle.Y + num2 + GetAscent(font) + 1.5f * pdfPen.Width));
		float num3 = layoutRectangle.Y + num2 + font.Metrics.GetHeight(format) / 2f + 1.5f * pdfPen.Width;
		LineInfo[] lines = result.Lines;
		int i = 0;
		for (int lineCount = result.LineCount; i < lineCount; i++)
		{
			LineInfo lineInfo = lines[i];
			_ = lineInfo.Text;
			float width = lineInfo.Width;
			float horizontalAlignShift = GetHorizontalAlignShift(width, layoutRectangle.Width, format);
			float lineIndent = GetLineIndent(lineInfo, format, layoutRectangle, i == 0);
			horizontalAlignShift += ((!RightToLeft(format)) ? lineIndent : 0f);
			float num4 = layoutRectangle.X + horizontalAlignShift;
			float x = ((!ShouldJustify(lineInfo, layoutRectangle.Width, format)) ? (num4 + width - lineIndent) : (num4 + layoutRectangle.Width - lineIndent));
			if (font.Underline)
			{
				float num5 = num;
				if (m_isEMFPlus || m_isEMF || IsDirectPDF)
				{
					if (Tag != null || IsTaggedPdf)
					{
						isTemplateGraphics = true;
					}
					DrawLine(pdfPen, num4, num5, x, num5);
					if (Tag != null || IsTaggedPdf)
					{
						isTemplateGraphics = false;
					}
					num += result.LineHeight;
				}
				else
				{
					if (Tag != null || IsTaggedPdf)
					{
						isTemplateGraphics = true;
					}
					DrawLine(pdfPen, num4, 0f - num5, x, 0f - num5);
					if (Tag != null || IsTaggedPdf)
					{
						isTemplateGraphics = false;
					}
					num -= result.LineHeight;
				}
			}
			if (font.Strikeout)
			{
				float num6 = num3;
				DrawLine(pdfPen, num4, num6, x, num6);
				num3 += result.LineHeight;
			}
		}
	}

	private PdfPen CreateUnderlineStikeoutPen(PdfPen pen, PdfBrush brush, PdfFont font, PdfStringFormat format)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		float width = font.Metrics.GetSize(format) / 20f;
		PdfPen result = null;
		if (pen != null)
		{
			result = new PdfPen(pen.Color, width);
		}
		else if (brush != null)
		{
			result = new PdfPen(brush, width);
		}
		return result;
	}

	private void DrawLayoutResult(PdfStringLayoutResult result, PdfFont font, PdfStringFormat format, RectangleF layoutRectangle)
	{
		if (result == null)
		{
			throw new ArgumentNullException("result");
		}
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		LineInfo[] lines = result.Lines;
		float num = ((format == null || format.LineSpacing == 0f) ? font.Height : (format.LineSpacing + font.Height));
		PdfTrueTypeFont pdfTrueTypeFont = font as PdfTrueTypeFont;
		bool flag = pdfTrueTypeFont?.Unicode ?? false;
		bool flag2 = pdfTrueTypeFont?.Embed ?? false;
		float num2 = 0f;
		float num3 = 0f;
		int i = 0;
		for (int num4 = lines.Length; i < num4; i++)
		{
			LineInfo lineInfo = lines[i];
			string text = lineInfo.Text;
			float width = lineInfo.Width;
			if ((text == null || text.Length == 0) && (!m_isItalic || m_isSkewed))
			{
				float textVerticalAlignShift = GetTextVerticalAlignShift(result.ActualSize.Height, layoutRectangle.Height, format);
				PdfTransformationMatrix pdfTransformationMatrix = new PdfTransformationMatrix();
				float num5 = 0f - (layoutRectangle.Y + font.Height) - font.Metrics.GetDescent(format) - textVerticalAlignShift;
				num5 -= num * (float)(i + 1);
				pdfTransformationMatrix.Translate(layoutRectangle.X, num5);
				if (m_isSkewed)
				{
					pdfTransformationMatrix.Skew(0f, 11f);
				}
				m_streamWriter.ModifyTM(pdfTransformationMatrix);
				continue;
			}
			float horizontalAlignShift = GetHorizontalAlignShift(width, layoutRectangle.Width, format);
			float lineIndent = GetLineIndent(lineInfo, format, layoutRectangle, i == 0);
			horizontalAlignShift += ((!RightToLeft(format)) ? lineIndent : 0f);
			if (horizontalAlignShift != 0f && !m_isEmfTextScaled)
			{
				if (lineInfo.OpenTypeGlyphList != null)
				{
					float textVerticalAlignShift2 = GetTextVerticalAlignShift(result.ActualSize.Height, layoutRectangle.Height, format);
					PdfTransformationMatrix pdfTransformationMatrix2 = new PdfTransformationMatrix();
					float num6 = 0f - (layoutRectangle.Y + font.Height) - font.Metrics.GetDescent(format) - textVerticalAlignShift2;
					num6 -= num * (float)i;
					pdfTransformationMatrix2.Translate(layoutRectangle.X + horizontalAlignShift, num6);
					m_streamWriter.ModifyTM(pdfTransformationMatrix2);
				}
				else
				{
					m_streamWriter.StartNextLine(horizontalAlignShift, 0f);
				}
			}
			if (font is PdfCjkStandardFont)
			{
				DrawCjkString(lineInfo, layoutRectangle, font, format);
			}
			else if (flag)
			{
				DrawUnicodeLine(lineInfo, layoutRectangle, font, format);
			}
			else if (flag2)
			{
				DrawAsciiLine(lineInfo, layoutRectangle, font, format, flag2);
			}
			else
			{
				DrawAsciiLine(lineInfo, layoutRectangle, font, format);
			}
			if (i + 1 == num4)
			{
				continue;
			}
			if (!m_isItalic || m_isSkewed)
			{
				float textVerticalAlignShift3 = GetTextVerticalAlignShift(result.ActualSize.Height, layoutRectangle.Height, format);
				PdfTransformationMatrix pdfTransformationMatrix3 = new PdfTransformationMatrix();
				float num7 = 0f - (layoutRectangle.Y + font.Height) - font.Metrics.GetDescent(format) - textVerticalAlignShift3;
				num7 -= num * (float)(i + 1);
				pdfTransformationMatrix3.Translate(layoutRectangle.X, num7);
				if (m_isSkewed)
				{
					pdfTransformationMatrix3.Skew(0f, 11f);
				}
				m_streamWriter.ModifyTM(pdfTransformationMatrix3);
			}
			else
			{
				float num8 = 0.19486f;
				if (lines[i + 1].OpenTypeGlyphList != null)
				{
					PdfTransformationMatrix pdfTransformationMatrix4 = new PdfTransformationMatrix();
					num2 = num * (float)(i + 2);
					num3 += font.Height * num8 - horizontalAlignShift;
					pdfTransformationMatrix4.Translate(num3, 0f - num2);
					m_streamWriter.ModifyTM(pdfTransformationMatrix4);
				}
				else
				{
					m_streamWriter.StartNextLine(font.Height * num8 - horizontalAlignShift, 0f);
				}
			}
		}
		m_getResources().RequireProcSet("Text");
	}

	private void DrawAsciiLine(LineInfo lineInfo, RectangleF layoutRectangle, PdfFont font, PdfStringFormat format, bool embed)
	{
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		JustifyLine(lineInfo, layoutRectangle.Width, format);
		string text = lineInfo.Text;
		PdfString asciiString = GetAsciiString(text);
		m_streamWriter.ShowNextLineText(asciiString);
		PdfTrueTypeFont ttfFont = font as PdfTrueTypeFont;
		ConvertToUnicode(text, ttfFont, format);
	}

	internal void DrawStringLayoutResult(PdfStringLayoutResult result, PdfFont font, PdfPen pen, PdfBrush brush, RectangleF layoutRectangle, PdfStringFormat format, double maxRowFontSize, PdfFont maxPdfFont, PdfStringFormat maxPdfFormat)
	{
		if (result == null)
		{
			throw new ArgumentNullException("result");
		}
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		if (result.Empty)
		{
			return;
		}
		bool num = format != null && !format.LineLimit;
		bool flag = format == null || !format.NoClip;
		bool num2 = num && flag;
		PdfGraphicsState state = null;
		BeginMarkContent();
		if (num2)
		{
			state = Save();
			RectangleF clip = new RectangleF(layoutRectangle.Location, result.ActualSize);
			if (layoutRectangle.Width > 0f)
			{
				clip.Width = layoutRectangle.Width;
			}
			if (format.LineAlignment == PdfVerticalAlignment.Middle)
			{
				clip.Y += (layoutRectangle.Height - clip.Height) / 2f;
			}
			else if (format.LineAlignment == PdfVerticalAlignment.Bottom)
			{
				clip.Y += layoutRectangle.Height - clip.Height;
			}
			SetClip(clip);
		}
		ApplyStringSettings(font, pen, brush, format, layoutRectangle);
		float num3 = format?.HorizontalScalingFactor ?? 100f;
		if (num3 != m_previousTextScaling)
		{
			m_streamWriter.SetTextScaling(num3);
			m_previousTextScaling = num3;
		}
		float num4 = GetTextVerticalAlignShift(result.ActualSize.Height, layoutRectangle.Height, format);
		float num5 = ((format == null || format.LineSpacing == 0f) ? font.Height : format.LineSpacing);
		bool flag2 = format != null && format.SubSuperScript == PdfSubSuperScript.SubScript;
		float num6 = 0f;
		if (m_isEMFPlus || m_isUseFontSize)
		{
			if (m_isUseFontSize)
			{
				if (format.LineAlignment == PdfVerticalAlignment.Bottom && (double)num6 == 0.0)
				{
					num6 = (flag2 ? (maxPdfFont.Height - (maxPdfFont.Height + maxPdfFont.Metrics.GetDescent(maxPdfFormat))) : (maxPdfFont.Height - maxPdfFont.Size));
				}
				else
				{
					num6 = (flag2 ? (num5 - (font.Height + font.Metrics.GetDescent(format))) : (num5 - font.Size));
				}
			}
			else if (format.LineAlignment == PdfVerticalAlignment.Bottom && (double)num6 == 0.0)
			{
				num6 = (flag2 ? (maxPdfFont.Height - (maxPdfFont.Height + maxPdfFont.Metrics.GetDescent(maxPdfFormat))) : (maxPdfFont.Height - maxPdfFont.Metrics.GetAscent(maxPdfFormat)));
			}
			else
			{
				num6 = (flag2 ? (num5 - (font.Height + font.Metrics.GetDescent(format))) : (num5 - font.Metrics.GetAscent(format)));
			}
		}
		else if (format.LineAlignment == PdfVerticalAlignment.Bottom && (double)num6 == 0.0)
		{
			num6 = (flag2 ? (maxPdfFont.Height - (maxPdfFont.Height + maxPdfFont.Metrics.GetDescent(maxPdfFormat))) : (maxPdfFont.Height - maxPdfFont.Metrics.GetAscent(maxPdfFormat)));
		}
		else
		{
			num6 = (flag2 ? (num5 - (font.Height + font.Metrics.GetDescent(format))) : (num5 - font.Metrics.GetAscent(format)));
		}
		if (m_isEMF && m_isBaselineFormat && format != null && format.Alignment != PdfTextAlignment.Right)
		{
			num6 = 0f;
		}
		PdfTransformationMatrix pdfTransformationMatrix = new PdfTransformationMatrix();
		if (m_isEMF && m_isBaselineFormat && format != null && format.Alignment != PdfTextAlignment.Right)
		{
			pdfTransformationMatrix.Translate(layoutRectangle.X, 0f - layoutRectangle.Y);
		}
		else if (IsDirectPDF && font.Underline)
		{
			pdfTransformationMatrix.Translate(layoutRectangle.X, 0f - (layoutRectangle.Y + GetAscent(font)) - num4);
		}
		else
		{
			pdfTransformationMatrix.Translate(layoutRectangle.X, 0f - (layoutRectangle.Y + font.Height) - ((font.Metrics.GetDescent(format) > 0f) ? (0f - font.Metrics.GetDescent(format)) : font.Metrics.GetDescent(format)) - num4);
		}
		m_streamWriter.ModifyTM(pdfTransformationMatrix);
		if (layoutRectangle.Height < font.Size && result.ActualSize.Height - layoutRectangle.Height < font.Size / 2f - 1f)
		{
			num4 = 0f;
		}
		if (m_isEMF && result.ActualSize.Height - layoutRectangle.Height > font.Size / 2f - 1f)
		{
			num4 = GetTextVerticalAlignShift(result.ActualSize.Height, font.Height, format);
		}
		_ = 0f;
		DrawLayoutResult(result, font, format, layoutRectangle);
		if (num4 != 0f)
		{
			m_streamWriter.StartNextLine(0f, 0f - (num4 - result.LineHeight));
		}
		if (!m_artifactBMCAdded && ((Layer != null && Page != null && Page is PdfPage && (Page as PdfPage).Section.ParentDocument is PdfDocument && (Page as PdfPage).Section.ParentDocument.FileStructure.TaggedPdf) || (PdfCatalog.StructTreeRoot != null && !IsTemplateGraphics && (Tag != null || IsTaggedPdf))))
		{
			m_streamWriter.WriteTag("EMC");
		}
		m_streamWriter.EndText();
		UnderlineStrikeoutText(pen, brush, result, font, layoutRectangle, format);
		m_isEMFPlus = false;
		m_isUseFontSize = false;
		if (num2)
		{
			Restore(state);
		}
		EndMarkContent();
	}

	internal void DrawStringLayoutResult(PdfStringLayoutResult result, PdfFont font, PdfPen pen, PdfBrush brush, RectangleF layoutRectangle, PdfStringFormat format)
	{
		if (result == null)
		{
			throw new ArgumentNullException("result");
		}
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		if (result.Empty && !IsTaggedPdf)
		{
			return;
		}
		bool num = format != null && !format.LineLimit;
		bool flag = format == null || !format.NoClip;
		bool num2 = num && flag;
		PdfGraphicsState state = null;
		BeginMarkContent();
		if (num2)
		{
			state = Save();
			RectangleF clip = new RectangleF(layoutRectangle.Location, result.ActualSize);
			if (layoutRectangle.Width > 0f)
			{
				clip.Width = layoutRectangle.Width;
			}
			if (format.LineAlignment == PdfVerticalAlignment.Middle)
			{
				clip.Y += (layoutRectangle.Height - clip.Height) / 2f;
			}
			else if (format.LineAlignment == PdfVerticalAlignment.Bottom)
			{
				clip.Y += layoutRectangle.Height - clip.Height;
			}
			SetClip(clip);
		}
		if (font is PdfTrueTypeFont { InternalFont: not null } pdfTrueTypeFont && pdfTrueTypeFont.InternalFont is UnicodeTrueTypeFont && pdfTrueTypeFont.Italic)
		{
			UnicodeTrueTypeFont unicodeTrueTypeFont = (UnicodeTrueTypeFont)pdfTrueTypeFont.InternalFont;
			if (unicodeTrueTypeFont != null && !unicodeTrueTypeFont.TtfMetrics.IsItalic)
			{
				gState = Save();
				m_isRestoreGraphics = true;
				m_isItalic = true;
			}
		}
		ApplyStringSettings(font, pen, brush, format, layoutRectangle);
		float num3 = format?.HorizontalScalingFactor ?? 100f;
		if (num3 != m_previousTextScaling && !m_isEmfTextScaled)
		{
			m_streamWriter.SetTextScaling(num3);
			m_previousTextScaling = num3;
		}
		float num4 = GetTextVerticalAlignShift(result.ActualSize.Height, layoutRectangle.Height, format);
		float num5 = ((format == null || format.LineSpacing == 0f) ? font.Height : (format.LineSpacing + font.Height));
		bool flag2 = format != null && format.SubSuperScript == PdfSubSuperScript.SubScript;
		float num6 = 0f;
		num6 = ((!m_isEMFPlus && !m_isUseFontSize) ? (flag2 ? (num5 - (font.Height + font.Metrics.GetDescent(format))) : (num5 - font.Metrics.GetAscent(format))) : ((!m_isUseFontSize) ? (flag2 ? (num5 - (font.Height + font.Metrics.GetDescent(format))) : (num5 - font.Metrics.GetAscent(format))) : (flag2 ? (num5 - (font.Height + font.Metrics.GetDescent(format))) : (num5 - font.Size))));
		if (m_isEMF && m_isBaselineFormat && format != null && format.Alignment != PdfTextAlignment.Right)
		{
			num6 = 0f;
		}
		if (format != null && format.LineAlignment == PdfVerticalAlignment.Bottom && m_isBaselineFormat && !m_isEMFPlus && layoutRectangle.Height - result.ActualSize.Height != 0f && layoutRectangle.Height - result.ActualSize.Height < font.Size / 2f - 1f && Math.Round(layoutRectangle.Height, 2) <= Math.Round(font.Height, 2))
		{
			num6 = 0f - num5 / font.Size;
		}
		if (m_isItalic)
		{
			if (format != null && format.MeasureTiltingSpace)
			{
				m_isSkewed = true;
			}
			else
			{
				if (format != null && format.EnableBaseline)
				{
					TranslateTransform(layoutRectangle.X + font.Size / 5f, layoutRectangle.Y + num4);
				}
				else
				{
					TranslateTransform(layoutRectangle.X + font.Size / 5f, layoutRectangle.Y - num6 + num4);
				}
				SkewTransform(0f, -11f);
			}
		}
		if (format != null && format.EnableBaseline)
		{
			float num7 = font.Metrics.GetDescent(format);
			if (num7 < 0f)
			{
				num7 = 0f - num7;
			}
			if (format.LineAlignment == PdfVerticalAlignment.Bottom)
			{
				layoutRectangle.Y += num7;
			}
			if (format.LineAlignment == PdfVerticalAlignment.Top)
			{
				layoutRectangle.Y -= num7;
			}
		}
		PdfTransformationMatrix pdfTransformationMatrix = null;
		if (m_isEMF && m_isEmfTextScaled)
		{
			pdfTransformationMatrix = new PdfTransformationMatrix();
			if (num5 > font.Size && !m_isBaselineFormat)
			{
				pdfTransformationMatrix.Translate(layoutRectangle.X, 0f - (layoutRectangle.Y + num5 - num6));
			}
			else
			{
				pdfTransformationMatrix.Translate(layoutRectangle.X, 0f - (layoutRectangle.Y - num6));
			}
			pdfTransformationMatrix.Scale(m_emfScalingFactor.Width, m_emfScalingFactor.Height);
			m_streamWriter.ModifyTM(pdfTransformationMatrix);
		}
		else if (!m_isItalic || m_isSkewed)
		{
			pdfTransformationMatrix = new PdfTransformationMatrix();
			if (m_isEMF && m_isBaselineFormat && format != null && format.Alignment != PdfTextAlignment.Right)
			{
				pdfTransformationMatrix.Translate(layoutRectangle.X, 0f - layoutRectangle.Y);
			}
			else if (format != null && format.SubSuperScript == PdfSubSuperScript.SuperScript)
			{
				pdfTransformationMatrix.Translate(layoutRectangle.X, 0f - (layoutRectangle.Y + font.Height) + ((font.Metrics.GetDescent(format) > 0f) ? (0f - font.Metrics.GetDescent(format)) : font.Metrics.GetDescent(format)) + font.Height / 1.5f);
			}
			else if (IsDirectPDF)
			{
				pdfTransformationMatrix.Translate(layoutRectangle.X, 0f - (layoutRectangle.Y + GetAscent(font)) - num4);
			}
			else
			{
				pdfTransformationMatrix.Translate(layoutRectangle.X, 0f - (layoutRectangle.Y + font.Height) - ((font.Metrics.GetDescent(format) > 0f) ? (0f - font.Metrics.GetDescent(format)) : font.Metrics.GetDescent(format)) - num4);
			}
			if (m_isSkewed)
			{
				pdfTransformationMatrix.Skew(0f, 11f);
			}
			m_streamWriter.ModifyTM(pdfTransformationMatrix);
		}
		else
		{
			m_streamWriter.StartNextLine(0f, 0f);
		}
		if (m_isEMF && m_isBaselineFormat && format != null && format.Alignment == PdfTextAlignment.Right)
		{
			if (num5 > font.Size && m_isItalic && !m_isSkewed)
			{
				m_streamWriter.SetLeading(num5);
			}
		}
		else if (!m_isEMF || (!m_isBaselineFormat && !m_isEmfTextScaled))
		{
			if (num5 >= font.Size && m_isItalic && !m_isSkewed)
			{
				if (HasOtfGlyphList(result))
				{
					PdfTransformationMatrix pdfTransformationMatrix2 = new PdfTransformationMatrix();
					pdfTransformationMatrix2.Translate(0f, 0f - num5);
					m_streamWriter.ModifyTM(pdfTransformationMatrix2);
				}
				else
				{
					m_streamWriter.SetLeading(num5);
				}
			}
		}
		else
		{
			m_streamWriter.SetLeading(0f);
		}
		if (layoutRectangle.Height < font.Size && result.ActualSize.Height - layoutRectangle.Height < font.Size / 2f - 1f)
		{
			num4 = 0f;
		}
		if (m_isEMF && result.ActualSize.Height - layoutRectangle.Height > font.Size / 2f - 1f)
		{
			num4 = GetTextVerticalAlignShift(result.ActualSize.Height, font.Height, format);
		}
		if (num4 != 0f && !m_isEmfTextScaled && format != null && format.LineAlignment == PdfVerticalAlignment.Bottom && m_isBaselineFormat && !m_isEMFPlus && layoutRectangle.Height - result.ActualSize.Height != 0f && layoutRectangle.Height - result.ActualSize.Height > font.Size / 2f - 1f)
		{
			num4 -= (num6 - (flag2 ? (num5 - (font.Height + font.Metrics.GetDescent(format))) : (num5 - font.Size))) / 2f;
		}
		DrawLayoutResult(result, font, format, layoutRectangle);
		if (num4 != 0f && !m_isEmfTextScaled)
		{
			m_streamWriter.StartNextLine(0f, 0f - (num4 - result.LineHeight));
		}
		if (!m_artifactBMCAdded)
		{
			if ((Layer != null && Page != null && Page is PdfPage && (Page as PdfPage).Section.ParentDocument is PdfDocument && (Page as PdfPage).Section.ParentDocument.FileStructure.TaggedPdf) || (PdfCatalog.StructTreeRoot != null && !IsTemplateGraphics && (Tag != null || IsTaggedPdf)))
			{
				if (IsDirectPDF)
				{
					if (Tag is PdfStructureElement && !(Tag as PdfStructureElement).IsAdded)
					{
						m_streamWriter.WriteTag("EMC");
					}
				}
				else
				{
					m_streamWriter.WriteTag("EMC");
				}
			}
			if (PdfCatalog.StructTreeRoot != null && Layer != null && Page != null && Page is PdfLoadedPage && (Page as PdfLoadedPage).Document.FileStructure.TaggedPdf)
			{
				m_streamWriter.WriteTag("EMC");
			}
		}
		m_streamWriter.EndText();
		if (m_isItalic || m_isRestoreGraphics)
		{
			if (gState != null)
			{
				Restore(gState);
			}
			m_isItalic = false;
			m_isSkewed = false;
		}
		UnderlineStrikeoutText(pen, brush, result, font, layoutRectangle, format);
		m_isEMFPlus = false;
		m_isUseFontSize = false;
		if (num2)
		{
			Restore(state);
		}
		EndMarkContent();
	}

	private bool HasOtfGlyphList(PdfStringLayoutResult result)
	{
		int i = 0;
		for (int num = result.Lines.Length; i < num; i++)
		{
			if (result.Lines[i].OpenTypeGlyphList != null)
			{
				return true;
			}
		}
		return false;
	}

	private float GetAscent(PdfFont pdfFont)
	{
		float result = 0f;
		if (IsDirectPDF)
		{
			if (pdfFont.Ascent != 0f)
			{
				result = pdfFont.Ascent;
			}
			else if (pdfFont is PdfTrueTypeFont { TtfReader: not null } pdfTrueTypeFont && pdfTrueTypeFont.TtfReader.m_CellAscent != 0 && pdfTrueTypeFont.TtfReader.m_EmHeight != 0 && pdfTrueTypeFont.Name != null && pdfTrueTypeFont.Name != "Cambria Math")
			{
				int emHeight = pdfTrueTypeFont.TtfReader.m_EmHeight;
				float num = 0f;
				num = (((pdfTrueTypeFont.TtfReader.m_fsSelection & 0x80) == 0) ? ((float)pdfTrueTypeFont.TtfReader.m_CellAscent) : ((float)pdfTrueTypeFont.TtfReader.m_stypoAscent));
				result = (float)((double)(pdfTrueTypeFont.Size * num) / (double)emHeight);
			}
			else
			{
				result = pdfFont.Metrics.GetAscent(null);
			}
		}
		return result;
	}

	private float GetLineIndent(LineInfo lineInfo, PdfStringFormat format, RectangleF layoutBounds, bool firstLine)
	{
		float result = 0f;
		bool flag = (lineInfo.LineType & LineType.FirstParagraphLine) > LineType.None;
		if (format != null && flag)
		{
			result = (firstLine ? format.FirstLineIndent : format.m_paragraphIndent);
			result = ((layoutBounds.Width > 0f) ? Math.Min(layoutBounds.Width, result) : result);
		}
		return result;
	}

	private bool RightToLeft(PdfStringFormat format)
	{
		bool result = format?.RightToLeft ?? false;
		if (format != null && format.TextDirection != 0)
		{
			result = true;
		}
		return result;
	}

	internal RectangleF GetLineBounds(int lineIndex, PdfStringLayoutResult result, PdfFont font, RectangleF layoutRectangle, PdfStringFormat format)
	{
		if (result == null)
		{
			throw new ArgumentNullException("result");
		}
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		RectangleF result2 = RectangleF.Empty;
		if (!result.Empty && lineIndex < result.LineCount && lineIndex >= 0)
		{
			LineInfo lineInfo = result.Lines[lineIndex];
			float y = GetTextVerticalAlignShift(result.ActualSize.Height, layoutRectangle.Height, format) + layoutRectangle.Y + result.LineHeight * (float)lineIndex;
			float width = lineInfo.Width;
			float horizontalAlignShift = GetHorizontalAlignShift(width, layoutRectangle.Width, format);
			float lineIndent = GetLineIndent(lineInfo, format, layoutRectangle, lineIndex == 0);
			horizontalAlignShift += ((!RightToLeft(format)) ? lineIndent : 0f);
			float x = layoutRectangle.X + horizontalAlignShift;
			float width2 = ((!ShouldJustify(lineInfo, layoutRectangle.Width, format)) ? (width - lineIndent) : (layoutRectangle.Width - lineIndent));
			float lineHeight = result.LineHeight;
			result2 = new RectangleF(x, y, width2, lineHeight);
		}
		return result2;
	}

	internal void SetBBox(RectangleF bounds)
	{
		m_streamWriter.GetStream()["BBox"] = PdfArray.FromRectangle(bounds);
	}

	private string SetArtifact(PdfArtifact artifact)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (artifact.ArtifactType != PdfArtifactType.None)
		{
			stringBuilder.Append("/Type /" + artifact.ArtifactType.ToString() + " ");
		}
		if (artifact.BoundingBox != RectangleF.Empty)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			stringBuilder.Append("/BBox [" + artifact.BoundingBox.X.ToString(invariantCulture) + " " + artifact.BoundingBox.Y.ToString(invariantCulture) + " " + artifact.BoundingBox.Width.ToString(invariantCulture) + " " + artifact.BoundingBox.Height.ToString(invariantCulture) + "] ");
		}
		if (artifact.Attached != null)
		{
			stringBuilder.Append("/Attached [" + GetEdges(artifact.Attached) + "]");
		}
		if (artifact.SubType != PdfArtifactSubType.None)
		{
			stringBuilder.Append("/Subtype /" + artifact.SubType);
		}
		return stringBuilder.ToString();
	}

	private string GetEdges(PdfAttached attached)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (attached.Bottom)
		{
			stringBuilder.Append("/Bottom ");
		}
		if (attached.Top)
		{
			stringBuilder.Append("/Top ");
		}
		if (attached.Left)
		{
			stringBuilder.Append("/Left ");
		}
		if (attached.Right)
		{
			stringBuilder.Append("/Right ");
		}
		return stringBuilder.ToString();
	}

	protected void OnStructElementChanged(PdfTag tag)
	{
		this.StructElementChanged?.Invoke(tag);
	}

	internal void ApplyTag(PdfTag tag)
	{
		PdfStructTreeRoot structTreeRoot = PdfCatalog.StructTreeRoot;
		if (tag is PdfStructureElement)
		{
			int num = structTreeRoot.Add(tag as PdfStructureElement, Page, tag.Bounds);
			string abbrevation = (Tag as PdfStructureElement).Abbrevation;
			StreamWriter.WriteTag(string.Format("/{0} <</E (" + abbrevation + ") /MCID {1} >>BDC", structTreeRoot.ConvertToEquivalentTag((Tag as PdfStructureElement).TagType), num));
		}
		else if (tag is PdfArtifact)
		{
			string arg = SetArtifact(tag as PdfArtifact);
			StreamWriter.Write(string.Format("/Artifact << {0} >>BDC" + Environment.NewLine, arg));
		}
	}

	internal void DrawString(string text, PdfFont font, PdfBrush brush, RectangleF rect, PdfStringFormat format, bool directConversion)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		bool flag = false;
		if (font is PdfTrueTypeFont { InternalFont: UnicodeTrueTypeFont { SkipFontEmbed: false } internalFont })
		{
			internalFont.m_isClearUsedChars = true;
		}
		SizeF textSize = rect.Size;
		float num = 1f;
		num = ScaleText(text, font, rect, out textSize, format);
		m_isEMFPlus = true;
		if (num != 1f)
		{
			if (format == null)
			{
				format = new PdfStringFormat();
			}
			format.HorizontalScalingFactor = num * 100f;
		}
		OnDrawPrimitive();
		rect.Width /= num;
		if (rect.Width == 0f)
		{
			rect.Width = textSize.Width;
		}
		else if (rect.Height == 0f)
		{
			rect.Height = textSize.Height;
		}
		if (font.Name.ToLower().Contains("calibri"))
		{
			m_isUseFontSize = true;
		}
		format.LineLimit = false;
		format.NoClip = true;
		if (rect.Width == 0f && PdfString.IsUnicode(text) && font is PdfTrueTypeFont)
		{
			font.MeasureString(text, format);
			flag = (font as PdfTrueTypeFont).IsContainsFont;
		}
		if (rect.Width > 0f || flag)
		{
			DrawString(text, font, brush, rect, format);
		}
	}

	private bool IsContainFont(PdfTrueTypeFont font, string text, PdfStringFormat format)
	{
		if (IsOpenTypeFont(format, font))
		{
			OtfGlyphInfoList glyphList = null;
			ScriptTags[] tags = ObtainTags(text);
			font.GetLineWidth(text, format, out glyphList, tags);
		}
		else
		{
			font.GetLineWidth(text, null);
		}
		return font.IsContainsFont;
	}

	private bool IsOpenTypeFont(PdfStringFormat format, PdfTrueTypeFont font)
	{
		bool result = false;
		if (format != null && format.ComplexScript && font.InternalFont is UnicodeTrueTypeFont)
		{
			result = font.TtfReader.isOTFFont();
		}
		return result;
	}

	private ScriptTags[] ObtainTags(string line)
	{
		Dictionary<ScriptTags, int> dictionary = new Dictionary<ScriptTags, int>();
		LanguageUtil languageUtil = new LanguageUtil();
		for (int i = 0; i < line.Length; i++)
		{
			ScriptTags language = languageUtil.GetLanguage(line[i]);
			ScriptTags scriptTags = language;
			if (scriptTags != 0)
			{
				scriptTags = languageUtil.GetGlyphTag(line[i]);
			}
			if (language != 0 && !ScriptTags.Common.Equals(scriptTags) && !ScriptTags.Unknown.Equals(scriptTags) && !ScriptTags.Inherited.Equals(scriptTags))
			{
				if (dictionary.ContainsKey(language))
				{
					dictionary[language]++;
				}
				else
				{
					dictionary.Add(language, 1);
				}
			}
		}
		ScriptTags[] array = new ScriptTags[dictionary.Count];
		dictionary.Keys.CopyTo(array, 0);
		return array;
	}

	private float ScaleText(string text, PdfFont pdfFont, RectangleF rect, out SizeF textSize, PdfStringFormat format)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (pdfFont == null)
		{
			throw new ArgumentNullException("pdfFont");
		}
		float result = 1f;
		textSize = rect.Size;
		if (text.Length > 0)
		{
			if (format == null)
			{
				format = new PdfStringFormat();
			}
			if (text.EndsWith(" "))
			{
				format.MeasureTrailingSpaces = true;
			}
			textSize = pdfFont.MeasureString(text, format);
			if (rect.Width > 0f && rect.Width < 2.1474836E+09f && textSize.Width > rect.Width && textSize.Width > 0f)
			{
				result = rect.Width / textSize.Width;
			}
		}
		return result;
	}

	internal void ResetClip()
	{
		ClipBounds = DocIOPageBounds;
		RealClip = RectangleF.Empty;
		m_stateChanged = true;
		m_textClip = RectangleF.Empty;
		m_clipPath = false;
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

	internal void SetClip(RectangleF rect, CombineMode mode)
	{
		RealClip = rect;
		m_textClip = rect;
		SetClip();
	}

	private void OnDrawPrimitive()
	{
		if (m_stateChanged)
		{
			InternalResetClip();
			m_bFirstCall = false;
			PutComment("OnDrawPrimitive");
			Save();
			SetPdfClipPath();
			m_stateChanged = false;
		}
	}

	internal PdfStringFormat ConvertFormat(StringFormat format)
	{
		return ConvertFormat(format, isComplexScript: false);
	}

	internal PdfStringFormat ConvertFormat(StringFormat format, bool isComplexScript)
	{
		PdfStringFormat pdfStringFormat = null;
		if (format != null)
		{
			PutComment("String Format Flags: " + format.FormatFlags.ToString() + "(" + (int)format.FormatFlags + ")");
			PutComment("Alignment: " + format.Alignment);
			PutComment("Line Alignment: " + format.LineAlignment);
			pdfStringFormat = new PdfStringFormat();
			pdfStringFormat.LineLimit = false;
			pdfStringFormat.Alignment = ConvertAlingnmet(format.Alignment);
			pdfStringFormat.LineAlignment = ConvertLineAlignment(format.LineAlignment);
			pdfStringFormat.ComplexScript = isComplexScript;
			pdfStringFormat.NoClip = true;
			pdfStringFormat.TextDirection = (((format.FormatFlags & StringFormatFlags.DirectionRightToLeft) != 0) ? PdfTextDirection.RightToLeft : PdfTextDirection.None);
			pdfStringFormat.isCustomRendering = true;
			if (pdfStringFormat.NoClip)
			{
				pdfStringFormat.LineLimit = false;
			}
			pdfStringFormat.WordWrap = GetWrapType(format.FormatFlags);
		}
		return pdfStringFormat;
	}

	private PdfVerticalAlignment ConvertLineAlignment(StringAlignment stringAlignment)
	{
		PdfVerticalAlignment pdfVerticalAlignment = PdfVerticalAlignment.Middle;
		return stringAlignment switch
		{
			StringAlignment.Far => PdfVerticalAlignment.Bottom, 
			StringAlignment.Near => PdfVerticalAlignment.Top, 
			_ => PdfVerticalAlignment.Middle, 
		};
	}

	private PdfWordWrapType GetWrapType(StringFormatFlags stringFormatFlags)
	{
		PdfWordWrapType result = PdfWordWrapType.Word;
		if ((stringFormatFlags & StringFormatFlags.NoWrap) != 0)
		{
			result = PdfWordWrapType.None;
		}
		return result;
	}

	private PdfTextAlignment ConvertAlingnmet(StringAlignment stringAlignment)
	{
		PdfTextAlignment pdfTextAlignment = PdfTextAlignment.Left;
		return stringAlignment switch
		{
			StringAlignment.Far => PdfTextAlignment.Right, 
			StringAlignment.Center => PdfTextAlignment.Center, 
			_ => PdfTextAlignment.Left, 
		};
	}

	private void TranslateTransform(float dx, float dy, MatrixOrder order)
	{
		PutComment("TranslateTransform");
		m_isTranslate = true;
		if (order == MatrixOrder.Append)
		{
			if (dy < 0f)
			{
				TranslateTransform(dx, dy);
				m_translateTransform = new SizeF(0f - dx, 0f - dy);
			}
			else if (dx > dy)
			{
				TranslateTransform(dx, dy);
				m_translateTransform = new SizeF(dx, dy);
			}
			else if (dx < 0f && dx < dy)
			{
				TranslateTransform(dx, dy);
				m_translateTransform = new SizeF(dx, UpdateY(dy));
			}
			else
			{
				TranslateTransform(dx, UpdateY(dy));
			}
		}
		else
		{
			Transform = Transform;
		}
	}

	internal void RotateTransform(float angle, MatrixOrder order)
	{
		PutComment("RotateTransform");
		if (order == MatrixOrder.Append)
		{
			RotateTransform(angle);
			return;
		}
		PdfTransformationMatrix pdfTransformationMatrix = new PdfTransformationMatrix();
		PdfTransformationMatrix rotateTransform = GetRotateTransform(angle, pdfTransformationMatrix);
		Matrix.Multiply(pdfTransformationMatrix);
		Transform = rotateTransform.Matrix;
	}

	private PdfGraphicsUnit GraphicsToPrintUnits(GraphicsUnit gUnits)
	{
		return gUnits switch
		{
			GraphicsUnit.Display => PdfGraphicsUnit.Pixel, 
			GraphicsUnit.Document => PdfGraphicsUnit.Document, 
			GraphicsUnit.Inch => PdfGraphicsUnit.Inch, 
			GraphicsUnit.Millimeter => PdfGraphicsUnit.Millimeter, 
			GraphicsUnit.Pixel => PdfGraphicsUnit.Pixel, 
			GraphicsUnit.Point => PdfGraphicsUnit.Point, 
			_ => PdfGraphicsUnit.Point, 
		};
	}

	private void InternalResetClip()
	{
		if (!m_bFirstCall)
		{
			PutComment("InternalResetClip");
			m_bFirstCall = true;
			Restore();
			m_stateChanged = true;
		}
	}

	private void SetTransform()
	{
		InternalResetClip();
		PutComment("SetTransform");
		m_clipPath = false;
		if (!m_bFirstTransform && !m_stateRestored)
		{
			Restore();
		}
		else
		{
			m_bFirstTransform = false;
			m_stateRestored = false;
		}
		Save();
	}

	internal void ResetTransform()
	{
		PutComment("ResetTransform");
		m_clipPath = false;
		InternalResetClip();
		SetTransform();
		m_pageUnitScaleSize = new SizeF(1f, 1f);
		RealClip = RectangleF.Empty;
	}

	private void SetClip()
	{
		m_stateChanged = true;
	}

	private void SetPdfClipPath()
	{
		PdfPath pdfPath = null;
		if (RealClip.X != 0f || RealClip.Y != 0f || RealClip.Width != 0f || RealClip.Height != 0f)
		{
			pdfPath = new PdfPath();
			pdfPath.AddRectangle(RealClip);
			RealClip = RectangleF.Empty;
		}
		if (pdfPath != null && pdfPath.PointCount > 0)
		{
			PointF[] pathPoints = pdfPath.PathPoints;
			byte[] pathTypes = pdfPath.PathTypes;
			PdfFillMode mode = PdfFillMode.Winding;
			PdfPath path = new PdfPath(pathPoints, pathTypes);
			SetClip(path, mode);
		}
	}

	private void InternalResetTransformation()
	{
		PdfTransformationMatrix pdfTransformationMatrix = new PdfTransformationMatrix();
		if (m_bFirstTransform)
		{
			return;
		}
		PutComment("InternalResetTransformation");
		if (m_multiplyTransform != null)
		{
			if (m_multiplyTransform.Elements[0] != pdfTransformationMatrix.Matrix.Elements[0] || m_multiplyTransform.Elements[1] != pdfTransformationMatrix.Matrix.Elements[1] || m_multiplyTransform.Elements[2] != pdfTransformationMatrix.Matrix.Elements[2] || m_multiplyTransform.Elements[3] != pdfTransformationMatrix.Matrix.Elements[3] || !m_isTranslate)
			{
				Restore();
			}
		}
		else if (m_stateChanged && m_bFirstCall && !m_stateRestored)
		{
			Restore();
		}
		m_bFirstTransform = true;
	}

	internal int DirectPDFSave()
	{
		PutComment("Save");
		InternalResetClip();
		InternalResetTransformation();
		PdfGraphicsState value = Save();
		if (m_graphicsStates == null)
		{
			m_graphicsStates = new Dictionary<int, PdfGraphicsState>();
			m_GraphicsStateCount = 0;
		}
		m_GraphicsStateCount++;
		m_graphicsStates.Add(m_GraphicsStateCount, value);
		return m_GraphicsStateCount;
	}

	internal void Restore(int gState)
	{
		PutComment("Restore");
		if (m_graphicsStates == null)
		{
			m_graphicsStates = new Dictionary<int, PdfGraphicsState>();
		}
		PdfGraphicsState pdfGraphicsState = m_graphicsStates[gState];
		if (pdfGraphicsState != null)
		{
			Restore(pdfGraphicsState);
		}
		m_stateChanged = true;
		m_bFirstCall = true;
		m_bFirstTransform = false;
		m_stateRestored = true;
	}

	internal void Clear(PdfColor color)
	{
		DrawRectangle(new PdfSolidBrush(color), m_clipBounds);
	}

	internal void Clear()
	{
		DrawRectangle(PdfBrushes.White, m_clipBounds);
	}

	internal void SetTag(PdfTag element)
	{
		Tag = element;
		if (Tag is PdfArtifact)
		{
			string arg = SetArtifact(Tag as PdfArtifact);
			StreamWriter.Write(string.Format("/Artifact << {0} >>BDC" + Environment.NewLine, arg));
			Save();
		}
		else if (Tag != null)
		{
			(Tag as PdfStructureElement).m_isActiveSetTag = true;
			ApplyTag(Tag);
		}
		m_artifactBMCAdded = true;
	}

	internal void ReSetTag()
	{
		if (Tag != null)
		{
			if (Tag is PdfStructureElement)
			{
				PdfStructureElement obj = Tag as PdfStructureElement;
				obj.IsAdded = false;
				obj.m_isActiveSetTag = false;
			}
			if (Tag is PdfArtifact)
			{
				Restore();
			}
			Tag = null;
			m_streamWriter.WriteTag("EMC");
			m_artifactBMCAdded = false;
		}
	}

	internal PdfImage GetImage(Stream stream, PdfDocument document)
	{
		PdfImage pdfImage;
		if (OptimizeIdenticalImages)
		{
			if (!stream.IsJpeg() && !stream.IsPng())
			{
				throw new PdfException("Only JPEG and PNG images are supported");
			}
			stream.Position = 0L;
			MemoryStream memoryStream = stream as MemoryStream;
			if (memoryStream == null)
			{
				byte[] array = new byte[stream.Length];
				stream.Read(array, 0, array.Length);
				memoryStream = new MemoryStream(array);
			}
			memoryStream.Position = 0L;
			bool flag = false;
			PdfPage pdfPage = Page as PdfPage;
			string text = null;
			if (pdfPage != null && pdfPage.Document != null)
			{
				stream.Position = 0L;
				byte[] array2 = new byte[(int)stream.Length];
				stream.Read(array2, 0, array2.Length);
				stream.Position = 0L;
				text = pdfPage.Document.CreateHashFromStream(array2);
				flag = pdfPage.Document.ImageCollection.ContainsKey(text);
			}
			if (!string.IsNullOrEmpty(text))
			{
				if (flag)
				{
					pdfImage = document.ImageCollection[text];
				}
				else
				{
					pdfImage = new PdfBitmap(memoryStream);
					document.ImageCollection.Add(text, pdfImage);
				}
			}
			else
			{
				pdfImage = new PdfBitmap(memoryStream);
			}
		}
		else
		{
			pdfImage = new PdfBitmap(stream);
		}
		return pdfImage;
	}

	internal int ObtainGraphicsRotation(PdfArray matrix)
	{
		int num = 0;
		num = (int)Math.Round(Math.Atan2((matrix[2] as PdfNumber).FloatValue, (matrix[0] as PdfNumber).FloatValue) * 180.0 / Math.PI);
		switch (num)
		{
		case -90:
			num = 90;
			break;
		case -180:
			num = 180;
			break;
		case 90:
			num = 270;
			break;
		}
		return num;
	}

	private PointF ModifyLocation(PdfArray matrix, SizeF size, PointF location)
	{
		int num = ObtainGraphicsRotation(matrix);
		PointF result = location;
		new PdfTransformationMatrix();
		switch (num)
		{
		case 90:
			result.X += size.Height;
			result.Y += size.Width - size.Height;
			break;
		case 270:
			result.Y += 0f - size.Height;
			break;
		case 180:
			result.X += size.Width;
			result.Y += 0f - size.Height;
			break;
		}
		return result;
	}
}
