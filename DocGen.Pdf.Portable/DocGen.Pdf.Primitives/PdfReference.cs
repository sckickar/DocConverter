using System;
using System.Globalization;
using DocGen.Pdf.IO;

namespace DocGen.Pdf.Primitives;

internal class PdfReference : IPdfPrimitive
{
	public readonly long ObjNum;

	public readonly int GenNum;

	private ObjectStatus m_status;

	private bool m_isSaving;

	private int m_index;

	private int m_position = -1;

	private bool m_isDisposed;

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

	internal bool IsDisposed
	{
		get
		{
			return m_isDisposed;
		}
		set
		{
			m_isDisposed = value;
		}
	}

	public PdfReference(long objNum, int genNum)
	{
		ObjNum = objNum;
		GenNum = genNum;
	}

	public PdfReference(string objNum, string genNum)
	{
		double result = 0.0;
		double result2 = 0.0;
		if (!double.TryParse(objNum, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
		{
			throw new ArgumentException("Invalid format (must be an integer)", "objNum");
		}
		if (!double.TryParse(genNum, NumberStyles.Integer, CultureInfo.InvariantCulture, out result2))
		{
			throw new ArgumentException("Invalid format (must be an integer)", "genNum");
		}
		ObjNum = (int)result;
		GenNum = (int)result2;
	}

	public override string ToString()
	{
		return $"{ObjNum} {GenNum} R";
	}

	public override bool Equals(object obj)
	{
		PdfReference pdfReference = obj as PdfReference;
		if (pdfReference == null)
		{
			return false;
		}
		if (pdfReference.ObjNum == ObjNum)
		{
			return pdfReference.GenNum == GenNum;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (int)(ObjNum + GenNum << 24);
	}

	IPdfPrimitive IPdfPrimitive.Clone(PdfCrossTable crossTable)
	{
		return null;
	}

	public static bool operator ==(PdfReference ref1, PdfReference ref2)
	{
		if ((object)ref1 == null || (object)ref2 == null)
		{
			return (object)ref1 == ref2;
		}
		if (ref1.ObjNum == ref2.ObjNum)
		{
			return ref1.GenNum == ref2.GenNum;
		}
		return false;
	}

	public static bool operator !=(PdfReference ref1, PdfReference ref2)
	{
		return !(ref1 == ref2);
	}

	public void Save(IPdfWriter writer)
	{
		writer.Write(ToString());
	}
}
