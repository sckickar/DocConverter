namespace DocGen.OfficeChart;

internal interface IName : IParentApplication
{
	int Index { get; }

	string Name { get; set; }

	string NameLocal { get; set; }

	IRange RefersToRange { get; set; }

	string Value { get; set; }

	bool Visible { get; set; }

	bool IsLocal { get; }

	string ValueR1C1 { get; }

	string RefersTo { get; }

	string RefersToR1C1 { get; }

	IWorksheet Worksheet { get; }

	string Scope { get; }

	void Delete();
}
