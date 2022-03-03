namespace GraphAlgorithms;

public class Algorithms
{
    public static Node<T>? BFS<T>(T value, Node<T> start) {
        HashSet<Node<T>> alreadyVisited = new HashSet<Node<T>>();
        Queue<Node<T>> queue = new Queue<Node<T>>();
        queue.Enqueue(start);

        Node<T> currentNode;

        while (queue.Count != 0) {
            currentNode = queue.Dequeue();

            if (currentNode.getValue().Equals(value)) {
                return currentNode;
            } else {
                alreadyVisited.Add(currentNode);
                foreach (Node<T> neighbor in currentNode.getNeighbors())
                {
                    if (!alreadyVisited.Contains(neighbor))
                        queue.Enqueue(neighbor);
                }
            }
        }

        return null;
    }

    public static Node<T>? DFS<T>(T value, Node<T> start) {
        Stack<Node<T>> stack = new Stack<Node<T>>();
        HashSet<Node<T>> alreadyVisited = new HashSet<Node<T>>();
        stack.Push(start);
        while (stack.Count != 0) {
            Node<T> current = stack.Pop();
            if (!alreadyVisited.Contains(current)) {
                alreadyVisited.Add(current);

                if (current.getValue().Equals(value))
                    return current;
                foreach (Node<T> dest in current.getNeighbors()) 
                {
                    if (!alreadyVisited.Contains(current))
                        stack.Push(dest);
                }
            }
        }

        return null;
    }
}