
/* site.js - felles JavaScript (Sarah)
   1) hamburgermeny   2) header får bakgrunn ved scrolling   3) parallakse på hero-bildet
   Kartkoden ligger ikke her: home-map.js (forsiden) og Marius sin egen fil (kartsidene). */


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



/* --- 2. Gradvis header-overgang ved scrolling --- */
(function () {
    const header = document.querySelector('.site-header--overlay');
    const hero = document.querySelector('.hero');

    if (!header || !hero) return;

    let ticking = false;

    function updateHeader() {
        const scrollY = window.scrollY;

        // Finn bunnen av hele hero-seksjonen
        const heroBottom =
            hero.getBoundingClientRect().bottom + scrollY;

        // Overgangen varer gjennom hele hero-seksjonen.
        // Headeren er helt grønn når hero er scrollet forbi.
        const fadeDistance = Math.max(
            1,
            heroBottom - header.offsetHeight
        );

        // Gjennomsiktighet fra 0 til 1
        
        const progress = Math.min(1, Math.max(0, scrollY / fadeDistance));

// Headeren forblir transparent de første 25 %
        const fade = Math.min(1, Math.max(0, progress / 0.35));

// Myk overgang til mørkegrønn
        const opacity = fade * fade * (3 - 2 * fade);

        header.style.setProperty(
            '--header-opacity',
            opacity.toString()
        );

        ticking = false;
    }

    function requestUpdate() {
        if (!ticking) {
            requestAnimationFrame(updateHeader);
            ticking = true;
        }
    }

    updateHeader();

    window.addEventListener(
        'scroll',
        requestUpdate,
        { passive: true }
    );

    window.addEventListener('resize', requestUpdate);
})();



// --- 3. Parallakse ---
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
    }, { passive: true });
})();
// Profilmenyen lukkes ved klikk utenfor, tabbing ut eller Escape.
(function () {
    const menu = document.querySelector('.profile-menu');
    if (!menu) return;
    document.addEventListener('click', function (event) {
        if (!menu.contains(event.target)) menu.open = false;
    });
    menu.addEventListener('focusout', function (event) {
        if (!menu.contains(event.relatedTarget)) menu.open = false;
    });
    document.addEventListener('keydown', function (event) {
        if (event.key === 'Escape' && menu.open) {
            menu.open = false;
            menu.querySelector('summary').focus();
            event.stopImmediatePropagation();
        }
    }, true);
})();
