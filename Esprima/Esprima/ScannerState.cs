using System.Collections.Generic;

namespace Esprima;

internal readonly struct ScannerState
{
	public readonly int Index;

	public readonly int LineNumber;

	public readonly int LineStart;

	public readonly Stack<string> CurlyStack;

	public ScannerState(int index, int lineNumber, int lineStart, Stack<string> curlyStack)
	{
		Index = index;
		LineNumber = lineNumber;
		LineStart = lineStart;
		CurlyStack = curlyStack;
	}
}
