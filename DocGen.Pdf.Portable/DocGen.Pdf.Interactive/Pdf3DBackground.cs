using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class Pdf3DBackground : IPdfWrapper
{
	private const float MaxColourChannelValue = 255f;

	private PdfColor m_backgroundColor;

	private bool m_applyEntire;

	private PdfDictionary m_dictionary = new PdfDictionary();

	public PdfColor Color
	{
		get
		{
			return m_backgroundColor;
		}
		set
		{
			m_backgroundColor = value;
		}
	}

	public bool ApplyToEntireAnnotation
	{
		get
		{
			return m_applyEntire;
		}
		set
		{
			m_applyEntire = value;
		}
	}

	internal PdfDictionary Dictionary => m_dictionary;

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	public Pdf3DBackground()
	{
		Initialize();
	}

	public Pdf3DBackground(PdfColor color)
		: this()
	{
		m_backgroundColor = color;
	}

	protected virtual void Initialize()
	{
		m_dictionary.BeginSave += Dictionary_BeginSave;
		m_dictionary.SetProperty("Type", new PdfName("3DBG"));
	}

	private void Dictionary_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		Save();
	}

	protected virtual void Save()
	{
		Dictionary["Subtype"] = new PdfName("SC");
		Dictionary.SetProperty("CS", new PdfName("DeviceRGB"));
		Dictionary.SetProperty("EA", new PdfBoolean(m_applyEntire));
		_ = m_backgroundColor;
		PdfArray pdfArray = new PdfArray();
		pdfArray.Insert(0, new PdfNumber((float)(int)m_backgroundColor.R / 255f));
		pdfArray.Insert(1, new PdfNumber((float)(int)m_backgroundColor.G / 255f));
		pdfArray.Insert(2, new PdfNumber((float)(int)m_backgroundColor.B / 255f));
		Dictionary["C"] = new PdfArray(pdfArray);
	}
}
