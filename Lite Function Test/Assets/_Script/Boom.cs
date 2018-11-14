using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

//炸裂
public class Boom : MonoBehaviour
{
    [SerializeField] Ease ease = Ease.Linear;
    [SerializeField] float fixTime = 1;
    [SerializeField] PhysicMaterial phyMat;
    Transform[] childs;
    Vector3[] initialPos;
    Vector3[] initialRot;
    Rigidbody[] rigidbodys;
    Tween[] tweens;
    bool boom = true;

    void GetChildTransform(ref List <Transform> childsList,Transform parent)
    {
        int l = parent.childCount;
        if (l == 0)
            return;

        Transform[] childs = new Transform[l];
        foreach (Transform t in parent)
        {
            childsList.Add(t);
            GetChildTransform(ref childsList, t);
        }
    }
    void DoBoom()
    {
        StartCoroutine(DoBoooom());
    }
    IEnumerator DoBoooom()
    {
        yield return new WaitForEndOfFrame();

        float f = 0;    //阻力
        float startSpeed = 20;   //速度
        float x = 1f;  //膨胀系数
        float xMax = 3;
        while (x < xMax*0.95)
        {
            yield return new WaitForFixedUpdate();
            x += Time.deltaTime * (startSpeed - f);
            f = startSpeed * (x / xMax);
            for (int i=0;i <childs .Length;i++)
            {
                childs[i].localPosition = initialPos[i] * x;
            }
        }
        yield return new WaitForSeconds(0.5f);
        UseGravity();
    }

    void UseGravity(Rigidbody rigidbody =null)
    {
        if (rigidbody !=null)
        {
            rigidbody.useGravity = true;
            rigidbody.isKinematic = false;
            return;
        }
        foreach (Rigidbody r in rigidbodys)
        {
            r.useGravity = true;
            r.isKinematic = false;
        }

        Invoke("Fix", 3);
    }

    void Fix()
    {
        for (int i=0;i <childs .Length;i++)
        {
            rigidbodys[i].useGravity = false;
            rigidbodys[i].isKinematic = true;

            Vector3[] wayPoints = new Vector3[2] { childs[i].localPosition, initialPos[i] };
            childs[i].DOLocalPath(wayPoints, fixTime)
                .SetOptions(false)
                .SetEase(ease);
            childs[i].DOLocalRotate(initialRot[i], fixTime)
                .SetEase(ease);
        }
    }
    
    void Start()
    {
        List<Transform> childsList = new List<Transform>();
        GetChildTransform(ref childsList, transform);
        childs = childsList.ToArray();

        initialPos = new Vector3[childs.Length];
        initialRot = new Vector3[childs.Length];
        rigidbodys = new Rigidbody[childs.Length];

        for (int i = 0; i < childs.Length; i++)
        {
            initialPos[i] = childs[i].localPosition;
            initialRot[i] = childs[i].localEulerAngles;
            Rigidbody r = childs[i].gameObject.GetComponent<Rigidbody>();
            if (r == null)
                r = childs[i].gameObject.AddComponent<Rigidbody>();
            rigidbodys[i] = r;
            r.useGravity = false;
            r.isKinematic = true;
            r.gameObject.AddComponent<BoxCollider>();
        }
    }

    private void Update()
    {
        if (Input .GetKeyDown (KeyCode .Space))
        {
            DoBoom();
            boom = !boom;
        }
    }


}
