using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.ResourceParameters;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
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

    public AuthorsController(
        ICourseLibraryRepository courseLibraryRepository,
        IMapper mapper,
        IPropertyMappingService propertyMappingService)
    {
        _courseLibraryRepository = courseLibraryRepository ??
                                   throw new ArgumentNullException(nameof(courseLibraryRepository));
        _mapper = mapper ??
                  throw new ArgumentNullException(nameof(mapper));
        _propertyMappingService = propertyMappingService;
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


    [HttpGet(Name = "GetAuthors")]
    [HttpHead]
    public async Task<IActionResult> GetAuthors(
        [FromQuery] AuthorResourceParameters authorResourceParameters)
    {
        if (!_propertyMappingService
                .ValidMappingExistsFor<AuthorDto, Author>(
                    authorResourceParameters.OrderBy))
            return BadRequest();

        // get authors from repo
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

        // return them
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
        // get author from repo
        var authorFromRepo = await _courseLibraryRepository.GetAuthorAsync(authorId);

        if (authorFromRepo == null) return NotFound();

        // return author
        return Ok(_mapper.Map<AuthorDto>(authorFromRepo).ShapeData(fields));
    }


    [HttpPost]
    public async Task<ActionResult<AuthorDto>> CreateAuthor(AuthorForCreationDto author)
    {
        var authorEntity = _mapper.Map<Author>(author);

        _courseLibraryRepository.AddAuthor(authorEntity);
        await _courseLibraryRepository.SaveAsync();

        var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);

        return CreatedAtRoute("GetAuthor",
            new { authorId = authorToReturn.Id },
            authorToReturn);
    }


    [HttpOptions]
    public IActionResult GetAuthorOptions()
    {
        Response.Headers.Add("Allow", "GET,HEAD,POST,OPTIONS");
        return Ok();
    }
}