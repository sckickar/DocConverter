namespace DocGen.PdfViewer.Base;

internal class Put : PostScriptOperators
{
	public override void Execute(FontInterpreter interpreter)
	{
		object last = interpreter.Operands.GetLast();
		object last2 = interpreter.Operands.GetLast();
		object last3 = interpreter.Operands.GetLast();
		if (last3 is PostScriptArray)
		{
			PostScriptArray obj = (PostScriptArray)last3;
			Helper.ParseInteger(last2, out var res);
			obj[res] = last;
		}
		else if (last3 is PostScriptDict)
		{
			((PostScriptDict)last3)[(string)last2] = last;
		}
		else if (last3 is PostScriptStrHelper)
		{
			PostScriptStrHelper obj2 = (PostScriptStrHelper)last3;
			Helper.ParseInteger(last2, out var res2);
			Helper.ParseInteger(last, out var res3);
			obj2[res2] = (char)res3;
		}
	}
}
