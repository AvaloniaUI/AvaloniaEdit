namespace AvaloniaEdit.Utils
{
	internal static class TabStop
	{
		/// <summary>
		/// Gets the number of columns between <paramref name="column"/> and the next tab stop.
		/// </summary>
		/// <param name="column">The zero-based column a tab starts at.</param>
		/// <param name="indentationSize">The number of columns between two tab stops.</param>
		public static int ColumnsUntilNext(int column, int indentationSize)
		{
			return indentationSize - column % indentationSize;
		}
	}
}
