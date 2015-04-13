using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using VistaExtensions;

namespace ASTM
{
	/// <summary>
	/// Interaction logic for AboutWindow.xaml
	/// </summary>
	public partial class AboutWindow : Window
	{
		public AboutWindow()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Constructor that takes a parent for this dialog.
		/// </summary>
		/// <param name="parent">Parent window for this dialog.</param>
		public AboutWindow(Window parent)
			: this()
		{
			this.Owner = parent;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				//Trying apply glass effect to all window area.
				if (GlassExtender.IsGlassEffectEnabled())
				{
					GlassExtender.Extend(this, -1, -1, -1, -1);
				}
				else
				{
					//If glass effect turned off.
					this.Background = Brushes.Gainsboro;
				}
			}
			catch (GlassEffectExtendingException)
			{
				//If running under Windows XP or earlier.
				this.Background = Brushes.Gainsboro;
			}
		}

		private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Uri.ToString()));
		}

		private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			this.DragMove();
		}
	}
}
