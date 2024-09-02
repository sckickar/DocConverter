using System.Collections.Generic;

namespace DocGen.Pdf.Security;

internal class ECBrainpoolAlgorithm
{
	internal class ECBP160 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECBP160();

		private ECBP160()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			FiniteCurves finiteCurves = new FiniteCurves(new Number(PdfHexEncoder.DecodeString("RTk1RTRBNUY3MzcwNTlEQzYwREZDN0FEOTVCM0Q4MTM5NTE1NjIwRg=="), 16), new Number(PdfHexEncoder.DecodeString("MzQwRTdCRTJBMjgwRUI3NEUyQkU2MUJBREE3NDVEOTdFOEY3QzMwMA=="), 16), new Number(PdfHexEncoder.DecodeString("MUU1ODlBODU5NTQyMzQxMjEzNEZBQTJEQkRFQzk1QzhEODY3NUU1OA=="), 16));
			return new ECX9Field(finiteCurves, finiteCurves.GetDecodedECPoint(PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("MDRCRUQ1QUYxNkVBM0Y2QTRGNjI5MzhDNDYzMUVCNUFGN0JEQkNEQkMzMTY2N0NCNDc3QTFBOEVDMzM4Rjk0NzQxNjY5Qzk3NjMxNkRBNjMyMQ=="))), new Number(PdfHexEncoder.DecodeString("RTk1RTRBNUY3MzcwNTlEQzYwREY1OTkxRDQ1MDI5NDA5RTYwRkMwOQ=="), 16), new Number("01", 16));
		}
	}

	internal class ECBP160T1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECBP160T1();

		private ECBP160T1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			FiniteCurves finiteCurves = new FiniteCurves(new Number(PdfHexEncoder.DecodeString("RTk1RTRBNUY3MzcwNTlEQzYwREZDN0FEOTVCM0Q4MTM5NTE1NjIwRg=="), 16), new Number(PdfHexEncoder.DecodeString("RTk1RTRBNUY3MzcwNTlEQzYwREZDN0FEOTVCM0Q4MTM5NTE1NjIwQw=="), 16), new Number(PdfHexEncoder.DecodeString("N0E1NTZCNkRBRTUzNUI3QjUxRUQyQzREN0RBQTdBMEI1QzU1RjM4MA=="), 16));
			return new ECX9Field(finiteCurves, finiteCurves.GetDecodedECPoint(PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("MDRCMTk5QjEzQjlCMzRFRkMxMzk3RTY0QkFFQjA1QUNDMjY1RkYyMzc4QURENjcxOEI3QzdDMTk2MUYwOTkxQjg0MjQ0Mzc3MjE1MkM5RTBBRA=="))), new Number(PdfHexEncoder.DecodeString("RTk1RTRBNUY3MzcwNTlEQzYwREY1OTkxRDQ1MDI5NDA5RTYwRkMwOQ=="), 16), new Number("01", 16));
		}
	}

	internal class ECBP192R1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECBP192R1();

		private ECBP192R1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			FiniteCurves finiteCurves = new FiniteCurves(new Number(PdfHexEncoder.DecodeString("QzMwMkY0MUQ5MzJBMzZDREE3QTM0NjMwOTNEMThEQjc4RkNFNDc2REUxQTg2Mjk3"), 16), new Number(PdfHexEncoder.DecodeString("NkE5MTE3NDA3NkIxRTBFMTlDMzlDMDMxRkU4Njg1QzFDQUUwNDBFNUM2OUEyOEVG"), 16), new Number(PdfHexEncoder.DecodeString("NDY5QTI4RUY3QzI4Q0NBM0RDNzIxRDA0NEY0NDk2QkNDQTdFRjQxNDZGQkYyNUM5"), 16));
			return new ECX9Field(finiteCurves, finiteCurves.GetDecodedECPoint(PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("MDRDMEEwNjQ3RUFBQjZBNDg3NTNCMDMzQzU2Q0IwRjA5MDBBMkY1QzQ4NTMzNzVGRDYxNEI2OTA4NjZBQkQ1QkI4OEI1RjQ4MjhDMTQ5MDAwMkU2NzczRkEyRkEyOTlCOEY="))), new Number(PdfHexEncoder.DecodeString("QzMwMkY0MUQ5MzJBMzZDREE3QTM0NjJGOUU5RTkxNkI1QkU4RjEwMjlBQzRBQ0Mx"), 16), new Number("01", 16));
		}
	}

	internal class ECBP192T1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECBP192T1();

		private ECBP192T1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			FiniteCurves finiteCurves = new FiniteCurves(new Number(PdfHexEncoder.DecodeString("QzMwMkY0MUQ5MzJBMzZDREE3QTM0NjMwOTNEMThEQjc4RkNFNDc2REUxQTg2Mjk3"), 16), new Number(PdfHexEncoder.DecodeString("QzMwMkY0MUQ5MzJBMzZDREE3QTM0NjMwOTNEMThEQjc4RkNFNDc2REUxQTg2Mjk0"), 16), new Number(PdfHexEncoder.DecodeString("MTNENTZGRkFFQzc4NjgxRTY4RjlERUI0M0IzNUJFQzJGQjY4NTQyRTI3ODk3Qjc5"), 16));
			return new ECX9Field(finiteCurves, finiteCurves.GetDecodedECPoint(PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("MDQzQUU5RTU4QzgyRjYzQzMwMjgyRTFGRTdCQkY0M0ZBNzJDNDQ2QUY2RjQ2MTgxMjkwOTdFMkM1NjY3QzIyMjNBOTAyQUI1Q0E0NDlEMDA4NEI3RTVCM0RFN0NDQzAxQzk="))), new Number(PdfHexEncoder.DecodeString("QzMwMkY0MUQ5MzJBMzZDREE3QTM0NjJGOUU5RTkxNkI1QkU4RjEwMjlBQzRBQ0Mx"), 16), new Number("01", 16));
		}
	}

	internal class ECBP224R1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECBP224R1();

		private ECBP224R1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			FiniteCurves finiteCurves = new FiniteCurves(new Number(PdfHexEncoder.DecodeString("RDdDMTM0QUEyNjQzNjY4NjJBMTgzMDI1NzVEMUQ3ODdCMDlGMDc1Nzk3REE4OUY1N0VDOEMwRkY="), 16), new Number(PdfHexEncoder.DecodeString("NjhBNUU2MkNBOUNFNkMxQzI5OTgwM0E2QzE1MzBCNTE0RTE4MkFEOEIwMDQyQTU5Q0FEMjlGNDM="), 16), new Number(PdfHexEncoder.DecodeString("MjU4MEY2M0NDRkU0NDEzODg3MDcxM0IxQTkyMzY5RTMzRTIxMzVEMjY2REJCMzcyMzg2QzQwMEI="), 16));
			return new ECX9Field(finiteCurves, finiteCurves.GetDecodedECPoint(PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("MDQwRDkwMjlBRDJDN0U1Q0Y0MzQwODIzQjJBODdEQzY4QzlFNENFMzE3NEMxRTZFRkRFRTEyQzA3RDU4QUE1NkY3NzJDMDcyNkYyNEM2Qjg5RTRFQ0RBQzI0MzU0QjlFOTlDQUEzRjZEMzc2MTQwMkNE"))), new Number(PdfHexEncoder.DecodeString("RDdDMTM0QUEyNjQzNjY4NjJBMTgzMDI1NzVEMEZCOThEMTE2QkM0QjZEREVCQ0EzQTVBNzkzOUY="), 16), new Number("01", 16));
		}
	}

	internal class ECBP224T1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECBP224T1();

		private ECBP224T1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			FiniteCurves finiteCurves = new FiniteCurves(new Number(PdfHexEncoder.DecodeString("RDdDMTM0QUEyNjQzNjY4NjJBMTgzMDI1NzVEMUQ3ODdCMDlGMDc1Nzk3REE4OUY1N0VDOEMwRkY="), 16), new Number(PdfHexEncoder.DecodeString("RDdDMTM0QUEyNjQzNjY4NjJBMTgzMDI1NzVEMUQ3ODdCMDlGMDc1Nzk3REE4OUY1N0VDOEMwRkM="), 16), new Number(PdfHexEncoder.DecodeString("NEIzMzdEOTM0MTA0Q0Q3QkVGMjcxQkY2MENFRDFFRDIwREExNEMwOEIzQkI2NEYxOEE2MDg4OEQ="), 16));
			return new ECX9Field(finiteCurves, finiteCurves.GetDecodedECPoint(PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("MDQ2QUIxRTM0NENFMjVGRjM4OTY0MjRFN0ZGRTE0NzYyRUNCNDlGODkyOEFDMEM3NjAyOUI0RDU4MDAzNzRFOUY1MTQzRTU2OENEMjNGM0Y0RDdDMEQ0QjFFNDFDOENDMEQxQzZBQkQ1RjFBNDZEQjRD"))), new Number(PdfHexEncoder.DecodeString("RDdDMTM0QUEyNjQzNjY4NjJBMTgzMDI1NzVEMEZCOThEMTE2QkM0QjZEREVCQ0EzQTVBNzkzOUY="), 16), new Number("01", 16));
		}
	}

	internal class ECBP256R1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECBP256R1();

		private ECBP256R1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			FiniteCurves finiteCurves = new FiniteCurves(new Number(PdfHexEncoder.DecodeString("QTlGQjU3REJBMUVFQTlCQzNFNjYwQTkwOUQ4MzhENzI2RTNCRjYyM0Q1MjYyMDI4MjAxMzQ4MUQxRjZFNTM3Nw=="), 16), new Number(PdfHexEncoder.DecodeString("N0Q1QTA5NzVGQzJDMzA1N0VFRjY3NTMwNDE3QUZGRTdGQjgwNTVDMTI2REM1QzZDRTk0QTRCNDRGMzMwQjVEOQ=="), 16), new Number(PdfHexEncoder.DecodeString("MjZEQzVDNkNFOTRBNEI0NEYzMzBCNUQ5QkJENzdDQkY5NTg0MTYyOTVDRjdFMUNFNkJDQ0RDMThGRjhDMDdCNg=="), 16));
			return new ECX9Field(finiteCurves, finiteCurves.GetDecodedECPoint(PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("MDQ4QkQyQUVCOUNCN0U1N0NCMkM0QjQ4MkZGQzgxQjdBRkI5REUyN0UxRTNCRDIzQzIzQTQ0NTNCRDlBQ0UzMjYyNTQ3RUY4MzVDM0RBQzRGRDk3Rjg0NjFBMTQ2MTFEQzlDMjc3NDUxMzJERUQ4RTU0NUMxRDU0QzcyRjA0Njk5Nw=="))), new Number(PdfHexEncoder.DecodeString("QTlGQjU3REJBMUVFQTlCQzNFNjYwQTkwOUQ4MzhENzE4QzM5N0FBM0I1NjFBNkY3OTAxRTBFODI5NzQ4NTZBNw=="), 16), new Number("01", 16));
		}
	}

	internal class ECBP256T1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECBP256T1();

		private ECBP256T1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			FiniteCurves finiteCurves = new FiniteCurves(new Number(PdfHexEncoder.DecodeString("QTlGQjU3REJBMUVFQTlCQzNFNjYwQTkwOUQ4MzhENzI2RTNCRjYyM0Q1MjYyMDI4MjAxMzQ4MUQxRjZFNTM3Nw=="), 16), new Number(PdfHexEncoder.DecodeString("QTlGQjU3REJBMUVFQTlCQzNFNjYwQTkwOUQ4MzhENzI2RTNCRjYyM0Q1MjYyMDI4MjAxMzQ4MUQxRjZFNTM3NA=="), 16), new Number(PdfHexEncoder.DecodeString("NjYyQzYxQzQzMEQ4NEVBNEZFNjZBNzczM0QwQjc2QjdCRjkzRUJDNEFGMkY0OTI1NkFFNTgxMDFGRUU5MkIwNA=="), 16));
			return new ECX9Field(finiteCurves, finiteCurves.GetDecodedECPoint(PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("MDRBM0U4RUIzQ0MxQ0ZFN0I3NzMyMjEzQjIzQTY1NjE0OUFGQTE0MkM0N0FBRkJDMkI3OUExOTE1NjJFMTMwNUY0MkQ5OTZDODIzNDM5QzU2RDdGN0IyMkUxNDY0NDQxN0U2OUJDQjZERTM5RDAyNzAwMURBQkU4RjM1QjI1QzlCRQ=="))), new Number(PdfHexEncoder.DecodeString("QTlGQjU3REJBMUVFQTlCQzNFNjYwQTkwOUQ4MzhENzE4QzM5N0FBM0I1NjFBNkY3OTAxRTBFODI5NzQ4NTZBNw=="), 16), new Number("01", 16));
		}
	}

	internal class ECBP320R1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECBP320R1();

		private ECBP320R1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			FiniteCurves finiteCurves = new FiniteCurves(new Number(PdfHexEncoder.DecodeString("RDM1RTQ3MjAzNkJDNEZCN0UxM0M3ODVFRDIwMUUwNjVGOThGQ0ZBNkY2RjQwREVGNEY5MkI5RUM3ODkzRUMyOEZDRDQxMkIxRjFCMzJFMjc="), 16), new Number(PdfHexEncoder.DecodeString("M0VFMzBCNTY4RkJBQjBGODgzQ0NFQkQ0NkQzRjNCQjhBMkE3MzUxM0Y1RUI3OURBNjYxOTBFQjA4NUZGQTlGNDkyRjM3NUE5N0Q4NjBFQjQ="), 16), new Number(PdfHexEncoder.DecodeString("NTIwODgzOTQ5REZEQkM0MkQzQUQxOTg2NDA2ODhBNkZFMTNGNDEzNDk1NTRCNDlBQ0MzMURDQ0Q4ODQ1Mzk4MTZGNUVCNEFDOEZCMUYxQTY="), 16));
			return new ECX9Field(finiteCurves, finiteCurves.GetDecodedECPoint(PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("MDQ0M0JEN0U5QUZCNTNEOEI4NTI4OUJDQzQ4RUU1QkZFNkYyMDEzN0QxMEEwODdFQjZFNzg3MUUyQTEwQTU5OUM3MTBBRjhEMEQzOUUyMDYxMTE0RkREMDU1NDVFQzFDQzhBQjQwOTMyNDdGNzcyNzVFMDc0M0ZGRUQxMTcxODJFQUE5Qzc3ODc3QUFBQzZBQzdEMzUyNDVEMTY5MkU4RUUx"))), new Number(PdfHexEncoder.DecodeString("RDM1RTQ3MjAzNkJDNEZCN0UxM0M3ODVFRDIwMUUwNjVGOThGQ0ZBNUI2OEYxMkEzMkQ0ODJFQzdFRTg2NThFOTg2OTE1NTVCNDRDNTkzMTE="), 16), new Number("01", 16));
		}
	}

	internal class ECBP320T1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECBP320T1();

		private ECBP320T1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			FiniteCurves finiteCurves = new FiniteCurves(new Number(PdfHexEncoder.DecodeString("RDM1RTQ3MjAzNkJDNEZCN0UxM0M3ODVFRDIwMUUwNjVGOThGQ0ZBNkY2RjQwREVGNEY5MkI5RUM3ODkzRUMyOEZDRDQxMkIxRjFCMzJFMjc="), 16), new Number(PdfHexEncoder.DecodeString("RDM1RTQ3MjAzNkJDNEZCN0UxM0M3ODVFRDIwMUUwNjVGOThGQ0ZBNkY2RjQwREVGNEY5MkI5RUM3ODkzRUMyOEZDRDQxMkIxRjFCMzJFMjQ="), 16), new Number(PdfHexEncoder.DecodeString("QTdGNTYxRTAzOEVCMUVENTYwQjNEMTQ3REI3ODIwMTMwNjRDMTlGMjdFRDI3QzY3ODBBQUY3N0ZCOEE1NDdDRUI1QjRGRUY0MjIzNDAzNTM="), 16));
			return new ECX9Field(finiteCurves, finiteCurves.GetDecodedECPoint(PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("MDQ5MjVCRTlGQjAxQUZDNkZCNEQzRTdENDk5MDAxMEY4MTM0MDhBQjEwNkM0RjA5Q0I3RUUwNzg2OENDMTM2RkZGMzM1N0Y2MjRBMjFCRUQ1MjYzQkEzQTdBMjc0ODNFQkY2NjcxREJFRjdBQkIzMEVCRUUwODRFNThBMEIwNzdBRDQyQTVBMDk4OUQxRUU3MUIxQjlCQzA0NTVGQjBEMkMz"))), new Number(PdfHexEncoder.DecodeString("RDM1RTQ3MjAzNkJDNEZCN0UxM0M3ODVFRDIwMUUwNjVGOThGQ0ZBNUI2OEYxMkEzMkQ0ODJFQzdFRTg2NThFOTg2OTE1NTVCNDRDNTkzMTE="), 16), new Number("01", 16));
		}
	}

	internal class ECBP384R1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECBP384R1();

		private ECBP384R1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			FiniteCurves finiteCurves = new FiniteCurves(new Number(PdfHexEncoder.DecodeString("OENCOTFFODJBMzM4NkQyODBGNUQ2RjdFNTBFNjQxREYxNTJGNzEwOUVENTQ1NkI0MTJCMURBMTk3RkI3MTEyM0FDRDNBNzI5OTAxRDFBNzE4NzQ3MDAxMzMxMDdFQzUz"), 16), new Number(PdfHexEncoder.DecodeString("N0JDMzgyQzYzRDhDMTUwQzNDNzIwODBBQ0UwNUFGQTBDMkJFQTI4RTRGQjIyNzg3MTM5MTY1RUZCQTkxRjkwRjhBQTU4MTRBNTAzQUQ0RUIwNEE4QzdERDIyQ0UyODI2"), 16), new Number(PdfHexEncoder.DecodeString("NEE4QzdERDIyQ0UyODI2OEIzOUI1NTQxNkYwNDQ3QzJGQjc3REUxMDdEQ0QyQTYyRTg4MEVBNTNFRUI2MkQ1N0NCNDM5MDI5NURCQzk5NDNBQjc4Njk2RkE1MDRDMTE="), 16));
			return new ECX9Field(finiteCurves, finiteCurves.GetDecodedECPoint(PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("MDQxRDFDNjRGMDY4Q0Y0NUZGQTJBNjNBODFCN0MxM0Y2Qjg4NDdBM0U3N0VGMTRGRTNEQjdGQ0FGRTBDQkQxMEU4RTgyNkUwMzQzNkQ2NDZBQUVGODdCMkUyNDdENEFGMUU4QUJFMUQ3NTIwRjlDMkE0NUNCMUVCOEU5NUNGRDU1MjYyQjcwQjI5RkVFQzU4NjRFMTlDMDU0RkY5OTEyOTI4MEU0NjQ2MjE3NzkxODExMTQyODIwMzQxMjYzQzUzMTU="))), new Number(PdfHexEncoder.DecodeString("OENCOTFFODJBMzM4NkQyODBGNUQ2RjdFNTBFNjQxREYxNTJGNzEwOUVENTQ1NkIzMUYxNjZFNkNBQzA0MjVBN0NGM0FCNkFGNkI3RkMzMTAzQjg4MzIwMkU5MDQ2NTY1"), 16), new Number("01", 16));
		}
	}

	internal class ECBP384T1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECBP384T1();

		private ECBP384T1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			FiniteCurves finiteCurves = new FiniteCurves(new Number(PdfHexEncoder.DecodeString("OENCOTFFODJBMzM4NkQyODBGNUQ2RjdFNTBFNjQxREYxNTJGNzEwOUVENTQ1NkI0MTJCMURBMTk3RkI3MTEyM0FDRDNBNzI5OTAxRDFBNzE4NzQ3MDAxMzMxMDdFQzUz"), 16), new Number(PdfHexEncoder.DecodeString("OENCOTFFODJBMzM4NkQyODBGNUQ2RjdFNTBFNjQxREYxNTJGNzEwOUVENTQ1NkI0MTJCMURBMTk3RkI3MTEyM0FDRDNBNzI5OTAxRDFBNzE4NzQ3MDAxMzMxMDdFQzUw"), 16), new Number(PdfHexEncoder.DecodeString("N0Y1MTlFQURBN0JEQTgxQkQ4MjZEQkE2NDc5MTBGOEM0QjkzNDZFRDhDQ0RDNjRFNEIxQUJEMTE3NTZEQ0UxRDIwNzRBQTI2M0I4ODgwNUNFRDcwMzU1QTMzQjQ3MUVF"), 16));
			return new ECX9Field(finiteCurves, finiteCurves.GetDecodedECPoint(PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("MDQxOERFOThCMDJEQjlBMzA2RjJBRkNENzIzNUY3MkE4MTlCODBBQjEyRUJENjUzMTcyNDc2RkVDRDQ2MkFBQkZGQzRGRjE5MUI5NDZBNUY1NEQ4RDBBQTJGNDE4ODA4Q0MyNUFCMDU2OTYyRDMwNjUxQTExNEFGRDI3NTVBRDMzNjc0N0Y5MzQ3NUI3QTFGQ0EzQjg4RjJCNkEyMDhDQ0ZFNDY5NDA4NTg0REMyQjI5MTI2NzVCRjVCOUU1ODI5Mjg="))), new Number(PdfHexEncoder.DecodeString("OENCOTFFODJBMzM4NkQyODBGNUQ2RjdFNTBFNjQxREYxNTJGNzEwOUVENTQ1NkIzMUYxNjZFNkNBQzA0MjVBN0NGM0FCNkFGNkI3RkMzMTAzQjg4MzIwMkU5MDQ2NTY1"), 16), new Number("01", 16));
		}
	}

	internal class ECBP512R1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECBP512R1();

		private ECBP512R1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			FiniteCurves finiteCurves = new FiniteCurves(new Number(PdfHexEncoder.DecodeString("QUFERDlEQjhEQkU5QzQ4QjNGRDRFNkFFMzNDOUZDMDdDQjMwOERCM0IzQzlEMjBFRDY2MzlDQ0E3MDMzMDg3MTdENEQ5QjAwOUJDNjY4NDJBRUNEQTEyQUU2QTM4MEU2Mjg4MUZGMkYyRDgyQzY4NTI4QUE2MDU2NTgzQTQ4RjM="), 16), new Number(PdfHexEncoder.DecodeString("NzgzMEEzMzE4QjYwM0I4OUUyMzI3MTQ1QUMyMzRDQzU5NENCREQ4RDNERjkxNjEwQTgzNDQxQ0FFQTk4NjNCQzJERUQ1RDVBQTgyNTNBQTEwQTJFRjFDOThCOUFDOEI1N0YxMTE3QTcyQkYyQzdCOUU3QzFBQzRENzdGQzk0Q0E="), 16), new Number(PdfHexEncoder.DecodeString("M0RGOTE2MTBBODM0NDFDQUVBOTg2M0JDMkRFRDVENUFBODI1M0FBMTBBMkVGMUM5OEI5QUM4QjU3RjExMTdBNzJCRjJDN0I5RTdDMUFDNEQ3N0ZDOTRDQURDMDgzRTY3OTg0MDUwQjc1RUJBRTVERDI4MDlCRDYzODAxNkY3MjM="), 16));
			return new ECX9Field(finiteCurves, finiteCurves.GetDecodedECPoint(PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("MDQ4MUFFRTRCREQ4MkVEOTY0NUEyMTMyMkU5QzRDNkE5Mzg1RUQ5RjcwQjVEOTE2QzFCNDNCNjJFRUY0RDAwOThFRkYzQjFGNzhFMkQwRDQ4RDUwRDE2ODdCOTNCOTdENUY3QzZENTA0NzQwNkE1RTY4OEIzNTIyMDlCQ0I5RjgyMjdEREUzODVENTY2MzMyRUNDMEVBQkZBOUNGNzgyMkZERjIwOUY3MDAyNEE1N0IxQUEwMDBDNTVCODgxRjgxMTFCMkRDREU0OTRBNUY0ODVFNUJDQTRCRDg4QTI3NjNBRUQxQ0EyQjJGQThGMDU0MDY3OENEMUUwRjNBRDgwODky"))), new Number(PdfHexEncoder.DecodeString("QUFERDlEQjhEQkU5QzQ4QjNGRDRFNkFFMzNDOUZDMDdDQjMwOERCM0IzQzlEMjBFRDY2MzlDQ0E3MDMzMDg3MDU1M0U1QzQxNENBOTI2MTk0MTg2NjExOTdGQUMxMDQ3MURCMUQzODEwODVEREFEREI1ODc5NjgyOUNBOTAwNjk="), 16), new Number("01", 16));
		}
	}

	internal class ECBP512T1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECBP512T1();

		private ECBP512T1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			FiniteCurves finiteCurves = new FiniteCurves(new Number(PdfHexEncoder.DecodeString("QUFERDlEQjhEQkU5QzQ4QjNGRDRFNkFFMzNDOUZDMDdDQjMwOERCM0IzQzlEMjBFRDY2MzlDQ0E3MDMzMDg3MTdENEQ5QjAwOUJDNjY4NDJBRUNEQTEyQUU2QTM4MEU2Mjg4MUZGMkYyRDgyQzY4NTI4QUE2MDU2NTgzQTQ4RjM="), 16), new Number(PdfHexEncoder.DecodeString("QUFERDlEQjhEQkU5QzQ4QjNGRDRFNkFFMzNDOUZDMDdDQjMwOERCM0IzQzlEMjBFRDY2MzlDQ0E3MDMzMDg3MTdENEQ5QjAwOUJDNjY4NDJBRUNEQTEyQUU2QTM4MEU2Mjg4MUZGMkYyRDgyQzY4NTI4QUE2MDU2NTgzQTQ4RjA="), 16), new Number(PdfHexEncoder.DecodeString("N0NCQkJDRjk0NDFDRkFCNzZFMTg5MEU0Njg4NEVBRTMyMUY3MEMwQkNCNDk4MTUyNzg5NzUwNEJFQzNFMzZBNjJCQ0RGQTIzMDQ5NzY1NDBGNjQ1MDA4NUYyREFFMTQ1QzIyNTUzQjQ2NTc2MzY4OTE4MEVBMjU3MTg2NzQyM0U="), 16));
			return new ECX9Field(finiteCurves, finiteCurves.GetDecodedECPoint(PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("MDQ2NDBFQ0U1QzEyNzg4NzE3QjlDMUJBMDZDQkMyQTZGRUJBODU4NDI0NThDNTZEREU5REIxNzU4RDM5QzAzMTNEODJCQTUxNzM1Q0RCM0VBNDk5QUE3N0E3RDY5NDNBNjRGN0EzRjI1RkUyNkYwNkI1MUJBQTI2OTZGQTkwMzVEQTVCNTM0QkQ1OTVGNUFGMEZBMkM4OTIzNzZDODRBQ0UxQkI0RTMwMTlCNzE2MzRDMDExMzExNTlDQUUwM0NFRTlEOTkzMjE4NEJFRUYyMTZCRDcxREYyREFERjg2QTYyNzMwNkVDRkY5NkRCQjhCQUNFMTk4QjYxRTAwRjhCMzMy"))), new Number(PdfHexEncoder.DecodeString("QUFERDlEQjhEQkU5QzQ4QjNGRDRFNkFFMzNDOUZDMDdDQjMwOERCM0IzQzlEMjBFRDY2MzlDQ0E3MDMzMDg3MDU1M0U1QzQxNENBOTI2MTk0MTg2NjExOTdGQUMxMDQ3MURCMUQzODEwODVEREFEREI1ODc5NjgyOUNBOTAwNjk="), 16), new Number("01", 16));
		}
	}

	internal static Dictionary<DerObjectID, ECX9Params> curves;

	internal static Dictionary<string, DerObjectID> objectIds;

	internal static Dictionary<DerObjectID, string> curveNames;

	private static void CreateCurve(string name, DerObjectID oid, ECX9Params id)
	{
		objectIds.Add(name, oid);
		curveNames.Add(oid, name);
		curves.Add(oid, id);
	}

	static ECBrainpoolAlgorithm()
	{
		curves = new Dictionary<DerObjectID, ECX9Params>();
		objectIds = new Dictionary<string, DerObjectID>();
		curveNames = new Dictionary<DerObjectID, string>();
		CreateCurve("brainpoolp160r1", ECBrainpoolIDs.BrainpoolP160R1, ECBP160.primeField);
		CreateCurve("brainpoolp160t1", ECBrainpoolIDs.BrainpoolP160T1, ECBP160T1.primeField);
		CreateCurve("brainpoolp192r1", ECBrainpoolIDs.BrainpoolP192R1, ECBP192R1.primeField);
		CreateCurve("brainpoolp192t1", ECBrainpoolIDs.BrainpoolP192T1, ECBP192T1.primeField);
		CreateCurve("brainpoolp224r1", ECBrainpoolIDs.BrainpoolP224R1, ECBP224R1.primeField);
		CreateCurve("brainpoolp224t1", ECBrainpoolIDs.BrainpoolP224T1, ECBP224T1.primeField);
		CreateCurve("brainpoolp256r1", ECBrainpoolIDs.BrainpoolP256R1, ECBP256R1.primeField);
		CreateCurve("brainpoolp256t1", ECBrainpoolIDs.BrainpoolP256T1, ECBP256T1.primeField);
		CreateCurve("brainpoolp320r1", ECBrainpoolIDs.BrainpoolP320R1, ECBP320R1.primeField);
		CreateCurve("brainpoolp320t1", ECBrainpoolIDs.BrainpoolP320T1, ECBP320T1.primeField);
		CreateCurve("brainpoolp384r1", ECBrainpoolIDs.BrainpoolP384R1, ECBP384R1.primeField);
		CreateCurve("brainpoolp384t1", ECBrainpoolIDs.BrainpoolP384T1, ECBP384T1.primeField);
		CreateCurve("brainpoolp512r1", ECBrainpoolIDs.BrainpoolP512R1, ECBP512R1.primeField);
		CreateCurve("brainpoolp512t1", ECBrainpoolIDs.BrainpoolP512T1, ECBP512T1.primeField);
	}

	public static ECX9Field GetByOid(DerObjectID oid)
	{
		ECX9Params eCX9Params = null;
		if (curves.ContainsKey(oid))
		{
			eCX9Params = curves[oid];
		}
		return eCX9Params?.Parameters;
	}
}
