function SetPreferredThemeDefault() {
    let mediaQuery = matchMedia("(prefers-color-scheme: dark)");
    document.documentElement.setAttribute("data-bs-theme", mediaQuery.matches ? "dark" : "light");
}

function SetPreferredThemeOverride(theme) {
    document.documentElement.setAttribute("data-bs-theme", theme);
}

function OverridePreferredTheme(theme) {
    document.removeEventListener("DOMContentLoaded", SetPreferredThemeDefault);

    if (document.readyState == "loading")
        document.addEventListener("DOMContentLoaded", () => { SetPreferredThemeOverride(theme) });
    else
        SetPreferredThemeOverride(theme)
}

if (document.readyState == "loading")
    document.addEventListener("DOMContentLoaded", SetPreferredThemeDefault)
else
    SetPreferredThemeDefault()
