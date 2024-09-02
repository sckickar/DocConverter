using System;
using System.Globalization;
using DocGen.Pdf.IO;

namespace DocGen.Pdf.Primitives;

internal class PdfNumber : IPdfPrimitive
{
	private int m_intValue;

	private float m_floatValue;

	private bool m_isInteger;

	private bool m_IsLong;

	private ObjectStatus m_status;

	private bool m_isSaving;

	private int m_index;

	private int m_position = -1;

	private long m_longValue;

	internal long LongValue
	{
		get
		{
			return m_longValue;
		}
		set
		{
			m_isInteger = false;
			m_IsLong = true;
			m_longValue = value;
			m_intValue = (int)value;
			m_floatValue = value;
		}
	}

	public int IntValue
	{
		get
		{
			return m_intValue;
		}
		set
		{
			m_isInteger = true;
			m_intValue = value;
			m_longValue = value;
			m_floatValue = value;
		}
	}

	public float FloatValue
	{
		get
		{
			return m_floatValue;
		}
		set
		{
			m_isInteger = false;
			m_floatValue = value;
			m_intValue = (int)value;
		}
	}

	public bool IsInteger
	{
		get
		{
			return m_isInteger;
		}
		set
		{
			m_isInteger = value;
		}
	}

	internal bool IsLong
	{
		get
		{
			return m_IsLong;
		}
		set
		{
			m_IsLong = value;
		}
	}

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

	internal PdfNumber(int value)
	{
		IntValue = value;
	}

	internal PdfNumber(long value)
	{
		LongValue = value;
	}

	internal PdfNumber(float value)
	{
		FloatValue = value;
	}

	internal PdfNumber(double value)
	{
		FloatValue = (float)value;
	}

	public static string FloatToString(float number)
	{
		return number.ToString("######################.00######", CultureInfo.InvariantCulture);
	}

	public static float Min(float x, float y, float z)
	{
		float val = Math.Min(x, y);
		return Math.Min(z, val);
	}

	public static float Max(float x, float y, float z)
	{
		float val = Math.Max(x, y);
		return Math.Max(z, val);
	}

	public void Save(IPdfWriter writer)
	{
		if (IsInteger)
		{
			writer.Write(IntValue.ToString(CultureInfo.InvariantCulture));
		}
		else if (IsLong)
		{
			writer.Write(LongValue.ToString(CultureInfo.InvariantCulture));
		}
		else
		{
			writer.Write(FloatToString(FloatValue));
		}
	}

	public IPdfPrimitive Clone(PdfCrossTable crossTable)
	{
		PdfNumber pdfNumber = null;
		if (IsInteger)
		{
			return new PdfNumber(IntValue);
		}
		if (IsLong)
		{
			return new PdfNumber(LongValue);
		}
		return new PdfNumber(FloatValue);
	}
}
