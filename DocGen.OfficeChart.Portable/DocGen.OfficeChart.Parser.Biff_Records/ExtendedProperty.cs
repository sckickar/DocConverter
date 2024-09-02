using System;
using System.Collections.Generic;

namespace DocGen.OfficeChart.Parser.Biff_Records;

internal class ExtendedProperty
{
	public const int MaxTintValue = 32767;

	private ushort m_usType;

	private ushort m_propSize;

	private ushort m_colorType = 2;

	private uint m_colorValue;

	private double m_tintAndShade;

	private long reserved;

	private ushort m_fontScheme;

	private ushort m_textIndentationLevel;

	private uint m_gradientType;

	private long m_iAngle;

	private long m_fillToRectLeft;

	private long m_fillToRectRight;

	private long m_fillToRectTop;

	private long m_fillToRectBottom;

	private uint m_gradStopCount;

	private int m_gradColorValue;

	private long m_gradPostition;

	private long m_gradTint;

	private List<GradStops> m_gradstops;

	public CellPropertyExtensionType Type
	{
		get
		{
			return (CellPropertyExtensionType)m_usType;
		}
		set
		{
			m_usType = (byte)value;
		}
	}

	public ushort Size
	{
		get
		{
			return m_propSize;
		}
		set
		{
			m_propSize = value;
		}
	}

	public ColorType ColorType
	{
		get
		{
			return (ColorType)m_colorType;
		}
		set
		{
			m_colorType = (byte)value;
		}
	}

	public uint ColorValue
	{
		get
		{
			return m_colorValue;
		}
		set
		{
			m_colorValue = value;
		}
	}

	public double Tint
	{
		get
		{
			return m_tintAndShade;
		}
		set
		{
			m_tintAndShade = value;
		}
	}

	public long Reserved
	{
		get
		{
			return reserved;
		}
		set
		{
			reserved = value;
		}
	}

	public FontScheme FontScheme
	{
		get
		{
			return (FontScheme)m_fontScheme;
		}
		set
		{
			m_fontScheme = (byte)value;
		}
	}

	public ushort Indent
	{
		get
		{
			return m_textIndentationLevel;
		}
		set
		{
			if (m_textIndentationLevel > 250)
			{
				throw new ArgumentOutOfRangeException("Indent level", "Text indentation level must be less than or equal to 250");
			}
			m_textIndentationLevel = value;
		}
	}

	public List<GradStops> GradStops
	{
		get
		{
			return m_gradstops;
		}
		set
		{
			m_gradstops = value;
		}
	}

	public ExtendedProperty()
	{
		m_gradstops = new List<GradStops>();
	}

	public int ParseExtendedProperty(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_usType = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_propSize = provider.ReadUInt16(iOffset);
		iOffset += 2;
		if (Type == CellPropertyExtensionType.GradientFill)
		{
			iOffset = ParseGradient(provider, iOffset, version);
		}
		else if (Type == CellPropertyExtensionType.FontScheme)
		{
			switch (m_propSize - 4)
			{
			case 1:
				m_fontScheme = provider.ReadByte(iOffset);
				iOffset++;
				break;
			case 2:
				m_fontScheme = provider.ReadUInt16(iOffset);
				iOffset += 2;
				break;
			}
		}
		else if (Type == CellPropertyExtensionType.TextIndentationLevel)
		{
			m_textIndentationLevel = provider.ReadUInt16(iOffset);
			iOffset += 2;
		}
		else
		{
			iOffset = ParseFullColor(provider, iOffset, version);
		}
		return iOffset;
	}

	public int InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usType);
		iOffset += 2;
		provider.WriteUInt16(iOffset, Size);
		iOffset += 2;
		if (Type == CellPropertyExtensionType.GradientFill)
		{
			iOffset = SerializeGradient(provider, iOffset, version);
		}
		else if (Type == CellPropertyExtensionType.FontScheme)
		{
			switch (m_propSize - 4)
			{
			case 1:
				provider.WriteByte(iOffset, (byte)m_fontScheme);
				iOffset++;
				break;
			case 2:
				provider.WriteUInt16(iOffset, m_fontScheme);
				iOffset += 2;
				break;
			}
		}
		else if (Type == CellPropertyExtensionType.TextIndentationLevel)
		{
			provider.WriteUInt16(iOffset, m_textIndentationLevel);
			iOffset += 2;
		}
		else
		{
			iOffset = SerializeFullColor(provider, iOffset, version);
		}
		return iOffset;
	}

	public int ParseFullColor(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_colorType = provider.ReadByte(iOffset);
		iOffset += 2;
		m_tintAndShade = provider.ReadInt16(iOffset);
		iOffset += 2;
		m_colorValue = provider.ReadUInt32(iOffset);
		iOffset += 4;
		reserved = provider.ReadInt64(iOffset);
		iOffset += 8;
		return iOffset;
	}

	public int SerializeFullColor(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_colorType);
		iOffset += 2;
		if (m_usType == 4 && m_colorType == 3)
		{
			m_tintAndShade *= 32767.0;
		}
		provider.WriteUInt16(iOffset, (ushort)m_tintAndShade);
		iOffset += 2;
		provider.WriteUInt32(iOffset, m_colorValue);
		iOffset += 4;
		provider.WriteInt64(iOffset, reserved);
		iOffset += 8;
		return iOffset;
	}

	public int ParseGradient(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_gradientType = provider.ReadByte(iOffset);
		iOffset += 4;
		m_iAngle = provider.ReadInt64(iOffset);
		iOffset += 8;
		m_fillToRectLeft = provider.ReadInt64(iOffset);
		iOffset += 8;
		m_fillToRectRight = provider.ReadInt64(iOffset);
		iOffset += 8;
		m_fillToRectTop = provider.ReadInt64(iOffset);
		iOffset += 8;
		m_fillToRectBottom = provider.ReadInt64(iOffset);
		iOffset += 8;
		m_gradStopCount = provider.ReadUInt32(iOffset);
		iOffset += 4;
		for (int i = 0; i < m_gradStopCount; i++)
		{
			GradStops gradStops = new GradStops();
			iOffset = gradStops.ParseGradStops(provider, iOffset, version);
			m_gradstops.Add(gradStops);
		}
		return iOffset;
	}

	public int SerializeGradient(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteByte(iOffset, (byte)m_gradientType);
		iOffset += 4;
		provider.WriteInt64(iOffset, m_iAngle);
		iOffset += 8;
		provider.WriteInt64(iOffset, m_fillToRectLeft);
		iOffset += 8;
		provider.WriteInt64(iOffset, m_fillToRectRight);
		iOffset += 8;
		provider.WriteInt64(iOffset, m_fillToRectTop);
		iOffset += 8;
		provider.WriteInt64(iOffset, m_fillToRectBottom);
		iOffset += 8;
		provider.WriteUInt32(iOffset, m_gradStopCount);
		iOffset += 4;
		foreach (GradStops gradStop in GradStops)
		{
			iOffset = gradStop.InfillInternalData(provider, iOffset, version);
		}
		return iOffset;
	}
}
