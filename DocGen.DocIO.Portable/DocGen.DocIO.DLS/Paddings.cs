using System;
using DocGen.DocIO.DLS.XML;

namespace DocGen.DocIO.DLS;

public class Paddings : FormatBase
{
	public const int LeftKey = 1;

	public const int TopKey = 2;

	public const int BottomKey = 3;

	public const int RightKey = 4;

	public float Left
	{
		get
		{
			return (float)base[1];
		}
		set
		{
			base[1] = value;
		}
	}

	public float Top
	{
		get
		{
			return (float)base[2];
		}
		set
		{
			base[2] = value;
		}
	}

	public float Right
	{
		get
		{
			return (float)base[4];
		}
		set
		{
			base[4] = value;
		}
	}

	public float Bottom
	{
		get
		{
			return (float)base[3];
		}
		set
		{
			base[3] = value;
		}
	}

	public float All
	{
		set
		{
			float num2 = (Bottom = value);
			float num4 = (Top = num2);
			float left = (Right = num4);
			Left = left;
		}
	}

	internal bool IsEmpty
	{
		get
		{
			if (Left == 0f && Right == 0f && Top == 0f)
			{
				return Bottom == 0f;
			}
			return false;
		}
	}

	internal Paddings(FormatBase parent, int baseKey)
		: base(parent, baseKey)
	{
	}

	internal Paddings()
	{
	}

	internal void UpdatePaddings(Paddings padding)
	{
		if (!padding.IsDefault)
		{
			Left = padding.Left;
			Right = padding.Right;
			Top = padding.Top;
			Bottom = padding.Bottom;
		}
	}

	internal void ImportPaddings(Paddings basePaddings)
	{
		if (basePaddings.HasKey(1))
		{
			Left = basePaddings.Left;
		}
		if (basePaddings.HasKey(4))
		{
			Right = basePaddings.Right;
		}
		if (basePaddings.HasKey(2))
		{
			Top = basePaddings.Top;
		}
		if (basePaddings.HasKey(3))
		{
			Bottom = basePaddings.Bottom;
		}
	}

	internal bool Compare(Paddings paddings)
	{
		if (!Compare(GetBaseKey(1), paddings))
		{
			return false;
		}
		if (!Compare(GetBaseKey(4), paddings))
		{
			return false;
		}
		if (!Compare(GetBaseKey(2), paddings))
		{
			return false;
		}
		if (!Compare(GetBaseKey(3), paddings))
		{
			return false;
		}
		return true;
	}

	internal Paddings Clone()
	{
		return new Paddings
		{
			Left = Left,
			Right = Right,
			Top = Top,
			Bottom = Bottom
		};
	}

	protected override object GetDefValue(int key)
	{
		return key switch
		{
			1 => 0f, 
			4 => 0f, 
			2 => 0f, 
			3 => 0f, 
			_ => throw new ArgumentException("key has invalid value"), 
		};
	}

	protected override void InitXDLSHolder()
	{
		if (base.IsDefault)
		{
			base.XDLSHolder.SkipMe = true;
		}
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		if (HasKey(1))
		{
			writer.WriteValue("Left", Left);
		}
		if (HasKey(4))
		{
			writer.WriteValue("Right", Right);
		}
		if (HasKey(3))
		{
			writer.WriteValue("Bottom", Bottom);
		}
		if (HasKey(2))
		{
			writer.WriteValue("Top", Top);
		}
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		if (reader.HasAttribute("Left"))
		{
			Left = reader.ReadFloat("Left");
		}
		if (reader.HasAttribute("Right"))
		{
			Right = reader.ReadFloat("Right");
		}
		if (reader.HasAttribute("Bottom"))
		{
			Bottom = reader.ReadFloat("Bottom");
		}
		if (reader.HasAttribute("Top"))
		{
			Top = reader.ReadFloat("Top");
		}
	}

	protected override void OnChange(FormatBase format, int propertyKey)
	{
		base.OnChange(format, propertyKey);
	}
}
