using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace AvaloniaEdit.Utils
{
	[TestFixture]
	public class TabStopTests
	{
		[Test]
		public void A_Column_On_A_Tab_Stop_Reaches_The_Next_One()
		{
			Assert.AreEqual(4, TabStop.ColumnsUntilNext(0, 4));
			Assert.AreEqual(4, TabStop.ColumnsUntilNext(4, 4));
			Assert.AreEqual(4, TabStop.ColumnsUntilNext(8, 4));
		}

		[Test]
		public void A_Column_Between_Tab_Stops_Reaches_The_Next_One()
		{
			Assert.AreEqual(3, TabStop.ColumnsUntilNext(1, 4));
			Assert.AreEqual(2, TabStop.ColumnsUntilNext(2, 4));
			Assert.AreEqual(1, TabStop.ColumnsUntilNext(3, 4));
		}

		[Test]
		public void Every_Column_Is_A_Tab_Stop_When_The_Indentation_Size_Is_One()
		{
			Assert.AreEqual(1, TabStop.ColumnsUntilNext(0, 1));
			Assert.AreEqual(1, TabStop.ColumnsUntilNext(1, 1));
			Assert.AreEqual(1, TabStop.ColumnsUntilNext(2, 1));
		}
	}
}
