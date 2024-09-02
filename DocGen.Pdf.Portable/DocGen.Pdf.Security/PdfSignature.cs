using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Security;

public class PdfSignature
{
	internal PdfSignatureDictionary m_signatureDictionary;

	private PdfSignatureField m_field;

	private PdfLoadedSignatureField m_sigField;

	private PdfCertificate m_pdfCertificate;

	private string m_reason;

	private PdfPageBase m_page;

	private string m_location;

	private string m_contactInformation;

	private PdfArray m_byteRange;

	private bool m_certeficated;

	private PdfCertificationFlags m_documentPermission = PdfCertificationFlags.ForbidChanges;

	private TimeStampServer m_timeStampServer;

	private bool m_hasDocumentPermission;

	internal PdfDocumentBase m_document;

	private bool m_drawSignatureAppearance;

	private bool m_enabledValiadtionAppearance;

	private SizeF size;

	internal bool isTimeStampOnly;

	private PdfDictionary m_dssDictionary;

	private bool m_enableLTV;

	internal DateTime m_signedDate;

	internal string m_signedName;

	private PdfSignatureSettings m_signatureSettings = new PdfSignatureSettings();

	private IPdfExternalSigner m_externalSigner;

	private byte[] m_ocsp;

	private ICollection<byte[]> m_crlBytes;

	private bool m_isExternalOCSP;

	private List<X509Certificate2> m_externalRootCert;

	private List<X509Certificate> m_externalChain;

	private byte[] m_inputBytes;

	private Stream m_stream;

	private ManualResetEvent m_allDone = new ManualResetEvent(initialState: false);

	private uint m_estimatedSize;

	internal bool m_lock;

	internal bool m_signed;

	private byte[] m_signatureContentBytes;

	internal bool m_isPermissionUpdated;

	internal bool m_addCerts;

	public PdfSignatureSettings Settings => m_signatureSettings;

	[Obsolete("Please use Appearance instead")]
	public PdfAppearance Appearence
	{
		get
		{
			if (m_field != null)
			{
				return m_field.Appearance;
			}
			return m_sigField.Appearance;
		}
	}

	public PdfAppearance Appearance
	{
		get
		{
			if (m_field != null)
			{
				return m_field.Appearance;
			}
			return m_sigField.Appearance;
		}
	}

	public PointF Location
	{
		get
		{
			if (m_field != null)
			{
				return m_field.Location;
			}
			return m_sigField.Location;
		}
		set
		{
			if (m_field != null)
			{
				m_field.Location = value;
			}
			else
			{
				m_sigField.Location = value;
			}
		}
	}

	public RectangleF Bounds
	{
		get
		{
			if (m_field != null)
			{
				return m_field.Bounds;
			}
			return m_sigField.Bounds;
		}
		set
		{
			if (m_field != null)
			{
				m_field.Bounds = value;
				if (m_field.m_loadedStyleField != null)
				{
					m_field.m_loadedStyleField.Bounds = value;
				}
			}
			else
			{
				m_sigField.Bounds = value;
			}
		}
	}

	public string ContactInfo
	{
		get
		{
			return m_contactInformation;
		}
		set
		{
			m_contactInformation = value;
		}
	}

	internal PdfArray ByteRange
	{
		get
		{
			return m_byteRange;
		}
		set
		{
			m_byteRange = value;
		}
	}

	public string Reason
	{
		get
		{
			return m_reason;
		}
		set
		{
			m_reason = value;
		}
	}

	public string LocationInfo
	{
		get
		{
			return m_location;
		}
		set
		{
			m_location = value;
		}
	}

	public bool Certificated
	{
		get
		{
			if (!m_certeficated && m_document != null && m_document is PdfLoadedDocument && m_document.Catalog != null)
			{
				m_certeficated = CheckCertificated();
			}
			return m_certeficated;
		}
		set
		{
			if (CheckCertificated() && !RemoveUnusedDocMDP())
			{
				throw new ArgumentException("The document may contain at most one author signature!");
			}
			m_certeficated = value;
		}
	}

	public PdfCertificationFlags DocumentPermissions
	{
		get
		{
			return m_documentPermission;
		}
		set
		{
			m_documentPermission = value;
			m_isPermissionUpdated = true;
		}
	}

	internal bool HasDocumentPermission
	{
		get
		{
			return m_hasDocumentPermission;
		}
		set
		{
			m_hasDocumentPermission = value;
		}
	}

	public PdfCertificate Certificate
	{
		get
		{
			return m_pdfCertificate;
		}
		set
		{
			m_pdfCertificate = value;
		}
	}

	public bool EnableLtv
	{
		get
		{
			return m_enableLTV;
		}
		set
		{
			m_enableLTV = value;
			EnableLtvFromTimestamp();
			if (m_enableLTV && Certificate != null)
			{
				string key = "";
				List<X509Certificate> list = new List<X509Certificate>();
				if (Certificate.m_pkcs7Certificate != null)
				{
					foreach (string item in Certificate.m_pkcs7Certificate.KeyEnumerable)
					{
						if (Certificate.m_pkcs7Certificate.IsKey(item) && Certificate.m_pkcs7Certificate.GetKey(item).Key.IsPrivate)
						{
							key = item;
							break;
						}
					}
					X509Certificates[] certificateChain = Certificate.m_pkcs7Certificate.GetCertificateChain(key);
					for (int i = 0; i < certificateChain.Length; i++)
					{
						list.Add(certificateChain[i].Certificate);
					}
				}
				else if (Certificate.Chains != null)
				{
					for (int j = 0; j < Certificate.Chains.Length; j++)
					{
						list.Add(Certificate.Chains[j]);
					}
				}
				else if (Certificate.X509Certificate != null)
				{
					X509Chain x509Chain = new X509Chain();
					x509Chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
					x509Chain.Build(Certificate.X509Certificate);
					X509CertificateParser x509CertificateParser = new X509CertificateParser();
					Certificate.Chains = new X509Certificate[x509Chain.ChainElements.Count];
					for (int k = 0; k < x509Chain.ChainElements.Count; k++)
					{
						X509Certificate x509Certificate = x509CertificateParser.ReadCertificate(x509Chain.ChainElements[k].Certificate.RawData);
						if (x509Certificate != null)
						{
							Certificate.Chains[k] = x509Certificate;
							list.Add(x509Certificate);
						}
					}
				}
				if (ExternalSigner != null)
				{
					EnableExternalLTV();
				}
				else
				{
					m_enableLTV = GetLTVData(list, null, null);
				}
			}
			else if (ExternalSigner != null)
			{
				EnableExternalLTV();
			}
		}
	}

	public bool Visible
	{
		get
		{
			if (m_field != null)
			{
				size = m_field.Size;
			}
			else
			{
				size = m_sigField.Size;
			}
			if (size.Height == 0f && size.Width == 0f)
			{
				return false;
			}
			return true;
		}
	}

	public TimeStampServer TimeStampServer
	{
		get
		{
			return m_timeStampServer;
		}
		set
		{
			m_timeStampServer = value;
		}
	}

	internal PdfField Field
	{
		get
		{
			if (m_field == null)
			{
				return m_sigField;
			}
			return m_field;
		}
	}

	internal bool DrawFieldAppearance => m_drawSignatureAppearance;

	public bool EnableValidationAppearance
	{
		get
		{
			return m_enabledValiadtionAppearance;
		}
		set
		{
			m_enabledValiadtionAppearance = value;
			if (value)
			{
				Appearance.Normal.IsSignatureAppearanceValidation = value;
			}
		}
	}

	public DateTime SignedDate => m_signedDate;

	public string SignedName
	{
		get
		{
			return m_signedName;
		}
		set
		{
			m_signedName = value;
		}
	}

	internal bool IsLTVEnabled
	{
		get
		{
			if (m_enableLTV)
			{
				return m_enableLTV;
			}
			if (m_document != null && m_document.Catalog != null)
			{
				return m_document.Catalog.ContainsKey("DSS");
			}
			return false;
		}
	}

	internal byte[] OCSP => m_ocsp;

	internal ICollection<byte[]> CRLBytes => m_crlBytes;

	internal IPdfExternalSigner ExternalSigner => m_externalSigner;

	internal List<X509Certificate2> ExternalCertificates => m_externalRootCert;

	public uint EstimatedSignatureSize
	{
		internal get
		{
			return m_estimatedSize;
		}
		set
		{
			m_estimatedSize = value;
		}
	}

	public bool IsLocked
	{
		get
		{
			return m_lock;
		}
		set
		{
			m_lock = value;
		}
	}

	internal byte[] SignatureContentBytes
	{
		get
		{
			return m_signatureContentBytes;
		}
		set
		{
			m_signatureContentBytes = value;
		}
	}

	public event PdfSignatureEventHandler ComputeHash;

	internal event AsyncPdfSignatureEventHandler AsyncComputeHash;

	[Obsolete("Please use PdfSignature(PdfPage page, PdfCertificate cert, string signatureName)instead")]
	public PdfSignature()
	{
		m_drawSignatureAppearance = true;
	}

	public PdfSignature(PdfPage page, PdfCertificate cert, string signatureName)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		if (cert == null)
		{
			throw new ArgumentNullException("cert");
		}
		m_page = page;
		m_pdfCertificate = cert;
		CheckAnnotationElementsContainsSignature(page, signatureName);
		m_field = new PdfSignatureField(page, signatureName);
		PdfDocument pdfDocument = (PdfDocument)(m_document = page.Document);
		pdfDocument.Form.Fields.Add(m_field);
		pdfDocument.Form.SignatureFlags = SignatureFlags.SignaturesExists | SignatureFlags.AppendOnly;
		pdfDocument.Catalog.BeginSave += Catalog_BeginSave;
		m_field.Dictionary.BeginSave += Dictionary_BeginSave;
		if (!m_field.m_SkipKidsCertificate)
		{
			m_signatureDictionary = new PdfSignatureDictionary(pdfDocument, this, cert);
			pdfDocument.PdfObjects.Add(((IPdfWrapper)m_signatureDictionary).Element);
			if (!pdfDocument.CrossTable.IsMerging)
			{
				((IPdfWrapper)m_signatureDictionary).Element.Position = -1;
			}
			m_field.Widget.Dictionary.SetProperty("V", new PdfReferenceHolder(m_signatureDictionary));
			m_signatureDictionary.Archive = false;
		}
		m_field.Widget.Dictionary.SetProperty("Ff", new PdfNumber(0));
	}

	public PdfSignature(PdfPage page, string signatureName)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		m_page = page;
		CheckAnnotationElementsContainsSignature(page, signatureName);
		m_field = new PdfSignatureField(page, signatureName);
		isTimeStampOnly = true;
		PdfDocument pdfDocument = (PdfDocument)(m_document = page.Document);
		pdfDocument.Form.Fields.Add(m_field);
		pdfDocument.Form.SignatureFlags = SignatureFlags.SignaturesExists | SignatureFlags.AppendOnly;
		pdfDocument.Catalog.BeginSave += Catalog_BeginSave;
		m_field.Dictionary.BeginSave += Dictionary_BeginSave;
		if (!m_field.m_SkipKidsCertificate)
		{
			m_signatureDictionary = new PdfSignatureDictionary(pdfDocument, this);
			pdfDocument.PdfObjects.Add(((IPdfWrapper)m_signatureDictionary).Element);
			if (!pdfDocument.CrossTable.IsMerging)
			{
				((IPdfWrapper)m_signatureDictionary).Element.Position = -1;
			}
			m_field.Widget.Dictionary.SetProperty("V", new PdfReferenceHolder(m_signatureDictionary));
			m_signatureDictionary.Archive = false;
		}
		m_field.Widget.Dictionary.SetProperty("Ff", new PdfNumber(0));
	}

	public PdfSignature(PdfLoadedPage page, string signatureName)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		m_page = page;
		CheckAnnotationElementsContainsSignature(page, signatureName);
		m_field = new PdfSignatureField(page, signatureName);
		isTimeStampOnly = true;
		PdfLoadedDocument pdfLoadedDocument = (PdfLoadedDocument)(m_document = page.Document as PdfLoadedDocument);
		pdfLoadedDocument.Form.Fields.Add(m_field);
		pdfLoadedDocument.Form.SignatureFlags = SignatureFlags.SignaturesExists | SignatureFlags.AppendOnly;
		pdfLoadedDocument.Catalog.BeginSave += Catalog_BeginSave;
		m_field.Dictionary.BeginSave += Dictionary_BeginSave;
		if (!m_field.m_SkipKidsCertificate)
		{
			m_signatureDictionary = new PdfSignatureDictionary(pdfLoadedDocument, this);
			pdfLoadedDocument.PdfObjects.Add(((IPdfWrapper)m_signatureDictionary).Element);
			if (!pdfLoadedDocument.CrossTable.IsMerging)
			{
				((IPdfWrapper)m_signatureDictionary).Element.Position = -1;
			}
			m_field.Widget.Dictionary.SetProperty("V", new PdfReferenceHolder(m_signatureDictionary));
			m_signatureDictionary.Archive = false;
		}
		m_field.Widget.Dictionary.SetProperty("Ff", new PdfNumber(0));
	}

	public PdfSignature(PdfDocumentBase document, PdfPageBase page, PdfCertificate certificate, string signatureName)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		m_page = page;
		m_pdfCertificate = certificate;
		m_document = document;
		CheckAnnotationElementsContainsSignature(page, signatureName);
		m_field = new PdfSignatureField(page, signatureName);
		if (m_field.Dictionary.ContainsKey("Kids"))
		{
			PdfArray pdfArray = m_field.Dictionary["Kids"] as PdfArray;
			for (int i = 0; i < pdfArray.Count; i++)
			{
				if (PdfCrossTable.Dereference(pdfArray[i]) is PdfDictionary pdfDictionary)
				{
					pdfDictionary.BeginSave += Dictionary_BeginSave;
				}
			}
		}
		PdfForm pdfForm = document.ObtainForm();
		if (pdfForm is PdfLoadedForm pdfLoadedForm)
		{
			pdfLoadedForm.Fields.Add(m_field);
		}
		else
		{
			pdfForm.Fields.Add(m_field);
		}
		pdfForm.SignatureFlags = SignatureFlags.SignaturesExists | SignatureFlags.AppendOnly;
		document.Catalog.BeginSave += Catalog_BeginSave;
		m_field.Dictionary.BeginSave += Dictionary_BeginSave;
		if (!m_field.m_SkipKidsCertificate)
		{
			m_signatureDictionary = new PdfSignatureDictionary(document, this, certificate);
			document.PdfObjects.Add(((IPdfWrapper)m_signatureDictionary).Element);
			if (!document.CrossTable.IsMerging)
			{
				((IPdfWrapper)m_signatureDictionary).Element.Position = -1;
			}
			m_field.Dictionary.SetProperty("V", new PdfReferenceHolder(m_signatureDictionary));
			m_signatureDictionary.Archive = false;
		}
		m_field.Widget.Dictionary.SetProperty("Ff", new PdfNumber(0));
	}

	public PdfSignature(PdfDocumentBase document, PdfPageBase page, PdfCertificate certificate, string signatureName, PdfLoadedSignatureField loadedField)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		m_page = page;
		m_pdfCertificate = certificate;
		m_document = document;
		m_sigField = loadedField;
		PdfLoadedForm pdfLoadedForm = document.ObtainForm() as PdfLoadedForm;
		if (pdfLoadedForm != null && m_sigField.Form == null)
		{
			pdfLoadedForm.Fields.Add(m_sigField);
		}
		pdfLoadedForm.SignatureFlags = SignatureFlags.SignaturesExists | SignatureFlags.AppendOnly;
		document.Catalog.BeginSave += Catalog_BeginSave;
		m_sigField.Dictionary.BeginSave += Dictionary_BeginSave;
		m_signatureDictionary = new PdfSignatureDictionary(document, this, certificate);
		document.PdfObjects.Add(((IPdfWrapper)m_signatureDictionary).Element);
		if (!document.CrossTable.IsMerging)
		{
			((IPdfWrapper)m_signatureDictionary).Element.Position = -1;
		}
		m_sigField.Dictionary.SetProperty("V", new PdfReferenceHolder(m_signatureDictionary));
		m_sigField.Widget.Bounds = m_sigField.Bounds;
		m_sigField.Dictionary.SetProperty("Ff", new PdfNumber(0));
		m_signatureDictionary.Archive = false;
	}

	internal void SetValidationApperance()
	{
		if (EnableValidationAppearance)
		{
			float num = 0.3f;
			SizeF sizeF = Bounds.Size;
			PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 12f);
			float[] array = findScale(sizeF.Width, sizeF.Height);
			float height = sizeF.Height * num;
			if (m_page != null && (m_page.Rotation == PdfPageRotateAngle.RotateAngle90 || m_page.Rotation == PdfPageRotateAngle.RotateAngle270))
			{
				height = sizeF.Width * num;
			}
			PdfTemplate pdfTemplate = new PdfTemplate(sizeF, writeTransformation: false);
			pdfTemplate.CustomPdfTemplateName = "FRM";
			PdfTemplate pdfTemplate2 = new PdfTemplate(new SizeF(100f, 100f), writeTransformation: false);
			pdfTemplate2.CustomPdfTemplateName = "n0";
			pdfTemplate2.Clear("% DSBlank");
			PdfGraphicsState state = pdfTemplate.Graphics.Save();
			if (m_page != null && m_page.Rotation == PdfPageRotateAngle.RotateAngle90)
			{
				pdfTemplate.Graphics.RotateTransform(-90f);
				pdfTemplate.Graphics.DrawPdfTemplate(pdfTemplate2, new PointF(0f, 0f));
			}
			else
			{
				pdfTemplate.Graphics.DrawPdfTemplate(pdfTemplate2, new PointF(0f, -100f));
			}
			pdfTemplate.Graphics.Restore(state);
			PdfTemplate pdfTemplate3 = new PdfTemplate(new SizeF(100f, 100f), writeTransformation: false);
			pdfTemplate3.CustomPdfTemplateName = "n1";
			pdfTemplate3.Clear("% DSUnkown\n");
			string s = "cQoxIEcKMSBnCjAuMSAwIDAgMC4xIDkgMCBjbQowIEogMCBqIDQgTSBbXTAgZAoxIGkKMCBnCjMxMyAyOTIgbQozMTMgNDA0IDMyNSA0NTMgNDMyIDUyOSBjCjQ3OCA1NjEgNTA0IDU5NyA1MDQgNjQ1IGMKNTA0IDczNiA0NDAgNzYwIDM5MSA3NjAgYwoyODYgNzYwIDI3MSA2ODEgMjY1IDYyNiBjCjI2NSA2MjUgbAoxMDAgNjI1IGwKMTAwIDgyOCAyNTMgODk4IDM4MSA4OTggYwo0NTEgODk4IDY3OSA4NzggNjc5IDY1MCBjCjY3OSA1NTUgNjI4IDQ5OSA1MzggNDM1IGMKNDg4IDM5OSA0NjcgMzc2IDQ2NyAyOTIgYwozMTMgMjkyIGwKaAozMDggMjE0IDE3MCAtMTY0IHJlCmYKMC40NCBHCjEuMiB3CjEgMSAwIHJnCjI4NyAzMTggbQoyODcgNDMwIDI5OSA0NzkgNDA2IDU1NSBjCjQ1MSA1ODcgNDc4IDYyMyA0NzggNjcxIGMKNDc4IDc2MiA0MTQgNzg2IDM2NSA3ODYgYwoyNjAgNzg2IDI0NSA3MDcgMjM5IDY1MiBjCjIzOSA2NTEgbAo3NCA2NTEgbAo3NCA4NTQgMjI3IDkyNCAzNTUgOTI0IGMKNDI1IDkyNCA2NTMgOTA0IDY1MyA2NzYgYwo2NTMgNTgxIDYwMiA1MjUgNTEyIDQ2MSBjCjQ2MiA0MjUgNDQxIDQwMiA0NDEgMzE4IGMKMjg3IDMxOCBsCmgKMjgyIDI0MCAxNzAgLTE2NCByZQpCClE=";
			pdfTemplate3.Graphics.StreamWriter.Write(Convert.FromBase64String(s));
			PdfGraphicsState state2 = pdfTemplate.Graphics.Save();
			float num2 = Math.Min(sizeF.Width, sizeF.Height);
			if (m_page != null && (m_page.Rotation == PdfPageRotateAngle.RotateAngle90 || m_page.Rotation == PdfPageRotateAngle.RotateAngle270))
			{
				pdfTemplate.Graphics.RotateTransform(-90f);
				pdfTemplate.Graphics.DrawPdfTemplate(pdfTemplate3, (sizeF.Width > sizeF.Height) ? new PointF(0f, (Math.Max(sizeF.Width, sizeF.Height) - num2) / 2f) : new PointF((Math.Max(sizeF.Width, sizeF.Height) - num2) / 2f, 0f), new SizeF(num2, num2));
			}
			else
			{
				pdfTemplate.Graphics.DrawPdfTemplate(pdfTemplate3, (sizeF.Width > sizeF.Height) ? new PointF((sizeF.Width - num2) / 2f, 0f - sizeF.Height) : new PointF(sizeF.Width - num2, (0f - sizeF.Height - num2) / 2f), new SizeF(num2, num2));
			}
			pdfTemplate.Graphics.Restore(state2);
			PdfGraphicsState state3 = pdfTemplate.Graphics.Save();
			if (m_page != null && (m_page.Rotation == PdfPageRotateAngle.RotateAngle90 || m_page.Rotation == PdfPageRotateAngle.RotateAngle270))
			{
				pdfTemplate.Graphics.RotateTransform(-90f);
				pdfTemplate.Graphics.DrawPdfTemplate(Appearance.AppearanceLayer, new PointF(0f, sizeF.Width / 4f));
			}
			else
			{
				pdfTemplate.Graphics.DrawPdfTemplate(Appearance.AppearanceLayer, new PointF(0f, 0f - (sizeF.Height - sizeF.Height / 4f)));
			}
			pdfTemplate.Graphics.Restore(state3);
			PdfTemplate pdfTemplate4 = new PdfTemplate(new SizeF(100f, 100f), writeTransformation: false);
			pdfTemplate4.CustomPdfTemplateName = "n3";
			pdfTemplate4.Clear("% DSBlank\n");
			PdfGraphicsState state4 = pdfTemplate.Graphics.Save();
			pdfTemplate.Graphics.ScaleTransform(array[0], array[0]);
			if (m_page != null && (m_page.Rotation == PdfPageRotateAngle.RotateAngle90 || m_page.Rotation == PdfPageRotateAngle.RotateAngle270))
			{
				pdfTemplate.Graphics.RotateTransform(-90f);
				pdfTemplate.Graphics.DrawPdfTemplate(pdfTemplate4, new PointF(0f, 0f));
			}
			else
			{
				pdfTemplate.Graphics.DrawPdfTemplate(pdfTemplate4, new PointF(array[1], 0f - (100f + array[2])));
			}
			pdfTemplate.Graphics.Restore(state4);
			PdfTemplate pdfTemplate5 = new PdfTemplate(sizeF.Width, sizeF.Height / 4f);
			if (m_page != null && (m_page.Rotation == PdfPageRotateAngle.RotateAngle90 || m_page.Rotation == PdfPageRotateAngle.RotateAngle270))
			{
				pdfTemplate5 = new PdfTemplate(sizeF.Height, sizeF.Width / 4f);
			}
			pdfTemplate5.CustomPdfTemplateName = "n4";
			string text = "Signature Not Verified";
			if (m_page != null && (m_page.Rotation == PdfPageRotateAngle.RotateAngle90 || m_page.Rotation == PdfPageRotateAngle.RotateAngle270))
			{
				SetFittingFontSize(new RectangleF(0f, 0f, sizeF.Height, height), font, text);
				pdfTemplate5.Graphics.DrawString(text, font, PdfBrushes.Black, new PointF(0f, 0f));
			}
			else
			{
				SetFittingFontSize(new RectangleF(0f, 0f, sizeF.Width, height), font, text);
				pdfTemplate5.Graphics.DrawString(text, font, PdfBrushes.Black, new PointF(0f, 0f));
			}
			PdfGraphicsState state5 = pdfTemplate.Graphics.Save();
			if ((m_page != null && m_page.Rotation == PdfPageRotateAngle.RotateAngle90) || m_page.Rotation == PdfPageRotateAngle.RotateAngle270)
			{
				pdfTemplate.Graphics.RotateTransform(-90f);
				pdfTemplate.Graphics.DrawPdfTemplate(pdfTemplate5, new PointF(0f, 0f));
			}
			else
			{
				pdfTemplate.Graphics.DrawPdfTemplate(pdfTemplate5, new PointF(0f, 0f - sizeF.Height));
			}
			pdfTemplate.Graphics.Restore(state5);
			PdfStream stream = pdfTemplate.Graphics.StreamWriter.GetStream();
			string @string = Encoding.UTF8.GetString(stream.Data, 0, stream.Data.Length);
			@string = ReviseSignatureValidationStream(@string);
			pdfTemplate.Graphics.StreamWriter.Clear();
			pdfTemplate.Graphics.StreamWriter.Write(@string);
			SetMatrix(pdfTemplate.m_content);
			Appearance.IsCompletedValidationAppearance = true;
			Appearance.Normal = new PdfTemplate(sizeF, writeTransformation: false);
			Appearance.Normal.Graphics.DrawPdfTemplate(pdfTemplate, new PointF(0f, 0f - sizeF.Height));
		}
	}

	private string ReviseSignatureValidationStream(string validationStreamData)
	{
		if (validationStreamData.Contains("\r\n"))
		{
			validationStreamData = validationStreamData.Replace("\r\n", " ");
		}
		if (validationStreamData.Contains("Q Q "))
		{
			validationStreamData = validationStreamData.Replace("Q Q ", "Q Q\r\n");
		}
		return validationStreamData;
	}

	private void SetFittingFontSize(RectangleF rect, PdfFont font, string text)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		if (m_field != null)
		{
			num3 = m_field.BorderWidth;
		}
		else if (m_sigField != null)
		{
			num3 = m_sigField.BorderWidth;
		}
		num2 = rect.Width - 4f * num3;
		float num4 = rect.Height - 2f * num3;
		float num5 = 0.248f;
		font.Style = PdfFontStyle.Bold;
		for (float num6 = 0f; num6 <= rect.Height; num6 += 1f)
		{
			font.Size = num6;
			SizeF sizeF = font.MeasureString(text);
			if (!(sizeF.Width > rect.Width) && !(sizeF.Height > num4))
			{
				continue;
			}
			num = num6;
			do
			{
				num = (font.Size = num - 0.001f);
				float lineWidth = font.GetLineWidth(text, new PdfStringFormat());
				if (num < num5)
				{
					font.Size = num5;
					break;
				}
				sizeF = font.MeasureString(text);
				if (lineWidth < num2 && sizeF.Height < num4)
				{
					font.Size = num;
					break;
				}
			}
			while (num > num5);
			break;
		}
	}

	private void SetMatrix(PdfDictionary template)
	{
		PdfArray pdfArray = null;
		_ = new float[0];
		if (template["BBox"] is PdfArray pdfArray2 && m_page != null && (m_page.Rotation == PdfPageRotateAngle.RotateAngle180 || m_page.Rotation == PdfPageRotateAngle.RotateAngle270))
		{
			pdfArray = new PdfArray(new float[6]
			{
				-1f,
				0f,
				0f,
				-1f,
				(pdfArray2[2] as PdfNumber).FloatValue,
				(pdfArray2[3] as PdfNumber).FloatValue
			});
			template["Matrix"] = pdfArray;
		}
	}

	private float[] findScale(float m_templateWidth, float m_templateHeight)
	{
		float[] array = new float[3];
		array[0] = Math.Min(m_templateWidth, m_templateHeight) * 0.9f;
		array[1] = (m_templateWidth - array[0]) / 2f;
		array[2] = (m_templateHeight - array[0]) / 2f;
		array[0] /= 100f;
		return array;
	}

	private void CheckAnnotationElementsContainsSignature(PdfPageBase page, string signatureName)
	{
		if (page.Dictionary == null || !page.Dictionary.ContainsKey("Annots"))
		{
			return;
		}
		PdfArray pdfArray = PdfCrossTable.Dereference(page.Dictionary["Annots"]) as PdfArray;
		PdfDictionary pdfDictionary = null;
		if (pdfArray != null && pdfArray.Elements.Count > 0)
		{
			pdfDictionary = PdfCrossTable.Dereference(pdfArray[pdfArray.Elements.Count - 1]) as PdfDictionary;
		}
		if (pdfDictionary != null && pdfDictionary.ContainsKey("T"))
		{
			PdfString pdfString = PdfCrossTable.Dereference(pdfDictionary["T"]) as PdfString;
			string text = string.Empty;
			if (pdfString != null)
			{
				text = Encoding.UTF8.GetString(pdfString.Bytes, 0, pdfString.Bytes.Length);
			}
			if (text == signatureName && pdfArray.Elements.Count > 0 && !pdfDictionary.ContainsKey("V"))
			{
				pdfArray.Elements.RemoveAt(pdfArray.Elements.Count - 1);
			}
		}
	}

	private void EnableLtvFromTimestamp()
	{
		if (m_timeStampServer != null && Certificate == null)
		{
			List<X509Certificate2> certificates = new List<X509Certificate2>();
			certificates = TimeStampServerCertificate(certificates);
			CreateLongTermValidity(certificates);
			certificates.Clear();
			certificates = null;
		}
	}

	public void CreateLongTermValidity(List<X509Certificate2> certificates, bool includePublicCertificates = false)
	{
		if (includePublicCertificates)
		{
			certificates = AddTimeStampCertificates(certificates);
		}
		List<X509Certificate> x509CertificateList = GetX509CertificateList(certificates);
		m_addCerts = includePublicCertificates;
		m_enableLTV = GetLTVData(x509CertificateList, null, null);
		m_addCerts = false;
	}

	public void CreateLongTermValidity(List<X509Certificate2> certificates, RevocationType type, bool includePublicCertificates = false)
	{
		if (includePublicCertificates)
		{
			certificates = AddTimeStampCertificates(certificates);
		}
		List<X509Certificate> x509CertificateList = GetX509CertificateList(certificates);
		m_addCerts = includePublicCertificates;
		m_enableLTV = GetLTVData(x509CertificateList, null, null, type);
		m_addCerts = false;
	}

	internal void CreateLongTermValidity(List<X509Certificate2> certificates, List<byte[]> ocspResponseData, List<byte[]> crlResponseData)
	{
		List<X509Certificate> x509CertificateList = GetX509CertificateList(certificates);
		m_enableLTV = GetLTVData(x509CertificateList, ocspResponseData, crlResponseData);
	}

	private List<X509Certificate> GetX509CertificateList(List<X509Certificate2> certificates)
	{
		if (certificates == null)
		{
			certificates = new List<X509Certificate2>();
		}
		m_enableLTV = true;
		X509CertificateParser x509CertificateParser = new X509CertificateParser();
		List<X509Certificate> list = new List<X509Certificate>();
		foreach (X509Certificate2 certificate in certificates)
		{
			list.Add(x509CertificateParser.ReadCertificate(certificate.RawData));
		}
		return list;
	}

	private bool GetLTVData(List<X509Certificate> x509CertificateList, List<byte[]> ocspResponseData, List<byte[]> crlResponseData)
	{
		return GetLTVData(x509CertificateList, ocspResponseData, crlResponseData, RevocationType.OcspAndCrl);
	}

	private bool GetLTVData(List<X509Certificate> x509CertificateList, List<byte[]> ocspResponseData, List<byte[]> crlResponseData, RevocationType revocationType)
	{
		bool flag = false;
		List<byte[]> list = new List<byte[]>();
		if (m_addCerts)
		{
			for (int i = 0; i < x509CertificateList.Count; i++)
			{
				X509Certificate x509Certificate = x509CertificateList[i];
				if (x509Certificate != null)
				{
					list.Add(x509Certificate.GetEncoded());
				}
			}
		}
		if (crlResponseData != null && crlResponseData.Count > 0 && ocspResponseData != null && ocspResponseData.Count > 0)
		{
			flag = GetDssDetails(crlResponseData, ocspResponseData, list);
		}
		else if (ocspResponseData == null && crlResponseData != null && crlResponseData.Count > 0)
		{
			flag = GetDssDetails(crlResponseData, GetOCSPData(new Ocsp(), x509CertificateList), list);
		}
		else
		{
			RevocationList crl = new RevocationList();
			if (ocspResponseData != null && ocspResponseData.Count > 0)
			{
				flag = GetDSSDetails(x509CertificateList, ocspResponseData, crl, list);
			}
			else
			{
				Ocsp ocsp = new Ocsp();
				flag = GetDSSDetails(x509CertificateList, ocsp, crl, revocationType, list);
			}
		}
		if (m_dssDictionary != null)
		{
			m_document.Catalog["DSS"] = new PdfReferenceHolder(m_dssDictionary);
			m_dssDictionary.Modify();
		}
		x509CertificateList.Clear();
		return flag;
	}

	public void AddExternalSigner(IPdfExternalSigner signer, List<X509Certificate2> publicCertificates, byte[] Ocsp)
	{
		m_externalSigner = signer;
		m_externalRootCert = publicCertificates;
		if (Ocsp != null)
		{
			m_isExternalOCSP = true;
		}
		m_ocsp = Ocsp;
		if (publicCertificates == null && m_pdfCertificate == null)
		{
			throw new PdfException("The certificate should not null");
		}
		if (ExternalCertificates == null && m_pdfCertificate != null)
		{
			m_externalRootCert = new List<X509Certificate2>();
			m_externalRootCert.Add(m_pdfCertificate.X509Certificate);
		}
		if (ExternalCertificates != null)
		{
			X509CertificateParser x509CertificateParser = new X509CertificateParser();
			m_externalChain = new List<X509Certificate>();
			foreach (X509Certificate2 externalCertificate in ExternalCertificates)
			{
				m_externalChain.Add(x509CertificateParser.ReadCertificate(externalCertificate.RawData));
			}
		}
		if (m_externalChain != null && OCSP != null && m_isExternalOCSP)
		{
			X509RevocationResponse x509RevocationResponse = (X509RevocationResponse)new OcspResponseHelper(new MemoryStream(m_ocsp)).GetResponseObject();
			if (x509RevocationResponse != null)
			{
				m_ocsp = x509RevocationResponse.EncodedBytes;
			}
		}
		if (EnableLtv)
		{
			EnableExternalLTV();
		}
	}

	internal void OnComputeHash(PdfSignatureEventArgs args)
	{
		if (this.ComputeHash != null)
		{
			this.ComputeHash(this, args);
		}
		if (this.AsyncComputeHash != null)
		{
			this.AsyncComputeHash(this, args).GetAwaiter().GetResult();
		}
	}

	public static void ReplaceEmptySignature(Stream inputFileStream, string pdfPassword, Stream outputFileStream, string signatureName, IPdfExternalSigner externalSigner, List<X509Certificate2> publicCertificates)
	{
		ReplaceEmptySignature(inputFileStream, pdfPassword, outputFileStream, signatureName, externalSigner, publicCertificates, isEncodeSignature: true);
	}

	public static void ReplaceEmptySignature(Stream inputFileStream, string pdfPassword, Stream outputFileStream, string signatureName, IPdfExternalSigner externalSigner, List<X509Certificate2> publicCertificates, bool isEncodeSignature)
	{
		if (inputFileStream == null)
		{
			throw new ArgumentNullException("inputFile is null");
		}
		if (inputFileStream.Length == 0L)
		{
			throw new PdfException("Contents of file stream is empty");
		}
		if (!inputFileStream.CanRead || !inputFileStream.CanSeek)
		{
			throw new ArgumentException("Can't use the specified stream.", "input stream");
		}
		if (inputFileStream.Position != 0L)
		{
			inputFileStream.Position = 0L;
		}
		if (outputFileStream == null)
		{
			throw new ArgumentNullException("outputFile is null");
		}
		if (!outputFileStream.CanWrite || !outputFileStream.CanSeek)
		{
			throw new ArgumentException("Can't use the specified stream to write", "output stream");
		}
		PdfLoadedDocument pdfLoadedDocument = new PdfLoadedDocument(inputFileStream, pdfPassword);
		try
		{
			PdfLoadedForm form = pdfLoadedDocument.Form;
			if (form == null)
			{
				return;
			}
			form.Fields.TryGetField(signatureName, out PdfLoadedField field);
			if (!(field is PdfLoadedSignatureField pdfLoadedSignatureField))
			{
				throw new PdfException("Signature field name not found");
			}
			if (pdfLoadedSignatureField.Dictionary == null || !pdfLoadedSignatureField.Dictionary.ContainsKey("V") || !(PdfCrossTable.Dereference(pdfLoadedSignatureField.Dictionary["V"]) is PdfDictionary pdfDictionary) || !pdfDictionary.ContainsKey("ByteRange") || !(PdfCrossTable.Dereference(pdfDictionary["ByteRange"]) is PdfArray { Count: >3 } pdfArray))
			{
				return;
			}
			long num = 0L;
			long num2 = 0L;
			long num3 = 0L;
			long num4 = 0L;
			if (pdfArray[0] is PdfNumber pdfNumber)
			{
				num = pdfNumber.LongValue;
			}
			if (pdfArray[1] is PdfNumber pdfNumber2)
			{
				num2 = pdfNumber2.LongValue;
			}
			if (pdfArray[2] is PdfNumber pdfNumber3)
			{
				num3 = pdfNumber3.LongValue;
			}
			if (pdfArray[3] is PdfNumber pdfNumber4)
			{
				num4 = pdfNumber4.LongValue;
			}
			long[] array = new long[4] { num, num2, num3, num4 };
			byte[] array2 = new byte[inputFileStream.Length];
			inputFileStream.Position = 0L;
			inputFileStream.Read(array2, 0, (int)inputFileStream.Length);
			IRandom source = new RandomArray(array2);
			IRandom[] array3 = new IRandom[array.Length / 2];
			for (int i = 0; i < array.Length; i += 2)
			{
				array3[i / 2] = new WindowRandom(source, array[i], array[i + 1]);
			}
			Stream stream = new RandomStream(new RandomGroup(array3));
			byte[] array4 = null;
			if (isEncodeSignature)
			{
				byte[] secondDigest = new MessageDigestAlgorithms().Digest(stream, externalSigner.HashAlgorithm);
				string empty = string.Empty;
				SignaturePrivateKey signaturePrivateKey = null;
				ICollection<X509Certificate> collection = new List<X509Certificate>();
				List<X509Certificate> list = null;
				if (externalSigner != null && publicCertificates != null)
				{
					X509CertificateParser x509CertificateParser = new X509CertificateParser();
					foreach (X509Certificate2 publicCertificate in publicCertificates)
					{
						collection.Add(x509CertificateParser.ReadCertificate(publicCertificate.RawData));
					}
					list = new List<X509Certificate>(collection);
					string hashAlgorithm = externalSigner.HashAlgorithm;
					byte[] timeStampResponse = null;
					EncryptionAlgorithms encryptionAlgorithms = new EncryptionAlgorithms();
					string encryptionAlgorithm = null;
					for (int j = 0; j < list.Count; j++)
					{
						if (encryptionAlgorithms.GetAlgorithm(list[j].SigAlgOid) == "ECDSA" || (list[j].CertificateStructure != null && list[j].CertificateStructure.SubjectPublicKeyInfo != null && list[j].CertificateStructure.SubjectPublicKeyInfo.Algorithm != null && list[j].CertificateStructure.SubjectPublicKeyInfo.Algorithm.ObjectID != null && encryptionAlgorithms.GetAlgorithm(list[j].CertificateStructure.SubjectPublicKeyInfo.Algorithm.ObjectID.ID) == "ECDSA"))
						{
							encryptionAlgorithm = "ECDSA";
							break;
						}
					}
					list = null;
					SignaturePrivateKey signaturePrivateKey2 = new SignaturePrivateKey(hashAlgorithm, encryptionAlgorithm);
					empty = signaturePrivateKey2.GetHashAlgorithm();
					signaturePrivateKey = signaturePrivateKey2;
					PdfCmsSigner pdfCmsSigner = new PdfCmsSigner(null, collection, empty, hasRSAdata: false);
					byte[] sequenceData = pdfCmsSigner.GetSequenceData(secondDigest, null, null, pdfLoadedSignatureField.Signature.Settings.CryptographicStandard);
					byte[] digest = externalSigner.Sign(sequenceData, out timeStampResponse);
					pdfCmsSigner.SetSignedData(digest, null, signaturePrivateKey.GetEncryptionAlgorithm());
					array4 = pdfCmsSigner.Sign(secondDigest, null, timeStampResponse, null, null, pdfLoadedSignatureField.Signature.Settings.CryptographicStandard, empty);
					byte[] destinationArray = new byte[array4.Length];
					Array.Copy(array4, 0, destinationArray, 0, array4.Length);
				}
			}
			else if (externalSigner != null)
			{
				stream.Position = 0L;
				byte[] array5 = new byte[stream.Length];
				stream.Read(array5, 0, array5.Length);
				byte[] timeStampResponse2 = null;
				array4 = externalSigner.Sign(array5, out timeStampResponse2);
			}
			int num5 = (int)(array[2] - array[1]) - 2;
			if (((uint)num5 & (true ? 1u : 0u)) != 0)
			{
				throw new PdfException("Allocated space was not enough");
			}
			int num6 = num5 / 2;
			if (num6 < array4.Length)
			{
				throw new PdfException("Signature content space is not enough for signed bytes");
			}
			TransferBytes(inputFileStream, 0L, array[1] + 1, outputFileStream);
			string value = PdfString.BytesToHex(array4);
			StringBuilder stringBuilder = new StringBuilder(num6 * 2);
			stringBuilder.Append(value);
			int num7 = (num6 - array4.Length) * 2;
			for (int k = 0; k < num7; k++)
			{
				stringBuilder.Append(0);
			}
			string text = stringBuilder.ToString();
			MemoryStream memoryStream = new MemoryStream(text.Length);
			memoryStream.Write(Encoding.UTF8.GetBytes(text), 0, text.Length);
			memoryStream.WriteTo(outputFileStream);
			TransferBytes(inputFileStream, array[2] - 1, array[3] + 1, outputFileStream);
			memoryStream.Dispose();
		}
		catch (Exception ex)
		{
			throw ex;
		}
		finally
		{
			pdfLoadedDocument.Close(completely: true);
		}
	}

	private static void TransferBytes(Stream sourceStream, long positionStart, long length, Stream destination)
	{
		if (length <= 0)
		{
			return;
		}
		long num = positionStart;
		byte[] array = new byte[8192];
		while (length > 0)
		{
			long num2 = FindLocation(num, array, 0, (int)Math.Min(array.Length, length), sourceStream);
			if (num2 <= 0)
			{
				throw new EndOfStreamException();
			}
			destination.Write(array, 0, (int)num2);
			num += num2;
			length -= num2;
		}
	}

	private static int FindLocation(long currentPosition, byte[] data, int offsetPosition, int length, Stream sourceStream)
	{
		if (sourceStream.Position != currentPosition)
		{
			sourceStream.Seek(currentPosition, SeekOrigin.Begin);
		}
		int num = sourceStream.Read(data, offsetPosition, length);
		if (num != 0)
		{
			return num;
		}
		return -1;
	}

	private bool CheckCertificated()
	{
		bool result = false;
		if (PdfCrossTable.Dereference(m_document.Catalog["Perms"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("DocMDP"))
		{
			PdfReferenceHolder pdfReferenceHolder = pdfDictionary["DocMDP"] as PdfReferenceHolder;
			if (m_signatureSettings != null && m_signatureSettings.SignatureField != null && m_signatureSettings.SignatureField != null && m_signatureSettings.SignatureField.Dictionary != null && m_signatureSettings.SignatureField.Dictionary.ContainsKey("V"))
			{
				PdfReferenceHolder pdfReferenceHolder2 = m_signatureSettings.SignatureField.Dictionary["V"] as PdfReferenceHolder;
				if (pdfReferenceHolder != null && pdfReferenceHolder2 != null && pdfReferenceHolder.Reference != null && pdfReferenceHolder2.Reference != null && pdfReferenceHolder.Reference.ObjNum == pdfReferenceHolder2.Reference.ObjNum)
				{
					result = true;
				}
			}
			else if (m_signatureSettings != null && m_signatureSettings.SignatureField == null)
			{
				result = true;
			}
			return result;
		}
		return result;
	}

	private bool RemoveUnusedDocMDP()
	{
		bool result = false;
		if (PdfCrossTable.Dereference(m_document.Catalog["Perms"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("DocMDP"))
		{
			PdfReferenceHolder pdfReferenceHolder = pdfDictionary["DocMDP"] as PdfReferenceHolder;
			if (pdfReferenceHolder != null && pdfReferenceHolder.Reference != null)
			{
				bool flag = false;
				if (Field != null && Field.Form != null && Field.Form is PdfLoadedForm { Fields: not null, Fields: not null } pdfLoadedForm)
				{
					foreach (IPdfWrapper field in pdfLoadedForm.Fields)
					{
						if (field is PdfSignatureField)
						{
							PdfSignatureField pdfSignatureField = field as PdfSignatureField;
							if (pdfSignatureField.Dictionary != null && pdfSignatureField.Dictionary.ContainsKey("V"))
							{
								PdfReferenceHolder pdfReferenceHolder2 = pdfSignatureField.Dictionary["V"] as PdfReferenceHolder;
								if (pdfReferenceHolder2 != null && pdfReferenceHolder2.Reference != null && pdfReferenceHolder2.Reference.ObjNum == pdfReferenceHolder.Reference.ObjNum)
								{
									flag = true;
									break;
								}
							}
						}
						else
						{
							if (!(field is PdfLoadedSignatureField))
							{
								continue;
							}
							PdfLoadedSignatureField pdfLoadedSignatureField = field as PdfLoadedSignatureField;
							if (pdfLoadedSignatureField.Dictionary != null && pdfLoadedSignatureField.Dictionary.ContainsKey("V"))
							{
								PdfReferenceHolder pdfReferenceHolder3 = pdfLoadedSignatureField.Dictionary["V"] as PdfReferenceHolder;
								if (pdfReferenceHolder3 != null && pdfReferenceHolder3.Reference != null && pdfReferenceHolder3.Reference.ObjNum == pdfReferenceHolder.Reference.ObjNum)
								{
									flag = true;
									break;
								}
							}
						}
					}
				}
				if (!flag)
				{
					pdfDictionary.Remove("DocMDP");
					result = true;
				}
			}
		}
		return result;
	}

	private bool GetDSSDetails(List<X509Certificate> certificates, Ocsp ocsp, RevocationList crl, RevocationType revocationType, List<byte[]> certCollection)
	{
		List<byte[]> list = new List<byte[]>();
		List<byte[]> list2 = new List<byte[]>();
		bool flag = true;
		for (int i = 0; i < certificates.Count; i++)
		{
			X509Certificate x509Certificate = certificates[i];
			if (x509Certificate.IssuerDN.Equals(x509Certificate.SubjectDN))
			{
				continue;
			}
			bool flag2 = false;
			byte[] array = null;
			if (ocsp != null && revocationType != RevocationType.Crl)
			{
				array = ocsp.GetEncodedOcspRspnose(x509Certificate, GetRoot(x509Certificate, certificates), null);
				if (array != null)
				{
					if (m_ocsp == null)
					{
						m_ocsp = array;
					}
					list2.Add(BuildOCSPResponse(array));
					flag2 = true;
				}
			}
			if (crl != null && (revocationType == RevocationType.Crl || revocationType == RevocationType.OcspAndCrl || (revocationType == RevocationType.OcspOrCrl && array == null)))
			{
				ICollection<byte[]> encoded = crl.GetEncoded(certificates[i], null);
				if (encoded != null)
				{
					foreach (byte[] item in encoded)
					{
						bool flag3 = false;
						foreach (byte[] item2 in list)
						{
							if (Asn1Constants.AreEqual(item2, item))
							{
								flag3 = true;
								flag2 = true;
								break;
							}
						}
						if (!flag3)
						{
							list.Add(item);
							flag2 = true;
						}
					}
				}
			}
			if (!flag2 && flag)
			{
				flag = false;
			}
		}
		if (ExternalSigner != null && CRLBytes == null && list.Count > 0)
		{
			m_crlBytes = list;
		}
		GetDssDetails(list, list2, certCollection);
		return flag;
	}

	private List<byte[]> GetOCSPData(Ocsp ocsp, List<X509Certificate> certificates)
	{
		List<byte[]> list = new List<byte[]>();
		for (int i = 0; i < certificates.Count; i++)
		{
			X509Certificate x509Certificate = certificates[i];
			byte[] array = null;
			if (ocsp != null)
			{
				array = ocsp.GetEncodedOcspRspnose(x509Certificate, GetRoot(x509Certificate, certificates), null);
				if (array != null)
				{
					list.Add(BuildOCSPResponse(array));
				}
			}
		}
		return list;
	}

	private bool GetDSSDetails(List<X509Certificate> certificates, List<byte[]> ocspCollection, RevocationList crl, List<byte[]> certCollection)
	{
		List<byte[]> list = new List<byte[]>();
		for (int i = 0; i < certificates.Count; i++)
		{
			X509Certificate x509Certificate = certificates[i];
			if (crl == null || x509Certificate == null)
			{
				continue;
			}
			ICollection<byte[]> encoded = crl.GetEncoded(x509Certificate, null);
			if (encoded == null)
			{
				continue;
			}
			foreach (byte[] item in encoded)
			{
				bool flag = false;
				foreach (byte[] item2 in list)
				{
					if (Asn1Constants.AreEqual(item2, item))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list.Add(item);
				}
			}
		}
		if (ExternalSigner != null && CRLBytes == null)
		{
			m_crlBytes = list;
		}
		return GetDssDetails(list, ocspCollection, certCollection);
	}

	private bool GetDssDetails(List<byte[]> crlCollection, List<byte[]> ocspCollection, List<byte[]> certCollection)
	{
		if (crlCollection.Count == 0 && ocspCollection.Count == 0 && certCollection.Count == 0)
		{
			return false;
		}
		if (m_document != null && m_document.Catalog != null && m_document.Catalog.ContainsKey("DSS"))
		{
			m_dssDictionary = PdfCrossTable.Dereference(m_document.Catalog["DSS"]) as PdfDictionary;
		}
		if (m_dssDictionary == null)
		{
			m_dssDictionary = new PdfDictionary();
		}
		PdfArray pdfArray = new PdfArray();
		PdfArray pdfArray2 = new PdfArray();
		PdfArray pdfArray3 = new PdfArray();
		PdfArray pdfArray4 = new PdfArray();
		PdfArray pdfArray5 = new PdfArray();
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		new List<string>();
		List<string> list3 = new List<string>();
		if (m_dssDictionary.ContainsKey("OCSPs"))
		{
			if (PdfCrossTable.Dereference(m_dssDictionary["OCSPs"]) is PdfArray pdfArray6)
			{
				pdfArray = pdfArray6;
			}
			if (pdfArray != null && pdfArray.Count > 0)
			{
				for (int i = 0; i < pdfArray.Count; i++)
				{
					byte[] decompressedData = (PdfCrossTable.Dereference(pdfArray[i]) as PdfStream).GetDecompressedData();
					list.Add(m_document.CreateHashFromStream(decompressedData));
				}
			}
		}
		if (m_dssDictionary.ContainsKey("CRLs"))
		{
			if (PdfCrossTable.Dereference(m_dssDictionary["CRLs"]) is PdfArray pdfArray7)
			{
				pdfArray2 = pdfArray7;
			}
			if (pdfArray2 != null && pdfArray2.Count > 0)
			{
				for (int j = 0; j < pdfArray2.Count; j++)
				{
					byte[] decompressedData2 = (PdfCrossTable.Dereference(pdfArray2[j]) as PdfStream).GetDecompressedData();
					list2.Add(m_document.CreateHashFromStream(decompressedData2));
				}
			}
		}
		PdfDictionary pdfDictionary = new PdfDictionary();
		if (m_dssDictionary.ContainsKey("VRI") && PdfCrossTable.Dereference(m_dssDictionary["VRI"]) is PdfDictionary pdfDictionary2)
		{
			pdfDictionary = pdfDictionary2;
		}
		if (m_dssDictionary.ContainsKey("Certs"))
		{
			if (PdfCrossTable.Dereference(m_dssDictionary["Certs"]) is PdfArray pdfArray8)
			{
				pdfArray5 = pdfArray8;
			}
			if (pdfArray5 != null && pdfArray5.Count > 0)
			{
				for (int k = 0; k < pdfArray5.Count; k++)
				{
					byte[] decompressedData3 = (PdfCrossTable.Dereference(pdfArray5[k]) as PdfStream).GetDecompressedData();
					list3.Add(m_document.CreateHashFromStream(decompressedData3));
				}
			}
		}
		for (int l = 0; l < ocspCollection.Count; l++)
		{
			byte[] streamBytes = ocspCollection[l];
			string item = m_document.CreateHashFromStream(streamBytes);
			if (!list.Contains(item))
			{
				PdfReferenceHolder element = new PdfReferenceHolder(new PdfStream(new PdfDictionary(), ocspCollection[l])
				{
					Compress = true
				});
				pdfArray.Add(element);
				pdfArray3.Add(element);
				list.Add(item);
			}
		}
		for (int m = 0; m < crlCollection.Count; m++)
		{
			byte[] streamBytes2 = crlCollection[m];
			string item2 = m_document.CreateHashFromStream(streamBytes2);
			if (!list2.Contains(item2))
			{
				PdfReferenceHolder element2 = new PdfReferenceHolder(new PdfStream(new PdfDictionary(), crlCollection[m])
				{
					Compress = true
				});
				pdfArray2.Add(element2);
				pdfArray4.Add(element2);
				list2.Add(item2);
			}
		}
		for (int n = 0; n < certCollection.Count; n++)
		{
			byte[] streamBytes3 = certCollection[n];
			string item3 = m_document.CreateHashFromStream(streamBytes3);
			if (!list3.Contains(item3))
			{
				PdfReferenceHolder element3 = new PdfReferenceHolder(new PdfStream(new PdfDictionary(), certCollection[n])
				{
					Compress = true
				});
				pdfArray5.Add(element3);
				list3.Add(item3);
			}
		}
		string value = GetVRIName().ToUpper();
		PdfDictionary pdfDictionary3 = new PdfDictionary();
		if (pdfDictionary.ContainsKey(new PdfName(value)))
		{
			if (PdfCrossTable.Dereference(pdfDictionary[new PdfName(value)]) is PdfDictionary pdfDictionary4)
			{
				pdfDictionary3 = pdfDictionary4;
			}
			if (pdfDictionary3.ContainsKey(new PdfName("OCSP")) && PdfCrossTable.Dereference(pdfDictionary3["OCSP"]) is PdfArray pdfArray9)
			{
				for (int num = 0; num < pdfArray3.Count; num++)
				{
					if (!pdfArray9.Contains(pdfArray3[num]))
					{
						pdfArray9.Add(pdfArray3[num]);
					}
				}
			}
			if (pdfDictionary3.ContainsKey(new PdfName("CRL")) && PdfCrossTable.Dereference(pdfDictionary3["CRL"]) is PdfArray pdfArray10)
			{
				for (int num2 = 0; num2 < pdfArray4.Count; num2++)
				{
					if (!pdfArray10.Contains(pdfArray4[num2]))
					{
						pdfArray10.Add(pdfArray4[num2]);
					}
				}
			}
		}
		else
		{
			pdfDictionary3.Items.Add(new PdfName("OCSP"), new PdfReferenceHolder(pdfArray));
			pdfDictionary3.Items.Add(new PdfName("CRL"), new PdfReferenceHolder(pdfArray2));
			pdfDictionary.Items.Add(new PdfName(GetVRIName().ToUpper()), new PdfReferenceHolder(pdfDictionary3));
		}
		pdfDictionary.Modify();
		m_dssDictionary.Items[new PdfName("OCSPs")] = new PdfReferenceHolder(pdfArray);
		m_dssDictionary.Items[new PdfName("CRLs")] = new PdfReferenceHolder(pdfArray2);
		m_dssDictionary.Items[new PdfName("VRI")] = new PdfReferenceHolder(pdfDictionary);
		if (certCollection.Count > 0)
		{
			m_dssDictionary.Items[new PdfName("Certs")] = new PdfReferenceHolder(pdfArray5);
		}
		list3.Clear();
		list2.Clear();
		list.Clear();
		return true;
	}

	private string GetVRIName()
	{
		byte[] array = null;
		array = ((SignatureContentBytes == null) ? Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()) : SignatureContentBytes);
		byte[] array2 = new MessageDigestAlgorithms().Digest("SHA1", array);
		StringBuilder stringBuilder = new StringBuilder();
		byte[] array3 = array2;
		foreach (byte b in array3)
		{
			stringBuilder.AppendFormat("{0:x2}", b);
		}
		return stringBuilder.ToString();
	}

	private static byte[] BuildOCSPResponse(byte[] BasicOCSPResponse)
	{
		DerOctet derOctet = new DerOctet(BasicOCSPResponse);
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection();
		asn1EncodeCollection.Add(OcspConstants.OcspBasic);
		asn1EncodeCollection.Add(derOctet);
		DerCatalogue derCatalogue = new DerCatalogue(0);
		Asn1EncodeCollection asn1EncodeCollection2 = new Asn1EncodeCollection();
		asn1EncodeCollection2.Add(derCatalogue);
		asn1EncodeCollection2.Add(new DerTag(isExplicit: true, 0, new DerSequence(asn1EncodeCollection)));
		return new DerSequence(asn1EncodeCollection2).GetEncoded();
	}

	private X509Certificate GetRoot(X509Certificate cert, List<X509Certificate> certs)
	{
		for (int i = 0; i < certs.Count; i++)
		{
			X509Certificate x509Certificate = certs[i];
			if (cert.IssuerDN.Equals(x509Certificate.SubjectDN))
			{
				try
				{
					cert.Verify(x509Certificate.GetPublicKey());
					return x509Certificate;
				}
				catch
				{
				}
			}
		}
		return null;
	}

	private byte[] GetEncoded(X509Certificate checkCert, X509Certificate rootCert, string url)
	{
		try
		{
			X509RevocationResponse basicOCSPResponse = GetBasicOCSPResponse(checkCert, rootCert, url);
			if (basicOCSPResponse != null)
			{
				OneTimeResponse[] responses = basicOCSPResponse.Responses;
				if (responses.Length == 1 && responses[0].CertificateStatus == CerificateStatus.Good)
				{
					return basicOCSPResponse.EncodedBytes;
				}
			}
		}
		catch (Exception)
		{
			return null;
		}
		return null;
	}

	internal X509RevocationResponse GetBasicOCSPResponse(X509Certificate checkCert, X509Certificate rootCert, string url)
	{
		OcspResponseHelper ocspResponse = GetOcspResponse(checkCert, rootCert, url);
		if (ocspResponse == null)
		{
			return null;
		}
		if (ocspResponse.Status != 0)
		{
			return null;
		}
		return (X509RevocationResponse)ocspResponse.GetResponseObject();
	}

	private OcspResponseHelper GetOcspResponse(X509Certificate checkCert, X509Certificate rootCert, string url)
	{
		if (checkCert == null || rootCert == null)
		{
			return null;
		}
		if (url == null)
		{
			url = new CertificateUtililty().GetOcspUrl(checkCert);
		}
		if (url == null)
		{
			return null;
		}
		return new OcspResponseHelper(new MemoryStream(GetTimeStampResponse(m_inputBytes = GenerateOCSPRequest(rootCert, checkCert.SerialNumber).GetEncoded(), url).Result));
	}

	internal async Task<byte[]> GetTimeStampResponse(byte[] request, string url)
	{
		HttpClient client = new HttpClient
		{
			BaseAddress = new Uri(url)
		};
		Stream obj = await client.GetStreamAsync(url.ToString());
		obj.Write(request, 0, request.Length);
		obj.Dispose();
		Stream stream = await (await client.GetAsync(url.ToString())).Content.ReadAsStreamAsync();
		MemoryStream memoryStream = new MemoryStream();
		byte[] array = new byte[1024];
		int count;
		while ((count = stream.Read(array, 0, array.Length)) > 0)
		{
			memoryStream.Write(array, 0, count);
		}
		return memoryStream.ToArray();
	}

	private static OcspRequestHelper GenerateOCSPRequest(X509Certificate issuerCert, Number serialNumber)
	{
		CertificateIdentity id = new CertificateIdentity("1.3.14.3.2.26", issuerCert, serialNumber);
		OcspRequestCreator ocspRequestCreator = new OcspRequestCreator();
		ocspRequestCreator.AddRequest(id);
		ocspRequestCreator.SetRequestExtensions(new X509Extensions(new Dictionary<DerObjectID, X509Extension> { 
		{
			OcspConstants.OcspNonce,
			new X509Extension(critical: false, new DerOctet(new DerOctet(PdfEncryption.CreateDocumentId()).GetEncoded()))
		} }));
		return ocspRequestCreator.Generate();
	}

	private static Asn1 GetExtensionValue(X509Certificate cert, string oid)
	{
		byte[] derEncoded = cert.GetExtension(new DerObjectID(oid)).GetDerEncoded();
		if (derEncoded == null)
		{
			return null;
		}
		return new Asn1Stream(new MemoryStream(((Asn1Octet)new Asn1Stream(new MemoryStream(derEncoded)).ReadAsn1()).GetOctets())).ReadAsn1();
	}

	internal void EnableExternalLTV()
	{
		if (m_externalChain != null)
		{
			if (OCSP != null && m_isExternalOCSP)
			{
				List<byte[]> list = new List<byte[]>();
				list.Add(OCSP);
				m_enableLTV = GetLTVData(m_externalChain, list, null);
			}
			else
			{
				m_enableLTV = GetLTVData(m_externalChain, null, null);
			}
		}
	}

	private void LockSignature()
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		pdfDictionary.SetProperty("Type", new PdfName("SigFieldLock"));
		pdfDictionary.SetProperty("Action", new PdfName("All"));
		pdfDictionary.SetProperty("P", new PdfNumber(1));
		if (m_field != null)
		{
			m_field.Dictionary.SetProperty("Lock", new PdfReferenceHolder(pdfDictionary));
		}
		else if (m_sigField != null)
		{
			m_sigField.Dictionary.SetProperty("Lock", new PdfReferenceHolder(pdfDictionary));
		}
	}

	private List<X509Certificate2> AddTimeStampCertificates(List<X509Certificate2> certificates)
	{
		if (certificates == null)
		{
			certificates = new List<X509Certificate2>();
		}
		if (certificates != null && Settings != null && Settings.SignatureField != null && Settings.SignatureField.CmsSigner != null)
		{
			if (certificates.Count == 0 && Settings.SignatureField.CmsSigner.CertificateList != null)
			{
				foreach (X509Certificate certificate in Settings.SignatureField.CmsSigner.CertificateList)
				{
					certificates.Add(new X509Certificate2(certificate.GetEncoded()));
				}
			}
			certificates = Settings.SignatureField.CmsSigner.GetTimeStampCertificates(certificates);
			if (m_timeStampServer != null)
			{
				certificates = TimeStampServerCertificate(certificates);
			}
		}
		else if (m_timeStampServer != null)
		{
			certificates = TimeStampServerCertificate(certificates);
		}
		return certificates;
	}

	private List<X509Certificate2> TimeStampServerCertificate(List<X509Certificate2> certificates)
	{
		if (m_timeStampServer != null)
		{
			byte[] hash = new MessageDigestAlgorithms().Digest(new MemoryStream(Encoding.ASCII.GetBytes("Test data")), "SHA256");
			byte[] asnEncodedTimestampRequest = new TimeStampRequestCreator(certReq: true).GetAsnEncodedTimestampRequest(hash);
			Asn1 asn = (new Asn1Stream(m_timeStampServer.GetTimeStampResponse(asnEncodedTimestampRequest)).ReadAsn1() as Asn1Sequence)[1] as Asn1;
			MemoryStream memoryStream = new MemoryStream();
			DerStream derStream = new DerStream(memoryStream);
			asn.Encode(derStream);
			byte[] input = memoryStream.ToArray();
			List<X509Certificate> list = new X509CertificateParser().ReadCertificates(input) as List<X509Certificate>;
			for (int num = list.Count - 1; num >= 0; num--)
			{
				X509Certificate2 item = new X509Certificate2(list[num].CertificateStructure.GetDerEncoded());
				if (!certificates.Contains(item))
				{
					certificates.Add(item);
				}
			}
			derStream.m_stream.Dispose();
			memoryStream.Dispose();
			return certificates;
		}
		return certificates;
	}

	private void Catalog_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		if (m_certeficated)
		{
			if (!(PdfCrossTable.Dereference(m_document.Catalog["Perms"]) is PdfDictionary pdfDictionary))
			{
				PdfDictionary pdfDictionary2 = new PdfDictionary();
				pdfDictionary2["DocMDP"] = new PdfReferenceHolder(m_signatureDictionary);
				m_document.Catalog["Perms"] = pdfDictionary2;
			}
			else if (!pdfDictionary.ContainsKey("DocMDP"))
			{
				pdfDictionary.SetProperty("DocMDP", new PdfReferenceHolder(m_signatureDictionary));
			}
		}
		else if (m_isPermissionUpdated)
		{
			PdfDictionary pdfDictionary3 = PdfCrossTable.Dereference(m_document.Catalog["Perms"]) as PdfDictionary;
			if (pdfDictionary3 == null)
			{
				pdfDictionary3 = new PdfDictionary();
				pdfDictionary3["DocMDP"] = new PdfReferenceHolder(m_signatureDictionary);
				m_document.Catalog["Perms"] = pdfDictionary3;
			}
		}
	}

	private void Dictionary_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		if (EnableValidationAppearance)
		{
			SetValidationApperance();
			if (m_field != null && !m_field.Dictionary.ContainsKey("Kids"))
			{
				EnableValidationAppearance = false;
			}
		}
		if (m_field != null)
		{
			m_field.Dictionary.Encrypt = m_document.Security.Enabled;
		}
		else
		{
			m_sigField.Dictionary.Encrypt = m_document.Security.Enabled;
		}
		if (m_sigField != null && Appearance != null && Appearance.GetNormalTemplate() != null)
		{
			if (m_sigField.Dictionary.ContainsKey("Kids"))
			{
				if (PdfCrossTable.Dereference(m_sigField.Dictionary["Kids"]) is PdfArray { Count: >0 } pdfArray)
				{
					for (int i = 0; i < pdfArray.Count; i++)
					{
						if (PdfCrossTable.Dereference(pdfArray[i]) is PdfDictionary pdfDictionary)
						{
							pdfDictionary.SetProperty("AP", Appearance);
						}
					}
				}
			}
			else
			{
				m_sigField.Dictionary.SetProperty("AP", Appearance);
			}
		}
		if (!m_certeficated && m_lock && !m_signed)
		{
			LockSignature();
		}
	}
}
