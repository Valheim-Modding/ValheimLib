# ValheimLib - A library to facilitate valheim modding

## About

ValheimLib is a modding framework for other mods to work in, providing centralized and simplified APIs for Valheim. 

This helps keeping mods compatible with each other.

At it's core, ValheimLib should not change how the game behaves without any other mod installed.

## Installation

The contents of `ValheimLib` should be extracted into the `BepInEx` folder, such that the ValheimLib folder in `plugins` in the archive is inside your `plugins` folder of your BepInEx installation.

## Developing

Documentation will be included in the  *xmldocs*, and further information may be on the dedicated [ValheimModding wiki](https://github.com/Valheim-Modding/Wiki). Do not hestiate to ask in [the modding discord](https://discord.gg/RBq2mzeu4z) too!

## Changelog

**0.0.7**

* FixReferences now handle enumerable of objects

**0.0.6**

* Mock use generic to support all Component
* InstantiateClone now register to znetscene if needed and tries to makes clone name unique
* Fix the SetupVisEquipment exception in the main menu

**0.0.5**

* Add Mock for CraftingStation. Better error handling for ItemDrop.IsValid

**0.0.4**

* Custom item data save system : Serialization to a file in case the user play the game without having custom items installed and don't want to see them gone when installing the mods back.

**0.0.3**

* Forgot to add the mmhook package dependency. Oops.

**0.0.2**

* Mock system for retrieving correct UnityObject instances at runtime.

**0.0.1**

* Helper for Prefab cloning
* Helper for custom item and recipes addition.
