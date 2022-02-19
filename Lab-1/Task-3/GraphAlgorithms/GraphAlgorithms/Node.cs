namespace GraphAlgorithms;

public class Node<T>
{
    private T _value;
    private readonly HashSet<Node<T>> _neighbors;
    
    public Node(T value) {
        _value = value;
        _neighbors = new HashSet<Node<T>>();
    }

    public void connect(Node<T> node)
    {
        _neighbors.Add(node);
        node._neighbors.Add(this);
    }

    public T getValue() {
        return _value;
    }

    public IReadOnlySet<Node<T>> getNeighbors() {
        return _neighbors;
    }
}