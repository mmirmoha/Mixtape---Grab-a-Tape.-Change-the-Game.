using System.Collections.Generic;
using UnityEngine;

public enum EntityKind { Hazard, Coin, Cassette, Ground, Decor, Note }

// Every scrolling world object. No physics — manual AABB via WorldRect.
public class Entity : MonoBehaviour
{
    public static readonly List<Entity> All = new List<Entity>();

    public EntityKind kind;
    public Vector2 halfSize = new Vector2(0.5f, 0.5f);
    public GameMode swapTo; // only meaningful for cassettes
    public bool consumed;   // rhythm notes: set once judged so they aren't scored twice

    public Rect WorldRect => new Rect((Vector2)transform.position - halfSize, halfSize * 2f);

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    // SetActive(false) first so OnDisable removes it from All immediately
    // (Destroy alone is deferred to end of frame → double-collect bugs).
    public void Kill()
    {
        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    public static Entity Spawn(string name, Sprite sprite, Vector3 pos, Vector2 scale, Color color, EntityKind kind, Vector2 halfSize, int order)
    {
        var go = new GameObject(name);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(scale.x, scale.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = order;
        var e = go.AddComponent<Entity>();
        e.kind = kind;
        e.halfSize = halfSize;
        return e;
    }
}
