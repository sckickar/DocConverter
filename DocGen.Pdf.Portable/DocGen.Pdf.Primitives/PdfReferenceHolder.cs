using System;
using DocGen.Pdf.IO;

namespace DocGen.Pdf.Primitives;

internal class PdfReferenceHolder : IPdfPrimitive
{
	private IPdfPrimitive m_object;

	private PdfCrossTable m_crossTable;

	private PdfReference m_reference;

	private int m_objectIndex = -1;

	private ObjectStatus m_status;

	private bool m_isSaving;

	private int m_index;

	private int m_position = -1;

	private static object m_lock = new object();

	internal IPdfPrimitive Object
	{
		get
		{
			if (m_reference != null || m_object == null)
			{
				m_object = ObtainObject();
			}
			return m_object;
		}
	}

	internal int Index
	{
		get
		{
			PdfMainObjectCollection pdfObjects = m_crossTable.PdfObjects;
			m_objectIndex = pdfObjects.GetObjectIndex(m_reference);
			if (m_objectIndex < 0)
			{
				lock (m_lock)
				{
					m_crossTable.GetObject(m_reference);
					m_objectIndex = pdfObjects.Count - 1;
				}
			}
			return m_objectIndex;
		}
	}

	public PdfReference Reference => m_reference;

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

	public PdfReferenceHolder(IPdfWrapper wrapper)
		: this(wrapper.Element)
	{
	}

	public PdfReferenceHolder(IPdfPrimitive obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		m_object = obj;
	}

	internal PdfReferenceHolder(PdfReference reference, PdfCrossTable crossTable)
	{
		if (crossTable == null)
		{
			throw new ArgumentNullException("crossTable");
		}
		if (reference == null)
		{
			throw new ArgumentNullException("reference");
		}
		m_crossTable = crossTable;
		m_reference = reference;
	}

	public void Save(IPdfWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		long position = writer.Position;
		PdfCrossTable crossTable = writer.Document.CrossTable;
		if (Object != null && crossTable.Document is PdfDocument)
		{
			Object.IsSaving = true;
		}
		PdfReference pdfReference = null;
		pdfReference = ((!writer.Document.FileStructure.IncrementalUpdate || !writer.Document.m_isStreamCopied) ? crossTable.GetReference(Object) : ((!(m_reference == null)) ? m_reference : crossTable.GetReference(Object)));
		if (writer.Position != position)
		{
			writer.Position = position;
		}
		pdfReference.Save(writer);
	}

	public IPdfPrimitive Clone(PdfCrossTable crossTable)
	{
		IPdfPrimitive pdfPrimitive = null;
		_ = string.Empty;
		PdfReference pdfReference = null;
		if (Reference != null && m_crossTable != null && m_crossTable.PageCorrespondance.ContainsKey(Reference))
		{
			return new PdfReferenceHolder(m_crossTable.PageCorrespondance[Reference] as PdfReference, crossTable);
		}
		if (crossTable.isTemplateMerging && m_crossTable != null)
		{
			m_crossTable.isTemplateMerging = true;
		}
		if (m_crossTable != null && m_crossTable.PageCorrespondance.ContainsKey(Object))
		{
			if (!(m_crossTable.PageCorrespondance[Object] is PdfPageBase { Dictionary: not null } pdfPageBase))
			{
				return new PdfNull();
			}
			pdfPrimitive = pdfPageBase.Dictionary;
		}
		else
		{
			if (Object is PdfNumber)
			{
				return new PdfNumber((Object as PdfNumber).FloatValue);
			}
			if (Object is PdfDictionary)
			{
				PdfName key = new PdfName("Type");
				PdfDictionary pdfDictionary = Object as PdfDictionary;
				if (pdfDictionary.ContainsKey(key))
				{
					PdfName pdfName = pdfDictionary[key] as PdfName;
					if (pdfName != null && pdfName.Value == "Page")
					{
						return new PdfNull();
					}
				}
			}
			if (Object is PdfName)
			{
				return new PdfName((Object as PdfName).Value);
			}
			if (crossTable.PrevReference != null && crossTable.PrevReference.Contains(Reference))
			{
				IPdfPrimitive pdfPrimitive2 = null;
				pdfPrimitive2 = ((crossTable.Document == null || crossTable.Document.EnableMemoryOptimization) ? m_crossTable.GetObject(Reference).ClonedObject : m_crossTable.GetObject(Reference));
				if (pdfPrimitive2 != null)
				{
					pdfReference = crossTable.GetReference(pdfPrimitive2);
					return new PdfReferenceHolder(pdfReference, crossTable);
				}
				return new PdfNull();
			}
			if (Reference != null)
			{
				crossTable.PrevReference.Add(Reference);
			}
			pdfPrimitive = ((Object is PdfCatalog) ? crossTable.Document.Catalog : Object.Clone(crossTable));
		}
		pdfReference = crossTable.GetReference(pdfPrimitive);
		PdfReferenceHolder result = new PdfReferenceHolder(pdfReference, crossTable);
		if (Reference != null && pdfReference != null && crossTable.IsTagged)
		{
			crossTable.PrevCloneReference[pdfReference.ObjNum] = Reference.ObjNum;
		}
		return result;
	}

	public override bool Equals(object obj)
	{
		PdfReferenceHolder pdfReferenceHolder = obj as PdfReferenceHolder;
		bool flag = pdfReferenceHolder != null;
		if (flag)
		{
			flag = ((!(m_reference != null) || !(pdfReferenceHolder.m_reference != null)) ? (flag & (pdfReferenceHolder.Object == Object)) : (flag & (pdfReferenceHolder.m_reference == m_reference)));
		}
		return flag;
	}

	public override int GetHashCode()
	{
		return Object.GetHashCode();
	}

	public static bool operator ==(PdfReferenceHolder rh1, PdfReferenceHolder rh2)
	{
		if ((object)rh1 == null || (object)rh2 == null)
		{
			return (object)rh1 == rh2;
		}
		return rh1.Equals(rh2);
	}

	public static bool operator !=(PdfReferenceHolder rh1, PdfReferenceHolder rh2)
	{
		return !(rh1 == rh2);
	}

	private IPdfPrimitive ObtainObject()
	{
		IPdfPrimitive result = null;
		if (m_reference != null && m_crossTable.PdfObjects != null)
		{
			if (Index >= 0)
			{
				result = m_crossTable.PdfObjects.GetObject(m_reference);
			}
		}
		else if (m_object != null)
		{
			result = m_object;
		}
		return result;
	}
}
