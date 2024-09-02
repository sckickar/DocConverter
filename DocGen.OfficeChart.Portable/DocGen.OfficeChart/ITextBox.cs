namespace DocGen.OfficeChart;

internal interface ITextBox : IParentApplication
{
	OfficeCommentHAlign HAlignment { get; set; }

	OfficeCommentVAlign VAlignment { get; set; }

	OfficeTextRotation TextRotation { get; set; }

	bool IsTextLocked { get; set; }

	IRichTextString RichText { get; set; }

	string Text { get; set; }
}
