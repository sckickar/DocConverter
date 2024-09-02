using System;
using System.Collections.Generic;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

[MsoDrawing(MsoRecords.msoUnknown)]
[CLSCompliant(false)]
internal class MsoUnknown : MsoBase
{
	public override bool NeedDataArray => true;

	public MsoUnknown(MsoBase parent)
		: base(parent)
	{
	}

	public MsoUnknown(MsoBase parent, byte[] data, int iOffset)
		: base(parent, data, iOffset)
	{
	}

	public override void ParseStructure(Stream stream)
	{
		if (m_iLength > 0)
		{
			m_data = new byte[m_iLength];
			stream.Read(m_data, 0, m_iLength);
		}
	}

	public override void InfillInternalData(Stream stream, int iOffset, List<int> arrBreaks, List<List<BiffRecordRaw>> arrRecords)
	{
		if (m_iLength > 0)
		{
			stream.Write(m_data, 0, m_iLength);
		}
	}
}
