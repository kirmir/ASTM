
namespace ASTM
{
	/// <summary>
	/// Model time task 2 class.
	/// </summary>
	public class TimeTask2 : AbstractTimeTask
	{
		/// <summary>
		/// Value that defines how frequent task will generate event for setting task's 1 stop time.
		/// </summary>
		public int EventTimeForTask1Tk { set; get; }

		/// <summary>
		/// Value that defines how frequent task will generate event for setting task's 1 pause time.
		/// </summary>
		public int EventTimeForTask1Pause { set; get; }

		/// <summary>
		/// Task's session time value before start. Need for reseting, because session time
		/// changes during modeling.
		/// </summary>
		public int SessionTimeBeforeStart { set; get; }

		/// <summary>
		/// Creates an instance of time task 2 class.
		/// </summary>
		/// <param name="taskID">Unique ID that will be applyed to new object.</param>
		/// <param name="name">Name of task for displaying in GUI.</param>
		/// <param name="managerSync">Threads synchronization object.</param>
		public TimeTask2(int taskID, string name, SyncThreads managerSync)
			: base(taskID, name, managerSync)
		{
			EventTimeForTask1Tk = -1;
			EventTimeForTask1Pause = -1;
		}

		/// <summary>
		/// Code that will be executed after each model timer tick.
		/// </summary>
		protected override void DoWork()
		{
			//Drawing function.
			OnTaskDrawStateEvent(new TaskDrawStateEventHandlerEventArgs(Sync.CurrentModelTime, GraphDrawer.BigBarHeight));
			//IO operation.
			if (Sync.Data.Count > 0)
			{
				OnTimeTaskIOEvent(new TimeTaskIOEventHandlerEventArgs(Sync.CurrentModelTime,
					false, Sync.Data[0] > Sync.FunctionDescreteMiddleValue));
				Sync.Data.RemoveAt(0);
			}
		}

		/// <summary>
		/// Resets parameters.
		/// </summary>
		protected override void doReset()
		{
			StartTime = -1;
			SessionTime = SessionTimeBeforeStart;
		}

		/// <summary>
		/// Task pause.
		/// </summary>
		protected override void doPause()
		{
		}

		/// <summary>
		/// Start working.
		/// </summary>
		protected override void doStart()
		{
			if (SessionsCount % EventTimeForTask1Tk == 0)
				OnTaskEvent(new TaskEventHandlerEventArgs(0, "Tk"));
			if (SessionsCount % EventTimeForTask1Pause == 0)
				OnTaskEvent(new TaskEventHandlerEventArgs(0, "Pause"));
		}

		/// <summary>
		/// Stop working.
		/// </summary>
		protected override void doStop()
		{
			SessionTime++;
		}
	}
}
