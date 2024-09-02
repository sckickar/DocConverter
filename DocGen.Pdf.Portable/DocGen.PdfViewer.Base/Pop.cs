namespace DocGen.PdfViewer.Base;

internal class Pop : Operator
{
	public override void Execute(CharacterBuilder buildChar)
	{
		buildChar.Operands.AddLast(buildChar.PostScriptStack.GetLast());
	}
}
