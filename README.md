# Spell Editor
In-game Spell editor for Daggerfall Unity 0.11.5

## Overview

Since 0.11.5, mods can provide a `SpellRecords.json` file to modify or add spells in DFU. This format was taken for ease of implementation, mostly, since this was the last version before the feature freeze. Modifying this JSON file by hand is tedious, error-prone, and requires knowledge of the engine to know which fields can be modified and how.

For my mod Unleveled Spells, I had to modify almost 100 spells: change their values, change the icon, potentially change effects or name. I also had to add new spells for monsters. Doing all that by hand would have taken lots of time at first, and would still take more time to maintain each time a tweak has to be made.

So I made my own spell editor. This is like a Spellmaker that loads and saves to `SpellRecords.json`. It works as a mod, but **it is not a mod**. It is meant to be installed as a "virtual mod" in a Daggerfall Unity source repository, under Assets/Game/Mods. Running it from the game will not work.

Once installed, get into any character save, open the console (press `), and enter "spelleditor", then close the console.

Then, select a mod from the mods installed in your DFU repository.
![Mod Picker example](Resources/ModPicker.png?raw=true)

You will see all the classic spells, and if you scroll down, any spells you may have added.
![Spell Picker example](Resources/SpellPicker.png?raw=true)

Select a spell, then use the Spellmaker as normal to change its properties.
![Spellmaker example](Resources/SpellMaker.png?raw=true)

Once done, press O.K., and you will see the changes from the spell picker screen again.
![Spell picker result](Resources/SpellPicker2.png?raw=true)

Once you press Save, a `SpellRecords.json` will be created in the mod's folder. Be sure to add it to the .dfmod.json file!

## Installation

1. Clone ![Daggerfall Unity](https://github.com/Interkarma/daggerfall-unity.git), or an equivalent fork
2. Go into <DaggerfallUnity>/Assets/Game/Mods
3. Clone this repository
4. Run the game from the Unity editor
5. Load any character save
6. Open the console (press `), type "spelleditor", press Enter, close the console
7. The spell editor should appear

## Troubleshoot

- Your mod's .dfmod.json file must be added to its own "files" for the editor to know where to find the SpellRecords.json
- The spell editor cannot use non-classic icons due to implementation limitations on the DFU side
