// Home page: reveal each content block only when it is fully in view.
// On small screens a block can be taller than the screen, so it also
// counts as "in view" once it fills most of the screen.
document.addEventListener("DOMContentLoaded", function () {
    const blocks = document.querySelectorAll(".reveal");
    if (blocks.length === 0) {
        return;
    }

    const observer = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            const fullyVisible = entry.intersectionRatio >= 0.99;
            const fillsScreen = entry.rootBounds !== null &&
                entry.intersectionRect.height >= entry.rootBounds.height * 0.9;
            entry.target.classList.toggle("visible", fullyVisible || fillsScreen);
        });
    }, { threshold: [0, 0.25, 0.5, 0.75, 0.9, 0.99, 1] });

    blocks.forEach(function (b) { observer.observe(b); });
});

// AI Images list: load each row's picture as a background image,
// so CSS can make it repeat across the row on small screens
document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".image-row-pic[data-bg]").forEach(function (el) {
        el.style.backgroundImage = "url('" + el.dataset.bg + "')";
    });
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