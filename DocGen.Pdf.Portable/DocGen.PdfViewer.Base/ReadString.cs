namespace DocGen.PdfViewer.Base;

internal class ReadString : PostScriptOperators
{
	public override void Execute(FontInterpreter interpreter)
	{
		PostScriptStrHelper lastAs = interpreter.Operands.GetLastAs<PostScriptStrHelper>();
		interpreter.Operands.GetLast();
		int num = 0;
		while (!interpreter.Reader.EndOfFile && num < lastAs.Capacity)
		{
			lastAs[num++] = (char)interpreter.Reader.Read();
		}
		interpreter.Operands.AddLast(lastAs);
		interpreter.Operands.AddLast(true);
	}
}
