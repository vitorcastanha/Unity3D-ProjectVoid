using UnityEngine;
using System.Collections;

public class UnitPath : MonoBehaviour
{

    [SerializeField] private Transform[] pathnode;

    public Transform GetClosest(Vector3 position)
    {
        if (pathnode.Length == 0)
        {
            Debug.LogError("There is no pathnodes registered.");
            return null;
        }
        if (pathnode.Length == 1)
        {
            return pathnode[0];
        }

        Transform closest = null;
        float distance = 0f;

        foreach (Transform pItem in pathnode)
        {
            float d = Vector3.Distance(position, pItem.position);

            if (closest == null || d < distance)
            {
                closest = pItem;
                distance = d;
            }
        }

        return closest;
    }

    public Transform GetNext(Transform current)
    {
        if (pathnode.Length == 0)
        {
            Debug.LogError("There is no pathnodes registered.");
            return null;
        }
        if (pathnode.Length == 1)
        {
            return pathnode[0];
        }

        int index = System.Array.IndexOf(pathnode, current);
        if (index == -1)
        {
            return pathnode[0];
        }

        if (index >= pathnode.Length - 1)
        {
            index = 0;
        }
        else
        {
            index++;
        }

        return pathnode[index];
    }
}
