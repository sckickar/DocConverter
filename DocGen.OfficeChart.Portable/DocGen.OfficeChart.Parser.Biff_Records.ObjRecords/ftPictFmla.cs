using System;

namespace DocGen.OfficeChart.Parser.Biff_Records.ObjRecords;

[CLSCompliant(false)]
internal class ftPictFmla : ObjSubRecord
{
	private const int FormulaStart = 14;

	private static readonly byte[] DefaultHeader = new byte[14]
	{
		30, 0, 5, 0, 12, 151, 65, 7, 2, 8,
		8, 232, 7, 3
	};

	private static readonly byte[] DefaultFooter = new byte[16]
	{
		0, 0, 0, 0, 68, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0
	};

	private byte[] m_arrHeader;

	private byte[] m_arrFooter;

	private string m_strFormula;

	public string Formula
	{
		get
		{
			return m_strFormula;
		}
		set
		{
			m_strFormula = value;
		}
	}

	public ftPictFmla()
		: base(TObjSubRecordType.ftPictFmla)
	{
		int num = DefaultHeader.Length;
		m_arrHeader = new byte[num];
		Buffer.BlockCopy(DefaultHeader, 0, m_arrHeader, 0, num);
		num = DefaultFooter.Length;
		m_arrFooter = new byte[num];
		Buffer.BlockCopy(DefaultFooter, 0, m_arrFooter, 0, num);
	}

	public ftPictFmla(TObjSubRecordType type, ushort length, byte[] buffer)
		: base(type, length, buffer)
	{
	}

	protected override void Parse(byte[] buffer)
	{
		m_arrHeader = new byte[14];
		Buffer.BlockCopy(buffer, 0, m_arrHeader, 0, 14);
		int offset = 14;
		m_strFormula = BiffRecordRaw.GetString16BitUpdateOffset(buffer, ref offset);
		int num = buffer.Length - offset;
		m_arrFooter = new byte[num];
		Buffer.BlockCopy(buffer, offset, m_arrFooter, 0, num);
	}

	public override void FillArray(DataProvider provider, int iOffset)
	{
		provider.WriteInt16(iOffset, (short)base.Type);
		iOffset += 2;
		int num = GetStoreSize(OfficeVersion.Excel97to2003) - 4;
		provider.WriteInt16(iOffset, (short)num);
		iOffset += 2;
		num = m_arrHeader.Length;
		provider.WriteBytes(iOffset, m_arrHeader, 0, num);
		iOffset += num;
		iOffset += provider.WriteString16Bit(iOffset, m_strFormula, isUnicode: false);
		num = m_arrFooter.Length;
		provider.WriteBytes(iOffset, m_arrFooter, 0, num);
		iOffset += num;
	}

	public override object Clone()
	{
		ftPictFmla obj = (ftPictFmla)base.Clone();
		obj.m_arrHeader = CloneUtils.CloneByteArray(m_arrHeader);
		obj.m_arrFooter = CloneUtils.CloneByteArray(m_arrFooter);
		return obj;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 4 + m_arrFooter.Length + m_arrHeader.Length + 3 + m_strFormula.Length;
	}
}
