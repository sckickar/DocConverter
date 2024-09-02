public class TextSearchItem
{
	internal string SearchWord { get; set; }

	internal TextSearchOptions SearchOption { get; set; }

	public TextSearchItem(string searchWord, TextSearchOptions searchOption)
	{
		SearchWord = searchWord;
		SearchOption = searchOption;
	}
}
