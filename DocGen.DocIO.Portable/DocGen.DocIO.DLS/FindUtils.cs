using System.Text.RegularExpressions;

namespace DocGen.DocIO.DLS;

internal class FindUtils
{
	internal const string DEF_WHOLE_WORD_BEFORE = "(?<=^|\\W|\\t)";

	internal const string DEF_WHOLE_WORD_AFTER = "(?=$|\\W|\\t)";

	internal const string DEF_WHOLE_WORD_EMPTY = "(?<=^|\\W|\\t)(?=$|\\W|\\t)";

	internal static bool IsPatternEmpty(Regex pattern)
	{
		string text = pattern.ToString();
		if (text.Length != 0)
		{
			return text == "(?<=^|\\W|\\t)(?=$|\\W|\\t)";
		}
		return true;
	}

	internal static Regex StringToRegex(string given, bool caseSensitive, bool wholeWord)
	{
		given = Regex.Escape(given);
		if (wholeWord)
		{
			given = "(?<=^|\\W|\\t)" + given + "(?=$|\\W|\\t)";
		}
		return new Regex(given, (!caseSensitive) ? RegexOptions.IgnoreCase : RegexOptions.None);
	}

	internal static int GetStartRangeIndex(WParagraph para, int start, out WTextRange tr)
	{
		tr = null;
		int result = 0;
		int i = 0;
		for (int count = para.Items.Count; i < count; i++)
		{
			int num = 0;
			int num2 = 0;
			if (para.Items[i] is Break && ((para.Items[i] as Break).BreakType == BreakType.LineBreak || (para.Items[i] as Break).BreakType == BreakType.TextWrappingBreak))
			{
				tr = (para.Items[i] as Break).TextRange;
				num = (para.Items[i] as Break).StartPos;
				num2 = (para.Items[i] as Break).EndPos - num;
			}
			else
			{
				tr = para.Items[i] as WTextRange;
				if (tr != null)
				{
					num = tr.StartPos;
					num2 = tr.TextLength;
				}
			}
			if (para.Items[i] is InlineContentControl inlineContentControl && inlineContentControl.EndPos >= start)
			{
				return GetStartRangeIndexInInlineContentControl(inlineContentControl.ParagraphItems, start, out tr);
			}
			if (tr != null && num + num2 >= start)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	internal static bool EnsureSameOwner(WTextRange startTextRange, WTextRange endTextRange)
	{
		Entity entity = ((startTextRange.Owner is Break) ? (startTextRange.Owner as Break).Owner : startTextRange.Owner);
		Entity entity2 = ((endTextRange.Owner is Break) ? (endTextRange.Owner as Break).Owner : endTextRange.Owner);
		int num = ((startTextRange.Owner is Break) ? (startTextRange.Owner as Break).Index : startTextRange.Index);
		int num2 = ((endTextRange.Owner is Break) ? (endTextRange.Owner as Break).Index : endTextRange.Index);
		ParagraphItemCollection paragraphItemCollection = ((entity is WParagraph) ? (entity as WParagraph).Items : (entity as InlineContentControl).ParagraphItems);
		if (entity == entity2)
		{
			for (int i = num; i < num2; i++)
			{
				if (paragraphItemCollection[i] is InlineContentControl)
				{
					return false;
				}
			}
		}
		return entity == entity2;
	}

	private static int GetStartRangeIndexInInlineContentControl(ParagraphItemCollection paragraphItems, int start, out WTextRange tr)
	{
		tr = null;
		int result = 0;
		int i = 0;
		for (int count = paragraphItems.Count; i < count; i++)
		{
			int num = 0;
			int num2 = 0;
			if (paragraphItems[i] is Break && ((paragraphItems[i] as Break).BreakType == BreakType.LineBreak || (paragraphItems[i] as Break).BreakType == BreakType.TextWrappingBreak))
			{
				tr = (paragraphItems[i] as Break).TextRange;
				num = (paragraphItems[i] as Break).StartPos;
				num2 = (paragraphItems[i] as Break).EndPos - num;
			}
			else
			{
				tr = paragraphItems[i] as WTextRange;
				if (tr != null)
				{
					num = tr.StartPos;
					num2 = tr.TextLength;
				}
			}
			if (tr != null && num + num2 >= start)
			{
				result = i;
				break;
			}
			if (paragraphItems[i] is InlineContentControl && paragraphItems[i].EndPos >= start)
			{
				return GetStartRangeIndexInInlineContentControl((paragraphItems[i] as InlineContentControl).ParagraphItems, start, out tr);
			}
		}
		return result;
	}
}
