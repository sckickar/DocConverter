using System;
using System.Collections.Generic;

namespace DocGen.Office;

internal class OfficeMathProperties
{
	internal const short BreakOnBinaryOperatorsKey = 58;

	internal const short BreakOnBinarySubtractionKey = 59;

	internal const short DefaultJustificationKey = 60;

	internal const short DisplayMathDefaultsKey = 61;

	internal const short InterEquationSpacingKey = 62;

	internal const short IntegralLimitLocationsKey = 63;

	internal const short IntraEquationSpacingKey = 64;

	internal const short LeftMarginKey = 65;

	internal const short MathFontKey = 66;

	internal const short NAryLimitLocationKey = 67;

	internal const short PostParagraphSpacingKey = 68;

	internal const short PreParagraphSpacingKey = 69;

	internal const short RightMarginKey = 70;

	internal const short SmallFractionKey = 71;

	internal const short WrapIndentKey = 72;

	internal const short WrapRightKey = 73;

	private Dictionary<int, object> m_propertiesHash;

	private byte m_bFlags;

	internal BreakOnBinaryOperatorsType BreakOnBinaryOperators
	{
		get
		{
			return (BreakOnBinaryOperatorsType)GetPropertyValue(58);
		}
		set
		{
			SetPropertyValue(58, value);
		}
	}

	internal BreakOnBinarySubtractionType BreakOnBinarySubtraction
	{
		get
		{
			return (BreakOnBinarySubtractionType)GetPropertyValue(59);
		}
		set
		{
			SetPropertyValue(59, value);
		}
	}

	internal MathJustification DefaultJustification
	{
		get
		{
			return (MathJustification)GetPropertyValue(60);
		}
		set
		{
			SetPropertyValue(60, value);
		}
	}

	internal bool DisplayMathDefaults
	{
		get
		{
			return (bool)GetPropertyValue(61);
		}
		set
		{
			SetPropertyValue(61, value);
		}
	}

	internal int InterEquationSpacing
	{
		get
		{
			return (int)GetPropertyValue(62);
		}
		set
		{
			SetPropertyValue(62, value);
		}
	}

	internal LimitLocationType IntegralLimitLocations
	{
		get
		{
			return (LimitLocationType)GetPropertyValue(63);
		}
		set
		{
			SetPropertyValue(63, value);
		}
	}

	internal int IntraEquationSpacing
	{
		get
		{
			return (int)GetPropertyValue(64);
		}
		set
		{
			SetPropertyValue(64, value);
		}
	}

	internal int LeftMargin
	{
		get
		{
			return (int)GetPropertyValue(65);
		}
		set
		{
			SetPropertyValue(65, value);
		}
	}

	internal string MathFont
	{
		get
		{
			return (string)GetPropertyValue(66);
		}
		set
		{
			SetPropertyValue(66, value);
		}
	}

	internal LimitLocationType NAryLimitLocation
	{
		get
		{
			return (LimitLocationType)GetPropertyValue(67);
		}
		set
		{
			SetPropertyValue(67, value);
		}
	}

	internal int PostParagraphSpacing
	{
		get
		{
			return (int)GetPropertyValue(68);
		}
		set
		{
			SetPropertyValue(68, value);
		}
	}

	internal int PreParagraphSpacing
	{
		get
		{
			return (int)GetPropertyValue(69);
		}
		set
		{
			SetPropertyValue(69, value);
		}
	}

	internal int RightMargin
	{
		get
		{
			return (int)GetPropertyValue(70);
		}
		set
		{
			SetPropertyValue(70, value);
		}
	}

	internal bool SmallFraction
	{
		get
		{
			return (bool)GetPropertyValue(71);
		}
		set
		{
			SetPropertyValue(71, value);
		}
	}

	internal int WrapIndent
	{
		get
		{
			return (int)GetPropertyValue(72);
		}
		set
		{
			SetPropertyValue(72, value);
		}
	}

	internal bool WrapRight
	{
		get
		{
			return (bool)GetPropertyValue(73);
		}
		set
		{
			SetPropertyValue(73, value);
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

	internal OfficeMathProperties()
	{
		IsDefault = true;
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
		return key switch
		{
			58 => BreakOnBinaryOperatorsType.Before, 
			59 => BreakOnBinarySubtractionType.MinusMinus, 
			60 => MathJustification.CenterGroup, 
			61 => true, 
			62 => 0, 
			63 => LimitLocationType.SubSuperscript, 
			64 => 0, 
			65 => 0, 
			66 => "CambriMath", 
			67 => LimitLocationType.UnderOver, 
			68 => 0, 
			69 => 0, 
			70 => 0, 
			71 => false, 
			72 => 1440, 
			73 => false, 
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

	internal OfficeMathProperties Clone()
	{
		return (OfficeMathProperties)MemberwiseClone();
	}
}
