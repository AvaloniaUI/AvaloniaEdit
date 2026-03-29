// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Source taken from: https://github.com/dotnet/runtime/blob/v10.0.105/src/libraries/Common/tests/Tests/System/Text/ValueStringBuilderTests.cs
// and converted from XUnit to NUnit tests.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using AvaloniaEdit.Utils;
using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace AvaloniaEdit.Tests.Utils
{
    [SuppressMessage("Reliability", "S2930: IDisposables should be disposed", Justification = "Just unit tests")]
    [TestFixture]
    public class ValueStringBuilderTests
    {
        [Test]
        public void Ctor_Default_CanAppend()
        {
            // Arrange
            ValueStringBuilder vsb = default;

            // Act
            int initialLength = vsb.Length;
            vsb.Append('a');
            int finalLength = vsb.Length;
            string result = vsb.ToString();

            // Assert
            Assert.AreEqual(0, initialLength);
            Assert.AreEqual(1, finalLength);
            Assert.AreEqual("a", result);
        }

        [Test]
        public void Ctor_Span_CanAppend()
        {
            // Arrange
            ValueStringBuilder vsb = new ValueStringBuilder(new char[1]);

            // Act
            int initialLength = vsb.Length;
            vsb.Append('a');
            int finalLength = vsb.Length;
            string result = vsb.ToString();

            // Assert
            Assert.AreEqual(0, initialLength);
            Assert.AreEqual(1, finalLength);
            Assert.AreEqual("a", result);
        }

        [Test]
        public void Ctor_InitialCapacity_CanAppend()
        {
            // Arrange
            ValueStringBuilder vsb = new ValueStringBuilder(1);

            // Act
            int initialLength = vsb.Length;
            vsb.Append('a');
            int finalLength = vsb.Length;
            string result = vsb.ToString();

            // Assert
            Assert.AreEqual(0, initialLength);
            Assert.AreEqual(1, finalLength);
            Assert.AreEqual("a", result);
        }

        [Test]
        public void Append_Char_MatchesStringBuilder()
        {
            // Arrange
            StringBuilder sb = new StringBuilder();
            ValueStringBuilder vsb = new ValueStringBuilder();

            // Act
            for (int i = 1; i <= 100; i++)
            {
                sb.Append((char)i);
                vsb.Append((char)i);
            }

            int actualLength = vsb.Length;
            int expectedLength = sb.Length;
            string actual = vsb.ToString();
            string expected = sb.ToString();

            // Assert
            Assert.AreEqual(expectedLength, actualLength);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Append_String_MatchesStringBuilder()
        {
            // Arrange
            StringBuilder sb = new StringBuilder();
            ValueStringBuilder vsb = new ValueStringBuilder();

            // Act
            for (int i = 1; i <= 100; i++)
            {
                string s = i.ToString();
                sb.Append(s);
                vsb.Append(s);
            }

            int actualLength = vsb.Length;
            int expectedLength = sb.Length;
            string actual = vsb.ToString();
            string expected = sb.ToString();

            // Assert
            Assert.AreEqual(expectedLength, actualLength);
            Assert.AreEqual(expected, actual);
        }

        [TestCase(0, 4 * 1024 * 1024)]
        [TestCase(1025, 4 * 1024 * 1024)]
        [TestCase(3 * 1024 * 1024, 6 * 1024 * 1024)]
        public void Append_String_Large_MatchesStringBuilder(int initialLength, int stringLength)
        {
            // Arrange
            StringBuilder sb = new StringBuilder(initialLength);
            ValueStringBuilder vsb = new ValueStringBuilder(new char[initialLength]);
            string s = new string('a', stringLength);

            // Act
            sb.Append(s);
            vsb.Append(s);

            int actualLength = vsb.Length;
            int expectedLength = sb.Length;
            string actual = vsb.ToString();
            string expected = sb.ToString();

            // Assert
            Assert.AreEqual(expectedLength, actualLength);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Append_CharInt_MatchesStringBuilder()
        {
            // Arrange
            StringBuilder sb = new StringBuilder();
            ValueStringBuilder vsb = new ValueStringBuilder();

            // Act
            for (int i = 1; i <= 100; i++)
            {
                sb.Append((char)i, i);
                vsb.Append((char)i, i);
            }

            int actualLength = vsb.Length;
            int expectedLength = sb.Length;
            string actual = vsb.ToString();
            string expected = sb.ToString();

            // Assert
            Assert.AreEqual(expectedLength, actualLength);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void AppendSpan_Capacity()
        {
            // Arrange
            ValueStringBuilder vsb = new ValueStringBuilder();

            // Act
            vsb.AppendSpan(17);
            int capacityAfterFirstAppendSpan = vsb.Capacity;

            vsb.AppendSpan(100);
            int capacityAfterSecondAppendSpan = vsb.Capacity;

            // Assert
            Assert.AreEqual(32, capacityAfterFirstAppendSpan);
            Assert.AreEqual(128, capacityAfterSecondAppendSpan);
        }

        [Test]
        public void AppendSpan_DataAppendedCorrectly()
        {
            // Arrange
            StringBuilder sb = new StringBuilder();
            ValueStringBuilder vsb = new ValueStringBuilder();

            // Act
            for (int i = 1; i <= 1000; i++)
            {
                string s = i.ToString();

                sb.Append(s);

                Span<char> span = vsb.AppendSpan(s.Length);
                s.AsSpan().CopyTo(span);
            }

            int actualLength = vsb.Length;
            int expectedLength = sb.Length;
            string actual = vsb.ToString();
            string expected = sb.ToString();

            // Assert
            Assert.AreEqual(expectedLength, actualLength);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Insert_IntCharInt_MatchesStringBuilder()
        {
            // Arrange
            StringBuilder sb = new StringBuilder();
            ValueStringBuilder vsb = new ValueStringBuilder();
            Random rand = new Random(42);

            // Act
            for (int i = 1; i <= 100; i++)
            {
                int index = rand.Next(sb.Length);
                sb.Insert(index, new string((char)i, 1), i);
                vsb.Insert(index, (char)i, i);
            }

            int actualLength = vsb.Length;
            int expectedLength = sb.Length;
            string actual = vsb.ToString();
            string expected = sb.ToString();

            // Assert
            Assert.AreEqual(expectedLength, actualLength);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Insert_IntString_MatchesStringBuilder()
        {
            // Arrange
            StringBuilder sb = new StringBuilder();
            ValueStringBuilder vsb = new ValueStringBuilder();

            // Act
            sb.Insert(0, new string('a', 6));
            vsb.Insert(0, new string('a', 6));
            int lengthAfterFirstInsert = vsb.Length;
            int capacityAfterFirstInsert = vsb.Capacity;

            sb.Insert(0, new string('b', 11));
            vsb.Insert(0, new string('b', 11));
            int lengthAfterSecondInsert = vsb.Length;
            int capacityAfterSecondInsert = vsb.Capacity;

            sb.Insert(0, new string('c', 15));
            vsb.Insert(0, new string('c', 15));
            int lengthAfterThirdInsert = vsb.Length;
            int capacityAfterThirdInsert = vsb.Capacity;

            sb.Length = 24;
            vsb.Length = 24;

            sb.Insert(0, new string('d', 40));
            vsb.Insert(0, new string('d', 40));
            int finalLength = vsb.Length;
            int finalCapacity = vsb.Capacity;
            string actual = vsb.ToString();
            string expected = sb.ToString();

            // Assert
            Assert.AreEqual(6, lengthAfterFirstInsert);
            Assert.AreEqual(16, capacityAfterFirstInsert);

            Assert.AreEqual(17, lengthAfterSecondInsert);
            Assert.AreEqual(32, capacityAfterSecondInsert);

            Assert.AreEqual(32, lengthAfterThirdInsert);
            Assert.AreEqual(32, capacityAfterThirdInsert);

            Assert.AreEqual(64, finalLength);
            Assert.AreEqual(64, finalCapacity);

            Assert.AreEqual(sb.Length, finalLength);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void AsSpan_ReturnsCorrectValue_DoesntClearBuilder()
        {
            // Arrange
            StringBuilder sb = new StringBuilder();
            ValueStringBuilder vsb = new ValueStringBuilder();

            for (int i = 1; i <= 100; i++)
            {
                string s = i.ToString();
                sb.Append(s);
                vsb.Append(s);
            }

            // Act
            string resultString = new string(vsb.AsSpan());
            int stringBuilderLength = sb.Length;
            int valueStringBuilderLength = vsb.Length;
            string valueStringBuilderString = vsb.ToString();
            string stringBuilderString = sb.ToString();

            // Assert
            Assert.AreEqual(stringBuilderString, resultString);
            Assert.AreNotEqual(0, stringBuilderLength);
            Assert.AreEqual(stringBuilderLength, valueStringBuilderLength);
            Assert.AreEqual(stringBuilderString, valueStringBuilderString);
        }

        [Test]
        public void ToString_ClearsBuilder_ThenReusable()
        {
            // Arrange
            const string text1 = "test";
            const string text2 = "another test";
            ValueStringBuilder vsb = new ValueStringBuilder();

            vsb.Append(text1);

            // Act
            int lengthBeforeToString = vsb.Length;
            string firstResult = vsb.ToString();
            int lengthAfterToString = vsb.Length;
            string secondResult = vsb.ToString();

            vsb.Append(text2);
            int finalLength = vsb.Length;
            string finalResult = vsb.ToString();

            // Assert
            Assert.AreEqual(text1.Length, lengthBeforeToString);
            Assert.AreEqual(text1, firstResult);
            Assert.AreEqual(0, lengthAfterToString);
            Assert.AreEqual(string.Empty, secondResult);
            Assert.AreEqual(text2.Length, finalLength);
            Assert.AreEqual(text2, finalResult);
        }

        [Test]
        public void Dispose_ClearsBuilder_ThenReusable()
        {
            // Arrange
            const string text1 = "test";
            const string text2 = "another test";
            ValueStringBuilder vsb = new ValueStringBuilder();

            vsb.Append(text1);

            // Act
            int lengthBeforeDispose = vsb.Length;
            vsb.Dispose();
            int lengthAfterDispose = vsb.Length;
            string resultAfterDispose = vsb.ToString();

            vsb.Append(text2);
            int finalLength = vsb.Length;
            string finalResult = vsb.ToString();

            // Assert
            Assert.AreEqual(text1.Length, lengthBeforeDispose);
            Assert.AreEqual(0, lengthAfterDispose);
            Assert.AreEqual(string.Empty, resultAfterDispose);
            Assert.AreEqual(text2.Length, finalLength);
            Assert.AreEqual(text2, finalResult);
        }

        [Test]
        public void Indexer()
        {
            // Arrange
            const string text1 = "foobar";
            ValueStringBuilder vsb = new ValueStringBuilder();
            vsb.Append(text1);

            // Act
            char originalCharacter = vsb[3];
            vsb[3] = 'c';
            char updatedCharacter = vsb[3];
            vsb.Dispose();

            // Assert
            Assert.AreEqual('b', originalCharacter);
            Assert.AreEqual('c', updatedCharacter);
        }

        [Test]
        public void EnsureCapacity_IfRequestedCapacityWins()
        {
            // Arrange
            // Note: constants used here may be dependent on minimal buffer size
            // the ArrayPool is able to return.
            ValueStringBuilder builder = new ValueStringBuilder(stackalloc char[32]);

            // Act
            builder.EnsureCapacity(65);
            int capacity = builder.Capacity;

            // Assert
            Assert.AreEqual(128, capacity);
        }

        [Test]
        public void EnsureCapacity_IfBufferTimesTwoWins()
        {
            // Arrange
            ValueStringBuilder builder = new ValueStringBuilder(stackalloc char[32]);

            // Act
            builder.EnsureCapacity(33);
            int capacity = builder.Capacity;
            builder.Dispose();

            // Assert
            Assert.AreEqual(64, capacity);
        }

        [Test]
        public void EnsureCapacity_NoAllocIfNotNeeded()
        {
            // Arrange
            // Note: constants used here may be dependent on minimal buffer size
            // the ArrayPool is able to return.
            ValueStringBuilder builder = new ValueStringBuilder(stackalloc char[64]);

            // Act
            builder.EnsureCapacity(16);
            int capacity = builder.Capacity;
            builder.Dispose();

            // Assert
            Assert.AreEqual(64, capacity);
        }
    }
}
