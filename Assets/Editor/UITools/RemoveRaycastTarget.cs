using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;

public class RemoveRaycastTarget : MonoBehaviour
{

    [MenuItem( "GameObject/UI/Image(NoRaycastTarget)" )]
    private static void CreateImage()
    {
        if ( Selection.activeTransform )
        {
            if ( Selection.activeTransform.GetComponentInParent<Canvas>() )
            {
                GameObject go = new GameObject( "Image", typeof( Image ) );
                go.GetComponent<Image>().raycastTarget = false;
                go.transform.SetParent( Selection.activeTransform );
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = Vector3.one;
            }
        }
    }

    [MenuItem( "GameObject/UI/Text(NoRaycastTarget NoRichText)" )]
    private static void CreateText()
    {
        if ( Selection.activeTransform )
        {
            if ( Selection.activeTransform.GetComponentInParent<Canvas>() )
            {
                GameObject go = new GameObject( "Text", typeof( Text ) );
                go.GetComponent<Text>().raycastTarget = false;
                go.GetComponent<Text>().supportRichText = false;
                go.transform.SetParent( Selection.activeTransform );
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = Vector3.one;
            }
        }
    }
}
