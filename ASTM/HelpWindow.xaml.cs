using System.Windows;

namespace ASTM
{
	/// <summary>
	/// Interaction logic for HelpWindow.xaml
	/// </summary>
	public partial class HelpWindow : Window
	{
		/// <summary>
		/// Shows if window was called when application started or from menu command.
		/// </summary>
		private bool isLoadingScreen;

		/// <summary>
		/// Creates help window.
		/// </summary>
		/// <param name="parent">Parent window for this dialog.</param>
		/// <param name="loadingScreen">Specify is it first application screen or this window is calling from menu.</param>
		public HelpWindow(Window parent, bool loadingScreen)
		{
			InitializeComponent();

			Owner = parent;
			isLoadingScreen = loadingScreen;
		}

		private void exitButton_Click(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			//Set buttons visibility.
			if (isLoadingScreen)
			{
				startButton.Visibility = Visibility.Visible;
				exitButton.Visibility = Visibility.Visible;
				okButton.Visibility = Visibility.Collapsed;
			}
		}

		private void startButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
