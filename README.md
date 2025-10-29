# Mech Template Builder

A C# console app for interactively building hierarchical mech templates from a parts catalog. You can navigate a JSON-like tree, add parts, and see stats like weight limit and scale at a glance.

## Features
- Interactive console navigation through a mech's part tree
- Add parts from a data-driven catalog (`Parts.txt`)
- Root-only frame rule: exactly one Frame allowed at the root
- Category-driven selection when adding children (Frames hidden for children)
- Stats display per step: Weight Limit and current node's Scale
- Scale system with validation
  - Frames get their Scale from `Parts.txt` automatically (no prompt)
  - Non-frame parts are prompted to choose a Scale
  - A child part's Scale must be the same or smaller than its parent's Scale
- **Save and Load Templates**
  - Save your templates to the `Templates` folder
  - Load existing templates at startup or during navigation
  - Templates stored as readable JSON files

## Scale System
- Supported scales (smallest → largest):
  1. Personal(1)
  2. Vehicle(2)
  3. House(3)
  4. Building(4)
- Rules:
  - If a part definition in `Parts.txt` includes a `Scale` (e.g., Frames), that Scale is applied automatically.
  - If a part definition has no `Scale`, you'll be asked to choose from the valid scales that are ≤ the parent part's scale.
  - If a chosen Scale would exceed the parent's Scale, the addition is blocked with a message.
- Display:
  - The UI header shows the current node's Scale.
  - Each listed part shows its `Scale` alongside its name and PartID.

## Weight Limit
- Computed via `StatCalc.GetWeightLimit()` from the template's root Frame name.
- Example mapping (can be adjusted in `StatCalc.cs`):
  - Exosuit → 2000
  - DemiMech → 1500
  - Light → 800
  - Medium → 1200
  - Heavy → 1800
  - Colossal → 3000

## Controls
- Type a number: Navigate into that part
- U: Go up
- A: Add new part (only shown when allowed)
- S: Save template (only at root level)
- L: Load template (only at root level)
- q: Quit

## Run
From the project folder:

```powershell
# build
dotnet build

# run
dotnet run
```

## Parts Catalog (`Parts.txt`)
`Parts.txt` is a JSON array of part definitions. Example:

```json
[
  { "name": "Exosuit-Frame",  "type": "Frame",  "WeightLimit": "500",  "Scale": "Personal(1)" },
  { "name": "Cockpit",        "type": "Control" },
  { "name": "Gun",            "type": "Weapon" }
]
```

Notes:
- `type` controls category selection and property display.
- If `Scale` is present, it will be used as-is (no prompt).
- If `Scale` is absent, the user will choose a valid Scale at add time.

## File Map
- `Program.cs` – Console UI and navigation
- `TemplateMenager.cs` – Template management, rules (root frame, scale enforcement)
- `PartManager.cs` – Parts catalog loading, category lists, property metadata, scale helpers
- `StatCalc.cs` – Weight limit derivation from root frame
- `PersistencyManager.cs` – Template save/load functionality
- `Parts.txt` – Parts data source
- `Templates/` – Folder where saved templates are stored

## Roadmap Ideas
- Additional stats derived from scales and part types
- More part-type-specific properties and validation rules
- Richer UI (colors, paging) or a GUI/web front-end
- Template export to different formats
- Template versioning and migration