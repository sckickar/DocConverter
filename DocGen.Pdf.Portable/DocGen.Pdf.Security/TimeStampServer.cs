using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace DocGen.Pdf.Security;

public class TimeStampServer
{
	private Uri m_server;

	private string m_username;

	private string m_password;

	private int m_timeOut;

	private byte[] inputBytes;

	private Stream m_stream;

	private ManualResetEvent allDone = new ManualResetEvent(initialState: false);

	public Uri Server
	{
		get
		{
			return m_server;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Server");
			}
			m_server = value;
		}
	}

	public string UserName
	{
		get
		{
			return m_username;
		}
		set
		{
			m_username = value;
		}
	}

	public string Password
	{
		get
		{
			return m_password;
		}
		set
		{
			m_password = value;
		}
	}

	public int TimeOut
	{
		get
		{
			return m_timeOut;
		}
		set
		{
			m_timeOut = value;
		}
	}

	public bool IsValid => IsValidTimeStamp();

	public TimeStampServer(Uri server)
	{
		if (server == null)
		{
			throw new ArgumentNullException("Sever");
		}
		m_server = server;
	}

	public TimeStampServer(Uri server, string username, string password)
		: this(server)
	{
		m_username = username;
		m_password = password;
	}

	public TimeStampServer(Uri server, string username, string password, int timeOut)
		: this(server, username, password)
	{
		m_timeOut = timeOut;
	}

	internal bool IsValidTimeStamp()
	{
		bool result = false;
		try
		{
			byte[] hash = new MessageDigestAlgorithms().Digest(new MemoryStream(Encoding.Unicode.GetBytes("Test data")), "SHA256");
			byte[] asnEncodedTimestampRequest = new TimeStampRequestCreator(certReq: true).GetAsnEncodedTimestampRequest(hash);
			byte[] timeStampResponse = GetTimeStampResponse(asnEncodedTimestampRequest);
			if (timeStampResponse.Length > 2 && timeStampResponse[0] == 48 && timeStampResponse[1] == 130)
			{
				result = true;
			}
		}
		catch
		{
			result = false;
		}
		return result;
	}

	internal byte[] GetTimeStampResponse(byte[] request)
	{
		inputBytes = request;
		HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(m_server.ToString());
		httpWebRequest.UserAgent = "syncfusion";
		httpWebRequest.ProtocolVersion = HttpVersion.Version11;
		httpWebRequest.ContentLength = request.Length;
		httpWebRequest.ContentType = "application/timestamp-query";
		httpWebRequest.Method = "POST";
		if (!string.IsNullOrEmpty(m_username))
		{
			string s = m_username + ":" + m_password;
			s = Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
			httpWebRequest.Headers["Authorization"] = "Basic " + s;
		}
		httpWebRequest.BeginGetRequestStream(GetRequestStreamCallback, httpWebRequest);
		allDone.WaitOne();
		allDone.Reset();
		MemoryStream memoryStream = new MemoryStream();
		byte[] array = new byte[1024];
		int num = 0;
		while ((num = m_stream.Read(array, 0, array.Length)) > 0)
		{
			memoryStream.Write(array, 0, num);
		}
		return memoryStream.ToArray();
	}

	private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
	{
		HttpWebRequest httpWebRequest = (HttpWebRequest)asynchronousResult.AsyncState;
		Stream stream = httpWebRequest.EndGetRequestStream(asynchronousResult);
		stream.Write(inputBytes, 0, inputBytes.Length);
		stream.Close();
		httpWebRequest.BeginGetResponse(GetResponseCallback, httpWebRequest);
		allDone.WaitOne();
	}

	private void GetResponseCallback(IAsyncResult asynchronousResult)
	{
		HttpWebRequest httpWebRequest = null;
		HttpWebResponse httpWebResponse = null;
		Stream stream = null;
		httpWebRequest = (HttpWebRequest)WebRequest.Create(m_server.ToString());
		httpWebRequest.UseDefaultCredentials = true;
		httpWebRequest.PreAuthenticate = true;
		httpWebRequest.Credentials = CredentialCache.DefaultCredentials;
		if (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(m_password))
		{
			httpWebRequest.Credentials = new NetworkCredential(UserName, m_password);
		}
		try
		{
			httpWebRequest = (HttpWebRequest)asynchronousResult.AsyncState;
			httpWebResponse = (HttpWebResponse)httpWebRequest.EndGetResponse(asynchronousResult);
			stream = httpWebResponse.GetResponseStream();
			m_stream = stream;
		}
		catch (Exception)
		{
			httpWebRequest = (HttpWebRequest)WebRequest.Create(m_server.ToString());
			httpWebRequest.UserAgent = "syncfusion";
			httpWebRequest.ProtocolVersion = HttpVersion.Version11;
			httpWebRequest.ContentLength = inputBytes.Length;
			httpWebRequest.ContentType = "application/timestamp-query";
			httpWebRequest.Method = "POST";
			if (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(m_password))
			{
				httpWebRequest.Credentials = new NetworkCredential(UserName, m_password);
			}
			stream = httpWebRequest.GetRequestStream();
			stream.Write(inputBytes, 0, inputBytes.Length);
			stream.Close();
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
