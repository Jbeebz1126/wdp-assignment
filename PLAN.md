# PLAN.md — 4-Week Build Plan

**Unit:** Web Design & Programming (7175), Assessment 3 (45 marks)
**Theme:** AI in Healthcare
**Deadline:** Week 11, Friday 11:59 pm — aim to submit 1–2 days early.
**Budget:** ~6 hours/week (~24 hrs total). Scale up if you have more.
**Machines:** build on Windows (Visual Studio 2022) or VS Code; **present in VS Code on the MacBook**. Database is SQLite so everything is portable.

Guiding principle: get the plumbing and content right first, style second, then
polish and rehearse. End every week with a `git pull` + `dotnet run` on the Mac.

Grading traps to keep in mind all month:
- External CSS/JS only — no inline `style=""`, no internal `<style>`/`<script>` code.
- Unhandled runtime exception = **−2 marks each**.
- Chrome and Edge must look identical = **−2 marks** if not.
- All media your own / copyright-free; every source credited on the Contact page.

---

## WEEK 1 — Foundation, portability & content

Goal: a portable, running skeleton with all your content gathered.

### Objectives
- [ ] MVC project created with Individual Authentication; builds and runs.
- [ ] Database confirmed/converted to **SQLite**; committed to git.
- [ ] Runs on the MacBook from a fresh clone (portability proven early).
- [ ] `_Layout.cshtml` shell: menu + Font Awesome footer, full-width, no gaps.
- [ ] External `site.css` and `site.js` created and confirmed loading.
- [ ] All media gathered + all block text written (off-code, in parallel).

### Guide
1. In the project folder terminal, run `dotnet new mvc --auth Individual -o .`,
   then `dotnet new gitignore` and `git init`.
2. Open `Program.cs` / `appsettings.json` and confirm `UseSqlite` +
   `"DataSource=app.db"`. If it says SQL Server/LocalDB, convert per CLAUDE.md §4.
3. `dotnet tool install --global dotnet-ef`, then `dotnet ef database update` to
   create `app.db`. Run `dotnet dev-certs https --trust`.
4. `dotnet watch run`, open the localhost URL, and confirm Register/Login work.
5. Build `_Layout.cshtml`: brand (GenAI/your name), menu links **Home, AI Images,
   Contact**, and a footer with **5 Font Awesome icons** (Home, AI Images, Contact
   share the menu links). Make the layout full-width — no side gaps, no gap between
   menu and content.
6. Create `wwwroot/css/site.css` and `wwwroot/js/site.js`, link them in the layout,
   and add a temporary test rule/log to confirm both load in dev tools.
7. Commit, push, then clone fresh on the Mac and `dotnet run` to prove portability.

**Content to gather this week (AI in Healthcare):**
- Hero: heading "AI in Healthcare" + a 2–3 sentence intro (your words) + attribution.
- Block 1 — **AI-Powered Diagnostics**: Aidoc (https://www.aidoc.com), Viz.ai (https://www.viz.ai)
- Block 2 — **Virtual Health Assistants**: Ada Health (https://ada.com), Symptomate (https://symptomate.com)
- Block 3 — **Predictive Analytics in Medicine**: Tempus (https://www.tempus.com), Merative (https://www.merative.com)
- For each block: your paragraph + a "Read more" link + 2 sub-images each with a
  hover caption (your words) and the external link above.
- Media: 1 background image + 6 sub-item images (copyright-free / conceptual, NOT
  company logos). Record every source URL for the Contact credits.

---

## WEEK 2 — Home page & data layer

Goal: the Home page matches the brief on desktop, and CRUD works.

### Objectives
- [ ] Parallax fixed-background sections working.
- [ ] Gradient hero heading (Anton) + intro text styled per the brief specs.
- [ ] Three healthcare blocks built with hover captions + external links.
- [ ] `site.js`: blocks appear only when **fully** in the viewport; hover effects;
      at least one HTML element generated via JS.
- [ ] `AIImage` model added, migrated; five CRUD views scaffolded.

### Guide
1. Add the parallax background: a fixed background image section with
   `background-attachment: fixed` (in `site.css`), so it stays put on scroll.
2. Style the hero heading in `site.css` using the brief's values (font-family
   `'Anton'`, the gradient `background-image`, `-webkit-background-clip: text`,
   transparent fill, stroke, `font-size: 7vw`, etc.). Import Anton from Google Fonts.
3. Build the three blocks. Each: an `h3` heading, your paragraph, a "Read more"
   anchor, and two square sub-blocks (image + hover caption + external `<a>`).
   Apply the square-block CSS (`font: normal 1.2vw arial;` and `:hover { opacity:
   0.9; transition: 2s ease; }`).
4. In `site.js`, use an **IntersectionObserver** with `threshold: 1.0` so each
   block reveals only when fully visible. Add mouseover/mouseout handlers for the
   hover text, and generate at least one HTML5 element in JS (the brief requires it).
5. Add `Models/AIImage.cs` **exactly** as given in the brief (CLAUDE.md §6). Create
   the DbContext entry, then `dotnet ef migrations add AddAIImage` and
   `dotnet ef database update`.
6. Scaffold the controller + the five views (Index, Create, Details, Edit, Delete)
   from the `AIImage` model. Confirm basic create/list works before styling.

---

## WEEK 3 — Auth, features & remaining pages

Goal: every functional requirement works; all pages exist.

### Objectives
- [ ] Roles enforced: visitor / member / admin; admin account seeded.
- [ ] Upload saves to DB + `wwwroot/uploads/`; UploadDate auto + read-only; Like read-only.
- [ ] Like button works; lists sorted by Like **descending** (Home + AIImages);
      one like per session; page scrolls the liked block to the top.
- [ ] Anchor links jump from a Home block to the matching AIImages block.
- [ ] Contact page (3 Bootstrap columns) with credits + links.
- [ ] Creative extra page built (responsive, external CSS/JS, linked from menu + footer).

### Guide
1. Set up roles. Seed an **Admin** user on startup; create the "Admin" role.
   Apply `[Authorize(Roles="Admin")]` to Edit/Delete, `[Authorize]` to Create.
   Show Like/Edit/Delete buttons by role in the view (admin: all three;
   member: Like only; visitor: none).
2. On the Home "Upload AI images" block: show "Please login or register…" for
   visitors, and "Click here to upload a new AI image" (linking to Create) once
   logged in.
3. Create page: Prompt as a 5-line `textarea`; ImageGenerator; UploadDate set to
   `DateTime.Now` and **read-only**; Like **read-only**; file input. On submit,
   save the record and copy the uploaded file into `wwwroot/uploads/`. Wrap file
   handling in try/catch.
4. Like feature: increment `Like`, then order both lists by `Like` descending.
   Use `canIncreaseLike` / a session flag so a user can only like once per session.
   After a like, scroll the block to the top (JS).
5. Anchor links: give each AIImages block an `id`; link Home blocks to
   `/AIImages#that-id`.
6. Contact page: Bootstrap 3-column row — **Contact Us**, **About Us**, **Copyright**.
   Same hero heading design as Home but **without** the text-stroke properties.
   Same parallax background. Fill in the credits (Unsplash/your sources, Font
   Awesome, Google Fonts) with links.
7. Creative extra page: your own design, responsive, external CSS/JS only, linked
   from **both** menu and footer, consistent look with the rest of the site.

---

## WEEK 4 — Responsive, testing, polish & submit

Goal: responsive on all breakpoints, tested, submitted early, demo rehearsed.
Split the week: **Days 1–3 finish**, **Days 4–5 demo prep**.

### Objectives (Days 1–3 — finish)
- [ ] Responsive at all three breakpoints: ≥768px, 576–768px, <576px.
- [ ] Hamburger menu at the small breakpoint; media queries adjust font sizes.
- [ ] AIImages list: max 6 blocks/row with equal spacing (desktop); stacks on mobile.
- [ ] Exception handling everywhere (file upload, DB, parsing) — none unhandled.
- [ ] Chrome vs Edge checked side-by-side — identical.
- [ ] Contact credits complete; placeholders like `[your name]` replaced.
- [ ] Final requirements checklist pass. **Zip and submit 1–2 days early.**

### Objectives (Days 4–5 — demo prep)
- [ ] Demo rehearsed in VS Code on the MacBook (full click-path).
- [ ] Backup screen-recording of the working site made.
- [ ] Q&A answers prepared for the parts examiners probe.

### Guide
1. Add media queries in `site.css` for the three viewport bands. Test using
   Chrome/Edge dev tools "Responsive" mode at 767, 700, and 500 px widths.
2. Make the navbar collapse to a hamburger below 576 px (Bootstrap navbar toggler).
   Scale hero/heading font sizes down via media queries so nothing overflows.
3. AIImages grid: up to 6 per row with equal gaps on desktop; use Bootstrap/Grid
   so it reflows to fewer columns, then a single stacked column on phones.
4. Walk every risky path and wrap it in try/catch with a graceful message: file
   upload, missing file, DB save/read, and any parsing. Test by deliberately
   submitting bad input.
5. Open the site in Chrome and Edge at the same width and compare pixel-for-pixel;
   fix any differences.
6. Final pass: re-read the brief's requirements list and tick each one. Confirm
   `wwwroot/uploads/` images and the SQLite `.db` are committed so the marker (and
   your Mac demo) see the same data. Zip the whole project and submit.
7. Demo rehearsal on the Mac: `dotnet run` → register/login → admin CRUD → Like +
   re-sort → responsive breakpoints in dev tools → the creative page. Know where
   each file lives so you can jump to it on request. Record a backup video.

---

## Demo Q&A — likely questions to prepare
- How does the "appears only when fully in viewport" effect work? (IntersectionObserver, threshold 1.0)
- How are the three roles enforced, and where? (Identity + `[Authorize]` attributes + role-based view logic)
- How does the Like sort and one-like-per-session logic work?
- How are uploaded files stored, and how is UploadDate set/kept read-only?
- Why SQLite, and how does the data travel between machines?
- Where is your CSS/JS, and how did you keep it fully external?

If Claude Code wrote any of these, make sure you can explain them in your own words.

---

## Weekly ritual (every week)
1. Commit after each working feature (small commits).
2. `git pull` + `dotnet run` on the MacBook — the demo machine stays current.
3. Update CLAUDE.md §9 "current focus" to the next week's objectives.

---

## Day-by-day schedule (24 days)

One focused objective per day. If a day slips, push it into the next buffer day
(6, 13, 20) rather than letting it cascade. Commit to git at the end of every
build day. Submission is Day 23 so Day 24 is pure demo prep + safety margin.

### Phase 1 — Foundation & content (Days 1–6)
- [ ] **Day 1** — Create project (`dotnet new mvc --auth Individual -o .`), confirm SQLite in `appsettings.json`, `dotnet ef database update`, run, verify Register/Login.
- [ ] **Day 2** — Git + portability: `dotnet new gitignore`, commit, push to GitHub; `dotnet dev-certs https --trust`; add CLAUDE.md + PLAN.md to repo.
- [ ] **Day 3** — Layout shell: `_Layout.cshtml` menu + Font Awesome footer, `site.css` full-width/nav/footer rules, confirm `site.js` loads.
- [ ] **Day 4** — Gather media: 1 background + 6 sub-item images (copyright-free, conceptual, no logos). Log every source URL.
- [ ] **Day 5** — Write all copy in your own words: hero intro, 3 block paragraphs, 6 hover captions; confirm external links resolve.
- [ ] **Day 6** — Buffer + start hero: parallax fixed-background section working.

### Phase 2 — Home page & data layer (Days 7–13)
- [ ] **Day 7** — Style hero: Anton gradient heading + intro text per brief specs.
- [ ] **Day 8** — Block 1 (AI-Powered Diagnostics): heading, paragraph, 2 square sub-blocks with images, hover captions, links.
- [ ] **Day 9** — Block 2 (Virtual Health Assistants): same structure.
- [ ] **Day 10** — Block 3 (Predictive Analytics in Medicine): same structure.
- [ ] **Day 11** — `site.js`: IntersectionObserver (fully-in-viewport reveal) + hover mouseover/mouseout effects.
- [ ] **Day 12** — `site.js`: generate an HTML5 element via JS; build "Upload AI images" block (login/register prompt for visitors).
- [ ] **Day 13** — Data layer: add `AIImage` model exactly, wire DbContext, `migrations add`, `database update`. Buffer.

### Phase 3 — Auth, features & remaining pages (Days 14–19)
- [ ] **Day 14** — Scaffold AIImages controller + all five views; confirm basic create/list.
- [ ] **Day 15** — Roles: seed admin, create Admin role, apply `[Authorize]`/`[Authorize(Roles="Admin")]`, show buttons by role.
- [ ] **Day 16** — Create page: 5-line Prompt textarea, ImageGenerator, auto+read-only UploadDate, read-only Like, upload to `wwwroot/uploads/` (try/catch).
- [ ] **Day 17** — Like feature: increment, sort by Like descending (Home + AIImages), one like per session, scroll liked block to top.
- [ ] **Day 18** — Anchor links (Home block → AIImages block) + style AIImages Index (max 6/row, heading design).
- [ ] **Day 19** — Contact page: 3 Bootstrap columns (Contact Us / About Us / Copyright), heading without stroke, parallax background, credits + links.

### Phase 4 — Responsive, testing, polish & submit (Days 20–24)
- [ ] **Day 20** — Creative extra page (own design, responsive, external CSS/JS, linked from menu + footer). Buffer.
- [ ] **Day 21** — Responsive: media queries for all three breakpoints (≥768, 576–768, <576), hamburger menu, font scaling.
- [ ] **Day 22** — Exception handling everywhere (upload, DB, parsing); Chrome vs Edge parity check.
- [ ] **Day 23** — Final pass: complete credits, replace placeholders, requirements checklist, then **zip and submit**.
- [ ] **Day 24** — Demo prep: rehearse full click-path on the Mac, record backup video, prep Q&A answers.
