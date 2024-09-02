namespace DocGen.Office;

internal class UnicodeLanguageInfo
{
	internal string LanguageName;

	internal int startAt;

	internal int endAt;

	internal UnicodeLanguageInfo(string name, int s, int e)
	{
		LanguageName = name;
		startAt = s;
		endAt = e;
	}
}
