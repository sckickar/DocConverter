using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace DocGen.Pdf.Security;

internal class RevocationList
{
	private IList<string> m_urls = new List<string>();

	internal PdfSignatureValidationResult result;

	private Stream m_stream;

	private ManualResetEvent allDone = new ManualResetEvent(initialState: false);

	private CertificateUtililty m_utility = new CertificateUtililty();

	private string m_currentUrl = string.Empty;

	internal RevocationList(ICollection<X509Certificate> chain)
	{
		foreach (X509Certificate item in chain)
		{
			Initialize(item);
		}
	}

	internal RevocationList()
	{
	}

	internal RevocationList(X509Certificate certificate)
	{
		Initialize(certificate);
	}

	private void Initialize(X509Certificate certificate)
	{
		List<string> list = null;
		try
		{
			list = m_utility.GetCrlUrls(certificate);
			if (list == null)
			{
				return;
			}
			foreach (string item in list)
			{
				AddUrl(item);
			}
		}
		catch
		{
			throw new Exception("Invalid CRL URL");
		}
	}

	protected virtual void AddUrl(string url)
	{
		if (!m_urls.Contains(url))
		{
			m_urls.Add(url);
		}
	}

	internal ICollection<byte[]> GetEncoded(X509Certificate certificate, string url)
	{
		if (certificate == null)
		{
			return null;
		}
		List<string> list = new List<string>(m_urls);
		if (list.Count == 0)
		{
			try
			{
				List<string> list2 = null;
				list2 = ((url == null) ? m_utility.GetCrlUrls(certificate) : new List<string> { url });
				if (list2 != null)
				{
					foreach (string item in list2)
					{
						list.Add(item);
					}
				}
			}
			catch (Exception ex)
			{
				if (result != null)
				{
					PdfSignatureValidationException ex2 = new PdfSignatureValidationException(ex.Message);
					ex2.ExceptionType = PdfSignatureValidationExceptionType.CRL;
					result.SignatureValidationErrors.Add(ex2);
				}
			}
		}
		List<byte[]> list3 = new List<byte[]>();
		foreach (string item2 in list)
		{
			string requestUriString = (m_currentUrl = item2);
			try
			{
				Stream stream = null;
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUriString);
				if (result == null)
				{
					httpWebRequest.Timeout = 5000;
				}
				httpWebRequest.BeginGetResponse(GetResponseCallback, httpWebRequest);
				allDone.WaitOne();
				allDone.Reset();
				stream = m_stream;
				byte[] array = new byte[1024];
				MemoryStream memoryStream = new MemoryStream();
				if (stream != null)
				{
					while (true)
					{
						int num = stream.Read(array, 0, array.Length);
						if (num <= 0)
						{
							break;
						}
						memoryStream.Write(array, 0, num);
					}
					try
					{
						stream.Dispose();
					}
					catch (Exception)
					{
					}
				}
				list3.Add(memoryStream.ToArray());
			}
			catch (Exception)
			{
				continue;
			}
			break;
		}
		return list3;
	}

	private void GetResponseCallback(IAsyncResult asynchronousResult)
	{
		HttpWebRequest httpWebRequest = null;
		HttpWebResponse httpWebResponse = null;
		Stream stream = null;
		httpWebRequest = (HttpWebRequest)WebRequest.Create(m_currentUrl);
		try
		{
			_ = ((HttpWebResponse)httpWebRequest.GetResponse()).StatusCode;
			httpWebRequest = (HttpWebRequest)asynchronousResult.AsyncState;
			httpWebResponse = (HttpWebResponse)httpWebRequest.EndGetResponse(asynchronousResult);
			stream = httpWebResponse.GetResponseStream();
			m_stream = stream;
		}
		catch (Exception)
		{
			try
			{
				httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
				m_stream = httpWebResponse.GetResponseStream();
			}
			catch (Exception)
			{
			}
		}
		allDone.Set();
	}
}
