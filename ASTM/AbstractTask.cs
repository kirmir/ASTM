using System;
using System.Threading;

namespace ASTM
{
	/// <summary>
	/// Possible task's states values.
	/// </summary>
	public enum TaskState
	{
		Active, Inactive, Paused
	}

	/// <summary>
	/// Possible task's modes values.
	/// </summary>
	public enum TaskMode
	{
		Running, WaitForRun, WaitForReady, Interrupted, Disabled, Paused
	}

	/// <summary>
	/// Task type.
	/// </summary>
	public enum TaskType
	{
		Interrupt, Time
	}

	/// <summary>
	/// Event arguments of TaskEvent.
	/// </summary>
	public class TaskEventHandlerEventArgs : EventArgs
	{
		/// <summary>
		/// Target task's ID.
		/// </summary>
		private readonly int id;

		/// <summary>
		/// Gets the ID of target task which parameter should be changed on this event.
		/// </summary>
		public int TargetTaskID
		{
			get
			{
				return id;
			}
		}

		/// <summary>
		/// Target task's parameter name.
		/// </summary>
		private readonly string paramName;

		/// <summary>
		/// Gets target task's parameter name which should be changed on this event.
		/// </summary>
		public string TargetTaskParamName
		{
			get
			{
				return paramName;
			}
		}

		/// <summary>
		/// Crates an instance of arguments for TaskEvent handler.
		/// </summary>
		/// <param name="targetID">The ID of target task which parameter should be changed on event.</param>
		/// <param name="targetParamName">Target task's parameter name which should be changed on event.</param>
		public TaskEventHandlerEventArgs(int targetID, string targetParamName)
		{
			id = targetID;
			paramName = targetParamName;
		}
	}

	/// <summary>
	/// Handler delegate for TaskEvent that indicates when need to change specified parameter of another task.
	/// </summary>
	/// <param name="sender">Task-sender.</param>
	/// <param name="e">Target task and parameter.</param>
	public delegate void TaskEventHandler(AbstractTask sender, TaskEventHandlerEventArgs e);

	/// <summary>
	/// Event arguments of TaskDrawStateEvent event.
	/// </summary>
	public class TaskDrawStateEventHandlerEventArgs : EventArgs
	{
		/// <summary>
		/// Time when task had changed state.
		/// </summary>
		private readonly int time;

		/// <summary>
		/// Gets the time when task had changed state and need to draw it.
		/// </summary>
		public int StateTime
		{
			get
			{
				return time;
			}
		}

		/// <summary>
		/// Task's state value in specified time.
		/// </summary>
		private readonly int state;

		/// <summary>
		/// Gets task's state value in specified time.
		/// </summary>
		public int StateValue
		{
			get
			{
				return state;
			}
		}

		/// <summary>
		/// Crates an instance of arguments for TaskDrawStateEvent event handler.
		/// </summary>
		/// <param name="stateTime">Time when task had changed state.</param>
		/// <param name="stateValue">Task's state value in specified time.</param>
		public TaskDrawStateEventHandlerEventArgs(int stateTime, int stateValue)
		{
			time = stateTime;
			state = stateValue;
		}
	}

	/// <summary>
	/// Handler delegate for TaskDrawStateEvent event that indicates when need to draw specified task state.
	/// </summary>
	/// <param name="sender">Task-sender.</param>
	/// <param name="e">Target state value in specified time.</param>
	public delegate void TaskDrawStateEventHandler(AbstractTask sender, TaskDrawStateEventHandlerEventArgs e);

	/// <summary>
	/// Represents abstract system task.
	/// </summary>
	public abstract class AbstractTask
	{
		/// <summary>
		/// Task's ID.
		/// </summary>
		private readonly int id;

		/// <summary>
		/// Gets task's unique ID.
		/// </summary>
		public int ID
		{
			get
			{
				return id;
			}
		}

		/// <summary>
		/// Task type.
		/// </summary>
		protected TaskType TypeOfTask = TaskType.Interrupt;

		/// <summary>
		/// Gets type of the task.
		/// </summary>
		public TaskType Type
		{
			get
			{
				return TypeOfTask;
			}
		}

		/// <summary>
		/// Gets or sets task's name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets task's critical running time (max duration of running).
		/// </summary>
		public int CriticalRunTime { get; set; }

		/// <summary>
		/// Gets or sets task's session time (runninng duration).
		/// </summary>
		public int SessionTime { get; set; }

		/// <summary>
		/// Gets or sets task's priority.
		/// </summary>
		public int Priority { get; set; }

		/// <summary>
		/// Gets or sets task's start time of active state.
		/// </summary>
		public int StartTime { get; set; }

		/// <summary>
		/// Gets or sets task's state.
		/// </summary>
		public TaskState State { get; set; }

		/// <summary>
		/// Gets or sets task's running mode.
		/// </summary>
		public TaskMode Mode { get; set; }

		/// <summary>
		/// Running duration of current session.
		/// </summary>
		public int RunningTime { get; private set; }

		/// <summary>
		/// Shows how many times task was executed.
		/// </summary>
		public int SessionsCount { get; private set; }

		/// <summary>
		/// Model time when task was interrupted.
		/// </summary>
		public int TimeWhenInterrupted { get; set; }

		/// <summary>
		/// Model time when task became ready for run and started waiting.
		/// </summary>
		public int TimeWhenBecomeWaitForRun { get; set; }

		/// <summary>
		/// Synchronization object for waiting model timer ticks and getting current model time.
		/// </summary>
		protected readonly SyncThreads Sync;

		/// <summary>
		/// Synchronization event for "suspending" thread.
		/// </summary>
		private readonly EventWaitHandle suspendSignal = new EventWaitHandle(true, EventResetMode.ManualReset);

		/// <summary>
		/// Task's thread from doing main work on the background of main model thread.
		/// </summary>
		private Thread taskThread;

		/// <summary>
		/// Initializes a new instance of the abstract system task class.
		/// </summary>
		/// <param name="taskID">Task's unique identifier (ID).</param>
		/// <param name="name">Name of the task.</param>
		/// <param name="managerSync">Synchronization object for waiting model timer ticks.</param>
		protected AbstractTask(int taskID, string name, SyncThreads managerSync)
		{
			id = taskID;
			Name = name;
			Sync = managerSync;

			//Default values for some task's parameters.
			StartTime = -1;
			State = TaskState.Active;
			Mode = TaskMode.WaitForRun;
		}

		/// <summary>
		/// Start task work thread.
		/// </summary>
		public void Start()
		{
			taskThread = new Thread(threadFunc) { IsBackground = true };
			taskThread.SetApartmentState(ApartmentState.STA);
			RunningTime = 0;
			SessionsCount++;
			suspendSignal.Set();
			taskThread.Start();
			doStart();
		}

		/// <summary>
		/// Start function for overriding.
		/// </summary>
		protected abstract void doStart();

		/// <summary>
		/// Stop task work thread.
		/// </summary>
		public void Stop()
		{
			suspendSignal.Reset();
			if (taskThread != null)
			{
				taskThread.Abort();
				taskThread.Join();
			}
			doStop();
		}

		/// <summary>
		/// Stop task work thread.
		/// </summary>
		protected abstract void doStop();

		/// <summary>
		/// Reset parameters.
		/// </summary>
		public void Reset()
		{
			SessionsCount = 0;
			doReset();
		}

		/// <summary>
		/// Parameters reset function for overriding.
		/// </summary>
		protected abstract void doReset();

		/// <summary>
		/// Pause task work thread.
		/// </summary>
		public void Pause()
		{
			suspendSignal.Reset();
			TimeWhenInterrupted = Sync.CurrentModelTime;
			doPause();
		}

		/// <summary>
		/// Pause function for overriding.
		/// </summary>
		protected abstract void doPause();

		/// <summary>
		/// Indicates that code must be executed on current synchronization timer tick.
		/// </summary>
		private bool doWorkOnTick = true;

		/// <summary>
		/// Continue task work thread.
		/// </summary>
		public void Continue(bool afterInterrupt = false)
		{
			if (afterInterrupt)
				doWorkOnTick = false;
			Sync.ModelTimerTickEvent.Reset();
			suspendSignal.Set();
		}

		/// <summary>
		/// Task's thread function.
		/// </summary>
		private void threadFunc()
		{
			do
			{
				Sync.ModelTimerTickEvent.WaitOne();
				suspendSignal.WaitOne();
				if (doWorkOnTick)
				{
					RunningTime++;
					DoWork();
				}
				else
				{
					doWorkOnTick = true;
				}
			}
			while (true);
		}

		/// <summary>
		/// Work function for task thread function.
		/// </summary>
		protected abstract void DoWork();

		/// <summary>
		/// Task's event that indicates when need to change certain property of target task.
		/// </summary>
		public event TaskEventHandler TaskEvent;

		/// <summary>
		/// Raises task event.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		protected void OnTaskEvent(TaskEventHandlerEventArgs e)
		{
			if (TaskEvent != null)
				TaskEvent(this, e);
		}

		/// <summary>
		/// Task's event that indicates when need to change certain property of target task.
		/// </summary>
		public event TaskDrawStateEventHandler TaskDrawStateEvent;

		/// <summary>
		/// Raises TaskDrawStateEvent task event.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		protected void OnTaskDrawStateEvent(TaskDrawStateEventHandlerEventArgs e)
		{
			if (TaskDrawStateEvent != null)
				TaskDrawStateEvent(this, e);
		}
	}
}
