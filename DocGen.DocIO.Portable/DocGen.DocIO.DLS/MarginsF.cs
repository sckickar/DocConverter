using System;

namespace DocGen.DocIO.DLS;

public sealed class MarginsF : FormatBase
{
	internal const int LeftKey = 1;

	internal const int RightKey = 2;

	internal const int TopKey = 3;

	internal const int BottomKey = 4;

	internal const int GutterKey = 5;

	public float All
	{
		get
		{
			if (!IsAll)
			{
				return 0f;
			}
			return Left;
		}
		set
		{
			if (Left != value || !IsAll)
			{
				float num2 = (Bottom = value);
				float num4 = (Top = num2);
				float left = (Right = num4);
				Left = left;
			}
		}
	}

	public float Left
	{
		get
		{
			return (float)GetPropertyValue(1);
		}
		set
		{
			if (value != Left)
			{
				SetPropertyValue(1, value);
			}
		}
	}

	public float Right
	{
		get
		{
			return (float)GetPropertyValue(2);
		}
		set
		{
			if (value != Right)
			{
				SetPropertyValue(2, value);
			}
		}
	}

	public float Top
	{
		get
		{
			return (float)GetPropertyValue(3);
		}
		set
		{
			if (value != Top)
			{
				SetPropertyValue(3, value);
			}
		}
	}

	public float Bottom
	{
		get
		{
			return (float)GetPropertyValue(4);
		}
		set
		{
			if (value != Bottom)
			{
				SetPropertyValue(4, value);
			}
		}
	}

	private bool IsAll
	{
		get
		{
			if (Left == Right && Right == Top)
			{
				return Top == Bottom;
			}
			return false;
		}
	}

	internal float Gutter
	{
		get
		{
			return (float)GetPropertyValue(5);
		}
		set
		{
			if (value != Gutter)
			{
				SetPropertyValue(5, value);
			}
		}
	}

	public MarginsF()
	{
	}

	public MarginsF(float left, float top, float right, float bottom)
	{
		Left = left;
		Top = top;
		Right = right;
		Bottom = bottom;
	}

	public MarginsF Clone()
	{
		return new MarginsF(Left, Top, Right, Bottom);
	}

	protected override object GetDefValue(int key)
	{
		switch (key)
		{
		case 1:
		case 2:
		case 3:
		case 4:
			return 20f;
		case 5:
			return 0f;
		default:
			throw new ArgumentNullException("key not found");
		}
	}

	internal void SetOldPropertyHashMarginValues(float left, float top, float right, float bottom, float gutter)
	{
		SetPropertyValue(1, left);
		SetPropertyValue(2, right);
		SetPropertyValue(3, top);
		SetPropertyValue(4, bottom);
		SetPropertyValue(5, gutter);
	}

	internal object GetPropertyValue(int propKey)
	{
		return base[propKey];
	}

	internal void SetPropertyValue(int propKey, object value)
	{
		base[propKey] = value;
	}

	internal bool Compare(MarginsF marginsF)
	{
		if (!Compare(3, marginsF))
		{
			return false;
		}
		if (!Compare(4, marginsF))
		{
			return false;
		}
		if (!Compare(1, marginsF))
		{
			return false;
		}
		if (!Compare(2, marginsF))
		{
			return false;
		}
		if (!Compare(5, marginsF))
		{
			return false;
		}
		return true;
	}
}
