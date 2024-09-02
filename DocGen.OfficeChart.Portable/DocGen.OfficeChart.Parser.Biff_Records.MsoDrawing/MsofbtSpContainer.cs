using System;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

[MsoDrawing(MsoRecords.msofbtSpContainer)]
[CLSCompliant(false)]
internal class MsofbtSpContainer : MsoContainerBase
{
	private const int DEF_VERSION = 15;

	public MsofbtSpContainer(MsoBase parent)
		: base(parent)
	{
		base.Version = 15;
	}

	public MsofbtSpContainer(MsoBase parent, byte[] data, int iOffset)
		: base(parent, data, iOffset)
	{
	}

	public MsofbtSpContainer(MsoBase parent, byte[] data, int iOffset, GetNextMsoDrawingData dataGetter)
		: base(parent, data, iOffset, dataGetter)
	{
	}
}
