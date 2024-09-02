namespace DocGen.PdfViewer.Base;

internal class Div : Operator
{
	public override void Execute(CharacterBuilder buildChar)
	{
		double lastAsReal = buildChar.Operands.GetLastAsReal();
		double num = buildChar.Operands.GetFirstAsInt();
		buildChar.Operands.AddLast(num / lastAsReal);
	}
}
