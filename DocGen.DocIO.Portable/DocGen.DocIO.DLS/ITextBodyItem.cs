using System.Text.RegularExpressions;

namespace DocGen.DocIO.DLS;

public interface ITextBodyItem : IEntity
{
	int Replace(Regex pattern, string replace);

	int Replace(string given, string replace, bool caseSensitive, bool wholeWord);

	int Replace(Regex pattern, TextSelection textSelection);
}
