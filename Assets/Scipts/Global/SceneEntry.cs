using UnityEngine;

public class SceneEntry : MonoBehaviour
{
    public bool pause = true;
    private GameObject parentAbil;
    void Start()
    {
        Time.timeScale = 0;
        parentAbil = GameObject.Find("ParentAbilities");
        parentAbil.transform.parent = transform.parent;
    }

    void Update()
    {
        if (!pause)
        {

        }
    }

    public void DestroyAnim()
    {
        Time.timeScale = 1;
        Destroy(transform.parent);
    }
}
