using System;
using System.Collections.Generic;
using DocGen.DocIO.ReaderWriter;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.DLS;

internal class ListPropertiesConverter
{
	[ThreadStatic]
	private static bool isFirstInvalidListID;

	[ThreadStatic]
	private static string m_defaultListStyleName;

	public static void Export(WListFormat listFormat, SinglePropertyModifierArray papxSprms, WordReaderBase reader)
	{
		if (papxSprms.Contain(9828))
		{
			listFormat.IsFormattingChange = true;
		}
		short formatIndex = papxSprms[17931]?.ShortValue ?? (-1);
		int levelIndex = papxSprms[9738]?.ByteValue ?? 0;
		Export(formatIndex, levelIndex, listFormat, reader);
	}

	public static void Export(int formatIndex, int levelIndex, WListFormat listFormat, WordReaderBase reader)
	{
		if (formatIndex == 0 && !listFormat.IsFormattingChange)
		{
			listFormat.IsEmptyList = true;
		}
		ListInfo listInfo = reader.ListInfo;
		if (formatIndex > 0 && listInfo != null && listInfo.ListFormatOverrides.Count > 0)
		{
			if (listInfo.ListFormatOverrides.Count < formatIndex)
			{
				if (listFormat.IsFormattingChange)
				{
					UpdateNewListFormat(reader, listFormat);
				}
				return;
			}
			int listID = listInfo.ListFormatOverrides[formatIndex - 1].ListID;
			ListData listFromId = listInfo.ListFormats.GetListFromId(listID);
			ListFormatOverride listFormatOverride = listInfo.ListFormatOverrides[formatIndex - 1];
			if (listFormatOverride.Levels.Count > 0)
			{
				listFormat.LFOStyleName = ExportListFormatOverrides(formatIndex, reader, listFormatOverride, listFormat);
			}
			if (listFromId != null)
			{
				ExportListFormat(listFormat, reader, listID, listFromId, levelIndex);
			}
			else
			{
				UpdateListStyleForInvalidListId(listFormat, listID, levelIndex);
			}
			UpdateNewListFormat(reader, listFormat);
			if (listFormatOverride.Levels.Count > 0)
			{
				ListOverrideStyle listOverrideStyle = listFormat.Document.ListOverrides.FindByName(listFormat.LFOStyleName);
				if (listOverrideStyle != null && listFormat.CurrentListStyle != null)
				{
					listOverrideStyle.ListID = listFormat.CurrentListStyle.ListID;
					listOverrideStyle.listStyleName = listFormat.CurrentListStyle.Name;
				}
			}
		}
		else if (levelIndex > 0 && levelIndex < 9)
		{
			listFormat.ListLevelNumber = levelIndex;
		}
	}

	private static void UpdateNewListFormat(WordReaderBase reader, WListFormat listFormat)
	{
		if (reader.PAPXSprms.GetBoolean(9828, defValue: false))
		{
			listFormat.IsFormattingChange = false;
			ExportNewListFormat(listFormat, reader);
		}
	}

	private static void UpdateListStyleForInvalidListId(WListFormat listFormat, int id, int levelIndex)
	{
		if (!isFirstInvalidListID)
		{
			ListStyle listStyle = new ListStyle(listFormat.Document);
			listStyle.CreateDefListLevels(ListType.Numbered);
			foreach (WListLevel level in listStyle.Levels)
			{
				level.PatternType = ListPatternType.Arabic;
				level.NumberAlignment = ListNumberAlignment.Left;
				level.ParagraphFormat.SetPropertyValue(5, -36f);
			}
			UpdateListType(listStyle);
			UpdateStyleName(listStyle);
			listFormat.Document.ListStyles.Add(listStyle);
			listFormat.ListLevelNumber = levelIndex;
			listFormat.ApplyStyle(listStyle.Name);
			isFirstInvalidListID = true;
			m_defaultListStyleName = listStyle.Name;
			return;
		}
		listFormat.Document.ListStyleNames.Add(id.ToString(), m_defaultListStyleName);
		listFormat.ListLevelNumber = levelIndex;
		listFormat.ApplyStyle(listFormat.Document.ListStyles[id].Name);
		if (!AdapterListIDHolder.Instance.LfoStyleIDtoName.ContainsKey(id))
		{
			ListOverrideStyle listOverrideStyle = new ListOverrideStyle(listFormat.Document);
			listOverrideStyle.Name = "LfoStyle_" + Guid.NewGuid();
			listOverrideStyle.listStyleName = listFormat.Document.ListStyleNames[id.ToString()];
			listFormat.Document.ListOverrides.Add(listOverrideStyle);
			AdapterListIDHolder.Instance.LfoStyleIDtoName.Add(id, listOverrideStyle.Name);
			for (int i = 0; i < 9; i++)
			{
				OverrideLevelFormat overrideLevelFormat = new OverrideLevelFormat(listFormat.Document);
				listOverrideStyle.OverrideLevels.Add(i, overrideLevelFormat);
				overrideLevelFormat.StartAt = 1;
				overrideLevelFormat.OverrideStartAtValue = true;
			}
			listFormat.LFOStyleName = listOverrideStyle.listStyleName;
		}
		else
		{
			listFormat.LFOStyleName = AdapterListIDHolder.Instance.LfoStyleIDtoName[id];
		}
	}

	private static void UpdateListType(ListStyle listStyle)
	{
		listStyle.ListType = ListType.Bulleted;
		foreach (WListLevel level in listStyle.Levels)
		{
			if (level.PatternType != ListPatternType.Bullet)
			{
				listStyle.ListType = ListType.Numbered;
				break;
			}
		}
	}

	private static void UpdateStyleName(ListStyle listStyle)
	{
		if (listStyle.ListType == ListType.Numbered)
		{
			listStyle.Name = "Numbered_" + Guid.NewGuid();
		}
		else
		{
			listStyle.Name = "Bulleted_" + Guid.NewGuid();
		}
	}

	public static void Import(ListStyle lstStyle, ListData listFormat, WordStyleSheet styleSheet)
	{
		int i = 0;
		for (int count = lstStyle.Levels.Count; i < count; i++)
		{
			WListLevel dlsListLevel = lstStyle.Levels[i];
			ListLevel listLevel = new ListLevel();
			ImportToDocListLevel(dlsListLevel, listLevel, styleSheet, i);
			listFormat.Levels.Add(listLevel);
		}
	}

	private static void ExportListFormat(WListFormat listFormat, WordReaderBase reader, int id, ListData listData, int levelIndex)
	{
		if (levelIndex > listData.Levels.Count)
		{
			levelIndex = 0;
		}
		if (levelIndex < listData.Levels.Count)
		{
			CheckListCollection(id, listFormat);
			string styleName = ExportListStyle(listFormat, reader, id, listData);
			listFormat.ListLevelNumber = levelIndex;
			listFormat.ApplyStyle(styleName);
		}
	}

	private static void ExportNewListFormat(WListFormat listFormat, WordReaderBase reader)
	{
		int num = reader.PAPX.PropertyModifiers.GetNewSprm(17931, 9828)?.ShortValue ?? short.MaxValue;
		if (num != 32767 && num > 0)
		{
			ListInfo listInfo = reader.ListInfo;
			int listID = listInfo.ListFormatOverrides[num - 1].ListID;
			ListData listFromId = listInfo.ListFormats.GetListFromId(listID);
			ListFormatOverride listFormatOverride = listInfo.ListFormatOverrides[num - 1];
			if (listFormatOverride.Levels.Count > 0)
			{
				listFormat.LFOStyleName = ExportListFormatOverrides(num, reader, listFormatOverride, listFormat);
			}
			string styleName = ExportListStyle(listFormat, reader, listID, listFromId);
			listFormat.ApplyStyle(styleName);
		}
		int num2 = reader.PAPX.PropertyModifiers.GetNewSprm(9738, 9828)?.ByteValue ?? byte.MaxValue;
		if (num2 != 255)
		{
			listFormat.ListLevelNumber = num2;
		}
	}

	private static string ExportListStyle(WListFormat listFormat, WordReaderBase reader, int id, ListData listData)
	{
		if (!AdapterListIDHolder.Instance.ListStyleIDtoName.ContainsKey(id))
		{
			bool flag = IsBulletPatternType(listData.Levels);
			ListType listType = (flag ? ListType.Bulleted : ListType.Numbered);
			ListStyle listStyle = ListStyle.CreateEmptyListStyle(listFormat.Document, listType, listData.SimpleList);
			ExportToListLevelCollection(listData.Levels, listStyle.Levels, reader);
			listStyle.Name = (flag ? ("Bulleted_" + Guid.NewGuid()) : ("Numbered_" + Guid.NewGuid()));
			AdapterListIDHolder.Instance.ListStyleIDtoName.Add(id, listStyle.Name);
			listStyle.ListID = id;
			listFormat.Document.ListStyles.Add(listStyle);
			listStyle.IsHybrid = listData.IsHybridMultilevel;
			if (listData.SimpleList)
			{
				listStyle.IsSimple = true;
			}
			return listStyle.Name;
		}
		return AdapterListIDHolder.Instance.ListStyleIDtoName[id];
	}

	private static void ExportToListLevelCollection(ListLevels lstLevels, ListLevelCollection lstLevelCol, WordReaderBase reader)
	{
		int i = 0;
		for (int count = lstLevels.Count; i < count; i++)
		{
			ExportToDLSListLevel(lstLevels[i], lstLevelCol[i], reader, i);
		}
	}

	private static void ExportToDLSListLevel(ListLevel docListLevel, WListLevel dlsListLevel, WordReaderBase reader, int levelNumber)
	{
		dlsListLevel.PatternType = docListLevel.m_nfc;
		dlsListLevel.UsePrevLevelPattern = docListLevel.m_bPrev;
		dlsListLevel.StartAt = docListLevel.m_startAt;
		dlsListLevel.NumberAlignment = docListLevel.m_jc;
		dlsListLevel.FollowCharacter = docListLevel.m_ixchFollow;
		dlsListLevel.IsLegalStyleNumbering = docListLevel.m_bLegal;
		dlsListLevel.NoRestartByHigher = docListLevel.m_bNoRestart;
		dlsListLevel.Word6Legacy = docListLevel.m_bWord6;
		dlsListLevel.LegacySpace = docListLevel.m_dxaSpace;
		dlsListLevel.LegacyIndent = docListLevel.m_dxaIndent;
		char[] separator = new char[2]
		{
			'\\',
			Convert.ToChar(levelNumber)
		};
		string[] array = docListLevel.m_str.Split(separator);
		if (dlsListLevel.PatternType == ListPatternType.Bullet)
		{
			if (array.Length > 1)
			{
				dlsListLevel.BulletCharacter = array[0];
				dlsListLevel.BulletCharacter += array[1];
			}
			else
			{
				dlsListLevel.BulletCharacter = docListLevel.m_str;
			}
		}
		else
		{
			if (array.Length > 1)
			{
				dlsListLevel.NumberPrefix = array[0];
				dlsListLevel.NumberSuffix = array[1];
			}
			else if (array[0] == string.Empty)
			{
				string numberPrefix = (dlsListLevel.NumberSuffix = null);
				dlsListLevel.NumberPrefix = numberPrefix;
			}
			else
			{
				dlsListLevel.NumberPrefix = array[0];
				dlsListLevel.NoLevelText = true;
			}
			if (dlsListLevel.PatternType == ListPatternType.None)
			{
				dlsListLevel.BulletCharacter = docListLevel.m_str;
			}
		}
		CharacterPropertiesConverter.SprmsToFormat(docListLevel.m_chpx.PropertyModifiers, dlsListLevel.CharacterFormat, reader.StyleSheet, reader.SttbfRMarkAuthorNames, isNewPropertyHash: true);
		ParagraphPropertiesConverter.SprmsToFormat(docListLevel.m_papx.PropertyModifiers, dlsListLevel.ParagraphFormat, reader.SttbfRMarkAuthorNames, reader.StyleSheet);
	}

	internal static void ImportToDocListLevel(WListLevel dlsListLevel, ListLevel docListLevel, WordStyleSheet styleSheet, int levelIndex)
	{
		docListLevel.m_nfc = dlsListLevel.PatternType;
		docListLevel.m_bPrev = dlsListLevel.UsePrevLevelPattern;
		docListLevel.m_startAt = dlsListLevel.StartAt;
		docListLevel.m_jc = dlsListLevel.NumberAlignment;
		docListLevel.m_ixchFollow = dlsListLevel.FollowCharacter;
		docListLevel.m_bLegal = dlsListLevel.IsLegalStyleNumbering;
		docListLevel.m_bNoRestart = dlsListLevel.NoRestartByHigher;
		docListLevel.m_bWord6 = dlsListLevel.Word6Legacy;
		docListLevel.m_dxaSpace = dlsListLevel.LegacySpace;
		docListLevel.m_dxaIndent = dlsListLevel.LegacyIndent;
		bool flag = false;
		if (dlsListLevel.PatternType == ListPatternType.None && dlsListLevel.BulletCharacter != null && dlsListLevel.BulletCharacter.Length > 0 && dlsListLevel.ParaStyleName == null)
		{
			flag = true;
		}
		if (dlsListLevel.PatternType != ListPatternType.Bullet && !flag)
		{
			char c = Convert.ToChar(levelIndex);
			if (dlsListLevel.NumberPrefix == null && dlsListLevel.NumberSuffix == null)
			{
				docListLevel.m_str = string.Empty;
			}
			else
			{
				docListLevel.m_str = dlsListLevel.NumberPrefix;
				if (!dlsListLevel.NoLevelText)
				{
					docListLevel.m_str = docListLevel.m_str + c + dlsListLevel.NumberSuffix;
				}
			}
			CreateCharacterOffsets(docListLevel.m_str, ref docListLevel.m_rgbxchNums);
		}
		else
		{
			docListLevel.m_str = (string.IsNullOrEmpty(dlsListLevel.BulletCharacter) ? string.Empty : dlsListLevel.BulletCharacter);
		}
		if (docListLevel.m_chpx == null)
		{
			docListLevel.m_chpx = new CharacterPropertyException();
		}
		CharacterPropertiesConverter.FormatToSprms(dlsListLevel.CharacterFormat, docListLevel.m_chpx.PropertyModifiers, styleSheet);
		if (docListLevel.m_papx == null)
		{
			docListLevel.m_papx = new ParagraphPropertyException();
		}
		ParagraphPropertiesConverter.FormatToSprms(dlsListLevel.ParagraphFormat, docListLevel.m_papx.PropertyModifiers, styleSheet);
	}

	private static void CreateCharacterOffsets(string numStr, ref byte[] characterOffsets)
	{
		if (numStr == string.Empty)
		{
			return;
		}
		characterOffsets[0] = 1;
		int num = 0;
		for (int i = 0; i < 9; i++)
		{
			char[] separator = new char[2]
			{
				'\\',
				Convert.ToChar(i)
			};
			string[] array = numStr.Split(separator);
			if (array.Length > 1)
			{
				byte b = (byte)(array[0].Length + 1);
				if (num <= 1 || characterOffsets[num - 1] <= b)
				{
					characterOffsets[num] = b;
				}
				else
				{
					characterOffsets[num] = characterOffsets[num - 1];
					characterOffsets[num - 1] = b;
				}
				num++;
			}
		}
	}

	private static bool IsBulletPatternType(ListLevels levels)
	{
		bool result = true;
		int i = 0;
		for (int count = levels.Count; i < count; i++)
		{
			if (levels[i].m_nfc != ListPatternType.Bullet)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private static void CheckListCollection(int listId, WListFormat listFormat)
	{
		if (AdapterListIDHolder.Instance.ListStyleIDtoName.ContainsKey(listId))
		{
			string name = AdapterListIDHolder.Instance.ListStyleIDtoName[listId];
			if (listFormat.Document.ListStyles.FindByName(name) == null)
			{
				AdapterListIDHolder.Instance.ListStyleIDtoName.Remove(listId);
			}
		}
	}

	private static bool UseBaseListStyle(IWordReaderBase reader)
	{
		bool result = false;
		if (reader.PAPX.PropertyModifiers[17931] == null)
		{
			result = true;
		}
		return result;
	}

	private static string ExportListFormatOverrides(int lfoIndex, WordReaderBase reader, ListFormatOverride lstOverride, WListFormat listFormat)
	{
		if (!AdapterListIDHolder.Instance.LfoStyleIDtoName.ContainsKey(lfoIndex - 1))
		{
			ListOverrideStyle listOverrideStyle = new ListOverrideStyle(listFormat.Document);
			ExportListOverride(lstOverride, listOverrideStyle, reader, listFormat.Document);
			listOverrideStyle.Name = "LfoStyle_" + Guid.NewGuid();
			listFormat.Document.ListOverrides.Add(listOverrideStyle);
			AdapterListIDHolder.Instance.LfoStyleIDtoName.Add(lfoIndex - 1, listOverrideStyle.Name);
			return listOverrideStyle.Name;
		}
		return AdapterListIDHolder.Instance.LfoStyleIDtoName[lfoIndex - 1];
	}

	internal static void ExportListOverride(ListFormatOverride sourceLfo, ListOverrideStyle listOverrideStyle, WordReaderBase reader, IWordDocument doc)
	{
		listOverrideStyle.m_res1 = sourceLfo.m_res1;
		listOverrideStyle.m_res2 = sourceLfo.m_res2;
		for (int i = 0; i < sourceLfo.Levels.Count; i++)
		{
			ListFormatOverrideLevel listFormatOverrideLevel = sourceLfo.Levels[i] as ListFormatOverrideLevel;
			OverrideLevelFormat overrideLevelFormat = new OverrideLevelFormat((WordDocument)doc);
			overrideLevelFormat.OverrideFormatting = listFormatOverrideLevel.m_bFormatting;
			overrideLevelFormat.OverrideStartAtValue = listFormatOverrideLevel.m_bStartAt;
			overrideLevelFormat.StartAt = listFormatOverrideLevel.m_startAt;
			overrideLevelFormat.m_reserved1 = listFormatOverrideLevel.m_reserved1;
			overrideLevelFormat.m_reserved2 = listFormatOverrideLevel.m_reserved2;
			overrideLevelFormat.m_reserved3 = listFormatOverrideLevel.m_reserved3;
			if (listFormatOverrideLevel.m_lvl != null && listFormatOverrideLevel.m_bFormatting)
			{
				ExportToDLSListLevel(listFormatOverrideLevel.m_lvl, overrideLevelFormat.OverrideListLevel, reader, listFormatOverrideLevel.m_ilvl);
			}
			listOverrideStyle.OverrideLevels.Add(listFormatOverrideLevel.m_ilvl, overrideLevelFormat);
		}
	}

	internal static void ImportListOverride(ListOverrideStyle listOverrideStyle, ListFormatOverride lfo, WordStyleSheet styleSheet)
	{
		lfo.m_res1 = listOverrideStyle.m_res1;
		lfo.m_res2 = listOverrideStyle.m_res2;
		foreach (KeyValuePair<short, short> item in listOverrideStyle.OverrideLevels.LevelIndex)
		{
			OverrideLevelFormat overrideLevelFormat = listOverrideStyle.OverrideLevels[item.Key];
			ListFormatOverrideLevel listFormatOverrideLevel = new ListFormatOverrideLevel(overrideLevelFormat.OverrideFormatting);
			listFormatOverrideLevel.m_ilvl = item.Key;
			listFormatOverrideLevel.m_bFormatting = overrideLevelFormat.OverrideFormatting;
			listFormatOverrideLevel.m_bStartAt = overrideLevelFormat.OverrideStartAtValue;
			listFormatOverrideLevel.m_startAt = overrideLevelFormat.StartAt;
			listFormatOverrideLevel.m_reserved1 = overrideLevelFormat.m_reserved1;
			listFormatOverrideLevel.m_reserved2 = overrideLevelFormat.m_reserved2;
			listFormatOverrideLevel.m_reserved3 = overrideLevelFormat.m_reserved3;
			if (overrideLevelFormat.OverrideListLevel != null && overrideLevelFormat.OverrideFormatting)
			{
				ImportToDocListLevel(overrideLevelFormat.OverrideListLevel, listFormatOverrideLevel.m_lvl, styleSheet, item.Key);
			}
			lfo.Levels.Add(listFormatOverrideLevel);
		}
	}
}
