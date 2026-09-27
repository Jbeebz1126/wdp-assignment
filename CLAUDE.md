# CLAUDE.md — Web Design & Programming (7175) Assessment 3

This is the project context for an assessed **individual** university assignment
(University of Canberra, Faculty of Science and Technology). Read this before
making any changes. The rules below are graded, so treat them as hard constraints,
not suggestions. When unsure, ask me rather than guessing — I need to understand
and be able to explain everything in a live demo and Q&A.

---

## 1. What this project is

A responsive ASP.NET Core MVC website introducing an AI theme, with role-based
access, an image-upload CRUD feature backed by a database, and a specific layout
that must match the provided brief.

**Theme:** AI in Healthcare

All media and text must be my own (copyright-free or self-made). Do NOT invent or
insert placeholder images/text from the sample brief — I supply the real content.

**Home-page content plan (three blocks, mirroring the sample structure).**
Each block = heading + my paragraph + a "Read more" link + two sub-images, each
with a hover caption and its own external link.

- Block 1 — **AI-Powered Diagnostics**: sub-items Aidoc (https://www.aidoc.com)
  and Viz.ai (https://www.viz.ai).
- Block 2 — **Virtual Health Assistants**: sub-items Ada Health (https://ada.com)
  and Symptomate by Infermedica (https://symptomate.com).
- Block 3 — **Predictive Analytics in Medicine**: sub-items Tempus
  (https://www.tempus.com) and Merative / formerly IBM Watson Health
  (https://www.merative.com).

The gallery images I upload via the Create page will be my own AI-generated
medical illustrations / concept images (satisfies "created by you"); the Prompt
and ImageGenerator fields record how each was made. Section 8 block names below
refer to the OLD sample (Chatbots/Image Generator/Companies) — use the three
blocks above instead.

---

## 2. Non-negotiable rules (these are graded)

- **External CSS only.** All styles live in `wwwroot/css/site.css` (more `.css`
  files allowed). NO inline styles (`style="..."`) and NO internal `<style>` blocks. Ever.
- **External JS only.** All script lives in `wwwroot/js/site.js`. NO inline
  handlers where avoidable and NO internal `<script>` code blocks.
- **Full-width layout.** No blank gaps on the sides, or between the menu and the
  page body.
- **Handle all runtime exceptions** (−2 marks each unhandled). Wrap risky code
  (file upload, DB access, parsing) in try/catch and fail gracefully.
- **Chrome/Edge parity** (−2 marks if they differ). Keep CSS/JS cross-browser safe.
- **Credit all sources** on the Contact page (media, Font Awesome, Google Fonts).
- Use the `AIImage` model in section 6 **exactly** as written.

If a request would break any of these rules, stop and flag it instead of complying.

---

## 3. Tech stack

- ASP.NET Core Web App (**MVC**), C#, .NET SDK
- Entity Framework Core
- **SQLite** for the database (NOT SQL Server / LocalDB — see section 4)
- ASP.NET Core **Identity** (Individual Authentication) for Register/Login + roles
- Bootstrap 5, Flexbox, CSS Grid, and CSS media queries for responsiveness
- Font Awesome (footer icons), Google Fonts (Anton, etc.)

**Why SQLite:** I build on a Windows PC but must present in **VS Code on a MacBook**.
LocalDB is Windows-only. SQLite is cross-platform and its `.db` file travels with
the repo, so the same data (uploaded images, Like counts) shows up on both machines.
Portability is a hard requirement — never reintroduce a Windows-only dependency.

---

## 4. Critical setup: convert the project to SQLite (do first)

The default MVC + Individual Auth template uses SQL Server. Convert it before
building features:

1. Add the package: `dotnet add package Microsoft.EntityFrameworkCore.Sqlite`
2. In `Program.cs`, change `options.UseSqlServer(connectionString)` to
   `options.UseSqlite(connectionString)`.
3. In `appsettings.json`, set the connection string to a file, e.g.
   `"DefaultConnection": "Data Source=app.db"`.
4. Delete the auto-generated `Migrations/` folder (it was built for SQL Server).
5. Ensure EF tools are available: `dotnet tool install --global dotnet-ef`
   (and the `Microsoft.EntityFrameworkCore.Design` package, usually already present).
6. Regenerate: `dotnet ef migrations add InitialCreate` then `dotnet ef database update`.
7. Confirm the app builds and runs, then commit.

**Verify portability every week:** fresh `git clone` on the Mac → `dotnet restore`
→ `dotnet run` should work with no manual fixes. Make sure `wwwroot/uploads/` and
the SQLite `.db` file are committed so images and Like counts appear on both machines.

---

## 5. Required file/folder structure

- `Views/Home/Index.cshtml` — home page (menu links: **GenAI** and **Home**)
- `Views/Home/Contact.cshtml` — Contact & About (3 Bootstrap columns)
- `Views/AIImages/` — `Index`, `Create`, `Details`, `Edit`, `Delete` (scaffolded from `AIImage`)
- `Views/Shared/_Layout.cshtml` — head, menu, footer (shared shell)
- Register / Login — from the Identity scaffold
- **One creative extra page** of my own design: responsive, external CSS/JS only,
  linked from BOTH the menu and footer, consistent look with the rest of the site.
- Menu and footer both link to Index, AIImages, and Contact. Footer uses 5 Font
  Awesome icons.

---

## 6. The AIImage model (use verbatim)

```csharp
public class AIImage
{
    public AIImage()
    {
        Prompt = "";
        ImageGenerator = "";
        Filename = "";
    }

    [Required]
    public int Id { get; set; }

    [Required]
    public string Prompt { get; set; }

    [Required]
    [Display(Name = "Image Generator")]
    public string ImageGenerator { get; set; }

    [Display(Name = "Upload Date")]
    public DateTime UploadDate { get; set; }

    public string Filename { get; set; }

    public int Like { get; set; }

    public bool canIncreaseLike { get; set; }
}
```

File location: `Models/AIImage.cs`.

---

## 7. Roles & access rules

- **Visitor** (not logged in): `Views/Home/` pages, Register, Login.
- **Member** (logged in, no role): the above + `AIImages/Create`.
- **Admin** (logged in, admin role): all pages, including Edit and Delete.

Seed an admin account. Apply `[Authorize(Roles = "Admin")]` to Edit/Delete and
`[Authorize]` to Create. On the AIImages list, only admin sees Like/Edit/Delete
buttons; members see only Like; visitors see no buttons.

---

## 8. Feature notes (for later weeks — context only)

- Home page: CSS parallax fixed background; `Anton` gradient heading; blocks
  (AI Chatbots / Image Generator / Companies) with hover captions + external links.
- `site.js`: `IntersectionObserver` (threshold 1.0) so a block appears only when
  **fully** in the viewport; hover/mouseout effects; JS-generated HTML elements.
- Upload: save files to `wwwroot/uploads/`, auto-set `UploadDate` to now;
  `UploadDate` and `Like` are read-only on forms.
- Like: increment, then **sort the list by Like descending** on both Home and
  AIImages; one like per session (log off/in to like again).
- Anchor links jump from a Home block to the matching AIImages block.

Exact CSS specs (fonts, sizes, shadows, gradients) come from the brief PDF — ask me
for the values when building a specific block rather than approximating.

---

## 9. CURRENT FOCUS — Week 1 (budget: ~6 hours this week)

Goal: a portable, running skeleton with content gathered. Plumbing before polish.

- [ ] Create the MVC project with Individual Authentication; confirm it builds/runs.
- [ ] **Convert to SQLite** per section 4; commit; push to git.
- [ ] Clone fresh on the Mac; `dotnet restore` + `dotnet run` works from clean clone.
- [ ] Build `_Layout.cshtml`: menu (GenAI, Home, AI Images, Contact) + Font Awesome
      footer; full-width, no gaps.
- [ ] Wire up external `wwwroot/css/site.css` and `wwwroot/js/site.js`; confirm both
      load (no inline/internal CSS or JS).
- [ ] (In parallel, off-code) gather all copyright-free media + write my own block
      text; record every source URL for the Contact credits.
- [ ] End of week: commit, `git pull` on the Mac, `dotnet run` there once more.

Do NOT start Home-page styling or CRUD features until the skeleton runs on both
machines. For this week, help me scaffold and explain — I want to understand each
piece for the Q&A.

---

## 10. Working conventions

- Commit to git after each working feature; keep commits small.
- After changes, run `dotnet build` and fix compile errors before handing back.
- Explain what you changed and why, in plain terms — I may be asked about it live.
- Prefer standard, cross-browser CSS/JS; avoid anything that renders differently in
  Chrome vs Edge.
- Never add a Windows-only dependency or move data out of the portable SQLite file.
