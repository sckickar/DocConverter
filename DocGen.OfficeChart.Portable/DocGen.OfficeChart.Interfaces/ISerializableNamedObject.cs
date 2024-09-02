using System;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Interfaces;

[CLSCompliant(false)]
internal interface ISerializableNamedObject : INamedObject
{
	new string Name { get; set; }

	int RealIndex { get; set; }

	event ValueChangedEventHandler NameChanged;

	void Serialize(OffsetArrayList records);
}
