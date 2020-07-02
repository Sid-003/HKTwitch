using System.Collections;
using UnityEngine;

namespace HollowTwitch.Components
{
    public class BetterSpawnJarControl : MonoBehaviour
    {
        public ParticleSystem ReadyDust;
        public ParticleSystem Trail;
        public ParticleSystem ParticleBreakSouth;
        public ParticleSystem ParticleBreak;

        public GameObject StrikeNailReaction;

        public GameObject EnemyPrefab;
        public int EnemyHP;

        public AudioClip Clip;

        private AudioSource _as;
        private CircleCollider2D _col;
        private Rigidbody2D _rb2d;
        private SpriteRenderer _sprite;

        private void Awake()
        {
            _col = GetComponent<CircleCollider2D>();
            _rb2d = GetComponent<Rigidbody2D>();
            _sprite = GetComponent<SpriteRenderer>();
            _as = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            _col.enabled = false;
            _sprite.enabled = false;
            
            StartCoroutine(Spawn());
        }

        private IEnumerator Spawn()
        {
            transform.SetPositionZ(0.01f);

            ReadyDust.Play();

            yield return new WaitForSeconds(0.5f);

            _col.enabled = true;

            _rb2d.velocity = new Vector2(0f, -25f);
            _rb2d.angularVelocity = Random.Range(0, 2) <= 0 ? 300 : -300;

            ReadyDust.Stop();
            Trail.Play();
            
            _sprite.enabled = true;
        }

        private void OnCollisionEnter2D()
        {
            GameCameras.instance.cameraShakeFSM.SendEvent("EnemyKillShake");

            Trail.Stop();
            ParticleBreakSouth.Play();
            ParticleBreak.Play();

            StrikeNailReaction.Spawn(transform.position);

            _col.enabled = false;

            _rb2d.velocity = Vector2.zero;
            _rb2d.angularVelocity = 0f;

            _sprite.enabled = false;

            _as.volume = GameManager.instance.gameSettings.soundVolume / 100f;
            _as.PlayOneShot(Clip);

            Vector3 pos = transform.position;
            
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector3.down, 20, 1 << 8);

            if (hit)
            {
                pos = hit.point;
                pos += new Vector3(0, 0.6f);
            }

            GameObject go = Instantiate(EnemyPrefab, pos, Quaternion.identity);
            
            go.SetActive(true);

            go.GetComponent<HealthManager>().hp = EnemyHP;

            go.tag = "Boss";
        }
    }
}