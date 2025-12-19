using UnityEngine;

public class SceneEntry : MonoBehaviour
{
    private GameObject parentAbil;
    void Start()
    {
        Time.timeScale = 0;
        GetComponent<Animator>().speed = 0f;
        parentAbil = GameObject.Find("ParentAbilities");
        parentAbil.transform.parent = GameObject.Find("parent").transform;
        parentAbil.transform.SetAsFirstSibling();
    }

    public void Intro(GameObject obj)
    {
        Destroy(obj);
        Time.timeScale = 1;
        GetComponent<Animator>().speed = 1f;
        GameObject.FindWithTag("Player").GetComponent<PlayerAbilities>().enabled = false;
    }

    public void DestroyAnim()
    {
        Destroy(gameObject);
    }
}
