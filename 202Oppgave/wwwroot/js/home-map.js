/* home-map.js - kartet på forsiden (Sarah)
   Viser et lite Leaflet-kart bak kortet "Kartoversikt". Det er bare en
   forhåndsvisning, så all interaksjon er slått av (kortet er lenken til hele kartet).

   Kartet vises alltid. Registrerte ressurser og behov kommer som markører så snart
   MapController.Markers() finnes. Mangler Leaflet (f.eks. uten nett), blir den lyse
   bakgrunnen (.home-map__bg) stående.

   AVTALE MED MARIUS
   MapController trenger en action "Markers" som returnerer JSON slik:
     [ { "type": "resource", "lat": 58.16, "lng": 8.0, "title": "Traktor" },
       { "type": "need",     "lat": 58.30, "lng": 7.9, "title": "Sandsekker" } ]
   - type er "resource" (ressurs) eller "need" (behov)
   - Index.cshtml setter data-markers-url automatisk når actionen finnes */
(function () {
    const wrap = document.querySelector('.home-map');
    const el = document.getElementById('home-map');

    // Leaflet er lastet i _Layout. Uten det beholder vi bakgrunnen.
    if (!wrap || !el || typeof L === 'undefined') return;

    // Bytt til samme kartfliser som kartsiden bruker, så de ser like ut.
    const TILE_URL = 'https://tile.openstreetmap.org/{z}/{x}/{y}.png';
    const TILE_ATTRIBUTION = '&copy; OpenStreetMap';

    // hele Norge, brukes som utsnitt når det ikke finnes markører
    const NORGE = [[57.9, 4.4], [71.3, 31.3]];

    // fargene hentes fra CSS, så de alltid stemmer med tegnforklaringen
    const css = getComputedStyle(document.documentElement);
    const dark = css.getPropertyValue('--green-900').trim() || '#093732';
    const lime = css.getPropertyValue('--lime').trim() || '#e1f18b';

    // alt interaktivt er slått av: kartet er bare en forhåndsvisning
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

    // Det hvite kortet ligger over venstre del av kartet. Padding til venstre
    // gjør at Norge og markørene havner til høyre for det (ikke på smal skjerm).
    const cardSpace = el.clientWidth > 700 ? 420 : 12;

    function fit(bounds) {
        map.fitBounds(bounds, {
            paddingTopLeft: [cardSpace, 12],
            paddingBottomRight: [12, 12],
            maxZoom: 9   // så ett enkelt punkt ikke zoomes for nært
        });
    }

    fit(NORGE);

    // kildehenvisning nede til venstre (tegnforklaringen ligger til høyre)
    L.control.attribution({ position: 'bottomleft', prefix: false })
        .addAttribution(TILE_ATTRIBUTION)
        .addTo(map);

    // vis kartet først når kartflisene er lastet
    L.tileLayer(TILE_URL, { maxZoom: 12 })
        .on('load', function () { wrap.classList.add('is-live'); })
        .addTo(map);

    // Ingen adresse = Marius har ikke laget Markers() ennå. Kartet vises uten markører.
    const url = el.dataset.markersUrl;
    if (!url) return;

    fetch(url, { headers: { Accept: 'application/json' } })
        .then(function (res) {
            if (!res.ok) throw new Error('HTTP ' + res.status);
            return res.json();
        })
        .then(addMarkers)
        .catch(function () { /* feil: kartet står igjen uten markører */ });

    function addMarkers(markers) {
        const points = [];

        markers.forEach(function (m) {
            if (typeof m.lat !== 'number' || typeof m.lng !== 'number') return;

            const isNeed = m.type === 'need';

            // samme utseende som prikkene i tegnforklaringen:
            // ressurs = mørk prikk, behov = lime prikk med mørk kant
            L.circleMarker([m.lat, m.lng], {
                radius: 7,
                weight: 2,
                color: isNeed ? dark : '#fff',
                fillColor: isNeed ? lime : dark,
                fillOpacity: 1
            }).bindTooltip(m.title || '').addTo(map);

            points.push([m.lat, m.lng]);
        });

        // zoom slik at alle markørene vises
        if (points.length > 0) fit(points);
    }
})();