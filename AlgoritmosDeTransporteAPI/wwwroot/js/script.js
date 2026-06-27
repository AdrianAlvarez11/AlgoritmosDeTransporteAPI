document.addEventListener('DOMContentLoaded', function() {

    const label_nav = document.getElementById('label_opciones');
    const nav = document.querySelector('nav');

    window.addEventListener('click', function(e) {
        if (label_nav.contains(e.target)) {
            nav.classList.toggle('invisible');
        }
        else if (!nav.classList.contains('invisible')) {
            const clicEnOpcion = e.target.closest('button') || e.target.closest('a');
            const clicFueraDelNav = !nav.contains(e.target);

            if (clicEnOpcion || clicFueraDelNav) {
                nav.classList.add('invisible');
            }
        }
    });



});