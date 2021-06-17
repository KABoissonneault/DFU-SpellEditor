using System.Collections.Generic;
using System.IO;
using System.Linq;

using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using static DaggerfallConnect.Save.SpellRecord;

using FullSerializer;

using UnityEngine;

namespace SpellEditorMod
{
    public class SpellEditorSpellPicker : DaggerfallListPickerWindow
    {
        readonly string ModTitle;

        Dictionary<int, fsData> spellRecords;

        Panel panelButtons;

        Button buttonNew;
        Button buttonRevert;
        Button buttonSave;

        public SpellEditorSpellPicker(string modTitle, IUserInterfaceManager uiManager, IUserInterfaceWindow previousWindow = null)
            : base(uiManager, previousWindow)
        {
            ModTitle = modTitle;
        }

        protected override void Setup()
        {
            base.Setup();

            SetupSpells();
            SetupButtons();
            SetupEvents();
        }

        private void SetupSpells()
        {
            Mod mod = ModManager.Instance.GetMod(ModTitle);
            string spellRecordsPath = mod.ModInfo.Files.Find(file => Path.GetFileName(file) == "SpellRecords.json");
            if (!string.IsNullOrEmpty(spellRecordsPath))
            {
                spellRecordsPath = spellRecordsPath.Replace("Assets/Game/Mods/", "");
                string spellRecordsFullPath = Path.Combine(ModManager.EditorModsDirectory, spellRecordsPath);
                List<fsData> listRecords = fsJsonParser.Parse(File.ReadAllText(spellRecordsFullPath)).AsList;
                spellRecords = listRecords.ToDictionary(data => (int)data.AsDictionary["index"].AsInt64);
            }

            var standardSpells = GameManager.Instance.EntityEffectBroker.StandardSpells;
            foreach (SpellRecordData standardSpell in standardSpells)
            {
                bool isClassic = standardSpell.index < 100;
                if (isClassic)
                {
                    if (spellRecords != null && spellRecords.TryGetValue(standardSpell.index, out fsData data))
                    {
                        ListBox.AddItem(standardSpell.spellName + " (*)", -1, standardSpell.index);
                    }
                    else
                    {
                        ListBox.AddItem(standardSpell.spellName, -1, standardSpell.index);
                    }

                }
                else
                {
                    ListBox.AddItem("+ " + standardSpell.spellName, -1, standardSpell.index);
                }
            }
        }

        private void SetupButtons()
        {
            Rect panelRect = new Rect(new Vector2(-pickerPanel.Size.x / 2, pickerPanel.Position.y - 14), new Vector2(pickerPanel.Size.x, 8));
            panelButtons = DaggerfallUI.AddPanel(panelRect, pickerPanel);
            panelButtons.HorizontalAlignment = HorizontalAlignment.Center;

            buttonNew = DaggerfallUI.AddButton(new Vector2(0, 0), new Vector2(32, 10), panelButtons);
            buttonNew.Label.Text = "New";
            buttonNew.Label.VerticalAlignment = VerticalAlignment.Middle;
            buttonNew.BackgroundColor = Color.gray;
            buttonNew.Hotkey = new HotkeySequence(KeyCode.N, HotkeySequence.KeyModifiers.Ctrl);

            buttonRevert = DaggerfallUI.AddButton(new Vector2(40, 0), new Vector2(32, 10), panelButtons);
            buttonRevert.Label.Text = "Revert";
            buttonRevert.Label.VerticalAlignment = VerticalAlignment.Middle;
            buttonRevert.BackgroundColor = Color.gray;
            buttonRevert.Hotkey = new HotkeySequence(KeyCode.R, HotkeySequence.KeyModifiers.Ctrl);

            buttonSave = DaggerfallUI.AddButton(new Vector2(80, 0), new Vector2(32, 10), panelButtons);
            buttonSave.Label.Text = "Save";
            buttonSave.Label.VerticalAlignment = VerticalAlignment.Middle;
            buttonSave.BackgroundColor = Color.gray;
            buttonSave.Hotkey = new HotkeySequence(KeyCode.S, HotkeySequence.KeyModifiers.Ctrl);
        }

        private void SetupEvents()
        {
            ListBox.OnSelectItem += ListBox_OnSelectItem;
            buttonNew.OnMouseClick += ButtonNew_OnMouseClick;
            SetRevertDisabled();
        }

        private void ButtonNew_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            var userInterfaceManager = DaggerfallUI.Instance.UserInterfaceManager;

            var NamePopup = new DaggerfallInputMessageBox(userInterfaceManager, this);
            NamePopup.OnGotUserInput += NamePopup_OnGotUserInput;

            userInterfaceManager.PushWindow(NamePopup);
        }

        private void NamePopup_OnGotUserInput(DaggerfallInputMessageBox sender, string input)
        {
            //spellRecords.
        }

        private void ListBox_OnSelectItem()
        {
            var spellIndex = (int)ListBox.SelectedValue.tag;

            if(spellRecords.ContainsKey(spellIndex))
            {
                SetRevertEnabled();
            }
            else
            {
                SetRevertDisabled();
            }
        }

        private void SetRevertDisabled()
        {
            buttonRevert.Label.TextColor = Color.gray / 2.0f;
            buttonRevert.OnMouseClick -= OnRevertClick;
        }

        private void SetRevertEnabled()
        {
            buttonRevert.Label.TextColor = DaggerfallUI.DaggerfallDefaultTextColor;
            buttonRevert.OnMouseClick += OnRevertClick;
        }

        private void OnRevertClick(BaseScreenComponent sender, Vector2 position)
        {
            if (ListBox.SelectedIndex == -1)
                return;

            var selectedItem = ListBox.SelectedValue;
            var spellIndex = (int)selectedItem.tag;

            if (!spellRecords.ContainsKey(spellIndex))
                return;

            spellRecords.Remove(spellIndex);
            if(spellIndex < 100)
            {
                GameManager.Instance.EntityEffectBroker.GetClassicSpellRecord(spellIndex, out SpellRecordData spellRecord);
                selectedItem.textLabel.Text = spellRecord.spellName;
            }
            else
            {
                ListBox.RemoveItem(ListBox.SelectedIndex);
            }

            SetRevertDisabled();
        }
    }
}