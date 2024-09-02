using System;
using DocGen.Drawing;
using DocGen.Pdf.ColorSpace;
using DocGen.Pdf.IO;

namespace DocGen.Pdf.Graphics;

public class PdfPen : ICloneable
{
	private PdfColor m_color = PdfColor.Empty;

	private float m_dashOffset;

	private float[] m_dashPattern = new float[0];

	private PdfDashStyle m_dashStyle;

	private PdfLineCap m_lineCap;

	private PdfLineJoin m_lineJoin;

	private float m_width = 1f;

	private PdfBrush m_brush;

	private float m_miterLimit;

	private PdfColorSpace m_colorSpace;

	private PdfExtendedColor m_colorspaces;

	private bool m_bImmutable;

	private float[] m_comoundArray;

	private PdfLineCap m_StartCap;

	private PdfLineCap m_EndCap;

	internal bool isSkipPatternWidth;

	internal PdfExtendedColor Colorspaces
	{
		get
		{
			return m_colorspaces;
		}
		set
		{
			m_colorspaces = value;
		}
	}

	public PdfBrush Brush
	{
		get
		{
			PdfBrush brush = ((m_brush != null) ? m_brush.Clone() : null);
			if (m_brush != null)
			{
				ResetStroking(brush);
			}
			return m_brush;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Brush");
			}
			CheckImmutability("Brush");
			AssignBrush(value);
		}
	}

	public PdfColor Color
	{
		get
		{
			return m_color;
		}
		set
		{
			CheckImmutability("Color");
			m_color = value;
		}
	}

	public float DashOffset
	{
		get
		{
			return m_dashOffset;
		}
		set
		{
			CheckImmutability("DashOffset");
			m_dashOffset = value;
		}
	}

	public float[] DashPattern
	{
		get
		{
			return m_dashPattern;
		}
		set
		{
			if (DashStyle == PdfDashStyle.Solid)
			{
				throw new ArgumentException("This operation is not allowed. Set Custom dash style to change the pattern.");
			}
			CheckImmutability("DashPattern");
			m_dashPattern = value;
		}
	}

	public PdfDashStyle DashStyle
	{
		get
		{
			return m_dashStyle;
		}
		set
		{
			CheckImmutability("DashStyle");
			if (m_dashStyle != value)
			{
				m_dashStyle = value;
				switch (m_dashStyle)
				{
				case PdfDashStyle.Dash:
					m_dashPattern = new float[2] { 3f, 1f };
					break;
				case PdfDashStyle.Dot:
					m_dashPattern = new float[2] { 1f, 1f };
					break;
				case PdfDashStyle.DashDot:
					m_dashPattern = new float[4] { 3f, 1f, 1f, 1f };
					break;
				case PdfDashStyle.DashDotDot:
					m_dashPattern = new float[6] { 3f, 1f, 1f, 1f, 1f, 1f };
					break;
				default:
					m_dashStyle = PdfDashStyle.Solid;
					m_dashPattern = new float[0];
					break;
				case PdfDashStyle.Custom:
					break;
				}
			}
		}
	}

	public PdfLineCap LineCap
	{
		get
		{
			return m_lineCap;
		}
		set
		{
			CheckImmutability("LineCap");
			m_lineCap = value;
		}
	}

	public PdfLineJoin LineJoin
	{
		get
		{
			return m_lineJoin;
		}
		set
		{
			CheckImmutability("LineJoin");
			m_lineJoin = value;
		}
	}

	public float Width
	{
		get
		{
			return m_width;
		}
		set
		{
			CheckImmutability("Width");
			m_width = value;
		}
	}

	public float MiterLimit
	{
		get
		{
			return m_miterLimit;
		}
		set
		{
			CheckImmutability("MiterLimit");
			m_miterLimit = value;
		}
	}

	internal bool IsImmutable => m_bImmutable;

	internal float[] CompoundArray
	{
		get
		{
			return m_comoundArray;
		}
		set
		{
			m_comoundArray = value;
			CreateCompoundPen();
		}
	}

	internal PdfLineCap StartCap
	{
		get
		{
			return m_StartCap;
		}
		set
		{
			m_StartCap = value;
			LineCap = m_StartCap;
		}
	}

	internal PdfLineCap EndCap
	{
		get
		{
			return m_EndCap;
		}
		set
		{
			m_EndCap = value;
			LineCap = m_EndCap;
		}
	}

	private PdfPen()
	{
	}

	public PdfPen(PdfColor color)
	{
		Color = color;
	}

	public PdfPen(Color color)
	{
		Color = new PdfColor(color);
	}

	public PdfPen(PdfColor color, float width)
		: this(color)
	{
		Width = width;
	}

	public PdfPen(Color color, float width)
		: this(color)
	{
		Width = width;
	}

	public PdfPen(PdfBrush brush)
	{
		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}
		AssignBrush(brush);
	}

	public PdfPen(PdfBrush brush, float width)
		: this(brush)
	{
		Width = width;
	}

	internal PdfPen(PdfColor color, bool immutable)
		: this(color)
	{
		m_bImmutable = immutable;
	}

	public PdfPen(PdfExtendedColor color)
	{
		_ = color.ColorSpace;
		m_colorspaces = color;
		_ = color.ColorSpace;
		m_colorspaces = color;
		if (color is PdfCalRGBColor)
		{
			PdfCalRGBColor pdfCalRGBColor = color as PdfCalRGBColor;
			m_color = new PdfColor((byte)pdfCalRGBColor.Red, (byte)pdfCalRGBColor.Green, (byte)pdfCalRGBColor.Blue);
		}
		else if (color is PdfCalGrayColor)
		{
			PdfCalGrayColor pdfCalGrayColor = color as PdfCalGrayColor;
			m_color = new PdfColor((int)(byte)pdfCalGrayColor.Gray);
			m_color.Gray = Convert.ToSingle(pdfCalGrayColor.Gray);
		}
		else if (color is PdfLabColor)
		{
			PdfLabColor pdfLabColor = color as PdfLabColor;
			m_color = new PdfColor((byte)pdfLabColor.L, (byte)pdfLabColor.A, (byte)pdfLabColor.B);
		}
		else if (color is PdfICCColor)
		{
			PdfICCColor pdfICCColor = color as PdfICCColor;
			if (pdfICCColor.ColorSpaces.AlternateColorSpace is PdfCalGrayColorSpace)
			{
				m_color = new PdfColor((int)(byte)pdfICCColor.ColorComponents[0]);
				m_color.Gray = Convert.ToSingle(pdfICCColor.ColorComponents[0]);
			}
			else if (pdfICCColor.ColorSpaces.AlternateColorSpace is PdfCalRGBColorSpace)
			{
				m_color = new PdfColor((byte)pdfICCColor.ColorComponents[0], (byte)pdfICCColor.ColorComponents[1], (byte)pdfICCColor.ColorComponents[2]);
			}
			else if (pdfICCColor.ColorSpaces.AlternateColorSpace is PdfLabColorSpace)
			{
				m_color = new PdfColor((byte)pdfICCColor.ColorComponents[0], (byte)pdfICCColor.ColorComponents[1], (byte)pdfICCColor.ColorComponents[2]);
			}
			else if (pdfICCColor.ColorSpaces.AlternateColorSpace is PdfDeviceColorSpace)
			{
				switch ((pdfICCColor.ColorSpaces.AlternateColorSpace as PdfDeviceColorSpace).DeviceColorSpaceType.ToString())
				{
				case "RGB":
					m_color = new PdfColor((byte)pdfICCColor.ColorComponents[0], (byte)pdfICCColor.ColorComponents[1], (byte)pdfICCColor.ColorComponents[2]);
					break;
				case "GrayScale":
					m_color = new PdfColor((int)(byte)pdfICCColor.ColorComponents[0]);
					m_color.Gray = Convert.ToSingle(pdfICCColor.ColorComponents[0]);
					break;
				case "CMYK":
					m_color = new PdfColor((float)pdfICCColor.ColorComponents[0], (float)pdfICCColor.ColorComponents[1], (float)pdfICCColor.ColorComponents[2], (float)pdfICCColor.ColorComponents[3]);
					break;
				}
			}
			else
			{
				m_color = new PdfColor((byte)pdfICCColor.ColorComponents[0], (byte)pdfICCColor.ColorComponents[1], (byte)pdfICCColor.ColorComponents[2]);
			}
		}
		else if (color is PdfSeparationColor)
		{
			PdfSeparationColor pdfSeparationColor = color as PdfSeparationColor;
			m_color.Gray = (float)pdfSeparationColor.Tint;
		}
		else if (color is PdfIndexedColor)
		{
			PdfIndexedColor pdfIndexedColor = color as PdfIndexedColor;
			m_color.G = (byte)pdfIndexedColor.SelectColorIndex;
		}
	}

	object ICloneable.Clone()
	{
		return Clone();
	}

	public PdfPen Clone()
	{
		return MemberwiseClone() as PdfPen;
	}

	private void AssignBrush(PdfBrush brush)
	{
		if (brush is PdfSolidBrush pdfSolidBrush)
		{
			Color = pdfSolidBrush.Color;
			m_brush = pdfSolidBrush;
		}
		else
		{
			m_brush = brush.Clone();
			SetStrokingToBrush(m_brush);
		}
	}

	private void SetStrokingToBrush(PdfBrush brush)
	{
		PdfTilingBrush pdfTilingBrush = brush as PdfTilingBrush;
		PdfGradientBrush pdfGradientBrush = brush as PdfGradientBrush;
		if (pdfTilingBrush != null)
		{
			pdfTilingBrush.Stroking = true;
		}
		else if (pdfGradientBrush != null)
		{
			pdfGradientBrush.Stroking = true;
		}
		else if (!(brush is PdfSolidBrush))
		{
			throw new ArgumentException("Unsupported brush.", "brush");
		}
	}

	private void ResetStroking(PdfBrush brush)
	{
		PdfTilingBrush pdfTilingBrush = m_brush as PdfTilingBrush;
		PdfGradientBrush pdfGradientBrush = m_brush as PdfGradientBrush;
		if (pdfTilingBrush != null)
		{
			pdfTilingBrush.Stroking = false;
		}
		else if (pdfGradientBrush != null)
		{
			pdfGradientBrush.Stroking = false;
		}
		else if (!(brush is PdfSolidBrush))
		{
			throw new ArgumentException("Unsupported brush.", "brush");
		}
	}

	internal bool MonitorChanges(PdfPen currentPen, PdfStreamWriter streamWriter, PdfGraphics.GetResources getResources, bool saveState, PdfColorSpace currentColorSpace, PdfTransformationMatrix matrix)
	{
		bool flag = false;
		saveState = true;
		if (currentPen == null)
		{
			flag = true;
		}
		flag = DashControl(currentPen, saveState, streamWriter);
		if (saveState || Width != currentPen.Width)
		{
			streamWriter.SetLineWidth(Width);
			flag = true;
		}
		if (saveState || LineJoin != currentPen.LineJoin)
		{
			streamWriter.SetLineJoin(LineJoin);
			flag = true;
		}
		if (saveState || LineCap != currentPen.LineCap)
		{
			streamWriter.SetLineCap(LineCap);
			flag = true;
		}
		if (saveState || MiterLimit != currentPen.MiterLimit)
		{
			float miterLimit = MiterLimit;
			if (miterLimit > 0f)
			{
				streamWriter.SetMiterLimit(miterLimit);
				flag = true;
			}
		}
		if (saveState || Color != currentPen.Color || Brush != currentPen.Brush || m_colorSpace != currentColorSpace)
		{
			PdfBrush brush = m_brush;
			if (brush != null && !(brush is PdfSolidBrush))
			{
				PdfBrush pdfBrush = brush.Clone();
				SetStrokingToBrush(pdfBrush);
				if (pdfBrush is PdfGradientBrush pdfGradientBrush)
				{
					PdfTransformationMatrix matrix2 = pdfGradientBrush.Matrix;
					if (matrix2 != null)
					{
						matrix.Multiply(matrix2);
					}
					pdfGradientBrush.Matrix = matrix;
				}
				PdfBrush brush2 = currentPen?.Brush;
				flag |= pdfBrush.MonitorChanges(brush2, streamWriter, getResources, saveState, currentColorSpace);
			}
			else if (Colorspaces == null)
			{
				streamWriter.SetColorAndSpace(Color, currentColorSpace, forStroking: true);
				flag = true;
			}
			else
			{
				streamWriter.SetColorAndSpace(Color, currentColorSpace, forStroking: true, check: true);
				flag = true;
			}
		}
		return flag;
	}

	internal bool MonitorChanges(PdfPen currentPen, PdfStreamWriter streamWriter, PdfGraphics.GetResources getResources, bool saveState, PdfColorSpace currentColorSpace, PdfTransformationMatrix matrix, bool iccBased)
	{
		bool flag = false;
		saveState = true;
		if (currentPen == null)
		{
			flag = true;
		}
		flag = DashControl(currentPen, saveState, streamWriter);
		if (saveState || Width != currentPen.Width)
		{
			streamWriter.SetLineWidth(Width);
			flag = true;
		}
		if (saveState || LineJoin != currentPen.LineJoin)
		{
			streamWriter.SetLineJoin(LineJoin);
			flag = true;
		}
		if (saveState || LineCap != currentPen.LineCap)
		{
			streamWriter.SetLineCap(LineCap);
			flag = true;
		}
		if (saveState || MiterLimit != currentPen.MiterLimit)
		{
			float miterLimit = MiterLimit;
			if (miterLimit > 0f)
			{
				streamWriter.SetMiterLimit(miterLimit);
				flag = true;
			}
		}
		if (saveState || Color != currentPen.Color || Brush != currentPen.Brush || m_colorSpace != currentColorSpace)
		{
			PdfBrush brush = m_brush;
			if (brush != null)
			{
				PdfBrush pdfBrush = brush.Clone();
				SetStrokingToBrush(pdfBrush);
				if (pdfBrush is PdfGradientBrush pdfGradientBrush)
				{
					PdfTransformationMatrix matrix2 = pdfGradientBrush.Matrix;
					if (matrix2 != null)
					{
						matrix.Multiply(matrix2);
					}
					pdfGradientBrush.Matrix = matrix;
				}
				PdfBrush brush2 = currentPen?.Brush;
				flag |= pdfBrush.MonitorChanges(brush2, streamWriter, getResources, saveState, currentColorSpace);
			}
			else if (Colorspaces == null)
			{
				streamWriter.SetColorAndSpace(Color, currentColorSpace, forStroking: true);
				flag = true;
			}
			else if (Colorspaces is PdfIndexedColor)
			{
				streamWriter.SetColorAndSpace(Color, currentColorSpace, forStroking: true, check: true, iccbased: true, indexed: true);
				flag = true;
			}
			else
			{
				streamWriter.SetColorAndSpace(Color, currentColorSpace, forStroking: true, check: true, iccbased: true);
				flag = true;
			}
		}
		return flag;
	}

	internal float[] GetPattern()
	{
		float[] array = DashPattern.Clone() as float[];
		if (!isSkipPatternWidth)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (Width != 0f)
				{
					array[i] *= Width;
				}
			}
			isSkipPatternWidth = false;
		}
		return array;
	}

	private bool DashControl(PdfPen pen, bool saveState, PdfStreamWriter streamWriter)
	{
		saveState = pen == null || (saveState | ((DashOffset != pen.DashOffset) | (DashPattern != pen.DashPattern) | (DashStyle != pen.DashStyle) | (Width != pen.Width)));
		if (saveState)
		{
			float width = Width;
			float[] pattern = GetPattern();
			streamWriter.SetLineDashPattern(pattern, DashOffset * width);
		}
		return saveState;
	}

	private void CheckImmutability(string propertyName)
	{
		if (m_bImmutable)
		{
			throw new ArgumentException("The immutable object can't be changed", propertyName);
		}
	}

	private void CreateCompoundPen()
	{
		PdfTilingBrush pdfTilingBrush = new PdfTilingBrush(new SizeF(Width, Width));
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < CompoundArray.Length; i += 2)
		{
			PdfPen pdfPen = new PdfPen(Color);
			pdfPen.Width = (CompoundArray[i + 1] - CompoundArray[i]) * pdfTilingBrush.Size.Width;
			if (i != 0)
			{
				num += (CompoundArray[i] - CompoundArray[i - 1]) * pdfTilingBrush.Size.Width + num2;
			}
			pdfTilingBrush.Graphics.DrawLine(pdfPen, 0f, num, pdfTilingBrush.Size.Width, num);
			num2 = pdfPen.Width;
		}
		Brush = pdfTilingBrush;
	}
}
