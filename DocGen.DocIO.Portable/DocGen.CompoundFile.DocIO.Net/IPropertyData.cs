using DocGen.CompoundFile.Net;

namespace DocGen.CompoundFile.DocIO.Net;

internal interface IPropertyData
{
	object Value { get; }

	VarEnum Type { get; }

	string Name { get; }

	int Id { get; set; }

	bool SetValue(object value, PropertyType type);
}
