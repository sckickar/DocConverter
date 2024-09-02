using System;
using System.Collections.Generic;

namespace DocGen.Office;

internal class OfficeMathDelimiter : OfficeMathFunctionBase, IOfficeMathDelimiter, IOfficeMathFunctionBase, IOfficeMathEntity
{
	internal const short BegCharKey = 17;

	internal const short EndCharKey = 18;

	internal const short NoRightCharKey = 19;

	internal const short NoLeftCharKey = 20;

	internal const short SeperatorCharKey = 21;

	internal const short GrowKey = 22;

	internal const short ShapeKey = 23;

	private Dictionary<int, object> m_propertiesHash;

	internal OfficeMaths m_equation;

	internal IOfficeRunFormat m_controlProperties;

	public string BeginCharacter
	{
		get
		{
			return (string)GetPropertyValue(17);
		}
		set
		{
			SetPropertyValue(17, value);
		}
	}

	public string EndCharacter
	{
		get
		{
			return (string)GetPropertyValue(18);
		}
		set
		{
			SetPropertyValue(18, value);
		}
	}

	public bool IsGrow
	{
		get
		{
			return (bool)GetPropertyValue(22);
		}
		set
		{
			SetPropertyValue(22, value);
		}
	}

	public string Seperator
	{
		get
		{
			return (string)GetPropertyValue(21);
		}
		set
		{
			SetPropertyValue(21, value);
		}
	}

	public MathDelimiterShapeType DelimiterShape
	{
		get
		{
			return (MathDelimiterShapeType)GetPropertyValue(23);
		}
		set
		{
			SetPropertyValue(23, value);
		}
	}

	public IOfficeMaths Equation => m_equation;

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

	public IOfficeRunFormat ControlProperties
	{
		get
		{
			if (m_controlProperties == null)
			{
				m_controlProperties = GetDefaultControlProperties();
			}
			return m_controlProperties;
		}
		set
		{
			m_controlProperties = value;
		}
	}

	internal OfficeMathDelimiter(IOfficeMathEntity owner)
		: base(owner)
	{
		m_type = MathFunctionType.Delimiter;
		m_equation = new OfficeMaths(this);
	}

	internal object GetPropertyValue(int propKey)
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
		case 17:
			return "(";
		case 18:
			return ")";
		case 21:
			return "|";
		case 19:
		case 20:
			return false;
		case 22:
			return true;
		case 23:
			return MathDelimiterShapeType.Centered;
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

	internal override OfficeMathFunctionBase Clone(IOfficeMathEntity owner)
	{
		OfficeMathDelimiter officeMathDelimiter = (OfficeMathDelimiter)MemberwiseClone();
		officeMathDelimiter.SetOwner(owner);
		if (officeMathDelimiter.m_controlProperties != null)
		{
			officeMathDelimiter.m_controlProperties = m_controlProperties.Clone();
		}
		officeMathDelimiter.m_equation = new OfficeMaths(officeMathDelimiter);
		m_equation.CloneItemsTo(officeMathDelimiter.m_equation);
		return officeMathDelimiter;
	}

	internal override void Close()
	{
		if (m_propertiesHash != null)
		{
			m_propertiesHash.Clear();
			m_propertiesHash = null;
		}
		if (m_equation != null)
		{
			m_equation.Close();
		}
		if (m_controlProperties != null)
		{
			m_controlProperties.Dispose();
			m_controlProperties = null;
		}
		base.Close();
	}
}
