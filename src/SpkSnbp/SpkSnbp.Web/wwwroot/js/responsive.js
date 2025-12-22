const menuBtn = document.getElementById('menu-btn');
const menuClose = document.getElementById('menu-close');
const navUl = document.querySelector('nav .navigation ul');
const submenuParents = document.querySelectorAll('nav .navigation ul li.has-submenu');

// Buka menu
menuBtn.addEventListener('click', () => {
    navUl.classList.add('active');
});

// Tutup menu
menuClose.addEventListener('click', () => {
    navUl.classList.remove('active');
});

// Toggle submenu di mobile
submenuParents.forEach(item => {
    item.addEventListener('click', () => {
        item.classList.toggle('active');
    });
});