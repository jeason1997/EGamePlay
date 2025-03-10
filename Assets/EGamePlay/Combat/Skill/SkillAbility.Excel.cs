﻿using System;
using GameUtils;
using ET;
using System.Collections.Generic;
using UnityEngine;

#if EGAMEPLAY_EXCEL
namespace EGamePlay.Combat
{
    public class SkillAbility : AbilityEntity
    {
        public SkillConfig SkillConfig { get; set; }
        public bool Spelling { get; set; }
        public GameTimer CooldownTimer { get; } = new GameTimer(1f);
        private List<StatusAbility> ChildrenStatuses { get; set; } = new List<StatusAbility>();


        public override void Awake(object initData)
        {
            base.Awake(initData);
            SkillConfig = initData as SkillConfig;
            if (SkillConfig.Type == "被动")
            {
                TryActivateAbility();
            }
        }

        public override void ActivateAbility()
        {
            base.ActivateAbility();
        }

        public override void EndAbility()
        {
            base.EndAbility();
        }

        public override AbilityExecution CreateExecution()
        {
            var execution = Entity.CreateWithParent<SkillExecution>(OwnerEntity, this);
            execution.AddComponent<UpdateComponent>();
            return execution;
        }

        public override void ApplyAbilityEffectsTo(CombatEntity targetEntity)
        {
            var Effects = new List<Effect>();
            var effect = ParseSkillDamage(SkillConfig);
            if (effect != null) Effects.Add(effect);
            effect = ParseEffect(SkillConfig, SkillConfig.Effect1);
            if (effect != null) Effects.Add(effect);
            effect = ParseEffect(SkillConfig, SkillConfig.Effect2);
            if (effect != null) Effects.Add(effect);
            effect = ParseEffect(SkillConfig, SkillConfig.Effect3);
            if (effect != null) Effects.Add(effect);
            foreach (var effectItem in Effects)
            {
                ApplyEffectTo(targetEntity, effectItem);
            }
        }

        public Effect ParseSkillDamage(SkillConfig skillConfig)
        {
            Effect effect = null;
            if (string.IsNullOrEmpty(skillConfig.DamageTarget) == false)
            {
                var damageEffect = new DamageEffect();
                effect = damageEffect;
                damageEffect.DamageValueFormula = skillConfig.ValueFormula;
                damageEffect.TriggerProbability = skillConfig.Probability;
                if (skillConfig.DamageTarget == "自身") damageEffect.AddSkillEffectTargetType = AddSkillEffetTargetType.Self;
                if (skillConfig.DamageTarget == "技能目标") damageEffect.AddSkillEffectTargetType = AddSkillEffetTargetType.SkillTarget;
                if (skillConfig.DamageType == "魔法伤害") damageEffect.DamageType = DamageType.Magic;
                if (skillConfig.DamageType == "物理伤害") damageEffect.DamageType = DamageType.Physic;
                if (skillConfig.DamageType == "真实伤害") damageEffect.DamageType = DamageType.Real;
            }
            return effect;
        }

        public Effect ParseEffect(SkillConfig skillConfig, string effectConfig)
        {
            Effect effect = null;
            if (!string.IsNullOrEmpty(effectConfig) && effectConfig.Contains("="))
            {
                effectConfig = effectConfig.Replace("=Id", $"={skillConfig.Id}");
                var arr = effectConfig.Split('=');
                var effectType = arr[0];
                var effectId = arr[1];
                var skillEffectConfig = ConfigHelper.Get<SkillEffectsConfig>(int.Parse(effectId));
                var KVList = new List<string>(3);
                KVList.Add(skillEffectConfig.KV1);
                KVList.Add(skillEffectConfig.KV2);
                KVList.Add(skillEffectConfig.KV3);
                //var effectJsonStr = "{";
                //foreach (var item in paramList)
                //{
                //    var fieldStr = item.Replace("=", ":");
                //    fieldStr = fieldStr.Replace("伤害类型", "DamageType");
                //    fieldStr = fieldStr.Replace("伤害取值", "DamageValueFormula");
                //    effectJsonStr += $"{fieldStr},";
                //}
                //effectJsonStr += "}";
                //Log.Debug(effectJsonStr);
                if (effectType == "Damage")
                {
                    var Type = "";
                    var DamageValueFormula = "";
                    foreach (var item in KVList)
                    {
                        if (string.IsNullOrEmpty(item)) continue;
                        if (item.Contains("伤害类型=")) Type = item.Replace("伤害类型=", "");
                        if (item.Contains("伤害取值=")) DamageValueFormula = item.Replace("伤害取值=", "");
                    }
                    var damageEffect = new DamageEffect();
                    effect = damageEffect;
                    damageEffect.DamageValueFormula = DamageValueFormula;
                    damageEffect.TriggerProbability = skillEffectConfig.Probability;
                    if (skillEffectConfig.Target == "自身") damageEffect.AddSkillEffectTargetType = AddSkillEffetTargetType.Self;
                    if (skillEffectConfig.Target == "技能目标") damageEffect.AddSkillEffectTargetType = AddSkillEffetTargetType.SkillTarget;
                    if (Type == "魔法伤害") damageEffect.DamageType = DamageType.Magic;
                    if (Type == "物理伤害") damageEffect.DamageType = DamageType.Physic;
                    if (Type == "真实伤害") damageEffect.DamageType = DamageType.Real;
                }
                if (effectType == "AddStatus")
                {
                    //var skillEffectConfig = ConfigHelper.Get<SkillEffectsConfig>(int.Parse(effectId));
                    var StatusID = "";
                    var Duration = "";
                    foreach (var item in KVList)
                    {
                        if (string.IsNullOrEmpty(item)) continue;
                        if (item.Contains("状态类型=")) StatusID = item.Replace("状态类型=", "");
                        if (item.Contains("持续时间=")) Duration = item.Replace("持续时间=", "");
                    }
                    var addStatusEffect = new AddStatusEffect();
                    effect = addStatusEffect;
                    addStatusEffect.AddStatus = Resources.Load<StatusConfigObject>($"StatusConfigs/Status_{StatusID}");
                    if (addStatusEffect.AddStatus == null)
                    {
                        addStatusEffect.AddStatus = Resources.Load<StatusConfigObject>($"StatusConfigs/BaseStatus/Status_{StatusID}");
                    }
                    addStatusEffect.Duration = (uint)(float.Parse(Duration) * 1000);
                    ParseParam(skillEffectConfig.Param1);
                    ParseParam(skillEffectConfig.Param2);
                    void ParseParam(string paramStr)
                    {
                        if (!string.IsNullOrEmpty(paramStr))
                        {
                            arr = paramStr.Split('=');
                            addStatusEffect.Params.Add(arr[0], arr[1]);
                        }
                    }
                }
            }
            else
            {
                effect = new CustomEffect() {  CustomEffectType = effectConfig };
            }
            return effect;
        }
    }
}
#endif