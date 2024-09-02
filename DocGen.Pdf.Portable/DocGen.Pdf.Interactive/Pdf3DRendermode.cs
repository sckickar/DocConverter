using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class Pdf3DRendermode : IPdfWrapper
{
	private Pdf3DRenderStyle m_style;

	private PdfColor m_faceColor;

	private PdfColor m_auxilaryColor;

	private float m_opacity;

	private float m_creaseValue;

	private PdfDictionary m_dictionary = new PdfDictionary();

	public Pdf3DRenderStyle Style
	{
		get
		{
			return m_style;
		}
		set
		{
			m_style = value;
		}
	}

	public PdfColor AuxilaryColor
	{
		get
		{
			return m_auxilaryColor;
		}
		set
		{
			m_auxilaryColor = value;
		}
	}

	public PdfColor FaceColor
	{
		get
		{
			return m_faceColor;
		}
		set
		{
			m_faceColor = value;
		}
	}

	public float CreaseValue
	{
		get
		{
			return m_creaseValue;
		}
		set
		{
			m_creaseValue = value;
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

	internal PdfDictionary Dictionary => m_dictionary;

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	public Pdf3DRendermode()
	{
		Initialize();
	}

	public Pdf3DRendermode(Pdf3DRenderStyle style)
		: this()
	{
		m_style = style;
	}

	protected virtual void Initialize()
	{
		m_dictionary.BeginSave += Dictionary_BeginSave;
		m_dictionary.SetProperty("Type", new PdfName("3DRenderMode"));
	}

	private void Dictionary_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		Save();
	}

	protected virtual void Save()
	{
		Dictionary["Subtype"] = new PdfName(m_style);
		_ = m_auxilaryColor;
		PdfArray pdfArray = new PdfArray();
		pdfArray.Insert(0, new PdfName("DeviceRGB"));
		pdfArray.Insert(1, new PdfNumber((float)(int)m_auxilaryColor.R / 255f));
		pdfArray.Insert(2, new PdfNumber((float)(int)m_auxilaryColor.G / 255f));
		pdfArray.Insert(3, new PdfNumber((float)(int)m_auxilaryColor.B / 255f));
		if (!m_auxilaryColor.IsEmpty)
		{
			Dictionary["AC"] = new PdfArray(pdfArray);
		}
		_ = m_faceColor;
		PdfArray pdfArray2 = new PdfArray();
		pdfArray2.Insert(0, new PdfName("DeviceRGB"));
		pdfArray2.Insert(1, new PdfNumber((float)(int)m_faceColor.R / 255f));
		pdfArray2.Insert(2, new PdfNumber((float)(int)m_faceColor.G / 255f));
		pdfArray2.Insert(3, new PdfNumber((float)(int)m_faceColor.B / 255f));
		if (!m_faceColor.IsEmpty)
		{
			Dictionary["FC"] = new PdfArray(pdfArray2);
		}
		if (m_opacity != 0f)
		{
			Dictionary.SetProperty("O", new PdfNumber(m_opacity));
		}
		if (m_creaseValue != 0f)
		{
			Dictionary.SetProperty("CV", new PdfNumber(m_creaseValue));
		}
	}
}
