function SetPreferredTheme() {
    let mediaQuery = matchMedia("(prefers-color-scheme: dark)")
    document.documentElement.setAttribute("data-bs-theme", mediaQuery.matches ? "dark" : "light" )
}

if (document.readyState == "loading")
    document.addEventListener("DOMContentLoaded", SetPreferredTheme)
else
    SetPreferredTheme()