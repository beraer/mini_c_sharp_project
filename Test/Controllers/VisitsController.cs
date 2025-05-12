
using Test.Models.DTOs;
using Test.Services;
using Microsoft.AspNetCore.Mvc;


namespace Test.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VisitsController : ControllerBase
{
    private readonly IDbService _dbService;

    public VisitsController(IDbService dbService)
    {
        _dbService = dbService;
    }
    
    [HttpGet("{visitId}")]
     public async Task<IActionResult> GetVisitAsync([FromRoute] int visitId)
     {
         var dto = await _dbService.GetVisitDetailsByIdAsync(visitId);

         if (dto is null)
             return NotFound(); //if it's not exist

         return Ok(dto);
     }

     [HttpPost]
     public async Task<IActionResult> CreateVisitAsync([FromBody] CreateVisitDto dto)
     {
         try
         {
             await _dbService.CreateVisitAsync(dto);
             return Ok();
         }
         catch (Exception ex)
         {
             if(ex.Message.Contains("not exist"))
                 return NotFound();
             
             if(ex.Message.Contains("already exist"))
                 return Conflict();
             
             return StatusCode(500);
         }
     }
}