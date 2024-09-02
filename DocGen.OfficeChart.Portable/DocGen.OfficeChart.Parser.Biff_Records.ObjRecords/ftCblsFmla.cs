using System;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Parser.Biff_Records.ObjRecords;

internal class ftCblsFmla : ObjSubRecord, IFormulaRecord
{
	private Ptg[] m_formula;

	public Ptg[] Formula
	{
		get
		{
			return m_formula;
		}
		set
		{
			m_formula = value;
		}
	}

	public ftCblsFmla()
		: base(TObjSubRecordType.ftCblsFmla)
	{
	}

	public ftCblsFmla(TObjSubRecordType type, ushort length, byte[] buffer)
		: base(type, length, buffer)
	{
	}

	protected override void Parse(byte[] buffer)
	{
		int num = 0;
		int iExpressionLength = BitConverter.ToInt16(buffer, num);
		num += 2;
		BitConverter.ToInt32(buffer, num);
		num += 4;
		ByteArrayDataProvider provider = new ByteArrayDataProvider(buffer);
		m_formula = FormulaUtil.ParseExpression(provider, num, iExpressionLength, out var _, OfficeVersion.Excel97to2003);
	}

	protected override void Serialize(DataProvider provider, int iOffset)
	{
		byte[] array = FormulaUtil.PtgArrayToByteArray(m_formula, OfficeVersion.Excel97to2003);
		int num = array.Length;
		provider.WriteInt16(iOffset, (short)num);
		iOffset += 2;
		provider.WriteInt32(iOffset, 0);
		iOffset += 4;
		provider.WriteBytes(iOffset, array);
		iOffset += num;
		provider.WriteByte(iOffset, 0);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return DVRecord.GetFormulaSize(m_formula, version, addAdditionalDataSize: true) + 4 + 2 + 4 + 1;
	}
}
