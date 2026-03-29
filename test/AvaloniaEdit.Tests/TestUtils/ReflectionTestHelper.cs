// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Reflection;

namespace AvaloniaEdit.Tests.TestUtils
{
    /// <summary>
    /// Provides reflection-based helper methods for invoking private and internal members
    /// in unit tests. This utility enables precise testing of private algorithms (e.g.,
    /// match quality scoring, CamelCase matching) without exposing implementation details
    /// in the public API.
    /// </summary>
    /// <remarks>
    /// Think of this class as a "backstage pass" - it lets tests reach behind the curtain
    /// to verify the internal machinery of a class, while the class itself maintains its
    /// proper encapsulation for consumers.
    /// </remarks>
    internal static class ReflectionTestHelper
    {
        private const BindingFlags InstanceNonPublic = BindingFlags.Instance | BindingFlags.NonPublic;
        private const BindingFlags StaticNonPublic = BindingFlags.Static | BindingFlags.NonPublic;
        private const BindingFlags InstanceAll = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags StaticAll = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Invokes a private instance method on the specified target object.
        /// </summary>
        /// <typeparam name="TResult">The expected return type of the method.</typeparam>
        /// <param name="target">The object instance on which to invoke the method.</param>
        /// <param name="methodName">The name of the private method to invoke.</param>
        /// <param name="args">The arguments to pass to the method.</param>
        /// <returns>The return value of the invoked method, cast to <typeparamref name="TResult"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="target"/> or <paramref name="methodName"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the specified method cannot be found on the target's type.
        /// </exception>
        public static TResult InvokePrivateMethod<TResult>(object target, string methodName, params object[] args)
        {
            ArgumentNullException.ThrowIfNull(target);
            ArgumentNullException.ThrowIfNull(methodName);

            // Use parameter types from args to disambiguate overloads.
            // Null arguments cannot be used to infer parameter types reliably, so reject them.
            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] is null)
                    {
                        throw new ArgumentException("Null arguments are not supported for overload resolution. " +
                            "Provide non-null arguments or use a different helper that allows specifying parameter types explicitly.",
                            nameof(args));
                    }
                }
            }

            Type[] parameterTypes = (args is null) ? Type.EmptyTypes : Array.ConvertAll(args, a => a.GetType());
            MethodInfo method = target.GetType().GetMethod(methodName, InstanceNonPublic, binder: null, types: parameterTypes, modifiers: null)
                ?? throw new InvalidOperationException($"Method '{methodName}' not found on type '{target.GetType().FullName}'. " +
                    $"Ensure the method exists and is a non-public instance method with the specified signature.");

            object result = method.Invoke(target, args);

            if (result is null)
            {
                var resultType = typeof(TResult);
                bool isNonNullableValueType = resultType.IsValueType && Nullable.GetUnderlyingType(resultType) is null;

                if (isNonNullableValueType)
                {
                    throw new InvalidOperationException($"Method '{methodName}' on type '{target.GetType().FullName}' returned null, " +
                        $"but the requested result type '{resultType.FullName}' is a non-nullable value type.");
                }

                return default;
            }
            return (TResult)result;
        }

        /// <summary>
        /// Invokes a private instance method that returns void on the specified target object.
        /// </summary>
        /// <param name="target">The object instance on which to invoke the method.</param>
        /// <param name="methodName">The name of the private method to invoke.</param>
        /// <param name="args">The arguments to pass to the method.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="target"/> or <paramref name="methodName"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the specified method cannot be found on the target's type.
        /// </exception>
        public static void InvokePrivateMethod(object target, string methodName, params object[] args)
        {
            ArgumentNullException.ThrowIfNull(target);
            ArgumentNullException.ThrowIfNull(methodName);

            MethodInfo method = target.GetType().GetMethod(methodName, InstanceNonPublic)
                ?? throw new InvalidOperationException($"Method '{methodName}' not found on type '{target.GetType().FullName}'. " +
                    $"Ensure the method exists and is a non-public instance method.");

            method.Invoke(target, args);
        }

        /// <summary>
        /// Invokes a private static method on the specified type.
        /// </summary>
        /// <typeparam name="TResult">The expected return type of the method.</typeparam>
        /// <param name="type">The type that declares the static method.</param>
        /// <param name="methodName">The name of the private static method to invoke.</param>
        /// <param name="args">The arguments to pass to the method.</param>
        /// <returns>The return value of the invoked method, cast to <typeparamref name="TResult"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="type"/> or <paramref name="methodName"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the specified method cannot be found on the given type.
        /// </exception>
        public static TResult InvokePrivateStaticMethod<TResult>(Type type, string methodName, params object[] args)
        {
            ArgumentNullException.ThrowIfNull(type);
            ArgumentNullException.ThrowIfNull(methodName);

            object[] actualArgs = args ?? Array.Empty<object>();
            Type[] parameterTypes = new Type[actualArgs.Length];
            for (int i = 0; i < actualArgs.Length; i++)
            {
                if (actualArgs[i] is null)
                {
                    throw new ArgumentException($"Cannot resolve overload for static method '{methodName}' on type '{type.FullName}' " +
                                                "because one of the arguments is null. Provide a non-null argument or adjust the helper.",
                        nameof(args));
                }

                parameterTypes[i] = actualArgs[i].GetType();
            }

            MethodInfo method = type.GetMethod(methodName, StaticNonPublic, binder: null, types: parameterTypes, modifiers: null)
                ?? throw new InvalidOperationException($"Static method '{methodName}' not found on type '{type.FullName}'. " +
                    $"Ensure the method exists and is a non-public static method with the specified signature.");

            object result = method.Invoke(null, actualArgs);
            if (result is null)
            {
                // Provide a clearer error when a null result is incompatible with TResult.
                Type resultType = typeof(TResult);
                if (resultType.IsValueType && Nullable.GetUnderlyingType(resultType) is null)
                {
                    throw new InvalidOperationException($"Static method '{methodName}' on type '{type.FullName}' returned null, " +
                        $"but the requested result type '{typeof(TResult).FullName}' is a non-nullable value type.");
                }

                return default;
            }
            return (TResult)result;
        }

        /// <summary>
        /// Gets the value of a private instance field on the specified target object.
        /// </summary>
        /// <typeparam name="TResult">The expected type of the field value.</typeparam>
        /// <param name="target">The object instance from which to read the field.</param>
        /// <param name="fieldName">The name of the private field to read.</param>
        /// <returns>The value of the field, cast to <typeparamref name="TResult"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="target"/> or <paramref name="fieldName"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the specified field cannot be found on the target's type.
        /// </exception>
        public static TResult GetPrivateField<TResult>(object target, string fieldName)
        {
            ArgumentNullException.ThrowIfNull(target);
            ArgumentNullException.ThrowIfNull(fieldName);

            FieldInfo field = target.GetType().GetField(fieldName, InstanceAll)
                ?? throw new InvalidOperationException($"Field '{fieldName}' not found on type '{target.GetType().FullName}'.");

            object value = field.GetValue(target);
            if (value is null)
            {
                var resultType = typeof(TResult);
                bool isNonNullableValueType = resultType.IsValueType && Nullable.GetUnderlyingType(resultType) is null;
                if (isNonNullableValueType)
                {
                    throw new InvalidOperationException($"Field '{fieldName}' on type '{target.GetType().FullName}' contains null, " +
                        $"but the requested result type '{resultType.FullName}' is a non-nullable value type.");
                }

                return default;
            }
            return (TResult)value;
        }

        /// <summary>
        /// Sets the value of a private instance field on the specified target object.
        /// </summary>
        /// <param name="target">The object instance on which to set the field.</param>
        /// <param name="fieldName">The name of the private field to set.</param>
        /// <param name="value">The value to assign to the field.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="target"/> or <paramref name="fieldName"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the specified field cannot be found on the target's type.
        /// </exception>
        public static void SetPrivateField(object target, string fieldName, object value)
        {
            ArgumentNullException.ThrowIfNull(target);
            ArgumentNullException.ThrowIfNull(fieldName);

            FieldInfo field = target.GetType().GetField(fieldName, InstanceAll)
                ?? throw new InvalidOperationException($"Field '{fieldName}' not found on type '{target.GetType().FullName}'.");

            field.SetValue(target, value);
        }

        /// <summary>
        /// Gets the value of a private static field on the specified type.
        /// </summary>
        /// <typeparam name="TResult">The expected type of the field value.</typeparam>
        /// <param name="type">The type that declares the static field.</param>
        /// <param name="fieldName">The name of the private static field to read.</param>
        /// <returns>The value of the field, cast to <typeparamref name="TResult"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="type"/> or <paramref name="fieldName"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the specified field cannot be found on the given type.
        /// </exception>
        public static TResult GetPrivateStaticField<TResult>(Type type, string fieldName)
        {
            ArgumentNullException.ThrowIfNull(type);
            ArgumentNullException.ThrowIfNull(fieldName);

            FieldInfo field = type.GetField(fieldName, StaticAll)
                ?? throw new InvalidOperationException($"Static field '{fieldName}' not found on type '{type.FullName}'.");

            object value = field.GetValue(null);
            Type resultType = typeof(TResult);
            if (value is null)
            {
                // For non-nullable value types, a null field value is an invalid state for the requested result type.
                if (resultType.IsValueType && Nullable.GetUnderlyingType(resultType) is null)
                {
                    throw new InvalidOperationException($"Static field '{fieldName}' on type '{type.FullName}' contains null, " +
                        $"which cannot be assigned to non-nullable value type '{resultType.FullName}'.");
                }

                return default!;
            }

            if (!resultType.IsInstanceOfType(value))
            {
                throw new InvalidOperationException($"Static field '{fieldName}' on type '{type.FullName}' has value of type '{value.GetType().FullName}', " +
                    $"which is not compatible with requested result type '{resultType.FullName}'.");
            }

            return (TResult)value;
        }

        /// <summary>
        /// Sets the value of a property on the specified target object, using either the property setter
        /// or the backing field if the setter is not accessible.
        /// </summary>
        /// <param name="target">The object instance on which to set the property.</param>
        /// <param name="propertyName">The name of the property to set.</param>
        /// <param name="value">The value to assign to the property.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="target"/> or <paramref name="propertyName"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when neither the property setter nor backing field can be found or accessed.
        /// </exception>
        public static void SetProperty(object target, string propertyName, object value)
        {
            ArgumentNullException.ThrowIfNull(target);
            ArgumentNullException.ThrowIfNull(propertyName);

            Type targetType = target.GetType();
            PropertyInfo property = targetType.GetProperty(propertyName, InstanceAll);

            // Try to use the property setter if available
            if (property != null && property.CanWrite)
            {
                property.SetValue(target, value);
                return;
            }

            // If property exists but is read-only, try to find the backing field
            if (property != null)
            {
                // Try common backing field patterns
                string camelCaseName = propertyName.Length > 1
                    ? $"{char.ToLowerInvariant(propertyName[0])}{propertyName[1..]}"
                    : char.ToLowerInvariant(propertyName[0]).ToString();

                string[] backingFieldPatterns = new[]
                {
                    $"<{propertyName}>k__BackingField",  // Auto-property backing field
                    $"_{camelCaseName}", // _camelCase
                    $"_{propertyName}", // _PropertyName
                };

                foreach (string fieldName in backingFieldPatterns)
                {
                    FieldInfo field = targetType.GetField(fieldName, InstanceAll);
                    if (field != null)
                    {
                        field.SetValue(target, value);
                        return;
                    }
                }
            }

            throw new InvalidOperationException($"Property '{propertyName}' not found or cannot be set on type '{targetType.FullName}'. " +
                $"Ensure the property exists with a setter or has an accessible backing field.");
        }
    }
}