using Esprima.Utils;

namespace Esprima.Ast;

public enum AssignmentOperator
{
	[EnumMember(Value = "=")]
	Assign,
	[EnumMember(Value = "+=")]
	PlusAssign,
	[EnumMember(Value = "-=")]
	MinusAssign,
	[EnumMember(Value = "*=")]
	TimesAssign,
	[EnumMember(Value = "/=")]
	DivideAssign,
	[EnumMember(Value = "%=")]
	ModuloAssign,
	[EnumMember(Value = "&=")]
	BitwiseAndAssign,
	[EnumMember(Value = "|=")]
	BitwiseOrAssign,
	[EnumMember(Value = "^=")]
	BitwiseXOrAssign,
	[EnumMember(Value = "<<=")]
	LeftShiftAssign,
	[EnumMember(Value = ">>=")]
	RightShiftAssign,
	[EnumMember(Value = ">>>=")]
	UnsignedRightShiftAssign,
	[EnumMember(Value = "**=")]
	ExponentiationAssign
}
