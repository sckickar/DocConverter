using System;
using System.Collections.Generic;

namespace DocGen.Office;

internal class OfficeMathEquationArray : OfficeMathFunctionBase, IOfficeMathEquationArray, IOfficeMathFunctionBase, IOfficeMathEntity
{
	internal const short AlignKey = 24;

	internal const short MaxDistKey = 25;

	internal const short ObjDistKey = 26;

	internal const short RowSpacingKey = 27;

	internal const short RowSpacingRuleKey = 28;

	internal OfficeMaths m_equation;

	private Dictionary<int, object> m_propertiesHash;

	internal IOfficeRunFormat m_controlProperties;

	public IOfficeMaths Equation => m_equation;

	public MathVerticalAlignment VerticalAlignment
	{
		get
		{
			return (MathVerticalAlignment)GetPropertyValue(24);
		}
		set
		{
			SetPropertyValue(24, value);
		}
	}

	public bool ExpandEquationContainer
	{
		get
		{
			return (bool)GetPropertyValue(25);
		}
		set
		{
			SetPropertyValue(25, value);
		}
	}

	public bool ExpandEquationContent
	{
		get
		{
			return (bool)GetPropertyValue(26);
		}
		set
		{
			SetPropertyValue(26, value);
		}
	}

	public float RowSpacing
	{
		get
		{
			return (float)GetPropertyValue(27);
		}
		set
		{
			if (RowSpacingRule == SpacingRule.Exactly && (value < 0f || value > 1584f))
			{
				throw new ArgumentException("RowSpacing must be between 0 pt and 1584 pt for Exactly spacing rule.");
			}
			if (RowSpacingRule == SpacingRule.Multiple && (value < 0f || value > 132f))
			{
				throw new ArgumentException("RowSpacing must be between 0 li and 1584 li for Multiple spacing rule.");
			}
			SetPropertyValue(27, value);
		}
	}

	public SpacingRule RowSpacingRule
	{
		get
		{
			return (SpacingRule)GetPropertyValue(28);
		}
		set
		{
			SetPropertyValue(28, value);
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

	internal OfficeMathEquationArray(IOfficeMathEntity owner)
		: base(owner)
	{
		m_type = MathFunctionType.EquationArray;
		m_equation = new OfficeMaths(this);
		m_propertiesHash = new Dictionary<int, object>();
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
		return key switch
		{
			24 => MathVerticalAlignment.Center, 
			25 => false, 
			26 => false, 
			27 => 0f, 
			28 => SpacingRule.Single, 
			_ => new ArgumentException("key has invalid value"), 
		};
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
		OfficeMathEquationArray officeMathEquationArray = (OfficeMathEquationArray)MemberwiseClone();
		officeMathEquationArray.SetOwner(owner);
		if (officeMathEquationArray.m_controlProperties != null)
		{
			officeMathEquationArray.m_controlProperties = m_controlProperties.Clone();
		}
		officeMathEquationArray.m_equation = new OfficeMaths(officeMathEquationArray);
		m_equation.CloneItemsTo(officeMathEquationArray.m_equation);
		return officeMathEquationArray;
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
			m_equation = null;
		}
		if (m_controlProperties != null)
		{
			m_controlProperties.Dispose();
			m_controlProperties = null;
		}
		base.Close();
	}
}
