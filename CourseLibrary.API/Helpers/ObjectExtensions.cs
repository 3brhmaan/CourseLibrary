﻿using System.Dynamic;
using System.Reflection;

namespace CourseLibrary.API.Helpers;

public static class ObjectExtensions
{
    public static ExpandoObject ShapeData<TSource>(
        this TSource source,
        string? fields)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));

        var dataShapedObject = new ExpandoObject();

        if (string.IsNullOrWhiteSpace(fields))
        {
            var propertyInfos = typeof(TSource)
                .GetProperties(BindingFlags.IgnoreCase |
                    BindingFlags.Public | BindingFlags.Instance);

            foreach (var propertyInfo in propertyInfos)
            {
                var propertyValue = propertyInfo
                    .GetValue(source);

                ((IDictionary<string, object>)dataShapedObject)
                    .Add(propertyInfo.Name, propertyValue);
            }

            return dataShapedObject;
        }


        var fieldsAfterSplit = fields.Split(',');

        foreach (var field in fieldsAfterSplit)
        {
            var propertyName = field.Trim();

            var propertyInfo = typeof(TSource)
                .GetProperty(propertyName, BindingFlags.IgnoreCase |
                    BindingFlags.Public | BindingFlags.Instance);

            if (propertyInfo is null)
                throw new Exception(
                $"Property {propertyName} wasn't found on" +
                       $" {typeof(TSource)}");

            var propertyValue = propertyInfo.GetValue(source);

            ((IDictionary<string, object>)dataShapedObject)
                .Add(propertyInfo.Name, propertyValue);
        }

        return dataShapedObject;
    }
}