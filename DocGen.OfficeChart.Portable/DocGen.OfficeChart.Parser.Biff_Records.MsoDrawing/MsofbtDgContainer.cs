using System;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

[MsoDrawing(MsoRecords.msofbtDgContainer)]
[CLSCompliant(false)]
internal class MsofbtDgContainer : MsoContainerBase
{
	private const int DEF_VERSION = 15;

	public MsofbtDgContainer(MsoBase parent)
		: base(parent)
	{
		base.Version = 15;
	}

	public MsofbtDgContainer(MsoBase parent, byte[] data, int iOffset)
		: base(parent, data, iOffset)
	{
	}

	public MsofbtDgContainer(MsoBase parent, byte[] data, int iOffset, GetNextMsoDrawingData dataGetter)
		: base(parent, data, iOffset, dataGetter)
	{
	}
}
