// Copyright (c) 2012, Joshua Burke  
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics.Contracts;

namespace Frost
{
	/// <summary>
	///   represents a transformation matrix for use in 2D space
	/// </summary>
	public struct Matrix3X2 : IEquatable<Matrix3X2>
	{
		private static readonly Matrix3X2 _Identity;

		private readonly float _11;
		private readonly float _12;
		private readonly float _21;
		private readonly float _22;
		private readonly float _31;
		private readonly float _32;

		static Matrix3X2()
		{
			_Identity = new Matrix3X2(1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f);
		}

		/// <summary>
		///   constructs a new <see cref="Matrix3X2" /> from its components
		/// </summary>
		/// <param name="m11"> the finite M11 component </param>
		/// <param name="m12"> the finite M12 component </param>
		/// <param name="m21"> the finite M21 component </param>
		/// <param name="m22"> the finite M22 component </param>
		/// <param name="m31"> the finite M31 component </param>
		/// <param name="m32"> the finite M32 component </param>
		public Matrix3X2(
			float m11, float m12, float m21, float m22, float m31, float m32)
		{
			Contract.Requires(Check.IsFinite(m11));
			Contract.Requires(Check.IsFinite(m12));
			Contract.Requires(Check.IsFinite(m21));
			Contract.Requires(Check.IsFinite(m22));
			Contract.Requires(Check.IsFinite(m31));
			Contract.Requires(Check.IsFinite(m32));

			_11 = m11;
			_12 = m12;
			_21 = m21;
			_22 = m22;
			_31 = m31;
			_32 = m32;

			Contract.Assert(M11.Equals(m11));
			Contract.Assert(M12.Equals(m12));
			Contract.Assert(M21.Equals(m21));
			Contract.Assert(M22.Equals(m22));
			Contract.Assert(M31.Equals(m31));
			Contract.Assert(M32.Equals(m32));
		}

		/// <summary>
		///   gets the identity matrix
		/// </summary>
		public static Matrix3X2 Identity
		{
			get
			{
				Contract.Ensures(Contract.Result<Matrix3X2>().Equals(_Identity));

				return _Identity;
			}
		}

		/// <summary>
		///   gets the M32 component of the <see cref="Matrix3X2" />
		/// </summary>
		public float M32
		{
			get
			{
				Contract.Ensures(Check.IsFinite(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(_32));

				return _32;
			}
		}

		/// <summary>
		///   gets the M31 component of the <see cref="Matrix3X2" />
		/// </summary>
		public float M31
		{
			get
			{
				Contract.Ensures(Check.IsFinite(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(_31));

				return _31;
			}
		}

		/// <summary>
		///   gets the M22 component of the <see cref="Matrix3X2" />
		/// </summary>
		public float M22
		{
			get
			{
				Contract.Ensures(Check.IsFinite(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(_22));

				return _22;
			}
		}

		/// <summary>
		///   gets the M21 component of the <see cref="Matrix3X2" />
		/// </summary>
		public float M21
		{
			get
			{
				Contract.Ensures(Check.IsFinite(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(_21));

				return _21;
			}
		}

		/// <summary>
		///   gets the M12 component of the <see cref="Matrix3X2" />
		/// </summary>
		public float M12
		{
			get
			{
				Contract.Ensures(Check.IsFinite(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(_12));

				return _12;
			}
		}

		/// <summary>
		///   gets the M11 component of the <see cref="Matrix3X2" />
		/// </summary>
		public float M11
		{
			get
			{
				Contract.Ensures(Check.IsFinite(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(_11));

				return _11;
			}
		}

		/// <summary>
		///   gets a value indicating whether the <see cref="Matrix3X2" /> is equal to <see cref="Matrix3X2.Identity" />
		/// </summary>
		public bool IsIdentity
		{
			get
			{
				return _11.Equals(1.0f) && _12.Equals(0.0f) && _21.Equals(0.0f) &&
					_22.Equals(1.0f) &&
						_31.Equals(0.0f) && _32.Equals(0.0f);
			}
		}

		public bool Equals(Matrix3X2 other)
		{
			return other._11.Equals(_11) && other._12.Equals(_12) &&
				other._21.Equals(_21) &&
					other._22.Equals(_22) && other._31.Equals(_31) && other._32.Equals(_32);
		}

		/// <summary>
		///   translates the <see cref="Matrix3X2" /> by the given amount
		/// </summary>
		/// <param name="amount"> the amount to translate </param>
		/// <param name="result"> stores the result of the translation </param>
		public void Translate(Size amount, out Matrix3X2 result)
		{
			Translate(amount.Width, amount.Height, out result);
		}

		/// <summary>
		///   translates the <see cref="Matrix3X2" /> by the given amounts
		/// </summary>
		/// <param name="width"> the amount to translate along the horizontal axis </param>
		/// <param name="height"> the amount to translate along the vertical axis </param>
		/// <param name="result"> stores the result of the translation </param>
		public void Translate(float width, float height, out Matrix3X2 result)
		{
			Contract.Requires(Check.IsFinite(width));
			Contract.Requires(Check.IsFinite(height));

			Matrix3X2 translation = new Matrix3X2(
				1.0f, 0.0f, 0.0f, 1.0f, width, height);

			translation.Multiply(ref this, out result);
		}

		/// <summary>
		///   skews the <see cref="Matrix3X2" /> by the given angles
		/// </summary>
		/// <param name="angleX"> the angle in degrees to skew horizontally </param>
		/// <param name="angleY"> the angle in degrees to skew vertically </param>
		/// <param name="result"> stores the result of the skew operation </param>
		public void Skew(float angleX, float angleY, out Matrix3X2 result)
		{
			Contract.Requires(Check.IsDegrees(angleX));
			Contract.Requires(Check.IsDegrees(angleY));

			double radiansX = (Math.PI * angleX) / 180.0;
			double radiansY = (Math.PI * angleY) / 180.0;

			Matrix3X2 skew = new Matrix3X2(
				1.0f,
				Convert.ToSingle(Math.Tan(radiansX)),
				Convert.ToSingle(Math.Tan(radiansY)),
				1.0f,
				0.0f,
				0.0f);

			skew.Multiply(ref this, out result);
		}

		/// <summary>
		///   scales the <see cref="Matrix3X2" /> by the given amount
		/// </summary>
		/// <param name="amount"> the amount to scale by </param>
		/// <param name="result"> stores the result of the scaling operation </param>
		public void Scale(Size amount, out Matrix3X2 result)
		{
			Scale(amount.Width, amount.Height, out result);
		}

		/// <summary>
		///   scales the <see cref="Matrix3X2" /> by the given amounts
		/// </summary>
		/// <param name="width"> the amount to scale along the horizontal axis </param>
		/// <param name="height"> the amount to scale along the vertical axis </param>
		/// <param name="result"> stores the result of the scaling operation </param>
		public void Scale(float width, float height, out Matrix3X2 result)
		{
			Contract.Requires(Check.IsPositive(width));
			Contract.Requires(Check.IsPositive(height));

			Matrix3X2 scaling = new Matrix3X2(width, 0.0f, 0.0f, height, 0.0f, 0.0f);

			scaling.Multiply(ref this, out result);
		}

		/// <summary>
		///   scales the <see cref="Matrix3X2" /> by the given amount
		/// </summary>
		/// <param name="amount"> the amount to scale by </param>
		/// <param name="origin"> the origin of the scaling </param>
		/// <param name="result"> stores the result of the scaling operation </param>
		public void Scale(Size amount, Point origin, out Matrix3X2 result)
		{
			Scale(amount.Width, amount.Height, origin.X, origin.Y, out result);
		}

		/// <summary>
		///   scales the <see cref="Matrix3X2" /> by the given amounts
		/// </summary>
		/// <param name="width"> the positive amount to scale on the horizontal axis </param>
		/// <param name="height"> the positive amount to scale on the vertical axis </param>
		/// <param name="originX"> the origin of the scaling operation on the horizontal axis </param>
		/// <param name="originY"> the origin of the scaling operation on the vertical axis </param>
		/// <param name="result"> stores the result of the scaling operation </param>
		public void Scale(
			float width,
			float height,
			float originX,
			float originY,
			out Matrix3X2 result)
		{
			Contract.Requires(Check.IsPositive(width));
			Contract.Requires(Check.IsPositive(height));
			Contract.Requires(Check.IsFinite(originX));
			Contract.Requires(Check.IsFinite(originY));

			float translationX = originX - (width * originX);
			float translationY = originY - (height * originY);

			Matrix3X2 scaling = new Matrix3X2(
				width, 0.0f, 0.0f, height, translationX, translationY);

			scaling.Multiply(ref this, out result);
		}

		/// <summary>
		///   rotates the <see cref="Matrix3X2" /> by the given angle
		/// </summary>
		/// <param name="angle"> the angle to rotate in degrees </param>
		/// <param name="result"> stores the result of the rotation </param>
		public void Rotate(float angle, out Matrix3X2 result)
		{
			Contract.Requires(Check.IsDegrees(angle));

			double radians = (Math.PI * angle) / 180.0;

			float rcos = Convert.ToSingle(Math.Cos(radians));
			float rsin = Convert.ToSingle(Math.Sin(radians));

			Matrix3X2 rotation = new Matrix3X2(rcos, rsin, -rsin, rcos, 0.0f, 0.0f);

			rotation.Multiply(ref this, out result);
		}

		/// <summary>
		///   rotates the <see cref="Matrix3X2" /> by the given angle
		/// </summary>
		/// <param name="angle"> the angle to rotate in degrees </param>
		/// <param name="origin"> the origin of the rotation </param>
		/// <param name="result"> stores the result of the rotation </param>
		public void Rotate(float angle, Point origin, out Matrix3X2 result)
		{
			Rotate(angle, origin.X, origin.Y, out result);
		}

		/// <summary>
		///   rotates the <see cref="Matrix3X2" /> by the given angle
		/// </summary>
		/// <param name="angle"> the angle to rotate in degrees </param>
		/// <param name="originX"> the origin of the rotation on the horizontal axis </param>
		/// <param name="originY"> the origin on the rotation on the vertical axis </param>
		/// <param name="result"> stores the result of the rotation </param>
		public void Rotate(
			float angle, float originX, float originY, out Matrix3X2 result)
		{
			Contract.Requires(Check.IsDegrees(angle));
			Contract.Requires(Check.IsFinite(originX));
			Contract.Requires(Check.IsFinite(originY));

			Matrix3X2 nTranslate;
			Matrix3X2 pTranslate;

			Identity.Translate(-originX, -originY, out nTranslate);
			Identity.Translate(+originX, +originY, out pTranslate);

			Matrix3X2 rotation;

			Identity.Rotate(angle, out rotation);

			Matrix3X2 temporary = nTranslate;

			temporary.Multiply(ref rotation, out temporary);
			temporary.Multiply(ref pTranslate, out temporary);
			temporary.Multiply(ref this, out result);
		}

		/// <summary>
		///   multiples the <see cref="Matrix3X2" /> by the given transformation
		/// </summary>
		/// <param name="right"> the transformation matrix </param>
		/// <param name="result"> stores the result of the multiplication </param>
		public void Multiply(ref Matrix3X2 right, out Matrix3X2 result)
		{
			float m11 = (_11 * right.M11) + (_12 * right.M21);
			float m12 = (_11 * right.M12) + (_12 * right.M22);
			float m21 = (_21 * right.M11) + (_22 * right.M21);
			float m22 = (_21 * right.M12) + (_22 * right.M22);
			float m31 = (_31 * right.M11) + (_32 * right.M21) + right.M31;
			float m32 = (_31 * right.M12) + (_32 * right.M22) + right.M32;

			result = new Matrix3X2(m11, m12, m21, m22, m31, m32);
		}

		public override string ToString()
		{
			return string.Format(
				"M11: {0}, M12: {1}, M21: {2}, M22: {3}, M31: {4}, M32: {5}",
				_11,
				_12,
				_21,
				_22,
				_31,
				_32);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj))
			{
				return false;
			}

			return obj is Matrix3X2 && Equals((Matrix3X2)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = _11.GetHashCode();
				result = (result * 397) ^ _12.GetHashCode();
				result = (result * 397) ^ _21.GetHashCode();
				result = (result * 397) ^ _22.GetHashCode();
				result = (result * 397) ^ _31.GetHashCode();
				result = (result * 397) ^ _32.GetHashCode();
				return result;
			}
		}

		[ContractInvariantMethod]
		private void Invariant()
		{
			Contract.Invariant(Check.IsFinite(_11));
			Contract.Invariant(Check.IsFinite(_12));
			Contract.Invariant(Check.IsFinite(_21));
			Contract.Invariant(Check.IsFinite(_22));
			Contract.Invariant(Check.IsFinite(_31));
			Contract.Invariant(Check.IsFinite(_32));
		}

		/// <summary>
		///   determines whether two instances of <see cref="Matrix3X2" /> are equal
		/// </summary>
		/// <param name="left"> the left operand </param>
		/// <param name="right"> the right operand </param>
		/// <returns> <c>true</c> if <paramref name="left" /> equals <paramref name="right" /> ; otherwise, <c>false</c> </returns>
		public static bool operator ==(Matrix3X2 left, Matrix3X2 right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///   determines whether two instances of <see cref="Matrix3X2" /> are not equal
		/// </summary>
		/// <param name="left"> the left operand </param>
		/// <param name="right"> the right operand </param>
		/// <returns> <c>true</c> if <paramref name="left" /> does not equal <paramref name="right" /> ; otherwise, <c>false</c> </returns>
		public static bool operator !=(Matrix3X2 left, Matrix3X2 right)
		{
			return !left.Equals(right);
		}
	}
}