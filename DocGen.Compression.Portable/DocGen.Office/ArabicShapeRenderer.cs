using System.Collections.Generic;
using System.Text;

namespace DocGen.Office;

internal class ArabicShapeRenderer
{
	private class ArabicShape
	{
		private char m_value;

		private char m_type;

		private char m_vowel;

		private int m_ligature;

		private int m_shapes = 1;

		internal char Value
		{
			get
			{
				return m_value;
			}
			set
			{
				m_value = value;
			}
		}

		internal char Type
		{
			get
			{
				return m_type;
			}
			set
			{
				m_type = value;
			}
		}

		internal char Vowel
		{
			get
			{
				return m_vowel;
			}
			set
			{
				m_vowel = value;
			}
		}

		internal int Ligature
		{
			get
			{
				return m_ligature;
			}
			set
			{
				m_ligature = value;
			}
		}

		internal int Shapes
		{
			get
			{
				return m_shapes;
			}
			set
			{
				m_shapes = value;
			}
		}
	}

	private readonly char[][] ArabicCharTable = new char[76][]
	{
		new char[2] { 'ء', 'ﺀ' },
		new char[3] { 'آ', 'ﺁ', 'ﺂ' },
		new char[3] { 'أ', 'ﺃ', 'ﺄ' },
		new char[3] { 'ؤ', 'ﺅ', 'ﺆ' },
		new char[3] { 'إ', 'ﺇ', 'ﺈ' },
		new char[5] { 'ئ', 'ﺉ', 'ﺊ', 'ﺋ', 'ﺌ' },
		new char[3] { 'ا', 'ﺍ', 'ﺎ' },
		new char[5] { 'ب', 'ﺏ', 'ﺐ', 'ﺑ', 'ﺒ' },
		new char[3] { 'ة', 'ﺓ', 'ﺔ' },
		new char[5] { 'ت', 'ﺕ', 'ﺖ', 'ﺗ', 'ﺘ' },
		new char[5] { 'ث', 'ﺙ', 'ﺚ', 'ﺛ', 'ﺜ' },
		new char[5] { 'ج', 'ﺝ', 'ﺞ', 'ﺟ', 'ﺠ' },
		new char[5] { 'ح', 'ﺡ', 'ﺢ', 'ﺣ', 'ﺤ' },
		new char[5] { 'خ', 'ﺥ', 'ﺦ', 'ﺧ', 'ﺨ' },
		new char[3] { 'د', 'ﺩ', 'ﺪ' },
		new char[3] { 'ذ', 'ﺫ', 'ﺬ' },
		new char[3] { 'ر', 'ﺭ', 'ﺮ' },
		new char[3] { 'ز', 'ﺯ', 'ﺰ' },
		new char[5] { 'س', 'ﺱ', 'ﺲ', 'ﺳ', 'ﺴ' },
		new char[5] { 'ش', 'ﺵ', 'ﺶ', 'ﺷ', 'ﺸ' },
		new char[5] { 'ص', 'ﺹ', 'ﺺ', 'ﺻ', 'ﺼ' },
		new char[5] { 'ض', 'ﺽ', 'ﺾ', 'ﺿ', 'ﻀ' },
		new char[5] { 'ط', 'ﻁ', 'ﻂ', 'ﻃ', 'ﻄ' },
		new char[5] { 'ظ', 'ﻅ', 'ﻆ', 'ﻇ', 'ﻈ' },
		new char[5] { 'ع', 'ﻉ', 'ﻊ', 'ﻋ', 'ﻌ' },
		new char[5] { 'غ', 'ﻍ', 'ﻎ', 'ﻏ', 'ﻐ' },
		new char[5] { 'ـ', 'ـ', 'ـ', 'ـ', 'ـ' },
		new char[5] { 'ف', 'ﻑ', 'ﻒ', 'ﻓ', 'ﻔ' },
		new char[5] { 'ق', 'ﻕ', 'ﻖ', 'ﻗ', 'ﻘ' },
		new char[5] { 'ك', 'ﻙ', 'ﻚ', 'ﻛ', 'ﻜ' },
		new char[5] { 'ل', 'ﻝ', 'ﻞ', 'ﻟ', 'ﻠ' },
		new char[5] { 'م', 'ﻡ', 'ﻢ', 'ﻣ', 'ﻤ' },
		new char[5] { 'ن', 'ﻥ', 'ﻦ', 'ﻧ', 'ﻨ' },
		new char[5] { 'ه', 'ﻩ', 'ﻪ', 'ﻫ', 'ﻬ' },
		new char[3] { 'و', 'ﻭ', 'ﻮ' },
		new char[5] { 'ى', 'ﻯ', 'ﻰ', 'ﯨ', 'ﯩ' },
		new char[5] { 'ي', 'ﻱ', 'ﻲ', 'ﻳ', 'ﻴ' },
		new char[3] { 'ٱ', 'ﭐ', 'ﭑ' },
		new char[5] { 'ٹ', 'ﭦ', 'ﭧ', 'ﭨ', 'ﭩ' },
		new char[5] { 'ٺ', 'ﭞ', 'ﭟ', 'ﭠ', 'ﭡ' },
		new char[5] { 'ٻ', 'ﭒ', 'ﭓ', 'ﭔ', 'ﭕ' },
		new char[5] { 'پ', 'ﭖ', 'ﭗ', 'ﭘ', 'ﭙ' },
		new char[5] { 'ٿ', 'ﭢ', 'ﭣ', 'ﭤ', 'ﭥ' },
		new char[5] { 'ڀ', 'ﭚ', 'ﭛ', 'ﭜ', 'ﭝ' },
		new char[5] { 'ڃ', 'ﭶ', 'ﭷ', 'ﭸ', 'ﭹ' },
		new char[5] { 'ڄ', 'ﭲ', 'ﭳ', 'ﭴ', 'ﭵ' },
		new char[5] { 'چ', 'ﭺ', 'ﭻ', 'ﭼ', 'ﭽ' },
		new char[5] { 'ڇ', 'ﭾ', 'ﭿ', 'ﮀ', 'ﮁ' },
		new char[3] { 'ڈ', 'ﮈ', 'ﮉ' },
		new char[3] { 'ڌ', 'ﮄ', 'ﮅ' },
		new char[3] { 'ڍ', 'ﮂ', 'ﮃ' },
		new char[3] { 'ڎ', 'ﮆ', 'ﮇ' },
		new char[3] { 'ڑ', 'ﮌ', 'ﮍ' },
		new char[3] { 'ژ', 'ﮊ', 'ﮋ' },
		new char[5] { 'ڤ', 'ﭪ', 'ﭫ', 'ﭬ', 'ﭭ' },
		new char[5] { 'ڦ', 'ﭮ', 'ﭯ', 'ﭰ', 'ﭱ' },
		new char[5] { 'ک', 'ﮎ', 'ﮏ', 'ﮐ', 'ﮑ' },
		new char[5] { 'ڭ', 'ﯓ', 'ﯔ', 'ﯕ', 'ﯖ' },
		new char[5] { 'گ', 'ﮒ', 'ﮓ', 'ﮔ', 'ﮕ' },
		new char[5] { 'ڱ', 'ﮚ', 'ﮛ', 'ﮜ', 'ﮝ' },
		new char[5] { 'ڳ', 'ﮖ', 'ﮗ', 'ﮘ', 'ﮙ' },
		new char[3] { 'ں', 'ﮞ', 'ﮟ' },
		new char[5] { 'ڻ', 'ﮠ', 'ﮡ', 'ﮢ', 'ﮣ' },
		new char[5] { 'ھ', 'ﮪ', 'ﮫ', 'ﮬ', 'ﮭ' },
		new char[3] { 'ۀ', 'ﮤ', 'ﮥ' },
		new char[5] { 'ہ', 'ﮦ', 'ﮧ', 'ﮨ', 'ﮩ' },
		new char[3] { 'ۅ', 'ﯠ', 'ﯡ' },
		new char[3] { 'ۆ', 'ﯙ', 'ﯚ' },
		new char[3] { 'ۇ', 'ﯗ', 'ﯘ' },
		new char[3] { 'ۈ', 'ﯛ', 'ﯜ' },
		new char[3] { 'ۉ', 'ﯢ', 'ﯣ' },
		new char[3] { 'ۋ', 'ﯞ', 'ﯟ' },
		new char[5] { 'ی', 'ﯼ', 'ﯽ', 'ﯾ', 'ﯿ' },
		new char[5] { 'ې', 'ﯤ', 'ﯥ', 'ﯦ', 'ﯧ' },
		new char[3] { 'ے', 'ﮮ', 'ﮯ' },
		new char[3] { 'ۓ', 'ﮰ', 'ﮱ' }
	};

	private const char Alef = 'ا';

	private const char AlefHamza = 'أ';

	private const char AlefHamzaBelow = 'إ';

	private const char AlefMadda = 'آ';

	private const char Lam = 'ل';

	private const char Hamza = 'ء';

	private const char ZeroWidthJoiner = '\u200d';

	private const char HamzaAbove = '\u0654';

	private const char HamzaBelow = '\u0655';

	private const char WawHamza = 'ؤ';

	private const char YehHamza = 'ئ';

	private const char Waw = 'و';

	private const char AlefMaksura = 'ى';

	private const char Yeh = 'ي';

	private const char FarsiYeh = 'ی';

	private const char Shadda = '\u0651';

	private const char Madda = '\u0653';

	private const char Lwa = 'ﻻ';

	private const char Lwawh = 'ﻷ';

	private const char Lwawhb = 'ﻹ';

	private const char Lwawm = 'ﻵ';

	private const char Bwhb = 'ۓ';

	private const char Fathatan = '\u064b';

	private const char SuperScriptAlef = '\u0670';

	private const int Vowel = 1;

	private Dictionary<char, char[]> m_arabicMapTable = new Dictionary<char, char[]>();

	internal ArabicShapeRenderer()
	{
		for (int i = 0; i < ArabicCharTable.Length; i++)
		{
			m_arabicMapTable[ArabicCharTable[i][0]] = ArabicCharTable[i];
		}
	}

	private char GetCharacterShape(char input, int index)
	{
		if (input >= 'ء' && input <= 'ۓ')
		{
			if (m_arabicMapTable.TryGetValue(input, out var value))
			{
				return value[index + 1];
			}
		}
		else if (input >= 'ﻵ' && input <= 'ﻻ')
		{
			return (char)(input + index);
		}
		return input;
	}

	internal string Shape(char[] text, int level)
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		foreach (char c in text)
		{
			if (c >= '\u0600' && c <= 'ۿ')
			{
				stringBuilder2.Append(c);
				continue;
			}
			if (stringBuilder2.Length > 0)
			{
				string value = DoShape(stringBuilder2.ToString().ToCharArray(), 0);
				stringBuilder.Append(value);
				stringBuilder2 = new StringBuilder();
			}
			stringBuilder.Append(c);
		}
		if (stringBuilder2.Length > 0)
		{
			string value2 = DoShape(stringBuilder2.ToString().ToCharArray(), 0);
			stringBuilder.Append(value2);
		}
		return stringBuilder.ToString();
	}

	private string DoShape(char[] input, int level)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		ArabicShape arabicShape = new ArabicShape();
		ArabicShape arabicShape2 = new ArabicShape();
		int num2;
		while (num < input.Length)
		{
			char c = input[num++];
			if (Ligature(c, arabicShape2) == 0)
			{
				int shapeCount = GetShapeCount(c);
				num2 = ((shapeCount != 1) ? 2 : 0);
				if (arabicShape.Shapes > 2)
				{
					num2++;
				}
				num2 %= arabicShape2.Shapes;
				arabicShape2.Value = GetCharacterShape(arabicShape2.Value, num2);
				Append(stringBuilder, arabicShape, level);
				arabicShape = arabicShape2;
				arabicShape2 = new ArabicShape();
				arabicShape2.Value = c;
				arabicShape2.Shapes = shapeCount;
				arabicShape2.Ligature++;
			}
		}
		num2 = ((arabicShape.Shapes > 2) ? 1 : 0);
		num2 %= arabicShape2.Shapes;
		arabicShape2.Value = GetCharacterShape(arabicShape2.Value, num2);
		Append(stringBuilder, arabicShape, level);
		Append(stringBuilder, arabicShape2, level);
		return stringBuilder.ToString();
	}

	private void Append(StringBuilder builder, ArabicShape shape, int level)
	{
		if (shape.Value == '\0')
		{
			return;
		}
		builder.Append(shape.Value);
		shape.Ligature--;
		if (shape.Type != 0)
		{
			if ((level & 1) == 0)
			{
				builder.Append(shape.Type);
				shape.Ligature--;
			}
			else
			{
				shape.Ligature--;
			}
		}
		if (shape.Vowel != 0)
		{
			if ((level & 1) == 0)
			{
				builder.Append(shape.Vowel);
				shape.Ligature--;
			}
			else
			{
				shape.Ligature--;
			}
		}
	}

	private int Ligature(char value, ArabicShape shape)
	{
		if (shape.Value != 0)
		{
			int result = 0;
			if ((value >= '\u064b' && value <= '\u0655') || value == '\u0670')
			{
				result = 1;
				if (shape.Vowel != 0 && value != '\u0651')
				{
					result = 2;
				}
				switch (value)
				{
				case '\u0651':
					if (shape.Type == '\0')
					{
						shape.Type = '\u0651';
						break;
					}
					return 0;
				case '\u0655':
					if (shape.Value == 'ا')
					{
						shape.Value = 'إ';
						result = 2;
					}
					else if (value == 'ﻻ')
					{
						shape.Value = 'ﻹ';
						result = 2;
					}
					else
					{
						shape.Type = '\u0655';
					}
					break;
				case '\u0654':
					if (shape.Value == 'ا')
					{
						shape.Value = 'أ';
						result = 2;
					}
					else if (shape.Value == 'ﻻ')
					{
						shape.Value = 'ﻷ';
						result = 2;
					}
					else if (shape.Value == 'و')
					{
						shape.Value = 'ؤ';
						result = 2;
					}
					else if (shape.Value == 'ي' || shape.Value == 'ى' || shape.Value == 'ی')
					{
						shape.Value = 'ئ';
						result = 2;
					}
					else
					{
						shape.Type = '\u0654';
					}
					break;
				case '\u0653':
					if (shape.Value == 'ا')
					{
						shape.Value = 'آ';
						result = 2;
					}
					break;
				default:
					shape.Vowel = value;
					break;
				}
				if (result == 1)
				{
					shape.Ligature++;
				}
				return result;
			}
			if (shape.Vowel != 0)
			{
				return 0;
			}
			if (shape.Value == 'ل')
			{
				switch (value)
				{
				case 'ا':
					shape.Value = 'ﻻ';
					shape.Shapes = 2;
					result = 3;
					break;
				case 'أ':
					shape.Value = 'ﻷ';
					shape.Shapes = 2;
					result = 3;
					break;
				case 'إ':
					shape.Value = 'ﻹ';
					shape.Shapes = 2;
					result = 3;
					break;
				case 'آ':
					shape.Value = 'ﻵ';
					shape.Shapes = 2;
					result = 3;
					break;
				}
			}
			else if (shape.Value == '\0')
			{
				shape.Value = value;
				shape.Shapes = GetShapeCount(value);
				result = 1;
			}
			return result;
		}
		return 0;
	}

	private int GetShapeCount(char shape)
	{
		if (shape >= 'ء' && shape <= 'ۓ' && (shape < '\u064b' || shape > '\u0655') && shape != '\u0670')
		{
			if (m_arabicMapTable.TryGetValue(shape, out var value))
			{
				return value.Length - 1;
			}
		}
		else if (shape == '\u200d')
		{
			return 4;
		}
		return 1;
	}

	internal void Dispose()
	{
		if (m_arabicMapTable != null)
		{
			m_arabicMapTable.Clear();
			m_arabicMapTable = null;
		}
	}
}
