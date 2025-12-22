// Scroll Atas
function scrollToTop() {
  window.scrollTo({
    top: 0,
    behavior: "smooth",
  });
}

document.addEventListener("DOMContentLoaded", function () {
  const scrollTopBtn = document.getElementById("scrollTopBtn");

  // Scroll 200px
  window.addEventListener("scroll", function () {
    if (window.scrollY > 200) {
      scrollTopBtn.classList.add("show");
    } else {
      scrollTopBtn.classList.remove("show");
    }
  });

  scrollTopBtn.addEventListener("click", scrollToTop);
});
