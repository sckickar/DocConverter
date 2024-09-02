using System;
using System.Globalization;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
[Token(FormulaToken.tMemArea1)]
[Token(FormulaToken.tMemArea2)]
[Token(FormulaToken.tMemArea3)]
internal class MemAreaPtg : Ptg, IAdditionalData
{
	private const int DEF_RECT_SIZE = 8;

	private const int DEF_HEADER_SIZE = 7;

	private int m_iReserved;

	private ushort m_usSubExpressionLength;

	private Ptg[] m_arrSubexpression;

	private Rectangle[] m_arrRects;

	[CLSCompliant(false)]
	public ushort SubExpressionLength => m_usSubExpressionLength;

	public Ptg[] Subexpression => m_arrSubexpression;

	public Rectangle[] Rectangles => m_arrRects;

	public int AdditionalDataSize
	{
		get
		{
			int num = ((m_arrRects != null) ? m_arrRects.Length : 0);
			return 2 + num * 8;
		}
	}

	[Preserve]
	public MemAreaPtg()
	{
	}

	[Preserve]
	public MemAreaPtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
	}

	[Preserve]
	public MemAreaPtg(string strFormula)
	{
	}

	public override int GetSize(OfficeVersion version)
	{
		return m_usSubExpressionLength + 7;
	}

	public override byte[] ToByteArray(OfficeVersion version)
	{
		return base.ToByteArray(version);
	}

	public override string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1, NumberFormatInfo numberFormat, bool isForSerialization)
	{
		return "MemArea";
	}

	public int ReadArray(DataProvider provider, int offset)
	{
		ushort num = provider.ReadUInt16(offset);
		offset += 2;
		m_arrRects = new Rectangle[num];
		for (int i = 0; i < num; i++)
		{
			ushort top = provider.ReadUInt16(offset);
			offset += 2;
			ushort bottom = provider.ReadUInt16(offset);
			offset += 2;
			ushort left = provider.ReadUInt16(offset);
			offset += 2;
			ushort right = provider.ReadUInt16(offset);
			offset += 2;
			m_arrRects[i] = Rectangle.FromLTRB(left, top, right, bottom);
		}
		return offset;
	}

	public override void InfillPTG(DataProvider provider, ref int offset, OfficeVersion version)
	{
		m_iReserved = provider.ReadInt32(offset);
		offset += 4;
		m_usSubExpressionLength = provider.ReadUInt16(offset);
		offset += 2;
		m_arrSubexpression = FormulaUtil.ParseExpression(provider, offset, m_usSubExpressionLength, out var _, version);
		offset += m_usSubExpressionLength;
	}
}
