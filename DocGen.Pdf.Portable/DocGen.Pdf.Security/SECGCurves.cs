using System.Collections.Generic;

namespace DocGen.Pdf.Security;

internal sealed class SECGCurves
{
	internal class ECSecp112R1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSecp112R1();

		private ECSecp112R1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number number = DecodeHex(PdfHexEncoder.DecodeString("REI3QzJBQkY2MkUzNUU2NjgwNzZCRUFEMjA4Qg=="));
			Number elementA = DecodeHex(PdfHexEncoder.DecodeString("REI3QzJBQkY2MkUzNUU2NjgwNzZCRUFEMjA4OA=="));
			Number elementB = DecodeHex(PdfHexEncoder.DecodeString("NjU5RUY4QkEwNDM5MTZFRURFODkxMTcwMkIyMg=="));
			byte[] seed = PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("MDBGNTBCMDI4RTRENjk2RTY3Njg3NTYxNTE3NTI5MDQ3Mjc4M0ZCMQ=="));
			Number num = DecodeHex(PdfHexEncoder.DecodeString("REI3QzJBQkY2MkUzNUU3NjI4REZBQzY1NjFDNQ=="));
			Number num2 = Number.ValueOf(1L);
			FiniteCurves finiteCurves = new FiniteCurves(number, elementA, elementB);
			EllipticPoint decodedECPoint = finiteCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("MDk0ODcyMzk5OTVBNUVFNzZCNTVGOUMyRjA5OA==") + PdfHexEncoder.DecodeString("QTg5Q0U1QUY4NzI0QzBBMjNFMEUwRkY3NzUwMA==")));
			return new ECX9Field(finiteCurves, decodedECPoint, num, num2, seed);
		}
	}

	internal class ECSecp112R2 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSecp112R2();

		private ECSecp112R2()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number number = DecodeHex(PdfHexEncoder.DecodeString("REI3QzJBQkY2MkUzNUU2NjgwNzZCRUFEMjA4Qg=="));
			Number elementA = DecodeHex(PdfHexEncoder.DecodeString("NjEyN0MyNEMwNUYzOEEwQUFBRjY1QzBFRjAyQw=="));
			Number elementB = DecodeHex(PdfHexEncoder.DecodeString("NTFERUYxODE1REI1RUQ3NEZDQzM0Qzg1RDcwOQ=="));
			byte[] seed = PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("MDAyNzU3QTExMTRENjk2RTY3Njg3NTYxNTE3NTUzMTZDMDVFMEJENA=="));
			Number num = DecodeHex(PdfHexEncoder.DecodeString("MzZERjBBQUZEOEI4RDc1OTdDQTEwNTIwRDA0Qg=="));
			Number num2 = Number.ValueOf(4L);
			FiniteCurves finiteCurves = new FiniteCurves(number, elementA, elementB);
			EllipticPoint decodedECPoint = finiteCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("NEJBMzBBQjVFODkyQjRFMTY0OUREMDkyODY0Mw==") + PdfHexEncoder.DecodeString("QURDRDQ2RjU4ODJFMzc0N0RFRjM2RTk1NkU5Nw==")));
			return new ECX9Field(finiteCurves, decodedECPoint, num, num2, seed);
		}
	}

	internal class ECSecp128R1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSecp128R1();

		private ECSecp128R1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number number = DecodeHex(PdfHexEncoder.DecodeString("RkZGRkZGRkRGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkY="));
			Number elementA = DecodeHex(PdfHexEncoder.DecodeString("RkZGRkZGRkRGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkM="));
			Number elementB = DecodeHex(PdfHexEncoder.DecodeString("RTg3NTc5QzExMDc5RjQzREQ4MjQ5OTNDMkNFRTVFRDM="));
			byte[] seed = PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("MDAwRTBENEQ2OTZFNjc2ODc1NjE1MTc1MENDMDNBNDQ3M0QwMzY3OQ=="));
			Number num = DecodeHex(PdfHexEncoder.DecodeString("RkZGRkZGRkUwMDAwMDAwMDc1QTMwRDFCOTAzOEExMTU="));
			Number num2 = Number.ValueOf(1L);
			FiniteCurves finiteCurves = new FiniteCurves(number, elementA, elementB);
			EllipticPoint decodedECPoint = finiteCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("MTYxRkY3NTI4Qjg5OUIyRDBDMjg2MDdDQTUyQzVCODY=") + PdfHexEncoder.DecodeString("Q0Y1QUM4Mzk1QkFGRUIxM0MwMkRBMjkyRERFRDdBODM=")));
			return new ECX9Field(finiteCurves, decodedECPoint, num, num2, seed);
		}
	}

	internal class ECSecp128R2 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSecp128R2();

		private ECSecp128R2()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number number = DecodeHex(PdfHexEncoder.DecodeString("RkZGRkZGRkRGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkY="));
			Number elementA = DecodeHex(PdfHexEncoder.DecodeString("RDYwMzE5OThEMUIzQkJGRUJGNTlDQzlCQkZGOUFFRTE="));
			Number elementB = DecodeHex(PdfHexEncoder.DecodeString("NUVFRUZDQTM4MEQwMjkxOURDMkM2NTU4QkI2RDhBNUQ="));
			byte[] seed = PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("MDA0RDY5NkU2NzY4NzU2MTUxNzUxMkQ4RjAzNDMxRkNFNjNCODhGNA=="));
			Number num = DecodeHex(PdfHexEncoder.DecodeString("M0ZGRkZGRkY3RkZGRkZGRkJFMDAyNDcyMDYxM0I1QTM="));
			Number num2 = Number.ValueOf(4L);
			FiniteCurves finiteCurves = new FiniteCurves(number, elementA, elementB);
			EllipticPoint decodedECPoint = finiteCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("N0I2QUE1RDg1RTU3Mjk4M0U2RkIzMkE3Q0RFQkMxNDA=") + PdfHexEncoder.DecodeString("MjdCNjkxNkE4OTREM0FFRTcxMDZGRTgwNUZDMzRCNDQ=")));
			return new ECX9Field(finiteCurves, decodedECPoint, num, num2, seed);
		}
	}

	internal class ECSecp160K1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSecp160K1();

		private ECSecp160K1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number number = DecodeHex(PdfHexEncoder.DecodeString("RkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkVGRkZGQUM3Mw=="));
			Number zero = Number.Zero;
			Number elementB = Number.ValueOf(7L);
			byte[] seed = null;
			Number num = DecodeHex(PdfHexEncoder.DecodeString("MDEwMDAwMDAwMDAwMDAwMDAwMDAwMUI4RkExNkRGQUI5QUNBMTZCNkIz"));
			Number num2 = Number.ValueOf(1L);
			FiniteCurves finiteCurves = new FiniteCurves(number, zero, elementB);
			EllipticPoint decodedECPoint = finiteCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("M0I0QzM4MkNFMzdBQTE5MkE0MDE5RTc2MzAzNkY0RjVERDREN0VCQg==") + PdfHexEncoder.DecodeString("OTM4Q0Y5MzUzMThGRENFRDZCQzI4Mjg2NTMxNzMzQzNGMDNDNEZFRQ==")));
			return new ECX9Field(finiteCurves, decodedECPoint, num, num2, seed);
		}
	}

	internal class ECSecp160R1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSecp160R1();

		private ECSecp160R1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number number = DecodeHex(PdfHexEncoder.DecodeString("RkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkY3RkZGRkZGRg=="));
			Number elementA = DecodeHex(PdfHexEncoder.DecodeString("RkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkY3RkZGRkZGQw=="));
			Number elementB = DecodeHex(PdfHexEncoder.DecodeString("MUM5N0JFRkM1NEJEN0E4QjY1QUNGODlGODFENEQ0QURDNTY1RkE0NQ=="));
			byte[] seed = PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("MTA1M0NERTQyQzE0RDY5NkU2NzY4NzU2MTUxNzUzM0JGM0Y4MzM0NQ=="));
			Number num = DecodeHex(PdfHexEncoder.DecodeString("MDEwMDAwMDAwMDAwMDAwMDAwMDAwMUY0QzhGOTI3QUVEM0NBNzUyMjU3"));
			Number num2 = Number.ValueOf(1L);
			FiniteCurves finiteCurves = new FiniteCurves(number, elementA, elementB);
			EllipticPoint decodedECPoint = finiteCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("NEE5NkI1Njg4RUY1NzMyODQ2NjQ2OTg5NjhDMzhCQjkxM0NCRkM4Mg==") + PdfHexEncoder.DecodeString("MjNBNjI4NTUzMTY4OTQ3RDU5RENDOTEyMDQyMzUxMzc3QUM1RkIzMg==")));
			return new ECX9Field(finiteCurves, decodedECPoint, num, num2, seed);
		}
	}

	internal class ECSecp160R2 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSecp160R2();

		private ECSecp160R2()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number number = DecodeHex(PdfHexEncoder.DecodeString("RkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkVGRkZGQUM3Mw=="));
			Number elementA = DecodeHex(PdfHexEncoder.DecodeString("RkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkVGRkZGQUM3MA=="));
			Number elementB = DecodeHex(PdfHexEncoder.DecodeString("QjRFMTM0RDNGQjU5RUI4QkFCNTcyNzQ5MDQ2NjRENUFGNTAzODhCQQ=="));
			byte[] seed = PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("Qjk5Qjk5QjA5OUIzMjNFMDI3MDlBNEQ2OTZFNjc2ODc1NjE1MTc1MQ=="));
			Number num = DecodeHex(PdfHexEncoder.DecodeString("MDEwMDAwMDAwMDAwMDAwMDAwMDAwMDM1MUVFNzg2QTgxOEYzQTFBMTZC"));
			Number num2 = Number.ValueOf(1L);
			FiniteCurves finiteCurves = new FiniteCurves(number, elementA, elementB);
			EllipticPoint decodedECPoint = finiteCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("NTJEQ0IwMzQyOTNBMTE3RTFGNEZGMTFCMzBGNzE5OUQzMTQ0Q0U2RA==") + PdfHexEncoder.DecodeString("RkVBRkZFRjJFMzMxRjI5NkUwNzFGQTBERjk5ODJDRkVBN0Q0M0YyRQ==")));
			return new ECX9Field(finiteCurves, decodedECPoint, num, num2, seed);
		}
	}

	internal class ECSecp192K1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSecp192K1();

		private ECSecp192K1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number number = DecodeHex(PdfHexEncoder.DecodeString("RkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRUZGRkZFRTM3"));
			Number zero = Number.Zero;
			Number elementB = Number.ValueOf(3L);
			byte[] seed = null;
			Number num = DecodeHex(PdfHexEncoder.DecodeString("RkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZFMjZGMkZDMTcwRjY5NDY2QTc0REVGRDhE"));
			Number num2 = Number.ValueOf(1L);
			FiniteCurves finiteCurves = new FiniteCurves(number, zero, elementB);
			EllipticPoint decodedECPoint = finiteCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("REI0RkYxMEVDMDU3RTlBRTI2QjA3RDAyODBCN0Y0MzQxREE1RDFCMUVBRTA2QzdE") + PdfHexEncoder.DecodeString("OUIyRjJGNkQ5QzU2MjhBNzg0NDE2M0QwMTVCRTg2MzQ0MDgyQUE4OEQ5NUUyRjlE")));
			return new ECX9Field(finiteCurves, decodedECPoint, num, num2, seed);
		}
	}

	internal class ECSecp192R1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSecp192R1();

		private ECSecp192R1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number number = DecodeHex(PdfHexEncoder.DecodeString("RkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkVGRkZGRkZGRkZGRkZGRkZG"));
			Number elementA = DecodeHex(PdfHexEncoder.DecodeString("RkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkVGRkZGRkZGRkZGRkZGRkZD"));
			Number elementB = DecodeHex(PdfHexEncoder.DecodeString("NjQyMTA1MTlFNTlDODBFNzBGQTdFOUFCNzIyNDMwNDlGRUI4REVFQ0MxNDZCOUIx"));
			byte[] seed = PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("MzA0NUFFNkZDODQyMkY2NEVENTc5NTI4RDM4MTIwRUFFMTIxOTZENQ=="));
			Number num = DecodeHex(PdfHexEncoder.DecodeString("FFFFFFFFFFFFFFFFFFFFFFFF99DEF836146BC9B1B4D22831"));
			Number num2 = Number.ValueOf(1L);
			FiniteCurves finiteCurves = new FiniteCurves(number, elementA, elementB);
			EllipticPoint decodedECPoint = finiteCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("MTg4REE4MEVCMDMwOTBGNjdDQkYyMEVCNDNBMTg4MDBGNEZGMEFGRDgyRkYxMDEy") + PdfHexEncoder.DecodeString("MDcxOTJCOTVGRkM4REE3ODYzMTAxMUVENkIyNENERDU3M0Y5NzdBMTFFNzk0ODEx")));
			return new ECX9Field(finiteCurves, decodedECPoint, num, num2, seed);
		}
	}

	internal class ECSecp244K1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSecp244K1();

		private ECSecp244K1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number number = DecodeHex(PdfHexEncoder.DecodeString("RkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZFRkZGRkU1NkQ="));
			Number zero = Number.Zero;
			Number elementB = Number.ValueOf(5L);
			byte[] seed = null;
			Number num = DecodeHex(PdfHexEncoder.DecodeString("MDEwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAxRENFOEQyRUM2MTg0Q0FGMEE5NzE3NjlGQjFGNw=="));
			Number num2 = Number.ValueOf(1L);
			FiniteCurves finiteCurves = new FiniteCurves(number, zero, elementB);
			EllipticPoint decodedECPoint = finiteCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("QTE0NTVCMzM0REYwOTlERjMwRkMyOEExNjlBNDY3RTlFNDcwNzVBOTBGN0U2NTBFQjZCN0E0NUM=") + PdfHexEncoder.DecodeString("N0UwODlGRUQ3RkJBMzQ0MjgyQ0FGQkQ2RjdFMzE5RjdDMEIwQkQ1OUUyQ0E0QkRCNTU2RDYxQTU=")));
			return new ECX9Field(finiteCurves, decodedECPoint, num, num2, seed);
		}
	}

	internal class ECSecp244R1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSecp244R1();

		private ECSecp244R1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number number = DecodeHex(PdfHexEncoder.DecodeString("RkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkYwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDE="));
			Number elementA = DecodeHex(PdfHexEncoder.DecodeString("RkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkVGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkU="));
			Number elementB = DecodeHex(PdfHexEncoder.DecodeString("QjQwNTBBODUwQzA0QjNBQkY1NDEzMjU2NTA0NEIwQjdEN0JGRDhCQTI3MEIzOTQzMjM1NUZGQjQ="));
			byte[] seed = PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("QkQ3MTM0NDc5OUQ1QzdGQ0RDNDVCNTlGQTNCOUFCOEY2QTk0OEJDNQ=="));
			Number num = DecodeHex(PdfHexEncoder.DecodeString("RkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRjE2QTJFMEI4RjAzRTEzREQyOTQ1NUM1QzJBM0Q="));
			Number num2 = Number.ValueOf(1L);
			FiniteCurves finiteCurves = new FiniteCurves(number, elementA, elementB);
			EllipticPoint decodedECPoint = finiteCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("QjcwRTBDQkQ2QkI0QkY3RjMyMTM5MEI5NEEwM0MxRDM1NkMyMTEyMjM0MzI4MEQ2MTE1QzFEMjE=") + PdfHexEncoder.DecodeString("QkQzNzYzODhCNUY3MjNGQjRDMjJERkU2Q0Q0Mzc1QTA1QTA3NDc2NDQ0RDU4MTk5ODUwMDdFMzQ=")));
			return new ECX9Field(finiteCurves, decodedECPoint, num, num2, seed);
		}
	}

	internal class ECSecp256K1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSecp256K1();

		private ECSecp256K1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number number = DecodeHex(PdfHexEncoder.DecodeString("RkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkVGRkZGRkMyRg=="));
			Number zero = Number.Zero;
			Number elementB = Number.ValueOf(7L);
			byte[] seed = null;
			Number num = DecodeHex(PdfHexEncoder.DecodeString("RkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkVCQUFFRENFNkFGNDhBMDNCQkZEMjVFOENEMDM2NDE0MQ=="));
			Number num2 = Number.ValueOf(1L);
			FiniteCurves finiteCurves = new FiniteCurves(number, zero, elementB);
			EllipticPoint decodedECPoint = finiteCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("NzlCRTY2N0VGOURDQkJBQzU1QTA2Mjk1Q0U4NzBCMDcwMjlCRkNEQjJEQ0UyOEQ5NTlGMjgxNUIxNkY4MTc5OA==") + PdfHexEncoder.DecodeString("NDgzQURBNzcyNkEzQzQ2NTVEQTRGQkZDMEUxMTA4QThGRDE3QjQ0OEE2ODU1NDE5OUM0N0QwOEZGQjEwRDRCOA==")));
			return new ECX9Field(finiteCurves, decodedECPoint, num, num2, seed);
		}
	}

	internal class ECSecp256R1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSecp256R1();

		private ECSecp256R1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number number = DecodeHex(PdfHexEncoder.DecodeString("RkZGRkZGRkYwMDAwMDAwMTAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMEZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRg=="));
			Number elementA = DecodeHex(PdfHexEncoder.DecodeString("RkZGRkZGRkYwMDAwMDAwMTAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMEZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGQw=="));
			Number elementB = DecodeHex(PdfHexEncoder.DecodeString("NUFDNjM1RDhBQTNBOTNFN0IzRUJCRDU1NzY5ODg2QkM2NTFEMDZCMENDNTNCMEY2M0JDRTNDM0UyN0QyNjA0Qg=="));
			byte[] seed = PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("QzQ5RDM2MDg4NkU3MDQ5MzZBNjY3OEUxMTM5RDI2Qjc4MTlGN0U5MA=="));
			Number num = DecodeHex(PdfHexEncoder.DecodeString("RkZGRkZGRkYwMDAwMDAwMEZGRkZGRkZGRkZGRkZGRkZCQ0U2RkFBREE3MTc5RTg0RjNCOUNBQzJGQzYzMjU1MQ=="));
			Number num2 = Number.ValueOf(1L);
			FiniteCurves finiteCurves = new FiniteCurves(number, elementA, elementB);
			EllipticPoint decodedECPoint = finiteCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("NkIxN0QxRjJFMTJDNDI0N0Y4QkNFNkU1NjNBNDQwRjI3NzAzN0Q4MTJERUIzM0EwRjRBMTM5NDVEODk4QzI5Ng==") + PdfHexEncoder.DecodeString("NEZFMzQyRTJGRTFBN0Y5QjhFRTdFQjRBN0MwRjlFMTYyQkNFMzM1NzZCMzE1RUNFQ0JCNjQwNjgzN0JGNTFGNQ==")));
			return new ECX9Field(finiteCurves, decodedECPoint, num, num2, seed);
		}
	}

	internal class ECSecp364R1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSecp364R1();

		private ECSecp364R1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number number = DecodeHex(PdfHexEncoder.DecodeString("RkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRUZGRkZGRkZGMDAwMDAwMDAwMDAwMDAwMEZGRkZGRkZG"));
			Number elementA = DecodeHex(PdfHexEncoder.DecodeString("RkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRUZGRkZGRkZGMDAwMDAwMDAwMDAwMDAwMEZGRkZGRkZD"));
			Number elementB = DecodeHex(PdfHexEncoder.DecodeString("QjMzMTJGQTdFMjNFRTdFNDk4OEUwNTZCRTNGODJEMTkxODFEOUM2RUZFODE0MTEyMDMxNDA4OEY1MDEzODc1QUM2NTYzOThEOEEyRUQxOUQyQTg1QzhFREQzRUMyQUVG"));
			byte[] seed = PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("QTMzNTkyNkFBMzE5QTI3QTFEMDA4OTZBNjc3M0E0ODI3QUNEQUM3Mw=="));
			Number num = DecodeHex(PdfHexEncoder.DecodeString("RkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGQzc2MzREODFGNDM3MkRERjU4MUEwREIyNDhCMEE3N0FFQ0VDMTk2QUNDQzUyOTcz"));
			Number num2 = Number.ValueOf(1L);
			FiniteCurves finiteCurves = new FiniteCurves(number, elementA, elementB);
			EllipticPoint decodedECPoint = finiteCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("QUE4N0NBMjJCRThCMDUzNzhFQjFDNzFFRjMyMEFENzQ2RTFEM0I2MjhCQTc5Qjk4NTlGNzQxRTA4MjU0MkEzODU1MDJGMjVEQkY1NTI5NkMzQTU0NUUzODcyNzYwQUI3") + PdfHexEncoder.DecodeString("MzYxN0RFNEE5NjI2MkM2RjVEOUU5OEJGOTI5MkRDMjlGOEY0MURCRDI4OUExNDdDRTlEQTMxMTNCNUYwQjhDMDBBNjBCMUNFMUQ3RTgxOUQ3QTQzMUQ3QzkwRUEwRTVG")));
			return new ECX9Field(finiteCurves, decodedECPoint, num, num2, seed);
		}
	}

	internal class ECSecp521R1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSecp521R1();

		private ECSecp521R1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number number = DecodeHex(PdfHexEncoder.DecodeString("MDFGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZG"));
			Number elementA = DecodeHex(PdfHexEncoder.DecodeString("MDFGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZD"));
			Number elementB = DecodeHex(PdfHexEncoder.DecodeString("MDA1MTk1M0VCOTYxOEUxQzlBMUY5MjlBMjFBMEI2ODU0MEVFQTJEQTcyNUI5OUIzMTVGM0I4QjQ4OTkxOEVGMTA5RTE1NjE5Mzk1MUVDN0U5MzdCMTY1MkMwQkQzQkIxQkYwNzM1NzNERjg4M0QyQzM0RjFFRjQ1MUZENDZCNTAzRjAw"));
			byte[] seed = PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("RDA5RTg4MDAyOTFDQjg1Mzk2Q0M2NzE3MzkzMjg0QUFBMERBNjRCQQ=="));
			Number num = DecodeHex(PdfHexEncoder.DecodeString("MDFGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkE1MTg2ODc4M0JGMkY5NjZCN0ZDQzAxNDhGNzA5QTVEMDNCQjVDOUI4ODk5QzQ3QUVCQjZGQjcxRTkxMzg2NDA5"));
			Number num2 = Number.ValueOf(1L);
			FiniteCurves finiteCurves = new FiniteCurves(number, elementA, elementB);
			EllipticPoint decodedECPoint = finiteCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("MDBDNjg1OEUwNkI3MDQwNEU5Q0Q5RTNFQ0I2NjIzOTVCNDQyOUM2NDgxMzkwNTNGQjUyMUY4MjhBRjYwNkI0RDNEQkFBMTRCNUU3N0VGRTc1OTI4RkUxREMxMjdBMkZGQThERTMzNDhCM0MxODU2QTQyOUJGOTdFN0UzMUMyRTVCRDY2") + PdfHexEncoder.DecodeString("MDExODM5Mjk2QTc4OUEzQkMwMDQ1QzhBNUZCNDJDN0QxQkQ5OThGNTQ0NDk1NzlCNDQ2ODE3QUZCRDE3MjczRTY2MkM5N0VFNzI5OTVFRjQyNjQwQzU1MEI5MDEzRkFEMDc2MTM1M0M3MDg2QTI3MkMyNDA4OEJFOTQ3NjlGRDE2NjUw")));
			return new ECX9Field(finiteCurves, decodedECPoint, num, num2, seed);
		}
	}

	internal class ECSecp113R1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSecp113R1();

		private const int bit = 113;

		private const int k = 9;

		private ECSecp113R1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number elementA = DecodeHex(PdfHexEncoder.DecodeString("MDAzMDg4MjUwQ0E2RTdDN0ZFNjQ5Q0U4NTgyMEY3"));
			Number elementB = DecodeHex(PdfHexEncoder.DecodeString("MDBFOEJFRTREM0UyMjYwNzQ0MTg4QkUwRTlDNzIz"));
			byte[] seed = PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("MTBFNzIzQUIxNEQ2OTZFNjc2ODc1NjE1MTc1NkZFQkY4RkNCNDlBOQ=="));
			Number number = DecodeHex(PdfHexEncoder.DecodeString("MDEwMDAwMDAwMDAwMDAwMEQ5Q0NFQzhBMzlFNTZG"));
			Number number2 = Number.ValueOf(2L);
			Field2MCurves field2MCurves = new Field2MCurves(113, 9, elementA, elementB, number, number2);
			EllipticPoint decodedECPoint = field2MCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("MDA5RDczNjE2RjM1RjRBQjE0MDdENzM1NjJDMTBG") + PdfHexEncoder.DecodeString("MDBBNTI4MzAyNzc5NThFRTg0RDEzMTVFRDMxODg2")));
			return new ECX9Field(field2MCurves, decodedECPoint, number, number2, seed);
		}
	}

	internal class ECSecp113R2 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSecp113R2();

		private const int bit = 113;

		private const int k = 9;

		private ECSecp113R2()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number elementA = DecodeHex(PdfHexEncoder.DecodeString("MDA2ODk5MThEQkVDN0U1QTBERDZERkMwQUE1NUM3"));
			Number elementB = DecodeHex(PdfHexEncoder.DecodeString("MDA5NUU5QTlFQzlCMjk3QkQ0QkYzNkUwNTkxODRG"));
			byte[] seed = PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("MTBDMEZCMTU3NjA4NjBERUYxRUVGNEQ2OTZFNjc2ODc1NjE1MTc1RA=="));
			Number number = DecodeHex(PdfHexEncoder.DecodeString("MDEwMDAwMDAwMDAwMDAwMTA4Nzg5QjI0OTZBRjkz"));
			Number number2 = Number.ValueOf(2L);
			Field2MCurves field2MCurves = new Field2MCurves(113, 9, elementA, elementB, number, number2);
			EllipticPoint decodedECPoint = field2MCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("MDFBNTdBNkE3QjI2Q0E1RUY1MkZDREI4MTY0Nzk3") + PdfHexEncoder.DecodeString("MDBCM0FEQzk0RUQxRkU2NzRDMDZFNjk1QkFCQTFE")));
			return new ECX9Field(field2MCurves, decodedECPoint, number, number2, seed);
		}
	}

	internal class ECSecp131R1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSecp131R1();

		private const int bit = 131;

		private const int k1 = 2;

		private const int k2 = 3;

		private const int k3 = 8;

		private ECSecp131R1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number elementA = DecodeHex(PdfHexEncoder.DecodeString("MDdBMTFCMDlBNzZCNTYyMTQ0NDE4RkYzRkY4QzI1NzBCOA=="));
			Number elementB = DecodeHex(PdfHexEncoder.DecodeString("MDIxN0MwNTYxMDg4NEI2M0I5QzZDNzI5MTY3OEY5RDM0MQ=="));
			byte[] seed = PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("NEQ2OTZFNjc2ODc1NjE1MTc1OTg1QkQzQURCQURBMjFCNDNBOTdFMg=="));
			Number number = DecodeHex(PdfHexEncoder.DecodeString("MDQwMDAwMDAwMDAwMDAwMDAyMzEyMzk1M0E5NDY0QjU0RA=="));
			Number number2 = Number.ValueOf(2L);
			Field2MCurves field2MCurves = new Field2MCurves(131, 2, 3, 8, elementA, elementB, number, number2);
			EllipticPoint decodedECPoint = field2MCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("MDA4MUJBRjkxRkRGOTgzM0M0MEY5QzE4MTM0MzYzODM5OQ==") + PdfHexEncoder.DecodeString("MDc4QzZFN0VBMzhDMDAxRjczQzgxMzRCMUI0RUY5RTE1MA==")));
			return new ECX9Field(field2MCurves, decodedECPoint, number, number2, seed);
		}
	}

	internal class ECSecp131R2 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSecp131R2();

		private const int bit = 131;

		private const int k1 = 2;

		private const int k2 = 3;

		private const int k3 = 8;

		private ECSecp131R2()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number elementA = DecodeHex(PdfHexEncoder.DecodeString("MDc4QzZFN0VBMzhDMDAxRjczQzgxMzRCMUI0RUY5RTE1MA=="));
			Number elementB = DecodeHex(PdfHexEncoder.DecodeString("MDRCODI2NkE0NkM1NTY1N0FDNzM0Q0UzOEYwMThGMjE5Mg=="));
			byte[] seed = PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("OTg1QkQzQURCQUQ0RDY5NkU2NzY4NzU2MTUxNzVBMjFCNDNBOTdFMw=="));
			Number number = DecodeHex(PdfHexEncoder.DecodeString("MDQwMDAwMDAwMDAwMDAwMDAxNjk1NEEyMzMwNDlCQTk4Rg=="));
			Number number2 = Number.ValueOf(2L);
			Field2MCurves field2MCurves = new Field2MCurves(131, 2, 3, 8, elementA, elementB, number, number2);
			EllipticPoint decodedECPoint = field2MCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("MDM1NkRDRDhGMkY5NTAzMUFENjUyRDIzOTUxQkIzNjZBOA==") + PdfHexEncoder.DecodeString("MDY0OEYwNkQ4Njc5NDBBNTM2NkQ5RTI2NURFOUVCMjQwRg==")));
			return new ECX9Field(field2MCurves, decodedECPoint, number, number2, seed);
		}
	}

	internal class ECSecp163K1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSecp163K1();

		private const int bit = 163;

		private const int k1 = 3;

		private const int k2 = 6;

		private const int k3 = 7;

		private ECSecp163K1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number elementA = Number.ValueOf(1L);
			Number elementB = Number.ValueOf(1L);
			byte[] seed = null;
			Number number = DecodeHex(PdfHexEncoder.DecodeString("MDQwMDAwMDAwMDAwMDAwMDAwMDAwMjAxMDhBMkUwQ0MwRDk5RjhBNUVG"));
			Number number2 = Number.ValueOf(2L);
			Field2MCurves field2MCurves = new Field2MCurves(163, 3, 6, 7, elementA, elementB, number, number2);
			EllipticPoint decodedECPoint = field2MCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("MDJGRTEzQzA1MzdCQkMxMUFDQUEwN0Q3OTNERTRFNkQ1RTVDOTRFRUU4") + PdfHexEncoder.DecodeString("MDI4OTA3MEZCMDVEMzhGRjU4MzIxRjJFODAwNTM2RDUzOENDREFBM0Q5")));
			return new ECX9Field(field2MCurves, decodedECPoint, number, number2, seed);
		}
	}

	internal class ECSecp163R1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSecp163R1();

		private const int bit = 163;

		private const int k1 = 3;

		private const int k2 = 6;

		private const int k3 = 7;

		private ECSecp163R1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number elementA = DecodeHex(PdfHexEncoder.DecodeString("MDdCNjg4MkNBQUVGQTg0Rjk1NTRGRjg0MjhCRDg4RTI0NkQyNzgyQUUy"));
			Number elementB = DecodeHex(PdfHexEncoder.DecodeString("MDcxMzYxMkRDRERDQjQwQUFCOTQ2QkRBMjlDQTkxRjczQUY5NThBRkQ5"));
			byte[] seed = PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("MjRCN0IxMzdDOEExNEQ2OTZFNjc2ODc1NjE1MTc1NkZEMERBMkU1Qw=="));
			Number number = DecodeHex(PdfHexEncoder.DecodeString("MDNGRkZGRkZGRkZGRkZGRkZGRkZGRjQ4QUFCNjg5QzI5Q0E3MTAyNzlC"));
			Number number2 = Number.ValueOf(2L);
			Field2MCurves field2MCurves = new Field2MCurves(163, 3, 6, 7, elementA, elementB, number, number2);
			EllipticPoint decodedECPoint = field2MCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("MDM2OTk3OTY5N0FCNDM4OTc3ODk1NjY3ODk1NjdGNzg3QTc4NzZBNjU0") + PdfHexEncoder.DecodeString("MDA0MzVFREI0MkVGQUZCMjk4OUQ1MUZFRkNFM0M4MDk4OEY0MUZGODgz")));
			return new ECX9Field(field2MCurves, decodedECPoint, number, number2, seed);
		}
	}

	internal class ECSecp163R2 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSecp163R2();

		private const int bit = 163;

		private const int k1 = 3;

		private const int k2 = 6;

		private const int k3 = 7;

		private ECSecp163R2()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number elementA = Number.ValueOf(1L);
			Number elementB = DecodeHex(PdfHexEncoder.DecodeString("MDIwQTYwMTkwN0I4Qzk1M0NBMTQ4MUVCMTA1MTJGNzg3NDRBMzIwNUZE"));
			byte[] seed = PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("ODVFMjVCRkU1Qzg2MjI2Q0RCMTIwMTZGNzU1M0Y5RDBFNjkzQTI2OA=="));
			Number number = DecodeHex(PdfHexEncoder.DecodeString("MDQwMDAwMDAwMDAwMDAwMDAwMDAwMjkyRkU3N0U3MEMxMkE0MjM0QzMz"));
			Number number2 = Number.ValueOf(2L);
			Field2MCurves field2MCurves = new Field2MCurves(163, 3, 6, 7, elementA, elementB, number, number2);
			EllipticPoint decodedECPoint = field2MCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("MDNGMEVCQTE2Mjg2QTJENTdFQTA5OTExNjhENDk5NDYzN0U4MzQzRTM2") + PdfHexEncoder.DecodeString("MDBENTFGQkM2QzcxQTAwOTRGQTJDREQ1NDVCMTFDNUMwQzc5NzMyNEYx")));
			return new ECX9Field(field2MCurves, decodedECPoint, number, number2, seed);
		}
	}

	internal class ECSecp193R1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSecp193R1();

		private const int bit = 193;

		private const int k = 15;

		private ECSecp193R1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number elementA = DecodeHex(PdfHexEncoder.DecodeString("MDAxNzg1OEZFQjdBOTg5NzUxNjlFMTcxRjc3QjQwODdERTA5OEFDOEE5MTFERjdCMDE="));
			Number elementB = DecodeHex(PdfHexEncoder.DecodeString("MDBGREZCNDlCRkU2QzNBODlGQUNBREFBN0ExRTVCQkM3Q0MxQzJFNUQ4MzE0Nzg4MTQ="));
			byte[] seed = PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("MTAzRkFFQzc0RDY5NkU2NzY4NzU2MTUxNzU3NzdGQzVCMTkxRUYzMA=="));
			Number number = DecodeHex(PdfHexEncoder.DecodeString("MDEwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDBDN0YzNEE3NzhGNDQzQUNDOTIwRUJBNDk="));
			Number number2 = Number.ValueOf(2L);
			Field2MCurves field2MCurves = new Field2MCurves(193, 15, elementA, elementB, number, number2);
			EllipticPoint decodedECPoint = field2MCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("MDFGNDgxQkM1RjBGRjg0QTc0QUQ2Q0RGNkZERUY0QkY2MTc5NjI1MzcyRDhDMEM1RTE=") + PdfHexEncoder.DecodeString("MDAyNUUzOTlGMjkwMzcxMkNDRjNFQTlFM0ExQUQxN0ZCMEIzMjAxQjZBRjdDRTFCMDU=")));
			return new ECX9Field(field2MCurves, decodedECPoint, number, number2, seed);
		}
	}

	internal class ECSecp193R2 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSecp193R2();

		private const int bit = 193;

		private const int k = 15;

		private ECSecp193R2()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number elementA = DecodeHex(PdfHexEncoder.DecodeString("MDE2M0YzNUE1MTM3QzJDRTNFQTZFRDg2NjcxOTBCMEJDNDNFQ0Q2OTk3NzcwMjcwOUI="));
			Number elementB = DecodeHex(PdfHexEncoder.DecodeString("MDBDOUJCOUU4OTI3RDRENjRDMzc3RTJBQjI4NTZBNUIxNkUzRUZCN0Y2MUQ0MzE2QUU="));
			byte[] seed = PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("MTBCN0I0RDY5NkU2NzY4NzU2MTUxNzUxMzdDOEExNkZEMERBMjIxMQ=="));
			Number number = DecodeHex(PdfHexEncoder.DecodeString("MDEwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDE1QUFCNTYxQjAwNTQxM0NDRDRFRTk5RDU="));
			Number number2 = Number.ValueOf(2L);
			Field2MCurves field2MCurves = new Field2MCurves(193, 15, elementA, elementB, number, number2);
			EllipticPoint decodedECPoint = field2MCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("MDBEOUI2N0QxOTJFMDM2N0M4MDNGMzlFMUE3RTgyQ0ExNEE2NTEzNTBBQUU2MTdFOEY=") + PdfHexEncoder.DecodeString("MDFDRTk0MzM1NjA3QzMwNEFDMjlFN0RFRkJEOUNBMDFGNTk2RjkyNzIyNENERUNGNkM=")));
			return new ECX9Field(field2MCurves, decodedECPoint, number, number2, seed);
		}
	}

	internal class ECSect233K1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSect233K1();

		private const int bit = 233;

		private const int k = 74;

		private ECSect233K1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number zero = Number.Zero;
			Number elementB = Number.ValueOf(1L);
			byte[] seed = null;
			Number number = DecodeHex(PdfHexEncoder.DecodeString("ODAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDA2OUQ1QkI5MTVCQ0Q0NkVGQjFBRDVGMTczQUJERg=="));
			Number number2 = Number.ValueOf(4L);
			Field2MCurves field2MCurves = new Field2MCurves(233, 74, zero, elementB, number, number2);
			EllipticPoint decodedECPoint = field2MCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("MDE3MjMyQkE4NTNBN0U3MzFBRjEyOUYyMkZGNDE0OTU2M0E0MTlDMjZCRjUwQTRDOUQ2RUVGQUQ2MTI2") + PdfHexEncoder.DecodeString("MDFEQjUzN0RFQ0U4MTlCN0Y3MEY1NTVBNjdDNDI3QThDRDlCRjE4QUVCOUI1NkUwQzExMDU2RkFFNkEz")));
			return new ECX9Field(field2MCurves, decodedECPoint, number, number2, seed);
		}
	}

	internal class ECSect233R1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSect233R1();

		private const int bit = 233;

		private const int k = 74;

		private ECSect233R1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number elementA = Number.ValueOf(1L);
			Number elementB = DecodeHex(PdfHexEncoder.DecodeString("MDA2NjY0N0VERTZDMzMyQzdGOEMwOTIzQkI1ODIxM0IzMzNCMjBFOUNFNDI4MUZFMTE1RjdEOEY5MEFE"));
			byte[] seed = PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("NzRENTlGRjA3RjZCNDEzRDBFQTE0QjM0NEIyMEEyREIwNDlCNTBDMw=="));
			Number number = DecodeHex(PdfHexEncoder.DecodeString("MDEwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMTNFOTc0RTcyRjhBNjkyMjAzMUQyNjAzQ0ZFMEQ3"));
			Number number2 = Number.ValueOf(2L);
			Field2MCurves field2MCurves = new Field2MCurves(233, 74, elementA, elementB, number, number2);
			EllipticPoint decodedECPoint = field2MCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("MDBGQUM5REZDQkFDODMxM0JCMjEzOUYxQkI3NTVGRUY2NUJDMzkxRjhCMzZGOEY4RUI3MzcxRkQ1NThC") + PdfHexEncoder.DecodeString("MDEwMDZBMDhBNDE5MDMzNTA2NzhFNTg1MjhCRUJGOEEwQkVGRjg2N0E3Q0EzNjcxNkY3RTAxRjgxMDUy")));
			return new ECX9Field(field2MCurves, decodedECPoint, number, number2, seed);
		}
	}

	internal class ECSect239K1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSect239K1();

		private const int bit = 239;

		private const int k = 158;

		private ECSect239K1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number zero = Number.Zero;
			Number elementB = Number.ValueOf(1L);
			byte[] seed = null;
			Number number = DecodeHex(PdfHexEncoder.DecodeString("MjAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwNUE3OUZFQzY3Q0I2RTkxRjFDMURBODAwRTQ3OEE1"));
			Number number2 = Number.ValueOf(4L);
			Field2MCurves field2MCurves = new Field2MCurves(239, 158, zero, elementB, number, number2);
			EllipticPoint decodedECPoint = field2MCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("MjlBMEI2QTg4N0E5ODNFOTczMDk4OEE2ODcyN0E4QjJEMTI2QzQ0Q0MyQ0M3QjJBNjU1NTE5MzAzNURD") + PdfHexEncoder.DecodeString("NzYzMTA4MDRGMTJFNTQ5QkRCMDExQzEwMzA4OUU3MzUxMEFDQjI3NUZDMzEyQTVEQzZCNzY1NTNGMENB")));
			return new ECX9Field(field2MCurves, decodedECPoint, number, number2, seed);
		}
	}

	internal class ECSect283K1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSect283K1();

		private const int bit = 283;

		private const int k1 = 5;

		private const int k2 = 7;

		private const int k3 = 12;

		private ECSect283K1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number zero = Number.Zero;
			Number elementB = Number.ValueOf(1L);
			byte[] seed = null;
			Number number = DecodeHex(PdfHexEncoder.DecodeString("MDFGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRTlBRTJFRDA3NTc3MjY1REZGN0Y5NDQ1MUUwNjFFMTYzQzYx"));
			Number number2 = Number.ValueOf(4L);
			Field2MCurves field2MCurves = new Field2MCurves(283, 5, 7, 12, zero, elementB, number, number2);
			EllipticPoint decodedECPoint = field2MCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("MDUwMzIxM0Y3OENBNDQ4ODNGMUEzQjgxNjJGMTg4RTU1M0NEMjY1RjIzQzE1NjdBMTY4NzY5MTNCMEMyQUMyNDU4NDkyODM2") + PdfHexEncoder.DecodeString("MDFDQ0RBMzgwRjFDOUUzMThEOTBGOTVEMDdFNTQyNkZFODdFNDVDMEU4MTg0Njk4RTQ1OTYyMzY0RTM0MTE2MTc3REQyMjU5")));
			return new ECX9Field(field2MCurves, decodedECPoint, number, number2, seed);
		}
	}

	internal class ECSect283R1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSect283R1();

		private const int bit = 283;

		private const int k1 = 5;

		private const int k2 = 7;

		private const int k3 = 12;

		private ECSect283R1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number elementA = Number.ValueOf(1L);
			Number elementB = DecodeHex(PdfHexEncoder.DecodeString("MDI3QjY4MEFDOEI4NTk2REE1QTRBRjhBMTlBMDMwM0ZDQTk3RkQ3NjQ1MzA5RkEyQTU4MTQ4NUFGNjI2M0UzMTNCNzlBMkY1"));
			byte[] seed = PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("NzdFMkIwNzM3MEVCMEY4MzJBNkRENUI2MkRGQzg4Q0QwNkJCODRCRQ=="));
			Number number = DecodeHex(PdfHexEncoder.DecodeString("MDNGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRUY5MDM5OTY2MEZDOTM4QTkwMTY1QjA0MkE3Q0VGQURCMzA3"));
			Number number2 = Number.ValueOf(2L);
			Field2MCurves field2MCurves = new Field2MCurves(283, 5, 7, 12, elementA, elementB, number, number2);
			EllipticPoint decodedECPoint = field2MCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("MDVGOTM5MjU4REI3REQ5MEUxOTM0RjhDNzBCMERGRUMyRUVEMjVCODU1N0VBQzlDODBFMkUxOThGOENEQkVDRDg2QjEyMDUz") + PdfHexEncoder.DecodeString("MDM2NzY4NTRGRTI0MTQxQ0I5OEZFNkQ0QjIwRDAyQjQ1MTZGRjcwMjM1MEVEREIwODI2Nzc5QzgxM0YwREY0NUJFODExMkY0")));
			return new ECX9Field(field2MCurves, decodedECPoint, number, number2, seed);
		}
	}

	internal class ECSect409K1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSect409K1();

		private const int bit = 409;

		private const int k = 87;

		private ECSect409K1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number zero = Number.Zero;
			Number elementB = Number.ValueOf(1L);
			byte[] seed = null;
			Number number = DecodeHex(PdfHexEncoder.DecodeString("N0ZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRTVGODNCMkQ0RUEyMDQwMEVDNDU1N0Q1RUQzRTNFN0NBNUI0QjVDODNCOEUwMUU1RkNG"));
			Number number2 = Number.ValueOf(4L);
			Field2MCurves field2MCurves = new Field2MCurves(409, 87, zero, elementB, number, number2);
			EllipticPoint decodedECPoint = field2MCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("MDA2MEYwNUY2NThGNDlDMUFEM0FCMTg5MEY3MTg0MjEwRUZEMDk4N0UzMDdDODRDMjdBQ0NGQjhGOUY2N0NDMkM0NjAxODlFQjVBQUFBNjJFRTIyMkVCMUIzNTU0MENGRTkwMjM3NDY=") + PdfHexEncoder.DecodeString("MDFFMzY5MDUwQjdDNEU0MkFDQkExREFDQkYwNDI5OUMzNDYwNzgyRjkxOEVBNDI3RTYzMjUxNjVFOUVBMTBFM0RBNUY2QzQyRTlDNTUyMTVBQTlDQTI3QTU4NjNFQzQ4RDhFMDI4NkI=")));
			return new ECX9Field(field2MCurves, decodedECPoint, number, number2, seed);
		}
	}

	internal class ECSect409R1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSect409R1();

		private const int bit = 409;

		private const int k = 87;

		private ECSect409R1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number elementA = Number.ValueOf(1L);
			Number elementB = DecodeHex(PdfHexEncoder.DecodeString("MDAyMUE1QzJDOEVFOUZFQjVDNEI5QTc1M0I3QjQ3NkI3RkQ2NDIyRUYxRjNERDY3NDc2MUZBOTlENkFDMjdDOEE5QTE5N0IyNzI4MjJGNkNENTdBNTVBQTRGNTBBRTMxN0IxMzU0NUY="));
			byte[] seed = PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("NDA5OUI1QTQ1N0Y5RDY5Rjc5MjEzRDA5NEM0QkNENEQ0MjYyMjEwQg=="));
			Number number = DecodeHex(PdfHexEncoder.DecodeString("MDEwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAxRTJBQUQ2QTYxMkYzMzMwN0JFNUZBNDdDM0M5RTA1MkY4MzgxNjRDRDM3RDlBMjExNzM="));
			Number number2 = Number.ValueOf(2L);
			Field2MCurves field2MCurves = new Field2MCurves(409, 87, elementA, elementB, number, number2);
			EllipticPoint decodedECPoint = field2MCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("MDE1RDQ4NjBEMDg4RERCMzQ5NkIwQzYwNjQ3NTYyNjA0NDFDREU0QUYxNzcxRDREQjAxRkZFNUIzNEU1OTcwM0RDMjU1QTg2OEExMTgwNTE1NjAzQUVBQjYwNzk0RTU0QkI3OTk2QTc=") + PdfHexEncoder.DecodeString("MDA2MUIxQ0ZBQjZCRTVGMzJCQkZBNzgzMjRFRDEwNkE3NjM2QjlDNUE3QkQxOThEMDE1OEFBNEY1NDg4RDA4RjM4NTE0RjFGREY0QjRGNDBEMjE4MUIzNjgxQzM2NEJBMDI3M0M3MDY=")));
			return new ECX9Field(field2MCurves, decodedECPoint, number, number2, seed);
		}
	}

	internal class ECSect571K1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSect571K1();

		private const int bit = 571;

		private const int k1 = 2;

		private const int k2 = 5;

		private const int k3 = 10;

		private ECSect571K1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number zero = Number.Zero;
			Number elementB = Number.ValueOf(1L);
			byte[] seed = null;
			Number number = DecodeHex(PdfHexEncoder.DecodeString("MDIwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMTMxODUwRTFGMTlBNjNFNEIzOTFBOERCOTE3RjQxMzhCNjMwRDg0QkU1RDYzOTM4MUU5MURFQjQ1Q0ZFNzc4RjYzN0MxMDAx"));
			Number number2 = Number.ValueOf(4L);
			Field2MCurves field2MCurves = new Field2MCurves(571, 2, 5, 10, zero, elementB, number, number2);
			EllipticPoint decodedECPoint = field2MCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("MDI2RUI3QTg1OTkyM0ZCQzgyMTg5NjMxRjgxMDNGRTRBQzlDQTI5NzAwMTJENUQ0NjAyNDgwNDgwMTg0MUNBNDQzNzA5NTg0OTNCMjA1RTY0N0RBMzA0REI0Q0VCMDhDQkJEMUJBMzk0OTQ3NzZGQjk4OEI0NzE3NERDQTg4QzdFMjk0NTI4M0EwMUM4OTcy") + PdfHexEncoder.DecodeString("MDM0OURDODA3RjRGQkYzNzRGNEFFQURFM0JDQTk1MzE0REQ1OENFQzlGMzA3QTU0RkZDNjFFRkMwMDZEOEEyQzlENDk3OUMwQUM0NEFFQTc0RkJFQkJCOUY3NzJBRURDQjYyMEIwMUE3QkE3QUYxQjMyMDQzMEM4NTkxOTg0RjYwMUNENEMxNDNFRjFDN0Ez")));
			return new ECX9Field(field2MCurves, decodedECPoint, number, number2, seed);
		}
	}

	internal class ECSect571R1 : ECX9Params
	{
		internal static readonly ECX9Params primeField = new ECSect571R1();

		private const int bit = 571;

		private const int k1 = 2;

		private const int k2 = 5;

		private const int k3 = 10;

		private ECSect571R1()
		{
		}

		protected override ECX9Field DefineParameters()
		{
			Number elementA = Number.ValueOf(1L);
			Number elementB = DecodeHex(PdfHexEncoder.DecodeString("MDJGNDBFN0UyMjIxRjI5NURFMjk3MTE3QjdGM0Q2MkY1QzZBOTdGRkNCOENFRkYxQ0Q2QkE4Q0U0QTlBMThBRDg0RkZBQkJEOEVGQTU5MzMyQkU3QUQ2NzU2QTY2RTI5NEFGRDE4NUE3OEZGMTJBQTUyMEU0REU3MzlCQUNBMEM3RkZFRkY3RjI5NTU3MjdB"));
			byte[] seed = PdfHexEncoder.Decode(PdfHexEncoder.DecodeString("MkFBMDU4RjczQTBFMzNBQjQ4NkIwRjYxMDQxMEM1M0E3RjEzMjMxMA=="));
			Number number = DecodeHex(PdfHexEncoder.DecodeString("MDNGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRkZGRTY2MUNFMThGRjU1OTg3MzA4MDU5QjE4NjgyMzg1MUVDN0REOUNBMTE2MURFOTNENTE3NEQ2NkU4MzgyRTlCQjJGRTg0RTQ3"));
			Number number2 = Number.ValueOf(2L);
			Field2MCurves field2MCurves = new Field2MCurves(571, 2, 5, 10, elementA, elementB, number, number2);
			EllipticPoint decodedECPoint = field2MCurves.GetDecodedECPoint(PdfHexEncoder.Decode("04" + PdfHexEncoder.DecodeString("MDMwMzAwMUQzNEI4NTYyOTZDMTZDMEQ0MEQzQ0Q3NzUwQTkzRDFEMjk1NUZBODBBQTVGNDBGQzhEQjdCMkFCREJERTUzOTUwRjRDMEQyOTNDREQ3MTFBMzVCNjdGQjE0OTlBRTYwMDM4NjE0RjEzOTRBQkZBM0I0Qzg1MEQ5MjdFMUU3NzY5QzhFRUMyRDE5") + PdfHexEncoder.DecodeString("MDM3QkYyNzM0MkRBNjM5QjZEQ0NGRkZFQjczRDY5RDc4QzZDMjdBNjAwOUNCQkNBMTk4MEY4NTMzOTIxRThBNjg0NDIzRTQzQkFCMDhBNTc2MjkxQUY4RjQ2MUJCMkE4QjM1MzFEMkYwNDg1QzE5QjE2RTJGMTUxNkUyM0REM0MxQTQ4MjdBRjFCOEFDMTVC")));
			return new ECX9Field(field2MCurves, decodedECPoint, number, number2, seed);
		}
	}

	internal static Dictionary<DerObjectID, ECX9Params> curves;

	internal static Dictionary<string, DerObjectID> objectIds;

	internal static Dictionary<DerObjectID, string> curveNames;

	private SECGCurves()
	{
	}

	private static Number DecodeHex(string hex)
	{
		return new Number(1, PdfHexEncoder.Decode(hex));
	}

	private static void DefineCurve(string name, DerObjectID oid, ECX9Params holder)
	{
		objectIds.Add(name, oid);
		curveNames.Add(oid, name);
		curves.Add(oid, holder);
	}

	static SECGCurves()
	{
		curves = new Dictionary<DerObjectID, ECX9Params>();
		objectIds = new Dictionary<string, DerObjectID>();
		curveNames = new Dictionary<DerObjectID, string>();
		DefineCurve("secp112r1", ECSecIDs.ECSECP112r1, ECSecp112R1.primeField);
		DefineCurve("secp112r2", ECSecIDs.ECSECP112r2, ECSecp112R2.primeField);
		DefineCurve("secp128r1", ECSecIDs.ECSECP128r1, ECSecp128R1.primeField);
		DefineCurve("secp128r2", ECSecIDs.ECSECP128r2, ECSecp128R2.primeField);
		DefineCurve("secp160k1", ECSecIDs.ECSECP160k1, ECSecp160K1.primeField);
		DefineCurve("secp160r1", ECSecIDs.ECSECP160r1, ECSecp160R1.primeField);
		DefineCurve("secp160r2", ECSecIDs.ECSECP160r2, ECSecp160R2.primeField);
		DefineCurve("secp192k1", ECSecIDs.ECSECP192k1, ECSecp192K1.primeField);
		DefineCurve("secp192r1", ECSecIDs.ECSECP192r1, ECSecp192R1.primeField);
		DefineCurve("secp224k1", ECSecIDs.ECSECP224k1, ECSecp244K1.primeField);
		DefineCurve("secp224r1", ECSecIDs.ECSECP224r1, ECSecp244R1.primeField);
		DefineCurve("secp256k1", ECSecIDs.ECSECP256k1, ECSecp256K1.primeField);
		DefineCurve("secp256r1", ECSecIDs.ECSECP256r1, ECSecp256R1.primeField);
		DefineCurve("secp384r1", ECSecIDs.ECSECP384r1, ECSecp364R1.primeField);
		DefineCurve("secp521r1", ECSecIDs.ECSECP521r1, ECSecp521R1.primeField);
		DefineCurve("sect113r1", ECSecIDs.ECSECG113r1, ECSecp113R1.primeField);
		DefineCurve("sect113r2", ECSecIDs.ECSECG113r2, ECSecp113R2.primeField);
		DefineCurve("sect131r1", ECSecIDs.ECSECG131r1, ECSecp131R1.primeField);
		DefineCurve("sect131r2", ECSecIDs.ECSECG131r2, ECSecp131R2.primeField);
		DefineCurve("sect163k1", ECSecIDs.ECSECG163k1, ECSecp163K1.primeField);
		DefineCurve("sect163r1", ECSecIDs.ECSECG163r1, ECSecp163R1.primeField);
		DefineCurve("sect163r2", ECSecIDs.ECSECG163r2, ECSecp163R2.primeField);
		DefineCurve("sect193r1", ECSecIDs.ECSECG193r1, ECSecp193R1.primeField);
		DefineCurve("sect193r2", ECSecIDs.ECSECG193r2, ECSecp193R2.primeField);
		DefineCurve("sect233k1", ECSecIDs.ECSECG233k1, ECSect233K1.primeField);
		DefineCurve("sect233r1", ECSecIDs.ECSECG233r1, ECSect233R1.primeField);
		DefineCurve("sect239k1", ECSecIDs.ECSECG239k1, ECSect239K1.primeField);
		DefineCurve("sect283k1", ECSecIDs.ECSECG283k1, ECSect283K1.primeField);
		DefineCurve("sect283r1", ECSecIDs.ECSECG283r1, ECSect283R1.primeField);
		DefineCurve("sect409k1", ECSecIDs.ECSECG409k1, ECSect409K1.primeField);
		DefineCurve("sect409r1", ECSecIDs.ECSECG409r1, ECSect409R1.primeField);
		DefineCurve("sect571k1", ECSecIDs.ECSECG571k1, ECSect571K1.primeField);
		DefineCurve("sect571r1", ECSecIDs.ECSECG571r1, ECSect571R1.primeField);
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
