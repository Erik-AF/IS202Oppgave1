
// JavaScript code.

/**
 * site.js – logikk for mobilmenyen.
 * Eier: Sarah (Frontend/UI).
 *
 * Leaflet-koden ligger i kartsiden siden den kun brukes der.
 */

(function () {
    const toggle = document.querySelector('.nav-toggle');
    const nav = document.getElementById('main-nav');
    if (!toggle || !nav) return;

    toggle.addEventListener('click', function () {
        const open = nav.classList.toggle('is-open');
        toggle.setAttribute('aria-expanded', String(open));
        toggle.setAttribute('aria-label', open ? 'Lukk meny' : 'Åpne meny');
    });

    // Lukk menyen med Escape og flytt fokus tilbake til knappen
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && nav.classList.contains('is-open')) {
            nav.classList.remove('is-open');
            toggle.setAttribute('aria-expanded', 'false');
            toggle.setAttribute('aria-label', 'Åpne meny');
            toggle.focus();
        }
    });
})();