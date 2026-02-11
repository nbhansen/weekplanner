using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GirafAPI.Data;
using GirafAPI.Entities.Activities;
using GirafAPI.Entities.Citizens;
using GirafAPI.Entities.Grades;
using GirafAPI.Entities.Invitations;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Pictograms;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Identity;

namespace Giraf.IntegrationTests.Utils.DbSeeders;

public abstract class DbSeeder
{
    public Dictionary<String, GirafUser> Users { get; } = new();
    public List<Organization> Organizations { get; } = new();
    public List<Citizen> Citizens { get; } = new();
    public List<Pictogram> Pictograms { get; } = new();
    public List<Activity> Activities { get; } = new();
    public List<Grade> Grades { get; } = new();
    public List<Invitation> Invitations { get; } = new();
    
    public abstract void SeedData(GirafDbContext dbContext, UserManager<GirafUser> userManager);

    public void SeedUsers(UserManager<GirafUser> userManager)
    {
        CreateAndAddUser(userManager, "owner", "owner@email.com", "Owner", "Ownerson");
        CreateAndAddUser(userManager, "admin", "admin@email.com", "Admin", "Adminson");
        CreateAndAddUser(userManager, "member", "member@email.com", "Member", "Memberson");
    }

    private void CreateAndAddUser(UserManager<GirafUser> userManager, string key, string email, string firstName, string lastName)
    {
        var user = new GirafUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Organizations = new List<Organization>()
        };
        var result = userManager.CreateAsync(user, "Password123!").GetAwaiter().GetResult();
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to create test user '{key}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
        Users.Add(key, user);
        userManager.AddClaimAsync(user, new Claim(ClaimTypes.Name, user.UserName!)).GetAwaiter().GetResult();
        userManager.AddClaimAsync(user, new Claim(JwtRegisteredClaimNames.Sub, user.Id)).GetAwaiter().GetResult();
    }

    public void SeedSingleUser(UserManager<GirafUser> userManager)
    {
        CreateAndAddUser(userManager, "user", "user@email.com", "User", "Userson");
    }

    public void SeedOrganization(GirafDbContext dbContext,
                                 UserManager<GirafUser> userManager,
                                 GirafUser owner, 
                                 List<GirafUser> admins, 
                                 List<GirafUser> members)
    {
        var organization = new Organization
        {
            Id = Organizations.Count + 1,
            Name = "Test Organization",
            Users = new List<GirafUser>(),
            Citizens = new List<Citizen>(),
            Grades = new List<Grade>()
        };
        dbContext.Organizations.Add(organization);
        
        userManager.AddClaimAsync(owner, new Claim("OrgOwner", organization.Id.ToString())).GetAwaiter().GetResult();
        userManager.AddClaimAsync(owner, new Claim("OrgAdmin", organization.Id.ToString())).GetAwaiter().GetResult();
        userManager.AddClaimAsync(owner, new Claim("OrgMember", organization.Id.ToString())).GetAwaiter().GetResult();
        organization.Users.Add(owner);

        if (admins.Any())
        {
            foreach (var admin in admins)
            {
                userManager.AddClaimAsync(admin, new Claim("OrgAdmin", organization.Id.ToString())).GetAwaiter().GetResult();
                userManager.AddClaimAsync(admin, new Claim("OrgMember", organization.Id.ToString())).GetAwaiter().GetResult();
                organization.Users.Add(admin);
            }
        }

        if (members.Any())
        {
            foreach (var member in members)
            {
                userManager.AddClaimAsync(member, new Claim("OrgMember", organization.Id.ToString())).GetAwaiter().GetResult();
                organization.Users.Add(member);
            }
        }

        foreach (var user in organization.Users)
        {
            user.Organizations.Add(organization);
        }
        
        dbContext.SaveChanges();
        Organizations.Add(organization);
    }

    public void SeedCitizens(GirafDbContext dbContext, Organization organization)
    {
        var citizens = new List<Citizen>();
        citizens.Add(new Citizen
        {
            Id = 1,
            FirstName = "Anders",
            LastName = "And",
            Organization = organization,
            Activities = new List<Activity>()
        });
        
        citizens.Add(new Citizen
        {
            Id = 2,
            FirstName = "Rasmus",
            LastName = "Klump",
            Organization = organization,
            Activities = new List<Activity>()
        });
        
        citizens.Add(new Citizen
        {
            Id = 3,
            FirstName = "Bjørnen",
            LastName = "Bjørn",
            Organization = organization,
            Activities = new List<Activity>()
        });

        foreach (var citizen in citizens)
        {
            organization.Citizens.Add(citizen);
            citizen.Organization = organization;
            dbContext.Citizens.Add(citizen);
            Citizens.Add(citizen);
        }
    }

    public void SeedPictogram(GirafDbContext dbContext, Organization organization)
    {
        var pictogram = new Pictogram
        {
            PictogramName = "Test Pictogram",
            PictogramUrl = "https://pictogram.com/pictogram.jpg",
            Id = Pictograms.Count + 1,
            OrganizationId = organization.Id
        };
        dbContext.Pictograms.Add(pictogram);
        dbContext.SaveChanges();
        Pictograms.Add(pictogram);
    }

    public void SeedCitizenActivity(GirafDbContext dbContext, int citizenId, Pictogram pictogram)
    {
        var activity = new Activity
        {
            Id = Activities.Count + 1,
            Date = DateOnly.FromDateTime(DateTime.Now),
            StartTime = TimeOnly.FromDateTime(DateTime.Now),
            EndTime = TimeOnly.FromDateTime(DateTime.Now.AddHours(1)),
            IsCompleted = false,
            Pictogram = pictogram
        };

        var citizen = dbContext.Citizens.Find(citizenId)
            ?? throw new InvalidOperationException($"Citizen with ID {citizenId} not found during seeding.");
        citizen.Activities.Add(activity);
        dbContext.Add(activity);
        dbContext.SaveChanges();
        Activities.Add(activity);
    }
    
    public void SeedGradeActivity(GirafDbContext dbContext, int gradeId, Pictogram pictogram)
    {
        var activity = new Activity
        {
            Id = Activities.Count + 1,
            Date = DateOnly.FromDateTime(DateTime.Now),
            StartTime = TimeOnly.FromDateTime(DateTime.Now),
            EndTime = TimeOnly.FromDateTime(DateTime.Now.AddHours(1)),
            IsCompleted = false,
            Pictogram = pictogram
        };

        var grade = dbContext.Grades.Find(gradeId)
            ?? throw new InvalidOperationException($"Grade with ID {gradeId} not found during seeding.");
        grade.Activities.Add(activity);
        dbContext.Add(activity);
        dbContext.SaveChanges();
        Activities.Add(activity);
    }

    public void SeedGrade(GirafDbContext dbContext, Organization organization)
    {
        var grade = new Grade
        {
            Id = Grades.Count + 1,
            Name = "Test Grade",
            Citizens = new List<Citizen>(),
            Activities = new List<Activity>()
        };
        
        dbContext.Grades.Add(grade);
        var org = dbContext.Organizations.Find(organization.Id)
            ?? throw new InvalidOperationException($"Organization with ID {organization.Id} not found during seeding.");
        org.Grades.Add(grade);
        dbContext.SaveChanges();
        Grades.Add(grade);
    }

    public void AddCitizensToGrade(GirafDbContext dbContext, int gradeId, List<Citizen> citizens)
    {
        var grade = dbContext.Grades.Find(gradeId)
            ?? throw new InvalidOperationException($"Grade with ID {gradeId} not found during seeding.");
        
        foreach (var citizen in citizens)
        {
            grade.Citizens.Add(citizen);
        }
        dbContext.SaveChanges();
    }

    public void SeedInvitation(GirafDbContext dbContext,
        int orgId,
        String senderId,
        String receiverId)
    {
        var invitation = new Invitation
        {
            Id = Invitations.Count + 1,
            OrganizationId = orgId,
            ReceiverId = receiverId,
            SenderId = senderId
        };
        
        dbContext.Invitations.Add(invitation);
        dbContext.SaveChanges();
        Invitations.Add(invitation);
    }
}