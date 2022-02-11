package com.bibletoon;

public class JNIMethods {
    native int SumTwoIntsInterop(int a, int b);
    static {
        System.load(System.getProperty("user.dir")+"/libs/jnisum.so"); //Absolute path!
    }
}
