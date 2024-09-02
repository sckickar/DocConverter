using System;
using System.Collections.Generic;

namespace DocGen.Office;

internal class IndicScriptLayouter
{
	private Dictionary<int, int[]> m_defaultGlyphs = new Dictionary<int, int[]>();

	internal IndicScriptLayouter()
	{
		m_defaultGlyphs.Add(6078, new int[2] { 6081, 6078 });
		m_defaultGlyphs.Add(6079, new int[2] { 6081, 6079 });
		m_defaultGlyphs.Add(6080, new int[2] { 6081, 6080 });
		m_defaultGlyphs.Add(6084, new int[2] { 6081, 6084 });
		m_defaultGlyphs.Add(6085, new int[2] { 6081, 6085 });
	}

	internal void ReplaceDefaultGlyphs(UnicodeTrueTypeFont font, OtfGlyphInfoList glyphList)
	{
		int index = glyphList.Index;
		for (int i = glyphList.Start; i < glyphList.End; i++)
		{
			if (glyphList.Glyphs[i].CharCode > -1 && m_defaultGlyphs.ContainsKey(glyphList.Glyphs[i].CharCode))
			{
				Replace(glyphList, i, font, m_defaultGlyphs[glyphList.Glyphs[i].CharCode]);
				i++;
			}
		}
		glyphList.Index = index;
	}

	internal void SetPosition(UnicodeTrueTypeFont font, IndicGlyphInfoList indicGlyphList, IndicScript iScript, IList<LookupTable> belowBaseForm, IList<LookupTable> postBaseForm, IList<LookupTable> preBaseForm)
	{
		if (iScript.Position != 2)
		{
			return;
		}
		OtfGlyphInfo glyph = GetGlyph(font, iScript);
		if (glyph == null)
		{
			return;
		}
		for (int i = 0; i < indicGlyphList.Glyphs.Count; i++)
		{
			if (indicGlyphList[i].Position == 4)
			{
				OtfGlyphInfo glyphInfo = indicGlyphList[i];
				indicGlyphList[i].Position = GetPosition(glyphInfo, glyph, iScript, belowBaseForm, postBaseForm, preBaseForm);
			}
		}
	}

	internal void Reorder(IndicGlyphInfoList glyphInfoList, IndicScript indicScript, IList<LookupTable> rephForm, IList<LookupTable> prebaseForm, bool oldScript, ScriptTags scriptTag)
	{
		int i;
		int num = (i = 0);
		int num2;
		int num3 = (num2 = glyphInfoList.Glyphs.Count);
		bool flag = false;
		if (indicScript.RephPosition != 1 && glyphInfoList.Glyphs.Count >= 3 && ((indicScript.RephMode == 0 && !IsCombined(glyphInfoList[num + 2].Group)) || (indicScript.RephMode == 1 && glyphInfoList[num + 2].Group == 6)))
		{
			List<OtfGlyphInfo> list = new List<OtfGlyphInfo>();
			list.Add(glyphInfoList[0]);
			list.Add(glyphInfoList[1]);
			list.Add((indicScript.RephMode == 1) ? glyphInfoList[2] : null);
			if (Replace(rephForm, list, 2) || (indicScript.RephMode == 1 && Replace(rephForm, list, 3)))
			{
				for (i += 2; i < num3 && IsCombined(glyphInfoList[i].Group); i++)
				{
				}
				num2 = num;
				flag = true;
			}
		}
		else if (indicScript.RephMode == 3 && glyphInfoList[num].Group == 15)
		{
			for (i++; i < num3 && IsCombined(glyphInfoList[i].Group); i++)
			{
			}
			num2 = num;
			flag = true;
		}
		if (indicScript.Position == 0)
		{
			num2 = num;
			for (int j = num2 + 1; j < num3; j++)
			{
				if (IsConsonant(glyphInfoList[j].Group))
				{
					glyphInfoList[j].Position = 8;
				}
			}
		}
		else if (indicScript.Position == 1)
		{
			if (!flag)
			{
				num2 = i;
			}
			for (int k = i; k < num3; k++)
			{
				if (IsConsonant(glyphInfoList[k].Group))
				{
					if (i < k && glyphInfoList[k - 1].Group == 6)
					{
						break;
					}
					num2 = k;
				}
			}
			for (int l = num2 + 1; l < num3; l++)
			{
				if (IsConsonant(glyphInfoList[l].Group))
				{
					glyphInfoList[l].Position = 8;
				}
			}
		}
		else if (indicScript.Position == 2)
		{
			int num4 = num3;
			bool flag2 = false;
			bool flag3 = false;
			while (true)
			{
				num4--;
				if (IsConsonant(glyphInfoList[num4].Group))
				{
					if (glyphInfoList[num4].Position != 8 && (glyphInfoList[num4].Position != 11 || flag2))
					{
						break;
					}
					if (glyphInfoList[num4].Position == 8)
					{
						flag2 = true;
					}
					num2 = num4;
				}
				else if (num < num4 && glyphInfoList[num4].Group == 6 && glyphInfoList[num4 - 1].Group == 4)
				{
					flag3 = true;
					break;
				}
				if (num4 <= i)
				{
					flag3 = true;
					break;
				}
			}
			if (!flag3)
			{
				num2 = num4;
			}
		}
		if (flag && num2 == num && i - num2 <= 2)
		{
			flag = false;
		}
		for (int m = num; m < num2; m++)
		{
			glyphInfoList[m].Position = Math.Min(3, glyphInfoList[m].Position);
		}
		if (num2 < num3)
		{
			glyphInfoList[num2].Position = 4;
		}
		for (int m = num2 + 1; m < num3; m++)
		{
			if (glyphInfoList[m].Group != 7)
			{
				continue;
			}
			for (int n = m + 1; n < num3; n++)
			{
				if (IsConsonant(glyphInfoList[n].Group))
				{
					glyphInfoList[n].Position = 13;
					break;
				}
			}
			break;
		}
		if (flag)
		{
			glyphInfoList[num].Position = 1;
		}
		if (oldScript)
		{
			bool flag4 = scriptTag != ScriptTags.Malayalam;
			for (int m = num2 + 1; m < num3; m++)
			{
				if (glyphInfoList[m].Group == 4)
				{
					int num5 = num3 - 1;
					while (num5 > m && !IsConsonant(glyphInfoList[num5].Group) && (!flag4 || glyphInfoList[num5].Group != 4))
					{
						num5--;
					}
					if (glyphInfoList[num5].Group != 4 && num5 > m)
					{
						OtfGlyphInfo glyph = glyphInfoList[m];
						glyphInfoList.Rearrange(m, m + 1, num5 - m);
						glyphInfoList.Set(num5, glyph);
					}
					break;
				}
			}
		}
		int position = 0;
		for (int m = num; m < num3; m++)
		{
			if (((1 << glyphInfoList[m].Group) & 0x80006078u) != 0L)
			{
				glyphInfoList[m].Position = position;
				if (glyphInfoList[m].Group != 4 || glyphInfoList[m].Position != 2)
				{
					continue;
				}
				for (int num6 = m; num6 > num; num6--)
				{
					if (glyphInfoList[num6 - 1].Position != 2)
					{
						glyphInfoList[m].Position = glyphInfoList[num6 - 1].Position;
						break;
					}
				}
			}
			else if (glyphInfoList[m].Position != 14)
			{
				position = glyphInfoList[m].Position;
			}
		}
		int num7 = num2;
		for (int m = num2 + 1; m < num3; m++)
		{
			if (IsConsonant(glyphInfoList[m].Group))
			{
				for (int num8 = num7 + 1; num8 < m; num8++)
				{
					if (glyphInfoList[num8].Position < 14)
					{
						glyphInfoList[num8].Position = glyphInfoList[m].Position;
					}
				}
				num7 = m;
			}
			else if (glyphInfoList[m].Group == 7)
			{
				num7 = m;
			}
		}
		glyphInfoList.DoOrder();
		num2 = num3;
		for (int m = num; m < num3; m++)
		{
			if (glyphInfoList[m].Position == 4)
			{
				num2 = m;
				break;
			}
		}
		int[] indicMask = GetIndicMask();
		for (int m = num; m < num3 && glyphInfoList[m].Position == 1; m++)
		{
			glyphInfoList[m].Mask |= indicMask[2];
		}
		int num9 = indicMask[7];
		if (!oldScript && indicScript.BlwfMode == 0)
		{
			num9 |= indicMask[5];
		}
		for (int m = num; m < num2; m++)
		{
			glyphInfoList[m].Mask |= num9;
		}
		num9 = 0;
		if (num2 < num3)
		{
			glyphInfoList[num2].Mask |= num9;
		}
		num9 = indicMask[5] | indicMask[6] | indicMask[8];
		for (int m = num2 + 1; m < num3; m++)
		{
			glyphInfoList[m].Mask |= num9;
		}
		if (oldScript && scriptTag == ScriptTags.Devanagari)
		{
			for (int m = num; m + 1 < num2; m++)
			{
				if (glyphInfoList[m].Group == 16 && glyphInfoList[m + 1].Group == 4 && (m + 2 == num2 || glyphInfoList[m + 2].Group != 6))
				{
					glyphInfoList[m].Mask |= indicMask[5];
					glyphInfoList[m + 1].Mask |= indicMask[5];
				}
			}
		}
		if (num2 + indicScript.Length >= num3)
		{
			return;
		}
		for (int m = num2 + 1; m + indicScript.Length - 1 < num3; m++)
		{
			List<OtfGlyphInfo> list2 = new List<OtfGlyphInfo>();
			for (int num10 = 0; num10 < indicScript.Length; num10++)
			{
				list2.Add(glyphInfoList[m + num10]);
			}
			if (Replace(prebaseForm, list2, indicScript.Length))
			{
				for (int num11 = 0; num11 < indicScript.Length; num11++)
				{
					glyphInfoList[m++].Mask |= indicMask[4];
				}
				break;
			}
		}
	}

	internal bool IsCombined(int group)
	{
		return IsPresent(group, 96L);
	}

	internal bool IsConsonant(int group)
	{
		return IsPresent(group, 2147563526L);
	}

	internal bool IsHalant(int group)
	{
		return IsPresent(group, 16400L);
	}

	internal bool IsPresent(int group, long flag)
	{
		return ((1L << group) & flag) != 0;
	}

	internal void Reorder(IndicGlyphInfoList glyphInfoList, IndicScript iScript, ScriptTags scriptTag)
	{
		int num = 0;
		int count = glyphInfoList.Glyphs.Count;
		bool flag = true;
		int i;
		for (i = num; i < count; i++)
		{
			if (glyphInfoList[i].Position >= 4)
			{
				if (num < i && glyphInfoList[i].Position > 4)
				{
					i--;
				}
				break;
			}
		}
		if (i == count && num < i && IsPresent(glyphInfoList[i - 1].Group, 64L))
		{
			i--;
		}
		if (i < count)
		{
			while (num < i && IsPresent(glyphInfoList[i].Group, 16408L))
			{
				i--;
			}
		}
		if (num + 1 < count && num < i)
		{
			int num2 = ((i == count) ? (i - 2) : (i - 1));
			if (scriptTag != ScriptTags.Malayalam && scriptTag != ScriptTags.Tamil)
			{
				while (num2 > num && !IsPresent(glyphInfoList[num2].Group, 16528L))
				{
					num2--;
				}
				if (IsHalant(glyphInfoList[num2].Group) && glyphInfoList[num2].Position != 2)
				{
					if (num2 + 1 < count && IsCombined(glyphInfoList[num2 + 1].Group))
					{
						num2++;
					}
				}
				else
				{
					num2 = num;
				}
			}
			if (num < num2 && glyphInfoList[num2].Position != 2)
			{
				for (int num3 = num2; num3 > num; num3--)
				{
					if (glyphInfoList[num3 - 1].Position == 2)
					{
						int num4 = num3 - 1;
						if (num4 < i && i <= num2)
						{
							i--;
						}
						OtfGlyphInfo glyph = glyphInfoList[num4];
						glyphInfoList.Rearrange(num4, num4 + 1, num2 - num4);
						glyphInfoList.Set(num2, glyph);
						num2--;
					}
				}
			}
		}
		if (num + 1 < count && glyphInfoList[num].Position == 1 && glyphInfoList[num].Group != 15)
		{
			int j = num + 1;
			int rephPosition = iScript.RephPosition;
			bool flag2 = false;
			bool flag3 = false;
			if (rephPosition == 12)
			{
				flag2 = true;
			}
			if (!flag2)
			{
				for (j = num + 1; j < i && !IsHalant(glyphInfoList[j].Group); j++)
				{
				}
				if (j < i && IsHalant(glyphInfoList[j].Group))
				{
					if (j + 1 < i && IsCombined(glyphInfoList[j + 1].Group))
					{
						j++;
					}
					flag3 = true;
				}
				if (!flag3 && rephPosition == 5)
				{
					for (j = i; j + 1 < count && glyphInfoList[j + 1].Position <= 5; j++)
					{
					}
					if (j < count)
					{
						flag3 = true;
					}
				}
				if (!flag3 && rephPosition == 9)
				{
					for (j = i; j + 1 < count && ((1L << glyphInfoList[j + 1].Position) & 0x5800) == 0L; j++)
					{
					}
					if (j < count)
					{
						flag3 = true;
					}
				}
			}
			if (!flag3)
			{
				for (j = num + 1; j < i && !IsHalant(glyphInfoList[j].Group); j++)
				{
				}
				if (j < i && IsHalant(glyphInfoList[j].Group))
				{
					if (j + 1 < i && IsCombined(glyphInfoList[j + 1].Group))
					{
						j++;
					}
					flag3 = true;
				}
			}
			if (!flag3)
			{
				j = count - 1;
				while (j > num && glyphInfoList[j].Position == 14)
				{
					j--;
				}
			}
			IndicGlyphInfo glyph2 = glyphInfoList[num];
			glyphInfoList.Rearrange(num, num + 1, j - num);
			glyphInfoList.Set(j, glyph2);
			if (num < i && i <= j)
			{
				i--;
			}
		}
		if (!flag || i + 1 >= count)
		{
			return;
		}
		int k = i + 1;
		int[] indicMask = GetIndicMask();
		for (; k < count; k++)
		{
			if ((glyphInfoList[k].Mask & indicMask[4]) == 0)
			{
				continue;
			}
			if (!glyphInfoList[k].Substitute || !((iScript.Length == 1) ^ glyphInfoList[k].Ligate))
			{
				break;
			}
			int num5 = i;
			if (scriptTag != ScriptTags.Malayalam && scriptTag != ScriptTags.Tamil)
			{
				while (num5 > num && !IsPresent(glyphInfoList[num5 - 1].Group, 16528L))
				{
					num5--;
				}
				if (num5 > num && glyphInfoList[num5 - 1].Group == 7)
				{
					for (int l = i + 1; l < k; l++)
					{
						if (glyphInfoList[l].Group == 7)
						{
							num5--;
							break;
						}
					}
				}
			}
			if (num5 > num && IsHalant(glyphInfoList[num5 - 1].Group) && num5 < count && IsCombined(glyphInfoList[num5].Group))
			{
				num5++;
			}
			int num6 = k;
			OtfGlyphInfo glyph3 = glyphInfoList[num6];
			glyphInfoList.Rearrange(num5 + 1, num5, num6 - num5);
			glyphInfoList.Set(num5, glyph3);
			if (num5 <= i && i < num6)
			{
				i++;
			}
			break;
		}
	}

	private int[] GetIndicMask()
	{
		int[] array = new int[21];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = 1 << i;
		}
		return array;
	}

	private bool Replace(IList<LookupTable> featureTables, IList<OtfGlyphInfo> glyphInfo, int glyphLength)
	{
		bool result = false;
		if (featureTables != null)
		{
			OtfGlyphInfoList glyphInfoList = new OtfGlyphInfoList((glyphInfo as List<OtfGlyphInfo>).GetRange(0, glyphLength), 0, glyphLength);
			foreach (LookupTable featureTable in featureTables)
			{
				if (featureTable.ReplaceGlyphs(glyphInfoList))
				{
					result = true;
				}
			}
		}
		return result;
	}

	private OtfGlyphInfo GetGlyph(UnicodeTrueTypeFont font, IndicScript indicScript)
	{
		TtfGlyphInfo glyph = font.TtfReader.GetGlyph((char)indicScript.InitialChar);
		return new OtfGlyphInfo(glyph.CharCode, glyph.Index, glyph.Width);
	}

	private int GetPosition(OtfGlyphInfo glyphInfo, OtfGlyphInfo advancedGlyphInfo, IndicScript iScript, IList<LookupTable> belowBaseForm, IList<LookupTable> postBaseForm, IList<LookupTable> preBaseForm)
	{
		List<OtfGlyphInfo> list = new List<OtfGlyphInfo>();
		list.Add(advancedGlyphInfo);
		list.Add(glyphInfo);
		list.Add(advancedGlyphInfo);
		if (Replace(belowBaseForm, list, 2) || Replace(belowBaseForm, list.GetRange(1, list.Count - 1), 2))
		{
			return 8;
		}
		if (Replace(postBaseForm, list, 2) || Replace(postBaseForm, list.GetRange(1, list.Count - 1), 2))
		{
			return 11;
		}
		if ((iScript.Length == 2 && (Replace(preBaseForm, list, 2) || Replace(preBaseForm, list.GetRange(1, list.Count - 1), 2))) || (iScript.Length == 1 && Replace(preBaseForm, list.GetRange(1, list.Count - 1), 1)))
		{
			return 11;
		}
		return 4;
	}

	private void Replace(OtfGlyphInfoList glyphList, int index, UnicodeTrueTypeFont font, int[] charCodes)
	{
		OtfTable otfTable = font?.TtfReader.GSUB;
		if (otfTable != null)
		{
			return;
		}
		int[] array = new int[charCodes.Length];
		for (int i = 0; i < charCodes.Length; i++)
		{
			TtfGlyphInfo glyph = font.TtfReader.GetGlyph(charCodes[i]);
			OtfGlyphInfo otfGlyphInfo = new OtfGlyphInfo(glyph.CharCode, glyph.Index, glyph.Width);
			if (otfGlyphInfo == null || otfGlyphInfo.Index <= 0)
			{
				return;
			}
			array[i] = otfGlyphInfo.Index;
		}
		glyphList.Index = index;
		glyphList.CombineAlternateGlyphs(otfTable, array);
	}
}
