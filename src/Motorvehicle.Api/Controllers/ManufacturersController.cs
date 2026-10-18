using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Motorvehicle.App.Data;

namespace Motorvehicle.Api.Controllers;

public record ManufacturerRecord(int ManufacturerKey, string ManufacturerName);

[ApiController]
[Route("api/[controller]")]
public class ManufacturersController : ControllerBase
{
    private readonly AppDbContext _db;

    public ManufacturersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<ManufacturerRecord>>> GetAll()
    {
        List<ManufacturerRecord> manufacturers = await _db.Manufacturers
            .AsNoTracking()
            .OrderBy(m => m.ManufacturerName)
            .Select(m => new ManufacturerRecord(m.ManufacturerKey, m.ManufacturerName))
            .ToListAsync();

        return Ok(manufacturers);
    }
}
