namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

internal enum FormulaToken
{
	None = 0,
	tAdd = 3,
	tSub = 4,
	tMul = 5,
	tDiv = 6,
	tPower = 7,
	tConcat = 8,
	tLessThan = 9,
	tLessEqual = 10,
	tEqual = 11,
	tGreaterEqual = 12,
	tGreater = 13,
	tNotEqual = 14,
	tCellRangeIntersection = 15,
	tCellRangeList = 16,
	tCellRange = 17,
	tUnaryPlus = 18,
	tUnaryMinus = 19,
	tPercent = 20,
	tParentheses = 21,
	tFunction1 = 33,
	tFunction2 = 65,
	tFunction3 = 97,
	tFunctionVar1 = 34,
	tFunctionVar2 = 66,
	tFunctionVar3 = 98,
	tFunctionCE1 = 56,
	tFunctionCE2 = 88,
	tFunctionCE3 = 120,
	tMissingArgument = 22,
	tStringConstant = 23,
	tError = 28,
	tBoolean = 29,
	tInteger = 30,
	tNumber = 31,
	tExp = 1,
	tTbl = 2,
	tExtended = 24,
	tAttr = 25,
	tSheet = 26,
	tEndSheet = 27,
	tArray1 = 32,
	tArray2 = 64,
	tArray3 = 96,
	tName1 = 35,
	tName2 = 67,
	tName3 = 99,
	tRef1 = 36,
	tRef2 = 68,
	tRef3 = 100,
	tArea1 = 37,
	tArea2 = 69,
	tArea3 = 101,
	tMemArea1 = 38,
	tMemArea2 = 70,
	tMemArea3 = 102,
	tMemErr1 = 39,
	tMemErr2 = 71,
	tMemErr3 = 103,
	tMemNoMem1 = 40,
	tMemNoMem2 = 72,
	tMemNoMem3 = 104,
	tMemFunc1 = 41,
	tMemFunc2 = 73,
	tMemFunc3 = 105,
	tRefErr1 = 42,
	tRefErr2 = 74,
	tRefErr3 = 106,
	tAreaErr1 = 43,
	tAreaErr2 = 75,
	tAreaErr3 = 107,
	tRefN1 = 44,
	tRefN2 = 76,
	tRefN3 = 108,
	tAreaN1 = 45,
	tAreaN2 = 77,
	tAreaN3 = 109,
	tMemAreaN1 = 46,
	tMemAreaN2 = 78,
	tMemAreaN3 = 110,
	tMemNoMemN1 = 47,
	tMemNoMemN2 = 79,
	tMemNoMemN3 = 111,
	tNameX1 = 57,
	tNameX2 = 89,
	tNameX3 = 121,
	tRef3d1 = 58,
	tRef3d2 = 90,
	tRef3d3 = 122,
	tArea3d1 = 59,
	tArea3d2 = 91,
	tArea3d3 = 123,
	tRefErr3d1 = 60,
	tRefErr3d2 = 92,
	tRefErr3d3 = 124,
	tAreaErr3d1 = 61,
	tAreaErr3d2 = 93,
	tAreaErr3d3 = 125,
	EndOfFormula = 4097,
	CloseParenthesis = 4098,
	Comma = 4099,
	OpenBracket = 4100,
	CloseBracket = 4101,
	ValueTrue = 4102,
	ValueFalse = 4103,
	Space = 4104,
	Identifier = 4105,
	DDELink = 4106,
	Identifier3D = 4107
}
