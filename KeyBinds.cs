using Nautilus.Handlers;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Tweaks_Fixes
{
    internal class KeyBinds
    {
        static public GameInput.Button moveAllItemsButton = EnumHandler.AddEntry<GameInput.Button>("TF_move_all_items")
        .CreateInput(Language.main.Get("TF_move_all_items"), Language.main.Get("TF_move_all_items_desc"))
        .WithKeyboardBinding(GameInputHandler.Paths.Keyboard.Shift)
        .WithControllerBinding(GameInputHandler.Paths.Gamepad.RightTrigger)
        .WithCategory(Main.MODNAME).AvoidConflicts();

        static public GameInput.Button moveSameItemsButton = EnumHandler.AddEntry<GameInput.Button>("TF_move_same_items")
            .CreateInput(Language.main.Get("TF_move_same_items"), Language.main.Get("TF_move_same_items_desc"))
            .WithKeyboardBinding(GameInputHandler.Paths.Keyboard.Ctrl)
            .WithControllerBinding(GameInputHandler.Paths.Gamepad.LeftTrigger)
            .WithCategory(Main.MODNAME).AvoidConflicts();

        static public GameInput.Button previousPDAtab = EnumHandler.AddEntry<GameInput.Button>("TF_previous_PDA_tab")
            .CreateInput(Language.main.Get("TF_previous_PDA_tab"), Language.main.Get("TF_previous_PDA_tab_desc"))
            .WithKeyboardBinding(GameInputHandler.Paths.Keyboard.Q)
            .WithCategory(Main.MODNAME).AvoidConflicts();

        static public GameInput.Button nextPDAtab = EnumHandler.AddEntry<GameInput.Button>("TF_next_PDA_tab")
            .CreateInput(Language.main.Get("TF_next_PDA_tab"), Language.main.Get("TF_next_PDA_tab_desc"))
            .WithKeyboardBinding(GameInputHandler.Paths.Keyboard.E)
            .WithCategory(Main.MODNAME).AvoidConflicts();

        static public GameInput.Button quickSlotCycle = EnumHandler.AddEntry<GameInput.Button>("TF_quick_slot_cycle")
            .CreateInput(Language.main.Get("TF_quick_slot_cycle"), Language.main.Get("TF_quick_slot_cycle_desc"))
            .WithKeyboardBinding(GameInputHandler.Paths.Keyboard.Alt)
            .WithControllerBinding(GameInputHandler.Paths.Gamepad.ButtonSouth)
            .WithCategory(Main.MODNAME).AvoidConflicts();

        static public GameInput.Button autoMove = EnumHandler.AddEntry<GameInput.Button>("TF_auto_move")
        .CreateInput(Language.main.Get("TF_auto_move"), Language.main.Get("TF_auto_move_desc"))
        .WithKeyboardBinding(GameInputHandler.Paths.Keyboard.Alt)
        .WithControllerBinding(GameInputHandler.Paths.Gamepad.ButtonSouth)
        .WithCategory(Main.MODNAME).AvoidConflicts();






    }
}
