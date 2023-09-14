using BepInEx;
using BepInEx.Configuration;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using SpecialWeapons;
using System;
using UnityEngine;

namespace ValheimModding
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class SpecialWeapons : BaseUnityPlugin
    {
        public const string PluginGUID = "DeathWizsh.SpecialWeapons";
        public const string PluginName = "Special Weapons";
        public const string PluginVersion = "1.0.0";
        
        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private AssetBundle specialWeaponsBundle;
        private GameObject demoHammerHammerPrefab;
        private GameObject demoHammerAtgeirPrefab;
        private GameObject cultivatorAtgeirAtgeirPrefab;
        private GameObject cultivatorAtgeirBattleAxePrefab;
        private GameObject triSwordLightningPrefab;
        private GameObject triSwordFirePrefab;
        private GameObject triSwordFrostPrefab;

        private Sprite demolisherSprite;
        private Sprite himminAflSprite;
        private Sprite lightningSprite;
        private Sprite swordFireSprite;
        private Sprite frostSprite;

        private string sectionGeneral = "1. General";
        private ConfigEntry<bool> configEnable;
        private ConfigEntry<KeyboardShortcut> configWeaponModeKey;

        private string sectionDemoHammer = "2. Demolition Hammer";
        private ConfigEntry<bool> configDemoHammerEnable;
        private ConfigEntry<string> configDemoHammerName;
        private ConfigEntry<string> configDemoHammerDescription;
        private ConfigEntry<string> configDemoHammerCraftingStation;
        private ConfigEntry<int> configDemoHammerMinStationLevel;
        private ConfigEntry<string> configDemoHammerRecipe;
        private ConfigEntry<string> configDemoHammerRecipeUpgrade;
        private ConfigEntry<int> configDemoHammerRecipeMultiplier;
        private ConfigEntry<int> configDemoHammerMaxQuality;
        private ConfigEntry<float> configDemoHammerMovementSpeed;
        private ConfigEntry<float> configDemoHammerDamageMultiplier;
        private ConfigEntry<int> configDemoHammerBlockArmor;
        private ConfigEntry<int> configDemoHammerBlockForce;
        private ConfigEntry<int> configDemoHammerKnockBack;
        private ConfigEntry<int> configDemoHammerBackStab;
        private ConfigEntry<int> configDemoHammerUseStamina;
        private ConfigEntry<int> configDemoHammerUseStaminaHammer;
        private ConfigEntry<int> configDemoHammerUseStaminaAtgeir;

        private string sectionTriSword = "3. Tri Sword";
        private ConfigEntry<bool> configTriSwordEnable;
        private ConfigEntry<string> configTriSwordName;
        private ConfigEntry<string> configTriSwordDescription;
        private ConfigEntry<string> configTriSwordCraftingStation;
        private ConfigEntry<int> configTriSwordMinStationLevel;
        private ConfigEntry<string> configTriSwordRecipe;
        private ConfigEntry<string> configTriSwordRecipeUpgrade;
        private ConfigEntry<int> configTriSwordRecipeMultiplier;
        private ConfigEntry<int> configTriSwordMaxQuality;

        private string sectionCultivatorAtgeir = "4. Cultivator Atgeir";
        private ConfigEntry<bool> configCultivatorAtgeirEnable;
        private ConfigEntry<string> configCultivatorAtgeirName;
        private ConfigEntry<string> configCultivatorAtgeirDescription;
        private ConfigEntry<string> configCultivatorAtgeirCraftingStation;
        private ConfigEntry<int> configCultivatorAtgeirMinStationLevel;
        private ConfigEntry<string> configCultivatorAtgeirRecipe;
        private ConfigEntry<string> configCultivatorAtgeirRecipeUpgrade;
        private ConfigEntry<int> configCultivatorAtgeirRecipeMultiplier;
        private ConfigEntry<int> configCultivatorAtgeirMaxQuality;
        private ConfigEntry<float> configCultivatorAtgeirMovementSpeed;
        private ConfigEntry<float> configCultivatorAtgeirDamageMultiplier;
        private ConfigEntry<int> configCultivatorAtgeirBlockArmor;
        private ConfigEntry<int> configCultivatorAtgeirBlockForce;
        private ConfigEntry<int> configCultivatorAtgeirKnockBack;
        private ConfigEntry<int> configCultivatorAtgeirBackStab;
        private ConfigEntry<int> configCultivatorAtgeirUseStamina;
        private ConfigEntry<int> configCultivatorAtgeirUseStaminaAtgeir;
        private ConfigEntry<int> configCultivatorAtgeirUseStaminaPoke;

        private ButtonConfig weaponModeButton;

        private CustomStatusEffect demoHammerHammerStatusEffect;
        private CustomStatusEffect demoHammerAtgeirStatusEffect;
        private CustomStatusEffect triSwordLightningStatusEffect;
        private CustomStatusEffect triSwordFireStatusEffect;
        private CustomStatusEffect triSwordFrostStatusEffect;

        private void Awake()
        {
            InitConfig();

            //if (!configEnable.Value) return;
            //if (!configDemoHammerEnable.Value && !configTriSwordEnable.Value && !configCultivatorAtgeirEnable.Value)
            //{
            //    Jotunn.Logger.LogWarning("Mod enabled but all the weapons are not, skipping initialisation...");
            //    return;
            //}

            InitAssetBundle();
            InitInputs();
            InitStatusEffects();

            if (configDemoHammerEnable.Value)
                PrefabManager.OnVanillaPrefabsAvailable += AddDemoHammer;

            if (configTriSwordEnable.Value)
                PrefabManager.OnVanillaPrefabsAvailable += AddTriSword;

            if (configCultivatorAtgeirEnable.Value)
                PrefabManager.OnVanillaPrefabsAvailable += AddCultivatorAtgeir;
        }

        private void Update()
        {
            // Since our Update function in our BepInEx mod class will load BEFORE Valheim loads,
            // we need to check that ZInput is ready to use first.
            if (ZInput.instance != null)
            {
                // KeyboardShortcuts are also injected into the ZInput system
                if (weaponModeButton != null && MessageHud.instance != null)
                {
                    if (ZInput.GetButtonDown(weaponModeButton.Name) && MessageHud.instance.m_msgQeue.Count == 0)
                    {
                        if (Player.m_localPlayer)
                        {
                            ItemDrop.ItemData weapon = Player.m_localPlayer.GetCurrentWeapon();
                            if (weapon != null && weapon.m_shared.m_name == configDemoHammerName.Value)
                            {
                                if (weapon.m_shared.m_attack.m_drawDurationMin == 0)
                                {
                                    ItemDrop itemDrop = demoHammerAtgeirPrefab.GetComponent<ItemDrop>();
                                    weapon.m_dropPrefab = demoHammerAtgeirPrefab;
                                    weapon.m_shared = itemDrop.m_itemData.m_shared;
                                    weapon.m_shared.m_attack.m_drawDurationMin = 1;
                                }
                                else
                                {
                                    ItemDrop itemDrop = demoHammerHammerPrefab.GetComponent<ItemDrop>();
                                    weapon.m_dropPrefab = demoHammerHammerPrefab;
                                    weapon.m_shared = itemDrop.m_itemData.m_shared;
                                    weapon.m_shared.m_attack.m_drawDurationMin = 0;
                                }


                                Player.m_localPlayer.UnequipItem(weapon);
                                Player.m_localPlayer.EquipItem(weapon);
                                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Changed mode to " + (weapon.m_shared.m_attack.m_drawDurationMin == 0 ? "Hammer" : "Atgeir"));
                            }

                            else if (weapon != null && weapon.m_shared.m_name == configCultivatorAtgeirName.Value)
                            {
                                if (weapon.m_shared.m_attack.m_drawDurationMin == 0)
                                {
                                    ItemDrop itemDrop = cultivatorAtgeirBattleAxePrefab.GetComponent<ItemDrop>();
                                    weapon.m_shared = itemDrop.m_itemData.m_shared;
                                    weapon.m_shared.m_attack.m_drawDurationMin = 1;
                                }
                                else
                                {
                                    ItemDrop itemDrop = cultivatorAtgeirAtgeirPrefab.GetComponent<ItemDrop>();
                                    weapon.m_shared = itemDrop.m_itemData.m_shared;
                                    weapon.m_shared.m_attack.m_drawDurationMin = 0;
                                }

                                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Changed mode to " + (weapon.m_shared.m_attack.m_drawDurationMin == 0 ? "Atgeir" : "Poke"));
                            }

                            else if (weapon != null && weapon.m_shared.m_name == configTriSwordName.Value)
                            {
                                if (weapon.m_shared.m_attack.m_drawDurationMin == 0)
                                {
                                    ItemDrop itemDrop = triSwordFirePrefab.GetComponent<ItemDrop>();
                                    weapon.m_dropPrefab = triSwordFirePrefab;
                                    weapon.m_shared = itemDrop.m_itemData.m_shared;
                                    weapon.m_shared.m_attack.m_drawDurationMin = 1;
                                }
                                else if (weapon.m_shared.m_attack.m_drawDurationMin == 1)
                                {
                                    ItemDrop itemDrop = triSwordFrostPrefab.GetComponent<ItemDrop>();
                                    weapon.m_dropPrefab = triSwordFrostPrefab;
                                    weapon.m_shared = itemDrop.m_itemData.m_shared;
                                    weapon.m_shared.m_attack.m_drawDurationMin = 2;
                                }
                                else
                                {
                                    ItemDrop itemDrop = triSwordLightningPrefab.GetComponent<ItemDrop>();
                                    weapon.m_dropPrefab = triSwordLightningPrefab;
                                    weapon.m_shared = itemDrop.m_itemData.m_shared;
                                    weapon.m_shared.m_attack.m_drawDurationMin = 0;
                                }

                                Player.m_localPlayer.UnequipItem(weapon);
                                Player.m_localPlayer.EquipItem(weapon);
                                // Player.m_localPlayer.StartEmote("Cheer");
                                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Changed mode to " + (weapon.m_shared.m_attack.m_drawDurationMin == 0 ? "Lightning" : weapon.m_shared.m_attack.m_drawDurationMin == 1 ? "Fire" : "Frost"));
                            }
                        }
                    }
                }
            }
        }

        private void AddDemoHammer()
        {
            try
            {
                ConfigRecipe recipe = new ConfigRecipe(configDemoHammerRecipe.Value, configDemoHammerRecipeUpgrade.Value);
                ItemConfig itemConfig = new ItemConfig();
                itemConfig.Name = configDemoHammerName.Value;
                itemConfig.Description = configDemoHammerDescription.Value;
                itemConfig.CraftingStation = configDemoHammerCraftingStation.Value;
                itemConfig.MinStationLevel = configDemoHammerMinStationLevel.Value;

                foreach (var requirement in recipe.requirements)
                {
                    int multiplier = configDemoHammerRecipeMultiplier.Value != 0 ? configDemoHammerRecipeMultiplier.Value : 1;
                    int amountPerlevel = requirement.amountPerLevel * multiplier;
                    itemConfig.AddRequirement(new RequirementConfig(requirement.material, requirement.amount, amountPerlevel, true));
                }

                ItemDrop itemDropHammer = demoHammerHammerPrefab.GetComponent<ItemDrop>();
                itemDropHammer.m_itemData.m_shared.m_maxQuality = configDemoHammerMaxQuality.Value;
                itemDropHammer.m_itemData.m_shared.m_equipStatusEffect = demoHammerHammerStatusEffect.StatusEffect;
                itemDropHammer.m_itemData.m_shared.m_movementModifier = configDemoHammerMovementSpeed.Value;
                itemDropHammer.m_itemData.m_shared.m_damages.m_blunt = itemDropHammer.m_itemData.m_shared.m_damages.m_blunt * configDemoHammerDamageMultiplier.Value;
                itemDropHammer.m_itemData.m_shared.m_damages.m_lightning = itemDropHammer.m_itemData.m_shared.m_damages.m_lightning * configDemoHammerDamageMultiplier.Value;
                itemDropHammer.m_itemData.m_shared.m_blockPower = configDemoHammerBlockArmor.Value;
                itemDropHammer.m_itemData.m_shared.m_deflectionForce = configDemoHammerBlockForce.Value;
                itemDropHammer.m_itemData.m_shared.m_attackForce = configDemoHammerKnockBack.Value;
                itemDropHammer.m_itemData.m_shared.m_backstabBonus = configDemoHammerBackStab.Value;
                itemDropHammer.m_itemData.m_shared.m_attack.m_attackStamina = configDemoHammerUseStamina.Value;
                itemDropHammer.m_itemData.m_shared.m_secondaryAttack.m_attackStamina = configDemoHammerUseStaminaHammer.Value;

                ItemDrop itemDropAtgeir = demoHammerAtgeirPrefab.GetComponent<ItemDrop>();
                itemDropAtgeir.m_itemData.m_shared.m_name = configDemoHammerName.Value;
                itemDropAtgeir.m_itemData.m_shared.m_description = configDemoHammerDescription.Value;
                itemDropAtgeir.m_itemData.m_shared.m_maxQuality = configDemoHammerMaxQuality.Value;
                itemDropAtgeir.m_itemData.m_shared.m_equipStatusEffect = demoHammerAtgeirStatusEffect.StatusEffect;
                itemDropAtgeir.m_itemData.m_shared.m_movementModifier = configDemoHammerMovementSpeed.Value;
                itemDropAtgeir.m_itemData.m_shared.m_damages.m_blunt = itemDropAtgeir.m_itemData.m_shared.m_damages.m_blunt * configDemoHammerDamageMultiplier.Value;
                itemDropAtgeir.m_itemData.m_shared.m_damages.m_lightning = itemDropAtgeir.m_itemData.m_shared.m_damages.m_lightning * configDemoHammerDamageMultiplier.Value;
                itemDropAtgeir.m_itemData.m_shared.m_blockPower = configDemoHammerBlockArmor.Value;
                itemDropAtgeir.m_itemData.m_shared.m_deflectionForce = configDemoHammerBlockForce.Value;
                itemDropAtgeir.m_itemData.m_shared.m_attackForce = configDemoHammerKnockBack.Value;
                itemDropAtgeir.m_itemData.m_shared.m_backstabBonus = configDemoHammerBackStab.Value;
                itemDropAtgeir.m_itemData.m_shared.m_attack.m_attackStamina = configDemoHammerUseStamina.Value;
                itemDropAtgeir.m_itemData.m_shared.m_secondaryAttack.m_attackStamina = configDemoHammerUseStaminaAtgeir.Value;

                ItemManager.Instance.AddItem(new CustomItem(demoHammerHammerPrefab, true, itemConfig));
                ItemManager.Instance.AddItem(new CustomItem(demoHammerAtgeirPrefab, true, new ItemConfig()));
                PrefabManager.OnVanillaPrefabsAvailable -= AddDemoHammer;
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise the Demolition Hammer: " + error);
            }
        }

        private void AddTriSword()
        {
            try
            {
                ConfigRecipe recipe = new ConfigRecipe(configTriSwordRecipe.Value, configTriSwordRecipeUpgrade.Value);
                ItemConfig itemConfig = new ItemConfig();
                itemConfig.Name = configTriSwordName.Value;
                itemConfig.Description = configTriSwordDescription.Value;
                itemConfig.CraftingStation = configTriSwordCraftingStation.Value;
                itemConfig.MinStationLevel = configTriSwordMinStationLevel.Value;

                foreach (var requirement in recipe.requirements)
                {
                    int multiplier = configTriSwordRecipeMultiplier.Value != 0 ? configTriSwordRecipeMultiplier.Value : 1;
                    int amountPerlevel = requirement.amountPerLevel * multiplier;
                    itemConfig.AddRequirement(new RequirementConfig(requirement.material, requirement.amount, amountPerlevel, true));
                }

                ItemDrop itemDropLightning = triSwordLightningPrefab.GetComponent<ItemDrop>();
                itemDropLightning.m_itemData.m_shared.m_maxQuality = configTriSwordMaxQuality.Value;
                itemDropLightning.m_itemData.m_shared.m_equipStatusEffect = triSwordLightningStatusEffect.StatusEffect;
                ItemDrop itemDropFire = triSwordFirePrefab.GetComponent<ItemDrop>();
                itemDropFire.m_itemData.m_shared.m_name = configTriSwordName.Value;
                itemDropFire.m_itemData.m_shared.m_description = configTriSwordDescription.Value;
                itemDropFire.m_itemData.m_shared.m_maxQuality = configTriSwordMaxQuality.Value;
                itemDropFire.m_itemData.m_shared.m_equipStatusEffect = triSwordFireStatusEffect.StatusEffect;
                ItemDrop itemDropFrost = triSwordFrostPrefab.GetComponent<ItemDrop>();
                itemDropFrost.m_itemData.m_shared.m_name = configTriSwordName.Value;
                itemDropFrost.m_itemData.m_shared.m_description = configTriSwordDescription.Value;
                itemDropFrost.m_itemData.m_shared.m_maxQuality = configTriSwordMaxQuality.Value;
                itemDropFrost.m_itemData.m_shared.m_equipStatusEffect = triSwordFrostStatusEffect.StatusEffect;

                ItemManager.Instance.AddItem(new CustomItem(triSwordLightningPrefab, true, itemConfig));
                ItemManager.Instance.AddItem(new CustomItem(triSwordFirePrefab, true, new ItemConfig()));
                ItemManager.Instance.AddItem(new CustomItem(triSwordFrostPrefab, true, new ItemConfig()));

                //CustomItem DirksBoner = new CustomItem("DirksBoner_DW", "SwordMistwalker", DirkBonerConfig);

                //// Stats
                //DirksBoner.ItemDrop.m_itemData.m_shared.m_damages.m_damage = 0;
                //DirksBoner.ItemDrop.m_itemData.m_shared.m_damages.m_slash = 35;
                //DirksBoner.ItemDrop.m_itemData.m_shared.m_damages.m_frost = 40;
                //DirksBoner.ItemDrop.m_itemData.m_shared.m_damages.m_spirit = 20;
                //DirksBoner.ItemDrop.m_itemData.m_shared.m_attack.m_attackStamina = 12;
                //DirksBoner.ItemDrop.m_itemData.m_shared.m_blockPower = 30;
                //DirksBoner.ItemDrop.m_itemData.m_shared.m_attackForce = 80;
                //DirksBoner.ItemDrop.m_itemData.m_shared.m_deflectionForce = 30;

                //// Quality
                //DirksBoner.ItemDrop.m_itemData.m_shared.m_maxQuality = 10;
                //DirksBoner.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_slash = 3;
                //DirksBoner.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_frost = 3;
                //DirksBoner.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_spirit = 3;
                //DirksBoner.ItemDrop.m_itemData.m_shared.m_blockPowerPerLevel = 5;
                //DirksBoner.ItemDrop.m_itemData.m_shared.m_durabilityPerLevel = 50;

                //ItemManager.Instance.AddItem(DirksBoner);

                PrefabManager.OnVanillaPrefabsAvailable -= AddTriSword;
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise the Tri Sword: " + error);
            }
        }

        private void AddCultivatorAtgeir()
        {
            try
            {
                ConfigRecipe recipe = new ConfigRecipe(configCultivatorAtgeirRecipe.Value, configCultivatorAtgeirRecipeUpgrade.Value);
                ItemConfig itemConfig = new ItemConfig();
                itemConfig.Name = configCultivatorAtgeirName.Value;
                itemConfig.Description = configCultivatorAtgeirDescription.Value;
                itemConfig.CraftingStation = configCultivatorAtgeirCraftingStation.Value;
                itemConfig.MinStationLevel = configCultivatorAtgeirMinStationLevel.Value;

                foreach (var requirement in recipe.requirements)
                {
                    int multiplier = configCultivatorAtgeirRecipeMultiplier.Value != 0 ? configCultivatorAtgeirRecipeMultiplier.Value : 1;
                    int amountPerlevel = requirement.amountPerLevel * multiplier;
                    itemConfig.AddRequirement(new RequirementConfig(requirement.material, requirement.amount, amountPerlevel, true));
                }

                ItemDrop itemDropAtgeir = cultivatorAtgeirAtgeirPrefab.GetComponent<ItemDrop>();
                itemDropAtgeir.m_itemData.m_shared.m_maxQuality = configCultivatorAtgeirMaxQuality.Value;
                itemDropAtgeir.m_itemData.m_shared.m_movementModifier = configCultivatorAtgeirMovementSpeed.Value;
                itemDropAtgeir.m_itemData.m_shared.m_damages.m_pierce = itemDropAtgeir.m_itemData.m_shared.m_damages.m_blunt * configCultivatorAtgeirDamageMultiplier.Value;
                itemDropAtgeir.m_itemData.m_shared.m_damages.m_lightning = itemDropAtgeir.m_itemData.m_shared.m_damages.m_lightning * configCultivatorAtgeirDamageMultiplier.Value;
                itemDropAtgeir.m_itemData.m_shared.m_blockPower = configCultivatorAtgeirBlockArmor.Value;
                itemDropAtgeir.m_itemData.m_shared.m_deflectionForce = configCultivatorAtgeirBlockForce.Value;
                itemDropAtgeir.m_itemData.m_shared.m_attackForce = configCultivatorAtgeirKnockBack.Value;
                itemDropAtgeir.m_itemData.m_shared.m_backstabBonus = configCultivatorAtgeirBackStab.Value;
                itemDropAtgeir.m_itemData.m_shared.m_attack.m_attackStamina = configCultivatorAtgeirUseStamina.Value;
                itemDropAtgeir.m_itemData.m_shared.m_secondaryAttack.m_attackStamina = configCultivatorAtgeirUseStaminaAtgeir.Value;

                ItemDrop itemDropBattleAxe = cultivatorAtgeirBattleAxePrefab.GetComponent<ItemDrop>();
                itemDropBattleAxe.m_itemData.m_shared.m_name = configCultivatorAtgeirName.Value;
                itemDropBattleAxe.m_itemData.m_shared.m_description = configCultivatorAtgeirDescription.Value;
                itemDropBattleAxe.m_itemData.m_shared.m_maxQuality = configCultivatorAtgeirMaxQuality.Value;
                itemDropBattleAxe.m_itemData.m_shared.m_movementModifier = configCultivatorAtgeirMovementSpeed.Value;
                itemDropBattleAxe.m_itemData.m_shared.m_damages.m_pierce = itemDropBattleAxe.m_itemData.m_shared.m_damages.m_blunt * configCultivatorAtgeirDamageMultiplier.Value;
                itemDropBattleAxe.m_itemData.m_shared.m_damages.m_lightning = itemDropBattleAxe.m_itemData.m_shared.m_damages.m_lightning * configCultivatorAtgeirDamageMultiplier.Value;
                itemDropBattleAxe.m_itemData.m_shared.m_blockPower = configCultivatorAtgeirBlockArmor.Value;
                itemDropBattleAxe.m_itemData.m_shared.m_deflectionForce = configCultivatorAtgeirBlockForce.Value;
                itemDropBattleAxe.m_itemData.m_shared.m_attackForce = configCultivatorAtgeirKnockBack.Value;
                itemDropBattleAxe.m_itemData.m_shared.m_backstabBonus = configCultivatorAtgeirBackStab.Value;
                itemDropBattleAxe.m_itemData.m_shared.m_attack.m_attackStamina = configCultivatorAtgeirUseStamina.Value;
                itemDropBattleAxe.m_itemData.m_shared.m_secondaryAttack.m_attackStamina = configCultivatorAtgeirUseStaminaPoke.Value;

                ItemManager.Instance.AddItem(new CustomItem(cultivatorAtgeirAtgeirPrefab, true, itemConfig));
                ItemManager.Instance.AddItem(new CustomItem(cultivatorAtgeirBattleAxePrefab, true, new ItemConfig()));
                PrefabManager.OnVanillaPrefabsAvailable -= AddCultivatorAtgeir;
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise the Cultivator Atgeir: " + error);
            }
        }

        private void InitStatusEffects()
        {
            try
            {
                StatusEffect demoHammerHammerEffect = ScriptableObject.CreateInstance<StatusEffect>();
                demoHammerHammerEffect.name = "DemoHammerHammerEffect";
                demoHammerHammerEffect.m_name = "Hammer Mode";
                demoHammerHammerEffect.m_icon = demolisherSprite;
                demoHammerHammerEffect.m_startMessageType = MessageHud.MessageType.Center;
                demoHammerHammerEffect.m_startMessage = "";
                demoHammerHammerEffect.m_stopMessageType = MessageHud.MessageType.Center;
                demoHammerHammerEffect.m_stopMessage = "";
                demoHammerHammerEffect.m_tooltip = "Hammer Time!";
                demoHammerHammerStatusEffect = new CustomStatusEffect(demoHammerHammerEffect, fixReference: false);  // We dont need to fix refs here, because no mocks were used
                ItemManager.Instance.AddStatusEffect(demoHammerHammerStatusEffect);

                StatusEffect demoHammerAtgeirEffect = ScriptableObject.CreateInstance<StatusEffect>();
                demoHammerAtgeirEffect.name = "DemoHammerAtgeirEffect";
                demoHammerAtgeirEffect.m_name = "Atgeir Mode";
                demoHammerAtgeirEffect.m_icon = himminAflSprite;
                demoHammerAtgeirEffect.m_startMessageType = MessageHud.MessageType.Center;
                demoHammerAtgeirEffect.m_startMessage = "";
                demoHammerAtgeirEffect.m_stopMessageType = MessageHud.MessageType.Center;
                demoHammerAtgeirEffect.m_stopMessage = "";
                demoHammerAtgeirEffect.m_tooltip = "Swirl that Hammer around as if it was an Atgeir!";
                demoHammerAtgeirStatusEffect = new CustomStatusEffect(demoHammerAtgeirEffect, fixReference: false);  // We dont need to fix refs here, because no mocks were used
                ItemManager.Instance.AddStatusEffect(demoHammerAtgeirStatusEffect);

                StatusEffect triSwordLightningEffect = ScriptableObject.CreateInstance<StatusEffect>();
                triSwordLightningEffect.name = "TriSwordLightningEffect";
                triSwordLightningEffect.m_name = "Lightning Mode";
                triSwordLightningEffect.m_icon = lightningSprite;
                triSwordLightningEffect.m_startMessageType = MessageHud.MessageType.Center;
                triSwordLightningEffect.m_startMessage = "";
                triSwordLightningEffect.m_stopMessageType = MessageHud.MessageType.Center;
                triSwordLightningEffect.m_stopMessage = "";
                triSwordLightningEffect.m_tooltip = "The power of Thor flows through the blade!";
                triSwordLightningStatusEffect = new CustomStatusEffect(triSwordLightningEffect, fixReference: false);  // We dont need to fix refs here, because no mocks were used
                ItemManager.Instance.AddStatusEffect(triSwordLightningStatusEffect);

                StatusEffect triSwordFireEffect = ScriptableObject.CreateInstance<StatusEffect>();
                triSwordFireEffect.name = "TriSwordFireEffect";
                triSwordFireEffect.m_name = "Fire Mode";
                triSwordFireEffect.m_icon = swordFireSprite;
                triSwordFireEffect.m_startMessageType = MessageHud.MessageType.Center;
                triSwordFireEffect.m_startMessage = "";
                triSwordFireEffect.m_stopMessageType = MessageHud.MessageType.Center;
                triSwordFireEffect.m_stopMessage = "";
                triSwordFireEffect.m_tooltip = "Your blade is on fire!";
                triSwordFireStatusEffect = new CustomStatusEffect(triSwordFireEffect, fixReference: false);  // We dont need to fix refs here, because no mocks were used
                ItemManager.Instance.AddStatusEffect(triSwordFireStatusEffect);

                StatusEffect triSwordFrostEffect = ScriptableObject.CreateInstance<StatusEffect>();
                triSwordFrostEffect.name = "TriSwordFrostEffect";
                triSwordFrostEffect.m_name = "Frost Mode";
                triSwordFrostEffect.m_icon = frostSprite;
                triSwordFrostEffect.m_startMessageType = MessageHud.MessageType.Center;
                triSwordFrostEffect.m_startMessage = "";
                triSwordFrostEffect.m_stopMessageType = MessageHud.MessageType.Center;
                triSwordFrostEffect.m_stopMessage = "";
                triSwordFrostEffect.m_tooltip = "Your blade feels as cold as Jotunheim!";
                triSwordFrostStatusEffect = new CustomStatusEffect(triSwordFrostEffect, fixReference: false);  // We dont need to fix refs here, because no mocks were used
                ItemManager.Instance.AddStatusEffect(triSwordFrostStatusEffect);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise status effects: " + error);
            }
        }

        private void InitConfig()
        {
            try
            {
                Config.SaveOnConfigSet = true;

                // General
                configDemoHammerEnable = base.Config.Bind(new ConfigDefinition(sectionGeneral, "Enable"), true,
                    new ConfigDescription("Enable this mod", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configWeaponModeKey = Config.Bind(sectionGeneral, "Weapon mode key", new KeyboardShortcut(KeyCode.Y),
                    new ConfigDescription("Key to change the weaponmode (applies to all weapons)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                // Demolition Hammer
                configDemoHammerEnable = base.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Enable"), true,
                    new ConfigDescription("Enable the Demolition Hammer", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configDemoHammerName = base.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Name"), "Kimetsus Special",
                    new ConfigDescription("The name given to the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configDemoHammerDescription = base.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Description"), "May the bones of you enemies forever be crushed, my friend... to dust!",
                    new ConfigDescription("The description given to the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configDemoHammerCraftingStation = base.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Crafting station"), "Forge",
                    new ConfigDescription("The crafting station the item can be created in",
                    new AcceptableValueList<string>(new string[] { "Disabled", "Inventory", "Workbench", "Cauldron", "Forge", "ArtisanTable", "StoneCutter", "MageTable", "BlackForge" }),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configDemoHammerMinStationLevel = base.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Required station level"), 1,
                    new ConfigDescription("The required station level to craft the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configDemoHammerRecipe = base.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Crafting costs"), "Silver:30,ElderBark:10,Thunderstone:5,YmirRemains:5",
                    new ConfigDescription("The items required to craft the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configDemoHammerRecipeUpgrade = base.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Upgrade costs"), "Silver:5,ElderBark:3,Thunderstone:1",
                    new ConfigDescription("The costs to upgrade the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configDemoHammerRecipeMultiplier = base.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Upgrade multiplier"), 1,
                    new ConfigDescription("The multiplier applied to the upgrade costs", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configDemoHammerMaxQuality = base.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Max quality"), 4,
                    new ConfigDescription("The maximum quality the item can become", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configDemoHammerMovementSpeed = base.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Movement speed"), -0.05f,
                    new ConfigDescription("The movement speed stat on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configDemoHammerDamageMultiplier = base.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Damage multiplier"), 1f,
                    new ConfigDescription("Multiplier to adjust the damage on the item (90 blunt, 30 lightning)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configDemoHammerBlockArmor = base.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Block armor"), 40,
                    new ConfigDescription("The block armor on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configDemoHammerBlockForce = base.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Block force"), 70,
                    new ConfigDescription("The block force on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configDemoHammerKnockBack = base.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Knockback"), 75,
                    new ConfigDescription("The knockback on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configDemoHammerBackStab = base.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Backstab"), 3,
                    new ConfigDescription("The block armor on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configDemoHammerUseStamina = base.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Attack stamina"), 22,
                    new ConfigDescription("Normal attack stamina usage", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configDemoHammerUseStaminaHammer = base.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Secondary hammer ability stamina"), 28,
                    new ConfigDescription("The secondary hammer attack stamina usage", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configDemoHammerUseStaminaAtgeir = base.Config.Bind(new ConfigDefinition(sectionDemoHammer, "Secondary atgeir ability stamina"), 40,
                    new ConfigDescription("The secondary atgeir attack stamina usage", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                // Tri Sword
                configTriSwordEnable = base.Config.Bind(new ConfigDefinition(sectionTriSword, "Enable"), true,
                    new ConfigDescription("Enable the Tri Sword", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configTriSwordName = base.Config.Bind(new ConfigDefinition(sectionTriSword, "Name"), "Dirks Dominance",
                    new ConfigDescription("The name given to the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configTriSwordDescription = base.Config.Bind(new ConfigDefinition(sectionTriSword, "Description"), "Got to assert your DOMINANCE!",
                    new ConfigDescription("The description given to the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configTriSwordCraftingStation = base.Config.Bind(new ConfigDefinition(sectionTriSword, "Crafting station"), "Forge",
                    new ConfigDescription("The crafting station the item can be created in",
                    new AcceptableValueList<string>(new string[] { "Disabled", "Inventory", "Workbench", "Cauldron", "Forge", "ArtisanTable", "StoneCutter", "MageTable", "BlackForge" }),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configTriSwordMinStationLevel = base.Config.Bind(new ConfigDefinition(sectionTriSword, "Required station level"), 1,
                    new ConfigDescription("The required station level to craft the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configTriSwordRecipe = base.Config.Bind(new ConfigDefinition(sectionTriSword, "Crafting costs"), "Silver:25,ElderBark:10,FreezeGland:5,YmirRemains:5",
                    new ConfigDescription("The items required to craft the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configTriSwordRecipeUpgrade = base.Config.Bind(new ConfigDefinition(sectionTriSword, "Upgrade costs"), "Silver:5,ElderBark:5,FreezeGland:3",
                    new ConfigDescription("The costs to upgrade the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configTriSwordRecipeMultiplier = base.Config.Bind(new ConfigDefinition(sectionTriSword, "Upgrade multiplier"), 1,
                    new ConfigDescription("The multiplier applied to the upgrade costs", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configTriSwordMaxQuality = base.Config.Bind(new ConfigDefinition(sectionTriSword, "Max quality"), 4,
                    new ConfigDescription("The maximum quality the item can become", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                // Cultivator Atgeir
                configCultivatorAtgeirEnable = base.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Enable"), true,
                    new ConfigDescription("Enable the Cultivator Atgeir", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configCultivatorAtgeirName = base.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Name"), "Marks Hark",
                    new ConfigDescription("The name given to the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configCultivatorAtgeirDescription = base.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Description"), "Imma steal them sweet lewds from Durk! Grabby grabby!",
                    new ConfigDescription("The description given to the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configCultivatorAtgeirCraftingStation = base.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Crafting station"), "Forge",
                    new ConfigDescription("The crafting station the item can be created in",
                    new AcceptableValueList<string>(new string[] { "Disabled", "Inventory", "Workbench", "Cauldron", "Forge", "ArtisanTable", "StoneCutter", "MageTable", "BlackForge" }),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configCultivatorAtgeirMinStationLevel = base.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Required station level"), 1,
                    new ConfigDescription("The required station level to craft the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configCultivatorAtgeirRecipe = base.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Crafting costs"), "Silver:25,ElderBark:15,FreezeGland:5,YmirRemains:5",
                    new ConfigDescription("The items required to craft the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configCultivatorAtgeirRecipeUpgrade = base.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Upgrade costs"), "Silver:5,ElderBark:5,FreezeGland:3",
                    new ConfigDescription("The costs to upgrade the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configCultivatorAtgeirRecipeMultiplier = base.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Upgrade multiplier"), 1,
                    new ConfigDescription("The multiplier applied to the upgrade costs", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configCultivatorAtgeirMaxQuality = base.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Max quality"), 4,
                    new ConfigDescription("The maximum quality the item can become", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configCultivatorAtgeirMovementSpeed = base.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Movement speed"), 0f,
                    new ConfigDescription("The movement speed stat on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configCultivatorAtgeirDamageMultiplier = base.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Damage multiplier"), 1f,
                    new ConfigDescription("Multiplier to adjust the damage on the item (85, 40 lightning)", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configCultivatorAtgeirBlockArmor = base.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Block armor"), 64,
                    new ConfigDescription("The block armor on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configCultivatorAtgeirBlockForce = base.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Block force"), 40,
                    new ConfigDescription("The block force on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configCultivatorAtgeirKnockBack = base.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Knockback"), 40,
                    new ConfigDescription("The knockback on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configCultivatorAtgeirBackStab = base.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Backstab"), 3,
                    new ConfigDescription("The block armor on the item", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configCultivatorAtgeirUseStamina = base.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Attack stamina"), 20,
                    new ConfigDescription("Normal attack stamina usage", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configCultivatorAtgeirUseStaminaAtgeir = base.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Secondary atgeir ability stamina"), 40,
                    new ConfigDescription("The secondary atgeir attack stamina usage", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

                configCultivatorAtgeirUseStaminaPoke = base.Config.Bind(new ConfigDefinition(sectionCultivatorAtgeir, "Secondary poke ability stamina"), 9,
                    new ConfigDescription("The secondary poke attack stamina usage", null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise config: " + error);
            }
        }

        private void InitAssetBundle()
        {
            try
            {
                specialWeaponsBundle = AssetUtils.LoadAssetBundleFromResources("specialweapons_dw");
                demoHammerHammerPrefab = specialWeaponsBundle.LoadAsset<GameObject>("Demo_Hammer_Hammer_DW");
                demoHammerAtgeirPrefab = specialWeaponsBundle.LoadAsset<GameObject>("Demo_Hammer_Atgeir_DW");
                cultivatorAtgeirAtgeirPrefab = specialWeaponsBundle.LoadAsset<GameObject>("Cultivator_Atgeir_Atgeir_DW");
                cultivatorAtgeirBattleAxePrefab = specialWeaponsBundle.LoadAsset<GameObject>("Cultivator_Atgeir_BattleAxe_DW");
                triSwordLightningPrefab = specialWeaponsBundle.LoadAsset<GameObject>("TriSword_Lightning_DW");
                triSwordFirePrefab = specialWeaponsBundle.LoadAsset<GameObject>("TriSword_Fire_DW");
                triSwordFrostPrefab = specialWeaponsBundle.LoadAsset<GameObject>("TriSword_Frost_DW");

                demolisherSprite = specialWeaponsBundle.LoadAsset<Sprite>("SledgeDemolisher_DW");
                himminAflSprite = specialWeaponsBundle.LoadAsset<Sprite>("AtgeirHimminAfl_DW");
                lightningSprite = specialWeaponsBundle.LoadAsset<Sprite>("Lightning_DW");
                swordFireSprite = specialWeaponsBundle.LoadAsset<Sprite>("SwordFire_DW");
                frostSprite = specialWeaponsBundle.LoadAsset<Sprite>("Frost_DW");
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise asset bundle: " + error);
            }
        }

        private void InitInputs()
        {
            try
            {
                weaponModeButton = new ButtonConfig
                {
                    Name = "Weapon mode",
                    ShortcutConfig = configWeaponModeKey,
                    ActiveInGUI = true,
                    ActiveInCustomGUI = true,
                };

                InputManager.Instance.AddButton(PluginGUID, weaponModeButton);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise inputs: "+ error);
            }
        }
    }
}
