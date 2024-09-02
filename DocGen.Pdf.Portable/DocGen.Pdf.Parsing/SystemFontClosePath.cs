namespace DocGen.Pdf.Parsing;

internal class SystemFontClosePath : SystemFontOperator
{
	public override void Execute(SystemFontBuildChar buildChar)
	{
		buildChar.CurrentPathFigure.IsClosed = true;
	}
}
