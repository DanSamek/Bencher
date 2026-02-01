const THEME_KEY = 'theme';
const LIGHT_THEME_VALUE = 'light';
const DARK_THEME_VALUE = 'dark';

function getTheme() {
    const theme = localStorage.getItem(THEME_KEY);
    return theme == null ? LIGHT_THEME_VALUE : theme;
}

function loadTheme(theme){
    document.documentElement.setAttribute('data-bs-theme', theme);
    document.documentElement.classList = [];
    document.documentElement.classList.add(theme);
}

function displayCorrectTheme(theme) {
    const themeSelectors = document.querySelectorAll(".theme-selector");
    themeSelectors.forEach(themeSelector => {
        const light = themeSelector.querySelector('.light');
        const dark = themeSelector.querySelector('.dark');

        const config = theme === LIGHT_THEME_VALUE
            ? [dark, light]
            : [light, dark];
        
        config[1].style.display = 'none';
        config[0].style.display = 'block';  
    })
}

function setTheme(theme) {
    localStorage.setItem(THEME_KEY, theme);
    loadTheme(theme);
    displayCorrectTheme(theme);
}

function ensureReloadRedirect(path){
    window.location = path;
}

loadTheme(getTheme()); // ensures no flicking light -> dark when loading.


const editorMap = new Map();
function registerMonacoEditor(elementId, language, initialValue, readonly) {
    require.config({ paths: { 'vs': 'lib/monaco-editor/min/vs' }});
    require(['vs/editor/editor.main'], function() {
        const theme = getTheme() === LIGHT_THEME_VALUE ? 'vs' : 'vs-dark';
        const editor =  monaco.editor.create(document.getElementById(elementId),{
            value: initialValue,
            language: language,
            automaticLayout: true,
            theme: theme,
            readOnly: readonly
        });
        editorMap.set(elementId, editor);
    });
}

function getMonacoEditorText(elementId){
    const result = editorMap.get(elementId).getValue();
    return result;
}

// https://learn.microsoft.com/en-us/aspnet/core/blazor/file-downloads?view=aspnetcore-9.0
async function downloadFile(fileName, contentStreamReference) {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();
    URL.revokeObjectURL(url);
}
