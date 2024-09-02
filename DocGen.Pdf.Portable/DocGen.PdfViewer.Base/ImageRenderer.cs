using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using DocGen.Drawing;
using DocGen.Pdf;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.PdfViewer.Base;

internal class ImageRenderer
{
	private bool m_isNegative;

	private Dictionary<string, bool> m_layersVisibilityDictionary = new Dictionary<string, bool>();

	private bool m_skipRendering;

	private int m_inlayersCount;

	internal bool IsTextSearch;

	internal bool IsPdfium;

	private string[] m_rectangleValue;

	private bool m_isTrEntry;

	private bool m_isExtendedGraphicStateContainsSMask;

	public bool IsExtendedGraphicsState;

	public bool IsGraphicsState;

	public bool IsPdfiumRendering;

	public Color TextColor;

	internal Matrix transformMatrix;

	internal GraphicsObject m_graphicsObject = new GraphicsObject();

	internal PointF currentTransformLocation;

	private char[] m_symbolChars = new char[6] { '(', ')', '[', ']', '<', '>' };

	private char[] m_startText = new char[3] { '(', '[', '<' };

	private char[] m_endText = new char[3] { ')', ']', '>' };

	private PdfPageResources m_resources;

	private PdfRecordCollection m_contentElements;

	private PointF m_currentLocation = PointF.Empty;

	private bool m_beginText;

	private RectangleF m_clipRectangle;

	private float m_mitterLength;

	internal Stack<GraphicsState> m_graphicsState = new Stack<GraphicsState>();

	internal Stack<GraphicObjectData> m_objects = new Stack<GraphicObjectData>();

	private float m_textScaling = 100f;

	private bool textMatrix;

	private float m_textElementWidth;

	private string[] m_backupColorElements;

	private string m_backupColorSpace;

	private PointF m_endTextPosition;

	private bool m_isCurrentPositionChanged;

	internal bool isFindText;

	internal bool isExportAsImage;

	internal bool isExtractLineCollection;

	private PdfViewerExceptions exception = new PdfViewerExceptions();

	private DeviceCMYK decodecmykColor;

	private string[] m_dashedLine;

	private bool isNegativeFont;

	private string m_clippingPath;

	private float m_lineCap = float.MinValue;

	private float m_lineJoin = float.MinValue;

	private int RenderingMode;

	private float m_opacity;

	private List<RectangleF> m_clipRectangleList = new List<RectangleF>();

	private bool IsTransparentText;

	private List<CffGlyphs> m_glyphDataCollection = new List<CffGlyphs>();

	private Matrix m_imageCommonMatrix;

	public char findpath;

	private bool isScaledText;

	private float currentPageHeight;

	public bool isBlack;

	private Dictionary<SystemFontFontDescriptor, SystemFontOpenTypeFontSource> testdict = new Dictionary<SystemFontFontDescriptor, SystemFontOpenTypeFontSource>();

	private PdfDictionary m_inlineParameters = new PdfDictionary();

	internal Dictionary<string, string> substitutedFontsList = new Dictionary<string, string>();

	private List<string> BIparameter = new List<string>();

	internal List<Glyph> imageRenderGlyphList;

	internal List<TextElement> extractTextElement = new List<TextElement>();

	internal float pageRotation;

	internal float zoomFactor = 1f;

	private bool isNextFill;

	private bool isWinAnsiEncoding;

	private int m_tilingType;

	private bool m_isExtractTextData;

	internal bool m_extractTags;

	private int m_mcid = -1;

	internal Dictionary<int, string> m_abbreviationCollection;

	private string m_currentFontName;

	internal bool continueOnError = true;

	private Matrix XFormsMatrix;

	private Matrix m_d1Matrix;

	private Matrixx m_d0Matrix;

	private Matrix m_type3TextLineMatrix;

	private float m_type3FontScallingFactor;

	private PdfRecordCollection m_type3RecordCollection;

	private bool m_isType3Font;

	private TransformationStack m_transformations;

	private FontStructure m_type3FontStruct;

	private float m_spacingWidth;

	private string m_type3GlyphID;

	private float m_type3WhiteSpaceWidth;

	private Dictionary<string, List<List<int>>> m_type3GlyphPath;

	private bool m_istype3FontContainCTRM;

	private MemoryStream m_outStream;

	internal PathGeometry CurrentGeometry = new PathGeometry();

	private PathGeometry BackupCurrentGeometry = new PathGeometry();

	private PathFigure m_currentPath;

	private bool containsImage;

	internal int xobjectGraphicsCount;

	internal bool isXGraphics;

	private string[] clipRectShape;

	private bool isRect;

	internal float m_characterSpacing;

	internal float m_wordSpacing;

	internal bool m_selectablePrintDocument;

	private float m_textAngle;

	internal float m_pageHeight;

	internal bool m_isPrintSelected;

	internal bool IsExtractTextData
	{
		get
		{
			return m_isExtractTextData;
		}
		set
		{
			m_isExtractTextData = value;
		}
	}

	internal Dictionary<string, bool> LayersVisibilityDictionary
	{
		get
		{
			return m_layersVisibilityDictionary;
		}
		set
		{
			m_layersVisibilityDictionary = value;
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
			m_isCurrentPositionChanged = true;
		}
	}

	internal float StrokingOpacity
	{
		get
		{
			return Objects.m_strokingOpacity;
		}
		set
		{
			Objects.m_strokingOpacity = value;
		}
	}

	internal float NonStrokingOpacity
	{
		get
		{
			return Objects.m_nonStrokingOpacity;
		}
		set
		{
			Objects.m_nonStrokingOpacity = value;
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

	private GraphicObjectData Objects => m_objects.Peek();

	private string CurrentFont
	{
		get
		{
			if (Objects.CurrentFont != null)
			{
				return Objects.CurrentFont;
			}
			string result = "";
			foreach (GraphicObjectData @object in m_objects)
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
			foreach (GraphicObjectData @object in m_objects)
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
			foreach (GraphicObjectData @object in m_objects)
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

	private Matrix CTRM
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

	private Matrix TextLineMatrix
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

	private Matrix TextMatrix
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

	private Matrix DocumentMatrix
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

	private Matrix TextMatrixUpdate
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

	private Matrix Drawing2dMatrixCTM
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

	private Matrix TransformMatrixTM
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

	private float CharacterSpacing
	{
		get
		{
			return m_characterSpacing;
		}
		set
		{
			m_characterSpacing = value;
		}
	}

	private float WordSpacing
	{
		get
		{
			return m_wordSpacing;
		}
		set
		{
			m_wordSpacing = value;
		}
	}

	public ImageRenderer(PdfRecordCollection contentElements, PdfPageResources resources, float pageBottom, float left, DeviceCMYK cmyk)
	{
		GraphicObjectData graphicObjectData = new GraphicObjectData();
		int num = 96;
		graphicObjectData.Ctm = Matrix.Identity;
		Matrix matrix = m_graphicsObject.TransformMatrix;
		graphicObjectData.Ctm.Translate((float)(matrix.OffsetX / 1.333), (float)(matrix.OffsetY / 1.333));
		graphicObjectData.drawing2dMatrixCTM = new Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);
		graphicObjectData.drawing2dMatrixCTM.Translate((float)(matrix.OffsetX / 1.333), (float)(matrix.OffsetY / 1.333));
		Matrix matrix2 = matrix;
		graphicObjectData.documentMatrix = new Matrix(1.33333333333333 * (double)(num / 96) * matrix2.M11, 0.0, 0.0, -1.33333333333333 * (double)(num / 96) * matrix2.M22, 0.0, (double)pageBottom * matrix2.M22);
		m_objects.Push(graphicObjectData);
		m_objects.Push(graphicObjectData);
		m_contentElements = contentElements;
		m_resources = resources;
		currentPageHeight = pageBottom;
		decodecmykColor = cmyk;
		imageRenderGlyphList = new List<Glyph>();
	}

	public ImageRenderer(PdfRecordCollection contentElements, PdfPageResources resources, GraphicsObject g, DeviceCMYK cmyk, float pageHeight)
	{
		GraphicObjectData graphicObjectData = new GraphicObjectData();
		int num = 96;
		graphicObjectData.m_mitterLength = 1f;
		graphicObjectData.Ctm = Matrix.Identity;
		graphicObjectData.Ctm.Translate((float)(g.TransformMatrix.OffsetX / 1.333), (float)(g.TransformMatrix.OffsetY / 1.333));
		graphicObjectData.drawing2dMatrixCTM = new Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);
		graphicObjectData.drawing2dMatrixCTM.Translate((float)(g.TransformMatrix.OffsetX / 1.333), (float)(g.TransformMatrix.OffsetY / 1.333));
		Matrix matrix = g.TransformMatrix;
		graphicObjectData.documentMatrix = new Matrix(1.33333333333333 * (double)(num / 96) * matrix.M11, 0.0, 0.0, -1.33333333333333 * (double)(num / 96) * matrix.M22, 0.0, (double)pageHeight * matrix.M22);
		m_objects.Push(graphicObjectData);
		m_objects.Push(graphicObjectData);
		m_contentElements = contentElements;
		m_resources = resources;
		currentPageHeight = pageHeight;
		decodecmykColor = cmyk;
		imageRenderGlyphList = new List<Glyph>();
		m_type3GlyphPath = new Dictionary<string, List<List<int>>>();
	}

	private Matrix SetMatrix(double a, double b, double c, double d, double e, double f)
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
		CTRM = new Matrix(a, b, c, d, e, f) * m_objects.ToArray()[0].Ctm;
		return new Matrix((float)CTRM.M11, (float)CTRM.M12, (float)CTRM.M21, (float)CTRM.M22, (float)CTRM.OffsetX, (float)CTRM.OffsetY);
	}

	private void SetTextMatrix(double a, double b, double c, double d, double e, double f)
	{
		Matrix textLineMatrix = (TextMatrix = new Matrix
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

	private Matrix GetTextRenderingMatrix(bool isPath)
	{
		Matrix matrix = default(Matrix);
		matrix.M11 = FontSize * (HorizontalScaling / 100f);
		matrix.M22 = (isPath ? FontSize : (0f - FontSize));
		matrix.OffsetY = (isPath ? ((float)Rise) : (FontSize + (float)Rise));
		return matrix * TextLineMatrix * CTRM;
	}

	private Matrix GetTextRenderingMatrix()
	{
		Matrix matrix = default(Matrix);
		matrix.M11 = FontSize * (HorizontalScaling / 100f);
		matrix.M22 = 0f - FontSize;
		matrix.OffsetY = FontSize + (float)Rise;
		return matrix * CTRM;
	}

	private void MoveToNextLine(double tx, double ty)
	{
		Matrix matrix = default(Matrix);
		matrix.M11 = 1.0;
		matrix.M12 = 0.0;
		matrix.OffsetX = tx;
		matrix.M21 = 0.0;
		matrix.M22 = 1.0;
		matrix.OffsetY = ty;
		Matrix matrix2 = matrix;
		matrix = (TextMatrix = matrix2 * TextLineMatrix);
		TextLineMatrix = matrix;
	}

	private float FloatParse(string textString)
	{
		return float.Parse(textString, CultureInfo.InvariantCulture.NumberFormat);
	}

	public void RenderAsImage()
	{
		try
		{
			PdfRecordCollection pdfRecordCollection = null;
			pdfRecordCollection = ((!m_isType3Font || isExtractLineCollection || IsExtractTextData) ? m_contentElements : m_type3RecordCollection);
			if (pdfRecordCollection == null)
			{
				return;
			}
			for (int i = 0; i < pdfRecordCollection.RecordCollection.Count; i++)
			{
				int num = 0;
				int num2 = 0;
				int num3 = 0;
				string text = pdfRecordCollection.RecordCollection[i].OperatorName;
				string[] operands = pdfRecordCollection.RecordCollection[i].Operands;
				char[] symbolChars = m_symbolChars;
				for (int j = 0; j < symbolChars.Length; j++)
				{
					char c = symbolChars[j];
					if (text.Contains(c.ToString()))
					{
						text = text.Replace(c.ToString(), "");
					}
				}
				if (text == "scn")
				{
					num = 0;
					string[] array = operands;
					for (int j = 0; j < array.Length; j++)
					{
						_ = array[j];
						num++;
					}
					if (num == 1)
					{
						for (num2 = i; num2 < pdfRecordCollection.RecordCollection.Count; num2++)
						{
							if (!(pdfRecordCollection.RecordCollection[num2].OperatorName == "f"))
							{
								continue;
							}
							for (num3 = i; num3 < num2; num3++)
							{
								if (pdfRecordCollection.RecordCollection[num3].OperatorName == "re")
								{
									_ = pdfRecordCollection.RecordCollection[num3].Operands[2];
									break;
								}
								if (pdfRecordCollection.RecordCollection[num3].OperatorName == "c")
								{
									_ = pdfRecordCollection.RecordCollection[num3].Operands[2];
									break;
								}
							}
							break;
						}
					}
				}
				string text2 = text.Trim();
				if (text2 != null)
				{
					int num4;
					string text3;
					bool flag;
					PdfDictionary xObjectDictionary;
					Matrix textLineMatrix;
					Matrix documentMatrix4;
					Matrix matrix9;
					float num11;
					float num12;
					float num13;
					float num14;
					float num15;
					float num16;
					GraphicsState item3;
					switch (text2.Length)
					{
					case 3:
						switch (text2[0])
						{
						case 'B':
						{
							if (!(text2 == "BDC"))
							{
								break;
							}
							if (operands.Length <= 1)
							{
								continue;
							}
							string key = operands[1].Replace("/", "");
							if (m_layersVisibilityDictionary.ContainsKey(key) && !m_layersVisibilityDictionary[key])
							{
								m_skipRendering = true;
							}
							if (m_extractTags)
							{
								if (m_inlayersCount <= 0)
								{
									m_mcid = -1;
								}
								string value = null;
								char[] separator = new char[5] { ' ', '(', ')', '>', '<' };
								string[] array2 = operands[1].Replace("/", " ").Split(separator);
								for (int k = 0; k < array2.Length; k++)
								{
									string text4 = array2[k];
									if (text4 == "E")
									{
										if (k + 1 < array2.Length && array2[k + 1].Length > 2)
										{
											value = array2[k + 1];
											k++;
										}
									}
									else
									{
										if (!(text4 == "MCID"))
										{
											continue;
										}
										try
										{
											m_mcid = int.Parse(array2[k + 1]);
											if (!string.IsNullOrEmpty(value))
											{
												if (m_abbreviationCollection == null)
												{
													m_abbreviationCollection = new Dictionary<int, string>();
												}
												m_abbreviationCollection[m_mcid] = value;
											}
										}
										catch (Exception)
										{
											m_mcid = -1;
										}
										break;
									}
								}
							}
							if (m_extractTags)
							{
								m_inlayersCount++;
							}
							else if (m_skipRendering)
							{
								m_inlayersCount++;
							}
							continue;
						}
						case 'E':
							if (!(text2 == "EMC"))
							{
								break;
							}
							if (m_extractTags)
							{
								if (m_inlayersCount > 0)
								{
									m_inlayersCount--;
								}
								if (m_inlayersCount <= 0)
								{
									m_mcid = -1;
								}
							}
							else
							{
								if (m_inlayersCount > 0)
								{
									m_inlayersCount--;
								}
								if (m_inlayersCount <= 0)
								{
									m_skipRendering = false;
								}
							}
							continue;
						case 'S':
							if (text2 == "SCN")
							{
								continue;
							}
							break;
						case 's':
							if (text2 == "scn")
							{
								continue;
							}
							break;
						}
						break;
					case 1:
						switch (text2[0])
						{
						case 'q':
						{
							GraphicObjectData graphicObjectData = new GraphicObjectData();
							if (m_objects.Count > 0)
							{
								GraphicObjectData graphicObjectData2 = m_objects.ToArray()[0];
								graphicObjectData.Ctm = graphicObjectData2.Ctm;
								graphicObjectData.m_mitterLength = graphicObjectData2.m_mitterLength;
								graphicObjectData.textLineMatrix = graphicObjectData2.textLineMatrix;
								graphicObjectData.documentMatrix = graphicObjectData2.documentMatrix;
								graphicObjectData.textMatrixUpdate = graphicObjectData2.textMatrixUpdate;
								graphicObjectData.drawing2dMatrixCTM = graphicObjectData2.drawing2dMatrixCTM;
								graphicObjectData.HorizontalScaling = graphicObjectData2.HorizontalScaling;
								graphicObjectData.Rise = graphicObjectData2.Rise;
								graphicObjectData.transformMatrixTM = graphicObjectData2.transformMatrixTM;
								graphicObjectData.CharacterSpacing = graphicObjectData2.CharacterSpacing;
								graphicObjectData.WordSpacing = graphicObjectData2.WordSpacing;
								graphicObjectData.m_nonStrokingOpacity = graphicObjectData2.m_nonStrokingOpacity;
								graphicObjectData.m_strokingOpacity = graphicObjectData2.m_strokingOpacity;
							}
							if (isXGraphics)
							{
								xobjectGraphicsCount++;
							}
							m_objects.Push(graphicObjectData);
							GraphicsState item = m_graphicsObject.Save();
							m_graphicsState.Push(item);
							continue;
						}
						case 'Q':
							if (isXGraphics)
							{
								xobjectGraphicsCount--;
							}
							m_objects.Pop();
							if (m_graphicsState.Count > 0)
							{
								m_graphicsObject.Restore(m_graphicsState.Pop());
							}
							IsGraphicsState = false;
							IsTransparentText = false;
							continue;
						case '\'':
						{
							MoveToNextLineWithCurrentTextLeading();
							Matrix textRenderingMatrix = GetTextRenderingMatrix(isPath: false);
							TextMatrixUpdate = textRenderingMatrix;
							_ = DocumentMatrix;
							if (TextScaling != 100f)
							{
								GraphicsState item2 = m_graphicsObject.Save();
								m_graphicsState.Push(item2);
								m_graphicsObject.ScaleTransform(TextScaling / 100f, 1.0);
								isScaledText = true;
								CurrentLocation = new PointF(CurrentLocation.X / (TextScaling / 100f), CurrentLocation.Y);
							}
							RenderTextElementWithLeading(operands, text);
							continue;
						}
						case 'k':
							m_opacity = StrokingOpacity;
							TextColor = GetColor(operands, "DeviceCMYK");
							continue;
						case 'K':
							m_opacity = NonStrokingOpacity;
							TextColor = GetColor(operands, "DeviceCMYK");
							continue;
						case 'g':
							m_opacity = StrokingOpacity;
							TextColor = GetColor(operands, "Gray");
							continue;
						case 'G':
							m_opacity = NonStrokingOpacity;
							TextColor = GetColor(operands, "Gray");
							continue;
						case 'd':
							if (operands[0] != "[]" && !operands[0].Contains("\n"))
							{
								m_dashedLine = operands;
							}
							continue;
						case 'b':
							break;
						case 'B':
							goto IL_1492;
						case 'n':
							BackupCurrentGeometry = CurrentGeometry;
							CurrentGeometry = new PathGeometry();
							m_currentPath = null;
							continue;
						case 'j':
							m_lineJoin = FloatParse(operands[0]);
							continue;
						case 'J':
							m_lineCap = FloatParse(operands[0]);
							continue;
						case 'w':
							MitterLength = FloatParse(operands[0]);
							continue;
						case 'W':
						{
							if (m_isType3Font)
							{
								continue;
							}
							m_clippingPath = text;
							Matrix documentMatrix3 = DocumentMatrix;
							Matrix matrix8 = new Matrix(Drawing2dMatrixCTM.M11, Drawing2dMatrixCTM.M12, Drawing2dMatrixCTM.M21, Drawing2dMatrixCTM.M22, Drawing2dMatrixCTM.OffsetX, Drawing2dMatrixCTM.OffsetY) * documentMatrix3;
							new Matrix((float)Math.Round(matrix8.M11, 5, MidpointRounding.ToEven), (float)Math.Round(matrix8.M12, 5, MidpointRounding.ToEven), (float)Math.Round(matrix8.M21, 5, MidpointRounding.ToEven), (float)Math.Round(matrix8.M22, 5, MidpointRounding.ToEven), (float)Math.Round(matrix8.OffsetX, 5, MidpointRounding.ToEven), (float)Math.Round(matrix8.OffsetY, 5, MidpointRounding.ToEven));
							_ = m_graphicsObject.TransformMatrix;
							m_graphicsObject.TransformMatrix = new Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);
							foreach (PathFigure figure in CurrentGeometry.Figures)
							{
								figure.IsClosed = true;
								figure.IsFilled = true;
							}
							CurrentGeometry.FillRule = FillRule.Nonzero;
							continue;
						}
						case 'S':
						case 's':
							if (!m_skipRendering && text.Trim() == "s" && m_currentPath != null)
							{
								m_currentPath.IsClosed = true;
							}
							continue;
						case 'f':
							if (!m_skipRendering)
							{
								CurrentLocation = PointF.Empty;
							}
							continue;
						case 'h':
							if (m_currentPath != null)
							{
								m_currentPath.IsClosed = true;
							}
							continue;
						case 'm':
							if (!m_skipRendering)
							{
							}
							continue;
						case 'c':
							if (!m_skipRendering)
							{
							}
							continue;
						case 'l':
							if (!m_skipRendering)
							{
							}
							continue;
						default:
							goto end_IL_01b8;
						case 'v':
						case 'y':
							continue;
						}
						goto IL_1348;
					case 2:
						{
							switch (text2[1])
							{
							case 'r':
								break;
							case 'm':
								goto IL_0442;
							case 'T':
								goto IL_0469;
							case '*':
								goto IL_0490;
							case 'J':
								goto IL_04ea;
							case 'j':
								goto IL_0500;
							case 'f':
								goto IL_0516;
							case 'D':
								goto IL_052c;
							case 'd':
								goto IL_0553;
							case 'L':
								goto IL_0569;
							case 'w':
								goto IL_057f;
							case 'c':
								goto IL_0595;
							case 'z':
								goto IL_05bc;
							case 'G':
								goto IL_05d2;
							case 'C':
								goto IL_05e8;
							case 's':
								goto IL_05fe;
							case 'S':
								goto IL_0625;
							case 'g':
								goto IL_063b;
							case 'o':
								goto IL_0651;
							case 'e':
								goto IL_0667;
							case '0':
								goto IL_067d;
							case '1':
								goto IL_0693;
							case 'I':
								goto IL_06a9;
							case 'h':
								goto IL_06bf;
							default:
								goto end_IL_01b8;
							}
							if (!(text2 == "Tr"))
							{
								break;
							}
							RenderingMode = int.Parse(operands[0]);
							continue;
						}
						IL_06bf:
						if (text2 == "sh")
						{
							continue;
						}
						break;
						IL_06a9:
						if (text2 == "EI")
						{
							continue;
						}
						break;
						IL_0693:
						if (!(text2 == "d1"))
						{
							break;
						}
						m_d1Matrix = new Matrix(FloatParse(operands[0]), FloatParse(operands[1]), FloatParse(operands[2]), FloatParse(operands[3]), FloatParse(operands[4]), FloatParse(operands[5]));
						continue;
						IL_0516:
						if (!(text2 == "Tf"))
						{
							break;
						}
						RenderFont(operands);
						continue;
						IL_067d:
						if (!(text2 == "d0"))
						{
							break;
						}
						m_d0Matrix = new Matrixx(FloatParse(operands[0]), FloatParse(operands[1]));
						continue;
						IL_0500:
						if (!(text2 == "Tj"))
						{
							break;
						}
						if (!m_skipRendering)
						{
							RenderTextElement(operands, text);
						}
						continue;
						IL_0667:
						if (!(text2 == "re"))
						{
							break;
						}
						if (!m_skipRendering)
						{
						}
						continue;
						IL_05e8:
						if (text2 == "SC")
						{
							continue;
						}
						break;
						IL_0651:
						if (!(text2 == "Do"))
						{
							break;
						}
						if (!m_skipRendering && !m_isType3Font)
						{
							GetXObject(operands);
						}
						continue;
						IL_05d2:
						if (!(text2 == "RG"))
						{
							break;
						}
						m_opacity = NonStrokingOpacity;
						TextColor = GetColor(operands, "RGB");
						continue;
						IL_04ea:
						if (!(text2 == "TJ"))
						{
							break;
						}
						if (!m_skipRendering && FontSize != 0f)
						{
							RenderTextElementWithSpacing(operands, text);
						}
						continue;
						IL_1348:
						if (!m_skipRendering)
						{
							if (m_currentPath != null)
							{
								m_currentPath.IsClosed = true;
							}
							Matrix documentMatrix = DocumentMatrix;
							Matrix matrix = new Matrix(Drawing2dMatrixCTM.M11, Drawing2dMatrixCTM.M12, Drawing2dMatrixCTM.M21, Drawing2dMatrixCTM.M22, Drawing2dMatrixCTM.OffsetX, Drawing2dMatrixCTM.OffsetY) * documentMatrix;
							Matrix matrix2 = new Matrix((float)matrix.M11, (float)matrix.M12, (float)matrix.M21, (float)matrix.M22, (float)matrix.OffsetX, (float)matrix.OffsetY);
							Matrix matrix3 = default(Matrix);
							matrix3 *= matrix2;
							_ = m_graphicsObject.TransformMatrix;
							m_graphicsObject.TransformMatrix = new Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);
							m_graphicsObject.TransformMatrix = matrix3;
							m_currentPath = null;
						}
						continue;
						IL_063b:
						if (!(text2 == "rg"))
						{
							break;
						}
						m_opacity = StrokingOpacity;
						TextColor = GetColor(operands, "RGB");
						continue;
						IL_0595:
						if (!(text2 == "Tc"))
						{
							if (text2 == "sc")
							{
								continue;
							}
							break;
						}
						GetCharacterSpacing(operands);
						continue;
						IL_0625:
						if (text2 == "CS")
						{
							continue;
						}
						break;
						IL_05fe:
						if (text2 == "cs")
						{
							continue;
						}
						if (!(text2 == "gs"))
						{
							break;
						}
						if (IsPdfium)
						{
							continue;
						}
						IsTransparentText = false;
						if (!m_resources.ContainsKey(operands[0].Substring(1)))
						{
							continue;
						}
						num4 = 0;
						text3 = null;
						flag = true;
						xObjectDictionary = (m_resources[operands[0].Substring(1)] as XObjectElement).XObjectDictionary;
						if (xObjectDictionary.ContainsKey("OPM"))
						{
							num4 = (xObjectDictionary["OPM"] as PdfNumber).IntValue;
						}
						if (xObjectDictionary.ContainsKey("SMask"))
						{
							m_isExtendedGraphicStateContainsSMask = true;
						}
						if (xObjectDictionary.ContainsKey("AIS"))
						{
							flag = (xObjectDictionary["AIS"] as PdfBoolean).Value;
						}
						if (NonStrokingOpacity != 0f)
						{
							_ = StrokingOpacity;
							_ = 0f;
						}
						if (xObjectDictionary.ContainsKey("HT"))
						{
							PdfName pdfName = xObjectDictionary["HT"] as PdfName;
							if (pdfName != null)
							{
								text3 = pdfName.Value;
							}
							else
							{
								pdfName = (xObjectDictionary["HT"] as PdfReferenceHolder).Object as PdfName;
								if (pdfName != null)
								{
									text3 = pdfName.Value;
								}
							}
						}
						else if (xObjectDictionary.ContainsKey("CA") || xObjectDictionary.ContainsKey("ca"))
						{
							if (xObjectDictionary.ContainsKey("CA"))
							{
								NonStrokingOpacity = (xObjectDictionary["CA"] as PdfNumber).FloatValue;
							}
							if (xObjectDictionary.ContainsKey("ca"))
							{
								if (flag)
								{
									StrokingOpacity = (xObjectDictionary["ca"] as PdfNumber).FloatValue;
								}
								else if (!isXGraphics)
								{
									StrokingOpacity = (xObjectDictionary["ca"] as PdfNumber).FloatValue;
								}
							}
							if (NonStrokingOpacity != 0f)
							{
								_ = StrokingOpacity;
								_ = 0f;
							}
							m_opacity = StrokingOpacity;
							TextColor = GetColor(m_backupColorElements, m_backupColorSpace);
							IsGraphicsState = true;
						}
						else if (xObjectDictionary.ContainsKey("TR"))
						{
							if (xObjectDictionary["TR"].ToString().Replace("/", "") == "Identity" && num4 == 1)
							{
								m_isTrEntry = true;
							}
						}
						else if (!xObjectDictionary.ContainsKey("TR") && num4 == 1 && xObjectDictionary.ContainsKey("Type") && xObjectDictionary["Type"].ToString().Replace("/", "") == "ExtGState" && num4 == 1)
						{
							m_isTrEntry = true;
						}
						if (num4 == 1 && text3 == "Default")
						{
							IsExtendedGraphicsState = true;
						}
						continue;
						IL_05bc:
						if (!(text2 == "Tz"))
						{
							break;
						}
						GetScalingFactor(operands);
						continue;
						IL_1492:
						if (!m_skipRendering)
						{
							Matrix documentMatrix2 = DocumentMatrix;
							Matrix matrix4 = new Matrix(Drawing2dMatrixCTM.M11, Drawing2dMatrixCTM.M12, Drawing2dMatrixCTM.M21, Drawing2dMatrixCTM.M22, Drawing2dMatrixCTM.OffsetX, Drawing2dMatrixCTM.OffsetY) * documentMatrix2;
							Matrix matrix5 = new Matrix((float)matrix4.M11, (float)matrix4.M12, (float)matrix4.M21, (float)matrix4.M22, (float)matrix4.OffsetX, (float)matrix4.OffsetY);
							Matrix matrix6 = default(Matrix);
							matrix6 *= matrix5;
							Matrix matrix7 = m_graphicsObject.TransformMatrix;
							m_graphicsObject.TransformMatrix = new Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);
							m_graphicsObject.TransformMatrix = matrix6;
							m_graphicsObject.TransformMatrix = matrix7;
							m_currentPath = null;
						}
						continue;
						IL_057f:
						if (!(text2 == "Tw"))
						{
							break;
						}
						GetWordSpacing(operands);
						continue;
						IL_0569:
						if (!(text2 == "TL"))
						{
							break;
						}
						SetTextLeading(FloatParse(operands[0]));
						continue;
						IL_0490:
						switch (text2)
						{
						case "T*":
							break;
						case "b*":
							goto IL_1348;
						case "B*":
							goto IL_1492;
						case "W*":
							goto IL_17c5;
						case "f*":
							goto IL_197b;
						default:
							goto end_IL_01b8;
						}
						if (!m_skipRendering)
						{
							MoveToNextLineWithCurrentTextLeading();
							DrawNewLine();
						}
						continue;
						IL_0553:
						if (!(text2 == "Td"))
						{
							break;
						}
						CurrentLocation = new PointF(CurrentLocation.X + FloatParse(operands[0]), CurrentLocation.Y - FloatParse(operands[1]));
						MoveToNextLine(FloatParse(operands[0]), FloatParse(operands[1]));
						continue;
						IL_0469:
						if (!(text2 == "BT"))
						{
							if (!(text2 == "ET"))
							{
								break;
							}
							CurrentLocation = PointF.Empty;
							if (isScaledText)
							{
								isScaledText = false;
								m_graphicsObject.Restore(m_graphicsState.Pop());
							}
							if (textMatrix)
							{
								m_graphicsObject.Restore(m_graphicsState.Pop());
								textMatrix = false;
							}
							if (RenderingMode != 3 && RenderingMode == 2 && pdfRecordCollection.RecordCollection.Count > i + 1 && pdfRecordCollection.RecordCollection[i + 1].OperatorName != "q")
							{
								RenderingMode = 0;
							}
							m_lineJoin = float.MinValue;
							m_lineCap = float.MinValue;
							continue;
						}
						textLineMatrix = (TextMatrix = Matrix.Identity);
						TextLineMatrix = textLineMatrix;
						m_beginText = true;
						CurrentLocation = PointF.Empty;
						continue;
						IL_052c:
						if (!(text2 == "TD"))
						{
							if (text2 == "ID")
							{
								continue;
							}
							break;
						}
						CurrentLocation = new PointF(CurrentLocation.X + FloatParse(operands[0]), CurrentLocation.Y - FloatParse(operands[1]));
						MoveToNextLineWithLeading(operands);
						continue;
						IL_197b:
						if (!m_skipRendering)
						{
							CurrentLocation = PointF.Empty;
						}
						continue;
						IL_17c5:
						m_clippingPath = text;
						documentMatrix4 = DocumentMatrix;
						matrix9 = new Matrix(Drawing2dMatrixCTM.M11, Drawing2dMatrixCTM.M12, Drawing2dMatrixCTM.M21, Drawing2dMatrixCTM.M22, Drawing2dMatrixCTM.OffsetX, Drawing2dMatrixCTM.OffsetY) * documentMatrix4;
						new Matrix((float)Math.Round(matrix9.M11, 5, MidpointRounding.ToEven), (float)Math.Round(matrix9.M12, 5, MidpointRounding.ToEven), (float)Math.Round(matrix9.M21, 5, MidpointRounding.ToEven), (float)Math.Round(matrix9.M22, 5, MidpointRounding.ToEven), (float)Math.Round(matrix9.OffsetX, 5, MidpointRounding.ToEven), (float)Math.Round(matrix9.OffsetY, 5, MidpointRounding.ToEven));
						_ = m_graphicsObject.TransformMatrix;
						m_graphicsObject.TransformMatrix = new Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0);
						foreach (PathFigure figure2 in CurrentGeometry.Figures)
						{
							figure2.IsClosed = true;
							figure2.IsFilled = true;
						}
						CurrentGeometry.FillRule = FillRule.EvenOdd;
						continue;
						IL_0442:
						if (!(text2 == "Tm"))
						{
							if (!(text2 == "cm"))
							{
								break;
							}
							float num5 = FloatParse(operands[0]);
							float num6 = FloatParse(operands[1]);
							float num7 = FloatParse(operands[2]);
							float num8 = FloatParse(operands[3]);
							float num9 = FloatParse(operands[4]);
							float num10 = FloatParse(operands[5]);
							Drawing2dMatrixCTM = SetMatrix(num5, num6, num7, num8, num9, num10);
							m_imageCommonMatrix = new Matrix(num5, num6, num7, num8, num9, num10);
							continue;
						}
						num11 = FloatParse(operands[0]);
						num12 = FloatParse(operands[1]);
						num13 = FloatParse(operands[2]);
						num14 = FloatParse(operands[3]);
						num15 = FloatParse(operands[4]);
						num16 = FloatParse(operands[5]);
						SetTextMatrix(num11, num12, num13, num14, num15, num16);
						if (textMatrix)
						{
							m_graphicsObject.Restore(m_graphicsState.Pop());
						}
						item3 = m_graphicsObject.Save();
						m_graphicsState.Push(item3);
						m_graphicsObject.MultiplyTransform(new Matrix(num11, 0f - num12, 0f - num13, num14, num15, 0f - num16));
						CurrentLocation = new Point(0.0, 0.0);
						textMatrix = true;
						continue;
						end_IL_01b8:
						break;
					}
				}
				_ = IsPdfium;
			}
			m_isType3Font = false;
		}
		catch (Exception ex2)
		{
			if (ex2.Message.Contains(" does not supported") || ex2.Message.Contains("Error in identifying the ImageFilter"))
			{
				exception.Exceptions.Append("\r\n\r\n" + ex2.Message + "\r\n");
				return;
			}
			if (continueOnError)
			{
				exception.Exceptions.Append("\r\n\r\n" + ex2.Message + ex2.StackTrace + "\r\n");
				return;
			}
			throw ex2;
		}
	}

	private void GetInlineImageParameters(List<string> element)
	{
		int i = 0;
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		for (; i < element.Count; i += 2)
		{
			dictionary.Add(element[i].Substring(1, element[i].Length - 1), element[i + 1]);
		}
		m_inlineParameters = new PdfDictionary();
		foreach (KeyValuePair<string, string> item in dictionary)
		{
			switch (item.Key)
			{
			case "Height":
			case "H":
				m_inlineParameters.SetNumber("Height", FloatParse(item.Value));
				break;
			case "W":
			case "Width":
				m_inlineParameters.SetNumber("Width", FloatParse(item.Value));
				break;
			case "BitsPerComponent":
			case "BPC":
				m_inlineParameters.SetNumber("BitsPerComponent", FloatParse(item.Value));
				break;
			case "IM":
			case "ImageMask":
				if (item.Value == "true")
				{
					m_inlineParameters.SetBoolean("ImageMask", value: true);
				}
				else
				{
					m_inlineParameters.SetBoolean("ImageMask", value: false);
				}
				break;
			case "CS":
			case "ColorSpace":
				if (item.Value.Substring(1) == "RGB" || item.Value.Substring(1) == "DeviceRGB")
				{
					m_inlineParameters.SetName("ColorSpace", "DeviceRGB");
				}
				else if (item.Value.Substring(1) == "G")
				{
					m_inlineParameters.SetName("ColorSpace", "DeviceGray");
				}
				else if (item.Value.Substring(1) == "CMYK")
				{
					m_inlineParameters.SetName("ColorSpace", "DeviceCMYK");
				}
				else if (m_resources.ContainsKey(item.Value.Replace("/", "")))
				{
					PdfArray primitive = (m_resources[item.Value.Replace("/", "")] as ExtendColorspace).ColorSpaceValueArray as PdfArray;
					m_inlineParameters.SetProperty("ColorSpace", primitive);
				}
				break;
			case "Decode":
			case "D":
			{
				string value = item.Value;
				value = value.Replace("[", string.Empty);
				value = value.Replace("]", string.Empty);
				char[] separator = new char[2] { ' ', '\n' };
				string[] array2 = value.Split(separator, StringSplitOptions.RemoveEmptyEntries);
				float[] array3 = new float[array2.Length];
				for (int l = 0; l < array2.Length; l++)
				{
					array3[l] = FloatParse(array2[l]);
				}
				m_inlineParameters.SetProperty("Decode", new PdfArray(array3));
				break;
			}
			case "DP":
			{
				string blackis = "";
				GetDecodeParams(item.Value, out int predictor, out int columns, out int colors, out int k, out blackis);
				PdfDictionary pdfDictionary = new PdfDictionary();
				pdfDictionary.SetNumber("Predictor", predictor);
				pdfDictionary.SetNumber("Columns", columns);
				pdfDictionary.SetNumber("Colors", colors);
				pdfDictionary.SetNumber("K", k);
				if (item.Value.Contains("BlackIs1"))
				{
					if (blackis == "true")
					{
						pdfDictionary.SetBoolean("BlackIs1", value: true);
					}
					else
					{
						pdfDictionary.SetBoolean("BlackIs1", value: false);
					}
				}
				m_inlineParameters.SetProperty("DecodeParms", pdfDictionary);
				break;
			}
			case "Filter":
			case "F":
			{
				PdfArray pdfArray = new PdfArray();
				string[] array = new string[2];
				if (item.Value.Contains(" "))
				{
					array = item.Value.Split(' ');
					for (int j = 0; j < array.Length; j++)
					{
						if (j == 0)
						{
							array[j] = array[j].Substring(2);
						}
						else if (j == array.Length - 1)
						{
							array[^1] = array[^1].Substring(1, array[^1].Length - 2);
						}
						else
						{
							array[j] = array[j].Substring(1);
						}
						if (array[j] == "Fl")
						{
							array[j] = "FlateDecode";
						}
						PdfName element2 = new PdfName(array[j]);
						pdfArray.Add(element2);
					}
				}
				else
				{
					array[0] = item.Value.Substring(1);
					array[0] = RemoveUnwantedChar(array[0]);
					if (array[0] == "Fl")
					{
						array[0] = "FlateDecode";
					}
					else if (array[0] == "CCF")
					{
						array[0] = "CCITTFaxDecode";
					}
					else if (array[0] == "AHx")
					{
						array[0] = "ASCIIHex";
					}
					else if (array[0] == "A85")
					{
						array[0] = "ASCII85Decode";
					}
					else if (array[0] == "LZW")
					{
						array[0] = "LZWDecode";
					}
					else if (array[0] == "RL")
					{
						array[0] = "RunLengthDecode";
					}
					else if (array[0] == "DCT")
					{
						array[0] = "DCTDecode";
					}
					PdfName element3 = new PdfName(array[0]);
					pdfArray.Add(element3);
				}
				m_inlineParameters.SetProperty("Filter", pdfArray);
				break;
			}
			}
		}
	}

	private string RemoveUnwantedChar(string originalString)
	{
		char[] array = new char[2] { '/', ']' };
		for (int i = 0; i < array.Length; i++)
		{
			if (originalString.Contains(array[i].ToString()))
			{
				originalString = originalString.Replace(array[i].ToString(), "");
			}
		}
		return originalString;
	}

	private void GetDecodeParams(string decodeParam, out int predictor, out int columns, out int colors, out int k, out string blackis1)
	{
		predictor = 0;
		colors = 1;
		columns = 1;
		k = 0;
		blackis1 = "";
		decodeParam = decodeParam.Remove(0, 2);
		decodeParam = decodeParam.Remove(decodeParam.Length - 2, 2);
		char[] separator = new char[2] { ' ', '\n' };
		string[] array = decodeParam.Split(separator);
		int num = 0;
		while (num < array.Length)
		{
			switch (array[num])
			{
			case "/Predictor":
				int.TryParse(array[++num], out predictor);
				break;
			case "/K":
				int.TryParse(array[++num], out k);
				break;
			case "/Columns":
				int.TryParse(array[++num], out columns);
				break;
			case "/Colors":
				int.TryParse(array[++num], out colors);
				break;
			case "/BlackIs1":
				blackis1 = array[++num];
				break;
			default:
				num++;
				break;
			}
		}
	}

	private void MoveToNextLineWithLeading(string[] element)
	{
		SetTextLeading(0f - FloatParse(element[1]));
		MoveToNextLine(FloatParse(element[0]), FloatParse(element[1]));
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
		FontSize = FloatParse(fontElements[i + 1]);
	}

	private void RenderTextElement(string[] textElements, string tokenType)
	{
		RenderedString renderedString = new RenderedString();
		renderedString.Text = string.Join("", textElements);
		if (!m_resources.ContainsKey(CurrentFont) || !(m_resources[CurrentFont] is FontStructure fontStructure))
		{
			return;
		}
		fontStructure.IsSameFont = m_resources.isSameFont();
		if (fontStructure.FontSize != FontSize)
		{
			fontStructure.FontSize = FontSize;
		}
		byte[] bytes = Encoding.Unicode.GetBytes(fontStructure.ToGetEncodedText(renderedString.Text, m_resources.isSameFont()));
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		int num = 0;
		for (int i = 0; i < bytes.Length; i += 2)
		{
			dictionary.Add(num, bytes[i]);
			num++;
		}
		renderedString = fontStructure.Decode(renderedString.Text, m_resources.isSameFont());
		TextElement textElement = new TextElement(renderedString.Text, DocumentMatrix);
		textElement.EncodedTextBytes = dictionary;
		textElement.FontName = fontStructure.FontName;
		textElement.FontStyle = fontStructure.FontStyle;
		textElement.TextColor = TextColor;
		textElement.FontSize = FontSize;
		textElement.TextScaling = m_textScaling;
		textElement.FontEncoding = fontStructure.FontEncoding;
		textElement.FontGlyphWidths = fontStructure.FontGlyphWidths;
		textElement.DefaultGlyphWidth = fontStructure.DefaultGlyphWidth;
		textElement.isNegativeFont = isNegativeFont;
		textElement.UnicodeCharMapTable = fontStructure.UnicodeCharMapTable;
		textElement.IsFindText = isFindText;
		textElement.IsPdfium = IsPdfium;
		Dictionary<int, int> fontGlyphWidths = fontStructure.FontGlyphWidths;
		textElement.IsType1Font = fontStructure.IsType1Font;
		textElement.Is1C = fontStructure.Is1C;
		textElement.IsCID = fontStructure.IsCID;
		textElement.CharacterMapTable = fontStructure.CharacterMapTable;
		textElement.CidToGidReverseMapTable = fontStructure.CidToGidReverseMapTable;
		textElement.ReverseMapTable = fontStructure.ReverseMapTable;
		textElement.Fontfile2Glyph = fontStructure.GlyphFontFile2;
		textElement.structure = fontStructure;
		textElement.Isembeddedfont = fontStructure.IsEmbedded;
		textElement.Ctm = CTRM;
		textElement.textLineMatrix = TextMatrix;
		textElement.IsExtractTextData = m_isExtractTextData;
		textElement.Rise = Rise;
		textElement.transformMatrix = DocumentMatrix;
		textElement.documentMatrix = DocumentMatrix;
		textElement.FontID = CurrentFont;
		textElement.OctDecMapTable = fontStructure.OctDecMapTable;
		textElement.TextHorizontalScaling = HorizontalScaling;
		textElement.ZapfPostScript = fontStructure.ZapfPostScript;
		textElement.LineWidth = MitterLength;
		textElement.RenderingMode = RenderingMode;
		textElement.testdict = testdict;
		textElement.pageRotation = pageRotation;
		textElement.zoomFactor = zoomFactor;
		textElement.SubstitutedFontsList = substitutedFontsList;
		textElement.m_mcid = m_mcid;
		textElement.renderedString = renderedString;
		if (fontStructure.Flags != null)
		{
			textElement.FontFlag = fontStructure.Flags.IntValue;
		}
		if (fontStructure.IsType1Font)
		{
			textElement.IsType1Font = true;
			textElement.differenceTable = fontStructure.differenceTable;
			textElement.differenceMappedTable = fontStructure.DifferencesDictionary;
			textElement.OctDecMapTable = fontStructure.OctDecMapTable;
			if (!m_glyphDataCollection.Contains(textElement.m_cffGlyphs))
			{
				m_glyphDataCollection.Add(textElement.m_cffGlyphs);
			}
		}
		textElement.WordSpacing = Objects.WordSpacing;
		textElement.CharacterSpacing = Objects.CharacterSpacing;
		if (m_beginText)
		{
			m_beginText = false;
		}
		Matrix txtMatrix = default(Matrix);
		if (fontStructure.fontType.Value != "Type3" || isExtractLineCollection || IsExtractTextData)
		{
			if (m_isCurrentPositionChanged)
			{
				m_isCurrentPositionChanged = false;
				m_endTextPosition = CurrentLocation;
				m_textElementWidth = textElement.Render(m_graphicsObject, new PointF(m_endTextPosition.X, m_endTextPosition.Y - FontSize), m_textScaling, fontGlyphWidths, fontStructure.Type1GlyphHeight, fontStructure.differenceTable, fontStructure.DifferencesDictionary, fontStructure.differenceEncoding, out txtMatrix);
				extractTextElement.Add(textElement);
				if (textElement.Fontfile2Glyph != null && textElement.Fontfile2Glyph.IsFontFile2)
				{
					textElement.textElementGlyphList = ReplaceLigatureFromGlyphList(textElement.textElementGlyphList);
				}
				imageRenderGlyphList.AddRange(textElement.textElementGlyphList);
			}
			else
			{
				m_endTextPosition = new PointF(m_endTextPosition.X + m_textElementWidth, m_endTextPosition.Y);
				m_textElementWidth = textElement.Render(m_graphicsObject, new PointF(m_endTextPosition.X, m_endTextPosition.Y - FontSize), m_textScaling, fontGlyphWidths, fontStructure.Type1GlyphHeight, fontStructure.differenceTable, fontStructure.DifferencesDictionary, fontStructure.differenceEncoding, out txtMatrix);
				extractTextElement.Add(textElement);
				if (textElement.Fontfile2Glyph != null && textElement.Fontfile2Glyph.IsFontFile2)
				{
					textElement.textElementGlyphList = ReplaceLigatureFromGlyphList(textElement.textElementGlyphList);
				}
				imageRenderGlyphList.AddRange(textElement.textElementGlyphList);
			}
		}
		else
		{
			Matrix textLineMatrix = TextLineMatrix;
			m_graphicsObject.TransformMatrix = new Matrix((float)DocumentMatrix.M11, (float)DocumentMatrix.M12, (float)DocumentMatrix.M21, (float)DocumentMatrix.M22, (float)DocumentMatrix.OffsetX, (float)DocumentMatrix.OffsetY);
			m_graphicsObject.TranslateTransform(0.0, (float)TextLineMatrix.OffsetY);
			m_spacingWidth = 0f;
			RenderType3GlyphImages(fontStructure, renderedString.Text);
			TextLineMatrix = textLineMatrix;
			FontStructure fontStructure2 = fontStructure;
			txtMatrix = Matrix.Identity;
		}
		TextMatrix = txtMatrix;
	}

	private void UpdateTextMatrix(double tj)
	{
		double num = 0.0 - tj * 0.001 * (double)FontSize * (double)HorizontalScaling / 100.0;
		Point point = Objects.textLineMatrix.Transform(new Point(0.0, 0.0));
		Point point2 = Objects.textLineMatrix.Transform(new Point(num, 0.0));
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

	private void RenderType3GlyphImagesTJ(FontStructure structure, List<RenderedString> decodedCollection)
	{
		for (int i = 0; i < decodedCollection.Count; i++)
		{
			string text = decodedCollection[i].Text;
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
					byte[] decompressedData = pdfStream.GetDecompressedData();
					memoryStream.Read(decompressedData, 0, decompressedData.Length);
					memoryStream.Write(array, 0, array.Length);
					pdfStream.InternalStream.WriteTo(memoryStream);
					memoryStream.Write(array, 0, array.Length);
					bool flag = false;
					Matrix cTRM = CTRM;
					Matrix drawing2dMatrixCTM = Drawing2dMatrixCTM;
					Matrix matrix = TextLineMatrix * CTRM;
					Stack<GraphicObjectData> objects = m_objects;
					if (structure.FontDictionary.ContainsKey("FontMatrix"))
					{
						PdfArray pdfArray = structure.FontDictionary["FontMatrix"] as PdfArray;
						XFormsMatrix = new Matrix((pdfArray[0] as PdfNumber).FloatValue, (pdfArray[1] as PdfNumber).FloatValue, (pdfArray[2] as PdfNumber).FloatValue, (pdfArray[3] as PdfNumber).FloatValue, (pdfArray[4] as PdfNumber).FloatValue, (pdfArray[5] as PdfNumber).FloatValue);
						matrix = XFormsMatrix * matrix;
						m_type3FontScallingFactor = (float)matrix.M11;
					}
					if (memoryStream != null)
					{
						ContentParser contentParser = new ContentParser(decompressedData);
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
						PdfPageResources resources = m_resources;
						m_resources = structure.Type3FontGlyphImages;
						m_transformations = new TransformationStack(DocumentMatrix);
						m_type3FontStruct = structure;
						m_isType3Font = true;
						if (flag)
						{
							CTRM = new Matrix((float)matrix.M11, (float)matrix.M12, (float)matrix.M21, (float)matrix.M22, (float)matrix.OffsetX, (float)matrix.OffsetY);
							Drawing2dMatrixCTM = new Matrix((float)matrix.M11, (float)matrix.M12, (float)matrix.M21, (float)matrix.M22, (float)matrix.OffsetX, (float)matrix.OffsetY);
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

	private void RenderType3GlyphImages(FontStructure structure, string renderingText)
	{
		for (int i = 0; i < renderingText.Length; i++)
		{
			string text = structure.DecodeType3FontData(renderingText[i].ToString());
			if (structure.Type3FontCharProcsDict != null && !structure.Type3FontCharProcsDict.ContainsKey(text))
			{
				text = FontStructure.GetCharCode(text);
			}
			string text2 = string.Empty;
			string text3 = string.Empty;
			if (renderingText.Length == 1)
			{
				Dictionary<string, string> differencesDictionary = structure.DifferencesDictionary;
				int num = renderingText.ToCharArray()[0];
				text2 = differencesDictionary[num.ToString()];
				if (text2.Length == 7 && text2.ToLowerInvariant().StartsWith("uni"))
				{
					text3 = text2;
					text2 = structure.DecodeToUnicode(text2);
				}
				else
				{
					text2 = string.Empty;
				}
			}
			if (structure.Type3FontCharProcsDict == null || (!structure.Type3FontCharProcsDict.ContainsKey(text) && !(text2 == text)))
			{
				continue;
			}
			m_type3GlyphID = text + structure.FontRefNumber;
			MemoryStream memoryStream = new MemoryStream();
			PdfStream pdfStream = new PdfStream();
			pdfStream = ((!(text2 == text) || !(text3 != string.Empty)) ? structure.Type3FontCharProcsDict[text] : structure.Type3FontCharProcsDict[text3]);
			byte[] array = PdfString.StringToByte("\r\n");
			byte[] decompressedData = pdfStream.GetDecompressedData();
			memoryStream.Write(decompressedData, 0, decompressedData.Length);
			memoryStream.Write(array, 0, array.Length);
			bool flag = false;
			Matrix cTRM = CTRM;
			Matrix drawing2dMatrixCTM = Drawing2dMatrixCTM;
			Matrix matrix = TextLineMatrix * CTRM;
			Stack<GraphicObjectData> objects = m_objects;
			if (structure.FontDictionary.ContainsKey("FontMatrix"))
			{
				PdfArray pdfArray = structure.FontDictionary["FontMatrix"] as PdfArray;
				XFormsMatrix = new Matrix((pdfArray[0] as PdfNumber).FloatValue, (pdfArray[1] as PdfNumber).FloatValue, (pdfArray[2] as PdfNumber).FloatValue, (pdfArray[3] as PdfNumber).FloatValue, (pdfArray[4] as PdfNumber).FloatValue, (pdfArray[5] as PdfNumber).FloatValue);
				matrix = XFormsMatrix * matrix;
				m_type3FontScallingFactor = (float)matrix.M11;
			}
			if (memoryStream != null)
			{
				ContentParser contentParser = new ContentParser(memoryStream.ToArray());
				m_type3RecordCollection = contentParser.ReadContent();
				for (int j = 0; j < m_type3RecordCollection.RecordCollection.Count; j++)
				{
					string text4 = m_type3RecordCollection.RecordCollection[j].OperatorName;
					if (text4 == "cm")
					{
						m_istype3FontContainCTRM = true;
					}
					char[] symbolChars = m_symbolChars;
					for (int num = 0; num < symbolChars.Length; num++)
					{
						char c = symbolChars[num];
						if (text4.Contains(c.ToString()))
						{
							text4 = text4.Replace(c.ToString(), "");
						}
					}
					if (text4.Trim() == "BI" || text4.Trim() == "Do")
					{
						flag = true;
					}
				}
				PdfPageResources resources = m_resources;
				m_resources = structure.Type3FontGlyphImages;
				m_transformations = new TransformationStack(DocumentMatrix);
				m_type3FontStruct = structure;
				m_isType3Font = true;
				if (flag)
				{
					CTRM = new Matrix((float)matrix.M11, (float)matrix.M12, (float)matrix.M21, (float)matrix.M22, (float)matrix.OffsetX, (float)matrix.OffsetY);
					Drawing2dMatrixCTM = new Matrix((float)matrix.M11, (float)matrix.M12, (float)matrix.M21, (float)matrix.M22, (float)matrix.OffsetX, (float)matrix.OffsetY);
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
				float num2 = (float)m_d0Matrix.M11;
				TextLineMatrix = CalculateType3TextMatrixupdate(TextLineMatrix, num2 * m_type3FontScallingFactor, isImage: true);
			}
			else if (renderingText[i].ToString() == " " && !flag)
			{
				float num3 = (float)m_d1Matrix.M11;
				TextLineMatrix = CalculateType3TextMatrixupdate(TextLineMatrix, num3 * m_type3FontScallingFactor, isImage: true);
			}
			CTRM = cTRM;
			Drawing2dMatrixCTM = drawing2dMatrixCTM;
			m_objects = objects;
		}
	}

	private void RenderTextElementWithLeading(string[] textElements, string tokenType)
	{
		RenderedString renderedString = new RenderedString();
		renderedString.Text = string.Join("", textElements);
		if (!m_resources.ContainsKey(CurrentFont))
		{
			return;
		}
		(m_resources[CurrentFont] as FontStructure).IsSameFont = m_resources.isSameFont();
		if ((m_resources[CurrentFont] as FontStructure).FontSize != FontSize)
		{
			(m_resources[CurrentFont] as FontStructure).FontSize = FontSize;
		}
		FontStructure fontStructure = m_resources[CurrentFont] as FontStructure;
		renderedString = fontStructure.Decode(renderedString.Text, m_resources.isSameFont());
		TextElement textElement = new TextElement(renderedString.Text, DocumentMatrix);
		textElement.FontName = fontStructure.FontName;
		textElement.FontStyle = fontStructure.FontStyle;
		textElement.TextColor = TextColor;
		textElement.FontSize = FontSize;
		textElement.TextScaling = m_textScaling;
		textElement.FontEncoding = fontStructure.FontEncoding;
		textElement.FontGlyphWidths = fontStructure.FontGlyphWidths;
		textElement.DefaultGlyphWidth = fontStructure.DefaultGlyphWidth;
		textElement.Text = renderedString.Text;
		textElement.IsFindText = isFindText;
		textElement.IsPdfium = IsPdfium;
		textElement.IsTransparentText = IsTransparentText;
		textElement.UnicodeCharMapTable = fontStructure.UnicodeCharMapTable;
		Dictionary<int, int> fontGlyphWidths = fontStructure.FontGlyphWidths;
		textElement.CidToGidReverseMapTable = fontStructure.CidToGidReverseMapTable;
		textElement.IsType1Font = fontStructure.IsType1Font;
		textElement.Is1C = fontStructure.Is1C;
		textElement.IsCID = fontStructure.IsCID;
		textElement.CharacterMapTable = fontStructure.CharacterMapTable;
		textElement.ReverseMapTable = fontStructure.ReverseMapTable;
		textElement.structure = fontStructure;
		textElement.Isembeddedfont = fontStructure.IsEmbedded;
		textElement.Ctm = CTRM;
		textElement.textLineMatrix = TextMatrix;
		textElement.Rise = Rise;
		textElement.transformMatrix = DocumentMatrix;
		textElement.documentMatrix = DocumentMatrix;
		textElement.FontID = CurrentFont;
		textElement.OctDecMapTable = fontStructure.OctDecMapTable;
		textElement.TextHorizontalScaling = HorizontalScaling;
		textElement.ZapfPostScript = fontStructure.ZapfPostScript;
		textElement.LineWidth = MitterLength;
		textElement.RenderingMode = RenderingMode;
		textElement.testdict = testdict;
		textElement.pageRotation = pageRotation;
		textElement.zoomFactor = zoomFactor;
		textElement.SubstitutedFontsList = substitutedFontsList;
		textElement.m_mcid = m_mcid;
		if (fontStructure.Flags != null)
		{
			textElement.FontFlag = fontStructure.Flags.IntValue;
		}
		if (fontStructure.IsType1Font)
		{
			textElement.IsType1Font = true;
			textElement.differenceTable = fontStructure.differenceTable;
			textElement.differenceMappedTable = fontStructure.DifferencesDictionary;
			if (!m_glyphDataCollection.Contains(textElement.m_cffGlyphs))
			{
				m_glyphDataCollection.Add(textElement.m_cffGlyphs);
			}
		}
		textElement.WordSpacing = Objects.WordSpacing;
		textElement.CharacterSpacing = Objects.CharacterSpacing;
		if (TextScaling != 100f)
		{
			textElement.Textscalingfactor = TextScaling / 100f;
		}
		if (m_beginText)
		{
			m_beginText = false;
		}
		if (!string.IsNullOrEmpty(m_currentFontName) && !isExtractLineCollection && !IsExtractTextData && textElement.FontName == "Cambria")
		{
			textElement.FontName = m_currentFontName;
		}
		else
		{
			m_currentFontName = textElement.FontName;
		}
		Matrix txtMatrix = default(Matrix);
		if (fontStructure.fontType.Value != "Type3" || isExtractLineCollection || IsExtractTextData)
		{
			if (m_isCurrentPositionChanged)
			{
				m_isCurrentPositionChanged = false;
				m_endTextPosition = CurrentLocation;
				m_textElementWidth = textElement.Render(m_graphicsObject, new PointF(m_endTextPosition.X, m_endTextPosition.Y + (0f - TextLeading) / 4f), m_textScaling, fontGlyphWidths, fontStructure.Type1GlyphHeight, fontStructure.differenceTable, fontStructure.DifferencesDictionary, fontStructure.differenceEncoding, out txtMatrix);
				extractTextElement.Add(textElement);
				if (textElement.Fontfile2Glyph != null && textElement.Fontfile2Glyph.IsFontFile2)
				{
					textElement.textElementGlyphList = ReplaceLigatureFromGlyphList(textElement.textElementGlyphList);
				}
				imageRenderGlyphList.AddRange(textElement.textElementGlyphList);
			}
			else
			{
				m_endTextPosition = new PointF(m_endTextPosition.X + m_textElementWidth, m_endTextPosition.Y);
				m_textElementWidth = textElement.Render(m_graphicsObject, new PointF(m_endTextPosition.X, m_endTextPosition.Y + (0f - TextLeading) / 4f), m_textScaling, fontGlyphWidths, fontStructure.Type1GlyphHeight, fontStructure.differenceTable, fontStructure.DifferencesDictionary, fontStructure.differenceEncoding, out txtMatrix);
				extractTextElement.Add(textElement);
				if (textElement.Fontfile2Glyph != null && textElement.Fontfile2Glyph.IsFontFile2)
				{
					textElement.textElementGlyphList = ReplaceLigatureFromGlyphList(textElement.textElementGlyphList);
				}
				imageRenderGlyphList.AddRange(textElement.textElementGlyphList);
			}
		}
		else
		{
			Matrix textLineMatrix = TextLineMatrix;
			FontStructure fontStructure2 = fontStructure;
			m_graphicsObject.TransformMatrix = new Matrix((float)DocumentMatrix.M11, (float)DocumentMatrix.M12, (float)DocumentMatrix.M21, (float)DocumentMatrix.M22, (float)DocumentMatrix.OffsetX, (float)DocumentMatrix.OffsetY);
			m_graphicsObject.TranslateTransform(0.0, (float)TextLineMatrix.OffsetY);
			m_spacingWidth = 0f;
			RenderType3GlyphImages(fontStructure, renderedString.Text);
			TextLineMatrix = textLineMatrix;
			fontStructure = fontStructure2;
		}
		TextMatrix = txtMatrix;
	}

	private List<Glyph> ReplaceLigatureFromGlyphList(List<Glyph> textElementList)
	{
		for (int i = 0; i < textElementList.Count; i++)
		{
			string text = textElementList[i].ToString();
			if (text == null || (!(text == "ﬀ") && text.Length != 2))
			{
				continue;
			}
			if (text == "ﬀ")
			{
				text = "ff";
			}
			if (text != null)
			{
				Rect boundingRect = textElementList[i].BoundingRect;
				char[] array = text.ToCharArray();
				Glyph glyph = textElementList[i];
				glyph.ToUnicode = array[0].ToString();
				glyph.BoundingRect = new Rect(boundingRect.X, boundingRect.Y, boundingRect.Width / 2.0, boundingRect.Height);
				Glyph glyph2 = new Glyph();
				glyph2.FontStyle = textElementList[i].FontStyle;
				glyph2.FontFamily = textElementList[i].FontFamily;
				glyph2.FontSize = textElementList[i].FontSize;
				glyph2.CharId = textElementList[i].CharId;
				glyph2.ToUnicode = text[1].ToString();
				glyph2.BoundingRect = new Rect(boundingRect.X + boundingRect.Width / 2.0, boundingRect.Y, boundingRect.Width / 2.0, boundingRect.Height);
				textElementList.RemoveAt(i);
				textElementList.Insert(i, glyph);
				if (textElementList.Count > i + 1 && textElementList[i + 1].ToUnicode == null)
				{
					textElementList.RemoveAt(i + 1);
				}
				textElementList.Insert(i + 1, glyph2);
				i++;
			}
		}
		return textElementList;
	}

	private void RenderTextElementWithSpacing(string[] textElements, string tokenType)
	{
		List<RenderedString> list = new List<RenderedString>();
		string text = string.Join("", textElements);
		if (!m_resources.ContainsKey(CurrentFont))
		{
			return;
		}
		(m_resources[CurrentFont] as FontStructure).IsSameFont = m_resources.isSameFont();
		if ((m_resources[CurrentFont] as FontStructure).FontSize != FontSize)
		{
			(m_resources[CurrentFont] as FontStructure).FontSize = FontSize;
		}
		FontStructure fontStructure = m_resources[CurrentFont] as FontStructure;
		List<float> characterSpacings = null;
		list = fontStructure.DecodeTextTJ(text, m_resources.isSameFont());
		byte[] bytes = Encoding.Unicode.GetBytes(fontStructure.ToGetEncodedText(text, m_resources.isSameFont()));
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		int num = 0;
		for (int i = 0; i < bytes.Length; i += 2)
		{
			dictionary.Add(num, bytes[i]);
			num++;
		}
		TextElement textElement = new TextElement(text, DocumentMatrix);
		textElement.IsExtractTextData = m_isExtractTextData;
		textElement.FontName = fontStructure.FontName;
		textElement.FontStyle = fontStructure.FontStyle;
		textElement.TextColor = TextColor;
		textElement.FontSize = FontSize;
		textElement.TextScaling = m_textScaling;
		textElement.EncodedTextBytes = dictionary;
		textElement.FontEncoding = fontStructure.FontEncoding;
		textElement.FontGlyphWidths = fontStructure.FontGlyphWidths;
		textElement.DefaultGlyphWidth = fontStructure.DefaultGlyphWidth;
		textElement.RenderingMode = RenderingMode;
		textElement.IsFindText = isFindText;
		textElement.IsPdfium = IsPdfium;
		textElement.UnicodeCharMapTable = fontStructure.UnicodeCharMapTable;
		Dictionary<int, int> fontGlyphWidths = fontStructure.FontGlyphWidths;
		textElement.CidToGidReverseMapTable = fontStructure.CidToGidReverseMapTable;
		textElement.IsType1Font = fontStructure.IsType1Font;
		textElement.Is1C = fontStructure.Is1C;
		textElement.IsCID = fontStructure.IsCID;
		textElement.CharacterMapTable = fontStructure.CharacterMapTable;
		textElement.ReverseMapTable = fontStructure.ReverseMapTable;
		textElement.Fontfile2Glyph = fontStructure.GlyphFontFile2;
		textElement.structure = fontStructure;
		textElement.Isembeddedfont = fontStructure.IsEmbedded;
		textElement.Ctm = CTRM;
		textElement.textLineMatrix = TextMatrix;
		textElement.Rise = Rise;
		textElement.transformMatrix = DocumentMatrix;
		textElement.documentMatrix = DocumentMatrix;
		textElement.FontID = CurrentFont;
		textElement.OctDecMapTable = fontStructure.OctDecMapTable;
		textElement.TextHorizontalScaling = HorizontalScaling;
		textElement.ZapfPostScript = fontStructure.ZapfPostScript;
		textElement.LineWidth = MitterLength;
		textElement.RenderingMode = RenderingMode;
		textElement.testdict = testdict;
		textElement.pageRotation = pageRotation;
		textElement.zoomFactor = zoomFactor;
		textElement.SubstitutedFontsList = substitutedFontsList;
		textElement.m_mcid = m_mcid;
		textElement.isExtractText = isExtractLineCollection;
		if (fontStructure.BaseFontEncoding == "WinAnsiEncoding")
		{
			isWinAnsiEncoding = true;
		}
		if (fontStructure.Flags != null)
		{
			textElement.FontFlag = fontStructure.Flags.IntValue;
		}
		if (fontStructure.IsType1Font)
		{
			textElement.IsType1Font = true;
			textElement.differenceTable = fontStructure.differenceTable;
			textElement.differenceMappedTable = fontStructure.DifferencesDictionary;
		}
		textElement.WordSpacing = Objects.WordSpacing;
		textElement.CharacterSpacing = Objects.CharacterSpacing;
		if (m_beginText)
		{
			m_beginText = false;
		}
		Matrix textmatrix = default(Matrix);
		if (fontStructure.fontType.Value != "Type3" || isExtractLineCollection || IsExtractTextData)
		{
			if (m_isCurrentPositionChanged)
			{
				m_isCurrentPositionChanged = false;
				m_endTextPosition = CurrentLocation;
				m_textElementWidth = textElement.RenderWithSpace(m_graphicsObject, new PointF(m_endTextPosition.X, m_endTextPosition.Y - FontSize), list, characterSpacings, m_textScaling, fontGlyphWidths, fontStructure.Type1GlyphHeight, fontStructure.differenceTable, fontStructure.DifferencesDictionary, fontStructure.differenceEncoding, out textmatrix);
				extractTextElement.Add(textElement);
				if (textElement.Fontfile2Glyph != null && textElement.Fontfile2Glyph.IsFontFile2)
				{
					textElement.textElementGlyphList = ReplaceLigatureFromGlyphList(textElement.textElementGlyphList);
				}
				imageRenderGlyphList.AddRange(textElement.textElementGlyphList);
			}
			else
			{
				m_endTextPosition = new PointF(m_endTextPosition.X + m_textElementWidth, m_endTextPosition.Y);
				m_textElementWidth = textElement.RenderWithSpace(m_graphicsObject, new PointF(m_endTextPosition.X, m_endTextPosition.Y - FontSize), list, characterSpacings, m_textScaling, fontGlyphWidths, fontStructure.Type1GlyphHeight, fontStructure.differenceTable, fontStructure.DifferencesDictionary, fontStructure.differenceEncoding, out textmatrix);
				extractTextElement.Add(textElement);
				if (textElement.Fontfile2Glyph != null && textElement.Fontfile2Glyph.IsFontFile2)
				{
					textElement.textElementGlyphList = ReplaceLigatureFromGlyphList(textElement.textElementGlyphList);
				}
				imageRenderGlyphList.AddRange(textElement.textElementGlyphList);
			}
		}
		else
		{
			Matrix textLineMatrix = TextLineMatrix;
			FontStructure fontStructure2 = fontStructure;
			m_spacingWidth = 0f;
			RenderType3GlyphImagesTJ(fontStructure, list);
			TextLineMatrix = textLineMatrix;
			fontStructure = fontStructure2;
		}
		TextMatrix = textmatrix;
	}

	public bool IsValidEmail(string email)
	{
		Regex regex = new Regex("^[-a-zA-Z0-9][-.a-zA-Z0-9]*@[-.a-zA-Z0-9]+(\\.[-.a-zA-Z0-9]+)*\\.(com|edu|info|gov|int|mil|net|org|biz|name|museum|coop|aero|pro|tv|[a-zA-Z]{2})$", RegexOptions.IgnorePatternWhitespace);
		bool flag = false;
		if (string.IsNullOrEmpty(email))
		{
			return false;
		}
		return regex.IsMatch(email);
	}

	private Color GetColor(string[] colorElement, string colorSpace)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 1f;
		if (colorSpace == "RGB" && colorElement.Length == 3)
		{
			num = FloatParse(colorElement[0]);
			num2 = FloatParse(colorElement[1]);
			num3 = FloatParse(colorElement[2]);
			num4 = m_opacity;
		}
		else if (colorSpace == "Gray" && colorElement.Length == 1)
		{
			num = (num2 = (num3 = (colorElement[0].Contains("/") ? 0f : ((!isBlack) ? FloatParse(colorElement[0]) : 0f))));
			num4 = m_opacity;
		}
		else if (colorSpace == "DeviceCMYK" && colorElement.Length == 4)
		{
			float.TryParse(colorElement[0], out var result);
			float.TryParse(colorElement[1], out var result2);
			float.TryParse(colorElement[2], out var result3);
			float.TryParse(colorElement[3], out var result4);
			return ConvertCMYKtoRGB(result, result2, result3, result4);
		}
		return Color.FromArgb((byte)(num4 * 255f), (byte)(num * 255f), (byte)(num2 * 255f), (byte)(num3 * 255f));
	}

	private Color ConvertCMYKtoRGB(float c, float m, float y, float k)
	{
		float num = (float)((double)c * (-4.387332384609988 * (double)c + 54.48615194189176 * (double)m + 18.82290502165302 * (double)y + 212.25662451639585 * (double)k + -285.2331026137004) + (double)m * (1.7149763477362134 * (double)m - 5.6096736904047315 * (double)y + -17.873870861415444 * (double)k - 5.497006427196366) + (double)y * (-2.5217340131683033 * (double)y - 21.248923337353073 * (double)k + 17.5119270841813) + (double)k * (-21.86122147463605 * (double)k - 189.48180835922747) + 255.0);
		float num2 = (float)((double)c * (8.841041422036149 * (double)c + 60.118027045597366 * (double)m + 6.871425592049007 * (double)y + 31.159100130055922 * (double)k + -79.2970844816548) + (double)m * (-15.310361306967817 * (double)m + 17.575251261109482 * (double)y + 131.35250912493976 * (double)k - 190.9453302588951) + (double)y * (4.444339102852739 * (double)y + 9.8632861493405 * (double)k - 24.86741582555878) + (double)k * (-20.737325471181034 * (double)k - 187.80453709719578) + 255.0);
		float num3 = (float)((double)c * (0.8842522430003296 * (double)c + 8.078677503112928 * (double)m + 30.89978309703729 * (double)y - 0.23883238689178934 * (double)k + -14.183576799673286) + (double)m * (10.49593273432072 * (double)m + 63.02378494754052 * (double)y + 50.606957656360734 * (double)k - 112.23884253719248) + (double)y * (0.03296041114873217 * (double)y + 115.60384449646641 * (double)k + -193.58209356861505) + (double)k * (-22.33816807309886 * (double)k - 180.12613974708367) + 255.0);
		return Color.FromArgb(255, (int)((num > 255f) ? 255f : ((num < 0f) ? 0f : num)), (int)((num2 > 255f) ? 255f : ((num2 < 0f) ? 0f : num2)), (int)((num3 > 255f) ? 255f : ((num3 < 0f) ? 0f : num3)));
	}

	private void DrawNewLine()
	{
		m_isCurrentPositionChanged = true;
		if (m_resources[CurrentFont] is FontStructure fontStructure)
		{
			Font font = new Font(TextElement.CheckFontName(fontStructure.FontName), FontSize);
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

	private void GetWordSpacing(string[] spacing)
	{
		Objects.WordSpacing = FloatParse(spacing[0]);
	}

	private void GetCharacterSpacing(string[] spacing)
	{
		Objects.CharacterSpacing = FloatParse(spacing[0]);
		m_characterSpacing = Objects.CharacterSpacing;
	}

	private void GetScalingFactor(string[] scaling)
	{
		m_textScaling = FloatParse(scaling[0]);
		HorizontalScaling = FloatParse(scaling[0]);
	}

	private Matrix CalculateTextMatrixupdate(Matrix m, float Width, bool isImage)
	{
		double offsetX = ((!isImage) ? ((double)(Width * FontSize) * 0.001) : ((double)(Width * 1f)));
		return new Matrix(1.0, 0.0, 0.0, 1.0, offsetX, 0.0) * m;
	}

	private Matrix GetTransformationMatrix(Matrix transform)
	{
		return new Matrix((float)transform.M11, (float)transform.M12, (float)transform.M21, (float)transform.M22, (float)transform.OffsetX, (float)transform.OffsetY);
	}

	private void AddLine(string[] line)
	{
		CurrentLocation = new PointF(FloatParse(line[0]), FloatParse(line[1]));
		m_currentPath.Segments.Add(new LineSegment
		{
			Point = CurrentLocation
		});
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
		bezierSegment.Point1 = CurrentLocation;
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

	private void BeginPath(string[] point)
	{
		CurrentLocation = new PointF(FloatParse(point[0]), FloatParse(point[1]));
		if (m_currentPath != null && m_currentPath.Segments.Count == 0)
		{
			CurrentGeometry.Figures.Remove(CurrentPath);
		}
		m_currentPath = new PathFigure();
		m_currentPath.StartPoint = CurrentLocation;
		CurrentGeometry.Figures.Add(m_currentPath);
	}

	private void BeginPath(float x, float y)
	{
		CurrentLocation = new PointF(x, y);
		if (m_currentPath != null && m_currentPath.Segments.Count == 0)
		{
			CurrentGeometry.Figures.Remove(CurrentPath);
		}
		m_currentPath = new PathFigure();
		m_currentPath.StartPoint = CurrentLocation;
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
			Point = CurrentLocation
		});
	}

	private void GetXObject(string[] xobjectElement)
	{
		if (!m_resources.ContainsKey(xobjectElement[0].Replace("/", "")) || !(m_resources[xobjectElement[0].Replace("/", "")] is XObjectElement) || !(m_resources[xobjectElement[0].Replace("/", "")] is XObjectElement xObjectElement))
		{
			return;
		}
		if (m_selectablePrintDocument)
		{
			xObjectElement.m_isPrintSelected = m_selectablePrintDocument;
			xObjectElement.m_pageHeight = m_pageHeight;
		}
		m_graphicsState = xObjectElement.Render(m_graphicsObject, m_resources, m_graphicsState, m_objects, currentPageHeight, out List<Glyph> glyphList);
		if (glyphList != null)
		{
			glyphList = ReplaceLigatureFromGlyphList(glyphList);
		}
		if (imageRenderGlyphList != null && glyphList != null)
		{
			imageRenderGlyphList.AddRange(glyphList);
		}
		if (xObjectElement.IsExtractLineCollection)
		{
			if (extractTextElement.Count == 0)
			{
				extractTextElement = xObjectElement.ExtractTextElements;
			}
			else
			{
				for (int i = 0; i < xObjectElement.ExtractTextElements.Count; i++)
				{
					extractTextElement.Add(xObjectElement.ExtractTextElements[i]);
				}
			}
		}
		glyphList.Clear();
	}

	private Matrix CalculateType3TextMatrixupdate(Matrix m, float Width, bool isImage)
	{
		double offsetX = ((double)Width * (double)FontSize + (double)Objects.CharacterSpacing + (double)m_type3WhiteSpaceWidth) * (double)(HorizontalScaling / 100f);
		return new Matrix(1.0, 0.0, 0.0, 1.0, offsetX, 0.0) * m;
	}

	private void GetColorSpaceValue(string[] colorspaceelement)
	{
		if (!m_resources.ContainsKey(colorspaceelement[0].Replace("/", "")) || !(m_resources[colorspaceelement[0].Replace("/", "")] is ExtendColorspace))
		{
			return;
		}
		foreach (IPdfPrimitive item in (m_resources[colorspaceelement[0].Replace("/", "")] as ExtendColorspace).ColorSpaceValueArray as PdfArray)
		{
			if (item is PdfName && (item as PdfName).Value == "Black")
			{
				isBlack = true;
			}
		}
	}
}
