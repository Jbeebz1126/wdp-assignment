document.addEventListener("DOMContentLoaded", function () {
    const blocks = document.querySelectorAll(".reveal");
    const observer = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            if (entry.isIntersecting) {
                entry.target.classList.add("visible");
            } else {
                entry.target.classList.remove("visible");
            }
        });
    }, { threshold: 1.0 });  // reveal only when the block is FULLY in view
    blocks.forEach(function (b) { observer.observe(b); });
});

// Create/Edit page: show the chosen image in the preview box
document.addEventListener("DOMContentLoaded", function () {
    const fileInput = document.getElementById("ImageFile");
    const preview = document.getElementById("previewImage");
    if (fileInput && preview) {
        fileInput.addEventListener("change", function () {
            if (fileInput.files && fileInput.files[0]) {
                preview.src = URL.createObjectURL(fileInput.files[0]);
            }
        });
    }
});

