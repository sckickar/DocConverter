using DocGen.PdfViewer.Base;

namespace DocGen.Pdf.Parsing;

internal class SystemFontSbw : SystemFontOperator
{
	public override void Execute(SystemFontBuildChar buildChar)
	{
		buildChar.Operands.GetLastAsInt();
		int lastAsInt = buildChar.Operands.GetLastAsInt();
		int lastAsInt2 = buildChar.Operands.GetLastAsInt();
		int lastAsInt3 = buildChar.Operands.GetLastAsInt();
		buildChar.Operands.Clear();
		buildChar.Width = lastAsInt;
		buildChar.CurrentPoint = new Point(lastAsInt3, lastAsInt2);
	}
}
