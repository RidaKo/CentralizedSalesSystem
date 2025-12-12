using CentralizedSalesSystem.API.Data;
using CentralizedSalesSystem.API.Models;
using CentralizedSalesSystem.API.Models.Auth.enums;
using CentralizedSalesSystem.API.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CentralizedSalesSystem.API.Services;

public class UserService : IUserService
{
    private readonly CentralizedSalesDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(CentralizedSalesDbContext db, IPasswordHasher<User> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<object> GetAllAsync(int page, int limit, string? sortBy, string? sortDirection,
        string? filterByName, string? filterByPhone, string? filterByEmail, Status? filterByActivity, long? filterByBusinessId)
    {
        var query = _db.Users.AsQueryable();

        // Name is not stored in the current model; ignoring filterByName to honor "don't change models".
        if (!string.IsNullOrWhiteSpace(filterByPhone))
            query = query.Where(u => u.Phone.Contains(filterByPhone));
        if (!string.IsNullOrWhiteSpace(filterByEmail))
            query = query.Where(u => u.Email.Contains(filterByEmail));
        if (filterByActivity.HasValue)
            query = query.Where(u => u.Status == filterByActivity.Value);
        if (filterByBusinessId.HasValue)
            query = query.Where(u => u.BusinessId == filterByBusinessId.Value);

        query = (sortBy?.ToLowerInvariant(), sortDirection?.ToLowerInvariant()) switch
        {
            ("phone", "desc") => query.OrderByDescending(u => u.Phone),
            ("phone", _) => query.OrderBy(u => u.Phone),
            ("email", "desc") => query.OrderByDescending(u => u.Email),
            ("email", _) => query.OrderBy(u => u.Email),
            ("name", "desc") => query.OrderByDescending(u => u.Id),
            ("name", _) => query.OrderBy(u => u.Id),
            _ => query.OrderBy(u => u.Id)
        };

        var total = await query.CountAsync();
        var users = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();
        var data = users.Select(MapToDto).ToList();

        var totalPages = (int)Math.Ceiling(total / (double)limit);
        return new { data, page, limit, total, totalPages };
    }

    public async Task<UserResponseDto?> GetByIdAsync(long id)
    {
        var entity = await _db.Users.FindAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<UserResponseDto> CreateAsync(UserCreateDto dto)
    {
        var user = new User
        {
            BusinessId = dto.BusinessId,
            Email = dto.Email,
            Phone = dto.Phone,
            Status = dto.Activity
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return MapToDto(user);
    }

    public async Task<UserResponseDto?> PatchAsync(long id, UserPatchDto dto)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return null;

        if (dto.BusinessId.HasValue) user.BusinessId = dto.BusinessId.Value;
        if (!string.IsNullOrWhiteSpace(dto.Email)) user.Email = dto.Email;
        if (!string.IsNullOrWhiteSpace(dto.Phone)) user.Phone = dto.Phone;
        if (dto.Activity.HasValue) user.Status = dto.Activity.Value;
        if (!string.IsNullOrWhiteSpace(dto.Password))
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        await _db.SaveChangesAsync();
        return MapToDto(user);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return false;
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return true;
    }

    private static UserResponseDto MapToDto(User u) => new()
    {
        Id = u.Id,
        BusinessId = u.BusinessId,
        Name = null, // Name not available in current model; honoring constraint to not change models.
        Email = u.Email,
        Phone = u.Phone,
        Activity = u.Status
    };
}
