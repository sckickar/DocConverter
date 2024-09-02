using System;

namespace DocGen.Office;

[Flags]
internal enum TtfCompositeGlyphFlags : ushort
{
	ARG_1_AND_2_ARE_WORDS = 1,
	ARGS_ARE_XY_VALUES = 2,
	ROUND_XY_TO_GRID = 4,
	WE_HAVE_A_SCALE = 8,
	RESERVED = 0x10,
	MORE_COMPONENTS = 0x20,
	WE_HAVE_AN_X_AND_Y_SCALE = 0x40,
	WE_HAVE_A_TWO_BY_TWO = 0x80,
	WE_HAVE_INSTRUCTIONS = 0x100,
	USE_MY_METRICS = 0x200
}
