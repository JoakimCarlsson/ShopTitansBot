﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Riposte;
using Riposte.Sim;
using ShopTitansCheat.Data;
using ShopTitansCheat.Utils;
using UnityEngine;

namespace ShopTitansCheat.Components
{
    class CraftingComponent
    {
        private int _i = 1;

        internal void Craft()
        {
            foreach (Equipment item in Settings.Crafting.CraftingEquipmentsList)
            {
                if (item.Done)
                    continue;

                if (!Core.StartCraft(item.ShortName))
                {
                    Log.PrintConsoleMessage($"Not enough for {item.FullName}, continuing", ConsoleColor.Red);
                    return;
                }

                Log.PrintConsoleMessage($"Sucesfully crafted {item.FullName}, {item.ItemQuality}", ConsoleColor.Green);
                item.Done = true;
                item.FullName = $"{item.FullName}, {item.Done}";

                if (Settings.Crafting.CraftingEquipmentsList.All(i => i.Done))
                {
                    Log.PrintConsoleMessage($"Crafted everything in list. \tRestarting.", ConsoleColor.Green);

                    foreach (Equipment equipment in Settings.Crafting.CraftingEquipmentsList)
                    {
                        equipment.FullName = equipment.FullName.Replace(", True", "");
                        equipment.Done = false;
                    }

                    return;
                }
            }
        }

        internal bool GlitchCraft()
        {
            foreach (Equipment item in Settings.Crafting.CraftingEquipmentsList)
            {
                if (item.Done)
                    continue;

                if (!Core.StartCraft(item.ShortName))
                {
                    Log.PrintConsoleMessage($"Not enough resources for {item.FullName}", ConsoleColor.Red);
                    return true;
                }

                Equipment equipment = Core.PeekCraft(item.ShortName)[0];

                if (equipment.ItemQuality >= item.ItemQuality)
                {
                    Log.PrintConsoleMessage($"{equipment}, Tries: {_i}", ConsoleColor.Green);

                    _i = 1;
                    item.Done = true;
                    item.FullName = $"{item.FullName}, {item.Done}";
                    return true;
                }

                Log.PrintConsoleMessage($"{equipment}, Tries: {_i++}", ConsoleColor.Yellow);
                Game.Instance.Restart();
                Core.CollectGarbage();
                return false;
            }

            if (Settings.Crafting.CraftingEquipmentsList.All(i => i.Done))
            {
                Log.PrintConsoleMessage("We are done\n Stopping.", ConsoleColor.Green);
                return true;
            }

            return false;
        }

        internal bool CraftRandomStuffOverValue(int value)
        {
            List<GClass281> tmpList = Game.User.observableDictionary_2.Values.ToList();
            tmpList.Shuffle();

            foreach (GClass281 blueprint in tmpList)
            {
                ItemData itemData = Game.Data.method_257(blueprint.string_0);
                string fullName = Game.Texts.GetText(blueprint.method_0());

                if (itemData.Value < value || fullName.Contains("Element") || fullName.Contains("Spirit"))
                    continue;

                if (Core.StartCraft(blueprint.string_0))
                {
                    Log.PrintConsoleMessage($"started crafting: {blueprint.string_0}", ConsoleColor.Green);
                    return true;
                }

                Log.PrintConsoleMessage($"not enough materials too craft: {fullName}", ConsoleColor.Red);

                return false;
            }
            return false;
        }

        private IEnumerator WaitThenStart(int seconds)
        {
            Log.PrintConsoleMessage($"We are waiting {seconds} seconds.", ConsoleColor.Blue);
            yield return new WaitForSeconds(seconds);
        }
    }
}