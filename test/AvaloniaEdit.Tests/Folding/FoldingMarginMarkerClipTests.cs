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
using CollectionAssert = NUnit.Framework.Legacy.CollectionAssert;

namespace AvaloniaEdit.Folding
{
    [TestFixture]
    public class FoldingMarginMarkerClipTests
    {
        /// <summary>A text view with one open folding over its first four lines, and a folding margin beside it.</summary>
        private static FoldingMargin CreateMargin()
        {
            var document = new TextDocument("BEGIN\n  one\n  two\nEND\nafter");
            var textView = new TextView { Document = document };
            var manager = new FoldingManager(document);
            manager.CreateFolding(0, document.GetLineByNumber(4).EndOffset);
            var margin = new FoldingMargin { FoldingManager = manager, TextView = textView };

            textView.Measure(new Size(400, 300));
            textView.Arrange(new Rect(0, 0, 400, 300));
            textView.EnsureVisualLines();
            margin.Measure(new Size(20, 300));
            margin.Arrange(new Rect(0, 0, 20, 300));
            return margin;
        }

        /// <summary>Each line the margin draws, with the clips of the groups it sits in. (The markers are child controls
        /// and draw themselves.)</summary>
        private static List<(GeometryDrawing Line, List<Geometry> Clips)> RenderedLines(FoldingMargin margin)
        {
            var group = new DrawingGroup();
            using (var context = group.Open())
            {
                margin.Render(context);
            }
            var found = new List<(GeometryDrawing, List<Geometry>)>();
            void Walk(DrawingGroup g, List<Geometry> clips)
            {
                var inner = g.ClipGeometry is { } clip ? new List<Geometry>(clips) { clip } : clips;
                foreach (var child in g.Children)
                {
                    if (child is DrawingGroup nested) Walk(nested, inner);
                    else if (child is GeometryDrawing { Pen: not null } line) found.Add((line, inner));
                }
            }
            Walk(group, new List<Geometry>());
            return found;
        }

        [AvaloniaTest]
        public void Fold_Lines_Should_Leave_Out_The_Markers()
        {
            var margin = CreateMargin();
            margin.FoldingMarkerBackgroundBrush = Brushes.Transparent;
            var marker = margin.GetVisualChildren().OfType<FoldingMarginMarker>().Single();
            Assert.IsTrue(marker.Bounds.Width > 0 && marker.Bounds.Height > 0);

            var lines = RenderedLines(margin);

            // the vertical line starts in the marker's middle: every line is drawn through a clip that is the margin less
            // the marker, so it can't show inside a marker whose background isn't opaque (headless drawing can't hit-test
            // geometry, so the clip's shape is checked)
            Assert.IsNotEmpty(lines);
            foreach (var (_, clips) in lines)
            {
                var clip = clips.OfType<CombinedGeometry>().Single();
                Assert.AreEqual(GeometryCombineMode.Exclude, clip.GeometryCombineMode);
                Assert.AreEqual(new Rect(margin.Bounds.Size), ((RectangleGeometry)clip.Geometry1).Rect);
                var left = ((GeometryGroup)clip.Geometry2).Children.OfType<RectangleGeometry>().Select(r => r.Rect).ToList();
                CollectionAssert.AreEqual(new[] { marker.Bounds }, left);
            }
        }
    }
}
