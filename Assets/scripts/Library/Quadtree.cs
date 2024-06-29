using System.Collections.Generic;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Unity.VisualScripting;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine;

[Serializable]
public sealed class Quadtree
{
    private readonly List<ISpatialEntity2d> _elements = new List<ISpatialEntity2d>();

    private readonly int _bucketCapacity;
    private readonly int _maxDepth;

    public RectangleF bounds
    { get; }

    private Quadtree _topLeft, _topRight, _bottomLeft, _bottomRight;

    public Quadtree(RectangleF bounds)
        : this(bounds, 32, 5)
    { }

    public Quadtree(RectangleF bounds, int bucketCapacity, int maxDepth)
    {
        _bucketCapacity = bucketCapacity;
        _maxDepth = maxDepth;

        this.bounds = bounds;
    }

    public int Level
    { get; private set; }

    public List<Quadtree> GetChildren()
    {
        List<Quadtree> children = new List<Quadtree>();
        if (_topLeft != null)
            children.Add(_topLeft);
        if(_topRight != null)
            children.Add(_topRight);
        if(_bottomLeft != null)
            children.Add(_bottomLeft);
        if(_bottomRight != null)
            children.Add(_bottomRight);
        return children;
    }

    public bool IsLeaf
        => _topLeft == null || _topRight == null || _bottomLeft == null || _bottomRight == null;

    public void Insert(ISpatialEntity2d element)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (!bounds.Contains(element.bounds))
            throw new ArgumentException("ElementOutsideQuadtreeBounds", nameof(element));

        // A node exceeding its allotted number of items will get split (if it hasn't been already) into four equal quadrants.
        if (_elements.Count >= _bucketCapacity)
            Split();

        Quadtree containingChild = GetContainingChild(element.bounds);

        if (containingChild != null)
        {
            containingChild.Insert(element);
        }
        else
        {   // If no child was returned, then this is either a leaf node, or the element's boundaries overlap multiple children.
            // Either way, the element gets assigned to this node.
            _elements.Add(element);
        }
    }

    /// <summary>
    /// Removes the specified spatial element from the quadtree and whichever node it's been assigned to.
    /// </summary>
    /// <param name="element">The spatial element to remove from the quadtree.</param>
    /// <returns>True if <c>element</c> is successfully removed; otherwise, false.</returns>
    public bool Remove(global::ISpatialEntity2d element)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        Quadtree containingChild = GetContainingChild(element.bounds);

        // If no child was returned, then this is the leaf node (or potentially non-leaf node, if the element's boundaries overlap
        // multiple children) containing the element.
        bool removed = containingChild?.Remove(element) ?? _elements.Remove(element);

        // If the total descendant element count is less than the bucket capacity, we ensure the node is in a non-split state.
        if (removed && CountElements() <= _bucketCapacity)
            Merge();

        return removed;
    }

    /// <summary>
    /// Looks for and returns all spatial elements that exist within this node and its children whose bounds collide with
    /// the specified spatial element.
    /// </summary>
    /// <param name="element">The spatial element to find collisions for.</param>
    /// <returns>All spatial elements that collide with <c>element</c>.</returns>
    public List<ISpatialEntity2d> FindCollisions(ISpatialEntity2d element)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        var nodes = new Queue<Quadtree>();
        var collisions = new List<ISpatialEntity2d>();

        nodes.Enqueue(this);

        while (nodes.Count > 0)
        {
            var node = nodes.Dequeue();

            if (!element.bounds.IntersectsWith(node.bounds))
                continue;

            collisions.AddRange(node._elements.FindAll(e => e.bounds.IntersectsWith(element.bounds)));

            if (!node.IsLeaf)
            {
                if (element.bounds.IntersectsWith(node._topLeft.bounds))
                    nodes.Enqueue(node._topLeft);

                if (element.bounds.IntersectsWith(node._topRight.bounds))
                    nodes.Enqueue(node._topRight);

                if (element.bounds.IntersectsWith(node._bottomLeft.bounds))
                    nodes.Enqueue(node._bottomLeft);

                if (element.bounds.IntersectsWith(node._bottomRight.bounds))
                    nodes.Enqueue(node._bottomRight);
            }
        }

        return collisions;
    }

    /// <summary>
    /// Gets the total number of elements belonging to this and all descending nodes.
    /// </summary>
    /// <returns>The total number of elements belong to this and all descending nodes.</returns>
    public int CountElements()
    {
        int count = _elements.Count;

        if (!IsLeaf)
        {
            count += _topLeft.CountElements();
            count += _topRight.CountElements();
            count += _bottomLeft.CountElements();
            count += _bottomRight.CountElements();
        }

        return count;
    }

    /// <summary>
    /// Retrieves the elements belonging to this and all descendant nodes.
    /// </summary>
    /// <returns>A sequence of the elements belonging to this and all descendant nodes.</returns>
    public List<ISpatialEntity2d> GetElements()
    {
        var children = new List<ISpatialEntity2d>();
        var nodes = new Queue<Quadtree>();

        nodes.Enqueue(this);

        while (nodes.Count > 0)
        {
            var node = nodes.Dequeue();

            if (!node.IsLeaf)
            {
                nodes.Enqueue(node._topLeft);
                nodes.Enqueue(node._topRight);
                nodes.Enqueue(node._bottomLeft);
                nodes.Enqueue(node._bottomRight);
            }

            children.AddRange(node._elements);
        }

        return children;
    }

    private void Split()
    {   // If we're not a leaf node, then we're already split.
        if (!IsLeaf)
            return;

        // Splitting is only allowed if it doesn't cause us to exceed our maximum depth.
        if (Level + 1 > _maxDepth)
            return;

        PointF center = bounds.Center();
        _topLeft = CreateChild(bounds.Location);
        _topRight = CreateChild(new PointF(center.X, bounds.Location.Y));
        _bottomLeft = CreateChild(new PointF(bounds.Location.X, center.Y));
        _bottomRight = CreateChild(new PointF(center.X, center.Y));

        var elements = new List<ISpatialEntity2d>(_elements);

        foreach (var element in elements)
        {
            Quadtree containingChild = GetContainingChild(element.bounds);
            // An element is only moved if it completely fits into a child quadrant.
            if (containingChild != null)
            {
                _elements.Remove(element);

                containingChild.Insert(element);
            }
        }
    }

    private Quadtree CreateChild(PointF location)
        => new(new RectangleF(location, bounds.Size / 2), _bucketCapacity, _maxDepth)
        {
            Level = Level + 1
        };

    private void Merge()
    {   // If we're a leaf node, then there is nothing to merge.
        if (IsLeaf)
            return;

        _elements.AddRange(_topLeft._elements);
        _elements.AddRange(_topRight._elements);
        _elements.AddRange(_bottomLeft._elements);
        _elements.AddRange(_bottomRight._elements);

        _topLeft = _topRight = _bottomLeft = _bottomRight = null;
    }

    private Quadtree GetContainingChild(RectangleF bounds)
    {
        if (IsLeaf)
            return null;

        if (_topLeft.bounds.Contains(bounds))
            return _topLeft;

        if (_topRight.bounds.Contains(bounds))
            return _topRight;

        if (_bottomLeft.bounds.Contains(bounds))
            return _bottomLeft;

        return _bottomRight.bounds.Contains(bounds) ? _bottomRight : null;
    }
}
