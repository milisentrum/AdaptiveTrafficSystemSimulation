using UnityDevKit.Interactables;
using UnityEngine;

namespace UnityDevKit.Player.Extensions
{
    public class PlayerInteraction : PlayerExtension
    {
        [Header("Interaction settings")] 
        [SerializeField] private float distance = 3f;

        [SerializeField] private LayerMask layerMask;

        private Camera mainCamera;
        private Transform mainCameraTransform;

        private InteractionBase interaction;

        private const int InteractingFrameDelay = 10;

        protected override void Awake()
        {
            base.Awake();
            interaction = GetComponent<InteractionBase>();
        }

        protected override void Start()
        {
            base.Start();
            mainCamera = Camera.main;
            mainCameraTransform = mainCamera.transform;
        }

        protected override void Update()
        {
            base.Update();
            if (Time.frameCount % InteractingFrameDelay == 0)
            {
                Searching();
            }

            if (Input.GetMouseButtonDown(0))
            {
                interaction.Interact();
            }

            if (Input.GetMouseButtonUp(0))
            {
                interaction.AfterInteract();
            }
        }

        private void Searching()
        {
            var ray = new Ray(mainCameraTransform.position, mainCameraTransform.forward);
            Debug.DrawRay(ray.origin, ray.direction * distance, Color.magenta);

            if (Physics.Raycast(ray, out var hitInfo, distance, layerMask))
            {
                var hitObject = hitInfo.collider.GetComponent<InteractableBase>();
                if (hitObject != null)
                {
                    interaction.FocusObject(hitObject);
                    // TODO;
                    return;
                }
            }

            interaction.LoseFocus();
        }
    }
}