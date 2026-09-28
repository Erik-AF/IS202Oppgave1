// Delte hjelpefunksjoner for kart-visningene i appen.
// Holdes i egen fil (i stedet for inline i hver .cshtml) slik at andre
// views kan gjenbruke samme logikk uten å duplisere kode.

// getMarkerColor: bestemmer fargekode ut fra kategori/prioritet/status.
//   rød = akutt behov, gul = planlagt behov,
//   grønn = tilgjengelig ressurs, blå = offentlig aktør.
function getMarkerColor(punkt) {
    if (punkt.kategori === "behov" && punkt.prioritet === "Akutt") return "red";
    if (punkt.kategori === "behov" && punkt.prioritet === "Planlagt") return "#d4b800";
    if (punkt.kategori === "ressurs" && punkt.status === "Tilgjengelig") return "green";
    if (punkt.kategori === "offentlig_aktor") return "blue";
    return "gray"; // fallback for uventede/andre tilstander
}