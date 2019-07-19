using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace KoiVM.Runtime.Execution.Internal {
	internal static unsafe class TypedReferenceHelpers {
		static Hashtable castHelpers = new Hashtable();
		static Hashtable makeHelpers = new Hashtable();
		static Hashtable unboxHelpers = new Hashtable();
		static Hashtable setHelpers = new Hashtable();
		static Hashtable fieldAddrHelpers = new Hashtable();

		delegate void Cast(TypedRefPtr typedRef);

		delegate void Make(void* ptr, TypedRefPtr typedRef);

		delegate void Unbox(object box, TypedRefPtr typedRef);

		delegate void Set(object value, TypedRefPtr typedRef);

		delegate void FieldAdr(TypedRefPtr value, TypedRefPtr typedRef);

		public static void CastTypedRef(TypedRefPtr typedRef, Type targetType) {
			Type sourceType = TypedReference.GetTargetType(*(TypedReference*)typedRef);
			var key = new KeyValuePair<Type, Type>(sourceType, targetType);

			var helper = castHelpers[key];
			if (helper == null) {
				lock (castHelpers) {
					helper = castHelpers[key];
					if (helper == null) {
						helper = BuildCastHelper(sourceType, targetType);
						castHelpers[key] = helper;
					}
				}
			}
			((Cast)helper)(typedRef);
		}

		public static void MakeTypedRef(void* ptr, TypedRefPtr typedRef, Type targetType) {
			var helper = makeHelpers[targetType];
			if (helper == null) {
				lock (makeHelpers) {
					helper = makeHelpers[targetType];
					if (helper == null) {
						helper = BuildMakeHelper(targetType);
						makeHelpers[targetType] = helper;
					}
				}
			}
			((Make)helper)(ptr, typedRef);
		}

		public static void UnboxTypedRef(object box, TypedRefPtr typedRef) {
			UnboxTypedRef(box, typedRef, box.GetType());
			if (box is IValueTypeBox)
				CastTypedRef(typedRef, ((IValueTypeBox)box).GetValueType());
		}

		public static void UnboxTypedRef(object box, TypedRefPtr typedRef, Type boxType) {
			var helper = unboxHelpers[boxType];
			if (helper == null) {
				lock (unboxHelpers) {
					helper = unboxHelpers[boxType];
					if (helper == null) {
						helper = BuildUnboxHelper(boxType);
						unboxHelpers[boxType] = helper;
					}
				}
			}
			((Unbox)helper)(box, typedRef);
		}

		public static void SetTypedRef(object value, TypedRefPtr typedRef) {
			Type type = TypedReference.GetTargetType(*(TypedReference*)typedRef);
			var helper = setHelpers[type];
			if (helper == null) {
				lock (setHelpers) {
					helper = setHelpers[type];
					if (helper == null) {
						helper = BuildSetHelper(type);
						setHelpers[type] = helper;
					}
				}
			}
			((Set)helper)(value, typedRef);
		}

		public static void GetFieldAddr(VMContext context, object obj, FieldInfo field, TypedRefPtr typedRef) {
			var helper = fieldAddrHelpers[field];
			if (helper == null) {
				lock (fieldAddrHelpers) {
					helper = fieldAddrHelpers[field];
					if (helper == null) {
						helper = BuildAddrHelper(field);
						fieldAddrHelpers[field] = helper;
					}
				}
			}
			TypedReference objRef;
			if (obj == null) {
				objRef = default(TypedReference);
			}
			else if (obj is IReference)
				((IReference)obj).ToTypedReference(context, &objRef, field.DeclaringType);
			else {
				objRef = __makeref(obj);
				CastTypedRef(&objRef, obj.GetType());
			}

			((FieldAdr)helper)(&objRef, typedRef);
		}

		static FieldInfo typedPtrField = typeof(TypedRefPtr).GetFields()[0];

		static Cast BuildCastHelper(Type sourceType, Type targetType) {
			var dm = new DynamicMethod("", typeof(void), new[] { typeof(TypedRefPtr) }, Unverifier.Module, true);
			var gen = dm.GetILGenerator();

			gen.Emit(System.Reflection.Emit.OpCodes.Ldarga, 0);
			gen.Emit(System.Reflection.Emit.OpCodes.Ldfld, typedPtrField);
			gen.Emit(System.Reflection.Emit.OpCodes.Dup);
			gen.Emit(System.Reflection.Emit.OpCodes.Ldobj, typeof(TypedReference));
			gen.Emit(System.Reflection.Emit.OpCodes.Refanyval, sourceType);
			gen.Emit(System.Reflection.Emit.OpCodes.Mkrefany, targetType);
			gen.Emit(System.Reflection.Emit.OpCodes.Stobj, typeof(TypedReference));
			gen.Emit(System.Reflection.Emit.OpCodes.Ret);
			return (Cast)dm.CreateDelegate(typeof(Cast));
		}

		static Make BuildMakeHelper(Type targetType) {
			var dm = new DynamicMethod("", typeof(void), new[] { typeof(void*), typeof(TypedRefPtr) }, Unverifier.Module, true);
			var gen = dm.GetILGenerator();

			gen.Emit(System.Reflection.Emit.OpCodes.Ldarga, 1);
			gen.Emit(System.Reflection.Emit.OpCodes.Ldfld, typedPtrField);
			gen.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
			gen.Emit(System.Reflection.Emit.OpCodes.Mkrefany, targetType);
			gen.Emit(System.Reflection.Emit.OpCodes.Stobj, typeof(TypedReference));
			gen.Emit(System.Reflection.Emit.OpCodes.Ret);

			return (Make)dm.CreateDelegate(typeof(Make));
		}

		static Unbox BuildUnboxHelper(Type boxType) {
			var dm = new DynamicMethod("", typeof(void), new[] { typeof(object), typeof(TypedRefPtr) }, Unverifier.Module, true);
			var gen = dm.GetILGenerator();

			gen.Emit(System.Reflection.Emit.OpCodes.Ldarga, 1);
			gen.Emit(System.Reflection.Emit.OpCodes.Ldfld, typedPtrField);
			gen.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
			gen.Emit(System.Reflection.Emit.OpCodes.Unbox, boxType); // Unbox pointer is readonly? Never mind...
			gen.Emit(System.Reflection.Emit.OpCodes.Mkrefany, boxType);
			gen.Emit(System.Reflection.Emit.OpCodes.Stobj, typeof(TypedReference));
			gen.Emit(System.Reflection.Emit.OpCodes.Ret);

			return (Unbox)dm.CreateDelegate(typeof(Unbox));
		}

		static Set BuildSetHelper(Type refType) {
			var dm = new DynamicMethod("", typeof(void), new[] { typeof(object), typeof(TypedRefPtr) }, Unverifier.Module, true);
			var gen = dm.GetILGenerator();

			gen.Emit(System.Reflection.Emit.OpCodes.Ldarga, 1);
			gen.Emit(System.Reflection.Emit.OpCodes.Ldfld, typedPtrField);
			gen.Emit(System.Reflection.Emit.OpCodes.Ldobj, typeof(TypedReference));
			gen.Emit(System.Reflection.Emit.OpCodes.Refanyval, refType);
			gen.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
			gen.Emit(System.Reflection.Emit.OpCodes.Unbox_Any, refType);
			gen.Emit(System.Reflection.Emit.OpCodes.Stobj, refType);
			gen.Emit(System.Reflection.Emit.OpCodes.Ret);

			return (Set)dm.CreateDelegate(typeof(Set));
		}

		static FieldAdr BuildAddrHelper(FieldInfo field) {
			var dm = new DynamicMethod("", typeof(void), new[] { typeof(TypedRefPtr), typeof(TypedRefPtr) }, Unverifier.Module, true);
			var gen = dm.GetILGenerator();

			if (field.IsStatic) {
				gen.Emit(System.Reflection.Emit.OpCodes.Ldarga, 1);
				gen.Emit(System.Reflection.Emit.OpCodes.Ldfld, typedPtrField);

				gen.Emit(System.Reflection.Emit.OpCodes.Ldsflda, field);
				gen.Emit(System.Reflection.Emit.OpCodes.Mkrefany, field.FieldType);
				gen.Emit(System.Reflection.Emit.OpCodes.Stobj, typeof(TypedReference));
				gen.Emit(System.Reflection.Emit.OpCodes.Ret);
			}
			else {
				gen.Emit(System.Reflection.Emit.OpCodes.Ldarga, 1);
				gen.Emit(System.Reflection.Emit.OpCodes.Ldfld, typedPtrField);

				gen.Emit(System.Reflection.Emit.OpCodes.Ldarga, 0);
				gen.Emit(System.Reflection.Emit.OpCodes.Ldfld, typedPtrField);
				gen.Emit(System.Reflection.Emit.OpCodes.Ldobj, typeof(TypedReference));
				gen.Emit(System.Reflection.Emit.OpCodes.Refanyval, field.DeclaringType);
				if (!field.DeclaringType.IsValueType)
					gen.Emit(System.Reflection.Emit.OpCodes.Ldobj, field.DeclaringType);

				gen.Emit(System.Reflection.Emit.OpCodes.Ldflda, field);
				gen.Emit(System.Reflection.Emit.OpCodes.Mkrefany, field.FieldType);
				gen.Emit(System.Reflection.Emit.OpCodes.Stobj, typeof(TypedReference));
				gen.Emit(System.Reflection.Emit.OpCodes.Ret);
			}

			return (FieldAdr)dm.CreateDelegate(typeof(FieldAdr));
		}
	}
}