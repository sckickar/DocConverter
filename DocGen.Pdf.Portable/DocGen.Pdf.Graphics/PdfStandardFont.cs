using System;
using System.Collections.Generic;
using System.Text;
using DocGen.Pdf.Graphics.Fonts;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Graphics;

public class PdfStandardFont : PdfFont
{
	private const int c_charOffset = 32;

	private PdfFontFamily m_fontFamily;

	internal Dictionary<char, float> charWidthDictionary = new Dictionary<char, float>();

	private static List<int> usedChars = new List<int>();

	private PdfDictionary dictionary = new PdfDictionary();

	internal Encoding fontEncoding;

	private Dictionary<int, string> unicodeToNames;

	private Dictionary<string, int> widthTableUpdate;

	private int[] charcode = new int[3680]
	{
		65, 198, 508, 482, 63462, 193, 63457, 258, 7854, 1232,
		7862, 7856, 7858, 7860, 461, 9398, 194, 7844, 7852, 7846,
		7848, 63458, 7850, 63177, 63412, 1040, 512, 196, 1234, 478,
		63460, 7840, 480, 192, 63456, 7842, 1236, 514, 913, 902,
		256, 65313, 260, 197, 506, 7680, 63461, 63329, 195, 63459,
		1329, 66, 9399, 7682, 7684, 1041, 1330, 914, 385, 7686,
		65314, 63220, 63330, 386, 67, 1342, 262, 63178, 63221, 268,
		199, 7688, 63463, 9400, 264, 266, 63416, 1353, 1212, 1063,
		1214, 1206, 1268, 1347, 1227, 1208, 935, 391, 63222, 65315,
		1361, 63331, 68, 497, 452, 1332, 393, 270, 7696, 9401,
		7698, 272, 7690, 7692, 1044, 1006, 8710, 916, 394, 63179,
		63180, 63181, 63400, 988, 1026, 7694, 65316, 63223, 63332, 395,
		498, 453, 1248, 1029, 1039, 69, 201, 63465, 276, 282,
		7708, 1333, 9402, 202, 7870, 7704, 7878, 7872, 7874, 63466,
		7876, 1028, 516, 203, 63467, 278, 7864, 1060, 200, 63464,
		1335, 7866, 8551, 518, 1124, 1051, 8554, 274, 7702, 7700,
		1052, 65317, 1053, 1186, 330, 1188, 1223, 280, 400, 917,
		904, 1056, 398, 1069, 1057, 1194, 425, 63333, 919, 1336,
		905, 208, 63472, 7868, 7706, 8364, 439, 494, 440, 70,
		9403, 7710, 1366, 996, 401, 1138, 8548, 65318, 8547, 63334,
		71, 13191, 500, 915, 404, 1002, 286, 486, 290, 9404,
		284, 288, 1043, 1346, 1172, 1170, 1168, 403, 1331, 1027,
		7712, 65319, 63182, 63328, 63335, 667, 484, 72, 9679, 9642,
		9643, 9633, 13259, 1192, 1202, 1066, 294, 7722, 7720, 9405,
		292, 7718, 7714, 7716, 65320, 1344, 1000, 63336, 63183, 63224,
		13200, 73, 1071, 306, 1070, 205, 63469, 300, 463, 9406,
		206, 63470, 1030, 520, 207, 7726, 1252, 63471, 304, 7882,
		1238, 1045, 8465, 204, 63468, 7880, 1048, 522, 1049, 298,
		1250, 65321, 1339, 1025, 302, 921, 406, 938, 906, 63337,
		407, 296, 7724, 1140, 1142, 74, 1345, 9407, 308, 1032,
		1355, 65322, 63338, 75, 13189, 13261, 1184, 7728, 1050, 1178,
		1219, 922, 1182, 1180, 488, 310, 9408, 7730, 1364, 1343,
		1061, 998, 408, 1036, 7732, 65323, 1152, 990, 1134, 63339,
		76, 455, 63167, 313, 923, 317, 315, 9409, 7740, 319,
		7734, 7736, 1340, 456, 1033, 7738, 65324, 321, 63225, 63340,
		77, 13190, 63184, 63407, 7742, 9410, 7744, 7746, 1348, 65325,
		63341, 412, 924, 78, 458, 323, 327, 325, 9411, 7754,
		7748, 7750, 413, 8552, 459, 1034, 7752, 65326, 1350, 63342,
		209, 63473, 925, 79, 338, 63226, 211, 63475, 1256, 1258,
		334, 465, 415, 9412, 212, 7888, 7896, 7890, 7892, 63476,
		7894, 1054, 336, 524, 214, 1254, 63478, 7884, 63227, 210,
		63474, 1365, 8486, 7886, 416, 7898, 7906, 7900, 7902, 7904,
		418, 526, 332, 7762, 7760, 1120, 937, 1146, 1148, 911,
		927, 908, 65327, 8544, 490, 492, 390, 216, 510, 63480,
		63343, 1150, 213, 7756, 7758, 63477, 80, 7764, 9413, 7766,
		1055, 1354, 1190, 934, 420, 928, 1363, 65328, 936, 1136,
		63344, 81, 9414, 65329, 63345, 82, 1356, 340, 344, 342,
		9415, 528, 7768, 7770, 7772, 1360, 8476, 929, 63228, 530,
		7774, 65330, 63346, 641, 694, 83, 9484, 9492, 9488, 9496,
		9532, 9516, 9524, 9500, 9508, 9472, 9474, 9569, 9570, 9558,
		9557, 9571, 9553, 9559, 9565, 9564, 9563, 9566, 9567, 9562,
		9556, 9577, 9574, 9568, 9552, 9580, 9575, 9576, 9572, 9573,
		9561, 9560, 9554, 9555, 9579, 9578, 346, 7780, 992, 352,
		7782, 63229, 350, 399, 1240, 1242, 9416, 348, 536, 7776,
		7778, 7784, 1357, 8550, 1351, 1064, 1065, 994, 1210, 1004,
		931, 8549, 65331, 1068, 63347, 986, 84, 932, 358, 356,
		354, 9417, 7792, 7786, 7788, 1058, 1196, 8553, 1204, 920,
		428, 222, 63486, 8546, 63230, 1359, 7790, 65332, 1337, 444,
		388, 423, 430, 1062, 1035, 63348, 8555, 8545, 85, 218,
		63482, 364, 467, 9418, 219, 7798, 63483, 1059, 368, 532,
		220, 471, 7794, 473, 1264, 475, 469, 63484, 7908, 217,
		63481, 7910, 431, 7912, 7920, 7914, 7916, 7918, 1266, 534,
		1144, 362, 1262, 7802, 65333, 370, 933, 978, 979, 433,
		939, 980, 910, 366, 1038, 63349, 1198, 1200, 360, 7800,
		7796, 86, 9419, 7806, 1042, 1358, 434, 65334, 1352, 63350,
		7804, 87, 7810, 9420, 372, 7812, 7814, 7816, 7808, 65335,
		63351, 88, 9421, 7820, 7818, 1341, 926, 65336, 63352, 89,
		221, 63485, 1122, 9422, 374, 376, 63487, 7822, 7924, 1067,
		1272, 7922, 435, 7926, 1349, 1031, 1362, 65337, 63353, 7928,
		1130, 1132, 1126, 1128, 90, 1334, 377, 381, 63231, 9423,
		7824, 379, 7826, 1047, 1176, 1246, 918, 1338, 1217, 1046,
		1174, 1244, 7828, 65338, 63354, 437, 97, 2438, 225, 2310,
		2694, 2566, 2622, 13059, 2494, 2366, 2750, 1375, 2416, 2437,
		12570, 259, 7855, 1233, 7863, 7857, 7859, 7861, 462, 9424,
		226, 7845, 7853, 7847, 7849, 7851, 180, 791, 769, 2388,
		719, 833, 1072, 513, 2673, 2309, 228, 1235, 479, 7841,
		481, 230, 509, 12624, 483, 8213, 8356, 63172, 63173, 1073,
		1074, 1075, 1076, 1077, 1105, 1078, 1079, 1080, 1081, 1082,
		1083, 1084, 1085, 1086, 1087, 1088, 1089, 1090, 1091, 1092,
		1093, 1094, 1095, 1096, 1097, 1098, 1099, 1100, 1101, 1102,
		1103, 1169, 1106, 1107, 1108, 1109, 1110, 1111, 1112, 1113,
		1114, 1115, 1116, 1118, 63174, 1119, 1123, 1139, 1141, 63175,
		63176, 1241, 8206, 8207, 8205, 1642, 1548, 1632, 1633, 1634,
		1635, 1636, 1637, 1638, 1639, 1640, 1641, 1563, 1567, 1569,
		1570, 1571, 1572, 1573, 1574, 1575, 1576, 1577, 1578, 1579,
		1580, 1581, 1582, 1583, 1584, 1585, 1586, 1587, 1588, 1589,
		1590, 1591, 1592, 1593, 1594, 1600, 1601, 1602, 1603, 1604,
		1605, 1606, 1608, 1609, 1610, 1611, 1612, 1613, 1614, 1615,
		1616, 1617, 1618, 1607, 1700, 1662, 1670, 1688, 1711, 1657,
		1672, 1681, 1722, 1746, 1749, 8362, 1470, 1475, 1488, 1489,
		1490, 1491, 1492, 1493, 1494, 1495, 1496, 1497, 1498, 1499,
		1500, 1501, 1502, 1503, 1504, 1505, 1506, 1507, 1508, 1509,
		1510, 1511, 1512, 1513, 1514, 64298, 64299, 64331, 64287, 1520,
		1521, 1522, 64309, 1460, 1461, 1462, 1467, 1464, 1463, 1456,
		1458, 1457, 1459, 1474, 1473, 1465, 1468, 1469, 1471, 1472,
		700, 8453, 8467, 8470, 8236, 8237, 8238, 8204, 1645, 701,
		224, 2693, 2565, 12354, 7843, 2448, 12574, 2320, 1237, 2704,
		2576, 2632, 65226, 65227, 65228, 515, 2504, 2376, 2760, 12450,
		65393, 12623, 64304, 65166, 65156, 65160, 64335, 65154, 65264, 65267,
		65268, 64302, 64303, 8501, 8780, 945, 940, 257, 65345, 38,
		65286, 63270, 13250, 12578, 12580, 3674, 8736, 12296, 65087, 12297,
		65088, 9001, 9002, 8491, 903, 2386, 2434, 2306, 2690, 261,
		13056, 9372, 1370, 63743, 8784, 8776, 8786, 8773, 12686, 12685,
		8978, 7834, 229, 507, 7681, 8596, 8675, 8672, 8674, 8673,
		8660, 8659, 8656, 8658, 8657, 8595, 8601, 8600, 8681, 709,
		706, 707, 708, 63719, 8592, 8653, 8646, 8678, 8594, 8655,
		10142, 8644, 8680, 8676, 8677, 8593, 8597, 8616, 8598, 8645,
		8599, 8679, 63718, 94, 65342, 126, 65374, 593, 594, 12353,
		12449, 65383, 42, 8727, 65290, 65121, 8258, 63209, 8771, 64,
		227, 65312, 65131, 592, 2452, 12576, 2324, 2708, 2580, 2519,
		2636, 2508, 2380, 2764, 2365, 1377, 64288, 98, 2476, 92,
		65340, 2348, 2732, 2604, 12400, 3647, 12496, 124, 65372, 12549,
		9425, 7683, 7685, 9836, 8757, 65168, 65169, 12409, 65170, 64671,
		64520, 64621, 12505, 1378, 946, 976, 64305, 64332, 2477, 2349,
		2733, 2605, 595, 12403, 12499, 664, 2562, 13105, 9670, 9660,
		9668, 9664, 12304, 65083, 12305, 65084, 9699, 9698, 9644, 9658,
		9654, 9787, 9632, 9733, 9700, 9701, 9652, 9650, 9251, 7687,
		9608, 65346, 3610, 12412, 12508, 9373, 13251, 63732, 123, 63731,
		63730, 65371, 65115, 63729, 65079, 125, 63742, 63741, 65373, 65116,
		63740, 65080, 91, 63728, 63727, 65339, 63726, 93, 63739, 63738,
		65341, 63737, 728, 814, 774, 815, 785, 865, 810, 826,
		166, 384, 63210, 387, 12406, 12502, 8226, 9688, 8729, 9678,
		99, 1390, 2458, 263, 2330, 2714, 2586, 13192, 2433, 784,
		2305, 2689, 8682, 711, 812, 780, 8629, 12568, 269, 231,
		7689, 9426, 265, 597, 267, 13253, 184, 807, 162, 8451,
		63199, 65504, 63394, 63200, 1401, 2459, 2331, 2715, 2587, 12564,
		1213, 10003, 1215, 1207, 1269, 1395, 1228, 1209, 967, 12919,
		12823, 12905, 12618, 12809, 3594, 3592, 3593, 3596, 392, 12918,
		12822, 12904, 12616, 12808, 12828, 9675, 8855, 8857, 8853, 12342,
		9680, 9681, 710, 813, 770, 8999, 450, 448, 449, 451,
		9827, 9831, 13220, 65347, 13216, 1409, 58, 8353, 65306, 65109,
		721, 720, 44, 787, 789, 63171, 1373, 63201, 65292, 788,
		65104, 63202, 786, 699, 9788, 8750, 8963, 6, 7, 8,
		24, 13, 17, 18, 19, 20, 127, 16, 25, 5,
		4, 27, 23, 3, 12, 28, 29, 9, 10, 21,
		30, 15, 14, 2, 1, 26, 22, 31, 11, 169,
		63721, 63193, 12300, 65378, 65089, 12301, 65379, 65090, 13183, 13255,
		13254, 9374, 8354, 663, 8911, 8910, 164, 63185, 63186, 63188,
		63189, 100, 1380, 2470, 2342, 65214, 65215, 65216, 8224, 8225,
		2726, 2598, 12384, 12480, 64307, 65194, 2404, 1447, 1157, 63187,
		12298, 65085, 12299, 65086, 811, 2405, 63190, 783, 8748, 8215,
		819, 831, 698, 8214, 782, 12553, 13256, 271, 7697, 9427,
		7699, 273, 2465, 2337, 2721, 2593, 64393, 2396, 2466, 2338,
		2722, 2594, 7691, 7693, 1643, 176, 1453, 12391, 1007, 12487,
		9003, 8998, 948, 397, 2552, 676, 2471, 2343, 2727, 2599,
		599, 901, 836, 9830, 9826, 168, 63191, 804, 776, 63192,
		12386, 12482, 12291, 247, 8739, 8725, 9619, 7695, 13207, 65348,
		9604, 3598, 3604, 12393, 12489, 36, 63203, 65284, 63268, 65129,
		63204, 8363, 13094, 729, 775, 803, 12539, 305, 63166, 644,
		8901, 9676, 798, 725, 9375, 63211, 598, 396, 12389, 12485,
		499, 675, 454, 677, 1249, 101, 233, 9793, 2447, 12572,
		277, 2317, 2701, 2373, 2757, 283, 7709, 1381, 1415, 9428,
		234, 7871, 7705, 7879, 7873, 7875, 7877, 517, 2319, 235,
		279, 7865, 2575, 2631, 232, 2703, 1383, 12573, 12360, 7867,
		12575, 56, 2542, 9319, 10129, 2414, 9329, 9349, 9369, 2798,
		2670, 12328, 9835, 12839, 8328, 65304, 63288, 9339, 9359, 1784,
		8567, 8312, 3672, 519, 1125, 12456, 65396, 2676, 12628, 8712,
		9322, 9342, 9362, 8570, 8230, 8942, 275, 7703, 7701, 8212,
		65073, 65349, 1371, 8709, 12579, 8211, 65074, 1187, 331, 12581,
		1189, 1224, 8194, 281, 12627, 603, 666, 604, 606, 605,
		9376, 949, 941, 61, 65309, 65126, 8316, 8801, 12582, 600,
		1195, 643, 646, 2318, 2374, 426, 645, 12359, 12455, 65386,
		8494, 63212, 951, 1384, 942, 240, 7869, 7707, 1425, 477,
		12641, 2503, 2375, 2759, 33, 1372, 8252, 161, 63393, 65281,
		63265, 8707, 658, 495, 659, 441, 442, 102, 2398, 2654,
		8457, 12552, 9429, 7711, 1414, 65234, 65235, 65236, 997, 9792,
		64256, 64259, 64260, 64257, 9326, 9346, 9366, 8210, 64314, 713,
		9673, 53, 2539, 9316, 10126, 2411, 8541, 2795, 2667, 12325,
		12836, 8325, 65301, 63285, 9336, 9356, 1781, 8564, 8309, 3669,
		64258, 402, 65350, 13209, 3615, 3613, 3663, 8704, 52, 2538,
		9315, 10125, 2410, 2794, 2666, 12324, 12835, 8324, 65300, 2551,
		63284, 9335, 9355, 1780, 8563, 8308, 9325, 9345, 9365, 3668,
		715, 9377, 8260, 8355, 103, 2455, 501, 2327, 64403, 64404,
		64405, 2711, 2583, 12364, 12460, 947, 611, 736, 1003, 12557,
		287, 487, 291, 9430, 285, 289, 12370, 12466, 8785, 1436,
		1523, 1437, 223, 1438, 1524, 12307, 2456, 1394, 2328, 2712,
		2584, 65230, 65231, 65232, 1173, 1171, 2394, 2650, 608, 13203,
		12366, 12462, 1379, 64306, 446, 660, 662, 704, 661, 705,
		740, 673, 674, 7713, 65351, 12372, 12468, 9378, 13228, 8711,
		96, 790, 768, 2387, 718, 65344, 832, 62, 8805, 8923,
		65310, 8819, 8823, 8807, 65125, 609, 485, 12368, 171, 187,
		8249, 8250, 12464, 13080, 13257, 104, 1193, 1729, 2489, 1203,
		2361, 2745, 2617, 65186, 65187, 12399, 65188, 13098, 12495, 65418,
		2637, 12644, 8636, 8640, 13258, 295, 12559, 7723, 7721, 9431,
		293, 7719, 7715, 7717, 9829, 9825, 64308, 64423, 65258, 64421,
		64420, 64424, 65259, 12408, 64425, 65260, 13179, 12504, 65421, 13110,
		615, 13113, 614, 689, 12923, 12827, 12909, 12622, 12813, 12402,
		12498, 65419, 7830, 65352, 1392, 3627, 12411, 12507, 65422, 3630,
		777, 801, 802, 13122, 1001, 795, 9832, 8962, 9379, 688,
		613, 12405, 13107, 12501, 65420, 733, 779, 405, 45, 63205,
		65293, 65123, 63206, 8208, 105, 237, 2439, 12583, 301, 464,
		9432, 238, 521, 12943, 12939, 12863, 12858, 12965, 12294, 12289,
		65380, 12855, 12963, 12847, 12861, 12957, 12864, 12950, 12854, 12843,
		12850, 12964, 12293, 12952, 12856, 12967, 12966, 12969, 12846, 12842,
		12852, 12290, 12958, 12867, 12857, 12862, 12968, 12953, 12866, 12851,
		12288, 12853, 12849, 12859, 12848, 12860, 12844, 12845, 12295, 12942,
		12938, 12948, 12944, 12940, 12941, 2311, 239, 7727, 1253, 7883,
		1239, 12917, 12821, 12903, 12615, 12807, 236, 2695, 2567, 12356,
		7881, 2440, 2312, 2696, 2568, 2624, 523, 2496, 2368, 2752,
		307, 12452, 65394, 12643, 732, 1452, 299, 1251, 8787, 2623,
		65353, 8734, 1387, 8747, 8993, 63733, 8992, 8745, 13061, 9689,
		303, 953, 970, 912, 617, 943, 9380, 2674, 12355, 12451,
		65384, 2554, 616, 63213, 12445, 12541, 297, 7725, 12585, 2495,
		2367, 2751, 1143, 106, 1393, 2460, 2332, 2716, 2588, 12560,
		496, 9433, 309, 669, 607, 65182, 65183, 65184, 64395, 2461,
		2333, 2717, 2589, 1403, 12292, 65354, 9381, 690, 107, 1185,
		2453, 7729, 1179, 2325, 64315, 65242, 65243, 65244, 64333, 2709,
		2581, 12363, 1220, 12459, 65398, 954, 1008, 12657, 12676, 12664,
		12665, 13069, 12533, 13188, 1183, 65392, 1181, 12558, 13193, 489,
		311, 9434, 7731, 1412, 12369, 12465, 65401, 1391, 12534, 312,
		2454, 2326, 2710, 2582, 65190, 65191, 65192, 999, 2393, 2649,
		12920, 12824, 12906, 12619, 12810, 3586, 3589, 3587, 3588, 3675,
		409, 3590, 13201, 12365, 12461, 65399, 13077, 13078, 13076, 12910,
		12814, 12896, 12593, 12800, 12595, 7733, 13208, 13222, 65355, 13218,
		12371, 13248, 3585, 12467, 65402, 13086, 1153, 12927, 835, 9382,
		13226, 1135, 13263, 670, 12367, 12463, 65400, 13240, 13246, 108,
		2482, 314, 2354, 2738, 2610, 3653, 65276, 65272, 65271, 65274,
		65273, 65275, 65270, 65269, 955, 411, 64316, 65246, 64714, 65247,
		64713, 64715, 65010, 65248, 64904, 64716, 9711, 410, 620, 12556,
		318, 316, 9435, 7741, 320, 7735, 7737, 794, 792, 60,
		8804, 8922, 65308, 8818, 8822, 8806, 65124, 622, 9612, 621,
		1388, 457, 63168, 2355, 2739, 7739, 2356, 2529, 2401, 2531,
		2403, 619, 65356, 13264, 3628, 8743, 172, 8976, 8744, 3621,
		383, 65102, 818, 65101, 9674, 9383, 322, 63214, 9617, 3622,
		2444, 2316, 2530, 2402, 13267, 109, 2478, 175, 817, 772,
		717, 65507, 7743, 2350, 2734, 2606, 1444, 12414, 63637, 63636,
		3659, 63635, 63628, 63627, 3656, 63626, 63620, 3633, 63625, 3655,
		63631, 63630, 3657, 63629, 63634, 63633, 3658, 63632, 3654, 12510,
		65423, 9794, 13127, 1455, 13187, 12551, 13268, 9436, 13221, 7745,
		7747, 65250, 65251, 65252, 64721, 64584, 13133, 12417, 13182, 12513,
		65426, 64318, 1396, 1445, 1446, 625, 13202, 65381, 183, 12914,
		12818, 12900, 12609, 12656, 12804, 12654, 12655, 12415, 12511, 65424,
		8722, 800, 8854, 727, 8723, 8242, 13130, 13129, 624, 13206,
		13219, 65357, 13215, 12418, 13249, 12514, 65427, 13270, 3617, 13223,
		13224, 9384, 13227, 13235, 63215, 623, 181, 13186, 8811, 8810,
		13196, 956, 13197, 12416, 12512, 65425, 13205, 215, 13211, 1443,
		9834, 9837, 9839, 13234, 13238, 13244, 13241, 13239, 13247, 13245,
		110, 2472, 324, 2344, 2728, 2600, 12394, 12490, 65413, 329,
		13185, 12555, 160, 328, 326, 9437, 7755, 7749, 7751, 12397,
		12493, 65416, 13195, 2457, 2329, 2713, 2585, 3591, 12435, 626,
		627, 12911, 12815, 12597, 12897, 12598, 12596, 12648, 12801, 12647,
		12646, 12395, 12491, 65414, 63641, 3661, 57, 2543, 9320, 10130,
		2415, 2799, 2671, 12329, 12840, 8329, 65305, 63289, 9340, 9360,
		1785, 8568, 8313, 9330, 9350, 9370, 3673, 460, 12531, 65437,
		414, 7753, 65358, 13210, 2467, 2339, 2723, 2595, 2345, 12398,
		12494, 65417, 3603, 3609, 65254, 64415, 65255, 64722, 64587, 65256,
		64725, 64590, 64653, 8716, 8713, 8800, 8815, 8817, 8825, 8802,
		8814, 8816, 8742, 8832, 8836, 8833, 8837, 1398, 9385, 13233,
		8319, 241, 957, 12396, 12492, 65415, 2492, 2364, 2748, 2620,
		35, 65283, 65119, 884, 885, 64320, 13237, 13243, 2462, 2334,
		2718, 2590, 111, 243, 3629, 629, 1257, 1259, 2451, 12571,
		335, 2321, 2705, 2377, 2761, 466, 9438, 244, 7889, 7897,
		7891, 7893, 7895, 337, 525, 2323, 246, 1255, 7885, 339,
		12634, 731, 808, 242, 2707, 1413, 12362, 7887, 417, 7899,
		7907, 7901, 7903, 7905, 419, 527, 12458, 65397, 12631, 1451,
		333, 7763, 7761, 2384, 969, 982, 1121, 631, 1147, 1149,
		974, 2768, 959, 972, 65359, 49, 2535, 9312, 10122, 2407,
		8228, 8539, 63196, 2791, 2663, 189, 12321, 12832, 8321, 65297,
		2548, 63281, 9332, 9352, 1777, 188, 8560, 185, 3665, 8531,
		491, 493, 2579, 2635, 596, 9386, 9702, 8997, 170, 186,
		8735, 2322, 2378, 248, 511, 12361, 12457, 65387, 63216, 1151,
		245, 7757, 7759, 12577, 8254, 65098, 773, 65097, 65100, 65099,
		2507, 2379, 2763, 112, 13184, 13099, 2474, 7765, 2346, 8671,
		8670, 2730, 2602, 12401, 3631, 12497, 1156, 1216, 12671, 182,
		8741, 40, 64830, 63725, 63724, 8333, 65288, 65113, 8317, 63723,
		65077, 41, 64831, 63736, 63735, 8334, 65289, 65114, 8318, 63734,
		65078, 8706, 1433, 13225, 1441, 12550, 9439, 7767, 64324, 13115,
		64323, 1402, 64343, 64344, 12410, 64345, 12506, 1191, 64334, 37,
		65285, 65130, 46, 1417, 65377, 63207, 65294, 65106, 63208, 834,
		8869, 8240, 8359, 13194, 2475, 2347, 2731, 2603, 966, 981,
		12922, 12826, 12908, 12621, 12812, 632, 3642, 421, 3614, 3612,
		3616, 960, 12915, 12819, 12662, 12901, 12658, 12610, 12805, 12660,
		12612, 12661, 12663, 12659, 12404, 12500, 1411, 43, 799, 177,
		726, 65291, 65122, 8314, 65360, 13272, 12413, 9759, 9756, 9758,
		9757, 12509, 3611, 12306, 12320, 9387, 8826, 8478, 697, 8245,
		8719, 8965, 12540, 8984, 8834, 8835, 8759, 8733, 968, 1137,
		1158, 13232, 12407, 12503, 13236, 13242, 113, 2392, 1448, 65238,
		65239, 65240, 1439, 12561, 9440, 672, 65361, 64327, 9388, 9833,
		63, 1374, 191, 63423, 894, 65311, 63295, 34, 8222, 8220,
		65282, 12318, 12317, 8221, 8216, 8219, 8217, 8218, 39, 65287,
		114, 1404, 2480, 341, 2352, 8730, 63717, 13230, 13231, 13229,
		2736, 2608, 12425, 12521, 65431, 2545, 2544, 612, 8758, 12566,
		345, 343, 9441, 529, 7769, 7771, 7773, 8251, 8838, 8839,
		174, 63720, 63194, 1408, 65198, 12428, 12524, 65434, 64328, 8765,
		1431, 638, 639, 2525, 2397, 961, 637, 635, 693, 1009,
		734, 12913, 12817, 12899, 12608, 12602, 12649, 12601, 12603, 12652,
		12803, 12607, 12604, 12651, 12605, 12606, 12650, 12653, 793, 8895,
		12426, 12522, 65432, 730, 805, 778, 703, 1369, 796, 723,
		702, 825, 722, 531, 13137, 7775, 636, 634, 65362, 12429,
		12525, 65435, 3619, 9389, 2524, 2353, 2652, 64397, 2528, 2400,
		2784, 2500, 2372, 2756, 63217, 9616, 633, 692, 12427, 12523,
		65433, 2546, 2547, 63197, 3620, 2443, 2315, 2699, 2499, 2371,
		2755, 115, 2488, 347, 7781, 2360, 65210, 65211, 65212, 2744,
		2616, 12373, 12469, 65403, 65018, 64321, 3634, 3649, 3652, 3651,
		3635, 3632, 3648, 63622, 3637, 63621, 3636, 3650, 63624, 3639,
		63623, 3638, 3640, 3641, 12569, 353, 7783, 351, 601, 1243,
		602, 9442, 349, 537, 7777, 7779, 7785, 828, 8243, 714,
		167, 65202, 65203, 65204, 1426, 1405, 12379, 12475, 65406, 59,
		65307, 65108, 12444, 65439, 13090, 13091, 55, 2541, 9318, 10128,
		2413, 8542, 2797, 2669, 12327, 12838, 8327, 65303, 63287, 9338,
		9358, 1783, 8566, 8311, 9328, 9348, 9368, 3671, 173, 1399,
		2486, 64609, 64606, 64608, 64610, 64607, 9618, 2358, 2742, 2614,
		1427, 12565, 65206, 65207, 65208, 995, 1211, 1005, 64329, 64300,
		64301, 642, 963, 962, 1010, 12375, 12471, 65404, 8764, 12916,
		12820, 12670, 12902, 12666, 12613, 12667, 12806, 12669, 12668, 54,
		2540, 9317, 10127, 2412, 2796, 2668, 12326, 12837, 8326, 65302,
		63286, 9337, 9357, 1782, 8565, 8310, 9327, 2553, 9347, 9367,
		3670, 47, 65295, 7835, 9786, 65363, 12381, 12477, 65407, 824,
		823, 3625, 3624, 3595, 3626, 32, 9824, 9828, 9390, 827,
		13252, 13213, 9641, 9636, 13199, 13214, 13262, 13265, 13266, 13198,
		13269, 13212, 13217, 9638, 9639, 9640, 9637, 9635, 13275, 2487,
		2359, 2743, 12617, 12677, 12672, 12594, 12645, 12611, 12614, 12600,
		63218, 163, 65505, 822, 821, 8842, 8827, 8715, 12377, 12473,
		65405, 8721, 8843, 13276, 13180, 116, 2468, 8868, 8867, 2340,
		2724, 2596, 65218, 65219, 12383, 65220, 13181, 12479, 65408, 964,
		64330, 359, 12554, 357, 680, 355, 64379, 64380, 64381, 9443,
		7793, 7831, 7787, 7789, 1197, 65174, 64674, 64524, 65175, 12390,
		64673, 64523, 65172, 65176, 64676, 64526, 64627, 12486, 65411, 8481,
		9742, 1440, 1449, 9321, 12841, 9341, 9361, 8569, 679, 64312,
		1205, 1435, 2469, 2341, 2725, 2597, 65196, 63640, 63639, 3660,
		63638, 65178, 65179, 65180, 8756, 952, 977, 12921, 12825, 12907,
		12620, 12811, 9324, 9344, 9364, 3601, 429, 3602, 254, 3607,
		3600, 3608, 3606, 1154, 1644, 51, 2537, 9314, 10124, 2409,
		8540, 2793, 2665, 12323, 12834, 8323, 65299, 2550, 63283, 9334,
		9354, 1779, 190, 63198, 8562, 179, 3667, 13204, 12385, 12481,
		65409, 12912, 12816, 12898, 12599, 12802, 816, 771, 864, 820,
		830, 1430, 2672, 1155, 1407, 7791, 65364, 1385, 12392, 12488,
		65412, 741, 745, 742, 744, 743, 445, 389, 424, 900,
		13095, 3599, 12308, 65117, 65081, 12309, 65118, 65082, 3605, 427,
		9391, 8482, 63722, 63195, 648, 678, 64326, 63219, 2463, 2335,
		2719, 2591, 64359, 64360, 64361, 2464, 2336, 2720, 2592, 647,
		12388, 12484, 65410, 12387, 12483, 65391, 9323, 9343, 9363, 8571,
		9331, 21316, 9351, 9371, 50, 2536, 9313, 10123, 2408, 8229,
		65072, 2792, 2664, 12322, 12833, 8322, 65298, 2549, 63282, 9333,
		9353, 1778, 8561, 443, 178, 3666, 8532, 117, 250, 649,
		2441, 12584, 365, 468, 9444, 251, 7799, 2385, 369, 533,
		2313, 252, 472, 7795, 474, 1265, 476, 470, 7909, 249,
		2697, 2569, 12358, 7911, 432, 7913, 7921, 7915, 7917, 7919,
		1267, 535, 12454, 65395, 1145, 12636, 363, 1263, 7803, 2625,
		65365, 95, 65343, 65075, 65103, 8746, 371, 9392, 9600, 1476,
		965, 971, 944, 650, 973, 797, 724, 2675, 367, 12357,
		12453, 65385, 1199, 1201, 361, 7801, 7797, 2442, 2314, 2698,
		2570, 2626, 2498, 2370, 2754, 2497, 2369, 2753, 118, 2357,
		2741, 2613, 12535, 9445, 7807, 64363, 64364, 64365, 12537, 781,
		809, 716, 712, 1406, 651, 12536, 2509, 2381, 2765, 2435,
		2307, 2691, 65366, 1400, 12446, 12542, 12443, 65438, 12538, 9393,
		7805, 652, 12436, 12532, 119, 7811, 12633, 12431, 12527, 65436,
		12632, 12430, 12526, 13143, 12316, 65076, 65262, 65158, 13277, 9446,
		373, 7813, 7815, 7817, 12433, 8472, 12529, 12638, 12637, 7809,
		12302, 65091, 12303, 65092, 9671, 9672, 9663, 9661, 9667, 9665,
		12310, 12311, 9657, 9655, 9734, 9743, 12312, 12313, 9653, 9651,
		12432, 12528, 12639, 65367, 12434, 12530, 65382, 8361, 65510, 3623,
		9394, 7832, 695, 653, 447, 120, 829, 12562, 9447, 7821,
		7819, 1389, 958, 65368, 9395, 739, 121, 13134, 2479, 253,
		2351, 12626, 2735, 2607, 12420, 12516, 65428, 12625, 3662, 12419,
		12515, 65388, 9448, 375, 255, 7823, 7925, 64431, 65266, 65162,
		65163, 65164, 64733, 64600, 64660, 1745, 12630, 165, 65509, 12629,
		12678, 1450, 1273, 12673, 12675, 12674, 1434, 7923, 436, 7927,
		1397, 12642, 9775, 1410, 65369, 64313, 12424, 12681, 12520, 65430,
		12635, 12423, 12519, 65390, 1011, 12680, 12679, 3618, 3597, 9396,
		890, 837, 422, 7833, 696, 7929, 654, 12422, 12684, 12518,
		65429, 12640, 1131, 1133, 1127, 1129, 12421, 12517, 65389, 12683,
		12682, 2527, 2399, 122, 1382, 378, 2395, 2651, 65222, 65223,
		12374, 65224, 65200, 12470, 1429, 1428, 1432, 64310, 12567, 382,
		9449, 7825, 657, 380, 7827, 1177, 1247, 12380, 12476, 48,
		2534, 2406, 2790, 2662, 8320, 65296, 63280, 1776, 8304, 3664,
		65279, 8203, 950, 12563, 1386, 1218, 1175, 1245, 12376, 12472,
		1454, 7829, 65370, 12382, 12478, 9397, 656, 438, 12378, 12474
	};

	private string unicodeNames = "A,AE,AEacute,AEmacron,AEsmall,Aacute,Aacutesmall,Abreve,Abreveacute,Abrevecyrillic,Abrevedotbelow,Abrevegrave,Abrevehookabove,Abrevetilde,Acaron,Acircle,Acircumflex,Acircumflexacute,Acircumflexdotbelow,Acircumflexgrave,Acircumflexhookabove,Acircumflexsmall,Acircumflextilde,Acute,Acutesmall,afii10017,Adblgrave,Adieresis,Adieresiscyrillic,Adieresismacron,Adieresissmall,Adotbelow,Adotmacron,Agrave,Agravesmall,Ahookabove,Aiecyrillic,Ainvertedbreve,Alpha,Alphatonos,Amacron,Amonospace,Aogonek,Aring,Aringacute,Aringbelow,Aringsmall,Asmall,Atilde,Atildesmall,Aybarmenian,B,Bcircle,Bdotaccent,Bdotbelow,afii10018,Benarmenian,Beta,Bhook,Blinebelow,Bmonospace,Brevesmall,Bsmall,Btopbar,C,Caarmenian,Cacute,Caron,Caronsmall,Ccaron,Ccedilla,Ccedillaacute,Ccedillasmall,Ccircle,Ccircumflex,Cdotaccent,Cedillasmall,Chaarmenian,Cheabkhasiancyrillic,afii10041,Chedescenderabkhasiancyrillic,Chedescendercyrillic,Chedieresiscyrillic,Cheharmenian,Chekhakassiancyrillic,Cheverticalstrokecyrillic,Chi,Chook,Circumflexsmall,Cmonospace,Coarmenian,Csmall,D,DZ,DZcaron,Daarmenian,Dafrican,Dcaron,Dcedilla,Dcircle,Dcircumflexbelow,Dcroat,Ddotaccent,Ddotbelow,afii10021,Deicoptic,increment,Delta,Dhook,Dieresis,DieresisAcute,DieresisGrave,Dieresissmall,Digammagreek,afii10051,Dlinebelow,Dmonospace,Dotaccentsmall,Dsmall,Dtopbar,Dz,Dzcaron,Dzeabkhasiancyrillic,afii10054,afii10145,E,Eacute,Eacutesmall,Ebreve,Ecaron,Ecedillabreve,Echarmenian,Ecircle,Ecircumflex,Ecircumflexacute,Ecircumflexbelow,Ecircumflexdotbelow,Ecircumflexgrave,Ecircumflexhookabove,Ecircumflexsmall,Ecircumflextilde,afii10053,Edblgrave,Edieresis,Edieresissmall,Edotaccent,Edotbelow,afii10038,Egrave,Egravesmall,Eharmenian,Ehookabove,Eightroman,Einvertedbreve,Eiotifiedcyrillic,afii10029,Elevenroman,Emacron,Emacronacute,Emacrongrave,afii10030,Emonospace,afii10031,Endescendercyrillic,Eng,Enghecyrillic,Enhookcyrillic,Eogonek,Eopen,Epsilon,Epsilontonos,afii10034,Ereversed,afii10047,afii10035,Esdescendercyrillic,Esh,Esmall,Eta,Etarmenian,Etatonos,Eth,Ethsmall,Etilde,Etildebelow,Euro,Ezh,Ezhcaron,Ezhreversed,F,Fcircle,Fdotaccent,Feharmenian,Feicoptic,Fhook,afii10147,Fiveroman,Fmonospace,Fourroman,Fsmall,G,GBsquare,Gacute,Gamma,Gammaafrican,Gangiacoptic,Gbreve,Gcaron,Gcommaaccent,Gcircle,Gcircumflex,Gdotaccent,afii10020,Ghadarmenian,Ghemiddlehookcyrillic,Ghestrokecyrillic,afii10050,Ghook,Gimarmenian,afii10052,Gmacron,Gmonospace,Grave,Gravesmall,Gsmall,Gsmallhook,Gstroke,H,H18533,H18543,H18551,H22073,HPsquare,Haabkhasiancyrillic,Hadescendercyrillic,afii10044,Hbar,Hbrevebelow,Hcedilla,Hcircle,Hcircumflex,Hdieresis,Hdotaccent,Hdotbelow,Hmonospace,Hoarmenian,Horicoptic,Hsmall,Hungarumlaut,Hungarumlautsmall,Hzsquare,I,afii10049,IJ,afii10048,Iacute,Iacutesmall,Ibreve,Icaron,Icircle,Icircumflex,Icircumflexsmall,afii10055,Idblgrave,Idieresis,Idieresisacute,Idieresiscyrillic,Idieresissmall,Idotaccent,Idotbelow,Iebrevecyrillic,afii10022,Ifraktur,Igrave,Igravesmall,Ihookabove,afii10026,Iinvertedbreve,afii10027,Imacron,Imacroncyrillic,Imonospace,Iniarmenian,afii10023,Iogonek,Iota,Iotaafrican,Iotadieresis,Iotatonos,Ismall,Istroke,Itilde,Itildebelow,afii10148,Izhitsadblgravecyrillic,J,Jaarmenian,Jcircle,Jcircumflex,afii10057,Jheharmenian,Jmonospace,Jsmall,K,KBsquare,KKsquare,Kabashkircyrillic,Kacute,afii10028,Kadescendercyrillic,Kahookcyrillic,Kappa,Kastrokecyrillic,Kaverticalstrokecyrillic,Kcaron,Kcommaaccent,Kcircle,Kdotbelow,Keharmenian,Kenarmenian,afii10039,Kheicoptic,Khook,afii10061,Klinebelow,Kmonospace,Koppacyrillic,Koppagreek,Ksicyrillic,Ksmall,L,LJ,LL,Lacute,Lambda,Lcaron,Lcommaaccent,Lcircle,Lcircumflexbelow,Ldot,Ldotbelow,Ldotbelowmacron,Liwnarmenian,Lj,afii10058,Llinebelow,Lmonospace,Lslash,Lslashsmall,Lsmall,M,MBsquare,Macron,Macronsmall,Macute,Mcircle,Mdotaccent,Mdotbelow,Menarmenian,Mmonospace,Msmall,Mturned,Mu,N,NJ,Nacute,Ncaron,Ncommaaccent,Ncircle,Ncircumflexbelow,Ndotaccent,Ndotbelow,Nhookleft,Nineroman,Nj,afii10059,Nlinebelow,Nmonospace,Nowarmenian,Nsmall,Ntilde,Ntildesmall,Nu,O,OE,OEsmall,Oacute,Oacutesmall,Obarredcyrillic,Obarreddieresiscyrillic,Obreve,Ocaron,Ocenteredtilde,Ocircle,Ocircumflex,Ocircumflexacute,Ocircumflexdotbelow,Ocircumflexgrave,Ocircumflexhookabove,Ocircumflexsmall,Ocircumflextilde,afii10032,Ohungarumlaut,Odblgrave,Odieresis,Odieresiscyrillic,Odieresissmall,Odotbelow,Ogoneksmall,Ograve,Ogravesmall,Oharmenian,Omega,Ohookabove,Ohorn,Ohornacute,Ohorndotbelow,Ohorngrave,Ohornhookabove,Ohorntilde,Oi,Oinvertedbreve,Omacron,Omacronacute,Omacrongrave,Omegacyrillic,Omega,Omegaroundcyrillic,Omegatitlocyrillic,Omegatonos,Omicron,Omicrontonos,Omonospace,Oneroman,Oogonek,Oogonekmacron,Oopen,Oslash,Oslashacute,Oslashsmall,Osmall,Otcyrillic,Otilde,Otildeacute,Otildedieresis,Otildesmall,P,Pacute,Pcircle,Pdotaccent,afii10033,Peharmenian,Pemiddlehookcyrillic,Phi,Phook,Pi,Piwrarmenian,Pmonospace,Psi,Psicyrillic,Psmall,Q,Qcircle,Qmonospace,Qsmall,R,Raarmenian,Racute,Rcaron,Rcommaaccent,Rcircle,Rdblgrave,Rdotaccent,Rdotbelow,Rdotbelowmacron,Reharmenian,Rfraktur,Rho,Ringsmall,Rinvertedbreve,Rlinebelow,Rmonospace,Rsmall,Rsmallinverted,Rsmallinvertedsuperior,S,SF010000,SF020000,SF030000,SF040000,SF050000,SF060000,SF070000,SF080000,SF090000,SF100000,SF110000,SF190000,SF200000,SF210000,SF220000,SF230000,SF240000,SF250000,SF260000,SF270000,SF280000,SF360000,SF370000,SF380000,SF390000,SF400000,SF410000,SF420000,SF430000,SF440000,SF450000,SF460000,SF470000,SF480000,SF490000,SF500000,SF510000,SF520000,SF530000,SF540000,Sacute,Sacutedotaccent,Sampigreek,Scaron,Scarondotaccent,Scaronsmall,Scedilla,Schwa,Schwacyrillic,Schwadieresiscyrillic,Scircle,Scircumflex,Scommaaccent,Sdotaccent,Sdotbelow,Sdotbelowdotaccent,Seharmenian,Sevenroman,Shaarmenian,afii10042,afii10043,Sheicoptic,Shhacyrillic,Shimacoptic,Sigma,Sixroman,Smonospace,afii10046,Ssmall,Stigmagreek,T,Tau,Tbar,Tcaron,Tcommaaccent,Tcircle,Tcircumflexbelow,Tdotaccent,Tdotbelow,afii10036,Tedescendercyrillic,Tenroman,Tetsecyrillic,Theta,Thook,Thorn,Thornsmall,Threeroman,Tildesmall,Tiwnarmenian,Tlinebelow,Tmonospace,Toarmenian,Tonefive,Tonesix,Tonetwo,Tretroflexhook,afii10040,afii10060,Tsmall,Twelveroman,Tworoman,U,Uacute,Uacutesmall,Ubreve,Ucaron,Ucircle,Ucircumflex,Ucircumflexbelow,Ucircumflexsmall,afii10037,Uhungarumlaut,Udblgrave,Udieresis,Udieresisacute,Udieresisbelow,Udieresiscaron,Udieresiscyrillic,Udieresisgrave,Udieresismacron,Udieresissmall,Udotbelow,Ugrave,Ugravesmall,Uhookabove,Uhorn,Uhornacute,Uhorndotbelow,Uhorngrave,Uhornhookabove,Uhorntilde,Uhungarumlautcyrillic,Uinvertedbreve,Ukcyrillic,Umacron,Umacroncyrillic,Umacrondieresis,Umonospace,Uogonek,Upsilon,Upsilon1,Upsilonacutehooksymbolgreek,Upsilonafrican,Upsilondieresis,Upsilondieresishooksymbolgreek,Upsilontonos,Uring,afii10062,Usmall,Ustraightcyrillic,Ustraightstrokecyrillic,Utilde,Utildeacute,Utildebelow,V,Vcircle,Vdotbelow,afii10019,Vewarmenian,Vhook,Vmonospace,Voarmenian,Vsmall,Vtilde,W,Wacute,Wcircle,Wcircumflex,Wdieresis,Wdotaccent,Wdotbelow,Wgrave,Wmonospace,Wsmall,X,Xcircle,Xdieresis,Xdotaccent,Xeharmenian,Xi,Xmonospace,Xsmall,Y,Yacute,Yacutesmall,afii10146,Ycircle,Ycircumflex,Ydieresis,Ydieresissmall,Ydotaccent,Ydotbelow,afii10045,Yerudieresiscyrillic,Ygrave,Yhook,Yhookabove,Yiarmenian,afii10056,Yiwnarmenian,Ymonospace,Ysmall,Ytilde,Yusbigcyrillic,Yusbigiotifiedcyrillic,Yuslittlecyrillic,Yuslittleiotifiedcyrillic,Z,Zaarmenian,Zacute,Zcaron,Zcaronsmall,Zcircle,Zcircumflex,Zdotaccent,Zdotbelow,afii10025,Zedescendercyrillic,Zedieresiscyrillic,Zeta,Zhearmenian,Zhebrevecyrillic,afii10024,Zhedescendercyrillic,Zhedieresiscyrillic,Zlinebelow,Zmonospace,Zsmall,Zstroke,a,aabengali,aacute,aadeva,aagujarati,aagurmukhi,aamatragurmukhi,aarusquare,aavowelsignbengali,aavowelsigndeva,aavowelsigngujarati,abbreviationmarkarmenian,abbreviationsigndeva,abengali,abopomofo,abreve,abreveacute,abrevecyrillic,abrevedotbelow,abrevegrave,abrevehookabove,abrevetilde,acaron,acircle,acircumflex,acircumflexacute,acircumflexdotbelow,acircumflexgrave,acircumflexhookabove,acircumflextilde,acute,acutebelowcmb,acutecomb,acutedeva,acutelowmod,acutetonecmb,afii10065,adblgrave,addakgurmukhi,adeva,adieresis,adieresiscyrillic,adieresismacron,adotbelow,adotmacron,ae,aeacute,aekorean,aemacron,afii00208,lira,afii10063,afii10064,afii10066,afii10067,afii10068,afii10069,afii10070,afii10071,afii10072,afii10073,afii10074,afii10075,afii10076,afii10077,afii10078,afii10079,afii10080,afii10081,afii10082,afii10083,afii10084,afii10085,afii10086,afii10087,afii10088,afii10089,afii10090,afii10091,afii10092,afii10093,afii10094,afii10095,afii10096,afii10097,afii10098,afii10099,afii10100,afii10101,afii10102,afii10103,afii10104,afii10105,afii10106,afii10107,afii10108,afii10109,afii10110,afii10192,afii10193,afii10194,afii10195,afii10196,afii10831,afii10832,afii10846,afii299,afii300,afii301,afii57381,afii57388,afii57392,afii57393,afii57394,afii57395,afii57396,afii57397,afii57398,afii57399,afii57400,afii57401,afii57403,afii57407,afii57409,afii57410,afii57411,afii57412,afii57413,afii57414,afii57415,afii57416,afii57417,afii57418,afii57419,afii57420,afii57421,afii57422,afii57423,afii57424,afii57425,afii57426,afii57427,afii57428,afii57429,afii57430,afii57431,afii57432,afii57433,afii57434,afii57440,afii57441,afii57442,afii57443,afii57444,afii57445,afii57446,afii57448,afii57449,afii57450,afii57451,afii57452,afii57453,afii57454,afii57455,afii57456,afii57457,afii57458,afii57470,afii57505,afii57506,afii57507,afii57508,afii57509,afii57511,afii57512,afii57513,afii57514,afii57519,afii57534,afii57636,afii57645,afii57658,afii57664,afii57665,afii57666,afii57667,afii57668,afii57669,afii57670,afii57671,afii57672,afii57673,afii57674,afii57675,afii57676,afii57677,afii57678,afii57679,afii57680,afii57681,afii57682,afii57683,afii57684,afii57685,afii57686,afii57687,afii57688,afii57689,afii57690,afii57694,afii57695,afii57700,afii57705,afii57716,afii57717,afii57718,afii57723,afii57793,afii57794,afii57795,afii57796,afii57797,afii57798,afii57799,afii57800,afii57801,afii57802,afii57803,afii57804,afii57806,afii57807,afii57839,afii57841,afii57842,afii57929,afii61248,afii61289,afii61352,afii61573,afii61574,afii61575,afii61664,afii63167,afii64937,agrave,agujarati,agurmukhi,ahiragana,ahookabove,aibengali,aibopomofo,aideva,aiecyrillic,aigujarati,aigurmukhi,aimatragurmukhi,ainfinalarabic,aininitialarabic,ainmedialarabic,ainvertedbreve,aivowelsignbengali,aivowelsigndeva,aivowelsigngujarati,akatakana,akatakanahalfwidth,akorean,alefdageshhebrew,aleffinalarabic,alefhamzaabovefinalarabic,alefhamzabelowfinalarabic,aleflamedhebrew,alefmaddaabovefinalarabic,alefmaksurafinalarabic,yehinitialarabic,yehmedialarabic,alefpatahhebrew,alefqamatshebrew,aleph,allequal,alpha,alphatonos,amacron,amonospace,ampersand,ampersandmonospace,ampersandsmall,amsquare,anbopomofo,angbopomofo,angkhankhuthai,angle,anglebracketleft,anglebracketleftvertical,anglebracketright,anglebracketrightvertical,angleleft,angleright,angstrom,anoteleia,anudattadeva,anusvarabengali,anusvaradeva,anusvaragujarati,aogonek,apaatosquare,aparen,apostrophearmenian,apple,approaches,approxequal,approxequalorimage,congruent,araeaekorean,araeakorean,arc,arighthalfring,aring,aringacute,aringbelow,arrowboth,arrowdashdown,arrowdashleft,arrowdashright,arrowdashup,arrowdblboth,arrowdbldown,arrowdblleft,arrowdblright,arrowdblup,arrowdown,arrowdownleft,arrowdownright,arrowdownwhite,arrowheaddownmod,arrowheadleftmod,arrowheadrightmod,arrowheadupmod,arrowhorizex,arrowleft,arrowleftdblstroke,arrowleftoverright,arrowleftwhite,arrowright,arrowrightdblstroke,arrowrightheavy,arrowrightoverleft,arrowrightwhite,arrowtableft,arrowtabright,arrowup,arrowupdn,arrowupdnbse,arrowupleft,arrowupleftofdown,arrowupright,arrowupwhite,arrowvertex,asciicircum,asciicircummonospace,asciitilde,asciitildemonospace,ascript,ascriptturned,asmallhiragana,asmallkatakana,asmallkatakanahalfwidth,asterisk,asteriskmath,asteriskmonospace,asterisksmall,asterism,asuperior,asymptoticallyequal,at,atilde,atmonospace,atsmall,aturned,aubengali,aubopomofo,audeva,augujarati,augurmukhi,aulengthmarkbengali,aumatragurmukhi,auvowelsignbengali,auvowelsigndeva,auvowelsigngujarati,avagrahadeva,aybarmenian,ayinaltonehebrew,b,babengali,backslash,backslashmonospace,badeva,bagujarati,bagurmukhi,bahiragana,bahtthai,bakatakana,bar,barmonospace,bbopomofo,bcircle,bdotaccent,bdotbelow,beamedsixteenthnotes,because,behfinalarabic,behinitialarabic,behiragana,behmedialarabic,behmeeminitialarabic,behmeemisolatedarabic,behnoonfinalarabic,bekatakana,benarmenian,beta,betasymbolgreek,betdageshhebrew,betrafehebrew,bhabengali,bhadeva,bhagujarati,bhagurmukhi,bhook,bihiragana,bikatakana,bilabialclick,bindigurmukhi,birusquare,blackdiamond,triagdn,triaglf,blackleftpointingtriangle,blacklenticularbracketleft,blacklenticularbracketleftvertical,blacklenticularbracketright,blacklenticularbracketrightvertical,blacklowerlefttriangle,blacklowerrighttriangle,filledrect,triagrt,blackrightpointingtriangle,invsmileface,filledbox,blackstar,blackupperlefttriangle,blackupperrighttriangle,blackuppointingsmalltriangle,triagup,blank,blinebelow,block,bmonospace,bobaimaithai,bohiragana,bokatakana,bparen,bqsquare,braceex,braceleft,braceleftbt,braceleftmid,braceleftmonospace,braceleftsmall,bracelefttp,braceleftvertical,braceright,bracerightbt,bracerightmid,bracerightmonospace,bracerightsmall,bracerighttp,bracerightvertical,bracketleft,bracketleftbt,bracketleftex,bracketleftmonospace,bracketlefttp,bracketright,bracketrightbt,bracketrightex,bracketrightmonospace,bracketrighttp,breve,brevebelowcmb,brevecmb,breveinvertedbelowcmb,breveinvertedcmb,breveinverteddoublecmb,bridgebelowcmb,bridgeinvertedbelowcmb,brokenbar,bstroke,bsuperior,btopbar,buhiragana,bukatakana,bullet,invbullet,bulletoperator,bullseye,c,caarmenian,cabengali,cacute,cadeva,cagujarati,cagurmukhi,calsquare,candrabindubengali,candrabinducmb,candrabindudeva,candrabindugujarati,capslock,caron,caronbelowcmb,caroncmb,carriagereturn,cbopomofo,ccaron,ccedilla,ccedillaacute,ccircle,ccircumflex,ccurl,cdotaccent,cdsquare,cedilla,cedillacmb,cent,centigrade,centinferior,centmonospace,centoldstyle,centsuperior,chaarmenian,chabengali,chadeva,chagujarati,chagurmukhi,chbopomofo,cheabkhasiancyrillic,checkmark,chedescenderabkhasiancyrillic,chedescendercyrillic,chedieresiscyrillic,cheharmenian,chekhakassiancyrillic,cheverticalstrokecyrillic,chi,chieuchacirclekorean,chieuchaparenkorean,chieuchcirclekorean,chieuchkorean,chieuchparenkorean,chochangthai,chochanthai,chochingthai,chochoethai,chook,cieucacirclekorean,cieucaparenkorean,cieuccirclekorean,cieuckorean,cieucparenkorean,cieucuparenkorean,circle,circlemultiply,circleot,circleplus,circlepostalmark,circlewithlefthalfblack,circlewithrighthalfblack,circumflex,circumflexbelowcmb,circumflexcmb,clear,clickalveolar,clickdental,clicklateral,clickretroflex,club,clubsuitwhite,cmcubedsquare,cmonospace,cmsquaredsquare,coarmenian,colon,colonmonetary,colonmonospace,colonsmall,colontriangularhalfmod,colontriangularmod,comma,commaabovecmb,commaaboverightcmb,commaaccent,commaarmenian,commainferior,commamonospace,commareversedabovecmb,commasmall,commasuperior,commaturnedabovecmb,commaturnedmod,sun,contourintegral,control,controlACK,controlBEL,controlBS,controlCAN,controlCR,controlDC1,controlDC2,controlDC3,controlDC4,controlDEL,controlDLE,controlEM,controlENQ,controlEOT,controlESC,controlETB,controlETX,controlFF,controlFS,controlGS,controlHT,controlLF,controlNAK,controlRS,controlSI,controlSO,controlSOT,controlSTX,controlSUB,controlSYN,controlUS,controlVT,copyright,copyrightsans,copyrightserif,cornerbracketleft,cornerbracketlefthalfwidth,cornerbracketleftvertical,cornerbracketright,cornerbracketrighthalfwidth,cornerbracketrightvertical,corporationsquare,cosquare,coverkgsquare,cparen,cruzeiro,cstretched,curlyand,curlyor,currency,cyrBreve,cyrFlex,cyrbreve,cyrflex,d,daarmenian,dabengali,dadeva,dadfinalarabic,dadinitialarabic,dadmedialarabic,dagger,daggerdbl,dagujarati,dagurmukhi,dahiragana,dakatakana,daletdageshhebrew,dalfinalarabic,danda,dargalefthebrew,dasiapneumatacyrilliccmb,dblGrave,dblanglebracketleft,dblanglebracketleftvertical,dblanglebracketright,dblanglebracketrightvertical,dblarchinvertedbelowcmb,dbldanda,dblgrave,dblgravecmb,dblintegral,underscoredbl,dbllowlinecmb,dbloverlinecmb,dblprimemod,dblverticalbar,dblverticallineabovecmb,dbopomofo,dbsquare,dcaron,dcedilla,dcircle,dcircumflexbelow,dcroat,ddabengali,ddadeva,ddagujarati,ddagurmukhi,ddalfinalarabic,dddhadeva,ddhabengali,ddhadeva,ddhagujarati,ddhagurmukhi,ddotaccent,ddotbelow,decimalseparatorpersian,degree,dehihebrew,dehiragana,deicoptic,dekatakana,deleteleft,deleteright,delta,deltaturned,denominatorminusonenumeratorbengali,dezh,dhabengali,dhadeva,dhagujarati,dhagurmukhi,dhook,dieresistonos,dialytikatonoscmb,diamond,diamondsuitwhite,dieresis,dieresisacute,dieresisbelowcmb,dieresiscmb,dieresisgrave,dihiragana,dikatakana,dittomark,divide,divides,divisionslash,dkshade,dlinebelow,dlsquare,dmonospace,dnblock,dochadathai,dodekthai,dohiragana,dokatakana,dollar,dollarinferior,dollarmonospace,dollaroldstyle,dollarsmall,dollarsuperior,dong,dorusquare,dotaccent,dotaccentcmb,dotbelowcomb,dotkatakana,dotlessi,dotlessj,dotlessjstrokehook,dotmath,dottedcircle,downtackbelowcmb,downtackmod,dparen,dsuperior,dtail,dtopbar,duhiragana,dukatakana,dz,dzaltone,dzcaron,dzcurl,dzeabkhasiancyrillic,e,eacute,earth,ebengali,ebopomofo,ebreve,ecandradeva,ecandragujarati,ecandravowelsigndeva,ecandravowelsigngujarati,ecaron,ecedillabreve,echarmenian,echyiwnarmenian,ecircle,ecircumflex,ecircumflexacute,ecircumflexbelow,ecircumflexdotbelow,ecircumflexgrave,ecircumflexhookabove,ecircumflextilde,edblgrave,edeva,edieresis,edotaccent,edotbelow,eegurmukhi,eematragurmukhi,egrave,egujarati,eharmenian,ehbopomofo,ehiragana,ehookabove,eibopomofo,eight,eightbengali,eightcircle,eightcircleinversesansserif,eightdeva,eighteencircle,eighteenparen,eighteenperiod,eightgujarati,eightgurmukhi,eighthangzhou,musicalnotedbl,eightideographicparen,eightinferior,eightmonospace,eightoldstyle,eightparen,eightperiod,eightpersian,eightroman,eightsuperior,eightthai,einvertedbreve,eiotifiedcyrillic,ekatakana,ekatakanahalfwidth,ekonkargurmukhi,ekorean,element,elevencircle,elevenparen,elevenperiod,elevenroman,ellipsis,ellipsisvertical,emacron,emacronacute,emacrongrave,emdash,emdashvertical,emonospace,emphasismarkarmenian,emptyset,enbopomofo,endash,endashvertical,endescendercyrillic,eng,engbopomofo,enghecyrillic,enhookcyrillic,enspace,eogonek,eokorean,eopen,eopenclosed,eopenreversed,eopenreversedclosed,eopenreversedhook,eparen,epsilon,epsilontonos,equal,equalmonospace,equalsmall,equalsuperior,equivalence,erbopomofo,ereversed,esdescendercyrillic,esh,eshcurl,eshortdeva,eshortvowelsigndeva,eshreversedloop,eshsquatreversed,esmallhiragana,esmallkatakana,esmallkatakanahalfwidth,estimated,esuperior,eta,etarmenian,etatonos,eth,etilde,etildebelow,etnahtalefthebrew,eturned,eukorean,evowelsignbengali,evowelsigndeva,evowelsigngujarati,exclam,exclamarmenian,exclamdbl,exclamdown,exclamdownsmall,exclammonospace,exclamsmall,existential,ezh,ezhcaron,ezhcurl,ezhreversed,ezhtail,f,fadeva,fagurmukhi,fahrenheit,fbopomofo,fcircle,fdotaccent,feharmenian,fehfinalarabic,fehinitialarabic,fehmedialarabic,feicoptic,female,ff,ffi,ffl,fi,fifteencircle,fifteenparen,fifteenperiod,figuredash,finalkafdageshhebrew,firsttonechinese,fisheye,five,fivebengali,fivecircle,fivecircleinversesansserif,fivedeva,fiveeighths,fivegujarati,fivegurmukhi,fivehangzhou,fiveideographicparen,fiveinferior,fivemonospace,fiveoldstyle,fiveparen,fiveperiod,fivepersian,fiveroman,fivesuperior,fivethai,fl,florin,fmonospace,fmsquare,fofanthai,fofathai,fongmanthai,universal,four,fourbengali,fourcircle,fourcircleinversesansserif,fourdeva,fourgujarati,fourgurmukhi,fourhangzhou,fourideographicparen,fourinferior,fourmonospace,fournumeratorbengali,fouroldstyle,fourparen,fourperiod,fourpersian,fourroman,foursuperior,fourteencircle,fourteenparen,fourteenperiod,fourthai,fourthtonechinese,fparen,fraction,franc,g,gabengali,gacute,gadeva,gaffinalarabic,gafinitialarabic,gafmedialarabic,gagujarati,gagurmukhi,gahiragana,gakatakana,gamma,gammalatinsmall,gammasuperior,gangiacoptic,gbopomofo,gbreve,gcaron,gcommaaccent,gcircle,gcircumflex,gdotaccent,gehiragana,gekatakana,geometricallyequal,gereshaccenthebrew,gereshhebrew,gereshmuqdamhebrew,germandbls,gershayimaccenthebrew,gershayimhebrew,getamark,ghabengali,ghadarmenian,ghadeva,ghagujarati,ghagurmukhi,ghainfinalarabic,ghaininitialarabic,ghainmedialarabic,ghemiddlehookcyrillic,ghestrokecyrillic,ghhadeva,ghhagurmukhi,ghook,ghzsquare,gihiragana,gikatakana,gimarmenian,gimeldageshhebrew,glottalinvertedstroke,glottalstop,glottalstopinverted,glottalstopmod,glottalstopreversed,glottalstopreversedmod,glottalstopreversedsuperior,glottalstopstroke,glottalstopstrokereversed,gmacron,gmonospace,gohiragana,gokatakana,gparen,gpasquare,gradient,grave,gravebelowcmb,gravecomb,gravedeva,gravelowmod,gravemonospace,gravetonecmb,greater,greaterequal,greaterequalorless,greatermonospace,greaterorequivalent,greaterorless,greateroverequal,greatersmall,gscript,gstroke,guhiragana,guillemotleft,guillemotright,guilsinglleft,guilsinglright,gukatakana,guramusquare,gysquare,h,haabkhasiancyrillic,hehaltonearabic,habengali,hadescendercyrillic,hadeva,hagujarati,hagurmukhi,hahfinalarabic,hahinitialarabic,hahiragana,hahmedialarabic,haitusquare,hakatakana,hakatakanahalfwidth,halantgurmukhi,hangulfiller,harpoonleftbarbup,harpoonrightbarbup,hasquare,hbar,hbopomofo,hbrevebelow,hcedilla,hcircle,hcircumflex,hdieresis,hdotaccent,hdotbelow,heart,heartsuitwhite,hedageshhebrew,hehfinalaltonearabic,hehfinalarabic,hehhamzaabovefinalarabic,hehhamzaaboveisolatedarabic,hehinitialaltonearabic,hehinitialarabic,hehiragana,hehmedialaltonearabic,hehmedialarabic,heiseierasquare,hekatakana,hekatakanahalfwidth,hekutaarusquare,henghook,herutusquare,hhook,hhooksuperior,hieuhacirclekorean,hieuhaparenkorean,hieuhcirclekorean,hieuhkorean,hieuhparenkorean,hihiragana,hikatakana,hikatakanahalfwidth,hlinebelow,hmonospace,hoarmenian,hohipthai,hohiragana,hokatakana,hokatakanahalfwidth,honokhukthai,hookabovecomb,hookpalatalizedbelowcmb,hookretroflexbelowcmb,hoonsquare,horicoptic,horncmb,hotsprings,house,hparen,hsuperior,hturned,huhiragana,huiitosquare,hukatakana,hukatakanahalfwidth,hungarumlaut,hungarumlautcmb,hv,hyphen,hypheninferior,hyphenmonospace,hyphensmall,hyphensuperior,hyphentwo,i,iacute,ibengali,ibopomofo,ibreve,icaron,icircle,icircumflex,idblgrave,ideographearthcircle,ideographfirecircle,ideographicallianceparen,ideographiccallparen,ideographiccentrecircle,ideographicclose,ideographiccomma,ideographiccommaleft,ideographiccongratulationparen,ideographiccorrectcircle,ideographicearthparen,ideographicenterpriseparen,ideographicexcellentcircle,ideographicfestivalparen,ideographicfinancialcircle,ideographicfinancialparen,ideographicfireparen,ideographichaveparen,ideographichighcircle,ideographiciterationmark,ideographiclaborcircle,ideographiclaborparen,ideographicleftcircle,ideographiclowcircle,ideographicmedicinecircle,ideographicmetalparen,ideographicmoonparen,ideographicnameparen,ideographicperiod,ideographicprintcircle,ideographicreachparen,ideographicrepresentparen,ideographicresourceparen,ideographicrightcircle,ideographicsecretcircle,ideographicselfparen,ideographicsocietyparen,ideographicspace,ideographicspecialparen,ideographicstockparen,ideographicstudyparen,ideographicsunparen,ideographicsuperviseparen,ideographicwaterparen,ideographicwoodparen,ideographiczero,ideographmetalcircle,ideographmooncircle,ideographnamecircle,ideographsuncircle,ideographwatercircle,ideographwoodcircle,ideva,idieresis,idieresisacute,idieresiscyrillic,idotbelow,iebrevecyrillic,ieungacirclekorean,ieungaparenkorean,ieungcirclekorean,ieungkorean,ieungparenkorean,igrave,igujarati,igurmukhi,ihiragana,ihookabove,iibengali,iideva,iigujarati,iigurmukhi,iimatragurmukhi,iinvertedbreve,iivowelsignbengali,iivowelsigndeva,iivowelsigngujarati,ij,ikatakana,ikatakanahalfwidth,ikorean,tilde,iluyhebrew,imacron,imacroncyrillic,imageorapproximatelyequal,imatragurmukhi,imonospace,infinity,iniarmenian,integral,integralbt,integralex,integraltp,intersection,intisquare,invcircle,iogonek,iota,iotadieresis,iotadieresistonos,iotalatin,iotatonos,iparen,irigurmukhi,ismallhiragana,ismallkatakana,ismallkatakanahalfwidth,issharbengali,istroke,isuperior,iterationhiragana,iterationkatakana,itilde,itildebelow,iubopomofo,ivowelsignbengali,ivowelsigndeva,ivowelsigngujarati,izhitsadblgravecyrillic,j,jaarmenian,jabengali,jadeva,jagujarati,jagurmukhi,jbopomofo,jcaron,jcircle,jcircumflex,jcrossedtail,jdotlessstroke,jeemfinalarabic,jeeminitialarabic,jeemmedialarabic,jehfinalarabic,jhabengali,jhadeva,jhagujarati,jhagurmukhi,jheharmenian,jis,jmonospace,jparen,jsuperior,k,kabashkircyrillic,kabengali,kacute,kadescendercyrillic,kadeva,kafdageshhebrew,kaffinalarabic,kafinitialarabic,kafmedialarabic,kafrafehebrew,kagujarati,kagurmukhi,kahiragana,kahookcyrillic,kakatakana,kakatakanahalfwidth,kappa,kappasymbolgreek,kapyeounmieumkorean,kapyeounphieuphkorean,kapyeounpieupkorean,kapyeounssangpieupkorean,karoriisquare,kasmallkatakana,kasquare,kastrokecyrillic,katahiraprolongmarkhalfwidth,kaverticalstrokecyrillic,kbopomofo,kcalsquare,kcaron,kcommaaccent,kcircle,kdotbelow,keharmenian,kehiragana,kekatakana,kekatakanahalfwidth,kenarmenian,kesmallkatakana,kgreenlandic,khabengali,khadeva,khagujarati,khagurmukhi,khahfinalarabic,khahinitialarabic,khahmedialarabic,kheicoptic,khhadeva,khhagurmukhi,khieukhacirclekorean,khieukhaparenkorean,khieukhcirclekorean,khieukhkorean,khieukhparenkorean,khokhaithai,khokhonthai,khokhuatthai,khokhwaithai,khomutthai,khook,khorakhangthai,khzsquare,kihiragana,kikatakana,kikatakanahalfwidth,kiroguramusquare,kiromeetorusquare,kirosquare,kiyeokacirclekorean,kiyeokaparenkorean,kiyeokcirclekorean,kiyeokkorean,kiyeokparenkorean,kiyeoksioskorean,klinebelow,klsquare,kmcubedsquare,kmonospace,kmsquaredsquare,kohiragana,kohmsquare,kokaithai,kokatakana,kokatakanahalfwidth,kooposquare,koppacyrillic,koreanstandardsymbol,koroniscmb,kparen,kpasquare,ksicyrillic,ktsquare,kturned,kuhiragana,kukatakana,kukatakanahalfwidth,kvsquare,kwsquare,l,labengali,lacute,ladeva,lagujarati,lagurmukhi,lakkhangyaothai,lamaleffinalarabic,lamalefhamzaabovefinalarabic,lamalefhamzaaboveisolatedarabic,lamalefhamzabelowfinalarabic,lamalefhamzabelowisolatedarabic,lamalefisolatedarabic,lamalefmaddaabovefinalarabic,lamalefmaddaaboveisolatedarabic,lambda,lambdastroke,lameddageshhebrew,lamfinalarabic,lamhahinitialarabic,lammeemkhahinitialarabic,lamjeeminitialarabic,lamkhahinitialarabic,lamlamhehisolatedarabic,lammedialarabic,lammeemhahinitialarabic,lammeeminitialarabic,largecircle,lbar,lbelt,lbopomofo,lcaron,lcommaaccent,lcircle,lcircumflexbelow,ldot,ldotbelow,ldotbelowmacron,leftangleabovecmb,lefttackbelowcmb,less,lessequal,lessequalorgreater,lessmonospace,lessorequivalent,lessorgreater,lessoverequal,lesssmall,lezh,lfblock,lhookretroflex,liwnarmenian,lj,ll,lladeva,llagujarati,llinebelow,llladeva,llvocalicbengali,llvocalicdeva,llvocalicvowelsignbengali,llvocalicvowelsigndeva,lmiddletilde,lmonospace,lmsquare,lochulathai,logicaland,logicalnot,revlogicalnot,logicalor,lolingthai,longs,lowlinecenterline,lowlinecmb,lowlinedashed,lozenge,lparen,lslash,lsuperior,ltshade,luthai,lvocalicbengali,lvocalicdeva,lvocalicvowelsignbengali,lvocalicvowelsigndeva,lxsquare,m,mabengali,macron,macronbelowcmb,macroncmb,macronlowmod,macronmonospace,macute,madeva,magujarati,magurmukhi,mahapakhlefthebrew,mahiragana,maichattawalowleftthai,maichattawalowrightthai,maichattawathai,maichattawaupperleftthai,maieklowleftthai,maieklowrightthai,maiekthai,maiekupperleftthai,maihanakatleftthai,maihanakatthai,maitaikhuleftthai,maitaikhuthai,maitholowleftthai,maitholowrightthai,maithothai,maithoupperleftthai,maitrilowleftthai,maitrilowrightthai,maitrithai,maitriupperleftthai,maiyamokthai,makatakana,makatakanahalfwidth,male,mansyonsquare,masoracirclehebrew,masquare,mbopomofo,mbsquare,mcircle,mcubedsquare,mdotaccent,mdotbelow,meemfinalarabic,meeminitialarabic,meemmedialarabic,meemmeeminitialarabic,meemmeemisolatedarabic,meetorusquare,mehiragana,meizierasquare,mekatakana,mekatakanahalfwidth,memdageshhebrew,menarmenian,merkhalefthebrew,merkhakefulalefthebrew,mhook,mhzsquare,middledotkatakanahalfwidth,periodcentered,mieumacirclekorean,mieumaparenkorean,mieumcirclekorean,mieumkorean,mieumpansioskorean,mieumparenkorean,mieumpieupkorean,mieumsioskorean,mihiragana,mikatakana,mikatakanahalfwidth,minus,minusbelowcmb,minuscircle,minusmod,minusplus,minute,miribaarusquare,mirisquare,mlonglegturned,mlsquare,mmcubedsquare,mmonospace,mmsquaredsquare,mohiragana,mohmsquare,mokatakana,mokatakanahalfwidth,molsquare,momathai,moverssquare,moverssquaredsquare,mparen,mpasquare,mssquare,msuperior,mturned,mu,muasquare,muchgreater,muchless,mufsquare,mu,mugsquare,muhiragana,mukatakana,mukatakanahalfwidth,mulsquare,multiply,mumsquare,munahlefthebrew,musicalnote,musicflatsign,musicsharpsign,mussquare,muvsquare,muwsquare,mvmegasquare,mvsquare,mwmegasquare,mwsquare,n,nabengali,nacute,nadeva,nagujarati,nagurmukhi,nahiragana,nakatakana,nakatakanahalfwidth,napostrophe,nasquare,nbopomofo,nonbreakingspace,ncaron,ncommaaccent,ncircle,ncircumflexbelow,ndotaccent,ndotbelow,nehiragana,nekatakana,nekatakanahalfwidth,nfsquare,ngabengali,ngadeva,ngagujarati,ngagurmukhi,ngonguthai,nhiragana,nhookleft,nhookretroflex,nieunacirclekorean,nieunaparenkorean,nieuncieuckorean,nieuncirclekorean,nieunhieuhkorean,nieunkorean,nieunpansioskorean,nieunparenkorean,nieunsioskorean,nieuntikeutkorean,nihiragana,nikatakana,nikatakanahalfwidth,nikhahitleftthai,nikhahitthai,nine,ninebengali,ninecircle,ninecircleinversesansserif,ninedeva,ninegujarati,ninegurmukhi,ninehangzhou,nineideographicparen,nineinferior,ninemonospace,nineoldstyle,nineparen,nineperiod,ninepersian,nineroman,ninesuperior,nineteencircle,nineteenparen,nineteenperiod,ninethai,nj,nkatakana,nkatakanahalfwidth,nlegrightlong,nlinebelow,nmonospace,nmsquare,nnabengali,nnadeva,nnagujarati,nnagurmukhi,nnnadeva,nohiragana,nokatakana,nokatakanahalfwidth,nonenthai,nonuthai,noonfinalarabic,noonghunnafinalarabic,nooninitialarabic,noonjeeminitialarabic,noonjeemisolatedarabic,noonmedialarabic,noonmeeminitialarabic,noonmeemisolatedarabic,noonnoonfinalarabic,notcontains,notelement,notequal,notgreater,notgreaternorequal,notgreaternorless,notidentical,notless,notlessnorequal,notparallel,notprecedes,notsubset,notsucceeds,notsuperset,nowarmenian,nparen,nssquare,nsuperior,ntilde,nu,nuhiragana,nukatakana,nukatakanahalfwidth,nuktabengali,nuktadeva,nuktagujarati,nuktagurmukhi,numbersign,numbersignmonospace,numbersignsmall,numeralsigngreek,numeralsignlowergreek,nundageshhebrew,nvsquare,nwsquare,nyabengali,nyadeva,nyagujarati,nyagurmukhi,o,oacute,oangthai,obarred,obarredcyrillic,obarreddieresiscyrillic,obengali,obopomofo,obreve,ocandradeva,ocandragujarati,ocandravowelsigndeva,ocandravowelsigngujarati,ocaron,ocircle,ocircumflex,ocircumflexacute,ocircumflexdotbelow,ocircumflexgrave,ocircumflexhookabove,ocircumflextilde,ohungarumlaut,odblgrave,odeva,odieresis,odieresiscyrillic,odotbelow,oe,oekorean,ogonek,ogonekcmb,ograve,ogujarati,oharmenian,ohiragana,ohookabove,ohorn,ohornacute,ohorndotbelow,ohorngrave,ohornhookabove,ohorntilde,oi,oinvertedbreve,okatakana,okatakanahalfwidth,okorean,olehebrew,omacron,omacronacute,omacrongrave,omdeva,omega,omega1,omegacyrillic,omegalatinclosed,omegaroundcyrillic,omegatitlocyrillic,omegatonos,omgujarati,omicron,omicrontonos,omonospace,one,onebengali,onecircle,onecircleinversesansserif,onedeva,onedotenleader,oneeighth,onefitted,onegujarati,onegurmukhi,onehalf,onehangzhou,oneideographicparen,oneinferior,onemonospace,onenumeratorbengali,oneoldstyle,oneparen,oneperiod,onepersian,onequarter,oneroman,onesuperior,onethai,onethird,oogonek,oogonekmacron,oogurmukhi,oomatragurmukhi,oopen,oparen,openbullet,option,ordfeminine,ordmasculine,orthogonal,oshortdeva,oshortvowelsigndeva,oslash,oslashacute,osmallhiragana,osmallkatakana,osmallkatakanahalfwidth,osuperior,otcyrillic,otilde,otildeacute,otildedieresis,oubopomofo,overline,overlinecenterline,overlinecmb,overlinedashed,overlinedblwavy,overlinewavy,ovowelsignbengali,ovowelsigndeva,ovowelsigngujarati,p,paampssquare,paasentosquare,pabengali,pacute,padeva,pagedown,pageup,pagujarati,pagurmukhi,pahiragana,paiyannoithai,pakatakana,palatalizationcyrilliccmb,palochkacyrillic,pansioskorean,paragraph,parallel,parenleft,parenleftaltonearabic,parenleftbt,parenleftex,parenleftinferior,parenleftmonospace,parenleftsmall,parenleftsuperior,parenlefttp,parenleftvertical,parenright,parenrightaltonearabic,parenrightbt,parenrightex,parenrightinferior,parenrightmonospace,parenrightsmall,parenrightsuperior,parenrighttp,parenrightvertical,partialdiff,pashtahebrew,pasquare,pazerhebrew,pbopomofo,pcircle,pdotaccent,pedageshhebrew,peezisquare,pefinaldageshhebrew,peharmenian,pehfinalarabic,pehinitialarabic,pehiragana,pehmedialarabic,pekatakana,pemiddlehookcyrillic,perafehebrew,percent,percentmonospace,percentsmall,period,periodarmenian,periodhalfwidth,periodinferior,periodmonospace,periodsmall,periodsuperior,perispomenigreekcmb,perpendicular,perthousand,peseta,pfsquare,phabengali,phadeva,phagujarati,phagurmukhi,phi,phi1,phieuphacirclekorean,phieuphaparenkorean,phieuphcirclekorean,phieuphkorean,phieuphparenkorean,philatin,phinthuthai,phook,phophanthai,phophungthai,phosamphaothai,pi,pieupacirclekorean,pieupaparenkorean,pieupcieuckorean,pieupcirclekorean,pieupkiyeokkorean,pieupkorean,pieupparenkorean,pieupsioskiyeokkorean,pieupsioskorean,pieupsiostikeutkorean,pieupthieuthkorean,pieuptikeutkorean,pihiragana,pikatakana,piwrarmenian,plus,plusbelowcmb,plusminus,plusmod,plusmonospace,plussmall,plussuperior,pmonospace,pmsquare,pohiragana,pointingindexdownwhite,pointingindexleftwhite,pointingindexrightwhite,pointingindexupwhite,pokatakana,poplathai,postalmark,postalmarkface,pparen,precedes,prescription,primemod,primereversed,product,projective,prolongedkana,propellor,propersubset,propersuperset,proportion,proportional,psi,psicyrillic,psilipneumatacyrilliccmb,pssquare,puhiragana,pukatakana,pvsquare,pwsquare,q,qadeva,qadmahebrew,qaffinalarabic,qafinitialarabic,qafmedialarabic,qarneyparahebrew,qbopomofo,qcircle,qhook,qmonospace,qofdageshhebrew,qparen,quarternote,question,questionarmenian,questiondown,questiondownsmall,questiongreek,questionmonospace,questionsmall,quotedbl,quotedblbase,quotedblleft,quotedblmonospace,quotedblprime,quotedblprimereversed,quotedblright,quoteleft,quotereversed,quoteright,quotesinglbase,quotesingle,quotesinglemonospace,r,raarmenian,rabengali,racute,radeva,radical,radicalex,radoverssquare,radoverssquaredsquare,radsquare,ragujarati,ragurmukhi,rahiragana,rakatakana,rakatakanahalfwidth,ralowerdiagonalbengali,ramiddlediagonalbengali,ramshorn,ratio,rbopomofo,rcaron,rcommaaccent,rcircle,rdblgrave,rdotaccent,rdotbelow,rdotbelowmacron,referencemark,reflexsubset,reflexsuperset,registered,registersans,registerserif,reharmenian,rehfinalarabic,rehiragana,rekatakana,rekatakanahalfwidth,reshdageshhebrew,reversedtilde,reviamugrashhebrew,rfishhook,rfishhookreversed,rhabengali,rhadeva,rho,rhook,rhookturned,rhookturnedsuperior,rhosymbolgreek,rhotichookmod,rieulacirclekorean,rieulaparenkorean,rieulcirclekorean,rieulhieuhkorean,rieulkiyeokkorean,rieulkiyeoksioskorean,rieulkorean,rieulmieumkorean,rieulpansioskorean,rieulparenkorean,rieulphieuphkorean,rieulpieupkorean,rieulpieupsioskorean,rieulsioskorean,rieulthieuthkorean,rieultikeutkorean,rieulyeorinhieuhkorean,righttackbelowcmb,righttriangle,rihiragana,rikatakana,rikatakanahalfwidth,ring,ringbelowcmb,ringcmb,ringhalfleft,ringhalfleftarmenian,ringhalfleftbelowcmb,ringhalfleftcentered,ringhalfright,ringhalfrightbelowcmb,ringhalfrightcentered,rinvertedbreve,rittorusquare,rlinebelow,rlongleg,rlonglegturned,rmonospace,rohiragana,rokatakana,rokatakanahalfwidth,roruathai,rparen,rrabengali,rradeva,rragurmukhi,rrehfinalarabic,rrvocalicbengali,rrvocalicdeva,rrvocalicgujarati,rrvocalicvowelsignbengali,rrvocalicvowelsigndeva,rrvocalicvowelsigngujarati,rsuperior,rtblock,rturned,rturnedsuperior,ruhiragana,rukatakana,rukatakanahalfwidth,rupeemarkbengali,rupeesignbengali,rupiah,ruthai,rvocalicbengali,rvocalicdeva,rvocalicgujarati,rvocalicvowelsignbengali,rvocalicvowelsigndeva,rvocalicvowelsigngujarati,s,sabengali,sacute,sacutedotaccent,sadeva,sadfinalarabic,sadinitialarabic,sadmedialarabic,sagujarati,sagurmukhi,sahiragana,sakatakana,sakatakanahalfwidth,sallallahoualayhewasallamarabic,samekhdageshhebrew,saraaathai,saraaethai,saraaimaimalaithai,saraaimaimuanthai,saraamthai,saraathai,saraethai,saraiileftthai,saraiithai,saraileftthai,saraithai,saraothai,saraueeleftthai,saraueethai,saraueleftthai,sarauethai,sarauthai,sarauuthai,sbopomofo,scaron,scarondotaccent,scedilla,schwa,schwadieresiscyrillic,schwahook,scircle,scircumflex,scommaaccent,sdotaccent,sdotbelow,sdotbelowdotaccent,seagullbelowcmb,second,secondtonechinese,section,seenfinalarabic,seeninitialarabic,seenmedialarabic,segoltahebrew,seharmenian,sehiragana,sekatakana,sekatakanahalfwidth,semicolon,semicolonmonospace,semicolonsmall,semivoicedmarkkana,semivoicedmarkkanahalfwidth,sentisquare,sentosquare,seven,sevenbengali,sevencircle,sevencircleinversesansserif,sevendeva,seveneighths,sevengujarati,sevengurmukhi,sevenhangzhou,sevenideographicparen,seveninferior,sevenmonospace,sevenoldstyle,sevenparen,sevenperiod,sevenpersian,sevenroman,sevensuperior,seventeencircle,seventeenparen,seventeenperiod,seventhai,softhyphen,shaarmenian,shabengali,shaddadammaarabic,shaddadammatanarabic,shaddafathaarabic,shaddakasraarabic,shaddakasratanarabic,shade,shadeva,shagujarati,shagurmukhi,shalshelethebrew,shbopomofo,sheenfinalarabic,sheeninitialarabic,sheenmedialarabic,sheicoptic,shhacyrillic,shimacoptic,shindageshhebrew,shindageshshindothebrew,shindageshsindothebrew,shook,sigma,sigma1,sigmalunatesymbolgreek,sihiragana,sikatakana,sikatakanahalfwidth,similar,siosacirclekorean,siosaparenkorean,sioscieuckorean,sioscirclekorean,sioskiyeokkorean,sioskorean,siosnieunkorean,siosparenkorean,siospieupkorean,siostikeutkorean,six,sixbengali,sixcircle,sixcircleinversesansserif,sixdeva,sixgujarati,sixgurmukhi,sixhangzhou,sixideographicparen,sixinferior,sixmonospace,sixoldstyle,sixparen,sixperiod,sixpersian,sixroman,sixsuperior,sixteencircle,sixteencurrencydenominatorbengali,sixteenparen,sixteenperiod,sixthai,slash,slashmonospace,slongdotaccent,smileface,smonospace,sohiragana,sokatakana,sokatakanahalfwidth,soliduslongoverlaycmb,solidusshortoverlaycmb,sorusithai,sosalathai,sosothai,sosuathai,space,spade,spadesuitwhite,sparen,squarebelowcmb,squarecc,squarecm,squarediagonalcrosshatchfill,squarehorizontalfill,squarekg,squarekm,squarekmcapital,squareln,squarelog,squaremg,squaremil,squaremm,squaremsquared,squareorthogonalcrosshatchfill,squareupperlefttolowerrightfill,squareupperrighttolowerleftfill,squareverticalfill,squarewhitewithsmallblack,srsquare,ssabengali,ssadeva,ssagujarati,ssangcieuckorean,ssanghieuhkorean,ssangieungkorean,ssangkiyeokkorean,ssangnieunkorean,ssangpieupkorean,ssangsioskorean,ssangtikeutkorean,ssuperior,sterling,sterlingmonospace,strokelongoverlaycmb,strokeshortoverlaycmb,subsetnotequal,succeeds,suchthat,suhiragana,sukatakana,sukatakanahalfwidth,summation,supersetnotequal,svsquare,syouwaerasquare,t,tabengali,tackdown,tackleft,tadeva,tagujarati,tagurmukhi,tahfinalarabic,tahinitialarabic,tahiragana,tahmedialarabic,taisyouerasquare,takatakana,takatakanahalfwidth,tau,tavdageshhebrew,tbar,tbopomofo,tcaron,tccurl,tcommaaccent,tchehfinalarabic,tchehmeeminitialarabic,tchehmedialarabic,tcircle,tcircumflexbelow,tdieresis,tdotaccent,tdotbelow,tedescendercyrillic,tehfinalarabic,tehhahinitialarabic,tehhahisolatedarabic,tehinitialarabic,tehiragana,tehjeeminitialarabic,tehjeemisolatedarabic,tehmarbutafinalarabic,tehmedialarabic,tehmeeminitialarabic,tehmeemisolatedarabic,tehnoonfinalarabic,tekatakana,tekatakanahalfwidth,telephone,telephoneblack,telishagedolahebrew,telishaqetanahebrew,tencircle,tenideographicparen,tenparen,tenperiod,tenroman,tesh,tetdageshhebrew,tetsecyrillic,tevirlefthebrew,thabengali,thadeva,thagujarati,thagurmukhi,thalfinalarabic,thanthakhatlowleftthai,thanthakhatlowrightthai,thanthakhatthai,thanthakhatupperleftthai,thehfinalarabic,thehinitialarabic,thehmedialarabic,therefore,theta,theta1,thieuthacirclekorean,thieuthaparenkorean,thieuthcirclekorean,thieuthkorean,thieuthparenkorean,thirteencircle,thirteenparen,thirteenperiod,thonangmonthothai,thook,thophuthaothai,thorn,thothahanthai,thothanthai,thothongthai,thothungthai,thousandcyrillic,thousandsseparatorpersian,three,threebengali,threecircle,threecircleinversesansserif,threedeva,threeeighths,threegujarati,threegurmukhi,threehangzhou,threeideographicparen,threeinferior,threemonospace,threenumeratorbengali,threeoldstyle,threeparen,threeperiod,threepersian,threequarters,threequartersemdash,threeroman,threesuperior,threethai,thzsquare,tihiragana,tikatakana,tikatakanahalfwidth,tikeutacirclekorean,tikeutaparenkorean,tikeutcirclekorean,tikeutkorean,tikeutparenkorean,tildebelowcmb,tildecomb,tildedoublecmb,tildeoverlaycmb,tildeverticalcmb,tipehalefthebrew,tippigurmukhi,titlocyrilliccmb,tiwnarmenian,tlinebelow,tmonospace,toarmenian,tohiragana,tokatakana,tokatakanahalfwidth,tonebarextrahighmod,tonebarextralowmod,tonebarhighmod,tonebarlowmod,tonebarmidmod,tonefive,tonesix,tonetwo,tonos,tonsquare,topatakthai,tortoiseshellbracketleft,tortoiseshellbracketleftsmall,tortoiseshellbracketleftvertical,tortoiseshellbracketright,tortoiseshellbracketrightsmall,tortoiseshellbracketrightvertical,totaothai,tpalatalhook,tparen,trademark,trademarksans,trademarkserif,tretroflexhook,ts,tsadidageshhebrew,tsuperior,ttabengali,ttadeva,ttagujarati,ttagurmukhi,ttehfinalarabic,ttehinitialarabic,ttehmedialarabic,tthabengali,tthadeva,tthagujarati,tthagurmukhi,tturned,tuhiragana,tukatakana,tukatakanahalfwidth,tusmallhiragana,tusmallkatakana,tusmallkatakanahalfwidth,twelvecircle,twelveparen,twelveperiod,twelveroman,twentycircle,twentyhangzhou,twentyparen,twentyperiod,two,twobengali,twocircle,twocircleinversesansserif,twodeva,twodotenleader,twodotleadervertical,twogujarati,twogurmukhi,twohangzhou,twoideographicparen,twoinferior,twomonospace,twonumeratorbengali,twooldstyle,twoparen,twoperiod,twopersian,tworoman,twostroke,twosuperior,twothai,twothirds,u,uacute,ubar,ubengali,ubopomofo,ubreve,ucaron,ucircle,ucircumflex,ucircumflexbelow,udattadeva,uhungarumlaut,udblgrave,udeva,udieresis,udieresisacute,udieresisbelow,udieresiscaron,udieresiscyrillic,udieresisgrave,udieresismacron,udotbelow,ugrave,ugujarati,ugurmukhi,uhiragana,uhookabove,uhorn,uhornacute,uhorndotbelow,uhorngrave,uhornhookabove,uhorntilde,uhungarumlautcyrillic,uinvertedbreve,ukatakana,ukatakanahalfwidth,ukcyrillic,ukorean,umacron,umacroncyrillic,umacrondieresis,umatragurmukhi,umonospace,underscore,underscoremonospace,underscorevertical,underscorewavy,union,uogonek,uparen,upblock,upperdothebrew,upsilon,upsilondieresis,upsilondieresistonos,upsilonlatin,upsilontonos,uptackbelowcmb,uptackmod,uragurmukhi,uring,usmallhiragana,usmallkatakana,usmallkatakanahalfwidth,ustraightcyrillic,ustraightstrokecyrillic,utilde,utildeacute,utildebelow,uubengali,uudeva,uugujarati,uugurmukhi,uumatragurmukhi,uuvowelsignbengali,uuvowelsigndeva,uuvowelsigngujarati,uvowelsignbengali,uvowelsigndeva,uvowelsigngujarati,v,vadeva,vagujarati,vagurmukhi,vakatakana,vcircle,vdotbelow,vehfinalarabic,vehinitialarabic,vehmedialarabic,vekatakana,verticallineabovecmb,verticallinebelowcmb,verticallinelowmod,verticallinemod,vewarmenian,vhook,vikatakana,viramabengali,viramadeva,viramagujarati,visargabengali,visargadeva,visargagujarati,vmonospace,voarmenian,voicediterationhiragana,voicediterationkatakana,voicedmarkkana,voicedmarkkanahalfwidth,vokatakana,vparen,vtilde,vturned,vuhiragana,vukatakana,w,wacute,waekorean,wahiragana,wakatakana,wakatakanahalfwidth,wakorean,wasmallhiragana,wasmallkatakana,wattosquare,wavedash,wavyunderscorevertical,wawfinalarabic,wawhamzaabovefinalarabic,wbsquare,wcircle,wcircumflex,wdieresis,wdotaccent,wdotbelow,wehiragana,weierstrass,wekatakana,wekorean,weokorean,wgrave,whitecornerbracketleft,whitecornerbracketleftvertical,whitecornerbracketright,whitecornerbracketrightvertical,whitediamond,whitediamondcontainingblacksmalldiamond,whitedownpointingsmalltriangle,whitedownpointingtriangle,whiteleftpointingsmalltriangle,whiteleftpointingtriangle,whitelenticularbracketleft,whitelenticularbracketright,whiterightpointingsmalltriangle,whiterightpointingtriangle,whitestar,whitetelephone,whitetortoiseshellbracketleft,whitetortoiseshellbracketright,whiteuppointingsmalltriangle,whiteuppointingtriangle,wihiragana,wikatakana,wikorean,wmonospace,wohiragana,wokatakana,wokatakanahalfwidth,won,wonmonospace,wowaenthai,wparen,wring,wsuperior,wturned,wynn,x,xabovecmb,xbopomofo,xcircle,xdieresis,xdotaccent,xeharmenian,xi,xmonospace,xparen,xsuperior,y,yaadosquare,yabengali,yacute,yadeva,yaekorean,yagujarati,yagurmukhi,yahiragana,yakatakana,yakatakanahalfwidth,yakorean,yamakkanthai,yasmallhiragana,yasmallkatakana,yasmallkatakanahalfwidth,ycircle,ycircumflex,ydieresis,ydotaccent,ydotbelow,yehbarreefinalarabic,yehfinalarabic,yehhamzaabovefinalarabic,yehhamzaaboveinitialarabic,yehhamzaabovemedialarabic,yehmeeminitialarabic,yehmeemisolatedarabic,yehnoonfinalarabic,yehthreedotsbelowarabic,yekorean,yen,yenmonospace,yeokorean,yeorinhieuhkorean,yerahbenyomolefthebrew,yerudieresiscyrillic,yesieungkorean,yesieungpansioskorean,yesieungsioskorean,yetivhebrew,ygrave,yhook,yhookabove,yiarmenian,yikorean,yinyang,yiwnarmenian,ymonospace,yoddageshhebrew,yohiragana,yoikorean,yokatakana,yokatakanahalfwidth,yokorean,yosmallhiragana,yosmallkatakana,yosmallkatakanahalfwidth,yotgreek,yoyaekorean,yoyakorean,yoyakthai,yoyingthai,yparen,ypogegrammeni,ypogegrammenigreekcmb,yr,yring,ysuperior,ytilde,yturned,yuhiragana,yuikorean,yukatakana,yukatakanahalfwidth,yukorean,yusbigcyrillic,yusbigiotifiedcyrillic,yuslittlecyrillic,yuslittleiotifiedcyrillic,yusmallhiragana,yusmallkatakana,yusmallkatakanahalfwidth,yuyekorean,yuyeokorean,yyabengali,yyadeva,z,zaarmenian,zacute,zadeva,zagurmukhi,zahfinalarabic,zahinitialarabic,zahiragana,zahmedialarabic,zainfinalarabic,zakatakana,zaqefgadolhebrew,zaqefqatanhebrew,zarqahebrew,zayindageshhebrew,zbopomofo,zcaron,zcircle,zcircumflex,zcurl,zdotaccent,zdotbelow,zedescendercyrillic,zedieresiscyrillic,zehiragana,zekatakana,zero,zerobengali,zerodeva,zerogujarati,zerogurmukhi,zeroinferior,zeromonospace,zerooldstyle,zeropersian,zerosuperior,zerothai,zerowidthjoiner,zerowidthspace,zeta,zhbopomofo,zhearmenian,zhebrevecyrillic,zhedescendercyrillic,zhedieresiscyrillic,zihiragana,zikatakana,zinorhebrew,zlinebelow,zmonospace,zohiragana,zokatakana,zparen,zretroflexhook,zstroke,zuhiragana,zukatakana";

	private string characterNames = "space,exclam,quotedbl,numbersign,dollar,percent,ampersand,quoteright,parenleft,parenright,asterisk,plus,comma,hyphen,period,slash,zero,one,two,three,four,five,six,seven,eight,nine,colon,semicolon,less,equal,greater,question,at,A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,bracketleft,backslash,bracketright,asciicircum,underscore,quoteleft,a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z,braceleft,bar,braceright,asciitilde,exclamdown,cent,sterling,fraction,yen,florin,section,currency,quotesingle,quotedblleft,guillemotleft,guilsinglleft,guilsinglright,fi,fl,endash,dagger,daggerdbl,periodcentered,paragraph,bullet,quotesinglbase,quotedblbase,quotedblright,guillemotright,ellipsis,perthousand,questiondown,grave,acute,circumflex,tilde,macron,breve,dotaccent,dieresis,ring,cedilla,hungarumlaut,ogonek,caron,emdash,AE,ordfeminine,Lslash,Oslash,OE,ordmasculine,ae,dotlessi,lslash,oslash,oe,germandbls,Idieresis,eacute,abreve,uhungarumlaut,ecaron,Ydieresis,divide,Yacute,Acircumflex,aacute,Ucircumflex,yacute,scommaaccent,ecircumflex,Uring,Udieresis,aogonek,Uacute,uogonek,Edieresis,Dcroat,commaaccent,copyright,Emacron,ccaron,aring,Ncommaaccent,lacute,agrave,Tcommaaccent,Cacute,atilde,Edotaccent,scaron,scedilla,iacute,lozenge,Rcaron,Gcommaaccent,ucircumflex,acircumflex,Amacron,rcaron,ccedilla,Zdotaccent,Thorn,Omacron,Racute,Sacute,dcaron,Umacron,uring,threebaseior,Ograve,Agrave,Abreve,multiply,uacute,Tcaron,partialdiff,ydieresis,Nacute,icircumflex,Ecircumflex,adieresis,edieresis,cacute,nacute,umacron,Ncaron,Iacute,plusminus,brokenbar,registered,Gbreve,Idotaccent,summation,Egrave,racute,omacron,Zacute,Zcaron,greaterequal,Eth,Ccedilla,lcommaaccent,tcaron,eogonek,Uogonek,Aacute,Adieresis,egrave,zacute,iogonek,Oacute,oacute,amacron,sacute,idieresis,Ocircumflex,Ugrave,Delta,thorn,twobaseior,Odieresis,mu,igrave,ohungarumlaut,Eogonek,dcroat,threequarters,Scedilla,lcaron,Kcommaaccent,Lacute,trademark,edotaccent,Igrave,Imacron,Lcaron,onehalf,lessequal,ocircumflex,ntilde,Uhungarumlaut,Eacute,emacron,gbreve,onequarter,Scaron,Scommaaccent,Ohungarumlaut,degree,ograve,Ccaron,ugrave,radical,Dcaron,rcommaaccent,Ntilde,otilde,Rcommaaccent,Lcommaaccent,Atilde,Aogonek,Aring,Otilde,zdotaccent,Ecaron,Iogonek,kcommaaccent,minus,Icircumflex,ncaron,tcommaaccent,logicalnot,odieresis,udieresis,notequal,gcommaaccent,eth,zcaron,ncommaaccent,onebaseior,imacron,Euro";

	private int[] hevetica = new int[315]
	{
		278, 278, 355, 556, 556, 889, 667, 222, 333, 333,
		389, 584, 278, 333, 278, 278, 556, 556, 556, 556,
		556, 556, 556, 556, 556, 556, 278, 278, 584, 584,
		584, 556, 1015, 667, 667, 722, 722, 667, 611, 778,
		722, 278, 500, 667, 556, 833, 722, 778, 667, 778,
		722, 667, 611, 722, 667, 944, 667, 667, 611, 278,
		278, 278, 469, 556, 222, 556, 556, 500, 556, 556,
		278, 556, 556, 222, 222, 500, 222, 833, 556, 556,
		556, 556, 333, 500, 278, 556, 500, 722, 500, 500,
		500, 334, 260, 334, 584, 333, 556, 556, 167, 556,
		556, 556, 556, 191, 333, 556, 333, 333, 500, 500,
		556, 556, 556, 278, 537, 350, 222, 333, 333, 556,
		1000, 1000, 611, 333, 333, 333, 333, 333, 333, 333,
		333, 333, 333, 333, 333, 333, 1000, 1000, 370, 556,
		778, 1000, 365, 889, 278, 222, 611, 944, 611, 278,
		556, 556, 556, 556, 667, 584, 667, 667, 556, 722,
		500, 500, 556, 722, 722, 556, 722, 556, 667, 722,
		250, 737, 667, 500, 556, 722, 222, 556, 611, 722,
		556, 667, 500, 500, 278, 471, 722, 778, 556, 556,
		667, 333, 500, 611, 667, 778, 722, 667, 643, 722,
		556, 333, 778, 667, 667, 584, 556, 611, 476, 500,
		722, 278, 667, 556, 556, 500, 556, 556, 722, 278,
		584, 260, 737, 778, 278, 600, 667, 333, 556, 611,
		611, 549, 722, 722, 222, 317, 556, 722, 667, 667,
		556, 500, 222, 778, 556, 556, 500, 278, 778, 722,
		612, 556, 333, 778, 556, 278, 556, 667, 556, 834,
		667, 299, 667, 556, 1000, 556, 278, 278, 556, 834,
		549, 556, 556, 722, 667, 556, 556, 834, 667, 667,
		778, 400, 556, 722, 556, 453, 722, 333, 722, 556,
		722, 556, 667, 667, 667, 778, 500, 667, 278, 500,
		584, 278, 556, 278, 584, 556, 556, 549, 556, 556,
		500, 556, 333, 278, 556
	};

	private int[] heveticaBold = new int[315]
	{
		278, 333, 474, 556, 556, 889, 722, 278, 333, 333,
		389, 584, 278, 333, 278, 278, 556, 556, 556, 556,
		556, 556, 556, 556, 556, 556, 333, 333, 584, 584,
		584, 611, 975, 722, 722, 722, 722, 667, 611, 778,
		722, 278, 556, 722, 611, 833, 722, 778, 667, 778,
		722, 667, 611, 722, 667, 944, 667, 667, 611, 333,
		278, 333, 584, 556, 278, 556, 611, 556, 611, 556,
		333, 611, 611, 278, 278, 556, 278, 889, 611, 611,
		611, 611, 389, 556, 333, 611, 556, 778, 556, 556,
		500, 389, 280, 389, 584, 333, 556, 556, 167, 556,
		556, 556, 556, 238, 500, 556, 333, 333, 611, 611,
		556, 556, 556, 278, 556, 350, 278, 500, 500, 556,
		1000, 1000, 611, 333, 333, 333, 333, 333, 333, 333,
		333, 333, 333, 333, 333, 333, 1000, 1000, 370, 611,
		778, 1000, 365, 889, 278, 278, 611, 944, 611, 278,
		556, 556, 611, 556, 667, 584, 667, 722, 556, 722,
		556, 556, 556, 722, 722, 556, 722, 611, 667, 722,
		250, 737, 667, 556, 556, 722, 278, 556, 611, 722,
		556, 667, 556, 556, 278, 494, 722, 778, 611, 556,
		722, 389, 556, 611, 667, 778, 722, 667, 743, 722,
		611, 333, 778, 722, 722, 584, 611, 611, 494, 556,
		722, 278, 667, 556, 556, 556, 611, 611, 722, 278,
		584, 280, 737, 778, 278, 600, 667, 389, 611, 611,
		611, 549, 722, 722, 278, 389, 556, 722, 722, 722,
		556, 500, 278, 778, 611, 556, 556, 278, 778, 722,
		612, 611, 333, 778, 611, 278, 611, 667, 611, 834,
		667, 400, 722, 611, 1000, 556, 278, 278, 611, 834,
		549, 611, 611, 722, 667, 556, 611, 834, 667, 667,
		778, 400, 611, 722, 611, 549, 722, 389, 722, 611,
		722, 611, 722, 722, 722, 778, 500, 667, 278, 556,
		584, 278, 611, 333, 584, 611, 611, 549, 611, 611,
		500, 611, 333, 278, 556
	};

	private int[] courier = new int[315]
	{
		600, 600, 600, 600, 600, 600, 600, 600, 600, 600,
		600, 600, 600, 600, 600, 600, 600, 600, 600, 600,
		600, 600, 600, 600, 600, 600, 600, 600, 600, 600,
		600, 600, 600, 600, 600, 600, 600, 600, 600, 600,
		600, 600, 600, 600, 600, 600, 600, 600, 600, 600,
		600, 600, 600, 600, 600, 600, 600, 600, 600, 600,
		600, 600, 600, 600, 600, 600, 600, 600, 600, 600,
		600, 600, 600, 600, 600, 600, 600, 600, 600, 600,
		600, 600, 600, 600, 600, 600, 600, 600, 600, 600,
		600, 600, 600, 600, 600, 600, 600, 600, 600, 600,
		600, 600, 600, 600, 600, 600, 600, 600, 600, 600,
		600, 600, 600, 600, 600, 600, 600, 600, 600, 600,
		600, 600, 600, 600, 600, 600, 600, 600, 600, 600,
		600, 600, 600, 600, 600, 600, 600, 600, 600, 600,
		600, 600, 600, 600, 600, 600, 600, 600, 600, 600,
		600, 600, 600, 600, 600, 600, 600, 600, 600, 600,
		600, 600, 600, 600, 600, 600, 600, 600, 600, 600,
		600, 600, 600, 600, 600, 600, 600, 600, 600, 600,
		600, 600, 600, 600, 600, 600, 600, 600, 600, 600,
		600, 600, 600, 600, 600, 600, 600, 600, 600, 600,
		600, 600, 600, 600, 600, 600, 600, 600, 600, 600,
		600, 600, 600, 600, 600, 600, 600, 600, 600, 600,
		600, 600, 600, 600, 600, 600, 600, 600, 600, 600,
		600, 600, 600, 600, 600, 600, 600, 600, 600, 600,
		600, 600, 600, 600, 600, 600, 600, 600, 600, 600,
		600, 600, 600, 600, 600, 600, 600, 600, 600, 600,
		600, 600, 600, 600, 600, 600, 600, 600, 600, 600,
		600, 600, 600, 600, 600, 600, 600, 600, 600, 600,
		600, 600, 600, 600, 600, 600, 600, 600, 600, 600,
		600, 600, 600, 600, 600, 600, 600, 600, 600, 600,
		600, 600, 600, 600, 600, 600, 600, 600, 600, 600,
		600, 600, 600, 600, 600
	};

	private int[] symbol = new int[190]
	{
		250, 333, 713, 500, 549, 833, 778, 439, 333, 333,
		500, 549, 250, 549, 250, 278, 500, 500, 500, 500,
		500, 500, 500, 500, 500, 500, 278, 278, 549, 549,
		549, 444, 549, 722, 667, 722, 612, 611, 763, 603,
		722, 333, 631, 722, 686, 889, 722, 722, 768, 741,
		556, 592, 611, 690, 439, 768, 645, 795, 611, 333,
		863, 333, 658, 500, 500, 631, 549, 549, 494, 439,
		521, 411, 603, 329, 603, 549, 549, 576, 521, 549,
		549, 521, 549, 603, 439, 576, 713, 686, 493, 686,
		494, 480, 200, 480, 549, 750, 620, 247, 549, 167,
		713, 500, 753, 753, 753, 753, 1042, 987, 603, 987,
		603, 400, 549, 411, 549, 549, 713, 494, 460, 549,
		549, 549, 549, 1000, 603, 1000, 658, 823, 686, 795,
		987, 768, 768, 823, 768, 768, 713, 713, 713, 713,
		713, 713, 713, 768, 713, 790, 790, 890, 823, 549,
		250, 713, 603, 603, 1042, 987, 603, 987, 603, 494,
		329, 790, 790, 786, 713, 384, 384, 384, 384, 384,
		384, 494, 494, 494, 494, 329, 274, 686, 686, 686,
		384, 384, 384, 384, 384, 384, 494, 494, 494, 790
	};

	private int[] zapfDingbat = new int[202]
	{
		278, 974, 961, 974, 980, 719, 789, 790, 791, 690,
		960, 939, 549, 855, 911, 933, 911, 945, 974, 755,
		846, 762, 761, 571, 677, 763, 760, 759, 754, 494,
		552, 537, 577, 692, 786, 788, 788, 790, 793, 794,
		816, 823, 789, 841, 823, 833, 816, 831, 923, 744,
		723, 749, 790, 792, 695, 776, 768, 792, 759, 707,
		708, 682, 701, 826, 815, 789, 789, 707, 687, 696,
		689, 786, 787, 713, 791, 785, 791, 873, 761, 762,
		762, 759, 759, 892, 892, 788, 784, 438, 138, 277,
		415, 392, 392, 668, 668, 390, 390, 317, 317, 276,
		276, 509, 509, 410, 410, 234, 234, 334, 334, 732,
		544, 544, 910, 667, 760, 760, 776, 595, 694, 626,
		788, 788, 788, 788, 788, 788, 788, 788, 788, 788,
		788, 788, 788, 788, 788, 788, 788, 788, 788, 788,
		788, 788, 788, 788, 788, 788, 788, 788, 788, 788,
		788, 788, 788, 788, 788, 788, 788, 788, 788, 788,
		894, 838, 1016, 458, 748, 924, 748, 918, 927, 928,
		928, 834, 873, 828, 924, 924, 917, 930, 931, 463,
		883, 836, 836, 867, 867, 696, 696, 874, 874, 760,
		946, 771, 865, 771, 888, 967, 888, 831, 873, 927,
		970, 918
	};

	private int[] timeRoman = new int[315]
	{
		250, 333, 408, 500, 500, 833, 778, 333, 333, 333,
		500, 564, 250, 333, 250, 278, 500, 500, 500, 500,
		500, 500, 500, 500, 500, 500, 278, 278, 564, 564,
		564, 444, 921, 722, 667, 667, 722, 611, 556, 722,
		722, 333, 389, 722, 611, 889, 722, 722, 556, 722,
		667, 556, 611, 722, 722, 944, 722, 722, 611, 333,
		278, 333, 469, 500, 333, 444, 500, 444, 500, 444,
		333, 500, 500, 278, 278, 500, 278, 778, 500, 500,
		500, 500, 333, 389, 278, 500, 500, 722, 500, 500,
		444, 480, 200, 480, 541, 333, 500, 500, 167, 500,
		500, 500, 500, 180, 444, 500, 333, 333, 556, 556,
		500, 500, 500, 250, 453, 350, 333, 444, 444, 500,
		1000, 1000, 444, 333, 333, 333, 333, 333, 333, 333,
		333, 333, 333, 333, 333, 333, 1000, 889, 276, 611,
		722, 889, 310, 667, 278, 278, 500, 722, 500, 333,
		444, 444, 500, 444, 722, 564, 722, 722, 444, 722,
		500, 389, 444, 722, 722, 444, 722, 500, 611, 722,
		250, 760, 611, 444, 444, 722, 278, 444, 611, 667,
		444, 611, 389, 389, 278, 471, 667, 722, 500, 444,
		722, 333, 444, 611, 556, 722, 667, 556, 588, 722,
		500, 300, 722, 722, 722, 564, 500, 611, 476, 500,
		722, 278, 611, 444, 444, 444, 500, 500, 722, 333,
		564, 200, 760, 722, 333, 600, 611, 333, 500, 611,
		611, 549, 722, 667, 278, 326, 444, 722, 722, 722,
		444, 444, 278, 722, 500, 444, 389, 278, 722, 722,
		612, 500, 300, 722, 500, 278, 500, 611, 500, 750,
		556, 344, 722, 611, 980, 444, 333, 333, 611, 750,
		549, 500, 500, 722, 611, 444, 500, 750, 556, 556,
		722, 400, 500, 667, 500, 453, 722, 333, 722, 500,
		667, 611, 722, 722, 722, 722, 444, 611, 333, 500,
		564, 333, 500, 278, 564, 500, 500, 549, 500, 500,
		444, 500, 300, 278, 500
	};

	private int[] timesBold = new int[315]
	{
		250, 333, 555, 500, 500, 1000, 833, 333, 333, 333,
		500, 570, 250, 333, 250, 278, 500, 500, 500, 500,
		500, 500, 500, 500, 500, 500, 333, 333, 570, 570,
		570, 500, 930, 722, 667, 722, 722, 667, 611, 778,
		778, 389, 500, 778, 667, 944, 722, 778, 611, 778,
		722, 556, 667, 722, 722, 1000, 722, 722, 667, 333,
		278, 333, 581, 500, 333, 500, 556, 444, 556, 444,
		333, 500, 556, 278, 333, 556, 278, 833, 556, 500,
		556, 556, 444, 389, 333, 556, 500, 722, 500, 500,
		444, 394, 220, 394, 520, 333, 500, 500, 167, 500,
		500, 500, 500, 278, 500, 500, 333, 333, 556, 556,
		500, 500, 500, 250, 540, 350, 333, 500, 500, 500,
		1000, 1000, 500, 333, 333, 333, 333, 333, 333, 333,
		333, 333, 333, 333, 333, 333, 1000, 1000, 300, 667,
		778, 1000, 330, 722, 278, 278, 500, 722, 556, 389,
		444, 500, 556, 444, 722, 570, 722, 722, 500, 722,
		500, 389, 444, 722, 722, 500, 722, 556, 667, 722,
		250, 747, 667, 444, 500, 722, 278, 500, 667, 722,
		500, 667, 389, 389, 278, 494, 722, 778, 556, 500,
		722, 444, 444, 667, 611, 778, 722, 556, 672, 722,
		556, 300, 778, 722, 722, 570, 556, 667, 494, 500,
		722, 278, 667, 500, 444, 444, 556, 556, 722, 389,
		570, 220, 747, 778, 389, 600, 667, 444, 500, 667,
		667, 549, 722, 722, 278, 416, 444, 722, 722, 722,
		444, 444, 278, 778, 500, 500, 389, 278, 778, 722,
		612, 556, 300, 778, 556, 278, 500, 667, 556, 750,
		556, 394, 778, 667, 1000, 444, 389, 389, 667, 750,
		549, 500, 556, 722, 667, 444, 500, 750, 556, 556,
		778, 400, 500, 722, 556, 549, 722, 444, 722, 500,
		722, 667, 722, 722, 722, 778, 444, 667, 389, 556,
		570, 389, 556, 333, 570, 500, 556, 549, 500, 500,
		444, 556, 300, 278, 500
	};

	private int[] timesItaic = new int[315]
	{
		250, 333, 420, 500, 500, 833, 778, 333, 333, 333,
		500, 675, 250, 333, 250, 278, 500, 500, 500, 500,
		500, 500, 500, 500, 500, 500, 333, 333, 675, 675,
		675, 500, 920, 611, 611, 667, 722, 611, 611, 722,
		722, 333, 444, 667, 556, 833, 667, 722, 611, 722,
		611, 500, 556, 722, 611, 833, 611, 556, 556, 389,
		278, 389, 422, 500, 333, 500, 500, 444, 500, 444,
		278, 500, 500, 278, 278, 444, 278, 722, 500, 500,
		500, 500, 389, 389, 278, 500, 444, 667, 444, 444,
		389, 400, 275, 400, 541, 389, 500, 500, 167, 500,
		500, 500, 500, 214, 556, 500, 333, 333, 500, 500,
		500, 500, 500, 250, 523, 350, 333, 556, 556, 500,
		889, 1000, 500, 333, 333, 333, 333, 333, 333, 333,
		333, 333, 333, 333, 333, 333, 889, 889, 276, 556,
		722, 944, 310, 667, 278, 278, 500, 667, 500, 333,
		444, 500, 500, 444, 556, 675, 556, 611, 500, 722,
		444, 389, 444, 722, 722, 500, 722, 500, 611, 722,
		250, 760, 611, 444, 500, 667, 278, 500, 556, 667,
		500, 611, 389, 389, 278, 471, 611, 722, 500, 500,
		611, 389, 444, 556, 611, 722, 611, 500, 544, 722,
		500, 300, 722, 611, 611, 675, 500, 556, 476, 444,
		667, 278, 611, 500, 444, 444, 500, 500, 667, 333,
		675, 275, 760, 722, 333, 600, 611, 389, 500, 556,
		556, 549, 722, 667, 278, 300, 444, 722, 611, 611,
		444, 389, 278, 722, 500, 500, 389, 278, 722, 722,
		612, 500, 300, 722, 500, 278, 500, 611, 500, 750,
		500, 300, 667, 556, 980, 444, 333, 333, 611, 750,
		549, 500, 500, 722, 611, 444, 500, 750, 500, 500,
		722, 400, 500, 667, 500, 453, 722, 389, 667, 500,
		611, 556, 611, 611, 611, 722, 389, 611, 333, 444,
		675, 333, 500, 278, 675, 500, 500, 549, 500, 500,
		389, 500, 300, 278, 500
	};

	private int[] timesBoldItalic = new int[315]
	{
		250, 389, 555, 500, 500, 833, 778, 333, 333, 333,
		500, 570, 250, 333, 250, 278, 500, 500, 500, 500,
		500, 500, 500, 500, 500, 500, 333, 333, 570, 570,
		570, 500, 832, 667, 667, 667, 722, 667, 667, 722,
		778, 389, 500, 667, 611, 889, 722, 722, 611, 722,
		667, 556, 611, 722, 667, 889, 667, 611, 611, 333,
		278, 333, 570, 500, 333, 500, 500, 444, 500, 444,
		333, 500, 556, 278, 278, 500, 278, 778, 556, 500,
		500, 500, 389, 389, 278, 556, 444, 667, 500, 444,
		389, 348, 220, 348, 570, 389, 500, 500, 167, 500,
		500, 500, 500, 278, 500, 500, 333, 333, 556, 556,
		500, 500, 500, 250, 500, 350, 333, 500, 500, 500,
		1000, 1000, 500, 333, 333, 333, 333, 333, 333, 333,
		333, 333, 333, 333, 333, 333, 1000, 944, 266, 611,
		722, 944, 300, 722, 278, 278, 500, 722, 500, 389,
		444, 500, 556, 444, 611, 570, 611, 667, 500, 722,
		444, 389, 444, 722, 722, 500, 722, 556, 667, 722,
		250, 747, 667, 444, 500, 722, 278, 500, 611, 667,
		500, 667, 389, 389, 278, 494, 667, 722, 556, 500,
		667, 389, 444, 611, 611, 722, 667, 556, 608, 722,
		556, 300, 722, 667, 667, 570, 556, 611, 494, 444,
		722, 278, 667, 500, 444, 444, 556, 556, 722, 389,
		570, 220, 747, 722, 389, 600, 667, 389, 500, 611,
		611, 549, 722, 667, 278, 366, 444, 722, 667, 667,
		444, 389, 278, 722, 500, 500, 389, 278, 722, 722,
		612, 500, 300, 722, 576, 278, 500, 667, 500, 750,
		556, 382, 667, 611, 1000, 444, 389, 389, 611, 750,
		549, 500, 556, 722, 667, 444, 500, 750, 556, 556,
		722, 400, 500, 667, 556, 549, 722, 389, 722, 500,
		667, 611, 667, 667, 667, 722, 389, 667, 389, 500,
		606, 389, 556, 278, 606, 500, 556, 549, 500, 500,
		389, 556, 300, 278, 500
	};

	public PdfFontFamily FontFamily => m_fontFamily;

	private Dictionary<int, string> CharacterMapTable
	{
		get
		{
			if (unicodeToNames == null)
			{
				unicodeToNames = new Dictionary<int, string>();
				string[] array = unicodeNames.Split(',');
				for (int i = 0; i < charcode.Length; i++)
				{
					unicodeToNames[charcode[i]] = array[i];
				}
			}
			return unicodeToNames;
		}
	}

	private Dictionary<string, int> FindWidth
	{
		get
		{
			if (widthTableUpdate == null)
			{
				widthTableUpdate = new Dictionary<string, int>();
				string[] array = characterNames.Split(',');
				int[] array2 = FindStandardWidth();
				for (int i = 0; i < array2.Length; i++)
				{
					widthTableUpdate[array[i]] = array2[i];
				}
			}
			return widthTableUpdate;
		}
	}

	public PdfStandardFont(PdfFontFamily fontFamily, float size)
		: this(fontFamily, size, PdfFontStyle.Regular)
	{
	}

	public PdfStandardFont(PdfFontFamily fontFamily, float size, PdfFontStyle style)
		: base(size, style)
	{
		m_fontFamily = fontFamily;
		CheckStyle();
		InitializeInternals();
	}

	internal PdfStandardFont(PdfFontFamily fontFamily, float size, PdfFontStyle style, bool isLicense)
		: base(size, style)
	{
		m_fontFamily = fontFamily;
		CheckStyle();
		InitializeInternals(isLicense);
	}

	public PdfStandardFont(PdfStandardFont prototype, float size)
		: this(prototype.FontFamily, size, prototype.Style)
	{
	}

	public PdfStandardFont(PdfStandardFont prototype, float size, PdfFontStyle style)
		: this(prototype.FontFamily, size, style)
	{
	}

	~PdfStandardFont()
	{
		if (charWidthDictionary != null)
		{
			charWidthDictionary = null;
		}
	}

	public void SetTextEncoding(Encoding textEncoding)
	{
		fontEncoding = textEncoding;
		if (PdfDocument.Cache.Search(this) != null)
		{
			((IPdfCache)this).SetInternals((IPdfPrimitive)CreateInternals());
		}
		dictionary.BeginSave += FontBeginSave;
	}

	protected internal override float GetCharWidth(char charCode, PdfStringFormat format)
	{
		float charWidthInternal = GetCharWidthInternal(charCode, format);
		float size = base.Metrics.GetSize(format);
		return charWidthInternal * (0.001f * size);
	}

	protected internal override float GetLineWidth(string line, PdfStringFormat format)
	{
		if (line == null)
		{
			throw new ArgumentNullException("line");
		}
		float num = 0f;
		lock (PdfFont.s_syncObject)
		{
			line = ((fontEncoding == null) ? Convert(line) : Convert(line, fontEncoding));
			int i = 0;
			for (int length = line.Length; i < length; i++)
			{
				char c = line[i];
				if (charWidthDictionary.ContainsKey(c))
				{
					num += charWidthDictionary[c];
					continue;
				}
				float charWidthInternal = GetCharWidthInternal(c, format);
				charWidthDictionary.Add(c, charWidthInternal);
				num += charWidthInternal;
			}
			float size = base.Metrics.GetSize(format);
			num *= 0.001f * size;
			return ApplyFormatSettings(line, format, num);
		}
	}

	protected override bool EqualsToFont(PdfFont font)
	{
		bool result = false;
		if (font is PdfStandardFont pdfStandardFont)
		{
			bool num = FontFamily == pdfStandardFont.FontFamily;
			bool flag = (base.Style & ~(PdfFontStyle.Underline | PdfFontStyle.Strikeout)) == (pdfStandardFont.Style & ~(PdfFontStyle.Underline | PdfFontStyle.Strikeout));
			result = num && flag;
		}
		return result;
	}

	private void InitializeInternals()
	{
		InitializeInternals(isLicense: false);
	}

	private void InitializeInternals(bool isLicense)
	{
		if (!isLicense && PdfDocument.ConformanceLevel != 0 && PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_X1A2001)
		{
			throw new PdfConformanceException("All the fonts must be embedded in " + PdfDocument.ConformanceLevel.ToString() + " document.");
		}
		lock (PdfFont.s_syncObject)
		{
			IPdfCache pdfCache = null;
			if (PdfDocument.EnableCache)
			{
				pdfCache = PdfDocument.Cache.Search(this);
			}
			IPdfPrimitive pdfPrimitive = null;
			if (pdfCache == null)
			{
				PdfFontMetrics metrics = PdfStandardFontMetricsFactory.GetMetrics(m_fontFamily, base.Style, base.Size);
				base.Metrics = metrics;
				pdfPrimitive = CreateInternals();
			}
			else
			{
				pdfPrimitive = pdfCache.GetInternals();
				PdfFontMetrics metrics2 = ((PdfFont)pdfCache).Metrics;
				metrics2 = (PdfFontMetrics)metrics2.Clone();
				metrics2.Size = base.Size;
				base.Metrics = metrics2;
			}
			((IPdfCache)this).SetInternals(pdfPrimitive);
		}
	}

	private PdfDictionary CreateInternals()
	{
		dictionary["Type"] = new PdfName("Font");
		dictionary["Subtype"] = new PdfName("Type1");
		dictionary["BaseFont"] = new PdfName(base.Metrics.PostScriptName);
		if (FontFamily != PdfFontFamily.Symbol && FontFamily != PdfFontFamily.ZapfDingbats)
		{
			string value = FontEncoding.WinAnsiEncoding.ToString();
			dictionary["Encoding"] = new PdfName(value);
		}
		return dictionary;
	}

	private int[] FindStandardWidth()
	{
		int[] result = new int[0];
		if (FontFamily == PdfFontFamily.Helvetica)
		{
			result = ((((base.Style & PdfFontStyle.Bold) <= PdfFontStyle.Regular || (base.Style & PdfFontStyle.Italic) <= PdfFontStyle.Regular) && (base.Style & PdfFontStyle.Bold) <= PdfFontStyle.Regular) ? hevetica : heveticaBold);
		}
		else if (FontFamily == PdfFontFamily.Courier)
		{
			result = courier;
		}
		else if (FontFamily == PdfFontFamily.TimesRoman)
		{
			result = (((base.Style & PdfFontStyle.Bold) > PdfFontStyle.Regular && (base.Style & PdfFontStyle.Italic) > PdfFontStyle.Regular) ? timesBoldItalic : (((base.Style & PdfFontStyle.Bold) > PdfFontStyle.Regular) ? timesBold : (((base.Style & PdfFontStyle.Italic) <= PdfFontStyle.Regular) ? timeRoman : timesItaic)));
		}
		else if (FontFamily == PdfFontFamily.Symbol)
		{
			result = symbol;
		}
		else if (FontFamily == PdfFontFamily.ZapfDingbats)
		{
			result = zapfDingbat;
		}
		return result;
	}

	private void FontBeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		if (fontEncoding == null || FontFamily == PdfFontFamily.Symbol || FontFamily == PdfFontFamily.ZapfDingbats)
		{
			return;
		}
		if (widthTableUpdate == null)
		{
			UpdateWidthTable();
		}
		List<int> list = usedChars;
		list.Sort();
		short[] array = new short[256];
		for (int i = 0; i < list.Count; i++)
		{
			array[list[i] & 0xFF] = 1;
		}
		int num = 0;
		int num2 = 0;
		for (num = 0; num < 256 && array[num] == 0; num++)
		{
		}
		num2 = 255;
		while (num2 >= num && array[num2] == 0)
		{
			num2--;
		}
		if (num > 255)
		{
			num = 255;
			num2 = 255;
		}
		PdfDictionary pdfDictionary = new PdfDictionary();
		pdfDictionary["Type"] = new PdfName("Encoding");
		PdfArray pdfArray = new PdfArray();
		for (int j = num; j <= num2; j++)
		{
			string value = string.Empty;
			string @string = fontEncoding.GetString(new byte[1] { (byte)j }, 0, 1);
			char key = '?';
			if (@string.Length > 0)
			{
				key = @string.ToCharArray()[0];
			}
			CharacterMapTable.TryGetValue(key, out value);
			if (value != null)
			{
				num = j;
				break;
			}
		}
		bool flag = true;
		for (int k = num; k <= num2; k++)
		{
			if (array[k] != 0)
			{
				if (flag)
				{
					pdfArray.Add(new PdfNumber(k));
					flag = false;
				}
				string value2 = string.Empty;
				string string2 = fontEncoding.GetString(new byte[1] { (byte)k }, 0, 1);
				char key2 = '?';
				if (string2.Length > 0)
				{
					key2 = string2.ToCharArray()[0];
				}
				CharacterMapTable.TryGetValue(key2, out value2);
				if (value2 != null)
				{
					pdfArray.Add(new PdfName(value2));
				}
				else
				{
					pdfArray.Add(new PdfName(".notdef"));
				}
			}
			else
			{
				flag = true;
			}
		}
		pdfDictionary["Differences"] = pdfArray;
		dictionary["Encoding"] = pdfDictionary;
		dictionary["FirstChar"] = new PdfNumber(num);
		dictionary["LastChar"] = new PdfNumber(num2);
		PdfArray pdfArray2 = new PdfArray();
		for (int l = num; l <= num2; l++)
		{
			float value3 = 0f;
			int num3 = l;
			if (base.Metrics != null && base.Metrics.WidthTable != null && num3 > -1 && list.Contains(l) && base.Metrics.WidthTable.ToArray().Count > num3)
			{
				value3 = base.Metrics.WidthTable[num3];
				pdfArray2.Add(new PdfNumber(value3));
			}
			else
			{
				pdfArray2.Add(new PdfNumber(value3));
			}
		}
		dictionary["Widths"] = pdfArray2;
	}

	private void CheckStyle()
	{
		if (FontFamily == PdfFontFamily.Symbol || FontFamily == PdfFontFamily.ZapfDingbats)
		{
			PdfFontStyle style = base.Style;
			style &= ~(PdfFontStyle.Bold | PdfFontStyle.Italic);
			SetStyle(style);
		}
	}

	private float GetCharWidthInternal(char charCode, PdfStringFormat format)
	{
		float num = 0f;
		int num2 = 0;
		num2 = ((!Enum.IsDefined(typeof(PdfFontFamily), base.Name)) ? charCode : (charCode - 32));
		num2 = ((num2 >= 0 && num2 != 128) ? num2 : 0);
		if (fontEncoding != null && widthTableUpdate == null)
		{
			UpdateWidthTable();
		}
		WidthTable widthTable = base.Metrics.WidthTable;
		if (fontEncoding != null)
		{
			return widthTable[charCode];
		}
		return widthTable[num2];
	}

	internal static string Convert(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		byte[] bytes = new Windows1252Encoding().GetBytes(text);
		int num = bytes.Length;
		char[] array = new char[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = (char)bytes[i];
		}
		return new string(array);
	}

	internal static string Convert(string text, Encoding encoding)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		byte[] bytes = encoding.GetBytes(text);
		int num = bytes.Length;
		char[] array = new char[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = (char)bytes[i];
			if (!usedChars.Contains(bytes[i]))
			{
				usedChars.Add(bytes[i]);
			}
		}
		return new string(array);
	}

	private void UpdateWidthTable()
	{
		float[] array = new float[256];
		string empty = string.Empty;
		byte[] array2 = new byte[1];
		for (int i = 0; i < 256; i++)
		{
			array2[0] = (byte)i;
			empty = fontEncoding.GetString(array2, 0, 1);
			char key = ((empty.Length <= 0) ? '?' : empty[0]);
			CharacterMapTable.TryGetValue(key, out string value);
			if (value != null && FindWidth.ContainsKey(value))
			{
				array[i] = FindWidth[value];
			}
		}
		base.Metrics.WidthTable = new StandardWidthTable(array);
	}
}
