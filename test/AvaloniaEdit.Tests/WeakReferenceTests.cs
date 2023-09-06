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

#if !DEBUG
using System;
using System.Runtime.CompilerServices;
using Avalonia.Headless.NUnit;
using AvaloniaEdit.AvaloniaMocks;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using NUnit.Framework;

namespace AvaloniaEdit
{
    [TestFixture]
    public class WeakReferenceTests
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference CreateControl<T>(Action<T> action = null)
            where T : class, new()
        {
            WeakReference wr;

            var control = new T();
            wr = new WeakReference(control);
            action?.Invoke(control);
             control = null;

            GarbageCollect();

            return wr;
        }

        //[AvaloniaTest] currently failing due to Headless platform doesn't behave as the previous UnitTestApplication
        public void TextViewCanBeCollectedTest()
        {
            var wr = CreateControl<TextView>();
            Assert.IsFalse(wr.IsAlive);
        }

        //[AvaloniaTest] currently failing due to Headless platform doesn't behave as the previous UnitTestApplication
        public void DocumentDoesNotHoldReferenceToTextView()
        {
            TextDocument textDocument = new TextDocument();
            Assert.AreEqual(0, textDocument.LineTrackers.Count);

            var wr = CreateControl<TextView>(t => t.Document = textDocument);

            Assert.IsFalse(wr.IsAlive);
            // document cannot immediately clear the line tracker
            Assert.AreEqual(1, textDocument.LineTrackers.Count);

            // but it should clear it on the next change
            textDocument.Insert(0, "a");
            Assert.AreEqual(0, textDocument.LineTrackers.Count);
        }

        //[AvaloniaTest] // currently fails due to some Avalonia static
        void DocumentDoesNotHoldReferenceToTextArea()
        {
            var textDocument = new TextDocument();
            var wr = CreateControl<TextArea>(t => t.Document = textDocument);
            Assert.IsFalse(wr.IsAlive);
            GC.KeepAlive(textDocument);
        }

        //[AvaloniaTest] // currently fails due to some Avalonia static
        void DocumentDoesNotHoldReferenceToTextEditor()
        {
            var textDocument = new TextDocument();
            var wr = CreateControl<TextEditor>(t => t.Document = textDocument);
            Assert.IsFalse(wr.IsAlive);
            GC.KeepAlive(textDocument);
        }

        //[AvaloniaTest] currently failing due to Headless platform doesn't behave as the previous UnitTestApplication
        public void DocumentDoesNotHoldReferenceToLineMargin()
        {
            TextDocument textDocument = new TextDocument();
            var wr = CreateControl<TextView>(t =>
            {
                t.Document = textDocument;
                new LineNumberMargin { TextView = t };
            });

            Assert.IsFalse(wr.IsAlive);
            GC.KeepAlive(textDocument);
        }

        static void GarbageCollect()
        {
            for (int i = 0; i < 3; i++)
            {
                GC.WaitForPendingFinalizers();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            }
        }
    }
}
#endif