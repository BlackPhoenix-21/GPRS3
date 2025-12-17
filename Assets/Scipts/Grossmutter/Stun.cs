using UnityEngine;

public class Stun : MonoBehaviour
{
    public Vector2 collison = new Vector2(10, 15);
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Grossmutter"))
        {
            GameObject game = col.gameObject;
            if (game.GetComponent<Grossmutter>().state != Grossmutter.GrossmutterState.Charge)
                return;
            print("test");
            game.GetComponent<Grossmutter>().Stunned();
            Rigidbody2D rb = game.GetComponent<Rigidbody2D>();
            rb.AddForce(-collison, ForceMode2D.Impulse);
        }
    }
}
