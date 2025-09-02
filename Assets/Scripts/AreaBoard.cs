using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaBoard : MonoBehaviour
{
    [SerializeField]
    private int idArea;
    /*  [SerializeField]
      private Collider2D colliderArea;

      public Collider2D ColliderArea { get => colliderArea; set => colliderArea = value; }*/
    public int IdArea { get => idArea; set => idArea = value; }
    public List<CaseBoard> CasesOnArea { get => casesOnArea; set => casesOnArea = value; }

    public bool IsActive { get => isActive; set => isActive = value; }
    public GameObject CaseArea { get => caseArea; set => caseArea = value; }
    public SphereCollider ColliderSphere { get => colliderSphere; set => colliderSphere = value; }
    public Transform Entry { get => entry; set => entry = value; }
    public Transform Exit { get => exit; set => exit = value; }

    [SerializeField]
    private List<CaseBoard> casesOnArea;


    private GameObject GetObject(int ringNumber)//0 base, 1 2 or 0
    {
        if(IsActive)
        {
            switch (ringNumber)
            {
                case 0:
                    switch (idArea)
                    {
                        case 1:
                            return mainPlateformType1;
                            
                        case 2:
                            return mainPlateformType2;
                        case 3:
                            return mainPlateformType3;
                        case 4:
                            return mainPlateformType4;
                        case 5:
                            return mainPlateformType5;
                    }
                    break;
                case 1:
                    switch (idArea)
                    {
                        case 1:
                            return ring1Type1;

                        case 2:
                            return ring1Type2;
                        case 3:
                            return ring1Type3;
                        case 4:
                            return ring1Type4;
                        case 5:
                            return ring1Type5;
                    }
                    break;
                case 2:
                    switch (idArea)
                    {
                        case 1:
                            return ring2Type1;

                        case 2:
                            return ring2Type2;
                        case 3:
                            return ring2Type3;
                        case 4:
                            return ring2Type4;
                        case 5:
                            return ring2Type5;
                    }
                    break;
                case 3:
                    switch (idArea)
                    {
                        case 1:
                            return ring3Type1;

                        case 2:
                            return ring3Type2;
                        case 3:
                            return ring3Type3;
                        case 4:
                            return ring3Type4;
                        case 5:
                            return ring3Type5;
                    }
                    break;

            }
        }
        else
        {
            switch (ringNumber)
            {
                case 0:
                    return mainPlateformBase;
                case 1:
                    return ring1Base;
                case 2:
                    return ring2Base;
                case 3:
                    return ring3Base;

            }
        }

        return null;
    }

    public GameObject MainPlateform
    {
        get => GetObject(0);
    }

    public GameObject Ring1
    {
        get => GetObject(1);
    }

    public GameObject Ring2
    {
        get => GetObject(2);
    }

    public GameObject Ring3
    {
        get => GetObject(3);
    }


    [SerializeField]
     private GameObject mainPlateformBase;

     [SerializeField]
     private GameObject ring1Base;

     [SerializeField]
     private GameObject ring2Base;

     [SerializeField]
     private GameObject ring3Base;



    [SerializeField]
    private GameObject mainPlateformType1;

    [SerializeField]
    private GameObject ring1Type1;

    [SerializeField]
    private GameObject ring2Type1;

    [SerializeField]
    private GameObject ring3Type1;




    [SerializeField]
    private GameObject mainPlateformType2;

    [SerializeField]
    private GameObject ring1Type2;

    [SerializeField]
    private GameObject ring2Type2;

    [SerializeField]
    private GameObject ring3Type2;




    [SerializeField]
    private GameObject mainPlateformType3;

    [SerializeField]
    private GameObject ring1Type3;

    [SerializeField]
    private GameObject ring2Type3;

    [SerializeField]
    private GameObject ring3Type3;




    [SerializeField]
    private GameObject mainPlateformType4;

    [SerializeField]
    private GameObject ring1Type4;

    [SerializeField]
    private GameObject ring2Type4;

    [SerializeField]
    private GameObject ring3Type4;

    [SerializeField]
    private GameObject mainPlateformType5;

    [SerializeField]
    private GameObject ring1Type5;

    [SerializeField]
    private GameObject ring2Type5;

    [SerializeField]
    private GameObject ring3Type5;




    [SerializeField]
    private GameObject typeBase;

       [SerializeField]
    private GameObject type1;

       [SerializeField]
    private GameObject type2;

       [SerializeField]
    private GameObject type3;

       [SerializeField]
    private GameObject type4;

    [SerializeField]
    private GameObject type5;

    private void SetObjectById()
    {
        // base stays off
        switch (idArea)
        {
            case 1:
                typeBase.SetActive(false);
                type1.SetActive(true);
                break;

            case 2:
                typeBase.SetActive(false);
                type2.SetActive(true);
                break;
            case 3:
                typeBase.SetActive(false);
                type3.SetActive(true);
                break;
            case 4:
                typeBase.SetActive(false);
                type4.SetActive(true);
                break;
            case 5:
                typeBase.SetActive(false);
                type5.SetActive(true);
                break;
        }





        if (mainPlateformBase) mainPlateformBase.SetActive(false);
        if (ring1Base) ring1Base.SetActive(false);
        if (ring2Base) ring2Base.SetActive(false);
        if (ring3Base) ring3Base.SetActive(false);

            bool on1 = idArea == 1;
            bool on2 = idArea == 2;
            bool on3 = idArea == 3;
            bool on4 = idArea == 4;

            // type 1
            if (mainPlateformType1) mainPlateformType1.SetActive(on1);
            if (ring1Type1) ring1Type1.SetActive(on1);
            if (ring2Type1) ring2Type1.SetActive(on1);
            if (ring3Type1) ring3Type1.SetActive(on1);

            // type 2
            if (mainPlateformType2) mainPlateformType2.SetActive(on2);
            if (ring1Type2) ring1Type2.SetActive(on2);
            if (ring2Type2) ring2Type2.SetActive(on2);
            if (ring3Type2) ring3Type2.SetActive(on2);

            // type 3
            if (mainPlateformType3) mainPlateformType3.SetActive(on3);
            if (ring1Type3) ring1Type3.SetActive(on3);
            if (ring2Type3) ring2Type3.SetActive(on3);
            if (ring3Type3) ring3Type3.SetActive(on3);

            // type 4
            if (mainPlateformType4) mainPlateformType4.SetActive(on4);
            if (ring1Type4) ring1Type4.SetActive(on4);
            if (ring2Type4) ring2Type4.SetActive(on4);
            if (ring3Type4) ring3Type4.SetActive(on4);

        // type 5
        if (mainPlateformType5) mainPlateformType5.SetActive(on4);
        if (ring1Type5) ring1Type5.SetActive(on4);
        if (ring2Type5) ring2Type5.SetActive(on4);
        if (ring3Type5) ring3Type5.SetActive(on4);

    }





    private bool clear = false;
    public bool Clear { get => clear; set => clear = value; }

    [SerializeField]
    private SphereCollider colliderSphere;

    [SerializeField]
    private bool isActive = false;

    [SerializeField]
    private float yposActiveRing1 = 0f;
    [SerializeField]
    private float yposActiveRing2 = 0f;
    [SerializeField]
    private float yposActiveRing3 = 0f;

    [SerializeField]
    private float yposInactiveRing1 = -0f;
    [SerializeField]
    private float yposInactiveRing2 = -0.09f;
    [SerializeField]
    private float yposInactiveRing3 = -0.19f;


    [SerializeField]
    private Transform entry;
    [SerializeField]
    private Transform exit;


    [SerializeField]
    private GameObject caseArea;


    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.Linear(0, 0, 1, 1);

    private Coroutine ring1Routine;
    private Coroutine ring2Routine;
    private Coroutine ring3Routine;


    private void Awake()
    {
       // SetInactive();
        //    InitializeVisuals();
    }

    /*
    private void InitializeVisuals()
    {
        //st visuals from questiontype
    }*/
    private void Start()
    {
        VisualsManager.Instance.AreaBoards.Add(this);

    }

    public float fadeDuration = 1f; // Duration of the fade



    public void SetActive()
    {

        if(isActive) return;
        SetObjectById();

        IsActive = true;
        StartSmoothMove(ref ring1Routine, Ring1, yposActiveRing1);
        StartSmoothMove(ref ring2Routine, Ring2, yposActiveRing2);
        StartSmoothMove(ref ring3Routine, Ring3, yposActiveRing3);
    }

   

    public void SetInactive()
    {
        IsActive= false;
        StartSmoothMove(ref ring1Routine, Ring1, yposInactiveRing1);
        StartSmoothMove(ref ring2Routine, Ring2, yposInactiveRing2);
        StartSmoothMove(ref ring3Routine, Ring3, yposInactiveRing3);
    }

    private void StartSmoothMove(ref Coroutine routine, GameObject ring, float targetY)
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(MoveY(ring, targetY, animationDuration));
    }

    private IEnumerator MoveY(GameObject obj, float targetY, float duration)
    {
        Vector3 startPos = obj.transform.localPosition;
        Vector3 endPos = new Vector3(startPos.x, targetY, startPos.z);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = easeCurve.Evaluate(t);
            obj.transform.localPosition = Vector3.Lerp(startPos, endPos, easedT);
            yield return null;
        }

        obj.transform.localPosition = endPos;
    }

    private void SetRingY(GameObject ring, float yPos)
    {
        Vector3 pos = ring.transform.localPosition;
        pos.y = yPos;
        ring.transform.localPosition = pos;
    }

}
