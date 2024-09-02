using DocGen.Pdf.IO;

namespace DocGen.Pdf.Primitives;

internal class PdfNull : IPdfPrimitive
{
	private ObjectStatus m_status;

	private bool m_isSaving;

	private int m_index;

	private int m_position = -1;

	public ObjectStatus Status
	{
		get
		{
			return m_status;
		}
		set
		{
			m_status = value;
		}
	}

	public bool IsSaving
	{
		get
		{
			return m_isSaving;
		}
		set
		{
			m_isSaving = value;
		}
	}

	public int ObjectCollectionIndex
	{
		get
		{
			return m_index;
		}
		set
		{
			m_index = value;
		}
	}

	public int Position
	{
		get
		{
			return m_position;
		}
		set
		{
			m_position = value;
		}
	}

	public IPdfPrimitive ClonedObject => null;

	public void Save(IPdfWriter writer)
	{
		writer.Write("null");
	}

	public IPdfPrimitive Clone(PdfCrossTable crossTable)
	{
		return new PdfNull();
	}
}
