# Breaking Plates

A *Breaking Bad* parody mod for [PlateUp!](https://store.steampowered.com/app/1599600/PlateUp/):
cook **Blue Rock Candy** in lab flasks, smash it up and serve it in little bags. It's just sugar. Probably.

> Status: **in development (v0.2)** – playable, not yet on the Steam Workshop.

---

## The dish

| Step | What you do |
|---|---|
| 1 | Take a **lab flask** (only 4 – put them back when you're done) and add **sugar** and **water** |
| 2 | **Cook** it on a hob → blue syrup |
| 3 | Take it off the heat and let it **cool** (12 s) – a **freezer** does it in 4 s → crystals |
| 4 | **Smash** the crystals by hand on a counter → 5 portions |
| 5 | Put a portion into a **candy bag** and serve – no plate needed |

Every candy bag looks the same: clear foil, blue zip. Toppings and variants are marked on the bag itself.

## Cards

| Card | Type | What it adds | Quote |
|---|---|---|---|
| **Blue Rock Candy** | Main (base) | The dish above | *"Say my name."* |
| **99.1% Purity** | Main | Adds the **Crystal Purifier**. Put a flask of crystals in it to make them pure (white crystals, `99`). | *"I am the one who knocks."* |
| **Chili P** | Topping | Chop a jalapeño and add it to the bag → a **chili clipped to the zip** | *"Chili P is my signature!"* |
| **Sugar-Free** | Main | Cook with **stevia** instead of sugar → bag with a **stevia sprig** | *"Stevia."* |
| **On a Stick** | Main | Put a **candy stick** into the hot syrup before it sets → 5 rock candy lollis straight from the flask | *"Yeah, science!"* |
| **Mass Production** | Main | Groups order a **whole flask** to share – in every variant you sell (normal, 99.1%, sugar-free) | *"I'm in the empire business."* |
| **Schraderbräu** | Side | Home-brewed beer from the tap | *"Jesus, Marie!"* |
| **Breaking Even** | Effect | Fewer customers, but you lose 25% of the day's earnings | *"Better call Saul!"* |

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
