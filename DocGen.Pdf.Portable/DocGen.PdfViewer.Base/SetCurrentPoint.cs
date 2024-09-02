namespace DocGen.PdfViewer.Base;

internal class SetCurrentPoint : Operator
{
	public override void Execute(CharacterBuilder buildChar)
	{
		int lastAsInt = buildChar.Operands.GetLastAsInt();
		int lastAsInt2 = buildChar.Operands.GetLastAsInt();
		buildChar.Operands.Clear();
		buildChar.CurrentPoint = new Point(lastAsInt2, lastAsInt);
	}
}
