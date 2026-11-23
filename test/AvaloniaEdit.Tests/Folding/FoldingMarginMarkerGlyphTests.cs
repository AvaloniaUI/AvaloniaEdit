using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.VisualTree;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace AvaloniaEdit.Folding
{
    [TestFixture]
    public class FoldingMarginMarkerGlyphTests
    {
        /// <summary>A text view with one folding over its first four lines, folded or not, and a folding margin beside it
        /// in a marker brush and a different selected (hover) brush.</summary>
        private static (FoldingMargin Margin, Visual Marker) CreateMarker(bool folded)
        {
            var document = new TextDocument("BEGIN\n  one\n  two\nEND\nafter");
            var textView = new TextView { Document = document };
            var manager = new FoldingManager(document);
            manager.CreateFolding(0, document.GetLineByNumber(4).EndOffset).IsFolded = folded;
            var margin = new FoldingMargin
            {
                FoldingManager = manager,
                TextView = textView,
                FoldingMarkerBrush = Brushes.Gray,
                SelectedFoldingMarkerBrush = Brushes.Black,
            };

            textView.Measure(new Size(400, 300));
            textView.Arrange(new Rect(0, 0, 400, 300));
            textView.EnsureVisualLines();
            margin.Measure(new Size(20, 300));
            margin.Arrange(new Rect(0, 0, 20, 300));
            return (margin, margin.GetVisualChildren().OfType<FoldingMarginMarker>().Single());
        }

        /// <summary>The strokes a marker draws: its box, then its minus (and the plus's upright when folded).</summary>
        private static List<IPen> Strokes(Visual marker)
        {
            var group = new DrawingGroup();
            using (var context = group.Open())
            {
                marker.Render(context);
            }
            var pens = new List<IPen>();
            void Walk(DrawingGroup g)
            {
                foreach (var child in g.Children)
                {
                    if (child is DrawingGroup nested) Walk(nested);
                    else if (child is GeometryDrawing { Pen: { } pen }) pens.Add(pen);
                }
            }
            Walk(group);
            return pens;
        }

        private static string Describe(List<IPen> pens) => "drawn in: " + string.Join(", ", pens.Select(p => p.Brush));

        [AvaloniaTest]
        [TestCase(false, 2)] // the box and the minus
        [TestCase(true, 3)] // the box and the plus
        public void By_Default_The_Sign_Should_Be_In_The_Selected_Brush_As_Before(bool folded, int strokes)
        {
            var (margin, marker) = CreateMarker(folded);

            var pens = Strokes(marker);

            Assert.IsNull(margin.FoldingMarkerSignBrush);
            Assert.AreEqual(strokes, pens.Count);
            Assert.AreSame(Brushes.Gray, pens[0].Brush, Describe(pens)); // the box
            Assert.IsTrue(pens.Skip(1).All(p => ReferenceEquals(p.Brush, Brushes.Black)), Describe(pens)); // the sign
        }

        [AvaloniaTest]
        [TestCase(false, 2)]
        [TestCase(true, 3)]
        public void The_Sign_Should_Take_The_Sign_Brush_When_One_Is_Set(bool folded, int strokes)
        {
            var (margin, marker) = CreateMarker(folded);

            margin.FoldingMarkerSignBrush = Brushes.Gray;

            var pens = Strokes(marker);
            Assert.AreEqual(strokes, pens.Count);
            Assert.IsTrue(pens.All(p => ReferenceEquals(p.Brush, Brushes.Gray)), Describe(pens));
        }

        [AvaloniaTest]
        public void The_Sign_Brush_Should_Come_From_A_Parent()
        {
            var parent = new Avalonia.Controls.StackPanel();
            var (margin, marker) = CreateMarker(folded: true);
            parent.Children.Add(margin);

            parent.SetValue(FoldingMargin.FoldingMarkerSignBrushProperty, Brushes.Red);

            var pens = Strokes(marker);
            Assert.IsTrue(pens.Skip(1).All(p => ReferenceEquals(p.Brush, Brushes.Red)), Describe(pens));
        }
    }
}
