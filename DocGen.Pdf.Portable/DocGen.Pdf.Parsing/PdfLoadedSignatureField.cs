using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;
using DocGen.Pdf.Security;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedSignatureField : PdfLoadedStyledField
{
	private PdfSignature m_signature;

	private PdfCmsSigner m_pdfCmsSigner;

	private bool m_isSigned;

	private string m_message = string.Empty;

	private List<long> m_skipedObjects;

	private PdfLoadedSignatureItemCollection m_items;

	private PdfSignatureValidationResult result;

	private PdfSignatureValidationOptions signatureOptions;

	private bool m_isVerified;

	private int m_revision = -1;

	private bool m_isAcroFormDic;

	private bool isComment;

	public bool IsSigned
	{
		get
		{
			if (!m_isSigned)
			{
				CheckSigned();
			}
			return m_isSigned;
		}
	}

	public PdfLoadedSignatureItemCollection Items
	{
		get
		{
			return m_items;
		}
		internal set
		{
			m_items = value;
		}
	}

	internal PdfCmsSigner CmsSigner
	{
		get
		{
			if (m_pdfCmsSigner == null)
			{
				InitializeSigner();
			}
			return m_pdfCmsSigner;
		}
	}

	public PdfSignature Signature
	{
		get
		{
			if (m_signature == null)
			{
				if (base.Dictionary.ContainsKey("V"))
				{
					SetSignature(base.Dictionary["V"]);
				}
				if (base.Dictionary.ContainsKey("Lock"))
				{
					SetLock(base.Dictionary["Lock"]);
				}
			}
			return m_signature;
		}
		set
		{
			m_signature = value;
			base.Changed = true;
			NotifyPropertyChanged("Signature");
		}
	}

	internal PdfAppearance Appearance
	{
		get
		{
			if (Page != null && base.Widget != null && base.Widget.Page == null && base.Widget.LoadedPage == null)
			{
				base.Widget.SetPage(Page);
			}
			if (base.Widget != null)
			{
				return base.Widget.Appearance;
			}
			return null;
		}
	}

	public int Revision
	{
		get
		{
			if (IsSigned && m_revision == -1)
			{
				m_revision = SignedRevision();
			}
			return m_revision;
		}
	}

	internal PdfLoadedSignatureField(PdfDictionary dictionary, PdfCrossTable crossTable)
		: base(dictionary, crossTable)
	{
		PdfArray kids = base.Kids;
		m_items = new PdfLoadedSignatureItemCollection();
		if (kids == null || kids.Count <= 1)
		{
			return;
		}
		for (int i = 0; i < kids.Count; i++)
		{
			if (crossTable.GetObject(kids[i]) is PdfDictionary dictionary2)
			{
				PdfLoadedSignatureItem item = new PdfLoadedSignatureItem(this, i, dictionary2);
				m_items.Add(item);
			}
		}
	}

	private void CheckSigned()
	{
		try
		{
			if (base.Dictionary != null && base.Dictionary.ContainsKey("V") && PdfCrossTable.Dereference(base.Dictionary["V"]) is PdfDictionary)
			{
				m_isSigned = true;
			}
		}
		catch
		{
			m_message = "There are errors in the formatting or information contained in the signature";
		}
	}

	private void SetLock(IPdfPrimitive primitive)
	{
		if (m_signature != null)
		{
			PdfDictionary pdfDictionary = null;
			if (primitive is PdfReferenceHolder)
			{
				pdfDictionary = (primitive as PdfReferenceHolder).Object as PdfDictionary;
			}
			else if (primitive is PdfDictionary)
			{
				pdfDictionary = primitive as PdfDictionary;
			}
			if (pdfDictionary != null)
			{
				m_signature.m_lock = true;
			}
		}
	}

	private void SetSignature(IPdfPrimitive signature)
	{
		PdfReferenceHolder pdfReferenceHolder = signature as PdfReferenceHolder;
		if (!(pdfReferenceHolder != null) || !(pdfReferenceHolder.Object is PdfDictionary pdfDictionary))
		{
			return;
		}
		m_signature = new PdfSignature();
		m_signature.m_signed = true;
		m_signature.Settings.SignatureField = this;
		m_signature.m_document = base.CrossTable.Document;
		string text = string.Empty;
		if (pdfDictionary.ContainsKey("SubFilter"))
		{
			PdfName pdfName = PdfCrossTable.Dereference(pdfDictionary["SubFilter"]) as PdfName;
			if (pdfName != null)
			{
				text = pdfName.Value;
			}
			if (text == "ETSI.CAdES.detached")
			{
				m_signature.Settings.CryptographicStandard = CryptographicStandard.CADES;
			}
		}
		if (base.CrossTable.Document is PdfDocument)
		{
			if (pdfDictionary.ContainsKey("Reference") && pdfDictionary["Reference"] is PdfArray pdfArray && pdfArray.Elements[0] is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("Data"))
			{
				PdfMainObjectCollection pdfObjects = base.CrossTable.Document.PdfObjects;
				PdfReferenceHolder pdfReferenceHolder2 = pdfDictionary2["Data"] as PdfReferenceHolder;
				if (pdfReferenceHolder2 != null && !pdfObjects.ContainsReference(pdfReferenceHolder2.Reference))
				{
					pdfReferenceHolder2 = new PdfReferenceHolder(pdfObjects.GetObject(pdfReferenceHolder2.Reference.ObjectCollectionIndex));
					pdfDictionary2.SetProperty("Data", pdfReferenceHolder2);
				}
			}
			pdfDictionary.Remove("ByteRange");
			new PdfSignatureDictionary(base.CrossTable.Document, pdfDictionary);
			base.Dictionary.Remove("Contents");
			base.Dictionary.Remove("ByteRange");
		}
		else if (base.CrossTable.Document is PdfLoadedDocument && m_signature != null && m_signature.Certificate == null && pdfDictionary.ContainsKey("Contents"))
		{
			byte[] array = null;
			if (PdfCrossTable.Dereference(pdfDictionary["Contents"]) is PdfString pdfString)
			{
				array = pdfString.Bytes;
				m_signature.SignatureContentBytes = array;
			}
			if (array != null)
			{
				X509Certificate2 x509Certificate = null;
				try
				{
					List<DocGen.Pdf.Security.X509Certificate> list = new X509CertificateParser().ReadCertificates(array) as List<DocGen.Pdf.Security.X509Certificate>;
					List<X509Certificate2> list2 = new List<X509Certificate2>();
					for (int num = list.Count - 1; num >= 0; num--)
					{
						list2.Add(new X509Certificate2(list[num].CertificateStructure.GetDerEncoded()));
					}
					x509Certificate = ((list2.Count == 0) ? new X509Certificate2(array) : ((list2.Count != 1) ? FindEndChainCertificate(list2) : list2[0]));
				}
				catch (Exception)
				{
					PdfCmsSigner cmsSigner = CmsSigner;
					if (cmsSigner != null && cmsSigner.SignerCertificate != null)
					{
						m_signature.Certificate = new PdfCertificate(cmsSigner);
					}
					else
					{
						x509Certificate = null;
					}
				}
				if (x509Certificate != null)
				{
					PdfCertificate certificate = new PdfCertificate(x509Certificate, buildChain: false);
					m_signature.Certificate = certificate;
				}
			}
		}
		if (pdfDictionary.ContainsKey("M") && pdfDictionary["M"] is PdfString)
		{
			m_signature.m_signedDate = base.Dictionary.GetDateTime(pdfDictionary["M"] as PdfString);
		}
		if (pdfDictionary.ContainsKey("Name") && pdfDictionary["Name"] is PdfString && PdfCrossTable.Dereference(pdfDictionary["Name"]) is PdfString pdfString2)
		{
			m_signature.m_signedName = pdfString2.Value;
		}
		if (pdfDictionary.ContainsKey("Reason") && PdfCrossTable.Dereference(pdfDictionary["Reason"]) is PdfString pdfString3)
		{
			m_signature.Reason = pdfString3.Value;
		}
		if (pdfDictionary.ContainsKey("Location") && PdfCrossTable.Dereference(pdfDictionary["Location"]) is PdfString pdfString4)
		{
			m_signature.LocationInfo = pdfString4.Value;
		}
		if (pdfDictionary.ContainsKey("ContactInfo") && PdfCrossTable.Dereference(pdfDictionary["ContactInfo"]) is PdfString pdfString5)
		{
			m_signature.ContactInfo = pdfString5.Value;
		}
		if (!pdfDictionary.ContainsKey("ByteRange"))
		{
			return;
		}
		m_signature.ByteRange = pdfDictionary["ByteRange"] as PdfArray;
		if (base.CrossTable.DocumentCatalog != null)
		{
			PdfDictionary documentCatalog = base.CrossTable.DocumentCatalog;
			bool flag = false;
			if (documentCatalog.ContainsKey("Perms"))
			{
				IPdfPrimitive pdfPrimitive = documentCatalog["Perms"];
				if (((pdfPrimitive is PdfReferenceHolder) ? (pdfPrimitive as PdfReferenceHolder).Object : pdfPrimitive) is PdfDictionary pdfDictionary3 && pdfDictionary3.ContainsKey("DocMDP"))
				{
					IPdfPrimitive pdfPrimitive2 = pdfDictionary3["DocMDP"];
					if (((pdfPrimitive2 is PdfReferenceHolder) ? (pdfPrimitive2 as PdfReferenceHolder).Object : pdfPrimitive2) is PdfDictionary pdfDictionary4 && pdfDictionary4.ContainsKey("ByteRange"))
					{
						PdfArray pdfArray2 = PdfCrossTable.Dereference(pdfDictionary4["ByteRange"]) as PdfArray;
						bool flag2 = true;
						if (pdfArray2 != null && m_signature != null && m_signature.ByteRange != null)
						{
							for (int i = 0; i < pdfArray2.Count; i++)
							{
								PdfNumber pdfNumber = pdfArray2[i] as PdfNumber;
								PdfNumber pdfNumber2 = m_signature.ByteRange[i] as PdfNumber;
								if (pdfNumber != null && pdfNumber2 != null && pdfNumber.LongValue != pdfNumber2.LongValue)
								{
									flag2 = false;
									break;
								}
							}
						}
						flag = flag2;
					}
				}
			}
			if (flag && pdfDictionary.ContainsKey("Reference"))
			{
				IPdfPrimitive pdfPrimitive3 = pdfDictionary["Reference"];
				if (pdfPrimitive3 is PdfArray)
				{
					pdfPrimitive3 = (pdfPrimitive3 as PdfArray).Elements[0];
				}
				PdfDictionary pdfDictionary5 = null;
				if (((pdfPrimitive3 is PdfReferenceHolder) ? (pdfPrimitive3 as PdfReferenceHolder).Object : pdfPrimitive3) is PdfDictionary pdfDictionary6 && pdfDictionary6.ContainsKey("TransformParams"))
				{
					pdfPrimitive3 = pdfDictionary6["TransformParams"];
					if (((pdfPrimitive3 is PdfReferenceHolder) ? (pdfPrimitive3 as PdfReferenceHolder).Object : pdfPrimitive3) is PdfDictionary pdfDictionary7 && pdfDictionary7.ContainsKey("P"))
					{
						m_signature.HasDocumentPermission = true;
						if (PdfCrossTable.Dereference(pdfDictionary7["P"]) is PdfNumber pdfNumber3)
						{
							m_signature.DocumentPermissions = (PdfCertificationFlags)pdfNumber3.IntValue;
						}
					}
					else
					{
						m_signature.HasDocumentPermission = false;
					}
				}
				else
				{
					m_signature.HasDocumentPermission = false;
				}
			}
			else
			{
				m_signature.HasDocumentPermission = false;
			}
		}
		else
		{
			m_signature.HasDocumentPermission = false;
		}
	}

	private X509Certificate2 FindEndChainCertificate(List<X509Certificate2> chainElements)
	{
		X509Certificate2 x509Certificate = null;
		Dictionary<string, X509Certificate2> dictionary = new Dictionary<string, X509Certificate2>();
		Dictionary<string, X509Certificate2> dictionary2 = new Dictionary<string, X509Certificate2>();
		foreach (X509Certificate2 chainElement in chainElements)
		{
			dictionary[chainElement.SubjectName.Name] = chainElement;
			dictionary2[chainElement.IssuerName.Name] = chainElement;
		}
		foreach (string key in dictionary.Keys)
		{
			if (!dictionary2.ContainsKey(key))
			{
				x509Certificate = dictionary[key];
			}
		}
		dictionary.Clear();
		dictionary2.Clear();
		return x509Certificate;
	}

	private void InitializeSigner()
	{
		try
		{
			if (!base.Dictionary.ContainsKey("V"))
			{
				return;
			}
			PdfReferenceHolder pdfReferenceHolder = base.Dictionary["V"] as PdfReferenceHolder;
			if (!(pdfReferenceHolder != null) || !(pdfReferenceHolder.Object is PdfDictionary pdfDictionary))
			{
				return;
			}
			string text = string.Empty;
			if (pdfDictionary.ContainsKey("SubFilter"))
			{
				PdfName pdfName = PdfCrossTable.Dereference(pdfDictionary["SubFilter"]) as PdfName;
				if (pdfName != null)
				{
					text = pdfName.Value;
				}
			}
			if (!pdfDictionary.ContainsKey("Contents"))
			{
				return;
			}
			byte[] array = null;
			if (PdfCrossTable.Dereference(pdfDictionary["Contents"]) is PdfString pdfString)
			{
				array = pdfString.Bytes;
			}
			if (array == null)
			{
				return;
			}
			if (text == "adbe.x509.rsa_sha1" && pdfDictionary.ContainsKey("Cert"))
			{
				if (!(PdfCrossTable.Dereference(pdfDictionary["Cert"]) is PdfString pdfString2))
				{
					if (PdfCrossTable.Dereference(pdfDictionary["Cert"]) is PdfArray { Count: >0 } pdfArray && PdfCrossTable.Dereference(pdfArray[0]) is PdfString pdfString3)
					{
						m_pdfCmsSigner = new PdfCmsSigner(array, pdfString3.Bytes);
					}
				}
				else
				{
					m_pdfCmsSigner = new PdfCmsSigner(array, pdfString2.Bytes);
				}
			}
			else
			{
				m_pdfCmsSigner = new PdfCmsSigner(array, text, signatureOptions);
			}
			if (m_pdfCmsSigner != null && pdfDictionary.ContainsKey("Type"))
			{
				PdfName pdfName2 = PdfCrossTable.Dereference(pdfDictionary["Type"]) as PdfName;
				if (pdfName2 != null && pdfName2.Value == "DocTimeStamp")
				{
					m_pdfCmsSigner.m_timeStampDocument = true;
				}
			}
			PdfArray byteRange = pdfDictionary["ByteRange"] as PdfArray;
			UpdateByteRange(m_pdfCmsSigner, byteRange);
		}
		catch
		{
			m_message = "There are errors in the formatting or information contained in the signature";
		}
	}

	internal override void BeginSave()
	{
		base.BeginSave();
		if (m_signature != null && m_signature.Certificate != null && !m_signature.Certificated)
		{
			PdfSignatureDictionary wrapper = new PdfSignatureDictionary(base.CrossTable.Document, m_signature, m_signature.Certificate);
			base.Dictionary["V"] = new PdfReferenceHolder(wrapper);
		}
	}

	internal new PdfField Clone(PdfDictionary dictionary, PdfPage page)
	{
		PdfCrossTable crossTable = page.Section.ParentDocument.CrossTable;
		PdfLoadedSignatureField pdfLoadedSignatureField = new PdfLoadedSignatureField(dictionary, crossTable);
		pdfLoadedSignatureField.Page = page;
		pdfLoadedSignatureField.SetName(GetFieldName());
		pdfLoadedSignatureField.Widget.Dictionary = base.Widget.Dictionary.Clone(crossTable) as PdfDictionary;
		return pdfLoadedSignatureField;
	}

	internal override PdfLoadedFieldItem CreateLoadedItem(PdfDictionary dictionary)
	{
		return base.CreateLoadedItem(dictionary);
	}

	internal override void Draw()
	{
		if (base.Flatten)
		{
			FlattenSignatureApperance();
		}
	}

	private RectangleF CalculateTemplateBounds(RectangleF bounds, PdfPageBase page, PdfTemplate template, PdfGraphics graphics)
	{
		float x = bounds.X;
		float y = bounds.Y;
		float width = bounds.Width;
		float height = bounds.Height;
		if (page != null)
		{
			switch (ObtainGraphicsRotation(graphics.Matrix))
			{
			case 90:
				graphics.TranslateTransform(template.Height, 0f);
				graphics.RotateTransform(90f);
				x = bounds.X;
				y = 0f - (page.Size.Height - bounds.Y - bounds.Height);
				break;
			case 180:
				graphics.TranslateTransform(template.Width, template.Height);
				graphics.RotateTransform(180f);
				x = 0f - (page.Size.Width - (bounds.X + template.Width));
				y = 0f - (page.Size.Height - bounds.Y - template.Height);
				break;
			case 270:
				graphics.TranslateTransform(0f, template.Width);
				graphics.RotateTransform(270f);
				if (page.Rotation == PdfPageRotateAngle.RotateAngle270 && base.RotationAngle == 270 && !m_isCropBox)
				{
					x = 0f - (page.Size.Width - bounds.X - template.Width);
					y = bounds.Y - bounds.Width;
					width = bounds.Height;
					height = bounds.Width;
				}
				else
				{
					x = 0f - (page.Size.Width - bounds.X - bounds.Width);
					y = bounds.Y;
				}
				break;
			}
		}
		return new RectangleF(x, y, width, height);
	}

	internal int ObtainGraphicsRotation(PdfTransformationMatrix matrix)
	{
		int num = 0;
		num = (int)Math.Round(Math.Atan2(matrix.Matrix.Elements[2], matrix.Matrix.Elements[0]) * 180.0 / Math.PI);
		switch (num)
		{
		case -90:
			num = 90;
			break;
		case -180:
			num = 180;
			break;
		case 90:
			num = 270;
			break;
		}
		return num;
	}

	private bool CheckCertificateValidity(DateTime date, DateTime validFrom, DateTime validTo)
	{
		return date.CompareTo(validTo) < 0;
	}

	public PdfSignatureValidationResult ValidateSignature()
	{
		PdfSignatureValidationOptions options = new PdfSignatureValidationOptions();
		return ValidateSignature(options);
	}

	public PdfSignatureValidationResult ValidateSignature(PdfSignatureValidationOptions options)
	{
		if (!m_isVerified)
		{
			m_isVerified = true;
			result = Validate(options);
			if (result != null)
			{
				if (result.IsDocumentModified)
				{
					result.SignatureStatus = SignatureStatus.Invalid;
				}
				else
				{
					bool flag = result.IsValidAtCurrentTime || result.IsValidAtSignedTime || result.IsValidAtTimeStampTime;
					bool flag2 = false;
					RevocationResult revocationResult = result.RevocationResult;
					if (revocationResult != null && (revocationResult.OcspRevocationStatus == RevocationStatus.Good || revocationResult.OcspRevocationStatus == RevocationStatus.None) && !revocationResult.IsRevokedCRL)
					{
						flag2 = true;
					}
					if (flag2 && flag)
					{
						result.SignatureStatus = SignatureStatus.Unknown;
						result.IsSignatureValid = true;
					}
					else if (options != null && !options.ValidateRevocationStatus && flag)
					{
						result.SignatureStatus = SignatureStatus.Unknown;
						result.IsSignatureValid = true;
					}
					else
					{
						result.SignatureStatus = SignatureStatus.Invalid;
					}
				}
			}
			else if (m_message != string.Empty)
			{
				result = new PdfSignatureValidationResult();
				result.SignatureStatus = SignatureStatus.Invalid;
				result.SignatureValidationErrors.Add(new PdfSignatureValidationException(m_message));
			}
		}
		return result;
	}

	private List<byte[]> GetByteCollection(PdfArray array)
	{
		if (array != null)
		{
			List<byte[]> list = new List<byte[]>();
			for (int i = 0; i < array.Count; i++)
			{
				if (PdfCrossTable.Dereference(array[i]) is PdfStream pdfStream)
				{
					list.Add(pdfStream.GetDecompressedData());
				}
			}
			if (list.Count <= 0)
			{
				return null;
			}
			return list;
		}
		return null;
	}

	private PdfSignatureValidationResult Validate(PdfSignatureValidationOptions options)
	{
		signatureOptions = options;
		if (IsSigned && CmsSigner != null && Signature != null)
		{
			PdfSignatureValidationResult pdfSignatureValidationResult = new PdfSignatureValidationResult();
			pdfSignatureValidationResult.SignatureName = GetFieldName();
			if (m_signature.Settings != null)
			{
				pdfSignatureValidationResult.DigestAlgorithm = Signature.Settings.DigestAlgorithm;
				pdfSignatureValidationResult.CryptographicStandard = Signature.Settings.CryptographicStandard;
			}
			pdfSignatureValidationResult.IsCertificated = m_signature.Certificated;
			pdfSignatureValidationResult.SignatureAlgorithm = m_pdfCmsSigner.EncryptionAlgorithm;
			pdfSignatureValidationResult.Signer = CmsSigner;
			CmsSigner.Signature = m_signature;
			pdfSignatureValidationResult.Certificates = GetCertficates();
			if (!VerifyChecksum())
			{
				pdfSignatureValidationResult.IsDocumentModified = true;
				pdfSignatureValidationResult.SignatureValidationErrors.Add(new PdfSignatureValidationException("The document has been altered or corrupted since the signature was applied"));
				return pdfSignatureValidationResult;
			}
			if (CheckIncrementUpdate())
			{
				pdfSignatureValidationResult.IsDocumentModified = true;
				pdfSignatureValidationResult.SignatureValidationErrors.Add(new PdfSignatureValidationException("The document has been altered or corrupted since the signature was applied"));
				return pdfSignatureValidationResult;
			}
			pdfSignatureValidationResult.IsDocumentModified = false;
			try
			{
				if (options != null && options.ValidateRevocationStatus)
				{
					pdfSignatureValidationResult.signatureOptions = options;
					pdfSignatureValidationResult.RevocationResult = ValidateRevocation(pdfSignatureValidationResult);
				}
			}
			catch (Exception ex)
			{
				pdfSignatureValidationResult.SignatureValidationErrors.Add(new PdfSignatureValidationException(ex.Message));
				pdfSignatureValidationResult.RevocationResult = null;
			}
			if (pdfSignatureValidationResult.Certificates.Count > 0)
			{
				X509Certificate2 x509Certificate = pdfSignatureValidationResult.Certificates[0];
				DateTime notBefore = x509Certificate.NotBefore;
				DateTime notAfter = x509Certificate.NotAfter;
				if (CheckCertificateValidity(m_signature.SignedDate, notBefore, notAfter))
				{
					pdfSignatureValidationResult.IsValidAtSignedTime = true;
				}
				else if (!pdfSignatureValidationResult.m_isValidOCSPorCRLtimeValidation)
				{
					pdfSignatureValidationResult.SignatureValidationErrors.Add(new PdfSignatureValidationException("The signature is not valid at signing date. Signing time is from the clock on the signer's computer"));
				}
				if (CheckCertificateValidity(DateTime.Now, notBefore, notAfter))
				{
					pdfSignatureValidationResult.IsValidAtCurrentTime = true;
				}
				else if (!pdfSignatureValidationResult.m_isValidOCSPorCRLtimeValidation)
				{
					pdfSignatureValidationResult.SignatureValidationErrors.Add(new PdfSignatureValidationException("The signature is not valid at current date."));
				}
				pdfSignatureValidationResult.TimeStampInformation = VerifyTimeStamp();
				if (pdfSignatureValidationResult.TimeStampInformation != null && pdfSignatureValidationResult.TimeStampInformation.IsValid)
				{
					if (CheckCertificateValidity(pdfSignatureValidationResult.TimeStampInformation.Time, notBefore, notAfter))
					{
						pdfSignatureValidationResult.IsValidAtTimeStampTime = true;
					}
					else
					{
						pdfSignatureValidationResult.SignatureValidationErrors.Add(new PdfSignatureValidationException("The signature includes an embedded timestamp but it could not be verified."));
					}
				}
			}
			return pdfSignatureValidationResult;
		}
		return null;
	}

	public PdfSignatureValidationResult ValidateSignature(X509CertificateCollection rootCertificates)
	{
		PdfSignatureValidationOptions options = new PdfSignatureValidationOptions();
		return ValidateSignature(rootCertificates, options);
	}

	public PdfSignatureValidationResult ValidateSignature(X509CertificateCollection rootCertificates, PdfSignatureValidationOptions options)
	{
		CmsSigner.rootCertificates = rootCertificates;
		PdfSignatureValidationResult pdfSignatureValidationResult = ValidateSignature(options);
		if (pdfSignatureValidationResult != null)
		{
			List<DocGen.Pdf.Security.X509Certificate> list = new List<DocGen.Pdf.Security.X509Certificate>();
			X509CertificateParser x509CertificateParser = new X509CertificateParser();
			foreach (X509Certificate2 rootCertificate in rootCertificates)
			{
				list.Add(x509CertificateParser.ReadCertificate(rootCertificate.RawData));
			}
			if (m_pdfCmsSigner.ValidateCertificateWithCollection(list, Signature.SignedDate, pdfSignatureValidationResult) && pdfSignatureValidationResult.SignatureStatus == SignatureStatus.Unknown)
			{
				pdfSignatureValidationResult.SignatureStatus = SignatureStatus.Valid;
			}
		}
		return pdfSignatureValidationResult;
	}

	private X509Certificate2Collection GetCertficates()
	{
		if (m_pdfCmsSigner != null)
		{
			return m_pdfCmsSigner.GetCertificates();
		}
		return null;
	}

	private bool VerifyChecksum()
	{
		bool flag = false;
		if (m_pdfCmsSigner != null)
		{
			flag = m_pdfCmsSigner.ValidateChecksum();
		}
		return flag;
	}

	private bool CheckIncrementUpdate()
	{
		bool flag = false;
		if (base.CrossTable != null)
		{
			PdfDictionary trailer = base.CrossTable.Trailer;
			if (trailer != null && !trailer.ContainsKey("Prev"))
			{
				return false;
			}
			m_skipedObjects = new List<long>();
			Dictionary<long, List<ObjectInformation>> dictionary = null;
			if (base.CrossTable.CrossTable != null && base.CrossTable.CrossTable.AllTables != null)
			{
				dictionary = base.CrossTable.CrossTable.AllTables;
			}
			PdfArray pdfArray = null;
			if (Signature != null && Signature.ByteRange != null)
			{
				pdfArray = Signature.ByteRange;
			}
			long[] array = new long[4];
			if (pdfArray != null && pdfArray.Count > 3)
			{
				if (pdfArray[0] is PdfNumber pdfNumber)
				{
					array[0] = pdfNumber.LongValue;
				}
				if (pdfArray[1] is PdfNumber pdfNumber2)
				{
					array[1] = pdfNumber2.LongValue;
				}
				if (pdfArray[2] is PdfNumber pdfNumber3)
				{
					array[2] = pdfNumber3.LongValue;
				}
				if (pdfArray[3] is PdfNumber pdfNumber4)
				{
					array[3] = pdfNumber4.LongValue;
				}
			}
			List<long> list = new List<long>();
			if (base.CrossTable.m_pdfObjects == null)
			{
				base.CrossTable.m_pdfObjects = new List<long>();
				FindAllReferences(trailer);
			}
			PdfDictionary documentCatalog = base.CrossTable.DocumentCatalog;
			if (documentCatalog != null)
			{
				Dictionary<PdfName, IPdfPrimitive> items = documentCatalog.Items;
				PdfName key = new PdfName("AcroForm");
				if (items != null)
				{
					if (items.ContainsKey(key))
					{
						IPdfPrimitive pdfPrimitive = items[key];
						PdfReferenceHolder pdfReferenceHolder = pdfPrimitive as PdfReferenceHolder;
						if (pdfReferenceHolder != null && pdfReferenceHolder.Reference != null)
						{
							list.Add(pdfReferenceHolder.Reference.ObjNum);
							if (PdfCrossTable.Dereference(pdfPrimitive) is PdfDictionary formDictionary)
							{
								ReadAllReferences(formDictionary);
							}
						}
						else
						{
							m_isAcroFormDic = true;
							if (pdfPrimitive is PdfDictionary formDictionary2)
							{
								ReadAllReferences(formDictionary2);
							}
						}
					}
					key = new PdfName("DSS");
					if (items.ContainsKey(key))
					{
						IPdfPrimitive pdfPrimitive2 = items[key];
						PdfReferenceHolder pdfReferenceHolder2 = pdfPrimitive2 as PdfReferenceHolder;
						if (pdfReferenceHolder2 != null && pdfReferenceHolder2.Reference != null)
						{
							list.Add(pdfReferenceHolder2.Reference.ObjNum);
							if (PdfCrossTable.Dereference(pdfPrimitive2) is PdfDictionary formDictionary3)
							{
								ReadAllReferences(formDictionary3);
							}
						}
						else if (PdfCrossTable.Dereference(pdfPrimitive2) is PdfDictionary formDictionary4)
						{
							ReadAllReferences(formDictionary4);
						}
					}
				}
			}
			if (trailer != null && trailer.ContainsKey("Info"))
			{
				IPdfPrimitive primitive = trailer["Info"];
				ReadAllSubReferences(primitive);
			}
			foreach (long key2 in dictionary.Keys)
			{
				foreach (ObjectInformation item in dictionary[key2])
				{
					if (item.Archive != null)
					{
						_ = item.Offset;
						if (!base.CrossTable.m_pdfObjects.Contains(key2))
						{
							base.CrossTable.m_pdfObjects.Add(key2);
						}
					}
				}
			}
			Dictionary<PdfStream, long[]> dictionary2 = null;
			if (base.CrossTable != null && base.CrossTable.CrossTable != null)
			{
				dictionary2 = base.CrossTable.CrossTable.archiveIndices;
			}
			if (dictionary2 != null)
			{
				foreach (KeyValuePair<PdfStream, long[]> item2 in dictionary2)
				{
					long[] value = item2.Value;
					if (value == null || value.Length == 0)
					{
						continue;
					}
					for (int i = 0; i < value.Length; i += 2)
					{
						if (!base.CrossTable.m_pdfObjects.Contains(value[i]))
						{
							base.CrossTable.m_pdfObjects.Add(value[i]);
						}
					}
				}
			}
			foreach (long key3 in dictionary.Keys)
			{
				if (!base.CrossTable.m_pdfObjects.Contains(key3))
				{
					continue;
				}
				if (flag)
				{
					break;
				}
				List<ObjectInformation> list2 = dictionary[key3];
				Dictionary<long, ObjectInformation> dictionary3 = new Dictionary<long, ObjectInformation>();
				Dictionary<long, ObjectInformation> dictionary4 = new Dictionary<long, ObjectInformation>();
				foreach (ObjectInformation item3 in list2)
				{
					long num = ((item3.Archive == null) ? item3.Offset : item3.Archive.ArchiveNumber);
					if ((array[0] <= num && array[1] >= num) || (array[2] <= num && array[2] + array[3] >= num))
					{
						dictionary3[num] = item3;
					}
					else
					{
						dictionary4[num] = item3;
					}
				}
				if (Signature.HasDocumentPermission && Signature.DocumentPermissions == PdfCertificationFlags.ForbidChanges && dictionary4.Count > 0)
				{
					flag = true;
					break;
				}
				ObjectInformation objectInformation = list2[0];
				bool flag2 = false;
				bool flag3 = false;
				PdfDictionary pdfDictionary = null;
				PdfArray pdfArray2 = null;
				if (objectInformation == null)
				{
					continue;
				}
				PdfDictionary pdfDictionary2 = ReadDictionary(objectInformation);
				if (pdfDictionary2 == null)
				{
					continue;
				}
				if (pdfDictionary2.ContainsKey("Type"))
				{
					PdfName pdfName = pdfDictionary2["Type"] as PdfName;
					if (pdfName != null && pdfName.Value == "Catalog")
					{
						flag2 = true;
					}
				}
				if (pdfDictionary2 != null && pdfDictionary2.ContainsKey("Lock"))
				{
					flag3 = true;
				}
				if (!flag2 && !m_skipedObjects.Contains(key3))
				{
					if (list2.Count == dictionary3.Count)
					{
						flag = false;
						ObjectInformation objectInformation2 = null;
						using (Dictionary<long, ObjectInformation>.KeyCollection.Enumerator enumerator4 = dictionary3.Keys.GetEnumerator())
						{
							if (enumerator4.MoveNext())
							{
								long current5 = enumerator4.Current;
								objectInformation2 = dictionary3[current5];
							}
						}
						if (objectInformation2 != null)
						{
							PdfDictionary pdfDictionary3 = ReadDictionary(objectInformation2);
							if (pdfDictionary3 != null && pdfDictionary3.ContainsKey("Title") && !flag && IsSigned)
							{
								flag = CompareObjects(pdfDictionary3, list, key3, pdfDictionary2);
							}
						}
						continue;
					}
					if (list2.Count == dictionary4.Count)
					{
						if (pdfDictionary2.ContainsKey("Type"))
						{
							PdfName pdfName2 = pdfDictionary2["Type"] as PdfName;
							if (pdfName2 != null)
							{
								if (pdfName2.Value == "Sig" || pdfName2.Value == "DocTimeStamp")
								{
									flag = false;
								}
								else if (pdfName2.Value == "Page")
								{
									flag = (dictionary3.Count != 0 || list2.Count != 1 || list2[0].Archive == null) && (dictionary3.Count != 0 || list2.Count <= 1 || VerifyPageisModify(pdfDictionary2, list2));
								}
								else if (pdfName2.Value == "Annot")
								{
									flag = !CheckSubType(pdfDictionary2);
								}
							}
						}
						else if (pdfDictionary2.ContainsKey("Subtype"))
						{
							flag = !CheckSubType(pdfDictionary2);
						}
						else if (pdfDictionary2 is PdfStream)
						{
							flag = true;
						}
						continue;
					}
					ObjectInformation objectInformation3 = null;
					using (Dictionary<long, ObjectInformation>.KeyCollection.Enumerator enumerator4 = dictionary3.Keys.GetEnumerator())
					{
						if (enumerator4.MoveNext())
						{
							long current6 = enumerator4.Current;
							objectInformation3 = dictionary3[current6];
						}
					}
					if (objectInformation3 == null)
					{
						continue;
					}
					PdfDictionary pdfDictionary4 = ReadDictionary(objectInformation3);
					if (pdfDictionary4 == null)
					{
						continue;
					}
					if (pdfDictionary4.ContainsKey("Type"))
					{
						PdfName pdfName3 = pdfDictionary4["Type"] as PdfName;
						if (pdfName3 != null && pdfName3.Value == "Page" && pdfDictionary4.ContainsKey("Annots") && PdfCrossTable.Dereference(pdfDictionary4["Annots"]) is PdfArray oldAnnots && pdfDictionary2.ContainsKey("Annots") && PdfCrossTable.Dereference(pdfDictionary2["Annots"]) is PdfArray newAnnots)
						{
							flag = CheckFormFieldRemoved(oldAnnots, newAnnots);
						}
					}
					if (pdfDictionary4 != null && pdfDictionary4.ContainsKey("Names") && (PdfCrossTable.Dereference(pdfDictionary4["Names"]) as PdfArray).Elements[0] is PdfString pdfString && pdfString.Value.Contains("Comment"))
					{
						isComment = true;
					}
					if (!flag && !isComment)
					{
						flag = CompareObjects(pdfDictionary4, list, key3, pdfDictionary2);
					}
				}
				else
				{
					if (!flag3)
					{
						continue;
					}
					if (PdfCrossTable.Dereference(pdfDictionary2["Lock"]) is PdfDictionary pdfDictionary5 && pdfDictionary5.ContainsKey("Fields"))
					{
						pdfArray2 = PdfCrossTable.Dereference(pdfDictionary5["Fields"]) as PdfArray;
					}
					if (pdfArray2 == null)
					{
						PdfDictionary pdfDictionary6 = null;
						if (pdfDictionary2 != null && pdfDictionary2.ContainsKey("V") && PdfCrossTable.Dereference(pdfDictionary2["V"]) is PdfDictionary pdfDictionary7 && pdfDictionary7.ContainsKey("Reference"))
						{
							flag = true;
						}
					}
				}
			}
		}
		return flag;
	}

	private bool VerifyPageisModify(PdfDictionary dictionary, List<ObjectInformation> entry)
	{
		ObjectInformation objInfo = entry[1];
		PdfDictionary pdfDictionary = ReadDictionary(objInfo);
		ObjectInformation objInfo2 = entry[0];
		PdfDictionary pdfDictionary2 = ReadDictionary(objInfo2);
		bool hasDocumentPermission = Signature.HasDocumentPermission;
		if (hasDocumentPermission && Signature.DocumentPermissions == PdfCertificationFlags.ForbidChanges)
		{
			return true;
		}
		if (!hasDocumentPermission)
		{
			if (pdfDictionary2 != null && pdfDictionary != null && pdfDictionary2.ContainsKey("Contents") && pdfDictionary.ContainsKey("Contents"))
			{
				PdfReferenceHolder pdfReferenceHolder = pdfDictionary2["Contents"] as PdfReferenceHolder;
				PdfReferenceHolder pdfReferenceHolder2 = pdfDictionary["Contents"] as PdfReferenceHolder;
				if (pdfReferenceHolder != null && pdfReferenceHolder2 != null && pdfReferenceHolder.Reference != null && pdfReferenceHolder2.Reference != null && pdfReferenceHolder.Reference.ObjNum != pdfReferenceHolder2.Reference.ObjNum)
				{
					return true;
				}
			}
			if (pdfDictionary2 != null && pdfDictionary != null)
			{
				foreach (PdfName key in pdfDictionary2.Keys)
				{
					if (!pdfDictionary.ContainsKey(key))
					{
						if (key.Value != "Annots")
						{
							return true;
						}
						continue;
					}
					PdfReferenceHolder pdfReferenceHolder3 = pdfDictionary2[key] as PdfReferenceHolder;
					PdfReferenceHolder pdfReferenceHolder4 = pdfDictionary[key] as PdfReferenceHolder;
					if (pdfReferenceHolder3 != null && pdfReferenceHolder4 != null && pdfReferenceHolder3.Reference != null && pdfReferenceHolder4.Reference != null && pdfReferenceHolder3.Reference.ObjNum != pdfReferenceHolder4.Reference.ObjNum)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private PdfDictionary ReadDictionary(ObjectInformation objInfo)
	{
		PdfDictionary pdfDictionary = null;
		if (objInfo.Type == ObjectType.Normal)
		{
			if (objInfo.Obj != null)
			{
				pdfDictionary = objInfo.Obj as PdfDictionary;
			}
			else
			{
				PdfReader pdfReader = null;
				if (base.CrossTable != null && base.CrossTable.CrossTable != null)
				{
					pdfReader = base.CrossTable.CrossTable.Reader;
				}
				if (pdfReader != null)
				{
					long position = pdfReader.Position;
					pdfReader.Position = objInfo;
					PdfParser pdfParser = new PdfParser(base.CrossTable.CrossTable, pdfReader, base.CrossTable);
					for (int i = 0; i < 4; i++)
					{
						pdfParser.Advance();
					}
					if (pdfParser.GetNext() == DocGen.Pdf.IO.TokenType.DictionaryStart)
					{
						pdfDictionary = pdfParser.Dictionary() as PdfDictionary;
					}
					pdfReader.Position = position;
				}
			}
		}
		else if (objInfo.Type == ObjectType.Packed)
		{
			if (objInfo.Obj != null)
			{
				pdfDictionary = objInfo.Obj as PdfDictionary;
			}
			else if (objInfo.Archive != null && objInfo.Parser != null)
			{
				PdfParser parser = objInfo.Parser;
				long position2 = parser.Position;
				parser.StartFrom(objInfo.Offset);
				if (parser.GetNext() == DocGen.Pdf.IO.TokenType.DictionaryStart)
				{
					pdfDictionary = parser.Dictionary() as PdfDictionary;
				}
				parser.SetOffset(position2);
			}
		}
		return pdfDictionary;
	}

	private bool CheckFormFieldRemoved(PdfArray oldAnnots, PdfArray newAnnots)
	{
		bool flag = false;
		foreach (IPdfPrimitive element in oldAnnots.Elements)
		{
			if (!newAnnots.Contains(element))
			{
				PdfReferenceHolder pdfReferenceHolder = element as PdfReferenceHolder;
				if (pdfReferenceHolder != null && PdfCrossTable.Dereference(pdfReferenceHolder.Object) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("Subtype"))
				{
					PdfName pdfName = PdfCrossTable.Dereference(pdfDictionary["Subtype"]) as PdfName;
					if (pdfName != null && pdfName.Value == "Widget")
					{
						flag = true;
					}
					break;
				}
			}
			if ((element as PdfReferenceHolder).Object is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("Parent") && m_isAcroFormDic && !((pdfDictionary2["Parent"] as PdfReferenceHolder).Object as PdfDictionary).ContainsKey("SigFlags"))
			{
				flag = true;
			}
		}
		if (!flag)
		{
			foreach (IPdfPrimitive element2 in newAnnots.Elements)
			{
				if ((element2 as PdfReferenceHolder).Object is PdfDictionary pdfDictionary3 && pdfDictionary3.ContainsKey("Parent") && m_isAcroFormDic && !((pdfDictionary3["Parent"] as PdfReferenceHolder).Object as PdfDictionary).ContainsKey("SigFlags"))
				{
					flag = true;
				}
			}
		}
		return flag;
	}

	private bool CheckSubType(PdfDictionary dictionary)
	{
		bool flag = false;
		PdfName pdfName = dictionary["Subtype"] as PdfName;
		if (pdfName != null && pdfName.Value == "Widget" && dictionary.ContainsKey("FT"))
		{
			PdfName pdfName2 = PdfCrossTable.Dereference(dictionary["FT"]) as PdfName;
			flag = pdfName2 != null && pdfName2.Value == "Sig";
		}
		else if (pdfName != null && IsAnnotation(pdfName.Value) && (!Signature.HasDocumentPermission || (Signature.HasDocumentPermission && Signature.DocumentPermissions == PdfCertificationFlags.AllowComments)))
		{
			flag = true;
		}
		else if (pdfName != null && pdfName.Value == "Form")
		{
			flag = true;
		}
		return flag;
	}

	private bool CheckSubType(PdfDictionary newer, PdfDictionary older)
	{
		bool flag = false;
		PdfName pdfName = newer["Subtype"] as PdfName;
		if (pdfName != null && pdfName.Value == "Widget")
		{
			flag = true;
		}
		else if (pdfName != null && IsAnnotation(pdfName.Value))
		{
			flag = !Signature.HasDocumentPermission || (Signature.HasDocumentPermission && Signature.DocumentPermissions == PdfCertificationFlags.AllowComments) || AreEqual(newer, older, ignoreAnnotation: false);
		}
		return flag;
	}

	private bool CompareObjects(PdfDictionary dictionaryObj, List<long> skipObjects, long key, PdfDictionary dictionary)
	{
		bool flag = false;
		if (dictionaryObj != null)
		{
			flag = ((skipObjects.Count <= 0 || !skipObjects.Contains(key)) ? (!AreEqual(dictionaryObj, dictionary, ignoreAnnotation: true)) : ReadFormReferences(dictionaryObj, dictionary));
		}
		return flag;
	}

	private bool ReadFormReferences(PdfDictionary dictionaryObj, PdfDictionary dictionary)
	{
		bool flag = false;
		if (dictionaryObj.ContainsKey("Fields") && dictionary.ContainsKey("Fields"))
		{
			PdfArray pdfArray = dictionaryObj["Fields"] as PdfArray;
			PdfArray pdfArray2 = dictionary["Fields"] as PdfArray;
			if (pdfArray != null && pdfArray2 != null)
			{
				if (pdfArray.Count == pdfArray2.Count)
				{
					foreach (IPdfPrimitive item in pdfArray2)
					{
						if (!pdfArray.Contains(item))
						{
							flag = true;
							break;
						}
						PdfReferenceHolder pdfReferenceHolder = item as PdfReferenceHolder;
						if (pdfReferenceHolder != null && pdfReferenceHolder.Object is PdfDictionary formDictionary)
						{
							ReadAllReferences(formDictionary);
						}
					}
				}
				else if (pdfArray2.Count > pdfArray.Count)
				{
					foreach (IPdfPrimitive item2 in pdfArray2)
					{
						PdfReferenceHolder pdfReferenceHolder2 = item2 as PdfReferenceHolder;
						if (!(pdfReferenceHolder2 != null))
						{
							continue;
						}
						PdfDictionary pdfDictionary = pdfReferenceHolder2.Object as PdfDictionary;
						if (pdfDictionary != null && !pdfArray.Contains(item2) && pdfDictionary.ContainsKey("FT"))
						{
							PdfName pdfName = PdfCrossTable.Dereference(pdfDictionary["FT"]) as PdfName;
							if (pdfName != null && pdfName.Value != "Sig")
							{
								flag = true;
								break;
							}
						}
						if (!flag && pdfDictionary != null)
						{
							ReadAllReferences(pdfDictionary);
						}
					}
				}
				else
				{
					flag = true;
				}
			}
		}
		return flag;
	}

	private void ReadAllReferences(PdfDictionary formDictionary)
	{
		foreach (PdfName key in formDictionary.Keys)
		{
			if (key.Value != "P" && key.Value != "Parent")
			{
				IPdfPrimitive primitive = formDictionary[key];
				ReadAllSubReferences(primitive);
			}
		}
	}

	private void ReadAllSubReferences(IPdfPrimitive primitive)
	{
		if (primitive == null)
		{
			return;
		}
		if (primitive is PdfReferenceHolder)
		{
			PdfReferenceHolder pdfReferenceHolder = primitive as PdfReferenceHolder;
			if (pdfReferenceHolder != null && pdfReferenceHolder.Reference != null && !m_skipedObjects.Contains(pdfReferenceHolder.Reference.ObjNum))
			{
				m_skipedObjects.Add(pdfReferenceHolder.Reference.ObjNum);
				ReadAllSubReferences(pdfReferenceHolder.Object);
			}
		}
		else if (primitive is PdfDictionary)
		{
			ReadAllReferences(primitive as PdfDictionary);
		}
		else
		{
			if (!(primitive is PdfArray) || !(primitive is PdfArray { Count: >0 } pdfArray))
			{
				return;
			}
			foreach (IPdfPrimitive element in pdfArray.Elements)
			{
				ReadAllSubReferences(element);
			}
		}
	}

	private bool IsAnnotation(string subType)
	{
		bool flag = false;
		switch (subType)
		{
		case "Line":
		case "Link":
		case "Text":
		case "FreeText":
		case "PolyLine":
		case "Squiggly":
		case "Screen":
		case "Circle":
		case "Square":
		case "Polygon":
		case "TrapNet":
		case "Highlight":
		case "Underline":
		case "StrikeOut":
		case "Watermark":
		case "Stamp":
		case "Caret":
		case "Popup":
		case "Sound":
		case "Movie":
		case "Ink":
		case "U3D":
		case "FileAttachment":
		case "PrinterMark":
			flag = true;
			break;
		}
		return flag;
	}

	private bool AreEqual(PdfDictionary older, PdfDictionary newer, bool ignoreAnnotation)
	{
		bool flag = true;
		bool flag2 = false;
		if (newer != null && newer is PdfStream && newer.ContainsKey("Subtype"))
		{
			PdfName pdfName = PdfCrossTable.Dereference(newer["Subtype"]) as PdfName;
			if (pdfName != null && pdfName.Value == "XML")
			{
				flag2 = true;
			}
		}
		if (older != null && newer != null && !flag2)
		{
			PdfName pdfName2 = null;
			if (newer.ContainsKey("Type"))
			{
				pdfName2 = PdfCrossTable.Dereference(newer["Type"]) as PdfName;
			}
			if (ignoreAnnotation && pdfName2 != null && pdfName2.Value == "Annot")
			{
				flag = CheckSubType(newer, older);
			}
			else if (newer.ContainsKey("FT"))
			{
				PdfName pdfName3 = newer["FT"] as PdfName;
				if (pdfName3 != null)
				{
					flag = pdfName3.Value == "Sig" || pdfName3.Value == "Tx" || pdfName3.Value == "Btn" || pdfName3.Value == "Ch";
				}
			}
			else
			{
				foreach (PdfName key in older.Keys)
				{
					if (newer.ContainsKey(key))
					{
						if (key.Value != "Annots")
						{
							IPdfPrimitive older2 = older[key];
							IPdfPrimitive newer2 = newer[key];
							flag = IsEqual(older2, newer2);
						}
						if (!flag)
						{
							break;
						}
						continue;
					}
					flag = false;
					break;
				}
			}
		}
		return flag;
	}

	private bool IsEqual(IPdfPrimitive older, IPdfPrimitive newer)
	{
		bool flag = true;
		if (older is PdfName && newer is PdfName)
		{
			flag = (older as PdfName).Value == (newer as PdfName).Value;
		}
		else if (older is PdfString && newer is PdfString)
		{
			flag = (older as PdfString).Value == (newer as PdfString).Value;
		}
		else if (older is PdfBoolean && newer is PdfBoolean)
		{
			flag = (older as PdfBoolean).Value == (newer as PdfBoolean).Value;
		}
		else if (older is PdfReferenceHolder && newer is PdfReferenceHolder)
		{
			PdfReference reference = (older as PdfReferenceHolder).Reference;
			PdfReference reference2 = (newer as PdfReferenceHolder).Reference;
			flag = (reference == null && reference2 == null) || (reference != null && reference2 != null && reference.ObjNum == reference2.ObjNum);
		}
		else if (older is PdfArray && newer is PdfArray)
		{
			PdfArray pdfArray = older as PdfArray;
			PdfArray pdfArray2 = newer as PdfArray;
			if (pdfArray.Count == pdfArray2.Count)
			{
				for (int i = 0; i < pdfArray.Count; i++)
				{
					flag = IsEqual(pdfArray[i], pdfArray2[i]);
					if (!flag)
					{
						break;
					}
				}
			}
			else
			{
				flag = false;
			}
		}
		else if (!(older is PdfNumber) || !(newer is PdfNumber))
		{
			flag = ((older is PdfDictionary && newer is PdfDictionary) ? AreEqual(older as PdfDictionary, newer as PdfDictionary, ignoreAnnotation: true) : ((older is PdfNull && newer is PdfNull) ? true : false));
		}
		else
		{
			flag = (older as PdfNumber).FloatValue == (newer as PdfNumber).FloatValue;
			if (!flag)
			{
				double num = Math.Round((older as PdfNumber).FloatValue, 4);
				double num2 = Math.Round((newer as PdfNumber).FloatValue, 4);
				flag = num == num2;
			}
			flag &= (older as PdfNumber).IntValue == (newer as PdfNumber).IntValue;
		}
		return flag;
	}

	private RevocationResult ValidateRevocation(PdfSignatureValidationResult result)
	{
		if (m_pdfCmsSigner != null)
		{
			List<byte[]> crlByteCollection = null;
			List<byte[]> ocspByteCollection = null;
			if (m_signature.m_document != null && m_signature.m_document.Catalog != null && m_signature.m_document.Catalog.ContainsKey("DSS") && PdfCrossTable.Dereference(m_signature.m_document.Catalog["DSS"]) is PdfDictionary pdfDictionary)
			{
				if (pdfDictionary.ContainsKey("CRLs"))
				{
					PdfArray array = PdfCrossTable.Dereference(pdfDictionary["CRLs"]) as PdfArray;
					crlByteCollection = GetByteCollection(array);
				}
				if (pdfDictionary.ContainsKey("OCSPs"))
				{
					PdfArray array2 = PdfCrossTable.Dereference(pdfDictionary["OCSPs"]) as PdfArray;
					ocspByteCollection = GetByteCollection(array2);
				}
			}
			m_pdfCmsSigner.m_crlByteCollection = crlByteCollection;
			m_pdfCmsSigner.m_ocspByteCollection = ocspByteCollection;
			RevocationResult revocationResult = m_pdfCmsSigner.CheckRevocation(Signature.SignedDate, result);
			result.LtvVerificationInfo = m_pdfCmsSigner.m_ltvVerificationInfo;
			return revocationResult;
		}
		return null;
	}

	private TimeStampInformation VerifyTimeStamp()
	{
		if (m_signature == null)
		{
			m_signature = Signature;
		}
		if (m_pdfCmsSigner != null)
		{
			return m_pdfCmsSigner.ValidateTimestamp();
		}
		return null;
	}

	private void UpdateByteRange(PdfCmsSigner pkcs7, PdfArray byteRange)
	{
		if (!(base.CrossTable.Document is PdfLoadedDocument pdfLoadedDocument))
		{
			return;
		}
		Stream stream = pdfLoadedDocument.m_stream;
		byte[] array = new byte[stream.Length];
		stream.Position = 0L;
		stream.Read(array, 0, (int)stream.Length);
		IRandom source = new RandomArray(array);
		long[] array2 = new long[4];
		if (byteRange != null && byteRange.Count > 3)
		{
			if (byteRange[0] is PdfNumber pdfNumber)
			{
				array2[0] = pdfNumber.LongValue;
			}
			if (byteRange[1] is PdfNumber pdfNumber2)
			{
				array2[1] = pdfNumber2.LongValue;
			}
			if (byteRange[2] is PdfNumber pdfNumber3)
			{
				array2[2] = pdfNumber3.LongValue;
			}
			if (byteRange[3] is PdfNumber pdfNumber4)
			{
				array2[3] = pdfNumber4.LongValue;
			}
		}
		IRandom[] array3 = new IRandom[array2.Length / 2];
		for (int i = 0; i < array2.Length; i += 2)
		{
			array3[i / 2] = new WindowRandom(source, array2[i], array2[i + 1]);
		}
		RandomStream randomStream = null;
		try
		{
			randomStream = new RandomStream(new RandomGroup(array3));
			byte[] array4 = new byte[8192];
			int length;
			while ((length = randomStream.Read(array4, 0, array4.Length)) > 0)
			{
				pkcs7.Update(array4, 0, length);
			}
		}
		finally
		{
			if (randomStream != null)
			{
				randomStream = null;
			}
		}
	}

	private void FlattenSignatureApperance()
	{
		if (Items != null && Items.Count > 0)
		{
			int count = Items.Count;
			PdfTemplate pdfTemplate = null;
			for (int i = 0; i < count; i++)
			{
				PdfLoadedSignatureItem pdfLoadedSignatureItem = Items[i];
				PdfDictionary dictionary = pdfLoadedSignatureItem.Dictionary;
				PdfPageBase page = pdfLoadedSignatureItem.Page;
				if (dictionary != null && page != null)
				{
					if (pdfTemplate == null && i == 0)
					{
						pdfTemplate = GetItemTemplate(dictionary);
					}
					FlattenSignature(dictionary, page, pdfTemplate, pdfLoadedSignatureItem.Bounds);
				}
			}
			return;
		}
		PdfArray kids = base.Kids;
		if (kids != null)
		{
			for (int j = 0; j < kids.Count; j++)
			{
				PdfDictionary dictionary2 = (kids.Elements[j] as PdfReferenceHolder).Object as PdfDictionary;
				FlattenSignature(dictionary2, Page, null, base.Bounds);
			}
		}
		else
		{
			FlattenSignature(base.Dictionary, Page, null, base.Bounds);
		}
	}

	private PdfTemplate GetItemTemplate(PdfDictionary Dictionary)
	{
		if (Dictionary["AP"] != null && PdfCrossTable.Dereference(Dictionary["AP"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("N") && PdfCrossTable.Dereference(pdfDictionary["N"]) is PdfDictionary pdfDictionary2 && pdfDictionary2 is PdfStream template)
		{
			return new PdfTemplate(template);
		}
		return null;
	}

	private void FlattenSignature(PdfDictionary Dictionary, PdfPageBase Page, PdfTemplate signatureTemplate, RectangleF Bounds)
	{
		if (Dictionary["AP"] != null)
		{
			if (!(PdfCrossTable.Dereference(Dictionary["AP"]) is PdfDictionary pdfDictionary) || !pdfDictionary.ContainsKey("N") || !(PdfCrossTable.Dereference(pdfDictionary["N"]) is PdfDictionary pdfDictionary2) || !(pdfDictionary2 is PdfStream template))
			{
				return;
			}
			PdfTemplate template2 = ((signatureTemplate == null) ? new PdfTemplate(template) : signatureTemplate);
			if (Page != null)
			{
				PdfGraphics graphics = Page.Graphics;
				PdfGraphicsState state = graphics.Save();
				if (Page.Rotation != 0)
				{
					RectangleF rectangleF = CalculateTemplateBounds(Bounds, Page, template2, graphics);
					graphics.DrawPdfTemplate(template2, rectangleF.Location, rectangleF.Size);
				}
				else
				{
					graphics.DrawPdfTemplate(template2, Bounds.Location, Bounds.Size);
				}
				graphics.Restore(state);
			}
		}
		else
		{
			if (signatureTemplate == null)
			{
				return;
			}
			if (Page != null)
			{
				PdfGraphics graphics2 = Page.Graphics;
				PdfGraphicsState state2 = graphics2.Save();
				if (Page.Rotation != 0)
				{
					RectangleF rectangleF2 = CalculateTemplateBounds(Bounds, Page, signatureTemplate, graphics2);
					graphics2.DrawPdfTemplate(signatureTemplate, rectangleF2.Location, rectangleF2.Size);
				}
				else
				{
					graphics2.DrawPdfTemplate(signatureTemplate, Bounds.Location, Bounds.Size);
				}
				graphics2.Restore(state2);
			}
		}
	}

	private void FindAllReferences(PdfDictionary formDictionary)
	{
		foreach (PdfName key in formDictionary.Keys)
		{
			if (key.Value != "P" && key.Value != "Parent")
			{
				IPdfPrimitive primitive = formDictionary[key];
				FindAllSubReferences(primitive);
			}
		}
	}

	private void FindAllSubReferences(IPdfPrimitive primitive)
	{
		if (primitive == null)
		{
			return;
		}
		if (primitive is PdfReferenceHolder)
		{
			PdfReferenceHolder pdfReferenceHolder = primitive as PdfReferenceHolder;
			if (pdfReferenceHolder.Reference != null && !base.CrossTable.m_pdfObjects.Contains(pdfReferenceHolder.Reference.ObjNum))
			{
				base.CrossTable.m_pdfObjects.Add(pdfReferenceHolder.Reference.ObjNum);
				FindAllSubReferences(pdfReferenceHolder.Object);
			}
		}
		else if (primitive is PdfDictionary)
		{
			FindAllReferences(primitive as PdfDictionary);
		}
		else
		{
			if (!(primitive is PdfArray))
			{
				return;
			}
			PdfArray pdfArray = primitive as PdfArray;
			if (pdfArray.Count <= 0)
			{
				return;
			}
			foreach (IPdfPrimitive element in pdfArray.Elements)
			{
				FindAllSubReferences(element);
			}
		}
	}

	internal int SignedRevision()
	{
		PdfArray byteRange = Signature.ByteRange;
		long[] array = new long[4];
		if (byteRange != null && byteRange.Count > 3)
		{
			if (byteRange[0] is PdfNumber pdfNumber)
			{
				array[0] = pdfNumber.LongValue;
			}
			if (byteRange[1] is PdfNumber pdfNumber2)
			{
				array[1] = pdfNumber2.LongValue;
			}
			if (byteRange[2] is PdfNumber pdfNumber3)
			{
				array[2] = pdfNumber3.LongValue;
			}
			if (byteRange[3] is PdfNumber pdfNumber4)
			{
				array[3] = pdfNumber4.LongValue;
			}
		}
		long num = array[0] + array[1];
		long num2 = array[2] + array[3];
		int num3 = -1;
		if (Page is PdfLoadedPage { Document: PdfLoadedDocument document } && base.CrossTable != null && base.CrossTable.CrossTable != null)
		{
			_ = document.Revisions.Length;
			List<long> eofOffset = base.CrossTable.CrossTable.EofOffset;
			if (eofOffset != null)
			{
				for (int i = 0; i < eofOffset.Count; i++)
				{
					long num4 = eofOffset[i];
					if (num4 > num && num4 == num2)
					{
						num3 = i + 1;
						break;
					}
				}
			}
		}
		return num3;
	}
}
