document.addEventListener('DOMContentLoaded', function () {
    applyThemePreference();
    setupThemeOptions();
    setupReducedMotionToggle();
});

function applyThemePreference() {
    const theme = localStorage.getItem('theme') || 'light';
    const reduceMotion = localStorage.getItem('reduceMotion') === 'true';

    document.documentElement.setAttribute('data-theme', theme);

    if (reduceMotion) {
        document.body.classList.add('reduce-motion');
    }

    const activeThemeRadio = document.querySelector(`input[name="theme"][value="${theme}"]`);
    if (activeThemeRadio) {
        activeThemeRadio.checked = true;
        highlightActiveTheme(theme);
    }

    const reduceMotionToggle = document.getElementById('reduceMotionToggle');
    if (reduceMotionToggle) {
        reduceMotionToggle.checked = reduceMotion;
    }

    dispatchThemeChangeEvent(theme);
}

function setupThemeOptions() {

    const themeRadios = document.querySelectorAll('input[name="theme"]');
    themeRadios.forEach(radio => {
        radio.addEventListener('change', function () {
            const theme = this.value;

            document.body.classList.add('theme-transition');

            setTheme(theme);

            setTimeout(() => {
                document.body.classList.remove('theme-transition');
            }, 1000);
        });


        const themeOption = radio.closest('.theme-option');
        if (themeOption) {
            themeOption.addEventListener('click', function () {
                const input = this.querySelector('input[type="radio"]');
                if (input) {
                    input.checked = true;
                    input.dispatchEvent(new Event('change'));
                }
            });
        }
    });


    highlightActiveTheme(localStorage.getItem('theme') || 'light');
}


function setupReducedMotionToggle() {
    const reduceMotionToggle = document.getElementById('reduceMotionToggle');

    if (reduceMotionToggle) {
        reduceMotionToggle.addEventListener('change', function () {
            if (this.checked) {
                document.body.classList.add('reduce-motion');
                localStorage.setItem('reduceMotion', 'true');
            } else {
                document.body.classList.remove('reduce-motion');
                localStorage.setItem('reduceMotion', 'false');
            }

            if (typeof snackbar !== 'undefined') {
                snackbar.info('Motion preference updated');
            }
        });
    }
}

function setTheme(theme) {
    document.documentElement.setAttribute('data-theme', theme);
    localStorage.setItem('theme', theme);

    highlightActiveTheme(theme);
    dispatchThemeChangeEvent(theme);

    if (typeof snackbar !== 'undefined') {
        snackbar.info(`Theme updated to ${theme}`);
    }
}


function highlightActiveTheme(theme) {
    document.querySelectorAll('.theme-option').forEach(option => {
        option.classList.remove('active');
    });

    const activeOption = document.querySelector(`.theme-option[data-theme="${theme}"]`);
    if (activeOption) {
        activeOption.classList.add('active');
    }
}


function dispatchThemeChangeEvent(theme) {
    document.dispatchEvent(new CustomEvent('themeChange', {
        detail: { theme: theme }
    }));
}


function checkSystemPreference() {
    if (localStorage.getItem('theme') === null) {
        const prefersDarkMode = window.matchMedia &&
            window.matchMedia('(prefers-color-scheme: dark)').matches;

        setTheme(prefersDarkMode ? 'dark' : 'light');
    }

    if (window.matchMedia) {
        window.matchMedia('(prefers-color-scheme: dark)')
            .addEventListener('change', e => {
                if (localStorage.getItem('theme') === null) {
                    setTheme(e.matches ? 'dark' : 'light');
                }
            });
    }
}