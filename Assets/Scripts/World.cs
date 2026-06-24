using UnityEngine;

// The scroller: moves every entity left at the current run speed and culls off-screen ones.
public class World : MonoBehaviour
{
    public static World Instance { get; private set; }

    void Awake() => Instance = this;

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null || !gm.WorldScrolling) return;
        float dx = gm.Speed * Time.deltaTime;
        var all = Entity.All;
        for (int i = all.Count - 1; i >= 0; i--)
        {
            var t = all[i].transform;
            t.position += Vector3.left * dx;
            if (t.position.x < -16f) all[i].Kill();
        }
    }

    public Entity Overlap(Rect r, EntityKind kind)
    {
        var all = Entity.All;
        for (int i = 0; i < all.Count; i++)
        {
            var e = all[i];
            if (e.kind == kind && e.WorldRect.Overlaps(r)) return e;
        }
        return null;
    }

    // Highest ground top at or below maxTop within the x range (for landing checks).
    public float? GroundTopUnder(float xMin, float xMax, float maxTop)
    {
        float best = float.NegativeInfinity;
        bool found = false;
        var all = Entity.All;
        for (int i = 0; i < all.Count; i++)
        {
            var e = all[i];
            if (e.kind != EntityKind.Ground) continue;
            var r = e.WorldRect;
            if (r.xMax < xMin || r.xMin > xMax) continue;
            if (r.yMax <= maxTop && r.yMax > best)
            {
                best = r.yMax;
                found = true;
            }
        }
        return found ? best : (float?)null;
    }

    public void Clear(params EntityKind[] kinds)
    {
        var all = Entity.All;
        for (int i = all.Count - 1; i >= 0; i--)
            foreach (var k in kinds)
                if (all[i].kind == k)
                {
                    all[i].Kill();
                    break;
                }
    }

    public void ClearAll()
    {
        var all = Entity.All;
        for (int i = all.Count - 1; i >= 0; i--) all[i].Kill();
    }
}
