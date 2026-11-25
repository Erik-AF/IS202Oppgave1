using System.Globalization;
using System.IO.Compression;
using System.Xml.Linq;
using _202Oppgave.Models;

namespace _202Oppgave.Services;

/// Bygger en XLSX-fil med ressurser som tekstceller uten å kreve Excel på serveren.
public static class ResourceExcelExport
{
    private static readonly XNamespace Sheet = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private static readonly XNamespace Relations = "http://schemas.openxmlformats.org/package/2006/relationships";
    private const string OfficeRelations = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

    /// Bygger et XLSX-øyeblikksbilde med eksporttid i UTC, filterrad og frosne overskrifter.
    public static byte[] Create(IEnumerable<Resource> resources, DateTimeOffset exportedAt)
    {
        var rows = new List<string?[]> {
            new[] { "Ko/ordinert – tilgjengelige ressurser" },
            new[] { "Eksportert (UTC)", exportedAt.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) },
            new[] { "Øyeblikksbilde – status kan endres etter nedlasting." },
            new[] { "Ressurs", "Kategori", "Sted", "Status", "Beskrivelse", "Kontaktperson", "Telefon", "E-post", "Tilgjengelig fra", "Tilgjengelig til", "Breddegrad", "Lengdegrad" }
        };
        foreach (var r in resources)
            rows.Add(new[] { r.Type, r.CategoryName, r.Area, r.Status, r.Description,
                r.ContactName, r.ContactPhone, r.ContactEmail,
                r.AvailableFrom?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                r.AvailableTo?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                r.Latitude?.ToString(CultureInfo.InvariantCulture), r.Longitude?.ToString(CultureInfo.InvariantCulture) });

        using var output = new MemoryStream();
        using (var zip = new ZipArchive(output, ZipArchiveMode.Create, true))
        {
            XNamespace types = "http://schemas.openxmlformats.org/package/2006/content-types";
            Write(zip, "[Content_Types].xml", new XElement(types + "Types",
                new XElement(types + "Default", new XAttribute("Extension", "rels"), new XAttribute("ContentType", "application/vnd.openxmlformats-package.relationships+xml")),
                new XElement(types + "Default", new XAttribute("Extension", "xml"), new XAttribute("ContentType", "application/xml")),
                new XElement(types + "Override", new XAttribute("PartName", "/xl/workbook.xml"), new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml")),
                new XElement(types + "Override", new XAttribute("PartName", "/xl/worksheets/sheet1.xml"), new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"))));
            Write(zip, "_rels/.rels", Relationship("officeDocument", "xl/workbook.xml"));
            Write(zip, "xl/_rels/workbook.xml.rels", Relationship("worksheet", "worksheets/sheet1.xml"));
            Write(zip, "xl/workbook.xml", new XElement(Sheet + "workbook",
                new XAttribute(XNamespace.Xmlns + "r", OfficeRelations),
                new XElement(Sheet + "sheets", new XElement(Sheet + "sheet", new XAttribute("name", "Ressurser"),
                    new XAttribute("sheetId", "1"), new XAttribute(XName.Get("id", OfficeRelations), "rId1")))));
            Write(zip, "xl/worksheets/sheet1.xml", new XElement(Sheet + "worksheet",
                new XElement(Sheet + "sheetViews", new XElement(Sheet + "sheetView", new XAttribute("workbookViewId", "0"),
                    new XElement(Sheet + "pane", new XAttribute("ySplit", "4"), new XAttribute("topLeftCell", "A5"), new XAttribute("activePane", "bottomLeft"), new XAttribute("state", "frozen")))),
                new XElement(Sheet + "cols", new XElement(Sheet + "col", new XAttribute("min", "1"), new XAttribute("max", "12"), new XAttribute("width", "25"), new XAttribute("customWidth", "1"))),
                new XElement(Sheet + "sheetData", rows.Select((row, index) => new XElement(Sheet + "row", new XAttribute("r", index + 1),
                    row.Select((value, column) => Cell($"{(char)('A' + column)}{index + 1}", value))))),
                new XElement(Sheet + "autoFilter", new XAttribute("ref", $"A4:L{rows.Count}"))));
        }
        return output.ToArray();
    }

    /// Oppretter en Open XML-relasjon mellom delene i XLSX-arkivet.
    private static XElement Relationship(string type, string target) => new(Relations + "Relationships",
        new XElement(Relations + "Relationship", new XAttribute("Id", "rId1"),
            new XAttribute("Type", OfficeRelations + "/" + type), new XAttribute("Target", target)));

    /// Lagrer innhold som tekst for å bevare telefonnumre og unngå formler; fjerner ugyldige kontrolltegn og begrenser cellelengden.
    private static XElement Cell(string address, string? value)
    {
        var text = new string((value ?? "").Where(c => !char.IsControl(c) || c is '\t' or '\n' or '\r').ToArray());
        if (text.Length > 32767) text = text[..32767];
        return new XElement(Sheet + "c", new XAttribute("r", address), new XAttribute("t", "inlineStr"),
            new XElement(Sheet + "is", new XElement(Sheet + "t", new XAttribute(XNamespace.Xml + "space", "preserve"), text)));
    }

    /// Skriver et XML-dokument til riktig del av XLSX-arkivet.
    private static void Write(ZipArchive zip, string path, XElement document)
    {
        using var stream = zip.CreateEntry(path).Open();
        new XDocument(document).Save(stream);
    }
}
