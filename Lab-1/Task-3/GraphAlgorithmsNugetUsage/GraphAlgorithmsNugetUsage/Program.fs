open System
open GraphAlgorithms;

[<EntryPoint>]
let main argv =
    let node = new Node<int>(5);
    let node2 = new Node<int>(6);
    let node3 = new Node<int>(8);
    node.connect(node2);
    node2.connect(node3);
    let result = Algorithms.BFS(8,node);
    0 
