using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class Pdf3DView : IPdfWrapper
{
	private Pdf3DBackground m_3dBackground;

	private Pdf3DCrossSectionCollection m_3dCrossSectionCollection;

	private float[] m_centretoWorldMatrix;

	private Pdf3DLighting m_3dLighting;

	private Pdf3DNodeCollection m_3dNodeCollection;

	private Pdf3DProjection m_3dProjection;

	private Pdf3DRendermode m_3dRendermode;

	private bool m_resetNodesState;

	private float m_centreOfOrbit;

	private string m_externalName;

	private string m_internalName;

	private string m_viewNodeName;

	private PdfDictionary m_dictionary = new PdfDictionary();

	public Pdf3DBackground Background
	{
		get
		{
			return m_3dBackground;
		}
		set
		{
			m_3dBackground = value;
		}
	}

	public float[] CameraToWorldMatrix
	{
		get
		{
			return m_centretoWorldMatrix;
		}
		set
		{
			m_centretoWorldMatrix = value;
			if (m_centretoWorldMatrix != null && m_centretoWorldMatrix.Length < 12)
			{
				throw new ArgumentOutOfRangeException("CameraToWorldMatrix.Length", "CameraToWorldMatrix array must have at least 12 elements.");
			}
		}
	}

	public float CenterOfOrbit
	{
		get
		{
			return m_centreOfOrbit;
		}
		set
		{
			m_centreOfOrbit = value;
		}
	}

	public Pdf3DCrossSectionCollection CrossSections => m_3dCrossSectionCollection;

	public string ExternalName
	{
		get
		{
			return m_externalName;
		}
		set
		{
			m_externalName = value;
		}
	}

	public string InternalName
	{
		get
		{
			return m_internalName;
		}
		set
		{
			m_internalName = value;
		}
	}

	public Pdf3DLighting LightingScheme
	{
		get
		{
			return m_3dLighting;
		}
		set
		{
			m_3dLighting = value;
		}
	}

	public Pdf3DNodeCollection Nodes => m_3dNodeCollection;

	public Pdf3DProjection Projection
	{
		get
		{
			return m_3dProjection;
		}
		set
		{
			m_3dProjection = value;
		}
	}

	public Pdf3DRendermode RenderMode
	{
		get
		{
			return m_3dRendermode;
		}
		set
		{
			m_3dRendermode = value;
		}
	}

	public bool ResetNodesState
	{
		get
		{
			return m_resetNodesState;
		}
		set
		{
			m_resetNodesState = value;
		}
	}

	public string ViewNodeName
	{
		get
		{
			return m_viewNodeName;
		}
		set
		{
			m_viewNodeName = value;
		}
	}

	internal PdfDictionary Dictionary => m_dictionary;

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	public Pdf3DView()
	{
		Initialize();
		m_3dNodeCollection = new Pdf3DNodeCollection();
		m_3dCrossSectionCollection = new Pdf3DCrossSectionCollection();
	}

	protected virtual void Initialize()
	{
		m_dictionary.BeginSave += Dictionary_BeginSave;
		Dictionary["Type"] = new PdfName("3DView");
	}

	private void Dictionary_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		Save();
	}

	protected virtual void Save()
	{
		if (m_externalName != null && m_externalName.Length > 0)
		{
			Dictionary["XN"] = new PdfString(m_externalName);
		}
		if (m_internalName != null && m_internalName.Length > 0)
		{
			Dictionary["IN"] = new PdfString(m_internalName);
		}
		if (m_viewNodeName != null && m_viewNodeName.Length > 0)
		{
			Dictionary["MS"] = new PdfName("U3D");
			Dictionary["U3DPath"] = new PdfName(m_internalName);
		}
		else
		{
			if (m_centretoWorldMatrix == null)
			{
				throw new ArgumentNullException("CameraToWorldMatrix", "Either ViewNodeName or CameraToWorldMatrix properties must be specified.");
			}
			Dictionary["MS"] = new PdfName("M");
			PdfArray pdfArray = new PdfArray();
			for (int i = 0; i < m_centretoWorldMatrix.Length; i++)
			{
				pdfArray.Insert(i, new PdfNumber(m_centretoWorldMatrix[i]));
			}
			Dictionary.SetProperty("C2W", new PdfArray(pdfArray));
		}
		if (m_centreOfOrbit > 0f)
		{
			Dictionary.SetProperty("CO", new PdfNumber(m_centreOfOrbit));
		}
		if (m_3dProjection != null)
		{
			Dictionary["P"] = new PdfReferenceHolder(m_3dProjection);
		}
		if (m_3dBackground != null)
		{
			Dictionary["BG"] = new PdfReferenceHolder(m_3dBackground);
		}
		if (m_3dRendermode != null)
		{
			Dictionary["RM"] = new PdfReferenceHolder(m_3dRendermode);
		}
		if (m_3dLighting != null)
		{
			Dictionary["LS"] = new PdfReferenceHolder(m_3dLighting);
		}
		if (m_3dCrossSectionCollection != null && m_3dCrossSectionCollection.Count > 0)
		{
			PdfArray pdfArray2 = new PdfArray();
			for (int j = 0; j < m_3dCrossSectionCollection.Count; j++)
			{
				pdfArray2.Insert(j, new PdfReferenceHolder(m_3dCrossSectionCollection[j]));
			}
			Dictionary.SetProperty("SA", new PdfArray(pdfArray2));
		}
		if (m_3dNodeCollection != null && m_3dNodeCollection.Count > 0)
		{
			PdfArray pdfArray3 = new PdfArray();
			for (int k = 0; k < m_3dNodeCollection.Count; k++)
			{
				pdfArray3.Insert(k, new PdfReferenceHolder(m_3dNodeCollection[k]));
			}
			Dictionary.SetProperty("NA", new PdfArray(pdfArray3));
		}
		Dictionary.SetProperty("NR", new PdfBoolean(m_resetNodesState));
	}
}
