import com.bibletoon.graphAlgorithms.Algorithms
import com.bibletoon.graphAlgorithms.Node

fun main(args: Array<String>) {
    var node = Node<Int>(5)
    var node2 = Node<Int>(6);
    var node3 = Node<Int>(8);
    node.connect(node2);
    node2.connect(node3);
    Algorithms.BFS(8, node);
}