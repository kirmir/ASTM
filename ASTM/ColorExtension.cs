using System.Windows.Media;

namespace ASTM
{
	/// <summary>
	/// Adds additional methods to standard Color class.
	/// </summary>
	public static class ColorExtension
	{
		/// <summary>
		/// Changes color alpha-channel by multiplying it.
		/// </summary>
		/// <param name="original">Original color.</param>
		/// <param name="multiplier">Alpha-channel multiplier.</param>
		/// <returns>New color with changed alpha-channel.</returns>
		public static Color MultiplyAlpha(this Color original, double multiplier)
		{
			return new Color
					{
						A = (byte)(original.A * multiplier),
						R = original.R,
						G = original.G,
						B = original.B
					};
		}
	}
}
