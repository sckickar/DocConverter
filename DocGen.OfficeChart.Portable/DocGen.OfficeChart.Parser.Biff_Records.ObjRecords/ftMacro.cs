using System;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Parser.Biff_Records.ObjRecords;

internal class ftMacro : ObjSubRecord
{
	private Ptg[] m_arrTokens;

	public Ptg[] Tokens
	{
		get
		{
			return m_arrTokens;
		}
		set
		{
			m_arrTokens = value;
		}
	}

	public ftMacro()
		: base(TObjSubRecordType.ftMacro)
	{
	}

	[CLSCompliant(false)]
	public ftMacro(ushort length, byte[] buffer)
		: base(TObjSubRecordType.ftMacro, length, buffer)
	{
	}

	protected override void Parse(byte[] buffer)
	{
		int num = 0;
		int iExpressionLength = BitConverter.ToUInt16(buffer, num);
		num += 2;
		num += 4;
		ByteArrayDataProvider provider = new ByteArrayDataProvider(buffer);
		m_arrTokens = FormulaUtil.ParseExpression(provider, num, iExpressionLength, out var _, OfficeVersion.Excel97to2003);
	}

	protected override void Serialize(DataProvider provider, int iOffset)
	{
		byte[] array = FormulaUtil.PtgArrayToByteArray(m_arrTokens, OfficeVersion.Excel97to2003);
		int num = array.Length;
		provider.WriteUInt16(iOffset, (ushort)num);
		iOffset += 2;
		provider.WriteInt32(iOffset, 0);
		iOffset += 4;
		provider.WriteBytes(iOffset, array);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		int num = DVRecord.GetFormulaSize(m_arrTokens, version, addAdditionalDataSize: true) + 4 + 2 + 4;
		if (num % 2 != 0)
		{
			num++;
		}
		return num;
	}

	public override object Clone()
	{
		ftMacro obj = (ftMacro)base.Clone();
		obj.m_arrTokens = CloneUtils.ClonePtgArray(m_arrTokens);
		return obj;
	}
}
