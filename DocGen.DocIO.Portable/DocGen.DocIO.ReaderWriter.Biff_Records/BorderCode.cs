using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.DocIO.DLS;
using DocGen.Drawing;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class BorderCode : BaseWordRecord
{
	private int DEF_NEW_BRC_LENGTH = 8;

	private byte m_dptLineWidth;

	private byte m_brcType;

	private byte m_colorId;

	private Color m_colorExt = Color.Empty;

	private byte m_props;

	private byte m_bFlags = 1;

	internal byte LineWidth
	{
		get
		{
			return m_dptLineWidth;
		}
		set
		{
			m_dptLineWidth = value;
			IsDefault = false;
		}
	}

	internal byte BorderType
	{
		get
		{
			return m_brcType;
		}
		set
		{
			m_brcType = value;
			IsDefault = false;
		}
	}

	internal byte Space
	{
		get
		{
			return (byte)(m_props & 0x1Fu);
		}
		set
		{
			byte b = value;
			m_props &= 224;
			m_props += b;
			IsDefault = false;
		}
	}

	internal bool Shadow
	{
		get
		{
			return (byte)((byte)(m_props & 0x20) >> 5) == 1;
		}
		set
		{
			byte b = (value ? ((byte)1) : ((byte)0));
			m_props &= 223;
			b <<= 5;
			m_props += b;
			IsDefault = false;
		}
	}

	internal byte LineColor
	{
		get
		{
			return m_colorId;
		}
		set
		{
			m_colorId = value;
			IsDefault = false;
		}
	}

	internal Color LineColorExt
	{
		get
		{
			if (m_colorExt == Color.Empty)
			{
				return WordColor.ConvertIdToColor(m_colorId);
			}
			return m_colorExt;
		}
		set
		{
			m_colorExt = value;
			IsDefault = false;
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

	internal bool IsClear => m_dptLineWidth == byte.MaxValue;

	internal BorderCode()
	{
	}

	internal BorderCode(byte[] arr, int iOffset)
	{
		Parse(arr, iOffset);
	}

	internal void Parse(byte[] arr, int iOffset)
	{
		m_dptLineWidth = arr[iOffset];
		m_brcType = arr[iOffset + 1];
		m_colorId = arr[iOffset + 2];
		m_props = arr[iOffset + 3];
		IsDefault = false;
	}

	internal void ParseNewBrc(byte[] arr, int iOffset)
	{
		if (arr.Length - iOffset >= DEF_NEW_BRC_LENGTH)
		{
			m_colorExt = WordColor.ConvertRGBToColor(BitConverter.ToUInt32(arr, iOffset));
			m_dptLineWidth = arr[iOffset + 4];
			m_brcType = arr[iOffset + 5];
			m_props = arr[iOffset + 6];
			_ = arr[iOffset + 7];
			IsDefault = false;
		}
	}

	internal void SaveBytes(byte[] arr, int iOffset)
	{
		arr[iOffset] = m_dptLineWidth;
		arr[iOffset + 1] = m_brcType;
		arr[iOffset + 2] = m_colorId;
		arr[iOffset + 3] = m_props;
		IsDefault = false;
	}

	internal void SaveNewBrc(byte[] arr, int iOffset)
	{
		BitConverter.GetBytes(WordColor.ConvertColorToRGB(m_colorExt)).CopyTo(arr, iOffset);
		arr[iOffset + 4] = m_dptLineWidth;
		arr[iOffset + 5] = m_brcType;
		arr[iOffset + 6] = m_props;
		arr[iOffset + 7] = 0;
		IsDefault = false;
	}

	internal void Read(BinaryReader reader)
	{
		m_dptLineWidth = reader.ReadByte();
		if (m_dptLineWidth != byte.MaxValue)
		{
			m_brcType = reader.ReadByte();
			m_colorId = reader.ReadByte();
			m_props = reader.ReadByte();
			IsDefault = false;
		}
		else
		{
			reader.ReadByte();
			reader.ReadByte();
			reader.ReadByte();
		}
	}

	internal void Write(Stream stream)
	{
		if (!IsClear)
		{
			stream.WriteByte(m_dptLineWidth);
			stream.WriteByte(m_brcType);
			stream.WriteByte(m_colorId);
			stream.WriteByte(m_props);
		}
		else
		{
			BaseWordRecord.WriteUInt32(stream, uint.MaxValue);
		}
	}

	internal BorderCode Clone()
	{
		return MemberwiseClone() as BorderCode;
	}

	internal bool Compare(BorderCode border)
	{
		if (LineWidth != border.LineWidth)
		{
			return false;
		}
		if (LineColor != border.LineColor)
		{
			return false;
		}
		if (BorderType != border.BorderType)
		{
			return false;
		}
		if (Space != border.Space)
		{
			return false;
		}
		if (Shadow != border.Shadow)
		{
			return false;
		}
		if (LineColorExt != border.LineColorExt)
		{
			return false;
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(LineWidth + ";");
		stringBuilder.Append(BorderType + ";");
		stringBuilder.Append(Space + ";");
		string text = (Shadow ? "1" : "0");
		stringBuilder.Append(text + ";");
		stringBuilder.Append(LineColorExt.ToArgb() + ";");
		return stringBuilder;
	}

	private static List<string> GetIgnorableProperties()
	{
		return new List<string> { "LineColor", "UnderlyingStructure", "IsClear", "Length", "IsDefault" };
	}
}
