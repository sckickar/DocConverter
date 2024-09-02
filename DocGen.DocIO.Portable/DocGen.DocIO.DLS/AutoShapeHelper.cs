using System;
using System.Collections.Generic;

namespace DocGen.DocIO.DLS;

internal class AutoShapeHelper
{
	internal Dictionary<string, int> dictionary;

	internal Dictionary<string, string> shapeTypes = new Dictionary<string, string>();

	internal AutoShapeConstant GetAutoShapeConstant(string string_4)
	{
		if (string_4 != null)
		{
			if (dictionary == null || dictionary.Count == 0)
			{
				dictionary = new Dictionary<string, int>(187);
				dictionary.Add("line", 0);
				dictionary.Add("lineInv", 1);
				dictionary.Add("triangle", 2);
				dictionary.Add("rtTriangle", 3);
				dictionary.Add("rect", 4);
				dictionary.Add("diamond", 5);
				dictionary.Add("parallelogram", 6);
				dictionary.Add("trapezoid", 7);
				dictionary.Add("nonIsoscelesTrapezoid", 8);
				dictionary.Add("pentagon", 9);
				dictionary.Add("hexagon", 10);
				dictionary.Add("heptagon", 11);
				dictionary.Add("octagon", 12);
				dictionary.Add("decagon", 13);
				dictionary.Add("dodecagon", 14);
				dictionary.Add("star4", 15);
				dictionary.Add("star5", 16);
				dictionary.Add("star6", 17);
				dictionary.Add("star7", 18);
				dictionary.Add("star8", 19);
				dictionary.Add("star10", 20);
				dictionary.Add("star12", 21);
				dictionary.Add("star16", 22);
				dictionary.Add("star24", 23);
				dictionary.Add("star32", 24);
				dictionary.Add("roundRect", 25);
				dictionary.Add("round1Rect", 26);
				dictionary.Add("round2SameRect", 27);
				dictionary.Add("round2DiagRect", 28);
				dictionary.Add("snipRoundRect", 29);
				dictionary.Add("snip1Rect", 30);
				dictionary.Add("snip2SameRect", 31);
				dictionary.Add("snip2DiagRect", 32);
				dictionary.Add("plaque", 33);
				dictionary.Add("ellipse", 34);
				dictionary.Add("teardrop", 35);
				dictionary.Add("homePlate", 36);
				dictionary.Add("chevron", 37);
				dictionary.Add("pieWedge", 38);
				dictionary.Add("pie", 39);
				dictionary.Add("blockArc", 40);
				dictionary.Add("donut", 41);
				dictionary.Add("noSmoking", 42);
				dictionary.Add("rightArrow", 43);
				dictionary.Add("leftArrow", 44);
				dictionary.Add("upArrow", 45);
				dictionary.Add("downArrow", 46);
				dictionary.Add("stripedRightArrow", 47);
				dictionary.Add("notchedRightArrow", 48);
				dictionary.Add("bentUpArrow", 49);
				dictionary.Add("leftRightArrow", 50);
				dictionary.Add("upDownArrow", 51);
				dictionary.Add("leftUpArrow", 52);
				dictionary.Add("leftRightUpArrow", 53);
				dictionary.Add("quadArrow", 54);
				dictionary.Add("leftArrowCallout", 55);
				dictionary.Add("rightArrowCallout", 56);
				dictionary.Add("upArrowCallout", 57);
				dictionary.Add("downArrowCallout", 58);
				dictionary.Add("leftRightArrowCallout", 59);
				dictionary.Add("upDownArrowCallout", 60);
				dictionary.Add("quadArrowCallout", 61);
				dictionary.Add("bentArrow", 62);
				dictionary.Add("uturnArrow", 63);
				dictionary.Add("circularArrow", 64);
				dictionary.Add("leftCircularArrow", 65);
				dictionary.Add("leftRightCircularArrow", 66);
				dictionary.Add("curvedRightArrow", 67);
				dictionary.Add("curvedLeftArrow", 68);
				dictionary.Add("curvedUpArrow", 69);
				dictionary.Add("curvedDownArrow", 70);
				dictionary.Add("swooshArrow", 71);
				dictionary.Add("cube", 72);
				dictionary.Add("can", 73);
				dictionary.Add("lightningBolt", 74);
				dictionary.Add("heart", 75);
				dictionary.Add("sun", 76);
				dictionary.Add("moon", 77);
				dictionary.Add("smileyFace", 78);
				dictionary.Add("irregularSeal1", 79);
				dictionary.Add("irregularSeal2", 80);
				dictionary.Add("foldedCorner", 81);
				dictionary.Add("bevel", 82);
				dictionary.Add("frame", 83);
				dictionary.Add("halfFrame", 84);
				dictionary.Add("corner", 85);
				dictionary.Add("diagStripe", 86);
				dictionary.Add("chord", 87);
				dictionary.Add("arc", 88);
				dictionary.Add("leftBracket", 89);
				dictionary.Add("rightBracket", 90);
				dictionary.Add("leftBrace", 91);
				dictionary.Add("rightBrace", 92);
				dictionary.Add("bracketPair", 93);
				dictionary.Add("bracePair", 94);
				dictionary.Add("straightConnector1", 95);
				dictionary.Add("bentConnector2", 96);
				dictionary.Add("bentConnector3", 97);
				dictionary.Add("bentConnector4", 98);
				dictionary.Add("bentConnector5", 99);
				dictionary.Add("curvedConnector2", 100);
				dictionary.Add("curvedConnector3", 101);
				dictionary.Add("curvedConnector4", 102);
				dictionary.Add("curvedConnector5", 103);
				dictionary.Add("callout1", 104);
				dictionary.Add("callout2", 105);
				dictionary.Add("callout3", 106);
				dictionary.Add("accentCallout1", 107);
				dictionary.Add("accentCallout2", 108);
				dictionary.Add("accentCallout3", 109);
				dictionary.Add("borderCallout1", 110);
				dictionary.Add("borderCallout2", 111);
				dictionary.Add("borderCallout3", 112);
				dictionary.Add("accentBorderCallout1", 113);
				dictionary.Add("accentBorderCallout2", 114);
				dictionary.Add("accentBorderCallout3", 115);
				dictionary.Add("wedgeRectCallout", 116);
				dictionary.Add("wedgeRoundRectCallout", 117);
				dictionary.Add("wedgeEllipseCallout", 118);
				dictionary.Add("cloudCallout", 119);
				dictionary.Add("cloud", 120);
				dictionary.Add("ribbon", 121);
				dictionary.Add("ribbon2", 122);
				dictionary.Add("ellipseRibbon", 123);
				dictionary.Add("ellipseRibbon2", 124);
				dictionary.Add("leftRightRibbon", 125);
				dictionary.Add("verticalScroll", 126);
				dictionary.Add("horizontalScroll", 127);
				dictionary.Add("wave", 128);
				dictionary.Add("doubleWave", 129);
				dictionary.Add("plus", 130);
				dictionary.Add("flowChartProcess", 131);
				dictionary.Add("flowChartDecision", 132);
				dictionary.Add("flowChartInputOutput", 133);
				dictionary.Add("flowChartPredefinedProcess", 134);
				dictionary.Add("flowChartInternalStorage", 135);
				dictionary.Add("flowChartDocument", 136);
				dictionary.Add("flowChartMultidocument", 137);
				dictionary.Add("flowChartTerminator", 138);
				dictionary.Add("flowChartPreparation", 139);
				dictionary.Add("flowChartManualInput", 140);
				dictionary.Add("flowChartManualOperation", 141);
				dictionary.Add("flowChartConnector", 142);
				dictionary.Add("flowChartPunchedCard", 143);
				dictionary.Add("flowChartPunchedTape", 144);
				dictionary.Add("flowChartSummingJunction", 145);
				dictionary.Add("flowChartOr", 146);
				dictionary.Add("flowChartCollate", 147);
				dictionary.Add("flowChartSort", 148);
				dictionary.Add("flowChartExtract", 149);
				dictionary.Add("flowChartMerge", 150);
				dictionary.Add("flowChartOfflineStorage", 151);
				dictionary.Add("flowChartOnlineStorage", 152);
				dictionary.Add("flowChartMagneticTape", 153);
				dictionary.Add("flowChartMagneticDisk", 154);
				dictionary.Add("flowChartMagneticDrum", 155);
				dictionary.Add("flowChartDisplay", 156);
				dictionary.Add("flowChartDelay", 157);
				dictionary.Add("flowChartAlternateProcess", 158);
				dictionary.Add("flowChartOffpageConnector", 159);
				dictionary.Add("actionButtonBlank", 160);
				dictionary.Add("actionButtonHome", 161);
				dictionary.Add("actionButtonHelp", 162);
				dictionary.Add("actionButtonInformation", 163);
				dictionary.Add("actionButtonForwardNext", 164);
				dictionary.Add("actionButtonBackPrevious", 165);
				dictionary.Add("actionButtonEnd", 166);
				dictionary.Add("actionButtonBeginning", 167);
				dictionary.Add("actionButtonReturn", 168);
				dictionary.Add("actionButtonDocument", 169);
				dictionary.Add("actionButtonSound", 170);
				dictionary.Add("actionButtonMovie", 171);
				dictionary.Add("gear6", 172);
				dictionary.Add("gear9", 173);
				dictionary.Add("funnel", 174);
				dictionary.Add("mathPlus", 175);
				dictionary.Add("mathMinus", 176);
				dictionary.Add("mathMultiply", 177);
				dictionary.Add("mathDivide", 178);
				dictionary.Add("mathEqual", 179);
				dictionary.Add("mathNotEqual", 180);
				dictionary.Add("cornerTabs", 181);
				dictionary.Add("squareTabs", 182);
				dictionary.Add("plaqueTabs", 183);
				dictionary.Add("chartX", 184);
				dictionary.Add("chartStar", 185);
				dictionary.Add("chartPlus", 186);
			}
			if (dictionary.TryGetValue(string_4, out var value))
			{
				switch (value)
				{
				case 0:
					return AutoShapeConstant.Index_0;
				case 1:
					return AutoShapeConstant.Index_1;
				case 2:
					return AutoShapeConstant.Index_2;
				case 3:
					return AutoShapeConstant.Index_3;
				case 4:
					return AutoShapeConstant.Index_4;
				case 5:
					return AutoShapeConstant.Index_5;
				case 6:
					return AutoShapeConstant.Index_6;
				case 7:
					return AutoShapeConstant.Index_7;
				case 8:
					return AutoShapeConstant.Index_8;
				case 9:
					return AutoShapeConstant.Index_9;
				case 10:
					return AutoShapeConstant.Index_10;
				case 11:
					return AutoShapeConstant.Index_11;
				case 12:
					return AutoShapeConstant.Index_12;
				case 13:
					return AutoShapeConstant.Index_13;
				case 14:
					return AutoShapeConstant.Index_14;
				case 15:
					return AutoShapeConstant.Index_15;
				case 16:
					return AutoShapeConstant.Index_16;
				case 17:
					return AutoShapeConstant.Index_17;
				case 18:
					return AutoShapeConstant.Index_18;
				case 19:
					return AutoShapeConstant.Index_19;
				case 20:
					return AutoShapeConstant.Index_20;
				case 21:
					return AutoShapeConstant.Index_21;
				case 22:
					return AutoShapeConstant.Index_22;
				case 23:
					return AutoShapeConstant.Index_23;
				case 24:
					return AutoShapeConstant.Index_24;
				case 25:
					return AutoShapeConstant.Index_25;
				case 26:
					return AutoShapeConstant.Index_26;
				case 27:
					return AutoShapeConstant.Index_27;
				case 28:
					return AutoShapeConstant.Index_28;
				case 29:
					return AutoShapeConstant.Index_29;
				case 30:
					return AutoShapeConstant.Index_30;
				case 31:
					return AutoShapeConstant.Index_31;
				case 32:
					return AutoShapeConstant.Index_32;
				case 33:
					return AutoShapeConstant.Index_33;
				case 34:
					return AutoShapeConstant.Index_34;
				case 35:
					return AutoShapeConstant.Index_35;
				case 36:
					return AutoShapeConstant.Index_36;
				case 37:
					return AutoShapeConstant.Index_37;
				case 38:
					return AutoShapeConstant.Index_38;
				case 39:
					return AutoShapeConstant.Index_39;
				case 40:
					return AutoShapeConstant.Index_40;
				case 41:
					return AutoShapeConstant.Index_41;
				case 42:
					return AutoShapeConstant.Index_42;
				case 43:
					return AutoShapeConstant.Index_43;
				case 44:
					return AutoShapeConstant.Index_44;
				case 45:
					return AutoShapeConstant.Index_45;
				case 46:
					return AutoShapeConstant.Index_46;
				case 47:
					return AutoShapeConstant.Index_47;
				case 48:
					return AutoShapeConstant.Index_48;
				case 49:
					return AutoShapeConstant.Index_49;
				case 50:
					return AutoShapeConstant.Index_50;
				case 51:
					return AutoShapeConstant.Index_51;
				case 52:
					return AutoShapeConstant.Index_52;
				case 53:
					return AutoShapeConstant.Index_53;
				case 54:
					return AutoShapeConstant.Index_54;
				case 55:
					return AutoShapeConstant.Index_55;
				case 56:
					return AutoShapeConstant.Index_56;
				case 57:
					return AutoShapeConstant.Index_57;
				case 58:
					return AutoShapeConstant.Index_58;
				case 59:
					return AutoShapeConstant.Index_59;
				case 60:
					return AutoShapeConstant.Index_60;
				case 61:
					return AutoShapeConstant.Index_61;
				case 62:
					return AutoShapeConstant.Index_62;
				case 63:
					return AutoShapeConstant.Index_63;
				case 64:
					return AutoShapeConstant.Index_64;
				case 65:
					return AutoShapeConstant.Index_65;
				case 66:
					return AutoShapeConstant.Index_66;
				case 67:
					return AutoShapeConstant.Index_67;
				case 68:
					return AutoShapeConstant.Index_68;
				case 69:
					return AutoShapeConstant.Index_69;
				case 70:
					return AutoShapeConstant.Index_70;
				case 71:
					return AutoShapeConstant.Index_71;
				case 72:
					return AutoShapeConstant.Index_72;
				case 73:
					return AutoShapeConstant.Index_73;
				case 74:
					return AutoShapeConstant.Index_74;
				case 75:
					return AutoShapeConstant.Index_75;
				case 76:
					return AutoShapeConstant.Index_76;
				case 77:
					return AutoShapeConstant.Index_77;
				case 78:
					return AutoShapeConstant.Index_78;
				case 79:
					return AutoShapeConstant.Index_79;
				case 80:
					return AutoShapeConstant.Index_80;
				case 81:
					return AutoShapeConstant.Index_81;
				case 82:
					return AutoShapeConstant.Index_82;
				case 83:
					return AutoShapeConstant.Index_83;
				case 84:
					return AutoShapeConstant.Index_84;
				case 85:
					return AutoShapeConstant.Index_85;
				case 86:
					return AutoShapeConstant.Index_86;
				case 87:
					return AutoShapeConstant.Index_87;
				case 88:
					return AutoShapeConstant.Index_88;
				case 89:
					return AutoShapeConstant.Index_89;
				case 90:
					return AutoShapeConstant.Index_90;
				case 91:
					return AutoShapeConstant.Index_91;
				case 92:
					return AutoShapeConstant.Index_92;
				case 93:
					return AutoShapeConstant.Index_93;
				case 94:
					return AutoShapeConstant.Index_94;
				case 95:
					return AutoShapeConstant.Index_95;
				case 96:
					return AutoShapeConstant.Index_96;
				case 97:
					return AutoShapeConstant.Index_97;
				case 98:
					return AutoShapeConstant.Index_98;
				case 99:
					return AutoShapeConstant.Index_99;
				case 100:
					return AutoShapeConstant.Index_100;
				case 101:
					return AutoShapeConstant.Index_101;
				case 102:
					return AutoShapeConstant.Index_102;
				case 103:
					return AutoShapeConstant.Index_103;
				case 104:
					return AutoShapeConstant.Index_104;
				case 105:
					return AutoShapeConstant.Index_105;
				case 106:
					return AutoShapeConstant.Index_106;
				case 107:
					return AutoShapeConstant.Index_107;
				case 108:
					return AutoShapeConstant.Index_108;
				case 109:
					return AutoShapeConstant.Index_109;
				case 110:
					return AutoShapeConstant.Index_110;
				case 111:
					return AutoShapeConstant.Index_111;
				case 112:
					return AutoShapeConstant.Index_112;
				case 113:
					return AutoShapeConstant.Index_113;
				case 114:
					return AutoShapeConstant.Index_114;
				case 115:
					return AutoShapeConstant.Index_115;
				case 116:
					return AutoShapeConstant.Index_116;
				case 117:
					return AutoShapeConstant.Index_117;
				case 118:
					return AutoShapeConstant.Index_118;
				case 119:
					return AutoShapeConstant.Index_119;
				case 120:
					return AutoShapeConstant.Index_120;
				case 121:
					return AutoShapeConstant.Index_121;
				case 122:
					return AutoShapeConstant.Index_122;
				case 123:
					return AutoShapeConstant.Index_123;
				case 124:
					return AutoShapeConstant.Index_124;
				case 125:
					return AutoShapeConstant.Index_125;
				case 126:
					return AutoShapeConstant.Index_126;
				case 127:
					return AutoShapeConstant.Index_127;
				case 128:
					return AutoShapeConstant.Index_128;
				case 129:
					return AutoShapeConstant.Index_129;
				case 130:
					return AutoShapeConstant.Index_130;
				case 131:
					return AutoShapeConstant.Index_131;
				case 132:
					return AutoShapeConstant.Index_132;
				case 133:
					return AutoShapeConstant.Index_133;
				case 134:
					return AutoShapeConstant.Index_134;
				case 135:
					return AutoShapeConstant.Index_135;
				case 136:
					return AutoShapeConstant.Index_136;
				case 137:
					return AutoShapeConstant.Index_137;
				case 138:
					return AutoShapeConstant.Index_138;
				case 139:
					return AutoShapeConstant.Index_139;
				case 140:
					return AutoShapeConstant.Index_140;
				case 141:
					return AutoShapeConstant.Index_141;
				case 142:
					return AutoShapeConstant.Index_142;
				case 143:
					return AutoShapeConstant.Index_143;
				case 144:
					return AutoShapeConstant.Index_144;
				case 145:
					return AutoShapeConstant.Index_145;
				case 146:
					return AutoShapeConstant.Index_146;
				case 147:
					return AutoShapeConstant.Index_147;
				case 148:
					return AutoShapeConstant.Index_148;
				case 149:
					return AutoShapeConstant.Index_149;
				case 150:
					return AutoShapeConstant.Index_150;
				case 151:
					return AutoShapeConstant.Index_151;
				case 152:
					return AutoShapeConstant.Index_152;
				case 153:
					return AutoShapeConstant.Index_153;
				case 154:
					return AutoShapeConstant.Index_154;
				case 155:
					return AutoShapeConstant.Index_155;
				case 156:
					return AutoShapeConstant.Index_156;
				case 157:
					return AutoShapeConstant.Index_157;
				case 158:
					return AutoShapeConstant.Index_158;
				case 159:
					return AutoShapeConstant.Index_159;
				case 160:
					return AutoShapeConstant.Index_160;
				case 161:
					return AutoShapeConstant.Index_161;
				case 162:
					return AutoShapeConstant.Index_162;
				case 163:
					return AutoShapeConstant.Index_163;
				case 164:
					return AutoShapeConstant.Index_164;
				case 165:
					return AutoShapeConstant.Index_165;
				case 166:
					return AutoShapeConstant.Index_166;
				case 167:
					return AutoShapeConstant.Index_167;
				case 168:
					return AutoShapeConstant.Index_168;
				case 169:
					return AutoShapeConstant.Index_169;
				case 170:
					return AutoShapeConstant.Index_170;
				case 171:
					return AutoShapeConstant.Index_171;
				case 172:
					return AutoShapeConstant.Index_172;
				case 173:
					return AutoShapeConstant.Index_173;
				case 174:
					return AutoShapeConstant.Index_174;
				case 175:
					return AutoShapeConstant.Index_175;
				case 176:
					return AutoShapeConstant.Index_176;
				case 177:
					return AutoShapeConstant.Index_177;
				case 178:
					return AutoShapeConstant.Index_178;
				case 179:
					return AutoShapeConstant.Index_179;
				case 180:
					return AutoShapeConstant.Index_180;
				case 181:
					return AutoShapeConstant.Index_181;
				case 182:
					return AutoShapeConstant.Index_182;
				case 183:
					return AutoShapeConstant.Index_183;
				case 184:
					return AutoShapeConstant.Index_184;
				case 185:
					return AutoShapeConstant.Index_185;
				case 186:
					return AutoShapeConstant.Index_186;
				}
			}
		}
		return AutoShapeConstant.Index_187;
	}

	internal AutoShapeType GetAutoShapeType(AutoShapeConstant enum184_0)
	{
		return enum184_0 switch
		{
			AutoShapeConstant.Index_0 => AutoShapeType.Line, 
			AutoShapeConstant.Index_2 => AutoShapeType.IsoscelesTriangle, 
			AutoShapeConstant.Index_3 => AutoShapeType.RightTriangle, 
			AutoShapeConstant.Index_4 => AutoShapeType.Rectangle, 
			AutoShapeConstant.Index_5 => AutoShapeType.Diamond, 
			AutoShapeConstant.Index_6 => AutoShapeType.Parallelogram, 
			AutoShapeConstant.Index_7 => AutoShapeType.Trapezoid, 
			AutoShapeConstant.Index_9 => AutoShapeType.RegularPentagon, 
			AutoShapeConstant.Index_10 => AutoShapeType.Hexagon, 
			AutoShapeConstant.Index_11 => AutoShapeType.Heptagon, 
			AutoShapeConstant.Index_12 => AutoShapeType.Octagon, 
			AutoShapeConstant.Index_13 => AutoShapeType.Decagon, 
			AutoShapeConstant.Index_14 => AutoShapeType.Dodecagon, 
			AutoShapeConstant.Index_15 => AutoShapeType.Star4Point, 
			AutoShapeConstant.Index_16 => AutoShapeType.Star5Point, 
			AutoShapeConstant.Index_17 => AutoShapeType.Star6Point, 
			AutoShapeConstant.Index_18 => AutoShapeType.Star7Point, 
			AutoShapeConstant.Index_19 => AutoShapeType.Star8Point, 
			AutoShapeConstant.Index_20 => AutoShapeType.Star10Point, 
			AutoShapeConstant.Index_21 => AutoShapeType.Star12Point, 
			AutoShapeConstant.Index_22 => AutoShapeType.Star16Point, 
			AutoShapeConstant.Index_23 => AutoShapeType.Star24Point, 
			AutoShapeConstant.Index_24 => AutoShapeType.Star32Point, 
			AutoShapeConstant.Index_25 => AutoShapeType.RoundedRectangle, 
			AutoShapeConstant.Index_26 => AutoShapeType.RoundSingleCornerRectangle, 
			AutoShapeConstant.Index_27 => AutoShapeType.RoundSameSideCornerRectangle, 
			AutoShapeConstant.Index_28 => AutoShapeType.RoundDiagonalCornerRectangle, 
			AutoShapeConstant.Index_29 => AutoShapeType.SnipAndRoundSingleCornerRectangle, 
			AutoShapeConstant.Index_30 => AutoShapeType.SnipSingleCornerRectangle, 
			AutoShapeConstant.Index_31 => AutoShapeType.SnipSameSideCornerRectangle, 
			AutoShapeConstant.Index_32 => AutoShapeType.SnipDiagonalCornerRectangle, 
			AutoShapeConstant.Index_33 => AutoShapeType.Plaque, 
			AutoShapeConstant.Index_34 => AutoShapeType.Oval, 
			AutoShapeConstant.Index_35 => AutoShapeType.Teardrop, 
			AutoShapeConstant.Index_36 => AutoShapeType.Pentagon, 
			AutoShapeConstant.Index_37 => AutoShapeType.Chevron, 
			AutoShapeConstant.Index_39 => AutoShapeType.Pie, 
			AutoShapeConstant.Index_40 => AutoShapeType.BlockArc, 
			AutoShapeConstant.Index_41 => AutoShapeType.Donut, 
			AutoShapeConstant.Index_42 => AutoShapeType.NoSymbol, 
			AutoShapeConstant.Index_43 => AutoShapeType.RightArrow, 
			AutoShapeConstant.Index_44 => AutoShapeType.LeftArrow, 
			AutoShapeConstant.Index_45 => AutoShapeType.UpArrow, 
			AutoShapeConstant.Index_46 => AutoShapeType.DownArrow, 
			AutoShapeConstant.Index_47 => AutoShapeType.StripedRightArrow, 
			AutoShapeConstant.Index_48 => AutoShapeType.NotchedRightArrow, 
			AutoShapeConstant.Index_49 => AutoShapeType.BentUpArrow, 
			AutoShapeConstant.Index_50 => AutoShapeType.LeftRightArrow, 
			AutoShapeConstant.Index_51 => AutoShapeType.UpDownArrow, 
			AutoShapeConstant.Index_52 => AutoShapeType.LeftUpArrow, 
			AutoShapeConstant.Index_53 => AutoShapeType.LeftRightUpArrow, 
			AutoShapeConstant.Index_54 => AutoShapeType.QuadArrow, 
			AutoShapeConstant.Index_55 => AutoShapeType.LeftArrowCallout, 
			AutoShapeConstant.Index_56 => AutoShapeType.RightArrowCallout, 
			AutoShapeConstant.Index_57 => AutoShapeType.UpArrowCallout, 
			AutoShapeConstant.Index_58 => AutoShapeType.DownArrowCallout, 
			AutoShapeConstant.Index_59 => AutoShapeType.LeftRightArrowCallout, 
			AutoShapeConstant.Index_60 => AutoShapeType.UpDownArrowCallout, 
			AutoShapeConstant.Index_61 => AutoShapeType.QuadArrowCallout, 
			AutoShapeConstant.Index_62 => AutoShapeType.BentArrow, 
			AutoShapeConstant.Index_63 => AutoShapeType.UTurnArrow, 
			AutoShapeConstant.Index_64 => AutoShapeType.CircularArrow, 
			AutoShapeConstant.Index_67 => AutoShapeType.CurvedRightArrow, 
			AutoShapeConstant.Index_68 => AutoShapeType.CurvedLeftArrow, 
			AutoShapeConstant.Index_69 => AutoShapeType.CurvedUpArrow, 
			AutoShapeConstant.Index_70 => AutoShapeType.CurvedDownArrow, 
			AutoShapeConstant.Index_72 => AutoShapeType.Cube, 
			AutoShapeConstant.Index_73 => AutoShapeType.Can, 
			AutoShapeConstant.Index_74 => AutoShapeType.LightningBolt, 
			AutoShapeConstant.Index_75 => AutoShapeType.Heart, 
			AutoShapeConstant.Index_76 => AutoShapeType.Sun, 
			AutoShapeConstant.Index_77 => AutoShapeType.Moon, 
			AutoShapeConstant.Index_78 => AutoShapeType.SmileyFace, 
			AutoShapeConstant.Index_79 => AutoShapeType.Explosion1, 
			AutoShapeConstant.Index_80 => AutoShapeType.Explosion2, 
			AutoShapeConstant.Index_81 => AutoShapeType.FoldedCorner, 
			AutoShapeConstant.Index_82 => AutoShapeType.Bevel, 
			AutoShapeConstant.Index_83 => AutoShapeType.Frame, 
			AutoShapeConstant.Index_84 => AutoShapeType.HalfFrame, 
			AutoShapeConstant.Index_85 => AutoShapeType.L_Shape, 
			AutoShapeConstant.Index_86 => AutoShapeType.DiagonalStripe, 
			AutoShapeConstant.Index_87 => AutoShapeType.Chord, 
			AutoShapeConstant.Index_88 => AutoShapeType.Arc, 
			AutoShapeConstant.Index_89 => AutoShapeType.LeftBracket, 
			AutoShapeConstant.Index_90 => AutoShapeType.RightBracket, 
			AutoShapeConstant.Index_91 => AutoShapeType.LeftBrace, 
			AutoShapeConstant.Index_92 => AutoShapeType.RightBrace, 
			AutoShapeConstant.Index_93 => AutoShapeType.DoubleBracket, 
			AutoShapeConstant.Index_94 => AutoShapeType.DoubleBrace, 
			AutoShapeConstant.Index_95 => AutoShapeType.StraightConnector, 
			AutoShapeConstant.Index_96 => AutoShapeType.BentConnector2, 
			AutoShapeConstant.Index_97 => AutoShapeType.ElbowConnector, 
			AutoShapeConstant.Index_98 => AutoShapeType.BentConnector4, 
			AutoShapeConstant.Index_99 => AutoShapeType.BentConnector5, 
			AutoShapeConstant.Index_100 => AutoShapeType.CurvedConnector2, 
			AutoShapeConstant.Index_101 => AutoShapeType.CurvedConnector, 
			AutoShapeConstant.Index_102 => AutoShapeType.CurvedConnector4, 
			AutoShapeConstant.Index_103 => AutoShapeType.CurvedConnector5, 
			AutoShapeConstant.Index_104 => AutoShapeType.LineCallout1NoBorder, 
			AutoShapeConstant.Index_105 => AutoShapeType.LineCallout2NoBorder, 
			AutoShapeConstant.Index_106 => AutoShapeType.LineCallout3NoBorder, 
			AutoShapeConstant.Index_107 => AutoShapeType.LineCallout1AccentBar, 
			AutoShapeConstant.Index_108 => AutoShapeType.LineCallout2AccentBar, 
			AutoShapeConstant.Index_109 => AutoShapeType.LineCallout3AccentBar, 
			AutoShapeConstant.Index_110 => AutoShapeType.LineCallout1, 
			AutoShapeConstant.Index_111 => AutoShapeType.LineCallout2, 
			AutoShapeConstant.Index_112 => AutoShapeType.LineCallout3, 
			AutoShapeConstant.Index_113 => AutoShapeType.LineCallout1BorderAndAccentBar, 
			AutoShapeConstant.Index_114 => AutoShapeType.LineCallout2BorderAndAccentBar, 
			AutoShapeConstant.Index_115 => AutoShapeType.LineCallout3BorderAndAccentBar, 
			AutoShapeConstant.Index_116 => AutoShapeType.RectangularCallout, 
			AutoShapeConstant.Index_117 => AutoShapeType.RoundedRectangularCallout, 
			AutoShapeConstant.Index_118 => AutoShapeType.OvalCallout, 
			AutoShapeConstant.Index_119 => AutoShapeType.CloudCallout, 
			AutoShapeConstant.Index_120 => AutoShapeType.Cloud, 
			AutoShapeConstant.Index_121 => AutoShapeType.DownRibbon, 
			AutoShapeConstant.Index_122 => AutoShapeType.UpRibbon, 
			AutoShapeConstant.Index_123 => AutoShapeType.CurvedDownRibbon, 
			AutoShapeConstant.Index_124 => AutoShapeType.CurvedUpRibbon, 
			AutoShapeConstant.Index_126 => AutoShapeType.VerticalScroll, 
			AutoShapeConstant.Index_127 => AutoShapeType.HorizontalScroll, 
			AutoShapeConstant.Index_128 => AutoShapeType.Wave, 
			AutoShapeConstant.Index_129 => AutoShapeType.DoubleWave, 
			AutoShapeConstant.Index_130 => AutoShapeType.Cross, 
			AutoShapeConstant.Index_131 => AutoShapeType.FlowChartProcess, 
			AutoShapeConstant.Index_132 => AutoShapeType.FlowChartDecision, 
			AutoShapeConstant.Index_133 => AutoShapeType.FlowChartData, 
			AutoShapeConstant.Index_134 => AutoShapeType.FlowChartPredefinedProcess, 
			AutoShapeConstant.Index_135 => AutoShapeType.FlowChartInternalStorage, 
			AutoShapeConstant.Index_136 => AutoShapeType.FlowChartDocument, 
			AutoShapeConstant.Index_137 => AutoShapeType.FlowChartMultiDocument, 
			AutoShapeConstant.Index_138 => AutoShapeType.FlowChartTerminator, 
			AutoShapeConstant.Index_139 => AutoShapeType.FlowChartPreparation, 
			AutoShapeConstant.Index_140 => AutoShapeType.FlowChartManualInput, 
			AutoShapeConstant.Index_141 => AutoShapeType.FlowChartManualOperation, 
			AutoShapeConstant.Index_142 => AutoShapeType.FlowChartConnector, 
			AutoShapeConstant.Index_143 => AutoShapeType.FlowChartCard, 
			AutoShapeConstant.Index_144 => AutoShapeType.FlowChartPunchedTape, 
			AutoShapeConstant.Index_145 => AutoShapeType.FlowChartSummingJunction, 
			AutoShapeConstant.Index_146 => AutoShapeType.FlowChartOr, 
			AutoShapeConstant.Index_147 => AutoShapeType.FlowChartCollate, 
			AutoShapeConstant.Index_148 => AutoShapeType.FlowChartSort, 
			AutoShapeConstant.Index_149 => AutoShapeType.FlowChartExtract, 
			AutoShapeConstant.Index_150 => AutoShapeType.FlowChartMerge, 
			AutoShapeConstant.Index_152 => AutoShapeType.FlowChartStoredData, 
			AutoShapeConstant.Index_153 => AutoShapeType.FlowChartSequentialAccessStorage, 
			AutoShapeConstant.Index_154 => AutoShapeType.FlowChartMagneticDisk, 
			AutoShapeConstant.Index_155 => AutoShapeType.FlowChartDirectAccessStorage, 
			AutoShapeConstant.Index_156 => AutoShapeType.FlowChartDisplay, 
			AutoShapeConstant.Index_157 => AutoShapeType.FlowChartDelay, 
			AutoShapeConstant.Index_158 => AutoShapeType.FlowChartAlternateProcess, 
			AutoShapeConstant.Index_159 => AutoShapeType.FlowChartOffPageConnector, 
			AutoShapeConstant.Index_175 => AutoShapeType.MathPlus, 
			AutoShapeConstant.Index_176 => AutoShapeType.MathMinus, 
			AutoShapeConstant.Index_177 => AutoShapeType.MathMultiply, 
			AutoShapeConstant.Index_178 => AutoShapeType.MathDivision, 
			AutoShapeConstant.Index_179 => AutoShapeType.MathEqual, 
			AutoShapeConstant.Index_180 => AutoShapeType.MathNotEqual, 
			_ => AutoShapeType.Unknown, 
		};
	}

	internal AutoShapeConstant GetAutoShapeConstant(AutoShapeType autoShapeType_0)
	{
		return autoShapeType_0 switch
		{
			AutoShapeType.Rectangle => AutoShapeConstant.Index_4, 
			AutoShapeType.RoundedRectangle => AutoShapeConstant.Index_25, 
			AutoShapeType.Oval => AutoShapeConstant.Index_34, 
			AutoShapeType.Diamond => AutoShapeConstant.Index_5, 
			AutoShapeType.IsoscelesTriangle => AutoShapeConstant.Index_2, 
			AutoShapeType.RightTriangle => AutoShapeConstant.Index_3, 
			AutoShapeType.Parallelogram => AutoShapeConstant.Index_6, 
			AutoShapeType.Trapezoid => AutoShapeConstant.Index_7, 
			AutoShapeType.Hexagon => AutoShapeConstant.Index_10, 
			AutoShapeType.Octagon => AutoShapeConstant.Index_12, 
			AutoShapeType.Cross => AutoShapeConstant.Index_130, 
			AutoShapeType.Star5Point => AutoShapeConstant.Index_16, 
			AutoShapeType.RightArrow => AutoShapeConstant.Index_43, 
			AutoShapeType.Pentagon => AutoShapeConstant.Index_36, 
			AutoShapeType.Cube => AutoShapeConstant.Index_72, 
			AutoShapeType.Arc => AutoShapeConstant.Index_88, 
			AutoShapeType.Line => AutoShapeConstant.Index_0, 
			AutoShapeType.Plaque => AutoShapeConstant.Index_33, 
			AutoShapeType.Can => AutoShapeConstant.Index_73, 
			AutoShapeType.Donut => AutoShapeConstant.Index_41, 
			AutoShapeType.StraightConnector => AutoShapeConstant.Index_95, 
			AutoShapeType.BentConnector2 => AutoShapeConstant.Index_96, 
			AutoShapeType.ElbowConnector => AutoShapeConstant.Index_97, 
			AutoShapeType.BentConnector4 => AutoShapeConstant.Index_98, 
			AutoShapeType.BentConnector5 => AutoShapeConstant.Index_99, 
			AutoShapeType.CurvedConnector2 => AutoShapeConstant.Index_100, 
			AutoShapeType.CurvedConnector => AutoShapeConstant.Index_101, 
			AutoShapeType.CurvedConnector4 => AutoShapeConstant.Index_102, 
			AutoShapeType.CurvedConnector5 => AutoShapeConstant.Index_103, 
			AutoShapeType.LineCallout1 => AutoShapeConstant.Index_110, 
			AutoShapeType.LineCallout2 => AutoShapeConstant.Index_111, 
			AutoShapeType.LineCallout3 => AutoShapeConstant.Index_112, 
			AutoShapeType.LineCallout1AccentBar => AutoShapeConstant.Index_107, 
			AutoShapeType.LineCallout2AccentBar => AutoShapeConstant.Index_108, 
			AutoShapeType.LineCallout3AccentBar => AutoShapeConstant.Index_109, 
			AutoShapeType.LineCallout1NoBorder => AutoShapeConstant.Index_104, 
			AutoShapeType.LineCallout2NoBorder => AutoShapeConstant.Index_105, 
			AutoShapeType.LineCallout3NoBorder => AutoShapeConstant.Index_106, 
			AutoShapeType.LineCallout1BorderAndAccentBar => AutoShapeConstant.Index_113, 
			AutoShapeType.LineCallout2BorderAndAccentBar => AutoShapeConstant.Index_114, 
			AutoShapeType.LineCallout3BorderAndAccentBar => AutoShapeConstant.Index_115, 
			AutoShapeType.DownRibbon => AutoShapeConstant.Index_121, 
			AutoShapeType.UpRibbon => AutoShapeConstant.Index_122, 
			AutoShapeType.Chevron => AutoShapeConstant.Index_37, 
			AutoShapeType.RegularPentagon => AutoShapeConstant.Index_9, 
			AutoShapeType.NoSymbol => AutoShapeConstant.Index_42, 
			AutoShapeType.Star8Point => AutoShapeConstant.Index_19, 
			AutoShapeType.Star16Point => AutoShapeConstant.Index_22, 
			AutoShapeType.Star32Point => AutoShapeConstant.Index_24, 
			AutoShapeType.RectangularCallout => AutoShapeConstant.Index_116, 
			AutoShapeType.RoundedRectangularCallout => AutoShapeConstant.Index_117, 
			AutoShapeType.OvalCallout => AutoShapeConstant.Index_118, 
			AutoShapeType.Wave => AutoShapeConstant.Index_128, 
			AutoShapeType.FoldedCorner => AutoShapeConstant.Index_81, 
			AutoShapeType.LeftArrow => AutoShapeConstant.Index_44, 
			AutoShapeType.DownArrow => AutoShapeConstant.Index_46, 
			AutoShapeType.UpArrow => AutoShapeConstant.Index_45, 
			AutoShapeType.LeftRightArrow => AutoShapeConstant.Index_50, 
			AutoShapeType.UpDownArrow => AutoShapeConstant.Index_51, 
			AutoShapeType.Explosion1 => AutoShapeConstant.Index_79, 
			AutoShapeType.Explosion2 => AutoShapeConstant.Index_80, 
			AutoShapeType.LightningBolt => AutoShapeConstant.Index_74, 
			AutoShapeType.Heart => AutoShapeConstant.Index_75, 
			AutoShapeType.QuadArrow => AutoShapeConstant.Index_54, 
			AutoShapeType.LeftArrowCallout => AutoShapeConstant.Index_55, 
			AutoShapeType.RightArrowCallout => AutoShapeConstant.Index_56, 
			AutoShapeType.UpArrowCallout => AutoShapeConstant.Index_57, 
			AutoShapeType.DownArrowCallout => AutoShapeConstant.Index_58, 
			AutoShapeType.LeftRightArrowCallout => AutoShapeConstant.Index_59, 
			AutoShapeType.UpDownArrowCallout => AutoShapeConstant.Index_60, 
			AutoShapeType.QuadArrowCallout => AutoShapeConstant.Index_61, 
			AutoShapeType.Bevel => AutoShapeConstant.Index_82, 
			AutoShapeType.LeftBracket => AutoShapeConstant.Index_89, 
			AutoShapeType.RightBracket => AutoShapeConstant.Index_90, 
			AutoShapeType.LeftBrace => AutoShapeConstant.Index_91, 
			AutoShapeType.RightBrace => AutoShapeConstant.Index_92, 
			AutoShapeType.LeftUpArrow => AutoShapeConstant.Index_52, 
			AutoShapeType.BentUpArrow => AutoShapeConstant.Index_49, 
			AutoShapeType.BentArrow => AutoShapeConstant.Index_62, 
			AutoShapeType.Star24Point => AutoShapeConstant.Index_23, 
			AutoShapeType.StripedRightArrow => AutoShapeConstant.Index_47, 
			AutoShapeType.NotchedRightArrow => AutoShapeConstant.Index_48, 
			AutoShapeType.BlockArc => AutoShapeConstant.Index_40, 
			AutoShapeType.SmileyFace => AutoShapeConstant.Index_78, 
			AutoShapeType.VerticalScroll => AutoShapeConstant.Index_126, 
			AutoShapeType.HorizontalScroll => AutoShapeConstant.Index_127, 
			AutoShapeType.CircularArrow => AutoShapeConstant.Index_64, 
			AutoShapeType.UTurnArrow => AutoShapeConstant.Index_63, 
			AutoShapeType.CurvedRightArrow => AutoShapeConstant.Index_67, 
			AutoShapeType.CurvedLeftArrow => AutoShapeConstant.Index_68, 
			AutoShapeType.CurvedUpArrow => AutoShapeConstant.Index_69, 
			AutoShapeType.CurvedDownArrow => AutoShapeConstant.Index_70, 
			AutoShapeType.CloudCallout => AutoShapeConstant.Index_119, 
			AutoShapeType.CurvedDownRibbon => AutoShapeConstant.Index_123, 
			AutoShapeType.CurvedUpRibbon => AutoShapeConstant.Index_124, 
			AutoShapeType.FlowChartProcess => AutoShapeConstant.Index_131, 
			AutoShapeType.FlowChartDecision => AutoShapeConstant.Index_132, 
			AutoShapeType.FlowChartData => AutoShapeConstant.Index_133, 
			AutoShapeType.FlowChartPredefinedProcess => AutoShapeConstant.Index_134, 
			AutoShapeType.FlowChartInternalStorage => AutoShapeConstant.Index_135, 
			AutoShapeType.FlowChartDocument => AutoShapeConstant.Index_136, 
			AutoShapeType.FlowChartMultiDocument => AutoShapeConstant.Index_137, 
			AutoShapeType.FlowChartTerminator => AutoShapeConstant.Index_138, 
			AutoShapeType.FlowChartPreparation => AutoShapeConstant.Index_139, 
			AutoShapeType.FlowChartManualInput => AutoShapeConstant.Index_140, 
			AutoShapeType.FlowChartManualOperation => AutoShapeConstant.Index_141, 
			AutoShapeType.FlowChartConnector => AutoShapeConstant.Index_142, 
			AutoShapeType.FlowChartCard => AutoShapeConstant.Index_143, 
			AutoShapeType.FlowChartPunchedTape => AutoShapeConstant.Index_144, 
			AutoShapeType.FlowChartSummingJunction => AutoShapeConstant.Index_145, 
			AutoShapeType.FlowChartOr => AutoShapeConstant.Index_146, 
			AutoShapeType.FlowChartCollate => AutoShapeConstant.Index_147, 
			AutoShapeType.FlowChartSort => AutoShapeConstant.Index_148, 
			AutoShapeType.FlowChartExtract => AutoShapeConstant.Index_149, 
			AutoShapeType.FlowChartMerge => AutoShapeConstant.Index_150, 
			AutoShapeType.FlowChartStoredData => AutoShapeConstant.Index_152, 
			AutoShapeType.FlowChartSequentialAccessStorage => AutoShapeConstant.Index_153, 
			AutoShapeType.FlowChartMagneticDisk => AutoShapeConstant.Index_154, 
			AutoShapeType.FlowChartDirectAccessStorage => AutoShapeConstant.Index_155, 
			AutoShapeType.FlowChartDisplay => AutoShapeConstant.Index_156, 
			AutoShapeType.FlowChartDelay => AutoShapeConstant.Index_157, 
			AutoShapeType.FlowChartAlternateProcess => AutoShapeConstant.Index_158, 
			AutoShapeType.FlowChartOffPageConnector => AutoShapeConstant.Index_159, 
			AutoShapeType.LeftRightUpArrow => AutoShapeConstant.Index_53, 
			AutoShapeType.Sun => AutoShapeConstant.Index_76, 
			AutoShapeType.Moon => AutoShapeConstant.Index_77, 
			AutoShapeType.DoubleBracket => AutoShapeConstant.Index_93, 
			AutoShapeType.DoubleBrace => AutoShapeConstant.Index_94, 
			AutoShapeType.Star4Point => AutoShapeConstant.Index_15, 
			AutoShapeType.DoubleWave => AutoShapeConstant.Index_129, 
			AutoShapeType.Heptagon => AutoShapeConstant.Index_11, 
			AutoShapeType.Decagon => AutoShapeConstant.Index_13, 
			AutoShapeType.Dodecagon => AutoShapeConstant.Index_14, 
			AutoShapeType.Star6Point => AutoShapeConstant.Index_17, 
			AutoShapeType.Star7Point => AutoShapeConstant.Index_18, 
			AutoShapeType.Star10Point => AutoShapeConstant.Index_20, 
			AutoShapeType.Star12Point => AutoShapeConstant.Index_21, 
			AutoShapeType.RoundSingleCornerRectangle => AutoShapeConstant.Index_26, 
			AutoShapeType.RoundSameSideCornerRectangle => AutoShapeConstant.Index_27, 
			AutoShapeType.RoundDiagonalCornerRectangle => AutoShapeConstant.Index_28, 
			AutoShapeType.SnipAndRoundSingleCornerRectangle => AutoShapeConstant.Index_29, 
			AutoShapeType.SnipSingleCornerRectangle => AutoShapeConstant.Index_30, 
			AutoShapeType.SnipSameSideCornerRectangle => AutoShapeConstant.Index_31, 
			AutoShapeType.SnipDiagonalCornerRectangle => AutoShapeConstant.Index_32, 
			AutoShapeType.Teardrop => AutoShapeConstant.Index_35, 
			AutoShapeType.Pie => AutoShapeConstant.Index_39, 
			AutoShapeType.Frame => AutoShapeConstant.Index_83, 
			AutoShapeType.HalfFrame => AutoShapeConstant.Index_84, 
			AutoShapeType.L_Shape => AutoShapeConstant.Index_85, 
			AutoShapeType.DiagonalStripe => AutoShapeConstant.Index_86, 
			AutoShapeType.Chord => AutoShapeConstant.Index_87, 
			AutoShapeType.Cloud => AutoShapeConstant.Index_120, 
			AutoShapeType.MathPlus => AutoShapeConstant.Index_175, 
			AutoShapeType.MathMinus => AutoShapeConstant.Index_176, 
			AutoShapeType.MathMultiply => AutoShapeConstant.Index_177, 
			AutoShapeType.MathDivision => AutoShapeConstant.Index_178, 
			AutoShapeType.MathEqual => AutoShapeConstant.Index_179, 
			AutoShapeType.MathNotEqual => AutoShapeConstant.Index_180, 
			_ => AutoShapeConstant.Index_187, 
		};
	}

	internal string GetAutoShapeString(AutoShapeConstant enum184_0)
	{
		return enum184_0 switch
		{
			AutoShapeConstant.Index_0 => "line", 
			AutoShapeConstant.Index_1 => "lineInv", 
			AutoShapeConstant.Index_2 => "triangle", 
			AutoShapeConstant.Index_3 => "rtTriangle", 
			AutoShapeConstant.Index_4 => "rect", 
			AutoShapeConstant.Index_5 => "diamond", 
			AutoShapeConstant.Index_6 => "parallelogram", 
			AutoShapeConstant.Index_7 => "trapezoid", 
			AutoShapeConstant.Index_8 => "nonIsoscelesTrapezoid", 
			AutoShapeConstant.Index_9 => "pentagon", 
			AutoShapeConstant.Index_10 => "hexagon", 
			AutoShapeConstant.Index_11 => "heptagon", 
			AutoShapeConstant.Index_12 => "octagon", 
			AutoShapeConstant.Index_13 => "decagon", 
			AutoShapeConstant.Index_14 => "dodecagon", 
			AutoShapeConstant.Index_15 => "star4", 
			AutoShapeConstant.Index_16 => "star5", 
			AutoShapeConstant.Index_17 => "star6", 
			AutoShapeConstant.Index_18 => "star7", 
			AutoShapeConstant.Index_19 => "star8", 
			AutoShapeConstant.Index_20 => "star10", 
			AutoShapeConstant.Index_21 => "star12", 
			AutoShapeConstant.Index_22 => "star16", 
			AutoShapeConstant.Index_23 => "star24", 
			AutoShapeConstant.Index_24 => "star32", 
			AutoShapeConstant.Index_25 => "roundRect", 
			AutoShapeConstant.Index_26 => "round1Rect", 
			AutoShapeConstant.Index_27 => "round2SameRect", 
			AutoShapeConstant.Index_28 => "round2DiagRect", 
			AutoShapeConstant.Index_29 => "snipRoundRect", 
			AutoShapeConstant.Index_30 => "snip1Rect", 
			AutoShapeConstant.Index_31 => "snip2SameRect", 
			AutoShapeConstant.Index_32 => "snip2DiagRect", 
			AutoShapeConstant.Index_33 => "plaque", 
			AutoShapeConstant.Index_34 => "ellipse", 
			AutoShapeConstant.Index_35 => "teardrop", 
			AutoShapeConstant.Index_36 => "homePlate", 
			AutoShapeConstant.Index_37 => "chevron", 
			AutoShapeConstant.Index_38 => "pieWedge", 
			AutoShapeConstant.Index_39 => "pie", 
			AutoShapeConstant.Index_40 => "blockArc", 
			AutoShapeConstant.Index_41 => "donut", 
			AutoShapeConstant.Index_42 => "noSmoking", 
			AutoShapeConstant.Index_43 => "rightArrow", 
			AutoShapeConstant.Index_44 => "leftArrow", 
			AutoShapeConstant.Index_45 => "upArrow", 
			AutoShapeConstant.Index_46 => "downArrow", 
			AutoShapeConstant.Index_47 => "stripedRightArrow", 
			AutoShapeConstant.Index_48 => "notchedRightArrow", 
			AutoShapeConstant.Index_49 => "bentUpArrow", 
			AutoShapeConstant.Index_50 => "leftRightArrow", 
			AutoShapeConstant.Index_51 => "upDownArrow", 
			AutoShapeConstant.Index_52 => "leftUpArrow", 
			AutoShapeConstant.Index_53 => "leftRightUpArrow", 
			AutoShapeConstant.Index_54 => "quadArrow", 
			AutoShapeConstant.Index_55 => "leftArrowCallout", 
			AutoShapeConstant.Index_56 => "rightArrowCallout", 
			AutoShapeConstant.Index_57 => "upArrowCallout", 
			AutoShapeConstant.Index_58 => "downArrowCallout", 
			AutoShapeConstant.Index_59 => "leftRightArrowCallout", 
			AutoShapeConstant.Index_60 => "upDownArrowCallout", 
			AutoShapeConstant.Index_61 => "quadArrowCallout", 
			AutoShapeConstant.Index_62 => "bentArrow", 
			AutoShapeConstant.Index_63 => "uturnArrow", 
			AutoShapeConstant.Index_64 => "circularArrow", 
			AutoShapeConstant.Index_65 => "leftCircularArrow", 
			AutoShapeConstant.Index_66 => "leftRightCircularArrow", 
			AutoShapeConstant.Index_67 => "curvedRightArrow", 
			AutoShapeConstant.Index_68 => "curvedLeftArrow", 
			AutoShapeConstant.Index_69 => "curvedUpArrow", 
			AutoShapeConstant.Index_70 => "curvedDownArrow", 
			AutoShapeConstant.Index_71 => "swooshArrow", 
			AutoShapeConstant.Index_72 => "cube", 
			AutoShapeConstant.Index_73 => "can", 
			AutoShapeConstant.Index_74 => "lightningBolt", 
			AutoShapeConstant.Index_75 => "heart", 
			AutoShapeConstant.Index_76 => "sun", 
			AutoShapeConstant.Index_77 => "moon", 
			AutoShapeConstant.Index_78 => "smileyFace", 
			AutoShapeConstant.Index_79 => "irregularSeal1", 
			AutoShapeConstant.Index_80 => "irregularSeal2", 
			AutoShapeConstant.Index_81 => "foldedCorner", 
			AutoShapeConstant.Index_82 => "bevel", 
			AutoShapeConstant.Index_83 => "frame", 
			AutoShapeConstant.Index_84 => "halfFrame", 
			AutoShapeConstant.Index_85 => "corner", 
			AutoShapeConstant.Index_86 => "diagStripe", 
			AutoShapeConstant.Index_87 => "chord", 
			AutoShapeConstant.Index_88 => "arc", 
			AutoShapeConstant.Index_89 => "leftBracket", 
			AutoShapeConstant.Index_90 => "rightBracket", 
			AutoShapeConstant.Index_91 => "leftBrace", 
			AutoShapeConstant.Index_92 => "rightBrace", 
			AutoShapeConstant.Index_93 => "bracketPair", 
			AutoShapeConstant.Index_94 => "bracePair", 
			AutoShapeConstant.Index_95 => "straightConnector1", 
			AutoShapeConstant.Index_96 => "bentConnector2", 
			AutoShapeConstant.Index_97 => "bentConnector3", 
			AutoShapeConstant.Index_98 => "bentConnector4", 
			AutoShapeConstant.Index_99 => "bentConnector5", 
			AutoShapeConstant.Index_100 => "curvedConnector2", 
			AutoShapeConstant.Index_101 => "curvedConnector3", 
			AutoShapeConstant.Index_102 => "curvedConnector4", 
			AutoShapeConstant.Index_103 => "curvedConnector5", 
			AutoShapeConstant.Index_104 => "callout1", 
			AutoShapeConstant.Index_105 => "callout2", 
			AutoShapeConstant.Index_106 => "callout3", 
			AutoShapeConstant.Index_107 => "accentCallout1", 
			AutoShapeConstant.Index_108 => "accentCallout2", 
			AutoShapeConstant.Index_109 => "accentCallout3", 
			AutoShapeConstant.Index_110 => "borderCallout1", 
			AutoShapeConstant.Index_111 => "borderCallout2", 
			AutoShapeConstant.Index_112 => "borderCallout3", 
			AutoShapeConstant.Index_113 => "accentBorderCallout1", 
			AutoShapeConstant.Index_114 => "accentBorderCallout2", 
			AutoShapeConstant.Index_115 => "accentBorderCallout3", 
			AutoShapeConstant.Index_116 => "wedgeRectCallout", 
			AutoShapeConstant.Index_117 => "wedgeRoundRectCallout", 
			AutoShapeConstant.Index_118 => "wedgeEllipseCallout", 
			AutoShapeConstant.Index_119 => "cloudCallout", 
			AutoShapeConstant.Index_120 => "cloud", 
			AutoShapeConstant.Index_121 => "ribbon", 
			AutoShapeConstant.Index_122 => "ribbon2", 
			AutoShapeConstant.Index_123 => "ellipseRibbon", 
			AutoShapeConstant.Index_124 => "ellipseRibbon2", 
			AutoShapeConstant.Index_125 => "leftRightRibbon", 
			AutoShapeConstant.Index_126 => "verticalScroll", 
			AutoShapeConstant.Index_127 => "horizontalScroll", 
			AutoShapeConstant.Index_128 => "wave", 
			AutoShapeConstant.Index_129 => "doubleWave", 
			AutoShapeConstant.Index_130 => "plus", 
			AutoShapeConstant.Index_131 => "flowChartProcess", 
			AutoShapeConstant.Index_132 => "flowChartDecision", 
			AutoShapeConstant.Index_133 => "flowChartInputOutput", 
			AutoShapeConstant.Index_134 => "flowChartPredefinedProcess", 
			AutoShapeConstant.Index_135 => "flowChartInternalStorage", 
			AutoShapeConstant.Index_136 => "flowChartDocument", 
			AutoShapeConstant.Index_137 => "flowChartMultidocument", 
			AutoShapeConstant.Index_138 => "flowChartTerminator", 
			AutoShapeConstant.Index_139 => "flowChartPreparation", 
			AutoShapeConstant.Index_140 => "flowChartManualInput", 
			AutoShapeConstant.Index_141 => "flowChartManualOperation", 
			AutoShapeConstant.Index_142 => "flowChartConnector", 
			AutoShapeConstant.Index_143 => "flowChartPunchedCard", 
			AutoShapeConstant.Index_144 => "flowChartPunchedTape", 
			AutoShapeConstant.Index_145 => "flowChartSummingJunction", 
			AutoShapeConstant.Index_146 => "flowChartOr", 
			AutoShapeConstant.Index_147 => "flowChartCollate", 
			AutoShapeConstant.Index_148 => "flowChartSort", 
			AutoShapeConstant.Index_149 => "flowChartExtract", 
			AutoShapeConstant.Index_150 => "flowChartMerge", 
			AutoShapeConstant.Index_151 => "flowChartOfflineStorage", 
			AutoShapeConstant.Index_152 => "flowChartOnlineStorage", 
			AutoShapeConstant.Index_153 => "flowChartMagneticTape", 
			AutoShapeConstant.Index_154 => "flowChartMagneticDisk", 
			AutoShapeConstant.Index_155 => "flowChartMagneticDrum", 
			AutoShapeConstant.Index_156 => "flowChartDisplay", 
			AutoShapeConstant.Index_157 => "flowChartDelay", 
			AutoShapeConstant.Index_158 => "flowChartAlternateProcess", 
			AutoShapeConstant.Index_159 => "flowChartOffpageConnector", 
			AutoShapeConstant.Index_160 => "actionButtonBlank", 
			AutoShapeConstant.Index_161 => "actionButtonHome", 
			AutoShapeConstant.Index_162 => "actionButtonHelp", 
			AutoShapeConstant.Index_163 => "actionButtonInformation", 
			AutoShapeConstant.Index_164 => "actionButtonForwardNext", 
			AutoShapeConstant.Index_165 => "actionButtonBackPrevious", 
			AutoShapeConstant.Index_166 => "actionButtonEnd", 
			AutoShapeConstant.Index_167 => "actionButtonBeginning", 
			AutoShapeConstant.Index_168 => "actionButtonReturn", 
			AutoShapeConstant.Index_169 => "actionButtonDocument", 
			AutoShapeConstant.Index_170 => "actionButtonSound", 
			AutoShapeConstant.Index_171 => "actionButtonMovie", 
			AutoShapeConstant.Index_172 => "gear6", 
			AutoShapeConstant.Index_173 => "gear9", 
			AutoShapeConstant.Index_174 => "funnel", 
			AutoShapeConstant.Index_175 => "mathPlus", 
			AutoShapeConstant.Index_176 => "mathMinus", 
			AutoShapeConstant.Index_177 => "mathMultiply", 
			AutoShapeConstant.Index_178 => "mathDivide", 
			AutoShapeConstant.Index_179 => "mathEqual", 
			AutoShapeConstant.Index_180 => "mathNotEqual", 
			AutoShapeConstant.Index_181 => "cornerTabs", 
			AutoShapeConstant.Index_182 => "squareTabs", 
			AutoShapeConstant.Index_183 => "plaqueTabs", 
			AutoShapeConstant.Index_184 => "chartX", 
			AutoShapeConstant.Index_185 => "chartStar", 
			AutoShapeConstant.Index_186 => "chartPlus", 
			_ => null, 
		};
	}

	internal AutoShapeType GetAutoShapeType(string shapeTypeID)
	{
		CreateAutoShapeDictionary();
		string[] array = new string[shapeTypes.Count];
		string[] array2 = new string[shapeTypes.Count];
		shapeTypes.Keys.CopyTo(array, 0);
		shapeTypes.Values.CopyTo(array2, 0);
		int num = Array.IndexOf(array2, shapeTypeID);
		if (num == -1)
		{
			return AutoShapeType.Unknown;
		}
		return (AutoShapeType)Enum.Parse(typeof(AutoShapeType), array[num], ignoreCase: true);
	}

	internal string GetAutoShapeTypeIndex(AutoShapeType autoShapeType)
	{
		CreateAutoShapeDictionary();
		return shapeTypes[autoShapeType.ToString()];
	}

	internal string GetShapeTypeIDorAttributeToCheck(AutoShapeType autoShapeType)
	{
		string autoShapeTypeIndex = GetAutoShapeTypeIndex(autoShapeType);
		switch (autoShapeTypeIndex)
		{
		case "Cloud":
		case "Chord":
		case "Frame":
		case "Minus":
		case "Equal":
		case "Round Diagonal Corner Rectangle":
		case "Snip Same Side Corner Rectangle":
		case "Multiply":
		case "Teardrop":
		case "Division":
		case "Heptagon":
		case "Decagon":
		case "L-Shape":
		case "Dodecagon":
		case "Not Equal":
		case "6-Point Star":
		case "7-Point Star":
		case "10-Point Star":
		case "12-Point Star":
		case "Snip Single Corner Rectangle":
		case "Snip Diagonal Corner Rectangle":
		case "Snip and Round Single Corner Rectangle":
		case "Round Single Corner Rectangle":
		case "Round Same Side Corner Rectangle":
		case "Pie":
		case "Half Frame":
		case "Diagonal Stripe":
		case "Plus":
			return "id=\"" + autoShapeTypeIndex + "\"";
		default:
			return "id=\"_x0000_t" + autoShapeTypeIndex + "\"";
		}
	}

	private void CreateAutoShapeDictionary()
	{
		if (shapeTypes.Count == 0)
		{
			shapeTypes.Add("Unknown", "-1");
			shapeTypes.Add("Rectangle", "1");
			shapeTypes.Add("RoundedRectangle ", "2");
			shapeTypes.Add("SnipSingleCornerRectangle", "Snip Single Corner Rectangle");
			shapeTypes.Add("SnipSameSideCornerRectangle", "Snip Same Side Corner Rectangle");
			shapeTypes.Add("SnipDiagonalCornerRectangle", "Snip Diagonal Corner Rectangle");
			shapeTypes.Add("SnipAndRoundSingleCornerRectangle", "Snip and Round Single Corner Rectangle");
			shapeTypes.Add("RoundSingleCornerRectangle", "Round Single Corner Rectangle");
			shapeTypes.Add("RoundSameSideCornerRectangle", "Round Same Side Corner Rectangle");
			shapeTypes.Add("RoundDiagonalCornerRectangle", "Round Diagonal Corner Rectangle");
			shapeTypes.Add("Oval", "3");
			shapeTypes.Add("IsoscelesTriangle", "5");
			shapeTypes.Add("RightTriangle", "6");
			shapeTypes.Add("Parallelogram", "7");
			shapeTypes.Add("Trapezoid", "8");
			shapeTypes.Add("Diamond", "4");
			shapeTypes.Add("RegularPentagon", "56");
			shapeTypes.Add("Hexagon", "9");
			shapeTypes.Add("Heptagon", "Heptagon");
			shapeTypes.Add("Octagon", "10");
			shapeTypes.Add("Decagon", "Decagon");
			shapeTypes.Add("Dodecagon", "Dodecagon");
			shapeTypes.Add("Pie", "Pie");
			shapeTypes.Add("Chord", "Chord");
			shapeTypes.Add("Teardrop", "Teardrop");
			shapeTypes.Add("Frame", "Frame");
			shapeTypes.Add("HalfFrame", "Half Frame");
			shapeTypes.Add("L_Shape", "L-Shape");
			shapeTypes.Add("DiagonalStripe", "Diagonal Stripe");
			shapeTypes.Add("Cross", "11");
			shapeTypes.Add("Plaque", "21");
			shapeTypes.Add("Can", "22");
			shapeTypes.Add("Cube", "16");
			shapeTypes.Add("Bevel", "84");
			shapeTypes.Add("Donut", "23");
			shapeTypes.Add("NoSymbol", "57");
			shapeTypes.Add("BlockArc", "95");
			shapeTypes.Add("FoldedCorner", "65");
			shapeTypes.Add("SmileyFace", "96");
			shapeTypes.Add("Heart", "74");
			shapeTypes.Add("LightningBolt", "73");
			shapeTypes.Add("Sun", "183");
			shapeTypes.Add("Moon", "184");
			shapeTypes.Add("Cloud", "Cloud");
			shapeTypes.Add("Arc", "19");
			shapeTypes.Add("DoubleBracket", "185");
			shapeTypes.Add("DoubleBrace", "186");
			shapeTypes.Add("LeftBracket", "85");
			shapeTypes.Add("RightBracket", "86");
			shapeTypes.Add("LeftBrace", "87");
			shapeTypes.Add("RightBrace", "88");
			shapeTypes.Add("RightArrow", "13");
			shapeTypes.Add("LeftArrow", "66");
			shapeTypes.Add("UpArrow", "68");
			shapeTypes.Add("DownArrow", "67");
			shapeTypes.Add("LeftRightArrow", "69");
			shapeTypes.Add("UpDownArrow", "70");
			shapeTypes.Add("QuadArrow", "76");
			shapeTypes.Add("LeftRightUpArrow", "182");
			shapeTypes.Add("BentArrow", "91");
			shapeTypes.Add("UTurnArrow", "101");
			shapeTypes.Add("LeftUpArrow", "89");
			shapeTypes.Add("BentUpArrow", "90");
			shapeTypes.Add("CurvedRightArrow", "102");
			shapeTypes.Add("CurvedLeftArrow", "103");
			shapeTypes.Add("CurvedUpArrow", "104");
			shapeTypes.Add("CurvedDownArrow", "105");
			shapeTypes.Add("StripedRightArrow", "93");
			shapeTypes.Add("NotchedRightArrow", "94");
			shapeTypes.Add("Pentagon", "15");
			shapeTypes.Add("Chevron", "55");
			shapeTypes.Add("RightArrowCallout", "78");
			shapeTypes.Add("DownArrowCallout", "80");
			shapeTypes.Add("LeftArrowCallout", "77");
			shapeTypes.Add("UpArrowCallout", "79");
			shapeTypes.Add("LeftRightArrowCallout", "81");
			shapeTypes.Add("UpDownArrowCallout", "82");
			shapeTypes.Add("QuadArrowCallout", "83");
			shapeTypes.Add("CircularArrow", "99");
			shapeTypes.Add("MathPlus", "Plus");
			shapeTypes.Add("MathMinus", "Minus");
			shapeTypes.Add("MathMultiply", "Multiply");
			shapeTypes.Add("MathDivision", "Division");
			shapeTypes.Add("MathEqual", "Equal");
			shapeTypes.Add("MathNotEqual", "Not Equal");
			shapeTypes.Add("FlowChartProcess", "109");
			shapeTypes.Add("FlowChartAlternateProcess", "176");
			shapeTypes.Add("FlowChartDecision", "110");
			shapeTypes.Add("FlowChartData", "111");
			shapeTypes.Add("FlowChartPredefinedProcess", "112");
			shapeTypes.Add("FlowChartInternalStorage", "113");
			shapeTypes.Add("FlowChartDocument", "114");
			shapeTypes.Add("FlowChartMultiDocument", "115");
			shapeTypes.Add("FlowChartTerminator", "116");
			shapeTypes.Add("FlowChartPreparation", "117");
			shapeTypes.Add("FlowChartManualInput", "118");
			shapeTypes.Add("FlowChartManualOperation", "119");
			shapeTypes.Add("FlowChartConnector", "120");
			shapeTypes.Add("FlowChartOffPageConnector", "177");
			shapeTypes.Add("FlowChartCard", "121");
			shapeTypes.Add("FlowChartPunchedTape", "122");
			shapeTypes.Add("FlowChartSummingJunction", "123");
			shapeTypes.Add("FlowChartOr", "124");
			shapeTypes.Add("FlowChartCollate", "125");
			shapeTypes.Add("FlowChartSort", "126");
			shapeTypes.Add("FlowChartExtract", "127");
			shapeTypes.Add("FlowChartMerge", "128");
			shapeTypes.Add("FlowChartStoredData", "130");
			shapeTypes.Add("FlowChartDelay", "135");
			shapeTypes.Add("FlowChartSequentialAccessStorage", "131");
			shapeTypes.Add("FlowChartMagneticDisk", "132");
			shapeTypes.Add("FlowChartDirectAccessStorage", "133");
			shapeTypes.Add("FlowChartDisplay", "134");
			shapeTypes.Add("Explosion1", "71");
			shapeTypes.Add("Explosion2", "72");
			shapeTypes.Add("Star4Point", "187");
			shapeTypes.Add("Star5Point", "12");
			shapeTypes.Add("Star6Point", "6-Point Star");
			shapeTypes.Add("Star7Point", "7-Point Star");
			shapeTypes.Add("Star8Point", "58");
			shapeTypes.Add("Star10Point", "10-Point Star");
			shapeTypes.Add("Star12Point", "12-Point Star");
			shapeTypes.Add("Star16Point", "59");
			shapeTypes.Add("Star24Point", "92");
			shapeTypes.Add("Star32Point", "60");
			shapeTypes.Add("UpRibbon", "54");
			shapeTypes.Add("DownRibbon", "53");
			shapeTypes.Add("CurvedUpRibbon", "108");
			shapeTypes.Add("CurvedDownRibbon", "107");
			shapeTypes.Add("VerticalScroll", "97");
			shapeTypes.Add("HorizontalScroll", "98");
			shapeTypes.Add("Wave", "64");
			shapeTypes.Add("DoubleWave", "188");
			shapeTypes.Add("RectangularCallout", "61");
			shapeTypes.Add("RoundedRectangularCallout", "17");
			shapeTypes.Add("OvalCallout", "63");
			shapeTypes.Add("CloudCallout", "106");
			shapeTypes.Add("LineCallout1", "47");
			shapeTypes.Add("LineCallout2", "48");
			shapeTypes.Add("LineCallout3", "49");
			shapeTypes.Add("LineCallout1AccentBar", "44");
			shapeTypes.Add("LineCallout2AccentBar", "45");
			shapeTypes.Add("LineCallout3AccentBar", "46");
			shapeTypes.Add("LineCallout1NoBorder", "41");
			shapeTypes.Add("LineCallout2NoBorder", "42");
			shapeTypes.Add("LineCallout3NoBorder", "43");
			shapeTypes.Add("LineCallout1BorderAndAccentBar", "50");
			shapeTypes.Add("LineCallout2BorderAndAccentBar", "51");
			shapeTypes.Add("LineCallout3BorderAndAccentBar", "52");
			shapeTypes.Add("Line", "Line");
			shapeTypes.Add("StraightConnector", "32");
			shapeTypes.Add("CurvedConnector", "38");
			shapeTypes.Add("ElbowConnector", "34");
			shapeTypes.Add("CurvedConnector2", "37");
			shapeTypes.Add("CurvedConnector4", "39");
			shapeTypes.Add("CurvedConnector5", "40");
			shapeTypes.Add("BentConnector2", "33");
			shapeTypes.Add("BentConnector4", "35");
			shapeTypes.Add("BentConnector5", "36");
		}
	}

	internal void Close()
	{
		if (dictionary != null)
		{
			dictionary.Clear();
			dictionary = null;
		}
		if (shapeTypes != null)
		{
			shapeTypes.Clear();
			shapeTypes = null;
		}
	}
}
