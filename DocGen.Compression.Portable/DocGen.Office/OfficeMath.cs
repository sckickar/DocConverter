using System;
using System.Collections.Generic;

namespace DocGen.Office;

internal class OfficeMath : OwnerHolder, IOfficeMath, IOfficeMathEntity
{
	internal const short ArgumentSizeKey = 74;

	internal const short AlignPointKey = 76;

	private OfficeMathBaseCollection m_functions;

	internal OfficeMathMatrixColumn m_parentCol;

	internal OfficeMathMatrixRow m_parentRow;

	private OfficeMathBreaks m_breaks;

	private Dictionary<int, object> m_propertiesHash;

	internal int AlignPoint
	{
		get
		{
			return (int)GetPropertyValue(76);
		}
		set
		{
			SetPropertyValue(76, value);
		}
	}

	public int ArgumentSize
	{
		get
		{
			return (int)GetPropertyValue(74);
		}
		set
		{
			SetPropertyValue(74, value);
		}
	}

	public IOfficeMathBaseCollection Functions => m_functions;

	internal int NestingLevel => 0;

	public IOfficeMathMatrixColumn OwnerColumn => m_parentCol;

	public IOfficeMath OwnerMath => GetOwnerMath();

	public IOfficeMathMatrixRow OwnerRow => m_parentRow;

	public IOfficeMathBreaks Breaks => m_breaks;

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

	internal OfficeMath(IOfficeMathEntity owner)
		: base(owner)
	{
		m_functions = new OfficeMathBaseCollection(this);
		m_breaks = new OfficeMathBreaks(base.OwnerMathEntity);
		if (owner is OfficeMathMatrixRow)
		{
			m_parentRow = owner as OfficeMathMatrixRow;
		}
		else if (owner is OfficeMathMatrixColumn)
		{
			m_parentCol = owner as OfficeMathMatrixColumn;
		}
	}

	private IOfficeMath GetOwnerMath()
	{
		object ownerMathEntity = base.OwnerMathEntity;
		while (!(ownerMathEntity is IOfficeMath) && !(ownerMathEntity is IOfficeMathParagraph))
		{
			if (ownerMathEntity is OfficeMathFunctionBase)
			{
				ownerMathEntity = (ownerMathEntity as OfficeMathFunctionBase).OwnerMathEntity;
			}
		}
		return ownerMathEntity as OfficeMath;
	}

	internal void Buildup()
	{
	}

	internal void ConvertToLiteralText()
	{
	}

	internal void ConvertToMathText()
	{
	}

	internal void ConvertToNormalText()
	{
	}

	internal void Linearize()
	{
	}

	internal void Remove()
	{
	}

	internal override void Close()
	{
		if (m_propertiesHash != null)
		{
			m_propertiesHash.Clear();
			m_propertiesHash = null;
		}
		if (m_breaks != null)
		{
			m_breaks.Close();
			m_breaks = null;
		}
		if (m_functions != null)
		{
			m_functions.Close();
			m_functions = null;
		}
		base.Close();
	}

	internal OfficeMath CloneImpl(IOfficeMathEntity owner)
	{
		OfficeMath officeMath = (OfficeMath)MemberwiseClone();
		officeMath.SetOwner(owner);
		officeMath.m_breaks = new OfficeMathBreaks(officeMath);
		m_breaks.CloneItemsTo(officeMath.m_breaks);
		officeMath.m_functions = new OfficeMathBaseCollection(officeMath);
		m_functions.CloneItemsTo(officeMath.m_functions);
		return officeMath;
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
		case 76:
			return -1;
		case 74:
			if (!(base.OwnerMathEntity is OfficeMathFunctionBase officeMathFunctionBase))
			{
				return 0;
			}
			switch (officeMathFunctionBase.Type)
			{
			case MathFunctionType.Box:
			case MathFunctionType.GroupCharacter:
				return 0;
			case MathFunctionType.Limit:
			case MathFunctionType.NArray:
			case MathFunctionType.LeftSubSuperscript:
			case MathFunctionType.SubSuperscript:
			case MathFunctionType.RightSubSuperscript:
				return -1;
			case MathFunctionType.Radical:
				return -2;
			default:
				return new ArgumentException("Cannot change argument size for this function");
			}
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
}
