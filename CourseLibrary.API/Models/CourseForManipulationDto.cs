using System.ComponentModel.DataAnnotations;
using CourseLibrary.API.ValidationAttributes;

namespace CourseLibrary.API.Models;

[CourseTitleMustBeDifferentFromDescription]
public abstract class CourseForManipulationDto 
{
    [Required(ErrorMessage = "you should fill out the title")]
    [MaxLength(100, ErrorMessage = "the title shouldn't have more than 100 character")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1500, ErrorMessage = "the description shouldn't have more than 1500 character")]
    public virtual string Description { get; set; } = string.Empty;
}