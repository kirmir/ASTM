using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ASTM
{
	/// <summary>
	/// Tasks manager working statuses.
	/// </summary>
	public enum ManagerStatus
	{
		Stopped, Running, Paused, Done
	}

	/// <summary>
	/// Event arguments manager's requests.
	/// </summary>
	public class ManagerRequestEventHandlerEventArgs : EventArgs
	{
		/// <summary>
		/// Target task's ID.
		/// </summary>
		private readonly int id;

		/// <summary>
		/// Gets the ID of the request target task.
		/// </summary>
		public int TargetTaskID
		{
			get
			{
				return id;
			}
		}

		/// <summary>
		/// Request time position.
		/// </summary>
		private readonly int timePos;

		/// <summary>
		/// Gets request time position.
		/// </summary>
		public int TimePosition
		{
			get
			{
				return timePos;
			}
		}

		/// <summary>
		/// Crates an instance of the arguments for ManagerRequestEvent handler.
		/// </summary>
		/// <param name="targetID">The ID of target task which parameter should be changed on event.</param>
		/// <param name="time">Request time position.</param>
		public ManagerRequestEventHandlerEventArgs(int targetID, int time)
		{
			id = targetID;
			timePos = time;
		}
	}

	/// <summary>
	/// Handler delegate for ManagerRequestEvent that indicates when manager ask's user for actions.
	/// </summary>
	/// <param name="e">Target task and time.</param>
	public delegate void ManagerRequestEventHandler(ManagerRequestEventHandlerEventArgs e);

	/// <summary>
	/// Task manager of abstract system that controls system time and working of all tasks.
	/// </summary>
	public class TaskManager : INotifyPropertyChanged
	{
		#region Init const data.

		/// <summary>
		/// Interval in seconds between next step execution. Used also for animation speed.
		/// </summary>
		public const int TickInterval = 1;

		/// <summary>
		/// Function maximal value.
		/// </summary>
		public const double FuncMax = 1.571;

		/// <summary>
		/// Function minimal value.
		/// </summary>
		public const double FuncMin = 0.197;

		/// <summary>
		/// Function maximal value argument.
		/// </summary>
		public const double FuncMaxArg = 0;

		/// <summary>
		/// Function minimal value argument.
		/// </summary>
		public const double FuncMinArg = 13;

		/// <summary>
		/// Max time of manager working (and diagram).
		/// </summary>
		public const int MaxWorkingTime = 65;

		/// <summary>
		/// Signal graphic function.
		/// </summary>
		/// <param name="t">Argument (time).</param>
		/// <returns>Function value in moment t.</returns>
		public static double GraphicFunc(double t)
		{
			return ExtMath.Acot((t % 13) / 0.65 / 4);
		}

		#endregion

		/// <summary>
		/// Manager timer that synchronize tasks manager work.
		/// </summary>
		private readonly DispatcherTimer timer;

		/// <summary>
		/// Abstract system tasks.
		/// </summary>
		public List<AbstractTask> Tasks { get; private set; }

		/// <summary>
		/// Gets current internal manager time.
		/// </summary>
		public int CurrentTime { get; private set; }

		/// <summary>
		/// Gets tasks manager current working status.
		/// </summary>
		public ManagerStatus Status { get; private set; }

		/// <summary>
		/// Draws diagram of tasks activity, signal funtion and points on it.
		/// </summary>
		public readonly GraphDrawer Drawer;

		/// <summary>
		/// Synchronization object to send to time tasks threads timer tick event.
		/// </summary>
		private readonly SyncThreads sync;

		/// <summary>
		/// Currently running task.
		/// </summary>
		private AbstractTask activeTask;

		/// <summary>
		/// Creates tasks manager that controls tasks working.
		/// </summary>
		/// <param name="drawingArea">Collection for visual elements of diagram which must be drawen.</param>
		public TaskManager(UIElementCollection drawingArea)
		{
			//Set status.
			Status = ManagerStatus.Stopped;

			sync = new SyncThreads((FuncMin + FuncMax) / 2);

			//Creates information for drawer.
			TaskInfo[] info = new[]
			                  	{
			                  		new TaskInfo(0, "Временная задача 1", Color.FromArgb(240, 75, 100, 255)),
			                  		new TaskInfo(1, "Временная задача 2", Color.FromArgb(240, 75, 175, 0)),
			                  		new TaskInfo(2, "Задача по прерыванию", Color.FromArgb(240, 200, 100, 0)),
			                  	};


			//Add tasks.
			Tasks = new List<AbstractTask>
			        	{
			        		new TimeTask1(info[0].ID, info[0].Name, sync),
			        		new TimeTask2(info[1].ID, info[1].Name, sync)
			        	};

			//Set events handlers.
			foreach (AbstractTimeTask task in Tasks)
			{
				task.TaskDrawStateEvent += taskDrawStateEventHandler;
				task.TimeTaskIOEvent += timeTaskIOEventHandler;
				task.TaskEvent += taskEventHandler;
			}

			//Create drawer of diagram.
			Drawer = new GraphDrawer(MaxWorkingTime, info, TickInterval, drawingArea, GraphicFunc,
				FuncMax, FuncMin, FuncMaxArg, FuncMinArg);

			//Creates timer.
			timer = new DispatcherTimer();
			timer.Tick += timer_Tick;
			timer.Interval = new TimeSpan(0, 0, TickInterval);
		}

		/// <summary>
		/// Creates an interrupt task and start it.
		/// </summary>
		/// <param name="startTime">Time when task must start.</param>
		/// <param name="criticalRunTime">Max time of running.</param>
		/// <param name="sessionTime">Running duration.</param>
		public void AddInterruptTask(int startTime, int criticalRunTime, int sessionTime)
		{
			InterruptTask task = new InterruptTask(Drawer.TasksInfo[2].ID, Drawer.TasksInfo[2].Name, sync)
			                     	{
			                     		CriticalRunTime = criticalRunTime,
			                     		SessionTime = sessionTime,
			                     		StartTime = startTime
			                     	};
			task.TaskDrawStateEvent += taskDrawStateEventHandler;
			Tasks.Add(task);
			Drawer.DrawMarker(task.ID, task.StartTime, MarkerType.StartTime);
		}

		/// <summary>
		/// Tasks event drawing handler. Need to draw specified state on the graph.
		/// </summary>
		/// <param name="sender">Task-sender of event.</param>
		/// <param name="e">State parameters.</param>
		private void taskDrawStateEventHandler(AbstractTask sender, TaskDrawStateEventHandlerEventArgs e)
		{
			Action action = () => Drawer.DrawNextPos(e.StateValue, sender.ID, e.StateTime);
			Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, action);
		}

		/// <summary>
		/// Handler for TimeTaskIOEvent that indicates when need to draw IO operation representation.
		/// </summary>
		/// <param name="sender">Task-sender.</param>
		/// <param name="e">Operation type and value.</param>
		private void timeTaskIOEventHandler(AbstractTimeTask sender, TimeTaskIOEventHandlerEventArgs e)
		{
			Action action;
			if (e.ReadOperation)
			{
				action = () => Drawer.DrawMarker(sender.ID, e.TimePosition, MarkerType.ReadPoint);
			}
			else
			{
				action = () => Drawer.DrawNextDescreteValue(e.TimePosition, e.WriteValue ? 1 : -1);
			}

			//Execute code.
			Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, action);
		}

		/// <summary>
		/// Handler for the TaskEvent that indicates when need to change specified parameter of another task.
		/// </summary>
		/// <param name="sender">Task-sender.</param>
		/// <param name="e">Target task and parameter.</param>
		public void taskEventHandler(AbstractTask sender, TaskEventHandlerEventArgs e)
		{
			Action action = delegate
			                {
			                	AbstractTimeTask targetTask =
									(AbstractTimeTask)Tasks.First(task => task.ID == e.TargetTaskID);
			                	switch (e.TargetTaskParamName)
			                	{
			                		case "Tk":
										if (targetTask.State == TaskState.Active)
										{
											targetTask.StopTime = CurrentTime;
											//Draw marker.
											Drawer.DrawMarker(targetTask.ID, CurrentTime, MarkerType.StopTime);
										}
			                			break;
			                		case "Pause":
										if (targetTask.State == TaskState.Active)
										{
											targetTask.State = TaskState.Paused;
											targetTask.Pause();
											//Draw marker.
											Drawer.DrawMarker(targetTask.ID, CurrentTime, MarkerType.Pause);
										}
			                			break;
			                	}
								//Draw event marker.
								Drawer.DrawMarker(sender.ID, CurrentTime, MarkerType.Event);
			                };
			Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, action);
		}

		//Timer tick handler.
		private void timer_Tick(object sender, EventArgs e)
		{
			CurrentTime++;
			//Raise update event.
			OnPropertyChanged(new PropertyChangedEventArgs("CurrentTime"));

			if (CurrentTime >= MaxWorkingTime)
			{
				timer.Stop();
				Status = ManagerStatus.Done;
				Drawer.HighlightAxis(CurrentTime);
				//Raise an event of working done.
				Drawer.DrawNextDescreteValue(MaxWorkingTime, 0);
				OnManagerDone();
				return;
			}

			doTimerWork();
			
			Drawer.HighlightAxis(CurrentTime);
			//Raise tick event.
			OnManagerTimerTick();
		}

		/// <summary>
		/// Work that manager do every timer tick.
		/// </summary>
		private void doTimerWork()
		{
			//Update synchronization object.
			sync.CurrentModelTime = CurrentTime;

			foreach (AbstractTimeTask task in Tasks.Where(task => task.Type == TaskType.Time))
			{
				if (task.State == TaskState.Inactive && task.Mode == TaskMode.Disabled &&
					task.StartTime == CurrentTime)
				{
					task.State = TaskState.Active;
					task.Mode = TaskMode.WaitForRun;
					task.TimeWhenBecomeWaitForRun = CurrentTime;
				}
				else if (task.State == TaskState.Active && task.StopTime > 0 && CurrentTime >= task.StopTime)
				{
					task.State = TaskState.Inactive;
					task.Mode = TaskMode.Disabled;
					task.Stop();
					TimeTask2 task2 = task as TimeTask2;
					if (task2 != null)
					{
						task2.SessionTime--;
					}
					if (activeTask == task)
					{
						activeTask = null;
					}
				}
				else if (task.State == TaskState.Active && task.Mode == TaskMode.WaitForReady &&
					((CurrentTime - task.StartTime) % task.PeriodTime == 0))
				{
					task.Mode = TaskMode.WaitForRun;
					task.TimeWhenBecomeWaitForRun = CurrentTime;
				}
			}

			if (activeTask != null)
			{
				if (activeTask.Type == TaskType.Interrupt)
				{
					//Interrupt has done his work.
					if (activeTask.RunningTime == activeTask.SessionTime)
					{
						//Stop and remove interrupt task from tasks list.
						activeTask.Stop();
						Drawer.DrawMarker(activeTask.ID, CurrentTime, MarkerType.StopTime);
						Tasks.Remove(activeTask);
						activeTask = null;
					}
				}
				else
				{
					if (activeTask.RunningTime == activeTask.SessionTime)
					{
						activeTask.Mode = TaskMode.WaitForReady;
						activeTask.Stop();
						activeTask = null;
					}

					//Search for interrupts.
					AbstractTask intTask = Tasks.FirstOrDefault(task =>
					                                            task.State == TaskState.Active &&
																task.Mode == TaskMode.WaitForRun &&
					                                            task.Type == TaskType.Interrupt);

					//Interrupt current time task and start interrupt task.
					if (intTask != null && activeTask != null && activeTask.Type == TaskType.Time)
					{
						activeTask.Mode = TaskMode.Interrupted;
						activeTask.Pause();
						//Fix the problem with threads synchronization.
						sync.ModelTimerTickEvent.Set();
						Drawer.DrawMarker(activeTask.ID, CurrentTime, MarkerType.Interrupt);
						activeTask = null;
					}
				}
			}

			if (activeTask == null)
			{
				//Search task that will be started.
				AbstractTask taskToStart = null;
				foreach (AbstractTask task in Tasks)
				{
					//Check only active and "ready" tasks.
					if (task.State == TaskState.Active &&
						(task.Mode == TaskMode.WaitForRun || task.Mode == TaskMode.Interrupted))
					{
						if (taskToStart == null)
						{
							taskToStart = task;
						}
						else
						{
							//Both tasks are time tasks.
							if (taskToStart.Type == TaskType.Time && task.Type == TaskType.Time)
							{
								//Interrupted task has higher priority that planned to start task.
								if (task.Mode == TaskMode.Interrupted && taskToStart.Mode != TaskMode.Interrupted)
								{
									taskToStart = task;
								}
								else if (task.Mode != TaskMode.Interrupted && taskToStart.Mode != TaskMode.Interrupted
									&& task.Priority < taskToStart.Priority)
								{
									taskToStart = task;
								}
							}
							else if (taskToStart.Type == TaskType.Time && task.Type == TaskType.Interrupt)
							{
								//Interrupt has always highest priority.
								taskToStart = task;
							}
						}
					}
				}

				//Start task.
				if (taskToStart != null)
				{
					activeTask = taskToStart;
					if (activeTask.Mode == TaskMode.Interrupted)
					{
						activeTask.Continue(true);
						Drawer.DrawMarker(activeTask.ID, CurrentTime, MarkerType.Continue);
					}
					else
					{
						activeTask.Start();
					}
					activeTask.Mode = TaskMode.Running;
				}
			}

			foreach (AbstractTimeTask task in Tasks.Where(task =>
				task.Type == TaskType.Time && task.State == TaskState.Active))
			{
				if (task.Mode == TaskMode.WaitForRun && 
					CurrentTime - task.TimeWhenBecomeWaitForRun >= task.CriticalDelayTime)
				{
					task.State = TaskState.Paused;
					task.Mode = TaskMode.Disabled;
					Drawer.DrawMarker(task.ID, CurrentTime, MarkerType.Request);
					OnManagerRequestEvent(new ManagerRequestEventHandlerEventArgs(task.ID, CurrentTime));
				}
				else if (task.Mode == TaskMode.Running && task.RunningTime >= task.CriticalRunTime)
				{
					//Break task that work more time than available.
					task.Mode = TaskMode.WaitForReady;
					task.Stop();
					TimeTask2 task2 = task as TimeTask2;
					if (task2 != null)
					{
						task2.SessionTime = task2.SessionTimeBeforeStart;
					}
					//Draw break marker.
					Drawer.DrawMarker(task.ID, CurrentTime, MarkerType.Break);

					activeTask = null;
				}
			}

			//Draw tasks in WaitForRun states.
			foreach (AbstractTask task in Tasks.Where(task =>
				task.Type == TaskType.Time && task.State == TaskState.Active &&
				(task.Mode == TaskMode.WaitForRun || task.Mode == TaskMode.Interrupted)))
			{
				Drawer.DrawNextPos(GraphDrawer.SmallBarHeight, task.ID, CurrentTime);
			}

			//Synchronize task's threads with timer (only if any active available).
			if (activeTask != null)
			{
				sync.ModelTimerTickEvent.Set();
			}
		}

		/// <summary>
		/// Start task manager.
		/// </summary>
		public void Start()
		{
			if (Status == ManagerStatus.Stopped)
			{
				//Reset data.
				CurrentTime = 0;
				Drawer.NewDrawing();
				//Raise update event.
				OnPropertyChanged(new PropertyChangedEventArgs("CurrentTime"));
				//Raise an event of starting work.
				OnManagerStarted();
				//Update markers.
				foreach (AbstractTask task in Tasks)
				{
					DrawPlannedPoints(task.ID);
				}
			}
			else
			{
				//Raise an event of continue work.
				OnManagerContinue();
			}

			//Start working.
			timer.Start();
			doTimerWork();
			//Set status.
			Status = ManagerStatus.Running;
		}

		/// <summary>
		/// Stop task manager.
		/// </summary>
		public void Stop()
		{
			//Stop working.
			timer.Stop();
			//Stop all tasks.
			foreach (AbstractTask task in Tasks)
			{
				task.Stop();
				task.Reset();
				task.State = TaskState.Inactive;
				task.Mode = TaskMode.Disabled;
			}
			//Remove all interrupts.
			Tasks.RemoveAll(task => task.Type == TaskType.Interrupt);
			//Reset data.
			CurrentTime = 0;
			activeTask = null;
			sync.CurrentModelTime = 0;
			sync.Data.Clear();
			Drawer.NewDrawing();
			//Raise update event.
			OnPropertyChanged(new PropertyChangedEventArgs("CurrentTime"));
			//Set status.
			Status = ManagerStatus.Stopped;
			//Raise an event of stopping work.
			OnManagerStopped();
		}

		/// <summary>
		/// Pause task manager.
		/// </summary>
		public void Pause()
		{
			if (CurrentTime >= MaxWorkingTime - 1)
				return;

			CurrentTime++;
			//Raise update event.
			OnPropertyChanged(new PropertyChangedEventArgs("CurrentTime"));
			
			Drawer.HighlightAxis(CurrentTime);

			//Stop working.
			timer.Stop();
			//Set status.
			Status = ManagerStatus.Paused;
			//Raise an event of pausing work.
			OnManagerPaused();
		}

		/// <summary>
		/// Executes drawer functions and sets visual points of task's running planned times, start and stop time.
		/// </summary>
		/// <param name="taskID">The ID of task which planned times must be drawed.</param>
		public void DrawPlannedPoints(int taskID)
		{
			//Find task with specified ID.
			AbstractTimeTask task = null;
			foreach (AbstractTask abstractTask in Tasks)
			{
				if (abstractTask.ID == taskID)
				{
					if ((abstractTask as AbstractTimeTask) != null)
					{
						task = (AbstractTimeTask)abstractTask;
						break;
					}
					return;
				}
			}

			if (task == null)
				return;

			//Draw markers.
			if (task.StartTime >= 0 && task.StartTime < MaxWorkingTime)
			{
				//Draw start time marker.
				Drawer.DrawMarker(task.ID, task.StartTime, MarkerType.StartTime);

				//Draw planned time periodic markers.
				int lastTime = (task.StopTime >= 0 && task.StopTime < MaxWorkingTime) ? task.StopTime : MaxWorkingTime;
				int curTime = task.StartTime + task.PeriodTime;
				while (curTime < lastTime)
				{
					Drawer.DrawMarker(task.ID, curTime, MarkerType.PlannedTime);
					curTime += task.PeriodTime;
				}
			}

			//Draw stop time marker.
			if (task.StopTime >= 0 && task.StopTime < MaxWorkingTime)
			{
				Drawer.DrawMarker(task.ID, task.StopTime, MarkerType.StopTime);
			}
		}

		#region Events.

		/// <summary>
		/// Property changing event for updating data bindings.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Raises properties changing event.
		/// </summary>
		/// <param name="e">Name of the updated property.</param>
		public void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, e);
		}

		/// <summary>
		/// Manager starting work event.
		/// </summary>
		public event EventHandler ManagerStarted;

		/// <summary>
		/// Raises manager starting work event.
		/// </summary>
		public void OnManagerStarted()
		{
			if (ManagerStarted != null)
				ManagerStarted(this, null);
		}

		/// <summary>
		/// Manager pausing work event.
		/// </summary>
		public event EventHandler ManagerPaused;

		/// <summary>
		/// Raises manager pausing work event.
		/// </summary>
		public void OnManagerPaused()
		{
			if (ManagerPaused != null)
				ManagerPaused(this, null);
		}

		/// <summary>
		/// Manager stoping work event.
		/// </summary>
		public event EventHandler ManagerStopped;

		/// <summary>
		/// Raises manager stoping work event.
		/// </summary>
		public void OnManagerStopped()
		{
			if (ManagerStopped != null)
				ManagerStopped(this, null);
		}

		/// <summary>
		/// Manager continue work event.
		/// </summary>
		public event EventHandler ManagerContinue;

		/// <summary>
		/// Raises manager continue work event.
		/// </summary>
		public void OnManagerContinue()
		{
			if (ManagerContinue != null)
				ManagerContinue(this, null);
		}

		/// <summary>
		/// Manager working done event.
		/// </summary>
		public event EventHandler ManagerDone;

		/// <summary>
		/// Raises manager working done event.
		/// </summary>
		public void OnManagerDone()
		{
			if (ManagerDone != null)
				ManagerDone(this, null);
		}

		/// <summary>
		/// Internal timer tick event.
		/// </summary>
		public event EventHandler ManagerTimerTick;

		/// <summary>
		/// Raises manager timer tick event.
		/// </summary>
		public void OnManagerTimerTick()
		{
			if (ManagerTimerTick != null)
				ManagerTimerTick(this, null);
		}

		/// <summary>
		/// Manager request event that indicates when need to ask user for actions.
		/// </summary>
		public event ManagerRequestEventHandler ManagerRequestEvent;

		/// <summary>
		/// Raises manager request event.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		protected void OnManagerRequestEvent(ManagerRequestEventHandlerEventArgs e)
		{
			if (ManagerRequestEvent != null)
				ManagerRequestEvent(e);
		}

		#endregion
	}
}
