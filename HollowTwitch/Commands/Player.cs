using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using GlobalEnums;
using HollowTwitch.Entities.Attributes;
using HollowTwitch.ModHelpers;
using HollowTwitch.Precondition;
using HollowTwitch.Utils;
using HutongGames.PlayMaker;
using Modding;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Vasi;
using UObject = UnityEngine.Object;

namespace HollowTwitch.Commands
{
    public class Player
    {
        // Most (all) of the commands stolen from Chaos Mod by Seanpr
        private GameObject _maggot;

        public Player()
        {
            IEnumerator GetMaggotPrime()
            {
                const string hwurmpURL = "https://cdn.discordapp.com/attachments/410556297046523905/716824653280313364/hwurmpU.png";

                UnityWebRequest www = UnityWebRequestTexture.GetTexture(hwurmpURL);

                yield return www.SendWebRequest();

                Texture texture = DownloadHandlerTexture.GetContent(www);

                Sprite maggotPrime = Sprite.Create
                (
                    (Texture2D) texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );

                _maggot = new GameObject("maggot");
                _maggot.AddComponent<SpriteRenderer>().sprite = maggotPrime;
                _maggot.SetActive(false);

                UObject.DontDestroyOnLoad(_maggot);
            }

            GameManager.instance.StartCoroutine(GetMaggotPrime());
        }

        [HKCommand("ax2uBlind")]
        [Summary("Makes all rooms dark like lanternless rooms for a time.")]
        [Cooldown(60)]
        public IEnumerator Blind()
        {
            void OnSceneLoad(On.GameManager.orig_EnterHero orig, GameManager self, bool additiveGateSearch)
            {
                orig(self, additiveGateSearch);
                
                DarknessHelper.Darken();
            }

            DarknessHelper.Darken();
            On.GameManager.EnterHero += OnSceneLoad;

            yield return new WaitForSecondsRealtime(30);

            On.GameManager.EnterHero -= OnSceneLoad;
            DarknessHelper.Lighten();
        }

        [HKCommand("nopogo")]
        [Summary("Disables pogo knockback temporarily.")]
        [Cooldown(60)]
        public IEnumerator PogoKnockback()
        {
            void NoBounce(On.HeroController.orig_Bounce orig, HeroController self) { }

            On.HeroController.Bounce += NoBounce;

            yield return new WaitForSecondsRealtime(30);

            On.HeroController.Bounce -= NoBounce;
        }

        [HKCommand("conveyor")]
        [Summary("Floors or walls will act like conveyors")]
        [Cooldown(60)]
        public IEnumerator Conveyor()
        {
            bool vert = Random.Range(0, 2) == 0;

            float speed = Random.Range(-30f, 30f);

            HeroController hc = HeroController.instance;

            if (vert)
            {
                hc.cState.onConveyorV = true;
                hc.GetComponent<ConveyorMovementHero>().StartConveyorMove(0f, speed);
            }
            else
            {
                hc.cState.onConveyor = true;
                hc.SetConveyorSpeed(speed);
            }

            yield return new WaitForSecondsRealtime(30);

            if (vert)
                hc.cState.onConveyorV = false;
            else
                hc.cState.onConveyor = false;

            hc.GetComponent<ConveyorMovementHero>().StopConveyorMove();
        }

        [HKCommand("jumplength")]
        [Summary("Gives a random jump length.")]
        [Cooldown(60)]
        public IEnumerator JumpLength()
        {
            HeroController hc = HeroController.instance;

            int prev_steps = hc.JUMP_STEPS;

            hc.JUMP_STEPS = Random.Range(hc.JUMP_STEPS / 2, hc.JUMP_STEPS * 8);

            yield return new WaitForSecondsRealtime(30);

            hc.JUMP_STEPS = prev_steps;
        }

        [HKCommand("lifeblood")]
        [Cooldown(40)]
        public IEnumerator Lifeblood()
        {
            int r = Random.Range(1, 10);

            for (int i = 0; i < r; i++)
            {
                yield return null;

                EventRegister.SendEvent("ADD BLUE HEALTH");

                yield return null;
            }
        }

        [HKCommand("godmode")]
        [Cooldown(60)]
        public IEnumerator Godmode()
        {
            static int TakeHealth(int damage) => 0;

            static HitInstance HitInstance(Fsm owner, HitInstance hit)
            {
                hit.DamageDealt = 1 << 8;

                return hit;
            }

            ModHooks.TakeHealthHook += TakeHealth;
            ModHooks.HitInstanceHook += HitInstance;

            yield return new WaitForSeconds(15f);

            ModHooks.TakeHealthHook -= TakeHealth;
            ModHooks.HitInstanceHook -= HitInstance;
        }

        [HKCommand("sleep")]
        [Cooldown(60)]
        public IEnumerator Sleep()
        {
            const string SLEEP_CLIP = "Wake Up Ground";

            HeroController hc = HeroController.instance;

            var anim = hc.GetComponent<HeroAnimationController>();

            anim.PlayClip(SLEEP_CLIP);

            hc.StopAnimationControl();
            hc.RelinquishControl();

            yield return new WaitForSeconds(anim.GetClipDuration(SLEEP_CLIP));

            hc.StartAnimationControl();
            hc.RegainControl();
        }

        [HKCommand("limitSoul")]
        [Cooldown(60)]
        public IEnumerator LimitSoul()
        {
            yield return PlayerDataUtil.FakeSet(nameof(PlayerData.soulLimited), false, 30);
        }

        [HKCommand("jumpspeed")]
        [Summary("Gives a random jump speed.")]
        [Cooldown(60)]
        public IEnumerator JumpSpeed()
        {
            HeroController hc = HeroController.instance;

            float prev_speed = hc.JUMP_SPEED;

            hc.JUMP_SPEED = Random.Range(hc.JUMP_SPEED / 4f, hc.JUMP_SPEED * 4f);

            yield return new WaitForSecondsRealtime(30);

            hc.JUMP_SPEED = prev_speed;
        }

        [HKCommand("wind")]
        [Summary("Make it a windy day.")]
        [Cooldown(180)]
        public IEnumerator Wind()
        {
            float speed = Random.Range(-6f, 6f);

            float prev_s = HeroController.instance.conveyorSpeed;
            
            HeroController.instance.cState.inConveyorZone = true;
            HeroController.instance.conveyorSpeed = speed;
            
            void BeforePlayerDead()
            {
                HeroController.instance.cState.inConveyorZone = false;
                HeroController.instance.conveyorSpeed = prev_s;
                
                ModHooks.BeforePlayerDeadHook -= BeforePlayerDead;
            }

            // Prevent wind from pushing you OOB on respawn.
            ModHooks.BeforePlayerDeadHook += BeforePlayerDead;

            yield return new WaitForSecondsRealtime(30);
            
            ModHooks.BeforePlayerDeadHook -= BeforePlayerDead;

            HeroController.instance.cState.inConveyorZone = false;
            HeroController.instance.conveyorSpeed = prev_s;
        }

        [HKCommand("dashSpeed")]
        [Summary("Change dash speed.")]
        [Cooldown(60)]
        public IEnumerator DashSpeed()
        {
            HeroController hc = HeroController.instance;

            float len = Random.Range(.25f * hc.DASH_SPEED, hc.DASH_SPEED * 12f);
            float orig_dash = hc.DASH_SPEED;

            hc.DASH_SPEED = len;

            yield return new WaitForSecondsRealtime(30);

            hc.DASH_SPEED = orig_dash;
        }

        [HKCommand("dashLength")]
        [Summary("Changes dash length.")]
        [Cooldown(60)]
        public IEnumerator DashLength()
        {
            HeroController hc = HeroController.instance;

            float len = Random.Range(.25f * hc.DASH_TIME, hc.DASH_TIME * 12f);
            float orig_dash = hc.DASH_TIME;

            hc.DASH_TIME = len;

            yield return new WaitForSecondsRealtime(30);

            hc.DASH_TIME = orig_dash;
        }

        [HKCommand("dashVector")]
        [Summary("Changes dash vector. New vector generated when dashing in a new direction.")]
        [Cooldown(60)]
        public IEnumerator DashVector()
        {
            Vector2? vec = null;
            Vector2? orig = null;

            Vector2 VectorHook(Vector2 change)
            {
                if (
                    orig == change
                    && vec is Vector2 v
                )
                    return v;

                const float factor = 4f;

                orig = change;

                float mag = change.magnitude;

                float x = factor * Random.Range(-mag, mag);
                float y = factor * Random.Range(-mag, mag);

                return (Vector2) (vec = new Vector2(x, y));
            }

            ModHooks.DashVectorHook += VectorHook;

            yield return new WaitForSecondsRealtime(30f);

            ModHooks.DashVectorHook -= VectorHook;
        }

        [HKCommand("triplejump")]
        [Summary("Gives you triple jump. Wings is enabled for the duration of this command.")]
        public IEnumerator TripleJump()
        {
            bool triple_jump = false;

            void Triple(On.HeroController.orig_DoDoubleJump orig, HeroController self)
            {
                orig(self);

                if (!triple_jump)
                {
                    Mirror.SetField(self, "doubleJumped", false);

                    triple_jump = true;
                }
                else
                {
                    triple_jump = false;
                }
            }
            
            On.HeroController.DoDoubleJump += Triple;

            yield return PlayerDataUtil.FakeSet(nameof(PlayerData.hasDoubleJump), true, 30);
                                                        
            On.HeroController.DoDoubleJump -= Triple;
        }

        [HKCommand("overflow")]
        [Cooldown(40)]
        [Summary("Gain more soul than you're able to carry, allowing you to cast 6 spells with base vessels.")]
        public void OverflowSoul()
        {
            HeroController.instance.AddMPChargeSpa(99 * 2);

            PlayerData.instance.MPCharge += 99;
        }

        [HKCommand("timescale")]
        [Summary("Changes the timescale of the game for the time specified. Limit: [0.01, 2f]")]
        [Cooldown(60 * 2)]
        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public IEnumerator ChangeTimescale([EnsureFloat(0.01f, 2f)] float scale)
        {
            SanicHelper.TimeScale = scale;

            Time.timeScale = Time.timeScale == 0 ? 0 : scale;

            yield return new WaitForSecondsRealtime(60);

            Time.timeScale = Time.timeScale == 0 ? 0 : 1;

            SanicHelper.TimeScale = 1;
        }

        [HKCommand("gravity")]
        [Summary("Changes the gravity to the specified scale. Scale Limit: [0.2, 1.9]")]
        [Cooldown(60 * 2)]
        public IEnumerator ChangeGravity([EnsureFloat(0.2f, 1.90f)] float scale)
        {
            var rigidBody = HeroController.instance.gameObject.GetComponent<Rigidbody2D>();

            float def = rigidBody.gravityScale;

            rigidBody.gravityScale = scale;

            yield return new WaitForSecondsRealtime(30);

            rigidBody.gravityScale = def;
        }


        [HKCommand("invertcontrols")]
        [Summary("Inverts the move direction of the player.")]
        [Cooldown(60)]
        public IEnumerator InvertControls()
        {
            void Invert(On.HeroController.orig_Move orig, HeroController self, float dir)
            {
                if (HeroController.instance.transitionState != HeroTransitionState.WAITING_TO_TRANSITION)
                {
                    orig(self, dir);
                    
                    return;
                }

                orig(self, -dir);
            }
            
            Vector2 InvertDash(Vector2 change)
            {
                return -change;
            }

            On.HeroController.Move += Invert;
            ModHooks.DashVectorHook += InvertDash;
            
            yield return new WaitForSecondsRealtime(60);
            
            On.HeroController.Move -= Invert;
            ModHooks.DashVectorHook -= InvertDash;
        }

        [HKCommand("slippery")]
        [Summary("Makes the floor have no friction at all. Lasts for 60 seconds.")]
        public IEnumerator Slippery()
        {
            float last_move_dir = 0;

            void Slip(On.HeroController.orig_Move orig, HeroController self, float move_direction)
            {
                if (HeroController.instance.transitionState != HeroTransitionState.WAITING_TO_TRANSITION)
                {
                    orig(self, move_direction);
                    
                    return;
                }

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (move_direction == 0f)
                {
                    move_direction = last_move_dir;
                }

                orig(self, move_direction);

                last_move_dir = move_direction;
            }
            
            On.HeroController.Move += Slip;

            yield return new WaitForSecondsRealtime(60);
            
            On.HeroController.Move -= Slip;
        }

        [HKCommand("nailscale")]
        [Summary("Makes the nail huge or tiny. Scale limit: [.3, 5]")]
        public IEnumerator NailScale([EnsureFloat(.3f, 5f)] float nailScale)
        {
            void ChangeNailScale(On.NailSlash.orig_StartSlash orig, NailSlash self)
            {
                orig(self);
                
                self.transform.localScale *= nailScale;
            }

            On.NailSlash.StartSlash += ChangeNailScale;

            yield return new WaitForSecondsRealtime(30f);
            
            On.NailSlash.StartSlash -= ChangeNailScale;
        }

        [HKCommand("bindings")]
        [Summary("Enables bindings.")]
        [Cooldown(60 * 5)]
        public IEnumerator EnableBindings()
        {
            BindingsHelper.AddDetours();

            On.BossSceneController.RestoreBindings += BindingsHelper.NoOp;
            On.GGCheckBoundSoul.OnEnter += BindingsHelper.CheckBoundSoulEnter;

            BindingsHelper.ShowIcons();

            yield return new WaitForSecondsRealtime(60);

            BindingsHelper.Unload();
        }

        [HKCommand("float")]
        [Cooldown(180)]
        [Summary("Gain float for 30s.")]
        public IEnumerator Float()
        {
            static void NoOp(On.HeroController.orig_AffectedByGravity orig, HeroController self, bool gravityapplies) {}
            
            HeroController.instance.AffectedByGravity(false);

            On.HeroController.AffectedByGravity += NoOp;

            yield return new WaitForSeconds(30);
            
            On.HeroController.AffectedByGravity -= NoOp;
            
            HeroController.instance.AffectedByGravity(true);
        }

        [HKCommand("Salubra")]
        [Cooldown(30)]
        [Summary("Gain salubra's blessing even when off a bench within a room.")]
        public void Salubra()
        {
            GameObject bg = GameObject.Find("Blessing Ghost");

            bg.LocateMyFSM("Blessing Control").SetState("Start Blessing");
        }

        [HKCommand("hwurmpU")]
        [Summary("I don't even know honestly.")]
        [Cooldown(60 * 5)]
        public IEnumerator EnableMaggotPrimeSkin()
        {
            GameObject go = UObject.Instantiate(_maggot, HeroController.instance.transform.position + new Vector3(0, 0, -1f), Quaternion.identity);

            go.transform.parent = HeroController.instance.transform;
            
            go.SetActive(true);

            var renderer = HeroController.instance.GetComponent<MeshRenderer>();
            renderer.enabled = false;

            yield return new WaitForSecondsRealtime(60 * 2);

            UObject.DestroyImmediate(go);

            renderer.enabled = true;
        }

        [HKCommand("walkspeed")]
        [Cooldown(180)]
        [Summary("Gain a random walk speed. Limit: [0.3, 10]")]
        public IEnumerator WalkSpeed([EnsureFloat(0.3f, 10f)] float speed)
        {
            float prev_speed = HeroController.instance.RUN_SPEED;

            HeroController.instance.RUN_SPEED *= speed;

            yield return new WaitForSeconds(20f);

            HeroController.instance.RUN_SPEED = prev_speed;
        }

        [HKCommand("geo")]
        [Cooldown(400)]
        [Summary("Explode with geo.")]
        public void Geo()
        {
            GameObject[] geos = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name.StartsWith("Geo")).ToArray();
            
            GameObject large = geos.First(x => x.name.Contains("Large"));
            GameObject medium = geos.First(x => x.name.Contains("Med"));
            GameObject small = geos.First(x => x.name.Contains("Small"));
            
            small.SetActive(true);
            medium.SetActive(true);
            large.SetActive(true);
            
            HeroController.instance.proxyFSM.SendEvent("HeroCtrl-HeroDamaged");
            HeroController.instance.StartCoroutine(
                (IEnumerator) typeof(HeroController)
                              .GetMethod("StartRecoil", BindingFlags.NonPublic | BindingFlags.Instance)
                              ?.Invoke(HeroController.instance, new object[] { CollisionSide.left, true, 0 })
            );
            
            SpawnGeo(Random.Range(200, 4000), small, medium, large);
        }

        [HKCommand("respawn")]
        [Cooldown(120)]
        [Summary("Hazard respawn")]
        public void HazardRespawn()
        {
            // Don't trigger during transitions or anything
            if (HeroController.instance.transitionState != HeroTransitionState.WAITING_TO_TRANSITION)
                return;
            
            HeroController.instance.StartCoroutine(HeroController.instance.HazardRespawn());
        }

        [HKCommand("knockback")]
        [Cooldown(40)]
        public void Knockback(string dirStr)
        {
            CollisionSide dir = dirStr switch
            {
                "left" => CollisionSide.left,
                "right" => CollisionSide.right,
                "up" => CollisionSide.top,
                "top" => CollisionSide.top,
                "down" => CollisionSide.bottom,
                "bottom" => CollisionSide.bottom,
                _ => CollisionSide.other
            };
            
            HeroController.instance.proxyFSM.SendEvent("HeroCtrl-HeroDamaged");
            HeroController.instance.StartCoroutine(
                (IEnumerator) typeof(HeroController)
                              .GetMethod("StartRecoil", BindingFlags.NonPublic | BindingFlags.Instance)
                              ?.Invoke(HeroController.instance, new object[] { dir, true, 0 })
            );
        }

        private static void SpawnGeo(int amount, GameObject smallGeoPrefab, GameObject mediumGeoPrefab, GameObject largeGeoPrefab)
        {
            if (amount <= 0) return;

            if (smallGeoPrefab == null || mediumGeoPrefab == null || largeGeoPrefab == null)
            {
                HeroController.instance.AddGeo(amount);
                
                return;
            }

            var random = new System.Random();

            int smallNum = random.Next(0, amount / 10);
            amount -= smallNum;

            int largeNum = random.Next(amount / (25 * 2), amount / 25 + 1);
            amount -= largeNum * 25;

            int medNum = amount / 5;
            amount -= medNum * 5;

            smallNum += amount;

            FlingUtils.SpawnAndFling
            (
                new FlingUtils.Config
                {
                    Prefab = smallGeoPrefab,
                    AmountMin = smallNum,
                    AmountMax = smallNum,
                    SpeedMin = 15f,
                    SpeedMax = 30f,
                    AngleMin = 80f,
                    AngleMax = 115f
                },
                HeroController.instance.transform,
                new Vector3(0f, 0f, 0f)
            );
            
            FlingUtils.SpawnAndFling
            (
                new FlingUtils.Config
                {
                    Prefab = mediumGeoPrefab,
                    AmountMin = medNum,
                    AmountMax = medNum,
                    SpeedMin = 15f,
                    SpeedMax = 30f,
                    AngleMin = 80f,
                    AngleMax = 115f
                },
                HeroController.instance.transform,
                new Vector3(0f, 0f, 0f)
            );
            
            FlingUtils.SpawnAndFling
            (
                new FlingUtils.Config
                {
                    Prefab = largeGeoPrefab,
                    AmountMin = largeNum,
                    AmountMax = largeNum,
                    SpeedMin = 15f,
                    SpeedMax = 30f,
                    AngleMin = 80f,
                    AngleMax = 115f
                },
                HeroController.instance.transform,
                new Vector3(0f, 0f, 0f)
            );
        }
        
        [HKCommand("slaphand")]
        [Cooldown(120)]
        [Summary("Heavy blow if it was actually good.")]
        public IEnumerator SlapHand()
        {
            void SlashHit(Collider2D col, GameObject gameobject)
            {
                Vector3 dir = (col.transform.position - HeroController.instance.transform.position).normalized;

                if (!(col.gameObject.GetComponent<Rigidbody2D>() is Rigidbody2D rb2d)) return;
                
                rb2d.velocity = 40 * dir;
                rb2d.drag = 6;
            }
            
            ModHooks.SlashHitHook += SlashHit;

            yield return new WaitForSeconds(60f);
            
            ModHooks.SlashHitHook -= SlashHit;
        }

        [HKCommand("toggle")]
        [Summary("Toggles an ability for 45 seconds. Options: [dash, superdash, claw, wings, nail, tear, dnail]")]
        [Cooldown(60 * 4)]
        public IEnumerator ToggleAbility(string ability)
        {
            const float time = 45;

            PlayerData pd = PlayerData.instance;

            switch (ability)
            {
                case "dash":
                    yield return PlayerDataUtil.FakeSet(nameof(PlayerData.canDash), pd.canDash ^ true, time);
                    break;
                case "superdash":
                    yield return PlayerDataUtil.FakeSet(nameof(PlayerData.hasSuperDash), pd.hasSuperDash ^ true, time);
                    break;
                case "claw":
                    yield return PlayerDataUtil.FakeSet(nameof(PlayerData.hasWalljump), pd.hasWalljump ^ true, time);
                    break;
                case "wings":
                    yield return PlayerDataUtil.FakeSet(nameof(PlayerData.hasDoubleJump), pd.hasDoubleJump ^ true, time);
                    break;
                case "tear":
                    yield return PlayerDataUtil.FakeSet(nameof(PlayerData.hasAcidArmour), pd.hasAcidArmour ^ true, time);
                    break;
                case "dnail":
                    yield return PlayerDataUtil.FakeSet(nameof(PlayerData.hasDreamNail), pd.hasDreamNail ^ true, time);
                    break;
                case "nail":
                    Mirror.SetField(HeroController.instance, "attack_cooldown", 15f);
                    break;
            }
        }

        [HKCommand("doubledamage")]
        [Summary("Makes the player take double damage.")]
        [Cooldown(60)]
        public IEnumerator DoubleDamage()
        {
            static int InstanceOnTakeDamageHook(ref int hazardtype, int damage) => damage * 2;
            ModHooks.TakeDamageHook += InstanceOnTakeDamageHook;
            yield return new WaitForSecondsRealtime(30);
            ModHooks.TakeDamageHook -= InstanceOnTakeDamageHook;
        }
    }
}