namespace DocGen.PdfViewer.Base;

internal class RLineTo : Operator
{
	public override void Execute(CharacterBuilder interpreter)
	{
		while (interpreter.Operands.Count / 2 > 0)
		{
			int firstAsInt = interpreter.Operands.GetFirstAsInt();
			int firstAsInt2 = interpreter.Operands.GetFirstAsInt();
			Operator.LineTo(interpreter, firstAsInt, firstAsInt2);
		}
		interpreter.Operands.Clear();
	}
}
