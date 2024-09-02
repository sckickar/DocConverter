using System;
using System.Text;
using DocGen.CompoundFile.DocIO;
using DocGen.CompoundFile.DocIO.Net;
using DocGen.CompoundFile.Net;

namespace DocGen.DocIO.DLS;

public class DocumentProperty
{
	private const int DEF_START_ID2 = 1000;

	private const int DEF_FILE_TIME_START_YEAR = 1600;

	private BuiltInProperty m_propertyId;

	private string m_strName;

	private object m_value;

	private DocGen.CompoundFile.DocIO.PropertyType m_type;

	private string m_strLinkSource;

	private byte m_bFlags;

	internal bool IsBuiltIn => m_strName == null;

	internal BuiltInProperty PropertyId
	{
		get
		{
			return m_propertyId;
		}
		set
		{
			m_propertyId = value;
		}
	}

	public string Name
	{
		get
		{
			if (m_strName != null)
			{
				return m_strName;
			}
			return m_propertyId.ToString();
		}
	}

	public object Value
	{
		get
		{
			return m_value;
		}
		set
		{
			m_value = value;
			DetectPropertyType();
		}
	}

	public PropertyValueType ValueType
	{
		get
		{
			if (m_value is string)
			{
				return PropertyValueType.String;
			}
			if (m_value is bool)
			{
				return PropertyValueType.Boolean;
			}
			if (m_value is DateTime)
			{
				return PropertyValueType.Date;
			}
			if (m_value is int || m_value is int || m_value is uint)
			{
				return PropertyValueType.Int;
			}
			if (m_value is double)
			{
				return PropertyValueType.Double;
			}
			if (Value is float)
			{
				return PropertyValueType.Float;
			}
			if (Value is byte[])
			{
				return PropertyValueType.ByteArray;
			}
			throw new Exception("Property value is of unsupported type.");
		}
	}

	internal bool Boolean
	{
		get
		{
			if (m_type == DocGen.CompoundFile.DocIO.PropertyType.Bool)
			{
				return Convert.ToBoolean(m_value);
			}
			throw new InvalidCastException("Can't convert value to boolean.");
		}
		set
		{
			m_type = DocGen.CompoundFile.DocIO.PropertyType.Bool;
			m_value = value;
		}
	}

	internal int Integer
	{
		get
		{
			DetectPropertyType();
			if (m_value != null && m_type == DocGen.CompoundFile.DocIO.PropertyType.Int && !string.IsNullOrEmpty(m_value.ToString()) && int.TryParse(m_value.ToString(), out var result))
			{
				return result;
			}
			throw new InvalidCastException("Can't convert value to integer.");
		}
		set
		{
			m_type = DocGen.CompoundFile.DocIO.PropertyType.Int;
			m_value = value;
		}
	}

	internal int Int32
	{
		get
		{
			DetectPropertyType();
			if (m_type == DocGen.CompoundFile.DocIO.PropertyType.Int32)
			{
				return Convert.ToInt32(m_value);
			}
			throw new InvalidCastException("Can't convert value to integer.");
		}
		set
		{
			m_type = DocGen.CompoundFile.DocIO.PropertyType.Int32;
			m_value = value;
		}
	}

	internal double Double
	{
		get
		{
			DetectPropertyType();
			if (m_type == DocGen.CompoundFile.DocIO.PropertyType.Double)
			{
				return Convert.ToDouble(m_value);
			}
			throw new InvalidCastException("Can't convert value to integer.");
		}
		set
		{
			m_type = DocGen.CompoundFile.DocIO.PropertyType.Double;
			m_value = value;
		}
	}

	internal string Text
	{
		get
		{
			if (m_type == DocGen.CompoundFile.DocIO.PropertyType.Empty)
			{
				DetectPropertyType();
			}
			if (m_type == DocGen.CompoundFile.DocIO.PropertyType.String || m_type == DocGen.CompoundFile.DocIO.PropertyType.AsciiString)
			{
				return Convert.ToString(m_value);
			}
			throw new InvalidCastException("Can't convert value to string.");
		}
		set
		{
			m_type = DetectStringType(value);
			m_value = value;
		}
	}

	internal DateTime DateTime
	{
		get
		{
			try
			{
				DetectPropertyType();
				if (m_type == DocGen.CompoundFile.DocIO.PropertyType.DateTime)
				{
					return Convert.ToDateTime(m_value);
				}
				return DateTime.MinValue;
			}
			catch
			{
				return DateTime.MinValue;
			}
		}
		set
		{
			m_type = DocGen.CompoundFile.DocIO.PropertyType.DateTime;
			m_value = value;
		}
	}

	internal TimeSpan TimeSpan
	{
		get
		{
			return (TimeSpan)Value;
		}
		set
		{
			m_value = value;
		}
	}

	internal byte[] Blob
	{
		get
		{
			if (m_type == DocGen.CompoundFile.DocIO.PropertyType.Blob)
			{
				return (byte[])m_value;
			}
			throw new InvalidCastException("Can't convert value to Blob.");
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_type = DocGen.CompoundFile.DocIO.PropertyType.Blob;
			m_value = value;
		}
	}

	public ClipboardData ClipboardData
	{
		get
		{
			if (m_type == DocGen.CompoundFile.DocIO.PropertyType.ClipboardData)
			{
				return (ClipboardData)m_value;
			}
			throw new InvalidCastException("Can't convert value to ClipboardData.");
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_type = DocGen.CompoundFile.DocIO.PropertyType.ClipboardData;
			m_value = value;
		}
	}

	internal string[] StringArray
	{
		get
		{
			if (m_type == DocGen.CompoundFile.DocIO.PropertyType.StringArray)
			{
				return (string[])m_value;
			}
			throw new InvalidCastException("Can't convert value to an array of strings.");
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_type = DocGen.CompoundFile.DocIO.PropertyType.StringArray;
			m_value = value;
		}
	}

	internal object[] ObjectArray
	{
		get
		{
			if (m_type == DocGen.CompoundFile.DocIO.PropertyType.ObjectArray)
			{
				return (object[])m_value;
			}
			throw new InvalidCastException("Can't convert value to an array of strings.");
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_type = DocGen.CompoundFile.DocIO.PropertyType.ObjectArray;
			m_value = value;
		}
	}

	internal DocGen.CompoundFile.DocIO.PropertyType PropertyType
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

	internal string LinkSource
	{
		get
		{
			return m_strLinkSource;
		}
		set
		{
			if (IsBuiltIn)
			{
				throw new InvalidOperationException("This operation can't be performed on built-in property.");
			}
			m_strLinkSource = value;
			LinkToContent = true;
		}
	}

	internal bool LinkToContent
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal string InternalName => m_strName;

	private DocumentProperty()
	{
	}

	internal DocumentProperty(string strName, object value)
	{
		if (strName == null)
		{
			throw new ArgumentNullException("strName");
		}
		if (strName.Length == 0)
		{
			throw new ArgumentException("strName - string cannot be empty.");
		}
		m_strName = strName;
		Value = value;
		m_type = DetectPropertyType(value);
	}

	internal DocumentProperty(string strName, object value, DocGen.CompoundFile.DocIO.PropertyType type)
	{
		if (strName == null)
		{
			throw new ArgumentNullException("strName");
		}
		if (strName.Length == 0)
		{
			throw new ArgumentException("strName - string cannot be empty.");
		}
		m_strName = strName;
		m_value = value;
		m_type = type;
	}

	internal DocumentProperty(BuiltInProperty propertyId, object value)
	{
		m_propertyId = propertyId;
		m_value = value;
		m_type = DetectPropertyType(value);
	}

	internal DocumentProperty(IPropertyData variant, bool bSummary)
	{
		if (variant == null)
		{
			throw new ArgumentNullException("variant");
		}
		m_strName = variant.Name;
		if (m_strName == null)
		{
			if (bSummary)
			{
				m_propertyId = (BuiltInProperty)variant.Id;
			}
			else if (variant.Id == 1)
			{
				m_propertyId = (BuiltInProperty)variant.Id;
			}
			else
			{
				m_propertyId = (BuiltInProperty)(variant.Id + 1000 - 2);
			}
		}
		if (bSummary && m_propertyId == BuiltInProperty.EditTime && variant.Value is DateTime)
		{
			m_value = TimeSpan.FromTicks(((DateTime)variant.Value).Ticks - 504911232000000000L);
		}
		else
		{
			m_value = variant.Value;
		}
		m_type = (DocGen.CompoundFile.DocIO.PropertyType)variant.Type;
	}

	private DocGen.CompoundFile.DocIO.PropertyType DetectStringType(string value)
	{
		if (Encoding.UTF8.GetByteCount(value) != value.Length)
		{
			return DocGen.CompoundFile.DocIO.PropertyType.String;
		}
		return DocGen.CompoundFile.DocIO.PropertyType.AsciiString;
	}

	public bool ToBool()
	{
		return (bool)m_value;
	}

	public DateTime ToDateTime()
	{
		return ((DateTime)m_value).Date;
	}

	public float ToFloat()
	{
		return Convert.ToSingle(m_value);
	}

	public double ToDouble()
	{
		return (double)m_value;
	}

	public int ToInt()
	{
		return (int)m_value;
	}

	public override string ToString()
	{
		return (string)m_value;
	}

	public byte[] ToByteArray()
	{
		return (byte[])m_value;
	}

	internal bool FillPropVariant(IPropertyData variant, int iPropertyId)
	{
		if (variant == null)
		{
			throw new ArgumentNullException("variant");
		}
		if (m_value == null)
		{
			return false;
		}
		if (IsBuiltIn)
		{
			bool bSummary;
			int id = CorrectIndex(m_propertyId, out bSummary);
			variant.Id = id;
		}
		else
		{
			variant.Id = iPropertyId;
		}
		object value = m_value;
		if (IsBuiltIn && variant.Id == 10 && m_value is TimeSpan)
		{
			value = DateTime.Now;
		}
		return variant.SetValue(value, m_type);
	}

	internal int CorrectIndex(BuiltInProperty propertyId, out bool bSummary)
	{
		int num = (int)propertyId;
		if (num >= 1000)
		{
			num -= 998;
			bSummary = false;
		}
		else
		{
			bSummary = true;
		}
		return num;
	}

	internal static DocGen.CompoundFile.DocIO.PropertyType DetectPropertyType(object value)
	{
		DocGen.CompoundFile.DocIO.PropertyType result = DocGen.CompoundFile.DocIO.PropertyType.Null;
		if (value is string)
		{
			result = ((Encoding.UTF8.GetByteCount(value as string) == (value as string).Length) ? DocGen.CompoundFile.DocIO.PropertyType.AsciiString : DocGen.CompoundFile.DocIO.PropertyType.String);
		}
		else if (value is double)
		{
			result = DocGen.CompoundFile.DocIO.PropertyType.Double;
		}
		else if (value is int)
		{
			result = DocGen.CompoundFile.DocIO.PropertyType.Int32;
		}
		else if (value is bool)
		{
			result = DocGen.CompoundFile.DocIO.PropertyType.Bool;
		}
		else if (value is DateTime || value is TimeSpan)
		{
			result = DocGen.CompoundFile.DocIO.PropertyType.DateTime;
		}
		else if (value is object[])
		{
			result = DocGen.CompoundFile.DocIO.PropertyType.ObjectArray;
		}
		else if (value is string[])
		{
			result = DocGen.CompoundFile.DocIO.PropertyType.StringArray;
		}
		else if (value is byte[])
		{
			result = DocGen.CompoundFile.DocIO.PropertyType.Blob;
		}
		else if (value is ClipboardData)
		{
			result = DocGen.CompoundFile.DocIO.PropertyType.ClipboardData;
		}
		return result;
	}

	private void DetectPropertyType()
	{
		if (m_value is string)
		{
			m_type = DetectStringType((string)m_value);
		}
		else if (m_value is double)
		{
			m_type = DocGen.CompoundFile.DocIO.PropertyType.Double;
		}
		else if (m_value is int)
		{
			m_type = DocGen.CompoundFile.DocIO.PropertyType.Int32;
		}
		else if (m_value is bool)
		{
			m_type = DocGen.CompoundFile.DocIO.PropertyType.Bool;
		}
		else if (m_value is DateTime || m_value is TimeSpan)
		{
			m_type = DocGen.CompoundFile.DocIO.PropertyType.DateTime;
		}
		else if (m_value is object[])
		{
			m_type = DocGen.CompoundFile.DocIO.PropertyType.ObjectArray;
		}
		else if (m_value is string[])
		{
			m_type = DocGen.CompoundFile.DocIO.PropertyType.StringArray;
		}
		else if (m_value is byte[])
		{
			m_type = DocGen.CompoundFile.DocIO.PropertyType.Blob;
		}
		else if (m_value is ClipboardData)
		{
			m_type = DocGen.CompoundFile.DocIO.PropertyType.ClipboardData;
		}
	}

	internal void SetLinkSource(IPropertyData variant)
	{
		if (variant == null)
		{
			throw new ArgumentNullException("variant");
		}
		if (variant.Type != VarEnum.VT_LPSTR && variant.Type != VarEnum.VT_LPWSTR)
		{
			throw new ArgumentOutOfRangeException("LinkSource");
		}
		LinkSource = variant.Value.ToString();
	}

	public DocumentProperty Clone()
	{
		DocumentProperty obj = (DocumentProperty)MemberwiseClone();
		obj.CloneValue();
		return obj;
	}

	private void CloneValue()
	{
		if (m_value != null)
		{
			switch (m_type)
			{
			case DocGen.CompoundFile.DocIO.PropertyType.Blob:
				m_value = CloneUtils.CloneByteArray(Blob);
				break;
			case DocGen.CompoundFile.DocIO.PropertyType.StringArray:
				m_value = CloneUtils.CloneStringArray(StringArray);
				break;
			case DocGen.CompoundFile.DocIO.PropertyType.ObjectArray:
				m_value = CloneUtils.CloneArray(ObjectArray);
				break;
			case DocGen.CompoundFile.DocIO.PropertyType.ClipboardData:
				m_value = CloneUtils.CloneCloneable(ClipboardData);
				break;
			}
		}
	}

	internal void Close()
	{
		if (m_value != null)
		{
			m_value = null;
		}
	}
}
