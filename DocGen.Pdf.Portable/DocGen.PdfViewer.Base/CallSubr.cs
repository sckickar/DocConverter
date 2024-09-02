namespace DocGen.PdfViewer.Base;

internal class CallSubr : Operator
{
	public override void Execute(CharacterBuilder interpreter)
	{
		interpreter.ExecuteSubr(interpreter.Operands.GetLastAsInt());
	}
}
