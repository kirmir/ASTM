using System.Collections.Generic;
using System.Threading;

namespace ASTM
{
	/// <summary>
	/// Class of tasks threads synchronization object with main program thread.
	/// </summary>
	public class SyncThreads
	{
		/// <summary>
		/// Thread synchronization event.
		/// </summary>
		private readonly EventWaitHandle timerTick;

		/// <summary>
		/// Gets thread synchronization event.
		/// </summary>
		public EventWaitHandle ModelTimerTickEvent
		{
			get
			{
				return timerTick;
			}
		}

		/// <summary>
		/// Gets or sets current model time.
		/// </summary>
		public int CurrentModelTime { set; get; }

		/// <summary>
		/// Dictionary of function values and arguments (as keys), calculated by task.
		/// </summary>
		public List<double> Data { get; private set; }

		/// <summary>
		/// Function middle value.
		/// </summary>
		private readonly double functionMiddleValue;

		/// <summary>
		/// Gets function middle value.
		/// </summary>
		public double FunctionDescreteMiddleValue
		{
			get
			{
				return functionMiddleValue;
			}
		}

		/// <summary>
		/// Creates an instance of tasks threads synchronization object.
		/// </summary>
		/// <param name="funcMiddleVal">Function middle value.</param>
		public SyncThreads(double funcMiddleVal)
		{
			timerTick = new AutoResetEvent(false);
			Data = new List<double>();
			functionMiddleValue = funcMiddleVal;
		}
	}
}
