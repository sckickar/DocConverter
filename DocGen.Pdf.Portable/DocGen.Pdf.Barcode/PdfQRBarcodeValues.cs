using System;

namespace DocGen.Pdf.Barcode;

internal class PdfQRBarcodeValues
{
	private QRCodeVersion version;

	private PdfErrorCorrectionLevel errorCorrectionLevel;

	private int numberOfDataCodeWord;

	private int numberOfErrorCorrectionCodeWords;

	private int[] numberOfErrorCorrectionBlocks;

	private int end;

	private int dataCapacity;

	private byte[] formatInformation;

	private byte[] versionInformation;

	private static int[] numberOfErrorCorrectingCodeWords = new int[160]
	{
		7, 10, 13, 17, 10, 16, 22, 28, 15, 26,
		36, 44, 20, 36, 52, 64, 26, 48, 72, 88,
		36, 64, 96, 112, 40, 72, 108, 130, 48, 88,
		132, 156, 60, 110, 160, 192, 72, 130, 192, 224,
		80, 150, 224, 264, 96, 176, 260, 308, 104, 198,
		288, 352, 120, 216, 320, 384, 132, 240, 360, 432,
		144, 280, 408, 480, 168, 308, 448, 532, 180, 338,
		504, 588, 196, 364, 546, 650, 224, 416, 600, 700,
		224, 442, 644, 750, 252, 476, 690, 816, 270, 504,
		750, 900, 300, 560, 810, 960, 312, 588, 870, 1050,
		336, 644, 952, 1110, 360, 700, 1020, 1200, 390, 728,
		1050, 1260, 420, 784, 1140, 1350, 450, 812, 1200, 1440,
		480, 868, 1290, 1530, 510, 924, 1350, 1620, 540, 980,
		1440, 1710, 570, 1036, 1530, 1800, 570, 1064, 1590, 1890,
		600, 1120, 1680, 1980, 630, 1204, 1770, 2100, 660, 1260,
		1860, 2220, 720, 1316, 1950, 2310, 750, 1372, 2040, 2430
	};

	private string[] CP437CharSet = new string[49]
	{
		"2591", "2592", "2593", "2502", "2524", "2561", "2562", "2556", "2555", "2563",
		"2551", "2557", "255D", "255C", "255B", "2510", "2514", "2534", "252C", "251C",
		"2500", "253C", "255E", "255F", "255A", "2554", "2569", "2566", "2560", "2550",
		"256C", "2567", "2568", "2564", "2565", "2559", "2558", "2552", "2553", "256B",
		"256A", "2518", "250C", "2588", "2584", "258C", "2590", "2580", "25A0"
	};

	private string[] iSO8859_2CharSet = new string[57]
	{
		"104", "2D8", "141", "13D", "15A", "160", "15E", "164", "179", "17D",
		"17B", "105", "2DB", "142", "13E", "15B", "2C7", "161", "15F", "165",
		"17A", "2DD", "17E", "17C", "154", "102", "139", "106", "10C", "118",
		"11A", "10E", "110", "143", "147", "150", "158", "16E", "170", "162",
		"155", "103", "13A", "107", "10D", "119", "11B", "10F", "111", "144",
		"148", "151", "159", "16F", "171", "163", "2D9"
	};

	private string[] iSO8859_3CharSet = new string[26]
	{
		"126", "124", "130", "15E", "11E", "134", "17B", "127", "125", "131",
		"15F", "11F", "135", "17C", "10A", "108", "120", "11C", "16C", "15C",
		"10B", "109", "121", "11D", "16D", "15D"
	};

	private string[] iSO8859_4CharSet = new string[49]
	{
		"104", "138", "156", "128", "13B", "160", "112", "122", "166", "17D",
		"105", "2DB", "157", "129", "13C", "2C7", "161", "113", "123", "167",
		"14A", "17E", "14B", "100", "12E", "10C", "118", "116", "12A", "110",
		"145", "14C", "136", "172", "168", "16A", "101", "12F", "10D", "119",
		"117", "12B", "111", "146", "14D", "137", "173", "169", "16B"
	};

	private string[] windows1250CharSet = new string[10] { "141", "104", "15E", "17B", "142", "105", "15F", "13D", "13E", "17C" };

	private string[] windows1251CharSet = new string[30]
	{
		"402", "403", "453", "409", "40A", "40C", "40B", "40F", "452", "459",
		"45A", "45C", "45B", "45F", "40E", "45E", "408", "490", "401", "404",
		"407", "406", "456", "491", "451", "454", "458", "405", "455", "457"
	};

	private string[] windows1252CharSet = new string[27]
	{
		"20AC", "201A", "192", "201E", "2026", "2020", "2021", "2C6", "2030", "160",
		"2039", "152", "17D", "2018", "2019", "201C", "201D", "2022", "2013", "2014",
		"2DC", "2122", "161", "203A", "153", "17E", "178"
	};

	private string[] windows1256CharSet = new string[21]
	{
		"67E", "679", "152", "686", "698", "688", "6AF", "6A9", "691", "153",
		"6BA", "6BE", "6C1", "644", "645", "646", "647", "648", "649", "64A",
		"6D2"
	};

	private int[] CP437ReplaceNumber = new int[49]
	{
		176, 177, 178, 179, 180, 181, 182, 183, 184, 185,
		186, 187, 188, 189, 190, 191, 192, 193, 194, 195,
		196, 197, 198, 199, 200, 201, 202, 203, 204, 205,
		206, 207, 208, 209, 210, 211, 212, 213, 214, 215,
		216, 217, 218, 219, 220, 221, 222, 223, 254
	};

	private int[] iSO8859_2ReplaceNumber = new int[57]
	{
		161, 162, 163, 165, 166, 169, 170, 171, 172, 174,
		175, 177, 178, 179, 181, 182, 183, 185, 186, 187,
		188, 189, 190, 191, 192, 195, 197, 198, 200, 202,
		204, 207, 208, 209, 210, 213, 216, 217, 219, 222,
		224, 227, 229, 230, 232, 234, 236, 239, 240, 241,
		242, 245, 248, 249, 251, 254, 255
	};

	private int[] iSO8859_3ReplaceNumber = new int[26]
	{
		161, 166, 169, 170, 171, 172, 175, 177, 182, 185,
		186, 187, 188, 191, 197, 198, 213, 216, 221, 222,
		229, 230, 245, 248, 253, 254
	};

	private int[] iSO8859_4ReplaceNumber = new int[49]
	{
		161, 162, 163, 165, 166, 169, 170, 171, 172, 174,
		177, 178, 179, 181, 182, 183, 185, 186, 187, 188,
		189, 190, 191, 192, 199, 200, 202, 204, 207, 208,
		209, 210, 211, 217, 221, 222, 224, 231, 232, 234,
		236, 239, 240, 241, 242, 243, 249, 253, 254
	};

	private int[] windows1250ReplaceNumber = new int[10] { 163, 165, 170, 175, 179, 185, 186, 188, 190, 191 };

	private int[] windows1251ReplaceNumber = new int[30]
	{
		128, 129, 131, 138, 140, 141, 142, 143, 144, 154,
		156, 157, 158, 159, 161, 162, 163, 165, 168, 170,
		175, 178, 179, 180, 184, 186, 188, 189, 190, 191
	};

	private int[] windows1252ReplaceNumber = new int[27]
	{
		128, 130, 131, 132, 133, 134, 135, 136, 137, 138,
		139, 140, 142, 145, 146, 147, 148, 149, 150, 151,
		152, 153, 154, 155, 156, 158, 159
	};

	private int[] windows1256ReplaceNumber = new int[21]
	{
		129, 138, 140, 141, 142, 143, 144, 152, 154, 156,
		159, 170, 192, 225, 227, 228, 229, 230, 236, 237,
		255
	};

	private static int[] endValues = new int[40]
	{
		208, 359, 567, 807, 1079, 1383, 1568, 1936, 2336, 2768,
		3232, 3728, 4256, 4651, 5243, 5867, 6523, 7211, 7931, 8683,
		9252, 10068, 10916, 11796, 12708, 13652, 14628, 15371, 16411, 17483,
		18587, 19723, 20891, 22091, 23008, 24272, 25568, 26896, 28256, 29648
	};

	private static int[] dataCapacityValues = new int[40]
	{
		26, 44, 70, 100, 134, 172, 196, 242, 292, 346,
		404, 466, 532, 581, 655, 733, 815, 901, 991, 1085,
		1156, 1258, 1364, 1474, 1588, 1706, 1828, 1921, 2051, 2185,
		2323, 2465, 2611, 2761, 2876, 3034, 3196, 3362, 3532, 3706
	};

	internal static int[] numericDataCapacityLow = new int[40]
	{
		41, 77, 127, 187, 255, 322, 370, 461, 552, 652,
		772, 883, 1022, 1101, 1250, 1408, 1548, 1725, 1903, 2061,
		2232, 2409, 2620, 2812, 3057, 3283, 3517, 3669, 3909, 4158,
		4417, 4686, 4965, 5253, 5529, 5836, 6153, 6479, 6743, 7089
	};

	internal static int[] numericDataCapacityMedium = new int[40]
	{
		34, 63, 101, 149, 202, 255, 293, 365, 432, 513,
		604, 691, 796, 871, 991, 1082, 1212, 1346, 1500, 1600,
		1708, 1872, 2059, 2188, 2395, 2544, 2701, 2857, 3035, 3289,
		3486, 3693, 3909, 4134, 4343, 4588, 4775, 5039, 5313, 5596
	};

	internal static int[] numericDataCapacityQuartile = new int[40]
	{
		27, 48, 77, 111, 144, 178, 207, 259, 312, 364,
		427, 489, 580, 621, 703, 775, 876, 948, 1063, 1159,
		1224, 1358, 1468, 1588, 1718, 1804, 1933, 2085, 2181, 2358,
		2473, 2670, 2805, 2949, 3081, 3244, 3417, 3599, 3791, 3993
	};

	internal static int[] numericDataCapacityHigh = new int[40]
	{
		17, 34, 58, 82, 106, 139, 154, 202, 235, 288,
		331, 374, 427, 468, 530, 602, 674, 746, 813, 919,
		969, 1056, 1108, 1228, 1286, 1425, 1501, 1581, 1677, 1782,
		1897, 2022, 2157, 2301, 2361, 2524, 2625, 2735, 2927, 3057
	};

	internal static int[] alphanumericDataCapacityLow = new int[40]
	{
		25, 47, 77, 114, 154, 195, 224, 279, 335, 395,
		468, 535, 619, 667, 758, 854, 938, 1046, 1153, 1249,
		1352, 1460, 1588, 1704, 1853, 1990, 2132, 2223, 2369, 2520,
		2677, 2840, 3009, 3183, 3351, 3537, 3729, 3927, 4087, 4296
	};

	internal static int[] alphanumericDataCapacityMedium = new int[40]
	{
		20, 38, 61, 90, 122, 154, 178, 221, 262, 311,
		366, 419, 483, 528, 600, 656, 734, 816, 909, 970,
		1035, 1134, 1248, 1326, 1451, 1542, 1637, 1732, 1839, 1994,
		2113, 2238, 2369, 2506, 2632, 2780, 2894, 3054, 3220, 3391
	};

	internal static int[] alphanumericDataCapacityQuartile = new int[40]
	{
		16, 29, 47, 67, 87, 108, 125, 157, 189, 221,
		259, 296, 352, 376, 426, 470, 531, 574, 644, 702,
		742, 823, 890, 963, 1041, 1094, 1172, 1263, 1322, 1429,
		1499, 1618, 1700, 1787, 1867, 1966, 2071, 2181, 2298, 2420
	};

	internal static int[] alphanumericDataCapacityHigh = new int[40]
	{
		10, 20, 35, 50, 64, 84, 93, 122, 143, 174,
		200, 227, 259, 283, 321, 365, 408, 452, 493, 557,
		587, 640, 672, 744, 779, 864, 910, 958, 1016, 1080,
		1150, 1226, 1307, 1394, 1431, 1530, 1591, 1658, 1774, 1852
	};

	internal static int[] binaryDataCapacityLow = new int[40]
	{
		17, 32, 53, 78, 106, 134, 154, 192, 230, 271,
		321, 367, 425, 458, 520, 586, 644, 718, 792, 858,
		929, 1003, 1091, 1171, 1273, 1367, 1465, 1528, 1628, 1732,
		1840, 1952, 2068, 2188, 2303, 2431, 2563, 2699, 2809, 2953
	};

	internal static int[] binaryDataCapacityMedium = new int[40]
	{
		14, 26, 42, 62, 84, 106, 122, 152, 180, 213,
		251, 287, 331, 362, 412, 450, 504, 560, 624, 666,
		711, 779, 857, 911, 997, 1059, 1125, 1190, 1264, 1370,
		1452, 1538, 1628, 1722, 1809, 1911, 1989, 2099, 2213, 2331
	};

	internal static int[] binaryDataCapacityQuartile = new int[40]
	{
		11, 20, 32, 46, 60, 74, 86, 108, 130, 151,
		177, 203, 241, 258, 292, 322, 364, 394, 442, 482,
		509, 565, 611, 661, 715, 751, 805, 868, 908, 982,
		1030, 1112, 1168, 1228, 1283, 1351, 1423, 1499, 1579, 1663
	};

	internal static int[] binaryDataCapacityHigh = new int[40]
	{
		7, 14, 24, 34, 44, 58, 64, 84, 98, 119,
		137, 155, 177, 194, 220, 250, 280, 310, 338, 382,
		403, 439, 461, 511, 535, 593, 625, 658, 698, 742,
		790, 842, 898, 958, 983, 1051, 1093, 1139, 1219, 1273
	};

	internal static int[] mixedDataCapacityLow = new int[40]
	{
		152, 272, 440, 640, 864, 1088, 1248, 1552, 1856, 2192,
		2592, 2960, 3424, 3688, 4184, 4712, 5176, 5768, 6360, 6888,
		7456, 8048, 8752, 9392, 10208, 10960, 11744, 12248, 13048, 13880,
		4744, 15640, 16568, 17528, 18448, 19472, 20528, 21616, 22496, 23648
	};

	internal static int[] mixedDataCapacityMedium = new int[40]
	{
		128, 244, 352, 512, 688, 864, 992, 1232, 1456, 1728,
		2032, 2320, 2672, 2920, 3320, 3624, 4056, 4504, 5016, 5352,
		5712, 6256, 6880, 7312, 8000, 8496, 9024, 9544, 10136, 10984,
		1640, 12328, 13048, 13800, 14496, 15312, 15936, 16816, 17728, 18672
	};

	internal static int[] mixedDataCapacityQuartile = new int[40]
	{
		104, 176, 272, 384, 496, 608, 704, 880, 1056, 1232,
		1440, 1648, 1952, 2088, 2360, 2600, 2936, 3176, 3560, 3880,
		4096, 4544, 4912, 5312, 5744, 6032, 6464, 6968, 7288, 7880,
		8264, 8920, 9368, 9848, 10288, 10832, 11408, 12016, 12656, 13328
	};

	internal static int[] mixedDataCapacityHigh = new int[40]
	{
		72, 128, 208, 288, 368, 480, 528, 688, 800, 976,
		1120, 1264, 1440, 1576, 1784, 2024, 2264, 2504, 2728, 3080,
		3248, 3536, 3712, 4112, 4304, 4768, 5024, 5288, 5608, 5960,
		6344, 6760, 7208, 7688, 7888, 8432, 8768, 9136, 9776, 10208
	};

	internal int NumberOfDataCodeWord
	{
		get
		{
			return numberOfDataCodeWord;
		}
		private set
		{
			numberOfDataCodeWord = value;
		}
	}

	internal int NumberOfErrorCorrectingCodeWords
	{
		get
		{
			return numberOfErrorCorrectionCodeWords;
		}
		private set
		{
			numberOfErrorCorrectionCodeWords = value;
		}
	}

	internal int[] NumberOfErrorCorrectionBlocks
	{
		get
		{
			return numberOfErrorCorrectionBlocks;
		}
		private set
		{
			numberOfErrorCorrectionBlocks = value;
		}
	}

	internal int End
	{
		get
		{
			return end;
		}
		private set
		{
			end = value;
		}
	}

	internal int DataCapacity
	{
		get
		{
			return dataCapacity;
		}
		private set
		{
			dataCapacity = value;
		}
	}

	internal byte[] FormatInformation
	{
		get
		{
			return formatInformation;
		}
		private set
		{
			formatInformation = value;
		}
	}

	internal byte[] VersionInformation
	{
		get
		{
			return versionInformation;
		}
		private set
		{
			versionInformation = value;
		}
	}

	public PdfQRBarcodeValues(QRCodeVersion version, PdfErrorCorrectionLevel errorCorrectionLevel)
	{
		this.version = version;
		this.errorCorrectionLevel = errorCorrectionLevel;
		NumberOfDataCodeWord = ObtainNumberOfDataCodeWord();
		NumberOfErrorCorrectingCodeWords = ObtainNumberOfErrorCorrectionCodeWords();
		NumberOfErrorCorrectionBlocks = ObtainNumberOfErrorCorrectionBlocks();
		End = ObtainEnd();
		DataCapacity = ObtainDataCapacity();
		FormatInformation = ObtainFormatInformation();
		VersionInformation = ObtainVersionInformation();
	}

	internal int GetAlphanumericvalues(char Value)
	{
		int num = 0;
		return Value switch
		{
			'0' => 0, 
			'1' => 1, 
			'2' => 2, 
			'3' => 3, 
			'4' => 4, 
			'5' => 5, 
			'6' => 6, 
			'7' => 7, 
			'8' => 8, 
			'9' => 9, 
			'A' => 10, 
			'B' => 11, 
			'C' => 12, 
			'D' => 13, 
			'E' => 14, 
			'F' => 15, 
			'G' => 16, 
			'H' => 17, 
			'I' => 18, 
			'J' => 19, 
			'K' => 20, 
			'L' => 21, 
			'M' => 22, 
			'N' => 23, 
			'O' => 24, 
			'P' => 25, 
			'Q' => 26, 
			'R' => 27, 
			'S' => 28, 
			'T' => 29, 
			'U' => 30, 
			'V' => 31, 
			'W' => 32, 
			'X' => 33, 
			'Y' => 34, 
			'Z' => 35, 
			' ' => 36, 
			'$' => 37, 
			'%' => 38, 
			'*' => 39, 
			'+' => 40, 
			'-' => 41, 
			'.' => 42, 
			'/' => 43, 
			':' => 44, 
			_ => throw new BarcodeException("Not a valid input"), 
		};
	}

	private int ObtainNumberOfDataCodeWord()
	{
		int result = 0;
		switch ((int)version)
		{
		case 1:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 19;
				break;
			case 15:
				result = 16;
				break;
			case 25:
				result = 13;
				break;
			case 30:
				result = 9;
				break;
			}
			break;
		case 2:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 34;
				break;
			case 15:
				result = 28;
				break;
			case 25:
				result = 22;
				break;
			case 30:
				result = 16;
				break;
			}
			break;
		case 3:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 55;
				break;
			case 15:
				result = 44;
				break;
			case 25:
				result = 34;
				break;
			case 30:
				result = 26;
				break;
			}
			break;
		case 4:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 80;
				break;
			case 15:
				result = 64;
				break;
			case 25:
				result = 48;
				break;
			case 30:
				result = 36;
				break;
			}
			break;
		case 5:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 108;
				break;
			case 15:
				result = 86;
				break;
			case 25:
				result = 62;
				break;
			case 30:
				result = 46;
				break;
			}
			break;
		case 6:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 136;
				break;
			case 15:
				result = 108;
				break;
			case 25:
				result = 76;
				break;
			case 30:
				result = 60;
				break;
			}
			break;
		case 7:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 156;
				break;
			case 15:
				result = 124;
				break;
			case 25:
				result = 88;
				break;
			case 30:
				result = 66;
				break;
			}
			break;
		case 8:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 194;
				break;
			case 15:
				result = 154;
				break;
			case 25:
				result = 110;
				break;
			case 30:
				result = 86;
				break;
			}
			break;
		case 9:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 232;
				break;
			case 15:
				result = 182;
				break;
			case 25:
				result = 132;
				break;
			case 30:
				result = 100;
				break;
			}
			break;
		case 10:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 274;
				break;
			case 15:
				result = 216;
				break;
			case 25:
				result = 154;
				break;
			case 30:
				result = 122;
				break;
			}
			break;
		case 11:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 324;
				break;
			case 15:
				result = 254;
				break;
			case 25:
				result = 180;
				break;
			case 30:
				result = 140;
				break;
			}
			break;
		case 12:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 370;
				break;
			case 15:
				result = 290;
				break;
			case 25:
				result = 206;
				break;
			case 30:
				result = 158;
				break;
			}
			break;
		case 13:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 428;
				break;
			case 15:
				result = 334;
				break;
			case 25:
				result = 244;
				break;
			case 30:
				result = 180;
				break;
			}
			break;
		case 14:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 461;
				break;
			case 15:
				result = 365;
				break;
			case 25:
				result = 261;
				break;
			case 30:
				result = 197;
				break;
			}
			break;
		case 15:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 523;
				break;
			case 15:
				result = 415;
				break;
			case 25:
				result = 295;
				break;
			case 30:
				result = 223;
				break;
			}
			break;
		case 16:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 589;
				break;
			case 15:
				result = 453;
				break;
			case 25:
				result = 325;
				break;
			case 30:
				result = 253;
				break;
			}
			break;
		case 17:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 647;
				break;
			case 15:
				result = 507;
				break;
			case 25:
				result = 367;
				break;
			case 30:
				result = 283;
				break;
			}
			break;
		case 18:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 721;
				break;
			case 15:
				result = 563;
				break;
			case 25:
				result = 397;
				break;
			case 30:
				result = 313;
				break;
			}
			break;
		case 19:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 795;
				break;
			case 15:
				result = 627;
				break;
			case 25:
				result = 445;
				break;
			case 30:
				result = 341;
				break;
			}
			break;
		case 20:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 861;
				break;
			case 15:
				result = 669;
				break;
			case 25:
				result = 485;
				break;
			case 30:
				result = 385;
				break;
			}
			break;
		case 21:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 932;
				break;
			case 15:
				result = 714;
				break;
			case 25:
				result = 512;
				break;
			case 30:
				result = 406;
				break;
			}
			break;
		case 22:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 1006;
				break;
			case 15:
				result = 782;
				break;
			case 25:
				result = 568;
				break;
			case 30:
				result = 442;
				break;
			}
			break;
		case 23:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 1094;
				break;
			case 15:
				result = 860;
				break;
			case 25:
				result = 614;
				break;
			case 30:
				result = 464;
				break;
			}
			break;
		case 24:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 1174;
				break;
			case 15:
				result = 914;
				break;
			case 25:
				result = 664;
				break;
			case 30:
				result = 514;
				break;
			}
			break;
		case 25:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 1276;
				break;
			case 15:
				result = 1000;
				break;
			case 25:
				result = 718;
				break;
			case 30:
				result = 538;
				break;
			}
			break;
		case 26:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 1370;
				break;
			case 15:
				result = 1062;
				break;
			case 25:
				result = 754;
				break;
			case 30:
				result = 596;
				break;
			}
			break;
		case 27:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 1468;
				break;
			case 15:
				result = 1128;
				break;
			case 25:
				result = 808;
				break;
			case 30:
				result = 628;
				break;
			}
			break;
		case 28:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 1531;
				break;
			case 15:
				result = 1193;
				break;
			case 25:
				result = 871;
				break;
			case 30:
				result = 661;
				break;
			}
			break;
		case 29:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 1631;
				break;
			case 15:
				result = 1267;
				break;
			case 25:
				result = 911;
				break;
			case 30:
				result = 701;
				break;
			}
			break;
		case 30:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 1735;
				break;
			case 15:
				result = 1373;
				break;
			case 25:
				result = 985;
				break;
			case 30:
				result = 745;
				break;
			}
			break;
		case 31:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 1843;
				break;
			case 15:
				result = 1455;
				break;
			case 25:
				result = 1033;
				break;
			case 30:
				result = 793;
				break;
			}
			break;
		case 32:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 1955;
				break;
			case 15:
				result = 1541;
				break;
			case 25:
				result = 1115;
				break;
			case 30:
				result = 845;
				break;
			}
			break;
		case 33:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 2071;
				break;
			case 15:
				result = 1631;
				break;
			case 25:
				result = 1171;
				break;
			case 30:
				result = 901;
				break;
			}
			break;
		case 34:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 2191;
				break;
			case 15:
				result = 1725;
				break;
			case 25:
				result = 1231;
				break;
			case 30:
				result = 961;
				break;
			}
			break;
		case 35:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 2306;
				break;
			case 15:
				result = 1812;
				break;
			case 25:
				result = 1286;
				break;
			case 30:
				result = 986;
				break;
			}
			break;
		case 36:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 2434;
				break;
			case 15:
				result = 1914;
				break;
			case 25:
				result = 1354;
				break;
			case 30:
				result = 1054;
				break;
			}
			break;
		case 37:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 2566;
				break;
			case 15:
				result = 1992;
				break;
			case 25:
				result = 1426;
				break;
			case 30:
				result = 1096;
				break;
			}
			break;
		case 38:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 2702;
				break;
			case 15:
				result = 2102;
				break;
			case 25:
				result = 1502;
				break;
			case 30:
				result = 1142;
				break;
			}
			break;
		case 39:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 2812;
				break;
			case 15:
				result = 2216;
				break;
			case 25:
				result = 1582;
				break;
			case 30:
				result = 1222;
				break;
			}
			break;
		case 40:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = 2956;
				break;
			case 15:
				result = 2334;
				break;
			case 25:
				result = 1666;
				break;
			case 30:
				result = 1276;
				break;
			}
			break;
		}
		return result;
	}

	private int ObtainNumberOfErrorCorrectionCodeWords()
	{
		int num = (int)(version - 1) * 4;
		switch ((int)errorCorrectionLevel)
		{
		case 7:
			num = num;
			break;
		case 15:
			num++;
			break;
		case 25:
			num += 2;
			break;
		case 30:
			num += 3;
			break;
		}
		return numberOfErrorCorrectingCodeWords[num];
	}

	private int[] ObtainNumberOfErrorCorrectionBlocks()
	{
		int[] result = null;
		switch ((int)version)
		{
		case 1:
			result = new int[1] { 1 };
			break;
		case 2:
			result = new int[1] { 1 };
			break;
		case 3:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[1] { 1 };
				break;
			case 15:
				result = new int[1] { 1 };
				break;
			case 25:
				result = new int[1] { 2 };
				break;
			case 30:
				result = new int[1] { 2 };
				break;
			}
			break;
		case 4:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[1] { 1 };
				break;
			case 15:
				result = new int[1] { 2 };
				break;
			case 25:
				result = new int[1] { 2 };
				break;
			case 30:
				result = new int[1] { 4 };
				break;
			}
			break;
		case 5:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[1] { 1 };
				break;
			case 15:
				result = new int[1] { 2 };
				break;
			case 25:
				result = new int[6] { 2, 33, 15, 2, 34, 16 };
				break;
			case 30:
				result = new int[6] { 2, 33, 11, 2, 34, 12 };
				break;
			}
			break;
		case 6:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[1] { 2 };
				break;
			case 15:
				result = new int[1] { 4 };
				break;
			case 25:
				result = new int[1] { 4 };
				break;
			case 30:
				result = new int[1] { 4 };
				break;
			}
			break;
		case 7:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[1] { 2 };
				break;
			case 15:
				result = new int[1] { 4 };
				break;
			case 25:
				result = new int[6] { 2, 32, 14, 4, 33, 15 };
				break;
			case 30:
				result = new int[6] { 4, 39, 13, 1, 40, 14 };
				break;
			}
			break;
		case 8:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[1] { 2 };
				break;
			case 15:
				result = new int[6] { 2, 60, 38, 2, 61, 39 };
				break;
			case 25:
				result = new int[6] { 4, 40, 18, 2, 41, 19 };
				break;
			case 30:
				result = new int[6] { 4, 40, 14, 2, 41, 15 };
				break;
			}
			break;
		case 9:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[1] { 2 };
				break;
			case 15:
				result = new int[6] { 3, 58, 36, 2, 59, 37 };
				break;
			case 25:
				result = new int[6] { 4, 36, 16, 4, 37, 17 };
				break;
			case 30:
				result = new int[6] { 4, 36, 12, 4, 37, 13 };
				break;
			}
			break;
		case 10:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[6] { 2, 86, 68, 2, 87, 69 };
				break;
			case 15:
				result = new int[6] { 4, 69, 43, 1, 70, 44 };
				break;
			case 25:
				result = new int[6] { 6, 43, 19, 2, 44, 20 };
				break;
			case 30:
				result = new int[6] { 6, 43, 15, 2, 44, 16 };
				break;
			}
			break;
		case 11:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[1] { 4 };
				break;
			case 15:
				result = new int[6] { 1, 80, 50, 4, 81, 51 };
				break;
			case 25:
				result = new int[6] { 4, 50, 22, 4, 51, 23 };
				break;
			case 30:
				result = new int[6] { 3, 36, 12, 8, 37, 13 };
				break;
			}
			break;
		case 12:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[6] { 2, 116, 92, 2, 117, 93 };
				break;
			case 15:
				result = new int[6] { 6, 58, 36, 2, 59, 37 };
				break;
			case 25:
				result = new int[6] { 4, 46, 20, 6, 47, 21 };
				break;
			case 30:
				result = new int[6] { 7, 42, 14, 4, 43, 15 };
				break;
			}
			break;
		case 13:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[1] { 4 };
				break;
			case 15:
				result = new int[6] { 8, 59, 37, 1, 60, 38 };
				break;
			case 25:
				result = new int[6] { 8, 44, 20, 4, 45, 21 };
				break;
			case 30:
				result = new int[6] { 12, 33, 11, 4, 34, 12 };
				break;
			}
			break;
		case 14:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[6] { 3, 145, 115, 1, 146, 116 };
				break;
			case 15:
				result = new int[6] { 4, 64, 40, 5, 65, 41 };
				break;
			case 25:
				result = new int[6] { 11, 36, 16, 5, 37, 17 };
				break;
			case 30:
				result = new int[6] { 11, 36, 12, 5, 37, 13 };
				break;
			}
			break;
		case 15:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[6] { 5, 109, 87, 1, 110, 88 };
				break;
			case 15:
				result = new int[6] { 5, 65, 41, 5, 66, 42 };
				break;
			case 25:
				result = new int[6] { 5, 54, 24, 7, 55, 25 };
				break;
			case 30:
				result = new int[6] { 11, 36, 12, 7, 37, 13 };
				break;
			}
			break;
		case 16:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[6] { 5, 112, 98, 1, 123, 99 };
				break;
			case 15:
				result = new int[6] { 7, 73, 45, 3, 74, 46 };
				break;
			case 25:
				result = new int[6] { 15, 43, 19, 2, 44, 20 };
				break;
			case 30:
				result = new int[6] { 3, 45, 15, 13, 46, 16 };
				break;
			}
			break;
		case 17:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[6] { 1, 135, 107, 5, 136, 108 };
				break;
			case 15:
				result = new int[6] { 10, 74, 46, 1, 75, 47 };
				break;
			case 25:
				result = new int[6] { 1, 50, 22, 15, 51, 23 };
				break;
			case 30:
				result = new int[6] { 2, 42, 14, 17, 43, 15 };
				break;
			}
			break;
		case 18:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[6] { 5, 150, 120, 1, 151, 121 };
				break;
			case 15:
				result = new int[6] { 9, 69, 43, 4, 70, 44 };
				break;
			case 25:
				result = new int[6] { 17, 50, 22, 1, 51, 23 };
				break;
			case 30:
				result = new int[6] { 2, 42, 14, 19, 43, 15 };
				break;
			}
			break;
		case 19:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[6] { 3, 141, 113, 4, 142, 114 };
				break;
			case 15:
				result = new int[6] { 3, 70, 44, 11, 71, 45 };
				break;
			case 25:
				result = new int[6] { 17, 47, 21, 4, 48, 22 };
				break;
			case 30:
				result = new int[6] { 9, 39, 13, 16, 40, 14 };
				break;
			}
			break;
		case 20:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[6] { 3, 135, 107, 5, 136, 108 };
				break;
			case 15:
				result = new int[6] { 3, 67, 41, 13, 68, 42 };
				break;
			case 25:
				result = new int[6] { 15, 54, 24, 5, 55, 25 };
				break;
			case 30:
				result = new int[6] { 15, 43, 15, 10, 44, 16 };
				break;
			}
			break;
		case 21:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[6] { 4, 144, 116, 4, 145, 117 };
				break;
			case 15:
				result = new int[1] { 17 };
				break;
			case 25:
				result = new int[6] { 17, 50, 22, 6, 51, 23 };
				break;
			case 30:
				result = new int[6] { 19, 46, 16, 6, 47, 17 };
				break;
			}
			break;
		case 22:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[6] { 2, 139, 111, 7, 140, 112 };
				break;
			case 15:
				result = new int[1] { 17 };
				break;
			case 25:
				result = new int[6] { 7, 54, 24, 16, 55, 25 };
				break;
			case 30:
				result = new int[1] { 34 };
				break;
			}
			break;
		case 23:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[6] { 4, 151, 121, 5, 152, 122 };
				break;
			case 15:
				result = new int[6] { 4, 75, 47, 14, 76, 48 };
				break;
			case 25:
				result = new int[6] { 11, 54, 24, 14, 55, 25 };
				break;
			case 30:
				result = new int[6] { 16, 45, 15, 14, 46, 16 };
				break;
			}
			break;
		case 24:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[6] { 6, 147, 117, 4, 148, 118 };
				break;
			case 15:
				result = new int[6] { 6, 73, 45, 14, 74, 46 };
				break;
			case 25:
				result = new int[6] { 11, 54, 24, 16, 55, 25 };
				break;
			case 30:
				result = new int[6] { 30, 46, 16, 2, 47, 17 };
				break;
			}
			break;
		case 25:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[6] { 8, 132, 106, 4, 133, 107 };
				break;
			case 15:
				result = new int[6] { 8, 75, 47, 13, 76, 48 };
				break;
			case 25:
				result = new int[6] { 7, 54, 24, 22, 55, 25 };
				break;
			case 30:
				result = new int[6] { 22, 45, 15, 13, 46, 16 };
				break;
			}
			break;
		case 26:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[6] { 10, 142, 114, 2, 143, 115 };
				break;
			case 15:
				result = new int[6] { 19, 74, 46, 4, 75, 47 };
				break;
			case 25:
				result = new int[6] { 28, 50, 22, 6, 51, 23 };
				break;
			case 30:
				result = new int[6] { 33, 46, 16, 4, 47, 17 };
				break;
			}
			break;
		case 27:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[6] { 8, 152, 122, 4, 153, 123 };
				break;
			case 15:
				result = new int[6] { 22, 73, 45, 3, 74, 46 };
				break;
			case 25:
				result = new int[6] { 8, 53, 23, 26, 54, 24 };
				break;
			case 30:
				result = new int[6] { 12, 45, 15, 28, 46, 16 };
				break;
			}
			break;
		case 28:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[6] { 3, 147, 117, 10, 148, 118 };
				break;
			case 15:
				result = new int[6] { 3, 73, 45, 23, 74, 46 };
				break;
			case 25:
				result = new int[6] { 4, 54, 24, 31, 55, 25 };
				break;
			case 30:
				result = new int[6] { 11, 45, 15, 31, 46, 16 };
				break;
			}
			break;
		case 29:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[6] { 7, 146, 116, 7, 147, 117 };
				break;
			case 15:
				result = new int[6] { 21, 73, 45, 7, 74, 46 };
				break;
			case 25:
				result = new int[6] { 1, 53, 23, 37, 54, 24 };
				break;
			case 30:
				result = new int[6] { 19, 45, 15, 26, 46, 16 };
				break;
			}
			break;
		case 30:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[6] { 5, 145, 115, 10, 146, 116 };
				break;
			case 15:
				result = new int[6] { 19, 75, 47, 10, 76, 48 };
				break;
			case 25:
				result = new int[6] { 15, 54, 24, 25, 55, 25 };
				break;
			case 30:
				result = new int[6] { 23, 45, 15, 25, 46, 16 };
				break;
			}
			break;
		case 31:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[6] { 13, 145, 115, 3, 146, 116 };
				break;
			case 15:
				result = new int[6] { 2, 74, 46, 29, 75, 47 };
				break;
			case 25:
				result = new int[6] { 42, 54, 24, 1, 55, 25 };
				break;
			case 30:
				result = new int[6] { 23, 45, 15, 28, 46, 16 };
				break;
			}
			break;
		case 32:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[1] { 17 };
				break;
			case 15:
				result = new int[6] { 10, 74, 46, 23, 75, 47 };
				break;
			case 25:
				result = new int[6] { 10, 54, 24, 35, 55, 25 };
				break;
			case 30:
				result = new int[6] { 19, 45, 15, 35, 46, 16 };
				break;
			}
			break;
		case 33:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[6] { 17, 145, 115, 1, 146, 116 };
				break;
			case 15:
				result = new int[6] { 14, 74, 46, 21, 75, 47 };
				break;
			case 25:
				result = new int[6] { 29, 54, 24, 19, 55, 25 };
				break;
			case 30:
				result = new int[6] { 11, 45, 15, 46, 46, 16 };
				break;
			}
			break;
		case 34:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[6] { 13, 145, 115, 6, 146, 116 };
				break;
			case 15:
				result = new int[6] { 14, 74, 46, 23, 75, 47 };
				break;
			case 25:
				result = new int[6] { 44, 54, 24, 7, 55, 25 };
				break;
			case 30:
				result = new int[6] { 59, 46, 16, 1, 47, 17 };
				break;
			}
			break;
		case 35:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[6] { 12, 151, 121, 7, 152, 122 };
				break;
			case 15:
				result = new int[6] { 12, 75, 47, 26, 76, 48 };
				break;
			case 25:
				result = new int[6] { 39, 54, 24, 14, 55, 25 };
				break;
			case 30:
				result = new int[6] { 22, 45, 15, 41, 46, 16 };
				break;
			}
			break;
		case 36:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[6] { 6, 151, 121, 14, 152, 122 };
				break;
			case 15:
				result = new int[6] { 6, 75, 47, 34, 76, 48 };
				break;
			case 25:
				result = new int[6] { 46, 54, 24, 10, 55, 25 };
				break;
			case 30:
				result = new int[6] { 2, 45, 15, 64, 46, 16 };
				break;
			}
			break;
		case 37:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[6] { 17, 152, 122, 4, 153, 123 };
				break;
			case 15:
				result = new int[6] { 29, 74, 46, 14, 75, 47 };
				break;
			case 25:
				result = new int[6] { 49, 54, 24, 10, 55, 25 };
				break;
			case 30:
				result = new int[6] { 24, 45, 15, 46, 46, 16 };
				break;
			}
			break;
		case 38:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[6] { 4, 152, 122, 18, 153, 123 };
				break;
			case 15:
				result = new int[6] { 13, 74, 46, 32, 75, 47 };
				break;
			case 25:
				result = new int[6] { 48, 54, 24, 14, 55, 25 };
				break;
			case 30:
				result = new int[6] { 42, 45, 15, 32, 46, 16 };
				break;
			}
			break;
		case 39:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[6] { 20, 147, 117, 4, 148, 118 };
				break;
			case 15:
				result = new int[6] { 40, 75, 47, 7, 76, 48 };
				break;
			case 25:
				result = new int[6] { 43, 54, 24, 22, 55, 25 };
				break;
			case 30:
				result = new int[6] { 10, 45, 15, 67, 46, 16 };
				break;
			}
			break;
		case 40:
			switch ((int)errorCorrectionLevel)
			{
			case 7:
				result = new int[6] { 19, 148, 118, 6, 149, 119 };
				break;
			case 15:
				result = new int[6] { 18, 75, 47, 31, 76, 48 };
				break;
			case 25:
				result = new int[6] { 34, 54, 24, 34, 55, 25 };
				break;
			case 30:
				result = new int[6] { 20, 45, 15, 61, 46, 16 };
				break;
			}
			break;
		}
		return result;
	}

	private int ObtainEnd()
	{
		return endValues[(int)(version - 1)];
	}

	private int ObtainDataCapacity()
	{
		return dataCapacityValues[(int)(version - 1)];
	}

	private byte[] ObtainFormatInformation()
	{
		byte[] result = null;
		switch ((int)errorCorrectionLevel)
		{
		case 7:
			result = new byte[15]
			{
				1, 0, 1, 1, 1, 0, 0, 1, 0, 0,
				0, 1, 1, 1, 1
			};
			break;
		case 15:
			result = new byte[15]
			{
				1, 1, 0, 1, 0, 0, 1, 0, 1, 1,
				0, 1, 1, 0, 1
			};
			break;
		case 25:
			result = new byte[15]
			{
				0, 1, 1, 0, 0, 0, 0, 0, 0, 0,
				1, 0, 1, 1, 0
			};
			break;
		case 30:
			result = new byte[15]
			{
				0, 0, 0, 0, 1, 0, 1, 1, 1, 0,
				0, 1, 1, 0, 0
			};
			break;
		}
		return result;
	}

	private byte[] ObtainVersionInformation()
	{
		byte[] result = null;
		switch ((int)version)
		{
		case 7:
			result = new byte[18]
			{
				0, 0, 1, 0, 1, 0, 0, 1, 0, 0,
				1, 1, 1, 1, 1, 0, 0, 0
			};
			break;
		case 8:
			result = new byte[18]
			{
				0, 0, 1, 1, 1, 1, 0, 1, 1, 0,
				1, 0, 0, 0, 0, 1, 0, 0
			};
			break;
		case 9:
			result = new byte[18]
			{
				1, 0, 0, 1, 1, 0, 0, 1, 0, 1,
				0, 1, 1, 0, 0, 1, 0, 0
			};
			break;
		case 10:
			result = new byte[18]
			{
				1, 1, 0, 0, 1, 0, 1, 1, 0, 0,
				1, 0, 0, 1, 0, 1, 0, 0
			};
			break;
		case 11:
			result = new byte[18]
			{
				0, 1, 1, 0, 1, 1, 1, 1, 1, 1,
				0, 1, 1, 1, 0, 1, 0, 0
			};
			break;
		case 12:
			result = new byte[18]
			{
				0, 1, 0, 0, 0, 1, 1, 0, 1, 1,
				1, 0, 0, 0, 1, 1, 0, 0
			};
			break;
		case 13:
			result = new byte[18]
			{
				1, 1, 1, 0, 0, 0, 1, 0, 0, 0,
				0, 1, 1, 0, 1, 1, 0, 0
			};
			break;
		case 14:
			result = new byte[18]
			{
				1, 0, 1, 1, 0, 0, 0, 0, 0, 1,
				1, 0, 0, 1, 1, 1, 0, 0
			};
			break;
		case 15:
			result = new byte[18]
			{
				0, 0, 0, 1, 0, 1, 0, 0, 1, 0,
				0, 1, 1, 1, 1, 1, 0, 0
			};
			break;
		case 16:
			result = new byte[18]
			{
				0, 0, 0, 1, 1, 1, 1, 0, 1, 1,
				0, 1, 0, 0, 0, 0, 1, 0
			};
			break;
		case 17:
			result = new byte[18]
			{
				1, 0, 1, 1, 1, 0, 1, 0, 0, 0,
				1, 0, 1, 0, 0, 0, 1, 0
			};
			break;
		case 18:
			result = new byte[18]
			{
				1, 1, 1, 0, 1, 0, 0, 0, 0, 1,
				0, 1, 0, 1, 0, 0, 1, 0
			};
			break;
		case 19:
			result = new byte[18]
			{
				0, 1, 0, 0, 1, 1, 0, 0, 1, 0,
				1, 0, 1, 1, 0, 0, 1, 0
			};
			break;
		case 20:
			result = new byte[18]
			{
				0, 1, 1, 0, 0, 1, 0, 1, 1, 0,
				0, 1, 0, 0, 1, 0, 1, 0
			};
			break;
		case 21:
			result = new byte[18]
			{
				1, 1, 0, 0, 0, 0, 0, 1, 0, 1,
				1, 0, 1, 0, 1, 0, 1, 0
			};
			break;
		case 22:
			result = new byte[18]
			{
				1, 0, 0, 1, 0, 0, 1, 1, 0, 0,
				0, 1, 0, 1, 1, 0, 1, 0
			};
			break;
		case 23:
			result = new byte[18]
			{
				0, 0, 1, 1, 0, 1, 1, 1, 1, 1,
				1, 0, 1, 1, 1, 0, 1, 0
			};
			break;
		case 24:
			result = new byte[18]
			{
				0, 0, 1, 0, 0, 0, 1, 1, 0, 1,
				1, 1, 0, 0, 0, 1, 1, 0
			};
			break;
		case 25:
			result = new byte[18]
			{
				1, 0, 0, 0, 0, 1, 1, 1, 1, 0,
				0, 0, 1, 0, 0, 1, 1, 0
			};
			break;
		case 26:
			result = new byte[18]
			{
				1, 1, 0, 1, 0, 1, 0, 1, 1, 1,
				1, 1, 0, 1, 0, 1, 1, 0
			};
			break;
		case 27:
			result = new byte[18]
			{
				0, 1, 1, 1, 0, 0, 0, 1, 0, 0,
				0, 0, 1, 1, 0, 1, 1, 0
			};
			break;
		case 28:
			result = new byte[18]
			{
				0, 1, 0, 1, 1, 0, 0, 0, 0, 0,
				1, 1, 0, 0, 1, 1, 1, 0
			};
			break;
		case 29:
			result = new byte[18]
			{
				1, 1, 1, 1, 1, 1, 0, 0, 1, 1,
				0, 0, 1, 0, 1, 1, 1, 0
			};
			break;
		case 30:
			result = new byte[18]
			{
				1, 0, 1, 0, 1, 1, 1, 0, 1, 0,
				1, 1, 0, 1, 1, 1, 1, 0
			};
			break;
		case 31:
			result = new byte[18]
			{
				0, 0, 0, 0, 1, 0, 1, 0, 0, 1,
				0, 0, 1, 1, 1, 1, 1, 0
			};
			break;
		case 32:
			result = new byte[18]
			{
				1, 0, 1, 0, 1, 0, 1, 1, 1, 0,
				0, 1, 0, 0, 0, 0, 0, 1
			};
			break;
		case 33:
			result = new byte[18]
			{
				0, 0, 0, 0, 1, 1, 1, 1, 0, 1,
				1, 0, 1, 0, 0, 0, 0, 1
			};
			break;
		case 34:
			result = new byte[18]
			{
				0, 1, 0, 1, 1, 1, 0, 1, 0, 0,
				0, 1, 0, 1, 0, 0, 0, 1
			};
			break;
		case 35:
			result = new byte[18]
			{
				1, 1, 1, 1, 1, 0, 0, 1, 1, 1,
				1, 0, 1, 1, 0, 0, 0, 1
			};
			break;
		case 36:
			result = new byte[18]
			{
				1, 1, 0, 1, 0, 0, 0, 0, 1, 1,
				0, 1, 0, 0, 1, 0, 0, 1
			};
			break;
		case 37:
			result = new byte[18]
			{
				0, 1, 1, 1, 0, 1, 0, 0, 0, 0,
				1, 0, 1, 0, 1, 0, 0, 1
			};
			break;
		case 38:
			result = new byte[18]
			{
				0, 0, 1, 0, 0, 1, 1, 0, 0, 1,
				0, 1, 0, 1, 1, 0, 0, 1
			};
			break;
		case 39:
			result = new byte[18]
			{
				1, 0, 0, 0, 0, 0, 1, 0, 1, 0,
				1, 0, 1, 1, 1, 0, 0, 1
			};
			break;
		case 40:
			result = new byte[18]
			{
				1, 0, 0, 1, 0, 1, 1, 0, 0, 0,
				1, 1, 0, 0, 0, 1, 0, 1
			};
			break;
		}
		return result;
	}

	internal int GetNumberInEci(char inputChar)
	{
		int num = inputChar;
		string value = num.ToString("X");
		int num2 = Array.IndexOf(CP437CharSet, value);
		if (num2 > -1)
		{
			return CP437ReplaceNumber[num2];
		}
		num2 = Array.IndexOf(iSO8859_2CharSet, value);
		if (num2 > -1)
		{
			return iSO8859_2ReplaceNumber[num2];
		}
		num2 = Array.IndexOf(iSO8859_3CharSet, value);
		if (num2 > -1)
		{
			return iSO8859_3ReplaceNumber[num2];
		}
		num2 = Array.IndexOf(iSO8859_4CharSet, value);
		if (num2 > -1)
		{
			return iSO8859_4ReplaceNumber[num2];
		}
		if (inputChar >= 'Ё' && inputChar <= 'џ' && inputChar != 'Ѝ' && inputChar != 'ѐ' && inputChar != 'ѝ')
		{
			return inputChar - 864;
		}
		if ((inputChar >= 'ء' && inputChar <= 'غ') || (inputChar >= 'ـ' && inputChar <= '\u0652') || inputChar == '؟' || inputChar == '؛' || inputChar == '،')
		{
			return inputChar - 1376;
		}
		if (inputChar < '\u0384' || inputChar > 'ώ')
		{
			switch (inputChar)
			{
			case 'ͺ':
				break;
			case 'א':
			case 'ב':
			case 'ג':
			case 'ד':
			case 'ה':
			case 'ו':
			case 'ז':
			case 'ח':
			case 'ט':
			case 'י':
			case 'ך':
			case 'כ':
			case 'ל':
			case 'ם':
			case 'מ':
			case 'ן':
			case 'נ':
			case 'ס':
			case 'ע':
			case 'ף':
			case 'פ':
			case 'ץ':
			case 'צ':
			case 'ק':
			case 'ר':
			case 'ש':
			case 'ת':
				return inputChar - 1264;
			default:
				if (inputChar >= 'ก' && inputChar <= '๛')
				{
					return inputChar - 3424;
				}
				num2 = Array.IndexOf(windows1250CharSet, value);
				if (num2 > -1)
				{
					return windows1250ReplaceNumber[num2];
				}
				num2 = Array.IndexOf(windows1251CharSet, value);
				if (num2 > -1)
				{
					return windows1251ReplaceNumber[num2];
				}
				num2 = Array.IndexOf(windows1252CharSet, value);
				if (num2 > -1)
				{
					return windows1252ReplaceNumber[num2];
				}
				num2 = Array.IndexOf(windows1256CharSet, value);
				if (num2 > -1)
				{
					return windows1256ReplaceNumber[num2];
				}
				return inputChar;
			}
		}
		return inputChar - 720;
	}

	internal static int GetNumericDataCapacity(QRCodeVersion version, PdfErrorCorrectionLevel errorCorrectionLevel)
	{
		int[] array = null;
		switch ((int)errorCorrectionLevel)
		{
		case 7:
			array = numericDataCapacityLow;
			break;
		case 15:
			array = numericDataCapacityMedium;
			break;
		case 25:
			array = numericDataCapacityQuartile;
			break;
		case 30:
			array = numericDataCapacityHigh;
			break;
		}
		return array[(int)(version - 1)];
	}

	internal static int GetAlphanumericDataCapacity(QRCodeVersion version, PdfErrorCorrectionLevel errorCorrectionLevel)
	{
		int[] array = null;
		switch ((int)errorCorrectionLevel)
		{
		case 7:
			array = alphanumericDataCapacityLow;
			break;
		case 15:
			array = alphanumericDataCapacityMedium;
			break;
		case 25:
			array = alphanumericDataCapacityQuartile;
			break;
		case 30:
			array = alphanumericDataCapacityHigh;
			break;
		}
		return array[(int)(version - 1)];
	}

	internal static int GetBinaryDataCapacity(QRCodeVersion version, PdfErrorCorrectionLevel errorCorrectionLevel)
	{
		int[] array = null;
		switch ((int)errorCorrectionLevel)
		{
		case 7:
			array = binaryDataCapacityLow;
			break;
		case 15:
			array = binaryDataCapacityMedium;
			break;
		case 25:
			array = binaryDataCapacityQuartile;
			break;
		case 30:
			array = binaryDataCapacityHigh;
			break;
		}
		return array[(int)(version - 1)];
	}

	internal static int GetMixedDataCapacity(QRCodeVersion version, PdfErrorCorrectionLevel errorCorrectionLevel)
	{
		int[] array = null;
		switch ((int)errorCorrectionLevel)
		{
		case 7:
			array = mixedDataCapacityLow;
			break;
		case 15:
			array = mixedDataCapacityMedium;
			break;
		case 25:
			array = mixedDataCapacityQuartile;
			break;
		case 30:
			array = mixedDataCapacityHigh;
			break;
		}
		return array[(int)(version - 1)];
	}
}
