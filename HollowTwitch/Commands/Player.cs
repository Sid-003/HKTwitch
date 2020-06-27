using System.Collections;
using System.Diagnostics.CodeAnalysis;
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
using Random = UnityEngine.Random;
using UObject = UnityEngine.Object;

namespace HollowTwitch.Commands
{
    public class Player
    {
        // Most (all) of the commands stolen from Chaos Mod by Seanpr
        private GameObject _maggot;

        public Player()
        {
            On.HeroController.Move += InvertControls;
            On.NailSlash.StartSlash += ChangeNailScale;
            On.HeroController.CheckTouchingGround += OnTouchingGround;
            ModHooks.Instance.DashVectorHook += OnDashVector;

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
        [Summary("Enables darkness for some time.")]
        [Cooldown(60)]
        public IEnumerator Blind()
        {
            DarknessHelper.Darken();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoad;

            yield return new WaitForSecondsRealtime(30);

            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoad;
            DarknessHelper.Lighten();
        }

        [HKCommand("nopogo")]
        [Summary("Disables pogo.")]
        [Cooldown(60)]
        public IEnumerator PogoKnockback()
        {
            void NoBounce(On.HeroController.orig_Bounce orig, HeroController self) { }

            On.HeroController.Bounce += NoBounce;

            yield return new WaitForSecondsRealtime(30);

            On.HeroController.Bounce -= NoBounce;
        }

        [HKCommand("conveyor")]
        [Summary("Conveyor storage.")]
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

            ModHooks.Instance.TakeHealthHook += TakeHealth;
            ModHooks.Instance.HitInstanceHook += HitInstance;

            yield return new WaitForSeconds(15f);

            ModHooks.Instance.TakeHealthHook -= TakeHealth;
            ModHooks.Instance.HitInstanceHook -= HitInstance;
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
            // If they're already limited I don't want to just free them
            bool orig_val = PlayerData.instance.soulLimited;

            PlayerData.instance.soulLimited = true;

            yield return new SaveDefer(30f, pd => { pd.soulLimited = orig_val; });
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
            float speed = Random.Range(-10f, 10f);

            float prev_s = HeroController.instance.conveyorSpeed;

            HeroController.instance.cState.inConveyorZone = true;
            HeroController.instance.conveyorSpeed = speed;

            yield return new WaitForSecondsRealtime(30);

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
        [Summary("Change dash length.")]
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
        [Summary("Change dash vector.")]
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

            ModHooks.Instance.DashVectorHook += VectorHook;

            yield return new WaitForSecondsRealtime(30f);

            ModHooks.Instance.DashVectorHook -= VectorHook;
        }

        [HKCommand("triplejump")]
        public IEnumerator TripleJump()
        {
            bool triple_jump = false;

            void Triple(On.HeroController.orig_DoDoubleJump orig, HeroController self)
            {
                orig(self);

                if (!triple_jump)
                {
                    ReflectionHelper.SetAttr(self, "doubleJumped", false);

                    triple_jump = true;
                }
                else
                {
                    triple_jump = false;
                }
            }

            On.HeroController.DoDoubleJump += Triple;

            yield return new WaitForSeconds(30);

            On.HeroController.DoDoubleJump -= Triple;
        }

        private static void OnSceneLoad(Scene arg0, LoadSceneMode arg1)
        {
            if (HeroController.instance == null) return;

            DarknessHelper.Darken();
        }

        [HKCommand("overflow")]
        [Cooldown(40)]
        public void OverflowSoul()
        {
            HeroController.instance.AddMPChargeSpa(99 * 2);

            PlayerData.instance.MPCharge += 99;
        }

        [HKCommand("timescale")]
        [Summary("Changes the timescale of the game for the time specified.")]
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


        private bool _inverted;
        private bool _slippery;
        private float _lastMoveDir;

        [HKCommand("invertcontrols")]
        [Summary("Inverts the move direction of the player.")]
        [Cooldown(60)]
        public IEnumerator InvertControls()
        {
            _inverted = true;
            yield return new WaitForSecondsRealtime(60);
            _inverted = false;
        }

        [HKCommand("slippery")]
        [Summary("Makes the floor have no friction at all. Lasts for 60 seconds.")]
        public IEnumerator Slippery()
        {
            _slippery = true;
            yield return new WaitForSecondsRealtime(60);
            _slippery = false;
        }

        private void InvertControls(On.HeroController.orig_Move orig, HeroController self, float move_direction)
        {
            if (HeroController.instance.transitionState != HeroTransitionState.WAITING_TO_TRANSITION)
            {
                orig(self, move_direction);
                return;
            }

            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (move_direction == 0f && _slippery)
                move_direction = _lastMoveDir == 0f ? 1f : _lastMoveDir;
            // ReSharper restore CompareOfFloatsByEqualityOperator

            if (_inverted)
                move_direction = -move_direction;
            orig(self, move_direction);

            _lastMoveDir = move_direction;
        }

        private Vector2 OnDashVector(Vector2 change)
        {
            if (_inverted)
                return -1 * change;
            return change;
        }

        private float _nailScale = 1f;

        [HKCommand("nailscale")]
        [Summary("Makes the nail huge or tiny.")]
        public IEnumerator NailScale([EnsureFloat(.3f, 5f)] float nailScale)
        {
            _nailScale = nailScale;
            yield return new WaitForSecondsRealtime(30f);
            _nailScale = 1f;
        }

        private void ChangeNailScale(On.NailSlash.orig_StartSlash orig, NailSlash self)
        {
            orig(self);
            self.transform.localScale *= _nailScale;
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

        private bool _floorislava;

        [HKCommand("floorislava")]
        [Summary("Makes the floor do damage.")]
        [Cooldown(60 * 2)]
        public IEnumerator FloorIsLava([EnsureFloat(10, 60)] float seconds)
        {
            // Stolen from zaliant
            _floorislava = true;

            yield return new WaitForSecondsRealtime(seconds);

            _floorislava = false;
        }

        private bool OnTouchingGround(On.HeroController.orig_CheckTouchingGround orig, HeroController self)
        {
            bool touching = orig(self);

            if (touching && _floorislava && !GameManager.instance.IsInSceneTransition)
                self.TakeDamage(null, CollisionSide.bottom, 1, 69420);

            return touching;
        }

        [HKCommand("float")]
        [Cooldown(180)]
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
            go.SetActive(true);

            var renderer = HeroController.instance.GetComponent<MeshRenderer>();
            renderer.enabled = false;

            yield return new WaitForSecondsRealtime(60 * 2);

            UObject.DestroyImmediate(go);

            renderer.enabled = true;
        }

        [HKCommand("toggle")]
        [Summary("Toggles an ability for 45 seconds.")]
        [Cooldown(60 * 4)]
        public IEnumerator ToggleAbility(string ability)
        {
            const float time = 45;

            switch (ability)
            {
                case "dash":
                    PlayerData.instance.canDash ^= true;
                    yield return new SaveDefer(time, pd => pd.canDash ^= true);
                    break;
                case "superdash":
                    PlayerData.instance.hasSuperDash ^= true;
                    yield return new SaveDefer(time, pd => pd.hasSuperDash ^= true);
                    break;
                case "claw":
                    PlayerData.instance.hasWalljump ^= true;
                    yield return new SaveDefer(time, pd => pd.hasWalljump ^= true);
                    break;
                case "wings":
                    PlayerData.instance.hasDoubleJump ^= true;
                    yield return new SaveDefer(time, pd => pd.hasDoubleJump ^= true);
                    break;
                case "nail":
                    ReflectionHelper.SetAttr(HeroController.instance, "attack_cooldown", 15f);
                    break;
            }
        }

        [HKCommand("doubledamage")]
        [Summary("Makes the player take double damage.")]
        [Cooldown(60)]
        public IEnumerator DoubleDamage()
        {
            static int InstanceOnTakeDamageHook(ref int hazardtype, int damage) => damage * 2;
            ModHooks.Instance.TakeDamageHook += InstanceOnTakeDamageHook;
            yield return new WaitForSecondsRealtime(30);
            ModHooks.Instance.TakeDamageHook -= InstanceOnTakeDamageHook;
        }
    }
}