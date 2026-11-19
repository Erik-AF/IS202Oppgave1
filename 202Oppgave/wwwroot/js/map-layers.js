// Felles bakgrunnsvalg. Markører og tegninger beholdes når kartlaget byttes.
function addMapLayers(map, standardLayer) {
    const topographic = L.tileLayer('https://{s}.tile.opentopomap.org/{z}/{x}/{y}.png', {
        maxNativeZoom: 17,
        maxZoom: 19,
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>, SRTM | <a href="https://opentopomap.org">OpenTopoMap</a> (<a href="https://creativecommons.org/licenses/by-sa/3.0/">CC-BY-SA</a>)'
    });
    const satellite = L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}', {
        maxZoom: 19,
        attribution: 'Source: Esri, Vantor, Earthstar Geographics, and the GIS User Community'
    });

    L.control.layers({
        'Standardkart': standardLayer,
        'Topografisk': topographic,
        'Satellitt': satellite
    }, null, { position: 'topright' }).addTo(map);
}
