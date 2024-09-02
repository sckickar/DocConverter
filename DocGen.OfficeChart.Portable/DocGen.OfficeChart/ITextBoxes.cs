namespace DocGen.OfficeChart;

internal interface ITextBoxes
{
	int Count { get; }

	ITextBoxShape this[int index] { get; }

	ITextBoxShape this[string name] { get; }

	ITextBoxShape AddTextBox(int row, int column, int height, int width);
}
