using HollowTwitch.Entities;
using Modding;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using UnityEngine;

namespace HollowTwitch.Commands.ModHelpers
{
    public static class BindingsHelper
    {
        //stolen 100% from https://github.com/fifty-six/HollowKnight.Bindings
        private static readonly List<Detour> _detours =  new List<Detour>();

        public static bool True() => true;

        public static int BoundNailDamage()
        {
            int @base = PlayerData.instance.nailDamage;

            return @base < 13 ? Mathf.RoundToInt(@base * .8f) : 13;
        }

        public static int BoundMaxHealth() => 4;

        private static readonly string[] BindingProperties =
        {
            nameof(BossSequenceController.BoundNail),
            nameof(BossSequenceController.BoundCharms),
            nameof(BossSequenceController.BoundShell),
            nameof(BossSequenceController.BoundSoul)
        };


        public static void AddDetours()
        {
            foreach (string property in BindingProperties)
            {
                _detours.Add
                (
                    new Detour
                    (
                        typeof(BossSequenceController).GetProperty(property)?.GetGetMethod(),
                        typeof(BindingsHelper).GetMethod(nameof(True))
                    )
                ); 
            }

            _detours.Add
            (
                new Detour
                (
                    typeof(BossSequenceController).GetProperty(nameof(BossSequenceController.BoundNailDamage))?.GetGetMethod(),
                    typeof(BindingsHelper).GetMethod(nameof(BoundNailDamage))
                )
            );

            _detours.Add
            (
                new Detour
                (
                    typeof(BossSequenceController).GetProperty(nameof(BossSequenceController.BoundMaxHealth))?.GetGetMethod(),
                    typeof(BindingsHelper).GetMethod(nameof(BoundMaxHealth))
                )
            );

           
        }

        
        public static void ShowIcons()
            => _ = GameManager.instance.StartCoroutine(ShowIconsCoroutine());

        public static void CheckBoundSoulEnter(On.GGCheckBoundSoul.orig_OnEnter orig, GGCheckBoundSoul self)
        {
            self.Fsm.Event(self.boundEvent);
            self.Finish();
        }

        public static void NoOp(On.BossSceneController.orig_RestoreBindings orig, BossSceneController self) { }

        private static IEnumerator ShowIconsCoroutine()
        {
            yield return new WaitWhile(() => HeroController.instance == null);

            yield return null;

            EventRegister.SendEvent("SHOW BOUND NAIL");
            EventRegister.SendEvent("SHOW BOUND CHARMS");
            EventRegister.SendEvent("BIND VESSEL ORB");

            GameManager.instance.soulOrb_fsm.SetState("Bound");

            if (PlayerData.instance.equippedCharms.Count == 0) yield break;

            foreach (int charm in PlayerData.instance.equippedCharms)
            {
                GameManager.instance.SetPlayerDataBool($"equippedCharm_{charm}", false);
            }

            PlayerData.instance.equippedCharms.Clear();
        }

        public static void RestoreBindingsUI()
        {
            EventRegister.SendEvent("UPDATE BLUE HEALTH");
            EventRegister.SendEvent("HIDE BOUND NAIL");
            PlayMakerFSM.BroadcastEvent("CHARM EQUIP CHECK");
            EventRegister.SendEvent("HIDE BOUND CHARMS");
            GameManager.instance.soulOrb_fsm.SendEvent("MP LOSE");
            EventRegister.SendEvent("UNBIND VESSEL ORB");
            PlayMakerFSM.BroadcastEvent("CHARM INDICATOR CHECK");
            PlayMakerFSM.BroadcastEvent("HUD IN");
        }

        public static void Unload()
        {
            foreach (Detour d in _detours)
                d.Dispose();

            _detours.Clear();

            On.BossSceneController.RestoreBindings -= NoOp;
            On.GGCheckBoundSoul.OnEnter -= CheckBoundSoulEnter;
            RestoreBindingsUI();
            
        }
    }
}
