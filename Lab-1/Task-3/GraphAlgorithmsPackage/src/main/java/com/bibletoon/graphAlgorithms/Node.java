package com.bibletoon.graphAlgorithms;

import java.util.Collection;
import java.util.HashSet;
import java.util.Set;

public class Node<T> {
    private T value;
    private Set<Node<T>> neighbors;

    public Node(T value) {
        this.value = value;
        this.neighbors = new HashSet<>();
    }

    public void connect(Node<T> node) {
        this.neighbors.add(node);
        node.neighbors.add(this);
    }

    public T getValue() {
        return value;
    }

    public Collection<Node<T>> getNeighbors() {
        return neighbors;
    }
}