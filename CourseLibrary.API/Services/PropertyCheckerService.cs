using System.Reflection;

namespace CourseLibrary.API.Services;

public class PropertyCheckerService : IPropertyCheckerService
{
    public bool TypeHasProperties<T>(string? fields)
    {
        if (string.IsNullOrWhiteSpace(fields))
            return true;

        var fieldsAfterSplit = fields.Split(',');
        foreach (var field in fieldsAfterSplit)
        {
            var propertyName = field.Trim();

            var propertyInfo = typeof(T)
                .GetProperty(propertyName, BindingFlags.Public |
                    BindingFlags.IgnoreCase | BindingFlags.Instance);

            if (propertyInfo is null)
                return false;
        }

        return true;
    }
}