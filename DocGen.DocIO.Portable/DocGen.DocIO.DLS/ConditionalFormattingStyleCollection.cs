using System;

namespace DocGen.DocIO.DLS;

public class ConditionalFormattingStyleCollection : CollectionImpl
{
	private new IWordDocument m_doc;

	internal new IWordDocument Document => m_doc;

	public ConditionalFormattingStyle this[ConditionalFormattingType formattingType]
	{
		get
		{
			ConditionalFormattingStyle result = null;
			for (int i = 0; i < base.InnerList.Count; i++)
			{
				if ((base.InnerList[i] as ConditionalFormattingStyle).ConditionalFormattingType == formattingType)
				{
					result = base.InnerList[i] as ConditionalFormattingStyle;
					break;
				}
			}
			return result;
		}
	}

	internal ConditionalFormattingStyleCollection(WordDocument doc)
		: base(doc, doc, 12)
	{
		m_doc = doc;
	}

	public ConditionalFormattingStyle Add(ConditionalFormattingType conditionalFormattingType)
	{
		if (base.InnerList.Count > 0)
		{
			int count = base.InnerList.Count;
			bool flag = false;
			for (int i = 0; i < count; i++)
			{
				if ((base.InnerList[i] as ConditionalFormattingStyle).ConditionalFormattingType == conditionalFormattingType)
				{
					flag = true;
					break;
				}
				flag = false;
			}
			if (!flag)
			{
				ConditionalFormattingStyle conditionalFormattingStyle = new ConditionalFormattingStyle(conditionalFormattingType, Document);
				base.InnerList.Add(conditionalFormattingStyle);
				return conditionalFormattingStyle;
			}
			throw new ArgumentException("The given style already exist in the collcetion");
		}
		ConditionalFormattingStyle conditionalFormattingStyle2 = new ConditionalFormattingStyle(conditionalFormattingType, Document);
		base.InnerList.Add(conditionalFormattingStyle2);
		return conditionalFormattingStyle2;
	}

	public void Remove(ConditionalFormattingStyle conditionalFormattingStyle)
	{
		base.InnerList.Remove(conditionalFormattingStyle);
	}
}
