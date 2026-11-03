
// JavaScript code.

/**
 * site.js – logikk for mobilmenyen.
 * Eier: Sarah (Frontend/UI).
 *
 * Leaflet-koden ligger i kartsiden, siden den kun brukes der.
 */


// --- 1. Hamburgermeny ---
// CSS viser menyen når den har klassen "is-open" (se site.css)

(function () {
    const toggle = document.querySelector('.nav-toggle');
    const nav = document.getElementById('main-nav');
    if (!toggle || !nav) return;

    toggle.addEventListener('click', function () {
        // toggle legger til klassen hvis den mangler, og fjerner den hvis den finnes
        const open = nav.classList.toggle('is-open');

        // oppdaterer teksten skjermlesere bruker
        toggle.setAttribute('aria-expanded', String(open));
        toggle.setAttribute('aria-label', open ? 'Lukk meny' : 'Åpne meny');
    });

    // Escape lukker menyen og setter fokus tilbake på knappen
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && nav.classList.contains('is-open')) {
            nav.classList.remove('is-open');
            toggle.setAttribute('aria-expanded', 'false');
            toggle.setAttribute('aria-label', 'Åpne meny');
            toggle.focus();
        }
    });
})();


// --- 2. Parallakse ---
// Bildet i hero beveger seg litt saktere enn resten av siden når man scroller.

(function () {
    // finnes bare på forsiden (data-parallax står i Index.cshtml)
    const heroImg = document.querySelector('.hero [data-parallax]');
    if (!heroImg) return;

    // ingen effekt hvis brukeren har valgt "reduser bevegelse" i systemet
    const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (prefersReducedMotion) return;

    let ticking = false;

    function updateParallax() {
        // 0.3 = bildet flytter seg 30 % av scrolle-avstanden (lavere = roligere)
        heroImg.style.transform = 'translateY(' + (window.scrollY * 0.3) + 'px)';
        ticking = false;
    }

    window.addEventListener('scroll', function () {
        // requestAnimationFrame: bare én oppdatering per bilde, gir jevnere scroll på mobil
        if (!ticking) {
            requestAnimationFrame(updateParallax);
            ticking = true;
        }
    });
})();
 