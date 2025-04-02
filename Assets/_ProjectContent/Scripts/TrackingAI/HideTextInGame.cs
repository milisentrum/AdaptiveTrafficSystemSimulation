using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class HideTextInGame : MonoBehaviour
{
    private TextMesh textMesh;

    // Start is called before the first frame update
    void Start()
    {
        textMesh = GetComponent<TextMesh>();

        if (textMesh != null)
        {
            textMesh.gameObject.SetActive(false); 
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (textMesh != null)
        {
            Handles.Label(transform.position, textMesh.text);
        }
    }
#endif

}
