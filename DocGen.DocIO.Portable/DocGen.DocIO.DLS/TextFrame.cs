using System.Collections.Generic;
using System.Text;

namespace DocGen.DocIO.DLS;

public class TextFrame : OwnerHolder
{
	private bool m_bNoWrap;

	private bool m_spAutoFit;

	private bool m_normAutoFit;

	private bool m_noAutoFit;

	private TextDirection m_TextDirection;

	private byte m_flag;

	private Shape m_shape;

	private ChildShape m_childShape;

	private float m_widthRelativePercent;

	private float m_heightRelativePercent;

	private WidthOrigin m_widthRelation = WidthOrigin.Page;

	private HeightOrigin m_heightRelation = HeightOrigin.Page;

	protected Dictionary<int, object> m_propertiesHash;

	internal const byte ShapeAutoFitKey = 0;

	internal const byte NoAutoFitKey = 1;

	internal const byte NormalAutoFitKey = 2;

	private VerticalAlignment m_TextVerticalAlignment;

	private float m_HorizontalRelativePercent = float.MinValue;

	private float m_VerticalRelativePercent = float.MinValue;

	internal InternalMargin m_intMargin;

	internal bool HasInternalMargin
	{
		get
		{
			return (m_flag & 1) != 0;
		}
		set
		{
			m_flag = (byte)((m_flag & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal bool Upright
	{
		get
		{
			return (m_flag & 4) >> 2 != 0;
		}
		set
		{
			m_flag = (byte)((m_flag & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	public TextDirection TextDirection
	{
		get
		{
			return m_TextDirection;
		}
		set
		{
			m_TextDirection = value;
		}
	}

	public VerticalAlignment TextVerticalAlignment
	{
		get
		{
			return m_TextVerticalAlignment;
		}
		set
		{
			m_TextVerticalAlignment = value;
		}
	}

	internal float WidthRelativePercent
	{
		get
		{
			return m_widthRelativePercent;
		}
		set
		{
			m_widthRelativePercent = value;
		}
	}

	internal float HeightRelativePercent
	{
		get
		{
			return m_heightRelativePercent;
		}
		set
		{
			m_heightRelativePercent = value;
		}
	}

	internal WidthOrigin WidthOrigin
	{
		get
		{
			return m_widthRelation;
		}
		set
		{
			m_widthRelation = value;
		}
	}

	internal HeightOrigin HeightOrigin
	{
		get
		{
			return m_heightRelation;
		}
		set
		{
			m_heightRelation = value;
		}
	}

	internal float HorizontalRelativePercent
	{
		get
		{
			return m_HorizontalRelativePercent;
		}
		set
		{
			m_HorizontalRelativePercent = value;
		}
	}

	internal float VerticalRelativePercent
	{
		get
		{
			return m_VerticalRelativePercent;
		}
		set
		{
			m_VerticalRelativePercent = value;
		}
	}

	public InternalMargin InternalMargin
	{
		get
		{
			if (m_intMargin == null)
			{
				m_intMargin = new InternalMargin();
				m_intMargin.SetOwner(base.OwnerBase);
			}
			return m_intMargin;
		}
	}

	internal bool NoWrap
	{
		get
		{
			return m_bNoWrap;
		}
		set
		{
			m_bNoWrap = value;
		}
	}

	internal bool NoAutoFit
	{
		get
		{
			if (HasKey(1))
			{
				return (bool)PropertiesHash[1];
			}
			return m_noAutoFit;
		}
		set
		{
			m_noAutoFit = value;
			SetKeyValue(1, value);
		}
	}

	internal bool NormalAutoFit
	{
		get
		{
			if (HasKey(2))
			{
				return (bool)PropertiesHash[2];
			}
			return m_normAutoFit;
		}
		set
		{
			m_normAutoFit = value;
			SetKeyValue(2, value);
		}
	}

	internal bool ShapeAutoFit
	{
		get
		{
			if (HasKey(0))
			{
				return (bool)PropertiesHash[0];
			}
			return m_spAutoFit;
		}
		set
		{
			m_spAutoFit = value;
			SetKeyValue(0, value);
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

	internal bool HasKey(int Key)
	{
		if (m_propertiesHash != null && m_propertiesHash.ContainsKey(Key))
		{
			return true;
		}
		return false;
	}

	private void SetKeyValue(int propKey, object value)
	{
		this[propKey] = value;
	}

	internal TextFrame(Shape shape)
	{
		m_shape = shape;
	}

	internal TextFrame(ChildShape childShape)
	{
		m_childShape = childShape;
	}

	internal bool Compare(TextFrame textFrame)
	{
		if (HasInternalMargin != textFrame.HasInternalMargin || Upright != textFrame.Upright || NoWrap != textFrame.NoWrap || NoAutoFit != textFrame.NoAutoFit || NormalAutoFit != textFrame.NormalAutoFit || ShapeAutoFit != textFrame.ShapeAutoFit || TextDirection != textFrame.TextDirection || TextVerticalAlignment != textFrame.TextVerticalAlignment || WidthOrigin != textFrame.WidthOrigin || HeightOrigin != textFrame.HeightOrigin || WidthRelativePercent != textFrame.WidthRelativePercent || HeightRelativePercent != textFrame.HeightRelativePercent || HorizontalRelativePercent != textFrame.HorizontalRelativePercent || VerticalRelativePercent != textFrame.VerticalRelativePercent)
		{
			return false;
		}
		if ((InternalMargin == null && textFrame.InternalMargin != null) || (InternalMargin != null && textFrame.InternalMargin == null))
		{
			return false;
		}
		if (InternalMargin != null && !InternalMargin.Compare(textFrame.InternalMargin))
		{
			return false;
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = (HasInternalMargin ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (Upright ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (NoWrap ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (NoAutoFit ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (NormalAutoFit ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (ShapeAutoFit ? "1" : "0");
		stringBuilder.Append(text + ";");
		stringBuilder.Append((int)TextDirection + ";");
		stringBuilder.Append((int)TextVerticalAlignment + ";");
		stringBuilder.Append((int)WidthOrigin + ";");
		stringBuilder.Append((int)HeightOrigin + ";");
		stringBuilder.Append(WidthRelativePercent + ";");
		stringBuilder.Append(HeightRelativePercent + ";");
		stringBuilder.Append(HorizontalRelativePercent + ";");
		stringBuilder.Append(VerticalRelativePercent + ";");
		if (InternalMargin != null)
		{
			stringBuilder.Append(InternalMargin.GetAsString());
		}
		return stringBuilder;
	}
}
