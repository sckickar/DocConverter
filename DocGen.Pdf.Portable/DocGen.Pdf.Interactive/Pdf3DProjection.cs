using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class Pdf3DProjection : IPdfWrapper
{
	private Pdf3DProjectionType m_Type;

	private Pdf3DProjectionClipStyle m_ClipStyle;

	private Pdf3DProjectionOrthoScaleMode m_OrthoScalemode;

	private float m_farClipDistance;

	private float m_fieldOfView;

	private float m_nearClipDistance;

	private float m_scaling;

	private PdfDictionary m_dictionary = new PdfDictionary();

	public Pdf3DProjectionType ProjectionType
	{
		get
		{
			return m_Type;
		}
		set
		{
			m_Type = value;
		}
	}

	public Pdf3DProjectionClipStyle ClipStyle
	{
		get
		{
			return m_ClipStyle;
		}
		set
		{
			m_ClipStyle = value;
		}
	}

	public Pdf3DProjectionOrthoScaleMode OrthoScaleMode
	{
		get
		{
			return m_OrthoScalemode;
		}
		set
		{
			m_OrthoScalemode = value;
		}
	}

	public float FarClipDistance
	{
		get
		{
			return m_farClipDistance;
		}
		set
		{
			m_farClipDistance = value;
		}
	}

	public float FieldOfView
	{
		get
		{
			return m_fieldOfView;
		}
		set
		{
			m_fieldOfView = value;
		}
	}

	public float NearClipDistance
	{
		get
		{
			return m_nearClipDistance;
		}
		set
		{
			m_nearClipDistance = value;
		}
	}

	public float Scaling
	{
		get
		{
			return m_scaling;
		}
		set
		{
			m_scaling = value;
		}
	}

	internal PdfDictionary Dictionary => m_dictionary;

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	public Pdf3DProjection()
	{
		Initialize();
	}

	public Pdf3DProjection(Pdf3DProjectionType type)
		: this()
	{
		m_Type = type;
	}

	protected virtual void Initialize()
	{
		m_dictionary.BeginSave += Dictionary_BeginSave;
		m_dictionary.SetProperty("Subtype", new PdfName("P"));
	}

	private void Dictionary_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		Save();
	}

	protected virtual void Save()
	{
		if (ClipStyle == Pdf3DProjectionClipStyle.ExplicitNearFar)
		{
			Dictionary.SetProperty("CS", new PdfName("XNF"));
		}
		else
		{
			Dictionary.SetProperty("CS", new PdfName("ANF"));
		}
		if (ClipStyle == Pdf3DProjectionClipStyle.ExplicitNearFar && m_farClipDistance >= 0f)
		{
			Dictionary.SetProperty("F", new PdfNumber(m_farClipDistance));
		}
		if (m_Type == Pdf3DProjectionType.Perspective && m_nearClipDistance >= 0f)
		{
			Dictionary.SetProperty("N", new PdfNumber(m_nearClipDistance));
		}
		else if (m_Type == Pdf3DProjectionType.Orthographic && m_ClipStyle == Pdf3DProjectionClipStyle.ExplicitNearFar && m_nearClipDistance >= 0f)
		{
			Dictionary.SetProperty("N", new PdfNumber(m_nearClipDistance));
		}
		if (m_Type == Pdf3DProjectionType.Perspective)
		{
			Dictionary.SetProperty("FOV", new PdfNumber(m_fieldOfView));
		}
		if (m_scaling > 0f)
		{
			if (m_Type == Pdf3DProjectionType.Perspective)
			{
				Dictionary.SetProperty("PS", new PdfNumber(m_scaling));
			}
			if (m_Type == Pdf3DProjectionType.Orthographic)
			{
				Dictionary.SetProperty("OS", new PdfNumber(m_scaling));
			}
		}
		else if (m_Type == Pdf3DProjectionType.Perspective)
		{
			if (m_OrthoScalemode == Pdf3DProjectionOrthoScaleMode.Absolute)
			{
				Dictionary.SetProperty("PS", new PdfName("Absolute"));
			}
			else if (m_OrthoScalemode == Pdf3DProjectionOrthoScaleMode.Height)
			{
				Dictionary.SetProperty("PS", new PdfName("H"));
			}
			else if (m_OrthoScalemode == Pdf3DProjectionOrthoScaleMode.Max)
			{
				Dictionary.SetProperty("PS", new PdfName("Max"));
			}
			else if (m_OrthoScalemode == Pdf3DProjectionOrthoScaleMode.Min)
			{
				Dictionary.SetProperty("PS", new PdfName("Min"));
			}
			else if (m_OrthoScalemode == Pdf3DProjectionOrthoScaleMode.Width)
			{
				Dictionary.SetProperty("PS", new PdfName("W"));
			}
		}
		if (m_Type == Pdf3DProjectionType.Orthographic)
		{
			if (m_OrthoScalemode == Pdf3DProjectionOrthoScaleMode.Absolute)
			{
				Dictionary.SetProperty("OB", new PdfName("Absolute"));
			}
			else if (m_OrthoScalemode == Pdf3DProjectionOrthoScaleMode.Height)
			{
				Dictionary.SetProperty("OB", new PdfName("H"));
			}
			else if (m_OrthoScalemode == Pdf3DProjectionOrthoScaleMode.Max)
			{
				Dictionary.SetProperty("OB", new PdfName("Max"));
			}
			else if (m_OrthoScalemode == Pdf3DProjectionOrthoScaleMode.Min)
			{
				Dictionary.SetProperty("OB", new PdfName("Min"));
			}
			else if (m_OrthoScalemode == Pdf3DProjectionOrthoScaleMode.Width)
			{
				Dictionary.SetProperty("OB", new PdfName("W"));
			}
		}
	}
}
