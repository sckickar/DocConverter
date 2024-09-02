using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Esprima.Ast;

namespace Esprima.Utils;

public static class AstJson
{
	public sealed class Options
	{
		public static readonly Options Default = new Options();

		public bool IncludingLineColumn { get; private set; }

		public bool IncludingRange { get; private set; }

		public LocationMembersPlacement LocationMembersPlacement { get; private set; }

		public Options()
		{
		}

		private Options(Options options)
		{
			IncludingLineColumn = options.IncludingLineColumn;
			IncludingRange = options.IncludingRange;
			LocationMembersPlacement = options.LocationMembersPlacement;
		}

		public Options WithIncludingLineColumn(bool value)
		{
			if (value != IncludingLineColumn)
			{
				return new Options(this)
				{
					IncludingLineColumn = value
				};
			}
			return this;
		}

		public Options WithIncludingRange(bool value)
		{
			if (value != IncludingRange)
			{
				return new Options(this)
				{
					IncludingRange = value
				};
			}
			return this;
		}

		public Options WithLocationMembersPlacement(LocationMembersPlacement value)
		{
			if (value != LocationMembersPlacement)
			{
				return new Options(this)
				{
					LocationMembersPlacement = value
				};
			}
			return this;
		}
	}

	private sealed class Visitor : AstVisitor
	{
		private sealed class ObservableStack<T> : IDisposable
		{
			private readonly Stack<T> _stack = new Stack<T>();

			public event Action<T> Pushed;

			public event Action<T> Popped;

			public IDisposable Push(T item)
			{
				_stack.Push(item);
				this.Pushed?.Invoke(item);
				return this;
			}

			public void Dispose()
			{
				T obj = _stack.Pop();
				this.Popped?.Invoke(obj);
			}
		}

		private readonly JsonWriter _writer;

		private readonly ObservableStack<INode> _stack;

		private static readonly ConditionalWeakTable<Type, IDictionary> EnumMap = new ConditionalWeakTable<Type, IDictionary>();

		public Visitor(JsonWriter writer, bool includeLineColumn, bool includeRange, LocationMembersPlacement locationMembersPlacement)
		{
			Visitor visitor = this;
			_writer = writer ?? throw new ArgumentNullException("writer");
			_stack = new ObservableStack<INode>();
			_stack.Pushed += delegate(INode node)
			{
				visitor._writer.StartObject();
				if ((includeLineColumn || includeRange) && locationMembersPlacement == LocationMembersPlacement.Start)
				{
					WriteLocationInfo(node);
				}
				visitor.Member("type", node.Type.ToString());
			};
			_stack.Popped += delegate(INode node)
			{
				if ((includeLineColumn || includeRange) && locationMembersPlacement == LocationMembersPlacement.End)
				{
					WriteLocationInfo(node);
				}
				visitor._writer.EndObject();
			};
			void Write(Position position)
			{
				writer.StartObject();
				visitor.Member("line", position.Line);
				visitor.Member("column", position.Column);
				writer.EndObject();
			}
			void WriteLocationInfo(INode node)
			{
				if (includeRange)
				{
					writer.Member("range");
					writer.StartArray();
					writer.Number(node.Range.Start);
					writer.Number(node.Range.End);
					writer.EndArray();
				}
				if (includeLineColumn)
				{
					writer.Member("loc");
					writer.StartObject();
					writer.Member("start");
					Write(node.Location.Start);
					writer.Member("end");
					Write(node.Location.End);
					writer.EndObject();
				}
			}
		}

		private IDisposable StartNodeObject(INode node)
		{
			return _stack.Push(node);
		}

		private void EmptyNodeObject(INode node)
		{
			using (StartNodeObject(node))
			{
			}
		}

		private void Member(string name)
		{
			_writer.Member(name);
		}

		private void Member(string name, INode node)
		{
			Member(name);
			Visit(node);
		}

		private void Member(string name, string value)
		{
			Member(name);
			_writer.String(value);
		}

		private void Member(string name, bool value)
		{
			Member(name);
			_writer.Boolean(value);
		}

		private void Member(string name, int value)
		{
			Member(name);
			_writer.Number(value);
		}

		private void Member<T>(string name, T value) where T : Enum
		{
			Dictionary<T, string> dictionary = (Dictionary<T, string>)EnumMap.GetValue(value.GetType(), (Type t) => (from f in t.GetRuntimeFields()
				where f.IsStatic
				select f).ToDictionary((FieldInfo f) => (T)f.GetValue(null), delegate(FieldInfo f)
			{
				EnumMemberAttribute customAttribute = f.GetCustomAttribute<EnumMemberAttribute>();
				return (customAttribute == null) ? f.Name.ToLowerInvariant() : customAttribute.Value;
			}));
			Member(name, dictionary[value]);
		}

		private void Member<T>(string name, in NodeList<T> nodes) where T : class, INode
		{
			Member(name, in nodes, (T node) => node);
		}

		private void Member<T>(string name, in NodeList<T> list, Func<T, INode> nodeSelector) where T : class, INode
		{
			Member(name);
			_writer.StartArray();
			foreach (T item in list)
			{
				Visit(nodeSelector(item));
			}
			_writer.EndArray();
		}

		public override void Visit(INode node)
		{
			if (node != null)
			{
				base.Visit(node);
			}
			else
			{
				_writer.Null();
			}
		}

		protected override void VisitProgram(Program program)
		{
			using (StartNodeObject(program))
			{
				Member("body", in program.Body, (IStatementListItem e) => e);
				Member("sourceType", program.SourceType);
			}
		}

		protected override void VisitUnknownNode(INode node)
		{
			throw new NotSupportedException("Unknown node type: " + node.Type);
		}

		protected override void VisitCatchClause(CatchClause catchClause)
		{
			using (StartNodeObject(catchClause))
			{
				Member("param", (INode)catchClause.Param);
				Member("body", (INode)catchClause.Body);
			}
		}

		protected override void VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
		{
			using (StartNodeObject(functionDeclaration))
			{
				Member("id", (INode)functionDeclaration.Id);
				Member("params", in functionDeclaration.Params);
				Member("body", functionDeclaration.Body);
				Member("generator", functionDeclaration.Generator);
				Member("expression", functionDeclaration.Expression);
				Member("async", functionDeclaration.Async);
			}
		}

		protected override void VisitWithStatement(WithStatement withStatement)
		{
			using (StartNodeObject(withStatement))
			{
				Member("object", (INode)withStatement.Object);
				Member("body", (INode)withStatement.Body);
			}
		}

		protected override void VisitWhileStatement(WhileStatement whileStatement)
		{
			using (StartNodeObject(whileStatement))
			{
				Member("test", (INode)whileStatement.Test);
				Member("body", (INode)whileStatement.Body);
			}
		}

		protected override void VisitVariableDeclaration(VariableDeclaration variableDeclaration)
		{
			using (StartNodeObject(variableDeclaration))
			{
				Member("declarations", in variableDeclaration.Declarations);
				Member("kind", variableDeclaration.Kind);
			}
		}

		protected override void VisitTryStatement(TryStatement tryStatement)
		{
			using (StartNodeObject(tryStatement))
			{
				Member("block", (INode)tryStatement.Block);
				Member("handler", (INode)tryStatement.Handler);
				Member("finalizer", (INode)tryStatement.Finalizer);
			}
		}

		protected override void VisitThrowStatement(ThrowStatement throwStatement)
		{
			using (StartNodeObject(throwStatement))
			{
				Member("argument", (INode)throwStatement.Argument);
			}
		}

		protected override void VisitAwaitExpression(AwaitExpression awaitExpression)
		{
			using (StartNodeObject(awaitExpression))
			{
				Member("argument", (INode)awaitExpression.Argument);
			}
		}

		protected override void VisitSwitchStatement(SwitchStatement switchStatement)
		{
			using (StartNodeObject(switchStatement))
			{
				Member("discriminant", (INode)switchStatement.Discriminant);
				Member("cases", in switchStatement.Cases);
			}
		}

		protected override void VisitSwitchCase(SwitchCase switchCase)
		{
			using (StartNodeObject(switchCase))
			{
				Member("test", (INode)switchCase.Test);
				Member("consequent", in switchCase.Consequent, (IStatementListItem e) => e);
			}
		}

		protected override void VisitReturnStatement(ReturnStatement returnStatement)
		{
			using (StartNodeObject(returnStatement))
			{
				Member("argument", (INode)returnStatement.Argument);
			}
		}

		protected override void VisitLabeledStatement(LabeledStatement labeledStatement)
		{
			using (StartNodeObject(labeledStatement))
			{
				Member("label", (INode)labeledStatement.Label);
				Member("body", (INode)labeledStatement.Body);
			}
		}

		protected override void VisitIfStatement(IfStatement ifStatement)
		{
			using (StartNodeObject(ifStatement))
			{
				Member("test", (INode)ifStatement.Test);
				Member("consequent", (INode)ifStatement.Consequent);
				Member("alternate", (INode)ifStatement.Alternate);
			}
		}

		protected override void VisitEmptyStatement(EmptyStatement emptyStatement)
		{
			EmptyNodeObject(emptyStatement);
		}

		protected override void VisitDebuggerStatement(DebuggerStatement debuggerStatement)
		{
			EmptyNodeObject(debuggerStatement);
		}

		protected override void VisitExpressionStatement(ExpressionStatement expressionStatement)
		{
			using (StartNodeObject(expressionStatement))
			{
				if (expressionStatement is Directive directive)
				{
					Member("directive", directive.Directiv);
				}
				Member("expression", (INode)expressionStatement.Expression);
			}
		}

		protected override void VisitForStatement(ForStatement forStatement)
		{
			using (StartNodeObject(forStatement))
			{
				Member("init", forStatement.Init);
				Member("test", (INode)forStatement.Test);
				Member("update", (INode)forStatement.Update);
				Member("body", (INode)forStatement.Body);
			}
		}

		protected override void VisitForInStatement(ForInStatement forInStatement)
		{
			using (StartNodeObject(forInStatement))
			{
				Member("left", forInStatement.Left);
				Member("right", (INode)forInStatement.Right);
				Member("body", (INode)forInStatement.Body);
				Member("each", forInStatement.Each);
			}
		}

		protected override void VisitDoWhileStatement(DoWhileStatement doWhileStatement)
		{
			using (StartNodeObject(doWhileStatement))
			{
				Member("body", (INode)doWhileStatement.Body);
				Member("test", (INode)doWhileStatement.Test);
			}
		}

		protected override void VisitArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression)
		{
			using (StartNodeObject(arrowFunctionExpression))
			{
				Member("id", (INode)arrowFunctionExpression.Id);
				Member("params", in arrowFunctionExpression.Params);
				Member("body", arrowFunctionExpression.Body);
				Member("generator", arrowFunctionExpression.Generator);
				Member("expression", arrowFunctionExpression.Expression);
				Member("async", arrowFunctionExpression.Async);
			}
		}

		protected override void VisitUnaryExpression(UnaryExpression unaryExpression)
		{
			using (StartNodeObject(unaryExpression))
			{
				Member("operator", unaryExpression.Operator);
				Member("argument", (INode)unaryExpression.Argument);
				Member("prefix", unaryExpression.Prefix);
			}
		}

		protected override void VisitUpdateExpression(UpdateExpression updateExpression)
		{
			VisitUnaryExpression(updateExpression);
		}

		protected override void VisitThisExpression(ThisExpression thisExpression)
		{
			EmptyNodeObject(thisExpression);
		}

		protected override void VisitSequenceExpression(SequenceExpression sequenceExpression)
		{
			using (StartNodeObject(sequenceExpression))
			{
				Member("expressions", in sequenceExpression.Expressions);
			}
		}

		protected override void VisitObjectExpression(ObjectExpression objectExpression)
		{
			using (StartNodeObject(objectExpression))
			{
				Member("properties", in objectExpression.Properties);
			}
		}

		protected override void VisitNewExpression(NewExpression newExpression)
		{
			using (StartNodeObject(newExpression))
			{
				Member("callee", (INode)newExpression.Callee);
				Member("arguments", in newExpression.Arguments, (ArgumentListElement e) => e);
			}
		}

		protected override void VisitMemberExpression(MemberExpression memberExpression)
		{
			using (StartNodeObject(memberExpression))
			{
				Member("computed", memberExpression.Computed);
				Member("object", (INode)memberExpression.Object);
				Member("property", (INode)memberExpression.Property);
			}
		}

		protected override void VisitLogicalExpression(BinaryExpression binaryExpression)
		{
			VisitBinaryExpression(binaryExpression);
		}

		protected override void VisitLiteral(Literal literal)
		{
			using (StartNodeObject(literal))
			{
				_writer.Member("value");
				object value = literal.Value;
				if (value != null)
				{
					if (!(value is bool flag))
					{
						if (!(value is Regex))
						{
							if (value is double n)
							{
								_writer.Number(n);
							}
							else
							{
								_writer.String(Convert.ToString(value, CultureInfo.InvariantCulture));
							}
						}
						else
						{
							_writer.StartObject();
							_writer.EndObject();
						}
					}
					else
					{
						_writer.Boolean(flag);
					}
				}
				else
				{
					_writer.Null();
				}
				Member("raw", literal.Raw);
				if (literal.Regex != null)
				{
					_writer.Member("regex");
					_writer.StartObject();
					Member("pattern", literal.Regex.Pattern);
					Member("flags", literal.Regex.Flags);
					_writer.EndObject();
				}
			}
		}

		protected override void VisitIdentifier(Identifier identifier)
		{
			using (StartNodeObject(identifier))
			{
				Member("name", identifier.Name);
			}
		}

		protected override void VisitFunctionExpression(IFunction function)
		{
			using (StartNodeObject((Node)function))
			{
				Member("id", (INode)function.Id);
				Member("params", in function.Params);
				Member("body", function.Body);
				Member("generator", function.Generator);
				Member("expression", function.Expression);
				Member("async", function.Async);
			}
		}

		protected override void VisitClassExpression(ClassExpression classExpression)
		{
			using (StartNodeObject(classExpression))
			{
				Member("id", (INode)classExpression.Id);
				Member("superClass", (INode)classExpression.SuperClass);
				Member("body", (INode)classExpression.Body);
			}
		}

		protected override void VisitExportDefaultDeclaration(ExportDefaultDeclaration exportDefaultDeclaration)
		{
			using (StartNodeObject(exportDefaultDeclaration))
			{
				Member("declaration", exportDefaultDeclaration.Declaration.As<INode>());
			}
		}

		protected override void VisitExportAllDeclaration(ExportAllDeclaration exportAllDeclaration)
		{
			using (StartNodeObject(exportAllDeclaration))
			{
				Member("source", (INode)exportAllDeclaration.Source);
			}
		}

		protected override void VisitExportNamedDeclaration(ExportNamedDeclaration exportNamedDeclaration)
		{
			using (StartNodeObject(exportNamedDeclaration))
			{
				Member("declaration", exportNamedDeclaration.Declaration.As<INode>());
				Member("specifiers", in exportNamedDeclaration.Specifiers);
				Member("source", (INode)exportNamedDeclaration.Source);
			}
		}

		protected override void VisitExportSpecifier(ExportSpecifier exportSpecifier)
		{
			using (StartNodeObject(exportSpecifier))
			{
				Member("exported", (INode)exportSpecifier.Exported);
				Member("local", (INode)exportSpecifier.Local);
			}
		}

		protected override void VisitImport(Import import)
		{
			using (StartNodeObject(import))
			{
			}
		}

		protected override void VisitImportDeclaration(ImportDeclaration importDeclaration)
		{
			using (StartNodeObject(importDeclaration))
			{
				Member("specifiers", in importDeclaration.Specifiers, (ImportDeclarationSpecifier e) => e);
				Member("source", (INode)importDeclaration.Source);
			}
		}

		protected override void VisitImportNamespaceSpecifier(ImportNamespaceSpecifier importNamespaceSpecifier)
		{
			using (StartNodeObject(importNamespaceSpecifier))
			{
				Member("local", (INode)importNamespaceSpecifier.Local);
			}
		}

		protected override void VisitImportDefaultSpecifier(ImportDefaultSpecifier importDefaultSpecifier)
		{
			using (StartNodeObject(importDefaultSpecifier))
			{
				Member("local", (INode)importDefaultSpecifier.Local);
			}
		}

		protected override void VisitImportSpecifier(ImportSpecifier importSpecifier)
		{
			using (StartNodeObject(importSpecifier))
			{
				Member("local", (INode)importSpecifier.Local);
				Member("imported", (INode)importSpecifier.Imported);
			}
		}

		protected override void VisitMethodDefinition(MethodDefinition methodDefinition)
		{
			using (StartNodeObject(methodDefinition))
			{
				Member("key", (INode)methodDefinition.Key);
				Member("computed", methodDefinition.Computed);
				Member("value", (INode)methodDefinition.Value);
				Member("kind", methodDefinition.Kind);
				Member("static", methodDefinition.Static);
			}
		}

		protected override void VisitForOfStatement(ForOfStatement forOfStatement)
		{
			using (StartNodeObject(forOfStatement))
			{
				Member("left", forOfStatement.Left);
				Member("right", (INode)forOfStatement.Right);
				Member("body", (INode)forOfStatement.Body);
			}
		}

		protected override void VisitClassDeclaration(ClassDeclaration classDeclaration)
		{
			using (StartNodeObject(classDeclaration))
			{
				Member("id", (INode)classDeclaration.Id);
				Member("superClass", (INode)classDeclaration.SuperClass);
				Member("body", (INode)classDeclaration.Body);
			}
		}

		protected override void VisitClassBody(ClassBody classBody)
		{
			using (StartNodeObject(classBody))
			{
				Member("body", in classBody.Body);
			}
		}

		protected override void VisitYieldExpression(YieldExpression yieldExpression)
		{
			using (StartNodeObject(yieldExpression))
			{
				Member("argument", (INode)yieldExpression.Argument);
				Member("delegate", yieldExpression.Delegate);
			}
		}

		protected override void VisitTaggedTemplateExpression(TaggedTemplateExpression taggedTemplateExpression)
		{
			using (StartNodeObject(taggedTemplateExpression))
			{
				Member("tag", (INode)taggedTemplateExpression.Tag);
				Member("quasi", (INode)taggedTemplateExpression.Quasi);
			}
		}

		protected override void VisitSuper(Super super)
		{
			EmptyNodeObject(super);
		}

		protected override void VisitMetaProperty(MetaProperty metaProperty)
		{
			using (StartNodeObject(metaProperty))
			{
				Member("meta", (INode)metaProperty.Meta);
				Member("property", (INode)metaProperty.Property);
			}
		}

		protected override void VisitArrowParameterPlaceHolder(ArrowParameterPlaceHolder arrowParameterPlaceHolder)
		{
			throw new NotImplementedException();
		}

		protected override void VisitObjectPattern(ObjectPattern objectPattern)
		{
			using (StartNodeObject(objectPattern))
			{
				Member("properties", in objectPattern.Properties);
			}
		}

		protected override void VisitSpreadElement(SpreadElement spreadElement)
		{
			using (StartNodeObject(spreadElement))
			{
				Member("argument", (INode)spreadElement.Argument);
			}
		}

		protected override void VisitAssignmentPattern(AssignmentPattern assignmentPattern)
		{
			using (StartNodeObject(assignmentPattern))
			{
				Member("left", assignmentPattern.Left);
				Member("right", assignmentPattern.Right);
			}
		}

		protected override void VisitArrayPattern(ArrayPattern arrayPattern)
		{
			using (StartNodeObject(arrayPattern))
			{
				Member("elements", in arrayPattern.Elements);
			}
		}

		protected override void VisitVariableDeclarator(VariableDeclarator variableDeclarator)
		{
			using (StartNodeObject(variableDeclarator))
			{
				Member("id", (INode)variableDeclarator.Id);
				Member("init", (INode)variableDeclarator.Init);
			}
		}

		protected override void VisitTemplateLiteral(TemplateLiteral templateLiteral)
		{
			using (StartNodeObject(templateLiteral))
			{
				Member("quasis", in templateLiteral.Quasis);
				Member("expressions", in templateLiteral.Expressions);
			}
		}

		protected override void VisitTemplateElement(TemplateElement templateElement)
		{
			using (StartNodeObject(templateElement))
			{
				_writer.Member("value");
				_writer.StartObject();
				Member("raw", templateElement.Value.Raw);
				Member("cooked", templateElement.Value.Cooked);
				_writer.EndObject();
				Member("tail", templateElement.Tail);
			}
		}

		protected override void VisitRestElement(RestElement restElement)
		{
			using (StartNodeObject(restElement))
			{
				Member("argument", restElement.Argument);
			}
		}

		protected override void VisitProperty(Property property)
		{
			using (StartNodeObject(property))
			{
				Member("key", (INode)property.Key);
				Member("computed", property.Computed);
				Member("value", (INode)property.Value);
				Member("kind", property.Kind);
				Member("method", property.Method);
				Member("shorthand", property.Shorthand);
			}
		}

		protected override void VisitConditionalExpression(ConditionalExpression conditionalExpression)
		{
			using (StartNodeObject(conditionalExpression))
			{
				Member("test", (INode)conditionalExpression.Test);
				Member("consequent", (INode)conditionalExpression.Consequent);
				Member("alternate", (INode)conditionalExpression.Alternate);
			}
		}

		protected override void VisitCallExpression(CallExpression callExpression)
		{
			using (StartNodeObject(callExpression))
			{
				Member("callee", (INode)callExpression.Callee);
				Member("arguments", in callExpression.Arguments, (ArgumentListElement e) => (Expression)e);
			}
		}

		protected override void VisitBinaryExpression(BinaryExpression binaryExpression)
		{
			using (StartNodeObject(binaryExpression))
			{
				Member("operator", binaryExpression.Operator);
				Member("left", (INode)binaryExpression.Left);
				Member("right", (INode)binaryExpression.Right);
			}
		}

		protected override void VisitArrayExpression(ArrayExpression arrayExpression)
		{
			using (StartNodeObject(arrayExpression))
			{
				Member("elements", in arrayExpression.Elements);
			}
		}

		protected override void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
		{
			using (StartNodeObject(assignmentExpression))
			{
				Member("operator", assignmentExpression.Operator);
				Member("left", assignmentExpression.Left);
				Member("right", (INode)assignmentExpression.Right);
			}
		}

		protected override void VisitContinueStatement(ContinueStatement continueStatement)
		{
			using (StartNodeObject(continueStatement))
			{
				Member("label", (INode)continueStatement.Label);
			}
		}

		protected override void VisitBreakStatement(BreakStatement breakStatement)
		{
			using (StartNodeObject(breakStatement))
			{
				Member("label", (INode)breakStatement.Label);
			}
		}

		protected override void VisitBlockStatement(BlockStatement blockStatement)
		{
			using (StartNodeObject(blockStatement))
			{
				Member("body", in blockStatement.Body, (IStatementListItem e) => (Statement)e);
			}
		}
	}

	public static string ToJsonString(this INode node)
	{
		return node.ToJsonString((string)null);
	}

	public static string ToJsonString(this INode node, string indent)
	{
		return node.ToJsonString(Options.Default, indent);
	}

	public static string ToJsonString(this INode node, Options options)
	{
		return node.ToJsonString(options, null);
	}

	public static string ToJsonString(this INode node, Options options, string indent)
	{
		using StringWriter stringWriter = new StringWriter();
		node.WriteJson(stringWriter, options, indent);
		return stringWriter.ToString();
	}

	public static void WriteJson(this INode node, TextWriter writer)
	{
		node.WriteJson(writer, (string)null);
	}

	public static void WriteJson(this INode node, TextWriter writer, string indent)
	{
		node.WriteJson(writer, Options.Default, indent);
	}

	public static void WriteJson(this INode node, TextWriter writer, Options options)
	{
		node.WriteJson(writer, options, null);
	}

	public static void WriteJson(this INode node, TextWriter writer, Options options, string indent)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		new Visitor(new JsonTextWriter(writer, indent), options.IncludingLineColumn, options.IncludingRange, options.LocationMembersPlacement).Visit(node);
	}

	public static void WriteJson(this INode node, JsonWriter writer, Options options)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		new Visitor(writer, options.IncludingLineColumn, options.IncludingRange, options.LocationMembersPlacement).Visit(node);
	}
}
