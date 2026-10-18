using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Motorvehicle.App.Tools;

namespace Motorvehicle.Api.Controllers;

public record CreateVehicleRequest(
    string? OwnerFirstName,
    string? OwnerLastName,
    string? OwnerOrganisation,
    int ManufacturerKey,
    int ManufacturedYear,
    decimal VehicleWeight
);

[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly VehicleTools _vehicleTools;

    public VehiclesController(VehicleTools vehicleTools)
    {
        _vehicleTools = vehicleTools;
    }

    [HttpGet]
    public async Task<ActionResult<List<VehicleRecord>>> GetAll(
        [FromQuery] string? sortBy = "ownerName",
        [FromQuery] string? sortDirection = "asc")
    {
        List<VehicleRecord> vehicles = await _vehicleTools.GetVehiclesAsync(sortBy, sortDirection);
        return Ok(vehicles);
    }

    [HttpPost]
    public async Task<ActionResult<VehicleRecord>> Create([FromBody] CreateVehicleRequest request)
    {
        (bool success, string? error, VehicleRecord? created) result = await _vehicleTools.CreateVehicleAsync(
            request.OwnerFirstName,
            request.OwnerLastName,
            request.OwnerOrganisation,
            request.ManufacturerKey,
            request.ManufacturedYear,
            request.VehicleWeight);

        if (!result.success)
        {
            return BadRequest(new { message = result.error });
        }

        return CreatedAtAction(nameof(GetAll), new { id = result.created!.VehicleIdentifier }, result.created);
    }
}
