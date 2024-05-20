﻿using AutoMapper;
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
            case ResourceUriType.Current:
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


    public IEnumerable<LinkDto> CreateLinksForAuthors(
        AuthorResourceParameters authorResourceParameters ,
        bool hasNext ,
        bool hasPrevious)
    {
        var links = new List<LinkDto>();

        links.Add(new(
            CreateAuthorsResourceUri(authorResourceParameters , ResourceUriType.Current) ,
            "self" ,
            "GET")
        );

        if(hasNext)
        {
            links.Add(new(
                CreateAuthorsResourceUri(authorResourceParameters, ResourceUriType.NextPage),
                "next-page",
                "GET")
            );
        }

        if(hasPrevious)
        {
            links.Add(new(
                CreateAuthorsResourceUri(authorResourceParameters, ResourceUriType.PreviousPage),
                "previous-page",
                "GET")
            );
        }

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

        var paginationMetaData = new
        {
            totalCount = authorsFromRepo.TotalCount,
            pageSize = authorsFromRepo.PageSize,
            currentPage = authorsFromRepo.CurrentPage,
            totalPages = authorsFromRepo.TotalPages
        };

        Response.Headers.Add(
            "X-Pagination",
            JsonSerializer.Serialize(paginationMetaData));

        var links = CreateLinksForAuthors(authorResourceParameters ,
                                     authorsFromRepo.HasNext , authorsFromRepo.HasPrevious);

        var shapedAuthors = _mapper
                    .Map<IEnumerable<AuthorDto>>(authorsFromRepo)
                    .ShapeData(authorResourceParameters.Fields);

        var shapedAuthorsWithLinks = shapedAuthors.Select(author =>
        {
            var authorAsDictionary = author as IDictionary<string, object?>;
            var authorLinks = CreateLinksForAuthor((Guid)authorAsDictionary["Id"], null);
            authorAsDictionary.Add("links", authorLinks);

            return authorAsDictionary;
        });

        var linkedCollectionResource = new
        {
            value = shapedAuthorsWithLinks,
            links = links
        };

        return Ok(linkedCollectionResource);
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