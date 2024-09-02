namespace DocGen.OfficeChart;

internal interface ITextRange
{
	string Text { get; set; }

	IRichTextString RichText { get; }
}
