using CourseLibrary.API.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.Models;

public class CourseForUpdateDto : CourseForManipulationDto
{
    [Required(ErrorMessage = "you should fill out the description")]
    public override string Description
    {
        get => base.Description;
        set => base.Description = value;
    }
}