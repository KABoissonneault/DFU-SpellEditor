using System.Collections.Generic;
using System.IO;
using System.Linq;

using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using static DaggerfallConnect.Save.SpellRecord;

using FullSerializer;

using UnityEngine;
using DaggerfallWorkshop.Utility;
using static DaggerfallWorkshop.Game.UserInterfaceWindows.DaggerfallMessageBox;

namespace SpellEditorMod
{
    public class SpellEditorSpellPicker : DaggerfallListPickerWindow
    {
        readonly string ModTitle;

        Dictionary<int, fsData> spellRecords = null;
        Dictionary<int, SpellRecordData> classicSpellRecords;

        Panel panelButtons;

        Button buttonNew;
        Button buttonRevert;
        Button buttonSave;

        TextBox textNextSpellIndex;

        int modNextSpellIndex = -1;

        public SpellEditorSpellPicker(string modTitle, IUserInterfaceManager uiManager, IUserInterfaceWindow previousWindow = null)
            : base(uiManager, previousWindow)
        {
            ModTitle = modTitle;
        }

        public override void CancelWindow()
        {
            if(HasUnsavedChanges())
            {
                DaggerfallMessageBox mb = new DaggerfallMessageBox(uiManager, this);
                mb.SetTextTokens(DaggerfallUnity.TextProvider.CreateTokens(TextFile.Formatting.JustifyLeft, "Editor has unsaved changes. Save before quitting?"));
                mb.AddButton(MessageBoxButtons.Yes);
                mb.AddButton(MessageBoxButtons.No);
                mb.AddButton(MessageBoxButtons.Cancel);
                mb.OnButtonClick += CancelWindow_OnButtonClick;
                mb.Show();
            }
            else
            {
                base.CancelWindow();
            }
        }

        private void CancelWindow_OnButtonClick(DaggerfallMessageBox sender, MessageBoxButtons messageBoxButton)
        {
            sender.CloseWindow();

            if(messageBoxButton == MessageBoxButtons.Yes)
            {
                Save();
                CloseWindow();
            }
            else if(messageBoxButton == MessageBoxButtons.No)
            {
                CloseWindow();
            }
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

#if UNITY_EDITOR
            var dfModFile = mod.ModInfo.Files.Find(filepath => Path.GetFileName(filepath).EndsWith("dfmod.json"));
            if (!string.IsNullOrEmpty(dfModFile))
            {
                var spellRecordsFullPath = Path.Combine(ModManager.EditorModsDirectory,
                    dfModFile.Replace("Assets/Game/Mods/", "").Replace(Path.GetFileName(dfModFile), ""),
                    "SpellRecords.json");
                if (File.Exists(spellRecordsFullPath))
                {
                    List<fsData> listRecords = fsJsonParser.Parse(File.ReadAllText(spellRecordsFullPath)).AsList;
                    spellRecords = listRecords.ToDictionary(data => (int)data.AsDictionary["index"].AsInt64);
                }
                else
                {
                    spellRecords = new Dictionary<int, fsData>();
                }
            }
            else
            {
                DaggerfallMessageBox mb = new DaggerfallMessageBox(uiManager, this);
                mb.SetTextTokens(DaggerfallUnity.TextProvider.CreateTokens(TextFile.Formatting.JustifyLeft, $"{Path.GetFileName(dfModFile)} not found listed in its own files. Save mod settings correctly before use."));
                mb.ClickAnywhereToClose = true;
                mb.Show();

                mb.OnClose += Error_OnClose;
                return;
            }
#endif // UNITY_EDITOR

            var classicSpells = DaggerfallSpellReader.ReadSpellsFile(Path.Combine(DaggerfallUnity.Arena2Path, DaggerfallSpellReader.DEFAULT_FILENAME)).Where(spell => spell.spellName != "Holy Touch");

            foreach (SpellRecordData classicSpell in classicSpells)
            {
                if (spellRecords.TryGetValue(classicSpell.index, out fsData data))
                {
                    ListBox.AddItem($"[{classicSpell.index}] {classicSpell.spellName} (*)", -1, classicSpell.index);
                }
                else
                {
                    ListBox.AddItem($"[{classicSpell.index}] {classicSpell.spellName}", -1, classicSpell.index);
                }
            }

            classicSpellRecords = classicSpells.ToDictionary(spell => spell.index);

            var addedSpells = spellRecords.Where(kvp => kvp.Key > 99).Select(kvp => kvp.Value);
            modNextSpellIndex = 100;
            foreach (fsData addedSpell in addedSpells)
            {
                var spellName = addedSpell.AsDictionary["spellName"].AsString;
                var spellIndex = (int)addedSpell.AsDictionary["index"].AsInt64;
                ListBox.AddItem($"+ [{spellIndex}] {spellName}", -1, spellIndex);
                modNextSpellIndex = Mathf.Max(modNextSpellIndex, spellIndex + 1);
            }            
        }

        private void Error_OnClose()
        {
            CloseWindow();
        }

        private bool IsNewDisabled(int spellIndex)
        {
            if (spellIndex < 100)
                return false;

            return spellRecords.TryGetValue(spellIndex, out fsData _);
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
            buttonRevert.Label.Text = "Remove";
            buttonRevert.Label.TextColor = Color.gray / 2.0f;
            buttonRevert.Label.VerticalAlignment = VerticalAlignment.Middle;
            buttonRevert.BackgroundColor = Color.gray;
            buttonRevert.Hotkey = new HotkeySequence(KeyCode.R, HotkeySequence.KeyModifiers.Ctrl);

            buttonSave = DaggerfallUI.AddButton(new Vector2(80, 0), new Vector2(32, 10), panelButtons);
            buttonSave.Label.Text = "Save";
            buttonSave.Label.VerticalAlignment = VerticalAlignment.Middle;
            buttonSave.BackgroundColor = Color.gray;
            buttonSave.Hotkey = new HotkeySequence(KeyCode.S, HotkeySequence.KeyModifiers.Ctrl);

            textNextSpellIndex = DaggerfallUI.AddTextBoxWithFocus(new Rect(new Vector2(128, 0), new Vector2(32, 10)), string.Empty, panelButtons, 4);
            textNextSpellIndex.BackgroundColor = Color.gray;
            textNextSpellIndex.Numeric = true;
            textNextSpellIndex.NumericMode = NumericMode.Natural;
            textNextSpellIndex.OnType += TextNextSpellIndex_OnType;
            textNextSpellIndex.OnKeyboardEvent += TextNextSpellIndex_OnKeyboardEvent;
            textNextSpellIndex.Text = modNextSpellIndex.ToString();
            textNextSpellIndex.VerticalAlignment = VerticalAlignment.Middle;
            textNextSpellIndex.TextOffset = 2;

            DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, -8), "New Spell Index", textNextSpellIndex);
        }

        private void TextNextSpellIndex_OnKeyboardEvent(BaseScreenComponent sender, Event keyboardEvent)
        {
            int nextId = int.Parse(textNextSpellIndex.ResultText);
            if (IsNewDisabled(nextId))
            {
                textNextSpellIndex.Cursor.Color = Color.red;
                SetNewDisabled();
            }
            else
            {
                textNextSpellIndex.Cursor.Color = DaggerfallUI.DaggerfallDefaultTextCursorColor;
                modNextSpellIndex = nextId;
                SetNewEnabled();
            }
        }

        private void TextNextSpellIndex_OnType()
        {
            int nextId = int.Parse(textNextSpellIndex.ResultText);
            if(IsNewDisabled(nextId))
            {
                textNextSpellIndex.Cursor.Color = Color.red;
                SetNewDisabled();
            }
            else
            {
                textNextSpellIndex.Cursor.Color = DaggerfallUI.DaggerfallDefaultTextCursorColor;
                modNextSpellIndex = nextId;
                SetNewEnabled();
            }
        }

        private void IncrementModIndex()
        {
            while (IsNewDisabled(++modNextSpellIndex)) ;

            textNextSpellIndex.Text = modNextSpellIndex.ToString();
        }

        private void SetupEvents()
        {
            ListBox.OnSelectItem += ListBox_OnSelectItem;
            ListBox.OnUseSelectedItem += ListBox_OnUseSelectedItem;
            buttonNew.OnMouseClick += ButtonNew_OnMouseClick;

            if(spellRecords.ContainsKey((int)ListBox.SelectedValue.tag))
                SetRevertEnabled();
            else
                SetRevertDisabled();

            SetSaveDisabled();
        }

        private void SetNewDisabled()
        {
            if (buttonNew.Label.TextColor == Color.gray / 2.0f)
                return;

            buttonNew.Label.TextColor = Color.gray / 2.0f;
            buttonNew.OnMouseClick -= ButtonNew_OnMouseClick;
        }

        private void SetNewEnabled()
        {
            if (buttonNew.Label.TextColor == DaggerfallUI.DaggerfallDefaultTextColor)
                return;

            buttonNew.Label.TextColor = DaggerfallUI.DaggerfallDefaultTextColor;
            buttonNew.OnMouseClick += ButtonNew_OnMouseClick;
        }

        private void ButtonNew_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            var userInterfaceManager = DaggerfallUI.Instance.UserInterfaceManager;

            Dictionary<string, fsData> spellProperties = new Dictionary<string, fsData>();
            spellProperties["index"] = new fsData(modNextSpellIndex);
            var newSpell = new fsData(spellProperties);

            var BundleEditor = new SpellBundleEditor(userInterfaceManager, this);
            BundleEditor.SpellData = newSpell;
            BundleEditor.OnSpellConfirmed += BundleEditor_OnNewSpellConfirmed;

            userInterfaceManager.PushWindow(BundleEditor);
        }

        private void BundleEditor_OnNewSpellConfirmed(fsData spellData)
        {
            SetSaveEnabled();
            spellRecords.Add(modNextSpellIndex, spellData);

            bool added = false;
            for(int i = 0; i < ListBox.ListItems.Count; ++i)
            {
                if((int)ListBox.ListItems[i].tag > modNextSpellIndex)
                {
                    added = true;
                    ListBox.AddItem($"+ [{modNextSpellIndex}] {spellData.AsDictionary["spellName"].AsString}", i, modNextSpellIndex);
                    break;
                }
            }

            if(!added)
            {
                ListBox.AddItem($"+ [{modNextSpellIndex}] {spellData.AsDictionary["spellName"].AsString}", -1, modNextSpellIndex);
            }

            IncrementModIndex();
        }

        private void BundleEditor_OnEditSpellConfirmed(fsData spellData, int spellIndex)
        {
            SetSaveEnabled();

            if((int)ListBox.SelectedValue.tag == spellIndex)
                SetRevertEnabled();

            var item = ListBox.ListItems.Find(i => (int)i.tag == spellIndex);
            if(spellIndex < 100)
            {
                item.textLabel.Text = $"[{spellIndex}] {spellData.AsDictionary["spellName"].AsString} (*)";
            }
            else
            {
                item.textLabel.Text = $"+ [{spellIndex}] {spellData.AsDictionary["spellName"].AsString}";
            }

            spellRecords[spellIndex] = spellData;           
        }

        private void ListBox_OnSelectItem()
        {
            if (ListBox.SelectedIndex == -1)
                return;

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

        private void ListBox_OnUseSelectedItem()
        {
            var spellIndex = (int)ListBox.SelectedValue.tag;

            var userInterfaceManager = DaggerfallUI.Instance.UserInterfaceManager;
            var BundleEditor = new SpellBundleEditor(userInterfaceManager, this);
            if (spellRecords != null && spellRecords.TryGetValue(spellIndex, out fsData existingData))
            {
                BundleEditor.SpellData = existingData;
            }
            else
            {
                var newData = fsData.CreateDictionary();
                var fields = newData.AsDictionary;

                var classicSpellRecord = classicSpellRecords[spellIndex];

                fields["spellName"] = new fsData(classicSpellRecord.spellName);
                fields["element"] = new fsData(classicSpellRecord.element);
                fields["rangeType"] = new fsData(classicSpellRecord.rangeType);
                fields["cost"] = new fsData(classicSpellRecord.cost);
                fields["index"] = new fsData(classicSpellRecord.index);
                fields["icon"] = new fsData(classicSpellRecord.icon);
                var effects = fields["effects"] = fsData.CreateList();
                foreach(EffectRecordData effectRecord in classicSpellRecord.effects)
                {
                    if (effectRecord.type == -1)
                        continue;

                    var effect = fsData.CreateDictionary();

                    effect.AsDictionary.Add("type", new fsData(effectRecord.type));
                    effect.AsDictionary.Add("subType", new fsData(effectRecord.subType));
                    effect.AsDictionary.Add("durationBase", new fsData(effectRecord.durationBase));
                    effect.AsDictionary.Add("durationMod", new fsData(effectRecord.durationMod));
                    effect.AsDictionary.Add("durationPerLevel", new fsData(effectRecord.durationPerLevel));
                    effect.AsDictionary.Add("chanceBase", new fsData(effectRecord.chanceBase));
                    effect.AsDictionary.Add("chanceMod", new fsData(effectRecord.chanceMod));
                    effect.AsDictionary.Add("chancePerLevel", new fsData(effectRecord.chancePerLevel));
                    effect.AsDictionary.Add("magnitudeBaseLow", new fsData(effectRecord.magnitudeBaseLow));
                    effect.AsDictionary.Add("magnitudeBaseHigh", new fsData(effectRecord.magnitudeBaseHigh));
                    effect.AsDictionary.Add("magnitudeLevelBase", new fsData(effectRecord.magnitudeLevelBase));
                    effect.AsDictionary.Add("magnitudeLevelHigh", new fsData(effectRecord.magnitudeLevelHigh));
                    effect.AsDictionary.Add("magnitudePerLevel", new fsData(effectRecord.magnitudePerLevel));

                    effects.AsList.Add(effect);
                }

                BundleEditor.SpellData = newData;
            }            
            
            BundleEditor.OnSpellConfirmed += record => BundleEditor_OnEditSpellConfirmed(record, spellIndex);

            userInterfaceManager.PushWindow(BundleEditor);
        }

        private void SetRevertDisabled()
        {
            if (buttonRevert.Label.TextColor == Color.gray / 2.0f)
                return;

            buttonRevert.Label.TextColor = Color.gray / 2.0f;
            buttonRevert.OnMouseClick -= OnRevertClick;
        }

        private void SetRevertEnabled()
        {
            if (buttonRevert.Label.TextColor == DaggerfallUI.DaggerfallDefaultTextColor)
                return;

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
            SetSaveEnabled();
        }

        private void ButtonSave_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            Save();
        }

        void Save()
        {
#if UNITY_EDITOR
            Mod mod = ModManager.Instance.GetMod(ModTitle);
            var dfModFile = mod.ModInfo.Files.Find(filepath => Path.GetFileName(filepath).EndsWith("dfmod.json"));

            var modFolder = dfModFile.Replace("Assets/Game/Mods/", "");
            modFolder = modFolder.Replace(Path.GetFileName(modFolder), "");

            string spellRecordsFullPath = Path.Combine(ModManager.EditorModsDirectory, modFolder, "SpellRecords.json");
            using (var FileWriter = File.CreateText(spellRecordsFullPath))
            {
                fsData fileOutput = fsData.CreateList();
                fileOutput.AsList.AddRange(spellRecords.Values);

                fsJsonPrinter.PrettyJson(fileOutput, FileWriter);
            }

            SetSaveDisabled();
#endif
        }

        void SetSaveEnabled()
        {
            if (buttonSave.Label.TextColor == DaggerfallUI.DaggerfallDefaultTextColor)
                return;

            buttonSave.Label.TextColor = DaggerfallUI.DaggerfallDefaultTextColor;
            buttonSave.OnMouseClick += ButtonSave_OnMouseClick;
        }

        void SetSaveDisabled()
        {
            if (buttonSave.Label.TextColor == Color.gray / 2.0f)
                return;

            buttonSave.Label.TextColor = Color.gray / 2.0f;
            buttonSave.OnMouseClick -= ButtonSave_OnMouseClick;
        }

        bool HasUnsavedChanges()
        {
            return buttonSave.Label.TextColor == DaggerfallUI.DaggerfallDefaultTextColor;
        }
    }
}