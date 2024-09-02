using Esprima.Utils;

namespace Esprima.Ast;

public enum BinaryOperator
{
	[EnumMember(Value = "+")]
	Plus,
	[EnumMember(Value = "-")]
	Minus,
	[EnumMember(Value = "*")]
	Times,
	[EnumMember(Value = "/")]
	Divide,
	[EnumMember(Value = "%")]
	Modulo,
	[EnumMember(Value = "==")]
	Equal,
	[EnumMember(Value = "!=")]
	NotEqual,
	[EnumMember(Value = ">")]
	Greater,
	[EnumMember(Value = ">=")]
	GreaterOrEqual,
	[EnumMember(Value = "<")]
	Less,
	[EnumMember(Value = "<=")]
	LessOrEqual,
	[EnumMember(Value = "===")]
	StrictlyEqual,
	[EnumMember(Value = "!==")]
	StricltyNotEqual,
	[EnumMember(Value = "&")]
	BitwiseAnd,
	[EnumMember(Value = "|")]
	BitwiseOr,
	[EnumMember(Value = "^")]
	BitwiseXOr,
	[EnumMember(Value = "<<")]
	LeftShift,
	[EnumMember(Value = ">>")]
	RightShift,
	[EnumMember(Value = ">>>")]
	UnsignedRightShift,
	[EnumMember(Value = "instanceof")]
	InstanceOf,
	[EnumMember(Value = "in")]
	In,
	[EnumMember(Value = "&&")]
	LogicalAnd,
	[EnumMember(Value = "||")]
	LogicalOr,
	[EnumMember(Value = "**")]
	Exponentiation
}
