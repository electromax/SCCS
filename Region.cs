using System;
using System.Collections.Generic;
using System.Text;

namespace Sccs
{
	/// <summary>
	/// Represents a region of text (where to begin and where to end).
	/// </summary>
	class Region
	{
		/// <summary>
		/// The number of the region's first line (inclusive)
		/// </summary>
		public int Begin;

		/// <summary>
		/// The number of the region's last line (exclusive)
		/// </summary>
		public int End;

		public Region(int Begin, int End)
		{
			this.Begin = Begin;
			this.End = End;
		}

		/// <summary>
		/// Tests if the regions contain the same strings
		/// </summary>
		/// <param name="other">Other region</param>
		/// <param name="list">List of strings</param>
		/// <returns>True, if the same</returns>
		public bool Same(Region other, List<string> list)
		{
			if (Begin < 0 || End > list.Count)
				return false;
			if (End - Begin != other.End - other.Begin)
				return false;
			for (int i = Begin; i < End; i++)
				if (list[i] != list[other.Begin - Begin + i])
					return false;
			return true;
		}
	}
}
