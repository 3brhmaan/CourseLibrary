using CourseLibrary.API.Services;
using System.Linq.Dynamic.Core;

namespace CourseLibrary.API.Helpers;

public static class IQueryableExtensions
{
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> source,
        string orderBy,
        Dictionary<string, PropertyMappingValue> mappingDictionary)
    {
        if(source is null)
            throw new ArgumentNullException(nameof(source));

        if (mappingDictionary is null)
            throw new ArgumentNullException(nameof(mappingDictionary));

        if (string.IsNullOrWhiteSpace(orderBy))
            return source;

        var orderByAfterSplit = orderBy.Split(',');

        var orderByString = string.Empty;

        foreach (var orderByClause in orderByAfterSplit)
        {
            var trimmedOrderByClause = orderByClause.Trim();

            var isDesc = trimmedOrderByClause.EndsWith(" desc");

            var propertyName = trimmedOrderByClause.Split(' ')[0];

            if (!mappingDictionary.ContainsKey(propertyName))
                throw new ArgumentException($"Key mapping for {propertyName} is missing");

            var propertyMappingValue = mappingDictionary[propertyName];

            if (propertyMappingValue is null)
                throw new ArgumentNullException(nameof(propertyMappingValue));

            if (propertyMappingValue.Revert)
                isDesc = !isDesc;

            foreach (var destinationProperty in propertyMappingValue.DestinationProperties)
            {
                orderByString = orderByString +
                    (string.IsNullOrWhiteSpace(orderByString) ? string.Empty : ", ") +
                    destinationProperty +
                    (isDesc ? " descending" : " ascending");
            }
        }

        return source.OrderBy(orderByString);
    }
}