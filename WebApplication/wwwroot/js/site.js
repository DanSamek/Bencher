const THEME_KEY = 'theme';
const LIGHT_THEME_VALUE = 'light';

function setTheme(theme) {
    localStorage.setItem(THEME_KEY, theme);
    ensureTheme(theme);
}

function getTheme() {
    const theme = localStorage.getItem(THEME_KEY);
    return theme == null ? LIGHT_THEME_VALUE : theme;
}

function ensureTheme(theme) {
    document.documentElement.setAttribute('data-bs-theme', theme);
}

document.addEventListener("DOMContentLoaded", function (event) {
    ensureTheme(getTheme());
});

