using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class Pdf3DAnimation : IPdfWrapper
{
	private PDF3DAnimationType m_type;

	private int m_playCount;

	private float m_timeMultiplier;

	private PdfDictionary m_dictionary = new PdfDictionary();

	public PDF3DAnimationType Type
	{
		get
		{
			return m_type;
		}
		set
		{
			m_type = value;
		}
	}

	public int PlayCount
	{
		get
		{
			return m_playCount;
		}
		set
		{
			m_playCount = value;
		}
	}

	public float TimeMultiplier
	{
		get
		{
			return m_timeMultiplier;
		}
		set
		{
			m_timeMultiplier = value;
		}
	}

	internal PdfDictionary Dictionary => m_dictionary;

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	public Pdf3DAnimation()
	{
		Initialize();
	}

	public Pdf3DAnimation(PDF3DAnimationType type)
		: this()
	{
		m_type = type;
	}

	protected virtual void Initialize()
	{
		m_dictionary.BeginSave += Dictionary_BeginSave;
		m_dictionary.SetProperty("Type", new PdfName("3DAnimationStyle"));
	}

	private void Dictionary_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		Save();
	}

	protected virtual void Save()
	{
		Dictionary["Subtype"] = new PdfName(m_type);
		Dictionary.SetProperty("PC", new PdfNumber(m_playCount));
		Dictionary.SetProperty("TM", new PdfNumber(m_timeMultiplier));
	}
}
