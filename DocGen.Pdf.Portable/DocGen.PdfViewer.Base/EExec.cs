namespace DocGen.PdfViewer.Base;

internal class EExec : PostScriptOperators
{
	public override void Execute(FontInterpreter interpreter)
	{
		interpreter.Operands.GetLast();
		interpreter.Reader.SkipWhiteSpaces();
		EExecEncryption eExecEncryption = new EExecEncryption(interpreter.Reader);
		interpreter.Reader.PushEncryption(eExecEncryption);
		eExecEncryption.Initialize();
	}
}
