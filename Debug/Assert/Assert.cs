//----------------------------------------------------------------------------------------------
// author Leonid [Zanleo] Voitko
//----------------------------------------------------------------------------------------------

// Ons ome unity version - not work, so, simple replace in cur file UNITY_ASSERTIONS string to some else)
//#undef UNITY_ASSERTIONS

namespace Core.Utils
{
	using System.Collections.Generic;
	using System.Diagnostics;
	using UAssert = UnityEngine.Assertions.Assert;
	// For quick use this class Ctrl+Shif+H and replace
	// using UnityEngine.Assertions;
	// to 
	// using Assert = Core.Utils.Assert;

	/// <summary>
	///   <para>The Assert class contains assertion methods for setting invariants in the code.</para>
	/// </summary>
	[DebuggerStepThrough]
	public static class Assert
	{
		/// <summary>
		///   <para>Should an exception be thrown on a failure.</para>
		/// </summary>
		public static bool raiseExceptions
		{
			get { return UAssert.raiseExceptions; }
			set { UAssert.raiseExceptions = value; }
		}

		/// <summary>
		///   <para>Asserts that the condition is true.</para>
		/// </summary>
		/// <param name="condition"></param>
		/// <param name="message"></param>
		[Conditional("UNITY_ASSERTIONS")]
		public static void IsTrue(bool condition)
		{
			IsTrue(condition, (string)null);
		}

		/// <summary>
		///   <para>Asserts that the condition is true.</para>
		/// </summary>
		/// <param name="condition">If condition is false, then message be thrown</param>
		/// <param name="message"></param>
		[Conditional("UNITY_ASSERTIONS")]
		public static void IsTrue(bool condition, string message)
		{
			UAssert.IsTrue(condition, message);
		}

		/// <summary>
		///   <para>Asserts that the condition is false.</para>
		/// </summary>
		/// <param name="condition"></param>
		/// <param name="message"></param>
		[Conditional("UNITY_ASSERTIONS")]
		public static void IsFalse(bool condition)
		{
			IsFalse(condition, (string)null);
		}

		/// <summary>
		///   <para>Asserts that the condition is false.</para>
		/// </summary>
		/// <param name="condition"></param>
		/// <param name="message"></param>
		[Conditional("UNITY_ASSERTIONS")]
		public static void IsFalse(bool condition, string message)
		{
			UAssert.IsFalse(condition, message);
		}

		/// <summary>
		///         <para>Asserts that the values are approximately equal. An absolute error check is used for approximate equality check (|a-b| &lt; tolerance). Default tolerance is 0.00001f.
		/// 
		/// Note: Every time you call the method with tolerance specified, a new instance of Assertions.Comparers.FloatComparer is created. For performance reasons you might want to instance your own comparer and pass it to the AreEqual method. If the tolerance is not specifies, a default comparer is used and the issue does not occur.</para>
		///       </summary>
		/// <param name="tolerance">Tolerance of approximation.</param>
		/// <param name="expected"></param>
		/// <param name="actual"></param>
		/// <param name="message"></param>
		[Conditional("UNITY_ASSERTIONS")]
		public static void AreApproximatelyEqual(float expected, float actual)
		{
			UAssert.AreApproximatelyEqual(expected, actual);
		}

		/// <summary>
		///         <para>Asserts that the values are approximately equal. An absolute error check is used for approximate equality check (|a-b| &lt; tolerance). Default tolerance is 0.00001f.
		/// 
		/// Note: Every time you call the method with tolerance specified, a new instance of Assertions.Comparers.FloatComparer is created. For performance reasons you might want to instance your own comparer and pass it to the AreEqual method. If the tolerance is not specifies, a default comparer is used and the issue does not occur.</para>
		///       </summary>
		/// <param name="tolerance">Tolerance of approximation.</param>
		/// <param name="expected"></param>
		/// <param name="actual"></param>
		/// <param name="message"></param>
		[Conditional("UNITY_ASSERTIONS")]
		public static void AreApproximatelyEqual(float expected, float actual, string message)
		{
			UAssert.AreApproximatelyEqual(expected, actual, message);
		}

		/// <summary>
		///         <para>Asserts that the values are approximately equal. An absolute error check is used for approximate equality check (|a-b| &lt; tolerance). Default tolerance is 0.00001f.
		/// 
		/// Note: Every time you call the method with tolerance specified, a new instance of Assertions.Comparers.FloatComparer is created. For performance reasons you might want to instance your own comparer and pass it to the AreEqual method. If the tolerance is not specifies, a default comparer is used and the issue does not occur.</para>
		///       </summary>
		/// <param name="tolerance">Tolerance of approximation.</param>
		/// <param name="expected"></param>
		/// <param name="actual"></param>
		/// <param name="message"></param>
		[Conditional("UNITY_ASSERTIONS")]
		public static void AreApproximatelyEqual(float expected, float actual, float tolerance)
		{
			UAssert.AreApproximatelyEqual(expected, actual, tolerance);
		}

		/// <summary>
		///         <para>Asserts that the values are approximately equal. An absolute error check is used for approximate equality check (|a-b| &lt; tolerance). Default tolerance is 0.00001f.
		/// 
		/// Note: Every time you call the method with tolerance specified, a new instance of Assertions.Comparers.FloatComparer is created. For performance reasons you might want to instance your own comparer and pass it to the AreEqual method. If the tolerance is not specifies, a default comparer is used and the issue does not occur.</para>
		///       </summary>
		/// <param name="tolerance">Tolerance of approximation.</param>
		/// <param name="expected"></param>
		/// <param name="actual"></param>
		/// <param name="message"></param>
		[Conditional("UNITY_ASSERTIONS")]
		public static void AreApproximatelyEqual(float expected, float actual, float tolerance, string message)
		{
			UAssert.AreApproximatelyEqual(expected, actual, tolerance, message);
		}

		/// <summary>
		///   <para>Asserts that the values are approximately not equal. An absolute error check is used for approximate equality check (|a-b| &lt; tolerance). Default tolerance is 0.00001f.</para>
		/// </summary>
		/// <param name="tolerance">Tolerance of approximation.</param>
		/// <param name="expected"></param>
		/// <param name="actual"></param>
		/// <param name="message"></param>
		[Conditional("UNITY_ASSERTIONS")]
		public static void AreNotApproximatelyEqual(float expected, float actual)
		{
			UAssert.AreNotApproximatelyEqual(expected, actual);
		}

		/// <summary>
		///   <para>Asserts that the values are approximately not equal. An absolute error check is used for approximate equality check (|a-b| &lt; tolerance). Default tolerance is 0.00001f.</para>
		/// </summary>
		/// <param name="tolerance">Tolerance of approximation.</param>
		/// <param name="expected"></param>
		/// <param name="actual"></param>
		/// <param name="message"></param>
		[Conditional("UNITY_ASSERTIONS")]
		public static void AreNotApproximatelyEqual(float expected, float actual, string message)
		{
			UAssert.AreNotApproximatelyEqual(expected, actual, message);
		}

		/// <summary>
		///   <para>Asserts that the values are approximately not equal. An absolute error check is used for approximate equality check (|a-b| &lt; tolerance). Default tolerance is 0.00001f.</para>
		/// </summary>
		/// <param name="tolerance">Tolerance of approximation.</param>
		/// <param name="expected"></param>
		/// <param name="actual"></param>
		/// <param name="message"></param>
		[Conditional("UNITY_ASSERTIONS")]
		public static void AreNotApproximatelyEqual(float expected, float actual, float tolerance)
		{
			UAssert.AreNotApproximatelyEqual(expected, actual, tolerance);
		}

		/// <summary>
		///   <para>Asserts that the values are approximately not equal. An absolute error check is used for approximate equality check (|a-b| &lt; tolerance). Default tolerance is 0.00001f.</para>
		/// </summary>
		/// <param name="tolerance">Tolerance of approximation.</param>
		/// <param name="expected"></param>
		/// <param name="actual"></param>
		/// <param name="message"></param>
		[Conditional("UNITY_ASSERTIONS")]
		public static void AreNotApproximatelyEqual(float expected, float actual, float tolerance, string message)
		{
			UAssert.AreNotApproximatelyEqual(expected, actual, tolerance, message);
		}

		[Conditional("UNITY_ASSERTIONS")]
		public static void AreEqual<T>(T expected, T actual)
		{
			UAssert.AreEqual(expected, actual);
		}

		[Conditional("UNITY_ASSERTIONS")]
		public static void AreEqual<T>(T expected, T actual, string message)
		{
			UAssert.AreEqual(expected, actual, message);
		}

		[Conditional("UNITY_ASSERTIONS")]
		public static void AreEqual<T>(T expected, T actual, string message, IEqualityComparer<T> comparer)
		{
			UAssert.AreEqual(expected, actual, message, comparer);
		}

		[Conditional("UNITY_ASSERTIONS")]
		public static void AreEqual(UnityEngine.Object expected, UnityEngine.Object actual, string message)
		{
			UAssert.AreEqual(expected, actual, message);
		}

		[Conditional("UNITY_ASSERTIONS")]
		public static void AreNotEqual<T>(T expected, T actual)
		{
			UAssert.AreNotEqual(expected, actual);
		}

		[Conditional("UNITY_ASSERTIONS")]
		public static void AreNotEqual<T>(T expected, T actual, string message)
		{
			UAssert.AreNotEqual(expected, actual, message);
		}

		[Conditional("UNITY_ASSERTIONS")]
		public static void AreNotEqual<T>(T expected, T actual, string message, IEqualityComparer<T> comparer)
		{
			UAssert.AreNotEqual(expected, actual, message, comparer);
		}

		[Conditional("UNITY_ASSERTIONS")]
		public static void AreNotEqual(UnityEngine.Object expected, UnityEngine.Object actual, string message)
		{
			UAssert.AreNotEqual(expected, actual, message);
		}

		[Conditional("UNITY_ASSERTIONS")]
		public static void IsNull<T>(T value) where T : class
		{
			UAssert.IsNull(value);
		}

		[Conditional("UNITY_ASSERTIONS")]
		public static void IsNull<T>(T value, string message) where T : class
		{
			UAssert.IsNull(value, message);
		}

		[Conditional("UNITY_ASSERTIONS")]
		public static void IsNull(UnityEngine.Object value, string message)
		{
			UAssert.IsNull(value, message);
		}

		[Conditional("UNITY_ASSERTIONS")]
		public static void IsNotNull<T>(T value) where T : class
		{
			UAssert.IsNotNull(value);
		}

		[Conditional("UNITY_ASSERTIONS")]
		public static void IsNotNull<T>(T value, string message) where T : class
		{
			UAssert.IsNotNull(value, message);
		}

		[Conditional("UNITY_ASSERTIONS")]
		public static void IsNotNull(UnityEngine.Object value, string message)
		{
			UAssert.IsNotNull(value, message);
		}
	}
}