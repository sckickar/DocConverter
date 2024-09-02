namespace DocGen.OfficeChart;

internal interface ITextBoxShapeEx : ITextBoxShape, ITextBox, IParentApplication, IShape
{
	string TextLink { get; set; }
}
