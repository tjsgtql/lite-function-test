using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public struct Shatter
{
    public Transform transform;
    public Rigidbody rigidbody;
    public Tween boomTween;
    public Tween fixTweenPos;
    public Tweener fixTweenRot;
    public Vector3 initialPos;
    public Vector3 initialRot;
}

//炸裂
public class Boom : MonoBehaviour
{
    [SerializeField] Ease ease = Ease.Linear;
    [SerializeField] float fixTime = 1;

    List<Shatter> shatters = new List<Shatter>();

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
        #region 老方法
        //StartCoroutine(DoBoooom());
        //for (int i = 0; i < childs.Length; i++)
        //{
        //    Vector3[] wayPoints = new Vector3[2] { childs[i].localPosition, initialPos[i] * 3 };
        //    childs[i].DOLocalPath(wayPoints, fixTime)
        //        .SetOptions(false)
        //        .SetEase(ease);
        //}
        //Invoke("UseGravity", 3);
        #endregion

        #region 新方法
        for (int i=0;i <shatters .Count;i++)
        {
            Shatter shatter = shatters[i];
            if (shatter .boomTween !=null)
            {
                shatter.boomTween.Restart();
            }else
            {
                Vector3 mov = shatter.initialPos * 3;
                shatter.boomTween = shatter.transform.DOLocalMove(mov, 1)
                    .SetAutoKill(false)
                    .SetEase(ease);
            }
            //因为shatter是struct,所以最后要写回去
            shatters[i] = shatter;
        }
        Invoke("UseGravity", 3);
        #endregion
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
    
    //TODO:启动物理效果之后应该把所有物体的父节点都置为空，当Fix的时候应该恢复原先的层级关系
    void UseGravity()
    {
        #region 旧方法
        //foreach (Rigidbody r in rigidbodys)
        //{
        //    r.useGravity = true;
        //    r.isKinematic = false;
        //}
        #endregion

        #region 新方法
        var s = shatters.GetEnumerator();
        while (s .MoveNext())
        {
            s.Current.rigidbody.useGravity = true;
            s.Current.rigidbody.isKinematic = false;
        }
        #endregion

        Invoke("Fix", 3);
    }

    void Fix()
    {
        #region 旧方法
        //for (int i=0;i <childs .Length;i++)
        //{
        //    rigidbodys[i].useGravity = false;
        //    rigidbodys[i].isKinematic = true;

        //    Vector3[] wayPoints = new Vector3[2] { childs[i].localPosition, initialPos[i] };
        //    childs[i].DOLocalPath(wayPoints, fixTime)
        //        .SetOptions(false)
        //        .SetEase(ease);
        //    childs[i].DOLocalRotate(initialRot[i], fixTime)
        //        .SetEase(ease);
        //}
        #endregion
        
        for (int i=0;i <shatters .Count;i++)
        {
            ///因为boom后启动了物理效果，每次位置都不一样，所以fixTweenPos和fixTweenPos每次都需要重新设置路径
            ///不知道这样还有没有保存tween变量的意义 >>> 测完了，没意义。每次fixTweenPos = shatter.transform.DOLocalMove操作之后都会生成新的tween。对于这种还是执行完了自动销毁的好
            Shatter shatter = shatters[i];
            Tween tweenPos = shatter.transform.DOLocalMove(shatter.initialPos, 1)
                .SetEase(ease);
            Tween tweenRot = shatter.transform.DOLocalRotate(shatter.initialRot, 1)
                .SetEase(ease);
            //关闭物理效果
            shatter.rigidbody.useGravity = false;
            shatter.rigidbody.isKinematic = true;

            shatters[i] = shatter;
        }
    }
    
    void Start()
    {
        List<Transform> childsList = new List<Transform>();
        
        #region 老方法
        //GetChildTransform(ref childsList, transform);
        //childs = childsList.ToArray();

        //initialPos = new Vector3[childs.Length];
        //initialRot = new Vector3[childs.Length];
        //rigidbodys = new Rigidbody[childs.Length];

        //for (int i = 0; i < childs.Length; i++)
        //{
        //    initialPos[i] = childs[i].localPosition;
        //    initialRot[i] = childs[i].localEulerAngles;
        //    Rigidbody r = childs[i].gameObject.GetComponent<Rigidbody>();
        //    if (r == null)
        //        r = childs[i].gameObject.AddComponent<Rigidbody>();
        //    rigidbodys[i] = r;
        //    r.useGravity = false;
        //    r.isKinematic = true;
        //    r.gameObject.AddComponent<BoxCollider>();
        //}
        #endregion


        #region 新方法
        Transform []childsTransform = transform.GetComponentsInChildren<Transform>();
        foreach (Transform t in childsTransform)
        {
            ///如果这个物体时可见的，
            ///只处理可见的物体，空物体就不用boom了
            if (t .GetComponent<MeshRenderer>())
            {
                if (!t .gameObject .GetComponent <Collider >())
                    t.gameObject.AddComponent<BoxCollider>();

                Shatter shatter = new Shatter();
                shatter.transform = t;
                shatter.rigidbody = t.gameObject.GetComponent<Rigidbody>() ? t.gameObject.GetComponent<Rigidbody>() : t.gameObject.AddComponent<Rigidbody>();
                shatter.rigidbody.useGravity = false;
                shatter.rigidbody.isKinematic = true;
                shatter.initialPos = t.localPosition;
                shatter.initialRot = t.localEulerAngles;

                shatters.Add(shatter);
            }
        }
        #endregion
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
