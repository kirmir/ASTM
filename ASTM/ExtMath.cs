using System;

namespace ASTM
{
	/// <summary>
	/// Additional Math functions.
	/// </summary>
	public static class ExtMath
	{
		/// <summary>
		/// Returns the angle whose cotangent is the specified number.
		/// </summary>
		/// <param name="d">A number representing a cotangent.</param>
		/// <returns>An angle measured in radians.</returns>
		public static double Acot(double d)
		{
			return Math.Atan(1 / d);
		}
	}
}
