using System;
using System.Collections.Generic;
using System.Text;

namespace Sccs
{
	/// <summary>
	/// Represents the replacement.
	/// </summary>
	class Replacement
	{
		/// <summary>
		/// The region to find (in the original text)
		/// </summary>
		public Region FindRegion = null;

		/// <summary>
		/// The region to replace (in the new text)
		/// </summary>
		public Region ReplaceRegion = null;
	}
}
