using Esprima.Utils;

namespace Esprima.Ast;

public enum UnaryOperator
{
	[EnumMember(Value = "+")]
	Plus,
	[EnumMember(Value = "-")]
	Minus,
	[EnumMember(Value = "~")]
	BitwiseNot,
	[EnumMember(Value = "!")]
	LogicalNot,
	[EnumMember(Value = "delete")]
	Delete,
	[EnumMember(Value = "void")]
	Void,
	[EnumMember(Value = "typeof")]
	TypeOf,
	[EnumMember(Value = "++")]
	Increment,
	[EnumMember(Value = "--")]
	Decrement
}
