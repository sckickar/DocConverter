using System;
using DocGen.Pdf.Functions;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Graphics;

public abstract class PdfGradientBrush : PdfBrush, IPdfWrapper
{
	private PdfColor m_background;

	private bool m_bStroking;

	private PdfDictionary m_patternDictionary;

	private PdfDictionary m_shading;

	private PdfTransformationMatrix m_matrix;

	private PdfExternalGraphicsState m_externalState;

	private PdfColorSpace m_colorSpace;

	private PdfFunction m_function;

	public PdfColor Background
	{
		get
		{
			return m_background;
		}
		set
		{
			m_background = value;
			PdfDictionary shading = Shading;
			if (value == PdfColor.Empty)
			{
				shading.Remove("Background");
			}
			else
			{
				shading["Background"] = value.ToArray(ColorSpace);
			}
		}
	}

	public bool AntiAlias
	{
		get
		{
			return (Shading["AntiAlias"] as PdfBoolean).Value;
		}
		set
		{
			PdfDictionary shading = Shading;
			if (!(shading["AntiAlias"] is PdfBoolean pdfBoolean))
			{
				PdfBoolean value2 = new PdfBoolean(value);
				shading["AntiAlias"] = value2;
			}
			else
			{
				pdfBoolean.Value = value;
			}
		}
	}

	internal PdfFunction Function
	{
		get
		{
			return m_function;
		}
		set
		{
			m_function = value;
			if (value != null)
			{
				Shading["Function"] = new PdfReferenceHolder(m_function);
			}
			else
			{
				Shading.Remove("Function");
			}
		}
	}

	internal PdfArray BBox
	{
		get
		{
			return Shading["BBox"] as PdfArray;
		}
		set
		{
			PdfDictionary shading = Shading;
			if (value == null)
			{
				shading.Remove("BBox");
			}
			else
			{
				shading["BBox"] = value;
			}
		}
	}

	internal PdfColorSpace ColorSpace
	{
		get
		{
			return m_colorSpace;
		}
		set
		{
			IPdfPrimitive pdfPrimitive = Shading["ColorSpace"];
			if (value == m_colorSpace && pdfPrimitive != null)
			{
				return;
			}
			m_colorSpace = value;
			string value2 = ColorSpaceToDeviceName(value);
			if (pdfPrimitive != null)
			{
				PdfName pdfName = pdfPrimitive as PdfName;
				if (pdfName != null)
				{
					pdfName.Value = value2;
				}
				else
				{
					Shading["ColorSpace"] = new PdfName(value2);
				}
			}
			else
			{
				Shading["ColorSpace"] = new PdfName(value2);
			}
		}
	}

	internal bool Stroking
	{
		get
		{
			return m_bStroking;
		}
		set
		{
			m_bStroking = value;
		}
	}

	internal PdfDictionary PatternDictionary
	{
		get
		{
			if (m_patternDictionary == null)
			{
				m_patternDictionary = new PdfDictionary();
			}
			return m_patternDictionary;
		}
	}

	internal PdfDictionary Shading
	{
		get
		{
			return m_shading;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Shading");
			}
			if (value != m_shading)
			{
				m_shading = value;
				PatternDictionary["Shading"] = new PdfReferenceHolder(m_shading);
			}
		}
	}

	internal PdfTransformationMatrix Matrix
	{
		get
		{
			return m_matrix;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Matrix");
			}
			if (value != m_matrix)
			{
				m_matrix = value.Clone();
				PdfArray value2 = new PdfArray(m_matrix.Matrix.Elements);
				m_patternDictionary["Matrix"] = value2;
			}
		}
	}

	internal PdfExternalGraphicsState ExternalState
	{
		get
		{
			return m_externalState;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("ExternalState");
			}
			if (value != m_externalState)
			{
				m_externalState = value;
				m_patternDictionary["ExtGState"] = ((IPdfWrapper)m_externalState).Element;
			}
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_patternDictionary;

	internal PdfGradientBrush(PdfDictionary shading)
	{
		if (shading == null)
		{
			throw new ArgumentNullException("shading");
		}
		m_patternDictionary = new PdfDictionary();
		m_patternDictionary["Type"] = new PdfName("Pattern");
		m_patternDictionary["PatternType"] = new PdfNumber(2);
		Shading = shading;
		ColorSpace = PdfColorSpace.RGB;
	}

	internal override bool MonitorChanges(PdfBrush brush, PdfStreamWriter streamWriter, PdfGraphics.GetResources getResources, bool saveChanges, PdfColorSpace currentColorSpace)
	{
		bool result = false;
		if (brush != this)
		{
			if (ColorSpace != currentColorSpace)
			{
				ColorSpace = currentColorSpace;
				ResetFunction();
			}
			streamWriter.SetColorSpace("Pattern", m_bStroking);
			PdfName name = getResources().GetName(this);
			streamWriter.SetColourWithPattern(null, name, m_bStroking);
			result = true;
		}
		return result;
	}

	internal override bool MonitorChanges(PdfBrush brush, PdfStreamWriter streamWriter, PdfGraphics.GetResources getResources, bool saveChanges, PdfColorSpace currentColorSpace, bool check)
	{
		bool result = false;
		if (brush != this)
		{
			if (ColorSpace != currentColorSpace)
			{
				ColorSpace = currentColorSpace;
				ResetFunction();
			}
			streamWriter.SetColorSpace("Pattern", m_bStroking);
			PdfName name = getResources().GetName(this);
			streamWriter.SetColourWithPattern(null, name, m_bStroking);
			result = true;
		}
		return result;
	}

	internal override bool MonitorChanges(PdfBrush brush, PdfStreamWriter streamWriter, PdfGraphics.GetResources getResources, bool saveChanges, PdfColorSpace currentColorSpace, bool check, bool iccbased)
	{
		bool result = false;
		if (brush != this)
		{
			if (ColorSpace != currentColorSpace)
			{
				ColorSpace = currentColorSpace;
				ResetFunction();
			}
			streamWriter.SetColorSpace("Pattern", m_bStroking);
			PdfName name = getResources().GetName(this);
			streamWriter.SetColourWithPattern(null, name, m_bStroking);
			result = true;
		}
		return result;
	}

	internal override bool MonitorChanges(PdfBrush brush, PdfStreamWriter streamWriter, PdfGraphics.GetResources getResources, bool saveChanges, PdfColorSpace currentColorSpace, bool check, bool iccbased, bool indexed)
	{
		bool result = false;
		if (brush != this)
		{
			if (ColorSpace != currentColorSpace)
			{
				ColorSpace = currentColorSpace;
				ResetFunction();
			}
			streamWriter.SetColorSpace("Pattern", m_bStroking);
			PdfName name = getResources().GetName(this);
			streamWriter.SetColourWithPattern(null, name, m_bStroking);
			result = true;
		}
		return result;
	}

	internal override void ResetChanges(PdfStreamWriter streamWriter)
	{
	}

	internal static string ColorSpaceToDeviceName(PdfColorSpace colorSpace)
	{
		return colorSpace switch
		{
			PdfColorSpace.RGB => "DeviceRGB", 
			PdfColorSpace.CMYK => "DeviceCMYK", 
			PdfColorSpace.GrayScale => "DeviceGray", 
			_ => throw new ArgumentException("Unsupported colour space: " + colorSpace, "colorSpace"), 
		};
	}

	internal void ResetPatternDictionary(PdfDictionary dictionary)
	{
		m_patternDictionary = dictionary;
	}

	internal abstract void ResetFunction();

	protected void CloneAntiAliasingValue(PdfGradientBrush brush)
	{
		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}
		if (Shading["AntiAlias"] is PdfBoolean pdfBoolean)
		{
			brush.Shading["AntiAlias"] = new PdfBoolean(pdfBoolean.Value);
		}
	}

	protected void CloneBackgroundValue(PdfGradientBrush brush)
	{
		PdfColor background = Background;
		if (!background.IsEmpty)
		{
			brush.Background = background;
		}
	}
}
