using System;

namespace ASTM
{
	/// <summary>
	/// Time task's argument of read/write operation event.
	/// </summary>
	public class TimeTaskIOEventHandlerEventArgs : EventArgs
	{
		/// <summary>
		/// Operation time.
		/// </summary>
		private readonly int time;

		/// <summary>
		/// Gets time when operation was done.
		/// </summary>
		public int TimePosition
		{
			get
			{
				return time;
			}
		}

		/// <summary>
		/// Shows type of operation (True if its read).
		/// </summary>
		private readonly bool read;

		/// <summary>
		/// Gets the operation type (True if its read operation).
		/// </summary>
		public bool ReadOperation
		{
			get
			{
				return read;
			}
		}

		/// <summary>
		/// Descrete value for write operation.
		/// </summary>
		private readonly bool value;

		/// <summary>
		/// Gets descrete value of write operation.
		/// </summary>
		public bool WriteValue
		{
			get
			{
				return value;
			}
		}

		/// <summary>
		/// Creates an instance of task IO event arguments.
		/// </summary>
		/// <param name="timePos">Time when operation was done.</param>
		/// <param name="readOp">Read opearion or not.</param>
		/// <param name="writeValue">Descrete value of write operation.</param>
		public TimeTaskIOEventHandlerEventArgs(int timePos, bool readOp, bool writeValue)
		{
			read = readOp;
			value = writeValue;
			time = timePos;
		}
	}

	/// <summary>
	/// Handler delegate for TimeTaskIOEvent that indicates when need to draw IO operation representation.
	/// </summary>
	/// <param name="sender">Task-sender.</param>
	/// <param name="e">Operation type and value.</param>
	public delegate void TimeTaskIOEventHandler(AbstractTimeTask sender, TimeTaskIOEventHandlerEventArgs e);

	/// <summary>
	/// Represents abstract system time task.
	/// </summary>
	public abstract class AbstractTimeTask : AbstractTask
	{
		/// <summary>
		/// Gets or sets task's period time.
		/// </summary>
		public int PeriodTime { get; set; }

		/// <summary>
		/// Gets or sets task's critical running time (max duration of running).
		/// </summary>
		public int CriticalDelayTime { get; set; }

		/// <summary>
		/// Gets or sets stop time when task will become inactive.
		/// </summary>
		public int StopTime { get; set; }

		/// <summary>
		/// Initializes a new instance of the abstract system time task class.
		/// </summary>
		/// <param name="taskID">Task's unique identifier (ID).</param>
		/// <param name="name">Name of the task.</param>
		/// <param name="managerSync">Synchronization object for waiting model timer ticks.</param>
		protected AbstractTimeTask(int taskID, string name, SyncThreads managerSync)
			: base(taskID, name, managerSync)
		{
			//Default values for some task's parameters.
			StopTime = -1;
			State = TaskState.Inactive;
			Mode = TaskMode.Disabled;

			TypeOfTask = TaskType.Time;
		}

		/// <summary>
		/// Task's IO event that indicates when need to draw IO operation representation.
		/// </summary>
		public event TimeTaskIOEventHandler TimeTaskIOEvent;

		/// <summary>
		/// Raises task IO event.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		protected void OnTimeTaskIOEvent(TimeTaskIOEventHandlerEventArgs e)
		{
			if (TimeTaskIOEvent != null)
				TimeTaskIOEvent(this, e);
		}
	}
}
