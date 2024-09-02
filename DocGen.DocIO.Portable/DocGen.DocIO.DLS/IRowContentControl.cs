namespace DocGen.DocIO.DLS;

internal interface IRowContentControl
{
	ContentControlProperties ContentControlProperties { get; }

	WCharacterFormat BreakCharacterFormat { get; }
}
