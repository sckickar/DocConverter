namespace DocGen.Styles;

internal interface IStyleInfo
{
	bool IsEmpty { get; }

	bool IsChanged { get; }

	StyleInfoStore Store { get; }

	bool IsSubset(IStyleInfo style);

	void ModifyStyle(IStyleInfo style, StyleModifyType mt);

	void MergeStyle(IStyleInfo style);

	void ParseString(string s);

	bool HasValue(StyleInfoProperty sip);

	object GetValue(StyleInfoProperty sip);
}
