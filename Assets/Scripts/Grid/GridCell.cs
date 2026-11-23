using UnityEngine;
using TowerDefense.Data;
using TowerDefense.Towers;

namespace TowerDefense.Grid
{
    public class GridCell : MonoBehaviour
    {
        [SerializeField] private int gridX;
        [SerializeField] private int gridY;
        [SerializeField] private ZoneType zone;
        [SerializeField] private SpriteRenderer tileRenderer;
        [SerializeField] private SpriteRenderer highlightRenderer;

        public int GridX => gridX;
        public int GridY => gridY;
        public ZoneType Zone => zone;
        public bool IsOccupied => PlacedTower != null;
        public Tower PlacedTower { get; private set; }

        private void Awake()
        {
            if (tileRenderer == null) tileRenderer = GetComponent<SpriteRenderer>();
            if (highlightRenderer == null)
            {
                Transform hl = transform.Find("Highlight");
                if (hl != null) highlightRenderer = hl.GetComponent<SpriteRenderer>();
            }
        }

        public void SetupInEditor(int x, int y, ZoneType zoneType, SpriteRenderer tileRend, SpriteRenderer hlRend)
        {
            gridX = x;
            gridY = y;
            zone = zoneType;
            tileRenderer = tileRend;
            highlightRenderer = hlRend;
        }

        public void Initialize(int x, int y, ZoneType zoneType, Sprite tileSprite)
        {
            gridX = x;
            gridY = y;
            zone = zoneType;

            if (tileRenderer == null)
            {
                tileRenderer = GetComponent<SpriteRenderer>();
                if (tileRenderer == null) tileRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
            tileRenderer.sprite = tileSprite;
            tileRenderer.sortingOrder = 0;

            if (highlightRenderer == null)
            {
                Transform hl = transform.Find("Highlight");
                if (hl == null)
                {
                    GameObject highlightObj = new GameObject("Highlight");
                    highlightObj.transform.SetParent(transform);
                    highlightObj.transform.localPosition = Vector3.zero;
                    highlightObj.transform.localScale = Vector3.one;
                    highlightRenderer = highlightObj.AddComponent<SpriteRenderer>();
                }
                else
                {
                    highlightRenderer = hl.GetComponent<SpriteRenderer>();
                }
            }
            highlightRenderer.sprite = tileSprite;
            highlightRenderer.sortingOrder = 1;
            highlightRenderer.color = Color.clear;
        }

        public bool CanBuild()
        {
            return zone == ZoneType.BuildableB && !IsOccupied;
        }

        public void SetTower(Tower tower)
        {
            PlacedTower = tower;
        }

        public void ClearTower()
        {
            PlacedTower = null;
        }

        public void SetHighlight(bool active, Color color)
        {
            if (highlightRenderer != null)
            {
                highlightRenderer.color = active ? color : Color.clear;
            }
        }

        public void ResetHighlight()
        {
            if (highlightRenderer != null)
            {
                highlightRenderer.color = Color.clear;
            }
        }
    }
}
