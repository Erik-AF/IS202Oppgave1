// Filtrerer behovene som allerede er hentet fra serveren.
(() => {
    const filter = document.querySelector('[data-need-filter]');
    if (!filter) return;
    const search = filter.querySelector('input[type="search"]');
    const priority = filter.querySelector('select');
    const items = [...document.querySelectorAll('[data-need-item]')];
    const count = document.getElementById('need-count');
    const empty = document.getElementById('need-no-matches');
    function update() {
        const query = search.value.trim().toLocaleLowerCase('nb-NO');
        let visible = 0;
        for (const item of items) {
            const matches = item.textContent.toLocaleLowerCase('nb-NO').includes(query)
                && (!priority.value || item.dataset.priority === priority.value);
            item.hidden = !matches;
            if (matches) visible++;
        }
        count.textContent = visible + ' behov funnet';
        empty.hidden = visible > 0 || items.length === 0;
    }
    filter.addEventListener('submit', event => { event.preventDefault(); update(); });
    search.addEventListener('input', update);
    priority.addEventListener('change', update);
    filter.addEventListener('reset', () => requestAnimationFrame(update));
})();
