using System;

namespace DocGen.PdfViewer.Base;

internal class Copy : PostScriptOperators
{
	public override void Execute(FontInterpreter interpreter)
	{
		object last = interpreter.Operands.GetLast();
		if (last is int)
		{
			ExecuteFirstForm(interpreter, (int)last);
			return;
		}
		throw new NotSupportedException("This operator is not supported.");
	}

	private static void ExecuteFirstForm(FontInterpreter interpreter, int n)
	{
		object[] array = new object[n];
		for (int num = n - 1; num >= n; num--)
		{
			array[num] = interpreter.Operands.GetLast();
		}
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < array.Length; j++)
			{
				interpreter.Operands.AddLast(array[j]);
			}
		}
	}
}
