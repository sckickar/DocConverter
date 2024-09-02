using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using SkiaSharp;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;
using DocGen.PdfViewer.Base;

namespace DocGen.Pdf.Redaction;

internal class PdfImageRendererNet
{
	internal PdfLoadedPage m_loadedPage;

	internal List<PageURLNet> URLDictonary = new List<PageURLNet>();

	internal List<PdfPath> PdfPaths = new List<PdfPath>();

	private float pt = 1.3333f;

	private bool m_isTrEntry;

	private bool m_isExtendedGraphicStateContainsSMask;

	internal bool IsExtendedGraphicsState;

	internal bool IsGraphicsState;

	internal GraphicsPath m_graphicspathtoclip;

	internal DocGen.Drawing.Matrix transformMatrix = new DocGen.Drawing.Matrix();

	internal PointF currentTransformLocation;

	private char[] m_symbolChars = new char[6] { '(', ')', '[', ']', '<', '>' };

	private char[] m_startText = new char[3] { '(', '[', '<' };

	private char[] m_endText = new char[3] { ')', ']', '>' };

	private PdfPageResourcesHelper m_resources;

	internal List<PdfRedaction> redactions;

	internal Dictionary<string, SKPaint> skpaintCache = new Dictionary<string, SKPaint>();

	private PdfRecordCollection m_contentElements;

	private PointF m_currentLocation = PointF.Empty;

	private bool m_beginText;

	private GraphicsPath m_path;

	private List<GraphicsPath> m_subPaths = new List<GraphicsPath>();

	private List<GraphicsPath> m_tempSubPaths = new List<GraphicsPath>();

	private RectangleF m_clipRectangle;

	private float m_mitterLength;

	private GraphicsHelper m_graphics;

	private Stack<DocGen.Pdf.Graphics.GraphicsState> m_graphicsState = new Stack<DocGen.Pdf.Graphics.GraphicsState>();

	internal Stack<GraphicObjectDataHelperNet> m_objects = new Stack<GraphicObjectDataHelperNet>();

	private float m_textScaling = 100f;

	private bool textMatrix;

	private float m_textElementWidth;

	private string[] m_backupColorElements;

	private string m_backupColorSpace;

	private PointF m_endTextPosition;

	private bool m_isCurrentPositionChanged;

	internal bool isFindText;

	private PdfBrush transperentStrokingBrush;

	private PdfBrush transperentNonStrokingBrush;

	private PdfViewerExceptions exception = new PdfViewerExceptions();

	private DocGen.PdfViewer.Base.DeviceCMYK decodecmykColor;

	private string[] m_dashedLine;

	private bool isNegativeFont;

	private string m_clippingPath;

	private float m_lineCap;

	private int RenderingMode;

	private float m_opacity;

	private List<RectangleF> m_clipRectangleList = new List<RectangleF>();

	private bool IsTransparentText;

	private List<CffGlyphs> m_glyphDataCollection = new List<CffGlyphs>();

	private DocGen.Drawing.Matrix m_imageCommonMatrix;

	internal char findpath;

	private bool isScaledText;

	private float currentPageHeight;

	internal bool isBlack;

	private Dictionary<SystemFontFontDescriptor, SystemFontOpenTypeFontSource> testdict = new Dictionary<SystemFontFontDescriptor, SystemFontOpenTypeFontSource>();

	private PdfDictionary m_inlineParameters = new PdfDictionary();

	private List<string> BIparameter = new List<string>();

	internal List<DocGen.PdfViewer.Base.Glyph> imageRenderGlyphList;

	internal float pageRotation;

	internal float zoomFactor = 1f;

	private bool isNextFill;

	private bool isWinAnsiEncoding;

	private int m_tilingType;

	internal List<int[]> elementRange = new List<int[]>();

	internal List<RectangleF> RedactionBounds = new List<RectangleF>();

	private List<string> decodedStringData = new List<string>();

	private FontStructureHelperBase fontStructure;

	internal bool isExportAsImage;

	internal Dictionary<string, string> substitutedFontsList = new Dictionary<string, string>();

	private double det = 6.7500003375002535;

	private double m03 = 0.44444444444443115;

	private double m12 = -0.22222218888888887;

	private double pow1 = 0.037037037037037;

	private double pow2 = 0.296296296296296;

	private int recordCount;

	private DocGen.PdfViewer.Base.Matrix XFormsMatrix;

	private DocGen.PdfViewer.Base.Matrix m_d1Matrix;

	private Matrixx m_d0Matrix;

	private DocGen.PdfViewer.Base.Matrix m_type3TextLineMatrix;

	private float m_type3FontScallingFactor;

	private PdfRecordCollection m_type3RecordCollection;

	private bool m_isType3Font;

	private TransformationStack m_transformations;

	private FontStructureHelperNet m_type3FontStruct;

	private float m_spacingWidth;

	private string m_type3GlyphID;

	private float m_type3WhiteSpaceWidth;

	private Dictionary<string, List<List<int>>> m_type3GlyphPath;

	private bool m_istype3FontContainCTRM;

	private MemoryStream m_outStream;

	private const int PathTypesValuesMask = 15;

	private bool isContainsRedactionText;

	private bool isNotUpdated;

	private bool removePathLines;

	private RectangleF rectValue;

	private bool isContainsImages;

	private PathGeometry CurrentGeometry = new PathGeometry();

	private PathGeometry BackupCurrentGeometry = new PathGeometry();

	private PathFigure m_currentPath;

	private bool containsImage;

	internal int xobjectGraphicsCount;

	internal bool isXGraphics;

	private string[] clipRectShape;

	private bool isRect;

	internal float m_characterSpacing;

	internal bool m_selectablePrintDocument;

	private float m_textAngle;

	internal float m_pageHeight;

	internal bool m_isPrintSelected;

	private PointF CurrentLocation
	{
		get
		{
			return m_currentLocation;
		}
		set
		{
			m_currentLocation = value;
			m_isCurrentPositionChanged = true;
		}
	}

	private PdfBrush NonStrokingBrush
	{
		get
		{
			if (Objects.NonStrokingBrush != null)
			{
				return Objects.NonStrokingBrush;
			}
			foreach (GraphicObjectDataHelperNet @object in m_objects)
			{
				if (@object.NonStrokingBrush != null)
				{
					return @object.NonStrokingBrush;
				}
			}
			return null;
		}
	}

	private PdfBrush StrokingBrush
	{
		get
		{
			if (Objects.StrokingBrush != null)
			{
				return Objects.StrokingBrush;
			}
			foreach (GraphicObjectDataHelperNet @object in m_objects)
			{
				if (@object.StrokingBrush != null)
				{
					return @object.StrokingBrush;
				}
			}
			return null;
		}
	}

	private RectangleF ClipRectangle
	{
		get
		{
			return m_clipRectangle;
		}
		set
		{
			m_clipRectangle = value;
		}
	}

	private float MitterLength
	{
		get
		{
			return m_objects.ToArray()[0].m_mitterLength;
		}
		set
		{
			m_objects.ToArray()[0].m_mitterLength = value;
		}
	}

	private float TextScaling
	{
		get
		{
			return m_textScaling;
		}
		set
		{
			m_textScaling = value;
		}
	}

	private GraphicsPath Path
	{
		get
		{
			return m_path;
		}
		set
		{
			m_path = value;
		}
	}

	private GraphicObjectDataHelperNet Objects => m_objects.Peek();

	private string CurrentFont
	{
		get
		{
			if (Objects.CurrentFont != null)
			{
				return Objects.CurrentFont;
			}
			string result = "";
			foreach (GraphicObjectDataHelperNet @object in m_objects)
			{
				if (@object.CurrentFont != null)
				{
					result = @object.CurrentFont;
					break;
				}
			}
			return result;
		}
		set
		{
			Objects.CurrentFont = value;
		}
	}

	private float FontSize
	{
		get
		{
			if (Objects.CurrentFont != null)
			{
				return Objects.FontSize;
			}
			float result = 0f;
			foreach (GraphicObjectDataHelperNet @object in m_objects)
			{
				if (@object.CurrentFont != null)
				{
					result = @object.FontSize;
					break;
				}
			}
			return result;
		}
		set
		{
			Objects.FontSize = value;
		}
	}

	private float TextLeading
	{
		get
		{
			if (Objects.CurrentFont != null)
			{
				return Objects.TextLeading;
			}
			float result = 0f;
			foreach (GraphicObjectDataHelperNet @object in m_objects)
			{
				if (@object.CurrentFont != null)
				{
					result = @object.TextLeading;
				}
			}
			return result;
		}
		set
		{
			Objects.TextLeading = value;
		}
	}

	private PathFigure CurrentPath
	{
		get
		{
			return m_currentPath;
		}
		set
		{
			m_currentPath = value;
		}
	}

	private DocGen.PdfViewer.Base.Matrix CTRM
	{
		get
		{
			return Objects.Ctm;
		}
		set
		{
			Objects.Ctm = value;
		}
	}

	private DocGen.PdfViewer.Base.Matrix TextLineMatrix
	{
		get
		{
			return Objects.textLineMatrix;
		}
		set
		{
			Objects.textLineMatrix = value;
		}
	}

	private DocGen.PdfViewer.Base.Matrix TextMatrix
	{
		get
		{
			return Objects.textMatrix;
		}
		set
		{
			Objects.textMatrix = value;
		}
	}

	private DocGen.PdfViewer.Base.Matrix DocumentMatrix
	{
		get
		{
			return Objects.documentMatrix;
		}
		set
		{
			Objects.documentMatrix = value;
		}
	}

	private DocGen.PdfViewer.Base.Matrix TextMatrixUpdate
	{
		get
		{
			return Objects.textMatrixUpdate;
		}
		set
		{
			Objects.textMatrixUpdate = value;
		}
	}

	private DocGen.Drawing.Matrix Drawing2dMatrixCTM
	{
		get
		{
			return Objects.drawing2dMatrixCTM;
		}
		set
		{
			Objects.drawing2dMatrixCTM = value;
		}
	}

	private float HorizontalScaling
	{
		get
		{
			return Objects.HorizontalScaling;
		}
		set
		{
			Objects.HorizontalScaling = value;
		}
	}

	private int Rise
	{
		get
		{
			return Objects.Rise;
		}
		set
		{
			Objects.Rise = value;
		}
	}

	private DocGen.PdfViewer.Base.Matrix TransformMatrixTM
	{
		get
		{
			return Objects.transformMatrixTM;
		}
		set
		{
			Objects.transformMatrixTM = value;
		}
	}

	internal PdfImageRendererNet(PdfRecordCollection contentElements, PdfPageResourcesHelper resources, GraphicsHelper g, bool newPage, float pageBottom, float left, DocGen.PdfViewer.Base.DeviceCMYK cmyk)
	{
		GraphicObjectDataHelperNet graphicObjectDataHelperNet = new GraphicObjectDataHelperNet();
		graphicObjectDataHelperNet.Ctm = DocGen.PdfViewer.Base.Matrix.Identity;
		graphicObjectDataHelperNet.Ctm.Translate((float)((double)g.Transform.Elements[4] / 1.333), (float)((double)g.Transform.Elements[5] / 1.333));
		graphicObjectDataHelperNet.drawing2dMatrixCTM = new DocGen.Drawing.Matrix(1f, 0f, 0f, 1f, 0f, 0f);
		graphicObjectDataHelperNet.drawing2dMatrixCTM.Translate((float)((double)g.Transform.Elements[4] / 1.333), (float)((double)g.Transform.Elements[5] / 1.333));
		DocGen.Drawing.Matrix transform = g.Transform;
		graphicObjectDataHelperNet.documentMatrix = new DocGen.PdfViewer.Base.Matrix(1.33333333333333 * (double)(g.DpiX / 96f) * (double)transform.Elements[0], 0.0, 0.0, -1.33333333333333 * (double)(g.DpiX / 96f) * (double)transform.Elements[3], 0.0, pageBottom * transform.Elements[3]);
		m_objects.Push(graphicObjectDataHelperNet);
		m_objects.Push(graphicObjectDataHelperNet);
		m_contentElements = contentElements;
		m_resources = resources;
		m_graphics = g;
		m_graphics.SmoothingMode = SmoothingMode.AntiAlias;
		g.PageUnit = GraphicsUnit.Point;
		currentPageHeight = pageBottom;
		if (newPage)
		{
			g.TranslateTransform(left, pageBottom);
		}
		decodecmykColor = cmyk;
		imageRenderGlyphList = new List<DocGen.PdfViewer.Base.Glyph>();
	}

	private PdfImageRendererNet(PdfRecordCollection contentElements, PdfPageResourcesHelper resources, GraphicsHelper g, bool newPage, DocGen.PdfViewer.Base.DeviceCMYK cmyk, float pageHeight)
	{
		GraphicObjectDataHelperNet graphicObjectDataHelperNet = new GraphicObjectDataHelperNet();
		graphicObjectDataHelperNet.m_mitterLength = 1f;
		graphicObjectDataHelperNet.Ctm = DocGen.PdfViewer.Base.Matrix.Identity;
		graphicObjectDataHelperNet.Ctm.Translate((float)((double)g.Transform.Elements[4] / 1.333), (float)((double)g.Transform.Elements[5] / 1.333));
		graphicObjectDataHelperNet.drawing2dMatrixCTM = new DocGen.Drawing.Matrix(1f, 0f, 0f, 1f, 0f, 0f);
		graphicObjectDataHelperNet.drawing2dMatrixCTM.Translate((float)((double)g.Transform.Elements[4] / 1.333), (float)((double)g.Transform.Elements[5] / 1.333));
		DocGen.Drawing.Matrix transform = g.Transform;
		graphicObjectDataHelperNet.documentMatrix = new DocGen.PdfViewer.Base.Matrix(1.33333333333333 * (double)(g.DpiX / 96f) * (double)transform.Elements[0], 0.0, 0.0, -1.33333333333333 * (double)(g.DpiX / 96f) * (double)transform.Elements[3], 0.0, pageHeight * transform.Elements[3]);
		m_objects.Push(graphicObjectDataHelperNet);
		m_objects.Push(graphicObjectDataHelperNet);
		m_contentElements = contentElements;
		m_resources = resources;
		m_graphics = g;
		m_graphics.SmoothingMode = SmoothingMode.AntiAlias;
		g.PageUnit = GraphicsUnit.Point;
		currentPageHeight = pageHeight;
		if (newPage)
		{
			g.TranslateTransform(0f, g.ClipBounds.Bottom);
		}
		decodecmykColor = cmyk;
		imageRenderGlyphList = new List<DocGen.PdfViewer.Base.Glyph>();
		m_type3GlyphPath = new Dictionary<string, List<List<int>>>();
	}

	internal PdfImageRendererNet(int recordCount)
	{
		this.recordCount = recordCount;
	}

	private DocGen.Drawing.Matrix SetMatrix(double a, double b, double c, double d, double e, double f)
	{
		if ((m_isType3Font && m_isTrEntry) || (isWinAnsiEncoding && m_isType3Font))
		{
			if (e <= 0.0 || e > 1.0 || m_d1Matrix.OffsetY > 0.0)
			{
				e = Math.Round(m_d1Matrix.OffsetX / m_d1Matrix.M11);
				if (double.IsInfinity(e))
				{
					e = Math.Round(m_d1Matrix.M11 / m_d1Matrix.OffsetX);
				}
			}
			if (m_d1Matrix.OffsetY > 0.0)
			{
				f /= m_d1Matrix.OffsetY;
			}
			else if (m_d1Matrix.OffsetY < 0.0 && f % m_d1Matrix.OffsetY == 0.0)
			{
				f = 0.0 - f / m_d1Matrix.OffsetY;
			}
		}
		CTRM = new DocGen.PdfViewer.Base.Matrix(a, b, c, d, e, f) * m_objects.ToArray()[0].Ctm;
		return new DocGen.Drawing.Matrix((float)CTRM.M11, (float)CTRM.M12, (float)CTRM.M21, (float)CTRM.M22, (float)CTRM.OffsetX, (float)CTRM.OffsetY);
	}

	private void SetTextMatrix(double a, double b, double c, double d, double e, double f)
	{
		DocGen.PdfViewer.Base.Matrix textLineMatrix = (TextMatrix = new DocGen.PdfViewer.Base.Matrix
		{
			M11 = a,
			M12 = b,
			M21 = c,
			M22 = d,
			OffsetX = e,
			OffsetY = f
		});
		TextLineMatrix = textLineMatrix;
	}

	private void MoveToNextLineWithCurrentTextLeading()
	{
		MoveToNextLine(0.0, TextLeading);
	}

	private DocGen.PdfViewer.Base.Matrix GetTextRenderingMatrix(bool isPath)
	{
		DocGen.PdfViewer.Base.Matrix matrix = default(DocGen.PdfViewer.Base.Matrix);
		matrix.M11 = FontSize * (HorizontalScaling / 100f);
		matrix.M22 = (isPath ? FontSize : (0f - FontSize));
		matrix.OffsetY = (isPath ? ((float)Rise) : (FontSize + (float)Rise));
		return matrix * TextLineMatrix * CTRM;
	}

	private DocGen.PdfViewer.Base.Matrix GetTextRenderingMatrix()
	{
		DocGen.PdfViewer.Base.Matrix matrix = default(DocGen.PdfViewer.Base.Matrix);
		matrix.M11 = FontSize * (HorizontalScaling / 100f);
		matrix.M22 = 0f - FontSize;
		matrix.OffsetY = FontSize + (float)Rise;
		return matrix * CTRM;
	}

	private void MoveToNextLine(double tx, double ty)
	{
		DocGen.PdfViewer.Base.Matrix matrix = default(DocGen.PdfViewer.Base.Matrix);
		matrix.M11 = 1.0;
		matrix.M12 = 0.0;
		matrix.OffsetX = tx;
		matrix.M21 = 0.0;
		matrix.M22 = 1.0;
		matrix.OffsetY = ty;
		DocGen.PdfViewer.Base.Matrix matrix2 = matrix;
		matrix = (TextMatrix = matrix2 * TextLineMatrix);
		TextLineMatrix = matrix;
	}

	internal MemoryStream RenderAsImage()
	{
		PdfStream pdfStream = new PdfStream();
		PdfRecordCollection pdfRecordCollection = null;
		pdfRecordCollection = ((!m_isType3Font) ? m_contentElements : m_type3RecordCollection);
		if (pdfRecordCollection != null)
		{
			recordCount = pdfRecordCollection.RecordCollection.Count;
			bool flag = false;
			for (int i = 0; i < pdfRecordCollection.RecordCollection.Count; i++)
			{
				string text = pdfRecordCollection.RecordCollection[i].OperatorName;
				string[] operands = pdfRecordCollection.RecordCollection[i].Operands;
				string text2 = null;
				bool isSkip = false;
				bool changeOperator = false;
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
					GraphicObjectDataHelperNet graphicObjectDataHelperNet3 = new GraphicObjectDataHelperNet();
					if (m_objects.Count > 0)
					{
						GraphicObjectDataHelperNet graphicObjectDataHelperNet4 = m_objects.ToArray()[0];
						graphicObjectDataHelperNet3.Ctm = graphicObjectDataHelperNet4.Ctm;
						graphicObjectDataHelperNet3.m_mitterLength = graphicObjectDataHelperNet4.m_mitterLength;
						graphicObjectDataHelperNet3.textLineMatrix = graphicObjectDataHelperNet4.textLineMatrix;
						graphicObjectDataHelperNet3.documentMatrix = graphicObjectDataHelperNet4.documentMatrix;
						graphicObjectDataHelperNet3.textMatrixUpdate = graphicObjectDataHelperNet4.textMatrixUpdate;
						graphicObjectDataHelperNet3.drawing2dMatrixCTM = graphicObjectDataHelperNet4.drawing2dMatrixCTM;
						graphicObjectDataHelperNet3.HorizontalScaling = graphicObjectDataHelperNet4.HorizontalScaling;
						graphicObjectDataHelperNet3.Rise = graphicObjectDataHelperNet4.Rise;
						graphicObjectDataHelperNet3.transformMatrixTM = graphicObjectDataHelperNet4.transformMatrixTM;
						graphicObjectDataHelperNet3.CharacterSpacing = graphicObjectDataHelperNet4.CharacterSpacing;
						graphicObjectDataHelperNet3.WordSpacing = graphicObjectDataHelperNet4.WordSpacing;
						graphicObjectDataHelperNet3.m_nonStrokingOpacity = graphicObjectDataHelperNet4.m_nonStrokingOpacity;
						graphicObjectDataHelperNet3.m_strokingOpacity = graphicObjectDataHelperNet4.m_strokingOpacity;
					}
					if (isXGraphics)
					{
						xobjectGraphicsCount++;
					}
					m_objects.Push(graphicObjectDataHelperNet3);
					DocGen.Pdf.Graphics.GraphicsState item4 = m_graphics.Save();
					m_graphicsState.Push(item4);
					break;
				}
				case "Q":
					if (isXGraphics)
					{
						xobjectGraphicsCount--;
					}
					m_objects.Pop();
					if (m_graphicsState.Count > 0)
					{
						m_graphics.Restore(m_graphicsState.Pop());
					}
					m_graphicspathtoclip = null;
					IsGraphicsState = false;
					IsTransparentText = false;
					break;
				case "Tm":
				{
					float num7 = float.Parse(operands[0], NumberStyles.Float, CultureInfo.InvariantCulture);
					float num8 = float.Parse(operands[1], NumberStyles.Float, CultureInfo.InvariantCulture);
					float num9 = float.Parse(operands[2], NumberStyles.Float, CultureInfo.InvariantCulture);
					float num10 = float.Parse(operands[3], NumberStyles.Float, CultureInfo.InvariantCulture);
					float num11 = float.Parse(operands[4], NumberStyles.Float, CultureInfo.InvariantCulture);
					float num12 = float.Parse(operands[5], NumberStyles.Float, CultureInfo.InvariantCulture);
					SetTextMatrix(num7, num8, num9, num10, num11, num12);
					if (textMatrix)
					{
						m_graphics.Restore(m_graphicsState.Pop());
					}
					DocGen.Pdf.Graphics.GraphicsState item = m_graphics.Save();
					m_graphicsState.Push(item);
					m_graphics.MultiplyTransform(new DocGen.Drawing.Matrix(num7, 0f - num8, 0f - num9, num10, num11, 0f - num12));
					CurrentLocation = new DocGen.Drawing.Point(0, 0);
					textMatrix = true;
					if (isFoundText(new PointF(num11, num12)))
					{
						isContainsRedactionText = true;
						isNotUpdated = true;
					}
					if (pdfRecordCollection.RecordCollection.Count != i + 1 && !isContainsRedactionText)
					{
						switch (pdfRecordCollection.RecordCollection[i + 1].OperatorName)
						{
						case "TJ":
						case "Tj":
						case "'":
							isContainsRedactionText = true;
							isNotUpdated = true;
							break;
						}
					}
					if (!isContainsRedactionText && m_loadedPage.Size.Height == num12)
					{
						isContainsRedactionText = true;
						isNotUpdated = true;
					}
					break;
				}
				case "cm":
				{
					float num = float.Parse(operands[0], NumberStyles.Float, CultureInfo.InvariantCulture);
					float num2 = float.Parse(operands[1], NumberStyles.Float, CultureInfo.InvariantCulture);
					float num3 = float.Parse(operands[2], NumberStyles.Float, CultureInfo.InvariantCulture);
					float num4 = float.Parse(operands[3], NumberStyles.Float, CultureInfo.InvariantCulture);
					float num5 = float.Parse(operands[4], NumberStyles.Float, CultureInfo.InvariantCulture);
					float num6 = float.Parse(operands[5], NumberStyles.Float, CultureInfo.InvariantCulture);
					if (isFoundText(new PointF(num5, num6)))
					{
						isContainsRedactionText = true;
						isNotUpdated = true;
					}
					Drawing2dMatrixCTM = SetMatrix(num, num2, num3, num4, num5, num6);
					m_imageCommonMatrix = new DocGen.Drawing.Matrix(num, num2, num3, num4, num5, num6);
					break;
				}
				case "BT":
				{
					DocGen.PdfViewer.Base.Matrix textLineMatrix = (TextMatrix = DocGen.PdfViewer.Base.Matrix.Identity);
					TextLineMatrix = textLineMatrix;
					m_beginText = true;
					CurrentLocation = PointF.Empty;
					if (!isContainsRedactionText)
					{
						isContainsRedactionText = !isContainsRedactionText;
					}
					break;
				}
				case "ET":
					CurrentLocation = PointF.Empty;
					if (isScaledText)
					{
						isScaledText = false;
						m_graphics.Restore(m_graphicsState.Pop());
					}
					if (textMatrix)
					{
						m_graphics.Restore(m_graphicsState.Pop());
						textMatrix = false;
					}
					if (RenderingMode != 3)
					{
						RenderingMode = 0;
					}
					isContainsRedactionText = false;
					isNotUpdated = false;
					break;
				case "T*":
					MoveToNextLineWithCurrentTextLeading();
					DrawNewLine();
					break;
				case "TJ":
					try
					{
						if (isContainsRedactionText)
						{
							isNotUpdated = false;
							decodedStringData = new List<string>();
							imageRenderGlyphList.Clear();
							RenderTextElementWithSpacing(operands, text);
							text2 = ReplaceText(string.Join("", operands), imageRenderGlyphList, out isSkip, out changeOperator);
							if (text2 != null && operands[0].Equals(text2))
							{
								isNotUpdated = true;
							}
						}
					}
					catch (Exception)
					{
					}
					break;
				case "Tj":
					try
					{
						if (isContainsRedactionText)
						{
							isNotUpdated = false;
							decodedStringData = new List<string>();
							imageRenderGlyphList.Clear();
							RenderTextElement(operands, text);
							text2 = ReplaceText(string.Join("", operands), imageRenderGlyphList, out isSkip, out changeOperator);
							if (text2 != null && operands[0].Equals(text2))
							{
								isNotUpdated = true;
							}
						}
					}
					catch (Exception)
					{
					}
					break;
				case "'":
					try
					{
						if (isContainsRedactionText)
						{
							isNotUpdated = false;
							decodedStringData = new List<string>();
							MoveToNextLineWithCurrentTextLeading();
							DocGen.PdfViewer.Base.Matrix textRenderingMatrix = GetTextRenderingMatrix(isPath: false);
							TextMatrixUpdate = textRenderingMatrix;
							_ = DocumentMatrix;
							if (TextScaling != 100f)
							{
								DocGen.Pdf.Graphics.GraphicsState item3 = m_graphics.Save();
								m_graphicsState.Push(item3);
								m_graphics.ScaleTransform(TextScaling / 100f, 1f);
								isScaledText = true;
								CurrentLocation = new PointF(CurrentLocation.X / (TextScaling / 100f), CurrentLocation.Y);
							}
							imageRenderGlyphList.Clear();
							RenderTextElementWithLeading(operands, text);
							text2 = ReplaceText(string.Join("", operands), imageRenderGlyphList, out isSkip, out changeOperator);
							if (text2 != null && operands[0].Equals(text2))
							{
								isNotUpdated = true;
							}
						}
					}
					catch (Exception)
					{
					}
					break;
				case "Tf":
					RenderFont(operands);
					break;
				case "TD":
					CurrentLocation = new PointF(CurrentLocation.X + float.Parse(operands[0], NumberStyles.Float, CultureInfo.InvariantCulture), CurrentLocation.Y - float.Parse(operands[1], NumberStyles.Float, CultureInfo.InvariantCulture));
					MoveToNextLineWithLeading(operands);
					if (isFoundText(CurrentLocation))
					{
						isContainsRedactionText = true;
						isNotUpdated = true;
					}
					if (pdfRecordCollection.RecordCollection.Count != i + 1 && !isContainsRedactionText)
					{
						switch (pdfRecordCollection.RecordCollection[i + 1].OperatorName)
						{
						case "TJ":
						case "Tj":
						case "'":
							isContainsRedactionText = true;
							isNotUpdated = true;
							break;
						}
					}
					break;
				case "Td":
					CurrentLocation = new PointF(CurrentLocation.X + float.Parse(operands[0], NumberStyles.Float, CultureInfo.InvariantCulture), CurrentLocation.Y - float.Parse(operands[1], NumberStyles.Float, CultureInfo.InvariantCulture));
					MoveToNextLine(float.Parse(operands[0], NumberStyles.Float, CultureInfo.InvariantCulture), float.Parse(operands[1], NumberStyles.Float, CultureInfo.InvariantCulture));
					if (isFoundText(CurrentLocation))
					{
						isContainsRedactionText = true;
						isNotUpdated = true;
					}
					if (pdfRecordCollection.RecordCollection.Count != i + 1 && !isContainsRedactionText)
					{
						switch (pdfRecordCollection.RecordCollection[i + 1].OperatorName)
						{
						case "TJ":
						case "Tj":
						case "'":
							isContainsRedactionText = true;
							isNotUpdated = true;
							break;
						}
					}
					break;
				case "TL":
					SetTextLeading(float.Parse(operands[0], NumberStyles.Float, CultureInfo.InvariantCulture));
					break;
				case "Tw":
					GetWordSpacing(operands);
					break;
				case "Tc":
					GetCharacterSpacing(operands);
					break;
				case "Tz":
					GetScalingFactor(operands);
					break;
				case "Do":
					isContainsImages = true;
					if (m_isType3Font)
					{
						GetType3XObject(operands);
					}
					else
					{
						if (!m_resources.ContainsKey(operands[0].Replace("/", "")))
						{
							break;
						}
						if (m_resources[operands[0].Replace("/", "")] is XObjectElement)
						{
							string empty = string.Empty;
							XObjectElement xObjectElement = m_resources[operands[0].Replace("/", "")] as XObjectElement;
							if (!(xObjectElement.ObjectType == "Form"))
							{
								break;
							}
							PdfStream pdfStream2 = xObjectElement.XObjectDictionary as PdfStream;
							pdfStream2.Decompress();
							PdfRecordCollection contentElements = new ContentParser(pdfStream2.InternalStream.ToArray()).ReadContent();
							PageResourceLoader pageResourceLoader = new PageResourceLoader();
							PdfDictionary pdfDictionary = new PdfDictionary();
							PdfDictionary xObjectDictionary = xObjectElement.XObjectDictionary;
							PdfPageResourcesHelper pdfPageResourcesHelper = new PdfPageResourcesHelper();
							if (xObjectDictionary.ContainsKey("Resources"))
							{
								pdfDictionary = ((xObjectDictionary["Resources"] is PdfReference) ? ((xObjectDictionary["Resources"] as PdfReferenceHolder).Object as PdfDictionary) : ((!(xObjectDictionary["Resources"] is PdfReferenceHolder)) ? (xObjectDictionary["Resources"] as PdfDictionary) : ((xObjectDictionary["Resources"] as PdfReferenceHolder).Object as PdfDictionary)));
								if (pdfDictionary.ContainsKey("Pattern"))
								{
									foreach (KeyValuePair<PdfName, IPdfPrimitive> item5 in (PdfCrossTable.Dereference(pdfDictionary["Pattern"]) as PdfDictionary).Items)
									{
										if (item5.Value != null && PdfCrossTable.Dereference(item5.Value) is PdfStream { isSkip: not false } pdfStream3)
										{
											pdfStream3.isSkip = false;
										}
									}
								}
								Dictionary<string, PdfMatrix> commonMatrix = new Dictionary<string, PdfMatrix>();
								pdfPageResourcesHelper = pageResourceLoader.UpdatePageResources(pdfPageResourcesHelper, pageResourceLoader.GetImageResources(pdfDictionary, null, ref commonMatrix));
								pdfPageResourcesHelper = pageResourceLoader.UpdatePageResources(pdfPageResourcesHelper, pageResourceLoader.GetFontResources(pdfDictionary));
								pdfPageResourcesHelper = pageResourceLoader.UpdatePageResources(pdfPageResourcesHelper, pageResourceLoader.GetExtendedGraphicResources(pdfDictionary));
								pdfPageResourcesHelper = pageResourceLoader.UpdatePageResources(pdfPageResourcesHelper, pageResourceLoader.GetColorSpaceResource(pdfDictionary));
								pdfPageResourcesHelper = pageResourceLoader.UpdatePageResources(pdfPageResourcesHelper, pageResourceLoader.GetShadingResource(pdfDictionary));
								pdfPageResourcesHelper = pageResourceLoader.UpdatePageResources(pdfPageResourcesHelper, pageResourceLoader.GetPatternResource(pdfDictionary));
							}
							DocGen.PdfViewer.Base.Matrix matrix = DocGen.PdfViewer.Base.Matrix.Identity;
							if (xObjectDictionary.ContainsKey("Matrix"))
							{
								PdfArray pdfArray = new PdfArray();
								if (xObjectDictionary["Matrix"] is PdfArray && xObjectDictionary["Matrix"] is PdfArray pdfArray2)
								{
									float floatValue = (pdfArray2[0] as PdfNumber).FloatValue;
									float floatValue2 = (pdfArray2[1] as PdfNumber).FloatValue;
									float floatValue3 = (pdfArray2[2] as PdfNumber).FloatValue;
									float floatValue4 = (pdfArray2[3] as PdfNumber).FloatValue;
									float floatValue5 = (pdfArray2[4] as PdfNumber).FloatValue;
									float floatValue6 = (pdfArray2[5] as PdfNumber).FloatValue;
									matrix = new DocGen.PdfViewer.Base.Matrix(floatValue, floatValue2, floatValue3, floatValue4, floatValue5, floatValue6);
									if (floatValue5 != 0f || floatValue6 != 0f)
									{
										m_graphics.TranslateTransform(floatValue5, 0f - floatValue6);
									}
									if (floatValue != 0f || floatValue4 != 0f)
									{
										m_graphics.ScaleTransform(floatValue, floatValue4);
									}
									double num13 = Math.Acos(floatValue);
									double num14 = Math.Round(180.0 / Math.PI * num13);
									double num15 = Math.Asin(floatValue2);
									double num16 = Math.Round(180.0 / Math.PI * num15);
									if (num14 == num16)
									{
										m_graphics.RotateTransform(0f - (float)num14);
									}
									else if (!double.IsNaN(num16))
									{
										m_graphics.RotateTransform(0f - (float)num16);
									}
									else if (!double.IsNaN(num14))
									{
										m_graphics.RotateTransform(0f - (float)num14);
									}
								}
							}
							if (pdfDictionary == null)
							{
								break;
							}
							DocGen.PdfViewer.Base.DeviceCMYK cmyk = new DocGen.PdfViewer.Base.DeviceCMYK();
							if (pdfPageResourcesHelper.fontCollection.Count == 0 && m_resources.fontCollection.Count > 0)
							{
								foreach (KeyValuePair<string, FontStructureHelperBase> item6 in m_resources.fontCollection)
								{
									pdfPageResourcesHelper.Resources.Add(item6.Key, item6.Value);
									pdfPageResourcesHelper.fontCollection.Add(item6.Key, item6.Value);
								}
							}
							PdfImageRendererNet pdfImageRendererNet = new PdfImageRendererNet(contentElements, pdfPageResourcesHelper, m_graphics, newPage: false, cmyk, currentPageHeight);
							pdfImageRendererNet.RedactionBounds = RedactionBounds;
							pdfImageRendererNet.m_loadedPage = m_loadedPage;
							pdfImageRendererNet.redactions = redactions;
							pdfImageRendererNet.m_objects = m_objects;
							GraphicObjectDataHelperNet graphicObjectDataHelperNet = new GraphicObjectDataHelperNet();
							if (pdfImageRendererNet.m_objects.Count > 0)
							{
								GraphicObjectDataHelperNet graphicObjectDataHelperNet2 = pdfImageRendererNet.m_objects.ToArray()[0];
								graphicObjectDataHelperNet.Ctm = graphicObjectDataHelperNet2.Ctm;
								graphicObjectDataHelperNet.m_mitterLength = graphicObjectDataHelperNet2.m_mitterLength;
								graphicObjectDataHelperNet.textLineMatrix = graphicObjectDataHelperNet2.textLineMatrix;
								graphicObjectDataHelperNet.documentMatrix = graphicObjectDataHelperNet2.documentMatrix;
								graphicObjectDataHelperNet.textMatrixUpdate = graphicObjectDataHelperNet2.textMatrixUpdate;
								graphicObjectDataHelperNet.drawing2dMatrixCTM = graphicObjectDataHelperNet2.drawing2dMatrixCTM;
								graphicObjectDataHelperNet.HorizontalScaling = graphicObjectDataHelperNet2.HorizontalScaling;
								graphicObjectDataHelperNet.Rise = graphicObjectDataHelperNet2.Rise;
								graphicObjectDataHelperNet.transformMatrixTM = graphicObjectDataHelperNet2.transformMatrixTM;
								graphicObjectDataHelperNet.CharacterSpacing = graphicObjectDataHelperNet2.CharacterSpacing;
								graphicObjectDataHelperNet.WordSpacing = graphicObjectDataHelperNet2.WordSpacing;
								graphicObjectDataHelperNet.m_nonStrokingOpacity = graphicObjectDataHelperNet2.m_nonStrokingOpacity;
								graphicObjectDataHelperNet.m_strokingOpacity = graphicObjectDataHelperNet2.m_strokingOpacity;
							}
							pdfImageRendererNet.m_objects.Push(graphicObjectDataHelperNet);
							DocGen.Pdf.Graphics.GraphicsState item2 = m_graphics.Save();
							pdfImageRendererNet.m_graphicsState.Push(item2);
							DocGen.PdfViewer.Base.Matrix ctm = m_objects.ToArray()[0].Ctm;
							DocGen.PdfViewer.Base.Matrix ctm2 = matrix * ctm;
							m_objects.ToArray()[0].drawing2dMatrixCTM = new DocGen.Drawing.Matrix((float)ctm2.M11, (float)ctm2.M12, (float)ctm2.M21, (float)ctm2.M22, (float)ctm2.OffsetX, (float)ctm2.OffsetY);
							m_objects.ToArray()[0].Ctm = ctm2;
							if (empty != xObjectElement.ObjectName && xObjectDictionary.ContainsKey("BBox"))
							{
								new PdfArray();
								if (xObjectDictionary["BBox"] is PdfArray)
								{
									PdfArray obj = xObjectDictionary["BBox"] as PdfArray;
									float floatValue7 = (obj[0] as PdfNumber).FloatValue;
									float floatValue8 = (obj[1] as PdfNumber).FloatValue;
									float width = (obj[2] as PdfNumber).FloatValue - floatValue7;
									float height = (obj[3] as PdfNumber).FloatValue - floatValue8;
									RectangleF rectangleF = new RectangleF(floatValue7, floatValue8, width, height);
									DocGen.PdfViewer.Base.Matrix documentMatrix3 = m_objects.ToArray()[0].documentMatrix;
									DocGen.PdfViewer.Base.Matrix matrix2 = matrix * ctm * documentMatrix3;
									_ = m_graphics.Transform;
									_ = m_graphics.PageUnit;
									m_graphics.PageUnit = GraphicsUnit.Pixel;
									m_graphics.Transform = new DocGen.Drawing.Matrix(1f, 0f, 0f, 1f, 0f, 0f);
									m_graphics.Transform = new DocGen.Drawing.Matrix((float)matrix2.M11, (float)matrix2.M12, (float)matrix2.M21, (float)matrix2.M22, (float)matrix2.OffsetX, (float)matrix2.OffsetY);
									m_graphics.SetClip(rectangleF, CombineMode.Intersect);
									empty = xObjectElement.ObjectName;
								}
							}
							pdfImageRendererNet.m_selectablePrintDocument = m_isPrintSelected;
							pdfImageRendererNet.m_pageHeight = m_pageHeight;
							pdfImageRendererNet.isXGraphics = true;
							pdfImageRendererNet.substitutedFontsList = substitutedFontsList;
							pdfImageRendererNet.zoomFactor = zoomFactor;
							MemoryStream memoryStream = pdfImageRendererNet.RenderAsImage();
							pdfImageRendererNet.m_objects.Pop();
							if (pdfImageRendererNet.m_graphicsState.Count > 0)
							{
								m_graphics.Restore(pdfImageRendererNet.m_graphicsState.Pop());
							}
							pdfImageRendererNet.m_graphicspathtoclip = null;
							pdfImageRendererNet.IsGraphicsState = false;
							pdfImageRendererNet.isXGraphics = false;
							while (pdfImageRendererNet.xobjectGraphicsCount > 0)
							{
								m_objects.Pop();
								pdfImageRendererNet.xobjectGraphicsCount--;
							}
							imageRenderGlyphList.AddRange(pdfImageRendererNet.imageRenderGlyphList);
							m_objects.ToArray()[0].Ctm = ctm;
							PdfStream pdfStream4 = new PdfStream();
							pdfStream4.Data = memoryStream.ToArray();
							pdfStream4.Compress = true;
							memoryStream.Dispose();
							pdfStream2.Clear();
							pdfStream2.Items.Remove(new PdfName("Length"));
							pdfStream2.Data = pdfStream4.Data;
							pdfStream2.Compress = true;
							pdfStream2.Modify();
						}
						else
						{
							GetXObject(operands);
						}
					}
					break;
				case "re":
				{
					if (RenderingMode == 3 && transperentStrokingBrush != null && transperentStrokingBrush != null)
					{
						Objects.NonStrokingBrush = transperentNonStrokingBrush;
						Objects.StrokingBrush = transperentStrokingBrush;
					}
					if (i < pdfRecordCollection.RecordCollection.Count && pdfRecordCollection.RecordCollection[i + 1].OperatorName == "f")
					{
						isNextFill = true;
					}
					float x = float.Parse(operands[0], NumberStyles.Float, CultureInfo.InvariantCulture);
					float y = float.Parse(operands[1], NumberStyles.Float, CultureInfo.InvariantCulture);
					float width2 = float.Parse(operands[2], NumberStyles.Float, CultureInfo.InvariantCulture);
					float height2 = float.Parse(operands[3], NumberStyles.Float, CultureInfo.InvariantCulture);
					if (isFoundText(new PointF(x, y)))
					{
						isContainsRedactionText = true;
						isNotUpdated = true;
					}
					if (!flag)
					{
						rectValue = new RectangleF(x, y, width2, height2);
						flag = true;
					}
					GetClipRectangle(operands);
					break;
				}
				case "d":
					if (operands[0] != "[]" && !operands[0].Contains("\n"))
					{
						m_dashedLine = operands;
					}
					break;
				case "d0":
					m_d0Matrix = new Matrixx(float.Parse(operands[0], NumberStyles.Float, CultureInfo.InvariantCulture), float.Parse(operands[1], NumberStyles.Float, CultureInfo.InvariantCulture));
					break;
				case "d1":
					m_d1Matrix = new DocGen.PdfViewer.Base.Matrix(float.Parse(operands[0], NumberStyles.Float, CultureInfo.InvariantCulture), float.Parse(operands[1], NumberStyles.Float, CultureInfo.InvariantCulture), float.Parse(operands[2], NumberStyles.Float, CultureInfo.InvariantCulture), float.Parse(operands[3], NumberStyles.Float, CultureInfo.InvariantCulture), float.Parse(operands[4], NumberStyles.Float, CultureInfo.InvariantCulture), float.Parse(operands[5], NumberStyles.Float, CultureInfo.InvariantCulture));
					break;
				case "gs":
				{
					IsTransparentText = false;
					if (!m_resources.ContainsKey(operands[0].Substring(1)))
					{
						break;
					}
					int num17 = 0;
					string text3 = null;
					bool flag3 = true;
					PdfDictionary xObjectDictionary2 = (m_resources[operands[0].Substring(1)] as XObjectElement).XObjectDictionary;
					if (xObjectDictionary2.ContainsKey("OPM"))
					{
						num17 = (xObjectDictionary2["OPM"] as PdfNumber).IntValue;
					}
					if (xObjectDictionary2.ContainsKey("SMask"))
					{
						m_isExtendedGraphicStateContainsSMask = true;
					}
					if (xObjectDictionary2.ContainsKey("AIS"))
					{
						flag3 = (xObjectDictionary2["AIS"] as PdfBoolean).Value;
					}
					bool flag4 = false;
					if (Objects.m_nonStrokingOpacity == 0f || Objects.m_strokingOpacity == 0f)
					{
						flag4 = true;
					}
					if (xObjectDictionary2.ContainsKey("HT"))
					{
						PdfName pdfName = xObjectDictionary2["HT"] as PdfName;
						if (pdfName != null)
						{
							text3 = pdfName.Value;
						}
						else
						{
							pdfName = (xObjectDictionary2["HT"] as PdfReferenceHolder).Object as PdfName;
							if (pdfName != null)
							{
								text3 = pdfName.Value;
							}
						}
					}
					else if (xObjectDictionary2.ContainsKey("CA") || xObjectDictionary2.ContainsKey("ca"))
					{
						if (xObjectDictionary2.ContainsKey("CA"))
						{
							Objects.m_nonStrokingOpacity = (xObjectDictionary2["CA"] as PdfNumber).FloatValue;
						}
						if (xObjectDictionary2.ContainsKey("ca"))
						{
							if (flag3)
							{
								Objects.m_strokingOpacity = (xObjectDictionary2["ca"] as PdfNumber).FloatValue;
							}
							else if (!isXGraphics)
							{
								Objects.m_strokingOpacity = (xObjectDictionary2["ca"] as PdfNumber).FloatValue;
							}
						}
						if (Objects.m_nonStrokingOpacity == 0f || Objects.m_strokingOpacity == 0f)
						{
							flag4 = true;
						}
						if (flag4)
						{
							if (StrokingBrush != null && m_backupColorElements != null && m_backupColorSpace != null)
							{
								m_opacity = Objects.m_strokingOpacity;
								SetStrokingColor(GetColor(m_backupColorElements, "Stroking", m_backupColorSpace));
							}
							if (NonStrokingBrush != null && m_backupColorElements != null && m_backupColorSpace != null)
							{
								m_opacity = Objects.m_nonStrokingOpacity;
								SetNonStrokingColor(GetColor(m_backupColorElements, "NonStroking", m_backupColorSpace));
							}
						}
						IsGraphicsState = true;
					}
					else if (xObjectDictionary2.ContainsKey("TR"))
					{
						if (xObjectDictionary2["TR"].ToString().Replace("/", "") == "Identity" && num17 == 1)
						{
							m_isTrEntry = true;
						}
					}
					else if (!xObjectDictionary2.ContainsKey("TR") && num17 == 1 && xObjectDictionary2.ContainsKey("Type") && xObjectDictionary2["Type"].ToString().Replace("/", "") == "ExtGState" && num17 == 1)
					{
						m_isTrEntry = true;
					}
					if (num17 == 1 && text3 == "Default")
					{
						IsExtendedGraphicsState = true;
					}
					break;
				}
				case "n":
					BackupCurrentGeometry = CurrentGeometry;
					CurrentGeometry = new PathGeometry();
					m_currentPath = null;
					break;
				case "J":
					m_lineCap = float.Parse(operands[0], NumberStyles.Float, CultureInfo.InvariantCulture);
					break;
				case "w":
					MitterLength = float.Parse(operands[0], NumberStyles.Float, CultureInfo.InvariantCulture);
					break;
				case "W":
				{
					if (m_isType3Font)
					{
						break;
					}
					m_clippingPath = text;
					DocGen.PdfViewer.Base.Matrix documentMatrix2 = DocumentMatrix;
					DocGen.PdfViewer.Base.Matrix transform2 = new DocGen.PdfViewer.Base.Matrix(Drawing2dMatrixCTM.Elements[0], Drawing2dMatrixCTM.Elements[1], Drawing2dMatrixCTM.Elements[2], Drawing2dMatrixCTM.Elements[3], Drawing2dMatrixCTM.OffsetX, Drawing2dMatrixCTM.OffsetY) * documentMatrix2;
					new DocGen.Drawing.Matrix((float)Math.Round(transform2.M11, 5, MidpointRounding.ToEven), (float)Math.Round(transform2.M12, 5, MidpointRounding.ToEven), (float)Math.Round(transform2.M21, 5, MidpointRounding.ToEven), (float)Math.Round(transform2.M22, 5, MidpointRounding.ToEven), (float)Math.Round(transform2.OffsetX, 5, MidpointRounding.ToEven), (float)Math.Round(transform2.OffsetY, 5, MidpointRounding.ToEven));
					_ = m_graphics.Transform;
					_ = m_graphics.PageUnit;
					m_graphics.PageUnit = GraphicsUnit.Pixel;
					m_graphics.Transform = new DocGen.Drawing.Matrix(1f, 0f, 0f, 1f, 0f, 0f);
					foreach (PathFigure figure in CurrentGeometry.Figures)
					{
						figure.IsClosed = true;
						figure.IsFilled = true;
					}
					CurrentGeometry.FillRule = FillRule.Nonzero;
					GraphicsPath geometry2 = GetGeometry(CurrentGeometry, transform2);
					bool flag2 = true;
					PointF[] pathPoints = geometry2.PathPoints;
					for (int j = 0; j < pathPoints.Length; j++)
					{
						PointF pointF = pathPoints[j];
						if (pointF.X < 0f || pointF.Y < 0f)
						{
							flag2 = false;
							break;
						}
					}
					if (geometry2.PointCount != 0 && flag2)
					{
						m_graphics.SetClip(geometry2, CombineMode.Intersect);
					}
					break;
				}
				case "W*":
				{
					m_clippingPath = text;
					DocGen.PdfViewer.Base.Matrix documentMatrix = DocumentMatrix;
					DocGen.PdfViewer.Base.Matrix transform = new DocGen.PdfViewer.Base.Matrix(Drawing2dMatrixCTM.Elements[0], Drawing2dMatrixCTM.Elements[1], Drawing2dMatrixCTM.Elements[2], Drawing2dMatrixCTM.Elements[3], Drawing2dMatrixCTM.OffsetX, Drawing2dMatrixCTM.OffsetY) * documentMatrix;
					new DocGen.Drawing.Matrix((float)Math.Round(transform.M11, 5, MidpointRounding.ToEven), (float)Math.Round(transform.M12, 5, MidpointRounding.ToEven), (float)Math.Round(transform.M21, 5, MidpointRounding.ToEven), (float)Math.Round(transform.M22, 5, MidpointRounding.ToEven), (float)Math.Round(transform.OffsetX, 5, MidpointRounding.ToEven), (float)Math.Round(transform.OffsetY, 5, MidpointRounding.ToEven));
					_ = m_graphics.Transform;
					_ = m_graphics.PageUnit;
					m_graphics.PageUnit = GraphicsUnit.Pixel;
					m_graphics.Transform = new DocGen.Drawing.Matrix(1f, 0f, 0f, 1f, 0f, 0f);
					foreach (PathFigure figure2 in CurrentGeometry.Figures)
					{
						figure2.IsClosed = true;
						figure2.IsFilled = true;
					}
					CurrentGeometry.FillRule = FillRule.EvenOdd;
					GraphicsPath geometry = GetGeometry(CurrentGeometry, transform);
					if (geometry.PointCount != 0)
					{
						m_graphics.SetClip(geometry, CombineMode.Intersect);
					}
					break;
				}
				case "m":
					BackupCurrentGeometry = CurrentGeometry;
					CurrentGeometry = new PathGeometry();
					m_currentPath = null;
					if (FindRedactpath(pdfRecordCollection, i))
					{
						removePathLines = true;
					}
					break;
				}
				recordCount = pdfRecordCollection.RecordCollection.Count;
				if (!changeOperator)
				{
					text2 = null;
				}
				if (!isSkip && !removePathLines)
				{
					OptimizeContent(pdfRecordCollection, i, text2, pdfStream);
				}
				if (removePathLines && (text.Trim() == "h" || text.Trim() == "f" || text.Trim() == "f*"))
				{
					string operatorName = pdfRecordCollection.RecordCollection[i + 1].OperatorName;
					if (text.Trim() == "h" && (operatorName != "f" || operatorName != "f*"))
					{
						removePathLines = false;
					}
					else if (text.Trim() == "f" || text.Trim() == "f*")
					{
						removePathLines = false;
					}
				}
			}
			pdfStream.Write("\r\n");
			m_isType3Font = false;
		}
		MemoryStream memoryStream2 = new MemoryStream();
		byte[] array = new byte[4096];
		pdfStream.InternalStream.Position = 0L;
		int count;
		while ((count = pdfStream.InternalStream.Read(array, 0, array.Length)) > 0)
		{
			memoryStream2.Write(array, 0, count);
		}
		pdfStream.Clear();
		pdfStream.InternalStream.Dispose();
		pdfStream.InternalStream.Close();
		pdfStream.InternalStream = null;
		return memoryStream2;
	}

	private void AddLine(string[] line)
	{
		CurrentLocation = new PointF(FloatParse(line[0]), FloatParse(line[1]));
		PointF pointF = new PointF(CurrentLocation.X, CurrentLocation.Y);
		m_currentPath.Segments.Add(new LineSegment
		{
			Point = pointF
		});
	}

	private void BeginPath(string[] point)
	{
		CurrentLocation = new PointF(FloatParse(point[0]), FloatParse(point[1]));
		if (m_currentPath != null && m_currentPath.Segments.Count == 0)
		{
			CurrentGeometry.Figures.Remove(CurrentPath);
		}
		m_currentPath = new PathFigure();
		PointF pointF = new PointF(CurrentLocation.X, CurrentLocation.Y);
		m_currentPath.StartPoint = pointF;
		CurrentGeometry.Figures.Add(m_currentPath);
	}

	private void AddBezierCurve(string[] curve)
	{
		BezierSegment bezierSegment = new BezierSegment();
		bezierSegment.Point1 = new PointF(FloatParse(curve[0]), FloatParse(curve[1]));
		bezierSegment.Point2 = new PointF(FloatParse(curve[2]), FloatParse(curve[3]));
		bezierSegment.Point3 = new PointF(FloatParse(curve[4]), FloatParse(curve[5]));
		m_currentPath.Segments.Add(bezierSegment);
		CurrentLocation = new PointF(FloatParse(curve[4]), FloatParse(curve[5]));
	}

	private void AddBezierCurve2(string[] curve)
	{
		BezierSegment bezierSegment = new BezierSegment();
		PointF pointF = new PointF(CurrentLocation.X, CurrentLocation.Y);
		bezierSegment.Point1 = pointF;
		bezierSegment.Point2 = new PointF(FloatParse(curve[0]), FloatParse(curve[1]));
		bezierSegment.Point3 = new PointF(FloatParse(curve[2]), FloatParse(curve[3]));
		m_currentPath.Segments.Add(bezierSegment);
		CurrentLocation = new PointF(FloatParse(curve[2]), FloatParse(curve[3]));
	}

	private void AddBezierCurve3(string[] curve)
	{
		BezierSegment bezierSegment = new BezierSegment();
		bezierSegment.Point1 = new PointF(FloatParse(curve[0]), FloatParse(curve[1]));
		bezierSegment.Point2 = new PointF(FloatParse(curve[2]), FloatParse(curve[3]));
		bezierSegment.Point3 = new PointF(FloatParse(curve[2]), FloatParse(curve[3]));
		m_currentPath.Segments.Add(bezierSegment);
		CurrentLocation = new PointF(FloatParse(curve[2]), FloatParse(curve[3]));
	}

	private void EndPathLine()
	{
		if (m_currentPath != null)
		{
			m_currentPath.IsClosed = true;
		}
	}

	private float FloatParse(string textString)
	{
		return float.Parse(textString, CultureInfo.InvariantCulture.NumberFormat);
	}

	internal bool FindRedactpath(PdfRecordCollection recordCollection, int i)
	{
		bool result = false;
		for (int j = i; j < recordCollection.RecordCollection.Count; j++)
		{
			string text = recordCollection.RecordCollection[j].OperatorName;
			string[] operands = recordCollection.RecordCollection[j].Operands;
			char[] symbolChars = m_symbolChars;
			for (int k = 0; k < symbolChars.Length; k++)
			{
				char c = symbolChars[k];
				if (text.Contains(c.ToString()))
				{
					text = text.Replace(c.ToString(), "");
				}
			}
			switch (text.Trim())
			{
			case "h":
			{
				DocGen.PdfViewer.Base.Matrix documentMatrix = DocumentMatrix;
				DocGen.PdfViewer.Base.Matrix transform = new DocGen.PdfViewer.Base.Matrix(Drawing2dMatrixCTM.Elements[0], Drawing2dMatrixCTM.Elements[1], Drawing2dMatrixCTM.Elements[2], Drawing2dMatrixCTM.Elements[3], Drawing2dMatrixCTM.OffsetX, Drawing2dMatrixCTM.OffsetY) * documentMatrix;
				new DocGen.Drawing.Matrix((float)Math.Round(transform.M11, 5, MidpointRounding.ToEven), (float)Math.Round(transform.M12, 5, MidpointRounding.ToEven), (float)Math.Round(transform.M21, 5, MidpointRounding.ToEven), (float)Math.Round(transform.M22, 5, MidpointRounding.ToEven), (float)Math.Round(transform.OffsetX, 5, MidpointRounding.ToEven), (float)Math.Round(transform.OffsetY, 5, MidpointRounding.ToEven));
				GraphicsPath geometry = GetGeometry(CurrentGeometry, transform);
				RectangleF bounds = geometry.GetBounds();
				bounds.X /= pt;
				bounds.Y /= pt;
				bounds.Width /= pt;
				bounds.Height /= pt;
				RectangleF empty = RectangleF.Empty;
				foreach (PdfRedaction redaction in redactions)
				{
					if (redaction.TextOnly)
					{
						continue;
					}
					empty = RectangleF.Intersect(new RectangleF(redaction.Bounds.X, redaction.Bounds.Y, redaction.Bounds.Width, redaction.Bounds.Height), bounds);
					if (empty != RectangleF.Empty)
					{
						if (empty.Contains(bounds) || empty.Equals(bounds) || ((int)empty.X == (int)bounds.X && (int)empty.Y == (int)bounds.Y && (int)empty.Width == (int)bounds.Width && (int)empty.Height == (int)bounds.Height))
						{
							return true;
						}
						PdfPath pdfPath = new PdfPath();
						RectangleF rectangle = new RectangleF(empty.X, empty.Y, empty.Width, empty.Height);
						redaction.m_success = true;
						pdfPath.AddRectangle(rectangle);
						PdfPaths.Add(pdfPath);
					}
				}
				if (geometry.PointCount != 0 && Path != null)
				{
					Path.CloseFigure();
					m_subPaths.Add(Path);
					m_tempSubPaths.Clear();
					Path = new GraphicsPath();
				}
				if (m_currentPath != null)
				{
					m_currentPath.IsClosed = true;
				}
				return false;
			}
			case "m":
				BeginPath(operands);
				break;
			case "c":
				AddBezierCurve(operands);
				break;
			case "l":
				AddLine(operands);
				break;
			case "v":
				AddBezierCurve2(operands);
				break;
			case "y":
				AddBezierCurve3(operands);
				break;
			}
		}
		return result;
	}

	internal void OptimizeContent(PdfRecordCollection recordCollection, int i, string updatedText, PdfStream stream)
	{
		PdfRecord pdfRecord = recordCollection.RecordCollection[i];
		if (pdfRecord.Operands != null && pdfRecord.Operands.Length >= 1)
		{
			if (pdfRecord.OperatorName == "ID")
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int j = 0; j < pdfRecord.Operands.Length; j++)
				{
					if (j + 1 < pdfRecord.Operands.Length && pdfRecord.Operands[j].Contains("/") && pdfRecord.Operands[j + 1].Contains("/"))
					{
						stringBuilder.Append(pdfRecord.Operands[j]);
						stringBuilder.Append(" ");
						stringBuilder.Append(pdfRecord.Operands[j + 1]);
						stringBuilder.Append("\r\n");
						j++;
					}
					else if (j + 1 < pdfRecord.Operands.Length && pdfRecord.Operands[j].Contains("/"))
					{
						stringBuilder.Append(pdfRecord.Operands[j]);
						stringBuilder.Append(" ");
						stringBuilder.Append(pdfRecord.Operands[j + 1]);
						stringBuilder.Append("\r\n");
						j++;
					}
					else
					{
						stringBuilder.Append(pdfRecord.Operands[j]);
						stringBuilder.Append("\r\n");
					}
				}
				string s = stringBuilder.ToString();
				byte[] bytes = Encoding.Default.GetBytes(s);
				stream.Write(bytes);
			}
			else
			{
				for (int k = 0; k < pdfRecord.Operands.Length; k++)
				{
					string value = pdfRecord.Operands[k];
					if ((pdfRecord.OperatorName == "Tj" || pdfRecord.OperatorName == "'" || pdfRecord.OperatorName == "\"" || pdfRecord.OperatorName == "TJ") && updatedText != null)
					{
						value = updatedText;
						if (pdfRecord.OperatorName == "'")
						{
							stream.Write("T*");
							stream.Write(" ");
						}
						pdfRecord.OperatorName = "TJ";
					}
					PdfString pdfString = new PdfString(value);
					stream.Write(pdfString.Bytes);
					if (pdfRecord.OperatorName != "Tj" && pdfRecord.OperatorName != "'" && pdfRecord.OperatorName != "\"" && pdfRecord.OperatorName != "TJ")
					{
						stream.Write(" ");
					}
				}
			}
		}
		else if (pdfRecord.Operands == null && pdfRecord.InlineImageBytes != null)
		{
			string @string = Encoding.Default.GetString(pdfRecord.InlineImageBytes);
			byte[] bytes2 = Encoding.Default.GetBytes(@string);
			stream.Write(bytes2);
			stream.Write(" ");
		}
		stream.Write(pdfRecord.OperatorName);
		if (i + 1 < recordCount)
		{
			if (pdfRecord.OperatorName == "ID")
			{
				stream.Write("\n");
			}
			else if (i + 1 < recordCount && (pdfRecord.OperatorName == "W" || pdfRecord.OperatorName == "W*") && recordCollection.RecordCollection[i + 1].OperatorName == "n")
			{
				stream.Write(" ");
			}
			else if (pdfRecord.OperatorName == "w" || pdfRecord.OperatorName == "EI")
			{
				stream.Write(" ");
			}
			else
			{
				stream.Write("\r\n");
			}
		}
	}

	private List<StringMappingNet> MapString(string[] mainTextCollection, List<DocGen.PdfViewer.Base.Glyph> imgGlyph, FontStructureHelperBase structure)
	{
		List<StringMappingNet> list = new List<StringMappingNet>();
		DocGen.PdfViewer.Base.Glyph[] array = imgGlyph.ToArray();
		int num = 0;
		for (int i = 0; i < mainTextCollection.Length; i++)
		{
			int index = mainTextCollection[i].Length - 1;
			if (mainTextCollection[i][0] != '(' && mainTextCollection[i][index] != ')')
			{
				StringMappingNet stringMappingNet = new StringMappingNet();
				stringMappingNet.text = mainTextCollection[i];
				list.Add(stringMappingNet);
				continue;
			}
			if (structure.FontEncoding == "Identity-H" || (structure.FontEncoding == "" && structure.CharacterMapTable != null && structure.CharacterMapTable.Count > 0))
			{
				StringMappingNet stringMappingNet2 = new StringMappingNet();
				stringMappingNet2.text = mainTextCollection[i];
				string text = mainTextCollection[i];
				text = text.Substring(1, text.Length - 2);
				int num2 = text.Length;
				int num3 = 0;
				if (num2 > 1)
				{
					int num4 = num;
					while (num3 < text.Length)
					{
						string toUnicode = array[num4].ToUnicode;
						if (toUnicode != null)
						{
							char[] array2 = toUnicode.ToCharArray();
							if (array2.Length != 0 && structure.CharacterMapTable.ContainsKey((int)array2[0]) && structure.FontEncoding == string.Empty)
							{
								string text2 = structure.CharacterMapTable[(int)array2[0]];
								num3 += text2.Length;
								num4++;
							}
							else
							{
								num3 += toUnicode.Length;
								num4++;
							}
						}
						else
						{
							num4++;
						}
					}
					num2 = num4 - num;
				}
				else if (num < array.Length && array[num].ToUnicode == null)
				{
					num2++;
				}
				stringMappingNet2.glyph = new DocGen.PdfViewer.Base.Glyph[num2];
				System.Array.Copy(array, num, stringMappingNet2.glyph, 0, num2);
				num += num2;
				list.Add(stringMappingNet2);
				continue;
			}
			StringMappingNet stringMappingNet3 = new StringMappingNet();
			stringMappingNet3.text = mainTextCollection[i];
			string text3 = mainTextCollection[i];
			bool flag = text3.Length >= 2;
			bool flag2 = text3.StartsWith("(");
			bool flag3 = text3.EndsWith(")");
			if (flag && flag2 && !flag3)
			{
				text3 = text3.Substring(1, text3.Length - 1);
			}
			else if (flag && !flag2 && flag3)
			{
				text3 = text3.Substring(0, text3.Length - 1);
			}
			else
			{
				if (!flag)
				{
					continue;
				}
				text3 = text3.Substring(1, text3.Length - 2);
			}
			int length = text3.Length;
			stringMappingNet3.glyph = new DocGen.PdfViewer.Base.Glyph[length];
			System.Array.Copy(array, num, stringMappingNet3.glyph, 0, length);
			num += length;
			list.Add(stringMappingNet3);
		}
		return list;
	}

	private string ReplaceText(string text, List<DocGen.PdfViewer.Base.Glyph> imageGlyph, out bool isSkip, out bool changeOperator)
	{
		if (imageGlyph.Count == 0)
		{
			isSkip = false;
			changeOperator = false;
			return text;
		}
		bool flag = false;
		bool flag2 = false;
		isSkip = false;
		changeOperator = false;
		bool isHex = false;
		if (m_resources.ContainsKey(CurrentFont))
		{
			FontStructureHelperNet fontStructureHelperNet = m_resources[CurrentFont] as FontStructureHelperNet;
			if (fontStructureHelperNet.CharacterMapTable != null && fontStructureHelperNet.CharacterMapTable.Count > 0)
			{
				IPdfPrimitive pdfPrimitive = fontStructureHelperNet.FontDictionary["ToUnicode"];
				PdfStream pdfStream = ((!(pdfPrimitive is PdfReferenceHolder)) ? (pdfPrimitive as PdfStream) : ((pdfPrimitive as PdfReferenceHolder).Object as PdfStream));
				pdfStream.isSkip = false;
			}
			int num = text.IndexOf('<');
			text.IndexOf('>');
			if (num >= 0)
			{
				isHex = true;
			}
			fontStructureHelperNet.IsSameFont = m_resources.isSameFont();
			if (fontStructureHelperNet.FontSize != FontSize)
			{
				fontStructureHelperNet.FontSize = FontSize;
			}
			if (fontStructureHelperNet.FontGlyphWidths == null || fontStructureHelperNet.FontGlyphWidths.Count != 0)
			{
				if (fontStructureHelperNet.FontEncoding == "MacRomanEncoding" && text.Length != imageGlyph.Count)
				{
					if (text[0] == '(' || text[0] == '<')
					{
						text = "(" + fontStructureHelperNet.Decode(text, isSameFont: true) + ")";
					}
					else if (text[0] == '[')
					{
						string decodedString = string.Empty;
						DecodeTextTJ(fontStructureHelperNet, text, isSameFont: true, out decodedString);
						text = decodedString;
					}
				}
				else if (fontStructureHelperNet.FontEncoding == "Identity-H" && fontStructureHelperNet.CharacterMapTable.Count > 0)
				{
					if (text[0] == '(')
					{
						text = "(" + fontStructureHelperNet.DecodeTextExtraction(text, isSameFont: true) + ")";
					}
					else if (text[0] == '[')
					{
						List<string> decodedList = fontStructureHelperNet.DecodeTextExtractionTJ(text, isSameFont: true);
						List<string> list = AddEscapeSymbols(decodedList);
						string text2 = "[";
						for (int i = 0; i < list.Count; i++)
						{
							text2 = ((list[i].Length == 0 || list[i][list[i].Length - 1] != 's') ? (text2 + list[i]) : (text2 + "(" + list[i].Substring(0, list[i].Length - 1) + ")"));
						}
						text2 += "]";
						text = text2;
					}
				}
				else if (text[0] == '(' || text[0] == '<')
				{
					text = "(" + fontStructureHelperNet.DecodeTextExtraction(text, isSameFont: true) + ")";
				}
				else if (text[0] == '[')
				{
					List<string> list2 = fontStructureHelperNet.DecodeTextExtractionTJ(text, isSameFont: true);
					string text3 = "[";
					for (int j = 0; j < list2.Count; j++)
					{
						text3 = ((list2[j].Length == 0 || list2[j][list2[j].Length - 1] != 's') ? (text3 + list2[j]) : (text3 + "(" + list2[j].Substring(0, list2[j].Length - 1) + ")"));
					}
					text3 += "]";
					text = text3;
				}
			}
		}
		foreach (DocGen.PdfViewer.Base.Glyph item in imageGlyph)
		{
			if (isFoundRect(item.BoundingRect))
			{
				flag = true;
				item.IsReplace = true;
			}
			else
			{
				flag2 = true;
				item.IsReplace = false;
			}
		}
		if (!flag && flag2)
		{
			changeOperator = false;
			return text;
		}
		string[] array = null;
		fontStructure = m_resources[CurrentFont] as FontStructureHelperNet;
		if (text[0] == '(')
		{
			array = ((!(fontStructure.FontName == "ZapfDingbats") || fontStructure.IsEmbedded) ? new string[1] { text } : GetSplittedString("(" + fontStructure.Decode(text, v: true) + ")"));
		}
		else if (text[0] == '[')
		{
			text = text.TrimStart('[');
			text = text.TrimEnd(']');
			array = GetSplittedString(text);
			if (fontStructure.FontName == "ZapfDingbats" && !fontStructure.IsEmbedded)
			{
				for (int k = 0; k < array.Length; k++)
				{
					if (array[k].StartsWith("(") || array[k].StartsWith("<"))
					{
						array[k] = "(" + fontStructure.Decode(array[k], v: true) + ")";
					}
				}
			}
		}
		else if (text[0] == '<')
		{
			array = new string[1] { "(" + fontStructure.Decode(text, v: true) + ")" };
		}
		if ((fontStructure.FontEncoding == "Identity-H" || fontStructure.FontEncoding == "Encoding" || fontStructure.FontEncoding == "WinAnsiEncoding") && array.Length == decodedStringData.Count)
		{
			for (int l = 0; l < array.Length; l++)
			{
				if (array[l][0] == '(')
				{
					array[l] = "(" + decodedStringData[l] + ")";
				}
			}
		}
		List<StringMappingNet> list3 = null;
		try
		{
			list3 = MapString(array, imageGlyph, fontStructure);
		}
		catch (Exception)
		{
		}
		changeOperator = true;
		string text4 = string.Empty;
		foreach (StringMappingNet item2 in list3)
		{
			text4 += item2.GetText(fontStructure, isHex);
		}
		text4 = text4.Insert(0, "[");
		return text4.Insert(text4.Length, "]");
	}

	private List<string> DecodeTextTJ(FontStructureHelperNet structure, string textToDecode, bool isSameFont, out string decodedString)
	{
		decodedString = string.Empty;
		string text = string.Empty;
		string text2 = textToDecode;
		structure.IsSameFont = isSameFont;
		List<string> list = new List<string>();
		structure.IsHexaDecimalString = false;
		switch (text2[0])
		{
		case '(':
			if (text2.Contains("\\\n"))
			{
				StringBuilder stringBuilder2 = new StringBuilder(text2);
				stringBuilder2.Replace("\\\n", "");
				text2 = stringBuilder2.ToString();
			}
			text2 = text2.Substring(1, text2.Length - 2);
			text = structure.GetLiteralString(text2);
			text = structure.SkipEscapeSequence(text);
			if (structure.FontDictionary.ContainsKey("Encoding") && structure.FontDictionary["Encoding"] is PdfName && (structure.FontDictionary["Encoding"] as PdfName).Value == "Identity-H")
			{
				List<byte> list2 = new List<byte>();
				string text7 = text;
				foreach (char c in text7)
				{
					list2.Add((byte)c);
				}
				text = Encoding.BigEndianUnicode.GetString(list2.ToArray());
			}
			if (structure.FontName == "ZapfDingbats" && !structure.IsEmbedded)
			{
				decodedString = "(" + structure.MapZapf(text) + ")";
			}
			break;
		case '[':
			if (text2.Contains("\\\n"))
			{
				StringBuilder stringBuilder = new StringBuilder(text2);
				stringBuilder.Replace("\\\n", "");
				text2 = stringBuilder.ToString();
			}
			text2 = text2.Substring(1, text2.Length - 2);
			decodedString += "[";
			while (text2.Length > 0)
			{
				bool flag = false;
				int num = text2.IndexOf('(');
				int num2 = text2.IndexOf(')');
				for (int i = num2 + 1; i < text2.Length && text2[i] != '('; i++)
				{
					if (text2[i] == ')')
					{
						num2 = i;
						break;
					}
				}
				int num3 = text2.IndexOf('<');
				int num4 = text2.IndexOf('>');
				if (num3 < num && num3 > -1)
				{
					num = num3;
					num2 = num4;
					flag = true;
				}
				string text3;
				if (num < 0)
				{
					num = text2.IndexOf('<');
					num2 = text2.IndexOf('>');
					if (num < 0)
					{
						text3 = text2;
						decodedString += text3;
						list.Add(text3);
						break;
					}
					flag = true;
				}
				if (num2 < 0 && text2.Length > 0)
				{
					text3 = text2;
					decodedString += text3;
					list.Add(text3);
					break;
				}
				if (num2 > 0)
				{
					while (text2[num2 - 1] == '\\' && (num2 - 1 <= 0 || text2[num2 - 2] != '\\') && text2.IndexOf(')', num2 + 1) >= 0)
					{
						num2 = text2.IndexOf(')', num2 + 1);
					}
				}
				if (num != 0)
				{
					text3 = text2.Substring(0, num);
					decodedString += text3;
					list.Add(text3);
				}
				string text4 = text2.Substring(num + 1, num2 - num - 1);
				if (flag)
				{
					text3 = structure.GetHexaDecimalString(text4);
					if (text3.Contains("\\"))
					{
						text3 = text3.Replace("\\", "\\\\");
					}
					if (structure.FontName == "ZapfDingbats" && !structure.IsEmbedded)
					{
						text3 = structure.MapZapf(text3);
					}
					decodedString = decodedString + "(" + text3 + ")";
					text += text3;
				}
				else
				{
					text3 = structure.GetLiteralString(text4);
					if (structure.FontName == "ZapfDingbats" && !structure.IsEmbedded)
					{
						text3 = structure.MapZapf(text3);
					}
					decodedString = decodedString + "(" + text3 + ")";
					text += text3;
				}
				if (text3.Contains("\\000"))
				{
					text3 = text3.Replace("\\000", "");
				}
				if (text3.Contains("\0") && (!structure.CharacterMapTable.ContainsKey(0.0) || structure.CharacterMapTable.ContainsValue("\0")) && (structure.CharacterMapTable.Count > 0 || (structure.IsCID && !structure.FontDictionary.ContainsKey("ToUnicode"))))
				{
					text3 = text3.Replace("\0", "");
				}
				if (!structure.IsTextExtraction)
				{
					text3 = structure.SkipEscapeSequence(text3);
				}
				if (structure.CidToGidMap != null)
				{
					text3 = structure.MapCidToGid(text3);
				}
				if (text3.Length > 0)
				{
					if (text3[0] >= '\u0e00' && text3[0] <= '\u0e7f' && list.Count > 0)
					{
						string text5 = list[0];
						text3 = text5.Remove(text5.Length - 1) + text3;
						list[0] = text3 + "s";
					}
					else if ((text3[0] == ' ' || text3[0] == '/') && text3.Length > 1)
					{
						if (text3[1] >= '\u0e00' && text3[1] <= '\u0e7f' && list.Count > 0)
						{
							string text6 = list[0];
							text3 = text6.Remove(text6.Length - 1) + text3;
							list[0] = text3 + "s";
						}
						else
						{
							text3 += "s";
							list.Add(text3);
						}
					}
					else
					{
						text3 += "s";
						list.Add(text3);
					}
				}
				else
				{
					text3 += "s";
					list.Add(text3);
				}
				text2 = text2.Substring(num2 + 1, text2.Length - num2 - 1);
			}
			decodedString += "]";
			break;
		case '<':
		{
			string hexEncodedText = text2.Substring(1, text2.Length - 2);
			text = structure.GetHexaDecimalString(hexEncodedText);
			if (structure.FontName == "ZapfDingbats" && !structure.IsEmbedded)
			{
				string text3 = structure.MapZapf(text);
			}
			decodedString = decodedString + "(" + text + ")";
			break;
		}
		}
		text = structure.SkipEscapeSequence(text);
		return list;
	}

	private List<string> AddEscapeSymbols(List<string> decodedList)
	{
		List<string> list = new List<string>();
		foreach (string decoded in decodedList)
		{
			if (decoded.Length > 0 && decoded[decoded.Length - 1] == 's')
			{
				MemoryStream memoryStream = new MemoryStream();
				foreach (char c in decoded)
				{
					switch ((byte)c)
					{
					case 40:
					case 41:
					case 92:
						memoryStream.WriteByte(92);
						memoryStream.WriteByte((byte)c);
						break;
					case 13:
						memoryStream.WriteByte(92);
						memoryStream.WriteByte(112);
						break;
					default:
						memoryStream.WriteByte((byte)c);
						break;
					}
				}
				if (memoryStream.Length > 0)
				{
					list.Add(Encoding.Default.GetString(memoryStream.ToArray()));
				}
			}
			else
			{
				list.Add(decoded);
			}
		}
		return list;
	}

	private string[] GetSplittedString(string text)
	{
		List<string> list = new List<string>();
		string text2 = string.Empty;
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			if (c == '\\')
			{
				continue;
			}
			if (c == '(' && i + 1 < text.Length && i + 2 < text.Length && (text[i + 1] == '(' || text[i + 1] == ')') && text[i + 2] == ')')
			{
				if (text2 != string.Empty)
				{
					list.Add(text2);
				}
				text2 = string.Empty;
				text2 = c.ToString() + text[i + 1] + text[i + 2];
				list.Add(text2);
				text2 = string.Empty;
				i += 2;
			}
			else if ((c == '(' || c == '<') && i - 1 >= 0 && text[i - 1] != '\\')
			{
				if (text2 != string.Empty)
				{
					list.Add(text2);
				}
				text2 = c.ToString();
			}
			else if ((c == ')' || c == '>') && i - 1 >= 0 && text[i - 1] != '\\')
			{
				if (text2 != string.Empty)
				{
					text2 += c;
					list.Add(text2);
					text2 = string.Empty;
				}
			}
			else
			{
				text2 += c;
			}
		}
		if (text2 != string.Empty)
		{
			list.Add(text2);
		}
		return list.ToArray();
	}

	private bool isFoundRect(RectangleF syncrect)
	{
		RectangleF bounds = new RectangleF(syncrect.X, syncrect.Y, syncrect.Width, syncrect.Height);
		bool result = false;
		bounds = GetRelativeBounds(bounds);
		foreach (PdfRedaction redaction in redactions)
		{
			PointF pointF = new PointF(bounds.X, bounds.Y);
			PointF pointF2 = new PointF(bounds.X + bounds.Width, bounds.Y);
			PointF pointF3 = new PointF(bounds.X, bounds.Y + bounds.Height);
			PointF pointF4 = new PointF(bounds.X + bounds.Width, bounds.Y + bounds.Height);
			RectangleF rectangleF = new RectangleF(redaction.Bounds.X, redaction.Bounds.Y, redaction.Bounds.Width, redaction.Bounds.Height);
			PointF pointF5 = new PointF(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f);
			if (rectangleF.Contains(pointF5) && (rectangleF.Contains(bounds) || rectangleF.IntersectsWith(bounds) || rectangleF.Contains(pointF) || rectangleF.Contains(pointF2) || rectangleF.Contains(pointF3) || rectangleF.Contains(pointF4)))
			{
				redaction.m_success = true;
				result = true;
				break;
			}
			if (isContainsRedactionText && !isNotUpdated)
			{
				redaction.m_success = true;
			}
		}
		return result;
	}

	private bool isFoundText(PointF location)
	{
		bool result = false;
		if (location.Y < 0f)
		{
			location.Y = 0f - location.Y;
		}
		location = GetRelativelocation(location);
		int num = (int)location.Y;
		_ = location.X;
		foreach (PdfRedaction redaction in redactions)
		{
			RectangleF rectangleF = new RectangleF(redaction.Bounds.X, redaction.Bounds.Y, redaction.Bounds.Width, redaction.Bounds.Height);
			if (isContainsImages)
			{
				result = true;
				break;
			}
			if ((int)rectangleF.Y == num || (int)rectangleF.Y == num - 1 || (int)rectangleF.Y == num + 1)
			{
				result = true;
				break;
			}
			num = (int)location.Y;
			if ((rectangleF.Y >= (float)num && (float)num >= rectangleF.Y - rectangleF.Height) || (rectangleF.Y <= (float)num && (float)num <= rectangleF.Y + rectangleF.Height))
			{
				result = true;
				break;
			}
			num = (int)(m_loadedPage.Size.Height - location.Y);
			if ((rectangleF.Y >= (float)num && (float)num >= rectangleF.Y - rectangleF.Height) || (rectangleF.Y <= (float)num && (float)num <= rectangleF.Y + rectangleF.Height))
			{
				result = true;
				break;
			}
			if (rectValue.Y != 0f)
			{
				if (rectValue.Y < 0f)
				{
					rectValue.Y = 0f - rectValue.Y;
				}
				num = (int)(location.Y + rectValue.Y);
				if ((rectangleF.Y >= (float)num && (float)num >= rectangleF.Y - rectangleF.Height) || (rectangleF.Y <= (float)num && (float)num <= rectangleF.Y + rectangleF.Height))
				{
					result = true;
					break;
				}
				if (rectValue.Height < 0f)
				{
					rectValue.Height = 0f - rectValue.Height;
				}
				num = (int)(rectValue.Height - (float)num);
				if ((rectangleF.Y >= (float)num && (float)num >= rectangleF.Y - rectangleF.Height) || (rectangleF.Y <= (float)num && (float)num <= rectangleF.Y + rectangleF.Height))
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	private PointF GetRelativelocation(PointF location)
	{
		SizeF size = m_loadedPage.Size;
		PointF result = location;
		if (m_loadedPage.Rotation == PdfPageRotateAngle.RotateAngle90)
		{
			result.X = size.Height - location.Y;
			result.Y = location.X;
		}
		else if (m_loadedPage.Rotation == PdfPageRotateAngle.RotateAngle270)
		{
			result.Y = size.Width - location.X;
			result.X = location.Y;
		}
		return result;
	}

	private RectangleF GetRelativeBounds(RectangleF bounds)
	{
		SizeF size = m_loadedPage.Size;
		RectangleF result = bounds;
		if (m_loadedPage.Rotation == PdfPageRotateAngle.RotateAngle90)
		{
			result.X = size.Height - (bounds.Y + bounds.Height);
			result.Y = bounds.X;
			result.Width = bounds.Height;
			result.Height = bounds.Width;
		}
		else if (m_loadedPage.Rotation == PdfPageRotateAngle.RotateAngle270)
		{
			result.Y = size.Width - (bounds.X + bounds.Width);
			result.X = bounds.Y;
			result.Width = bounds.Height;
			result.Height = bounds.Width;
		}
		else if (m_loadedPage.Rotation == PdfPageRotateAngle.RotateAngle180)
		{
			result.X = size.Width - (bounds.X + bounds.Width);
			result.Y = size.Height - (bounds.Y + bounds.Height);
		}
		return result;
	}

	private void SetStrokingColor(Color color)
	{
		Objects.StrokingBrush = new PdfPen(color).Brush;
	}

	private void SetNonStrokingColor(Color color)
	{
		Objects.NonStrokingBrush = new PdfPen(color).Brush;
	}

	private void MoveToNextLineWithLeading(string[] element)
	{
		SetTextLeading(0f - float.Parse(element[1], NumberStyles.Float, CultureInfo.InvariantCulture));
		MoveToNextLine(float.Parse(element[0], NumberStyles.Float, CultureInfo.InvariantCulture), float.Parse(element[1], NumberStyles.Float, CultureInfo.InvariantCulture));
	}

	private void SetTextLeading(float txtLeading)
	{
		TextLeading = 0f - txtLeading;
	}

	private void RenderFont(string[] fontElements)
	{
		int i;
		for (i = 0; i < fontElements.Length; i++)
		{
			if (fontElements[i].Contains("/"))
			{
				CurrentFont = fontElements[i].Replace("/", "");
				break;
			}
		}
		FontSize = float.Parse(fontElements[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture);
	}

	private void RenderTextElement(string[] textElements, string tokenType)
	{
		string text = string.Join("", textElements);
		if (!m_resources.ContainsKey(CurrentFont))
		{
			return;
		}
		(m_resources[CurrentFont] as FontStructureHelperNet).IsSameFont = m_resources.isSameFont();
		if ((m_resources[CurrentFont] as FontStructureHelperNet).FontSize != FontSize)
		{
			(m_resources[CurrentFont] as FontStructureHelperNet).FontSize = FontSize;
		}
		FontStructureHelperNet fontStructureHelperNet = m_resources[CurrentFont] as FontStructureHelperNet;
		if (fontStructureHelperNet == null)
		{
			return;
		}
		byte[] bytes = Encoding.Unicode.GetBytes(fontStructureHelperNet.ToGetEncodedText(text, m_resources.isSameFont()));
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		int num = 0;
		for (int i = 0; i < bytes.Length; i += 2)
		{
			dictionary.Add(num, bytes[i]);
			num++;
		}
		text = fontStructureHelperNet.Decode(text, m_resources.isSameFont());
		TextElementHelperNet textElementHelperNet = new TextElementHelperNet(text, DocumentMatrix);
		textElementHelperNet.FontStyle = fontStructureHelperNet.FontStyle;
		textElementHelperNet.EncodedTextBytes = dictionary;
		textElementHelperNet.FontName = fontStructureHelperNet.FontName;
		SKPaint sKPaint2;
		if (!skpaintCache.ContainsKey(CurrentFont + fontStructureHelperNet.FontRefNumber))
		{
			SKPaint sKPaint = new SKPaint();
			SKTypeface typeface = SKTypeface.FromFamilyName(fontStructureHelperNet.FontName, GetFontStyle(fontStructureHelperNet.FontStyle));
			sKPaint.Typeface = typeface;
			sKPaint.TextSize = fontStructureHelperNet.FontSize;
			sKPaint2 = sKPaint;
			skpaintCache[CurrentFont + fontStructureHelperNet.FontRefNumber] = sKPaint2;
		}
		else
		{
			sKPaint2 = skpaintCache[CurrentFont + fontStructureHelperNet.FontRefNumber];
		}
		textElementHelperNet.skpaint = sKPaint2;
		textElementHelperNet.Font = fontStructureHelperNet.CurrentFont;
		textElementHelperNet.FontSize = FontSize;
		textElementHelperNet.TextScaling = m_textScaling;
		textElementHelperNet.FontEncoding = fontStructureHelperNet.FontEncoding;
		textElementHelperNet.FontGlyphWidths = fontStructureHelperNet.FontGlyphWidths;
		textElementHelperNet.DefaultGlyphWidth = fontStructureHelperNet.DefaultGlyphWidth;
		textElementHelperNet.isNegativeFont = isNegativeFont;
		textElementHelperNet.UnicodeCharMapTable = fontStructureHelperNet.UnicodeCharMapTable;
		textElementHelperNet.IsFindText = isFindText;
		Dictionary<int, int> fontGlyphWidths = fontStructureHelperNet.FontGlyphWidths;
		textElementHelperNet.IsType1Font = fontStructureHelperNet.IsType1Font;
		textElementHelperNet.Is1C = fontStructureHelperNet.Is1C;
		textElementHelperNet.IsCID = fontStructureHelperNet.IsCID;
		textElementHelperNet.CharacterMapTable = fontStructureHelperNet.CharacterMapTable;
		textElementHelperNet.CidToGidReverseMapTable = fontStructureHelperNet.CidToGidReverseMapTable;
		textElementHelperNet.ReverseMapTable = fontStructureHelperNet.ReverseMapTable;
		textElementHelperNet.Fontfile2Glyph = fontStructureHelperNet.GlyphFontFile2;
		textElementHelperNet.structure = fontStructureHelperNet;
		textElementHelperNet.Isembeddedfont = fontStructureHelperNet.IsEmbedded;
		textElementHelperNet.Ctm = CTRM;
		textElementHelperNet.textLineMatrix = TextMatrix;
		textElementHelperNet.Rise = Rise;
		textElementHelperNet.transformMatrix = DocumentMatrix;
		textElementHelperNet.documentMatrix = DocumentMatrix;
		textElementHelperNet.FontID = CurrentFont;
		textElementHelperNet.OctDecMapTable = fontStructureHelperNet.OctDecMapTable;
		textElementHelperNet.TextHorizontalScaling = HorizontalScaling;
		textElementHelperNet.ZapfPostScript = fontStructureHelperNet.ZapfPostScript;
		textElementHelperNet.LineWidth = MitterLength;
		textElementHelperNet.RenderingMode = RenderingMode;
		textElementHelperNet.testdict = testdict;
		textElementHelperNet.pageRotation = pageRotation;
		textElementHelperNet.zoomFactor = zoomFactor;
		textElementHelperNet.SubstitutedFontsList = substitutedFontsList;
		if (fontStructureHelperNet.Flags != null)
		{
			textElementHelperNet.FontFlag = fontStructureHelperNet.Flags.IntValue;
		}
		if (fontStructureHelperNet.IsType1Font)
		{
			textElementHelperNet.IsType1Font = true;
			textElementHelperNet.differenceTable = fontStructureHelperNet.differenceTable;
			textElementHelperNet.differenceMappedTable = fontStructureHelperNet.DifferencesDictionary;
			textElementHelperNet.m_cffGlyphs = fontStructureHelperNet.m_cffGlyphs;
			textElementHelperNet.OctDecMapTable = fontStructureHelperNet.OctDecMapTable;
			if (!m_glyphDataCollection.Contains(textElementHelperNet.m_cffGlyphs))
			{
				m_glyphDataCollection.Add(textElementHelperNet.m_cffGlyphs);
			}
		}
		if (StrokingBrush != null)
		{
			if (textElementHelperNet.RenderingMode == 3)
			{
				textElementHelperNet.PathBrush = new PdfPen(Color.Transparent).Brush;
			}
			else
			{
				textElementHelperNet.PathBrush = StrokingBrush;
			}
		}
		else
		{
			textElementHelperNet.PathBrush = new PdfPen(Color.Black).Brush;
		}
		if (NonStrokingBrush != null)
		{
			textElementHelperNet.PathNonStrokeBrush = NonStrokingBrush;
		}
		else
		{
			textElementHelperNet.PathNonStrokeBrush = new PdfPen(Color.Black).Brush;
		}
		textElementHelperNet.WordSpacing = Objects.WordSpacing;
		textElementHelperNet.CharacterSpacing = Objects.CharacterSpacing;
		if (m_beginText)
		{
			m_beginText = false;
		}
		DocGen.PdfViewer.Base.Matrix txtMatrix = default(DocGen.PdfViewer.Base.Matrix);
		if (fontStructureHelperNet.fontType.Value != "Type3")
		{
			if (m_isCurrentPositionChanged)
			{
				m_isCurrentPositionChanged = false;
				m_endTextPosition = CurrentLocation;
				if (textElementHelperNet.structure.IsMappingDone && textElementHelperNet.structure.FontEncoding == "Identity-H" && textElementHelperNet.structure.CharacterMapTable.Count > 0)
				{
					textElementHelperNet.structure.IsMappingDone = false;
				}
				m_textElementWidth = textElementHelperNet.Render(m_graphics, new PointF(m_endTextPosition.X, m_endTextPosition.Y - FontSize), m_textScaling, fontGlyphWidths, fontStructureHelperNet.Type1GlyphHeight, fontStructureHelperNet.differenceTable, fontStructureHelperNet.DifferencesDictionary, fontStructureHelperNet.differenceEncoding, out txtMatrix);
				decodedStringData.Add(GetTextFromGlyph(textElementHelperNet.textElementGlyphList, 0));
				imageRenderGlyphList.AddRange(textElementHelperNet.textElementGlyphList);
				textElementHelperNet.textElementGlyphList.Clear();
			}
			else
			{
				m_endTextPosition = new PointF(m_endTextPosition.X + m_textElementWidth, m_endTextPosition.Y);
				m_textElementWidth = textElementHelperNet.Render(m_graphics, new PointF(m_endTextPosition.X, m_endTextPosition.Y - FontSize), m_textScaling, fontGlyphWidths, fontStructureHelperNet.Type1GlyphHeight, fontStructureHelperNet.differenceTable, fontStructureHelperNet.DifferencesDictionary, fontStructureHelperNet.differenceEncoding, out txtMatrix);
				decodedStringData.Add(GetTextFromGlyph(textElementHelperNet.textElementGlyphList, 0));
				imageRenderGlyphList.AddRange(textElementHelperNet.textElementGlyphList);
				textElementHelperNet.textElementGlyphList.Clear();
			}
		}
		else
		{
			DocGen.PdfViewer.Base.Matrix textLineMatrix = TextLineMatrix;
			FontStructureHelperNet fontStructureHelperNet2 = fontStructureHelperNet;
			m_graphics.Transform = new DocGen.Drawing.Matrix((float)DocumentMatrix.M11, (float)DocumentMatrix.M12, (float)DocumentMatrix.M21, (float)DocumentMatrix.M22, (float)DocumentMatrix.OffsetX, (float)DocumentMatrix.OffsetY);
			m_graphics.TranslateTransform(0f, (float)TextLineMatrix.OffsetY);
			m_spacingWidth = 0f;
			RenderType3GlyphImages(fontStructureHelperNet, text);
			TextLineMatrix = textLineMatrix;
			fontStructureHelperNet = fontStructureHelperNet2;
			txtMatrix = DocGen.PdfViewer.Base.Matrix.Identity;
		}
		TextMatrix = txtMatrix;
		string text2 = textElementHelperNet.renderedText;
		if (!fontStructureHelperNet.IsMappingDone)
		{
			if (fontStructureHelperNet.CharacterMapTable != null && fontStructureHelperNet.CharacterMapTable.Count > 0)
			{
				text2 = fontStructureHelperNet.MapCharactersFromTable(text2);
			}
			else if (fontStructureHelperNet.DifferencesDictionary != null && fontStructureHelperNet.DifferencesDictionary.Count > 0)
			{
				text2 = fontStructureHelperNet.MapDifferences(text2);
			}
		}
		_ = CTRM * TextLineMatrix;
		float num2 = new PdfUnitConverter().ConvertFromPixels(currentPageHeight, PdfGraphicsUnit.Point);
		if (!text2.Contains("www") && !text2.Contains("http") && !IsValidEmail(text2) && !text2.Contains(".com"))
		{
			return;
		}
		float textSize = 0f;
		DocGen.PdfViewer.Base.Matrix matrix = default(DocGen.PdfViewer.Base.Matrix);
		matrix.M11 = FontSize * (HorizontalScaling / 100f);
		matrix.M22 = 0f - FontSize;
		matrix.OffsetY = FontSize + (float)Rise;
		DocGen.PdfViewer.Base.Matrix matrix2 = matrix * TextLineMatrix * CTRM;
		if (matrix2.M11 != 0.0 && matrix2.M22 != 0.0)
		{
			textSize = (float)matrix2.M11;
			textSize = Math.Abs(textSize);
		}
		else if (matrix2.M12 != 0.0 && matrix2.M21 != 0.0)
		{
			textSize = (float)matrix2.M12;
			textSize = Math.Abs(textSize);
		}
		SizeF sizeF = SizeF.Empty;
		using (SKPaint sKPaint3 = new SKPaint())
		{
			using SKTypeface typeface2 = SKTypeface.FromFamilyName(fontStructureHelperNet.FontName, GetFontStyle(fontStructureHelperNet.FontStyle));
			sKPaint3.Typeface = typeface2;
			sKPaint3.TextSize = textSize;
			sizeF = m_graphics.MeasureString(text2, sKPaint2);
		}
		_ = TextLeading;
		DocGen.PdfViewer.Base.Matrix matrix3 = CTRM * TextLineMatrix;
		if (TextLineMatrix.M12 != 0.0 && TextLineMatrix.M21 != 0.0)
		{
			matrix3.OffsetX = TextLineMatrix.OffsetY;
			matrix3.OffsetY = 0.0 - TextLineMatrix.OffsetX;
		}
		else
		{
			matrix3.OffsetX = matrix2.OffsetX;
			matrix3.OffsetY = matrix2.OffsetY;
			matrix3 *= new DocGen.PdfViewer.Base.Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0f - num2);
		}
		currentTransformLocation = new PointF((float)matrix3.OffsetX, 0f - (float)matrix3.OffsetY);
		Regex regex = new Regex("\\b(?:http://|https://|www\\.)\\S+\\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		string uRI = string.Empty;
		foreach (Match item2 in regex.Matches(text2))
		{
			uRI = item2.Value;
		}
		if (IsValidEmail(textElementHelperNet.renderedText) || text2.Contains("@"))
		{
			uRI = "mailto:" + text2;
		}
		PageURLNet item = new PageURLNet(transformMatrix, uRI, new PointF(currentTransformLocation.X, currentTransformLocation.Y), sizeF.Width, sizeF.Height);
		URLDictonary.Add(item);
	}

	private SKTextAlign GetTextAlignment(StringAlignment alignment)
	{
		return alignment switch
		{
			StringAlignment.Far => SKTextAlign.Right, 
			StringAlignment.Center => SKTextAlign.Center, 
			_ => SKTextAlign.Left, 
		};
	}

	private SKFontStyle GetFontStyle(FontStyle style)
	{
		if (style == FontStyle.Bold && style == FontStyle.Italic)
		{
			return SKFontStyle.BoldItalic;
		}
		return style switch
		{
			FontStyle.Bold => SKFontStyle.Bold, 
			FontStyle.Italic => SKFontStyle.Italic, 
			_ => SKFontStyle.Normal, 
		};
	}

	private void UpdateTextMatrix(double tj)
	{
		double num = 0.0 - tj * 0.001 * (double)FontSize * (double)HorizontalScaling / 100.0;
		DocGen.PdfViewer.Base.Point point = Objects.textLineMatrix.Transform(new DocGen.PdfViewer.Base.Point(0.0, 0.0));
		DocGen.PdfViewer.Base.Point point2 = Objects.textLineMatrix.Transform(new DocGen.PdfViewer.Base.Point(num, 0.0));
		if (point.X != point2.X)
		{
			Objects.textLineMatrix.OffsetX = point2.X;
		}
		else
		{
			Objects.textLineMatrix.OffsetY = point2.Y;
		}
		m_spacingWidth += (float)num;
	}

	private string GetTextFromGlyph(List<DocGen.PdfViewer.Base.Glyph> glyph, int startIndex)
	{
		string text = string.Empty;
		for (int i = startIndex; i < glyph.Count; i++)
		{
			text += glyph[i].ToUnicode;
		}
		return text;
	}

	private void RenderType3GlyphImagesTJ(FontStructureHelperNet structure, List<string> decodedCollection)
	{
		for (int i = 0; i < decodedCollection.Count; i++)
		{
			string text = decodedCollection[i];
			if (double.TryParse(text, out var result))
			{
				UpdateTextMatrix(result);
				TextMatrix = TextLineMatrix;
				continue;
			}
			text = text.Remove(text.Length - 1, 1);
			for (int j = 0; j < text.Length; j++)
			{
				if (text[j] == ' ' && Objects.WordSpacing > 0f)
				{
					m_type3WhiteSpaceWidth = Objects.WordSpacing;
				}
				else
				{
					m_type3WhiteSpaceWidth = 0f;
				}
				string text2 = structure.DecodeType3FontData(text[j].ToString());
				if (!structure.Type3FontCharProcsDict.ContainsKey(text2))
				{
					text2 = FontStructure.GetCharCode(text2);
				}
				if (structure.Type3FontCharProcsDict != null && structure.Type3FontCharProcsDict.ContainsKey(text2))
				{
					m_type3GlyphID = text2 + structure.FontRefNumber;
					MemoryStream memoryStream = new MemoryStream();
					PdfStream pdfStream = structure.Type3FontCharProcsDict[text2];
					byte[] array = PdfString.StringToByte("\r\n");
					pdfStream.Decompress();
					pdfStream.InternalStream.WriteTo(memoryStream);
					memoryStream.Write(array, 0, array.Length);
					bool flag = false;
					DocGen.PdfViewer.Base.Matrix cTRM = CTRM;
					DocGen.Drawing.Matrix drawing2dMatrixCTM = Drawing2dMatrixCTM;
					DocGen.PdfViewer.Base.Matrix matrix = TextLineMatrix * CTRM;
					Stack<GraphicObjectDataHelperNet> objects = m_objects;
					if (structure.FontDictionary.ContainsKey("FontMatrix"))
					{
						PdfArray pdfArray = structure.FontDictionary["FontMatrix"] as PdfArray;
						XFormsMatrix = new DocGen.PdfViewer.Base.Matrix((pdfArray[0] as PdfNumber).FloatValue, (pdfArray[1] as PdfNumber).FloatValue, (pdfArray[2] as PdfNumber).FloatValue, (pdfArray[3] as PdfNumber).FloatValue, (pdfArray[4] as PdfNumber).FloatValue, (pdfArray[5] as PdfNumber).FloatValue);
						matrix = XFormsMatrix * matrix;
						m_type3FontScallingFactor = (float)matrix.M11;
					}
					if (memoryStream != null)
					{
						ContentParser contentParser = new ContentParser(memoryStream.ToArray());
						m_type3RecordCollection = contentParser.ReadContent();
						for (int k = 0; k < m_type3RecordCollection.RecordCollection.Count; k++)
						{
							string text3 = m_type3RecordCollection.RecordCollection[k].OperatorName;
							if (text3 == "cm")
							{
								m_istype3FontContainCTRM = true;
							}
							char[] symbolChars = m_symbolChars;
							for (int l = 0; l < symbolChars.Length; l++)
							{
								char c = symbolChars[l];
								if (text3.Contains(c.ToString()))
								{
									text3 = text3.Replace(c.ToString(), "");
								}
							}
							if (text3.Trim() == "BI" || text3.Trim() == "Do")
							{
								flag = true;
							}
						}
						PdfPageResourcesHelper resources = m_resources;
						m_resources = structure.Type3FontGlyphImages;
						m_transformations = new TransformationStack(DocumentMatrix);
						m_type3FontStruct = structure;
						m_isType3Font = true;
						if (flag)
						{
							CTRM = new DocGen.PdfViewer.Base.Matrix((float)matrix.M11, (float)matrix.M12, (float)matrix.M21, (float)matrix.M22, (float)matrix.OffsetX, (float)matrix.OffsetY);
							Drawing2dMatrixCTM = new DocGen.Drawing.Matrix((float)matrix.M11, (float)matrix.M12, (float)matrix.M21, (float)matrix.M22, (float)matrix.OffsetX, (float)matrix.OffsetY);
						}
						RenderAsImage();
						m_resources = resources;
						if (flag)
						{
							TextLineMatrix = m_type3TextLineMatrix;
						}
					}
					CTRM = cTRM;
					Drawing2dMatrixCTM = drawing2dMatrixCTM;
					m_objects = objects;
					continue;
				}
				float num;
				if (m_d1Matrix.M11 == 0.0)
				{
					num = (float)(m_d1Matrix.M21 - m_d1Matrix.OffsetX);
					if (num < 0f)
					{
						num = 0f - num;
					}
				}
				else
				{
					num = (float)m_d1Matrix.M11;
				}
				TextLineMatrix = CalculateType3TextMatrixupdate(TextLineMatrix, num * m_type3FontScallingFactor, isImage: true);
			}
		}
	}

	private void RenderType3GlyphImages(FontStructureHelperNet structure, string renderingText)
	{
		for (int i = 0; i < renderingText.Length; i++)
		{
			string text = structure.DecodeType3FontData(renderingText[i].ToString());
			if (structure.Type3FontCharProcsDict != null && !structure.Type3FontCharProcsDict.ContainsKey(text))
			{
				text = FontStructureHelperNet.GetCharCode(text);
			}
			if (structure.Type3FontCharProcsDict == null || !structure.Type3FontCharProcsDict.ContainsKey(text))
			{
				continue;
			}
			m_type3GlyphID = text + structure.FontRefNumber;
			MemoryStream memoryStream = new MemoryStream();
			PdfStream pdfStream = structure.Type3FontCharProcsDict[text];
			byte[] array = PdfString.StringToByte("\r\n");
			pdfStream.Decompress();
			pdfStream.InternalStream.WriteTo(memoryStream);
			memoryStream.Write(array, 0, array.Length);
			bool flag = false;
			DocGen.PdfViewer.Base.Matrix cTRM = CTRM;
			DocGen.Drawing.Matrix drawing2dMatrixCTM = Drawing2dMatrixCTM;
			DocGen.PdfViewer.Base.Matrix matrix = TextLineMatrix * CTRM;
			Stack<GraphicObjectDataHelperNet> objects = m_objects;
			if (structure.FontDictionary.ContainsKey("FontMatrix"))
			{
				PdfArray pdfArray = structure.FontDictionary["FontMatrix"] as PdfArray;
				XFormsMatrix = new DocGen.PdfViewer.Base.Matrix((pdfArray[0] as PdfNumber).FloatValue, (pdfArray[1] as PdfNumber).FloatValue, (pdfArray[2] as PdfNumber).FloatValue, (pdfArray[3] as PdfNumber).FloatValue, (pdfArray[4] as PdfNumber).FloatValue, (pdfArray[5] as PdfNumber).FloatValue);
				matrix = XFormsMatrix * matrix;
				m_type3FontScallingFactor = (float)matrix.M11;
			}
			if (memoryStream != null)
			{
				ContentParser contentParser = new ContentParser(memoryStream.ToArray());
				m_type3RecordCollection = contentParser.ReadContent();
				for (int j = 0; j < m_type3RecordCollection.RecordCollection.Count; j++)
				{
					string text2 = m_type3RecordCollection.RecordCollection[j].OperatorName;
					if (text2 == "cm")
					{
						m_istype3FontContainCTRM = true;
					}
					char[] symbolChars = m_symbolChars;
					for (int k = 0; k < symbolChars.Length; k++)
					{
						char c = symbolChars[k];
						if (text2.Contains(c.ToString()))
						{
							text2 = text2.Replace(c.ToString(), "");
						}
					}
					if (text2.Trim() == "BI" || text2.Trim() == "Do")
					{
						flag = true;
					}
				}
				PdfPageResourcesHelper resources = m_resources;
				m_resources = structure.Type3FontGlyphImages;
				m_transformations = new TransformationStack(DocumentMatrix);
				m_type3FontStruct = structure;
				m_isType3Font = true;
				if (flag)
				{
					CTRM = new DocGen.PdfViewer.Base.Matrix((float)matrix.M11, (float)matrix.M12, (float)matrix.M21, (float)matrix.M22, (float)matrix.OffsetX, (float)matrix.OffsetY);
					Drawing2dMatrixCTM = new DocGen.Drawing.Matrix((float)matrix.M11, (float)matrix.M12, (float)matrix.M21, (float)matrix.M22, (float)matrix.OffsetX, (float)matrix.OffsetY);
				}
				RenderAsImage();
				m_resources = resources;
				if (flag)
				{
					TextLineMatrix = m_type3TextLineMatrix;
				}
			}
			if (text == "space")
			{
				float num = (float)m_d0Matrix.M11;
				TextLineMatrix = CalculateType3TextMatrixupdate(TextLineMatrix, num * m_type3FontScallingFactor, isImage: true);
			}
			CTRM = cTRM;
			Drawing2dMatrixCTM = drawing2dMatrixCTM;
			m_objects = objects;
		}
	}

	private void RenderTextElementWithLeading(string[] textElements, string tokenType)
	{
		string textToDecode = string.Join("", textElements);
		if (!m_resources.ContainsKey(CurrentFont))
		{
			return;
		}
		(m_resources[CurrentFont] as FontStructureHelperNet).IsSameFont = m_resources.isSameFont();
		if ((m_resources[CurrentFont] as FontStructureHelperNet).FontSize != FontSize)
		{
			(m_resources[CurrentFont] as FontStructureHelperNet).FontSize = FontSize;
		}
		FontStructureHelperNet fontStructureHelperNet = m_resources[CurrentFont] as FontStructureHelperNet;
		textToDecode = fontStructureHelperNet.Decode(textToDecode, m_resources.isSameFont());
		if (fontStructureHelperNet == null)
		{
			return;
		}
		TextElementHelperNet textElementHelperNet = new TextElementHelperNet(textToDecode, DocumentMatrix);
		textElementHelperNet.FontStyle = fontStructureHelperNet.FontStyle;
		textElementHelperNet.Font = fontStructureHelperNet.CurrentFont;
		textElementHelperNet.FontName = fontStructureHelperNet.FontName;
		SKPaint sKPaint2;
		if (!skpaintCache.ContainsKey(CurrentFont + fontStructureHelperNet.FontRefNumber))
		{
			SKPaint sKPaint = new SKPaint();
			SKTypeface typeface = SKTypeface.FromFamilyName(fontStructureHelperNet.FontName, GetFontStyle(fontStructureHelperNet.FontStyle));
			sKPaint.Typeface = typeface;
			sKPaint.TextSize = fontStructureHelperNet.FontSize;
			sKPaint2 = sKPaint;
			skpaintCache[CurrentFont + fontStructureHelperNet.FontRefNumber] = sKPaint2;
		}
		else
		{
			sKPaint2 = skpaintCache[CurrentFont + fontStructureHelperNet.FontRefNumber];
		}
		textElementHelperNet.skpaint = sKPaint2;
		_ = fontStructureHelperNet.CurrentFont;
		textElementHelperNet.FontSize = FontSize;
		textElementHelperNet.TextScaling = m_textScaling;
		textElementHelperNet.FontEncoding = fontStructureHelperNet.FontEncoding;
		textElementHelperNet.FontGlyphWidths = fontStructureHelperNet.FontGlyphWidths;
		textElementHelperNet.DefaultGlyphWidth = fontStructureHelperNet.DefaultGlyphWidth;
		textElementHelperNet.Text = textToDecode;
		textElementHelperNet.IsFindText = isFindText;
		textElementHelperNet.IsTransparentText = IsTransparentText;
		textElementHelperNet.UnicodeCharMapTable = fontStructureHelperNet.UnicodeCharMapTable;
		Dictionary<int, int> fontGlyphWidths = fontStructureHelperNet.FontGlyphWidths;
		textElementHelperNet.IsType1Font = fontStructureHelperNet.IsType1Font;
		textElementHelperNet.Is1C = fontStructureHelperNet.Is1C;
		textElementHelperNet.IsCID = fontStructureHelperNet.IsCID;
		textElementHelperNet.CharacterMapTable = fontStructureHelperNet.CharacterMapTable;
		textElementHelperNet.ReverseMapTable = fontStructureHelperNet.ReverseMapTable;
		textElementHelperNet.Fontfile2Glyph = fontStructureHelperNet.GlyphFontFile2;
		textElementHelperNet.structure = fontStructureHelperNet;
		textElementHelperNet.Isembeddedfont = fontStructureHelperNet.IsEmbedded;
		textElementHelperNet.Ctm = CTRM;
		textElementHelperNet.textLineMatrix = TextMatrix;
		textElementHelperNet.Rise = Rise;
		textElementHelperNet.transformMatrix = DocumentMatrix;
		textElementHelperNet.documentMatrix = DocumentMatrix;
		textElementHelperNet.FontID = CurrentFont;
		textElementHelperNet.OctDecMapTable = fontStructureHelperNet.OctDecMapTable;
		textElementHelperNet.TextHorizontalScaling = HorizontalScaling;
		textElementHelperNet.ZapfPostScript = fontStructureHelperNet.ZapfPostScript;
		textElementHelperNet.LineWidth = MitterLength;
		textElementHelperNet.RenderingMode = RenderingMode;
		textElementHelperNet.testdict = testdict;
		textElementHelperNet.pageRotation = pageRotation;
		textElementHelperNet.zoomFactor = zoomFactor;
		textElementHelperNet.SubstitutedFontsList = substitutedFontsList;
		if (fontStructureHelperNet.Flags != null)
		{
			textElementHelperNet.FontFlag = fontStructureHelperNet.Flags.IntValue;
		}
		if (fontStructureHelperNet.IsType1Font)
		{
			textElementHelperNet.IsType1Font = true;
			textElementHelperNet.differenceTable = fontStructureHelperNet.differenceTable;
			textElementHelperNet.differenceMappedTable = fontStructureHelperNet.DifferencesDictionary;
			textElementHelperNet.m_cffGlyphs = fontStructureHelperNet.m_cffGlyphs;
			if (!m_glyphDataCollection.Contains(textElementHelperNet.m_cffGlyphs))
			{
				m_glyphDataCollection.Add(textElementHelperNet.m_cffGlyphs);
			}
		}
		if (StrokingBrush != null)
		{
			if (Objects.m_strokingOpacity != 1f)
			{
				m_opacity = Objects.m_strokingOpacity;
				SetStrokingColor(GetColor(m_backupColorElements, "Stroking", m_backupColorSpace));
				textElementHelperNet.PathBrush = StrokingBrush;
			}
			else
			{
				textElementHelperNet.PathBrush = StrokingBrush;
			}
		}
		else
		{
			textElementHelperNet.PathBrush = new PdfPen(Color.Black).Brush;
		}
		if (NonStrokingBrush != null)
		{
			textElementHelperNet.PathNonStrokeBrush = NonStrokingBrush;
		}
		textElementHelperNet.WordSpacing = Objects.WordSpacing;
		textElementHelperNet.CharacterSpacing = Objects.CharacterSpacing;
		if (TextScaling != 100f)
		{
			textElementHelperNet.Textscalingfactor = TextScaling / 100f;
		}
		if (m_beginText)
		{
			m_beginText = false;
		}
		DocGen.PdfViewer.Base.Matrix txtMatrix = default(DocGen.PdfViewer.Base.Matrix);
		if (fontStructureHelperNet.fontType.Value != "Type3")
		{
			if (m_isCurrentPositionChanged)
			{
				m_isCurrentPositionChanged = false;
				m_endTextPosition = CurrentLocation;
				if (textElementHelperNet.structure.IsMappingDone && textElementHelperNet.structure.FontEncoding == "Identity-H" && textElementHelperNet.structure.CharacterMapTable.Count > 0)
				{
					textElementHelperNet.structure.IsMappingDone = false;
				}
				m_textElementWidth = textElementHelperNet.Render(m_graphics, new PointF(m_endTextPosition.X, m_endTextPosition.Y + (0f - TextLeading) / 4f), m_textScaling, fontGlyphWidths, fontStructureHelperNet.Type1GlyphHeight, fontStructureHelperNet.differenceTable, fontStructureHelperNet.DifferencesDictionary, fontStructureHelperNet.differenceEncoding, out txtMatrix);
				decodedStringData.Add(GetTextFromGlyph(textElementHelperNet.textElementGlyphList, 0));
				imageRenderGlyphList.AddRange(textElementHelperNet.textElementGlyphList);
				textElementHelperNet.textElementGlyphList.Clear();
			}
			else
			{
				m_endTextPosition = new PointF(m_endTextPosition.X + m_textElementWidth, m_endTextPosition.Y);
				m_textElementWidth = textElementHelperNet.Render(m_graphics, new PointF(m_endTextPosition.X, m_endTextPosition.Y + (0f - TextLeading) / 4f), m_textScaling, fontGlyphWidths, fontStructureHelperNet.Type1GlyphHeight, fontStructureHelperNet.differenceTable, fontStructureHelperNet.DifferencesDictionary, fontStructureHelperNet.differenceEncoding, out txtMatrix);
				decodedStringData.Add(GetTextFromGlyph(textElementHelperNet.textElementGlyphList, 0));
				imageRenderGlyphList.AddRange(textElementHelperNet.textElementGlyphList);
				textElementHelperNet.textElementGlyphList.Clear();
			}
		}
		else
		{
			DocGen.PdfViewer.Base.Matrix textLineMatrix = TextLineMatrix;
			FontStructureHelperNet fontStructureHelperNet2 = fontStructureHelperNet;
			m_graphics.Transform = new DocGen.Drawing.Matrix((float)DocumentMatrix.M11, (float)DocumentMatrix.M12, (float)DocumentMatrix.M21, (float)DocumentMatrix.M22, (float)DocumentMatrix.OffsetX, (float)DocumentMatrix.OffsetY);
			m_graphics.TranslateTransform(0f, (float)TextLineMatrix.OffsetY);
			m_spacingWidth = 0f;
			RenderType3GlyphImages(fontStructureHelperNet, textToDecode);
			TextLineMatrix = textLineMatrix;
			fontStructureHelperNet = fontStructureHelperNet2;
		}
		TextMatrix = txtMatrix;
		string text = textElementHelperNet.renderedText;
		if (!fontStructureHelperNet.IsMappingDone)
		{
			if (fontStructureHelperNet.CharacterMapTable != null && fontStructureHelperNet.CharacterMapTable.Count > 0)
			{
				text = fontStructureHelperNet.MapCharactersFromTable(text);
			}
			else if (fontStructureHelperNet.DifferencesDictionary != null && fontStructureHelperNet.DifferencesDictionary.Count > 0)
			{
				text = fontStructureHelperNet.MapDifferences(text);
			}
		}
		_ = CTRM * TextLineMatrix;
		float num = new PdfUnitConverter().ConvertFromPixels(currentPageHeight, PdfGraphicsUnit.Point);
		DrawNewLine();
		if (!text.Contains("www") && !text.Contains("http") && !IsValidEmail(text) && !text.Contains(".com"))
		{
			return;
		}
		float num2 = (float)TextLineMatrix.M11 * FontSize;
		SizeF sizeF = SizeF.Empty;
		using (SKPaint sKPaint3 = new SKPaint())
		{
			using SKTypeface typeface2 = SKTypeface.FromFamilyName(fontStructureHelperNet.FontName, GetFontStyle(fontStructureHelperNet.FontStyle));
			sKPaint3.Typeface = typeface2;
			sKPaint3.TextSize = num2;
			sizeF = m_graphics.MeasureString(text, sKPaint2);
		}
		_ = TextLeading;
		DocGen.PdfViewer.Base.Matrix matrix = CTRM * TextLineMatrix;
		matrix *= new DocGen.PdfViewer.Base.Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0f - num);
		currentTransformLocation = new PointF((float)matrix.OffsetX, 0f - ((float)matrix.OffsetY + num2));
		Regex regex = new Regex("\\b(?:http://|https://|www\\.)\\S+\\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		string uRI = string.Empty;
		foreach (Match item2 in regex.Matches(text))
		{
			uRI = item2.Value;
		}
		if (IsValidEmail(textElementHelperNet.renderedText) || text.Contains("@"))
		{
			uRI = "mailto:" + text;
		}
		PageURLNet item = new PageURLNet(transformMatrix, uRI, new PointF(currentTransformLocation.X, currentTransformLocation.Y), sizeF.Width, sizeF.Height);
		URLDictonary.Add(item);
	}

	private void RenderTextElementWithSpacing(string[] textElements, string tokenType)
	{
		List<string> list = new List<string>();
		string text = string.Join("", textElements);
		if (!m_resources.ContainsKey(CurrentFont))
		{
			return;
		}
		(m_resources[CurrentFont] as FontStructureHelperNet).IsSameFont = m_resources.isSameFont();
		if ((m_resources[CurrentFont] as FontStructureHelperNet).FontSize != FontSize)
		{
			(m_resources[CurrentFont] as FontStructureHelperNet).FontSize = FontSize;
		}
		FontStructureHelperNet fontStructureHelperNet = m_resources[CurrentFont] as FontStructureHelperNet;
		if (fontStructureHelperNet == null)
		{
			return;
		}
		List<float> characterSpacings = null;
		string decodedString;
		if (fontStructureHelperNet.FontName == "ZapfDingbats" && !fontStructureHelperNet.IsEmbedded)
		{
			list = DecodeTextTJ(fontStructureHelperNet, text, m_resources.isSameFont(), out decodedString);
		}
		else if (fontStructureHelperNet.FontEncoding == "Identity-H" || (fontStructureHelperNet.FontName == "MinionPro" && fontStructureHelperNet.FontEncoding == "Encoding"))
		{
			fontStructureHelperNet.IsMappingDone = false;
			list = fontStructureHelperNet.DecodeTextTJ(text, m_resources.isSameFont());
			decodedString = "[";
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Length != 0 && list[i][list[i].Length - 1] == 's')
				{
					string empty = string.Empty;
					empty = ((fontStructureHelperNet.CharacterMapTable != null && fontStructureHelperNet.CharacterMapTable.Count > 0) ? fontStructureHelperNet.MapCharactersFromTable(list[i].Substring(0, list[i].Length - 1)) : ((fontStructureHelperNet.DifferencesDictionary == null || fontStructureHelperNet.DifferencesDictionary.Count <= 0) ? list[i].Substring(0, list[i].Length - 1) : fontStructureHelperNet.MapDifferences(empty)));
					if (fontStructureHelperNet.CidToGidMap != null)
					{
						empty = fontStructureHelperNet.MapCidToGid(empty);
					}
					decodedString = decodedString + "(" + empty + ")";
				}
				else
				{
					decodedString += list[i];
				}
			}
			decodedString += "]";
		}
		else
		{
			list = fontStructureHelperNet.DecodeTextTJ(text, m_resources.isSameFont());
			decodedString = text;
		}
		TextElementHelperNet textElementHelperNet = new TextElementHelperNet(decodedString, DocumentMatrix);
		textElementHelperNet.FontStyle = fontStructureHelperNet.FontStyle;
		textElementHelperNet.Font = fontStructureHelperNet.CurrentFont;
		textElementHelperNet.FontName = fontStructureHelperNet.FontName;
		SKPaint sKPaint2;
		if (!skpaintCache.ContainsKey(CurrentFont + fontStructureHelperNet.FontRefNumber))
		{
			SKPaint sKPaint = new SKPaint();
			SKTypeface typeface = SKTypeface.FromFamilyName(fontStructureHelperNet.FontName, GetFontStyle(fontStructureHelperNet.FontStyle));
			sKPaint.Typeface = typeface;
			sKPaint.TextSize = fontStructureHelperNet.FontSize;
			sKPaint2 = sKPaint;
			skpaintCache[CurrentFont + fontStructureHelperNet.FontRefNumber] = sKPaint2;
		}
		else
		{
			sKPaint2 = skpaintCache[CurrentFont + fontStructureHelperNet.FontRefNumber];
		}
		textElementHelperNet.skpaint = sKPaint2;
		textElementHelperNet.FontSize = FontSize;
		textElementHelperNet.TextScaling = m_textScaling;
		textElementHelperNet.FontEncoding = fontStructureHelperNet.FontEncoding;
		textElementHelperNet.FontGlyphWidths = fontStructureHelperNet.FontGlyphWidths;
		textElementHelperNet.DefaultGlyphWidth = fontStructureHelperNet.DefaultGlyphWidth;
		textElementHelperNet.RenderingMode = RenderingMode;
		textElementHelperNet.IsFindText = isFindText;
		textElementHelperNet.UnicodeCharMapTable = fontStructureHelperNet.UnicodeCharMapTable;
		Dictionary<int, int> fontGlyphWidths = fontStructureHelperNet.FontGlyphWidths;
		textElementHelperNet.CidToGidReverseMapTable = fontStructureHelperNet.CidToGidReverseMapTable;
		textElementHelperNet.IsType1Font = fontStructureHelperNet.IsType1Font;
		textElementHelperNet.Is1C = fontStructureHelperNet.Is1C;
		textElementHelperNet.IsCID = fontStructureHelperNet.IsCID;
		textElementHelperNet.CharacterMapTable = fontStructureHelperNet.CharacterMapTable;
		textElementHelperNet.ReverseMapTable = fontStructureHelperNet.ReverseMapTable;
		textElementHelperNet.GlyfDatapath = fontStructureHelperNet.Graphic;
		textElementHelperNet.Fontfile2Glyph = fontStructureHelperNet.GlyphFontFile2;
		textElementHelperNet.structure = fontStructureHelperNet;
		textElementHelperNet.Isembeddedfont = fontStructureHelperNet.IsEmbedded;
		textElementHelperNet.Ctm = CTRM;
		textElementHelperNet.textLineMatrix = TextMatrix;
		textElementHelperNet.Rise = Rise;
		textElementHelperNet.transformMatrix = DocumentMatrix;
		textElementHelperNet.documentMatrix = DocumentMatrix;
		textElementHelperNet.FontID = CurrentFont;
		textElementHelperNet.OctDecMapTable = fontStructureHelperNet.OctDecMapTable;
		textElementHelperNet.TextHorizontalScaling = HorizontalScaling;
		textElementHelperNet.ZapfPostScript = fontStructureHelperNet.ZapfPostScript;
		textElementHelperNet.LineWidth = MitterLength;
		textElementHelperNet.RenderingMode = RenderingMode;
		textElementHelperNet.testdict = testdict;
		textElementHelperNet.pageRotation = pageRotation;
		textElementHelperNet.zoomFactor = zoomFactor;
		textElementHelperNet.SubstitutedFontsList = substitutedFontsList;
		if (fontStructureHelperNet.BaseFontEncoding == "WinAnsiEncoding")
		{
			isWinAnsiEncoding = true;
		}
		if (fontStructureHelperNet.Flags != null)
		{
			textElementHelperNet.FontFlag = fontStructureHelperNet.Flags.IntValue;
		}
		if (fontStructureHelperNet.IsType1Font)
		{
			textElementHelperNet.IsType1Font = true;
			textElementHelperNet.differenceTable = fontStructureHelperNet.differenceTable;
			textElementHelperNet.differenceMappedTable = fontStructureHelperNet.DifferencesDictionary;
			textElementHelperNet.m_cffGlyphs = fontStructureHelperNet.m_cffGlyphs;
			if (!m_glyphDataCollection.Contains(textElementHelperNet.m_cffGlyphs))
			{
				m_glyphDataCollection.Add(textElementHelperNet.m_cffGlyphs);
			}
		}
		if (StrokingBrush != null)
		{
			if (textElementHelperNet.RenderingMode == 3)
			{
				textElementHelperNet.PathBrush = new PdfPen(Color.Transparent).Brush;
			}
			else
			{
				textElementHelperNet.PathBrush = StrokingBrush;
				if (isXGraphics)
				{
					Color color = ((PdfSolidBrush)StrokingBrush).Color;
					float num = Math.Max(0f, Math.Min(1f, Objects.m_strokingOpacity));
					int alpha = (int)Math.Floor((num == 1f) ? 255f : (num * 255f));
					((PdfSolidBrush)textElementHelperNet.PathBrush).Color = Color.FromArgb(alpha, color.R, color.G, color.B);
				}
			}
		}
		else
		{
			textElementHelperNet.PathBrush = new PdfPen(Color.Black).Brush;
		}
		if (NonStrokingBrush != null)
		{
			textElementHelperNet.PathNonStrokeBrush = NonStrokingBrush;
		}
		textElementHelperNet.WordSpacing = Objects.WordSpacing;
		textElementHelperNet.CharacterSpacing = Objects.CharacterSpacing;
		if (m_beginText)
		{
			m_beginText = false;
		}
		DocGen.PdfViewer.Base.Matrix textmatrix = default(DocGen.PdfViewer.Base.Matrix);
		if (fontStructureHelperNet.fontType.Value != "Type3")
		{
			if (m_isCurrentPositionChanged)
			{
				m_isCurrentPositionChanged = false;
				m_endTextPosition = CurrentLocation;
				m_textElementWidth = 0f;
				int startIndex = 0;
				string fontName = textElementHelperNet.FontName;
				textElementHelperNet.m_isRectation = true;
				foreach (string item2 in list)
				{
					List<string> list2 = new List<string>();
					list2.Add(item2);
					if (fontName == "Zapdingbats" && !fontStructure.IsEmbedded)
					{
						textElementHelperNet.FontName = fontName;
					}
					m_textElementWidth += textElementHelperNet.RenderWithSpace(m_graphics, new PointF(m_endTextPosition.X, m_endTextPosition.Y - FontSize), list2, characterSpacings, m_textScaling, fontGlyphWidths, fontStructureHelperNet.Type1GlyphHeight, fontStructureHelperNet.differenceTable, fontStructureHelperNet.DifferencesDictionary, fontStructureHelperNet.differenceEncoding, out textmatrix);
					decodedStringData.Add(GetTextFromGlyph(textElementHelperNet.textElementGlyphList, startIndex));
					startIndex = textElementHelperNet.textElementGlyphList.Count;
				}
				imageRenderGlyphList.AddRange(textElementHelperNet.textElementGlyphList);
				textElementHelperNet.textElementGlyphList.Clear();
				textElementHelperNet.m_isRectation = false;
			}
			else
			{
				m_endTextPosition = new PointF(m_endTextPosition.X + m_textElementWidth, m_endTextPosition.Y);
				m_textElementWidth = 0f;
				int startIndex2 = 0;
				textElementHelperNet.m_isRectation = true;
				foreach (string item3 in list)
				{
					List<string> list3 = new List<string>();
					list3.Add(item3);
					m_textElementWidth = textElementHelperNet.RenderWithSpace(m_graphics, new PointF(m_endTextPosition.X, m_endTextPosition.Y - FontSize), list3, characterSpacings, m_textScaling, fontGlyphWidths, fontStructureHelperNet.Type1GlyphHeight, fontStructureHelperNet.differenceTable, fontStructureHelperNet.DifferencesDictionary, fontStructureHelperNet.differenceEncoding, out textmatrix);
					decodedStringData.Add(GetTextFromGlyph(textElementHelperNet.textElementGlyphList, startIndex2));
					startIndex2 = textElementHelperNet.textElementGlyphList.Count;
				}
				imageRenderGlyphList.AddRange(textElementHelperNet.textElementGlyphList);
				textElementHelperNet.textElementGlyphList.Clear();
				textElementHelperNet.m_isRectation = false;
			}
		}
		else
		{
			DocGen.PdfViewer.Base.Matrix textLineMatrix = TextLineMatrix;
			FontStructureHelperNet fontStructureHelperNet2 = fontStructureHelperNet;
			m_graphics.Transform = new DocGen.Drawing.Matrix((float)DocumentMatrix.M11, (float)DocumentMatrix.M12, (float)DocumentMatrix.M21, (float)DocumentMatrix.M22, (float)DocumentMatrix.OffsetX, (float)DocumentMatrix.OffsetY);
			m_graphics.TranslateTransform(0f, (float)TextLineMatrix.OffsetY);
			m_spacingWidth = 0f;
			RenderType3GlyphImagesTJ(fontStructureHelperNet, list);
			m_textElementWidth = 0f;
			int startIndex3 = 0;
			textElementHelperNet.m_isRectation = true;
			foreach (string item4 in list)
			{
				List<string> list4 = new List<string>();
				list4.Add(item4);
				m_textElementWidth = textElementHelperNet.RenderWithSpace(m_graphics, new PointF(m_endTextPosition.X, m_endTextPosition.Y - FontSize), list4, characterSpacings, m_textScaling, fontGlyphWidths, fontStructureHelperNet.Type1GlyphHeight, fontStructureHelperNet.differenceTable, fontStructureHelperNet.DifferencesDictionary, fontStructureHelperNet.differenceEncoding, out textmatrix);
				decodedStringData.Add(GetTextFromGlyph(textElementHelperNet.textElementGlyphList, startIndex3));
				startIndex3 = textElementHelperNet.textElementGlyphList.Count;
			}
			imageRenderGlyphList.AddRange(textElementHelperNet.textElementGlyphList);
			textElementHelperNet.textElementGlyphList.Clear();
			TextLineMatrix = textLineMatrix;
			fontStructureHelperNet = fontStructureHelperNet2;
			textElementHelperNet.m_isRectation = false;
		}
		TextMatrix = textmatrix;
		string text2 = textElementHelperNet.renderedText;
		if (!fontStructureHelperNet.IsMappingDone)
		{
			if (fontStructureHelperNet.CharacterMapTable != null && fontStructureHelperNet.CharacterMapTable.Count > 0)
			{
				text2 = fontStructureHelperNet.MapCharactersFromTable(text2);
			}
			else if (fontStructureHelperNet.DifferencesDictionary != null && fontStructureHelperNet.DifferencesDictionary.Count > 0)
			{
				text2 = fontStructureHelperNet.MapDifferences(text2);
			}
		}
		_ = CTRM * TextLineMatrix;
		float num2 = new PdfUnitConverter().ConvertFromPixels(currentPageHeight, PdfGraphicsUnit.Point);
		if (!text2.Contains("www") && !text2.Contains("http") && !IsValidEmail(text2) && !text2.Contains(".com"))
		{
			return;
		}
		float num3 = 0f;
		if (TextLineMatrix.M11 != 0.0 && TextLineMatrix.M22 != 0.0)
		{
			num3 = (float)TextLineMatrix.M11 * FontSize;
			if (TextLineMatrix.M11 <= 0.0)
			{
				num3 = FontSize;
			}
		}
		else if (TextLineMatrix.M12 != 0.0 && TextLineMatrix.M21 != 0.0)
		{
			num3 = (float)TextLineMatrix.M12 * FontSize;
			if (TextLineMatrix.M12 <= 0.0)
			{
				num3 = FontSize;
			}
		}
		SizeF sizeF = SizeF.Empty;
		using (SKPaint sKPaint3 = new SKPaint())
		{
			using SKTypeface typeface2 = SKTypeface.FromFamilyName(fontStructureHelperNet.FontName, GetFontStyle(fontStructureHelperNet.FontStyle));
			sKPaint3.Typeface = typeface2;
			sKPaint3.TextSize = num3;
			sizeF = m_graphics.MeasureString(text2, sKPaint2);
		}
		_ = TextLeading;
		DocGen.PdfViewer.Base.Matrix matrix = CTRM * TextLineMatrix;
		if (TextLineMatrix.M12 != 0.0 && TextLineMatrix.M21 != 0.0)
		{
			matrix.OffsetX = TextLineMatrix.OffsetY;
			matrix.OffsetY = 0.0 - TextLineMatrix.OffsetX;
		}
		else
		{
			matrix.OffsetX = TextLineMatrix.OffsetX;
			matrix.OffsetY = TextLineMatrix.OffsetY;
			matrix *= new DocGen.PdfViewer.Base.Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0f - num2);
		}
		currentTransformLocation = new PointF((float)matrix.OffsetX, 0f - ((float)matrix.OffsetY + num3));
		Regex regex = new Regex("\\b(?:http://|https://|www\\.)\\S+\\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		string uRI = string.Empty;
		foreach (Match item5 in regex.Matches(text2))
		{
			uRI = item5.Value;
		}
		if (IsValidEmail(textElementHelperNet.renderedText) || text2.Contains("@"))
		{
			uRI = "mailto:" + text2;
		}
		PageURLNet item = ((m_endTextPosition.Y + (0f - TextLeading - FontSize) == 0f) ? new PageURLNet(transformMatrix, uRI, new PointF(m_endTextPosition.X, m_endTextPosition.Y), m_textElementWidth, FontSize) : new PageURLNet(transformMatrix, uRI, new PointF(currentTransformLocation.X, currentTransformLocation.Y), sizeF.Width, sizeF.Height));
		URLDictonary.Add(item);
	}

	private bool IsValidEmail(string email)
	{
		Regex regex = new Regex("^[-a-zA-Z0-9][-.a-zA-Z0-9]*@[-.a-zA-Z0-9]+(\\.[-.a-zA-Z0-9]+)*\\.(com|edu|info|gov|int|mil|net|org|biz|name|museum|coop|aero|pro|tv|[a-zA-Z]{2})$", RegexOptions.IgnorePatternWhitespace);
		bool flag = false;
		if (string.IsNullOrEmpty(email))
		{
			return false;
		}
		return regex.IsMatch(email);
	}

	private Color GetColor(string[] colorElement, string type, string colorSpace)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 1f;
		if (colorSpace == "RGB" && colorElement.Length == 3)
		{
			num = float.Parse(colorElement[0], NumberStyles.Float, CultureInfo.InvariantCulture);
			num2 = float.Parse(colorElement[1], NumberStyles.Float, CultureInfo.InvariantCulture);
			num3 = float.Parse(colorElement[2], NumberStyles.Float, CultureInfo.InvariantCulture);
			num4 = m_opacity;
		}
		else if (colorSpace == "Gray" && colorElement.Length == 1)
		{
			num = (num2 = (num3 = (colorElement[0].Contains("/") ? 0f : ((!isBlack) ? float.Parse(colorElement[0], NumberStyles.Float, CultureInfo.InvariantCulture) : 0f))));
			num4 = m_opacity;
		}
		else if (colorSpace == "DeviceCMYK" && colorElement.Length == 4)
		{
			float.TryParse(colorElement[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var result);
			float.TryParse(colorElement[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var result2);
			float.TryParse(colorElement[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var result3);
			float.TryParse(colorElement[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var result4);
			return ConvertCMYKtoRGB(result, result2, result3, result4);
		}
		return Color.FromArgb((byte)(num4 * 255f), (byte)(num * 255f), (byte)(num2 * 255f), (byte)(num3 * 255f));
	}

	private Color ConvertCMYKtoRGB(float c, float m, float y, float k)
	{
		float num = 255f * (1f - c) * (1f - k);
		float num2 = 255f * (1f - m) * (1f - k);
		float num3 = 255f * (1f - y) * (1f - k);
		return Color.FromArgb(255, (int)((num > 255f) ? 255f : ((num < 0f) ? 0f : num)), (int)((num2 > 255f) ? 255f : ((num2 < 0f) ? 0f : num2)), (int)((num3 > 255f) ? 255f : ((num3 < 0f) ? 0f : num3)));
	}

	private void DrawNewLine()
	{
		m_isCurrentPositionChanged = true;
		if (m_resources[CurrentFont] is FontStructureHelperNet fontStructureHelperNet)
		{
			Font font = new Font(TextElementHelperNet.CheckFontName(fontStructureHelperNet.FontName), FontSize, fontStructureHelperNet.FontStyle);
			if (0f - TextLeading != 0f)
			{
				m_currentLocation.Y = ((0f - TextLeading < 0f) ? (m_currentLocation.Y - (0f - TextLeading)) : (m_currentLocation.Y + (0f - TextLeading)));
			}
			else
			{
				m_currentLocation.Y += font.Size;
			}
			font.Dispose();
		}
	}

	private void GetClipRectangle(string[] rectangle)
	{
		if (!m_selectablePrintDocument)
		{
			float num = float.Parse(rectangle[0], NumberStyles.Float, CultureInfo.InvariantCulture);
			float num2 = float.Parse(rectangle[1], NumberStyles.Float, CultureInfo.InvariantCulture);
			float num3 = float.Parse(rectangle[2], NumberStyles.Float, CultureInfo.InvariantCulture);
			float num4 = float.Parse(rectangle[3], NumberStyles.Float, CultureInfo.InvariantCulture);
			if (num < 0f && num4 < 0f && isNextFill && 0f - num4 >= currentPageHeight)
			{
				num = 0f;
				num4 = 0f;
			}
			isNextFill = false;
			BeginPath(num, num2);
			AddLine(num + num3, num2);
			AddLine(num + num3, num2 + num4);
			AddLine(num, num2 + num4);
			EndPath();
			new RectangleF(num, num2, num3, num4);
			return;
		}
		clipRectShape = rectangle;
		isRect = true;
		float num5 = float.Parse(rectangle[0], NumberStyles.Float, CultureInfo.InvariantCulture);
		float num6 = 0f - float.Parse(rectangle[1], NumberStyles.Float, CultureInfo.InvariantCulture);
		float num7 = float.Parse(rectangle[2], NumberStyles.Float, CultureInfo.InvariantCulture);
		float num8 = 0f - float.Parse(rectangle[3], NumberStyles.Float, CultureInfo.InvariantCulture);
		RectangleF rectangleF = new RectangleF(num5, num6, num7, num8);
		bool flag = false;
		bool flag2 = false;
		if (num8 < 0f)
		{
			num8 = 0f - num8;
			num6 -= num8;
			flag = true;
		}
		if (num7 < 0f)
		{
			num7 = 0f - num7;
			num5 -= num7;
			flag2 = true;
		}
		if (flag && flag2)
		{
			rectangleF = new RectangleF(num5, num6, num7, num8);
		}
		if (ClipRectangle != RectangleF.Empty)
		{
			m_clipRectangle.Intersect(rectangleF);
		}
		else
		{
			m_clipRectangle = new RectangleF(num5, num6, num7, num8);
		}
		Path = new GraphicsPath();
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddRectangle(rectangleF);
		m_tempSubPaths.Add(graphicsPath);
	}

	private void GetWordSpacing(string[] spacing)
	{
		Objects.WordSpacing = float.Parse(spacing[0], NumberStyles.Float, CultureInfo.InvariantCulture);
	}

	private void GetCharacterSpacing(string[] spacing)
	{
		Objects.CharacterSpacing = float.Parse(spacing[0], NumberStyles.Float, CultureInfo.InvariantCulture);
		m_characterSpacing = Objects.CharacterSpacing;
	}

	private void GetScalingFactor(string[] scaling)
	{
		m_textScaling = float.Parse(scaling[0], NumberStyles.Float, CultureInfo.InvariantCulture);
		HorizontalScaling = float.Parse(scaling[0], NumberStyles.Float, CultureInfo.InvariantCulture);
	}

	private void writePoint(PdfStream stream, PointF point)
	{
		stream.Write(PdfNumber.FloatToString(point.X));
		stream.Write(" ");
		stream.Write(PdfNumber.FloatToString(point.Y));
		stream.Write(" ");
	}

	private DocGen.PdfViewer.Base.Matrix CalculateTextMatrixupdate(DocGen.PdfViewer.Base.Matrix m, float Width, bool isImage)
	{
		double offsetX = ((!isImage) ? ((double)(Width * FontSize) * 0.001) : ((double)(Width * 1f)));
		return new DocGen.PdfViewer.Base.Matrix(1.0, 0.0, 0.0, 1.0, offsetX, 0.0) * m;
	}

	private DocGen.Drawing.Matrix GetTransformationMatrix(DocGen.PdfViewer.Base.Matrix transform)
	{
		return new DocGen.Drawing.Matrix((float)transform.M11, (float)transform.M12, (float)transform.M21, (float)transform.M22, (float)transform.OffsetX, (float)transform.OffsetY);
	}

	private void BeginPath(float x, float y)
	{
		CurrentLocation = new PointF(x, y);
		if (m_currentPath != null && m_currentPath.Segments.Count == 0)
		{
			CurrentGeometry.Figures.Remove(CurrentPath);
		}
		m_currentPath = new PathFigure();
		m_currentPath.StartPoint = new PointF(CurrentLocation.X, CurrentLocation.Y);
		CurrentGeometry.Figures.Add(m_currentPath);
	}

	private void EndPath()
	{
		if (m_currentPath != null)
		{
			m_currentPath.IsClosed = true;
		}
	}

	private void AddLine(float x, float y)
	{
		CurrentLocation = new PointF(x, y);
		m_currentPath.Segments.Add(new LineSegment
		{
			Point = new PointF(CurrentLocation.X, CurrentLocation.Y)
		});
	}

	private GraphicsPath GetGeometry(PathGeometry geometry, DocGen.PdfViewer.Base.Matrix transform)
	{
		DocGen.Drawing.Matrix transformationMatrix = PdfElementsRendererNet.GetTransformationMatrix(transform);
		GraphicsPath graphicsPath = new GraphicsPath();
		foreach (PathFigure figure in geometry.Figures)
		{
			graphicsPath.StartFigure();
			PointF pointF = new PointF((float)figure.StartPoint.X, (float)figure.StartPoint.Y);
			foreach (PathSegment segment in figure.Segments)
			{
				if (segment is LineSegment)
				{
					LineSegment lineSegment = (LineSegment)segment;
					PointF[] array = new PointF[2]
					{
						pointF,
						new PointF((float)lineSegment.Point.X, (float)lineSegment.Point.Y)
					};
					transformationMatrix.TransformPoints(array);
					graphicsPath.AddLine(array[0], array[1]);
					pointF = new PointF((float)lineSegment.Point.X, (float)lineSegment.Point.Y);
				}
				else if (segment is BezierSegment)
				{
					BezierSegment bezierSegment = segment as BezierSegment;
					PointF[] array2 = new PointF[4]
					{
						pointF,
						new PointF((float)bezierSegment.Point1.X, (float)bezierSegment.Point1.Y),
						new PointF((float)bezierSegment.Point2.X, (float)bezierSegment.Point2.Y),
						new PointF((float)bezierSegment.Point3.X, (float)bezierSegment.Point3.Y)
					};
					transformationMatrix.TransformPoints(array2);
					graphicsPath.AddBezier(array2[0], array2[1], array2[2], array2[3]);
					pointF = new PointF((float)bezierSegment.Point3.X, (float)bezierSegment.Point3.Y);
				}
			}
			if (figure.IsClosed)
			{
				graphicsPath.CloseFigure();
			}
		}
		return graphicsPath;
	}

	private void GetXObject(string[] xobjectElement)
	{
		if (!m_resources.ContainsKey(xobjectElement[0].Replace("/", "")) || !(m_resources[xobjectElement[0].Replace("/", "")] is ImageStructureNet))
		{
			return;
		}
		ImageStructureNet imageStructureNet = m_resources[xobjectElement[0].Replace("/", "")] as ImageStructureNet;
		imageStructureNet.m_isExtGStateContainsSMask = m_isExtendedGraphicStateContainsSMask;
		Bitmap bitmap = null;
		DocGen.Drawing.Matrix transform = m_graphics.Transform;
		GraphicsUnit pageUnit = m_graphics.PageUnit;
		m_graphics.PageUnit = GraphicsUnit.Pixel;
		m_graphics.Transform = new DocGen.Drawing.Matrix(1f, 0f, 0f, 1f, 0f, 0f);
		DocGen.PdfViewer.Base.Matrix matrix = new DocGen.PdfViewer.Base.Matrix(Drawing2dMatrixCTM.Elements[0], Drawing2dMatrixCTM.Elements[1], Drawing2dMatrixCTM.Elements[2], Drawing2dMatrixCTM.Elements[3], Drawing2dMatrixCTM.OffsetX, Drawing2dMatrixCTM.OffsetY);
		matrix.Scale(1.0, -1.0, 0.0, 1.0);
		DocGen.PdfViewer.Base.Matrix matrix2 = matrix * DocumentMatrix;
		m_graphics.Transform = new DocGen.Drawing.Matrix((float)matrix2.M11, (float)matrix2.M12, (float)matrix2.M21, (float)matrix2.M22, (float)matrix2.OffsetX, (float)Math.Round((float)matrix2.OffsetY, 5));
		if (imageStructureNet.IsImageMask && bitmap != null)
		{
			if (bitmap == null)
			{
				bitmap = GetMaskImagefromStream(imageStructureNet);
			}
			else if (!imageStructureNet.m_isBlackIs1 && imageStructureNet.ImageFilter[0] != "DCTDecode")
			{
				bitmap = GetMaskImage(bitmap);
			}
			_ = m_outStream;
		}
		else
		{
			_ = imageStructureNet.outStream;
		}
		if (bitmap != null)
		{
			InterpolationMode interpolationMode = m_graphics.InterpolationMode;
			m_graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			ImageAttributes imageAttributes = new ImageAttributes();
			if (imageStructureNet.ImageDictionary != null && imageStructureNet.ImageDictionary.ContainsKey("Intent") && (imageStructureNet.ImageDictionary["Intent"] as PdfName).Value == "RelativeColorimetric")
			{
				ColorMatrix colorMatrix = new ColorMatrix();
				colorMatrix.Matrix33 = Objects.m_strokingOpacity;
				new ImageAttributes();
				imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
			}
			m_graphics.DrawImage(bitmap, new Rectangle(0, 0, 1, 1), 0f, 0f, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, imageAttributes);
			imageAttributes.Dispose();
			m_graphics.InterpolationMode = interpolationMode;
		}
		m_graphics.Transform = transform;
		m_graphics.PageUnit = pageUnit;
	}

	private void GetType3XObject(string[] xobjectElement)
	{
		if (m_resources.ContainsKey(xobjectElement[0].Replace("/", "")) && m_resources[xobjectElement[0].Replace("/", "")] is ImageStructureNet)
		{
			ImageStructureNet imageStructureNet = m_resources[xobjectElement[0].Replace("/", "")] as ImageStructureNet;
			new MemoryStream();
			ImageHelper imageHelper = null;
			if (imageStructureNet.EmbeddedImage != null && imageStructureNet.EmbeddedImage.m_sKBitmap != null)
			{
				imageHelper = new ImageHelper(imageStructureNet.EmbeddedImage.m_imageData);
			}
			FontStructureHelperNet type3FontStruct = m_type3FontStruct;
			TextElementHelperNet textElementHelperNet = new TextElementHelperNet(imageHelper, DocumentMatrix);
			textElementHelperNet.Font = type3FontStruct.CurrentFont;
			SKPaint skpaint;
			if (!skpaintCache.ContainsKey(CurrentFont + type3FontStruct.FontRefNumber))
			{
				SKPaint sKPaint = new SKPaint();
				SKTypeface typeface = SKTypeface.FromFamilyName(type3FontStruct.FontName, GetFontStyle(type3FontStruct.FontStyle));
				sKPaint.Typeface = typeface;
				sKPaint.TextSize = type3FontStruct.FontSize;
				skpaint = sKPaint;
			}
			else
			{
				skpaint = skpaintCache[CurrentFont + type3FontStruct.FontRefNumber];
			}
			textElementHelperNet.skpaint = skpaint;
			textElementHelperNet.type3GlyphImage = imageHelper;
			textElementHelperNet.FontSize = FontSize;
			textElementHelperNet.TextScaling = m_textScaling;
			textElementHelperNet.isNegativeFont = isNegativeFont;
			textElementHelperNet.UnicodeCharMapTable = type3FontStruct.UnicodeCharMapTable;
			Dictionary<int, int> fontGlyphWidths = type3FontStruct.FontGlyphWidths;
			textElementHelperNet.structure = type3FontStruct;
			textElementHelperNet.Ctm = CTRM;
			textElementHelperNet.textLineMatrix = new DocGen.PdfViewer.Base.Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);
			textElementHelperNet.Rise = Rise;
			textElementHelperNet.documentMatrix = DocumentMatrix;
			textElementHelperNet.FontID = CurrentFont;
			textElementHelperNet.OctDecMapTable = type3FontStruct.OctDecMapTable;
			textElementHelperNet.TextHorizontalScaling = HorizontalScaling;
			textElementHelperNet.ZapfPostScript = type3FontStruct.ZapfPostScript;
			textElementHelperNet.LineWidth = MitterLength;
			textElementHelperNet.RenderingMode = RenderingMode;
			textElementHelperNet.testdict = testdict;
			if (StrokingBrush != null)
			{
				textElementHelperNet.PathBrush = StrokingBrush;
			}
			else
			{
				textElementHelperNet.PathBrush = new PdfPen(Color.Black).Brush;
			}
			textElementHelperNet.WordSpacing = Objects.WordSpacing;
			textElementHelperNet.CharacterSpacing = Objects.CharacterSpacing;
			DocGen.PdfViewer.Base.Matrix txtMatrix = default(DocGen.PdfViewer.Base.Matrix);
			textElementHelperNet.Render(m_graphics, new PointF(m_endTextPosition.X, m_endTextPosition.Y - FontSize), m_textScaling, fontGlyphWidths, type3FontStruct.Type1GlyphHeight, type3FontStruct.differenceTable, type3FontStruct.DifferencesDictionary, type3FontStruct.differenceEncoding, out txtMatrix);
			TextLineMatrix = CalculateTextMatrixupdate(TextLineMatrix, (float)m_d1Matrix.M11, isImage: true);
			m_type3TextLineMatrix = TextLineMatrix;
		}
	}

	private DocGen.PdfViewer.Base.Matrix CalculateType3TextMatrixupdate(DocGen.PdfViewer.Base.Matrix m, float Width, bool isImage)
	{
		double offsetX = ((double)Width * (double)FontSize + (double)Objects.CharacterSpacing + (double)m_type3WhiteSpaceWidth) * (double)(HorizontalScaling / 100f);
		return new DocGen.PdfViewer.Base.Matrix(1.0, 0.0, 0.0, 1.0, offsetX, 0.0) * m;
	}

	private Bitmap GetMaskImagefromStream(ImageStructureNet structure)
	{
		try
		{
			byte[] array = structure.outStream.ToArray();
			Color color = ((StrokingBrush != null) ? ((Color)new PdfPen(StrokingBrush).Color) : ((Color)new PdfPen(Color.Black).Color));
			int item = (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
			int num = 1;
			int num2 = 0;
			int num3 = 0;
			int num4 = (1 << num) - 1;
			int num5 = num;
			int num6 = 0;
			int num7 = 0;
			PdfArray decodeArray = structure.DecodeArray;
			if (decodeArray != null)
			{
				num7 = (decodeArray[0] as PdfNumber).IntValue;
			}
			List<int> list = new List<int>();
			for (int i = 0; i < (int)structure.Height; i++)
			{
				for (int j = 0; j < (int)structure.Width; j++)
				{
					if (num5 == 8)
					{
						num6 = array[num2++];
					}
					if (num5 != 16)
					{
						int num8 = (array[num2] >> 8 - num3 - num) & num4;
						num3 += num;
						if (num3 == 8)
						{
							num2++;
							num3 = 0;
						}
						num6 = num8;
					}
					else
					{
						num6 = (array[num2++] << 8) | array[num2++];
					}
					if (num7 == num6)
					{
						list.Add(item);
					}
					else
					{
						list.Add(num6);
					}
				}
				if (num3 != 0)
				{
					num2++;
					num3 = 0;
				}
			}
			int[] array2 = list.ToArray();
			Bitmap bitmap = new Bitmap((int)structure.Width, (int)structure.Height);
			BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			byte[] array3 = new byte[bitmapData.Stride * bitmap.Height];
			for (int k = 0; k < array2.Length; k++)
			{
				Color color2 = Color.FromArgb(array2[k]);
				int num9 = k % (int)structure.Width;
				int num10 = k / (int)structure.Width;
				array3[bitmapData.Stride * num10 + 4 * num9] = color2.B;
				array3[bitmapData.Stride * num10 + 4 * num9 + 1] = color2.G;
				array3[bitmapData.Stride * num10 + 4 * num9 + 2] = color2.R;
				array3[bitmapData.Stride * num10 + 4 * num9 + 3] = color2.A;
			}
			Marshal.Copy(array3, 0, bitmap.m_sKBitmap.GetPixels(), array3.Length);
			bitmap.UnlockBits(bitmapData);
			m_outStream = new MemoryStream();
			bitmap.Save(m_outStream, ImageFormat.Png);
			return new Bitmap(m_outStream);
		}
		catch
		{
			return null;
		}
	}

	private Bitmap GetMaskImage(Bitmap currentimage)
	{
		if (StrokingBrush != null || NonStrokingBrush != null)
		{
			int height = currentimage.Height;
			int width = currentimage.Width;
			Color color = default(Color);
			Color color2 = default(Color);
			if (StrokingBrush != null)
			{
				color = new PdfPen(StrokingBrush).Color;
			}
			else if (NonStrokingBrush != null)
			{
				color2 = new PdfPen(NonStrokingBrush).Color;
			}
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					if (currentimage.GetPixel(j, i).ToArgb() == Color.Black.ToArgb())
					{
						if (StrokingBrush != null)
						{
							currentimage.SetPixel(j, i, color);
						}
						else if (NonStrokingBrush != null)
						{
							currentimage.SetPixel(j, i, color2);
						}
					}
				}
			}
		}
		return currentimage;
	}
}
