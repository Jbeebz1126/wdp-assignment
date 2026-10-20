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



// Compare AI Tools page: filter buttons and side-by-side comparison
document.addEventListener("DOMContentLoaded", function () {
    const filterButtons = document.querySelectorAll(".filter-btn");
    const toolCards = document.querySelectorAll(".tool-card");

    // Filter: show only the cards in the chosen category
    filterButtons.forEach(function (btn) {
        btn.addEventListener("click", function () {
            const category = btn.dataset.filter;
            filterButtons.forEach(function (b) { b.classList.remove("active"); });
            btn.classList.add("active");
            toolCards.forEach(function (card) {
                const show = category === "all" || card.dataset.category === category;
                card.classList.toggle("hidden-card", !show);
            });
        });
    });

    const selectA = document.getElementById("compareA");
    const selectB = document.getElementById("compareB");
    const result = document.getElementById("compareResult");
    if (!selectA || !selectB || !result) {
        return; // not on the Compare page
    }

    // Fill both dropdowns from the cards (JavaScript creating HTML elements)
    toolCards.forEach(function (card) {
        [selectA, selectB].forEach(function (select) {
            const option = document.createElement("option");
            option.value = card.id;
            option.textContent = card.dataset.name;
            select.appendChild(option);
        });
    });
    if (selectB.options.length > 1) {
        selectB.selectedIndex = 1;
    }

    // Build the comparison table for the two chosen tools
    function updateComparison() {
        const cardA = document.getElementById(selectA.value);
        const cardB = document.getElementById(selectB.value);
        if (!cardA || !cardB) {
            return;
        }

        const rows = [
            ["Category", "categoryLabel"],
            ["Main focus", "focus"],
            ["Used by", "users"],
            ["Access", "access"]
        ];

        const table = document.createElement("table");
        table.className = "compare-table";

        const headRow = table.createTHead().insertRow();
        ["", cardA.dataset.name, cardB.dataset.name].forEach(function (text) {
            const th = document.createElement("th");
            th.textContent = text;
            headRow.appendChild(th);
        });

        const body = table.createTBody();
        rows.forEach(function (row) {
            const tr = body.insertRow();
            const label = document.createElement("th");
            label.textContent = row[0];
            tr.appendChild(label);
            [cardA, cardB].forEach(function (card) {
                const td = tr.insertCell();
                td.textContent = card.dataset[row[1]];
            });
        });

        result.replaceChildren(table);
    }

    selectA.addEventListener("change", updateComparison);
    selectB.addEventListener("change", updateComparison);
    updateComparison();
});