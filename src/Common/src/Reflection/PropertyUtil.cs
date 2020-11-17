// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Fusonic.Extensions.Common.Reflection
{
    public static class PropertyUtil
    {
        /// <summary> Returns the name of the property used in the given expression. </summary>
        public static string GetName<T, TResult>(Expression<Func<T, TResult>> propertyExpression)
        {
            if (propertyExpression == null)
                throw new ArgumentNullException(nameof(propertyExpression));

            return GetPropertyMember(propertyExpression.Body).Name;
        }

        /// <summary> Returns the name of the property used in the given expression. </summary>
        public static string GetName<T>(Expression<Func<T, object>> propertyExpression)
        {
            if (propertyExpression == null)
                throw new ArgumentNullException(nameof(propertyExpression));

            return GetPropertyMember(propertyExpression.Body).Name;
        }

        /// <summary> Returns the type of the property used in the given expression. </summary>
        public static Type GetType<T>(Expression<Func<T, object>> propertyExpression)
        {
            if (propertyExpression == null)
                throw new ArgumentNullException(nameof(propertyExpression));

            var propertyMember = GetPropertyMember(propertyExpression.Body);
            var propertyType = propertyMember.DeclaringType!.GetProperty(propertyMember.Name)!.PropertyType;
            return propertyType;
        }

        private static MemberInfo GetPropertyMember(Expression expression)
        {
            if (expression is UnaryExpression unaryExpression)
                return GetMember(unaryExpression.Operand);

            return GetMember(expression);

            MemberInfo GetMember(Expression exp)
            {
                if (exp is MemberExpression memberExp && memberExp.Member.MemberType == MemberTypes.Property)
                    return memberExp.Member;

                throw new ArgumentException("Invalid propertyExpression");
            }
        }
    }
}
