using System;

namespace Esprima.Ast;

[Flags]
public enum PropertyKind
{
	None = 0,
	Data = 1,
	Get = 2,
	Set = 4,
	Init = 8,
	Constructor = 0x10,
	Method = 0x20
}
