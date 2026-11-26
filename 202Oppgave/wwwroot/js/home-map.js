/// Kartforhåndsvisning med valgbare bakgrunnslag. Uten markørdata beholdes bakgrunnskartet.

(function () {
    const wrap = document.querySelector('.home-map');
    const el = document.getElementById('home-map');

    if (!wrap || !el || typeof L === 'undefined') return;

    const TILE_URL = 'https://tile.openstreetmap.org/{z}/{x}/{y}.png';
    const TILE_ATTRIBUTION = '&copy; OpenStreetMap';

    const NORGE = [[57.9, 4.4], [71.3, 31.3]];

    const css = getComputedStyle(document.documentElement);
    const dark = css.getPropertyValue('--green-900').trim() || '#093732';
    const lime = css.getPropertyValue('--lime').trim() || '#e1f18b';

    const map = L.map(el, {
        zoomControl: false,
        attributionControl: false,
        dragging: false,
        touchZoom: false,
        scrollWheelZoom: false,
        doubleClickZoom: false,
        boxZoom: false,
        keyboard: false
    });

    const cardSpace = el.clientWidth > 700 ? 420 : 12;

    /// Tilpasser utsnittet med plass til forsidekortet og begrenser innzooming ved få markører.
    function fit(bounds) {
        map.fitBounds(bounds, {
            paddingTopLeft: [cardSpace, 12],
            paddingBottomRight: [12, 12],
            maxZoom: 9
        });
    }

    fit(NORGE);

    L.control.attribution({ position: 'bottomleft', prefix: false })
        .addTo(map);

    const standardLayer = L.tileLayer(TILE_URL, { maxZoom: 12, attribution: TILE_ATTRIBUTION })
        .on('load', function () { wrap.classList.add('is-live'); })
        .addTo(map);
    addMapLayers(map, standardLayer);

    const url = el.dataset.markersUrl;
    if (!url) return;

    fetch(url, { headers: { Accept: 'application/json' } })
        .then(function (res) {
            if (!res.ok) throw new Error('HTTP ' + res.status);
            return res.json();
        })
        .then(addMarkers)
        .catch(function () {   });

    function addMarkers(markers) {
        const points = [];

        markers.forEach(function (m) {
            if (typeof m.lat !== 'number' || typeof m.lng !== 'number') return;

            const isNeed = m.type === 'need';

            L.circleMarker([m.lat, m.lng], {
                radius: 7,
                weight: 2,
                color: isNeed ? dark : '#fff',
                fillColor: isNeed ? lime : dark,
                fillOpacity: 1
            }).bindTooltip(m.title || '').addTo(map);

            points.push([m.lat, m.lng]);
        });

        if (points.length > 0) fit(points);
    }
})();