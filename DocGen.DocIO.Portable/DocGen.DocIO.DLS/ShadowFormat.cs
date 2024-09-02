using System;
using System.Text;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

internal class ShadowFormat
{
	private float m_shadowOffsetX;

	private float m_nonchoiceshadowOffsetX;

	private float m_nonchoiceshadowOffsetY;

	private float m_shadowOffsetY;

	private float m_shadowOffset2X;

	private float m_shadowOffset2Y;

	private float m_originX;

	private float m_originY;

	private string m_shadowPerspectiveMatrix;

	private short m_horizontalSkewAngle;

	private short m_verticalSkewAngle;

	private double m_horizontalScalingFactor;

	private double m_verticalScalingFactor;

	private bool m_rotateWithShape;

	private double m_direction;

	private double m_distance;

	private double m_blurRadius;

	private ShadowAlignment m_align;

	private Color m_color;

	private Color m_backColor;

	private string m_name;

	private ShadowType m_shadowType;

	private bool m_visible;

	private float m_transparency;

	private bool m_obscured;

	private ShapeBase m_shape;

	private ushort m_flagsA;

	private byte m_flagsB;

	internal string m_type;

	internal const byte ShadowOffsetXKey = 0;

	internal const byte ShadowOffsetYKey = 1;

	internal const byte ShadowOffset2XKey = 2;

	internal const byte ShadowOffset2YKey = 3;

	internal const byte OriginXKey = 4;

	internal const byte OriginYKey = 5;

	internal const byte ShadowPerspectiveMatrixKey = 6;

	internal const byte HorizontalSkewAngleKey = 7;

	internal const byte VerticalSkewAngleKey = 8;

	internal const byte HorizontalScalingFactorKey = 9;

	internal const byte VerticalScalingFactorKey = 10;

	internal const byte RotateWithShapeKey = 11;

	internal const byte DirectionKey = 12;

	internal const byte DistanceKey = 13;

	internal const byte BlurKey = 14;

	internal const byte AlignmentKey = 15;

	internal const byte ColorKey = 16;

	internal const byte Color2Key = 17;

	internal const byte NameKey = 18;

	internal const byte ShadowTypeKey = 19;

	internal const byte VisibleKey = 20;

	internal const byte TransparencyKey = 21;

	internal const byte ObscuredKey = 22;

	internal float ShadowOffsetX
	{
		get
		{
			return m_shadowOffsetX;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFFFEu) | 1u);
			m_shadowOffsetX = value;
		}
	}

	internal float NonChoiceShadowOffsetX
	{
		get
		{
			return m_nonchoiceshadowOffsetX;
		}
		set
		{
			m_nonchoiceshadowOffsetX = value;
		}
	}

	internal float ShadowOffsetY
	{
		get
		{
			return m_shadowOffsetY;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFFFDu) | 2u);
			m_shadowOffsetY = value;
		}
	}

	internal float NonChoiceShadowOffsetY
	{
		get
		{
			return m_nonchoiceshadowOffsetY;
		}
		set
		{
			m_nonchoiceshadowOffsetY = value;
		}
	}

	internal float ShadowOffset2X
	{
		get
		{
			return m_shadowOffset2X;
		}
		set
		{
			m_flagsA = (byte)((m_flagsA & 0xFFFBu) | 4u);
			m_shadowOffset2X = value;
		}
	}

	internal float ShadowOffset2Y
	{
		get
		{
			return m_shadowOffset2Y;
		}
		set
		{
			m_flagsA = (byte)((m_flagsA & 0xFFF7u) | 8u);
			m_shadowOffset2Y = value;
		}
	}

	internal float OriginX
	{
		get
		{
			return m_originX;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFFEFu) | 0x10u);
			m_originX = value;
		}
	}

	internal float OriginY
	{
		get
		{
			return m_originY;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFFDFu) | 0x20u);
			m_originY = value;
		}
	}

	internal string ShadowPerspectiveMatrix
	{
		get
		{
			return m_shadowPerspectiveMatrix;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFFBFu) | 0x40u);
			m_shadowPerspectiveMatrix = value;
		}
	}

	internal short HorizontalSkewAngle
	{
		get
		{
			return m_horizontalSkewAngle;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFF7Fu) | 0x80u);
			m_horizontalSkewAngle = value;
		}
	}

	internal short VerticalSkewAngle
	{
		get
		{
			return m_verticalSkewAngle;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xFEFFu) | 0x100u);
			m_verticalSkewAngle = value;
		}
	}

	internal double HorizontalScalingFactor
	{
		get
		{
			return m_horizontalScalingFactor;
		}
		set
		{
			if (value < -5400000.0 || value > 5400000.0)
			{
				throw new ArgumentOutOfRangeException("Horizontal Skew angle must be between -90 and 90 degrees");
			}
			m_flagsA = (ushort)((m_flagsA & 0xFDFFu) | 0x200u);
			m_horizontalScalingFactor = value;
		}
	}

	internal double VerticalScalingFactor
	{
		get
		{
			return m_verticalScalingFactor;
		}
		set
		{
			if (value < -5400000.0 || value > 5400000.0)
			{
				throw new ArgumentOutOfRangeException("Vertical Skew angle must be between -90 and 90 degrees");
			}
			m_flagsA = (ushort)((m_flagsA & 0xFBFFu) | 0x400u);
			m_verticalScalingFactor = value;
		}
	}

	internal bool RotateWithShape
	{
		get
		{
			return m_rotateWithShape;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0xF7FFu) | 0x800u);
			m_rotateWithShape = value;
		}
	}

	internal double Direction
	{
		get
		{
			return m_direction;
		}
		set
		{
			if (value < 0.0 || value > 21600000.0)
			{
				throw new ArgumentOutOfRangeException("Angle must be between 0 and 360");
			}
			m_flagsA = (ushort)((m_flagsA & 0xEFFFu) | 0x1000u);
			m_direction = value;
		}
	}

	internal double Distance
	{
		get
		{
			return m_distance;
		}
		set
		{
			if (value < 0.0 || value > 2147483647.0)
			{
				throw new ArgumentOutOfRangeException("Distance must be between 0 and 100");
			}
			m_flagsA = (ushort)((m_flagsA & 0xDFFFu) | 0x2000u);
			m_distance = value;
		}
	}

	internal double Blur
	{
		get
		{
			return m_blurRadius;
		}
		set
		{
			if (value < 0.0 || value > 2147483647.0)
			{
				throw new ArgumentOutOfRangeException("Blur radius value must be between 0 and 100");
			}
			m_flagsA = (ushort)((m_flagsA & 0xBFFFu) | 0x4000u);
			m_blurRadius = value;
		}
	}

	internal ShadowAlignment Alignment
	{
		get
		{
			return m_align;
		}
		set
		{
			m_flagsA = (ushort)((m_flagsA & 0x7FFFu) | 0x8000u);
			m_align = value;
		}
	}

	internal Color Color
	{
		get
		{
			return m_color;
		}
		set
		{
			m_flagsB = (byte)((m_flagsB & 0xFEu) | 1u);
			m_color = value;
		}
	}

	internal Color Color2
	{
		get
		{
			return m_backColor;
		}
		set
		{
			m_flagsB = (byte)((m_flagsB & 0xFDu) | 2u);
			m_backColor = value;
		}
	}

	internal string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_flagsB = (byte)((m_flagsB & 0xFBu) | 4u);
			m_name = value;
		}
	}

	internal ShadowType ShadowType
	{
		get
		{
			return m_shadowType;
		}
		set
		{
			m_flagsB = (byte)((m_flagsB & 0xF7u) | 8u);
			m_shadowType = value;
		}
	}

	internal bool Visible
	{
		get
		{
			return m_visible;
		}
		set
		{
			m_flagsB = (byte)((m_flagsB & 0xEFu) | 0x10u);
			m_visible = value;
		}
	}

	internal float Transparency
	{
		get
		{
			return m_transparency;
		}
		set
		{
			m_flagsB = (byte)((m_flagsB & 0xDFu) | 0x20u);
			m_transparency = value;
		}
	}

	internal bool Obscured
	{
		get
		{
			return m_obscured;
		}
		set
		{
			m_flagsB = (byte)((m_flagsB & 0xBFu) | 0x40u);
			m_obscured = value;
		}
	}

	internal ShadowFormat(ShapeBase shape)
	{
		m_shape = shape;
		m_color = Color.White;
		m_shadowOffsetX = 25400f;
		m_shadowOffsetY = 25400f;
		m_shadowOffset2X = -25400f;
		m_shadowOffset2Y = -25400f;
		m_visible = true;
		m_horizontalScalingFactor = 100.0;
		m_verticalScalingFactor = 100.0;
	}

	internal void Close()
	{
		if (m_shape != null)
		{
			m_shape = null;
		}
	}

	internal bool HasKey(int propertyKey)
	{
		if (propertyKey < 16)
		{
			return (m_flagsA & (ushort)Math.Pow(2.0, propertyKey)) >> propertyKey != 0;
		}
		return (m_flagsB & (byte)Math.Pow(2.0, propertyKey - 16)) >> propertyKey - 16 != 0;
	}

	internal ShadowFormat Clone()
	{
		return (ShadowFormat)MemberwiseClone();
	}

	internal bool Compare(ShadowFormat shadowFormat)
	{
		if (RotateWithShape != shadowFormat.RotateWithShape || Visible != shadowFormat.Visible || Alignment != shadowFormat.Alignment || ShadowType != shadowFormat.ShadowType || ShadowOffsetX != shadowFormat.ShadowOffsetX || ShadowOffsetY != shadowFormat.ShadowOffsetY || ShadowOffset2X != shadowFormat.ShadowOffset2X || ShadowOffset2Y != shadowFormat.ShadowOffset2Y || OriginX != shadowFormat.OriginX || OriginY != shadowFormat.OriginY || ShadowPerspectiveMatrix != shadowFormat.ShadowPerspectiveMatrix || HorizontalSkewAngle != shadowFormat.HorizontalSkewAngle || VerticalSkewAngle != shadowFormat.VerticalSkewAngle || HorizontalScalingFactor != shadowFormat.HorizontalScalingFactor || VerticalScalingFactor != shadowFormat.VerticalScalingFactor || Direction != shadowFormat.Direction || Distance != shadowFormat.Distance || Blur != shadowFormat.Blur || Transparency != shadowFormat.Transparency || Color.ToArgb() != shadowFormat.Color.ToArgb() || Color2.ToArgb() != shadowFormat.Color2.ToArgb())
		{
			return false;
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(RotateWithShape ? "1" : "0;");
		stringBuilder.Append(Visible ? "1" : "0;");
		stringBuilder.Append((int)Alignment + ";");
		stringBuilder.Append((int)ShadowType + ";");
		stringBuilder.Append(ShadowOffsetX + ";");
		stringBuilder.Append(ShadowOffsetY + ";");
		stringBuilder.Append(ShadowOffset2X + ";");
		stringBuilder.Append(ShadowOffset2Y + ";");
		stringBuilder.Append(OriginX + ";");
		stringBuilder.Append(OriginY + ";");
		stringBuilder.Append(ShadowPerspectiveMatrix + ";");
		stringBuilder.Append(HorizontalSkewAngle + ";");
		stringBuilder.Append(VerticalSkewAngle + ";");
		stringBuilder.Append(HorizontalScalingFactor + ";");
		stringBuilder.Append(VerticalScalingFactor + ";");
		stringBuilder.Append(Direction + ";");
		stringBuilder.Append(Distance + ";");
		stringBuilder.Append(Blur + ";");
		stringBuilder.Append(Transparency + ";");
		stringBuilder.Append(Color.ToArgb() + ";");
		stringBuilder.Append(Color2.ToArgb() + ";");
		return stringBuilder;
	}
}
