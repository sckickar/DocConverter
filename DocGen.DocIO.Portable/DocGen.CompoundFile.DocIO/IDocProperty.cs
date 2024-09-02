using System;

namespace DocGen.CompoundFile.DocIO;

internal interface IDocProperty
{
	bool IsBuiltIn { get; }

	BuiltInProperty PropertyId { get; }

	string Name { get; }

	object Value { get; set; }

	bool Boolean { get; set; }

	int Integer { get; set; }

	int Int32 { get; set; }

	double Double { get; set; }

	string Text { get; set; }

	DateTime DateTime { get; set; }

	TimeSpan TimeSpan { get; set; }

	string LinkSource { get; set; }

	bool LinkToContent { get; set; }
}
