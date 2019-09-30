using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Utility
{
    /// <summary>
    /// Simple but efficient heap based generic priority queue
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PriorityQueue<T>
    {
        /// <summary>
        /// Key that contains value and priority
        /// </summary>
        struct Key
        {
            public T value;
            public float priority;

            public Key(T val, float prior)
            {
                value = val;
                priority = prior;
            }
        }

        /// <summary>
        /// List of elements in the queue
        /// </summary>
        private List<Key> itemList;

        /// <summary>
        /// Is the queue empty or not?
        /// </summary>
        public bool Empty { get { return itemList.Count <= 0; } }

        public PriorityQueue(int size = 0)
        {
            itemList = new List<Key>(size);
        }

        public PriorityQueue(PriorityQueue<T> baseQueue)
        {
            itemList = new List<Key>(baseQueue.itemList);
        }


        /// <summary>
        /// Get the number of elements stored in the queue
        /// </summary>
        /// <returns>number of elements</returns>
        public int Count()
        {
            return itemList.Count;
        }

        /// <summary>
        /// Clear queue from all elements
        /// </summary>
        public void Clear()
        {
            itemList.Clear();
        }


        /// <summary>
        /// Swaps the elements stored in the indices i and j
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        private void Swap(int i, int j)
        {
            Key tmp = itemList[i];
            itemList[i] = itemList[j];
            itemList[j] = tmp;
        }

        /// <summary>
        /// Get the index of the parent of the node in index i
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private static int GetParent(int i)
        {
            return (i + 1) / 2 - 1;
        }

        /// <summary>
        /// Get the index of the left child of the node in index i
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        protected static int GetLeftChild(int i)
        {
            return i * 2 + 1;
        }

        /// <summary>
        /// Get the index of the right child of the node in index i
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private static int GetRightChild(int i)
        {
            return i * 2 + 2;
        }

        /// <summary>
        /// Get the element with lowest priority and remove it
        /// </summary>
        /// <returns></returns>
        public T Pop()
        {
            if (!Empty)
            {
                Key oldRoot = itemList[0];
                itemList[0] = itemList[itemList.Count - 1];
                itemList.RemoveAt(itemList.Count - 1);
                PercolateDown(0);
                return oldRoot.value;
            }
            throw new System.Exception("Trying to pop from empty queue");
        }

        /// <summary>
        /// Get the element with lowest priority without removing it
        /// </summary>
        /// <returns></returns>
        public T Peek()
        {
            if (!Empty)
            {
                return itemList[0].value;
            }
            throw new System.Exception("Trying to peek empty queue");
        }


        /// <summary>
        /// Get the lowest priority value in the queue
        /// </summary>
        /// <returns>Returns float.MaxValue if .</returns>
        public float PeekPriority()
        {
            if (!Empty)
            {
                return itemList[0].priority;
            }
            throw new System.Exception("Trying to peek empty queue");
        }

        /// <summary>
        /// Takes element in given index and moves it down in the heap until the element 
        /// below is greater or it hits the bottom returns position of element
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        private int PercolateDown(int idx)
        {
            int leftChild = GetLeftChild(idx);
            int rightChild = GetRightChild(idx);
            while ((leftChild <= itemList.Count - 1 && itemList[leftChild].priority < itemList[idx].priority) || (rightChild <= itemList.Count - 1 && itemList[rightChild].priority < itemList[idx].priority))
            {
                if (rightChild > itemList.Count - 1 || itemList[leftChild].priority < itemList[rightChild].priority)
                {
                    Swap(leftChild, idx);
                    idx = leftChild;
                }
                else
                {
                    Swap(rightChild, idx);
                    idx = rightChild;
                }
                leftChild = GetLeftChild(idx);
                rightChild = GetRightChild(idx);

            }
            return idx;
        }

        /// <summary>
        /// Add new element with given priority.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="priority">Priority in the queue</param>
        public void Add(T value, float priority)
        {
            itemList.Add(new Key(value, priority));

            PercolateUp(itemList.Count - 1);
        }

        /// <summary>
        /// Moves the element in index idx up until the element above is
        /// smaller or it hits the root
        /// </summary>
        /// <param name="idx"></param>
        /// <returns>final position of element</returns>
        private int PercolateUp(int idx)
        {
            int parent = GetParent(idx);
            while (parent >= 0 && itemList[parent].priority > itemList[idx].priority)
            {
                Swap(parent, idx);
                idx = parent;
                parent = GetParent(idx);
            }
            return idx;
        }

        private int Height()
        {
            int h = 0;
            for (int i = itemList.Count - 1; i > 0; i /= 2)
                ++h;
            return h;
        }

        /// <summary>
        /// Formatted list of elements as a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string str = "[";
            foreach (var item in itemList)
            {
                str += "(" + item.value.ToString() + ", " + item.priority + "),";
            }
            str += "]";

            return str;
        }
    }
}