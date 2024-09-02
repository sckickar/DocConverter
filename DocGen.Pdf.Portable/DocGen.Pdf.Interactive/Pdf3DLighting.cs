using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class Pdf3DLighting : IPdfWrapper
{
	private Pdf3DLightingStyle m_lightingStyle;

	private PdfDictionary m_dictionary = new PdfDictionary();

	public Pdf3DLightingStyle Style
	{
		get
		{
			return m_lightingStyle;
		}
		set
		{
			m_lightingStyle = value;
		}
	}

	internal PdfDictionary Dictionary => m_dictionary;

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	public Pdf3DLighting()
	{
		Initialize();
	}

	public Pdf3DLighting(Pdf3DLightingStyle style)
		: this()
	{
		m_lightingStyle = style;
	}

	protected virtual void Initialize()
	{
		m_dictionary.BeginSave += Dictionary_BeginSave;
		m_dictionary.SetProperty("Type", new PdfName("3DLightingScheme"));
	}

	private void Dictionary_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		Save();
	}

	protected virtual void Save()
	{
		Dictionary.SetProperty("Subtype", new PdfName(m_lightingStyle));
	}
}
