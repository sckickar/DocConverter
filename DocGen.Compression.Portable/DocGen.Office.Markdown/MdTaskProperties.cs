namespace DocGen.Office.Markdown;

internal class MdTaskProperties
{
	private bool isChecked;

	private string checkedMarker = "- [X] ";

	private string uncheckedmarker = "- [ ] ";

	internal bool IsChecked
	{
		get
		{
			return isChecked;
		}
		set
		{
			isChecked = value;
		}
	}

	internal string CheckedMarker
	{
		get
		{
			return checkedMarker;
		}
		set
		{
			checkedMarker = value;
		}
	}

	internal string Uncheckedmarker
	{
		get
		{
			return uncheckedmarker;
		}
		set
		{
			uncheckedmarker = value;
		}
	}
}
