document.addEventListener("DOMContentLoaded", function () {
    const toggleDarkMode = document.getElementById("toggleDarkMode");

    toggleDarkMode.addEventListener("click", function () {
        document.body.classList.toggle("dark-mode");
    });
});
