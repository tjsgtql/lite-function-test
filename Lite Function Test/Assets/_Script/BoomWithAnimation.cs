using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomWithAnimation : MonoBehaviour
{
	Animation animation;
	Transform []childs;

    void Start()
    {
		animation =GetComponent <Animation>();
		childs =transform .GetComponentsInChildren <Transform >();

    }

	public void BoomAnimator(){
		animation .Play ();//("CDZ-boom");

	}
}
