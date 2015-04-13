using System.Windows.Media;

namespace ASTM
{
	/// <summary>
	/// Represents information about task for graph drawer.
	/// </summary>
	public class TaskInfo
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
		/// Gets or sets task's name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets task's color for visualization.
		/// </summary>
		public Color VisualColor { get; set; }

		/// <summary>
		/// Initializes a new instance of the task's information class.
		/// </summary>
		/// <param name="taskID">Task's unique identifier (ID).</param>
		/// <param name="name">Name of the task.</param>
		/// <param name="taskColor">Color of the task.</param>
		public TaskInfo(int taskID, string name, Color taskColor)
		{
			id = taskID;
			Name = name;
			VisualColor = taskColor;
		}
	}
}
