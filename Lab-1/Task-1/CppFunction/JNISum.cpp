#include "JNISum.h"

JNIEXPORT jint JNICALL Java_com_bibletoon_JNIMethods_SumTwoIntsInterop
        (JNIEnv *, jobject, jint a, jint b) {
    return a+b;
}