using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Color = System.Windows.Media.Color;

namespace VistaExtensions
{
	/// <summary>
	/// Represents erros during extending window frame with glass effect. 
	/// </summary>
	public class GlassEffectExtendingException : Exception
	{
		/// <summary>
		/// Default constructor without parameters.
		/// </summary>
		public GlassEffectExtendingException()
		{
		}

		/// <summary>
		/// Constructor with string parameter for error description.
		/// </summary>
		/// <param name="message">Error description text.</param>
		public GlassEffectExtendingException(string message)
			: base(message)
		{
		}
	}

	/// <summary>
	/// Helper for extending area of Windows Vista glass effect on the windows.
	/// </summary>
	public static class GlassExtender
	{
		#region Imported functions

		/// <summary>
		/// Extends the window glass effect frame behind the client area.
		/// </summary>
		/// <param name="hWnd">The handle to the window for which the frame is extended into the client area.</param>
		/// <param name="pMarInset">A pointer to a margins structure that describes the margins to use when
		/// extending the frame into the client area.</param>
		/// <returns>If the function succeeds, it returns 0. Otherwise, it returns a negative error code.</returns>
		[DllImport("DwmApi.dll")]
		private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Margins pMarInset);

		/// <summary>
		/// Obtains a value that indicates whether DWM composition is enabled.
		/// </summary>
		/// <param name="pfEnabled">Receives TRUE if DWM composition is enabled; otherwise, FALSE.</param>
		/// <returns>If the function succeeds, it returns 0. Otherwise, it returns an negative error code.</returns>
		[DllImport("dwmapi.dll")]
		private static extern IntPtr DwmIsCompositionEnabled(out bool pfEnabled);

		#endregion

		#region Private methods

		/// <summary>
		/// Checks current Windows version.
		/// </summary>
		/// <returns>Returns TRUE if version is Vista or higher.</returns>
		private static bool IsValidWindowsVersion()
		{
			return (Environment.OSVersion.Version.Major >= 6);
		}

		/// <summary>
		/// Get system DPI adjusted window margins.
		/// </summary>
		/// <param name="windowHandle">Handle of window which argins will be adjusted.</param>
		/// <param name="left">Left margin width.</param>
		/// <param name="right">Right margin width.</param>
		/// <param name="top">Top margin height.</param>
		/// <param name="bottom">Bottom margin height.</param>
		/// <returns>Adjusted margins of specified window.</returns>
		private static Margins GetDpiAdjustedMargins(IntPtr windowHandle, int left, int right, int top, int bottom)
		{
			//Get system DPI parameters.
			Graphics g = Graphics.FromHwnd(windowHandle);
			float desktopDpiX = g.DpiX;
			float desktopDpiY = g.DpiY;
			//Adjust margins.
			Margins margins = new Margins
								{
									cxLeftWidth = Convert.ToInt32(left * (desktopDpiX / 96)),
									cxRightWidth = Convert.ToInt32(right * (desktopDpiX / 96)),
									cyTopHeight = Convert.ToInt32(top * (desktopDpiY / 96)),
									cyBottomHeight = Convert.ToInt32(bottom * (desktopDpiY / 96))
								};
			return margins;
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Checks if glass effect currently enabled in the system.
		/// </summary>
		/// <returns>If enabled function returns TRUE, otherwise FALSE.</returns>
		public static bool IsGlassEffectEnabled()
		{
			bool glassEnabled;
			if (DwmIsCompositionEnabled(out glassEnabled) == IntPtr.Zero)
				return glassEnabled;
			return false;
		}

		/// <summary>
		/// Extending Windows Vista window frame glass effect.
		/// </summary>
		/// <param name="win">Window which will be modified.</param>
		/// <param name="left">Width of left glass effect margin (or -1 for max width).</param>
		/// <param name="right">Width of right glass effect margin (or -1 for max width).</param>
		/// <param name="top">Height of top glass effect margin (or -1 for max height).</param>
		/// <param name="bottom">Height of bottom glass effect margin (or -1 for max height).</param>
		public static void Extend(Window win, int left, int right, int top, int bottom)
		{
			//Check Windows version.
			if (!IsValidWindowsVersion())
			{
				throw new GlassEffectExtendingException("Invalid Windows version for using window frame glass effect.");
			}

			//Obtain Win32 window handle for WPF window.
			var windowInterop = new WindowInteropHelper(win);
			IntPtr windowHandle = windowInterop.Handle;
			HwndSource windowSrc = HwndSource.FromHwnd(windowHandle);
			if (windowSrc != null && windowSrc.CompositionTarget != null)
			{
				windowSrc.CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);
			}
			else
			{
				throw new GlassEffectExtendingException("Unable to obtain HwndSource from current window handle.");
			}

			//Adjusting margins depends on system DPI.
			Margins margins = GetDpiAdjustedMargins(windowHandle, left, right, top, bottom);
			//Extending window glass frame.
			int returnVal = DwmExtendFrameIntoClientArea(windowHandle, ref margins);
			if (returnVal < 0)
			{
				throw new GlassEffectExtendingException(String.Format("Extending frame into client area failed. " +
					"Return code of DwmExtendFrameIntoClientArea function is {0}.", returnVal));
			}
		}

		#endregion

		#region Nested type: Margins

		/// <summary>
		/// Margins of window glass effect.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		private struct Margins
		{
			public int cxLeftWidth;
			public int cxRightWidth;
			public int cyTopHeight;
			public int cyBottomHeight;
		}

		#endregion
	}
}