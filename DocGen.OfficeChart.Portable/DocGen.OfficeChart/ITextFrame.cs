namespace DocGen.OfficeChart;

internal interface ITextFrame
{
	bool IsTextOverFlow { get; set; }

	bool WrapTextInShape { get; set; }

	bool IsAutoSize { get; set; }

	int MarginLeftPt { get; set; }

	int TopMarginPt { get; set; }

	int RightMarginPt { get; set; }

	int BottomMarginPt { get; set; }

	bool IsAutoMargins { get; set; }

	TextVertOverflowType TextVertOverflowType { get; set; }

	TextHorzOverflowType TextHorzOverflowType { get; set; }

	OfficeHorizontalAlignment HorizontalAlignment { get; set; }

	OfficeVerticalAlignment VerticalAlignment { get; set; }

	TextDirection TextDirection { get; set; }

	ITextRange TextRange { get; }
}
