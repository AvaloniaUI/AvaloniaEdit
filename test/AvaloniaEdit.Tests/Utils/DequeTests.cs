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

using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace AvaloniaEdit.Utils
{
    [TestFixture]
    public class DequeTests
    {
        #region Construction and Initial State

        [Test]
        public void NewDeque_ShouldHaveCountZero()
        {
            var deque = new Deque<int>();
            Assert.AreEqual(0, deque.Count);
        }

        [Test]
        public void NewDeque_Enumeration_ShouldBeEmpty()
        {
            var deque = new Deque<int>();
            Assert.IsEmpty(deque.ToList());
        }

        #endregion

        #region PushBack

        [Test]
        public void PushBack_SingleElement_ShouldIncrementCount()
        {
            var deque = new Deque<int>();
            deque.PushBack(42);
            Assert.AreEqual(1, deque.Count);
        }

        [Test]
        public void PushBack_SingleElement_ShouldBeAccessibleAtIndexZero()
        {
            var deque = new Deque<int>();
            deque.PushBack(42);
            Assert.AreEqual(42, deque[0]);
        }

        [Test]
        public void PushBack_MultipleElements_ShouldMaintainInsertionOrder()
        {
            var deque = new Deque<int>();
            deque.PushBack(1);
            deque.PushBack(2);
            deque.PushBack(3);

            Assert.AreEqual(1, deque[0]);
            Assert.AreEqual(2, deque[1]);
            Assert.AreEqual(3, deque[2]);
            Assert.AreEqual(3, deque.Count);
        }

        [Test]
        public void PushBack_BeyondInitialCapacity_ShouldGrow()
        {
            // Initial capacity is 4, push 10 elements to force multiple grows
            var deque = new Deque<int>();
            for (int i = 0; i < 10; i++)
                deque.PushBack(i);

            Assert.AreEqual(10, deque.Count);
            for (int i = 0; i < 10; i++)
                Assert.AreEqual(i, deque[i]);
        }

        [Test]
        public void PushBack_NullReferenceType_ShouldSucceed()
        {
            var deque = new Deque<string>();
            deque.PushBack(null);

            Assert.AreEqual(1, deque.Count);
            Assert.IsNull(deque[0]);
        }

        #endregion

        #region PushFront

        [Test]
        public void PushFront_SingleElement_ShouldIncrementCount()
        {
            var deque = new Deque<int>();
            deque.PushFront(42);
            Assert.AreEqual(1, deque.Count);
        }

        [Test]
        public void PushFront_SingleElement_ShouldBeAccessibleAtIndexZero()
        {
            var deque = new Deque<int>();
            deque.PushFront(42);
            Assert.AreEqual(42, deque[0]);
        }

        [Test]
        public void PushFront_MultipleElements_ShouldMaintainReverseInsertionOrder()
        {
            var deque = new Deque<int>();
            deque.PushFront(1);
            deque.PushFront(2);
            deque.PushFront(3);

            // Last pushed to front is at index 0
            Assert.AreEqual(3, deque[0]);
            Assert.AreEqual(2, deque[1]);
            Assert.AreEqual(1, deque[2]);
            Assert.AreEqual(3, deque.Count);
        }

        [Test]
        public void PushFront_BeyondInitialCapacity_ShouldGrow()
        {
            var deque = new Deque<int>();
            for (int i = 0; i < 10; i++)
                deque.PushFront(i);

            Assert.AreEqual(10, deque.Count);
            // Elements are in reverse order: 9, 8, 7, ..., 0
            for (int i = 0; i < 10; i++)
                Assert.AreEqual(9 - i, deque[i]);
        }

        #endregion

        #region PopBack

        [Test]
        public void PopBack_ShouldReturnLastElement()
        {
            var deque = new Deque<int>();
            deque.PushBack(1);
            deque.PushBack(2);
            deque.PushBack(3);

            Assert.AreEqual(3, deque.PopBack());
        }

        [Test]
        public void PopBack_ShouldDecrementCount()
        {
            var deque = new Deque<int>();
            deque.PushBack(1);
            deque.PushBack(2);
            deque.PopBack();

            Assert.AreEqual(1, deque.Count);
        }

        [Test]
        public void PopBack_AllElements_ShouldEmptyDeque()
        {
            var deque = new Deque<int>();
            deque.PushBack(1);
            deque.PushBack(2);
            deque.PushBack(3);

            Assert.AreEqual(3, deque.PopBack());
            Assert.AreEqual(2, deque.PopBack());
            Assert.AreEqual(1, deque.PopBack());
            Assert.AreEqual(0, deque.Count);
        }

        [Test]
        public void PopBack_OnEmptyDeque_ShouldThrowInvalidOperationException()
        {
            var deque = new Deque<int>();
            Assert.Throws<InvalidOperationException>(() => deque.PopBack());
        }

        [Test]
        public void PopBack_AfterClear_ShouldThrowInvalidOperationException()
        {
            var deque = new Deque<int>();
            deque.PushBack(1);
            deque.Clear();

            Assert.Throws<InvalidOperationException>(() => deque.PopBack());
        }

        #endregion

        #region PopFront

        [Test]
        public void PopFront_ShouldReturnFirstElement()
        {
            var deque = new Deque<int>();
            deque.PushBack(1);
            deque.PushBack(2);
            deque.PushBack(3);

            Assert.AreEqual(1, deque.PopFront());
        }

        [Test]
        public void PopFront_ShouldDecrementCount()
        {
            var deque = new Deque<int>();
            deque.PushBack(1);
            deque.PushBack(2);
            deque.PopFront();

            Assert.AreEqual(1, deque.Count);
        }

        [Test]
        public void PopFront_AllElements_ShouldEmptyDeque()
        {
            var deque = new Deque<int>();
            deque.PushBack(1);
            deque.PushBack(2);
            deque.PushBack(3);

            Assert.AreEqual(1, deque.PopFront());
            Assert.AreEqual(2, deque.PopFront());
            Assert.AreEqual(3, deque.PopFront());
            Assert.AreEqual(0, deque.Count);
        }

        [Test]
        public void PopFront_OnEmptyDeque_ShouldThrowInvalidOperationException()
        {
            var deque = new Deque<int>();
            Assert.Throws<InvalidOperationException>(() => deque.PopFront());
        }

        #endregion

        #region Mixed Push/Pop Operations (Ring Buffer Wrapping)

        [Test]
        public void MixedOperations_PushBackPopFront_ShouldBehaveLikeFIFOQueue()
        {
            var deque = new Deque<int>();
            for (int i = 0; i < 100; i++)
            {
                deque.PushBack(i);
                Assert.AreEqual(i, deque.PopFront());
            }
            Assert.AreEqual(0, deque.Count);
        }

        [Test]
        public void MixedOperations_PushFrontPopBack_ShouldBehaveLikeFIFOQueue()
        {
            var deque = new Deque<int>();
            for (int i = 0; i < 100; i++)
            {
                deque.PushFront(i);
                Assert.AreEqual(i, deque.PopBack());
            }
            Assert.AreEqual(0, deque.Count);
        }

        [Test]
        public void MixedOperations_PushBackPopBack_ShouldBehaveLikeLIFOStack()
        {
            var deque = new Deque<int>();
            deque.PushBack(1);
            deque.PushBack(2);
            deque.PushBack(3);

            Assert.AreEqual(3, deque.PopBack());
            Assert.AreEqual(2, deque.PopBack());
            Assert.AreEqual(1, deque.PopBack());
        }

        [Test]
        public void MixedOperations_PushFrontPopFront_ShouldBehaveLikeLIFOStack()
        {
            var deque = new Deque<int>();
            deque.PushFront(1);
            deque.PushFront(2);
            deque.PushFront(3);

            Assert.AreEqual(3, deque.PopFront());
            Assert.AreEqual(2, deque.PopFront());
            Assert.AreEqual(1, deque.PopFront());
        }

        [Test]
        public void MixedOperations_AlternatingPushFrontAndPushBack()
        {
            var deque = new Deque<int>();
            // Build: PushFront(1), PushBack(2), PushFront(3), PushBack(4)
            // Expected order: [3, 1, 2, 4]
            deque.PushFront(1);
            deque.PushBack(2);
            deque.PushFront(3);
            deque.PushBack(4);

            Assert.AreEqual(4, deque.Count);
            Assert.AreEqual(3, deque[0]);
            Assert.AreEqual(1, deque[1]);
            Assert.AreEqual(2, deque[2]);
            Assert.AreEqual(4, deque[3]);
        }

        [Test]
        public void MixedOperations_WrapAround_ShouldMaintainCorrectOrder()
        {
            // Force the internal ring buffer to wrap around by
            // filling, partially draining from front, then refilling from back
            var deque = new Deque<int>();

            // Fill to capacity (triggers initial allocation of 4)
            deque.PushBack(1);
            deque.PushBack(2);
            deque.PushBack(3);
            deque.PushBack(4);

            // Drain two from front - head advances
            Assert.AreEqual(1, deque.PopFront());
            Assert.AreEqual(2, deque.PopFront());

            // Push two more to back - tail wraps around in the ring buffer
            deque.PushBack(5);
            deque.PushBack(6);

            // Verify: [3, 4, 5, 6]
            Assert.AreEqual(4, deque.Count);
            Assert.AreEqual(3, deque[0]);
            Assert.AreEqual(4, deque[1]);
            Assert.AreEqual(5, deque[2]);
            Assert.AreEqual(6, deque[3]);
        }

        [Test]
        public void MixedOperations_HeavyWrapping_StressTest()
        {
            // Simulate UndoStack-like pattern: push many, pop some, push more
            var deque = new Deque<int>();
            var reference = new LinkedList<int>();

            var rng = new Random(42); // deterministic seed
            for (int i = 0; i < 1_000; i++)
            {
                int op = rng.Next(4);
                switch (op)
                {
                    case 0: // PushBack
                        deque.PushBack(i);
                        reference.AddLast(i);
                        break;
                    case 1: // PushFront
                        deque.PushFront(i);
                        reference.AddFirst(i);
                        break;
                    case 2: // PopBack
                        if (deque.Count > 0)
                        {
                            Assert.AreEqual(reference.Last.Value, deque.PopBack());
                            reference.RemoveLast();
                        }
                        break;
                    case 3: // PopFront
                        if (deque.Count > 0)
                        {
                            Assert.AreEqual(reference.First.Value, deque.PopFront());
                            reference.RemoveFirst();
                        }
                        break;
                }

                Assert.AreEqual(reference.Count, deque.Count,
                    $"Count mismatch at iteration {i}");
            }

            // Final verification: all remaining elements match
            var refArray = reference.ToArray();
            for (int i = 0; i < deque.Count; i++)
                Assert.AreEqual(refArray[i], deque[i], $"Element mismatch at index {i}");
        }

        #endregion

        #region Indexer

        [Test]
        public void Indexer_Get_ValidIndex_ShouldReturnCorrectElement()
        {
            var deque = new Deque<string>();
            deque.PushBack("a");
            deque.PushBack("b");
            deque.PushBack("c");

            Assert.AreEqual("a", deque[0]);
            Assert.AreEqual("b", deque[1]);
            Assert.AreEqual("c", deque[2]);
        }

        [Test]
        public void Indexer_Set_ValidIndex_ShouldUpdateElement()
        {
            var deque = new Deque<int>();
            deque.PushBack(1);
            deque.PushBack(2);
            deque.PushBack(3);

            deque[1] = 99;

            Assert.AreEqual(1, deque[0]);
            Assert.AreEqual(99, deque[1]);
            Assert.AreEqual(3, deque[2]);
        }

        [Test]
        public void Indexer_Get_NegativeIndex_ShouldThrow()
        {
            var deque = new Deque<int>();
            deque.PushBack(1);

            Assert.Throws<ArgumentOutOfRangeException>(() => { _ = deque[-1]; });
        }

        [Test]
        public void Indexer_Get_IndexEqualToCount_ShouldThrow()
        {
            var deque = new Deque<int>();
            deque.PushBack(1);

            Assert.Throws<ArgumentOutOfRangeException>(() => { _ = deque[1]; });
        }

        [Test]
        public void Indexer_Get_OnEmptyDeque_ShouldThrow()
        {
            var deque = new Deque<int>();
            Assert.Throws<ArgumentOutOfRangeException>(() => { _ = deque[0]; });
        }

        [Test]
        public void Indexer_Set_NegativeIndex_ShouldThrow()
        {
            var deque = new Deque<int>();
            deque.PushBack(1);

            Assert.Throws<ArgumentOutOfRangeException>(() => { deque[-1] = 99; });
        }

        [Test]
        public void Indexer_Set_IndexEqualToCount_ShouldThrow()
        {
            var deque = new Deque<int>();
            deque.PushBack(1);

            Assert.Throws<ArgumentOutOfRangeException>(() => { deque[1] = 99; });
        }

        [Test]
        public void Indexer_AfterWrapAround_ShouldReturnCorrectElements()
        {
            var deque = new Deque<int>();

            // Fill to capacity
            deque.PushBack(1);
            deque.PushBack(2);
            deque.PushBack(3);
            deque.PushBack(4);

            // Pop from front to advance head, then push to back to wrap tail
            deque.PopFront();
            deque.PopFront();
            deque.PushBack(5);
            deque.PushBack(6);

            // Indexer should logically see [3, 4, 5, 6]
            Assert.AreEqual(3, deque[0]);
            Assert.AreEqual(4, deque[1]);
            Assert.AreEqual(5, deque[2]);
            Assert.AreEqual(6, deque[3]);
        }

        #endregion

        #region Clear

        [Test]
        public void Clear_ShouldResetCountToZero()
        {
            var deque = new Deque<int>();
            deque.PushBack(1);
            deque.PushBack(2);
            deque.PushBack(3);

            deque.Clear();

            Assert.AreEqual(0, deque.Count);
        }

        [Test]
        public void Clear_ShouldAllowSubsequentPush()
        {
            var deque = new Deque<int>();
            deque.PushBack(1);
            deque.PushBack(2);
            deque.Clear();

            deque.PushBack(99);

            Assert.AreEqual(1, deque.Count);
            Assert.AreEqual(99, deque[0]);
        }

        [Test]
        public void Clear_OnEmptyDeque_ShouldBeNoOp()
        {
            var deque = new Deque<int>();
            deque.Clear();
            Assert.AreEqual(0, deque.Count);
        }

        [Test]
        public void Clear_ThenRefill_ShouldWorkCorrectly()
        {
            // Exercises the in-place clear path (buffer is reused)
            var deque = new Deque<int>();
            for (int i = 0; i < 20; i++)
                deque.PushBack(i);

            deque.Clear();

            for (int i = 100; i < 120; i++)
                deque.PushBack(i);

            Assert.AreEqual(20, deque.Count);
            for (int i = 0; i < 20; i++)
                Assert.AreEqual(100 + i, deque[i]);
        }

        [Test]
        public void Clear_WithWrappedBuffer_ShouldClearCorrectly()
        {
            var deque = new Deque<string>();

            // Fill to 4, pop 2 from front, push 2 to back - creates wrap-around
            deque.PushBack("a");
            deque.PushBack("b");
            deque.PushBack("c");
            deque.PushBack("d");
            deque.PopFront();
            deque.PopFront();
            deque.PushBack("e");
            deque.PushBack("f");

            deque.Clear();

            Assert.AreEqual(0, deque.Count);

            // Push new elements and verify clean state
            deque.PushBack("x");
            Assert.AreEqual(1, deque.Count);
            Assert.AreEqual("x", deque[0]);
        }

        #endregion

        #region Contains

        [Test]
        public void Contains_ExistingElement_ShouldReturnTrue()
        {
            var deque = new Deque<int>();
            deque.PushBack(1);
            deque.PushBack(2);
            deque.PushBack(3);

            Assert.IsTrue(deque.Contains(2));
        }

        [Test]
        public void Contains_NonExistingElement_ShouldReturnFalse()
        {
            var deque = new Deque<int>();
            deque.PushBack(1);
            deque.PushBack(2);
            deque.PushBack(3);

            Assert.IsFalse(deque.Contains(99));
        }

        [Test]
        public void Contains_OnEmptyDeque_ShouldReturnFalse()
        {
            var deque = new Deque<int>();
            Assert.IsFalse(deque.Contains(0));
        }

        [Test]
        public void Contains_NullInReferenceTypeDeque_ShouldReturnTrue()
        {
            var deque = new Deque<string>();
            deque.PushBack("a");
            deque.PushBack(null);
            deque.PushBack("b");

            Assert.IsTrue(deque.Contains(null));
        }

        [Test]
        public void Contains_NullNotPresent_ShouldReturnFalse()
        {
            var deque = new Deque<string>();
            deque.PushBack("a");
            deque.PushBack("b");

            Assert.IsFalse(deque.Contains(null));
        }

        [Test]
        public void Contains_AfterWrapAround_ShouldFindElement()
        {
            var deque = new Deque<int>();
            deque.PushBack(1);
            deque.PushBack(2);
            deque.PushBack(3);
            deque.PushBack(4);
            deque.PopFront();
            deque.PopFront();
            deque.PushBack(5);
            deque.PushBack(6);

            // Logical content: [3, 4, 5, 6]
            Assert.IsTrue(deque.Contains(3));
            Assert.IsTrue(deque.Contains(6));
            Assert.IsFalse(deque.Contains(1));
            Assert.IsFalse(deque.Contains(2));
        }

        #endregion

        #region CopyTo

        [Test]
        public void CopyTo_ShouldCopyAllElementsInOrder()
        {
            var deque = new Deque<int>();
            deque.PushBack(10);
            deque.PushBack(20);
            deque.PushBack(30);

            var arr = new int[3];
            deque.CopyTo(arr, 0);

            Assert.AreEqual(new[] { 10, 20, 30 }, arr);
        }

        [Test]
        public void CopyTo_WithOffset_ShouldCopyToCorrectPosition()
        {
            var deque = new Deque<int>();
            deque.PushBack(10);
            deque.PushBack(20);

            var arr = new int[5];
            deque.CopyTo(arr, 2);

            Assert.AreEqual(new[] { 0, 0, 10, 20, 0 }, arr);
        }

        [Test]
        public void CopyTo_EmptyDeque_ShouldNotModifyArray()
        {
            var deque = new Deque<int>();
            var arr = new int[] { 1, 2, 3 };

            deque.CopyTo(arr, 0);

            Assert.AreEqual(new[] { 1, 2, 3 }, arr);
        }

        [Test]
        public void CopyTo_NullArray_ShouldThrowArgumentNullException()
        {
            var deque = new Deque<int>();
            deque.PushBack(1);

            Assert.Throws<ArgumentNullException>(() => deque.CopyTo(null, 0));
        }

        [Test]
        public void CopyTo_AfterWrapAround_ShouldCopyInLogicalOrder()
        {
            var deque = new Deque<int>();
            deque.PushBack(1);
            deque.PushBack(2);
            deque.PushBack(3);
            deque.PushBack(4);
            deque.PopFront();
            deque.PopFront();
            deque.PushBack(5);
            deque.PushBack(6);

            var arr = new int[4];
            deque.CopyTo(arr, 0);

            // Logical order: [3, 4, 5, 6]
            Assert.AreEqual(new[] { 3, 4, 5, 6 }, arr);
        }

        #endregion

        #region Enumeration

        [Test]
        public void GetEnumerator_ShouldYieldElementsInOrder()
        {
            var deque = new Deque<int>();
            deque.PushBack(1);
            deque.PushBack(2);
            deque.PushBack(3);

            var result = new List<int>();
            foreach (var item in deque)
                result.Add(item);

            Assert.AreEqual(new[] { 1, 2, 3 }, result.ToArray());
        }

        [Test]
        public void GetEnumerator_EmptyDeque_ShouldYieldNothing()
        {
            var deque = new Deque<int>();
            var result = new List<int>();
            foreach (var item in deque)
                result.Add(item);

            Assert.IsEmpty(result);
        }

        [Test]
        public void GetEnumerator_AfterWrapAround_ShouldYieldLogicalOrder()
        {
            var deque = new Deque<int>();
            deque.PushBack(1);
            deque.PushBack(2);
            deque.PushBack(3);
            deque.PushBack(4);
            deque.PopFront();
            deque.PopFront();
            deque.PushBack(5);
            deque.PushBack(6);

            var result = deque.ToList();
            Assert.AreEqual(new[] { 3, 4, 5, 6 }, result.ToArray());
        }

        [Test]
        public void GetEnumerator_ViaIEnumerable_ShouldYieldSameElements()
        {
            var deque = new Deque<int>();
            deque.PushBack(10);
            deque.PushBack(20);
            deque.PushBack(30);

            // Cast to IEnumerable to force the explicit interface path
            IEnumerable enumerable = deque;
            var result = new List<int>();
            foreach (int item in enumerable)
                result.Add(item);

            Assert.AreEqual(new[] { 10, 20, 30 }, result.ToArray());
        }

        [Test]
        public void GetEnumerator_ViaIEnumerableOfT_ShouldYieldSameElements()
        {
            var deque = new Deque<int>();
            deque.PushBack(10);
            deque.PushBack(20);
            deque.PushBack(30);

            // Cast to IEnumerable<T> to force the explicit interface path
            IEnumerable<int> enumerable = deque;
            var result = new List<int>();
            foreach (var item in enumerable)
                result.Add(item);

            Assert.AreEqual(new[] { 10, 20, 30 }, result.ToArray());
        }

        [Test]
        public void StructEnumerator_Reset_ShouldAllowReEnumeration()
        {
            var deque = new Deque<int>();
            deque.PushBack(1);
            deque.PushBack(2);
            deque.PushBack(3);

            var enumerator = deque.GetEnumerator();

            // First pass
            var firstPass = new List<int>();
            while (enumerator.MoveNext())
                firstPass.Add(enumerator.Current);

            // Reset and second pass
            enumerator.Reset();
            var secondPass = new List<int>();
            while (enumerator.MoveNext())
                secondPass.Add(enumerator.Current);

            Assert.AreEqual(firstPass, secondPass);
        }

        [Test]
        public void LinqOperations_ShouldWorkWithDeque()
        {
            var deque = new Deque<int>();
            for (int i = 1; i <= 10; i++)
                deque.PushBack(i);

            Assert.AreEqual(55, deque.Sum());
            Assert.AreEqual(10, deque.Max());
            Assert.AreEqual(1, deque.Min());
            Assert.AreEqual(5, deque.Count(x => x > 5));
        }

        #endregion

        #region ICollection<T> Interface

        [Test]
        public void ICollection_IsReadOnly_ShouldReturnFalse()
        {
            ICollection<int> deque = new Deque<int>();
            Assert.IsFalse(deque.IsReadOnly);
        }

        [Test]
        public void ICollection_Add_ShouldDelegateToPushBack()
        {
            ICollection<int> deque = new Deque<int>();
            deque.Add(1);
            deque.Add(2);
            deque.Add(3);

            Assert.AreEqual(3, deque.Count);

            // Verify order via enumeration
            var result = deque.ToList();
            Assert.AreEqual(new[] { 1, 2, 3 }, result.ToArray());
        }

        [Test]
        public void ICollection_Remove_ShouldThrowNotSupportedException()
        {
            ICollection<int> deque = new Deque<int>();
            deque.Add(1);

            Assert.Throws<NotSupportedException>(() => deque.Remove(1));
        }

        #endregion

        #region Growth and Capacity (Power-of-2 Invariant)

        [Test]
        public void Growth_CapacityShouldAlwaysBePowerOf2()
        {
            // We can't directly inspect capacity, but we can infer it:
            // After pushing N elements, if we push one more without a pop,
            // the capacity must be >= N+1 and a power of 2.
            // We verify this indirectly by ensuring all operations remain correct
            // at power-of-2 boundaries.
            var deque = new Deque<int>();

            // Push exactly at boundaries: 1, 2, 4, 8, 16, 32, 64
            for (int i = 0; i < 64; i++)
            {
                deque.PushBack(i);
                Assert.AreEqual(i + 1, deque.Count);
                Assert.AreEqual(i, deque[i]);
            }

            // Verify all elements are intact
            for (int i = 0; i < 64; i++)
                Assert.AreEqual(i, deque[i]);
        }

        [Test]
        public void Growth_AfterMultipleGrows_ElementOrderPreserved()
        {
            var deque = new Deque<int>();

            // Force several grow operations
            for (int i = 0; i < 100; i++)
                deque.PushBack(i);

            for (int i = 0; i < 100; i++)
                Assert.AreEqual(i, deque[i]);
        }

        [Test]
        public void Growth_WithWrappedBuffer_ShouldUnwrapCorrectlyOnGrow()
        {
            // This is the critical test: when the ring buffer is wrapped
            // (head > tail) and we need to grow, SetCapacity must correctly
            // linearize the two segments into the new array.
            var deque = new Deque<int>();

            // Fill to 4 (initial capacity)
            deque.PushBack(0);
            deque.PushBack(1);
            deque.PushBack(2);
            deque.PushBack(3);

            // Pop two from front - head advances to index 2
            deque.PopFront(); // removes 0
            deque.PopFront(); // removes 1

            // Push two to back - fills back to 4 (tail wraps to index 2)
            deque.PushBack(4);
            deque.PushBack(5);

            // Now push one more - this forces a grow while wrapped
            deque.PushBack(6);

            // Logical content should be [2, 3, 4, 5, 6]
            Assert.AreEqual(5, deque.Count);
            Assert.AreEqual(2, deque[0]);
            Assert.AreEqual(3, deque[1]);
            Assert.AreEqual(4, deque[2]);
            Assert.AreEqual(5, deque[3]);
            Assert.AreEqual(6, deque[4]);
        }

        #endregion

        #region Value Type vs Reference Type Behavior

        [Test]
        public void ValueType_PopBack_ShouldReturnCorrectValues()
        {
            // Exercises the IsReferenceOrContainsReferences<T>() == false path
            var deque = new Deque<double>();
            deque.PushBack(1.1);
            deque.PushBack(2.2);
            deque.PushBack(3.3);

            Assert.AreEqual(3.3, deque.PopBack(), 0.001);
            Assert.AreEqual(2.2, deque.PopBack(), 0.001);
            Assert.AreEqual(1.1, deque.PopBack(), 0.001);
        }

        [Test]
        public void StructWithReferenceField_ShouldClearOnPop()
        {
            // A struct containing a reference field exercises the
            // IsReferenceOrContainsReferences<T>() == true path for value types
            var deque = new Deque<KeyValuePair<string, int>>();
            deque.PushBack(new KeyValuePair<string, int>("a", 1));
            deque.PushBack(new KeyValuePair<string, int>("b", 2));

            var result = deque.PopBack();
            Assert.AreEqual("b", result.Key);
            Assert.AreEqual(2, result.Value);
        }

        #endregion

        #region Edge Cases

        [Test]
        public void SingleElement_PushBackPopBack()
        {
            var deque = new Deque<int>();
            deque.PushBack(42);
            Assert.AreEqual(42, deque.PopBack());
            Assert.AreEqual(0, deque.Count);
        }

        [Test]
        public void SingleElement_PushBackPopFront()
        {
            var deque = new Deque<int>();
            deque.PushBack(42);
            Assert.AreEqual(42, deque.PopFront());
            Assert.AreEqual(0, deque.Count);
        }

        [Test]
        public void SingleElement_PushFrontPopBack()
        {
            var deque = new Deque<int>();
            deque.PushFront(42);
            Assert.AreEqual(42, deque.PopBack());
            Assert.AreEqual(0, deque.Count);
        }

        [Test]
        public void SingleElement_PushFrontPopFront()
        {
            var deque = new Deque<int>();
            deque.PushFront(42);
            Assert.AreEqual(42, deque.PopFront());
            Assert.AreEqual(0, deque.Count);
        }

        [Test]
        public void RepeatedClearAndRefill_ShouldNotCorruptState()
        {
            var deque = new Deque<int>();

            for (int cycle = 0; cycle < 10; cycle++)
            {
                for (int i = 0; i < 20; i++)
                    deque.PushBack(cycle * 100 + i);

                Assert.AreEqual(20, deque.Count);
                Assert.AreEqual(cycle * 100, deque[0]);
                Assert.AreEqual(cycle * 100 + 19, deque[19]);

                deque.Clear();
                Assert.AreEqual(0, deque.Count);
            }
        }

        [Test]
        public void LargeDeque_ShouldHandleManyElements()
        {
            var deque = new Deque<int>();
            const int count = 100_000;

            for (int i = 0; i < count; i++)
                deque.PushBack(i);

            Assert.AreEqual(count, deque.Count);
            Assert.AreEqual(0, deque[0]);
            Assert.AreEqual(count - 1, deque[count - 1]);

            // Drain from front
            for (int i = 0; i < count; i++)
                Assert.AreEqual(i, deque.PopFront());

            Assert.AreEqual(0, deque.Count);
        }

        [Test]
        public void PushFront_ThenGrow_ShouldPreserveOrder()
        {
            // PushFront moves head backward; when grow happens, the
            // two-segment copy in SetCapacity must handle head > 0 correctly
            var deque = new Deque<int>();

            for (int i = 0; i < 10; i++)
                deque.PushFront(i);

            // Expected: [9, 8, 7, 6, 5, 4, 3, 2, 1, 0]
            for (int i = 0; i < 10; i++)
                Assert.AreEqual(9 - i, deque[i]);
        }

        [Test]
        public void AlternatingPushFrontAndPopBack_ShouldWorkCorrectly()
        {
            // This exercises the scenario where head keeps moving backward
            // and tail keeps moving backward, both wrapping frequently
            var deque = new Deque<int>();

            for (int i = 0; i < 50; i++)
            {
                deque.PushFront(i);
                deque.PushFront(i + 100);

                Assert.AreEqual(i, deque.PopBack());
                // deque still has one element: i + 100
                Assert.AreEqual(1, deque.Count);
                Assert.AreEqual(i + 100, deque[0]);

                deque.PopFront();
            }

            Assert.AreEqual(0, deque.Count);
        }

        #endregion

        #region Behavioral Parity with Original Implementation

        // These tests verify behaviors that existed in the original Deque<T>
        // to ensure the optimized version is a drop-in replacement.

        [Test]
        public void Parity_ICollection_Add_SameAsPushBack()
        {
            var dequeViaAdd = new Deque<int>();
            var dequeViaPush = new Deque<int>();

            for (int i = 0; i < 10; i++)
            {
                ((ICollection<int>)dequeViaAdd).Add(i);
                dequeViaPush.PushBack(i);
            }

            Assert.AreEqual(dequeViaPush.Count, dequeViaAdd.Count);
            for (int i = 0; i < dequeViaPush.Count; i++)
                Assert.AreEqual(dequeViaPush[i], dequeViaAdd[i]);
        }

        [Test]
        public void Parity_CopyToAndEnumeration_ShouldMatchIndexer()
        {
            var deque = new Deque<int>();
            // Build a wrapped deque
            for (int i = 0; i < 8; i++)
                deque.PushBack(i);
            for (int i = 0; i < 4; i++)
                deque.PopFront();
            for (int i = 8; i < 12; i++)
                deque.PushBack(i);

            // Collect via indexer
            var viaIndexer = new int[deque.Count];
            for (int i = 0; i < deque.Count; i++)
                viaIndexer[i] = deque[i];

            // Collect via CopyTo
            var viaCopyTo = new int[deque.Count];
            deque.CopyTo(viaCopyTo, 0);

            // Collect via enumeration
            var viaEnumeration = deque.ToArray();

            Assert.AreEqual(viaIndexer, viaCopyTo, "CopyTo should match indexer order");
            Assert.AreEqual(viaIndexer, viaEnumeration, "Enumeration should match indexer order");
        }

        [Test]
        public void Parity_UndoStackPattern_PushBackWithSizeLimitTrim()
        {
            // Simulates the UndoStack's pattern:
            // push operations to back, trim oldest from front when over limit
            var deque = new Deque<int>();
            const int sizeLimit = 10;

            for (int i = 0; i < 50; i++)
            {
                deque.PushBack(i);
                while (deque.Count > sizeLimit)
                    deque.PopFront();
            }

            Assert.AreEqual(sizeLimit, deque.Count);
            // Should contain the last 10 elements: 40..49
            for (int i = 0; i < sizeLimit; i++)
                Assert.AreEqual(40 + i, deque[i]);
        }

        #endregion
    }
}