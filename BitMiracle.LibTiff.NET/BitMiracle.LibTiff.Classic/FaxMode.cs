using System;

namespace BitMiracle.LibTiff.Classic;

[Flags]
public enum FaxMode
{
	CLASSIC = 0,
	NORTC = 1,
	NOEOL = 2,
	BYTEALIGN = 4,
	WORDALIGN = 8,
	CLASSF = 1
}
