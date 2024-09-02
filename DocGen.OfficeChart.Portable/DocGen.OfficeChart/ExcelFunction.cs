using System;
using System.ComponentModel;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart;

[CLSCompliant(false)]
internal enum ExcelFunction
{
	NONE = 65535,
	CustomFunction = 255,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	ABS = 24,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	ACOS = 99,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	ACOSH = 233,
	[ReferenceIndex(1)]
	ADDRESS = 219,
	[ReferenceIndex(2)]
	AND = 36,
	[DefaultValue(1)]
	[ReferenceIndex(1)]
	AREAS = 75,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	ASIN = 98,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	ASINH = 232,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	ATAN = 18,
	[DefaultValue(2)]
	[ReferenceIndex(2)]
	ATAN2 = 97,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	ATANH = 234,
	[ReferenceIndex(1)]
	[ReferenceIndex(typeof(ArrayPtg), 3)]
	AVEDEV = 269,
	[ReferenceIndex(1)]
	AVERAGE = 5,
	[ReferenceIndex(1)]
	AVERAGEA = 361,
	HEX2BIN = 400,
	HEX2DEC = 401,
	HEX2OCT = 402,
	COUNTIFS = 403,
	BIN2DEC = 404,
	BIN2HEX = 405,
	BIN2OCT = 406,
	DEC2BIN = 407,
	DEC2HEX = 408,
	DEC2OCT = 409,
	OCT2BIN = 410,
	OCT2DEC = 411,
	OCT2HEX = 412,
	ODDFPRICE = 413,
	ODDFYIELD = 414,
	ODDLPRICE = 415,
	ODDLYIELD = 416,
	ISODD = 417,
	ISEVEN = 418,
	LCM = 419,
	GCD = 420,
	SUMIFS = 421,
	AVERAGEIF = 422,
	AVERAGEIFS = 423,
	CONVERT = 424,
	COMPLEX = 425,
	COUPDAYBS = 426,
	COUPDAYS = 427,
	COUPDAYSNC = 428,
	COUPNCD = 429,
	COUPNUM = 430,
	COUPPCD = 431,
	DELTA = 432,
	DISC = 433,
	DOLLARDE = 434,
	DOLLARFR = 435,
	DURATION = 436,
	EDATE = 437,
	EFFECT = 438,
	EOMONTH = 439,
	ERF = 440,
	ERFC = 441,
	FACTDOUBLE = 442,
	GESTEP = 443,
	IFERROR = 444,
	IMABS = 445,
	IMAGINARY = 446,
	IMARGUMENT = 447,
	IMCONJUGATE = 448,
	IMCOS = 449,
	IMEXP = 450,
	IMLN = 451,
	IMLOG10 = 452,
	IMLOG2 = 453,
	IMREAL = 454,
	IMSIN = 455,
	IMSQRT = 456,
	IMSUB = 457,
	IMSUM = 458,
	IMDIV = 459,
	IMPOWER = 460,
	IMPRODUCT = 461,
	ACCRINT = 462,
	ACCRINTM = 463,
	AGGREGATE = 464,
	AMORDEGRC = 465,
	AMORLINC = 466,
	BAHTTEXT = 467,
	BESSELI = 468,
	BESSELJ = 469,
	BESSELK = 470,
	BESSELY = 471,
	CUBEKPIMEMBER = 472,
	CUBEMEMBER = 473,
	CUBERANKEDMEMBER = 474,
	CUBESET = 475,
	CUBESETCOUNT = 476,
	CUBEMEMBERPROPERTY = 477,
	CUMIPMT = 478,
	CUMPRINC = 479,
	FVSCHEDULE = 480,
	INTRATE = 481,
	LINTEST = 482,
	CUBEVALUE = 483,
	MDURATION = 484,
	MROUND = 485,
	MULTINOMIAL = 486,
	NETWORKDAYS = 487,
	NOMINAL = 488,
	PRICE = 489,
	PRICEDISC = 490,
	PRICEMAT = 491,
	QUOTIENT = 492,
	RANDBETWEEN = 493,
	RECEIVED = 494,
	SERIESSUM = 495,
	SQRTPI = 496,
	TBILLEQ = 497,
	TBILLPRICE = 498,
	TBILLYIELD = 499,
	WEEKNUM = 500,
	WORKDAY = 501,
	XIRR = 502,
	XNPV = 503,
	YEARFRAC = 504,
	YIELD = 505,
	YIELDDISC = 506,
	YIELDMAT = 507,
	WORKDAYINTL = 508,
	BETA_INV = 509,
	BINOM_DIST = 510,
	BINOM_INV = 511,
	CEILING_PRECISE = 512,
	CHISQ_DIST = 513,
	CHISQ_DIST_RT = 514,
	CHISQ_INV = 515,
	CHISQ_INV_RT = 516,
	CHISQ_TEST = 517,
	CONFIDENCE_NORM = 518,
	CONFIDENCE_T = 519,
	COVARIANCE_P = 520,
	COVARIANCE_S = 521,
	ERF_PRECISE = 522,
	ERFC_PRECISE = 523,
	F_DIST = 524,
	F_DIST_RT = 525,
	F_INV = 526,
	F_INV_RT = 527,
	F_TEST = 528,
	FLOOR_PRECISE = 529,
	GAMMA_DIST = 530,
	GAMMA_INV = 531,
	GAMMALN_PRECISE = 532,
	HYPGEOM_DIST = 533,
	LOGNORM_DIST = 534,
	LOGNORM_INV = 535,
	MODE_MULT = 536,
	MODE_SNGL = 537,
	NEGBINOM_DIST = 538,
	NETWORKDAYS_INTL = 539,
	NORM_DIST = 540,
	NORM_INV = 541,
	NORM_S_DIST = 542,
	PERCENTILE_EXC = 543,
	PERCENTILE_INC = 544,
	PERCENTRANK_EXC = 545,
	PERCENTRANK_INC = 546,
	POISSON_DIST = 547,
	QUARTILE_EXC = 548,
	QUARTILE_INC = 549,
	RANK_AVG = 550,
	RANK_EQ = 551,
	STDEV_P = 552,
	STDEV_S = 553,
	T_DIST = 554,
	T_DIST_2T = 555,
	T_DIST_RT = 556,
	T_INV = 557,
	T_INV_2T = 558,
	T_TEST = 559,
	VAR_P = 560,
	VAR_S = 561,
	WEIBULL_DIST = 562,
	WORKDAY_INTL = 563,
	Z_TEST = 564,
	BETA_DIST = 565,
	EUROCONVERT = 566,
	PHONETIC = 567,
	REGISTER_ID = 568,
	SQL_REQUEST = 569,
	JIS = 570,
	EXPON_DIST = 571,
	DAYS = 572,
	ISOWEEKNUM = 573,
	BITAND = 574,
	BITLSHIFT = 575,
	BITOR = 576,
	BITRSHIFT = 577,
	BITXOR = 578,
	IMCOSH = 579,
	IMCOT = 580,
	IMCSC = 581,
	IMCSCH = 582,
	IMSEC = 583,
	IMSECH = 584,
	IMSINH = 585,
	IMTAN = 586,
	PDURATION = 587,
	RRI = 588,
	ISFORMULA = 589,
	SHEET = 590,
	SHEETS = 591,
	IFNA = 592,
	XOR = 593,
	FORMULATEXT = 594,
	ACOT = 595,
	ACOTH = 596,
	ARABIC = 597,
	BASE = 598,
	CEILING_MATH = 599,
	COMBINA = 600,
	COT = 601,
	COTH = 602,
	CSC = 603,
	CSCH = 604,
	DECIMAL = 605,
	FLOOR_MATH = 606,
	ISO_CEILING = 607,
	MUNIT = 608,
	SEC = 609,
	SECH = 610,
	BINOM_DIST_RANGE = 611,
	GAMMA = 612,
	GAUSS = 613,
	PERMUTATIONA = 614,
	PHI = 615,
	SKEW_P = 616,
	NUMBERVALUE = 617,
	UNICHAR = 618,
	UNICODE = 619,
	ENCODEURL = 620,
	FILTERXML = 621,
	WEBSERVICE = 622,
	[ReferenceIndex(2)]
	BETADIST = 270,
	[ReferenceIndex(2)]
	BETAINV = 272,
	[DefaultValue(4)]
	[ReferenceIndex(2)]
	BINOMDIST = 273,
	[DefaultValue(2)]
	[ReferenceIndex(2)]
	CEILING = 288,
	[ReferenceIndex(new int[] { 2, 1 })]
	CELL = 125,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	CHAR = 111,
	[DefaultValue(2)]
	[ReferenceIndex(2)]
	CHIDIST = 274,
	[DefaultValue(2)]
	[ReferenceIndex(2)]
	CHIINV = 275,
	[DefaultValue(2)]
	[ReferenceIndex(3)]
	CHITEST = 306,
	CHOOSE = 100,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	CLEAN = 162,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	CODE = 121,
	[ReferenceIndex(1)]
	COLUMN = 9,
	[DefaultValue(1)]
	[ReferenceIndex(1)]
	COLUMNS = 77,
	[DefaultValue(2)]
	[ReferenceIndex(2)]
	COMBIN = 276,
	[ReferenceIndex(2)]
	CONCATENATE = 336,
	[DefaultValue(3)]
	[ReferenceIndex(2)]
	CONFIDENCE = 277,
	[DefaultValue(2)]
	[ReferenceIndex(3)]
	CORREL = 307,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	COS = 16,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	COSH = 230,
	[ReferenceIndex(1)]
	COUNT = 0,
	[ReferenceIndex(1)]
	COUNTA = 169,
	[DefaultValue(1)]
	[ReferenceIndex(1)]
	COUNTBLANK = 347,
	[DefaultValue(2)]
	[ReferenceIndex(new int[] { 1, 2 })]
	COUNTIF = 346,
	[DefaultValue(2)]
	[ReferenceIndex(3)]
	COVAR = 308,
	[DefaultValue(3)]
	[ReferenceIndex(2)]
	CRITBINOM = 278,
	[DefaultValue(3)]
	[ReferenceIndex(2)]
	DATE = 65,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	DATEVALUE = 140,
	[DefaultValue(3)]
	[ReferenceIndex(1)]
	DAVERAGE = 42,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	DAY = 67,
	[ReferenceIndex(2)]
	DAYS360 = 220,
	[ReferenceIndex(2)]
	DB = 247,
	[DefaultValue(3)]
	[ReferenceIndex(1)]
	DCOUNT = 40,
	[DefaultValue(3)]
	[ReferenceIndex(1)]
	DCOUNTA = 199,
	[ReferenceIndex(2)]
	DDB = 144,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	DEGREES = 343,
	[ReferenceIndex(1)]
	DEVSQ = 318,
	[DefaultValue(3)]
	[ReferenceIndex(1)]
	DMAX = 44,
	[ReferenceIndex(1)]
	[DefaultValue(3)]
	DMIN = 43,
	[ReferenceIndex(2)]
	DOLLAR = 13,
	[DefaultValue(3)]
	[ReferenceIndex(1)]
	DPRODUCT = 189,
	[DefaultValue(3)]
	[ReferenceIndex(1)]
	DSTDEV = 45,
	[DefaultValue(3)]
	[ReferenceIndex(1)]
	DSTDEVP = 195,
	[DefaultValue(3)]
	[ReferenceIndex(1)]
	DSUM = 41,
	[DefaultValue(3)]
	[ReferenceIndex(1)]
	DVAR = 47,
	[DefaultValue(3)]
	[ReferenceIndex(1)]
	DVARP = 196,
	[ReferenceIndex(1)]
	ERROR = 84,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	ERRORTYPE = 261,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	EVEN = 279,
	[DefaultValue(2)]
	[ReferenceIndex(2)]
	EXACT = 117,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	EXP = 21,
	[DefaultValue(3)]
	[ReferenceIndex(2)]
	EXPONDIST = 280,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	FACT = 184,
	[DefaultValue(0)]
	[ReferenceIndex(1)]
	FALSE = 35,
	[DefaultValue(3)]
	[ReferenceIndex(2)]
	FDIST = 281,
	[ReferenceIndex(2)]
	FIND = 124,
	[ReferenceIndex(2)]
	FINDB = 205,
	[DefaultValue(3)]
	[ReferenceIndex(2)]
	FINV = 282,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	FISHER = 283,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	FISHERINV = 284,
	[ReferenceIndex(2)]
	FIXED = 14,
	[DefaultValue(2)]
	[ReferenceIndex(2)]
	FLOOR = 285,
	[DefaultValue(3)]
	[ReferenceIndex(new int[] { 2, 3, 3 })]
	FORECAST = 309,
	[DefaultValue(2)]
	[ReferenceIndex(1)]
	FREQUENCY = 252,
	[DefaultValue(2)]
	[ReferenceIndex(3)]
	FTEST = 310,
	[ReferenceIndex(2)]
	FV = 57,
	[DefaultValue(4)]
	[ReferenceIndex(2)]
	GAMMADIST = 286,
	[DefaultValue(3)]
	[ReferenceIndex(2)]
	GAMMAINV = 287,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	GAMMALN = 271,
	[ReferenceIndex(1)]
	GEOMEAN = 319,
	[ReferenceIndex(1)]
	GETPIVOTDATA = 358,
	[ReferenceIndex(1)]
	GROWTH = 52,
	[ReferenceIndex(1)]
	HARMEAN = 320,
	[ReferenceIndex(new int[] { 2, 1 })]
	HLOOKUP = 101,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	HOUR = 71,
	[ReferenceIndex(2)]
	HYPERLINK = 359,
	[DefaultValue(4)]
	[ReferenceIndex(2)]
	HYPGEOMDIST = 289,
	[ReferenceIndex(2)]
	IF = 1,
	[ReferenceIndex(1)]
	[ReferenceIndex(typeof(ArrayPtg), 3)]
	INDEX = 29,
	[ReferenceIndex(2)]
	INDIRECT = 148,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	INFO = 244,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	INT = 25,
	[DefaultValue(2)]
	[ReferenceIndex(3)]
	INTERCEPT = 311,
	[ReferenceIndex(2)]
	IPMT = 167,
	[ReferenceIndex(1)]
	[ReferenceIndex(typeof(ArrayPtg), 3)]
	IRR = 62,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	ISBLANK = 129,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	ISERR = 126,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	ISERROR = 3,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	ISLOGICAL = 198,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	ISNA = 2,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	ISNONTEXT = 190,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	ISNUMBER = 128,
	[DefaultValue(4)]
	[ReferenceIndex(2)]
	ISPMT = 350,
	[DefaultValue(1)]
	[ReferenceIndex(1)]
	ISREF = 105,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	ISTEXT = 127,
	[ReferenceIndex(1)]
	[ReferenceIndex(typeof(ArrayPtg), 3)]
	KURT = 322,
	[ReferenceIndex(1)]
	[DefaultValue(2)]
	[ReferenceIndex(typeof(ArrayPtg), 3)]
	LARGE = 325,
	[ReferenceIndex(2)]
	LEFT = 115,
	[ReferenceIndex(2)]
	LEFTB = 208,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	LEN = 32,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	LENB = 211,
	[ReferenceIndex(1)]
	LINEST = 49,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	LN = 22,
	[ReferenceIndex(2)]
	LOG = 109,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	LOG10 = 23,
	[ReferenceIndex(1)]
	[ReferenceIndex(typeof(ArrayPtg), 3)]
	LOGEST = 51,
	[DefaultValue(3)]
	[ReferenceIndex(2)]
	LOGINV = 291,
	[DefaultValue(3)]
	[ReferenceIndex(2)]
	LOGNORMDIST = 290,
	[ReferenceIndex(new int[] { 2, 1, 1 })]
	[ReferenceIndex(typeof(ArrayPtg), 3)]
	LOOKUP = 28,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	LOWER = 112,
	[ReferenceIndex(new int[] { 2, 1 })]
	[ReferenceIndex(typeof(ArrayPtg), 3)]
	MATCH = 64,
	[ReferenceIndex(1)]
	[ReferenceIndex(typeof(ArrayPtg), 3)]
	MAX = 7,
	[ReferenceIndex(1)]
	[ReferenceIndex(typeof(ArrayPtg), 3)]
	MAXA = 362,
	[DefaultValue(1)]
	[ReferenceIndex(3)]
	MDETERM = 163,
	[ReferenceIndex(1)]
	[ReferenceIndex(typeof(ArrayPtg), 3)]
	MEDIAN = 227,
	[DefaultValue(3)]
	[ReferenceIndex(2)]
	MID = 31,
	[DefaultValue(3)]
	[ReferenceIndex(2)]
	MIDB = 210,
	[ReferenceIndex(1)]
	[ReferenceIndex(typeof(ArrayPtg), 3)]
	MIN = 6,
	[ReferenceIndex(1)]
	[ReferenceIndex(typeof(ArrayPtg), 3)]
	MINA = 363,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	MINUTE = 72,
	[DefaultValue(1)]
	[ReferenceIndex(3)]
	MINVERSE = 164,
	[DefaultValue(3)]
	[ReferenceIndex(new int[] { 1, 2, 2 })]
	[ReferenceIndex(typeof(ArrayPtg), 3)]
	MIRR = 61,
	[DefaultValue(2)]
	[ReferenceIndex(3)]
	MMULT = 165,
	[DefaultValue(2)]
	[ReferenceIndex(2)]
	MOD = 39,
	[ReferenceIndex(3)]
	MODE = 330,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	MONTH = 68,
	[DefaultValue(1)]
	[ReferenceIndex(1)]
	N = 131,
	[DefaultValue(0)]
	[ReferenceIndex(1)]
	NA = 10,
	[DefaultValue(3)]
	[ReferenceIndex(2)]
	NEGBINOMDIST = 292,
	[DefaultValue(4)]
	[ReferenceIndex(2)]
	NORMDIST = 293,
	[DefaultValue(3)]
	[ReferenceIndex(2)]
	NORMINV = 295,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	NORMSDIST = 294,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	NORMSINV = 296,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	NOT = 38,
	[DefaultValue(0)]
	[ReferenceIndex(1)]
	NOW = 74,
	[ReferenceIndex(2)]
	NPER = 58,
	[ReferenceIndex(typeof(AreaPtg), new int[] { 2, 1 })]
	[ReferenceIndex(2)]
	NPV = 11,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	ODD = 298,
	[ReferenceIndex(1)]
	OFFSET = 78,
	[ReferenceIndex(2)]
	OR = 37,
	[DefaultValue(2)]
	[ReferenceIndex(3)]
	PEARSON = 312,
	[DefaultValue(2)]
	[ReferenceIndex(new int[] { 1, 2 })]
	PERCENTILE = 328,
	[ReferenceIndex(new int[] { 1, 2 })]
	PERCENTRANK = 329,
	[DefaultValue(2)]
	[ReferenceIndex(2)]
	PERMUT = 299,
	[DefaultValue(0)]
	[ReferenceIndex(1)]
	PI = 19,
	[ReferenceIndex(2)]
	PMT = 59,
	[DefaultValue(3)]
	[ReferenceIndex(2)]
	POISSON = 300,
	[DefaultValue(2)]
	[ReferenceIndex(2)]
	POWER = 337,
	[ReferenceIndex(2)]
	PPMT = 168,
	[ReferenceIndex(new int[] { 3, 3, 2 })]
	PROB = 317,
	[ReferenceIndex(1)]
	[ReferenceIndex(typeof(ArrayPtg), 3)]
	PRODUCT = 183,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	PROPER = 114,
	[ReferenceIndex(2)]
	PV = 56,
	[DefaultValue(2)]
	[ReferenceIndex(new int[] { 1, 2 })]
	QUARTILE = 327,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	RADIANS = 342,
	[DefaultValue(0)]
	[ReferenceIndex(1)]
	RAND = 63,
	[ReferenceIndex(new int[] { 2, 1 })]
	RANK = 216,
	[ReferenceIndex(2)]
	RATE = 60,
	[DefaultValue(4)]
	[ReferenceIndex(2)]
	REPLACE = 119,
	[DefaultValue(4)]
	[ReferenceIndex(2)]
	REPLACEB = 207,
	[ReferenceIndex(2)]
	RIGHT = 116,
	[ReferenceIndex(2)]
	RIGHTB = 209,
	[ReferenceIndex(2)]
	ROMAN = 354,
	[DefaultValue(2)]
	[ReferenceIndex(2)]
	ROUND = 27,
	[DefaultValue(2)]
	[ReferenceIndex(2)]
	ROUNDDOWN = 213,
	[DefaultValue(2)]
	[ReferenceIndex(2)]
	ROUNDUP = 212,
	[ReferenceIndex(1)]
	ROW = 8,
	[DefaultValue(1)]
	[ReferenceIndex(1)]
	ROWS = 76,
	[DefaultValue(2)]
	[ReferenceIndex(3)]
	RSQ = 313,
	[ReferenceIndex(2)]
	SEARCH = 82,
	[ReferenceIndex(2)]
	SEARCHB = 206,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	SECOND = 73,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	SIGN = 26,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	SIN = 15,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	SINH = 229,
	[ReferenceIndex(1)]
	SKEW = 323,
	[DefaultValue(3)]
	[ReferenceIndex(2)]
	SLN = 142,
	[DefaultValue(2)]
	[ReferenceIndex(3)]
	SLOPE = 315,
	[DefaultValue(2)]
	[ReferenceIndex(1)]
	SMALL = 326,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	SQRT = 20,
	[DefaultValue(3)]
	[ReferenceIndex(2)]
	STANDARDIZE = 297,
	[ReferenceIndex(1)]
	STDEV = 12,
	[ReferenceIndex(1)]
	STDEVA = 366,
	[ReferenceIndex(1)]
	STDEVP = 193,
	[ReferenceIndex(1)]
	STDEVPA = 364,
	[DefaultValue(2)]
	[ReferenceIndex(3)]
	STEYX = 314,
	[ReferenceIndex(2)]
	SUBSTITUTE = 120,
	[ReferenceIndex(new int[] { 2, 1 })]
	SUBTOTAL = 344,
	[ReferenceIndex(1)]
	SUM = 4,
	[ReferenceIndex(new int[] { 1, 2, 1 })]
	SUMIF = 345,
	[ReferenceIndex(3)]
	SUMPRODUCT = 228,
	[ReferenceIndex(1)]
	SUMSQ = 321,
	[DefaultValue(2)]
	[ReferenceIndex(3)]
	SUMX2MY2 = 304,
	[DefaultValue(2)]
	[ReferenceIndex(3)]
	SUMX2PY2 = 305,
	[DefaultValue(2)]
	[ReferenceIndex(3)]
	SUMXMY2 = 303,
	[DefaultValue(4)]
	[ReferenceIndex(2)]
	SYD = 143,
	[DefaultValue(1)]
	[ReferenceIndex(1)]
	T = 130,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	TAN = 17,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	TANH = 231,
	[DefaultValue(3)]
	[ReferenceIndex(2)]
	TDIST = 301,
	[DefaultValue(2)]
	[ReferenceIndex(2)]
	TEXT = 48,
	[DefaultValue(3)]
	[ReferenceIndex(2)]
	TIME = 66,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	TIMEVALUE = 141,
	[DefaultValue(2)]
	[ReferenceIndex(2)]
	TINV = 332,
	[DefaultValue(0)]
	[ReferenceIndex(1)]
	TODAY = 221,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	[ReferenceIndex(typeof(ArrayPtg), 3)]
	TRANSPOSE = 83,
	[ReferenceIndex(1)]
	[ReferenceIndex(typeof(ArrayPtg), 3)]
	TREND = 50,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	TRIM = 118,
	[DefaultValue(2)]
	[ReferenceIndex(3)]
	TRIMMEAN = 331,
	[DefaultValue(0)]
	[ReferenceIndex(1)]
	TRUE = 34,
	[ReferenceIndex(2)]
	TRUNC = 197,
	[DefaultValue(4)]
	[ReferenceIndex(3)]
	TTEST = 316,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	[ReferenceIndex(typeof(ArrayPtg), 3)]
	TYPE = 86,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	UPPER = 113,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	VALUE = 33,
	[ReferenceIndex(1)]
	VAR = 46,
	[ReferenceIndex(1)]
	VARA = 367,
	[ReferenceIndex(1)]
	VARP = 194,
	[ReferenceIndex(1)]
	VARPA = 365,
	[ReferenceIndex(2)]
	VDB = 222,
	[ReferenceIndex(new int[] { 2, 1 })]
	VLOOKUP = 102,
	[ReferenceIndex(2)]
	WEEKDAY = 70,
	[DefaultValue(4)]
	[ReferenceIndex(2)]
	WEIBULL = 302,
	[DefaultValue(1)]
	[ReferenceIndex(2)]
	YEAR = 69,
	[ReferenceIndex(1)]
	[ReferenceIndex(typeof(ArrayPtg), 3)]
	ZTEST = 324,
	[DefaultValue(0)]
	ABSREF = 79,
	[DefaultValue(0)]
	ACTIVECELL = 94,
	[DefaultValue(1)]
	ADDBAR = 151,
	[DefaultValue(1)]
	ADDCOMMAND = 153,
	[DefaultValue(1)]
	ADDMENU = 152,
	[DefaultValue(1)]
	ADDTOOLBAR = 253,
	[DefaultValue(0)]
	APPTITLE = 262,
	[DefaultValue(1)]
	ARGUMENT = 81,
	[DefaultValue(1)]
	ASC = 214,
	[DefaultValue(1)]
	CALL = 150,
	[DefaultValue(0)]
	CALLER = 89,
	[DefaultValue(0)]
	CANCELKEY = 170,
	[DefaultValue(1)]
	CHECKCOMMAND = 155,
	[DefaultValue(1)]
	CREATEOBJECT = 236,
	[DefaultValue(1)]
	CUSTOMREPEAT = 240,
	[DefaultValue(1)]
	CUSTOMUNDO = 239,
	[DefaultValue(3)]
	DATEDIF = 351,
	[DefaultValue(1)]
	DATESTRING = 352,
	[DefaultValue(1)]
	DBCS = 215,
	[DefaultValue(1)]
	DELETEBAR = 200,
	[DefaultValue(1)]
	DELETECOMMAND = 159,
	[DefaultValue(1)]
	DELETEMENU = 158,
	[DefaultValue(1)]
	DELETETOOLBAR = 254,
	[DefaultValue(1)]
	DEREF = 90,
	[DefaultValue(3)]
	[ReferenceIndex(1)]
	DGET = 235,
	[DefaultValue(1)]
	DIALOGBOX = 161,
	[DefaultValue(1)]
	DIRECTORY = 123,
	[DefaultValue(1)]
	DOCUMENTS = 93,
	[DefaultValue(1)]
	ECHO = 87,
	[DefaultValue(1)]
	ENABLECOMMAND = 154,
	[DefaultValue(1)]
	ENABLETOOL = 265,
	[DefaultValue(1)]
	EVALUATE = 257,
	[DefaultValue(1)]
	EXEC = 110,
	[DefaultValue(1)]
	EXECUTE = 178,
	[DefaultValue(1)]
	FILES = 166,
	[DefaultValue(1)]
	FOPEN = 132,
	[DefaultValue(1)]
	FORMULACONVERT = 241,
	[DefaultValue(1)]
	FPOS = 139,
	[DefaultValue(1)]
	FREAD = 136,
	[DefaultValue(1)]
	FREADLN = 135,
	[DefaultValue(1)]
	FSIZE = 134,
	[DefaultValue(1)]
	FWRITE = 138,
	[DefaultValue(1)]
	FWRITELN = 137,
	[DefaultValue(1)]
	FCLOSE = 133,
	[DefaultValue(1)]
	GETBAR = 182,
	[DefaultValue(1)]
	GETCELL = 185,
	[DefaultValue(1)]
	GETCHARTITEM = 160,
	[DefaultValue(1)]
	GETDEF = 145,
	[DefaultValue(0)]
	GETDOCUMENT = 188,
	[DefaultValue(1)]
	GETFORMULA = 106,
	[DefaultValue(1)]
	GETLINKINFO = 242,
	[DefaultValue(1)]
	GETMOVIE = 335,
	[DefaultValue(1)]
	GETNAME = 107,
	[DefaultValue(1)]
	GETNOTE = 191,
	[DefaultValue(1)]
	GETOBJECT = 246,
	[DefaultValue(1)]
	GETPIVOTFIELD = 340,
	[DefaultValue(1)]
	GETPIVOTITEM = 341,
	[DefaultValue(1)]
	GETPIVOTTABLE = 339,
	[DefaultValue(1)]
	GETTOOL = 259,
	[DefaultValue(1)]
	GETTOOLBAR = 258,
	[DefaultValue(1)]
	GETWINDOW = 187,
	[DefaultValue(1)]
	GETWORKBOOK = 268,
	[DefaultValue(1)]
	GETWORKSPACE = 186,
	[DefaultValue(1)]
	GOTO = 53,
	[DefaultValue(1)]
	GROUP = 245,
	[DefaultValue(1)]
	HALT = 54,
	[DefaultValue(1)]
	HELP = 181,
	[DefaultValue(0)]
	INITIATE = 175,
	[DefaultValue(0)]
	INPUT = 104,
	[DefaultValue(0)]
	LASTERROR = 238,
	[DefaultValue(0)]
	LINKS = 103,
	[DefaultValue(1)]
	MOVIECOMMAND = 334,
	[DefaultValue(1)]
	NAMES = 122,
	[DefaultValue(1)]
	NOTE = 192,
	[DefaultValue(1)]
	NUMBERSTRING = 353,
	[DefaultValue(1)]
	OPENDIALOG = 355,
	[DefaultValue(1)]
	OPTIONSLISTSGET = 349,
	[DefaultValue(0)]
	PAUSE = 248,
	[DefaultValue(1)]
	PIVOTADDDATA = 338,
	[DefaultValue(1)]
	POKE = 177,
	[DefaultValue(1)]
	PRESSTOOL = 266,
	[DefaultValue(1)]
	REFTEXT = 146,
	[DefaultValue(1)]
	REGISTER = 149,
	[DefaultValue(1)]
	REGISTERID = 267,
	[DefaultValue(1)]
	RELREF = 80,
	[DefaultValue(2)]
	RENAMECOMMAND = 156,
	[DefaultValue(2)]
	REPT = 30,
	[DefaultValue(1)]
	REQUEST = 176,
	[DefaultValue(1)]
	RESETTOOLBAR = 256,
	[DefaultValue(0)]
	RESTART = 180,
	[DefaultValue(0)]
	RESULT = 96,
	[DefaultValue(0)]
	RESUME = 251,
	[DefaultValue(0)]
	SAVEDIALOG = 356,
	[DefaultValue(0)]
	SAVETOOLBAR = 264,
	[DefaultValue(0)]
	SCENARIOGET = 348,
	[DefaultValue(0)]
	SELECTION = 95,
	[DefaultValue(1)]
	SERIES = 92,
	[DefaultValue(1)]
	SETNAME = 88,
	[DefaultValue(1)]
	SETVALUE = 108,
	[DefaultValue(0)]
	SHOWBAR = 157,
	[DefaultValue(1)]
	SPELLINGCHECK = 260,
	[DefaultValue(1)]
	STEP = 85,
	[DefaultValue(1)]
	TERMINATE = 179,
	[DefaultValue(1)]
	TEXTBOX = 243,
	[DefaultValue(1)]
	TEXTREF = 147,
	[DefaultValue(1)]
	UNREGISTER = 201,
	[DefaultValue(1)]
	USDOLLAR = 204,
	[DefaultValue(1)]
	VOLATILE = 237,
	[DefaultValue(1)]
	WINDOWS = 91,
	[DefaultValue(1)]
	WINDOWTITLE = 263
}
