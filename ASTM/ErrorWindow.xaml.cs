using System.Windows;

namespace ASTM
{
	/// <summary>
	/// Interaction logic for ErrorWindow.xaml
	/// </summary>
	public partial class ErrorWindow : Window
	{
		/// <summary>
		/// Creates dialog box with error details.
		/// </summary>
		/// <param name="parent">Parent window for this dialog.</param>
		/// <param name="title">Dialog title.</param>
		/// <param name="header">Error header text.</param>
		/// <param name="text">Error text.</param>
		/// <param name="details">Error details text (under expander).</param>
		public ErrorWindow(Window parent, string title, string header, string text, string details)
		{
			InitializeComponent();

			Owner = parent;
			Title = title;
			errorHeader.Text = header;
			errorText.Text = text;
			errorDetails.Text = details;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			System.Media.SystemSounds.Exclamation.Play();
		}

		private void detailsExpander_Expanded(object sender, RoutedEventArgs e)
		{
			errorDetails.Visibility = Visibility.Visible;
			detailsExpander.Header = "Меньше сведений";
		}

		private void detailsExpander_Collapsed(object sender, RoutedEventArgs e)
		{
			errorDetails.Visibility = Visibility.Collapsed;
			detailsExpander.Header = "Подробнее";
		}
	}
}
