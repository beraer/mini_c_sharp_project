using System.ComponentModel.DataAnnotations;

namespace Test.Models.DTOs;

public class VisitResponseDto
{
    [Required]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }
    
    [Required]
    public ClientDto Client { get; set; }
    
    [Required]
    public MechanicDto Mechanic { get; set; }

    [Required]
    public List<VisitServicesDto> VisitServices { get; set; }
}

public class ClientDto
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; }
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; }
    
    [Required]
    [DataType(DataType.Date)]
    public DateTime BirthDate { get; set; }
}

public class MechanicDto
{
    [Required]
    public int MechanicId { get; set; }
    
    [Required]
    [StringLength(14)]
    public string LicenceNumber { get; set; }
}

public class VisitServicesDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }
    
    [Required]
    [Range(typeof(decimal), "0.01", "99999999.99")]
    public decimal ServiceFee { get; set; }
}