using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Unit))]
public class UnitScript : MonoBehaviour {

    protected Unit unit;

    //Makes sure it gets the referrence for all controllers before they Start()
    private void Awake()
    {
        unit = GetComponent<Unit>();
    }
        
}
