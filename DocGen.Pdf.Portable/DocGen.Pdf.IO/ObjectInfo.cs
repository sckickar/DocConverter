using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.IO;

internal class ObjectInfo
{
	private bool m_bModified;

	private IPdfPrimitive m_object;

	private PdfReference m_reference;

	internal bool Modified
	{
		get
		{
			bool flag = m_bModified;
			if (Object is IPdfChangable pdfChangable)
			{
				flag |= pdfChangable.Changed;
			}
			return flag;
		}
	}

	internal PdfReference Reference => m_reference;

	internal IPdfPrimitive Object
	{
		get
		{
			return m_object;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Object");
			}
			m_object = value;
		}
	}

	internal ObjectInfo(IPdfPrimitive obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		m_object = obj;
		m_bModified = true;
	}

	internal ObjectInfo(IPdfPrimitive obj, PdfReference reference)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		if (reference == null)
		{
			throw new ArgumentNullException("reference");
		}
		m_object = obj;
		m_reference = reference;
	}

	public void SetModified()
	{
		m_bModified = true;
	}

	internal void SetReference(PdfReference reference)
	{
		if (reference == null)
		{
			throw new ArgumentNullException("reference");
		}
		if (m_reference != null)
		{
			throw new ArgumentException("The object has the reference bound to it.", "reference");
		}
		m_reference = reference;
	}

	public override string ToString()
	{
		return ((m_reference != null) ? m_reference.ToString() : string.Empty) + " : " + Object.GetType().Name;
	}

	public static bool operator ==(ObjectInfo oi, object obj)
	{
		bool result = false;
		if (oi != null)
		{
			result = oi.Equals(obj);
		}
		return result;
	}

	public static bool operator !=(ObjectInfo oi, object obj)
	{
		return !(oi == obj);
	}

	public override bool Equals(object obj)
	{
		bool result = false;
		if (obj != null)
		{
			IPdfPrimitive pdfPrimitive = obj as IPdfPrimitive;
			ObjectInfo objectInfo = obj as ObjectInfo;
			if (pdfPrimitive != null)
			{
				result = Object == pdfPrimitive;
			}
			else if (objectInfo != null)
			{
				result = objectInfo.Object == Object;
			}
		}
		return result;
	}
}
