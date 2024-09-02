using System;
using System.Collections.Generic;

namespace Esprima.Ast;

public class AssignmentExpression : Node, Expression, INode, PropertyValue, IDeclaration, IStatementListItem, ArgumentListElement, ArrayExpressionElement
{
	public readonly AssignmentOperator Operator;

	public readonly INode Left;

	public readonly Expression Right;

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(Left, Right);

	public AssignmentExpression(string op, INode left, Expression right)
		: base(Nodes.AssignmentExpression)
	{
		Operator = ParseAssignmentOperator(op);
		Left = left;
		Right = right;
	}

	public static AssignmentOperator ParseAssignmentOperator(string op)
	{
		return op switch
		{
			"=" => AssignmentOperator.Assign, 
			"+=" => AssignmentOperator.PlusAssign, 
			"-=" => AssignmentOperator.MinusAssign, 
			"*=" => AssignmentOperator.TimesAssign, 
			"/=" => AssignmentOperator.DivideAssign, 
			"%=" => AssignmentOperator.ModuloAssign, 
			"&=" => AssignmentOperator.BitwiseAndAssign, 
			"|=" => AssignmentOperator.BitwiseOrAssign, 
			"^=" => AssignmentOperator.BitwiseXOrAssign, 
			"**=" => AssignmentOperator.ExponentiationAssign, 
			"<<=" => AssignmentOperator.LeftShiftAssign, 
			">>=" => AssignmentOperator.RightShiftAssign, 
			">>>=" => AssignmentOperator.UnsignedRightShiftAssign, 
			_ => throw new ArgumentOutOfRangeException("Invalid assignment operator: " + op), 
		};
	}
}
