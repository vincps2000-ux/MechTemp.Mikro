# Project Description: Mech Template Builder

This project is a C# console application for interactively building hierarchical templates for mechs (mechanical units/robots). The user can add, navigate, and organize parts of a mech using a text-based interface. The application is designed to be extensible and modular, with the following key components:

## Main Features
- **Interactive Navigation:** Users can navigate through a tree of mech parts, add new parts, and move up/down the hierarchy.
- **Part Selection:** When adding a new part, the user selects from a list of available parts defined in `Parts.txt`. Only parts of type `Frame` can be added at the root, and only one frame is allowed at the root layer.
- **Add Part Restriction:** The UI will only show the "Add New Part" option if the current layer can accept a new part, as determined by the `CanAdd()` method in `Templatemanager`. At the root, this means only one frame can be added; at other layers, additional rules can be implemented as needed.
- **Template Management:** The `Templatemanager` class manages the current mech template, allowing parts to be added at any level, with logic to restrict additions based on layer rules.
- **Template Persistence:** Users can save templates to disk and load them later. Templates are stored in the `Templates` folder as JSON files. The system supports creating new templates or loading existing ones at startup, and saving/loading during navigation.
- **Parts Data:** All available parts (with optional types) are stored in `Parts.txt` as a JSON array.
 - **Stats Display:** The UI shows a recalculated weight limit each navigation step. The weight limit is calculated by `StatCalc.GetWeightLimit()` based on the template's root frame.
- **Scale Display:** The UI header shows the current node's Scale, and each listed part shows its Scale alongside its name.
 - **Category-driven Selection:** Categories are read from `Parts.txt` via `PartManager.GetAllCategories()` so the category list shown to the user matches the source data exactly.
 - **Frame Root-only Rule:** Frames are enforced as root-only in the UI; when adding a child under a parent, the "Frame" category is filtered out.

## Key Files
- `Program.cs`: Main entry point and navigation logic for the console UI.
- `TemplateMenager.cs`: Contains the `Templatemanager` class for managing the mech template structure.
- `PartManager.cs`: Loads available parts from `Parts.txt` and provides filtering by type.
 - `StatCalc.cs`: Calculates template-level stats (currently weight limit) from the JSON template.
 - `PartManager.cs`: Loads available parts from `Parts.txt`, provides filtering by type, and can list all categories.
 - `PartManager.cs` also provides Scale helpers and a `ChooseScale(parentScale)` prompt used only for parts that lack a predefined Scale in `Parts.txt`.
 - `PersistencyManager.cs`: Handles saving and loading templates to/from the `Templates` folder. Provides methods for listing, loading, saving, and deleting template files.
- `Parts.txt`: JSON file listing all possible parts and their types (e.g., Frame, Weapon, etc.).
- `Templates/`: Folder where saved templates are stored as JSON files.

## Usage
- On startup, the user can choose to create a new template or load an existing one from the Templates folder.
- The user can add parts, but only if the current layer allows it (e.g., only one frame at the root).
- The "Add New Part" option is only shown when allowed by the template rules.
- The navigation UI allows moving up and down the part hierarchy.
 - Scale rules: a child part's Scale must be the same or smaller than its parent's Scale. Frames have Scale predefined in `Parts.txt` and will not prompt for Scale.
- At the root level, users can press 'S' to save the current template or 'L' to load a different template.
- Templates are automatically saved to the `Templates` folder as JSON files with proper indentation.

## Extensibility
- The part system is data-driven; new parts or types can be added by editing `Parts.txt`.
- The navigation and template logic can be extended for more complex mech-building features.

## Notes for Future AI-Agents
- The code is modular and should be easy to extend for new features (e.g., part attributes, validation, export formats).
- The UI is intentionally simple for easy automation or migration to a GUI/web interface.
- All part and template logic is separated from the UI for maintainability.
- The `CanAdd()` method in `Templatemanager` enforces layer-specific rules for part addition, and the UI respects these rules.
 - The UI now recalculates stats each step using `StatCalc` and displays a weight limit. The `PartManager` exposes `GetAllCategories()` and `GetTypeForName()` to support category-driven selection.
 - The scale system ensures frames use predefined scales and only prompts for Scale when it's not in the part definition; child parts cannot exceed the parent's Scale.
 - The `PersistencyManager` handles all file I/O for templates, automatically managing the Templates folder and ensuring proper JSON formatting.
 - When loading a template, the `Templatemanager` automatically updates its internal ID counter to prevent conflicts with existing part IDs.

---
This file is intended to help future AI agents or developers quickly understand the project structure and goals.
