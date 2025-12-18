using CentralizedSalesSystem.API.Data;
using CentralizedSalesSystem.API.Models;
using CentralizedSalesSystem.API.Models.Auth.DTOs;
using CentralizedSalesSystem.API.Models.Auth.enums;
using CentralizedSalesSystem.API.Models.Business;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CentralizedSalesSystem.API.Services;

public class OwnerSignupService : IOwnerSignupService
{
    private readonly CentralizedSalesDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;

    public OwnerSignupService(CentralizedSalesDbContext db, IPasswordHasher<User> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<OwnerSignupResponse> RegisterOwnerAsync(OwnerSignupRequest request, CancellationToken cancellationToken = default)
    {
        // Wrap entire flow in a transaction to keep it atomic.
        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;

        var business = new Business
        {
            Name = request.Business.Name,
            Phone = request.Business.Phone,
            Address = request.Business.Address,
            Email = request.Business.Email,
            Country = string.IsNullOrWhiteSpace(request.Business.Country) ? "N/A" : request.Business.Country,
            Currency = request.Business.Currency,
            SubscriptionPlan = request.Business.SubscriptionPlan
        };

        _db.Businesses.Add(business);
        await _db.SaveChangesAsync(cancellationToken);

        var owner = new User
        {
            BusinessId = business.Id,
            Email = request.Owner.Email,
            Phone = request.Owner.Phone,
            Status = Status.Active
        };
        owner.PasswordHash = _passwordHasher.HashPassword(owner, request.Owner.Password);

        _db.Users.Add(owner);
        await _db.SaveChangesAsync(cancellationToken);

        business.OwnerId = owner.Id;
        _db.Businesses.Update(business);
        await _db.SaveChangesAsync(cancellationToken);

        var defaultRoles = await DefaultRoleProvisioning.EnsureDefaultRolesAsync(_db, business.Id, now, cancellationToken);
        await DefaultRoleProvisioning.EnsureUserRoleAsync(_db, owner, defaultRoles.OwnerRole, now, cancellationToken);

        await tx.CommitAsync(cancellationToken);

        return new OwnerSignupResponse
        {
            BusinessId = business.Id,
            OwnerUserId = owner.Id
        };
    }
}
