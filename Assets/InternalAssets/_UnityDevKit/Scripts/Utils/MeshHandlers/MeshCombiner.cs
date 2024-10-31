using MyBox;
using UnityEngine;

namespace UnityDevKit.Utils.MeshHandlers
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class MeshCombiner : MonoBehaviour
    {
        private void BaseCombine()
        {
            var meshFilters = GetComponentsInChildren<MeshFilter>();
            var combine = new CombineInstance[meshFilters.Length];

            var i = 1;
            var myTransform = transform.worldToLocalMatrix;
            while (i < meshFilters.Length)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;

                combine[i].transform = myTransform * meshFilters[i].transform.localToWorldMatrix;
                meshFilters[i].gameObject.SetActive(false);

                i++;
            }

            var meshFilter = transform.GetComponent<MeshFilter>();
            meshFilter.sharedMesh = new Mesh();
            // Instantiating mesh due to calling MeshFilter.mesh during edit mode. This will leak meshes. Please use MeshFilter.sharedMesh instead.
            meshFilter.sharedMesh.CombineMeshes(combine); 
            transform.gameObject.SetActive(true);

            var meshCollider = GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                meshCollider.sharedMesh = transform.GetComponent<MeshFilter>().sharedMesh;
            }
        }
#if UNITY_EDITOR
        [ButtonMethod]
        public string Combine()
        {
            BaseCombine();

            return $"Mesh has been combined";
        }
#endif
    }
}