namespace DocGen.PdfViewer.Base;

internal class HLineTo : Operator
{
	public override void Execute(CharacterBuilder interpreter)
	{
		if (interpreter.Operands.Count % 2 == 0)
		{
			while (interpreter.Operands.Count > 0)
			{
				int firstAsInt = interpreter.Operands.GetFirstAsInt();
				int firstAsInt2 = interpreter.Operands.GetFirstAsInt();
				Operator.HLineTo(interpreter, firstAsInt);
				Operator.VLineTo(interpreter, firstAsInt2);
			}
		}
		else
		{
			int firstAsInt3 = interpreter.Operands.GetFirstAsInt();
			Operator.HLineTo(interpreter, firstAsInt3);
			while (interpreter.Operands.Count > 0)
			{
				int firstAsInt4 = interpreter.Operands.GetFirstAsInt();
				int firstAsInt5 = interpreter.Operands.GetFirstAsInt();
				Operator.VLineTo(interpreter, firstAsInt4);
				Operator.HLineTo(interpreter, firstAsInt5);
			}
		}
		interpreter.Operands.Clear();
	}
}
