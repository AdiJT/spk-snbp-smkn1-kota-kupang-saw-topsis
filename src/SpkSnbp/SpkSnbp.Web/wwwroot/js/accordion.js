const accordionItems = document.querySelectorAll(".kurikulum-item");

accordionItems.forEach((item) => {
  const accordionHeader = item.querySelector(".kurikulum-header");

  accordionHeader.addEventListener("click", () => {
    toggleItem(item);
  });
});

const toggleItem = (item) => {
  const accordionContent = item.querySelector(".kurikulum-poin");

  if (item.classList.contains("accordion-open")) {
    accordionContent.removeAttribute("style");
    item.classList.remove("accordion-open");
  } else {
    accordionContent.style.height = accordionContent.scrollHeight + "px";
    item.classList.add("accordion-open");
  }
};
