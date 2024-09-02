namespace DocGen.PdfViewer.Base;

internal class ClosePath : Operator
{
	public override void Execute(CharacterBuilder buildChar)
	{
		buildChar.CurrentPathFigure.IsClosed = true;
	}
}
