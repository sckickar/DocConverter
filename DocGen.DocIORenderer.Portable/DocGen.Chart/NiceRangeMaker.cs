#define TRACE
using System;
using System.Diagnostics;

namespace DocGen.Chart;

internal class NiceRangeMaker : INiceRangeMaker
{
	protected class OpState
	{
		private NiceRangeMaker parent;

		private double min;

		private double max;

		private double interval;

		private double calcMin;

		private double calcMax;

		private double calcInterval;

		private int adjustedPlaces;

		public double Min => min;

		public double Max => max;

		public double Interval => interval;

		public double CalcMin
		{
			get
			{
				return calcMin;
			}
			set
			{
				calcMin = value;
			}
		}

		public double CalcMax
		{
			get
			{
				return calcMax;
			}
			set
			{
				calcMax = value;
			}
		}

		public double CalcInterval
		{
			get
			{
				return calcInterval;
			}
			set
			{
				calcInterval = value;
			}
		}

		public int AdjustedPlaces
		{
			get
			{
				return adjustedPlaces;
			}
			set
			{
				adjustedPlaces = value;
			}
		}

		public OpState(NiceRangeMaker parent, double min, double max)
		{
			this.parent = parent;
			this.min = min;
			this.max = max;
			interval = (this.max - this.min) / (double)this.parent.DesiredIntervals;
			calcMin = min;
			calcMax = max;
			calcInterval = Interval;
		}

		public void UpdateCalcInterval()
		{
			CalcInterval = (CalcMax - CalcMin) / (double)parent.DesiredIntervals;
		}
	}

	private const int DESIRED_INTERVALS = 12;

	private int desiredIntervals;

	private bool preferZero = true;

	private bool forceZero;

	private ChartAxisRangePaddingType m_rangePaddingType;

	private double m_padding;

	public virtual int DesiredIntervals
	{
		get
		{
			return desiredIntervals;
		}
		set
		{
			desiredIntervals = value;
		}
	}

	public virtual ChartAxisRangePaddingType RangePaddingType
	{
		get
		{
			return m_rangePaddingType;
		}
		set
		{
			m_rangePaddingType = value;
		}
	}

	public virtual bool PreferZero
	{
		get
		{
			return preferZero;
		}
		set
		{
			preferZero = value;
		}
	}

	public virtual bool ForceZero
	{
		get
		{
			return forceZero;
		}
		set
		{
			forceZero = value;
		}
	}

	public NiceRangeMaker(int desiredIntervals)
	{
		this.desiredIntervals = desiredIntervals;
	}

	public NiceRangeMaker()
		: this(12)
	{
	}

	public virtual MinMaxInfo MakeNiceRange(double min, double max, ChartAxisRangePaddingType rangePaddingType)
	{
		if (max < min)
		{
			throw new ArgumentOutOfRangeException("Minimum value cannot be greater than or equals maximum value.");
		}
		DoubleRange range = new DoubleRange(min, max);
		if (forceZero)
		{
			range = DoubleRange.Union(range, 0.0);
		}
		OpState opState = new OpState(this, range.Start, range.End);
		CreatePadding(opState, rangePaddingType);
		TweakMinMax(opState);
		AdjustPlaces(opState);
		CalcNiceValues(opState);
		UndoAdjustPlaces(opState);
		TweakToFitZero(opState);
		if (rangePaddingType == ChartAxisRangePaddingType.Calculate)
		{
			TweakBoundaries(opState);
		}
		return new MinMaxInfo(opState.CalcMin, opState.CalcMax, opState.CalcInterval);
	}

	protected virtual void CreatePadding(OpState opState, ChartAxisRangePaddingType rangePaddingType)
	{
		m_padding = 0.0;
		if (rangePaddingType == ChartAxisRangePaddingType.Calculate)
		{
			m_padding = (opState.Max - opState.Min) / (double)(desiredIntervals * 2);
		}
		opState.CalcMax = opState.Max + m_padding;
		opState.CalcMin = opState.Min - m_padding;
	}

	protected virtual void TweakMinMax(OpState opState)
	{
		if (opState.CalcMin == 0.0 && opState.CalcMax == 0.0)
		{
			opState.CalcMax = 1.0;
			opState.UpdateCalcInterval();
		}
		if (opState.CalcMin == opState.CalcMax)
		{
			if (Math.Sign(opState.Max) == -1)
			{
				opState.CalcMax = 0.0;
			}
			else
			{
				opState.CalcMin = 0.0;
			}
			opState.UpdateCalcInterval();
		}
	}

	protected virtual void AdjustPlaces(OpState opState)
	{
		double calcInterval = opState.CalcInterval;
		int num = (int)Math.Floor(Math.Log10(calcInterval)) - 1;
		calcInterval /= Math.Pow(10.0, num);
		opState.AdjustedPlaces = num;
		opState.CalcInterval = Math.Ceiling(calcInterval);
		opState.CalcMin /= Math.Pow(10.0, num);
		opState.CalcMax /= Math.Pow(10.0, num);
	}

	protected virtual void CalcNiceValues(OpState opState)
	{
		CalcNiceInterval(opState);
		CalcNiceMin(opState);
		CalcNiceMax(opState);
	}

	protected virtual void CalcNiceInterval(OpState opState)
	{
		Trace.Assert(Math.Round(opState.CalcInterval, 0) == opState.CalcInterval);
		string text = opState.CalcInterval.ToString("F0");
		int length = text.Length;
		Trace.Assert(length >= 2);
		double val = double.Parse(text.Substring(0, 2));
		double num = MakeNiceNumber(val);
		string text2 = new string('0', length - 2);
		text2 = num + text2;
		opState.CalcInterval = double.Parse(text2);
	}

	protected virtual double MakeNiceNumber(double val)
	{
		if (val < 10.0)
		{
			return 10.0;
		}
		if (val < 20.0)
		{
			return 20.0;
		}
		if (val < 25.0)
		{
			return 25.0;
		}
		if (val < 50.0)
		{
			return 50.0;
		}
		return 100.0;
	}

	protected virtual void CalcNiceMin(OpState opState)
	{
		opState.CalcMin = ChartMath.Round(opState.CalcMin, opState.CalcInterval, up: false);
	}

	protected virtual void CalcNiceMax(OpState opState)
	{
		opState.CalcMax = ((RangePaddingType == ChartAxisRangePaddingType.None) ? ChartMath.RoundDateTimeRange(opState.CalcMax, opState.CalcInterval, up: true) : ChartMath.Round(opState.CalcMax, opState.CalcInterval, up: true));
	}

	protected virtual void UndoAdjustPlaces(OpState opState)
	{
		opState.CalcMin *= Math.Pow(10.0, opState.AdjustedPlaces);
		opState.CalcMax *= Math.Pow(10.0, opState.AdjustedPlaces);
		opState.CalcInterval *= Math.Pow(10.0, opState.AdjustedPlaces);
	}

	protected virtual void TweakToFitZero(OpState opState)
	{
		if (!preferZero && !forceZero)
		{
			return;
		}
		int num = (int)((opState.CalcMax - opState.CalcMin) / opState.CalcInterval);
		if (Math.Sign(opState.Max) == -1 || Math.Sign(opState.Max) == 0)
		{
			if (forceZero)
			{
				opState.CalcMax = 0.0;
			}
			else if (opState.CalcMax + (double)(num / 2) * opState.CalcInterval >= 0.0)
			{
				opState.CalcMax = 0.0;
			}
		}
		else if (Math.Sign(opState.Min) == 1 || Math.Sign(opState.Min) == 0)
		{
			if (forceZero)
			{
				opState.CalcMin = 0.0;
			}
			else if (opState.CalcMin - (double)(num / 2) * opState.CalcInterval <= 0.0)
			{
				opState.CalcMin = 0.0;
			}
		}
	}

	protected virtual void TweakBoundaries(OpState opState)
	{
		if (opState.CalcMin == opState.Min && (!forceZero || opState.CalcMin != 0.0))
		{
			opState.CalcMin -= opState.CalcInterval;
		}
		if (opState.CalcMax == opState.Max && (!forceZero || opState.CalcMax != 0.0))
		{
			opState.CalcMax += opState.CalcInterval;
		}
	}
}
