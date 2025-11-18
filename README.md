# Mech Template Builder

A C# console app for interactively building hierarchical mech templates from a parts catalog. You can navigate a JSON-like tree, add parts, and see stats like weight limit and scale at a glance.

## Features
- Interactive console navigation through a mech's part tree
- Add parts from a data-driven catalog (`Parts.txt`)
- Root-only frame rule: exactly one Frame allowed at the root
- Category-driven selection when adding children (Frames hidden for children)
- Stats display per step: Total Weight / Weight Limit and current node's Scale
- Tag system on parts with descriptions
- Scale system with validation
  - Frames and most parts have fixed Scales from `Parts.txt` (no prompt)
  - Only extremities (Joints, Connectors, etc.) with `dynamicScale: true` prompt for Scale
  - A child part's Scale must be the same or smaller than its parent's Scale
- Weight system with dynamic calculation
  - Each part has a base Weight in `Parts.txt`
  - Parts with `WeightFormula` calculate weight based on their actual Scale
  - Total weight is displayed and must not exceed the frame's Weight Limit
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

## Weight System
- **Weight Limit**: Read from the Frame's `WeightLimit` property in `Parts.txt`
  - Exosuit-Frame → 500
  - DemiMech-Frame → 1000
  - Light-Frame → 5000
  - Medium-Frame → 10000
  - Heavy-Frame → 20000
  - Colossal-Frame → 50000
- **Total Weight**: Calculated by `StatCalc.CalculateTotalWeight()` by summing all parts
  - **Frames do NOT count toward weight** - they define the capacity, not consume it
  - Fixed-scale parts (Weapons, Sensors, Controls) use their base Weight value
  - Dynamic-scale parts (extremities) use `WeightFormula` (e.g., `Weight * Scale`)
  - Example: A Joint with Weight=5 at Vehicle(2) scale weighs 5 × 2 = 10
- **Weight Limit Enforcement**: 
  - When adding a part, the system checks if it would exceed the weight limit
  - If exceeded, the part is **rejected** with a detailed error message showing:
    - Current weight
    - Part weight being added
    - Total that would result
    - Weight limit
    - Amount over the limit
  - This prevents building over-weight templates

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
  { "name": "Exosuit-Frame", "type": "Frame", "WeightLimit": "500", "Scale": "Personal(1)", "Weight": "50" },
  { "name": "Personal Cockpit", "type": "Control", "Scale": "Personal(1)", "Weight": "15" },
  { "name": "Pistol", "type": "Weapon", "Scale": "Personal(1)", "Weight": "2" },
  { "name": "Joint", "type": "extremity", "dynamicScale": true, "MinScale": 1, "Weight": "5", "WeightFormula": "Weight * Scale" }
]
```

Notes:
- `type` controls category selection and property display.
- `Scale`: Fixed-scale parts have this predefined; dynamic-scale parts prompt the user.
- `dynamicScale`: If `true`, user chooses Scale within constraints (typically extremities only).
- `Weight`: Base weight value for the part.
- `WeightFormula`: Optional formula for dynamic weight calculation (e.g., "Weight * Scale").
- `Mounting`: Added automatically for Control/Weapon/Sensor parts (Internal or External).

## Tag System
- Each part can declare `Tags` in `Parts.txt` to classify behavior and attributes (e.g., `Melee`, `Ranged`, `Physical`, `Energy`, `Explosive`).
- Tags can carry values using parentheses, e.g., `distance(300)` for effective range or `Energy(40)` for energy rating.
- Descriptions for tags are defined in `Config/Tags.txt` and are shown in the Info screen as a legend.
- Viewing tags:
  - In the Design Bureau, press `I` at the root to open the Info screen which now shows:
    - Actions (with descriptions)
    - Tags by Part (the tags assigned to each part instance)
    - Tag Legend (unique tag names with descriptions)

## File Map
- `Program.cs` – Console UI and navigation
- `TemplateMenager.cs` – Template management, rules (root frame, scale enforcement)
- `PartManager.cs` – Parts catalog loading, category lists, property metadata, scale helpers
- `StatCalc.cs` – Weight limit derivation from root frame
- `PersistencyManager.cs` – Template save/load functionality
- `Managers/TagsManager.cs` – Loads tag descriptions and parses tag names/values
- `Parts.txt` – Parts data source
- `Templates/` – Folder where saved templates are stored

## Roadmap Ideas
- Additional stats derived from scales and part types
- More part-type-specific properties and validation rules
- Richer UI (colors, paging) or a GUI/web front-end
- Template export to different formats
- Template versioning and migration