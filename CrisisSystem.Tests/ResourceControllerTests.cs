
using _202Oppgave.Controllers;
using _202Oppgave.DataAccess.Repositories;
using _202Oppgave.Models.Entities;
using _202Oppgave.Models.Authorization;
using _202Oppgave.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Xunit;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace CrisisSystem.Tests;

public class ResourceControllerTests
{
    private class FakeResourceRepository : IResourceRepository
    {
        public List<Resource> Resources { get; } = new();

        public Task AddAsync(Resource resource)
        {
            resource.Id = Resources.Count + 1;
            Resources.Add(resource);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Resource resource)
        {
            return Task.CompletedTask;
        }

        public Task<List<Resource>> GetAllAsync()
        {
            return Task.FromResult(Resources.ToList());
        }

        public Task<Resource?> GetByIdAsync(int id)
        {
            return Task.FromResult(
                Resources.FirstOrDefault(r => r.Id == id));
        }

        public Task<List<Resource>> GetByUserIdAsync(string userId)
        {
            return Task.FromResult(
                Resources.Where(r => r.UserId == userId).ToList());
        }

        public List<ResourceImage> Images { get; } = new();

        public Task<ResourceImage?> GetImageAsync(int resourceId)
        {
            return Task.FromResult(
                Images.FirstOrDefault(i => i.ResourceId == resourceId));
        }

        public Task SaveImageAsync(int resourceId, byte[] data, string contentType)
        {

            Images.RemoveAll(i => i.ResourceId == resourceId);
            Images.Add(new ResourceImage
            {
                ResourceId = resourceId,
                Data = data,
                ContentType = contentType
            });
            return Task.CompletedTask;
        }

        public Task DeleteAsync(int id)
        {
            Resources.RemoveAll(r => r.Id == id);
            return Task.CompletedTask;
        }

        public Task<List<Category>> GetCategoriesAsync()
        {
            return Task.FromResult(new List<Category>());
        }
    }

    private static ResourceController CreateLoggedInController(FakeResourceRepository repository)
    {
        return new ResourceController(repository, null!)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") }, "Test"))
                }
            }
        };
    }

    [Fact]
    public async Task Create_ValidResource_ShouldSaveResource()
    {
        var repository = new FakeResourceRepository();
        var controller = CreateLoggedInController(repository);

        var model = new ResourceViewModel
        {
            Type = "Generator",
            Area = "Kristiansand",
            Latitude = 58.1467,
            Longitude = 7.9956,
            ContactName = "Testperson",
            ContactPhone = "12345678",
            CategoryId = 1
        };

        await controller.Create(model);

        var saved = Assert.Single(repository.Resources);

        Assert.Equal("Generator", saved.Type);
        Assert.Equal("Kristiansand", saved.Name);
        Assert.Equal(model.Latitude, saved.Latitude);
        Assert.Equal(model.Longitude, saved.Longitude);
        Assert.Equal("Testperson", saved.ContactName);
        Assert.Equal("user-123", saved.UserId);
    }

    [Fact]
    public async Task Create_ValidResource_ShouldRedirectToDetails()
    {
        var repository = new FakeResourceRepository();
        var controller = CreateLoggedInController(repository);

        var model = new ResourceViewModel
        {
            Type = "Drikkevann",
            Area = "Grimstad",
            Latitude = 58.3405,
            Longitude = 8.5934,
            ContactName = "Testperson",
            ContactPhone = "12345678",
            CategoryId = 1
        };

        var result = await controller.Create(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal(repository.Resources[0].Id, redirect.RouteValues!["id"]);
    }

    [Fact]
    public async Task Create_WithoutLoggedInUser_ShouldNotSave()
    {
        var repository = new FakeResourceRepository();
        var controller = new ResourceController(repository, null!)
        {

            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var result = await controller.Create(new ResourceViewModel());

        Assert.IsType<ChallengeResult>(result);
        Assert.Empty(repository.Resources);
    }

[Fact]
public async Task Index_Search_ShouldReturnMatchingResources()
{
    var repository = new FakeResourceRepository();

    repository.Resources.Add(new Resource
    {
        Id = 1,
        Type = "Generator",
        Name = "Kristiansand"
    });

    repository.Resources.Add(new Resource
    {
        Id = 2,
        Type = "Drikkevann",
        Name = "Grimstad"
    });

    var controller = new ResourceController(repository, null!);

    var result = await controller.Index(
        "generator", null, null, null, null);

    var view = Assert.IsType<ViewResult>(result);
    var model = Assert.IsType<ResourceIndexViewModel>(view.Model);

    var resource = Assert.Single(model.Resources);
    Assert.Equal("Generator", resource.Type);
}

[Fact]
public async Task Index_FilterByArea_ShouldReturnCorrectResources()
{
    var repository = new FakeResourceRepository();

    repository.Resources.Add(new Resource
    {
        Id = 1,
        Type = "Generator",
        Name = "Kristiansand"
    });

    repository.Resources.Add(new Resource
    {
        Id = 2,
        Type = "Drikkevann",
        Name = "Grimstad"
    });

    var controller = new ResourceController(repository, null!);

    var result = await controller.Index(
        null, null, null, "Grimstad", null);

    var view = Assert.IsType<ViewResult>(result);
    var model = Assert.IsType<ResourceIndexViewModel>(view.Model);

    var resource = Assert.Single(model.Resources);
    Assert.Equal("Grimstad", resource.Area);
}

[Fact]
public async Task Index_FilterByStatus_ShouldReturnCorrectResources()
{
    var repository = new FakeResourceRepository();

    repository.Resources.Add(new Resource
    {
        Id = 1,
        Type = "Generator",
        Name = "Kristiansand",
        Status = "Tilgjengelig"
    });

    repository.Resources.Add(new Resource
    {
        Id = 2,
        Type = "Drikkevann",
        Name = "Grimstad",
        Status = "Utilgjengelig"
    });

    var controller = new ResourceController(repository, null!);

    var result = await controller.Index(
        null, null, "Tilgjengelig", null, null);

    var view = Assert.IsType<ViewResult>(result);
    var model = Assert.IsType<ResourceIndexViewModel>(view.Model);

    var resource = Assert.Single(model.Resources);
    Assert.Equal("Tilgjengelig", resource.Status);
}

[Fact]
public async Task Edit_ExistingResource_ShouldUpdateResource()
{
    var repository = new FakeResourceRepository();
    repository.Resources.Add(new Resource
    {
        Id = 1,
        UserId = "user-123",
        Type = "Generator",
        Name = "Kristiansand"
    });

    var controller = CreateLoggedInController(repository);
    var model = new ResourceViewModel
    {
        Type = "Drikkevann",
        Area = "Grimstad",
        Latitude = 58.3405,
        Longitude = 8.5934,
        ContactName = "Testperson",
        ContactPhone = "12345678",
        CategoryId = 1
    };

    var result = await controller.Edit(1, model);

    var updated = Assert.Single(repository.Resources);
    Assert.Equal("Drikkevann", updated.Type);
    Assert.Equal("Grimstad", updated.Name);

    var redirect = Assert.IsType<RedirectToActionResult>(result);
    Assert.Equal("Details", redirect.ActionName);
}

[Fact]
public async Task Edit_NonExistingResource_ShouldReturnNotFound()
{
    var controller = new ResourceController(
        new FakeResourceRepository(), null!);

    var model = new ResourceViewModel
    {
        Type = "Generator",
        Area = "Kristiansand",
        Latitude = 58.1467,
        Longitude = 7.9956,
        ContactName = "Testperson",
        ContactPhone = "12345678",
        CategoryId = 1
    };

    var result = await controller.Edit(999, model);

    Assert.IsType<NotFoundResult>(result);
}

[Fact]
public async Task Delete_ExistingResource_ShouldRemoveResource()
{
    var repository = new FakeResourceRepository();
    repository.Resources.Add(new Resource
    {
        Id = 1,
        Type = "Generator",
        Name = "Kristiansand",
        UserId = "user-123"
    });

    var controller = CreateLoggedInController(repository);

    var result = await controller.Delete(1);

    Assert.Empty(repository.Resources);

    var redirect = Assert.IsType<RedirectToActionResult>(result);
    Assert.Equal("Index", redirect.ActionName);
}

[Fact]
public async Task Delete_NonExistingResource_ShouldReturnNotFound()
{
    var controller = new ResourceController(
        new FakeResourceRepository(), null!);

    var result = await controller.Delete(999);

    Assert.IsType<NotFoundResult>(result);
}

    private static IFormFile CreateTestImage()
    {
        var bytes = new byte[] { 1, 2, 3, 4 };
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "Image", "test.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
    }

    [Fact]
    public async Task Create_WithImage_ShouldSaveImageInDatabase()
    {
        var repository = new FakeResourceRepository();
        var controller = CreateLoggedInController(repository);

        var model = new ResourceViewModel
        {
            Type = "Generator",
            Area = "Kristiansand",
            Latitude = 58.1467,
            Longitude = 7.9956,
            ContactName = "Testperson",
            ContactPhone = "12345678",
            CategoryId = 1,
            Image = CreateTestImage()
        };

        await controller.Create(model);

        var saved = Assert.Single(repository.Resources);
        var image = Assert.Single(repository.Images);

        Assert.Equal(saved.Id, image.ResourceId);
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, image.Data);
        Assert.Equal("image/jpeg", image.ContentType);

        Assert.Equal($"/Resource/Image/{saved.Id}", saved.ImagePath);
    }

    [Fact]
    public async Task Image_Existing_ShouldReturnFile()
    {
        var repository = new FakeResourceRepository();
        await repository.SaveImageAsync(1, new byte[] { 9, 8, 7 }, "image/png");

        var controller = new ResourceController(repository, null!);

        var result = await controller.Image(1);

        var file = Assert.IsType<FileContentResult>(result);
        Assert.Equal("image/png", file.ContentType);
        Assert.Equal(new byte[] { 9, 8, 7 }, file.FileContents);
    }

    [Fact]
    public async Task Image_Missing_ShouldReturnNotFound()
    {
        var controller = new ResourceController(new FakeResourceRepository(), null!);

        var result = await controller.Image(999);

        Assert.IsType<NotFoundResult>(result);
    }

    private static ResourceController CreateControllerAs(
        FakeResourceRepository repository, string userId, bool isAdmin = false)
    {
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };

        if (isAdmin)
            claims.Add(new Claim(ClaimTypes.Role, RoleNames.Admin));

        return new ResourceController(repository, null!)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
                }
            }
        };
    }

    private static FakeResourceRepository CreateRepositoryWithOwnedResource()
    {
        var repository = new FakeResourceRepository();
        repository.Resources.Add(new Resource
        {
            Id = 1,
            UserId = "user-123",
            Type = "Generator",
            Name = "Kristiansand"
        });
        return repository;
    }

    [Fact]
    public async Task EditGet_OtherUsersResource_ShouldReturnForbid()
    {
        var repository = CreateRepositoryWithOwnedResource();
        var controller = CreateControllerAs(repository, "user-999");

        var result = await controller.Edit(1);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task EditPost_OtherUsersResource_ShouldReturnForbid_AndNotChange()
    {
        var repository = CreateRepositoryWithOwnedResource();
        var controller = CreateControllerAs(repository, "user-999");

        var model = new ResourceViewModel
        {
            Type = "Endret",
            Area = "Grimstad",
            Latitude = 58.3405,
            Longitude = 8.5934,
            ContactName = "Testperson",
            ContactPhone = "12345678",
            CategoryId = 1
        };

        var result = await controller.Edit(1, model);

        Assert.IsType<ForbidResult>(result);

        Assert.Equal("Generator", repository.Resources.Single().Type);
    }

    [Fact]
    public async Task Delete_OtherUsersResource_ShouldReturnForbid_AndKeepResource()
    {
        var repository = CreateRepositoryWithOwnedResource();
        var controller = CreateControllerAs(repository, "user-999");

        var result = await controller.Delete(1);

        Assert.IsType<ForbidResult>(result);

        Assert.Single(repository.Resources);
    }

    [Fact]
    public async Task Delete_AsAdmin_ShouldRemoveOtherUsersResource()
    {
        var repository = CreateRepositoryWithOwnedResource();
        var controller = CreateControllerAs(repository, "admin-1", isAdmin: true);

        var result = await controller.Delete(1);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Empty(repository.Resources);
    }

}
