using System;
using System.Collections.Generic;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

[MsoDrawing(MsoRecords.msofbtRegroupItems)]
[CLSCompliant(false)]
internal class MsofbtRegroupItems : MsoBase
{
	private byte[] m_arrData;

	public MsofbtRegroupItems(MsoBase parent)
		: base(parent)
	{
	}

	public MsofbtRegroupItems(MsoBase parent, byte[] data, int iOffset)
		: base(parent, data, iOffset)
	{
	}

	public override void InfillInternalData(Stream stream, int iOffset, List<int> arrBreaks, List<List<BiffRecordRaw>> arrRecords)
	{
		m_iLength = ((m_arrData != null) ? m_arrData.Length : 0);
		if (m_iLength > 0 && m_arrData != null)
		{
			stream.Write(m_arrData, 0, m_iLength);
		}
	}

	public override void ParseStructure(Stream stream)
	{
		if (m_iLength > 0)
		{
			m_arrData = new byte[m_iLength];
			stream.Read(m_arrData, 0, m_iLength);
		}
	}
}
