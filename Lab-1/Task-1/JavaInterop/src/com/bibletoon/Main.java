package com.bibletoon;

public class Main {
    public static void main(String[] args) {
        JNIMethods jniMethods = new JNIMethods();
        System.out.println(jniMethods.SumTwoIntsInterop(10, 10));
    }
}
