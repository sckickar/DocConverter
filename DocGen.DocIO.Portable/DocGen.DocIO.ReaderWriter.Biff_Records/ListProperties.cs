using System.Collections.Generic;
using DocGen.DocIO.DLS;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

internal class ListProperties
{
	private ListInfo m_listInfo;

	private ParagraphPropertyException m_papx;

	private Dictionary<string, short> m_overrideStyles = new Dictionary<string, short>();

	private Dictionary<string, short> m_styles = new Dictionary<string, short>();

	internal Dictionary<string, short> StyleListIndexes => m_styles;

	internal ListProperties(ListInfo listInfo, ParagraphPropertyException papx)
	{
		m_listInfo = listInfo;
		m_papx = papx;
	}

	internal void Close()
	{
		m_listInfo = null;
		m_papx.Close();
		if (m_overrideStyles != null)
		{
			m_overrideStyles.Clear();
			m_overrideStyles = null;
		}
		if (m_styles != null)
		{
			m_styles.Clear();
			m_styles = null;
		}
	}

	internal void UpdatePAPX(ParagraphPropertyException papx)
	{
		m_papx = papx;
	}

	internal void ContinueCurrentList(ListData listData, WListFormat listFormat, WordStyleSheet styleSheet)
	{
		string lFOStyleName = listFormat.LFOStyleName;
		if (lFOStyleName != null)
		{
			lFOStyleName += listFormat.CustomStyleName;
			if (m_overrideStyles.ContainsKey(lFOStyleName))
			{
				m_papx.PropertyModifiers.SetShortValue(17931, m_overrideStyles[lFOStyleName]);
			}
			else
			{
				m_papx.PropertyModifiers.SetShortValue(17931, m_listInfo.ApplyLFO(listData, listFormat, styleSheet));
				m_overrideStyles.Add(lFOStyleName, m_papx.PropertyModifiers.GetShort(17931, -1));
			}
		}
		else if (m_styles.ContainsKey(listFormat.CustomStyleName))
		{
			m_papx.PropertyModifiers.SetShortValue(17931, m_styles[listFormat.CustomStyleName]);
		}
		m_papx.PropertyModifiers.SetByteValue(9738, (byte)listFormat.ListLevelNumber);
	}

	internal int ApplyList(ListData listData, WListFormat listFormat, WordStyleSheet styleSheet, bool applyToPap)
	{
		short num = m_listInfo.ApplyList(listData, listFormat, styleSheet);
		if (applyToPap)
		{
			m_papx.PropertyModifiers.SetShortValue(17931, num);
			m_papx.PropertyModifiers.SetByteValue(9738, (byte)listFormat.ListLevelNumber);
		}
		if (listFormat.LFOStyleName != null)
		{
			string key = listFormat.LFOStyleName + listFormat.CustomStyleName;
			if (!m_overrideStyles.ContainsKey(key))
			{
				m_overrideStyles.Add(key, num);
			}
		}
		if (!m_styles.ContainsKey(listFormat.CustomStyleName))
		{
			m_styles.Add(listFormat.CustomStyleName, num);
		}
		else
		{
			m_styles[listFormat.CustomStyleName] = num;
		}
		return num;
	}

	internal int ApplyBaseStyleList(ListData listData, WListFormat listFormat, WordStyleSheet styleSheet)
	{
		short num = m_listInfo.ApplyList(listData, listFormat, styleSheet);
		m_styles.Add(listFormat.CustomStyleName, num);
		return num;
	}
}
