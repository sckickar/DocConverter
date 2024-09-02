using System;
using DocGen.DocIO.DLS;
using DocGen.Drawing;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

internal class ShadingDescriptor
{
	internal const int DEF_SHD_LENGTH = 2;

	internal const int DEF_SHD_NEW_LENGTH = 10;

	private uint m_foreColorExt = 4278190080u;

	private uint m_backColorExt = 4278190080u;

	private TextureStyle m_pattern;

	internal Color ForeColor
	{
		get
		{
			if (m_foreColorExt != 4278190080u)
			{
				return WordColor.ConvertRGBToColor(m_foreColorExt);
			}
			return Color.Empty;
		}
		set
		{
			m_foreColorExt = WordColor.ConvertColorToRGB(value);
		}
	}

	internal Color BackColor
	{
		get
		{
			if (m_backColorExt != 4278190080u)
			{
				return WordColor.ConvertRGBToColor(m_backColorExt);
			}
			return Color.Empty;
		}
		set
		{
			m_backColorExt = WordColor.ConvertColorToRGB(value);
		}
	}

	internal TextureStyle Pattern
	{
		get
		{
			return m_pattern;
		}
		set
		{
			m_pattern = value;
		}
	}

	internal static int StructLength => 2;

	internal ShadingDescriptor(short shd)
	{
		Read(shd);
	}

	internal ShadingDescriptor()
	{
	}

	internal ShadingDescriptor Clone()
	{
		return new ShadingDescriptor
		{
			m_foreColorExt = m_foreColorExt,
			m_backColorExt = m_backColorExt,
			m_pattern = m_pattern
		};
	}

	internal void Read(short shd)
	{
		m_foreColorExt = WordColor.ConvertIdToRGB(shd & 0x1F);
		m_backColorExt = WordColor.ConvertIdToRGB((shd & 0x3E0) >> 5);
		m_pattern = (TextureStyle)((shd & 0xFC00) >> 10);
	}

	internal short Save()
	{
		int num = 0;
		num = (int)WordColor.ArgbArray[0] | WordColor.ConvertRGBToId(m_foreColorExt);
		num = (int)WordColor.ArgbArray[0] | (WordColor.ConvertRGBToId(m_backColorExt) << 5);
		num = ((m_pattern != TextureStyle.TextureNil) ? (num | ((int)m_pattern << 10)) : (num | 0));
		return (short)num;
	}

	internal void ReadNewShd(byte[] shd, int offset)
	{
		if (offset < shd.Length - 3 || offset > shd.Length - 1)
		{
			m_foreColorExt = BitConverter.ToUInt32(shd, offset);
		}
		if (offset + 4 <= shd.Length - 1 && (offset + 4 < shd.Length - 3 || offset + 4 > shd.Length - 1))
		{
			m_backColorExt = BitConverter.ToUInt32(shd, offset + 4);
		}
		if (offset + 8 <= shd.Length - 1)
		{
			m_pattern = (TextureStyle)BitConverter.ToUInt16(shd, offset + 8);
		}
	}

	internal byte[] SaveNewShd()
	{
		byte[] array = new byte[10];
		BitConverter.GetBytes(m_foreColorExt).CopyTo(array, 0);
		BitConverter.GetBytes(m_backColorExt).CopyTo(array, 4);
		BitConverter.GetBytes((ushort)m_pattern).CopyTo(array, 8);
		return array;
	}
}
