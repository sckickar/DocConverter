namespace DocGen.Pdf.Parsing;

internal class SystemFontCallOtherSubr : SystemFontOperator
{
	public override void Execute(SystemFontBuildChar interpreter)
	{
		int lastAsInt = interpreter.Operands.GetLastAsInt();
		int lastAsInt2 = interpreter.Operands.GetLastAsInt();
		switch (lastAsInt)
		{
		default:
		{
			for (int i = 0; i < lastAsInt2; i++)
			{
				interpreter.PostScriptStack.AddLast(interpreter.Operands.GetLast());
			}
			break;
		}
		case 3:
			interpreter.PostScriptStack.AddLast(3);
			break;
		case 0:
			interpreter.PostScriptStack.AddLast(interpreter.Operands.GetLast());
			interpreter.PostScriptStack.AddLast(interpreter.Operands.GetLast());
			break;
		}
		interpreter.Operands.Clear();
	}
}
