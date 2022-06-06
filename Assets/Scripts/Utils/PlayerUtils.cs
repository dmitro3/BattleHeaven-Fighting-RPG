using UnityEngine;

public class PlayerUtils
{
    public static int maxEnergy = 5;
    public static Fighter FindInactiveFighter()
    {
        return FindInactiveFighterGameObject().GetComponent<Fighter>();
    }

    public static GameObject FindInactiveFighterGameObject() 
    {
        return GameObject.Find("FighterWrapper").transform.Find("Fighter").gameObject;
    }
}
