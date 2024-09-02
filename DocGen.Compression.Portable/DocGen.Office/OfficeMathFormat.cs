using System;
using System.Collections.Generic;

namespace DocGen.Office;

internal class OfficeMathFormat : OwnerHolder, IOfficeMathFormat
{
	internal const short HasAlignmentKey = 53;

	internal const short HasLiteralKey = 54;

	internal const short HasNormalTextKey = 55;

	internal const short ScriptKey = 56;

	internal const short StyleKey = 57;

	private byte m_bFlags;

	private OfficeMathBreak m_break;

	private Dictionary<int, object> m_propertiesHash;

	public bool HasAlignment
	{
		get
		{
			return (bool)GetPropertyValue(53);
		}
		set
		{
			SetPropertyValue(53, value);
		}
	}

	public IOfficeMathBreak Break
	{
		get
		{
			return m_break;
		}
		set
		{
			m_break = (OfficeMathBreak)value;
			IsDefault = false;
		}
	}

	public bool HasLiteral
	{
		get
		{
			return (bool)GetPropertyValue(54);
		}
		set
		{
			SetPropertyValue(54, value);
		}
	}

	public bool HasNormalText
	{
		get
		{
			return (bool)GetPropertyValue(55);
		}
		set
		{
			SetPropertyValue(55, value);
		}
	}

	public MathFontType Font
	{
		get
		{
			return (MathFontType)GetPropertyValue(56);
		}
		set
		{
			SetPropertyValue(56, value);
		}
	}

	public MathStyleType Style
	{
		get
		{
			return (MathStyleType)GetPropertyValue(57);
		}
		set
		{
			SetPropertyValue(57, value);
		}
	}

	internal Dictionary<int, object> PropertiesHash
	{
		get
		{
			if (m_propertiesHash == null)
			{
				m_propertiesHash = new Dictionary<int, object>();
			}
			return m_propertiesHash;
		}
	}

	internal object this[int key]
	{
		get
		{
			if (PropertiesHash.ContainsKey(key))
			{
				return PropertiesHash[key];
			}
			return GetDefValue(key);
		}
		set
		{
			PropertiesHash[key] = value;
			IsDefault = false;
		}
	}

	internal bool IsDefault
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

	internal OfficeMathFormat(IOfficeMathEntity owner)
		: base(owner)
	{
		IsDefault = true;
	}

	internal override void Close()
	{
		if (m_propertiesHash != null)
		{
			m_propertiesHash.Clear();
			m_propertiesHash = null;
		}
		if (m_break != null)
		{
			m_break.Close();
		}
		base.Close();
	}

	private object GetPropertyValue(int propKey)
	{
		return this[propKey];
	}

	internal void SetPropertyValue(int propKey, object value)
	{
		this[propKey] = value;
	}

	private object GetDefValue(int key)
	{
		switch (key)
		{
		case 53:
		case 54:
		case 55:
			return false;
		case 56:
			return MathFontType.Roman;
		case 57:
			return MathStyleType.Italic;
		default:
			return new ArgumentException("key has invalid value");
		}
	}

	internal bool HasValue(int propertyKey)
	{
		if (HasKey(propertyKey))
		{
			return true;
		}
		return false;
	}

	internal bool HasKey(int key)
	{
		if (PropertiesHash == null)
		{
			return false;
		}
		return PropertiesHash.ContainsKey(key);
	}

	internal OfficeMathFormat Clone(IOfficeMathEntity owner)
	{
		OfficeMathFormat officeMathFormat = (OfficeMathFormat)MemberwiseClone();
		officeMathFormat.SetOwner(owner);
		if (m_break != null)
		{
			officeMathFormat.m_break = m_break.Clone(officeMathFormat);
		}
		return officeMathFormat;
	}
}
