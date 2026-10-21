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

using AvaloniaEdit.Converters;
using System;
using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace AvaloniaEdit.Converters
{
    [TestFixture]
    public class TextLengthToBoolConverterTests
    {
        [Test]
        public void ConvertZeroReturnsTrue()
        {
            Assert.IsTrue((Boolean)TextLengthToBoolConverter.Instance.Convert(0, null, null, null),
                "Zero text length should indicate empty text");
        }
        
        [Test]
        public void ConvertNonZeroReturnsFalse()
        {
            Assert.IsFalse((Boolean)TextLengthToBoolConverter.Instance.Convert(1, null, null, null),
                "Non-zero text length should indicate non-empty text");
            Assert.IsFalse((Boolean)TextLengthToBoolConverter.Instance.Convert(42, null, null, null),
                "Non-zero text length should indicate non-empty text");
        }
        
        [Test]
        public void ConvertNullReturnsTrue()
        {
            Assert.IsTrue((Boolean)TextLengthToBoolConverter.Instance.Convert(null, null, null, null),
                "Null should indicate empty text");
        }
        
        [Test]
        public void ConvertBackThrows()
        {
            Assert.Throws<NotSupportedException>(
                () => TextLengthToBoolConverter.Instance.ConvertBack(null, null, null, null),
                "ConvertBack must not be supported");
        }
    }
}