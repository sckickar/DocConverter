namespace DocGen.PdfViewer.Base;

internal class Get : PostScriptOperators
{
	public override void Execute(FontInterpreter interpreter)
	{
		object last = interpreter.Operands.GetLast();
		object last2 = interpreter.Operands.GetLast();
		if (last2 is PostScriptArray)
		{
			PostScriptArray postScriptArray = (PostScriptArray)last2;
			Helper.ParseInteger(last, out var res);
			interpreter.Operands.AddLast(postScriptArray[res]);
		}
		else if (last2 is PostScriptDict)
		{
			PostScriptDict postScriptDict = (PostScriptDict)last2;
			interpreter.Operands.AddLast(postScriptDict[(string)last]);
		}
		else if (last2 is PostScriptStrHelper)
		{
			PostScriptStrHelper postScriptStrHelper = (PostScriptStrHelper)last2;
			Helper.ParseInteger(last, out var res2);
			interpreter.Operands.AddLast(postScriptStrHelper[res2]);
		}
	}
}
