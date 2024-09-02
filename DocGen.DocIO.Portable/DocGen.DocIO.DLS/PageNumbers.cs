using System;

namespace DocGen.DocIO.DLS;

public class PageNumbers : FormatBase
{
	internal const int ChapterPageSeparatorKey = 51;

	internal const int HeadingLevelForChapterKey = 52;

	public ChapterPageSeparatorType ChapterPageSeparator
	{
		get
		{
			return (ChapterPageSeparatorType)GetPropertyValue(51);
		}
		set
		{
			SetPropertyValue(51, value);
		}
	}

	public HeadingLevel HeadingLevelForChapter
	{
		get
		{
			return (HeadingLevel)GetPropertyValue(52);
		}
		set
		{
			SetPropertyValue(52, value);
		}
	}

	internal PageNumbers Clone()
	{
		PageNumbers pageNumbers = new PageNumbers();
		pageNumbers.ImportContainer(this);
		pageNumbers.CopyProperties(this);
		return pageNumbers;
	}

	internal object GetPropertyValue(int propKey)
	{
		return base[propKey];
	}

	internal bool Compare(PageNumbers pageNumbers)
	{
		if (!Compare(52, pageNumbers))
		{
			return false;
		}
		if (!Compare(51, pageNumbers))
		{
			return false;
		}
		return true;
	}

	internal void SetPropertyValue(int propKey, object value)
	{
		base[propKey] = value;
		OnStateChange(this);
	}

	public PageNumbers()
	{
		SetPropertyValue(51, ChapterPageSeparatorType.Hyphen);
		SetPropertyValue(52, HeadingLevel.None);
	}

	protected override object GetDefValue(int key)
	{
		return key switch
		{
			52 => HeadingLevel.None, 
			51 => ChapterPageSeparatorType.Hyphen, 
			_ => throw new ArgumentException("key not found"), 
		};
	}
}
