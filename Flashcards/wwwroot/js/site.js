// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Set color mode
let colorMode = localStorage.getItem('color-mode') ?? 'light'
document.querySelector('html').setAttribute('data-bs-theme', colorMode)
