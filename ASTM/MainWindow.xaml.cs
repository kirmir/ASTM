using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ASTM
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			AutoScroll = true;

			InitializeComponent();
		}

		#region Commands definition.

		/// <summary>
		/// Start abstract system timer command for menu and buttons.
		/// </summary>
		public static readonly RoutedUICommand StartCommand =
			new RoutedUICommand("_Запуск", "StartCommand", typeof(MainWindow),
								new InputGestureCollection(new InputGesture[]
								                           	{
								                           		new KeyGesture(Key.F5, ModifierKeys.None, "F5")
								                           	}));

		/// <summary>
		/// Stop abstract system timer command for menu and buttons.
		/// </summary>
		public static readonly RoutedUICommand StopCommand =
			new RoutedUICommand("_Остановка", "StopCommand", typeof(MainWindow),
								new InputGestureCollection(new InputGesture[]
								                           	{
								                           		new KeyGesture(Key.F10, ModifierKeys.None, "F10")
								                           	}));

		/// <summary>
		/// Pause abstract system timer command for menu and buttons.
		/// </summary>
		public static readonly RoutedUICommand PauseCommand =
			new RoutedUICommand("_Приостановка", "PauseCommand", typeof(MainWindow),
								new InputGestureCollection(new InputGesture[]
								                           	{
								                           		new KeyGesture(Key.F8, ModifierKeys.None, "F8")
								                           	}));

		/// <summary>
		/// Reset abstract system timer command for menu and buttons.
		/// </summary>
		public static readonly RoutedUICommand ResetCommand =
			new RoutedUICommand("С_брос...", "ResetCommand", typeof(MainWindow));

		/// <summary>
		/// Pause abstract system timer command for menu and buttons.
		/// </summary>
		public static readonly RoutedUICommand HelpCommand =
			new RoutedUICommand("Т_ехническое задание...", "HelpCommand", typeof(MainWindow),
								new InputGestureCollection(new InputGesture[]
								                           	{
								                           		new KeyGesture(Key.F1, ModifierKeys.None, "F1")
								                           	}));

		#endregion

		/// <summary>
		/// Is it need to autoscroll area or not.
		/// </summary>
		public bool AutoScroll { get; set; }

		/// <summary>
		/// Task manager of abstract system that controls system time and working of all tasks.
		/// </summary>
		public TaskManager Manager;

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			//Set current time.
			realTimeBox.Text = DateTime.Now.ToString("HH:mm:ss");
			//Start real time timer.
			DispatcherTimer realTimeTimer = new DispatcherTimer();
			realTimeTimer.Tick += realTimeTimer_Tick;
			realTimeTimer.Interval = new TimeSpan(0, 0, 1);
			realTimeTimer.Start();

			createTaskManager();

			//Show help window.
			HelpWindow dlg = new HelpWindow(this, true);
			dlg.ShowDialog();
		}

		/// <summary>
		/// Creates task manager and sets all event handlers.
		/// </summary>
		private void createTaskManager()
		{
			//Creates TaskManager.
			if (Manager != null)
				Manager.Stop();
			Manager = new TaskManager(graph.Children);
			//Add events handlers.
			Manager.ManagerStarted += Manager_ManagerStarted;
			Manager.ManagerPaused += Manager_ManagerPaused;
			Manager.ManagerStopped += Manager_ManagerStopped;
			Manager.ManagerContinue += Manager_ManagerContinue;
			Manager.ManagerDone += Manager_ManagerDone;
			Manager.ManagerTimerTick += Manager_ManagerTimerTick;
			Manager.ManagerRequestEvent += Manager_ManagerRequest;

			systemTimeTextBox.DataContext = Manager;
		}

		#region Tasks manager events handlers.

		private void Manager_ManagerTimerTick(object sender, EventArgs e)
		{
			if (AutoScroll)
			{
				double curPos = graph.Margin.Left + Manager.CurrentTime * GraphDrawer.PointsPerInterval;
				if (curPos + GraphDrawer.PointsPerInterval > scrollArea.HorizontalOffset + scrollArea.ViewportWidth)
				{
					scrollArea.ScrollToHorizontalOffset(curPos - 3);
				}
			}

			//Update task's parameters on the screen.
			TimeTask1 task1 = (TimeTask1)Manager.Tasks[0];
			taskTime1StateTextBlock.Text = getTaskStateText(task1.State);
			taskTime1ModeTextBlock.Text = getTaskModeText(task1.Mode);

			TimeTask2 task2 = (TimeTask2)Manager.Tasks[1];
			taskTime2TsTextBox.Text = task2.SessionTime.ToString();
			taskTime2StateTextBlock.Text = getTaskStateText(task2.State);
			taskTime2ModeTextBlock.Text = getTaskModeText(task2.Mode);

			if (Manager.Tasks.Count > 2)
			{
				taskIntStateTextBlock.Text = getTaskStateText(Manager.Tasks[2].State);
				taskIntModeTextBlock.Text = getTaskModeText(Manager.Tasks[2].Mode);
			}
			else
			{
				taskIntStateTextBlock.Text = "-";
				taskIntModeTextBlock.Text = "-";
			}
		}

		private void Manager_ManagerStarted(object sender, EventArgs e)
		{
			startTimeTextBox.Text = DateTime.Now.ToString("HH:mm:ss");
			endTimeTextBox.Text = "00:00:00";
			statusBarText.Text = "Работа менеджера задач запущена";

			if (AutoScroll)
			{
				scrollArea.ScrollToHorizontalOffset(0);
			}

			//Set ReadOnly properties.
			parametersReadOnly(true);
		}

		private void Manager_ManagerPaused(object sender, EventArgs e)
		{
			endTimeTextBox.Text = DateTime.Now.ToString("HH:mm:ss");
			statusBarText.Text = "Работа менеджера задач приостановлена";
		}

		private void Manager_ManagerStopped(object sender, EventArgs e)
		{
			startTimeTextBox.Text = "00:00:00";
			endTimeTextBox.Text = "00:00:00";
			statusBarText.Text = "Работа менеджера задач остановлена";
			
			taskTime2TsTextBox.Text = Manager.Tasks[1].SessionTime.ToString();
			//Set visibility of entered parameters.
			taskTime1TnTextBlock.Visibility = Visibility.Collapsed;
			taskTime1TnImages.Visibility = Visibility.Visible;
			taskTime2TnTextBlock.Visibility = Visibility.Collapsed;
			taskTime2TnImages.Visibility = Visibility.Visible;
			
			//Update task's parameters on the screen.
			TimeTask1 task1 = (TimeTask1)Manager.Tasks[0];
			taskTime1StateTextBlock.Text = getTaskStateText(task1.State);
			taskTime1ModeTextBlock.Text = getTaskModeText(task1.Mode);

			TimeTask2 task2 = (TimeTask2)Manager.Tasks[1];
			taskTime2TsTextBox.Text = task2.SessionTime.ToString();
			taskTime2StateTextBlock.Text = getTaskStateText(task2.State);
			taskTime2ModeTextBlock.Text = getTaskModeText(task2.Mode);

			//Set ReadOnly properties.
			parametersReadOnly(false);

			//Hide manager requests.
			requestPanel1.Visibility = Visibility.Collapsed;
			requestPanel2.Visibility = Visibility.Collapsed;
		}

		private void Manager_ManagerContinue(object sender, EventArgs e)
		{
			statusBarText.Text = "Работа менеджера задач возобновлена";
		}

		private void Manager_ManagerDone(object sender, EventArgs e)
		{
			endTimeTextBox.Text = DateTime.Now.ToString("HH:mm:ss");
			CommandManager.InvalidateRequerySuggested();
			statusBarText.Text = "Работа менеджера задач выполнена";
		}

		private void Manager_ManagerRequest(ManagerRequestEventHandlerEventArgs e)
		{
			if (e.TargetTaskID == 0)
			{
				requestPanel1.Visibility = Visibility.Visible;
				statusBarText.Text = "Получен запрос от менеджера касательно временной задачи 1";
				requestPanel1.Margin = new Thickness(e.TimePosition * GraphDrawer.PointsPerInterval +
					graph.Margin.Left - 27, GraphDrawer.PointsBetweenAxes + graph.Margin.Top + 6, 0, 0);
			}
			else
			{
				requestPanel2.Visibility = Visibility.Visible;
				statusBarText.Text = "Получен запрос от менеджера касательно временной задачи 2";
				requestPanel2.Margin = new Thickness(e.TimePosition * GraphDrawer.PointsPerInterval +
					graph.Margin.Left - 27, 2 * GraphDrawer.PointsBetweenAxes + graph.Margin.Top + 6, 0, 0);
			}
			SystemSounds.Beep.Play();
		}

		#endregion

		/// <summary>
		/// Helper function for searching all controls of the specified type.
		/// </summary>
		/// <typeparam name="T">Type of control.</typeparam>
		/// <param name="depObj">Where to look for controls.</param>
		/// <returns>Enumerable list of controls.</returns>
		public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
		{
			if (depObj != null)
			{
				for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
				{
					DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
					if (child != null && child is T)
					{
						yield return (T)child;
					}

					foreach (T childOfChild in FindVisualChildren<T>(child))
					{
						yield return childOfChild;
					}
				}
			}
		}

		/// <summary>
		/// Locks/unlocks tasks parameters text boxes.
		/// </summary>
		/// <param name="isReadOnly">Value of IsReadOnly property of text boxes which will be set.</param>
		private void parametersReadOnly(bool isReadOnly)
		{
			//Configurate color for background.
			SolidColorBrush col = new SolidColorBrush(isReadOnly ?
				Color.FromRgb(240, 240, 240) : Color.FromRgb(255, 255, 255));
			//Enumerate all text boxes.
			foreach (TextBox child in FindVisualChildren<TextBox>(allParametersGrid))
			{
				//Set ReadOnly property.
				child.IsReadOnly = isReadOnly;
				//Set back color.
				child.Background = col;
			}
		}

		//Update real time in the window.
		private void realTimeTimer_Tick(object sender, EventArgs e)
		{
			realTimeBox.Text = DateTime.Now.ToString("HH:mm:ss");
		}

		private void Exit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void SaveDiagram_Click(object sender, RoutedEventArgs e)
		{
			// Configure save file dialog box.
			Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
			                                     	{
			                                     		FileName = "Диаграмма",
			                                     		DefaultExt = ".png",
			                                     		Filter = "Изображения PNG|*.png|Все файлы|*.*"
			                                     	};

			// Show save file dialog box.
			bool? result = dlg.ShowDialog();

			// Process save file dialog box results.
			if (result == false)
			{
				return;
			}

			//Prepare image.
			RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)graph.ActualWidth,
				(int)graph.ActualHeight, 96, 96, PixelFormats.Pbgra32);
			VisualBrush sourceBrush = new VisualBrush(graph);

			DrawingVisual drawingVisual = new DrawingVisual();
			DrawingContext drawingContext = drawingVisual.RenderOpen();

			using (drawingContext)
			{
				drawingContext.DrawRectangle(sourceBrush, null, new Rect(new Point(0, 0),
					new Point(graph.ActualWidth, graph.ActualHeight)));
			}
			renderTarget.Render(drawingVisual);

			PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
			pngEncoder.Frames.Add(BitmapFrame.Create(renderTarget));

			//Save to specified file.
			using (Stream fs = File.Create(dlg.FileName))
			{
				pngEncoder.Save(fs);
			}
		}

		private void About_Click(object sender, RoutedEventArgs e)
		{
			AboutWindow about = new AboutWindow(this);
			about.ShowDialog();
		}

		#region Checking valid numbers entering.

		/// <summary>
		/// Previous value of tasks parameters text boxes.
		/// </summary>
		private string previousParameterValue = "0";

		private void ParamsTextBoxes_GotFocus(object sender, RoutedEventArgs e)
		{
			//Save previous text of current TextBox.
			previousParameterValue = ((TextBox)sender).Text;
		}

		private void ParamsTextBoxes_LostFocus(object sender, RoutedEventArgs e)
		{
			//Check text box for entering only numbers.
			TextBox source = sender as TextBox;
			Regex regex = new Regex("^[0-9]*$");
			if (source.Text == string.Empty || !regex.IsMatch(source.Text))
			{
				ErrorWindow dlg = new ErrorWindow(this, "Ошибка ввода", "Некорректное значение.",
				                                  "Введенный параметр содержит недопустимый символ либо значение.",
				                                  "Параметр " + source.Tag + " имеет физический смысл и должен представлять собой неотрицательное число.");
				dlg.ShowDialog();
				source.Text = previousParameterValue;
			}
		}

		#endregion

		/// <summary>
		/// Gets text equivalent to task's states.
		/// </summary>
		/// <param name="state">State to translate.</param>
		/// <returns>Text equivalent.</returns>
		private static string getTaskStateText(TaskState state)
		{
			switch (state)
			{
				case TaskState.Active:
					return "Активна";
				case TaskState.Inactive:
					return "Неактивна";
				case TaskState.Paused:
					return "Приостановлена";
				default:
					return "-";
			}
		}

		/// <summary>
		/// Gets text equivalent to task's modes.
		/// </summary>
		/// <param name="mode">Mode to translate.</param>
		/// <returns>Text equivalent.</returns>
		private static string getTaskModeText(TaskMode mode)
		{
			switch (mode)
			{
				case TaskMode.Disabled:
					return "Отключена";
				case TaskMode.Paused:
					return "Приостановлена";
				case TaskMode.Interrupted:
					return "Прерывание";
				case TaskMode.Running:
					return "Выполнение";
				case TaskMode.WaitForReady:
					return "Ожидание готовности";
				case TaskMode.WaitForRun:
					return "Ожидание выполнения";
				default:
					return "-";
			}
		}

		#region Commands realization.

		private void StartCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (Manager.Status != ManagerStatus.Paused)
			{
				Regex regex = new Regex("^[0-9]*$");
				//Enumerate all text boxes.
				foreach (TextBox child in FindVisualChildren<TextBox>(allParametersGrid))
				{
					if (child.Text == string.Empty || !regex.IsMatch(child.Text))
					{
						ErrorWindow dlg = new ErrorWindow(this, "Ошибка ввода", "Некорректное значение.",
														  "Введенный параметр содержит недопустимый символ либо значение.",
														  "Параметр " + child.Tag + " имеет физический смысл и должен представлять собой неотрицательное число.");
						dlg.ShowDialog();
						child.Text = previousParameterValue;
						return;
					}
				}

				#region Checking parameter Tp

				if (int.Parse(taskTime1TpTextBox.Text) == 0)
				{
					ErrorWindow dlg = new ErrorWindow(this, "Ошибка параметра", "Неверное значение.",
					                                  "Параметр " + taskTime1TpTextBox.Tag + " должен иметь положительное значение.",
					                                  "Период получения задачей управления Тп имеет физический смысл и поэтому не может быть нулевым.");
					dlg.ShowDialog();
					taskTime1TpTextBox.Focus();
					return;
				}
				if (int.Parse(taskTime1TpTextBox.Text) < int.Parse(taskTime1TsTextBox.Text))
				{
					ErrorWindow dlg = new ErrorWindow(this, "Ошибка параметра", "Неверное значение.",
					                                  "Параметр " + taskTime1TpTextBox.Tag + " не может быть меньше значения параметра " + taskTime1TsTextBox.Tag + ".",
					                                  "Период получения задачей управления Тп имеет физический смысл и поэтому не может быть меньше времени выполнения ей своих функций (времени сеанса работы Тс).");
					dlg.ShowDialog();
					taskTime1TpTextBox.Focus();
					return;
				}
				if (int.Parse(taskTime1TpTextBox.Text) <= int.Parse(taskTime1TkrTextBox.Text))
				{
					ErrorWindow dlg = new ErrorWindow(this, "Ошибка параметра", "Неверное значение.",
													  "Параметр " + taskTime1TpTextBox.Tag + " не может быть меньше или равен значению параметра " + taskTime1TkrTextBox.Tag + ".",
													  "Период получения задачей управления Тп не может быть меньше или равен максимальному времени выполнения ей своих функций (критическому времени сеанса работы Ткр).");
					dlg.ShowDialog();
					taskTime1TpTextBox.Focus();
					return;
				}
				if (int.Parse(taskTime2TpTextBox.Text) == 0)
				{
					ErrorWindow dlg = new ErrorWindow(this, "Ошибка параметра", "Неверное значение.",
					                                  "Параметр " + taskTime2TpTextBox.Tag + " должен иметь положительное значение.",
					                                  "Период получения задачей управления Тп имеет физический смысл и поэтому не может быть нулевым.");
					dlg.ShowDialog();
					taskTime2TpTextBox.Focus();
					return;
				}
				if (int.Parse(taskTime2TpTextBox.Text) < int.Parse(taskTime2TsTextBox.Text))
				{
					ErrorWindow dlg = new ErrorWindow(this, "Ошибка параметра", "Неверное значение.",
					                                  "Параметр " + taskTime2TpTextBox.Tag + " не может быть меньше значения параметра " + taskTime2TsTextBox.Tag + ".",
					                                  "Период получения задачей управления Тп  имеет физический смысл и поэтому не может быть меньше времени выполнения ей своих функций (времени сеанса работы Тс).");
					dlg.ShowDialog();
					taskTime2TpTextBox.Focus();
					return;
				}
				if (int.Parse(taskTime2TpTextBox.Text) <= int.Parse(taskTime2TkrTextBox.Text))
				{
					ErrorWindow dlg = new ErrorWindow(this, "Ошибка параметра", "Неверное значение.",
													  "Параметр " + taskTime2TpTextBox.Tag + " не может быть меньше или равен значению параметра " + taskTime2TkrTextBox.Tag + ".",
													  "Период получения задачей управления Тп не может быть меньше или равен максимальному времени выполнения ей своих функций (критическому времени сеанса работы Ткр).");
					dlg.ShowDialog();
					taskTime2TpTextBox.Focus();
					return;
				}

				#endregion

				#region Checking parameter Tkr

				if (int.Parse(taskTime1TkrTextBox.Text) <= int.Parse(taskTime1TsTextBox.Text))
				{
					ErrorWindow dlg = new ErrorWindow(this, "Ошибка параметра", "Неверное значение.",
					                                  "Параметр " + taskTime1TkrTextBox.Tag + " не может быть меньше или равно значению параметра " + taskTime1TsTextBox.Tag + ".",
					                                  "Максимально допустимое время работы задачи Ткр не может быть заведомо меньше времени выполнения ей своих функций (времени сеанса работы Тс).");
					dlg.ShowDialog();
					taskTime1TkrTextBox.Focus();
					return;
				}
				if (int.Parse(taskTime2TkrTextBox.Text) < int.Parse(taskTime2TsTextBox.Text))
				{
					ErrorWindow dlg = new ErrorWindow(this, "Ошибка параметра", "Неверное значение.",
					                                  "Параметр " + taskTime2TkrTextBox.Tag + " не может быть меньше или равно значению параметра " + taskTime2TsTextBox.Tag + ".",
					                                  "Максимально допустимое время работы задачи Ткр не может быть заведомо меньше времени выполнения ей своих функций (времени сеанса работы Тс).");
					dlg.ShowDialog();
					taskTime2TkrTextBox.Focus();
					return;
				}
				if (int.Parse(taskIntTkrTextBox.Text) < int.Parse(taskIntTsTextBox.Text))
				{
					ErrorWindow dlg = new ErrorWindow(this, "Ошибка параметра", "Неверное значение.",
					                                  "Параметр " + taskIntTkrTextBox.Tag + " не может быть меньше или равно значению параметра " + taskIntTsTextBox.Tag + ".",
					                                  "Максимально допустимое время работы задачи Ткр не может быть заведомо меньше времени выполнения ей своих функций (времени сеанса работы Тс).");
					dlg.ShowDialog();
					taskIntTkrTextBox.Focus();
					return;
				}

				#endregion

				#region Checking parameter Pr

				if (int.Parse(taskTime1PrTextBox.Text) == int.Parse(taskTime2PrTextBox.Text))
				{
					ErrorWindow dlg = new ErrorWindow(this, "Ошибка параметра", "Неверное значение.",
					                                  "Параметр " + taskTime1PrTextBox.Tag + " не может быть равен параметру " + taskTime2PrTextBox.Tag + ".",
					                                  "Задачи одного типа не могут иметь одинаковое значение приоритета, так как менеджер не сможет выяснить какой задаче передать управление в случае готовности обеих. Задача с меньшим значением параметра имеет более высокий приоритет.");
					dlg.ShowDialog();
					taskTime1PrTextBox.Focus();
					return;
				}

				#endregion

				#region Checking parameter Tc

				if (int.Parse(taskTime1TsTextBox.Text) == 0)
				{
					ErrorWindow dlg = new ErrorWindow(this, "Ошибка параметра", "Неверное значение.",
					                                  "Параметр " + taskTime1TsTextBox.Tag + " должен иметь положительное значение.",
					                                  "Время выполнения задачей своих функций Тс (время сеанса) имеет физический смысл и поэтому не может быть нулевым.");
					dlg.ShowDialog();
					taskTime1TsTextBox.Focus();
					return;
				}
				if (int.Parse(taskTime2TsTextBox.Text) == 0)
				{
					ErrorWindow dlg = new ErrorWindow(this, "Ошибка параметра", "Неверное значение.",
					                                  "Параметр " + taskTime2TsTextBox.Tag + " должен иметь положительное значение.",
					                                  "Время выполнения задачей своих функций Тс (время сеанса) имеет физический смысл и поэтому не может быть нулевым.");
					dlg.ShowDialog();
					taskTime2TsTextBox.Focus();
					return;
				}
				if (int.Parse(taskIntTsTextBox.Text) == 0)
				{
					ErrorWindow dlg = new ErrorWindow(this, "Ошибка параметра", "Неверное значение.",
					                                  "Параметр " + taskIntTsTextBox.Tag + " должен иметь положительное значение.",
					                                  "Время выполнения задачей своих функций Тс (время сеанса) имеет физический смысл и поэтому не может быть нулевым.");
					dlg.ShowDialog();
					taskIntTsTextBox.Focus();
					return;
				}

				#endregion

				#region Checking parameter Tk

				if (int.Parse(taskTime1TkTextBox.Text) == 0)
				{
					ErrorWindow dlg = new ErrorWindow(this, "Ошибка параметра", "Неверное значение.",
					                                  "Параметр " + taskTime1TkTextBox.Tag + " должен иметь положительное значение.",
					                                  "Частота возникновения события, влияющего на завершение работы другой задачи, не может быть нулевой, так как имеет физический смысл.");
					dlg.ShowDialog();
					taskTime1TkTextBox.Focus();
					return;
				}
				if (int.Parse(taskTime2TkTextBox.Text) == 0)
				{
					ErrorWindow dlg = new ErrorWindow(this, "Ошибка параметра", "Неверное значение.",
					                                  "Параметр " + taskTime2TkTextBox.Tag + " должен иметь положительное значение.",
					                                  "Время жизни задачи на протяжении процесса моделирования Тк (время окончания ее работы) не может быть нулевым, так как имеет физический смысл.");
					dlg.ShowDialog();
					taskTime2TkTextBox.Focus();
					return;
				}

				#endregion

				#region Checking parameter Pause

				if (int.Parse(taskTime1PauseTextBox.Text) == 0)
				{
					ErrorWindow dlg = new ErrorWindow(this, "Ошибка параметра", "Неверное значение.",
					                                  "Параметр " + taskTime1PauseTextBox.Tag + " должен иметь положительное значение.",
					                                  "Частота возникновения события, влияющего на приостановку работы другой задачи, не может быть нулевой, так как имеет физический смысл.");
					dlg.ShowDialog();
					taskTime1PauseTextBox.Focus();
					return;
				}

				#endregion

				//Set specified time task 1 parameters.
				TimeTask1 task1 = (TimeTask1)Manager.Tasks[0];
				task1.PeriodTime = int.Parse(taskTime1TpTextBox.Text);
				task1.CriticalDelayTime = int.Parse(taskTime1TzTextBox.Text);
				task1.CriticalRunTime = int.Parse(taskTime1TkrTextBox.Text);
				task1.Priority = int.Parse(taskTime1PrTextBox.Text);
				task1.SessionTime = int.Parse(taskTime1TsTextBox.Text);
				taskTime1StateTextBlock.Text = getTaskStateText(task1.State);
				taskTime1ModeTextBlock.Text = getTaskModeText(task1.Mode);

				//Set specified time task 2 parameters.
				TimeTask2 task2 = (TimeTask2)Manager.Tasks[1];
				task2.PeriodTime = int.Parse(taskTime2TpTextBox.Text);
				task2.CriticalDelayTime = int.Parse(taskTime2TzTextBox.Text);
				task2.CriticalRunTime = int.Parse(taskTime2TkrTextBox.Text);
				task2.Priority = int.Parse(taskTime2PrTextBox.Text);
				task2.SessionTime = int.Parse(taskTime2TsTextBox.Text);
				task2.StopTime = int.Parse(taskTime2TkTextBox.Text);
				task2.EventTimeForTask1Tk = int.Parse(taskTime1TkTextBox.Text);
				task2.EventTimeForTask1Pause = int.Parse(taskTime1PauseTextBox.Text);
				task2.SessionTimeBeforeStart = int.Parse(taskTime2TsTextBox.Text);
				taskTime2StateTextBlock.Text = getTaskStateText(task2.State);
				taskTime2ModeTextBlock.Text = getTaskModeText(task2.Mode);
			}

			Manager.Start();
		}

		private void PauseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			Manager.Pause();
		}

		private void StopCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			Manager.Stop();
		}

		private void ResetCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (MessageBox.Show("Вы действительно хотите сбросить все введенные параметры и результаты моделирования?",
				"Сброс", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
			{
				StopCommand.Execute(null, this);

				createTaskManager();

				//Set status.
				statusBarText.Text = "Готов к работе";

				//Clear tasks parameters text boxes.
				taskTime1TpTextBox.Text = "0";
				taskTime2TpTextBox.Text = "0";
				taskTime1TzTextBox.Text = "0";
				taskTime2TzTextBox.Text = "0";
				taskTime1TkrTextBox.Text = "0";
				taskTime2TkrTextBox.Text = "0";
				taskIntTkrTextBox.Text = "0";
				taskTime1PrTextBox.Text = "0";
				taskTime2PrTextBox.Text = "0";
				taskTime1TsTextBox.Text = "0";
				taskTime2TsTextBox.Text = "0";
				taskIntTsTextBox.Text = "0";
				taskTime2TkTextBox.Text = "0";
				taskTime1TkTextBox.Text = "0";
				taskTime1PauseTextBox.Text = "0";

				//Set visibility of entered parameters.
				taskTime1TnTextBlock.Visibility = Visibility.Collapsed;
				taskTime1TnImages.Visibility = Visibility.Visible;
				taskTime2TnTextBlock.Visibility = Visibility.Collapsed;
				taskTime2TnImages.Visibility = Visibility.Visible;

				statusBarText.Text = "Все параметры менеджера задач сброшены";
			}
		}

		private void HelpCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			HelpWindow dlg = new HelpWindow(this, false);
			dlg.ShowDialog();
		}

		private void StartCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (Manager == null)
				return;
			e.CanExecute = (Manager.Status == ManagerStatus.Paused || Manager.Status == ManagerStatus.Stopped);
		}

		private void PauseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (Manager == null)
				return;
			e.CanExecute = (Manager.Status == ManagerStatus.Running);
		}

		private void StopCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (Manager == null)
				return;
			e.CanExecute = (Manager.Status != ManagerStatus.Stopped);
		}

		#endregion

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (Manager.Status == ManagerStatus.Stopped || Manager.Status == ManagerStatus.Done)
				return;
			if (taskTime1TnTextBlock.Visibility == Visibility.Collapsed && e.Key == Key.F2)
			{
				//Time task (set start time).
				Manager.Tasks[0].StartTime = Manager.CurrentTime + 1;
				Manager.Tasks[0].State = TaskState.Active;
				Manager.Tasks[0].Mode = TaskMode.WaitForRun;
				Manager.Tasks[0].TimeWhenBecomeWaitForRun = Manager.CurrentTime + 1;
				taskTime1TnTextBlock.Text = (Manager.CurrentTime + 1).ToString();
				taskTime1StateTextBlock.Text = getTaskStateText(Manager.Tasks[0].State);
				taskTime1ModeTextBlock.Text = getTaskModeText(Manager.Tasks[0].Mode);
				taskTime1TnTextBlock.Visibility = Visibility.Visible;
				taskTime1TnImages.Visibility = Visibility.Collapsed;
				statusBarText.Text = "Параметр " + taskTime1TnTextBlock.Tag + " установлен";
				if (Manager.Status != ManagerStatus.Stopped)
					Manager.DrawPlannedPoints(0);
			}
			else if (e.Key == Key.F4)
			{
				//Create interrupt task.
				Manager.AddInterruptTask(Manager.CurrentTime + 1,
					int.Parse(taskIntTkrTextBox.Text), int.Parse(taskIntTsTextBox.Text));
				taskIntStateTextBlock.Text = getTaskStateText(Manager.Tasks[2].State);
				taskIntModeTextBlock.Text = getTaskModeText(Manager.Tasks[2].Mode);
				statusBarText.Text = "Возникло прерывание в момент времени " + (Manager.CurrentTime + 1);
			}
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			Manager.Stop();
		}

		private void scrollArea_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (Manager.Status == ManagerStatus.Stopped || Manager.Status == ManagerStatus.Done)
				return;
			if (taskTime2TnTextBlock.Visibility == Visibility.Collapsed)
			{
				Manager.Tasks[1].StartTime = Manager.CurrentTime + 1;
				Manager.Tasks[1].State = TaskState.Active;
				Manager.Tasks[1].Mode = TaskMode.WaitForRun;
				Manager.Tasks[1].TimeWhenBecomeWaitForRun = Manager.CurrentTime + 1;
				taskTime2TnTextBlock.Text = (Manager.CurrentTime + 1).ToString();
				taskTime2StateTextBlock.Text = getTaskStateText(Manager.Tasks[1].State);
				taskTime2ModeTextBlock.Text = getTaskModeText(Manager.Tasks[1].Mode);
				taskTime2TnTextBlock.Visibility = Visibility.Visible;
				taskTime2TnImages.Visibility = Visibility.Collapsed;
				statusBarText.Text = "Параметр " + taskTime2TnTextBlock.Tag + " установлен";
				if (Manager.Status != ManagerStatus.Stopped)
					Manager.DrawPlannedPoints(1);
			}
		}

		private void scrollArea_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (Manager.Status == ManagerStatus.Stopped || Manager.Status == ManagerStatus.Done)
				return;
			if (Manager.Tasks[0].State == TaskState.Paused && Manager.Tasks[0].Mode != TaskMode.Disabled)
			{
				Manager.Tasks[0].State = TaskState.Active;
				Manager.Tasks[0].Mode = TaskMode.WaitForReady;
				taskTime1StateTextBlock.Text = getTaskStateText(TaskState.Active);
				statusBarText.Text = "Работа временной задачи 2 возобновлена после приостановки";
				Manager.Drawer.DrawMarker(Manager.Tasks[0].ID, Manager.CurrentTime, MarkerType.Resume);
			}
		}

		private void skipTask1Request_Click(object sender, RoutedEventArgs e)
		{
			Manager.Tasks[0].State = TaskState.Active;
			Manager.Tasks[0].Mode = TaskMode.WaitForReady;
			requestPanel1.Visibility = Visibility.Collapsed;
			Manager.Drawer.DrawMarker(0, Manager.CurrentTime, MarkerType.Skip);
			taskTime1StateTextBlock.Text = getTaskStateText(TaskState.Active);
			taskTime1ModeTextBlock.Text = getTaskModeText(TaskMode.WaitForReady);
			statusBarText.Text = "Плановый запуск временной задачи 1 пропущен";
		}

		private void shutdownTask1Request_Click(object sender, RoutedEventArgs e)
		{
			Manager.Tasks[0].State = TaskState.Inactive;
			Manager.Tasks[0].Mode = TaskMode.Disabled;
			requestPanel1.Visibility = Visibility.Collapsed;
			Manager.Drawer.DrawMarker(0, Manager.CurrentTime, MarkerType.Shutdown);
			taskTime1StateTextBlock.Text = getTaskStateText(TaskState.Inactive);
			taskTime1ModeTextBlock.Text = getTaskModeText(TaskMode.Disabled);
			statusBarText.Text = "Временная задача 1 снята";
		}

		private void skipTask2Request_Click(object sender, RoutedEventArgs e)
		{
			Manager.Tasks[1].State = TaskState.Active;
			Manager.Tasks[1].Mode = TaskMode.WaitForReady;
			requestPanel2.Visibility = Visibility.Collapsed;
			Manager.Drawer.DrawMarker(1, Manager.CurrentTime, MarkerType.Skip);
			taskTime2StateTextBlock.Text = getTaskStateText(TaskState.Active);
			taskTime2ModeTextBlock.Text = getTaskModeText(TaskMode.WaitForReady);
			statusBarText.Text = "Плановый запуск временной задачи 2 пропущен";
		}

		private void shutdownTask2Request_Click(object sender, RoutedEventArgs e)
		{
			Manager.Tasks[1].State = TaskState.Inactive;
			Manager.Tasks[1].Mode = TaskMode.Disabled;
			requestPanel2.Visibility = Visibility.Collapsed;
			Manager.Drawer.DrawMarker(1, Manager.CurrentTime, MarkerType.Shutdown);
			taskTime2StateTextBlock.Text = getTaskStateText(TaskState.Inactive);
			taskTime2ModeTextBlock.Text = getTaskModeText(TaskMode.Disabled);
			statusBarText.Text = "Временная задача 2 снята";
		}
	}
}
