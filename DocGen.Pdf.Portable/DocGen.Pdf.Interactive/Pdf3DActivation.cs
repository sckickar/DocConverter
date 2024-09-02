using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class Pdf3DActivation : IPdfWrapper
{
	private Pdf3DActivationMode m_activationMode;

	private Pdf3DActivationState m_activationState;

	private Pdf3DDeactivationMode m_deactivationMode;

	private Pdf3DDeactivationState m_deactivationState;

	private bool m_showToolbar = true;

	private bool m_showUI;

	private PdfDictionary m_dictionary = new PdfDictionary();

	public Pdf3DActivationMode ActivationMode
	{
		get
		{
			return m_activationMode;
		}
		set
		{
			m_activationMode = value;
		}
	}

	public Pdf3DDeactivationMode DeactivationMode
	{
		get
		{
			return m_deactivationMode;
		}
		set
		{
			m_deactivationMode = value;
		}
	}

	public Pdf3DActivationState ActivationState
	{
		get
		{
			return m_activationState;
		}
		set
		{
			m_activationState = value;
		}
	}

	public Pdf3DDeactivationState DeactivationState
	{
		get
		{
			return m_deactivationState;
		}
		set
		{
			m_deactivationState = value;
		}
	}

	public bool ShowToolbar
	{
		get
		{
			return m_showToolbar;
		}
		set
		{
			m_showToolbar = value;
		}
	}

	public bool ShowUI
	{
		get
		{
			return m_showUI;
		}
		set
		{
			m_showUI = value;
		}
	}

	internal PdfDictionary Dictionary => m_dictionary;

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	public Pdf3DActivation()
	{
		Initialize();
	}

	protected virtual void Initialize()
	{
		m_dictionary.BeginSave += Dictionary_BeginSave;
	}

	private void Dictionary_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		Save();
	}

	protected virtual void Save()
	{
		if (m_activationMode == Pdf3DActivationMode.PageVisible)
		{
			Dictionary.SetProperty("A", new PdfName("PV"));
		}
		else if (m_activationMode == Pdf3DActivationMode.PageOpen)
		{
			Dictionary.SetProperty("A", new PdfName("PO"));
		}
		else if (m_activationMode == Pdf3DActivationMode.ExplicitActivation)
		{
			Dictionary.SetProperty("A", new PdfName("XA"));
		}
		if (m_activationState == Pdf3DActivationState.Instantiated)
		{
			Dictionary.SetProperty("AIS", new PdfName("I"));
		}
		else
		{
			Dictionary.SetProperty("AIS", new PdfName("L"));
		}
		if (m_deactivationMode == Pdf3DDeactivationMode.PageClose)
		{
			Dictionary.SetProperty("D", new PdfName("PC"));
		}
		else if (m_deactivationMode == Pdf3DDeactivationMode.PageInvisible)
		{
			Dictionary.SetProperty("D", new PdfName("PI"));
		}
		else if (m_deactivationMode == Pdf3DDeactivationMode.ExplicitDeactivation)
		{
			Dictionary.SetProperty("D", new PdfName("XD"));
		}
		if (m_deactivationState == Pdf3DDeactivationState.Uninstantiated)
		{
			Dictionary.SetProperty("DIS", new PdfName("U"));
		}
		else if (m_deactivationState == Pdf3DDeactivationState.Instantiated)
		{
			Dictionary.SetProperty("DIS", new PdfName("I"));
		}
		else if (m_deactivationState == Pdf3DDeactivationState.Live)
		{
			Dictionary.SetProperty("DIS", new PdfName("L"));
		}
		Dictionary.SetProperty("TB", new PdfBoolean(m_showToolbar));
		Dictionary.SetProperty("NP", new PdfBoolean(m_showUI));
	}
}
