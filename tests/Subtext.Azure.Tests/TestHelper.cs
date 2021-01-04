using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Subtext.Azure.Tests
{
    internal static class TestHelper
    {
        public static void SetProperty<TObject, TValue>(TObject @object, Expression<Func<TObject, TValue>> propertyExpression, TValue value)
        {
            if (!(propertyExpression.Body is MemberExpression memberExpression))
            {
                throw new ArgumentNullException(nameof(memberExpression));
            }

            var propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo == null)
            {
                throw new ArgumentNullException(nameof(propertyInfo));
            }

            propertyInfo.SetValue(@object, value);
        }
    }
}
