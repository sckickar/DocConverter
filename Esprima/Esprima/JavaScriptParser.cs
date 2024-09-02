using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Esprima.Ast;

namespace Esprima;

public class JavaScriptParser
{
	private sealed class Context
	{
		public bool IsModule;

		public bool AllowIn;

		public bool AllowStrictDirective;

		public bool AllowYield;

		public bool Await;

		public Token FirstCoverInitializedNameError;

		public bool IsAssignmentTarget;

		public bool IsBindingElement;

		public bool InFunctionBody;

		public bool InIteration;

		public bool InSwitch;

		public bool Strict;

		public HashSet<string> LabelSet;
	}

	private struct HoistingScopeLists
	{
		public ArrayList<IFunctionDeclaration> FunctionDeclarations;

		public ArrayList<VariableDeclaration> VariableDeclarations;
	}

	private class ParsedParameters
	{
		private HashSet<string> paramSet;

		public Token FirstRestricted;

		public string Message;

		public ArrayList<INode> Parameters;

		public Token Stricted;

		public bool Simple;

		public bool ParamSetContains(string key)
		{
			if (paramSet != null)
			{
				return paramSet.Contains(key);
			}
			return false;
		}

		public void ParamSetAdd(string key)
		{
			HashSet<string> obj = paramSet ?? new HashSet<string>();
			HashSet<string> hashSet = obj;
			paramSet = obj;
			hashSet.Add(key);
		}
	}

	private static readonly HashSet<string> AssignmentOperators = new HashSet<string>
	{
		"=", "*=", "**=", "/=", "%=", "+=", "-=", "<<=", ">>=", ">>>=",
		"&=", "^=", "|="
	};

	private readonly Stack<HoistingScopeLists> _hoistingScopes = new Stack<HoistingScopeLists>();

	private Token _lookahead;

	private readonly Context _context;

	private readonly Marker _startMarker;

	private readonly Marker _lastMarker;

	private readonly Scanner _scanner;

	private readonly IErrorHandler _errorHandler;

	private readonly ParserOptions _config;

	private bool _hasLineTerminator;

	private readonly Action<INode> _action;

	internal List<Token> Tokens = new List<Token>();

	private readonly Func<Expression> parseAssignmentExpression;

	private readonly Func<Expression> parseExponentiationExpression;

	private readonly Func<Expression> parseUnaryExpression;

	private readonly Func<Expression> parseExpression;

	private readonly Func<Node> parseNewExpression;

	private readonly Func<Expression> parsePrimaryExpression;

	private readonly Func<INode> parseGroupExpression;

	private readonly Func<Expression> parseArrayInitializer;

	private readonly Func<Expression> parseObjectInitializer;

	private readonly Func<Expression> parseBinaryExpression;

	private readonly Func<Expression> parseLeftHandSideExpression;

	private readonly Func<Expression> parseLeftHandSideExpressionAllowCall;

	private readonly Func<Statement> parseStatement;

	private Token cacheToken;

	private const int MaxAssignmentDepth = 100;

	private int _assignmentDepth;

	private static readonly HashSet<string> PunctuatorExpressionStart = new HashSet<string>
	{
		"[", "(", "{", "+", "-", "!", "~", "++", "--", "/",
		"/="
	};

	private static readonly HashSet<string> KeywordExpressionStart = new HashSet<string> { "class", "delete", "function", "let", "new", "super", "this", "typeof", "void", "yield" };

	public JavaScriptParser(string code)
		: this(code, new ParserOptions())
	{
	}

	public JavaScriptParser(string code, ParserOptions options)
		: this(code, options, null)
	{
	}

	public JavaScriptParser(string code, ParserOptions options, Action<INode> _action)
	{
		if (code == null)
		{
			throw new ArgumentNullException("code");
		}
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		parseAssignmentExpression = ParseAssignmentExpression;
		parseExponentiationExpression = ParseExponentiationExpression;
		parseUnaryExpression = ParseUnaryExpression;
		parseExpression = ParseExpression;
		parseNewExpression = ParseNewExpression;
		parsePrimaryExpression = ParsePrimaryExpression;
		parseGroupExpression = ParseGroupExpression;
		parseArrayInitializer = ParseArrayInitializer;
		parseObjectInitializer = ParseObjectInitializer;
		parseBinaryExpression = ParseBinaryExpression;
		parseLeftHandSideExpression = ParseLeftHandSideExpression;
		parseLeftHandSideExpressionAllowCall = ParseLeftHandSideExpressionAllowCall;
		parseStatement = ParseStatement;
		_config = options;
		this._action = _action;
		_errorHandler = _config.ErrorHandler;
		_errorHandler.Tolerant = _config.Tolerant;
		_scanner = new Scanner(code, _config);
		_context = new Context
		{
			Await = false,
			AllowIn = true,
			AllowYield = true,
			AllowStrictDirective = true,
			FirstCoverInitializedNameError = null,
			IsAssignmentTarget = false,
			IsBindingElement = false,
			InFunctionBody = false,
			InIteration = false,
			InSwitch = false,
			LabelSet = new HashSet<string>(),
			Strict = false
		};
		_startMarker = new Marker
		{
			Index = 0,
			Line = _scanner.LineNumber,
			Column = 0
		};
		_lastMarker = new Marker
		{
			Index = 0,
			Line = _scanner.LineNumber,
			Column = 0
		};
		NextToken();
		_lastMarker = new Marker
		{
			Index = _scanner.Index,
			Line = _scanner.LineNumber,
			Column = _scanner.Index - _scanner.LineStart
		};
	}

	[Obsolete("Should use explicit ParseScript or ParseModule")]
	public Program ParseProgram()
	{
		if (_config.SourceType != SourceType.Script)
		{
			return ParseModule();
		}
		return ParseScript();
	}

	public Module ParseModule()
	{
		_context.Strict = true;
		_context.IsModule = true;
		_scanner.IsModule = true;
		EnterHoistingScope();
		Marker marker = CreateNode();
		ArrayList<IStatementListItem> arrayList = ParseDirectivePrologues();
		while (_lookahead.Type != TokenType.EOF)
		{
			arrayList.Push(ParseStatementListItem());
		}
		NodeList<IStatementListItem> body = NodeList.From(ref arrayList);
		return Finalize(marker, new Module(in body, LeaveHoistingScope()));
	}

	public Script ParseScript(bool strict = false)
	{
		if (strict)
		{
			_context.Strict = true;
		}
		EnterHoistingScope();
		Marker marker = CreateNode();
		ArrayList<IStatementListItem> arrayList = ParseDirectivePrologues();
		while (_lookahead.Type != TokenType.EOF)
		{
			arrayList.Push(ParseStatementListItem());
		}
		NodeList<IStatementListItem> body = NodeList.From(ref arrayList);
		return Finalize(marker, new Script(in body, _context.Strict, LeaveHoistingScope()));
	}

	private void CollectComments()
	{
		if (!_config.Comment)
		{
			_scanner.ScanCommentsInternal();
			return;
		}
		ArrayList<Comment> arrayList = _scanner.ScanCommentsInternal();
		if (arrayList.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < arrayList.Count; i++)
		{
			Comment comment = arrayList[i];
			Comment comment2 = default(Comment);
			comment2.Type = ((!comment.MultiLine) ? CommentType.Line : CommentType.Block);
			comment2.Value = _scanner.Source.Slice(comment.Slice[0], comment.Slice[1]);
			if (_config.Range)
			{
				comment2.Start = comment.Start;
				comment2.End = comment.End;
			}
			if (_config.Loc)
			{
				comment2.Loc = comment.Loc;
			}
		}
	}

	private string GetTokenRaw(Token token)
	{
		return _scanner.Source.Slice(token.Start, token.End);
	}

	private Token ConvertToken(Token token)
	{
		Token token2 = new Token
		{
			Type = token.Type,
			Value = GetTokenRaw(token)
		};
		if (_config.Range)
		{
			token2.Start = token.Start;
			token2.End = token.End;
		}
		if (_config.Loc)
		{
			Position start = new Position(_startMarker.Line, _startMarker.Column);
			Position end = new Position(_scanner.LineNumber, _scanner.Index - _scanner.LineStart);
			token2.Location = token2.Location.WithPosition(start, end);
		}
		if (token.RegexValue != null)
		{
			token2.RegexValue = token.RegexValue;
		}
		return token2;
	}

	private Token NextToken()
	{
		Token lookahead = _lookahead;
		_lastMarker.Index = _scanner.Index;
		_lastMarker.Line = _scanner.LineNumber;
		_lastMarker.Column = _scanner.Index - _scanner.LineStart;
		CollectComments();
		if (_scanner.Index != _startMarker.Index)
		{
			_startMarker.Index = _scanner.Index;
			_startMarker.Line = _scanner.LineNumber;
			_startMarker.Column = _scanner.Index - _scanner.LineStart;
		}
		Token token = _scanner.Lex();
		_hasLineTerminator = lookahead != null && token != null && lookahead.LineNumber != token.LineNumber;
		if (token != null && _context.Strict && token.Type == TokenType.Identifier && Scanner.IsStrictModeReservedWord((string)token.Value))
		{
			token.Type = TokenType.Keyword;
		}
		_lookahead = token;
		if (_config.Tokens && token.Type != TokenType.EOF)
		{
			Tokens.Add(ConvertToken(token));
		}
		return lookahead;
	}

	private Token NextRegexToken()
	{
		CollectComments();
		Token token = _scanner.ScanRegExp();
		if (_config.Tokens)
		{
			Tokens.RemoveAt(Tokens.Count - 1);
			Tokens.Add(ConvertToken(token));
		}
		_lookahead = token;
		NextToken();
		return token;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private Marker CreateNode()
	{
		return new Marker(_startMarker.Index, _startMarker.Line, _startMarker.Column);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Marker StartNode(Token token, int lastLineStart = 0)
	{
		int num = token.Start - token.LineStart;
		int num2 = token.LineNumber;
		if (num < 0)
		{
			num += lastLineStart;
			num2--;
		}
		return new Marker(token.Start, num2, num);
	}

	private T Finalize<T>(Marker marker, T node) where T : INode
	{
		if (_config.Range)
		{
			node.Range = new Esprima.Ast.Range(marker.Index, _lastMarker.Index);
		}
		if (_config.Loc)
		{
			Position start = new Position(marker.Line, marker.Column);
			Position end = new Position(_lastMarker.Line, _lastMarker.Column);
			node.Location = new Location(start, end, _errorHandler.Source);
		}
		_action?.Invoke(node);
		return node;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void Expect(string value)
	{
		Token token = NextToken();
		if (token.Type != TokenType.Punctuator || !value.Equals(token.Value))
		{
			ThrowUnexpectedToken(token);
		}
	}

	private void ExpectCommaSeparator()
	{
		if (_config.Tolerant)
		{
			Token lookahead = _lookahead;
			if (lookahead.Type == TokenType.Punctuator && ",".Equals(lookahead.Value))
			{
				NextToken();
			}
			else if (lookahead.Type == TokenType.Punctuator && ";".Equals(lookahead.Value))
			{
				NextToken();
				TolerateUnexpectedToken(lookahead);
			}
			else
			{
				TolerateUnexpectedToken(lookahead, "Unexpected token {0}");
			}
		}
		else
		{
			Expect(",");
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ExpectKeyword(string keyword)
	{
		Token token = NextToken();
		if (token.Type != TokenType.Keyword || !keyword.Equals(token.Value))
		{
			ThrowUnexpectedToken(token);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool Match(string value)
	{
		if (_lookahead.Type == TokenType.Punctuator)
		{
			return value.Equals(_lookahead.Value);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool MatchKeyword(string keyword)
	{
		if (_lookahead.Type == TokenType.Keyword)
		{
			return keyword.Equals(_lookahead.Value);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool MatchContextualKeyword(string keyword)
	{
		if (_lookahead.Type == TokenType.Identifier)
		{
			return keyword.Equals(_lookahead.Value);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool MatchAssign()
	{
		if (_lookahead.Type != TokenType.Punctuator)
		{
			return false;
		}
		string item = (string)_lookahead.Value;
		return AssignmentOperators.Contains(item);
	}

	private T IsolateCoverGrammar<T>(Func<T> parseFunction)
	{
		bool isBindingElement = _context.IsBindingElement;
		bool isAssignmentTarget = _context.IsAssignmentTarget;
		Token firstCoverInitializedNameError = _context.FirstCoverInitializedNameError;
		_context.IsBindingElement = true;
		_context.IsAssignmentTarget = true;
		_context.FirstCoverInitializedNameError = null;
		T result = parseFunction();
		if (_context.FirstCoverInitializedNameError != null)
		{
			ThrowUnexpectedToken(_context.FirstCoverInitializedNameError);
		}
		_context.IsBindingElement = isBindingElement;
		_context.IsAssignmentTarget = isAssignmentTarget;
		_context.FirstCoverInitializedNameError = firstCoverInitializedNameError;
		return result;
	}

	private T InheritCoverGrammar<T>(Func<T> parseFunction) where T : class, INode
	{
		bool isBindingElement = _context.IsBindingElement;
		bool isAssignmentTarget = _context.IsAssignmentTarget;
		Token firstCoverInitializedNameError = _context.FirstCoverInitializedNameError;
		_context.IsBindingElement = true;
		_context.IsAssignmentTarget = true;
		_context.FirstCoverInitializedNameError = null;
		T result = parseFunction();
		_context.IsBindingElement = _context.IsBindingElement && isBindingElement;
		_context.IsAssignmentTarget = _context.IsAssignmentTarget && isAssignmentTarget;
		_context.FirstCoverInitializedNameError = firstCoverInitializedNameError ?? _context.FirstCoverInitializedNameError;
		return result;
	}

	private void ConsumeSemicolon()
	{
		if (Match(";"))
		{
			NextToken();
		}
		else if (!_hasLineTerminator)
		{
			if (_lookahead.Type != TokenType.EOF && !Match("}"))
			{
				ThrowUnexpectedToken(_lookahead);
			}
			_lastMarker.Index = _startMarker.Index;
			_lastMarker.Line = _startMarker.Line;
			_lastMarker.Column = _startMarker.Column;
		}
	}

	private Expression ParsePrimaryExpression()
	{
		Marker marker = CreateNode();
		Expression result = null;
		switch (_lookahead.Type)
		{
		case TokenType.Identifier:
		{
			if ((_context.IsModule || _context.Await) && "await".Equals(_lookahead.Value))
			{
				TolerateUnexpectedToken(_lookahead);
			}
			Expression expression2;
			if (!MatchAsyncFunction())
			{
				Expression expression = Finalize(marker, new Identifier((string)NextToken().Value));
				expression2 = expression;
			}
			else
			{
				Expression expression = ParseFunctionExpression();
				expression2 = expression;
			}
			result = expression2;
			break;
		}
		case TokenType.StringLiteral:
		{
			if (_context.Strict && _lookahead.Octal)
			{
				TolerateUnexpectedToken(_lookahead, "Octal literals are not allowed in strict mode.");
			}
			_context.IsAssignmentTarget = false;
			_context.IsBindingElement = false;
			Token token = NextToken();
			string tokenRaw = GetTokenRaw(token);
			result = Finalize(marker, new Literal((string)token.Value, tokenRaw));
			break;
		}
		case TokenType.NumericLiteral:
		{
			if (_context.Strict && _lookahead.Octal)
			{
				TolerateUnexpectedToken(_lookahead, "Octal literals are not allowed in strict mode.");
			}
			_context.IsAssignmentTarget = false;
			_context.IsBindingElement = false;
			Token token = NextToken();
			string tokenRaw = GetTokenRaw(token);
			result = Finalize(marker, new Literal(token.NumericValue, tokenRaw));
			break;
		}
		case TokenType.BooleanLiteral:
		{
			_context.IsAssignmentTarget = false;
			_context.IsBindingElement = false;
			Token token = NextToken();
			string tokenRaw = GetTokenRaw(token);
			result = Finalize(marker, new Literal("true".Equals(token.Value), tokenRaw));
			break;
		}
		case TokenType.NullLiteral:
		{
			_context.IsAssignmentTarget = false;
			_context.IsBindingElement = false;
			Token token = NextToken();
			string tokenRaw = GetTokenRaw(token);
			result = Finalize(marker, new Literal(TokenType.NullLiteral, null, tokenRaw));
			break;
		}
		case TokenType.Template:
			result = ParseTemplateLiteral();
			break;
		case TokenType.Punctuator:
			switch ((string)_lookahead.Value)
			{
			case "(":
				_context.IsBindingElement = false;
				result = InheritCoverGrammar(parseGroupExpression).As<Expression>();
				break;
			case "[":
				result = InheritCoverGrammar(parseArrayInitializer);
				break;
			case "{":
				result = InheritCoverGrammar(parseObjectInitializer);
				break;
			case "/":
			case "/=":
			{
				_context.IsAssignmentTarget = false;
				_context.IsBindingElement = false;
				_scanner.Index = _startMarker.Index;
				Token token = NextRegexToken();
				string tokenRaw = GetTokenRaw(token);
				result = Finalize(marker, new Literal(token.RegexValue.Pattern, token.RegexValue.Flags, token.Value, tokenRaw));
				break;
			}
			default:
				ThrowUnexpectedToken(NextToken());
				break;
			}
			break;
		case TokenType.Keyword:
			if (!_context.Strict && _context.AllowYield && MatchKeyword("yield"))
			{
				result = ParseIdentifierName();
				break;
			}
			if (!_context.Strict && MatchKeyword("let"))
			{
				result = Finalize(marker, new Identifier((string)NextToken().Value));
				break;
			}
			_context.IsAssignmentTarget = false;
			_context.IsBindingElement = false;
			if (MatchKeyword("function"))
			{
				result = ParseFunctionExpression();
			}
			else if (MatchKeyword("this"))
			{
				NextToken();
				result = Finalize(marker, new ThisExpression());
			}
			else if (MatchKeyword("class"))
			{
				result = ParseClassExpression();
			}
			else if (MatchImportCall())
			{
				result = ParseImportCall();
			}
			else
			{
				ThrowUnexpectedToken(NextToken());
			}
			break;
		default:
			ThrowUnexpectedToken(NextToken());
			break;
		}
		return result;
	}

	private bool IsLeftHandSide(Expression expr)
	{
		if (expr.Type != Nodes.Identifier)
		{
			return expr.Type == Nodes.MemberExpression;
		}
		return true;
	}

	private SpreadElement ParseSpreadElement()
	{
		Marker marker = CreateNode();
		Expect("...");
		Expression argument = InheritCoverGrammar(parseAssignmentExpression);
		return Finalize(marker, new SpreadElement(argument));
	}

	private ArrayExpression ParseArrayInitializer()
	{
		Marker marker = CreateNode();
		ArrayList<ArrayExpressionElement> arrayList = default(ArrayList<ArrayExpressionElement>);
		Expect("[");
		while (!Match("]"))
		{
			if (Match(","))
			{
				NextToken();
				arrayList.Add(null);
			}
			else if (Match("..."))
			{
				SpreadElement item = ParseSpreadElement();
				if (!Match("]"))
				{
					_context.IsAssignmentTarget = false;
					_context.IsBindingElement = false;
					Expect(",");
				}
				arrayList.Add(item);
			}
			else
			{
				arrayList.Add(InheritCoverGrammar(parseAssignmentExpression));
				if (!Match("]"))
				{
					Expect(",");
				}
			}
		}
		Expect("]");
		NodeList<ArrayExpressionElement> elements = NodeList.From(ref arrayList);
		return Finalize(marker, new ArrayExpression(in elements));
	}

	private BlockStatement ParsePropertyMethod(ParsedParameters parameters)
	{
		_context.IsAssignmentTarget = false;
		_context.IsBindingElement = false;
		bool strict = _context.Strict;
		bool allowStrictDirective = _context.AllowStrictDirective;
		_context.AllowStrictDirective = parameters.Simple;
		BlockStatement result = IsolateCoverGrammar(ParseFunctionSourceElements);
		if (_context.Strict && parameters.FirstRestricted != null)
		{
			TolerateUnexpectedToken(parameters.FirstRestricted, parameters.Message);
		}
		if (_context.Strict && parameters.Stricted != null)
		{
			TolerateUnexpectedToken(parameters.Stricted, parameters.Message);
		}
		_context.Strict = strict;
		_context.AllowStrictDirective = allowStrictDirective;
		return result;
	}

	private FunctionExpression ParsePropertyMethodFunction()
	{
		EnterHoistingScope();
		Marker marker = CreateNode();
		bool allowYield = _context.AllowYield;
		_context.AllowYield = true;
		ParsedParameters parsedParameters = ParseFormalParameters();
		BlockStatement body = ParsePropertyMethod(parsedParameters);
		_context.AllowYield = allowYield;
		NodeList<INode> parameters = NodeList.From(ref parsedParameters.Parameters);
		return Finalize(marker, new FunctionExpression(null, in parameters, body, generator: false, _context.Strict, async: false, LeaveHoistingScope()));
	}

	private FunctionExpression ParsePropertyMethodAsyncFunction()
	{
		EnterHoistingScope();
		Marker marker = CreateNode();
		bool allowYield = _context.AllowYield;
		bool await = _context.Await;
		_context.AllowYield = false;
		_context.Await = true;
		ParsedParameters parsedParameters = ParseFormalParameters();
		BlockStatement body = ParsePropertyMethod(parsedParameters);
		_context.AllowYield = allowYield;
		_context.Await = await;
		NodeList<INode> parameters = NodeList.From(ref parsedParameters.Parameters);
		return Finalize(marker, new FunctionExpression(null, in parameters, body, generator: false, strict: true, async: true, LeaveHoistingScope()));
	}

	private Expression ParseObjectPropertyKey()
	{
		Marker marker = CreateNode();
		Token token = NextToken();
		Expression result = null;
		switch (token.Type)
		{
		case TokenType.StringLiteral:
		{
			if (_context.Strict && token.Octal)
			{
				TolerateUnexpectedToken(token, "Octal literals are not allowed in strict mode.");
			}
			string tokenRaw = GetTokenRaw(token);
			result = Finalize(marker, new Literal((string)token.Value, tokenRaw));
			break;
		}
		case TokenType.NumericLiteral:
		{
			if (_context.Strict && token.Octal)
			{
				TolerateUnexpectedToken(token, "Octal literals are not allowed in strict mode.");
			}
			string tokenRaw = GetTokenRaw(token);
			result = Finalize(marker, new Literal(token.NumericValue, tokenRaw));
			break;
		}
		case TokenType.BooleanLiteral:
		case TokenType.Identifier:
		case TokenType.Keyword:
		case TokenType.NullLiteral:
			result = Finalize(marker, new Identifier((string)token.Value));
			break;
		case TokenType.Punctuator:
			if ("[".Equals(token.Value))
			{
				result = IsolateCoverGrammar(parseAssignmentExpression);
				Expect("]");
			}
			else
			{
				ThrowUnexpectedToken(token);
			}
			break;
		default:
			ThrowUnexpectedToken(token);
			break;
		}
		return result;
	}

	private static bool IsPropertyKey(INode key, string value)
	{
		if (key.Type == Nodes.Identifier)
		{
			return value.Equals(key.As<Identifier>().Name);
		}
		if (key.Type == Nodes.Literal)
		{
			return value.Equals(key.As<Literal>().StringValue);
		}
		return false;
	}

	private Property ParseObjectProperty(Token hasProto)
	{
		Marker marker = CreateNode();
		Token lookahead = _lookahead;
		Expression expression = null;
		PropertyValue value = null;
		bool flag = false;
		bool method = false;
		bool shorthand = false;
		bool flag2 = false;
		if (lookahead.Type == TokenType.Identifier)
		{
			string text = (string)lookahead.Value;
			NextToken();
			flag = Match("[");
			flag2 = !_hasLineTerminator && text == "async" && !Match(":") && !Match("(") && !Match("*") && !Match(",");
			Expression expression3;
			if (!flag2)
			{
				Expression expression2 = Finalize(marker, new Identifier(text));
				expression3 = expression2;
			}
			else
			{
				expression3 = ParseObjectPropertyKey();
			}
			expression = expression3;
		}
		else if (Match("*"))
		{
			NextToken();
		}
		else
		{
			flag = Match("[");
			expression = ParseObjectPropertyKey();
		}
		bool flag3 = QualifiedPropertyName(_lookahead);
		PropertyKind kind;
		if (lookahead.Type == TokenType.Identifier && !flag2 && "get".Equals(lookahead.Value) && flag3)
		{
			kind = PropertyKind.Get;
			flag = Match("[");
			expression = ParseObjectPropertyKey();
			_context.AllowYield = false;
			value = ParseGetterMethod();
		}
		else if (lookahead.Type == TokenType.Identifier && !flag2 && "set".Equals(lookahead.Value) && flag3)
		{
			kind = PropertyKind.Set;
			flag = Match("[");
			expression = ParseObjectPropertyKey();
			value = ParseSetterMethod();
		}
		else if (lookahead.Type == TokenType.Punctuator && "*".Equals(lookahead.Value) && flag3)
		{
			kind = PropertyKind.Init;
			flag = Match("[");
			expression = ParseObjectPropertyKey();
			value = ParseGeneratorMethod();
			method = true;
		}
		else
		{
			if (expression == null)
			{
				ThrowUnexpectedToken(_lookahead);
			}
			kind = PropertyKind.Init;
			if (Match(":") && !flag2)
			{
				if (!flag && IsPropertyKey(expression, "__proto__"))
				{
					if (hasProto.Value != null)
					{
						TolerateError("Duplicate __proto__ fields are not allowed in object literals");
					}
					hasProto.Value = "true";
					hasProto.BooleanValue = true;
				}
				NextToken();
				value = InheritCoverGrammar(parseAssignmentExpression);
			}
			else if (Match("("))
			{
				PropertyValue propertyValue2;
				if (!flag2)
				{
					PropertyValue propertyValue = ParsePropertyMethodFunction();
					propertyValue2 = propertyValue;
				}
				else
				{
					PropertyValue propertyValue = ParsePropertyMethodAsyncFunction();
					propertyValue2 = propertyValue;
				}
				value = propertyValue2;
				method = true;
			}
			else if (lookahead.Type == TokenType.Identifier)
			{
				Identifier identifier = (Identifier)expression;
				if (Match("="))
				{
					_context.FirstCoverInitializedNameError = _lookahead;
					NextToken();
					shorthand = true;
					Expression right = IsolateCoverGrammar(parseAssignmentExpression);
					value = Finalize(marker, new AssignmentPattern(identifier, right));
				}
				else
				{
					shorthand = true;
					value = identifier;
				}
			}
			else
			{
				ThrowUnexpectedToken(NextToken());
			}
		}
		return Finalize(marker, new Property(kind, expression, flag, value, method, shorthand));
	}

	private ObjectExpression ParseObjectInitializer()
	{
		Marker marker = CreateNode();
		ArrayList<ObjectExpressionProperty> arrayList = default(ArrayList<ObjectExpressionProperty>);
		Token token = RentToken();
		token.Value = "false";
		token.BooleanValue = false;
		Expect("{");
		while (!Match("}"))
		{
			ObjectExpressionProperty objectExpressionProperty2;
			if (!Match("..."))
			{
				ObjectExpressionProperty objectExpressionProperty = ParseObjectProperty(token);
				objectExpressionProperty2 = objectExpressionProperty;
			}
			else
			{
				ObjectExpressionProperty objectExpressionProperty = ParseSpreadElement();
				objectExpressionProperty2 = objectExpressionProperty;
			}
			ObjectExpressionProperty item = objectExpressionProperty2;
			arrayList.Add(item);
			if (!Match("}"))
			{
				ExpectCommaSeparator();
			}
		}
		Expect("}");
		ReturnToken(token);
		NodeList<ObjectExpressionProperty> properties = NodeList.From(ref arrayList);
		return Finalize(marker, new ObjectExpression(in properties));
	}

	private Token RentToken()
	{
		if (cacheToken != null)
		{
			cacheToken.Clear();
			return cacheToken;
		}
		return new Token();
	}

	private void ReturnToken(Token t)
	{
		cacheToken = t;
	}

	private TemplateElement ParseTemplateHead()
	{
		Marker marker = CreateNode();
		Token token = NextToken();
		TemplateElement.TemplateElementValue value = new TemplateElement.TemplateElementValue
		{
			Raw = token.RawTemplate,
			Cooked = (string)token.Value
		};
		return Finalize(marker, new TemplateElement(value, token.Tail));
	}

	private TemplateElement ParseTemplateElement()
	{
		if (_lookahead.Type != TokenType.Template)
		{
			ThrowUnexpectedToken();
		}
		Marker marker = CreateNode();
		Token token = NextToken();
		TemplateElement.TemplateElementValue value = new TemplateElement.TemplateElementValue
		{
			Raw = token.RawTemplate,
			Cooked = (string)token.Value
		};
		return Finalize(marker, new TemplateElement(value, token.Tail));
	}

	private TemplateLiteral ParseTemplateLiteral()
	{
		Marker marker = CreateNode();
		ArrayList<Expression> arrayList = default(ArrayList<Expression>);
		ArrayList<TemplateElement> arrayList2 = default(ArrayList<TemplateElement>);
		TemplateElement templateElement = ParseTemplateHead();
		arrayList2.Add(templateElement);
		while (!templateElement.Tail)
		{
			arrayList.Add(ParseExpression());
			templateElement = ParseTemplateElement();
			arrayList2.Add(templateElement);
		}
		NodeList<TemplateElement> quasis = NodeList.From(ref arrayList2);
		NodeList<Expression> expressions = NodeList.From(ref arrayList);
		return Finalize(marker, new TemplateLiteral(in quasis, in expressions));
	}

	private INode ReinterpretExpressionAsPattern(INode expr)
	{
		INode node = expr;
		switch (expr.Type)
		{
		case Nodes.SpreadElement:
			node = new RestElement(ReinterpretExpressionAsPattern(expr.As<SpreadElement>().Argument).As<IArrayPatternElement>());
			node.Range = expr.Range;
			node.Location = (_config.Loc ? expr.Location : default(Location));
			break;
		case Nodes.ArrayExpression:
		{
			ArrayList<IArrayPatternElement> arrayList = default(ArrayList<IArrayPatternElement>);
			foreach (ArrayExpressionElement element in expr.As<ArrayExpression>().Elements)
			{
				if (element != null)
				{
					arrayList.Add(ReinterpretExpressionAsPattern(element).As<IArrayPatternElement>());
				}
				else
				{
					arrayList.Add(null);
				}
			}
			NodeList<IArrayPatternElement> elements = NodeList.From(ref arrayList);
			node = new ArrayPattern(in elements);
			node.Range = expr.Range;
			node.Location = (_config.Loc ? expr.Location : default(Location));
			break;
		}
		case Nodes.ObjectExpression:
		{
			ArrayList<INode> arrayList2 = default(ArrayList<INode>);
			foreach (ObjectExpressionProperty property2 in expr.As<ObjectExpression>().Properties)
			{
				if (property2 is Property property)
				{
					property.Value = ReinterpretExpressionAsPattern(property.Value).As<PropertyValue>();
					arrayList2.Add(property);
				}
				else
				{
					arrayList2.Add(ReinterpretExpressionAsPattern(property2));
				}
			}
			NodeList<INode> properties = NodeList.From(ref arrayList2);
			node = new ObjectPattern(in properties);
			node.Range = expr.Range;
			node.Location = (_config.Loc ? expr.Location : default(Location));
			break;
		}
		case Nodes.AssignmentExpression:
		{
			AssignmentExpression assignmentExpression = expr.As<AssignmentExpression>();
			node = new AssignmentPattern(assignmentExpression.Left, assignmentExpression.Right);
			node.Range = expr.Range;
			node.Location = (_config.Loc ? expr.Location : default(Location));
			break;
		}
		}
		return node;
	}

	private INode ParseGroupExpression()
	{
		Expect("(");
		INode node;
		if (Match(")"))
		{
			NextToken();
			if (!Match("=>"))
			{
				Expect("=>");
			}
			node = ArrowParameterPlaceHolder.Empty;
		}
		else
		{
			Token lookahead = _lookahead;
			ArrayList<Token> parameters = default(ArrayList<Token>);
			if (Match("..."))
			{
				RestElement restElement = ParseRestElement(ref parameters);
				Expect(")");
				if (!Match("=>"))
				{
					Expect("=>");
				}
				NodeList<INode> parameters2 = new NodeList<INode>(new INode[1] { restElement }, 1);
				node = new ArrowParameterPlaceHolder(in parameters2, async: false);
			}
			else
			{
				bool flag = false;
				_context.IsBindingElement = true;
				node = InheritCoverGrammar(parseAssignmentExpression);
				if (Match(","))
				{
					ArrayList<Expression> arrayList = default(ArrayList<Expression>);
					_context.IsAssignmentTarget = false;
					arrayList.Add(node.As<Expression>());
					while (_lookahead.Type != TokenType.EOF && Match(","))
					{
						NextToken();
						if (Match(")"))
						{
							NextToken();
							for (int i = 0; i < arrayList.Count; i++)
							{
								ReinterpretExpressionAsPattern(arrayList[i]);
							}
							flag = true;
							ArrayList<INode> arrayList2 = arrayList.Select((Func<Expression, INode>)((Expression e) => e));
							NodeList<INode> parameters2 = NodeList.From(ref arrayList2);
							node = new ArrowParameterPlaceHolder(in parameters2, async: false);
						}
						else if (Match("..."))
						{
							if (!_context.IsBindingElement)
							{
								ThrowUnexpectedToken(_lookahead);
							}
							arrayList.Add(ParseRestElement(ref parameters).As<Expression>());
							Expect(")");
							if (!Match("=>"))
							{
								Expect("=>");
							}
							_context.IsBindingElement = false;
							ArrayList<Expression> arrayList3 = default(ArrayList<Expression>);
							foreach (Expression item in arrayList)
							{
								arrayList3.Add(ReinterpretExpressionAsPattern(item).As<Expression>());
							}
							arrayList = arrayList3;
							flag = true;
							ArrayList<INode> arrayList4 = arrayList.Select((Func<Expression, INode>)((Expression e) => e));
							NodeList<INode> parameters2 = NodeList.From(ref arrayList4);
							node = new ArrowParameterPlaceHolder(in parameters2, async: false);
						}
						else
						{
							arrayList.Add(InheritCoverGrammar(parseAssignmentExpression));
						}
						if (flag)
						{
							break;
						}
					}
					if (!flag)
					{
						Marker marker = StartNode(lookahead);
						NodeList<Expression> expressions = NodeList.From(ref arrayList);
						node = Finalize(marker, new SequenceExpression(in expressions));
					}
				}
				if (!flag)
				{
					Expect(")");
					if (Match("=>"))
					{
						if (node.Type == Nodes.Identifier && ((Identifier)node).Name == "yield")
						{
							flag = true;
							NodeList<INode> parameters2 = new NodeList<INode>(new INode[1] { node }, 1);
							node = new ArrowParameterPlaceHolder(in parameters2, async: false);
						}
						if (!flag)
						{
							if (!_context.IsBindingElement)
							{
								ThrowUnexpectedToken(_lookahead);
							}
							if (node.Type == Nodes.SequenceExpression)
							{
								SequenceExpression sequenceExpression = node.As<SequenceExpression>();
								ArrayList<Expression> arrayList5 = default(ArrayList<Expression>);
								foreach (Expression expression in sequenceExpression.Expressions)
								{
									arrayList5.Add(ReinterpretExpressionAsPattern(expression).As<Expression>());
								}
								NodeList<Expression> expressions = NodeList.From(ref arrayList5);
								sequenceExpression.UpdateExpressions(in expressions);
							}
							else
							{
								node = ReinterpretExpressionAsPattern(node);
							}
							if (node.Type == Nodes.SequenceExpression)
							{
								NodeList<INode> parameters2 = node.As<SequenceExpression>().Expressions.AsNodes();
								node = new ArrowParameterPlaceHolder(in parameters2, async: false);
							}
							else
							{
								NodeList<INode> parameters2 = new NodeList<INode>(new INode[1] { node }, 1);
								node = new ArrowParameterPlaceHolder(in parameters2, async: false);
							}
						}
					}
					_context.IsBindingElement = false;
				}
			}
		}
		return node;
	}

	private NodeList<ArgumentListElement> ParseArguments()
	{
		ArrayList<ArgumentListElement> arrayList = default(ArrayList<ArgumentListElement>);
		Expect("(");
		if (!Match(")"))
		{
			do
			{
				Expression expression;
				if (!Match("..."))
				{
					expression = IsolateCoverGrammar(parseAssignmentExpression);
				}
				else
				{
					Expression expression2 = ParseSpreadElement();
					expression = expression2;
				}
				Expression item = expression;
				arrayList.Add(item);
				if (Match(")"))
				{
					break;
				}
				ExpectCommaSeparator();
			}
			while (!Match(")"));
		}
		Expect(")");
		return NodeList.From(ref arrayList);
	}

	private static bool IsIdentifierName(Token token)
	{
		if (token.Type != TokenType.Identifier && token.Type != TokenType.Keyword && token.Type != 0)
		{
			return token.Type == TokenType.NullLiteral;
		}
		return true;
	}

	private Identifier ParseIdentifierName()
	{
		Marker marker = CreateNode();
		Token token = NextToken();
		if (!IsIdentifierName(token))
		{
			ThrowUnexpectedToken(token);
		}
		return Finalize(marker, new Identifier((string)token.Value));
	}

	private Node ParseNewExpression()
	{
		Marker marker = CreateNode();
		Identifier meta = ParseIdentifierName();
		Node node = null;
		if (Match("."))
		{
			NextToken();
			if (_lookahead.Type == TokenType.Identifier && _context.InFunctionBody && "target".Equals(_lookahead.Value))
			{
				Identifier property = ParseIdentifierName();
				node = new MetaProperty(meta, property);
			}
			else
			{
				ThrowUnexpectedToken(_lookahead);
			}
		}
		else if (MatchKeyword("import"))
		{
			ThrowUnexpectedToken(_lookahead);
		}
		else
		{
			Expression callee = IsolateCoverGrammar(parseLeftHandSideExpression);
			NodeList<ArgumentListElement> args = (Match("(") ? ParseArguments() : default(NodeList<ArgumentListElement>));
			node = new NewExpression(callee, in args);
			_context.IsAssignmentTarget = false;
			_context.IsBindingElement = false;
		}
		return Finalize(marker, node);
	}

	private Expression ParseAsyncArgument()
	{
		Expression result = ParseAssignmentExpression();
		_context.FirstCoverInitializedNameError = null;
		return result;
	}

	private NodeList<ArgumentListElement> ParseAsyncArguments()
	{
		Expect("(");
		ArrayList<ArgumentListElement> arrayList = default(ArrayList<ArgumentListElement>);
		if (!Match(")"))
		{
			do
			{
				Expression expression;
				if (!Match("..."))
				{
					expression = IsolateCoverGrammar(ParseAsyncArgument);
				}
				else
				{
					Expression expression2 = ParseSpreadElement();
					expression = expression2;
				}
				Expression item = expression;
				arrayList.Add(item);
				if (Match(")"))
				{
					break;
				}
				ExpectCommaSeparator();
			}
			while (!Match(")"));
		}
		Expect(")");
		return NodeList.From(ref arrayList);
	}

	private bool MatchImportCall()
	{
		bool flag = MatchKeyword("import");
		if (flag)
		{
			ScannerState state = _scanner.SaveState();
			_scanner.ScanComments();
			Token token = _scanner.Lex();
			_scanner.RestoreState(in state);
			flag = token.Type == TokenType.Punctuator && (string)token.Value == "(";
		}
		return flag;
	}

	private Import ParseImportCall()
	{
		Marker marker = CreateNode();
		ExpectKeyword("import");
		return Finalize(marker, new Import());
	}

	private Expression ParseLeftHandSideExpressionAllowCall()
	{
		Token lookahead = _lookahead;
		bool flag = MatchContextualKeyword("async");
		bool allowIn = _context.AllowIn;
		_context.AllowIn = true;
		Expression expression;
		if (MatchKeyword("super") && _context.InFunctionBody)
		{
			Marker marker = CreateNode();
			NextToken();
			expression = Finalize(marker, new Super()).As<Expression>();
			if (!Match("(") && !Match(".") && !Match("["))
			{
				ThrowUnexpectedToken(_lookahead);
			}
		}
		else
		{
			expression = (MatchKeyword("new") ? InheritCoverGrammar(parseNewExpression).As<Expression>() : InheritCoverGrammar(parsePrimaryExpression));
		}
		while (true)
		{
			if (Match("."))
			{
				_context.IsBindingElement = false;
				_context.IsAssignmentTarget = true;
				Expect(".");
				Identifier property = ParseIdentifierName();
				expression = Finalize(StartNode(lookahead), new StaticMemberExpression(expression, property));
			}
			else if (Match("("))
			{
				bool num = flag && lookahead.LineNumber == _lookahead.LineNumber;
				_context.IsBindingElement = false;
				_context.IsAssignmentTarget = false;
				NodeList<ArgumentListElement> args = (num ? ParseAsyncArguments() : ParseArguments());
				if (expression.Type == Nodes.Import && args.Count != 1)
				{
					TolerateError("Unexpected token");
				}
				expression = Finalize(StartNode(lookahead), new CallExpression(expression, in args));
				if (num && Match("=>"))
				{
					ArrayList<INode> arrayList = default(ArrayList<INode>);
					for (int i = 0; i < args.Count; i++)
					{
						arrayList.Add(ReinterpretExpressionAsPattern(args[i]));
					}
					NodeList<INode> parameters = NodeList.From(ref arrayList);
					expression = new ArrowParameterPlaceHolder(in parameters, async: true);
				}
			}
			else if (Match("["))
			{
				_context.IsBindingElement = false;
				_context.IsAssignmentTarget = true;
				Expect("[");
				Expression property2 = IsolateCoverGrammar(parseExpression);
				Expect("]");
				expression = Finalize(StartNode(lookahead), new ComputedMemberExpression(expression, property2));
			}
			else
			{
				if (_lookahead.Type != TokenType.Template || !_lookahead.Head)
				{
					break;
				}
				TemplateLiteral quasi = ParseTemplateLiteral();
				expression = Finalize(StartNode(lookahead), new TaggedTemplateExpression(expression, quasi));
			}
		}
		_context.AllowIn = allowIn;
		return expression;
	}

	private Super ParseSuper()
	{
		Marker marker = CreateNode();
		ExpectKeyword("super");
		if (!Match("[") && !Match("."))
		{
			ThrowUnexpectedToken(_lookahead);
		}
		return Finalize(marker, new Super());
	}

	private Expression ParseLeftHandSideExpression()
	{
		Marker marker = StartNode(_lookahead);
		object obj;
		if (!MatchKeyword("super") || !_context.InFunctionBody)
		{
			obj = (MatchKeyword("new") ? ((Expression)InheritCoverGrammar(parseNewExpression)) : InheritCoverGrammar(parsePrimaryExpression));
		}
		else
		{
			Expression expression = ParseSuper();
			obj = expression;
		}
		Expression expression2 = (Expression)obj;
		while (true)
		{
			if (Match("["))
			{
				_context.IsBindingElement = false;
				_context.IsAssignmentTarget = true;
				Expect("[");
				Expression property = IsolateCoverGrammar(parseExpression);
				Expect("]");
				expression2 = Finalize(marker, new ComputedMemberExpression(expression2, property));
			}
			else if (Match("."))
			{
				_context.IsBindingElement = false;
				_context.IsAssignmentTarget = true;
				Expect(".");
				Identifier property2 = ParseIdentifierName();
				expression2 = Finalize(marker, new StaticMemberExpression(expression2, property2));
			}
			else
			{
				if (_lookahead.Type != TokenType.Template || !_lookahead.Head)
				{
					break;
				}
				TemplateLiteral quasi = ParseTemplateLiteral();
				expression2 = Finalize(marker, new TaggedTemplateExpression(expression2, quasi));
			}
		}
		return expression2;
	}

	private Expression ParseUpdateExpression()
	{
		Token lookahead = _lookahead;
		Expression expression;
		if (Match("++") || Match("--"))
		{
			Marker marker = StartNode(lookahead);
			Token token = NextToken();
			expression = InheritCoverGrammar(parseUnaryExpression);
			if (_context.Strict && expression.Type == Nodes.Identifier && Scanner.IsRestrictedWord(expression.As<Identifier>().Name))
			{
				TolerateError("Prefix increment/decrement may not have eval or arguments operand in strict mode");
			}
			if (!_context.IsAssignmentTarget)
			{
				TolerateError("Invalid left-hand side in assignment");
			}
			bool prefix = true;
			expression = Finalize(marker, new UpdateExpression((string)token.Value, expression, prefix));
			_context.IsAssignmentTarget = false;
			_context.IsBindingElement = false;
		}
		else
		{
			expression = InheritCoverGrammar(parseLeftHandSideExpressionAllowCall);
			if (!_hasLineTerminator && _lookahead.Type == TokenType.Punctuator && (Match("++") || Match("--")))
			{
				if (_context.Strict && expression.Type == Nodes.Identifier && Scanner.IsRestrictedWord(expression.As<Identifier>().Name))
				{
					TolerateError("Postfix increment/decrement may not have eval or arguments operand in strict mode");
				}
				if (!_context.IsAssignmentTarget)
				{
					TolerateError("Invalid left-hand side in assignment");
				}
				_context.IsAssignmentTarget = false;
				_context.IsBindingElement = false;
				object value = NextToken().Value;
				bool prefix2 = false;
				expression = Finalize(StartNode(lookahead), new UpdateExpression((string)value, expression, prefix2));
			}
		}
		return expression;
	}

	private Expression ParseAwaitExpression()
	{
		Marker marker = CreateNode();
		NextToken();
		Expression argument = ParseUnaryExpression();
		return Finalize(marker, new AwaitExpression(argument));
	}

	private Expression ParseUnaryExpression()
	{
		Expression expression;
		if (!Match("+") && !Match("-") && !Match("~") && !Match("!") && !MatchKeyword("delete") && !MatchKeyword("void") && !MatchKeyword("typeof"))
		{
			expression = ((!_context.Await || !MatchContextualKeyword("await")) ? ParseUpdateExpression() : ParseAwaitExpression());
		}
		else
		{
			Marker marker = StartNode(_lookahead);
			Token token = NextToken();
			expression = InheritCoverGrammar(parseUnaryExpression);
			expression = Finalize(marker, new UnaryExpression((string)token.Value, expression));
			UnaryExpression unaryExpression = expression.As<UnaryExpression>();
			if (_context.Strict && unaryExpression.Operator == UnaryOperator.Delete && unaryExpression.Argument.Type == Nodes.Identifier)
			{
				TolerateError("Delete of an unqualified identifier in strict mode.");
			}
			_context.IsAssignmentTarget = false;
			_context.IsBindingElement = false;
		}
		return expression;
	}

	private Expression ParseExponentiationExpression()
	{
		Token lookahead = _lookahead;
		Expression expression = InheritCoverGrammar(parseUnaryExpression);
		if (expression.Type != Nodes.UnaryExpression && Match("**"))
		{
			NextToken();
			_context.IsAssignmentTarget = false;
			_context.IsBindingElement = false;
			Expression left = expression;
			Expression right = IsolateCoverGrammar(parseExponentiationExpression);
			expression = Finalize(StartNode(lookahead), new BinaryExpression("**", left, right));
		}
		return expression;
	}

	private int BinaryPrecedence(Token token)
	{
		int result = 0;
		object value = token.Value;
		if (token.Type == TokenType.Punctuator)
		{
			switch ((string)value)
			{
			case ")":
			case ";":
			case ",":
			case "=":
			case "]":
				result = 0;
				break;
			case "||":
				result = 1;
				break;
			case "&&":
				result = 2;
				break;
			case "|":
				result = 3;
				break;
			case "^":
				result = 4;
				break;
			case "&":
				result = 5;
				break;
			case "==":
			case "!=":
			case "===":
			case "!==":
				result = 6;
				break;
			case "<":
			case ">":
			case "<=":
			case ">=":
				result = 7;
				break;
			case "<<":
			case ">>":
			case ">>>":
				result = 8;
				break;
			case "+":
			case "-":
				result = 9;
				break;
			case "*":
			case "/":
			case "%":
				result = 11;
				break;
			default:
				result = 0;
				break;
			}
		}
		else if (token.Type == TokenType.Keyword)
		{
			result = (("instanceof".Equals(value) || (_context.AllowIn && "in".Equals(value))) ? 7 : 0);
		}
		return result;
	}

	private Expression ParseBinaryExpression()
	{
		Token lookahead = _lookahead;
		Expression expression = InheritCoverGrammar(parseExponentiationExpression);
		Token lookahead2 = _lookahead;
		int num = BinaryPrecedence(lookahead2);
		if (num > 0)
		{
			NextToken();
			_context.IsAssignmentTarget = false;
			_context.IsBindingElement = false;
			Stack<Token> stack = new Stack<Token>(new Token[2] { lookahead, _lookahead });
			Expression item = expression;
			Expression item2 = IsolateCoverGrammar(parseExponentiationExpression);
			ArrayList<object> arrayList = default(ArrayList<object>);
			arrayList.Add(item);
			arrayList.Add(lookahead2.Value);
			arrayList.Add(item2);
			ArrayList<object> arrayList2 = arrayList;
			ArrayList<int> arrayList3 = default(ArrayList<int>);
			arrayList3.Add(num);
			ArrayList<int> arrayList4 = arrayList3;
			while (true)
			{
				num = BinaryPrecedence(_lookahead);
				if (num <= 0)
				{
					break;
				}
				while (arrayList2.Count > 2 && num <= arrayList4[arrayList4.Count - 1])
				{
					item2 = (Expression)arrayList2.Pop();
					string op = (string)arrayList2.Pop();
					arrayList4.Pop();
					item = (Expression)arrayList2.Pop();
					stack.Pop();
					Marker marker = StartNode(stack.Peek());
					arrayList2.Push(Finalize(marker, new BinaryExpression(op, item, item2)));
				}
				arrayList2.Push(NextToken().Value);
				arrayList4.Push(num);
				stack.Push(_lookahead);
				arrayList2.Push(IsolateCoverGrammar(parseExponentiationExpression));
			}
			int num2 = arrayList2.Count - 1;
			expression = (Expression)arrayList2[num2];
			Token token = stack.Pop();
			while (num2 > 1)
			{
				Token token2 = stack.Pop();
				int lastLineStart = token?.LineStart ?? 0;
				Marker marker2 = StartNode(token2, lastLineStart);
				string op2 = (string)arrayList2[num2 - 1];
				expression = Finalize(marker2, new BinaryExpression(op2, (Expression)arrayList2[num2 - 2], expression));
				num2 -= 2;
				token = token2;
			}
		}
		return expression;
	}

	private Expression ParseConditionalExpression()
	{
		Token lookahead = _lookahead;
		Expression expression = InheritCoverGrammar(parseBinaryExpression);
		if (Match("?"))
		{
			NextToken();
			bool allowIn = _context.AllowIn;
			_context.AllowIn = true;
			Expression consequent = IsolateCoverGrammar(parseAssignmentExpression);
			_context.AllowIn = allowIn;
			Expect(":");
			Expression alternate = IsolateCoverGrammar(parseAssignmentExpression);
			expression = Finalize(StartNode(lookahead), new ConditionalExpression(expression, consequent, alternate));
			_context.IsAssignmentTarget = false;
			_context.IsBindingElement = false;
		}
		return expression;
	}

	private void CheckPatternParam(ParsedParameters options, INode param)
	{
		switch (param.Type)
		{
		case Nodes.Identifier:
			ValidateParam(options, param, param.As<Identifier>().Name);
			break;
		case Nodes.RestElement:
			CheckPatternParam(options, param.As<RestElement>().Argument);
			break;
		case Nodes.AssignmentPattern:
			CheckPatternParam(options, param.As<AssignmentPattern>().Left);
			break;
		case Nodes.ArrayPattern:
			foreach (IArrayPatternElement element in param.As<ArrayPattern>().Elements)
			{
				if (element != null)
				{
					CheckPatternParam(options, element);
				}
			}
			break;
		case Nodes.ObjectPattern:
			foreach (INode property2 in param.As<ObjectPattern>().Properties)
			{
				INode param2;
				if (!(property2 is Property property))
				{
					param2 = property2;
				}
				else
				{
					INode value = property.Value;
					param2 = value;
				}
				CheckPatternParam(options, param2);
			}
			break;
		}
		options.Simple = options.Simple && param is Identifier;
	}

	private ParsedParameters ReinterpretAsCoverFormalsList(INode expr)
	{
		bool flag = false;
		ArrayList<INode> destination;
		switch (expr.Type)
		{
		case Nodes.Identifier:
		{
			ArrayList<INode> arrayList = new ArrayList<INode>(1);
			arrayList.Add(expr);
			destination = arrayList;
			break;
		}
		case Nodes.ArrowParameterPlaceHolder:
		{
			ArrowParameterPlaceHolder arrowParameterPlaceHolder = expr.As<ArrowParameterPlaceHolder>();
			destination = new ArrayList<INode>(arrowParameterPlaceHolder.Params.Count);
			destination.AddRange(in arrowParameterPlaceHolder.Params);
			flag = arrowParameterPlaceHolder.Async;
			break;
		}
		default:
			return null;
		}
		ParsedParameters parsedParameters = new ParsedParameters
		{
			Simple = true
		};
		for (int i = 0; i < destination.Count; i++)
		{
			INode node = destination[i];
			if (node.Type == Nodes.AssignmentPattern)
			{
				AssignmentPattern assignmentPattern = node.As<AssignmentPattern>();
				if (assignmentPattern.Right.Type == Nodes.YieldExpression)
				{
					if (assignmentPattern.Right.As<YieldExpression>().Argument != null)
					{
						ThrowUnexpectedToken(_lookahead);
					}
					assignmentPattern.Right = new Identifier("yield")
					{
						Location = (_config.Loc ? assignmentPattern.Right.Location : default(Location)),
						Range = assignmentPattern.Right.Range
					};
				}
			}
			else if (flag && node.Type == Nodes.Identifier && node.As<Identifier>().Name == "await")
			{
				ThrowUnexpectedToken(_lookahead);
			}
			CheckPatternParam(parsedParameters, node);
			destination[i] = node;
		}
		if (_context.Strict || !_context.AllowYield)
		{
			for (int j = 0; j < destination.Count; j++)
			{
				if (destination[j].Type == Nodes.YieldExpression)
				{
					ThrowUnexpectedToken(_lookahead);
				}
			}
		}
		if (parsedParameters.Message == "Strict mode function may not have duplicate parameter names")
		{
			Token token = (_context.Strict ? parsedParameters.Stricted : parsedParameters.FirstRestricted);
			ThrowUnexpectedToken(token, parsedParameters.Message);
		}
		return new ParsedParameters
		{
			Simple = parsedParameters.Simple,
			Parameters = destination,
			Stricted = parsedParameters.Stricted,
			FirstRestricted = parsedParameters.FirstRestricted,
			Message = parsedParameters.Message
		};
	}

	private Expression ParseAssignmentExpression()
	{
		if (_assignmentDepth++ > 100)
		{
			ThrowUnexpectedToken(_lookahead, "Maximum statements depth reached");
		}
		INode node;
		if (!_context.AllowYield && MatchKeyword("yield"))
		{
			node = ParseYieldExpression();
		}
		else
		{
			Token lookahead = _lookahead;
			Token token = lookahead;
			node = ParseConditionalExpression();
			if (token.Type == TokenType.Identifier && token.LineNumber == _lookahead.LineNumber && (string)token.Value == "async" && (_lookahead.Type == TokenType.Identifier || MatchKeyword("yield")))
			{
				Expression expression = ParsePrimaryExpression();
				ReinterpretExpressionAsPattern(expression);
				NodeList<INode> parameters = new NodeList<INode>(new INode[1] { expression }, 1);
				node = new ArrowParameterPlaceHolder(in parameters, async: true);
			}
			if (node.Type == Nodes.ArrowParameterPlaceHolder || Match("=>"))
			{
				EnterHoistingScope();
				_context.IsAssignmentTarget = false;
				_context.IsBindingElement = false;
				bool flag = node is ArrowParameterPlaceHolder arrowParameterPlaceHolder && arrowParameterPlaceHolder.Async;
				ParsedParameters parsedParameters = ReinterpretAsCoverFormalsList(node);
				if (parsedParameters != null)
				{
					if (_hasLineTerminator)
					{
						TolerateUnexpectedToken(_lookahead);
					}
					_context.FirstCoverInitializedNameError = null;
					bool strict = _context.Strict;
					bool allowStrictDirective = _context.AllowStrictDirective;
					_context.AllowStrictDirective = parsedParameters.Simple;
					bool allowYield = _context.AllowYield;
					bool await = _context.Await;
					_context.AllowYield = true;
					_context.Await = flag;
					Marker marker = StartNode(lookahead);
					Expect("=>");
					INode node2;
					if (Match("{"))
					{
						bool allowIn = _context.AllowIn;
						_context.AllowIn = true;
						node2 = ParseFunctionSourceElements();
						_context.AllowIn = allowIn;
					}
					else
					{
						node2 = IsolateCoverGrammar(parseAssignmentExpression);
					}
					bool expression2 = node2.Type != Nodes.BlockStatement;
					if (_context.Strict && parsedParameters.FirstRestricted != null)
					{
						ThrowUnexpectedToken(parsedParameters.FirstRestricted, parsedParameters.Message);
					}
					if (_context.Strict && parsedParameters.Stricted != null)
					{
						TolerateUnexpectedToken(parsedParameters.Stricted, parsedParameters.Message);
					}
					NodeList<INode> parameters = NodeList.From(ref parsedParameters.Parameters);
					node = Finalize(marker, new ArrowFunctionExpression(in parameters, node2, expression2, flag, LeaveHoistingScope()));
					_context.Strict = strict;
					_context.AllowStrictDirective = allowStrictDirective;
					_context.AllowYield = allowYield;
					_context.Await = await;
				}
			}
			else if (MatchAssign())
			{
				if (!_context.IsAssignmentTarget)
				{
					TolerateError("Invalid left-hand side in assignment");
				}
				if (_context.Strict && node.Type == Nodes.Identifier)
				{
					Identifier identifier = node.As<Identifier>();
					if (Scanner.IsRestrictedWord(identifier.Name))
					{
						TolerateUnexpectedToken(token, "Assignment to eval or arguments is not allowed in strict mode");
					}
					if (Scanner.IsStrictModeReservedWord(identifier.Name))
					{
						TolerateUnexpectedToken(token, "Use of future reserved word in strict mode");
					}
				}
				if (!Match("="))
				{
					_context.IsAssignmentTarget = false;
					_context.IsBindingElement = false;
				}
				else
				{
					node = ReinterpretExpressionAsPattern(node);
				}
				token = NextToken();
				Expression right = IsolateCoverGrammar(parseAssignmentExpression);
				node = Finalize(StartNode(lookahead), new AssignmentExpression((string)token.Value, node, right));
				_context.FirstCoverInitializedNameError = null;
			}
		}
		_assignmentDepth--;
		return node.As<Expression>();
	}

	public Expression ParseExpression()
	{
		Token lookahead = _lookahead;
		Expression expression = IsolateCoverGrammar(parseAssignmentExpression);
		if (Match(","))
		{
			ArrayList<Expression> arrayList = default(ArrayList<Expression>);
			arrayList.Push(expression);
			while (_lookahead.Type != TokenType.EOF && Match(","))
			{
				NextToken();
				arrayList.Push(IsolateCoverGrammar(parseAssignmentExpression));
			}
			Marker marker = StartNode(lookahead);
			NodeList<Expression> expressions = NodeList.From(ref arrayList);
			expression = Finalize(marker, new SequenceExpression(in expressions));
		}
		return expression;
	}

	private IStatementListItem ParseStatementListItem()
	{
		IStatementListItem statementListItem = null;
		_context.IsAssignmentTarget = true;
		_context.IsBindingElement = true;
		if (_lookahead.Type == TokenType.Keyword)
		{
			switch ((string)_lookahead.Value)
			{
			case "export":
				if (!_context.IsModule)
				{
					TolerateUnexpectedToken(_lookahead, "Unexpected token");
				}
				return ParseExportDeclaration();
			case "import":
				if (MatchImportCall())
				{
					return ParseExpressionStatement();
				}
				if (!_context.IsModule)
				{
					TolerateUnexpectedToken(_lookahead, "Unexpected token");
				}
				return ParseImportDeclaration();
			case "const":
			{
				bool inFor = false;
				return ParseLexicalDeclaration(ref inFor);
			}
			case "function":
				return ParseFunctionDeclaration();
			case "class":
				return ParseClassDeclaration();
			case "let":
			{
				bool inFor = false;
				return IsLexicalDeclaration() ? ParseLexicalDeclaration(ref inFor) : ParseStatement();
			}
			default:
				return ParseStatement();
			}
		}
		return ParseStatement();
	}

	private BlockStatement ParseBlock()
	{
		Marker marker = CreateNode();
		Expect("{");
		ArrayList<IStatementListItem> arrayList = default(ArrayList<IStatementListItem>);
		while (!Match("}"))
		{
			arrayList.Add(ParseStatementListItem());
		}
		Expect("}");
		NodeList<IStatementListItem> body = NodeList.From(ref arrayList);
		return Finalize(marker, new BlockStatement(in body));
	}

	private VariableDeclarator ParseLexicalBinding(VariableDeclarationKind kind, ref bool inFor)
	{
		Marker marker = CreateNode();
		ArrayList<Token> parameters = default(ArrayList<Token>);
		IArrayPatternElement arrayPatternElement = ParsePattern(ref parameters, kind);
		if (_context.Strict && arrayPatternElement.Type == Nodes.Identifier && Scanner.IsRestrictedWord(arrayPatternElement.As<Identifier>().Name))
		{
			TolerateError("Variable name may not be eval or arguments in strict mode");
		}
		Expression init = null;
		if (kind == VariableDeclarationKind.Const)
		{
			if (!MatchKeyword("in") && !MatchContextualKeyword("of"))
			{
				if (Match("="))
				{
					NextToken();
					init = IsolateCoverGrammar(parseAssignmentExpression);
				}
				else
				{
					ThrowError("Missing initializer in {0} declaration", "const");
				}
			}
		}
		else if ((!inFor && arrayPatternElement.Type != Nodes.Identifier) || Match("="))
		{
			Expect("=");
			init = IsolateCoverGrammar(parseAssignmentExpression);
		}
		return Finalize(marker, new VariableDeclarator(arrayPatternElement, init));
	}

	private NodeList<VariableDeclarator> ParseBindingList(VariableDeclarationKind kind, ref bool inFor)
	{
		ArrayList<VariableDeclarator> arrayList = default(ArrayList<VariableDeclarator>);
		arrayList.Add(ParseLexicalBinding(kind, ref inFor));
		ArrayList<VariableDeclarator> arrayList2 = arrayList;
		while (Match(","))
		{
			NextToken();
			arrayList2.Add(ParseLexicalBinding(kind, ref inFor));
		}
		return NodeList.From(ref arrayList2);
	}

	private bool IsLexicalDeclaration()
	{
		ScannerState state = _scanner.SaveState();
		_scanner.ScanComments();
		Token token = _scanner.Lex();
		_scanner.RestoreState(in state);
		if (token.Type != TokenType.Identifier && (token.Type != TokenType.Punctuator || !((string)token.Value == "[")) && (token.Type != TokenType.Punctuator || !((string)token.Value == "{")) && (token.Type != TokenType.Keyword || !((string)token.Value == "let")))
		{
			if (token.Type == TokenType.Keyword)
			{
				return (string)token.Value == "yield";
			}
			return false;
		}
		return true;
	}

	private VariableDeclaration ParseLexicalDeclaration(ref bool inFor)
	{
		Marker marker = CreateNode();
		string kindString = (string)NextToken().Value;
		VariableDeclarationKind kind = ParseVariableDeclarationKind(kindString);
		NodeList<VariableDeclarator> declarations = ParseBindingList(kind, ref inFor);
		ConsumeSemicolon();
		return Finalize(marker, new VariableDeclaration(in declarations, kind));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private VariableDeclarationKind ParseVariableDeclarationKind(string kindString)
	{
		return kindString switch
		{
			"const" => VariableDeclarationKind.Const, 
			"let" => VariableDeclarationKind.Let, 
			"var" => VariableDeclarationKind.Var, 
			_ => throw CreateError("Unknown declaration kind '{0}'", kindString), 
		};
	}

	private RestElement ParseBindingRestElement(ref ArrayList<Token> parameters, VariableDeclarationKind? kind)
	{
		Marker marker = CreateNode();
		Expect("...");
		IArrayPatternElement argument = ParsePattern(ref parameters, kind);
		return Finalize(marker, new RestElement(argument));
	}

	private ArrayPattern ParseArrayPattern(ref ArrayList<Token> parameters, VariableDeclarationKind? kind)
	{
		Marker marker = CreateNode();
		Expect("[");
		ArrayList<IArrayPatternElement> arrayList = default(ArrayList<IArrayPatternElement>);
		while (!Match("]"))
		{
			if (Match(","))
			{
				NextToken();
				arrayList.Push(null);
				continue;
			}
			if (Match("..."))
			{
				arrayList.Push(ParseBindingRestElement(ref parameters, kind));
				break;
			}
			arrayList.Push(ParsePatternWithDefault(ref parameters, kind));
			if (!Match("]"))
			{
				Expect(",");
			}
		}
		Expect("]");
		NodeList<IArrayPatternElement> elements = NodeList.From(ref arrayList);
		return Finalize(marker, new ArrayPattern(in elements));
	}

	private Property ParsePropertyPattern(ref ArrayList<Token> parameters, VariableDeclarationKind? kind)
	{
		Marker marker = CreateNode();
		bool computed = false;
		bool shorthand = false;
		bool method = false;
		Expression key;
		PropertyValue value;
		if (_lookahead.Type == TokenType.Identifier)
		{
			Token lookahead = _lookahead;
			key = ParseVariableIdentifier();
			Identifier identifier = Finalize(marker, new Identifier((string)lookahead.Value));
			if (Match("="))
			{
				parameters.Push(lookahead);
				shorthand = true;
				NextToken();
				Expression right = ParseAssignmentExpression();
				value = Finalize(StartNode(lookahead), new AssignmentPattern(identifier, right));
			}
			else if (!Match(":"))
			{
				parameters.Push(lookahead);
				shorthand = true;
				value = identifier;
			}
			else
			{
				Expect(":");
				value = (PropertyValue)ParsePatternWithDefault(ref parameters, kind);
			}
		}
		else
		{
			computed = Match("[");
			key = ParseObjectPropertyKey();
			Expect(":");
			value = (PropertyValue)ParsePatternWithDefault(ref parameters, kind);
		}
		return Finalize(marker, new Property(PropertyKind.Init, key, computed, value, method, shorthand));
	}

	private RestElement ParseRestProperty(ref ArrayList<Token> parameters, VariableDeclarationKind? kind)
	{
		Marker marker = CreateNode();
		Expect("...");
		IArrayPatternElement argument = ParsePattern(ref parameters);
		if (Match("="))
		{
			ThrowError("Unexpected token =");
		}
		if (!Match("}"))
		{
			ThrowError("Unexpected token");
		}
		return Finalize(marker, new RestElement(argument));
	}

	private ObjectPattern ParseObjectPattern(ref ArrayList<Token> parameters, VariableDeclarationKind? kind)
	{
		Marker marker = CreateNode();
		ArrayList<INode> arrayList = default(ArrayList<INode>);
		Expect("{");
		while (!Match("}"))
		{
			INode item;
			if (!Match("..."))
			{
				INode node = ParsePropertyPattern(ref parameters, kind);
				item = node;
			}
			else
			{
				INode node = ParseRestProperty(ref parameters, kind);
				item = node;
			}
			arrayList.Push(item);
			if (!Match("}"))
			{
				Expect(",");
			}
		}
		Expect("}");
		NodeList<INode> properties = NodeList.From(ref arrayList);
		return Finalize(marker, new ObjectPattern(in properties));
	}

	private IArrayPatternElement ParsePattern(ref ArrayList<Token> parameters, VariableDeclarationKind? kind = null)
	{
		if (Match("["))
		{
			return ParseArrayPattern(ref parameters, kind);
		}
		if (Match("{"))
		{
			return ParseObjectPattern(ref parameters, kind);
		}
		if (MatchKeyword("let") && (kind == VariableDeclarationKind.Const || kind == VariableDeclarationKind.Let))
		{
			TolerateUnexpectedToken(_lookahead, "let is disallowed as a lexically bound name");
		}
		parameters.Push(_lookahead);
		return ParseVariableIdentifier(kind);
	}

	private IArrayPatternElement ParsePatternWithDefault(ref ArrayList<Token> parameters, VariableDeclarationKind? kind = null)
	{
		Token lookahead = _lookahead;
		IArrayPatternElement arrayPatternElement = ParsePattern(ref parameters, kind);
		if (Match("="))
		{
			NextToken();
			bool allowYield = _context.AllowYield;
			_context.AllowYield = true;
			Expression right = IsolateCoverGrammar(parseAssignmentExpression);
			_context.AllowYield = allowYield;
			arrayPatternElement = Finalize(StartNode(lookahead), new AssignmentPattern(arrayPatternElement, right));
		}
		return arrayPatternElement;
	}

	private Identifier ParseVariableIdentifier(VariableDeclarationKind? kind = null)
	{
		Marker marker = CreateNode();
		Token token = NextToken();
		if (token.Type == TokenType.Keyword && (string)token.Value == "yield")
		{
			if (_context.Strict)
			{
				TolerateUnexpectedToken(token, "Use of future reserved word in strict mode");
			}
			if (!_context.AllowYield)
			{
				ThrowUnexpectedToken(token);
			}
		}
		else if (token.Type != TokenType.Identifier)
		{
			if (_context.Strict && token.Type == TokenType.Keyword && Scanner.IsStrictModeReservedWord((string)token.Value))
			{
				TolerateUnexpectedToken(token, "Use of future reserved word in strict mode");
			}
			else
			{
				string text = token.Value as string;
				if (_context.Strict || text == null || text != "let" || kind != VariableDeclarationKind.Var)
				{
					ThrowUnexpectedToken(token);
				}
			}
		}
		else if ((_context.IsModule || _context.Await) && token.Type == TokenType.Identifier && (string)token.Value == "await")
		{
			TolerateUnexpectedToken(token);
		}
		return Finalize(marker, new Identifier((string)token.Value));
	}

	private VariableDeclarator ParseVariableDeclaration(ref bool inFor)
	{
		Marker marker = CreateNode();
		ArrayList<Token> parameters = default(ArrayList<Token>);
		IArrayPatternElement arrayPatternElement = ParsePattern(ref parameters, VariableDeclarationKind.Var);
		if (_context.Strict && arrayPatternElement.Type == Nodes.Identifier && Scanner.IsRestrictedWord(arrayPatternElement.As<Identifier>().Name))
		{
			TolerateError("Variable name may not be eval or arguments in strict mode");
		}
		Expression init = null;
		if (Match("="))
		{
			NextToken();
			init = IsolateCoverGrammar(parseAssignmentExpression);
		}
		else if (arrayPatternElement.Type != Nodes.Identifier && !inFor)
		{
			Expect("=");
		}
		return Finalize(marker, new VariableDeclarator(arrayPatternElement, init));
	}

	private NodeList<VariableDeclarator> ParseVariableDeclarationList(ref bool inFor)
	{
		bool inFor2 = inFor;
		ArrayList<VariableDeclarator> arrayList = default(ArrayList<VariableDeclarator>);
		arrayList.Push(ParseVariableDeclaration(ref inFor2));
		while (Match(","))
		{
			NextToken();
			arrayList.Push(ParseVariableDeclaration(ref inFor2));
		}
		return NodeList.From(ref arrayList);
	}

	private VariableDeclaration ParseVariableStatement()
	{
		Marker marker = CreateNode();
		ExpectKeyword("var");
		bool inFor = false;
		NodeList<VariableDeclarator> declarations = ParseVariableDeclarationList(ref inFor);
		ConsumeSemicolon();
		return Hoist(Finalize(marker, new VariableDeclaration(in declarations, VariableDeclarationKind.Var)));
	}

	private EmptyStatement ParseEmptyStatement()
	{
		Marker marker = CreateNode();
		Expect(";");
		return Finalize(marker, new EmptyStatement());
	}

	private ExpressionStatement ParseExpressionStatement()
	{
		Marker marker = CreateNode();
		Expression expression = ParseExpression();
		ConsumeSemicolon();
		return Finalize(marker, new ExpressionStatement(expression));
	}

	private Statement ParseIfClause()
	{
		if (_context.Strict && MatchKeyword("function"))
		{
			TolerateError("In strict mode code, functions can only be declared at top level or inside a block");
		}
		return ParseStatement();
	}

	private IfStatement ParseIfStatement()
	{
		Marker marker = CreateNode();
		Statement alternate = null;
		ExpectKeyword("if");
		Expect("(");
		Expression test = ParseExpression();
		Statement consequent;
		if (!Match(")") && _config.Tolerant)
		{
			TolerateUnexpectedToken(NextToken());
			consequent = Finalize(CreateNode(), new EmptyStatement());
		}
		else
		{
			Expect(")");
			consequent = ParseIfClause();
			if (MatchKeyword("else"))
			{
				NextToken();
				alternate = ParseIfClause();
			}
		}
		return Finalize(marker, new IfStatement(test, consequent, alternate));
	}

	private DoWhileStatement ParseDoWhileStatement()
	{
		Marker marker = CreateNode();
		ExpectKeyword("do");
		bool inIteration = _context.InIteration;
		_context.InIteration = true;
		Statement body = ParseStatement();
		_context.InIteration = inIteration;
		ExpectKeyword("while");
		Expect("(");
		Expression test = ParseExpression();
		if (!Match(")") && _config.Tolerant)
		{
			TolerateUnexpectedToken(NextToken());
		}
		else
		{
			Expect(")");
			if (Match(";"))
			{
				NextToken();
			}
		}
		return Finalize(marker, new DoWhileStatement(body, test));
	}

	private WhileStatement ParseWhileStatement()
	{
		Marker marker = CreateNode();
		ExpectKeyword("while");
		Expect("(");
		Expression test = ParseExpression();
		Statement body;
		if (!Match(")") && _config.Tolerant)
		{
			TolerateUnexpectedToken(NextToken());
			body = Finalize(CreateNode(), new EmptyStatement());
		}
		else
		{
			Expect(")");
			bool inIteration = _context.InIteration;
			_context.InIteration = true;
			body = ParseStatement();
			_context.InIteration = inIteration;
		}
		return Finalize(marker, new WhileStatement(test, body));
	}

	private Statement ParseForStatement()
	{
		INode node = null;
		Expression test = null;
		Expression update = null;
		bool flag = true;
		INode node2 = null;
		Expression right = null;
		Marker marker = CreateNode();
		ExpectKeyword("for");
		Expect("(");
		if (Match(";"))
		{
			NextToken();
		}
		else if (MatchKeyword("var"))
		{
			Marker marker2 = CreateNode();
			NextToken();
			bool allowIn = _context.AllowIn;
			_context.AllowIn = false;
			bool inFor = true;
			NodeList<VariableDeclarator> declarations = ParseVariableDeclarationList(ref inFor);
			_context.AllowIn = allowIn;
			if (declarations.Count == 1 && MatchKeyword("in"))
			{
				VariableDeclarator variableDeclarator = declarations[0];
				if (variableDeclarator.Init != null && (variableDeclarator.Id.Type == Nodes.ArrayPattern || variableDeclarator.Id.Type == Nodes.ObjectPattern || _context.Strict))
				{
					TolerateError("'{0} loop variable declaration may not have an initializer", "for-in");
				}
				node2 = Hoist(Finalize(marker2, new VariableDeclaration(in declarations, VariableDeclarationKind.Var)));
				NextToken();
				right = ParseExpression();
				node = null;
			}
			else if (declarations.Count == 1 && declarations[0].Init == null && MatchContextualKeyword("of"))
			{
				node2 = Hoist(Finalize(marker2, new VariableDeclaration(in declarations, VariableDeclarationKind.Var)));
				NextToken();
				right = ParseAssignmentExpression();
				node = null;
				flag = false;
			}
			else
			{
				node = Hoist(Finalize(marker2, new VariableDeclaration(in declarations, VariableDeclarationKind.Var)));
				Expect(";");
			}
		}
		else if (MatchKeyword("const") || MatchKeyword("let"))
		{
			Marker marker3 = CreateNode();
			string text = (string)NextToken().Value;
			VariableDeclarationKind kind = ParseVariableDeclarationKind(text);
			if (!_context.Strict && (string)_lookahead.Value == "in")
			{
				node2 = Finalize(marker3, new Identifier(text));
				NextToken();
				right = ParseExpression();
				node = null;
			}
			else
			{
				bool allowIn2 = _context.AllowIn;
				_context.AllowIn = false;
				bool inFor2 = true;
				NodeList<VariableDeclarator> declarations2 = ParseBindingList(kind, ref inFor2);
				_context.AllowIn = allowIn2;
				if (declarations2.Count == 1 && declarations2[0].Init == null && MatchKeyword("in"))
				{
					node2 = Hoist(Finalize(marker3, new VariableDeclaration(in declarations2, kind)));
					NextToken();
					right = ParseExpression();
					node = null;
				}
				else if (declarations2.Count == 1 && declarations2[0].Init == null && MatchContextualKeyword("of"))
				{
					node2 = Hoist(Finalize(marker3, new VariableDeclaration(in declarations2, kind)));
					NextToken();
					right = ParseAssignmentExpression();
					node = null;
					flag = false;
				}
				else
				{
					ConsumeSemicolon();
					node = Finalize(marker3, new VariableDeclaration(in declarations2, kind));
				}
			}
		}
		else
		{
			Token lookahead = _lookahead;
			bool isBindingElement = _context.IsBindingElement;
			bool isAssignmentTarget = _context.IsAssignmentTarget;
			Token firstCoverInitializedNameError = _context.FirstCoverInitializedNameError;
			bool allowIn3 = _context.AllowIn;
			_context.AllowIn = false;
			node = InheritCoverGrammar(parseAssignmentExpression);
			_context.AllowIn = allowIn3;
			if (MatchKeyword("in"))
			{
				if (!_context.IsAssignmentTarget || node.Type == Nodes.AssignmentExpression)
				{
					TolerateError("Invalid left-hand side in for-in");
				}
				NextToken();
				node = ReinterpretExpressionAsPattern(node);
				node2 = node;
				right = ParseExpression();
				node = null;
			}
			else if (MatchContextualKeyword("of"))
			{
				if (!_context.IsAssignmentTarget || node.Type == Nodes.AssignmentExpression)
				{
					TolerateError("Invalid left-hand side in for-loop");
				}
				NextToken();
				node = ReinterpretExpressionAsPattern(node);
				node2 = node;
				right = ParseAssignmentExpression();
				node = null;
				flag = false;
			}
			else
			{
				_context.IsBindingElement = isBindingElement;
				_context.IsAssignmentTarget = isAssignmentTarget;
				_context.FirstCoverInitializedNameError = firstCoverInitializedNameError;
				if (Match(","))
				{
					ArrayList<Expression> arrayList = new ArrayList<Expression>(1);
					arrayList.Add((Expression)node);
					ArrayList<Expression> arrayList2 = arrayList;
					while (Match(","))
					{
						NextToken();
						arrayList2.Push(IsolateCoverGrammar(parseAssignmentExpression));
					}
					Marker marker4 = StartNode(lookahead);
					NodeList<Expression> expressions = NodeList.From(ref arrayList2);
					node = Finalize(marker4, new SequenceExpression(in expressions));
				}
				Expect(";");
			}
		}
		if (node2 == null)
		{
			if (!Match(";"))
			{
				test = IsolateCoverGrammar(parseExpression);
			}
			Expect(";");
			if (!Match(")"))
			{
				update = IsolateCoverGrammar(parseExpression);
			}
		}
		Statement body;
		if (!Match(")") && _config.Tolerant)
		{
			TolerateUnexpectedToken(NextToken());
			body = Finalize(CreateNode(), new EmptyStatement());
		}
		else
		{
			Expect(")");
			bool inIteration = _context.InIteration;
			_context.InIteration = true;
			body = IsolateCoverGrammar(parseStatement);
			_context.InIteration = inIteration;
		}
		if (node2 != null)
		{
			if (!flag)
			{
				return Finalize(marker, new ForOfStatement(node2, right, body));
			}
			return Finalize(marker, new ForInStatement(node2, right, body));
		}
		return Finalize(marker, new ForStatement(node, test, update, body));
	}

	private ContinueStatement ParseContinueStatement()
	{
		Marker marker = CreateNode();
		ExpectKeyword("continue");
		Identifier identifier = null;
		if (_lookahead.Type == TokenType.Identifier && !_hasLineTerminator)
		{
			identifier = ParseVariableIdentifier();
			string name = identifier.Name;
			if (!_context.LabelSet.Contains(name))
			{
				ThrowError("Undefined label \"{0}\"", identifier.Name);
			}
		}
		ConsumeSemicolon();
		if (identifier == null && !_context.InIteration)
		{
			ThrowError("Illegal continue statement");
		}
		return Finalize(marker, new ContinueStatement(identifier));
	}

	private BreakStatement ParseBreakStatement()
	{
		Marker marker = CreateNode();
		ExpectKeyword("break");
		Identifier identifier = null;
		if (_lookahead.Type == TokenType.Identifier && !_hasLineTerminator)
		{
			identifier = ParseVariableIdentifier();
			string name = identifier.Name;
			if (!_context.LabelSet.Contains(name))
			{
				ThrowError("Undefined label \"{0}\"", identifier.Name);
			}
		}
		ConsumeSemicolon();
		if (identifier == null && !_context.InIteration && !_context.InSwitch)
		{
			ThrowError("Illegal break statement");
		}
		return Finalize(marker, new BreakStatement(identifier));
	}

	private ReturnStatement ParseReturnStatement()
	{
		if (!_context.InFunctionBody)
		{
			TolerateError("Illegal return statement");
		}
		Marker marker = CreateNode();
		ExpectKeyword("return");
		Expression argument = (((!Match(";") && !Match("}") && !_hasLineTerminator && _lookahead.Type != TokenType.EOF) || _lookahead.Type == TokenType.StringLiteral || _lookahead.Type == TokenType.Template) ? ParseExpression() : null);
		ConsumeSemicolon();
		return Finalize(marker, new ReturnStatement(argument));
	}

	private WithStatement ParseWithStatement()
	{
		if (_context.Strict)
		{
			TolerateError("Strict mode code may not include a with statement");
		}
		Marker marker = CreateNode();
		ExpectKeyword("with");
		Expect("(");
		Expression obj = ParseExpression();
		Statement body;
		if (!Match(")") && _config.Tolerant)
		{
			TolerateUnexpectedToken(NextToken());
			body = Finalize(CreateNode(), new EmptyStatement());
		}
		else
		{
			Expect(")");
			body = ParseStatement();
		}
		return Finalize(marker, new WithStatement(obj, body));
	}

	private SwitchCase ParseSwitchCase()
	{
		Marker marker = CreateNode();
		Expression expression;
		if (MatchKeyword("default"))
		{
			NextToken();
			expression = null;
		}
		else
		{
			ExpectKeyword("case");
			expression = ParseExpression();
		}
		Expect(":");
		ArrayList<IStatementListItem> arrayList = default(ArrayList<IStatementListItem>);
		while (!Match("}") && !MatchKeyword("default") && !MatchKeyword("case"))
		{
			arrayList.Push(ParseStatementListItem());
		}
		Expression test = expression;
		NodeList<IStatementListItem> consequent = NodeList.From(ref arrayList);
		return Finalize(marker, new SwitchCase(test, in consequent));
	}

	private SwitchStatement ParseSwitchStatement()
	{
		Marker marker = CreateNode();
		ExpectKeyword("switch");
		Expect("(");
		Expression discriminant = ParseExpression();
		Expect(")");
		bool inSwitch = _context.InSwitch;
		_context.InSwitch = true;
		ArrayList<SwitchCase> arrayList = default(ArrayList<SwitchCase>);
		bool flag = false;
		Expect("{");
		while (!Match("}"))
		{
			SwitchCase switchCase = ParseSwitchCase();
			if (switchCase.Test == null)
			{
				if (flag)
				{
					ThrowError("More than one default clause in switch statement");
				}
				flag = true;
			}
			arrayList.Push(switchCase);
		}
		Expect("}");
		_context.InSwitch = inSwitch;
		NodeList<SwitchCase> cases = NodeList.From(ref arrayList);
		return Finalize(marker, new SwitchStatement(discriminant, in cases));
	}

	private Statement ParseLabelledStatement()
	{
		Marker marker = CreateNode();
		Expression expression = ParseExpression();
		Statement node;
		if (expression.Type == Nodes.Identifier && Match(":"))
		{
			NextToken();
			Identifier identifier = expression.As<Identifier>();
			string name = identifier.Name;
			if (_context.LabelSet.Contains(name))
			{
				ThrowError("{0} \"{1}\" has already been declared", "Label", identifier.Name);
			}
			_context.LabelSet.Add(name);
			Statement body;
			if (MatchKeyword("class"))
			{
				TolerateUnexpectedToken(_lookahead);
				body = ParseClassDeclaration();
			}
			else if (MatchKeyword("function"))
			{
				Token lookahead = _lookahead;
				IFunctionDeclaration functionDeclaration = ParseFunctionDeclaration();
				if (_context.Strict)
				{
					TolerateUnexpectedToken(lookahead, "In strict mode code, functions can only be declared at top level or inside a block");
				}
				else if (functionDeclaration.Generator)
				{
					TolerateUnexpectedToken(lookahead, "Generator declarations are not allowed in legacy contexts");
				}
				body = (Statement)functionDeclaration;
			}
			else
			{
				body = ParseStatement();
			}
			_context.LabelSet.Remove(name);
			node = new LabeledStatement(identifier, body);
		}
		else
		{
			ConsumeSemicolon();
			node = new ExpressionStatement(expression);
		}
		return Finalize(marker, node);
	}

	private ThrowStatement ParseThrowStatement()
	{
		Marker marker = CreateNode();
		ExpectKeyword("throw");
		if (_hasLineTerminator)
		{
			ThrowError("Illegal newline after throw");
		}
		Expression argument = ParseExpression();
		ConsumeSemicolon();
		return Finalize(marker, new ThrowStatement(argument));
	}

	private CatchClause ParseCatchClause()
	{
		Marker marker = CreateNode();
		ExpectKeyword("catch");
		Expect("(");
		if (Match(")"))
		{
			ThrowUnexpectedToken(_lookahead);
		}
		ArrayList<Token> parameters = default(ArrayList<Token>);
		IArrayPatternElement arrayPatternElement = ParsePattern(ref parameters);
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
		for (int i = 0; i < parameters.Count; i++)
		{
			string key = (string)parameters[i].Value;
			if (dictionary.ContainsKey(key))
			{
				TolerateError("Duplicate binding {0}", parameters[i].Value);
			}
			dictionary[key] = true;
		}
		if (_context.Strict && arrayPatternElement.Type == Nodes.Identifier && Scanner.IsRestrictedWord(arrayPatternElement.As<Identifier>().Name))
		{
			TolerateError("Catch variable may not be eval or arguments in strict mode");
		}
		Expect(")");
		BlockStatement body = ParseBlock();
		return Finalize(marker, new CatchClause(arrayPatternElement.As<IArrayPatternElement>(), body));
	}

	private BlockStatement ParseFinallyClause()
	{
		ExpectKeyword("finally");
		return ParseBlock();
	}

	private TryStatement ParseTryStatement()
	{
		Marker marker = CreateNode();
		ExpectKeyword("try");
		BlockStatement block = ParseBlock();
		CatchClause catchClause = (MatchKeyword("catch") ? ParseCatchClause() : null);
		BlockStatement blockStatement = (MatchKeyword("finally") ? ParseFinallyClause() : null);
		if (catchClause == null && blockStatement == null)
		{
			ThrowError("Missing catch or finally after try");
		}
		return Finalize(marker, new TryStatement(block, catchClause, blockStatement));
	}

	private DebuggerStatement ParseDebuggerStatement()
	{
		Marker marker = CreateNode();
		ExpectKeyword("debugger");
		ConsumeSemicolon();
		return Finalize(marker, new DebuggerStatement());
	}

	private Statement ParseStatement()
	{
		Statement result = null;
		switch (_lookahead.Type)
		{
		case TokenType.BooleanLiteral:
		case TokenType.NullLiteral:
		case TokenType.NumericLiteral:
		case TokenType.StringLiteral:
		case TokenType.RegularExpression:
		case TokenType.Template:
			result = ParseExpressionStatement();
			break;
		case TokenType.Punctuator:
			result = (string)_lookahead.Value switch
			{
				"{" => ParseBlock(), 
				"(" => ParseExpressionStatement(), 
				";" => ParseEmptyStatement(), 
				_ => ParseExpressionStatement(), 
			};
			break;
		case TokenType.Identifier:
			result = (MatchAsyncFunction() ? ((Statement)ParseFunctionDeclaration()) : ParseLabelledStatement());
			break;
		case TokenType.Keyword:
			result = (string)_lookahead.Value switch
			{
				"break" => ParseBreakStatement(), 
				"continue" => ParseContinueStatement(), 
				"debugger" => ParseDebuggerStatement(), 
				"do" => ParseDoWhileStatement(), 
				"for" => ParseForStatement(), 
				"function" => (Statement)ParseFunctionDeclaration(), 
				"if" => ParseIfStatement(), 
				"return" => ParseReturnStatement(), 
				"switch" => ParseSwitchStatement(), 
				"throw" => ParseThrowStatement(), 
				"try" => ParseTryStatement(), 
				"var" => ParseVariableStatement(), 
				"while" => ParseWhileStatement(), 
				"with" => ParseWithStatement(), 
				_ => ParseExpressionStatement(), 
			};
			break;
		default:
			ThrowUnexpectedToken(_lookahead);
			break;
		}
		return result;
	}

	private BlockStatement ParseFunctionSourceElements()
	{
		Marker marker = CreateNode();
		Expect("{");
		ArrayList<IStatementListItem> arrayList = ParseDirectivePrologues();
		bool flag = _context.LabelSet.Count == 0;
		HashSet<string> labelSet = _context.LabelSet;
		bool inIteration = _context.InIteration;
		bool inSwitch = _context.InSwitch;
		bool inFunctionBody = _context.InFunctionBody;
		_context.LabelSet = (flag ? labelSet : new HashSet<string>());
		_context.InIteration = false;
		_context.InSwitch = false;
		_context.InFunctionBody = true;
		while (_lookahead.Type != TokenType.EOF && !Match("}"))
		{
			arrayList.Push(ParseStatementListItem());
		}
		Expect("}");
		_context.LabelSet = labelSet;
		if (flag)
		{
			_context.LabelSet.Clear();
		}
		_context.InIteration = inIteration;
		_context.InSwitch = inSwitch;
		_context.InFunctionBody = inFunctionBody;
		NodeList<IStatementListItem> body = NodeList.From(ref arrayList);
		return Finalize(marker, new BlockStatement(in body));
	}

	private void ValidateParam(ParsedParameters options, INode param, string name)
	{
		if (_context.Strict)
		{
			if (Scanner.IsRestrictedWord(name))
			{
				options.Stricted = new Token();
				options.Message = "Parameter name eval or arguments is not allowed in strict mode";
			}
			if (options.ParamSetContains(name))
			{
				options.Stricted = new Token();
				options.Message = "Strict mode function may not have duplicate parameter names";
			}
		}
		else if (options.FirstRestricted == null)
		{
			if (Scanner.IsRestrictedWord(name))
			{
				options.FirstRestricted = new Token();
				options.Message = "Parameter name eval or arguments is not allowed in strict mode";
			}
			else if (Scanner.IsStrictModeReservedWord(name))
			{
				options.FirstRestricted = new Token();
				options.Message = "Use of future reserved word in strict mode";
			}
			else if (options.ParamSetContains(name))
			{
				options.Stricted = new Token();
				options.Message = "Strict mode function may not have duplicate parameter names";
			}
		}
		options.ParamSetAdd(name);
	}

	private void ValidateParam2(ParsedParameters options, Token param, string name)
	{
		if (_context.Strict)
		{
			if (Scanner.IsRestrictedWord(name))
			{
				options.Stricted = param;
				options.Message = "Parameter name eval or arguments is not allowed in strict mode";
			}
			if (options.ParamSetContains(name))
			{
				options.Stricted = param;
				options.Message = "Strict mode function may not have duplicate parameter names";
			}
		}
		else if (options.FirstRestricted == null)
		{
			if (Scanner.IsRestrictedWord(name))
			{
				options.FirstRestricted = param;
				options.Message = "Parameter name eval or arguments is not allowed in strict mode";
			}
			else if (Scanner.IsStrictModeReservedWord(name))
			{
				options.FirstRestricted = param;
				options.Message = "Use of future reserved word in strict mode";
			}
			else if (options.ParamSetContains(name))
			{
				options.Stricted = param;
				options.Message = "Strict mode function may not have duplicate parameter names";
			}
		}
		options.ParamSetAdd(name);
	}

	private RestElement ParseRestElement(ref ArrayList<Token> parameters)
	{
		Marker marker = CreateNode();
		Expect("...");
		IArrayPatternElement argument = ParsePattern(ref parameters);
		if (Match("="))
		{
			ThrowError("Unexpected token =");
		}
		if (!Match(")"))
		{
			ThrowError("Rest parameter must be last formal parameter");
		}
		return Finalize(marker, new RestElement(argument));
	}

	private void ParseFormalParameter(ParsedParameters options)
	{
		ArrayList<Token> parameters = default(ArrayList<Token>);
		IArrayPatternElement arrayPatternElement;
		if (!Match("..."))
		{
			arrayPatternElement = ParsePatternWithDefault(ref parameters);
		}
		else
		{
			IArrayPatternElement arrayPatternElement2 = ParseRestElement(ref parameters);
			arrayPatternElement = arrayPatternElement2;
		}
		INode node = arrayPatternElement;
		for (int i = 0; i < parameters.Count; i++)
		{
			ValidateParam2(options, parameters[i], (string)parameters[i].Value);
		}
		options.Simple = options.Simple && node is Identifier;
		options.Parameters.Push(node);
	}

	private ParsedParameters ParseFormalParameters(Token firstRestricted = null)
	{
		ParsedParameters parsedParameters = new ParsedParameters
		{
			Simple = true,
			FirstRestricted = firstRestricted
		};
		Expect("(");
		if (!Match(")"))
		{
			parsedParameters.Parameters = default(ArrayList<INode>);
			while (_lookahead.Type != TokenType.EOF)
			{
				ParseFormalParameter(parsedParameters);
				if (Match(")"))
				{
					break;
				}
				Expect(",");
				if (Match(")"))
				{
					break;
				}
			}
		}
		Expect(")");
		return new ParsedParameters
		{
			Simple = parsedParameters.Simple,
			Parameters = parsedParameters.Parameters,
			Stricted = parsedParameters.Stricted,
			FirstRestricted = parsedParameters.FirstRestricted,
			Message = parsedParameters.Message
		};
	}

	private bool MatchAsyncFunction()
	{
		bool flag = MatchContextualKeyword("async");
		if (flag)
		{
			ScannerState state = _scanner.SaveState();
			_scanner.ScanComments();
			Token token = _scanner.Lex();
			_scanner.RestoreState(in state);
			flag = state.LineNumber == token.LineNumber && token.Type == TokenType.Keyword && (string)token.Value == "function";
		}
		return flag;
	}

	private IFunctionDeclaration ParseFunctionDeclaration(bool identifierIsOptional = false)
	{
		EnterHoistingScope();
		Marker marker = CreateNode();
		bool flag = MatchContextualKeyword("async");
		if (flag)
		{
			NextToken();
		}
		ExpectKeyword("function");
		bool flag2 = !flag && Match("*");
		if (flag2)
		{
			NextToken();
		}
		string message = null;
		Identifier id = null;
		Token firstRestricted = null;
		if (!identifierIsOptional || !Match("("))
		{
			Token lookahead = _lookahead;
			id = ParseVariableIdentifier();
			if (_context.Strict)
			{
				if (Scanner.IsRestrictedWord((string)lookahead.Value))
				{
					TolerateUnexpectedToken(lookahead, "Function name may not be eval or arguments in strict mode");
				}
			}
			else if (Scanner.IsRestrictedWord((string)lookahead.Value))
			{
				firstRestricted = lookahead;
				message = "Function name may not be eval or arguments in strict mode";
			}
			else if (Scanner.IsStrictModeReservedWord((string)lookahead.Value))
			{
				firstRestricted = lookahead;
				message = "Use of future reserved word in strict mode";
			}
		}
		bool await = _context.Await;
		bool allowYield = _context.AllowYield;
		_context.Await = flag;
		_context.AllowYield = !flag2;
		ParsedParameters parsedParameters = ParseFormalParameters(firstRestricted);
		NodeList<INode> parameters = NodeList.From(ref parsedParameters.Parameters);
		Token stricted = parsedParameters.Stricted;
		firstRestricted = parsedParameters.FirstRestricted;
		if (parsedParameters.Message != null)
		{
			message = parsedParameters.Message;
		}
		bool strict = _context.Strict;
		bool allowStrictDirective = _context.AllowStrictDirective;
		_context.AllowStrictDirective = parsedParameters.Simple;
		BlockStatement body = ParseFunctionSourceElements();
		if (_context.Strict && firstRestricted != null)
		{
			ThrowUnexpectedToken(firstRestricted, message);
		}
		if (_context.Strict && stricted != null)
		{
			TolerateUnexpectedToken(stricted, message);
		}
		bool strict2 = _context.Strict;
		_context.AllowStrictDirective = allowStrictDirective;
		_context.Strict = strict;
		_context.Await = await;
		_context.AllowYield = allowYield;
		FunctionDeclaration functionDeclaration = Finalize(marker, new FunctionDeclaration(id, in parameters, body, flag2, strict2, flag, LeaveHoistingScope()));
		HoistingScopeLists item = _hoistingScopes.Pop();
		item.FunctionDeclarations.Add(functionDeclaration);
		_hoistingScopes.Push(item);
		return functionDeclaration;
	}

	private FunctionExpression ParseFunctionExpression()
	{
		EnterHoistingScope();
		Marker marker = CreateNode();
		bool flag = MatchContextualKeyword("async");
		if (flag)
		{
			NextToken();
		}
		ExpectKeyword("function");
		bool flag2 = !flag && Match("*");
		if (flag2)
		{
			NextToken();
		}
		string message = null;
		Expression expression = null;
		Token firstRestricted = null;
		bool await = _context.Await;
		bool allowYield = _context.AllowYield;
		_context.Await = flag;
		_context.AllowYield = !flag2;
		if (!Match("("))
		{
			Token lookahead = _lookahead;
			expression = ((!_context.Strict && !flag2 && MatchKeyword("yield")) ? ParseIdentifierName() : ParseVariableIdentifier());
			if (_context.Strict)
			{
				if (Scanner.IsRestrictedWord((string)lookahead.Value))
				{
					TolerateUnexpectedToken(lookahead, "Function name may not be eval or arguments in strict mode");
				}
			}
			else if (Scanner.IsRestrictedWord((string)lookahead.Value))
			{
				firstRestricted = lookahead;
				message = "Function name may not be eval or arguments in strict mode";
			}
			else if (Scanner.IsStrictModeReservedWord((string)lookahead.Value))
			{
				firstRestricted = lookahead;
				message = "Use of future reserved word in strict mode";
			}
		}
		ParsedParameters parsedParameters = ParseFormalParameters(firstRestricted);
		NodeList<INode> parameters = NodeList.From(ref parsedParameters.Parameters);
		Token stricted = parsedParameters.Stricted;
		firstRestricted = parsedParameters.FirstRestricted;
		if (parsedParameters.Message != null)
		{
			message = parsedParameters.Message;
		}
		bool strict = _context.Strict;
		bool allowStrictDirective = _context.AllowStrictDirective;
		_context.AllowStrictDirective = parsedParameters.Simple;
		BlockStatement body = ParseFunctionSourceElements();
		if (_context.Strict && firstRestricted != null)
		{
			ThrowUnexpectedToken(firstRestricted, message);
		}
		if (_context.Strict && stricted != null)
		{
			TolerateUnexpectedToken(stricted, message);
		}
		bool strict2 = _context.Strict;
		_context.Strict = strict;
		_context.AllowStrictDirective = allowStrictDirective;
		_context.Await = await;
		_context.AllowYield = allowYield;
		return Finalize(marker, new FunctionExpression((Identifier)expression, in parameters, body, flag2, strict2, flag, LeaveHoistingScope()));
	}

	private ExpressionStatement ParseDirective()
	{
		Token lookahead = _lookahead;
		string text = null;
		Marker marker = CreateNode();
		Expression expression = ParseExpression();
		if (expression.Type == Nodes.Literal)
		{
			text = GetTokenRaw(lookahead).Slice(1, -1);
		}
		ConsumeSemicolon();
		return Finalize(marker, (text != null) ? new Directive(expression, text) : new ExpressionStatement(expression));
	}

	private ArrayList<IStatementListItem> ParseDirectivePrologues()
	{
		Token token = null;
		ArrayList<IStatementListItem> result = default(ArrayList<IStatementListItem>);
		while (true)
		{
			Token lookahead = _lookahead;
			if (lookahead.Type != TokenType.StringLiteral)
			{
				break;
			}
			ExpressionStatement expressionStatement = ParseDirective();
			result.Push(expressionStatement);
			string text = (expressionStatement as Directive)?.Directiv;
			if (text == null)
			{
				break;
			}
			if (text == "use strict")
			{
				_context.Strict = true;
				if (token != null)
				{
					TolerateUnexpectedToken(token, "Octal literals are not allowed in strict mode.");
				}
				if (!_context.AllowStrictDirective)
				{
					TolerateUnexpectedToken(lookahead, "Illegal 'use strict' directive in function with non-simple parameter list");
				}
			}
			else if (token == null && lookahead.Octal)
			{
				token = lookahead;
			}
		}
		return result;
	}

	private static bool QualifiedPropertyName(Token token)
	{
		switch (token.Type)
		{
		case TokenType.BooleanLiteral:
		case TokenType.Identifier:
		case TokenType.Keyword:
		case TokenType.NullLiteral:
		case TokenType.NumericLiteral:
		case TokenType.StringLiteral:
			return true;
		case TokenType.Punctuator:
			return (string)token.Value == "[";
		default:
			return false;
		}
	}

	private FunctionExpression ParseGetterMethod()
	{
		EnterHoistingScope();
		Marker marker = CreateNode();
		bool allowYield = _context.AllowYield;
		_context.AllowYield = true;
		ParsedParameters parsedParameters = ParseFormalParameters();
		if (parsedParameters.Parameters.Count > 0)
		{
			TolerateError("Getter must not have any formal parameters");
		}
		BlockStatement body = ParsePropertyMethod(parsedParameters);
		_context.AllowYield = allowYield;
		NodeList<INode> parameters = NodeList.From(ref parsedParameters.Parameters);
		return Finalize(marker, new FunctionExpression(null, in parameters, body, generator: false, _context.Strict, async: false, LeaveHoistingScope()));
	}

	private FunctionExpression ParseSetterMethod()
	{
		EnterHoistingScope();
		Marker marker = CreateNode();
		bool allowYield = _context.AllowYield;
		_context.AllowYield = true;
		ParsedParameters parsedParameters = ParseFormalParameters();
		if (parsedParameters.Parameters.Count != 1)
		{
			TolerateError("Setter must have exactly one formal parameter");
		}
		else if (parsedParameters.Parameters[0] is RestElement)
		{
			TolerateError("Setter function argument must not be a rest parameter");
		}
		BlockStatement body = ParsePropertyMethod(parsedParameters);
		_context.AllowYield = allowYield;
		NodeList<INode> parameters = NodeList.From(ref parsedParameters.Parameters);
		return Finalize(marker, new FunctionExpression(null, in parameters, body, generator: false, _context.Strict, async: false, LeaveHoistingScope()));
	}

	private FunctionExpression ParseGeneratorMethod()
	{
		EnterHoistingScope();
		Marker marker = CreateNode();
		bool allowYield = _context.AllowYield;
		_context.AllowYield = true;
		ParsedParameters parsedParameters = ParseFormalParameters();
		_context.AllowYield = false;
		BlockStatement body = ParsePropertyMethod(parsedParameters);
		_context.AllowYield = allowYield;
		NodeList<INode> parameters = NodeList.From(ref parsedParameters.Parameters);
		return Finalize(marker, new FunctionExpression(null, in parameters, body, generator: true, _context.Strict, async: false, LeaveHoistingScope()));
	}

	private bool IsStartOfExpression()
	{
		bool result = true;
		if (!(_lookahead.Value is string item))
		{
			return result;
		}
		switch (_lookahead.Type)
		{
		case TokenType.Punctuator:
			result = PunctuatorExpressionStart.Contains(item);
			break;
		case TokenType.Keyword:
			result = KeywordExpressionStart.Contains(item);
			break;
		}
		return result;
	}

	private YieldExpression ParseYieldExpression()
	{
		Marker marker = CreateNode();
		ExpectKeyword("yield");
		Expression argument = null;
		bool flag = false;
		if (!_hasLineTerminator)
		{
			bool allowYield = _context.AllowYield;
			_context.AllowYield = false;
			flag = Match("*");
			if (flag)
			{
				NextToken();
				argument = ParseAssignmentExpression();
			}
			else if (IsStartOfExpression())
			{
				argument = ParseAssignmentExpression();
			}
			_context.AllowYield = allowYield;
		}
		return Finalize(marker, new YieldExpression(argument, flag));
	}

	private ClassProperty ParseClassElement(ref bool hasConstructor)
	{
		Token lookahead = _lookahead;
		Marker marker = CreateNode();
		PropertyKind propertyKind = PropertyKind.None;
		Expression expression = null;
		FunctionExpression functionExpression = null;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		if (Match("*"))
		{
			NextToken();
		}
		else
		{
			flag = Match("[");
			expression = ParseObjectPropertyKey();
			if (expression.Type switch
			{
				Nodes.Identifier => expression.As<Identifier>().Name, 
				Nodes.Literal => expression.As<Literal>().StringValue, 
				_ => throw new NotSupportedException(), 
			} == "static" && (QualifiedPropertyName(_lookahead) || Match("*")))
			{
				lookahead = _lookahead;
				flag3 = true;
				flag = Match("[");
				if (Match("*"))
				{
					NextToken();
				}
				else
				{
					expression = ParseObjectPropertyKey();
				}
			}
			if (lookahead.Type == TokenType.Identifier && !_hasLineTerminator && (string)lookahead.Value == "async" && (!(_lookahead.Value is string text) || (text != ":" && text != "(" && text != "*")))
			{
				flag4 = true;
				lookahead = _lookahead;
				expression = ParseObjectPropertyKey();
				if (lookahead.Type == TokenType.Identifier && (string)lookahead.Value == "constructor")
				{
					TolerateUnexpectedToken(lookahead, "Class constructor may not be an async method");
				}
			}
		}
		bool flag5 = QualifiedPropertyName(_lookahead);
		if (lookahead.Type == TokenType.Identifier)
		{
			if ((string)lookahead.Value == "get" && flag5)
			{
				propertyKind = PropertyKind.Get;
				flag = Match("[");
				expression = ParseObjectPropertyKey();
				_context.AllowYield = false;
				functionExpression = ParseGetterMethod();
			}
			else if ((string)lookahead.Value == "set" && flag5)
			{
				propertyKind = PropertyKind.Set;
				flag = Match("[");
				expression = ParseObjectPropertyKey();
				functionExpression = ParseSetterMethod();
			}
		}
		else if (lookahead.Type == TokenType.Punctuator && (string)lookahead.Value == "*" && flag5)
		{
			propertyKind = PropertyKind.Init;
			flag = Match("[");
			expression = ParseObjectPropertyKey();
			functionExpression = ParseGeneratorMethod();
			flag2 = true;
		}
		if (propertyKind == PropertyKind.None && expression != null && Match("("))
		{
			propertyKind = PropertyKind.Init;
			functionExpression = (flag4 ? ParsePropertyMethodAsyncFunction() : ParsePropertyMethodFunction());
			flag2 = true;
		}
		if (propertyKind == PropertyKind.None)
		{
			ThrowUnexpectedToken(_lookahead);
		}
		if (propertyKind == PropertyKind.Init)
		{
			propertyKind = PropertyKind.Method;
		}
		if (!flag)
		{
			if (flag3 && IsPropertyKey(expression, "prototype"))
			{
				ThrowUnexpectedToken(lookahead, "Classes may not have static property named prototype");
			}
			if (!flag3 && IsPropertyKey(expression, "constructor"))
			{
				if (propertyKind != PropertyKind.Method || !flag2 || functionExpression.Generator)
				{
					ThrowUnexpectedToken(lookahead, "Class constructor may not be an accessor");
				}
				if (hasConstructor)
				{
					ThrowUnexpectedToken(lookahead, "A class may only have one constructor");
				}
				else
				{
					hasConstructor = true;
				}
				propertyKind = PropertyKind.Constructor;
			}
		}
		return Finalize(marker, new MethodDefinition(expression, flag, functionExpression, propertyKind, flag3));
	}

	private ArrayList<ClassProperty> ParseClassElementList()
	{
		ArrayList<ClassProperty> result = default(ArrayList<ClassProperty>);
		bool hasConstructor = false;
		Expect("{");
		while (!Match("}"))
		{
			if (Match(";"))
			{
				NextToken();
			}
			else
			{
				result.Push(ParseClassElement(ref hasConstructor));
			}
		}
		Expect("}");
		return result;
	}

	private ClassBody ParseClassBody()
	{
		Marker marker = CreateNode();
		ArrayList<ClassProperty> arrayList = ParseClassElementList();
		NodeList<ClassProperty> body = NodeList.From(ref arrayList);
		return Finalize(marker, new ClassBody(in body));
	}

	private ClassDeclaration ParseClassDeclaration(bool identifierIsOptional = false)
	{
		Marker marker = CreateNode();
		bool strict = _context.Strict;
		_context.Strict = true;
		ExpectKeyword("class");
		Identifier id = ((identifierIsOptional && _lookahead.Type != TokenType.Identifier) ? null : ParseVariableIdentifier());
		Expression superClass = null;
		if (MatchKeyword("extends"))
		{
			NextToken();
			superClass = IsolateCoverGrammar(ParseLeftHandSideExpressionAllowCall);
		}
		ClassBody body = ParseClassBody();
		_context.Strict = strict;
		return Finalize(marker, new ClassDeclaration(id, superClass, body));
	}

	private ClassExpression ParseClassExpression()
	{
		Marker marker = CreateNode();
		bool strict = _context.Strict;
		_context.Strict = true;
		ExpectKeyword("class");
		Identifier id = ((_lookahead.Type == TokenType.Identifier) ? ParseVariableIdentifier() : null);
		Expression superClass = null;
		if (MatchKeyword("extends"))
		{
			NextToken();
			superClass = IsolateCoverGrammar(ParseLeftHandSideExpressionAllowCall);
		}
		ClassBody body = ParseClassBody();
		_context.Strict = strict;
		return Finalize(marker, new ClassExpression(id, superClass, body));
	}

	private Literal ParseModuleSpecifier()
	{
		Marker marker = CreateNode();
		if (_lookahead.Type != TokenType.StringLiteral)
		{
			ThrowError("Unexpected token");
		}
		Token token = NextToken();
		string tokenRaw = GetTokenRaw(token);
		return Finalize(marker, new Literal((string)token.Value, tokenRaw));
	}

	private ImportSpecifier ParseImportSpecifier()
	{
		Marker marker = CreateNode();
		Identifier identifier;
		Identifier local;
		if (_lookahead.Type == TokenType.Identifier)
		{
			identifier = ParseVariableIdentifier();
			local = identifier;
			if (MatchContextualKeyword("as"))
			{
				NextToken();
				local = ParseVariableIdentifier();
			}
		}
		else
		{
			identifier = ParseIdentifierName();
			local = identifier;
			if (MatchContextualKeyword("as"))
			{
				NextToken();
				local = ParseVariableIdentifier();
			}
			else
			{
				ThrowUnexpectedToken(NextToken());
			}
		}
		return Finalize(marker, new ImportSpecifier(local, identifier));
	}

	private ArrayList<ImportSpecifier> ParseNamedImports()
	{
		Expect("{");
		ArrayList<ImportSpecifier> result = default(ArrayList<ImportSpecifier>);
		while (!Match("}"))
		{
			result.Push(ParseImportSpecifier());
			if (!Match("}"))
			{
				Expect(",");
			}
		}
		Expect("}");
		return result;
	}

	private ImportDefaultSpecifier ParseImportDefaultSpecifier()
	{
		Marker marker = CreateNode();
		Identifier local = ParseIdentifierName();
		return Finalize(marker, new ImportDefaultSpecifier(local));
	}

	private ImportNamespaceSpecifier ParseImportNamespaceSpecifier()
	{
		Marker marker = CreateNode();
		Expect("*");
		if (!MatchContextualKeyword("as"))
		{
			ThrowError("Unexpected token");
		}
		NextToken();
		Identifier local = ParseIdentifierName();
		return Finalize(marker, new ImportNamespaceSpecifier(local));
	}

	private ImportDeclaration ParseImportDeclaration()
	{
		if (_context.InFunctionBody)
		{
			ThrowError("Unexpected token");
		}
		Marker marker = CreateNode();
		ExpectKeyword("import");
		ArrayList<ImportDeclarationSpecifier> arrayList = default(ArrayList<ImportDeclarationSpecifier>);
		Literal source;
		if (_lookahead.Type == TokenType.StringLiteral)
		{
			source = ParseModuleSpecifier();
		}
		else
		{
			if (Match("{"))
			{
				arrayList.AddRange(ParseNamedImports());
			}
			else if (Match("*"))
			{
				arrayList.Push(ParseImportNamespaceSpecifier());
			}
			else if (IsIdentifierName(_lookahead) && !MatchKeyword("default"))
			{
				arrayList.Push(ParseImportDefaultSpecifier());
				if (Match(","))
				{
					NextToken();
					if (Match("*"))
					{
						arrayList.Push(ParseImportNamespaceSpecifier());
					}
					else if (Match("{"))
					{
						arrayList.AddRange(ParseNamedImports());
					}
					else
					{
						ThrowUnexpectedToken(_lookahead);
					}
				}
			}
			else
			{
				ThrowUnexpectedToken(NextToken());
			}
			if (!MatchContextualKeyword("from"))
			{
				string messageFormat = ((_lookahead.Value != null) ? "Unexpected token {0}" : "Unexpected token");
				ThrowError(messageFormat, _lookahead.Value);
			}
			NextToken();
			source = ParseModuleSpecifier();
		}
		ConsumeSemicolon();
		NodeList<ImportDeclarationSpecifier> specifiers = NodeList.From(ref arrayList);
		return Finalize(marker, new ImportDeclaration(in specifiers, source));
	}

	private ExportSpecifier ParseExportSpecifier()
	{
		Marker marker = CreateNode();
		Identifier identifier = ParseIdentifierName();
		Identifier exported = identifier;
		if (MatchContextualKeyword("as"))
		{
			NextToken();
			exported = ParseIdentifierName();
		}
		return Finalize(marker, new ExportSpecifier(identifier, exported));
	}

	private IStatementListItem ParseExportDeclaration()
	{
		if (_context.InFunctionBody)
		{
			ThrowError("Unexpected token");
		}
		Marker marker = CreateNode();
		ExpectKeyword("export");
		if (MatchKeyword("default"))
		{
			NextToken();
			if (MatchKeyword("function"))
			{
				IFunctionDeclaration declaration = ParseFunctionDeclaration(identifierIsOptional: true);
				return Finalize(marker, new ExportDefaultDeclaration(declaration));
			}
			if (MatchKeyword("class"))
			{
				ClassDeclaration declaration2 = ParseClassDeclaration(identifierIsOptional: true);
				return Finalize(marker, new ExportDefaultDeclaration(declaration2));
			}
			if (MatchContextualKeyword("async"))
			{
				IDeclaration declaration4;
				if (!MatchAsyncFunction())
				{
					IDeclaration declaration3 = ParseAssignmentExpression();
					declaration4 = declaration3;
				}
				else
				{
					IDeclaration declaration3 = ParseFunctionDeclaration(identifierIsOptional: true);
					declaration4 = declaration3;
				}
				IDeclaration declaration5 = declaration4;
				return Finalize(marker, new ExportDefaultDeclaration(declaration5));
			}
			if (MatchContextualKeyword("from"))
			{
				ThrowError("Unexpected token {0}", _lookahead.Value);
			}
			Expression expression;
			if (!Match("{"))
			{
				if (!Match("["))
				{
					expression = ParseAssignmentExpression();
				}
				else
				{
					Expression expression2 = ParseArrayInitializer();
					expression = expression2;
				}
			}
			else
			{
				Expression expression2 = ParseObjectInitializer();
				expression = expression2;
			}
			Expression declaration6 = expression;
			ConsumeSemicolon();
			return Finalize(marker, new ExportDefaultDeclaration(declaration6));
		}
		if (Match("*"))
		{
			NextToken();
			if (!MatchContextualKeyword("from"))
			{
				string messageFormat = ((_lookahead.Value != null) ? "Unexpected token {0}" : "Unexpected token");
				ThrowError(messageFormat, _lookahead.Value);
			}
			NextToken();
			Literal source = ParseModuleSpecifier();
			ConsumeSemicolon();
			return Finalize(marker, new ExportAllDeclaration(source));
		}
		NodeList<ExportSpecifier> specifiers;
		if (_lookahead.Type == TokenType.Keyword)
		{
			IStatementListItem statementListItem = null;
			switch (_lookahead.Value as string)
			{
			case "let":
			case "const":
			{
				bool inFor = false;
				statementListItem = ParseLexicalDeclaration(ref inFor);
				break;
			}
			case "var":
			case "class":
			case "function":
				statementListItem = ParseStatementListItem();
				break;
			default:
				ThrowUnexpectedToken(_lookahead);
				break;
			}
			IStatementListItem declaration7 = statementListItem;
			specifiers = default(NodeList<ExportSpecifier>);
			return Finalize(marker, new ExportNamedDeclaration(declaration7, in specifiers, null));
		}
		if (MatchAsyncFunction())
		{
			IFunctionDeclaration declaration8 = ParseFunctionDeclaration();
			specifiers = default(NodeList<ExportSpecifier>);
			return Finalize(marker, new ExportNamedDeclaration(declaration8, in specifiers, null));
		}
		ArrayList<ExportSpecifier> arrayList = default(ArrayList<ExportSpecifier>);
		Literal source2 = null;
		bool flag = false;
		Expect("{");
		while (!Match("}"))
		{
			flag = flag || MatchKeyword("default");
			arrayList.Push(ParseExportSpecifier());
			if (!Match("}"))
			{
				Expect(",");
			}
		}
		Expect("}");
		if (MatchContextualKeyword("from"))
		{
			NextToken();
			source2 = ParseModuleSpecifier();
			ConsumeSemicolon();
		}
		else if (flag)
		{
			string messageFormat2 = ((_lookahead.Value != null) ? "Unexpected token {0}" : "Unexpected token");
			ThrowError(messageFormat2, _lookahead.Value);
		}
		else
		{
			ConsumeSemicolon();
		}
		specifiers = NodeList.From(ref arrayList);
		return Finalize(marker, new ExportNamedDeclaration(null, in specifiers, source2));
	}

	private void ThrowError(string messageFormat, params object[] values)
	{
		throw CreateError(messageFormat, values);
	}

	private ParserException CreateError(string messageFormat, params object[] values)
	{
		string message = string.Format(messageFormat, values);
		int index = _lastMarker.Index;
		int line = _lastMarker.Line;
		int column = _lastMarker.Column + 1;
		return _errorHandler.CreateError(index, line, column, message);
	}

	private void TolerateError(string messageFormat, params object[] values)
	{
		string message = string.Format(messageFormat, values);
		int index = _lastMarker.Index;
		int lineNumber = _scanner.LineNumber;
		int column = _lastMarker.Column + 1;
		_errorHandler.TolerateError(index, lineNumber, column, message);
	}

	private ParserException UnexpectedTokenError(Token token, string message = null)
	{
		string format = message ?? "Unexpected token {0}";
		string arg;
		if (token != null)
		{
			if (message == null)
			{
				format = ((token.Type == TokenType.EOF) ? "Unexpected end of input" : ((token.Type == TokenType.Identifier) ? "Unexpected identifier" : ((token.Type == TokenType.NumericLiteral) ? "Unexpected number" : ((token.Type == TokenType.StringLiteral) ? "Unexpected string" : ((token.Type == TokenType.Template) ? "Unexpected quasi {0}" : "Unexpected token {0}")))));
				if (token.Type == TokenType.Keyword)
				{
					if (Scanner.IsFutureReservedWord((string)token.Value))
					{
						format = "Unexpected reserved word";
					}
					else if (_context.Strict && Scanner.IsStrictModeReservedWord((string)token.Value))
					{
						format = "Use of future reserved word in strict mode";
					}
				}
			}
			arg = ((token.Type == TokenType.Template) ? token.RawTemplate : Convert.ToString(token.Value));
		}
		else
		{
			arg = "ILLEGAL";
		}
		format = string.Format(format, arg);
		if (token != null && token.LineNumber > 0)
		{
			int start = token.Start;
			int lineNumber = token.LineNumber;
			int num = _lastMarker.Index - _lastMarker.Column;
			int column = token.Start - num + 1;
			return _errorHandler.CreateError(start, lineNumber, column, format);
		}
		int index = _lastMarker.Index;
		int line = _lastMarker.Line;
		int column2 = _lastMarker.Column + 1;
		return _errorHandler.CreateError(index, line, column2, format);
	}

	private void ThrowUnexpectedToken(Token token = null, string message = null)
	{
		throw UnexpectedTokenError(token, message);
	}

	private void TolerateUnexpectedToken(Token token, string message = null)
	{
		_errorHandler.Tolerate(UnexpectedTokenError(token, message));
	}

	private void EnterHoistingScope()
	{
		_hoistingScopes.Push(default(HoistingScopeLists));
	}

	private HoistingScope LeaveHoistingScope()
	{
		HoistingScopeLists hoistingScopeLists = _hoistingScopes.Pop();
		NodeList<IFunctionDeclaration> functionDeclarations = NodeList.From(ref hoistingScopeLists.FunctionDeclarations);
		NodeList<VariableDeclaration> variableDeclarations = NodeList.From(ref hoistingScopeLists.VariableDeclarations);
		return new HoistingScope(in functionDeclarations, in variableDeclarations);
	}

	private VariableDeclaration Hoist(VariableDeclaration variableDeclaration)
	{
		if (variableDeclaration.Kind == VariableDeclarationKind.Var)
		{
			HoistingScopeLists item = _hoistingScopes.Pop();
			item.VariableDeclarations.Add(variableDeclaration);
			_hoistingScopes.Push(item);
		}
		return variableDeclaration;
	}
}
