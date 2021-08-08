using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Utility.AssetInjection;
using System;
using UnityEngine;
using static DaggerfallWorkshop.Game.UserInterfaceWindows.DaggerfallMessageBox;

namespace SpellEditorMod
{
    public class SpellEffectEditor : DaggerfallPopupWindow
    {
        #region UI Rects

        protected const int spinnerWidth = 24;
        protected const int spinnerHeight = 16;

        protected Rect spinnerUpButtonRect = new Rect(0, 0, spinnerWidth, 5);
        protected Rect spinnerDownButtonRect = new Rect(0, 11, spinnerWidth, 5);
        protected Rect spinnerValueLabelRect = new Rect(0, 5, spinnerWidth, 6);
        protected Rect durationBaseSpinnerRect = new Rect(64, 94, spinnerWidth, spinnerHeight);
        protected Rect durationPlusSpinnerRect = new Rect(104, 94, spinnerWidth, spinnerHeight);
        protected Rect durationPerLevelSpinnerRect = new Rect(160, 94, spinnerWidth, spinnerHeight);
        protected Rect chanceBaseSpinnerRect = new Rect(64, 114, spinnerWidth, spinnerHeight);
        protected Rect chancePlusSpinnerRect = new Rect(104, 114, spinnerWidth, spinnerHeight);
        protected Rect chancePerLevelSpinnerRect = new Rect(160, 114, spinnerWidth, spinnerHeight);
        protected Rect magnitudeBaseMinSpinnerRect = new Rect(64, 134, spinnerWidth, spinnerHeight);
        protected Rect magnitudeBaseMaxSpinnerRect = new Rect(104, 134, spinnerWidth, spinnerHeight);
        protected Rect magnitudePlusMinSpinnerRect = new Rect(64, 152, spinnerWidth, spinnerHeight);
        protected Rect magnitudePlusMaxSpinnerRect = new Rect(104, 152, spinnerWidth, spinnerHeight);
        protected Rect magnitudePerLevelSpinnerRect = new Rect(160, 152, spinnerWidth, spinnerHeight);

        protected Rect cancelButtonRect = new Rect(172, 170, 32, 16);
        protected Rect confirmButtonRect = new Rect(116, 170, 32, 16);

        #endregion

        #region UI Controls

        protected Panel descriptionPanel;
        protected MultiFormatTextLabel descriptionLabel;

        protected UpDownSpinner durationBaseSpinner;
        protected UpDownSpinner durationPlusSpinner;
        protected UpDownSpinner durationPerLevelSpinner;
        protected UpDownSpinner chanceBaseSpinner;
        protected UpDownSpinner chancePlusSpinner;
        protected UpDownSpinner chancePerLevelSpinner;
        protected UpDownSpinner magnitudeBaseMinSpinner;
        protected UpDownSpinner magnitudeBaseMaxSpinner;
        protected UpDownSpinner magnitudePlusMinSpinner;
        protected UpDownSpinner magnitudePlusMaxSpinner;
        protected UpDownSpinner magnitudePerLevelSpinner;

        protected TextLabel spellCost1Label;
        protected TextLabel spellCost2Label;
        protected TextLabel spellCost3Label;

        protected Button cancelButton;
        protected Button confirmButton;

        #endregion

        #region UI Textures

        protected Texture2D baseTexture;

        #endregion

        #region Fields

        protected const string baseTextureFilename = "SpellEffectEditorMask.png";
        protected const string noEffectTemplateError = "SpellEffectEditor does not have an EffectTemplate set.";

        protected Color hotButtonColor = new Color32(200, 200, 200, 100);

        protected IEntityEffect effectTemplate = null;
        protected EffectEntry? inputEffect = null;

        protected bool entryValid = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets IEntityEffect template for effect editor.
        /// </summary>
        public IEntityEffect EffectTemplate
        {
            get { return effectTemplate; }
            set { SetEffectTemplate(value); }
        }

        /// <summary>
        /// Gets or sets effect entry details after using settings window.
        /// Setting entry will also assign EffectTemplate based on entry key.
        /// </summary>
        public EffectEntry EffectEntry
        {
            get { return GetEffectEntry(); }
            set { SetEffectEntry(value); }
        }

        public bool EntryValid
        {
            get { return entryValid; }
        }

        #endregion

        #region Events

        public delegate void OnSettingsChangedHandler();
        public event OnSettingsChangedHandler OnSettingsChanged;

        protected void RaiseSettingsChanged()
        {
            OnSettingsChanged?.Invoke();
        }

        #endregion

        #region Constructors

        public SpellEffectEditor(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous = null)
            : base(uiManager, previous)
        {
        }

        #endregion

        #region Setup Methods

        protected override void Setup()
        {
            // Load all the textures used by effect editor window
            LoadTextures();

            // Setup native panel background
            NativePanel.BackgroundTexture = baseTexture;

            // Setup controls
            SetupEffectDescriptionPanels();
            SetupSpinners();
            SetupButtons();
            InitControlState();

            // Spell cost label
            spellCost1Label = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(275, 107), string.Empty, NativePanel);
            spellCost2Label = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(275, 137), string.Empty, NativePanel);
            spellCost3Label = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(275, 167), string.Empty, NativePanel);

            IsSetup = true;
            UpdateCosts();
        }

        public override void OnPush()
        {   
            if (IsSetup)
            {
                InitControlState();
                UpdateCosts();
            }
        }

        public override void Update()
        {
            base.Update();

            if (DaggerfallUI.Instance.HotkeySequenceProcessed == HotkeySequence.HotkeySequenceProcessStatus.NotFound
                && Input.GetKeyUp(KeyCode.Return))
            {
                OnConfirm();
            }
        }

        public override void CancelWindow()
        {
            entryValid = false;
            base.CancelWindow();
        }

        #endregion

        #region Protected Methods
        protected virtual void LoadTextures()
        {
            // Load source textures
            TextureReplacement.TryImportImage(baseTextureFilename, true, out baseTexture);
        }

        protected virtual void SetupEffectDescriptionPanels()
        {
            // Create parent panel to house effect description
            Panel descriptionParentPanel = DaggerfallUI.AddPanel(new Rect(5, 19, 312, 69), NativePanel);
            descriptionParentPanel.HorizontalAlignment = HorizontalAlignment.Center;

            // Create description panel centred inside of parent
            descriptionPanel = DaggerfallUI.AddPanel(descriptionParentPanel, AutoSizeModes.None);
            descriptionPanel.Size = new Vector2(306, 69);
            descriptionPanel.HorizontalAlignment = HorizontalAlignment.Center;
            descriptionPanel.VerticalAlignment = VerticalAlignment.Middle;
            DaggerfallUI.Instance.SetDaggerfallPopupStyle(DaggerfallUI.PopupStyle.Parchment, descriptionPanel);

            // Create multiline label for description text
            descriptionLabel = new MultiFormatTextLabel();
            descriptionLabel.HorizontalAlignment = HorizontalAlignment.Center;
            descriptionLabel.VerticalAlignment = VerticalAlignment.Middle;
            descriptionPanel.Components.Add(descriptionLabel);
        }

        protected virtual void SetupSpinners()
        {
            // Add spinner controls
            durationBaseSpinner = new UpDownSpinner(durationBaseSpinnerRect, spinnerUpButtonRect, spinnerDownButtonRect, spinnerValueLabelRect, 0, null, NativePanel);
            durationPlusSpinner = new UpDownSpinner(durationPlusSpinnerRect, spinnerUpButtonRect, spinnerDownButtonRect, spinnerValueLabelRect, 0, null, NativePanel);
            durationPerLevelSpinner = new UpDownSpinner(durationPerLevelSpinnerRect, spinnerUpButtonRect, spinnerDownButtonRect, spinnerValueLabelRect, 0, null, NativePanel);
            chanceBaseSpinner = new UpDownSpinner(chanceBaseSpinnerRect, spinnerUpButtonRect, spinnerDownButtonRect, spinnerValueLabelRect, 0, null, NativePanel);
            chancePlusSpinner = new UpDownSpinner(chancePlusSpinnerRect, spinnerUpButtonRect, spinnerDownButtonRect, spinnerValueLabelRect, 0, null, NativePanel);
            chancePerLevelSpinner = new UpDownSpinner(chancePerLevelSpinnerRect, spinnerUpButtonRect, spinnerDownButtonRect, spinnerValueLabelRect, 0, null, NativePanel);
            magnitudeBaseMinSpinner = new UpDownSpinner(magnitudeBaseMinSpinnerRect, spinnerUpButtonRect, spinnerDownButtonRect, spinnerValueLabelRect, 0, null, NativePanel);
            magnitudeBaseMaxSpinner = new UpDownSpinner(magnitudeBaseMaxSpinnerRect, spinnerUpButtonRect, spinnerDownButtonRect, spinnerValueLabelRect, 0, null, NativePanel);
            magnitudePlusMinSpinner = new UpDownSpinner(magnitudePlusMinSpinnerRect, spinnerUpButtonRect, spinnerDownButtonRect, spinnerValueLabelRect, 0, null, NativePanel);
            magnitudePlusMaxSpinner = new UpDownSpinner(magnitudePlusMaxSpinnerRect, spinnerUpButtonRect, spinnerDownButtonRect, spinnerValueLabelRect, 0, null, NativePanel);
            magnitudePerLevelSpinner = new UpDownSpinner(magnitudePerLevelSpinnerRect, spinnerUpButtonRect, spinnerDownButtonRect, spinnerValueLabelRect, 0, null, NativePanel);

            // Set spinner mouse over colours
            durationBaseSpinner.SetMouseOverBackgroundColor(hotButtonColor);
            durationPlusSpinner.SetMouseOverBackgroundColor(hotButtonColor);
            durationPerLevelSpinner.SetMouseOverBackgroundColor(hotButtonColor);
            chanceBaseSpinner.SetMouseOverBackgroundColor(hotButtonColor);
            chancePlusSpinner.SetMouseOverBackgroundColor(hotButtonColor);
            chancePerLevelSpinner.SetMouseOverBackgroundColor(hotButtonColor);
            magnitudeBaseMinSpinner.SetMouseOverBackgroundColor(hotButtonColor);
            magnitudeBaseMaxSpinner.SetMouseOverBackgroundColor(hotButtonColor);
            magnitudePlusMinSpinner.SetMouseOverBackgroundColor(hotButtonColor);
            magnitudePlusMaxSpinner.SetMouseOverBackgroundColor(hotButtonColor);
            magnitudePerLevelSpinner.SetMouseOverBackgroundColor(hotButtonColor);

            // Set spinner ranges
            durationBaseSpinner.SetRange(0, 999);
            durationPlusSpinner.SetRange(0, 999);
            durationPerLevelSpinner.SetRange(1, 99);
            chanceBaseSpinner.SetRange(0, 999);
            chancePlusSpinner.SetRange(0, 999);
            chancePerLevelSpinner.SetRange(1, 99);
            magnitudeBaseMinSpinner.SetRange(0, 999);
            magnitudeBaseMaxSpinner.SetRange(0, 999);
            magnitudePlusMinSpinner.SetRange(0, 999);
            magnitudePlusMaxSpinner.SetRange(0, 999);
            magnitudePerLevelSpinner.SetRange(1, 99);

            // Set spinner events
            durationBaseSpinner.OnValueChanged += DurationBaseSpinner_OnValueChanged;
            durationPlusSpinner.OnValueChanged += DurationPlusSpinner_OnValueChanged;
            durationPerLevelSpinner.OnValueChanged += DurationPerLevelSpinner_OnValueChanged;
            chanceBaseSpinner.OnValueChanged += ChanceBaseSpinner_OnValueChanged;
            chancePlusSpinner.OnValueChanged += ChancePlusSpinner_OnValueChanged;
            chancePerLevelSpinner.OnValueChanged += ChancePerLevelSpinner_OnValueChanged;
            magnitudeBaseMinSpinner.OnValueChanged += MagnitudeBaseMinSpinner_OnValueChanged;
            magnitudeBaseMaxSpinner.OnValueChanged += MagnitudeBaseMaxSpinner_OnValueChanged;
            magnitudePlusMinSpinner.OnValueChanged += MagnitudePlusMinSpinner_OnValueChanged;
            magnitudePlusMaxSpinner.OnValueChanged += MagnitudePlusMaxSpinner_OnValueChanged;
            magnitudePerLevelSpinner.OnValueChanged += MagnitudePerLevelSpinner_OnValueChanged;
        }

        protected virtual void SetupButtons()
        {
            // Exit button
            cancelButton = DaggerfallUI.AddButton(cancelButtonRect, NativePanel);
            cancelButton.OnMouseClick += CancelButton_OnMouseClick;

            confirmButton = DaggerfallUI.AddButton(confirmButtonRect, NativePanel);
            confirmButton.OnMouseClick += OkayButton_OnMouseClick;
        }

        protected virtual void InitControlState()
        {
            // Must have an effect template set
            if (EffectTemplate == null)
                return;

            // Get description text
            TextFile.Token[] descriptionTokens = EffectTemplate.SpellMakerDescription;
            if (descriptionTokens == null || descriptionTokens.Length == 0)
                throw new Exception(string.Format("DaggerfallEffectSettingsEditorWindow: EffectTemplate {0} does not present any spellmaker description text.", EffectTemplate.Key));
            else
                descriptionLabel.SetText(descriptionTokens);

            // Duration support
            if (EffectTemplate.Properties.SupportDuration)
            {
                durationBaseSpinner.Enabled = true;
                durationPlusSpinner.Enabled = true;
                durationPerLevelSpinner.Enabled = true;
            }
            else
            {
                durationBaseSpinner.Enabled = false;
                durationPlusSpinner.Enabled = false;
                durationPerLevelSpinner.Enabled = false;
            }

            // Chance support
            if (EffectTemplate.Properties.SupportChance)
            {
                chanceBaseSpinner.Enabled = true;
                chancePlusSpinner.Enabled = true;
                chancePerLevelSpinner.Enabled = true;
            }
            else
            {
                chanceBaseSpinner.Enabled = false;
                chancePlusSpinner.Enabled = false;
                chancePerLevelSpinner.Enabled = false;
            }

            // Magnitude support
            if (EffectTemplate.Properties.SupportMagnitude)
            {
                magnitudeBaseMinSpinner.Enabled = true;
                magnitudeBaseMaxSpinner.Enabled = true;
                magnitudePlusMinSpinner.Enabled = true;
                magnitudePlusMaxSpinner.Enabled = true;
                magnitudePerLevelSpinner.Enabled = true;
            }
            else
            {
                magnitudeBaseMinSpinner.Enabled = false;
                magnitudeBaseMaxSpinner.Enabled = false;
                magnitudePlusMinSpinner.Enabled = false;
                magnitudePlusMaxSpinner.Enabled = false;
                magnitudePerLevelSpinner.Enabled = false;
            }

            if (inputEffect != null)
            {
                var entry = inputEffect.Value;

                // Assign settings to spinners
                durationBaseSpinner.Value = entry.Settings.DurationBase;
                durationPlusSpinner.Value = entry.Settings.DurationPlus;
                durationPerLevelSpinner.Value = entry.Settings.DurationPerLevel;
                chanceBaseSpinner.Value = entry.Settings.ChanceBase;
                chancePlusSpinner.Value = entry.Settings.ChancePlus;
                chancePerLevelSpinner.Value = entry.Settings.ChancePerLevel;
                magnitudeBaseMinSpinner.Value = entry.Settings.MagnitudeBaseMin;
                magnitudeBaseMaxSpinner.Value = entry.Settings.MagnitudeBaseMax;
                magnitudePlusMinSpinner.Value = entry.Settings.MagnitudePlusMin;
                magnitudePlusMaxSpinner.Value = entry.Settings.MagnitudePlusMax;
                magnitudePerLevelSpinner.Value = entry.Settings.MagnitudePerLevel;
            }
        }

        protected virtual EffectEntry GetEffectEntry()
        {
            // Must have an effect template set
            if (EffectTemplate == null)
                throw new Exception(noEffectTemplateError);

            // Create settings for effect
            EffectSettings effectSettings = new EffectSettings();
            if (IsSetup)
            {
                // Assign from UI control when setup
                effectSettings.DurationBase = durationBaseSpinner.Value;
                effectSettings.DurationPlus = durationPlusSpinner.Value;
                effectSettings.DurationPerLevel = durationPerLevelSpinner.Value;
                effectSettings.ChanceBase = chanceBaseSpinner.Value;
                effectSettings.ChancePlus = chancePlusSpinner.Value;
                effectSettings.ChancePerLevel = chancePerLevelSpinner.Value;
                effectSettings.MagnitudeBaseMin = magnitudeBaseMinSpinner.Value;
                effectSettings.MagnitudeBaseMax = magnitudeBaseMaxSpinner.Value;
                effectSettings.MagnitudePlusMin = magnitudePlusMinSpinner.Value;
                effectSettings.MagnitudePlusMax = magnitudePlusMaxSpinner.Value;
                effectSettings.MagnitudePerLevel = magnitudePerLevelSpinner.Value;
            }

            // Create entry
            EffectEntry effectEntry = new EffectEntry();
            effectEntry.Key = EffectTemplate.Key;
            effectEntry.Settings = effectSettings;

            return effectEntry;
        }

        protected virtual void SetEffectEntry(EffectEntry entry)
        {
            // Assign effect template based on entry key
            EffectTemplate = GameManager.Instance.EntityEffectBroker.GetEffectTemplate(entry.Key);
            if (EffectTemplate == null)
                throw new Exception(string.Format("SetEffectEntry() could not find effect key {0}", entry.Key));

            inputEffect = entry;

            if (!IsSetup)
                return;

            // Assign settings to spinners
            durationBaseSpinner.Value = entry.Settings.DurationBase;
            durationPlusSpinner.Value = entry.Settings.DurationPlus;
            durationPerLevelSpinner.Value = entry.Settings.DurationPerLevel;
            chanceBaseSpinner.Value = entry.Settings.ChanceBase;
            chancePlusSpinner.Value = entry.Settings.ChancePlus;
            chancePerLevelSpinner.Value = entry.Settings.ChancePerLevel;
            magnitudeBaseMinSpinner.Value = entry.Settings.MagnitudeBaseMin;
            magnitudeBaseMaxSpinner.Value = entry.Settings.MagnitudeBaseMax;
            magnitudePlusMinSpinner.Value = entry.Settings.MagnitudePlusMin;
            magnitudePlusMaxSpinner.Value = entry.Settings.MagnitudePlusMax;
            magnitudePerLevelSpinner.Value = entry.Settings.MagnitudePerLevel;
        }

        protected virtual void UpdateCosts()
        {
            if (!IsSetup)
                return;

            if (OnSettingsChanged != null)
                OnSettingsChanged();

            short permanentValue = GameManager.Instance.PlayerEntity.Skills.GetPermanentSkillValue((DFCareer.Skills)EffectTemplate.Properties.MagicSkill);

            int[] mods = new int[(int)DFCareer.Skills.Count];

            // Spell cost 1
            mods[(int)EffectTemplate.Properties.MagicSkill] = 5 - permanentValue;
            GameManager.Instance.PlayerEntity.Skills.AssignMods(mods);

            (int _, int spellPointCost1) = FormulaHelper.CalculateEffectCosts(EffectEntry);
            spellCost1Label.Text = spellPointCost1.ToString();

            // Spell cost 2
            mods[(int)EffectTemplate.Properties.MagicSkill] = 50 - permanentValue;
            GameManager.Instance.PlayerEntity.Skills.AssignMods(mods);

            (int _, int spellPointCost2) = FormulaHelper.CalculateEffectCosts(EffectEntry);
            spellCost2Label.Text = spellPointCost2.ToString();

            // Spell cost 3
            mods[(int)EffectTemplate.Properties.MagicSkill] = 100 - permanentValue;
            GameManager.Instance.PlayerEntity.Skills.AssignMods(mods);

            (int _, int spellPointCost3) = FormulaHelper.CalculateEffectCosts(EffectEntry);
            spellCost3Label.Text = spellPointCost3.ToString();
        }

        protected virtual void SetSpinners(EffectSettings settings)
        {
            if (!IsSetup)
                return;

            durationBaseSpinner.Value = settings.DurationBase;
            durationPlusSpinner.Value = settings.DurationPlus;
            durationPerLevelSpinner.Value = settings.DurationPerLevel;
            chanceBaseSpinner.Value = settings.ChanceBase;
            chancePlusSpinner.Value = settings.ChancePlus;
            chancePerLevelSpinner.Value = settings.ChancePerLevel;
            magnitudeBaseMinSpinner.Value = settings.MagnitudeBaseMin;
            magnitudeBaseMaxSpinner.Value = settings.MagnitudeBaseMax;
            magnitudePlusMinSpinner.Value = settings.MagnitudePlusMin;
            magnitudePlusMaxSpinner.Value = settings.MagnitudePlusMax;
            magnitudePerLevelSpinner.Value = settings.MagnitudePerLevel;
        }

        protected virtual void SetEffectTemplate(IEntityEffect effectTemplate)
        {
            this.effectTemplate = effectTemplate;
            inputEffect = null;
            SetSpinners(new EffectSettings());
        }

        #endregion

        #region Event Handlers

        protected void DurationBaseSpinner_OnValueChanged()
        {
            UpdateCosts();
        }

        protected void DurationPlusSpinner_OnValueChanged()
        {
            UpdateCosts();
        }

        protected void DurationPerLevelSpinner_OnValueChanged()
        {
            UpdateCosts();
        }

        protected void ChanceBaseSpinner_OnValueChanged()
        {
            UpdateCosts();
        }

        protected void ChancePlusSpinner_OnValueChanged()
        {
            UpdateCosts();
        }

        protected void ChancePerLevelSpinner_OnValueChanged()
        {
            UpdateCosts();
        }

        protected void MagnitudeBaseMinSpinner_OnValueChanged()
        {
            if (magnitudeBaseMinSpinner.Value > magnitudeBaseMaxSpinner.Value)
                magnitudeBaseMaxSpinner.Value = magnitudeBaseMinSpinner.Value;

            UpdateCosts();
        }

        protected void MagnitudeBaseMaxSpinner_OnValueChanged()
        {
            if (magnitudeBaseMaxSpinner.Value < magnitudeBaseMinSpinner.Value)
                magnitudeBaseMinSpinner.Value = magnitudeBaseMaxSpinner.Value;

            UpdateCosts();
        }

        protected void MagnitudePlusMinSpinner_OnValueChanged()
        {
            if (magnitudePlusMinSpinner.Value > magnitudePlusMaxSpinner.Value)
                magnitudePlusMaxSpinner.Value = magnitudePlusMinSpinner.Value;

            UpdateCosts();
        }

        protected void MagnitudePlusMaxSpinner_OnValueChanged()
        {
            if (magnitudePlusMaxSpinner.Value < magnitudePlusMinSpinner.Value)
                magnitudePlusMinSpinner.Value = magnitudePlusMaxSpinner.Value;

            UpdateCosts();
        }

        protected void MagnitudePerLevelSpinner_OnValueChanged()
        {
            UpdateCosts();
        }

        private bool AreEffectAttributesValid()
        {
            if (EffectTemplate.Properties.SupportDuration
                && durationBaseSpinner.Value + durationPlusSpinner.Value == 0)
                return false;

            if (EffectTemplate.Properties.SupportChance
                && chanceBaseSpinner.Value + chancePlusSpinner.Value == 0)
                return false;

            if (EffectTemplate.Properties.SupportMagnitude
                && magnitudeBaseMinSpinner.Value + magnitudeBaseMaxSpinner.Value + magnitudePlusMinSpinner.Value + magnitudePlusMaxSpinner.Value == 0)
                return false;

            return true;
        }

        void OnConfirm()
        {
            if (!AreEffectAttributesValid())
            {
                DaggerfallMessageBox mb = new DaggerfallMessageBox(uiManager, this);
                mb.SetTextTokens(DaggerfallUnity.TextProvider.CreateTokens(TextFile.Formatting.JustifyLeft, "Effect values are invalid. Do you want to discard?"));
                mb.AddButton(MessageBoxButtons.Yes);
                mb.AddButton(MessageBoxButtons.No);
                mb.OnButtonClick += DiscardWindow_OnButtonClick;
                mb.Show();
            }
            else
            {
                entryValid = true;
                CloseWindow();
            }
        }

        protected void CancelButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            entryValid = false;
            CloseWindow();
        }

        void DiscardWindow_OnButtonClick(DaggerfallMessageBox sender, MessageBoxButtons messageBoxButton)
        {
            sender.CloseWindow();

            if (messageBoxButton == MessageBoxButtons.Yes)
            {
                entryValid = false;
                CloseWindow();
            }
        }

        protected void OkayButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            OnConfirm();
        }

        #endregion
    }
}