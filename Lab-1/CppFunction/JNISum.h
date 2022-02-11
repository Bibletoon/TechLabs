#include <jni.h>

#ifndef _Included_ru_forwolk_test_JNISum
#define _Included_ru_forwolk_test_JNISum
#ifdef __cplusplus
extern "C" {
#endif

JNIEXPORT jint JNICALL Java_com_bibletoon_JNIMethods_SumTwoIntsInterop
  (JNIEnv *, jobject, jint, jint);

#ifdef __cplusplus
}
#endif
#endif