namespace DocGen.PdfViewer.Base;

internal class RMoveTo : Operator
{
	public override void Execute(CharacterBuilder interpreter)
	{
		Operator.ReadWidth(interpreter, 2);
		int firstAsInt = interpreter.Operands.GetFirstAsInt();
		int firstAsInt2 = interpreter.Operands.GetFirstAsInt();
		Operator.MoveTo(interpreter, firstAsInt, firstAsInt2);
		interpreter.Operands.Clear();
	}
}
