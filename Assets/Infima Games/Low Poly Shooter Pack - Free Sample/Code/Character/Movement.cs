// Copyright 2021, Infima Games. All Rights Reserved.

using System.Linq;
using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class Movement : MovementBehaviour
    {
        #region FIELDS SERIALIZED

        [Header("Audio Clips")]
        [Tooltip("The audio clip that is played while walking.")]
        [SerializeField]
        private AudioClip audioClipWalking;

        [Tooltip("The audio clip that is played while running.")]
        [SerializeField]
        private AudioClip audioClipRunning;

        [Header("Speeds")]
        [SerializeField]
        private float speedWalking = 5.0f;

        [Tooltip("How fast the player moves while running."), SerializeField]
        private float speedRunning = 9.0f;

        #endregion

        #region PROPERTIES
        private Vector3 Velocity
        {
            get => rigidBody.linearVelocity;
            set => rigidBody.linearVelocity = value;
        }
        #endregion

        #region FIELDS
        private Rigidbody rigidBody;
        private CapsuleCollider capsule;
        private AudioSource audioSource;

        private bool grounded;

        private CharacterBehaviour playerCharacter;
        private WeaponBehaviour equippedWeapon;

        private readonly RaycastHit[] groundHits = new RaycastHit[8];
        #endregion

        #region UNITY FUNCTIONS
        protected override void Awake()
        {
            // ServiceLocator가 준비되지 않은 경우를 방어
            var gameModeService = ServiceLocator.Current?.Get<IGameModeService>();
            if (gameModeService == null)
            {
                Debug.LogWarning("[Movement] GameModeService not found! Disabling Movement on " + gameObject.name);
                enabled = false;
                return;
            }

            playerCharacter = gameModeService.GetPlayerCharacter();

            if (playerCharacter == null)
                Debug.LogWarning("[Movement] PlayerCharacter not found! Movement will remain idle on " + gameObject.name);
        }

        protected override void Start()
        {
            rigidBody = GetComponent<Rigidbody>();
            rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
            capsule = GetComponent<CapsuleCollider>();

            // Audio Source Setup.
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.clip = audioClipWalking;
            audioSource.loop = true;
        }

        private void OnCollisionStay()
        {
            if (capsule == null)
                return;

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
            if (playerCharacter == null)
                return;

            MoveCharacter();
            grounded = false;
        }

        protected override void Update()
        {
            if (playerCharacter == null)
                return;

            var inventory = playerCharacter.GetInventory();
            if (inventory == null)
                return;

            equippedWeapon = inventory.GetEquipped();

            PlayFootstepSounds();
        }
        #endregion

        #region METHODS
        private void MoveCharacter()
        {
            if (playerCharacter == null)
                return;

            Vector2 frameInput = playerCharacter.GetInputMovement();
            var movement = new Vector3(frameInput.x, 0.0f, frameInput.y);

            movement *= playerCharacter.IsRunning() ? speedRunning : speedWalking;
            movement = transform.TransformDirection(movement);

            Velocity = new Vector3(movement.x, 0.0f, movement.z);
        }

        private void PlayFootstepSounds()
        {
            if (audioSource == null || rigidBody == null)
                return;

            if (grounded && rigidBody.linearVelocity.sqrMagnitude > 0.1f)
            {
                audioSource.clip = playerCharacter != null && playerCharacter.IsRunning()
                    ? audioClipRunning
                    : audioClipWalking;

                if (!audioSource.isPlaying)
                    audioSource.Play();
            }
            else if (audioSource.isPlaying)
            {
                audioSource.Pause();
            }
        }
        #endregion
    }
}
