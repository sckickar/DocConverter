using System;
using System.Collections.Generic;
using System.Text;

namespace DocGen.DocIO.DLS;

public class InternalMargin : OwnerHolder
{
	internal const float DEF_HORIZMARGIN = 7.087f;

	internal const float DEF_VERTMARGIN = 3.685f;

	internal const byte LeftKey = 0;

	internal const byte RightKey = 1;

	internal const byte TopKey = 2;

	internal const byte BottomKey = 3;

	internal float m_intLeftMarg;

	internal float m_intRightMarg;

	internal float m_intTopMarg;

	internal float m_intBottomMarg;

	protected Dictionary<int, object> m_propertiesHash;

	public float Left
	{
		get
		{
			if (HasKey(0))
			{
				return (float)PropertiesHash[0];
			}
			return m_intLeftMarg;
		}
		set
		{
			if (value < 0f)
			{
				throw new ArgumentOutOfRangeException("Left", "Internal left margin must be higher than 0");
			}
			IsShapeSupportInternalMargin();
			if (base.Document != null && !base.Document.IsOpening && base.OwnerBase is WTextBox && (base.OwnerBase as WTextBox).IsShape)
			{
				(base.OwnerBase as WTextBox).Shape.TextFrame.InternalMargin.Left = value;
			}
			m_intLeftMarg = value;
			SetKeyValue(0, value);
		}
	}

	public float Right
	{
		get
		{
			if (HasKey(1))
			{
				return (float)PropertiesHash[1];
			}
			return m_intRightMarg;
		}
		set
		{
			if (value < 0f)
			{
				throw new ArgumentOutOfRangeException("Right", "Internal right margin must be higher than 0");
			}
			IsShapeSupportInternalMargin();
			if (base.Document != null && !base.Document.IsOpening && base.OwnerBase is WTextBox && (base.OwnerBase as WTextBox).IsShape)
			{
				(base.OwnerBase as WTextBox).Shape.TextFrame.InternalMargin.Right = value;
			}
			m_intRightMarg = value;
			SetKeyValue(1, value);
		}
	}

	public float Top
	{
		get
		{
			if (HasKey(2))
			{
				return (float)PropertiesHash[2];
			}
			return m_intTopMarg;
		}
		set
		{
			if (value < 0f)
			{
				throw new ArgumentOutOfRangeException("Top", "Internal top margin must be higher than 0");
			}
			IsShapeSupportInternalMargin();
			if (base.Document != null && !base.Document.IsOpening && base.OwnerBase is WTextBox && (base.OwnerBase as WTextBox).IsShape)
			{
				(base.OwnerBase as WTextBox).Shape.TextFrame.InternalMargin.Top = value;
			}
			m_intTopMarg = value;
			SetKeyValue(2, value);
		}
	}

	public float Bottom
	{
		get
		{
			if (HasKey(3))
			{
				return (float)PropertiesHash[3];
			}
			return m_intBottomMarg;
		}
		set
		{
			if (value < 0f)
			{
				throw new ArgumentOutOfRangeException("Bottom", "Internal bottom margin must be higher than 0");
			}
			IsShapeSupportInternalMargin();
			if (base.Document != null && !base.Document.IsOpening && base.OwnerBase is WTextBox && (base.OwnerBase as WTextBox).IsShape)
			{
				(base.OwnerBase as WTextBox).Shape.TextFrame.InternalMargin.Bottom = value;
			}
			m_intBottomMarg = value;
			SetKeyValue(3, value);
		}
	}

	protected object this[int key]
	{
		get
		{
			return key;
		}
		set
		{
			PropertiesHash[key] = value;
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

	public InternalMargin()
	{
		m_intLeftMarg = 7.087f;
		m_intRightMarg = 7.087f;
		m_intTopMarg = 3.685f;
		m_intBottomMarg = 3.685f;
	}

	private void IsShapeSupportInternalMargin()
	{
		if (base.Document != null && !base.Document.IsOpening && base.OwnerBase is Shape && ((base.OwnerBase as Shape).AutoShapeType == AutoShapeType.Line || (base.OwnerBase as Shape).AutoShapeType == AutoShapeType.ElbowConnector || (base.OwnerBase as Shape).AutoShapeType == AutoShapeType.CurvedConnector || (base.OwnerBase as Shape).AutoShapeType == AutoShapeType.StraightConnector || (base.OwnerBase as Shape).AutoShapeType == AutoShapeType.BentConnector2 || (base.OwnerBase as Shape).AutoShapeType == AutoShapeType.BentConnector4 || (base.OwnerBase as Shape).AutoShapeType == AutoShapeType.BentConnector5 || (base.OwnerBase as Shape).AutoShapeType == AutoShapeType.CurvedConnector2 || (base.OwnerBase as Shape).AutoShapeType == AutoShapeType.CurvedConnector4 || (base.OwnerBase as Shape).AutoShapeType == AutoShapeType.CurvedConnector5))
		{
			throw new Exception("This property is not valid for " + (base.OwnerBase as Shape).AutoShapeType.ToString() + " type");
		}
	}

	internal bool HasKey(int Key)
	{
		if (m_propertiesHash != null && m_propertiesHash.ContainsKey(Key))
		{
			return true;
		}
		return false;
	}

	internal void SetKeyValue(int propKey, object value)
	{
		this[propKey] = value;
	}

	internal void SetDefaultMargins()
	{
		m_intLeftMarg = 7.2f;
		m_intRightMarg = 7.2f;
		m_intBottomMarg = 3.6f;
		m_intTopMarg = 3.6f;
	}

	internal InternalMargin Clone()
	{
		return (InternalMargin)MemberwiseClone();
	}

	internal bool Compare(InternalMargin internalMargin)
	{
		if (Left != internalMargin.Left || Right != internalMargin.Right || Top != internalMargin.Top || Bottom != internalMargin.Bottom)
		{
			return false;
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(Left + ";");
		stringBuilder.Append(Right + ";");
		stringBuilder.Append(Top + ";");
		stringBuilder.Append(Bottom + ";");
		return stringBuilder;
	}
}
