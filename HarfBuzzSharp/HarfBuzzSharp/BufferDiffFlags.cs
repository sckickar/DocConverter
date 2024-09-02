namespace HarfBuzzSharp;

public enum BufferDiffFlags
{
	Equal = 0,
	ContentTypeMismatch = 1,
	LengthMismatch = 2,
	NotdefPresent = 4,
	DottedCirclePresent = 8,
	CodepointMismatch = 0x10,
	ClusterMismatch = 0x20,
	GlyphFlagsMismatch = 0x40,
	PositionMismatch = 0x80
}
