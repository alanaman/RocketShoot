using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CircularBuffer<T>
{
    private T[] buffer;
    private int head;
    private int tail;
    private int count;

    public CircularBuffer(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be greater than zero.", nameof(capacity));

        buffer = new T[capacity];
        head = 0;
        tail = 0;
        count = 0;
    }

    public int Capacity => buffer.Length;
    public int Count => count;

    public void Add(T item)
    {
        if (count == Capacity)
        {
            buffer[tail] = item;
            tail = (tail + 1) % Capacity;
            head = tail;
        }
        else
        {
            buffer[tail] = item;
            tail = (tail + 1) % Capacity;
            count++;
        }
    }

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= count)
                throw new IndexOutOfRangeException();

            int actualIndex = (head + index) % Capacity;
            return buffer[actualIndex];
        }
    }

    public List<T> GetBatch(int size)
    {

        int sampleSize = Math.Min(size, Count);

        int[] chosenIndices = Enumerable.Range(0, Count)
                                        .OrderBy(x => UnityEngine.Random.Range(int.MinValue, int.MaxValue))
                                        .Take(sampleSize)
                                        .ToArray();

        List<T> buffer = chosenIndices.Select(index => this[index]).ToList();

        return buffer;
    }
}
