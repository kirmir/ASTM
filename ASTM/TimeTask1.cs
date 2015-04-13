namespace ASTM
{
	/// <summary>
	/// Model time task 1 class.
	/// </summary>
	public class TimeTask1 : AbstractTimeTask
	{
		/// <summary>
		/// Creates an instance of time task 1 class.
		/// </summary>
		/// <param name="taskID">Unique ID that will be applyed to new object.</param>
		/// <param name="name">Name of task for displaying in GUI.</param>
		/// <param name="managerSync">Threads synchronization object.</param>
		public TimeTask1(int taskID, string name, SyncThreads managerSync)
			: base(taskID, name, managerSync)
		{
		}

		/// <summary>
		/// Code that will be executed after each model timer tick.
		/// </summary>
		protected override void DoWork()
		{
			//Drawing function.
			OnTaskDrawStateEvent(new TaskDrawStateEventHandlerEventArgs(Sync.CurrentModelTime, GraphDrawer.BigBarHeight));
			//IO function.
			Sync.Data.Add(TaskManager.GraphicFunc(Sync.CurrentModelTime));
			OnTimeTaskIOEvent(new TimeTaskIOEventHandlerEventArgs(Sync.CurrentModelTime, true, false));
		}

		/// <summary>
		/// Resets parameters.
		/// </summary>
		protected override void doReset()
		{
			StartTime = -1;
			StopTime = -1;
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
		}

		/// <summary>
		/// Stop working.
		/// </summary>
		protected override void doStop()
		{
		}
	}
}
