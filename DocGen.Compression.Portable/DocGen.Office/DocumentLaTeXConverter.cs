namespace DocGen.Office;

internal abstract class DocumentLaTeXConverter
{
	internal abstract void CreateMathRunElement(IOfficeMathRunElement officeMathRunElement, string text);

	internal abstract void AppendTextInMathRun(IOfficeMathRunElement officeMathRunElement, string text);

	internal abstract string GetText(IOfficeMathRunElement officeMathRunElement);
}
