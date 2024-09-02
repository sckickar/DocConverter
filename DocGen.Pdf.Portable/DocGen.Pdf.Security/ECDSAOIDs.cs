namespace DocGen.Pdf.Security;

internal abstract class ECDSAOIDs
{
	internal const string IDx962 = "1.2.840.10045";

	public static readonly DerObjectID X90IDx962_1 = new DerObjectID("1.2.840.10045");

	public static readonly DerObjectID X90FieldID = X90IDx962_1.Branch("1");

	public static readonly DerObjectID X90UniqueID = X90FieldID.Branch("1");

	public static readonly DerObjectID X90RecordID = X90FieldID.Branch("2");

	public static readonly DerObjectID X90TNObjID = X90RecordID.Branch("3.2");

	public static readonly DerObjectID X90PPObjID = X90RecordID.Branch("3.3");

	public static readonly DerObjectID X90SignType = X90IDx962_1.Branch("4");

	public static readonly DerObjectID ECDSAwithSHA1 = X90SignType.Branch("1");

	public static readonly DerObjectID X90KeyType = X90IDx962_1.Branch("2");

	public static readonly DerObjectID IdECPublicKey = X90KeyType.Branch("1");

	public static readonly DerObjectID ECDSAwithSHA2 = X90SignType.Branch("3");

	public static readonly DerObjectID ECDSAwithSHA224 = ECDSAwithSHA2.Branch("1");

	public static readonly DerObjectID ECDSAwithSHA256 = ECDSAwithSHA2.Branch("2");

	public static readonly DerObjectID ECDSAwithSHA384 = ECDSAwithSHA2.Branch("3");

	public static readonly DerObjectID ECDSAwithSHA512 = ECDSAwithSHA2.Branch("4");

	public static readonly DerObjectID EllipticCurve = X90IDx962_1.Branch("3");

	public static readonly DerObjectID Curves = EllipticCurve.Branch("0");

	public static readonly DerObjectID ECP163v1 = Curves.Branch("1");

	public static readonly DerObjectID ECP163v2 = Curves.Branch("2");

	public static readonly DerObjectID ECP163v3 = Curves.Branch("3");

	public static readonly DerObjectID ECP176w1 = Curves.Branch("4");

	public static readonly DerObjectID ECP191v1 = Curves.Branch("5");

	public static readonly DerObjectID ECP191v2 = Curves.Branch("6");

	public static readonly DerObjectID ECP191v3 = Curves.Branch("7");

	public static readonly DerObjectID ECP208w1 = Curves.Branch("10");

	public static readonly DerObjectID ECP239v1 = Curves.Branch("11");

	public static readonly DerObjectID ECP239v2 = Curves.Branch("12");

	public static readonly DerObjectID ECP239v3 = Curves.Branch("13");

	public static readonly DerObjectID ECP272w1 = Curves.Branch("16");

	public static readonly DerObjectID ECP304w1 = Curves.Branch("17");

	public static readonly DerObjectID ECP359v1 = Curves.Branch("18");

	public static readonly DerObjectID ECP368w1 = Curves.Branch("19");

	public static readonly DerObjectID ECP431r1 = Curves.Branch("20");

	public static readonly DerObjectID ECPC = EllipticCurve.Branch("1");

	public static readonly DerObjectID ECPC192v1 = ECPC.Branch("1");

	public static readonly DerObjectID ECPC192v2 = ECPC.Branch("2");

	public static readonly DerObjectID ECPC192v3 = ECPC.Branch("3");

	public static readonly DerObjectID ECPC239v1 = ECPC.Branch("4");

	public static readonly DerObjectID ECPC239v2 = ECPC.Branch("5");

	public static readonly DerObjectID ECPC239v3 = ECPC.Branch("6");

	public static readonly DerObjectID ECPC256v1 = ECPC.Branch("7");
}
