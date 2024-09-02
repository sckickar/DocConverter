using System;
using System.Collections.Generic;

namespace DocGen.Office;

internal class OfficeMathFraction : OfficeMathFunctionBase, IOfficeMathFraction, IOfficeMathFunctionBase, IOfficeMathEntity
{
	internal const short TypeKey = 29;

	private OfficeMath m_denominator;

	private OfficeMath m_numerator;

	private Dictionary<int, object> m_propertiesHash;

	internal IOfficeRunFormat m_controlProperties;

	public MathFractionType FractionType
	{
		get
		{
			return (MathFractionType)GetPropertyValue(29);
		}
		set
		{
			SetPropertyValue(29, value);
		}
	}

	public IOfficeMath Denominator => m_denominator;

	public IOfficeMath Numerator => m_numerator;

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

	internal OfficeMathFraction(IOfficeMathEntity owner)
		: base(owner)
	{
		m_type = MathFunctionType.Fraction;
		m_denominator = new OfficeMath(this);
		m_numerator = new OfficeMath(this);
	}

	internal override void Close()
	{
		if (m_propertiesHash != null)
		{
			m_propertiesHash.Clear();
			m_propertiesHash = null;
		}
		if (m_denominator != null)
		{
			m_denominator.Close();
		}
		if (m_numerator != null)
		{
			m_numerator.Close();
		}
		if (m_controlProperties != null)
		{
			m_controlProperties.Dispose();
			m_controlProperties = null;
		}
		base.Close();
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
		if (key == 29)
		{
			return MathFractionType.NormalFractionBar;
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

	internal override OfficeMathFunctionBase Clone(IOfficeMathEntity owner)
	{
		OfficeMathFraction officeMathFraction = (OfficeMathFraction)MemberwiseClone();
		officeMathFraction.SetOwner(owner);
		if (officeMathFraction.m_controlProperties != null)
		{
			officeMathFraction.m_controlProperties = m_controlProperties.Clone();
		}
		officeMathFraction.m_denominator = m_denominator.CloneImpl(officeMathFraction);
		officeMathFraction.m_numerator = m_numerator.CloneImpl(officeMathFraction);
		return officeMathFraction;
	}
}
