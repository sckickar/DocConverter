using System;
using System.Diagnostics;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart;

internal class ExcelEngine : IDisposable
{
	private ApplicationImpl m_appl;

	private bool m_bDisposed;

	private bool m_bAskSaveOnDestroy;

	public IApplication Excel
	{
		[DebuggerStepThrough]
		get
		{
			if (m_bDisposed)
			{
				throw new ObjectDisposedException("Application", "Cannot use dipose object.");
			}
			return m_appl;
		}
	}

	public bool ThrowNotSavedOnDestroy
	{
		get
		{
			return m_bAskSaveOnDestroy;
		}
		set
		{
			m_bAskSaveOnDestroy = value;
		}
	}

	internal static bool IsSecurityGranted => true;

	public ExcelEngine()
	{
		bool evalExpired = false;
		if (IsSecurityGranted)
		{
			evalExpired = ValidateLicense();
		}
		m_appl = new ApplicationImpl(this);
		m_appl.EvalExpired = evalExpired;
	}

	~ExcelEngine()
	{
		Dispose();
	}

	public void Dispose()
	{
		if (!m_bDisposed)
		{
			if (m_bAskSaveOnDestroy && !m_appl.IsSaved)
			{
				throw new ExcelWorkbookNotSavedException("Object cannot be disposed. Save workbook or set property ThrowNotSavedOnDestoy to false.");
			}
			IWorkbooks workbooks = m_appl.Workbooks;
			for (int num = workbooks.Count - 1; num >= 0; num--)
			{
				(workbooks[num] as WorkbookImpl).ClearExtendedFormats();
				workbooks[num].Close();
			}
			m_appl.Dispose();
			m_appl = null;
			m_bDisposed = true;
			GC.SuppressFinalize(this);
		}
	}

	internal static bool ValidateLicense()
	{
		return false;
	}
}
