// Copyright (c) 2012, Joshua Burke
// All rights reserved.
// 
// See LICENSE for more information.

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Frost
{
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

		[ContractInvariantMethod] private void Invariant()
		{
			Contract.Invariant(Check.IsFinite(this._11));
			Contract.Invariant(Check.IsFinite(this._12));
			Contract.Invariant(Check.IsFinite(this._21));
			Contract.Invariant(Check.IsFinite(this._22));
			Contract.Invariant(Check.IsFinite(this._31));
			Contract.Invariant(Check.IsFinite(this._32));
		}

		public Matrix3X2(
			float m11, float m12, float m21, float m22, float m31, float m32)
		{
			Contract.Requires(Check.IsFinite(m11));
			Contract.Requires(Check.IsFinite(m12));
			Contract.Requires(Check.IsFinite(m21));
			Contract.Requires(Check.IsFinite(m22));
			Contract.Requires(Check.IsFinite(m31));
			Contract.Requires(Check.IsFinite(m32));

			this._11 = m11;
			this._12 = m12;
			this._21 = m21;
			this._22 = m22;
			this._31 = m31;
			this._32 = m32;

			Contract.Assert(M11.Equals(m11));
			Contract.Assert(M12.Equals(m12));
			Contract.Assert(M21.Equals(m21));
			Contract.Assert(M22.Equals(m22));
			Contract.Assert(M31.Equals(m31));
			Contract.Assert(M32.Equals(m32));
		}

		public static Matrix3X2 Identity
		{
			get { return _Identity; }
		}

		public float M32
		{
			get
			{
				Contract.Ensures(Check.IsFinite(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(this._32));

				return this._32;
			}
		}

		public float M31
		{
			get
			{
				Contract.Ensures(Check.IsFinite(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(this._31));

				return this._31;
			}
		}

		public float M22
		{
			get
			{
				Contract.Ensures(Check.IsFinite(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(this._22));

				return this._22;
			}
		}

		public float M21
		{
			get
			{
				Contract.Ensures(Check.IsFinite(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(this._21));

				return this._21;
			}
		}

		public float M12
		{
			get
			{
				Contract.Ensures(Check.IsFinite(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(this._12));

				return this._12;
			}
		}

		public float M11
		{
			get
			{
				Contract.Ensures(Check.IsFinite(Contract.Result<float>()));
				Contract.Ensures(Contract.Result<float>().Equals(this._11));

				return this._11;
			}
		}

		public bool IsIdentity
		{
			get
			{
				return this._11.Equals(1.0f) && this._12.Equals(0.0f) &&
				       this._21.Equals(0.0f) && this._22.Equals(1.0f) &&
				       this._31.Equals(0.0f) && this._32.Equals(0.0f);
			}
		}

		public bool Equals(Matrix3X2 other)
		{
			return other._11.Equals(this._11) && other._12.Equals(this._12) &&
			       other._21.Equals(this._21) && other._22.Equals(this._22) &&
			       other._31.Equals(this._31) && other._32.Equals(this._32);
		}

		public void Translate(
			float width, float height, out Matrix3X2 result)
		{
			Contract.Requires(Check.IsFinite(width));
			Contract.Requires(Check.IsFinite(height));

			result = new Matrix3X2(1.0f, 0.0f, 0.0f, 1.0f, width, height);

			result.Multiply(ref this, out result);
		}

		public void Skew(float angleX, float angleY, out Matrix3X2 result)
		{
			Contract.Requires(Check.IsDegrees(angleX));
			Contract.Requires(Check.IsDegrees(angleY));

			double radiansX = (Math.PI * angleX) / 180.0;
			double radiansY = (Math.PI * angleY) / 180.0;

			result = new Matrix3X2(
				1.0f,
				Convert.ToSingle(Math.Tan(radiansX)),
				Convert.ToSingle(Math.Tan(radiansY)),
				1.0f,
				0.0f,
				0.0f);

			result.Multiply(ref this, out result);
		}

		public void Scale(float width, float height, out Matrix3X2 result)
		{
			Contract.Requires(Check.IsPositive(width));
			Contract.Requires(Check.IsPositive(height));

			result = new Matrix3X2(width, 0.0f, 0.0f, height, 0.0f, 0.0f);

			result.Multiply(ref this, out result);
		}

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

			result = new Matrix3X2(
				width, 0.0f, 0.0f, height, translationX, translationY);

			result.Multiply(ref this, out result);
		}

		public void Rotate(float angle, out Matrix3X2 result)
		{
			Contract.Requires(Check.IsDegrees(angle));

			double radians = (Math.PI * angle) / 180.0;

			float rcos = Convert.ToSingle(Math.Cos(radians));
			float rsin = Convert.ToSingle(Math.Sin(radians));

			result = new Matrix3X2(rcos, rsin, -rsin, rcos, 0.0f, 0.0f);

			result.Multiply(ref this, out result);
		}

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

			result = nTranslate;

			result.Multiply(ref rotation, out result);
			result.Multiply(ref pTranslate, out result);
			result.Multiply(ref this, out result);
		}

		public void Multiply(ref Matrix3X2 right, out Matrix3X2 result)
		{
			float m11 = (this._11 * right.M11) + (this._12 * right.M21);
			float m12 = (this._11 * right.M12) + (this._12 * right.M22);
			float m21 = (this._21 * right.M11) + (this._22 * right.M21);
			float m22 = (this._21 * right.M12) + (this._22 * right.M22);
			float m31 = (this._31 * right.M11) + (this._32 * right.M21) +
			            right.M31;
			float m32 = (this._31 * right.M12) + (this._32 * right.M22) +
			            right.M32;

			result = new Matrix3X2(m11, m12, m21, m22, m31, m32);
		}

		public override string ToString()
		{
			return
				string.Format(
					"M11: {0}, M12: {1}, M21: {2}, M22: {3}, M31: {4}, M32: {5}",
					this._11,
					this._12,
					this._21,
					this._22,
					this._31,
					this._32);
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
				int result = this._11.GetHashCode();
				result = (result * 397) ^ this._12.GetHashCode();
				result = (result * 397) ^ this._21.GetHashCode();
				result = (result * 397) ^ this._22.GetHashCode();
				result = (result * 397) ^ this._31.GetHashCode();
				result = (result * 397) ^ this._32.GetHashCode();
				return result;
			}
		}

		public static bool operator ==(Matrix3X2 left, Matrix3X2 right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Matrix3X2 left, Matrix3X2 right)
		{
			return !left.Equals(right);
		}

#if(UNIT_TESTING)
		[Fact] internal static void Test0()
		{
			Assert.Equal(0, new Matrix3X2(0, 1, 2, 3, 4, 5).M11);
			Assert.Equal(1, new Matrix3X2(0, 1, 2, 3, 4, 5).M12);
			Assert.Equal(2, new Matrix3X2(0, 1, 2, 3, 4, 5).M21);
			Assert.Equal(3, new Matrix3X2(0, 1, 2, 3, 4, 5).M22);
			Assert.Equal(4, new Matrix3X2(0, 1, 2, 3, 4, 5).M31);
			Assert.Equal(5, new Matrix3X2(0, 1, 2, 3, 4, 5).M32);

			Assert.True(Identity.IsIdentity);

			Matrix3X2 matrix1;
			Matrix3X2 matrix2;

			Identity.Rotate(50, out matrix1);
			Identity.Rotate(50, 0, 0, out matrix2);

			Assert.Equal(matrix1, matrix2);

			Identity.Scale(2, 2, out matrix1);
			Identity.Scale(2, 2, 0, 0, out matrix2);

			Assert.Equal(matrix1, matrix2);

			Identity.Skew(50, 50, out matrix1);

			Assert.TestObject(Identity, matrix1);
		}
#endif
	}
}