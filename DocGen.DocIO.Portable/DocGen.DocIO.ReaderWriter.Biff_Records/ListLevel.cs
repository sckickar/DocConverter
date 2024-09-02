using System.IO;
using DocGen.DocIO.DLS;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

internal class ListLevel : BaseWordRecord
{
	internal int m_startAt;

	internal ListPatternType m_nfc;

	internal ListNumberAlignment m_jc;

	internal bool m_bLegal;

	internal bool m_bNoRestart;

	internal bool m_bPrev;

	internal bool m_bPrevSpace;

	internal bool m_bWord6;

	internal bool m_unused;

	internal byte[] m_rgbxchNums;

	internal FollowCharacterType m_ixchFollow;

	internal int m_dxaSpace;

	internal int m_dxaIndent;

	internal int m_reserved;

	internal CharacterPropertyException m_chpx;

	internal ParagraphPropertyException m_papx;

	internal string m_str;

	internal ListLevel()
	{
		m_rgbxchNums = new byte[9];
	}

	internal ListLevel(Stream stream)
	{
		Read(stream);
	}

	internal void Write(Stream stream)
	{
		_ = stream.Position;
		BaseWordRecord.WriteInt32(stream, m_startAt);
		stream.WriteByte((byte)m_nfc);
		int num = 0;
		num |= (int)m_jc;
		num |= (m_bLegal ? 4 : 0);
		num |= (m_bNoRestart ? 8 : 0);
		num |= (m_bPrev ? 16 : 0);
		num |= (m_bPrevSpace ? 32 : 0);
		num |= (m_bWord6 ? 64 : 0);
		num |= (m_unused ? 128 : 0);
		stream.WriteByte((byte)num);
		stream.Write(m_rgbxchNums, 0, m_rgbxchNums.Length);
		stream.WriteByte((byte)m_ixchFollow);
		BaseWordRecord.WriteInt32(stream, m_dxaSpace);
		BaseWordRecord.WriteInt32(stream, m_dxaIndent);
		stream.WriteByte((byte)m_chpx.PropertyModifiers.Length);
		stream.WriteByte((byte)m_papx.PropertyModifiers.Length);
		BaseWordRecord.WriteUInt16(stream, (ushort)m_reserved);
		m_papx.PropertyModifiers.Save(stream);
		m_chpx.PropertyModifiers.Save(stream);
		BaseWordRecord.WriteString(stream, m_str);
	}

	private void Read(Stream stream)
	{
		_ = stream.Position;
		m_rgbxchNums = new byte[9];
		m_startAt = (int)BaseWordRecord.ReadUInt32(stream);
		m_nfc = (ListPatternType)stream.ReadByte();
		int num = stream.ReadByte();
		m_jc = (ListNumberAlignment)(byte)(num & 3);
		m_bLegal = (num & 4) != 0;
		m_bNoRestart = (num & 8) != 0;
		m_bPrev = (num & 0x10) != 0;
		m_bPrevSpace = (num & 0x20) != 0;
		m_bWord6 = (num & 0x40) != 0;
		m_unused = (num & 0x80) != 0;
		m_rgbxchNums = ReadBytes(stream, 9);
		m_ixchFollow = (FollowCharacterType)stream.ReadByte();
		m_dxaSpace = (int)BaseWordRecord.ReadUInt32(stream);
		m_dxaIndent = (int)BaseWordRecord.ReadUInt32(stream);
		int dataLen = stream.ReadByte();
		int dataLen2 = stream.ReadByte();
		m_reserved = BaseWordRecord.ReadUInt16(stream);
		m_chpx = new CharacterPropertyException();
		m_papx = new ParagraphPropertyException();
		ReadListSprms(dataLen2, stream, isChpx: false);
		ReadListSprms(dataLen, stream, isChpx: true);
		m_str = BaseWordRecord.ReadString(stream);
	}

	private void ReadListSprms(int dataLen, Stream stream, bool isChpx)
	{
		int num = 0;
		if (dataLen == 0)
		{
			return;
		}
		SinglePropertyModifierArray singlePropertyModifierArray = (isChpx ? m_chpx.PropertyModifiers : m_papx.PropertyModifiers);
		byte[] array = new byte[dataLen];
		stream.Read(array, 0, dataLen);
		while (dataLen - num > 1)
		{
			SinglePropertyModifierRecord singlePropertyModifierRecord = new SinglePropertyModifierRecord();
			try
			{
				num = singlePropertyModifierRecord.Parse(array, num);
			}
			catch
			{
				num = dataLen;
			}
			if (singlePropertyModifierArray.IsValidCharacterPropertySprm(singlePropertyModifierRecord))
			{
				singlePropertyModifierArray.CheckDuplicateSprms(singlePropertyModifierRecord);
			}
			singlePropertyModifierArray.Add(singlePropertyModifierRecord);
		}
	}
}
