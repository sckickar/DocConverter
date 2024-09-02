using System;
using System.Collections.Generic;
using SkiaSharp;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;
using DocGen.PdfViewer.Base;

namespace DocGen.Pdf.Parsing;

internal class Pattern : Colorspace
{
	private Pattern m_patterType;

	private Colorspace m_alternateColorspace;

	private PdfArray m_patternMatrix;

	private IPdfPrimitive m_patternDictioanry;

	private GraphicObjectDataHelperNet m_objects;

	private Bitmap m_brushImage;

	private Colorspace m_currentColorspace;

	private string[] m_dashedLine;

	private float m_lineCap;

	private PointF m_currentLocation = PointF.Empty;

	private bool m_isCurrentPositionChanged;

	private PathGeometry m_currentGeometry = new PathGeometry();

	private PathFigure m_currentPath;

	private GraphicsHelper m_brushGraphics;

	private bool m_isRectangle;

	private bool m_isCircle;

	private string m_rectangleWidth;

	private TilingPattern m_tilingPattern;

	internal Colorspace AlternateColorspace
	{
		get
		{
			return m_alternateColorspace;
		}
		set
		{
			m_alternateColorspace = value;
		}
	}

	internal PdfArray PatternMatrix
	{
		get
		{
			return m_patternMatrix;
		}
		set
		{
			m_patternMatrix = value;
		}
	}

	internal Pattern Type
	{
		get
		{
			return m_patterType;
		}
		set
		{
			m_patterType = value;
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

	private DocGen.Drawing.Matrix CurrentMatrix
	{
		get
		{
			return m_objects.drawing2dMatrixCTM;
		}
		set
		{
			m_objects.drawing2dMatrixCTM = value;
		}
	}

	private float MitterLength
	{
		get
		{
			return m_objects.m_mitterLength;
		}
		set
		{
			m_objects.m_mitterLength = value;
		}
	}

	private DocGen.PdfViewer.Base.Matrix DocumentMatrix
	{
		get
		{
			return m_objects.documentMatrix;
		}
		set
		{
			m_objects.documentMatrix = value;
		}
	}

	private DocGen.Drawing.Matrix Drawing2dMatrixCTM
	{
		get
		{
			return m_objects.drawing2dMatrixCTM;
		}
		set
		{
			m_objects.drawing2dMatrixCTM = value;
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

	private PdfBrush NonStrokingBrush
	{
		get
		{
			if (m_objects.NonStrokingBrush != null)
			{
				return m_objects.NonStrokingBrush;
			}
			return null;
		}
	}

	private PdfBrush StrokingBrush
	{
		get
		{
			if (m_objects.StrokingBrush != null)
			{
				return m_objects.StrokingBrush;
			}
			return null;
		}
	}

	internal override int Components => 1;

	internal void SetValue(IPdfPrimitive array)
	{
	}

	private Pattern GetPatternType(IPdfPrimitive patternValue)
	{
		if (patternValue is PdfDictionary pdfDictionary)
		{
			if (pdfDictionary.Items.ContainsKey(new PdfName("PatternType")))
			{
				switch ((pdfDictionary.Items[new PdfName("PatternType")] as PdfNumber).IntValue)
				{
				case 1:
					return Type = new TilingPattern(pdfDictionary);
				case 2:
					return Type = new ShadingPattern(pdfDictionary, m_isRectangle, m_isCircle, m_rectangleWidth);
				}
			}
			else if (pdfDictionary.Items.ContainsKey(new PdfName("ShadingType")))
			{
				return Type = new ShadingPattern(pdfDictionary, m_isRectangle, m_isCircle, m_rectangleWidth);
			}
		}
		return null;
	}

	private void SetPattern(string[] pars, PdfPageResources resource)
	{
		if (resource.ContainsKey(pars[0].Replace("/", "")) && resource[pars[0].Replace("/", "")] is ExtendColorspace)
		{
			m_patternDictioanry = (resource[pars[0].Replace("/", "")] as ExtendColorspace).ColorSpaceValueArray;
		}
	}

	internal override Color GetColor(string[] pars)
	{
		return Color.Gray;
	}

	internal override Color GetColor(byte[] bytes, int offset)
	{
		return Colorspace.GetRgbColor(bytes, offset);
	}

	internal override PdfBrush GetBrush(string[] pars, PdfPageResources resource)
	{
		return new PdfSolidBrush(Color.Gray);
	}

	internal override void SetOperatorValues(bool IsRectangle, bool IsCircle, string RectangleWidth)
	{
		m_isRectangle = IsRectangle;
		m_isCircle = IsCircle;
		m_rectangleWidth = RectangleWidth;
	}

	private void GetTilingImage(TilingPattern tilingPattern)
	{
		m_tilingPattern = tilingPattern;
		if (tilingPattern.BoundingRectangle.Width < 1 && tilingPattern.BoundingRectangle.Height < 1)
		{
			m_brushImage = new Bitmap(2, 2);
		}
		else
		{
			m_brushImage = new Bitmap(tilingPattern.BoundingRectangle.Width, tilingPattern.BoundingRectangle.Height);
		}
		m_objects = new GraphicObjectDataHelperNet();
		m_brushGraphics = new GraphicsHelper(m_brushImage);
		tilingPattern.TilingPatternMatrix = GetPatternMatrix(tilingPattern);
		m_objects.Ctm = DocGen.PdfViewer.Base.Matrix.Identity;
		char[] array = new char[6] { '(', ')', '[', ']', '<', '>' };
		PdfStream data = tilingPattern.Data;
		PdfRecordCollection pdfRecordCollection = new ContentParser(data.GetDecompressedData()).ReadContent();
		PageResourceLoader pageResourceLoader = new PageResourceLoader();
		new PdfDictionary();
		DocGen.Drawing.Matrix transform = m_brushGraphics.Transform;
		m_objects.documentMatrix = new DocGen.PdfViewer.Base.Matrix(1.33333333333333 * (double)(m_brushGraphics.DpiX / 96f) * (double)transform.Elements[0], 0.0, 0.0, -1.33333333333333 * (double)(m_brushGraphics.DpiX / 96f) * (double)transform.Elements[3], 0.0, (float)m_brushImage.Height * transform.Elements[3]);
		DocGen.Drawing.Matrix currentMatrix = (Drawing2dMatrixCTM = new DocGen.Drawing.Matrix(1f, 0f, 0f, 1f, 0f, 0f));
		CurrentMatrix = currentMatrix;
		if (pdfRecordCollection != null)
		{
			for (int i = 0; i < pdfRecordCollection.RecordCollection.Count; i++)
			{
				string text = pdfRecordCollection.RecordCollection[i].OperatorName;
				string[] operands = pdfRecordCollection.RecordCollection[i].Operands;
				char[] array2 = array;
				for (int j = 0; j < array2.Length; j++)
				{
					char c = array2[j];
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
				switch (text2.Length)
				{
				case 2:
					switch (text2[1])
					{
					case 's':
						if (text2 == "cs")
						{
							SetStrokingColorspace(GetColorspace(operands));
						}
						break;
					case 'S':
						if (text2 == "CS")
						{
							SetNonStrokingColorspace(GetColorspace(operands));
						}
						break;
					case 'g':
						if (text2 == "rg")
						{
							SetStrokingRGBColor(GetColor(operands, "stroking", "RGB"));
						}
						break;
					case 'G':
						if (text2 == "RG")
						{
							SetNonStrokingRGBColor(GetColor(operands, "nonstroking", "RGB"));
						}
						break;
					case 'e':
						if (text2 == "re")
						{
							GetClipRectangle(operands);
						}
						break;
					case 'm':
						if (text2 == "cm")
						{
							float m = float.Parse(operands[0]);
							float m2 = float.Parse(operands[1]);
							float m3 = float.Parse(operands[2]);
							float m4 = float.Parse(operands[3]);
							float dx = float.Parse(operands[4]);
							float dy = float.Parse(operands[5]);
							CurrentMatrix = new DocGen.Drawing.Matrix(m, m2, m3, m4, dx, dy);
						}
						break;
					case 'o':
					{
						if (!(text2 == "Do"))
						{
							break;
						}
						PdfDictionary resources = tilingPattern.Resources;
						if (!resources.ContainsKey("XObject"))
						{
							break;
						}
						PdfPageResources pageResources = new PdfPageResources();
						PdfDictionary pdfDictionary = new PdfDictionary();
						if (resources["XObject"] is PdfDictionary)
						{
							pdfDictionary = resources["XObject"] as PdfDictionary;
						}
						else if (resources["XObject"] as PdfReferenceHolder != null && (resources["XObject"] as PdfReferenceHolder).Object is PdfDictionary)
						{
							pdfDictionary = (resources["XObject"] as PdfReferenceHolder).Object as PdfDictionary;
						}
						Dictionary<string, PdfMatrix> commonMatrix = new Dictionary<string, PdfMatrix>();
						pageResources = pageResourceLoader.UpdatePageResources(pageResources, pageResourceLoader.GetImageResources(resources, null, ref commonMatrix));
						foreach (KeyValuePair<PdfName, IPdfPrimitive> item in pdfDictionary.Items)
						{
							ImageStructureNet imageStructureNet = null;
							if (!item.Key.Value.Contains(operands[0].Replace("/", "")) || !pageResources.ContainsKey(operands[0].Replace("/", "")) || !(pageResources.Resources[operands[0].Replace("/", "")] is ImageStructureNet))
							{
								continue;
							}
							imageStructureNet = pageResources.Resources[operands[0].Replace("/", "")] as ImageStructureNet;
							bool flag = false;
							Bitmap bitmap;
							if (tilingPattern.BoundingRectangle.Width < 1 && tilingPattern.BoundingRectangle.Height < 1)
							{
								bitmap = new Bitmap(2, 2);
								flag = true;
							}
							else
							{
								bitmap = new Bitmap(tilingPattern.BoundingRectangle.Width, tilingPattern.BoundingRectangle.Height);
							}
							using GraphicsHelper graphicsHelper = new GraphicsHelper(bitmap);
							Bitmap embeddedImage = imageStructureNet.EmbeddedImage;
							_ = graphicsHelper.Transform;
							_ = graphicsHelper.PageUnit;
							graphicsHelper.PageUnit = GraphicsUnit.Pixel;
							graphicsHelper.Transform = new DocGen.Drawing.Matrix(1f, 0f, 0f, 1f, 0f, 0f);
							new DocGen.Drawing.Matrix(CurrentMatrix.Elements[0], CurrentMatrix.Elements[1], CurrentMatrix.Elements[2], CurrentMatrix.Elements[3], CurrentMatrix.Elements[4], CurrentMatrix.Elements[5]);
							if (!flag)
							{
								graphicsHelper.Transform = Scale(1f, -1f, 0f, 1f);
							}
							if (embeddedImage == null)
							{
								break;
							}
							graphicsHelper.DrawImage(embeddedImage, new RectangleF(0f, 0f, 1f, 1f));
							m_brushImage = bitmap;
							continue;
						}
						break;
					}
					}
					break;
				case 1:
					switch (text2[0])
					{
					case 'd':
						if (operands[0] != "[]" && !operands[0].Contains("\n"))
						{
							m_dashedLine = operands;
						}
						break;
					case 'w':
						MitterLength = float.Parse(operands[0]);
						break;
					case 'W':
					{
						DocGen.PdfViewer.Base.Matrix documentMatrix = DocumentMatrix;
						DocGen.PdfViewer.Base.Matrix transform2 = new DocGen.PdfViewer.Base.Matrix(Drawing2dMatrixCTM.Elements[0], Drawing2dMatrixCTM.Elements[1], Drawing2dMatrixCTM.Elements[2], Drawing2dMatrixCTM.Elements[3], Drawing2dMatrixCTM.OffsetX, Drawing2dMatrixCTM.OffsetY) * documentMatrix;
						new DocGen.Drawing.Matrix((float)Math.Round(transform2.M11, 5, MidpointRounding.ToEven), (float)Math.Round(transform2.M12, 5, MidpointRounding.ToEven), (float)Math.Round(transform2.M21, 5, MidpointRounding.ToEven), (float)Math.Round(transform2.M22, 5, MidpointRounding.ToEven), (float)Math.Round(transform2.OffsetX, 5, MidpointRounding.ToEven), (float)Math.Round(transform2.OffsetY, 5, MidpointRounding.ToEven));
						_ = m_brushGraphics.Transform;
						_ = m_brushGraphics.PageUnit;
						m_brushGraphics.PageUnit = GraphicsUnit.Pixel;
						m_brushGraphics.Transform = new DocGen.Drawing.Matrix(1f, 0f, 0f, 1f, 0f, 0f);
						foreach (PathFigure figure in m_currentGeometry.Figures)
						{
							figure.IsClosed = true;
							figure.IsFilled = true;
						}
						m_currentGeometry.FillRule = FillRule.Nonzero;
						GraphicsPath geometry = GetGeometry(m_currentGeometry, transform2);
						if (geometry.PointCount != 0)
						{
							m_brushGraphics.SetClip(geometry);
						}
						break;
					}
					case 'J':
						m_lineCap = float.Parse(operands[0]);
						break;
					case 'm':
						BeginPath(operands);
						break;
					case 'l':
						AddLine(operands);
						break;
					case 'S':
					case 's':
						DrawPath();
						break;
					case 'f':
						FillPath("Winding");
						CurrentLocation = PointF.Empty;
						break;
					case 'k':
						SetStrokingCMYKColor(GetColor(operands, "stroking", "DeviceCMYK"));
						break;
					case 'K':
						SetNonStrokingCMYKColor(GetColor(operands, "nonstroking", "DeviceCMYK"));
						break;
					}
					break;
				}
			}
		}
		data.InternalStream.Dispose();
		tilingPattern.EmbeddedImage = m_brushImage;
	}

	private void SetStrokingCMYKColor(Color color)
	{
		m_objects.StrokingColorspace = new DeviceCMYK();
		SetStrokingColor(color);
	}

	private void SetNonStrokingCMYKColor(Color color)
	{
		m_objects.NonStrokingColorspace = new DeviceCMYK();
		SetNonStrokingColor(color);
	}

	private void FillPath(string mode)
	{
		PdfPen pdfPen = null;
		if (!(StrokingBrush is PdfTextureBrush))
		{
			pdfPen = ((StrokingBrush == null) ? new PdfPen(Color.Black) : new PdfPen(StrokingBrush));
		}
		DocGen.PdfViewer.Base.Matrix documentMatrix = DocumentMatrix;
		DocGen.PdfViewer.Base.Matrix matrix = new DocGen.PdfViewer.Base.Matrix(Drawing2dMatrixCTM.Elements[0], Drawing2dMatrixCTM.Elements[1], Drawing2dMatrixCTM.Elements[2], Drawing2dMatrixCTM.Elements[3], Drawing2dMatrixCTM.OffsetX, Drawing2dMatrixCTM.OffsetY) * documentMatrix;
		DocGen.Drawing.Matrix matrix2 = new DocGen.Drawing.Matrix((float)matrix.M11, (float)matrix.M12, (float)matrix.M21, (float)matrix.M22, (float)matrix.OffsetX, (float)matrix.OffsetY);
		DocGen.Drawing.Matrix matrix3 = new DocGen.Drawing.Matrix();
		matrix3.Multiply(matrix2);
		DocGen.Drawing.Matrix transform = m_brushGraphics.Transform;
		GraphicsUnit pageUnit = m_brushGraphics.PageUnit;
		m_brushGraphics.PageUnit = GraphicsUnit.Pixel;
		m_brushGraphics.Transform = new DocGen.Drawing.Matrix(1f, 0f, 0f, 1f, 0f, 0f);
		m_brushGraphics.Transform = matrix3;
		foreach (PathFigure figure in m_currentGeometry.Figures)
		{
			figure.IsClosed = true;
			figure.IsFilled = true;
		}
		m_currentGeometry.FillRule = ((mode == "Winding") ? FillRule.Nonzero : FillRule.EvenOdd);
		GraphicsPath geometry = GetGeometry(m_currentGeometry, new DocGen.PdfViewer.Base.Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0));
		geometry.FillMode = ((m_currentGeometry.FillRule == FillRule.Nonzero) ? FillMode.Winding : FillMode.Alternate);
		m_brushGraphics.FillPath(pdfPen.Color, geometry);
		m_currentGeometry = new PathGeometry();
		m_currentPath = null;
		m_brushGraphics.Transform = transform;
		m_brushGraphics.PageUnit = pageUnit;
	}

	private void GetClipRectangle(string[] rectangle)
	{
		float num = float.Parse(rectangle[0]);
		float num2 = float.Parse(rectangle[1]);
		float num3 = float.Parse(rectangle[2]);
		float num4 = float.Parse(rectangle[3]);
		BeginPath(num, num2);
		AddLine(num + num3, num2);
		AddLine(num + num3, num2 + num4);
		AddLine(num, num2 + num4);
		EndPath();
		new RectangleF(num, num2, num3, num4);
	}

	private void AddLine(float x, float y)
	{
		CurrentLocation = new PointF(x, y);
		m_currentPath.Segments.Add(new LineSegment
		{
			Point = new PointF(CurrentLocation.X, CurrentLocation.Y)
		});
	}

	private void EndPath()
	{
		if (m_currentPath != null)
		{
			m_currentPath.IsClosed = true;
		}
	}

	private void DrawPath()
	{
		PdfPen pdfPen = new PdfPen((NonStrokingBrush == null) ? new PdfPen(Color.Black).Brush : NonStrokingBrush);
		DocGen.Drawing.Matrix transform = m_brushGraphics.Transform;
		GraphicsUnit pageUnit = m_brushGraphics.PageUnit;
		m_brushGraphics.PageUnit = GraphicsUnit.Pixel;
		m_brushGraphics.Transform = new DocGen.Drawing.Matrix(1f, 0f, 0f, 1f, 0f, 0f);
		m_brushGraphics.Transform = CurrentMatrix;
		GraphicsPath geometry = GetGeometry(m_currentGeometry, new DocGen.PdfViewer.Base.Matrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0));
		if (MitterLength != 0f)
		{
			pdfPen.Width = MitterLength;
		}
		if (m_dashedLine != null)
		{
			string text = m_dashedLine[0];
			text = text.Substring(1, text.Length - 2);
			text = text.Trim();
			List<string> list = new List<string>();
			string[] array = text.Split(' ');
			foreach (string text2 in array)
			{
				if (text2 == "0")
				{
					list.Add("0.000000001");
				}
				else if (text2 != "")
				{
					list.Add(text2);
				}
			}
			float[] array2 = new float[list.Count];
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j] != "")
				{
					array2[j] = float.Parse(list[j]);
				}
			}
			if (array2.Length != 0 && MitterLength < array2[0] && MitterLength != 0f)
			{
				for (int k = 0; k < array2.Length; k++)
				{
					array2[k] /= MitterLength;
				}
			}
			if (array2.Length != 0)
			{
				pdfPen.DashPattern = array2;
			}
			if (m_lineCap == 1f)
			{
				m_dashedLine = null;
			}
		}
		m_currentGeometry.FillRule = FillRule.Nonzero;
		geometry.FillMode = FillMode.Alternate;
		PdfColor color = pdfPen.Color;
		SKPaint sKPaint = new SKPaint();
		sKPaint.Color = new SKColor(color.R, color.G, color.B, color.A);
		sKPaint.IsAntialias = true;
		m_brushGraphics.m_canvas.DrawPath(geometry, sKPaint);
		m_currentGeometry = new PathGeometry();
		m_currentPath = null;
		m_brushGraphics.Transform = transform;
		m_brushGraphics.PageUnit = pageUnit;
	}

	private void AddLine(string[] line)
	{
		CurrentLocation = new PointF(float.Parse(line[0]), float.Parse(line[1]));
		m_currentPath.Segments.Add(new LineSegment
		{
			Point = new PointF(CurrentLocation.X, CurrentLocation.Y)
		});
	}

	private void BeginPath(string[] point)
	{
		CurrentLocation = new PointF(float.Parse(point[0]), float.Parse(point[1]));
		if (m_currentPath != null && m_currentPath.Segments.Count == 0)
		{
			m_currentGeometry.Figures.Remove(CurrentPath);
		}
		m_currentPath = new PathFigure();
		m_currentPath.StartPoint = new PointF(CurrentLocation.X, CurrentLocation.Y);
		m_currentGeometry.Figures.Add(m_currentPath);
	}

	private void BeginPath(float x, float y)
	{
		CurrentLocation = new PointF(x, y);
		if (m_currentPath != null && m_currentPath.Segments.Count == 0)
		{
			m_currentGeometry.Figures.Remove(CurrentPath);
		}
		m_currentPath = new PathFigure();
		m_currentPath.StartPoint = new PointF(CurrentLocation.X, CurrentLocation.Y);
		m_currentGeometry.Figures.Add(m_currentPath);
	}

	internal GraphicsPath GetGeometry(PathGeometry geometry, DocGen.PdfViewer.Base.Matrix transform)
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

	private Color GetColor(string[] colorElement, string type, string colorSpace)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 1f;
		if (colorSpace == "RGB" && colorElement.Length == 3)
		{
			num = float.Parse(colorElement[0]);
			num2 = float.Parse(colorElement[1]);
			num3 = float.Parse(colorElement[2]);
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
		float num = 255f * (1f - c) * (1f - k);
		float num2 = 255f * (1f - m) * (1f - k);
		float num3 = 255f * (1f - y) * (1f - k);
		return Color.FromArgb(255, (int)((num > 255f) ? 255f : ((num < 0f) ? 0f : num)), (int)((num2 > 255f) ? 255f : ((num2 < 0f) ? 0f : num2)), (int)((num3 > 255f) ? 255f : ((num3 < 0f) ? 0f : num3)));
	}

	private void SetStrokingRGBColor(Color color)
	{
		m_objects.StrokingColorspace = new DeviceRGB();
		SetStrokingColor(color);
	}

	private void SetStrokingColor(Color color)
	{
		m_objects.StrokingBrush = new PdfPen(color).Brush;
	}

	private void SetNonStrokingRGBColor(Color color)
	{
		m_objects.NonStrokingColorspace = new DeviceRGB();
		SetNonStrokingColor(color);
	}

	private void SetNonStrokingColor(Color color)
	{
		m_objects.NonStrokingBrush = new PdfPen(color).Brush;
	}

	private void SetNonStrokingColorspace(Colorspace colorspace)
	{
		m_objects.NonStrokingColorspace = colorspace;
	}

	private void SetStrokingColorspace(Colorspace colorspace)
	{
		m_objects.StrokingColorspace = colorspace;
	}

	private Colorspace GetColorspace(string[] colorspaceelement)
	{
		if (Colorspace.IsColorSpace(colorspaceelement[0].Replace("/", "")))
		{
			m_currentColorspace = Colorspace.CreateColorSpace(colorspaceelement[0].Replace("/", ""));
		}
		else if (m_tilingPattern.Resources.ContainsKey(colorspaceelement[0].Replace("/", "")))
		{
			if (m_tilingPattern.Resources[colorspaceelement[0].Replace("/", "")] is ExtendColorspace)
			{
				ExtendColorspace obj = m_tilingPattern.Resources[colorspaceelement[0].Replace("/", "")] as ExtendColorspace;
				if (obj.ColorSpaceValueArray is PdfArray pdfArray)
				{
					m_currentColorspace = Colorspace.CreateColorSpace((pdfArray[0] as PdfName).Value, pdfArray);
				}
				PdfName pdfName = obj.ColorSpaceValueArray as PdfName;
				if (pdfName != null)
				{
					m_currentColorspace = Colorspace.CreateColorSpace(pdfName.Value);
				}
				if (obj.ColorSpaceValueArray is PdfDictionary array)
				{
					m_currentColorspace = Colorspace.CreateColorSpace("Shading", array);
				}
			}
		}
		else if (m_tilingPattern.Resources.ContainsKey("ColorSpace"))
		{
			PdfDictionary pdfDictionary = m_tilingPattern.Resources["ColorSpace"] as PdfDictionary;
			if (pdfDictionary.ContainsKey(colorspaceelement[0].Replace("/", "")) && pdfDictionary[colorspaceelement[0].Replace("/", "")] is PdfReferenceHolder && (pdfDictionary[colorspaceelement[0].Replace("/", "")] as PdfReferenceHolder).Object is PdfArray pdfArray2)
			{
				m_currentColorspace = Colorspace.CreateColorSpace((pdfArray2[0] as PdfName).Value, pdfArray2);
			}
		}
		return m_currentColorspace;
	}

	private DocGen.Drawing.Matrix Scale(float scaleX, float scaleY, float centerX, float centerY)
	{
		return Multiply(new DocGen.Drawing.Matrix(scaleX, 0f, 0f, scaleY, centerX, centerY), CurrentMatrix);
	}

	private DocGen.Drawing.Matrix Multiply(DocGen.Drawing.Matrix matrix1, DocGen.Drawing.Matrix matrix2)
	{
		return new DocGen.Drawing.Matrix(matrix1.Elements[0] * matrix2.Elements[0] + matrix1.Elements[1] * matrix2.Elements[2], matrix1.Elements[0] * matrix2.Elements[1] + matrix1.Elements[1] * matrix2.Elements[3], matrix1.Elements[2] * matrix2.Elements[0] + matrix1.Elements[3] * matrix2.Elements[2], matrix1.Elements[2] * matrix2.Elements[1] + matrix1.Elements[3] * matrix2.Elements[3], matrix1.OffsetX * matrix2.Elements[0] + matrix1.OffsetY * matrix2.Elements[2] + matrix2.OffsetX, matrix1.OffsetX * matrix2.Elements[1] + matrix1.OffsetY * matrix2.Elements[3] + matrix2.OffsetY);
	}

	private DocGen.Drawing.Matrix GetPatternMatrix(TilingPattern tilingPattern)
	{
		float floatValue = (tilingPattern.PatternMatrix[0] as PdfNumber).FloatValue;
		float floatValue2 = (tilingPattern.PatternMatrix[1] as PdfNumber).FloatValue;
		float floatValue3 = (tilingPattern.PatternMatrix[2] as PdfNumber).FloatValue;
		float floatValue4 = (tilingPattern.PatternMatrix[3] as PdfNumber).FloatValue;
		float floatValue5 = (tilingPattern.PatternMatrix[4] as PdfNumber).FloatValue;
		float floatValue6 = (tilingPattern.PatternMatrix[5] as PdfNumber).FloatValue;
		return new DocGen.Drawing.Matrix(floatValue, floatValue2, floatValue3, floatValue4, floatValue5, floatValue6);
	}
}
