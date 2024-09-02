using DocGen.Drawing;
using DocGen.Pdf.IO;

namespace DocGen.Pdf;

public class PdfArtifact : PdfTag
{
	private RectangleF m_boundingBox;

	private PdfArtifactType m_artifactType;

	private PdfArtifactSubType m_subType;

	private PdfAttached m_attached;

	public RectangleF BoundingBox
	{
		get
		{
			return m_boundingBox;
		}
		set
		{
			m_boundingBox = value;
		}
	}

	public PdfArtifactType ArtifactType
	{
		get
		{
			return m_artifactType;
		}
		set
		{
			m_artifactType = value;
		}
	}

	public PdfArtifactSubType SubType
	{
		get
		{
			return m_subType;
		}
		set
		{
			m_subType = value;
		}
	}

	public PdfAttached Attached
	{
		get
		{
			return m_attached;
		}
		set
		{
			m_attached = value;
		}
	}

	public PdfArtifact()
	{
		m_boundingBox = default(RectangleF);
		m_artifactType = PdfArtifactType.None;
		m_subType = PdfArtifactSubType.None;
		if (PdfCatalog.StructTreeRoot == null)
		{
			PdfCatalog.m_structTreeRoot = new PdfStructTreeRoot();
		}
	}

	public PdfArtifact(PdfArtifactType type)
		: this()
	{
		m_artifactType = type;
	}

	public PdfArtifact(PdfArtifactType type, PdfAttached attached)
		: this()
	{
		m_artifactType = type;
		m_attached = attached;
	}

	public PdfArtifact(PdfArtifactType type, PdfAttached attached, PdfArtifactSubType subType)
		: this()
	{
		m_artifactType = type;
		m_attached = attached;
		m_subType = subType;
	}

	public PdfArtifact(PdfArtifactType type, RectangleF bBox, PdfAttached attached, PdfArtifactSubType subType)
		: this()
	{
		m_artifactType = type;
		m_boundingBox = bBox;
		m_attached = attached;
		m_subType = subType;
	}
}
