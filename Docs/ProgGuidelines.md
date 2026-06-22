## Key Principles
- Favor readability over brevity. Clarity is more important than any time saved from omitting a few vowels.
- The goal is to make your code more readable, maintainable, and consistent. Favor what can be pronounced naturally. e.g., HorizontalAlignment instead of AlignmentHorizontal (more English-readable).
- Doing things like naming right from the beginning will save you time and effort later. Particularly when debugging, and extending functionality.
- Building on above, a code style standard is a living document. It should be updated as the project evolves and the team grows.

## Naming
- Pick meaningful names from the beginning to minimize refactoring later.
- Variable names should be descriptive, clear, and unambiguous because they represent a thing or state.
- Use a noun when naming them except when the variable is of the type bool.
- Prefix Booleans with a verb to make their meaning more apparent. e.g., isDead, isWalking, hasDamageMultiplier.
- Use meaningful names. Don’t abbreviate (unless it’s math or commonly accepted). Your variable names should reveal their intent.
- Choose identifier names that are easily readable. For example, a property named HorizontalAlignment is more readable than AlignmentHorizontal.
- Make type names unambiguous across namespaces and problem domains by avoiding common terms

## Casting and Prefixes
- Use PascalCase for public fields (e.g., ExamplePlayerController, MaxHealth, etc.)
- Use camelCase for private fields adding an underscore prefix to differentiate them from local variables (e.g., _playerController, _maxHealth, etc.)
- Use camelCase for local variables without any prefix (e.g., playerHealth, isAlive, etc.)
- If you have a Monobehaviour in a file, the source file name must match.
- Drop redundant initializers (i.e., no '= 0' on the ints, '= null' on ref types, etc.) as they are initialized to 0 or null by default
- Always specify access level modifiers (public, protected, private, etc.)
- Avoid redundant names: If your class is called Player, you don't need to create member variables called PlayerScore or PlayerTarget.

## Formatting and Spacing
- Always open curly braces on a new line.
- Readability is key.Try to keep lines short.Consider horizontal whitespace.
- Try to keep lines short with maxi line width around 120 characters.
- Break a long line into smaller statements rather than letting it overflow.
- Use a single space before flow control conditions, e.g. while (x == y).
- Avoid spaces inside brackets, e.g. x = dataArray[index].
- Don’t use spaces between a function name and parenthesis, e.g. DropPowerUp(myPrefab, 0, 1);
- Use a single space after a comma between function arguments, e.g. CollectItem(myObject, 0, 1);
- Don’t add a space after the parenthesis and function arguments, e.g. CollectItem(myObject, 0, 1);
- Use a single space before flow control conditions and a single space before and after comparison operators, e.g. if (x == y).

## Comments
- If you need to add a comment to explain a convoluted tangle of logic, consider restructuring your code to be more obvious.
- Good naming can take out the guesswork. Consider renaming before commenting.
- Only add comments when needed. That is when the code isn’t self-explanatory and needs clarification beyond good naming revealing the intent.
- Rather than simply answering "what" or "how," comments can fill in the gaps and tell us "why."
- Use the // comment to keep the explanation next to the logic.
- Use a Tooltip instead of a comment for serialized fields if your fields in the Inspector need explanation.
- Rather than using Regions think of them as a code smell indicating your class is too large and needs refactoring.
- Use a summary XML tag in front of public methods or functions when needed for output documentation/Intellisense.

## Class Organization
- Organize your class in the following order: Events, Fields, Properties
- Monobehaviour methods (Awake, Start, OnEnable, OnDisable, OnDestroy, etc.), public methods, internal methods, private methods.
- Methods are grouped by descending accessibility: public, then internal, then private. Keep each access group together rather than interleaving by call order.
- Use of #region is generally discouraged as it can hide complexity and make it harder to read the code.

## Refactoring Existing Code
- When applying these guidelines to existing code, **never rename or change the declaration of public fields or `[SerializeField]` private fields that are not properties**. Unity serializes these by name and stores Inspector references against them — renaming or removing them silently breaks prefabs, scenes and ScriptableObjects, with no compile-time warning.
- Properties (including expression-bodied `=>` accessors) are not serialized and may be renamed safely.
- Private fields without `[SerializeField]` are not serialized and may be renamed safely (e.g. `currentParties` → `_currentParties`).
- If a public/serialized field's name violates the guidelines, leave it as-is and note the discrepancy rather than risk data loss. Renames of serialized fields require a deliberate migration step (e.g. `[FormerlySerializedAs]`) and explicit user approval.