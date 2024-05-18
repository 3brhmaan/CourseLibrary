namespace CourseLibrary.API.Services;

public interface IPropertyCheckerService
{
    bool TypeHasProperty<T>(string? fields);
}