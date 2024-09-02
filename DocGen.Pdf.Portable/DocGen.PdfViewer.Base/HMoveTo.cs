namespace DocGen.PdfViewer.Base;

internal class HMoveTo : Operator
{
	public override void Execute(CharacterBuilder interpreter)
	{
		Operator.ReadWidth(interpreter, 1);
		int firstAsInt = interpreter.Operands.GetFirstAsInt();
		Operator.MoveTo(interpreter, firstAsInt, 0);
		interpreter.Operands.Clear();
	}
}
