namespace DocGen.OfficeChart;

internal interface IRichTextString : IParentApplication, IOptimizedUpdate
{
	string Text { get; set; }

	string RtfText { get; set; }

	bool IsFormatted { get; }

	IOfficeFont GetFont(int iPosition);

	void SetFont(int iStartPos, int iEndPos, IOfficeFont font);

	void ClearFormatting();

	void Clear();

	void Append(string text, IOfficeFont font);
}
