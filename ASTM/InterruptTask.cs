namespace ASTM
{
	/// <summary>
	/// Model interrupt task class.
	/// </summary>
	class InterruptTask : AbstractTask
	{
		/// <summary>
		/// Creates an instance of interrupt task class.
		/// </summary>
		/// <param name="taskID">Unique ID that will be applyed to new object.</param>
		/// <param name="name">Name of task for displaying in GUI.</param>
		/// <param name="managerSync">Threads synchronization object.</param>
		public InterruptTask(int taskID, string name, SyncThreads managerSync)
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
		}

		/// <summary>
		/// Resets parameters.
		/// </summary>
		protected override void doReset()
		{
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
