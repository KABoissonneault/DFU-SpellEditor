using DaggerfallConnect;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Utility.AssetInjection;
using FullSerializer;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpellEditorMod
{
    public class SpellBundleEditor : DaggerfallPopupWindow
    {
        #region UI Rects

        protected DFSize selectedIconsBaseSize = new DFSize(40, 80);

        protected Vector2 tipLabelPos = new Vector2(5, 22);
        protected Vector2 nameLabelPos = new Vector2(60, 185);
        protected Rect effect1NameRect = new Rect(3, 30, 230, 9);
        protected Rect effect2NameRect = new Rect(3, 62, 230, 9);
        protected Rect effect3NameRect = new Rect(3, 94, 230, 9);
        protected Rect addEffectButtonRect = new Rect(244, 114, 28, 28);
        protected Rect buyButtonRect = new Rect(244, 163, 24, 16);
        protected Rect exitButtonRect = new Rect(244, 179, 24, 16);
        protected Rect casterOnlyButtonRect = new Rect(275, 114, 24, 16);
        protected Rect byTouchButtonRect = new Rect(275, 130, 24, 16);
        protected Rect singleTargetAtRangeButtonRect = new Rect(275, 146, 24, 16);
        protected Rect areaAroundCasterButtonRect = new Rect(275, 162, 24, 16);
        protected Rect areaAtRangeButtonRect = new Rect(275, 178, 24, 16);
        protected Rect fireBasedButtonRect = new Rect(299, 114, 16, 16);
        protected Rect coldBasedButtonRect = new Rect(299, 130, 16, 16);
        protected Rect poisonBasedButtonRect = new Rect(299, 146, 16, 16);
        protected Rect shockBasedButtonRect = new Rect(299, 162, 16, 16);
        protected Rect magicBasedButtonRect = new Rect(299, 178, 16, 16);
        protected Rect nextIconButtonRect = new Rect(275, 80, 9, 16);
        protected Rect previousIconButtonRect = new Rect(275, 96, 9, 16);
        protected Rect selectIconButtonRect = new Rect(288, 94, 16, 16);
        protected Rect nameSpellButtonRect = new Rect(59, 184, 142, 7);

        protected Rect casterOnlySubRect = new Rect(0, 0, 24, 16);
        protected Rect byTouchSubRect = new Rect(0, 16, 24, 16);
        protected Rect singleTargetAtRangeSubRect = new Rect(0, 32, 24, 16);
        protected Rect areaAroundCasterSubRect = new Rect(0, 48, 24, 16);
        protected Rect areaAtRangeSubRect = new Rect(0, 64, 24, 16);

        protected Rect fireBasedSubRect = new Rect(24, 0, 16, 16);
        protected Rect coldBasedSubRect = new Rect(24, 16, 16, 16);
        protected Rect poisonBasedSubRect = new Rect(24, 32, 16, 16);
        protected Rect shockBasedSubRect = new Rect(24, 48, 16, 16);
        protected Rect magicBasedSubRect = new Rect(24, 64, 16, 16);

        #endregion

        #region UI Controls

        protected TextLabel tipLabel;
        protected TextLabel effect1NameLabel;
        protected TextLabel effect2NameLabel;
        protected TextLabel effect3NameLabel;
        protected TextLabel goldCostLabel;
        protected TextLabel spellSkillLevel1Label;
        protected TextLabel spellSkillLevel2Label;
        protected TextLabel spellSkillLevel3Label;
        protected TextLabel spellPointCost1Label;
        protected TextLabel spellPointCost2Label;
        protected TextLabel spellPointCost3Label;
        protected TextLabel spellNameLabel;

        protected DaggerfallListPickerWindow effectGroupPicker;
        protected DaggerfallListPickerWindow effectSubGroupPicker;
        protected SpellEffectEditor effectEditor;
        protected SpellIconPickerWindow iconPicker;

        protected Button selectIconButton;

        protected Button casterOnlyButton;
        protected Button byTouchButton;
        protected Button singleTargetAtRangeButton;
        protected Button areaAroundCasterButton;
        protected Button areaAtRangeButton;

        protected Button fireBasedButton;
        protected Button coldBasedButton;
        protected Button poisonBasedButton;
        protected Button shockBasedButton;
        protected Button magicBasedButton;

        #endregion

        #region UI Textures

        protected Texture2D baseTexture;
        protected Texture2D selectedIconsTexture;

        protected Texture2D casterOnlySelectedTexture;
        protected Texture2D byTouchSelectedTexture;
        protected Texture2D singleTargetAtRangeSelectedTexture;
        protected Texture2D areaAroundCasterSelectedTexture;
        protected Texture2D areaAtRangeSelectedTexture;

        protected Texture2D fireBasedSelectedTexture;
        protected Texture2D coldBasedSelectedTexture;
        protected Texture2D poisonBasedSelectedTexture;
        protected Texture2D shockBasedSelectedTexture;
        protected Texture2D magicBasedSelectedTexture;

        #endregion

        #region Fields

        protected const MagicCraftingStations thisMagicStation = MagicCraftingStations.SpellMaker;

        protected const string baseTextureFilename = "SpellBundleEditorMask.png";
        protected const string goldSelectIconsFilename = "MASK01I0.IMG";
        protected const string colorSelectIconsFilename = "MASK04I0.IMG";

        protected const int alternateAlphaIndex = 12;
        protected const int maxEffectsPerSpell = 3;
        protected const int defaultSpellIcon = 1;
        protected const TargetTypes defaultTargetFlags = EntityEffectBroker.TargetFlags_All;
        protected const ElementTypes defaultElementFlags = EntityEffectBroker.ElementFlags_MagicOnly;

        protected const SoundClips inscribeGrimoire = SoundClips.ParchmentScratching;

        List<IEntityEffect> enumeratedEffectTemplates = new List<IEntityEffect>();

        EffectEntry[] effectEntries = new EffectEntry[maxEffectsPerSpell];

        protected int editOrDeleteSlot = -1;
        protected TargetTypes allowedTargets = defaultTargetFlags;
        protected ElementTypes allowedElements = defaultElementFlags;
        protected TargetTypes selectedTarget = TargetTypes.CasterOnly;
        protected ElementTypes selectedElement = ElementTypes.Magic;
        protected SpellIcon selectedIcon;

        int totalGoldCost = 0;
        int totalSpellPointCost1 = 0;
        int totalSpellPointCost2 = 0;
        int totalSpellPointCost3 = 0;

        protected EffectEntry[] EffectEntries { get { return effectEntries; } }

        #endregion

        public fsData SpellData
        {
            get;
            set;
        }

        public delegate void SpellConfirmedDelegate(fsData spellData);
        public event SpellConfirmedDelegate OnSpellConfirmed;

        #region Constructors

        public SpellBundleEditor(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous = null)
            : base(uiManager, previous)
        {

        }

        #endregion

        #region Setup Methods

        protected override void Setup()
        {
            // Load all the textures used by spell maker window
            LoadTextures();

            // Always dim background
            ParentPanel.BackgroundColor = ScreenDimColor;

            // Setup native panel background
            NativePanel.BackgroundColor = new Color(0, 0, 0, 0.75f);
            NativePanel.BackgroundTexture = baseTexture;

            // Setup controls
            SetupLabels();
            SetupButtons();
            SetupPickers();

            SetExistingSpell();

            // Setup effect editor window
            effectEditor = new SpellEffectEditor(uiManager, this);
            effectEditor.OnClose += EffectEditor_OnClose;

            // Setup icon picker
            iconPicker = (SpellIconPickerWindow)UIWindowFactory.GetInstance(UIWindowType.SpellIconPicker, uiManager, this);
            iconPicker.OnClose += IconPicker_OnClose;
        }

        public override void OnPush()
        {
            InitEffectSlots();

            SetDefaults();
        }

        void SetExistingSpell()
        {
            if (SpellData == null)
                return;

            var fields = SpellData.AsDictionary;

            fsData currentValue;
            if (fields.TryGetValue("spellName", out currentValue))
            {
                spellNameLabel.Text = currentValue.AsString;
            }
            else
            {
                spellNameLabel.Text = string.Empty;
            }

            if (fields.TryGetValue("element", out currentValue))
            {
                ElementTypes elementType = (ElementTypes)(1 << (int)currentValue.AsInt64);
                allowedElements |= elementType;
                SetSpellElement(elementType);
            }
            else
            {
                SetSpellElement(ElementTypes.Magic);
            }

            if (fields.TryGetValue("rangeType", out currentValue))
            {
                TargetTypes targetType = (TargetTypes)(1 << (int)currentValue.AsInt64);
                allowedTargets |= targetType;
                SetSpellTarget(targetType);
            }
            else
            {
                SetSpellTarget(TargetTypes.CasterOnly);
            }

            if (fields.TryGetValue("icon", out currentValue))
            {
                selectedIcon.index = (int)currentValue.AsInt64;
            }
            else
            {
                selectedIcon.index = defaultSpellIcon;
            }

            effect1NameLabel.Text = string.Empty;
            effect2NameLabel.Text = string.Empty;
            effect3NameLabel.Text = string.Empty;
            if (fields.TryGetValue("effects", out currentValue))
            {
                var effects = currentValue.AsList;
                for (int i = 0; i < effects.Count; ++i)
                {
                    var effectObject = effects[i];
                    var effectFields = effectObject.AsDictionary;

                    int type = (int)effectFields["type"].AsInt64;
                    int subType = (int)effectFields["subType"].AsInt64;
                    subType = (subType < 0) ? 255 : subType; // Entity effect keys use 255 instead of -1 for subtyp

                    int classicKey = BaseEntityEffect.MakeClassicKey((byte)type, (byte)subType);

                    IEntityEffect effectTemplate = GameManager.Instance.EntityEffectBroker.GetEffectTemplate(classicKey);

                    ref EffectEntry entry = ref effectEntries[i];
                    entry.Key = effectTemplate.Key;

                    if (effectTemplate.Properties.SupportDuration)
                    {
                        entry.Settings.DurationBase = (int)effectFields["durationBase"].AsInt64;
                        entry.Settings.DurationPlus = (int)effectFields["durationMod"].AsInt64;
                        entry.Settings.DurationPerLevel = (int)effectFields["durationPerLevel"].AsInt64;
                    }

                    if (effectTemplate.Properties.SupportChance)
                    {
                        entry.Settings.ChanceBase = (int)effectFields["chanceBase"].AsInt64;
                        entry.Settings.ChancePlus = (int)effectFields["chanceMod"].AsInt64;
                        entry.Settings.ChancePerLevel = (int)effectFields["chancePerLevel"].AsInt64;
                    }

                    if (effectTemplate.Properties.SupportMagnitude)
                    {
                        entry.Settings.MagnitudeBaseMin = (int)effectFields["magnitudeBaseLow"].AsInt64;
                        entry.Settings.MagnitudeBaseMax = (int)effectFields["magnitudeBaseHigh"].AsInt64;
                        entry.Settings.MagnitudePlusMin = (int)effectFields["magnitudeLevelBase"].AsInt64;
                        entry.Settings.MagnitudePlusMax = (int)effectFields["magnitudeLevelHigh"].AsInt64;
                        entry.Settings.MagnitudePerLevel = (int)effectFields["magnitudePerLevel"].AsInt64;
                    }

                    switch (i)
                    {
                        case 0:
                            effect1NameLabel.Text = effectTemplate.DisplayName;
                            break;
                        case 1:
                            effect2NameLabel.Text = effectTemplate.DisplayName;
                            break;
                        case 2:
                            effect3NameLabel.Text = effectTemplate.DisplayName;
                            break;
                    }
                }
            }

            UpdateAllowedButtons();
                        
            int[] mods = new int[(int)DFCareer.Skills.Count];

            // Spell cost 1
            foreach (EffectEntry effectEntry in effectEntries)
            {
                if (string.IsNullOrEmpty(effectEntry.Key))
                    continue;

                var effectTemplate = GameManager.Instance.EntityEffectBroker.GetEffectTemplate(effectEntry.Key);
                short permanentValue = GameManager.Instance.PlayerEntity.Skills.GetPermanentSkillValue((DFCareer.Skills)effectTemplate.Properties.MagicSkill);
                mods[(int)effectTemplate.Properties.MagicSkill] = 5 - permanentValue;
            }

            GameManager.Instance.PlayerEntity.Skills.AssignMods(mods);
            (totalGoldCost, totalSpellPointCost1) = FormulaHelper.CalculateTotalEffectCosts(effectEntries, selectedTarget);

            // Spell cost 2
            foreach (EffectEntry effectEntry in effectEntries)
            {
                if (string.IsNullOrEmpty(effectEntry.Key))
                    continue;

                var effectTemplate = GameManager.Instance.EntityEffectBroker.GetEffectTemplate(effectEntry.Key);
                short permanentValue = GameManager.Instance.PlayerEntity.Skills.GetPermanentSkillValue((DFCareer.Skills)effectTemplate.Properties.MagicSkill);
                mods[(int)effectTemplate.Properties.MagicSkill] = 50 - permanentValue;
            }

            GameManager.Instance.PlayerEntity.Skills.AssignMods(mods);
            totalSpellPointCost2 = FormulaHelper.CalculateTotalEffectCosts(effectEntries, selectedTarget).spellPointCost;

            // Spell cost 3
            foreach (EffectEntry effectEntry in effectEntries)
            {
                if (string.IsNullOrEmpty(effectEntry.Key))
                    continue;

                var effectTemplate = GameManager.Instance.EntityEffectBroker.GetEffectTemplate(effectEntry.Key);
                short permanentValue = GameManager.Instance.PlayerEntity.Skills.GetPermanentSkillValue((DFCareer.Skills)effectTemplate.Properties.MagicSkill);
                mods[(int)effectTemplate.Properties.MagicSkill] = 100 - permanentValue;
            }

            GameManager.Instance.PlayerEntity.Skills.AssignMods(mods);
            totalSpellPointCost3 = FormulaHelper.CalculateTotalEffectCosts(effectEntries, selectedTarget).spellPointCost;

            SetIcon(selectedIcon);
            SetStatusLabels();
        }

        protected virtual void SetDefaults()
        {
            allowedTargets = defaultTargetFlags;
            allowedElements = defaultElementFlags;
            selectedIcon = new SpellIcon()
            {
                index = defaultSpellIcon,
            };
            editOrDeleteSlot = -1;

            for (int i = 0; i < maxEffectsPerSpell; i++)
            {
                effectEntries[i] = new EffectEntry();
            }

            if (IsSetup)
            {
                SetExistingSpell();
            }
        }

        #endregion

        #region Protected Methods

        protected virtual void LoadTextures()
        {
            // Load source textures
            TextureReplacement.TryImportImage(baseTextureFilename, true, out baseTexture);
            selectedIconsTexture = ImageReader.GetTexture(goldSelectIconsFilename);

            // Load target texture
            casterOnlySelectedTexture = ImageReader.GetSubTexture(selectedIconsTexture, casterOnlySubRect, selectedIconsBaseSize);
            byTouchSelectedTexture = ImageReader.GetSubTexture(selectedIconsTexture, byTouchSubRect, selectedIconsBaseSize);
            singleTargetAtRangeSelectedTexture = ImageReader.GetSubTexture(selectedIconsTexture, singleTargetAtRangeSubRect, selectedIconsBaseSize);
            areaAroundCasterSelectedTexture = ImageReader.GetSubTexture(selectedIconsTexture, areaAroundCasterSubRect, selectedIconsBaseSize);
            areaAtRangeSelectedTexture = ImageReader.GetSubTexture(selectedIconsTexture, areaAtRangeSubRect, selectedIconsBaseSize);

            fireBasedSelectedTexture = ImageReader.GetSubTexture(selectedIconsTexture, fireBasedSubRect, selectedIconsBaseSize);
            coldBasedSelectedTexture = ImageReader.GetSubTexture(selectedIconsTexture, coldBasedSubRect, selectedIconsBaseSize);
            poisonBasedSelectedTexture = ImageReader.GetSubTexture(selectedIconsTexture, poisonBasedSubRect, selectedIconsBaseSize);
            shockBasedSelectedTexture = ImageReader.GetSubTexture(selectedIconsTexture, shockBasedSubRect, selectedIconsBaseSize);
            magicBasedSelectedTexture = ImageReader.GetSubTexture(selectedIconsTexture, magicBasedSubRect, selectedIconsBaseSize);
        }

        protected virtual void SetupLabels()
        {
            // Tip label
            tipLabel = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, tipLabelPos, string.Empty, NativePanel);

            // Status labels
            goldCostLabel = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(59, 158), string.Empty, NativePanel);

            spellSkillLevel1Label = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(74, 167), string.Empty, NativePanel);
            spellSkillLevel1Label.Text = "5";
            spellSkillLevel2Label = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(95, 167), string.Empty, NativePanel);
            spellSkillLevel2Label.Text = "50";
            spellSkillLevel3Label = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(116, 167), string.Empty, NativePanel);
            spellSkillLevel3Label.Text = "100";

            spellPointCost1Label = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(74, 176), string.Empty, NativePanel);
            spellPointCost2Label = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(95, 176), string.Empty, NativePanel);
            spellPointCost3Label = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(116, 176), string.Empty, NativePanel);

            // Name label
            spellNameLabel = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, nameLabelPos, string.Empty, NativePanel);
            spellNameLabel.ShadowPosition = Vector2.zero;

            // Effect1
            Panel effect1NamePanel = DaggerfallUI.AddPanel(effect1NameRect, NativePanel);
            effect1NamePanel.HorizontalAlignment = HorizontalAlignment.Center;
            effect1NamePanel.OnMouseClick += Effect1NamePanel_OnMouseClick;
            effect1NameLabel = DaggerfallUI.AddTextLabel(DaggerfallUI.LargeFont, Vector2.zero, string.Empty, effect1NamePanel);
            effect1NameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            effect1NameLabel.ShadowPosition = Vector2.zero;

            // Effect2
            Panel effect2NamePanel = DaggerfallUI.AddPanel(effect2NameRect, NativePanel);
            effect2NamePanel.HorizontalAlignment = HorizontalAlignment.Center;
            effect2NamePanel.OnMouseClick += Effect2NamePanel_OnMouseClick;
            effect2NameLabel = DaggerfallUI.AddTextLabel(DaggerfallUI.LargeFont, Vector2.zero, string.Empty, effect2NamePanel);
            effect2NameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            effect2NameLabel.ShadowPosition = Vector2.zero;

            // Effect3
            Panel effect3NamePanel = DaggerfallUI.AddPanel(effect3NameRect, NativePanel);
            effect3NamePanel.HorizontalAlignment = HorizontalAlignment.Center;
            effect3NamePanel.OnMouseClick += Effect3NamePanel_OnMouseClick;
            effect3NameLabel = DaggerfallUI.AddTextLabel(DaggerfallUI.LargeFont, Vector2.zero, string.Empty, effect3NamePanel);
            effect3NameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            effect3NameLabel.ShadowPosition = Vector2.zero;
        }

        protected virtual void SetupPickers()
        {
            // Use a picker for effect group
            effectGroupPicker = new DaggerfallListPickerWindow(uiManager, this);
            effectGroupPicker.ListBox.OnUseSelectedItem += AddEffectGroupListBox_OnUseSelectedItem;

            // Use another picker for effect subgroup
            // This allows user to hit escape and return to effect group list, unlike classic which dumps whole spellmaker UI
            effectSubGroupPicker = new DaggerfallListPickerWindow(uiManager, this);
            effectSubGroupPicker.ListBox.OnUseSelectedItem += AddEffectSubGroup_OnUseSelectedItem;
        }

        protected virtual void SetupButtons()
        {
            // Control
            AddTipButton(addEffectButtonRect, "addEffect", AddEffectButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerAddEffect);
            AddTipButton(buyButtonRect, "buySpell", BuyButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerBuySpell);
            AddTipButton(exitButtonRect, "exit", ExitButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerExit);
            AddTipButton(nameSpellButtonRect, "nameSpell", NameSpellButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerNameSpell);

            // Target
            casterOnlyButton = AddTipButton(casterOnlyButtonRect, "casterOnly", CasterOnlyButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerTargetCaster);
            byTouchButton = AddTipButton(byTouchButtonRect, "byTouch", ByTouchButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerTargetTouch);
            singleTargetAtRangeButton = AddTipButton(singleTargetAtRangeButtonRect, "singleTargetAtRange", SingleTargetAtRangeButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerTargetSingleAtRange);
            areaAroundCasterButton = AddTipButton(areaAroundCasterButtonRect, "areaAroundCaster", AreaAroundCasterButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerTargetAroundCaster);
            areaAtRangeButton = AddTipButton(areaAtRangeButtonRect, "areaAtRange", AreaAtRangeButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerTargetAreaAtRange);

            // Element
            fireBasedButton = AddTipButton(fireBasedButtonRect, "fireBased", FireBasedButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerElementFire);
            coldBasedButton = AddTipButton(coldBasedButtonRect, "coldBased", ColdBasedButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerElementCold);
            poisonBasedButton = AddTipButton(poisonBasedButtonRect, "poisonBased", PoisonBasedButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerElementPoison);
            shockBasedButton = AddTipButton(shockBasedButtonRect, "shockBased", ShockBasedButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerElementShock);
            magicBasedButton = AddTipButton(magicBasedButtonRect, "magicBased", MagicBasedButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerElementMagic);

            // Icons
            AddTipButton(nextIconButtonRect, "nextIcon", NextIconButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerNextIcon);
            AddTipButton(previousIconButtonRect, "previousIcon", PreviousIconButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerPrevIcon);
            selectIconButton = AddTipButton(selectIconButtonRect, "selectIcon", SelectIconButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerSelectIcon);

            // Select default buttons
            UpdateAllowedButtons();
        }

        protected virtual void SetIcon(SpellIcon icon)
        {
            // Fallback to classic index if no valid icon pack key
            if (string.IsNullOrEmpty(icon.key) || !DaggerfallUI.Instance.SpellIconCollection.HasPack(icon.key))
            {
                icon.key = string.Empty;
                icon.index = icon.index % DaggerfallUI.Instance.SpellIconCollection.SpellIconCount;
            }

            // Set icon
            selectedIcon = icon;
            selectIconButton.BackgroundTexture = DaggerfallUI.Instance.SpellIconCollection.GetSpellIcon(selectedIcon);
        }

        protected virtual void SetStatusLabels()
        {
            goldCostLabel.Text = totalGoldCost.ToString();
            spellPointCost1Label.Text = totalSpellPointCost1.ToString();
            spellPointCost2Label.Text = totalSpellPointCost2.ToString();
            spellPointCost3Label.Text = totalSpellPointCost3.ToString();
        }

        #endregion

        #region Private Methods

        protected virtual Button AddTipButton(Rect rect, string tipID, BaseScreenComponent.OnMouseClickHandler handler, DaggerfallShortcut.Buttons button)
        {
            Button tipButton = DaggerfallUI.AddButton(rect, NativePanel);
            tipButton.OnMouseEnter += TipButton_OnMouseEnter;
            tipButton.OnMouseLeave += TipButton_OnMouseLeave;
            tipButton.OnMouseClick += handler;
            tipButton.Hotkey = DaggerfallShortcut.GetBinding(button);
            tipButton.Tag = tipID;

            return tipButton;
        }

        protected virtual void InitEffectSlots()
        {
            effectEntries = new EffectEntry[maxEffectsPerSpell];
            for (int i = 0; i < maxEffectsPerSpell; i++)
            {
                effectEntries[i] = new EffectEntry();
            }
        }

        protected virtual void ClearPendingDeleteEffectSlot()
        {
            if (editOrDeleteSlot == -1)
                return;

            effectEntries[editOrDeleteSlot] = new EffectEntry();
            UpdateSlotText(editOrDeleteSlot, string.Empty);
            editOrDeleteSlot = -1;
            UpdateAllowedButtons();
        }

        protected int GetFirstFreeEffectSlotIndex()
        {
            for (int i = 0; i < maxEffectsPerSpell; i++)
            {
                if (string.IsNullOrEmpty(effectEntries[i].Key))
                    return i;
            }

            return -1;
        }

        protected int GetFirstUsedEffectSlotIndex()
        {
            for (int i = 0; i < maxEffectsPerSpell; i++)
            {
                if (!string.IsNullOrEmpty(effectEntries[i].Key))
                    return i;
            }

            return -1;
        }

        protected int CountUsedEffectSlots()
        {
            int total = 0;
            for (int i = 0; i < maxEffectsPerSpell; i++)
            {
                if (!string.IsNullOrEmpty(effectEntries[i].Key))
                    total++;
            }

            return total;
        }

        protected virtual void UpdateSlotText(int slot, string text)
        {
            // Get text label to update
            TextLabel label = null;
            switch (slot)
            {
                case 0:
                    label = effect1NameLabel;
                    break;
                case 1:
                    label = effect2NameLabel;
                    break;
                case 2:
                    label = effect3NameLabel;
                    break;
                default:
                    return;
            }

            // Set label text
            label.Text = text;
        }

        protected virtual void AddAndEditSlot(IEntityEffect effectTemplate)
        {
            effectEditor.EffectTemplate = effectTemplate;
            int slot = GetFirstFreeEffectSlotIndex();
            effectEntries[slot] = effectEditor.EffectEntry;
            UpdateSlotText(slot, effectEditor.EffectTemplate.DisplayName);
            UpdateAllowedButtons();
            editOrDeleteSlot = slot;
            uiManager.PushWindow(effectEditor);
        }

        protected virtual void EditOrDeleteSlot(int slot)
        {
            const int howToAlterSpell = 1708;

            // Do nothing if slot not set
            if (string.IsNullOrEmpty(effectEntries[slot].Key))
                return;

            // Offer to edit or delete effect
            editOrDeleteSlot = slot;
            DaggerfallMessageBox mb = new DaggerfallMessageBox(uiManager, this);
            mb.SetTextTokens(howToAlterSpell);
            Button editButton = mb.AddButton(DaggerfallMessageBox.MessageBoxButtons.Edit);
            editButton.OnMouseClick += EditButton_OnMouseClick;
            Button deleteButton = mb.AddButton(DaggerfallMessageBox.MessageBoxButtons.Delete);
            deleteButton.OnMouseClick += DeleteButton_OnMouseClick;
            mb.OnButtonClick += EditOrDeleteSpell_OnButtonClick;
            mb.OnCancel += EditOrDeleteSpell_OnCancel;
            mb.Show();
        }

        protected virtual void SetSpellTarget(TargetTypes targetType)
        {
            // Exclude target types based on effects added
            if ((allowedTargets & targetType) == TargetTypes.None)
                return;

            // Clear buttons
            casterOnlyButton.BackgroundTexture = null;
            byTouchButton.BackgroundTexture = null;
            singleTargetAtRangeButton.BackgroundTexture = null;
            areaAroundCasterButton.BackgroundTexture = null;
            areaAtRangeButton.BackgroundTexture = null;

            // Set selected icon
            switch (targetType)
            {
                case TargetTypes.CasterOnly:
                    casterOnlyButton.BackgroundTexture = casterOnlySelectedTexture;
                    break;
                case TargetTypes.ByTouch:
                    byTouchButton.BackgroundTexture = byTouchSelectedTexture;
                    break;
                case TargetTypes.SingleTargetAtRange:
                    singleTargetAtRangeButton.BackgroundTexture = singleTargetAtRangeSelectedTexture;
                    break;
                case TargetTypes.AreaAroundCaster:
                    areaAroundCasterButton.BackgroundTexture = areaAroundCasterSelectedTexture;
                    break;
                case TargetTypes.AreaAtRange:
                    areaAtRangeButton.BackgroundTexture = areaAtRangeSelectedTexture;
                    break;
            }

            selectedTarget = targetType;
            UpdateSpellCosts();
        }

        protected virtual void SetSpellElement(ElementTypes elementType)
        {
            // Exclude element types based on effects added
            if ((allowedElements & elementType) == ElementTypes.None)
                return;

            // Clear buttons
            fireBasedButton.BackgroundTexture = null;
            coldBasedButton.BackgroundTexture = null;
            poisonBasedButton.BackgroundTexture = null;
            shockBasedButton.BackgroundTexture = null;
            magicBasedButton.BackgroundTexture = null;

            // Set selected icon
            switch (elementType)
            {
                case ElementTypes.Fire:
                    fireBasedButton.BackgroundTexture = fireBasedSelectedTexture;
                    break;
                case ElementTypes.Cold:
                    coldBasedButton.BackgroundTexture = coldBasedSelectedTexture;
                    break;
                case ElementTypes.Poison:
                    poisonBasedButton.BackgroundTexture = poisonBasedSelectedTexture;
                    break;
                case ElementTypes.Shock:
                    shockBasedButton.BackgroundTexture = shockBasedSelectedTexture;
                    break;
                case ElementTypes.Magic:
                    magicBasedButton.BackgroundTexture = magicBasedSelectedTexture;
                    break;
            }

            selectedElement = elementType;
        }

        protected virtual void UpdateAllowedButtons()
        {
            // Set defaults when no effects added
            if (GetFirstUsedEffectSlotIndex() == -1)
            {
                allowedTargets = defaultTargetFlags;
                allowedElements = defaultElementFlags;
                SetSpellTarget(TargetTypes.CasterOnly);
                SetSpellElement(ElementTypes.Magic);
                EnforceSelectedButtons();
                return;
            }

            // Combine flags
            allowedTargets = EntityEffectBroker.TargetFlags_All;
            allowedElements = EntityEffectBroker.ElementFlags_MagicOnly;
            for (int i = 0; i < maxEffectsPerSpell; i++)
            {
                // Must be a valid entry
                if (!string.IsNullOrEmpty(effectEntries[i].Key))
                {
                    // Get effect template
                    IEntityEffect effectTemplate = GameManager.Instance.EntityEffectBroker.GetEffectTemplate(effectEntries[i].Key);

                    // Allowed targets are least permissive result set from combined target flags
                    allowedTargets = allowedTargets & effectTemplate.Properties.AllowedTargets;

                    // Allowed elements are most permissive result set from combined element flags (magic always allowed)
                    allowedElements = allowedElements | effectTemplate.Properties.AllowedElements;
                }
            }

            // Ensure a valid button is selected
            EnforceSelectedButtons();
        }

        protected void EnforceSelectedButtons()
        {
            if ((allowedTargets & selectedTarget) == TargetTypes.None)
                SelectFirstAllowedTargetType();

            if ((allowedElements & selectedElement) == ElementTypes.None)
                SelectFirstAllowedElementType();
        }

        protected virtual void SelectFirstAllowedTargetType()
        {
            if ((allowedTargets & TargetTypes.CasterOnly) == TargetTypes.CasterOnly)
            {
                SetSpellTarget(TargetTypes.CasterOnly);
                return;
            }
            else if ((allowedTargets & TargetTypes.ByTouch) == TargetTypes.ByTouch)
            {
                SetSpellTarget(TargetTypes.ByTouch);
                return;
            }
            else if ((allowedTargets & TargetTypes.SingleTargetAtRange) == TargetTypes.SingleTargetAtRange)
            {
                SetSpellTarget(TargetTypes.SingleTargetAtRange);
                return;
            }
            else if ((allowedTargets & TargetTypes.AreaAroundCaster) == TargetTypes.AreaAroundCaster)
            {
                SetSpellTarget(TargetTypes.AreaAroundCaster);
                return;
            }
            else if ((allowedTargets & TargetTypes.AreaAtRange) == TargetTypes.AreaAtRange)
            {
                SetSpellTarget(TargetTypes.AreaAtRange);
                return;
            }
        }

        protected virtual void SelectFirstAllowedElementType()
        {
            if ((allowedElements & ElementTypes.Fire) == ElementTypes.Fire)
            {
                SetSpellElement(ElementTypes.Fire);
                return;
            }
            else if ((allowedElements & ElementTypes.Cold) == ElementTypes.Cold)
            {
                SetSpellElement(ElementTypes.Cold);
                return;
            }
            else if ((allowedElements & ElementTypes.Poison) == ElementTypes.Poison)
            {
                SetSpellElement(ElementTypes.Poison);
                return;
            }
            else if ((allowedElements & ElementTypes.Shock) == ElementTypes.Shock)
            {
                SetSpellElement(ElementTypes.Shock);
                return;
            }
            else if ((allowedElements & ElementTypes.Magic) == ElementTypes.Magic)
            {
                SetSpellElement(ElementTypes.Magic);
                return;
            }
        }

        protected List<EffectEntry> GetEffectEntries()
        {
            // Get a list of actual effect entries and ignore empty slots
            List<EffectEntry> effects = new List<EffectEntry>();
            for (int i = 0; i < maxEffectsPerSpell; i++)
            {
                if (!string.IsNullOrEmpty(effectEntries[i].Key))
                    effects.Add(effectEntries[i]);
            }

            return effects;
        }

        protected void UpdateSpellCosts()
        {
            // Note: Daggerfall shows gold cost 0 and spellpoint cost 5 with no effects added
            // Not copying this behaviour at this time intentionally as it seems unclear for an invalid
            // spell to have any casting cost at all - may change later
            totalSpellPointCost1 = 0;
            totalSpellPointCost2 = 0;
            totalSpellPointCost3 = 0;

            // Do nothing when effect editor not setup or not used effect slots
            // This means there is nothing to calculate
            if ((editOrDeleteSlot != -1 && (effectEditor == null || !effectEditor.IsSetup)) || CountUsedEffectSlots() == 0)
            {
                SetStatusLabels();
                return;
            }

            // Update slot being edited with current effect editor settings
            if (editOrDeleteSlot != -1)
                effectEntries[editOrDeleteSlot] = effectEditor.EffectEntry;

            // Get total costs
            int[] mods = new int[(int)DFCareer.Skills.Count];

            // Spell cost 1
            foreach (EffectEntry effectEntry in effectEntries)
            {
                if (string.IsNullOrEmpty(effectEntry.Key))
                    continue;

                var effectTemplate = GameManager.Instance.EntityEffectBroker.GetEffectTemplate(effectEntry.Key);
                short permanentValue = GameManager.Instance.PlayerEntity.Skills.GetPermanentSkillValue((DFCareer.Skills)effectTemplate.Properties.MagicSkill);
                mods[(int)effectTemplate.Properties.MagicSkill] = 5 - permanentValue;
            }

            GameManager.Instance.PlayerEntity.Skills.AssignMods(mods);
            (totalGoldCost, totalSpellPointCost1) = FormulaHelper.CalculateTotalEffectCosts(effectEntries, selectedTarget);

            // Spell cost 2
            foreach (EffectEntry effectEntry in effectEntries)
            {
                if (string.IsNullOrEmpty(effectEntry.Key))
                    continue;

                var effectTemplate = GameManager.Instance.EntityEffectBroker.GetEffectTemplate(effectEntry.Key);
                short permanentValue = GameManager.Instance.PlayerEntity.Skills.GetPermanentSkillValue((DFCareer.Skills)effectTemplate.Properties.MagicSkill);
                mods[(int)effectTemplate.Properties.MagicSkill] = 50 - permanentValue;
            }

            GameManager.Instance.PlayerEntity.Skills.AssignMods(mods);
            totalSpellPointCost2 = FormulaHelper.CalculateTotalEffectCosts(effectEntries, selectedTarget).spellPointCost;

            // Spell cost 3
            foreach (EffectEntry effectEntry in effectEntries)
            {
                if (string.IsNullOrEmpty(effectEntry.Key))
                    continue;

                var effectTemplate = GameManager.Instance.EntityEffectBroker.GetEffectTemplate(effectEntry.Key);
                short permanentValue = GameManager.Instance.PlayerEntity.Skills.GetPermanentSkillValue((DFCareer.Skills)effectTemplate.Properties.MagicSkill);
                mods[(int)effectTemplate.Properties.MagicSkill] = 100 - permanentValue;
            }

            GameManager.Instance.PlayerEntity.Skills.AssignMods(mods);
            totalSpellPointCost3 = FormulaHelper.CalculateTotalEffectCosts(effectEntries, selectedTarget).spellPointCost;

            SetStatusLabels();
        }

        #endregion

        #region Button Events

        protected virtual void AddEffectButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            const int noMoreThan3Effects = 1707;

            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);

            // Must have a free effect slot
            if (GetFirstFreeEffectSlotIndex() == -1)
            {
                DaggerfallMessageBox mb = new DaggerfallMessageBox(
                    uiManager,
                    DaggerfallMessageBox.CommonMessageBoxButtons.Nothing,
                    DaggerfallUnity.Instance.TextProvider.GetRSCTokens(noMoreThan3Effects),
                    this);
                mb.ClickAnywhereToClose = true;
                mb.Show();
                return;
            }

            // Clear existing
            effectGroupPicker.ListBox.ClearItems();
            tipLabel.Text = string.Empty;

            // Populate group names
            string[] groupNames = GameManager.Instance.EntityEffectBroker.GetGroupNames(true, thisMagicStation);
            effectGroupPicker.ListBox.AddItems(groupNames);
            effectGroupPicker.ListBox.SelectedIndex = 0;

            // Show effect group picker
            uiManager.PushWindow(effectGroupPicker);
        }

        int TrailingZeroCount(int value)
        {
            int count = 0;
            while((value & 1) == 0)
            {
                value >>= 1;
                ++count;
            }
            return count;
        }

        protected virtual void BuyButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            const int youMustChooseAName = 1704;

            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);

            // Spell must have at least one effect - adding custom message
            List<EffectEntry> effects = GetEffectEntries();
            if (effects.Count == 0)
            {
                DaggerfallUI.MessageBox(TextManager.Instance.GetLocalizedText("noEffectsError"));
                return;
            }

            // Spell must have a name; Only bother the player if everything else is correct
            if (string.IsNullOrEmpty(spellNameLabel.Text))
            {
                DaggerfallUI.MessageBox(youMustChooseAName);
                return;
            }

            var fields = SpellData.AsDictionary;

            fields["spellName"] = new fsData(spellNameLabel.Text);
            fields["element"] = new fsData(TrailingZeroCount((int)selectedElement));
            fields["rangeType"] = new fsData(TrailingZeroCount((int)selectedTarget));
            fields["icon"] = new fsData(selectedIcon.index);

            var effectsData = fields["effects"] = fsData.CreateList();
            foreach (EffectEntry effectEntry in effects)
            {
                var effectData = fsData.CreateDictionary();
                var effectDataFields = effectData.AsDictionary;

                // This is how we get the "classic" type and subtype
                IEntityEffect effectInstance = GameManager.Instance.EntityEffectBroker.InstantiateEffect(effectEntry);
                var classicKey = effectInstance.Properties.ClassicKey;
                BaseEntityEffect.ReverseClasicKey(classicKey, out byte type, out byte subType, out BaseEntityEffect.ClassicEffectFamily family);

                int subTypeValue = subType != 255 ? subType : -1;

                effectDataFields["type"] = new fsData(type);
                effectDataFields["subType"] = new fsData(subTypeValue);

                if (effectInstance.Properties.SupportDuration)
                {
                    effectDataFields["durationBase"] = new fsData(effectEntry.Settings.DurationBase);
                    effectDataFields["durationMod"] = new fsData(effectEntry.Settings.DurationPlus);
                    effectDataFields["durationPerLevel"] = new fsData(effectEntry.Settings.DurationPerLevel);
                }

                if (effectInstance.Properties.SupportChance)
                {
                    effectDataFields["chanceBase"] = new fsData(effectEntry.Settings.ChanceBase);
                    effectDataFields["chanceMod"] = new fsData(effectEntry.Settings.ChancePlus);
                    effectDataFields["chancePerLevel"] = new fsData(effectEntry.Settings.ChancePerLevel);
                }

                if (effectInstance.Properties.SupportMagnitude)
                {
                    effectDataFields["magnitudeBaseLow"] = new fsData(effectEntry.Settings.MagnitudeBaseMin);
                    effectDataFields["magnitudeBaseHigh"] = new fsData(effectEntry.Settings.MagnitudeBaseMax);
                    effectDataFields["magnitudeLevelBase"] = new fsData(effectEntry.Settings.MagnitudePlusMin);
                    effectDataFields["magnitudeLevelHigh"] = new fsData(effectEntry.Settings.MagnitudePlusMax);
                    effectDataFields["magnitudePerLevel"] = new fsData(effectEntry.Settings.MagnitudePerLevel);
                }

                effectsData.AsList.Add(effectData);
            }

            OnSpellConfirmed?.Invoke(SpellData);

            DaggerfallUI.Instance.PlayOneShot(inscribeGrimoire);

            PopWindow();
        }

        protected virtual void ExitButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            CloseWindow();
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        protected virtual void CasterOnlyButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SetSpellTarget(TargetTypes.CasterOnly);
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        protected virtual void ByTouchButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SetSpellTarget(TargetTypes.ByTouch);
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        protected virtual void SingleTargetAtRangeButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SetSpellTarget(TargetTypes.SingleTargetAtRange);
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        protected virtual void AreaAroundCasterButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SetSpellTarget(TargetTypes.AreaAroundCaster);
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        protected virtual void AreaAtRangeButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SetSpellTarget(TargetTypes.AreaAtRange);
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        protected virtual void FireBasedButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SetSpellElement(ElementTypes.Fire);
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        protected virtual void ColdBasedButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SetSpellElement(ElementTypes.Cold);
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        protected virtual void PoisonBasedButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SetSpellElement(ElementTypes.Poison);
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        protected virtual void ShockBasedButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SetSpellElement(ElementTypes.Shock);
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        protected virtual void MagicBasedButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SetSpellElement(ElementTypes.Magic);
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void NextIconButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            int index = selectedIcon.index + 1;
            if (index >= DaggerfallUI.Instance.SpellIconCollection.GetIconCount(selectedIcon.key))
                index = 0;

            selectedIcon.index = index;
            SetIcon(selectedIcon);
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        protected virtual void SelectIconButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            uiManager.PushWindow(iconPicker);
        }

        protected virtual void PreviousIconButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            int index = selectedIcon.index - 1;
            if (index < 0)
                index = DaggerfallUI.Instance.SpellIconCollection.GetIconCount(selectedIcon.key) - 1;

            selectedIcon.index = index;
            SetIcon(selectedIcon);
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        protected virtual void NameSpellButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            DaggerfallInputMessageBox mb = new DaggerfallInputMessageBox(uiManager, this);
            mb.TextBox.Text = spellNameLabel.Text;
            mb.SetTextBoxLabel(TextManager.Instance.GetLocalizedText("enterSpellName") + " ");
            mb.OnGotUserInput += EnterName_OnGotUserInput;
            mb.Show();
        }

        protected virtual void AddEffectGroupListBox_OnUseSelectedItem()
        {
            // Clear existing
            effectSubGroupPicker.ListBox.ClearItems();
            enumeratedEffectTemplates.Clear();

            // Enumerate subgroup effect key name pairs
            enumeratedEffectTemplates = GameManager.Instance.EntityEffectBroker.GetEffectTemplates(effectGroupPicker.ListBox.SelectedItem, thisMagicStation);
            if (enumeratedEffectTemplates.Count < 1)
                throw new Exception(string.Format("Could not find any effect templates for group {0}", effectGroupPicker.ListBox.SelectedItem));

            // If this is a solo effect without any subgroups names defined (e.g. "Regenerate") then go straight to effect editor
            if (enumeratedEffectTemplates.Count == 1 && string.IsNullOrEmpty(enumeratedEffectTemplates[0].SubGroupName))
            {
                effectGroupPicker.CloseWindow();
                AddAndEditSlot(enumeratedEffectTemplates[0]);
                return;
            }

            // Sort list by subgroup name
            enumeratedEffectTemplates.Sort((s1, s2) => s1.SubGroupName.CompareTo(s2.SubGroupName));

            // Populate subgroup names in list box
            foreach (IEntityEffect effect in enumeratedEffectTemplates)
            {
                effectSubGroupPicker.ListBox.AddItem(effect.SubGroupName);
            }
            effectSubGroupPicker.ListBox.SelectedIndex = 0;

            // Show effect subgroup picker
            // Note: In classic the group name is now shown (and mostly obscured) behind the picker at first available effect slot
            // This is not easily visible and not sure if this really communicates anything useful to user
            // Daggerfall Unity also allows user to cancel via escape back to previous dialog, so changing this beheaviour intentionally
            uiManager.PushWindow(effectSubGroupPicker);
        }

        protected virtual void AddEffectSubGroup_OnUseSelectedItem()
        {
            // Close effect pickers
            effectGroupPicker.CloseWindow();
            effectSubGroupPicker.CloseWindow();

            // Get selected effect from those on offer
            IEntityEffect effectTemplate = enumeratedEffectTemplates[effectSubGroupPicker.ListBox.SelectedIndex];
            if (effectTemplate != null)
            {
                AddAndEditSlot(effectTemplate);
                //Debug.LogFormat("Selected effect {0} {1} with key {2}", effectTemplate.GroupName, effectTemplate.SubGroupName, effectTemplate.Key);
            }
        }

        protected virtual void EditOrDeleteSpell_OnCancel(DaggerfallPopupWindow sender)
        {
            editOrDeleteSlot = -1;
        }

        protected virtual void EditOrDeleteSpell_OnButtonClick(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton)
        {
            sender.CloseWindow();
        }

        protected virtual void DeleteButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            // Delete effect entry
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            ClearPendingDeleteEffectSlot();
            UpdateSpellCosts();
        }

        protected virtual void EditButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            // Edit effect entry
            effectEditor.EffectEntry = effectEntries[editOrDeleteSlot];
            uiManager.PushWindow(effectEditor);
        }

        #endregion

        #region Effect Editor Events

        protected virtual void EffectEditor_OnClose()
        {
            if (effectEditor.EntryValid)
            {
                EffectEntries[editOrDeleteSlot] = effectEditor.EffectEntry;
                UpdateSpellCosts();
                UpdateSlotText(editOrDeleteSlot, effectEditor.EffectTemplate.DisplayName);
            }

            editOrDeleteSlot = -1;
            UpdateAllowedButtons();
        }

        protected virtual void IconPicker_OnClose()
        {
            if (iconPicker.SelectedIcon != null)
                SetIcon(iconPicker.SelectedIcon.Value);
        }

        protected virtual void Effect1NamePanel_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            EditOrDeleteSlot(0);
        }

        protected virtual void Effect2NamePanel_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            EditOrDeleteSlot(1);
        }

        protected virtual void Effect3NamePanel_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            EditOrDeleteSlot(2);
        }

        protected virtual void EnterName_OnGotUserInput(DaggerfallInputMessageBox sender, string input)
        {
            spellNameLabel.Text = input;
        }

        #endregion

        #region Tip Events

        protected bool lockTip = false;
        protected virtual void TipButton_OnMouseEnter(BaseScreenComponent sender)
        {
            // Lock tip if already has text, this means we are changing directly to adjacent button
            // This prevents OnMouseLeave event from previous button wiping tip text of new button
            if (!string.IsNullOrEmpty(tipLabel.Text))
                lockTip = true;

            string tip = (string)sender.Tag;
            if (tip == "buySpell")
            {
                tipLabel.Text = "Confirm Changes";
            }
            else
            {
                tipLabel.Text = TextManager.Instance.GetLocalizedText(sender.Tag as string);
            }

            if (sender is Button)
            {
                Button buttonSender = (Button)sender;
                if (buttonSender.Hotkey != HotkeySequence.None)
                    tipLabel.Text += string.Format(" ({0})", buttonSender.Hotkey);
            }
        }

        protected virtual void TipButton_OnMouseLeave(BaseScreenComponent sender)
        {
            // Clear tip when not locked, otherwise reset tip lock
            if (!lockTip)
                tipLabel.Text = string.Empty;
            else
                lockTip = false;
        }

        #endregion
    }
}