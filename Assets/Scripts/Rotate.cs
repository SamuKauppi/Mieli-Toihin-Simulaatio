using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    //pyörittää objektia ympäri, aina kun laitetaan päälle
    public float rotatespeed;

    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(RotateAround());
    }

    IEnumerator RotateAround()
    {
        while (true)
        {
            LeanTween.rotateAround(gameObject, Vector3.up, 360f, rotatespeed);
            yield return new WaitForSecondsRealtime(rotatespeed);
        }
    }
}
