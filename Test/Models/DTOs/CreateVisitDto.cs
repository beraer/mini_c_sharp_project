using System.ComponentModel.DataAnnotations;

namespace Test.Models.DTOs;


public class CreateVisitDto
{
    [Required]
    public int VisitId { get; set; }
    
    [Required]
    public int ClientId { get; set; }
    
    [Required]
    [StringLength(14)]
    public string MechanicLicenceNumber { get; set; }
    
    [Required]
    public List<ServiceDto> Services { get; set; }
}

public class ServiceDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }
    
    [Required]
    [Range(typeof(decimal), "0.01", "99999999.99")]
    public decimal ServiceFee { get; set; }
}