/// Felles meny- og scrolleffekter. Profilmenyen kan også lukkes med tastatur.
﻿

(function () {
    const toggle = document.querySelector('.nav-toggle');
    const nav = document.getElementById('main-nav');
    if (!toggle || !nav) return;

    toggle.addEventListener('click', function () {

        const open = nav.classList.toggle('is-open');

        toggle.setAttribute('aria-expanded', String(open));
        toggle.setAttribute('aria-label', open ? 'Lukk meny' : 'Åpne meny');
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && nav.classList.contains('is-open')) {
            nav.classList.remove('is-open');
            toggle.setAttribute('aria-expanded', 'false');
            toggle.setAttribute('aria-label', 'Åpne meny');
            toggle.focus();
        }
    });
})();

(function () {
    const header = document.querySelector('.site-header--overlay');
    const hero = document.querySelector('.hero');

    if (!header || !hero) return;

    let ticking = false;

    function updateHeader() {
        const scrollY = window.scrollY;

        const heroBottom =
            hero.getBoundingClientRect().bottom + scrollY;

        const fadeDistance = Math.max(
            1,
            heroBottom - header.offsetHeight
        );

        const progress = Math.min(1, Math.max(0, scrollY / fadeDistance));

        const fade = Math.min(1, Math.max(0, progress / 0.35));

        const opacity = fade * fade * (3 - 2 * fade);

        header.style.setProperty(
            '--header-opacity',
            opacity.toString()
        );

        ticking = false;
    }

    /// Samler scrolloppdateringer til neste animasjonsramme for å unngå unødvendig arbeid.
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

(function () {

    const heroImg = document.querySelector('.hero [data-parallax]');
    if (!heroImg) return;

    const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (prefersReducedMotion) return;

    let ticking = false;

    /// Flytter bakgrunnsbildet langsommere enn innholdet for å gi dybde ved scrolling.
    function updateParallax() {

        heroImg.style.transform = 'translateY(' + (window.scrollY * 0.3) + 'px)';
        ticking = false;
    }

    window.addEventListener('scroll', function () {

        if (!ticking) {
            requestAnimationFrame(updateParallax);
            ticking = true;
        }
    }, { passive: true });
})();

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
