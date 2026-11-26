using System.IO.Compression;
using System.Xml.Linq;
using _202Oppgave.Controllers;
using _202Oppgave.DataAccess.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CrisisSystem.Tests;

public class ResourceExportTests
{
    [Fact]
    public async Task Export_FiltersUnavailableResourcesAndWritesUserDataAsText()
    {
        var repo = new Mock<IResourceRepository>();
        repo.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<_202Oppgave.Models.Entities.Resource> {
            new() { Id = 1, Type = "=1+1", Name = "Oslo", Status = "Tilgjengelig", ContactPhone = "00123456", Description = "<tekst>&" },
            new() { Id = 2, Type = "Should not appear", Name = "Oslo", Status = "Utilgjengelig" },
            new() { Id = 3, Type = "Wrong area", Name = "Bergen", Status = "Tilgjengelig" }
        });
        var controller = new ResourceController(repo.Object, null!);
        var file = Assert.IsType<FileContentResult>(await controller.ExportExcel(null, null, null, "Oslo", null));
        using var zip = new ZipArchive(new MemoryStream(file.FileContents));
        using var sheet = zip.GetEntry("xl/worksheets/sheet1.xml")!.Open();
        var xml = XDocument.Load(sheet); XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var data = xml.Descendants(ns + "row").Where(r => int.Parse(r.Attribute("r")!.Value) >= 5).ToList();
        var row = Assert.Single(data);
        Assert.Contains(row.Descendants(ns + "t"), t => t.Value == "=1+1");
        Assert.Contains(row.Descendants(ns + "t"), t => t.Value == "00123456");
        Assert.Contains(row.Descendants(ns + "t"), t => t.Value == "<tekst>&");
        Assert.All(row.Elements(ns + "c"), c => Assert.Equal("inlineStr", c.Attribute("t")!.Value));
        Assert.Empty(xml.Descendants(ns + "f"));
    }
}
