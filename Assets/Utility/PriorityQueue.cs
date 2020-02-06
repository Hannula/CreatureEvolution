using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Pathfinding.Utility
{
    /// <summary>
    /// Simple but efficient heap based generic priority queue
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PriorityQueue<T>
    {
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

        private List<Key> itemList;

        public bool Empty { get { return itemList.Count <= 0; } }
        
        public PriorityQueue()
        {
            itemList = new List<Key>();
        }

        /*
          public int elements ()      
          return the number of elements in stored in the heap
         */
        public int Count()
        {
            return itemList.Count;
        }

        public void Clear()
        {
            itemList.Clear();
        }


        /*
          private void swap (int i, int j)
          swaps the elements stored in the indices i and j
        */
        private void Swap(int i, int j)
        {
            Key tmp = itemList[i];
            itemList[i] = itemList[j];
            itemList[j] = tmp;
        }

        /*
          private static int parent (int i)
          returns the index of the parent of the node in index i
        */
        private static int GetParent(int i)
        {
            return (i + 1 ) / 2 - 1;
        }

        /*
          private static int leftChild (int i)
          returns the index of the left child of the node in index i
        */
        protected static int GetLeftChild(int i)
        {
            return i * 2 + 1;
        }
        /*
          private static int rightChild (int i)
          returns the index of the right child of the node in index i
        */
        private static int GetRightChild(int i)
        {
            return i * 2 + 2;
        }

        /// <summary>
        /// Get the minimum value.
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
            return default(T);
        }

        /// <summary>
        /// Get the minimum value without removing it
        /// </summary>
        /// <returns></returns>
        public T Peek()
        {
            if (!Empty)
            {
                return itemList[0].value;
            }
            return default(T);
        }


        /// <summary>
        /// Peeks the priority.
        /// </summary>
        /// <returns>The priority. -1 if the priority queue is empty.</returns>
        public float PeekPriority()
        {
            if (!Empty)
            {
                return itemList[0].priority;
            }
            return -1;
        }

        /*
          private int percolateDown (int idx)
          takes element in index and moves it down in the heap until the element 
          below is greater or it hits the bottom returns position of element
        */
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
        /// Add new value with given priority.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="priority">Priority in the queue</param>
        public void Add(T value, float priority)
        {
            itemList.Add(new Key(value, priority));
            
            PercolateUp(itemList.Count - 1);
        }

        /*
          private void percolateUp (int idx)
          Moves the element in index idx up until the element above is
          smaller or it hits the root. Returns final position of element.
        */
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
            for (int i = itemList.Count - 1 ; i > 0; i /= 2)
                ++h;
            return h;
        }

        public override string ToString()
        {
            string str = "[";
            foreach(var item in itemList)
            {
                str += "(" + item.value + ", " + item.priority+")," ;
            }
            str += "]";

            return str;
        }
    }
}