using System;
using DocGen.Pdf.ColorSpace;
using DocGen.Pdf.IO;

namespace DocGen.Pdf.Graphics;

public sealed class PdfSolidBrush : PdfBrush
{
	private PdfColor m_color;

	private PdfColorSpace m_colorSpace;

	private bool m_bImmutable;

	private PdfExtendedColor m_colorspaces;

	public PdfColor Color
	{
		get
		{
			return m_color;
		}
		set
		{
			if (m_bImmutable)
			{
				throw new ArgumentException("Can't change immutable object.", "Color");
			}
			m_color = value;
		}
	}

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

	public PdfSolidBrush(PdfColor color)
	{
		m_color = color;
	}

	public PdfSolidBrush(PdfExtendedColor color)
	{
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

	internal PdfSolidBrush(PdfColor color, bool immutable)
		: this(color)
	{
		m_bImmutable = immutable;
	}

	private PdfSolidBrush()
		: this(new PdfColor(0, 0, 0))
	{
	}

	internal override bool MonitorChanges(PdfBrush brush, PdfStreamWriter streamWriter, PdfGraphics.GetResources getResources, bool saveChanges, PdfColorSpace currentColorSpace)
	{
		if (streamWriter == null)
		{
			throw new ArgumentNullException("streamWriter");
		}
		if (getResources == null)
		{
			throw new ArgumentNullException("getResources");
		}
		bool result = false;
		if (brush == null)
		{
			result = true;
			streamWriter.SetColorAndSpace(m_color, currentColorSpace, forStroking: false);
		}
		else if (brush != this)
		{
			if (brush is PdfSolidBrush pdfSolidBrush)
			{
				if (pdfSolidBrush.Color != Color || pdfSolidBrush.m_colorSpace != currentColorSpace)
				{
					result = true;
					streamWriter.SetColorAndSpace(m_color, currentColorSpace, forStroking: false);
				}
				else if (pdfSolidBrush.m_colorSpace == currentColorSpace && currentColorSpace == PdfColorSpace.RGB)
				{
					result = true;
					streamWriter.SetColorAndSpace(m_color, currentColorSpace, forStroking: false);
				}
			}
			else
			{
				brush.ResetChanges(streamWriter);
				streamWriter.SetColorAndSpace(m_color, currentColorSpace, forStroking: false);
				result = true;
			}
		}
		return result;
	}

	internal override bool MonitorChanges(PdfBrush brush, PdfStreamWriter streamWriter, PdfGraphics.GetResources getResources, bool saveChanges, PdfColorSpace currentColorSpace, bool check)
	{
		if (streamWriter == null)
		{
			throw new ArgumentNullException("streamWriter");
		}
		if (getResources == null)
		{
			throw new ArgumentNullException("getResources");
		}
		bool result = false;
		if (brush == null)
		{
			result = true;
			streamWriter.SetColorAndSpace(m_color, currentColorSpace, forStroking: false, check: false);
		}
		else if (brush != this)
		{
			if (brush is PdfSolidBrush pdfSolidBrush)
			{
				if (pdfSolidBrush.Color != Color || pdfSolidBrush.m_colorSpace != currentColorSpace)
				{
					result = true;
					streamWriter.SetColorAndSpace(m_color, currentColorSpace, forStroking: false, check: false);
				}
			}
			else
			{
				brush.ResetChanges(streamWriter);
				streamWriter.SetColorAndSpace(m_color, currentColorSpace, forStroking: false);
				result = true;
			}
		}
		return result;
	}

	internal override bool MonitorChanges(PdfBrush brush, PdfStreamWriter streamWriter, PdfGraphics.GetResources getResources, bool saveChanges, PdfColorSpace currentColorSpace, bool check, bool iccbased)
	{
		if (streamWriter == null)
		{
			throw new ArgumentNullException("streamWriter");
		}
		if (getResources == null)
		{
			throw new ArgumentNullException("getResources");
		}
		bool result = false;
		if (brush == null)
		{
			result = true;
			streamWriter.SetColorAndSpace(m_color, currentColorSpace, forStroking: false, check: false, iccbased: false);
		}
		else if (brush != this)
		{
			if (brush is PdfSolidBrush pdfSolidBrush)
			{
				if (pdfSolidBrush.Color != Color || pdfSolidBrush.m_colorSpace != currentColorSpace)
				{
					result = true;
					streamWriter.SetColorAndSpace(m_color, currentColorSpace, forStroking: false, check: false, iccbased: false);
				}
				else if (pdfSolidBrush.m_colorSpace == currentColorSpace && currentColorSpace == PdfColorSpace.RGB)
				{
					result = true;
					streamWriter.SetColorAndSpace(m_color, currentColorSpace, forStroking: false, check: false, iccbased: false);
				}
			}
			else
			{
				brush.ResetChanges(streamWriter);
				streamWriter.SetColorAndSpace(m_color, currentColorSpace, forStroking: false);
				result = true;
			}
		}
		return result;
	}

	internal override bool MonitorChanges(PdfBrush brush, PdfStreamWriter streamWriter, PdfGraphics.GetResources getResources, bool saveChanges, PdfColorSpace currentColorSpace, bool check, bool iccbased, bool indexed)
	{
		if (streamWriter == null)
		{
			throw new ArgumentNullException("streamWriter");
		}
		if (getResources == null)
		{
			throw new ArgumentNullException("getResources");
		}
		bool result = false;
		if (brush == null)
		{
			result = true;
			streamWriter.SetColorAndSpace(m_color, currentColorSpace, forStroking: false, check: false, iccbased: false, indexed: false);
		}
		else if (brush != this)
		{
			if (brush is PdfSolidBrush pdfSolidBrush)
			{
				if (pdfSolidBrush.Color != Color || pdfSolidBrush.m_colorSpace != currentColorSpace)
				{
					result = true;
					streamWriter.SetColorAndSpace(m_color, currentColorSpace, forStroking: false, check: false, iccbased: false, indexed: false);
				}
			}
			else
			{
				brush.ResetChanges(streamWriter);
				streamWriter.SetColorAndSpace(m_color, currentColorSpace, forStroking: false);
				result = true;
			}
		}
		return result;
	}

	internal override void ResetChanges(PdfStreamWriter streamWriter)
	{
		streamWriter.SetColorAndSpace(new PdfColor(0, 0, 0), PdfColorSpace.RGB, forStroking: false);
	}

	public override PdfBrush Clone()
	{
		return MemberwiseClone() as PdfSolidBrush;
	}
}
