using System;
using System.Reflection;

namespace Subtext.Azure.Tests
{
    internal class PrivateObject
    {
        private readonly object _referenceObj;

        public PrivateObject(object referenceObj)
        {
            _referenceObj = referenceObj ?? throw new ArgumentNullException(nameof(referenceObj));
        }

        public object GetField(string fieldName)
        {
            var fieldInfo = GetInternalField(fieldName);

            return fieldInfo.GetValue(_referenceObj);
        }

        public void SetField(string fieldName, object value)
        {
            var fieldInfo = GetInternalField(fieldName);

            fieldInfo.SetValue(_referenceObj, value);
        }

        private FieldInfo GetInternalField(string fieldName)
        {
            var fieldInfo = _referenceObj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo == null)
                throw new ArgumentNullException(nameof(fieldInfo), $"Field with name '{fieldName}' not found");

            return fieldInfo;
        }
    }
}
