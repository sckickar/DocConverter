using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Security;
using DocGen.Drawing;

namespace DocGen.Chart;

[ImmutableObject(true)]
[MergableProperty(true)]
[RefreshProperties(RefreshProperties.Repaint)]
internal sealed class ChartBorder : ISerializable
{
	private ChartBorderStyle _style;

	private ChartBorderWeight _weight;

	private Color _color;

	private static Color Black = Color.Black;

	private const char separator = ';';

	private static char[] separators = new char[1] { ';' };

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool IsEmpty => this == new ChartBorder();

	[MergableProperty(true)]
	[NotifyParentProperty(true)]
	public ChartBorderStyle Style => _style;

	[MergableProperty(true)]
	[NotifyParentProperty(true)]
	public ChartBorderWeight Weight => _weight;

	[MergableProperty(true)]
	[NotifyParentProperty(true)]
	public Color Color => _color;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int Width
	{
		get
		{
			int result = 1;
			switch (Weight)
			{
			case ChartBorderWeight.Medium:
				result = 2;
				break;
			case ChartBorderWeight.Thick:
				result = 4;
				break;
			case ChartBorderWeight.Thin:
				result = 1;
				break;
			}
			return result;
		}
	}

	public object Clone()
	{
		return MemberwiseClone();
	}

	public ChartBorder()
		: this(ChartBorderStyle.NotSet, Black, ChartBorderWeight.Thin)
	{
	}

	public ChartBorder(ChartBorderStyle style)
		: this(style, Black, ChartBorderWeight.Thin)
	{
	}

	public ChartBorder(ChartBorderStyle style, Color color)
		: this(style, color, ChartBorderWeight.Thin)
	{
	}

	public ChartBorder(ChartBorderStyle style, Color color, ChartBorderWeight weight)
	{
		if (!Enum.IsDefined(typeof(ChartBorderStyle), style))
		{
			throw new ArgumentException("Invalid ChartBorderStyle value.");
		}
		_style = style;
		if (!Enum.IsDefined(typeof(ChartBorderWeight), weight))
		{
			throw new ArgumentException("Invalid ChartBorderWeight value.");
		}
		_weight = weight;
		_color = color;
	}

	private ChartBorder(SerializationInfo info, StreamingContext context)
	{
		_style = (ChartBorderStyle)info.GetValue("Style", typeof(ChartBorderStyle));
		_weight = (ChartBorderWeight)info.GetValue("Weight", typeof(ChartBorderWeight));
		_color = (Color)info.GetValue("Color", typeof(Color));
	}

	[SecurityCritical]
	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Style", _style);
		info.AddValue("Weight", _weight);
		info.AddValue("Color", _color);
	}

	public ChartBorder MakeBlackAndWhite()
	{
		return new ChartBorder(Style, Color.Black, Weight);
	}

	internal void Dispose()
	{
		separators = null;
	}

	public override string ToString()
	{
		return base.ToString();
	}

	public string ToString(string format)
	{
		return base.ToString();
	}

	public override bool Equals(object o)
	{
		bool result = false;
		if (this == o)
		{
			return true;
		}
		if (o is ChartBorder)
		{
			ChartBorder chartBorder = (ChartBorder)o;
			result = chartBorder.Color == Color && chartBorder.Style == Style && chartBorder.Weight == Weight;
		}
		return result;
	}

	public static bool operator ==(ChartBorder lhs, ChartBorder rhs)
	{
		if ((object)lhs == null || (object)rhs == null)
		{
			return false;
		}
		return lhs.Equals(rhs);
	}

	public static bool operator !=(ChartBorder lhs, ChartBorder rhs)
	{
		if ((object)lhs == null || (object)rhs == null)
		{
			return false;
		}
		return !lhs.Equals(rhs);
	}

	public static bool Compare(ChartBorder lhs, ChartBorder rhs)
	{
		if ((object)lhs == null || (object)rhs == null)
		{
			return false;
		}
		return lhs.Equals(rhs);
	}

	public override int GetHashCode()
	{
		return ((int)Style << 28) | ((int)Weight << 24) | (Color.ToArgb() & 0xFFFFF);
	}
}
