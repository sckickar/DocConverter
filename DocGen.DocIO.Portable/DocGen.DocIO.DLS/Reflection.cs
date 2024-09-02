using System;
using System.Text;

namespace DocGen.DocIO.DLS;

internal class Reflection
{
	private float m_blurRadius;

	private int m_direction;

	private float m_distance;

	private float m_startOpacity;

	private float m_startPosition;

	private float m_endOpacity;

	private float m_endPosition;

	private int m_fadeDirection;

	private bool m_rotWithShape;

	private int m_horizontalSkew;

	private int m_verticalSkew;

	private float m_horizontalRatio;

	private float m_verticalRatio;

	private TextureAlignment m_refAlign;

	private ushort m_flag;

	private ShapeBase m_shape;

	private const byte BlurRadiusKey = 0;

	private const byte DirectionKey = 1;

	private const byte DistanceKey = 2;

	private const byte TranparencyKey = 3;

	private const byte StartPositionKey = 4;

	private const byte EndOpacityKey = 5;

	private const byte SizeKey = 6;

	private const byte FadeDirectionKey = 7;

	private const byte RotateWithShapekey = 8;

	private const byte HorizontalSkewkey = 9;

	private const byte VerticalSkewKey = 10;

	private const byte HorizontalRatioKey = 11;

	private const byte VerticalRatioKey = 12;

	private const byte AlignmentKey = 13;

	internal float Blur
	{
		get
		{
			return m_blurRadius;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xFFFEu) | 1u);
			m_blurRadius = value;
		}
	}

	internal int Direction
	{
		get
		{
			return m_direction;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xFFFDu) | 2u);
			m_direction = value;
		}
	}

	internal float Offset
	{
		get
		{
			return m_distance;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xFFFBu) | 4u);
			m_distance = value;
		}
	}

	internal float Transparency
	{
		get
		{
			return m_startOpacity;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xFFF7u) | 8u);
			m_startOpacity = value;
		}
	}

	internal float StartPosition
	{
		get
		{
			return m_startPosition;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xFFEFu) | 0x10u);
			m_startPosition = value;
		}
	}

	internal float EndOpacity
	{
		get
		{
			return m_endOpacity;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xFFDFu) | 0x20u);
			m_endOpacity = value;
		}
	}

	internal float Size
	{
		get
		{
			return m_endPosition;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xFFBFu) | 0x40u);
			m_endPosition = value;
		}
	}

	internal int FadeDirection
	{
		get
		{
			return m_fadeDirection;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xFF7Fu) | 0x80u);
			m_fadeDirection = value;
		}
	}

	internal int HorizontalSkew
	{
		get
		{
			return m_horizontalSkew;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xFEFFu) | 0x100u);
			m_horizontalSkew = value;
		}
	}

	internal int VerticalSkew
	{
		get
		{
			return m_verticalSkew;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xFDFFu) | 0x200u);
			m_verticalSkew = value;
		}
	}

	internal float HorizontalRatio
	{
		get
		{
			return m_horizontalRatio;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xFBFFu) | 0x400u);
			m_horizontalRatio = value;
		}
	}

	internal float VerticalRatio
	{
		get
		{
			return m_verticalRatio;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xF7FFu) | 0x800u);
			m_verticalRatio = value;
		}
	}

	internal bool RotateWithShape
	{
		get
		{
			return m_rotWithShape;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xEFFFu) | 0x1000u);
			m_rotWithShape = value;
		}
	}

	internal TextureAlignment Alignment
	{
		get
		{
			return m_refAlign;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xDFFFu) | 0x2000u);
			m_refAlign = value;
		}
	}

	internal Reflection(ShapeBase shape)
	{
		m_shape = shape;
	}

	internal bool HasKey(int propertyKey)
	{
		return (m_flag & (ushort)Math.Pow(2.0, propertyKey)) >> propertyKey != 0;
	}

	internal void Close()
	{
		if (m_shape != null)
		{
			m_shape = null;
		}
	}

	internal Reflection Clone()
	{
		return (Reflection)MemberwiseClone();
	}

	internal bool Compare(Reflection reflectionFormat)
	{
		if (RotateWithShape != reflectionFormat.RotateWithShape || Alignment != reflectionFormat.Alignment || Blur != reflectionFormat.Blur || Direction != reflectionFormat.Direction || Offset != reflectionFormat.Offset || Transparency != reflectionFormat.Transparency || StartPosition != reflectionFormat.StartPosition || EndOpacity != reflectionFormat.EndOpacity || Size != reflectionFormat.Size || FadeDirection != reflectionFormat.FadeDirection || HorizontalSkew != reflectionFormat.HorizontalSkew || VerticalSkew != reflectionFormat.VerticalSkew || HorizontalRatio != reflectionFormat.HorizontalRatio || VerticalRatio != reflectionFormat.VerticalRatio)
		{
			return false;
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(RotateWithShape ? "1" : "0;");
		stringBuilder.Append((int)Alignment + ";");
		stringBuilder.Append(Blur + ";");
		stringBuilder.Append(Direction + ";");
		stringBuilder.Append(Offset + ";");
		stringBuilder.Append(Transparency + ";");
		stringBuilder.Append(StartPosition + ";");
		stringBuilder.Append(EndOpacity + ";");
		stringBuilder.Append(Size + ";");
		stringBuilder.Append(FadeDirection + ";");
		stringBuilder.Append(HorizontalSkew + ";");
		stringBuilder.Append(VerticalSkew + ";");
		stringBuilder.Append(HorizontalRatio + ";");
		stringBuilder.Append(VerticalRatio + ";");
		return stringBuilder;
	}
}
