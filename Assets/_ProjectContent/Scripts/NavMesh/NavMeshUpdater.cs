using UnityDevKit.Patterns;
using UnityEngine;
using UnityEngine.AI;

namespace AdaptiveTrafficSystem.NavMesh
{
    [RequireComponent(typeof(NavMeshSurface))]
    public class NavMeshUpdater : Singleton<NavMeshUpdater>
    {
        private NavMeshSurface _surface;

        public override void Awake() 
        {
            base.Awake();
            _surface = GetComponent<NavMeshSurface>();
        }

        public void UpdateNavMesh()
        {
            _surface.UpdateNavMesh(_surface.navMeshData);	
        }
    }
}