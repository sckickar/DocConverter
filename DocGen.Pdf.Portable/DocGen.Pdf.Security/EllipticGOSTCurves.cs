using System.Collections.Generic;

namespace DocGen.Pdf.Security;

internal sealed class EllipticGOSTCurves
{
	internal static Dictionary<DerObjectID, EllipticCurveParams> parameters;

	internal static Dictionary<string, DerObjectID> curveIds;

	internal static Dictionary<DerObjectID, string> curveNames;

	private EllipticGOSTCurves()
	{
	}

	static EllipticGOSTCurves()
	{
		parameters = new Dictionary<DerObjectID, EllipticCurveParams>();
		curveIds = new Dictionary<string, DerObjectID>();
		curveNames = new Dictionary<DerObjectID, string>();
		Number number = new Number(PdfHexEncoder.DecodeString("MTE1NzkyMDg5MjM3MzE2MTk1NDIzNTcwOTg1MDA4Njg3OTA3ODUzMjY5OTg0NjY1NjQwNTY0MDM5NDU3NTg0MDA3OTEzMTI5NjM5MzE5"));
		Number numberX = new Number(PdfHexEncoder.DecodeString("MTE1NzkyMDg5MjM3MzE2MTk1NDIzNTcwOTg1MDA4Njg3OTA3ODUzMDczNzYyOTA4NDk5MjQzMjI1Mzc4MTU1ODA1MDc5MDY4ODUwMzIz"));
		FiniteCurves finiteCurves = new FiniteCurves(number, new Number(PdfHexEncoder.DecodeString("MTE1NzkyMDg5MjM3MzE2MTk1NDIzNTcwOTg1MDA4Njg3OTA3ODUzMjY5OTg0NjY1NjQwNTY0MDM5NDU3NTg0MDA3OTEzMTI5NjM5MzE2")), new Number("166"));
		EllipticCurveParams value = new EllipticCurveParams(finiteCurves, finiteCurves.ECPoints(Number.One, new Number(PdfHexEncoder.DecodeString("NjQwMzM4ODExNDI5MjcyMDI2ODM2NDk4ODE0NTA0MzM0NzM5ODU5MzE3NjAyNjg4ODQ5NDEyODg4NTI3NDU4MDM5MDg4Nzg2Mzg2MTI=")), isCompress: false), numberX);
		parameters[CRYPTOIDs.IDR3410A] = value;
		Number number2 = new Number(PdfHexEncoder.DecodeString("MTE1NzkyMDg5MjM3MzE2MTk1NDIzNTcwOTg1MDA4Njg3OTA3ODUzMjY5OTg0NjY1NjQwNTY0MDM5NDU3NTg0MDA3OTEzMTI5NjM5MzE5"));
		numberX = new Number(PdfHexEncoder.DecodeString("MTE1NzkyMDg5MjM3MzE2MTk1NDIzNTcwOTg1MDA4Njg3OTA3ODUzMDczNzYyOTA4NDk5MjQzMjI1Mzc4MTU1ODA1MDc5MDY4ODUwMzIz"));
		finiteCurves = new FiniteCurves(number2, new Number(PdfHexEncoder.DecodeString("MTE1NzkyMDg5MjM3MzE2MTk1NDIzNTcwOTg1MDA4Njg3OTA3ODUzMjY5OTg0NjY1NjQwNTY0MDM5NDU3NTg0MDA3OTEzMTI5NjM5MzE2")), new Number("166"));
		value = new EllipticCurveParams(finiteCurves, finiteCurves.ECPoints(Number.One, new Number(PdfHexEncoder.DecodeString("NjQwMzM4ODExNDI5MjcyMDI2ODM2NDk4ODE0NTA0MzM0NzM5ODU5MzE3NjAyNjg4ODQ5NDEyODg4NTI3NDU4MDM5MDg4Nzg2Mzg2MTI=")), isCompress: false), numberX);
		parameters[CRYPTOIDs.IDR3410XA] = value;
		Number number3 = new Number(PdfHexEncoder.DecodeString("NTc4OTYwNDQ2MTg2NTgwOTc3MTE3ODU0OTI1MDQzNDM5NTM5MjY2MzQ5OTIzMzI4MjAyODIwMTk3Mjg3OTIwMDM5NTY1NjQ4MjMxOTM="));
		numberX = new Number(PdfHexEncoder.DecodeString("NTc4OTYwNDQ2MTg2NTgwOTc3MTE3ODU0OTI1MDQzNDM5NTM5MjcxMDIxMzMxNjAyNTU4MjY4MjAwNjg4NDQ0OTYwODc3MzIwNjY3MDM="));
		finiteCurves = new FiniteCurves(number3, new Number(PdfHexEncoder.DecodeString("NTc4OTYwNDQ2MTg2NTgwOTc3MTE3ODU0OTI1MDQzNDM5NTM5MjY2MzQ5OTIzMzI4MjAyODIwMTk3Mjg3OTIwMDM5NTY1NjQ4MjMxOTA=")), new Number(PdfHexEncoder.DecodeString("MjgwOTEwMTkzNTMwNTgwOTAwOTY5OTY5NzkwMDAzMDk1NjA3NTkxMjQzNjg1NTgwMTQ4NjU5NTc2NTU4NDI4NzIzOTczMDEyNjc1OTU=")));
		value = new EllipticCurveParams(finiteCurves, finiteCurves.ECPoints(Number.One, new Number(PdfHexEncoder.DecodeString("Mjg3OTI2NjU4MTQ4NTQ2MTEyOTY5OTIzNDc0NTgzODAyODQxMzUwMjg2MzY3NzgyMjkxMTMwMDU3NTYzMzQ3MzA5OTYzMDM4ODgxMjQ=")), isCompress: false), numberX);
		parameters[CRYPTOIDs.IDR3410B] = value;
		Number number4 = new Number(PdfHexEncoder.DecodeString("NzAzOTAwODUzNTIwODMzMDUxOTk1NDc3MTgwMTkwMTg0Mzc4NDEwNzk1MTY2MzAwNDUxODA0NzEyODQzNDY4NDM3MDU2MzM1MDI2MTk="));
		numberX = new Number(PdfHexEncoder.DecodeString("NzAzOTAwODUzNTIwODMzMDUxOTk1NDc3MTgwMTkwMTg0Mzc4NDA5MjA4ODI2NDcxNjQwODEwMzUzMjI2MDE0NTgzNTIyOTgzOTY2MDE="));
		finiteCurves = new FiniteCurves(number4, new Number(PdfHexEncoder.DecodeString("NzAzOTAwODUzNTIwODMzMDUxOTk1NDc3MTgwMTkwMTg0Mzc4NDEwNzk1MTY2MzAwNDUxODA0NzEyODQzNDY4NDM3MDU2MzM1MDI2MTY=")), new Number("32858"));
		value = new EllipticCurveParams(finiteCurves, finiteCurves.ECPoints(Number.Zero, new Number(PdfHexEncoder.DecodeString("Mjk4MTg4OTM5MTc3MzEyNDA3MzM0NzEyNzMyNDAzMTQ3Njk5MjcyNDA1NTA4MTIzODM2OTU2ODkxNDY0OTUyNjE2MDQ1NjU5OTAyNDc=")), isCompress: false), numberX);
		parameters[CRYPTOIDs.IDR3410XB] = value;
		Number number5 = new Number(PdfHexEncoder.DecodeString("NzAzOTAwODUzNTIwODMzMDUxOTk1NDc3MTgwMTkwMTg0Mzc4NDEwNzk1MTY2MzAwNDUxODA0NzEyODQzNDY4NDM3MDU2MzM1MDI2MTk="));
		numberX = new Number(PdfHexEncoder.DecodeString("NzAzOTAwODUzNTIwODMzMDUxOTk1NDc3MTgwMTkwMTg0Mzc4NDA5MjA4ODI2NDcxNjQwODEwMzUzMjI2MDE0NTgzNTIyOTgzOTY2MDE="));
		finiteCurves = new FiniteCurves(number5, new Number(PdfHexEncoder.DecodeString("NzAzOTAwODUzNTIwODMzMDUxOTk1NDc3MTgwMTkwMTg0Mzc4NDEwNzk1MTY2MzAwNDUxODA0NzEyODQzNDY4NDM3MDU2MzM1MDI2MTY=")), new Number("32858"));
		value = new EllipticCurveParams(finiteCurves, finiteCurves.ECPoints(Number.Zero, new Number(PdfHexEncoder.DecodeString("Mjk4MTg4OTM5MTc3MzEyNDA3MzM0NzEyNzMyNDAzMTQ3Njk5MjcyNDA1NTA4MTIzODM2OTU2ODkxNDY0OTUyNjE2MDQ1NjU5OTAyNDc=")), isCompress: false), numberX);
		parameters[CRYPTOIDs.IDR3410C] = value;
		curveIds["IDR3410-2001-CryptoPro-A"] = CRYPTOIDs.IDR3410A;
		curveIds["IDR3410-2001-CryptoPro-B"] = CRYPTOIDs.IDR3410B;
		curveIds["IDR3410-2001-CryptoPro-C"] = CRYPTOIDs.IDR3410C;
		curveIds["IDR3410-2001-CryptoPro-XchA"] = CRYPTOIDs.IDR3410XA;
		curveIds["IDR3410-2001-CryptoPro-XchB"] = CRYPTOIDs.IDR3410XB;
		curveNames[CRYPTOIDs.IDR3410A] = "IDR3410-2001-CryptoPro-A";
		curveNames[CRYPTOIDs.IDR3410B] = "IDR3410-2001-CryptoPro-B";
		curveNames[CRYPTOIDs.IDR3410C] = "IDR3410-2001-CryptoPro-C";
		curveNames[CRYPTOIDs.IDR3410XA] = "IDR3410-2001-CryptoPro-XchA";
		curveNames[CRYPTOIDs.IDR3410XB] = "IDR3410-2001-CryptoPro-XchB";
	}

	public static EllipticCurveParams GetByOid(DerObjectID oid)
	{
		if (parameters.ContainsKey(oid))
		{
			return parameters[oid];
		}
		return null;
	}
}
