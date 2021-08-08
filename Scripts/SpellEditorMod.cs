using System.Collections;
using System.Linq;

using UnityEngine;

using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.UserInterfaceWindows;

using Wenzil.Console;
using System.IO;
using DaggerfallWorkshop.Game.UserInterface;

namespace SpellEditorMod
{
    public class SpellEditorMod : MonoBehaviour
    {
        private static Mod mod;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;

            var go = new GameObject(mod.Title);
            go.AddComponent<SpellEditorMod>();

            mod.IsReady = true;
        }

        private void Start()
        {
#if UNITY_EDITOR
            ConsoleCommandsDatabase.RegisterCommand(SpellEditorCommand.name, SpellEditorCommand.description, SpellEditorCommand.usage, SpellEditorCommand.Execute);
#endif
        }

#if UNITY_EDITOR
        private static class SpellEditorCommand
        {
            static ConsoleController controller;

            public static readonly string name = "spelleditor";
            public static readonly string description = "Edit standard spells";
            public static readonly string usage = "spelleditor [modname]";

            public static string Execute(params string[] args)
            {
                if (args.Length > 0)
                {
                    if (ModManager.Instance.GetMod(args[0]) == null)
                    {
                        return $"Mod '{args[0]}' not found";
                    }
                    ModManager.Instance.StartCoroutine(WaitAndOpenSpellEditorWindow(args[0]));
                    return $"Editing spells for mod '{args[0]}'. Close the console to open mod picker window.";
                }
                else
                {
                    if (!ModManager.Instance)
                        return "ModManager instance not found.";

                    string[] modTitles = ModManager.Instance.Mods.Where(x => x.IsVirtual && x.Title != "Spell Editor").Select(x => x.Title).ToArray();
                    if (modTitles.Length == 0)
                        return "There are no mods that have been loaded from Unity.";

                    ModManager.Instance.StartCoroutine(OpenModPickerWindow(modTitles));
                    return $"Found {modTitles.Length} mod(s) loaded from Unity. Close the console to open mod picker window.";
                }

            }

            private static IEnumerator WaitAndOpenSpellEditorWindow(string modTitle)
            {
                if (!FindController())
                    yield break;

                while (controller.ui.isConsoleOpen)
                    yield return null;

                OpenSpellEditor(modTitle);
            }

            private static IEnumerator OpenModPickerWindow(string[] modTitles)
            {
                if (!FindController())
                    yield break;

                while (controller.ui.isConsoleOpen)
                    yield return null;

                var userInterfaceManager = DaggerfallUI.Instance.UserInterfaceManager;

                var listPicker = new DaggerfallListPickerWindow(userInterfaceManager);
                listPicker.ListBox.AddItems(modTitles);
                listPicker.OnItemPicked += (index, modTitle) =>
                {
                    OpenSpellEditor(modTitle, listPicker);
                };
                userInterfaceManager.PushWindow(listPicker);
            }

            private static bool FindController()
            {
                if (controller)
                    return true;

                GameObject console = GameObject.Find("Console");
                if (console && (controller = console.GetComponent<ConsoleController>()))
                    return true;

                Debug.LogError("Failed to find console controller.");
                return false;
            }

            private static void OpenSpellEditor(string modTitle, IUserInterfaceWindow previousWindow = null)
            {
                var userInterfaceManager = DaggerfallUI.Instance.UserInterfaceManager;                
                var spellPicker = new SpellEditorSpellPicker(modTitle, userInterfaceManager, previousWindow);
                userInterfaceManager.PushWindow(spellPicker);
            }
        }
#endif
    }
}
