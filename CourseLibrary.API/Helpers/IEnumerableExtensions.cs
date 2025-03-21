﻿using System.Dynamic;
using System.Reflection;

namespace CourseLibrary.API.Helpers;

public static class IEnumerableExtensions
{
    public static IEnumerable<ExpandoObject> ShapeData<TSource>(
        this IEnumerable<TSource> source,
        string? fields)
    {
        if(source is null)
            throw new ArgumentNullException(nameof(source));

        var propertyInfoList = new List<PropertyInfo>();

        if (string.IsNullOrWhiteSpace(fields))
        {
            var propertyInfos = typeof(TSource)
                .GetProperties(BindingFlags.IgnoreCase | 
                    BindingFlags.Public | BindingFlags.Instance);

            propertyInfoList.AddRange(propertyInfos);
        }
        else
        {
            var fieldsAfterSplit = fields.Split(',');

            foreach (var field in fieldsAfterSplit)
            {
                var propertyName = field.Trim();

                var propertyInfo = typeof(TSource)
                    .GetProperty(propertyName, BindingFlags.IgnoreCase | 
                        BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo is not null)
                    propertyInfoList.Add(propertyInfo);
                else
                    throw new Exception($"Property {propertyName} wasn't found on" +
                                        $" {typeof(TSource)}");
            }
        }

        var expandoObjectList = new List<ExpandoObject>();

        foreach (TSource sourceObject in source)
        {
            var dataShapedObject = new ExpandoObject();

            foreach (var propertyInfo in propertyInfoList)
            {
                var propertyValue = propertyInfo.GetValue(sourceObject);

                ((IDictionary<string , object>)dataShapedObject)
                    .Add(propertyInfo.Name , propertyValue);
            }

            expandoObjectList.Add(dataShapedObject);
        }

        return expandoObjectList;
    }
}