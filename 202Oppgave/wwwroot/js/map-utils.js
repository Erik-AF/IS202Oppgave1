// Delte hjelpefunksjoner for kartvisningene.
function getMarkerColor(punkt) {
    if (punkt.kategori === 'behov' && punkt.prioritet === 'Akutt') return '#c84b3c';
    if (punkt.kategori === 'behov' && punkt.prioritet === 'Planlagt') return '#a7770a';
    if (punkt.kategori === 'ressurs' && punkt.status === 'Tilgjengelig') return '#20745c';
    if (punkt.kategori === 'offentlig_aktor') return '#3279b5';
    return '#65737d';
}

// Kun faste SVG-symboler brukes i HTML; brukerens tekst settes ikke inn her.
function getMapSymbol(punkt) {
    if (punkt.kategori === 'behov') return 'warning';
    const type = String(punkt.type ?? '').toLocaleLowerCase('nb-NO');
    if (/båt|bat\b|boat|fartøy|ferry/.test(type)) return 'boat';
    if (/ambulanse|førstehjelp|medisin|helse/.test(type)) return 'medical';
    if (/lastebil|traktor|transport|truck/.test(type)) return 'truck';
    if (/bil|kjøretøy|car/.test(type)) return 'car';
    if (/drikkevann|vann|water/.test(type)) return 'water';
    if (/strøm|aggregat|generator|batteri/.test(type)) return 'power';
    return 'box';
}

function createMapIcon(punkt) {
    const paths = {
        warning: '<path d="M12 3 2 21h20L12 3Z"/><path d="M12 9v5m0 3v.1"/>',
        boat: '<path d="M4 14 12 10l8 4-3 5H7l-3-5Z"/><path d="M8 12V6h8v6M12 6V3M2 21q2-2 5 0t5 0 5 0 5 0"/>',
        car: '<path d="m5 10 2-5h10l2 5M4 10h16v8H4zM7 18v2m10-2v2M7 14h1m8 0h1"/>',
        truck: '<path d="M3 5h11v12H3zM14 10h4l3 4v3h-7"/><circle cx="7" cy="18" r="2"/><circle cx="18" cy="18" r="2"/>',
        water: '<path d="M12 2S5 10 5 15a7 7 0 0 0 14 0c0-5-7-13-7-13Z"/><path d="M8 15a4 4 0 0 0 4 4"/>',
        power: '<path d="m13 2-9 12h7l-1 8 10-13h-7l1-7Z"/>',
        medical: '<path d="M9 3h6v6h6v6h-6v6H9v-6H3V9h6V3Z"/>',
        box: '<path d="m3 7 9-4 9 4v10l-9 4-9-4V7Zm0 0 9 4 9-4M12 11v10M7 5l10 5"/>'
    };
    const pin = document.createElement('span');
    pin.className = 'crisis-pin';
    pin.style.backgroundColor = getMarkerColor(punkt);
    pin.innerHTML = '<svg class="crisis-pin-symbol" viewBox="0 0 24 24" fill="none" ' +
        'stroke="currentColor" stroke-width="1.8" stroke-linecap="round" ' +
        'stroke-linejoin="round" aria-hidden="true">' + paths[getMapSymbol(punkt)] + '</svg>';
    return L.divIcon({
        className: 'crisis-marker',
        html: pin,
        iconSize: [44, 54],
        iconAnchor: [22, 52],
        popupAnchor: [0, -48]
    });
}

// Beholder kartformen. Sirkler har radius i GeoJSON-egenskapene.
function createMapFeature(punkt) {
    const color = getMarkerColor(punkt);
    const style = { color, weight: 3, fillColor: color, fillOpacity: 0.22 };
    if (punkt.geometryJson) {
        try {
            const geometry = typeof punkt.geometryJson === 'string'
                ? JSON.parse(punkt.geometryJson) : punkt.geometryJson;
            return L.geoJSON(geometry, {
                style,
                pointToLayer: function (feature, latlng) {
                    const radius = feature.properties?.radius;
                    if (typeof radius === 'number' && radius > 0) {
                        return L.circle(latlng, { ...style, radius });
                    }
                    return L.marker(latlng, {
                        icon: createMapIcon(punkt), title: String(punkt.type ?? '')
                    });
                }
            });
        } catch (error) {
            console.warn('Kunne ikke lese kartform for registrering:', punkt.id, error);
        }
    }
    if (punkt.latitude == null || punkt.longitude == null) return null;
    const lat = Number(punkt.latitude);
    const lng = Number(punkt.longitude);
    if (!Number.isFinite(lat) || !Number.isFinite(lng) ||
        lat < -90 || lat > 90 || lng < -180 || lng > 180) return null;
    return L.marker([lat, lng], {
        icon: createMapIcon(punkt), title: String(punkt.type ?? '')
    });
}
