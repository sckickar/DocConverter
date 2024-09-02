namespace DocGen.DocIO.DLS;

internal interface ICellContentControl
{
	ContentControlProperties ContentControlProperties { get; }

	WCharacterFormat BreakCharacterFormat { get; }
}
