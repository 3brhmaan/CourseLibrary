using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.Models;

public class CourseForCreationDto
{
    [Required(ErrorMessage = "you should fill out the title")]
    [MaxLength(100 , ErrorMessage = "the title shouldn't have more than 100 character")]
    public string Title { get; set; } = string.Empty;
    [MaxLength(1500, ErrorMessage = "the description shouldn't have more than 1500 character")]
    public string Description { get; set; } = string.Empty;
}

