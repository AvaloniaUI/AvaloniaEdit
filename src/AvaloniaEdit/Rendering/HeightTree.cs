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

// Enable this define to use expensive consistency checks in debug builds.
// (will cause performance to degrade from O(lg N) to O(N), which may cause some operations
//  that are already linear to go quadratic)
//#define DATACONSISTENCYTEST

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using AvaloniaEdit.Document;

namespace AvaloniaEdit.Rendering
{
	/// <summary>
	/// A tree that maps line numbers to visual positions.
	/// The balancing of the tree work as in a B+ tree.
	/// </summary>
	internal sealed class HeightTree : ILineTracker, IDisposable
	{
		#region Constructor
		private readonly TextDocument _document;
		private HeightTreeNode _root;
		private WeakLineTracker _weakLineTracker;

		public HeightTree(TextDocument document, double defaultLineHeight)
		{
			_document = document;
			_weakLineTracker = WeakLineTracker.Register(document, this);
			this.DefaultLineHeight = defaultLineHeight;
			RebuildDocument();
		}

		public void Dispose()
		{
			if (_weakLineTracker != null)
				_weakLineTracker.Deregister();
			_root = null;
			_weakLineTracker = null;
		}

		public bool IsDisposed {
			get {
				return _root == null;
			}
		}

		private double _defaultLineHeight;

		public double DefaultLineHeight {
			get { return _defaultLineHeight; }
			set {
				var oldValue = _defaultLineHeight;
				if (oldValue == value)
					return;
				_defaultLineHeight = value;
				// update the stored value in all nodes:
				_root?.UpdateHeight(oldValue, value);
			}
		}
		#endregion

		#region RebuildDocument
		void ILineTracker.ChangeComplete(DocumentChangeEventArgs e)
		{
#if DEBUG
			//Debug.WriteLine(GetTreeAsString());
			CheckProperties();
#endif
		}

		void ILineTracker.SetLineLength(DocumentLine ls, int newTotalLength)
		{
		}

		/// <summary>
		/// Rebuild the tree, in O(n).
		/// </summary>
		public void RebuildDocument()
		{
			foreach (CollapsedLineSection s in GetAllCollapsedSections()) {
				s.Reset();
			}
			int lineCount = _document.LineCount;
			// List of inner nodes that are not yet full and not yet connected to their parent
			var innerNodes = new List<HeightTreeInnerNode>();
			innerNodes.Add(new HeightTreeInnerNode());
			// Create leaf nodes
			int pos = 0;
			while (pos < lineCount) {
				int linesInThisNode = Math.Min(lineCount - pos, HeightTreeLeafNode.MaxLineCount);
				HeightTreeLeafNode leafNode = HeightTreeLeafNode.Create(linesInThisNode, _defaultLineHeight);
				innerNodes[0].InsertChild(innerNodes[0].childCount, leafNode);
				pos += linesInThisNode;
				// Restore invariant that innerNodes are not yet full
				int level = 0;
				while (innerNodes[level].childCount == HeightTreeInnerNode.MaxChildCount) {
					if (level + 1 == innerNodes.Count) {
						innerNodes.Add(new HeightTreeInnerNode());
					}
					innerNodes[level + 1].InsertChild(innerNodes[level + 1].childCount, innerNodes[level]);
					innerNodes[level] = new HeightTreeInnerNode();
					level++;
				}
			}
			// Connect inner nodes
			for (int level = 0; level < innerNodes.Count - 1; level++) {
				innerNodes[level + 1].InsertChild(innerNodes[level + 1].childCount, innerNodes[level]);
			}
			if (innerNodes[innerNodes.Count - 1].childCount == 1) {
				// The root node is a leaf node
				_root = innerNodes[innerNodes.Count - 1].children[0];
				_root.parent = null;
			} else {
				_root = innerNodes[innerNodes.Count - 1];
			}
			Debug.Assert(_root.LineCount == lineCount);

			// All nodes except for the last in each layer are completely full, 
			// but the last nodes may be nearly empty, requiring a rebalancing
			// to establish the B+ tree invariant.
			(_root as HeightTreeInnerNode)?.RebalanceLastChild();
#if DEBUG
			//Debug.WriteLine(GetTreeAsString());
			CheckProperties();
#endif
		}
		#endregion

		#region Insert/Remove lines
		static int opId = 0;

		void ILineTracker.BeforeRemoveLine(DocumentLine line)
		{
			//Debug.WriteLine($"#{++opId} BeforeRemoveLine " + line.LineNumber);
			//Debug.WriteLine(GetTreeAsString());
			_root.DeleteLine(line.LineNumber - 1, null, null);
			if (_root is HeightTreeInnerNode { childCount: 1 } innerNode) {
				// Reduce the height of the tree by one level
				_root = innerNode.children[0];
				_root.parent = null;
			}
			//Debug.WriteLine(GetTreeAsString());
			// CheckProperties would fail here because the line numbers are not updated yet
			// We will call it in ChangeComplete.
		}

		void ILineTracker.LineInserted(DocumentLine insertionPos, DocumentLine newLine)
		{
			//Debug.WriteLine($"#{++opId} LineInserted " + newLine.LineNumber);
			//Debug.WriteLine(GetTreeAsString());
			var newSibling = _root.InsertLine(newLine.LineNumber - 1, _defaultLineHeight);
			if (newSibling != null) {
				// Increase the height of the tree by one level
				_root = HeightTreeInnerNode.NewRoot(_root, newSibling);
			}
#if DEBUG
			//Debug.WriteLine(GetTreeAsString());
			CheckProperties();
#endif
		}
		#endregion

		#region GetLeafForLineNumber
		HeightTreeLeafNode GetLeafForLineNumber(int lineNumber, out int indexInLeaf)
		{
			HeightTreeNode node = _root;
			int line = lineNumber - 1;
			while (node is HeightTreeInnerNode inner) {
				int childIndex = inner.FindChildForLine(line, out line);
				node = inner.children[childIndex];
			}
			indexInLeaf = line;
			return (HeightTreeLeafNode)node;
		}
		#endregion

		#region Public methods
		public int GetLineByVisualPosition(double position)
		{
			int result = 1;
			HeightTreeNode node = _root;
			while (node is HeightTreeInnerNode inner) {
				int childIndex = inner.FindChildForVisualPosition(position, out position);
				result += inner.GetTotalLineCountUntilChildIndex(childIndex);
				node = inner.children[childIndex];
			}
			result += ((HeightTreeLeafNode)node).FindChildForVisualPosition(position);
			return result;
		}

		public double GetVisualPosition(int lineNumber)
		{
			double result = 0;
			HeightTreeNode node = _root;
			int line = lineNumber - 1;
			while (node is HeightTreeInnerNode inner) {
				int childIndex = inner.FindChildForLine(line, out line);
				result += inner.GetTotalHeightUntilChildIndex(childIndex);
				if ((inner.collapsed & (1 << childIndex)) != 0) {
					// The child is collapsed, so we can skip the rest of the tree
					return result;
				}
				node = inner.children[childIndex];
			}
			result += ((HeightTreeLeafNode)node).GetTotalHeightUntilChildIndex(line);
			return result;
		}

		public double GetVisualPosition(DocumentLine line)
		{
			return GetVisualPosition(line.LineNumber);
		}

		public double GetHeight(int lineNumber)
		{
			var leaf = GetLeafForLineNumber(lineNumber, out int indexInLeaf);
			return leaf.GetHeight(indexInLeaf);
		}

		public double GetHeight(DocumentLine line)
		{
			return GetHeight(line.LineNumber);
		}

		public void SetHeight(int lineNumber, double val)
		{
			_root.SetHeight(lineNumber - 1, val);
		}

		public void SetHeight(DocumentLine line, double val)
		{
			SetHeight(line.LineNumber, val);
		}

		public bool GetIsCollapsed(int lineNumber)
		{
			return _root.GetIsCollapsed(lineNumber - 1);
		}

		/// <summary>
		/// Collapses the specified text section.
		/// Runtime: O(log n)
		/// </summary>
		public CollapsedLineSection CollapseText(DocumentLine start, DocumentLine end)
		{
			if (!_document.Lines.Contains(start))
				throw new ArgumentException("Line is not part of this document", nameof(start));
			if (!_document.Lines.Contains(end))
				throw new ArgumentException("Line is not part of this document", nameof(end));
			// Our start/end parameters are both inclusive
			int startLineNumber = start.LineNumber;
			int endLineNumber = end.LineNumber;
			if (startLineNumber > endLineNumber)
				throw new ArgumentException("start must be a line before end");
			CollapsedLineSection section = new CollapsedLineSection(this, start, end);
			_root.AddCollapsedSection(startLineNumber - 1, endLineNumber - 1, section);
			//Debug.WriteLine(GetTreeAsString());
#if DEBUG
			CheckProperties();
#endif
			return section;
		}
		#endregion

		#region LineCount & TotalHeight
		public int LineCount {
			get {
				return _root.LineCount;
			}
		}

		public double TotalHeight {
			get {
				return _root.TotalHeight;
			}
		}
		#endregion

		#region GetAllCollapsedSections
		internal IEnumerable<CollapsedLineSection> GetAllCollapsedSections()
		{
			return _root?.GetAllCollapsedSections(HeightTreeNode.EventKind.Start) ?? Enumerable.Empty<CollapsedLineSection>();
		}
		#endregion

		#region CheckProperties
#if DEBUG
		[Conditional("DATACONSISTENCYTEST")]
		void CheckProperties()
		{
			_root?.CheckInvariant(true, 1);
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		internal string GetTreeAsString()
		{
			StringBuilder b = new StringBuilder();
			_root?.AppendTreeToString(b, 0, 1);
			return b.ToString();
		}
#endif
		#endregion
	}
}
