using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ASTM
{
	/// <summary>
	/// Types of markers on the graph. Each marker has different image, that will be loaded from resources.
	/// </summary>
	public enum MarkerType
	{
		StartTime, StopTime, PlannedTime, Break, Event, Pause,
		Resume, ReadPoint, Request, Skip, Shutdown, Interrupt, Continue
	}

	/// <summary>
	/// Draws tasks activity graph on the specified container.
	/// </summary>
	public class GraphDrawer
	{
		#region Graphic sizes parameters constants.

		/// <summary>
		/// Height of descrete value line.
		/// </summary>
		private const double DescreteValueHeight = 15;

		/// <summary>
		/// Height of of big bar. Conctant for use in external code.
		/// </summary>
		public static int BigBarHeight = 15;

		/// <summary>
		/// Height of of small bar. Conctant for use in external code.
		/// </summary>
		public static int SmallBarHeight = 5;

		/// <summary>
		/// Width of each time interval on the graph.
		/// </summary>
		public const double PointsPerInterval = 15;

		/// <summary>
		/// Height of interval between horizontal axes.
		/// </summary>
		public static int PointsBetweenAxes = 50;

		/// <summary>
		/// Height of function graph.
		/// </summary>
		private const int MaxPointsGraph = 60;

		#endregion

		#region All colors definition.

		/// <summary>
		/// Color of non-highlighted graph vertical axis.
		/// </summary>
		private static readonly Color VerticalAxisNormalColor = Color.FromArgb(150, 0, 0, 0);

		/// <summary>
		/// Color of highlighted graph vertical axis.
		/// </summary>
		private static readonly Color VerticalAxisHighlightColor = Color.FromRgb(250, 20, 0);

		/// <summary>
		/// Color of vertical main (first left) axis.
		/// </summary>
		private static readonly Color LeftAxisColor = Color.FromRgb(100, 100, 100);

		/// <summary>
		/// Color of axes captions.
		/// </summary>
		private static readonly Color AxesCaptionsColor = Color.FromArgb(150, 0, 0, 0);

		/// <summary>
		/// Color of the graphic descrete values line.
		/// </summary>
		private static readonly Color DescreteValuesLineColor = Color.FromArgb(230, 60, 150, 0);

		/// <summary>
		/// Color of function graphic horizontal axis.
		/// </summary>
		private static readonly Color FunctionGraphicAxisColor = Color.FromRgb(200, 100, 255);

		/// <summary>
		/// Color of function graphic middle line.
		/// </summary>
		private static readonly Color FunctionGraphicMiddleLineColor = Color.FromArgb(150, 0, 0, 0);

		/// <summary>
		/// Color of function graphic.
		/// </summary>
		private static readonly Color FunctionGraphicColor = Color.FromRgb(0, 0, 255);

		#endregion

		#region All constant init data.

		/// <summary>
		/// Tasks details. Need to know their colors and names.
		/// </summary>
		public readonly TaskInfo[] TasksInfo;

		/// <summary>
		/// Gets count of graph intervals (vertical axes).
		/// </summary>
		public int IntervalsCount { get; private set; }

		/// <summary>
		/// Number of tasks in graph (horizontal axes).
		/// </summary>
		private readonly int maxTasks;

		/// <summary>
		/// Gets delay in time of each graph interval for animating rectangle drawing.
		/// </summary>
		private readonly int intervalDelay;

		/// <summary>
		/// Graphic function.
		/// </summary>
		/// <param name="x">Argument.</param>
		/// <returns>Function value.</returns>
		public delegate double Function(double x);

		/// <summary>
		/// Signal graphic function.
		/// </summary>
		private readonly Function f;

		/// <summary>
		/// Function maximal value.
		/// </summary>
		private readonly double funcMax;

		/// <summary>
		/// Function minimal value.
		/// </summary>
		private readonly double funcMin;

		/// <summary>
		/// Function maximal value argument.
		/// </summary>
		private readonly double funcMaxArg;

		/// <summary>
		/// Function minimal value argument.
		/// </summary>
		private readonly double funcMinArg;

		#endregion

		#region All calculated init data.

		/// <summary>
		/// Size of function graphs positive axis.
		/// </summary>
		private readonly int positiveGraphAxisPoints;

		/// <summary>
		/// Size of function graphs negative axis.
		/// </summary>
		private readonly int negativeGraphAxisPoints;

		/// <summary>
		/// Y-position of function graph.
		/// </summary>
		private readonly int funcGraphPosY;

		/// <summary>
		/// Y-position of function graphic middle line.
		/// </summary>
		private readonly double funcMiddleLinePosY;

		/// <summary>
		/// Height of diagram (without space for numbers).
		/// </summary>
		private readonly double diagramHeight;

		#endregion

		/// <summary>
		/// Collection for saving shapes which window must draw.
		/// </summary>
		private readonly UIElementCollection elems;

		/// <summary>
		/// Previous states of tasks for the future analyzing. Need to save this data for drawing nicely looking
		/// rectangles when task has the same state few times in succession (in this way need just extend width of
		/// previous state rectangle).
		/// </summary>
		private int[,] history;

		/// <summary>
		/// Array of lists of all tasks states rectangles with key for fast searching when need extend
		/// previous rectangle. Key shows when (interval number) rectangle was created. Array index
		/// represents task.
		/// </summary>
		private readonly Dictionary<int, Rectangle>[] bars;

		/// <summary>
		/// Descrete values of function graphic.
		/// </summary>
		private readonly List<Point> descreteData;

		/// <summary>
		/// List of all vertical axes for highlighting them.
		/// </summary>
		private List<Line> vertAxes;

		/// <summary>
		/// Initializes a new instance of the GraphDrawer class with specified parameters.
		/// </summary>
		/// <param name="maxLength">Length of the max.</param>
		/// <param name="tasks">The tasks.</param>
		/// <param name="animationSpeed">The animation speed.</param>
		/// <param name="elements">The elements.</param>
		/// <param name="func">Signal graphic function.</param>
		/// <param name="fMax">Function maximal value.</param>
		/// <param name="fMin">Function minimal value.</param>
		/// <param name="fMaxArg">Function maximal value argument.</param>
		/// <param name="fMinArg">Function minimal value argument.</param>
		public GraphDrawer(int maxLength, TaskInfo[] tasks, int animationSpeed, UIElementCollection elements,
			Function func, double fMax, double fMin, double fMaxArg, double fMinArg)
		{
			IntervalsCount = maxLength;
			TasksInfo = tasks;
			maxTasks = tasks.Length;
			intervalDelay = animationSpeed;
			elems = elements;
			f = func;
			funcMax = fMax;
			funcMin = fMin;
			funcMaxArg = fMaxArg;
			funcMinArg = fMinArg;
			//Calculate parameters of function graph.
			positiveGraphAxisPoints = fMax > 0 ? MaxPointsGraph + PointsBetweenAxes : PointsBetweenAxes;
			negativeGraphAxisPoints = fMin < 0 ? MaxPointsGraph + 10 : 10;
			funcGraphPosY = maxTasks * PointsBetweenAxes + positiveGraphAxisPoints;
			funcMiddleLinePosY = funcGraphPosY - (MaxPointsGraph / funcMax) * (funcMax + funcMin) / 2;
			//Calculate height of diagram (without space for numbers).
			diagramHeight = maxTasks * PointsBetweenAxes + positiveGraphAxisPoints + negativeGraphAxisPoints;
			//Creating dictionary of dictionaries for rectangles which will represent tasks states.
			bars = new Dictionary<int, Rectangle>[maxTasks];
			//Creating List of function descrete values.
			descreteData = new List<Point>();
			//Draw all init lines.
			NewDrawing();
		}

		#region Init drawing of blank area (all axes, numbers and function).

		/// <summary>
		/// Draws function graphic.
		/// </summary>
		private void drawFunc()
		{
			//Max value of agrument.
			double max = IntervalsCount * PointsPerInterval;
			//Multiplier of function value for correct drawing in the window.
			double mx = MaxPointsGraph / funcMax;
			
			//Calculating points and creating polyline.
			Polyline graphic = new Polyline();
			for (double x = 0; x < max; x++)
			{
				double y = funcGraphPosY - mx * f(x / PointsPerInterval);
				graphic.Points.Add(new Point(x, y));
			}

			//Draw graphic.
			graphic.Stroke = new SolidColorBrush(FunctionGraphicColor);
			graphic.StrokeThickness = 1;
			elems.Add(graphic);

			//Draw maximal and minimal value on the left axis.
			drawGraphNumers(funcMax, funcMaxArg * PointsPerInterval + 3, mx * funcMax + 3);
			drawGraphNumers(funcMin, funcMinArg * PointsPerInterval + 3, mx * funcMin + 3);

			//Draw middle line.
			Line ln = new Line();
			ln.X1 = 0;
			ln.X2 = IntervalsCount * PointsPerInterval;
			ln.Y1 = ln.Y2 = funcMiddleLinePosY;
			ln.Stroke = new SolidColorBrush(FunctionGraphicMiddleLineColor);
			ln.StrokeThickness = 1;
			ln.StrokeDashArray = new DoubleCollection { 7, 3 };
			elems.Add(ln);
		}

		/// <summary>
		/// Draws numers on the graphic.
		/// </summary>
		/// <param name="v">Value to draw.</param>
		/// <param name="x">Position X.</param>
		/// <param name="y">Position Y.</param>
		private void drawGraphNumers(double v, double x, double y)
		{
			TextBlock text = new TextBlock();
			text.Text = string.Format("{0:0.#}", v);
			text.Margin = new Thickness(x, funcGraphPosY - y - text.FontSize, 0, 0);
			text.VerticalAlignment = VerticalAlignment.Top;
			text.HorizontalAlignment = HorizontalAlignment.Left;
			elems.Add(text);
		}

		/// <summary>
		/// Draws horizontal and vertical axes lines.
		/// </summary>
		private void drawAxes()
		{
			//Draw horizontal axes.
			for (int i = 0; i < maxTasks; i++)
			{
				//Draw axis.
				Line axis = new Line();
				axis.X1 = 0;
				axis.X2 = IntervalsCount * PointsPerInterval;
				axis.Y1 = axis.Y2 = (i + 1) * PointsBetweenAxes;
				axis.Stroke = new SolidColorBrush(TasksInfo[i].VisualColor);
				axis.StrokeThickness = 3;
				elems.Add(axis);

				//Draw caption of axis.
				TextBlock caption = new TextBlock();
				caption.Text = TasksInfo[i].Name;
				caption.Foreground = new SolidColorBrush(AxesCaptionsColor);
				caption.Margin = new Thickness(4, i * PointsBetweenAxes + 3, 0, 0);
				caption.VerticalAlignment = VerticalAlignment.Top;
				caption.HorizontalAlignment = HorizontalAlignment.Left;
				elems.Add(caption);
			}

			//Draw function graphic axis.
			Line funcAxis = new Line();
			funcAxis.X1 = 0;
			funcAxis.X2 = IntervalsCount * PointsPerInterval;
			funcAxis.Y1 = funcAxis.Y2 = funcGraphPosY;
			funcAxis.Stroke = new SolidColorBrush(FunctionGraphicAxisColor);
			funcAxis.StrokeThickness = 3;
			elems.Add(funcAxis);

			//Draw vertical dashed lines and numbers.
			for (int i = 1; i < IntervalsCount; i++)
			{
				//Draw vertical lines.
				Line vert = new Line();
				vert.X1 = vert.X2 = i * PointsPerInterval;
				vert.Y1 = 0;
				vert.Y2 = diagramHeight;
				vert.Stroke = new SolidColorBrush(VerticalAxisNormalColor);
				vert.StrokeThickness = 1;
				elems.Add(vert);
				vertAxes.Add(vert);

				//Draw numbers.
				if (i % 2 == 0)
				{
					TextBlock text = new TextBlock();
					text.Text = i.ToString();
					text.Margin = new Thickness(i * PointsPerInterval, diagramHeight, 0, 0);
					text.VerticalAlignment = VerticalAlignment.Top;
					text.HorizontalAlignment = HorizontalAlignment.Left;
					elems.Add(text);
				}
			}

			//Draw left axis.
			Line leftAxis = new Line();
			leftAxis.X1 = leftAxis.X2 = 0;
			leftAxis.Y1 = 0;
			leftAxis.Y2 = maxTasks * PointsBetweenAxes + positiveGraphAxisPoints + negativeGraphAxisPoints;
			leftAxis.Stroke = new SolidColorBrush(LeftAxisColor);
			leftAxis.StrokeThickness = 3;
			elems.Add(leftAxis);
			vertAxes.Insert(0, leftAxis);

			//Draw left axis number.
			TextBlock leftAxisText = new TextBlock();
			leftAxisText.Text = "0";
			leftAxisText.Margin = new Thickness(0, diagramHeight, 0, 0);
			leftAxisText.VerticalAlignment = VerticalAlignment.Top;
			leftAxisText.HorizontalAlignment = HorizontalAlignment.Left;
			elems.Add(leftAxisText);
		}

		#endregion

		#region Drawing control functions (add numbers, data, changing position etc).

		/// <summary>
		/// Creates new graphic area.
		/// </summary>
		public void NewDrawing()
		{
			//Reset data.
			elems.Clear();
			descreteData.Clear();
			//Add first descrete value for graphic start.
			descreteData.Add(new Point(0, funcMiddleLinePosY));
			//Creating history of tasks states values.
			history = new int[maxTasks, IntervalsCount];
			//Creating bars dictionaries.
			for (int i = 0; i < bars.Length; i++)
			{
				bars[i] = new Dictionary<int, Rectangle>();
			}
			//Creating list for vertical axes.
			vertAxes = new List<Line>();
			//Draw all axes.
			drawAxes();
			//Draw function.
			drawFunc();
		}

		/// <summary>
		/// Draws lines that connects specified points on the function graphic.
		/// </summary>
		/// <param name="timePos">Position on the graphic (x-value, time).</param>
		/// <param name="val">Descrete value on the current time position (1, -1 or 0 as previous value). </param>
		public void DrawNextDescreteValue(int timePos, int val)
		{
			//Calculate bew point on the graph.
			double yPos = val != 0 ? funcMiddleLinePosY - val * DescreteValueHeight :
				descreteData[descreteData.Count - 1].Y;
			Point p = new Point(timePos * PointsPerInterval, yPos);
			//Store data.
			descreteData.Add(p);
			//Draw X-line to the new point.
			int last = descreteData.Count - 1;
			Line connectionLine = new Line
			                      	{
			                      		X1 = descreteData[last - 1].X,
			                      		X2 = descreteData[last].X,
			                      		Y1 = descreteData[last - 1].Y,
			                      		Y2 = descreteData[last - 1].Y,
										Stroke = new SolidColorBrush(DescreteValuesLineColor),
			                      		StrokeThickness = 3
			                      	};
			elems.Add(connectionLine);
			//Draw Y-line to the new point.
			if (descreteData[last - 1].Y != descreteData[last].Y)
			{
				connectionLine = new Line
				                 	{
				                 		X1 = descreteData[last].X,
				                 		X2 = descreteData[last].X,
				                 		Y1 = descreteData[last - 1].Y,
				                 		Y2 = descreteData[last].Y,
				                 		Stroke = new SolidColorBrush(DescreteValuesLineColor),
				                 		StrokeThickness = 3
				                 	};
				elems.Add(connectionLine);
			}
			//Draw dot.
			if (val != 0)
			{
				Ellipse ell = new Ellipse
				              	{
				              		Width = 8,
				              		Height = 8,
				              		Margin = new Thickness(p.X - 4, p.Y - 4, 0, 0),
				              		HorizontalAlignment = HorizontalAlignment.Left,
				              		VerticalAlignment = VerticalAlignment.Top,
				              		Fill = new RadialGradientBrush(Color.FromRgb(255, 100, 0), Color.FromRgb(200, 50, 0))
				              	};
				elems.Add(ell);
			}
		}

		/// <summary>
		/// Draws next tasks states.
		/// </summary>
		/// <param name="taskState">State of the task (must be specified rectangles height).</param>
		/// <param name="taskIndex">Index of task whose state is specified.</param>
		/// <param name="timePos">Time when need to set task state.</param>
		public void DrawNextPos(int taskState, int taskIndex, int timePos)
		{
			//Skip task without state value.
			if (taskState == 0)
				return;

			//Stops if have reached last time interval.
			if (timePos == IntervalsCount)
				return;

			//Add new specified state to history.
			history[taskIndex, timePos] = taskState;

			//Check if need to extend previous rectangle or create new one. Searching back history for the same
			//state value as the same.
			int j;
			for (j = timePos; j > 0; j--)
			{
				if (history[taskIndex, j] != history[taskIndex, j - 1])
					break;
			}

			if (j == timePos)
			{
				//Draw new rectangle.
				Rectangle bar = new Rectangle();
				bar.Height = taskState;
				bar.Width = PointsPerInterval;
				bar.Fill = new LinearGradientBrush(TasksInfo[taskIndex].VisualColor, TasksInfo[taskIndex].VisualColor.MultiplyAlpha(0.7), 45);
				bar.Margin = new Thickness(timePos * PointsPerInterval, (taskIndex + 1) * PointsBetweenAxes - taskState, 0, 0);
				bar.VerticalAlignment = VerticalAlignment.Top;
				bar.HorizontalAlignment = HorizontalAlignment.Left;
				//elems.Add(bar);
				elems.Insert(0, bar);
				bars[taskIndex].Add(timePos, bar);
				//Creating animation of rectangle drawing.
				DoubleAnimation anim = new DoubleAnimation(0, PointsPerInterval, TimeSpan.FromSeconds(intervalDelay));
				bar.BeginAnimation(FrameworkElement.WidthProperty, anim);
			}
			else
			{
				//Extend existing rectangle.
				Rectangle bar = bars[taskIndex][j];
				//Creating animation of rectangle drawing.
				DoubleAnimation anim = new DoubleAnimation(bar.Width, bar.Width + PointsPerInterval, TimeSpan.FromSeconds(intervalDelay));
				bar.BeginAnimation(FrameworkElement.WidthProperty, anim);
			}

			//Fix first left axis drawing (left axis must overlap first rectangle).
			if (timePos == 0)
			{
				//Draw left axis (need to overlap first tasks states rectangles).
				Line leftAxis = new Line();
				leftAxis.X1 = leftAxis.X2 = 0;
				leftAxis.Y1 = 0;
				leftAxis.Y2 = diagramHeight;
				leftAxis.Stroke = new SolidColorBrush(LeftAxisColor);
				leftAxis.StrokeThickness = 3;
				//Remove old left axis.
				elems.Remove(vertAxes[0]);
				//Add new left axis and replace old one.
				elems.Add(leftAxis);
				vertAxes.RemoveAt(0);
				vertAxes.Insert(0, leftAxis);
			}
		}

		/// <summary>
		/// Hightlight vertical axis.
		/// </summary>
		/// <param name="i">Vertical axis number.</param>
		public void HighlightAxis(int i)
		{
			//Color previous axis as normal.
			if (i - 1 < vertAxes.Count && i > 0)
				vertAxes[i - 1].Stroke = new SolidColorBrush(VerticalAxisNormalColor);
			
			//Highlight specified axis.
			if (i < vertAxes.Count && i > 0)
				vertAxes[i].Stroke = new SolidColorBrush(VerticalAxisHighlightColor);
		}

		public void DrawMarker(int taskID, int pos, MarkerType marker)
		{
			//Find axis appropriated to task's ID.
			int i;
			for (i = 0; i < TasksInfo.Length; i++)
			{
				if (TasksInfo[i].ID == taskID)
					break;
			}

			//Set marker on the specified position.
			Image img = new Image
			            	{
			            		HorizontalAlignment = HorizontalAlignment.Left,
			            		VerticalAlignment = VerticalAlignment.Top,
			            		Stretch = Stretch.None
			            	};

			Uri uriSource = null;
			//Choose image and load it from resources.
			switch (marker)
			{
				case MarkerType.StartTime:
					img.Margin = new Thickness(pos * PointsPerInterval - 2, (i + 1) * PointsBetweenAxes - 16, 0, 0);
					uriSource = new Uri("/ASTM;component/Images/StartTime.png", UriKind.Relative);
					break;
				case MarkerType.StopTime:
					img.Margin = new Thickness(pos * PointsPerInterval - 14, (i + 1) * PointsBetweenAxes - 16, 0, 0);
					uriSource = new Uri("/ASTM;component/Images/StopTime.png", UriKind.Relative);
					break;
				case MarkerType.PlannedTime:
					img.Margin = new Thickness(pos * PointsPerInterval - 10.1, (i + 1) * PointsBetweenAxes - 16, 0, 0);
					uriSource = new Uri("/ASTM;component/Images/PlannedTime.png", UriKind.Relative);
					break;
				case MarkerType.Break:
					img.Margin = new Thickness(pos * PointsPerInterval - 7.8, (i + 1) * PointsBetweenAxes - 7, 0, 0);
					uriSource = new Uri("/ASTM;component/Images/Break.png", UriKind.Relative);
					break;
				case MarkerType.Event:
					img.Margin = new Thickness(pos * PointsPerInterval - 4.4, (i + 1) * PointsBetweenAxes - 30, 0, 0);
					uriSource = new Uri("/ASTM;component/Images/Event.png", UriKind.Relative);
					break;
				case MarkerType.Pause:
					img.Margin = new Thickness(pos * PointsPerInterval - 8.2, (i + 1) * PointsBetweenAxes - 7, 0, 0);
					uriSource = new Uri("/ASTM;component/Images/PauseTask.png", UriKind.Relative);
					break;
				case MarkerType.Resume:
					img.Margin = new Thickness(pos * PointsPerInterval - 8, (i + 1) * PointsBetweenAxes - 7, 0, 0);
					uriSource = new Uri("/ASTM;component/Images/ResumeTask.png", UriKind.Relative);
					break;
				case MarkerType.ReadPoint:
					img.Margin = new Thickness(pos * PointsPerInterval - 7.6, funcGraphPosY - 8.4, 0, 0);
					uriSource = new Uri("/ASTM;component/Images/ReadPoint.png", UriKind.Relative);
					break;
				case MarkerType.Request:
					img.Margin = new Thickness(pos * PointsPerInterval - 8.5, (i + 1) * PointsBetweenAxes - 8.3, 0, 0);
					uriSource = new Uri("/ASTM;component/Images/Request.png", UriKind.Relative);
					break;
				case MarkerType.Skip:
					img.Margin = new Thickness(pos * PointsPerInterval - 7.7, (i + 1) * PointsBetweenAxes - 7.7, 0, 0);
					uriSource = new Uri("/ASTM;component/Images/Skip.png", UriKind.Relative);
					break;
				case MarkerType.Shutdown:
					img.Margin = new Thickness(pos * PointsPerInterval - 7.7, (i + 1) * PointsBetweenAxes - 7.7, 0, 0);
					uriSource = new Uri("/ASTM;component/Images/Shutdown.png", UriKind.Relative);
					break;
				case MarkerType.Interrupt:
					img.Margin = new Thickness(pos * PointsPerInterval - 7.7, (i + 1) * PointsBetweenAxes - 7.7, 0, 0);
					uriSource = new Uri("/ASTM;component/Images/Interrupt.png", UriKind.Relative);
					break;
				case MarkerType.Continue:
					img.Margin = new Thickness(pos * PointsPerInterval - 7.7, (i + 1) * PointsBetweenAxes - 7.7, 0, 0);
					uriSource = new Uri("/ASTM;component/Images/Continue.png", UriKind.Relative);
					break;
			}

			if (uriSource != null)
			{
				img.Height = img.Width = 16;
				img.Stretch = Stretch.Uniform;
				img.Source = new BitmapImage(uriSource);
				elems.Add(img);
			}
		}

		#endregion
	}
}