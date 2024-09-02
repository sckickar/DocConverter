namespace DocGen.PdfViewer.Base;

internal class CallGSubr : Operator
{
	public override void Execute(CharacterBuilder interpreter)
	{
		interpreter.ExecuteGlobalSubr(interpreter.Operands.GetLastAsInt());
	}
}
