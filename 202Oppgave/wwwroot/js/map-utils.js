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

    function identify(value) {
        if (typeof value !== 'string' || !value.trim()) return null;
        const text = value.toLocaleLowerCase('nb-NO')
            .normalize('NFKC').replace(/[-_\s/]+/g, ' ');
        if (/helikopter|hellikopter|helicopter/.test(text)) return 'other';
        if (/an{1,2}leg{1,2}s?\s*ma(?:sk|s)in|gravemaskin|hjullaster|bulldoser|dumper/.test(text)) return 'construction';
        if (/personell|personel|personale|personer|menneske|\bperson\b|ferdighet|kompetanse|mannskap|frivillig/.test(text)) return 'personnel';
        if (/material|matrial|materiell|plank|trevirke|trelast|sandsekk/.test(text)) return 'materials';
        if (/drone/.test(text)) return 'drone';
        if (/aggregat|generator/.test(text)) return 'generator';
        if (/båt|\bboat\b|fartøy|ferry/.test(text)) return 'boat';
        if (/ambulanse|førstehjelp|medisin|helse/.test(text)) return 'medical';
        if (/traktor|tractor/.test(text)) return 'tractor';
        if (/lastebil|transport|truck/.test(text)) return 'truck';
        if (/bil|kjøretøy|\bcar\b/.test(text)) return 'car';
        if (/drikkevann|vann|water/.test(text)) return 'water';
        if (/strøm|batteri/.test(text)) return 'power';
        if (/\bannet\b|\bandre\b|\bother\b/.test(text)) return 'other';
        return null;
    }

    // En konkret traktor i Type skal få traktorikon, også i en generell kategori.
    if (/traktor|tractor/i.test(String(punkt.type ?? ''))) return 'tractor';

    // Kategorien har ellers prioritet, også når Type inneholder andre nøkkelord.
    const categoryNames = [
        punkt.categoryName,
        punkt.kategoriNavn,
        typeof punkt.category === 'string' ? punkt.category : punkt.category?.name
    ];
    for (const name of categoryNames) {
        const symbol = identify(name);
        if (symbol) return symbol;
    }
    return identify(punkt.type) ?? 'other';
}

function createMapIcon(punkt) {
    const paths = {
        warning: '<path d="M12 3 2 21h20L12 3Z"/><path d="M12 9v5m0 3v.1"/>',
        // Menneske i samme hvite strekstil som de andre symbolene.
        personnel: '<circle cx="12" cy="4.5" r="2.5"/><path d="M8 21v-6H6v-4a3 3 0 0 1 3-3h6a3 3 0 0 1 3 3v4h-2v6M12 15v6M9 11v4h6v-4"/>',
        // Tre stablede planker med korte linjer som viser trestruktur.
        materials: '<rect x="2" y="3" width="20" height="5" rx="1"/><rect x="2" y="9.5" width="20" height="5" rx="1"/><rect x="2" y="16" width="20" height="5" rx="1"/><path d="M5 5.5h5m5 0h4M6 12h8m3 0h2M5 18.5h4m4 0h6"/>',
        drone: '<rect x="9" y="9" width="6" height="6" rx="2"/><path d="m9 9-3-3m9 3 3-3m-9 9-3 3m9-3 3 3"/><ellipse cx="5" cy="5" rx="4" ry="2.5"/><ellipse cx="19" cy="5" rx="4" ry="2.5"/><ellipse cx="5" cy="19" rx="4" ry="2.5"/><ellipse cx="19" cy="19" rx="4" ry="2.5"/>',
        construction: '<rect x="2" y="17" width="12" height="4" rx="2"/><path d="M4 17V9h6l2 8M5 12h5M11 13l5-9 3 2 2 9M18 15h4v4h-6l2-4ZM5 19h6"/>',
        generator: '<rect x="3" y="6" width="18" height="14" rx="2"/><path d="M8 6V3h8v3M6 20v2m12-2v2M7 10v6m3-6v6m7-7-3 4h4l-3 4"/>',
        boat: '<path d="M4 14 12 10l8 4-3 5H7l-3-5Z"/><path d="M8 12V6h8v6M12 6V3M2 21q2-2 5 0t5 0 5 0 5 0"/>',
        car: '<path d="m5 10 2-5h10l2 5M4 10h16v8H4zM7 18v2m10-2v2M7 14h1m8 0h1"/>',
        tractor: '<circle cx="7" cy="17" r="5"/><circle cx="7" cy="17" r="1.5"/><circle cx="20" cy="19" r="2.5"/><path d="M4 12V4h8l2 10h6v2.5M5 4h8M7 7v4h6M12 17h5.5M17 14V8h2M3 2h11"/>',
        truck: '<path d="M3 5h11v12H3zM14 10h4l3 4v3h-7"/><circle cx="7" cy="18" r="2"/><circle cx="18" cy="18" r="2"/>',
        water: '<path d="M12 2S5 10 5 15a7 7 0 0 0 14 0c0-5-7-13-7-13Z"/><path d="M8 15a4 4 0 0 0 4 4"/>',
        power: '<path d="m13 2-9 12h7l-1 8 10-13h-7l1-7Z"/>',
        medical: '<path d="M9 3h6v6h6v6h-6v6H9v-6H3V9h6V3Z"/>',
        other: '<circle cx="12" cy="12" r="9"/><circle cx="7" cy="12" r="1" fill="currentColor"/><circle cx="12" cy="12" r="1" fill="currentColor"/><circle cx="17" cy="12" r="1" fill="currentColor"/>'
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
