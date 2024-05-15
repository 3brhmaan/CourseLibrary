using System.ComponentModel.DataAnnotations;
using CourseLibrary.API.Models;

namespace CourseLibrary.API.ValidationAttributes;

public class CourseTitleMustBeDifferentFromDescriptionAttribute
    : ValidationAttribute
{
    public CourseTitleMustBeDifferentFromDescriptionAttribute()
    {
        
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (validationContext.ObjectInstance is not CourseForManipulationDto course)
        {
            throw new Exception(
         $"Attribute " +
                $"{nameof(CourseTitleMustBeDifferentFromDescriptionAttribute)} " +
                $"Must be applied to a " +
                $"{nameof(CourseForManipulationDto)} or derived type");
        }

        if (course.Title == course.Description)
        {
            return new ValidationResult(
                "The provided Description Should be different form the Title",
                new[] { nameof(CourseForManipulationDto) });
        }

        // if title != description
        return ValidationResult.Success;
    }
}
