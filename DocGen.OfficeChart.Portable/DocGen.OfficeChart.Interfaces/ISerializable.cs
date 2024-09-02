using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Interfaces;

[CLSCompliant(false)]
internal interface ISerializable
{
	void Serialize(IList<IBiffStorage> records);
}
