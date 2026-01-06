using System.Linq;
using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class Movement : MovementBehaviour
    {
        [Header("Audio Clips")]
        [SerializeField] private AudioClip audioClipWalking;
        [SerializeField] private AudioClip audioClipRunning;

        [Header("Speeds")]
        [SerializeField] private float speedWalking = 5.0f;
        [SerializeField] private float speedRunning = 9.0f;

        [Header("Jump")]
        [SerializeField] private float jumpVelocity = 6.0f;

        private Vector3 Velocity
        {
            get => rigidBody.linearVelocity;
            set => rigidBody.linearVelocity = value;
        }

        private Rigidbody rigidBody;
        private CapsuleCollider capsule;
        private AudioSource audioSource;

        private bool grounded;

        private CharacterBehaviour playerCharacter;
        private WeaponBehaviour equippedWeapon;

        private readonly RaycastHit[] groundHits = new RaycastHit[8];

        protected override void Awake()
        {
            playerCharacter = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
        }

        protected override void Start()
        {
            rigidBody = GetComponent<Rigidbody>();

            // ✅ 추천: X/Z만 고정
            rigidBody.constraints = RigidbodyConstraints.FreezeRotation;


            capsule = GetComponent<CapsuleCollider>();

            audioSource = GetComponent<AudioSource>();
            audioSource.clip = audioClipWalking;
            audioSource.loop = true;
        }

        private void OnCollisionStay()
        {
            Bounds bounds = capsule.bounds;
            Vector3 extents = bounds.extents;
            float radius = extents.x - 0.01f;

            Physics.SphereCastNonAlloc(bounds.center, radius, Vector3.down,
                groundHits, extents.y - radius * 0.5f, ~0, QueryTriggerInteraction.Ignore);

            if (!groundHits.Any(hit => hit.collider != null && hit.collider != capsule))
                return;

            for (var i = 0; i < groundHits.Length; i++)
                groundHits[i] = new RaycastHit();

            grounded = true;
        }

        protected override void FixedUpdate()
        {
            MoveCharacter();
            grounded = false;
        }

        protected override void Update()
        {
            equippedWeapon = playerCharacter.GetInventory().GetEquipped();

            PlayFootstepSounds();

            // ✅ 점프 (스페이스)
            if (grounded && Input.GetKeyDown(KeyCode.Space))
            {
                var v = Velocity;
                if (v.y < 0) v.y = 0;
                v.y = jumpVelocity;
                Velocity = v;
            }
        }

        private void MoveCharacter()
        {
            Vector2 frameInput = playerCharacter.GetInputMovement();
            var movement = new Vector3(frameInput.x, 0.0f, frameInput.y);

            movement *= playerCharacter.IsRunning() ? speedRunning : speedWalking;
            movement = transform.TransformDirection(movement);

            // ✅ 핵심: Y를 0으로 고정하지 말고 유지!
            Velocity = new Vector3(movement.x, Velocity.y, movement.z);
        }

        private void PlayFootstepSounds()
        {
            if (grounded && rigidBody.linearVelocity.sqrMagnitude > 0.1f)
            {
                audioSource.clip = playerCharacter.IsRunning() ? audioClipRunning : audioClipWalking;
                if (!audioSource.isPlaying)
                    audioSource.Play();
            }
            else if (audioSource.isPlaying)
                audioSource.Pause();
        }
    }
}
