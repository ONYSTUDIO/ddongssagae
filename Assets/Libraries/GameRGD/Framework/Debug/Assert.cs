using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Helen
{
    [DebuggerStepThrough]
    public static class Assert
    {
        private static void Fail(string message, string userMessage)
        {
            throw new UnityEngine.Assertions.AssertionException(message, string.Format("Message: {0}", userMessage));
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void Fail()
        {
            Assert.Fail((string)null);
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void Fail(string message)
        {
            Assert.Fail(AssertionMessageUtil.BooleanFailureMessage(true), message);
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void IsEquals<T>(T obj1, T obj2)
        {
            Assert.IsEquals(obj1, obj2, (string)null);
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void IsEquals<T>(T obj1, T obj2, string message)
        {
            Assert.IsTrue(obj1.Equals(obj2), message);
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void IsTrue(bool condition)
        {
            Assert.IsTrue(condition, (string)null);
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void IsTrue(bool condition, string message)
        {
            if (condition)
                return;
            Assert.Fail(AssertionMessageUtil.BooleanFailureMessage(true), message);
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void IsFalse(bool condition)
        {
            Assert.IsFalse(condition, (string)null);
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void IsFalse(bool condition, string message)
        {
            if (!condition)
                return;
            Assert.Fail(AssertionMessageUtil.BooleanFailureMessage(false), message);
        }
        [Conditional("UNITY_ASSERTIONS")]
        public static void AreApproximatelyEqual(float expected, float actual)
        {
            Assert.AreEqual<float>(expected, actual, (string)null, (IEqualityComparer<float>)UnityEngine.Assertions.Comparers.FloatComparer.s_ComparerWithDefaultTolerance);
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void AreApproximatelyEqual(float expected, float actual, string message)
        {
            Assert.AreEqual<float>(expected, actual, message, (IEqualityComparer<float>)UnityEngine.Assertions.Comparers.FloatComparer.s_ComparerWithDefaultTolerance);
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void AreApproximatelyEqual(float expected, float actual, float tolerance)
        {
            Assert.AreApproximatelyEqual(expected, actual, tolerance, (string)null);
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void AreApproximatelyEqual(float expected, float actual, float tolerance, string message)
        {
            Assert.AreEqual<float>(expected, actual, message, (IEqualityComparer<float>)new UnityEngine.Assertions.Comparers.FloatComparer(tolerance));
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void AreNotApproximatelyEqual(float expected, float actual)
        {
            Assert.AreNotEqual<float>(expected, actual, (string)null, (IEqualityComparer<float>)UnityEngine.Assertions.Comparers.FloatComparer.s_ComparerWithDefaultTolerance);
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void AreNotApproximatelyEqual(float expected, float actual, string message)
        {
            Assert.AreNotEqual<float>(expected, actual, message, (IEqualityComparer<float>)UnityEngine.Assertions.Comparers.FloatComparer.s_ComparerWithDefaultTolerance);
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void AreNotApproximatelyEqual(float expected, float actual, float tolerance)
        {
            Assert.AreNotApproximatelyEqual(expected, actual, tolerance, (string)null);
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void AreNotApproximatelyEqual(float expected, float actual, float tolerance, string message)
        {
            Assert.AreNotEqual<float>(expected, actual, message, (IEqualityComparer<float>)new UnityEngine.Assertions.Comparers.FloatComparer(tolerance));
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void AreEqual<T>(T expected, T actual)
        {
            Assert.AreEqual<T>(expected, actual, (string)null);
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void AreEqual<T>(T expected, T actual, string message)
        {
            Assert.AreEqual<T>(expected, actual, message, (IEqualityComparer<T>)EqualityComparer<T>.Default);
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void AreEqual<T>(T expected, T actual, string message, IEqualityComparer<T> comparer)
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T)))
            {
                Assert.AreEqual((object)expected as UnityEngine.Object, (object)actual as UnityEngine.Object, message);
            }
            else
            {
                if (comparer.Equals(actual, expected))
                    return;
                Assert.Fail(AssertionMessageUtil.GetEqualityMessage((object)actual, (object)expected, true), message);
            }
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void AreEqual(UnityEngine.Object expected, UnityEngine.Object actual, string message)
        {
            if (!(actual != expected))
                return;
            Assert.Fail(AssertionMessageUtil.GetEqualityMessage((object)actual, (object)expected, true), message);
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void AreNotEqual<T>(T expected, T actual)
        {
            Assert.AreNotEqual<T>(expected, actual, (string)null);
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void AreNotEqual<T>(T expected, T actual, string message)
        {
            Assert.AreNotEqual<T>(expected, actual, message, (IEqualityComparer<T>)EqualityComparer<T>.Default);
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void AreNotEqual<T>(T expected, T actual, string message, IEqualityComparer<T> comparer)
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T)))
            {
                Assert.AreNotEqual((object)expected as UnityEngine.Object, (object)actual as UnityEngine.Object, message);
            }
            else
            {
                if (!comparer.Equals(actual, expected))
                    return;
                Assert.Fail(AssertionMessageUtil.GetEqualityMessage((object)actual, (object)expected, false), message);
            }
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void AreNotEqual(UnityEngine.Object expected, UnityEngine.Object actual, string message)
        {
            if (!(actual == expected))
                return;
            Assert.Fail(AssertionMessageUtil.GetEqualityMessage((object)actual, (object)expected, false), message);
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void IsNull<T>(T value) where T : class
        {
            Assert.IsNull<T>(value, (string)null);
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void IsNull<T>(T value, string message) where T : class
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T)))
            {
                Assert.IsNull((object)value as UnityEngine.Object, message);
            }
            else
            {
                if ((object)value == null)
                    return;
                Assert.Fail(AssertionMessageUtil.NullFailureMessage((object)value, true), message);
            }
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void IsNull(UnityEngine.Object value, string message)
        {
            if (!(value != (UnityEngine.Object)null))
                return;
            Assert.Fail(AssertionMessageUtil.NullFailureMessage((object)value, true), message);
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void IsNotNull<T>(T value) where T : class
        {
            Assert.IsNotNull<T>(value, (string)null);
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void IsNotNull<T>(T value, string message) where T : class
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T)))
            {
                Assert.IsNotNull((object)value as UnityEngine.Object, message);
            }
            else
            {
                if ((object)value != null)
                    return;
                Assert.Fail(AssertionMessageUtil.NullFailureMessage((object)value, false), message);
            }
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void IsNotNull(UnityEngine.Object value, string message)
        {
            if (!(value == (UnityEngine.Object)null))
                return;
            Assert.Fail(AssertionMessageUtil.NullFailureMessage((object)value, false), message);
        }

        private class AssertionMessageUtil
        {
            private const string k_Expected = "Expected:";
            private const string k_AssertionFailed = "Assertion failed.";

            public static string GetMessage(string failureMessage)
            {
                return string.Format("{0} {1}", k_AssertionFailed, (object)failureMessage);
            }

            public static string GetMessage(string failureMessage, string expected)
            {
                return AssertionMessageUtil.GetMessage(string.Format("{0}{1}{2} {3}", (object)failureMessage, (object)System.Environment.NewLine, k_Expected, (object)expected));
            }

            public static string GetEqualityMessage(object actual, object expected, bool expectEqual)
            {
                return AssertionMessageUtil.GetMessage(string.Format("Values are {0}equal.", (object)(!expectEqual ? string.Empty : "not ")), string.Format("{0} {2} {1}", actual, expected, (object)(!expectEqual ? "!=" : "==")));
            }

            public static string NullFailureMessage(object value, bool expectNull)
            {
                return AssertionMessageUtil.GetMessage(string.Format("Value was {0}Null", (object)(!expectNull ? string.Empty : "not ")), string.Format("Value was {0}Null", (object)(!expectNull ? "not " : string.Empty)));
            }

            public static string BooleanFailureMessage(bool expected)
            {
                return AssertionMessageUtil.GetMessage("Value was " + (object)!expected, expected.ToString());
            }
        }
    }
}
