using CentralizedSalesSystem.API.Data;
using CentralizedSalesSystem.API.Models.Business;
using CentralizedSalesSystem.API.Models.Business.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CentralizedSalesSystem.API.Services;

public class BusinessService : IBusinessService
{
    private readonly CentralizedSalesDbContext _db;

    public BusinessService(CentralizedSalesDbContext db)
    {
        _db = db;
    }

    public async Task<object> GetAllAsync(int page, int limit, string? sortBy, string? sortDirection,
        string? filterByName, string? filterByPhone, string? filterByAddress, string? filterByEmail,
        Currency? filterByCurrency, SubscriptionPlan? filterBySubscriptionPlan, long? filterByOwnerId)
    {
        var query = _db.Businesses.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filterByName))
            query = query.Where(b => b.Name.Contains(filterByName));
        if (!string.IsNullOrWhiteSpace(filterByPhone))
            query = query.Where(b => b.Phone.Contains(filterByPhone));
        if (!string.IsNullOrWhiteSpace(filterByAddress))
            query = query.Where(b => b.Address.Contains(filterByAddress));
        if (!string.IsNullOrWhiteSpace(filterByEmail))
            query = query.Where(b => b.Email.Contains(filterByEmail));
        if (filterByCurrency.HasValue)
            query = query.Where(b => b.Currency == filterByCurrency.Value);
        if (filterBySubscriptionPlan.HasValue)
            query = query.Where(b => b.SubscriptionPlan == filterBySubscriptionPlan.Value);
        if (filterByOwnerId.HasValue)
            query = query.Where(b => b.OwnerId == filterByOwnerId.Value);

        query = 
         (sortBy?.ToLowerInvariant(), sortDirection?.ToLowerInvariant()) switch
        {
            ("name", "desc") => query.OrderByDescending(b => b.Name),
            ("name", _) => query.OrderBy(b => b.Name),
            ("address", "desc") => query.OrderByDescending(b => b.Address),
            ("address", _) => query.OrderBy(b => b.Address),
            ("email", "desc") => query.OrderByDescending(b => b.Email),
            ("email", _) => query.OrderBy(b => b.Email),
            _ => query.OrderBy(b => b.Id)
        };

        var total = await query.CountAsync();
        var entities = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();
        var data = entities.Select(MapToDto).ToList();

        var totalPages = (int)Math.Ceiling(total / (double)limit);
        return new { data, page, limit, total, totalPages };
    }

    public async Task<BusinessResponseDto?> GetByIdAsync(long id)
    {
        var entity = await _db.Businesses.FindAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<BusinessResponseDto> CreateAsync(BusinessCreateDto dto)
    {
        var entity = new Business
        {
            Name = dto.Name,
            Phone = dto.Phone,
            Address = dto.Address,
            Email = dto.Email,
            OwnerId = dto.Owner,
            Currency = dto.Currency,
            SubscriptionPlan = dto.SubscriptionPlan,
            Country = "N/A"
        };

        _db.Businesses.Add(entity);
        await _db.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task<BusinessResponseDto?> PatchAsync(long id, BusinessPatchDto dto)
    {
        var entity = await _db.Businesses.FindAsync(id);
        if (entity == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Name)) entity.Name = dto.Name;
        if (!string.IsNullOrWhiteSpace(dto.Phone)) entity.Phone = dto.Phone;
        if (!string.IsNullOrWhiteSpace(dto.Address)) entity.Address = dto.Address;
        if (!string.IsNullOrWhiteSpace(dto.Email)) entity.Email = dto.Email;
        if (dto.Owner.HasValue) entity.OwnerId = dto.Owner.Value;
        if (dto.Currency.HasValue) entity.Currency = dto.Currency.Value;
        if (dto.SubscriptionPlan.HasValue) entity.SubscriptionPlan = dto.SubscriptionPlan.Value;

        await _db.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var entity = await _db.Businesses.FindAsync(id);
        if (entity == null) return false;
        _db.Businesses.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    private static BusinessResponseDto MapToDto(Business b) => new()
    {
        Id = b.Id,
        Name = b.Name,
        Phone = b.Phone,
        Address = b.Address,
        Email = b.Email,
        Owner = b.OwnerId,
        Currency = b.Currency,
        SubscriptionPlan = b.SubscriptionPlan
    };
}
