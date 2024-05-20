using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.ResourceParameters;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CourseLibrary.API.Controllers;

[ApiController]
[Route("api/authors")]
public class AuthorsController : ControllerBase
{
    private readonly ICourseLibraryRepository _courseLibraryRepository;
    private readonly IMapper _mapper;
    private readonly IPropertyMappingService _propertyMappingService;
    private readonly IPropertyCheckerService _propertyCheckerService;
    private readonly ProblemDetailsFactory _problemDetailsFactory;

    public AuthorsController(
        ICourseLibraryRepository courseLibraryRepository,
        IMapper mapper,
        IPropertyMappingService propertyMappingService ,
        IPropertyCheckerService propertyCheckerService ,
        ProblemDetailsFactory problemDetailsFactory)
    {
        _courseLibraryRepository = courseLibraryRepository ??
                                   throw new ArgumentNullException(nameof(courseLibraryRepository));
        _mapper = mapper ??
                  throw new ArgumentNullException(nameof(mapper));
        _propertyMappingService = propertyMappingService;
        _propertyCheckerService = propertyCheckerService;
        _problemDetailsFactory = problemDetailsFactory;
    }


    public string? CreateAuthorsResourceUri(
        AuthorResourceParameters authorResourceParameters,
        ResourceUriType type)
    {
        switch (type)
        {
            case ResourceUriType.PreviousPage:
                return Url.Link("GetAuthors", new
                {
                    pageNumber = authorResourceParameters.PageNumber - 1,
                    pageSize = authorResourceParameters.PageSize,
                    mainCategory = authorResourceParameters.MainCategory,
                    searchQuery = authorResourceParameters.SearchQuery,
                    orderBy = authorResourceParameters.OrderBy ,
                    fields = authorResourceParameters.Fields
                });
                break;
            case ResourceUriType.NextPage:
                return Url.Link("GetAuthors", new
                {
                    pageNumber = authorResourceParameters.PageNumber + 1,
                    pageSize = authorResourceParameters.PageSize,
                    mainCategory = authorResourceParameters.MainCategory,
                    searchQuery = authorResourceParameters.SearchQuery,
                    orderBy = authorResourceParameters.OrderBy,
                    fields = authorResourceParameters.Fields
                });
                break;
            default:
                return Url.Link("GetAuthors", new
                {
                    pageNumber = authorResourceParameters.PageNumber,
                    pageSize = authorResourceParameters.PageSize,
                    mainCategory = authorResourceParameters.MainCategory,
                    searchQuery = authorResourceParameters.SearchQuery,
                    orderBy = authorResourceParameters.OrderBy,
                    fields = authorResourceParameters.Fields
                });
                break;
        }
    }


    public IEnumerable<LinkDto> CreateLinksForAuthor(
        Guid authorId,
        string? fields)
    {
        var links = new List<LinkDto>();

        if (string.IsNullOrWhiteSpace(fields))
        {
            links.Add(
                new LinkDto(
                    Url.Link("GetAuthor" , new {authorId}) ,
                    "self" ,
                    "GET")
            );
        }
        else
        {
            links.Add(
                new LinkDto(
                    Url.Link("GetAuthor", new { authorId , fields}),
                    "self",
                    "GET")
            );
        }

        links.Add(
            new LinkDto(
                Url.Link("CreateCourseForAuthor", new { authorId }),
                "Create_Course_For_Author",
                "POST")
        );

        links.Add(
            new LinkDto(
                Url.Link("GetCoursesForAuthor", new { authorId }),
                "Get_Course_For_Author",
                "GET")
        );

        return links;
    }


    [HttpGet(Name = "GetAuthors")]
    [HttpHead]
    public async Task<IActionResult> GetAuthors(
        [FromQuery] AuthorResourceParameters authorResourceParameters)
    {
        if (!_propertyMappingService
                .ValidMappingExistsFor<AuthorDto, Author>(
                    authorResourceParameters.OrderBy))
        {
            return BadRequest();
        }

        if (!_propertyCheckerService.TypeHasProperty<AuthorDto>(
                authorResourceParameters.Fields))
        {
            return BadRequest(
            _problemDetailsFactory.CreateProblemDetails(
                HttpContext,
                statusCode: 400,
                detail: $"not all requested data shaping fields exist on " +
                    $"the resource: {authorResourceParameters.Fields}"));
        }


        var authorsFromRepo = await _courseLibraryRepository
            .GetAuthorsAsync(authorResourceParameters);

        var previousPageLink = authorsFromRepo.HasPrevious
            ? CreateAuthorsResourceUri(
                authorResourceParameters,
                ResourceUriType.PreviousPage)
            : null;

        var nextPageLink = authorsFromRepo.HasNext
            ? CreateAuthorsResourceUri(
                authorResourceParameters,
                ResourceUriType.NextPage)
            : null;

        var paginationMetaData = new
        {
            totalCount = authorsFromRepo.TotalCount,
            pageSize = authorsFromRepo.PageSize,
            currentPage = authorsFromRepo.CurrentPage,
            totalPages = authorsFromRepo.TotalPages,
            previousPageLink,
            nextPageLink
        };

        Response.Headers.Add(
            "X-Pagination",
            JsonSerializer.Serialize(paginationMetaData));


        return Ok(
            _mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo)
                .ShapeData(authorResourceParameters.Fields)
        );
    }


    [HttpGet("{authorId}", Name = "GetAuthor")]
    public async Task<IActionResult> GetAuthor(
        Guid authorId ,
        string? fields)
    {
        if (!_propertyCheckerService.TypeHasProperty<AuthorDto>(
                fields))
        {
            return BadRequest(
            _problemDetailsFactory.CreateProblemDetails(
                HttpContext,
                statusCode: 400,
                detail: $"not all requested data shaping fields exist on " +
                    $"the resource: {fields}"));
        }

        var authorFromRepo = await _courseLibraryRepository.GetAuthorAsync(authorId);

        if (authorFromRepo == null) return NotFound();

        var links = CreateLinksForAuthor(authorId, fields);

        var linkedResourceToBeReturn = _mapper.Map<AuthorDto>(authorFromRepo)
            .ShapeData(fields) as IDictionary<string, object?>;

        linkedResourceToBeReturn.Add("links" , links);

        return Ok(linkedResourceToBeReturn);
    }

    
    [HttpPost]
    public async Task<ActionResult<AuthorDto>> CreateAuthor(AuthorForCreationDto author)
    {
        var authorEntity = _mapper.Map<Author>(author);

        _courseLibraryRepository.AddAuthor(authorEntity);
        await _courseLibraryRepository.SaveAsync();

        var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);

        var links = CreateLinksForAuthor(authorToReturn.Id, null);
        
        var linkedResourceToReturn = authorToReturn.ShapeData(null) 
            as IDictionary<string, object?>;

        linkedResourceToReturn.Add("links" , links);

        return CreatedAtRoute("GetAuthor",
            new { authorId = linkedResourceToReturn["Id"] },
            linkedResourceToReturn);
    }


    [HttpOptions]
    public IActionResult GetAuthorOptions()
    {
        Response.Headers.Add("Allow", "GET,HEAD,POST,OPTIONS");
        return Ok();
    }
}