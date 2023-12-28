using OpenTK.Mathematics;

namespace CodeFirst.Physics;

public interface IQuadTreeVisitor
{
	void VisitBranch(QuadTree qt, int node, int depth, Box2 bounds);
	void VisitLeaf(QuadTree qt, int node, int depth, Box2 bounds);
}

public class QuadTree
{
	private const int QUAD_TREE_CHILD_COUNT = 4;

	private const int ELEMENT_NODE_INDEX_NEXT = 0;
	private const int ELEMENT_NODE_INDEX_ELEMENT = 1;

	private const int ELEMENT_INDEX_LEFT = 0;
	private const int ELEMENT_INDEX_TOP = 1;
	private const int ELEMENT_INDEX_RIGHT = 2;
	private const int ELEMENT_INDEX_BOTTOM = 3;

	private const int ELEMENT_INDEX_ID = 4;

	private const int NODE_INDEX_FIRST_CHILD = 0;
	private const int NODE_INDEX_COUNT = 1;

	private const int NODE_DATA_INDEX_X = 0;
	private const int NODE_DATA_INDEX_Y = 1;
	private const int NODE_DATA_INDEX_WIDTH = 2;
	private const int NODE_DATA_INDEX_HEIGHT = 3;
	private const int NODE_DATA_INDEX_INDEX = 4;
	private const int NODE_DATA_INDEX_DEPTH = 5;
	private const int NODE_DATA_INDEX_NUM = 6;

	private readonly IntList elementNodes = new(2);
	private readonly IntList elements = new(5);
	private readonly IntList nodes = new(2);

	private readonly int rootX;
	private readonly int rootY;
	private readonly int rootWidth;
	private readonly int rootHeight;

	private readonly int maxElements;
	private readonly int maxDepth;

	private bool[] temp;
	private int tempSize;

	public QuadTree(int width, int height, int initialMaxElements, int initialMaxDepth)
	{
		maxElements = initialMaxElements;
		maxDepth = initialMaxDepth;

		// Insert the root node to the qt.
		nodes.Insert();
		nodes.Set(n: 0, NODE_INDEX_FIRST_CHILD, val: -1);
		nodes.Set(n: 0, NODE_INDEX_COUNT, val: 0);

		// Set the extents of the root node.
		rootX = width / 2;
		rootY = height / 2;
		rootWidth = rootX;
		rootHeight = rootY;

		temp = new bool[tempSize];
	}

	// Outputs a list of elements found in the specified rectangle.
	public int Insert(int id, float left, float top, float right, float bottom)
	{
		// Insert a new element.
		var newElement = elements.Insert();
		elements.Set(newElement, ELEMENT_INDEX_LEFT, Floor(left));
		elements.Set(newElement, ELEMENT_INDEX_TOP, Floor(top));
		elements.Set(newElement, ELEMENT_INDEX_RIGHT, Floor(right));
		elements.Set(newElement, ELEMENT_INDEX_BOTTOM, Floor(bottom));
		elements.Set(newElement, ELEMENT_INDEX_ID, id);

		// Insert the element to the appropriate leaf node(s).
		NodeInsert(index: 0, depth: 0, rootX, rootY, rootWidth, rootHeight, newElement);
		return newElement;
	}

	// Removes the specified element from the tree.
	public void Remove(int element)
	{
		// Find the leaves.
		var left = elements.Get(element, ELEMENT_INDEX_LEFT);
		var top = elements.Get(element, ELEMENT_INDEX_TOP);
		var right = elements.Get(element, ELEMENT_INDEX_RIGHT);
		var bottom = elements.Get(element, ELEMENT_INDEX_BOTTOM);
		var leaves = FindLeaves(node: 0, depth: 0, rootX, rootY, rootWidth, rootHeight, left, top, right, bottom);

		// For each leaf node, remove the element node.
		for (var j = 0; j < leaves.Size(); ++j)
		{
			var nodeDataIndex = leaves.Get(j, NODE_DATA_INDEX_INDEX);

			// Walk the list until we find the element node.
			var nodeIndex = nodes.Get(nodeDataIndex, NODE_INDEX_FIRST_CHILD);
			var previousIndex = -1;
			while (nodeIndex != -1 && elementNodes.Get(nodeIndex, ELEMENT_NODE_INDEX_ELEMENT) != element)
			{
				previousIndex = nodeIndex;
				nodeIndex = elementNodes.Get(nodeIndex, ELEMENT_NODE_INDEX_NEXT);
			}

			if (nodeIndex == -1)
			{
				continue;
			}

			// Remove the element node.
			var nextIndex = elementNodes.Get(nodeIndex, ELEMENT_NODE_INDEX_NEXT);
			if (previousIndex == -1)
			{
				nodes.Set(nodeDataIndex, NODE_INDEX_FIRST_CHILD, nextIndex);
			}
			else
			{
				elementNodes.Set(previousIndex, ELEMENT_NODE_INDEX_NEXT, nextIndex);
			}

			elementNodes.Erase(nodeIndex);

			// Decrement the leaf element count.
			nodes.Set(nodeDataIndex, NODE_INDEX_COUNT, nodes.Get(nodeDataIndex, NODE_INDEX_COUNT) - 1);
		}

		// Remove the element.
		elements.Erase(element);
	}

	// Cleans up the tree, removing empty leaves.
	public void CleanUp()
	{
		var toProcess = new IntList(1);

		// Only process the root if it's not a leaf.
		if (nodes.Get(n: 0, NODE_INDEX_COUNT) == -1)
		{
			// Push the root index to the stack.
			toProcess.Set(toProcess.PushBack(), field: 0, val: 0);
		}

		while (toProcess.Size() > 0)
		{
			// Pop a node from the stack.
			var node = toProcess.Get(toProcess.Size() - 1, field: 0);
			var firstChild = nodes.Get(node, NODE_INDEX_FIRST_CHILD);
			var emptyLeafCount = 0;
			toProcess.PopBack();

			// Loop through the children.
			for (var i = 0; i < QUAD_TREE_CHILD_COUNT; ++i)
			{
				var child = firstChild + i;

				// Increment empty leaf count if the child is an empty 
				// leaf. Otherwise if the child is a branch, add it to
				// the stack to be processed in the next iteration.
				if (nodes.Get(child, NODE_INDEX_COUNT) == 0)
				{
					emptyLeafCount += 1;
				}
				else if (nodes.Get(child, NODE_INDEX_COUNT) == -1)
				{
					// Push the child index to the stack.
					toProcess.Set(toProcess.PushBack(), field: 0, child);
				}
			}

			// If all the children were empty leaves, remove them and 
			// make this node the new empty leaf.
			if (emptyLeafCount != QUAD_TREE_CHILD_COUNT)
			{
				continue;
			}

			// Remove all 4 children in reverse order so that they 
			// can be reclaimed on subsequent insertions in proper
			// order.
			nodes.Erase(firstChild + 3);
			nodes.Erase(firstChild + 2);
			nodes.Erase(firstChild + 1);
			nodes.Erase(firstChild + 0);

			// Make this node the new empty leaf.
			nodes.Set(node, NODE_INDEX_FIRST_CHILD, val: -1);
			nodes.Set(node, NODE_INDEX_COUNT, val: 0);
		}
	}

	private readonly IntList tmpResult = new(1);

	// Returns a list of elements found in the specified rectangle excluding the
	// specified element to omit.
	public IntList Query(float xLeft, float yTop, float xRight, float yBottom, int omitElement = -1)
	{
		var result = tmpResult;
		result.Clear();

		// Find the leaves that intersect the specified query rectangle.
		var queryLeft = Floor(xLeft);
		var queryTop = Floor(yTop);
		var queryRight = Floor(xRight);
		var queryBottom = Floor(yBottom);
		var leaves = FindLeaves(node: 0, depth: 0, rootX, rootY, rootWidth, rootHeight, queryLeft, queryTop, queryRight,
			queryBottom);

		if (tempSize < elements.Size())
		{
			tempSize = elements.Size();
			temp = new bool[tempSize];
		}

		// For each leaf node, look for elements that intersect.
		for (var i = 0; i < leaves.Size(); i++)
		{
			var nodeDataIndex = leaves.Get(i, NODE_DATA_INDEX_INDEX);

			// Walk the list and add elements that intersect.
			var elementNodeIndex = nodes.Get(nodeDataIndex, NODE_INDEX_FIRST_CHILD);
			while (elementNodeIndex != -1)
			{
				var element = elementNodes.Get(elementNodeIndex, ELEMENT_NODE_INDEX_ELEMENT);
				var left = elements.Get(element, ELEMENT_INDEX_LEFT);
				var top = elements.Get(element, ELEMENT_INDEX_TOP);
				var right = elements.Get(element, ELEMENT_INDEX_RIGHT);
				var bottom = elements.Get(element, ELEMENT_INDEX_BOTTOM);
				if (!temp[element] && element != omitElement && Intersect(queryLeft, queryTop, queryRight, queryBottom,
					    left, top, right, bottom))
				{
					result.Set(result.PushBack(), field: 0, element);
					temp[element] = true;
				}

				elementNodeIndex = elementNodes.Get(elementNodeIndex, ELEMENT_NODE_INDEX_NEXT);
			}
		}

		// Unmark the elements that were inserted.
		for (var i = 0; i < result.Size(); i++)
		{
			temp[result.Get(i, field: 0)] = false;
		}

		return result;
	}

	// Traverses all the nodes in the tree, calling 'branch' for branch nodes and 'leaf' 
	// for leaf nodes.
	public void Traverse(IQuadTreeVisitor visitor)
	{
		var toProcess = new IntList(NODE_DATA_INDEX_NUM);
		PushNode(toProcess, nodeDataIndex: 0, nodeDataDepth: 0, rootX, rootY, rootWidth, rootHeight);

		while (toProcess.Size() > 0)
		{
			var backIndex = toProcess.Size() - 1;
			var nodeDataX = toProcess.Get(backIndex, NODE_DATA_INDEX_X);
			var nodeDataY = toProcess.Get(backIndex, NODE_DATA_INDEX_Y);
			var nodeDataWidth = toProcess.Get(backIndex, NODE_DATA_INDEX_WIDTH);
			var nodeDataHeight = toProcess.Get(backIndex, NODE_DATA_INDEX_HEIGHT);
			var nodeDataIndex = toProcess.Get(backIndex, NODE_DATA_INDEX_INDEX);
			var nodeDataDepth = toProcess.Get(backIndex, NODE_DATA_INDEX_DEPTH);
			var firstChild = nodes.Get(nodeDataIndex, NODE_INDEX_FIRST_CHILD);
			toProcess.PopBack();

			var bounds = Box2.FromSize(new Vector2(nodeDataX, nodeDataY), new Vector2(nodeDataWidth, nodeDataHeight));
			if (nodes.Get(nodeDataIndex, NODE_INDEX_COUNT) == -1)
			{
				// Push the children of the branch to the stack.
				int hx = nodeDataWidth >> 1, hy = nodeDataHeight >> 1;
				int l = nodeDataX - hx, t = nodeDataY - hx, r = nodeDataX + hx, b = nodeDataY + hy;
				PushNode(toProcess, firstChild + 0, nodeDataDepth + 1, l, t, hx, hy);
				PushNode(toProcess, firstChild + 1, nodeDataDepth + 1, r, t, hx, hy);
				PushNode(toProcess, firstChild + 2, nodeDataDepth + 1, l, b, hx, hy);
				PushNode(toProcess, firstChild + 3, nodeDataDepth + 1, r, b, hx, hy);
				visitor.VisitBranch(this, nodeDataIndex, nodeDataDepth, bounds);
			}
			else
			{
				visitor.VisitLeaf(this, nodeDataIndex, nodeDataDepth, bounds);
			}
		}
	}

	private static int Floor(float val) => (int)val;

	private static bool Intersect(int l1, int t1, int r1, int b1,
		int l2, int t2, int r2, int b2) =>
		l2 <= r1 && r2 >= l1 && t2 <= b1 && b2 >= t1;

	private static void PushNode(IntList nodes, int nodeDataIndex, int nodeDataDepth, int nodeDataX, int nodeDataY,
		int nodeDataWidth, int nodeDataHeight)
	{
		var backIndex = nodes.PushBack();
		nodes.Set(backIndex, NODE_DATA_INDEX_X, nodeDataX);
		nodes.Set(backIndex, NODE_DATA_INDEX_Y, nodeDataY);
		nodes.Set(backIndex, NODE_DATA_INDEX_WIDTH, nodeDataWidth);
		nodes.Set(backIndex, NODE_DATA_INDEX_HEIGHT, nodeDataHeight);
		nodes.Set(backIndex, NODE_DATA_INDEX_INDEX, nodeDataIndex);
		nodes.Set(backIndex, NODE_DATA_INDEX_DEPTH, nodeDataDepth);
	}

	private readonly IntList tmpLeaves = new(NODE_DATA_INDEX_NUM);
	private readonly IntList tmpToProcess = new(NODE_DATA_INDEX_NUM);

	private IntList FindLeaves(int node, int depth,
		int mx, int my, int sx, int sy,
		int lft, int top, int rgt, int btm)
	{
		var leaves = tmpLeaves;
		leaves.Clear();
		var toProcess = tmpToProcess;
		toProcess.Clear();
		PushNode(toProcess, node, depth, mx, my, sx, sy);

		while (toProcess.Size() > 0)
		{
			var backIndex = toProcess.Size() - 1;
			var nodeDataX = toProcess.Get(backIndex, NODE_DATA_INDEX_X);
			var nodeDataY = toProcess.Get(backIndex, NODE_DATA_INDEX_Y);
			var nodeDataWidth = toProcess.Get(backIndex, NODE_DATA_INDEX_WIDTH);
			var nodeDataHeight = toProcess.Get(backIndex, NODE_DATA_INDEX_HEIGHT);
			var nodeDataIndex = toProcess.Get(backIndex, NODE_DATA_INDEX_INDEX);
			var nodeDataDepth = toProcess.Get(backIndex, NODE_DATA_INDEX_DEPTH);
			toProcess.PopBack();

			// If this node is a leaf, insert it to the list.
			if (nodes.Get(nodeDataIndex, NODE_INDEX_COUNT) != -1)
			{
				PushNode(leaves, nodeDataIndex, nodeDataDepth, nodeDataX, nodeDataY, nodeDataWidth, nodeDataHeight);
			}
			else
			{
				// Otherwise push the children that intersect the rectangle.
				var fc = nodes.Get(nodeDataIndex, NODE_INDEX_FIRST_CHILD);
				int hx = nodeDataWidth / 2, hy = nodeDataHeight / 2;
				int l = nodeDataX - hx, t = nodeDataY - hx, r = nodeDataX + hx, b = nodeDataY + hy;

				if (top <= nodeDataY)
				{
					if (lft <= nodeDataX)
					{
						PushNode(toProcess, fc + 0, nodeDataDepth + 1, l, t, hx, hy);
					}

					if (rgt > nodeDataX)
					{
						PushNode(toProcess, fc + 1, nodeDataDepth + 1, r, t, hx, hy);
					}
				}

				if (btm > nodeDataY)
				{
					if (lft <= nodeDataX)
					{
						PushNode(toProcess, fc + 2, nodeDataDepth + 1, l, b, hx, hy);
					}

					if (rgt > nodeDataX)
					{
						PushNode(toProcess, fc + 3, nodeDataDepth + 1, r, b, hx, hy);
					}
				}
			}
		}

		return leaves;
	}

	private void NodeInsert(int index, int depth, int mx, int my, int sx, int sy, int element)
	{
		// Find the leaves and insert the element to all the leaves found.
		var lft = elements.Get(element, ELEMENT_INDEX_LEFT);
		var top = elements.Get(element, ELEMENT_INDEX_TOP);
		var rgt = elements.Get(element, ELEMENT_INDEX_RIGHT);
		var btm = elements.Get(element, ELEMENT_INDEX_BOTTOM);
		var leaves = FindLeaves(index, depth, mx, my, sx, sy, lft, top, rgt, btm);

		for (var j = 0; j < leaves.Size(); ++j)
		{
			var nodeDataX = leaves.Get(j, NODE_DATA_INDEX_X);
			var nodeDataY = leaves.Get(j, NODE_DATA_INDEX_Y);
			var nodeDataWidth = leaves.Get(j, NODE_DATA_INDEX_WIDTH);
			var nodeDataHeight = leaves.Get(j, NODE_DATA_INDEX_HEIGHT);
			var nodeDataIndex = leaves.Get(j, NODE_DATA_INDEX_INDEX);
			var nodeDataDepth = leaves.Get(j, NODE_DATA_INDEX_DEPTH);
			LeafInsert(nodeDataIndex, nodeDataDepth, nodeDataX, nodeDataY, nodeDataWidth, nodeDataHeight, element);
		}
	}

	private void LeafInsert(int node, int depth, int mx, int my, int sx, int sy, int element)
	{
		// Insert the element node to the leaf.
		var nodeDataFirstChild = nodes.Get(node, NODE_INDEX_FIRST_CHILD);
		nodes.Set(node, NODE_INDEX_FIRST_CHILD, elementNodes.Insert());
		elementNodes.Set(nodes.Get(node, NODE_INDEX_FIRST_CHILD), ELEMENT_NODE_INDEX_NEXT, nodeDataFirstChild);
		elementNodes.Set(nodes.Get(node, NODE_INDEX_FIRST_CHILD), ELEMENT_NODE_INDEX_ELEMENT, element);

		// If the leaf is full, split it.
		if (nodes.Get(node, NODE_INDEX_COUNT) == maxElements && depth < maxDepth)
		{
			// Transfer elements from the leaf node to a list of elements.
			var newElements = new IntList(1);
			while (nodes.Get(node, NODE_INDEX_FIRST_CHILD) != -1)
			{
				var index = nodes.Get(node, NODE_INDEX_FIRST_CHILD);
				var nextIndex = elementNodes.Get(index, ELEMENT_NODE_INDEX_NEXT);
				var elt = elementNodes.Get(index, ELEMENT_NODE_INDEX_ELEMENT); // Element

				// Pop off the element node from the leaf and remove it from the qt.
				nodes.Set(node, NODE_INDEX_FIRST_CHILD, nextIndex);
				elementNodes.Erase(index);

				// Insert element to the list.
				newElements.Set(newElements.PushBack(), field: 0, elt);
			}

			// Start by allocating 4 child nodes.
			var firstChild = nodes.Insert();
			nodes.Insert();
			nodes.Insert();
			nodes.Insert();
			nodes.Set(node, NODE_INDEX_FIRST_CHILD, firstChild);

			// Initialize the new child nodes.
			for (var j = 0; j < 4; ++j)
			{
				nodes.Set(firstChild + j, NODE_INDEX_FIRST_CHILD, val: -1);
				nodes.Set(firstChild + j, NODE_INDEX_COUNT, val: 0);
			}

			// Transfer the elements in the former leaf node to its new children.
			nodes.Set(node, NODE_INDEX_COUNT, val: -1);
			for (var j = 0; j < newElements.Size(); ++j)
			{
				NodeInsert(node, depth, mx, my, sx, sy, newElements.Get(j, field: 0));
			}
		}
		else
		{
			// Increment the leaf element count.
			nodes.Set(node, NODE_INDEX_COUNT, nodes.Get(node, NODE_INDEX_COUNT) + 1);
		}
	}
}

public class IntList
{
	private readonly int fieldCount;

	private int[] data;
	private int num;
	private int size = 128;
	private int freeElement = -1;

	// Creates a new list of elements which each consist of integer fields.
	// 'start_num_fields' specifies the number of integer fields each element has.
	public IntList(int initialFieldCount)
	{
		fieldCount = initialFieldCount;
		data = new int[size * initialFieldCount];
	}

	// Returns the number of elements in the list.
	public int Size() => num;

	// Returns the value of the specified field for the nth element.
	public int Get(int n, int field) =>
		// assert n >= 0 && n < num && field >= 0 && field < num_fields;
		data[n * fieldCount + field];

	// Sets the value of the specified field for the nth element.
	public void Set(int n, int field, int val)
	{
		// assert n >= 0 && n < num && field >= 0 && field < num_fields;
		data[n * fieldCount + field] = val;
	}

	// Clears the list, making it empty.
	public void Clear()
	{
		num = 0;
		freeElement = -1;
	}

	// Inserts an element to the back of the list and returns an index to it.
	public int PushBack()
	{
		var newPosition = (num + 1) * fieldCount;

		// If the list is full, we need to reallocate the buffer to make room
		// for the new element.
		if (newPosition > size)
		{
			// Use double the size for the new capacity.
			var newSize = newPosition * 2;

			// Allocate new array and copy former contents.
			var newArray = new int[newSize];
			Array.Copy(data, newArray, size);
			data = newArray;

			// Set the old capacity to the new capacity.
			size = newSize;
		}

		return num++;
	}

	// Removes the element at the back of the list.
	public void PopBack()
	{
		// Just decrement the list size.
		num -= 1;
	}

	// Inserts an element to a vacant position in the list and returns an index to it.
	public int Insert()
	{
		if (freeElement == -1)
		{
			return PushBack();
		}

		var index = freeElement;
		var pos = index * fieldCount;

		freeElement = data[pos];

		return index;
	}

	// Removes the nth element in the list.
	public void Erase(int n)
	{
		// Push the element to the free list.
		var index = n * fieldCount;
		data[index] = freeElement;
		freeElement = n;
	}
}