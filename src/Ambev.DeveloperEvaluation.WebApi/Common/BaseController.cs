using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Common;

[Route("api/[controller]")]
[ApiController]
public class BaseController : ControllerBase
{
    protected IActionResult Ok<T>(T data) =>
        base.Ok(new ApiResponseWithData<T> { Data = data, Success = true });

    protected IActionResult Created<T>(string actionName, object routeValues, T data) =>
        base.CreatedAtAction(actionName, routeValues,
            new ApiResponseWithData<T> { Data = data, Success = true });

    protected IActionResult OkPaginated<T>(IEnumerable<T> data, int currentPage, int totalPages, int totalCount) =>
        base.Ok(new PaginatedResponse<T>
        {
            Data = data,
            CurrentPage = currentPage,
            TotalPages = totalPages,
            TotalCount = totalCount,
            Success = true
        });
}
