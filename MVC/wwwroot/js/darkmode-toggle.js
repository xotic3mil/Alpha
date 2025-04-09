document.addEventListener('DOMContentLoaded', function () {
    applyDarkModePreference();

    const darkModeToggle = document.getElementById('darkModeToggle');
    if (!darkModeToggle) return; 


    darkModeToggle.checked = isDarkModeEnabled();


    darkModeToggle.addEventListener('change', function () {
        if (darkModeToggle.checked) {
            enableDarkMode();
        } else {
            disableDarkMode();
        }
    });


    checkSystemPreference();
});

function isDarkModeEnabled() {
    return localStorage.getItem('darkMode') === 'true';
}

function enableDarkMode() {
    document.body.classList.add('dark-mode');
    localStorage.setItem('darkMode', 'true');


    document.dispatchEvent(new CustomEvent('darkModeChange', { detail: { darkMode: true } }));
}

function disableDarkMode() {
    document.body.classList.remove('dark-mode');
    localStorage.setItem('darkMode', 'false');

    document.dispatchEvent(new CustomEvent('darkModeChange', { detail: { darkMode: false } }));
}


function applyDarkModePreference() {
    if (localStorage.getItem('darkMode') === 'true') {
        document.body.classList.add('dark-mode');
    }
}


function checkSystemPreference() {
    if (localStorage.getItem('darkMode') === null) {
        const prefersDarkMode = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
        if (prefersDarkMode) {
            enableDarkMode();
            const darkModeToggle = document.getElementById('darkModeToggle');
            if (darkModeToggle) darkModeToggle.checked = true;
        }
    }
    if (window.matchMedia) {
        window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
            if (localStorage.getItem('darkMode') === null) {
                if (e.matches) {
                    enableDarkMode();
                } else {
                    disableDarkMode();
                }

                const darkModeToggle = document.getElementById('darkModeToggle');
                if (darkModeToggle) darkModeToggle.checked = e.matches;
            }
        });
    }
}