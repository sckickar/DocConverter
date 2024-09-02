using System;
using System.Collections.Generic;

namespace DocGen.Office;

internal class OfficeMathBreak : OwnerHolder, IOfficeMathBreak
{
	internal const short AlignAtKey = 16;

	private Dictionary<int, object> m_propertiesHash;

	public int AlignAt
	{
		get
		{
			return (int)GetPropertyValue(16);
		}
		set
		{
			SetPropertyValue(16, value);
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
		}
	}

	internal OfficeMathBreak(IOfficeMathEntity owner)
		: base(owner)
	{
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
		if (key == 16)
		{
			return 0;
		}
		return new ArgumentException("key has invalid value");
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

	internal OfficeMathBreak Clone(IOfficeMathEntity owner)
	{
		OfficeMathBreak obj = (OfficeMathBreak)MemberwiseClone();
		obj.SetOwner(owner);
		return obj;
	}

	internal override void Close()
	{
		if (m_propertiesHash != null)
		{
			m_propertiesHash.Clear();
			m_propertiesHash = null;
		}
		base.Close();
	}

	internal OfficeMathBreak Clone()
	{
		return (OfficeMathBreak)MemberwiseClone();
	}
}
