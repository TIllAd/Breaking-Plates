# Breaking Plates

A *Breaking Bad* parody mod for [PlateUp!](https://store.steampowered.com/app/1599600/PlateUp/):
cook **Blue Rock Candy** in lab flasks, smash it up and serve it in little bags. It's just sugar. Probably.

> Status: **in development (v0.2)** – playable, not yet on the Steam Workshop.

---

## The dish

| Step | What you do |
|---|---|
| 1 | Take a **lab flask** and add **sugar** and **water** |
| 2 | **Cook** it → blue syrup |
| 3 | Let it **cool down** (12 s, or 4 s in a **freezer**) to get crystals |
| 4 | **Interact** on a counter to smash the crystals → 5 portions |
| 5 | **Portion** it and combine with a **candy bag** to serve |

Every candy bag looks the same: clear foil, blue zip. Toppings and variants are marked on the bag itself.

## Cards

| Card | Type | ★ | Customers | Order pays | What it adds | Quote |
|---|---|---|---|---|---|---|
| **Blue Rock Candy** | Main (base) | 3 | small decrease | 5 | The dish above | *"Say my name."* |
| **99.1% Purity** | Main | 2 | small decrease | 6 | **Recrystallise**: smashed candy + water back into the flask, cook and cool again → pure (white, `99`). Pure candy also counts for normal orders | *"A higher purity means a greater yield."* |
| **Chili P** | Topping | 1 | small decrease | +1 | Chopped jalapeño in the bag → **chili clipped to the zip** | *"Chili P is my signature!"* |
| **Sugar-Free** | Main | 2 | small decrease | 5 | Cook with **stevia** instead of sugar → bag with a **stevia sprig** | *"I'll need more stevia."* |
| **On a Stick** | Main | 2 | small decrease | 5 | **Candy stick** into the hot syrup → 5 lollis straight from the flask | *"Yeah, science!"* |
| **Mass Production** | Main | 2 | small decrease | 20 | Groups order a **whole flask** (up to 4 customers) – in every variant you sell | *"I'm in the empire business."* |
| **Schraderbräu** | Side | 1 | small decrease | 2 | Home-brewed beer from the tap | *"Jesus, Marie!"* |
| **Breaking Even** | Effect | – | **large decrease** | –25% daily earnings | Fewer customers, but less money | *"Better call Saul!"* |

"Small decrease" / "large decrease" are the game's customer tags (about −15% / −28% customers).
Values are a first pass and still need balancing in real runs.

## Decorations

- **Periodic Table** poster (Br and Ba highlighted)
- **Yellow Barrels** – definitely just decoration
- **Los Pollitos Hermanos** sign

## Colourblind labels

| Label | Meaning |
|---|---|
| `Su` / `Stv` / `W` | sugar / stevia / water in the flask |
| `Sy` | syrup |
| `Cr` / `Sh` | crystals / smashed crystals |
| `99` | pure (99.1%) |
| `SF` | sugar-free |
| `St` | with sticks (lollis) |
| `P` | Chili P |

## Easter egg

If [Chicken & Rice](https://github.com/TIllAd/ChickenFoodMod) is installed too, its cards show up more often. Los Pollos Hermanos would approve.

---

## Requirements

- [KitchenLib](https://steamcommunity.com/sharedfiles/filedetails/?id=2898069883)
- [IngredientLib](https://steamcommunity.com/sharedfiles/filedetails/?id=2913877103)

## Building

```powershell
dotnet build BreakingPlates -c Release -p:EnableModDeployLocal=false
copy BreakingPlates\bin\Release\net472\BreakingPlates-Workshop.dll Workshop\content\
```

A **Debug** build copies itself into `PlateUp\Mods\BreakingPlates` and adds developer tools:

- **F8 live tuning**: shapes, positions and colours are read from `PlateUp\Mods\BreakingPlates\tuning.txt`; edit a value and press F8 in game.
- **Data dump**: `breakingplates_dump.txt` with the game data the mod builds on.

All models (flasks, bags, stickers, barrels, poster, sign) are built in code from simple shapes – there is no Unity asset bundle.

## Testing

Use [PlateUp-ModSelfTest](https://github.com/TIllAd/PlateUp-ModSelfTest) with the prefix `TillAd.PlateUp.BreakingPlates`.
It checks every order and ingredient combination with the game's own logic and renders everything the mod adds.

## Publishing

Same as Chicken & Rice: create the item in the ModUploader (**Create**), then upload `Workshop\` via **Update** with "Content" ticked.

---

*Fan-made parody. Not affiliated with or endorsed by AMC, Sony Pictures Television or the makers of PlateUp!.*
