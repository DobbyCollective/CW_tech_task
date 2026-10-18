using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Motorvehicle.App.Compositions;
using Motorvehicle.App.Tools;

namespace Motorvehicle.Api.Controllers;

public record UpsertCategoryRequest(
    string CategoryName,
    string IconIdentifier,
    decimal MinimumKg,
    decimal? MaximumKg
);

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly CategoryTools _categoryTools;

    public CategoriesController(CategoryTools categoryTools)
    {
        _categoryTools = categoryTools;
    }

    [HttpGet]
    public async Task<ActionResult<List<WeightCategory>>> GetAll()
    {
        List<WeightCategory> categories = await _categoryTools.GetAllOrderedAsync();
        return Ok(categories);
    }

    [HttpPost]
    public async Task<ActionResult<WeightCategory>> Create([FromBody] UpsertCategoryRequest request)
    {
        (bool success, string? error, WeightCategory? created) result = await _categoryTools.CreateCategoryAsync(
            request.CategoryName,
            request.IconIdentifier,
            request.MinimumKg,
            request.MaximumKg);

        if (!result.success)
        {
            return BadRequest(new { message = result.error });
        }

        return CreatedAtAction(nameof(GetAll), new { id = result.created!.WeightKey }, result.created);
    }

    [HttpPut("{weightKey:int}")]
    public async Task<ActionResult<WeightCategory>> Update(int weightKey, [FromBody] UpsertCategoryRequest request)
    {
        (bool success, string? error, WeightCategory? updated) result = await _categoryTools.UpdateCategoryAsync(
            weightKey,
            request.CategoryName,
            request.IconIdentifier,
            request.MinimumKg,
            request.MaximumKg);

        if (!result.success)
        {
            bool isNotFound = result.error != null &&
                              result.error.Contains("not found", StringComparison.OrdinalIgnoreCase);

            return isNotFound
                ? NotFound(new { message = result.error })
                : BadRequest(new { message = result.error });
        }

        return Ok(result.updated);
    }

    [HttpDelete("{weightKey:int}")]
    public async Task<IActionResult> Delete(int weightKey)
    {
        (bool success, string? error) result = await _categoryTools.DeleteCategoryAsync(weightKey);

        if (!result.success)
        {
            bool isNotFound = result.error != null &&
                              result.error.Contains("not found", StringComparison.OrdinalIgnoreCase);

            return isNotFound
                ? NotFound(new { message = result.error })
                : BadRequest(new { message = result.error });
        }

        return NoContent();
    }
}
