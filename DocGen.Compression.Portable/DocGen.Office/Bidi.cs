using System.Collections.Generic;
using System.Text;

namespace DocGen.Office;

internal class Bidi
{
	private int[] m_indexes;

	private byte[] m_indexLevels;

	private Dictionary<int, int> m_mirroringShapeCharacters = new Dictionary<int, int>();

	internal Bidi()
	{
		Update();
	}

	private string DoMirrorShaping(string text)
	{
		char[] array = new char[text.Length];
		for (int i = 0; i < text.Length; i++)
		{
			if ((m_indexLevels[i] & 1) == 1 && m_mirroringShapeCharacters.ContainsKey(text[i]))
			{
				array[i] = (char)m_mirroringShapeCharacters[text[i]];
			}
			else
			{
				array[i] = text[i];
			}
		}
		return new string(array);
	}

	private OtfGlyphInfo[] DoMirrorShaping(OtfGlyphInfo[] text, TtfReader ttfReader)
	{
		OtfGlyphInfo[] array = new OtfGlyphInfo[text.Length];
		for (int i = 0; i < text.Length; i++)
		{
			char key = (char)((text[i].CharCode > 0) ? text[i].CharCode : text[i].Characters[0]);
			if ((m_indexLevels[i] & 1) == 1 && m_mirroringShapeCharacters.ContainsKey(key))
			{
				TtfGlyphInfo glyph = ttfReader.GetGlyph((char)m_mirroringShapeCharacters[key]);
				OtfGlyphInfo otfGlyphInfo = new OtfGlyphInfo(glyph.CharCode, glyph.Index, glyph.Width);
				array[i] = otfGlyphInfo;
			}
			else
			{
				array[i] = text[i];
			}
		}
		return array;
	}

	internal string GetLogicalToVisualString(string inputText, bool isRTL)
	{
		m_indexLevels = new byte[inputText.Length];
		m_indexes = new int[inputText.Length];
		RTLCharacters rTLCharacters = new RTLCharacters();
		m_indexLevels = rTLCharacters.GetVisualOrder(inputText, isRTL);
		SetDefaultIndexLevel();
		DoOrder(0, m_indexLevels.Length - 1);
		string text = DoMirrorShaping(inputText);
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < m_indexes.Length; i++)
		{
			int index = m_indexes[i];
			stringBuilder.Append(text[index]);
		}
		return stringBuilder.ToString();
	}

	internal OtfGlyphInfo[] GetLogicalToVisualGlyphs(List<OtfGlyphInfo> inputText, bool isRTL, TtfReader ttfReader)
	{
		m_indexLevels = new byte[inputText.Count];
		m_indexes = new int[inputText.Count];
		RTLCharacters rTLCharacters = new RTLCharacters();
		m_indexLevels = rTLCharacters.GetVisualOrder(inputText.ToArray(), isRTL);
		SetDefaultIndexLevel();
		DoOrder(0, m_indexLevels.Length - 1);
		OtfGlyphInfo[] array = DoMirrorShaping(inputText.ToArray(), ttfReader);
		List<OtfGlyphInfo> list = new List<OtfGlyphInfo>();
		for (int i = 0; i < m_indexes.Length; i++)
		{
			int num = m_indexes[i];
			list.Add(array[num]);
		}
		return list.ToArray();
	}

	private void SetDefaultIndexLevel()
	{
		for (int i = 0; i < m_indexLevels.Length; i++)
		{
			m_indexes[i] = i;
		}
	}

	private void DoOrder(int sIndex, int eIndex)
	{
		byte b = m_indexLevels[sIndex];
		byte b2 = b;
		byte b3 = b;
		byte b4 = b;
		for (int i = sIndex + 1; i <= eIndex; i++)
		{
			byte b5 = m_indexLevels[i];
			if (b5 > b)
			{
				b = b5;
			}
			else if (b5 < b2)
			{
				b2 = b5;
			}
			b3 &= b5;
			b4 |= b5;
		}
		if ((b4 & 1) == 0)
		{
			return;
		}
		if ((b3 & 1) == 1)
		{
			ReArrange(sIndex, eIndex + 1);
			return;
		}
		b2 = (byte)(b2 | 1u);
		while (b >= b2)
		{
			int num = sIndex;
			while (true)
			{
				if (num <= eIndex && m_indexLevels[num] < b)
				{
					num++;
					continue;
				}
				if (num > eIndex)
				{
					break;
				}
				int j;
				for (j = num + 1; j <= eIndex && m_indexLevels[j] >= b; j++)
				{
				}
				ReArrange(num, j);
				num = j + 1;
			}
			b--;
		}
	}

	private void ReArrange(int i, int j)
	{
		int num = (i + j) / 2;
		j--;
		while (i < num)
		{
			int num2 = m_indexes[i];
			m_indexes[i] = m_indexes[j];
			m_indexes[j] = num2;
			i++;
			j--;
		}
	}

	private void Update()
	{
		m_mirroringShapeCharacters[40] = 41;
		m_mirroringShapeCharacters[41] = 40;
		m_mirroringShapeCharacters[60] = 62;
		m_mirroringShapeCharacters[62] = 60;
		m_mirroringShapeCharacters[91] = 93;
		m_mirroringShapeCharacters[93] = 91;
		m_mirroringShapeCharacters[123] = 125;
		m_mirroringShapeCharacters[125] = 123;
		m_mirroringShapeCharacters[171] = 187;
		m_mirroringShapeCharacters[187] = 171;
		m_mirroringShapeCharacters[8249] = 8250;
		m_mirroringShapeCharacters[8250] = 8249;
		m_mirroringShapeCharacters[8261] = 8262;
		m_mirroringShapeCharacters[8262] = 8261;
		m_mirroringShapeCharacters[8317] = 8318;
		m_mirroringShapeCharacters[8318] = 8317;
		m_mirroringShapeCharacters[8333] = 8334;
		m_mirroringShapeCharacters[8334] = 8333;
		m_mirroringShapeCharacters[8712] = 8715;
		m_mirroringShapeCharacters[8713] = 8716;
		m_mirroringShapeCharacters[8714] = 8717;
		m_mirroringShapeCharacters[8715] = 8712;
		m_mirroringShapeCharacters[8716] = 8713;
		m_mirroringShapeCharacters[8717] = 8714;
		m_mirroringShapeCharacters[8725] = 10741;
		m_mirroringShapeCharacters[8764] = 8765;
		m_mirroringShapeCharacters[8765] = 8764;
		m_mirroringShapeCharacters[8771] = 8909;
		m_mirroringShapeCharacters[8786] = 8787;
		m_mirroringShapeCharacters[8787] = 8786;
		m_mirroringShapeCharacters[8788] = 8789;
		m_mirroringShapeCharacters[8789] = 8788;
		m_mirroringShapeCharacters[8804] = 8805;
		m_mirroringShapeCharacters[8805] = 8804;
		m_mirroringShapeCharacters[8806] = 8807;
		m_mirroringShapeCharacters[8807] = 8806;
		m_mirroringShapeCharacters[8808] = 8809;
		m_mirroringShapeCharacters[8809] = 8808;
		m_mirroringShapeCharacters[8810] = 8811;
		m_mirroringShapeCharacters[8811] = 8810;
		m_mirroringShapeCharacters[8814] = 8815;
		m_mirroringShapeCharacters[8815] = 8814;
		m_mirroringShapeCharacters[8816] = 8817;
		m_mirroringShapeCharacters[8817] = 8816;
		m_mirroringShapeCharacters[8818] = 8819;
		m_mirroringShapeCharacters[8819] = 8818;
		m_mirroringShapeCharacters[8820] = 8821;
		m_mirroringShapeCharacters[8821] = 8820;
		m_mirroringShapeCharacters[8822] = 8823;
		m_mirroringShapeCharacters[8823] = 8822;
		m_mirroringShapeCharacters[8824] = 8825;
		m_mirroringShapeCharacters[8825] = 8824;
		m_mirroringShapeCharacters[8826] = 8827;
		m_mirroringShapeCharacters[8827] = 8826;
		m_mirroringShapeCharacters[8828] = 8829;
		m_mirroringShapeCharacters[8829] = 8828;
		m_mirroringShapeCharacters[8830] = 8831;
		m_mirroringShapeCharacters[8831] = 8830;
		m_mirroringShapeCharacters[8832] = 8833;
		m_mirroringShapeCharacters[8833] = 8832;
		m_mirroringShapeCharacters[8834] = 8835;
		m_mirroringShapeCharacters[8835] = 8834;
		m_mirroringShapeCharacters[8836] = 8837;
		m_mirroringShapeCharacters[8837] = 8836;
		m_mirroringShapeCharacters[8838] = 8839;
		m_mirroringShapeCharacters[8839] = 8838;
		m_mirroringShapeCharacters[8840] = 8841;
		m_mirroringShapeCharacters[8841] = 8840;
		m_mirroringShapeCharacters[8842] = 8843;
		m_mirroringShapeCharacters[8843] = 8842;
		m_mirroringShapeCharacters[8847] = 8848;
		m_mirroringShapeCharacters[8848] = 8847;
		m_mirroringShapeCharacters[8849] = 8850;
		m_mirroringShapeCharacters[8850] = 8849;
		m_mirroringShapeCharacters[8856] = 10680;
		m_mirroringShapeCharacters[8866] = 8867;
		m_mirroringShapeCharacters[8867] = 8866;
		m_mirroringShapeCharacters[8870] = 10974;
		m_mirroringShapeCharacters[8872] = 10980;
		m_mirroringShapeCharacters[8873] = 10979;
		m_mirroringShapeCharacters[8875] = 10981;
		m_mirroringShapeCharacters[8880] = 8881;
		m_mirroringShapeCharacters[8881] = 8880;
		m_mirroringShapeCharacters[8882] = 8883;
		m_mirroringShapeCharacters[8883] = 8882;
		m_mirroringShapeCharacters[8884] = 8885;
		m_mirroringShapeCharacters[8885] = 8884;
		m_mirroringShapeCharacters[8886] = 8887;
		m_mirroringShapeCharacters[8887] = 8886;
		m_mirroringShapeCharacters[8905] = 8906;
		m_mirroringShapeCharacters[8906] = 8905;
		m_mirroringShapeCharacters[8907] = 8908;
		m_mirroringShapeCharacters[8908] = 8907;
		m_mirroringShapeCharacters[8909] = 8771;
		m_mirroringShapeCharacters[8912] = 8913;
		m_mirroringShapeCharacters[8913] = 8912;
		m_mirroringShapeCharacters[8918] = 8919;
		m_mirroringShapeCharacters[8919] = 8918;
		m_mirroringShapeCharacters[8920] = 8921;
		m_mirroringShapeCharacters[8921] = 8920;
		m_mirroringShapeCharacters[8922] = 8923;
		m_mirroringShapeCharacters[8923] = 8922;
		m_mirroringShapeCharacters[8924] = 8925;
		m_mirroringShapeCharacters[8925] = 8924;
		m_mirroringShapeCharacters[8926] = 8927;
		m_mirroringShapeCharacters[8927] = 8926;
		m_mirroringShapeCharacters[8928] = 8929;
		m_mirroringShapeCharacters[8929] = 8928;
		m_mirroringShapeCharacters[8930] = 8931;
		m_mirroringShapeCharacters[8931] = 8930;
		m_mirroringShapeCharacters[8932] = 8933;
		m_mirroringShapeCharacters[8933] = 8932;
		m_mirroringShapeCharacters[8934] = 8935;
		m_mirroringShapeCharacters[8935] = 8934;
		m_mirroringShapeCharacters[8936] = 8937;
		m_mirroringShapeCharacters[8937] = 8936;
		m_mirroringShapeCharacters[8938] = 8939;
		m_mirroringShapeCharacters[8939] = 8938;
		m_mirroringShapeCharacters[8940] = 8941;
		m_mirroringShapeCharacters[8941] = 8940;
		m_mirroringShapeCharacters[8944] = 8945;
		m_mirroringShapeCharacters[8945] = 8944;
		m_mirroringShapeCharacters[8946] = 8954;
		m_mirroringShapeCharacters[8947] = 8955;
		m_mirroringShapeCharacters[8948] = 8956;
		m_mirroringShapeCharacters[8950] = 8957;
		m_mirroringShapeCharacters[8951] = 8958;
		m_mirroringShapeCharacters[8954] = 8946;
		m_mirroringShapeCharacters[8955] = 8947;
		m_mirroringShapeCharacters[8956] = 8948;
		m_mirroringShapeCharacters[8957] = 8950;
		m_mirroringShapeCharacters[8958] = 8951;
		m_mirroringShapeCharacters[8968] = 8969;
		m_mirroringShapeCharacters[8969] = 8968;
		m_mirroringShapeCharacters[8970] = 8971;
		m_mirroringShapeCharacters[8971] = 8970;
		m_mirroringShapeCharacters[9001] = 9002;
		m_mirroringShapeCharacters[9002] = 9001;
		m_mirroringShapeCharacters[10088] = 10089;
		m_mirroringShapeCharacters[10089] = 10088;
		m_mirroringShapeCharacters[10090] = 10091;
		m_mirroringShapeCharacters[10091] = 10090;
		m_mirroringShapeCharacters[10092] = 10093;
		m_mirroringShapeCharacters[10093] = 10092;
		m_mirroringShapeCharacters[10094] = 10095;
		m_mirroringShapeCharacters[10095] = 10094;
		m_mirroringShapeCharacters[10096] = 10097;
		m_mirroringShapeCharacters[10097] = 10096;
		m_mirroringShapeCharacters[10098] = 10099;
		m_mirroringShapeCharacters[10099] = 10098;
		m_mirroringShapeCharacters[10100] = 10101;
		m_mirroringShapeCharacters[10101] = 10100;
		m_mirroringShapeCharacters[10197] = 10198;
		m_mirroringShapeCharacters[10198] = 10197;
		m_mirroringShapeCharacters[10205] = 10206;
		m_mirroringShapeCharacters[10206] = 10205;
		m_mirroringShapeCharacters[10210] = 10211;
		m_mirroringShapeCharacters[10211] = 10210;
		m_mirroringShapeCharacters[10212] = 10213;
		m_mirroringShapeCharacters[10213] = 10212;
		m_mirroringShapeCharacters[10214] = 10215;
		m_mirroringShapeCharacters[10215] = 10214;
		m_mirroringShapeCharacters[10216] = 10217;
		m_mirroringShapeCharacters[10217] = 10216;
		m_mirroringShapeCharacters[10218] = 10219;
		m_mirroringShapeCharacters[10219] = 10218;
		m_mirroringShapeCharacters[10627] = 10628;
		m_mirroringShapeCharacters[10628] = 10627;
		m_mirroringShapeCharacters[10629] = 10630;
		m_mirroringShapeCharacters[10630] = 10629;
		m_mirroringShapeCharacters[10631] = 10632;
		m_mirroringShapeCharacters[10632] = 10631;
		m_mirroringShapeCharacters[10633] = 10634;
		m_mirroringShapeCharacters[10634] = 10633;
		m_mirroringShapeCharacters[10635] = 10636;
		m_mirroringShapeCharacters[10636] = 10635;
		m_mirroringShapeCharacters[10637] = 10640;
		m_mirroringShapeCharacters[10638] = 10639;
		m_mirroringShapeCharacters[10639] = 10638;
		m_mirroringShapeCharacters[10640] = 10637;
		m_mirroringShapeCharacters[10641] = 10642;
		m_mirroringShapeCharacters[10642] = 10641;
		m_mirroringShapeCharacters[10643] = 10644;
		m_mirroringShapeCharacters[10644] = 10643;
		m_mirroringShapeCharacters[10645] = 10646;
		m_mirroringShapeCharacters[10646] = 10645;
		m_mirroringShapeCharacters[10647] = 10648;
		m_mirroringShapeCharacters[10648] = 10647;
		m_mirroringShapeCharacters[10680] = 8856;
		m_mirroringShapeCharacters[10688] = 10689;
		m_mirroringShapeCharacters[10689] = 10688;
		m_mirroringShapeCharacters[10692] = 10693;
		m_mirroringShapeCharacters[10693] = 10692;
		m_mirroringShapeCharacters[10703] = 10704;
		m_mirroringShapeCharacters[10704] = 10703;
		m_mirroringShapeCharacters[10705] = 10706;
		m_mirroringShapeCharacters[10706] = 10705;
		m_mirroringShapeCharacters[10708] = 10709;
		m_mirroringShapeCharacters[10709] = 10708;
		m_mirroringShapeCharacters[10712] = 10713;
		m_mirroringShapeCharacters[10713] = 10712;
		m_mirroringShapeCharacters[10714] = 10715;
		m_mirroringShapeCharacters[10715] = 10714;
		m_mirroringShapeCharacters[10741] = 8725;
		m_mirroringShapeCharacters[10744] = 10745;
		m_mirroringShapeCharacters[10745] = 10744;
		m_mirroringShapeCharacters[10748] = 10749;
		m_mirroringShapeCharacters[10749] = 10748;
		m_mirroringShapeCharacters[10795] = 10796;
		m_mirroringShapeCharacters[10796] = 10795;
		m_mirroringShapeCharacters[10797] = 10796;
		m_mirroringShapeCharacters[10798] = 10797;
		m_mirroringShapeCharacters[10804] = 10805;
		m_mirroringShapeCharacters[10805] = 10804;
		m_mirroringShapeCharacters[10812] = 10813;
		m_mirroringShapeCharacters[10813] = 10812;
		m_mirroringShapeCharacters[10852] = 10853;
		m_mirroringShapeCharacters[10853] = 10852;
		m_mirroringShapeCharacters[10873] = 10874;
		m_mirroringShapeCharacters[10874] = 10873;
		m_mirroringShapeCharacters[10877] = 10878;
		m_mirroringShapeCharacters[10878] = 10877;
		m_mirroringShapeCharacters[10879] = 10880;
		m_mirroringShapeCharacters[10880] = 10879;
		m_mirroringShapeCharacters[10881] = 10882;
		m_mirroringShapeCharacters[10882] = 10881;
		m_mirroringShapeCharacters[10883] = 10884;
		m_mirroringShapeCharacters[10884] = 10883;
		m_mirroringShapeCharacters[10891] = 10892;
		m_mirroringShapeCharacters[10892] = 10891;
		m_mirroringShapeCharacters[10897] = 10898;
		m_mirroringShapeCharacters[10898] = 10897;
		m_mirroringShapeCharacters[10899] = 10900;
		m_mirroringShapeCharacters[10900] = 10899;
		m_mirroringShapeCharacters[10901] = 10902;
		m_mirroringShapeCharacters[10902] = 10901;
		m_mirroringShapeCharacters[10903] = 10904;
		m_mirroringShapeCharacters[10904] = 10903;
		m_mirroringShapeCharacters[10905] = 10906;
		m_mirroringShapeCharacters[10906] = 10905;
		m_mirroringShapeCharacters[10907] = 10908;
		m_mirroringShapeCharacters[10908] = 10907;
		m_mirroringShapeCharacters[10913] = 10914;
		m_mirroringShapeCharacters[10914] = 10913;
		m_mirroringShapeCharacters[10918] = 10919;
		m_mirroringShapeCharacters[10919] = 10918;
		m_mirroringShapeCharacters[10920] = 10921;
		m_mirroringShapeCharacters[10921] = 10920;
		m_mirroringShapeCharacters[10922] = 10923;
		m_mirroringShapeCharacters[10923] = 10922;
		m_mirroringShapeCharacters[10924] = 10925;
		m_mirroringShapeCharacters[10925] = 10924;
		m_mirroringShapeCharacters[10927] = 10928;
		m_mirroringShapeCharacters[10928] = 10927;
		m_mirroringShapeCharacters[10931] = 10932;
		m_mirroringShapeCharacters[10932] = 10931;
		m_mirroringShapeCharacters[10939] = 10940;
		m_mirroringShapeCharacters[10940] = 10939;
		m_mirroringShapeCharacters[10941] = 10942;
		m_mirroringShapeCharacters[10942] = 10941;
		m_mirroringShapeCharacters[10943] = 10944;
		m_mirroringShapeCharacters[10944] = 10943;
		m_mirroringShapeCharacters[10945] = 10946;
		m_mirroringShapeCharacters[10946] = 10945;
		m_mirroringShapeCharacters[10947] = 10948;
		m_mirroringShapeCharacters[10948] = 10947;
		m_mirroringShapeCharacters[10949] = 10950;
		m_mirroringShapeCharacters[10950] = 10949;
		m_mirroringShapeCharacters[10957] = 10958;
		m_mirroringShapeCharacters[10958] = 10957;
		m_mirroringShapeCharacters[10959] = 10960;
		m_mirroringShapeCharacters[10960] = 10959;
		m_mirroringShapeCharacters[10961] = 10962;
		m_mirroringShapeCharacters[10962] = 10961;
		m_mirroringShapeCharacters[10963] = 10964;
		m_mirroringShapeCharacters[10964] = 10963;
		m_mirroringShapeCharacters[10965] = 10966;
		m_mirroringShapeCharacters[10966] = 10965;
		m_mirroringShapeCharacters[10974] = 8870;
		m_mirroringShapeCharacters[10979] = 8873;
		m_mirroringShapeCharacters[10980] = 8872;
		m_mirroringShapeCharacters[10981] = 8875;
		m_mirroringShapeCharacters[10988] = 10989;
		m_mirroringShapeCharacters[10989] = 10988;
		m_mirroringShapeCharacters[10999] = 11000;
		m_mirroringShapeCharacters[11000] = 10999;
		m_mirroringShapeCharacters[11001] = 11002;
		m_mirroringShapeCharacters[11002] = 11001;
		m_mirroringShapeCharacters[12296] = 12297;
		m_mirroringShapeCharacters[12297] = 12296;
		m_mirroringShapeCharacters[12298] = 12299;
		m_mirroringShapeCharacters[12299] = 12298;
		m_mirroringShapeCharacters[12300] = 12301;
		m_mirroringShapeCharacters[12301] = 12300;
		m_mirroringShapeCharacters[12302] = 12303;
		m_mirroringShapeCharacters[12303] = 12302;
		m_mirroringShapeCharacters[12304] = 12305;
		m_mirroringShapeCharacters[12305] = 12304;
		m_mirroringShapeCharacters[12308] = 12309;
		m_mirroringShapeCharacters[12309] = 12308;
		m_mirroringShapeCharacters[12310] = 12311;
		m_mirroringShapeCharacters[12311] = 12310;
		m_mirroringShapeCharacters[12312] = 12313;
		m_mirroringShapeCharacters[12313] = 12312;
		m_mirroringShapeCharacters[12314] = 12315;
		m_mirroringShapeCharacters[12315] = 12314;
		m_mirroringShapeCharacters[65288] = 65289;
		m_mirroringShapeCharacters[65289] = 65288;
		m_mirroringShapeCharacters[65308] = 65310;
		m_mirroringShapeCharacters[65310] = 65308;
		m_mirroringShapeCharacters[65339] = 65341;
		m_mirroringShapeCharacters[65341] = 65339;
		m_mirroringShapeCharacters[65371] = 65373;
		m_mirroringShapeCharacters[65373] = 65371;
		m_mirroringShapeCharacters[65375] = 65376;
		m_mirroringShapeCharacters[65376] = 65375;
		m_mirroringShapeCharacters[65378] = 65379;
		m_mirroringShapeCharacters[65379] = 65378;
	}
}
