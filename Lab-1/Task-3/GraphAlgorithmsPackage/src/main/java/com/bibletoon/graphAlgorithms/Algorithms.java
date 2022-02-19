package com.bibletoon.graphAlgorithms;

import java.util.*;

public class Algorithms {
    public static <T> Optional<Node<T>> BFS(T value, Node<T> start) {
        Set<Node<T>> alreadyVisited = new HashSet<Node<T>>();
        Queue<Node<T>> queue = new ArrayDeque<>();
        queue.add(start);

        Node<T> currentNode;

        while (!queue.isEmpty()) {
            currentNode = queue.remove();

            if (currentNode.getValue().equals(value)) {
                return Optional.of(currentNode);
            } else {
                alreadyVisited.add(currentNode);
                queue.addAll(currentNode.getNeighbors());
                queue.removeAll(alreadyVisited);
            }
        }

        return Optional.empty();
    }

    public static <T> Optional<Node<T>> DFS(T value, Node<T> start) {
        Stack<Node<T>> stack = new Stack<>();
        HashSet<Node<T>> alreadyVisited = new HashSet<>();
        stack.push(start);
        while (!stack.isEmpty()) {
            Node<T> current = stack.pop();
            if (!alreadyVisited.contains(current)) {
                alreadyVisited.add(current);
                if (current.getValue() == value)
                    return Optional.of(current);
                for (Node<T> dest : current.getNeighbors()) {
                    if (!alreadyVisited.contains(current))
                        stack.push(dest);
                }
            }
        }

        return Optional.empty();
    }
}
