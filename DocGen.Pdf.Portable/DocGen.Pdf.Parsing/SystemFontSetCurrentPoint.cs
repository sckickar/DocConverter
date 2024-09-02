using DocGen.PdfViewer.Base;

namespace DocGen.Pdf.Parsing;

internal class SystemFontSetCurrentPoint : SystemFontOperator
{
	public override void Execute(SystemFontBuildChar buildChar)
	{
		int lastAsInt = buildChar.Operands.GetLastAsInt();
		int lastAsInt2 = buildChar.Operands.GetLastAsInt();
		buildChar.Operands.Clear();
		buildChar.CurrentPoint = new Point(lastAsInt2, lastAsInt);
	}
}
