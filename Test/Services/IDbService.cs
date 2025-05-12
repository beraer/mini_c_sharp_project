using Test.Models.DTOs;

namespace Test.Services;

public interface IDbService
{
    Task<VisitResponseDto> GetVisitDetailsByIdAsync(int VisitId);
    public Task CreateVisitAsync(CreateVisitDto dto);
}