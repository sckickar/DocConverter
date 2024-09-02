using System.Collections.Generic;

namespace DocGen.Pdf.Security;

internal sealed class ECNamedCurves
{
	internal static Dictionary<string, DerObjectID> curvesObjIds;

	internal static Dictionary<DerObjectID, string> curveNames;

	private ECNamedCurves()
	{
	}

	private static void CreateNamedCurves(string name, DerObjectID oid)
	{
		curvesObjIds.Add(name, oid);
		curveNames.Add(oid, name);
	}

	static ECNamedCurves()
	{
		curvesObjIds = new Dictionary<string, DerObjectID>();
		curveNames = new Dictionary<DerObjectID, string>();
		CreateNamedCurves("B-571", ECSecIDs.ECSECG571r1);
		CreateNamedCurves("B-409", ECSecIDs.ECSECG409r1);
		CreateNamedCurves("B-283", ECSecIDs.ECSECG283r1);
		CreateNamedCurves("B-233", ECSecIDs.ECSECG233r1);
		CreateNamedCurves("B-163", ECSecIDs.ECSECG163r2);
		CreateNamedCurves("K-571", ECSecIDs.ECSECG571k1);
		CreateNamedCurves("K-409", ECSecIDs.ECSECG409k1);
		CreateNamedCurves("K-283", ECSecIDs.ECSECG283k1);
		CreateNamedCurves("K-233", ECSecIDs.ECSECG233k1);
		CreateNamedCurves("K-163", ECSecIDs.ECSECG163k1);
		CreateNamedCurves("P-521", ECSecIDs.ECSECP521r1);
		CreateNamedCurves("P-384", ECSecIDs.ECSECP384r1);
		CreateNamedCurves("P-256", ECSecIDs.ECSECP256r1);
		CreateNamedCurves("P-224", ECSecIDs.ECSECP224r1);
		CreateNamedCurves("P-192", ECSecIDs.ECSECP192r1);
	}

	public static ECX9Field GetByOid(DerObjectID oid)
	{
		return SECGCurves.GetByOid(oid);
	}
}
