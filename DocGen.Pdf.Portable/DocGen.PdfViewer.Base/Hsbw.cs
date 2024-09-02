namespace DocGen.PdfViewer.Base;

internal class Hsbw : Operator
{
	public override void Execute(CharacterBuilder buildChar)
	{
		int lastAsInt = buildChar.Operands.GetLastAsInt();
		int lastAsInt2 = buildChar.Operands.GetLastAsInt();
		buildChar.Operands.Clear();
		buildChar.Width = lastAsInt;
		buildChar.CurrentPoint = new Point(lastAsInt2, 0.0);
	}
}
