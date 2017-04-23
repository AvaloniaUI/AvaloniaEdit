// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System.Diagnostics.CodeAnalysis;
using AvaloniaEdit.Document;

namespace AvaloniaEdit.Rendering
{
	/// <summary>
	/// Represents a collapsed line section.
	/// Use the Uncollapse() method to uncollapse the section.
	/// </summary>
	public sealed class CollapsedLineSection
	{
	    private DocumentLine _start;
	    private DocumentLine _end;
	    private readonly HeightTree _heightTree;
		
		#if DEBUG
		internal string Id;
	    private static int _nextId;
		#else
		internal const string Id = "";
		#endif
		
		internal CollapsedLineSection(HeightTree heightTree, DocumentLine start, DocumentLine end)
		{
			_heightTree = heightTree;
			_start = start;
			_end = end;
			#if DEBUG
			unchecked {
				Id = " #" + (_nextId++);
			}
			#endif
		}
		
		/// <summary>
		/// Gets if the document line is collapsed.
		/// This property initially is true and turns to false when uncollapsing the section.
		/// </summary>
		public bool IsCollapsed => _start != null;

	    /// <summary>
		/// Gets the start line of the section.
		/// When the section is uncollapsed or the text containing it is deleted,
		/// this property returns null.
		/// </summary>
		public DocumentLine Start {
			get => _start;
	        internal set => _start = value;
	    }
		
		/// <summary>
		/// Gets the end line of the section.
		/// When the section is uncollapsed or the text containing it is deleted,
		/// this property returns null.
		/// </summary>
		public DocumentLine End {
			get => _end;
		    internal set => _end = value;
		}
		
		/// <summary>
		/// Uncollapses the section.
		/// This causes the Start and End properties to be set to null!
		/// Does nothing if the section is already uncollapsed.
		/// </summary>
		public void Uncollapse()
		{
			if (_start == null)
				return;
			
			_heightTree.Uncollapse(this);
			#if DEBUG
			//heightTree.CheckProperties();
			#endif
			
			_start = null;
			_end = null;
		}
		
		/// <summary>
		/// Gets a string representation of the collapsed section.
		/// </summary>
		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.Int32.ToString")]
		public override string ToString()
		{
			return "[CollapsedSection" + Id + " Start=" + (_start != null ? _start.LineNumber.ToString() : "null")
				+ " End=" + (_end != null ? _end.LineNumber.ToString() : "null") + "]";
		}
	}
}
