using System;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class Pdf3DCrossSection : IPdfWrapper
{
	private float[] m_center;

	private PdfColor m_color;

	private PdfColor m_intersectionColor;

	private bool m_intersectionIsVisible;

	private object[] m_orientation;

	private float m_opacity;

	private PdfDictionary m_dictionary = new PdfDictionary();

	public float[] Center
	{
		get
		{
			return m_center;
		}
		set
		{
			m_center = value;
			if (m_center == null || m_center.Length < 3)
			{
				throw new ArgumentOutOfRangeException("Center.Length", "Center Array must have atleast 3 elements.");
			}
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
			m_color = value;
		}
	}

	public PdfColor IntersectionColor
	{
		get
		{
			return m_intersectionColor;
		}
		set
		{
			m_intersectionColor = value;
		}
	}

	public bool IntersectionIsVisible
	{
		get
		{
			return m_intersectionIsVisible;
		}
		set
		{
			m_intersectionIsVisible = value;
		}
	}

	public float Opacity
	{
		get
		{
			return m_opacity;
		}
		set
		{
			m_opacity = value;
		}
	}

	public object[] Orientation
	{
		get
		{
			return m_orientation;
		}
		set
		{
			m_orientation = value;
			if (m_orientation == null || m_orientation.Length < 3)
			{
				throw new ArgumentOutOfRangeException("Orientation.Length", "Orientation Array must have atleast 3 elements.");
			}
		}
	}

	internal PdfDictionary Dictionary => m_dictionary;

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	public Pdf3DCrossSection()
	{
		Initialize();
	}

	protected virtual void Initialize()
	{
		m_dictionary.BeginSave += Dictionary_BeginSave;
		m_dictionary.SetProperty("Type", new PdfName("3DCrossSection"));
	}

	private void Dictionary_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		Save();
	}

	protected virtual void Save()
	{
		if (m_center != null)
		{
			Dictionary.SetProperty("C", new PdfArray(m_center));
		}
		if (m_orientation != null)
		{
			PdfArray pdfArray = new PdfArray();
			if (m_orientation[0] == null)
			{
				pdfArray.Insert(0, new PdfName("null"));
			}
			else
			{
				pdfArray.Insert(0, new PdfName((string)m_orientation[0]));
			}
			if (m_orientation[1] == null)
			{
				pdfArray.Insert(1, new PdfName("null"));
			}
			else
			{
				pdfArray.Insert(1, new PdfName((string)m_orientation[1]));
			}
			if (m_orientation[2] == null)
			{
				pdfArray.Insert(2, new PdfName("null"));
			}
			else
			{
				pdfArray.Insert(2, new PdfName((string)m_orientation[2]));
			}
			Dictionary["O"] = new PdfArray(pdfArray);
		}
		Dictionary.SetProperty("PO", new PdfNumber(m_opacity));
		if (m_color != PdfColor.Empty)
		{
			float value = (float)(int)m_color.R / 255f;
			float value2 = (float)(int)m_color.G / 255f;
			float value3 = (float)(int)m_color.B / 255f;
			PdfArray pdfArray2 = new PdfArray();
			pdfArray2.Insert(0, new PdfName("DeviceRGB"));
			pdfArray2.Insert(1, new PdfNumber(value));
			pdfArray2.Insert(2, new PdfNumber(value2));
			pdfArray2.Insert(3, new PdfNumber(value3));
			Dictionary["PC"] = new PdfArray(pdfArray2);
		}
		if (m_intersectionColor != PdfColor.Empty)
		{
			float value4 = (float)(int)m_intersectionColor.R / 255f;
			float value5 = (float)(int)m_intersectionColor.G / 255f;
			float value6 = (float)(int)m_intersectionColor.B / 255f;
			PdfArray pdfArray3 = new PdfArray();
			pdfArray3.Insert(0, new PdfName("DeviceRGB"));
			pdfArray3.Insert(1, new PdfNumber(value4));
			pdfArray3.Insert(2, new PdfNumber(value5));
			pdfArray3.Insert(3, new PdfNumber(value6));
			Dictionary["IC"] = new PdfArray(pdfArray3);
		}
		Dictionary.SetProperty("IV", new PdfBoolean(m_intersectionIsVisible));
	}
}
