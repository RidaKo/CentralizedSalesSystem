using CentralizedSalesSystem.API.Models.Orders.enums;

namespace CentralizedSalesSystem.API.Models.Orders.DTOs.TableDTOs;

public class TablePatchDto
{
    public long? BusinessId { get; set; }
    public string? Name { get; set; }
    public int? Capacity { get; set; }
    public TableStatus? Status { get; set; }
}

