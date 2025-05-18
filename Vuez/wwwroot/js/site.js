// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function showFileName() {
    const input = document.getElementById('file');
    const fileName = input.files[0] ? input.files[0].name : "Žiadny súbor nebol vybraný";
    document.getElementById('file-name').textContent = fileName;
}


