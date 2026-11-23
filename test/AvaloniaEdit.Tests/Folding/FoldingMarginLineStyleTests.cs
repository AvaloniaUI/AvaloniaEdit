using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
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
    public class FoldingMarginLineStyleTests
    {
        /// <summary>A text view with one open folding over its first four lines, and a folding margin beside it,
        /// inside <paramref name="parent"/> when one is given.</summary>
        private static FoldingMargin CreateMargin(Panel parent = null)
        {
            var document = new TextDocument("BEGIN\n  one\n  two\nEND\nafter");
            var textView = new TextView { Document = document };
            var manager = new FoldingManager(document);
            manager.CreateFolding(0, document.GetLineByNumber(4).EndOffset);
            var margin = new FoldingMargin { FoldingManager = manager, TextView = textView };
            parent?.Children.Add(margin);

            textView.Measure(new Size(400, 300));
            textView.Arrange(new Rect(0, 0, 400, 300));
            textView.EnsureVisualLines();
            margin.Measure(new Size(20, 300));
            margin.Arrange(new Rect(0, 0, 20, 300));
            return margin;
        }

        /// <summary>The lines the margin draws, wherever they sit in the drawing (the markers are child controls and draw
        /// themselves).</summary>
        private static List<GeometryDrawing> RenderedLines(FoldingMargin margin)
        {
            var group = new DrawingGroup();
            using (var context = group.Open())
            {
                margin.Render(context);
            }
            var found = new List<GeometryDrawing>();
            void Walk(DrawingGroup g)
            {
                foreach (var child in g.Children)
                {
                    if (child is DrawingGroup nested) Walk(nested);
                    else if (child is GeometryDrawing { Pen: not null } line) found.Add(line);
                }
            }
            Walk(group);
            return found;
        }

        [AvaloniaTest]
        public void Fold_Lines_Should_Be_Solid_And_Shown_By_Default()
        {
            var margin = CreateMargin();

            Assert.IsTrue(margin.ShowFoldingLines);
            Assert.IsNull(margin.FoldingLineDashStyle);
            var lines = RenderedLines(margin);
            Assert.IsNotEmpty(lines);
            Assert.IsTrue(lines.All(l => l.Pen.DashStyle == null));
        }

        [AvaloniaTest]
        public void Fold_Lines_Should_Use_The_Dash_Style()
        {
            var margin = CreateMargin();

            margin.FoldingLineDashStyle = DashStyle.Dash;

            var lines = RenderedLines(margin);
            Assert.IsNotEmpty(lines);
            Assert.IsTrue(lines.All(l => ReferenceEquals(l.Pen.DashStyle, DashStyle.Dash)));
        }

        [AvaloniaTest]
        public void Fold_Lines_Should_Keep_The_Dash_Style_When_The_Brush_Changes()
        {
            var margin = CreateMargin();
            var dots = new DashStyle(new[] { 1.0, 2.0 }, 0);
            margin.FoldingLineDashStyle = dots;

            margin.FoldingMarkerBrush = Brushes.Red;

            var lines = RenderedLines(margin);
            Assert.IsNotEmpty(lines);
            Assert.IsTrue(lines.All(l => ReferenceEquals(l.Pen.DashStyle, dots) && ReferenceEquals(l.Pen.Brush, Brushes.Red)));
        }

        [AvaloniaTest]
        public void Fold_Lines_Should_Take_The_Dash_Style_From_A_Parent()
        {
            var parent = new StackPanel();
            var margin = CreateMargin(parent);

            parent.SetValue(FoldingMargin.FoldingLineDashStyleProperty, DashStyle.Dash);

            Assert.AreSame(DashStyle.Dash, margin.FoldingLineDashStyle);
            var lines = RenderedLines(margin);
            Assert.IsNotEmpty(lines);
            Assert.IsTrue(lines.All(l => ReferenceEquals(l.Pen.DashStyle, DashStyle.Dash)));
        }

        [AvaloniaTest]
        public void Hidden_Fold_Lines_Should_Not_Be_Drawn_But_The_Markers_Stay()
        {
            var margin = CreateMargin();

            margin.ShowFoldingLines = false;

            Assert.IsEmpty(RenderedLines(margin));
            Assert.AreEqual(1, margin.GetVisualChildren().OfType<FoldingMarginMarker>().Count());
        }

        [AvaloniaTest]
        public void Fold_Lines_Should_Come_Back_When_Shown_Again()
        {
            var margin = CreateMargin();
            margin.ShowFoldingLines = false;

            margin.ShowFoldingLines = true;

            Assert.IsNotEmpty(RenderedLines(margin));
        }
    }
}
