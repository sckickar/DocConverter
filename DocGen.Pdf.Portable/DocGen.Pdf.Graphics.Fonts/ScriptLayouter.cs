using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DocGen.Pdf.Graphics.Fonts;

internal class ScriptLayouter
{
	private Dictionary<ScriptTags, string[]> m_opentTypeScriptTags = new Dictionary<ScriptTags, string[]>();

	private int[] arabicIsolatedCode = new int[7] { 1575, 1577, 1583, 1584, 1585, 1586, 1608 };

	internal ScriptLayouter()
	{
		m_opentTypeScriptTags.Add(ScriptTags.Arabic, new string[1] { "arab" });
		m_opentTypeScriptTags.Add(ScriptTags.Bengali, new string[2] { "bng2", "beng" });
		m_opentTypeScriptTags.Add(ScriptTags.Devanagari, new string[2] { "dev2", "deva" });
		m_opentTypeScriptTags.Add(ScriptTags.Gurmukhi, new string[2] { "gur2", "guru" });
		m_opentTypeScriptTags.Add(ScriptTags.Gujarati, new string[2] { "gjr2", "gujr" });
		m_opentTypeScriptTags.Add(ScriptTags.Kannada, new string[2] { "knd2", "knda" });
		m_opentTypeScriptTags.Add(ScriptTags.Khmer, new string[1] { "khmr" });
		m_opentTypeScriptTags.Add(ScriptTags.Latin, new string[1] { "latn" });
		m_opentTypeScriptTags.Add(ScriptTags.Malayalam, new string[2] { "mlm2", "mlym" });
		m_opentTypeScriptTags.Add(ScriptTags.Oriya, new string[2] { "ory2", "orya" });
		m_opentTypeScriptTags.Add(ScriptTags.Tamil, new string[2] { "tml2", "taml" });
		m_opentTypeScriptTags.Add(ScriptTags.Telugu, new string[2] { "tel2", "telu" });
		m_opentTypeScriptTags.Add(ScriptTags.Thai, new string[1] { "thai" });
	}

	private IList<IndicGlyphInfoList> GetIndicCharacterGroup(OtfGlyphInfoList glyphList)
	{
		IndicCharacterClassifier indicCharacterClassifier = new IndicCharacterClassifier();
		string clustersPattern = indicCharacterClassifier.GetClustersPattern(glyphList);
		Match match = new Regex(indicCharacterClassifier.Pattern).Match("X" + clustersPattern);
		IList<IndicGlyphInfoList> list = new List<IndicGlyphInfoList>();
		while (match.Success)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = glyphList.Start + match.Index - 1; i < glyphList.Start + (match.Index + match.Length) - 1; i++)
			{
				if (stringBuilder == null)
				{
					break;
				}
				OtfGlyphInfo otfGlyphInfo = glyphList.Glyphs[i];
				if (otfGlyphInfo.CharCode > -1)
				{
					stringBuilder.Append(char.ConvertFromUtf32(otfGlyphInfo.CharCode));
				}
				else
				{
					stringBuilder = null;
				}
			}
			list.Add(new IndicGlyphInfoList(glyphList, glyphList.Start + match.Index - 1, glyphList.Start + (match.Index + match.Length) - 1, stringBuilder?.ToString()));
			match = match.NextMatch();
		}
		return list;
	}

	internal bool DoLayout(UnicodeTrueTypeFont font, OtfGlyphInfoList glyphInfoList, ScriptTags script)
	{
		GSUBTable gSUB = font.TtfReader.GSUB;
		string[] value = null;
		m_opentTypeScriptTags.TryGetValue(script, out value);
		if (value != null && gSUB != null)
		{
			string otfScriptTag = null;
			LanguageRecord languageRecord = null;
			for (int i = 0; i < value.Length; i++)
			{
				languageRecord = gSUB.LanguageRecord(value[i]);
				if (languageRecord != null)
				{
					otfScriptTag = value[i];
					break;
				}
			}
			if (languageRecord != null)
			{
				bool result = false;
				switch (script)
				{
				case ScriptTags.Bengali:
				case ScriptTags.Devanagari:
				case ScriptTags.Gujarati:
				case ScriptTags.Gurmukhi:
				case ScriptTags.Khmer:
				case ScriptTags.Kannada:
				case ScriptTags.Malayalam:
				case ScriptTags.Oriya:
				case ScriptTags.Tamil:
				case ScriptTags.Telugu:
					return DoIndicScriptShape(font, glyphInfoList, script, gSUB, otfScriptTag, languageRecord);
				case ScriptTags.Thai:
					return DoThaiScriptShape(font, glyphInfoList, gSUB, otfScriptTag, languageRecord);
				case ScriptTags.Arabic:
					return DoArabicScriptShape(font, glyphInfoList, gSUB, otfScriptTag, languageRecord);
				default:
					return result;
				}
			}
			return false;
		}
		return false;
	}

	private bool DoThaiScriptShape(UnicodeTrueTypeFont font, OtfGlyphInfoList glyphList, GSUBTable gsubTable, string otfScriptTag, LanguageRecord languageRecord)
	{
		glyphList.IsThaiShaping = true;
		bool flag = false;
		IDictionary<string, IList<LookupTable>> lookupTables = GetLookupTables(font, gsubTable, otfScriptTag, languageRecord);
		IList<LookupTable> lookupTable = GetLookupTable(lookupTables, "ccmp");
		flag = ReplaceGlyphs(lookupTable, glyphList);
		lookupTable = GetLookupTable(lookupTables, "mark");
		if (ReplaceGlyphs(lookupTable, glyphList))
		{
			flag = true;
		}
		lookupTable = GetLookupTable(lookupTables, "mkmk");
		if (ReplaceGlyphs(lookupTable, glyphList))
		{
			flag = true;
		}
		glyphList.IsThaiShaping = false;
		return flag;
	}

	private bool DoArabicScriptShape(UnicodeTrueTypeFont font, OtfGlyphInfoList glyphList, GSUBTable gsubTable, string otfScriptTag, LanguageRecord languageRecord)
	{
		bool flag = false;
		IDictionary<string, IList<LookupTable>> lookupTables = GetLookupTables(font, gsubTable, otfScriptTag, languageRecord);
		IList<LookupTable> lookupTable = GetLookupTable(lookupTables, "init");
		IList<LookupTable> lookupTable2 = GetLookupTable(lookupTables, "medi");
		IList<LookupTable> lookupTable3 = GetLookupTable(lookupTables, "fina");
		IList<LookupTable> lookupTable4 = GetLookupTable(lookupTables, "rlig");
		glyphList.IsThaiShaping = true;
		IList<LookupTable> lookupTable5 = GetLookupTable(lookupTables, "ccmp");
		flag = ReplaceGlyphs(lookupTable5, glyphList);
		glyphList.IsThaiShaping = false;
		IList<int> list = BreakArabicLineIntoWords(glyphList, lookupTable, lookupTable2, lookupTable3, font.TtfReader.GDEF);
		if (DoInitMediFinaShaping(glyphList, lookupTable, lookupTable2, lookupTable3, font.TtfReader.GDEF, list))
		{
			flag = true;
		}
		int start = glyphList.Start;
		int end = glyphList.End;
		int num = 0;
		for (int i = 0; i < list.Count; i += 2)
		{
			int start2 = list[i] + num;
			int num2 = list[i + 1] + num;
			glyphList.Start = start2;
			glyphList.End = num2;
			if (DoLigatureFeature(glyphList, lookupTable4))
			{
				flag = true;
			}
			num += glyphList.End - num2;
			list[i] = glyphList.Start;
			list[i + 1] = glyphList.End;
		}
		end += num;
		string[] array = new string[3] { "curs", "mark", "mkmk" };
		for (int j = 0; j < list.Count; j += 2)
		{
			glyphList.Start = list[j];
			glyphList.End = list[j + 1];
			string[] array2 = array;
			foreach (string tag in array2)
			{
				if (ReplaceGlyphs(GetLookupTable(lookupTables, tag), glyphList))
				{
					flag = true;
				}
			}
		}
		glyphList.Start = start;
		glyphList.End = end;
		return flag;
	}

	private IList<LookupTable> GetLookupTable(IDictionary<string, IList<LookupTable>> lookupTables, string tag)
	{
		lookupTables.TryGetValue(tag, out IList<LookupTable> value);
		return value;
	}

	private ScriptFeature[] GetScriptFeatures(int type)
	{
		List<ScriptFeature> list = new List<ScriptFeature>();
		switch (type)
		{
		case 1:
			list.Add(new ScriptFeature("nukt", complete: true, 1));
			list.Add(new ScriptFeature("akhn", complete: true, 2));
			list.Add(new ScriptFeature("rphf", complete: false, 4));
			list.Add(new ScriptFeature("rkrf", complete: true, 8));
			list.Add(new ScriptFeature("pref", complete: false, 16));
			list.Add(new ScriptFeature("blwf", complete: false, 32));
			list.Add(new ScriptFeature("abvf", complete: false, 64));
			list.Add(new ScriptFeature("half", complete: false, 128));
			list.Add(new ScriptFeature("pstf", complete: false, 256));
			list.Add(new ScriptFeature("vatu", complete: true, 512));
			list.Add(new ScriptFeature("cjct", complete: true, 1024));
			list.Add(new ScriptFeature("cfar", complete: false, 2048));
			break;
		case 2:
			list.Add(new ScriptFeature("init", complete: false, 4096));
			list.Add(new ScriptFeature("pres", complete: true, 8192));
			list.Add(new ScriptFeature("abvs", complete: true, 16384));
			list.Add(new ScriptFeature("blws", complete: true, 32768));
			list.Add(new ScriptFeature("psts", complete: true, 65536));
			list.Add(new ScriptFeature("haln", complete: true, 131072));
			break;
		case 3:
			list.Add(new ScriptFeature("dist", complete: true, 262144));
			list.Add(new ScriptFeature("abvm", complete: true, 524288));
			list.Add(new ScriptFeature("blwm", complete: true, 1048576));
			break;
		}
		return list.ToArray();
	}

	private bool DoIndicScriptShape(UnicodeTrueTypeFont font, OtfGlyphInfoList glyphList, ScriptTags script, GSUBTable gsubTable, string otfScriptTag, LanguageRecord gsubLanguageRecord)
	{
		bool result = false;
		NormalizeGlyphList(font, glyphList);
		IndicScriptLayouter indicScriptLayouter = new IndicScriptLayouter();
		indicScriptLayouter.ReplaceDefaultGlyphs(font, glyphList);
		IList<IndicGlyphInfoList> indicCharacterGroup = GetIndicCharacterGroup(glyphList);
		IDictionary<string, IList<LookupTable>> lookupTables = GetLookupTables(font, gsubTable, otfScriptTag, gsubLanguageRecord);
		if (indicCharacterGroup != null && indicCharacterGroup.Count > 0)
		{
			ScriptFeature[] scriptFeatures = GetScriptFeatures(1);
			ScriptFeature[] scriptFeatures2 = GetScriptFeatures(2);
			ScriptFeature[] scriptFeatures3 = GetScriptFeatures(3);
			IndicScript indicScript = GetIndicScript(script);
			bool oldScript = indicScript.OldVersion && !otfScriptTag.EndsWith("2");
			IList<LookupTable> lookupTable = GetLookupTable(lookupTables, "locl");
			IList<LookupTable> lookupTable2 = GetLookupTable(lookupTables, "ccmp");
			IList<LookupTable> lookupTable3 = GetLookupTable(lookupTables, "blwf");
			IList<LookupTable> lookupTable4 = GetLookupTable(lookupTables, "pstf");
			IList<LookupTable> lookupTable5 = GetLookupTable(lookupTables, "pref");
			IList<LookupTable> lookupTable6 = GetLookupTable(lookupTables, "rphf");
			for (int i = 0; i < indicCharacterGroup.Count; i++)
			{
				IndicGlyphInfoList indicGlyphInfoList = indicCharacterGroup[i];
				ReplaceGlyphs(lookupTable, indicGlyphInfoList);
				ReplaceGlyphs(lookupTable2, indicGlyphInfoList);
				indicScriptLayouter.SetPosition(font, indicGlyphInfoList, indicScript, lookupTable3, lookupTable4, lookupTable5);
				indicScriptLayouter.Reorder(indicGlyphInfoList, indicScript, lookupTable6, lookupTable5, oldScript, script);
				ApplyScriptFeatures(scriptFeatures, indicGlyphInfoList, lookupTables);
				indicScriptLayouter.Reorder(indicGlyphInfoList, indicScript, script);
				indicGlyphInfoList = (indicCharacterGroup[i] = ApplyScriptFeatures(indicGlyphInfoList, scriptFeatures2, lookupTables));
				IList<LookupTable> lookupTable7 = GetLookupTable(lookupTables, "calt");
				ReplaceGlyphs(lookupTable7, indicGlyphInfoList);
				lookupTable7 = GetLookupTable(lookupTables, "clig");
				ReplaceGlyphs(lookupTable7, indicGlyphInfoList);
				ApplyScriptFeatures(scriptFeatures3, indicGlyphInfoList, lookupTables);
			}
			OtfGlyphInfoList otfGlyphInfoList = new OtfGlyphInfoList();
			for (int j = 0; j < glyphList.Start; j++)
			{
				otfGlyphInfoList.Glyphs.Add(glyphList.Glyphs[j]);
				otfGlyphInfoList.Text.Add(null);
			}
			int num = glyphList.Start;
			foreach (IndicGlyphInfoList item in indicCharacterGroup)
			{
				if (item.GlyphInfoStart > num)
				{
					for (int k = num; k < item.GlyphInfoStart; k++)
					{
						otfGlyphInfoList.Glyphs.Add(glyphList.Glyphs[k]);
						otfGlyphInfoList.Text.Add(null);
					}
				}
				int count = otfGlyphInfoList.Glyphs.Count;
				for (int l = 0; l < item.Glyphs.Count; l++)
				{
					otfGlyphInfoList.Glyphs.Add(item.Glyphs[l]);
					otfGlyphInfoList.Text.Add(null);
				}
				otfGlyphInfoList.SetText(count, otfGlyphInfoList.Glyphs.Count, item.GetText());
				num = item.GlyphInfoEnd;
			}
			for (int m = num; m < glyphList.End; m++)
			{
				otfGlyphInfoList.Glyphs.Add(glyphList.Glyphs[m]);
				otfGlyphInfoList.Text.Add(null);
			}
			for (int n = glyphList.End; n < glyphList.Glyphs.Count; n++)
			{
				otfGlyphInfoList.Glyphs.Add(glyphList.Glyphs[n]);
				otfGlyphInfoList.Text.Add(null);
			}
			otfGlyphInfoList.End = otfGlyphInfoList.Glyphs.Count;
			glyphList.ReplaceContent(otfGlyphInfoList);
			result = true;
		}
		return result;
	}

	private void ApplyScriptFeatures(ScriptFeature[] scriptFeatures, IndicGlyphInfoList indicGlyphInfo, IDictionary<string, IList<LookupTable>> lookupTables)
	{
		for (int i = 0; i < scriptFeatures.Length; i++)
		{
			IList<LookupTable> lookupTable = GetLookupTable(lookupTables, scriptFeatures[i].Name);
			ReplaceGlyphs(lookupTable, scriptFeatures[i], indicGlyphInfo);
		}
	}

	private IDictionary<string, IList<LookupTable>> GetLookupTables(UnicodeTrueTypeFont font, GSUBTable gsubTable, string otfScriptTag, LanguageRecord languageRecord)
	{
		IDictionary<string, IList<LookupTable>> dictionary = new Dictionary<string, IList<LookupTable>>();
		int[] records = languageRecord.Records;
		foreach (int index in records)
		{
			FeatureRecord featureRecord = gsubTable.OTFeature.Records[index];
			IList<LookupTable> lookups = gsubTable.GetLookups(new FeatureRecord[1] { featureRecord });
			if (dictionary.ContainsKey(featureRecord.Tag))
			{
				dictionary[featureRecord.Tag] = lookups;
			}
			else
			{
				dictionary.Add(featureRecord.Tag, lookups);
			}
		}
		GPOSTable gPOS = font.TtfReader.GPOS;
		if (gPOS != null)
		{
			LanguageRecord languageRecord2 = gPOS.LanguageRecord(otfScriptTag);
			if (languageRecord2 != null)
			{
				records = languageRecord2.Records;
				foreach (int index2 in records)
				{
					FeatureRecord featureRecord2 = gPOS.OTFeature.Records[index2];
					IList<LookupTable> lookups2 = gPOS.GetLookups(new FeatureRecord[1] { featureRecord2 });
					if (dictionary.ContainsKey(featureRecord2.Tag))
					{
						dictionary[featureRecord2.Tag] = lookups2;
					}
					else
					{
						dictionary.Add(featureRecord2.Tag, lookups2);
					}
				}
			}
		}
		return dictionary;
	}

	private void NormalizeGlyphList(UnicodeTrueTypeFont font, OtfGlyphInfoList glyphList)
	{
		for (int i = glyphList.Start; i < glyphList.End; i++)
		{
			OtfGlyphInfo otfGlyphInfo = glyphList.Glyphs[i];
			string text = null;
			if (otfGlyphInfo.CharCode > -1)
			{
				text = char.ConvertFromUtf32(otfGlyphInfo.CharCode);
			}
			else if (otfGlyphInfo.Characters != null)
			{
				text = new string(otfGlyphInfo.Characters);
			}
			if (text == null)
			{
				continue;
			}
			string text2 = text.Normalize(NormalizationForm.FormD);
			if (text.Equals(text2))
			{
				continue;
			}
			glyphList.Index = i;
			int num = 0;
			List<int> list = new List<int>();
			while (num < text2.Length)
			{
				if (char.IsSurrogatePair(text2, num))
				{
					list.Add(char.ConvertToUtf32(text2, num));
					num += 2;
				}
				else
				{
					list.Add(text2[num]);
					num++;
				}
			}
			int[] array = list.ToArray();
			int[] array2 = new int[array.Length];
			for (int j = 0; j < array.Length; j++)
			{
				array2[j] = font.TtfReader.ReadGlyph(array[j], isOpenType: false).Index;
			}
			glyphList.CombineAlternateGlyphs(font.TtfReader.GSUB, array2);
		}
	}

	private bool ReplaceGlyphs(IList<LookupTable> lookupTables, OtfGlyphInfoList glyphList)
	{
		bool result = false;
		if (lookupTables != null)
		{
			foreach (LookupTable lookupTable in lookupTables)
			{
				if (lookupTable != null && lookupTable.ReplaceGlyphs(glyphList))
				{
					result = true;
				}
			}
		}
		return result;
	}

	private bool ReplaceGlyphsOne(IList<LookupTable> lookupTables, OtfGlyphInfoList glyphList)
	{
		bool result = false;
		if (lookupTables != null)
		{
			foreach (LookupTable lookupTable in lookupTables)
			{
				if (lookupTable != null && lookupTable.ReplaceGlyph(glyphList))
				{
					result = true;
				}
			}
		}
		return result;
	}

	private bool ReplaceGlyphs(IList<LookupTable> lookupTables, ScriptFeature scriptFeature, IndicGlyphInfoList glyphInfoList)
	{
		bool result = false;
		if (lookupTables != null)
		{
			foreach (LookupTable lookupTable in lookupTables)
			{
				if (lookupTable == null)
				{
					continue;
				}
				if (scriptFeature.IsComplete)
				{
					if (lookupTable.ReplaceGlyphs(glyphInfoList))
					{
						result = true;
					}
					continue;
				}
				int num = glyphInfoList.End;
				int start;
				int i = (start = glyphInfoList.Start);
				while (i < num)
				{
					for (; i < num && (glyphInfoList[i].Mask & scriptFeature.Mask) == 0; i++)
					{
					}
					if (i < num)
					{
						int j;
						for (j = i + 1; j < num && (glyphInfoList[j].Mask & scriptFeature.Mask) != 0; j++)
						{
						}
						glyphInfoList.Start = i;
						glyphInfoList.End = j;
						if (lookupTable.ReplaceGlyphs(glyphInfoList))
						{
							result = true;
						}
						num += glyphInfoList.End - j;
						i = glyphInfoList.End;
						glyphInfoList.Start = start;
						glyphInfoList.End = num;
					}
				}
			}
		}
		return result;
	}

	private IndicGlyphInfoList ApplyScriptFeatures(IndicGlyphInfoList indicGlyphInfoList, ScriptFeature[] scriptFeatures, IDictionary<string, IList<LookupTable>> lookupTables)
	{
		List<IndicGlyphItem> list = new List<IndicGlyphItem>();
		List<IndicGlyphItem> list2 = new List<IndicGlyphItem>();
		list.Add(new IndicGlyphItem(indicGlyphInfoList, 0));
		IndicGlyphItem indicGlyphItem = null;
		while (list.Count > 0)
		{
			IndicGlyphItem indicGlyphItem2 = list[0];
			list.RemoveAt(0);
			IndicGlyphInfoList glyphList = indicGlyphItem2.GlyphList;
			bool flag = false;
			foreach (ScriptFeature scriptFeature in scriptFeatures)
			{
				IndicGlyphInfoList glyphInfoList = (IndicGlyphInfoList)glyphList.SubSet(glyphList.Start, glyphList.End);
				IList<LookupTable> value = null;
				lookupTables.TryGetValue(scriptFeature.Name, out value);
				if (!ReplaceGlyphs(value, scriptFeature, glyphInfoList))
				{
					continue;
				}
				flag = true;
				IndicGlyphItem item = new IndicGlyphItem(glyphInfoList, indicGlyphItem2.Position + 1);
				if (!list2.Contains(item))
				{
					list2.Add(item);
					if (!list.Contains(item))
					{
						list.Add(item);
					}
				}
			}
			if (!flag && (indicGlyphItem == null || indicGlyphItem.Position < indicGlyphItem2.Position || (indicGlyphItem.Position == indicGlyphItem2.Position && indicGlyphItem.Length > indicGlyphItem2.Length)))
			{
				indicGlyphItem = indicGlyphItem2;
			}
		}
		if (indicGlyphItem != null)
		{
			return indicGlyphItem.GlyphList;
		}
		return indicGlyphInfoList;
	}

	private IndicScript GetIndicScript(ScriptTags script)
	{
		IndicScript result = default(IndicScript);
		switch (script)
		{
		case ScriptTags.Devanagari:
			result.OldVersion = true;
			result.InitialChar = 2381;
			result.Position = 2;
			result.RephPosition = 10;
			result.RephMode = 0;
			result.BlwfMode = 0;
			result.Length = 2;
			break;
		case ScriptTags.Bengali:
			result.OldVersion = true;
			result.InitialChar = 2509;
			result.Position = 2;
			result.RephPosition = 9;
			result.RephMode = 0;
			result.BlwfMode = 0;
			result.Length = 2;
			break;
		case ScriptTags.Gurmukhi:
			result.OldVersion = true;
			result.InitialChar = 2637;
			result.Position = 2;
			result.RephPosition = 7;
			result.RephMode = 0;
			result.BlwfMode = 0;
			result.Length = 2;
			break;
		case ScriptTags.Gujarati:
			result.OldVersion = true;
			result.InitialChar = 2765;
			result.Position = 2;
			result.RephPosition = 10;
			result.RephMode = 0;
			result.BlwfMode = 0;
			result.Length = 2;
			break;
		case ScriptTags.Oriya:
			result.OldVersion = true;
			result.InitialChar = 2893;
			result.Position = 2;
			result.RephPosition = 5;
			result.RephMode = 0;
			result.BlwfMode = 0;
			result.Length = 2;
			break;
		case ScriptTags.Tamil:
			result.OldVersion = true;
			result.InitialChar = 3021;
			result.Position = 2;
			result.RephPosition = 12;
			result.RephMode = 0;
			result.BlwfMode = 0;
			result.Length = 2;
			break;
		case ScriptTags.Telugu:
			result.OldVersion = true;
			result.InitialChar = 3149;
			result.Position = 2;
			result.RephPosition = 12;
			result.RephMode = 1;
			result.BlwfMode = 1;
			result.Length = 2;
			break;
		case ScriptTags.Kannada:
			result.OldVersion = true;
			result.InitialChar = 3277;
			result.Position = 2;
			result.RephPosition = 12;
			result.RephMode = 0;
			result.BlwfMode = 1;
			result.Length = 2;
			break;
		case ScriptTags.Malayalam:
			result.OldVersion = true;
			result.InitialChar = 3405;
			result.Position = 2;
			result.RephPosition = 5;
			result.RephMode = 3;
			result.BlwfMode = 0;
			result.Length = 2;
			break;
		case ScriptTags.Khmer:
			result.OldVersion = true;
			result.InitialChar = 6098;
			result.Position = 0;
			result.RephPosition = 1;
			result.RephMode = 2;
			result.BlwfMode = 0;
			result.Length = 2;
			break;
		default:
			throw new Exception("Invalid script");
		}
		return result;
	}

	private IList<int> BreakArabicLineIntoWords(OtfGlyphInfoList glyphList, IList<LookupTable> initial, IList<LookupTable> medial, IList<LookupTable> terminal, GDEFTable gDEFTable)
	{
		IList<int> list = new List<int>();
		bool flag = false;
		char c = 'ـ';
		bool flag2 = false;
		LanguageUtil languageUtil = new LanguageUtil();
		for (int i = 0; i < glyphList.Glyphs.Count; i++)
		{
			OtfGlyphInfo otfGlyphInfo = glyphList.Glyphs[i];
			bool flag3 = gDEFTable.GlyphCdefTable.IsMark(otfGlyphInfo.Index);
			bool flag4 = i + 1 < glyphList.End && gDEFTable.GlyphCdefTable.IsMark(glyphList.Glyphs[i + 1].Index);
			bool flag5 = ScriptTags.Arabic.Equals(languageUtil.GetLanguage((char)otfGlyphInfo.CharCode)) || flag3 || c == otfGlyphInfo.CharCode;
			if (flag)
			{
				bool flag6 = !flag3 && !IsMedialLetter(new string(otfGlyphInfo.Characters));
				if ((!flag4 && (flag6 || flag2)) || !flag5)
				{
					if (!flag5)
					{
						list.Add(i);
					}
					else
					{
						list.Add(i + 1);
					}
					flag = false;
					flag2 = false;
				}
				else if (!flag2)
				{
					flag2 = flag6;
				}
			}
			else if (flag5 && IsMedialLetter(new string(otfGlyphInfo.Characters)))
			{
				list.Add(i);
				flag = true;
			}
		}
		if (list.Count % 2 != 0)
		{
			list.Add(glyphList.Glyphs.Count);
		}
		return list;
	}

	private bool IsMedialLetter(string word)
	{
		string text = word.Normalize(NormalizationForm.FormKD);
		for (int i = 0; i < text.Length; i++)
		{
			if (Array.BinarySearch(arabicIsolatedCode, text[i]) >= 0)
			{
				return false;
			}
		}
		return true;
	}

	private bool DoInitMediFinaShaping(OtfGlyphInfoList glyphLine, IList<LookupTable> initial, IList<LookupTable> medial, IList<LookupTable> final, GDEFTable gDEFTable, IList<int> words)
	{
		if (initial == null || medial == null || final == null)
		{
			return false;
		}
		bool result = false;
		for (int i = 0; i < words.Count; i += 2)
		{
			int num = words[i];
			int num2 = words[i + 1];
			IList<int> list = new List<int>();
			for (int j = num; j < num2; j++)
			{
				if (!gDEFTable.GlyphCdefTable.IsMark(glyphLine.Glyphs[j].Index))
				{
					list.Add(j);
				}
			}
			if (list.Count > 1)
			{
				glyphLine.Index = list[0];
				if (ReplaceGlyphsOne(initial, glyphLine))
				{
					result = true;
				}
			}
			for (int k = 1; k < list.Count - 1; k++)
			{
				glyphLine.Index = list[k];
				if (ReplaceGlyphsOne(medial, glyphLine))
				{
					result = true;
				}
			}
			if (list.Count > 1)
			{
				glyphLine.Index = list[list.Count - 1];
				if (ReplaceGlyphsOne(final, glyphLine))
				{
					result = true;
				}
			}
		}
		return result;
	}

	private bool DoLigatureFeature(OtfGlyphInfoList glyphLine, IList<LookupTable> ligature)
	{
		bool result = false;
		if (glyphLine != null && ligature != null)
		{
			foreach (LookupTable item in ligature)
			{
				if (item != null && item.ReplaceGlyphs(glyphLine))
				{
					result = true;
				}
			}
		}
		return result;
	}
}
