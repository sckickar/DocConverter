using System;

namespace DocGen.Office;

internal sealed class Operators
{
	public const string obj = "obj";

	public const string endobj = "endobj";

	public const string R = "R";

	public const string WhiteSpace = " ";

	public const string Slash = "/";

	public const string LessThan = "<";

	public const string GreaterThan = ">";

	public const string NewLine = "\r\n";

	public const string RegexNewLine = "\\\\r\\\\n";

	public const string stream = "stream";

	public const string endstream = "endstream";

	public const string xref = "xref";

	public const string f = "f";

	public const string n = "n";

	public const string trailer = "trailer";

	public const string startxref = "startxref";

	public const string EOF = "%%EOF";

	public const string header = "%PDF-1.5";

	public const string BeginText = "BT";

	public const string EndText = "ET";

	public const string BeginPath = "m";

	public const string AppendLineSegment = "l";

	public const string Stroke = "S";

	public const string Fill = "f";

	public const string Fill_EvenOdd = "f*";

	public const string FillStroke = "B";

	public const string FillStroke_EvenOdd = "B*";

	public const string AppendBezierCurve = "c";

	public const string AppendRectangle = "re";

	public const string SaveState = "q";

	public const string RestoreState = "Q";

	public const string PaintXObject = "Do";

	public const string ModifyCTM = "cm";

	public const string ModifyTM = "Tm";

	public const string SetLineWidth = "w";

	public const string SetLineCapStyle = "J";

	public const string SetLineJoinStyle = "j";

	public const string SetDashPattern = "d";

	public const string SetFlatnessTolerance = "i";

	public const string ClosePath = "h";

	public const string CloseStrokePath = "s";

	public const string CloseFillStrokePath = "b";

	public const string SetCharacterSpace = "Tc";

	public const string SetWordSpace = "Tw";

	public const string SetHorizontalScaling = "Tz";

	public const string SetTextLeading = "TL";

	public const string SetFont = "Tf";

	public const string SetRenderingMode = "Tr";

	public const string SetTextRise = "Ts";

	public const string SetTextScaling = "Tz";

	public const string SetCoords = "Td";

	public const string SetCoordsAndLeading = "TD";

	public const string GoToNextLine = "T*";

	public const string SetText = "Tj";

	public const string SetTextWithFormatting = "TJ";

	public const string SetTextOnNewLine = "'";

	public const string SetTextOnNewLineWithSpacings = "\"";

	public const string SelectColorSpaceForStroking = "CS";

	public const string SelectColorSpaceForNonStroking = "cs";

	public const string SetRGBColorForStroking = "RG";

	public const string SetRGBColorForNonStroking = "rg";

	public const string SetCMYKColorForStroking = "K";

	public const string SetCMYKColorForNonstroking = "k";

	public const string SetGrayColorForStroking = "G";

	public const string SetGrayColorForNonstroking = "g";

	public const string Pattern = "Pattern";

	public const string SetColorAndPattern = "scn";

	public const string SetColorAndPatternStroking = "SCN";

	public const string ClipPath = "W";

	public const string ClipPath_EvenOdd = "W*";

	public const string EndPath = "n";

	public const string SetGraphicsState = "gs";

	public const string Comment = "%";

	public const string AnyRegexSymbol = ".*";

	public const string BeginMarkedSequence = "BMC";

	public const string EndMarkedSequence = "EMC";

	public const string EvenOdd = "*";

	public const string AppendBezierCurve2 = "v";

	public const string AppendBezierCurve1 = "y";

	public const string SetMiterLimit = "M";

	public const string SetColorRenderingIntent = "ri";

	public const string SetColorStroking = "SC";

	public const string SetColorNonStroking = "sc";

	public const string Para = "P";

	public const string Mcid = "MCID";

	private Operators()
	{
		throw new NotSupportedException();
	}
}
