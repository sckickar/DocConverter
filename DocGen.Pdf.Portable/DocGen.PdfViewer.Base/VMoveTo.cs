namespace DocGen.PdfViewer.Base;

internal class VMoveTo : Operator
{
	public override void Execute(CharacterBuilder interpreter)
	{
		Operator.ReadWidth(interpreter, 1);
		int firstAsInt = interpreter.Operands.GetFirstAsInt();
		Operator.MoveTo(interpreter, 0, firstAsInt);
		interpreter.Operands.Clear();
	}
}
